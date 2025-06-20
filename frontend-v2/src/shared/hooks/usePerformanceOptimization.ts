/**
 * Performance Optimization Hooks
 * 
 * Provides comprehensive performance optimization utilities including
 * memoization, debouncing, throttling, lazy loading, and virtual scrolling.
 */

import { useState, useEffect, useCallback, useMemo, useRef } from 'react'
import { debounce, throttle } from 'lodash-es'

// Debounced value hook
export const useDebounce = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState<T>(value)

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value)
    }, delay)

    return () => {
      clearTimeout(handler)
    }
  }, [value, delay])

  return debouncedValue
}

// Throttled callback hook
export const useThrottle = <T extends (...args: any[]) => any>(
  callback: T,
  delay: number
): T => {
  const throttledCallback = useMemo(
    () => throttle(callback, delay),
    [callback, delay]
  )

  useEffect(() => {
    return () => {
      throttledCallback.cancel()
    }
  }, [throttledCallback])

  return throttledCallback as T
}

// Debounced callback hook
export const useDebounceCallback = <T extends (...args: any[]) => any>(
  callback: T,
  delay: number
): T => {
  const debouncedCallback = useMemo(
    () => debounce(callback, delay),
    [callback, delay]
  )

  useEffect(() => {
    return () => {
      debouncedCallback.cancel()
    }
  }, [debouncedCallback])

  return debouncedCallback as T
}

// Virtual scrolling hook
export const useVirtualScrolling = <T>(
  items: T[],
  itemHeight: number,
  containerHeight: number,
  overscan: number = 5
) => {
  const [scrollTop, setScrollTop] = useState(0)

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

  const handleScroll = useCallback((event: React.UIEvent<HTMLDivElement>) => {
    setScrollTop(event.currentTarget.scrollTop)
  }, [])

  return {
    visibleItems,
    totalHeight,
    handleScroll,
    scrollTop
  }
}

// Intersection Observer hook for lazy loading
export const useIntersectionObserver = (
  options: IntersectionObserverInit = {}
) => {
  const [isIntersecting, setIsIntersecting] = useState(false)
  const [entry, setEntry] = useState<IntersectionObserverEntry | null>(null)
  const elementRef = useRef<HTMLElement | null>(null)

  useEffect(() => {
    const element = elementRef.current
    if (!element) return

    const observer = new IntersectionObserver(
      ([entry]) => {
        setIsIntersecting(entry.isIntersecting)
        setEntry(entry)
      },
      options
    )

    observer.observe(element)

    return () => {
      observer.unobserve(element)
    }
  }, [options])

  return { elementRef, isIntersecting, entry }
}

// Memoized component wrapper
export const useMemoizedComponent = <P extends object>(
  Component: React.ComponentType<P>,
  deps: React.DependencyList
) => {
  return useMemo(() => Component, deps)
}

// Performance monitoring hook
export const usePerformanceMonitor = (name: string) => {
  const startTimeRef = useRef<number>()

  const start = useCallback(() => {
    startTimeRef.current = performance.now()
  }, [])

  const end = useCallback(() => {
    if (startTimeRef.current) {
      const duration = performance.now() - startTimeRef.current
      console.log(`Performance [${name}]: ${duration.toFixed(2)}ms`)
      return duration
    }
    return 0
  }, [name])

  const measure = useCallback((fn: () => void) => {
    start()
    fn()
    return end()
  }, [start, end])

  return { start, end, measure }
}

// Memory usage monitoring
export const useMemoryMonitor = () => {
  const [memoryInfo, setMemoryInfo] = useState<any>(null)

  useEffect(() => {
    const updateMemoryInfo = () => {
      if ('memory' in performance) {
        setMemoryInfo((performance as any).memory)
      }
    }

    updateMemoryInfo()
    const interval = setInterval(updateMemoryInfo, 5000)

    return () => clearInterval(interval)
  }, [])

  return memoryInfo
}

// Optimized search hook
export const useOptimizedSearch = <T>(
  items: T[],
  searchTerm: string,
  searchFields: (keyof T)[],
  debounceMs: number = 300
) => {
  const debouncedSearchTerm = useDebounce(searchTerm, debounceMs)

  const filteredItems = useMemo(() => {
    if (!debouncedSearchTerm.trim()) return items

    const lowercaseSearch = debouncedSearchTerm.toLowerCase()
    
    return items.filter(item =>
      searchFields.some(field => {
        const value = item[field]
        return value && 
               String(value).toLowerCase().includes(lowercaseSearch)
      })
    )
  }, [items, debouncedSearchTerm, searchFields])

  return {
    filteredItems,
    isSearching: searchTerm !== debouncedSearchTerm,
    searchTerm: debouncedSearchTerm
  }
}

// Batch processing hook
export const useBatchProcessor = <T, R>(
  processor: (batch: T[]) => Promise<R[]>,
  batchSize: number = 100,
  delay: number = 0
) => {
  const [isProcessing, setIsProcessing] = useState(false)
  const [progress, setProgress] = useState(0)
  const [results, setResults] = useState<R[]>([])

  const processBatch = useCallback(async (items: T[]) => {
    setIsProcessing(true)
    setProgress(0)
    setResults([])

    const batches = []
    for (let i = 0; i < items.length; i += batchSize) {
      batches.push(items.slice(i, i + batchSize))
    }

    const allResults: R[] = []

    for (let i = 0; i < batches.length; i++) {
      const batchResults = await processor(batches[i])
      allResults.push(...batchResults)
      setResults([...allResults])
      setProgress(((i + 1) / batches.length) * 100)

      if (delay > 0 && i < batches.length - 1) {
        await new Promise(resolve => setTimeout(resolve, delay))
      }
    }

    setIsProcessing(false)
    return allResults
  }, [processor, batchSize, delay])

  return {
    processBatch,
    isProcessing,
    progress,
    results
  }
}

// Component lazy loading hook
export const useLazyComponent = <P extends object>(
  importFn: () => Promise<{ default: React.ComponentType<P> }>,
  fallback?: React.ComponentType
) => {
  const [Component, setComponent] = useState<React.ComponentType<P> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<Error | null>(null)

  const loadComponent = useCallback(async () => {
    if (Component) return Component

    setIsLoading(true)
    setError(null)

    try {
      const module = await importFn()
      setComponent(() => module.default)
      return module.default
    } catch (err) {
      setError(err as Error)
      throw err
    } finally {
      setIsLoading(false)
    }
  }, [importFn, Component])

  return {
    Component: Component || fallback,
    loadComponent,
    isLoading,
    error
  }
}
