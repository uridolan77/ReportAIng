import React, { memo, useMemo, useCallback } from 'react';
import { Table, Card, List, Typography } from 'antd';

// Performance monitoring hook
const usePerformanceMonitor = (componentName: string) => {
  React.useEffect(() => {
    const startTime = performance.now();
    return () => {
      const endTime = performance.now();
      if (process.env.NODE_ENV === 'development') {
        console.log(`${componentName} render time: ${endTime - startTime}ms`);
      }
    };
  });
};

const { Text } = Typography;

// Memoized data table with performance monitoring
interface MemoizedDataTableProps {
  data: any[];
  columns: any[];
  loading?: boolean;
  onRowClick?: (record: any) => void;
  pageSize?: number;
}

export const MemoizedDataTable = memo<MemoizedDataTableProps>(({
  data,
  columns,
  loading = false,
  onRowClick,
  pageSize = 50
}) => {
  usePerformanceMonitor('MemoizedDataTable');

  // Memoize expensive column calculations
  const processedColumns = useMemo(() => {
    return columns.map(col => ({
      ...col,
      sorter: col.sortable ? (a: any, b: any) => {
        const aVal = a[col.dataIndex];
        const bVal = b[col.dataIndex];
        if (typeof aVal === 'string') return aVal.localeCompare(bVal);
        return aVal - bVal;
      } : undefined,
      render: col.render || ((text: any) => text)
    }));
  }, [columns]);

  // Memoize row selection handlers
  const handleRowClick = useCallback((record: any) => {
    onRowClick?.(record);
  }, [onRowClick]);

  // Memoize pagination config
  const paginationConfig = useMemo(() => ({
    pageSize,
    showSizeChanger: true,
    showQuickJumper: true,
    showTotal: (total: number, range: [number, number]) =>
      `${range[0]}-${range[1]} of ${total} items`,
  }), [pageSize]);

  return (
    <Table
      dataSource={data}
      columns={processedColumns}
      loading={loading}
      pagination={paginationConfig}
      onRow={(record) => ({
        onClick: () => handleRowClick(record),
        style: { cursor: onRowClick ? 'pointer' : 'default' }
      })}
      scroll={{ x: 'max-content' }}
      size="small"
    />
  );
}, (prevProps, nextProps) => {
  // Custom comparison function for better memoization
  return (
    prevProps.data === nextProps.data &&
    prevProps.columns === nextProps.columns &&
    prevProps.loading === nextProps.loading &&
    prevProps.pageSize === nextProps.pageSize &&
    prevProps.onRowClick === nextProps.onRowClick
  );
});

MemoizedDataTable.displayName = 'MemoizedDataTable';

// Memoized chart container with lazy loading
interface MemoizedChartContainerProps {
  title: string;
  data: any[];
  chartType: 'bar' | 'line' | 'pie' | 'heatmap' | 'treemap' | 'network';
  config?: any;
  onInteraction?: (data: any) => void;
}

export const MemoizedChartContainer = memo<MemoizedChartContainerProps>(({
  title,
  data,
  chartType,
  config = {},
  onInteraction
}) => {
  usePerformanceMonitor('MemoizedChartContainer');

  // Render chart based on type
  const renderChart = () => {
    switch (chartType) {
      case 'heatmap':
        const HeatmapChart = React.lazy(() => import('../Visualization/D3Charts/HeatmapChart').then(m => ({ default: m.HeatmapChart })));
        return <HeatmapChart {...chartProps} />;
      case 'treemap':
        const TreemapChart = React.lazy(() => import('../Visualization/D3Charts/TreemapChart').then(m => ({ default: m.TreemapChart })));
        return <TreemapChart {...chartProps} />;
      case 'network':
        const NetworkChart = React.lazy(() => import('../Visualization/D3Charts/NetworkChart').then(m => ({ default: m.NetworkChart })));
        return <NetworkChart {...chartProps} />;
      default:
        return <div>Chart type not supported</div>;
    }
  };

  // Memoize chart props based on chart type
  const chartProps = useMemo(() => {
    const baseProps = {
      config: {
        title,
        interactive: true,
        ...config
      }
    };

    switch (chartType) {
      case 'treemap':
        return {
          ...baseProps,
          data: Array.isArray(data) ? { name: 'Root', children: data } : data,
          onNodeClick: onInteraction
        };
      case 'network':
        return {
          ...baseProps,
          data,
          onNodeClick: onInteraction,
          onLinkClick: onInteraction
        };
      case 'heatmap':
        return {
          ...baseProps,
          data,
          onCellClick: onInteraction
        };
      default:
        return {
          ...baseProps,
          data,
          onCellClick: onInteraction
        };
    }
  }, [data, title, config, onInteraction, chartType]);

  return (
    <Card title={title} size="small">
      <React.Suspense fallback={<div>Loading chart...</div>}>
        {renderChart()}
      </React.Suspense>
    </Card>
  );
});

MemoizedChartContainer.displayName = 'MemoizedChartContainer';

// Memoized list with virtualization for large datasets
interface MemoizedVirtualListProps {
  items: any[];
  renderItem: (item: any, index: number) => React.ReactNode;
  itemHeight?: number;
  maxHeight?: number;
}

export const MemoizedVirtualList = memo<MemoizedVirtualListProps>(({
  items,
  renderItem,
  itemHeight = 50,
  maxHeight = 400
}) => {
  usePerformanceMonitor('MemoizedVirtualList');

  // Memoize the render function to prevent unnecessary re-renders
  const memoizedRenderItem = useCallback((item: any, index: number) => {
    return (
      <div key={index} style={{ height: itemHeight }}>
        {renderItem(item, index)}
      </div>
    );
  }, [renderItem, itemHeight]);

  // Use virtual scrolling for large lists
  const shouldUseVirtualScrolling = items.length > 100;

  if (shouldUseVirtualScrolling) {
    // Import virtual scrolling component dynamically
    const VirtualScrollList = React.lazy(() =>
      import('./VirtualScrollList').then(m => ({ default: m.VirtualScrollList }))
    );

    return (
      <React.Suspense fallback={<div>Loading list...</div>}>
        <VirtualScrollList
          items={items}
          itemHeight={itemHeight}
          containerHeight={maxHeight}
          renderItem={memoizedRenderItem}
        />
      </React.Suspense>
    );
  }

  // Regular list for smaller datasets
  return (
    <List
      dataSource={items}
      renderItem={memoizedRenderItem}
      style={{ maxHeight, overflow: 'auto' }}
      size="small"
    />
  );
});

MemoizedVirtualList.displayName = 'MemoizedVirtualList';

// Higher-order component for performance monitoring
export const withPerformanceMonitoring = <P extends object>(
  Component: React.ComponentType<P>,
  componentName?: string
) => {
  const WrappedComponent = memo((props: P) => {
    usePerformanceMonitor(componentName || Component.displayName || Component.name);
    return <Component {...props} />;
  });

  WrappedComponent.displayName = `withPerformanceMonitoring(${componentName || Component.displayName || Component.name})`;

  return WrappedComponent;
};

// Memoized dashboard grid with dynamic loading
interface MemoizedDashboardGridProps {
  widgets: Array<{
    id: string;
    type: string;
    title: string;
    data: any;
    config?: any;
    span?: number;
  }>;
  onWidgetInteraction?: (widgetId: string, data: any) => void;
}

export const MemoizedDashboardGrid = memo<MemoizedDashboardGridProps>(({
  widgets,
  onWidgetInteraction
}) => {
  usePerformanceMonitor('MemoizedDashboardGrid');

  // Memoize widget components
  const renderedWidgets = useMemo(() => {
    return widgets.map(widget => {
      const handleInteraction = (data: any) => {
        onWidgetInteraction?.(widget.id, data);
      };

      return (
        <div key={widget.id} style={{ gridColumn: `span ${widget.span || 1}` }}>
          <MemoizedChartContainer
            title={widget.title}
            data={widget.data}
            chartType={widget.type as any}
            config={widget.config}
            onInteraction={handleInteraction}
          />
        </div>
      );
    });
  }, [widgets, onWidgetInteraction]);

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
        gap: '16px',
        padding: '16px'
      }}
    >
      {renderedWidgets}
    </div>
  );
});

MemoizedDashboardGrid.displayName = 'MemoizedDashboardGrid';

// Performance metrics display component
interface PerformanceMetricsProps {
  showDetails?: boolean;
}

export const PerformanceMetrics = memo<PerformanceMetricsProps>(({ showDetails = false }) => {
  const [metrics, setMetrics] = React.useState<any>(null);

  React.useEffect(() => {
    const updateMetrics = () => {
      if ('memory' in performance) {
        const memory = (performance as any).memory;
        setMetrics({
          usedJSHeapSize: memory.usedJSHeapSize,
          totalJSHeapSize: memory.totalJSHeapSize,
          jsHeapSizeLimit: memory.jsHeapSizeLimit,
          timestamp: Date.now()
        });
      }
    };

    updateMetrics();
    const interval = setInterval(updateMetrics, 5000);
    return () => clearInterval(interval);
  }, []);

  if (!metrics) return null;

  const formatBytes = (bytes: number) => {
    return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
  };

  const usagePercentage = ((metrics.usedJSHeapSize / metrics.totalJSHeapSize) * 100).toFixed(1);

  return (
    <Card size="small" title="Performance Metrics">
      <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
        <div>
          <Text strong>Memory Usage:</Text>
          <br />
          <Text>{formatBytes(metrics.usedJSHeapSize)} ({usagePercentage}%)</Text>
        </div>
        {showDetails && (
          <>
            <div>
              <Text strong>Total Heap:</Text>
              <br />
              <Text>{formatBytes(metrics.totalJSHeapSize)}</Text>
            </div>
            <div>
              <Text strong>Heap Limit:</Text>
              <br />
              <Text>{formatBytes(metrics.jsHeapSizeLimit)}</Text>
            </div>
          </>
        )}
      </div>
    </Card>
  );
});

PerformanceMetrics.displayName = 'PerformanceMetrics';
