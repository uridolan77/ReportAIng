import React, { useMemo, useRef, useEffect, useState, useCallback } from 'react'
import { Table, Spin, Empty, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'

const { Text } = Typography

interface VirtualTableProps<T = any> {
  columns: ColumnsType<T>
  dataSource: T[]
  height?: number
  rowHeight?: number
  overscan?: number
  loading?: boolean
  onScroll?: (scrollTop: number) => void
  onRowClick?: (record: T, index: number) => void
  rowKey?: string | ((record: T) => string)
  className?: string
  style?: React.CSSProperties
}

interface VirtualizedData<T> {
  items: T[]
  startIndex: number
  endIndex: number
  totalHeight: number
  offsetY: number
}

export const VirtualTable = <T extends Record<string, any>>({
  columns,
  dataSource,
  height = 400,
  rowHeight = 54,
  overscan = 5,
  loading = false,
  onScroll,
  onRowClick,
  rowKey = 'id',
  className,
  style
}: VirtualTableProps<T>) => {
  const containerRef = useRef<HTMLDivElement>(null)
  const [scrollTop, setScrollTop] = useState(0)
  const [containerHeight, setContainerHeight] = useState(height)

  // Calculate visible range
  const virtualizedData = useMemo((): VirtualizedData<T> => {
    const totalHeight = dataSource.length * rowHeight
    const startIndex = Math.max(0, Math.floor(scrollTop / rowHeight) - overscan)
    const endIndex = Math.min(
      dataSource.length - 1,
      Math.ceil((scrollTop + containerHeight) / rowHeight) + overscan
    )
    
    const items = dataSource.slice(startIndex, endIndex + 1)
    const offsetY = startIndex * rowHeight

    return {
      items,
      startIndex,
      endIndex,
      totalHeight,
      offsetY
    }
  }, [dataSource, scrollTop, containerHeight, rowHeight, overscan])

  // Handle scroll events
  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    const scrollTop = e.currentTarget.scrollTop
    setScrollTop(scrollTop)
    onScroll?.(scrollTop)
  }, [onScroll])

  // Update container height on resize
  useEffect(() => {
    const updateHeight = () => {
      if (containerRef.current) {
        const rect = containerRef.current.getBoundingClientRect()
        setContainerHeight(rect.height)
      }
    }

    updateHeight()
    window.addEventListener('resize', updateHeight)
    return () => window.removeEventListener('resize', updateHeight)
  }, [])

  // Get row key
  const getRowKey = useCallback((record: T, index: number): string => {
    if (typeof rowKey === 'function') {
      return rowKey(record)
    }
    return record[rowKey] || index.toString()
  }, [rowKey])

  if (loading) {
    return (
      <div 
        style={{ 
          height, 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center',
          ...style 
        }}
        className={className}
      >
        <Spin size="large" />
      </div>
    )
  }

  if (!dataSource.length) {
    return (
      <div 
        style={{ 
          height, 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center',
          ...style 
        }}
        className={className}
      >
        <Empty description="No data" />
      </div>
    )
  }

  return (
    <div
      ref={containerRef}
      style={{
        height,
        overflow: 'auto',
        border: '1px solid #f0f0f0',
        borderRadius: '6px',
        ...style
      }}
      className={className}
      onScroll={handleScroll}
    >
      {/* Virtual container */}
      <div style={{ height: virtualizedData.totalHeight, position: 'relative' }}>
        {/* Header */}
        <div
          style={{
            position: 'sticky',
            top: 0,
            zIndex: 10,
            backgroundColor: '#fafafa',
            borderBottom: '1px solid #f0f0f0',
            display: 'flex',
            height: rowHeight,
            alignItems: 'center'
          }}
        >
          {columns.map((column, index) => (
            <div
              key={column.key || index}
              style={{
                flex: column.width ? `0 0 ${column.width}px` : 1,
                padding: '0 16px',
                fontWeight: 'bold',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap'
              }}
            >
              {column.title}
            </div>
          ))}
        </div>

        {/* Virtual rows */}
        <div
          style={{
            transform: `translateY(${virtualizedData.offsetY}px)`,
            position: 'relative'
          }}
        >
          {virtualizedData.items.map((record, virtualIndex) => {
            const actualIndex = virtualizedData.startIndex + virtualIndex
            const key = getRowKey(record, actualIndex)
            
            return (
              <div
                key={key}
                style={{
                  display: 'flex',
                  height: rowHeight,
                  alignItems: 'center',
                  borderBottom: '1px solid #f0f0f0',
                  cursor: onRowClick ? 'pointer' : 'default',
                  backgroundColor: actualIndex % 2 === 0 ? '#ffffff' : '#fafafa'
                }}
                onClick={() => onRowClick?.(record, actualIndex)}
                onMouseEnter={(e) => {
                  e.currentTarget.style.backgroundColor = '#e6f7ff'
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.backgroundColor = actualIndex % 2 === 0 ? '#ffffff' : '#fafafa'
                }}
              >
                {columns.map((column, columnIndex) => {
                  const value = record[column.dataIndex as string]
                  const renderedValue = column.render 
                    ? column.render(value, record, actualIndex)
                    : value

                  return (
                    <div
                      key={column.key || columnIndex}
                      style={{
                        flex: column.width ? `0 0 ${column.width}px` : 1,
                        padding: '0 16px',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap'
                      }}
                    >
                      {renderedValue}
                    </div>
                  )
                })}
              </div>
            )
          })}
        </div>
      </div>

      {/* Scroll indicator */}
      <div
        style={{
          position: 'absolute',
          top: 0,
          right: 0,
          width: '4px',
          height: '100%',
          backgroundColor: '#f0f0f0',
          pointerEvents: 'none'
        }}
      >
        <div
          style={{
            width: '100%',
            backgroundColor: '#1890ff',
            height: `${(containerHeight / virtualizedData.totalHeight) * 100}%`,
            transform: `translateY(${(scrollTop / virtualizedData.totalHeight) * containerHeight}px)`,
            transition: 'transform 0.1s ease'
          }}
        />
      </div>

      {/* Performance info */}
      <div
        style={{
          position: 'absolute',
          bottom: '8px',
          right: '8px',
          backgroundColor: 'rgba(0, 0, 0, 0.7)',
          color: 'white',
          padding: '4px 8px',
          borderRadius: '4px',
          fontSize: '11px',
          pointerEvents: 'none'
        }}
      >
        <Text style={{ color: 'white', fontSize: '11px' }}>
          Showing {virtualizedData.items.length} of {dataSource.length} rows
        </Text>
      </div>
    </div>
  )
}

// Hook for virtual scrolling
export const useVirtualScroll = <T,>(
  data: T[],
  itemHeight: number,
  containerHeight: number
) => {
  const [scrollTop, setScrollTop] = useState(0)
  const [overscan] = useState(5)

  const virtualizedData = useMemo(() => {
    const totalHeight = data.length * itemHeight
    const startIndex = Math.max(0, Math.floor(scrollTop / itemHeight) - overscan)
    const endIndex = Math.min(
      data.length - 1,
      Math.ceil((scrollTop + containerHeight) / itemHeight) + overscan
    )
    
    const visibleItems = data.slice(startIndex, endIndex + 1)
    const offsetY = startIndex * itemHeight

    return {
      visibleItems,
      startIndex,
      endIndex,
      totalHeight,
      offsetY,
      visibleCount: visibleItems.length
    }
  }, [data, scrollTop, itemHeight, containerHeight, overscan])

  return {
    ...virtualizedData,
    scrollTop,
    setScrollTop
  }
}

// Performance optimized list component
export const VirtualList: React.FC<{
  items: any[]
  height: number
  itemHeight: number
  renderItem: (item: any, index: number) => React.ReactNode
  onScroll?: (scrollTop: number) => void
}> = ({ items, height, itemHeight, renderItem, onScroll }) => {
  const { visibleItems, startIndex, totalHeight, offsetY, setScrollTop } = useVirtualScroll(
    items,
    itemHeight,
    height
  )

  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    const scrollTop = e.currentTarget.scrollTop
    setScrollTop(scrollTop)
    onScroll?.(scrollTop)
  }, [setScrollTop, onScroll])

  return (
    <div
      style={{ height, overflow: 'auto' }}
      onScroll={handleScroll}
    >
      <div style={{ height: totalHeight, position: 'relative' }}>
        <div style={{ transform: `translateY(${offsetY}px)` }}>
          {visibleItems.map((item, index) => (
            <div key={startIndex + index} style={{ height: itemHeight }}>
              {renderItem(item, startIndex + index)}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
