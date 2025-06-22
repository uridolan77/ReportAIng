import React, { useState, useMemo } from 'react'
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  Typography,
  Select,
  DatePicker,
  Space,
  Button,
  Tag,
  Table,
  Tooltip,
  Badge,
} from 'antd'
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  RiseOutlined,
  DownloadOutlined,
  CalendarOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons'
import {
  useGetEnhancedBusinessMetadataStatisticsQuery,
  type BusinessMetadataStatistics,
} from '@shared/store/api/businessApi'
import type { ColumnsType } from 'antd/es/table'

const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { Option } = Select

interface MetadataAnalyticsDashboardProps {
  timeRange?: [string, string]
  selectedSchemas?: string[]
  selectedDomains?: string[]
}

export const MetadataAnalyticsDashboard: React.FC<MetadataAnalyticsDashboardProps> = ({
  timeRange,
  selectedSchemas = [],
  selectedDomains = [],
}) => {
  const [viewType, setViewType] = useState<'overview' | 'trends' | 'distribution'>('overview')
  const [selectedMetric, setSelectedMetric] = useState<'completeness' | 'quality' | 'usage'>('completeness')

  const {
    data: statistics,
    isLoading: statisticsLoading,
    error: statisticsError,
  } = useGetEnhancedBusinessMetadataStatisticsQuery()

  // Mock trend data - in real implementation, this would come from API
  const trendData = useMemo(() => {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun']
    return months.map((month, index) => ({
      month,
      completeness: 60 + index * 5 + Math.random() * 10,
      quality: 65 + index * 4 + Math.random() * 8,
      usage: 70 + index * 3 + Math.random() * 12,
    }))
  }, [])

  // Process statistics for visualization
  const processedStats = useMemo(() => {
    if (!statistics) return null

    const domainData = Object.entries(statistics.tablesByDomain).map(([domain, count]) => ({
      domain,
      count,
      percentage: (count / statistics.totalTables) * 100,
    }))

    const schemaData = Object.entries(statistics.tablesBySchema).map(([schema, count]) => ({
      schema,
      count,
      percentage: (count / statistics.totalTables) * 100,
    }))

    return {
      domainData,
      schemaData,
      completenessRate: (statistics.populatedTables / statistics.totalTables) * 100,
      aiEnhancementRate: (statistics.tablesWithAIMetadata / statistics.totalTables) * 100,
    }
  }, [statistics])

  const renderOverviewCards = () => (
    <Row gutter={16} style={{ marginBottom: 24 }}>
      <Col span={6}>
        <Card>
          <Statistic
            title="Total Tables"
            value={statistics?.totalTables || 0}
            prefix={<DatabaseOutlined />}
            loading={statisticsLoading}
          />
          <Progress
            percent={100}
            strokeColor="#1890ff"
            showInfo={false}
            size="small"
            style={{ marginTop: 8 }}
          />
        </Card>
      </Col>
      <Col span={6}>
        <Card>
          <Statistic
            title="Metadata Completeness"
            value={processedStats?.completenessRate || 0}
            precision={1}
            suffix="%"
            loading={statisticsLoading}
            valueStyle={{ 
              color: (processedStats?.completenessRate || 0) >= 80 ? '#3f8600' : '#faad14' 
            }}
          />
          <Progress
            percent={processedStats?.completenessRate || 0}
            strokeColor={(processedStats?.completenessRate || 0) >= 80 ? '#52c41a' : '#faad14'}
            showInfo={false}
            size="small"
            style={{ marginTop: 8 }}
          />
        </Card>
      </Col>
      <Col span={6}>
        <Card>
          <Statistic
            title="AI Enhanced"
            value={processedStats?.aiEnhancementRate || 0}
            precision={1}
            suffix="%"
            loading={statisticsLoading}
            prefix={<CheckCircleOutlined />}
          />
          <Progress
            percent={processedStats?.aiEnhancementRate || 0}
            strokeColor="#52c41a"
            showInfo={false}
            size="small"
            style={{ marginTop: 8 }}
          />
        </Card>
      </Col>
      <Col span={6}>
        <Card>
          <Statistic
            title="Average Quality"
            value={statistics?.averageMetadataCompleteness || 0}
            precision={1}
            suffix="/100"
            loading={statisticsLoading}
            prefix={<RiseOutlined />}
          />
          <Progress
            percent={statistics?.averageMetadataCompleteness || 0}
            strokeColor="#722ed1"
            showInfo={false}
            size="small"
            style={{ marginTop: 8 }}
          />
        </Card>
      </Col>
    </Row>
  )

  const renderDomainDistribution = () => {
    const columns: ColumnsType<any> = [
      {
        title: 'Domain',
        dataIndex: 'domain',
        key: 'domain',
        render: (domain: string) => <Tag color="blue">{domain}</Tag>,
      },
      {
        title: 'Tables',
        dataIndex: 'count',
        key: 'count',
        align: 'right',
      },
      {
        title: 'Percentage',
        dataIndex: 'percentage',
        key: 'percentage',
        align: 'right',
        render: (percentage: number) => (
          <Space>
            <Text>{percentage.toFixed(1)}%</Text>
            <Progress
              percent={percentage}
              size="small"
              showInfo={false}
              style={{ width: 60 }}
            />
          </Space>
        ),
      },
    ]

    return (
      <Card title="Distribution by Domain" size="small">
        <Table
          columns={columns}
          dataSource={processedStats?.domainData || []}
          pagination={false}
          size="small"
          loading={statisticsLoading}
        />
      </Card>
    )
  }

  const renderSchemaDistribution = () => {
    const columns: ColumnsType<any> = [
      {
        title: 'Schema',
        dataIndex: 'schema',
        key: 'schema',
        render: (schema: string) => <Tag color="green">{schema}</Tag>,
      },
      {
        title: 'Tables',
        dataIndex: 'count',
        key: 'count',
        align: 'right',
      },
      {
        title: 'Percentage',
        dataIndex: 'percentage',
        key: 'percentage',
        align: 'right',
        render: (percentage: number) => (
          <Space>
            <Text>{percentage.toFixed(1)}%</Text>
            <Progress
              percent={percentage}
              size="small"
              showInfo={false}
              style={{ width: 60 }}
            />
          </Space>
        ),
      },
    ]

    return (
      <Card title="Distribution by Schema" size="small">
        <Table
          columns={columns}
          dataSource={processedStats?.schemaData || []}
          pagination={false}
          size="small"
          loading={statisticsLoading}
        />
      </Card>
    )
  }

  const renderTrendChart = () => (
    <Card title="Metadata Quality Trends" size="small">
      <div style={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Text type="secondary">
          Interactive trend charts would be implemented here using a charting library like Chart.js or D3
        </Text>
      </div>
      <Row gutter={16} style={{ marginTop: 16 }}>
        {trendData.map((data, index) => (
          <Col key={index} span={4}>
            <div style={{ textAlign: 'center' }}>
              <Text strong>{data.month}</Text>
              <div>
                <Progress
                  type="circle"
                  percent={data[selectedMetric]}
                  size={40}
                  format={(percent) => `${Math.round(percent || 0)}`}
                />
              </div>
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  )

  const renderActiveUsers = () => (
    <Card title="Most Active Users" size="small">
      <Space direction="vertical" style={{ width: '100%' }}>
        {statistics?.mostActiveUsers?.map((user, index) => (
          <div key={user} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Space>
              <Badge count={index + 1} style={{ backgroundColor: '#52c41a' }} />
              <Text>{user}</Text>
            </Space>
            <Progress
              percent={Math.random() * 40 + 60}
              size="small"
              showInfo={false}
              style={{ width: 100 }}
            />
          </div>
        )) || []}
      </Space>
    </Card>
  )

  const handleExport = () => {
    // Implementation for exporting analytics data
    console.log('Exporting analytics data...')
  }

  return (
    <div>
      {/* Controls */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Select
                value={viewType}
                onChange={setViewType}
                style={{ width: 120 }}
              >
                <Option value="overview">Overview</Option>
                <Option value="trends">Trends</Option>
                <Option value="distribution">Distribution</Option>
              </Select>
              <Select
                value={selectedMetric}
                onChange={setSelectedMetric}
                style={{ width: 120 }}
              >
                <Option value="completeness">Completeness</Option>
                <Option value="quality">Quality</Option>
                <Option value="usage">Usage</Option>
              </Select>
              <RangePicker />
            </Space>
          </Col>
          <Col>
            <Space>
              <Button icon={<DownloadOutlined />} onClick={handleExport}>
                Export
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Overview Cards */}
      {renderOverviewCards()}

      {/* Main Content */}
      {viewType === 'overview' && (
        <Row gutter={16}>
          <Col span={8}>
            {renderDomainDistribution()}
          </Col>
          <Col span={8}>
            {renderSchemaDistribution()}
          </Col>
          <Col span={8}>
            {renderActiveUsers()}
          </Col>
        </Row>
      )}

      {viewType === 'trends' && (
        <Row gutter={16}>
          <Col span={24}>
            {renderTrendChart()}
          </Col>
        </Row>
      )}

      {viewType === 'distribution' && (
        <Row gutter={16}>
          <Col span={12}>
            {renderDomainDistribution()}
          </Col>
          <Col span={12}>
            {renderSchemaDistribution()}
          </Col>
        </Row>
      )}

      {/* Summary Stats */}
      <Card title="Summary Statistics" style={{ marginTop: 16 }}>
        <Row gutter={16}>
          <Col span={6}>
            <Statistic
              title="Total Columns"
              value={statistics?.totalColumns || 0}
              loading={statisticsLoading}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Populated Columns"
              value={statistics?.populatedColumns || 0}
              loading={statisticsLoading}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Glossary Terms"
              value={statistics?.totalGlossaryTerms || 0}
              loading={statisticsLoading}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Last Update"
              value={statistics?.lastPopulationRun ? new Date(statistics.lastPopulationRun).toLocaleDateString() : 'Never'}
              loading={statisticsLoading}
            />
          </Col>
        </Row>
      </Card>
    </div>
  )
}

export default MetadataAnalyticsDashboard
