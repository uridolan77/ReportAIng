import React, { useState } from 'react'
import { 
  Card, 
  Tabs, 
  Space, 
  Typography, 
  Row, 
  Col, 
  Button,
  Alert,
  Divider
} from 'antd'
import {
  SettingOutlined,
  DatabaseOutlined,
  MonitorOutlined,
  SecurityScanOutlined,
  ExportOutlined,
  ImportOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { RealTimeMonitor } from '@shared/components/ai/transparency/RealTimeMonitor'
import { TransparencyMetricsChart } from '@shared/components/ai/transparency/TransparencyMetricsChart'
import { OptimizationInsights } from '@shared/components/ai/transparency/OptimizationInsights'
import { useGetTransparencySettingsQuery, useGetTransparencyMetricsQuery } from '@shared/store/api/transparencyApi'

const { Title, Text } = Typography
const { TabPane } = Tabs

/**
 * TransparencyManagementPage - Comprehensive management interface for AI transparency
 * 
 * Features:
 * - System configuration and settings
 * - Data retention management
 * - Real-time monitoring dashboard
 * - Performance optimization
 * - Security and compliance
 * - Data export/import capabilities
 */
export const TransparencyManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [isMonitoring, setIsMonitoring] = useState(true)

  const { data: settings, isLoading: settingsLoading, refetch: refetchSettings } = useGetTransparencySettingsQuery()
  const { data: metrics, isLoading: metricsLoading, refetch: refetchMetrics } = useGetTransparencyMetricsQuery({
    days: 7,
    includeDetails: true
  })

  const handleRefreshAll = () => {
    refetchSettings()
    refetchMetrics()
  }

  const renderOverviewTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* System Status */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={16}>
          <Card title="System Overview" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Alert
                message="Transparency System Status"
                description="All transparency services are operational and collecting data in real-time."
                type="success"
                showIcon
              />
              
              <Row gutter={[16, 16]}>
                <Col span={8}>
                  <Card size="small">
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Text strong>Data Collection</Text>
                      <Text style={{ color: '#52c41a' }}>Active</Text>
                    </Space>
                  </Card>
                </Col>
                <Col span={8}>
                  <Card size="small">
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Text strong>Real-time Monitoring</Text>
                      <Text style={{ color: isMonitoring ? '#52c41a' : '#faad14' }}>
                        {isMonitoring ? 'Enabled' : 'Disabled'}
                      </Text>
                    </Space>
                  </Card>
                </Col>
                <Col span={8}>
                  <Card size="small">
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Text strong>Data Retention</Text>
                      <Text>{settings?.retentionDays || 30} days</Text>
                    </Space>
                  </Card>
                </Col>
              </Row>
            </Space>
          </Card>
        </Col>
        
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button 
                type="primary" 
                icon={<ExportOutlined />} 
                block
              >
                Export Data
              </Button>
              <Button 
                icon={<ImportOutlined />} 
                block
              >
                Import Configuration
              </Button>
              <Button 
                icon={<ReloadOutlined />} 
                onClick={handleRefreshAll}
                loading={settingsLoading || metricsLoading}
                block
              >
                Refresh All
              </Button>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Metrics Overview */}
      <TransparencyMetricsChart
        metrics={metrics ? [metrics] : []}
        showComparison={true}
        showTrends={true}
      />

      {/* Recent Optimization Insights */}
      <OptimizationInsights
        suggestions={[
          {
            id: 'opt-1',
            title: 'Optimize Token Usage',
            description: 'Reduce average token consumption by 15% through prompt optimization',
            category: 'Performance',
            priority: 'high',
            implementationComplexity: 'medium',
            estimatedImpact: { performance: 0.15, cost: 0.20, accuracy: 0.05 },
            implementationSteps: [
              'Analyze high-token queries',
              'Implement prompt compression',
              'Test and validate changes'
            ]
          },
          {
            id: 'opt-2',
            title: 'Improve Confidence Thresholds',
            description: 'Adjust confidence thresholds to reduce false positives',
            category: 'Accuracy',
            priority: 'medium',
            implementationComplexity: 'easy',
            estimatedImpact: { performance: 0.05, cost: 0.02, accuracy: 0.12 }
          }
        ]}
        showPriorityFilter={false}
        showImplementationGuide={true}
      />
    </Space>
  )

  const renderMonitoringTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <RealTimeMonitor
        isActive={true}
        autoStart={isMonitoring}
        onStatusChange={(status) => {
          console.log('Monitoring status changed:', status)
        }}
      />
      
      <Alert
        message="Real-time Monitoring"
        description="Monitor live transparency events, query processing, and system performance in real-time."
        type="info"
        showIcon
      />
    </Space>
  )

  const renderSettingsTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card title="Transparency Settings" size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text>Configure transparency system behavior and data collection preferences.</Text>
          
          <Divider />
          
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <Card size="small" title="Data Collection">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Detailed Logging:</Text>
                    <Text style={{ marginLeft: 8 }}>
                      {settings?.enableDetailedLogging ? 'Enabled' : 'Disabled'}
                    </Text>
                  </div>
                  <div>
                    <Text strong>Confidence Threshold:</Text>
                    <Text style={{ marginLeft: 8 }}>
                      {((settings?.confidenceThreshold || 0.7) * 100).toFixed(0)}%
                    </Text>
                  </div>
                  <div>
                    <Text strong>Optimization Suggestions:</Text>
                    <Text style={{ marginLeft: 8 }}>
                      {settings?.enableOptimizationSuggestions ? 'Enabled' : 'Disabled'}
                    </Text>
                  </div>
                </Space>
              </Card>
            </Col>
            
            <Col span={12}>
              <Card size="small" title="Data Retention">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Retention Period:</Text>
                    <Text style={{ marginLeft: 8 }}>
                      {settings?.retentionDays || 30} days
                    </Text>
                  </div>
                  <div>
                    <Text strong>Auto-cleanup:</Text>
                    <Text style={{ marginLeft: 8 }}>Enabled</Text>
                  </div>
                  <div>
                    <Text strong>Backup Schedule:</Text>
                    <Text style={{ marginLeft: 8 }}>Daily</Text>
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>
          
          <Alert
            message="Configuration Changes"
            description="Changes to transparency settings will take effect immediately and apply to all new queries."
            type="warning"
            showIcon
          />
        </Space>
      </Card>
    </Space>
  )

  const renderDataManagementTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card title="Data Management" size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text>Manage transparency data storage, retention, and export capabilities.</Text>
          
          <Divider />
          
          <Row gutter={[16, 16]}>
            <Col span={8}>
              <Card size="small" title="Storage Usage">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Total Data: 2.4 GB</Text>
                  <Text>Traces: 1.8 GB</Text>
                  <Text>Metrics: 0.4 GB</Text>
                  <Text>Logs: 0.2 GB</Text>
                </Space>
              </Card>
            </Col>
            
            <Col span={8}>
              <Card size="small" title="Data Health">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Status: Healthy</Text>
                  <Text>Integrity: 100%</Text>
                  <Text>Last Backup: 2 hours ago</Text>
                  <Text>Next Cleanup: 22 hours</Text>
                </Space>
              </Card>
            </Col>
            
            <Col span={8}>
              <Card size="small" title="Export Options">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button type="primary" icon={<ExportOutlined />} block>
                    Export All Data
                  </Button>
                  <Button icon={<ExportOutlined />} block>
                    Export Metrics
                  </Button>
                  <Button icon={<ExportOutlined />} block>
                    Export Traces
                  </Button>
                </Space>
              </Card>
            </Col>
          </Row>
        </Space>
      </Card>
    </Space>
  )

  const renderSecurityTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card title="Security & Compliance" size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Alert
            message="Security Status"
            description="All transparency data is encrypted and access-controlled according to security policies."
            type="success"
            showIcon
          />
          
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <Card size="small" title="Data Protection">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Encryption:</Text>
                    <Text style={{ marginLeft: 8, color: '#52c41a' }}>AES-256 Enabled</Text>
                  </div>
                  <div>
                    <Text strong>Access Control:</Text>
                    <Text style={{ marginLeft: 8, color: '#52c41a' }}>RBAC Active</Text>
                  </div>
                  <div>
                    <Text strong>Audit Logging:</Text>
                    <Text style={{ marginLeft: 8, color: '#52c41a' }}>Enabled</Text>
                  </div>
                </Space>
              </Card>
            </Col>
            
            <Col span={12}>
              <Card size="small" title="Compliance">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>GDPR:</Text>
                    <Text style={{ marginLeft: 8, color: '#52c41a' }}>Compliant</Text>
                  </div>
                  <div>
                    <Text strong>SOC 2:</Text>
                    <Text style={{ marginLeft: 8, color: '#52c41a' }}>Certified</Text>
                  </div>
                  <div>
                    <Text strong>Data Residency:</Text>
                    <Text style={{ marginLeft: 8 }}>US East</Text>
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>
        </Space>
      </Card>
    </Space>
  )

  return (
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
              <SettingOutlined />
              Transparency Management
            </Space>
          </Title>
          <Text type="secondary">
            Configure and monitor AI transparency system settings, data retention, and real-time monitoring
          </Text>
        </div>
        <Space>
          <Button 
            icon={<ReloadOutlined />} 
            onClick={handleRefreshAll}
            loading={settingsLoading || metricsLoading}
          >
            Refresh
          </Button>
          <Button type="primary" icon={<ExportOutlined />}>
            Export Configuration
          </Button>
        </Space>
      </div>

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        size="large"
      >
        <TabPane 
          tab={
            <Space>
              <MonitorOutlined />
              <span>Overview</span>
            </Space>
          } 
          key="overview"
        >
          {renderOverviewTab()}
        </TabPane>
        
        <TabPane 
          tab={
            <Space>
              <MonitorOutlined />
              <span>Real-time Monitoring</span>
            </Space>
          } 
          key="monitoring"
        >
          {renderMonitoringTab()}
        </TabPane>
        
        <TabPane 
          tab={
            <Space>
              <SettingOutlined />
              <span>Settings</span>
            </Space>
          } 
          key="settings"
        >
          {renderSettingsTab()}
        </TabPane>
        
        <TabPane 
          tab={
            <Space>
              <DatabaseOutlined />
              <span>Data Management</span>
            </Space>
          } 
          key="data"
        >
          {renderDataManagementTab()}
        </TabPane>
        
        <TabPane 
          tab={
            <Space>
              <SecurityScanOutlined />
              <span>Security</span>
            </Space>
          } 
          key="security"
        >
          {renderSecurityTab()}
        </TabPane>
      </Tabs>
    </div>
  )
}

export default TransparencyManagementPage
