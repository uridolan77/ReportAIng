// Common AI Component Types

import type { ReactNode } from 'react'
import type { AIFeatureFlags, AIError, AIPerformanceMetrics } from '@shared/types/ai'

// ============================================================================
// BASE COMPONENT INTERFACES
// ============================================================================

export interface BaseAIComponentProps {
  className?: string
  style?: React.CSSProperties
  loading?: boolean
  error?: AIError | string
  disabled?: boolean
  testId?: string
}

export interface AIFeatureWrapperProps extends BaseAIComponentProps {
  children: ReactNode
  feature: keyof AIFeatureFlags
  fallback?: ReactNode
  onFeatureUnavailable?: () => void
  gracefulDegradation?: boolean
}

export interface ConfidenceIndicatorProps extends BaseAIComponentProps {
  confidence: number
  threshold?: number
  showLabel?: boolean
  showPercentage?: boolean
  size?: 'small' | 'medium' | 'large'
  type?: 'circle' | 'bar' | 'badge'
  color?: 'auto' | 'green' | 'orange' | 'red'
}

export interface AIErrorBoundaryProps {
  children: ReactNode
  fallback?: ReactNode
  onError?: (error: Error, errorInfo: any) => void
  resetOnPropsChange?: boolean
  resetKeys?: any[]
}

export interface AILoadingSpinnerProps extends BaseAIComponentProps {
  size?: 'small' | 'medium' | 'large'
  message?: string
  showProgress?: boolean
  progress?: number
  cancelable?: boolean
  onCancel?: () => void
}

export interface AIStatusBadgeProps extends BaseAIComponentProps {
  status: 'healthy' | 'degraded' | 'unhealthy' | 'unknown'
  showText?: boolean
  showIcon?: boolean
  size?: 'small' | 'medium' | 'large'
  pulse?: boolean
}

// ============================================================================
// HOOK INTERFACES
// ============================================================================

export interface UseAIFeatureFlagsResult {
  flags: AIFeatureFlags
  isFeatureEnabled: (feature: keyof AIFeatureFlags) => boolean
  loading: boolean
  error?: string
  refreshFlags: () => Promise<void>
}

export interface UseAIHealthResult {
  isHealthy: boolean
  status: 'healthy' | 'degraded' | 'unhealthy' | 'checking'
  lastCheck?: string
  issues: string[]
  loading: boolean
  error?: string
  checkHealth: () => Promise<void>
}

export interface UseConfidenceThresholdResult {
  threshold: number
  setThreshold: (threshold: number) => void
  isAboveThreshold: (confidence: number) => boolean
  getConfidenceLevel: (confidence: number) => 'high' | 'medium' | 'low'
  getConfidenceColor: (confidence: number) => string
}

export interface UseAIPerformanceResult {
  metrics: AIPerformanceMetrics[]
  currentMetrics?: AIPerformanceMetrics
  isRecording: boolean
  startRecording: (componentName: string) => void
  stopRecording: () => void
  recordMetric: (metric: Partial<AIPerformanceMetrics>) => void
  clearMetrics: () => void
}

// ============================================================================
// UTILITY INTERFACES
// ============================================================================

export interface AIComponentConfig {
  enablePerformanceTracking: boolean
  enableErrorReporting: boolean
  defaultConfidenceThreshold: number
  gracefulDegradation: boolean
  retryAttempts: number
  retryDelay: number
  cacheTimeout: number
}

export interface AITheme {
  colors: {
    confidence: {
      high: string
      medium: string
      low: string
    }
    status: {
      healthy: string
      degraded: string
      unhealthy: string
      unknown: string
    }
    ai: {
      primary: string
      secondary: string
      accent: string
      background: string
      border: string
    }
  }
  spacing: {
    xs: string
    sm: string
    md: string
    lg: string
    xl: string
  }
  typography: {
    confidence: {
      fontSize: string
      fontWeight: string
    }
    status: {
      fontSize: string
      fontWeight: string
    }
  }
  animations: {
    duration: {
      fast: string
      normal: string
      slow: string
    }
    easing: {
      ease: string
      easeIn: string
      easeOut: string
      easeInOut: string
    }
  }
}

// ============================================================================
// EVENT INTERFACES
// ============================================================================

export interface AIComponentEvent {
  type: string
  component: string
  data?: any
  timestamp: string
  userId?: string
  sessionId?: string
}

export interface ConfidenceEvent extends AIComponentEvent {
  type: 'confidence_displayed' | 'confidence_clicked' | 'threshold_changed'
  confidence: number
  threshold?: number
}

export interface FeatureEvent extends AIComponentEvent {
  type: 'feature_enabled' | 'feature_disabled' | 'feature_unavailable'
  feature: keyof AIFeatureFlags
  fallbackUsed?: boolean
}

export interface PerformanceEvent extends AIComponentEvent {
  type: 'performance_recorded' | 'performance_threshold_exceeded'
  metrics: AIPerformanceMetrics
  threshold?: number
}

// ============================================================================
// CONTEXT INTERFACES
// ============================================================================

export interface AIContextValue {
  config: AIComponentConfig
  theme: AITheme
  featureFlags: AIFeatureFlags
  performance: {
    isRecording: boolean
    metrics: AIPerformanceMetrics[]
  }
  health: {
    status: 'healthy' | 'degraded' | 'unhealthy' | 'checking'
    lastCheck?: string
    issues: string[]
  }
  confidenceThreshold: number
  setConfidenceThreshold: (threshold: number) => void
  recordEvent: (event: AIComponentEvent) => void
}

// ============================================================================
// ACCESSIBILITY INTERFACES
// ============================================================================

export interface AIAccessibilityProps {
  'aria-label'?: string
  'aria-describedby'?: string
  'aria-expanded'?: boolean
  'aria-selected'?: boolean
  'aria-disabled'?: boolean
  role?: string
  tabIndex?: number
}

export interface ConfidenceAccessibility extends AIAccessibilityProps {
  confidenceLabel?: string
  confidenceDescription?: string
  thresholdLabel?: string
}

export interface StatusAccessibility extends AIAccessibilityProps {
  statusLabel?: string
  statusDescription?: string
  alertLevel?: 'polite' | 'assertive'
}

// ============================================================================
// RESPONSIVE INTERFACES
// ============================================================================

export interface ResponsiveConfig {
  breakpoints: {
    xs: number
    sm: number
    md: number
    lg: number
    xl: number
  }
  components: {
    [componentName: string]: {
      [breakpoint: string]: {
        visible?: boolean
        layout?: 'horizontal' | 'vertical' | 'grid'
        columns?: number
        spacing?: string
        fontSize?: string
      }
    }
  }
}

export interface UseResponsiveResult {
  breakpoint: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
  isMobile: boolean
  isTablet: boolean
  isDesktop: boolean
  getComponentConfig: (componentName: string) => any
}

// ============================================================================
// TESTING INTERFACES
// ============================================================================

export interface AIComponentTestProps {
  'data-testid'?: string
  'data-test-confidence'?: number
  'data-test-status'?: string
  'data-test-feature'?: string
}

export interface MockAIData {
  confidence?: number
  status?: 'healthy' | 'degraded' | 'unhealthy'
  features?: Partial<AIFeatureFlags>
  error?: AIError
  loading?: boolean
  delay?: number
}
