import React, { Component, ErrorInfo } from 'react';
import { Result, Button } from 'antd';
import { ErrorService } from '../../services/errorService';

interface Props {
  children: React.ReactNode;
  fallback?: React.ComponentType<{ error: Error; resetError: () => void }>;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    ErrorService.logError(error, {
      componentStack: errorInfo.componentStack,
      errorBoundary: true,
    });
  }

  resetError = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        const FallbackComponent = this.props.fallback;
        return <FallbackComponent error={this.state.error!} resetError={this.resetError} />;
      }

      return (
        <Result
          status="error"
          title="Something went wrong"
          subTitle="An unexpected error occurred. Please try refreshing the page or contact support if the problem persists."
          extra={
            <Button type="primary" onClick={this.resetError}>
              Try Again
            </Button>
          }
        />
      );
    }

    return this.props.children;
  }
}

// Specific error fallback components
export const QueryErrorFallback: React.FC<{ error: Error; resetError: () => void }> = ({ 
  error, 
  resetError 
}) => (
  <Result
    status="warning"
    title="Query Error"
    subTitle={`There was an issue with your query: ${error.message}`}
    extra={[
      <Button type="primary" onClick={resetError} key="retry">
        Try Again
      </Button>,
      <Button key="home" onClick={() => window.location.href = '/'}>
        Go Home
      </Button>
    ]}
  />
);

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
