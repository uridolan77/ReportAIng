import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Progress,
  Typography,
  Tag,
  Space,
  Tooltip,
  Button,
  Collapse,
  List,
  Alert,
  Divider,
  Timeline,
  Badge
} from 'antd'
import {
  BulbOutlined,
  DatabaseOutlined,
  QuestionCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BookOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  BranchesOutlined,
  TagOutlined,
  ClockCircleOutlined,
  AimOutlined
} from '@ant-design/icons'
import type { SemanticAnalysis } from '@shared/types/chat'

const { Text, Title, Paragraph } = Typography
const { Panel } = Collapse

interface SemanticInsightsPanelProps {
  analysis: SemanticAnalysis
  onEntityClick?: (entity: any) => void
  onSuggestionClick?: (suggestion: string) => void
  showAdvanced?: boolean
}

export const SemanticInsightsPanel: React.FC<SemanticInsightsPanelProps> = ({
  analysis,
  onEntityClick,
  onSuggestionClick,
  showAdvanced = false
}) => {
  const [activeKey, setActiveKey] = useState<string[]>(['overview'])

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return '#52c41a'
    if (confidence >= 0.6) return '#faad14'
    return '#ff4d4f'
  }

  const getConfidenceLabel = (confidence: number) => {
    if (confidence >= 0.8) return 'High'
    if (confidence >= 0.6) return 'Medium'
    return 'Low'
  }

  const getEntityIcon = (type: string) => {
    switch (type) {
      case 'table': return <DatabaseOutlined />
      case 'column': return <BranchesOutlined />
      case 'metric': return <AimOutlined />
      case 'dimension': return <BookOutlined />
      case 'filter': return <ExclamationCircleOutlined />
      default: return <QuestionCircleOutlined />
    }
  }

  const getEntityColor = (type: string) => {
    switch (type) {
      case 'table': return 'blue'
      case 'column': return 'cyan'
      case 'metric': return 'green'
      case 'dimension': return 'purple'
      case 'filter': return 'orange'
      default: return 'default'
    }
  }

  const renderOverview = () => (
    <Row gutter={[16, 16]}>
      <Col span={8}>
        <Card size="small" style={{ textAlign: 'center' }}>
          <div style={{
            fontSize: '32px',
            fontWeight: 'bold',
            color: getConfidenceColor(analysis.confidence),
            marginBottom: '8px'
          }}>
            {(analysis.confidence * 100).toFixed(0)}%
          </div>
          <Text type="secondary">Overall Confidence</Text>
          <div style={{ marginTop: '8px' }}>
            <Tag color={getConfidenceColor(analysis.confidence)}>
              {getConfidenceLabel(analysis.confidence)}
            </Tag>
          </div>
        </Card>
      </Col>
      
      <Col span={8}>
        <Card size="small" style={{ textAlign: 'center' }}>
          <div style={{
            fontSize: '24px',
            fontWeight: 'bold',
            color: '#1890ff',
            marginBottom: '8px'
          }}>
            {analysis.intent}
          </div>
          <Text type="secondary">Query Intent</Text>
          <div style={{ marginTop: '8px' }}>
            <Tag color="blue">{analysis.queryType || 'Analysis'}</Tag>
          </div>
        </Card>
      </Col>
      
      <Col span={8}>
        <Card size="small" style={{ textAlign: 'center' }}>
          <div style={{
            fontSize: '24px',
            fontWeight: 'bold',
            color: '#722ed1',
            marginBottom: '8px'
          }}>
            {analysis.entities.length}
          </div>
          <Text type="secondary">Entities Found</Text>
          <div style={{ marginTop: '8px' }}>
            <Tag color="purple">
              {analysis.relevantTables.length} tables
            </Tag>
          </div>
        </Card>
      </Col>
    </Row>
  )

  const renderEntities = () => (
    <div>
      <Title level={5} style={{ marginBottom: '16px' }}>
        <DatabaseOutlined /> Identified Entities
      </Title>
      
      <List
        size="small"
        dataSource={analysis.entities}
        renderItem={(entity, index) => (
          <List.Item
            style={{
              cursor: onEntityClick ? 'pointer' : 'default',
              padding: '12px',
              border: '1px solid #f0f0f0',
              borderRadius: '8px',
              marginBottom: '8px',
              background: '#fafafa'
            }}
            onClick={() => onEntityClick?.(entity)}
          >
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <Space>
                <Tag
                  icon={getEntityIcon(entity.type)}
                  color={getEntityColor(entity.type)}
                >
                  {entity.type}
                </Tag>
                <Text strong>{entity.name}</Text>
                {entity.businessMeaning && (
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {entity.businessMeaning}
                  </Text>
                )}
              </Space>
              
              <Space>
                <Progress
                  type="circle"
                  size={24}
                  percent={entity.confidence * 100}
                  strokeColor={getConfidenceColor(entity.confidence)}
                  format={() => ''}
                />
                <Text type="secondary" style={{ fontSize: '11px' }}>
                  {(entity.confidence * 100).toFixed(0)}%
                </Text>
              </Space>
            </Space>
          </List.Item>
        )}
      />
    </div>
  )

  const renderBusinessContext = () => (
    <div>
      <Title level={5} style={{ marginBottom: '16px' }}>
        <BookOutlined /> Business Context
      </Title>
      
      {analysis.businessTerms.length > 0 && (
        <div style={{ marginBottom: '24px' }}>
          <Text strong style={{ display: 'block', marginBottom: '8px' }}>
            Business Terms:
          </Text>
          <Space wrap>
            {analysis.businessTerms.map((term, index) => (
              <Tooltip key={index} title={term.definition}>
                <Tag
                  color="green"
                  style={{ cursor: 'help', marginBottom: '4px' }}
                >
                  {term.term} ({(term.confidence * 100).toFixed(0)}%)
                </Tag>
              </Tooltip>
            ))}
          </Space>
        </div>
      )}

      {analysis.relevantTables.length > 0 && (
        <div>
          <Text strong style={{ display: 'block', marginBottom: '8px' }}>
            Relevant Tables:
          </Text>
          <List
            size="small"
            dataSource={analysis.relevantTables}
            renderItem={(table) => (
              <List.Item style={{ padding: '8px 0' }}>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                  <Space>
                    <DatabaseOutlined style={{ color: '#1890ff' }} />
                    <Text strong>{table.schemaName}.{table.tableName}</Text>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {table.businessPurpose}
                    </Text>
                  </Space>
                  <Badge
                    count={`${(table.relevanceScore * 100).toFixed(0)}%`}
                    style={{
                      backgroundColor: getConfidenceColor(table.relevanceScore)
                    }}
                  />
                </Space>
              </List.Item>
            )}
          />
        </div>
      )}
    </div>
  )

  const renderAmbiguities = () => {
    if (analysis.ambiguities.length === 0) {
      return (
        <Alert
          type="success"
          message="No Ambiguities Detected"
          description="Your query is clear and unambiguous."
          showIcon
        />
      )
    }

    return (
      <div>
        <Alert
          type="warning"
          message={`${analysis.ambiguities.length} Potential Ambiguities Detected`}
          description="These terms could have multiple meanings in your data context."
          showIcon
          style={{ marginBottom: '16px' }}
        />
        
        <List
          size="small"
          dataSource={analysis.ambiguities}
          renderItem={(ambiguity) => (
            <List.Item>
              <Card size="small" style={{ width: '100%' }}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong style={{ color: '#faad14' }}>
                    "{ambiguity.term}"
                  </Text>
                  
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Possible meanings:
                    </Text>
                    <ul style={{ margin: '4px 0 8px 16px', padding: 0 }}>
                      {ambiguity.possibleMeanings.map((meaning, index) => (
                        <li key={index} style={{ fontSize: '12px', color: '#666' }}>
                          {meaning}
                        </li>
                      ))}
                    </ul>
                  </div>
                  
                  <Alert
                    type="info"
                    message={ambiguity.suggestedClarification}
                    style={{ fontSize: '12px' }}
                    showIcon
                  />
                </Space>
              </Card>
            </List.Item>
          )}
        />
      </div>
    )
  }

  const renderSuggestions = () => (
    <div>
      <Title level={5} style={{ marginBottom: '16px' }}>
        <BulbOutlined /> Suggested Follow-up Queries
      </Title>
      
      <List
        size="small"
        dataSource={analysis.suggestedQueries}
        renderItem={(query, index) => (
          <List.Item
            style={{
              cursor: onSuggestionClick ? 'pointer' : 'default',
              padding: '12px',
              border: '1px solid #e6f7ff',
              borderRadius: '8px',
              marginBottom: '8px',
              background: '#f6ffed'
            }}
            onClick={() => onSuggestionClick?.(query)}
          >
            <Space>
              <ThunderboltOutlined style={{ color: '#52c41a' }} />
              <Text>{query}</Text>
              {onSuggestionClick && (
                <Button type="link" size="small">
                  Try this →
                </Button>
              )}
            </Space>
          </List.Item>
        )}
      />
    </div>
  )

  const renderProcessingTimeline = () => {
    if (!showAdvanced) return null

    const timelineItems = [
      {
        color: 'green',
        dot: <CheckCircleOutlined />,
        children: (
          <div>
            <Text strong>Query Parsing</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Extracted {analysis.entities.length} entities
            </Text>
          </div>
        )
      },
      {
        color: 'blue',
        dot: <DatabaseOutlined />,
        children: (
          <div>
            <Text strong>Context Analysis</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Matched {analysis.relevantTables.length} relevant tables
            </Text>
          </div>
        )
      },
      {
        color: analysis.ambiguities.length > 0 ? 'orange' : 'green',
        dot: analysis.ambiguities.length > 0 ? <ExclamationCircleOutlined /> : <CheckCircleOutlined />,
        children: (
          <div>
            <Text strong>Ambiguity Detection</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {analysis.ambiguities.length === 0 
                ? 'No ambiguities found' 
                : `${analysis.ambiguities.length} potential ambiguities`}
            </Text>
          </div>
        )
      },
      {
        color: 'purple',
        dot: <BulbOutlined />,
        children: (
          <div>
            <Text strong>Suggestion Generation</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Generated {analysis.suggestedQueries.length} follow-up suggestions
            </Text>
          </div>
        )
      }
    ]

    return (
      <div>
        <Title level={5} style={{ marginBottom: '16px' }}>
          <ClockCircleOutlined /> Processing Timeline
        </Title>
        <Timeline items={timelineItems} />
      </div>
    )
  }

  return (
    <Card
      title={
        <Space>
          <EyeOutlined />
          <Text strong>Semantic Analysis</Text>
          <Tag color={getConfidenceColor(analysis.confidence)}>
            {getConfidenceLabel(analysis.confidence)} Confidence
          </Tag>
        </Space>
      }
      style={{ marginTop: '16px' }}
    >
      <Collapse
        activeKey={activeKey}
        onChange={setActiveKey}
        ghost
        items={[
          {
            key: 'overview',
            label: (
              <Space>
                <AimOutlined />
                <Text strong>Overview</Text>
              </Space>
            ),
            children: renderOverview()
          },
          {
            key: 'entities',
            label: (
              <Space>
                <DatabaseOutlined />
                <Text strong>Entities ({analysis.entities.length})</Text>
              </Space>
            ),
            children: renderEntities()
          },
          {
            key: 'context',
            label: (
              <Space>
                <BookOutlined />
                <Text strong>Business Context</Text>
              </Space>
            ),
            children: renderBusinessContext()
          },
          {
            key: 'ambiguities',
            label: (
              <Space>
                <ExclamationCircleOutlined />
                <Text strong>Ambiguities ({analysis.ambiguities.length})</Text>
                {analysis.ambiguities.length > 0 && (
                  <Badge count={analysis.ambiguities.length} size="small" />
                )}
              </Space>
            ),
            children: renderAmbiguities()
          },
          {
            key: 'suggestions',
            label: (
              <Space>
                <BulbOutlined />
                <Text strong>Suggestions ({analysis.suggestedQueries.length})</Text>
              </Space>
            ),
            children: renderSuggestions()
          },
          ...(showAdvanced ? [{
            key: 'timeline',
            label: (
              <Space>
                <ClockCircleOutlined />
                <Text strong>Processing Details</Text>
              </Space>
            ),
            children: renderProcessingTimeline()
          }] : [])
        ]}
      />
    </Card>
  )
}
