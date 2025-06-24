import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Tabs,
  Space,
  Typography,
  Button,
  Alert,
  Divider,
  Tag,
  Input
} from 'antd'
import {
  EyeOutlined,
  ApiOutlined,
  BarChartOutlined,
  PlayCircleOutlined,
  DatabaseOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import {
  ProcessFlowDashboard,
  ProcessFlowAnalytics,
  ProcessFlowSessionViewer,
  TokenUsageAnalyzer,
  PerformanceMetricsViewer
} from '@shared/components/ai/transparency'

const { Title, Text, Paragraph } = Typography

/**
 * ProcessFlowDemo - Demonstration page for ProcessFlow transparency system
 * 
 * Features:
 * - Interactive demo of all ProcessFlow components
 * - Sample data visualization
 * - Component showcase
 * - Educational content about ProcessFlow system
 */
export const ProcessFlowDemo: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [demoSessionId, setDemoSessionId] = useState('demo-session-12345')

  const demoFilters = {
    days: 7,
    includeDetails: true
  }

  const demoAnalyticsFilters = {
    days: 7,
    includeStepDetails: true,
    includeTokenUsage: true,
    includePerformanceMetrics: true
  }

  const renderOverviewTab = () => (
    <div>
      <Alert
        message="ProcessFlow Transparency System Demo"
        description="This demo showcases the comprehensive ProcessFlow transparency system with real-time monitoring, analytics, and detailed session tracking."
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="System Overview" extra={<EyeOutlined />}>
            <Paragraph>
              The ProcessFlow transparency system provides complete visibility into AI processing workflows:
            </Paragraph>
            <ul>
              <li><strong>Real-time Monitoring:</strong> Track active sessions and performance</li>
              <li><strong>Step-by-step Analysis:</strong> Detailed breakdown of each processing step</li>
              <li><strong>Token Usage Tracking:</strong> Comprehensive cost and usage analytics</li>
              <li><strong>Performance Metrics:</strong> Duration, confidence, and success rates</li>
              <li><strong>Transparency Data:</strong> Complete AI model transparency</li>
            </ul>
          </Card>
        </Col>
        
        <Col xs={24} lg={12}>
          <Card title="Key Features" extra={<ApiOutlined />}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Tag color="blue">Real-time Dashboard</Tag>
                <Text>Live metrics and monitoring</Text>
              </div>
              <div>
                <Tag color="green">Analytics Engine</Tag>
                <Text>Comprehensive data analysis</Text>
              </div>
              <div>
                <Tag color="orange">Session Tracking</Tag>
                <Text>Detailed session information</Text>
              </div>
              <div>
                <Tag color="purple">Cost Monitoring</Tag>
                <Text>Token usage and cost tracking</Text>
              </div>
              <div>
                <Tag color="red">Performance Analysis</Tag>
                <Text>Performance metrics and trends</Text>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>

      <Divider />

      <ProcessFlowDashboard 
        filters={demoFilters}
        showCharts={true}
        showMetrics={true}
      />
    </div>
  )

  const renderAnalyticsTab = () => (
    <div>
      <Alert
        message="ProcessFlow Analytics Demo"
        description="Comprehensive analytics showing token usage, performance trends, and cost analysis."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <ProcessFlowAnalytics 
        defaultFilters={demoAnalyticsFilters}
        showExport={true}
      />
    </div>
  )

  const renderSessionViewerTab = () => (
    <div>
      <Alert
        message="Session Viewer Demo"
        description="View detailed information about ProcessFlow sessions including steps, logs, and transparency data."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <Card style={{ marginBottom: 16 }}>
        <Space>
          <Text strong>Demo Session ID:</Text>
          <Input 
            value={demoSessionId}
            onChange={(e) => setDemoSessionId(e.target.value)}
            placeholder="Enter session ID"
            style={{ width: 200 }}
          />
          <Button type="primary" icon={<PlayCircleOutlined />}>
            Load Session
          </Button>
        </Space>
      </Card>
      
      <ProcessFlowSessionViewer
        sessionId={demoSessionId}
        showDetailedSteps={true}
        showLogs={true}
        showTransparency={true}
        compact={false}
      />
    </div>
  )

  const renderTokenUsageTab = () => (
    <div>
      <Alert
        message="Token Usage Analytics Demo"
        description="Detailed token usage analysis with cost breakdowns and model comparisons."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <TokenUsageAnalyzer 
        timeRange={7}
        showCostBreakdown={true}
        showModelComparison={true}
        compact={false}
      />
    </div>
  )

  const renderPerformanceTab = () => (
    <div>
      <Alert
        message="Performance Metrics Demo"
        description="Performance monitoring with trends, alerts, and detailed metrics analysis."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <PerformanceMetricsViewer 
        timeRange={7}
        showTrends={true}
        showAlerts={true}
        compact={false}
      />
    </div>
  )

  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <EyeOutlined />
          <span>Overview</span>
        </Space>
      ),
      children: renderOverviewTab()
    },
    {
      key: 'analytics',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Analytics</span>
        </Space>
      ),
      children: renderAnalyticsTab()
    },
    {
      key: 'session-viewer',
      label: (
        <Space>
          <DatabaseOutlined />
          <span>Session Viewer</span>
        </Space>
      ),
      children: renderSessionViewerTab()
    },
    {
      key: 'token-usage',
      label: (
        <Space>
          <DollarOutlined />
          <span>Token Usage</span>
        </Space>
      ),
      children: renderTokenUsageTab()
    },
    {
      key: 'performance',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: renderPerformanceTab()
    }
  ]

  return (
    <div style={{ padding: 24 }}>
      {/* Header */}
      <div style={{ marginBottom: 24 }}>
        <Title level={2}>
          <Space>
            <EyeOutlined />
            ProcessFlow Transparency Demo
          </Space>
        </Title>
        <Text type="secondary">
          Interactive demonstration of the ProcessFlow transparency system components and capabilities
        </Text>
      </div>

      {/* Demo Notice */}
      <Alert
        message="Demo Environment"
        description="This is a demonstration environment showcasing ProcessFlow transparency components. In a production environment, these components would display real data from your ProcessFlow sessions."
        type="warning"
        showIcon
        icon={<InfoCircleOutlined />}
        style={{ marginBottom: 24 }}
      />

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
      />
    </div>
  )
}

export default ProcessFlowDemo
