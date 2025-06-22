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
  Divider,
  Form,
  Switch,
  Slider,
  InputNumber,
  message
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
import {
  useGetTransparencySettingsQuery,
  useGetTransparencyMetricsQuery,
  useGetDataManagementStatsQuery,
  useGetOptimizationInsightsQuery,
  useGetRealTimeMonitoringDataQuery,
  useUpdateTransparencySettingsMutation,
  useExportTransparencyDataMutation,
  useTriggerDataCleanupMutation,
  useTransparencyManagement
} from '@shared/store/api/transparencyApi'
import { useGetSecuritySettingsQuery } from '@shared/store/api/adminApi'
import { useGetSystemHealthQuery } from '@shared/store/api/featuresApi'

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
  const [isImporting, setIsImporting] = useState(false)

  // Use the comprehensive hook for all data
  const {
    settings,
    metrics,
    monitoring,
    dataStats,
    optimizations,
    isLoading,
    error,
    updateSettings,
    triggerCleanup,
    refetch
  } = useTransparencyManagement()

  // Additional hooks for specific functionality
  const [exportData] = useExportTransparencyDataMutation()

  // Security and system health data
  const { data: securitySettings, isLoading: securityLoading } = useGetSecuritySettingsQuery()
  const { data: systemHealth, isLoading: healthLoading } = useGetSystemHealthQuery()

  const handleRefreshAll = () => {
    refetch()
  }

  const handleImportConfiguration = () => {
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = '.json'
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return

      setIsImporting(true)
      try {
        const text = await file.text()
        const config = JSON.parse(text)

        // Validate and apply configuration
        if (config.settings) {
          await updateSettings(config.settings).unwrap()
          message.success('Configuration imported successfully')
          refetch()
        } else {
          message.error('Invalid configuration file format')
        }
      } catch (error) {
        message.error('Failed to import configuration')
        console.error('Import error:', error)
      } finally {
        setIsImporting(false)
      }
    }
    input.click()
  }

  const renderOverviewTab = () => {
    if (isLoading) {
      return (
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Space direction="vertical">
            <div>Loading transparency overview...</div>
          </Space>
        </div>
      )
    }

    if (error) {
      return (
        <Alert
          message="Error Loading Overview"
          description="Failed to load transparency overview data. Please try refreshing."
          type="error"
          showIcon
          action={
            <Button size="small" onClick={handleRefreshAll}>
              Retry
            </Button>
          }
        />
      )
    }

    return (
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* System Status */}
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={16}>
            <Card title="System Overview" size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Alert
                  message="Transparency System Status"
                  description={`System is ${monitoring?.activeQueries !== undefined ? 'operational' : 'checking status'}. ${monitoring?.totalQueries || 0} total queries processed.`}
                  type={monitoring?.errorRate && monitoring.errorRate < 0.05 ? "success" : "warning"}
                  showIcon
                />

                <Row gutter={[16, 16]}>
                  <Col span={8}>
                    <Card size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Text strong>Data Collection</Text>
                        <Text style={{ color: settings?.enableDetailedLogging ? '#52c41a' : '#faad14' }}>
                          {settings?.enableDetailedLogging ? 'Active' : 'Limited'}
                        </Text>
                      </Space>
                    </Card>
                  </Col>
                  <Col span={8}>
                    <Card size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Text strong>Real-time Monitoring</Text>
                        <Text style={{ color: monitoring?.activeQueries !== undefined ? '#52c41a' : '#faad14' }}>
                          {monitoring?.activeQueries !== undefined ? 'Connected' : 'Disconnected'}
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
                  onClick={async () => {
                    try {
                      const blob = await exportData({
                        format: 'json',
                        dateRange: {
                          start: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
                          end: new Date().toISOString()
                        }
                      }).unwrap()

                      const url = window.URL.createObjectURL(blob)
                      const a = document.createElement('a')
                      a.href = url
                      a.download = `transparency-data-${new Date().toISOString().split('T')[0]}.json`
                      document.body.appendChild(a)
                      a.click()
                      window.URL.revokeObjectURL(url)
                      document.body.removeChild(a)
                    } catch (error) {
                      console.error('Export failed:', error)
                    }
                  }}
                  block
                >
                  Export Data
                </Button>
                <Button
                  icon={<ImportOutlined />}
                  onClick={handleImportConfiguration}
                  loading={isImporting}
                  block
                >
                  Import Configuration
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  onClick={handleRefreshAll}
                  loading={isLoading}
                  block
                >
                  Refresh All
                </Button>
              </Space>
            </Card>
          </Col>
        </Row>

        {/* Real-time Statistics */}
        <Row gutter={[16, 16]}>
          <Col xs={24} md={6}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text type="secondary">Active Queries</Text>
                <Text strong style={{ fontSize: '24px' }}>
                  {monitoring?.activeQueries || 0}
                </Text>
              </Space>
            </Card>
          </Col>
          <Col xs={24} md={6}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text type="secondary">Average Confidence</Text>
                <Text strong style={{ fontSize: '24px' }}>
                  {monitoring?.averageConfidence ? `${(monitoring.averageConfidence * 100).toFixed(1)}%` : 'N/A'}
                </Text>
              </Space>
            </Card>
          </Col>
          <Col xs={24} md={6}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text type="secondary">Error Rate</Text>
                <Text strong style={{ fontSize: '24px', color: monitoring?.errorRate && monitoring.errorRate > 0.05 ? '#ff4d4f' : '#52c41a' }}>
                  {monitoring?.errorRate ? `${(monitoring.errorRate * 100).toFixed(2)}%` : '0%'}
                </Text>
              </Space>
            </Card>
          </Col>
          <Col xs={24} md={6}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text type="secondary">Avg Processing Time</Text>
                <Text strong style={{ fontSize: '24px' }}>
                  {monitoring?.averageProcessingTime ? `${monitoring.averageProcessingTime.toFixed(0)}ms` : 'N/A'}
                </Text>
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
          suggestions={optimizations || []}
          showPriorityFilter={false}
          showImplementationGuide={true}
        />
      </Space>
    )
  }

  const renderMonitoringTab = () => {
    if (isLoading) {
      return (
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Space direction="vertical">
            <div>Loading real-time monitoring data...</div>
          </Space>
        </div>
      )
    }

    return (
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Connection Status */}
        <Alert
          message="Real-time Monitoring Status"
          description={
            monitoring
              ? `Connected - Last update: ${new Date(monitoring.lastUpdate).toLocaleTimeString()}`
              : "Connecting to real-time monitoring..."
          }
          type={monitoring ? "success" : "warning"}
          showIcon
          action={
            <Button size="small" onClick={handleRefreshAll}>
              Refresh
            </Button>
          }
        />

        {/* Real-time Statistics Grid */}
        <Row gutter={[16, 16]}>
          <Col xs={24} md={8}>
            <Card title="Active Queries" size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#1890ff' }}>
                  {monitoring?.activeQueries || 0}
                </div>
                <Text type="secondary">Currently Processing</Text>
              </div>
            </Card>
          </Col>
          <Col xs={24} md={8}>
            <Card title="Total Queries" size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#52c41a' }}>
                  {monitoring?.totalQueries || 0}
                </div>
                <Text type="secondary">Today</Text>
              </div>
            </Card>
          </Col>
          <Col xs={24} md={8}>
            <Card title="Error Rate" size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{
                  fontSize: '32px',
                  fontWeight: 'bold',
                  color: monitoring?.errorRate && monitoring.errorRate > 0.05 ? '#ff4d4f' : '#52c41a'
                }}>
                  {monitoring?.errorRate ? `${(monitoring.errorRate * 100).toFixed(1)}%` : '0%'}
                </div>
                <Text type="secondary">Error Rate</Text>
              </div>
            </Card>
          </Col>
        </Row>

        {/* Real-time Monitor Component */}
        <RealTimeMonitor
          isActive={true}
          autoStart={isMonitoring}
          onStatusChange={(status) => {
            setIsMonitoring(status === 'active')
          }}
          data={monitoring}
        />

        {/* Performance Metrics */}
        <Row gutter={[16, 16]}>
          <Col xs={24} md={12}>
            <Card title="Performance Metrics" size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Average Processing Time:</Text>
                  <Text strong>
                    {monitoring?.averageProcessingTime ? `${monitoring.averageProcessingTime.toFixed(0)}ms` : 'N/A'}
                  </Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Average Confidence:</Text>
                  <Text strong>
                    {monitoring?.averageConfidence ? `${(monitoring.averageConfidence * 100).toFixed(1)}%` : 'N/A'}
                  </Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>System Status:</Text>
                  <Text strong style={{ color: monitoring ? '#52c41a' : '#faad14' }}>
                    {monitoring ? 'Operational' : 'Checking...'}
                  </Text>
                </div>
              </Space>
            </Card>
          </Col>
          <Col xs={24} md={12}>
            <Card title="Monitoring Controls" size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Button
                  type={isMonitoring ? "default" : "primary"}
                  onClick={() => setIsMonitoring(!isMonitoring)}
                  block
                >
                  {isMonitoring ? 'Pause Monitoring' : 'Resume Monitoring'}
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  onClick={handleRefreshAll}
                  loading={isLoading}
                  block
                >
                  Refresh Data
                </Button>
              </Space>
            </Card>
          </Col>
        </Row>
      </Space>
    )
  }

  const renderSettingsTab = () => {
    const [form] = Form.useForm()
    const [isUpdating, setIsUpdating] = useState(false)

    // Initialize form with current settings
    React.useEffect(() => {
      if (settings) {
        form.setFieldsValue({
          enableDetailedLogging: settings.enableDetailedLogging,
          confidenceThreshold: Math.round(settings.confidenceThreshold * 100),
          retentionDays: settings.retentionDays,
          enableOptimizationSuggestions: settings.enableOptimizationSuggestions
        })
      }
    }, [settings, form])

    const handleSaveSettings = async (values: any) => {
      setIsUpdating(true)
      try {
        await updateSettings({
          enableDetailedLogging: values.enableDetailedLogging,
          confidenceThreshold: values.confidenceThreshold / 100,
          retentionDays: values.retentionDays,
          enableOptimizationSuggestions: values.enableOptimizationSuggestions
        }).unwrap()

        message.success('Settings updated successfully')
        refetch() // Refresh all data
      } catch (error) {
        message.error('Failed to update settings')
        console.error('Settings update error:', error)
      } finally {
        setIsUpdating(false)
      }
    }

    if (isLoading) {
      return (
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Space direction="vertical">
            <div>Loading transparency settings...</div>
          </Space>
        </div>
      )
    }

    return (
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Card title="Transparency Settings" size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text>Configure transparency system behavior and data collection preferences.</Text>

            <Divider />

            <Form
              form={form}
              layout="vertical"
              onFinish={handleSaveSettings}
              disabled={isUpdating}
            >
              <Row gutter={[24, 16]}>
                <Col span={12}>
                  <Card size="small" title="Data Collection Settings">
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Form.Item
                        name="enableDetailedLogging"
                        label="Detailed Logging"
                        valuePropName="checked"
                      >
                        <Switch
                          checkedChildren="Enabled"
                          unCheckedChildren="Disabled"
                        />
                      </Form.Item>

                      <Form.Item
                        name="confidenceThreshold"
                        label="Confidence Threshold (%)"
                      >
                        <Slider
                          min={50}
                          max={95}
                          marks={{
                            50: '50%',
                            70: '70%',
                            85: '85%',
                            95: '95%'
                          }}
                          tooltip={{ formatter: (value) => `${value}%` }}
                        />
                      </Form.Item>

                      <Form.Item
                        name="enableOptimizationSuggestions"
                        label="Optimization Suggestions"
                        valuePropName="checked"
                      >
                        <Switch
                          checkedChildren="Enabled"
                          unCheckedChildren="Disabled"
                        />
                      </Form.Item>
                    </Space>
                  </Card>
                </Col>

                <Col span={12}>
                  <Card size="small" title="Data Retention Settings">
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Form.Item
                        name="retentionDays"
                        label="Data Retention Period (days)"
                        rules={[
                          { required: true, message: 'Please enter retention period' },
                          { type: 'number', min: 1, max: 365, message: 'Must be between 1 and 365 days' }
                        ]}
                      >
                        <InputNumber
                          min={1}
                          max={365}
                          style={{ width: '100%' }}
                          placeholder="Enter days"
                        />
                      </Form.Item>

                      <div style={{ marginTop: 16 }}>
                        <Text strong>Current Storage:</Text>
                        <div style={{ marginTop: 8 }}>
                          <Text>Total Size: {dataStats?.totalDataSize ? `${(dataStats.totalDataSize / 1024 / 1024 / 1024).toFixed(2)} GB` : 'Loading...'}</Text>
                        </div>
                        <div>
                          <Text>Health: </Text>
                          <Text style={{
                            color: dataStats?.dataHealth?.status === 'healthy' ? '#52c41a' :
                                   dataStats?.dataHealth?.status === 'warning' ? '#faad14' : '#ff4d4f'
                          }}>
                            {dataStats?.dataHealth?.status || 'Unknown'}
                          </Text>
                        </div>
                      </div>
                    </Space>
                  </Card>
                </Col>
              </Row>

              <Divider />

              <Row justify="end">
                <Space>
                  <Button onClick={() => form.resetFields()}>
                    Reset
                  </Button>
                  <Button
                    type="primary"
                    htmlType="submit"
                    loading={isUpdating}
                  >
                    Save Settings
                  </Button>
                </Space>
              </Row>
            </Form>

            <Alert
              message="Configuration Changes"
              description="Changes to transparency settings will take effect immediately and apply to all new queries."
              type="info"
              showIcon
            />
          </Space>
        </Card>
      </Space>
    )
  }

  const renderDataManagementTab = () => {
    const [isExporting, setIsExporting] = useState(false)
    const [isCleaningUp, setIsCleaningUp] = useState(false)

    const handleExport = async (format: 'json' | 'csv' | 'excel', type?: 'all' | 'metrics' | 'traces') => {
      setIsExporting(true)
      try {
        const exportParams: any = {
          format,
          dateRange: {
            start: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
            end: new Date().toISOString()
          }
        }

        const blob = await exportData(exportParams).unwrap()

        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `transparency-${type || 'all'}-${new Date().toISOString().split('T')[0]}.${format}`
        document.body.appendChild(a)
        a.click()
        window.URL.revokeObjectURL(url)
        document.body.removeChild(a)

        message.success(`${type || 'All'} data exported successfully`)
      } catch (error) {
        message.error('Export failed')
        console.error('Export error:', error)
      } finally {
        setIsExporting(false)
      }
    }

    const handleCleanup = async () => {
      setIsCleaningUp(true)
      try {
        await triggerCleanup({
          olderThanDays: settings?.retentionDays || 30,
          dryRun: false
        }).unwrap()

        message.success('Data cleanup completed successfully')
        refetch() // Refresh data stats
      } catch (error) {
        message.error('Cleanup failed')
        console.error('Cleanup error:', error)
      } finally {
        setIsCleaningUp(false)
      }
    }

    if (isLoading) {
      return (
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Space direction="vertical">
            <div>Loading data management information...</div>
          </Space>
        </div>
      )
    }

    return (
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Card title="Data Management" size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text>Manage transparency data storage, retention, and export capabilities.</Text>

            <Divider />

            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Card size="small" title="Storage Usage">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Text strong>
                      Total Data: {dataStats?.totalDataSize ? `${(dataStats.totalDataSize / 1024 / 1024 / 1024).toFixed(2)} GB` : 'Loading...'}
                    </Text>
                    <Text>
                      Traces: {dataStats?.tracesSize ? `${(dataStats.tracesSize / 1024 / 1024 / 1024).toFixed(2)} GB` : 'Loading...'}
                    </Text>
                    <Text>
                      Metrics: {dataStats?.metricsSize ? `${(dataStats.metricsSize / 1024 / 1024 / 1024).toFixed(2)} GB` : 'Loading...'}
                    </Text>
                    <Text>
                      Logs: {dataStats?.logsSize ? `${(dataStats.logsSize / 1024 / 1024 / 1024).toFixed(2)} GB` : 'Loading...'}
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={8}>
                <Card size="small" title="Data Health">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Text strong>
                      Status: <span style={{
                        color: dataStats?.dataHealth?.status === 'healthy' ? '#52c41a' :
                               dataStats?.dataHealth?.status === 'warning' ? '#faad14' : '#ff4d4f'
                      }}>
                        {dataStats?.dataHealth?.status || 'Unknown'}
                      </span>
                    </Text>
                    <Text>
                      Integrity: {dataStats?.dataHealth?.integrity ? `${dataStats.dataHealth.integrity}%` : 'Checking...'}
                    </Text>
                    <Text>
                      Last Backup: {dataStats?.dataHealth?.lastBackup ? new Date(dataStats.dataHealth.lastBackup).toLocaleString() : 'Unknown'}
                    </Text>
                    <Text>
                      Next Cleanup: {dataStats?.dataHealth?.nextCleanup ? new Date(dataStats.dataHealth.nextCleanup).toLocaleString() : 'Unknown'}
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={8}>
                <Card size="small" title="Export Options">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button
                      type="primary"
                      icon={<ExportOutlined />}
                      onClick={() => handleExport('json', 'all')}
                      loading={isExporting}
                      block
                    >
                      Export All Data
                    </Button>
                    <Button
                      icon={<ExportOutlined />}
                      onClick={() => handleExport('json', 'metrics')}
                      loading={isExporting}
                      block
                    >
                      Export Metrics
                    </Button>
                    <Button
                      icon={<ExportOutlined />}
                      onClick={() => handleExport('json', 'traces')}
                      loading={isExporting}
                      block
                    >
                      Export Traces
                    </Button>
                  </Space>
                </Card>
              </Col>
            </Row>

            <Divider />

            {/* Data Management Actions */}
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Card size="small" title="Maintenance Actions">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button
                      danger
                      onClick={handleCleanup}
                      loading={isCleaningUp}
                      block
                    >
                      Run Data Cleanup
                    </Button>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Removes data older than {settings?.retentionDays || 30} days
                    </Text>
                  </Space>
                </Card>
              </Col>
              <Col span={12}>
                <Card size="small" title="Export Formats">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button
                      onClick={() => handleExport('csv', 'all')}
                      loading={isExporting}
                      block
                    >
                      Export as CSV
                    </Button>
                    <Button
                      onClick={() => handleExport('excel', 'all')}
                      loading={isExporting}
                      block
                    >
                      Export as Excel
                    </Button>
                  </Space>
                </Card>
              </Col>
            </Row>
          </Space>
        </Card>
      </Space>
    )
  }

  const renderSecurityTab = () => {
    if (securityLoading || healthLoading) {
      return (
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Space direction="vertical">
            <div>Loading security information...</div>
          </Space>
        </div>
      )
    }

    const overallSecurityStatus = systemHealth?.status === 'healthy' ? 'success' :
                                  systemHealth?.status === 'warning' ? 'warning' : 'error'

    return (
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Card title="Security & Compliance" size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Alert
              message="Security Status"
              description={`System security status: ${systemHealth?.status || 'Unknown'}. ${systemHealth?.services?.length || 0} services monitored.`}
              type={overallSecurityStatus}
              showIcon
              action={
                <Button size="small" onClick={handleRefreshAll}>
                  Refresh
                </Button>
              }
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
                      <Text strong>Two-Factor Auth:</Text>
                      <Text style={{
                        marginLeft: 8,
                        color: securitySettings?.enableTwoFactorAuth ? '#52c41a' : '#faad14'
                      }}>
                        {securitySettings?.enableTwoFactorAuth ? 'Enabled' : 'Optional'}
                      </Text>
                    </div>
                    <div>
                      <Text strong>Session Timeout:</Text>
                      <Text style={{ marginLeft: 8 }}>
                        {securitySettings?.sessionTimeout ? `${securitySettings.sessionTimeout} minutes` : 'Default'}
                      </Text>
                    </div>
                  </Space>
                </Card>
              </Col>

              <Col span={12}>
                <Card size="small" title="Compliance & Monitoring">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>Password Policy:</Text>
                      <Text style={{ marginLeft: 8, color: '#52c41a' }}>
                        {securitySettings?.passwordPolicy?.minLength ? 'Enforced' : 'Basic'}
                      </Text>
                    </div>
                    <div>
                      <Text strong>Login Attempts:</Text>
                      <Text style={{ marginLeft: 8 }}>
                        Max {securitySettings?.maxLoginAttempts || 5} attempts
                      </Text>
                    </div>
                    <div>
                      <Text strong>Lockout Duration:</Text>
                      <Text style={{ marginLeft: 8 }}>
                        {securitySettings?.lockoutDuration ? `${securitySettings.lockoutDuration} minutes` : 'Default'}
                      </Text>
                    </div>
                    <div>
                      <Text strong>Allowed Domains:</Text>
                      <Text style={{ marginLeft: 8 }}>
                        {securitySettings?.allowedDomains?.length || 0} configured
                      </Text>
                    </div>
                  </Space>
                </Card>
              </Col>
            </Row>

            <Divider />

            {/* System Services Status */}
            <Card size="small" title="System Services">
              <Row gutter={[16, 16]}>
                {systemHealth?.services?.map((service, index) => (
                  <Col xs={24} md={8} key={index}>
                    <Card size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Text strong>{service.name}</Text>
                          <Text style={{
                            color: service.status === 'up' ? '#52c41a' :
                                   service.status === 'degraded' ? '#faad14' : '#ff4d4f'
                          }}>
                            {service.status.toUpperCase()}
                          </Text>
                        </div>
                        <div>
                          <Text type="secondary">Response Time: {service.responseTime}ms</Text>
                        </div>
                        <div>
                          <Text type="secondary">
                            Last Check: {new Date(service.lastCheck).toLocaleTimeString()}
                          </Text>
                        </div>
                      </Space>
                    </Card>
                  </Col>
                )) || (
                  <Col span={24}>
                    <Text type="secondary">No service information available</Text>
                  </Col>
                )}
              </Row>
            </Card>

            {/* Security Alerts */}
            {systemHealth?.alerts && systemHealth.alerts.length > 0 && (
              <Card size="small" title="Security Alerts">
                <Space direction="vertical" style={{ width: '100%' }}>
                  {systemHealth.alerts.map((alert, index) => (
                    <Alert
                      key={index}
                      message={alert.message}
                      type={alert.level === 'error' ? 'error' : alert.level === 'warning' ? 'warning' : 'info'}
                      showIcon
                      closable
                      description={`${new Date(alert.timestamp).toLocaleString()}`}
                    />
                  ))}
                </Space>
              </Card>
            )}
          </Space>
        </Card>
      </Space>
    )
  }

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
            loading={isLoading}
          >
            Refresh
          </Button>
          <Button
            type="primary"
            icon={<ExportOutlined />}
            onClick={async () => {
              try {
                const blob = await exportData({
                  format: 'json',
                  dateRange: {
                    start: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
                    end: new Date().toISOString()
                  }
                }).unwrap()

                const url = window.URL.createObjectURL(blob)
                const a = document.createElement('a')
                a.href = url
                a.download = `transparency-configuration-${new Date().toISOString().split('T')[0]}.json`
                document.body.appendChild(a)
                a.click()
                window.URL.revokeObjectURL(url)
                document.body.removeChild(a)

                message.success('Configuration exported successfully')
              } catch (error) {
                message.error('Export failed')
                console.error('Export error:', error)
              }
            }}
          >
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
