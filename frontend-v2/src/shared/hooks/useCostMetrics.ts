import { useCallback } from 'react'
import {
  useGetCostAnalyticsQuery,
  useGetCostTrendsQuery,
  useGetRealTimeCostMetricsQuery,
  useGetCostForecastQuery,
  useGetOptimizationRecommendationsQuery,
} from '../store/api/costApi'
import type { TimeRange } from '../types/cost'

// Helper function to get date range from time range
const getDateRange = (timeRange: TimeRange) => {
  const endDate = new Date()
  const startDate = new Date()

  switch (timeRange) {
    case '1h':
      startDate.setHours(startDate.getHours() - 1)
      break
    case '24h':
      startDate.setDate(startDate.getDate() - 1)
      break
    case '7d':
      startDate.setDate(startDate.getDate() - 7)
      break
    case '30d':
      startDate.setDate(startDate.getDate() - 30)
      break
    case '90d':
      startDate.setDate(startDate.getDate() - 90)
      break
    case '1y':
      startDate.setFullYear(startDate.getFullYear() - 1)
      break
    default:
      startDate.setDate(startDate.getDate() - 30)
  }

  return {
    startDate: startDate.toISOString(),
    endDate: endDate.toISOString(),
  }
}

export const useCostMetrics = (timeRange: TimeRange = '30d') => {
  const { startDate, endDate } = getDateRange(timeRange)
  
  const analytics = useGetCostAnalyticsQuery({ startDate, endDate })
  const trends = useGetCostTrendsQuery({ periods: 30 })
  const realTime = useGetRealTimeCostMetricsQuery()
  const forecast = useGetCostForecastQuery({ days: 30 })
  const recommendations = useGetOptimizationRecommendationsQuery()
  
  const refetchAll = useCallback(() => {
    analytics.refetch()
    trends.refetch()
    realTime.refetch()
    forecast.refetch()
    recommendations.refetch()
  }, [analytics, trends, realTime, forecast, recommendations])

  return {
    analytics: analytics.data,
    trends: trends.data,
    realTime: realTime.data,
    forecast: forecast.data,
    recommendations: recommendations.data,
    isLoading: analytics.isLoading || trends.isLoading || realTime.isLoading,
    error: analytics.error || trends.error || realTime.error,
    refetch: refetchAll,
  }
}

export const useCostBreakdown = (dimension: string, timeRange: TimeRange = '30d') => {
  const { startDate, endDate } = getDateRange(timeRange)
  
  const { data, isLoading, error, refetch } = useGetCostAnalyticsQuery({ 
    startDate, 
    endDate 
  })

  const getBreakdownData = useCallback(() => {
    if (!data) return []

    switch (dimension) {
      case 'provider':
        return Object.entries(data.costByProvider || {}).map(([key, value]) => ({
          label: key,
          value,
          percentage: (value / data.totalCost) * 100,
        }))
      case 'user':
        return Object.entries(data.costByUser || {}).map(([key, value]) => ({
          label: key,
          value,
          percentage: (value / data.totalCost) * 100,
        }))
      case 'department':
        return Object.entries(data.costByDepartment || {}).map(([key, value]) => ({
          label: key,
          value,
          percentage: (value / data.totalCost) * 100,
        }))
      case 'model':
        return Object.entries(data.costByModel || {}).map(([key, value]) => ({
          label: key,
          value,
          percentage: (value / data.totalCost) * 100,
        }))
      default:
        return []
    }
  }, [data, dimension])

  return {
    breakdown: getBreakdownData(),
    total: data?.totalCost || 0,
    isLoading,
    error,
    refetch,
  }
}

export const useCostAlerts = () => {
  const { data: realTimeMetrics } = useGetRealTimeCostMetricsQuery()
  
  const alerts = realTimeMetrics?.alerts || []
  const criticalAlerts = alerts.filter(alert => alert.severity === 'critical')
  const highAlerts = alerts.filter(alert => alert.severity === 'high')
  
  return {
    alerts,
    criticalCount: criticalAlerts.length,
    highCount: highAlerts.length,
    totalCount: alerts.length,
    hasCritical: criticalAlerts.length > 0,
    hasHigh: highAlerts.length > 0,
  }
}

export const useCostEfficiency = (timeRange: TimeRange = '30d') => {
  const { analytics, trends } = useCostMetrics(timeRange)
  
  const efficiency = analytics?.costEfficiency || 0
  const savingsOpportunities = analytics?.costSavingsOpportunities || 0
  
  // Calculate trend direction
  const trendDirection = trends?.summary?.trendDirection || 'stable'
  const changePercentage = trends?.summary?.changePercentage || 0
  
  return {
    efficiency,
    savingsOpportunities,
    trendDirection,
    changePercentage,
    isEfficient: efficiency > 0.8, // 80% efficiency threshold
    hasOpportunities: savingsOpportunities > 0,
  }
}
