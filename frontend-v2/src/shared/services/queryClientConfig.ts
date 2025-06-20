/**
 * Enhanced QueryClient Configuration for BI Applications
 * 
 * Provides intelligent caching strategies, background synchronization,
 * and performance optimizations for data-heavy BI operations.
 */

import { QueryClient, QueryKey } from '@tanstack/react-query'

// Cache tier definitions for different data types
export type CacheTier = 'real-time' | 'metrics' | 'reports' | 'static' | 'user-data'

export interface CacheConfig {
  staleTime: number
  cacheTime: number
  refetchInterval?: number
  refetchOnWindowFocus?: boolean
  refetchOnReconnect?: boolean
}

// Intelligent cache configuration based on data type
export const cacheConfigs: Record<CacheTier, CacheConfig> = {
  'real-time': {
    staleTime: 30 * 1000, // 30 seconds
    cacheTime: 2 * 60 * 1000, // 2 minutes
    refetchInterval: 30 * 1000, // Auto-refresh every 30 seconds
    refetchOnWindowFocus: true,
    refetchOnReconnect: true,
  },
  'metrics': {
    staleTime: 2 * 60 * 1000, // 2 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
    refetchInterval: 2 * 60 * 1000, // Auto-refresh every 2 minutes
    refetchOnWindowFocus: true,
    refetchOnReconnect: true,
  },
  'reports': {
    staleTime: 10 * 60 * 1000, // 10 minutes
    cacheTime: 60 * 60 * 1000, // 1 hour
    refetchOnWindowFocus: false,
    refetchOnReconnect: true,
  },
  'static': {
    staleTime: 60 * 60 * 1000, // 1 hour
    cacheTime: 24 * 60 * 60 * 1000, // 24 hours
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  },
  'user-data': {
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 30 * 60 * 1000, // 30 minutes
    refetchOnWindowFocus: true,
    refetchOnReconnect: true,
  },
}

// Determine cache tier from query key
export const getCacheTier = (queryKey: QueryKey): CacheTier => {
  const key = Array.isArray(queryKey) ? queryKey[0] : queryKey
  
  if (typeof key !== 'string') return 'metrics'
  
  // Real-time data patterns
  if (key.includes('real-time') || key.includes('live') || key.includes('streaming')) {
    return 'real-time'
  }
  
  // Metrics and analytics
  if (key.includes('metrics') || key.includes('analytics') || key.includes('dashboard')) {
    return 'metrics'
  }
  
  // Reports and historical data
  if (key.includes('reports') || key.includes('history') || key.includes('export')) {
    return 'reports'
  }
  
  // Static configuration data
  if (key.includes('config') || key.includes('schema') || key.includes('metadata')) {
    return 'static'
  }
  
  // User-specific data
  if (key.includes('user') || key.includes('profile') || key.includes('preferences')) {
    return 'user-data'
  }
  
  // Default to metrics
  return 'metrics'
}

// Enhanced QueryClient with intelligent caching
export const createBIQueryClient = (): QueryClient => {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Dynamic configuration based on query key
        staleTime: (query) => {
          const tier = getCacheTier(query.queryKey)
          return cacheConfigs[tier].staleTime
        },
        cacheTime: (query) => {
          const tier = getCacheTier(query.queryKey)
          return cacheConfigs[tier].cacheTime
        },
        refetchOnWindowFocus: (query) => {
          const tier = getCacheTier(query.queryKey)
          return cacheConfigs[tier].refetchOnWindowFocus ?? false
        },
        refetchOnReconnect: (query) => {
          const tier = getCacheTier(query.queryKey)
          return cacheConfigs[tier].refetchOnReconnect ?? true
        },
        
        // Error handling
        retry: (failureCount, error: any) => {
          // Don't retry on 4xx errors (client errors)
          if (error?.status >= 400 && error?.status < 500) {
            return false
          }
          // Retry up to 3 times for other errors
          return failureCount < 3
        },
        
        retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
        
        // Performance optimizations
        structuralSharing: true,
        keepPreviousData: true,
      },
      
      mutations: {
        retry: 1,
        retryDelay: 1000,
      },
    },
    
    // Query cache configuration
    queryCache: {
      onError: (error, query) => {
        console.error(`Query failed [${query.queryKey.join(', ')}]:`, error)
      },
      onSuccess: (data, query) => {
        if (import.meta.env.DEV) {
          console.log(`Query succeeded [${query.queryKey.join(', ')}]:`, data)
        }
      },
    },
    
    // Mutation cache configuration
    mutationCache: {
      onError: (error, variables, context, mutation) => {
        console.error(`Mutation failed:`, error)
      },
      onSuccess: (data, variables, context, mutation) => {
        if (import.meta.env.DEV) {
          console.log(`Mutation succeeded:`, data)
        }
      },
    },
  })
}

// Background sync utilities
export class BackgroundSync {
  private static intervals: Map<string, NodeJS.Timeout> = new Map()
  
  static startSync(queryClient: QueryClient, queryKey: QueryKey, intervalMs: number) {
    const keyString = JSON.stringify(queryKey)
    
    // Clear existing interval if any
    this.stopSync(keyString)
    
    // Start new interval
    const interval = setInterval(() => {
      queryClient.invalidateQueries({ queryKey })
    }, intervalMs)
    
    this.intervals.set(keyString, interval)
  }
  
  static stopSync(keyString: string) {
    const interval = this.intervals.get(keyString)
    if (interval) {
      clearInterval(interval)
      this.intervals.delete(keyString)
    }
  }
  
  static stopAllSync() {
    this.intervals.forEach((interval) => clearInterval(interval))
    this.intervals.clear()
  }
}

// Query key factories for consistent naming
export const queryKeys = {
  // Dashboard queries
  dashboard: {
    all: ['dashboard'] as const,
    lists: () => [...queryKeys.dashboard.all, 'list'] as const,
    list: (filters: Record<string, unknown>) => [...queryKeys.dashboard.lists(), filters] as const,
    details: () => [...queryKeys.dashboard.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.dashboard.details(), id] as const,
  },
  
  // Metrics queries
  metrics: {
    all: ['metrics'] as const,
    system: () => [...queryKeys.metrics.all, 'system'] as const,
    cost: (timeRange: string) => [...queryKeys.metrics.all, 'cost', timeRange] as const,
    performance: (entityType: string, entityId: string) => 
      [...queryKeys.metrics.all, 'performance', entityType, entityId] as const,
  },
  
  // Real-time queries
  realTime: {
    all: ['real-time'] as const,
    dashboard: () => [...queryKeys.realTime.all, 'dashboard'] as const,
    queries: () => [...queryKeys.realTime.all, 'queries'] as const,
    health: () => [...queryKeys.realTime.all, 'health'] as const,
  },
  
  // User queries
  user: {
    all: ['user'] as const,
    profile: () => [...queryKeys.user.all, 'profile'] as const,
    preferences: () => [...queryKeys.user.all, 'preferences'] as const,
  },
} as const

// Performance monitoring utilities
export const performanceMonitor = {
  logSlowQueries: (threshold: number = 1000) => {
    return {
      onSuccess: (data: unknown, query: any) => {
        const duration = Date.now() - query.state.dataUpdatedAt
        if (duration > threshold) {
          console.warn(`Slow query detected [${query.queryKey.join(', ')}]: ${duration}ms`)
        }
      },
    }
  },
  
  trackCacheHitRate: () => {
    let hits = 0
    let misses = 0
    
    return {
      onSuccess: (data: unknown, query: any) => {
        if (query.state.isFetching) {
          misses++
        } else {
          hits++
        }
        
        if ((hits + misses) % 100 === 0) {
          const hitRate = (hits / (hits + misses)) * 100
          console.log(`Cache hit rate: ${hitRate.toFixed(1)}%`)
        }
      },
    }
  },
}

export default createBIQueryClient
