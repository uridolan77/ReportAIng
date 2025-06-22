import React, { useState } from 'react'
import { 
  Modal, 
  Timeline, 
  Card, 
  Button, 
  Space, 
  Typography, 
  Tag, 
  Tooltip,
  Row,
  Col,
  Divider,
  Alert,
  Collapse,
  Descriptions,
  message
} from 'antd'
import { 
  HistoryOutlined, 
  UserOutlined,
  CalendarOutlined,
  FileTextOutlined,
  BranchesOutlined,
  RollbackOutlined,
  EyeOutlined,
  DiffOutlined,
  DownloadOutlined,
  TagOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'

dayjs.extend(relativeTime)

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

interface TemplateVersionHistoryProps {
  visible: boolean
  onClose: () => void
  template?: any
}

// Mock version history data - in real implementation, this would come from API
const mockVersionHistory = [
  {
    version: '2.1.0',
    timestamp: '2024-01-15T10:30:00Z',
    author: 'john.doe@company.com',
    changeType: 'major',
    description: 'Added support for complex joins and subqueries',
    changes: [
      'Enhanced SQL generation logic for complex queries',
      'Added validation for nested subqueries',
      'Improved error handling for malformed queries'
    ],
    performanceImpact: {
      successRate: 0.92,
      avgResponseTime: 1.2,
      usageCount: 1250
    },
    tags: ['enhancement', 'sql', 'performance'],
    isCurrent: true
  },
  {
    version: '2.0.1',
    timestamp: '2024-01-10T14:20:00Z',
    author: 'jane.smith@company.com',
    changeType: 'patch',
    description: 'Fixed issue with date formatting in WHERE clauses',
    changes: [
      'Fixed date format handling in SQL generation',
      'Updated regex patterns for date validation',
      'Added unit tests for date scenarios'
    ],
    performanceImpact: {
      successRate: 0.89,
      avgResponseTime: 1.1,
      usageCount: 980
    },
    tags: ['bugfix', 'dates'],
    isCurrent: false
  },
  {
    version: '2.0.0',
    timestamp: '2024-01-05T09:15:00Z',
    author: 'mike.wilson@company.com',
    changeType: 'major',
    description: 'Major rewrite to support new AI model capabilities',
    changes: [
      'Complete template restructure for GPT-4 compatibility',
      'Added context-aware prompt engineering',
      'Implemented dynamic schema injection',
      'Enhanced natural language understanding'
    ],
    performanceImpact: {
      successRate: 0.87,
      avgResponseTime: 1.3,
      usageCount: 750
    },
    tags: ['major-update', 'ai-model', 'restructure'],
    isCurrent: false
  },
  {
    version: '1.5.2',
    timestamp: '2023-12-20T16:45:00Z',
    author: 'sarah.johnson@company.com',
    changeType: 'minor',
    description: 'Added support for aggregate functions',
    changes: [
      'Added COUNT, SUM, AVG, MIN, MAX function support',
      'Improved GROUP BY clause generation',
      'Enhanced HAVING clause logic'
    ],
    performanceImpact: {
      successRate: 0.85,
      avgResponseTime: 1.0,
      usageCount: 650
    },
    tags: ['feature', 'aggregates'],
    isCurrent: false
  }
]

export const TemplateVersionHistory: React.FC<TemplateVersionHistoryProps> = ({
  visible,
  onClose,
  template
}) => {
  const [selectedVersion, setSelectedVersion] = useState<any>(null)
  const [showDiff, setShowDiff] = useState(false)

  const getChangeTypeColor = (changeType: string) => {
    switch (changeType) {
      case 'major': return 'red'
      case 'minor': return 'blue'
      case 'patch': return 'green'
      default: return 'default'
    }
  }

  const getChangeTypeIcon = (changeType: string) => {
    switch (changeType) {
      case 'major': return <BranchesOutlined />
      case 'minor': return <FileTextOutlined />
      case 'patch': return <TagOutlined />
      default: return <HistoryOutlined />
    }
  }

  const handleRevertToVersion = (version: any) => {
    Modal.confirm({
      title: 'Revert to Version',
      content: `Are you sure you want to revert to version ${version.version}? This will create a new version with the content from ${version.version}.`,
      onOk: () => {
        message.success(`Reverted to version ${version.version}`)
        onClose()
      }
    })
  }

  const handleViewDiff = (version: any) => {
    setSelectedVersion(version)
    setShowDiff(true)
  }

  const renderVersionCard = (version: any) => (
    <Card 
      size="small" 
      style={{ marginBottom: '16px' }}
      title={
        <Space>
          {getChangeTypeIcon(version.changeType)}
          <Text strong>Version {version.version}</Text>
          {version.isCurrent && <Tag color="green">Current</Tag>}
          <Tag color={getChangeTypeColor(version.changeType)}>
            {version.changeType.toUpperCase()}
          </Tag>
        </Space>
      }
      extra={
        <Space>
          <Button 
            size="small" 
            icon={<EyeOutlined />}
            onClick={() => setSelectedVersion(version)}
          >
            View
          </Button>
          <Button 
            size="small" 
            icon={<DiffOutlined />}
            onClick={() => handleViewDiff(version)}
          >
            Diff
          </Button>
          {!version.isCurrent && (
            <Button 
              size="small" 
              icon={<RollbackOutlined />}
              onClick={() => handleRevertToVersion(version)}
            >
              Revert
            </Button>
          )}
        </Space>
      }
    >
      <Row gutter={16}>
        <Col span={16}>
          <Descriptions size="small" column={1}>
            <Descriptions.Item label="Author">
              <Space>
                <UserOutlined />
                {version.author}
              </Space>
            </Descriptions.Item>
            <Descriptions.Item label="Date">
              <Space>
                <CalendarOutlined />
                {dayjs(version.timestamp).format('MMM DD, YYYY HH:mm')}
                <Text type="secondary">({dayjs(version.timestamp).fromNow()})</Text>
              </Space>
            </Descriptions.Item>
            <Descriptions.Item label="Description">
              {version.description}
            </Descriptions.Item>
          </Descriptions>

          <Divider style={{ margin: '12px 0' }} />

          <div>
            <Text strong>Changes:</Text>
            <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
              {version.changes.map((change: string, index: number) => (
                <li key={index} style={{ marginBottom: '4px' }}>
                  <Text>{change}</Text>
                </li>
              ))}
            </ul>
          </div>

          <div style={{ marginTop: '12px' }}>
            <Text strong>Tags:</Text>
            <div style={{ marginTop: '4px' }}>
              {version.tags.map((tag: string) => (
                <Tag key={tag} size="small">{tag}</Tag>
              ))}
            </div>
          </div>
        </Col>

        <Col span={8}>
          <Card size="small" title="Performance Impact">
            <Row gutter={8}>
              <Col span={24}>
                <div style={{ textAlign: 'center', marginBottom: '8px' }}>
                  <div style={{ fontSize: '18px', fontWeight: 'bold', color: '#52c41a' }}>
                    {(version.performanceImpact.successRate * 100).toFixed(1)}%
                  </div>
                  <div style={{ fontSize: '12px', color: '#666' }}>Success Rate</div>
                </div>
              </Col>
              <Col span={12}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '14px', fontWeight: 'bold' }}>
                    {version.performanceImpact.avgResponseTime}s
                  </div>
                  <div style={{ fontSize: '10px', color: '#666' }}>Avg Response</div>
                </div>
              </Col>
              <Col span={12}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '14px', fontWeight: 'bold' }}>
                    {version.performanceImpact.usageCount}
                  </div>
                  <div style={{ fontSize: '10px', color: '#666' }}>Total Uses</div>
                </div>
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>
    </Card>
  )

  const renderTimeline = () => (
    <Timeline mode="left">
      {mockVersionHistory.map((version, index) => (
        <Timeline.Item
          key={version.version}
          color={version.isCurrent ? 'green' : 'blue'}
          dot={getChangeTypeIcon(version.changeType)}
          label={
            <div style={{ width: '120px' }}>
              <div style={{ fontWeight: 'bold' }}>v{version.version}</div>
              <div style={{ fontSize: '12px', color: '#666' }}>
                {dayjs(version.timestamp).format('MMM DD, YYYY')}
              </div>
            </div>
          }
        >
          {renderVersionCard(version)}
        </Timeline.Item>
      ))}
    </Timeline>
  )

  return (
    <>
      <Modal
        title={
          <Space>
            <HistoryOutlined />
            Version History: {template?.templateName}
          </Space>
        }
        open={visible}
        onCancel={onClose}
        width={1200}
        footer={[
          <Button key="close" onClick={onClose}>
            Close
          </Button>,
          <Button key="export" icon={<DownloadOutlined />}>
            Export History
          </Button>
        ]}
      >
        <div style={{ maxHeight: '70vh', overflow: 'auto' }}>
          <Alert
            message="Version Control Information"
            description="This template has been through multiple iterations. You can view details, compare versions, and revert to previous versions if needed."
            type="info"
            showIcon
            style={{ marginBottom: '16px' }}
          />

          {renderTimeline()}
        </div>
      </Modal>

      {/* Version Details Modal */}
      <Modal
        title={`Version ${selectedVersion?.version} Details`}
        open={!!selectedVersion && !showDiff}
        onCancel={() => setSelectedVersion(null)}
        width={800}
        footer={[
          <Button key="close" onClick={() => setSelectedVersion(null)}>
            Close
          </Button>,
          <Button 
            key="revert" 
            type="primary"
            icon={<RollbackOutlined />}
            onClick={() => handleRevertToVersion(selectedVersion)}
            disabled={selectedVersion?.isCurrent}
          >
            Revert to This Version
          </Button>
        ]}
      >
        {selectedVersion && (
          <div>
            <Descriptions bordered column={2}>
              <Descriptions.Item label="Version" span={1}>
                {selectedVersion.version}
                {selectedVersion.isCurrent && <Tag color="green" style={{ marginLeft: '8px' }}>Current</Tag>}
              </Descriptions.Item>
              <Descriptions.Item label="Change Type" span={1}>
                <Tag color={getChangeTypeColor(selectedVersion.changeType)}>
                  {selectedVersion.changeType.toUpperCase()}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Author" span={1}>
                {selectedVersion.author}
              </Descriptions.Item>
              <Descriptions.Item label="Date" span={1}>
                {dayjs(selectedVersion.timestamp).format('MMMM DD, YYYY HH:mm:ss')}
              </Descriptions.Item>
              <Descriptions.Item label="Description" span={2}>
                {selectedVersion.description}
              </Descriptions.Item>
            </Descriptions>

            <Divider />

            <Title level={5}>Detailed Changes</Title>
            <ul>
              {selectedVersion.changes.map((change: string, index: number) => (
                <li key={index} style={{ marginBottom: '8px' }}>
                  {change}
                </li>
              ))}
            </ul>

            <Divider />

            <Title level={5}>Performance Metrics</Title>
            <Row gutter={16}>
              <Col span={8}>
                <Card size="small">
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                      {(selectedVersion.performanceImpact.successRate * 100).toFixed(1)}%
                    </div>
                    <div>Success Rate</div>
                  </div>
                </Card>
              </Col>
              <Col span={8}>
                <Card size="small">
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                      {selectedVersion.performanceImpact.avgResponseTime}s
                    </div>
                    <div>Avg Response Time</div>
                  </div>
                </Card>
              </Col>
              <Col span={8}>
                <Card size="small">
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#722ed1' }}>
                      {selectedVersion.performanceImpact.usageCount}
                    </div>
                    <div>Total Usage</div>
                  </div>
                </Card>
              </Col>
            </Row>
          </div>
        )}
      </Modal>

      {/* Diff Modal */}
      <Modal
        title={`Version Diff: ${selectedVersion?.version}`}
        open={showDiff}
        onCancel={() => setShowDiff(false)}
        width={1000}
        footer={[
          <Button key="close" onClick={() => setShowDiff(false)}>
            Close
          </Button>
        ]}
      >
        <Alert
          message="Version Comparison"
          description="Detailed diff view would be implemented here showing line-by-line changes between versions."
          type="info"
          showIcon
        />
      </Modal>
    </>
  )
}

export default TemplateVersionHistory
