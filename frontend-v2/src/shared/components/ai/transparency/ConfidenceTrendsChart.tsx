import React, { useEffect, useRef, useState } from 'react'
import { Card, Select, Space, Typography, Spin, Alert, Button, Tooltip } from 'antd'
import { LineChartOutlined, ReloadOutlined, InfoCircleOutlined } from '@ant-design/icons'
import * as d3 from 'd3'

const { Title, Text } = Typography
const { Option } = Select

export interface ConfidenceTrendsChartProps {
  data?: Array<{
    date: string
    confidence: number
    traceCount: number
    avgTokens?: number
  }>
  timeRange?: 'day' | 'week' | 'month'
  height?: number
  onTimeRangeChange?: (range: 'day' | 'week' | 'month') => void
  loading?: boolean
  className?: string
}

/**
 * ConfidenceTrendsChart - D3.js visualization for confidence trends over time
 * 
 * Features:
 * - Interactive line chart with confidence trends
 * - Hover tooltips with detailed information
 * - Time range selection (day/week/month)
 * - Responsive design
 * - Confidence threshold indicators
 * - Trace count correlation
 */
export const ConfidenceTrendsChart: React.FC<ConfidenceTrendsChartProps> = ({
  data = [],
  timeRange = 'week',
  height = 400,
  onTimeRangeChange,
  loading = false,
  className
}) => {
  const svgRef = useRef<SVGSVGElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const [dimensions, setDimensions] = useState({ width: 800, height })

  // Update dimensions on resize
  useEffect(() => {
    const updateDimensions = () => {
      if (containerRef.current) {
        const { width } = containerRef.current.getBoundingClientRect()
        setDimensions({ width: width - 32, height }) // Account for padding
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
    svg.selectAll('*').remove() // Clear previous chart

    const margin = { top: 20, right: 30, bottom: 40, left: 50 }
    const width = dimensions.width - margin.left - margin.right
    const chartHeight = dimensions.height - margin.top - margin.bottom

    // Create main group
    const g = svg
      .append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`)

    // Parse dates and prepare data
    const parseDate = d3.timeParse('%Y-%m-%d')
    const processedData = data.map(d => ({
      ...d,
      date: parseDate(d.date.split('T')[0]) || new Date(),
      confidence: d.confidence * 100 // Convert to percentage
    })).sort((a, b) => a.date.getTime() - b.date.getTime())

    // Create scales
    const xScale = d3.scaleTime()
      .domain(d3.extent(processedData, d => d.date) as [Date, Date])
      .range([0, width])

    const yScale = d3.scaleLinear()
      .domain([0, 100]) // Confidence percentage
      .range([chartHeight, 0])

    const traceCountScale = d3.scaleLinear()
      .domain(d3.extent(processedData, d => d.traceCount) as [number, number])
      .range([2, 8]) // Circle radius range

    // Create line generator
    const line = d3.line<typeof processedData[0]>()
      .x(d => xScale(d.date))
      .y(d => yScale(d.confidence))
      .curve(d3.curveMonotoneX)

    // Add gradient definition
    const gradient = svg.append('defs')
      .append('linearGradient')
      .attr('id', 'confidence-gradient')
      .attr('gradientUnits', 'userSpaceOnUse')
      .attr('x1', 0).attr('y1', chartHeight)
      .attr('x2', 0).attr('y2', 0)

    gradient.append('stop')
      .attr('offset', '0%')
      .attr('stop-color', '#ff4d4f')
      .attr('stop-opacity', 0.1)

    gradient.append('stop')
      .attr('offset', '60%')
      .attr('stop-color', '#faad14')
      .attr('stop-opacity', 0.3)

    gradient.append('stop')
      .attr('offset', '100%')
      .attr('stop-color', '#52c41a')
      .attr('stop-opacity', 0.5)

    // Add confidence threshold lines
    const thresholds = [
      { value: 80, label: 'High Confidence', color: '#52c41a' },
      { value: 60, label: 'Medium Confidence', color: '#faad14' },
      { value: 40, label: 'Low Confidence', color: '#ff4d4f' }
    ]

    thresholds.forEach(threshold => {
      g.append('line')
        .attr('x1', 0)
        .attr('x2', width)
        .attr('y1', yScale(threshold.value))
        .attr('y2', yScale(threshold.value))
        .attr('stroke', threshold.color)
        .attr('stroke-dasharray', '3,3')
        .attr('stroke-opacity', 0.5)

      g.append('text')
        .attr('x', width - 5)
        .attr('y', yScale(threshold.value) - 5)
        .attr('text-anchor', 'end')
        .attr('font-size', '10px')
        .attr('fill', threshold.color)
        .text(`${threshold.value}%`)
    })

    // Add area under the line
    const area = d3.area<typeof processedData[0]>()
      .x(d => xScale(d.date))
      .y0(chartHeight)
      .y1(d => yScale(d.confidence))
      .curve(d3.curveMonotoneX)

    g.append('path')
      .datum(processedData)
      .attr('fill', 'url(#confidence-gradient)')
      .attr('d', area)

    // Add the confidence line
    g.append('path')
      .datum(processedData)
      .attr('fill', 'none')
      .attr('stroke', '#1890ff')
      .attr('stroke-width', 3)
      .attr('d', line)

    // Add data points
    const tooltip = d3.select('body').append('div')
      .attr('class', 'tooltip')
      .style('position', 'absolute')
      .style('visibility', 'hidden')
      .style('background', 'rgba(0, 0, 0, 0.8)')
      .style('color', 'white')
      .style('padding', '8px')
      .style('border-radius', '4px')
      .style('font-size', '12px')
      .style('pointer-events', 'none')
      .style('z-index', 1000)

    g.selectAll('.dot')
      .data(processedData)
      .enter().append('circle')
      .attr('class', 'dot')
      .attr('cx', d => xScale(d.date))
      .attr('cy', d => yScale(d.confidence))
      .attr('r', d => traceCountScale(d.traceCount))
      .attr('fill', '#1890ff')
      .attr('stroke', '#fff')
      .attr('stroke-width', 2)
      .style('cursor', 'pointer')
      .on('mouseover', function(event, d) {
        d3.select(this)
          .transition()
          .duration(200)
          .attr('r', traceCountScale(d.traceCount) + 2)

        tooltip
          .style('visibility', 'visible')
          .html(`
            <div><strong>Date:</strong> ${d3.timeFormat('%Y-%m-%d')(d.date)}</div>
            <div><strong>Confidence:</strong> ${d.confidence.toFixed(1)}%</div>
            <div><strong>Traces:</strong> ${d.traceCount}</div>
            ${d.avgTokens ? `<div><strong>Avg Tokens:</strong> ${d.avgTokens}</div>` : ''}
          `)
      })
      .on('mousemove', function(event) {
        tooltip
          .style('top', (event.pageY - 10) + 'px')
          .style('left', (event.pageX + 10) + 'px')
      })
      .on('mouseout', function(event, d) {
        d3.select(this)
          .transition()
          .duration(200)
          .attr('r', traceCountScale(d.traceCount))

        tooltip.style('visibility', 'hidden')
      })

    // Add axes
    const xAxis = d3.axisBottom(xScale)
      .tickFormat(d3.timeFormat(timeRange === 'day' ? '%H:%M' : timeRange === 'week' ? '%m/%d' : '%m/%Y'))

    const yAxis = d3.axisLeft(yScale)
      .tickFormat(d => `${d}%`)

    g.append('g')
      .attr('transform', `translate(0,${chartHeight})`)
      .call(xAxis)
      .append('text')
      .attr('x', width / 2)
      .attr('y', 35)
      .attr('fill', 'black')
      .style('text-anchor', 'middle')
      .text('Time')

    g.append('g')
      .call(yAxis)
      .append('text')
      .attr('transform', 'rotate(-90)')
      .attr('y', -35)
      .attr('x', -chartHeight / 2)
      .attr('fill', 'black')
      .style('text-anchor', 'middle')
      .text('Confidence (%)')

    // Cleanup function
    return () => {
      tooltip.remove()
    }

  }, [data, dimensions, timeRange, loading])

  if (loading) {
    return (
      <Card className={className}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text type="secondary">Loading confidence trends...</Text>
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
          description="No confidence trend data found for the selected time range."
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
          <LineChartOutlined />
          <span>Confidence Trends</span>
        </Space>
      }
      className={className}
      extra={
        <Space>
          <Select
            value={timeRange}
            onChange={onTimeRangeChange}
            style={{ width: 100 }}
          >
            <Option value="day">Day</Option>
            <Option value="week">Week</Option>
            <Option value="month">Month</Option>
          </Select>
          <Tooltip title="Circle size represents trace count">
            <InfoCircleOutlined />
          </Tooltip>
          <Button size="small" icon={<ReloadOutlined />}>
            Refresh
          </Button>
        </Space>
      }
    >
      <div ref={containerRef} style={{ width: '100%' }}>
        <svg
          ref={svgRef}
          width={dimensions.width}
          height={dimensions.height}
          style={{ display: 'block' }}
        />
      </div>
      
      {/* Legend */}
      <div style={{ marginTop: 16, display: 'flex', justifyContent: 'center', gap: '20px' }}>
        <Space>
          <div style={{ width: 12, height: 3, backgroundColor: '#52c41a' }} />
          <Text type="secondary" style={{ fontSize: '12px' }}>High (80%+)</Text>
        </Space>
        <Space>
          <div style={{ width: 12, height: 3, backgroundColor: '#faad14' }} />
          <Text type="secondary" style={{ fontSize: '12px' }}>Medium (60-80%)</Text>
        </Space>
        <Space>
          <div style={{ width: 12, height: 3, backgroundColor: '#ff4d4f' }} />
          <Text type="secondary" style={{ fontSize: '12px' }}>Low (&lt;60%)</Text>
        </Space>
      </div>
    </Card>
  )
}

export default ConfidenceTrendsChart
