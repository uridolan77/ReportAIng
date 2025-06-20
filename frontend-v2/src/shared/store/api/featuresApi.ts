import { baseApi } from './baseApi'

export interface StartStreamingRequest {
  query: string
  sessionId?: string
  options?: {
    enableProgress: boolean
    enableSemanticAnalysis: boolean
    maxExecutionTime: number
  }
}

export interface StreamingSession {
  sessionId: string
  status: 'active' | 'completed' | 'error' | 'cancelled'
  startTime: string
  endTime?: string
  query?: string
}

export interface RealTimeDashboardData {
  activeQueries: number
  totalQueriesToday: number
  averageResponseTime: number
  systemLoad: number
  connectedUsers: number
  recentActivity: Array<{
    timestamp: string
    user: string
    action: string
    query?: string
  }>
}

export interface LiveChartData {
  id: string
  title: string
  type: 'line' | 'bar' | 'pie' | 'gauge'
  data: any[]
  lastUpdated: string
  refreshInterval: number
  isRealTime: boolean
}

export interface UserDashboardData {
  recentQueries: Array<{
    id: string
    query: string
    timestamp: string
    executionTime: number
    status: string
  }>
  favoriteQueries: Array<{
    id: string
    title: string
    query: string
    lastUsed: string
  }>
  quickStats: {
    totalQueries: number
    avgExecutionTime: number
    successRate: number
    dataExplored: string
  }
  suggestedQueries: Array<{
    title: string
    description: string
    query: string
    category: string
  }>
}

export interface SystemStatistics {
  queryMetrics: {
    totalQueries: number
    successfulQueries: number
    failedQueries: number
    averageExecutionTime: number
    peakConcurrentQueries: number
  }
  systemHealth: {
    cpuUsage: number
    memoryUsage: number
    diskUsage: number
    databaseConnections: number
    uptime: string
  }
  userActivity: {
    activeUsers: number
    totalUsers: number
    newUsersToday: number
    queriesPerUser: number
  }
  dataMetrics: {
    tablesQueried: number
    rowsProcessed: number
    dataTransferred: string
    cacheHitRate: number
  }
}

export interface QueryAnalytics {
  period: string
  queryTrends: Array<{
    date: string
    count: number
    avgExecutionTime: number
  }>
  popularTables: Array<{
    tableName: string
    queryCount: number
    avgExecutionTime: number
  }>
  userActivity: Array<{
    userId: string
    userName: string
    queryCount: number
    lastActive: string
  }>
  errorAnalysis: Array<{
    errorType: string
    count: number
    percentage: number
  }>
}

export interface PerformanceMetrics {
  timeRange: string
  responseTimeMetrics: {
    p50: number
    p90: number
    p95: number
    p99: number
    average: number
  }
  throughputMetrics: {
    queriesPerSecond: number
    peakQPS: number
    totalQueries: number
  }
  resourceUtilization: {
    cpuUsage: Array<{ timestamp: string; value: number }>
    memoryUsage: Array<{ timestamp: string; value: number }>
    diskIO: Array<{ timestamp: string; read: number; write: number }>
  }
  errorRates: {
    totalErrors: number
    errorRate: number
    errorsByType: Array<{ type: string; count: number }>
  }
}

export const featuresApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Streaming Session Management
    startStreamingSession: builder.mutation<StreamingSession, StartStreamingRequest>({
      query: (body) => ({
        url: '/features/streaming/sessions/start',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['StreamingSession'],
    }),

    stopStreamingSession: builder.mutation<{ success: boolean }, string>({
      query: (sessionId) => ({
        url: `/features/streaming/sessions/${sessionId}/stop`,
        method: 'POST',
      }),
      invalidatesTags: ['StreamingSession'],
    }),

    getActiveStreamingSessions: builder.query<StreamingSession[], void>({
      query: () => '/features/streaming/sessions/active',
      providesTags: ['StreamingSession'],
    }),

    // Real-time Dashboard
    getRealTimeDashboard: builder.query<RealTimeDashboardData, void>({
      query: () => '/features/streaming/dashboard',
      // Refresh every 5 seconds for real-time data
      pollingInterval: 5000,
    }),

    getLiveCharts: builder.query<LiveChartData[], void>({
      query: () => '/features/streaming/charts',
      // Refresh every 10 seconds
      pollingInterval: 10000,
    }),

    // Dashboard Data
    getUserDashboard: builder.query<UserDashboardData, void>({
      query: () => '/dashboard/user',
      providesTags: ['Dashboard'],
    }),

    getSystemStatistics: builder.query<SystemStatistics, { days?: number }>({
      query: ({ days = 30 }) => `/dashboard/system-stats?days=${days}`,
      providesTags: ['Dashboard'],
    }),

    getQueryAnalytics: builder.query<QueryAnalytics, { period?: string }>({
      query: ({ period = 'week' }) => `/dashboard/analytics?period=${period}`,
      providesTags: ['Dashboard'],
    }),

    getPerformanceMetrics: builder.query<PerformanceMetrics, { days?: number }>({
      query: ({ days = 7 }) => `/dashboard/performance?days=${days}`,
      providesTags: ['Dashboard'],
    }),

    // Real-time Notifications
    subscribeToNotifications: builder.query<any, { userId: string }>({
      query: ({ userId }) => `/features/notifications/subscribe?userId=${userId}`,
      // This would typically be handled by WebSocket, but included for completeness
    }),

    // Live Query Monitoring
    getActiveQueries: builder.query<Array<{
      sessionId: string
      query: string
      user: string
      startTime: string
      estimatedCompletion: string
      progress: number
    }>, void>({
      query: () => '/features/streaming/queries/active',
      pollingInterval: 2000, // Refresh every 2 seconds
    }),

    // System Health Monitoring
    getSystemHealth: builder.query<{
      status: 'healthy' | 'warning' | 'critical'
      services: Array<{
        name: string
        status: 'up' | 'down' | 'degraded'
        responseTime: number
        lastCheck: string
      }>
      alerts: Array<{
        level: 'info' | 'warning' | 'error'
        message: string
        timestamp: string
      }>
    }, void>({
      query: () => '/features/monitoring/health',
      pollingInterval: 30000, // Refresh every 30 seconds
    }),
  }),
})

export const {
  // Streaming
  useStartStreamingSessionMutation,
  useStopStreamingSessionMutation,
  useGetActiveStreamingSessionsQuery,
  
  // Real-time Dashboard
  useGetRealTimeDashboardQuery,
  useGetLiveChartsQuery,
  
  // Dashboard
  useGetUserDashboardQuery,
  useGetSystemStatisticsQuery,
  useGetQueryAnalyticsQuery,
  useGetPerformanceMetricsQuery,
  
  // Monitoring
  useSubscribeToNotificationsQuery,
  useGetActiveQueriesQuery,
  useGetSystemHealthQuery,
} = featuresApi
