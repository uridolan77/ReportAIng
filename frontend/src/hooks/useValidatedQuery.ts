import { useQuery, useMutation, useQueryClient, UseQueryOptions, UseMutationOptions } from '@tanstack/react-query';
import { z } from 'zod';
import { validateApiResponse, createValidationMiddleware } from '../utils/validation';
import { ApiService } from '../services/api';
import {
  QueryResponseSchema,
  EnhancedQueryResponseSchema,
  SchemaMetadataSchema,
  DashboardOverviewSchema,
  HealthStatusSchema,
  QueryHistoryItemSchema,
  UserProfileSchema,
  AuthenticationResultSchema,
} from '../schemas/api';

// Enhanced query hook with automatic validation
export function useValidatedQuery<TData, TError = Error>(
  queryKey: any[],
  queryFn: () => Promise<unknown>,
  schema: z.ZodSchema<TData>,
  options?: Omit<UseQueryOptions<TData, TError>, 'queryKey' | 'queryFn'> & {
    validationContext?: string;
  }
) {
  const { validationContext, ...queryOptions } = options || {};

  return useQuery<TData, TError>({
    queryKey,
    queryFn: async () => {
      const rawData = await queryFn();
      return validateApiResponse(schema, rawData, validationContext || queryKey.join(':'));
    },
    ...queryOptions,
  });
}

// Enhanced mutation hook with validation
export function useValidatedMutation<TData, TError = Error, TVariables = void>(
  mutationFn: (variables: TVariables) => Promise<unknown>,
  schema: z.ZodSchema<TData>,
  options?: Omit<UseMutationOptions<TData, TError, TVariables>, 'mutationFn'> & {
    validationContext?: string;
  }
) {
  const { validationContext, ...mutationOptions } = options || {};

  return useMutation<TData, TError, TVariables>({
    mutationFn: async (variables) => {
      const rawData = await mutationFn(variables);
      return validateApiResponse(schema, rawData, validationContext || 'mutation');
    },
    ...mutationOptions,
  });
}

// Specific hooks for common API calls

// Authentication hooks
export function useLogin() {
  return useValidatedMutation(
    (credentials: { username: string; password: string }) => ApiService.login(credentials),
    AuthenticationResultSchema,
    {
      validationContext: 'login',
    }
  );
}

export function useCurrentUser() {
  return useValidatedQuery(
    ['auth', 'currentUser'],
    () => ApiService.getCurrentUser(),
    UserProfileSchema,
    {
      validationContext: 'currentUser',
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: (failureCount, error) => {
        // Don't retry on 401 errors
        if (error instanceof Error && error.message.includes('401')) {
          return false;
        }
        return failureCount < 3;
      },
    }
  );
}

// Query execution hooks
export function useExecuteQuery() {
  const queryClient = useQueryClient();

  return useValidatedMutation(
    (request: {
      naturalLanguageQuery: string;
      includeExplanation?: boolean;
      maxRows?: number;
      timeout?: number;
      useCache?: boolean;
      context?: Record<string, unknown>;
    }) => ApiService.executeQuery(request),
    QueryResponseSchema,
    {
      validationContext: 'executeQuery',
      onSuccess: (data) => {
        // Invalidate query history when a new query is executed
        queryClient.invalidateQueries({ queryKey: ['queries', 'history'] });

        // Cache the query result
        queryClient.setQueryData(['queries', 'result', data.queryId], data);
      },
    }
  );
}

export function useExecuteEnhancedQuery() {
  const queryClient = useQueryClient();

  return useValidatedMutation(
    (request: {
      naturalLanguageQuery: string;
      includeExplanation?: boolean;
      maxRows?: number;
      timeout?: number;
      useCache?: boolean;
      context?: Record<string, unknown>;
    }) => ApiService.executeQuery(request),
    EnhancedQueryResponseSchema,
    {
      validationContext: 'executeEnhancedQuery',
      onSuccess: (data) => {
        queryClient.invalidateQueries({ queryKey: ['queries', 'history'] });

        if (data.processedQuery) {
          queryClient.setQueryData(['queries', 'enhanced', data.processedQuery.sql], data);
        }
      },
    }
  );
}

export function useValidateSql() {
  return useValidatedMutation(
    (sql: string) => ApiService.validateSql(sql),
    z.object({
      isValid: z.boolean(),
      errors: z.array(z.string()),
      warnings: z.array(z.string()),
      suggestions: z.array(z.string()),
    }),
    {
      validationContext: 'validateSql',
    }
  );
}

// Query history hooks
export function useQueryHistory(page: number = 1, pageSize: number = 20) {
  return useValidatedQuery(
    ['queries', 'history', page, pageSize],
    () => ApiService.getQueryHistory(page, pageSize),
    z.array(QueryHistoryItemSchema),
    {
      validationContext: 'queryHistory',
      staleTime: 2 * 60 * 1000, // 2 minutes
      placeholderData: (previousData) => previousData,
    }
  );
}

export function useQuerySuggestions() {
  return useValidatedQuery(
    ['queries', 'suggestions'],
    () => ApiService.getQuerySuggestions(),
    z.array(z.string()),
    {
      validationContext: 'querySuggestions',
      staleTime: 10 * 60 * 1000, // 10 minutes
    }
  );
}

// Schema metadata hooks
export function useSchemaMetadata() {
  return useValidatedQuery(
    ['schema', 'metadata'],
    () => ApiService.getSchemaMetadata(),
    SchemaMetadataSchema,
    {
      validationContext: 'schemaMetadata',
      staleTime: 30 * 60 * 1000, // 30 minutes
      gcTime: 60 * 60 * 1000, // 1 hour
    }
  );
}

export function useTableMetadata(tableName: string) {
  return useValidatedQuery(
    ['schema', 'tables', tableName],
    () => ApiService.getTableMetadata(tableName),
    z.object({
      name: z.string(),
      schema: z.string(),
      columns: z.array(z.object({
        name: z.string(),
        dataType: z.string(),
        isNullable: z.boolean(),
        maxLength: z.number().optional(),
        description: z.string().optional(),
      })),
      description: z.string().optional(),
    }),
    {
      validationContext: `tableMetadata:${tableName}`,
      enabled: !!tableName,
      staleTime: 30 * 60 * 1000, // 30 minutes
    }
  );
}

// Dashboard hooks
export function useDashboardOverview() {
  return useValidatedQuery(
    ['dashboard', 'overview'],
    () => ApiService.getDashboardOverview(),
    DashboardOverviewSchema,
    {
      validationContext: 'dashboardOverview',
      staleTime: 5 * 60 * 1000, // 5 minutes
      refetchInterval: 10 * 60 * 1000, // Refetch every 10 minutes
    }
  );
}

// Health check hooks
export function useHealthStatus() {
  return useValidatedQuery(
    ['health', 'status'],
    () => ApiService.getHealthStatus(),
    HealthStatusSchema,
    {
      validationContext: 'healthStatus',
      staleTime: 1 * 60 * 1000, // 1 minute
      refetchInterval: 2 * 60 * 1000, // Refetch every 2 minutes
      retry: (failureCount, error) => {
        // Always retry health checks, but with exponential backoff
        return failureCount < 5;
      },
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
    }
  );
}

// Streaming query hook
export function useStreamingQuery() {
  const queryClient = useQueryClient();

  return {
    startStream: (
      query: string,
      onData: (data: any) => void,
      onError: (error: Error) => void,
      onComplete: () => void
    ) => {
      return ApiService.startStreamingQuery(
        query,
        (data) => {
          // Validate streaming data if needed
          try {
            onData(data);
          } catch (error) {
            console.warn('Invalid streaming data received:', data);
          }
        },
        onError,
        () => {
          // Invalidate query history when streaming completes
          queryClient.invalidateQueries({ queryKey: ['queries', 'history'] });
          onComplete();
        }
      );
    }
  };
}

// Utility hooks for cache management
export function useInvalidateQueries() {
  const queryClient = useQueryClient();

  return {
    invalidateAll: () => queryClient.invalidateQueries(),
    invalidateQueries: (queryKey: any[]) => queryClient.invalidateQueries({ queryKey }),
    invalidateQueryHistory: () => queryClient.invalidateQueries({ queryKey: ['queries', 'history'] }),
    invalidateSchema: () => queryClient.invalidateQueries({ queryKey: ['schema'] }),
    invalidateDashboard: () => queryClient.invalidateQueries({ queryKey: ['dashboard'] }),
    invalidateHealth: () => queryClient.invalidateQueries({ queryKey: ['health'] }),
  };
}

export function usePrefetchQueries() {
  const queryClient = useQueryClient();

  return {
    prefetchQueryHistory: (page: number = 1, pageSize: number = 20) => {
      return queryClient.prefetchQuery({
        queryKey: ['queries', 'history', page, pageSize],
        queryFn: () => ApiService.getQueryHistory(page, pageSize),
        staleTime: 2 * 60 * 1000,
      });
    },

    prefetchSchemaMetadata: () => {
      return queryClient.prefetchQuery({
        queryKey: ['schema', 'metadata'],
        queryFn: () => ApiService.getSchemaMetadata(),
        staleTime: 30 * 60 * 1000,
      });
    },

    prefetchDashboard: () => {
      return queryClient.prefetchQuery({
        queryKey: ['dashboard', 'overview'],
        queryFn: () => ApiService.getDashboardOverview(),
        staleTime: 5 * 60 * 1000,
      });
    },
  };
}

// Error boundary hook for validation errors
export function useValidationErrorHandler() {
  return {
    handleValidationError: (error: Error, context?: string) => {
      if (process.env.NODE_ENV === 'development') {
        console.error(`Validation error${context ? ` in ${context}` : ''}:`, error);
      }

      // You can add custom error handling logic here
      // For example, showing a toast notification or logging to an external service

      if (process.env.NODE_ENV === 'development') {
        // In development, show more detailed error information
        console.group('Validation Error Details');
        console.error('Error:', error.message);
        console.error('Stack:', error.stack);
        console.error('Context:', context);
        console.groupEnd();
      }
    },
  };
}
