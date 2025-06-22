import React, { useState } from 'react'
import { 
  Modal, 
  Tabs, 
  Upload, 
  Button, 
  Select, 
  Checkbox, 
  Alert, 
  Progress, 
  List, 
  Typography, 
  Space,
  Card,
  Row,
  Col,
  message,
  Divider
} from 'antd'
import { 
  UploadOutlined, 
  DownloadOutlined, 
  FileTextOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  CloudUploadOutlined,
  FileExcelOutlined,
  FileOutlined
} from '@ant-design/icons'
import type { UploadProps } from 'antd'

const { Title, Text } = Typography
const { Dragger } = Upload

interface TemplateImportExportProps {
  visible: boolean
  onClose: () => void
}

export const TemplateImportExport: React.FC<TemplateImportExportProps> = ({
  visible,
  onClose
}) => {
  const [activeTab, setActiveTab] = useState('export')
  const [exportFormat, setExportFormat] = useState('json')
  const [selectedTemplates, setSelectedTemplates] = useState<string[]>([])
  const [includeMetadata, setIncludeMetadata] = useState(true)
  const [includeVersionHistory, setIncludeVersionHistory] = useState(false)
  const [importProgress, setImportProgress] = useState(0)
  const [importResults, setImportResults] = useState<any[]>([])
  const [isProcessing, setIsProcessing] = useState(false)

  // Mock template data for export selection
  const availableTemplates = [
    { key: 'sql_generation', name: 'SQL Generation Template', intentType: 'sql_generation' },
    { key: 'insight_generation', name: 'Insight Generation Template', intentType: 'insight_generation' },
    { key: 'explanation', name: 'Explanation Template', intentType: 'explanation' },
    { key: 'data_analysis', name: 'Data Analysis Template', intentType: 'data_analysis' }
  ]

  const handleExport = async () => {
    setIsProcessing(true)
    try {
      // Simulate export process
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      const exportData = {
        templates: selectedTemplates.length > 0 ? selectedTemplates : availableTemplates.map(t => t.key),
        format: exportFormat,
        includeMetadata,
        includeVersionHistory,
        exportDate: new Date().toISOString(),
        version: '1.0'
      }

      // Create and download file
      const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `templates_export_${new Date().toISOString().split('T')[0]}.${exportFormat}`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      message.success('Templates exported successfully')
    } catch (error) {
      message.error('Export failed')
    } finally {
      setIsProcessing(false)
    }
  }

  const uploadProps: UploadProps = {
    name: 'file',
    multiple: false,
    accept: '.json,.csv,.xlsx',
    beforeUpload: (file) => {
      handleImport(file)
      return false // Prevent automatic upload
    },
    showUploadList: false
  }

  const handleImport = async (file: File) => {
    setIsProcessing(true)
    setImportProgress(0)
    setImportResults([])

    try {
      const text = await file.text()
      let data

      if (file.name.endsWith('.json')) {
        data = JSON.parse(text)
      } else if (file.name.endsWith('.csv')) {
        // Parse CSV (simplified)
        data = parseCSV(text)
      } else {
        throw new Error('Unsupported file format')
      }

      // Simulate import progress
      const templates = data.templates || [data]
      for (let i = 0; i < templates.length; i++) {
        await new Promise(resolve => setTimeout(resolve, 500))
        setImportProgress(((i + 1) / templates.length) * 100)
        
        // Validate and process each template
        const result = await processTemplate(templates[i])
        setImportResults(prev => [...prev, result])
      }

      message.success(`Successfully imported ${templates.length} templates`)
    } catch (error) {
      message.error('Import failed: ' + (error as Error).message)
    } finally {
      setIsProcessing(false)
    }
  }

  const parseCSV = (text: string) => {
    // Simplified CSV parsing
    const lines = text.split('\n')
    const headers = lines[0].split(',')
    const templates = []

    for (let i = 1; i < lines.length; i++) {
      if (lines[i].trim()) {
        const values = lines[i].split(',')
        const template: any = {}
        headers.forEach((header, index) => {
          template[header.trim()] = values[index]?.trim()
        })
        templates.push(template)
      }
    }

    return { templates }
  }

  const processTemplate = async (templateData: any) => {
    // Validate template structure
    const errors = []
    const warnings = []

    if (!templateData.templateKey) {
      errors.push('Missing template key')
    }
    if (!templateData.templateName) {
      errors.push('Missing template name')
    }
    if (!templateData.content) {
      errors.push('Missing template content')
    }
    if (!templateData.intentType) {
      warnings.push('Missing intent type, defaulting to "general"')
    }

    return {
      templateKey: templateData.templateKey,
      templateName: templateData.templateName,
      status: errors.length > 0 ? 'error' : warnings.length > 0 ? 'warning' : 'success',
      errors,
      warnings
    }
  }

  const exportTab = (
    <div>
      <Card title="Export Configuration" style={{ marginBottom: '16px' }}>
        <Row gutter={16}>
          <Col span={12}>
            <div style={{ marginBottom: '16px' }}>
              <Text strong>Export Format:</Text>
              <Select
                value={exportFormat}
                onChange={setExportFormat}
                style={{ width: '100%', marginTop: '8px' }}
              >
                <Select.Option value="json">JSON</Select.Option>
                <Select.Option value="csv">CSV</Select.Option>
                <Select.Option value="xlsx">Excel (XLSX)</Select.Option>
              </Select>
            </div>

            <div style={{ marginBottom: '16px' }}>
              <Text strong>Include Options:</Text>
              <div style={{ marginTop: '8px' }}>
                <Checkbox
                  checked={includeMetadata}
                  onChange={(e) => setIncludeMetadata(e.target.checked)}
                >
                  Business Metadata
                </Checkbox>
              </div>
              <div style={{ marginTop: '4px' }}>
                <Checkbox
                  checked={includeVersionHistory}
                  onChange={(e) => setIncludeVersionHistory(e.target.checked)}
                >
                  Version History
                </Checkbox>
              </div>
            </div>
          </Col>

          <Col span={12}>
            <div>
              <Text strong>Select Templates:</Text>
              <Select
                mode="multiple"
                placeholder="Select templates to export (leave empty for all)"
                value={selectedTemplates}
                onChange={setSelectedTemplates}
                style={{ width: '100%', marginTop: '8px' }}
              >
                {availableTemplates.map(template => (
                  <Select.Option key={template.key} value={template.key}>
                    {template.name}
                  </Select.Option>
                ))}
              </Select>
            </div>
          </Col>
        </Row>

        <Divider />

        <Alert
          message="Export Information"
          description={`Exporting ${selectedTemplates.length > 0 ? selectedTemplates.length : availableTemplates.length} templates in ${exportFormat.toUpperCase()} format${includeMetadata ? ' with metadata' : ''}${includeVersionHistory ? ' and version history' : ''}.`}
          type="info"
          showIcon
          style={{ marginBottom: '16px' }}
        />

        <Button
          type="primary"
          icon={<DownloadOutlined />}
          onClick={handleExport}
          loading={isProcessing}
          size="large"
          block
        >
          Export Templates
        </Button>
      </Card>
    </div>
  )

  const importTab = (
    <div>
      <Card title="Import Templates" style={{ marginBottom: '16px' }}>
        <Alert
          message="Supported Formats"
          description="Upload JSON, CSV, or Excel files containing template data. Ensure your file includes required fields: templateKey, templateName, content, and intentType."
          type="info"
          showIcon
          style={{ marginBottom: '16px' }}
        />

        <Dragger {...uploadProps} style={{ marginBottom: '16px' }}>
          <p className="ant-upload-drag-icon">
            <CloudUploadOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
          </p>
          <p className="ant-upload-text">Click or drag file to this area to upload</p>
          <p className="ant-upload-hint">
            Support for JSON, CSV, and Excel files. Maximum file size: 10MB
          </p>
        </Dragger>

        {isProcessing && (
          <Card size="small" style={{ marginBottom: '16px' }}>
            <div style={{ textAlign: 'center' }}>
              <Progress percent={Math.round(importProgress)} />
              <Text>Processing templates...</Text>
            </div>
          </Card>
        )}

        {importResults.length > 0 && (
          <Card title="Import Results" size="small">
            <List
              size="small"
              dataSource={importResults}
              renderItem={(result) => (
                <List.Item>
                  <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                    <Space>
                      {result.status === 'success' && <CheckCircleOutlined style={{ color: '#52c41a' }} />}
                      {result.status === 'warning' && <ExclamationCircleOutlined style={{ color: '#fa8c16' }} />}
                      {result.status === 'error' && <ExclamationCircleOutlined style={{ color: '#f5222d' }} />}
                      <Text strong>{result.templateName || result.templateKey}</Text>
                    </Space>
                    <Space>
                      {result.errors?.length > 0 && (
                        <Text type="danger">{result.errors.length} errors</Text>
                      )}
                      {result.warnings?.length > 0 && (
                        <Text style={{ color: '#fa8c16' }}>{result.warnings.length} warnings</Text>
                      )}
                    </Space>
                  </Space>
                </List.Item>
              )}
            />
          </Card>
        )}
      </Card>
    </div>
  )

  const templatesTab = (
    <div>
      <Card title="Template Library" style={{ marginBottom: '16px' }}>
        <Alert
          message="Template Sharing"
          description="Browse and import templates from the community library or share your own templates with others."
          type="info"
          showIcon
          style={{ marginBottom: '16px' }}
        />

        <Row gutter={16}>
          <Col span={8}>
            <Card size="small" hoverable>
              <div style={{ textAlign: 'center' }}>
                <FileTextOutlined style={{ fontSize: '32px', color: '#1890ff' }} />
                <Title level={5}>SQL Templates</Title>
                <Text type="secondary">25 templates</Text>
                <div style={{ marginTop: '8px' }}>
                  <Button size="small">Browse</Button>
                </div>
              </div>
            </Card>
          </Col>
          <Col span={8}>
            <Card size="small" hoverable>
              <div style={{ textAlign: 'center' }}>
                <FileExcelOutlined style={{ fontSize: '32px', color: '#52c41a' }} />
                <Title level={5}>Analytics Templates</Title>
                <Text type="secondary">18 templates</Text>
                <div style={{ marginTop: '8px' }}>
                  <Button size="small">Browse</Button>
                </div>
              </div>
            </Card>
          </Col>
          <Col span={8}>
            <Card size="small" hoverable>
              <div style={{ textAlign: 'center' }}>
                <FileOutlined style={{ fontSize: '32px', color: '#722ed1' }} />
                <Title level={5}>Report Templates</Title>
                <Text type="secondary">12 templates</Text>
                <div style={{ marginTop: '8px' }}>
                  <Button size="small">Browse</Button>
                </div>
              </div>
            </Card>
          </Col>
        </Row>
      </Card>
    </div>
  )

  const tabs = [
    {
      key: 'export',
      label: (
        <Space>
          <DownloadOutlined />
          Export
        </Space>
      ),
      children: exportTab
    },
    {
      key: 'import',
      label: (
        <Space>
          <UploadOutlined />
          Import
        </Space>
      ),
      children: importTab
    },
    {
      key: 'library',
      label: (
        <Space>
          <FileTextOutlined />
          Template Library
        </Space>
      ),
      children: templatesTab
    }
  ]

  return (
    <Modal
      title="Template Import/Export"
      open={visible}
      onCancel={onClose}
      width={800}
      footer={[
        <Button key="close" onClick={onClose}>
          Close
        </Button>
      ]}
    >
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabs}
      />
    </Modal>
  )
}

export default TemplateImportExport
