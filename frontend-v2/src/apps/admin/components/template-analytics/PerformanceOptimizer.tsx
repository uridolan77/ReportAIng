import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Button, 
  Progress, 
  Alert, 
  Table, 
  Tag, 
  Space,
  Tooltip,
  Modal,
  Form,
  Input,
  Select,
  Slider,
  Switch,
  Statistic,
  List,
  Avatar,
  Badge,
  message
} from 'antd'
import {
  RocketOutlined,
  ThunderboltOutlined,
  SettingOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BulbOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  LineChartOutlined,
  RiseOutlined,
  FallOutlined
} from '@ant-design/icons'
import { PerformanceLineChart, PerformanceBarChart } from '@shared/components/charts/PerformanceChart'

const { Title, Text } = Typography
const { TextArea } = Input

interface OptimizationRule {
  id: string
  name: string
  description: string
  category: 'performance' | 'quality' | 'cost' | 'reliability'
  impact: 'low' | 'medium' | 'high'
  effort: 'low' | 'medium' | 'high'
  status: 'active' | 'inactive' | 'testing'
  metrics: {
    successRateImprovement: number
    responseTimeImprovement: number
    costReduction: number
  }
}

interface OptimizationJob {
  id: string
  templateKey: string
  templateName: string
  optimizationType: string
  status: 'running' | 'completed' | 'failed' | 'queued'
  progress: number
  startTime: string
  estimatedCompletion: string
  results?: {
    beforeMetrics: any
    afterMetrics: any
    improvement: number
  }
}

export const PerformanceOptimizer: React.FC = () => {
  const [isOptimizationModalVisible, setIsOptimizationModalVisible] = useState(false)
  const [selectedTemplate, setSelectedTemplate] = useState<string>('')
  const [form] = Form.useForm()

  // Mock optimization data
  const [optimizationRules] = useState<OptimizationRule[]>([
    {
      id: 'rule_1',
      name: 'Prompt Length Optimization',
      description: 'Automatically optimize prompt length for better performance',
      category: 'performance',
      impact: 'high',
      effort: 'low',
      status: 'active',
      metrics: {
        successRateImprovement: 12.5,
        responseTimeImprovement: 23.0,
        costReduction: 8.5
      }
    },
    {
      id: 'rule_2',
      name: 'Context Window Management',
      description: 'Intelligently manage context window size based on query complexity',
      category: 'performance',
      impact: 'medium',
      effort: 'medium',
      status: 'active',
      metrics: {
        successRateImprovement: 8.2,
        responseTimeImprovement: 15.5,
        costReduction: 12.0
      }
    },
    {
      id: 'rule_3',
      name: 'Error Pattern Recognition',
      description: 'Detect and prevent common error patterns in template execution',
      category: 'reliability',
      impact: 'high',
      effort: 'high',
      status: 'testing',
      metrics: {
        successRateImprovement: 18.7,
        responseTimeImprovement: 5.2,
        costReduction: 3.1
      }
    }
  ])

  const [optimizationJobs] = useState<OptimizationJob[]>([
    {
      id: 'job_1',
      templateKey: 'sql_generation',
      templateName: 'SQL Generation Template',
      optimizationType: 'Prompt Optimization',
      status: 'running',
      progress: 67,
      startTime: '2024-01-15T10:30:00Z',
      estimatedCompletion: '2024-01-15T11:15:00Z'
    },
    {
      id: 'job_2',
      templateKey: 'insight_generation',
      templateName: 'Insight Generation Template',
      optimizationType: 'Context Optimization',
      status: 'completed',
      progress: 100,
      startTime: '2024-01-15T09:00:00Z',
      estimatedCompletion: '2024-01-15T09:45:00Z',
      results: {
        beforeMetrics: { successRate: 85.2, responseTime: 1.8 },
        afterMetrics: { successRate: 91.5, responseTime: 1.4 },
        improvement: 7.4
      }
    }
  ])

  const handleStartOptimization = async (values: any) => {
    try {
      // Simulate optimization start
      message.success('Optimization job started successfully')
      setIsOptimizationModalVisible(false)
      form.resetFields()
    } catch (error) {
      message.error('Failed to start optimization')
    }
  }

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'performance': return '#52c41a'
      case 'quality': return '#1890ff'
      case 'cost': return '#fa8c16'
      case 'reliability': return '#722ed1'
      default: return '#d9d9d9'
    }
  }

  const getImpactColor = (impact: string) => {
    switch (impact) {
      case 'high': return '#f5222d'
      case 'medium': return '#fa8c16'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'green'
      case 'inactive': return 'default'
      case 'testing': return 'blue'
      case 'running': return 'processing'
      case 'completed': return 'success'
      case 'failed': return 'error'
      case 'queued': return 'warning'
      default: return 'default'
    }
  }

  const rulesColumns = [
    {
      title: 'Optimization Rule',
      key: 'rule',
      render: (record: OptimizationRule) => (
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
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      render: (category: string) => (
        <Tag color={getCategoryColor(category)}>
          {category.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Impact',
      dataIndex: 'impact',
      key: 'impact',
      render: (impact: string) => (
        <Tag color={getImpactColor(impact)}>
          {impact.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Badge status={getStatusColor(status)} text={status.toUpperCase()} />
      )
    },
    {
      title: 'Expected Improvement',
      key: 'improvement',
      render: (record: OptimizationRule) => (
        <div>
          <div>Success Rate: +{record.metrics.successRateImprovement}%</div>
          <div>Response Time: -{record.metrics.responseTimeImprovement}%</div>
          <div>Cost: -{record.metrics.costReduction}%</div>
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: OptimizationRule) => (
        <Space>
          <Button size="small" icon={<SettingOutlined />}>
            Configure
          </Button>
          <Button 
            size="small" 
            type={record.status === 'active' ? 'default' : 'primary'}
            icon={record.status === 'active' ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
          >
            {record.status === 'active' ? 'Disable' : 'Enable'}
          </Button>
        </Space>
      )
    }
  ]

  const jobsColumns = [
    {
      title: 'Template',
      key: 'template',
      render: (record: OptimizationJob) => (
        <div>
          <Text strong>{record.templateName}</Text>
          <div style={{ marginTop: '4px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.templateKey}
            </Text>
          </div>
        </div>
      )
    },
    {
      title: 'Optimization Type',
      dataIndex: 'optimizationType',
      key: 'optimizationType'
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Badge status={getStatusColor(status)} text={status.toUpperCase()} />
      )
    },
    {
      title: 'Progress',
      key: 'progress',
      render: (record: OptimizationJob) => (
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
      title: 'Results',
      key: 'results',
      render: (record: OptimizationJob) => {
        if (record.results) {
          return (
            <div>
              <Text style={{ color: '#52c41a' }}>
                +{record.results.improvement}% improvement
              </Text>
            </div>
          )
        }
        return <Text type="secondary">Pending</Text>
      }
    }
  ]

  // Mock performance data
  const performanceData = [
    { time: '00:00', before: 85, after: 91 },
    { time: '04:00', before: 87, after: 93 },
    { time: '08:00', before: 82, after: 89 },
    { time: '12:00', before: 88, after: 94 },
    { time: '16:00', before: 86, after: 92 },
    { time: '20:00', before: 89, after: 95 }
  ]

  return (
    <div>
      {/* Quick Actions */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Active Optimizations"
              value={optimizationRules.filter(r => r.status === 'active').length}
              prefix={<RocketOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Running Jobs"
              value={optimizationJobs.filter(j => j.status === 'running').length}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Avg Improvement"
              value={12.3}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card style={{ textAlign: 'center' }}>
            <Button 
              type="primary" 
              icon={<PlayCircleOutlined />}
              onClick={() => setIsOptimizationModalVisible(true)}
              size="large"
            >
              Start Optimization
            </Button>
          </Card>
        </Col>
      </Row>

      {/* Performance Impact Chart */}
      <Card title="Optimization Impact Over Time" style={{ marginBottom: '24px' }}>
        <PerformanceLineChart
          data={performanceData}
          xAxisKey="time"
          lines={[
            { key: 'before', color: '#fa8c16', name: 'Before Optimization' },
            { key: 'after', color: '#52c41a', name: 'After Optimization' }
          ]}
          height={300}
        />
      </Card>

      {/* Optimization Rules */}
      <Card 
        title="Optimization Rules" 
        extra={
          <Button icon={<SettingOutlined />}>
            Manage Rules
          </Button>
        }
        style={{ marginBottom: '24px' }}
      >
        <Table
          columns={rulesColumns}
          dataSource={optimizationRules}
          pagination={false}
          size="small"
        />
      </Card>

      {/* Active Jobs */}
      <Card title="Optimization Jobs">
        <Table
          columns={jobsColumns}
          dataSource={optimizationJobs}
          pagination={false}
          size="small"
        />
      </Card>

      {/* Start Optimization Modal */}
      <Modal
        title="Start Template Optimization"
        open={isOptimizationModalVisible}
        onCancel={() => setIsOptimizationModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setIsOptimizationModalVisible(false)}>
            Cancel
          </Button>,
          <Button key="start" type="primary" onClick={() => form.submit()}>
            Start Optimization
          </Button>
        ]}
      >
        <Form form={form} layout="vertical" onFinish={handleStartOptimization}>
          <Form.Item
            name="templateKey"
            label="Template to Optimize"
            rules={[{ required: true, message: 'Please select a template' }]}
          >
            <Select placeholder="Select template">
              <Select.Option value="sql_generation">SQL Generation Template</Select.Option>
              <Select.Option value="insight_generation">Insight Generation Template</Select.Option>
              <Select.Option value="explanation">Explanation Template</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="optimizationType"
            label="Optimization Type"
            rules={[{ required: true, message: 'Please select optimization type' }]}
          >
            <Select placeholder="Select optimization type">
              <Select.Option value="prompt">Prompt Optimization</Select.Option>
              <Select.Option value="context">Context Optimization</Select.Option>
              <Select.Option value="performance">Performance Tuning</Select.Option>
              <Select.Option value="comprehensive">Comprehensive Optimization</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="aggressiveness"
            label="Optimization Aggressiveness"
            initialValue={50}
          >
            <Slider
              marks={{
                0: 'Conservative',
                50: 'Balanced',
                100: 'Aggressive'
              }}
            />
          </Form.Item>

          <Form.Item
            name="preserveCompatibility"
            label="Preserve Backward Compatibility"
            valuePropName="checked"
            initialValue={true}
          >
            <Switch />
          </Form.Item>

          <Form.Item
            name="notes"
            label="Notes"
          >
            <TextArea 
              rows={3} 
              placeholder="Optional notes about this optimization..."
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

export default PerformanceOptimizer
