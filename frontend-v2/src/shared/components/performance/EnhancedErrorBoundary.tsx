/**
 * Enhanced Error Boundary with Recovery Mechanisms
 * 
 * Provides advanced error handling with automatic retry, different error levels,
 * and intelligent recovery strategies for BI dashboard components.
 */

import React, { Component, ErrorInfo, ReactNode } from 'react'
import { Result, Button, Typography, Card, Space, Alert, Collapse } from 'antd'
import { ReloadOutlined, BugOutlined, WarningOutlined, InfoCircleOutlined } from '@ant-design/icons'

const { Paragraph, Text } = Typography
const { Panel } = Collapse

interface Props {
  children: ReactNode
  fallback?: ReactNode
  onError?: (error: Error, errorInfo: ErrorInfo) => void
  /** Error boundary level for different recovery strategies */
  level?: 'page' | 'component' | 'widget'
  /** Custom error messages */
  errorMessages?: {
    title?: string
    subtitle?: string
    description?: string
  }
  /** Enable automatic retry */
  autoRetry?: boolean
  /** Auto retry delay in milliseconds */
  autoRetryDelay?: number
  /** Maximum auto retry attempts */
  maxRetryAttempts?: number
  /** Show detailed error info */
  showDetails?: boolean
}

interface State {
  hasError: boolean
  error?: Error
  errorInfo?: ErrorInfo
  retryCount: number
  isRetrying: boolean
  lastErrorTime: number
}

export class EnhancedErrorBoundary extends Component<Props, State> {
  private retryTimeoutId?: NodeJS.Timeout

  constructor(props: Props) {
    super(props)
    this.state = { 
      hasError: false, 
      retryCount: 0, 
      isRetrying: false,
      lastErrorTime: 0,
    }
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return { 
      hasError: true, 
      error,
      lastErrorTime: Date.now(),
    }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    this.setState({ errorInfo })
    this.props.onError?.(error, errorInfo)
    
    // Log error to console in development
    if (import.meta.env.DEV) {
      console.error('EnhancedErrorBoundary caught an error:', error, errorInfo)
    }

    // Log error to external service in production
    if (import.meta.env.PROD) {
      this.logErrorToService(error, errorInfo)
    }

    // Auto retry if enabled
    if (this.props.autoRetry && this.state.retryCount < (this.props.maxRetryAttempts || 3)) {
      this.scheduleAutoRetry()
    }
  }

  componentWillUnmount() {
    if (this.retryTimeoutId) {
      clearTimeout(this.retryTimeoutId)
    }
  }

  logErrorToService = (error: Error, errorInfo: ErrorInfo) => {
    // In a real application, you would send this to your error tracking service
    // e.g., Sentry, LogRocket, Bugsnag, etc.
    console.log('Logging error to service:', {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      url: window.location.href,
    })
  }

  scheduleAutoRetry = () => {
    this.setState({ isRetrying: true })
    
    this.retryTimeoutId = setTimeout(() => {
      this.handleRetry(true)
    }, this.props.autoRetryDelay || 3000)
  }

  handleRetry = (isAutoRetry = false) => {
    if (this.retryTimeoutId) {
      clearTimeout(this.retryTimeoutId)
    }

    this.setState(prevState => ({ 
      hasError: false, 
      error: undefined, 
      errorInfo: undefined,
      retryCount: isAutoRetry ? prevState.retryCount + 1 : 0,
      isRetrying: false,
    }))
  }

  getErrorSeverity = (): 'low' | 'medium' | 'high' => {
    const { error } = this.state
    if (!error) return 'low'

    // Classify error severity based on error type and message
    if (error.name === 'ChunkLoadError' || error.message.includes('Loading chunk')) {
      return 'medium' // Network/loading issues
    }
    
    if (error.name === 'TypeError' && error.message.includes('Cannot read property')) {
      return 'high' // Data structure issues
    }

    if (error.message.includes('Network Error') || error.message.includes('fetch')) {
      return 'medium' // Network issues
    }

    return 'high' // Unknown errors are treated as high severity
  }

  getRecoveryActions = () => {
    const severity = this.getErrorSeverity()
    const { level = 'component' } = this.props

    const actions = []

    // Always show retry button
    actions.push(
      <Button 
        type="primary" 
        icon={<ReloadOutlined />} 
        onClick={() => this.handleRetry()} 
        key="retry"
        loading={this.state.isRetrying}
      >
        {this.state.isRetrying ? 'Retrying...' : 'Try Again'}
      </Button>
    )

    // Show refresh page for high severity or page-level errors
    if (severity === 'high' || level === 'page') {
      actions.push(
        <Button onClick={() => window.location.reload()} key="refresh">
          Refresh Page
        </Button>
      )
    }

    // Show go back for page-level errors
    if (level === 'page') {
      actions.push(
        <Button onClick={() => window.history.back()} key="back">
          Go Back
        </Button>
      )
    }

    return actions
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback
      }

      const severity = this.getErrorSeverity()
      const { level = 'component', errorMessages = {}, showDetails = import.meta.env.DEV } = this.props
      
      const defaultMessages = {
        title: level === 'page' ? 'Page Error' : 'Component Error',
        subtitle: severity === 'high' 
          ? 'A critical error occurred that prevented this from working properly.'
          : 'A temporary error occurred. This usually resolves itself.',
        description: severity === 'medium'
          ? 'This might be a temporary network issue. Please try again.'
          : 'If this problem persists, please contact support.',
      }

      const messages = { ...defaultMessages, ...errorMessages }

      // For widget-level errors, show a compact error state
      if (level === 'widget') {
        return (
          <Alert
            message="Widget Error"
            description="This widget encountered an error and cannot be displayed."
            type="error"
            showIcon
            action={
              <Button size="small" onClick={() => this.handleRetry()}>
                Retry
              </Button>
            }
            style={{ margin: '8px' }}
          />
        )
      }

      return (
        <Card style={{ margin: level === 'page' ? '20px' : '8px' }}>
          <Result
            status="error"
            title={messages.title}
            subTitle={messages.subtitle}
            icon={severity === 'high' ? <BugOutlined /> : <WarningOutlined />}
            extra={this.getRecoveryActions()}
          >
            <div style={{ textAlign: 'left' }}>
              <Paragraph>{messages.description}</Paragraph>
              
              {this.state.retryCount > 0 && (
                <Alert
                  message={`Retry attempt ${this.state.retryCount} of ${this.props.maxRetryAttempts || 3}`}
                  type="info"
                  showIcon
                  style={{ marginBottom: '16px' }}
                />
              )}

              {showDetails && this.state.error && (
                <Collapse ghost>
                  <Panel 
                    header={
                      <Space>
                        <InfoCircleOutlined />
                        <Text>Error Details (Development Mode)</Text>
                      </Space>
                    } 
                    key="details"
                  >
                    <Paragraph>
                      <Text strong>Error Message:</Text>
                    </Paragraph>
                    <Paragraph>
                      <Text code>{this.state.error.message}</Text>
                    </Paragraph>
                    
                    {this.state.error.stack && (
                      <>
                        <Paragraph>
                          <Text strong>Stack Trace:</Text>
                        </Paragraph>
                        <Paragraph>
                          <Text code style={{ fontSize: '11px', whiteSpace: 'pre-wrap' }}>
                            {this.state.error.stack}
                          </Text>
                        </Paragraph>
                      </>
                    )}
                    
                    {this.state.errorInfo && (
                      <>
                        <Paragraph>
                          <Text strong>Component Stack:</Text>
                        </Paragraph>
                        <Paragraph>
                          <Text code style={{ fontSize: '11px', whiteSpace: 'pre-wrap' }}>
                            {this.state.errorInfo.componentStack}
                          </Text>
                        </Paragraph>
                      </>
                    )}
                  </Panel>
                </Collapse>
              )}
            </div>
          </Result>
        </Card>
      )
    }

    return this.props.children
  }
}

export default EnhancedErrorBoundary
