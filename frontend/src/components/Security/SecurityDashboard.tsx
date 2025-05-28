import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Switch,
  Button,
  Alert,
  Space,
  Typography,
  Table,
  Tag,
  Progress,
  Tooltip,
  Modal,
  Input,
  Form
} from 'antd';
import {
  SafetyOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  LockOutlined,
  KeyOutlined,
  EyeOutlined,
  SettingOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { validateQuery, validateNaturalLanguageQuery } from '../../utils/queryValidator';
import { SecurityUtils } from '../../utils/security';
import { tokenManager } from '../../services/tokenManager';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;

interface SecurityMetrics {
  totalQueries: number;
  blockedQueries: number;
  warningQueries: number;
  securityScore: number;
  lastSecurityScan: string;
  activeTokens: number;
  failedLogins: number;
}

interface SecurityEvent {
  id: string;
  timestamp: string;
  type: 'blocked_query' | 'failed_login' | 'token_refresh' | 'suspicious_activity';
  severity: 'low' | 'medium' | 'high';
  description: string;
  userAgent?: string;
  ipAddress?: string;
}

export const SecurityDashboard: React.FC = () => {
  const [metrics, setMetrics] = useState<SecurityMetrics>({
    totalQueries: 0,
    blockedQueries: 0,
    warningQueries: 0,
    securityScore: 85,
    lastSecurityScan: new Date().toISOString(),
    activeTokens: 0,
    failedLogins: 0
  });

  const [securityEvents, setSecurityEvents] = useState<SecurityEvent[]>([]);
  const [securitySettings, setSecuritySettings] = useState({
    enableQueryValidation: true,
    enableTokenRotation: true,
    enableRateLimiting: true,
    enableSecurityLogging: true,
    maxQueryLength: 5000,
    tokenExpiryMinutes: 60,
    maxFailedLogins: 5
  });

  const [testQueryVisible, setTestQueryVisible] = useState(false);
  const [testQuery, setTestQuery] = useState('');
  const [testResult, setTestResult] = useState<any>(null);

  useEffect(() => {
    loadSecurityMetrics();
    loadSecurityEvents();
  }, []);

  const loadSecurityMetrics = async () => {
    // In a real app, this would fetch from an API
    const mockMetrics: SecurityMetrics = {
      totalQueries: 1247,
      blockedQueries: 23,
      warningQueries: 156,
      securityScore: calculateSecurityScore(),
      lastSecurityScan: new Date().toISOString(),
      activeTokens: 12,
      failedLogins: 3
    };
    setMetrics(mockMetrics);
  };

  const loadSecurityEvents = () => {
    // Mock security events
    const mockEvents: SecurityEvent[] = [
      {
        id: '1',
        timestamp: new Date(Date.now() - 3600000).toISOString(),
        type: 'blocked_query',
        severity: 'high',
        description: 'SQL injection attempt detected in query',
        userAgent: 'Mozilla/5.0...',
        ipAddress: '192.168.1.100'
      },
      {
        id: '2',
        timestamp: new Date(Date.now() - 7200000).toISOString(),
        type: 'failed_login',
        severity: 'medium',
        description: 'Multiple failed login attempts',
        ipAddress: '10.0.0.50'
      },
      {
        id: '3',
        timestamp: new Date(Date.now() - 10800000).toISOString(),
        type: 'token_refresh',
        severity: 'low',
        description: 'Token refresh completed successfully',
        userAgent: 'Mozilla/5.0...'
      }
    ];
    setSecurityEvents(mockEvents);
  };

  const calculateSecurityScore = (): number => {
    let score = 100;

    // Deduct points for security issues
    if (!securitySettings.enableQueryValidation) score -= 20;
    if (!securitySettings.enableTokenRotation) score -= 15;
    if (!securitySettings.enableRateLimiting) score -= 10;
    if (!securitySettings.enableSecurityLogging) score -= 5;

    // Deduct points for recent security events
    const recentHighSeverityEvents = securityEvents.filter(
      event => event.severity === 'high' &&
      Date.now() - new Date(event.timestamp).getTime() < 24 * 60 * 60 * 1000
    ).length;

    score -= recentHighSeverityEvents * 10;

    return Math.max(score, 0);
  };

  const handleSettingChange = (setting: string, value: any) => {
    setSecuritySettings(prev => ({
      ...prev,
      [setting]: value
    }));
  };

  const testQuerySecurity = () => {
    if (!testQuery.trim()) return;

    const result = validateNaturalLanguageQuery(testQuery);
    setTestResult(result);
  };

  const clearSecurityLogs = () => {
    Modal.confirm({
      title: 'Clear Security Logs',
      content: 'Are you sure you want to clear all security event logs? This action cannot be undone.',
      okText: 'Clear Logs',
      okType: 'danger',
      onOk: () => {
        setSecurityEvents([]);
      }
    });
  };

  const refreshTokens = async () => {
    try {
      await tokenManager.refreshAccessToken();
      loadSecurityMetrics();
    } catch (error) {
      console.error('Failed to refresh tokens:', error);
    }
  };

  const getSecurityScoreColor = (score: number): string => {
    if (score >= 90) return '#52c41a';
    if (score >= 70) return '#faad14';
    return '#ff4d4f';
  };

  const getSeverityColor = (severity: string): string => {
    switch (severity) {
      case 'high': return 'red';
      case 'medium': return 'orange';
      case 'low': return 'green';
      default: return 'default';
    }
  };

  const securityEventColumns = [
    {
      title: 'Time',
      dataIndex: 'timestamp',
      key: 'timestamp',
      render: (timestamp: string) => new Date(timestamp).toLocaleString(),
      width: 150
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => (
        <Tag color="blue">{type.replace('_', ' ').toUpperCase()}</Tag>
      ),
      width: 120
    },
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      render: (severity: string) => (
        <Tag color={getSeverityColor(severity)}>{severity.toUpperCase()}</Tag>
      ),
      width: 100
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description'
    },
    {
      title: 'IP Address',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
      width: 120
    }
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Title level={3}>
        <SafetyOutlined /> Security Dashboard
      </Title>

      {/* Security Score Overview */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Security Score"
              value={metrics.securityScore}
              suffix="/100"
              valueStyle={{ color: getSecurityScoreColor(metrics.securityScore) }}
              prefix={<SafetyOutlined />}
            />
            <Progress
              percent={metrics.securityScore}
              strokeColor={getSecurityScoreColor(metrics.securityScore)}
              size="small"
              style={{ marginTop: '8px' }}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Blocked Queries"
              value={metrics.blockedQueries}
              valueStyle={{ color: '#ff4d4f' }}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Active Tokens"
              value={metrics.activeTokens}
              valueStyle={{ color: '#1890ff' }}
              prefix={<KeyOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Failed Logins"
              value={metrics.failedLogins}
              valueStyle={{ color: '#faad14' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Security Settings */}
      <Card title="Security Settings" style={{ marginBottom: '24px' }}>
        <Row gutter={[16, 16]}>
          <Col xs={24} md={12}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text strong>Query Validation</Text>
                <Switch
                  checked={securitySettings.enableQueryValidation}
                  onChange={(checked) => handleSettingChange('enableQueryValidation', checked)}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text strong>Token Rotation</Text>
                <Switch
                  checked={securitySettings.enableTokenRotation}
                  onChange={(checked) => handleSettingChange('enableTokenRotation', checked)}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text strong>Rate Limiting</Text>
                <Switch
                  checked={securitySettings.enableRateLimiting}
                  onChange={(checked) => handleSettingChange('enableRateLimiting', checked)}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text strong>Security Logging</Text>
                <Switch
                  checked={securitySettings.enableSecurityLogging}
                  onChange={(checked) => handleSettingChange('enableSecurityLogging', checked)}
                />
              </div>
            </Space>
          </Col>

          <Col xs={24} md={12}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button
                type="primary"
                icon={<EyeOutlined />}
                onClick={() => setTestQueryVisible(true)}
              >
                Test Query Security
              </Button>

              <Button
                icon={<ReloadOutlined />}
                onClick={refreshTokens}
              >
                Refresh Tokens
              </Button>

              <Button
                danger
                onClick={clearSecurityLogs}
              >
                Clear Security Logs
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Security Events */}
      <Card title="Recent Security Events">
        <Table
          dataSource={securityEvents}
          columns={securityEventColumns}
          rowKey="id"
          size="small"
          pagination={{ pageSize: 10 }}
        />
      </Card>

      {/* Test Query Modal */}
      <Modal
        title="Test Query Security"
        open={testQueryVisible}
        onCancel={() => setTestQueryVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setTestQueryVisible(false)}>
            Cancel
          </Button>,
          <Button key="test" type="primary" onClick={testQuerySecurity}>
            Test Query
          </Button>
        ]}
        width={800}
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <TextArea
            rows={4}
            placeholder="Enter a query to test security validation..."
            value={testQuery}
            onChange={(e) => setTestQuery(e.target.value)}
          />

          {testResult && (
            <Alert
              type={testResult.isValid ? 'success' : 'error'}
              message={testResult.isValid ? 'Query Passed Security Validation' : 'Query Failed Security Validation'}
              description={
                <div>
                  <div><strong>Risk Level:</strong> {testResult.riskLevel}</div>
                  {testResult.errors.length > 0 && (
                    <div><strong>Errors:</strong> {testResult.errors.join(', ')}</div>
                  )}
                  {testResult.warnings.length > 0 && (
                    <div><strong>Warnings:</strong> {testResult.warnings.join(', ')}</div>
                  )}
                </div>
              }
              showIcon
            />
          )}
        </Space>
      </Modal>
    </div>
  );
};
