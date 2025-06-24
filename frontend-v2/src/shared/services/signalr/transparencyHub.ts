import { HubConnectionBuilder, HubConnection, HubConnectionState } from '@microsoft/signalr'
import { store } from '@shared/store'
import { selectAccessToken } from '@shared/store/auth'
import { validateTokenForSignalR, isAuthenticationError } from '@shared/utils/tokenUtils'

// Global flag to prevent reconnection attempts when authentication fails
let authenticationFailed = false

/**
 * TransparencyHubService - SignalR service for real-time transparency updates
 * 
 * Provides real-time communication with the backend transparency hub
 * Handles connection management, event subscriptions, and error recovery
 */
class TransparencyHubService {
  private connection: HubConnection | null = null
  private listeners: Map<string, Function[]> = new Map()
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private isConnecting = false

  /**
   * Connect to the transparency SignalR hub
   */
  async connect(): Promise<void> {
    if (this.isConnecting || this.connection?.state === HubConnectionState.Connected) {
      return
    }

    this.isConnecting = true

    try {
      const state = store.getState()
      const token = selectAccessToken(state)

      // Validate token before attempting connection
      const tokenValidation = validateTokenForSignalR(token)
      if (!tokenValidation.isValid) {
        console.warn(`Cannot connect to SignalR: ${tokenValidation.reason}`)
        authenticationFailed = true
        throw new Error(`Authentication failed: ${tokenValidation.reason}`)
      }

      // Reset authentication failure flag if we have a valid token
      if (authenticationFailed) {
        console.log('🔐 Valid token detected - resetting authentication failure flag')
        authenticationFailed = false
      }

      // Get the backend URL from environment or default
      const backendUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:55244'
      const hubUrl = `${backendUrl}/hubs/transparency`

      console.log('Connecting to transparency hub:', hubUrl)

      this.connection = new HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => {
            // Always get fresh token from store
            const currentState = store.getState()
            const currentToken = selectAccessToken(currentState)

            const validation = validateTokenForSignalR(currentToken)
            if (!validation.isValid) {
              console.warn(`Token validation failed during SignalR connection: ${validation.reason}`)
              return ''
            }

            return validation.token!
          },
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 2s, 4s, 8s, 16s, 30s
            const delay = Math.min(2000 * Math.pow(2, retryContext.previousRetryCount), 30000)
            console.log(`SignalR reconnect attempt ${retryContext.previousRetryCount + 1}, delay: ${delay}ms`)
            return delay
          }
        })
        .build()

      // Setup event handlers
      this.setupEventHandlers()

      // Setup connection event handlers
      this.connection.onreconnecting(() => {
        console.log('Transparency hub reconnecting...')
        this.emit('connectionStateChanged', { state: 'reconnecting' })
      })

      this.connection.onreconnected(() => {
        console.log('Transparency hub reconnected')
        this.reconnectAttempts = 0
        this.emit('connectionStateChanged', { state: 'connected' })
      })

      this.connection.onclose((error) => {
        console.log('Transparency hub connection closed', error)
        this.emit('connectionStateChanged', { state: 'disconnected', error })

        // Don't attempt to reconnect if the error is due to authentication
        if (error && isAuthenticationError(error)) {
          console.warn('SignalR connection closed due to authentication failure - not attempting reconnect')
          authenticationFailed = true
          this.emit('authenticationFailed', { error })
          return
        }

        this.handleConnectionClosed(error)
      })

      await this.connection.start()
      console.log('Transparency hub connected successfully')
      this.reconnectAttempts = 0
      this.emit('connectionStateChanged', { state: 'connected' })

    } catch (error) {
      console.error('Failed to connect to transparency hub:', error)
      this.emit('connectionStateChanged', { state: 'error', error })
      throw error
    } finally {
      this.isConnecting = false
    }
  }

  /**
   * Setup SignalR event handlers for transparency events
   */
  private setupEventHandlers(): void {
    if (!this.connection) return

    // Real-time transparency updates
    this.connection.on('TransparencyUpdate', (update) => {
      console.log('Transparency update received:', update)
      this.emit('transparencyUpdate', update)
    })

    // Step completion notifications
    this.connection.on('StepCompleted', (data) => {
      console.log('Step completed:', data)
      this.emit('stepCompleted', data)
    })

    // Real-time confidence changes
    this.connection.on('ConfidenceUpdate', (data) => {
      console.log('Confidence updated:', data)
      this.emit('confidenceUpdate', data)
    })

    // Trace completion
    this.connection.on('TraceCompleted', (data) => {
      console.log('Trace completed:', data)
      this.emit('traceCompleted', data)
    })

    // Error notifications
    this.connection.on('TransparencyError', (data) => {
      console.error('Transparency error:', data)
      this.emit('transparencyError', data)
    })

    // Optimization suggestions
    this.connection.on('OptimizationSuggestion', (data) => {
      console.log('Optimization suggestion:', data)
      this.emit('optimizationSuggestion', data)
    })

    // Performance metrics updates
    this.connection.on('MetricsUpdate', (data) => {
      console.log('Metrics updated:', data)
      this.emit('metricsUpdate', data)
    })
  }

  /**
   * Handle connection closed event
   */
  private async handleConnectionClosed(error?: Error): Promise<void> {
    // Don't attempt to reconnect if authentication has failed
    if (authenticationFailed) {
      console.warn('Not attempting reconnect - authentication failed')
      this.emit('connectionStateChanged', { state: 'failed', error })
      return
    }

    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      const delay = Math.min(2000 * Math.pow(2, this.reconnectAttempts - 1), 30000)

      console.log(`Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`)

      setTimeout(() => {
        this.connect().catch(err => {
          console.error('Reconnection attempt failed:', err)
        })
      }, delay)
    } else {
      console.error('Max reconnection attempts reached')
      this.emit('connectionStateChanged', { state: 'failed', error })
    }
  }

  /**
   * Disconnect from the transparency hub
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.listeners.clear()
      console.log('Transparency hub disconnected')
    }
  }

  /**
   * Subscribe to specific trace updates
   */
  async subscribeToTrace(traceId: string): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) {
      try {
        await this.connection.invoke('SubscribeToTrace', traceId)
        console.log(`Subscribed to trace: ${traceId}`)
      } catch (error) {
        console.error(`Failed to subscribe to trace ${traceId}:`, error)
        throw error
      }
    } else {
      throw new Error('Hub connection is not established')
    }
  }

  /**
   * Unsubscribe from specific trace updates
   */
  async unsubscribeFromTrace(traceId: string): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) {
      try {
        await this.connection.invoke('UnsubscribeFromTrace', traceId)
        console.log(`Unsubscribed from trace: ${traceId}`)
      } catch (error) {
        console.error(`Failed to unsubscribe from trace ${traceId}:`, error)
        throw error
      }
    }
  }

  /**
   * Get current transparency status
   */
  async getTransparencyStatus(): Promise<any> {
    if (this.connection?.state === HubConnectionState.Connected) {
      try {
        return await this.connection.invoke('GetTransparencyStatus')
      } catch (error) {
        console.error('Failed to get transparency status:', error)
        throw error
      }
    } else {
      throw new Error('Hub connection is not established')
    }
  }

  /**
   * Subscribe to events
   */
  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, [])
    }
    this.listeners.get(event)!.push(callback)
  }

  /**
   * Unsubscribe from events
   */
  off(event: string, callback?: Function): void {
    if (!callback) {
      this.listeners.delete(event)
      return
    }

    const callbacks = this.listeners.get(event)
    if (callbacks) {
      const index = callbacks.indexOf(callback)
      if (index > -1) {
        callbacks.splice(index, 1)
      }
    }
  }

  /**
   * Emit events to listeners
   */
  private emit(event: string, data: any): void {
    const callbacks = this.listeners.get(event) || []
    callbacks.forEach(callback => {
      try {
        callback(data)
      } catch (error) {
        console.error(`Error in event callback for ${event}:`, error)
      }
    })
  }

  /**
   * Get connection state
   */
  get connectionState(): HubConnectionState | null {
    return this.connection?.state || null
  }

  /**
   * Check if connected
   */
  get isConnected(): boolean {
    return this.connection?.state === HubConnectionState.Connected
  }

  /**
   * Reset authentication failure flag (call when new valid token is available)
   */
  static resetAuthenticationFailure(): void {
    authenticationFailed = false
    console.log('🔐 Authentication failure flag reset - SignalR can attempt connections again')
  }
}

// Export singleton instance
export const transparencyHub = new TransparencyHubService()

// Export types for TypeScript
export interface TransparencyUpdateEvent {
  traceId: string
  step: string
  progress: number
  confidence: number
  timestamp: string
}

export interface StepCompletedEvent {
  traceId: string
  step: {
    id: string
    stepName: string
    stepOrder: number
    success: boolean
    confidence: number
    tokensAdded: number
    content: string
    details: any
  }
}

export interface ConfidenceUpdateEvent {
  traceId: string
  confidence: number
  factors: Array<{
    factorName: string
    score: number
    weight: number
  }>
  timestamp: string
}

export interface TraceCompletedEvent {
  traceId: string
  success: boolean
  finalConfidence: number
  totalTokens: number
  processingTime: number
  timestamp: string
}

export interface TransparencyErrorEvent {
  traceId?: string
  error: string
  details?: any
  timestamp: string
}

export interface OptimizationSuggestionEvent {
  traceId: string
  suggestion: {
    suggestionId: string
    type: string
    title: string
    description: string
    priority: string
    estimatedTokenSaving: number
    estimatedPerformanceGain: number
  }
}

export interface MetricsUpdateEvent {
  type: 'confidence' | 'performance' | 'usage'
  data: any
  timestamp: string
}

export interface ConnectionStateEvent {
  state: 'connecting' | 'connected' | 'reconnecting' | 'disconnected' | 'error' | 'failed'
  error?: Error
}
