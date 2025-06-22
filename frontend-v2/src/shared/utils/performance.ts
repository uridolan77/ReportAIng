import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { debounce, throttle } from 'lodash'

/**
 * Performance optimization utilities for transparency components
 */

// Virtual scrolling hook for large datasets
export const useVirtualScrolling = <T>(
  items: T[],
  itemHeight: number,
  containerHeight: number,
  overscan: number = 5
) => {
  const [scrollTop, setScrollTop] = useState(0)
  const scrollElementRef = useRef<HTMLDivElement>(null)

  const visibleRange = useMemo(() => {
    const startIndex = Math.max(0, Math.floor(scrollTop / itemHeight) - overscan)
    const endIndex = Math.min(
      items.length - 1,
      Math.ceil((scrollTop + containerHeight) / itemHeight) + overscan
    )
    return { startIndex, endIndex }
  }, [scrollTop, itemHeight, containerHeight, items.length, overscan])

  const visibleItems = useMemo(() => {
    return items.slice(visibleRange.startIndex, visibleRange.endIndex + 1).map((item, index) => ({
      item,
      index: visibleRange.startIndex + index,
      style: {
        position: 'absolute' as const,
        top: (visibleRange.startIndex + index) * itemHeight,
        height: itemHeight,
        width: '100%'
      }
    }))
  }, [items, visibleRange, itemHeight])

  const totalHeight = items.length * itemHeight

  const handleScroll = useCallback(
    throttle((e: React.UIEvent<HTMLDivElement>) => {
      setScrollTop(e.currentTarget.scrollTop)
    }, 16), // ~60fps
    []
  )

  return {
    scrollElementRef,
    visibleItems,
    totalHeight,
    handleScroll,
    scrollTop
  }
}

// Debounced search hook
export const useDebouncedSearch = (
  searchFunction: (query: string) => void,
  delay: number = 300
) => {
  const [searchQuery, setSearchQuery] = useState('')
  
  const debouncedSearch = useMemo(
    () => debounce(searchFunction, delay),
    [searchFunction, delay]
  )

  useEffect(() => {
    if (searchQuery) {
      debouncedSearch(searchQuery)
    }
    return () => {
      debouncedSearch.cancel()
    }
  }, [searchQuery, debouncedSearch])

  return { searchQuery, setSearchQuery }
}

// Memory-efficient data caching
export class DataCache<T> {
  private cache = new Map<string, { data: T; timestamp: number; ttl: number }>()
  private maxSize: number
  private defaultTTL: number

  constructor(maxSize: number = 100, defaultTTL: number = 5 * 60 * 1000) {
    this.maxSize = maxSize
    this.defaultTTL = defaultTTL
  }

  set(key: string, data: T, ttl?: number): void {
    // Remove oldest entries if cache is full
    if (this.cache.size >= this.maxSize) {
      const oldestKey = this.cache.keys().next().value
      this.cache.delete(oldestKey)
    }

    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      ttl: ttl || this.defaultTTL
    })
  }

  get(key: string): T | null {
    const entry = this.cache.get(key)
    if (!entry) return null

    // Check if entry has expired
    if (Date.now() - entry.timestamp > entry.ttl) {
      this.cache.delete(key)
      return null
    }

    return entry.data
  }

  has(key: string): boolean {
    return this.get(key) !== null
  }

  clear(): void {
    this.cache.clear()
  }

  size(): number {
    return this.cache.size
  }

  // Clean up expired entries
  cleanup(): void {
    const now = Date.now()
    for (const [key, entry] of this.cache.entries()) {
      if (now - entry.timestamp > entry.ttl) {
        this.cache.delete(key)
      }
    }
  }
}

// Global cache instances
export const transparencyCache = new DataCache(200, 10 * 60 * 1000) // 10 minutes TTL
export const metricsCache = new DataCache(50, 5 * 60 * 1000) // 5 minutes TTL

// Intersection Observer hook for lazy loading
export const useIntersectionObserver = (
  callback: () => void,
  options: IntersectionObserverInit = {}
) => {
  const targetRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const target = targetRef.current
    if (!target) return

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          callback()
        }
      },
      {
        threshold: 0.1,
        ...options
      }
    )

    observer.observe(target)

    return () => {
      observer.unobserve(target)
    }
  }, [callback, options])

  return targetRef
}

// Performance monitoring hook
export const usePerformanceMonitor = (componentName: string) => {
  const renderStartTime = useRef<number>(Date.now())
  const [renderTime, setRenderTime] = useState<number>(0)

  useEffect(() => {
    const endTime = Date.now()
    const duration = endTime - renderStartTime.current
    setRenderTime(duration)

    // Log slow renders in development
    if (process.env.NODE_ENV === 'development' && duration > 100) {
      console.warn(`Slow render detected in ${componentName}: ${duration}ms`)
    }
  })

  const markRenderStart = useCallback(() => {
    renderStartTime.current = Date.now()
  }, [])

  return { renderTime, markRenderStart }
}

// Memoization utilities
export const createMemoizedSelector = <T, R>(
  selector: (data: T) => R,
  equalityFn?: (a: R, b: R) => boolean
) => {
  let lastInput: T
  let lastResult: R
  let hasResult = false

  return (input: T): R => {
    if (!hasResult || input !== lastInput) {
      const newResult = selector(input)
      
      if (!hasResult || !equalityFn || !equalityFn(lastResult, newResult)) {
        lastResult = newResult
        lastInput = input
        hasResult = true
      }
    }
    
    return lastResult
  }
}

// Batch processing for large operations
export const batchProcess = async <T, R>(
  items: T[],
  processor: (item: T) => Promise<R>,
  batchSize: number = 10,
  onProgress?: (completed: number, total: number) => void
): Promise<R[]> => {
  const results: R[] = []
  
  for (let i = 0; i < items.length; i += batchSize) {
    const batch = items.slice(i, i + batchSize)
    const batchResults = await Promise.all(batch.map(processor))
    results.push(...batchResults)
    
    onProgress?.(Math.min(i + batchSize, items.length), items.length)
    
    // Allow other tasks to run
    await new Promise(resolve => setTimeout(resolve, 0))
  }
  
  return results
}

// Memory usage monitoring
export const getMemoryUsage = (): {
  used: number
  total: number
  percentage: number
} => {
  if ('memory' in performance) {
    const memory = (performance as any).memory
    return {
      used: memory.usedJSHeapSize,
      total: memory.totalJSHeapSize,
      percentage: (memory.usedJSHeapSize / memory.totalJSHeapSize) * 100
    }
  }
  
  return { used: 0, total: 0, percentage: 0 }
}

// Component size optimization
export const useComponentSize = () => {
  const [size, setSize] = useState({ width: 0, height: 0 })
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!ref.current) return

    const resizeObserver = new ResizeObserver((entries) => {
      const entry = entries[0]
      if (entry) {
        setSize({
          width: entry.contentRect.width,
          height: entry.contentRect.height
        })
      }
    })

    resizeObserver.observe(ref.current)

    return () => {
      resizeObserver.disconnect()
    }
  }, [])

  return { ref, size }
}

// Efficient data filtering
export const useOptimizedFilter = <T>(
  data: T[],
  filterFn: (item: T) => boolean,
  dependencies: any[] = []
) => {
  return useMemo(() => {
    return data.filter(filterFn)
  }, [data, ...dependencies])
}

// Lazy component loading
export const useLazyComponent = <T extends React.ComponentType<any>>(
  importFn: () => Promise<{ default: T }>,
  fallback?: React.ComponentType
) => {
  const [Component, setComponent] = useState<T | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<Error | null>(null)

  const loadComponent = useCallback(async () => {
    if (Component || loading) return

    setLoading(true)
    setError(null)

    try {
      const module = await importFn()
      setComponent(() => module.default)
    } catch (err) {
      setError(err as Error)
    } finally {
      setLoading(false)
    }
  }, [Component, loading, importFn])

  return { Component, loading, error, loadComponent }
}

// Performance metrics collection
export interface PerformanceMetrics {
  renderTime: number
  memoryUsage: number
  cacheHitRate: number
  componentCount: number
}

export const collectPerformanceMetrics = (): PerformanceMetrics => {
  const memory = getMemoryUsage()
  
  return {
    renderTime: performance.now(),
    memoryUsage: memory.percentage,
    cacheHitRate: transparencyCache.size() > 0 ? 0.85 : 0, // Simulated
    componentCount: document.querySelectorAll('[data-testid]').length
  }
}

// Export all utilities
export default {
  useVirtualScrolling,
  useDebouncedSearch,
  DataCache,
  transparencyCache,
  metricsCache,
  useIntersectionObserver,
  usePerformanceMonitor,
  createMemoizedSelector,
  batchProcess,
  getMemoryUsage,
  useComponentSize,
  useOptimizedFilter,
  useLazyComponent,
  collectPerformanceMetrics
}
