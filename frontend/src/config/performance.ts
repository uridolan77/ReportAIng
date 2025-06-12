/**
 * Ultimate Performance Configuration
 * Comprehensive performance settings for optimal application performance
 */

// ===== PERFORMANCE CONSTANTS =====
export const PERFORMANCE_CONFIG = {
  // Query Performance
  QUERY: {
    DEFAULT_TIMEOUT: 30000, // 30 seconds
    MAX_TIMEOUT: 300000, // 5 minutes
    RETRY_ATTEMPTS: 3,
    RETRY_DELAY: 1000, // 1 second
    BATCH_SIZE: 1000,
    MAX_ROWS: 10000,
    STREAMING_CHUNK_SIZE: 100,
    DEBOUNCE_DELAY: 300, // 300ms
    THROTTLE_DELAY: 1000, // 1 second
  },

  // UI Performance
  UI: {
    VIRTUAL_SCROLL_THRESHOLD: 100,
    LAZY_LOAD_THRESHOLD: 50,
    INTERSECTION_THRESHOLD: 0.1,
    ANIMATION_DURATION: 300,
    DEBOUNCE_SEARCH: 300,
    THROTTLE_SCROLL: 16, // 60fps
    PAGINATION_SIZE: 50,
    MAX_VISIBLE_ITEMS: 1000,
  },

  // Memory Management
  MEMORY: {
    MAX_CACHE_SIZE: 100 * 1024 * 1024, // 100MB
    CACHE_TTL: 5 * 60 * 1000, // 5 minutes
    GC_INTERVAL: 30 * 1000, // 30 seconds
    MAX_HISTORY_ITEMS: 100,
    MAX_UNDO_STACK: 50,
    WEAK_MAP_CLEANUP_INTERVAL: 60 * 1000, // 1 minute
  },

  // Network Performance
  NETWORK: {
    MAX_CONCURRENT_REQUESTS: 6,
    REQUEST_TIMEOUT: 30000,
    RETRY_ATTEMPTS: 3,
    RETRY_DELAY: 1000,
    COMPRESSION_THRESHOLD: 1024, // 1KB
    PREFETCH_THRESHOLD: 2,
    WEBSOCKET_RECONNECT_DELAY: 5000,
    WEBSOCKET_MAX_RECONNECTS: 10,
  },

  // Bundle Performance
  BUNDLE: {
    CHUNK_SIZE_WARNING: 500 * 1024, // 500KB
    CHUNK_SIZE_ERROR: 1024 * 1024, // 1MB
    MAX_CHUNKS: 20,
    PRELOAD_CHUNKS: 3,
    PREFETCH_CHUNKS: 5,
    DYNAMIC_IMPORT_DELAY: 100,
  },

  // Monitoring Thresholds
  MONITORING: {
    FCP_THRESHOLD: 1800, // First Contentful Paint
    LCP_THRESHOLD: 2500, // Largest Contentful Paint
    FID_THRESHOLD: 100, // First Input Delay
    CLS_THRESHOLD: 0.1, // Cumulative Layout Shift
    TTFB_THRESHOLD: 800, // Time to First Byte
    TTI_THRESHOLD: 3800, // Time to Interactive
    MEMORY_WARNING: 50 * 1024 * 1024, // 50MB
    MEMORY_ERROR: 100 * 1024 * 1024, // 100MB
  },
} as const;

// ===== PERFORMANCE UTILITIES =====
export const PerformanceUtils = {
  // Debounce function with immediate option
  debounce: <T extends (...args: any[]) => any>(
    func: T,
    wait: number,
    immediate = false
  ): ((...args: Parameters<T>) => void) => {
    let timeout: NodeJS.Timeout | null = null;
    return (...args: Parameters<T>) => {
      const callNow = immediate && !timeout;
      if (timeout) clearTimeout(timeout);
      timeout = setTimeout(() => {
        timeout = null;
        if (!immediate) func(...args);
      }, wait);
      if (callNow) func(...args);
    };
  },

  // Throttle function with leading and trailing options
  throttle: <T extends (...args: any[]) => any>(
    func: T,
    limit: number,
    options: { leading?: boolean; trailing?: boolean } = {}
  ): ((...args: Parameters<T>) => void) => {
    let inThrottle: boolean;
    let lastFunc: NodeJS.Timeout;
    let lastRan: number;
    const { leading = true, trailing = true } = options;

    return (...args: Parameters<T>) => {
      if (!inThrottle) {
        if (leading) func(...args);
        lastRan = Date.now();
        inThrottle = true;
      } else {
        if (trailing) {
          clearTimeout(lastFunc);
          lastFunc = setTimeout(() => {
            if (Date.now() - lastRan >= limit) {
              func(...args);
              lastRan = Date.now();
            }
          }, limit - (Date.now() - lastRan));
        }
      }
    };
  },

  // Memory usage monitoring
  getMemoryUsage: (): MemoryInfo | null => {
    if ('memory' in performance) {
      return (performance as any).memory;
    }
    return null;
  },

  // Performance measurement
  measurePerformance: (name: string, fn: () => void): number => {
    const start = performance.now();
    fn();
    const end = performance.now();
    const duration = end - start;
    
    if (duration > PERFORMANCE_CONFIG.MONITORING.FID_THRESHOLD) {
      console.warn(`Performance warning: ${name} took ${duration.toFixed(2)}ms`);
    }
    
    return duration;
  },

  // Async performance measurement
  measureAsyncPerformance: async (name: string, fn: () => Promise<void>): Promise<number> => {
    const start = performance.now();
    await fn();
    const end = performance.now();
    const duration = end - start;
    
    if (duration > PERFORMANCE_CONFIG.MONITORING.FID_THRESHOLD) {
      console.warn(`Async performance warning: ${name} took ${duration.toFixed(2)}ms`);
    }
    
    return duration;
  },

  // Batch processing utility
  batchProcess: async <T, R>(
    items: T[],
    processor: (batch: T[]) => Promise<R[]>,
    batchSize = PERFORMANCE_CONFIG.QUERY.BATCH_SIZE
  ): Promise<R[]> => {
    const results: R[] = [];
    for (let i = 0; i < items.length; i += batchSize) {
      const batch = items.slice(i, i + batchSize);
      const batchResults = await processor(batch);
      results.push(...batchResults);
    }
    return results;
  },

  // Lazy loading utility
  createLazyLoader: <T>(
    loader: () => Promise<T>,
    threshold = PERFORMANCE_CONFIG.UI.LAZY_LOAD_THRESHOLD
  ) => {
    let loaded = false;
    let loading = false;
    let result: T | null = null;

    return {
      load: async (): Promise<T> => {
        if (loaded && result) return result;
        if (loading) {
          // Wait for existing load to complete
          while (loading) {
            await new Promise(resolve => setTimeout(resolve, 10));
          }
          return result!;
        }

        loading = true;
        try {
          result = await loader();
          loaded = true;
          return result;
        } finally {
          loading = false;
        }
      },
      isLoaded: () => loaded,
      isLoading: () => loading,
      reset: () => {
        loaded = false;
        loading = false;
        result = null;
      },
    };
  },
} as const;

// ===== PERFORMANCE MONITORING =====
export const PerformanceMonitor = {
  // Web Vitals monitoring
  observeWebVitals: (callback: (metric: any) => void) => {
    if (typeof window !== 'undefined') {
      // Observe FCP, LCP, FID, CLS
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          callback({
            name: entry.name,
            value: entry.startTime,
            rating: entry.startTime < PERFORMANCE_CONFIG.MONITORING.FCP_THRESHOLD ? 'good' : 'poor',
          });
        }
      });

      observer.observe({ entryTypes: ['paint', 'largest-contentful-paint', 'first-input', 'layout-shift'] });
    }
  },

  // Memory monitoring
  startMemoryMonitoring: (interval = 30000) => {
    if (typeof window !== 'undefined') {
      return setInterval(() => {
        const memory = PerformanceUtils.getMemoryUsage();
        if (memory) {
          if (memory.usedJSHeapSize > PERFORMANCE_CONFIG.MONITORING.MEMORY_WARNING) {
            console.warn('Memory usage warning:', memory);
          }
          if (memory.usedJSHeapSize > PERFORMANCE_CONFIG.MONITORING.MEMORY_ERROR) {
            console.error('Memory usage critical:', memory);
          }
        }
      }, interval);
    }
    return null;
  },

  // Bundle size monitoring
  checkBundleSize: () => {
    if (typeof window !== 'undefined' && 'performance' in window) {
      const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
      const transferSize = navigation.transferSize;
      
      if (transferSize > PERFORMANCE_CONFIG.BUNDLE.CHUNK_SIZE_WARNING) {
        console.warn(`Bundle size warning: ${(transferSize / 1024).toFixed(2)}KB`);
      }
      
      return transferSize;
    }
    return 0;
  },
} as const;

// ===== TYPE DEFINITIONS =====
export interface PerformanceMetric {
  name: string;
  value: number;
  rating: 'good' | 'needs-improvement' | 'poor';
  timestamp: number;
}

export interface MemoryInfo {
  usedJSHeapSize: number;
  totalJSHeapSize: number;
  jsHeapSizeLimit: number;
}

export interface PerformanceConfig {
  enableMonitoring: boolean;
  enableMemoryTracking: boolean;
  enableWebVitals: boolean;
  enableBundleAnalysis: boolean;
  reportingInterval: number;
  thresholds: {
    fcp: number;
    lcp: number;
    fid: number;
    cls: number;
    memory: number;
  };
}

// ===== DEFAULT EXPORT =====
export default {
  config: PERFORMANCE_CONFIG,
  utils: PerformanceUtils,
  monitor: PerformanceMonitor,
} as const;
