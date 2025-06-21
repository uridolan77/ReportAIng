/**
 * Performance Monitor
 * 
 * Comprehensive performance monitoring including:
 * - Web Vitals tracking (CLS, FID, FCP, LCP, TTFB)
 * - Custom performance metrics
 * - Error tracking and reporting
 * - User interaction analytics
 * - Resource loading performance
 */

export interface PerformanceMetric {
  name: string
  value: number
  unit: string
  timestamp: number
  rating: 'good' | 'needs-improvement' | 'poor'
  metadata?: Record<string, any>
}

export interface ErrorReport {
  id: string
  message: string
  stack?: string
  filename?: string
  lineno?: number
  colno?: number
  timestamp: number
  userAgent: string
  url: string
  userId?: string
  sessionId: string
  severity: 'low' | 'medium' | 'high' | 'critical'
  context?: Record<string, any>
}

export interface UserInteraction {
  type: 'click' | 'scroll' | 'navigation' | 'form' | 'search'
  target: string
  timestamp: number
  duration?: number
  metadata?: Record<string, any>
}

export interface PerformanceReport {
  sessionId: string
  timestamp: number
  metrics: PerformanceMetric[]
  errors: ErrorReport[]
  interactions: UserInteraction[]
  deviceInfo: {
    userAgent: string
    screen: { width: number; height: number }
    connection?: string
    memory?: number
  }
  pageInfo: {
    url: string
    referrer: string
    loadTime: number
    domContentLoaded: number
  }
}

class PerformanceMonitor {
  private metrics: PerformanceMetric[] = []
  private errors: ErrorReport[] = []
  private interactions: UserInteraction[] = []
  private sessionId: string
  private reportingEndpoint?: string
  private reportingInterval: number = 30000 // 30 seconds
  private maxMetrics: number = 1000
  private isEnabled: boolean = true

  constructor(config?: {
    reportingEndpoint?: string
    reportingInterval?: number
    maxMetrics?: number
    enabled?: boolean
  }) {
    this.sessionId = this.generateSessionId()
    this.reportingEndpoint = config?.reportingEndpoint
    this.reportingInterval = config?.reportingInterval || 30000
    this.maxMetrics = config?.maxMetrics || 1000
    this.isEnabled = config?.enabled !== false

    if (this.isEnabled) {
      this.initialize()
    }
  }

  private generateSessionId(): string {
    return `session-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
  }

  private initialize() {
    this.setupWebVitals()
    this.setupErrorTracking()
    this.setupInteractionTracking()
    this.setupResourceTracking()
    this.setupNavigationTracking()
    this.startReporting()
  }

  private setupWebVitals() {
    // Cumulative Layout Shift (CLS)
    this.observeLayoutShift()
    
    // First Input Delay (FID)
    this.observeFirstInput()
    
    // Largest Contentful Paint (LCP)
    this.observeLargestContentfulPaint()
    
    // First Contentful Paint (FCP)
    this.observeFirstContentfulPaint()
    
    // Time to First Byte (TTFB)
    this.measureTTFB()
  }

  private observeLayoutShift() {
    if ('PerformanceObserver' in window) {
      const observer = new PerformanceObserver((list) => {
        let clsValue = 0
        for (const entry of list.getEntries()) {
          if (!(entry as any).hadRecentInput) {
            clsValue += (entry as any).value
          }
        }
        
        if (clsValue > 0) {
          this.addMetric({
            name: 'CLS',
            value: clsValue,
            unit: 'score',
            timestamp: Date.now(),
            rating: clsValue <= 0.1 ? 'good' : clsValue <= 0.25 ? 'needs-improvement' : 'poor',
          })
        }
      })
      
      observer.observe({ entryTypes: ['layout-shift'] })
    }
  }

  private observeFirstInput() {
    if ('PerformanceObserver' in window) {
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          const fid = (entry as any).processingStart - entry.startTime
          this.addMetric({
            name: 'FID',
            value: fid,
            unit: 'ms',
            timestamp: Date.now(),
            rating: fid <= 100 ? 'good' : fid <= 300 ? 'needs-improvement' : 'poor',
          })
        }
      })
      
      observer.observe({ entryTypes: ['first-input'] })
    }
  }

  private observeLargestContentfulPaint() {
    if ('PerformanceObserver' in window) {
      const observer = new PerformanceObserver((list) => {
        const entries = list.getEntries()
        const lastEntry = entries[entries.length - 1]
        const lcp = lastEntry.startTime
        
        this.addMetric({
          name: 'LCP',
          value: lcp,
          unit: 'ms',
          timestamp: Date.now(),
          rating: lcp <= 2500 ? 'good' : lcp <= 4000 ? 'needs-improvement' : 'poor',
        })
      })
      
      observer.observe({ entryTypes: ['largest-contentful-paint'] })
    }
  }

  private observeFirstContentfulPaint() {
    if ('PerformanceObserver' in window) {
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          if (entry.name === 'first-contentful-paint') {
            this.addMetric({
              name: 'FCP',
              value: entry.startTime,
              unit: 'ms',
              timestamp: Date.now(),
              rating: entry.startTime <= 1800 ? 'good' : entry.startTime <= 3000 ? 'needs-improvement' : 'poor',
            })
          }
        }
      })
      
      observer.observe({ entryTypes: ['paint'] })
    }
  }

  private measureTTFB() {
    if ('performance' in window && 'getEntriesByType' in performance) {
      const navigationEntries = performance.getEntriesByType('navigation') as PerformanceNavigationTiming[]
      if (navigationEntries.length > 0) {
        const ttfb = navigationEntries[0].responseStart - navigationEntries[0].requestStart
        
        this.addMetric({
          name: 'TTFB',
          value: ttfb,
          unit: 'ms',
          timestamp: Date.now(),
          rating: ttfb <= 800 ? 'good' : ttfb <= 1800 ? 'needs-improvement' : 'poor',
        })
      }
    }
  }

  private setupErrorTracking() {
    // JavaScript errors
    window.addEventListener('error', (event) => {
      this.addError({
        id: this.generateErrorId(),
        message: event.message,
        stack: event.error?.stack,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
        timestamp: Date.now(),
        userAgent: navigator.userAgent,
        url: window.location.href,
        sessionId: this.sessionId,
        severity: this.determineSeverity(event.message),
      })
    })

    // Unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      this.addError({
        id: this.generateErrorId(),
        message: `Unhandled Promise Rejection: ${event.reason}`,
        stack: event.reason?.stack,
        timestamp: Date.now(),
        userAgent: navigator.userAgent,
        url: window.location.href,
        sessionId: this.sessionId,
        severity: 'medium',
      })
    })
  }

  private setupInteractionTracking() {
    // Click tracking
    document.addEventListener('click', (event) => {
      const target = this.getElementSelector(event.target as Element)
      this.addInteraction({
        type: 'click',
        target,
        timestamp: Date.now(),
        metadata: {
          x: event.clientX,
          y: event.clientY,
          button: event.button,
        },
      })
    })

    // Scroll tracking (throttled)
    let scrollTimeout: NodeJS.Timeout
    document.addEventListener('scroll', () => {
      clearTimeout(scrollTimeout)
      scrollTimeout = setTimeout(() => {
        this.addInteraction({
          type: 'scroll',
          target: 'window',
          timestamp: Date.now(),
          metadata: {
            scrollY: window.scrollY,
            scrollX: window.scrollX,
          },
        })
      }, 100)
    })
  }

  private setupResourceTracking() {
    if ('PerformanceObserver' in window) {
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          const resource = entry as PerformanceResourceTiming
          
          this.addMetric({
            name: 'Resource Load Time',
            value: resource.responseEnd - resource.startTime,
            unit: 'ms',
            timestamp: Date.now(),
            rating: this.rateResourceLoadTime(resource.responseEnd - resource.startTime),
            metadata: {
              name: resource.name,
              type: this.getResourceType(resource.name),
              size: resource.transferSize,
            },
          })
        }
      })
      
      observer.observe({ entryTypes: ['resource'] })
    }
  }

  private setupNavigationTracking() {
    // Page visibility changes
    document.addEventListener('visibilitychange', () => {
      this.addInteraction({
        type: 'navigation',
        target: 'page',
        timestamp: Date.now(),
        metadata: {
          visibilityState: document.visibilityState,
        },
      })
    })
  }

  private startReporting() {
    if (this.reportingEndpoint) {
      setInterval(() => {
        this.sendReport()
      }, this.reportingInterval)

      // Send report on page unload
      window.addEventListener('beforeunload', () => {
        this.sendReport(true)
      })
    }
  }

  private addMetric(metric: PerformanceMetric) {
    this.metrics.push(metric)
    
    // Keep only recent metrics
    if (this.metrics.length > this.maxMetrics) {
      this.metrics = this.metrics.slice(-this.maxMetrics)
    }

    // Log critical performance issues
    if (metric.rating === 'poor') {
      console.warn(`[Performance] Poor ${metric.name}: ${metric.value}${metric.unit}`)
    }
  }

  private addError(error: ErrorReport) {
    this.errors.push(error)
    console.error('[Performance] Error tracked:', error)
  }

  private addInteraction(interaction: UserInteraction) {
    this.interactions.push(interaction)
    
    // Keep only recent interactions
    if (this.interactions.length > 1000) {
      this.interactions = this.interactions.slice(-1000)
    }
  }

  private generateErrorId(): string {
    return `error-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
  }

  private determineSeverity(message: string): 'low' | 'medium' | 'high' | 'critical' {
    if (message.includes('Script error') || message.includes('Network error')) {
      return 'low'
    }
    if (message.includes('TypeError') || message.includes('ReferenceError')) {
      return 'medium'
    }
    if (message.includes('SecurityError') || message.includes('ChunkLoadError')) {
      return 'high'
    }
    return 'medium'
  }

  private getElementSelector(element: Element): string {
    if (element.id) return `#${element.id}`
    if (element.className) return `.${element.className.split(' ')[0]}`
    return element.tagName.toLowerCase()
  }

  private rateResourceLoadTime(time: number): 'good' | 'needs-improvement' | 'poor' {
    if (time <= 1000) return 'good'
    if (time <= 3000) return 'needs-improvement'
    return 'poor'
  }

  private getResourceType(url: string): string {
    if (url.match(/\.(js)$/)) return 'script'
    if (url.match(/\.(css)$/)) return 'stylesheet'
    if (url.match(/\.(png|jpg|jpeg|gif|svg|webp)$/)) return 'image'
    if (url.match(/\.(woff|woff2|ttf|eot)$/)) return 'font'
    return 'other'
  }

  private async sendReport(isBeacon: boolean = false) {
    if (!this.reportingEndpoint || this.metrics.length === 0) return

    const report: PerformanceReport = {
      sessionId: this.sessionId,
      timestamp: Date.now(),
      metrics: [...this.metrics],
      errors: [...this.errors],
      interactions: [...this.interactions],
      deviceInfo: {
        userAgent: navigator.userAgent,
        screen: {
          width: screen.width,
          height: screen.height,
        },
        connection: (navigator as any).connection?.effectiveType,
        memory: (performance as any).memory?.usedJSHeapSize,
      },
      pageInfo: {
        url: window.location.href,
        referrer: document.referrer,
        loadTime: performance.now(),
        domContentLoaded: performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart,
      },
    }

    try {
      if (isBeacon && 'sendBeacon' in navigator) {
        navigator.sendBeacon(this.reportingEndpoint, JSON.stringify(report))
      } else {
        await fetch(this.reportingEndpoint, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(report),
        })
      }

      // Clear sent data
      this.metrics = []
      this.errors = []
      this.interactions = []
    } catch (error) {
      console.error('[Performance] Failed to send report:', error)
    }
  }

  // Public API
  public trackCustomMetric(name: string, value: number, unit: string = 'ms') {
    this.addMetric({
      name,
      value,
      unit,
      timestamp: Date.now(),
      rating: 'good', // Default rating for custom metrics
    })
  }

  public trackUserAction(action: string, metadata?: Record<string, any>) {
    this.addInteraction({
      type: 'click',
      target: action,
      timestamp: Date.now(),
      metadata,
    })
  }

  public getMetrics(): PerformanceMetric[] {
    return [...this.metrics]
  }

  public getErrors(): ErrorReport[] {
    return [...this.errors]
  }

  public enable() {
    this.isEnabled = true
    this.initialize()
  }

  public disable() {
    this.isEnabled = false
  }
}

// Global instance
export const performanceMonitor = new PerformanceMonitor({
  reportingEndpoint: (typeof process !== 'undefined' && process.env?.REACT_APP_PERFORMANCE_ENDPOINT) || undefined,
  enabled: (typeof process !== 'undefined' && process.env?.NODE_ENV === 'production') || false,
})

export default PerformanceMonitor
