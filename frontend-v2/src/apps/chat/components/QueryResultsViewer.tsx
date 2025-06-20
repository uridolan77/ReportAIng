import React, { useState, useMemo } from 'react'
import {
  Card,
  Tabs,
  Button,
  Space,
  Typography,
  Select,
  Tooltip,
  Dropdown,
  Switch,
  Row,
  Col,
  Statistic,
  Tag,
  Alert
} from 'antd'
import {
  TableOutlined,
  BarChartOutlined,
  DownloadOutlined,
  FullscreenOutlined,
  SettingOutlined,
  ShareAltOutlined,
  FilterOutlined,
  SearchOutlined,
  PieChartOutlined,
  LineChartOutlined,
  DotChartOutlined
} from '@ant-design/icons'
import { DataTable } from '@shared/components/core/DataTable'
import { Chart } from '@shared/components/core/Chart'
import type { ChatMessage } from '@shared/types/chat'

const { Text, Title } = Typography
const { TabPane } = Tabs
const { Option } = Select

interface QueryResultsViewerProps {
  message: ChatMessage
  onExport?: (format: string, data: any[]) => void
  onShare?: () => void
  height?: number
  defaultView?: 'table' | 'chart'
}

export const QueryResultsViewer: React.FC<QueryResultsViewerProps> = ({
  message,
  onExport,
  onShare,
  height = 500,
  defaultView = 'table'
}) => {
  const [activeTab, setActiveTab] = useState(defaultView)
  const [chartType, setChartType] = useState<'bar' | 'line' | 'pie' | 'area' | 'scatter'>('bar')
  const [showSettings, setShowSettings] = useState(false)
  const [isFullscreen, setIsFullscreen] = useState(false)

  const { results, sql, resultMetadata } = message

  // Process and analyze data
  const processedData = useMemo(() => {
    if (!results || !Array.isArray(results)) return []
    return results
  }, [results])

  const columns = useMemo(() => {
    if (!processedData.length) return []
    return Object.keys(processedData[0]).map(key => ({
      key,
      title: key.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
      dataIndex: key,
      type: inferColumnType(processedData, key),
      sortable: true,
      filterable: true
    }))
  }, [processedData])

  // Infer column types for better visualization
  function inferColumnType(data: any[], column: string): 'text' | 'number' | 'date' | 'boolean' {
    if (!data.length) return 'text'
    
    const sample = data[0][column]
    if (typeof sample === 'number') return 'number'
    if (typeof sample === 'boolean') return 'boolean'
    if (sample instanceof Date || /^\d{4}-\d{2}-\d{2}/.test(sample)) return 'date'
    return 'text'
  }

  // Get suggested chart configurations
  const getChartSuggestions = () => {
    if (!columns.length) return []

    const numericColumns = columns.filter(col => col.type === 'number')
    const textColumns = columns.filter(col => col.type === 'text')
    const dateColumns = columns.filter(col => col.type === 'date')

    const suggestions = []

    // Bar chart for categorical data
    if (textColumns.length > 0 && numericColumns.length > 0) {
      suggestions.push({
        type: 'bar',
        title: `${numericColumns[0].title} by ${textColumns[0].title}`,
        xAxis: textColumns[0].key,
        yAxis: numericColumns[0].key,
        icon: <BarChartOutlined />
      })
    }

    // Line chart for time series
    if (dateColumns.length > 0 && numericColumns.length > 0) {
      suggestions.push({
        type: 'line',
        title: `${numericColumns[0].title} over time`,
        xAxis: dateColumns[0].key,
        yAxis: numericColumns[0].key,
        icon: <LineChartOutlined />
      })
    }

    // Pie chart for categorical distribution
    if (textColumns.length > 0 && numericColumns.length > 0 && processedData.length <= 20) {
      suggestions.push({
        type: 'pie',
        title: `Distribution of ${numericColumns[0].title}`,
        xAxis: textColumns[0].key,
        yAxis: numericColumns[0].key,
        icon: <PieChartOutlined />
      })
    }

    return suggestions
  }

  const chartSuggestions = getChartSuggestions()

  // Export functionality
  const handleExport = (format: string) => {
    if (onExport) {
      onExport(format, processedData)
    } else {
      // Default export logic
      const dataStr = format === 'json' 
        ? JSON.stringify(processedData, null, 2)
        : convertToCSV(processedData)
      
      const blob = new Blob([dataStr], { 
        type: format === 'json' ? 'application/json' : 'text/csv' 
      })
      
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `query_results.${format}`
      link.click()
      URL.revokeObjectURL(url)
    }
  }

  const convertToCSV = (data: any[]) => {
    if (!data.length) return ''
    
    const headers = Object.keys(data[0])
    const csvContent = [
      headers.join(','),
      ...data.map(row => 
        headers.map(header => {
          const value = row[header]
          return typeof value === 'string' && value.includes(',') 
            ? `"${value}"` 
            : value
        }).join(',')
      )
    ].join('\n')
    
    return csvContent
  }

  const exportOptions = [
    { key: 'csv', label: 'Export as CSV', icon: <DownloadOutlined /> },
    { key: 'json', label: 'Export as JSON', icon: <DownloadOutlined /> },
    { key: 'excel', label: 'Export as Excel', icon: <DownloadOutlined /> }
  ]

  if (!results || !Array.isArray(results) || results.length === 0) {
    return (
      <Card style={{ marginTop: 16 }}>
        <Alert
          message="No Results"
          description="The query executed successfully but returned no data."
          type="info"
          showIcon
        />
      </Card>
    )
  }

  return (
    <Card
      style={{ 
        marginTop: 16,
        ...(isFullscreen && {
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          zIndex: 1000,
          margin: 0,
          borderRadius: 0
        })
      }}
      title={
        <Space>
          <Text strong>Query Results</Text>
          <Tag color="blue">{processedData.length} rows</Tag>
          {resultMetadata && (
            <Tag color="green">{resultMetadata.executionTime}ms</Tag>
          )}
        </Space>
      }
      extra={
        <Space>
          <Dropdown
            menu={{
              items: exportOptions.map(option => ({
                key: option.key,
                label: option.label,
                icon: option.icon,
                onClick: () => handleExport(option.key)
              }))
            }}
            trigger={['click']}
          >
            <Button icon={<DownloadOutlined />}>
              Export
            </Button>
          </Dropdown>
          
          {onShare && (
            <Button icon={<ShareAltOutlined />} onClick={onShare}>
              Share
            </Button>
          )}
          
          <Button
            icon={<SettingOutlined />}
            onClick={() => setShowSettings(!showSettings)}
          />
          
          <Button
            icon={<FullscreenOutlined />}
            onClick={() => setIsFullscreen(!isFullscreen)}
          />
        </Space>
      }
      bodyStyle={{ padding: 0 }}
    >
      {/* Quick Stats */}
      {resultMetadata && (
        <div style={{ padding: '16px', borderBottom: '1px solid #f0f0f0' }}>
          <Row gutter={16}>
            <Col span={6}>
              <Statistic
                title="Rows"
                value={resultMetadata.rowCount}
                prefix={<TableOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Columns"
                value={resultMetadata.columnCount}
                prefix={<FilterOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Execution Time"
                value={resultMetadata.executionTime}
                suffix="ms"
                prefix={<SearchOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Complexity"
                value={resultMetadata.queryComplexity}
                valueStyle={{ 
                  color: resultMetadata.queryComplexity === 'Simple' ? '#52c41a' : 
                         resultMetadata.queryComplexity === 'Medium' ? '#faad14' : '#ff4d4f' 
                }}
              />
            </Col>
          </Row>
        </div>
      )}

      {/* Settings Panel */}
      {showSettings && (
        <div style={{ padding: '16px', background: '#fafafa', borderBottom: '1px solid #f0f0f0' }}>
          <Row gutter={16} align="middle">
            <Col span={8}>
              <Space>
                <Text strong>Chart Type:</Text>
                <Select
                  value={chartType}
                  onChange={setChartType}
                  style={{ width: 120 }}
                >
                  <Option value="bar">Bar</Option>
                  <Option value="line">Line</Option>
                  <Option value="pie">Pie</Option>
                  <Option value="area">Area</Option>
                  <Option value="scatter">Scatter</Option>
                </Select>
              </Space>
            </Col>
            <Col span={8}>
              <Space>
                <Text strong>Default View:</Text>
                <Switch
                  checked={activeTab === 'chart'}
                  onChange={(checked) => setActiveTab(checked ? 'chart' : 'table')}
                  checkedChildren="Chart"
                  unCheckedChildren="Table"
                />
              </Space>
            </Col>
            <Col span={8}>
              {chartSuggestions.length > 0 && (
                <Space>
                  <Text strong>Suggestions:</Text>
                  {chartSuggestions.slice(0, 3).map((suggestion, index) => (
                    <Tooltip key={index} title={suggestion.title}>
                      <Button
                        size="small"
                        icon={suggestion.icon}
                        onClick={() => setChartType(suggestion.type as any)}
                      />
                    </Tooltip>
                  ))}
                </Space>
              )}
            </Col>
          </Row>
        </div>
      )}

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        style={{ padding: '0 16px' }}
        items={[
          {
            key: 'table',
            label: (
              <Space>
                <TableOutlined />
                Table View
              </Space>
            ),
            children: (
              <DataTable
                data={processedData}
                columns={columns}
                loading={false}
                pagination={true}
                pageSize={50}
                searchable={true}
                filterable={true}
                exportable={false} // Handled by parent
                height={isFullscreen ? window.innerHeight - 300 : height - 100}
              />
            )
          },
          {
            key: 'chart',
            label: (
              <Space>
                <BarChartOutlined />
                Chart View
              </Space>
            ),
            children: (
              <div style={{ padding: '16px 0' }}>
                <Chart
                  data={processedData}
                  columns={columns.map(col => col.key)}
                  config={{
                    type: chartType,
                    title: `${columns[1]?.title || 'Value'} by ${columns[0]?.title || 'Category'}`,
                    xAxis: columns[0]?.key || 'x',
                    yAxis: columns[1]?.key || 'y',
                    colorScheme: 'default',
                    showGrid: true,
                    showLegend: true,
                    showAnimation: true,
                    interactive: true
                  }}
                  height={isFullscreen ? window.innerHeight - 400 : height - 150}
                />
              </div>
            )
          }
        ]}
      />

      {/* SQL Query Display */}
      {sql && (
        <div style={{ 
          padding: '16px', 
          borderTop: '1px solid #f0f0f0',
          background: '#fafafa'
        }}>
          <Text strong style={{ display: 'block', marginBottom: '8px' }}>
            Executed Query:
          </Text>
          <pre style={{
            background: '#f5f5f5',
            padding: '12px',
            borderRadius: '6px',
            fontSize: '12px',
            fontFamily: 'Monaco, Menlo, monospace',
            margin: 0,
            overflow: 'auto',
            maxHeight: '150px'
          }}>
            {sql}
          </pre>
        </div>
      )}
    </Card>
  )
}
