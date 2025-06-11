/**
 * Performance Utilities
 * 
 * Advanced performance optimization utilities including bundle analysis,
 * code splitting, lazy loading, and performance monitoring.
 */

// Bundle Optimization
export { BundleAnalyzer } from './BundleAnalyzer';
export { CodeSplitter } from './CodeSplitter';
export { LazyLoader } from './LazyLoader';

// Performance Monitoring
export { PerformanceMonitor } from './PerformanceMonitor';
export { MetricsCollector } from './MetricsCollector';
export { PerformanceProfiler } from './PerformanceProfiler';

// Memory Management
export { MemoryManager } from './MemoryManager';
export { CacheOptimizer } from './CacheOptimizer';
export { ResourceManager } from './ResourceManager';

// Rendering Optimization
export { VirtualizationManager } from './VirtualizationManager';
export { RenderOptimizer } from './RenderOptimizer';
export { ComponentProfiler } from './ComponentProfiler';

// Network Optimization
export { RequestOptimizer } from './RequestOptimizer';
export { PreloadManager } from './PreloadManager';
export { ServiceWorkerManager } from './ServiceWorkerManager';

// Utilities
export { performanceUtils } from './utils';
export { benchmarkUtils } from './benchmark';
export { optimizationUtils } from './optimization';

// Types
export type * from './types';
