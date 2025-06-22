import { baseApi } from './baseApi'

// Types based on the database schema and controller
export interface LLMProviderConfig {
  providerId: string
  name: string
  type: 'openai' | 'anthropic' | 'azure' | 'google' | 'local'
  apiKey?: string
  endpoint?: string
  organization?: string
  isEnabled: boolean
  isDefault: boolean
  settings?: string // JSON string
  createdAt: string
  updatedAt: string
}

export interface LLMModelConfig {
  modelId: string
  providerId: string
  name: string
  displayName: string
  temperature: number
  maxTokens: number
  topP: number
  frequencyPenalty: number
  presencePenalty: number
  isEnabled: boolean
  useCase?: string
  costPerToken: number
  capabilities?: string // JSON string
}

export interface LLMProviderStatus {
  providerId: string
  isHealthy: boolean
  status: 'healthy' | 'degraded' | 'unhealthy' | 'offline'
  responseTime?: number
  lastChecked: string
  errorMessage?: string
  metadata?: Record<string, any>
}

export interface LLMUsageLog {
  id: number
  requestId: string
  userId: string
  providerId: string
  modelId: string
  requestType: string
  requestText: string
  responseText: string
  inputTokens: number
  outputTokens: number
  totalTokens: number
  cost: number
  durationMs: number
  success: boolean
  errorMessage?: string
  timestamp: string
  metadata?: string
}

export interface LLMUsageAnalytics {
  totalRequests: number
  totalTokens: number
  totalCost: number
  averageResponseTime: number
  successRate: number
  requestsByProvider: Record<string, number>
  costsByProvider: Record<string, number>
  tokensByProvider: Record<string, number>
  requestsByModel: Record<string, number>
  dailyUsage: Array<{
    date: string
    requests: number
    tokens: number
    cost: number
  }>
}

export interface LLMCostSummary {
  providerId: string
  providerName: string
  totalCost: number
  totalRequests: number
  totalTokens: number
  averageCostPerRequest: number
  averageCostPerToken: number
  period: {
    startDate: string
    endDate: string
  }
}

export interface CostAlert {
  id: number
  providerId: string
  alertType: 'threshold' | 'budget' | 'anomaly'
  message: string
  threshold: number
  currentValue: number
  isEnabled: boolean
  createdAt: string
}

export interface ProviderPerformanceMetrics {
  providerId: string
  averageResponseTime: number
  successRate: number
  totalRequests: number
  errorCount: number
  errorDistribution: Record<string, number>
  throughput: number
  availability: number
}

export interface ErrorAnalysis {
  providerId: string
  totalErrors: number
  errorTypes: Record<string, number>
  errorTrends: Array<{
    date: string
    count: number
    type: string
  }>
  commonErrors: Array<{
    message: string
    count: number
    lastOccurred: string
  }>
}

export interface DashboardSummary {
  providers: {
    total: number
    enabled: number
    healthy: number
  }
  usage: {
    totalRequests: number
    totalTokens: number
    averageResponseTime: number
    successRate: number
  }
  costs: {
    currentMonth: number
    total30Days: number
    activeAlerts: number
  }
  performance: {
    averageResponseTime: number
    overallSuccessRate: number
    totalErrors: number
  }
  lastUpdated: string
}

/**
 * LLM Management API - RTK Query service for LLM provider and model management
 * 
 * Integrates with LLMManagementController.cs backend
 * Base URL: /api/llmmanagement
 */
export const llmManagementApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Provider Management
    getProviders: builder.query<LLMProviderConfig[], void>({
      query: () => '/llmmanagement/providers',
      providesTags: ['LLMProvider'],
    }),

    getProvider: builder.query<LLMProviderConfig, string>({
      query: (providerId) => `/llmmanagement/providers/${providerId}`,
      providesTags: (result, error, providerId) => [{ type: 'LLMProvider', id: providerId }],
    }),

    saveProvider: builder.mutation<LLMProviderConfig, LLMProviderConfig>({
      query: (provider) => ({
        url: '/llmmanagement/providers',
        method: 'POST',
        body: provider,
      }),
      invalidatesTags: ['LLMProvider'],
    }),

    deleteProvider: builder.mutation<void, string>({
      query: (providerId) => ({
        url: `/llmmanagement/providers/${providerId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['LLMProvider'],
    }),

    testProvider: builder.mutation<LLMProviderStatus, string>({
      query: (providerId) => ({
        url: `/llmmanagement/providers/${providerId}/test`,
        method: 'POST',
      }),
    }),

    getProviderHealth: builder.query<LLMProviderStatus[], void>({
      query: () => '/llmmanagement/providers/health',
      providesTags: ['LLMProviderHealth'],
    }),

    // Model Management
    getModels: builder.query<LLMModelConfig[], { providerId?: string }>({
      query: ({ providerId }) => ({
        url: '/llmmanagement/models',
        params: providerId ? { providerId } : {},
      }),
      providesTags: ['LLMModel'],
    }),

    getModel: builder.query<LLMModelConfig, string>({
      query: (modelId) => `/llmmanagement/models/${modelId}`,
      providesTags: (result, error, modelId) => [{ type: 'LLMModel', id: modelId }],
    }),

    saveModel: builder.mutation<LLMModelConfig, LLMModelConfig>({
      query: (model) => ({
        url: '/llmmanagement/models',
        method: 'POST',
        body: model,
      }),
      invalidatesTags: ['LLMModel'],
    }),

    deleteModel: builder.mutation<void, string>({
      query: (modelId) => ({
        url: `/llmmanagement/models/${modelId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['LLMModel'],
    }),

    getDefaultModel: builder.query<LLMModelConfig, string>({
      query: (useCase) => `/llmmanagement/models/default/${useCase}`,
      providesTags: ['LLMModel'],
    }),

    setDefaultModel: builder.mutation<{ message: string }, { modelId: string; useCase: string }>({
      query: ({ modelId, useCase }) => ({
        url: `/llmmanagement/models/${modelId}/set-default/${useCase}`,
        method: 'POST',
      }),
      invalidatesTags: ['LLMModel'],
    }),

    // Usage Tracking
    getUsageHistory: builder.query<LLMUsageLog[], {
      startDate?: string
      endDate?: string
      providerId?: string
      modelId?: string
      userId?: string
      requestType?: string
      skip?: number
      take?: number
    }>({
      query: (params) => ({
        url: '/llmmanagement/usage/history',
        params,
      }),
      providesTags: ['LLMUsage'],
    }),

    getUsageAnalytics: builder.query<LLMUsageAnalytics, {
      startDate: string
      endDate: string
      providerId?: string
      modelId?: string
    }>({
      query: (params) => ({
        url: '/llmmanagement/usage/analytics',
        params,
      }),
      providesTags: ['LLMUsage'],
    }),

    exportUsageData: builder.mutation<Blob, {
      startDate: string
      endDate: string
      format?: 'csv' | 'json'
    }>({
      query: ({ startDate, endDate, format = 'csv' }) => ({
        url: '/llmmanagement/usage/export',
        params: { startDate, endDate, format },
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Cost Management
    getCostSummary: builder.query<LLMCostSummary[], {
      startDate: string
      endDate: string
      providerId?: string
    }>({
      query: (params) => ({
        url: '/llmmanagement/costs/summary',
        params,
      }),
      providesTags: ['LLMCost'],
    }),

    getCurrentMonthCost: builder.query<number, { providerId?: string }>({
      query: ({ providerId }) => ({
        url: '/llmmanagement/costs/current-month',
        params: providerId ? { providerId } : {},
      }),
      providesTags: ['LLMCost'],
    }),

    getCostProjections: builder.query<Record<string, number>, void>({
      query: () => '/llmmanagement/costs/projections',
      providesTags: ['LLMCost'],
    }),

    setCostLimit: builder.mutation<{ message: string }, { providerId: string; monthlyLimit: number }>({
      query: ({ providerId, monthlyLimit }) => ({
        url: `/llmmanagement/costs/limits/${providerId}`,
        method: 'POST',
        body: monthlyLimit,
      }),
      invalidatesTags: ['LLMCost'],
    }),

    getCostAlerts: builder.query<CostAlert[], void>({
      query: () => '/llmmanagement/costs/alerts',
      providesTags: ['LLMCost'],
    }),

    // Performance Monitoring
    getPerformanceMetrics: builder.query<Record<string, ProviderPerformanceMetrics>, {
      startDate: string
      endDate: string
    }>({
      query: (params) => ({
        url: '/llmmanagement/performance/metrics',
        params,
      }),
      providesTags: ['LLMPerformance'],
    }),

    getCacheHitRates: builder.query<Record<string, number>, void>({
      query: () => '/llmmanagement/performance/cache-hit-rates',
      providesTags: ['LLMPerformance'],
    }),

    getErrorAnalysis: builder.query<Record<string, ErrorAnalysis>, {
      startDate: string
      endDate: string
    }>({
      query: (params) => ({
        url: '/llmmanagement/performance/error-analysis',
        params,
      }),
      providesTags: ['LLMPerformance'],
    }),

    // Dashboard Summary
    getDashboardSummary: builder.query<DashboardSummary, void>({
      query: () => '/llmmanagement/dashboard/summary',
      providesTags: ['LLMDashboard'],
    }),
  }),
})

export const {
  // Provider Management
  useGetProvidersQuery,
  useGetProviderQuery,
  useSaveProviderMutation,
  useDeleteProviderMutation,
  useTestProviderMutation,
  useGetProviderHealthQuery,
  
  // Model Management
  useGetModelsQuery,
  useGetModelQuery,
  useSaveModelMutation,
  useDeleteModelMutation,
  useGetDefaultModelQuery,
  useSetDefaultModelMutation,
  
  // Usage Tracking
  useGetUsageHistoryQuery,
  useGetUsageAnalyticsQuery,
  useExportUsageDataMutation,
  
  // Cost Management
  useGetCostSummaryQuery,
  useGetCurrentMonthCostQuery,
  useGetCostProjectionsQuery,
  useSetCostLimitMutation,
  useGetCostAlertsQuery,
  
  // Performance Monitoring
  useGetPerformanceMetricsQuery,
  useGetCacheHitRatesQuery,
  useGetErrorAnalysisQuery,
  
  // Dashboard Summary
  useGetDashboardSummaryQuery,
} = llmManagementApi
