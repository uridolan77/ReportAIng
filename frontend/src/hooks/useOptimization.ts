import { useCallback, useMemo, useRef, useState, useEffect } from 'react';

// Deep comparison for complex objects
export const useDeepMemo = <T>(factory: () => T, deps: React.DependencyList): T => {
  const ref = useRef<{ deps: React.DependencyList; value: T }>();

  if (!ref.current || !deepEqual(ref.current.deps, deps)) {
    ref.current = { deps, value: factory() };
  }

  return ref.current.value;
};

// Deep equality check
function deepEqual(a: any, b: any): boolean {
  if (a === b) return true;
  if (a == null || b == null) return false;
  if (typeof a !== typeof b) return false;

  if (typeof a === 'object') {
    const keysA = Object.keys(a);
    const keysB = Object.keys(b);

    if (keysA.length !== keysB.length) return false;

    for (const key of keysA) {
      if (!keysB.includes(key) || !deepEqual(a[key], b[key])) {
        return false;
      }
    }

    return true;
  }

  return false;
}

// Stable callback that only changes when dependencies change
export const useStableCallback = <T extends (...args: any[]) => any>(
  callback: T,
  deps: React.DependencyList
): T => {
  const ref = useRef<T>();

  return useMemo(() => {
    ref.current = callback;
    return ((...args: any[]) => ref.current?.(...args)) as T;
  }, [callback, ...deps]);
};

// Memoized data processing
export const useProcessedData = <TInput, TOutput>(
  data: TInput[],
  processor: (data: TInput[]) => TOutput[],
  deps: React.DependencyList = []
): TOutput[] => {
  return useMemo(() => {
    if (!data || data.length === 0) return [];

    const startTime = performance.now();
    const result = processor(data);
    const endTime = performance.now();

    if (endTime - startTime > 10) {
      console.warn(`Data processing took ${(endTime - startTime).toFixed(2)}ms for ${data.length} items`);
    }

    return result;
  }, [data, processor, ...deps]);
};

// Chunked processing for large datasets
export const useChunkedProcessing = <TInput, TOutput>(
  data: TInput[],
  processor: (chunk: TInput[]) => TOutput[],
  chunkSize: number = 1000
): {
  processedData: TOutput[];
  isProcessing: boolean;
  progress: number;
} => {
  const [processedData, setProcessedData] = useState<TOutput[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    if (!data || data.length === 0) {
      setProcessedData([]);
      setProgress(0);
      return;
    }

    setIsProcessing(true);
    setProgress(0);

    const chunks = [];
    for (let i = 0; i < data.length; i += chunkSize) {
      chunks.push(data.slice(i, i + chunkSize));
    }

    let processedChunks: TOutput[] = [];
    let currentChunk = 0;

    const processNextChunk = () => {
      if (currentChunk >= chunks.length) {
        setProcessedData(processedChunks);
        setIsProcessing(false);
        setProgress(100);
        return;
      }

      const chunk = chunks[currentChunk];
      const result = processor(chunk);
      processedChunks = [...processedChunks, ...result];

      currentChunk++;
      setProgress((currentChunk / chunks.length) * 100);

      // Use requestIdleCallback for better performance
      if ('requestIdleCallback' in window) {
        requestIdleCallback(processNextChunk);
      } else {
        setTimeout(processNextChunk, 0);
      }
    };

    processNextChunk();
  }, [data, chunkSize, processor]);

  return { processedData, isProcessing, progress };
};

// Optimized search with debouncing
export const useOptimizedSearch = <T>(
  data: T[],
  searchTerm: string,
  searchFields: (keyof T)[],
  debounceMs: number = 300
): T[] => {
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState(searchTerm);

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, debounceMs);

    return () => clearTimeout(timer);
  }, [searchTerm, debounceMs]);

  return useMemo(() => {
    if (!debouncedSearchTerm.trim()) return data;

    const lowercaseSearch = debouncedSearchTerm.toLowerCase();

    return data.filter(item => {
      return searchFields.some(field => {
        const value = item[field];
        if (typeof value === 'string') {
          return value.toLowerCase().includes(lowercaseSearch);
        }
        if (typeof value === 'number') {
          return value.toString().includes(lowercaseSearch);
        }
        return false;
      });
    });
  }, [data, debouncedSearchTerm, searchFields]);
};

// Memoized sorting
export const useSortedData = <T>(
  data: T[],
  sortKey: keyof T | null,
  sortDirection: 'asc' | 'desc' = 'asc'
): T[] => {
  return useMemo(() => {
    if (!sortKey) return data;

    return [...data].sort((a, b) => {
      const aValue = a[sortKey];
      const bValue = b[sortKey];

      if (aValue === bValue) return 0;

      let comparison = 0;
      if (typeof aValue === 'string' && typeof bValue === 'string') {
        comparison = aValue.localeCompare(bValue);
      } else if (typeof aValue === 'number' && typeof bValue === 'number') {
        comparison = aValue - bValue;
      } else {
        comparison = String(aValue).localeCompare(String(bValue));
      }

      return sortDirection === 'asc' ? comparison : -comparison;
    });
  }, [data, sortKey, sortDirection]);
};

// Pagination with memoization
export const usePaginatedData = <T>(
  data: T[],
  pageSize: number,
  currentPage: number = 1
): {
  paginatedData: T[];
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
} => {
  return useMemo(() => {
    const totalPages = Math.ceil(data.length / pageSize);
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;

    return {
      paginatedData: data.slice(startIndex, endIndex),
      totalPages,
      hasNextPage: currentPage < totalPages,
      hasPreviousPage: currentPage > 1
    };
  }, [data, pageSize, currentPage]);
};

// Performance monitoring hook
export const usePerformanceMonitor = (componentName: string) => {
  const renderCount = useRef(0);
  const lastRenderTime = useRef(performance.now());

  useEffect(() => {
    renderCount.current++;
    const currentTime = performance.now();
    const timeSinceLastRender = currentTime - lastRenderTime.current;

    if (renderCount.current > 1 && timeSinceLastRender < 16) {
      console.warn(`${componentName} rendered ${renderCount.current} times in ${timeSinceLastRender.toFixed(2)}ms`);
    }

    lastRenderTime.current = currentTime;
  });

  return {
    renderCount: renderCount.current,
    getStats: () => ({
      renderCount: renderCount.current,
      lastRenderTime: lastRenderTime.current
    })
  };
};

// Optimized event handlers
export const useOptimizedEventHandlers = () => {
  const handlersRef = useRef<Map<string, (...args: any[]) => void>>(new Map());

  const createHandler = useCallback(<T extends (...args: any[]) => void>(
    key: string,
    handler: T
  ): T => {
    if (!handlersRef.current.has(key)) {
      handlersRef.current.set(key, handler);
    }
    return handlersRef.current.get(key) as T;
  }, []);

  const clearHandlers = useCallback(() => {
    handlersRef.current.clear();
  }, []);

  return { createHandler, clearHandlers };
};
