import { baseApi } from './baseApi'

export interface TuningDashboardData {
  totalTables: number
  tunedTables: number
  totalPatterns: number
  activePrompts: number
  lastTuningDate: string
  tuningCoverage: number
  recentActivity: Array<{
    timestamp: string
    action: string
    target: string
    user: string
  }>
  performanceMetrics: {
    avgQueryAccuracy: number
    avgResponseTime: number
    userSatisfactionScore: number
  }
}

export interface BusinessTableInfoDto {
  id: number
  schemaName: string
  tableName: string
  businessName?: string
  businessPurpose?: string
  domainClassification?: string
  keyColumns?: string[]
  relationships?: Array<{
    targetTable: string
    relationshipType: string
    description: string
  }>
  businessRules?: string[]
  sampleQueries?: string[]
  tags?: string[]
  priority: 'High' | 'Medium' | 'Low'
  lastTuned?: string
  tuningStatus: 'Not Started' | 'In Progress' | 'Complete' | 'Needs Review'
}

export interface QueryPatternDto {
  id: number
  name: string
  description: string
  pattern: string
  category: string
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced'
  exampleQueries: string[]
  sqlTemplate: string
  businessContext: string
  tags: string[]
  isActive: boolean
  usageCount: number
  successRate: number
  createdAt: string
  updatedAt: string
}

export interface CreateQueryPatternRequest {
  name: string
  description: string
  pattern: string
  category: string
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced'
  exampleQueries: string[]
  sqlTemplate: string
  businessContext: string
  tags: string[]
}

export interface PromptTemplateDto {
  id: number
  name: string
  description: string
  template: string
  category: 'Query Generation' | 'Schema Analysis' | 'Business Context' | 'Error Handling'
  variables: Array<{
    name: string
    type: string
    description: string
    required: boolean
    defaultValue?: string
  }>
  isActive: boolean
  version: string
  createdAt: string
  updatedAt: string
  usageStats: {
    totalUsage: number
    successRate: number
    avgResponseTime: number
  }
}

export const tuningApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Tuning Dashboard
    getTuningDashboard: builder.query<TuningDashboardData, void>({
      query: () => '/tuning/dashboard',
      providesTags: ['TuningDashboard'],
    }),

    // Business Tables for Tuning
    getTuningTables: builder.query<BusinessTableInfoDto[], {
      search?: string
      status?: string
      priority?: string
      domain?: string
      limit?: number
      offset?: number
    }>({
      query: ({ search, status, priority, domain, limit = 50, offset = 0 }) => {
        const params = new URLSearchParams()
        if (search) params.append('search', search)
        if (status) params.append('status', status)
        if (priority) params.append('priority', priority)
        if (domain) params.append('domain', domain)
        params.append('limit', String(limit))
        params.append('offset', String(offset))
        
        return `/tuning/tables?${params}`
      },
      providesTags: ['TuningTable'],
    }),

    getTuningTable: builder.query<BusinessTableInfoDto, number>({
      query: (id) => `/tuning/tables/${id}`,
      providesTags: (result, error, id) => [{ type: 'TuningTable', id }],
    }),

    updateTuningTable: builder.mutation<{ success: boolean }, {
      id: number
      data: Partial<BusinessTableInfoDto>
    }>({
      query: ({ id, data }) => ({
        url: `/tuning/tables/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'TuningTable', id },
        'TuningTable',
        'TuningDashboard'
      ],
    }),

    // Query Patterns
    getQueryPatterns: builder.query<QueryPatternDto[], {
      category?: string
      difficulty?: string
      isActive?: boolean
      search?: string
      limit?: number
      offset?: number
    }>({
      query: ({ category, difficulty, isActive, search, limit = 50, offset = 0 }) => {
        const params = new URLSearchParams()
        if (category) params.append('category', category)
        if (difficulty) params.append('difficulty', difficulty)
        if (isActive !== undefined) params.append('isActive', String(isActive))
        if (search) params.append('search', search)
        params.append('limit', String(limit))
        params.append('offset', String(offset))
        
        return `/tuning/patterns?${params}`
      },
      providesTags: ['QueryPattern'],
    }),

    getQueryPattern: builder.query<QueryPatternDto, number>({
      query: (id) => `/tuning/patterns/${id}`,
      providesTags: (result, error, id) => [{ type: 'QueryPattern', id }],
    }),

    createQueryPattern: builder.mutation<QueryPatternDto, CreateQueryPatternRequest>({
      query: (body) => ({
        url: '/tuning/patterns',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['QueryPattern', 'TuningDashboard'],
    }),

    updateQueryPattern: builder.mutation<{ success: boolean }, {
      id: number
      data: Partial<QueryPatternDto>
    }>({
      query: ({ id, data }) => ({
        url: `/tuning/patterns/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'QueryPattern', id },
        'QueryPattern',
        'TuningDashboard'
      ],
    }),

    deleteQueryPattern: builder.mutation<{ success: boolean }, number>({
      query: (id) => ({
        url: `/tuning/patterns/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['QueryPattern', 'TuningDashboard'],
    }),

    // Prompt Templates
    getPromptTemplates: builder.query<PromptTemplateDto[], {
      category?: string
      isActive?: boolean
      search?: string
      limit?: number
      offset?: number
    }>({
      query: ({ category, isActive, search, limit = 50, offset = 0 }) => {
        const params = new URLSearchParams()
        if (category) params.append('category', category)
        if (isActive !== undefined) params.append('isActive', String(isActive))
        if (search) params.append('search', search)
        params.append('limit', String(limit))
        params.append('offset', String(offset))
        
        return `/tuning/prompts?${params}`
      },
      providesTags: ['PromptTemplate'],
    }),

    getPromptTemplate: builder.query<PromptTemplateDto, number>({
      query: (id) => `/tuning/prompts/${id}`,
      providesTags: (result, error, id) => [{ type: 'PromptTemplate', id }],
    }),

    updatePromptTemplate: builder.mutation<{ success: boolean }, {
      id: number
      data: Partial<PromptTemplateDto>
    }>({
      query: ({ id, data }) => ({
        url: `/tuning/prompts/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'PromptTemplate', id },
        'PromptTemplate',
        'TuningDashboard'
      ],
    }),

    createPromptTemplate: builder.mutation<PromptTemplateDto, Partial<PromptTemplateDto>>({
      query: (body) => ({
        url: '/tuning/prompts',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['PromptTemplate', 'TuningDashboard'],
    }),

    deletePromptTemplate: builder.mutation<{ success: boolean }, number>({
      query: (id) => ({
        url: `/tuning/prompts/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['PromptTemplate', 'TuningDashboard'],
    }),

    // Testing and Validation
    testQueryPattern: builder.mutation<{
      success: boolean
      generatedSQL: string
      executionTime: number
      resultCount: number
      errors?: string[]
    }, {
      patternId: number
      testQuery: string
    }>({
      query: ({ patternId, testQuery }) => ({
        url: `/tuning/patterns/${patternId}/test`,
        method: 'POST',
        body: { testQuery },
      }),
    }),

    testPromptTemplate: builder.mutation<{
      success: boolean
      generatedResponse: string
      responseTime: number
      tokenUsage: number
      errors?: string[]
    }, {
      templateId: number
      variables: Record<string, any>
    }>({
      query: ({ templateId, variables }) => ({
        url: `/tuning/prompts/${templateId}/test`,
        method: 'POST',
        body: { variables },
      }),
    }),

    // Bulk Operations
    bulkUpdateTables: builder.mutation<{
      success: boolean
      updated: number
      errors: string[]
    }, {
      tableIds: number[]
      updates: Partial<BusinessTableInfoDto>
    }>({
      query: ({ tableIds, updates }) => ({
        url: '/tuning/tables/bulk-update',
        method: 'POST',
        body: { tableIds, updates },
      }),
      invalidatesTags: ['TuningTable', 'TuningDashboard'],
    }),

    exportTuningData: builder.mutation<Blob, {
      type: 'tables' | 'patterns' | 'prompts' | 'all'
      format: 'json' | 'csv' | 'xlsx'
    }>({
      query: ({ type, format }) => ({
        url: `/tuning/export?type=${type}&format=${format}`,
        method: 'POST',
        responseHandler: (response) => response.blob(),
      }),
    }),

    importTuningData: builder.mutation<{
      success: boolean
      imported: number
      errors: string[]
    }, {
      file: File
      type: 'tables' | 'patterns' | 'prompts'
      overwrite: boolean
    }>({
      query: ({ file, type, overwrite }) => {
        const formData = new FormData()
        formData.append('file', file)
        formData.append('type', type)
        formData.append('overwrite', String(overwrite))
        
        return {
          url: '/tuning/import',
          method: 'POST',
          body: formData,
        }
      },
      invalidatesTags: ['TuningTable', 'QueryPattern', 'PromptTemplate', 'TuningDashboard'],
    }),
  }),
})

export const {
  // Dashboard
  useGetTuningDashboardQuery,
  
  // Tables
  useGetTuningTablesQuery,
  useGetTuningTableQuery,
  useUpdateTuningTableMutation,
  
  // Query Patterns
  useGetQueryPatternsQuery,
  useGetQueryPatternQuery,
  useCreateQueryPatternMutation,
  useUpdateQueryPatternMutation,
  useDeleteQueryPatternMutation,
  
  // Prompt Templates
  useGetPromptTemplatesQuery,
  useGetPromptTemplateQuery,
  useUpdatePromptTemplateMutation,
  useCreatePromptTemplateMutation,
  useDeletePromptTemplateMutation,
  
  // Testing
  useTestQueryPatternMutation,
  useTestPromptTemplateMutation,
  
  // Bulk Operations
  useBulkUpdateTablesMutation,
  useExportTuningDataMutation,
  useImportTuningDataMutation,
} = tuningApi
