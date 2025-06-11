/**
 * Advanced Performance Monitor
 * 
 * Comprehensive performance monitoring system with real-time metrics,
 * automated optimization suggestions, and detailed reporting.
 */

export interface PerformanceMetrics {
  // Core Web Vitals
  lcp: number; // Largest Contentful Paint
  fid: number; // First Input Delay
  cls: number; // Cumulative Layout Shift
  fcp: number; // First Contentful Paint
  ttfb: number; // Time to First Byte
  
  // Custom Metrics
  componentRenderTime: number;
  apiResponseTime: number;
  bundleSize: number;
  memoryUsage: number;
  
  // User Experience
  interactionLatency: number;
  scrollPerformance: number;
  animationFrameRate: number;
  
  // Network
  networkLatency: number;
  cacheHitRate: number;
  resourceLoadTime: number;
}

export interface PerformanceThresholds {
  lcp: { good: number; poor: number };
  fid: { good: number; poor: number };
  cls: { good: number; poor: number };
  componentRenderTime: { good: number; poor: number };
  apiResponseTime: { good: number; poor: number };
  memoryUsage: { good: number; poor: number };
}

export interface PerformanceReport {
  timestamp: Date;
  metrics: PerformanceMetrics;
  score: number;
  grade: 'A' | 'B' | 'C' | 'D' | 'F';
  issues: PerformanceIssue[];
  suggestions: OptimizationSuggestion[];
}

export interface PerformanceIssue {
  type: 'critical' | 'warning' | 'info';
  metric: keyof PerformanceMetrics;
  value: number;
  threshold: number;
  description: string;
  impact: 'high' | 'medium' | 'low';
}

export interface OptimizationSuggestion {
  category: 'bundle' | 'rendering' | 'network' | 'memory' | 'user-experience';
  priority: 'high' | 'medium' | 'low';
  title: string;
  description: string;
  implementation: string;
  estimatedImpact: string;
}

export class PerformanceMonitor {
  private metrics: Partial<PerformanceMetrics> = {};
  private observers: Map<string, PerformanceObserver> = new Map();
  private thresholds: PerformanceThresholds;
  private isMonitoring = false;
  private reportCallbacks: ((report: PerformanceReport) => void)[] = [];

  constructor(thresholds?: Partial<PerformanceThresholds>) {
    this.thresholds = {
      lcp: { good: 2500, poor: 4000 },
      fid: { good: 100, poor: 300 },
      cls: { good: 0.1, poor: 0.25 },
      componentRenderTime: { good: 16, poor: 50 },
      apiResponseTime: { good: 200, poor: 1000 },
      memoryUsage: { good: 50, poor: 100 },
      ...thresholds,
    };
  }

  start(): void {
    if (this.isMonitoring) return;
    
    this.isMonitoring = true;
    this.setupObservers();
    this.startCustomMetrics();
  }

  stop(): void {
    if (!this.isMonitoring) return;
    
    this.isMonitoring = false;
    this.observers.forEach(observer => observer.disconnect());
    this.observers.clear();
  }

  private setupObservers(): void {
    // Core Web Vitals Observer
    if ('PerformanceObserver' in window) {
      // LCP Observer
      const lcpObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        const lastEntry = entries[entries.length - 1];
        this.metrics.lcp = lastEntry.startTime;
      });
      lcpObserver.observe({ type: 'largest-contentful-paint', buffered: true });
      this.observers.set('lcp', lcpObserver);

      // FID Observer
      const fidObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry: any) => {
          this.metrics.fid = entry.processingStart - entry.startTime;
        });
      });
      fidObserver.observe({ type: 'first-input', buffered: true });
      this.observers.set('fid', fidObserver);

      // CLS Observer
      const clsObserver = new PerformanceObserver((list) => {
        let clsValue = 0;
        const entries = list.getEntries();
        entries.forEach((entry: any) => {
          if (!entry.hadRecentInput) {
            clsValue += entry.value;
          }
        });
        this.metrics.cls = clsValue;
      });
      clsObserver.observe({ type: 'layout-shift', buffered: true });
      this.observers.set('cls', clsObserver);

      // Navigation Observer
      const navigationObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry: any) => {
          this.metrics.fcp = entry.firstContentfulPaint;
          this.metrics.ttfb = entry.responseStart - entry.requestStart;
        });
      });
      navigationObserver.observe({ type: 'navigation', buffered: true });
      this.observers.set('navigation', navigationObserver);

      // Resource Observer
      const resourceObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        let totalLoadTime = 0;
        entries.forEach((entry: any) => {
          totalLoadTime += entry.duration;
        });
        this.metrics.resourceLoadTime = totalLoadTime / entries.length;
      });
      resourceObserver.observe({ type: 'resource', buffered: true });
      this.observers.set('resource', resourceObserver);
    }
  }

  private startCustomMetrics(): void {
    // Memory Usage Monitoring
    if ('memory' in performance) {
      setInterval(() => {
        const memory = (performance as any).memory;
        this.metrics.memoryUsage = memory.usedJSHeapSize / 1024 / 1024; // MB
      }, 5000);
    }

    // Animation Frame Rate Monitoring
    let frameCount = 0;
    let lastTime = performance.now();
    
    const measureFrameRate = () => {
      frameCount++;
      const currentTime = performance.now();
      
      if (currentTime - lastTime >= 1000) {
        this.metrics.animationFrameRate = frameCount;
        frameCount = 0;
        lastTime = currentTime;
      }
      
      if (this.isMonitoring) {
        requestAnimationFrame(measureFrameRate);
      }
    };
    
    requestAnimationFrame(measureFrameRate);

    // Network Latency Monitoring
    this.measureNetworkLatency();
  }

  private async measureNetworkLatency(): Promise<void> {
    try {
      const start = performance.now();
      await fetch('/api/ping', { method: 'HEAD' });
      const end = performance.now();
      this.metrics.networkLatency = end - start;
    } catch (error) {
      console.warn('Network latency measurement failed:', error);
    }
  }

  measureComponentRender<T>(
    componentName: string,
    renderFunction: () => T
  ): T {
    const start = performance.now();
    const result = renderFunction();
    const end = performance.now();
    
    this.metrics.componentRenderTime = end - start;
    
    // Mark for performance timeline
    performance.mark(`${componentName}-render-start`);
    performance.mark(`${componentName}-render-end`);
    performance.measure(
      `${componentName}-render`,
      `${componentName}-render-start`,
      `${componentName}-render-end`
    );
    
    return result;
  }

  measureApiCall<T>(
    endpoint: string,
    apiCall: () => Promise<T>
  ): Promise<T> {
    const start = performance.now();
    
    return apiCall().then(result => {
      const end = performance.now();
      this.metrics.apiResponseTime = end - start;
      
      // Mark for performance timeline
      performance.mark(`api-${endpoint}-start`);
      performance.mark(`api-${endpoint}-end`);
      performance.measure(
        `api-${endpoint}`,
        `api-${endpoint}-start`,
        `api-${endpoint}-end`
      );
      
      return result;
    });
  }

  generateReport(): PerformanceReport {
    const issues = this.analyzeIssues();
    const suggestions = this.generateSuggestions(issues);
    const score = this.calculateScore();
    const grade = this.calculateGrade(score);

    return {
      timestamp: new Date(),
      metrics: this.metrics as PerformanceMetrics,
      score,
      grade,
      issues,
      suggestions,
    };
  }

  private analyzeIssues(): PerformanceIssue[] {
    const issues: PerformanceIssue[] = [];

    Object.entries(this.metrics).forEach(([metric, value]) => {
      const threshold = this.thresholds[metric as keyof PerformanceThresholds];
      if (threshold && value !== undefined) {
        if (value > threshold.poor) {
          issues.push({
            type: 'critical',
            metric: metric as keyof PerformanceMetrics,
            value,
            threshold: threshold.poor,
            description: `${metric} is critically slow`,
            impact: 'high',
          });
        } else if (value > threshold.good) {
          issues.push({
            type: 'warning',
            metric: metric as keyof PerformanceMetrics,
            value,
            threshold: threshold.good,
            description: `${metric} needs improvement`,
            impact: 'medium',
          });
        }
      }
    });

    return issues;
  }

  private generateSuggestions(issues: PerformanceIssue[]): OptimizationSuggestion[] {
    const suggestions: OptimizationSuggestion[] = [];

    issues.forEach(issue => {
      switch (issue.metric) {
        case 'lcp':
          suggestions.push({
            category: 'rendering',
            priority: 'high',
            title: 'Optimize Largest Contentful Paint',
            description: 'Reduce LCP by optimizing critical resources and server response times',
            implementation: 'Implement resource preloading, optimize images, and reduce server response time',
            estimatedImpact: 'High - Improves perceived loading performance',
          });
          break;
        
        case 'componentRenderTime':
          suggestions.push({
            category: 'rendering',
            priority: 'medium',
            title: 'Optimize Component Rendering',
            description: 'Reduce component render time with memoization and virtualization',
            implementation: 'Use React.memo, useMemo, and virtual scrolling for large lists',
            estimatedImpact: 'Medium - Improves UI responsiveness',
          });
          break;
        
        case 'memoryUsage':
          suggestions.push({
            category: 'memory',
            priority: 'high',
            title: 'Optimize Memory Usage',
            description: 'Reduce memory consumption to prevent performance degradation',
            implementation: 'Implement proper cleanup, reduce bundle size, and optimize data structures',
            estimatedImpact: 'High - Prevents memory leaks and crashes',
          });
          break;
      }
    });

    return suggestions;
  }

  private calculateScore(): number {
    const weights = {
      lcp: 0.25,
      fid: 0.25,
      cls: 0.25,
      componentRenderTime: 0.1,
      apiResponseTime: 0.1,
      memoryUsage: 0.05,
    };

    let totalScore = 0;
    let totalWeight = 0;

    Object.entries(weights).forEach(([metric, weight]) => {
      const value = this.metrics[metric as keyof PerformanceMetrics];
      const threshold = this.thresholds[metric as keyof PerformanceThresholds];
      
      if (value !== undefined && threshold) {
        let score = 100;
        if (value > threshold.good) {
          score = Math.max(0, 100 - ((value - threshold.good) / (threshold.poor - threshold.good)) * 50);
        }
        if (value > threshold.poor) {
          score = Math.max(0, 50 - ((value - threshold.poor) / threshold.poor) * 50);
        }
        
        totalScore += score * weight;
        totalWeight += weight;
      }
    });

    return totalWeight > 0 ? totalScore / totalWeight : 0;
  }

  private calculateGrade(score: number): 'A' | 'B' | 'C' | 'D' | 'F' {
    if (score >= 90) return 'A';
    if (score >= 80) return 'B';
    if (score >= 70) return 'C';
    if (score >= 60) return 'D';
    return 'F';
  }

  onReport(callback: (report: PerformanceReport) => void): void {
    this.reportCallbacks.push(callback);
  }

  getMetrics(): Partial<PerformanceMetrics> {
    return { ...this.metrics };
  }

  clearMetrics(): void {
    this.metrics = {};
  }
}
