import React, { useEffect, useRef, useState } from 'react'
import { Card, Select, Space, Typography, Spin, Alert, Button, Row, Col, Statistic } from 'antd'
import { BarChartOutlined, ReloadOutlined, DollarOutlined, ThunderboltOutlined } from '@ant-design/icons'
import * as d3 from 'd3'

const { Title, Text } = Typography
const { Option } = Select

export interface TokenUsageChartProps {
  data?: Array<{
    date: string
    totalTokens: number
    inputTokens: number
    outputTokens: number
    cost?: number
    model?: string
  }>
  chartType?: 'bar' | 'area' | 'stacked'
  timeRange?: 'day' | 'week' | 'month'
  height?: number
  onChartTypeChange?: (type: 'bar' | 'area' | 'stacked') => void
  onTimeRangeChange?: (range: 'day' | 'week' | 'month') => void
  loading?: boolean
  className?: string
}

/**
 * TokenUsageChart - D3.js visualization for token usage patterns
 * 
 * Features:
 * - Multiple chart types (bar, area, stacked)
 * - Input/output token breakdown
 * - Cost analysis integration
 * - Model-specific filtering
 * - Interactive tooltips
 * - Usage statistics summary
 */
export const TokenUsageChart: React.FC<TokenUsageChartProps> = ({
  data = [],
  chartType = 'stacked',
  timeRange = 'week',
  height = 400,
  onChartTypeChange,
  onTimeRangeChange,
  loading = false,
  className
}) => {
  const svgRef = useRef<SVGSVGElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const [dimensions, setDimensions] = useState({ width: 800, height })

  // Calculate summary statistics
  const stats = React.useMemo(() => {
    if (!data.length) return { totalTokens: 0, avgTokens: 0, totalCost: 0, efficiency: 0 }

    const totalTokens = data.reduce((sum, d) => sum + d.totalTokens, 0)
    const avgTokens = Math.round(totalTokens / data.length)
    const totalCost = data.reduce((sum, d) => sum + (d.cost || 0), 0)
    const efficiency = totalTokens > 0 ? (totalCost / totalTokens * 1000) : 0 // Cost per 1K tokens

    return { totalTokens, avgTokens, totalCost, efficiency }
  }, [data])

  // Update dimensions on resize
  useEffect(() => {
    const updateDimensions = () => {
      if (containerRef.current) {
        const { width } = containerRef.current.getBoundingClientRect()
        setDimensions({ width: width - 32, height })
      }
    }

    updateDimensions()
    window.addEventListener('resize', updateDimensions)
    return () => window.removeEventListener('resize', updateDimensions)
  }, [height])

  // Create D3 visualization
  useEffect(() => {
    if (!data.length || loading || !svgRef.current) return

    const svg = d3.select(svgRef.current)
    svg.selectAll('*').remove()

    const margin = { top: 20, right: 30, bottom: 40, left: 60 }
    const width = dimensions.width - margin.left - margin.right
    const chartHeight = dimensions.height - margin.top - margin.bottom

    const g = svg
      .append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`)

    // Parse dates and prepare data
    const parseDate = d3.timeParse('%Y-%m-%d')
    const processedData = data.map(d => ({
      ...d,
      date: parseDate(d.date.split('T')[0]) || new Date()
    })).sort((a, b) => a.date.getTime() - b.date.getTime())

    // Create scales
    const xScale = d3.scaleBand()
      .domain(processedData.map(d => d3.timeFormat('%m/%d')(d.date)))
      .range([0, width])
      .padding(0.1)

    const maxTokens = d3.max(processedData, d => d.totalTokens) || 0
    const yScale = d3.scaleLinear()
      .domain([0, maxTokens * 1.1])
      .range([chartHeight, 0])

    // Color scales
    const inputColor = '#1890ff'
    const outputColor = '#52c41a'
    const totalColor = '#722ed1'

    // Create tooltip
    const tooltip = d3.select('body').append('div')
      .attr('class', 'token-tooltip')
      .style('position', 'absolute')
      .style('visibility', 'hidden')
      .style('background', 'rgba(0, 0, 0, 0.8)')
      .style('color', 'white')
      .style('padding', '8px')
      .style('border-radius', '4px')
      .style('font-size', '12px')
      .style('pointer-events', 'none')
      .style('z-index', 1000)

    if (chartType === 'stacked') {
      // Stacked bar chart
      const stack = d3.stack<typeof processedData[0]>()
        .keys(['inputTokens', 'outputTokens'])
        .order(d3.stackOrderNone)
        .offset(d3.stackOffsetNone)

      const stackedData = stack(processedData)

      const colorScale = d3.scaleOrdinal()
        .domain(['inputTokens', 'outputTokens'])
        .range([inputColor, outputColor])

      g.selectAll('.layer')
        .data(stackedData)
        .enter().append('g')
        .attr('class', 'layer')
        .attr('fill', d => colorScale(d.key) as string)
        .selectAll('rect')
        .data(d => d)
        .enter().append('rect')
        .attr('x', (d, i) => xScale(d3.timeFormat('%m/%d')(processedData[i].date)) || 0)
        .attr('y', d => yScale(d[1]))
        .attr('height', d => yScale(d[0]) - yScale(d[1]))
        .attr('width', xScale.bandwidth())
        .style('cursor', 'pointer')
        .on('mouseover', function(event, d) {
          const dataIndex = stackedData[0].indexOf(d)
          const dataPoint = processedData[dataIndex]
          
          tooltip
            .style('visibility', 'visible')
            .html(`
              <div><strong>Date:</strong> ${d3.timeFormat('%Y-%m-%d')(dataPoint.date)}</div>
              <div><strong>Total Tokens:</strong> ${dataPoint.totalTokens.toLocaleString()}</div>
              <div><strong>Input:</strong> ${dataPoint.inputTokens.toLocaleString()}</div>
              <div><strong>Output:</strong> ${dataPoint.outputTokens.toLocaleString()}</div>
              ${dataPoint.cost ? `<div><strong>Cost:</strong> $${dataPoint.cost.toFixed(4)}</div>` : ''}
              ${dataPoint.model ? `<div><strong>Model:</strong> ${dataPoint.model}</div>` : ''}
            `)
        })
        .on('mousemove', function(event) {
          tooltip
            .style('top', (event.pageY - 10) + 'px')
            .style('left', (event.pageX + 10) + 'px')
        })
        .on('mouseout', function() {
          tooltip.style('visibility', 'hidden')
        })

    } else if (chartType === 'area') {
      // Area chart
      const area = d3.area<typeof processedData[0]>()
        .x(d => (xScale(d3.timeFormat('%m/%d')(d.date)) || 0) + xScale.bandwidth() / 2)
        .y0(chartHeight)
        .y1(d => yScale(d.totalTokens))
        .curve(d3.curveMonotoneX)

      // Add gradient
      const gradient = svg.append('defs')
        .append('linearGradient')
        .attr('id', 'token-gradient')
        .attr('gradientUnits', 'userSpaceOnUse')
        .attr('x1', 0).attr('y1', chartHeight)
        .attr('x2', 0).attr('y2', 0)

      gradient.append('stop')
        .attr('offset', '0%')
        .attr('stop-color', totalColor)
        .attr('stop-opacity', 0.1)

      gradient.append('stop')
        .attr('offset', '100%')
        .attr('stop-color', totalColor)
        .attr('stop-opacity', 0.8)

      g.append('path')
        .datum(processedData)
        .attr('fill', 'url(#token-gradient)')
        .attr('d', area)

      // Add line
      const line = d3.line<typeof processedData[0]>()
        .x(d => (xScale(d3.timeFormat('%m/%d')(d.date)) || 0) + xScale.bandwidth() / 2)
        .y(d => yScale(d.totalTokens))
        .curve(d3.curveMonotoneX)

      g.append('path')
        .datum(processedData)
        .attr('fill', 'none')
        .attr('stroke', totalColor)
        .attr('stroke-width', 3)
        .attr('d', line)

      // Add data points
      g.selectAll('.dot')
        .data(processedData)
        .enter().append('circle')
        .attr('class', 'dot')
        .attr('cx', d => (xScale(d3.timeFormat('%m/%d')(d.date)) || 0) + xScale.bandwidth() / 2)
        .attr('cy', d => yScale(d.totalTokens))
        .attr('r', 4)
        .attr('fill', totalColor)
        .attr('stroke', '#fff')
        .attr('stroke-width', 2)
        .style('cursor', 'pointer')
        .on('mouseover', function(event, d) {
          tooltip
            .style('visibility', 'visible')
            .html(`
              <div><strong>Date:</strong> ${d3.timeFormat('%Y-%m-%d')(d.date)}</div>
              <div><strong>Total Tokens:</strong> ${d.totalTokens.toLocaleString()}</div>
              <div><strong>Input:</strong> ${d.inputTokens.toLocaleString()}</div>
              <div><strong>Output:</strong> ${d.outputTokens.toLocaleString()}</div>
              ${d.cost ? `<div><strong>Cost:</strong> $${d.cost.toFixed(4)}</div>` : ''}
            `)
        })
        .on('mousemove', function(event) {
          tooltip
            .style('top', (event.pageY - 10) + 'px')
            .style('left', (event.pageX + 10) + 'px')
        })
        .on('mouseout', function() {
          tooltip.style('visibility', 'hidden')
        })

    } else {
      // Regular bar chart
      g.selectAll('.bar')
        .data(processedData)
        .enter().append('rect')
        .attr('class', 'bar')
        .attr('x', d => xScale(d3.timeFormat('%m/%d')(d.date)) || 0)
        .attr('y', d => yScale(d.totalTokens))
        .attr('width', xScale.bandwidth())
        .attr('height', d => chartHeight - yScale(d.totalTokens))
        .attr('fill', totalColor)
        .style('cursor', 'pointer')
        .on('mouseover', function(event, d) {
          d3.select(this).attr('fill', d3.color(totalColor)?.darker(0.5)?.toString() || totalColor)
          
          tooltip
            .style('visibility', 'visible')
            .html(`
              <div><strong>Date:</strong> ${d3.timeFormat('%Y-%m-%d')(d.date)}</div>
              <div><strong>Total Tokens:</strong> ${d.totalTokens.toLocaleString()}</div>
              <div><strong>Input:</strong> ${d.inputTokens.toLocaleString()}</div>
              <div><strong>Output:</strong> ${d.outputTokens.toLocaleString()}</div>
              ${d.cost ? `<div><strong>Cost:</strong> $${d.cost.toFixed(4)}</div>` : ''}
            `)
        })
        .on('mousemove', function(event) {
          tooltip
            .style('top', (event.pageY - 10) + 'px')
            .style('left', (event.pageX + 10) + 'px')
        })
        .on('mouseout', function() {
          d3.select(this).attr('fill', totalColor)
          tooltip.style('visibility', 'hidden')
        })
    }

    // Add axes
    const xAxis = d3.axisBottom(xScale)
    const yAxis = d3.axisLeft(yScale)
      .tickFormat(d => `${((d as number) / 1000).toFixed(0)}K`)

    g.append('g')
      .attr('transform', `translate(0,${chartHeight})`)
      .call(xAxis)
      .append('text')
      .attr('x', width / 2)
      .attr('y', 35)
      .attr('fill', 'black')
      .style('text-anchor', 'middle')
      .text('Date')

    g.append('g')
      .call(yAxis)
      .append('text')
      .attr('transform', 'rotate(-90)')
      .attr('y', -40)
      .attr('x', -chartHeight / 2)
      .attr('fill', 'black')
      .style('text-anchor', 'middle')
      .text('Tokens')

    // Cleanup
    return () => {
      tooltip.remove()
    }

  }, [data, dimensions, chartType, loading])

  if (loading) {
    return (
      <Card className={className}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text type="secondary">Loading token usage data...</Text>
          </div>
        </div>
      </Card>
    )
  }

  if (!data.length) {
    return (
      <Card className={className}>
        <Alert
          message="No Data Available"
          description="No token usage data found for the selected time range."
          type="info"
          showIcon
        />
      </Card>
    )
  }

  return (
    <Card
      title={
        <Space>
          <BarChartOutlined />
          <span>Token Usage Analytics</span>
        </Space>
      }
      className={className}
      extra={
        <Space>
          <Select
            value={chartType}
            onChange={onChartTypeChange}
            style={{ width: 100 }}
          >
            <Option value="stacked">Stacked</Option>
            <Option value="area">Area</Option>
            <Option value="bar">Bar</Option>
          </Select>
          <Select
            value={timeRange}
            onChange={onTimeRangeChange}
            style={{ width: 100 }}
          >
            <Option value="day">Day</Option>
            <Option value="week">Week</Option>
            <Option value="month">Month</Option>
          </Select>
          <Button size="small" icon={<ReloadOutlined />}>
            Refresh
          </Button>
        </Space>
      }
    >
      {/* Summary Statistics */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={6}>
          <Statistic
            title="Total Tokens"
            value={stats.totalTokens}
            formatter={(value) => `${(Number(value) / 1000).toFixed(1)}K`}
            prefix={<ThunderboltOutlined />}
          />
        </Col>
        <Col span={6}>
          <Statistic
            title="Avg per Day"
            value={stats.avgTokens}
            formatter={(value) => `${(Number(value) / 1000).toFixed(1)}K`}
          />
        </Col>
        <Col span={6}>
          <Statistic
            title="Total Cost"
            value={stats.totalCost}
            precision={4}
            prefix={<DollarOutlined />}
          />
        </Col>
        <Col span={6}>
          <Statistic
            title="Cost/1K Tokens"
            value={stats.efficiency}
            precision={4}
            prefix="$"
          />
        </Col>
      </Row>

      {/* Chart */}
      <div ref={containerRef} style={{ width: '100%' }}>
        <svg
          ref={svgRef}
          width={dimensions.width}
          height={dimensions.height}
          style={{ display: 'block' }}
        />
      </div>

      {/* Legend */}
      {chartType === 'stacked' && (
        <div style={{ marginTop: 16, display: 'flex', justifyContent: 'center', gap: '20px' }}>
          <Space>
            <div style={{ width: 12, height: 12, backgroundColor: '#1890ff' }} />
            <Text type="secondary" style={{ fontSize: '12px' }}>Input Tokens</Text>
          </Space>
          <Space>
            <div style={{ width: 12, height: 12, backgroundColor: '#52c41a' }} />
            <Text type="secondary" style={{ fontSize: '12px' }}>Output Tokens</Text>
          </Space>
        </div>
      )}
    </Card>
  )
}

export default TokenUsageChart
