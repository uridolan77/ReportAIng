import React, { Component, ErrorInfo, ReactNode } from 'react'
import { Result, Button, Card, Space, Typography, Alert, Collapse } from 'antd'
import {
  ExclamationCircleOutlined,
  ReloadOutlined,
  BugOutlined,
  InfoCircleOutlined,
  HomeOutlined
} from '@ant-design/icons'

const { Title, Text, Paragraph } = Typography

interface ErrorBoundaryState {
  hasError: boolean
  error: Error | null
  errorInfo: ErrorInfo | null
  errorId: string
}

interface ErrorBoundaryProps {
  children: ReactNode
  fallback?: ReactNode
  onError?: (error: Error, errorInfo: ErrorInfo) => void
  showDetails?: boolean
  level?: 'page' | 'component' | 'section'
  componentName?: string
}

/**
 * ErrorBoundary - Comprehensive error boundary with detailed error reporting
 * 
 * Features:
 * - Graceful error handling
 * - Detailed error information
 * - Recovery options
 * - Error reporting
 * - Accessibility support
 * - Different fallback levels
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props)
    
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
      errorId: ''
    }
  }

  static getDerivedStateFromError(error: Error): Partial<ErrorBoundaryState> {
    return {
      hasError: true,
      error,
      errorId: `error-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
    }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    this.setState({
      error,
      errorInfo
    })

    // Log error to console in development
    if (process.env.NODE_ENV === 'development') {
      console.error('ErrorBoundary caught an error:', error, errorInfo)
    }

    // Call custom error handler
    this.props.onError?.(error, errorInfo)

    // Report error to monitoring service
    this.reportError(error, errorInfo)
  }

  private reportError = (error: Error, errorInfo: ErrorInfo) => {
    // In a real application, this would send error to monitoring service
    const errorReport = {
      errorId: this.state.errorId,
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      url: window.location.href,
      componentName: this.props.componentName
    }

    // Example: Send to error tracking service
    // errorTrackingService.report(errorReport)
    
    console.warn('Error reported:', errorReport)
  }

  private handleRetry = () => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
      errorId: ''
    })
  }

  private handleReload = () => {
    window.location.reload()
  }

  private handleGoHome = () => {
    window.location.href = '/'
  }

  private renderPageLevelError = () => (
    <div style={{ 
      minHeight: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      padding: '20px'
    }}>
      <Result
        status="error"
        title="Something went wrong"
        subTitle="We're sorry, but something unexpected happened. Please try refreshing the page or contact support if the problem persists."
        extra={[
          <Button type="primary" key="reload" icon={<ReloadOutlined />} onClick={this.handleReload}>
            Reload Page
          </Button>,
          <Button key="home" icon={<HomeOutlined />} onClick={this.handleGoHome}>
            Go Home
          </Button>
        ]}
      >
        {this.props.showDetails && this.state.error && (
          <Card size="small" style={{ textAlign: 'left', marginTop: '20px' }}>
            <Collapse
              ghost
              items={[{
                key: 'details',
                label: 'Error Details',
                children: (
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Alert
                      message="Error ID"
                      description={this.state.errorId}
                      type="info"
                      size="small"
                    />
                    <div>
                      <Text strong>Error Message:</Text>
                      <Paragraph code>{this.state.error.message}</Paragraph>
                    </div>
                    {this.state.error.stack && (
                      <div>
                        <Text strong>Stack Trace:</Text>
                        <Paragraph code style={{ fontSize: '11px', maxHeight: '200px', overflow: 'auto' }}>
                          {this.state.error.stack}
                        </Paragraph>
                      </div>
                    )}
                  </Space>
                )
              }]}
            />
          </Card>
        )}
      </Result>
    </div>
  )

  private renderComponentLevelError = () => (
    <Card 
      size="small"
      style={{ 
        border: '1px solid #ff4d4f',
        background: '#fff2f0'
      }}
    >
      <Result
        status="error"
        title="Component Error"
        subTitle={`The ${this.props.componentName || 'component'} encountered an error and couldn't render properly.`}
        extra={[
          <Button key="retry" type="primary" size="small" icon={<ReloadOutlined />} onClick={this.handleRetry}>
            Try Again
          </Button>
        ]}
      >
        {this.props.showDetails && this.state.error && (
          <Alert
            message={this.state.error.message}
            description={`Error ID: ${this.state.errorId}`}
            type="error"
            size="small"
            style={{ textAlign: 'left' }}
          />
        )}
      </Result>
    </Card>
  )

  private renderSectionLevelError = () => (
    <Alert
      message="Section Error"
      description={
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text>This section encountered an error and couldn't load properly.</Text>
          <Space>
            <Button size="small" icon={<ReloadOutlined />} onClick={this.handleRetry}>
              Retry
            </Button>
            {this.props.showDetails && (
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Error ID: {this.state.errorId}
              </Text>
            )}
          </Space>
        </Space>
      }
      type="error"
      showIcon
      style={{ margin: '16px 0' }}
    />
  )

  render() {
    if (this.state.hasError) {
      // Custom fallback UI
      if (this.props.fallback) {
        return this.props.fallback
      }

      // Level-based fallback UI
      switch (this.props.level) {
        case 'page':
          return this.renderPageLevelError()
        case 'component':
          return this.renderComponentLevelError()
        case 'section':
          return this.renderSectionLevelError()
        default:
          return this.renderComponentLevelError()
      }
    }

    return this.props.children
  }
}

// Higher-order component for easy error boundary wrapping
export const withErrorBoundary = <P extends object>(
  Component: React.ComponentType<P>,
  errorBoundaryProps?: Omit<ErrorBoundaryProps, 'children'>
) => {
  const WrappedComponent = (props: P) => (
    <ErrorBoundary {...errorBoundaryProps}>
      <Component {...props} />
    </ErrorBoundary>
  )

  WrappedComponent.displayName = `withErrorBoundary(${Component.displayName || Component.name})`
  
  return WrappedComponent
}

// Specialized error boundaries for different contexts
export const TransparencyErrorBoundary: React.FC<{ children: ReactNode }> = ({ children }) => (
  <ErrorBoundary
    level="component"
    componentName="Transparency Component"
    showDetails={process.env.NODE_ENV === 'development'}
    onError={(error, errorInfo) => {
      console.error('Transparency component error:', error, errorInfo)
    }}
  >
    {children}
  </ErrorBoundary>
)

export const ChatErrorBoundary: React.FC<{ children: ReactNode }> = ({ children }) => (
  <ErrorBoundary
    level="section"
    componentName="Chat Interface"
    showDetails={process.env.NODE_ENV === 'development'}
    onError={(error, errorInfo) => {
      console.error('Chat interface error:', error, errorInfo)
    }}
  >
    {children}
  </ErrorBoundary>
)

export const PageErrorBoundary: React.FC<{ children: ReactNode }> = ({ children }) => (
  <ErrorBoundary
    level="page"
    showDetails={process.env.NODE_ENV === 'development'}
    onError={(error, errorInfo) => {
      console.error('Page-level error:', error, errorInfo)
    }}
  >
    {children}
  </ErrorBoundary>
)

export default ErrorBoundary
