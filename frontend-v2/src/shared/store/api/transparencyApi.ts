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







export default transparencyApi
