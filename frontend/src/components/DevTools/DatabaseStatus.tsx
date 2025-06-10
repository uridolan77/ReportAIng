import React, { useState, useEffect } from 'react';
import { Card, Button, Alert, Spin, Typography, Space, Tag, Divider, Row, Col, Statistic } from 'antd';
import { DatabaseOutlined, ReloadOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';

const { Text, Paragraph } = Typography;

interface DatabaseStatusProps {
  showDetails?: boolean;
}

interface DatabaseInfo {
  connected: boolean;
  serverName: string;
  databaseName: string;
  connectionString: string;
  lastConnectionTime?: string;
  tableCount?: number;
  error?: string;
}

export const DatabaseStatus: React.FC<DatabaseStatusProps> = ({ showDetails = true }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [dbInfo, setDbInfo] = useState<DatabaseInfo | null>(null);
  const [lastTestTime, setLastTestTime] = useState<string | null>(null);

  const testDatabaseConnection = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/database/status`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setDbInfo(data);
      } else {
        setDbInfo({
          connected: false,
          serverName: 'Unknown',
          databaseName: 'Unknown',
          connectionString: 'Failed to retrieve',
          error: `HTTP ${response.status}: ${response.statusText}`
        });
      }
      setLastTestTime(new Date().toLocaleString());
    } catch (error) {
      console.error('Error testing database connection:', error);
      setDbInfo({
        connected: false,
        serverName: 'Unknown',
        databaseName: 'Unknown',
        connectionString: 'Failed to retrieve',
        error: error instanceof Error ? error.message : 'Unknown error'
      });
      setLastTestTime(new Date().toLocaleString());
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    testDatabaseConnection();
  }, []);

  const getStatusIcon = (connected: boolean) => {
    return connected ? (
      <CheckCircleOutlined style={{ color: '#52c41a' }} />
    ) : (
      <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
    );
  };

  return (
    <Card
      title={
        <Space>
          <DatabaseOutlined />
          <span>Database Connection Status</span>
          {dbInfo?.connected && <Tag color="success">Connected</Tag>}
          {dbInfo?.connected === false && <Tag color="error">Disconnected</Tag>}
          {dbInfo === null && <Tag color="processing">Testing...</Tag>}
        </Space>
      }
      extra={
        <Button
          icon={<ReloadOutlined />}
          onClick={testDatabaseConnection}
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
          <Paragraph style={{ marginTop: 16 }}>Testing database connection...</Paragraph>
        </div>
      )}

      {!isLoading && dbInfo && (
        <>
          {dbInfo.connected && (
            <Alert
              message="Database Connection Successful"
              description="Successfully connected to the database server."
              type="success"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {!dbInfo.connected && (
            <Alert
              message="Database Connection Failed"
              description={dbInfo.error || "Unable to connect to the database server."}
              type="error"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {showDetails && (
            <>
              <Divider orientation="left">Connection Details</Divider>
              
              <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col span={8}>
                  <Statistic
                    title="Server"
                    value={dbInfo.serverName}
                    prefix={getStatusIcon(dbInfo.connected)}
                  />
                </Col>
                <Col span={8}>
                  <Statistic
                    title="Database"
                    value={dbInfo.databaseName}
                  />
                </Col>
                {dbInfo.tableCount && (
                  <Col span={8}>
                    <Statistic
                      title="Tables"
                      value={dbInfo.tableCount}
                    />
                  </Col>
                )}
              </Row>

              <Card size="small" title="Connection Information">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Connection String:</Text>
                    <br />
                    <Text code style={{ fontSize: '12px' }}>
                      {dbInfo.connectionString}
                    </Text>
                  </div>
                  
                  {dbInfo.lastConnectionTime && (
                    <div>
                      <Text strong>Last Connection:</Text>
                      <br />
                      <Text>{dbInfo.lastConnectionTime}</Text>
                    </div>
                  )}

                  {dbInfo.error && (
                    <div>
                      <Text strong>Error Details:</Text>
                      <br />
                      <Text type="danger">{dbInfo.error}</Text>
                    </div>
                  )}
                </Space>
              </Card>
            </>
          )}

          {lastTestTime && (
            <Paragraph style={{ marginTop: 16, textAlign: 'center' }}>
              <Text type="secondary">Last tested: {lastTestTime}</Text>
            </Paragraph>
          )}
        </>
      )}

      {!isLoading && !dbInfo && (
        <Alert
          message="No Database Information"
          description="Click 'Test Connection' to check the database connectivity."
          type="info"
          showIcon
        />
      )}
    </Card>
  );
};

export default DatabaseStatus;
