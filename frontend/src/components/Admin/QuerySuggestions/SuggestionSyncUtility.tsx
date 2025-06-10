import React, { useState } from 'react';
import { Card, Button, Space, Typography, Alert, Divider, List, Tag } from 'antd';
import { SyncOutlined, DatabaseOutlined, CheckCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { syncSuggestionsToDatabase } from '../../../utils/syncSuggestionsToDatabase';
import { suggestionCategories } from '../../QueryInterface/QuerySuggestions';

const { Title, Text, Paragraph } = Typography;

interface SyncLog {
  timestamp: string;
  type: 'info' | 'success' | 'error';
  message: string;
}

interface SuggestionSyncUtilityProps {
  onSyncComplete?: () => void;
}

export const SuggestionSyncUtility: React.FC<SuggestionSyncUtilityProps> = ({ onSyncComplete }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [syncLogs, setSyncLogs] = useState<SyncLog[]>([]);
  const [lastSyncResult, setLastSyncResult] = useState<'success' | 'error' | null>(null);

  const addLog = (type: SyncLog['type'], message: string) => {
    const log: SyncLog = {
      timestamp: new Date().toLocaleTimeString(),
      type,
      message
    };
    setSyncLogs(prev => [...prev, log]);
  };

  const handleSync = async () => {
    setIsLoading(true);
    setSyncLogs([]);
    setLastSyncResult(null);

    try {
      addLog('info', 'Starting suggestion database sync...');
      
      // Override console.log to capture logs
      const originalConsoleLog = console.log;
      const originalConsoleError = console.error;
      
      console.log = (message: string, ...args: any[]) => {
        addLog('info', message);
        originalConsoleLog(message, ...args);
      };
      
      console.error = (message: string, ...args: any[]) => {
        addLog('error', message);
        originalConsoleError(message, ...args);
      };

      await syncSuggestionsToDatabase();
      
      // Restore console methods
      console.log = originalConsoleLog;
      console.error = originalConsoleError;
      
      addLog('success', 'Suggestion sync completed successfully!');
      setLastSyncResult('success');

      // Notify parent component to refresh
      if (onSyncComplete) {
        onSyncComplete();
      }
      
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Sync failed:', error);
      }

      addLog('error', `Sync failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
      setLastSyncResult('error');
    } finally {
      setIsLoading(false);
    }
  };

  const getLogIcon = (type: SyncLog['type']) => {
    switch (type) {
      case 'success': return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'error': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      default: return <SyncOutlined style={{ color: '#1890ff' }} />;
    }
  };

  const getLogColor = (type: SyncLog['type']) => {
    switch (type) {
      case 'success': return '#f6ffed';
      case 'error': return '#fff2f0';
      default: return '#f0f9ff';
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div>
            <Title level={3}>
              <DatabaseOutlined /> Suggestion Database Sync Utility
            </Title>
            <Paragraph>
              This utility synchronizes the hardcoded frontend suggestions with the database. 
              It will replace all existing suggestions with the improved ones from the QuerySuggestions component.
            </Paragraph>
          </div>

          {lastSyncResult && (
            <Alert
              type={lastSyncResult === 'success' ? 'success' : 'error'}
              message={
                lastSyncResult === 'success' 
                  ? 'Sync completed successfully!' 
                  : 'Sync failed - check logs below'
              }
              showIcon
              closable
            />
          )}

          <Card size="small" title="Preview: Suggestions to be Synced">
            <Space direction="vertical" style={{ width: '100%' }}>
              {suggestionCategories.map((category, index) => (
                <div key={index}>
                  <Text strong style={{ color: category.color }}>
                    {category.title} ({category.suggestions.length} suggestions)
                  </Text>
                  <div style={{ marginLeft: '16px', marginTop: '8px' }}>
                    {category.suggestions.map((suggestion, suggestionIndex) => (
                      <Tag key={suggestionIndex} style={{ margin: '2px' }}>
                        {suggestion}
                      </Tag>
                    ))}
                  </div>
                  {index < suggestionCategories.length - 1 && <Divider style={{ margin: '12px 0' }} />}
                </div>
              ))}
            </Space>
          </Card>

          <div>
            <Button
              type="primary"
              size="large"
              icon={<SyncOutlined spin={isLoading} />}
              loading={isLoading}
              onClick={handleSync}
              disabled={isLoading}
            >
              {isLoading ? 'Syncing...' : 'Sync Suggestions to Database'}
            </Button>
            <Text type="secondary" style={{ marginLeft: '12px' }}>
              This will replace all existing suggestions in the database
            </Text>
          </div>

          {syncLogs.length > 0 && (
            <Card size="small" title="Sync Logs">
              <List
                size="small"
                dataSource={syncLogs}
                renderItem={(log) => (
                  <List.Item
                    style={{
                      backgroundColor: getLogColor(log.type),
                      margin: '4px 0',
                      padding: '8px 12px',
                      borderRadius: '4px',
                      border: `1px solid ${log.type === 'error' ? '#ffccc7' : log.type === 'success' ? '#b7eb8f' : '#91d5ff'}`
                    }}
                  >
                    <Space>
                      {getLogIcon(log.type)}
                      <Text code style={{ fontSize: '11px' }}>{log.timestamp}</Text>
                      <Text>{log.message}</Text>
                    </Space>
                  </List.Item>
                )}
                style={{ maxHeight: '300px', overflowY: 'auto' }}
              />
            </Card>
          )}
        </Space>
      </Card>
    </div>
  );
};

export default SuggestionSyncUtility;
