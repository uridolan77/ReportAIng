/**
 * Performance Components
 * Centralized exports for all performance optimization components
 */

// Core Performance Components
export * from './MemoizedComponents';
export { PerformanceMonitoringDashboard } from './PerformanceMonitoringDashboard';
export { PerformanceOptimizer } from './PerformanceOptimizer';
export { VirtualScrollList } from './VirtualScrollList';

// Type definitions for performance components
export interface PerformanceMonitoringProps {
  enableMetrics?: boolean;
  showRealTime?: boolean;
  onMetricsUpdate?: (metrics: PerformanceMetrics) => void;
}

export interface PerformanceMetrics {
  renderTime: number;
  memoryUsage: number;
  componentCount: number;
  reRenderCount: number;
  timestamp: number;
}

export interface VirtualScrollProps {
  items: any[];
  itemHeight: number;
  containerHeight: number;
  renderItem: (item: any, index: number) => React.ReactNode;
  overscan?: number;
}

export interface MemoizedComponentProps {
  shouldMemoize?: boolean;
  dependencies?: any[];
  children: React.ReactNode;
}

export interface PerformanceOptimizerProps {
  enableLazyLoading?: boolean;
  enableVirtualization?: boolean;
  enableMemoization?: boolean;
  children: React.ReactNode;
}
