import { baseApi } from './baseApi'

export interface QueryRequest {
  question: string
  context?: string
}

export interface EnhancedQueryRequest {
  query: string
  context?: string
  includeExplanation: boolean
  optimizeForPerformance: boolean
  maxResults?: number
  preferredTables?: string[]
  excludedTables?: string[]
}

export interface QueryMetadata {
  executionTime: number
  rowCount: number
  columnCount: number
  queryComplexity: 'Simple' | 'Medium' | 'Complex'
  tablesUsed: string[]
  estimatedCost: number
}

export interface QuerySemanticAnalysis {
  intent: 'Aggregation' | 'Filter' | 'Join' | 'Comparison' | 'Trend'
  entities: Array<{
    name: string
    type: string
    confidence: number
  }>
  businessTerms: string[]
  confidence: number
  ambiguities: Array<{
    term: string
    possibleMeanings: string[]
  }>
  suggestedClarifications: string[]
}

export interface EnhancedQueryResponse {
  sql: string
  explanation: string
  confidence: number
  semanticAnalysis: QuerySemanticAnalysis
  suggestedOptimizations: string[]
  estimatedExecutionTime: number
  results?: any[]
  metadata: QueryMetadata
}

export interface QueryHistoryItem {
  id: string
  question: string
  sql: string
  executedAt: string
  executionTime: number
  rowCount: number
  isFavorite: boolean
  tags: string[]
}

export interface StreamingQueryRequest {
  query: string
  sessionId?: string
}

export interface QueryDetailsResponse {
  id: string
  question: string
  sql: string
  executedAt: string
  executionTime: number
  status: 'completed' | 'running' | 'failed'
  rowCount: number
  columnCount: number
  complexity: string
  cost: number
  user: string
}

export interface QueryResultsResponse {
  data: any[]
  columns: Array<{
    key: string
    title: string
    type: string
  }>
  metadata: QueryMetadata
}

export const queryApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Standard Query Processing
    executeQuery: builder.mutation<{ sql: string; results: any[]; metadata: QueryMetadata }, QueryRequest>({
      query: (body) => ({
        url: '/query',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['QueryHistory'],
    }),
    
    // Enhanced Query with AI
    executeEnhancedQuery: builder.mutation<EnhancedQueryResponse, EnhancedQueryRequest>({
      query: (body) => ({
        url: '/query/enhanced',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['QueryHistory'],
    }),
    
    // Query History
    getQueryHistory: builder.query<{ queries: QueryHistoryItem[]; total: number; page: number }, { page?: number; limit?: number }>({
      query: ({ page = 1, limit = 20 }) => `/query/history?page=${page}&limit=${limit}`,
      providesTags: ['QueryHistory'],
    }),
    
    // Refresh Schema Cache
    refreshSchemaCache: builder.mutation<{ message: string; timestamp: string }, void>({
      query: () => ({
        url: '/query/refresh-schema',
        method: 'POST',
      }),
      invalidatesTags: ['Schema'],
    }),
    
    // Streaming Query (for real-time updates)
    startStreamingQuery: builder.mutation<{ sessionId: string }, StreamingQueryRequest>({
      query: (body) => ({
        url: '/query/streaming',
        method: 'POST',
        body,
      }),
    }),

    // Query Details
    getQueryDetails: builder.query<QueryDetailsResponse, string>({
      query: (queryId) => `/query/${queryId}/details`,
      providesTags: (result, error, queryId) => [{ type: 'Query', id: queryId }],
    }),

    // Query Results
    getQueryResults: builder.query<QueryResultsResponse, string>({
      query: (queryId) => `/query/${queryId}/results`,
      providesTags: (result, error, queryId) => [{ type: 'Query', id: queryId }],
    }),
  }),
})

export const {
  useExecuteQueryMutation,
  useExecuteEnhancedQueryMutation,
  useGetQueryHistoryQuery,
  useRefreshSchemaCacheMutation,
  useStartStreamingQueryMutation,
  useGetQueryDetailsQuery,
  useGetQueryResultsQuery,
} = queryApi
