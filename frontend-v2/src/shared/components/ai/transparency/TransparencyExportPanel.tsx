import React, { useState } from 'react'
import { 
  Card, 
  Form, 
  Select, 
  DatePicker, 
  Button, 
  Space, 
  Typography, 
  Alert, 
  Checkbox,
  Input,
  Row,
  Col,
  Statistic,
  Progress,
  message,
  Tooltip,
  Divider
} from 'antd'
import {
  DownloadOutlined,
  FileExcelOutlined,
  FileTextOutlined,
  FileOutlined,
  CalendarOutlined,
  FilterOutlined,
  InfoCircleOutlined,
  CloudDownloadOutlined
} from '@ant-design/icons'
import { useExportTransparencyDataMutation } from '@shared/store/api/transparencyApi'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { Option } = Select
const { TextArea } = Input

export interface TransparencyExportPanelProps {
  defaultUserId?: string
  defaultTraceIds?: string[]
  onExportComplete?: (filename: string) => void
  className?: string
}

interface ExportConfig {
  format: 'json' | 'csv' | 'excel'
  dateRange: [dayjs.Dayjs, dayjs.Dayjs] | null
  userId?: string
  traceIds?: string[]
  includeSteps: boolean
  includeMetrics: boolean
  includeConfidenceData: boolean
  includeOptimizations: boolean
  customFilename?: string
}

/**
 * TransparencyExportPanel - Export transparency data with flexible options
 * 
 * Features:
 * - Multiple export formats (JSON, CSV, Excel)
 * - Date range selection
 * - User and trace filtering
 * - Selective data inclusion
 * - Custom filename support
 * - Progress tracking
 * - Download management
 */
export const TransparencyExportPanel: React.FC<TransparencyExportPanelProps> = ({
  defaultUserId,
  defaultTraceIds,
  onExportComplete,
  className
}) => {
  const [form] = Form.useForm()
  const [exportConfig, setExportConfig] = useState<ExportConfig>({
    format: 'json',
    dateRange: [dayjs().subtract(7, 'days'), dayjs()],
    userId: defaultUserId,
    traceIds: defaultTraceIds,
    includeSteps: true,
    includeMetrics: true,
    includeConfidenceData: true,
    includeOptimizations: true
  })
  const [isExporting, setIsExporting] = useState(false)
  const [exportProgress, setExportProgress] = useState(0)
  const [estimatedSize, setEstimatedSize] = useState<string>('')

  const [exportTransparencyData] = useExportTransparencyDataMutation()

  // Format configurations
  const formatConfigs = {
    json: {
      icon: <FileOutlined />,
      label: 'JSON',
      description: 'Structured data format, best for programmatic use',
      extension: '.json',
      color: '#1890ff'
    },
    csv: {
      icon: <FileTextOutlined />,
      label: 'CSV',
      description: 'Comma-separated values, best for spreadsheet analysis',
      extension: '.csv',
      color: '#52c41a'
    },
    excel: {
      icon: <FileExcelOutlined />,
      label: 'Excel',
      description: 'Microsoft Excel format with multiple sheets',
      extension: '.xlsx',
      color: '#faad14'
    }
  }

  // Handle export configuration changes
  const handleConfigChange = (field: keyof ExportConfig, value: any) => {
    setExportConfig(prev => ({
      ...prev,
      [field]: value
    }))

    // Estimate file size based on configuration
    updateEstimatedSize({ ...exportConfig, [field]: value })
  }

  // Update estimated file size
  const updateEstimatedSize = (config: ExportConfig) => {
    // Simple estimation logic (in a real app, this might call an API)
    let baseSize = 1024 // 1KB base
    
    if (config.includeSteps) baseSize *= 3
    if (config.includeMetrics) baseSize *= 2
    if (config.includeConfidenceData) baseSize *= 1.5
    if (config.includeOptimizations) baseSize *= 1.2
    
    if (config.dateRange) {
      const days = config.dateRange[1].diff(config.dateRange[0], 'days')
      baseSize *= Math.max(1, days / 7) // Scale by weeks
    }

    if (config.format === 'excel') baseSize *= 1.5
    if (config.format === 'json') baseSize *= 1.2

    const sizeInKB = Math.round(baseSize)
    if (sizeInKB > 1024) {
      setEstimatedSize(`~${(sizeInKB / 1024).toFixed(1)} MB`)
    } else {
      setEstimatedSize(`~${sizeInKB} KB`)
    }
  }

  // Handle export execution
  const handleExport = async () => {
    try {
      setIsExporting(true)
      setExportProgress(0)

      // Simulate progress updates
      const progressInterval = setInterval(() => {
        setExportProgress(prev => Math.min(prev + 10, 90))
      }, 200)

      const exportRequest = {
        format: exportConfig.format,
        startDate: exportConfig.dateRange?.[0].toISOString(),
        endDate: exportConfig.dateRange?.[1].toISOString(),
        userId: exportConfig.userId,
        traceIds: exportConfig.traceIds,
        includeSteps: exportConfig.includeSteps,
        includeMetrics: exportConfig.includeMetrics,
        includeConfidenceData: exportConfig.includeConfidenceData,
        includeOptimizations: exportConfig.includeOptimizations
      }

      const result = await exportTransparencyData(exportRequest).unwrap()
      
      clearInterval(progressInterval)
      setExportProgress(100)

      // Create download link
      const url = window.URL.createObjectURL(result)
      const link = document.createElement('a')
      link.href = url
      
      const filename = exportConfig.customFilename || 
        `transparency-export-${dayjs().format('YYYY-MM-DD-HHmm')}${formatConfigs[exportConfig.format].extension}`
      
      link.download = filename
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      message.success('Export completed successfully!')
      onExportComplete?.(filename)

    } catch (error) {
      console.error('Export failed:', error)
      message.error('Export failed. Please try again.')
    } finally {
      setIsExporting(false)
      setExportProgress(0)
    }
  }

  // Validate form before export
  const canExport = exportConfig.dateRange && 
    (exportConfig.userId || (exportConfig.traceIds && exportConfig.traceIds.length > 0))

  return (
    <Card
      title={
        <Space>
          <CloudDownloadOutlined />
          <span>Export Transparency Data</span>
        </Space>
      }
      className={className}
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={exportConfig}
      >
        {/* Export Format Selection */}
        <Form.Item label="Export Format" required>
          <Row gutter={16}>
            {Object.entries(formatConfigs).map(([format, config]) => (
              <Col span={8} key={format}>
                <Card
                  size="small"
                  hoverable
                  style={{
                    border: exportConfig.format === format ? `2px solid ${config.color}` : '1px solid #d9d9d9',
                    cursor: 'pointer'
                  }}
                  onClick={() => handleConfigChange('format', format)}
                >
                  <Space direction="vertical" align="center" style={{ width: '100%' }}>
                    <div style={{ fontSize: '24px', color: config.color }}>
                      {config.icon}
                    </div>
                    <Text strong>{config.label}</Text>
                    <Text type="secondary" style={{ fontSize: '12px', textAlign: 'center' }}>
                      {config.description}
                    </Text>
                  </Space>
                </Card>
              </Col>
            ))}
          </Row>
        </Form.Item>

        {/* Date Range */}
        <Form.Item 
          label="Date Range" 
          required
          extra="Select the time period for data export"
        >
          <RangePicker
            value={exportConfig.dateRange}
            onChange={(dates) => handleConfigChange('dateRange', dates)}
            style={{ width: '100%' }}
            presets={[
              { label: 'Last 7 days', value: [dayjs().subtract(7, 'days'), dayjs()] },
              { label: 'Last 30 days', value: [dayjs().subtract(30, 'days'), dayjs()] },
              { label: 'Last 3 months', value: [dayjs().subtract(3, 'months'), dayjs()] },
            ]}
          />
        </Form.Item>

        {/* Filters */}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label="User ID (Optional)">
              <Input
                value={exportConfig.userId}
                onChange={(e) => handleConfigChange('userId', e.target.value)}
                placeholder="Filter by specific user"
                prefix={<FilterOutlined />}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Trace IDs (Optional)">
              <TextArea
                value={exportConfig.traceIds?.join('\n')}
                onChange={(e) => handleConfigChange('traceIds', e.target.value.split('\n').filter(Boolean))}
                placeholder="One trace ID per line"
                rows={3}
              />
            </Form.Item>
          </Col>
        </Row>

        {/* Data Inclusion Options */}
        <Form.Item label="Include Data">
          <Space direction="vertical">
            <Checkbox
              checked={exportConfig.includeSteps}
              onChange={(e) => handleConfigChange('includeSteps', e.target.checked)}
            >
              Processing Steps
            </Checkbox>
            <Checkbox
              checked={exportConfig.includeMetrics}
              onChange={(e) => handleConfigChange('includeMetrics', e.target.checked)}
            >
              Performance Metrics
            </Checkbox>
            <Checkbox
              checked={exportConfig.includeConfidenceData}
              onChange={(e) => handleConfigChange('includeConfidenceData', e.target.checked)}
            >
              Confidence Analysis
            </Checkbox>
            <Checkbox
              checked={exportConfig.includeOptimizations}
              onChange={(e) => handleConfigChange('includeOptimizations', e.target.checked)}
            >
              Optimization Suggestions
            </Checkbox>
          </Space>
        </Form.Item>

        {/* Custom Filename */}
        <Form.Item label="Custom Filename (Optional)">
          <Input
            value={exportConfig.customFilename}
            onChange={(e) => handleConfigChange('customFilename', e.target.value)}
            placeholder="Leave empty for auto-generated name"
            suffix={formatConfigs[exportConfig.format].extension}
          />
        </Form.Item>

        <Divider />

        {/* Export Summary */}
        <Row gutter={16} style={{ marginBottom: 16 }}>
          <Col span={8}>
            <Statistic
              title="Estimated Size"
              value={estimatedSize}
              prefix={<InfoCircleOutlined />}
            />
          </Col>
          <Col span={8}>
            <Statistic
              title="Format"
              value={formatConfigs[exportConfig.format].label}
              prefix={formatConfigs[exportConfig.format].icon}
            />
          </Col>
          <Col span={8}>
            <Statistic
              title="Date Range"
              value={exportConfig.dateRange ? 
                `${exportConfig.dateRange[1].diff(exportConfig.dateRange[0], 'days')} days` : 
                'Not set'
              }
              prefix={<CalendarOutlined />}
            />
          </Col>
        </Row>

        {/* Export Progress */}
        {isExporting && (
          <div style={{ marginBottom: 16 }}>
            <Text>Exporting data...</Text>
            <Progress percent={exportProgress} status="active" />
          </div>
        )}

        {/* Export Button */}
        <Form.Item>
          <Button
            type="primary"
            size="large"
            icon={<DownloadOutlined />}
            onClick={handleExport}
            loading={isExporting}
            disabled={!canExport}
            style={{ width: '100%' }}
          >
            {isExporting ? 'Exporting...' : 'Export Data'}
          </Button>
        </Form.Item>

        {/* Validation Message */}
        {!canExport && (
          <Alert
            message="Export Requirements"
            description="Please select a date range and specify either a User ID or Trace IDs to export."
            type="info"
            showIcon
          />
        )}
      </Form>
    </Card>
  )
}

export default TransparencyExportPanel
