/**
 * High-Performance Virtual Data Table
 * 
 * Implements virtual scrolling for large datasets to maintain smooth performance
 * even with 100,000+ records. Optimized for BI dashboard data display.
 */

import React, { useMemo, useCallback, useState } from 'react'
import { FixedSizeList as List, ListChildComponentProps } from 'react-window'
import { Table, Input, Button, Space, Typography, Card, Spin } from 'antd'
import { SearchOutlined, DownloadOutlined, FilterOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'

const { Text } = Typography

export interface VirtualDataTableColumn<T = any> {
  key: string
  title: string
  dataIndex: keyof T
  width?: number
  sortable?: boolean
  filterable?: boolean
  render?: (value: any, record: T, index: number) => React.ReactNode
  align?: 'left' | 'center' | 'right'
}

export interface VirtualDataTableProps<T = any> {
  /** Data source */
  data: T[]
  /** Column definitions */
  columns: VirtualDataTableColumn<T>[]
  /** Row height in pixels */
  rowHeight?: number
  /** Table height in pixels */
  height?: number
  /** Loading state */
  loading?: boolean
  /** Enable search */
  searchable?: boolean
  /** Search placeholder */
  searchPlaceholder?: string
  /** Enable export */
  exportable?: boolean
  /** Export filename */
  exportFilename?: string
  /** Row key extractor */
  rowKey?: keyof T | ((record: T) => string)
  /** Row click handler */
  onRowClick?: (record: T, index: number) => void
  /** Custom row class name */
  rowClassName?: (record: T, index: number) => string
  /** Empty state message */
  emptyText?: string
  /** Enable row selection */
  selectable?: boolean
  /** Selection change handler */
  onSelectionChange?: (selectedRows: T[]) => void
}

// Row component for virtual list
const VirtualRow: React.FC<ListChildComponentProps> = ({ index, style, data }) => {
  const { 
    filteredData, 
    columns, 
    onRowClick, 
    rowClassName, 
    rowKey,
    selectedRows,
    onRowSelect,
  } = data
  
  const record = filteredData[index]
  const key = typeof rowKey === 'function' ? rowKey(record) : record[rowKey as keyof typeof record]
  const isSelected = selectedRows.has(key)
  
  const handleClick = useCallback(() => {
    onRowClick?.(record, index)
  }, [record, index, onRowClick])
  
  const handleSelect = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    onRowSelect?.(key, e.target.checked)
  }, [key, onRowSelect])
  
  const className = `virtual-table-row ${rowClassName?.(record, index) || ''} ${isSelected ? 'selected' : ''}`
  
  return (
    <div style={style} className={className} onClick={handleClick}>
      <div className="virtual-table-row-content">
        {data.selectable && (
          <div className="virtual-table-cell" style={{ width: 50 }}>
            <input
              type="checkbox"
              checked={isSelected}
              onChange={handleSelect}
              onClick={(e) => e.stopPropagation()}
            />
          </div>
        )}
        {columns.map((column: VirtualDataTableColumn, colIndex: number) => {
          const value = record[column.dataIndex]
          const cellContent = column.render ? column.render(value, record, index) : value
          
          return (
            <div
              key={column.key}
              className="virtual-table-cell"
              style={{
                width: column.width || 150,
                textAlign: column.align || 'left',
              }}
            >
              {cellContent}
            </div>
          )
        })}
      </div>
    </div>
  )
}

export const VirtualDataTable = <T extends Record<string, any>>({
  data,
  columns,
  rowHeight = 50,
  height = 400,
  loading = false,
  searchable = true,
  searchPlaceholder = 'Search...',
  exportable = true,
  exportFilename = 'data.csv',
  rowKey = 'id',
  onRowClick,
  rowClassName,
  emptyText = 'No data available',
  selectable = false,
  onSelectionChange,
}: VirtualDataTableProps<T>) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set())
  
  // Filter data based on search term
  const filteredData = useMemo(() => {
    if (!searchTerm) return data
    
    const term = searchTerm.toLowerCase()
    return data.filter((record) =>
      columns.some((column) => {
        const value = record[column.dataIndex]
        return String(value).toLowerCase().includes(term)
      })
    )
  }, [data, searchTerm, columns])
  
  // Handle row selection
  const handleRowSelect = useCallback((key: string, selected: boolean) => {
    setSelectedRows(prev => {
      const newSet = new Set(prev)
      if (selected) {
        newSet.add(key)
      } else {
        newSet.delete(key)
      }
      
      // Notify parent of selection change
      if (onSelectionChange) {
        const selectedRecords = data.filter(record => {
          const recordKey = typeof rowKey === 'function' ? rowKey(record) : record[rowKey as keyof T]
          return newSet.has(String(recordKey))
        })
        onSelectionChange(selectedRecords)
      }
      
      return newSet
    })
  }, [data, rowKey, onSelectionChange])
  
  // Handle select all
  const handleSelectAll = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.checked) {
      const allKeys = new Set(
        filteredData.map(record => {
          const key = typeof rowKey === 'function' ? rowKey(record) : record[rowKey as keyof T]
          return String(key)
        })
      )
      setSelectedRows(allKeys)
      onSelectionChange?.(filteredData)
    } else {
      setSelectedRows(new Set())
      onSelectionChange?.([])
    }
  }, [filteredData, rowKey, onSelectionChange])
  
  // Export functionality
  const handleExport = useCallback(() => {
    const csvContent = [
      // Header
      columns.map(col => col.title).join(','),
      // Data rows
      ...filteredData.map(record =>
        columns.map(col => {
          const value = record[col.dataIndex]
          return `"${String(value).replace(/"/g, '""')}"`
        }).join(',')
      )
    ].join('\n')
    
    const blob = new Blob([csvContent], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = exportFilename
    link.click()
    URL.revokeObjectURL(url)
  }, [filteredData, columns, exportFilename])
  
  // List item data for virtual list
  const listItemData = {
    filteredData,
    columns,
    onRowClick,
    rowClassName,
    rowKey,
    selectedRows,
    onRowSelect: handleRowSelect,
    selectable,
  }
  
  const isAllSelected = selectedRows.size === filteredData.length && filteredData.length > 0
  const isIndeterminate = selectedRows.size > 0 && selectedRows.size < filteredData.length
  
  return (
    <Card>
      {/* Table Controls */}
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Space>
          {searchable && (
            <Input
              placeholder={searchPlaceholder}
              prefix={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{ width: 250 }}
            />
          )}
          <Text type="secondary">
            {filteredData.length} of {data.length} records
            {selectedRows.size > 0 && ` (${selectedRows.size} selected)`}
          </Text>
        </Space>
        
        <Space>
          {exportable && (
            <Button
              icon={<DownloadOutlined />}
              onClick={handleExport}
              disabled={filteredData.length === 0}
            >
              Export
            </Button>
          )}
        </Space>
      </div>
      
      {/* Table Header */}
      <div className="virtual-table-header">
        {selectable && (
          <div className="virtual-table-header-cell" style={{ width: 50 }}>
            <input
              type="checkbox"
              checked={isAllSelected}
              ref={(input) => {
                if (input) input.indeterminate = isIndeterminate
              }}
              onChange={handleSelectAll}
            />
          </div>
        )}
        {columns.map((column) => (
          <div
            key={column.key}
            className="virtual-table-header-cell"
            style={{
              width: column.width || 150,
              textAlign: column.align || 'left',
            }}
          >
            {column.title}
          </div>
        ))}
      </div>
      
      {/* Virtual List */}
      <Spin spinning={loading}>
        {filteredData.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '40px 0' }}>
            <Text type="secondary">{emptyText}</Text>
          </div>
        ) : (
          <List
            height={height}
            itemCount={filteredData.length}
            itemSize={rowHeight}
            itemData={listItemData}
            overscanCount={5}
          >
            {VirtualRow}
          </List>
        )}
      </Spin>
      
      {/* Table Styles */}
      <style jsx>{`
        .virtual-table-header {
          display: flex;
          border-bottom: 1px solid #f0f0f0;
          background: #fafafa;
          font-weight: 600;
        }
        
        .virtual-table-header-cell {
          padding: 12px 16px;
          border-right: 1px solid #f0f0f0;
          display: flex;
          align-items: center;
        }
        
        .virtual-table-row {
          border-bottom: 1px solid #f0f0f0;
          cursor: pointer;
          transition: background-color 0.2s;
        }
        
        .virtual-table-row:hover {
          background-color: #f5f5f5;
        }
        
        .virtual-table-row.selected {
          background-color: #e6f7ff;
        }
        
        .virtual-table-row-content {
          display: flex;
          height: 100%;
        }
        
        .virtual-table-cell {
          padding: 12px 16px;
          border-right: 1px solid #f0f0f0;
          display: flex;
          align-items: center;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }
      `}</style>
    </Card>
  )
}

export default VirtualDataTable
