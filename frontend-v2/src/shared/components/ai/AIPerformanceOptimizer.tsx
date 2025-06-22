import React, { Suspense, lazy, useMemo, useCallback, memo } from 'react'
import { Spin, Alert } from 'antd'
import { LoadingOutlined } from '@ant-design/icons'
import { useAIIntegration } from './AIIntegrationProvider'

// ============================================================================
// LAZY LOADING UTILITIES
// ============================================================================

/**
 * Enhanced lazy loading with error boundaries and loading states
 */
export const createLazyAIComponent = <T extends React.ComponentType<any>>(
  importFn: () => Promise<{ default: T }>,
  componentName: string,
  fallback?: React.ReactNode
) => {
  const LazyComponent = lazy(importFn)
  
  const WrappedComponent = memo((props: React.ComponentProps<T>) => {
    const { reportError } = useAIIntegration()
    
    const defaultFallback = (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        minHeight: '200px',
        flexDirection: 'column',
        gap: '16px'
      }}>
        <Spin 
          indicator={<LoadingOutlined style={{ fontSize: 24 }} spin />}
          size="large"
        />
        <div style={{ color: '#666', fontSize: '14px' }}>
          Loading {componentName}...
        </div>
      </div>
    )
    
    const handleError = useCallback((error: Error) => {
      reportError(componentName, `Failed to load component: ${error.message}`, 'high')
    }, [reportError, componentName])
    
    return (
      <Suspense fallback={fallback || defaultFallback}>
        <ErrorBoundaryWrapper onError={handleError}>
          <LazyComponent {...props} />
        </ErrorBoundaryWrapper>
      </Suspense>
    )
  })
  
  WrappedComponent.displayName = `Lazy(${componentName})`
  return WrappedComponent
}

// ============================================================================
// VIRTUAL SCROLLING COMPONENT
// ============================================================================

interface VirtualScrollProps {
  items: any[]
  itemHeight: number
  containerHeight: number
  renderItem: (item: any, index: number) => React.ReactNode
  overscan?: number
  className?: string
}

export const VirtualScroll: React.FC<VirtualScrollProps> = memo(({
  items,
  itemHeight,
  containerHeight,
  renderItem,
  overscan = 5,
  className
}) => {
  const [scrollTop, setScrollTop] = React.useState(0)
  
  const visibleRange = useMemo(() => {
    const start = Math.floor(scrollTop / itemHeight)
    const end = Math.min(
      start + Math.ceil(containerHeight / itemHeight),
      items.length - 1
    )
    
    return {
      start: Math.max(0, start - overscan),
      end: Math.min(items.length - 1, end + overscan)
    }
  }, [scrollTop, itemHeight, containerHeight, items.length, overscan])
  
  const visibleItems = useMemo(() => {
    return items.slice(visibleRange.start, visibleRange.end + 1)
  }, [items, visibleRange])
  
  const totalHeight = items.length * itemHeight
  const offsetY = visibleRange.start * itemHeight
  
  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    setScrollTop(e.currentTarget.scrollTop)
  }, [])
  
  return (
    <div
      className={className}
      style={{
        height: containerHeight,
        overflow: 'auto'
      }}
      onScroll={handleScroll}
    >
      <div style={{ height: totalHeight, position: 'relative' }}>
        <div style={{ transform: `translateY(${offsetY}px)` }}>
          {visibleItems.map((item, index) => (
            <div
              key={visibleRange.start + index}
              style={{ height: itemHeight }}
            >
              {renderItem(item, visibleRange.start + index)}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
})

VirtualScroll.displayName = 'VirtualScroll'

// ============================================================================
// MEMOIZATION UTILITIES
// ============================================================================

/**
 * Enhanced memo with deep comparison for AI component props
 */
export const memoizeAIComponent = <T extends React.ComponentType<any>>(
  Component: T,
  propsAreEqual?: (prevProps: React.ComponentProps<T>, nextProps: React.ComponentProps<T>) => boolean
) => {
  const defaultPropsAreEqual = (
    prevProps: React.ComponentProps<T>, 
    nextProps: React.ComponentProps<T>
  ) => {
    // Custom comparison for AI-specific props
    const aiSpecificProps = ['confidence', 'data', 'loading', 'error']
    
    for (const prop of aiSpecificProps) {
      if (prevProps[prop] !== nextProps[prop]) {
        return false
      }
    }
    
    // Shallow comparison for other props
    const prevKeys = Object.keys(prevProps)
    const nextKeys = Object.keys(nextProps)
    
    if (prevKeys.length !== nextKeys.length) {
      return false
    }
    
    for (const key of prevKeys) {
      if (prevProps[key] !== nextProps[key]) {
        return false
      }
    }
    
    return true
  }
  
  return memo(Component, propsAreEqual || defaultPropsAreEqual)
}

// ============================================================================
// DEBOUNCING AND THROTTLING
// ============================================================================

/**
 * Debounced callback hook for AI operations
 */
export const useAIDebounce = <T extends (...args: any[]) => any>(
  callback: T,
  delay: number,
  deps: React.DependencyList = []
) => {
  const timeoutRef = React.useRef<NodeJS.Timeout>()
  
  const debouncedCallback = useCallback((...args: Parameters<T>) => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
    
    timeoutRef.current = setTimeout(() => {
      callback(...args)
    }, delay)
  }, [callback, delay, ...deps])
  
  React.useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [])
  
  return debouncedCallback
}

/**
 * Throttled callback hook for AI operations
 */
export const useAIThrottle = <T extends (...args: any[]) => any>(
  callback: T,
  delay: number,
  deps: React.DependencyList = []
) => {
  const lastCallRef = React.useRef<number>(0)
  const timeoutRef = React.useRef<NodeJS.Timeout>()
  
  const throttledCallback = useCallback((...args: Parameters<T>) => {
    const now = Date.now()
    const timeSinceLastCall = now - lastCallRef.current
    
    if (timeSinceLastCall >= delay) {
      lastCallRef.current = now
      callback(...args)
    } else {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
      
      timeoutRef.current = setTimeout(() => {
        lastCallRef.current = Date.now()
        callback(...args)
      }, delay - timeSinceLastCall)
    }
  }, [callback, delay, ...deps])
  
  React.useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [])
  
  return throttledCallback
}

// ============================================================================
// INTERSECTION OBSERVER HOOK
// ============================================================================

/**
 * Intersection observer hook for lazy loading AI components
 */
export const useAIIntersectionObserver = (
  options: IntersectionObserverInit = {}
) => {
  const [isIntersecting, setIsIntersecting] = React.useState(false)
  const [hasIntersected, setHasIntersected] = React.useState(false)
  const elementRef = React.useRef<HTMLElement>(null)
  
  React.useEffect(() => {
    const element = elementRef.current
    if (!element) return
    
    const observer = new IntersectionObserver(
      ([entry]) => {
        setIsIntersecting(entry.isIntersecting)
        if (entry.isIntersecting && !hasIntersected) {
          setHasIntersected(true)
        }
      },
      {
        threshold: 0.1,
        rootMargin: '50px',
        ...options
      }
    )
    
    observer.observe(element)
    
    return () => {
      observer.unobserve(element)
    }
  }, [hasIntersected, options])
  
  return {
    elementRef,
    isIntersecting,
    hasIntersected
  }
}

// ============================================================================
// PERFORMANCE MONITORING HOOK
// ============================================================================

/**
 * Performance monitoring hook for AI components
 */
export const useAIPerformanceMetrics = (componentName: string) => {
  const [metrics, setMetrics] = React.useState({
    renderTime: 0,
    updateCount: 0,
    lastUpdate: Date.now()
  })
  
  const renderStartTime = React.useRef<number>(0)
  
  // Track render start
  React.useLayoutEffect(() => {
    renderStartTime.current = performance.now()
  })
  
  // Track render end
  React.useEffect(() => {
    const renderTime = performance.now() - renderStartTime.current
    setMetrics(prev => ({
      renderTime,
      updateCount: prev.updateCount + 1,
      lastUpdate: Date.now()
    }))
  })
  
  // Log performance metrics in development
  React.useEffect(() => {
    if (process.env.NODE_ENV === 'development' && metrics.updateCount > 0) {
      console.log(`[${componentName}] Performance:`, {
        renderTime: `${metrics.renderTime.toFixed(2)}ms`,
        updateCount: metrics.updateCount,
        avgRenderTime: `${(metrics.renderTime / metrics.updateCount).toFixed(2)}ms`
      })
    }
  }, [componentName, metrics])
  
  return metrics
}

// ============================================================================
// ERROR BOUNDARY WRAPPER
// ============================================================================

interface ErrorBoundaryWrapperProps {
  children: React.ReactNode
  onError?: (error: Error) => void
}

class ErrorBoundaryWrapper extends React.Component<
  ErrorBoundaryWrapperProps,
  { hasError: boolean }
> {
  constructor(props: ErrorBoundaryWrapperProps) {
    super(props)
    this.state = { hasError: false }
  }
  
  static getDerivedStateFromError(): { hasError: boolean } {
    return { hasError: true }
  }
  
  componentDidCatch(error: Error) {
    this.props.onError?.(error)
  }
  
  render() {
    if (this.state.hasError) {
      return (
        <Alert
          message="Component Error"
          description="This component failed to load. Please try refreshing the page."
          type="error"
          showIcon
        />
      )
    }
    
    return this.props.children
  }
}

// ============================================================================
// LAZY LOADED AI COMPONENTS
// ============================================================================

// Lazy load all major AI components for better performance
export const LazyAITransparencyPanel = createLazyAIComponent(
  () => import('./transparency/AITransparencyPanel'),
  'AITransparencyPanel'
)

export const LazyLLMProviderManager = createLazyAIComponent(
  () => import('./management/LLMProviderManager'),
  'LLMProviderManager'
)

export const LazyAISchemaExplorer = createLazyAIComponent(
  () => import('./insights/AISchemaExplorer'),
  'AISchemaExplorer'
)

export const LazyQueryOptimizationEngine = createLazyAIComponent(
  () => import('./insights/QueryOptimizationEngine'),
  'QueryOptimizationEngine'
)

export const LazyAutomatedInsightsGenerator = createLazyAIComponent(
  () => import('./insights/AutomatedInsightsGenerator'),
  'AutomatedInsightsGenerator'
)

export const LazyPredictiveAnalyticsPanel = createLazyAIComponent(
  () => import('./insights/PredictiveAnalyticsPanel'),
  'PredictiveAnalyticsPanel'
)

// ============================================================================
// PERFORMANCE OPTIMIZATION HOC
// ============================================================================

export const withAIPerformanceOptimization = <T extends React.ComponentType<any>>(
  Component: T,
  options: {
    enableVirtualization?: boolean
    enableMemoization?: boolean
    enableLazyLoading?: boolean
    debounceMs?: number
  } = {}
) => {
  const {
    enableMemoization = true,
    enableLazyLoading = false,
    debounceMs = 0
  } = options
  
  let OptimizedComponent = Component
  
  // Apply memoization
  if (enableMemoization) {
    OptimizedComponent = memoizeAIComponent(OptimizedComponent)
  }
  
  // Apply lazy loading
  if (enableLazyLoading) {
    OptimizedComponent = createLazyAIComponent(
      () => Promise.resolve({ default: OptimizedComponent }),
      Component.displayName || Component.name || 'Component'
    )
  }
  
  // Apply debouncing for props updates
  if (debounceMs > 0) {
    const DebouncedComponent = (props: React.ComponentProps<T>) => {
      const [debouncedProps, setDebouncedProps] = React.useState(props)
      
      const updateProps = useAIDebounce((newProps: React.ComponentProps<T>) => {
        setDebouncedProps(newProps)
      }, debounceMs, [])
      
      React.useEffect(() => {
        updateProps(props)
      }, [props, updateProps])
      
      return <OptimizedComponent {...debouncedProps} />
    }
    
    DebouncedComponent.displayName = `Debounced(${Component.displayName || Component.name})`
    OptimizedComponent = DebouncedComponent as any
  }
  
  return OptimizedComponent
}

export default {
  createLazyAIComponent,
  VirtualScroll,
  memoizeAIComponent,
  useAIDebounce,
  useAIThrottle,
  useAIIntersectionObserver,
  useAIPerformanceMetrics,
  withAIPerformanceOptimization,
  LazyAITransparencyPanel,
  LazyLLMProviderManager,
  LazyAISchemaExplorer,
  LazyQueryOptimizationEngine,
  LazyAutomatedInsightsGenerator,
  LazyPredictiveAnalyticsPanel
}
