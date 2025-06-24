import { baseApi } from './baseApi'
import type {
  ProcessFlowSession,
  ProcessFlowAnalyzeRequest,
  ProcessFlowMetricsFilters,
  ProcessFlowAnalyticsRequest,
  ProcessFlowAnalyticsResponse,
  ProcessFlowDashboardData,
  ProcessFlowAnalyticsExportConfig
} from '@shared/types/transparency'

/**
 * ProcessFlow Transparency API - RTK Query service for ProcessFlow transparency endpoints
 *
 * Integrates with the ProcessFlow system backend
 * Base URL: /api/transparency
 */
export const transparencyApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Get ProcessFlow session by ID
    getProcessFlowSession: builder.query<ProcessFlowSession, string>({
      query: (sessionId) => `/transparency/trace/${sessionId}`,
      providesTags: (result, error, sessionId) => [
        { type: 'ProcessFlowSession', id: sessionId }
      ],
    }),

    // Analyze prompt construction with ProcessFlow
    analyzePromptConstruction: builder.mutation<ProcessFlowSession, ProcessFlowAnalyzeRequest>({
      query: (body) => ({
        url: '/transparency/analyze',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['ProcessFlowSession'],
    }),

    // Get ProcessFlow analytics
    getProcessFlowAnalytics: builder.query<ProcessFlowAnalyticsResponse, ProcessFlowAnalyticsRequest>({
      query: (params) => ({
        url: '/transparency/analytics',
        params,
      }),
      providesTags: ['ProcessFlowAnalytics'],
    }),

    // Get ProcessFlow dashboard data
    getProcessFlowDashboard: builder.query<ProcessFlowDashboardData, ProcessFlowMetricsFilters>({
      query: (params) => ({
        url: '/transparency/dashboard',
        params,
      }),
      providesTags: ['ProcessFlowDashboard'],
    }),

    // Export ProcessFlow analytics data
    exportProcessFlowData: builder.mutation<Blob, ProcessFlowAnalyticsExportConfig>({
      query: (body) => ({
        url: '/transparency/export/processflow',
        method: 'POST',
        body,
        responseHandler: (response) => response.blob(),
      }),
    }),



    // Get ProcessFlow sessions with filtering and pagination
    getProcessFlowSessions: builder.query<{
      sessions: Array<{
        sessionId: string
        userId: string
        userQuery: string
        queryType: string
        status: string
        overallConfidence?: number
        totalDurationMs?: number
        startTime: string
        endTime?: string
      }>
      total: number
      page: number
      pageSize: number
    }, {
      page?: number
      pageSize?: number
      search?: string
      userId?: string
      status?: string
      queryType?: string
      dateFrom?: string
      dateTo?: string
      sortBy?: string
      sortOrder?: 'asc' | 'desc'
    }>({
      query: (params) => ({
        url: '/transparency/sessions',
        params,
      }),
      providesTags: ['ProcessFlowSessions'],
    }),

    // Get transparency dashboard metrics
    getTransparencyDashboardMetrics: builder.query<{
      totalTraces: number
      averageConfidence: number
      totalQueries: number
      successRate: number
      averageResponseTime: number
    }, { days?: number }>({
      query: (params) => ({
        url: '/transparency/dashboard/metrics',
        params,
      }),
      providesTags: ['TransparencyMetrics'],
    }),

    // Get transparency settings
    getTransparencySettings: builder.query<{
      enabled: boolean
      level: 'basic' | 'detailed' | 'expert'
      autoShow: boolean
      confidenceThreshold: number
    }, void>({
      query: () => '/transparency/settings',
      providesTags: ['TransparencySettings'],
    }),

    // Update transparency settings
    updateTransparencySettings: builder.mutation<void, {
      enabled?: boolean
      level?: 'basic' | 'detailed' | 'expert'
      autoShow?: boolean
      confidenceThreshold?: number
    }>({
      query: (body) => ({
        url: '/transparency/settings',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['TransparencySettings'],
    }),

    // Get transparency metrics
    getTransparencyMetrics: builder.query<{
      totalTraces: number
      averageConfidence: number
      performanceMetrics: any
      usageStats: any
    }, { period?: string }>({
      query: (params) => ({
        url: '/transparency/metrics',
        params,
      }),
      providesTags: ['TransparencyMetrics'],
    }),

    // Get model performance comparison
    getModelPerformanceComparison: builder.query<{
      models: Array<{
        modelId: string
        averageConfidence: number
        responseTime: number
        successRate: number
        usageCount: number
      }>
    }, { period?: string }>({
      query: (params) => ({
        url: '/transparency/models/comparison',
        params,
      }),
      providesTags: ['ModelPerformance'],
    }),

    // Get real-time monitoring data
    getRealTimeMonitoringData: builder.query<{
      activeQueries: number
      averageConfidence: number
      systemLoad: number
      connectionStatus: string
    }, void>({
      query: () => '/transparency/monitoring/realtime',
      providesTags: ['RealTimeMonitoring'],
    }),


  }),
  overrideExisting: false,
})

// Export hooks for use in components
export const {
  useGetProcessFlowSessionQuery,
  useGetProcessFlowAnalyticsQuery,
  useGetProcessFlowDashboardQuery,
  useExportProcessFlowDataMutation,
  useAnalyzePromptConstructionMutation,
  useGetProcessFlowSessionsQuery,
  useGetTransparencyDashboardMetricsQuery,
  useGetTransparencySettingsQuery,
  useUpdateTransparencySettingsMutation,
  useGetTransparencyMetricsQuery,
  useGetModelPerformanceComparisonQuery,
  useGetRealTimeMonitoringDataQuery,
} = transparencyApi

// Enhanced hooks for ProcessFlow functionality
export const useProcessFlowSession = (sessionId: string) => {
  const sessionQuery = useGetProcessFlowSessionQuery(sessionId, {
    skip: !sessionId,
  })

  return {
    session: sessionQuery.data,
    steps: sessionQuery.data?.steps || [],
    logs: sessionQuery.data?.logs || [],
    transparency: sessionQuery.data?.transparency,
    isLoading: sessionQuery.isLoading,
    error: sessionQuery.error,
    refetch: sessionQuery.refetch
  }
}

export const useProcessFlowDashboard = (filters: ProcessFlowMetricsFilters = {}) => {
  const dashboardQuery = useGetProcessFlowDashboardQuery(filters)
  const analyticsQuery = useGetProcessFlowAnalyticsQuery({
    ...filters,
    includeStepDetails: true,
    includeTokenUsage: true,
    includePerformanceMetrics: true
  })

  return {
    dashboard: dashboardQuery.data,
    analytics: analyticsQuery.data,
    isLoading: dashboardQuery.isLoading || analyticsQuery.isLoading,
    error: dashboardQuery.error || analyticsQuery.error,
    refetch: () => {
      dashboardQuery.refetch()
      analyticsQuery.refetch()
    }
  }
}

export const useProcessFlowManagement = () => {
  const analyticsQuery = useGetProcessFlowAnalyticsQuery({
    includeStepDetails: true,
    includeTokenUsage: true,
    includePerformanceMetrics: true
  })
  const dashboardQuery = useGetProcessFlowDashboardQuery({ days: 7, includeDetails: true })

  return {
    analytics: analyticsQuery.data,
    dashboard: dashboardQuery.data,
    isLoading: analyticsQuery.isLoading || dashboardQuery.isLoading,
    error: analyticsQuery.error || dashboardQuery.error,
    refetch: () => {
      analyticsQuery.refetch()
      dashboardQuery.refetch()
    }
  }
}

// Real-time monitoring hook
export const useRealTimeMonitoring = (options: { enabled?: boolean } = {}) => {
  const { enabled = true } = options

  const monitoringQuery = useGetRealTimeMonitoringDataQuery(undefined, {
    skip: !enabled,
    pollingInterval: 5000, // Poll every 5 seconds
  })

  return {
    data: monitoringQuery.data,
    isConnected: !monitoringQuery.error && !monitoringQuery.isLoading,
    isLoading: monitoringQuery.isLoading,
    error: monitoringQuery.error,
    refetch: monitoringQuery.refetch
  }
}

// Transparency review hook
export const useTransparencyReview = (sessionId?: string) => {
  const sessionQuery = useGetProcessFlowSessionQuery(sessionId || '', {
    skip: !sessionId,
  })

  return {
    session: sessionQuery.data,
    isLoading: sessionQuery.isLoading,
    error: sessionQuery.error,
    refetch: sessionQuery.refetch
  }
}







export default transparencyApi
