import React, { useState } from 'react'
import {
  Modal,
  Form,
  Select,
  Checkbox,
  Button,
  Space,
  Typography,
  Card,
  Row,
  Col,
  Progress,
  Alert,
  Divider,
  Radio,
  DatePicker,
} from 'antd'
import {
  DownloadOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  CloudDownloadOutlined,
  SettingOutlined,
} from '@ant-design/icons'
import { useEnhancedBusinessMetadata } from '@shared/hooks/useEnhancedBusinessMetadata'

const { Title, Text } = Typography
const { Option } = Select
const { RangePicker } = DatePicker

interface ExportOptions {
  format: 'excel' | 'csv' | 'pdf' | 'json'
  includeColumns: boolean
  includeGlossary: boolean
  includeValidation: boolean
  includeStatistics: boolean
  schemas: string[]
  domains: string[]
  dateRange?: [string, string]
}

interface MetadataExportManagerProps {
  visible: boolean
  onClose: () => void
  selectedTableIds?: number[]
}

export const MetadataExportManager: React.FC<MetadataExportManagerProps> = ({
  visible,
  onClose,
  selectedTableIds = [],
}) => {
  const [form] = Form.useForm()
  const [exporting, setExporting] = useState(false)
  const [exportProgress, setExportProgress] = useState(0)
  const [exportComplete, setExportComplete] = useState(false)

  const { statistics } = useEnhancedBusinessMetadata()

  const handleExport = async () => {
    try {
      const values = await form.validateFields()
      setExporting(true)
      setExportProgress(0)

      // Simulate export progress
      const progressInterval = setInterval(() => {
        setExportProgress(prev => {
          if (prev >= 100) {
            clearInterval(progressInterval)
            setExporting(false)
            setExportComplete(true)
            return 100
          }
          return prev + 10
        })
      }, 200)

      // In real implementation, this would call the export API
      console.log('Exporting with options:', values)
      
    } catch (error) {
      console.error('Export validation failed:', error)
      setExporting(false)
    }
  }

  const handleReset = () => {
    form.resetFields()
    setExportProgress(0)
    setExportComplete(false)
  }

  const handleClose = () => {
    handleReset()
    onClose()
  }

  const getFormatIcon = (format: string) => {
    switch (format) {
      case 'excel':
        return <FileExcelOutlined style={{ color: '#52c41a' }} />
      case 'csv':
        return <FileTextOutlined style={{ color: '#1890ff' }} />
      case 'pdf':
        return <FilePdfOutlined style={{ color: '#ff4d4f' }} />
      case 'json':
        return <FileTextOutlined style={{ color: '#722ed1' }} />
      default:
        return <DownloadOutlined />
    }
  }

  const formatOptions = [
    {
      value: 'excel',
      label: 'Excel (.xlsx)',
      description: 'Structured spreadsheet with multiple sheets',
      icon: <FileExcelOutlined style={{ color: '#52c41a' }} />,
    },
    {
      value: 'csv',
      label: 'CSV (.csv)',
      description: 'Comma-separated values for data analysis',
      icon: <FileTextOutlined style={{ color: '#1890ff' }} />,
    },
    {
      value: 'pdf',
      label: 'PDF (.pdf)',
      description: 'Formatted report for documentation',
      icon: <FilePdfOutlined style={{ color: '#ff4d4f' }} />,
    },
    {
      value: 'json',
      label: 'JSON (.json)',
      description: 'Machine-readable structured data',
      icon: <FileTextOutlined style={{ color: '#722ed1' }} />,
    },
  ]

  const availableSchemas = ['dbo', 'sales', 'finance', 'hr', 'operations']
  const availableDomains = ['Sales', 'Finance', 'HR', 'Operations', 'Marketing', 'Reference']

  return (
    <Modal
      title={
        <Space>
          <DownloadOutlined />
          <span>Export Metadata</span>
          {selectedTableIds.length > 0 && (
            <Text type="secondary">({selectedTableIds.length} tables selected)</Text>
          )}
        </Space>
      }
      open={visible}
      onCancel={handleClose}
      width={800}
      footer={[
        <Button key="reset" onClick={handleReset} disabled={exporting}>
          Reset
        </Button>,
        <Button key="cancel" onClick={handleClose} disabled={exporting}>
          Cancel
        </Button>,
        <Button
          key="export"
          type="primary"
          icon={<DownloadOutlined />}
          onClick={handleExport}
          loading={exporting}
          disabled={exportComplete}
        >
          {exportComplete ? 'Export Complete' : 'Export'}
        </Button>,
      ]}
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          format: 'excel',
          includeColumns: true,
          includeGlossary: false,
          includeValidation: false,
          includeStatistics: true,
          schemas: [],
          domains: [],
        }}
      >
        {/* Export Progress */}
        {(exporting || exportComplete) && (
          <Alert
            message={
              exportComplete ? 'Export completed successfully!' : 'Exporting metadata...'
            }
            description={
              <div style={{ marginTop: 8 }}>
                <Progress
                  percent={exportProgress}
                  status={exportComplete ? 'success' : 'active'}
                  strokeColor={exportComplete ? '#52c41a' : '#1890ff'}
                />
                {exportComplete && (
                  <div style={{ marginTop: 8 }}>
                    <Button
                      type="link"
                      icon={<CloudDownloadOutlined />}
                      onClick={() => console.log('Download file')}
                    >
                      Download File
                    </Button>
                  </div>
                )}
              </div>
            }
            type={exportComplete ? 'success' : 'info'}
            style={{ marginBottom: 16 }}
          />
        )}

        {/* Format Selection */}
        <Card title="Export Format" size="small" style={{ marginBottom: 16 }}>
          <Form.Item name="format">
            <Radio.Group>
              <Row gutter={16}>
                {formatOptions.map((option) => (
                  <Col key={option.value} span={12}>
                    <Radio.Button
                      value={option.value}
                      style={{
                        width: '100%',
                        height: 'auto',
                        padding: '12px',
                        textAlign: 'left',
                      }}
                    >
                      <Space direction="vertical" size="small">
                        <Space>
                          {option.icon}
                          <Text strong>{option.label}</Text>
                        </Space>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {option.description}
                        </Text>
                      </Space>
                    </Radio.Button>
                  </Col>
                ))}
              </Row>
            </Radio.Group>
          </Form.Item>
        </Card>

        {/* Content Options */}
        <Card title="Content Options" size="small" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="includeColumns" valuePropName="checked">
                <Checkbox>Include column metadata</Checkbox>
              </Form.Item>
              <Form.Item name="includeGlossary" valuePropName="checked">
                <Checkbox>Include business glossary</Checkbox>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="includeValidation" valuePropName="checked">
                <Checkbox>Include validation results</Checkbox>
              </Form.Item>
              <Form.Item name="includeStatistics" valuePropName="checked">
                <Checkbox>Include statistics summary</Checkbox>
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Filters */}
        <Card title="Filters" size="small" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="schemas" label="Schemas">
                <Select
                  mode="multiple"
                  placeholder="Select schemas (all if empty)"
                  allowClear
                >
                  {availableSchemas.map(schema => (
                    <Option key={schema} value={schema}>
                      {schema}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="domains" label="Domains">
                <Select
                  mode="multiple"
                  placeholder="Select domains (all if empty)"
                  allowClear
                >
                  {availableDomains.map(domain => (
                    <Option key={domain} value={domain}>
                      {domain}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="dateRange" label="Date Range">
            <RangePicker style={{ width: '100%' }} />
          </Form.Item>
        </Card>

        {/* Export Summary */}
        <Card title="Export Summary" size="small">
          <Row gutter={16}>
            <Col span={8}>
              <Text type="secondary">Total Tables:</Text>
              <br />
              <Text strong>
                {selectedTableIds.length > 0 ? selectedTableIds.length : statistics?.totalTables || 0}
              </Text>
            </Col>
            <Col span={8}>
              <Text type="secondary">Total Columns:</Text>
              <br />
              <Text strong>{statistics?.totalColumns || 0}</Text>
            </Col>
            <Col span={8}>
              <Text type="secondary">Glossary Terms:</Text>
              <br />
              <Text strong>{statistics?.totalGlossaryTerms || 0}</Text>
            </Col>
          </Row>
        </Card>

        {/* Export Tips */}
        <Alert
          message="Export Tips"
          description={
            <ul style={{ margin: 0, paddingLeft: 20 }}>
              <li>Excel format provides the most comprehensive view with multiple sheets</li>
              <li>CSV format is best for data analysis and importing into other tools</li>
              <li>PDF format creates a formatted report suitable for documentation</li>
              <li>JSON format provides machine-readable structured data</li>
              <li>Large exports may take several minutes to complete</li>
            </ul>
          }
          type="info"
          showIcon
          style={{ marginTop: 16 }}
        />
      </Form>
    </Modal>
  )
}

export default MetadataExportManager
