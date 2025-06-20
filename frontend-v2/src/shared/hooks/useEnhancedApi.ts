/**
 * Enhanced API Hooks
 * 
 * Provides enhanced versions of API hooks that automatically handle
 * fallback to mock data when real APIs are unavailable.
 */

import { useQuery, useMutation, UseQueryOptions, UseMutationOptions, useQueryClient } from '@tanstack/react-query'
import { apiCall, ApiToggleService } from '../services/apiToggleService'
import { MockDataService } from '../services/mockDataService'
import { useEffect, useState } from 'react'
import { useAppSelector } from '../hooks'
import { selectAccessToken } from '../store/auth'
import type { 
  BusinessTableInfoDto, 
  BusinessGlossaryDto 
} from '../store/api/businessApi'
import type { 
  AuthenticationResult, 
  UserInfo 
} from '../store/api/authApi'
import type { 
  EnhancedQueryResponse, 
  QueryHistoryItem 
} from '../store/api/queryApi'

// Helper function for authenticated API calls
function createAuthenticatedFetch(token?: string) {
  return async (url: string, options: RequestInit = {}) => {
    const headers = new Headers(options.headers)
    headers.set('Content-Type', 'application/json')

    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }

    const response = await fetch(url, {
      ...options,
      headers
    })

    if (!response.ok) {
      throw new Error(`API call failed: ${response.status} ${response.statusText}`)
    }

    return response.json()
  }
}

// Hook to manage API mode changes and query invalidation
function useApiModeSync() {
  const queryClient = useQueryClient()
  const [currentMode, setCurrentMode] = useState(ApiToggleService.getConfig().useMockData)

  useEffect(() => {
    const unsubscribe = ApiToggleService.addListener((config) => {
      if (config.useMockData !== currentMode) {
        setCurrentMode(config.useMockData)
        // Invalidate all queries when API mode changes
        queryClient.invalidateQueries()
        console.log(`🔄 API mode changed to ${config.useMockData ? 'Mock' : 'Real'}, invalidating all queries`)
      }
    })

    return unsubscribe
  }, [queryClient, currentMode])

  return currentMode
}

// Enhanced Business API Hooks
export function useEnhancedBusinessTables(options?: UseQueryOptions<BusinessTableInfoDto[]>) {
  const apiMode = useApiModeSync() // This will trigger query invalidation on mode change
  const token = useAppSelector(selectAccessToken)

  return useQuery({
    queryKey: ['businessTables'],
    queryFn: async () => {
      console.log(`🔍 Fetching business tables in ${apiMode ? 'Mock' : 'Real API'} mode`)
      const authenticatedFetch = createAuthenticatedFetch(token)

      return apiCall(
        // Real API call with authentication
        async () => {
          console.log('📡 Making authenticated API call to /api/business/tables')
          const data = await authenticatedFetch('/api/business/tables')
          console.log('📡 Real API response:', data)
          return data
        },
        // Mock data fallback
        async () => {
          console.log('🎭 Using mock data for business tables')
          const data = await MockDataService.getBusinessTables()
          console.log('🎭 Mock data:', data)
          return data
        },
        'business/tables'
      )
    },
    ...options
  })
}

export function useEnhancedBusinessGlossary(options?: UseQueryOptions<{ terms: BusinessGlossaryDto[]; total: number }>) {
  useApiModeSync() // Sync with API mode changes
  const token = useAppSelector(selectAccessToken)

  return useQuery({
    queryKey: ['businessGlossary'],
    queryFn: () => {
      const authenticatedFetch = createAuthenticatedFetch(token)

      return apiCall(
        // Real API call with authentication
        async () => {
          return await authenticatedFetch('/api/business/glossary')
        },
        // Mock data fallback
        () => MockDataService.getBusinessGlossary(),
        'business/glossary'
      )
    },
    ...options
  })
}

// Enhanced Business Metadata Management Hooks
export function useEnhancedBusinessMetadataStatus(options?: UseQueryOptions<any>) {
  useApiModeSync() // Sync with API mode changes
  const token = useAppSelector(selectAccessToken)

  return useQuery({
    queryKey: ['businessMetadataStatus'],
    queryFn: () => {
      const authenticatedFetch = createAuthenticatedFetch(token)

      return apiCall(
        // Real API call with authentication
        async () => {
          return await authenticatedFetch('/api/business-metadata/status')
        },
        // Mock data fallback
        () => MockDataService.getBusinessMetadataStatus(),
        'business-metadata/status'
      )
    },
    ...options
  })
}

export function useEnhancedSchemaTables(options?: UseQueryOptions<any[]>) {
  useApiModeSync() // Sync with API mode changes
  const token = useAppSelector(selectAccessToken)

  return useQuery({
    queryKey: ['schemaTables'],
    queryFn: () => {
      const authenticatedFetch = createAuthenticatedFetch(token)

      return apiCall(
        // Real API call with authentication
        async () => {
          return await authenticatedFetch('/api/schema/tables')
        },
        // Mock data fallback
        () => MockDataService.getAllSchemaTables(),
        'schema/tables'
      )
    },
    ...options
  })
}

export function useEnhancedQueryHistory(options?: UseQueryOptions<{ queries: QueryHistoryItem[]; total: number; page: number }>) {
  return useQuery({
    queryKey: ['queryHistory'],
    queryFn: () => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/query/history')
        if (!response.ok) throw new Error('Failed to fetch query history')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getQueryHistory(),
      'query/history'
    ),
    ...options
  })
}

export function useEnhancedSystemStatistics(options?: UseQueryOptions<any>) {
  return useQuery({
    queryKey: ['systemStatistics'],
    queryFn: () => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/admin/statistics')
        if (!response.ok) throw new Error('Failed to fetch system statistics')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getSystemStatistics(),
      'admin/statistics'
    ),
    ...options
  })
}

export function useEnhancedCostMetrics(timeRange: string, options?: UseQueryOptions<any>) {
  return useQuery({
    queryKey: ['costMetrics', timeRange],
    queryFn: () => apiCall(
      // Real API call
      async () => {
        const response = await fetch(`/api/cost/metrics?timeRange=${timeRange}`)
        if (!response.ok) throw new Error('Failed to fetch cost metrics')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getCostMetrics(timeRange),
      `cost/metrics?timeRange=${timeRange}`
    ),
    ...options
  })
}

// Enhanced Mutation Hooks
export function useEnhancedLogin(options?: UseMutationOptions<AuthenticationResult, Error, { username: string; password: string }>) {
  return useMutation({
    mutationFn: ({ username, password }) => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/auth/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ username, password })
        })
        if (!response.ok) {
          const errorData = await response.json().catch(() => ({}))
          throw new Error(errorData.message || `Login failed with status ${response.status}`)
        }
        return response.json()
      },
      // Mock data fallback - always succeeds for development
      () => MockDataService.login(username, password),
      'auth/login'
    ),
    ...options
  })
}

// Quick login hook for development
export function useQuickLogin(options?: UseMutationOptions<AuthenticationResult, Error, void>) {
  return useMutation({
    mutationFn: () => MockDataService.quickLogin(),
    ...options
  })
}

export function useEnhancedQueryExecution(options?: UseMutationOptions<EnhancedQueryResponse, Error, string>) {
  return useMutation({
    mutationFn: (query: string) => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/query/enhanced', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ 
            query, 
            includeExplanation: true, 
            optimizeForPerformance: false 
          })
        })
        if (!response.ok) throw new Error('Query execution failed')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.executeEnhancedQuery(query),
      'query/enhanced'
    ),
    ...options
  })
}

// Utility hook for current user with enhanced fallback
export function useEnhancedCurrentUser(options?: UseQueryOptions<UserInfo>) {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: () => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/user/profile')
        if (!response.ok) throw new Error('Failed to fetch current user')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getCurrentUser(),
      'user/profile'
    ),
    ...options
  })
}

// Hook for API status monitoring
export function useApiStatus() {
  return useQuery({
    queryKey: ['apiStatus'],
    queryFn: async () => {
      try {
        const response = await fetch('/api/health', { 
          method: 'GET',
          signal: AbortSignal.timeout(5000) // 5 second timeout
        })
        return {
          isOnline: response.ok,
          status: response.status,
          timestamp: new Date().toISOString()
        }
      } catch (error) {
        return {
          isOnline: false,
          status: 0,
          error: error instanceof Error ? error.message : 'Unknown error',
          timestamp: new Date().toISOString()
        }
      }
    },
    refetchInterval: 30000, // Check every 30 seconds
    retry: false
  })
}

// Development helper hook
export function useApiDebugInfo() {
  const apiStatus = useApiStatus()
  
  return {
    ...apiStatus,
    mockDataAvailable: MockDataService.isEnabled(),
    environment: import.meta.env.MODE,
    buildTime: import.meta.env.VITE_BUILD_TIME || 'unknown'
  }
}
