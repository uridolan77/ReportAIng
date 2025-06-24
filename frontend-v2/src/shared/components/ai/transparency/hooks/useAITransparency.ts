import { useState, useEffect, useCallback } from 'react'
import { transparencyHub } from '@shared/services/signalr/transparencyHub'
import { transparencySignalR } from '@shared/services/transparencySignalR'
import { useAppSelector } from '@shared/hooks'
import { selectAccessToken } from '@shared/store/auth'

/**
 * Hook for real-time transparency updates
 * 
 * Provides connection management and real-time updates for AI transparency
 * Uses SignalR for live communication with the backend
 */
export const useRealTimeTransparency = (userId?: string) => {
  const [isConnected, setIsConnected] = useState(false)
  const [lastUpdate, setLastUpdate] = useState<string | null>(null)
  const [connectionError, setConnectionError] = useState<string | null>(null)
  const [isConnecting, setIsConnecting] = useState(false)
  
  const accessToken = useAppSelector(selectAccessToken)

  // Connection management
  const connect = useCallback(async () => {
    if (isConnecting || isConnected) return
    
    setIsConnecting(true)
    setConnectionError(null)
    
    try {
      await transparencyHub.connect()
      setIsConnected(true)
      console.log('✅ Real-time transparency connected')
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Connection failed'
      setConnectionError(errorMessage)
      console.error('❌ Failed to connect transparency:', error)
    } finally {
      setIsConnecting(false)
    }
  }, [isConnecting, isConnected])

  const disconnect = useCallback(async () => {
    try {
      await transparencyHub.disconnect()
      setIsConnected(false)
      setLastUpdate(null)
      console.log('🔌 Real-time transparency disconnected')
    } catch (error) {
      console.error('❌ Error disconnecting transparency:', error)
    }
  }, [])

  // Setup event listeners
  useEffect(() => {
    if (!isConnected) return

    // Connection state changes
    const handleConnectionStateChanged = (event: any) => {
      setIsConnected(event.state === 'connected')
      if (event.error) {
        setConnectionError(event.error.message || 'Connection error')
      } else {
        setConnectionError(null)
      }
    }

    // Transparency updates
    const handleTransparencyUpdate = (data: any) => {
      setLastUpdate(new Date().toISOString())
      console.log('📊 Transparency update received:', data)
    }

    const handleStepCompleted = (data: any) => {
      setLastUpdate(new Date().toISOString())
      console.log('✅ Step completed:', data)
    }

    const handleConfidenceUpdate = (data: any) => {
      setLastUpdate(new Date().toISOString())
      console.log('📈 Confidence updated:', data)
    }

    const handleTraceCompleted = (data: any) => {
      setLastUpdate(new Date().toISOString())
      console.log('🎯 Trace completed:', data)
    }

    const handleTransparencyError = (data: any) => {
      setConnectionError(data.error || 'Transparency error')
      console.error('❌ Transparency error:', data)
    }

    // Subscribe to events
    transparencyHub.on('connectionStateChanged', handleConnectionStateChanged)
    transparencyHub.on('transparencyUpdate', handleTransparencyUpdate)
    transparencyHub.on('stepCompleted', handleStepCompleted)
    transparencyHub.on('confidenceUpdate', handleConfidenceUpdate)
    transparencyHub.on('traceCompleted', handleTraceCompleted)
    transparencyHub.on('transparencyError', handleTransparencyError)

    return () => {
      // Cleanup listeners
      transparencyHub.off('connectionStateChanged', handleConnectionStateChanged)
      transparencyHub.off('transparencyUpdate', handleTransparencyUpdate)
      transparencyHub.off('stepCompleted', handleStepCompleted)
      transparencyHub.off('confidenceUpdate', handleConfidenceUpdate)
      transparencyHub.off('traceCompleted', handleTraceCompleted)
      transparencyHub.off('transparencyError', handleTransparencyError)
    }
  }, [isConnected])

  // Auto-connect when user is available and token exists
  useEffect(() => {
    if (userId && accessToken && !isConnected && !isConnecting) {
      connect()
    }
  }, [userId, accessToken, isConnected, isConnecting, connect])

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (isConnected) {
        disconnect()
      }
    }
  }, [isConnected, disconnect])

  return {
    isConnected,
    lastUpdate,
    connectionError,
    isConnecting,
    connect,
    disconnect
  }
}

/**
 * Hook for AI transparency data management
 * 
 * Provides access to transparency traces, metrics, and analysis
 */
export const useAITransparency = (traceId?: string) => {
  const [transparencyData, setTransparencyData] = useState<any>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const refreshTrace = useCallback(async () => {
    if (!traceId) return

    setLoading(true)
    setError(null)

    try {
      // Get transparency status from hub
      const status = await transparencyHub.getTransparencyStatus()
      setTransparencyData(status)
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch transparency data'
      setError(errorMessage)
      console.error('❌ Error fetching transparency data:', err)
    } finally {
      setLoading(false)
    }
  }, [traceId])

  // Subscribe to specific trace updates
  useEffect(() => {
    if (traceId && transparencyHub.isConnected) {
      transparencyHub.subscribeToTrace(traceId).catch(err => {
        console.error('❌ Failed to subscribe to trace:', err)
        setError('Failed to subscribe to trace updates')
      })
    }
  }, [traceId])

  // Initial data fetch
  useEffect(() => {
    if (traceId) {
      refreshTrace()
    }
  }, [traceId, refreshTrace])

  return {
    transparencyData,
    loading,
    error,
    refreshTrace
  }
}

export default useRealTimeTransparency
