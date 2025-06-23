import { baseApi } from './baseApi'
import type { EnhancedQueryTransparencyData } from '@shared/types/transparency'

export interface QueryRequest {
  question: string
  context?: string
}

// ProcessFlow-enhanced query request
export interface ProcessFlowQueryRequest {
  query: string
  context?: string
  includeExplanation: boolean
  optimizeForPerformance: boolean
  maxResults?: number
  preferredTables?: string[]
  excludedTables?: string[]

  // ProcessFlow options
  includeTransparencyData: boolean
  enableProcessFlowTracking: boolean
  sessionId?: string
  trackingOptions?: {
    includeStepDetails?: boolean
    includeTokenUsage?: boolean
    includePerformanceMetrics?: boolean
  }
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

export interface ProcessFlowQueryResponse {
  sql: string
  explanation: string
  confidence: number
  semanticAnalysis: QuerySemanticAnalysis
  suggestedOptimizations: string[]
  estimatedExecutionTime: number
  results?: any[]
  metadata: QueryMetadata

  // ProcessFlow transparency metadata
  transparencyData: EnhancedQueryTransparencyData

  // ProcessFlow fields
  sessionId: string
  processingSteps: number
  totalProcessingTime: number
  success: boolean
  timestamp: string
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
    
    // ProcessFlow-enhanced query with full transparency tracking
    executeProcessFlowQuery: builder.mutation<ProcessFlowQueryResponse, ProcessFlowQueryRequest>({
      query: (body) => ({
        url: '/query/enhanced/processflow',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['QueryHistory', 'ProcessFlowSession'],
    }),

    // Get ProcessFlow session for a query
    getQueryProcessFlowSession: builder.query<{
      sessionId: string
      status: string
      steps: Array<{
        stepId: string
        stepType: string
        name: string
        status: string
        duration?: number
        confidence?: number
      }>
      transparency?: {
        totalTokens: number
        estimatedCost: number
        model: string
      }
    }, string>({
      query: (queryId) => `/query/${queryId}/processflow`,
      providesTags: (result, error, queryId) => [
        { type: 'QueryProcessFlow', id: queryId }
      ],
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
  useExecuteProcessFlowQueryMutation,
  useGetQueryProcessFlowSessionQuery,
  useGetQueryHistoryQuery,
  useRefreshSchemaCacheMutation,
  useStartStreamingQueryMutation,
  useGetQueryDetailsQuery,
  useGetQueryResultsQuery,
} = queryApi

// Enhanced hooks for ProcessFlow queries
export const useProcessFlowQuery = () => {
  const [executeQuery] = useExecuteProcessFlowQueryMutation()

  const executeWithTracking = async (request: ProcessFlowQueryRequest) => {
    try {
      const result = await executeQuery({
        ...request,
        includeTransparencyData: true,
        enableProcessFlowTracking: true,
        trackingOptions: {
          includeStepDetails: true,
          includeTokenUsage: true,
          includePerformanceMetrics: true,
          ...request.trackingOptions
        }
      }).unwrap()

      return result
    } catch (error) {
      console.error('ProcessFlow query execution failed:', error)
      throw error
    }
  }

  return {
    executeQuery: executeWithTracking,
    isLoading: false // This would be managed by the mutation state
  }
}

export const useQueryWithTransparency = (queryId: string) => {
  const detailsQuery = useGetQueryDetailsQuery(queryId, { skip: !queryId })
  const processFlowQuery = useGetQueryProcessFlowSessionQuery(queryId, { skip: !queryId })
  const resultsQuery = useGetQueryResultsQuery(queryId, { skip: !queryId })

  return {
    query: detailsQuery.data,
    processFlow: processFlowQuery.data,
    results: resultsQuery.data,
    isLoading: detailsQuery.isLoading || processFlowQuery.isLoading || resultsQuery.isLoading,
    error: detailsQuery.error || processFlowQuery.error || resultsQuery.error,
    refetch: () => {
      detailsQuery.refetch()
      processFlowQuery.refetch()
      resultsQuery.refetch()
    }
  }
}
