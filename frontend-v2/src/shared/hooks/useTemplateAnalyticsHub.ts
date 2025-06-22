import { useEffect, useState, useCallback, useRef } from 'react'
import { useSelector } from 'react-redux'
import { message } from 'antd'
import { 
  getTemplateAnalyticsHub, 
  disconnectTemplateAnalyticsHub,
  type TemplateAnalyticsHubEvents 
} from '@shared/services/signalr/templateAnalyticsHub'
import type { RootState } from '@shared/store'
import type {
  ComprehensiveAnalyticsDashboard,
  TemplatePerformanceMetrics,
  ABTestDetails,
  PerformanceAlert,
  RealTimeAnalyticsData
} from '@shared/types/templateAnalytics'

interface UseTemplateAnalyticsHubOptions {
  autoConnect?: boolean
  subscribeToPerformanceUpdates?: boolean
  subscribeToABTestUpdates?: boolean
  subscribeToAlerts?: boolean
  intentType?: string
  onDashboardUpdate?: (data: ComprehensiveAnalyticsDashboard) => void
  onPerformanceUpdate?: (templateKey: string, data: TemplatePerformanceMetrics) => void
  onABTestUpdate?: (testId: number, data: ABTestDetails) => void
  onNewAlert?: (alert: PerformanceAlert) => void
  onRealTimeUpdate?: (data: RealTimeAnalyticsData) => void
  onError?: (error: string) => void
}

interface UseTemplateAnalyticsHubReturn {
  isConnected: boolean
  connectionState: string
  connect: () => Promise<void>
  disconnect: () => Promise<void>
  subscribeToPerformanceUpdates: (intentType?: string) => Promise<void>
  subscribeToABTestUpdates: () => Promise<void>
  subscribeToAlerts: () => Promise<void>
  getRealTimeDashboard: () => Promise<RealTimeAnalyticsData>
  getTemplatePerformance: (templateKey: string) => Promise<TemplatePerformanceMetrics>
  lastUpdate: Date | null
  error: string | null
}

export const useTemplateAnalyticsHub = (
  options: UseTemplateAnalyticsHubOptions = {}
): UseTemplateAnalyticsHubReturn => {
  const {
    autoConnect = true,
    subscribeToPerformanceUpdates = false,
    subscribeToABTestUpdates = false,
    subscribeToAlerts = false,
    intentType,
    onDashboardUpdate,
    onPerformanceUpdate,
    onABTestUpdate,
    onNewAlert,
    onRealTimeUpdate,
    onError
  } = options

  // State
  const [isConnected, setIsConnected] = useState(false)
  const [connectionState, setConnectionState] = useState('Disconnected')
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null)
  const [error, setError] = useState<string | null>(null)

  // Get auth token from Redux store
  const accessToken = useSelector((state: RootState) => state.auth.accessToken)
  const getAuthToken = useCallback(() => accessToken, [accessToken])

  // Hub instance ref
  const hubRef = useRef(getTemplateAnalyticsHub(getAuthToken))

  // Event handlers
  const handleDashboardUpdate = useCallback((data: ComprehensiveAnalyticsDashboard) => {
    setLastUpdate(new Date())
    onDashboardUpdate?.(data)
  }, [onDashboardUpdate])

  const handlePerformanceUpdate = useCallback((templateKey: string, data: TemplatePerformanceMetrics) => {
    setLastUpdate(new Date())
    onPerformanceUpdate?.(templateKey, data)
  }, [onPerformanceUpdate])

  const handleABTestUpdate = useCallback((testId: number, data: ABTestDetails) => {
    setLastUpdate(new Date())
    onABTestUpdate?.(testId, data)
  }, [onABTestUpdate])

  const handleNewAlert = useCallback((alert: PerformanceAlert) => {
    setLastUpdate(new Date())
    message.warning(`New Alert: ${alert.message}`)
    onNewAlert?.(alert)
  }, [onNewAlert])

  const handleRealTimeUpdate = useCallback((data: RealTimeAnalyticsData) => {
    setLastUpdate(new Date())
    onRealTimeUpdate?.(data)
  }, [onRealTimeUpdate])

  const handleError = useCallback((errorMessage: string) => {
    setError(errorMessage)
    message.error(`Template Analytics Hub Error: ${errorMessage}`)
    onError?.(errorMessage)
  }, [onError])

  // Connection methods
  const connect = useCallback(async () => {
    try {
      setError(null)
      await hubRef.current.connect()
      setIsConnected(hubRef.current.connected)
      setConnectionState(hubRef.current.connectionState)

      // Auto-subscribe to events if requested
      if (subscribeToPerformanceUpdates) {
        await hubRef.current.subscribeToPerformanceUpdates(intentType)
      }
      if (subscribeToABTestUpdates) {
        await hubRef.current.subscribeToABTestUpdates()
      }
      if (subscribeToAlerts) {
        await hubRef.current.subscribeToAlerts()
      }

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown connection error'
      setError(errorMessage)
      console.error('Failed to connect to Template Analytics Hub:', error)
    }
  }, [subscribeToPerformanceUpdates, subscribeToABTestUpdates, subscribeToAlerts, intentType])

  const disconnect = useCallback(async () => {
    try {
      await disconnectTemplateAnalyticsHub()
      setIsConnected(false)
      setConnectionState('Disconnected')
      setError(null)
    } catch (error) {
      console.error('Failed to disconnect from Template Analytics Hub:', error)
    }
  }, [])

  // Hub method wrappers
  const subscribeToPerformanceUpdatesMethod = useCallback(async (intentType?: string) => {
    try {
      await hubRef.current.subscribeToPerformanceUpdates(intentType)
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to subscribe to performance updates'
      setError(errorMessage)
      throw error
    }
  }, [])

  const subscribeToABTestUpdatesMethod = useCallback(async () => {
    try {
      await hubRef.current.subscribeToABTestUpdates()
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to subscribe to A/B test updates'
      setError(errorMessage)
      throw error
    }
  }, [])

  const subscribeToAlertsMethod = useCallback(async () => {
    try {
      await hubRef.current.subscribeToAlerts()
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to subscribe to alerts'
      setError(errorMessage)
      throw error
    }
  }, [])

  const getRealTimeDashboard = useCallback(async () => {
    try {
      return await hubRef.current.getRealTimeDashboard()
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to get real-time dashboard'
      setError(errorMessage)
      throw error
    }
  }, [])

  const getTemplatePerformance = useCallback(async (templateKey: string) => {
    try {
      return await hubRef.current.getTemplatePerformance(templateKey)
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to get template performance'
      setError(errorMessage)
      throw error
    }
  }, [])

  // Set up event listeners
  useEffect(() => {
    const hub = hubRef.current

    // Subscribe to events
    hub.on('DashboardUpdate', handleDashboardUpdate)
    hub.on('PerformanceUpdate', handlePerformanceUpdate)
    hub.on('ABTestUpdate', handleABTestUpdate)
    hub.on('NewAlert', handleNewAlert)
    hub.on('RealTimeUpdate', handleRealTimeUpdate)
    hub.on('Error', handleError)

    return () => {
      // Unsubscribe from events
      hub.off('DashboardUpdate', handleDashboardUpdate)
      hub.off('PerformanceUpdate', handlePerformanceUpdate)
      hub.off('ABTestUpdate', handleABTestUpdate)
      hub.off('NewAlert', handleNewAlert)
      hub.off('RealTimeUpdate', handleRealTimeUpdate)
      hub.off('Error', handleError)
    }
  }, [
    handleDashboardUpdate,
    handlePerformanceUpdate,
    handleABTestUpdate,
    handleNewAlert,
    handleRealTimeUpdate,
    handleError
  ])

  // Auto-connect on mount
  useEffect(() => {
    if (autoConnect && accessToken) {
      connect()
    }

    return () => {
      if (autoConnect) {
        disconnect()
      }
    }
  }, [autoConnect, accessToken, connect, disconnect])

  // Update connection state periodically
  useEffect(() => {
    const interval = setInterval(() => {
      setIsConnected(hubRef.current.connected)
      setConnectionState(hubRef.current.connectionState)
    }, 1000)

    return () => clearInterval(interval)
  }, [])

  return {
    isConnected,
    connectionState,
    connect,
    disconnect,
    subscribeToPerformanceUpdates: subscribeToPerformanceUpdatesMethod,
    subscribeToABTestUpdates: subscribeToABTestUpdatesMethod,
    subscribeToAlerts: subscribeToAlertsMethod,
    getRealTimeDashboard,
    getTemplatePerformance,
    lastUpdate,
    error
  }
}

export default useTemplateAnalyticsHub
