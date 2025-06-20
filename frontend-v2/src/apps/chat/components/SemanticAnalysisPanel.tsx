import React from 'react'
import {
  Card,
  Row,
  Col,
  Tag,
  Typography,
  Progress,
  List,
  Space,
  Tooltip,
  Alert,
  Divider
} from 'antd'
import {
  BulbOutlined,
  DatabaseOutlined,
  QuestionCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BookOutlined
} from '@ant-design/icons'
import type { SemanticAnalysis } from '@shared/types/chat'

const { Text, Title } = Typography

interface SemanticAnalysisPanelProps {
  analysis: SemanticAnalysis
  compact?: boolean
}

export const SemanticAnalysisPanel: React.FC<SemanticAnalysisPanelProps> = ({
  analysis,
  compact = false,
}) => {
  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return '#52c41a'
    if (confidence >= 0.6) return '#faad14'
    return '#ff4d4f'
  }

  const getEntityTypeIcon = (type: string) => {
    switch (type) {
      case 'table':
        return <DatabaseOutlined />
      case 'column':
        return <DatabaseOutlined />
      case 'metric':
        return <BulbOutlined />
      case 'dimension':
        return <BookOutlined />
      default:
        return <QuestionCircleOutlined />
    }
  }

  const getEntityTypeColor = (type: string) => {
    switch (type) {
      case 'table':
        return 'blue'
      case 'column':
        return 'cyan'
      case 'metric':
        return 'green'
      case 'dimension':
        return 'purple'
      case 'filter':
        return 'orange'
      default:
        return 'default'
    }
  }

  if (compact) {
    return (
      <Space direction="vertical" style={{ width: '100%' }}>
        <Space>
          <Text strong>Intent:</Text>
          <Tag color="blue">{analysis.intent}</Tag>
          <Text strong>Confidence:</Text>
          <Progress
            type="circle"
            size={24}
            percent={analysis.confidence * 100}
            strokeColor={getConfidenceColor(analysis.confidence)}
            format={() => `${(analysis.confidence * 100).toFixed(0)}%`}
          />
        </Space>
        
        {analysis.entities.length > 0 && (
          <div>
            <Text strong>Entities: </Text>
            <Space wrap>
              {analysis.entities.slice(0, 5).map((entity, index) => (
                <Tooltip key={index} title={entity.businessMeaning || entity.name}>
                  <Tag
                    icon={getEntityTypeIcon(entity.type)}
                    color={getEntityTypeColor(entity.type)}
                  >
                    {entity.name}
                  </Tag>
                </Tooltip>
              ))}
              {analysis.entities.length > 5 && (
                <Text type="secondary">+{analysis.entities.length - 5} more</Text>
              )}
            </Space>
          </div>
        )}
      </Space>
    )
  }

  return (
    <div style={{ padding: '16px 0' }}>
      {/* Overall Analysis */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col span={8}>
          <Card size="small">
            <div style={{ textAlign: 'center' }}>
              <Title level={4} style={{ margin: 0, color: getConfidenceColor(analysis.confidence) }}>
                {(analysis.confidence * 100).toFixed(1)}%
              </Title>
              <Text type="secondary">Confidence</Text>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <div style={{ textAlign: 'center' }}>
              <Title level={4} style={{ margin: 0 }}>
                {analysis.intent}
              </Title>
              <Text type="secondary">Query Intent</Text>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <div style={{ textAlign: 'center' }}>
              <Title level={4} style={{ margin: 0 }}>
                {analysis.entities.length}
              </Title>
              <Text type="secondary">Entities Found</Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Entities */}
      {analysis.entities.length > 0 && (
        <Card size="small" title="Identified Entities" style={{ marginBottom: 16 }}>
          <List
            size="small"
            dataSource={analysis.entities}
            renderItem={(entity) => (
              <List.Item>
                <Space>
                  <Tag
                    icon={getEntityTypeIcon(entity.type)}
                    color={getEntityTypeColor(entity.type)}
                  >
                    {entity.type}
                  </Tag>
                  <Text strong>{entity.name}</Text>
                  <Progress
                    type="circle"
                    size={20}
                    percent={entity.confidence * 100}
                    strokeColor={getConfidenceColor(entity.confidence)}
                    format={() => ''}
                  />
                  {entity.businessMeaning && (
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {entity.businessMeaning}
                    </Text>
                  )}
                </Space>
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* Business Terms */}
      {analysis.businessTerms.length > 0 && (
        <Card size="small" title="Business Terms" style={{ marginBottom: 16 }}>
          <Space wrap>
            {analysis.businessTerms.map((term, index) => (
              <Tooltip key={index} title={term.definition}>
                <Tag
                  color="green"
                  style={{ cursor: 'help' }}
                >
                  {term.term} ({(term.confidence * 100).toFixed(0)}%)
                </Tag>
              </Tooltip>
            ))}
          </Space>
        </Card>
      )}

      {/* Relevant Tables */}
      {analysis.relevantTables.length > 0 && (
        <Card size="small" title="Relevant Tables" style={{ marginBottom: 16 }}>
          <List
            size="small"
            dataSource={analysis.relevantTables}
            renderItem={(table) => (
              <List.Item>
                <Space>
                  <DatabaseOutlined />
                  <Text strong>{table.schemaName}.{table.tableName}</Text>
                  <Progress
                    type="circle"
                    size={20}
                    percent={table.relevanceScore * 100}
                    strokeColor={getConfidenceColor(table.relevanceScore)}
                    format={() => ''}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {table.businessPurpose}
                  </Text>
                </Space>
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* Ambiguities */}
      {analysis.ambiguities.length > 0 && (
        <Alert
          type="warning"
          icon={<ExclamationCircleOutlined />}
          message="Potential Ambiguities Detected"
          description={
            <List
              size="small"
              dataSource={analysis.ambiguities}
              renderItem={(ambiguity) => (
                <List.Item>
                  <div>
                    <Text strong>"{ambiguity.term}"</Text> could mean:
                    <ul style={{ margin: '4px 0', paddingLeft: 20 }}>
                      {ambiguity.possibleMeanings.map((meaning, index) => (
                        <li key={index}>{meaning}</li>
                      ))}
                    </ul>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Suggestion: {ambiguity.suggestedClarification}
                    </Text>
                  </div>
                </List.Item>
              )}
            />
          }
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Suggested Queries */}
      {analysis.suggestedQueries.length > 0 && (
        <Card size="small" title="Suggested Follow-up Queries">
          <List
            size="small"
            dataSource={analysis.suggestedQueries}
            renderItem={(query, index) => (
              <List.Item>
                <Space>
                  <BulbOutlined style={{ color: '#faad14' }} />
                  <Text copyable={{ text: query }}>{query}</Text>
                </Space>
              </List.Item>
            )}
          />
        </Card>
      )}
    </div>
  )
}
