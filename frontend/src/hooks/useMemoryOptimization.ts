/**
 * Memory Optimization Hook
 * 
 * Advanced memory management for React 18 applications
 * with automatic cleanup, leak detection, and performance monitoring
 */

import { useEffect, useRef, useCallback, useState } from 'react';

interface MemoryMetrics {
  usedJSHeapSize: number;
  totalJSHeapSize: number;
  jsHeapSizeLimit: number;
  componentCount: number;
  listenerCount: number;
  observerCount: number;
}

interface MemoryOptimizationOptions {
  enableAutoCleanup?: boolean;
  enableLeakDetection?: boolean;
  cleanupInterval?: number;
  memoryThreshold?: number;
  onMemoryWarning?: (metrics: MemoryMetrics) => void;
  onMemoryLeak?: (leakInfo: any) => void;
}

export const useMemoryOptimization = (options: MemoryOptimizationOptions = {}) => {
  const {
    enableAutoCleanup = true,
    enableLeakDetection = true,
    cleanupInterval = 30000, // 30 seconds
    memoryThreshold = 50 * 1024 * 1024, // 50MB
    onMemoryWarning,
    onMemoryLeak
  } = options;

  const [memoryMetrics, setMemoryMetrics] = useState<MemoryMetrics | null>(null);
  const cleanupFunctions = useRef<Array<() => void>>([]);
  const eventListeners = useRef<Array<{ element: EventTarget; event: string; handler: EventListener }>>([]);
  const observers = useRef<Array<IntersectionObserver | MutationObserver | ResizeObserver>>([]);
  const timers = useRef<Array<NodeJS.Timeout>>([]);
  const componentRefs = useRef<Set<any>>(new Set());

  // Memory monitoring
  const getMemoryMetrics = useCallback((): MemoryMetrics | null => {
    if ('memory' in performance) {
      const memory = (performance as any).memory;
      return {
        usedJSHeapSize: memory.usedJSHeapSize,
        totalJSHeapSize: memory.totalJSHeapSize,
        jsHeapSizeLimit: memory.jsHeapSizeLimit,
        componentCount: componentRefs.current.size,
        listenerCount: eventListeners.current.length,
        observerCount: observers.current.length
      };
    }
    return null;
  }, []);

  // Register cleanup function
  const registerCleanup = useCallback((cleanupFn: () => void) => {
    cleanupFunctions.current.push(cleanupFn);
    return () => {
      const index = cleanupFunctions.current.indexOf(cleanupFn);
      if (index > -1) {
        cleanupFunctions.current.splice(index, 1);
      }
    };
  }, []);

  // Enhanced event listener management
  const addEventListenerWithCleanup = useCallback((
    element: EventTarget,
    event: string,
    handler: EventListener,
    options?: AddEventListenerOptions
  ) => {
    element.addEventListener(event, handler, options);
    
    const listenerInfo = { element, event, handler };
    eventListeners.current.push(listenerInfo);

    return () => {
      element.removeEventListener(event, handler, options);
      const index = eventListeners.current.indexOf(listenerInfo);
      if (index > -1) {
        eventListeners.current.splice(index, 1);
      }
    };
  }, []);

  // Observer management
  const createObserverWithCleanup = useCallback(<T extends IntersectionObserver | MutationObserver | ResizeObserver>(
    observer: T
  ): T => {
    observers.current.push(observer);
    
    const originalDisconnect = observer.disconnect.bind(observer);
    observer.disconnect = () => {
      originalDisconnect();
      const index = observers.current.indexOf(observer);
      if (index > -1) {
        observers.current.splice(index, 1);
      }
    };

    return observer;
  }, []);

  // Timer management
  const setTimeoutWithCleanup = useCallback((callback: () => void, delay: number) => {
    const timer = setTimeout(() => {
      callback();
      const index = timers.current.indexOf(timer);
      if (index > -1) {
        timers.current.splice(index, 1);
      }
    }, delay);
    
    timers.current.push(timer);
    return timer;
  }, []);

  const setIntervalWithCleanup = useCallback((callback: () => void, delay: number) => {
    const timer = setInterval(callback, delay);
    timers.current.push(timer);
    
    return () => {
      clearInterval(timer);
      const index = timers.current.indexOf(timer);
      if (index > -1) {
        timers.current.splice(index, 1);
      }
    };
  }, []);

  // Component reference tracking
  const trackComponent = useCallback((component: any) => {
    componentRefs.current.add(component);
    return () => {
      componentRefs.current.delete(component);
    };
  }, []);

  // Memory leak detection
  const detectMemoryLeaks = useCallback(() => {
    if (!enableLeakDetection) return;

    const metrics = getMemoryMetrics();
    if (!metrics) return;

    // Check for memory threshold breach
    if (metrics.usedJSHeapSize > memoryThreshold) {
      onMemoryWarning?.(metrics);
    }

    // Check for potential leaks
    const potentialLeaks = [];

    // Too many event listeners
    if (metrics.listenerCount > 100) {
      potentialLeaks.push({
        type: 'event_listeners',
        count: metrics.listenerCount,
        severity: 'warning'
      });
    }

    // Too many observers
    if (metrics.observerCount > 50) {
      potentialLeaks.push({
        type: 'observers',
        count: metrics.observerCount,
        severity: 'warning'
      });
    }

    // Memory growth pattern detection
    const memoryGrowthRate = metrics.usedJSHeapSize / metrics.totalJSHeapSize;
    if (memoryGrowthRate > 0.8) {
      potentialLeaks.push({
        type: 'memory_growth',
        rate: memoryGrowthRate,
        severity: 'error'
      });
    }

    if (potentialLeaks.length > 0) {
      onMemoryLeak?.({
        leaks: potentialLeaks,
        metrics,
        timestamp: Date.now()
      });
    }
  }, [enableLeakDetection, getMemoryMetrics, memoryThreshold, onMemoryWarning, onMemoryLeak]);

  // Automatic cleanup
  const performCleanup = useCallback(() => {
    if (!enableAutoCleanup) return;

    // Run registered cleanup functions
    cleanupFunctions.current.forEach(cleanup => {
      try {
        cleanup();
      } catch (error) {
        console.warn('Cleanup function failed:', error);
      }
    });

    // Force garbage collection if available
    if ('gc' in window && typeof (window as any).gc === 'function') {
      try {
        (window as any).gc();
      } catch (error) {
        // GC not available in production
      }
    }

    // Update metrics after cleanup
    const metrics = getMemoryMetrics();
    if (metrics) {
      setMemoryMetrics(metrics);
    }
  }, [enableAutoCleanup, getMemoryMetrics]);

  // Optimize large data structures
  const optimizeDataStructure = useCallback(<T>(
    data: T[],
    maxSize: number = 1000
  ): T[] => {
    if (data.length <= maxSize) return data;
    
    // Keep most recent items
    return data.slice(-maxSize);
  }, []);

  // Debounced function creator with cleanup
  const createDebouncedFunction = useCallback(<T extends (...args: any[]) => any>(
    func: T,
    delay: number
  ): T => {
    let timeoutId: NodeJS.Timeout;
    
    const debouncedFn = ((...args: Parameters<T>) => {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(() => func(...args), delay);
    }) as T;

    // Register cleanup
    registerCleanup(() => {
      clearTimeout(timeoutId);
    });

    return debouncedFn;
  }, [registerCleanup]);

  // Initialize memory monitoring
  useEffect(() => {
    if (!enableAutoCleanup && !enableLeakDetection) return;

    const interval = setInterval(() => {
      detectMemoryLeaks();
      performCleanup();
    }, cleanupInterval);

    return () => {
      clearInterval(interval);
    };
  }, [enableAutoCleanup, enableLeakDetection, cleanupInterval, detectMemoryLeaks, performCleanup]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      // Clean up all event listeners
      eventListeners.current.forEach(({ element, event, handler }) => {
        element.removeEventListener(event, handler);
      });

      // Disconnect all observers
      observers.current.forEach(observer => {
        observer.disconnect();
      });

      // Clear all timers
      timers.current.forEach(timer => {
        clearTimeout(timer);
        clearInterval(timer);
      });

      // Run all cleanup functions
      cleanupFunctions.current.forEach(cleanup => {
        try {
          cleanup();
        } catch (error) {
          console.warn('Cleanup function failed:', error);
        }
      });

      // Clear references
      componentRefs.current.clear();
      cleanupFunctions.current = [];
      eventListeners.current = [];
      observers.current = [];
      timers.current = [];
    };
  }, []);

  return {
    memoryMetrics,
    registerCleanup,
    addEventListenerWithCleanup,
    createObserverWithCleanup,
    setTimeoutWithCleanup,
    setIntervalWithCleanup,
    trackComponent,
    performCleanup,
    detectMemoryLeaks,
    optimizeDataStructure,
    createDebouncedFunction,
    getMemoryMetrics
  };
};
