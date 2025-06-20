/**
 * High-Performance Chart Component
 * 
 * Optimized chart rendering with throttling, memoization, and intelligent updates
 * for smooth performance with large datasets and real-time data.
 */

import React, { useMemo, useCallback, useRef, useEffect } from 'react'
import { Card, Spin, Alert, Button, Space } from 'antd'
import { ReloadOutlined, FullscreenOutlined } from '@ant-design/icons'
import {
  ResponsiveContainer,
  LineChart,
  BarChart,
  AreaChart,
  PieChart,
  ScatterChart,
  Line,
  Bar,
  Area,
  Pie,
  Cell,
  Scatter,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  Brush,
} from 'recharts'
import { useThrottledData, useChartResize, useChartAnimation } from '../../hooks/useChartOptimization'

export interface ChartDataPoint {
  [key: string]: string | number | Date
}

export interface PerformantChartProps {
  /** Chart data */
  data: ChartDataPoint[]
  /** Chart type */
  type: 'line' | 'bar' | 'area' | 'pie' | 'scatter'
  /** Chart title */
  title?: string
  /** Chart height */
  height?: number
  /** Chart width */
  width?: number | string
  /** Loading state */
  loading?: boolean
  /** Error state */
  error?: string | null
  /** X-axis data key */
  xAxisKey?: string
  /** Y-axis data key */
  yAxisKey?: string
  /** Additional data series */
  series?: Array<{
    key: string
    name: string
    color?: string
  }>
  /** Chart configuration */
  config?: {
    showGrid?: boolean
    showLegend?: boolean
    showTooltip?: boolean
    showBrush?: boolean
    enableAnimation?: boolean
    enableZoom?: boolean
    strokeWidth?: number
    fillOpacity?: number
  }
  /** Performance options */
  performance?: {
    throttleMs?: number
    maxDataPoints?: number
    enableVirtualization?: boolean
    updateStrategy?: 'replace' | 'append' | 'smart'
  }
  /** Event handlers */
  onDataPointClick?: (data: ChartDataPoint, index: number) => void
  onRefresh?: () => void
  /** Custom colors */
  colors?: string[]
  /** Responsive breakpoints */
  responsive?: boolean
}

// Default color palette optimized for accessibility
const DEFAULT_COLORS = [
  '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', 
  '#9467bd', '#8c564b', '#e377c2', '#7f7f7f',
  '#bcbd22', '#17becf'
]

// Memoized chart components for better performance
const MemoizedLineChart = React.memo(LineChart)
const MemoizedBarChart = React.memo(BarChart)
const MemoizedAreaChart = React.memo(AreaChart)
const MemoizedPieChart = React.memo(PieChart)
const MemoizedScatterChart = React.memo(ScatterChart)

export const PerformantChart: React.FC<PerformantChartProps> = ({
  data,
  type,
  title,
  height = 300,
  width = '100%',
  loading = false,
  error = null,
  xAxisKey = 'x',
  yAxisKey = 'y',
  series = [],
  config = {},
  performance = {},
  onDataPointClick,
  onRefresh,
  colors = DEFAULT_COLORS,
  responsive = true,
}) => {
  const chartRef = useRef<HTMLDivElement>(null)
  
  // Performance optimizations
  const {
    throttleMs = 1000,
    maxDataPoints = 1000,
    enableVirtualization = true,
    updateStrategy = 'smart',
  } = performance
  
  // Throttle data updates to prevent excessive re-renders
  const throttledData = useThrottledData(data, throttleMs)
  
  // Optimize data for rendering
  const optimizedData = useMemo(() => {
    let processedData = throttledData
    
    // Limit data points for performance
    if (processedData.length > maxDataPoints) {
      const step = Math.ceil(processedData.length / maxDataPoints)
      processedData = processedData.filter((_, index) => index % step === 0)
    }
    
    // Sort data if needed (for time series)
    if (xAxisKey && processedData.length > 0) {
      const firstValue = processedData[0][xAxisKey]
      if (firstValue instanceof Date || typeof firstValue === 'string') {
        processedData = [...processedData].sort((a, b) => {
          const aVal = new Date(a[xAxisKey] as string).getTime()
          const bVal = new Date(b[xAxisKey] as string).getTime()
          return aVal - bVal
        })
      }
    }
    
    return processedData
  }, [throttledData, maxDataPoints, xAxisKey])
  
  // Chart resize handling
  const { dimensions } = useChartResize(chartRef, responsive)
  
  // Animation configuration
  const animationConfig = useChartAnimation(config.enableAnimation !== false)
  
  // Memoized chart configuration
  const chartConfig = useMemo(() => ({
    showGrid: config.showGrid !== false,
    showLegend: config.showLegend !== false,
    showTooltip: config.showTooltip !== false,
    showBrush: config.showBrush === true,
    strokeWidth: config.strokeWidth || 2,
    fillOpacity: config.fillOpacity || 0.6,
    ...animationConfig,
  }), [config, animationConfig])
  
  // Handle data point clicks
  const handleDataPointClick = useCallback((data: any, index: number) => {
    onDataPointClick?.(data, index)
  }, [onDataPointClick])
  
  // Render chart based on type
  const renderChart = useCallback(() => {
    const commonProps = {
      data: optimizedData,
      width: typeof width === 'string' ? undefined : width,
      height,
      onClick: handleDataPointClick,
    }
    
    const axisProps = {
      ...(chartConfig.showGrid && { grid: <CartesianGrid strokeDasharray="3 3" /> }),
      xAxis: <XAxis dataKey={xAxisKey} />,
      yAxis: <YAxis />,
      ...(chartConfig.showTooltip && { tooltip: <Tooltip /> }),
      ...(chartConfig.showLegend && { legend: <Legend /> }),
      ...(chartConfig.showBrush && { brush: <Brush dataKey={xAxisKey} height={30} /> }),
    }
    
    switch (type) {
      case 'line':
        return (
          <MemoizedLineChart {...commonProps}>
            {axisProps.grid}
            {axisProps.xAxis}
            {axisProps.yAxis}
            {axisProps.tooltip}
            {axisProps.legend}
            <Line
              type="monotone"
              dataKey={yAxisKey}
              stroke={colors[0]}
              strokeWidth={chartConfig.strokeWidth}
              dot={optimizedData.length > 100 ? false : undefined}
              animationDuration={chartConfig.animationDuration}
            />
            {series.map((s, index) => (
              <Line
                key={s.key}
                type="monotone"
                dataKey={s.key}
                stroke={s.color || colors[(index + 1) % colors.length]}
                strokeWidth={chartConfig.strokeWidth}
                dot={optimizedData.length > 100 ? false : undefined}
                animationDuration={chartConfig.animationDuration}
              />
            ))}
            {axisProps.brush}
          </MemoizedLineChart>
        )
      
      case 'bar':
        return (
          <MemoizedBarChart {...commonProps}>
            {axisProps.grid}
            {axisProps.xAxis}
            {axisProps.yAxis}
            {axisProps.tooltip}
            {axisProps.legend}
            <Bar
              dataKey={yAxisKey}
              fill={colors[0]}
              animationDuration={chartConfig.animationDuration}
            />
            {series.map((s, index) => (
              <Bar
                key={s.key}
                dataKey={s.key}
                fill={s.color || colors[(index + 1) % colors.length]}
                animationDuration={chartConfig.animationDuration}
              />
            ))}
            {axisProps.brush}
          </MemoizedBarChart>
        )
      
      case 'area':
        return (
          <MemoizedAreaChart {...commonProps}>
            {axisProps.grid}
            {axisProps.xAxis}
            {axisProps.yAxis}
            {axisProps.tooltip}
            {axisProps.legend}
            <Area
              type="monotone"
              dataKey={yAxisKey}
              stroke={colors[0]}
              fill={colors[0]}
              fillOpacity={chartConfig.fillOpacity}
              animationDuration={chartConfig.animationDuration}
            />
            {series.map((s, index) => (
              <Area
                key={s.key}
                type="monotone"
                dataKey={s.key}
                stroke={s.color || colors[(index + 1) % colors.length]}
                fill={s.color || colors[(index + 1) % colors.length]}
                fillOpacity={chartConfig.fillOpacity}
                animationDuration={chartConfig.animationDuration}
              />
            ))}
            {axisProps.brush}
          </MemoizedAreaChart>
        )
      
      case 'pie':
        return (
          <MemoizedPieChart {...commonProps}>
            {axisProps.tooltip}
            {axisProps.legend}
            <Pie
              data={optimizedData}
              dataKey={yAxisKey}
              nameKey={xAxisKey}
              cx="50%"
              cy="50%"
              outerRadius={80}
              animationDuration={chartConfig.animationDuration}
            >
              {optimizedData.map((_, index) => (
                <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
              ))}
            </Pie>
          </MemoizedPieChart>
        )
      
      case 'scatter':
        return (
          <MemoizedScatterChart {...commonProps}>
            {axisProps.grid}
            {axisProps.xAxis}
            {axisProps.yAxis}
            {axisProps.tooltip}
            {axisProps.legend}
            <Scatter
              dataKey={yAxisKey}
              fill={colors[0]}
              animationDuration={chartConfig.animationDuration}
            />
          </MemoizedScatterChart>
        )
      
      default:
        return null
    }
  }, [
    optimizedData,
    type,
    width,
    height,
    xAxisKey,
    yAxisKey,
    series,
    colors,
    chartConfig,
    handleDataPointClick,
  ])
  
  // Handle error state
  if (error) {
    return (
      <Card title={title}>
        <Alert
          message="Chart Error"
          description={error}
          type="error"
          showIcon
          action={
            onRefresh && (
              <Button size="small" icon={<ReloadOutlined />} onClick={onRefresh}>
                Retry
              </Button>
            )
          }
        />
      </Card>
    )
  }
  
  return (
    <Card
      title={title}
      extra={
        <Space>
          {onRefresh && (
            <Button
              type="text"
              size="small"
              icon={<ReloadOutlined />}
              onClick={onRefresh}
              loading={loading}
            />
          )}
        </Space>
      }
    >
      <div ref={chartRef} style={{ width: '100%', height }}>
        <Spin spinning={loading}>
          <ResponsiveContainer width="100%" height="100%">
            {renderChart()}
          </ResponsiveContainer>
        </Spin>
      </div>
      
      {/* Performance info in dev mode */}
      {import.meta.env.DEV && (
        <div style={{ fontSize: '11px', color: '#999', marginTop: 8 }}>
          Rendering {optimizedData.length} of {data.length} data points
          {throttleMs > 0 && ` (throttled: ${throttleMs}ms)`}
        </div>
      )}
    </Card>
  )
}

export default PerformantChart
