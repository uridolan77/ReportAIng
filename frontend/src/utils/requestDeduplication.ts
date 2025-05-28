// Request deduplication utility for preventing duplicate API calls
class RequestDeduplicator {
  private pendingRequests = new Map<string, Promise<any>>();
  private requestCounts = new Map<string, number>();
  
  // Generate a unique key for the request
  private generateKey(url: string, options?: RequestInit): string {
    const method = options?.method || 'GET';
    const body = options?.body ? JSON.stringify(options.body) : '';
    const headers = options?.headers ? JSON.stringify(options.headers) : '';
    
    return `${method}:${url}:${body}:${headers}`;
  }
  
  // Deduplicate fetch requests
  async deduplicatedFetch<T>(url: string, options?: RequestInit): Promise<T> {
    const key = this.generateKey(url, options);
    
    // If request is already pending, return the existing promise
    if (this.pendingRequests.has(key)) {
      console.log(`Deduplicating request: ${key}`);
      this.requestCounts.set(key, (this.requestCounts.get(key) || 0) + 1);
      return this.pendingRequests.get(key)!;
    }
    
    // Create new request
    const promise = fetch(url, options)
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.json();
      })
      .finally(() => {
        // Clean up after request completes
        this.pendingRequests.delete(key);
        
        // Log deduplication stats
        const count = this.requestCounts.get(key) || 0;
        if (count > 0) {
          console.log(`Request ${key} was deduplicated ${count} times`);
        }
        this.requestCounts.delete(key);
      });
    
    this.pendingRequests.set(key, promise);
    this.requestCounts.set(key, 0);
    
    return promise;
  }
  
  // Cancel all pending requests
  cancelAllRequests(): void {
    this.pendingRequests.clear();
    this.requestCounts.clear();
  }
  
  // Get pending request count
  getPendingRequestCount(): number {
    return this.pendingRequests.size;
  }
  
  // Get deduplication stats
  getStats(): { pendingRequests: number; totalDeduplicated: number } {
    const totalDeduplicated = Array.from(this.requestCounts.values())
      .reduce((sum, count) => sum + count, 0);
    
    return {
      pendingRequests: this.pendingRequests.size,
      totalDeduplicated,
    };
  }
}

// Global instance
export const requestDeduplicator = new RequestDeduplicator();

// Enhanced fetch function with deduplication
export const deduplicatedFetch = <T>(url: string, options?: RequestInit): Promise<T> => {
  return requestDeduplicator.deduplicatedFetch<T>(url, options);
};

// React hook for using deduplicated fetch
import { useCallback, useRef, useEffect } from 'react';

export const useDeduplicatedFetch = () => {
  const deduplicatorRef = useRef(new RequestDeduplicator());
  
  // Clean up on unmount
  useEffect(() => {
    return () => {
      deduplicatorRef.current.cancelAllRequests();
    };
  }, []);
  
  const fetch = useCallback(<T>(url: string, options?: RequestInit): Promise<T> => {
    return deduplicatorRef.current.deduplicatedFetch<T>(url, options);
  }, []);
  
  const getStats = useCallback(() => {
    return deduplicatorRef.current.getStats();
  }, []);
  
  const cancelAll = useCallback(() => {
    deduplicatorRef.current.cancelAllRequests();
  }, []);
  
  return {
    fetch,
    getStats,
    cancelAll,
  };
};

// Cache-aware deduplication for React Query
export class ReactQueryDeduplicator {
  private static instance: ReactQueryDeduplicator;
  private pendingQueries = new Map<string, Promise<any>>();
  
  static getInstance(): ReactQueryDeduplicator {
    if (!ReactQueryDeduplicator.instance) {
      ReactQueryDeduplicator.instance = new ReactQueryDeduplicator();
    }
    return ReactQueryDeduplicator.instance;
  }
  
  // Deduplicate query functions
  async deduplicateQuery<T>(
    queryKey: string,
    queryFn: () => Promise<T>,
    options?: {
      timeout?: number;
      onDuplicate?: () => void;
    }
  ): Promise<T> {
    const key = Array.isArray(queryKey) ? JSON.stringify(queryKey) : queryKey;
    
    // If query is already pending, return the existing promise
    if (this.pendingQueries.has(key)) {
      options?.onDuplicate?.();
      return this.pendingQueries.get(key)!;
    }
    
    // Create timeout promise if specified
    const timeoutPromise = options?.timeout 
      ? new Promise<never>((_, reject) => {
          setTimeout(() => reject(new Error('Query timeout')), options.timeout);
        })
      : null;
    
    // Create the query promise
    const queryPromise = queryFn().finally(() => {
      this.pendingQueries.delete(key);
    });
    
    this.pendingQueries.set(key, queryPromise);
    
    // Race with timeout if specified
    if (timeoutPromise) {
      return Promise.race([queryPromise, timeoutPromise]);
    }
    
    return queryPromise;
  }
  
  // Check if query is pending
  isPending(queryKey: string): boolean {
    const key = Array.isArray(queryKey) ? JSON.stringify(queryKey) : queryKey;
    return this.pendingQueries.has(key);
  }
  
  // Cancel specific query
  cancelQuery(queryKey: string): void {
    const key = Array.isArray(queryKey) ? JSON.stringify(queryKey) : queryKey;
    this.pendingQueries.delete(key);
  }
  
  // Cancel all queries
  cancelAll(): void {
    this.pendingQueries.clear();
  }
  
  // Get stats
  getStats(): { pendingQueries: number; queryKeys: string[] } {
    return {
      pendingQueries: this.pendingQueries.size,
      queryKeys: Array.from(this.pendingQueries.keys()),
    };
  }
}

// Hook for React Query deduplication
export const useQueryDeduplication = () => {
  const deduplicator = ReactQueryDeduplicator.getInstance();
  
  const deduplicateQuery = useCallback(<T>(
    queryKey: string,
    queryFn: () => Promise<T>,
    options?: {
      timeout?: number;
      onDuplicate?: () => void;
    }
  ): Promise<T> => {
    return deduplicator.deduplicateQuery(queryKey, queryFn, options);
  }, [deduplicator]);
  
  const isPending = useCallback((queryKey: string): boolean => {
    return deduplicator.isPending(queryKey);
  }, [deduplicator]);
  
  const cancelQuery = useCallback((queryKey: string): void => {
    deduplicator.cancelQuery(queryKey);
  }, [deduplicator]);
  
  const getStats = useCallback(() => {
    return deduplicator.getStats();
  }, [deduplicator]);
  
  return {
    deduplicateQuery,
    isPending,
    cancelQuery,
    getStats,
  };
};

// Performance monitoring for deduplication
export class DeduplicationMonitor {
  private static metrics = {
    totalRequests: 0,
    deduplicatedRequests: 0,
    savedTime: 0,
    savedBandwidth: 0,
  };
  
  static recordRequest(wasDeduplicated: boolean, estimatedTime = 100, estimatedSize = 1024): void {
    this.metrics.totalRequests++;
    
    if (wasDeduplicated) {
      this.metrics.deduplicatedRequests++;
      this.metrics.savedTime += estimatedTime;
      this.metrics.savedBandwidth += estimatedSize;
    }
  }
  
  static getMetrics() {
    const efficiency = this.metrics.totalRequests > 0 
      ? (this.metrics.deduplicatedRequests / this.metrics.totalRequests) * 100 
      : 0;
    
    return {
      ...this.metrics,
      efficiency: Math.round(efficiency * 100) / 100,
    };
  }
  
  static reset(): void {
    this.metrics = {
      totalRequests: 0,
      deduplicatedRequests: 0,
      savedTime: 0,
      savedBandwidth: 0,
    };
  }
}

// Export the monitor for use in DevTools
export const deduplicationMonitor = DeduplicationMonitor;
