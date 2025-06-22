import React, { useState } from 'react'
import { 
  Modal, 
  Steps, 
  Form, 
  Input, 
  Select, 
  InputNumber, 
  Button, 
  Card, 
  Typography, 
  Space,
  Alert,
  Divider,
  Row,
  Col
} from 'antd'
import { 
  ExperimentOutlined, 
  SettingOutlined, 
  CheckCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'

const { Step } = Steps
const { Title, Text, Paragraph } = Typography
const { TextArea } = Input

interface TestCreationWizardProps {
  visible: boolean
  onCancel: () => void
  onSubmit: (values: any) => void
}

export const TestCreationWizard: React.FC<TestCreationWizardProps> = ({
  visible,
  onCancel,
  onSubmit
}) => {
  const [currentStep, setCurrentStep] = useState(0)
  const [form] = Form.useForm()
  const [formData, setFormData] = useState<any>({})

  const handleNext = async () => {
    try {
      const values = await form.validateFields()
      setFormData({ ...formData, ...values })
      setCurrentStep(currentStep + 1)
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const handlePrevious = () => {
    setCurrentStep(currentStep - 1)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      const finalData = { ...formData, ...values }
      onSubmit(finalData)
      handleReset()
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const handleReset = () => {
    setCurrentStep(0)
    setFormData({})
    form.resetFields()
  }

  const handleCancel = () => {
    handleReset()
    onCancel()
  }

  const steps = [
    {
      title: 'Test Setup',
      icon: <ExperimentOutlined />,
      content: (
        <div>
          <Title level={4}>Basic Test Information</Title>
          <Form form={form} layout="vertical">
            <Form.Item
              name="testName"
              label="Test Name"
              rules={[{ required: true, message: 'Please enter test name' }]}
            >
              <Input 
                placeholder="Enter descriptive test name (e.g., 'SQL Generation Prompt Optimization')" 
                size="large"
              />
            </Form.Item>

            <Form.Item
              name="originalTemplate"
              label="Original Template"
              rules={[{ required: true, message: 'Please select original template' }]}
            >
              <Select 
                placeholder="Select template to test" 
                size="large"
                showSearch
              >
                <Select.Option value="sql_generation">SQL Generation Template</Select.Option>
                <Select.Option value="insight_generation">Insight Generation Template</Select.Option>
                <Select.Option value="explanation">Explanation Template</Select.Option>
                <Select.Option value="data_analysis">Data Analysis Template</Select.Option>
                <Select.Option value="report_generation">Report Generation Template</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item
              name="testDescription"
              label="Test Description"
            >
              <TextArea
                rows={3}
                placeholder="Describe what you're testing and why..."
              />
            </Form.Item>
          </Form>
        </div>
      )
    },
    {
      title: 'Variant Design',
      icon: <SettingOutlined />,
      content: (
        <div>
          <Title level={4}>Create Your Variant</Title>
          <Alert
            message="Design Your Variant Template"
            description="Create a modified version of the original template. Focus on specific changes you want to test (e.g., different prompting style, additional context, modified instructions)."
            type="info"
            showIcon
            style={{ marginBottom: '16px' }}
          />
          
          <Form form={form} layout="vertical">
            <Form.Item
              name="variantContent"
              label="Variant Template Content"
              rules={[{ required: true, message: 'Please enter variant content' }]}
            >
              <TextArea
                rows={8}
                placeholder="Enter the variant template content..."
                style={{ fontFamily: 'monospace' }}
              />
            </Form.Item>

            <Form.Item
              name="variantDescription"
              label="What's Different?"
            >
              <TextArea
                rows={2}
                placeholder="Describe the key changes in this variant..."
              />
            </Form.Item>
          </Form>
        </div>
      )
    },
    {
      title: 'Configuration',
      icon: <SettingOutlined />,
      content: (
        <div>
          <Title level={4}>Test Configuration</Title>
          
          <Form form={form} layout="vertical">
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="trafficSplit"
                  label="Traffic Split (%)"
                  initialValue={50}
                  rules={[{ required: true, message: 'Please enter traffic split' }]}
                >
                  <Select size="large">
                    <Select.Option value={10}>10% Variant / 90% Original</Select.Option>
                    <Select.Option value={25}>25% Variant / 75% Original</Select.Option>
                    <Select.Option value={50}>50% Variant / 50% Original</Select.Option>
                    <Select.Option value={75}>75% Variant / 25% Original</Select.Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="minimumSampleSize"
                  label="Minimum Sample Size"
                  initialValue={1000}
                >
                  <InputNumber
                    min={100}
                    max={10000}
                    step={100}
                    style={{ width: '100%' }}
                    size="large"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="confidenceLevel"
                  label="Confidence Level (%)"
                  initialValue={95}
                >
                  <Select size="large">
                    <Select.Option value={90}>90% Confidence</Select.Option>
                    <Select.Option value={95}>95% Confidence</Select.Option>
                    <Select.Option value={99}>99% Confidence</Select.Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="testDuration"
                  label="Test Duration (Days)"
                  initialValue={30}
                >
                  <InputNumber
                    min={7}
                    max={90}
                    style={{ width: '100%' }}
                    size="large"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Alert
              message="Statistical Power"
              description="With these settings, you'll need approximately 1000 samples to detect a 5% improvement with 95% confidence."
              type="info"
              showIcon
            />
          </Form>
        </div>
      )
    },
    {
      title: 'Review',
      icon: <CheckCircleOutlined />,
      content: (
        <div>
          <Title level={4}>Review Your Test</Title>
          
          <Card style={{ marginBottom: '16px' }}>
            <Title level={5}>Test Details</Title>
            <Row gutter={16}>
              <Col span={12}>
                <Text strong>Test Name:</Text>
                <div>{formData.testName}</div>
              </Col>
              <Col span={12}>
                <Text strong>Original Template:</Text>
                <div>{formData.originalTemplate}</div>
              </Col>
            </Row>
            {formData.testDescription && (
              <div style={{ marginTop: '8px' }}>
                <Text strong>Description:</Text>
                <div>{formData.testDescription}</div>
              </div>
            )}
          </Card>

          <Card style={{ marginBottom: '16px' }}>
            <Title level={5}>Configuration</Title>
            <Row gutter={16}>
              <Col span={8}>
                <Text strong>Traffic Split:</Text>
                <div>{formData.trafficSplit}% Variant</div>
              </Col>
              <Col span={8}>
                <Text strong>Sample Size:</Text>
                <div>{formData.minimumSampleSize || 1000}</div>
              </Col>
              <Col span={8}>
                <Text strong>Confidence:</Text>
                <div>{formData.confidenceLevel || 95}%</div>
              </Col>
            </Row>
          </Card>

          <Alert
            message="Ready to Launch"
            description="Your A/B test is configured and ready to start. The test will begin immediately after creation and run for the specified duration."
            type="success"
            showIcon
          />
        </div>
      )
    }
  ]

  const getModalFooter = () => {
    if (currentStep === 0) {
      return [
        <Button key="cancel" onClick={handleCancel}>
          Cancel
        </Button>,
        <Button key="next" type="primary" onClick={handleNext}>
          Next
        </Button>
      ]
    } else if (currentStep === steps.length - 1) {
      return [
        <Button key="back" onClick={handlePrevious}>
          Previous
        </Button>,
        <Button key="cancel" onClick={handleCancel}>
          Cancel
        </Button>,
        <Button key="submit" type="primary" onClick={handleSubmit}>
          Create Test
        </Button>
      ]
    } else {
      return [
        <Button key="back" onClick={handlePrevious}>
          Previous
        </Button>,
        <Button key="cancel" onClick={handleCancel}>
          Cancel
        </Button>,
        <Button key="next" type="primary" onClick={handleNext}>
          Next
        </Button>
      ]
    }
  }

  return (
    <Modal
      title="Create A/B Test"
      open={visible}
      onCancel={handleCancel}
      footer={getModalFooter()}
      width={800}
      destroyOnClose
    >
      <Steps current={currentStep} style={{ marginBottom: '24px' }}>
        {steps.map(step => (
          <Step key={step.title} title={step.title} icon={step.icon} />
        ))}
      </Steps>
      
      <div style={{ minHeight: '400px' }}>
        {steps[currentStep].content}
      </div>
    </Modal>
  )
}

export default TestCreationWizard
