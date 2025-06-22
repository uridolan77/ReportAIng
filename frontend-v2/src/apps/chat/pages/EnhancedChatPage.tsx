import React, { useState, useEffect } from 'react'
import { Card, Row, Col, Space, Typography, Button, Alert, Switch, Tooltip, Badge, Divider, Statistic } from 'antd'
import {
  EyeOutlined,
  EyeInvisibleOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  BarChartOutlined,
  DatabaseOutlined,
  BulbOutlined,
  InfoCircleOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { ComprehensiveChatInterface } from '../components/ComprehensiveChatInterface'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { selectUser } from '@shared/store/auth'
import { useGetTransparencyDashboardMetricsQuery } from '@shared/store/api/transparencyApi'
import { transparencyConnectionManager } from '@shared/services/transparencyConnectionManager'
import { useRealTimeTransparency } from '@shared/components/ai/transparency/hooks/useAITransparency'

const { Title, Text, Paragraph } = Typography

/**
 * EnhancedChatPage - Dedicated page for the enhanced chat interface with AI transparency
 * 
 * Features:
 * - Comprehensive chat interface with transparency
 * - Real-time AI insights and monitoring
 * - Advanced mode toggle (Normal/Advanced)
 * - Business context analysis
 * - Performance metrics
 * - Query flow visualization
 */
export const EnhancedChatPage: React.FC = () => {
  const user = useAppSelector(selectUser)
  const [mode, setMode] = useState<'normal' | 'advanced'>('normal')
  const [showWelcome, setShowWelcome] = useState(true)

  // Get transparency metrics for the dashboard
  const { data: dashboardMetrics, isLoading: metricsLoading } = useGetTransparencyDashboardMetricsQuery({ days: 7 })

  // Real-time transparency connection
  const { isConnected, lastUpdate, connectionError } = useRealTimeTransparency(user?.id)

  // Auto-hide welcome after user interaction
  useEffect(() => {
    const timer = setTimeout(() => {
      setShowWelcome(false)
    }, 10000) // Hide after 10 seconds

    return () => clearTimeout(timer)
  }, [])

  const handleModeChange = (checked: boolean) => {
    setMode(checked ? 'advanced' : 'normal')
  }

  const renderWelcomeCard = () => {
    if (!showWelcome) return null

    return (
      <Alert
        message={
          <Space>
            <ThunderboltOutlined />
            <Text strong>Enhanced Chat Interface</Text>
          </Space>
        }
        description={
          <div>
            <Paragraph style={{ marginBottom: 12 }}>
              Welcome to the Enhanced Chat Interface with AI Transparency! This advanced interface provides:
            </Paragraph>
            <ul style={{ marginBottom: 12, paddingLeft: 20 }}>
              <li><strong>Real-time transparency:</strong> See how AI processes your queries step-by-step</li>
              <li><strong>Confidence indicators:</strong> Understand AI confidence levels for each response</li>
              <li><strong>Business context:</strong> View relevant business metadata and relationships</li>
              <li><strong>Performance metrics:</strong> Monitor query performance and optimization suggestions</li>
              <li><strong>Advanced analytics:</strong> Deep insights into AI decision-making processes</li>
            </ul>
            <Space>
              <Text type="secondary">
                Toggle between Normal and Advanced modes using the switch in the interface.
              </Text>
            </Space>
          </div>
        }
        type="info"
        showIcon
        closable
        onClose={() => setShowWelcome(false)}
        style={{ marginBottom: 24 }}
      />
    )
  }

  const renderModeInfo = () => {
    return (
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={16}>
          <Card size="small">
            <Row gutter={16} align="middle">
              <Col span={12}>
                <Space>
                  <Badge
                    status={mode === 'advanced' ? 'processing' : 'default'}
                    text={`${mode.charAt(0).toUpperCase() + mode.slice(1)} Mode`}
                  />
                  <Tooltip title={
                    mode === 'normal'
                      ? 'Normal mode provides standard chat with basic transparency features'
                      : 'Advanced mode includes comprehensive transparency tracking, business context analysis, and performance monitoring'
                  }>
                    <InfoCircleOutlined style={{ color: '#1890ff' }} />
                  </Tooltip>
                </Space>
              </Col>
              <Col span={12} style={{ textAlign: 'right' }}>
                <Space>
                  <Text type="secondary">Advanced Features:</Text>
                  <Switch
                    checked={mode === 'advanced'}
                    onChange={handleModeChange}
                    checkedChildren={<ThunderboltOutlined />}
                    unCheckedChildren={<SettingOutlined />}
                  />
                </Space>
              </Col>
            </Row>

            {mode === 'advanced' && (
              <>
                <Divider style={{ margin: '12px 0' }} />
                <Row gutter={16}>
                  <Col span={6}>
                    <Space>
                      <EyeOutlined style={{ color: '#52c41a' }} />
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Real-time Transparency
                      </Text>
                    </Space>
                  </Col>
                  <Col span={6}>
                    <Space>
                      <DatabaseOutlined style={{ color: '#1890ff' }} />
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Business Context
                      </Text>
                    </Space>
                  </Col>
                  <Col span={6}>
                    <Space>
                      <BarChartOutlined style={{ color: '#fa8c16' }} />
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Performance Metrics
                      </Text>
                    </Space>
                  </Col>
                  <Col span={6}>
                    <Space>
                      <BulbOutlined style={{ color: '#eb2f96' }} />
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        AI Analytics
                      </Text>
                    </Space>
                  </Col>
                </Row>
              </>
            )}
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small" loading={metricsLoading}>
            <Row gutter={8}>
              <Col span={12}>
                <Statistic
                  title="Total Traces"
                  value={dashboardMetrics?.totalTraces || 0}
                  prefix={<EyeOutlined />}
                  valueStyle={{ fontSize: '16px' }}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="Avg Confidence"
                  value={dashboardMetrics?.averageConfidence ? (dashboardMetrics.averageConfidence * 100).toFixed(0) : 0}
                  suffix="%"
                  prefix={<CheckCircleOutlined />}
                  valueStyle={{ fontSize: '16px', color: '#52c41a' }}
                />
              </Col>
            </Row>
            <Divider style={{ margin: '8px 0' }} />
            <Row gutter={8} align="middle">
              <Col span={12}>
                <Space size="small">
                  <Badge
                    status={isConnected ? 'processing' : 'error'}
                    text={isConnected ? 'Live' : 'Offline'}
                  />
                  <Text type="secondary" style={{ fontSize: '11px' }}>
                    Real-time
                  </Text>
                </Space>
              </Col>
              <Col span={12}>
                {lastUpdate && (
                  <Text type="secondary" style={{ fontSize: '11px' }}>
                    Updated: {new Date(lastUpdate).toLocaleTimeString()}
                  </Text>
                )}
                {connectionError && (
                  <Text type="danger" style={{ fontSize: '11px' }}>
                    Error: {connectionError}
                  </Text>
                )}
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>
    )
  }

  return (
    <PageLayout
      title="Enhanced Chat Interface"
      subtitle="AI-powered chat with comprehensive transparency and analytics"
      extra={
        <Space>
          <Button
            icon={<EyeOutlined />}
            onClick={() => window.open('/admin/ai-transparency', '_blank')}
          >
            Transparency Analysis
          </Button>
          <Button
            icon={<SettingOutlined />}
            onClick={() => {/* TODO: Open settings modal */}}
          >
            Settings
          </Button>
        </Space>
      }
    >
      <div style={{ height: 'calc(100vh - 200px)', display: 'flex', flexDirection: 'column' }}>
        {renderWelcomeCard()}
        {renderModeInfo()}
        
        <div style={{ flex: 1, minHeight: 0 }}>
          <ComprehensiveChatInterface
            mode={mode}
            showHeader={false}
            className="enhanced-chat-page"
          />
        </div>
      </div>
    </PageLayout>
  )
}

export default EnhancedChatPage
