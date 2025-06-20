import React from 'react'
import {
  Card,
  Row,
  Col,
  Progress,
  Typography,
  List,
  Tag,
  Alert,
  Button,
  Space,
  Statistic,
  Table,
  Tooltip
} from 'antd'
import {
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  ReloadOutlined,
  DownloadOutlined,
  WarningOutlined
} from '@ant-design/icons'
import { useValidateSemanticMetadataQuery } from '@shared/store/api/semanticApi'

const { Title, Text } = Typography

export const MetadataValidationReport: React.FC = () => {
  const { data: validationData, isLoading, refetch } = useValidateSemanticMetadataQuery()

  const getIssueIcon = (severity: 'error' | 'warning') => {
    return severity === 'error' ? (
      <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
    ) : (
      <WarningOutlined style={{ color: '#faad14' }} />
    )
  }

  const getIssueColor = (severity: 'error' | 'warning') => {
    return severity === 'error' ? 'red' : 'orange'
  }

  const mockValidationData = validationData || {
    isValid: false,
    coverage: 75,
    issues: [
      {
        type: 'Missing Business Purpose',
        message: 'Table "sales.orders" is missing business purpose description',
        severity: 'error' as const,
      },
      {
        type: 'Incomplete Column Metadata',
        message: 'Column "customer_id" in table "sales.customers" lacks business meaning',
        severity: 'error' as const,
      },
      {
        type: 'Low Quality Score',
        message: 'Column "address" has data quality score below threshold (3/10)',
        severity: 'warning' as const,
      },
      {
        type: 'Missing Natural Language Aliases',
        message: 'Table "hr.employees" has no natural language aliases defined',
        severity: 'warning' as const,
      },
      {
        type: 'Outdated Metadata',
        message: 'Business metadata for "finance.transactions" not updated in 6 months',
        severity: 'warning' as const,
      },
    ]
  }

  const errorCount = mockValidationData.issues.filter(issue => issue.severity === 'error').length
  const warningCount = mockValidationData.issues.filter(issue => issue.severity === 'warning').length

  const issueColumns = [
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      width: 100,
      render: (severity: 'error' | 'warning') => (
        <Tag color={getIssueColor(severity)} icon={getIssueIcon(severity)}>
          {severity.toUpperCase()}
        </Tag>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 200,
    },
    {
      title: 'Message',
      dataIndex: 'message',
      key: 'message',
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text}</Text>
        </Tooltip>
      ),
    },
  ]

  const coverageColor = mockValidationData.coverage > 80 ? '#52c41a' : 
                       mockValidationData.coverage > 60 ? '#faad14' : '#ff4d4f'

  return (
    <div>
      {/* Overall Status */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col span={24}>
          <Alert
            message={
              mockValidationData.isValid 
                ? "Metadata validation passed successfully" 
                : "Metadata validation found issues that need attention"
            }
            type={mockValidationData.isValid ? "success" : "warning"}
            icon={mockValidationData.isValid ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
            showIcon
            action={
              <Space>
                <Button size="small" icon={<ReloadOutlined />} onClick={() => refetch()}>
                  Refresh
                </Button>
                <Button size="small" icon={<DownloadOutlined />}>
                  Export Report
                </Button>
              </Space>
            }
          />
        </Col>
      </Row>

      {/* Metrics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Metadata Coverage"
              value={mockValidationData.coverage}
              suffix="%"
              valueStyle={{ color: coverageColor }}
            />
            <Progress 
              percent={mockValidationData.coverage} 
              strokeColor={coverageColor}
              size="small"
              showInfo={false}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Critical Errors"
              value={errorCount}
              valueStyle={{ color: errorCount > 0 ? '#ff4d4f' : '#52c41a' }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Warnings"
              value={warningCount}
              valueStyle={{ color: warningCount > 0 ? '#faad14' : '#52c41a' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Overall Status"
              value={mockValidationData.isValid ? "PASS" : "FAIL"}
              valueStyle={{ 
                color: mockValidationData.isValid ? '#52c41a' : '#ff4d4f' 
              }}
              prefix={
                mockValidationData.isValid ? 
                <CheckCircleOutlined /> : 
                <CloseCircleOutlined />
              }
            />
          </Card>
        </Col>
      </Row>

      {/* Detailed Issues */}
      <Card 
        title={`Validation Issues (${mockValidationData.issues.length})`}
        extra={
          <Space>
            <Text type="secondary">
              {errorCount} errors, {warningCount} warnings
            </Text>
          </Space>
        }
      >
        <Table
          columns={issueColumns}
          dataSource={mockValidationData.issues}
          loading={isLoading}
          rowKey={(record, index) => `${record.type}-${index}`}
          size="small"
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} issues`,
          }}
        />
      </Card>

      {/* Coverage Breakdown */}
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col xs={24} lg={12}>
          <Card title="Coverage by Category">
            <List
              size="small"
              dataSource={[
                { category: 'Business Purpose', coverage: 85, total: 120 },
                { category: 'Column Meanings', coverage: 72, total: 450 },
                { category: 'Natural Language Aliases', coverage: 60, total: 120 },
                { category: 'Business Rules', coverage: 45, total: 120 },
                { category: 'Data Quality Scores', coverage: 90, total: 450 },
              ]}
              renderItem={item => (
                <List.Item>
                  <div style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                      <Text>{item.category}</Text>
                      <Text>{item.coverage}%</Text>
                    </div>
                    <Progress 
                      percent={item.coverage} 
                      size="small" 
                      showInfo={false}
                      strokeColor={item.coverage > 80 ? '#52c41a' : item.coverage > 60 ? '#faad14' : '#ff4d4f'}
                    />
                    <Text type="secondary" style={{ fontSize: '11px' }}>
                      {Math.round(item.total * item.coverage / 100)} of {item.total} items
                    </Text>
                  </div>
                </List.Item>
              )}
            />
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Recommendations">
            <List
              size="small"
              dataSource={[
                {
                  title: 'Complete Missing Business Purposes',
                  description: 'Add business purpose descriptions to 18 tables',
                  priority: 'High',
                },
                {
                  title: 'Update Column Business Meanings',
                  description: 'Define business meanings for 126 columns',
                  priority: 'High',
                },
                {
                  title: 'Add Natural Language Aliases',
                  description: 'Create user-friendly aliases for 48 tables',
                  priority: 'Medium',
                },
                {
                  title: 'Review Data Quality Scores',
                  description: 'Update quality scores for 45 columns',
                  priority: 'Medium',
                },
                {
                  title: 'Document Business Rules',
                  description: 'Add business rules for 66 tables',
                  priority: 'Low',
                },
              ]}
              renderItem={item => (
                <List.Item>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Text strong>{item.title}</Text>
                      <Tag color={
                        item.priority === 'High' ? 'red' : 
                        item.priority === 'Medium' ? 'orange' : 'green'
                      }>
                        {item.priority}
                      </Tag>
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {item.description}
                    </Text>
                  </div>
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  )
}
