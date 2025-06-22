import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Select, 
  DatePicker,
  Checkbox,
  Typography,
  Space,
  Progress,
  Alert,
  Divider,
  List,
  Tag,
  Tooltip,
  Modal,
  Form,
  Input,
  message,
  Steps
} from 'antd'
import { 
  ExportOutlined,
  DownloadOutlined,
  SettingOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  DatabaseOutlined,
  BarChartOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { useExportAnalyticsMutation } from '@shared/store/api/templateAnalyticsApi'
import type { AnalyticsExportConfig } from '@shared/types/templateAnalytics'

const { Title, Text, Paragraph } = Typography
const { RangePicker } = DatePicker
const { Option } = Select
const { Step } = Steps

interface AdvancedExportInterfaceProps {
  onExportComplete?: (config: AnalyticsExportConfig) => void
}

interface ExportProgress {
  step: number
  message: string
  progress: number
}

export const AdvancedExportInterface: React.FC<AdvancedExportInterfaceProps> = ({
  onExportComplete
}) => {
  // State
  const [exportConfig, setExportConfig] = useState<Partial<AnalyticsExportConfig>>({
    format: 'Excel',
    dateRange: {
      startDate: dayjs().subtract(30, 'day').toISOString(),
      endDate: dayjs().toISOString()
    },
    includedMetrics: ['performance', 'usage', 'quality'],
    includeCharts: true,
    includeRawData: false
  })
  const [exportModalVisible, setExportModalVisible] = useState(false)
  const [exportProgress, setExportProgress] = useState<ExportProgress | null>(null)
  const [currentStep, setCurrentStep] = useState(0)

  // API
  const [exportAnalytics, { isLoading: isExporting }] = useExportAnalyticsMutation()

  // Available metrics
  const availableMetrics = [
    { key: 'performance', label: 'Performance Metrics', description: 'Success rates, response times, error counts' },
    { key: 'usage', label: 'Usage Analytics', description: 'Template usage patterns, frequency, trends' },
    { key: 'quality', label: 'Quality Metrics', description: 'Content quality scores, analysis results' },
    { key: 'abtests', label: 'A/B Test Results', description: 'Test outcomes, comparisons, recommendations' },
    { key: 'improvements', label: 'Improvement Suggestions', description: 'AI-generated optimization recommendations' },
    { key: 'predictions', label: 'Performance Predictions', description: 'ML-based performance forecasts' },
    { key: 'realtime', label: 'Real-time Data', description: 'Current live metrics and activities' }
  ]

  // Format configurations
  const formatConfig = {
    Excel: {
      icon: <FileExcelOutlined style={{ color: '#52c41a' }} />,
      description: 'Comprehensive spreadsheet with multiple sheets and charts',
      features: ['Multiple sheets', 'Charts', 'Formulas', 'Filtering'],
      fileSize: 'Medium'
    },
    CSV: {
      icon: <FileTextOutlined style={{ color: '#1890ff' }} />,
      description: 'Simple comma-separated values for data analysis',
      features: ['Raw data', 'Lightweight', 'Universal compatibility'],
      fileSize: 'Small'
    },
    JSON: {
      icon: <DatabaseOutlined style={{ color: '#722ed1' }} />,
      description: 'Structured data format for API integration',
      features: ['Structured data', 'API friendly', 'Nested objects'],
      fileSize: 'Small'
    }
  }

  const handleExport = async () => {
    if (!exportConfig.includedMetrics || exportConfig.includedMetrics.length === 0) {
      message.warning('Please select at least one metric to export')
      return
    }

    const fullConfig: AnalyticsExportConfig = {
      format: exportConfig.format || 'Excel',
      dateRange: exportConfig.dateRange || {
        startDate: dayjs().subtract(30, 'day').toISOString(),
        endDate: dayjs().toISOString()
      },
      includedMetrics: exportConfig.includedMetrics || [],
      intentTypeFilter: exportConfig.intentTypeFilter,
      includeCharts: exportConfig.includeCharts || false,
      includeRawData: exportConfig.includeRawData || false,
      exportedBy: 'current-user',
      requestedDate: new Date().toISOString()
    }

    try {
      // Simulate export progress
      setExportProgress({ step: 1, message: 'Preparing export...', progress: 10 })
      setCurrentStep(1)

      await new Promise(resolve => setTimeout(resolve, 1000))
      setExportProgress({ step: 2, message: 'Gathering data...', progress: 30 })
      setCurrentStep(2)

      await new Promise(resolve => setTimeout(resolve, 1500))
      setExportProgress({ step: 3, message: 'Processing analytics...', progress: 60 })
      setCurrentStep(3)

      const blob = await exportAnalytics(fullConfig).unwrap()

      setExportProgress({ step: 4, message: 'Generating file...', progress: 90 })
      setCurrentStep(4)

      await new Promise(resolve => setTimeout(resolve, 500))

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      const timestamp = dayjs().format('YYYY-MM-DD-HHmm')
      const extension = fullConfig.format.toLowerCase() === 'excel' ? 'xlsx' : 
                      fullConfig.format.toLowerCase() === 'csv' ? 'csv' : 'json'
      link.download = `template-analytics-${timestamp}.${extension}`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      setExportProgress({ step: 5, message: 'Export completed!', progress: 100 })
      setCurrentStep(5)

      setTimeout(() => {
        setExportProgress(null)
        setCurrentStep(0)
        setExportModalVisible(false)
        message.success('Analytics data exported successfully')
        onExportComplete?.(fullConfig)
      }, 1000)

    } catch (error) {
      setExportProgress(null)
      setCurrentStep(0)
      message.error('Failed to export analytics data')
    }
  }

  const handleDateRangeChange = (dates: [dayjs.Dayjs, dayjs.Dayjs] | null) => {
    if (dates) {
      setExportConfig(prev => ({
        ...prev,
        dateRange: {
          startDate: dates[0].toISOString(),
          endDate: dates[1].toISOString()
        }
      }))
    }
  }

  const handleMetricsChange = (checkedValues: string[]) => {
    setExportConfig(prev => ({
      ...prev,
      includedMetrics: checkedValues
    }))
  }

  const getEstimatedFileSize = (): string => {
    const baseSize = exportConfig.includedMetrics?.length || 1
    const multiplier = exportConfig.includeRawData ? 3 : 1
    const chartMultiplier = exportConfig.includeCharts ? 1.5 : 1
    
    const estimatedMB = Math.round(baseSize * multiplier * chartMultiplier * 2)
    return `~${estimatedMB}MB`
  }

  const getEstimatedRecords = (): string => {
    const days = dayjs(exportConfig.dateRange?.endDate).diff(
      dayjs(exportConfig.dateRange?.startDate), 
      'day'
    )
    const recordsPerDay = 1000 // Estimated
    return `~${(days * recordsPerDay).toLocaleString()} records`
  }

  return (
    <div className="advanced-export-interface">
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <ExportOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>Advanced Export</Title>
                <Text type="secondary">Export comprehensive analytics data with custom configurations</Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Button 
              type="primary" 
              size="large"
              icon={<DownloadOutlined />}
              onClick={() => setExportModalVisible(true)}
            >
              Configure Export
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Quick Export Options */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={8}>
          <Card title="Performance Report" size="small" style={{ height: '200px' }}>
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <BarChartOutlined style={{ fontSize: '48px', color: '#52c41a', marginBottom: '16px' }} />
              <Paragraph style={{ fontSize: '12px' }}>
                Complete performance analytics including success rates, response times, and trends
              </Paragraph>
              <Button
                type="primary"
                onClick={() => {
                  setExportConfig({
                    format: 'Excel',
                    dateRange: {
                      startDate: dayjs().subtract(30, 'day').toISOString(),
                      endDate: dayjs().toISOString()
                    },
                    includedMetrics: ['performance', 'usage'],
                    includeCharts: true,
                    includeRawData: false
                  })
                  handleExport()
                }}
                loading={isExporting}
                block
              >
                Export Performance
              </Button>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Quality Analysis" size="small" style={{ height: '200px' }}>
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <CheckCircleOutlined style={{ fontSize: '48px', color: '#1890ff', marginBottom: '16px' }} />
              <Paragraph style={{ fontSize: '12px' }}>
                Comprehensive quality metrics and improvement suggestions
              </Paragraph>
              <Button
                type="primary"
                onClick={() => {
                  setExportConfig({
                    format: 'Excel',
                    dateRange: {
                      startDate: dayjs().subtract(7, 'day').toISOString(),
                      endDate: dayjs().toISOString()
                    },
                    includedMetrics: ['quality', 'improvements'],
                    includeCharts: true,
                    includeRawData: true
                  })
                  handleExport()
                }}
                loading={isExporting}
                block
              >
                Export Quality
              </Button>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Complete Dataset" size="small" style={{ height: '200px' }}>
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <DatabaseOutlined style={{ fontSize: '48px', color: '#722ed1', marginBottom: '16px' }} />
              <Paragraph style={{ fontSize: '12px' }}>
                Full analytics dataset with all metrics and raw data
              </Paragraph>
              <Button
                type="primary"
                onClick={() => {
                  setExportConfig({
                    format: 'JSON',
                    dateRange: {
                      startDate: dayjs().subtract(90, 'day').toISOString(),
                      endDate: dayjs().toISOString()
                    },
                    includedMetrics: availableMetrics.map(m => m.key),
                    includeCharts: false,
                    includeRawData: true
                  })
                  handleExport()
                }}
                loading={isExporting}
                block
              >
                Export All Data
              </Button>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Export Configuration Modal */}
      <Modal
        title="Configure Export"
        open={exportModalVisible}
        onCancel={() => setExportModalVisible(false)}
        footer={null}
        width={800}
      >
        {exportProgress ? (
          <div style={{ textAlign: 'center', padding: '40px 0' }}>
            <Steps
              current={currentStep}
              size="small"
              style={{ marginBottom: '24px' }}
            >
              <Step title="Configure" icon={<SettingOutlined />} />
              <Step title="Prepare" icon={<DatabaseOutlined />} />
              <Step title="Process" icon={<BarChartOutlined />} />
              <Step title="Generate" icon={<FileExcelOutlined />} />
              <Step title="Complete" icon={<CheckCircleOutlined />} />
            </Steps>
            
            <div style={{ marginBottom: '24px' }}>
              {exportProgress.step < 5 ? (
                <LoadingOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
              ) : (
                <CheckCircleOutlined style={{ fontSize: '48px', color: '#52c41a' }} />
              )}
            </div>
            
            <Title level={4}>{exportProgress.message}</Title>
            <Progress 
              percent={exportProgress.progress} 
              status={exportProgress.step < 5 ? 'active' : 'success'}
              style={{ marginTop: '16px' }}
            />
          </div>
        ) : (
          <Form layout="vertical">
            {/* Format Selection */}
            <Form.Item label="Export Format">
              <Row gutter={16}>
                {Object.entries(formatConfig).map(([format, config]) => (
                  <Col span={8} key={format}>
                    <Card 
                      size="small"
                      style={{ 
                        cursor: 'pointer',
                        border: exportConfig.format === format ? '2px solid #1890ff' : '1px solid #d9d9d9'
                      }}
                      onClick={() => setExportConfig(prev => ({ ...prev, format: format as any }))}
                    >
                      <div style={{ textAlign: 'center' }}>
                        <div style={{ fontSize: '24px', marginBottom: '8px' }}>
                          {config.icon}
                        </div>
                        <Text strong>{format}</Text>
                        <div style={{ marginTop: '4px' }}>
                          <Tag size="small">{config.fileSize}</Tag>
                        </div>
                      </div>
                    </Card>
                  </Col>
                ))}
              </Row>
            </Form.Item>

            {/* Date Range */}
            <Form.Item label="Date Range">
              <RangePicker
                value={[
                  dayjs(exportConfig.dateRange?.startDate),
                  dayjs(exportConfig.dateRange?.endDate)
                ]}
                onChange={handleDateRangeChange}
                style={{ width: '100%' }}
              />
            </Form.Item>

            {/* Metrics Selection */}
            <Form.Item label="Included Metrics">
              <Checkbox.Group
                value={exportConfig.includedMetrics}
                onChange={handleMetricsChange}
                style={{ width: '100%' }}
              >
                <Row gutter={[16, 8]}>
                  {availableMetrics.map(metric => (
                    <Col span={12} key={metric.key}>
                      <Checkbox value={metric.key}>
                        <div>
                          <Text strong>{metric.label}</Text>
                          <div>
                            <Text type="secondary" style={{ fontSize: '11px' }}>
                              {metric.description}
                            </Text>
                          </div>
                        </div>
                      </Checkbox>
                    </Col>
                  ))}
                </Row>
              </Checkbox.Group>
            </Form.Item>

            {/* Additional Options */}
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item label="Intent Type Filter">
                  <Select
                    value={exportConfig.intentTypeFilter}
                    onChange={(value) => setExportConfig(prev => ({ ...prev, intentTypeFilter: value }))}
                    placeholder="All intent types"
                    allowClear
                  >
                    <Option value="sql_generation">SQL Generation</Option>
                    <Option value="insight_generation">Insight Generation</Option>
                    <Option value="explanation">Explanation</Option>
                    <Option value="data_analysis">Data Analysis</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="Options">
                  <Space direction="vertical">
                    <Checkbox
                      checked={exportConfig.includeCharts}
                      onChange={(e) => setExportConfig(prev => ({ ...prev, includeCharts: e.target.checked }))}
                    >
                      Include Charts
                    </Checkbox>
                    <Checkbox
                      checked={exportConfig.includeRawData}
                      onChange={(e) => setExportConfig(prev => ({ ...prev, includeRawData: e.target.checked }))}
                    >
                      Include Raw Data
                    </Checkbox>
                  </Space>
                </Form.Item>
              </Col>
            </Row>

            {/* Export Summary */}
            <Alert
              message="Export Summary"
              description={
                <div>
                  <Row gutter={16}>
                    <Col span={8}>
                      <Text strong>Estimated Size:</Text> {getEstimatedFileSize()}
                    </Col>
                    <Col span={8}>
                      <Text strong>Records:</Text> {getEstimatedRecords()}
                    </Col>
                    <Col span={8}>
                      <Text strong>Metrics:</Text> {exportConfig.includedMetrics?.length || 0}
                    </Col>
                  </Row>
                </div>
              }
              type="info"
              showIcon
              style={{ marginBottom: '16px' }}
            />

            {/* Action Buttons */}
            <div style={{ textAlign: 'right' }}>
              <Space>
                <Button onClick={() => setExportModalVisible(false)}>
                  Cancel
                </Button>
                <Button 
                  type="primary" 
                  onClick={handleExport}
                  loading={isExporting}
                  disabled={!exportConfig.includedMetrics || exportConfig.includedMetrics.length === 0}
                >
                  Start Export
                </Button>
              </Space>
            </div>
          </Form>
        )}
      </Modal>
    </div>
  )
}

export default AdvancedExportInterface
