import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type {
  ComprehensiveAnalyticsDashboard,
  TemplatePerformanceMetrics,
  ABTestDetails,
  PerformanceAlert,
  RealTimeAnalyticsData
} from '@shared/types/templateAnalytics'

export interface TemplateAnalyticsHubEvents {
  DashboardUpdate: (dashboardData: ComprehensiveAnalyticsDashboard) => void
  PerformanceUpdate: (templateKey: string, performanceData: TemplatePerformanceMetrics) => void
  ABTestUpdate: (testId: number, testData: ABTestDetails) => void
  NewAlert: (alert: PerformanceAlert) => void
  RealTimeUpdate: (data: RealTimeAnalyticsData) => void
  Error: (errorMessage: string) => void
}

export class TemplateAnalyticsHub {
  private connection: HubConnection | null = null
  private isConnected = false
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000 // Start with 1 second
  private eventHandlers: Map<string, Function[]> = new Map()

  constructor(private getAuthToken: () => string | null) {}

  async connect(): Promise<void> {
    if (this.connection && this.isConnected) {
      return
    }

    try {
      // Get the backend URL from environment or default
      const backendUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:55244'
      const hubUrl = `${backendUrl}/hubs/template-analytics`

      console.log('Connecting to template analytics hub:', hubUrl)

      this.connection = new HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => {
            const token = this.getAuthToken()
            if (!token) {
              console.warn('No token available for template analytics hub')
              return ''
            }

            // Check if token is expired
            try {
              const payload = JSON.parse(atob(token.split('.')[1]))
              const currentTime = Math.floor(Date.now() / 1000)
              if (payload.exp < currentTime) {
                console.warn('Token expired for template analytics hub')
                return ''
              }
            } catch {
              console.warn('Invalid token format for template analytics hub')
              return ''
            }

            return token
          }
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff with jitter
            const delay = Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000)
            return delay + Math.random() * 1000
          }
        })
        .configureLogging(LogLevel.Information)
        .build()

      // Set up event handlers
      this.setupEventHandlers()

      // Set up connection state handlers
      this.connection.onreconnecting(() => {
        console.log('Template Analytics Hub: Reconnecting...')
        this.isConnected = false
      })

      this.connection.onreconnected(() => {
        console.log('Template Analytics Hub: Reconnected')
        this.isConnected = true
        this.reconnectAttempts = 0
        this.reconnectDelay = 1000
        // Re-subscribe to events after reconnection
        this.resubscribeToEvents()
      })

      this.connection.onclose((error) => {
        console.log('Template Analytics Hub: Connection closed', error)
        this.isConnected = false
        
        if (error && this.reconnectAttempts < this.maxReconnectAttempts) {
          this.scheduleReconnect()
        }
      })

      await this.connection.start()
      this.isConnected = true
      this.reconnectAttempts = 0
      console.log('Template Analytics Hub: Connected successfully')

    } catch (error) {
      console.error('Template Analytics Hub: Failed to connect', error)
      this.scheduleReconnect()
      throw error
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.isConnected = false
      this.eventHandlers.clear()
      console.log('Template Analytics Hub: Disconnected')
    }
  }

  private setupEventHandlers(): void {
    if (!this.connection) return

    // Dashboard updates
    this.connection.on('DashboardUpdate', (dashboardData: ComprehensiveAnalyticsDashboard) => {
      this.emit('DashboardUpdate', dashboardData)
    })

    // Performance updates
    this.connection.on('PerformanceUpdate', (templateKey: string, performanceData: TemplatePerformanceMetrics) => {
      this.emit('PerformanceUpdate', templateKey, performanceData)
    })

    // A/B test updates
    this.connection.on('ABTestUpdate', (testId: number, testData: ABTestDetails) => {
      this.emit('ABTestUpdate', testId, testData)
    })

    // New alerts
    this.connection.on('NewAlert', (alert: PerformanceAlert) => {
      this.emit('NewAlert', alert)
    })

    // Real-time updates
    this.connection.on('RealTimeUpdate', (data: RealTimeAnalyticsData) => {
      this.emit('RealTimeUpdate', data)
    })

    // Error handling
    this.connection.on('Error', (errorMessage: string) => {
      console.error('Template Analytics Hub Error:', errorMessage)
      this.emit('Error', errorMessage)
    })
  }

  private scheduleReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Template Analytics Hub: Max reconnection attempts reached')
      return
    }

    this.reconnectAttempts++
    const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1)
    
    console.log(`Template Analytics Hub: Scheduling reconnect attempt ${this.reconnectAttempts} in ${delay}ms`)
    
    setTimeout(async () => {
      try {
        await this.connect()
      } catch (error) {
        console.error('Template Analytics Hub: Reconnection failed', error)
      }
    }, delay)
  }

  private async resubscribeToEvents(): Promise<void> {
    // Re-subscribe to any active subscriptions after reconnection
    // This would be called after successful reconnection
    try {
      await this.subscribeToPerformanceUpdates()
      await this.subscribeToABTestUpdates()
      await this.subscribeToAlerts()
    } catch (error) {
      console.error('Template Analytics Hub: Failed to resubscribe to events', error)
    }
  }

  // Hub method calls
  async subscribeToPerformanceUpdates(intentType?: string): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('Not connected to Template Analytics Hub')
    }

    try {
      await this.connection.invoke('SubscribeToPerformanceUpdates', intentType)
      console.log('Template Analytics Hub: Subscribed to performance updates', { intentType })
    } catch (error) {
      console.error('Template Analytics Hub: Failed to subscribe to performance updates', error)
      throw error
    }
  }

  async subscribeToABTestUpdates(): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('Not connected to Template Analytics Hub')
    }

    try {
      await this.connection.invoke('SubscribeToABTestUpdates')
      console.log('Template Analytics Hub: Subscribed to A/B test updates')
    } catch (error) {
      console.error('Template Analytics Hub: Failed to subscribe to A/B test updates', error)
      throw error
    }
  }

  async subscribeToAlerts(): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('Not connected to Template Analytics Hub')
    }

    try {
      await this.connection.invoke('SubscribeToAlerts')
      console.log('Template Analytics Hub: Subscribed to alerts')
    } catch (error) {
      console.error('Template Analytics Hub: Failed to subscribe to alerts', error)
      throw error
    }
  }

  async getRealTimeDashboard(): Promise<RealTimeAnalyticsData> {
    if (!this.connection || !this.isConnected) {
      throw new Error('Not connected to Template Analytics Hub')
    }

    try {
      const data = await this.connection.invoke('GetRealTimeDashboard')
      console.log('Template Analytics Hub: Retrieved real-time dashboard data')
      return data
    } catch (error) {
      console.error('Template Analytics Hub: Failed to get real-time dashboard', error)
      throw error
    }
  }

  async getTemplatePerformance(templateKey: string): Promise<TemplatePerformanceMetrics> {
    if (!this.connection || !this.isConnected) {
      throw new Error('Not connected to Template Analytics Hub')
    }

    try {
      const data = await this.connection.invoke('GetTemplatePerformance', templateKey)
      console.log('Template Analytics Hub: Retrieved template performance', { templateKey })
      return data
    } catch (error) {
      console.error('Template Analytics Hub: Failed to get template performance', error)
      throw error
    }
  }

  // Event subscription methods
  on<K extends keyof TemplateAnalyticsHubEvents>(
    event: K,
    handler: TemplateAnalyticsHubEvents[K]
  ): void {
    if (!this.eventHandlers.has(event)) {
      this.eventHandlers.set(event, [])
    }
    this.eventHandlers.get(event)!.push(handler)
  }

  off<K extends keyof TemplateAnalyticsHubEvents>(
    event: K,
    handler: TemplateAnalyticsHubEvents[K]
  ): void {
    const handlers = this.eventHandlers.get(event)
    if (handlers) {
      const index = handlers.indexOf(handler)
      if (index > -1) {
        handlers.splice(index, 1)
      }
    }
  }

  private emit(event: string, ...args: any[]): void {
    const handlers = this.eventHandlers.get(event)
    if (handlers) {
      handlers.forEach(handler => {
        try {
          handler(...args)
        } catch (error) {
          console.error(`Template Analytics Hub: Error in event handler for ${event}`, error)
        }
      })
    }
  }

  // Getters
  get connected(): boolean {
    return this.isConnected
  }

  get connectionState(): string {
    return this.connection?.state || 'Disconnected'
  }
}

// Singleton instance
let templateAnalyticsHubInstance: TemplateAnalyticsHub | null = null

export const getTemplateAnalyticsHub = (getAuthToken: () => string | null): TemplateAnalyticsHub => {
  if (!templateAnalyticsHubInstance) {
    templateAnalyticsHubInstance = new TemplateAnalyticsHub(getAuthToken)
  }
  return templateAnalyticsHubInstance
}

export const disconnectTemplateAnalyticsHub = async (): Promise<void> => {
  if (templateAnalyticsHubInstance) {
    await templateAnalyticsHubInstance.disconnect()
    templateAnalyticsHubInstance = null
  }
}
