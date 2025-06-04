/**
 * Active Result Store - Manages the currently active query result with persistence
 * This store ensures that query results persist across navigation and page refreshes
 */

import { create } from 'zustand';
import { persist, subscribeWithSelector } from 'zustand/middleware';
import { immer } from 'zustand/middleware/immer';
import type { QueryResponse } from '../types/query';

interface ActiveResultState {
  // Current active result
  activeResult: QueryResponse | null;
  activeQuery: string;

  // Metadata
  lastUpdated: number;
  sessionId: string;

  // Actions
  setActiveResult: (result: QueryResponse, query: string) => void;
  clearActiveResult: () => void;
  hasActiveResult: () => boolean;
  isResultExpired: (maxAgeMs?: number) => boolean;
  refreshActiveResult: () => void;
}

const DEFAULT_MAX_AGE_MS = 24 * 60 * 60 * 1000; // 24 hours

export const useActiveResultStore = create<ActiveResultState>()(
  subscribeWithSelector(
    persist(
      immer((set, get) => ({
        // Initial state
        activeResult: null,
        activeQuery: '',
        lastUpdated: 0,
        sessionId: '',

        // Set active result with metadata
        setActiveResult: (result: QueryResponse, query: string) => {
          console.log('🔍 ActiveResultStore - Setting active result:', {
            hasResult: !!result,
            success: result?.success,
            query,
            resultKeys: result ? Object.keys(result) : 'N/A',
            hasResultData: !!result?.result,
            hasResultDataArray: !!result?.result?.data,
            resultDataLength: result?.result?.data?.length
          });

          set((state) => {
            state.activeResult = result;
            state.activeQuery = query;
            state.lastUpdated = Date.now();
            state.sessionId = result.sessionId || '';
          });

          // Also store in legacy localStorage for backward compatibility
          try {
            const legacyData = {
              ...result,
              query,
              timestamp: Date.now()
            };
            localStorage.setItem('current-query-result', JSON.stringify(legacyData));
          } catch (error) {
            console.warn('Failed to store result in legacy localStorage:', error);
          }
        },

        // Clear active result
        clearActiveResult: () => {
          set((state) => {
            state.activeResult = null;
            state.activeQuery = '';
            state.lastUpdated = 0;
            state.sessionId = '';
          });

          // Also clear legacy localStorage
          try {
            localStorage.removeItem('current-query-result');
          } catch (error) {
            console.warn('Failed to clear legacy localStorage:', error);
          }
        },

        // Check if there's an active result
        hasActiveResult: () => {
          const state = get();
          return state.activeResult !== null && !state.isResultExpired();
        },

        // Check if result is expired
        isResultExpired: (maxAgeMs = DEFAULT_MAX_AGE_MS) => {
          const state = get();
          if (!state.activeResult || !state.lastUpdated) return true;
          return Date.now() - state.lastUpdated > maxAgeMs;
        },

        // Refresh active result (update timestamp)
        refreshActiveResult: () => {
          set((state) => {
            if (state.activeResult) {
              state.lastUpdated = Date.now();
            }
          });
        }
      })),
      {
        name: 'active-result-store',
        version: 1,
        // Only persist essential data
        partialize: (state) => ({
          activeResult: state.activeResult,
          activeQuery: state.activeQuery,
          lastUpdated: state.lastUpdated,
          sessionId: state.sessionId
        }),
        // Migrate from legacy localStorage on first load
        onRehydrateStorage: () => (state) => {
          if (state && !state.activeResult) {
            try {
              const legacyData = localStorage.getItem('current-query-result');
              if (legacyData) {
                const parsed = JSON.parse(legacyData);
                if (parsed && parsed.success !== undefined) {
                  // This looks like a valid query result
                  state.activeResult = parsed;
                  state.activeQuery = parsed.query || '';
                  state.lastUpdated = parsed.timestamp || Date.now();
                  state.sessionId = parsed.sessionId || '';
                  console.log('Migrated legacy result to active result store');
                }
              }
            } catch (error) {
              console.warn('Failed to migrate legacy result:', error);
            }
          }
        }
      }
    )
  )
);

// Selector hooks for common use cases
export const useActiveResult = () => {
  const store = useActiveResultStore();
  return {
    result: store.activeResult,
    query: store.activeQuery,
    hasResult: store.hasActiveResult(),
    isExpired: store.isResultExpired(),
    lastUpdated: store.lastUpdated
  };
};

export const useActiveResultActions = () => {
  const store = useActiveResultStore();
  return {
    setActiveResult: store.setActiveResult,
    clearActiveResult: store.clearActiveResult,
    refreshActiveResult: store.refreshActiveResult
  };
};

// Subscribe to changes for debugging
if (process.env.NODE_ENV === 'development') {
  useActiveResultStore.subscribe(
    (state) => state.activeResult,
    (activeResult, previousActiveResult) => {
      if (activeResult !== previousActiveResult) {
        console.log('🔄 Active result changed:', {
          hasResult: !!activeResult,
          query: useActiveResultStore.getState().activeQuery,
          timestamp: useActiveResultStore.getState().lastUpdated
        });
      }
    }
  );
}
