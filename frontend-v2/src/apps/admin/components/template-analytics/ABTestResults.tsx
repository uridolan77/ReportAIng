import React, { useState } from 'react'
import { 
  Card, 
  Table, 
  Tag, 
  Button, 
  Space, 
  Typography, 
  Row, 
  Col,
  Statistic,
  Select,
  DatePicker,
  Tooltip
} from 'antd'
import { 
  TrophyOutlined, 
  EyeOutlined, 
  DownloadOutlined,
  FilterOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { useGetABTestsQuery } from '@shared/store/api/templateAnalyticsApi'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

interface ABTestResultsProps {
  onViewDetails?: (test: any) => void
}

export const ABTestResults: React.FC<ABTestResultsProps> = ({ onViewDetails }) => {
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs] | null>(null)

  const { data: allTests, isLoading } = useGetABTestsQuery({})

  const getWinnerTag = (test: any) => {
    if (test.status !== 'completed') return null
    
    const originalRate = test.originalSuccessRate
    const variantRate = test.variantSuccessRate
    const isSignificant = test.statisticalSignificance >= 0.95
    
    if (!isSignificant) {
      return (
        <Tag color="orange" icon={<InfoCircleOutlined />}>
          Inconclusive
        </Tag>
      )
    }
    
    if (variantRate > originalRate) {
      return (
        <Tag color="green" icon={<CheckCircleOutlined />}>
          Variant Won
        </Tag>
      )
    } else {
      return (
        <Tag color="blue" icon={<CloseCircleOutlined />}>
          Original Won
        </Tag>
      )
    }
  }

  const getImprovementPercentage = (test: any) => {
    const originalRate = test.originalSuccessRate
    const variantRate = test.variantSuccessRate
    const improvement = ((variantRate - originalRate) / originalRate * 100)
    
    return (
      <Text 
        style={{ 
          color: improvement > 0 ? '#52c41a' : improvement < 0 ? '#f5222d' : '#666',
          fontWeight: 600
        }}
      >
        {improvement > 0 ? '+' : ''}{improvement.toFixed(1)}%
      </Text>
    )
  }

  const columns = [
    {
      title: 'Test Name',
      dataIndex: 'testName',
      key: 'testName',
      render: (text: string, record: any) => (
        <div>
          <div style={{ fontWeight: 600, marginBottom: '4px' }}>{text}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.originalTemplateKey}
          </Text>
        </div>
      )
    },
    {
      title: 'Duration',
      key: 'duration',
      render: (record: any) => {
        const start = dayjs(record.startDate)
        const end = record.endDate ? dayjs(record.endDate) : dayjs()
        const duration = end.diff(start, 'day')
        
        return (
          <div>
            <div>{duration} days</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {start.format('MMM DD')} - {end.format('MMM DD')}
            </Text>
          </div>
        )
      }
    },
    {
      title: 'Sample Size',
      key: 'sampleSize',
      render: (record: any) => {
        const total = record.originalUsageCount + record.variantUsageCount
        return (
          <div>
            <div style={{ fontWeight: 600 }}>{total.toLocaleString()}</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.originalUsageCount} / {record.variantUsageCount}
            </Text>
          </div>
        )
      }
    },
    {
      title: 'Performance',
      key: 'performance',
      render: (record: any) => (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
            <Text style={{ fontSize: '12px' }}>Original:</Text>
            <Text style={{ fontSize: '12px', fontWeight: 600 }}>
              {(record.originalSuccessRate * 100).toFixed(1)}%
            </Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
            <Text style={{ fontSize: '12px' }}>Variant:</Text>
            <Text style={{ fontSize: '12px', fontWeight: 600 }}>
              {(record.variantSuccessRate * 100).toFixed(1)}%
            </Text>
          </div>
          <div style={{ textAlign: 'center' }}>
            {getImprovementPercentage(record)}
          </div>
        </div>
      )
    },
    {
      title: 'Statistical Significance',
      key: 'significance',
      render: (record: any) => {
        const significance = record.statisticalSignificance
        const isSignificant = significance >= 0.95
        
        return (
          <div style={{ textAlign: 'center' }}>
            <div style={{ 
              fontSize: '16px', 
              fontWeight: 600,
              color: isSignificant ? '#52c41a' : '#fa8c16'
            }}>
              {(significance * 100).toFixed(1)}%
            </div>
            <Tag 
              color={isSignificant ? 'green' : 'orange'}
              size="small"
            >
              {isSignificant ? 'Significant' : 'Not Significant'}
            </Tag>
          </div>
        )
      }
    },
    {
      title: 'Result',
      key: 'result',
      render: (record: any) => (
        <div style={{ textAlign: 'center' }}>
          {getWinnerTag(record)}
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: any) => (
        <Space>
          <Button 
            size="small" 
            icon={<EyeOutlined />}
            onClick={() => onViewDetails?.(record)}
          >
            Details
          </Button>
          <Button 
            size="small" 
            icon={<DownloadOutlined />}
          >
            Export
          </Button>
        </Space>
      )
    }
  ]

  const getFilteredTests = () => {
    if (!allTests) return []
    
    let filtered = allTests
    
    if (statusFilter !== 'all') {
      filtered = filtered.filter(test => test.status === statusFilter)
    }
    
    if (dateRange) {
      filtered = filtered.filter(test => {
        const testStart = dayjs(test.startDate)
        return testStart.isAfter(dateRange[0]) && testStart.isBefore(dateRange[1])
      })
    }
    
    return filtered
  }

  const getResultsSummary = () => {
    const tests = getFilteredTests()
    const completedTests = tests.filter(test => test.status === 'completed')
    const significantTests = completedTests.filter(test => test.statisticalSignificance >= 0.95)
    const variantWins = significantTests.filter(test => 
      test.variantSuccessRate > test.originalSuccessRate
    )
    
    return {
      total: tests.length,
      completed: completedTests.length,
      significant: significantTests.length,
      variantWins: variantWins.length,
      winRate: significantTests.length > 0 ? (variantWins.length / significantTests.length * 100) : 0
    }
  }

  const summary = getResultsSummary()

  return (
    <div>
      {/* Summary Cards */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Total Tests"
              value={summary.total}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Completed"
              value={summary.completed}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Significant Results"
              value={summary.significant}
              prefix={<InfoCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Variant Win Rate"
              value={summary.winRate.toFixed(1)}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: summary.winRate > 50 ? '#52c41a' : '#fa8c16' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Filters */}
      <Card style={{ marginBottom: '16px' }}>
        <Row gutter={16} align="middle">
          <Col span={6}>
            <Space>
              <FilterOutlined />
              <Text strong>Filters:</Text>
            </Space>
          </Col>
          <Col span={6}>
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ width: '100%' }}
              placeholder="Filter by status"
            >
              <Select.Option value="all">All Tests</Select.Option>
              <Select.Option value="running">Running</Select.Option>
              <Select.Option value="completed">Completed</Select.Option>
              <Select.Option value="paused">Paused</Select.Option>
              <Select.Option value="cancelled">Cancelled</Select.Option>
            </Select>
          </Col>
          <Col span={8}>
            <RangePicker
              value={dateRange}
              onChange={setDateRange}
              style={{ width: '100%' }}
              placeholder={['Start Date', 'End Date']}
            />
          </Col>
          <Col span={4}>
            <Button 
              onClick={() => {
                setStatusFilter('all')
                setDateRange(null)
              }}
            >
              Clear Filters
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Results Table */}
      <Card title="Test Results">
        <Table
          columns={columns}
          dataSource={getFilteredTests()}
          loading={isLoading}
          rowKey="id"
          pagination={{ 
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => 
              `${range[0]}-${range[1]} of ${total} tests`
          }}
        />
      </Card>
    </div>
  )
}

export default ABTestResults
