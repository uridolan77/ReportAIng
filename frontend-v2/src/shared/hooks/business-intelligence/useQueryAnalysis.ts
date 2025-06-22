import { useState, useCallback } from 'react'
import { message } from 'antd'
import type {
  QueryAnalysisRequest,
  QueryAnalysisResponse,
  BusinessIntelligenceError
} from '@shared/types/business-intelligence'

interface UseQueryAnalysisReturn {
  analyzeQuery: (request: QueryAnalysisRequest) => Promise<QueryAnalysisResponse>
  isAnalyzing: boolean
  error: BusinessIntelligenceError | null
  lastAnalysis: QueryAnalysisResponse | null
  clearError: () => void
}

/**
 * Hook for analyzing natural language queries
 *
 * NOTE: This is a temporary implementation that will be replaced with RTK Query
 * when the backend Business Intelligence API endpoints are implemented.
 *
 * For now, it provides a mock implementation to support the UI development.
 */
export const useQueryAnalysis = (): UseQueryAnalysisReturn => {
  const [isAnalyzing, setIsAnalyzing] = useState(false)
  const [error, setError] = useState<BusinessIntelligenceError | null>(null)
  const [lastAnalysis, setLastAnalysis] = useState<QueryAnalysisResponse | null>(null)

  const analyzeQuery = useCallback(async (request: QueryAnalysisRequest): Promise<QueryAnalysisResponse> => {
    setIsAnalyzing(true)
    setError(null)

    try {
      // Validate request
      if (!request.query?.trim()) {
        throw new Error('Query is required')
      }

      if (!request.userId) {
        throw new Error('User ID is required')
      }

      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1500))

      // Mock successful response for development
      const mockResponse: QueryAnalysisResponse = {
        success: true,
        data: {
          queryId: `query-${Date.now()}`,
          analysisTimestamp: new Date().toISOString(),
          processingTimeMs: 1500,
          businessContext: {
            confidence: 0.89,
            intent: {
              type: 'aggregation',
              confidence: 0.92,
              complexity: 'moderate',
              description: 'User wants to aggregate data with grouping and time filtering',
              businessGoal: 'Analyze performance metrics over time',
              subIntents: ['time_analysis', 'aggregation'],
              reasoning: [
                'Query contains aggregation keywords',
                'Time dimension identified',
                'Grouping pattern detected'
              ]
            },
            entities: [
              {
                id: 'ent-001',
                name: 'sales',
                type: 'metric',
                startPosition: 0,
                endPosition: 5,
                confidence: 0.95,
                businessMeaning: 'Revenue generated from transactions',
                context: 'Primary business metric',
                suggestedColumns: ['amount', 'revenue', 'total_sales'],
                relatedTables: ['sales_fact', 'revenue_summary']
              }
            ],
            domain: {
              name: 'Business Analytics',
              description: 'Analysis of business performance and metrics',
              relevance: 0.94,
              concepts: ['Revenue', 'Performance', 'Analytics'],
              relationships: ['Sales connects to Revenue', 'Time connects to Trends']
            },
            businessTerms: ['Sales', 'Revenue', 'Analytics'],
            timeContext: {
              period: 'quarterly',
              granularity: 'quarter',
              relativeTo: 'current date'
            }
          },
          suggestedQueries: [
            {
              id: 'sugg-001',
              query: 'Show me monthly sales trends for this year',
              category: 'similar',
              confidence: 0.85,
              description: 'Similar analysis with different time granularity',
              estimatedComplexity: 'moderate',
              tags: ['sales', 'trends', 'monthly']
            }
          ],
          estimatedExecutionTime: 2.3,
          dataQualityScore: 0.85
        }
      }

      setLastAnalysis(mockResponse)

      // Log successful analysis for analytics
      console.log('Query analysis completed (MOCK):', {
        queryId: mockResponse.data.queryId,
        confidence: mockResponse.data.businessContext.confidence,
        processingTime: mockResponse.data.processingTimeMs
      })

      message.success('Query analyzed successfully (using mock data)')
      return mockResponse

    } catch (err: any) {
      const businessError: BusinessIntelligenceError = {
        code: 'ANALYSIS_FAILED',
        message: err.message || 'Failed to analyze query',
        details: {},
        timestamp: new Date().toISOString(),
        requestId: `req-${Date.now()}`
      }

      setError(businessError)
      message.error('Analysis failed. Please try again.')
      throw businessError
    } finally {
      setIsAnalyzing(false)
    }
  }, [])

  const clearError = useCallback(() => {
    setError(null)
  }, [])

  return {
    analyzeQuery,
    isAnalyzing,
    error,
    lastAnalysis,
    clearError
  }
}
