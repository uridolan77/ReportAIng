import React, { useState } from 'react'
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
  Divider,
  Button,
  Collapse
} from 'antd'
import {
  BulbOutlined,
  DatabaseOutlined,
  QuestionCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BookOutlined,
  EyeOutlined,
  BarChartOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { AITransparencyPanel } from '@shared/components/ai/transparency/AITransparencyPanel'
import { useFeatureFlag } from '@shared/components/ai/common/hooks/useAIFeatureFlags'
import type { SemanticAnalysis } from '@shared/types/chat'

const { Text, Title } = Typography
const { Panel } = Collapse

interface SemanticAnalysisPanelProps {
  analysis: SemanticAnalysis
  compact?: boolean
  showTransparency?: boolean
  traceId?: string
}

export const SemanticAnalysisPanel: React.FC<SemanticAnalysisPanelProps> = ({
  analysis,
  compact = false,
  showTransparency: propShowTransparency,
  traceId
}) => {
  const [showTransparencyPanel, setShowTransparencyPanel] = useState(false)
  const transparencyEnabled = useFeatureFlag('transparencyPanelEnabled')

  // Show transparency if explicitly requested or if feature is enabled and traceId is provided
  const shouldShowTransparency = propShowTransparency ?? (transparencyEnabled && !!traceId)
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
          <ConfidenceIndicator
            confidence={analysis.confidence}
            size="small"
            type="circle"
            showPercentage={true}
          />
          {shouldShowTransparency && (
            <Tooltip title="Show AI transparency">
              <Button
                size="small"
                type="text"
                icon={<EyeOutlined />}
                onClick={() => setShowTransparencyPanel(!showTransparencyPanel)}
              />
            </Tooltip>
          )}
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

        {/* Transparency Panel for compact view */}
        {shouldShowTransparency && showTransparencyPanel && traceId && (
          <Card size="small" style={{ marginTop: 8 }}>
            <AITransparencyPanel
              traceId={traceId}
              compact={true}
              showDetailedMetrics={false}
            />
          </Card>
        )}
      </Space>
    )
  }

  return (
    <div style={{ padding: '16px 0' }}>
      {/* Header with transparency toggle */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: 16
      }}>
        <Title level={5} style={{ margin: 0 }}>
          Semantic Analysis
        </Title>
        {shouldShowTransparency && (
          <Space>
            <Button
              size="small"
              type={showTransparencyPanel ? 'primary' : 'default'}
              icon={<EyeOutlined />}
              onClick={() => setShowTransparencyPanel(!showTransparencyPanel)}
            >
              AI Transparency
            </Button>
          </Space>
        )}
      </div>

      {/* Overall Analysis */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col span={8}>
          <Card size="small">
            <div style={{ textAlign: 'center' }}>
              <ConfidenceIndicator
                confidence={analysis.confidence}
                size="medium"
                type="circle"
                showLabel={true}
              />
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
                  <ConfidenceIndicator
                    confidence={entity.confidence}
                    size="small"
                    type="circle"
                    showPercentage={false}
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
                  <ConfidenceIndicator
                    confidence={table.relevanceScore}
                    size="small"
                    type="circle"
                    showPercentage={false}
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

      {/* AI Transparency Panel */}
      {shouldShowTransparency && showTransparencyPanel && traceId && (
        <div style={{ marginTop: 24 }}>
          <Divider />
          <AITransparencyPanel
            traceId={traceId}
            showDetailedMetrics={true}
            compact={false}
          />
        </div>
      )}
    </div>
  )
}
