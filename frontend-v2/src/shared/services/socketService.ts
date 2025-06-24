import * as signalR from '@microsoft/signalr'
import { store } from '../store'
import { selectAccessToken } from '../store/auth'
import { chatActions } from '../store/chat'
import type { StreamingProgress, ChatMessage } from '../types/chat'

export interface StreamingQueryProgress {
  sessionId: string
  phase: 'parsing' | 'analyzing' | 'executing' | 'formatting' | 'complete' | 'error'
  progress: number
  message: string
  timestamp: string
  phaseData?: {
    parsing?: {
      tokensProcessed: number
      entitiesFound: string[]
    }
    analyzing?: {
      tablesIdentified: string[]
      confidence: number
    }
    executing?: {
      queryPlan?: string
      estimatedTime?: number
      rowsProcessed?: number
    }
    formatting?: {
      rowsProcessed: number
      totalRows: number
      chartsGenerated?: number
    }
  }
}

export interface StreamingQueryComplete {
  sessionId: string
  messageId: string
  sql: string
  results: any[]
  metadata: {
    executionTime: number
    rowCount: number
    columnCount: number
    queryComplexity: 'Simple' | 'Medium' | 'Complex'
    tablesUsed: string[]
    estimatedCost: number
  }
  semanticAnalysis?: any
  timestamp: string
}

export interface StreamingQueryError {
  sessionId: string
  messageId?: string
  error: {
    code: string
    message: string
    details?: any
    retryable?: boolean
  }
  timestamp: string
}

class SocketService {
  private connection: signalR.HubConnection | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000

  async connect(): Promise<void> {
    try {
      const token = selectAccessToken(store.getState())

      if (!token) {
        throw new Error('No authentication token available')
      }

      // Get the backend URL from environment or default
      const backendUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:55244'

      // Create SignalR connection to QueryStatusHub
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${backendUrl}/hubs/query-status`, {
          accessTokenFactory: () => token,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectDelay * Math.pow(2, retryContext.previousRetryCount)
            }
            return null // Stop retrying
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // Set up event handlers
      this.setupEventHandlers()

      // Start the connection
      await this.connection.start()

      console.log('✅ SignalR connected to QueryStatusHub')
      this.reconnectAttempts = 0
      store.dispatch(chatActions.setIsConnected(true))
      store.dispatch(chatActions.setConnectionError(null))

    } catch (error) {
      console.error('❌ SignalR connection error:', error)
      store.dispatch(chatActions.setIsConnected(false))
      store.dispatch(chatActions.setConnectionError((error as Error).message))
      throw error
    }
  }

  private setupEventHandlers() {
    if (!this.connection) return

    // Connection state handlers
    this.connection.onclose((error) => {
      console.log('🔌 SignalR disconnected:', error)
      store.dispatch(chatActions.setIsConnected(false))
      if (error) {
        store.dispatch(chatActions.setConnectionError(error.message))
      }
    })

    this.connection.onreconnecting((error) => {
      console.log('🔄 SignalR reconnecting:', error)
      store.dispatch(chatActions.setIsConnected(false))
    })

    this.connection.onreconnected((connectionId) => {
      console.log('✅ SignalR reconnected:', connectionId)
      store.dispatch(chatActions.setIsConnected(true))
      store.dispatch(chatActions.setConnectionError(null))
    })

    // Streaming query progress
    this.connection.on('StreamingQueryProgress', (data: StreamingQueryProgress) => {
      console.log('📊 Streaming progress:', data)

      const progressData: StreamingProgress = {
        sessionId: data.sessionId,
        phase: data.phase,
        progress: data.progress,
        message: data.message,
        timestamp: new Date(data.timestamp),
        phaseData: data.phaseData,
      }

      store.dispatch(chatActions.setStreamingProgress(progressData))
    })

    // Streaming query complete
    this.connection.on('StreamingQueryComplete', (data: StreamingQueryComplete) => {
      console.log('✅ Streaming complete:', data)

      // Update the message with results
      store.dispatch(chatActions.updateMessage({
        id: data.messageId,
        updates: {
          sql: data.sql,
          results: data.results,
          resultMetadata: data.metadata,
          semanticAnalysis: data.semanticAnalysis,
          status: 'delivered',
        },
      }))

      // Clear streaming progress
      store.dispatch(chatActions.setStreamingProgress(null))
    })

    // Streaming query error
    this.connection.on('StreamingQueryError', (data: StreamingQueryError) => {
      console.error('❌ Streaming error:', data)

      if (data.messageId) {
        // Update the message with error
        store.dispatch(chatActions.updateMessage({
          id: data.messageId,
          updates: {
            status: 'error',
            error: data.error,
          },
        }))
      }

      // Clear streaming progress
      store.dispatch(chatActions.setStreamingProgress(null))
      store.dispatch(chatActions.setError(data.error.message))
    })

    // Typing indicators
    this.connection.on('UserTyping', (data: { userId: string; isTyping: boolean }) => {
      if (data.userId !== 'current-user') { // Don't show typing for current user
        store.dispatch(chatActions.setIsTyping(data.isTyping))
      }
    })

    // Real-time notifications
    this.connection.on('SystemNotification', (data: { type: string; message: string; data?: any }) => {
      console.log('🔔 System notification:', data)
      // Handle system notifications (new features, maintenance, etc.)
    })

    // Backend-specific events (matching QueryStatusHub events)
    this.connection.on('QueryStarted', (data) => {
      console.log('🚀 Query started (backend):', data)
      // Convert to streaming progress format
      store.dispatch(chatActions.setStreamingProgress({
        sessionId: data.QueryId,
        phase: 'starting',
        progress: 0,
        message: 'Query started...',
        timestamp: new Date(data.Timestamp),
      }))
    })

    this.connection.on('ProgressUpdate', (data) => {
      console.log('📊 Progress update (backend):', data)
      // Convert to streaming progress format
      store.dispatch(chatActions.setStreamingProgress({
        sessionId: data.OperationId,
        phase: 'executing',
        progress: data.Progress,
        message: data.Message || 'Processing...',
        timestamp: new Date(data.Timestamp),
      }))
    })

    this.connection.on('JoinedQueryGroup', (queryId) => {
      console.log('✅ Joined query group (backend):', queryId)
    })
  }

  async disconnect() {
    if (this.connection) {
      try {
        await this.connection.stop()
        console.log('🔌 SignalR disconnected')
      } catch (error) {
        console.error('❌ Error disconnecting SignalR:', error)
      }
      this.connection = null
    }
  }

  // Event listeners
  onStreamingQueryProgress(callback: (data: StreamingQueryProgress) => void) {
    this.connection?.on('StreamingQueryProgress', callback)
  }

  onStreamingQueryComplete(callback: (data: StreamingQueryComplete) => void) {
    this.connection?.on('StreamingQueryComplete', callback)
  }

  onStreamingQueryError(callback: (data: StreamingQueryError) => void) {
    this.connection?.on('StreamingQueryError', callback)
  }

  // Remove event listeners
  offStreamingQueryProgress(callback?: (data: StreamingQueryProgress) => void) {
    this.connection?.off('StreamingQueryProgress', callback)
  }

  offStreamingQueryComplete(callback?: (data: StreamingQueryComplete) => void) {
    this.connection?.off('StreamingQueryComplete', callback)
  }

  offStreamingQueryError(callback?: (data: StreamingQueryError) => void) {
    this.connection?.off('StreamingQueryError', callback)
  }

  // Generic event listeners for custom events
  on(eventName: string, callback: (...args: any[]) => void) {
    this.connection?.on(eventName, callback)
  }

  off(eventName: string, callback?: (...args: any[]) => void) {
    this.connection?.off(eventName, callback)
  }

  // Invoke SignalR methods (using correct backend method names)
  async joinQuerySession(sessionId: string) {
    try {
      await this.connection?.invoke('JoinQueryGroup', sessionId)
      console.log('✅ Joined query group:', sessionId)
    } catch (error) {
      console.error('❌ Error joining query group:', error)
    }
  }

  async leaveQuerySession(sessionId: string) {
    try {
      await this.connection?.invoke('LeaveQueryGroup', sessionId)
      console.log('✅ Left query group:', sessionId)
    } catch (error) {
      console.error('❌ Error leaving query group:', error)
    }
  }

  // Enhanced streaming methods (using correct backend method names)
  async startStreamingQuery(data: {
    query: string
    messageId: string
    conversationId?: string
    options?: any
  }) {
    try {
      // Note: Backend doesn't have StartStreamingQuery method in SignalR hub
      // This should be handled via REST API, not SignalR
      console.log('⚠️ StartStreamingQuery should be called via REST API, not SignalR')
    } catch (error) {
      console.error('❌ Error starting streaming query:', error)
    }
  }

  async cancelStreamingQuery(sessionId: string) {
    try {
      await this.connection?.invoke('RequestQueryCancellation', sessionId)
      console.log('✅ Requested query cancellation:', sessionId)
    } catch (error) {
      console.error('❌ Error requesting query cancellation:', error)
    }
  }

  // Streaming session management
  async startStreamingSession(data: {
    query: string
    sessionId?: string
    options?: {
      enableProgress: boolean
      enableSemanticAnalysis: boolean
      maxExecutionTime: number
    }
  }) {
    try {
      await this.connection?.invoke('StartStreamingSession', data)
    } catch (error) {
      console.error('❌ Error starting streaming session:', error)
    }
  }

  async stopStreamingSession(sessionId: string) {
    try {
      await this.connection?.invoke('StopStreamingSession', sessionId)
    } catch (error) {
      console.error('❌ Error stopping streaming session:', error)
    }
  }

  // Real-time dashboard subscriptions
  async subscribeToDashboard() {
    try {
      await this.connection?.invoke('SubscribeToDashboard')
    } catch (error) {
      console.error('❌ Error subscribing to dashboard:', error)
    }
  }

  async unsubscribeFromDashboard() {
    try {
      await this.connection?.invoke('UnsubscribeFromDashboard')
    } catch (error) {
      console.error('❌ Error unsubscribing from dashboard:', error)
    }
  }

  // Live chart subscriptions
  async subscribeToLiveCharts(chartIds: string[]) {
    try {
      await this.connection?.invoke('SubscribeToLiveCharts', chartIds)
    } catch (error) {
      console.error('❌ Error subscribing to live charts:', error)
    }
  }

  async unsubscribeFromLiveCharts(chartIds: string[]) {
    try {
      await this.connection?.invoke('UnsubscribeFromLiveCharts', chartIds)
    } catch (error) {
      console.error('❌ Error unsubscribing from live charts:', error)
    }
  }

  // System monitoring
  async subscribeToSystemHealth() {
    try {
      await this.connection?.invoke('SubscribeToSystemHealth')
    } catch (error) {
      console.error('❌ Error subscribing to system health:', error)
    }
  }

  async unsubscribeFromSystemHealth() {
    try {
      await this.connection?.invoke('UnsubscribeFromSystemHealth')
    } catch (error) {
      console.error('❌ Error unsubscribing from system health:', error)
    }
  }

  // Typing indicators (not available on backend)
  async sendTypingIndicator(isTyping: boolean, conversationId?: string) {
    try {
      // Backend doesn't have SendTypingIndicator method
      console.log('⚠️ Typing indicators not implemented on backend')
    } catch (error) {
      console.error('❌ Error sending typing indicator:', error)
    }
  }

  // Real-time collaboration (using available backend methods)
  async joinConversation(conversationId: string) {
    try {
      // Backend doesn't have JoinConversation method, using JoinQueryGroup instead
      await this.connection?.invoke('JoinQueryGroup', conversationId)
      console.log('✅ Joined conversation group:', conversationId)
    } catch (error) {
      console.error('❌ Error joining conversation:', error)
    }
  }

  async leaveConversation(conversationId: string) {
    try {
      // Backend doesn't have LeaveConversation method, using LeaveQueryGroup instead
      await this.connection?.invoke('LeaveQueryGroup', conversationId)
      console.log('✅ Left conversation group:', conversationId)
    } catch (error) {
      console.error('❌ Error leaving conversation:', error)
    }
  }

  // Utility methods
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }

  getConnection(): signalR.HubConnection | null {
    return this.connection
  }

  getConnectionState(): 'connected' | 'connecting' | 'disconnected' | 'error' {
    if (!this.connection) return 'disconnected'

    switch (this.connection.state) {
      case signalR.HubConnectionState.Connected:
        return 'connected'
      case signalR.HubConnectionState.Connecting:
      case signalR.HubConnectionState.Reconnecting:
        return 'connecting'
      case signalR.HubConnectionState.Disconnected:
        return 'disconnected'
      default:
        return 'error'
    }
  }
}

// Create singleton instance
export const socketService = new SocketService()

// React hook for using socket service
import { useEffect, useCallback } from 'react'

export const useSocket = () => {
  const connect = useCallback(async () => {
    try {
      await socketService.connect()
    } catch (error) {
      console.error('Failed to connect to SignalR:', error)
    }
  }, [])

  const disconnect = useCallback(async () => {
    await socketService.disconnect()
  }, [])

  useEffect(() => {
    return () => {
      // Cleanup on unmount
      socketService.disconnect()
    }
  }, [])

  return {
    connect,
    disconnect,
    isConnected: socketService.isConnected(),
    connection: socketService.getConnection(),
    onStreamingQueryProgress: socketService.onStreamingQueryProgress.bind(socketService),
    onStreamingQueryComplete: socketService.onStreamingQueryComplete.bind(socketService),
    onStreamingQueryError: socketService.onStreamingQueryError.bind(socketService),
    offStreamingQueryProgress: socketService.offStreamingQueryProgress.bind(socketService),
    offStreamingQueryComplete: socketService.offStreamingQueryComplete.bind(socketService),
    offStreamingQueryError: socketService.offStreamingQueryError.bind(socketService),
    joinQuerySession: socketService.joinQuerySession.bind(socketService),
    leaveQuerySession: socketService.leaveQuerySession.bind(socketService),
    on: socketService.on.bind(socketService),
    off: socketService.off.bind(socketService),
  }
}
