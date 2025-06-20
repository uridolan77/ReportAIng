/**
 * Performance Dashboard Demonstration
 * 
 * Showcases the new performance optimizations including:
 * - Virtual scrolling for large datasets
 * - Optimized charts with throttling
 * - Web Worker data processing
 * - Enhanced error boundaries
 */

import React, { useState, useMemo } from 'react'
import { Card, Row, Col, Button, Space, Typography, Statistic, Switch, InputNumber } from 'antd'
import { 
  ThunderboltOutlined, 
  DatabaseOutlined, 
  BarChartOutlined,
  ReloadOutlined,
  SettingOutlined,
} from '@ant-design/icons'
import {
  Dashboard,
  VirtualDataTable,
  PerformantChart,
  EnhancedErrorBoundary,
  useDataProcessor,
  useChartPerformance,
} from '@shared/components/core'

const { Title, Text } = Typography

// Generate large dataset for demonstration
const generateLargeDataset = (size: number) => {
  const data = []
  const startDate = new Date('2024-01-01')
  const statuses = ['active', 'inactive', 'pending'] as const
  const regions = ['US', 'EU', 'APAC', 'LATAM'] as const

  for (let i = 0; i < size; i++) {
    const date = new Date(startDate.getTime() + i * 24 * 60 * 60 * 1000)
    data.push({
      id: i + 1,
      date: date.toISOString().split('T')[0],
      timestamp: date.toISOString(),
      queries: Math.floor(Math.random() * 1000) + 100,
      users: Math.floor(Math.random() * 500) + 50,
      revenue: Math.floor(Math.random() * 10000) + 1000,
      cost: Math.floor(Math.random() * 5000) + 500,
      efficiency: Math.random() * 100,
      status: statuses[Math.floor(Math.random() * statuses.length)],
      region: regions[Math.floor(Math.random() * regions.length)],
    })
  }

  return data
}

export default function PerformanceDashboard() {
  const [dataSize, setDataSize] = useState(10000)
  const [enableVirtualization, setEnableVirtualization] = useState(true)
  const [enableThrottling, setEnableThrottling] = useState(true)
  const [enableWebWorker, setEnableWebWorker] = useState(true)
  
  // Generate data
  const rawData = useMemo(() => generateLargeDataset(dataSize), [dataSize])
  
  // Chart performance monitoring
  const { performanceMetrics, startRender, endRender } = useChartPerformance()
  
  // Web Worker data processing
  const { 
    aggregate, 
    filter, 
    isProcessing, 
    activeOperations 
  } = useDataProcessor({ debug: true })
  
  // Process data with Web Worker
  const [processedData, setProcessedData] = useState<any>(null)
  const [processingTime, setProcessingTime] = useState<number>(0)
  
  const handleDataProcessing = async () => {
    if (!enableWebWorker) {
      // Process on main thread (for comparison)
      const start = Date.now()
      const result = rawData.reduce((acc, item) => {
        const region = item.region
        if (region && !acc[region]) {
          acc[region] = { totalQueries: 0, totalRevenue: 0, count: 0 }
        }
        if (region) {
          acc[region].totalQueries += item.queries
          acc[region].totalRevenue += item.revenue
          acc[region].count += 1
        }
        return acc
      }, {} as Record<string, any>)
      
      const processed = Object.entries(result).map(([region, data]: [string, any]) => ({
        region,
        avgQueries: Math.round(data.totalQueries / data.count),
        totalRevenue: data.totalRevenue,
        count: data.count,
      }))
      
      setProcessedData(processed)
      setProcessingTime(Date.now() - start)
    } else {
      // Process with Web Worker
      const start = Date.now()
      try {
        const result = await aggregate(rawData, {
          groupBy: 'region',
          aggregations: {
            queries: 'avg',
            revenue: 'sum',
            users: 'count',
          },
        })
        setProcessedData(result)
        setProcessingTime(Date.now() - start)
      } catch (error) {
        console.error('Web Worker processing failed:', error)
      }
    }
  }
  
  // Chart data (sample for performance testing)
  const chartData = useMemo(() => {
    const sample = rawData.slice(0, Math.min(1000, rawData.length))
    return sample.map(item => ({
      date: item.date || '',
      queries: item.queries,
      users: item.users,
      revenue: item.revenue,
    }))
  }, [rawData])
  
  // Table columns
  const tableColumns = [
    { key: 'id', title: 'ID', dataIndex: 'id' as const, width: 80 },
    { key: 'date', title: 'Date', dataIndex: 'date' as const, width: 120 },
    { key: 'queries', title: 'Queries', dataIndex: 'queries' as const, width: 100 },
    { key: 'users', title: 'Users', dataIndex: 'users' as const, width: 100 },
    { key: 'revenue', title: 'Revenue', dataIndex: 'revenue' as const, width: 120, render: (val: number) => `$${val.toLocaleString()}` },
    { key: 'cost', title: 'Cost', dataIndex: 'cost' as const, width: 120, render: (val: number) => `$${val.toLocaleString()}` },
    { key: 'efficiency', title: 'Efficiency', dataIndex: 'efficiency' as const, width: 100, render: (val: number) => `${val.toFixed(1)}%` },
    { key: 'status', title: 'Status', dataIndex: 'status' as const, width: 100 },
    { key: 'region', title: 'Region', dataIndex: 'region' as const, width: 100 },
  ]
  
  return (
    <Dashboard.Root>
      <Dashboard.Layout
        title="Performance Dashboard"
        subtitle="Demonstration of performance optimizations"
        sections={[
          {
            title: "Performance Controls",
            children: (
              <Card>
                <Row gutter={[16, 16]}>
                  <Col span={6}>
                    <Space direction="vertical">
                      <Text strong>Data Size:</Text>
                      <InputNumber
                        value={dataSize}
                        onChange={(val) => setDataSize(val || 1000)}
                        min={1000}
                        max={100000}
                        step={1000}
                        formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      />
                    </Space>
                  </Col>
                  <Col span={6}>
                    <Space direction="vertical">
                      <Text strong>Virtual Scrolling:</Text>
                      <Switch
                        checked={enableVirtualization}
                        onChange={setEnableVirtualization}
                        checkedChildren="ON"
                        unCheckedChildren="OFF"
                      />
                    </Space>
                  </Col>
                  <Col span={6}>
                    <Space direction="vertical">
                      <Text strong>Chart Throttling:</Text>
                      <Switch
                        checked={enableThrottling}
                        onChange={setEnableThrottling}
                        checkedChildren="ON"
                        unCheckedChildren="OFF"
                      />
                    </Space>
                  </Col>
                  <Col span={6}>
                    <Space direction="vertical">
                      <Text strong>Web Worker:</Text>
                      <Switch
                        checked={enableWebWorker}
                        onChange={setEnableWebWorker}
                        checkedChildren="ON"
                        unCheckedChildren="OFF"
                      />
                    </Space>
                  </Col>
                </Row>
              </Card>
            ),
          },
          {
            title: "Performance Metrics",
            children: (
              <Row gutter={[16, 16]}>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Dataset Size"
                    value={dataSize.toLocaleString()}
                    prefix={<DatabaseOutlined />}
                    suffix="records"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Chart Renders"
                    value={performanceMetrics.renderCount}
                    prefix={<BarChartOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Avg Render Time"
                    value={performanceMetrics.averageRenderTime.toFixed(0)}
                    prefix={<ThunderboltOutlined />}
                    suffix="ms"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Processing Time"
                    value={processingTime}
                    prefix={<SettingOutlined />}
                    suffix="ms"
                  />
                </Col>
              </Row>
            ),
          },
          {
            title: "Virtual Data Table",
            children: (
              <EnhancedErrorBoundary level="component" autoRetry>
                {enableVirtualization ? (
                  <VirtualDataTable
                    data={rawData}
                    columns={tableColumns}
                    height={400}
                    rowHeight={50}
                    searchable
                    exportable
                    selectable
                    rowKey="id"
                  />
                ) : (
                  <Card>
                    <Text type="secondary">
                      Virtual scrolling is disabled. Enable it to view large datasets efficiently.
                    </Text>
                  </Card>
                )}
              </EnhancedErrorBoundary>
            ),
          },
          {
            title: "Performance Charts",
            children: (
              <Row gutter={[16, 16]}>
                <Col span={12}>
                  <EnhancedErrorBoundary level="widget">
                    <PerformantChart
                      data={chartData}
                      type="line"
                      title="Query Volume Trend"
                      xAxisKey="date"
                      yAxisKey="queries"
                      height={300}
                      performance={{
                        throttleMs: enableThrottling ? 1000 : 0,
                        maxDataPoints: 500,
                        enableVirtualization: true,
                      }}
                      onDataPointClick={(data, index) => {
                        console.log('Chart clicked:', data, index)
                      }}
                    />
                  </EnhancedErrorBoundary>
                </Col>
                <Col span={12}>
                  <EnhancedErrorBoundary level="widget">
                    <PerformantChart
                      data={chartData}
                      type="bar"
                      title="Revenue by Day"
                      xAxisKey="date"
                      yAxisKey="revenue"
                      height={300}
                      performance={{
                        throttleMs: enableThrottling ? 1000 : 0,
                        maxDataPoints: 500,
                      }}
                    />
                  </EnhancedErrorBoundary>
                </Col>
              </Row>
            ),
          },
          {
            title: "Web Worker Data Processing",
            children: (
              <Card>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Space>
                    <Button
                      type="primary"
                      icon={<ReloadOutlined />}
                      onClick={handleDataProcessing}
                      loading={isProcessing}
                    >
                      Process Data {enableWebWorker ? '(Web Worker)' : '(Main Thread)'}
                    </Button>
                    <Text type="secondary">
                      {activeOperations.length > 0 && `${activeOperations.length} operations running`}
                    </Text>
                  </Space>
                  
                  {processedData && (
                    <div>
                      <Text strong>Processed Results (by Region):</Text>
                      <pre style={{ background: '#f5f5f5', padding: '12px', borderRadius: '4px', marginTop: '8px' }}>
                        {JSON.stringify(processedData, null, 2)}
                      </pre>
                    </div>
                  )}
                </Space>
              </Card>
            ),
          },
        ]}
      />
    </Dashboard.Root>
  )
}
