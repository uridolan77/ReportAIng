/**
 * Real-time Service
 * 
 * Provides WebSocket connections, real-time updates, and live data streaming
 * for the BI Reporting Copilot application.
 */

import { io, Socket } from 'socket.io-client'

export interface RealTimeEvent {
  type: string
  data: any
  timestamp: string
  userId?: string
}

export interface QueryProgress {
  queryId: string
  stage: 'analyzing' | 'generating' | 'executing' | 'processing' | 'complete' | 'error'
  progress: number
  message?: string
  estimatedTimeRemaining?: number
}

export interface SystemMetrics {
  activeUsers: number
  activeQueries: number
  systemLoad: number
  memoryUsage: number
  responseTime: number
  timestamp: string
}

export interface CostAlert {
  id: string
  type: 'budget_exceeded' | 'unusual_spike' | 'optimization_opportunity'
  severity: 'low' | 'medium' | 'high' | 'critical'
  message: string
  timestamp: string
  acknowledged: boolean
}

class RealTimeService {
  private socket: Socket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000
  private eventListeners: Map<string, Set<Function>> = new Map()

  constructor() {
    // Disabled auto-connect to avoid conflicts with SignalR socketService
    // this.connect()
  }

  private connect() {
    // Use the backend API URL for socket connection
    const backendUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:55244'
    const socketUrl = import.meta.env.VITE_SOCKET_URL || backendUrl

    this.socket = io(socketUrl, {
      transports: ['websocket'],
      autoConnect: true,
      reconnection: true,
      reconnectionAttempts: this.maxReconnectAttempts,
      reconnectionDelay: this.reconnectDelay,
      auth: {
        token: localStorage.getItem('accessToken')
      }
    })

    this.setupEventHandlers()
  }

  private setupEventHandlers() {
    if (!this.socket) return

    this.socket.on('connect', () => {
      console.log('🔌 Real-time connection established')
      this.reconnectAttempts = 0
      this.emit('connection', { status: 'connected' })
    })

    this.socket.on('disconnect', (reason) => {
      console.log('🔌 Real-time connection lost:', reason)
      this.emit('connection', { status: 'disconnected', reason })
    })

    this.socket.on('connect_error', (error) => {
      console.error('🔌 Real-time connection error:', error)
      this.emit('connection', { status: 'error', error })
    })

    this.socket.on('reconnect', (attemptNumber) => {
      console.log(`🔌 Reconnected after ${attemptNumber} attempts`)
      this.emit('connection', { status: 'reconnected', attempts: attemptNumber })
    })

    this.socket.on('reconnect_failed', () => {
      console.error('🔌 Failed to reconnect after maximum attempts')
      this.emit('connection', { status: 'failed' })
    })

    // Query progress updates
    this.socket.on('query_progress', (data: QueryProgress) => {
      this.emit('queryProgress', data)
    })

    // System metrics updates
    this.socket.on('system_metrics', (data: SystemMetrics) => {
      this.emit('systemMetrics', data)
    })

    // Cost alerts
    this.socket.on('cost_alert', (data: CostAlert) => {
      this.emit('costAlert', data)
    })

    // User activity updates
    this.socket.on('user_activity', (data: any) => {
      this.emit('userActivity', data)
    })

    // Dashboard updates
    this.socket.on('dashboard_update', (data: any) => {
      this.emit('dashboardUpdate', data)
    })
  }

  // Event subscription
  public on(event: string, callback: Function): () => void {
    if (!this.eventListeners.has(event)) {
      this.eventListeners.set(event, new Set())
    }
    
    this.eventListeners.get(event)!.add(callback)

    // Return unsubscribe function
    return () => {
      const listeners = this.eventListeners.get(event)
      if (listeners) {
        listeners.delete(callback)
        if (listeners.size === 0) {
          this.eventListeners.delete(event)
        }
      }
    }
  }

  // Emit event to listeners
  private emit(event: string, data: any) {
    const listeners = this.eventListeners.get(event)
    if (listeners) {
      listeners.forEach(callback => {
        try {
          callback(data)
        } catch (error) {
          console.error(`Error in event listener for ${event}:`, error)
        }
      })
    }
  }

  // Send message to server
  public send(event: string, data: any) {
    if (this.socket?.connected) {
      this.socket.emit(event, data)
    } else {
      console.warn('Cannot send message: Socket not connected')
    }
  }

  // Subscribe to query progress
  public subscribeToQueryProgress(queryId: string, callback: (progress: QueryProgress) => void) {
    const unsubscribe = this.on('queryProgress', (data: QueryProgress) => {
      if (data.queryId === queryId) {
        callback(data)
      }
    })

    // Request progress updates for this query
    this.send('subscribe_query_progress', { queryId })

    return unsubscribe
  }

  // Subscribe to system metrics
  public subscribeToSystemMetrics(callback: (metrics: SystemMetrics) => void) {
    return this.on('systemMetrics', callback)
  }

  // Subscribe to cost alerts
  public subscribeToCostAlerts(callback: (alert: CostAlert) => void) {
    return this.on('costAlert', callback)
  }

  // Subscribe to dashboard updates
  public subscribeToDashboardUpdates(callback: (data: any) => void) {
    return this.on('dashboardUpdate', callback)
  }

  // Get connection status
  public isConnected(): boolean {
    return this.socket?.connected || false
  }

  // Manually reconnect
  public reconnect() {
    if (this.socket) {
      this.socket.disconnect()
      this.socket.connect()
    }
  }

  // Disconnect
  public disconnect() {
    if (this.socket) {
      this.socket.disconnect()
      this.socket = null
    }
    this.eventListeners.clear()
  }

  // Mock functionality removed
}

// Singleton instance
export const realTimeService = new RealTimeService()

// React hook for real-time features
export const useRealTime = () => {
  return {
    service: realTimeService,
    isConnected: realTimeService.isConnected(),
    on: realTimeService.on.bind(realTimeService),
    send: realTimeService.send.bind(realTimeService),
    subscribeToQueryProgress: realTimeService.subscribeToQueryProgress.bind(realTimeService),
    subscribeToSystemMetrics: realTimeService.subscribeToSystemMetrics.bind(realTimeService),
    subscribeToCostAlerts: realTimeService.subscribeToCostAlerts.bind(realTimeService),
    subscribeToDashboardUpdates: realTimeService.subscribeToDashboardUpdates.bind(realTimeService)
  }
}

// Real-time service initialized
// Mock functionality has been removed in favor of actual WebSocket connections
