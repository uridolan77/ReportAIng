/**
 * Analytics System
 * 
 * Comprehensive monitoring and analytics system for React applications
 * including user behavior tracking, performance monitoring, and business intelligence
 */

// Analytics interfaces
interface UserEvent {
  id: string;
  userId?: string;
  sessionId: string;
  timestamp: number;
  type: 'page_view' | 'click' | 'form_submit' | 'query_execute' | 'error' | 'custom';
  category: string;
  action: string;
  label?: string;
  value?: number;
  metadata?: Record<string, any>;
}

interface PerformanceMetric {
  id: string;
  timestamp: number;
  type: 'navigation' | 'resource' | 'paint' | 'layout' | 'custom';
  name: string;
  value: number;
  unit: 'ms' | 'bytes' | 'count' | 'percentage';
  metadata?: Record<string, any>;
}

interface BusinessMetric {
  id: string;
  timestamp: number;
  metric: string;
  value: number;
  dimensions: Record<string, string>;
  tags?: string[];
}

interface AnalyticsConfig {
  enabled: boolean;
  trackingId?: string;
  apiEndpoint?: string;
  batchSize: number;
  flushInterval: number;
  enableUserTracking: boolean;
  enablePerformanceTracking: boolean;
  enableErrorTracking: boolean;
  enableBusinessMetrics: boolean;
  samplingRate: number;
}

interface AnalyticsReport {
  timeRange: { start: number; end: number };
  userEvents: UserEvent[];
  performanceMetrics: PerformanceMetric[];
  businessMetrics: BusinessMetric[];
  summary: {
    totalUsers: number;
    totalSessions: number;
    totalPageViews: number;
    averageSessionDuration: number;
    bounceRate: number;
    conversionRate: number;
    errorRate: number;
    averageLoadTime: number;
  };
}

class AnalyticsSystem {
  private static instance: AnalyticsSystem;
  private config: AnalyticsConfig;
  private eventQueue: UserEvent[] = [];
  private performanceQueue: PerformanceMetric[] = [];
  private businessQueue: BusinessMetric[] = [];
  private sessionId: string;
  private userId?: string;
  private flushTimer?: NodeJS.Timeout;

  private constructor() {
    this.config = {
      enabled: true,
      batchSize: 50,
      flushInterval: 30000, // 30 seconds
      enableUserTracking: true,
      enablePerformanceTracking: true,
      enableErrorTracking: true,
      enableBusinessMetrics: true,
      samplingRate: 1.0
    };
    this.sessionId = this.generateSessionId();
    this.initializeTracking();
  }

  static getInstance(): AnalyticsSystem {
    if (!AnalyticsSystem.instance) {
      AnalyticsSystem.instance = new AnalyticsSystem();
    }
    return AnalyticsSystem.instance;
  }

  // Configuration
  configure(config: Partial<AnalyticsConfig>): void {
    this.config = { ...this.config, ...config };
    if (this.config.enabled) {
      this.startFlushTimer();
    } else {
      this.stopFlushTimer();
    }
  }

  setUserId(userId: string): void {
    this.userId = userId;
  }

  // Event tracking
  trackEvent(
    type: UserEvent['type'],
    category: string,
    action: string,
    label?: string,
    value?: number,
    metadata?: Record<string, any>
  ): void {
    if (!this.config.enabled || !this.config.enableUserTracking) return;
    if (Math.random() > this.config.samplingRate) return;

    const event: UserEvent = {
      id: this.generateId(),
      userId: this.userId,
      sessionId: this.sessionId,
      timestamp: Date.now(),
      type,
      category,
      action,
      label,
      value,
      metadata
    };

    this.eventQueue.push(event);
    this.checkFlushConditions();
  }

  // Page view tracking
  trackPageView(path: string, title?: string, metadata?: Record<string, any>): void {
    this.trackEvent('page_view', 'navigation', 'page_view', path, undefined, {
      title,
      url: window.location.href,
      referrer: document.referrer,
      ...metadata
    });
  }

  // Click tracking
  trackClick(element: string, location?: string, metadata?: Record<string, any>): void {
    this.trackEvent('click', 'interaction', 'click', element, undefined, {
      location,
      timestamp: Date.now(),
      ...metadata
    });
  }

  // Form submission tracking
  trackFormSubmit(formName: string, success: boolean, metadata?: Record<string, any>): void {
    this.trackEvent('form_submit', 'form', success ? 'submit_success' : 'submit_error', formName, undefined, metadata);
  }

  // Query execution tracking
  trackQueryExecution(
    queryType: string,
    executionTime: number,
    success: boolean,
    metadata?: Record<string, any>
  ): void {
    this.trackEvent('query_execute', 'query', success ? 'execute_success' : 'execute_error', queryType, executionTime, metadata);
  }

  // Error tracking
  trackError(error: Error, context?: string, metadata?: Record<string, any>): void {
    if (!this.config.enableErrorTracking) return;

    this.trackEvent('error', 'error', 'javascript_error', context, undefined, {
      message: error.message,
      stack: error.stack,
      name: error.name,
      url: window.location.href,
      userAgent: navigator.userAgent,
      ...metadata
    });
  }

  // Performance tracking
  trackPerformanceMetric(
    type: PerformanceMetric['type'],
    name: string,
    value: number,
    unit: PerformanceMetric['unit'],
    metadata?: Record<string, any>
  ): void {
    if (!this.config.enabled || !this.config.enablePerformanceTracking) return;

    const metric: PerformanceMetric = {
      id: this.generateId(),
      timestamp: Date.now(),
      type,
      name,
      value,
      unit,
      metadata
    };

    this.performanceQueue.push(metric);
    this.checkFlushConditions();
  }

  // Business metrics tracking
  trackBusinessMetric(
    metric: string,
    value: number,
    dimensions: Record<string, string>,
    tags?: string[]
  ): void {
    if (!this.config.enabled || !this.config.enableBusinessMetrics) return;

    const businessMetric: BusinessMetric = {
      id: this.generateId(),
      timestamp: Date.now(),
      metric,
      value,
      dimensions,
      tags
    };

    this.businessQueue.push(businessMetric);
    this.checkFlushConditions();
  }

  // Automatic performance tracking
  private initializeTracking(): void {
    if (typeof window === 'undefined') return;

    // Track page load performance
    window.addEventListener('load', () => {
      setTimeout(() => {
        this.trackNavigationTiming();
        this.trackResourceTiming();
        this.trackPaintTiming();
      }, 0);
    });

    // Track errors
    window.addEventListener('error', (event) => {
      this.trackError(event.error, 'global_error_handler', {
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno
      });
    });

    // Track unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      this.trackError(new Error(event.reason), 'unhandled_promise_rejection');
    });

    // Track visibility changes
    document.addEventListener('visibilitychange', () => {
      this.trackEvent('custom', 'engagement', 'visibility_change', document.visibilityState);
    });
  }

  private trackNavigationTiming(): void {
    if (!('performance' in window) || !performance.getEntriesByType) return;

    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    if (!navigation) return;

    this.trackPerformanceMetric('navigation', 'dom_content_loaded', navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart, 'ms');
    this.trackPerformanceMetric('navigation', 'load_complete', navigation.loadEventEnd - navigation.loadEventStart, 'ms');
    this.trackPerformanceMetric('navigation', 'dns_lookup', navigation.domainLookupEnd - navigation.domainLookupStart, 'ms');
    this.trackPerformanceMetric('navigation', 'tcp_connect', navigation.connectEnd - navigation.connectStart, 'ms');
    this.trackPerformanceMetric('navigation', 'request_response', navigation.responseEnd - navigation.requestStart, 'ms');
  }

  private trackResourceTiming(): void {
    if (!('performance' in window) || !performance.getEntriesByType) return;

    const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
    resources.forEach(resource => {
      this.trackPerformanceMetric('resource', 'resource_load', resource.duration, 'ms', {
        name: resource.name,
        type: resource.initiatorType,
        size: resource.transferSize
      });
    });
  }

  private trackPaintTiming(): void {
    if (!('performance' in window) || !performance.getEntriesByType) return;

    const paints = performance.getEntriesByType('paint');
    paints.forEach(paint => {
      this.trackPerformanceMetric('paint', paint.name, paint.startTime, 'ms');
    });
  }

  // Data flushing
  private checkFlushConditions(): void {
    const totalEvents = this.eventQueue.length + this.performanceQueue.length + this.businessQueue.length;
    if (totalEvents >= this.config.batchSize) {
      this.flush();
    }
  }

  private startFlushTimer(): void {
    this.stopFlushTimer();
    this.flushTimer = setInterval(() => {
      this.flush();
    }, this.config.flushInterval);
  }

  private stopFlushTimer(): void {
    if (this.flushTimer) {
      clearInterval(this.flushTimer);
      this.flushTimer = undefined;
    }
  }

  async flush(): Promise<void> {
    if (this.eventQueue.length === 0 && this.performanceQueue.length === 0 && this.businessQueue.length === 0) {
      return;
    }

    const payload = {
      sessionId: this.sessionId,
      userId: this.userId,
      timestamp: Date.now(),
      events: [...this.eventQueue],
      performance: [...this.performanceQueue],
      business: [...this.businessQueue]
    };

    // Clear queues
    this.eventQueue = [];
    this.performanceQueue = [];
    this.businessQueue = [];

    try {
      if (this.config.apiEndpoint) {
        await fetch(this.config.apiEndpoint, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(payload)
        });
      } else {
        // Log to console in development
        console.log('Analytics payload:', payload);
      }
    } catch (error) {
      console.error('Failed to send analytics data:', error);
      // Re-queue events on failure
      this.eventQueue.unshift(...payload.events);
      this.performanceQueue.unshift(...payload.performance);
      this.businessQueue.unshift(...payload.business);
    }
  }

  // Reporting
  generateReport(timeRange: { start: number; end: number }): AnalyticsReport {
    // This would typically fetch data from the analytics backend
    // For demo purposes, we'll return mock data
    const mockEvents: UserEvent[] = [];
    const mockPerformance: PerformanceMetric[] = [];
    const mockBusiness: BusinessMetric[] = [];

    return {
      timeRange,
      userEvents: mockEvents,
      performanceMetrics: mockPerformance,
      businessMetrics: mockBusiness,
      summary: {
        totalUsers: 1250,
        totalSessions: 3420,
        totalPageViews: 8750,
        averageSessionDuration: 420000, // 7 minutes
        bounceRate: 0.35,
        conversionRate: 0.12,
        errorRate: 0.02,
        averageLoadTime: 1200 // 1.2 seconds
      }
    };
  }

  // Utility methods
  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateSessionId(): string {
    return `session-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  // Cleanup
  destroy(): void {
    this.stopFlushTimer();
    this.flush();
  }
}

// Export singleton instance
export const analytics = AnalyticsSystem.getInstance();
export default AnalyticsSystem;
