/**
 * Accessible Chart Component
 * 
 * WCAG 2.1 AA compliant chart with:
 * - Keyboard navigation
 * - Screen reader support
 * - Data table alternative
 * - High contrast support
 * - Proper ARIA labels
 */

import React, { useState, useCallback, useMemo, useRef, useEffect } from 'react'
import { Card, Table, Button, Space, Typography, Switch, Tooltip } from 'antd'
import { 
  TableOutlined, 
  BarChartOutlined, 
  InfoCircleOutlined,
  DownloadOutlined,
} from '@ant-design/icons'
import { PerformantChart, type PerformantChartProps } from '../performance/PerformantChart'
import { useAccessibility } from './AccessibilityProvider'

const { Text, Title } = Typography

export interface AccessibleChartProps extends Omit<PerformantChartProps, 'onDataPointClick'> {
  /** Chart description for screen readers */
  description?: string
  /** Data table columns configuration */
  tableColumns?: Array<{
    key: string
    title: string
    dataIndex: string
    render?: (value: any, record: any) => React.ReactNode
  }>
  /** Enable data table view toggle */
  showTableToggle?: boolean
  /** Enable data export */
  enableExport?: boolean
  /** Chart summary for screen readers */
  summary?: string
  /** Keyboard navigation instructions */
  keyboardInstructions?: string
  /** Custom ARIA label */
  ariaLabel?: string
  /** Chart trends description */
  trendsDescription?: string
}

export const AccessibleChart: React.FC<AccessibleChartProps> = ({
  data,
  type,
  title,
  description,
  tableColumns,
  showTableToggle = true,
  enableExport = true,
  summary,
  keyboardInstructions,
  ariaLabel,
  trendsDescription,
  ...chartProps
}) => {
  const [showTable, setShowTable] = useState(false)
  const [focusedDataPoint, setFocusedDataPoint] = useState<number>(-1)
  const { settings, announce } = useAccessibility()
  const chartRef = useRef<HTMLDivElement>(null)
  const tableRef = useRef<HTMLDivElement>(null)

  // Generate accessible table columns if not provided
  const accessibleTableColumns = useMemo(() => {
    if (tableColumns) return tableColumns

    if (!data || data.length === 0) return []

    const firstRow = data[0]
    return Object.keys(firstRow).map(key => ({
      key,
      title: key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1'),
      dataIndex: key,
      render: (value: any) => {
        if (typeof value === 'number') {
          return value.toLocaleString()
        }
        return String(value)
      },
    }))
  }, [data, tableColumns])

  // Generate chart summary
  const chartSummary = useMemo(() => {
    if (summary) return summary

    if (!data || data.length === 0) {
      return 'No data available for this chart.'
    }

    const dataCount = data.length
    const chartTypeText = type === 'line' ? 'line chart' : 
                         type === 'bar' ? 'bar chart' :
                         type === 'pie' ? 'pie chart' :
                         type === 'area' ? 'area chart' : 'chart'

    return `${chartTypeText} with ${dataCount} data points. ${trendsDescription || ''}`
  }, [data, type, summary, trendsDescription])

  // Handle keyboard navigation
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (!data || data.length === 0) return

    switch (e.key) {
      case 'ArrowRight':
      case 'ArrowDown':
        e.preventDefault()
        setFocusedDataPoint(prev => {
          const next = Math.min(prev + 1, data.length - 1)
          const point = data[next]
          announce(`Data point ${next + 1} of ${data.length}: ${JSON.stringify(point)}`)
          return next
        })
        break
      
      case 'ArrowLeft':
      case 'ArrowUp':
        e.preventDefault()
        setFocusedDataPoint(prev => {
          const next = Math.max(prev - 1, 0)
          const point = data[next]
          announce(`Data point ${next + 1} of ${data.length}: ${JSON.stringify(point)}`)
          return next
        })
        break
      
      case 'Home':
        e.preventDefault()
        setFocusedDataPoint(0)
        announce(`First data point: ${JSON.stringify(data[0])}`)
        break
      
      case 'End':
        e.preventDefault()
        setFocusedDataPoint(data.length - 1)
        announce(`Last data point: ${JSON.stringify(data[data.length - 1])}`)
        break
      
      case 'Enter':
      case ' ':
        e.preventDefault()
        if (focusedDataPoint >= 0) {
          const point = data[focusedDataPoint]
          announce(`Selected data point: ${JSON.stringify(point)}`)
        }
        break
      
      case 't':
      case 'T':
        if (showTableToggle) {
          e.preventDefault()
          setShowTable(prev => !prev)
          announce(showTable ? 'Switched to chart view' : 'Switched to table view')
        }
        break
    }
  }, [data, focusedDataPoint, announce, showTable, showTableToggle])

  // Export data as CSV
  const handleExport = useCallback(() => {
    if (!data || data.length === 0) return

    const headers = accessibleTableColumns.map(col => col.title).join(',')
    const rows = data.map(row => 
      accessibleTableColumns.map(col => {
        const value = row[col.dataIndex]
        return `"${String(value).replace(/"/g, '""')}"`
      }).join(',')
    ).join('\n')

    const csv = `${headers}\n${rows}`
    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `${title || 'chart-data'}.csv`
    link.click()
    URL.revokeObjectURL(url)

    announce('Chart data exported as CSV file')
  }, [data, accessibleTableColumns, title, announce])

  // Focus management
  useEffect(() => {
    if (showTable && tableRef.current) {
      tableRef.current.focus()
    } else if (!showTable && chartRef.current) {
      chartRef.current.focus()
    }
  }, [showTable])

  const chartAriaLabel = ariaLabel || `${title || 'Chart'}: ${chartSummary}`

  return (
    <Card
      title={
        <Space>
          {title && <Title level={4}>{title}</Title>}
          {description && (
            <Tooltip title={description}>
              <InfoCircleOutlined />
            </Tooltip>
          )}
        </Space>
      }
      extra={
        <Space>
          {showTableToggle && (
            <Tooltip title="Toggle between chart and table view (Press T)">
              <Switch
                checked={showTable}
                onChange={(checked) => {
                  setShowTable(checked)
                  announce(checked ? 'Switched to table view' : 'Switched to chart view')
                }}
                checkedChildren={<TableOutlined />}
                unCheckedChildren={<BarChartOutlined />}
                aria-label="Toggle chart/table view"
              />
            </Tooltip>
          )}
          {enableExport && (
            <Tooltip title="Export chart data as CSV">
              <Button
                icon={<DownloadOutlined />}
                onClick={handleExport}
                aria-label="Export chart data"
              >
                Export
              </Button>
            </Tooltip>
          )}
        </Space>
      }
      className="accessible-chart"
    >
      {/* Chart description for screen readers */}
      <div className="sr-only">
        <p>{description}</p>
        <p>{chartSummary}</p>
        {keyboardInstructions && <p>Keyboard instructions: {keyboardInstructions}</p>}
        <p>
          Use arrow keys to navigate data points, Home/End for first/last point, 
          Enter or Space to select, and T to toggle table view.
        </p>
      </div>

      {showTable ? (
        <div
          ref={tableRef}
          tabIndex={0}
          role="region"
          aria-label="Chart data table"
          onKeyDown={handleKeyDown}
        >
          <Table
            dataSource={data}
            columns={accessibleTableColumns}
            pagination={{
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) => 
                `${range[0]}-${range[1]} of ${total} items`,
            }}
            rowKey={(record, index) => index?.toString() || '0'}
            className="table-accessible"
            summary={() => (
              <Table.Summary fixed>
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={accessibleTableColumns.length}>
                    <Text strong>
                      Total: {data.length} records
                    </Text>
                  </Table.Summary.Cell>
                </Table.Summary.Row>
              </Table.Summary>
            )}
            aria-label={`Data table for ${title || 'chart'}`}
          />
        </div>
      ) : (
        <div
          ref={chartRef}
          tabIndex={0}
          role="img"
          aria-label={chartAriaLabel}
          onKeyDown={handleKeyDown}
          className="chart-accessible"
          style={{
            outline: focusedDataPoint >= 0 ? '2px solid #005fcc' : 'none',
            outlineOffset: '2px',
          }}
        >
          <PerformantChart
            data={data}
            type={type}
            {...chartProps}
            config={{
              ...chartProps.config,
              // Enhance for accessibility
              showTooltip: true,
              showLegend: true,
            }}
            colors={settings.highContrast ? 
              ['#000000', '#666666', '#333333', '#999999'] : 
              chartProps.colors
            }
            performance={{
              ...chartProps.performance,
              enableAnimation: !settings.reduceMotion,
            }}
          />
          
          {/* Focused data point indicator */}
          {focusedDataPoint >= 0 && (
            <div className="sr-only" aria-live="polite">
              Data point {focusedDataPoint + 1} of {data.length} is focused
            </div>
          )}
        </div>
      )}
    </Card>
  )
}

export default AccessibleChart
