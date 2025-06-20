import { baseApi } from './baseApi'

export interface SystemStatistics {
  totalUsers: number
  activeUsers: number
  totalQueries: number
  queriesThisWeek: number
  averageQueryTime: number
  systemUptime: number
  memoryUsage: number
  cpuUsage: number
}

export interface QueryAnalytics {
  period: 'day' | 'week' | 'month'
  totalQueries: number
  successfulQueries: number
  failedQueries: number
  averageExecutionTime: number
  topTables: Array<{ tableName: string; queryCount: number }>
  queryComplexityDistribution: Record<string, number>
  userActivityDistribution: Array<{ userId: string; queryCount: number }>
}

export interface PerformanceMetrics {
  responseTime: {
    p50: number
    p95: number
    p99: number
  }
  throughput: number
  errorRate: number
  memoryUsage: number
  cpuUsage: number
  databaseConnections: number
}

export interface SystemConfiguration {
  applicationName: string
  version: string
  environment: string
  features: {
    semanticLayerEnabled: boolean
    realTimeStreamingEnabled: boolean
    mfaEnabled: boolean
    auditLoggingEnabled: boolean
  }
  limits: {
    maxQueryExecutionTime: number
    maxResultRows: number
    maxConcurrentQueries: number
    rateLimitPerUser: number
  }
  ai: {
    defaultProvider: string
    maxTokensPerQuery: number
    semanticThreshold: number
  }
}

export interface AIProviderConfig {
  id: string
  name: string
  type: 'OpenAI' | 'Azure' | 'Anthropic'
  endpoint: string
  model: string
  maxTokens: number
  temperature: number
  isActive: boolean
  priority: number
}

export interface SecuritySettings {
  passwordPolicy: {
    minLength: number
    requireUppercase: boolean
    requireLowercase: boolean
    requireNumbers: boolean
    requireSpecialChars: boolean
  }
  sessionTimeout: number
  maxLoginAttempts: number
  lockoutDuration: number
  enableTwoFactorAuth: boolean
  allowedDomains: string[]
}

export const adminApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Dashboard & Analytics
    getSystemStatistics: builder.query<SystemStatistics, { days?: number }>({
      query: ({ days = 30 }) => `/dashboard/system-stats?days=${days}`,
      providesTags: ['Analytics'],
    }),
    
    getQueryAnalytics: builder.query<QueryAnalytics, { period?: 'day' | 'week' | 'month' }>({
      query: ({ period = 'week' }) => `/dashboard/analytics?period=${period}`,
      providesTags: ['Analytics'],
    }),
    
    getPerformanceMetrics: builder.query<PerformanceMetrics, { days?: number }>({
      query: ({ days = 7 }) => `/dashboard/performance?days=${days}`,
      providesTags: ['Analytics'],
    }),
    
    // System Configuration
    getSystemConfiguration: builder.query<SystemConfiguration, void>({
      query: () => '/configuration/system',
      providesTags: ['SystemConfig'],
    }),
    
    updateSystemConfiguration: builder.mutation<{ success: boolean }, Partial<SystemConfiguration>>({
      query: (body) => ({
        url: '/configuration/system',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['SystemConfig'],
    }),
    
    // AI Provider Settings
    getAIProviders: builder.query<AIProviderConfig[], void>({
      query: () => '/configuration/ai-providers',
      providesTags: ['SystemConfig'],
    }),
    
    updateAIProvider: builder.mutation<{ success: boolean }, { providerId: string; config: Partial<AIProviderConfig> }>({
      query: ({ providerId, config }) => ({
        url: `/configuration/ai-providers/${providerId}`,
        method: 'PUT',
        body: config,
      }),
      invalidatesTags: ['SystemConfig'],
    }),
    
    // Security Settings
    getSecuritySettings: builder.query<SecuritySettings, void>({
      query: () => '/configuration/security',
      providesTags: ['SystemConfig'],
    }),
    
    updateSecuritySettings: builder.mutation<{ success: boolean }, Partial<SecuritySettings>>({
      query: (body) => ({
        url: '/configuration/security',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['SystemConfig'],
    }),

    // Schema Management
    refreshSchemaCache: builder.mutation<{ message: string; timestamp: string }, void>({
      query: () => ({
        url: '/query/refresh-schema',
        method: 'POST',
      }),
    }),

    // Semantic Metadata Management
    updateTableSemanticMetadata: builder.mutation<{ success: boolean }, {
      schemaName: string;
      tableName: string;
      metadata: any
    }>({
      query: ({ schemaName, tableName, metadata }) => ({
        url: `/semantic/tables/${schemaName}/${tableName}/metadata`,
        method: 'PUT',
        body: metadata,
      }),
    }),

    updateColumnSemanticMetadata: builder.mutation<{ success: boolean }, {
      tableName: string;
      columnName: string;
      metadata: any
    }>({
      query: ({ tableName, columnName, metadata }) => ({
        url: `/semantic/columns/${tableName}/${columnName}/metadata`,
        method: 'PUT',
        body: metadata,
      }),
    }),

    // Semantic Embeddings
    generateSemanticEmbeddings: builder.mutation<number, { forceRegeneration?: boolean }>({
      query: ({ forceRegeneration = false }) => ({
        url: `/semantic/embeddings/generate?forceRegeneration=${forceRegeneration}`,
        method: 'POST',
      }),
    }),

    // Semantic Validation
    validateSemanticMetadata: builder.query<any, void>({
      query: () => '/semantic/validate',
    }),

    // Semantic Enrichment
    enrichSchemaMetadata: builder.mutation<any, {
      tables?: string[];
      forceRegeneration?: boolean
    }>({
      query: (body) => ({
        url: '/semantic/enrich',
        method: 'POST',
        body,
      }),
    }),

    // Testing
    testSemanticLayer: builder.query<{
      businessFriendlySchema: string;
      relevantSchema: object;
      summary: object;
    }, { query: string }>({
      query: ({ query }) => `/semantic-layer/test?query=${encodeURIComponent(query)}`,
    }),

    // Configuration Management (Admin Only)
    getSystemConfiguration: builder.query<any, void>({
      query: () => '/configuration/system',
      providesTags: ['SystemConfig'],
    }),

    updateSystemConfiguration: builder.mutation<{ success: boolean }, any>({
      query: (body) => ({
        url: '/configuration/system',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['SystemConfig'],
    }),

    getAIProviders: builder.query<any[], void>({
      query: () => '/configuration/ai-providers',
      providesTags: ['AIProvider'],
    }),

    updateAIProvider: builder.mutation<{ success: boolean }, { providerId: string; config: any }>({
      query: ({ providerId, config }) => ({
        url: `/configuration/ai-providers/${providerId}`,
        method: 'PUT',
        body: config,
      }),
      invalidatesTags: ['AIProvider'],
    }),

    getSecuritySettings: builder.query<any, void>({
      query: () => '/configuration/security',
      providesTags: ['SecuritySettings'],
    }),

    updateSecuritySettings: builder.mutation<{ success: boolean }, any>({
      query: (body) => ({
        url: '/configuration/security',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['SecuritySettings'],
    }),
  }),
})

export const {
  useGetSystemStatisticsQuery,
  useGetQueryAnalyticsQuery,
  useGetPerformanceMetricsQuery,

  // Schema Management
  useRefreshSchemaCacheMutation,

  // Semantic Management
  useUpdateTableSemanticMetadataMutation,
  useUpdateColumnSemanticMetadataMutation,
  useGenerateSemanticEmbeddingsMutation,
  useValidateSemanticMetadataQuery,
  useEnrichSchemaMetadataMutation,
  useTestSemanticLayerQuery,

  // Configuration Management
  useGetSystemConfigurationQuery,
  useUpdateSystemConfigurationMutation,
  useGetAIProvidersQuery,
  useUpdateAIProviderMutation,
  useGetSecuritySettingsQuery,
  useUpdateSecuritySettingsMutation,
} = adminApi
