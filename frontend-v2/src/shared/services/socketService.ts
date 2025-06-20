import { io, Socket } from 'socket.io-client'
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
  private socket: Socket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000

  connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      const token = selectAccessToken(store.getState())

      if (!token) {
        reject(new Error('No authentication token available'))
        return
      }

      this.socket = io('/hub/query-status', {
        auth: {
          token,
        },
        transports: ['websocket'],
        upgrade: true,
        rememberUpgrade: true,
        timeout: 20000,
        forceNew: true,
      })

      this.socket.on('connect', () => {
        console.log('✅ Socket connected')
        this.reconnectAttempts = 0
        store.dispatch(chatActions.setIsConnected(true))
        store.dispatch(chatActions.setConnectionError(null))
        resolve()
      })

      this.socket.on('connect_error', (error) => {
        console.error('❌ Socket connection error:', error)
        store.dispatch(chatActions.setIsConnected(false))
        store.dispatch(chatActions.setConnectionError(error.message))
        reject(error)
      })

      this.socket.on('disconnect', (reason) => {
        console.log('🔌 Socket disconnected:', reason)
        store.dispatch(chatActions.setIsConnected(false))

        if (reason === 'io server disconnect') {
          // Server initiated disconnect, try to reconnect
          this.handleReconnect()
        }
      })

      this.socket.on('reconnect', (attemptNumber) => {
        console.log(`🔄 Socket reconnected after ${attemptNumber} attempts`)
        this.reconnectAttempts = 0
        store.dispatch(chatActions.setIsConnected(true))
        store.dispatch(chatActions.setConnectionError(null))
      })

      this.socket.on('reconnect_error', (error) => {
        console.error('❌ Socket reconnection error:', error)
        store.dispatch(chatActions.setConnectionError(error.message))
        this.handleReconnect()
      })

      // Set up event handlers for streaming
      this.setupEventHandlers()
    })
  }

  private setupEventHandlers() {
    if (!this.socket) return

    // Streaming query progress
    this.socket.on('StreamingQueryProgress', (data: StreamingQueryProgress) => {
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
    this.socket.on('StreamingQueryComplete', (data: StreamingQueryComplete) => {
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
    this.socket.on('StreamingQueryError', (data: StreamingQueryError) => {
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
    this.socket.on('UserTyping', (data: { userId: string; isTyping: boolean }) => {
      if (data.userId !== 'current-user') { // Don't show typing for current user
        store.dispatch(chatActions.setIsTyping(data.isTyping))
      }
    })

    // Real-time notifications
    this.socket.on('SystemNotification', (data: { type: string; message: string; data?: any }) => {
      console.log('🔔 System notification:', data)
      // Handle system notifications (new features, maintenance, etc.)
    })
  }

  private handleReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1)
      
      console.log(`🔄 Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`)
      
      setTimeout(() => {
        this.connect().catch((error) => {
          console.error('❌ Reconnection failed:', error)
        })
      }, delay)
    } else {
      console.error('❌ Max reconnection attempts reached')
    }
  }

  disconnect() {
    if (this.socket) {
      this.socket.disconnect()
      this.socket = null
    }
  }

  // Event listeners
  onStreamingQueryProgress(callback: (data: StreamingQueryProgress) => void) {
    this.socket?.on('StreamingQueryProgress', callback)
  }

  onStreamingQueryComplete(callback: (data: StreamingQueryComplete) => void) {
    this.socket?.on('StreamingQueryComplete', callback)
  }

  onStreamingQueryError(callback: (data: StreamingQueryError) => void) {
    this.socket?.on('StreamingQueryError', callback)
  }

  // Remove event listeners
  offStreamingQueryProgress(callback?: (data: StreamingQueryProgress) => void) {
    this.socket?.off('StreamingQueryProgress', callback)
  }

  offStreamingQueryComplete(callback?: (data: StreamingQueryComplete) => void) {
    this.socket?.off('StreamingQueryComplete', callback)
  }

  offStreamingQueryError(callback?: (data: StreamingQueryError) => void) {
    this.socket?.off('StreamingQueryError', callback)
  }

  // Emit events
  joinQuerySession(sessionId: string) {
    this.socket?.emit('joinQuerySession', { sessionId })
  }

  leaveQuerySession(sessionId: string) {
    this.socket?.emit('leaveQuerySession', { sessionId })
  }

  // Enhanced streaming methods
  startStreamingQuery(data: {
    query: string
    messageId: string
    conversationId?: string
    options?: any
  }) {
    this.socket?.emit('startStreamingQuery', data)
  }

  cancelStreamingQuery(sessionId: string) {
    this.socket?.emit('cancelStreamingQuery', { sessionId })
  }

  // Streaming session management
  startStreamingSession(data: {
    query: string
    sessionId?: string
    options?: {
      enableProgress: boolean
      enableSemanticAnalysis: boolean
      maxExecutionTime: number
    }
  }) {
    this.socket?.emit('startStreamingSession', data)
  }

  stopStreamingSession(sessionId: string) {
    this.socket?.emit('stopStreamingSession', { sessionId })
  }

  // Real-time dashboard subscriptions
  subscribeToDashboard() {
    this.socket?.emit('subscribeToDashboard')
  }

  unsubscribeFromDashboard() {
    this.socket?.emit('unsubscribeFromDashboard')
  }

  // Live chart subscriptions
  subscribeToLiveCharts(chartIds: string[]) {
    this.socket?.emit('subscribeToLiveCharts', { chartIds })
  }

  unsubscribeFromLiveCharts(chartIds: string[]) {
    this.socket?.emit('unsubscribeFromLiveCharts', { chartIds })
  }

  // System monitoring
  subscribeToSystemHealth() {
    this.socket?.emit('subscribeToSystemHealth')
  }

  unsubscribeFromSystemHealth() {
    this.socket?.emit('unsubscribeFromSystemHealth')
  }

  // Typing indicators
  sendTypingIndicator(isTyping: boolean, conversationId?: string) {
    this.socket?.emit('typing', { isTyping, conversationId })
  }

  // Real-time collaboration
  joinConversation(conversationId: string) {
    this.socket?.emit('joinConversation', { conversationId })
  }

  leaveConversation(conversationId: string) {
    this.socket?.emit('leaveConversation', { conversationId })
  }

  // Utility methods
  isConnected(): boolean {
    return this.socket?.connected ?? false
  }

  getSocket(): Socket | null {
    return this.socket
  }

  getConnectionState(): 'connected' | 'connecting' | 'disconnected' | 'error' {
    if (!this.socket) return 'disconnected'
    if (this.socket.connected) return 'connected'
    if (this.socket.connected === false && this.socket.disconnected === false) return 'connecting'
    return 'disconnected'
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
      console.error('Failed to connect to socket:', error)
    }
  }, [])

  const disconnect = useCallback(() => {
    socketService.disconnect()
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
    socket: socketService.getSocket(),
    onStreamingQueryProgress: socketService.onStreamingQueryProgress.bind(socketService),
    onStreamingQueryComplete: socketService.onStreamingQueryComplete.bind(socketService),
    onStreamingQueryError: socketService.onStreamingQueryError.bind(socketService),
    offStreamingQueryProgress: socketService.offStreamingQueryProgress.bind(socketService),
    offStreamingQueryComplete: socketService.offStreamingQueryComplete.bind(socketService),
    offStreamingQueryError: socketService.offStreamingQueryError.bind(socketService),
    joinQuerySession: socketService.joinQuerySession.bind(socketService),
    leaveQuerySession: socketService.leaveQuerySession.bind(socketService),
  }
}
