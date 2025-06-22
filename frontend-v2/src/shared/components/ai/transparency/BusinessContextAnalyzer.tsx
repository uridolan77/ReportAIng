import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Tag, 
  Button, 
  Row, 
  Col, 
  Descriptions,
  Alert,
  Timeline,
  Progress,
  Tooltip,
  Collapse,
  List,
  Avatar
} from 'antd'
import {
  DatabaseOutlined,
  TagsOutlined,
  BranchesOutlined,
  UserOutlined,
  TeamOutlined,
  EnvironmentOutlined,
  ClockCircleOutlined,
  InfoCircleOutlined,
  BulbOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/transparency'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

export interface BusinessContextAnalyzerProps {
  context: BusinessContextProfile
  showEntityDetails?: boolean
  showRelationships?: boolean
  showRecommendations?: boolean
  onEntitySelect?: (entity: string) => void
  className?: string
  testId?: string
}

interface ContextAnalysis {
  domainCoverage: number
  entityRelevance: number
  contextCompleteness: number
  recommendations: string[]
  keyInsights: string[]
}

/**
 * BusinessContextAnalyzer - Analyzes and visualizes business context for transparency traces
 * 
 * Features:
 * - Business domain analysis
 * - Entity relationship mapping
 * - Context completeness assessment
 * - Domain-specific insights
 * - Entity interaction visualization
 * - Business rule compliance
 */
export const BusinessContextAnalyzer: React.FC<BusinessContextAnalyzerProps> = ({
  context,
  showEntityDetails = true,
  showRelationships = true,
  showRecommendations = true,
  onEntitySelect,
  className,
  testId = 'business-context-analyzer'
}) => {
  const [selectedEntity, setSelectedEntity] = useState<string | null>(null)
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['overview'])

  // Analyze business context
  const analysis = useMemo((): ContextAnalysis => {
    if (!context) {
      return {
        domainCoverage: 0,
        entityRelevance: 0,
        contextCompleteness: 0,
        recommendations: ['No business context available'],
        keyInsights: []
      }
    }

    // Calculate domain coverage based on available fields
    const availableFields = [
      context.domain,
      context.context,
      context.entities,
      context.businessRules,
      context.stakeholders,
      context.processes
    ].filter(Boolean)
    const domainCoverage = (availableFields.length / 6) * 100

    // Calculate entity relevance
    const entityCount = context.entities?.length || 0
    const entityRelevance = Math.min(100, (entityCount / 5) * 100) // Assume 5 entities is optimal

    // Calculate context completeness
    const hasDescription = !!context.context
    const hasEntities = entityCount > 0
    const hasRules = !!(context.businessRules?.length)
    const hasStakeholders = !!(context.stakeholders?.length)
    const contextCompleteness = [hasDescription, hasEntities, hasRules, hasStakeholders]
      .filter(Boolean).length / 4 * 100

    // Generate recommendations
    const recommendations: string[] = []
    if (domainCoverage < 50) recommendations.push('Consider enriching business domain information')
    if (entityCount === 0) recommendations.push('Add relevant business entities for better context')
    if (!context.businessRules?.length) recommendations.push('Define business rules for better decision making')
    if (!context.stakeholders?.length) recommendations.push('Identify key stakeholders for this process')
    if (!context.context) recommendations.push('Provide detailed business context description')

    // Generate key insights
    const keyInsights: string[] = []
    if (entityCount > 10) keyInsights.push('Rich entity context detected - good for complex analysis')
    if (context.businessRules?.length > 5) keyInsights.push('Comprehensive business rules available')
    if (context.stakeholders?.length > 3) keyInsights.push('Multiple stakeholders involved - consider coordination')
    if (context.domain) keyInsights.push(`Domain-specific context: ${context.domain}`)

    return {
      domainCoverage,
      entityRelevance,
      contextCompleteness,
      recommendations,
      keyInsights
    }
  }, [context])

  const handleEntityClick = (entity: string) => {
    setSelectedEntity(entity)
    onEntitySelect?.(entity)
  }

  const renderOverview = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {/* Context Metrics */}
      <Row gutter={[16, 16]}>
        <Col span={8}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Domain Coverage</Text>
              <Progress 
                percent={analysis.domainCoverage}
                strokeColor="#1890ff"
                size="small"
              />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {analysis.domainCoverage.toFixed(0)}% complete
              </Text>
            </Space>
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Entity Relevance</Text>
              <Progress 
                percent={analysis.entityRelevance}
                strokeColor="#52c41a"
                size="small"
              />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {context?.entities?.length || 0} entities identified
              </Text>
            </Space>
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Context Completeness</Text>
              <Progress 
                percent={analysis.contextCompleteness}
                strokeColor="#722ed1"
                size="small"
              />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {analysis.contextCompleteness.toFixed(0)}% complete
              </Text>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Basic Information */}
      <Descriptions column={2} size="small" bordered>
        <Descriptions.Item label="Domain">
          <Space>
            <DatabaseOutlined />
            <Text>{context?.domain || 'Not specified'}</Text>
          </Space>
        </Descriptions.Item>
        <Descriptions.Item label="Entity Count">
          <Space>
            <TagsOutlined />
            <Text>{context?.entities?.length || 0}</Text>
          </Space>
        </Descriptions.Item>
        <Descriptions.Item label="Business Rules">
          <Space>
            <BranchesOutlined />
            <Text>{context?.businessRules?.length || 0}</Text>
          </Space>
        </Descriptions.Item>
        <Descriptions.Item label="Stakeholders">
          <Space>
            <TeamOutlined />
            <Text>{context?.stakeholders?.length || 0}</Text>
          </Space>
        </Descriptions.Item>
      </Descriptions>

      {/* Context Description */}
      {context?.context && (
        <Card title="Business Context" size="small">
          <Paragraph>{context.context}</Paragraph>
        </Card>
      )}

      {/* Key Insights */}
      {analysis.keyInsights.length > 0 && (
        <Alert
          message="Key Insights"
          description={
            <ul style={{ margin: 0, paddingLeft: 16 }}>
              {analysis.keyInsights.map((insight, index) => (
                <li key={index}>{insight}</li>
              ))}
            </ul>
          }
          type="info"
          showIcon
          icon={<BulbOutlined />}
        />
      )}
    </Space>
  )

  const renderEntities = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {context?.entities && context.entities.length > 0 ? (
        <List
          dataSource={context.entities}
          renderItem={(entity, index) => (
            <List.Item
              style={{ 
                cursor: 'pointer',
                backgroundColor: selectedEntity === entity ? '#f0f8ff' : 'transparent'
              }}
              onClick={() => handleEntityClick(entity)}
            >
              <List.Item.Meta
                avatar={
                  <Avatar 
                    icon={<TagsOutlined />} 
                    style={{ backgroundColor: '#1890ff' }}
                  />
                }
                title={entity}
                description={`Entity ${index + 1} - Click for details`}
              />
              <Space>
                <Tag color="blue">Entity</Tag>
                {selectedEntity === entity && (
                  <CheckCircleOutlined style={{ color: '#52c41a' }} />
                )}
              </Space>
            </List.Item>
          )}
        />
      ) : (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <TagsOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No entities identified</Text>
          </Space>
        </div>
      )}
    </Space>
  )

  const renderBusinessRules = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {context?.businessRules && context.businessRules.length > 0 ? (
        <Timeline>
          {context.businessRules.map((rule, index) => (
            <Timeline.Item
              key={index}
              dot={<BranchesOutlined />}
              color="blue"
            >
              <Card size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Rule {index + 1}</Text>
                  <Paragraph style={{ margin: 0 }}>{rule}</Paragraph>
                </Space>
              </Card>
            </Timeline.Item>
          ))}
        </Timeline>
      ) : (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BranchesOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No business rules defined</Text>
          </Space>
        </div>
      )}
    </Space>
  )

  const renderStakeholders = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {context?.stakeholders && context.stakeholders.length > 0 ? (
        <List
          dataSource={context.stakeholders}
          renderItem={(stakeholder, index) => (
            <List.Item>
              <List.Item.Meta
                avatar={
                  <Avatar 
                    icon={<UserOutlined />} 
                    style={{ backgroundColor: '#52c41a' }}
                  />
                }
                title={stakeholder}
                description={`Stakeholder ${index + 1}`}
              />
              <Tag color="green">Stakeholder</Tag>
            </List.Item>
          )}
        />
      ) : (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <TeamOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No stakeholders identified</Text>
          </Space>
        </div>
      )}
    </Space>
  )

  const renderRecommendations = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {analysis.recommendations.map((recommendation, index) => (
        <Alert
          key={index}
          message={recommendation}
          type={recommendation.includes('No business context') ? 'warning' : 'info'}
          showIcon
          icon={<BulbOutlined />}
        />
      ))}
    </Space>
  )

  if (!context) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <DatabaseOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No business context available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <DatabaseOutlined />
          <span>Business Context Analysis</span>
          <Tag color="blue">{context.domain || 'Unknown Domain'}</Tag>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Collapse 
        activeKey={expandedPanels}
        onChange={setExpandedPanels}
        ghost
      >
        <Panel header="Overview" key="overview">
          {renderOverview()}
        </Panel>
        
        {showEntityDetails && (
          <Panel header={`Entities (${context?.entities?.length || 0})`} key="entities">
            {renderEntities()}
          </Panel>
        )}
        
        <Panel header={`Business Rules (${context?.businessRules?.length || 0})`} key="rules">
          {renderBusinessRules()}
        </Panel>
        
        <Panel header={`Stakeholders (${context?.stakeholders?.length || 0})`} key="stakeholders">
          {renderStakeholders()}
        </Panel>
        
        {showRecommendations && (
          <Panel header="Recommendations" key="recommendations">
            {renderRecommendations()}
          </Panel>
        )}
      </Collapse>
    </Card>
  )
}

export default BusinessContextAnalyzer
