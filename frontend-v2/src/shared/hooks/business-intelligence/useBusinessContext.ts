import { useQuery, UseQueryResult } from '@tanstack/react-query'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

interface BusinessContextResponse {
  success: boolean
  data: {
    profile: BusinessContextProfile
    metadata: {
      processingTime: number
      modelVersion: string
      lastUpdated: string
    }
  }
  error?: any
}

interface UseBusinessContextReturn extends UseQueryResult<BusinessContextResponse> {
  businessContext: BusinessContextProfile | undefined
}

/**
 * Hook for fetching business context for a query
 *
 * NOTE: This is a temporary implementation that will be replaced with RTK Query
 * when the backend Business Intelligence API endpoints are implemented.
 *
 * For now, it provides a mock implementation to support the UI development.
 */
export const useBusinessContext = (
  query: string,
  userId: string = 'current-user', // TODO: Get from auth context
  options: {
    includeUserContext?: boolean
    enabled?: boolean
  } = {}
): UseBusinessContextReturn => {
  const { includeUserContext = true, enabled = true } = options

  const queryResult = useQuery({
    queryKey: ['business-context', query, userId, includeUserContext],
    queryFn: async (): Promise<BusinessContextResponse> => {
      if (!query?.trim()) {
        throw new Error('Query is required')
      }

      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 800))

      // Mock response for development
      const mockResponse: BusinessContextResponse = {
        success: true,
        data: {
          profile: {
            confidence: 0.89,
            intent: {
              type: 'aggregation',
              confidence: 0.92,
              complexity: 'moderate',
              description: 'User wants to aggregate data with grouping and time filtering',
              businessGoal: 'Analyze performance metrics over time'
            },
            entities: [],
            domain: {
              name: 'Business Analytics',
              description: 'Analysis of business performance and metrics',
              relevance: 0.94,
              concepts: ['Revenue', 'Performance', 'Analytics'],
              relationships: ['Sales connects to Revenue']
            },
            businessTerms: ['Sales', 'Revenue', 'Analytics']
          },
          metadata: {
            processingTime: 800,
            modelVersion: 'v2.1-mock',
            lastUpdated: new Date().toISOString()
          }
        }
      }

      return mockResponse
    },
    enabled: enabled && !!query?.trim(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
    retry: 1,
    retryDelay: 1000,
  })

  return {
    ...queryResult,
    businessContext: queryResult.data?.data.profile
  }
}
