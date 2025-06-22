import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Button, 
  Select,
  Tabs,
  Alert,
  Divider,
  Badge
} from 'antd'
import {
  SettingOutlined,
  BarChartOutlined,
  DollarOutlined,
  ThunderboltOutlined,
  ExperimentOutlined,
  RobotOutlined
} from '@ant-design/icons'
import { 
  LLMProviderManager,
  ModelPerformanceAnalytics,
  CostOptimizationPanel,
  AIConfigurationManager
} from '@shared/components/ai/management'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'

const { Title, Text } = Typography

/**
 * AIManagementDemo - Comprehensive demo of AI management components
 * 
 * Features:
 * - LLM provider management with health monitoring
 * - Model performance analytics and benchmarking
 * - Cost optimization with AI-powered recommendations
 * - Comprehensive AI configuration management
 * - Real-time monitoring and alerts
 * - Integration with backend management APIs
 */
export const AIManagementDemo: React.FC = () => {
  const [activeTab, setActiveTab] = useState('providers')
  const [selectedProvider, setSelectedProvider] = useState<string | null>(null)

  // Handle provider selection
  const handleProviderSelect = (providerId: string) => {
    setSelectedProvider(providerId)
  }

  // Handle optimization application
  const handleOptimizationApply = (optimization: any) => {
    console.log('Applying optimization:', optimization)
    // In real implementation, this would trigger the optimization
  }

  // Handle configuration changes
  const handleConfigurationChange = (config: any) => {
    console.log('Configuration changed:', config)
    // In real implementation, this would update the configuration
  }

  const tabItems = [
    {
      key: 'providers',
      label: (
        <Space>
          <RobotOutlined />
          <span>Provider Management</span>
          <Badge count={8} size="small" />
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="LLM Provider Management"
            description="Monitor and manage AI model providers, configure settings, and track health status in real-time."
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <LLMProviderManager
            showMetrics={true}
            showConfiguration={true}
            compact={false}
            onProviderSelect={handleProviderSelect}
          />
        </div>
      )
    },
    {
      key: 'performance',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Performance Analytics</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="Model Performance Analytics"
            description="Analyze model performance, compare benchmarks, and identify optimization opportunities."
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <ModelPerformanceAnalytics
            timeRange={7}
            showComparison={true}
            showRecommendations={true}
            compact={false}
            onModelSelect={handleProviderSelect}
          />
        </div>
      )
    },
    {
      key: 'cost',
      label: (
        <Space>
          <DollarOutlined />
          <span>Cost Optimization</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="AI Cost Optimization"
            description="Track AI usage costs, identify savings opportunities, and implement cost optimization strategies."
            type="warning"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <CostOptimizationPanel
            timeRange={30}
            showRecommendations={true}
            showProjections={true}
            compact={false}
            onOptimizationApply={handleOptimizationApply}
          />
        </div>
      )
    },
    {
      key: 'configuration',
      label: (
        <Space>
          <SettingOutlined />
          <span>Configuration</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="AI System Configuration"
            description="Configure AI system settings, manage transparency options, and customize behavior."
            type="success"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <AIConfigurationManager
            agentId={selectedProvider || undefined}
            showAdvanced={true}
            showPresets={true}
            onConfigurationChange={handleConfigurationChange}
          />
        </div>
      )
    }
  ]

  return (
    <AIFeatureWrapper feature="aiManagementEnabled">
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
                <ThunderboltOutlined />
                AI Management Demo
              </Space>
            </Title>
            <Text type="secondary">
              Comprehensive AI system management, monitoring, and optimization
            </Text>
          </div>
          <Space>
            <Button icon={<ExperimentOutlined />}>
              Run Diagnostics
            </Button>
            <Button type="primary">
              Export Report
            </Button>
          </Space>
        </div>

        {/* Overview Cards */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <RobotOutlined style={{ fontSize: '24px', color: '#1890ff', marginBottom: 8 }} />
                <div style={{ fontSize: '20px', fontWeight: 'bold' }}>8</div>
                <div style={{ color: '#666', fontSize: '14px' }}>Active Providers</div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <BarChartOutlined style={{ fontSize: '24px', color: '#52c41a', marginBottom: 8 }} />
                <div style={{ fontSize: '20px', fontWeight: 'bold' }}>94.5%</div>
                <div style={{ color: '#666', fontSize: '14px' }}>Success Rate</div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <DollarOutlined style={{ fontSize: '24px', color: '#faad14', marginBottom: 8 }} />
                <div style={{ fontSize: '20px', fontWeight: 'bold' }}>$1,247</div>
                <div style={{ color: '#666', fontSize: '14px' }}>Monthly Cost</div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <ThunderboltOutlined style={{ fontSize: '24px', color: '#722ed1', marginBottom: 8 }} />
                <div style={{ fontSize: '20px', fontWeight: 'bold' }}>1,156ms</div>
                <div style={{ color: '#666', fontSize: '14px' }}>Avg Response</div>
              </div>
            </Card>
          </Col>
        </Row>

        {/* Main Management Interface */}
        <Card>
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={tabItems}
            size="large"
          />
        </Card>

        {/* Integration Information */}
        <Card style={{ marginTop: 24 }}>
          <Title level={4}>
            <Space>
              <ExperimentOutlined />
              Integration Information
            </Space>
          </Title>
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <div>
                <Text strong>Real API Integration:</Text>
                <ul style={{ marginTop: 8, paddingLeft: 20 }}>
                  <li>Connected to IntelligentAgentsController.cs</li>
                  <li>Real-time health monitoring via WebSocket</li>
                  <li>Configuration changes applied immediately</li>
                  <li>Performance metrics updated every 30 seconds</li>
                </ul>
              </div>
            </Col>
            <Col xs={24} lg={12}>
              <div>
                <Text strong>Management Features:</Text>
                <ul style={{ marginTop: 8, paddingLeft: 20 }}>
                  <li>Provider health monitoring and alerting</li>
                  <li>Performance benchmarking and comparison</li>
                  <li>AI-powered cost optimization recommendations</li>
                  <li>Advanced configuration with presets</li>
                </ul>
              </div>
            </Col>
          </Row>
          
          <Divider />
          
          <Alert
            message="Demo Features"
            description={
              <div>
                <Text>This demo showcases the complete AI management suite:</Text>
                <ul style={{ marginTop: 8, paddingLeft: 20, marginBottom: 0 }}>
                  <li><strong>Provider Management:</strong> Monitor health, configure settings, enable/disable providers</li>
                  <li><strong>Performance Analytics:</strong> Compare models, track trends, identify bottlenecks</li>
                  <li><strong>Cost Optimization:</strong> Track spending, implement savings strategies, project costs</li>
                  <li><strong>Configuration:</strong> Manage system settings, transparency options, advanced features</li>
                </ul>
              </div>
            }
            type="info"
            showIcon
          />
        </Card>
      </div>
    </AIFeatureWrapper>
  )
}

export default AIManagementDemo
