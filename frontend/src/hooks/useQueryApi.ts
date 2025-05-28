import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys, optimisticUpdates, errorHandlers } from '../lib/react-query';
import { ApiService } from '../services/api';
import type { QueryRequest, QueryResponse } from '../types/query';

// Hook for executing queries with React Query
export const useExecuteQuery = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: QueryRequest): Promise<QueryResponse> => {
      return ApiService.executeQuery(request);
    },

    onMutate: async (request) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: queryKeys.queries.all });

      // Optimistically add to history
      const optimisticQuery = {
        id: `temp-${Date.now()}`,
        question: request.question,
        timestamp: new Date().toISOString(),
        status: 'executing' as const,
      };

      optimisticUpdates.addToHistory(optimisticQuery);

      return { optimisticQuery };
    },

    onSuccess: (data, variables, context) => {
      // Update the query cache with the result
      queryClient.setQueryData(
        queryKeys.queries.detail(data.queryId || 'latest'),
        data
      );

      // Update history with actual result
      queryClient.setQueryData(
        queryKeys.queries.history(),
        (old: any) => {
          if (!old) return [data];

          // Replace optimistic entry with real data
          return old.map((item: any) =>
            item.id === context?.optimisticQuery.id
              ? { ...data, timestamp: new Date().toISOString() }
              : item
          );
        }
      );

      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: queryKeys.queries.suggestions() });
    },

    onError: (error, variables, context) => {
      // Remove optimistic update on error
      if (context?.optimisticQuery) {
        queryClient.setQueryData(
          queryKeys.queries.history(),
          (old: any) => old?.filter((item: any) => item.id !== context.optimisticQuery.id) || []
        );
      }

      errorHandlers.defaultMutationError(error);
    },

    // Retry configuration
    retry: (failureCount, error: any) => {
      // Don't retry on client errors
      if (error?.response?.status >= 400 && error?.response?.status < 500) {
        return false;
      }
      return failureCount < 2; // Retry up to 2 times for queries
    },

    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 10000),
  });
};

// Hook for fetching query history with pagination
export const useQueryHistory = (page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: queryKeys.queries.history(page, pageSize),
    queryFn: () => ApiService.getQueryHistory(page, pageSize),

    // Keep previous data while fetching new page
    placeholderData: (previousData) => previousData,

    // Stale time for history (can be longer since it doesn't change often)
    staleTime: 10 * 60 * 1000, // 10 minutes

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for fetching query suggestions with debouncing
export const useQuerySuggestions = (context?: string, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.queries.suggestions(context),
    queryFn: () => ApiService.getQuerySuggestions(context),

    // Only fetch when enabled and context is provided
    enabled: enabled && (context === undefined || context.length > 2),

    // Shorter stale time for suggestions (they should be fresh)
    staleTime: 2 * 60 * 1000, // 2 minutes

    // Longer cache time since suggestions don't change often
    gcTime: 30 * 60 * 1000, // 30 minutes

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for fetching schema information
export const useSchema = () => {
  return useQuery({
    queryKey: queryKeys.schema.tables(),
    queryFn: () => ApiService.getSchema(),

    // Schema changes rarely, so longer stale time
    staleTime: 60 * 60 * 1000, // 1 hour
    gcTime: 2 * 60 * 60 * 1000, // 2 hours

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for fetching table details
export const useTableDetails = (tableName: string, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.schema.table(tableName),
    queryFn: () => ApiService.getTableDetails(tableName),

    enabled: enabled && !!tableName,

    // Table details change rarely
    staleTime: 30 * 60 * 1000, // 30 minutes
    gcTime: 60 * 60 * 1000, // 1 hour

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for health checks
export const useHealthCheck = (checkType: 'database' | 'api' | 'cache') => {
  return useQuery({
    queryKey: queryKeys.health[checkType](),
    queryFn: () => ApiService.healthCheck(checkType),

    // Health checks should be frequent but not too aggressive
    staleTime: 30 * 1000, // 30 seconds
    gcTime: 2 * 60 * 1000, // 2 minutes

    // Refetch on interval for real-time health monitoring
    refetchInterval: 60 * 1000, // 1 minute

    // Don't refetch on window focus for health checks
    refetchOnWindowFocus: false,

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for adding queries to favorites
export const useAddToFavorites = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (query: { query: string; timestamp: string; description: string }) => {
      return ApiService.addToFavorites(query);
    },

    onSuccess: () => {
      // Invalidate favorites list
      queryClient.invalidateQueries({ queryKey: ['favorites'] });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for clearing cache
export const useClearCache = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (pattern?: string) => {
      return ApiService.clearCache(pattern);
    },

    onSuccess: (data, pattern) => {
      if (pattern) {
        // Invalidate specific pattern
        queryClient.invalidateQueries({
          predicate: (query) => query.queryKey.some(key =>
            typeof key === 'string' && key.includes(pattern)
          )
        });
      } else {
        // Clear all cache
        queryClient.clear();
      }
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for getting cache metrics
export const useCacheMetrics = () => {
  return useQuery({
    queryKey: ['cache', 'metrics'],
    queryFn: () => ApiService.getCacheMetrics(),

    // Update cache metrics frequently
    staleTime: 10 * 1000, // 10 seconds
    refetchInterval: 30 * 1000, // 30 seconds

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for prefetching related data
export const usePrefetchRelatedData = () => {
  const queryClient = useQueryClient();

  const prefetchHistory = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.queries.history(1, 10),
      queryFn: () => ApiService.getQueryHistory(1, 10),
    });
  };

  const prefetchSuggestions = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.queries.suggestions(),
      queryFn: () => ApiService.getQuerySuggestions(),
    });
  };

  const prefetchSchema = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.schema.tables(),
      queryFn: () => ApiService.getSchema(),
    });
  };

  return {
    prefetchHistory,
    prefetchSuggestions,
    prefetchSchema,
  };
};

// Hook for background data synchronization
export const useBackgroundSync = () => {
  const queryClient = useQueryClient();

  const syncAll = () => {
    // Refetch active queries in the background
    queryClient.refetchQueries({
      type: 'active',
      stale: true
    });
  };

  const syncHistory = () => {
    queryClient.refetchQueries({
      queryKey: queryKeys.queries.history(),
      type: 'active'
    });
  };

  const syncHealth = () => {
    queryClient.refetchQueries({
      queryKey: queryKeys.health.all,
      type: 'active'
    });
  };

  return {
    syncAll,
    syncHistory,
    syncHealth,
  };
};
