import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Button, 
  Select, 
  DatePicker, 
  Form, 
  Checkbox,
  Table,
  Tag,
  Space,
  Alert,
  Progress,
  List,
  Avatar,
  Modal,
  Input,
  message,
  Tabs
} from 'antd'
import { 
  DownloadOutlined, 
  FileExcelOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  ScheduleOutlined,
  ShareAltOutlined,
  SettingOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  MailOutlined,
  CalendarOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'


const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { TextArea } = Input

interface ReportTemplate {
  id: string
  name: string
  description: string
  type: 'performance' | 'usage' | 'quality' | 'comprehensive'
  format: 'pdf' | 'excel' | 'csv' | 'json'
  frequency: 'daily' | 'weekly' | 'monthly' | 'quarterly'
  recipients: string[]
  lastGenerated: string
  status: 'active' | 'paused' | 'draft'
}

interface ExportJob {
  id: string
  reportName: string
  format: string
  status: 'queued' | 'processing' | 'completed' | 'failed'
  progress: number
  startTime: string
  estimatedCompletion?: string
  downloadUrl?: string
  fileSize?: string
}

export const ReportsExportDashboard: React.FC = () => {
  const [selectedReportType, setSelectedReportType] = useState('performance')
  const [isScheduleModalVisible, setIsScheduleModalVisible] = useState(false)
  const [isCustomReportModalVisible, setIsCustomReportModalVisible] = useState(false)
  const [form] = Form.useForm()

  // Mock data
  const [reportTemplates] = useState<ReportTemplate[]>([
    {
      id: 'rpt_1',
      name: 'Weekly Performance Summary',
      description: 'Comprehensive performance metrics for all templates',
      type: 'performance',
      format: 'pdf',
      frequency: 'weekly',
      recipients: ['team@company.com', 'manager@company.com'],
      lastGenerated: '2024-01-15T09:00:00Z',
      status: 'active'
    },
    {
      id: 'rpt_2',
      name: 'Monthly Usage Analytics',
      description: 'Detailed usage patterns and trends analysis',
      type: 'usage',
      format: 'excel',
      frequency: 'monthly',
      recipients: ['analytics@company.com'],
      lastGenerated: '2024-01-01T10:00:00Z',
      status: 'active'
    },
    {
      id: 'rpt_3',
      name: 'Quality Assessment Report',
      description: 'Template quality scores and improvement recommendations',
      type: 'quality',
      format: 'pdf',
      frequency: 'monthly',
      recipients: ['quality@company.com'],
      lastGenerated: '2024-01-01T11:00:00Z',
      status: 'paused'
    }
  ])

  const [exportJobs] = useState<ExportJob[]>([
    {
      id: 'job_1',
      reportName: 'Performance Dashboard Export',
      format: 'Excel',
      status: 'processing',
      progress: 67,
      startTime: '2024-01-15T14:30:00Z',
      estimatedCompletion: '2024-01-15T14:35:00Z'
    },
    {
      id: 'job_2',
      reportName: 'A/B Test Results',
      format: 'PDF',
      status: 'completed',
      progress: 100,
      startTime: '2024-01-15T14:00:00Z',
      downloadUrl: '/downloads/ab-test-results.pdf',
      fileSize: '2.3 MB'
    },
    {
      id: 'job_3',
      reportName: 'Template Inventory',
      format: 'CSV',
      status: 'failed',
      progress: 0,
      startTime: '2024-01-15T13:45:00Z'
    }
  ])

  const handleGenerateReport = async (reportType: string, format: string) => {
    try {
      message.success(`${format} report generation started`)
      // Simulate report generation
    } catch (error) {
      message.error('Failed to generate report')
    }
  }

  const handleScheduleReport = async (values: any) => {
    try {
      message.success('Report scheduled successfully')
      setIsScheduleModalVisible(false)
      form.resetFields()
    } catch (error) {
      message.error('Failed to schedule report')
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'green'
      case 'paused': return 'orange'
      case 'draft': return 'blue'
      case 'processing': return 'blue'
      case 'completed': return 'green'
      case 'failed': return 'red'
      case 'queued': return 'orange'
      default: return 'default'
    }
  }

  const getFormatIcon = (format: string) => {
    switch (format.toLowerCase()) {
      case 'pdf': return <FilePdfOutlined style={{ color: '#f5222d' }} />
      case 'excel': case 'xlsx': return <FileExcelOutlined style={{ color: '#52c41a' }} />
      case 'csv': return <FileTextOutlined style={{ color: '#1890ff' }} />
      default: return <FileTextOutlined />
    }
  }

  const reportTemplateColumns = [
    {
      title: 'Report Name',
      key: 'name',
      render: (record: ReportTemplate) => (
        <div>
          <Text strong>{record.name}</Text>
          <div style={{ marginTop: '4px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.description}
            </Text>
          </div>
        </div>
      )
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => (
        <Tag color="blue">{type.toUpperCase()}</Tag>
      )
    },
    {
      title: 'Format',
      dataIndex: 'format',
      key: 'format',
      render: (format: string) => (
        <Space>
          {getFormatIcon(format)}
          <Text>{format.toUpperCase()}</Text>
        </Space>
      )
    },
    {
      title: 'Frequency',
      dataIndex: 'frequency',
      key: 'frequency',
      render: (frequency: string) => (
        <Tag>{frequency.toUpperCase()}</Tag>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>
          {status.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Last Generated',
      dataIndex: 'lastGenerated',
      key: 'lastGenerated',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY HH:mm')
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: ReportTemplate) => (
        <Space>
          <Button size="small" icon={<DownloadOutlined />}>
            Generate Now
          </Button>
          <Button size="small" icon={<SettingOutlined />}>
            Configure
          </Button>
        </Space>
      )
    }
  ]

  const exportJobColumns = [
    {
      title: 'Report',
      dataIndex: 'reportName',
      key: 'reportName'
    },
    {
      title: 'Format',
      dataIndex: 'format',
      key: 'format',
      render: (format: string) => (
        <Space>
          {getFormatIcon(format)}
          <Text>{format}</Text>
        </Space>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>
          {status.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Progress',
      key: 'progress',
      render: (record: ExportJob) => (
        <div style={{ width: '100px' }}>
          <Progress 
            percent={record.progress} 
            size="small" 
            status={record.status === 'failed' ? 'exception' : 'active'}
          />
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: ExportJob) => (
        <Space>
          {record.status === 'completed' && record.downloadUrl && (
            <Button 
              size="small" 
              type="primary" 
              icon={<DownloadOutlined />}
              href={record.downloadUrl}
            >
              Download
            </Button>
          )}
          {record.status === 'processing' && (
            <Button size="small" icon={<LoadingOutlined />} disabled>
              Processing
            </Button>
          )}
          {record.status === 'failed' && (
            <Button size="small" icon={<DownloadOutlined />}>
              Retry
            </Button>
          )}
        </Space>
      )
    }
  ]

  const quickExportsTab = (
    <div>
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card hoverable onClick={() => handleGenerateReport('performance', 'pdf')}>
            <div style={{ textAlign: 'center' }}>
              <FilePdfOutlined style={{ fontSize: '32px', color: '#f5222d' }} />
              <Title level={5}>Performance Report</Title>
              <Text type="secondary">PDF Format</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card hoverable onClick={() => handleGenerateReport('usage', 'excel')}>
            <div style={{ textAlign: 'center' }}>
              <FileExcelOutlined style={{ fontSize: '32px', color: '#52c41a' }} />
              <Title level={5}>Usage Analytics</Title>
              <Text type="secondary">Excel Format</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card hoverable onClick={() => handleGenerateReport('abtests', 'pdf')}>
            <div style={{ textAlign: 'center' }}>
              <FilePdfOutlined style={{ fontSize: '32px', color: '#f5222d' }} />
              <Title level={5}>A/B Test Results</Title>
              <Text type="secondary">PDF Format</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card hoverable onClick={() => handleGenerateReport('templates', 'csv')}>
            <div style={{ textAlign: 'center' }}>
              <FileTextOutlined style={{ fontSize: '32px', color: '#1890ff' }} />
              <Title level={5}>Template Inventory</Title>
              <Text type="secondary">CSV Format</Text>
            </div>
          </Card>
        </Col>
      </Row>

      <Alert
        message="Quick Export"
        description="Click on any report type above to generate an instant export with current data."
        type="info"
        showIcon
        style={{ marginBottom: '24px' }}
      />

      <Card title="Export Jobs">
        <Table
          columns={exportJobColumns}
          dataSource={exportJobs}
          pagination={false}
          size="small"
        />
      </Card>
    </div>
  )

  const scheduledReportsTab = (
    <div>
      <div style={{ marginBottom: '24px', textAlign: 'right' }}>
        <Button 
          type="primary" 
          icon={<ScheduleOutlined />}
          onClick={() => setIsScheduleModalVisible(true)}
        >
          Schedule New Report
        </Button>
      </div>

      <Table
        columns={reportTemplateColumns}
        dataSource={reportTemplates}
        pagination={false}
      />
    </div>
  )

  const customReportsTab = (
    <div>
      <div style={{ marginBottom: '24px', textAlign: 'right' }}>
        <Button 
          type="primary" 
          icon={<SettingOutlined />}
          onClick={() => setIsCustomReportModalVisible(true)}
        >
          Create Custom Report
        </Button>
      </div>

      <Alert
        message="Custom Report Builder"
        description="Create tailored reports with specific metrics, filters, and formatting options."
        type="info"
        showIcon
      />
    </div>
  )

  const tabs = [
    {
      key: 'quick',
      label: (
        <Space>
          <DownloadOutlined />
          Quick Exports
        </Space>
      ),
      children: quickExportsTab
    },
    {
      key: 'scheduled',
      label: (
        <Space>
          <ScheduleOutlined />
          Scheduled Reports
        </Space>
      ),
      children: scheduledReportsTab
    },
    {
      key: 'custom',
      label: (
        <Space>
          <SettingOutlined />
          Custom Reports
        </Space>
      ),
      children: customReportsTab
    }
  ]

  return (
    <div>
        {/* Header */}
        <div style={{ marginBottom: '24px' }}>
          <Row gutter={16} align="middle">
            <Col span={16}>
              <Title level={2} style={{ margin: 0 }}>
                Reports & Export
              </Title>
              <Text type="secondary">
                Generate, schedule, and export comprehensive analytics reports
              </Text>
            </Col>
            <Col span={8} style={{ textAlign: 'right' }}>
              <Space>
                <Button icon={<ShareAltOutlined />}>
                  Share Dashboard
                </Button>
                <Button icon={<MailOutlined />}>
                  Email Report
                </Button>
              </Space>
            </Col>
          </Row>
        </div>

        {/* Main Content */}
        <Tabs items={tabs} />

        {/* Schedule Report Modal */}
        <Modal
          title="Schedule Report"
          open={isScheduleModalVisible}
          onCancel={() => setIsScheduleModalVisible(false)}
          footer={[
            <Button key="cancel" onClick={() => setIsScheduleModalVisible(false)}>
              Cancel
            </Button>,
            <Button key="schedule" type="primary" onClick={() => form.submit()}>
              Schedule Report
            </Button>
          ]}
        >
          <Form form={form} layout="vertical" onFinish={handleScheduleReport}>
            <Form.Item name="reportName" label="Report Name" rules={[{ required: true }]}>
              <Input placeholder="Enter report name" />
            </Form.Item>
            
            <Form.Item name="reportType" label="Report Type" rules={[{ required: true }]}>
              <Select placeholder="Select report type">
                <Select.Option value="performance">Performance Analytics</Select.Option>
                <Select.Option value="usage">Usage Analytics</Select.Option>
                <Select.Option value="quality">Quality Assessment</Select.Option>
                <Select.Option value="comprehensive">Comprehensive Report</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item name="format" label="Format" rules={[{ required: true }]}>
              <Select placeholder="Select format">
                <Select.Option value="pdf">PDF</Select.Option>
                <Select.Option value="excel">Excel</Select.Option>
                <Select.Option value="csv">CSV</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item name="frequency" label="Frequency" rules={[{ required: true }]}>
              <Select placeholder="Select frequency">
                <Select.Option value="daily">Daily</Select.Option>
                <Select.Option value="weekly">Weekly</Select.Option>
                <Select.Option value="monthly">Monthly</Select.Option>
                <Select.Option value="quarterly">Quarterly</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item name="recipients" label="Email Recipients">
              <Select mode="tags" placeholder="Enter email addresses">
                <Select.Option value="team@company.com">team@company.com</Select.Option>
                <Select.Option value="manager@company.com">manager@company.com</Select.Option>
              </Select>
            </Form.Item>
          </Form>
        </Modal>

        {/* Custom Report Modal */}
        <Modal
          title="Create Custom Report"
          open={isCustomReportModalVisible}
          onCancel={() => setIsCustomReportModalVisible(false)}
          width={800}
          footer={[
            <Button key="cancel" onClick={() => setIsCustomReportModalVisible(false)}>
              Cancel
            </Button>,
            <Button key="create" type="primary">
              Create Report
            </Button>
          ]}
        >
          <Alert
            message="Custom Report Builder"
            description="This feature allows you to create highly customized reports with specific metrics, filters, and visualizations."
            type="info"
            showIcon
            style={{ marginBottom: '16px' }}
          />
          <Text>Custom report builder interface would be implemented here with drag-and-drop components, metric selection, and advanced filtering options.</Text>
        </Modal>
      </div>
  )
}

export default ReportsExportDashboard
