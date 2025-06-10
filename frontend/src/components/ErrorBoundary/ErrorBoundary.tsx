import React, { Component, ErrorInfo } from 'react';
import { Result, Button, Alert, Space, Typography, Card, Collapse } from 'antd';
import {
  ReloadOutlined,
  BugOutlined,
  HomeOutlined,
  WarningOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import { ErrorService } from '../../services/errorService';

const { Text } = Typography;
const { Panel } = Collapse;

interface Props {
  children: React.ReactNode;
  fallback?: React.ComponentType<{ error: Error; resetError: () => void; errorInfo?: ErrorInfo }>;
  level?: 'page' | 'component' | 'critical';
  onError?: (error: Error, errorInfo: ErrorInfo) => void;
  maxRetries?: number;
  autoRetry?: boolean;
  retryDelay?: number;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
  retryCount: number;
  isRetrying: boolean;
  errorId: string;
}

export class ErrorBoundary extends Component<Props, State> {
  private retryTimeoutId: NodeJS.Timeout | null = null;

  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
      retryCount: 0,
      isRetrying: false,
      errorId: ''
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    const errorId = `error_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    return {
      hasError: true,
      error,
      errorId
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    this.setState({ errorInfo });

    // Log error with enhanced context
    ErrorService.logError(error, {
      componentStack: errorInfo.componentStack,
      errorBoundary: true,
      level: this.props.level || 'component',
      errorId: this.state.errorId,
      retryCount: this.state.retryCount,
      userAgent: navigator.userAgent,
      timestamp: new Date().toISOString(),
      url: window.location.href
    });

    // Call custom error handler if provided
    if (this.props.onError) {
      this.props.onError(error, errorInfo);
    }

    // Auto-retry for non-critical errors
    if (this.props.autoRetry && this.shouldAutoRetry(error)) {
      this.scheduleAutoRetry();
    }
  }

  componentWillUnmount() {
    if (this.retryTimeoutId) {
      clearTimeout(this.retryTimeoutId);
    }
  }

  private shouldAutoRetry(error: Error): boolean {
    const maxRetries = this.props.maxRetries || 3;
    const isRetryableError = this.isRetryableError(error);

    return (
      this.state.retryCount < maxRetries &&
      isRetryableError &&
      this.props.level !== 'critical'
    );
  }

  private isRetryableError(error: Error): boolean {
    // Network errors, timeout errors, and temporary failures are retryable
    const retryablePatterns = [
      /network/i,
      /timeout/i,
      /fetch/i,
      /connection/i,
      /temporary/i,
      /rate limit/i
    ];

    const errorMessage = error.message.toLowerCase();
    return retryablePatterns.some(pattern => pattern.test(errorMessage));
  }

  private scheduleAutoRetry(): void {
    const delay = this.props.retryDelay || (1000 * Math.pow(2, this.state.retryCount)); // Exponential backoff

    this.setState({ isRetrying: true });

    this.retryTimeoutId = setTimeout(() => {
      this.retryError();
    }, delay);
  }

  resetError = () => {
    if (this.retryTimeoutId) {
      clearTimeout(this.retryTimeoutId);
      this.retryTimeoutId = null;
    }

    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
      retryCount: 0,
      isRetrying: false,
      errorId: ''
    });
  };

  retryError = () => {
    this.setState(prevState => ({
      hasError: false,
      error: null,
      errorInfo: null,
      retryCount: prevState.retryCount + 1,
      isRetrying: false
    }));
  };

  private getErrorSeverity(): 'error' | 'warning' | 'info' {
    if (this.props.level === 'critical') return 'error';
    if (this.state.retryCount > 0) return 'warning';
    return 'error';
  }

  private getErrorTitle(): string {
    switch (this.props.level) {
      case 'critical':
        return 'Critical System Error';
      case 'page':
        return 'Page Error';
      case 'component':
      default:
        return 'Component Error';
    }
  }

  private getErrorDescription(): string {
    const { error, retryCount } = this.state;
    const maxRetries = this.props.maxRetries || 3;

    if (this.state.isRetrying) {
      return `Attempting to recover... (Retry ${retryCount + 1}/${maxRetries})`;
    }

    if (retryCount > 0) {
      return `Error persisted after ${retryCount} retry attempt${retryCount > 1 ? 's' : ''}. ${error?.message || 'Unknown error occurred.'}`;
    }

    return error?.message || 'An unexpected error occurred. Please try again or contact support if the problem persists.';
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        const FallbackComponent = this.props.fallback;
        return (
          <FallbackComponent
            error={this.state.error!}
            resetError={this.resetError}
            errorInfo={this.state.errorInfo || undefined}
          />
        );
      }

      const { error, errorInfo, retryCount, isRetrying, errorId } = this.state;
      const maxRetries = this.props.maxRetries || 3;
      const canRetry = retryCount < maxRetries;

      return (
        <div style={{ padding: '24px' }}>
          <Result
            status={this.getErrorSeverity()}
            title={this.getErrorTitle()}
            subTitle={this.getErrorDescription()}
            extra={
              <Space direction="vertical" style={{ width: '100%' }}>
                <Space wrap>
                  <Button
                    type="primary"
                    icon={<ReloadOutlined />}
                    onClick={this.resetError}
                    loading={isRetrying}
                    disabled={isRetrying}
                  >
                    {isRetrying ? 'Retrying...' : 'Try Again'}
                  </Button>

                  {canRetry && !isRetrying && (
                    <Button
                      icon={<ReloadOutlined />}
                      onClick={this.retryError}
                    >
                      Retry ({retryCount}/{maxRetries})
                    </Button>
                  )}

                  <Button
                    icon={<HomeOutlined />}
                    onClick={() => window.location.href = '/'}
                  >
                    Go Home
                  </Button>
                </Space>

                {/* Error Details for Development */}
                {process.env.NODE_ENV === 'development' && (
                  <Card style={{ marginTop: '16px', textAlign: 'left' }}>
                    <Collapse ghost>
                      <Panel
                        header={
                          <Space>
                            <BugOutlined />
                            <Text strong>Error Details (Development)</Text>
                          </Space>
                        }
                        key="error-details"
                      >
                        <Space direction="vertical" style={{ width: '100%' }}>
                          <div>
                            <Text strong>Error ID:</Text> <Text code>{errorId}</Text>
                          </div>
                          <div>
                            <Text strong>Message:</Text> <Text>{error?.message}</Text>
                          </div>
                          <div>
                            <Text strong>Stack:</Text>
                            <pre style={{
                              background: '#f5f5f5',
                              padding: '8px',
                              borderRadius: '4px',
                              fontSize: '12px',
                              overflow: 'auto',
                              maxHeight: '200px'
                            }}>
                              {error?.stack}
                            </pre>
                          </div>
                          {errorInfo && (
                            <div>
                              <Text strong>Component Stack:</Text>
                              <pre style={{
                                background: '#f5f5f5',
                                padding: '8px',
                                borderRadius: '4px',
                                fontSize: '12px',
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
                )}

                {/* User Guidance */}
                <Alert
                  type="info"
                  icon={<InfoCircleOutlined />}
                  message="What you can do:"
                  description={
                    <ul style={{ margin: 0, paddingLeft: '20px' }}>
                      <li>Try refreshing the page</li>
                      <li>Check your internet connection</li>
                      <li>Clear your browser cache</li>
                      <li>Contact support if the problem persists</li>
                    </ul>
                  }
                  style={{ textAlign: 'left' }}
                />
              </Space>
            }
          />
        </div>
      );
    }

    return this.props.children;
  }
}

// Specific error fallback components
export const QueryErrorFallback: React.FC<{
  error: Error;
  resetError: () => void;
  errorInfo?: ErrorInfo;
}> = ({ error, resetError, errorInfo }) => (
  <div style={{ padding: '16px' }}>
    <Alert
      type="error"
      icon={<WarningOutlined />}
      message="Query Execution Failed"
      description={
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text>{error.message}</Text>
          <Space>
            <Button type="primary" size="small" onClick={resetError}>
              Try Again
            </Button>
            <Button size="small" onClick={() => window.location.reload()}>
              Refresh Page
            </Button>
          </Space>
        </Space>
      }
      showIcon
    />
  </div>
);

export const NetworkErrorFallback: React.FC<{
  error: Error;
  resetError: () => void;
}> = ({ error, resetError }) => (
  <Result
    status="500"
    title="Connection Error"
    subTitle="Unable to connect to the server. Please check your internet connection and try again."
    extra={
      <Space>
        <Button type="primary" icon={<ReloadOutlined />} onClick={resetError}>
          Retry Connection
        </Button>
        <Button onClick={() => window.location.reload()}>
          Refresh Page
        </Button>
      </Space>
    }
  />
);

export const AuthErrorFallback: React.FC<{
  error: Error;
  resetError: () => void;
}> = ({ error, resetError }) => (
  <Result
    status="403"
    title="Authentication Error"
    subTitle="Your session has expired or you don't have permission to access this resource."
    extra={
      <Space>
        <Button type="primary" onClick={() => window.location.href = '/login'}>
          Login Again
        </Button>
        <Button onClick={resetError}>
          Try Again
        </Button>
      </Space>
    }
  />
);

// Higher-order component for wrapping components with error boundaries
export const withErrorBoundary = <P extends object>(
  Component: React.ComponentType<P>,
  errorBoundaryProps?: Omit<Props, 'children'>
) => {
  const WrappedComponent = (props: P) => (
    <ErrorBoundary {...errorBoundaryProps}>
      <Component {...props} />
    </ErrorBoundary>
  );

  WrappedComponent.displayName = `withErrorBoundary(${Component.displayName || Component.name})`;
  return WrappedComponent;
};

export const VisualizationErrorFallback: React.FC<{ error: Error; resetError: () => void }> = ({
  error,
  resetError
}) => (
  <Result
    status="warning"
    title="Visualization Error"
    subTitle={`Unable to render visualization: ${error.message}`}
    extra={
      <Button type="primary" onClick={resetError}>
        Retry Visualization
      </Button>
    }
  />
);
