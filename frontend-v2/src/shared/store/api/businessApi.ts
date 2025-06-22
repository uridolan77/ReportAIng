import { baseApi } from './baseApi'

export interface BusinessTableInfoDto {
  id: number
  tableName: string
  schemaName: string
  businessName?: string
  businessPurpose: string
  businessContext: string
  primaryUseCase: string
  commonQueryPatterns: string | string[]
  businessRules: string
  domainClassification: string
  naturalLanguageAliases: string | string[]
  usagePatterns: string | object
  dataQualityIndicators: string | object
  relationshipSemantics: string | object[]
  importanceScore: number
  usageFrequency: number
  businessOwner: string
  dataGovernancePolicies: string | string[]
  isActive: boolean
  createdDate: string
  updatedDate?: string
  createdBy?: string
  updatedBy?: string

  // Additional semantic and AI-related fields
  semanticDescription?: string
  businessProcesses?: string | string[]
  analyticalUseCases?: string | string[]
  reportingCategories?: string | string[]
  semanticRelationships?: string | object
  queryComplexityHints?: string | string[]
  businessGlossaryTerms?: string | string[]
  semanticCoverageScore?: number
  llmContextHints?: string | string[]
  vectorSearchKeywords?: string | string[]
  lastAnalyzed?: string

  columns?: BusinessColumnInfoDto[]
}

export interface BusinessColumnInfoDto {
  id: number
  tableInfoId: number
  columnName: string
  businessMeaning: string
  businessContext: string
  dataExamples: string
  validationRules: string
  naturalLanguageAliases: string
  valueExamples: string
  dataLineage: string
  calculationRules: string
  semanticTags: string
  businessDataType: string
  constraintsAndRules: string
  dataQualityScore: number
  usageFrequency: number
  preferredAggregation: string
  relatedBusinessTerms: string
  isKeyColumn: boolean
  isSensitiveData: boolean
  isCalculatedField: boolean
  isActive: boolean
  createdDate?: string
  updatedDate?: string
  createdBy?: string
  updatedBy?: string

  // Additional semantic and AI-related fields
  semanticContext?: string
  conceptualRelationships?: string
  domainSpecificTerms?: string
  queryIntentMapping?: string
  businessQuestionTypes?: string
  semanticSynonyms?: string
  analyticalContext?: string
  businessMetrics?: string
  semanticRelevanceScore?: number
  llmPromptHints?: string
  vectorSearchTags?: string
  businessPurpose?: string
  businessFriendlyName?: string
  naturalLanguageDescription?: string
  businessRules?: string
  relationshipContext?: string
  dataGovernanceLevel?: string
  lastBusinessReview?: string
  importanceScore?: number
}

export interface BusinessGlossaryDto {
  id: number
  term: string
  definition: string
  businessContext: string
  synonyms: string
  relatedTerms: string
  category: string
  domain: string
  examples: string
  mappedTables: string
  mappedColumns: string
  hierarchicalRelations: string
  preferredCalculation: string
  disambiguationRules: string
  businessOwner: string
  regulationReferences: string
  confidenceScore: number
  ambiguityScore: number
  contextualVariations: string
  usageCount: number
  lastUsed?: string
  lastValidated?: string
  isActive: boolean
}

export interface CreateBusinessTableRequest {
  tableName: string
  schemaName: string
  businessPurpose: string
  businessContext: string
  primaryUseCase: string
}

export interface UpdateBusinessTableRequest extends Partial<BusinessTableInfoDto> {
  id: number
}

export interface CreateGlossaryTermRequest {
  term: string
  definition: string
  businessContext: string
  category: string
  domain: string
}

export interface UpdateGlossaryTermRequest extends Partial<BusinessGlossaryDto> {
  id: number
}

export interface UpdateColumnRequest {
  columnName: string
  businessFriendlyName?: string
  businessDataType?: string
  naturalLanguageDescription?: string
  businessMeaning?: string
  businessContext?: string
  businessPurpose?: string
  dataExamples?: string[]
  valueExamples?: string[]
  validationRules?: string
  businessRules?: string
  preferredAggregation?: string
  dataGovernanceLevel?: string
  lastBusinessReview?: string
  dataQualityScore?: number
  usageFrequency?: number
  semanticRelevanceScore?: number
  importanceScore?: number
  isActive?: boolean
  isKeyColumn?: boolean
  isSensitiveData?: boolean
  isCalculatedField?: boolean
}

export const businessApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Business Tables
    getBusinessTables: builder.query<BusinessTableInfoDto[], void>({
      query: () => '/business/tables',
      providesTags: ['BusinessTable'],
      transformResponse: (response: any) => {
        // Handle both direct array response and wrapped response
        if (Array.isArray(response)) {
          return response
        }
        if (response?.data && Array.isArray(response.data)) {
          return response.data
        }
        if (response?.tables && Array.isArray(response.tables)) {
          return response.tables
        }
        return []
      },
    }),

    getBusinessTable: builder.query<BusinessTableInfoDto, number>({
      query: (id) => `/business/tables/${id}`,
      providesTags: (result, error, id) => [{ type: 'BusinessTable', id }],
    }),

    createBusinessTable: builder.mutation<BusinessTableInfoDto, CreateBusinessTableRequest>({
      query: (body) => ({
        url: '/business/tables',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['BusinessTable'],
    }),

    updateBusinessTable: builder.mutation<BusinessTableInfoDto, UpdateBusinessTableRequest>({
      query: ({ id, ...body }) => ({
        url: `/business/tables/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'BusinessTable', id }],
    }),

    deleteBusinessTable: builder.mutation<{ success: boolean }, number>({
      query: (id) => ({
        url: `/business/tables/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['BusinessTable'],
    }),

    // Business Columns
    getBusinessColumns: builder.query<BusinessColumnInfoDto[], number>({
      query: (tableId) => `/business/tables/${tableId}/columns`,
      providesTags: ['BusinessColumn'],
    }),

    getBusinessColumn: builder.query<BusinessColumnInfoDto, number>({
      query: (columnId) => `/business/columns/${columnId}`,
      providesTags: (result, error, columnId) => [{ type: 'BusinessColumn', id: columnId }],
    }),

    updateBusinessColumn: builder.mutation<BusinessColumnInfoDto, { columnId: number } & UpdateColumnRequest>({
      query: ({ columnId, ...body }) => ({
        url: `/business/columns/${columnId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { columnId }) => [
        { type: 'BusinessColumn', id: columnId },
        'BusinessColumn',
        'BusinessTable'
      ],
    }),

    // Business Glossary
    getBusinessGlossary: builder.query<{ terms: BusinessGlossaryDto[]; total: number }, { page?: number; limit?: number; category?: string }>({
      query: ({ page = 1, limit = 50, category }) => {
        const params = new URLSearchParams({
          page: page.toString(),
          limit: limit.toString(),
        })
        if (category) params.append('category', category)
        return `/business/glossary?${params}`
      },
      providesTags: ['GlossaryTerm'],
    }),

    createGlossaryTerm: builder.mutation<BusinessGlossaryDto, CreateGlossaryTermRequest>({
      query: (body) => ({
        url: '/business/glossary',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['GlossaryTerm'],
    }),

    updateGlossaryTerm: builder.mutation<BusinessGlossaryDto, UpdateGlossaryTermRequest>({
      query: ({ id, ...body }) => ({
        url: `/business/glossary/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'GlossaryTerm', id }],
    }),

    deleteGlossaryTerm: builder.mutation<{ success: boolean }, number>({
      query: (id) => ({
        url: `/business/glossary/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['GlossaryTerm'],
    }),

    // Business Metadata Management
    getBusinessMetadataStatus: builder.query<{
      totalTables: number
      populatedTables: number
      lastUpdate: string
    }, void>({
      query: () => '/business-metadata/status',
      providesTags: ['BusinessMetadata'],
    }),

    populateAllBusinessMetadata: builder.mutation<{
      success: boolean
      tablesProcessed: number
      tablesUpdated: number
      errors: string[]
      duration: number
    }, { useAI?: boolean; overwriteExisting?: boolean }>({
      query: ({ useAI = true, overwriteExisting = false }) => ({
        url: `/business-metadata/populate-all?useAI=${useAI}&overwriteExisting=${overwriteExisting}`,
        method: 'POST',
      }),
      invalidatesTags: ['BusinessMetadata', 'BusinessTable'],
    }),

    populateRelevantBusinessMetadata: builder.mutation<{
      success: boolean
      message: string
      summary: {
        totalProcessed: number
        successful: number
        errors: number
        skipped: number
        relevantTablesOnly: boolean
      }
      details: Array<{
        schema: string
        table: string
        success: boolean
        skipped: boolean
        error?: string
      }>
      processedAt: string
    }, { useAI?: boolean; overwriteExisting?: boolean }>({
      query: ({ useAI = true, overwriteExisting = false }) => ({
        url: `/business-metadata/populate-relevant?useAI=${useAI}&overwriteExisting=${overwriteExisting}`,
        method: 'POST',
      }),
      invalidatesTags: ['BusinessMetadata', 'BusinessTable'],
    }),

    populateTableBusinessMetadata: builder.mutation<{ success: boolean; message: string }, {
      schemaName: string
      tableName: string
      useAI?: boolean
      overwriteExisting?: boolean
    }>({
      query: ({ schemaName, tableName, useAI = true, overwriteExisting = false }) => ({
        url: `/business-metadata/populate-table/${schemaName}/${tableName}?useAI=${useAI}&overwriteExisting=${overwriteExisting}`,
        method: 'POST',
      }),
      invalidatesTags: ['BusinessMetadata', 'BusinessTable'],
    }),

    previewTableBusinessMetadata: builder.mutation<{
      success: boolean
      message: string
      preview: {
        schema: string
        table: string
        generationMethod: string
        estimatedBusinessPurpose: string
        estimatedDomain: string
        estimatedImportance: number
        note: string
      }
      generatedAt: string
    }, {
      schemaName: string
      tableName: string
      useAI?: boolean
    }>({
      query: ({ schemaName, tableName, useAI = true }) => ({
        url: `/business-metadata/preview-table/${schemaName}/${tableName}?useAI=${useAI}`,
        method: 'POST',
      }),
    }),

    validateBusinessMetadata: builder.query<{
      coverage: number
      missingTables: string[]
      issues: string[]
    }, void>({
      query: () => '/business-metadata/validate',
      providesTags: ['BusinessMetadata'],
    }),

    // Schema Discovery & Management
    getAllSchemaTables: builder.query<Array<{
      schemaName: string
      tableName: string
      businessPurpose?: string
      domainClassification?: string
      estimatedRowCount?: number
      lastUpdated?: string
    }>, void>({
      query: () => '/schema/tables',
      providesTags: ['Schema'],
    }),

    getSchemaTableDetails: builder.query<{
      schemaName: string
      tableName: string
      businessPurpose?: string
      domainClassification?: string
      estimatedRowCount?: number
      lastUpdated?: string
      columns?: Array<{
        columnName: string
        businessMeaning?: string
        businessDataType?: string
        isNullable?: boolean
        sampleValues?: string[]
      }>
    }, { schemaName: string; tableName: string }>({
      query: ({ schemaName, tableName }) => `/schema/tables/${schemaName}.${tableName}`,
      providesTags: (result, error, { schemaName, tableName }) => [
        { type: 'Schema', id: `${schemaName}.${tableName}` }
      ],
    }),

    getSchemaSummary: builder.query<{
      databaseName: string
      tableCount: number
      lastUpdated: string
    }, void>({
      query: () => '/schema/summary',
      providesTags: ['Schema'],
    }),

    getDataSources: builder.query<Array<{
      name: string
      schema: string
      type: string
      rowCount: number
    }>, void>({
      query: () => '/schema/datasources',
      providesTags: ['Schema'],
    }),

    refreshSchema: builder.mutation<{ message: string; tablesDiscovered: number }, void>({
      query: () => ({
        url: '/schema/refresh',
        method: 'POST',
      }),
      invalidatesTags: ['Schema', 'BusinessMetadata'],
    }),
  }),
})

export const {
  useGetBusinessTablesQuery,
  useGetBusinessTableQuery,
  useCreateBusinessTableMutation,
  useUpdateBusinessTableMutation,
  useDeleteBusinessTableMutation,
  useGetBusinessColumnsQuery,
  useGetBusinessColumnQuery,
  useUpdateBusinessColumnMutation,
  useGetBusinessGlossaryQuery,
  useCreateGlossaryTermMutation,
  useUpdateGlossaryTermMutation,
  useDeleteGlossaryTermMutation,

  // Business Metadata Management
  useGetBusinessMetadataStatusQuery,
  usePopulateAllBusinessMetadataMutation,
  usePopulateRelevantBusinessMetadataMutation,
  usePopulateTableBusinessMetadataMutation,
  usePreviewTableBusinessMetadataMutation,
  useValidateBusinessMetadataQuery,

  // Schema Discovery & Management
  useGetAllSchemaTablesQuery,
  useGetSchemaTableDetailsQuery,
  useGetSchemaSummaryQuery,
  useGetDataSourcesQuery,
  useRefreshSchemaMutation,
} = businessApi
