import { baseApi } from './baseApi'
import type {
  AgentOrchestrationResult,
  OrchestrationRequest,
  AgentCapabilities,
  SchemaNavigationResult,
  SchemaNavigationRequest,
  QueryUnderstandingResult,
  QueryUnderstandingRequest,
  AgentCommunicationLog,
  AgentPerformanceMetrics,
  AgentHealthStatus
} from '@shared/types/intelligentAgents'

/**
 * Intelligent Agents API - RTK Query service for AI agent orchestration
 * 
 * Integrates with the new IntelligentAgentsController.cs backend
 * Base URL: /api/intelligentagents
 */
export const intelligentAgentsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Orchestrate multi-agent tasks
    orchestrateTasks: builder.mutation<AgentOrchestrationResult, OrchestrationRequest>({
      query: (body) => ({
        url: '/intelligentagents/orchestrate',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['AgentOrchestration'],
    }),

    // Get agent capabilities
    getAgentCapabilities: builder.query<AgentCapabilities[], void>({
      query: () => '/intelligentagents/capabilities',
      providesTags: ['AgentCapabilities'],
    }),

    // Navigate database schemas
    navigateSchema: builder.mutation<SchemaNavigationResult, SchemaNavigationRequest>({
      query: (body) => ({
        url: '/intelligentagents/schema/navigate',
        method: 'POST',
        body,
      }),
    }),

    // Understand natural language queries
    understandQuery: builder.mutation<QueryUnderstandingResult, QueryUnderstandingRequest>({
      query: (body) => ({
        url: '/intelligentagents/query/understand',
        method: 'POST',
        body,
      }),
    }),

    // Get agent communication logs
    getCommunicationLogs: builder.query<AgentCommunicationLog[], { 
      agentId?: string
      limit?: number
      startDate?: string
      endDate?: string
    }>({
      query: ({ agentId, limit = 50, startDate, endDate }) => ({
        url: '/intelligentagents/communication/logs',
        params: { agentId, limit, startDate, endDate },
      }),
      providesTags: ['AgentCommunication'],
    }),

    // Get agent performance metrics
    getAgentPerformanceMetrics: builder.query<AgentPerformanceMetrics[], {
      agentId?: string
      timeRange?: 'hour' | 'day' | 'week' | 'month'
    }>({
      query: ({ agentId, timeRange = 'day' }) => ({
        url: '/intelligentagents/performance',
        params: { agentId, timeRange },
      }),
      providesTags: ['AgentPerformance'],
    }),

    // Get agent health status
    getAgentHealthStatus: builder.query<AgentHealthStatus[], void>({
      query: () => '/intelligentagents/health',
      providesTags: ['AgentHealth'],
    }),

    // Get orchestration history
    getOrchestrationHistory: builder.query<AgentOrchestrationResult[], {
      userId?: string
      limit?: number
      status?: 'pending' | 'running' | 'completed' | 'failed'
    }>({
      query: ({ userId, limit = 20, status }) => ({
        url: '/intelligentagents/orchestration/history',
        params: { userId, limit, status },
      }),
      providesTags: ['OrchestrationHistory'],
    }),

    // Cancel orchestration
    cancelOrchestration: builder.mutation<void, string>({
      query: (orchestrationId) => ({
        url: `/intelligentagents/orchestration/${orchestrationId}/cancel`,
        method: 'POST',
      }),
      invalidatesTags: ['AgentOrchestration', 'OrchestrationHistory'],
    }),

    // Get agent analytics
    getAgentAnalytics: builder.query<{
      totalTasks: number
      successRate: number
      averageResponseTime: number
      agentUtilization: Array<{ agentId: string; utilization: number }>
      taskDistribution: Array<{ taskType: string; count: number }>
      performanceTrends: Array<{ date: string; successRate: number; avgTime: number }>
    }, { days?: number }>({
      query: ({ days = 7 }) => ({
        url: '/intelligentagents/analytics',
        params: { days },
      }),
      providesTags: ['AgentAnalytics'],
    }),

    // Configure agent settings
    updateAgentSettings: builder.mutation<void, {
      agentId: string
      settings: {
        enabled?: boolean
        maxConcurrentTasks?: number
        timeout?: number
        retryAttempts?: number
        priority?: number
      }
    }>({
      query: ({ agentId, settings }) => ({
        url: `/intelligentagents/${agentId}/settings`,
        method: 'PUT',
        body: settings,
      }),
      invalidatesTags: ['AgentCapabilities', 'AgentHealth'],
    }),

    // Schema Navigation and Intelligence
    getSchemaNavigation: builder.query<any, { query?: string }>({
      query: ({ query }) => ({
        url: '/intelligentagents/schema/navigation',
        params: { query }
      }),
      providesTags: ['SchemaNavigation']
    }),

    getSchemaRecommendations: builder.query<any, { query?: string; context?: string }>({
      query: ({ query, context }) => ({
        url: '/intelligentagents/schema/recommendations',
        params: { query, context }
      }),
      providesTags: ['SchemaRecommendations']
    }),

    // Query Optimization
    getQueryOptimization: builder.query<any, { query: string }>({
      query: ({ query }) => ({
        url: '/intelligentagents/query/optimization',
        method: 'POST',
        body: { query }
      }),
      providesTags: ['QueryOptimization']
    }),

    applyOptimization: builder.mutation<any, { queryId: string; optimizationId: string }>({
      query: ({ queryId, optimizationId }) => ({
        url: '/intelligentagents/query/optimization/apply',
        method: 'POST',
        body: { queryId, optimizationId }
      }),
      invalidatesTags: ['QueryOptimization']
    }),

    // Automated Insights
    getAutomatedInsights: builder.query<any, { context?: string; categories?: string[]; limit?: number }>({
      query: ({ context, categories, limit = 20 }) => ({
        url: '/intelligentagents/insights/automated',
        params: { context, categories: categories?.join(','), limit }
      }),
      providesTags: ['AutomatedInsights']
    }),

    generateInsight: builder.mutation<any, { context?: string; category?: string }>({
      query: ({ context, category }) => ({
        url: '/intelligentagents/insights/generate',
        method: 'POST',
        body: { context, category }
      }),
      invalidatesTags: ['AutomatedInsights']
    }),

    // Predictive Analytics
    getPredictiveAnalytics: builder.query<any, { dataSource?: string; metric?: string; forecastDays?: number; model?: string }>({
      query: ({ dataSource, metric, forecastDays = 30, model }) => ({
        url: '/intelligentagents/analytics/predictive',
        params: { dataSource, metric, forecastDays, model }
      }),
      providesTags: ['PredictiveAnalytics']
    }),

    generateForecast: builder.mutation<any, { dataSource?: string; metric?: string; forecastDays?: number; model?: string }>({
      query: ({ dataSource, metric, forecastDays = 30, model }) => ({
        url: '/intelligentagents/analytics/forecast',
        method: 'POST',
        body: { dataSource, metric, forecastDays, model }
      }),
      invalidatesTags: ['PredictiveAnalytics']
    }),
  }),
  overrideExisting: false,
})

// Export RTK Query hooks
export const {
  useOrchestrateTasksMutation,
  useGetAgentCapabilitiesQuery,
  useNavigateSchemaMutation,
  useUnderstandQueryMutation,
  useGetCommunicationLogsQuery,
  useGetAgentPerformanceMetricsQuery,
  useGetAgentHealthStatusQuery,
  useGetOrchestrationHistoryQuery,
  useCancelOrchestrationMutation,
  useGetAgentAnalyticsQuery,
  useUpdateAgentSettingsMutation,
  useGetSchemaNavigationQuery,
  useGetSchemaRecommendationsQuery,
  useGetQueryOptimizationQuery,
  useApplyOptimizationMutation,
  useGetAutomatedInsightsQuery,
  useGenerateInsightMutation,
  useGetPredictiveAnalyticsQuery,
  useGenerateForecastMutation,
} = intelligentAgentsApi

/**
 * Enhanced hook for schema navigation with caching and error handling
 */
export const useSchemaNavigation = () => {
  const [navigateSchema, { isLoading, error }] = useNavigateSchemaMutation()
  
  const navigate = async (request: SchemaNavigationRequest) => {
    try {
      const result = await navigateSchema(request).unwrap()
      return result
    } catch (err) {
      console.error('Schema navigation failed:', err)
      throw err
    }
  }

  return {
    navigate,
    isLoading,
    error
  }
}

/**
 * Enhanced hook for query understanding with confidence tracking
 */
export const useQueryUnderstanding = () => {
  const [understandQuery, { isLoading, error }] = useUnderstandQueryMutation()
  
  const understand = async (request: QueryUnderstandingRequest) => {
    try {
      const result = await understandQuery(request).unwrap()
      return result
    } catch (err) {
      console.error('Query understanding failed:', err)
      throw err
    }
  }

  return {
    understand,
    isLoading,
    error
  }
}

/**
 * Enhanced hook for agent orchestration with real-time updates
 */
export const useAgentOrchestration = () => {
  const [orchestrate, { isLoading, error }] = useOrchestrateTasksMutation()
  const [cancel] = useCancelOrchestrationMutation()
  
  const startOrchestration = async (request: OrchestrationRequest) => {
    try {
      const result = await orchestrate(request).unwrap()
      return result
    } catch (err) {
      console.error('Orchestration failed:', err)
      throw err
    }
  }

  const cancelOrchestration = async (orchestrationId: string) => {
    try {
      await cancel(orchestrationId).unwrap()
    } catch (err) {
      console.error('Failed to cancel orchestration:', err)
      throw err
    }
  }

  return {
    startOrchestration,
    cancelOrchestration,
    isLoading,
    error
  }
}

/**
 * Hook for agent dashboard data
 */
export const useAgentDashboard = (days = 7) => {
  const { data: capabilities, isLoading: capabilitiesLoading } = useGetAgentCapabilitiesQuery()
  const { data: health, isLoading: healthLoading } = useGetAgentHealthStatusQuery()
  const { data: analytics, isLoading: analyticsLoading } = useGetAgentAnalyticsQuery({ days })
  const { data: history, isLoading: historyLoading } = useGetOrchestrationHistoryQuery({ limit: 10 })

  return {
    capabilities,
    health,
    analytics,
    history,
    isLoading: capabilitiesLoading || healthLoading || analyticsLoading || historyLoading,
    refetch: () => {
      // Trigger refetch for all queries
    }
  }
}

/**
 * Hook for real-time agent monitoring
 */
export const useAgentMonitoring = (agentId?: string) => {
  const { data: performance } = useGetAgentPerformanceMetricsQuery({ 
    agentId, 
    timeRange: 'hour' 
  }, {
    pollingInterval: 30000, // Poll every 30 seconds
  })
  
  const { data: logs } = useGetCommunicationLogsQuery({ 
    agentId, 
    limit: 20 
  }, {
    pollingInterval: 10000, // Poll every 10 seconds
  })

  return {
    performance,
    logs,
    isMonitoring: true
  }
}

export default intelligentAgentsApi
