import { QueryClient, DefaultOptions } from '@tanstack/react-query';

// Default options for React Query
const queryConfig: DefaultOptions = {
  queries: {
    // Stale time - how long data is considered fresh
    staleTime: 5 * 60 * 1000, // 5 minutes
    
    // Cache time - how long data stays in cache after component unmounts
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
    
    // Retry configuration
    retry: (failureCount, error: any) => {
      // Don't retry on 4xx errors (client errors)
      if (error?.response?.status >= 400 && error?.response?.status < 500) {
        return false;
      }
      // Retry up to 3 times for other errors
      return failureCount < 3;
    },
    
    // Retry delay with exponential backoff
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
    
    // Refetch on window focus (useful for real-time data)
    refetchOnWindowFocus: true,
    
    // Refetch on reconnect
    refetchOnReconnect: true,
    
    // Don't refetch on mount if data is fresh
    refetchOnMount: true,
  },
  mutations: {
    // Retry mutations once on failure
    retry: 1,
    
    // Retry delay for mutations
    retryDelay: 1000,
  },
};

// Create the query client
export const queryClient = new QueryClient({
  defaultOptions: queryConfig,
});

// Query keys factory for consistent key management
export const queryKeys = {
  // Query-related keys
  queries: {
    all: ['queries'] as const,
    lists: () => [...queryKeys.queries.all, 'list'] as const,
    list: (filters: Record<string, any>) => [...queryKeys.queries.lists(), { filters }] as const,
    details: () => [...queryKeys.queries.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.queries.details(), id] as const,
    history: (page?: number, pageSize?: number) => 
      [...queryKeys.queries.all, 'history', { page, pageSize }] as const,
    suggestions: (context?: string) => 
      [...queryKeys.queries.all, 'suggestions', { context }] as const,
    execute: (request: any) => 
      [...queryKeys.queries.all, 'execute', request] as const,
  },
  
  // Schema-related keys
  schema: {
    all: ['schema'] as const,
    tables: () => [...queryKeys.schema.all, 'tables'] as const,
    table: (tableName: string) => [...queryKeys.schema.tables(), tableName] as const,
    columns: (tableName: string) => [...queryKeys.schema.table(tableName), 'columns'] as const,
  },
  
  // Visualization-related keys
  visualizations: {
    all: ['visualizations'] as const,
    recommendations: (data: any) => 
      [...queryKeys.visualizations.all, 'recommendations', data] as const,
    charts: () => [...queryKeys.visualizations.all, 'charts'] as const,
    chart: (id: string) => [...queryKeys.visualizations.charts(), id] as const,
  },
  
  // Tuning-related keys
  tuning: {
    all: ['tuning'] as const,
    dashboard: () => [...queryKeys.tuning.all, 'dashboard'] as const,
    tables: () => [...queryKeys.tuning.all, 'tables'] as const,
    glossary: () => [...queryKeys.tuning.all, 'glossary'] as const,
    patterns: () => [...queryKeys.tuning.all, 'patterns'] as const,
    prompts: (page?: number, pageSize?: number) =>
      [...queryKeys.tuning.all, 'prompts', { page, pageSize }] as const,
    promptTemplates: () => [...queryKeys.tuning.all, 'prompt-templates'] as const,
    promptTemplate: (id: number) => [...queryKeys.tuning.promptTemplates(), id] as const,
  },
  
  // Health and status keys
  health: {
    all: ['health'] as const,
    database: () => [...queryKeys.health.all, 'database'] as const,
    api: () => [...queryKeys.health.all, 'api'] as const,
    cache: () => [...queryKeys.health.all, 'cache'] as const,
  },
  
  // User-related keys
  user: {
    all: ['user'] as const,
    profile: () => [...queryKeys.user.all, 'profile'] as const,
    preferences: () => [...queryKeys.user.all, 'preferences'] as const,
  },
} as const;

// Cache invalidation helpers
export const invalidateQueries = {
  // Invalidate all query-related data
  allQueries: () => queryClient.invalidateQueries({ queryKey: queryKeys.queries.all }),
  
  // Invalidate query history
  queryHistory: () => queryClient.invalidateQueries({ queryKey: queryKeys.queries.history() }),
  
  // Invalidate suggestions
  querySuggestions: () => queryClient.invalidateQueries({ 
    queryKey: queryKeys.queries.suggestions() 
  }),
  
  // Invalidate schema data
  schema: () => queryClient.invalidateQueries({ queryKey: queryKeys.schema.all }),
  
  // Invalidate tuning data
  tuning: () => queryClient.invalidateQueries({ queryKey: queryKeys.tuning.all }),
  
  // Invalidate health checks
  health: () => queryClient.invalidateQueries({ queryKey: queryKeys.health.all }),
};

// Prefetch helpers for better UX
export const prefetchQueries = {
  // Prefetch query history
  queryHistory: async (page = 1, pageSize = 20) => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.queries.history(page, pageSize),
      queryFn: () => import('../services/api').then(api => 
        api.ApiService.getQueryHistory(page, pageSize)
      ),
    });
  },
  
  // Prefetch suggestions
  querySuggestions: async (context?: string) => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.queries.suggestions(context),
      queryFn: () => import('../services/api').then(api => 
        api.ApiService.getQuerySuggestions(context)
      ),
    });
  },
  
  // Prefetch schema
  schema: async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.schema.tables(),
      queryFn: () => import('../services/api').then(api => 
        api.ApiService.getSchema()
      ),
    });
  },
};

// Optimistic update helpers
export const optimisticUpdates = {
  // Add query to history optimistically
  addToHistory: (query: any) => {
    queryClient.setQueryData(
      queryKeys.queries.history(),
      (old: any) => {
        if (!old) return [query];
        return [query, ...old.slice(0, 99)]; // Keep only 100 items
      }
    );
  },
  
  // Update query result optimistically
  updateQueryResult: (queryId: string, result: any) => {
    queryClient.setQueryData(
      queryKeys.queries.detail(queryId),
      result
    );
  },
};

// Background sync helpers
export const backgroundSync = {
  // Sync query history in background
  syncHistory: () => {
    queryClient.refetchQueries({ 
      queryKey: queryKeys.queries.history(),
      type: 'active' 
    });
  },
  
  // Sync health status in background
  syncHealth: () => {
    queryClient.refetchQueries({ 
      queryKey: queryKeys.health.all,
      type: 'active' 
    });
  },
};

// Error handling helpers
export const errorHandlers = {
  // Default error handler for queries
  defaultQueryError: (error: any) => {
    console.error('Query error:', error);
    
    // Handle specific error types
    if (error?.response?.status === 401) {
      // Handle authentication errors
      window.location.href = '/login';
    } else if (error?.response?.status >= 500) {
      // Handle server errors
      console.error('Server error occurred');
    }
  },
  
  // Default error handler for mutations
  defaultMutationError: (error: any) => {
    console.error('Mutation error:', error);
    
    // Show user-friendly error message
    // This could integrate with a toast notification system
  },
};
