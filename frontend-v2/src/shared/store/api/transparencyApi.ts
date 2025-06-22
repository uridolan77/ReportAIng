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

export default transparencyApi
