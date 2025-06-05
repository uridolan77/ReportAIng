/**
 * Request Signing Demo Component
 * Demonstrates the request signing functionality with interactive examples
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  Alert,
  Table,
  Tag,
  Tooltip,
  Modal,
  Form,
  Input,
  Select,
  Divider,
  Row,
  Col,
  Statistic,
} from 'antd';
import {
  KeyOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  SettingOutlined,
  PlayCircleOutlined,
  SecurityScanOutlined,
} from '@ant-design/icons';
import { requestSigning, SignedRequest } from '../../services/requestSigning';

const { Title, Text } = Typography;
const { TextArea } = Input;

interface DemoRequest {
  id: string;
  method: string;
  url: string;
  body?: any;
  timestamp: number;
  signed: boolean;
  verified: boolean;
  signature?: string;
  duration: number;
}

export const RequestSigningDemo: React.FC = () => {
  const [demoRequests, setDemoRequests] = useState<DemoRequest[]>([]);
  const [isInitialized, setIsInitialized] = useState(false);
  const [testModalVisible, setTestModalVisible] = useState(false);
  const [form] = Form.useForm();
  const [testResult, setTestResult] = useState<SignedRequest | null>(null);
  const [verificationResult, setVerificationResult] = useState<boolean | null>(null);

  useEffect(() => {
    initializeRequestSigning();
  }, []);

  const initializeRequestSigning = async () => {
    try {
      await requestSigning.initialize();
      setIsInitialized(true);
    } catch (error) {
      console.error('Failed to initialize request signing:', error);
    }
  };

  const generateDemoRequest = async () => {
    const methods = ['GET', 'POST', 'PUT', 'DELETE'];
    const urls = [
      '/api/query/execute',
      '/api/schema/metadata',
      '/api/dashboard/overview',
      '/api/auth/refresh',
      '/api/health/status',
    ];

    const method = methods[Math.floor(Math.random() * methods.length)];
    const url = urls[Math.floor(Math.random() * urls.length)];
    const body = method !== 'GET' ? { data: 'sample data', timestamp: Date.now() } : undefined;

    const startTime = Date.now();
    
    try {
      const signedRequest = await requestSigning.signRequest(method, url, {}, body);
      const verified = await requestSigning.verifyRequest(signedRequest);
      const duration = Date.now() - startTime;

      const demoRequest: DemoRequest = {
        id: `demo_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
        method,
        url,
        body,
        timestamp: startTime,
        signed: true,
        verified,
        signature: signedRequest.signature.substring(0, 16) + '...',
        duration,
      };

      setDemoRequests(prev => [demoRequest, ...prev.slice(0, 9)]); // Keep last 10
    } catch (error) {
      console.error('Failed to generate demo request:', error);
    }
  };

  const testCustomRequest = async (values: any) => {
    try {
      const startTime = Date.now();
      const signedRequest = await requestSigning.signRequest(
        values.method,
        values.url,
        { 'content-type': 'application/json' },
        values.body ? JSON.parse(values.body) : undefined
      );

      const verified = await requestSigning.verifyRequest(signedRequest);
      setTestResult(signedRequest);
      setVerificationResult(verified);

      // Add to demo requests
      const demoRequest: DemoRequest = {
        id: `custom_${Date.now()}`,
        method: values.method,
        url: values.url,
        body: values.body ? JSON.parse(values.body) : undefined,
        timestamp: startTime,
        signed: true,
        verified,
        signature: signedRequest.signature.substring(0, 16) + '...',
        duration: Date.now() - startTime,
      };

      setDemoRequests(prev => [demoRequest, ...prev.slice(0, 9)]);
    } catch (error) {
      console.error('Failed to test custom request:', error);
      Modal.error({
        title: 'Request Signing Failed',
        content: 'Failed to sign the custom request. Please check your input.',
      });
    }
  };

  const clearDemoRequests = () => {
    setDemoRequests([]);
  };

  const getSigningStats = () => {
    const total = demoRequests.length;
    const signed = demoRequests.filter(r => r.signed).length;
    const verified = demoRequests.filter(r => r.verified).length;
    const avgDuration = total > 0 
      ? Math.round(demoRequests.reduce((sum, r) => sum + r.duration, 0) / total)
      : 0;

    return { total, signed, verified, avgDuration };
  };

  const stats = getSigningStats();

  const columns = [
    {
      title: 'Method',
      dataIndex: 'method',
      key: 'method',
      width: 80,
      render: (method: string) => <Tag color="blue">{method}</Tag>,
    },
    {
      title: 'URL',
      dataIndex: 'url',
      key: 'url',
      ellipsis: true,
    },
    {
      title: 'Signed',
      dataIndex: 'signed',
      key: 'signed',
      width: 80,
      render: (signed: boolean) => (
        <Tooltip title={signed ? 'Request was signed' : 'Request was not signed'}>
          {signed ? (
            <CheckCircleOutlined style={{ color: '#52c41a' }} />
          ) : (
            <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
          )}
        </Tooltip>
      ),
    },
    {
      title: 'Verified',
      dataIndex: 'verified',
      key: 'verified',
      width: 80,
      render: (verified: boolean) => (
        <Tooltip title={verified ? 'Signature verified' : 'Signature verification failed'}>
          {verified ? (
            <CheckCircleOutlined style={{ color: '#52c41a' }} />
          ) : (
            <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
          )}
        </Tooltip>
      ),
    },
    {
      title: 'Signature',
      dataIndex: 'signature',
      key: 'signature',
      width: 120,
      render: (signature: string) => <Text code>{signature}</Text>,
    },
    {
      title: 'Duration',
      dataIndex: 'duration',
      key: 'duration',
      width: 100,
      render: (duration: number) => `${duration}ms`,
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>
        <SecurityScanOutlined /> Request Signing Demo
      </Title>

      <Alert
        message="Request Signing Security Feature"
        description="This demo showcases cryptographic request signing that adds HMAC signatures to API calls for enhanced security and integrity verification."
        type="info"
        showIcon
        style={{ marginBottom: '24px' }}
      />

      {/* Statistics */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Total Requests"
              value={stats.total}
              prefix={<EyeOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Signed Requests"
              value={stats.signed}
              valueStyle={{ color: '#52c41a' }}
              prefix={<KeyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Verified Signatures"
              value={stats.verified}
              valueStyle={{ color: '#1890ff' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Avg Signing Time"
              value={stats.avgDuration}
              suffix="ms"
              prefix={<SettingOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Controls */}
      <Card title="Demo Controls" style={{ marginBottom: '24px' }}>
        <Space>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={generateDemoRequest}
            disabled={!isInitialized}
          >
            Generate Demo Request
          </Button>
          
          <Button
            icon={<SettingOutlined />}
            onClick={() => setTestModalVisible(true)}
            disabled={!isInitialized}
          >
            Test Custom Request
          </Button>
          
          <Button onClick={clearDemoRequests}>
            Clear Demo Requests
          </Button>
        </Space>

        {!isInitialized && (
          <Alert
            message="Initializing Request Signing"
            description="Please wait while the request signing service is being initialized..."
            type="warning"
            showIcon
            style={{ marginTop: '16px' }}
          />
        )}
      </Card>

      {/* Demo Requests Table */}
      <Card title="Demo Requests">
        <Table
          columns={columns}
          dataSource={demoRequests}
          rowKey="id"
          size="small"
          pagination={false}
          locale={{ emptyText: 'No demo requests generated yet' }}
        />
      </Card>

      {/* Custom Request Test Modal */}
      <Modal
        title="Test Custom Request Signing"
        open={testModalVisible}
        onCancel={() => {
          setTestModalVisible(false);
          setTestResult(null);
          setVerificationResult(null);
          form.resetFields();
        }}
        footer={[
          <Button key="cancel" onClick={() => setTestModalVisible(false)}>
            Cancel
          </Button>,
          <Button key="test" type="primary" onClick={() => form.submit()}>
            Sign & Test Request
          </Button>,
        ]}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={testCustomRequest}
          initialValues={{ method: 'POST' }}
        >
          <Form.Item
            name="method"
            label="HTTP Method"
            rules={[{ required: true }]}
          >
            <Select>
              <Select.Option value="GET">GET</Select.Option>
              <Select.Option value="POST">POST</Select.Option>
              <Select.Option value="PUT">PUT</Select.Option>
              <Select.Option value="DELETE">DELETE</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="url"
            label="URL"
            rules={[{ required: true, message: 'Please enter a URL' }]}
          >
            <Input placeholder="/api/example/endpoint" />
          </Form.Item>

          <Form.Item
            name="body"
            label="Request Body (JSON)"
          >
            <TextArea
              rows={4}
              placeholder='{"key": "value"}'
            />
          </Form.Item>
        </Form>

        {testResult && (
          <div style={{ marginTop: '16px' }}>
            <Divider>Signing Result</Divider>
            
            <Alert
              type={verificationResult ? 'success' : 'error'}
              message={verificationResult ? 'Request Signed & Verified Successfully' : 'Signature Verification Failed'}
              style={{ marginBottom: '16px' }}
            />

            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Timestamp:</Text> {new Date(testResult.timestamp).toLocaleString()}
              </div>
              <div>
                <Text strong>Nonce:</Text> <Text code>{testResult.nonce}</Text>
              </div>
              <div>
                <Text strong>Signature:</Text> <Text code>{testResult.signature.substring(0, 32)}...</Text>
              </div>
              <div>
                <Text strong>Verification:</Text> {verificationResult ? 
                  <Tag color="green">PASSED</Tag> : 
                  <Tag color="red">FAILED</Tag>
                }
              </div>
            </Space>
          </div>
        )}
      </Modal>
    </div>
  );
};
