// Enhanced virtualization service for handling large datasets
import { useMemo, useCallback, useRef, useEffect } from 'react';

interface VirtualizationConfig {
  overscan: number;
  itemHeight: number;
  containerHeight: number;
  bufferSize: number;
  scrollThreshold: number;
  estimatedItemSize?: number;
  dynamicHeight?: boolean;
}

interface VirtualItem {
  index: number;
  start: number;
  end: number;
  size: number;
}

interface VirtualRange {
  start: number;
  end: number;
  overscanStart: number;
  overscanEnd: number;
}

export class VirtualizationService {
  private config: VirtualizationConfig;
  private scrollElement: HTMLElement | null = null;
  private itemSizes: Map<number, number> = new Map();
  private scrollOffset = 0;
  private isScrolling = false;
  private scrollTimeout: NodeJS.Timeout | null = null;

  constructor(config: VirtualizationConfig) {
    this.config = config;
  }

  setScrollElement(element: HTMLElement | null) {
    this.scrollElement = element;
  }

  updateConfig(config: Partial<VirtualizationConfig>) {
    this.config = { ...this.config, ...config };
  }

  getItemSize(index: number): number {
    if (this.config.dynamicHeight && this.itemSizes.has(index)) {
      return this.itemSizes.get(index)!;
    }
    return this.config.estimatedItemSize || this.config.itemHeight;
  }

  setItemSize(index: number, size: number) {
    if (this.config.dynamicHeight) {
      this.itemSizes.set(index, size);
    }
  }

  getTotalSize(itemCount: number): number {
    if (this.config.dynamicHeight) {
      let totalSize = 0;
      for (let i = 0; i < itemCount; i++) {
        totalSize += this.getItemSize(i);
      }
      return totalSize;
    }
    return itemCount * this.config.itemHeight;
  }

  getVirtualRange(itemCount: number, scrollOffset: number): VirtualRange {
    const containerHeight = this.config.containerHeight;
    const itemHeight = this.config.itemHeight;
    const overscan = this.config.overscan;

    // Calculate visible range
    const start = Math.floor(scrollOffset / itemHeight);
    const visibleCount = Math.ceil(containerHeight / itemHeight);
    const end = Math.min(start + visibleCount, itemCount - 1);

    // Add overscan
    const overscanStart = Math.max(0, start - overscan);
    const overscanEnd = Math.min(itemCount - 1, end + overscan);

    return {
      start,
      end,
      overscanStart,
      overscanEnd
    };
  }

  getVirtualItems(itemCount: number, scrollOffset: number): VirtualItem[] {
    const range = this.getVirtualRange(itemCount, scrollOffset);
    const items: VirtualItem[] = [];

    for (let i = range.overscanStart; i <= range.overscanEnd; i++) {
      const size = this.getItemSize(i);
      let start = 0;

      if (this.config.dynamicHeight) {
        for (let j = 0; j < i; j++) {
          start += this.getItemSize(j);
        }
      } else {
        start = i * this.config.itemHeight;
      }

      items.push({
        index: i,
        start,
        end: start + size,
        size
      });
    }

    return items;
  }

  handleScroll = (scrollOffset: number) => {
    this.scrollOffset = scrollOffset;
    this.isScrolling = true;

    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
    }

    this.scrollTimeout = setTimeout(() => {
      this.isScrolling = false;
    }, 150);
  };

  measureElement = (index: number, element: HTMLElement) => {
    if (this.config.dynamicHeight) {
      const size = element.getBoundingClientRect().height;
      this.setItemSize(index, size);
    }
  };

  getScrollToOffset(index: number): number {
    if (this.config.dynamicHeight) {
      let offset = 0;
      for (let i = 0; i < index; i++) {
        offset += this.getItemSize(i);
      }
      return offset;
    }
    return index * this.config.itemHeight;
  }

  scrollToIndex(index: number, align: 'start' | 'center' | 'end' = 'start') {
    if (!this.scrollElement) return;

    const containerHeight = this.config.containerHeight;
    const itemOffset = this.getScrollToOffset(index);
    const itemSize = this.getItemSize(index);

    let scrollOffset: number;
    switch (align) {
      case 'start':
        scrollOffset = itemOffset;
        break;
      case 'center':
        scrollOffset = itemOffset - (containerHeight - itemSize) / 2;
        break;
      case 'end':
        scrollOffset = itemOffset - containerHeight + itemSize;
        break;
    }

    this.scrollElement.scrollTop = Math.max(0, scrollOffset);
  }

  // Cleanup method
  destroy() {
    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
    }
    this.itemSizes.clear();
  }
}

// Hook for using virtualization in React components
export const useVirtualization = (
  itemCount: number,
  config: VirtualizationConfig
) => {
  const service = useRef(new VirtualizationService(config));
  const scrollElementRef = useRef<HTMLElement | null>(null);

  useEffect(() => {
    service.current.updateConfig(config);
  }, [config]);

  useEffect(() => {
    const currentService = service.current;
    return () => {
      currentService.destroy();
    };
  }, []);

  const setScrollElement = useCallback((element: HTMLElement | null) => {
    scrollElementRef.current = element;
    service.current.setScrollElement(element);
  }, []);

  const virtualItems = useMemo(() => {
    const scrollOffset = scrollElementRef.current?.scrollTop || 0;
    return service.current.getVirtualItems(itemCount, scrollOffset);
  }, [itemCount]);

  const totalSize = useMemo(() => {
    return service.current.getTotalSize(itemCount);
  }, [itemCount]);

  const scrollToIndex = useCallback((index: number, align?: 'start' | 'center' | 'end') => {
    service.current.scrollToIndex(index, align);
  }, []);

  const measureElement = useCallback((index: number, element: HTMLElement) => {
    service.current.measureElement(index, element);
  }, []);

  const handleScroll = useCallback((event: React.UIEvent<HTMLElement>) => {
    const scrollOffset = event.currentTarget.scrollTop;
    service.current.handleScroll(scrollOffset);
  }, []);

  return {
    virtualItems,
    totalSize,
    scrollToIndex,
    measureElement,
    handleScroll,
    setScrollElement
  };
};

// Performance monitoring utilities
export const createPerformanceMonitor = () => {
  const metrics = {
    renderTime: 0,
    scrollEvents: 0,
    itemsRendered: 0,
    memoryUsage: 0
  };

  const startRender = () => performance.now();
  
  const endRender = (startTime: number, itemCount: number) => {
    metrics.renderTime = performance.now() - startTime;
    metrics.itemsRendered = itemCount;
  };

  const trackScroll = () => {
    metrics.scrollEvents++;
  };

  const getMetrics = () => ({ ...metrics });

  const reset = () => {
    Object.keys(metrics).forEach(key => {
      (metrics as any)[key] = 0;
    });
  };

  return {
    startRender,
    endRender,
    trackScroll,
    getMetrics,
    reset
  };
};
