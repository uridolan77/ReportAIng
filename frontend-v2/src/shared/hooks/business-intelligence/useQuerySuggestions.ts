import { useQuery, UseQueryResult } from '@tanstack/react-query'

interface QuerySuggestion {
  id: string
  query: string
  category: 'popular' | 'recent' | 'recommended' | 'similar'
  confidence: number
  description: string
  estimatedComplexity: 'simple' | 'moderate' | 'complex'
  tags: string[]
}

interface QuerySuggestionsResponse {
  success: boolean
  data: {
    suggestions: QuerySuggestion[]
    categories: string[]
    totalSuggestions: number
  }
  error?: any
}

interface UseQuerySuggestionsReturn extends UseQueryResult<QuerySuggestionsResponse> {
  suggestions: QuerySuggestion[]
}

/**
 * Hook for fetching query suggestions
 *
 * NOTE: This is a temporary implementation that will be replaced with RTK Query
 * when the backend Business Intelligence API endpoints are implemented.
 *
 * For now, it provides mock suggestions to support the UI development.
 */
export const useQuerySuggestions = (
  userId: string = 'current-user', // TODO: Get from auth context
  options: {
    limit?: number
    category?: string
    context?: string
    enabled?: boolean
  } = {}
): UseQuerySuggestionsReturn => {
  const {
    limit = 10,
    category,
    context,
    enabled = true
  } = options

  const queryResult = useQuery({
    queryKey: ['query-suggestions', userId, limit, category, context],
    queryFn: async (): Promise<QuerySuggestionsResponse> => {
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 500))

      // Mock suggestions for development
      const mockSuggestions: QuerySuggestion[] = [
        {
          id: 'sugg-001',
          query: 'Show me quarterly sales by region for the last year',
          category: 'popular',
          confidence: 0.95,
          description: 'Popular query for regional sales analysis',
          estimatedComplexity: 'moderate',
          tags: ['sales', 'region', 'quarterly']
        },
        {
          id: 'sugg-002',
          query: 'What are the top 10 customers by revenue this month?',
          category: 'popular',
          confidence: 0.92,
          description: 'Frequently used customer ranking query',
          estimatedComplexity: 'simple',
          tags: ['customers', 'revenue', 'ranking']
        },
        {
          id: 'sugg-003',
          query: 'Compare product performance year over year',
          category: 'recommended',
          confidence: 0.88,
          description: 'Recommended for product analysis',
          estimatedComplexity: 'moderate',
          tags: ['products', 'performance', 'comparison']
        },
        {
          id: 'sugg-004',
          query: 'Show monthly trends for customer acquisition',
          category: 'recent',
          confidence: 0.85,
          description: 'Recently used trend analysis',
          estimatedComplexity: 'moderate',
          tags: ['customers', 'acquisition', 'trends']
        },
        {
          id: 'sugg-005',
          query: 'What is the average order value by customer segment?',
          category: 'popular',
          confidence: 0.90,
          description: 'Popular segmentation analysis',
          estimatedComplexity: 'simple',
          tags: ['orders', 'segments', 'average']
        }
      ]

      const filteredSuggestions = category
        ? mockSuggestions.filter(s => s.category === category)
        : mockSuggestions

      const limitedSuggestions = filteredSuggestions.slice(0, limit)

      const mockResponse: QuerySuggestionsResponse = {
        success: true,
        data: {
          suggestions: limitedSuggestions,
          categories: ['popular', 'recent', 'recommended', 'similar'],
          totalSuggestions: mockSuggestions.length
        }
      }

      return mockResponse
    },
    enabled,
    staleTime: 2 * 60 * 1000, // 2 minutes
    cacheTime: 5 * 60 * 1000, // 5 minutes
    retry: 1,
    retryDelay: 1000,
  })

  return {
    ...queryResult,
    suggestions: queryResult.data?.data.suggestions || []
  }
}
