/**
 * Chart Optimization Hooks
 * 
 * Custom hooks for optimizing chart performance including data throttling,
 * resize handling, and animation management.
 */

import { useState, useEffect, useRef, useMemo, useCallback } from 'react'
import { useDebounce } from 'use-debounce'

// Hook for throttling data updates
export const useThrottledData = <T>(data: T[], throttleMs: number = 1000): T[] => {
  const [throttledData] = useDebounce(data, throttleMs)
  return throttledData
}

// Hook for handling chart resize
export const useChartResize = (
  chartRef: React.RefObject<HTMLElement>,
  responsive: boolean = true
) => {
  const [dimensions, setDimensions] = useState({ width: 0, height: 0 })
  
  useEffect(() => {
    if (!responsive || !chartRef.current) return
    
    const resizeObserver = new ResizeObserver((entries) => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect
        setDimensions({ width, height })
      }
    })
    
    resizeObserver.observe(chartRef.current)
    
    return () => {
      resizeObserver.disconnect()
    }
  }, [chartRef, responsive])
  
  return { dimensions }
}

// Hook for managing chart animations
export const useChartAnimation = (enableAnimation: boolean = true) => {
  return useMemo(() => ({
    animationDuration: enableAnimation ? 1000 : 0,
    animationEasing: 'ease-out' as const,
  }), [enableAnimation])
}

// Hook for intelligent data sampling
export const useDataSampling = <T extends Record<string, any>>(
  data: T[],
  maxPoints: number = 1000,
  strategy: 'uniform' | 'adaptive' | 'time-based' = 'adaptive'
): T[] => {
  return useMemo(() => {
    if (data.length <= maxPoints) return data
    
    switch (strategy) {
      case 'uniform':
        // Simple uniform sampling
        const step = Math.ceil(data.length / maxPoints)
        return data.filter((_, index) => index % step === 0)
      
      case 'adaptive':
        // Adaptive sampling that preserves important data points
        return adaptiveSample(data, maxPoints)
      
      case 'time-based':
        // Time-based sampling for time series data
        return timeBasedSample(data, maxPoints)
      
      default:
        return data
    }
  }, [data, maxPoints, strategy])
}

// Adaptive sampling algorithm
const adaptiveSample = <T extends Record<string, any>>(data: T[], maxPoints: number): T[] => {
  if (data.length <= maxPoints) return data
  
  const result: T[] = []
  const step = data.length / maxPoints
  
  for (let i = 0; i < maxPoints; i++) {
    const index = Math.floor(i * step)
    result.push(data[index])
  }
  
  // Always include the last point
  if (result[result.length - 1] !== data[data.length - 1]) {
    result[result.length - 1] = data[data.length - 1]
  }
  
  return result
}

// Time-based sampling for time series data
const timeBasedSample = <T extends Record<string, any>>(data: T[], maxPoints: number): T[] => {
  if (data.length <= maxPoints) return data
  
  // Assume the first key that looks like a timestamp is the time key
  const timeKey = Object.keys(data[0] || {}).find(key => 
    key.toLowerCase().includes('time') || 
    key.toLowerCase().includes('date') ||
    key === 'x'
  )
  
  if (!timeKey) {
    // Fallback to uniform sampling
    const step = Math.ceil(data.length / maxPoints)
    return data.filter((_, index) => index % step === 0)
  }
  
  // Sort by time and sample uniformly
  const sortedData = [...data].sort((a, b) => {
    const aTime = new Date(a[timeKey]).getTime()
    const bTime = new Date(b[timeKey]).getTime()
    return aTime - bTime
  })
  
  const step = Math.ceil(sortedData.length / maxPoints)
  return sortedData.filter((_, index) => index % step === 0)
}

// Hook for chart performance monitoring
export const useChartPerformance = () => {
  const renderCount = useRef(0)
  const lastRenderTime = useRef(Date.now())
  const [performanceMetrics, setPerformanceMetrics] = useState({
    renderCount: 0,
    averageRenderTime: 0,
    lastRenderDuration: 0,
  })
  
  const startRender = useCallback(() => {
    lastRenderTime.current = Date.now()
  }, [])
  
  const endRender = useCallback(() => {
    const renderDuration = Date.now() - lastRenderTime.current
    renderCount.current += 1
    
    setPerformanceMetrics(prev => ({
      renderCount: renderCount.current,
      averageRenderTime: (prev.averageRenderTime * (renderCount.current - 1) + renderDuration) / renderCount.current,
      lastRenderDuration: renderDuration,
    }))
  }, [])
  
  return {
    performanceMetrics,
    startRender,
    endRender,
  }
}

// Hook for managing chart data updates
export const useChartDataManager = <T>(
  initialData: T[],
  updateStrategy: 'replace' | 'append' | 'smart' = 'replace',
  maxDataPoints: number = 1000
) => {
  const [data, setData] = useState<T[]>(initialData)
  
  const updateData = useCallback((newData: T[]) => {
    setData(prevData => {
      switch (updateStrategy) {
        case 'replace':
          return newData
        
        case 'append':
          const combined = [...prevData, ...newData]
          return combined.length > maxDataPoints 
            ? combined.slice(-maxDataPoints)
            : combined
        
        case 'smart':
          // Smart update: replace if completely different, append if similar
          if (prevData.length === 0 || newData.length === 0) {
            return newData
          }
          
          // Simple heuristic: if new data starts where old data ends, append
          const lastOldItem = prevData[prevData.length - 1]
          const firstNewItem = newData[0]
          
          // This is a simplified check - in practice, you'd want more sophisticated logic
          const shouldAppend = JSON.stringify(lastOldItem) !== JSON.stringify(firstNewItem)
          
          if (shouldAppend) {
            const combined = [...prevData, ...newData]
            return combined.length > maxDataPoints 
              ? combined.slice(-maxDataPoints)
              : combined
          } else {
            return newData
          }
        
        default:
          return newData
      }
    })
  }, [updateStrategy, maxDataPoints])
  
  const clearData = useCallback(() => {
    setData([])
  }, [])
  
  return {
    data,
    updateData,
    clearData,
  }
}

// Hook for chart zoom and pan functionality
export const useChartZoom = () => {
  const [zoomDomain, setZoomDomain] = useState<{ x?: [number, number], y?: [number, number] }>({})
  const [isPanning, setIsPanning] = useState(false)
  
  const handleZoom = useCallback((domain: { x?: [number, number], y?: [number, number] }) => {
    setZoomDomain(domain)
  }, [])
  
  const resetZoom = useCallback(() => {
    setZoomDomain({})
  }, [])
  
  const startPan = useCallback(() => {
    setIsPanning(true)
  }, [])
  
  const endPan = useCallback(() => {
    setIsPanning(false)
  }, [])
  
  return {
    zoomDomain,
    isPanning,
    handleZoom,
    resetZoom,
    startPan,
    endPan,
  }
}

// Hook for chart color management
export const useChartColors = (
  seriesCount: number,
  customColors?: string[],
  colorScheme: 'default' | 'accessible' | 'colorblind' = 'default'
) => {
  const colorSchemes = {
    default: [
      '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', 
      '#9467bd', '#8c564b', '#e377c2', '#7f7f7f',
      '#bcbd22', '#17becf'
    ],
    accessible: [
      '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728',
      '#9467bd', '#8c564b', '#e377c2', '#7f7f7f'
    ],
    colorblind: [
      '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728',
      '#9467bd', '#8c564b', '#e377c2', '#bcbd22'
    ]
  }
  
  return useMemo(() => {
    const baseColors = customColors || colorSchemes[colorScheme]
    const colors: string[] = []
    
    for (let i = 0; i < seriesCount; i++) {
      colors.push(baseColors[i % baseColors.length])
    }
    
    return colors
  }, [seriesCount, customColors, colorScheme])
}
