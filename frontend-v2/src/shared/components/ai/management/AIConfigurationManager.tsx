import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Form, 
  Input, 
  InputNumber, 
  Switch, 
  Select, 
  Button, 
  Space, 
  Typography, 
  Divider,
  Alert,
  Tabs,
  Row,
  Col,
  Slider,
  Tooltip,
  Badge,
  Tag
} from 'antd'
import {
  SettingOutlined,
  SaveOutlined,
  ReloadOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  SecurityScanOutlined,
  ExperimentOutlined,
  DatabaseOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { 
  useGetAgentCapabilitiesQuery, 
  useUpdateAgentSettingsMutation 
} from '@shared/store/api/intelligentAgentsApi'
import { useGetTransparencySettingsQuery, useUpdateTransparencySettingsMutation } from '@shared/store/api/transparencyApi'
import type { AgentCapabilities } from '@shared/types/intelligentAgents'

const { Title, Text } = Typography
const { TextArea } = Input

export interface AIConfigurationManagerProps {
  agentId?: string
  showAdvanced?: boolean
  showPresets?: boolean
  onConfigurationChange?: (config: any) => void
  className?: string
  testId?: string
}

interface ConfigurationPreset {
  id: string
  name: string
  description: string
  config: any
  category: 'performance' | 'cost' | 'accuracy' | 'custom'
}

/**
 * AIConfigurationManager - Comprehensive AI system configuration interface
 * 
 * Features:
 * - Agent-specific configuration management
 * - Real-time configuration validation
 * - Performance and cost impact preview
 * - Configuration presets and templates
 * - Advanced settings for power users
 * - Integration with backend configuration APIs
 */
export const AIConfigurationManager: React.FC<AIConfigurationManagerProps> = ({
  agentId,
  showAdvanced = false,
  showPresets = true,
  onConfigurationChange,
  className,
  testId = 'ai-configuration-manager'
}) => {
  const [form] = Form.useForm()
  const [activeTab, setActiveTab] = useState('general')
  const [hasChanges, setHasChanges] = useState(false)
  const [selectedPreset, setSelectedPreset] = useState<string | null>(null)

  // Real API data
  const { data: capabilities, isLoading: capabilitiesLoading, refetch: refetchCapabilities } = useGetAgentCapabilitiesQuery()
  const { data: transparencySettings, isLoading: transparencyLoading } = useGetTransparencySettingsQuery()
  const [updateAgentSettings, { isLoading: updateLoading }] = useUpdateAgentSettingsMutation()
  const [updateTransparencySettings] = useUpdateTransparencySettingsMutation()

  // Get current agent configuration
  const currentAgent = capabilities?.find(cap => cap.agentId === agentId) || capabilities?.[0]

  // Configuration presets
  const configurationPresets: ConfigurationPreset[] = [
    {
      id: 'high-performance',
      name: 'High Performance',
      description: 'Optimized for speed and throughput',
      category: 'performance',
      config: {
        maxConcurrentTasks: 10,
        timeout: 15000,
        retryAttempts: 2,
        priority: 8,
        temperature: 0.3,
        maxTokens: 2048
      }
    },
    {
      id: 'cost-optimized',
      name: 'Cost Optimized',
      description: 'Balanced performance with cost efficiency',
      category: 'cost',
      config: {
        maxConcurrentTasks: 5,
        timeout: 30000,
        retryAttempts: 3,
        priority: 5,
        temperature: 0.7,
        maxTokens: 1024
      }
    },
    {
      id: 'high-accuracy',
      name: 'High Accuracy',
      description: 'Maximum accuracy and reliability',
      category: 'accuracy',
      config: {
        maxConcurrentTasks: 3,
        timeout: 45000,
        retryAttempts: 5,
        priority: 9,
        temperature: 0.1,
        maxTokens: 4096
      }
    }
  ]

  // Initialize form with current configuration
  useEffect(() => {
    if (currentAgent) {
      form.setFieldsValue({
        ...currentAgent.configuration,
        enabled: currentAgent.status === 'active'
      })
    }
  }, [currentAgent, form])

  // Handle form changes
  const handleFormChange = () => {
    setHasChanges(true)
    const values = form.getFieldsValue()
    onConfigurationChange?.(values)
  }

  // Handle preset selection
  const handlePresetSelect = (presetId: string) => {
    const preset = configurationPresets.find(p => p.id === presetId)
    if (preset) {
      form.setFieldsValue(preset.config)
      setSelectedPreset(presetId)
      setHasChanges(true)
    }
  }

  // Handle configuration save
  const handleSave = async () => {
    try {
      const values = await form.validateFields()
      
      if (currentAgent) {
        await updateAgentSettings({
          agentId: currentAgent.agentId,
          settings: {
            enabled: values.enabled,
            maxConcurrentTasks: values.maxConcurrentTasks,
            timeout: values.timeout,
            retryAttempts: values.retryAttempts,
            priority: values.priority
          }
        }).unwrap()
        
        // Update transparency settings if changed
        if (values.transparencyEnabled !== undefined) {
          await updateTransparencySettings({
            enabled: values.transparencyEnabled,
            detailLevel: values.transparencyDetailLevel,
            includeConfidence: values.includeConfidence
          }).unwrap()
        }
        
        setHasChanges(false)
        refetchCapabilities()
      }
    } catch (error) {
      console.error('Configuration save failed:', error)
    }
  }

  // Handle reset
  const handleReset = () => {
    if (currentAgent) {
      form.setFieldsValue({
        ...currentAgent.configuration,
        enabled: currentAgent.status === 'active'
      })
      setHasChanges(false)
      setSelectedPreset(null)
    }
  }

  // Get preset category color
  const getPresetCategoryColor = (category: string) => {
    switch (category) {
      case 'performance': return '#1890ff'
      case 'cost': return '#52c41a'
      case 'accuracy': return '#722ed1'
      case 'custom': return '#fa8c16'
      default: return '#d9d9d9'
    }
  }

  // Render general configuration tab
  const renderGeneralTab = () => (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Basic Settings" size="small">
            <Form.Item
              name="enabled"
              label="Agent Enabled"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
            
            <Form.Item
              name="maxConcurrentTasks"
              label="Max Concurrent Tasks"
              rules={[{ required: true, type: 'number', min: 1, max: 20 }]}
            >
              <InputNumber 
                style={{ width: '100%' }}
                min={1}
                max={20}
              />
            </Form.Item>
            
            <Form.Item
              name="timeout"
              label="Timeout (ms)"
              rules={[{ required: true, type: 'number', min: 5000, max: 120000 }]}
            >
              <InputNumber 
                style={{ width: '100%' }}
                min={5000}
                max={120000}
                step={1000}
              />
            </Form.Item>
            
            <Form.Item
              name="retryAttempts"
              label="Retry Attempts"
              rules={[{ required: true, type: 'number', min: 0, max: 10 }]}
            >
              <InputNumber 
                style={{ width: '100%' }}
                min={0}
                max={10}
              />
            </Form.Item>
          </Card>
        </Col>
        
        <Col xs={24} lg={12}>
          <Card title="Performance Settings" size="small">
            <Form.Item
              name="priority"
              label="Priority Level"
              rules={[{ required: true, type: 'number', min: 1, max: 10 }]}
            >
              <Slider
                min={1}
                max={10}
                marks={{
                  1: 'Low',
                  5: 'Medium',
                  10: 'High'
                }}
              />
            </Form.Item>
            
            <Form.Item
              name="temperature"
              label="Temperature"
              rules={[{ type: 'number', min: 0, max: 2 }]}
            >
              <Slider
                min={0}
                max={2}
                step={0.1}
                marks={{
                  0: 'Deterministic',
                  1: 'Balanced',
                  2: 'Creative'
                }}
              />
            </Form.Item>
            
            <Form.Item
              name="maxTokens"
              label="Max Tokens"
              rules={[{ type: 'number', min: 100, max: 8192 }]}
            >
              <InputNumber 
                style={{ width: '100%' }}
                min={100}
                max={8192}
                step={256}
              />
            </Form.Item>
          </Card>
        </Col>
      </Row>
    </div>
  )

  // Render transparency configuration tab
  const renderTransparencyTab = () => (
    <div>
      <Alert
        message="AI Transparency Configuration"
        description="Configure how AI decision-making processes are tracked and displayed to users."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Transparency Settings" size="small">
            <Form.Item
              name="transparencyEnabled"
              label="Enable Transparency Tracking"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
            
            <Form.Item
              name="transparencyDetailLevel"
              label="Detail Level"
            >
              <Select>
                <Select.Option value="basic">Basic</Select.Option>
                <Select.Option value="detailed">Detailed</Select.Option>
                <Select.Option value="comprehensive">Comprehensive</Select.Option>
              </Select>
            </Form.Item>
            
            <Form.Item
              name="includeConfidence"
              label="Include Confidence Scores"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
            
            <Form.Item
              name="includeReasoning"
              label="Include AI Reasoning"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Card>
        </Col>
        
        <Col xs={24} lg={12}>
          <Card title="Privacy & Security" size="small">
            <Form.Item
              name="anonymizeData"
              label="Anonymize User Data"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
            
            <Form.Item
              name="retentionPeriod"
              label="Data Retention (days)"
            >
              <InputNumber 
                style={{ width: '100%' }}
                min={1}
                max={365}
              />
            </Form.Item>
            
            <Form.Item
              name="encryptTraces"
              label="Encrypt Trace Data"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Card>
        </Col>
      </Row>
    </div>
  )

  // Render advanced configuration tab
  const renderAdvancedTab = () => (
    <div>
      <Alert
        message="Advanced Configuration"
        description="These settings are for advanced users and may impact system performance."
        type="warning"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <Card title="Advanced Settings" size="small">
        <Form.Item
          name="customPromptTemplate"
          label="Custom Prompt Template"
        >
          <TextArea 
            rows={4}
            placeholder="Enter custom prompt template..."
          />
        </Form.Item>
        
        <Form.Item
          name="modelSpecificConfig"
          label="Model-Specific Configuration"
        >
          <TextArea 
            rows={6}
            placeholder="Enter JSON configuration..."
          />
        </Form.Item>
        
        <Form.Item
          name="debugMode"
          label="Debug Mode"
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>
        
        <Form.Item
          name="experimentalFeatures"
          label="Enable Experimental Features"
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>
      </Card>
    </div>
  )

  // Render configuration presets
  const renderPresets = () => (
    <Card title="Configuration Presets" size="small" style={{ marginBottom: 16 }}>
      <Row gutter={[8, 8]}>
        {configurationPresets.map(preset => (
          <Col key={preset.id} xs={24} sm={12} lg={8}>
            <Card
              size="small"
              hoverable
              style={{ 
                cursor: 'pointer',
                border: selectedPreset === preset.id ? '2px solid #1890ff' : '1px solid #f0f0f0'
              }}
              onClick={() => handlePresetSelect(preset.id)}
            >
              <Space direction="vertical" style={{ width: '100%' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text strong>{preset.name}</Text>
                  <Tag color={getPresetCategoryColor(preset.category)}>
                    {preset.category.toUpperCase()}
                  </Tag>
                </div>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {preset.description}
                </Text>
              </Space>
            </Card>
          </Col>
        ))}
      </Row>
    </Card>
  )

  const tabItems = [
    {
      key: 'general',
      label: (
        <Space>
          <SettingOutlined />
          <span>General</span>
        </Space>
      ),
      children: renderGeneralTab()
    },
    {
      key: 'transparency',
      label: (
        <Space>
          <SecurityScanOutlined />
          <span>Transparency</span>
        </Space>
      ),
      children: renderTransparencyTab()
    },
    ...(showAdvanced ? [{
      key: 'advanced',
      label: (
        <Space>
          <ExperimentOutlined />
          <span>Advanced</span>
        </Space>
      ),
      children: renderAdvancedTab()
    }] : [])
  ]

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 24 
      }}>
        <div>
          <Title level={4} style={{ margin: 0 }}>
            AI Configuration
          </Title>
          <Text type="secondary">
            {currentAgent ? `Configure ${currentAgent.name}` : 'Configure AI system settings'}
          </Text>
        </div>
        <Space>
          <Button 
            icon={<ReloadOutlined />}
            loading={capabilitiesLoading}
            onClick={handleReset}
            disabled={!hasChanges}
          >
            Reset
          </Button>
          <Button 
            type="primary"
            icon={<SaveOutlined />}
            loading={updateLoading}
            onClick={handleSave}
            disabled={!hasChanges}
          >
            Save Changes
          </Button>
        </Space>
      </div>

      {/* Configuration Presets */}
      {showPresets && renderPresets()}

      {/* Configuration Form */}
      <Form
        form={form}
        layout="vertical"
        onValuesChange={handleFormChange}
      >
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
        />
      </Form>

      {/* Changes Indicator */}
      {hasChanges && (
        <Alert
          message="Unsaved Changes"
          description="You have unsaved configuration changes. Click 'Save Changes' to apply them."
          type="warning"
          showIcon
          style={{ marginTop: 16 }}
        />
      )}
    </div>
  )
}

export default AIConfigurationManager
