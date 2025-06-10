/**
 * Performance Components
 * Advanced performance optimization components including virtualization and lazy loading
 */

import React, { Suspense, lazy, memo, useMemo, useCallback, useState, useEffect } from 'react';
import { Spin } from 'antd';

// Lazy Loading Component
export interface LazyComponentProps {
  loader: () => Promise<{ default: React.ComponentType<any> }>;
  fallback?: React.ReactNode;
  error?: React.ReactNode;
  delay?: number;
  children?: React.ReactNode;
}

export const LazyComponent: React.FC<LazyComponentProps> = ({
  loader,
  fallback = <Spin size="large" />,
  error = <div>Failed to load component</div>,
  delay = 200,
  children,
  ...props
}) => {
  const [showFallback, setShowFallback] = useState(false);
  const LazyLoadedComponent = useMemo(() => lazy(loader), [loader]);

  useEffect(() => {
    const timer = setTimeout(() => setShowFallback(true), delay);
    return () => clearTimeout(timer);
  }, [delay]);

  return (
    <Suspense fallback={showFallback ? fallback : null}>
      <LazyLoadedComponent {...props}>
        {children}
      </LazyLoadedComponent>
    </Suspense>
  );
};

// Virtual List Component
export interface VirtualListProps {
  items: any[];
  itemHeight: number;
  containerHeight: number;
  renderItem: (item: any, index: number) => React.ReactNode;
  overscan?: number;
  className?: string;
  style?: React.CSSProperties;
}

export const VirtualList: React.FC<VirtualListProps> = memo(({
  items,
  itemHeight,
  containerHeight,
  renderItem,
  overscan = 5,
  className,
  style,
}) => {
  const [scrollTop, setScrollTop] = useState(0);

  const visibleCount = Math.ceil(containerHeight / itemHeight);
  const startIndex = Math.max(0, Math.floor(scrollTop / itemHeight) - overscan);
  const endIndex = Math.min(items.length - 1, startIndex + visibleCount + overscan * 2);

  const visibleItems = useMemo(() => {
    return items.slice(startIndex, endIndex + 1).map((item, index) => ({
      item,
      index: startIndex + index,
    }));
  }, [items, startIndex, endIndex]);

  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    setScrollTop(e.currentTarget.scrollTop);
  }, []);

  return (
    <div
      className={`ui-virtual-list ${className || ''}`}
      style={{
        height: containerHeight,
        overflow: 'auto',
        ...style,
      }}
      onScroll={handleScroll}
    >
      <div
        style={{
          height: items.length * itemHeight,
          position: 'relative',
        }}
      >
        {visibleItems.map(({ item, index }) => (
          <div
            key={index}
            style={{
              position: 'absolute',
              top: index * itemHeight,
              left: 0,
              right: 0,
              height: itemHeight,
            }}
          >
            {renderItem(item, index)}
          </div>
        ))}
      </div>
    </div>
  );
});

VirtualList.displayName = 'VirtualList';

// Memoized Component Wrapper
export interface MemoizedProps {
  children: React.ReactNode;
  deps?: any[];
  compare?: (prevProps: any, nextProps: any) => boolean;
}

export const Memoized: React.FC<MemoizedProps> = memo(({ children }) => {
  return <>{children}</>;
}, (prevProps, nextProps) => {
  if (prevProps.compare) {
    return prevProps.compare(prevProps, nextProps);
  }
  return JSON.stringify(prevProps.deps) === JSON.stringify(nextProps.deps);
});

Memoized.displayName = 'Memoized';

// Debounced Component
export interface DebouncedProps {
  children: React.ReactNode;
  delay?: number;
  deps?: any[];
}

export const Debounced: React.FC<DebouncedProps> = ({ children, delay = 300, deps = [] }) => {
  const [debouncedChildren, setDebouncedChildren] = useState(children);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedChildren(children);
    }, delay);

    return () => clearTimeout(timer);
  }, [children, delay, ...deps]);

  return <>{debouncedChildren}</>;
};

// Intersection Observer Component
export interface InViewProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  threshold?: number;
  rootMargin?: string;
  triggerOnce?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export const InView: React.FC<InViewProps> = ({
  children,
  fallback = null,
  threshold = 0.1,
  rootMargin = '0px',
  triggerOnce = true,
  className,
  style,
}) => {
  const [isInView, setIsInView] = useState(false);
  const [hasTriggered, setHasTriggered] = useState(false);
  const [ref, setRef] = useState<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!ref) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        const inView = entry.isIntersecting;
        setIsInView(inView);
        
        if (inView && triggerOnce && !hasTriggered) {
          setHasTriggered(true);
        }
      },
      { threshold, rootMargin }
    );

    observer.observe(ref);

    return () => observer.disconnect();
  }, [ref, threshold, rootMargin, triggerOnce, hasTriggered]);

  const shouldRender = triggerOnce ? (isInView || hasTriggered) : isInView;

  return (
    <div
      ref={setRef}
      className={`ui-in-view ${className || ''}`}
      style={style}
    >
      {shouldRender ? children : fallback}
    </div>
  );
};

// Image Lazy Loading Component
export interface LazyImageProps {
  src: string;
  alt: string;
  placeholder?: string;
  fallback?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  onLoad?: () => void;
  onError?: () => void;
}

export const LazyImage: React.FC<LazyImageProps> = ({
  src,
  alt,
  placeholder,
  fallback = <div style={{ backgroundColor: '#f0f0f0', width: '100%', height: '100%' }} />,
  className,
  style,
  onLoad,
  onError,
}) => {
  const [loaded, setLoaded] = useState(false);
  const [error, setError] = useState(false);

  const handleLoad = useCallback(() => {
    setLoaded(true);
    onLoad?.();
  }, [onLoad]);

  const handleError = useCallback(() => {
    setError(true);
    onError?.();
  }, [onError]);

  if (error) {
    return <div className={`ui-lazy-image-error ${className || ''}`} style={style}>{fallback}</div>;
  }

  return (
    <InView fallback={placeholder ? <img src={placeholder} alt={alt} className={className} style={style} /> : fallback}>
      <img
        src={src}
        alt={alt}
        className={`ui-lazy-image ${className || ''}`}
        style={{
          opacity: loaded ? 1 : 0,
          transition: 'opacity 0.3s ease',
          ...style,
        }}
        onLoad={handleLoad}
        onError={handleError}
      />
    </InView>
  );
};

// Code Splitting Component
export interface CodeSplitProps {
  component: string;
  fallback?: React.ReactNode;
  error?: React.ReactNode;
  props?: any;
}

export const CodeSplit: React.FC<CodeSplitProps> = ({
  component,
  fallback = <Spin size="large" />,
  error = <div>Failed to load component</div>,
  props = {},
}) => {
  const LazyComponent = useMemo(() => {
    return lazy(() => import(`../${component}`).catch(() => ({ default: () => error })));
  }, [component, error]);

  return (
    <Suspense fallback={fallback}>
      <LazyComponent {...props} />
    </Suspense>
  );
};

// Performance Monitor Component
export interface PerformanceMonitorProps {
  children: React.ReactNode;
  onMetrics?: (metrics: any) => void;
  enabled?: boolean;
}

export const PerformanceMonitor: React.FC<PerformanceMonitorProps> = ({
  children,
  onMetrics,
  enabled = process.env.NODE_ENV === 'development',
}) => {
  useEffect(() => {
    if (!enabled || !onMetrics) return;

    const observer = new PerformanceObserver((list) => {
      const entries = list.getEntries();
      onMetrics(entries);
    });

    observer.observe({ entryTypes: ['measure', 'navigation', 'paint'] });

    return () => observer.disconnect();
  }, [enabled, onMetrics]);

  return <>{children}</>;
};

// Bundle Analyzer Component (Development only)
export interface BundleAnalyzerProps {
  enabled?: boolean;
  onAnalysis?: (analysis: any) => void;
}

export const BundleAnalyzer: React.FC<BundleAnalyzerProps> = ({
  enabled = process.env.NODE_ENV === 'development',
  onAnalysis,
}) => {
  useEffect(() => {
    if (!enabled || !onAnalysis) return;

    // Simulate bundle analysis
    const analysis = {
      totalSize: 0,
      chunks: [],
      modules: [],
      timestamp: Date.now(),
    };

    onAnalysis(analysis);
  }, [enabled, onAnalysis]);

  return null;
};

export default {
  LazyComponent,
  VirtualList,
  Memoized,
  Debounced,
  InView,
  LazyImage,
  CodeSplit,
  PerformanceMonitor,
  BundleAnalyzer,
};
