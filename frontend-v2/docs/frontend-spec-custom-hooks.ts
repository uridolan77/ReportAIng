// shared/hooks/useCostMetrics.ts
export const useCostMetrics = (timeRange: TimeRange = '30d') => {
  const { startDate, endDate } = getDateRange(timeRange)
  
  const analytics = useGetCostAnalyticsQuery({ startDate, endDate })
  const trends = useGetCostTrendsQuery({ periods: 30 })
  const realTime = useGetRealTimeCostMetricsQuery()
  
  return {
    analytics: analytics.data,
    trends: trends.data,
    realTime: realTime.data,
    isLoading: analytics.isLoading || trends.isLoading,
    error: analytics.error || trends.error,
    refetch: () => {
      analytics.refetch()
      trends.refetch()
      realTime.refetch()
    }
  }
}

// shared/hooks/usePerformanceMonitoring.ts
export const usePerformanceMonitoring = (entityType: string, entityId: string) => {
  const metrics = useAnalyzePerformanceQuery({ entityType, entityId })
  const bottlenecks = useIdentifyBottlenecksQuery({ entityType, entityId })
  const suggestions = useGetOptimizationSuggestionsQuery({ entityType, entityId })
  
  const [autoTune] = useAutoTunePerformanceMutation()
  
  const handleAutoTune = useCallback(async () => {
    try {
      await autoTune({ entityType, entityId }).unwrap()
      // Refetch data after auto-tuning
      metrics.refetch()
      bottlenecks.refetch()
      suggestions.refetch()
    } catch (error) {
      // Handle error
    }
  }, [autoTune, entityType, entityId])
  
  return {
    metrics: metrics.data,
    bottlenecks: bottlenecks.data,
    suggestions: suggestions.data,
    isLoading: metrics.isLoading || bottlenecks.isLoading || suggestions.isLoading,
    autoTune: handleAutoTune,
    refetch: () => {
      metrics.refetch()
      bottlenecks.refetch()
      suggestions.refetch()
    }
  }
}
