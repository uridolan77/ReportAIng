import React, { useState, useMemo } from 'react'
import { Card, Tabs, Space, Button, Switch, Typography, Statistic, Row, Col } from 'antd'
import { 
  TableOutlined, 
  BarChartOutlined, 
  CodeOutlined,
  DownloadOutlined,
  FullscreenOutlined,
  DollarOutlined
} from '@ant-design/icons'
import { VirtualTable, ExportManager, MonacoSQLEditor } from '../core'
import { D3ChartSelector } from '../charts'
import { QueryCostWidget } from '../cost'
import type { ColumnsType } from 'antd/es/table'

const { Title, Text } = Typography

interface QueryResult {
  columns: string[]
  rows: any[][]
  totalRows: number
  executionTime: number
  cost?: number
  sql: string
  metadata?: Record<string, any>
}

interface EnhancedQueryResultsProps {
  result: QueryResult
  loading?: boolean
  onRerun?: (sql: string) => void
  showCost?: boolean
  showAdvancedCharts?: boolean
  allowSqlEdit?: boolean
}

export const EnhancedQueryResults: React.FC<EnhancedQueryResultsProps> = ({
  result,
  loading = false,
  onRerun,
  showCost = true,
  showAdvancedCharts = true,
  allowSqlEdit = true
}) => {
  const [useVirtualScrolling, setUseVirtualScrolling] = useState(result.totalRows > 1000)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [editedSql, setEditedSql] = useState(result.sql)

  // Prepare data for different views
  const tableColumns: ColumnsType<any> = useMemo(() => {
    return result.columns.map((col, index) => ({
      title: col,
      dataIndex: index,
      key: col,
      width: 150,
      ellipsis: true,
      render: (value: any) => {
        if (value === null || value === undefined) return <Text type="secondary">NULL</Text>
        if (typeof value === 'number') return value.toLocaleString()
        return String(value)
      }
    }))
  }, [result.columns])

  const tableData = useMemo(() => {
    return result.rows.map((row, index) => ({
      key: index,
      ...row.reduce((acc, cell, cellIndex) => {
        acc[cellIndex] = cell
        return acc
      }, {} as Record<number, any>)
    }))
  }, [result.rows])

  // Prepare chart data
  const chartData = useMemo(() => {
    if (result.rows.length === 0) return []
    
    return result.rows.slice(0, 50).map((row, index) => ({
      label: String(row[0] || `Row ${index + 1}`),
      value: typeof row[1] === 'number' ? row[1] : index + 1,
      category: result.columns[0] || 'Data',
      metadata: { rowIndex: index }
    }))
  }, [result.rows, result.columns])

  // Export data preparation
  const exportData = {
    headers: result.columns,
    rows: result.rows,
    title: 'Query Results',
    metadata: {
      executionTime: result.executionTime,
      totalRows: result.totalRows,
      cost: result.cost,
      generatedAt: new Date().toISOString()
    }
  }

  const handleSqlRerun = () => {
    if (onRerun && editedSql.trim()) {
      onRerun(editedSql)
    }
  }

  const tabItems = [
    {
      key: 'table',
      label: (
        <Space>
          <TableOutlined />
          Table View
          <Text type="secondary">({result.totalRows.toLocaleString()} rows)</Text>
        </Space>
      ),
      children: (
        <div>
          {/* Table Controls */}
          <div style={{ marginBottom: '16px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Space>
              <Text>Virtual Scrolling:</Text>
              <Switch
                checked={useVirtualScrolling}
                onChange={setUseVirtualScrolling}
                disabled={result.totalRows <= 100}
              />
              <Text type="secondary">
                {useVirtualScrolling ? 'Optimized for large datasets' : 'Standard pagination'}
              </Text>
            </Space>
            
            <Space>
              <ExportManager data={exportData} />
              <Button 
                icon={<FullscreenOutlined />}
                onClick={() => setIsFullscreen(!isFullscreen)}
              >
                {isFullscreen ? 'Exit Fullscreen' : 'Fullscreen'}
              </Button>
            </Space>
          </div>

          {/* Table */}
          {useVirtualScrolling ? (
            <VirtualTable
              columns={tableColumns}
              dataSource={tableData}
              height={isFullscreen ? window.innerHeight - 200 : 500}
              rowHeight={54}
              loading={loading}
            />
          ) : (
            <div style={{ 
              height: isFullscreen ? window.innerHeight - 200 : 500,
              overflow: 'auto',
              border: '1px solid #f0f0f0',
              borderRadius: '6px'
            }}>
              {/* Standard table implementation would go here */}
              <div style={{ padding: '16px' }}>
                <Text>Standard table view for {result.totalRows} rows</Text>
                {/* In a real implementation, this would be an Ant Design Table */}
              </div>
            </div>
          )}
        </div>
      )
    },
    {
      key: 'charts',
      label: (
        <Space>
          <BarChartOutlined />
          Visualizations
        </Space>
      ),
      children: showAdvancedCharts ? (
        <D3ChartSelector data={chartData} />
      ) : (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Text type="secondary">Advanced charts not available</Text>
        </div>
      ),
      disabled: !showAdvancedCharts || chartData.length === 0
    },
    {
      key: 'sql',
      label: (
        <Space>
          <CodeOutlined />
          SQL Query
        </Space>
      ),
      children: (
        <div>
          <MonacoSQLEditor
            value={editedSql}
            onChange={setEditedSql}
            onExecute={handleSqlRerun}
            height={400}
            readOnly={!allowSqlEdit}
            showToolbar={allowSqlEdit}
            placeholder="SQL query will appear here..."
          />
          
          {allowSqlEdit && editedSql !== result.sql && (
            <div style={{ 
              marginTop: '16px', 
              padding: '12px', 
              backgroundColor: '#fff7e6',
              border: '1px solid #ffd591',
              borderRadius: '6px'
            }}>
              <Text type="warning">
                SQL has been modified. Click "Execute" to run the updated query.
              </Text>
            </div>
          )}
        </div>
      ),
      disabled: !allowSqlEdit && !result.sql
    }
  ]

  return (
    <div style={{ 
      position: isFullscreen ? 'fixed' : 'relative',
      top: isFullscreen ? 0 : 'auto',
      left: isFullscreen ? 0 : 'auto',
      right: isFullscreen ? 0 : 'auto',
      bottom: isFullscreen ? 0 : 'auto',
      zIndex: isFullscreen ? 1000 : 'auto',
      backgroundColor: isFullscreen ? 'white' : 'transparent',
      padding: isFullscreen ? '16px' : 0
    }}>
      {/* Results Header */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Execution Time"
              value={result.executionTime}
              suffix="ms"
              precision={2}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Total Rows"
              value={result.totalRows}
              formatter={(value) => value?.toLocaleString()}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Columns"
              value={result.columns.length}
            />
          </Col>
          {showCost && result.cost && (
            <Col xs={24} sm={12} md={6}>
              <Statistic
                title="Query Cost"
                value={result.cost}
                prefix={<DollarOutlined />}
                precision={4}
                formatter={(value) => `$${value}`}
              />
            </Col>
          )}
        </Row>
      </Card>

      {/* Cost Widget */}
      {showCost && (
        <div style={{ marginBottom: '16px' }}>
          <QueryCostWidget
            actualCost={result.cost}
            compact={true}
            showTrend={true}
          />
        </div>
      )}

      {/* Main Results */}
      <Card>
        <Tabs
          defaultActiveKey="table"
          items={tabItems}
          size="large"
          style={{ minHeight: isFullscreen ? 'calc(100vh - 200px)' : '600px' }}
        />
      </Card>

      {/* Fullscreen Overlay */}
      {isFullscreen && (
        <div 
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            zIndex: 999
          }}
          onClick={() => setIsFullscreen(false)}
        />
      )}
    </div>
  )
}
