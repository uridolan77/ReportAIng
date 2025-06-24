import * as signalR from '@microsoft/signalr'
import { store } from '@shared/store'
import { addTrace, updateTrace, setActiveTrace } from '@shared/store/aiTransparencySlice'

/**
 * SignalR service for real-time transparency updates
 * 
 * Connects to the backend SignalR hub for live transparency data
 * Provides real-time updates for query processing, confidence changes, and trace completion
 */
export class TransparencySignalRService {
  private connection: signalR.HubConnection | null = null
  private isConnected = false
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private listeners: Map<string, Set<(data: any) => void>> = new Map()

  constructor(private hubUrl?: string) {
    // Use environment variable or default
    this.hubUrl = hubUrl || `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:55244'}/hubs/transparency`
  }

  /**
   * Start SignalR connection
   */
  async connect(token?: string): Promise<void> {
    try {
      // Check if token is provided and valid
      if (!token) {
        console.warn('No token provided for transparency SignalR connection')
        throw new Error('No authentication token provided')
      }

      // Check if token is expired
      try {
        const payload = JSON.parse(atob(token.split('.')[1]))
        const currentTime = Math.floor(Date.now() / 1000)
        if (payload.exp < currentTime) {
          console.warn('Token expired for transparency SignalR connection')
          throw new Error('Authentication token is expired')
        }
      } catch (parseError) {
        console.warn('Invalid token format for transparency SignalR connection')
        throw new Error('Invalid authentication token format')
      }

      console.log('Connecting to transparency SignalR hub:', this.hubUrl)

      // Build connection
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => {
            // Always return the provided token
            if (!token) {
              console.warn('Token missing during SignalR connection')
              return ''
            }
            return token
          },
          transport: signalR.HttpTransportType.WebSockets,
          skipNegotiation: true
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < 5) {
              return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000)
            }
            return null // Stop retrying after 5 attempts
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // Set up event handlers
      this.setupEventHandlers()

      // Start connection
      await this.connection.start()
      console.log('🔗 Transparency SignalR connected')
      this.isConnected = true
      this.reconnectAttempts = 0

      // Join transparency group for real-time updates
      await this.connection.invoke('JoinTransparencyGroup')

    } catch (error) {
      console.error('❌ Failed to connect to Transparency SignalR:', error)
      this.isConnected = false
      throw error
    }
  }

  /**
   * Stop SignalR connection
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop()
        console.log('🔌 Transparency SignalR disconnected')
      } catch (error) {
        console.error('❌ Error disconnecting SignalR:', error)
      }
      this.connection = null
      this.isConnected = false
    }
  }

  /**
   * Subscribe to specific transparency events
   */
  subscribe(eventType: string, callback: (data: any) => void): () => void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, new Set())
    }
    this.listeners.get(eventType)!.add(callback)

    // Return unsubscribe function
    return () => {
      const listeners = this.listeners.get(eventType)
      if (listeners) {
        listeners.delete(callback)
        if (listeners.size === 0) {
          this.listeners.delete(eventType)
        }
      }
    }
  }

  /**
   * Send message to SignalR hub
   */
  async invoke(methodName: string, ...args: any[]): Promise<any> {
    if (this.connection && this.isConnected) {
      try {
        return await this.connection.invoke(methodName, ...args)
      } catch (error) {
        console.error(`❌ Failed to invoke ${methodName}:`, error)
        throw error
      }
    } else {
      throw new Error('SignalR connection not established')
    }
  }

  /**
   * Get connection status
   */
  getConnectionStatus(): boolean {
    return this.isConnected && this.connection?.state === signalR.HubConnectionState.Connected
  }

  /**
   * Get connection state
   */
  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state || null
  }

  /**
   * Set up SignalR event handlers
   */
  private setupEventHandlers(): void {
    if (!this.connection) return

    // Connection state events
    this.connection.onclose((error) => {
      console.log('🔌 SignalR connection closed:', error)
      this.isConnected = false
    })

    this.connection.onreconnecting((error) => {
      console.log('🔄 SignalR reconnecting:', error)
      this.isConnected = false
    })

    this.connection.onreconnected((connectionId) => {
      console.log('🔗 SignalR reconnected:', connectionId)
      this.isConnected = true
      // Rejoin transparency group after reconnection
      this.connection?.invoke('JoinTransparencyGroup').catch(console.error)
    })

    // Transparency-specific events
    this.connection.on('TraceStarted', (data) => {
      console.log('🚀 Trace started via SignalR:', data.traceId)
      this.handleTraceStarted(data)
      this.notifyListeners('TraceStarted', data)
    })

    this.connection.on('TraceUpdated', (data) => {
      console.log('📝 Trace updated via SignalR:', data.traceId)
      this.handleTraceUpdated(data)
      this.notifyListeners('TraceUpdated', data)
    })

    this.connection.on('TraceCompleted', (data) => {
      console.log('✅ Trace completed via SignalR:', data.traceId)
      this.handleTraceCompleted(data)
      this.notifyListeners('TraceCompleted', data)
    })

    this.connection.on('StepCompleted', (data) => {
      console.log('🔄 Step completed via SignalR:', data.stepName)
      this.handleStepCompleted(data)
      this.notifyListeners('StepCompleted', data)
    })

    this.connection.on('ConfidenceUpdated', (data) => {
      console.log('📊 Confidence updated via SignalR:', data.confidence)
      this.handleConfidenceUpdated(data)
      this.notifyListeners('ConfidenceUpdated', data)
    })

    this.connection.on('ProcessingError', (data) => {
      console.error('❌ Processing error via SignalR:', data.message)
      this.handleProcessingError(data)
      this.notifyListeners('ProcessingError', data)
    })

    // Real-time monitoring events
    this.connection.on('MetricsUpdated', (data) => {
      console.log('📈 Metrics updated via SignalR')
      this.notifyListeners('MetricsUpdated', data)
    })

    this.connection.on('SystemStatus', (data) => {
      console.log('🖥️ System status via SignalR:', data.status)
      this.notifyListeners('SystemStatus', data)
    })
  }

  /**
   * Handle trace started event
   */
  private handleTraceStarted(data: any): void {
    store.dispatch(addTrace({
      traceId: data.traceId,
      steps: [],
      finalPrompt: data.userQuestion || '',
      totalConfidence: 0,
      optimizationSuggestions: [],
      metadata: {
        modelUsed: 'unknown',
        provider: 'unknown',
        tokensUsed: 0,
        processingTime: 0
      }
    }))
    store.dispatch(setActiveTrace(data.traceId))
  }

  /**
   * Handle trace updated event
   */
  private handleTraceUpdated(data: any): void {
    store.dispatch(updateTrace({
      id: data.traceId,
      changes: {
        ...data.updates,
        lastUpdated: new Date().toISOString()
      }
    }))
  }

  /**
   * Handle trace completed event
   */
  private handleTraceCompleted(data: any): void {
    store.dispatch(updateTrace({
      id: data.traceId,
      changes: {
        totalConfidence: data.finalConfidence || 0,
        finalPrompt: data.finalPrompt || '',
        metadata: {
          modelUsed: data.modelUsed || 'unknown',
          provider: data.provider || 'unknown',
          tokensUsed: data.totalTokens || 0,
          processingTime: data.processingTimeMs || 0
        }
      }
    }))
  }

  /**
   * Handle step completed event
   */
  private handleStepCompleted(data: any): void {
    store.dispatch(updateTrace({
      id: data.traceId,
      changes: {
        steps: data.allSteps || []
      }
    }))
  }

  /**
   * Handle confidence updated event
   */
  private handleConfidenceUpdated(data: any): void {
    store.dispatch(updateTrace({
      id: data.traceId,
      changes: {
        totalConfidence: data.confidence || 0
      }
    }))
  }

  /**
   * Handle processing error event
   */
  private handleProcessingError(data: any): void {
    store.dispatch(updateTrace({
      id: data.traceId,
      changes: {
        finalPrompt: `Error: ${data.message}`,
        totalConfidence: 0
      }
    }))
  }

  /**
   * Notify event listeners
   */
  private notifyListeners(eventType: string, data: any): void {
    const listeners = this.listeners.get(eventType)
    if (listeners) {
      listeners.forEach(callback => {
        try {
          callback(data)
        } catch (error) {
          console.error(`❌ Error in ${eventType} listener:`, error)
        }
      })
    }
  }
}

// Create singleton instance
export const transparencySignalR = new TransparencySignalRService()

// Export types for TypeScript
export interface SignalRTraceEvent {
  traceId: string
  userId: string
  userQuestion: string
  intentType: string
  startedAt?: string
}

export interface SignalRStepEvent {
  traceId: string
  stepName: string
  progress: number
  allSteps?: any[]
}

export interface SignalRConfidenceEvent {
  traceId: string
  confidence: number
  factors?: any[]
  history?: any[]
}

export interface SignalRErrorEvent {
  traceId: string
  message: string
  details?: any
}
