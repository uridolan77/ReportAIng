import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Form, 
  Switch, 
  InputNumber, 
  Select, 
  Slider, 
  Button, 
  Space, 
  Typography, 
  Divider,
  Alert,
  Tabs,
  Row,
  Col,
  Tooltip,
  notification
} from 'antd'
import {
  SettingOutlined,
  SaveOutlined,
  ReloadOutlined,
  SecurityScanOutlined,
  DatabaseOutlined,
  EyeOutlined,
  BellOutlined,
  ExportOutlined,
  ImportOutlined
} from '@ant-design/icons'
import { useGetTransparencySettingsQuery, useUpdateTransparencySettingsMutation } from '@shared/store/api/transparencyApi'

const { Title, Text, Paragraph } = Typography
const { TabPane } = Tabs
const { Option } = Select

export interface TransparencySettingsProps {
  onSettingsChange?: (settings: TransparencySettings) => void
  showAdvanced?: boolean
  className?: string
  testId?: string
}

export interface TransparencySettings {
  // Core Settings
  enableDetailedLogging: boolean
  confidenceThreshold: number
  retentionDays: number
  enableOptimizationSuggestions: boolean
  
  // Real-time Settings
  enableRealTimeTracking: boolean
  realTimeUpdateInterval: number
  maxConcurrentTracking: number
  
  // Display Settings
  showConfidenceIndicators: boolean
  showTokenUsage: boolean
  showProcessingTime: boolean
  showBusinessContext: boolean
  
  // Privacy & Security
  anonymizeUserData: boolean
  encryptTraces: boolean
  enableAuditLogging: boolean
  dataResidency: 'us-east' | 'us-west' | 'eu-west' | 'asia-pacific'
  
  // Performance Settings
  enableCaching: boolean
  cacheExpirationHours: number
  maxTraceSize: number
  compressionLevel: 'none' | 'low' | 'medium' | 'high'
  
  // Notification Settings
  enableAlerts: boolean
  lowConfidenceThreshold: number
  highLatencyThreshold: number
  errorRateThreshold: number
  
  // Export/Import Settings
  defaultExportFormat: 'json' | 'csv' | 'excel'
  includeMetadataInExport: boolean
  autoBackupEnabled: boolean
  backupFrequency: 'daily' | 'weekly' | 'monthly'
}

const defaultSettings: TransparencySettings = {
  // Core Settings
  enableDetailedLogging: true,
  confidenceThreshold: 0.7,
  retentionDays: 30,
  enableOptimizationSuggestions: true,
  
  // Real-time Settings
  enableRealTimeTracking: true,
  realTimeUpdateInterval: 1000,
  maxConcurrentTracking: 10,
  
  // Display Settings
  showConfidenceIndicators: true,
  showTokenUsage: true,
  showProcessingTime: true,
  showBusinessContext: true,
  
  // Privacy & Security
  anonymizeUserData: false,
  encryptTraces: true,
  enableAuditLogging: true,
  dataResidency: 'us-east',
  
  // Performance Settings
  enableCaching: true,
  cacheExpirationHours: 24,
  maxTraceSize: 1024,
  compressionLevel: 'medium',
  
  // Notification Settings
  enableAlerts: true,
  lowConfidenceThreshold: 0.6,
  highLatencyThreshold: 3000,
  errorRateThreshold: 0.05,
  
  // Export/Import Settings
  defaultExportFormat: 'json',
  includeMetadataInExport: true,
  autoBackupEnabled: true,
  backupFrequency: 'weekly'
}

/**
 * TransparencySettings - Comprehensive settings management for AI transparency
 * 
 * Features:
 * - Core transparency configuration
 * - Real-time monitoring settings
 * - Privacy and security controls
 * - Performance optimization
 * - Notification preferences
 * - Data export/import settings
 * - Advanced configuration options
 */
export const TransparencySettings: React.FC<TransparencySettingsProps> = ({
  onSettingsChange,
  showAdvanced = false,
  className = '',
  testId = 'transparency-settings'
}) => {
  const [form] = Form.useForm()
  const [settings, setSettings] = useState<TransparencySettings>(defaultSettings)
  const [hasChanges, setHasChanges] = useState(false)
  const [activeTab, setActiveTab] = useState('core')

  const { data: serverSettings, isLoading, refetch } = useGetTransparencySettingsQuery()
  const [updateSettings, { isLoading: isUpdating }] = useUpdateTransparencySettingsMutation()

  // Load settings from server
  useEffect(() => {
    if (serverSettings) {
      const mergedSettings = { ...defaultSettings, ...serverSettings }
      setSettings(mergedSettings)
      form.setFieldsValue(mergedSettings)
    }
  }, [serverSettings, form])

  const handleSettingChange = (changedValues: Partial<TransparencySettings>) => {
    const newSettings = { ...settings, ...changedValues }
    setSettings(newSettings)
    setHasChanges(true)
    onSettingsChange?.(newSettings)
  }

  const handleSave = async () => {
    try {
      await updateSettings(settings).unwrap()
      setHasChanges(false)
      notification.success({
        message: 'Settings Saved',
        description: 'Transparency settings have been updated successfully.'
      })
    } catch (error) {
      notification.error({
        message: 'Save Failed',
        description: 'Failed to save transparency settings. Please try again.'
      })
    }
  }

  const handleReset = () => {
    setSettings(defaultSettings)
    form.setFieldsValue(defaultSettings)
    setHasChanges(true)
  }

  const handleReload = () => {
    refetch()
    setHasChanges(false)
  }

  const renderCoreSettings = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card size="small" title="Basic Configuration">
        <Row gutter={[16, 16]}>
          <Col span={12}>
            <Form.Item
              label="Enable Detailed Logging"
              name="enableDetailedLogging"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Enable Optimization Suggestions"
              name="enableOptimizationSuggestions"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Confidence Threshold"
              name="confidenceThreshold"
              tooltip="Minimum confidence level for query responses"
            >
              <Slider
                min={0.1}
                max={1.0}
                step={0.05}
                marks={{
                  0.1: '10%',
                  0.5: '50%',
                  0.7: '70%',
                  0.9: '90%',
                  1.0: '100%'
                }}
                tooltip={{ formatter: (value) => `${(value! * 100).toFixed(0)}%` }}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Data Retention (Days)"
              name="retentionDays"
              tooltip="How long to keep transparency data"
            >
              <InputNumber
                min={1}
                max={365}
                style={{ width: '100%' }}
                formatter={(value) => `${value} days`}
                parser={(value) => value!.replace(' days', '')}
              />
            </Form.Item>
          </Col>
        </Row>
      </Card>

      <Card size="small" title="Display Preferences">
        <Row gutter={[16, 16]}>
          <Col span={12}>
            <Form.Item
              label="Show Confidence Indicators"
              name="showConfidenceIndicators"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Show Token Usage"
              name="showTokenUsage"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Show Processing Time"
              name="showProcessingTime"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Show Business Context"
              name="showBusinessContext"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
        </Row>
      </Card>
    </Space>
  )

  const renderRealTimeSettings = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card size="small" title="Real-time Monitoring">
        <Row gutter={[16, 16]}>
          <Col span={24}>
            <Form.Item
              label="Enable Real-time Tracking"
              name="enableRealTimeTracking"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Update Interval (ms)"
              name="realTimeUpdateInterval"
              tooltip="How often to update real-time data"
            >
              <Select style={{ width: '100%' }}>
                <Option value={500}>500ms (High frequency)</Option>
                <Option value={1000}>1s (Normal)</Option>
                <Option value={2000}>2s (Low frequency)</Option>
                <Option value={5000}>5s (Very low)</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Max Concurrent Tracking"
              name="maxConcurrentTracking"
              tooltip="Maximum number of queries to track simultaneously"
            >
              <InputNumber
                min={1}
                max={50}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
        </Row>
      </Card>
    </Space>
  )

  const renderSecuritySettings = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Alert
        message="Security & Privacy Settings"
        description="These settings affect data protection and compliance. Changes may require system restart."
        type="info"
        showIcon
      />
      
      <Card size="small" title="Data Protection">
        <Row gutter={[16, 16]}>
          <Col span={12}>
            <Form.Item
              label="Anonymize User Data"
              name="anonymizeUserData"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Encrypt Traces"
              name="encryptTraces"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Enable Audit Logging"
              name="enableAuditLogging"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Data Residency"
              name="dataResidency"
            >
              <Select style={{ width: '100%' }}>
                <Option value="us-east">US East</Option>
                <Option value="us-west">US West</Option>
                <Option value="eu-west">EU West</Option>
                <Option value="asia-pacific">Asia Pacific</Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>
      </Card>
    </Space>
  )

  const renderPerformanceSettings = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card size="small" title="Performance Optimization">
        <Row gutter={[16, 16]}>
          <Col span={12}>
            <Form.Item
              label="Enable Caching"
              name="enableCaching"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Cache Expiration (Hours)"
              name="cacheExpirationHours"
            >
              <InputNumber
                min={1}
                max={168}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Max Trace Size (KB)"
              name="maxTraceSize"
            >
              <InputNumber
                min={100}
                max={10240}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Compression Level"
              name="compressionLevel"
            >
              <Select style={{ width: '100%' }}>
                <Option value="none">None</Option>
                <Option value="low">Low</Option>
                <Option value="medium">Medium</Option>
                <Option value="high">High</Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>
      </Card>
    </Space>
  )

  const renderNotificationSettings = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card size="small" title="Alert Configuration">
        <Row gutter={[16, 16]}>
          <Col span={24}>
            <Form.Item
              label="Enable Alerts"
              name="enableAlerts"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              label="Low Confidence Threshold"
              name="lowConfidenceThreshold"
            >
              <InputNumber
                min={0.1}
                max={1.0}
                step={0.05}
                style={{ width: '100%' }}
                formatter={(value) => `${(value! * 100).toFixed(0)}%`}
                parser={(value) => parseFloat(value!.replace('%', '')) / 100}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              label="High Latency Threshold (ms)"
              name="highLatencyThreshold"
            >
              <InputNumber
                min={1000}
                max={10000}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              label="Error Rate Threshold"
              name="errorRateThreshold"
            >
              <InputNumber
                min={0.01}
                max={0.5}
                step={0.01}
                style={{ width: '100%' }}
                formatter={(value) => `${(value! * 100).toFixed(0)}%`}
                parser={(value) => parseFloat(value!.replace('%', '')) / 100}
              />
            </Form.Item>
          </Col>
        </Row>
      </Card>
    </Space>
  )

  return (
    <div className={`transparency-settings ${className}`} data-testid={testId}>
      <Card
        title={
          <Space>
            <SettingOutlined />
            <span>Transparency Settings</span>
          </Space>
        }
        extra={
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={handleReload}
              loading={isLoading}
              size="small"
            >
              Reload
            </Button>
            <Button
              icon={<ReloadOutlined />}
              onClick={handleReset}
              size="small"
            >
              Reset
            </Button>
            <Button
              type="primary"
              icon={<SaveOutlined />}
              onClick={handleSave}
              loading={isUpdating}
              disabled={!hasChanges}
              size="small"
            >
              Save Changes
            </Button>
          </Space>
        }
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={settings}
          onValuesChange={handleSettingChange}
        >
          <Tabs activeKey={activeTab} onChange={setActiveTab}>
            <TabPane
              tab={
                <Space>
                  <EyeOutlined />
                  <span>Core Settings</span>
                </Space>
              }
              key="core"
            >
              {renderCoreSettings()}
            </TabPane>

            <TabPane
              tab={
                <Space>
                  <BellOutlined />
                  <span>Real-time</span>
                </Space>
              }
              key="realtime"
            >
              {renderRealTimeSettings()}
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
              {renderSecuritySettings()}
            </TabPane>

            {showAdvanced && (
              <>
                <TabPane
                  tab={
                    <Space>
                      <DatabaseOutlined />
                      <span>Performance</span>
                    </Space>
                  }
                  key="performance"
                >
                  {renderPerformanceSettings()}
                </TabPane>

                <TabPane
                  tab={
                    <Space>
                      <BellOutlined />
                      <span>Notifications</span>
                    </Space>
                  }
                  key="notifications"
                >
                  {renderNotificationSettings()}
                </TabPane>
              </>
            )}
          </Tabs>
        </Form>

        {hasChanges && (
          <Alert
            message="Unsaved Changes"
            description="You have unsaved changes. Click 'Save Changes' to apply them."
            type="warning"
            showIcon
            style={{ marginTop: 16 }}
          />
        )}
      </Card>
    </div>
  )
}

export default TransparencySettings
