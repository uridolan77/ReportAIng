/**
 * Modern Performance Components
 * 
 * Provides comprehensive performance optimization components including lazy loading,
 * virtualization, memoization, and performance monitoring.
 */

import React, { 
  forwardRef, 
  memo, 
  useMemo, 
  useCallback, 
  useEffect, 
  useState, 
  Suspense,
  ComponentType,
  ReactNode
} from 'react';
import { useInView } from 'react-intersection-observer';
import { FixedSizeList as List } from 'react-window';
import { designTokens } from './design-system';

// Types
export interface LazyComponentProps {
  loader: () => Promise<{ default: ComponentType<any> }>;
  fallback?: ReactNode;
  delay?: number;
  errorBoundary?: ComponentType<any>;
}

export interface VirtualListProps {
  items: any[];
  itemHeight: number;
  containerHeight: number;
  renderItem: (item: any, index: number) => ReactNode;
  overscan?: number;
  className?: string;
  style?: React.CSSProperties;
}

export interface MemoizedProps {
  children: ReactNode;
  dependencies?: any[];
  shouldUpdate?: (prevProps: any, nextProps: any) => boolean;
}

export interface InViewProps {
  children: ReactNode;
  threshold?: number;
  triggerOnce?: boolean;
  fallback?: ReactNode;
  rootMargin?: string;
}

export interface PerformanceMonitorProps {
  children: ReactNode;
  enabled?: boolean;
  onMetrics?: (metrics: PerformanceMetrics) => void;
  sampleRate?: number;
}

export interface PerformanceMetrics {
  renderTime: number;
  componentCount: number;
  memoryUsage?: number;
  timestamp: number;
}

// Lazy Component
export const LazyComponent: React.FC<LazyComponentProps> = ({
  loader,
  fallback = <div>Loading...</div>,
  delay = 0,
  errorBoundary: ErrorBoundary,
}) => {
  const [Component, setComponent] = useState<ComponentType<any> | null>(null);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let mounted = true;
    
    const loadComponent = async () => {
      try {
        if (delay > 0) {
          await new Promise(resolve => setTimeout(resolve, delay));
        }
        
        const module = await loader();
        
        if (mounted) {
          setComponent(() => module.default);
        }
      } catch (err) {
        if (mounted) {
          setError(err as Error);
        }
      }
    };

    loadComponent();

    return () => {
      mounted = false;
    };
  }, [loader, delay]);

  if (error) {
    if (ErrorBoundary) {
      return <ErrorBoundary error={error} />;
    }
    return <div>Error loading component: {error.message}</div>;
  }

  if (!Component) {
    return <>{fallback}</>;
  }

  return <Component />;
};

// Enhanced Virtual List Component with performance optimizations
export const VirtualList = forwardRef<HTMLDivElement, VirtualListProps>(
  ({
    items,
    itemHeight,
    containerHeight,
    renderItem,
    overscan = 5,
    className,
    style
  }, ref) => {
    // Memoize the row renderer for better performance
    const Row = useCallback(({ index, style: rowStyle }: { index: number; style: React.CSSProperties }) => {
      const item = items[index];
      if (!item) return null;

      return (
        <div style={rowStyle}>
          {renderItem(item, index)}
        </div>
      );
    }, [items, renderItem]);

    // Use intersection observer to only render when visible
    const { ref: inViewRef, inView } = useInView({
      threshold: 0,
      triggerOnce: false,
    });

    // For very large datasets, show placeholder when not in view
    if (!inView && items.length > 1000) {
      return (
        <div
          ref={(node) => {
            inViewRef(node);
            if (ref) {
              if (typeof ref === 'function') ref(node);
              else ref.current = node;
            }
          }}
          className={className}
          style={{
            ...style,
            height: containerHeight,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            backgroundColor: '#f8f9fa',
            border: '1px dashed #dee2e6'
          }}
        >
          <div style={{ color: '#6c757d', fontSize: '14px' }}>
            Large dataset - scroll to load content
          </div>
        </div>
      );
    }

    return (
      <div
        ref={(node) => {
          inViewRef(node);
          if (ref) {
            if (typeof ref === 'function') ref(node);
            else ref.current = node;
          }
        }}
        className={className}
        style={style}
      >
        <List
          height={containerHeight}
          itemCount={items.length}
          itemSize={itemHeight}
          overscanCount={overscan}
          width="100%"
        >
          {Row}
        </List>
      </div>
    );
  }
);

VirtualList.displayName = 'VirtualList';

// Memoized Component
export const Memoized: React.FC<MemoizedProps> = memo(
  ({ children, dependencies = [] }) => {
    return <>{children}</>;
  },
  (prevProps, nextProps) => {
    if (prevProps.shouldUpdate) {
      return !prevProps.shouldUpdate(prevProps, nextProps);
    }
    
    // Default comparison based on dependencies
    if (prevProps.dependencies && nextProps.dependencies) {
      return prevProps.dependencies.every((dep, index) => 
        dep === nextProps.dependencies![index]
      );
    }
    
    return false;
  }
);

Memoized.displayName = 'Memoized';

// In View Component
export const InView: React.FC<InViewProps> = ({
  children,
  threshold = 0.1,
  triggerOnce = false,
  fallback = null,
  rootMargin = '0px',
}) => {
  const { ref, inView } = useInView({
    threshold,
    triggerOnce,
    rootMargin,
  });

  return (
    <div ref={ref}>
      {inView ? children : fallback}
    </div>
  );
};

// Performance Monitor Component
export const PerformanceMonitor: React.FC<PerformanceMonitorProps> = ({
  children,
  enabled = true,
  onMetrics,
  sampleRate = 1.0,
}) => {
  const [, setMetrics] = useState<PerformanceMetrics | null>(null);

  useEffect(() => {
    if (!enabled || Math.random() > sampleRate) {
      return;
    }

    const startTime = performance.now();
    let componentCount = 0;

    // Count React components (simplified)
    const countComponents = (element: any): number => {
      if (!element) return 0;
      
      let count = 1;
      if (element.props && element.props.children) {
        const children = Array.isArray(element.props.children) 
          ? element.props.children 
          : [element.props.children];
        
        children.forEach((child: any) => {
          if (React.isValidElement(child)) {
            count += countComponents(child);
          }
        });
      }
      return count;
    };

    // Measure performance after render
    const measurePerformance = () => {
      const endTime = performance.now();
      const renderTime = endTime - startTime;
      
      // Get memory usage if available
      const memoryUsage = (performance as any).memory?.usedJSHeapSize;
      
      const performanceMetrics: PerformanceMetrics = {
        renderTime,
        componentCount,
        memoryUsage,
        timestamp: Date.now(),
      };

      setMetrics(performanceMetrics);
      
      if (onMetrics) {
        onMetrics(performanceMetrics);
      }
    };

    // Use requestIdleCallback if available, otherwise setTimeout
    if ('requestIdleCallback' in window) {
      (window as any).requestIdleCallback(measurePerformance);
    } else {
      setTimeout(measurePerformance, 0);
    }
  }, [enabled, onMetrics, sampleRate]);

  return <>{children}</>;
};

// Bundle Analyzer Component (Development only)
export const BundleAnalyzer: React.FC<{
  onAnalysis?: (analysis: any) => void;
  enabled?: boolean;
}> = ({ onAnalysis, enabled = process.env.NODE_ENV === 'development' }) => {
  useEffect(() => {
    if (!enabled || !onAnalysis) return;

    // Simple bundle analysis
    const analyzeBundle = () => {
      const scripts = Array.from(document.querySelectorAll('script[src]'));
      const stylesheets = Array.from(document.querySelectorAll('link[rel="stylesheet"]'));
      
      const analysis = {
        scripts: scripts.map((script: any) => ({
          src: script.src,
          size: script.src.length, // Simplified
        })),
        stylesheets: stylesheets.map((link: any) => ({
          href: link.href,
          size: link.href.length, // Simplified
        })),
        totalScripts: scripts.length,
        totalStylesheets: stylesheets.length,
        timestamp: Date.now(),
      };

      onAnalysis(analysis);
    };

    // Analyze after initial load
    if (document.readyState === 'complete') {
      analyzeBundle();
    } else {
      window.addEventListener('load', analyzeBundle);
      return () => window.removeEventListener('load', analyzeBundle);
    }
  }, [enabled, onAnalysis]);

  return null;
};

// Loading Fallback Component
export const LoadingFallback: React.FC<{
  message?: string;
  size?: 'small' | 'medium' | 'large';
}> = ({ message = 'Loading...', size = 'medium' }) => {
  const getSizeStyles = () => {
    const sizes = {
      small: { height: '100px', fontSize: designTokens.typography.fontSize.sm },
      medium: { height: '200px', fontSize: designTokens.typography.fontSize.base },
      large: { height: '300px', fontSize: designTokens.typography.fontSize.lg },
    };
    return sizes[size];
  };

  const sizeStyles = getSizeStyles();

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: sizeStyles.height,
        fontSize: sizeStyles.fontSize,
        color: designTokens.colors.textSecondary,
      }}
    >
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: designTokens.spacing.md,
        }}
      >
        <div
          style={{
            width: '20px',
            height: '20px',
            border: `2px solid ${designTokens.colors.border}`,
            borderTop: `2px solid ${designTokens.colors.primary}`,
            borderRadius: '50%',
            animation: 'spin 1s linear infinite',
          }}
        />
        {message}
      </div>
    </div>
  );
};
