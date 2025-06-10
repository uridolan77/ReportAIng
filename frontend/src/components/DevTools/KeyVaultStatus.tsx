import React, { useState, useEffect } from 'react';
import { Card, Button, Alert, Spin, Typography, Space, Tag, Divider, Row, Col, Statistic } from 'antd';
import { SafetyOutlined, ReloadOutlined, CheckCircleOutlined, CloseCircleOutlined, KeyOutlined } from '@ant-design/icons';

const { Text, Paragraph } = Typography;

interface KeyVaultStatusProps {
  showDetails?: boolean;
}

interface KeyVaultInfo {
  connected: boolean;
  vaultUrl: string;
  secretsCount?: number;
  lastAccessTime?: string;
  error?: string;
  secrets?: Array<{
    name: string;
    enabled: boolean;
    lastUpdated: string;
  }>;
}

export const KeyVaultStatus: React.FC<KeyVaultStatusProps> = ({ showDetails = true }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [vaultInfo, setVaultInfo] = useState<KeyVaultInfo | null>(null);
  const [lastTestTime, setLastTestTime] = useState<string | null>(null);

  const testKeyVaultConnection = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/keyvault/status`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setVaultInfo(data);
      } else {
        setVaultInfo({
          connected: false,
          vaultUrl: 'Unknown',
          error: `HTTP ${response.status}: ${response.statusText}`
        });
      }
      setLastTestTime(new Date().toLocaleString());
    } catch (error) {
      console.error('Error testing Key Vault connection:', error);
      setVaultInfo({
        connected: false,
        vaultUrl: 'Unknown',
        error: error instanceof Error ? error.message : 'Unknown error'
      });
      setLastTestTime(new Date().toLocaleString());
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    testKeyVaultConnection();
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
          <SafetyOutlined />
          <span>Azure Key Vault Status</span>
          {vaultInfo?.connected && <Tag color="success">Connected</Tag>}
          {vaultInfo?.connected === false && <Tag color="error">Disconnected</Tag>}
          {vaultInfo === null && <Tag color="processing">Testing...</Tag>}
        </Space>
      }
      extra={
        <Button
          icon={<ReloadOutlined />}
          onClick={testKeyVaultConnection}
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
          <Paragraph style={{ marginTop: 16 }}>Testing Key Vault connection...</Paragraph>
        </div>
      )}

      {!isLoading && vaultInfo && (
        <>
          {vaultInfo.connected && (
            <Alert
              message="Key Vault Connection Successful"
              description="Successfully connected to Azure Key Vault and can access secrets."
              type="success"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {!vaultInfo.connected && (
            <Alert
              message="Key Vault Connection Failed"
              description={vaultInfo.error || "Unable to connect to Azure Key Vault."}
              type="error"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          {showDetails && (
            <>
              <Divider orientation="left">Key Vault Details</Divider>
              
              <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col span={12}>
                  <Statistic
                    title="Vault URL"
                    value={vaultInfo.vaultUrl}
                    prefix={getStatusIcon(vaultInfo.connected)}
                  />
                </Col>
                {vaultInfo.secretsCount !== undefined && (
                  <Col span={12}>
                    <Statistic
                      title="Accessible Secrets"
                      value={vaultInfo.secretsCount}
                      prefix={<KeyOutlined />}
                    />
                  </Col>
                )}
              </Row>

              {vaultInfo.secrets && vaultInfo.secrets.length > 0 && (
                <Card size="small" title="Secret Status">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {vaultInfo.secrets.map((secret, index) => (
                      <div key={index} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Space>
                          {getStatusIcon(secret.enabled)}
                          <Text strong>{secret.name}</Text>
                        </Space>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Updated: {secret.lastUpdated}
                        </Text>
                      </div>
                    ))}
                  </Space>
                </Card>
              )}

              <Card size="small" title="Connection Information">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Vault URL:</Text>
                    <br />
                    <Text code style={{ fontSize: '12px' }}>
                      {vaultInfo.vaultUrl}
                    </Text>
                  </div>
                  
                  {vaultInfo.lastAccessTime && (
                    <div>
                      <Text strong>Last Access:</Text>
                      <br />
                      <Text>{vaultInfo.lastAccessTime}</Text>
                    </div>
                  )}

                  {vaultInfo.error && (
                    <div>
                      <Text strong>Error Details:</Text>
                      <br />
                      <Text type="danger">{vaultInfo.error}</Text>
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

      {!isLoading && !vaultInfo && (
        <Alert
          message="No Key Vault Information"
          description="Click 'Test Connection' to check the Key Vault connectivity."
          type="info"
          showIcon
        />
      )}
    </Card>
  );
};

export default KeyVaultStatus;
