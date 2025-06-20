import { baseApi } from './baseApi'

export interface SemanticEnrichmentRequest {
  query: string
  relevanceThreshold: number
  maxTables: number
  includeBusinessGlossary: boolean
  optimizeForLLM: boolean
  preferredDomains: string[]
  excludedTables: string[]
}

export interface EnhancedTableInfo {
  tableName: string
  schemaName: string
  businessPurpose: string
  relevanceScore: number
  enhancedColumns: EnhancedColumnInfo[]
  businessContext: string
  usagePatterns: string[]
  relationshipSemantics: string
  domainClassification: string
}

export interface EnhancedColumnInfo {
  columnName: string
  businessMeaning: string
  dataType: string
  relevanceScore: number
  semanticTags: string[]
  businessContext: string
}

export interface LLMOptimizedContext {
  contextualizedSchema: string
  relevantBusinessTerms: string[]
  queryIntent: string
  suggestedApproach: string
  potentialAmbiguities: string[]
}

export interface EnhancedSchemaResult {
  query: string
  queryAnalysis: {
    intent: string
    entities: Array<{ name: string; type: string; confidence: number }>
    businessTerms: string[]
    confidence: number
    ambiguities: Array<{ term: string; possibleMeanings: string[] }>
    suggestedClarifications: string[]
  }
  relevantTables: EnhancedTableInfo[]
  llmContext: LLMOptimizedContext
  generatedAt: string
  confidenceScore: number
}

export interface RelevantGlossaryTerm {
  term: string
  definition: string
  businessContext: string
  relevanceScore: number
  relatedTables: string[]
  relatedColumns: string[]
}

export interface TableSemanticMetadata {
  businessPurpose: string
  domainClassification: string
  usagePatterns: string[]
  relationshipSemantics: string
}

export interface ColumnSemanticMetadata {
  businessMeaning: string
  semanticTags: string[]
  businessContext: string
  dataLineage: string
}

export const semanticApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Analyze Query Semantics
    analyzeQuerySemantics: builder.mutation<EnhancedSchemaResult, SemanticEnrichmentRequest>({
      query: (body) => ({
        url: '/semantic/analyze',
        method: 'POST',
        body,
      }),
    }),
    
    // Enrich Schema Metadata
    enrichSchemaMetadata: builder.mutation<EnhancedSchemaResult, SemanticEnrichmentRequest>({
      query: (body) => ({
        url: '/semantic/enrich',
        method: 'POST',
        body,
      }),
    }),
    
    // Get Relevant Glossary Terms
    getRelevantGlossaryTerms: builder.query<RelevantGlossaryTerm[], { query: string; maxTerms?: number }>({
      query: ({ query, maxTerms = 10 }) => `/semantic/glossary/relevant?query=${encodeURIComponent(query)}&maxTerms=${maxTerms}`,
    }),
    
    // Update Table Semantic Metadata
    updateTableSemanticMetadata: builder.mutation<{ success: boolean }, { schemaName: string; tableName: string; metadata: TableSemanticMetadata }>({
      query: ({ schemaName, tableName, metadata }) => ({
        url: `/semantic/tables/${schemaName}/${tableName}/metadata`,
        method: 'PUT',
        body: metadata,
      }),
      invalidatesTags: ['BusinessTable'],
    }),
    
    // Update Column Semantic Metadata
    updateColumnSemanticMetadata: builder.mutation<{ success: boolean }, { tableName: string; columnName: string; metadata: ColumnSemanticMetadata }>({
      query: ({ tableName, columnName, metadata }) => ({
        url: `/semantic/columns/${tableName}/${columnName}/metadata`,
        method: 'PUT',
        body: metadata,
      }),
      invalidatesTags: ['BusinessColumn'],
    }),
    
    // Generate Semantic Embeddings
    generateSemanticEmbeddings: builder.mutation<number, { forceRegeneration?: boolean }>({
      query: ({ forceRegeneration = false }) => ({
        url: `/semantic/embeddings/generate?forceRegeneration=${forceRegeneration}`,
        method: 'POST',
      }),
    }),
    
    // Validate Semantic Metadata
    validateSemanticMetadata: builder.query<{
      isValid: boolean
      issues: Array<{ type: string; message: string; severity: 'error' | 'warning' }>
      coverage: number
    }, void>({
      query: () => '/semantic/validate',
    }),
    
    // Generate LLM-Optimized Context
    generateLLMContext: builder.mutation<LLMOptimizedContext, { query: string; intent: string }>({
      query: (body) => ({
        url: '/semantic/llm-context',
        method: 'POST',
        body,
      }),
    }),
  }),
})

export const {
  useAnalyzeQuerySemanticsMutation,
  useEnrichSchemaMetadataMutation,
  useGetRelevantGlossaryTermsQuery,
  useUpdateTableSemanticMetadataMutation,
  useUpdateColumnSemanticMetadataMutation,
  useGenerateSemanticEmbeddingsMutation,
  useValidateSemanticMetadataQuery,
  useGenerateLLMContextMutation,
} = semanticApi
