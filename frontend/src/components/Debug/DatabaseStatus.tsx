import React, { useState, useEffect, useCallback } from 'react';
import { Card, Alert, Spin, Typography, Space, Tag, Button, Divider } from 'antd';
import { DatabaseOutlined, ReloadOutlined, CheckCircleOutlined, CloseCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';

const { Text, Paragraph } = Typography;

interface DatabaseStatusProps {
  showDetails?: boolean;
}

interface DatabaseHealth {
  name: string;
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  description?: string;
  duration?: number;
  exception?: string;
}

interface HealthCheckResponse {
  status: string;
  timestamp: string;
  totalDuration: number;
  checks: DatabaseHealth[];
}

export const DatabaseStatus: React.FC<DatabaseStatusProps> = ({ showDetails = true }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [healthData, setHealthData] = useState<HealthCheckResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [lastCheckTime, setLastCheckTime] = useState<string | null>(null);

  const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:55243';

  const checkDatabaseHealth = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_BASE_URL}/health`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      if (response.ok) {
        const data: HealthCheckResponse = await response.json();
        setHealthData(data);
        setLastCheckTime(new Date().toLocaleString());
      } else {
        const errorText = await response.text();
        setError(`Health check failed: ${response.status} ${response.statusText} - ${errorText}`);
      }
    } catch (err: any) {
      setError(`Failed to check database health: ${err.message}`);
    } finally {
      setIsLoading(false);
    }
  }, [API_BASE_URL]);

  useEffect(() => {
    checkDatabaseHealth();
  }, [checkDatabaseHealth]);

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Healthy':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'Degraded':
        return <ExclamationCircleOutlined style={{ color: '#faad14' }} />;
      case 'Unhealthy':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
      default:
        return <ExclamationCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Healthy':
        return 'success';
      case 'Degraded':
        return 'warning';
      case 'Unhealthy':
        return 'error';
      default:
        return 'default';
    }
  };

  const getDatabaseChecks = () => {
    if (!healthData) return [];
    return healthData.checks.filter(check =>
      check.name.toLowerCase().includes('database') ||
      check.name.toLowerCase().includes('sql') ||
      check.name === 'bidatabase' ||
      check.name === 'defaultdb'
    );
  };

  const getOverallDatabaseStatus = () => {
    const dbChecks = getDatabaseChecks();
    if (dbChecks.length === 0) return 'Unknown';

    if (dbChecks.some(check => check.status === 'Unhealthy')) return 'Unhealthy';
    if (dbChecks.some(check => check.status === 'Degraded')) return 'Degraded';
    if (dbChecks.every(check => check.status === 'Healthy')) return 'Healthy';

    return 'Unknown';
  };

  const overallStatus = getOverallDatabaseStatus();
  const databaseChecks = getDatabaseChecks();

  return (
    <Card
      title={
        <Space>
          <DatabaseOutlined />
          <span>Database Connection Status</span>
          <Tag color={getStatusColor(overallStatus)}>{overallStatus}</Tag>
        </Space>
      }
      extra={
        <Button
          icon={<ReloadOutlined />}
          onClick={checkDatabaseHealth}
          loading={isLoading}
          size="small"
        >
          Check Status
        </Button>
      }
      style={{ marginBottom: 16 }}
    >
      {isLoading && (
        <div style={{ textAlign: 'center', padding: '20px' }}>
          <Spin size="large" />
          <Paragraph style={{ marginTop: 16 }}>Checking database connections...</Paragraph>
        </div>
      )}

      {error && (
        <Alert
          message="Health Check Failed"
          description={typeof error === 'string' ? error : error?.message || 'An error occurred'}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {!isLoading && healthData && (
        <>
          {overallStatus === 'Healthy' && (
            <Alert
              message="Database Connections Healthy"
              description="All database connections are working properly."
              type="success"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {overallStatus === 'Degraded' && (
            <Alert
              message="Database Connection Issues"
              description="Some database connections are experiencing issues but the system is still operational."
              type="warning"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {overallStatus === 'Unhealthy' && (
            <Alert
              message="Database Connection Failure"
              description="Critical database connections are failing. The system may not function properly."
              type="error"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {databaseChecks.length === 0 && (
            <Alert
              message="No Database Checks Found"
              description="No database health checks were found in the system status."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {showDetails && databaseChecks.length > 0 && (
            <>
              <Divider orientation="left">Database Health Details</Divider>
              <Space direction="vertical" style={{ width: '100%' }}>
                {databaseChecks.map((check, index) => (
                  <Card
                    key={index}
                    size="small"
                    title={
                      <Space>
                        {getStatusIcon(check.status)}
                        <Text strong>{check.name}</Text>
                        <Tag color={getStatusColor(check.status)}>{check.status}</Tag>
                      </Space>
                    }
                    style={{
                      borderColor: check.status === 'Healthy' ? '#52c41a' :
                                  check.status === 'Degraded' ? '#faad14' : '#ff4d4f',
                      borderWidth: 2
                    }}
                  >
                    {check.description && (
                      <Paragraph>
                        <Text strong>Description:</Text> {check.description}
                      </Paragraph>
                    )}

                    {check.duration && (
                      <Paragraph>
                        <Text strong>Response Time:</Text> {check.duration.toFixed(2)}ms
                      </Paragraph>
                    )}

                    {check.exception && (
                      <Paragraph>
                        <Text strong>Error:</Text> <Text type="danger">{check.exception}</Text>
                      </Paragraph>
                    )}
                  </Card>
                ))}
              </Space>
            </>
          )}

          {showDetails && (
            <>
              <Divider orientation="left">System Health Summary</Divider>
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text><strong>Overall Status:</strong> {healthData.status}</Text>
                <Text><strong>Total Duration:</strong> {healthData.totalDuration.toFixed(2)}ms</Text>
                <Text><strong>Total Checks:</strong> {healthData.checks.length}</Text>
                <Text><strong>Timestamp:</strong> {new Date(healthData.timestamp).toLocaleString()}</Text>
              </Space>
            </>
          )}

          {lastCheckTime && (
            <Paragraph style={{ marginTop: 16, textAlign: 'center' }}>
              <Text type="secondary">Last checked: {lastCheckTime}</Text>
            </Paragraph>
          )}
        </>
      )}
    </Card>
  );
};

export default DatabaseStatus;
