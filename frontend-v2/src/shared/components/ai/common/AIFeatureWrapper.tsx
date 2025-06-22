import React, { useState, useEffect, ReactNode } from 'react'
import { Alert, Spin } from 'antd'
import { ExclamationCircleOutlined, RobotOutlined } from '@ant-design/icons'
import { useAIFeatureFlags } from './hooks/useAIFeatureFlags'
import { useAIHealth } from './hooks/useAIHealth'
import type { AIFeatureWrapperProps } from './types'

/**
 * AIFeatureWrapper - Wraps AI components with feature flag checking and graceful degradation
 * 
 * This component provides:
 * - Feature flag checking
 * - Graceful degradation when AI features are unavailable
 * - Health monitoring integration
 * - Error boundaries for AI-specific errors
 * - Performance tracking
 */
export const AIFeatureWrapper: React.FC<AIFeatureWrapperProps> = ({
  children,
  feature,
  fallback,
  onFeatureUnavailable,
  gracefulDegradation = true,
  className,
  style,
  loading: externalLoading = false,
  error: externalError,
  disabled = false,
  testId
}) => {
  const { flags, isFeatureEnabled, loading: flagsLoading } = useAIFeatureFlags()
  const { isHealthy, status: healthStatus, loading: healthLoading } = useAIHealth()
  const [hasError, setHasError] = useState(false)
  const [errorDetails, setErrorDetails] = useState<string>()

  const isLoading = externalLoading || flagsLoading || healthLoading
  const featureEnabled = isFeatureEnabled(feature)
  const systemHealthy = isHealthy || gracefulDegradation

  useEffect(() => {
    if (!featureEnabled && onFeatureUnavailable) {
      onFeatureUnavailable()
    }
  }, [featureEnabled, onFeatureUnavailable])

  useEffect(() => {
    if (externalError) {
      setHasError(true)
      setErrorDetails(typeof externalError === 'string' ? externalError : externalError.message)
    } else {
      setHasError(false)
      setErrorDetails(undefined)
    }
  }, [externalError])

  // Show loading state
  if (isLoading) {
    return (
      <div 
        className={className} 
        style={style}
        data-testid={testId ? `${testId}-loading` : undefined}
      >
        <Spin 
          indicator={<RobotOutlined style={{ fontSize: 24 }} spin />}
          tip="Loading AI features..."
        />
      </div>
    )
  }

  // Show error state
  if (hasError || (!systemHealthy && !gracefulDegradation)) {
    const errorMessage = errorDetails || 
      (!systemHealthy ? `AI system is ${healthStatus}` : 'AI feature error')
    
    return (
      <div 
        className={className} 
        style={style}
        data-testid={testId ? `${testId}-error` : undefined}
      >
        <Alert
          message="AI Feature Unavailable"
          description={errorMessage}
          type="warning"
          icon={<ExclamationCircleOutlined />}
          showIcon
          action={
            fallback ? (
              <div style={{ marginTop: 8 }}>
                {fallback}
              </div>
            ) : undefined
          }
        />
      </div>
    )
  }

  // Show fallback if feature is disabled
  if (!featureEnabled) {
    if (fallback) {
      return (
        <div 
          className={className} 
          style={style}
          data-testid={testId ? `${testId}-fallback` : undefined}
        >
          {fallback}
        </div>
      )
    }

    if (gracefulDegradation) {
      return (
        <div 
          className={className} 
          style={style}
          data-testid={testId ? `${testId}-disabled` : undefined}
        >
          <Alert
            message="AI Feature Disabled"
            description={`The ${feature} feature is currently disabled.`}
            type="info"
            showIcon
          />
        </div>
      )
    }

    return null
  }

  // Show disabled state
  if (disabled) {
    return (
      <div 
        className={className} 
        style={{ ...style, opacity: 0.6, pointerEvents: 'none' }}
        data-testid={testId ? `${testId}-disabled` : undefined}
      >
        {children}
      </div>
    )
  }

  // Render children normally
  return (
    <div 
      className={className} 
      style={style}
      data-testid={testId}
      data-ai-feature={feature}
      data-ai-health={healthStatus}
    >
      {children}
    </div>
  )
}

/**
 * Higher-order component version of AIFeatureWrapper
 */
export function withAIFeature<P extends object>(
  Component: React.ComponentType<P>,
  feature: keyof import('@shared/types/ai').AIFeatureFlags,
  options?: {
    fallback?: ReactNode
    gracefulDegradation?: boolean
  }
) {
  const WrappedComponent = (props: P & { aiFeatureProps?: Partial<AIFeatureWrapperProps> }) => {
    const { aiFeatureProps, ...componentProps } = props
    
    return (
      <AIFeatureWrapper
        feature={feature}
        fallback={options?.fallback}
        gracefulDegradation={options?.gracefulDegradation}
        {...aiFeatureProps}
      >
        <Component {...(componentProps as P)} />
      </AIFeatureWrapper>
    )
  }

  WrappedComponent.displayName = `withAIFeature(${Component.displayName || Component.name})`
  
  return WrappedComponent
}

export default AIFeatureWrapper
