import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Typography,
  Space,
  Button,
  Badge,
  Tooltip,
  Drawer,
  message,
  Tabs
} from 'antd'
import {
  DashboardOutlined,
  BulbOutlined,
  RocketOutlined,
  BarChartOutlined,
  ExperimentOutlined,
  StarOutlined,
  ExportOutlined,
  ThunderboltOutlined,
  SettingOutlined,
  BellOutlined
} from '@ant-design/icons'
import { ComprehensiveAnalyticsDashboard } from './ComprehensiveAnalyticsDashboard'
import { TemplateImprovementDashboard } from './TemplateImprovementDashboard'
import {
  ContentQualityAnalyzer,
  TemplateOptimizationInterface,
  PerformancePredictionInterface,
  TemplateVariantsGenerator,
  AdvancedExportInterface,
  MLRecommendationEngine,
  RealTimeAnalyticsMonitor
} from '../../components/template-analytics'
import { useTemplateAnalyticsHub } from '@shared/hooks/useTemplateAnalyticsHub'

const { Title, Text } = Typography

type AnalyticsView = 
  | 'dashboard' 
  | 'improvement' 
  | 'quality' 
  | 'optimization' 
  | 'prediction' 
  | 'variants' 
  | 'export' 
  | 'recommendations' 
  | 'realtime'

export const EnhancedTemplateAnalytics: React.FC = () => {
  // State
  const [currentView, setCurrentView] = useState<AnalyticsView>('dashboard')
  const [selectedTemplate, setSelectedTemplate] = useState<string>('')
  const [notificationsVisible, setNotificationsVisible] = useState(false)
  const [recentAlerts, setRecentAlerts] = useState<any[]>([])

  // Real-time connection
  const { 
    isConnected, 
    lastUpdate,
    error: connectionError 
  } = useTemplateAnalyticsHub({
    autoConnect: true,
    subscribeToAlerts: true,
    subscribeToPerformanceUpdates: true,
    onNewAlert: (alert) => {
      setRecentAlerts(prev => [alert, ...prev.slice(0, 9)])
      message.warning(`New Alert: ${alert.message}`)
    }
  })

  // Tab items configuration
  const tabItems = [
    {
      key: 'dashboard',
      label: (
        <Space>
          <DashboardOutlined />
          Dashboard
        </Space>
      ),
      children: <ComprehensiveAnalyticsDashboard />
    },
    {
      key: 'improvement',
      label: (
        <Space>
          <BulbOutlined />
          Improvement
        </Space>
      ),
      children: (
        <TemplateImprovementDashboard
          templateKey={selectedTemplate}
          onTemplateSelect={(templateKey) => {
            setSelectedTemplate(templateKey)
            message.info(`Selected template: ${templateKey}`)
          }}
        />
      )
    },
    {
      key: 'quality',
      label: (
        <Space>
          <StarOutlined />
          Quality
        </Space>
      ),
      children: (
        <ContentQualityAnalyzer
          onAnalysisComplete={(analysis) => {
            message.success(`Quality analysis completed with score: ${(analysis.overallQualityScore * 100).toFixed(1)}%`)
          }}
        />
      )
    },
    {
      key: 'optimization',
      label: (
        <Space>
          <RocketOutlined />
          Optimization
        </Space>
      ),
      children: selectedTemplate ? (
        <TemplateOptimizationInterface
          templateKey={selectedTemplate}
          templateName={`Template: ${selectedTemplate}`}
          originalContent="// Original template content would be loaded here"
          onOptimizationComplete={(result) => {
            message.success(`Optimization completed with ${result.expectedPerformanceImprovement.toFixed(1)}% expected improvement`)
          }}
          onCreateABTest={(original, optimized) => {
            message.success('A/B test created successfully')
          }}
        />
      ) : (
        <Card style={{ textAlign: 'center', padding: '60px 0' }}>
          <RocketOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
          <Title level={4}>Select a Template to Optimize</Title>
          <Text type="secondary">
            Choose a template from the improvement dashboard to begin optimization
          </Text>
        </Card>
      )
    },
    {
      key: 'prediction',
      label: (
        <Space>
          <BulbOutlined />
          Prediction
        </Space>
      ),
      children: (
        <PerformancePredictionInterface
          onPredictionComplete={(prediction) => {
            message.success(`Performance prediction completed: ${(prediction.predictedSuccessRate * 100).toFixed(1)}% success rate`)
          }}
        />
      )
    },
    {
      key: 'variants',
      label: (
        <Space>
          <ExperimentOutlined />
          Variants
        </Space>
      ),
      children: selectedTemplate ? (
        <TemplateVariantsGenerator
          templateKey={selectedTemplate}
          templateName={`Template: ${selectedTemplate}`}
          originalContent="// Original template content would be loaded here"
          onVariantsGenerated={(variants) => {
            message.success(`Generated ${variants.length} template variants`)
          }}
          onCreateABTest={(variants) => {
            message.success(`Created A/B tests for ${variants.length} variants`)
          }}
        />
      ) : (
        <Card style={{ textAlign: 'center', padding: '60px 0' }}>
          <ExperimentOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
          <Title level={4}>Select a Template for Variant Generation</Title>
          <Text type="secondary">
            Choose a template to generate A/B testing variants
          </Text>
        </Card>
      )
    },
    {
      key: 'recommendations',
      label: (
        <Space>
          <ThunderboltOutlined />
          ML Recommendations
        </Space>
      ),
      children: (
        <MLRecommendationEngine
          onRecommendationApply={(recommendation) => {
            message.success(`Applied recommendation: ${recommendation.title}`)
          }}
        />
      )
    },
    {
      key: 'realtime',
      label: (
        <Space>
          <BarChartOutlined />
          Real-time
        </Space>
      ),
      children: (
        <RealTimeAnalyticsMonitor
          showAlerts={true}
          showPerformanceUpdates={true}
          showABTestUpdates={true}
        />
      )
    },
    {
      key: 'export',
      label: (
        <Space>
          <ExportOutlined />
          Export
        </Space>
      ),
      children: (
        <AdvancedExportInterface
          onExportComplete={(config) => {
            message.success(`Export completed in ${config.format} format`)
          }}
        />
      )
    }
  ]



  return (
    <div style={{ padding: '24px', background: '#f5f5f5', minHeight: '100vh' }}>
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space direction="vertical" size={0}>
              <Title level={2} style={{ margin: 0, color: '#1890ff' }}>
                <BulbOutlined style={{ marginRight: '8px' }} />
                Enhanced Template Analytics
              </Title>
              <Text type="secondary">
                AI-Powered Template Intelligence & Performance Optimization
              </Text>
            </Space>
          </Col>
          <Col>
            <Space>
              {/* Connection Status */}
              <div style={{
                padding: '8px 12px',
                background: isConnected ? '#f6ffed' : '#fff2e8',
                border: `1px solid ${isConnected ? '#b7eb8f' : '#ffbb96'}`,
                borderRadius: '6px',
                display: 'flex',
                alignItems: 'center',
                gap: '8px'
              }}>
                <div style={{
                  width: '8px',
                  height: '8px',
                  borderRadius: '50%',
                  backgroundColor: isConnected ? '#52c41a' : '#fa8c16'
                }} />
                <Text style={{ fontSize: '12px' }}>
                  {isConnected ? 'Real-time Connected' : 'Connecting...'}
                </Text>
              </div>

              {selectedTemplate && (
                <Badge.Ribbon text="Selected" color="blue">
                  <Card size="small" style={{ minWidth: '120px' }}>
                    <Text strong style={{ fontSize: '12px' }}>
                      {selectedTemplate}
                    </Text>
                  </Card>
                </Badge.Ribbon>
              )}

              <Tooltip title="Notifications">
                <Button
                  icon={<BellOutlined />}
                  onClick={() => setNotificationsVisible(true)}
                  style={{ position: 'relative' }}
                >
                  {recentAlerts.length > 0 && (
                    <Badge
                      count={recentAlerts.length}
                      size="small"
                      style={{ position: 'absolute', top: '-5px', right: '-5px' }}
                    />
                  )}
                </Button>
              </Tooltip>

              <Button icon={<SettingOutlined />}>
                Settings
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Connection Error Alert */}
      {connectionError && (
        <Card style={{ marginBottom: '24px' }}>
          <Space>
            <Text type="danger">Connection Error:</Text>
            <Text>{connectionError}</Text>
          </Space>
        </Card>
      )}

      {/* Main Content with Tabs */}
      <Card>
        <Tabs
          activeKey={currentView}
          onChange={(key) => setCurrentView(key as AnalyticsView)}
          items={tabItems}
          size="large"
          tabBarStyle={{ marginBottom: '24px' }}
        />
      </Card>

      {/* Notifications Drawer */}
      <Drawer
        title="Recent Alerts"
        placement="right"
        onClose={() => setNotificationsVisible(false)}
        open={notificationsVisible}
        width={400}
      >
        {recentAlerts.length > 0 ? (
          <div>
            {recentAlerts.map((alert, index) => (
              <Card key={index} size="small" style={{ marginBottom: '8px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <Text strong style={{ fontSize: '12px' }}>{alert.templateKey}</Text>
                    <div style={{ fontSize: '11px', color: '#666' }}>
                      {alert.message}
                    </div>
                  </div>
                  <Text type="secondary" style={{ fontSize: '10px' }}>
                    {new Date(alert.timestamp).toLocaleTimeString()}
                  </Text>
                </div>
              </Card>
            ))}
          </div>
        ) : (
          <div style={{ textAlign: 'center', padding: '40px 0' }}>
            <BellOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
            <Text type="secondary">No recent alerts</Text>
          </div>
        )}
      </Drawer>
    </div>
  )
}

export default EnhancedTemplateAnalytics
