import { baseApi } from './baseApi'
import type {
  TokenUsageStatistics,
  DailyTokenUsage,
  TokenUsageFilters,
  ProcessFlowSessionAnalytics,
  ProcessFlowStepAnalytics,
  EnhancedQueryAnalytics,
  ProcessFlowTransparencyMetrics,
  ProcessFlowRealTimeMetrics,
  ProcessFlowAnalyticsRequest,
  ProcessFlowAnalyticsResponse,
  ProcessFlowDashboardData,
  ProcessFlowAnalyticsExportConfig
} from '@shared/types/transparency'

/**
 * Analytics API - RTK Query service for ProcessFlow analytics endpoints
 * 
 * Integrates with the new AnalyticsController.cs backend
 * Base URL: /api/analytics
 */
export const analyticsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Token Usage Analytics
    getTokenUsageStatistics: builder.query<TokenUsageStatistics, TokenUsageFilters>({
      query: (params) => ({
        url: '/analytics/token-usage',
        params,
      }),
      providesTags: ['TokenUsage'],
    }),

    getDailyTokenUsage: builder.query<DailyTokenUsage[], TokenUsageFilters>({
      query: (params) => ({
        url: '/analytics/token-usage/daily',
        params,
      }),
      providesTags: ['TokenUsage'],
    }),

    getTokenUsageTrends: builder.query<Array<{
      date: string
      totalTokens: number
      totalCost: number
      averageTokensPerRequest: number
      requestCount: number
    }>, TokenUsageFilters>({
      query: (params) => ({
        url: '/analytics/token-usage/trends',
        params,
      }),
      providesTags: ['TokenUsage'],
    }),

    getTopTokenUsers: builder.query<Array<{
      userId: string
      userName?: string
      totalTokens: number
      totalCost: number
      requestCount: number
      averageTokensPerRequest: number
    }>, { limit?: number; days?: number }>({
      query: (params) => ({
        url: '/analytics/token-usage/top-users',
        params,
      }),
      providesTags: ['TokenUsage'],
    }),

    // ProcessFlow Session Analytics
    getSessionAnalytics: builder.query<ProcessFlowSessionAnalytics, ProcessFlowAnalyticsRequest>({
      query: (params) => ({
        url: '/analytics/sessions',
        params,
      }),
      providesTags: ['SessionAnalytics'],
    }),

    getStepAnalytics: builder.query<ProcessFlowStepAnalytics[], {
      stepType?: string
      startDate?: string
      endDate?: string
    }>({
      query: (params) => ({
        url: '/analytics/steps',
        params,
      }),
      providesTags: ['StepAnalytics'],
    }),

    // Query Analytics
    getQueryAnalytics: builder.query<EnhancedQueryAnalytics, {
      startDate?: string
      endDate?: string
      queryType?: string
      intentType?: string
    }>({
      query: (params) => ({
        url: '/analytics/queries',
        params,
      }),
      providesTags: ['QueryAnalytics'],
    }),

    // Transparency Metrics
    getTransparencyAnalytics: builder.query<ProcessFlowTransparencyMetrics, {
      startDate?: string
      endDate?: string
      model?: string
      includeDetails?: boolean
    }>({
      query: (params) => ({
        url: '/analytics/transparency',
        params,
      }),
      providesTags: ['TransparencyAnalytics'],
    }),

    // Real-time Metrics
    getRealTimeMetrics: builder.query<ProcessFlowRealTimeMetrics, void>({
      query: () => '/analytics/realtime',
      providesTags: ['RealTimeMetrics'],
    }),

    // Comprehensive Analytics
    getComprehensiveAnalytics: builder.query<ProcessFlowAnalyticsResponse, ProcessFlowAnalyticsRequest>({
      query: (params) => ({
        url: '/analytics/comprehensive',
        params,
      }),
      providesTags: ['ComprehensiveAnalytics'],
    }),

    // Dashboard Data
    getAnalyticsDashboard: builder.query<ProcessFlowDashboardData, {
      startDate?: string
      endDate?: string
      includeRealTime?: boolean
    }>({
      query: (params) => ({
        url: '/analytics/dashboard',
        params,
      }),
      providesTags: ['AnalyticsDashboard'],
    }),

    // Export Analytics
    exportAnalyticsData: builder.mutation<Blob, ProcessFlowAnalyticsExportConfig>({
      query: (config) => ({
        url: '/analytics/export',
        method: 'POST',
        body: config,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Performance Analytics
    getPerformanceAnalytics: builder.query<{
      averageSessionDuration: number
      averageStepDuration: number
      successRate: number
      errorRate: number
      throughput: number
      performanceTrends: Array<{
        date: string
        averageDuration: number
        successRate: number
        throughput: number
      }>
    }, { days?: number; granularity?: 'hour' | 'day' | 'week' }>({
      query: (params) => ({
        url: '/analytics/performance',
        params,
      }),
      providesTags: ['PerformanceAnalytics'],
    }),

    // Cost Analytics
    getCostAnalytics: builder.query<{
      totalCost: number
      costByModel: Record<string, number>
      costTrends: Array<{
        date: string
        cost: number
        tokens: number
        sessions: number
      }>
      projectedMonthlyCost: number
      costOptimizationSuggestions: Array<{
        type: string
        description: string
        potentialSavings: number
      }>
    }, { startDate?: string; endDate?: string }>({
      query: (params) => ({
        url: '/analytics/cost',
        params,
      }),
      providesTags: ['CostAnalytics'],
    }),

    // Error Analytics
    getErrorAnalytics: builder.query<{
      totalErrors: number
      errorRate: number
      errorsByType: Record<string, number>
      errorsByStep: Record<string, number>
      errorTrends: Array<{
        date: string
        errorCount: number
        errorRate: number
      }>
      topErrors: Array<{
        error: string
        count: number
        percentage: number
        lastOccurrence: string
      }>
    }, { days?: number; stepType?: string }>({
      query: (params) => ({
        url: '/analytics/errors',
        params,
      }),
      providesTags: ['ErrorAnalytics'],
    }),
  }),
  overrideExisting: false,
})

// Export hooks for use in components
export const {
  useGetTokenUsageStatisticsQuery,
  useGetDailyTokenUsageQuery,
  useGetTokenUsageTrendsQuery,
  useGetTopTokenUsersQuery,
  useGetSessionAnalyticsQuery,
  useGetStepAnalyticsQuery,
  useGetQueryAnalyticsQuery,
  useGetTransparencyAnalyticsQuery,
  useGetRealTimeMetricsQuery,
  useGetComprehensiveAnalyticsQuery,
  useGetAnalyticsDashboardQuery,
  useExportAnalyticsDataMutation,
  useGetPerformanceAnalyticsQuery,
  useGetCostAnalyticsQuery,
  useGetErrorAnalyticsQuery,
} = analyticsApi

// Enhanced hooks for common use cases
export const useTokenUsageDashboard = (filters: TokenUsageFilters = {}) => {
  const statisticsQuery = useGetTokenUsageStatisticsQuery(filters)
  const dailyUsageQuery = useGetDailyTokenUsageQuery(filters)
  const trendsQuery = useGetTokenUsageTrendsQuery(filters)
  const topUsersQuery = useGetTopTokenUsersQuery({ limit: 10, days: 30 })

  return {
    statistics: statisticsQuery.data,
    dailyUsage: dailyUsageQuery.data,
    trends: trendsQuery.data,
    topUsers: topUsersQuery.data,
    isLoading: statisticsQuery.isLoading || dailyUsageQuery.isLoading || 
               trendsQuery.isLoading || topUsersQuery.isLoading,
    error: statisticsQuery.error || dailyUsageQuery.error || 
           trendsQuery.error || topUsersQuery.error,
    refetch: () => {
      statisticsQuery.refetch()
      dailyUsageQuery.refetch()
      trendsQuery.refetch()
      topUsersQuery.refetch()
    }
  }
}

export const useAnalyticsDashboard = (filters: {
  startDate?: string
  endDate?: string
  includeRealTime?: boolean
} = {}) => {
  const dashboardQuery = useGetAnalyticsDashboardQuery(filters)
  const realTimeQuery = useGetRealTimeMetricsQuery(undefined, {
    pollingInterval: filters.includeRealTime ? 5000 : 0,
    skip: !filters.includeRealTime,
  })

  return {
    dashboard: dashboardQuery.data,
    realTime: realTimeQuery.data,
    isLoading: dashboardQuery.isLoading || (filters.includeRealTime && realTimeQuery.isLoading),
    error: dashboardQuery.error || realTimeQuery.error,
    refetch: () => {
      dashboardQuery.refetch()
      if (filters.includeRealTime) {
        realTimeQuery.refetch()
      }
    }
  }
}

export const usePerformanceOverview = (days = 7) => {
  const performanceQuery = useGetPerformanceAnalyticsQuery({ days })
  const costQuery = useGetCostAnalyticsQuery({ 
    startDate: new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString(),
    endDate: new Date().toISOString()
  })
  const errorQuery = useGetErrorAnalyticsQuery({ days })

  return {
    performance: performanceQuery.data,
    cost: costQuery.data,
    errors: errorQuery.data,
    isLoading: performanceQuery.isLoading || costQuery.isLoading || errorQuery.isLoading,
    error: performanceQuery.error || costQuery.error || errorQuery.error,
    refetch: () => {
      performanceQuery.refetch()
      costQuery.refetch()
      errorQuery.refetch()
    }
  }
}

export default analyticsApi
