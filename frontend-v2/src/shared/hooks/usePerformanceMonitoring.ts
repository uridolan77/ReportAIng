import { useCallback } from 'react'
import {
  useAnalyzePerformanceQuery,
  useIdentifyBottlenecksQuery,
  useGetOptimizationSuggestionsQuery,
  useAutoTunePerformanceMutation,
  useGetActiveAlertsQuery,
  useGetBenchmarksQuery,
} from '../store/api/performanceApi'
import type { PerformanceEntityType } from '../types/performance'

export const usePerformanceMonitoring = (entityType: PerformanceEntityType, entityId: string) => {
  const metrics = useAnalyzePerformanceQuery({ entityType, entityId })
  const bottlenecks = useIdentifyBottlenecksQuery({ entityType, entityId })
  const suggestions = useGetOptimizationSuggestionsQuery({ entityType, entityId })
  
  const [autoTune, autoTuneResult] = useAutoTunePerformanceMutation()
  
  const handleAutoTune = useCallback(async () => {
    try {
      await autoTune({ entityType, entityId }).unwrap()
      // Refetch data after auto-tuning
      metrics.refetch()
      bottlenecks.refetch()
      suggestions.refetch()
    } catch (error) {
      console.error('Auto-tune failed:', error)
      throw error
    }
  }, [autoTune, entityType, entityId, metrics, bottlenecks, suggestions])
  
  const refetchAll = useCallback(() => {
    metrics.refetch()
    bottlenecks.refetch()
    suggestions.refetch()
  }, [metrics, bottlenecks, suggestions])

  return {
    metrics: metrics.data,
    bottlenecks: bottlenecks.data,
    suggestions: suggestions.data,
    isLoading: metrics.isLoading || bottlenecks.isLoading || suggestions.isLoading,
    error: metrics.error || bottlenecks.error || suggestions.error,
    autoTune: handleAutoTune,
    autoTuneLoading: autoTuneResult.isLoading,
    autoTuneResult: autoTuneResult.data,
    refetch: refetchAll,
  }
}

export const usePerformanceAlerts = () => {
  const { data: alerts, isLoading, error, refetch } = useGetActiveAlertsQuery()
  
  const criticalAlerts = alerts?.alerts.filter(alert => alert.severity === 'Critical') || []
  const highAlerts = alerts?.alerts.filter(alert => alert.severity === 'High') || []
  const mediumAlerts = alerts?.alerts.filter(alert => alert.severity === 'Medium') || []
  const lowAlerts = alerts?.alerts.filter(alert => alert.severity === 'Low') || []
  
  return {
    alerts: alerts?.alerts || [],
    criticalAlerts,
    highAlerts,
    mediumAlerts,
    lowAlerts,
    totalCount: alerts?.alerts.length || 0,
    activeCount: alerts?.activeCount || 0,
    resolvedCount: alerts?.resolvedCount || 0,
    hasCritical: criticalAlerts.length > 0,
    hasHigh: highAlerts.length > 0,
    isLoading,
    error,
    refetch,
  }
}

export const usePerformanceBenchmarks = (category?: string) => {
  const { data, isLoading, error, refetch } = useGetBenchmarksQuery({ category })
  
  const passingBenchmarks = data?.benchmarks.filter(b => b.status === 'passing') || []
  const warningBenchmarks = data?.benchmarks.filter(b => b.status === 'warning') || []
  const failingBenchmarks = data?.benchmarks.filter(b => b.status === 'failing') || []
  
  const overallScore = data?.benchmarks.length 
    ? (passingBenchmarks.length / data.benchmarks.length) * 100 
    : 0
  
  return {
    benchmarks: data?.benchmarks || [],
    categories: data?.categories || [],
    passingBenchmarks,
    warningBenchmarks,
    failingBenchmarks,
    overallScore,
    totalCount: data?.benchmarks.length || 0,
    passingCount: passingBenchmarks.length,
    warningCount: warningBenchmarks.length,
    failingCount: failingBenchmarks.length,
    isLoading,
    error,
    refetch,
  }
}

export const usePerformanceScore = (entityType: PerformanceEntityType, entityId: string) => {
  const { metrics } = usePerformanceMonitoring(entityType, entityId)
  
  if (!metrics) {
    return {
      score: 0,
      grade: 'F',
      status: 'unknown',
    }
  }
  
  const score = metrics.metrics.performanceScore
  
  const getGrade = (score: number) => {
    if (score >= 90) return 'A'
    if (score >= 80) return 'B'
    if (score >= 70) return 'C'
    if (score >= 60) return 'D'
    return 'F'
  }
  
  const getStatus = (score: number) => {
    if (score >= 80) return 'excellent'
    if (score >= 60) return 'good'
    if (score >= 40) return 'fair'
    return 'poor'
  }
  
  return {
    score,
    grade: getGrade(score),
    status: getStatus(score),
    responseTime: metrics.metrics.averageResponseTime,
    throughput: metrics.metrics.throughputPerSecond,
    errorRate: metrics.metrics.errorRate,
  }
}

export const usePerformanceComparison = (
  entities: Array<{ type: PerformanceEntityType; id: string; name: string }>
) => {
  const results = entities.map(entity => {
    const { metrics, isLoading } = usePerformanceMonitoring(entity.type, entity.id)
    return {
      entity,
      metrics: metrics?.metrics,
      score: metrics?.score || 0,
      isLoading,
    }
  })
  
  const isLoading = results.some(r => r.isLoading)
  const validResults = results.filter(r => r.metrics)
  
  const bestPerforming = validResults.reduce((best, current) => 
    current.score > best.score ? current : best, validResults[0])
  
  const worstPerforming = validResults.reduce((worst, current) => 
    current.score < worst.score ? current : worst, validResults[0])
  
  return {
    results,
    bestPerforming,
    worstPerforming,
    averageScore: validResults.length 
      ? validResults.reduce((sum, r) => sum + r.score, 0) / validResults.length 
      : 0,
    isLoading,
  }
}
