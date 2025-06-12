/**
 * Performance Utilities
 *
 * Advanced performance optimization utilities including bundle analysis,
 * code splitting, lazy loading, and performance monitoring.
 */

// Core Performance Monitoring (Available)
export { PerformanceMonitor } from './PerformanceMonitor';
export { PerformanceOptimizer, performanceOptimizer } from './PerformanceOptimizer';

// Types for performance metrics
export interface PerformanceMetrics {
  fcp?: number;
  lcp?: number;
  fid?: number;
  cls?: number;
  memory?: number;
  timestamp: number;
}

export interface OptimizationSuggestion {
  type: string;
  message: string;
  severity: 'low' | 'medium' | 'high';
  impact: string;
}

export interface BundleAnalysis {
  chunks: Array<{ name: string; size: number }>;
  totalSize: number;
  recommendations: string[];
}

export interface MemoryUsage {
  used: number;
  total: number;
  percentage: number;
}

export interface RenderMetrics {
  renderTime: number;
  componentCount: number;
  reRenders: number;
}

export interface NetworkMetrics {
  requestCount: number;
  totalSize: number;
  averageTime: number;
}

// Stub implementations for missing modules
export const BundleAnalyzer = {
  analyze: (): Promise<BundleAnalysis> => Promise.resolve({
    chunks: [],
    totalSize: 0,
    recommendations: []
  }),
  getInstance: () => ({
    analyze: (): Promise<BundleAnalysis> => Promise.resolve({
      chunks: [],
      totalSize: 0,
      recommendations: []
    })
  })
};

export const CodeSplitter = {
  split: () => Promise.resolve(),
  getInstance: () => ({ split: () => Promise.resolve() })
};

export const LazyLoader = {
  load: () => Promise.resolve(),
  getInstance: () => ({ load: () => Promise.resolve() })
};

export const PerformanceProfiler = {
  profile: () => Promise.resolve(),
  getInstance: () => ({ profile: () => Promise.resolve() })
};

export const MemoryManager = {
  cleanup: () => Promise.resolve(),
  getInstance: () => ({ cleanup: () => Promise.resolve() })
};

export const CacheOptimizer = {
  optimize: () => Promise.resolve(),
  getInstance: () => ({ optimize: () => Promise.resolve() })
};

export const ResourceManager = {
  manage: () => Promise.resolve(),
  getInstance: () => ({ manage: () => Promise.resolve() })
};

export const VirtualizationManager = {
  virtualize: () => Promise.resolve(),
  getInstance: () => ({ virtualize: () => Promise.resolve() })
};

export const RenderOptimizer = {
  optimize: () => Promise.resolve(),
  getInstance: () => ({ optimize: () => Promise.resolve() })
};

export const ComponentProfiler = {
  profile: () => Promise.resolve(),
  getInstance: () => ({ profile: () => Promise.resolve() })
};
