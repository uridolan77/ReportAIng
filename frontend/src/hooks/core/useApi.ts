/**
 * Modern API Hook
 * 
 * Unified hook for API interactions with advanced features including
 * caching, retry logic, optimistic updates, and comprehensive state management.
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { useService } from '../../services/container/useService';
import { ApiService, ApiResponse } from '../../services/core/ApiService';

export interface UseApiOptions<T = any> {
  // Caching
  cache?: boolean;
  cacheKey?: string;
  cacheTTL?: number;
  
  // Retry & Error Handling
  retryAttempts?: number;
  retryDelay?: number;
  onError?: (error: Error) => void;
  onSuccess?: (data: T) => void;
  
  // Optimistic Updates
  optimisticUpdate?: T;
  rollbackOnError?: boolean;
  
  // Polling
  polling?: boolean;
  pollingInterval?: number;
  
  // Dependencies
  dependencies?: any[];
  enabled?: boolean;
  
  // Transform
  transform?: (data: any) => T;
  
  // Deduplication
  dedupe?: boolean;
}

export interface UseApiState<T = any> {
  data: T | null;
  loading: boolean;
  error: Error | null;
  success: boolean;
  lastFetch: Date | null;
  retryCount: number;
}

export interface UseApiReturn<T = any> extends UseApiState<T> {
  refetch: () => Promise<void>;
  mutate: (newData: T) => void;
  reset: () => void;
  cancel: () => void;
}

export function useApi<T = any>(
  endpoint: string | null,
  options: UseApiOptions<T> = {}
): UseApiReturn<T> {
  const apiService = useService<ApiService>('ApiService');
  const abortControllerRef = useRef<AbortController | null>(null);
  const pollingTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: false,
    error: null,
    success: false,
    lastFetch: null,
    retryCount: 0,
  });

  const {
    cache = true,
    cacheKey,
    cacheTTL,
    retryAttempts = 3,
    retryDelay = 1000,
    onError,
    onSuccess,
    optimisticUpdate,
    rollbackOnError = true,
    polling = false,
    pollingInterval = 30000,
    dependencies = [],
    enabled = true,
    transform,
    dedupe = true,
  } = options;

  // Cancel any ongoing request
  const cancel = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
    }
    
    if (pollingTimeoutRef.current) {
      clearTimeout(pollingTimeoutRef.current);
      pollingTimeoutRef.current = null;
    }
  }, []);

  // Reset state
  const reset = useCallback(() => {
    cancel();
    setState({
      data: null,
      loading: false,
      error: null,
      success: false,
      lastFetch: null,
      retryCount: 0,
    });
  }, [cancel]);

  // Mutate data optimistically
  const mutate = useCallback((newData: T) => {
    setState(prev => ({
      ...prev,
      data: newData,
      success: true,
      error: null,
    }));
  }, []);

  // Fetch data with retry logic
  const fetchData = useCallback(async (retryCount = 0): Promise<void> => {
    if (!endpoint || !enabled || !apiService) return;

    // Cancel previous request
    cancel();

    // Create new abort controller
    abortControllerRef.current = new AbortController();

    try {
      setState(prev => ({
        ...prev,
        loading: true,
        error: null,
        retryCount,
      }));

      // Apply optimistic update
      if (optimisticUpdate && retryCount === 0) {
        setState(prev => ({
          ...prev,
          data: optimisticUpdate,
        }));
      }

      const response: ApiResponse<T> = await apiService.get(
        endpoint,
        { signal: abortControllerRef.current.signal },
        { cache, cacheKey, cacheTTL }
      );

      if (!response.success) {
        throw new Error(response.message || 'API request failed');
      }

      let data = response.data;
      
      // Transform data if needed
      if (transform) {
        data = transform(data);
      }

      setState(prev => ({
        ...prev,
        data,
        loading: false,
        success: true,
        error: null,
        lastFetch: new Date(),
        retryCount,
      }));

      // Success callback
      if (onSuccess) {
        onSuccess(data);
      }

      // Setup polling if enabled
      if (polling && pollingInterval > 0) {
        pollingTimeoutRef.current = setTimeout(() => {
          fetchData(0);
        }, pollingInterval);
      }

    } catch (error: any) {
      // Don't update state if request was cancelled
      if (error.name === 'AbortError') {
        return;
      }

      // Rollback optimistic update on error
      if (optimisticUpdate && rollbackOnError && retryCount === 0) {
        setState(prev => ({
          ...prev,
          data: null,
        }));
      }

      // Retry logic
      if (retryCount < retryAttempts) {
        setTimeout(() => {
          fetchData(retryCount + 1);
        }, retryDelay * Math.pow(2, retryCount)); // Exponential backoff
        return;
      }

      // Final error state
      setState(prev => ({
        ...prev,
        loading: false,
        error: error as Error,
        success: false,
        retryCount,
      }));

      // Error callback
      if (onError) {
        onError(error as Error);
      }
    }
  }, [
    endpoint,
    enabled,
    apiService,
    cache,
    cacheKey,
    cacheTTL,
    transform,
    optimisticUpdate,
    rollbackOnError,
    polling,
    pollingInterval,
    retryAttempts,
    retryDelay,
    onSuccess,
    onError,
    cancel,
  ]);

  // Refetch function
  const refetch = useCallback(async () => {
    await fetchData(0);
  }, [fetchData]);

  // Effect to trigger initial fetch and handle dependencies
  useEffect(() => {
    fetchData(0);
    
    // Cleanup on unmount
    return () => {
      cancel();
    };
  }, [endpoint, enabled, ...dependencies]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      cancel();
    };
  }, [cancel]);

  return {
    ...state,
    refetch,
    mutate,
    reset,
    cancel,
  };
}

// Specialized hooks for common patterns
export function useApiQuery<T = any>(
  endpoint: string | null,
  options?: Omit<UseApiOptions<T>, 'cache'>
): UseApiReturn<T> {
  return useApi(endpoint, { ...options, cache: true });
}

export function useApiMutation<T = any>(
  endpoint: string | null,
  options?: Omit<UseApiOptions<T>, 'cache' | 'polling'>
): UseApiReturn<T> {
  return useApi(endpoint, { 
    ...options, 
    cache: false, 
    polling: false,
    enabled: false, // Manual trigger only
  });
}

export function useApiPolling<T = any>(
  endpoint: string | null,
  interval: number = 30000,
  options?: UseApiOptions<T>
): UseApiReturn<T> {
  return useApi(endpoint, {
    ...options,
    polling: true,
    pollingInterval: interval,
  });
}
