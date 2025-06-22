// AI Transparency Component Types

import type {
  PromptConstructionTrace,
  ConfidenceAnalysis,
  AlternativeOption,
  OptimizationSuggestion,
  AIDecisionExplanation,
  ModelPerformanceMetrics
} from '@shared/types/ai'

// ============================================================================
// COMPONENT PROPS INTERFACES
// ============================================================================

export interface AITransparencyPanelProps {
  traceId: string
  showDetailedMetrics?: boolean
  compact?: boolean
  onOptimizationSuggestion?: (suggestion: OptimizationSuggestion) => void
  onAlternativeSelect?: (alternative: AlternativeOption) => void
  className?: string
}

export interface PromptConstructionViewerProps {
  trace: PromptConstructionTrace
  interactive?: boolean
  showTimeline?: boolean
  onStepClick?: (stepIndex: number) => void
  className?: string
}

export interface ConfidenceBreakdownChartProps {
  analysis: ConfidenceAnalysis
  showFactors?: boolean
  interactive?: boolean
  onFactorClick?: (factor: string) => void
  chartType?: 'bar' | 'pie' | 'radar'
  className?: string
}

export interface AIDecisionExplainerProps {
  explanation: AIDecisionExplanation
  showAlternatives?: boolean
  showRecommendations?: boolean
  onAlternativeSelect?: (alternative: AlternativeOption) => void
  onRecommendationApply?: (recommendation: OptimizationSuggestion) => void
  className?: string
}

export interface ModelPerformanceMetricsProps {
  metrics: ModelPerformanceMetrics
  showTrends?: boolean
  showBenchmarks?: boolean
  timeRange?: 'hour' | 'day' | 'week' | 'month'
  onMetricClick?: (metric: string) => void
  className?: string
}

// ============================================================================
// COMPONENT STATE INTERFACES
// ============================================================================

export interface TransparencyPanelState {
  activeTab: string
  showAdvanced: boolean
  selectedStep?: number
  selectedFactor?: string
  loading: boolean
  error?: string
}

export interface PromptViewerState {
  expandedSteps: Set<number>
  selectedStep?: number
  showAlternatives: boolean
  animationEnabled: boolean
}

export interface ConfidenceChartState {
  selectedFactor?: string
  chartType: 'bar' | 'pie' | 'radar'
  showTooltip: boolean
  hoveredElement?: string
}

// ============================================================================
// DATA INTERFACES
// ============================================================================

export interface TransparencyData {
  trace: PromptConstructionTrace
  analysis: ConfidenceAnalysis
  explanation: AIDecisionExplanation
  alternatives: AlternativeOption[]
  recommendations: OptimizationSuggestion[]
  metadata: {
    timestamp: string
    version: string
    source: string
  }
}

export interface PromptStepVisualization {
  step: number
  title: string
  description: string
  confidence: number
  status: 'pending' | 'active' | 'complete' | 'error'
  duration?: number
  details?: Record<string, any>
}

export interface ConfidenceVisualization {
  factor: string
  value: number
  color: string
  description: string
  impact: 'high' | 'medium' | 'low'
  trend?: 'up' | 'down' | 'stable'
}

// ============================================================================
// HOOK INTERFACES
// ============================================================================

export interface UseAITransparencyResult {
  transparencyData?: TransparencyData
  loading: boolean
  error?: string
  refreshTrace: () => Promise<void>
  exportTrace: () => void
}

export interface UsePromptConstructionResult {
  trace?: PromptConstructionTrace
  currentStep?: number
  isComplete: boolean
  loading: boolean
  error?: string
  nextStep: () => void
  previousStep: () => void
  jumpToStep: (step: number) => void
}

export interface UseConfidenceAnalysisResult {
  analysis?: ConfidenceAnalysis
  selectedFactor?: string
  loading: boolean
  error?: string
  selectFactor: (factor: string) => void
  clearSelection: () => void
  refreshAnalysis: () => Promise<void>
}

// ============================================================================
// EVENT INTERFACES
// ============================================================================

export interface TransparencyEvent {
  type: 'step_selected' | 'factor_clicked' | 'alternative_selected' | 'recommendation_applied'
  data: any
  timestamp: string
  source: string
}

export interface PromptStepEvent {
  stepIndex: number
  stepName: string
  action: 'expand' | 'collapse' | 'select' | 'hover'
  metadata?: Record<string, any>
}

export interface ConfidenceFactorEvent {
  factor: string
  value: number
  action: 'click' | 'hover' | 'select'
  metadata?: Record<string, any>
}

// ============================================================================
// CONFIGURATION INTERFACES
// ============================================================================

export interface TransparencyConfig {
  defaultTab: string
  showAdvancedByDefault: boolean
  enableAnimations: boolean
  autoRefreshInterval?: number
  maxHistoryItems: number
  confidenceThreshold: number
}

export interface VisualizationConfig {
  colors: {
    high: string
    medium: string
    low: string
    error: string
    success: string
  }
  animations: {
    duration: number
    easing: string
    enabled: boolean
  }
  layout: {
    compact: boolean
    responsive: boolean
    maxWidth?: number
  }
}
