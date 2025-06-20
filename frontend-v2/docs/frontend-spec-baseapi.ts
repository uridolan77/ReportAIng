// services/api/baseApi.ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token
      if (token) {
        headers.set('authorization', `Bearer ${token}`)
      }
      return headers
    },
  }),
  tagTypes: [
    'CostAnalytics', 'Budget', 'CostHistory', 'CostRecommendations',
    'Performance', 'Benchmarks', 'PerformanceMetrics',
    'CacheStats', 'CacheConfig', 'CacheRecommendations',
    'ResourceQuota', 'ResourceUsage', 'CircuitBreaker',
    'ModelSelection', 'ModelPerformance', 'ModelCapabilities'
  ],
  endpoints: () => ({}),
})
