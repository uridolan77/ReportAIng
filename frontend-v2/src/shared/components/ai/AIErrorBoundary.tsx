import React, { Component, ErrorInfo, ReactNode } from 'react'
import { Result, Button, Card, Typography, Space, Alert, Collapse, Tag } from 'antd'
import {
  ExclamationCircleOutlined,
  ReloadOutlined,
  BugOutlined,
  InfoCircleOutlined,
  WarningOutlined
} from '@ant-design/icons'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

// ============================================================================
// ERROR BOUNDARY STATE AND PROPS
// ============================================================================

interface AIErrorBoundaryState {
  hasError: boolean
  error: Error | null
  errorInfo: ErrorInfo | null
  errorId: string
  retryCount: number
  lastErrorTime: number
}

interface AIErrorBoundaryProps {
  children: ReactNode
  componentName?: string
  fallback?: ReactNode
  onError?: (error: Error, errorInfo: ErrorInfo) => void
  maxRetries?: number
  resetOnPropsChange?: boolean
  showErrorDetails?: boolean
  enableReporting?: boolean
}

// ============================================================================
// AI ERROR BOUNDARY COMPONENT
// ============================================================================

export class AIErrorBoundary extends Component<AIErrorBoundaryProps, AIErrorBoundaryState> {
  private resetTimeoutId: number | null = null

  constructor(props: AIErrorBoundaryProps) {
    super(props)
    
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
      errorId: '',
      retryCount: 0,
      lastErrorTime: 0
    }
  }

  static getDerivedStateFromError(error: Error): Partial<AIErrorBoundaryState> {
    const errorId = `ai-error-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
    
    return {
      hasError: true,
      error,
      errorId,
      lastErrorTime: Date.now()
    }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    const { componentName = 'AIComponent', onError, enableReporting = true } = this.props
    
    this.setState({ errorInfo })
    
    // Call custom error handler
    onError?.(error, errorInfo)
    
    // Log error for debugging
    console.error(`AI Error Boundary caught an error in ${componentName}:`, {
      error,
      errorInfo,
      componentStack: errorInfo.componentStack,
      errorBoundary: this.constructor.name
    })
    
    // Report error to monitoring service (if enabled)
    if (enableReporting) {
      this.reportError(error, errorInfo)
    }
  }

  componentDidUpdate(prevProps: AIErrorBoundaryProps) {
    const { resetOnPropsChange = true } = this.props
    const { hasError } = this.state
    
    // Reset error state if props changed and resetOnPropsChange is enabled
    if (hasError && resetOnPropsChange && prevProps.children !== this.props.children) {
      this.resetErrorBoundary()
    }
  }

  componentWillUnmount() {
    if (this.resetTimeoutId) {
      clearTimeout(this.resetTimeoutId)
    }
  }

  private reportError = async (error: Error, errorInfo: ErrorInfo) => {
    try {
      // In a real application, this would send to an error reporting service
      const errorReport = {
        message: error.message,
        stack: error.stack,
        componentStack: errorInfo.componentStack,
        componentName: this.props.componentName,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        url: window.location.href,
        errorId: this.state.errorId
      }
      
      // Mock API call to error reporting service
      console.log('Error reported:', errorReport)
      
      // Could integrate with services like Sentry, LogRocket, etc.
      // await fetch('/api/errors', {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify(errorReport)
      // })
    } catch (reportingError) {
      console.error('Failed to report error:', reportingError)
    }
  }

  private resetErrorBoundary = () => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
      errorId: '',
      retryCount: 0,
      lastErrorTime: 0
    })
  }

  private handleRetry = () => {
    const { maxRetries = 3 } = this.props
    const { retryCount } = this.state
    
    if (retryCount < maxRetries) {
      this.setState(prevState => ({
        hasError: false,
        error: null,
        errorInfo: null,
        retryCount: prevState.retryCount + 1
      }))
      
      // Auto-reset after a delay if error persists
      this.resetTimeoutId = window.setTimeout(() => {
        if (this.state.hasError) {
          this.resetErrorBoundary()
        }
      }, 5000)
    }
  }

  private getErrorSeverity = (error: Error): 'low' | 'medium' | 'high' | 'critical' => {
    const message = error.message.toLowerCase()
    
    if (message.includes('network') || message.includes('fetch')) {
      return 'medium'
    }
    if (message.includes('permission') || message.includes('unauthorized')) {
      return 'high'
    }
    if (message.includes('critical') || message.includes('fatal')) {
      return 'critical'
    }
    
    return 'low'
  }

  private getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return '#ff4d4f'
      case 'high': return '#faad14'
      case 'medium': return '#1890ff'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  private renderErrorDetails = () => {
    const { error, errorInfo, errorId } = this.state
    const { showErrorDetails = false } = this.props
    
    if (!showErrorDetails || !error) return null
    
    const severity = this.getErrorSeverity(error)
    
    return (
      <Card size="small" style={{ marginTop: 16 }}>
        <Collapse size="small">
          <Panel 
            header={
              <Space>
                <BugOutlined />
                <span>Error Details</span>
                <Tag color={this.getSeverityColor(severity)}>
                  {severity.toUpperCase()}
                </Tag>
              </Space>
            } 
            key="error-details"
          >
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Error ID: </Text>
                <Text code>{errorId}</Text>
              </div>
              
              <div>
                <Text strong>Message: </Text>
                <Text>{error.message}</Text>
              </div>
              
              <div>
                <Text strong>Component: </Text>
                <Text>{this.props.componentName || 'Unknown'}</Text>
              </div>
              
              <div>
                <Text strong>Timestamp: </Text>
                <Text>{new Date(this.state.lastErrorTime).toLocaleString()}</Text>
              </div>
              
              {error.stack && (
                <div>
                  <Text strong>Stack Trace: </Text>
                  <pre style={{ 
                    fontSize: '11px', 
                    background: '#f5f5f5', 
                    padding: '8px', 
                    borderRadius: '4px',
                    overflow: 'auto',
                    maxHeight: '200px'
                  }}>
                    {error.stack}
                  </pre>
                </div>
              )}
              
              {errorInfo?.componentStack && (
                <div>
                  <Text strong>Component Stack: </Text>
                  <pre style={{ 
                    fontSize: '11px', 
                    background: '#f5f5f5', 
                    padding: '8px', 
                    borderRadius: '4px',
                    overflow: 'auto',
                    maxHeight: '200px'
                  }}>
                    {errorInfo.componentStack}
                  </pre>
                </div>
              )}
            </Space>
          </Panel>
        </Collapse>
      </Card>
    )
  }

  private renderFallbackUI = () => {
    const { fallback, componentName = 'AI Component', maxRetries = 3 } = this.props
    const { error, retryCount } = this.state
    
    if (fallback) {
      return fallback
    }
    
    const canRetry = retryCount < maxRetries
    const severity = error ? this.getErrorSeverity(error) : 'medium'
    
    return (
      <div style={{ padding: '24px' }}>
        <Result
          status="error"
          icon={<ExclamationCircleOutlined style={{ color: this.getSeverityColor(severity) }} />}
          title={`${componentName} Error`}
          subTitle={error?.message || 'An unexpected error occurred in the AI component.'}
          extra={
            <Space>
              {canRetry && (
                <Button 
                  type="primary" 
                  icon={<ReloadOutlined />}
                  onClick={this.handleRetry}
                >
                  Retry ({maxRetries - retryCount} attempts left)
                </Button>
              )}
              <Button onClick={this.resetErrorBoundary}>
                Reset Component
              </Button>
            </Space>
          }
        />
        
        {severity === 'critical' && (
          <Alert
            message="Critical Error"
            description="This error may affect other AI components. Please refresh the page if issues persist."
            type="error"
            showIcon
            style={{ marginTop: 16 }}
          />
        )}
        
        {retryCount >= maxRetries && (
          <Alert
            message="Maximum Retries Exceeded"
            description="The component has failed multiple times. Please check your connection and try refreshing the page."
            type="warning"
            showIcon
            style={{ marginTop: 16 }}
          />
        )}
        
        {this.renderErrorDetails()}
      </div>
    )
  }

  render() {
    const { hasError } = this.state
    const { children } = this.props
    
    if (hasError) {
      return this.renderFallbackUI()
    }
    
    return children
  }
}

// ============================================================================
// FUNCTIONAL ERROR BOUNDARY WRAPPER
// ============================================================================

interface AIErrorBoundaryWrapperProps extends Omit<AIErrorBoundaryProps, 'children'> {
  children: ReactNode
}

export const AIErrorBoundaryWrapper: React.FC<AIErrorBoundaryWrapperProps> = ({
  children,
  ...props
}) => {
  return (
    <AIErrorBoundary {...props}>
      {children}
    </AIErrorBoundary>
  )
}

// ============================================================================
// HOC FOR WRAPPING COMPONENTS
// ============================================================================

export function withAIErrorBoundary<P extends object>(
  Component: React.ComponentType<P>,
  errorBoundaryProps?: Omit<AIErrorBoundaryProps, 'children'>
) {
  const WrappedComponent = (props: P) => (
    <AIErrorBoundary 
      componentName={Component.displayName || Component.name}
      {...errorBoundaryProps}
    >
      <Component {...props} />
    </AIErrorBoundary>
  )
  
  WrappedComponent.displayName = `withAIErrorBoundary(${Component.displayName || Component.name})`
  
  return WrappedComponent
}

export default AIErrorBoundary
