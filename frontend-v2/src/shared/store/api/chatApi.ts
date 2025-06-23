import { baseApi } from './baseApi'
import type {
  ChatMessage,
  Conversation,
  QuerySuggestion,
  SendMessageRequest,
  SendMessageResponse,
  GetConversationsRequest,
  GetConversationsResponse,
  ExportOptions
} from '../../types/chat'

export const chatApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Conversation management
    getConversations: builder.query<GetConversationsResponse, GetConversationsRequest>({
      query: ({ page = 1, limit = 20, search, tags, dateRange }) => {
        const params = new URLSearchParams({
          page: page.toString(),
          limit: limit.toString(),
        })
        
        if (search) params.append('search', search)
        if (tags?.length) params.append('tags', tags.join(','))
        if (dateRange) {
          params.append('startDate', dateRange.start.toISOString())
          params.append('endDate', dateRange.end.toISOString())
        }
        
        return `/chat/conversations?${params}`
      },
      providesTags: ['Conversation'],
    }),

    getConversation: builder.query<Conversation, string>({
      query: (conversationId) => `/chat/conversations/${conversationId}`,
      providesTags: (result, error, id) => [{ type: 'Conversation', id }],
    }),

    createConversation: builder.mutation<Conversation, { title?: string; context?: any }>({
      query: (body) => ({
        url: '/chat/conversations',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Conversation'],
    }),

    updateConversation: builder.mutation<Conversation, { id: string; updates: Partial<Conversation> }>({
      query: ({ id, updates }) => ({
        url: `/chat/conversations/${id}`,
        method: 'PATCH',
        body: updates,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Conversation', id }],
    }),

    deleteConversation: builder.mutation<{ success: boolean }, string>({
      query: (conversationId) => ({
        url: `/chat/conversations/${conversationId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Conversation'],
    }),

    // Message management
    getMessages: builder.query<{ messages: ChatMessage[]; total: number }, { conversationId: string; page?: number; limit?: number }>({
      query: ({ conversationId, page = 1, limit = 50 }) => 
        `/chat/conversations/${conversationId}/messages?page=${page}&limit=${limit}`,
      providesTags: (result, error, { conversationId }) => [
        { type: 'Message', id: conversationId },
        ...((result?.messages || []).map(msg => ({ type: 'Message' as const, id: msg.id })))
      ],
    }),

    sendMessage: builder.mutation<SendMessageResponse, SendMessageRequest>({
      query: (body) => ({
        url: '/chat/messages',
        method: 'POST',
        body,
      }),
      invalidatesTags: (result, error, { conversationId }) => [
        { type: 'Message', id: conversationId },
        'QueryHistory',
      ],
    }),

    updateMessage: builder.mutation<ChatMessage, { id: string; updates: Partial<ChatMessage> }>({
      query: ({ id, updates }) => ({
        url: `/chat/messages/${id}`,
        method: 'PATCH',
        body: updates,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Message', id }],
    }),

    deleteMessage: builder.mutation<{ success: boolean }, string>({
      query: (messageId) => ({
        url: `/chat/messages/${messageId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, messageId) => [{ type: 'Message', id: messageId }],
    }),

    // Query suggestions and autocomplete
    getQuerySuggestions: builder.query<QuerySuggestion[], { query?: string; limit?: number; context?: any }>({
      query: ({ query = '', limit = 10, context }) => {
        const params = new URLSearchParams({
          q: query,
          limit: limit.toString(),
        })
        
        if (context) {
          params.append('context', JSON.stringify(context))
        }
        
        return `/chat/suggestions?${params}`
      },
    }),

    getPopularQueries: builder.query<QuerySuggestion[], { limit?: number; timeframe?: 'day' | 'week' | 'month' }>({
      query: ({ limit = 10, timeframe = 'week' }) =>
        `/QuerySuggestions/popular?count=${limit}`,
    }),

    getQueryTemplates: builder.query<QuerySuggestion[], { category?: string; limit?: number }>({
      query: ({ category, limit = 20 }) => {
        if (category) {
          return `/QuerySuggestions/category/${category}?includeInactive=false`
        }
        return `/QuerySuggestions/grouped?includeInactive=false`
      },
    }),

    // Enhanced query execution with streaming support
    executeEnhancedQuery: builder.mutation<{
      message: ChatMessage
      sessionId?: string
      streamingEnabled?: boolean
    }, {
      query: string
      conversationId?: string
      context?: any
      options?: {
        includeSemanticAnalysis?: boolean
        enableStreaming?: boolean
        maxResults?: number
        chartType?: string
      }
    }>({
      query: (body) => ({
        url: '/query/enhanced',
        method: 'POST',
        body: {
          Query: body.query,
          ExecuteQuery: true,
          IncludeAlternatives: true,
          IncludeSemanticAnalysis: body.options?.includeSemanticAnalysis ?? true
        },
      }),
      transformResponse: (response: any) => {
        // Transform backend response to expected frontend format
        return {
          message: {
            id: `msg-${Date.now()}`,
            type: 'assistant' as const,
            content: response.ProcessedQuery?.explanation || 'Query processed successfully',
            timestamp: new Date().toISOString(),
            sql: response.ProcessedQuery?.sql,
            results: response.QueryResult?.data,
            resultMetadata: response.QueryResult?.metadata,
            semanticAnalysis: response.SemanticAnalysis,
            status: 'delivered' as const
          },
          sessionId: undefined, // Backend doesn't provide sessionId for this endpoint
          streamingEnabled: false
        }
      },
      invalidatesTags: ['QueryHistory', 'Message'],
    }),

    // Message actions
    toggleMessageFavorite: builder.mutation<{ success: boolean; isFavorite: boolean }, string>({
      query: (messageId) => ({
        url: `/chat/messages/${messageId}/favorite`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, messageId) => [{ type: 'Message', id: messageId }],
    }),

    addMessageReaction: builder.mutation<{ success: boolean }, { messageId: string; reaction: string }>({
      query: ({ messageId, reaction }) => ({
        url: `/chat/messages/${messageId}/reactions`,
        method: 'POST',
        body: { reaction },
      }),
      invalidatesTags: (result, error, { messageId }) => [{ type: 'Message', id: messageId }],
    }),

    shareMessage: builder.mutation<{ shareUrl: string }, { messageId: string; permissions?: any }>({
      query: ({ messageId, permissions }) => ({
        url: `/chat/messages/${messageId}/share`,
        method: 'POST',
        body: { permissions },
      }),
    }),

    // Export functionality
    exportConversation: builder.mutation<{ downloadUrl: string }, { conversationId: string; options: ExportOptions }>({
      query: ({ conversationId, options }) => ({
        url: `/chat/conversations/${conversationId}/export`,
        method: 'POST',
        body: options,
      }),
    }),

    exportQueryResults: builder.mutation<{ downloadUrl: string }, { messageId: string; options: ExportOptions }>({
      query: ({ messageId, options }) => ({
        url: `/chat/messages/${messageId}/export`,
        method: 'POST',
        body: options,
      }),
    }),

    // Search and analytics
    searchMessages: builder.query<{ messages: ChatMessage[]; total: number }, {
      query: string
      conversationId?: string
      filters?: {
        type?: ChatMessage['type']
        dateRange?: { start: Date; end: Date }
        hasResults?: boolean
        isFavorite?: boolean
      }
      page?: number
      limit?: number
    }>({
      query: ({ query, conversationId, filters, page = 1, limit = 20 }) => {
        const params = new URLSearchParams({
          q: query,
          page: page.toString(),
          limit: limit.toString(),
        })
        
        if (conversationId) params.append('conversationId', conversationId)
        if (filters) {
          Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined) {
              params.append(key, typeof value === 'object' ? JSON.stringify(value) : String(value))
            }
          })
        }
        
        return `/chat/search?${params}`
      },
    }),

    getChatAnalytics: builder.query<{
      totalMessages: number
      totalConversations: number
      averageResponseTime: number
      popularQueries: string[]
      usageByDay: Array<{ date: string; count: number }>
      topTables: Array<{ table: string; count: number }>
    }, { timeframe?: 'day' | 'week' | 'month' | 'year' }>({
      query: ({ timeframe = 'month' }) => `/chat/analytics?timeframe=${timeframe}`,
    }),

    // Context and metadata
    getBusinessContext: builder.query<{
      tables: Array<{ name: string; description: string; relevance: number }>
      columns: Array<{ name: string; table: string; description: string }>
      glossaryTerms: Array<{ term: string; definition: string }>
    }, { query: string }>({
      query: ({ query }) => `/semantic-layer/business-schema?query=${encodeURIComponent(query)}`,
    }),

    updateConversationContext: builder.mutation<{ success: boolean }, { conversationId: string; context: any }>({
      query: ({ conversationId, context }) => ({
        url: `/chat/conversations/${conversationId}/context`,
        method: 'PUT',
        body: { context },
      }),
      invalidatesTags: (result, error, { conversationId }) => [{ type: 'Conversation', id: conversationId }],
    }),

    // Semantic Analysis
    analyzeQuerySemantics: builder.mutation<any, { query: string; intent?: string }>({
      query: (body) => ({
        url: '/semantic-layer/enhanced/analyze',
        method: 'POST',
        body,
      }),
    }),

    getRelevantGlossaryTerms: builder.query<Array<{ term: string; definition: string; confidence: number }>, { query: string; maxTerms?: number }>({
      query: ({ query, maxTerms = 10 }) => `/semantic-layer/enhanced/glossary/relevant?query=${encodeURIComponent(query)}&maxTerms=${maxTerms}`,
    }),

    generateLLMContext: builder.mutation<any, { query: string; intent: string }>({
      query: (body) => ({
        url: '/semantic-layer/enhanced/llm-context',
        method: 'POST',
        body,
      }),
    }),

    findSimilarSchemaElements: builder.mutation<any[], { element: any; threshold?: number; maxResults?: number }>({
      query: ({ element, threshold = 0.7, maxResults = 10 }) => ({
        url: '/semantic-layer/enhanced/similarity/find',
        method: 'POST',
        body: { element, threshold, maxResults },
      }),
    }),

    // Query History and Management
    refreshSchemaCache: builder.mutation<{ message: string; timestamp: string }, void>({
      query: () => ({
        url: '/query/refresh-schema',
        method: 'POST',
      }),
    }),
  }),
})

export const {
  // Conversation hooks
  useGetConversationsQuery,
  useGetConversationQuery,
  useCreateConversationMutation,
  useUpdateConversationMutation,
  useDeleteConversationMutation,
  
  // Message hooks
  useGetMessagesQuery,
  useSendMessageMutation,
  useUpdateMessageMutation,
  useDeleteMessageMutation,
  
  // Suggestion hooks
  useGetQuerySuggestionsQuery,
  useGetPopularQueriesQuery,
  useGetQueryTemplatesQuery,
  
  // Enhanced query execution
  useExecuteEnhancedQueryMutation,
  
  // Message actions
  useToggleMessageFavoriteMutation,
  useAddMessageReactionMutation,
  useShareMessageMutation,
  
  // Export hooks
  useExportConversationMutation,
  useExportQueryResultsMutation,
  
  // Search and analytics
  useSearchMessagesQuery,
  useGetChatAnalyticsQuery,
  
  // Context hooks
  useGetBusinessContextQuery,
  useUpdateConversationContextMutation,

  // Semantic Analysis hooks
  useAnalyzeQuerySemanticsMutation,
  useGetRelevantGlossaryTermsQuery,
  useGenerateLLMContextMutation,
  useFindSimilarSchemaElementsMutation,

  // Admin hooks
  useRefreshSchemaCacheMutation,
} = chatApi
