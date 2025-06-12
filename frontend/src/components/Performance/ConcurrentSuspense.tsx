/**
 * Concurrent Suspense Boundary
 * 
 * Advanced Suspense boundary with React 18 concurrent features
 * for optimal performance and user experience
 */

import React, { 
  Suspense, 
  startTransition, 
  useDeferredValue, 
  useState, 
  useCallback,
  useEffect,
  ReactNode 
} from 'react';
import { Spin, Alert, Button, Progress } from 'antd';
import { ReloadOutlined, ThunderboltOutlined } from '@ant-design/icons';

interface ConcurrentSuspenseProps {
  children: ReactNode;
  fallback?: ReactNode;
  errorFallback?: ReactNode;
  enableProgressiveLoading?: boolean;
  priority?: 'high' | 'normal' | 'low';
  timeout?: number;
  onLoadStart?: () => void;
  onLoadComplete?: () => void;
  onError?: (error: Error) => void;
}

interface LoadingState {
  isLoading: boolean;
  progress: number;
  stage: string;
  startTime: number;
}

export const ConcurrentSuspense: React.FC<ConcurrentSuspenseProps> = ({
  children,
  fallback,
  errorFallback,
  enableProgressiveLoading = true,
  priority = 'normal',
  timeout = 10000,
  onLoadStart,
  onLoadComplete,
  onError
}) => {
  const [loadingState, setLoadingState] = useState<LoadingState>({
    isLoading: false,
    progress: 0,
    stage: 'Initializing...',
    startTime: 0
  });
  const [error, setError] = useState<Error | null>(null);
  const [retryCount, setRetryCount] = useState(0);

  // Deferred loading state for smooth transitions
  const deferredLoading = useDeferredValue(loadingState.isLoading);

  // Progressive loading stages
  const loadingStages = [
    'Initializing components...',
    'Loading dependencies...',
    'Preparing interface...',
    'Finalizing...'
  ];

  // Simulate progressive loading
  useEffect(() => {
    if (loadingState.isLoading && enableProgressiveLoading) {
      const interval = setInterval(() => {
        setLoadingState(prev => {
          const newProgress = Math.min(prev.progress + 25, 100);
          const stageIndex = Math.floor(newProgress / 25);
          const stage = loadingStages[Math.min(stageIndex, loadingStages.length - 1)];
          
          if (newProgress >= 100) {
            clearInterval(interval);
            onLoadComplete?.();
            return { ...prev, isLoading: false, progress: 100, stage: 'Complete' };
          }
          
          return { ...prev, progress: newProgress, stage };
        });
      }, 300);

      return () => clearInterval(interval);
    }
  }, [loadingState.isLoading, enableProgressiveLoading, onLoadComplete]);

  // Timeout handling
  useEffect(() => {
    if (loadingState.isLoading) {
      const timeoutId = setTimeout(() => {
        setError(new Error(`Loading timeout after ${timeout}ms`));
        setLoadingState(prev => ({ ...prev, isLoading: false }));
      }, timeout);

      return () => clearTimeout(timeoutId);
    }
  }, [loadingState.isLoading, timeout]);

  const handleRetry = useCallback(() => {
    startTransition(() => {
      setError(null);
      setRetryCount(prev => prev + 1);
      setLoadingState({
        isLoading: true,
        progress: 0,
        stage: 'Retrying...',
        startTime: Date.now()
      });
      onLoadStart?.();
    });
  }, [onLoadStart]);

  const handleLoadStart = useCallback(() => {
    if (priority === 'low') {
      startTransition(() => {
        setLoadingState({
          isLoading: true,
          progress: 0,
          stage: 'Initializing...',
          startTime: Date.now()
        });
        onLoadStart?.();
      });
    } else {
      setLoadingState({
        isLoading: true,
        progress: 0,
        stage: 'Initializing...',
        startTime: Date.now()
      });
      onLoadStart?.();
    }
  }, [priority, onLoadStart]);

  // Error boundary functionality
  const handleError = useCallback((error: Error) => {
    setError(error);
    setLoadingState(prev => ({ ...prev, isLoading: false }));
    onError?.(error);
  }, [onError]);

  // Custom fallback with progressive loading
  const renderFallback = () => {
    if (fallback) return fallback;

    const loadingTime = Date.now() - loadingState.startTime;
    const isSlowLoading = loadingTime > 2000;

    return (
      <div style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '200px',
        padding: '40px',
        background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
        borderRadius: '12px',
        boxShadow: '0 4px 20px rgba(0, 0, 0, 0.1)'
      }}>
        <div style={{ marginBottom: '24px' }}>
          <Spin 
            size="large" 
            indicator={
              <ThunderboltOutlined 
                style={{ 
                  fontSize: 32, 
                  color: '#1890ff',
                  animation: 'spin 1s linear infinite'
                }} 
              />
            }
          />
        </div>

        {enableProgressiveLoading && (
          <div style={{ width: '100%', maxWidth: '300px', marginBottom: '16px' }}>
            <Progress
              percent={loadingState.progress}
              strokeColor={{
                '0%': '#108ee9',
                '100%': '#87d068',
              }}
              showInfo={false}
              strokeWidth={8}
            />
            <div style={{ 
              textAlign: 'center', 
              marginTop: '8px',
              fontSize: '14px',
              color: '#666'
            }}>
              {loadingState.stage}
            </div>
          </div>
        )}

        <div style={{ 
          fontSize: '16px', 
          fontWeight: 500, 
          color: '#1890ff',
          marginBottom: '8px'
        }}>
          Loading Application...
        </div>

        {isSlowLoading && (
          <div style={{ 
            fontSize: '12px', 
            color: '#999',
            textAlign: 'center'
          }}>
            This is taking longer than usual.<br />
            Please check your connection.
          </div>
        )}

        {retryCount > 0 && (
          <div style={{ 
            fontSize: '12px', 
            color: '#faad14',
            marginTop: '8px'
          }}>
            Retry attempt: {retryCount}
          </div>
        )}
      </div>
    );
  };

  // Error fallback
  const renderErrorFallback = () => {
    if (errorFallback) return errorFallback;

    return (
      <Alert
        message="Loading Error"
        description={
          <div>
            <p>{error?.message || 'Failed to load component'}</p>
            <Button 
              type="primary" 
              icon={<ReloadOutlined />}
              onClick={handleRetry}
              style={{ marginTop: '12px' }}
            >
              Retry Loading
            </Button>
          </div>
        }
        type="error"
        showIcon
        style={{
          margin: '20px',
          borderRadius: '8px'
        }}
      />
    );
  };

  if (error) {
    return renderErrorFallback();
  }

  return (
    <Suspense 
      fallback={deferredLoading ? renderFallback() : null}
    >
      <ErrorBoundary onError={handleError}>
        {children}
      </ErrorBoundary>
    </Suspense>
  );
};

// Simple Error Boundary for Suspense
class ErrorBoundary extends React.Component<
  { children: ReactNode; onError?: (error: Error) => void },
  { hasError: boolean }
> {
  constructor(props: { children: ReactNode; onError?: (error: Error) => void }) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): { hasError: boolean } {
    return { hasError: true };
  }

  componentDidCatch(error: Error) {
    this.props.onError?.(error);
  }

  render() {
    if (this.state.hasError) {
      return null; // Let parent handle error display
    }

    return this.props.children;
  }
}

export default ConcurrentSuspense;
