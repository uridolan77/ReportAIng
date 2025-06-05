import { useRef, useEffect, useState, useCallback, useMemo } from 'react';
import { VirtualizationService, createPerformanceMonitor } from '../services/VirtualizationService';

interface UseEnhancedVirtualizationProps {
  data: any[];
  itemHeight?: number;
  containerHeight?: number;
  enableDynamicHeight?: boolean;
  enablePerformanceMonitoring?: boolean;
  overscan?: number;
  bufferSize?: number;
  onPerformanceUpdate?: (metrics: any) => void;
}

export const useEnhancedVirtualization = ({
  data,
  itemHeight = 50,
  containerHeight = 500,
  enableDynamicHeight = false,
  enablePerformanceMonitoring = false,
  overscan = 5,
  bufferSize = 10,
  onPerformanceUpdate
}: UseEnhancedVirtualizationProps) => {
  const serviceRef = useRef<VirtualizationService | null>(null);
  const perfMonitorRef = useRef(createPerformanceMonitor());
  const containerRef = useRef<HTMLElement | null>(null);
  
  const [scrollOffset, setScrollOffset] = useState(0);
  const [isScrolling, setIsScrolling] = useState(false);
  const [itemSizes, setItemSizes] = useState<Map<number, number>>(new Map());

  // Initialize virtualization service
  useEffect(() => {
    if (!serviceRef.current) {
      serviceRef.current = new VirtualizationService({
        itemHeight,
        containerHeight,
        overscan,
        bufferSize,
        scrollThreshold: 100,
        estimatedItemSize: itemHeight,
        dynamicHeight: enableDynamicHeight
      });
    }
  }, [itemHeight, containerHeight, overscan, bufferSize, enableDynamicHeight]);

  // Update service configuration when props change
  useEffect(() => {
    if (serviceRef.current) {
      serviceRef.current.updateConfig({
        itemHeight,
        containerHeight,
        overscan,
        bufferSize,
        scrollThreshold: 100,
        estimatedItemSize: itemHeight,
        dynamicHeight: enableDynamicHeight
      });
    }
  }, [itemHeight, containerHeight, overscan, bufferSize, enableDynamicHeight]);

  // Calculate virtual items
  const virtualItems = useMemo(() => {
    if (!serviceRef.current) return [];
    return serviceRef.current.getVirtualItems(data.length, scrollOffset);
  }, [data.length, scrollOffset]);

  // Calculate total size
  const totalSize = useMemo(() => {
    if (!serviceRef.current) return data.length * itemHeight;
    return serviceRef.current.getTotalSize(data.length);
  }, [data.length, itemHeight]);

  // Scroll handling
  const handleScroll = useCallback((event: React.UIEvent<HTMLElement>) => {
    const newScrollOffset = event.currentTarget.scrollTop;
    setScrollOffset(newScrollOffset);
    setIsScrolling(true);
    
    if (serviceRef.current) {
      serviceRef.current.handleScroll(newScrollOffset);
    }

    if (enablePerformanceMonitoring) {
      perfMonitorRef.current.trackScroll();
    }

    // Reset scrolling state after delay
    setTimeout(() => setIsScrolling(false), 150);
  }, [enablePerformanceMonitoring]);

  // Measure element height
  const measureElement = useCallback((index: number, element: HTMLElement) => {
    if (enableDynamicHeight && element) {
      const size = element.getBoundingClientRect().height;
      setItemSizes(prev => new Map(prev.set(index, size)));
      
      if (serviceRef.current) {
        serviceRef.current.setItemSize(index, size);
      }
    }
  }, [enableDynamicHeight]);

  // Scroll to index
  const scrollToIndex = useCallback((index: number, align: 'start' | 'center' | 'end' = 'start') => {
    if (serviceRef.current && containerRef.current) {
      serviceRef.current.scrollToIndex(index, align);
    }
  }, []);

  // Set container element
  const setContainerElement = useCallback((element: HTMLElement | null) => {
    containerRef.current = element;
    if (serviceRef.current) {
      serviceRef.current.setScrollElement(element);
    }
  }, []);

  // Performance monitoring
  useEffect(() => {
    if (enablePerformanceMonitoring) {
      const perfMonitor = perfMonitorRef.current;
      const startTime = perfMonitor.startRender();

      return () => {
        perfMonitor.endRender(startTime, virtualItems.length);
        const metrics = perfMonitor.getMetrics();
        onPerformanceUpdate?.(metrics);
      };
    }
  }, [virtualItems, enablePerformanceMonitoring, onPerformanceUpdate]);

  // Get visible data slice
  const visibleData = useMemo(() => {
    return virtualItems.map(item => data[item.index]).filter(Boolean);
  }, [virtualItems, data]);

  // Get item size
  const getItemSize = useCallback((index: number) => {
    if (enableDynamicHeight && itemSizes.has(index)) {
      return itemSizes.get(index) || itemHeight;
    }
    if (serviceRef.current) {
      return serviceRef.current.getItemSize(index);
    }
    return itemHeight;
  }, [enableDynamicHeight, itemSizes, itemHeight]);

  // Get virtual range
  const virtualRange = useMemo(() => {
    if (!serviceRef.current) {
      return { start: 0, end: data.length - 1, overscanStart: 0, overscanEnd: data.length - 1 };
    }
    return serviceRef.current.getVirtualRange(data.length, scrollOffset);
  }, [data.length, scrollOffset]);

  // Cleanup
  useEffect(() => {
    return () => {
      if (serviceRef.current) {
        serviceRef.current.destroy();
      }
    };
  }, []);

  return {
    // Core virtualization data
    virtualItems,
    visibleData,
    totalSize,
    virtualRange,
    
    // State
    scrollOffset,
    isScrolling,
    
    // Actions
    handleScroll,
    measureElement,
    scrollToIndex,
    setContainerElement,
    getItemSize,
    
    // Performance
    performanceMetrics: enablePerformanceMonitoring 
      ? perfMonitorRef.current.getMetrics() 
      : null,
    
    // Service reference for advanced use cases
    virtualizationService: serviceRef.current
  };
};

export type { UseEnhancedVirtualizationProps };
