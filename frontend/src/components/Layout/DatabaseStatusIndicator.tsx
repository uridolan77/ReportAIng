import React, { useState, useEffect } from 'react';
import { Badge, Tooltip, Space, Typography, Button, Modal, Descriptions } from 'antd';
import {
  DatabaseOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { API_CONFIG } from '../../config/api';
import './DatabaseStatus.css';

const { Text } = Typography;

interface DatabaseStatus {
  isConnected: boolean;
  database?: string;
  server?: string;
  lastChecked?: string;
  responseTime?: number;
  error?: string;
  tables?: number;
}

export const DatabaseStatusIndicator: React.FC = () => {
  const [status, setStatus] = useState<DatabaseStatus>({ isConnected: false });
  const [loading, setLoading] = useState(false);
  const [showDetails, setShowDetails] = useState(false);

  const checkDatabaseStatus = async () => {
    setLoading(true);
    try {
      const startTime = Date.now();

      // Import getAuthHeaders for authenticated requests
      const { getAuthHeaders } = await import('../../config/api');
      const authHeaders = await getAuthHeaders();

      // Check health endpoint
      const healthResponse = await fetch(`${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.HEALTH}`, {
        headers: authHeaders
      });
      const healthData = await healthResponse.json();

      const responseTime = Date.now() - startTime;

      if (healthResponse.ok) {
        // Check if database-specific health checks are healthy
        const databaseChecks = healthData.checks?.filter((check: any) =>
          check.name === 'bidatabase' ||
          check.name === 'defaultdb' ||
          check.name.toLowerCase().includes('database')
        ) || [];

        const allDatabasesHealthy = databaseChecks.length > 0 &&
          databaseChecks.every((check: any) => check.status === 'Healthy');

        if (allDatabasesHealthy) {
          // Get database info from schema endpoint
          try {
            const schemaResponse = await fetch(`${API_CONFIG.BASE_URL}/api/schema/datasources`, {
              headers: authHeaders
            });
            const dataSources = await schemaResponse.json();

            setStatus({
              isConnected: true,
              database: 'DailyActionsDB',
              server: '185.64.56.157',
              lastChecked: new Date().toLocaleTimeString(),
              responseTime,
              tables: dataSources?.length || 0
            });
          } catch {
            setStatus({
              isConnected: true,
              database: 'Connected',
              server: 'Remote Server',
              lastChecked: new Date().toLocaleTimeString(),
              responseTime
            });
          }
        } else {
          const failedDatabases = databaseChecks.filter((check: any) => check.status !== 'Healthy');
          const errorMessage = failedDatabases.length > 0
            ? `Database issues: ${failedDatabases.map((db: any) => `${db.name} (${db.status})`).join(', ')}`
            : 'No database health checks found';

          setStatus({
            isConnected: false,
            error: errorMessage,
            lastChecked: new Date().toLocaleTimeString(),
            responseTime
          });
        }
      } else {
        setStatus({
          isConnected: false,
          error: healthData.error || 'Health check failed',
          lastChecked: new Date().toLocaleTimeString(),
          responseTime
        });
      }
    } catch (error) {
      setStatus({
        isConnected: false,
        error: error instanceof Error ? error.message : 'Connection failed',
        lastChecked: new Date().toLocaleTimeString()
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Delay initial check to allow authentication to settle
    const initialTimeout = setTimeout(() => {
      checkDatabaseStatus();
    }, 1000);

    // Check status every 30 seconds
    const interval = setInterval(checkDatabaseStatus, 30000);

    return () => {
      clearTimeout(initialTimeout);
      clearInterval(interval);
    };
  }, []);

  const getStatusIcon = () => {
    if (loading) {
      return <ReloadOutlined spin />;
    }

    if (status.isConnected) {
      return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
    }

    return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
  };

  const getStatusColor = () => {
    if (loading) return 'processing';
    return status.isConnected ? 'success' : 'error';
  };



  const getTooltipTitle = () => {
    if (loading) return 'Checking database connection...';

    if (status.isConnected) {
      return (
        <div>
          <div>✓ Database Connected</div>
          {status.server && <div>Server: {status.server}</div>}
          {status.responseTime && <div>Response: {status.responseTime}ms</div>}
          {status.lastChecked && <div>Last checked: {status.lastChecked}</div>}
        </div>
      );
    }

    return (
      <div>
        <div>✗ Database Disconnected</div>
        {status.error && <div>Error: {status.error}</div>}
        {status.lastChecked && <div>Last checked: {status.lastChecked}</div>}
      </div>
    );
  };

  return (
    <>
      <Tooltip
        title={getTooltipTitle()}
        classNames={{ root: "database-status-tooltip" }}
      >
        <div
          onClick={() => setShowDetails(true)}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            padding: '4px 8px',
            borderRadius: '4px',
            cursor: 'pointer',
            background: status.isConnected ? '#f6ffed' : '#fff2f0',
            border: `1px solid ${status.isConnected ? '#b7eb8f' : '#ffccc7'}`,
            transition: 'all 0.2s ease'
          }}
          data-testid="database-status"
        >
          {getStatusIcon()}
          <Text style={{
            fontSize: '12px',
            color: status.isConnected ? '#52c41a' : '#ff4d4f',
            fontWeight: 500
          }}>
            {status.isConnected ? 'DB Connected' : 'DB Offline'}
          </Text>
        </div>
      </Tooltip>

      <Modal
        title={
          <Space>
            <DatabaseOutlined />
            Database Connection Status
          </Space>
        }
        open={showDetails}
        onCancel={() => setShowDetails(false)}
        footer={[
          <Button key="refresh" icon={<ReloadOutlined />} onClick={checkDatabaseStatus} loading={loading}>
            Refresh
          </Button>,
          <Button key="close" onClick={() => setShowDetails(false)}>
            Close
          </Button>
        ]}
        width={500}
        className="database-status-modal"
      >
        <Descriptions column={1} bordered size="small">
          <Descriptions.Item
            label="Status"
            labelStyle={{ width: '120px' }}
          >
            <Space>
              {getStatusIcon()}
              <Text
                strong
                className={status.isConnected ? 'status-connected' : 'status-disconnected'}
              >
                {status.isConnected ? 'Connected' : 'Disconnected'}
              </Text>
            </Space>
          </Descriptions.Item>

          {status.database && (
            <Descriptions.Item label="Database">
              {status.database}
            </Descriptions.Item>
          )}

          {status.server && (
            <Descriptions.Item label="Server">
              {status.server}
            </Descriptions.Item>
          )}

          {status.responseTime && (
            <Descriptions.Item label="Response Time">
              {status.responseTime}ms
            </Descriptions.Item>
          )}

          {status.tables !== undefined && (
            <Descriptions.Item label="Available Tables">
              {status.tables}
            </Descriptions.Item>
          )}

          {status.lastChecked && (
            <Descriptions.Item label="Last Checked">
              {status.lastChecked}
            </Descriptions.Item>
          )}

          {status.error && (
            <Descriptions.Item label="Error">
              <Text type="danger">{status.error}</Text>
            </Descriptions.Item>
          )}
        </Descriptions>

        {!status.isConnected && (
          <div className="status-info-panel error">
            <Space>
              <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
              <Text>
                Database connection is required for querying data. Please check your connection settings.
              </Text>
            </Space>
          </div>
        )}

        {status.isConnected && (
          <div className="status-info-panel success">
            <Space>
              <CheckCircleOutlined style={{ color: '#52c41a' }} />
              <Text>
                Database is connected and ready for queries.
              </Text>
            </Space>
          </div>
        )}
      </Modal>
    </>
  );
};
