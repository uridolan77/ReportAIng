// AI Streaming API Types
// Based on the new AIStreamingController.cs backend DTOs

export interface StreamingSession {
  sessionId: string
  userId: string
  query: string
  status: 'active' | 'completed' | 'cancelled' | 'error'
  startTime: string
  endTime?: string
  currentPhase: string
  progress: number
  confidence: number
  estimatedTimeRemaining?: string
  metadata: Record<string, any>
  error?: string
}

export interface StreamingQueryResponse {
  sessionId: string
  type: 'progress' | 'result' | 'insight' | 'error' | 'complete'
  timestamp: string
  data: any
  metadata?: Record<string, any>
}

export interface StreamingInsightResponse {
  insightId: string
  queryId: string
  type: 'pattern' | 'anomaly' | 'trend' | 'recommendation' | 'warning'
  title: string
  description: string
  confidence: number
  data?: any
  timestamp: string
  relevance: number
  actionable: boolean
  metadata?: Record<string, any>
}

export interface StreamingAnalyticsResponse {
  timestamp: string
  type: 'session_start' | 'session_end' | 'progress_update' | 'insight_generated' | 'error_occurred'
  sessionId?: string
  userId?: string
  metrics: {
    activeSessions: number
    averageProcessingTime: number
    successRate: number
    errorRate: number
    throughput: number
  }
  data?: any
}

export interface StartStreamingRequest {
  query: string
  userId?: string
  context?: {
    conversationId?: string
    previousQueries?: string[]
    userPreferences?: Record<string, any>
    businessContext?: Record<string, any>
  }
  options?: {
    enableInsights?: boolean
    enableTransparency?: boolean
    maxProcessingTime?: number
    confidenceThreshold?: number
  }
}

export interface QueryProcessingProgress {
  phase: 'parsing' | 'understanding' | 'generation' | 'execution' | 'insights' | 'complete' | 'error'
  progress: number
  currentStep: string
  confidence: number
  estimatedTimeRemaining?: Date
  details?: {
    entitiesFound?: number
    tablesAnalyzed?: number
    rulesApplied?: number
    optimizationsApplied?: number
  }
  metadata?: Record<string, any>
}

export interface StreamingInsight {
  id: string
  type: 'pattern' | 'anomaly' | 'trend' | 'recommendation' | 'warning'
  title: string
  description: string
  confidence: number
  data?: any
  timestamp: string
  relevance: number
  actionable: boolean
  actions?: InsightAction[]
}

export interface InsightAction {
  id: string
  title: string
  description: string
  type: 'query' | 'filter' | 'drill_down' | 'export' | 'alert'
  parameters?: Record<string, any>
}

export interface StreamingMetrics {
  sessionId: string
  startTime: string
  currentTime: string
  totalDuration: number
  phases: {
    [phase: string]: {
      startTime: string
      endTime?: string
      duration?: number
      confidence: number
      throughput: number
      errors: number
    }
  }
  overallProgress: number
  estimatedCompletion: string
  performance: {
    averageResponseTime: number
    peakMemoryUsage: number
    cpuUtilization: number
    networkLatency: number
  }
}

export interface StreamingAlert {
  id: string
  type: 'performance' | 'error' | 'timeout' | 'resource'
  severity: 'low' | 'medium' | 'high' | 'critical'
  message: string
  timestamp: string
  sessionId?: string
  metric?: string
  threshold?: number
  currentValue?: number
  resolved?: boolean
  actions?: string[]
}

export interface StreamingStatus {
  isStreaming: boolean
  activeSessions: number
  queueLength: number
  averageWaitTime: number
  systemHealth: 'healthy' | 'degraded' | 'unhealthy'
  lastUpdate: string
}

// Real-time processing types
export interface ProcessingPhase {
  name: string
  status: 'pending' | 'active' | 'complete' | 'error' | 'cancelled'
  progress: number
  confidence: number
  startTime?: string
  endTime?: string
  duration?: number
  details?: Record<string, any>
  subSteps?: ProcessingSubStep[]
}

export interface ProcessingSubStep {
  name: string
  status: 'pending' | 'running' | 'complete' | 'error'
  progress: number
  confidence?: number
  description?: string
}

export interface RealTimeUpdate {
  type: 'phase_start' | 'phase_progress' | 'phase_complete' | 'insight_generated' | 'error' | 'cancelled'
  sessionId: string
  timestamp: string
  data: any
  metadata?: Record<string, any>
}

// Analytics types
export interface StreamingAnalytics {
  timeRange: {
    start: string
    end: string
  }
  summary: {
    totalSessions: number
    activeSessions: number
    completedSessions: number
    cancelledSessions: number
    errorSessions: number
    averageProcessingTime: number
    successRate: number
  }
  trends: {
    sessionsOverTime: Array<{ timestamp: string; count: number }>
    processingTimeOverTime: Array<{ timestamp: string; avgTime: number }>
    successRateOverTime: Array<{ timestamp: string; rate: number }>
  }
  performance: {
    averageResponseTime: number
    p95ResponseTime: number
    p99ResponseTime: number
    throughput: number
    errorRate: number
  }
  insights: {
    totalGenerated: number
    byType: Record<string, number>
    averageRelevance: number
    actionablePct: number
  }
}

// Configuration types
export interface StreamingConfig {
  maxConcurrentSessions: number
  defaultTimeout: number
  enableInsights: boolean
  enableTransparency: boolean
  retryAttempts: number
  retryDelay: number
  bufferSize: number
  compressionEnabled: boolean
}

// Error types
export interface StreamingError {
  code: string
  message: string
  details?: Record<string, any>
  timestamp: string
  sessionId?: string
  phase?: string
  recoverable: boolean
  suggestions?: string[]
}

// WebSocket message types
export interface WebSocketMessage {
  type: 'streaming_update' | 'insight_generated' | 'session_status' | 'error' | 'ping'
  sessionId?: string
  data: any
  timestamp: string
}

export interface StreamingConnectionState {
  isConnected: boolean
  connectionId?: string
  lastPing?: string
  reconnectAttempts: number
  error?: string
}

// Dashboard types
export interface StreamingDashboardData {
  currentStatus: StreamingStatus
  recentSessions: StreamingSession[]
  liveMetrics: StreamingMetrics[]
  alerts: StreamingAlert[]
  analytics: StreamingAnalytics
  insights: StreamingInsight[]
}

// Export utility types
export type StreamingPhase = QueryProcessingProgress['phase']
export type StreamingSessionStatus = StreamingSession['status']
export type InsightType = StreamingInsight['type']
export type AlertSeverity = StreamingAlert['severity']
