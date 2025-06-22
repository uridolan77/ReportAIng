import React from 'react'
import {
  Card,
  Row,
  Col,
  Progress,
  Typography,
  Space,
  Tag,
  Tooltip,
  Badge,
  Statistic,
} from 'antd'
import {
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  InfoCircleOutlined,
  TrophyOutlined,
  WarningOutlined,
} from '@ant-design/icons'

const { Title, Text } = Typography

interface QualityMetrics {
  completeness: number
  accuracy: number
  consistency: number
  timeliness: number
  validity: number
  overall: number
}

interface QualityScoreVisualizationProps {
  metrics: QualityMetrics
  tableName?: string
  schemaName?: string
  showDetails?: boolean
  size?: 'small' | 'default' | 'large'
}

export const QualityScoreVisualization: React.FC<QualityScoreVisualizationProps> = ({
  metrics,
  tableName,
  schemaName,
  showDetails = true,
  size = 'default',
}) => {
  const getScoreColor = (score: number) => {
    if (score >= 90) return '#52c41a' // Green
    if (score >= 80) return '#73d13d' // Light green
    if (score >= 70) return '#faad14' // Orange
    if (score >= 60) return '#fa8c16' // Dark orange
    return '#ff4d4f' // Red
  }

  const getScoreStatus = (score: number) => {
    if (score >= 90) return { text: 'Excellent', icon: <TrophyOutlined />, color: 'success' }
    if (score >= 80) return { text: 'Good', icon: <CheckCircleOutlined />, color: 'success' }
    if (score >= 70) return { text: 'Fair', icon: <ExclamationCircleOutlined />, color: 'warning' }
    if (score >= 60) return { text: 'Poor', icon: <WarningOutlined />, color: 'warning' }
    return { text: 'Critical', icon: <CloseCircleOutlined />, color: 'error' }
  }

  const overallStatus = getScoreStatus(metrics.overall)

  const qualityDimensions = [
    {
      key: 'completeness',
      label: 'Completeness',
      value: metrics.completeness,
      description: 'How complete is the metadata',
      icon: <InfoCircleOutlined />,
    },
    {
      key: 'accuracy',
      label: 'Accuracy',
      value: metrics.accuracy,
      description: 'How accurate is the metadata',
      icon: <CheckCircleOutlined />,
    },
    {
      key: 'consistency',
      label: 'Consistency',
      value: metrics.consistency,
      description: 'How consistent is the metadata',
      icon: <ExclamationCircleOutlined />,
    },
    {
      key: 'timeliness',
      label: 'Timeliness',
      value: metrics.timeliness,
      description: 'How up-to-date is the metadata',
      icon: <CloseCircleOutlined />,
    },
    {
      key: 'validity',
      label: 'Validity',
      value: metrics.validity,
      description: 'How valid is the metadata format',
      icon: <WarningOutlined />,
    },
  ]

  const renderOverallScore = () => (
    <Card>
      <div style={{ textAlign: 'center' }}>
        <Progress
          type="circle"
          percent={metrics.overall}
          strokeColor={getScoreColor(metrics.overall)}
          size={size === 'large' ? 120 : size === 'small' ? 80 : 100}
          format={(percent) => (
            <div>
              <div style={{ fontSize: size === 'large' ? '24px' : '18px', fontWeight: 'bold' }}>
                {percent}
              </div>
              <div style={{ fontSize: '12px', color: '#666' }}>
                Quality Score
              </div>
            </div>
          )}
        />
        <div style={{ marginTop: 16 }}>
          <Space direction="vertical" align="center">
            <Badge
              status={overallStatus.color as any}
              text={
                <Space>
                  {overallStatus.icon}
                  <Text strong>{overallStatus.text}</Text>
                </Space>
              }
            />
            {tableName && (
              <Text type="secondary">
                {schemaName}.{tableName}
              </Text>
            )}
          </Space>
        </div>
      </div>
    </Card>
  )

  const renderDimensionScores = () => (
    <Card title="Quality Dimensions" size="small">
      <Space direction="vertical" style={{ width: '100%' }}>
        {qualityDimensions.map((dimension) => (
          <div key={dimension.key}>
            <Row justify="space-between" align="middle" style={{ marginBottom: 8 }}>
              <Col>
                <Space>
                  <Tooltip title={dimension.description}>
                    {dimension.icon}
                  </Tooltip>
                  <Text>{dimension.label}</Text>
                </Space>
              </Col>
              <Col>
                <Text strong style={{ color: getScoreColor(dimension.value) }}>
                  {dimension.value}%
                </Text>
              </Col>
            </Row>
            <Progress
              percent={dimension.value}
              strokeColor={getScoreColor(dimension.value)}
              showInfo={false}
              size="small"
            />
          </div>
        ))}
      </Space>
    </Card>
  )

  const renderQualityInsights = () => {
    const insights = []
    
    if (metrics.overall >= 90) {
      insights.push({
        type: 'success',
        message: 'Excellent metadata quality! This table is well-documented.',
        icon: <TrophyOutlined />,
      })
    } else if (metrics.overall >= 80) {
      insights.push({
        type: 'success',
        message: 'Good metadata quality with minor areas for improvement.',
        icon: <CheckCircleOutlined />,
      })
    } else if (metrics.overall >= 70) {
      insights.push({
        type: 'warning',
        message: 'Fair metadata quality. Consider improving documentation.',
        icon: <ExclamationCircleOutlined />,
      })
    } else {
      insights.push({
        type: 'error',
        message: 'Poor metadata quality. Immediate attention required.',
        icon: <CloseCircleOutlined />,
      })
    }

    // Add specific dimension insights
    if (metrics.completeness < 70) {
      insights.push({
        type: 'warning',
        message: 'Metadata completeness is low. Add missing business context.',
        icon: <InfoCircleOutlined />,
      })
    }

    if (metrics.accuracy < 70) {
      insights.push({
        type: 'warning',
        message: 'Metadata accuracy needs improvement. Verify business rules.',
        icon: <ExclamationCircleOutlined />,
      })
    }

    if (metrics.timeliness < 70) {
      insights.push({
        type: 'warning',
        message: 'Metadata is outdated. Consider refreshing documentation.',
        icon: <CloseCircleOutlined />,
      })
    }

    return (
      <Card title="Quality Insights" size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          {insights.map((insight, index) => (
            <div key={index}>
              <Space>
                <span style={{ 
                  color: insight.type === 'success' ? '#52c41a' : 
                        insight.type === 'warning' ? '#faad14' : '#ff4d4f' 
                }}>
                  {insight.icon}
                </span>
                <Text>{insight.message}</Text>
              </Space>
            </div>
          ))}
        </Space>
      </Card>
    )
  }

  const renderCompactView = () => (
    <Space>
      <Progress
        type="circle"
        percent={metrics.overall}
        strokeColor={getScoreColor(metrics.overall)}
        size={60}
        format={(percent) => (
          <span style={{ fontSize: '12px', fontWeight: 'bold' }}>
            {percent}
          </span>
        )}
      />
      <Space direction="vertical" size="small">
        <Badge
          status={overallStatus.color as any}
          text={<Text strong>{overallStatus.text}</Text>}
        />
        {tableName && (
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {schemaName}.{tableName}
          </Text>
        )}
      </Space>
    </Space>
  )

  if (size === 'small' && !showDetails) {
    return renderCompactView()
  }

  return (
    <div>
      <Row gutter={16}>
        <Col span={showDetails ? 8 : 24}>
          {renderOverallScore()}
        </Col>
        {showDetails && (
          <>
            <Col span={8}>
              {renderDimensionScores()}
            </Col>
            <Col span={8}>
              {renderQualityInsights()}
            </Col>
          </>
        )}
      </Row>
    </div>
  )
}

// Helper component for displaying multiple quality scores in a grid
export const QualityScoreGrid: React.FC<{
  scores: Array<{
    tableName: string
    schemaName: string
    metrics: QualityMetrics
  }>
}> = ({ scores }) => {
  return (
    <Row gutter={[16, 16]}>
      {scores.map((score, index) => (
        <Col key={index} xs={24} sm={12} md={8} lg={6}>
          <QualityScoreVisualization
            metrics={score.metrics}
            tableName={score.tableName}
            schemaName={score.schemaName}
            showDetails={false}
            size="small"
          />
        </Col>
      ))}
    </Row>
  )
}

export default QualityScoreVisualization
