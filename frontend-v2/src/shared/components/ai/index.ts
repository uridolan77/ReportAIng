// ============================================================================
// AI COMPONENTS LIBRARY - COMPREHENSIVE INDEX
// ============================================================================

// Core Integration and Infrastructure
export { default as AIIntegrationProvider, useAIIntegration, useAIFeatureGate, useAIPerformanceMonitor, useAIErrorHandler } from './AIIntegrationProvider'
export { default as AIErrorBoundary, AIErrorBoundaryWrapper, withAIErrorBoundary } from './AIErrorBoundary'
export { default as AIAccessibilityProvider, useAIAccessibility, useAIFocusManagement, useAIKeyboardNavigation, useAIAnnouncements, withAIAccessibility } from './AIAccessibilityProvider'

// Performance Optimization
export {
  default as AIPerformanceOptimizer,
  createLazyAIComponent,
  VirtualScroll,
  memoizeAIComponent,
  useAIDebounce,
  useAIThrottle,
  useAIIntersectionObserver,
  useAIPerformanceMetrics,
  withAIPerformanceOptimization,
  LazyAITransparencyPanel,
  LazyLLMProviderManager,
  LazyAISchemaExplorer,
  LazyQueryOptimizationEngine,
  LazyAutomatedInsightsGenerator,
  LazyPredictiveAnalyticsPanel
} from './AIPerformanceOptimizer'

// Common Components and Utilities
export * from './common'

// AI Transparency and Explainability
export * from './transparency'

// AI Streaming and Real-time Processing
export * from './streaming'

// Business Intelligence and Context
export * from './intelligence'

// AI Management and Administration
export * from './management'

// Advanced AI Insights and Analytics
export * from './insights'

// ============================================================================
// TYPE EXPORTS
// ============================================================================

// Re-export all AI-related types
export type * from '@shared/types/transparency'
export type * from '@shared/types/intelligentAgents'

// ============================================================================
// UTILITY EXPORTS
// ============================================================================

export {
  generateAccessibleId,
  createAriaAttributes
} from './AIAccessibilityProvider'

// ============================================================================
// CONSTANTS AND CONFIGURATION
// ============================================================================

export const AI_COMPONENT_DEFAULTS = {
  CONFIDENCE_THRESHOLD: 0.7,
  DEBOUNCE_DELAY: 300,
  THROTTLE_DELAY: 1000,
  VIRTUAL_SCROLL_OVERSCAN: 5,
  PERFORMANCE_MONITOR_INTERVAL: 30000,
  ERROR_RETRY_ATTEMPTS: 3,
  ACCESSIBILITY_ANNOUNCEMENT_DELAY: 1000
} as const

export const AI_FEATURE_FLAGS = {
  TRANSPARENCY: 'transparency',
  STREAMING: 'streaming',
  INTELLIGENCE: 'intelligence',
  MANAGEMENT: 'management',
  INSIGHTS: 'insights',
  PREDICTIVE: 'predictive'
} as const

export const AI_COMPONENT_NAMES = {
  PROCESSFLOW_SESSION_VIEWER: 'ProcessFlowSessionViewer',
  PROCESSFLOW_DASHBOARD: 'ProcessFlowDashboard',
  PROCESSFLOW_ANALYTICS: 'ProcessFlowAnalytics',
  TOKEN_USAGE_ANALYZER: 'TokenUsageAnalyzer',
  PERFORMANCE_METRICS_VIEWER: 'PerformanceMetricsViewer',
  SEMANTIC_ANALYSIS_PANEL: 'SemanticAnalysisPanel',
  CHAT_INTERFACE: 'ChatInterface',
  LLM_PROVIDER_MANAGER: 'LLMProviderManager',
  MODEL_PERFORMANCE_ANALYTICS: 'ModelPerformanceAnalytics',
  COST_OPTIMIZATION_PANEL: 'CostOptimizationPanel',
  AI_CONFIGURATION_MANAGER: 'AIConfigurationManager',
  AI_SCHEMA_EXPLORER: 'AISchemaExplorer',
  QUERY_OPTIMIZATION_ENGINE: 'QueryOptimizationEngine',
  AUTOMATED_INSIGHTS_GENERATOR: 'AutomatedInsightsGenerator',
  PREDICTIVE_ANALYTICS_PANEL: 'PredictiveAnalyticsPanel',
  BUSINESS_CONTEXT_VISUALIZER: 'BusinessContextVisualizer'
} as const

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Initialize AI components with default configuration
 */
export const initializeAIComponents = (config?: {
  enableErrorNotifications?: boolean
  enablePerformanceMonitoring?: boolean
  enableKeyboardShortcuts?: boolean
  enableScreenReaderAnnouncements?: boolean
  globalConfidenceThreshold?: number
}) => {
  const defaultConfig = {
    enableErrorNotifications: true,
    enablePerformanceMonitoring: true,
    enableKeyboardShortcuts: true,
    enableScreenReaderAnnouncements: true,
    globalConfidenceThreshold: AI_COMPONENT_DEFAULTS.CONFIDENCE_THRESHOLD,
    ...config
  }

  return defaultConfig
}

/**
 * Validate AI component props
 */
export const validateAIComponentProps = (props: any, requiredProps: string[]) => {
  const missingProps = requiredProps.filter(prop => !(prop in props))

  if (missingProps.length > 0) {
    console.warn(`Missing required props for AI component: ${missingProps.join(', ')}`)
    return false
  }

  return true
}

/**
 * Format confidence score for display
 */
export const formatConfidence = (confidence: number, format: 'percentage' | 'decimal' | 'fraction' = 'percentage') => {
  switch (format) {
    case 'percentage':
      return `${Math.round(confidence * 100)}%`
    case 'decimal':
      return confidence.toFixed(2)
    case 'fraction':
      return `${Math.round(confidence * 100)}/100`
    default:
      return confidence.toString()
  }
}

/**
 * Get AI component theme based on confidence level
 */
export const getConfidenceTheme = (confidence: number) => {
  if (confidence >= 0.9) return { color: '#52c41a', level: 'high' }
  if (confidence >= 0.7) return { color: '#faad14', level: 'medium' }
  if (confidence >= 0.5) return { color: '#ff7875', level: 'low' }
  return { color: '#d9d9d9', level: 'very-low' }
}

/**
 * Create AI component test ID
 */
export const createAITestId = (componentName: string, suffix?: string) => {
  const baseId = componentName.toLowerCase().replace(/([A-Z])/g, '-$1').replace(/^-/, '')
  return suffix ? `${baseId}-${suffix}` : baseId
}

// ============================================================================
// VERSION AND METADATA
// ============================================================================

export const AI_COMPONENTS_VERSION = '1.0.0'
export const AI_COMPONENTS_BUILD_DATE = new Date().toISOString()

export const AI_COMPONENTS_METADATA = {
  version: AI_COMPONENTS_VERSION,
  buildDate: AI_COMPONENTS_BUILD_DATE,
  features: Object.values(AI_FEATURE_FLAGS),
  components: Object.values(AI_COMPONENT_NAMES),
  totalComponents: Object.keys(AI_COMPONENT_NAMES).length,
  description: 'Comprehensive AI components library for intelligent database interfaces',
  author: 'ReportAIng Development Team',
  license: 'MIT'
} as const
