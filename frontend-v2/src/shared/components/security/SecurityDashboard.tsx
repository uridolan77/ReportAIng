/**
 * Security Dashboard Component
 * 
 * Provides comprehensive security monitoring including:
 * - Threat detection and reporting
 * - Security configuration management
 * - Real-time security alerts
 * - Security metrics and analytics
 */

import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Alert, 
  Table, 
  Tag, 
  Space, 
  Button, 
  Typography, 
  Statistic,
  Switch,
  Tooltip,
  Modal,
  List,
  Progress,
} from 'antd'
import {
  SafetyOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BugOutlined,
  LockOutlined,
  EyeOutlined,
  SettingOutlined,
  ReloadOutlined,
  DownloadOutlined,
} from '@ant-design/icons'
import { useSecurity } from '../../security/SecurityProvider'

const { Title, Text } = Typography

interface SecurityDashboardProps {
  /** Show detailed threat information */
  showDetails?: boolean
  /** Enable real-time monitoring */
  realTimeMonitoring?: boolean
  /** Compact layout */
  compact?: boolean
}

export const SecurityDashboard: React.FC<SecurityDashboardProps> = ({
  showDetails = true,
  realTimeMonitoring = true,
  compact = false,
}) => {
  const [showConfig, setShowConfig] = useState(false)
  const [selectedThreat, setSelectedThreat] = useState<any>(null)
  
  const {
    config,
    updateConfig,
    getThreats,
    clearThreats,
    isSecureContext,
    csrfToken,
    generateCSRFToken,
  } = useSecurity()

  const threats = getThreats()

  // Security metrics
  const securityMetrics = useMemo(() => {
    const totalThreats = threats.length
    const blockedThreats = threats.filter(t => t.blocked).length
    const criticalThreats = threats.filter(t => t.severity === 'critical').length
    const highThreats = threats.filter(t => t.severity === 'high').length
    const recentThreats = threats.filter(t => 
      Date.now() - t.timestamp.getTime() < 24 * 60 * 60 * 1000
    ).length

    const threatsByType = threats.reduce((acc, threat) => {
      acc[threat.type] = (acc[threat.type] || 0) + 1
      return acc
    }, {} as Record<string, number>)

    return {
      totalThreats,
      blockedThreats,
      criticalThreats,
      highThreats,
      recentThreats,
      threatsByType,
      blockRate: totalThreats > 0 ? (blockedThreats / totalThreats) * 100 : 100,
    }
  }, [threats])

  // Security score calculation
  const securityScore = useMemo(() => {
    let score = 100

    // Deduct points for configuration issues
    if (!config.enableCSP) score -= 15
    if (!config.enableXSSProtection) score -= 20
    if (!config.enableCSRFProtection) score -= 15
    if (!config.enableInputSanitization) score -= 10
    if (!isSecureContext) score -= 20

    // Deduct points for recent threats
    score -= Math.min(securityMetrics.recentThreats * 2, 20)
    score -= securityMetrics.criticalThreats * 5
    score -= securityMetrics.highThreats * 2

    return Math.max(score, 0)
  }, [config, isSecureContext, securityMetrics])

  // Threat table columns
  const threatColumns = [
    {
      title: 'Time',
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 120,
      render: (timestamp: Date) => timestamp.toLocaleTimeString(),
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 100,
      render: (type: string) => (
        <Tag color={
          type === 'xss' ? 'red' :
          type === 'csrf' ? 'orange' :
          type === 'injection' ? 'volcano' :
          type === 'unauthorized' ? 'magenta' :
          'blue'
        }>
          {type.toUpperCase()}
        </Tag>
      ),
    },
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      width: 100,
      render: (severity: string) => (
        <Tag color={
          severity === 'critical' ? 'red' :
          severity === 'high' ? 'orange' :
          severity === 'medium' ? 'yellow' :
          'green'
        }>
          {severity.toUpperCase()}
        </Tag>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Status',
      dataIndex: 'blocked',
      key: 'blocked',
      width: 80,
      render: (blocked: boolean) => (
        <Tag color={blocked ? 'green' : 'red'}>
          {blocked ? 'BLOCKED' : 'ALLOWED'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_: any, record: any) => (
        <Button
          size="small"
          icon={<EyeOutlined />}
          onClick={() => setSelectedThreat(record)}
        >
          Details
        </Button>
      ),
    },
  ]

  // Export threats data
  const exportThreats = () => {
    const csv = [
      'Timestamp,Type,Severity,Description,Blocked,Source',
      ...threats.map(threat => 
        `"${threat.timestamp.toISOString()}","${threat.type}","${threat.severity}","${threat.description}","${threat.blocked}","${threat.source}"`
      )
    ].join('\n')

    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `security-threats-${new Date().toISOString().split('T')[0]}.csv`
    link.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div className="security-dashboard">
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Security Overview */}
        <Card
          title={
            <Space>
              <SafetyOutlined />
              <span>Security Overview</span>
            </Space>
          }
          extra={
            <Space>
              <Button
                icon={<SettingOutlined />}
                onClick={() => setShowConfig(true)}
              >
                Configure
              </Button>
              <Button
                icon={<ReloadOutlined />}
                onClick={() => window.location.reload()}
              >
                Refresh
              </Button>
            </Space>
          }
        >
          <Row gutter={[16, 16]}>
            <Col span={6}>
              <Statistic
                title="Security Score"
                value={securityScore}
                suffix="/100"
                valueStyle={{ 
                  color: securityScore >= 90 ? '#3f8600' : 
                         securityScore >= 70 ? '#faad14' : '#cf1322' 
                }}
                prefix={
                  securityScore >= 90 ? <CheckCircleOutlined /> :
                  securityScore >= 70 ? <WarningOutlined /> :
                  <ExclamationCircleOutlined />
                }
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Total Threats"
                value={securityMetrics.totalThreats}
                prefix={<BugOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Blocked Rate"
                value={securityMetrics.blockRate}
                suffix="%"
                precision={1}
                prefix={<LockOutlined />}
                valueStyle={{ color: '#3f8600' }}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Recent Threats (24h)"
                value={securityMetrics.recentThreats}
                prefix={<WarningOutlined />}
                valueStyle={{ 
                  color: securityMetrics.recentThreats > 0 ? '#cf1322' : '#3f8600' 
                }}
              />
            </Col>
          </Row>

          {/* Security Status Indicators */}
          <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
            <Col span={24}>
              <Space wrap>
                <Tag color={config.enableCSP ? 'green' : 'red'}>
                  CSP: {config.enableCSP ? 'ENABLED' : 'DISABLED'}
                </Tag>
                <Tag color={config.enableXSSProtection ? 'green' : 'red'}>
                  XSS Protection: {config.enableXSSProtection ? 'ENABLED' : 'DISABLED'}
                </Tag>
                <Tag color={config.enableCSRFProtection ? 'green' : 'red'}>
                  CSRF Protection: {config.enableCSRFProtection ? 'ENABLED' : 'DISABLED'}
                </Tag>
                <Tag color={isSecureContext ? 'green' : 'red'}>
                  Secure Context: {isSecureContext ? 'YES' : 'NO'}
                </Tag>
                <Tag color={csrfToken ? 'green' : 'red'}>
                  CSRF Token: {csrfToken ? 'ACTIVE' : 'MISSING'}
                </Tag>
              </Space>
            </Col>
          </Row>
        </Card>

        {/* Threat Analytics */}
        {showDetails && (
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <Card title="Threat Distribution" size="small">
                <List
                  dataSource={Object.entries(securityMetrics.threatsByType)}
                  renderItem={([type, count]) => (
                    <List.Item>
                      <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                        <Text>{type.toUpperCase()}</Text>
                        <Tag>{count}</Tag>
                      </Space>
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
            <Col span={12}>
              <Card title="Security Health" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text>Overall Security</Text>
                    <Progress 
                      percent={securityScore} 
                      status={securityScore >= 90 ? 'success' : securityScore >= 70 ? 'normal' : 'exception'}
                    />
                  </div>
                  <div>
                    <Text>Threat Block Rate</Text>
                    <Progress 
                      percent={securityMetrics.blockRate} 
                      status="success"
                    />
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>
        )}

        {/* Recent Threats */}
        <Card
          title={
            <Space>
              <BugOutlined />
              <span>Security Threats</span>
              <Tag color="blue">{threats.length}</Tag>
            </Space>
          }
          extra={
            <Space>
              <Button
                size="small"
                icon={<DownloadOutlined />}
                onClick={exportThreats}
                disabled={threats.length === 0}
              >
                Export
              </Button>
              <Button
                size="small"
                danger
                onClick={clearThreats}
                disabled={threats.length === 0}
              >
                Clear All
              </Button>
            </Space>
          }
        >
          <Table
            dataSource={threats}
            columns={threatColumns}
            rowKey="id"
            size="small"
            pagination={{
              pageSize: compact ? 5 : 10,
              showSizeChanger: true,
              showQuickJumper: true,
            }}
            locale={{ emptyText: 'No security threats detected' }}
          />
        </Card>

        {/* Security Alerts */}
        {securityMetrics.criticalThreats > 0 && (
          <Alert
            message="Critical Security Threats Detected"
            description={`${securityMetrics.criticalThreats} critical security threats require immediate attention.`}
            type="error"
            showIcon
            action={
              <Button size="small" danger>
                Review Threats
              </Button>
            }
          />
        )}

        {!isSecureContext && (
          <Alert
            message="Insecure Context Detected"
            description="The application is not running in a secure context (HTTPS). Some security features may be limited."
            type="warning"
            showIcon
          />
        )}
      </Space>

      {/* Security Configuration Modal */}
      <Modal
        title="Security Configuration"
        open={showConfig}
        onCancel={() => setShowConfig(false)}
        footer={[
          <Button key="close" onClick={() => setShowConfig(false)}>
            Close
          </Button>
        ]}
        width={600}
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <div>
                <Text strong>Content Security Policy (CSP)</Text>
                <br />
                <Text type="secondary">Prevents XSS attacks by controlling resource loading</Text>
              </div>
              <Switch
                checked={config.enableCSP}
                onChange={(checked) => updateConfig({ enableCSP: checked })}
              />
            </Space>
          </div>

          <div>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <div>
                <Text strong>XSS Protection</Text>
                <br />
                <Text type="secondary">Sanitizes HTML content to prevent script injection</Text>
              </div>
              <Switch
                checked={config.enableXSSProtection}
                onChange={(checked) => updateConfig({ enableXSSProtection: checked })}
              />
            </Space>
          </div>

          <div>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <div>
                <Text strong>CSRF Protection</Text>
                <br />
                <Text type="secondary">Prevents cross-site request forgery attacks</Text>
              </div>
              <Switch
                checked={config.enableCSRFProtection}
                onChange={(checked) => updateConfig({ enableCSRFProtection: checked })}
              />
            </Space>
          </div>

          <div>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <div>
                <Text strong>Input Sanitization</Text>
                <br />
                <Text type="secondary">Cleans user input to prevent injection attacks</Text>
              </div>
              <Switch
                checked={config.enableInputSanitization}
                onChange={(checked) => updateConfig({ enableInputSanitization: checked })}
              />
            </Space>
          </div>

          <div>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <div>
                <Text strong>Security Monitoring</Text>
                <br />
                <Text type="secondary">Monitors and reports security events</Text>
              </div>
              <Switch
                checked={config.enableMonitoring}
                onChange={(checked) => updateConfig({ enableMonitoring: checked })}
              />
            </Space>
          </div>

          {csrfToken && (
            <div>
              <Text strong>CSRF Token:</Text>
              <br />
              <Text code copyable style={{ fontSize: '12px' }}>
                {csrfToken}
              </Text>
              <br />
              <Button size="small" onClick={generateCSRFToken} style={{ marginTop: 8 }}>
                Regenerate Token
              </Button>
            </div>
          )}
        </Space>
      </Modal>

      {/* Threat Details Modal */}
      <Modal
        title="Threat Details"
        open={!!selectedThreat}
        onCancel={() => setSelectedThreat(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedThreat(null)}>
            Close
          </Button>
        ]}
        width={700}
      >
        {selectedThreat && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Type:</Text> <Tag color="red">{selectedThreat.type.toUpperCase()}</Tag>
            </div>
            <div>
              <Text strong>Severity:</Text> <Tag color="orange">{selectedThreat.severity.toUpperCase()}</Tag>
            </div>
            <div>
              <Text strong>Timestamp:</Text> {selectedThreat.timestamp.toLocaleString()}
            </div>
            <div>
              <Text strong>Source:</Text> {selectedThreat.source}
            </div>
            <div>
              <Text strong>Status:</Text> 
              <Tag color={selectedThreat.blocked ? 'green' : 'red'}>
                {selectedThreat.blocked ? 'BLOCKED' : 'ALLOWED'}
              </Tag>
            </div>
            <div>
              <Text strong>Description:</Text>
              <br />
              <Text>{selectedThreat.description}</Text>
            </div>
            {selectedThreat.details && (
              <div>
                <Text strong>Details:</Text>
                <pre style={{ 
                  background: '#f5f5f5', 
                  padding: '8px', 
                  borderRadius: '4px',
                  fontSize: '12px',
                  overflow: 'auto',
                  maxHeight: '200px'
                }}>
                  {JSON.stringify(selectedThreat.details, null, 2)}
                </pre>
              </div>
            )}
          </Space>
        )}
      </Modal>
    </div>
  )
}

export default SecurityDashboard
