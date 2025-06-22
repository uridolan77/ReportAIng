import { store } from '@shared/store'
import { addTrace, updateTrace, setActiveTrace, setCurrentAnalysis } from '@shared/store/aiTransparencySlice'

/**
 * WebSocket service for real-time transparency updates
 * 
 * Connects to the backend WebSocket endpoint for live transparency data
 * Handles real-time trace updates, confidence changes, and processing steps
 */
export class TransparencyWebSocketService {
  private ws: WebSocket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000
  private isConnected = false
  private listeners: Map<string, Set<(data: any) => void>> = new Map()

  constructor(private baseUrl: string = 'ws://localhost:55244') {}

  /**
   * Connect to the transparency WebSocket
   */
  connect(token?: string): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        const wsUrl = `${this.baseUrl}/ws/transparency${token ? `?token=${token}` : ''}`
        this.ws = new WebSocket(wsUrl)

        this.ws.onopen = () => {
          console.log('🔗 Transparency WebSocket connected')
          this.isConnected = true
          this.reconnectAttempts = 0
          resolve()
        }

        this.ws.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data)
            this.handleMessage(data)
          } catch (error) {
            console.error('❌ Failed to parse WebSocket message:', error)
          }
        }

        this.ws.onclose = (event) => {
          console.log('🔌 Transparency WebSocket disconnected:', event.code, event.reason)
          this.isConnected = false
          this.handleReconnect()
        }

        this.ws.onerror = (error) => {
          console.error('❌ Transparency WebSocket error:', error)
          this.isConnected = false
          reject(error)
        }

      } catch (error) {
        reject(error)
      }
    })
  }

  /**
   * Disconnect from WebSocket
   */
  disconnect(): void {
    if (this.ws) {
      this.ws.close()
      this.ws = null
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
   * Send message to WebSocket
   */
  send(message: any): void {
    if (this.ws && this.isConnected) {
      this.ws.send(JSON.stringify(message))
    } else {
      console.warn('⚠️ WebSocket not connected, cannot send message')
    }
  }

  /**
   * Get connection status
   */
  getConnectionStatus(): boolean {
    return this.isConnected
  }

  /**
   * Handle incoming WebSocket messages
   */
  private handleMessage(data: any): void {
    const { type, payload } = data

    switch (type) {
      case 'trace_started':
        this.handleTraceStarted(payload)
        break
      case 'trace_updated':
        this.handleTraceUpdated(payload)
        break
      case 'trace_completed':
        this.handleTraceCompleted(payload)
        break
      case 'step_completed':
        this.handleStepCompleted(payload)
        break
      case 'confidence_updated':
        this.handleConfidenceUpdated(payload)
        break
      case 'error':
        this.handleError(payload)
        break
      default:
        console.log('📨 Unknown WebSocket message type:', type)
    }

    // Notify subscribers
    const listeners = this.listeners.get(type)
    if (listeners) {
      listeners.forEach(callback => callback(payload))
    }
  }

  /**
   * Handle trace started event
   */
  private handleTraceStarted(payload: any): void {
    console.log('🚀 Trace started:', payload.traceId)
    store.dispatch(addTrace({
      traceId: payload.traceId,
      steps: [],
      finalPrompt: payload.userQuestion || '',
      totalConfidence: 0,
      optimizationSuggestions: [],
      metadata: {
        modelUsed: 'unknown',
        provider: 'unknown',
        tokensUsed: 0,
        processingTime: 0
      }
    }))
    store.dispatch(setActiveTrace(payload.traceId))
  }

  /**
   * Handle trace updated event
   */
  private handleTraceUpdated(payload: any): void {
    console.log('📝 Trace updated:', payload.traceId)
    store.dispatch(updateTrace({
      id: payload.traceId,
      changes: payload.updates
    }))
  }

  /**
   * Handle trace completed event
   */
  private handleTraceCompleted(payload: any): void {
    console.log('✅ Trace completed:', payload.traceId)
    store.dispatch(updateTrace({
      id: payload.traceId,
      changes: {
        totalConfidence: payload.finalConfidence || 0,
        finalPrompt: payload.finalPrompt || '',
        metadata: {
          modelUsed: payload.modelUsed || 'unknown',
          provider: payload.provider || 'unknown',
          tokensUsed: payload.totalTokens || 0,
          processingTime: payload.processingTimeMs || 0
        }
      }
    }))
  }

  /**
   * Handle step completed event
   */
  private handleStepCompleted(payload: any): void {
    console.log('🔄 Step completed:', payload.stepName, 'for trace:', payload.traceId)
    // Update trace with new step
    store.dispatch(updateTrace({
      id: payload.traceId,
      changes: {
        steps: payload.allSteps || []
      }
    }))
  }

  /**
   * Handle confidence updated event
   */
  private handleConfidenceUpdated(payload: any): void {
    console.log('📊 Confidence updated:', payload.confidence, 'for trace:', payload.traceId)
    store.dispatch(updateTrace({
      id: payload.traceId,
      changes: {
        totalConfidence: payload.confidence || 0
      }
    }))
  }

  /**
   * Handle error event
   */
  private handleError(payload: any): void {
    console.error('❌ WebSocket error:', payload.message)
    store.dispatch(updateTrace({
      id: payload.traceId,
      changes: {
        finalPrompt: `Error: ${payload.message}`,
        totalConfidence: 0
      }
    }))
  }

  /**
   * Handle reconnection logic
   */
  private handleReconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1)
      
      console.log(`🔄 Attempting to reconnect (${this.reconnectAttempts}/${this.maxReconnectAttempts}) in ${delay}ms`)
      
      setTimeout(() => {
        this.connect()
      }, delay)
    } else {
      console.error('❌ Max reconnection attempts reached')
    }
  }
}

// Create singleton instance
export const transparencyWebSocket = new TransparencyWebSocketService()

// Export types for TypeScript
export interface TransparencyWebSocketMessage {
  type: 'trace_started' | 'trace_updated' | 'trace_completed' | 'step_completed' | 'confidence_updated' | 'error'
  payload: any
}

export interface TraceStartedPayload {
  traceId: string
  userId: string
  userQuestion: string
  intentType: string
}

export interface TraceUpdatedPayload {
  traceId: string
  updates: any
}

export interface TraceCompletedPayload {
  traceId: string
  success: boolean
  finalConfidence: number
  totalTokens: number
}

export interface StepCompletedPayload {
  traceId: string
  stepName: string
  progress: number
  allSteps: any[]
}

export interface ConfidenceUpdatedPayload {
  traceId: string
  confidence: number
  factors: any[]
}
