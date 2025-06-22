import React, { useMemo, useCallback } from 'react'
import { List, Spin } from 'antd'
import { useVirtualScrolling, useComponentSize } from '@shared/utils/performance'

export interface VirtualListProps<T> {
  items: T[]
  itemHeight: number
  height?: number
  renderItem: (item: T, index: number) => React.ReactNode
  loading?: boolean
  loadMore?: () => void
  hasMore?: boolean
  overscan?: number
  className?: string
  testId?: string
}

/**
 * VirtualList - High-performance virtual scrolling list component
 * 
 * Features:
 * - Virtual scrolling for large datasets
 * - Automatic height calculation
 * - Infinite scrolling support
 * - Performance optimized rendering
 * - Memory efficient
 */
export const VirtualList = <T,>({
  items,
  itemHeight,
  height = 400,
  renderItem,
  loading = false,
  loadMore,
  hasMore = false,
  overscan = 5,
  className = '',
  testId = 'virtual-list'
}: VirtualListProps<T>) => {
  const { ref: containerRef, size } = useComponentSize()
  const containerHeight = height || size.height || 400

  const {
    scrollElementRef,
    visibleItems,
    totalHeight,
    handleScroll
  } = useVirtualScrolling(items, itemHeight, containerHeight, overscan)

  // Optimized render function
  const renderVirtualItem = useCallback((virtualItem: {
    item: T
    index: number
    style: React.CSSProperties
  }) => {
    return (
      <div key={virtualItem.index} style={virtualItem.style}>
        {renderItem(virtualItem.item, virtualItem.index)}
      </div>
    )
  }, [renderItem])

  // Handle infinite scrolling
  const handleScrollWithLoadMore = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    handleScroll(e)
    
    if (loadMore && hasMore && !loading) {
      const { scrollTop, scrollHeight, clientHeight } = e.currentTarget
      const scrollPercentage = (scrollTop + clientHeight) / scrollHeight
      
      // Load more when 80% scrolled
      if (scrollPercentage > 0.8) {
        loadMore()
      }
    }
  }, [handleScroll, loadMore, hasMore, loading])

  const containerStyle: React.CSSProperties = {
    height: containerHeight,
    overflow: 'auto',
    position: 'relative'
  }

  const contentStyle: React.CSSProperties = {
    height: totalHeight,
    position: 'relative'
  }

  return (
    <div 
      ref={containerRef}
      className={`virtual-list ${className}`}
      data-testid={testId}
      style={containerStyle}
    >
      <div
        ref={scrollElementRef}
        style={containerStyle}
        onScroll={handleScrollWithLoadMore}
      >
        <div style={contentStyle}>
          {visibleItems.map(renderVirtualItem)}
        </div>
        
        {loading && (
          <div style={{ 
            textAlign: 'center', 
            padding: '16px',
            position: 'absolute',
            bottom: 0,
            left: 0,
            right: 0,
            background: 'rgba(255, 255, 255, 0.9)'
          }}>
            <Spin size="small" />
          </div>
        )}
      </div>
    </div>
  )
}

// Specialized virtual list for transparency traces
export interface VirtualTraceListProps {
  traces: Array<{
    id: string
    traceId: string
    userQuestion: string
    confidence: number
    timestamp: string
    [key: string]: any
  }>
  onTraceSelect?: (traceId: string) => void
  loading?: boolean
  loadMore?: () => void
  hasMore?: boolean
  height?: number
  className?: string
}

export const VirtualTraceList: React.FC<VirtualTraceListProps> = ({
  traces,
  onTraceSelect,
  loading = false,
  loadMore,
  hasMore = false,
  height = 400,
  className = ''
}) => {
  const renderTraceItem = useCallback((trace: any, index: number) => (
    <List.Item
      key={trace.id}
      style={{ 
        padding: '12px 16px',
        borderBottom: '1px solid #f0f0f0',
        cursor: 'pointer',
        background: index % 2 === 0 ? '#fafafa' : '#ffffff'
      }}
      onClick={() => onTraceSelect?.(trace.traceId)}
    >
      <List.Item.Meta
        title={
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between',
            alignItems: 'center'
          }}>
            <span style={{ fontSize: '14px', fontWeight: 500 }}>
              {trace.userQuestion}
            </span>
            <span style={{ 
              fontSize: '12px', 
              color: trace.confidence > 0.8 ? '#52c41a' : '#faad14'
            }}>
              {(trace.confidence * 100).toFixed(0)}%
            </span>
          </div>
        }
        description={
          <div style={{ fontSize: '12px', color: '#666' }}>
            {trace.traceId} • {new Date(trace.timestamp).toLocaleString()}
          </div>
        }
      />
    </List.Item>
  ), [onTraceSelect])

  return (
    <VirtualList
      items={traces}
      itemHeight={80}
      height={height}
      renderItem={renderTraceItem}
      loading={loading}
      loadMore={loadMore}
      hasMore={hasMore}
      className={className}
      testId="virtual-trace-list"
    />
  )
}

// Specialized virtual list for steps
export interface VirtualStepListProps {
  steps: Array<{
    id: string
    stepName: string
    confidence: number
    processingTimeMs: number
    success: boolean
    [key: string]: any
  }>
  onStepSelect?: (stepId: string) => void
  height?: number
  className?: string
}

export const VirtualStepList: React.FC<VirtualStepListProps> = ({
  steps,
  onStepSelect,
  height = 300,
  className = ''
}) => {
  const renderStepItem = useCallback((step: any, index: number) => (
    <div
      key={step.id}
      style={{
        padding: '8px 12px',
        borderBottom: '1px solid #f0f0f0',
        cursor: 'pointer',
        background: step.success ? '#f6ffed' : '#fff2f0',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}
      onClick={() => onStepSelect?.(step.id)}
    >
      <div>
        <div style={{ fontSize: '13px', fontWeight: 500 }}>
          {step.stepName}
        </div>
        <div style={{ fontSize: '11px', color: '#666' }}>
          {step.processingTimeMs}ms
        </div>
      </div>
      <div style={{ 
        fontSize: '12px',
        color: step.confidence > 0.8 ? '#52c41a' : '#faad14'
      }}>
        {(step.confidence * 100).toFixed(0)}%
      </div>
    </div>
  ), [onStepSelect])

  return (
    <VirtualList
      items={steps}
      itemHeight={60}
      height={height}
      renderItem={renderStepItem}
      className={className}
      testId="virtual-step-list"
    />
  )
}

export default VirtualList
