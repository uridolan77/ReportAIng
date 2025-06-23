import React, { useState } from 'react'
import { 
  Tabs, 
  Card, 
  Row, 
  Col, 
  Button, 
  Space, 
  Typography, 
  Badge,
  Alert,
  Tooltip,
  Select
} from 'antd'
import { 
  SafetyOutlined, 
  RocketOutlined, 
  StarOutlined,
  BulbOutlined,
  ExperimentOutlined,
  BarChartOutlined,
  SettingOutlined,
  ReloadOutlined,
  PlusOutlined,
  ImportOutlined,
  ExportOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'

// Import existing components
import TemplateManagementHub from './TemplateManagementHub'
import { TemplateFeatures } from './TemplateFeatures'

// Import hooks
import { 
  useGetTemplateManagementDashboardQuery
} from '@shared/store/api/templateAnalyticsApi'

const { Title, Text } = Typography

export const TemplateManagementComplete: React.FC = () => {
  const [activeTab, setActiveTab] = useState('management')
  const [selectedTemplate, setSelectedTemplate] = useState<string>('')

  // API hooks
  const { 
    data: managementData, 
    isLoading: isLoadingManagement,
    refetch: refetchManagement 
  } = useGetTemplateManagementDashboardQuery()

  // Calculate stats
  const totalTemplates = managementData?.templates?.length || 0
  const activeTemplates = managementData?.templates?.filter(t => t.isActive)?.length || 0
  const templatesNeedingReview = managementData?.templates?.filter(t => 
    t.metrics && t.metrics.successRate < 0.8
  )?.length || 0

  const handleTemplateSelect = (templateKey: string) => {
    setSelectedTemplate(templateKey)
    setActiveTab('features') // Switch to features tab when template is selected
  }

  const tabItems = [
    {
      key: 'management',
      label: (
        <Space>
          <SafetyOutlined />
          Template Management
        </Space>
      ),
      children: (
        <div>
          {/* Quick Stats Row */}
          <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                    {totalTemplates}
                  </div>
                  <div style={{ color: '#666' }}>Total Templates</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                    {activeTemplates}
                  </div>
                  <div style={{ color: '#666' }}>Active Templates</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#faad14' }}>
                    {templatesNeedingReview}
                  </div>
                  <div style={{ color: '#666' }}>Need Review</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#722ed1' }}>
                    {managementData?.totalIntentTypes || 0}
                  </div>
                  <div style={{ color: '#666' }}>Intent Types</div>
                </div>
              </Card>
            </Col>
          </Row>

          {/* Template Management Hub */}
          <TemplateManagementHub onTemplateSelect={handleTemplateSelect} />
        </div>
      )
    },
    {
      key: 'features',
      label: (
        <Space>
          <RocketOutlined />
          AI Features
          {selectedTemplate && (
            <Badge status="processing" />
          )}
        </Space>
      ),
      children: (
        <div>
          {selectedTemplate ? (
            <div>
              {/* Selected Template Info */}
              <Card style={{ marginBottom: '16px' }}>
                <Row gutter={16} align="middle">
                  <Col span={16}>
                    <Space>
                      <Text strong>Selected Template:</Text>
                      <Badge status="success" text={selectedTemplate} />
                    </Space>
                  </Col>
                  <Col span={8} style={{ textAlign: 'right' }}>
                    <Button 
                      onClick={() => setSelectedTemplate('')}
                      size="small"
                    >
                      Clear Selection
                    </Button>
                  </Col>
                </Row>
              </Card>

              {/* Template Features Component with selected template */}
              <TemplateFeatures selectedTemplateKey={selectedTemplate} />
            </div>
          ) : (
            <Card>
              <div style={{ textAlign: 'center', padding: '60px 0' }}>
                <RocketOutlined style={{ fontSize: '64px', color: '#d9d9d9', marginBottom: '24px' }} />
                <Title level={3}>AI-Powered Template Features</Title>
                <Text type="secondary" style={{ fontSize: '16px', display: 'block', marginBottom: '24px' }}>
                  Select a template from the Management tab to access AI features including:
                </Text>
                
                <Row gutter={[16, 16]} style={{ marginTop: '32px', maxWidth: '600px', margin: '32px auto 0' }}>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <StarOutlined style={{ fontSize: '24px', color: '#faad14', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>Quality Analysis</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        AI-powered content scoring
                      </Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <BulbOutlined style={{ fontSize: '24px', color: '#52c41a', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>Optimization</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Strategic improvements
                      </Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <ExperimentOutlined style={{ fontSize: '24px', color: '#722ed1', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>A/B Variants</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Automated variant generation
                      </Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <BarChartOutlined style={{ fontSize: '24px', color: '#1890ff', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>ML Predictions</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Performance forecasting
                      </Text>
                    </Card>
                  </Col>
                </Row>

                <div style={{ marginTop: '32px' }}>
                  <Space>
                    <Button
                      type="primary"
                      size="large"
                      onClick={() => setActiveTab('management')}
                    >
                      Go to Template Management
                    </Button>
                    <Button
                      size="large"
                      onClick={() => {
                        // Set a demo template and switch to features
                        if (managementData?.templates && managementData.templates.length > 0) {
                          setSelectedTemplate(managementData.templates[0].templateKey)
                          setActiveTab('features')
                        }
                      }}
                    >
                      Try Demo Features
                    </Button>
                  </Space>
                </div>
              </div>
            </Card>
          )}
        </div>
      )
    },
    {
      key: 'operations',
      label: (
        <Space>
          <SettingOutlined />
          Operations
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="Template Operations"
            description="Bulk operations, import/export, and administrative functions for template management."
            type="info"
            showIcon
            style={{ marginBottom: '24px' }}
          />
          
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={8}>
              <Card title="Bulk Operations" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<PlusOutlined />}>
                    Create Multiple Templates
                  </Button>
                  <Button block icon={<StarOutlined />}>
                    Bulk Quality Analysis
                  </Button>
                  <Button block icon={<RocketOutlined />}>
                    Batch Optimization
                  </Button>
                </Space>
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card title="Import/Export" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<ImportOutlined />}>
                    Import Templates
                  </Button>
                  <Button block icon={<ExportOutlined />}>
                    Export Templates
                  </Button>
                  <Button block icon={<ExportOutlined />}>
                    Export Analytics
                  </Button>
                </Space>
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card title="Administration" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<SettingOutlined />}>
                    Template Policies
                  </Button>
                  <Button block icon={<SafetyOutlined />}>
                    Access Control
                  </Button>
                  <Button block icon={<BarChartOutlined />}>
                    Usage Reports
                  </Button>
                </Space>
              </Card>
            </Col>
          </Row>

          <Card title="Recent Operations" style={{ marginTop: '16px' }}>
            <div style={{ textAlign: 'center', padding: '40px 0' }}>
              <Text type="secondary">No recent operations to display</Text>
            </div>
          </Card>
        </div>
      )
    }
  ]

  return (
    <PageLayout
      title="Template Management"
      subtitle="Comprehensive template management with AI-powered features and operations"
      extra={
        <Space>
          {templatesNeedingReview > 0 && (
            <Alert
              message={`${templatesNeedingReview} template${templatesNeedingReview !== 1 ? 's' : ''} need review`}
              type="warning"
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
          <Tooltip title="Refresh data">
            <Button 
              icon={<ReloadOutlined />} 
              onClick={() => refetchManagement()}
              loading={isLoadingManagement}
            />
          </Tooltip>
          <Button icon={<PlusOutlined />} type="primary">
            New Template
          </Button>
        </Space>
      }
    >
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
        style={{ marginTop: '16px' }}
      />
    </PageLayout>
  )
}

export default TemplateManagementComplete
