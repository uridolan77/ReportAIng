import React, { useState, useCallback } from 'react'
import {
  Card,
  Spin,
  Empty,
  Alert,
  Space,
  Typography,
  Badge,
  Tooltip,
  Button,
  Collapse
} from 'antd'
import {
  BulbOutlined,
  TagsOutlined,
  ClockCircleOutlined,
  UserOutlined,
  EyeOutlined,
  SettingOutlined,
  BookOutlined,
  DatabaseOutlined
} from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

// Import section components
import IntentAnalysisSection from './sections/IntentAnalysisSection'
import EntityAnalysisSection from './sections/EntityAnalysisSection'
import DomainContextSection from './sections/DomainContextSection'
import TimeContextSection from './sections/TimeContextSection'
import BusinessTermsSection from './sections/BusinessTermsSection'
import UserContextSection from './sections/UserContextSection'

const { Title, Text } = Typography

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
  const [expandedSections, setExpandedSections] = useState<string[]>(['intent', 'entities'])

  const handleSectionToggle = useCallback((keys: string | string[]) => {
    setExpandedSections(Array.isArray(keys) ? keys : [keys])
  }, [])

  const getIntentColor = (type: string) => {
    switch (type) {
      case 'analytical': return 'blue'
      case 'operational': return 'green'
      case 'strategic': return 'purple'
      case 'exploratory': return 'orange'
      default: return 'default'
    }
  }

  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'simple': return 'green'
      case 'moderate': return 'orange'
      case 'complex': return 'red'
      default: return 'default'
    }
  }

  const getDomainIcon = (domainName: string) => {
    switch (domainName?.toLowerCase()) {
      case 'sales': return <DatabaseOutlined style={{ color: '#52c41a' }} />
      case 'marketing': return <DatabaseOutlined style={{ color: '#1890ff' }} />
      case 'finance': return <DatabaseOutlined style={{ color: '#fa8c16' }} />
      case 'operations': return <DatabaseOutlined style={{ color: '#722ed1' }} />
      default: return <DatabaseOutlined style={{ color: '#666' }} />
    }
  }

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
          description="No business context available"
        />
      </Card>
    )
  }

  const confidenceStatus = {
    status: context.confidence > 0.8 ? 'success' : context.confidence > 0.6 ? 'warning' : 'error',
    text: context.confidence > 0.8 ? 'High Confidence' : context.confidence > 0.6 ? 'Medium Confidence' : 'Low Confidence'
  }

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
        size="small"
      >
        <Alert
          message={confidenceStatus.text}
          description={`AI confidence in business context analysis: ${Math.round(context.confidence * 100)}%`}
          type={confidenceStatus.status}
          showIcon
          style={{ marginBottom: 16 }}
        />
      </Card>

      {/* Enhanced Collapsible Analysis Sections */}
      <Collapse
        activeKey={expandedSections}
        onChange={handleSectionToggle}
        size="large"
        ghost
        items={[
          // Intent Analysis Section
          {
            key: 'intent',
            label: (
              <Space>
                <BulbOutlined style={{ color: '#1890ff' }} />
                <span style={{ fontWeight: 600 }}>Intent Classification</span>
                <Badge
                  count={`${Math.round(context.intent.confidence * 100)}%`}
                  style={{ backgroundColor: '#52c41a' }}
                />
              </Space>
            ),
            children: (
              <IntentAnalysisSection 
                context={context} 
                interactive={interactive} 
              />
            )
          },
          // Enhanced Entities Section
          {
            key: 'entities',
            label: (
              <Space>
                <TagsOutlined style={{ color: '#13c2c2' }} />
                <span style={{ fontWeight: 600 }}>Detected Entities</span>
                <Badge count={context.entities.length} style={{ backgroundColor: '#13c2c2' }} />
              </Space>
            ),
            children: (
              <EntityAnalysisSection 
                context={context} 
                interactive={interactive}
                showEntityRelationships={showEntityRelationships}
                onEntityClick={onEntityClick}
              />
            )
          },
          // Enhanced Domain Context Section
          ...(showDomainDetails && context.domain ? [{
            key: 'domain',
            label: (
              <Space>
                {getDomainIcon(context.domain.name)}
                <span style={{ fontWeight: 600 }}>Domain Context</span>
                <Badge count={context.domain.name} style={{ backgroundColor: '#722ed1' }} />
              </Space>
            ),
            children: (
              <DomainContextSection 
                context={context} 
                interactive={interactive}
                onDomainExplore={onDomainExplore}
              />
            )
          }] : []),
          // Enhanced Time Context Section
          ...(context.timeContext ? [{
            key: 'time',
            label: (
              <Space>
                <ClockCircleOutlined style={{ color: '#fa8c16' }} />
                <span style={{ fontWeight: 600 }}>Time Context Analysis</span>
                <Badge count={context.timeContext.granularity} style={{ backgroundColor: '#fa8c16' }} />
              </Space>
            ),
            children: (
              <TimeContextSection context={context} />
            )
          }] : []),
          // Enhanced Business Terms Section
          ...(context.businessTerms.length > 0 ? [{
            key: 'terms',
            label: (
              <Space>
                <BookOutlined style={{ color: '#722ed1' }} />
                <span style={{ fontWeight: 600 }}>Related Business Terms</span>
                <Badge count={context.businessTerms.length} style={{ backgroundColor: '#722ed1' }} />
              </Space>
            ),
            children: (
              <BusinessTermsSection 
                context={context} 
                interactive={interactive}
              />
            )
          }] : []),
          // User Context Section
          ...(showUserContext && context.userContext ? [{
            key: 'user',
            label: (
              <Space>
                <UserOutlined style={{ color: '#52c41a' }} />
                <span style={{ fontWeight: 600 }}>User Context</span>
                <Badge count={context.userContext.role} style={{ backgroundColor: '#52c41a' }} />
              </Space>
            ),
            children: (
              <UserContextSection context={context} />
            )
          }] : [])
        ]}
      />
    </Space>
  )
}

export default BusinessContextPanel
