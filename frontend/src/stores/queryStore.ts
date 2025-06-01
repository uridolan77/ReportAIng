import { create } from 'zustand';
import { QueryRequest, QueryResponse, QueryHistoryItem } from '../types/query';
import { API_CONFIG, getApiUrl, getAuthHeaders } from '../config/api';
import { useAuthStore } from './authStore';
import { queryCacheService } from '../services/queryCacheService';
import { validateNaturalLanguageQuery, validateQueryContext } from '../utils/queryValidator';

interface QueryState {
  currentResult: QueryResponse | null;
  queryHistory: QueryHistoryItem[];
  isLoading: boolean;
  error: string | null;
  executeQuery: (request: QueryRequest) => Promise<void>;
  addToFavorites: (query: { query: string; timestamp: string; description: string }) => void;
  clearCurrentResult: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearCache: (pattern?: string) => Promise<void>;
  getCacheMetrics: () => Promise<any>;
}

export const useQueryStore = create<QueryState>((set, get) => ({
  currentResult: null,
  queryHistory: [],
  isLoading: false,
  error: null,

  executeQuery: async (request: QueryRequest) => {
    set({ isLoading: true, error: null });

    try {
      // Validate the query for security
      const validationResult = validateNaturalLanguageQuery(request.question);

      if (!validationResult.isValid) {
        const errorMessage = `Query validation failed: ${validationResult.errors.join(', ')}`;
        set({
          error: errorMessage,
          isLoading: false
        });
        console.error('Query validation failed:', validationResult);
        return;
      }

      // Show warnings if any
      if (validationResult.warnings.length > 0) {
        console.warn('Query validation warnings:', validationResult.warnings);
      }

      // Get user context for additional validation
      const authStateForValidation = useAuthStore.getState();
      const userRoles = authStateForValidation.user?.roles || [];
      const allowedTables = ['tbl_daily_actions', 'tbl_daily_actions_players', 'tbl_countries', 'tbl_currencies', 'tbl_whitelabels'];

      // Additional context-based validation
      const contextValidation = validateQueryContext(request.question, userRoles, allowedTables);
      if (!contextValidation.isValid) {
        const errorMessage = `Access denied: ${contextValidation.errors.join(', ')}`;
        set({
          error: errorMessage,
          isLoading: false
        });
        console.error('Query context validation failed:', contextValidation);
        return;
      }

      // Initialize cache service if not already done
      await queryCacheService.init();

      // Check cache first (only if caching is enabled)
      const isCacheEnabled = localStorage.getItem('cache-enabled') !== 'false';
      if (isCacheEnabled) {
        const cachedResult = await queryCacheService.getCachedResult(request);
        if (cachedResult) {
          console.log('Using cached query result');
          set({
            currentResult: { ...cachedResult, cached: true },
            isLoading: false,
            error: null
          });

          // Add cached result to history
          const historyItem: QueryHistoryItem = {
            id: cachedResult.queryId,
            question: request.question,
            sql: cachedResult.sql,
            timestamp: cachedResult.timestamp,
            successful: cachedResult.success,
            executionTimeMs: cachedResult.executionTimeMs,
            confidence: cachedResult.confidence,
            userId: 'current-user',
            sessionId: request.sessionId,
            error: cachedResult.error,
          };

          set(state => ({
            queryHistory: [historyItem, ...state.queryHistory.slice(0, 49)]
          }));
          return;
        }
      } else {
        console.log('Query caching disabled - skipping cache check and clearing any stale cache');
        // Clear any stale cache entries when caching is disabled
        await queryCacheService.invalidateCache();
      }

      // Get the current auth token (encrypted) and decrypt it
      const encryptedToken = authStateForValidation.token;
      let decryptedToken = null;

      if (encryptedToken) {
        try {
          const { SecurityUtils } = await import('../utils/security');
          decryptedToken = await SecurityUtils.decryptToken(encryptedToken);
        } catch (error) {
          console.warn('Failed to decrypt token in queryStore:', error);
        }
      }

      // Update request to include cache setting
      const requestWithCacheSettings = {
        ...request,
        options: {
          ...request.options,
          enableCache: isCacheEnabled
        }
      };

      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.QUERY.NATURAL_LANGUAGE), {
        method: 'POST',
        headers: await getAuthHeaders(decryptedToken || undefined),
        body: JSON.stringify(requestWithCacheSettings),
      });

      if (response.ok) {
        const result: QueryResponse = await response.json();

        // Cache the result for future use (only if caching is enabled)
        if (isCacheEnabled) {
          await queryCacheService.cacheResult(request, result);
          console.log('Query result cached');
        } else {
          console.log('Query caching disabled - result not cached');
        }

        set({
          currentResult: { ...result, cached: false },
          isLoading: false,
          error: null
        });

        // Add to history
        const historyItem: QueryHistoryItem = {
          id: result.queryId,
          question: request.question,
          sql: result.sql,
          timestamp: result.timestamp,
          successful: result.success,
          executionTimeMs: result.executionTimeMs,
          confidence: result.confidence,
          userId: 'current-user',
          sessionId: request.sessionId,
          error: result.error,
        };

        set(state => ({
          queryHistory: [historyItem, ...state.queryHistory.slice(0, 49)] // Keep last 50
        }));
      } else {
        const errorData = await response.json();
        set({
          error: errorData.error || 'Failed to execute query',
          isLoading: false
        });
      }
    } catch (error) {
      console.error('Query execution error:', error);

      set({
        currentResult: null,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Query execution failed',
      });

      throw error;
    }
  },

  addToFavorites: (query) => {
    // For now, just log it. In a real app, this would save to backend
    console.log('Added to favorites:', query);
  },

  clearCurrentResult: () => {
    set({ currentResult: null, error: null });
  },

  setLoading: (loading: boolean) => {
    set({ isLoading: loading });
  },

  setError: (error: string | null) => {
    set({ error });
  },

  clearCache: async (pattern?: string) => {
    try {
      // Clear IndexedDB cache
      await queryCacheService.invalidateCache(pattern);

      // Clear localStorage cache items
      if (!pattern) {
        // Clear all cache-related localStorage items
        Object.keys(localStorage).forEach(key => {
          if (key.includes('cache') || key.includes('query-result') || key.includes('prompt-cache')) {
            localStorage.removeItem(key);
          }
        });
        console.log('All caches cleared (IndexedDB + localStorage)');
      }

      // Cache cleared successfully
    } catch (error) {
      console.error('Failed to clear cache:', error);
      // Failed to clear cache - error handled by service layer
    }
  },

  getCacheMetrics: async () => {
    try {
      return await queryCacheService.getCacheMetrics();
    } catch (error) {
      // Failed to get cache metrics - error handled by service layer
      return null;
    }
  },
}));
