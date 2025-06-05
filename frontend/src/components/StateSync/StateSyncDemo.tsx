import React, { useState } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Input,
  Switch,
  Statistic,
  Tag,
  Space,
  Typography,
  Alert,
  Progress,
  Divider,
  Upload,
  message
} from 'antd';
import {
  SyncOutlined,
  CloudOutlined,
  ExportOutlined,
  ImportOutlined,
  DeleteOutlined,
  ReloadOutlined,
  TagsOutlined
} from '@ant-design/icons';
import { useStateSync, useSyncStatus, useStorageManagement } from '../Providers/StateSyncProvider';
import { useEnhancedState, useUserPreferences } from '../../hooks/useEnhancedState';

const { Title, Text } = Typography;
// TextArea not used in this component

export const StateSyncDemo: React.FC = () => {
  // testValue and setTestValue removed - not used
  const [broadcastMessage, setBroadcastMessage] = useState('');

  // State sync hooks
  const {
    tabId,
    broadcastToTabs,
    invalidateQueryAcrossTabs,
    clearCacheAcrossTabs
  } = useStateSync();

  const {
    isSyncing,
    hasError,
    lastSyncTime,
    connectedTabsCount,
    isOnline,
    isConnected
  } = useSyncStatus();

  const {
    stats,
    isCleaningUp,
    refreshStats,
    performCleanup,
    exportState,
    importState
  } = useStorageManagement();

  // Enhanced state examples
  const {
    state: sharedCounter,
    setState: setSharedCounter,
    isLoading: counterLoading
  } = useEnhancedState('shared-counter', 0, {
    persistence: {
      key: 'demo-shared-counter',
      version: 1,
      storage: 'localStorage',
      compression: false,
      encryption: false,
      maxAge: 24 * 60 * 60 * 1000,
      maxSize: 1024
    },
    crossTabSync: true,
    debounceMs: 100
  });

  const { preferences, updatePreference } = useUserPreferences({
    theme: 'light',
    notifications: true,
    autoSave: true
  });

  // appState and updateAppState removed - not used in this component

  // Handlers
  const handleBroadcast = () => {
    if (broadcastMessage.trim()) {
      broadcastToTabs('demo_message', {
        message: broadcastMessage,
        from: tabId,
        timestamp: Date.now()
      });
      setBroadcastMessage('');
      message.success('Message broadcasted to all tabs');
    }
  };

  const handleInvalidateQuery = () => {
    invalidateQueryAcrossTabs(['demo', 'test-query']);
    message.success('Query invalidated across all tabs');
  };

  const handleClearCache = () => {
    clearCacheAcrossTabs('demo');
    message.success('Demo cache cleared across all tabs');
  };

  const handleExport = async () => {
    try {
      const data = await exportState();
      const blob = new Blob([data], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `bi-reporting-state-${Date.now()}.json`;
      a.click();
      URL.revokeObjectURL(url);
      message.success('State exported successfully');
    } catch (error) {
      message.error('Failed to export state');
    }
  };

  const handleImport = (file: File) => {
    const reader = new FileReader();
    reader.onload = async (e) => {
      try {
        const data = e.target?.result as string;
        await importState(data);
        message.success('State imported successfully');
      } catch (error) {
        message.error('Failed to import state');
      }
    };
    reader.readAsText(file);
    return false; // Prevent default upload
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>
        <SyncOutlined spin={isSyncing} /> State Synchronization Demo
      </Title>

      {/* Status Overview */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Connection Status"
              value={isOnline ? 'Online' : 'Offline'}
              prefix={<CloudOutlined style={{ color: isOnline ? '#52c41a' : '#ff4d4f' }} />}
              valueStyle={{ color: isOnline ? '#52c41a' : '#ff4d4f' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Connected Tabs"
              value={connectedTabsCount}
              prefix={<TagsOutlined />}
              suffix={connectedTabsCount === 1 ? 'tab' : 'tabs'}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Sync Status"
              value={isSyncing ? 'Syncing' : hasError ? 'Error' : 'Idle'}
              prefix={<SyncOutlined spin={isSyncing} />}
              valueStyle={{
                color: isSyncing ? '#1890ff' : hasError ? '#ff4d4f' : '#52c41a'
              }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Last Sync"
              value={new Date(lastSyncTime).toLocaleTimeString()}
              prefix={<ReloadOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Tab Information */}
      <Card title="Tab Information" style={{ marginBottom: '16px' }}>
        <Space>
          <Text strong>Current Tab ID:</Text>
          <Tag color="blue">{tabId}</Tag>
          <Text strong>Status:</Text>
          <Tag color={isConnected ? 'green' : 'orange'}>
            {isConnected ? 'Connected' : 'Disconnected'}
          </Tag>
        </Space>
      </Card>

      <Row gutter={[16, 16]}>
        {/* Cross-Tab Communication */}
        <Col xs={24} lg={12}>
          <Card title="Cross-Tab Communication" style={{ height: '400px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Broadcast Message to All Tabs:</Text>
                <Input.Group compact style={{ marginTop: '8px' }}>
                  <Input
                    value={broadcastMessage}
                    onChange={(e) => setBroadcastMessage(e.target.value)}
                    placeholder="Enter message to broadcast"
                    onPressEnter={handleBroadcast}
                    style={{ width: 'calc(100% - 80px)' }}
                  />
                  <Button
                    type="primary"
                    onClick={handleBroadcast}
                    disabled={!broadcastMessage.trim()}
                  >
                    Send
                  </Button>
                </Input.Group>
              </div>

              <Divider />

              <Space direction="vertical" style={{ width: '100%' }}>
                <Button
                  onClick={handleInvalidateQuery}
                  icon={<ReloadOutlined />}
                  block
                >
                  Invalidate Demo Query Across Tabs
                </Button>
                <Button
                  onClick={handleClearCache}
                  icon={<DeleteOutlined />}
                  block
                >
                  Clear Demo Cache Across Tabs
                </Button>
              </Space>
            </Space>
          </Card>
        </Col>

        {/* Enhanced State Demo */}
        <Col xs={24} lg={12}>
          <Card title="Enhanced State Demo" style={{ height: '400px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Shared Counter (Synced Across Tabs):</Text>
                <div style={{ textAlign: 'center', margin: '16px 0' }}>
                  <Title level={2} style={{ margin: 0 }}>
                    {counterLoading ? '...' : sharedCounter}
                  </Title>
                  <Space style={{ marginTop: '8px' }}>
                    <Button
                      onClick={() => setSharedCounter(prev => prev - 1)}
                      disabled={counterLoading}
                    >
                      -1
                    </Button>
                    <Button
                      onClick={() => setSharedCounter(prev => prev + 1)}
                      disabled={counterLoading}
                    >
                      +1
                    </Button>
                    <Button
                      onClick={() => setSharedCounter(0)}
                      disabled={counterLoading}
                    >
                      Reset
                    </Button>
                  </Space>
                </div>
              </div>

              <Divider />

              <div>
                <Text strong>User Preferences:</Text>
                <Space direction="vertical" style={{ width: '100%', marginTop: '8px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text>Theme:</Text>
                    <Switch
                      checked={preferences.theme === 'dark'}
                      onChange={(checked) => updatePreference('theme', checked ? 'dark' : 'light')}
                      checkedChildren="Dark"
                      unCheckedChildren="Light"
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text>Notifications:</Text>
                    <Switch
                      checked={preferences.notifications}
                      onChange={(checked) => updatePreference('notifications', checked)}
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text>Auto Save:</Text>
                    <Switch
                      checked={preferences.autoSave}
                      onChange={(checked) => updatePreference('autoSave', checked)}
                    />
                  </div>
                </Space>
              </div>
            </Space>
          </Card>
        </Col>

        {/* Storage Management */}
        <Col xs={24} lg={12}>
          <Card title="Storage Management" style={{ height: '400px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              {stats && (
                <>
                  <div>
                    <Text strong>Local Storage:</Text>
                    <Progress
                      percent={Math.round((stats.localStorage.used / (stats.localStorage.used + stats.localStorage.available)) * 100)}
                      format={() => `${formatBytes(stats.localStorage.used)} used`}
                    />
                  </div>
                  <div>
                    <Text strong>Session Storage:</Text>
                    <Progress
                      percent={Math.round((stats.sessionStorage.used / (stats.sessionStorage.used + stats.sessionStorage.available)) * 100)}
                      format={() => `${formatBytes(stats.sessionStorage.used)} used`}
                    />
                  </div>
                </>
              )}

              <Divider />

              <Space direction="vertical" style={{ width: '100%' }}>
                <Button
                  onClick={refreshStats}
                  icon={<ReloadOutlined />}
                  block
                >
                  Refresh Storage Stats
                </Button>
                <Button
                  onClick={performCleanup}
                  icon={<DeleteOutlined />}
                  loading={isCleaningUp}
                  block
                >
                  Cleanup Expired Data
                </Button>
              </Space>
            </Space>
          </Card>
        </Col>

        {/* Import/Export */}
        <Col xs={24} lg={12}>
          <Card title="State Import/Export" style={{ height: '400px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Alert
                message="State Management"
                description="Export your application state for backup or import state from a backup file."
                type="info"
                showIcon
              />

              <Button
                onClick={handleExport}
                icon={<ExportOutlined />}
                type="primary"
                block
              >
                Export Application State
              </Button>

              <Upload
                beforeUpload={handleImport}
                accept=".json"
                showUploadList={false}
              >
                <Button
                  icon={<ImportOutlined />}
                  block
                >
                  Import Application State
                </Button>
              </Upload>

              <Alert
                message="Note"
                description="Importing state will reload the page to apply changes."
                type="warning"
                showIcon

              />
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
