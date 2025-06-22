import { baseApi } from './baseApi'

// Enhanced Business Metadata API Interfaces (matching backend documentation)
export interface BusinessTableInfo {
  id: number
  tableName: string
  schemaName: string
  businessPurpose: string
  businessContext: string
  primaryUseCase: string
  commonQueryPatterns: string[]
  businessRules: string
  domainClassification: string
  naturalLanguageAliases: string[]
  businessProcesses: string[]
  analyticalUseCases: string[]
  reportingCategories: string[]
  vectorSearchKeywords: string[]
  businessGlossaryTerms: string[]
  llmContextHints: string[]
  queryComplexityHints: string[]
  isActive: boolean
  createdDate: string
  updatedDate: string
  createdBy: string
  updatedBy: string
  columns: BusinessColumnInfo[]
}

export interface BusinessColumnInfo {
  id: number
  columnName: string
  dataType: string
  businessMeaning: string
  dataExamples: string[]
  isKeyColumn: boolean
  isPII: boolean
  businessRules: string
  validationRules: string[]
  semanticTags: string[]
  llmContextHints: string[]
}

// Legacy interface for backward compatibility
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

// Enhanced API Request/Response Interfaces
export interface GetBusinessTablesRequest {
  page?: number
  pageSize?: number
  search?: string
  schema?: string
  domain?: string
}

export interface PaginationInfo {
  currentPage: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface GetBusinessTablesResponse {
  success: boolean
  data: BusinessTableInfo[]
  pagination: PaginationInfo
  filters: {
    search?: string
    schema?: string
    domain?: string
  }
}

export interface GetBusinessTableResponse {
  success: boolean
  data: BusinessTableInfo
}

export interface CreateTableRequest {
  tableName: string
  schemaName: string
  businessPurpose: string
  businessContext: string
  primaryUseCase: string
  commonQueryPatterns: string[]
  businessRules: string
  domainClassification: string
  naturalLanguageAliases: string[]
  businessProcesses: string[]
  analyticalUseCases: string[]
  reportingCategories: string[]
  vectorSearchKeywords: string[]
  businessGlossaryTerms: string[]
  llmContextHints: string[]
  queryComplexityHints: string[]
}

export interface UpdateTableRequest {
  businessPurpose: string
  businessContext: string
  primaryUseCase: string
  commonQueryPatterns: string[]
  businessRules: string
  domainClassification: string
  naturalLanguageAliases: string[]
  businessProcesses: string[]
  analyticalUseCases: string[]
  reportingCategories: string[]
  vectorSearchKeywords: string[]
  businessGlossaryTerms: string[]
  llmContextHints: string[]
  queryComplexityHints: string[]
  isActive: boolean
}

export interface DeleteTableResponse {
  success: boolean
  message: string
}

export interface BusinessTableSearchRequest {
  searchQuery: string
  schemas: string[]
  domains: string[]
  tags: string[]
  includeColumns: boolean
  includeGlossaryTerms: boolean
  maxResults: number
  minRelevanceScore: number
}

export interface SearchBusinessTablesResponse {
  success: boolean
  data: BusinessTableInfo[]
  metadata: {
    searchQuery: string
    totalResults: number
    maxResults: number
    appliedFilters: {
      schemas: string[]
      domains: string[]
      tags: string[]
    }
  }
}

export interface BulkBusinessTableRequest {
  tableIds: number[]
  operation: 'Delete' | 'Activate' | 'Deactivate' | 'UpdateDomain' | 'UpdateOwner' | 'UpdateTags' | 'RegenerateMetadata'
  operationData?: any
}

export interface BulkOperationResponse {
  success: boolean
  message: string
  summary: {
    operation: string
    totalProcessed: number
    successful: number
    errors: number
  }
  results: Array<{
    tableId: number
    success: boolean
    message: string
  }>
}

export interface ValidationIssue {
  type: string
  message: string
  severity: string
  field?: string
  context?: any
}

export interface ValidationWarning {
  type: string
  message: string
  field?: string
}

export interface ValidationSuggestion {
  type: string
  message: string
  recommendedAction: string
}

export interface BusinessTableValidationRequest {
  tableId?: number
  schemaName?: string
  tableName?: string
  validateBusinessRules: boolean
  validateDataQuality: boolean
  validateRelationships: boolean
}

export interface BusinessTableValidationResponse {
  success: boolean
  data: {
    isValid: boolean
    issues: ValidationIssue[]
    warnings: ValidationWarning[]
    suggestions: ValidationSuggestion[]
    validatedAt: string
  }
}

export interface BusinessMetadataStatistics {
  totalTables: number
  populatedTables: number
  tablesWithAIMetadata: number
  tablesWithRuleBasedMetadata: number
  totalColumns: number
  populatedColumns: number
  totalGlossaryTerms: number
  lastPopulationRun: string
  tablesByDomain: Record<string, number>
  tablesBySchema: Record<string, number>
  mostActiveUsers: string[]
  averageMetadataCompleteness: number
}

// Enhanced Business Glossary Interfaces
export interface EnhancedBusinessGlossaryTerm {
  id: number
  term: string
  definition: string
  category: string
  domain: string
  businessOwner: string
  synonyms: string[]
  relatedTerms: string[]
  examples: string[]
  dataExamples: string[]
  mappedTables: string[]
  mappedColumns: string[]
  hierarchicalRelations: string[]
  preferredCalculation: string
  disambiguationRules: string
  regulationReferences: string[]
  contextualVariations: string[]
  usageFrequency: number
  qualityScore: number
  isActive: boolean
  createdDate: string
  updatedDate: string
  createdBy: string
  updatedBy: string
  tags: string[]
  relationships: GlossaryTermRelationship[]
}

export interface GlossaryTermRelationship {
  id: number
  relatedTermId: number
  relatedTermName: string
  relationshipType: 'synonym' | 'antonym' | 'broader' | 'narrower' | 'related' | 'partOf' | 'hasA'
  description: string
}

export interface BusinessDomain {
  id: number
  name: string
  description: string
  parentDomainId?: number
  parentDomainName?: string
  businessOwner: string
  dataGovernanceLevel: string
  subDomains: BusinessDomain[]
  relatedTables: string[]
  relatedGlossaryTerms: string[]
  domainExperts: string[]
  governanceRules: string[]
  dataClassification: string
  complianceRequirements: string[]
  isActive: boolean
  createdDate: string
  updatedDate: string
  createdBy: string
  updatedBy: string
}

// Enhanced API Request/Response Interfaces
export interface GetEnhancedGlossaryRequest {
  page?: number
  pageSize?: number
  search?: string
  category?: string
  domain?: string
  tags?: string[]
  includeRelationships?: boolean
}

export interface GetEnhancedGlossaryResponse {
  success: boolean
  data: EnhancedBusinessGlossaryTerm[]
  pagination: PaginationInfo
  filters: {
    search?: string
    category?: string
    domain?: string
    tags?: string[]
  }
}

export interface CreateGlossaryTermRequest {
  term: string
  definition: string
  category: string
  domain: string
  businessOwner: string
  synonyms?: string[]
  relatedTerms?: string[]
  examples?: string[]
  dataExamples?: string[]
  tags?: string[]
}

export interface UpdateGlossaryTermRequest {
  term?: string
  definition?: string
  category?: string
  domain?: string
  businessOwner?: string
  synonyms?: string[]
  relatedTerms?: string[]
  examples?: string[]
  dataExamples?: string[]
  tags?: string[]
  isActive?: boolean
}

export interface GlossaryTermSearchRequest {
  searchQuery: string
  categories: string[]
  domains: string[]
  tags: string[]
  includeDefinitions: boolean
  includeExamples: boolean
  maxResults: number
  minRelevanceScore: number
}

export interface SearchGlossaryTermsResponse {
  success: boolean
  data: EnhancedBusinessGlossaryTerm[]
  metadata: {
    searchQuery: string
    totalResults: number
    maxResults: number
    appliedFilters: {
      categories: string[]
      domains: string[]
      tags: string[]
    }
  }
}

export interface GetBusinessDomainsRequest {
  page?: number
  pageSize?: number
  search?: string
  parentDomainId?: number
  includeSubDomains?: boolean
}

export interface GetBusinessDomainsResponse {
  success: boolean
  data: BusinessDomain[]
  pagination: PaginationInfo
  filters: {
    search?: string
    parentDomainId?: number
  }
}

export interface CreateDomainRequest {
  name: string
  description: string
  parentDomainId?: number
  businessOwner: string
  dataGovernanceLevel: string
  domainExperts?: string[]
  governanceRules?: string[]
  dataClassification: string
  complianceRequirements?: string[]
}

export interface UpdateDomainRequest {
  name?: string
  description?: string
  parentDomainId?: number
  businessOwner?: string
  dataGovernanceLevel?: string
  domainExperts?: string[]
  governanceRules?: string[]
  dataClassification?: string
  complianceRequirements?: string[]
  isActive?: boolean
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

    // Enhanced Business Metadata API Endpoints (New Backend)
    getEnhancedBusinessTables: builder.query<GetBusinessTablesResponse, GetBusinessTablesRequest>({
      query: (params) => {
        const searchParams = new URLSearchParams()
        if (params.page) searchParams.append('page', params.page.toString())
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString())
        if (params.search) searchParams.append('search', params.search)
        if (params.schema) searchParams.append('schema', params.schema)
        if (params.domain) searchParams.append('domain', params.domain)

        return {
          url: '/business-metadata/tables',
          params: Object.fromEntries(searchParams),
        }
      },
      providesTags: ['EnhancedBusinessTable'],
    }),

    getEnhancedBusinessTable: builder.query<GetBusinessTableResponse, number>({
      query: (id) => `/business-metadata/tables/${id}`,
      providesTags: (result, error, id) => [{ type: 'EnhancedBusinessTable', id }],
    }),

    createEnhancedBusinessTable: builder.mutation<BusinessTableInfo, CreateTableRequest>({
      query: (body) => ({
        url: '/business-metadata/tables',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['EnhancedBusinessTable'],
    }),

    updateEnhancedBusinessTable: builder.mutation<BusinessTableInfo, { id: number } & UpdateTableRequest>({
      query: ({ id, ...body }) => ({
        url: `/business-metadata/tables/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'EnhancedBusinessTable', id }],
    }),

    deleteEnhancedBusinessTable: builder.mutation<DeleteTableResponse, number>({
      query: (id) => ({
        url: `/business-metadata/tables/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['EnhancedBusinessTable'],
    }),

    searchEnhancedBusinessTables: builder.mutation<SearchBusinessTablesResponse, BusinessTableSearchRequest>({
      query: (body) => ({
        url: '/business-metadata/tables/search',
        method: 'POST',
        body,
      }),
    }),

    bulkOperateEnhancedBusinessTables: builder.mutation<BulkOperationResponse, BulkBusinessTableRequest>({
      query: (body) => ({
        url: '/business-metadata/tables/bulk',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['EnhancedBusinessTable'],
    }),

    validateEnhancedBusinessTable: builder.mutation<BusinessTableValidationResponse, BusinessTableValidationRequest>({
      query: (body) => ({
        url: '/business-metadata/tables/validate',
        method: 'POST',
        body,
      }),
    }),

    getEnhancedBusinessMetadataStatistics: builder.query<BusinessMetadataStatistics, void>({
      query: () => '/business-metadata/statistics',
      providesTags: ['EnhancedBusinessMetadata'],
    }),

    // Enhanced Business Glossary API Endpoints
    getEnhancedBusinessGlossary: builder.query<GetEnhancedGlossaryResponse, GetEnhancedGlossaryRequest>({
      query: (params) => {
        const searchParams = new URLSearchParams()
        if (params.page) searchParams.append('page', params.page.toString())
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString())
        if (params.search) searchParams.append('search', params.search)
        if (params.category) searchParams.append('category', params.category)
        if (params.domain) searchParams.append('domain', params.domain)
        if (params.includeRelationships) searchParams.append('includeRelationships', params.includeRelationships.toString())
        if (params.tags) params.tags.forEach(tag => searchParams.append('tags', tag))

        return {
          url: '/business-metadata/glossary',
          params: Object.fromEntries(searchParams),
        }
      },
      providesTags: ['EnhancedGlossary'],
    }),

    getEnhancedGlossaryTerm: builder.query<{ success: boolean; data: EnhancedBusinessGlossaryTerm }, number>({
      query: (id) => `/business-metadata/glossary/${id}`,
      providesTags: (result, error, id) => [{ type: 'EnhancedGlossary', id }],
    }),

    createEnhancedGlossaryTerm: builder.mutation<EnhancedBusinessGlossaryTerm, CreateGlossaryTermRequest>({
      query: (body) => ({
        url: '/business-metadata/glossary',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['EnhancedGlossary'],
    }),

    updateEnhancedGlossaryTerm: builder.mutation<EnhancedBusinessGlossaryTerm, { id: number } & UpdateGlossaryTermRequest>({
      query: ({ id, ...body }) => ({
        url: `/business-metadata/glossary/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'EnhancedGlossary', id }],
    }),

    deleteEnhancedGlossaryTerm: builder.mutation<{ success: boolean; message: string }, number>({
      query: (id) => ({
        url: `/business-metadata/glossary/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['EnhancedGlossary'],
    }),

    searchEnhancedGlossaryTerms: builder.mutation<SearchGlossaryTermsResponse, GlossaryTermSearchRequest>({
      query: (body) => ({
        url: '/business-metadata/glossary/search',
        method: 'POST',
        body,
      }),
    }),

    // Enhanced Business Domains API Endpoints
    getEnhancedBusinessDomains: builder.query<GetBusinessDomainsResponse, GetBusinessDomainsRequest>({
      query: (params) => {
        const searchParams = new URLSearchParams()
        if (params.page) searchParams.append('page', params.page.toString())
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString())
        if (params.search) searchParams.append('search', params.search)
        if (params.parentDomainId) searchParams.append('parentDomainId', params.parentDomainId.toString())
        if (params.includeSubDomains) searchParams.append('includeSubDomains', params.includeSubDomains.toString())

        return {
          url: '/business-metadata/domains',
          params: Object.fromEntries(searchParams),
        }
      },
      providesTags: ['EnhancedDomain'],
    }),

    getEnhancedBusinessDomain: builder.query<{ success: boolean; data: BusinessDomain }, number>({
      query: (id) => `/business-metadata/domains/${id}`,
      providesTags: (result, error, id) => [{ type: 'EnhancedDomain', id }],
    }),

    createEnhancedBusinessDomain: builder.mutation<BusinessDomain, CreateDomainRequest>({
      query: (body) => ({
        url: '/business-metadata/domains',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['EnhancedDomain'],
    }),

    updateEnhancedBusinessDomain: builder.mutation<BusinessDomain, { id: number } & UpdateDomainRequest>({
      query: ({ id, ...body }) => ({
        url: `/business-metadata/domains/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'EnhancedDomain', id }],
    }),

    deleteEnhancedBusinessDomain: builder.mutation<{ success: boolean; message: string }, number>({
      query: (id) => ({
        url: `/business-metadata/domains/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['EnhancedDomain'],
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

  // Enhanced Business Metadata API Hooks
  useGetEnhancedBusinessTablesQuery,
  useGetEnhancedBusinessTableQuery,
  useCreateEnhancedBusinessTableMutation,
  useUpdateEnhancedBusinessTableMutation,
  useDeleteEnhancedBusinessTableMutation,
  useSearchEnhancedBusinessTablesMutation,
  useBulkOperateEnhancedBusinessTablesMutation,
  useValidateEnhancedBusinessTableMutation,
  useGetEnhancedBusinessMetadataStatisticsQuery,

  // Enhanced Business Glossary API Hooks
  useGetEnhancedBusinessGlossaryQuery,
  useGetEnhancedGlossaryTermQuery,
  useCreateEnhancedGlossaryTermMutation,
  useUpdateEnhancedGlossaryTermMutation,
  useDeleteEnhancedGlossaryTermMutation,
  useSearchEnhancedGlossaryTermsMutation,

  // Enhanced Business Domains API Hooks
  useGetEnhancedBusinessDomainsQuery,
  useGetEnhancedBusinessDomainQuery,
  useCreateEnhancedBusinessDomainMutation,
  useUpdateEnhancedBusinessDomainMutation,
  useDeleteEnhancedBusinessDomainMutation,
} = businessApi
