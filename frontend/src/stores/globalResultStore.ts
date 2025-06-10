/**
 * Global Result Store - Enhanced result management for cross-page availability
 * Extends activeResultStore with global sharing, history, and cross-page synchronization
 */

import { create } from 'zustand';
import { persist, subscribeWithSelector } from 'zustand/middleware';
import { immer } from 'zustand/middleware/immer';
import type { QueryResponse } from '../types/query';

// Enhanced result with metadata for global sharing
export interface GlobalResult {
  id: string;
  result: QueryResponse;
  query: string;
  timestamp: number;
  source: 'query' | 'visualization' | 'dashboard' | 'export' | 'api';
  tags: string[];
  metadata: {
    userId?: string;
    sessionId?: string;
    pageSource?: string;
    visualizationConfig?: any;
    dashboardId?: string;
  };
}

// Cross-page sharing context
export interface CrossPageContext {
  sourcePageId: string;
  targetPageId: string;
  sharedAt: number;
  permissions: string[];
}

interface GlobalResultState {
  // Current active result (enhanced)
  currentResult: GlobalResult | null;
  
  // Result history for navigation and reuse
  resultHistory: GlobalResult[];
  maxHistorySize: number;
  
  // Cross-page sharing
  sharedResults: Record<string, GlobalResult>;
  crossPageContexts: Record<string, CrossPageContext>;
  
  // Active contexts
  activeVisualizationId: string | null;
  activeDashboardId: string | null;
  activePageId: string;
  
  // Preferences
  autoShareResults: boolean;
  persistHistory: boolean;
  maxSharedResults: number;

  // Actions - Result Management
  setCurrentResult: (result: QueryResponse, query: string, source?: string, metadata?: any) => void;
  clearCurrentResult: () => void;
  
  // Actions - History Management
  addToHistory: (globalResult: GlobalResult) => void;
  removeFromHistory: (id: string) => void;
  clearHistory: () => void;
  getHistoryBySource: (source: string) => GlobalResult[];
  
  // Actions - Cross-page Sharing
  shareResult: (resultId: string, targetPageId: string, permissions?: string[]) => void;
  unshareResult: (resultId: string, targetPageId: string) => void;
  getSharedResults: (pageId: string) => GlobalResult[];
  
  // Actions - Context Management
  setActiveVisualization: (id: string | null) => void;
  setActiveDashboard: (id: string | null) => void;
  setActivePageId: (pageId: string) => void;
  
  // Actions - Preferences
  updatePreferences: (preferences: Partial<Pick<GlobalResultState, 'autoShareResults' | 'persistHistory' | 'maxSharedResults'>>) => void;
  
  // Selectors
  hasCurrentResult: () => boolean;
  getCurrentResultForPage: (pageId: string) => GlobalResult | null;
  getRecentResults: (limit?: number) => GlobalResult[];
  getResultsByTag: (tag: string) => GlobalResult[];
  searchResults: (query: string) => GlobalResult[];
}

const DEFAULT_MAX_HISTORY = 50;
const DEFAULT_MAX_SHARED = 20;

export const useGlobalResultStore = create<GlobalResultState>()(
  subscribeWithSelector(
    persist(
      immer((set, get) => ({
        // Initial state
        currentResult: null,
        resultHistory: [],
        maxHistorySize: DEFAULT_MAX_HISTORY,
        sharedResults: {},
        crossPageContexts: {},
        activeVisualizationId: null,
        activeDashboardId: null,
        activePageId: 'main',
        autoShareResults: true,
        persistHistory: true,
        maxSharedResults: DEFAULT_MAX_SHARED,

        // Set current result with enhanced metadata
        setCurrentResult: (result: QueryResponse, query: string, source = 'query', metadata = {}) => {
          const globalResult: GlobalResult = {
            id: `result-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
            result,
            query,
            timestamp: Date.now(),
            source: source as any,
            tags: [],
            metadata: {
              sessionId: result.sessionId,
              pageSource: get().activePageId,
              ...metadata
            }
          };

          set((state) => {
            state.currentResult = globalResult;
            
            // Add to history if enabled
            if (state.persistHistory) {
              state.resultHistory.unshift(globalResult);
              // Trim history to max size
              if (state.resultHistory.length > state.maxHistorySize) {
                state.resultHistory = state.resultHistory.slice(0, state.maxHistorySize);
              }
            }
            
            // Auto-share if enabled
            if (state.autoShareResults) {
              state.sharedResults[globalResult.id] = globalResult;
              // Trim shared results
              const sharedKeys = Object.keys(state.sharedResults);
              if (sharedKeys.length > state.maxSharedResults) {
                const entries = Object.entries(state.sharedResults);
                const oldestKey = entries.sort((a, b) => a[1].timestamp - b[1].timestamp)[0][0];
                delete state.sharedResults[oldestKey];
              }
            }
          });

          if (process.env.NODE_ENV === 'development') {
            console.log('🌐 GlobalResultStore - Set current result:', {
              id: globalResult.id,
              source,
              query: query.substring(0, 50) + '...',
              hasData: !!result.result?.data,
              dataLength: result.result?.data?.length,
              pageSource: globalResult.metadata.pageSource
            });
          }
        },

        // Clear current result
        clearCurrentResult: () => {
          set((state) => {
            state.currentResult = null;
          });
        },

        // Add to history manually
        addToHistory: (globalResult: GlobalResult) => {
          set((state) => {
            const existingIndex = state.resultHistory.findIndex(r => r.id === globalResult.id);
            if (existingIndex >= 0) {
              state.resultHistory[existingIndex] = globalResult;
            } else {
              state.resultHistory.unshift(globalResult);
              if (state.resultHistory.length > state.maxHistorySize) {
                state.resultHistory = state.resultHistory.slice(0, state.maxHistorySize);
              }
            }
          });
        },

        // Remove from history
        removeFromHistory: (id: string) => {
          set((state) => {
            state.resultHistory = state.resultHistory.filter(r => r.id !== id);
          });
        },

        // Clear history
        clearHistory: () => {
          set((state) => {
            state.resultHistory = [];
          });
        },

        // Get history by source
        getHistoryBySource: (source: string) => {
          return get().resultHistory.filter(r => r.source === source);
        },

        // Share result across pages
        shareResult: (resultId: string, targetPageId: string, permissions = ['read']) => {
          set((state) => {
            const result = state.resultHistory.find(r => r.id === resultId) || state.currentResult;
            if (result && result.id === resultId) {
              state.sharedResults[resultId] = result;
              state.crossPageContexts[`${resultId}-${targetPageId}`] = {
                sourcePageId: state.activePageId,
                targetPageId,
                sharedAt: Date.now(),
                permissions
              };
            }
          });
        },

        // Unshare result
        unshareResult: (resultId: string, targetPageId: string) => {
          set((state) => {
            delete state.crossPageContexts[`${resultId}-${targetPageId}`];
          });
        },

        // Get shared results for a page
        getSharedResults: (pageId: string) => {
          const state = get();
          const results: GlobalResult[] = [];

          Object.entries(state.crossPageContexts).forEach(([key, context]) => {
            if (context.targetPageId === pageId) {
              const resultId = key.split('-')[0];
              const result = state.sharedResults[resultId];
              if (result) {
                results.push(result);
              }
            }
          });

          return results.sort((a, b) => b.timestamp - a.timestamp);
        },

        // Set active visualization
        setActiveVisualization: (id: string | null) => {
          set((state) => {
            state.activeVisualizationId = id;
          });
        },

        // Set active dashboard
        setActiveDashboard: (id: string | null) => {
          set((state) => {
            state.activeDashboardId = id;
          });
        },

        // Set active page
        setActivePageId: (pageId: string) => {
          set((state) => {
            state.activePageId = pageId;
          });
        },

        // Update preferences
        updatePreferences: (preferences) => {
          set((state) => {
            Object.assign(state, preferences);
          });
        },

        // Check if has current result
        hasCurrentResult: () => {
          return get().currentResult !== null;
        },

        // Get current result for specific page
        getCurrentResultForPage: (pageId: string) => {
          const state = get();
          
          // First check if current result is for this page
          if (state.currentResult && state.currentResult.metadata.pageSource === pageId) {
            return state.currentResult;
          }
          
          // Then check shared results
          const sharedResults = state.getSharedResults(pageId);
          return sharedResults[0] || null;
        },

        // Get recent results
        getRecentResults: (limit = 10) => {
          return get().resultHistory.slice(0, limit);
        },

        // Get results by tag
        getResultsByTag: (tag: string) => {
          return get().resultHistory.filter(r => r.tags.includes(tag));
        },

        // Search results
        searchResults: (query: string) => {
          const searchTerm = query.toLowerCase();
          return get().resultHistory.filter(r => 
            r.query.toLowerCase().includes(searchTerm) ||
            r.tags.some(tag => tag.toLowerCase().includes(searchTerm))
          );
        }
      })),
      {
        name: 'global-result-store',
        version: 1,
        partialize: (state) => ({
          currentResult: state.currentResult,
          resultHistory: state.persistHistory ? state.resultHistory : [],
          sharedResults: state.sharedResults,
          crossPageContexts: state.crossPageContexts,
          activeVisualizationId: state.activeVisualizationId,
          activeDashboardId: state.activeDashboardId,
          activePageId: state.activePageId,
          autoShareResults: state.autoShareResults,
          persistHistory: state.persistHistory,
          maxSharedResults: state.maxSharedResults
        }),
        // No need for custom serialization with objects
        // serialize and deserialize are handled automatically
      }
    )
  )
);

// Convenient hooks for common use cases
export const useGlobalResult = () => {
  const store = useGlobalResultStore();
  return {
    currentResult: store.currentResult,
    hasResult: store.hasCurrentResult(),
    setResult: store.setCurrentResult,
    clearResult: store.clearCurrentResult
  };
};

export const useGlobalResultActions = () => {
  const store = useGlobalResultStore();
  return {
    setCurrentResult: store.setCurrentResult,
    clearCurrentResult: store.clearCurrentResult,
    shareResult: store.shareResult,
    unshareResult: store.unshareResult,
    setActiveVisualization: store.setActiveVisualization,
    setActiveDashboard: store.setActiveDashboard,
    setActivePageId: store.setActivePageId
  };
};

export const useGlobalResultHistory = () => {
  const store = useGlobalResultStore();
  return {
    history: store.resultHistory,
    addToHistory: store.addToHistory,
    removeFromHistory: store.removeFromHistory,
    clearHistory: store.clearHistory,
    getRecentResults: store.getRecentResults,
    searchResults: store.searchResults,
    getResultsByTag: store.getResultsByTag
  };
};

export const usePageResults = (pageId: string) => {
  const store = useGlobalResultStore();
  return {
    currentResult: store.getCurrentResultForPage(pageId),
    sharedResults: store.getSharedResults(pageId),
    shareResult: (resultId: string, permissions?: string[]) =>
      store.shareResult(resultId, pageId, permissions),
    unshareResult: (resultId: string) =>
      store.unshareResult(resultId, pageId)
  };
};

export const useVisualizationResults = () => {
  const store = useGlobalResultStore();
  return {
    currentResult: store.currentResult,
    activeVisualizationId: store.activeVisualizationId,
    setActiveVisualization: store.setActiveVisualization,
    visualizationResults: store.getHistoryBySource('visualization')
  };
};

export const useDashboardResults = () => {
  const store = useGlobalResultStore();
  return {
    currentResult: store.currentResult,
    activeDashboardId: store.activeDashboardId,
    setActiveDashboard: store.setActiveDashboard,
    dashboardResults: store.getHistoryBySource('dashboard')
  };
};

// Debug logging for development
if (process.env.NODE_ENV === 'development') {
  useGlobalResultStore.subscribe(
    (state) => state.currentResult,
    (currentResult, previousResult) => {
      if (currentResult !== previousResult) {
        console.log('🌐 Global result changed:', {
          hasResult: !!currentResult,
          resultId: currentResult?.id,
          source: currentResult?.source,
          pageSource: currentResult?.metadata.pageSource,
          query: currentResult?.query.substring(0, 50) + '...',
          historySize: useGlobalResultStore.getState().resultHistory.length
        });
      }
    }
  );
}
