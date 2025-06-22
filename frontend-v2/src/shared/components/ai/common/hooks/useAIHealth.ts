import { useState, useEffect, useCallback } from 'react'
import type { UseAIHealthResult } from '../types'

/**
 * Hook for monitoring AI system health
 * 
 * This hook provides:
 * - Real-time health monitoring
 * - Automatic health checks
 * - Issue tracking
 * - Recovery detection
 * - Performance metrics
 */
export const useAIHealth = (): UseAIHealthResult => {
  const [status, setStatus] = useState<'healthy' | 'degraded' | 'unhealthy' | 'checking'>('checking')
  const [lastCheck, setLastCheck] = useState<string>()
  const [issues, setIssues] = useState<string[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string>()

  // Perform health check
  const checkHealth = useCallback(async () => {
    setLoading(true)
    setError(undefined)

    try {
      // TODO: Replace with actual health check API when available
      // const response = await fetch('/api/ai/health')
      // const healthData = await response.json()

      // Mock health check for now
      const mockHealthCheck = () => {
        const random = Math.random()
        
        if (random > 0.9) {
          return {
            status: 'unhealthy' as const,
            issues: ['AI service unavailable', 'High error rate detected']
          }
        } else if (random > 0.7) {
          return {
            status: 'degraded' as const,
            issues: ['Slow response times', 'Some providers experiencing issues']
          }
        } else {
          return {
            status: 'healthy' as const,
            issues: []
          }
        }
      }

      const healthResult = mockHealthCheck()
      
      setStatus(healthResult.status)
      setIssues(healthResult.issues)
      setLastCheck(new Date().toISOString())

    } catch (err) {
      console.error('Health check failed:', err)
      setError(err instanceof Error ? err.message : 'Health check failed')
      setStatus('unhealthy')
      setIssues(['Health check failed'])
    } finally {
      setLoading(false)
    }
  }, [])

  // Determine if system is healthy
  const isHealthy = status === 'healthy' || status === 'degraded'

  // Initial health check
  useEffect(() => {
    checkHealth()
  }, [checkHealth])

  // Set up periodic health checks (every 30 seconds)
  useEffect(() => {
    const interval = setInterval(() => {
      checkHealth()
    }, 30 * 1000) // 30 seconds

    return () => clearInterval(interval)
  }, [checkHealth])

  // Set up visibility change listener to check health when tab becomes visible
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (!document.hidden) {
        checkHealth()
      }
    }

    document.addEventListener('visibilitychange', handleVisibilityChange)
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange)
  }, [checkHealth])

  return {
    isHealthy,
    status,
    lastCheck,
    issues,
    loading,
    error,
    checkHealth
  }
}

/**
 * Hook for checking if AI system is ready for use
 */
export const useAIReady = (): boolean => {
  const { isHealthy, status } = useAIHealth()
  return isHealthy && status !== 'checking'
}

/**
 * Hook for AI system status with detailed information
 */
export const useAIStatus = () => {
  const { status, lastCheck, issues, loading, error, checkHealth } = useAIHealth()
  
  const getStatusColor = () => {
    switch (status) {
      case 'healthy': return '#52c41a'
      case 'degraded': return '#faad14'
      case 'unhealthy': return '#ff4d4f'
      case 'checking': return '#1890ff'
      default: return '#d9d9d9'
    }
  }

  const getStatusText = () => {
    switch (status) {
      case 'healthy': return 'All systems operational'
      case 'degraded': return 'Some issues detected'
      case 'unhealthy': return 'System unavailable'
      case 'checking': return 'Checking status...'
      default: return 'Unknown status'
    }
  }

  const getStatusIcon = () => {
    switch (status) {
      case 'healthy': return 'check-circle'
      case 'degraded': return 'exclamation-circle'
      case 'unhealthy': return 'close-circle'
      case 'checking': return 'loading'
      default: return 'question-circle'
    }
  }

  return {
    status,
    lastCheck,
    issues,
    loading,
    error,
    checkHealth,
    statusColor: getStatusColor(),
    statusText: getStatusText(),
    statusIcon: getStatusIcon()
  }
}

export default useAIHealth
