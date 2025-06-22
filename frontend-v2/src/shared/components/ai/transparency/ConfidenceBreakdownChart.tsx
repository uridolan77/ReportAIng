import React, { useState, useMemo } from 'react'
import { Card, Row, Col, Progress, Typography, Space, Button, Tooltip, Tag, List } from 'antd'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer, PieChart, Pie, Cell, RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, Radar } from 'recharts'
import {
  BarChartOutlined,
  PieChartOutlined,
  RadarChartOutlined,
  InfoCircleOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useConfidenceThreshold } from '../common/hooks/useConfidenceThreshold'
import type { ConfidenceBreakdownChartProps, ConfidenceVisualization } from './types'

const { Title, Text, Paragraph } = Typography

/**
 * ConfidenceBreakdownChart - Interactive visualization of AI confidence analysis
 * 
 * Features:
 * - Multiple chart types (bar, pie, radar)
 * - Interactive factor exploration
 * - Confidence threshold visualization
 * - Factor impact analysis
 * - Detailed explanations
 * - Responsive design
 */
export const ConfidenceBreakdownChart: React.FC<ConfidenceBreakdownChartProps> = ({
  analysis,
  showFactors = true,
  interactive = true,
  onFactorClick,
  chartType: propChartType = 'bar',
  className,
  testId = 'confidence-breakdown-chart'
}) => {
  const [selectedFactor, setSelectedFactor] = useState<string | null>(null)
  const [chartType, setChartType] = useState<'bar' | 'pie' | 'radar'>(propChartType)
  const [showTooltip, setShowTooltip] = useState(false)
  
  const { getConfidenceColor, getConfidenceLevel } = useConfidenceThreshold()

  // Prepare chart data
  const chartData = useMemo(() => {
    return analysis.factors.map(factor => ({
      name: factor.name,
      value: factor.score * 100,
      score: factor.score,
      explanation: factor.explanation,
      impact: factor.impact,
      category: factor.category,
      color: getConfidenceColor(factor.score)
    }))
  }, [analysis.factors, getConfidenceColor])

  // Prepare breakdown data
  const breakdownData = useMemo(() => {
    return Object.entries(analysis.breakdown).map(([key, value]) => ({
      name: key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase()),
      value: value * 100,
      score: value,
      color: getConfidenceColor(value)
    }))
  }, [analysis.breakdown, getConfidenceColor])

  // Handle factor selection
  const handleFactorClick = (factorName: string) => {
    if (!interactive) return
    
    const newSelected = selectedFactor === factorName ? null : factorName
    setSelectedFactor(newSelected)
    onFactorClick?.(factorName)
  }

  // Get impact icon
  const getImpactIcon = (impact: string) => {
    switch (impact) {
      case 'high': return <TrophyOutlined style={{ color: '#ff4d4f' }} />
      case 'medium': return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
      case 'low': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      default: return <InfoCircleOutlined />
    }
  }

  // Get category color
  const getCategoryColor = (category: string) => {
    const colors = {
      context: '#1890ff',
      syntax: '#52c41a',
      semantics: '#722ed1',
      business: '#fa8c16'
    }
    return colors[category as keyof typeof colors] || '#d9d9d9'
  }

  // Render bar chart
  const renderBarChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis 
          dataKey="name" 
          angle={-45}
          textAnchor="end"
          height={80}
          fontSize={12}
        />
        <YAxis 
          domain={[0, 100]}
          label={{ value: 'Confidence %', angle: -90, position: 'insideLeft' }}
        />
        <Bar 
          dataKey="value" 
          fill="#1890ff"
          onClick={(data) => handleFactorClick(data.name)}
          style={{ cursor: interactive ? 'pointer' : 'default' }}
        >
          {chartData.map((entry, index) => (
            <Cell 
              key={`cell-${index}`} 
              fill={selectedFactor === entry.name ? '#ff7875' : entry.color}
            />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )

  // Render pie chart
  const renderPieChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <PieChart>
        <Pie
          data={chartData}
          cx="50%"
          cy="50%"
          labelLine={false}
          label={({ name, value }) => `${name}: ${value.toFixed(1)}%`}
          outerRadius={80}
          fill="#8884d8"
          dataKey="value"
          onClick={(data) => handleFactorClick(data.name)}
          style={{ cursor: interactive ? 'pointer' : 'default' }}
        >
          {chartData.map((entry, index) => (
            <Cell 
              key={`cell-${index}`} 
              fill={selectedFactor === entry.name ? '#ff7875' : entry.color}
            />
          ))}
        </Pie>
      </PieChart>
    </ResponsiveContainer>
  )

  // Render radar chart
  const renderRadarChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <RadarChart data={chartData}>
        <PolarGrid />
        <PolarAngleAxis dataKey="name" fontSize={12} />
        <PolarRadiusAxis 
          angle={90} 
          domain={[0, 100]}
          fontSize={10}
        />
        <Radar
          name="Confidence"
          dataKey="value"
          stroke="#1890ff"
          fill="#1890ff"
          fillOpacity={0.3}
          onClick={(data) => handleFactorClick(data.name)}
          style={{ cursor: interactive ? 'pointer' : 'default' }}
        />
      </RadarChart>
    </ResponsiveContainer>
  )

  // Render chart based on type
  const renderChart = () => {
    switch (chartType) {
      case 'pie': return renderPieChart()
      case 'radar': return renderRadarChart()
      default: return renderBarChart()
    }
  }

  // Get selected factor details
  const selectedFactorData = selectedFactor 
    ? analysis.factors.find(f => f.name === selectedFactor)
    : null

  return (
    <div className={className} data-testid={testId}>
      {/* Header with controls */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 16 
      }}>
        <Space>
          <Title level={5} style={{ margin: 0 }}>
            Confidence Analysis
          </Title>
          <ConfidenceIndicator
            confidence={analysis.overallConfidence}
            size="small"
            type="badge"
          />
        </Space>
        {interactive && (
          <Space>
            <Button
              size="small"
              type={chartType === 'bar' ? 'primary' : 'default'}
              icon={<BarChartOutlined />}
              onClick={() => setChartType('bar')}
            />
            <Button
              size="small"
              type={chartType === 'pie' ? 'primary' : 'default'}
              icon={<PieChartOutlined />}
              onClick={() => setChartType('pie')}
            />
            <Button
              size="small"
              type={chartType === 'radar' ? 'primary' : 'default'}
              icon={<RadarChartOutlined />}
              onClick={() => setChartType('radar')}
            />
          </Space>
        )}
      </div>

      <Row gutter={[16, 16]}>
        {/* Chart */}
        <Col xs={24} lg={showFactors ? 14 : 24}>
          <Card size="small" title="Confidence Factors">
            {renderChart()}
          </Card>
        </Col>

        {/* Factor details */}
        {showFactors && (
          <Col xs={24} lg={10}>
            <Card 
              size="small" 
              title={
                <Space>
                  <span>Factor Details</span>
                  {selectedFactor && (
                    <Tag color="blue">{selectedFactor}</Tag>
                  )}
                </Space>
              }
            >
              {selectedFactorData ? (
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
                      <Text strong>Score</Text>
                      <Text>{(selectedFactorData.score * 100).toFixed(1)}%</Text>
                    </div>
                    <Progress 
                      percent={selectedFactorData.score * 100}
                      strokeColor={getConfidenceColor(selectedFactorData.score)}
                      size="small"
                    />
                  </div>
                  
                  <div>
                    <Text strong>Impact Level</Text>
                    <div style={{ marginTop: 4 }}>
                      <Space>
                        {getImpactIcon(selectedFactorData.impact)}
                        <Tag color={selectedFactorData.impact === 'high' ? 'red' : selectedFactorData.impact === 'medium' ? 'orange' : 'green'}>
                          {selectedFactorData.impact.toUpperCase()}
                        </Tag>
                      </Space>
                    </div>
                  </div>

                  <div>
                    <Text strong>Category</Text>
                    <div style={{ marginTop: 4 }}>
                      <Tag color={getCategoryColor(selectedFactorData.category)}>
                        {selectedFactorData.category}
                      </Tag>
                    </div>
                  </div>

                  <div>
                    <Text strong>Explanation</Text>
                    <Paragraph style={{ marginTop: 4, marginBottom: 0 }}>
                      {selectedFactorData.explanation}
                    </Paragraph>
                  </div>
                </Space>
              ) : (
                <div style={{ textAlign: 'center', padding: '20px 0' }}>
                  <Text type="secondary">
                    Click on a factor in the chart to see details
                  </Text>
                </div>
              )}
            </Card>
          </Col>
        )}
      </Row>

      {/* Breakdown summary */}
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col span={24}>
          <Card size="small" title="Confidence Breakdown">
            <Row gutter={[16, 16]}>
              {breakdownData.map((item, index) => (
                <Col xs={12} sm={6} key={index}>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ marginBottom: 8 }}>
                      <Text strong>{item.name}</Text>
                    </div>
                    <Progress
                      type="circle"
                      percent={item.value}
                      size={60}
                      strokeColor={item.color}
                      format={() => `${item.value.toFixed(0)}%`}
                    />
                  </div>
                </Col>
              ))}
            </Row>
          </Card>
        </Col>
      </Row>

      {/* Recommendations */}
      {analysis.recommendations.length > 0 && (
        <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
          <Col span={24}>
            <Card size="small" title="Recommendations">
              <List
                size="small"
                dataSource={analysis.recommendations}
                renderItem={(recommendation, index) => (
                  <List.Item key={index}>
                    <Space>
                      <InfoCircleOutlined style={{ color: '#1890ff' }} />
                      <Text>{recommendation}</Text>
                    </Space>
                  </List.Item>
                )}
              />
            </Card>
          </Col>
        </Row>
      )}
    </div>
  )
}

export default ConfidenceBreakdownChart
