/**
 * Security Audit Dashboard
 * 
 * Advanced security monitoring and audit dashboard for Phase 8
 * featuring vulnerability scanning, policy enforcement, and compliance tracking
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  Alert,
  Button,
  Space,
  Typography,
  Table,
  Tag,
  Tabs,
  Badge,
  Timeline,
  Tooltip,
  Modal,
  Descriptions
} from 'antd';
import {
  ShieldOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BugOutlined,
  LockOutlined,
  AuditOutlined,
  DownloadOutlined,
  ReloadOutlined,
  EyeOutlined,
  SafetyOutlined
} from '@ant-design/icons';
import { securityAudit } from '../../security/SecurityAuditSystem';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

interface SecurityAuditDashboardProps {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const SecurityAuditDashboard: React.FC<SecurityAuditDashboardProps> = ({
  autoRefresh = true,
  refreshInterval = 300000 // 5 minutes
}) => {
  const [auditReport, setAuditReport] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [selectedVulnerability, setSelectedVulnerability] = useState<any>(null);
  const [modalVisible, setModalVisible] = useState(false);

  const runSecurityAudit = useCallback(async () => {
    setLoading(true);
    try {
      const report = await securityAudit.generateAuditReport();
      setAuditReport(report);
    } catch (error) {
      console.error('Security audit failed:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    runSecurityAudit();
    
    if (autoRefresh) {
      const interval = setInterval(runSecurityAudit, refreshInterval);
      return () => clearInterval(interval);
    }
  }, [runSecurityAudit, autoRefresh, refreshInterval]);

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return '#ff4d4f';
      case 'high': return '#ff7a45';
      case 'medium': return '#faad14';
      case 'low': return '#52c41a';
      default: return '#d9d9d9';
    }
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'critical': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'high': return <WarningOutlined style={{ color: '#ff7a45' }} />;
      case 'medium': return <BugOutlined style={{ color: '#faad14' }} />;
      case 'low': return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      default: return <CheckCircleOutlined />;
    }
  };

  const getSecurityScoreColor = (score: number) => {
    if (score >= 90) return '#52c41a';
    if (score >= 70) return '#faad14';
    if (score >= 50) return '#ff7a45';
    return '#ff4d4f';
  };

  const exportAuditData = () => {
    const data = securityAudit.exportAuditData();
    const blob = new Blob([data], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `security-audit-${new Date().toISOString()}.json`;
    link.click();
    URL.revokeObjectURL(url);
  };

  const renderSecurityOverview = () => {
    if (!auditReport) return null;

    const { vulnerabilities, securityScore, complianceStatus } = auditReport;
    const criticalCount = vulnerabilities.filter((v: any) => v.severity === 'critical').length;
    const highCount = vulnerabilities.filter((v: any) => v.severity === 'high').length;
    const mediumCount = vulnerabilities.filter((v: any) => v.severity === 'medium').length;
    const lowCount = vulnerabilities.filter((v: any) => v.severity === 'low').length;

    return (
      <Row gutter={[24, 24]}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Security Score"
              value={securityScore}
              suffix="/100"
              prefix={<ShieldOutlined />}
              valueStyle={{ color: getSecurityScoreColor(securityScore) }}
            />
            <Progress
              percent={securityScore}
              strokeColor={getSecurityScoreColor(securityScore)}
              size="small"
              style={{ marginTop: '8px' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Critical Issues"
              value={criticalCount}
              prefix={<ExclamationCircleOutlined />}
              valueStyle={{ color: criticalCount > 0 ? '#ff4d4f' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="High Priority"
              value={highCount}
              prefix={<WarningOutlined />}
              valueStyle={{ color: highCount > 0 ? '#ff7a45' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Vulnerabilities"
              value={vulnerabilities.length}
              prefix={<BugOutlined />}
              valueStyle={{ color: vulnerabilities.length > 0 ? '#faad14' : '#52c41a' }}
            />
          </Card>
        </Col>
      </Row>
    );
  };

  const renderVulnerabilitiesTable = () => {
    if (!auditReport) return null;

    const columns = [
      {
        title: 'Severity',
        dataIndex: 'severity',
        key: 'severity',
        render: (severity: string) => (
          <Space>
            {getSeverityIcon(severity)}
            <Tag color={getSeverityColor(severity)}>{severity.toUpperCase()}</Tag>
          </Space>
        ),
        sorter: (a: any, b: any) => {
          const order = { critical: 4, high: 3, medium: 2, low: 1 };
          return (order as any)[b.severity] - (order as any)[a.severity];
        }
      },
      {
        title: 'Type',
        dataIndex: 'type',
        key: 'type',
        render: (type: string) => <Tag>{type.toUpperCase()}</Tag>
      },
      {
        title: 'Description',
        dataIndex: 'description',
        key: 'description',
        ellipsis: true
      },
      {
        title: 'Location',
        dataIndex: 'location',
        key: 'location',
        ellipsis: true
      },
      {
        title: 'CVSS',
        dataIndex: 'cvss',
        key: 'cvss',
        render: (cvss: number) => cvss ? cvss.toFixed(1) : 'N/A'
      },
      {
        title: 'Action',
        key: 'action',
        render: (record: any) => (
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => {
              setSelectedVulnerability(record);
              setModalVisible(true);
            }}
          >
            Details
          </Button>
        )
      }
    ];

    return (
      <Table
        dataSource={auditReport.vulnerabilities}
        columns={columns}
        rowKey="id"
        size="small"
        pagination={{ pageSize: 10 }}
        loading={loading}
      />
    );
  };

  const renderComplianceStatus = () => {
    if (!auditReport) return null;

    const { complianceStatus } = auditReport;
    const complianceItems = [
      { name: 'OWASP Top 10', status: complianceStatus.owasp, key: 'owasp' },
      { name: 'GDPR', status: complianceStatus.gdpr, key: 'gdpr' },
      { name: 'HIPAA', status: complianceStatus.hipaa, key: 'hipaa' },
      { name: 'SOX', status: complianceStatus.sox, key: 'sox' }
    ];

    return (
      <Row gutter={[16, 16]}>
        {complianceItems.map(item => (
          <Col xs={12} md={6} key={item.key}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%', textAlign: 'center' }}>
                <div style={{ fontSize: '24px' }}>
                  {item.status ? (
                    <CheckCircleOutlined style={{ color: '#52c41a' }} />
                  ) : (
                    <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
                  )}
                </div>
                <Text strong>{item.name}</Text>
                <Tag color={item.status ? 'success' : 'error'}>
                  {item.status ? 'Compliant' : 'Non-Compliant'}
                </Tag>
              </Space>
            </Card>
          </Col>
        ))}
      </Row>
    );
  };

  const renderPolicyViolations = () => {
    if (!auditReport) return null;

    const { policyViolations } = auditReport;

    return (
      <Timeline>
        {policyViolations.map((violation: any, index: number) => (
          <Timeline.Item
            key={index}
            dot={getSeverityIcon(violation.severity)}
            color={getSeverityColor(violation.severity)}
          >
            <div>
              <Text strong>{violation.policy}</Text>
              <br />
              <Text type="secondary">{violation.rule}</Text>
              <br />
              <Tag color={getSeverityColor(violation.severity)} size="small">
                {violation.severity.toUpperCase()}
              </Tag>
            </div>
          </Timeline.Item>
        ))}
        {policyViolations.length === 0 && (
          <Timeline.Item dot={<CheckCircleOutlined />} color="green">
            <Text>No policy violations detected</Text>
          </Timeline.Item>
        )}
      </Timeline>
    );
  };

  const renderRecommendations = () => {
    if (!auditReport) return null;

    const { recommendations } = auditReport;

    return (
      <Space direction="vertical" style={{ width: '100%' }}>
        {recommendations.map((recommendation: string, index: number) => (
          <Alert
            key={index}
            message={recommendation}
            type="info"
            showIcon
            size="small"
          />
        ))}
        {recommendations.length === 0 && (
          <Alert
            message="No security recommendations at this time"
            type="success"
            showIcon
          />
        )}
      </Space>
    );
  };

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Title level={2}>
            <ShieldOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
            Security Audit Dashboard
          </Title>
          <Paragraph>
            Advanced security monitoring and vulnerability assessment for Phase 8
          </Paragraph>
        </div>
        <Space>
          <Button
            type="primary"
            icon={<ReloadOutlined />}
            onClick={runSecurityAudit}
            loading={loading}
          >
            Run Security Audit
          </Button>
          <Button
            icon={<DownloadOutlined />}
            onClick={exportAuditData}
          >
            Export Report
          </Button>
        </Space>
      </div>

      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {renderSecurityOverview()}

        <Tabs defaultActiveKey="vulnerabilities">
          <TabPane tab={<span><BugOutlined />Vulnerabilities</span>} key="vulnerabilities">
            <Card title="Security Vulnerabilities" size="small">
              {renderVulnerabilitiesTable()}
            </Card>
          </TabPane>

          <TabPane tab={<span><SafetyOutlined />Compliance</span>} key="compliance">
            <Card title="Compliance Status" size="small">
              {renderComplianceStatus()}
            </Card>
          </TabPane>

          <TabPane tab={<span><AuditOutlined />Policy Violations</span>} key="policies">
            <Card title="Security Policy Violations" size="small">
              {renderPolicyViolations()}
            </Card>
          </TabPane>

          <TabPane tab={<span><LockOutlined />Recommendations</span>} key="recommendations">
            <Card title="Security Recommendations" size="small">
              {renderRecommendations()}
            </Card>
          </TabPane>
        </Tabs>
      </Space>

      <Modal
        title="Vulnerability Details"
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setModalVisible(false)}>
            Close
          </Button>
        ]}
        width={600}
      >
        {selectedVulnerability && (
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Severity">
              <Space>
                {getSeverityIcon(selectedVulnerability.severity)}
                <Tag color={getSeverityColor(selectedVulnerability.severity)}>
                  {selectedVulnerability.severity.toUpperCase()}
                </Tag>
              </Space>
            </Descriptions.Item>
            <Descriptions.Item label="Type">
              {selectedVulnerability.type.toUpperCase()}
            </Descriptions.Item>
            <Descriptions.Item label="Description">
              {selectedVulnerability.description}
            </Descriptions.Item>
            <Descriptions.Item label="Location">
              {selectedVulnerability.location}
            </Descriptions.Item>
            <Descriptions.Item label="Recommendation">
              {selectedVulnerability.recommendation}
            </Descriptions.Item>
            {selectedVulnerability.cwe && (
              <Descriptions.Item label="CWE">
                {selectedVulnerability.cwe}
              </Descriptions.Item>
            )}
            {selectedVulnerability.cvss && (
              <Descriptions.Item label="CVSS Score">
                {selectedVulnerability.cvss}
              </Descriptions.Item>
            )}
          </Descriptions>
        )}
      </Modal>
    </div>
  );
};

export default SecurityAuditDashboard;
