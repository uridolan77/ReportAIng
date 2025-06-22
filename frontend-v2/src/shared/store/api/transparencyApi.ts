import { baseApi } from './baseApi'
import type {
  TransparencyReport,
  PromptConstructionTrace,
  ConfidenceBreakdown,
  AlternativeOption,
  OptimizationSuggestion,
  TransparencyMetrics,
  AnalyzePromptRequest,
  OptimizePromptRequest
} from '@shared/types/transparency'

/**
 * Transparency API - RTK Query service for AI transparency endpoints
 * 
 * Integrates with the new TransparencyController.cs backend
 * Base URL: /api/transparency
 */
export const transparencyApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Get transparency trace by ID
    getTransparencyTrace: builder.query<TransparencyReport, string>({
      query: (traceId) => `/transparency/trace/${traceId}`,
      providesTags: (result, error, traceId) => [
        { type: 'TransparencyTrace', id: traceId }
      ],
    }),

    // Analyze prompt construction
    analyzePromptConstruction: builder.mutation<PromptConstructionTrace, AnalyzePromptRequest>({
      query: (body) => ({
        url: '/transparency/analyze',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['TransparencyTrace'],
    }),

    // Get confidence breakdown by analysis ID
    getConfidenceBreakdown: builder.query<ConfidenceBreakdown, string>({
      query: (analysisId) => `/transparency/confidence/${analysisId}`,
      providesTags: (result, error, analysisId) => [
        { type: 'ConfidenceBreakdown', id: analysisId }
      ],
    }),

    // Get alternative options for a trace
    getAlternativeOptions: builder.query<AlternativeOption[], string>({
      query: (traceId) => `/transparency/alternatives/${traceId}`,
      providesTags: (result, error, traceId) => [
        { type: 'AlternativeOptions', id: traceId }
      ],
    }),

    // Get optimization suggestions
    getOptimizationSuggestions: builder.mutation<OptimizationSuggestion[], OptimizePromptRequest>({
      query: (body) => ({
        url: '/transparency/optimize',
        method: 'POST',
        body,
      }),
    }),

    // Get transparency metrics
    getTransparencyMetrics: builder.query<TransparencyMetrics, { 
      userId?: string
      days?: number 
      includeDetails?: boolean 
    }>({
      query: ({ userId, days = 7, includeDetails = false }) => ({
        url: '/transparency/metrics',
        params: { userId, days, includeDetails },
      }),
      providesTags: ['TransparencyMetrics'],
    }),

    // Get transparency metrics for dashboard
    getTransparencyDashboardMetrics: builder.query<{
      totalTraces: number
      averageConfidence: number
      topOptimizations: OptimizationSuggestion[]
      confidenceTrends: Array<{ date: string; confidence: number }>
      usageByModel: Array<{ model: string; count: number }>
    }, { days?: number }>({
      query: ({ days = 30 }) => ({
        url: '/transparency/metrics/dashboard',
        params: { days },
      }),
      providesTags: ['TransparencyDashboard'],
    }),

    // Export transparency data
    exportTransparencyData: builder.mutation<Blob, {
      traceIds?: string[]
      dateRange?: { start: string; end: string }
      format?: 'json' | 'csv' | 'excel'
    }>({
      query: (body) => ({
        url: '/transparency/export',
        method: 'POST',
        body,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Get transparency settings
    getTransparencySettings: builder.query<{
      enableDetailedLogging: boolean
      confidenceThreshold: number
      retentionDays: number
      enableOptimizationSuggestions: boolean
    }, void>({
      query: () => '/transparency/settings',
      providesTags: ['TransparencySettings'],
    }),

    // Update transparency settings
    updateTransparencySettings: builder.mutation<void, {
      enableDetailedLogging?: boolean
      confidenceThreshold?: number
      retentionDays?: number
      enableOptimizationSuggestions?: boolean
    }>({
      query: (body) => ({
        url: '/transparency/settings',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['TransparencySettings'],
    }),

    // Get transparency traces with filtering and pagination
    getTransparencyTraces: builder.query<{
      traces: Array<{
        id: string
        traceId: string
        userId: string
        userQuestion: string
        intentType: string
        overallConfidence: number
        totalTokens: number
        success: boolean
        createdAt: string
        processingTime: number
        stepCount: number
      }>
      total: number
      page: number
      pageSize: number
    }, {
      page?: number
      pageSize?: number
      search?: string
      userId?: string
      confidenceMin?: number
      confidenceMax?: number
      success?: boolean
      dateFrom?: string
      dateTo?: string
      sortBy?: string
      sortOrder?: 'asc' | 'desc'
    }>({
      query: (params) => ({
        url: '/transparency/traces',
        params,
      }),
      providesTags: ['TransparencyTraces'],
    }),

    // Get real-time monitoring data
    getRealTimeMonitoringData: builder.query<{
      activeQueries: number
      totalQueries: number
      averageConfidence: number
      averageProcessingTime: number
      errorRate: number
      lastUpdate: string
    }, void>({
      query: () => '/transparency/monitoring/status',
      providesTags: ['RealTimeMonitoring'],
    }),

    // Submit user feedback
    submitUserFeedback: builder.mutation<void, {
      queryId: string
      traceId?: string
      rating: number
      sentiment: 'positive' | 'negative' | 'neutral'
      comment?: string
      categories: string[]
      timestamp: string
      userId: string
    }>({
      query: (body) => ({
        url: '/transparency/feedback',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['TransparencyTraces', 'TransparencyMetrics'],
    }),

    // Get user feedback for a query
    getUserFeedback: builder.query<Array<{
      id: string
      type: 'rating' | 'comment' | 'suggestion'
      content: string
      rating?: number
      timestamp: string
      status: 'pending' | 'acknowledged' | 'implemented'
    }>, string>({
      query: (queryId) => `/transparency/feedback/${queryId}`,
      providesTags: (result, error, queryId) => [
        { type: 'UserFeedback', id: queryId }
      ],
    }),

    // Get model performance comparison data
    getModelPerformanceComparison: builder.query<Array<{
      id: string
      name: string
      version?: string
      provider: string
      metrics: {
        averageConfidence: number
        successRate: number
        averageResponseTime: number
        tokenEfficiency: number
        costPerQuery: number
        totalQueries: number
        errorRate: number
      }
      capabilities: string[]
      lastUpdated: string
    }>, { days?: number }>({
      query: ({ days = 30 }) => ({
        url: '/transparency/models/comparison',
        params: { days },
      }),
      providesTags: ['ModelPerformance'],
    }),

    // Get optimization insights
    getOptimizationInsights: builder.query<Array<{
      id: string
      title: string
      description: string
      category: string
      priority: 'high' | 'medium' | 'low'
      implementationComplexity: 'easy' | 'medium' | 'hard'
      estimatedImpact?: {
        performance: number
        cost: number
        accuracy: number
      }
      implementationSteps?: string[]
    }>, { category?: string; priority?: string }>({
      query: (params) => ({
        url: '/transparency/insights/optimization',
        params,
      }),
      providesTags: ['OptimizationInsights'],
    }),

    // Apply optimization suggestion
    applyOptimizationSuggestion: builder.mutation<void, {
      suggestionId: string
      userId: string
    }>({
      query: (body) => ({
        url: '/transparency/insights/apply',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['OptimizationInsights', 'TransparencySettings'],
    }),

    // Get data management statistics
    getDataManagementStats: builder.query<{
      totalDataSize: number
      tracesSize: number
      metricsSize: number
      logsSize: number
      dataHealth: {
        status: 'healthy' | 'warning' | 'critical'
        integrity: number
        lastBackup: string
        nextCleanup: string
      }
    }, void>({
      query: () => '/transparency/data/stats',
      providesTags: ['DataManagement'],
    }),

    // Trigger data cleanup
    triggerDataCleanup: builder.mutation<void, {
      olderThanDays: number
      dryRun?: boolean
    }>({
      query: (body) => ({
        url: '/transparency/data/cleanup',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['DataManagement', 'TransparencyTraces'],
    }),
  }),
  overrideExisting: false,
})

// Export hooks for use in components
export const {
  useGetTransparencyTraceQuery,
  useAnalyzePromptConstructionMutation,
  useGetConfidenceBreakdownQuery,
  useGetAlternativeOptionsQuery,
  useGetOptimizationSuggestionsMutation,
  useGetTransparencyMetricsQuery,
  useGetTransparencyDashboardMetricsQuery,
  useExportTransparencyDataMutation,
  useGetTransparencySettingsQuery,
  useUpdateTransparencySettingsMutation,
  useGetTransparencyTracesQuery,
  useGetRealTimeMonitoringDataQuery,
  useSubmitUserFeedbackMutation,
  useGetUserFeedbackQuery,
  useGetModelPerformanceComparisonQuery,
  useGetOptimizationInsightsQuery,
  useApplyOptimizationSuggestionMutation,
  useGetDataManagementStatsQuery,
  useTriggerDataCleanupMutation,
} = transparencyApi

// Enhanced hooks with additional functionality
export const useTransparencyTrace = (traceId: string) => {
  const traceQuery = useGetTransparencyTraceQuery(traceId, {
    skip: !traceId,
  })
  
  const confidenceQuery = useGetConfidenceBreakdownQuery(traceId, {
    skip: !traceId || !traceQuery.data,
  })
  
  const alternativesQuery = useGetAlternativeOptionsQuery(traceId, {
    skip: !traceId || !traceQuery.data,
  })

  return {
    trace: traceQuery.data,
    confidence: confidenceQuery.data,
    alternatives: alternativesQuery.data,
    isLoading: traceQuery.isLoading || confidenceQuery.isLoading || alternativesQuery.isLoading,
    error: traceQuery.error || confidenceQuery.error || alternativesQuery.error,
    refetch: () => {
      traceQuery.refetch()
      confidenceQuery.refetch()
      alternativesQuery.refetch()
    }
  }
}

export const useTransparencyDashboard = (days = 30) => {
  const metricsQuery = useGetTransparencyDashboardMetricsQuery({ days })
  const settingsQuery = useGetTransparencySettingsQuery()

  return {
    metrics: metricsQuery.data,
    settings: settingsQuery.data,
    isLoading: metricsQuery.isLoading || settingsQuery.isLoading,
    error: metricsQuery.error || settingsQuery.error,
    refetch: () => {
      metricsQuery.refetch()
      settingsQuery.refetch()
    }
  }
}

// Enhanced hook for transparency management page
export const useTransparencyManagement = () => {
  const settingsQuery = useGetTransparencySettingsQuery()
  const metricsQuery = useGetTransparencyMetricsQuery({ days: 7, includeDetails: true })
  const monitoringQuery = useGetRealTimeMonitoringDataQuery(undefined, {
    pollingInterval: 5000, // Poll every 5 seconds
  })
  const dataStatsQuery = useGetDataManagementStatsQuery()
  const optimizationQuery = useGetOptimizationInsightsQuery({})

  const [updateSettings] = useUpdateTransparencySettingsMutation()
  const [triggerCleanup] = useTriggerDataCleanupMutation()

  return {
    settings: settingsQuery.data,
    metrics: metricsQuery.data,
    monitoring: monitoringQuery.data,
    dataStats: dataStatsQuery.data,
    optimizations: optimizationQuery.data,
    isLoading: settingsQuery.isLoading || metricsQuery.isLoading ||
               monitoringQuery.isLoading || dataStatsQuery.isLoading,
    error: settingsQuery.error || metricsQuery.error ||
           monitoringQuery.error || dataStatsQuery.error,
    updateSettings,
    triggerCleanup,
    refetch: () => {
      settingsQuery.refetch()
      metricsQuery.refetch()
      monitoringQuery.refetch()
      dataStatsQuery.refetch()
      optimizationQuery.refetch()
    }
  }
}

// Enhanced hook for transparency review page
export const useTransparencyReview = (filters: {
  page?: number
  pageSize?: number
  search?: string
  userId?: string
  confidenceMin?: number
  confidenceMax?: number
  success?: boolean
  dateFrom?: string
  dateTo?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
} = {}) => {
  const tracesQuery = useGetTransparencyTracesQuery(filters)
  const modelComparisonQuery = useGetModelPerformanceComparisonQuery({ days: 30 })

  const [submitFeedback] = useSubmitUserFeedbackMutation()

  return {
    traces: tracesQuery.data?.traces || [],
    total: tracesQuery.data?.total || 0,
    page: tracesQuery.data?.page || 1,
    pageSize: tracesQuery.data?.pageSize || 20,
    modelComparison: modelComparisonQuery.data,
    isLoading: tracesQuery.isLoading || modelComparisonQuery.isLoading,
    error: tracesQuery.error || modelComparisonQuery.error,
    submitFeedback,
    refetch: () => {
      tracesQuery.refetch()
      modelComparisonQuery.refetch()
    }
  }
}

// Enhanced hook for real-time monitoring with error handling
export const useRealTimeMonitoring = (options: {
  pollingInterval?: number
  enabled?: boolean
} = {}) => {
  const { pollingInterval = 5000, enabled = true } = options

  const monitoringQuery = useGetRealTimeMonitoringDataQuery(undefined, {
    pollingInterval: enabled ? pollingInterval : 0,
    skip: !enabled,
  })

  return {
    data: monitoringQuery.data,
    isLoading: monitoringQuery.isLoading,
    error: monitoringQuery.error,
    isConnected: !monitoringQuery.error && monitoringQuery.data !== undefined,
    refetch: monitoringQuery.refetch
  }
}

// Enhanced hook for user feedback with optimistic updates
export const useUserFeedback = (queryId: string) => {
  const feedbackQuery = useGetUserFeedbackQuery(queryId, {
    skip: !queryId,
  })

  const [submitFeedback] = useSubmitUserFeedbackMutation()

  const submitFeedbackWithOptimisticUpdate = async (feedback: Parameters<typeof submitFeedback>[0]) => {
    try {
      await submitFeedback(feedback).unwrap()
      // Refetch to get updated data
      feedbackQuery.refetch()
    } catch (error) {
      console.error('Failed to submit feedback:', error)
      throw error
    }
  }

  return {
    feedback: feedbackQuery.data || [],
    isLoading: feedbackQuery.isLoading,
    error: feedbackQuery.error,
    submitFeedback: submitFeedbackWithOptimisticUpdate,
    refetch: feedbackQuery.refetch
  }
}

export default transparencyApi
