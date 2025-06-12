/**
 * Performance Optimizer
 * 
 * Comprehensive performance optimization utilities for React applications
 * including bundle analysis, memory management, and rendering optimization.
 */

export interface PerformanceMetrics {
  renderTime: number;
  bundleSize: number;
  memoryUsage: number;
  networkRequests: number;
  cacheHitRate: number;
  componentCount: number;
  rerenderCount: number;
  timestamp: number;
}

export interface OptimizationSuggestion {
  type: 'bundle' | 'memory' | 'render' | 'network' | 'cache';
  severity: 'low' | 'medium' | 'high' | 'critical';
  message: string;
  impact: string;
  solution: string;
}

export class PerformanceOptimizer {
  private static instance: PerformanceOptimizer;
  private metrics: PerformanceMetrics[] = [];
  private observers: PerformanceObserver[] = [];
  private memoryMonitorInterval?: NodeJS.Timeout;

  static getInstance(): PerformanceOptimizer {
    if (!PerformanceOptimizer.instance) {
      PerformanceOptimizer.instance = new PerformanceOptimizer();
    }
    return PerformanceOptimizer.instance;
  }

  initialize(): void {
    this.setupPerformanceObservers();
    this.startMemoryMonitoring();
    this.setupBundleAnalysis();
  }

  private setupPerformanceObservers(): void {
    if ('PerformanceObserver' in window) {
      // Monitor navigation timing
      const navObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry) => {
          if (entry.entryType === 'navigation') {
            this.recordMetric({
              renderTime: entry.duration,
              bundleSize: this.getBundleSize(),
              memoryUsage: this.getMemoryUsage(),
              networkRequests: performance.getEntriesByType('resource').length,
              cacheHitRate: this.calculateCacheHitRate(),
              componentCount: this.getComponentCount(),
              rerenderCount: 0,
              timestamp: Date.now(),
            });
          }
        });
      });

      navObserver.observe({ entryTypes: ['navigation'] });
      this.observers.push(navObserver);

      // Monitor resource loading
      const resourceObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry) => {
          if (entry.transferSize === 0 && entry.decodedBodySize > 0) {
            // Resource was served from cache
            this.updateCacheMetrics(true);
          } else {
            this.updateCacheMetrics(false);
          }
        });
      });

      resourceObserver.observe({ entryTypes: ['resource'] });
      this.observers.push(resourceObserver);
    }
  }

  private startMemoryMonitoring(): void {
    this.memoryMonitorInterval = setInterval(() => {
      const memoryUsage = this.getMemoryUsage();
      if (memoryUsage > 50) { // MB threshold
        console.warn('High memory usage detected:', memoryUsage, 'MB');
        this.suggestMemoryOptimizations();
      }
    }, 30000); // Check every 30 seconds
  }

  private setupBundleAnalysis(): void {
    if (process.env.NODE_ENV === 'development') {
      // Analyze bundle size and suggest optimizations
      this.analyzeBundleSize();
    }
  }

  private getBundleSize(): number {
    // Estimate bundle size from loaded resources
    const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
    return resources.reduce((total, resource) => {
      if (resource.name.includes('.js') || resource.name.includes('.css')) {
        return total + (resource.transferSize || 0);
      }
      return total;
    }, 0) / 1024; // Convert to KB
  }

  private getMemoryUsage(): number {
    if ('memory' in performance) {
      const memory = (performance as any).memory;
      return memory.usedJSHeapSize / 1024 / 1024; // Convert to MB
    }
    return 0;
  }

  private calculateCacheHitRate(): number {
    const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
    const cachedResources = resources.filter(r => r.transferSize === 0 && r.decodedBodySize > 0);
    return resources.length > 0 ? (cachedResources.length / resources.length) * 100 : 0;
  }

  private getComponentCount(): number {
    // Estimate component count from DOM elements with React attributes
    const reactElements = document.querySelectorAll('[data-reactroot], [data-react-*]');
    return reactElements.length;
  }

  private updateCacheMetrics(isHit: boolean): void {
    // Update cache hit rate metrics
    const lastMetric = this.metrics[this.metrics.length - 1];
    if (lastMetric) {
      // Update cache hit rate calculation
    }
  }

  recordMetric(metric: PerformanceMetrics): void {
    this.metrics.push(metric);
    
    // Keep only last 100 metrics
    if (this.metrics.length > 100) {
      this.metrics = this.metrics.slice(-100);
    }

    // Analyze and suggest optimizations
    this.analyzePerformance(metric);
  }

  private analyzePerformance(metric: PerformanceMetrics): void {
    const suggestions = this.generateOptimizationSuggestions(metric);
    if (suggestions.length > 0 && process.env.NODE_ENV === 'development') {
      console.group('Performance Optimization Suggestions');
      suggestions.forEach(suggestion => {
        console.warn(`[${suggestion.severity.toUpperCase()}] ${suggestion.message}`);
        console.info(`Impact: ${suggestion.impact}`);
        console.info(`Solution: ${suggestion.solution}`);
      });
      console.groupEnd();
    }
  }

  generateOptimizationSuggestions(metric: PerformanceMetrics): OptimizationSuggestion[] {
    const suggestions: OptimizationSuggestion[] = [];

    // Bundle size analysis
    if (metric.bundleSize > 1000) { // > 1MB
      suggestions.push({
        type: 'bundle',
        severity: 'high',
        message: 'Large bundle size detected',
        impact: 'Slower initial page load',
        solution: 'Implement code splitting and lazy loading for non-critical components'
      });
    }

    // Memory usage analysis
    if (metric.memoryUsage > 100) { // > 100MB
      suggestions.push({
        type: 'memory',
        severity: 'critical',
        message: 'High memory usage detected',
        impact: 'Potential memory leaks and poor performance',
        solution: 'Check for memory leaks, optimize component lifecycle, use React.memo'
      });
    }

    // Render time analysis
    if (metric.renderTime > 1000) { // > 1 second
      suggestions.push({
        type: 'render',
        severity: 'medium',
        message: 'Slow render time detected',
        impact: 'Poor user experience',
        solution: 'Optimize component rendering, use virtualization for large lists'
      });
    }

    // Cache hit rate analysis
    if (metric.cacheHitRate < 50) { // < 50%
      suggestions.push({
        type: 'cache',
        severity: 'medium',
        message: 'Low cache hit rate',
        impact: 'Increased network requests and slower loading',
        solution: 'Implement better caching strategies, use service workers'
      });
    }

    return suggestions;
  }

  private suggestMemoryOptimizations(): void {
    console.group('Memory Optimization Suggestions');
    console.warn('Consider the following optimizations:');
    console.info('1. Use React.memo for expensive components');
    console.info('2. Implement proper cleanup in useEffect hooks');
    console.info('3. Use virtualization for large lists');
    console.info('4. Optimize image loading and caching');
    console.info('5. Remove unused dependencies and code');
    console.groupEnd();
  }

  private analyzeBundleSize(): void {
    // Analyze bundle composition and suggest optimizations
    const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
    const jsResources = resources.filter(r => r.name.includes('.js'));
    const cssResources = resources.filter(r => r.name.includes('.css'));

    console.group('Bundle Analysis');
    console.info(`JavaScript files: ${jsResources.length}`);
    console.info(`CSS files: ${cssResources.length}`);
    console.info(`Total JS size: ${jsResources.reduce((total, r) => total + (r.transferSize || 0), 0) / 1024} KB`);
    console.info(`Total CSS size: ${cssResources.reduce((total, r) => total + (r.transferSize || 0), 0) / 1024} KB`);
    console.groupEnd();
  }

  getMetrics(): PerformanceMetrics[] {
    return [...this.metrics];
  }

  getLatestMetric(): PerformanceMetrics | null {
    return this.metrics.length > 0 ? this.metrics[this.metrics.length - 1] : null;
  }

  cleanup(): void {
    this.observers.forEach(observer => observer.disconnect());
    this.observers = [];
    
    if (this.memoryMonitorInterval) {
      clearInterval(this.memoryMonitorInterval);
    }
  }
}

// Export singleton instance
export const performanceOptimizer = PerformanceOptimizer.getInstance();
