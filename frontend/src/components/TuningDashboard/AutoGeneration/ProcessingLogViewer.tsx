import React from 'react';
import { Modal, List, Typography, Tag, Button, Space, Divider } from 'antd';
import { 
  InfoCircleOutlined, 
  CheckCircleOutlined, 
  ExclamationCircleOutlined, 
  CloseCircleOutlined,
  CopyOutlined,
  DownloadOutlined
} from '@ant-design/icons';

const { Text, Title } = Typography;

interface LogEntry {
  timestamp: string;
  level: 'info' | 'success' | 'warning' | 'error';
  message: string;
  details?: any;
}

interface ProcessingLogViewerProps {
  visible: boolean;
  onClose: () => void;
  logs: LogEntry[];
  title?: string;
}

export const ProcessingLogViewer: React.FC<ProcessingLogViewerProps> = ({
  visible,
  onClose,
  logs,
  title = 'Auto-Generation Processing Log'
}) => {
  const getLogIcon = (level: string) => {
    switch (level) {
      case 'success':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'warning':
        return <ExclamationCircleOutlined style={{ color: '#faad14' }} />;
      case 'error':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
      default:
        return <InfoCircleOutlined style={{ color: '#1890ff' }} />;
    }
  };

  const getLogColor = (level: string) => {
    switch (level) {
      case 'success':
        return 'success';
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      default:
        return 'blue';
    }
  };

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleTimeString();
  };

  const copyLogsToClipboard = () => {
    const logText = logs.map(log => 
      `[${formatTimestamp(log.timestamp)}] [${log.level.toUpperCase()}] ${log.message}${
        log.details ? '\n  Details: ' + JSON.stringify(log.details, null, 2) : ''
      }`
    ).join('\n');
    
    navigator.clipboard.writeText(logText);
  };

  const downloadLogs = () => {
    const logText = logs.map(log => 
      `[${formatTimestamp(log.timestamp)}] [${log.level.toUpperCase()}] ${log.message}${
        log.details ? '\n  Details: ' + JSON.stringify(log.details, null, 2) : ''
      }`
    ).join('\n');
    
    const blob = new Blob([logText], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `auto-generation-log-${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <Modal
      title={
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Title level={4} style={{ margin: 0 }}>
            📝 {title}
          </Title>
          <Space>
            <Button 
              size="small" 
              icon={<CopyOutlined />} 
              onClick={copyLogsToClipboard}
              title="Copy logs to clipboard"
            >
              Copy
            </Button>
            <Button 
              size="small" 
              icon={<DownloadOutlined />} 
              onClick={downloadLogs}
              title="Download logs as file"
            >
              Download
            </Button>
          </Space>
        </div>
      }
      open={visible}
      onCancel={onClose}
      width={800}
      footer={[
        <Button key="close" type="primary" onClick={onClose}>
          Close
        </Button>
      ]}
      style={{ top: 20 }}
    >
      <div style={{ marginBottom: '16px' }}>
        <Text type="secondary">
          Complete processing log with {logs.length} entries. 
          You can copy or download the logs for your records.
        </Text>
      </div>

      <Divider />

      <div style={{ maxHeight: '500px', overflowY: 'auto' }}>
        <List
          size="small"
          dataSource={logs}
          renderItem={(log, index) => (
            <List.Item
              style={{
                padding: '8px 12px',
                borderLeft: `3px solid ${
                  log.level === 'success' ? '#52c41a' :
                  log.level === 'warning' ? '#faad14' :
                  log.level === 'error' ? '#ff4d4f' : '#1890ff'
                }`,
                backgroundColor: index % 2 === 0 ? '#fafafa' : '#ffffff',
                marginBottom: '4px',
                borderRadius: '4px'
              }}
            >
              <div style={{ width: '100%' }}>
                <div style={{ display: 'flex', alignItems: 'center', marginBottom: '4px' }}>
                  {getLogIcon(log.level)}
                  <Tag 
                    color={getLogColor(log.level)} 
                    style={{ marginLeft: '8px', marginRight: '8px' }}
                  >
                    {log.level.toUpperCase()}
                  </Tag>
                  <Text type="secondary" style={{ fontSize: '11px' }}>
                    {formatTimestamp(log.timestamp)}
                  </Text>
                </div>
                
                <div style={{ marginLeft: '24px' }}>
                  <Text style={{ fontSize: '13px' }}>
                    {log.message}
                  </Text>
                  
                  {log.details && (
                    <details style={{ marginTop: '4px' }}>
                      <summary style={{
                        cursor: 'pointer',
                        color: '#1890ff',
                        fontSize: '11px',
                        padding: '2px 4px',
                        background: '#f0f8ff',
                        border: '1px solid #d6e4ff',
                        borderRadius: '3px',
                        display: 'inline-block'
                      }}>
                        View Details
                      </summary>
                      <pre style={{
                        whiteSpace: 'pre-wrap',
                        fontSize: '10px',
                        color: '#595959',
                        maxHeight: '100px',
                        overflow: 'auto',
                        margin: '4px 0 0 0',
                        padding: '6px',
                        background: '#f5f5f5',
                        border: '1px solid #e8e8e8',
                        borderRadius: '3px',
                        fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace'
                      }}>
                        {JSON.stringify(log.details, null, 2)}
                      </pre>
                    </details>
                  )}
                </div>
              </div>
            </List.Item>
          )}
        />
      </div>

      {logs.length === 0 && (
        <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
          <InfoCircleOutlined style={{ fontSize: '24px', marginBottom: '8px' }} />
          <div>No log entries available</div>
        </div>
      )}
    </Modal>
  );
};
