/**
 * Accessible Data Table Component
 * 
 * WCAG 2.1 AA compliant data table with:
 * - Keyboard navigation
 * - Screen reader support
 * - Sortable columns with announcements
 * - Row selection with proper ARIA
 * - Search and filter accessibility
 */

import React, { useState, useCallback, useMemo, useRef, useEffect } from 'react'
import { Table, Input, Button, Space, Typography, Select, Tooltip, Card } from 'antd'
import { 
  SearchOutlined, 
  SortAscendingOutlined, 
  SortDescendingOutlined,
  FilterOutlined,
  DownloadOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons'
import type { ColumnsType, TableProps } from 'antd/es/table'
import { useAccessibility } from './AccessibilityProvider'

const { Text } = Typography
const { Option } = Select

export interface AccessibleDataTableProps<T = any> extends Omit<TableProps<T>, 'columns'> {
  /** Table data */
  data: T[]
  /** Column definitions with accessibility enhancements */
  columns: Array<{
    key: string
    title: string
    dataIndex: string
    sortable?: boolean
    filterable?: boolean
    searchable?: boolean
    width?: number
    render?: (value: any, record: T, index: number) => React.ReactNode
    description?: string
    ariaLabel?: string
  }>
  /** Table caption for screen readers */
  caption?: string
  /** Table summary for screen readers */
  summary?: string
  /** Enable search functionality */
  searchable?: boolean
  /** Enable export functionality */
  exportable?: boolean
  /** Export filename */
  exportFilename?: string
  /** Custom search placeholder */
  searchPlaceholder?: string
  /** Enable row selection */
  selectable?: boolean
  /** Selection change handler */
  onSelectionChange?: (selectedRows: T[]) => void
  /** Custom empty state message */
  emptyText?: string
  /** Loading state */
  loading?: boolean
}

export const AccessibleDataTable = <T extends Record<string, any>>({
  data,
  columns,
  caption,
  summary,
  searchable = true,
  exportable = true,
  exportFilename = 'table-data.csv',
  searchPlaceholder = 'Search table data...',
  selectable = false,
  onSelectionChange,
  emptyText = 'No data available',
  loading = false,
  ...tableProps
}: AccessibleDataTableProps<T>) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [sortConfig, setSortConfig] = useState<{ key: string; direction: 'asc' | 'desc' } | null>(null)
  const [filters, setFilters] = useState<Record<string, string>>({})
  const [selectedRows, setSelectedRows] = useState<T[]>([])
  const [focusedCell, setFocusedCell] = useState<{ row: number; col: number } | null>(null)
  
  const { announce, settings } = useAccessibility()
  const tableRef = useRef<HTMLDivElement>(null)
  const searchRef = useRef<HTMLInputElement>(null)

  // Filter and search data
  const filteredData = useMemo(() => {
    let result = [...data]

    // Apply search
    if (searchTerm) {
      const searchLower = searchTerm.toLowerCase()
      result = result.filter(row =>
        columns.some(col => {
          if (!col.searchable) return false
          const value = row[col.dataIndex]
          return String(value).toLowerCase().includes(searchLower)
        })
      )
    }

    // Apply filters
    Object.entries(filters).forEach(([key, value]) => {
      if (value) {
        result = result.filter(row => {
          const rowValue = String(row[key]).toLowerCase()
          return rowValue.includes(value.toLowerCase())
        })
      }
    })

    // Apply sorting
    if (sortConfig) {
      result.sort((a, b) => {
        const aVal = a[sortConfig.key]
        const bVal = b[sortConfig.key]
        
        if (typeof aVal === 'number' && typeof bVal === 'number') {
          return sortConfig.direction === 'asc' ? aVal - bVal : bVal - aVal
        }
        
        const aStr = String(aVal).toLowerCase()
        const bStr = String(bVal).toLowerCase()
        
        if (sortConfig.direction === 'asc') {
          return aStr < bStr ? -1 : aStr > bStr ? 1 : 0
        } else {
          return aStr > bStr ? -1 : aStr < bStr ? 1 : 0
        }
      })
    }

    return result
  }, [data, searchTerm, filters, sortConfig, columns])

  // Handle sorting
  const handleSort = useCallback((columnKey: string) => {
    setSortConfig(prev => {
      const newDirection = prev?.key === columnKey && prev.direction === 'asc' ? 'desc' : 'asc'
      const column = columns.find(col => col.key === columnKey)
      const columnTitle = column?.title || columnKey
      
      announce(`Table sorted by ${columnTitle} in ${newDirection}ending order`)
      
      return { key: columnKey, direction: newDirection }
    })
  }, [columns, announce])

  // Handle filter change
  const handleFilterChange = useCallback((columnKey: string, value: string) => {
    setFilters(prev => ({ ...prev, [columnKey]: value }))
    const column = columns.find(col => col.key === columnKey)
    const columnTitle = column?.title || columnKey
    
    if (value) {
      announce(`Filter applied to ${columnTitle}: ${value}`)
    } else {
      announce(`Filter removed from ${columnTitle}`)
    }
  }, [columns, announce])

  // Handle search
  const handleSearch = useCallback((value: string) => {
    setSearchTerm(value)
    announce(`Search applied: ${value || 'cleared'}. ${filteredData.length} results found.`)
  }, [filteredData.length, announce])

  // Handle row selection
  const handleRowSelection = useCallback((selectedRowKeys: React.Key[], selectedRows: T[]) => {
    setSelectedRows(selectedRows)
    onSelectionChange?.(selectedRows)
    announce(`${selectedRows.length} rows selected`)
  }, [onSelectionChange, announce])

  // Export data
  const handleExport = useCallback(() => {
    const headers = columns.map(col => col.title).join(',')
    const rows = filteredData.map(row =>
      columns.map(col => {
        const value = row[col.dataIndex]
        return `"${String(value).replace(/"/g, '""')}"`
      }).join(',')
    ).join('\n')

    const csv = `${headers}\n${rows}`
    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = exportFilename
    link.click()
    URL.revokeObjectURL(url)

    announce('Table data exported as CSV file')
  }, [filteredData, columns, exportFilename, announce])

  // Generate accessible columns
  const accessibleColumns: ColumnsType<T> = useMemo(() => {
    return columns.map(col => ({
      key: col.key,
      title: (
        <Space>
          <span>{col.title}</span>
          {col.description && (
            <Tooltip title={col.description}>
              <InfoCircleOutlined />
            </Tooltip>
          )}
          {col.sortable && (
            <Button
              type="text"
              size="small"
              icon={
                sortConfig?.key === col.key ? (
                  sortConfig.direction === 'asc' ? 
                    <SortAscendingOutlined /> : 
                    <SortDescendingOutlined />
                ) : (
                  <SortAscendingOutlined style={{ opacity: 0.5 }} />
                )
              }
              onClick={() => handleSort(col.key)}
              aria-label={`Sort by ${col.title}`}
            />
          )}
        </Space>
      ),
      dataIndex: col.dataIndex,
      width: col.width,
      render: col.render,
      sorter: col.sortable,
      filterDropdown: col.filterable ? (
        <div style={{ padding: 8 }}>
          <Input
            placeholder={`Filter ${col.title}`}
            value={filters[col.key] || ''}
            onChange={(e) => handleFilterChange(col.key, e.target.value)}
            style={{ width: 200 }}
            aria-label={`Filter by ${col.title}`}
          />
        </div>
      ) : undefined,
      filterIcon: col.filterable ? (
        <FilterOutlined style={{ color: filters[col.key] ? '#1890ff' : undefined }} />
      ) : undefined,
      onHeaderCell: () => ({
        'aria-label': col.ariaLabel || `${col.title} column header`,
        'aria-sort': sortConfig?.key === col.key ? 
          (sortConfig.direction === 'asc' ? 'ascending' : 'descending') : 
          'none',
      }),
    }))
  }, [columns, sortConfig, filters, handleSort, handleFilterChange])

  // Keyboard navigation
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (!focusedCell) return

    const { row, col } = focusedCell
    const maxRow = filteredData.length - 1
    const maxCol = columns.length - 1

    switch (e.key) {
      case 'ArrowUp':
        e.preventDefault()
        if (row > 0) {
          setFocusedCell({ row: row - 1, col })
          announce(`Row ${row}, Column ${columns[col].title}`)
        }
        break
      
      case 'ArrowDown':
        e.preventDefault()
        if (row < maxRow) {
          setFocusedCell({ row: row + 1, col })
          announce(`Row ${row + 2}, Column ${columns[col].title}`)
        }
        break
      
      case 'ArrowLeft':
        e.preventDefault()
        if (col > 0) {
          setFocusedCell({ row, col: col - 1 })
          announce(`Row ${row + 1}, Column ${columns[col - 1].title}`)
        }
        break
      
      case 'ArrowRight':
        e.preventDefault()
        if (col < maxCol) {
          setFocusedCell({ row, col: col + 1 })
          announce(`Row ${row + 1}, Column ${columns[col + 1].title}`)
        }
        break
      
      case 'Home':
        e.preventDefault()
        setFocusedCell({ row, col: 0 })
        announce(`Row ${row + 1}, first column`)
        break
      
      case 'End':
        e.preventDefault()
        setFocusedCell({ row, col: maxCol })
        announce(`Row ${row + 1}, last column`)
        break
    }
  }, [focusedCell, filteredData.length, columns, announce])

  return (
    <Card className="accessible-data-table">
      {/* Table controls */}
      <Space style={{ marginBottom: 16, width: '100%', justifyContent: 'space-between' }}>
        <Space>
          {searchable && (
            <Input
              ref={searchRef}
              placeholder={searchPlaceholder}
              prefix={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              style={{ width: 300 }}
              aria-label="Search table data"
            />
          )}
          <Text type="secondary">
            {filteredData.length} of {data.length} rows
            {selectedRows.length > 0 && ` (${selectedRows.length} selected)`}
          </Text>
        </Space>
        
        <Space>
          {exportable && (
            <Button
              icon={<DownloadOutlined />}
              onClick={handleExport}
              disabled={filteredData.length === 0}
              aria-label="Export table data"
            >
              Export
            </Button>
          )}
        </Space>
      </Space>

      {/* Screen reader table description */}
      {(caption || summary) && (
        <div className="sr-only">
          {caption && <p>{caption}</p>}
          {summary && <p>{summary}</p>}
          <p>
            Use arrow keys to navigate cells, Home/End for first/last column.
            {selectable && ' Use Space to select rows.'}
          </p>
        </div>
      )}

      {/* Accessible table */}
      <div
        ref={tableRef}
        onKeyDown={handleKeyDown}
        tabIndex={0}
        role="region"
        aria-label={caption || 'Data table'}
      >
        <Table<T>
          {...tableProps}
          dataSource={filteredData}
          columns={accessibleColumns}
          loading={loading}
          locale={{ emptyText }}
          rowSelection={selectable ? {
            selectedRowKeys: selectedRows.map((_, index) => index),
            onChange: handleRowSelection,
            getCheckboxProps: (record) => ({
              'aria-label': `Select row`,
            }),
          } : undefined}
          pagination={{
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => 
              `Showing ${range[0]}-${range[1]} of ${total} items`,
            ...tableProps.pagination,
          }}
          rowKey={(record, index) => index?.toString() || '0'}
          className="table-accessible"
          onRow={(record, index) => ({
            onClick: () => setFocusedCell({ row: index || 0, col: 0 }),
            onKeyDown: (e) => {
              if (e.key === ' ' && selectable) {
                e.preventDefault()
                const isSelected = selectedRows.includes(record)
                const newSelection = isSelected 
                  ? selectedRows.filter(r => r !== record)
                  : [...selectedRows, record]
                setSelectedRows(newSelection)
                onSelectionChange?.(newSelection)
                announce(isSelected ? 'Row deselected' : 'Row selected')
              }
            },
            'aria-label': `Table row ${(index || 0) + 1}`,
            tabIndex: focusedCell?.row === index ? 0 : -1,
          })}
        />
      </div>
    </Card>
  )
}

export default AccessibleDataTable
