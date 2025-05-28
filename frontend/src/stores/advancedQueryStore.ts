import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { subscribeWithSelector } from 'zustand/middleware';
import { immer } from 'zustand/middleware/immer';
import { ApiService } from '../services/api';

interface QueryResult {
  id: string;
  sql: string;
  data: any[];
  columns: string[];
  executionTime: number;
  rowCount: number;
  timestamp: number;
  cached: boolean;
}

interface QueryHistory {
  id: string;
  question: string;
  sql: string;
  successful: boolean;
  executionTimeMs: number;
  timestamp: number;
  resultId?: string;
  error?: string;
}

interface QuerySuggestion {
  id: string;
  text: string;
  category: string;
  confidence: number;
  usage: number;
}

interface QueryCache {
  [key: string]: {
    result: QueryResult;
    expiry: number;
  };
}

interface AdvancedQueryState {
  // Current query state
  currentQuery: string;
  currentSql: string;
  isExecuting: boolean;
  lastResult: QueryResult | null;
  lastError: string | null;

  // History and cache
  history: QueryHistory[];
  cache: QueryCache;
  suggestions: QuerySuggestion[];

  // Settings
  settings: {
    cacheEnabled: boolean;
    cacheExpiryMs: number;
    maxHistoryItems: number;
    autoSuggestEnabled: boolean;
    sqlValidationEnabled: boolean;
  };

  // Actions
  setCurrentQuery: (query: string) => void;
  setCurrentSql: (sql: string) => void;
  executeQuery: (query: string) => Promise<QueryResult | null>;
  executeSql: (sql: string) => Promise<QueryResult | null>;
  clearHistory: () => void;
  clearCache: () => void;
  updateSettings: (settings: Partial<AdvancedQueryState['settings']>) => void;
  getSuggestions: (partial: string) => Promise<QuerySuggestion[]>;
  addToHistory: (query: QueryHistory) => void;
  getFromCache: (key: string) => QueryResult | null;
  addToCache: (key: string, result: QueryResult) => void;

  // Selectors
  getRecentQueries: (limit: number) => QueryHistory[];
  getSuccessfulQueries: () => QueryHistory[];
  getFailedQueries: () => QueryHistory[];
  getCacheStats: () => { size: number; hitRate: number };
}

export const useAdvancedQueryStore = create<AdvancedQueryState>()(
  subscribeWithSelector(
    persist(
      immer((set, get) => ({
        // Initial state
        currentQuery: '',
        currentSql: '',
        isExecuting: false,
        lastResult: null,
        lastError: null,
        history: [],
        cache: {},
        suggestions: [],
        settings: {
          cacheEnabled: true,
          cacheExpiryMs: 5 * 60 * 1000, // 5 minutes
          maxHistoryItems: 100,
          autoSuggestEnabled: true,
          sqlValidationEnabled: true
        },

        // Actions
        setCurrentQuery: (query) => set((state) => {
          state.currentQuery = query;
        }),

        setCurrentSql: (sql) => set((state) => {
          state.currentSql = sql;
        }),

        executeQuery: async (query: string) => {
          const state = get();

          set((draft) => {
            draft.isExecuting = true;
            draft.lastError = null;
            draft.currentQuery = query;
          });

          try {
            // Check cache first
            const cacheKey = `query:${query}`;
            if (state.settings.cacheEnabled) {
              const cached = state.getFromCache(cacheKey);
              if (cached) {
                set((draft) => {
                  draft.isExecuting = false;
                  draft.lastResult = { ...cached, cached: true };
                });
                return cached;
              }
            }

            const response = await fetch(`${process.env.REACT_APP_API_URL || 'https://localhost:55243'}/api/query/natural-language`, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('authToken') || ''}`
              },
              body: JSON.stringify({ question: query })
            });

            const data = await response.json();

            if (data.success) {
              const result: QueryResult = {
                id: `result-${Date.now()}`,
                sql: data.sql,
                data: data.results,
                columns: data.results.length > 0 ? Object.keys(data.results[0]) : [],
                executionTime: data.executionTime,
                rowCount: data.rowCount,
                timestamp: Date.now(),
                cached: false
              };

              // Add to cache
              if (state.settings.cacheEnabled) {
                state.addToCache(cacheKey, result);
              }

              // Add to history
              state.addToHistory({
                id: `history-${Date.now()}`,
                question: query,
                sql: response.sql,
                successful: true,
                executionTimeMs: response.executionTime,
                timestamp: Date.now(),
                resultId: result.id
              });

              set((draft) => {
                draft.isExecuting = false;
                draft.lastResult = result;
                draft.currentSql = response.sql;
              });

              return result;
            } else {
              throw new Error('Query execution failed');
            }
          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';

            // Add failed query to history
            state.addToHistory({
              id: `history-${Date.now()}`,
              question: query,
              sql: '',
              successful: false,
              executionTimeMs: 0,
              timestamp: Date.now(),
              error: errorMessage
            });

            set((draft) => {
              draft.isExecuting = false;
              draft.lastError = errorMessage;
            });

            return null;
          }
        },

        executeSql: async (sql: string) => {
          set((draft) => {
            draft.isExecuting = true;
            draft.lastError = null;
            draft.currentSql = sql;
          });

          try {
            const response = await fetch(`${process.env.REACT_APP_API_URL || 'https://localhost:55243'}/api/query/execute-sql`, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('authToken') || ''}`
              },
              body: JSON.stringify({ sql })
            });

            const data = await response.json();

            if (data.success) {
              const result: QueryResult = {
                id: `result-${Date.now()}`,
                sql,
                data: data.results,
                columns: data.results.length > 0 ? Object.keys(data.results[0]) : [],
                executionTime: data.executionTime,
                rowCount: data.rowCount,
                timestamp: Date.now(),
                cached: false
              };

              set((draft) => {
                draft.isExecuting = false;
                draft.lastResult = result;
              });

              return result;
            } else {
              throw new Error('SQL execution failed');
            }
          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';

            set((draft) => {
              draft.isExecuting = false;
              draft.lastError = errorMessage;
            });

            return null;
          }
        },

        clearHistory: () => set((state) => {
          state.history = [];
        }),

        clearCache: () => set((state) => {
          state.cache = {};
        }),

        updateSettings: (newSettings) => set((state) => {
          Object.assign(state.settings, newSettings);
        }),

        getSuggestions: async (partial: string) => {
          try {
            const response = await ApiService.get<{
              success: boolean;
              suggestions: string[];
            }>(`/api/query/suggestions?q=${encodeURIComponent(partial)}`);

            if (response.success) {
              const suggestions: QuerySuggestion[] = response.suggestions.map((text, index) => ({
                id: `suggestion-${index}`,
                text,
                category: 'general',
                confidence: 0.8,
                usage: 0
              }));

              set((draft) => {
                draft.suggestions = suggestions;
              });

              return suggestions;
            }
          } catch (error) {
            console.error('Failed to get suggestions:', error);
          }

          return [];
        },

        addToHistory: (query) => set((state) => {
          state.history.unshift(query);
          if (state.history.length > state.settings.maxHistoryItems) {
            state.history = state.history.slice(0, state.settings.maxHistoryItems);
          }
        }),

        getFromCache: (key: string) => {
          const cached = get().cache[key];
          if (cached && cached.expiry > Date.now()) {
            return cached.result;
          }
          return null;
        },

        addToCache: (key: string, result: QueryResult) => set((state) => {
          state.cache[key] = {
            result,
            expiry: Date.now() + state.settings.cacheExpiryMs
          };
        }),

        // Selectors
        getRecentQueries: (limit: number) => {
          return get().history.slice(0, limit);
        },

        getSuccessfulQueries: () => {
          return get().history.filter(q => q.successful);
        },

        getFailedQueries: () => {
          return get().history.filter(q => !q.successful);
        },

        getCacheStats: () => {
          const cache = get().cache;
          const history = get().history;
          const cacheSize = Object.keys(cache).length;
          const totalQueries = history.length;
          const cachedQueries = history.filter(q => q.successful).length;
          const hitRate = totalQueries > 0 ? (cachedQueries / totalQueries) * 100 : 0;

          return { size: cacheSize, hitRate };
        }
      })),
      {
        name: 'advanced-query-storage',
        storage: createJSONStorage(() => localStorage),
        partialize: (state) => ({
          history: state.history,
          settings: state.settings,
          suggestions: state.suggestions
        })
      }
    )
  )
);

// Selector hooks for better performance
export const useAdvancedCurrentQuery = () => useAdvancedQueryStore((state) => state.currentQuery);
export const useAdvancedCurrentSql = () => useAdvancedQueryStore((state) => state.currentSql);
export const useAdvancedIsExecuting = () => useAdvancedQueryStore((state) => state.isExecuting);
export const useAdvancedLastResult = () => useAdvancedQueryStore((state) => state.lastResult);
export const useAdvancedLastError = () => useAdvancedQueryStore((state) => state.lastError);
export const useAdvancedQueryHistory = () => useAdvancedQueryStore((state) => state.history);
export const useAdvancedQuerySettings = () => useAdvancedQueryStore((state) => state.settings);
