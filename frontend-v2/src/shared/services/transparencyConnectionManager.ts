import { transparencyWebSocket } from './transparencyWebSocket'
import { transparencySignalR } from './transparencySignalR'
import { store } from '@shared/store'
import { selectUser } from '@shared/store/auth'

/**
 * Connection Manager for Transparency Real-time Updates
 * 
 * Manages both WebSocket and SignalR connections for transparency features
 * Provides fallback mechanisms and connection health monitoring
 */
export class TransparencyConnectionManager {
  private isInitialized = false
  private preferredConnection: 'signalr' | 'websocket' = 'signalr'
  private activeConnection: 'signalr' | 'websocket' | null = null
  private connectionHealthInterval: NodeJS.Timeout | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 3

  /**
   * Initialize the connection manager
   */
  async initialize(options?: {
    preferredConnection?: 'signalr' | 'websocket'
    autoConnect?: boolean
  }): Promise<void> {
    if (this.isInitialized) {
      console.log('🔗 Transparency connection manager already initialized')
      return
    }

    this.preferredConnection = options?.preferredConnection || 'signalr'
    
    console.log(`🚀 Initializing transparency connection manager (preferred: ${this.preferredConnection})`)

    if (options?.autoConnect !== false) {
      await this.connect()
    }

    this.startHealthMonitoring()
    this.isInitialized = true
  }

  /**
   * Connect using the preferred method with fallback
   */
  async connect(): Promise<void> {
    const state = store.getState()
    const user = selectUser(state)
    const token = user?.token

    try {
      if (this.preferredConnection === 'signalr') {
        await this.connectSignalR(token)
      } else {
        await this.connectWebSocket(token)
      }
    } catch (error) {
      console.warn(`⚠️ Failed to connect via ${this.preferredConnection}, trying fallback`)
      await this.connectFallback(token)
    }
  }

  /**
   * Connect via SignalR
   */
  private async connectSignalR(token?: string): Promise<void> {
    try {
      await transparencySignalR.connect(token)
      this.activeConnection = 'signalr'
      this.reconnectAttempts = 0
      console.log('✅ Connected to transparency via SignalR')
    } catch (error) {
      console.error('❌ SignalR connection failed:', error)
      throw error
    }
  }

  /**
   * Connect via WebSocket
   */
  private async connectWebSocket(token?: string): Promise<void> {
    try {
      await transparencyWebSocket.connect(token)
      this.activeConnection = 'websocket'
      this.reconnectAttempts = 0
      console.log('✅ Connected to transparency via WebSocket')
    } catch (error) {
      console.error('❌ WebSocket connection failed:', error)
      throw error
    }
  }

  /**
   * Try fallback connection method
   */
  private async connectFallback(token?: string): Promise<void> {
    const fallbackMethod = this.preferredConnection === 'signalr' ? 'websocket' : 'signalr'
    
    try {
      if (fallbackMethod === 'signalr') {
        await this.connectSignalR(token)
      } else {
        await this.connectWebSocket(token)
      }
      console.log(`✅ Connected to transparency via fallback (${fallbackMethod})`)
    } catch (error) {
      console.error('❌ All connection methods failed:', error)
      throw new Error('Failed to establish transparency connection')
    }
  }

  /**
   * Disconnect from all services
   */
  async disconnect(): Promise<void> {
    console.log('🔌 Disconnecting transparency connections')
    
    this.stopHealthMonitoring()
    
    if (this.activeConnection === 'signalr' || !this.activeConnection) {
      await transparencySignalR.disconnect()
    }
    
    if (this.activeConnection === 'websocket' || !this.activeConnection) {
      transparencyWebSocket.disconnect()
    }
    
    this.activeConnection = null
    this.isInitialized = false
  }

  /**
   * Get current connection status
   */
  getConnectionStatus(): {
    isConnected: boolean
    activeConnection: string | null
    signalRStatus: boolean
    webSocketStatus: boolean
  } {
    return {
      isConnected: this.isConnected(),
      activeConnection: this.activeConnection,
      signalRStatus: transparencySignalR.getConnectionStatus(),
      webSocketStatus: transparencyWebSocket.getConnectionStatus()
    }
  }

  /**
   * Check if any connection is active
   */
  isConnected(): boolean {
    return transparencySignalR.getConnectionStatus() || transparencyWebSocket.getConnectionStatus()
  }

  /**
   * Subscribe to transparency events across all connections
   */
  subscribe(eventType: string, callback: (data: any) => void): () => void {
    const unsubscribeSignalR = transparencySignalR.subscribe(eventType, callback)
    const unsubscribeWebSocket = transparencyWebSocket.subscribe(eventType, callback)

    // Return combined unsubscribe function
    return () => {
      unsubscribeSignalR()
      unsubscribeWebSocket()
    }
  }

  /**
   * Send message via active connection
   */
  async send(message: any): Promise<void> {
    if (this.activeConnection === 'signalr' && transparencySignalR.getConnectionStatus()) {
      await transparencySignalR.invoke('SendMessage', message)
    } else if (this.activeConnection === 'websocket' && transparencyWebSocket.getConnectionStatus()) {
      transparencyWebSocket.send(message)
    } else {
      throw new Error('No active transparency connection available')
    }
  }

  /**
   * Start connection health monitoring
   */
  private startHealthMonitoring(): void {
    this.connectionHealthInterval = setInterval(() => {
      this.checkConnectionHealth()
    }, 10000) // Check every 10 seconds
  }

  /**
   * Stop connection health monitoring
   */
  private stopHealthMonitoring(): void {
    if (this.connectionHealthInterval) {
      clearInterval(this.connectionHealthInterval)
      this.connectionHealthInterval = null
    }
  }

  /**
   * Check connection health and attempt reconnection if needed
   */
  private async checkConnectionHealth(): Promise<void> {
    const isConnected = this.isConnected()
    
    if (!isConnected && this.reconnectAttempts < this.maxReconnectAttempts) {
      console.log(`🔄 Connection lost, attempting reconnection (${this.reconnectAttempts + 1}/${this.maxReconnectAttempts})`)
      this.reconnectAttempts++
      
      try {
        await this.connect()
        console.log('✅ Reconnection successful')
      } catch (error) {
        console.error('❌ Reconnection failed:', error)
      }
    } else if (!isConnected && this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('❌ Max reconnection attempts reached, stopping health monitoring')
      this.stopHealthMonitoring()
    }
  }

  /**
   * Reset reconnection attempts
   */
  resetReconnectionAttempts(): void {
    this.reconnectAttempts = 0
  }

  /**
   * Switch to different connection method
   */
  async switchConnection(method: 'signalr' | 'websocket'): Promise<void> {
    console.log(`🔄 Switching transparency connection to ${method}`)
    
    // Disconnect current connection
    if (this.activeConnection) {
      if (this.activeConnection === 'signalr') {
        await transparencySignalR.disconnect()
      } else {
        transparencyWebSocket.disconnect()
      }
    }

    // Connect with new method
    this.preferredConnection = method
    await this.connect()
  }
}

// Create singleton instance
export const transparencyConnectionManager = new TransparencyConnectionManager()

// Auto-initialize when imported
transparencyConnectionManager.initialize({ autoConnect: true }).catch(error => {
  console.error('❌ Failed to auto-initialize transparency connection manager:', error)
})

// Export types
export interface ConnectionStatus {
  isConnected: boolean
  activeConnection: string | null
  signalRStatus: boolean
  webSocketStatus: boolean
}

export interface ConnectionOptions {
  preferredConnection?: 'signalr' | 'websocket'
  autoConnect?: boolean
}
