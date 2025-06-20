import { baseApi } from './baseApi'

export interface BusinessTableInfoDto {
  id: number
  tableName: string
  schemaName: string
  businessPurpose: string
  businessContext: string
  primaryUseCase: string
  commonQueryPatterns: string
  businessRules: string
  domainClassification: string
  naturalLanguageAliases: string
  usagePatterns: string
  dataQualityIndicators: string
  relationshipSemantics: string
  importanceScore: number
  usageFrequency: number
  businessOwner: string
  dataGovernancePolicies: string
  isActive: boolean
  createdDate: string
  updatedDate?: string
  columns: BusinessColumnInfoDto[]
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

export const businessApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Business Tables
    getBusinessTables: builder.query<BusinessTableInfoDto[], void>({
      query: () => '/business/tables',
      providesTags: ['BusinessTable'],
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
  }),
})

export const {
  useGetBusinessTablesQuery,
  useGetBusinessTableQuery,
  useCreateBusinessTableMutation,
  useUpdateBusinessTableMutation,
  useDeleteBusinessTableMutation,
  useGetBusinessColumnsQuery,
  useGetBusinessGlossaryQuery,
  useCreateGlossaryTermMutation,
  useUpdateGlossaryTermMutation,
  useDeleteGlossaryTermMutation,
} = businessApi
