import React, { useState } from 'react'
import { 
  Tabs, 
  Card, 
  Row, 
  Col, 
  Button, 
  Space, 
  Typography, 
  Alert,
  Table,
  Tag,
  Progress,
  Statistic,
  Select,
  message
} from 'antd'
import { 
  DashboardOutlined, 
  ThunderboltOutlined, 
  WarningOutlined,
  CheckCircleOutlined,
  TrophyOutlined,
  ToolOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { 
  usePerformanceMonitoring, 
  usePerformanceAlerts, 
  usePerformanceBenchmarks 
} from '@shared/hooks/usePerformanceMonitoring'
import type { PerformanceEntityType } from '@shared/types/performance'

const { Title, Text } = Typography

const PerformanceOverviewTab: React.FC = () => {
  const [selectedEntity, setSelectedEntity] = useState<{type: PerformanceEntityType, id: string}>({
    type: 'system',
    id: 'global'
  })

  const { 
    metrics, 
    bottlenecks, 
    suggestions, 
    autoTune, 
    autoTuneLoading,
    isLoading 
  } = usePerformanceMonitoring(selectedEntity.type, selectedEntity.id)

  const handleAutoTune = async () => {
    try {
      await autoTune()
      message.success('Auto-tuning completed successfully')
    } catch (error) {
      message.error('Auto-tuning failed')
    }
  }

  const getScoreColor = (score: number) => {
    if (score >= 80) return '#52c41a'
    if (score >= 60) return '#faad14'
    return '#f5222d'
  }

  return (
    <div>
      {/* Entity Selection */}
      <Card style={{ marginBottom: '16px' }}>
        <Space>
          <Text>Monitor Entity:</Text>
          <Select
            style={{ width: 200 }}
            value={`${selectedEntity.type}:${selectedEntity.id}`}
            onChange={(value) => {
              const [type, id] = value.split(':')
              setSelectedEntity({ type: type as PerformanceEntityType, id })
            }}
          >
            <Select.Option value="system:global">System (Global)</Select.Option>
            <Select.Option value="database:main">Database (Main)</Select.Option>
            <Select.Option value="api:v1">API (v1)</Select.Option>
          </Select>
          <Button 
            type="primary"
            icon={<ToolOutlined />}
            loading={autoTuneLoading}
            onClick={handleAutoTune}
          >
            Auto-Tune Performance
          </Button>
        </Space>
      </Card>

      {/* Performance Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Performance Score"
              value={metrics?.metrics.performanceScore || 0}
              suffix="/100"
              valueStyle={{ color: getScoreColor(metrics?.metrics.performanceScore || 0) }}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={metrics?.metrics.averageResponseTime || 0}
              suffix="ms"
              precision={2}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Throughput"
              value={metrics?.metrics.throughputPerSecond || 0}
              suffix="req/s"
              precision={1}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Error Rate"
              value={(metrics?.metrics.errorRate || 0) * 100}
              suffix="%"
              precision={2}
              valueStyle={{ 
                color: (metrics?.metrics.errorRate || 0) > 0.05 ? '#f5222d' : '#52c41a' 
              }}
            />
          </Card>
        </Col>
      </Row>

      {/* Bottlenecks */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Performance Bottlenecks" loading={isLoading}>
            {bottlenecks && bottlenecks.bottlenecks.length > 0 ? (
              <div>
                {bottlenecks.bottlenecks.slice(0, 5).map((bottleneck) => (
                  <Card key={bottleneck.id} size="small" style={{ marginBottom: '8px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <div style={{ flex: 1 }}>
                        <div style={{ fontWeight: 'bold', marginBottom: '4px' }}>
                          {bottleneck.description}
                        </div>
                        <Space size="small">
                          <Tag color={
                            bottleneck.severity === 'Critical' ? 'red' :
                            bottleneck.severity === 'High' ? 'orange' :
                            bottleneck.severity === 'Medium' ? 'blue' : 'green'
                          }>
                            {bottleneck.severity}
                          </Tag>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {bottleneck.type}
                          </Text>
                        </Space>
                      </div>
                      <div style={{ textAlign: 'right' }}>
                        <div style={{ fontWeight: 'bold' }}>
                          {bottleneck.impactScore}/10
                        </div>
                        <div style={{ fontSize: '11px', color: '#666' }}>
                          Impact Score
                        </div>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            ) : (
              <Text type="secondary">No bottlenecks detected</Text>
            )}
          </Card>
        </Col>

        {/* Optimization Suggestions */}
        <Col xs={24} lg={12}>
          <Card title="Optimization Suggestions" loading={isLoading}>
            {suggestions && suggestions.suggestions.length > 0 ? (
              <div>
                {suggestions.suggestions.slice(0, 5).map((suggestion) => (
                  <Card key={suggestion.id} size="small" style={{ marginBottom: '8px' }}>
                    <div style={{ marginBottom: '8px' }}>
                      <Text strong>{suggestion.title}</Text>
                    </div>
                    <div style={{ marginBottom: '8px', fontSize: '12px', color: '#666' }}>
                      {suggestion.description}
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Space size="small">
                        <Tag color={
                          suggestion.impact === 'High' ? 'red' :
                          suggestion.impact === 'Medium' ? 'orange' : 'green'
                        }>
                          {suggestion.impact} Impact
                        </Tag>
                        <Tag color={
                          suggestion.effort === 'Low' ? 'green' :
                          suggestion.effort === 'Medium' ? 'blue' : 'purple'
                        }>
                          {suggestion.effort} Effort
                        </Tag>
                      </Space>
                      <Text type="success" style={{ fontSize: '12px' }}>
                        +{suggestion.estimatedImprovement}% improvement
                      </Text>
                    </div>
                  </Card>
                ))}
              </div>
            ) : (
              <Text type="secondary">No optimization suggestions available</Text>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  )
}

const AlertsTab: React.FC = () => {
  const { alerts, criticalAlerts, highAlerts, totalCount } = usePerformanceAlerts()

  const columns = [
    {
      title: 'Alert',
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: any) => (
        <div>
          <div style={{ fontWeight: 'bold' }}>{text}</div>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {record.description}
          </div>
        </div>
      )
    },
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      render: (severity: string) => (
        <Tag color={
          severity === 'Critical' ? 'red' :
          severity === 'High' ? 'orange' :
          severity === 'Medium' ? 'blue' : 'green'
        }>
          {severity}
        </Tag>
      )
    },
    {
      title: 'Entity',
      key: 'entity',
      render: (record: any) => (
        <div>
          <div>{record.entityType}</div>
          <div style={{ fontSize: '11px', color: '#666' }}>{record.entityId}</div>
        </div>
      )
    },
    {
      title: 'Threshold',
      key: 'threshold',
      render: (record: any) => (
        <div>
          <div>Threshold: {record.threshold}</div>
          <div style={{ color: '#f5222d' }}>Current: {record.currentValue}</div>
        </div>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={
          status === 'active' ? 'red' :
          status === 'acknowledged' ? 'orange' : 'green'
        }>
          {status}
        </Tag>
      )
    }
  ]

  return (
    <div>
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Alerts"
              value={totalCount}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Critical Alerts"
              value={criticalAlerts.length}
              valueStyle={{ color: '#f5222d' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="High Priority"
              value={highAlerts.length}
              valueStyle={{ color: '#faad14' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card title="Active Performance Alerts">
        <Table
          columns={columns}
          dataSource={alerts}
          rowKey="id"
          pagination={{
            pageSize: 10,
            showSizeChanger: true
          }}
        />
      </Card>
    </div>
  )
}

const BenchmarksTab: React.FC = () => {
  const { benchmarks, overallScore, passingCount, warningCount, failingCount } = usePerformanceBenchmarks()

  return (
    <div>
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Overall Score"
              value={overallScore}
              suffix="/100"
              precision={1}
              valueStyle={{ color: overallScore >= 80 ? '#52c41a' : overallScore >= 60 ? '#faad14' : '#f5222d' }}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Passing"
              value={passingCount}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Warning"
              value={warningCount}
              valueStyle={{ color: '#faad14' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Failing"
              value={failingCount}
              valueStyle={{ color: '#f5222d' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card title="Performance Benchmarks">
        <Row gutter={[16, 16]}>
          {benchmarks.map((benchmark) => (
            <Col key={benchmark.id} xs={24} sm={12} lg={8}>
              <Card size="small">
                <div style={{ marginBottom: '8px' }}>
                  <Text strong>{benchmark.name}</Text>
                  <Tag 
                    color={
                      benchmark.status === 'passing' ? 'green' :
                      benchmark.status === 'warning' ? 'orange' : 'red'
                    }
                    style={{ float: 'right' }}
                  >
                    {benchmark.status}
                  </Tag>
                </div>
                <div style={{ marginBottom: '8px', fontSize: '12px', color: '#666' }}>
                  {benchmark.description}
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span>Target: {benchmark.targetValue}</span>
                  <span>Current: {benchmark.currentValue}</span>
                </div>
                <Progress
                  percent={(benchmark.currentValue / benchmark.targetValue) * 100}
                  size="small"
                  status={
                    benchmark.status === 'passing' ? 'success' :
                    benchmark.status === 'warning' ? 'active' : 'exception'
                  }
                  style={{ marginTop: '8px' }}
                />
              </Card>
            </Col>
          ))}
        </Row>
      </Card>
    </div>
  )
}

export default function PerformanceMonitoring() {
  const { criticalAlerts, highAlerts } = usePerformanceAlerts()

  const tabItems = [
    {
      key: 'overview',
      label: (
        <span>
          <DashboardOutlined />
          Overview
        </span>
      ),
      children: <PerformanceOverviewTab />
    },
    {
      key: 'alerts',
      label: (
        <span>
          <WarningOutlined />
          Alerts
          {(criticalAlerts.length + highAlerts.length) > 0 && (
            <span style={{ 
              marginLeft: '4px', 
              backgroundColor: '#f5222d', 
              color: 'white', 
              borderRadius: '10px', 
              padding: '2px 6px', 
              fontSize: '11px' 
            }}>
              {criticalAlerts.length + highAlerts.length}
            </span>
          )}
        </span>
      ),
      children: <AlertsTab />
    },
    {
      key: 'benchmarks',
      label: (
        <span>
          <TrophyOutlined />
          Benchmarks
        </span>
      ),
      children: <BenchmarksTab />
    }
  ]

  return (
    <PageLayout
      title="Performance Monitoring"
      subtitle="Monitor and optimize system performance"
      extra={
        <Space>
          {(criticalAlerts.length > 0 || highAlerts.length > 0) && (
            <Alert
              message={`${criticalAlerts.length + highAlerts.length} Active Alert${criticalAlerts.length + highAlerts.length > 1 ? 's' : ''}`}
              type={criticalAlerts.length > 0 ? 'error' : 'warning'}
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
        </Space>
      }
    >
      <Tabs
        defaultActiveKey="overview"
        items={tabItems}
        size="large"
        style={{ marginTop: '16px' }}
      />
    </PageLayout>
  )
}
