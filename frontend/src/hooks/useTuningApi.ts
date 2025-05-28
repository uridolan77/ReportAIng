import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys, errorHandlers } from '../lib/react-query';
import { tuningApiService } from '../services/tuningApi';

// Hook for fetching tuning dashboard data
export const useTuningDashboard = () => {
  return useQuery({
    queryKey: queryKeys.tuning.dashboard(),
    queryFn: () => tuningApiService.getDashboard(),

    // Dashboard data changes infrequently
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for fetching business tables
export const useBusinessTables = () => {
  return useQuery({
    queryKey: queryKeys.tuning.tables(),
    queryFn: () => tuningApiService.getBusinessTables(),

    // Tables change rarely
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 60 * 60 * 1000, // 1 hour

    // onError deprecated in React Query v5 - use error boundaries instead
  });
};

// Hook for creating/updating business tables
export const useUpdateBusinessTable = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (table: any) => tuningApiService.updateBusinessTable(table.id, table),

    onSuccess: () => {
      // Invalidate business tables cache
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.tables() });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.dashboard() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for fetching business glossary
export const useBusinessGlossary = () => {
  return useQuery({
    queryKey: queryKeys.tuning.glossary(),
    queryFn: () => tuningApiService.getBusinessGlossary(),

    // Glossary changes infrequently
    staleTime: 15 * 60 * 1000, // 15 minutes
    gcTime: 60 * 60 * 1000, // 1 hour

    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for updating business glossary
export const useUpdateBusinessGlossary = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (glossary: any) => tuningApiService.updateBusinessGlossary(glossary),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.glossary() });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.dashboard() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for fetching query patterns
export const useQueryPatterns = () => {
  return useQuery({
    queryKey: queryKeys.tuning.patterns(),
    queryFn: () => tuningApiService.getQueryPatterns(),

    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes

    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for updating query patterns
export const useUpdateQueryPattern = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (pattern: any) => tuningApiService.updateQueryPattern(pattern.id, pattern),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.patterns() });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.dashboard() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for fetching prompt logs with pagination
export const usePromptLogs = (page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: queryKeys.tuning.prompts(page, pageSize),
    queryFn: () => tuningApiService.getPromptLogs(page, pageSize),

    // Keep previous data while fetching new page
    placeholderData: (previousData) => previousData,

    // Logs are frequently updated
    staleTime: 30 * 1000, // 30 seconds
    gcTime: 5 * 60 * 1000, // 5 minutes

    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for auto-generating table descriptions
export const useAutoGenerateTableDescriptions = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => tuningApiService.autoGenerateTableContexts(),

    onSuccess: () => {
      // Invalidate related caches
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.tables() });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.dashboard() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for auto-generating business glossary
export const useAutoGenerateGlossary = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => tuningApiService.autoGenerateGlossaryTerms(),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.glossary() });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.dashboard() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for clearing prompt cache
export const useClearPromptCache = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => tuningApiService.clearPromptCache(),

    onSuccess: () => {
      // Clear all query-related caches since prompts affect query results
      queryClient.invalidateQueries({ queryKey: queryKeys.queries.all });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.prompts() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for testing AI settings
export const useTestAISettings = () => {
  return useMutation({
    mutationFn: (settings: any) => tuningApiService.testAISettings(settings),

    // Don't retry AI tests automatically
    retry: false,

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for bulk operations
export const useBulkUpdateTables = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (tables: any[]) => tuningApiService.bulkUpdateTables(tables),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.tables() });
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.dashboard() });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for exporting tuning data
export const useExportTuningData = () => {
  return useMutation({
    mutationFn: (format: 'json' | 'csv' | 'excel') => tuningApiService.exportTuningData(format),

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for importing tuning data
export const useImportTuningData = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => tuningApiService.importTuningData(file),

    onSuccess: () => {
      // Invalidate all tuning data
      queryClient.invalidateQueries({ queryKey: queryKeys.tuning.all });
    },

    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for prefetching tuning data
export const usePrefetchTuningData = () => {
  const queryClient = useQueryClient();

  const prefetchDashboard = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.tuning.dashboard(),
      queryFn: () => tuningApiService.getDashboard(),
    });
  };

  const prefetchTables = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.tuning.tables(),
      queryFn: () => tuningApiService.getBusinessTables(),
    });
  };

  const prefetchGlossary = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.tuning.glossary(),
      queryFn: () => tuningApiService.getBusinessGlossary(),
    });
  };

  return {
    prefetchDashboard,
    prefetchTables,
    prefetchGlossary,
  };
};
