import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys, errorHandlers } from '../lib/react-query';
import { advancedVisualizationService } from '../services/advancedVisualizationService';

// Hook for getting visualization recommendations
export const useVisualizationRecommendations = (data: any[], enabled = true) => {
  return useQuery({
    queryKey: queryKeys.visualizations.recommendations(data),
    queryFn: () => advancedVisualizationService.getRecommendations(data),
    
    enabled: enabled && data && data.length > 0,
    
    // Recommendations can be cached for a while since they're based on data structure
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for generating charts
export const useGenerateChart = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (config: any) => advancedVisualizationService.generateChart(config),
    
    onSuccess: (data, variables) => {
      // Cache the generated chart
      queryClient.setQueryData(
        queryKeys.visualizations.chart(variables.id || 'latest'),
        data
      );
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for saving chart configurations
export const useSaveChartConfig = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (config: any) => advancedVisualizationService.saveChartConfig(config),
    
    onSuccess: () => {
      // Invalidate charts list
      queryClient.invalidateQueries({ queryKey: queryKeys.visualizations.charts() });
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for fetching saved charts
export const useSavedCharts = () => {
  return useQuery({
    queryKey: queryKeys.visualizations.charts(),
    queryFn: () => advancedVisualizationService.getSavedCharts(),
    
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for fetching a specific chart
export const useChart = (chartId: string, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.visualizations.chart(chartId),
    queryFn: () => advancedVisualizationService.getChart(chartId),
    
    enabled: enabled && !!chartId,
    
    staleTime: 15 * 60 * 1000, // 15 minutes
    gcTime: 60 * 60 * 1000, // 1 hour
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for deleting charts
export const useDeleteChart = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (chartId: string) => advancedVisualizationService.deleteChart(chartId),
    
    onSuccess: (data, chartId) => {
      // Remove from cache
      queryClient.removeQueries({ queryKey: queryKeys.visualizations.chart(chartId) });
      
      // Invalidate charts list
      queryClient.invalidateQueries({ queryKey: queryKeys.visualizations.charts() });
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for exporting charts
export const useExportChart = () => {
  return useMutation({
    mutationFn: ({ chartId, format }: { chartId: string; format: string }) => 
      advancedVisualizationService.exportChart(chartId, format),
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for generating interactive dashboards
export const useGenerateDashboard = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (config: any) => advancedVisualizationService.generateDashboard(config),
    
    onSuccess: (data) => {
      // Cache the dashboard
      queryClient.setQueryData(['dashboards', data.id], data);
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for real-time chart updates
export const useRealtimeChart = (chartId: string, enabled = true) => {
  return useQuery({
    queryKey: ['charts', chartId, 'realtime'],
    queryFn: () => advancedVisualizationService.getRealtimeData(chartId),
    
    enabled: enabled && !!chartId,
    
    // Very short stale time for real-time data
    staleTime: 5 * 1000, // 5 seconds
    gcTime: 30 * 1000, // 30 seconds
    
    // Refetch frequently for real-time updates
    refetchInterval: 10 * 1000, // 10 seconds
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for chart performance analytics
export const useChartAnalytics = (chartId: string, enabled = true) => {
  return useQuery({
    queryKey: ['charts', chartId, 'analytics'],
    queryFn: () => advancedVisualizationService.getChartAnalytics(chartId),
    
    enabled: enabled && !!chartId,
    
    staleTime: 60 * 1000, // 1 minute
    gcTime: 5 * 60 * 1000, // 5 minutes
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for optimizing chart performance
export const useOptimizeChart = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (chartConfig: any) => advancedVisualizationService.optimizeChart(chartConfig),
    
    onSuccess: (optimizedConfig, originalConfig) => {
      // Update the chart cache with optimized version
      if (originalConfig.id) {
        queryClient.setQueryData(
          queryKeys.visualizations.chart(originalConfig.id),
          optimizedConfig
        );
      }
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for chart collaboration features
export const useShareChart = () => {
  return useMutation({
    mutationFn: ({ chartId, shareConfig }: { chartId: string; shareConfig: any }) => 
      advancedVisualizationService.shareChart(chartId, shareConfig),
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for chart version history
export const useChartVersions = (chartId: string, enabled = true) => {
  return useQuery({
    queryKey: ['charts', chartId, 'versions'],
    queryFn: () => advancedVisualizationService.getChartVersions(chartId),
    
    enabled: enabled && !!chartId,
    
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for restoring chart versions
export const useRestoreChartVersion = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ chartId, versionId }: { chartId: string; versionId: string }) => 
      advancedVisualizationService.restoreChartVersion(chartId, versionId),
    
    onSuccess: (data, { chartId }) => {
      // Invalidate chart cache to refetch updated version
      queryClient.invalidateQueries({ queryKey: queryKeys.visualizations.chart(chartId) });
      queryClient.invalidateQueries({ queryKey: ['charts', chartId, 'versions'] });
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for bulk chart operations
export const useBulkChartOperations = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ operation, chartIds }: { operation: string; chartIds: string[] }) => 
      advancedVisualizationService.bulkChartOperation(operation, chartIds),
    
    onSuccess: () => {
      // Invalidate all chart-related queries
      queryClient.invalidateQueries({ queryKey: queryKeys.visualizations.all });
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for chart templates
export const useChartTemplates = () => {
  return useQuery({
    queryKey: ['chart-templates'],
    queryFn: () => advancedVisualizationService.getChartTemplates(),
    
    // Templates don't change often
    staleTime: 60 * 60 * 1000, // 1 hour
    gcTime: 2 * 60 * 60 * 1000, // 2 hours
    
    onError: errorHandlers.defaultQueryError,
  });
};

// Hook for creating charts from templates
export const useCreateFromTemplate = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ templateId, data }: { templateId: string; data: any }) => 
      advancedVisualizationService.createFromTemplate(templateId, data),
    
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.visualizations.charts() });
    },
    
    onError: errorHandlers.defaultMutationError,
  });
};

// Hook for prefetching visualization data
export const usePrefetchVisualizationData = () => {
  const queryClient = useQueryClient();
  
  const prefetchCharts = async () => {
    await queryClient.prefetchQuery({
      queryKey: queryKeys.visualizations.charts(),
      queryFn: () => advancedVisualizationService.getSavedCharts(),
    });
  };
  
  const prefetchTemplates = async () => {
    await queryClient.prefetchQuery({
      queryKey: ['chart-templates'],
      queryFn: () => advancedVisualizationService.getChartTemplates(),
    });
  };
  
  return {
    prefetchCharts,
    prefetchTemplates,
  };
};
