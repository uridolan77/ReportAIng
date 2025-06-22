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
  Tabs,
  List,
} from 'antd'
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  TrendingUpOutlined,
  DownloadOutlined,
  CalendarOutlined,
  BookOutlined,
  ApartmentOutlined,
  LinkOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons'
import {
  useGetEnhancedBusinessDomainsQuery,
  useGetEnhancedBusinessGlossaryQuery,
  useGetEnhancedBusinessTablesQuery,
  useGetEnhancedBusinessMetadataStatisticsQuery,
} from '@shared/store/api/businessApi'
import type { ColumnsType } from 'antd/es/table'

const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { Option } = Select
const { TabPane } = Tabs

interface DomainAnalytics {
  domainName: string
  totalTables: number
  totalTerms: number
  mappingCoverage: number
  qualityScore: number
  activeUsers: number
  lastUpdated: string
}

interface GlossaryAnalytics {
  category: string
  totalTerms: number
  mappedTerms: number
  usageFrequency: number
  qualityScore: number
  completeness: number
}

export const DomainGlossaryAnalytics: React.FC = () => {
  const [viewType, setViewType] = useState<'overview' | 'domains' | 'glossary' | 'integration'>('overview')
  const [selectedTimeRange, setSelectedTimeRange] = useState<'week' | 'month' | 'quarter'>('month')

  const { data: domainsData, isLoading: domainsLoading } = useGetEnhancedBusinessDomainsQuery({
    page: 1,
    pageSize: 100,
    includeSubDomains: true,
  })

  const { data: glossaryData, isLoading: glossaryLoading } = useGetEnhancedBusinessGlossaryQuery({
    page: 1,
    pageSize: 100,
    includeRelationships: true,
  })

  const { data: tablesData, isLoading: tablesLoading } = useGetEnhancedBusinessTablesQuery({
    page: 1,
    pageSize: 100,
  })

  const { data: statistics, isLoading: statisticsLoading } = useGetEnhancedBusinessMetadataStatisticsQuery()

  // Generate analytics data
  const domainAnalytics: DomainAnalytics[] = useMemo(() => {
    if (!domainsData?.data || !tablesData?.data || !glossaryData?.data) return []

    return domainsData.data.map(domain => {
      const domainTables = tablesData.data.filter(table => 
        table.domainClassification === domain.name
      )
      const domainTerms = glossaryData.data.filter(term => 
        term.domain === domain.name
      )

      return {
        domainName: domain.name,
        totalTables: domainTables.length,
        totalTerms: domainTerms.length,
        mappingCoverage: domainTables.length > 0 
          ? (domainTables.filter(table => table.businessGlossaryTerms?.length > 0).length / domainTables.length) * 100
          : 0,
        qualityScore: domainTerms.length > 0
          ? domainTerms.reduce((sum, term) => sum + (term.qualityScore || 70), 0) / domainTerms.length
          : 0,
        activeUsers: Math.floor(Math.random() * 10) + 1,
        lastUpdated: domain.updatedDate,
      }
    })
  }, [domainsData, tablesData, glossaryData])

  const glossaryAnalytics: GlossaryAnalytics[] = useMemo(() => {
    if (!glossaryData?.data) return []

    const categories = [...new Set(glossaryData.data.map(term => term.category))]
    
    return categories.map(category => {
      const categoryTerms = glossaryData.data.filter(term => term.category === category)
      const mappedTerms = categoryTerms.filter(term => term.mappedTables?.length > 0)

      return {
        category,
        totalTerms: categoryTerms.length,
        mappedTerms: mappedTerms.length,
        usageFrequency: categoryTerms.reduce((sum, term) => sum + (term.usageFrequency || 0), 0) / categoryTerms.length,
        qualityScore: categoryTerms.reduce((sum, term) => sum + (term.qualityScore || 70), 0) / categoryTerms.length,
        completeness: categoryTerms.length > 0 ? (mappedTerms.length / categoryTerms.length) * 100 : 0,
      }
    })
  }, [glossaryData])

  const domainColumns: ColumnsType<DomainAnalytics> = [
    {
      title: 'Domain',
      dataIndex: 'domainName',
      key: 'domainName',
      render: (name: string) => <Text strong>{name}</Text>,
    },
    {
      title: 'Tables',
      dataIndex: 'totalTables',
      key: 'totalTables',
      align: 'center',
      render: (count: number) => <Badge count={count} style={{ backgroundColor: '#52c41a' }} />,
    },
    {
      title: 'Terms',
      dataIndex: 'totalTerms',
      key: 'totalTerms',
      align: 'center',
      render: (count: number) => <Badge count={count} style={{ backgroundColor: '#1890ff' }} />,
    },
    {
      title: 'Mapping Coverage',
      dataIndex: 'mappingCoverage',
      key: 'mappingCoverage',
      render: (coverage: number) => (
        <Space>
          <Text style={{ color: coverage >= 80 ? '#52c41a' : coverage >= 60 ? '#faad14' : '#ff4d4f' }}>
            {coverage.toFixed(1)}%
          </Text>
          <Progress
            percent={coverage}
            size="small"
            showInfo={false}
            strokeColor={coverage >= 80 ? '#52c41a' : coverage >= 60 ? '#faad14' : '#ff4d4f'}
            style={{ width: 60 }}
          />
        </Space>
      ),
      sorter: (a, b) => a.mappingCoverage - b.mappingCoverage,
    },
    {
      title: 'Quality Score',
      dataIndex: 'qualityScore',
      key: 'qualityScore',
      render: (score: number) => (
        <Space>
          <Text style={{ color: score >= 80 ? '#52c41a' : score >= 60 ? '#faad14' : '#ff4d4f' }}>
            {score.toFixed(1)}
          </Text>
          <Progress
            percent={score}
            size="small"
            showInfo={false}
            strokeColor={score >= 80 ? '#52c41a' : score >= 60 ? '#faad14' : '#ff4d4f'}
            style={{ width: 60 }}
          />
        </Space>
      ),
      sorter: (a, b) => a.qualityScore - b.qualityScore,
    },
    {
      title: 'Active Users',
      dataIndex: 'activeUsers',
      key: 'activeUsers',
      align: 'center',
    },
  ]

  const glossaryColumns: ColumnsType<GlossaryAnalytics> = [
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      render: (category: string) => <Tag color="blue">{category}</Tag>,
    },
    {
      title: 'Total Terms',
      dataIndex: 'totalTerms',
      key: 'totalTerms',
      align: 'center',
    },
    {
      title: 'Mapped Terms',
      dataIndex: 'mappedTerms',
      key: 'mappedTerms',
      align: 'center',
      render: (mapped: number, record) => (
        <Space>
          <Text>{mapped}</Text>
          <Text type="secondary">/ {record.totalTerms}</Text>
        </Space>
      ),
    },
    {
      title: 'Completeness',
      dataIndex: 'completeness',
      key: 'completeness',
      render: (completeness: number) => (
        <Space>
          <Text style={{ color: completeness >= 80 ? '#52c41a' : completeness >= 60 ? '#faad14' : '#ff4d4f' }}>
            {completeness.toFixed(1)}%
          </Text>
          <Progress
            percent={completeness}
            size="small"
            showInfo={false}
            strokeColor={completeness >= 80 ? '#52c41a' : completeness >= 60 ? '#faad14' : '#ff4d4f'}
            style={{ width: 60 }}
          />
        </Space>
      ),
      sorter: (a, b) => a.completeness - b.completeness,
    },
    {
      title: 'Usage Frequency',
      dataIndex: 'usageFrequency',
      key: 'usageFrequency',
      render: (frequency: number) => (
        <Badge count={Math.round(frequency)} style={{ backgroundColor: '#722ed1' }} />
      ),
      sorter: (a, b) => a.usageFrequency - b.usageFrequency,
    },
    {
      title: 'Quality Score',
      dataIndex: 'qualityScore',
      key: 'qualityScore',
      render: (score: number) => (
        <Space>
          <Text style={{ color: score >= 80 ? '#52c41a' : score >= 60 ? '#faad14' : '#ff4d4f' }}>
            {score.toFixed(1)}
          </Text>
          <Progress
            percent={score}
            size="small"
            showInfo={false}
            strokeColor={score >= 80 ? '#52c41a' : score >= 60 ? '#faad14' : '#ff4d4f'}
            style={{ width: 60 }}
          />
        </Space>
      ),
      sorter: (a, b) => a.qualityScore - b.qualityScore,
    },
  ]

  const renderOverviewCards = () => (
    <Row gutter={16} style={{ marginBottom: 24 }}>
      <Col span={6}>
        <Card>
          <Statistic
            title="Total Domains"
            value={domainsData?.data?.length || 0}
            prefix={<ApartmentOutlined />}
            loading={domainsLoading}
          />
        </Card>
      </Col>
      <Col span={6}>
        <Card>
          <Statistic
            title="Total Glossary Terms"
            value={glossaryData?.data?.length || 0}
            prefix={<BookOutlined />}
            loading={glossaryLoading}
          />
        </Card>
      </Col>
      <Col span={6}>
        <Card>
          <Statistic
            title="Mapped Tables"
            value={tablesData?.data?.filter(table => table.businessGlossaryTerms?.length > 0).length || 0}
            suffix={`/ ${tablesData?.data?.length || 0}`}
            prefix={<LinkOutlined />}
            loading={tablesLoading}
          />
        </Card>
      </Col>
      <Col span={6}>
        <Card>
          <Statistic
            title="Average Quality"
            value={
              glossaryData?.data?.length > 0
                ? (glossaryData.data.reduce((sum, term) => sum + (term.qualityScore || 70), 0) / glossaryData.data.length).toFixed(1)
                : 0
            }
            suffix="/100"
            prefix={<TrendingUpOutlined />}
            loading={glossaryLoading}
          />
        </Card>
      </Col>
    </Row>
  )

  const renderIntegrationMetrics = () => (
    <Row gutter={16}>
      <Col span={12}>
        <Card title="Domain Coverage" size="small">
          <List
            size="small"
            dataSource={domainAnalytics.slice(0, 5)}
            renderItem={(domain) => (
              <List.Item>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                  <Text>{domain.domainName}</Text>
                  <Space>
                    <Text>{domain.mappingCoverage.toFixed(1)}%</Text>
                    <Progress
                      percent={domain.mappingCoverage}
                      size="small"
                      showInfo={false}
                      style={{ width: 80 }}
                    />
                  </Space>
                </Space>
              </List.Item>
            )}
          />
        </Card>
      </Col>
      <Col span={12}>
        <Card title="Category Completeness" size="small">
          <List
            size="small"
            dataSource={glossaryAnalytics.slice(0, 5)}
            renderItem={(category) => (
              <List.Item>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                  <Tag color="blue">{category.category}</Tag>
                  <Space>
                    <Text>{category.completeness.toFixed(1)}%</Text>
                    <Progress
                      percent={category.completeness}
                      size="small"
                      showInfo={false}
                      style={{ width: 80 }}
                    />
                  </Space>
                </Space>
              </List.Item>
            )}
          />
        </Card>
      </Col>
    </Row>
  )

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
                style={{ width: 150 }}
              >
                <Option value="overview">Overview</Option>
                <Option value="domains">Domains</Option>
                <Option value="glossary">Glossary</Option>
                <Option value="integration">Integration</Option>
              </Select>
              <Select
                value={selectedTimeRange}
                onChange={setSelectedTimeRange}
                style={{ width: 120 }}
              >
                <Option value="week">Last Week</Option>
                <Option value="month">Last Month</Option>
                <Option value="quarter">Last Quarter</Option>
              </Select>
              <RangePicker />
            </Space>
          </Col>
          <Col>
            <Space>
              <Button icon={<DownloadOutlined />}>
                Export Report
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Overview Cards */}
      {renderOverviewCards()}

      {/* Main Content */}
      {viewType === 'overview' && renderIntegrationMetrics()}

      {viewType === 'domains' && (
        <Card title="Domain Analytics">
          <Table
            columns={domainColumns}
            dataSource={domainAnalytics}
            rowKey="domainName"
            loading={domainsLoading}
            pagination={{ pageSize: 10 }}
            size="small"
          />
        </Card>
      )}

      {viewType === 'glossary' && (
        <Card title="Glossary Analytics">
          <Table
            columns={glossaryColumns}
            dataSource={glossaryAnalytics}
            rowKey="category"
            loading={glossaryLoading}
            pagination={{ pageSize: 10 }}
            size="small"
          />
        </Card>
      )}

      {viewType === 'integration' && (
        <Card title="Integration Analytics">
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Text type="secondary">
              Advanced integration analytics charts would be implemented here
            </Text>
          </div>
        </Card>
      )}
    </div>
  )
}

export default DomainGlossaryAnalytics
