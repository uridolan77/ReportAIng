// AI Streaming Component Types

import type {
  QueryProcessingProgress,
  StreamingSession,
  StreamingInsight,
  ProcessingPhaseDetails
} from '@shared/types/ai'

// ============================================================================
// COMPONENT PROPS INTERFACES
// ============================================================================

export interface RealTimeProcessingViewerProps {
  sessionId: string
  showPhaseDetails?: boolean
  showInsights?: boolean
  onPhaseComplete?: (phase: string) => void
  onInsightGenerated?: (insight: StreamingInsight) => void
  onCancel?: () => void
  className?: string
}

export interface StreamingInsightViewerProps {
  insights: StreamingInsight[]
  realTime?: boolean
  showFilters?: boolean
  maxInsights?: number
  onInsightClick?: (insight: StreamingInsight) => void
  onInsightDismiss?: (insightId: string) => void
  className?: string
}

export interface QueryProcessingFlowProps {
  session: StreamingSession
  showMetrics?: boolean
  showTimeline?: boolean
  interactive?: boolean
  onPhaseClick?: (phase: string) => void
  onStepClick?: (step: string) => void
  className?: string
}

export interface StreamingAnalyticsEngineProps {
  sessionId?: string
  showRealTimeMetrics?: boolean
  showPerformanceChart?: boolean
  timeWindow?: number
  onMetricAlert?: (metric: string, value: number) => void
  className?: string
}

export interface EnhancedStreamingProgressProps {
  session?: StreamingSession
  showConfidenceEvolution?: boolean
  showCancellation?: boolean
  showEstimatedTime?: boolean
  compact?: boolean
  onCancel?: () => void
  onRetry?: () => void
  className?: string
}

// ============================================================================
// COMPONENT STATE INTERFACES
// ============================================================================

export interface ProcessingViewerState {
  currentPhase: string
  expandedPhases: Set<string>
  showDetails: boolean
  autoScroll: boolean
  paused: boolean
}

export interface InsightViewerState {
  filteredInsights: StreamingInsight[]
  selectedTypes: Set<string>
  sortBy: 'timestamp' | 'relevance' | 'confidence'
  sortOrder: 'asc' | 'desc'
  searchQuery: string
}

export interface ProcessingFlowState {
  selectedPhase?: string
  selectedStep?: string
  showMetrics: boolean
  animationSpeed: number
  zoomLevel: number
}

export interface AnalyticsEngineState {
  metrics: StreamingMetrics
  alerts: StreamingAlert[]
  isRecording: boolean
  timeWindow: number
  selectedMetric?: string
}

// ============================================================================
// DATA INTERFACES
// ============================================================================

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
  metric?: string
  threshold?: number
  currentValue?: number
  resolved?: boolean
}

export interface PhaseVisualization {
  phase: string
  status: 'pending' | 'active' | 'complete' | 'error' | 'cancelled'
  progress: number
  confidence: number
  startTime: string
  endTime?: string
  duration?: number
  details: ProcessingPhaseDetails
  subSteps: {
    name: string
    status: 'pending' | 'running' | 'complete' | 'error'
    progress: number
    confidence?: number
  }[]
}

export interface InsightVisualization {
  insight: StreamingInsight
  displayType: 'card' | 'notification' | 'inline' | 'popup'
  priority: number
  animation: 'fade' | 'slide' | 'bounce' | 'none'
  autoHide?: number
  interactive: boolean
}

// ============================================================================
// HOOK INTERFACES
// ============================================================================

export interface UseStreamingSessionResult {
  session?: StreamingSession
  isActive: boolean
  currentPhase: string
  progress: QueryProcessingProgress
  insights: StreamingInsight[]
  metrics: StreamingMetrics
  loading: boolean
  error?: string
  startSession: (query: string) => Promise<void>
  cancelSession: () => void
  pauseSession: () => void
  resumeSession: () => void
}

export interface UseRealTimeInsightsResult {
  insights: StreamingInsight[]
  newInsightCount: number
  loading: boolean
  error?: string
  addInsight: (insight: StreamingInsight) => void
  dismissInsight: (id: string) => void
  clearInsights: () => void
  filterInsights: (filter: InsightFilter) => void
}

export interface UseStreamingMetricsResult {
  metrics: StreamingMetrics
  alerts: StreamingAlert[]
  isRecording: boolean
  loading: boolean
  error?: string
  startRecording: () => void
  stopRecording: () => void
  resetMetrics: () => void
  exportMetrics: () => void
}

// ============================================================================
// FILTER & SEARCH INTERFACES
// ============================================================================

export interface InsightFilter {
  types?: StreamingInsight['type'][]
  minConfidence?: number
  maxAge?: number
  searchQuery?: string
  relevanceThreshold?: number
}

export interface MetricsFilter {
  timeRange: {
    start: string
    end: string
  }
  phases?: string[]
  metrics?: string[]
  aggregation?: 'avg' | 'min' | 'max' | 'sum'
}

export interface ProcessingFilter {
  phases?: string[]
  minConfidence?: number
  showErrors?: boolean
  showCancelled?: boolean
  timeRange?: {
    start: string
    end: string
  }
}

// ============================================================================
// EVENT INTERFACES
// ============================================================================

export interface StreamingEvent {
  type: 'phase_start' | 'phase_complete' | 'insight_generated' | 'error' | 'cancelled'
  sessionId: string
  data: any
  timestamp: string
  metadata?: Record<string, any>
}

export interface PhaseEvent {
  phase: string
  action: 'start' | 'complete' | 'error' | 'cancel'
  progress: number
  confidence: number
  details?: ProcessingPhaseDetails
  timestamp: string
}

export interface InsightEvent {
  insight: StreamingInsight
  action: 'generated' | 'clicked' | 'dismissed' | 'expired'
  timestamp: string
  metadata?: Record<string, any>
}

// ============================================================================
// CONFIGURATION INTERFACES
// ============================================================================

export interface StreamingConfig {
  autoStart: boolean
  showRealTimeUpdates: boolean
  maxConcurrentSessions: number
  defaultTimeWindow: number
  insightRetentionTime: number
  enableMetricsRecording: boolean
  alertThresholds: {
    responseTime: number
    errorRate: number
    memoryUsage: number
    cpuUsage: number
  }
}

export interface VisualizationConfig {
  animationSpeed: number
  updateInterval: number
  maxDataPoints: number
  colorScheme: {
    pending: string
    active: string
    complete: string
    error: string
    cancelled: string
  }
  layout: {
    flowDirection: 'horizontal' | 'vertical'
    compactMode: boolean
    showLabels: boolean
    showProgress: boolean
  }
}
