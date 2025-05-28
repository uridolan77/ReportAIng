import React, { useState, useEffect, useCallback } from 'react';
import { Card, Alert, Spin, Typography, Space, Tag, Button, Divider } from 'antd';
import { KeyOutlined, ReloadOutlined, CheckCircleOutlined, CloseCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';

const { Text, Paragraph } = Typography;

interface KeyVaultStatusProps {
  showDetails?: boolean;
}

interface ConfigurationTest {
  name: string;
  status: 'success' | 'warning' | 'error';
  message: string;
  details?: any;
}

export const KeyVaultStatus: React.FC<KeyVaultStatusProps> = ({ showDetails = true }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [tests, setTests] = useState<ConfigurationTest[]>([]);
  const [lastCheckTime, setLastCheckTime] = useState<string | null>(null);

  const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:55243';

  const runConfigurationTests = useCallback(async () => {
    setIsLoading(true);
    const results: ConfigurationTest[] = [];

    try {
      // Test 1: Check if API can resolve configuration
      try {
        const response = await fetch(`${API_BASE_URL}/api/health/detailed`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
          credentials: 'include',
        });

        if (response.ok) {
          const data = await response.json();
          results.push({
            name: 'API Configuration Access',
            status: 'success',
            message: 'API can access configuration successfully',
            details: {
              environment: data.environment,
              timestamp: data.timestamp
            }
          });
        } else {
          results.push({
            name: 'API Configuration Access',
            status: 'error',
            message: `Failed to access API configuration: ${response.status} ${response.statusText}`,
          });
        }
      } catch (error: any) {
        results.push({
          name: 'API Configuration Access',
          status: 'error',
          message: `Error accessing API: ${error.message}`,
        });
      }

      // Test 2: Check database connection string resolution
      try {
        const response = await fetch(`${API_BASE_URL}/health`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
          credentials: 'include',
        });

        if (response.ok) {
          const data = await response.json();
          const dbChecks = data.checks?.filter((check: any) =>
            check.name.toLowerCase().includes('database') ||
            check.name === 'bidatabase'
          ) || [];

          if (dbChecks.length > 0) {
            const biDbCheck = dbChecks.find((check: any) => check.name === 'bidatabase');
            if (biDbCheck) {
              if (biDbCheck.status === 'Healthy') {
                results.push({
                  name: 'Database Connection Resolution',
                  status: 'success',
                  message: 'Database connection string resolved and working',
                  details: {
                    description: biDbCheck.description,
                    duration: biDbCheck.duration
                  }
                });
              } else if (biDbCheck.status === 'Degraded') {
                results.push({
                  name: 'Database Connection Resolution',
                  status: 'warning',
                  message: 'Database connection partially working but degraded',
                  details: {
                    description: biDbCheck.description,
                    exception: biDbCheck.exception,
                    duration: biDbCheck.duration
                  }
                });
              } else {
                results.push({
                  name: 'Database Connection Resolution',
                  status: 'error',
                  message: 'Database connection failed - likely Key Vault authentication issue',
                  details: {
                    description: biDbCheck.description,
                    exception: biDbCheck.exception,
                    possibleCauses: [
                      'Azure Key Vault authentication failed',
                      'Key Vault secrets not accessible',
                      'Connection string placeholders not resolved',
                      'Database server unreachable',
                      'Invalid credentials in Key Vault'
                    ]
                  }
                });
              }
            } else {
              results.push({
                name: 'Database Connection Resolution',
                status: 'warning',
                message: 'BIDatabase health check not found',
                details: {
                  availableChecks: dbChecks.map((check: any) => check.name)
                }
              });
            }
          } else {
            results.push({
              name: 'Database Connection Resolution',
              status: 'warning',
              message: 'No database health checks found',
            });
          }
        } else {
          results.push({
            name: 'Database Connection Resolution',
            status: 'error',
            message: `Health check failed: ${response.status} ${response.statusText}`,
          });
        }
      } catch (error: any) {
        results.push({
          name: 'Database Connection Resolution',
          status: 'error',
          message: `Error checking database health: ${error.message}`,
        });
      }

      // Test 3: Check OpenAI configuration
      try {
        const response = await fetch(`${API_BASE_URL}/health`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
          credentials: 'include',
        });

        if (response.ok) {
          const data = await response.json();
          const openAICheck = data.checks?.find((check: any) => check.name === 'openai');

          if (openAICheck) {
            if (openAICheck.status === 'Healthy') {
              results.push({
                name: 'OpenAI Configuration',
                status: 'success',
                message: 'OpenAI service configured and working',
                details: {
                  description: openAICheck.description,
                  duration: openAICheck.duration
                }
              });
            } else {
              results.push({
                name: 'OpenAI Configuration',
                status: 'warning',
                message: 'OpenAI service configuration issues',
                details: {
                  description: openAICheck.description,
                  exception: openAICheck.exception,
                  possibleCauses: [
                    'OpenAI API key not configured',
                    'Azure OpenAI endpoint not accessible',
                    'API quota exceeded',
                    'Network connectivity issues'
                  ]
                }
              });
            }
          } else {
            results.push({
              name: 'OpenAI Configuration',
              status: 'warning',
              message: 'OpenAI health check not found',
            });
          }
        }
      } catch (error: any) {
        results.push({
          name: 'OpenAI Configuration',
          status: 'error',
          message: `Error checking OpenAI health: ${error.message}`,
        });
      }

      setTests(results);
      setLastCheckTime(new Date().toLocaleString());
    } catch (error) {
      console.error('Error running configuration tests:', error);
    } finally {
      setIsLoading(false);
    }
  }, [API_BASE_URL]);

  useEffect(() => {
    runConfigurationTests();
  }, [runConfigurationTests]);

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'success':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'warning':
        return <ExclamationCircleOutlined style={{ color: '#faad14' }} />;
      case 'error':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
      default:
        return <ExclamationCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'success':
        return 'success';
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      default:
        return 'default';
    }
  };

  const getOverallStatus = () => {
    if (tests.length === 0) return 'unknown';
    if (tests.some(test => test.status === 'error')) return 'error';
    if (tests.some(test => test.status === 'warning')) return 'warning';
    return 'success';
  };

  const overallStatus = getOverallStatus();

  return (
    <Card
      title={
        <Space>
          <KeyOutlined />
          <span>Configuration & Key Vault Status</span>
          <Tag color={getStatusColor(overallStatus)}>
            {overallStatus === 'success' ? 'Configured' :
             overallStatus === 'warning' ? 'Issues' :
             overallStatus === 'error' ? 'Failed' : 'Unknown'}
          </Tag>
        </Space>
      }
      extra={
        <Button
          icon={<ReloadOutlined />}
          onClick={runConfigurationTests}
          loading={isLoading}
          size="small"
        >
          Test Configuration
        </Button>
      }
      style={{ marginBottom: 16 }}
    >
      {isLoading && (
        <div style={{ textAlign: 'center', padding: '20px' }}>
          <Spin size="large" />
          <Paragraph style={{ marginTop: 16 }}>Testing configuration and Key Vault access...</Paragraph>
        </div>
      )}

      {!isLoading && tests.length > 0 && (
        <>
          {overallStatus === 'success' && (
            <Alert
              message="Configuration Healthy"
              description="All configuration tests passed successfully."
              type="success"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {overallStatus === 'warning' && (
            <Alert
              message="Configuration Issues Detected"
              description="Some configuration tests show warnings. The system may have limited functionality."
              type="warning"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {overallStatus === 'error' && (
            <Alert
              message="Configuration Failures"
              description="Critical configuration issues detected. Key Vault authentication or database connectivity problems."
              type="error"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {showDetails && (
            <>
              <Divider orientation="left">Configuration Test Results</Divider>
              <Space direction="vertical" style={{ width: '100%' }}>
                {tests.map((test, index) => (
                  <Card
                    key={index}
                    size="small"
                    title={
                      <Space>
                        {getStatusIcon(test.status)}
                        <Text strong>{test.name}</Text>
                        <Tag color={getStatusColor(test.status)}>{test.status}</Tag>
                      </Space>
                    }
                    style={{
                      borderColor: test.status === 'success' ? '#52c41a' :
                                  test.status === 'warning' ? '#faad14' : '#ff4d4f',
                      borderWidth: 2
                    }}
                  >
                    <Paragraph>
                      <Text strong>Message:</Text> {test.message}
                    </Paragraph>

                    {test.details && (
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
                          {JSON.stringify(test.details, null, 2)}
                        </pre>
                      </details>
                    )}
                  </Card>
                ))}
              </Space>
            </>
          )}

          {lastCheckTime && (
            <Paragraph style={{ marginTop: 16, textAlign: 'center' }}>
              <Text type="secondary">Last tested: {lastCheckTime}</Text>
            </Paragraph>
          )}
        </>
      )}

      {!isLoading && tests.length === 0 && (
        <Alert
          message="No Test Results"
          description="Click 'Test Configuration' to check the system configuration."
          type="info"
          showIcon
        />
      )}
    </Card>
  );
};

export default KeyVaultStatus;
