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
  Modal,
  Input
} from 'antd';
import {
  SafetyOutlined,
  WarningOutlined,
  ExclamationCircleOutlined,
  LockOutlined,
  KeyOutlined,
  EyeOutlined,
  SettingOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { validateNaturalLanguageQuery } from '../../utils/queryValidator';
import { tokenManager } from '../../services/tokenManager';
import { secureApiClient, RequestMetrics } from '../../services/secureApiClient';
import { requestSigning, SigningConfig } from '../../services/requestSigning';

const { Title, Text } = Typography;
const { TextArea } = Input;

interface SecurityMetrics {
  totalQueries: number;
  blockedQueries: number;
  warningQueries: number;
  securityScore: number;
  lastSecurityScan: string;
  activeTokens: number;
  failedLogins: number;
  // New request signing metrics
  totalRequests: number;
  signedRequests: number;
  encryptedRequests: number;
  averageResponseTime: number;
  rateLimitHits: number;
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
    failedLogins: 0,
    // New request signing metrics
    totalRequests: 0,
    signedRequests: 0,
    encryptedRequests: 0,
    averageResponseTime: 0,
    rateLimitHits: 0,
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

  // New state for request signing features
  const [, setRequestMetrics] = useState<RequestMetrics[]>([]);
  const [signingConfig, setSigningConfig] = useState<SigningConfig | null>(null);
  const [signingConfigVisible, setSigningConfigVisible] = useState(false);

  useEffect(() => {
    loadSecurityMetrics();
    loadSecurityEvents();
    loadRequestMetrics();
    loadSigningConfig();

    // Set up periodic refresh for request metrics
    const interval = setInterval(loadRequestMetrics, 30000); // 30 seconds
    return () => clearInterval(interval);
  }, []);

  const loadSecurityMetrics = async () => {
    // Get request metrics from secure API client
    const apiMetrics = secureApiClient.getRequestMetrics();

    const mockMetrics: SecurityMetrics = {
      totalQueries: 1247,
      blockedQueries: 23,
      warningQueries: 156,
      securityScore: calculateSecurityScore(),
      lastSecurityScan: new Date().toISOString(),
      activeTokens: 12,
      failedLogins: 3,
      // New request signing metrics
      totalRequests: apiMetrics.length,
      signedRequests: apiMetrics.filter(m => m.signed).length,
      encryptedRequests: apiMetrics.filter(m => m.encrypted).length,
      averageResponseTime: apiMetrics.length > 0
        ? Math.round(apiMetrics.reduce((sum, m) => sum + m.duration, 0) / apiMetrics.length)
        : 0,
      rateLimitHits: apiMetrics.filter(m => m.status === 429).length,
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

  const loadRequestMetrics = () => {
    try {
      const metrics = secureApiClient.getRequestMetrics();
      setRequestMetrics(metrics);
    } catch (error) {
      console.error('Failed to load request metrics:', error);
    }
  };

  const loadSigningConfig = () => {
    try {
      const config = requestSigning.getConfig();
      setSigningConfig(config);
    } catch (error) {
      console.error('Failed to load signing config:', error);
    }
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
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <SafetyOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Security Dashboard
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Monitor security metrics, manage access controls, and track security events
        </p>
      </div>

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

      {/* Request Signing Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Requests"
              value={metrics.totalRequests}
              valueStyle={{ color: '#1890ff' }}
              prefix={<EyeOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Signed Requests"
              value={metrics.signedRequests}
              suffix={`/ ${metrics.totalRequests}`}
              valueStyle={{ color: '#52c41a' }}
              prefix={<KeyOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Encrypted Requests"
              value={metrics.encryptedRequests}
              suffix={`/ ${metrics.totalRequests}`}
              valueStyle={{ color: '#722ed1' }}
              prefix={<LockOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={metrics.averageResponseTime}
              suffix="ms"
              valueStyle={{ color: '#faad14' }}
              prefix={<SettingOutlined />}
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
                icon={<KeyOutlined />}
                onClick={() => setSigningConfigVisible(true)}
              >
                Configure Request Signing
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

      {/* Request Signing Configuration Modal */}
      <Modal
        title="Request Signing Configuration"
        open={signingConfigVisible}
        onCancel={() => setSigningConfigVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setSigningConfigVisible(false)}>
            Cancel
          </Button>,
          <Button
            key="save"
            type="primary"
            onClick={() => {
              // Save configuration logic would go here
              setSigningConfigVisible(false);
            }}
          >
            Save Configuration
          </Button>
        ]}
        width={600}
      >
        {signingConfig && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <Alert
              message="Request Signing Configuration"
              description="Configure cryptographic signing for API requests to enhance security."
              type="info"
              showIcon
            />

            <div>
              <Text strong>Algorithm:</Text> {signingConfig.algorithm}
            </div>
            <div>
              <Text strong>Include Body:</Text> {signingConfig.includeBody ? 'Yes' : 'No'}
            </div>
            <div>
              <Text strong>Timestamp Tolerance:</Text> {signingConfig.timestampTolerance} seconds
            </div>
            <div>
              <Text strong>Nonce Length:</Text> {signingConfig.nonceLength} bytes
            </div>
            <div>
              <Text strong>Include Headers:</Text> {signingConfig.includeHeaders.join(', ')}
            </div>
          </Space>
        )}
      </Modal>
    </div>
  );
};
