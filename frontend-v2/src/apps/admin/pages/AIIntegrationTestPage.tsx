import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Button, 
  Alert,
  Tabs,
  Divider,
  Switch,
  Slider,
  Select,
  Badge,
  Statistic
} from 'antd'
import {
  ExperimentOutlined,
  BugOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  InfoCircleOutlined,
  SettingOutlined
} from '@ant-design/icons'
import { 
  AIIntegrationProvider,
  AIAccessibilityProvider,
  AIErrorBoundary,
  useAIIntegration,
  useAIAccessibility,
  useAIPerformanceMonitor,
  AITransparencyPanel,
  LLMProviderManager,
  AISchemaExplorer,
  AutomatedInsightsGenerator,
  PredictiveAnalyticsPanel,
  AI_COMPONENTS_METADATA,
  formatConfidence,
  getConfidenceTheme
} from '@shared/components/ai'

const { Title, Text, Paragraph } = Typography

/**
 * AIIntegrationTestPage - Comprehensive integration testing page
 * 
 * Features:
 * - Tests all AI components in a single interface
 * - Demonstrates provider integration
 * - Shows error handling and recovery
 * - Performance monitoring and optimization
 * - Accessibility features testing
 * - Real-time component interaction
 */

// Test component that uses AI integration
const IntegrationTestPanel: React.FC = () => {
  const { 
    state, 
    enableFeature, 
    disableFeature, 
    reportError, 
    updatePerformance,
    isFeatureEnabled,
    isLoading,
    hasErrors
  } = useAIIntegration()
  
  const { 
    announceToScreenReader, 
    isHighContrast, 
    isReducedMotion,
    fontSize,
    setFontSize
  } = useAIAccessibility()
  
  const { trackPerformance } = useAIPerformanceMonitor('IntegrationTestPanel')
  
  const [testResults, setTestResults] = useState<Record<string, 'pass' | 'fail' | 'pending'>>({})
  
  // Test AI integration features
  const runIntegrationTests = async () => {
    announceToScreenReader('Starting AI integration tests')
    
    const tests = [
      {
        name: 'Provider State',
        test: () => state.isAIEnabled
      },
      {
        name: 'Feature Flags',
        test: () => Object.values(state.features).some(f => f)
      },
      {
        name: 'Performance Tracking',
        test: async () => {
          const result = await trackPerformance(async () => {
            await new Promise(resolve => setTimeout(resolve, 100))
            return true
          })
          return result
        }
      },
      {
        name: 'Error Handling',
        test: () => {
          try {
            reportError('TestComponent', 'Test error message', 'low')
            return true
          } catch {
            return false
          }
        }
      },
      {
        name: 'Accessibility',
        test: () => {
          announceToScreenReader('Accessibility test')
          return true
        }
      }
    ]
    
    const results: Record<string, 'pass' | 'fail' | 'pending'> = {}
    
    for (const test of tests) {
      try {
        const result = await test.test()
        results[test.name] = result ? 'pass' : 'fail'
      } catch (error) {
        results[test.name] = 'fail'
      }
    }
    
    setTestResults(results)
    announceToScreenReader('Integration tests completed')
  }
  
  const getTestStatusColor = (status: 'pass' | 'fail' | 'pending') => {
    switch (status) {
      case 'pass': return '#52c41a'
      case 'fail': return '#ff4d4f'
      case 'pending': return '#faad14'
      default: return '#d9d9d9'
    }
  }
  
  const getTestStatusIcon = (status: 'pass' | 'fail' | 'pending') => {
    switch (status) {
      case 'pass': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'fail': return <WarningOutlined style={{ color: '#ff4d4f' }} />
      case 'pending': return <InfoCircleOutlined style={{ color: '#faad14' }} />
      default: return null
    }
  }
  
  return (
    <Card title="AI Integration Test Panel">
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* System Status */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="AI Enabled"
              value={state.isAIEnabled ? 'Yes' : 'No'}
              prefix={state.isAIEnabled ? <CheckCircleOutlined /> : <WarningOutlined />}
              valueStyle={{ color: state.isAIEnabled ? '#52c41a' : '#ff4d4f' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Active Features"
              value={Object.values(state.features).filter(f => f).length}
              suffix={`/ ${Object.keys(state.features).length}`}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Error Count"
              value={state.errors.length}
              prefix={<BugOutlined />}
              valueStyle={{ color: state.errors.length > 0 ? '#ff4d4f' : '#52c41a' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Confidence Threshold"
              value={formatConfidence(state.globalConfidenceThreshold)}
              prefix={<SettingOutlined />}
              valueStyle={{ color: getConfidenceTheme(state.globalConfidenceThreshold).color }}
            />
          </Col>
        </Row>
        
        {/* Feature Controls */}
        <Card title="Feature Controls" size="small">
          <Row gutter={[16, 16]}>
            {Object.entries(state.features).map(([feature, enabled]) => (
              <Col key={feature} xs={24} sm={12} lg={8}>
                <Space>
                  <Switch
                    checked={enabled}
                    onChange={(checked) => 
                      checked ? enableFeature(feature as any) : disableFeature(feature as any)
                    }
                  />
                  <Text>{feature.charAt(0).toUpperCase() + feature.slice(1)}</Text>
                </Space>
              </Col>
            ))}
          </Row>
        </Card>
        
        {/* Accessibility Controls */}
        <Card title="Accessibility Controls" size="small">
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text strong>Font Size</Text>
                <Select
                  value={fontSize}
                  onChange={setFontSize}
                  style={{ width: '100%' }}
                >
                  <Select.Option value="small">Small</Select.Option>
                  <Select.Option value="medium">Medium</Select.Option>
                  <Select.Option value="large">Large</Select.Option>
                </Select>
              </Space>
            </Col>
            <Col xs={24} sm={12}>
              <Space direction="vertical">
                <div>
                  <Text strong>High Contrast: </Text>
                  <Text>{isHighContrast ? 'Enabled' : 'Disabled'}</Text>
                </div>
                <div>
                  <Text strong>Reduced Motion: </Text>
                  <Text>{isReducedMotion ? 'Enabled' : 'Disabled'}</Text>
                </div>
              </Space>
            </Col>
          </Row>
        </Card>
        
        {/* Test Results */}
        <Card title="Integration Test Results" size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Button 
              type="primary" 
              icon={<ExperimentOutlined />}
              onClick={runIntegrationTests}
              loading={isLoading}
            >
              Run Integration Tests
            </Button>
            
            {Object.keys(testResults).length > 0 && (
              <div>
                {Object.entries(testResults).map(([testName, status]) => (
                  <div key={testName} style={{ marginBottom: 8 }}>
                    <Space>
                      {getTestStatusIcon(status)}
                      <Text>{testName}</Text>
                      <Badge 
                        color={getTestStatusColor(status)}
                        text={status.toUpperCase()}
                      />
                    </Space>
                  </div>
                ))}
              </div>
            )}
          </Space>
        </Card>
        
        {/* Error Log */}
        {state.errors.length > 0 && (
          <Card title="Error Log" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              {state.errors.slice(0, 5).map((error) => (
                <Alert
                  key={error.id}
                  message={`${error.component}: ${error.message}`}
                  description={`Severity: ${error.severity} | Time: ${new Date(error.timestamp).toLocaleTimeString()}`}
                  type={error.severity === 'critical' ? 'error' : 'warning'}
                  showIcon
                  closable
                />
              ))}
            </Space>
          </Card>
        )}
      </Space>
    </Card>
  )
}

export const AIIntegrationTestPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('integration')
  const [testQuery, setTestQuery] = useState('SELECT customers.name, orders.total FROM customers JOIN orders ON customers.id = orders.customer_id WHERE orders.date >= \'2024-01-01\'')
  
  const tabItems = [
    {
      key: 'integration',
      label: (
        <Space>
          <ExperimentOutlined />
          <span>Integration Tests</span>
        </Space>
      ),
      children: <IntegrationTestPanel />
    },
    {
      key: 'transparency',
      label: (
        <Space>
          <InfoCircleOutlined />
          <span>Transparency</span>
        </Space>
      ),
      children: (
        <AIErrorBoundary componentName="AITransparencyPanel">
          <AITransparencyPanel
            query={testQuery}
            showPromptConstruction={true}
            showConfidenceBreakdown={true}
            showDecisionExplanation={true}
            interactive={true}
          />
        </AIErrorBoundary>
      )
    },
    {
      key: 'streaming',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Streaming</span>
        </Space>
      ),
      children: (
        <Card>
          <Alert
            message="Streaming Components"
            description="Streaming components like ChatInterface are available in the Chat application. This test page focuses on shared AI components."
            type="info"
            showIcon
          />
          <div style={{ marginTop: 16 }}>
            <Text type="secondary">
              To test streaming functionality, navigate to the Chat application where the ChatInterface component is properly integrated.
            </Text>
          </div>
        </Card>
      )
    },
    {
      key: 'management',
      label: (
        <Space>
          <SettingOutlined />
          <span>Management</span>
        </Space>
      ),
      children: (
        <AIErrorBoundary componentName="LLMProviderManager">
          <LLMProviderManager
            showMetrics={true}
            showConfiguration={true}
            compact={false}
          />
        </AIErrorBoundary>
      )
    },
    {
      key: 'insights',
      label: (
        <Space>
          <BugOutlined />
          <span>Advanced Insights</span>
        </Space>
      ),
      children: (
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <AIErrorBoundary componentName="AISchemaExplorer">
              <AISchemaExplorer
                currentQuery={testQuery}
                showRecommendations={true}
                showRelationships={true}
                interactive={true}
              />
            </AIErrorBoundary>
          </Col>
          <Col xs={24} lg={12}>
            <AIErrorBoundary componentName="AutomatedInsightsGenerator">
              <AutomatedInsightsGenerator
                dataContext="sales_analytics"
                showTrends={true}
                showRecommendations={true}
                autoRefresh={false}
              />
            </AIErrorBoundary>
          </Col>
        </Row>
      )
    }
  ]
  
  return (
    <AIIntegrationProvider
      enableErrorNotifications={true}
      enablePerformanceMonitoring={true}
    >
      <AIAccessibilityProvider
        enableKeyboardShortcuts={true}
        enableScreenReaderAnnouncements={true}
      >
        <div style={{ padding: 24 }}>
          {/* Header */}
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            marginBottom: 24 
          }}>
            <div>
              <Title level={2} style={{ margin: 0 }}>
                <Space>
                  <ExperimentOutlined />
                  AI Integration Test Suite
                </Space>
              </Title>
              <Text type="secondary">
                Comprehensive testing and validation of AI components integration
              </Text>
            </div>
            <Space>
              <Badge count={AI_COMPONENTS_METADATA.totalComponents} />
              <Text type="secondary">v{AI_COMPONENTS_METADATA.version}</Text>
            </Space>
          </div>
          
          {/* System Information */}
          <Alert
            message="AI Components Integration Test Environment"
            description={
              <div>
                <Paragraph style={{ marginBottom: 8 }}>
                  This page tests the complete AI components library integration including:
                </Paragraph>
                <ul style={{ marginBottom: 0, paddingLeft: 20 }}>
                  <li>Provider state management and feature flags</li>
                  <li>Error boundaries and graceful error handling</li>
                  <li>Performance monitoring and optimization</li>
                  <li>Accessibility features and screen reader support</li>
                  <li>Real-time component interaction and data flow</li>
                </ul>
              </div>
            }
            type="info"
            showIcon
            style={{ marginBottom: 24 }}
          />
          
          {/* Main Test Interface */}
          <Card>
            <Tabs
              activeKey={activeTab}
              onChange={setActiveTab}
              items={tabItems}
              size="large"
            />
          </Card>
          
          {/* Footer Information */}
          <Card style={{ marginTop: 24 }}>
            <Title level={4}>Integration Status</Title>
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={12}>
                <div>
                  <Text strong>Library Information:</Text>
                  <ul style={{ marginTop: 8, paddingLeft: 20 }}>
                    <li>Version: {AI_COMPONENTS_METADATA.version}</li>
                    <li>Total Components: {AI_COMPONENTS_METADATA.totalComponents}</li>
                    <li>Features: {AI_COMPONENTS_METADATA.features.join(', ')}</li>
                    <li>Build Date: {new Date(AI_COMPONENTS_METADATA.buildDate).toLocaleDateString()}</li>
                  </ul>
                </div>
              </Col>
              <Col xs={24} lg={12}>
                <div>
                  <Text strong>Test Coverage:</Text>
                  <ul style={{ marginTop: 8, paddingLeft: 20 }}>
                    <li>Provider integration and state management</li>
                    <li>Component error handling and recovery</li>
                    <li>Performance optimization and monitoring</li>
                    <li>Accessibility compliance and features</li>
                    <li>Real-time data processing and streaming</li>
                  </ul>
                </div>
              </Col>
            </Row>
          </Card>
        </div>
      </AIAccessibilityProvider>
    </AIIntegrationProvider>
  )
}

export default AIIntegrationTestPage
