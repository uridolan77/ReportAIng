import React, { useRef, useEffect, useState } from 'react'
import { Card, Select, Space, Typography, Tooltip } from 'antd'
import { InfoCircleOutlined } from '@ant-design/icons'

const { Title, Text } = Typography

// D3.js would be imported here in a real implementation
// import * as d3 from 'd3'

interface ChartData {
  label: string
  value: number
  category?: string
  date?: string
  metadata?: Record<string, any>
}

interface D3ChartProps {
  data: ChartData[]
  width?: number
  height?: number
  margin?: { top: number; right: number; bottom: number; left: number }
  title?: string
  subtitle?: string
  interactive?: boolean
  theme?: 'light' | 'dark'
}

// Advanced Sunburst Chart Component
export const D3SunburstChart: React.FC<D3ChartProps> = ({
  data,
  width = 400,
  height = 400,
  title = 'Sunburst Chart',
  interactive = true,
  theme = 'light'
}) => {
  const svgRef = useRef<SVGSVGElement>(null)
  const [selectedSegment, setSelectedSegment] = useState<string | null>(null)

  useEffect(() => {
    if (!svgRef.current || !data.length) return

    // Clear previous chart
    const svg = svgRef.current
    svg.innerHTML = ''

    // In a real implementation, this would use D3.js
    // const radius = Math.min(width, height) / 2
    // const color = d3.scaleOrdinal(d3.schemeCategory10)
    
    // Mock D3 implementation
    const mockD3Implementation = () => {
      // Create SVG structure
      const centerX = width / 2
      const centerY = height / 2
      const radius = Math.min(width, height) / 2 - 20

      // Create mock sunburst segments
      data.forEach((item, index) => {
        const angle = (index / data.length) * 2 * Math.PI
        const nextAngle = ((index + 1) / data.length) * 2 * Math.PI
        
        // Create path element (simplified)
        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path')
        const innerRadius = radius * 0.3
        const outerRadius = radius * 0.8
        
        // Simplified arc path
        const x1 = centerX + Math.cos(angle) * innerRadius
        const y1 = centerY + Math.sin(angle) * innerRadius
        const x2 = centerX + Math.cos(nextAngle) * innerRadius
        const y2 = centerY + Math.sin(nextAngle) * innerRadius
        const x3 = centerX + Math.cos(nextAngle) * outerRadius
        const y3 = centerY + Math.sin(nextAngle) * outerRadius
        const x4 = centerX + Math.cos(angle) * outerRadius
        const y4 = centerY + Math.sin(angle) * outerRadius
        
        const pathData = `M ${x1} ${y1} L ${x4} ${y4} A ${outerRadius} ${outerRadius} 0 0 1 ${x3} ${y3} L ${x2} ${y2} A ${innerRadius} ${innerRadius} 0 0 0 ${x1} ${y1} Z`
        
        path.setAttribute('d', pathData)
        path.setAttribute('fill', `hsl(${(index * 360) / data.length}, 70%, 60%)`)
        path.setAttribute('stroke', theme === 'dark' ? '#333' : '#fff')
        path.setAttribute('stroke-width', '2')
        path.style.cursor = 'pointer'
        
        if (interactive) {
          path.addEventListener('mouseenter', () => {
            path.setAttribute('opacity', '0.8')
            setSelectedSegment(item.label)
          })
          
          path.addEventListener('mouseleave', () => {
            path.setAttribute('opacity', '1')
            setSelectedSegment(null)
          })
        }
        
        svg.appendChild(path)
        
        // Add labels
        const labelAngle = (angle + nextAngle) / 2
        const labelRadius = (innerRadius + outerRadius) / 2
        const labelX = centerX + Math.cos(labelAngle) * labelRadius
        const labelY = centerY + Math.sin(labelAngle) * labelRadius
        
        const text = document.createElementNS('http://www.w3.org/2000/svg', 'text')
        text.setAttribute('x', labelX.toString())
        text.setAttribute('y', labelY.toString())
        text.setAttribute('text-anchor', 'middle')
        text.setAttribute('dominant-baseline', 'middle')
        text.setAttribute('fill', theme === 'dark' ? '#fff' : '#333')
        text.setAttribute('font-size', '12')
        text.textContent = item.label.length > 8 ? item.label.substring(0, 8) + '...' : item.label
        
        svg.appendChild(text)
      })
    }

    mockD3Implementation()
  }, [data, width, height, interactive, theme])

  return (
    <Card title={title}>
      <div style={{ textAlign: 'center' }}>
        <svg
          ref={svgRef}
          width={width}
          height={height}
          style={{ border: '1px solid #f0f0f0', borderRadius: '4px' }}
        />
        {selectedSegment && (
          <div style={{ marginTop: '8px' }}>
            <Text type="secondary">Selected: {selectedSegment}</Text>
          </div>
        )}
      </div>
    </Card>
  )
}

// Advanced Network Graph Component
export const D3NetworkGraph: React.FC<{
  nodes: Array<{ id: string; label: string; group?: number; value?: number }>
  links: Array<{ source: string; target: string; value?: number }>
  width?: number
  height?: number
  title?: string
}> = ({
  nodes,
  links,
  width = 600,
  height = 400,
  title = 'Network Graph'
}) => {
  const svgRef = useRef<SVGSVGElement>(null)

  useEffect(() => {
    if (!svgRef.current || !nodes.length) return

    const svg = svgRef.current
    svg.innerHTML = ''

    // Mock D3 force simulation
    const mockNetworkGraph = () => {
      // Position nodes in a circle for demo
      nodes.forEach((node, index) => {
        const angle = (index / nodes.length) * 2 * Math.PI
        const radius = Math.min(width, height) / 3
        const x = width / 2 + Math.cos(angle) * radius
        const y = height / 2 + Math.sin(angle) * radius

        // Draw links first (so they appear behind nodes)
        links.forEach(link => {
          if (link.source === node.id) {
            const targetIndex = nodes.findIndex(n => n.id === link.target)
            if (targetIndex !== -1) {
              const targetAngle = (targetIndex / nodes.length) * 2 * Math.PI
              const targetX = width / 2 + Math.cos(targetAngle) * radius
              const targetY = height / 2 + Math.sin(targetAngle) * radius

              const line = document.createElementNS('http://www.w3.org/2000/svg', 'line')
              line.setAttribute('x1', x.toString())
              line.setAttribute('y1', y.toString())
              line.setAttribute('x2', targetX.toString())
              line.setAttribute('y2', targetY.toString())
              line.setAttribute('stroke', '#999')
              line.setAttribute('stroke-width', (link.value || 1).toString())
              line.setAttribute('opacity', '0.6')
              
              svg.appendChild(line)
            }
          }
        })

        // Draw node
        const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle')
        circle.setAttribute('cx', x.toString())
        circle.setAttribute('cy', y.toString())
        circle.setAttribute('r', (10 + (node.value || 0) * 2).toString())
        circle.setAttribute('fill', `hsl(${(node.group || 0) * 60}, 70%, 60%)`)
        circle.setAttribute('stroke', '#fff')
        circle.setAttribute('stroke-width', '2')
        circle.style.cursor = 'pointer'
        
        svg.appendChild(circle)

        // Add label
        const text = document.createElementNS('http://www.w3.org/2000/svg', 'text')
        text.setAttribute('x', x.toString())
        text.setAttribute('y', (y + 25).toString())
        text.setAttribute('text-anchor', 'middle')
        text.setAttribute('font-size', '12')
        text.setAttribute('fill', '#333')
        text.textContent = node.label
        
        svg.appendChild(text)
      })
    }

    mockNetworkGraph()
  }, [nodes, links, width, height])

  return (
    <Card title={title}>
      <svg
        ref={svgRef}
        width={width}
        height={height}
        style={{ border: '1px solid #f0f0f0', borderRadius: '4px' }}
      />
    </Card>
  )
}

// Advanced Heatmap Component
export const D3Heatmap: React.FC<{
  data: Array<{ x: string; y: string; value: number }>
  width?: number
  height?: number
  title?: string
}> = ({
  data,
  width = 500,
  height = 300,
  title = 'Heatmap'
}) => {
  const svgRef = useRef<SVGSVGElement>(null)

  useEffect(() => {
    if (!svgRef.current || !data.length) return

    const svg = svgRef.current
    svg.innerHTML = ''

    // Get unique x and y values
    const xValues = [...new Set(data.map(d => d.x))]
    const yValues = [...new Set(data.map(d => d.y))]
    
    const cellWidth = width / xValues.length
    const cellHeight = height / yValues.length
    
    const maxValue = Math.max(...data.map(d => d.value))
    const minValue = Math.min(...data.map(d => d.value))

    data.forEach(item => {
      const xIndex = xValues.indexOf(item.x)
      const yIndex = yValues.indexOf(item.y)
      
      const x = xIndex * cellWidth
      const y = yIndex * cellHeight
      
      // Calculate color intensity
      const intensity = (item.value - minValue) / (maxValue - minValue)
      const color = `rgba(66, 139, 202, ${intensity})`
      
      const rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect')
      rect.setAttribute('x', x.toString())
      rect.setAttribute('y', y.toString())
      rect.setAttribute('width', cellWidth.toString())
      rect.setAttribute('height', cellHeight.toString())
      rect.setAttribute('fill', color)
      rect.setAttribute('stroke', '#fff')
      rect.setAttribute('stroke-width', '1')
      
      // Add tooltip on hover
      rect.addEventListener('mouseenter', (e) => {
        rect.setAttribute('stroke', '#333')
        rect.setAttribute('stroke-width', '2')
      })
      
      rect.addEventListener('mouseleave', (e) => {
        rect.setAttribute('stroke', '#fff')
        rect.setAttribute('stroke-width', '1')
      })
      
      svg.appendChild(rect)
      
      // Add value text if cell is large enough
      if (cellWidth > 40 && cellHeight > 20) {
        const text = document.createElementNS('http://www.w3.org/2000/svg', 'text')
        text.setAttribute('x', (x + cellWidth / 2).toString())
        text.setAttribute('y', (y + cellHeight / 2).toString())
        text.setAttribute('text-anchor', 'middle')
        text.setAttribute('dominant-baseline', 'middle')
        text.setAttribute('font-size', '10')
        text.setAttribute('fill', intensity > 0.5 ? '#fff' : '#333')
        text.textContent = item.value.toString()
        
        svg.appendChild(text)
      }
    })
  }, [data, width, height])

  return (
    <Card title={title}>
      <svg
        ref={svgRef}
        width={width}
        height={height}
        style={{ border: '1px solid #f0f0f0', borderRadius: '4px' }}
      />
    </Card>
  )
}

// Advanced Chart Selector Component
export const D3ChartSelector: React.FC<{
  data: ChartData[]
  onChartTypeChange?: (type: string) => void
}> = ({ data, onChartTypeChange }) => {
  const [chartType, setChartType] = useState('sunburst')

  const handleChartChange = (value: string) => {
    setChartType(value)
    onChartTypeChange?.(value)
  }

  const renderChart = () => {
    switch (chartType) {
      case 'sunburst':
        return <D3SunburstChart data={data} />
      case 'network':
        const nodes = data.map((item, index) => ({
          id: item.label,
          label: item.label,
          group: index % 3,
          value: item.value
        }))
        const links = data.slice(0, -1).map((item, index) => ({
          source: item.label,
          target: data[index + 1].label,
          value: Math.random() * 5
        }))
        return <D3NetworkGraph nodes={nodes} links={links} />
      case 'heatmap':
        const heatmapData = data.map((item, index) => ({
          x: `X${index % 5}`,
          y: `Y${Math.floor(index / 5)}`,
          value: item.value
        }))
        return <D3Heatmap data={heatmapData} />
      default:
        return <D3SunburstChart data={data} />
    }
  }

  return (
    <div>
      <div style={{ marginBottom: '16px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={4} style={{ margin: 0 }}>Advanced Visualizations</Title>
        <Space>
          <Text>Chart Type:</Text>
          <Select
            value={chartType}
            onChange={handleChartChange}
            style={{ width: 150 }}
          >
            <Select.Option value="sunburst">
              <Space>
                Sunburst Chart
                <Tooltip title="Hierarchical data visualization">
                  <InfoCircleOutlined />
                </Tooltip>
              </Space>
            </Select.Option>
            <Select.Option value="network">
              <Space>
                Network Graph
                <Tooltip title="Relationship visualization">
                  <InfoCircleOutlined />
                </Tooltip>
              </Space>
            </Select.Option>
            <Select.Option value="heatmap">
              <Space>
                Heatmap
                <Tooltip title="Density visualization">
                  <InfoCircleOutlined />
                </Tooltip>
              </Space>
            </Select.Option>
          </Select>
        </Space>
      </div>
      
      {renderChart()}
    </div>
  )
}
