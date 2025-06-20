/**
 * Performance Alerts Hook
 * 
 * Provides performance monitoring and alerting functionality
 */

import { useState, useEffect } from 'react'

export interface PerformanceAlert {
  id: string
  type: 'critical' | 'high' | 'medium' | 'low'
  message: string
  timestamp: Date
  source: string
  acknowledged: boolean
}

export const usePerformanceAlerts = () => {
  const [alerts, setAlerts] = useState<PerformanceAlert[]>([])

  // Mock data for demonstration
  useEffect(() => {
    const mockAlerts: PerformanceAlert[] = [
      {
        id: '1',
        type: 'critical',
        message: 'High memory usage detected',
        timestamp: new Date(),
        source: 'System Monitor',
        acknowledged: false,
      },
      {
        id: '2',
        type: 'high',
        message: 'Slow query performance',
        timestamp: new Date(),
        source: 'Query Monitor',
        acknowledged: false,
      },
    ]
    
    setAlerts(mockAlerts)
  }, [])

  const criticalAlerts = alerts.filter(alert => alert.type === 'critical')
  const highAlerts = alerts.filter(alert => alert.type === 'high')

  return {
    alerts,
    criticalAlerts,
    highAlerts,
    totalCount: alerts.length,
    criticalCount: criticalAlerts.length,
    highCount: highAlerts.length,
  }
}

export default usePerformanceAlerts
