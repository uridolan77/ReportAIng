import React, { useState } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Button, 
  Select, 
  DatePicker, 
  Checkbox, 
  Upload, 
  Progress, 
  Alert, 
  Tabs,
  Row,
  Col,
  Statistic,
  List,
  Tag,
  Modal,
  notification,
  Divider
} from 'antd'
import {
  ExportOutlined,
  ImportOutlined,
  DownloadOutlined,
  UploadOutlined,
  FileTextOutlined,
  DatabaseOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  DeleteOutlined,
  EyeOutlined
} from '@ant-design/icons'
import { useExportTransparencyDataMutation } from '@shared/store/api/transparencyApi'
import type { RcFile } from 'antd/es/upload'
import dayjs from 'dayjs'

const { Title, Text, Paragraph } = Typography
const { TabPane } = Tabs
const { RangePicker } = DatePicker
const { Option } = Select

export interface DataExportImportProps {
  onExportComplete?: (exportId: string, downloadUrl: string) => void
  onImportComplete?: (importId: string, results: ImportResults) => void
  className?: string
  testId?: string
}

interface ExportOptions {
  format: 'json' | 'csv' | 'excel' | 'parquet'
  dateRange: [dayjs.Dayjs, dayjs.Dayjs] | null
  includeTraces: boolean
  includeMetrics: boolean
  includeFeedback: boolean
  includeSettings: boolean
  includeMetadata: boolean
  compressionLevel: 'none' | 'gzip' | 'zip'
  maxFileSize: number
}

interface ImportResults {
  totalRecords: number
  successfulRecords: number
  failedRecords: number
  warnings: string[]
  errors: string[]
  duration: number
}

interface ExportJob {
  id: string
  status: 'pending' | 'processing' | 'completed' | 'failed'
  progress: number
  format: string
  size: number
  createdAt: string
  downloadUrl?: string
  error?: string
}

/**
 * DataExportImport - Comprehensive data export and import interface
 * 
 * Features:
 * - Multiple export formats (JSON, CSV, Excel, Parquet)
 * - Flexible date range selection
 * - Selective data inclusion options
 * - Compression options
 * - Import validation and error handling
 * - Export job tracking
 * - Bulk operations
 * - Data preview capabilities
 */
export const DataExportImport: React.FC<DataExportImportProps> = ({
  onExportComplete,
  onImportComplete,
  className = '',
  testId = 'data-export-import'
}) => {
  const [activeTab, setActiveTab] = useState('export')
  const [exportOptions, setExportOptions] = useState<ExportOptions>({
    format: 'json',
    dateRange: [dayjs().subtract(30, 'days'), dayjs()],
    includeTraces: true,
    includeMetrics: true,
    includeFeedback: true,
    includeSettings: false,
    includeMetadata: true,
    compressionLevel: 'gzip',
    maxFileSize: 100
  })
  
  const [exportJobs, setExportJobs] = useState<ExportJob[]>([
    {
      id: 'export-1',
      status: 'completed',
      progress: 100,
      format: 'json',
      size: 2.4,
      createdAt: '2024-01-15T10:30:00Z',
      downloadUrl: '/api/exports/export-1/download'
    },
    {
      id: 'export-2',
      status: 'processing',
      progress: 65,
      format: 'csv',
      size: 1.8,
      createdAt: '2024-01-15T11:00:00Z'
    }
  ])

  const [importProgress, setImportProgress] = useState(0)
  const [importResults, setImportResults] = useState<ImportResults | null>(null)
  const [previewModalVisible, setPreviewModalVisible] = useState(false)
  const [selectedExport, setSelectedExport] = useState<ExportJob | null>(null)

  const [exportData] = useExportTransparencyDataMutation()

  const handleExport = async () => {
    try {
      const exportJob: ExportJob = {
        id: `export-${Date.now()}`,
        status: 'processing',
        progress: 0,
        format: exportOptions.format,
        size: 0,
        createdAt: new Date().toISOString()
      }

      setExportJobs(prev => [exportJob, ...prev])

      // Simulate export progress
      const progressInterval = setInterval(() => {
        setExportJobs(prev => prev.map(job => 
          job.id === exportJob.id 
            ? { ...job, progress: Math.min(job.progress + 10, 100) }
            : job
        ))
      }, 500)

      // Simulate completion after 5 seconds
      setTimeout(() => {
        clearInterval(progressInterval)
        setExportJobs(prev => prev.map(job => 
          job.id === exportJob.id 
            ? { 
                ...job, 
                status: 'completed' as const, 
                progress: 100,
                size: Math.random() * 5 + 1,
                downloadUrl: `/api/exports/${exportJob.id}/download`
              }
            : job
        ))

        notification.success({
          message: 'Export Completed',
          description: 'Your data export has been completed successfully.'
        })

        onExportComplete?.(exportJob.id, `/api/exports/${exportJob.id}/download`)
      }, 5000)

    } catch (error) {
      notification.error({
        message: 'Export Failed',
        description: 'Failed to start data export. Please try again.'
      })
    }
  }

  const handleImport = (file: RcFile) => {
    setImportProgress(0)
    setImportResults(null)

    // Simulate import progress
    const progressInterval = setInterval(() => {
      setImportProgress(prev => {
        if (prev >= 100) {
          clearInterval(progressInterval)
          
          // Simulate import results
          const results: ImportResults = {
            totalRecords: 1250,
            successfulRecords: 1200,
            failedRecords: 50,
            warnings: [
              'Some records had missing confidence scores',
              'Duplicate trace IDs were found and merged'
            ],
            errors: [
              'Invalid date format in 15 records',
              'Missing required fields in 35 records'
            ],
            duration: 45.2
          }

          setImportResults(results)
          onImportComplete?.('import-' + Date.now(), results)

          notification.success({
            message: 'Import Completed',
            description: `Successfully imported ${results.successfulRecords} of ${results.totalRecords} records.`
          })

          return 100
        }
        return prev + 5
      })
    }, 200)

    return false // Prevent default upload
  }

  const handleDownload = (job: ExportJob) => {
    if (job.downloadUrl) {
      // In real app, this would trigger actual download
      window.open(job.downloadUrl, '_blank')
    }
  }

  const handleDeleteExport = (jobId: string) => {
    setExportJobs(prev => prev.filter(job => job.id !== jobId))
    notification.success({
      message: 'Export Deleted',
      description: 'Export job has been deleted successfully.'
    })
  }

  const handlePreviewExport = (job: ExportJob) => {
    setSelectedExport(job)
    setPreviewModalVisible(true)
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return '#52c41a'
      case 'processing': return '#1890ff'
      case 'failed': return '#ff4d4f'
      default: return '#faad14'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'completed': return <CheckCircleOutlined />
      case 'processing': return <ClockCircleOutlined />
      case 'failed': return <WarningOutlined />
      default: return <ClockCircleOutlined />
    }
  }

  const renderExportTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Export Configuration */}
      <Card size="small" title="Export Configuration">
        <Row gutter={[16, 16]}>
          <Col span={12}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Format</Text>
              <Select
                value={exportOptions.format}
                onChange={(value) => setExportOptions(prev => ({ ...prev, format: value }))}
                style={{ width: '100%' }}
              >
                <Option value="json">JSON</Option>
                <Option value="csv">CSV</Option>
                <Option value="excel">Excel</Option>
                <Option value="parquet">Parquet</Option>
              </Select>
            </Space>
          </Col>
          <Col span={12}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Date Range</Text>
              <RangePicker
                value={exportOptions.dateRange}
                onChange={(dates) => setExportOptions(prev => ({ ...prev, dateRange: dates }))}
                style={{ width: '100%' }}
              />
            </Space>
          </Col>
          <Col span={12}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Compression</Text>
              <Select
                value={exportOptions.compressionLevel}
                onChange={(value) => setExportOptions(prev => ({ ...prev, compressionLevel: value }))}
                style={{ width: '100%' }}
              >
                <Option value="none">None</Option>
                <Option value="gzip">GZIP</Option>
                <Option value="zip">ZIP</Option>
              </Select>
            </Space>
          </Col>
          <Col span={12}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Max File Size (MB)</Text>
              <Select
                value={exportOptions.maxFileSize}
                onChange={(value) => setExportOptions(prev => ({ ...prev, maxFileSize: value }))}
                style={{ width: '100%' }}
              >
                <Option value={50}>50 MB</Option>
                <Option value={100}>100 MB</Option>
                <Option value={500}>500 MB</Option>
                <Option value={1000}>1 GB</Option>
              </Select>
            </Space>
          </Col>
        </Row>

        <Divider />

        <Space direction="vertical" style={{ width: '100%' }}>
          <Text strong>Include Data Types</Text>
          <Row gutter={[16, 8]}>
            <Col span={12}>
              <Checkbox
                checked={exportOptions.includeTraces}
                onChange={(e) => setExportOptions(prev => ({ ...prev, includeTraces: e.target.checked }))}
              >
                Transparency Traces
              </Checkbox>
            </Col>
            <Col span={12}>
              <Checkbox
                checked={exportOptions.includeMetrics}
                onChange={(e) => setExportOptions(prev => ({ ...prev, includeMetrics: e.target.checked }))}
              >
                Performance Metrics
              </Checkbox>
            </Col>
            <Col span={12}>
              <Checkbox
                checked={exportOptions.includeFeedback}
                onChange={(e) => setExportOptions(prev => ({ ...prev, includeFeedback: e.target.checked }))}
              >
                User Feedback
              </Checkbox>
            </Col>
            <Col span={12}>
              <Checkbox
                checked={exportOptions.includeSettings}
                onChange={(e) => setExportOptions(prev => ({ ...prev, includeSettings: e.target.checked }))}
              >
                System Settings
              </Checkbox>
            </Col>
            <Col span={12}>
              <Checkbox
                checked={exportOptions.includeMetadata}
                onChange={(e) => setExportOptions(prev => ({ ...prev, includeMetadata: e.target.checked }))}
              >
                Metadata
              </Checkbox>
            </Col>
          </Row>
        </Space>

        <Divider />

        <Button
          type="primary"
          icon={<ExportOutlined />}
          onClick={handleExport}
          size="large"
          block
        >
          Start Export
        </Button>
      </Card>

      {/* Export Jobs */}
      <Card size="small" title={`Export Jobs (${exportJobs.length})`}>
        <List
          dataSource={exportJobs}
          renderItem={(job) => (
            <List.Item
              actions={[
                job.status === 'completed' && (
                  <Button
                    key="download"
                    type="link"
                    icon={<DownloadOutlined />}
                    onClick={() => handleDownload(job)}
                  >
                    Download
                  </Button>
                ),
                job.status === 'completed' && (
                  <Button
                    key="preview"
                    type="link"
                    icon={<EyeOutlined />}
                    onClick={() => handlePreviewExport(job)}
                  >
                    Preview
                  </Button>
                ),
                <Button
                  key="delete"
                  type="link"
                  icon={<DeleteOutlined />}
                  onClick={() => handleDeleteExport(job.id)}
                  danger
                >
                  Delete
                </Button>
              ].filter(Boolean)}
            >
              <List.Item.Meta
                avatar={getStatusIcon(job.status)}
                title={
                  <Space>
                    <Text strong>{job.id}</Text>
                    <Tag color={getStatusColor(job.status)}>
                      {job.status.toUpperCase()}
                    </Tag>
                    <Tag>{job.format.toUpperCase()}</Tag>
                  </Space>
                }
                description={
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Space>
                      <Text type="secondary">Created: {dayjs(job.createdAt).format('MMM DD, HH:mm')}</Text>
                      {job.size > 0 && (
                        <Text type="secondary">Size: {job.size.toFixed(1)} MB</Text>
                      )}
                    </Space>
                    {job.status === 'processing' && (
                      <Progress percent={job.progress} size="small" />
                    )}
                    {job.error && (
                      <Alert message={job.error} type="error" size="small" />
                    )}
                  </Space>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    </Space>
  )

  const renderImportTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Import Upload */}
      <Card size="small" title="Import Data">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Alert
            message="Import Guidelines"
            description="Supported formats: JSON, CSV, Excel. Maximum file size: 100MB. Ensure data follows the expected schema."
            type="info"
            showIcon
          />

          <Upload.Dragger
            name="file"
            multiple={false}
            beforeUpload={handleImport}
            accept=".json,.csv,.xlsx,.xls"
          >
            <p className="ant-upload-drag-icon">
              <UploadOutlined />
            </p>
            <p className="ant-upload-text">Click or drag file to this area to upload</p>
            <p className="ant-upload-hint">
              Support for JSON, CSV, and Excel files. Maximum size: 100MB.
            </p>
          </Upload.Dragger>

          {importProgress > 0 && importProgress < 100 && (
            <Progress
              percent={importProgress}
              status="active"
              format={(percent) => `Importing... ${percent}%`}
            />
          )}
        </Space>
      </Card>

      {/* Import Results */}
      {importResults && (
        <Card size="small" title="Import Results">
          <Row gutter={[16, 16]}>
            <Col span={6}>
              <Statistic
                title="Total Records"
                value={importResults.totalRecords}
                prefix={<DatabaseOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Successful"
                value={importResults.successfulRecords}
                valueStyle={{ color: '#52c41a' }}
                prefix={<CheckCircleOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Failed"
                value={importResults.failedRecords}
                valueStyle={{ color: '#ff4d4f' }}
                prefix={<WarningOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Duration"
                value={importResults.duration}
                suffix="s"
                prefix={<ClockCircleOutlined />}
              />
            </Col>
          </Row>

          {importResults.warnings.length > 0 && (
            <Alert
              message="Warnings"
              description={
                <ul>
                  {importResults.warnings.map((warning, index) => (
                    <li key={index}>{warning}</li>
                  ))}
                </ul>
              }
              type="warning"
              showIcon
              style={{ marginTop: 16 }}
            />
          )}

          {importResults.errors.length > 0 && (
            <Alert
              message="Errors"
              description={
                <ul>
                  {importResults.errors.map((error, index) => (
                    <li key={index}>{error}</li>
                  ))}
                </ul>
              }
              type="error"
              showIcon
              style={{ marginTop: 16 }}
            />
          )}
        </Card>
      )}
    </Space>
  )

  return (
    <div className={`data-export-import ${className}`} data-testid={testId}>
      <Card
        title={
          <Space>
            <DatabaseOutlined />
            <span>Data Export & Import</span>
          </Space>
        }
      >
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane
            tab={
              <Space>
                <ExportOutlined />
                <span>Export</span>
              </Space>
            }
            key="export"
          >
            {renderExportTab()}
          </TabPane>

          <TabPane
            tab={
              <Space>
                <ImportOutlined />
                <span>Import</span>
              </Space>
            }
            key="import"
          >
            {renderImportTab()}
          </TabPane>
        </Tabs>
      </Card>

      {/* Preview Modal */}
      <Modal
        title="Export Preview"
        open={previewModalVisible}
        onCancel={() => setPreviewModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setPreviewModalVisible(false)}>
            Close
          </Button>,
          selectedExport && (
            <Button
              key="download"
              type="primary"
              icon={<DownloadOutlined />}
              onClick={() => {
                handleDownload(selectedExport)
                setPreviewModalVisible(false)
              }}
            >
              Download
            </Button>
          )
        ]}
        width={800}
      >
        {selectedExport && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Text strong>Export ID:</Text> {selectedExport.id}
              </Col>
              <Col span={12}>
                <Text strong>Format:</Text> {selectedExport.format.toUpperCase()}
              </Col>
              <Col span={12}>
                <Text strong>Size:</Text> {selectedExport.size.toFixed(1)} MB
              </Col>
              <Col span={12}>
                <Text strong>Created:</Text> {dayjs(selectedExport.createdAt).format('MMM DD, YYYY HH:mm')}
              </Col>
            </Row>
            <Divider />
            <Text type="secondary">
              This export contains transparency data based on your selected criteria. 
              Click Download to save the file to your device.
            </Text>
          </Space>
        )}
      </Modal>
    </div>
  )
}

export default DataExportImport
