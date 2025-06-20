/**
 * Enhanced API Hooks
 * 
 * Provides enhanced versions of API hooks that automatically handle
 * fallback to mock data when real APIs are unavailable.
 */

import { useQuery, useMutation, UseQueryOptions, UseMutationOptions } from '@tanstack/react-query'
import { apiCall } from '../services/apiToggleService'
import { MockDataService } from '../services/mockDataService'
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

// Enhanced Business API Hooks
export function useEnhancedBusinessTables(options?: UseQueryOptions<BusinessTableInfoDto[]>) {
  return useQuery({
    queryKey: ['businessTables'],
    queryFn: () => apiCall(
      // Real API call (would use RTK Query)
      async () => {
        const response = await fetch('/api/business/tables')
        if (!response.ok) throw new Error('Failed to fetch business tables')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getBusinessTables(),
      'business/tables'
    ),
    ...options
  })
}

export function useEnhancedBusinessGlossary(options?: UseQueryOptions<{ terms: BusinessGlossaryDto[]; total: number }>) {
  return useQuery({
    queryKey: ['businessGlossary'],
    queryFn: () => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/business/glossary')
        if (!response.ok) throw new Error('Failed to fetch business glossary')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getBusinessGlossary(),
      'business/glossary'
    ),
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

export function useEnhancedSchemaTables(options?: UseQueryOptions<any[]>) {
  return useQuery({
    queryKey: ['schemaTables'],
    queryFn: () => apiCall(
      // Real API call
      async () => {
        const response = await fetch('/api/schema/tables')
        if (!response.ok) throw new Error('Failed to fetch schema tables')
        return response.json()
      },
      // Mock data fallback
      () => MockDataService.getAllSchemaTables(),
      'schema/tables'
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
