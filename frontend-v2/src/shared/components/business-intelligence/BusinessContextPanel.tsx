import React, { useState, useCallback } from 'react'
import {
  Card,
  Spin,
  Empty,
  Alert,
  Space,
  Typography,
  Tag,
  Progress,
  Row,
  Col,
  Tooltip,
  Button,
  Collapse,
  Statistic,
  Timeline,
  Badge,
  Divider,
  List,
  Avatar
} from 'antd'
import {
  BulbOutlined,
  TagsOutlined,
  ClockCircleOutlined,
  UserOutlined,
  InfoCircleOutlined,
  TrophyOutlined,
  DatabaseOutlined,
  TeamOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  SettingOutlined,
  BookOutlined
} from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

interface BusinessContextPanelProps {
  context?: BusinessContextProfile
  loading?: boolean
  interactive?: boolean
  showDomainDetails?: boolean
  showEntityRelationships?: boolean
  showAdvancedMetrics?: boolean
  showUserContext?: boolean
  onEntityClick?: (entityId: string) => void
  onDomainExplore?: (domain: string) => void
  error?: any
}

/**
 * BusinessContextPanel - Enhanced business context analysis with interactive features
 *
 * Features:
 * - Real-time business context visualization with confidence indicators
 * - Interactive intent classification with drill-down capabilities
 * - Advanced entity overview with relationship mapping
 * - Domain context information with exploration features
 * - Time context analysis with trend indicators
 * - User context integration with personalization
 * - Performance metrics and optimization insights
 */
export const BusinessContextPanel: React.FC<BusinessContextPanelProps> = ({
  context,
  loading = false,
  interactive = true,
  showDomainDetails = true,
  showEntityRelationships = true,
  showAdvancedMetrics = true,
  showUserContext = true,
  onEntityClick,
  onDomainExplore,
  error
}) => {
  const [expandedSections, setExpandedSections] = useState<string[]>(['overview'])
  const [selectedEntity, setSelectedEntity] = useState<string | null>(null)
  const handleEntityClick = useCallback((entityId: string) => {
    setSelectedEntity(entityId)
    if (onEntityClick) {
      onEntityClick(entityId)
    }
  }, [onEntityClick])

  const handleDomainExplore = useCallback((domain: string) => {
    if (onDomainExplore) {
      onDomainExplore(domain)
    }
  }, [onDomainExplore])

  const handleSectionToggle = useCallback((key: string | string[]) => {
    setExpandedSections(Array.isArray(key) ? key : [key])
  }, [])

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Analyzing business context...</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Processing entities, intent, and domain relationships...
            </Text>
          </div>
        </div>
      </Card>
    )
  }

  if (error) {
    return (
      <Card>
        <Alert
          message="Business Context Analysis Error"
          description={error.message || 'Failed to analyze business context'}
          type="error"
          showIcon
          action={
            <Button size="small" onClick={() => window.location.reload()}>
              Retry Analysis
            </Button>
          }
        />
      </Card>
    )
  }

  if (!context) {
    return (
      <Card>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            <div>
              <Text>Enter a query above to see business context analysis</Text>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                AI will analyze your query for business intent, entities, and domain context
              </Text>
            </div>
          }
        />
      </Card>
    )
  }

  // Enhanced utility functions
  const getIntentColor = (type: string) => {
    const colors: Record<string, string> = {
      'aggregation': 'blue',
      'comparison': 'green',
      'trend_analysis': 'purple',
      'data_retrieval': 'orange',
      'exploration': 'cyan',
      'filtering': 'magenta',
      'ranking': 'red',
      'calculation': 'gold'
    }
    return colors[type] || 'default'
  }

  const getComplexityColor = (complexity: string) => {
    const colors: Record<string, string> = {
      'simple': 'green',
      'moderate': 'orange',
      'complex': 'red',
      'very_complex': 'purple'
    }
    return colors[complexity] || 'default'
  }

  const getConfidenceStatus = (confidence: number) => {
    if (confidence >= 0.9) return { status: 'success' as const, text: 'Excellent' }
    if (confidence >= 0.8) return { status: 'success' as const, text: 'High' }
    if (confidence >= 0.7) return { status: 'normal' as const, text: 'Good' }
    if (confidence >= 0.6) return { status: 'normal' as const, text: 'Moderate' }
    return { status: 'exception' as const, text: 'Low' }
  }

  const getDomainIcon = (domain: string) => {
    const icons: Record<string, React.ReactNode> = {
      'Sales': <BarChartOutlined />,
      'Finance': <TrophyOutlined />,
      'Marketing': <TeamOutlined />,
      'Operations': <SettingOutlined />,
      'Analytics': <ThunderboltOutlined />
    }
    return icons[domain] || <DatabaseOutlined />
  }

  const confidenceStatus = getConfidenceStatus(context.confidence)

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Enhanced Overview Dashboard */}
      <Card
        title={
          <Space>
            <BulbOutlined />
            <span>Business Context Analysis</span>
            <Badge
              count={`${Math.round(context.confidence * 100)}%`}
              style={{ backgroundColor: confidenceStatus.status === 'success' ? '#52c41a' : '#1890ff' }}
            />
          </Space>
        }
        extra={
          interactive && (
            <Space>
              <Tooltip title="View detailed analysis">
                <Button icon={<EyeOutlined />} size="small" />
              </Tooltip>
              <Tooltip title="Export analysis">
                <Button icon={<SettingOutlined />} size="small" />
              </Tooltip>
            </Space>
          )
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Confidence Score"
              value={context.confidence}
              precision={2}
              suffix="/ 1.0"
              valueStyle={{
                color: confidenceStatus.status === 'success' ? '#3f8600' : '#1890ff'
              }}
              prefix={<TrophyOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Intent Type"
              value={context.intent.type.replace('_', ' ').toUpperCase()}
              valueStyle={{ color: '#722ed1' }}
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Entities Found"
              value={context.entities.length}
              valueStyle={{ color: '#13c2c2' }}
              prefix={<TagsOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Domain Relevance"
              value={context.domain?.relevance || 0}
              precision={2}
              suffix="/ 1.0"
              valueStyle={{ color: '#fa8c16' }}
              prefix={<DatabaseOutlined />}
            />
          </Col>
        </Row>
      </Card>

      {/* Enhanced Collapsible Analysis Sections */}
      <Collapse
        activeKey={expandedSections}
        onChange={handleSectionToggle}
        size="large"
        ghost
      >
        {/* Intent Analysis Section */}
        <Panel
          header={
            <Space>
              <BulbOutlined style={{ color: '#1890ff' }} />
              <span style={{ fontWeight: 600 }}>Intent Classification</span>
              <Tag color={getIntentColor(context.intent.type)}>
                {context.intent.type.replace('_', ' ').toUpperCase()}
              </Tag>
              <Tag color={getComplexityColor(context.intent.complexity)}>
                {context.intent.complexity.toUpperCase()}
              </Tag>
              <Badge
                count={`${Math.round(context.intent.confidence * 100)}%`}
                style={{ backgroundColor: '#52c41a' }}
              />
            </Space>
          }
          key="intent"
        >
          <Card size="small" style={{ marginBottom: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Paragraph>{context.intent.description}</Paragraph>

              {context.intent.businessGoal && (
                <div>
                  <Text strong style={{ color: '#722ed1' }}>
                    <TrophyOutlined /> Business Goal:
                  </Text>
                  <Paragraph style={{ marginLeft: 20, marginTop: 8 }}>
                    {context.intent.businessGoal}
                  </Paragraph>
                </div>
              )}

              {context.intent.reasoning && context.intent.reasoning.length > 0 && (
                <div>
                  <Text strong style={{ color: '#13c2c2' }}>
                    <InfoCircleOutlined /> Analysis Reasoning:
                  </Text>
                  <Timeline style={{ marginTop: 12 }}>
                    {context.intent.reasoning.map((reason, index) => (
                      <Timeline.Item
                        key={index}
                        color="blue"
                        dot={<InfoCircleOutlined style={{ fontSize: '12px' }} />}
                      >
                        <Text>{reason}</Text>
                      </Timeline.Item>
                    ))}
                  </Timeline>
                </div>
              )}

              {context.intent.recommendedActions && context.intent.recommendedActions.length > 0 && (
                <div>
                  <Text strong style={{ color: '#fa8c16' }}>
                    <ThunderboltOutlined /> Recommended Actions:
                  </Text>
                  <List
                    size="small"
                    style={{ marginTop: 8 }}
                    dataSource={context.intent.recommendedActions}
                    renderItem={(action, index) => (
                      <List.Item>
                        <List.Item.Meta
                          avatar={<Avatar size="small" style={{ backgroundColor: '#fa8c16' }}>{index + 1}</Avatar>}
                          description={action}
                        />
                      </List.Item>
                    )}
                  />
                </div>
              )}
            </Space>
          </Card>
        </Panel>

        {/* Enhanced Entities Section */}
        <Panel
          header={
            <Space>
              <TagsOutlined style={{ color: '#13c2c2' }} />
              <span style={{ fontWeight: 600 }}>Detected Entities</span>
              <Badge count={context.entities.length} style={{ backgroundColor: '#13c2c2' }} />
              {context.entities.length > 0 && (
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Avg Confidence: {Math.round((context.entities.reduce((sum, e) => sum + e.confidence, 0) / context.entities.length) * 100)}%
                </Text>
              )}
            </Space>
          }
          key="entities"
        >
          {context.entities.length > 0 ? (
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {/* Entity Summary */}
              <Row gutter={[16, 8]}>
                {context.entities.slice(0, 6).map((entity) => (
                  <Col key={entity.id} xs={12} sm={8} md={6}>
                    <Card
                      size="small"
                      hoverable={interactive}
                      onClick={() => interactive && handleEntityClick(entity.id)}
                      style={{
                        cursor: interactive ? 'pointer' : 'default',
                        border: selectedEntity === entity.id ? '2px solid #1890ff' : undefined
                      }}
                    >
                      <Statistic
                        title={
                          <Space>
                            <Tag color={entity.confidence > 0.8 ? 'green' : entity.confidence > 0.6 ? 'blue' : 'orange'}>
                              {entity.type}
                            </Tag>
                          </Space>
                        }
                        value={entity.name}
                        valueStyle={{ fontSize: '14px', fontWeight: 600 }}
                        suffix={
                          <Text type="secondary" style={{ fontSize: '10px' }}>
                            {Math.round(entity.confidence * 100)}%
                          </Text>
                        }
                      />
                    </Card>
                  </Col>
                ))}
              </Row>

              {/* Detailed Entity List */}
              {showEntityRelationships && (
                <List
                  size="small"
                  header={<Text strong>Entity Details & Relationships</Text>}
                  dataSource={context.entities}
                  renderItem={(entity) => (
                    <List.Item
                      actions={interactive ? [
                        <Button
                          key="explore"
                          type="link"
                          size="small"
                          onClick={() => handleEntityClick(entity.id)}
                        >
                          Explore
                        </Button>
                      ] : undefined}
                    >
                      <List.Item.Meta
                        avatar={
                          <Avatar
                            style={{
                              backgroundColor: entity.confidence > 0.8 ? '#52c41a' : '#1890ff'
                            }}
                          >
                            {entity.name.charAt(0).toUpperCase()}
                          </Avatar>
                        }
                        title={
                          <Space>
                            <Text strong>{entity.name}</Text>
                            <Tag color={entity.confidence > 0.8 ? 'green' : 'blue'}>
                              {entity.type}
                            </Tag>
                            <Progress
                              percent={Math.round(entity.confidence * 100)}
                              size="small"
                              style={{ width: 80 }}
                              showInfo={false}
                            />
                          </Space>
                        }
                        description={
                          <div>
                            {entity.businessMeaning && (
                              <Paragraph style={{ marginBottom: 4, fontSize: '12px' }}>
                                {entity.businessMeaning}
                              </Paragraph>
                            )}
                            {entity.suggestedColumns && entity.suggestedColumns.length > 0 && (
                              <div>
                                <Text strong style={{ fontSize: '11px' }}>Suggested Columns: </Text>
                                <Space wrap size="small">
                                  {entity.suggestedColumns.slice(0, 3).map((col, idx) => (
                                    <Tag key={idx} size="small" color="green">{col}</Tag>
                                  ))}
                                  {entity.suggestedColumns.length > 3 && (
                                    <Tag size="small" color="default">+{entity.suggestedColumns.length - 3} more</Tag>
                                  )}
                                </Space>
                              </div>
                            )}
                          </div>
                        }
                      />
                    </List.Item>
                  )}
                />
              )}
            </Space>
          ) : (
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description="No entities detected in this query"
            />
          )}
        </Panel>

        {/* Enhanced Domain Context Section */}
        {showDomainDetails && context.domain && (
          <Panel
            header={
              <Space>
                {getDomainIcon(context.domain.name)}
                <span style={{ fontWeight: 600 }}>Domain Context</span>
                <Tag color="purple">{context.domain.name}</Tag>
                <Progress
                  percent={Math.round(context.domain.relevance * 100)}
                  size="small"
                  style={{ width: 100 }}
                  strokeColor="#722ed1"
                />
              </Space>
            }
            key="domain"
          >
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <div>
                  <Title level={5} style={{ margin: 0, color: '#722ed1' }}>
                    {context.domain.name}
                  </Title>
                  <Text type="secondary">
                    Relevance Score: {Math.round(context.domain.relevance * 100)}%
                  </Text>
                </div>

                <Paragraph>{context.domain.description}</Paragraph>

                {context.domain.concepts.length > 0 && (
                  <div>
                    <Text strong style={{ color: '#13c2c2' }}>
                      <DatabaseOutlined /> Key Concepts:
                    </Text>
                    <div style={{ marginTop: 8 }}>
                      <Space wrap>
                        {context.domain.concepts.map((concept, index) => (
                          <Tag
                            key={index}
                            color="blue"
                            style={{ cursor: interactive ? 'pointer' : 'default' }}
                            onClick={() => interactive && handleDomainExplore(concept)}
                          >
                            {concept}
                          </Tag>
                        ))}
                      </Space>
                    </div>
                  </div>
                )}

                {context.domain.relationships.length > 0 && (
                  <div>
                    <Text strong style={{ color: '#fa8c16' }}>
                      <TeamOutlined /> Domain Relationships:
                    </Text>
                    <List
                      size="small"
                      style={{ marginTop: 8 }}
                      dataSource={context.domain.relationships}
                      renderItem={(relationship, index) => (
                        <List.Item>
                          <List.Item.Meta
                            avatar={<Avatar size="small" style={{ backgroundColor: '#fa8c16' }}>{index + 1}</Avatar>}
                            description={relationship}
                          />
                        </List.Item>
                      )}
                    />
                  </div>
                )}

                {interactive && (
                  <div style={{ textAlign: 'center', marginTop: 16 }}>
                    <Button
                      type="primary"
                      ghost
                      icon={<EyeOutlined />}
                      onClick={() => handleDomainExplore(context.domain!.name)}
                    >
                      Explore Domain
                    </Button>
                  </div>
                )}
              </Space>
            </Card>
          </Panel>
        )}

        {/* Enhanced Time Context Section */}
        {context.timeContext && (
          <Panel
            header={
              <Space>
                <ClockCircleOutlined style={{ color: '#fa8c16' }} />
                <span style={{ fontWeight: 600 }}>Time Context Analysis</span>
                <Tag color="orange">{context.timeContext.granularity}</Tag>
              </Space>
            }
            key="time"
          >
            <Card size="small">
              <Row gutter={[16, 16]}>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Time Period"
                    value={context.timeContext.period}
                    prefix={<ClockCircleOutlined />}
                    valueStyle={{ color: '#fa8c16' }}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Granularity"
                    value={context.timeContext.granularity}
                    prefix={<BarChartOutlined />}
                    valueStyle={{ color: '#13c2c2' }}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Reference Point"
                    value={context.timeContext.relativeTo || 'Absolute'}
                    prefix={<InfoCircleOutlined />}
                    valueStyle={{ color: '#722ed1' }}
                  />
                </Col>
              </Row>

              {context.timeContext.startDate && context.timeContext.endDate && (
                <div style={{ marginTop: 16 }}>
                  <Divider orientation="left" orientationMargin="0">
                    <Text strong>Date Range</Text>
                  </Divider>
                  <Timeline>
                    <Timeline.Item color="green">
                      <Text strong>Start: </Text>
                      <Text>{context.timeContext.startDate}</Text>
                    </Timeline.Item>
                    <Timeline.Item color="red">
                      <Text strong>End: </Text>
                      <Text>{context.timeContext.endDate}</Text>
                    </Timeline.Item>
                  </Timeline>
                </div>
              )}
            </Card>
          </Panel>
        )}

        {/* Enhanced Business Terms Section */}
        {context.businessTerms.length > 0 && (
          <Panel
            header={
              <Space>
                <BookOutlined style={{ color: '#722ed1' }} />
                <span style={{ fontWeight: 600 }}>Related Business Terms</span>
                <Badge count={context.businessTerms.length} style={{ backgroundColor: '#722ed1' }} />
              </Space>
            }
            key="terms"
          >
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text type="secondary">
                  Business terms and concepts identified in your query context
                </Text>
                <Space wrap size="middle">
                  {context.businessTerms.map((term, index) => (
                    <Tag
                      key={index}
                      color="purple"
                      style={{
                        cursor: interactive ? 'pointer' : 'default',
                        fontSize: '13px',
                        padding: '4px 8px'
                      }}
                      onClick={() => interactive && console.log('Explore term:', term)}
                    >
                      {term}
                    </Tag>
                  ))}
                </Space>
              </Space>
            </Card>
          </Panel>
        )}

        {/* User Context Section */}
        {showUserContext && context.userContext && (
          <Panel
            header={
              <Space>
                <UserOutlined style={{ color: '#52c41a' }} />
                <span style={{ fontWeight: 600 }}>User Context</span>
                <Tag color="green">{context.userContext.role}</Tag>
              </Space>
            }
            key="user"
          >
            <Card size="small">
              <Row gutter={[16, 16]}>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Role"
                    value={context.userContext.role}
                    prefix={<UserOutlined />}
                    valueStyle={{ color: '#52c41a' }}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Department"
                    value={context.userContext.department}
                    prefix={<TeamOutlined />}
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Access Level"
                    value={context.userContext.accessLevel}
                    prefix={<SettingOutlined />}
                    valueStyle={{ color: '#fa8c16' }}
                  />
                </Col>
              </Row>
            </Card>
          </Panel>
        )}
      </Collapse>
    </Space>
  )
}

export default BusinessContextPanel
