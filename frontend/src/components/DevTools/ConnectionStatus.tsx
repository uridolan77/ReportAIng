import React, { useState, useEffect } from 'react';
import { Card, Button, Alert, Spin, Typography, Space, Tag, Divider } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined, ReloadOutlined, ApiOutlined } from '@ant-design/icons';
import { ConnectionTester, ConnectionTestResult } from '../../utils/connectionTest';

const { Text, Paragraph } = Typography;

interface ConnectionStatusProps {
  showDetails?: boolean;
}

export const ConnectionStatus: React.FC<ConnectionStatusProps> = ({ showDetails = true }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [testResults, setTestResults] = useState<ConnectionTestResult[]>([]);
  const [lastTestTime, setLastTestTime] = useState<string | null>(null);

  const runConnectionTests = async () => {
    setIsLoading(true);
    try {
      const results = await ConnectionTester.runAllTests();
      setTestResults(results);
      setLastTestTime(new Date().toLocaleString());

      // Also display in console for debugging
      ConnectionTester.displayResults(results);
    } catch (error) {
      console.error('Error running connection tests:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    // Run tests on component mount
    runConnectionTests();
  }, []);

  const getOverallStatus = () => {
    if (testResults.length === 0) return 'unknown';
    const allPassed = testResults.every(result => result.success);
    return allPassed ? 'success' : 'error';
  };

  const getStatusIcon = (success: boolean) => {
    return success ? (
      <CheckCircleOutlined style={{ color: '#52c41a' }} />
    ) : (
      <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
    );
  };

  const overallStatus = getOverallStatus();

  return (
    <Card
      title={
        <Space>
          <ApiOutlined />
          <span>API Connection Status</span>
          {overallStatus === 'success' && <Tag color="success">Connected</Tag>}
          {overallStatus === 'error' && <Tag color="error">Connection Issues</Tag>}
          {overallStatus === 'unknown' && <Tag color="processing">Testing...</Tag>}
        </Space>
      }
      extra={
        <Button
          icon={<ReloadOutlined />}
          onClick={runConnectionTests}
          loading={isLoading}
          size="small"
        >
          Test Connection
        </Button>
      }
      style={{ marginBottom: 16 }}
    >
      {isLoading && (
        <div style={{ textAlign: 'center', padding: '20px' }}>
          <Spin size="large" />
          <Paragraph style={{ marginTop: 16 }}>Testing API connection...</Paragraph>
        </div>
      )}

      {!isLoading && testResults.length > 0 && (
        <>
          {overallStatus === 'success' && (
            <Alert
              message="API Connection Successful"
              description="All connection tests passed. The frontend can communicate with the backend API."
              type="success"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {overallStatus === 'error' && (
            <Alert
              message="API Connection Issues Detected"
              description="Some connection tests failed. Check the details below for troubleshooting information."
              type="error"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {showDetails && (
            <>
              <Divider orientation="left">Test Results</Divider>
              <Space direction="vertical" style={{ width: '100%' }}>
                {testResults.map((result, index) => (
                  <Card
                    key={index}
                    size="small"
                    title={
                      <Space>
                        {getStatusIcon(result.success)}
                        <Text strong>Test {index + 1}</Text>
                      </Space>
                    }
                    style={{
                      borderColor: result.success ? '#52c41a' : '#ff4d4f',
                      borderWidth: 2
                    }}
                  >
                    <Paragraph>
                      <Text strong>Message:</Text> {result.message}
                    </Paragraph>

                    <Paragraph>
                      <Text strong>Timestamp:</Text> {result.timestamp}
                    </Paragraph>

                    {result.details && (
                      <details>
                        <summary style={{ cursor: 'pointer', marginBottom: 8 }}>
                          <Text type="secondary">View Details</Text>
                        </summary>
                        <pre style={{
                          background: '#f5f5f5',
                          padding: 8,
                          borderRadius: 4,
                          fontSize: '12px',
                          overflow: 'auto'
                        }}>
                          {JSON.stringify(result.details, null, 2)}
                        </pre>
                      </details>
                    )}
                  </Card>
                ))}
              </Space>
            </>
          )}

          {lastTestTime && (
            <Paragraph style={{ marginTop: 16, textAlign: 'center' }}>
              <Text type="secondary">Last tested: {lastTestTime}</Text>
            </Paragraph>
          )}
        </>
      )}

      {!isLoading && testResults.length === 0 && (
        <Alert
          message="No Test Results"
          description="Click 'Test Connection' to check the API connectivity."
          type="info"
          showIcon
        />
      )}
    </Card>
  );
};

export default ConnectionStatus;
