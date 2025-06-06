import React, { useMemo } from 'react';
import { useVirtualScrolling } from '../../hooks/usePerformance';

interface VirtualScrollListProps<T> {
  items: T[];
  itemHeight: number;
  containerHeight: number;
  renderItem: (item: T, index: number) => React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  onScroll?: (scrollTop: number) => void;
}

export function VirtualScrollList<T>({
  items,
  itemHeight,
  containerHeight,
  renderItem,
  className,
  style,
  onScroll
}: VirtualScrollListProps<T>) {
  const {
    visibleItems,
    totalHeight,
    offsetY,
    handleScroll,
    visibleRange
  } = useVirtualScrolling(items, itemHeight, containerHeight);

  const handleScrollEvent = (event: React.UIEvent<HTMLDivElement>) => {
    handleScroll(event);
    onScroll?.(event.currentTarget.scrollTop);
  };

  const visibleItemsWithIndex = useMemo(() => {
    return visibleItems.map((item, index) => ({
      item,
      originalIndex: visibleRange.start + index
    }));
  }, [visibleItems, visibleRange.start]);

  return (
    <div
      className={className}
      style={{
        height: containerHeight,
        overflow: 'auto',
        ...style
      }}
      onScroll={handleScrollEvent}
      role="listbox"
      aria-label={`Virtual list with ${items.length} items`}
    >
      <div style={{ height: totalHeight, position: 'relative' }}>
        <div
          style={{
            transform: `translateY(${offsetY}px)`,
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0
          }}
        >
          {visibleItemsWithIndex.map(({ item, originalIndex }) => (
            <div
              key={originalIndex}
              style={{
                height: itemHeight,
                display: 'flex',
                alignItems: 'center'
              }}
              role="option"
              aria-selected={false}
              aria-posinset={originalIndex + 1}
              aria-setsize={items.length}
            >
              {renderItem(item, originalIndex)}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

// Example usage component for data tables
interface DataRow {
  id: string;
  name: string;
  value: number;
  category: string;
}

interface VirtualDataTableProps {
  data: DataRow[];
  onRowClick?: (row: DataRow) => void;
}

export const VirtualDataTable: React.FC<VirtualDataTableProps> = ({
  data,
  onRowClick
}) => {
  const renderRow = (row: DataRow, index: number) => (
    <div
      style={{
        display: 'flex',
        padding: '8px 16px',
        borderBottom: '1px solid #f0f0f0',
        cursor: onRowClick ? 'pointer' : 'default',
        backgroundColor: index % 2 === 0 ? '#fafafa' : '#ffffff'
      }}
      onClick={() => onRowClick?.(row)}
    >
      <div style={{ flex: 1, minWidth: 0 }}>
        <strong>{row.name}</strong>
      </div>
      <div style={{ flex: 1, minWidth: 0 }}>
        {row.value.toLocaleString()}
      </div>
      <div style={{ flex: 1, minWidth: 0 }}>
        {row.category}
      </div>
    </div>
  );

  return (
    <div>
      {/* Header */}
      <div
        style={{
          display: 'flex',
          padding: '12px 16px',
          backgroundColor: '#f5f5f5',
          borderBottom: '2px solid #d9d9d9',
          fontWeight: 'bold'
        }}
      >
        <div style={{ flex: 1 }}>Name</div>
        <div style={{ flex: 1 }}>Value</div>
        <div style={{ flex: 1 }}>Category</div>
      </div>

      {/* Virtual scrolling content */}
      <VirtualScrollList
        items={data}
        itemHeight={48}
        containerHeight={400}
        renderItem={renderRow}
        style={{ border: '1px solid #d9d9d9' }}
      />
    </div>
  );
};

// Performance monitoring wrapper
export const PerformanceMonitoredVirtualList = <T,>({
  items,
  ...props
}: VirtualScrollListProps<T>) => {
  const startTime = performance.now();

  React.useEffect(() => {
    const endTime = performance.now();
    const renderTime = endTime - startTime;

    if (renderTime > 16 && process.env.NODE_ENV === 'development') { // More than one frame (60fps)
      console.warn(`Virtual list render took ${renderTime.toFixed(2)}ms for ${items.length} items`);
    }
  }, [items.length, startTime]);

  return <VirtualScrollList items={items} {...props} />;
};
