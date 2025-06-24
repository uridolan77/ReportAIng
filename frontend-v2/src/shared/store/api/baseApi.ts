import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { RootState } from '../index'

const baseQuery = fetchBaseQuery({
  baseUrl: '/api',
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.accessToken
    if (token) {
      headers.set('authorization', `Bearer ${token}`)
    }
    headers.set('content-type', 'application/json')
    return headers
  },
})

const baseQueryWithReauth = async (args: any, api: any, extraOptions: any) => {
  let result = await baseQuery(args, api, extraOptions)

  if (result.error && result.error.status === 401) {
    const requestUrl = typeof args === 'string' ? args : args.url
    console.warn(`🔒 401 Unauthorized on ${requestUrl} - attempting token refresh`)

    // Don't try to refresh if this was already a refresh request
    if (requestUrl?.includes('/auth/refresh') || requestUrl?.includes('/auth/login')) {
      console.warn('🔒 401 on auth endpoint - logging out user')
      api.dispatch({ type: 'auth/logout' })
      return result
    }

    // Try to get a new token
    const refreshToken = (api.getState() as RootState).auth.refreshToken
    if (refreshToken) {
      console.log('🔄 Attempting token refresh...')
      const refreshResult = await baseQuery(
        {
          url: '/auth/refresh',
          method: 'POST',
          body: { refreshToken },
        },
        api,
        extraOptions
      )

      if (refreshResult.data) {
        console.log('✅ Token refresh successful - retrying original request')
        // Store the new token
        api.dispatch({
          type: 'auth/updateTokens',
          payload: refreshResult.data,
        })

        // Retry the original query
        result = await baseQuery(args, api, extraOptions)
      } else {
        console.warn('❌ Token refresh failed - logging out user')
        // Refresh failed, logout user
        api.dispatch({ type: 'auth/logout' })
      }
    } else {
      console.warn('❌ No refresh token available - logging out user')
      // No refresh token, logout user
      api.dispatch({ type: 'auth/logout' })
    }
  }

  return result
}

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: baseQueryWithReauth,
  tagTypes: [
    'User',
    'Query',
    'QueryHistory',
    'BusinessTable',
    'BusinessColumn',
    'BusinessMetadata',
    'GlossaryTerm',
    'Schema',
    'SystemConfig',
    'Analytics',
    'CostAnalytics',
    'CostHistory',
    'Budget',
    'CostRecommendations',
    'Performance',
    'PerformanceMetrics',
    'Benchmarks',
    'TuningDashboard',
    'TuningTable',
    'QueryPattern',
    'PromptTemplate',
    'StreamingSession',
    'Dashboard',
    'Conversation',
    'Message',
    'AIProvider',
    'SecuritySettings',
    'UserSessions',
    // LLM Management
    'LLMProvider',
    'LLMProviderHealth',
    'LLMModel',
    'LLMUsage',
    'LLMCost',
    'LLMPerformance',
    'LLMDashboard',
    // Enhanced Business Metadata
    'EnhancedBusinessTable',
    'EnhancedBusinessMetadata',
    'EnhancedGlossary',
    'EnhancedDomain',
    // Template Analytics
    'TemplatePerformance',
    'TemplateMetrics',
    'PerformanceAlert',
    'PerformanceTrend',
    'ABTest',
    'ABTestAnalysis',
    'ABTestRecommendation',
    'TemplateManagement',
    'TemplateVersion',
    'TemplateQuality',
    // Template Improvement
    'TemplateImprovement',
    'ImprovementSuggestion',
    'OptimizedTemplate',
    'PerformancePrediction',
    'TemplateVariant',
    'ContentQuality',
  ],
  endpoints: () => ({}),
})
