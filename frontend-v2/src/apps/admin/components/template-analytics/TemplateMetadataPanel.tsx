import React, { useState, useEffect } from 'react'
import { 
  Drawer, 
  Form, 
  Input, 
  Select, 
  Button, 
  Space, 
  Typography, 
  Card,
  Row,
  Col,
  DatePicker,
  Switch,
  Divider,
  Alert,
  Tabs,
  Tag,
  Rate,
  Slider,
  message
} from 'antd'
import { 
  SaveOutlined, 
  UserOutlined,
  TeamOutlined,
  FileTextOutlined,
  SafetyOutlined,
  StarOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { useUpdateTemplateMutation } from '@shared/store/api/templateAnalyticsApi'

const { Title, Text, Paragraph } = Typography
const { TextArea } = Input

interface TemplateMetadataPanelProps {
  visible: boolean
  onClose: () => void
  template?: any
  onSave: () => void
}

export const TemplateMetadataPanel: React.FC<TemplateMetadataPanelProps> = ({
  visible,
  onClose,
  template,
  onSave
}) => {
  const [form] = Form.useForm()
  const [activeTab, setActiveTab] = useState('business')
  const [updateTemplate] = useUpdateTemplateMutation()

  useEffect(() => {
    if (template?.businessMetadata) {
      const metadata = template.businessMetadata
      form.setFieldsValue({
        // Business Information
        businessPurpose: metadata.businessPurpose,
        businessFriendlyName: metadata.businessFriendlyName,
        businessContext: metadata.businessContext,
        useCases: metadata.useCases,
        targetAudience: metadata.targetAudience,
        businessRules: metadata.businessRules,
        
        // Governance
        dataGovernanceLevel: metadata.dataGovernanceLevel,
        lastBusinessReview: metadata.lastBusinessReview ? dayjs(metadata.lastBusinessReview) : null,
        businessOwner: metadata.businessOwner,
        technicalOwner: metadata.technicalOwner,
        approvalStatus: metadata.approvalStatus,
        
        // Quality & Usage
        qualityRating: template.qualityScore / 20, // Convert 0-100 to 0-5 stars
        usageFrequency: metadata.usageFrequency,
        criticalityLevel: metadata.criticalityLevel,
        maintenanceSchedule: metadata.maintenanceSchedule
      })
    } else {
      form.resetFields()
    }
  }, [template, form])

  const handleSave = async () => {
    try {
      const values = await form.validateFields()
      
      const businessMetadata = {
        businessPurpose: values.businessPurpose,
        businessFriendlyName: values.businessFriendlyName,
        businessContext: values.businessContext,
        useCases: values.useCases || [],
        targetAudience: values.targetAudience || [],
        businessRules: values.businessRules || [],
        dataGovernanceLevel: values.dataGovernanceLevel,
        lastBusinessReview: values.lastBusinessReview?.toISOString(),
        businessOwner: values.businessOwner,
        technicalOwner: values.technicalOwner,
        approvalStatus: values.approvalStatus
      }

      await updateTemplate({
        templateKey: template.templateKey,
        updates: { businessMetadata }
      }).unwrap()

      onSave()
    } catch (error) {
      message.error('Failed to update template metadata')
    }
  }

  const businessTab = (
    <div>
      <Card title="Business Information" size="small" style={{ marginBottom: '16px' }}>
        <Form.Item
          name="businessFriendlyName"
          label="Business Friendly Name"
          rules={[{ required: true, message: 'Please enter business friendly name' }]}
        >
          <Input placeholder="Human-readable name for business users" />
        </Form.Item>

        <Form.Item
          name="businessPurpose"
          label="Business Purpose"
          rules={[{ required: true, message: 'Please describe the business purpose' }]}
        >
          <TextArea 
            rows={3} 
            placeholder="Explain what business problem this template solves..."
          />
        </Form.Item>

        <Form.Item
          name="businessContext"
          label="Business Context"
        >
          <TextArea 
            rows={3} 
            placeholder="Provide additional context about when and how to use this template..."
          />
        </Form.Item>

        <Form.Item
          name="useCases"
          label="Use Cases"
        >
          <Select
            mode="tags"
            placeholder="Add specific use cases"
            style={{ width: '100%' }}
          >
            <Select.Option value="reporting">Reporting</Select.Option>
            <Select.Option value="analysis">Data Analysis</Select.Option>
            <Select.Option value="monitoring">Monitoring</Select.Option>
            <Select.Option value="compliance">Compliance</Select.Option>
          </Select>
        </Form.Item>

        <Form.Item
          name="targetAudience"
          label="Target Audience"
        >
          <Select
            mode="tags"
            placeholder="Who should use this template?"
            style={{ width: '100%' }}
          >
            <Select.Option value="business_analysts">Business Analysts</Select.Option>
            <Select.Option value="data_scientists">Data Scientists</Select.Option>
            <Select.Option value="executives">Executives</Select.Option>
            <Select.Option value="operations">Operations Team</Select.Option>
            <Select.Option value="finance">Finance Team</Select.Option>
          </Select>
        </Form.Item>

        <Form.Item
          name="businessRules"
          label="Business Rules"
        >
          <Select
            mode="tags"
            placeholder="Add business rules and constraints"
            style={{ width: '100%' }}
          >
            <Select.Option value="data_privacy">Data Privacy Compliant</Select.Option>
            <Select.Option value="audit_trail">Audit Trail Required</Select.Option>
            <Select.Option value="approval_required">Approval Required</Select.Option>
            <Select.Option value="restricted_access">Restricted Access</Select.Option>
          </Select>
        </Form.Item>
      </Card>
    </div>
  )

  const governanceTab = (
    <div>
      <Card title="Data Governance" size="small" style={{ marginBottom: '16px' }}>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="dataGovernanceLevel"
              label="Data Governance Level"
              rules={[{ required: true, message: 'Please select governance level' }]}
            >
              <Select placeholder="Select governance level">
                <Select.Option value="public">
                  <Space>
                    <Tag color="green">Public</Tag>
                    <span>No restrictions</span>
                  </Space>
                </Select.Option>
                <Select.Option value="internal">
                  <Space>
                    <Tag color="blue">Internal</Tag>
                    <span>Company internal use</span>
                  </Space>
                </Select.Option>
                <Select.Option value="confidential">
                  <Space>
                    <Tag color="orange">Confidential</Tag>
                    <span>Restricted access</span>
                  </Space>
                </Select.Option>
                <Select.Option value="restricted">
                  <Space>
                    <Tag color="red">Restricted</Tag>
                    <span>Highly sensitive</span>
                  </Space>
                </Select.Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="approvalStatus"
              label="Approval Status"
            >
              <Select placeholder="Select approval status">
                <Select.Option value="draft">
                  <Space>
                    <Tag color="default">Draft</Tag>
                    <span>Work in progress</span>
                  </Space>
                </Select.Option>
                <Select.Option value="pending_review">
                  <Space>
                    <Tag color="processing">Pending Review</Tag>
                    <span>Awaiting approval</span>
                  </Space>
                </Select.Option>
                <Select.Option value="approved">
                  <Space>
                    <Tag color="success">Approved</Tag>
                    <span>Ready for use</span>
                  </Space>
                </Select.Option>
                <Select.Option value="deprecated">
                  <Space>
                    <Tag color="error">Deprecated</Tag>
                    <span>No longer recommended</span>
                  </Space>
                </Select.Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="businessOwner"
              label="Business Owner"
              rules={[{ required: true, message: 'Please specify business owner' }]}
            >
              <Input 
                placeholder="Business owner name or team"
                prefix={<UserOutlined />}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="technicalOwner"
              label="Technical Owner"
            >
              <Input 
                placeholder="Technical owner name or team"
                prefix={<TeamOutlined />}
              />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          name="lastBusinessReview"
          label="Last Business Review"
        >
          <DatePicker 
            style={{ width: '100%' }}
            placeholder="Select last review date"
          />
        </Form.Item>

        <Alert
          message="Governance Guidelines"
          description={
            <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
              <li>Templates with 'Confidential' or 'Restricted' levels require approval</li>
              <li>Business reviews should be conducted quarterly for critical templates</li>
              <li>All templates must have a designated business owner</li>
              <li>Deprecated templates should be phased out within 90 days</li>
            </ul>
          }
          type="info"
          showIcon
          style={{ marginTop: '16px' }}
        />
      </Card>
    </div>
  )

  const qualityTab = (
    <div>
      <Card title="Quality & Usage Metrics" size="small" style={{ marginBottom: '16px' }}>
        <Form.Item
          name="qualityRating"
          label="Quality Rating"
        >
          <Rate 
            allowHalf 
            character={<StarOutlined />}
            style={{ fontSize: '24px' }}
          />
        </Form.Item>

        <Form.Item
          name="usageFrequency"
          label="Expected Usage Frequency"
        >
          <Select placeholder="How often will this template be used?">
            <Select.Option value="daily">Daily</Select.Option>
            <Select.Option value="weekly">Weekly</Select.Option>
            <Select.Option value="monthly">Monthly</Select.Option>
            <Select.Option value="quarterly">Quarterly</Select.Option>
            <Select.Option value="ad_hoc">Ad Hoc</Select.Option>
          </Select>
        </Form.Item>

        <Form.Item
          name="criticalityLevel"
          label="Business Criticality"
        >
          <Slider
            marks={{
              1: 'Low',
              2: 'Medium',
              3: 'High',
              4: 'Critical',
              5: 'Mission Critical'
            }}
            min={1}
            max={5}
            step={1}
            included={false}
            defaultValue={3}
          />
        </Form.Item>

        <Form.Item
          name="maintenanceSchedule"
          label="Maintenance Schedule"
        >
          <Select placeholder="How often should this template be reviewed?">
            <Select.Option value="monthly">Monthly</Select.Option>
            <Select.Option value="quarterly">Quarterly</Select.Option>
            <Select.Option value="semi_annually">Semi-Annually</Select.Option>
            <Select.Option value="annually">Annually</Select.Option>
            <Select.Option value="as_needed">As Needed</Select.Option>
          </Select>
        </Form.Item>

        <Divider />

        <Title level={5}>Current Performance Metrics</Title>
        <Row gutter={16}>
          <Col span={8}>
            <Card size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                  {template?.performanceMetrics?.successRate ? 
                    (template.performanceMetrics.successRate * 100).toFixed(1) + '%' : 'N/A'}
                </div>
                <div style={{ fontSize: '12px', color: '#666' }}>Success Rate</div>
              </div>
            </Card>
          </Col>
          <Col span={8}>
            <Card size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                  {template?.performanceMetrics?.totalUsages || 0}
                </div>
                <div style={{ fontSize: '12px', color: '#666' }}>Total Uses</div>
              </div>
            </Card>
          </Col>
          <Col span={8}>
            <Card size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#722ed1' }}>
                  {template?.qualityScore || 0}
                </div>
                <div style={{ fontSize: '12px', color: '#666' }}>Quality Score</div>
              </div>
            </Card>
          </Col>
        </Row>
      </Card>
    </div>
  )

  const tabs = [
    {
      key: 'business',
      label: (
        <Space>
          <FileTextOutlined />
          Business Info
        </Space>
      ),
      children: businessTab
    },
    {
      key: 'governance',
      label: (
        <Space>
          <SafetyOutlined />
          Governance
        </Space>
      ),
      children: governanceTab
    },
    {
      key: 'quality',
      label: (
        <Space>
          <StarOutlined />
          Quality & Usage
        </Space>
      ),
      children: qualityTab
    }
  ]

  return (
    <Drawer
      title={
        <Space>
          <FileTextOutlined />
          Template Metadata: {template?.templateName}
        </Space>
      }
      width={800}
      open={visible}
      onClose={onClose}
      extra={
        <Button type="primary" onClick={handleSave} icon={<SaveOutlined />}>
          Save Metadata
        </Button>
      }
    >
      <Form form={form} layout="vertical">
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabs}
        />
      </Form>
    </Drawer>
  )
}

export default TemplateMetadataPanel
