import React, { useState, useEffect } from 'react';
import { Alert, Button, Space, Typography } from 'antd';
import {
  DatabaseOutlined,
  ReloadOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import './DatabaseStatus.css';

const { Text } = Typography;

interface DatabaseConnectionBannerProps {
  style?: React.CSSProperties;
}

export const DatabaseConnectionBanner: React.FC<DatabaseConnectionBannerProps> = ({ style }) => {
  const [isConnected, setIsConnected] = useState<boolean | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [lastError, setLastError] = useState<string>('');

  const checkConnection = async () => {
    setIsChecking(true);
    try {
      const response = await fetch('https://localhost:55243/health');
      const data = await response.json();

      if (response.ok) {
        // Check if database connections are healthy
        const databaseChecks = data.checks?.filter((check: any) =>
          check.name.toLowerCase().includes('database') ||
          check.name.toLowerCase().includes('sql') ||
          check.name === 'bidatabase' ||
          check.name === 'defaultdb'
        ) || [];

        const hasDatabaseIssues = databaseChecks.some((check: any) => check.status === 'Unhealthy');

        if (!hasDatabaseIssues && databaseChecks.length > 0) {
          setIsConnected(true);
          setLastError('');
        } else if (databaseChecks.length === 0) {
          // No database checks found, assume connected if overall status is not Unhealthy
          setIsConnected(data.status !== 'Unhealthy');
          setLastError(data.status === 'Unhealthy' ? 'System health check failed' : '');
        } else {
          setIsConnected(false);
          setLastError('Database connection issues detected');
        }
      } else {
        setIsConnected(false);
        // Handle different error response formats
        let errorMessage = 'Health check failed';
        if (data.error) {
          if (typeof data.error === 'string') {
            errorMessage = data.error;
          } else if (data.error.message) {
            errorMessage = data.error.message;
          } else {
            errorMessage = JSON.stringify(data.error);
          }
        }
        setLastError(errorMessage);
      }
    } catch (error) {
      setIsConnected(false);
      setLastError(error instanceof Error ? error.message : String(error || 'Connection failed'));
    } finally {
      setIsChecking(false);
    }
  };

  useEffect(() => {
    checkConnection();

    // Check every 30 seconds
    const interval = setInterval(checkConnection, 30000);

    return () => clearInterval(interval);
  }, []);

  // Don't show banner if connected
  if (isConnected === true) {
    return null;
  }

  // Don't show banner while initial check is loading
  if (isConnected === null && isChecking) {
    return null;
  }

  const getAlertType = () => {
    if (isConnected === false) return 'error';
    return 'warning';
  };

  const getMessage = () => {
    if (isConnected === false) {
      const errorString = String(lastError || '');
      const isDatabaseIssue = errorString.includes('Database connection issues') ||
                             errorString.includes('Connection failed') ||
                             errorString.includes('Health check failed');

      return (
        <Space direction="vertical" size={4}>
          <Space>
            <ExclamationCircleOutlined />
            <Text strong>
              {isDatabaseIssue ? 'Database Connection Issues' : 'System Health Issues'}
            </Text>
          </Space>
          <Text>
            {isDatabaseIssue
              ? 'Unable to connect to the database. Query functionality may be limited.'
              : 'Some system components are experiencing issues but database connections are working.'
            }
            {errorString && ` Error: ${errorString}`}
          </Text>
        </Space>
      );
    }

    return (
      <Space>
        <DatabaseOutlined />
        <Text>Checking database connection...</Text>
      </Space>
    );
  };

  const getAction = () => {
    return (
      <Button
        size="small"
        icon={<ReloadOutlined />}
        onClick={checkConnection}
        loading={isChecking}
      >
        Retry Connection
      </Button>
    );
  };

  return (
    <Alert
      type={getAlertType()}
      message={getMessage()}
      action={getAction()}
      showIcon={false}
      closable={false}
      className={`database-connection-banner ${getAlertType()}`}
      style={style}
    />
  );
};
