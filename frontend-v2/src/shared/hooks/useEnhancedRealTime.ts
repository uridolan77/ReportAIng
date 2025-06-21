/**
 * Enhanced Real-time Hook
 * 
 * Provides optimized real-time data management with:
 * - Intelligent update batching
 * - Connection status management
 * - Automatic reconnection
 * - Data synchronization
 * - Performance optimizations
 */

import { useState, useEffect, useCallback, useRef } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useAccessibility } from '../components/accessibility/AccessibilityProvider'

export interface RealTimeConfig {
  /** WebSocket endpoint */
  endpoint: string
  /** Reconnection attempts */
  maxReconnectAttempts?: number
  /** Reconnection delay in ms */
  reconnectDelay?: number
  /** Update batching interval in ms */
  batchInterval?: number
  /** Enable debug logging */
  debug?: boolean
  /** Authentication token */
  token?: string
  /** Custom protocols */
  protocols?: string[]
}

export interface ConnectionStatus {
  status: 'connecting' | 'connected' | 'disconnected' | 'error' | 'reconnecting'
  lastConnected?: Date
  reconnectAttempts: number
  latency?: number
}

export interface RealTimeUpdate {
  type: string
  data: any
  timestamp: Date
  id: string
}

export const useEnhancedRealTime = (config: RealTimeConfig) => {
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>({
    status: 'disconnected',
    reconnectAttempts: 0,
  })
  
  const [updates, setUpdates] = useState<RealTimeUpdate[]>([])
  const [isSubscribed, setIsSubscribed] = useState(false)
  
  const wsRef = useRef<WebSocket | null>(null)
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const batchTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const pendingUpdatesRef = useRef<RealTimeUpdate[]>([])
  const subscriptionsRef = useRef<Set<string>>(new Set())
  const lastPingRef = useRef<number>(0)
  
  const queryClient = useQueryClient()
  const { announce } = useAccessibility()
  
  const {
    endpoint,
    maxReconnectAttempts = 5,
    reconnectDelay = 1000,
    batchInterval = 100,
    debug = false,
    token,
    protocols = [],
  } = config

  // Log debug messages
  const log = useCallback((message: string, ...args: any[]) => {
    if (debug) {
      console.log(`[RealTime] ${message}`, ...args)
    }
  }, [debug])

  // Process batched updates
  const processBatchedUpdates = useCallback(() => {
    if (pendingUpdatesRef.current.length === 0) return

    const batch = [...pendingUpdatesRef.current]
    pendingUpdatesRef.current = []

    log(`Processing ${batch.length} batched updates`)

    // Update state with batched updates
    setUpdates(prev => [...prev.slice(-100), ...batch]) // Keep last 100 updates

    // Invalidate relevant queries
    const queryKeys = new Set<string>()
    batch.forEach(update => {
      // Extract query keys from update type
      if (update.type.startsWith('dashboard')) {
        queryKeys.add('dashboard')
      }
      if (update.type.startsWith('metrics')) {
        queryKeys.add('metrics')
      }
      if (update.type.startsWith('cost')) {
        queryKeys.add('cost')
      }
    })

    // Invalidate queries efficiently
    queryKeys.forEach(key => {
      queryClient.invalidateQueries({ queryKey: [key] })
    })

    // Announce updates for accessibility
    if (batch.length > 0) {
      announce(`${batch.length} real-time updates received`, 'polite')
    }
  }, [log, queryClient, announce])

  // Schedule batched update processing
  const scheduleBatchUpdate = useCallback(() => {
    if (batchTimeoutRef.current) {
      clearTimeout(batchTimeoutRef.current)
    }

    batchTimeoutRef.current = setTimeout(processBatchedUpdates, batchInterval)
  }, [processBatchedUpdates, batchInterval])

  // Handle incoming messages
  const handleMessage = useCallback((event: MessageEvent) => {
    try {
      const message = JSON.parse(event.data)
      log('Received message:', message)

      // Handle different message types
      switch (message.type) {
        case 'ping':
          // Respond to ping for latency measurement
          if (wsRef.current?.readyState === WebSocket.OPEN) {
            wsRef.current.send(JSON.stringify({ type: 'pong', timestamp: Date.now() }))
          }
          break

        case 'pong':
          // Calculate latency
          const latency = Date.now() - lastPingRef.current
          setConnectionStatus(prev => ({ ...prev, latency }))
          break

        case 'update':
          // Handle data updates
          const update: RealTimeUpdate = {
            type: message.updateType || 'unknown',
            data: message.data,
            timestamp: new Date(message.timestamp || Date.now()),
            id: message.id || `${Date.now()}-${Math.random()}`,
          }

          // Add to pending updates for batching
          pendingUpdatesRef.current.push(update)
          scheduleBatchUpdate()
          break

        case 'subscription_confirmed':
          log('Subscription confirmed:', message.channel)
          break

        case 'error':
          console.error('WebSocket error message:', message.error)
          announce('Real-time connection error occurred', 'assertive')
          break

        default:
          log('Unknown message type:', message.type)
      }
    } catch (error) {
      console.error('Failed to parse WebSocket message:', error)
    }
  }, [log, scheduleBatchUpdate, announce])

  // Handle connection open
  const handleOpen = useCallback(() => {
    log('WebSocket connected')
    
    setConnectionStatus(prev => ({
      ...prev,
      status: 'connected',
      lastConnected: new Date(),
      reconnectAttempts: 0,
    }))

    // Send authentication if token provided
    if (token && wsRef.current) {
      wsRef.current.send(JSON.stringify({
        type: 'auth',
        token,
      }))
    }

    // Re-subscribe to channels
    subscriptionsRef.current.forEach(channel => {
      if (wsRef.current?.readyState === WebSocket.OPEN) {
        wsRef.current.send(JSON.stringify({
          type: 'subscribe',
          channel,
        }))
      }
    })

    // Start ping for latency monitoring
    const pingInterval = setInterval(() => {
      if (wsRef.current?.readyState === WebSocket.OPEN) {
        lastPingRef.current = Date.now()
        wsRef.current.send(JSON.stringify({ type: 'ping', timestamp: lastPingRef.current }))
      } else {
        clearInterval(pingInterval)
      }
    }, 30000) // Ping every 30 seconds

    announce('Real-time connection established', 'polite')
  }, [log, token, announce])

  // Handle connection close
  const handleClose = useCallback((event: CloseEvent) => {
    log('WebSocket closed:', event.code, event.reason)
    
    setConnectionStatus(prev => ({
      ...prev,
      status: event.wasClean ? 'disconnected' : 'error',
    }))

    // Attempt reconnection if not a clean close
    if (!event.wasClean && connectionStatus.reconnectAttempts < maxReconnectAttempts) {
      const delay = reconnectDelay * Math.pow(2, connectionStatus.reconnectAttempts)
      
      setConnectionStatus(prev => ({
        ...prev,
        status: 'reconnecting',
        reconnectAttempts: prev.reconnectAttempts + 1,
      }))

      log(`Reconnecting in ${delay}ms (attempt ${connectionStatus.reconnectAttempts + 1})`)
      
      reconnectTimeoutRef.current = setTimeout(() => {
        connect()
      }, delay)

      announce(`Connection lost. Reconnecting in ${Math.round(delay / 1000)} seconds`, 'polite')
    } else {
      announce('Real-time connection disconnected', 'polite')
    }
  }, [log, connectionStatus.reconnectAttempts, maxReconnectAttempts, reconnectDelay, announce])

  // Handle connection error
  const handleError = useCallback((event: Event) => {
    log('WebSocket error:', event)
    
    setConnectionStatus(prev => ({
      ...prev,
      status: 'error',
    }))

    announce('Real-time connection error', 'assertive')
  }, [log, announce])

  // Connect to WebSocket
  const connect = useCallback(() => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      return // Already connected
    }

    log('Connecting to WebSocket:', endpoint)
    
    setConnectionStatus(prev => ({
      ...prev,
      status: 'connecting',
    }))

    try {
      const wsUrl = token ? `${endpoint}?token=${encodeURIComponent(token)}` : endpoint
      wsRef.current = new WebSocket(wsUrl, protocols)
      
      wsRef.current.onopen = handleOpen
      wsRef.current.onmessage = handleMessage
      wsRef.current.onclose = handleClose
      wsRef.current.onerror = handleError
    } catch (error) {
      console.error('Failed to create WebSocket connection:', error)
      setConnectionStatus(prev => ({
        ...prev,
        status: 'error',
      }))
    }
  }, [endpoint, token, protocols, handleOpen, handleMessage, handleClose, handleError, log])

  // Disconnect from WebSocket
  const disconnect = useCallback(() => {
    log('Disconnecting WebSocket')
    
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current)
      reconnectTimeoutRef.current = null
    }

    if (wsRef.current) {
      wsRef.current.close(1000, 'Manual disconnect')
      wsRef.current = null
    }

    setConnectionStatus(prev => ({
      ...prev,
      status: 'disconnected',
      reconnectAttempts: 0,
    }))

    setIsSubscribed(false)
  }, [log])

  // Subscribe to a channel
  const subscribe = useCallback((channel: string) => {
    log('Subscribing to channel:', channel)
    
    subscriptionsRef.current.add(channel)
    
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify({
        type: 'subscribe',
        channel,
      }))
    }
    
    setIsSubscribed(true)
  }, [log])

  // Unsubscribe from a channel
  const unsubscribe = useCallback((channel: string) => {
    log('Unsubscribing from channel:', channel)
    
    subscriptionsRef.current.delete(channel)
    
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify({
        type: 'unsubscribe',
        channel,
      }))
    }

    if (subscriptionsRef.current.size === 0) {
      setIsSubscribed(false)
    }
  }, [log])

  // Send message
  const send = useCallback((message: any) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify(message))
      log('Sent message:', message)
    } else {
      log('Cannot send message: WebSocket not connected')
    }
  }, [log])

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      disconnect()
      
      if (batchTimeoutRef.current) {
        clearTimeout(batchTimeoutRef.current)
      }
    }
  }, [disconnect])

  return {
    // Connection management
    connect,
    disconnect,
    connectionStatus,
    
    // Subscription management
    subscribe,
    unsubscribe,
    isSubscribed,
    
    // Data
    updates,
    
    // Communication
    send,
    
    // Utilities
    clearUpdates: () => setUpdates([]),
  }
}
