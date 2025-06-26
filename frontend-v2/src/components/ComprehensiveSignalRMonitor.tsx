import React, { useState, useEffect } from 'react';
import {
  Card,
  Tabs,
  Table,
  Tag,
  Progress,
  Typography,
  Space,
  Button,
  Alert,
  Spin,
  Timeline,
  Badge,
  Statistic,
  Row,
  Col,
  Tree,
  Drawer,
  Tooltip,
  App,
  Empty,
  Divider,
  Input
} from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  ReloadOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  InfoCircleOutlined,
  EyeOutlined,
  ExpandOutlined,
  DatabaseOutlined,
  ApiOutlined,
  BugOutlined,
  ThunderboltOutlined,
  FieldTimeOutlined,
  FileTextOutlined,
  NodeIndexOutlined
} from '@ant-design/icons';
import { usePipelineTestMonitoring } from '../hooks/usePipelineTestMonitoring';
import TestRunDetailsViewer from './TestRunDetailsViewer';
import { PipelineTestResult, formatPipelineStepName } from '../types/aiPipelineTest';
import { useAppSelector } from '@shared/hooks';
import { selectAccessToken } from '@shared/store/auth';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;


interface ComprehensiveSignalRMonitorProps {
  testResult?: PipelineTestResult | null;
}

// Helper function to format JSON data for display
const formatJsonData = (data: any, maxDepth = 3, currentDepth = 0): any => {
  if (currentDepth >= maxDepth) return '...';
  
  if (Array.isArray(data)) {
    return data.map((item, index) => ({
      key: index,
      title: `[${index}]`,
      children: typeof item === 'object' ? formatJsonData(item, maxDepth, currentDepth + 1) : item
    }));
  }
  
  if (data && typeof data === 'object') {
    return Object.entries(data).map(([key, value]) => ({
      key,
      title: key,
      children: typeof value === 'object' ? formatJsonData(value, maxDepth, currentDepth + 1) : String(value)
    }));
  }
  
  return String(data);
};



export const ComprehensiveSignalRMonitor: React.FC<ComprehensiveSignalRMonitorProps> = ({
  testResult
}) => {
  const {
    isConnected,
    currentSession,
    stepProgress,
    connectionError,
    isReconnecting,
    connect,
    disconnect,
    joinTestSession,
    leaveTestSession,
    sendHeartbeat
  } = usePipelineTestMonitoring();

  const accessToken = useAppSelector(selectAccessToken);
  const [activeTab, setActiveTab] = useState('overview');
  const [autoConnect, setAutoConnect] = useState(true);

  // Auto-connect on mount
  useEffect(() => {
    if (autoConnect && !isConnected && !isReconnecting) {
      console.log('🔄 Auto-connecting to SignalR hub...');
      connect();
    }
  }, [autoConnect, isConnected, isReconnecting, connect]);

  // Connection status indicator
  const getConnectionStatus = () => {
    if (isReconnecting) return { status: 'processing', text: 'Reconnecting...' };
    if (isConnected) return { status: 'success', text: 'Connected' };
    if (connectionError) return { status: 'error', text: `Error: ${connectionError}` };
    return { status: 'default', text: 'Not Connected' };
  };

  const connectionStatus = getConnectionStatus();

  const handleConnectionToggle = () => {
    if (isConnected) {
      disconnect();
      setAutoConnect(false);
    } else {
      connect();
      setAutoConnect(true);
    }
  };

  return (
    <Card 
      title={
        <Space>
          <ApiOutlined />
          <span>SignalR Pipeline Monitor</span>
          <Badge 
            status={connectionStatus.status as any} 
            text={connectionStatus.text} 
          />
        </Space>
      }
      extra={
        <Space>
          <Button
            size="small"
            icon={<ReloadOutlined />}
            onClick={handleConnectionToggle}
            loading={isReconnecting}
          >
            {isConnected ? 'Disconnect' : 'Connect'}
          </Button>
        </Space>
      }
    >
      {connectionError && (
        <Alert
          message="Connection Error"
          description={connectionError}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Overview" key="overview">
          <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            <Col span={6}>
              <Statistic
                title="Connection Status"
                value={connectionStatus.text}
                prefix={isConnected ? <CheckCircleOutlined /> : <CloseCircleOutlined />}
                valueStyle={{ color: isConnected ? '#3f8600' : '#cf1322' }}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Active Session"
                value={currentSession?.sessionId || 'None'}
                prefix={<DatabaseOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Session Status"
                value={currentSession?.status || 'No Session'}
                prefix={<InfoCircleOutlined />}
                valueStyle={{
                  color: currentSession?.status === 'completed' ? '#3f8600' :
                        currentSession?.status === 'running' ? '#1890ff' :
                        currentSession?.status === 'error' ? '#cf1322' : '#666'
                }}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Active Steps"
                value={Object.keys(stepProgress).length}
                prefix={<NodeIndexOutlined />}
              />
            </Col>
          </Row>

          {/* Current Session Details */}
          {currentSession && (
            <Card size="small" title="Current Session Details" style={{ marginBottom: 16 }}>
              <Row gutter={[16, 8]}>
                <Col span={12}>
                  <Text strong>Test ID:</Text> <Text code>{currentSession.testId}</Text>
                </Col>
                <Col span={12}>
                  <Text strong>Status:</Text>
                  <Tag color={
                    currentSession.status === 'completed' ? 'green' :
                    currentSession.status === 'running' ? 'blue' :
                    currentSession.status === 'error' ? 'red' : 'default'
                  }>
                    {currentSession.status?.toUpperCase()}
                  </Tag>
                </Col>
                <Col span={12}>
                  <Text strong>Start Time:</Text> <Text>{currentSession.startTime ? new Date(currentSession.startTime).toLocaleTimeString() : 'N/A'}</Text>
                </Col>
                <Col span={12}>
                  <Text strong>Query:</Text> <Text ellipsis>{currentSession.query || 'No query'}</Text>
                </Col>
                {currentSession.endTime && (
                  <Col span={12}>
                    <Text strong>End Time:</Text> <Text>{new Date(currentSession.endTime).toLocaleTimeString()}</Text>
                  </Col>
                )}
                {currentSession.steps && currentSession.steps.length > 0 && (
                  <Col span={24}>
                    <Text strong>Configured Steps:</Text>
                    <div style={{ marginTop: 4 }}>
                      {currentSession.steps.map((step, index) => (
                        <Tag key={index} style={{ marginBottom: 4 }}>
                          {formatPipelineStepName ? formatPipelineStepName(step) : step}
                        </Tag>
                      ))}
                    </div>
                  </Col>
                )}
              </Row>
            </Card>
          )}

          {/* Step Progress Summary */}
          {Object.keys(stepProgress).length > 0 && (
            <Card size="small" title="Step Progress Summary">
              <Row gutter={[16, 8]}>
                {Object.entries(stepProgress).map(([stepName, progress]) => (
                  <Col span={12} key={stepName}>
                    <div style={{ marginBottom: 8 }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Text strong>{formatPipelineStepName ? formatPipelineStepName(stepName) : stepName}</Text>
                        <Tag color={
                          progress.status === 'completed' ? 'green' :
                          progress.status === 'running' ? 'blue' :
                          progress.status === 'error' ? 'red' : 'default'
                        }>
                          {progress.status?.toUpperCase()}
                        </Tag>
                      </div>
                      {progress.status === 'running' && (
                        <Progress
                          percent={progress.progress || 0}
                          size="small"
                          status={progress.status === 'error' ? 'exception' : 'active'}
                        />
                      )}
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {progress.message || 'No message'}
                      </Text>
                    </div>
                  </Col>
                ))}
              </Row>
            </Card>
          )}
        </TabPane>

        <TabPane tab="Test Results" key="results">
          <TestRunDetailsViewer testResult={testResult} />
        </TabPane>

        <TabPane tab="Session Management" key="session">
          <Card size="small" title="Session Controls" style={{ marginBottom: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Current Session:</Text>
                <div style={{ marginTop: 8 }}>
                  {currentSession ? (
                    <Tag color="blue" style={{ padding: '4px 8px' }}>
                      {currentSession.sessionId}
                    </Tag>
                  ) : (
                    <Text type="secondary">No active session</Text>
                  )}
                </div>
              </div>

              <Divider style={{ margin: '12px 0' }} />

              <div>
                <Text strong>Manual Session Management:</Text>
                <div style={{ marginTop: 8 }}>
                  <Space>
                    <Input
                      placeholder="Enter test session ID"
                      style={{ width: 200 }}
                      id="session-id-input"
                    />
                    <Button
                      type="primary"
                      size="small"
                      disabled={!isConnected}
                      onClick={() => {
                        const input = document.getElementById('session-id-input') as HTMLInputElement;
                        if (input?.value) {
                          joinTestSession(input.value);
                          input.value = '';
                        }
                      }}
                    >
                      Join Session
                    </Button>
                    {currentSession && (
                      <Button
                        size="small"
                        disabled={!isConnected}
                        onClick={() => leaveTestSession(currentSession.sessionId)}
                      >
                        Leave Current Session
                      </Button>
                    )}
                  </Space>
                </div>
              </div>
            </Space>
          </Card>

          <Card size="small" title="Connection Diagnostics">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Row gutter={16}>
                <Col span={12}>
                  <Text strong>SignalR Hub URL:</Text>
                  <br />
                  <Text code style={{ fontSize: '12px' }}>
                    {window.location.origin}/hubs/pipeline-test
                  </Text>
                </Col>
                <Col span={12}>
                  <Text strong>Authentication:</Text>
                  <br />
                  <Text type="secondary">
                    {accessToken ? 'Token Available' : 'No Token'}
                  </Text>
                </Col>
              </Row>

              <Divider style={{ margin: '12px 0' }} />

              <Space>
                <Button
                  size="small"
                  icon={<ReloadOutlined />}
                  onClick={sendHeartbeat}
                  disabled={!isConnected}
                >
                  Send Heartbeat
                </Button>
                <Button
                  size="small"
                  icon={<BugOutlined />}
                  onClick={() => {
                    console.log('🔍 SignalR State:', {
                      isConnected,
                      currentSession,
                      stepProgress,
                      connectionError
                    });
                    message.info('Check console for SignalR state details');
                  }}
                >
                  Debug State
                </Button>
              </Space>
            </Space>
          </Card>
        </TabPane>

        <TabPane tab="Live Progress" key="progress">
          {Object.keys(stepProgress).length > 0 ? (
            <div>
              {/* Real-time Progress Header */}
              <div style={{ marginBottom: 16, padding: 12, backgroundColor: '#f0f2f5', borderRadius: 6 }}>
                <Row gutter={16}>
                  <Col span={8}>
                    <Statistic
                      title="Total Steps"
                      value={Object.keys(stepProgress).length}
                      prefix={<NodeIndexOutlined />}
                      size="small"
                    />
                  </Col>
                  <Col span={8}>
                    <Statistic
                      title="Completed"
                      value={Object.values(stepProgress).filter(p => p.status === 'completed').length}
                      prefix={<CheckCircleOutlined />}
                      valueStyle={{ color: '#3f8600' }}
                      size="small"
                    />
                  </Col>
                  <Col span={8}>
                    <Statistic
                      title="Running"
                      value={Object.values(stepProgress).filter(p => p.status === 'running').length}
                      prefix={<Spin size="small" />}
                      valueStyle={{ color: '#1890ff' }}
                      size="small"
                    />
                  </Col>
                </Row>
              </div>

              {/* Live Timeline */}
              <Timeline>
                {Object.entries(stepProgress)
                  .sort(([, a], [, b]) => {
                    // Sort by start time, then by status priority
                    const timeA = a.startTime ? new Date(a.startTime).getTime() : 0;
                    const timeB = b.startTime ? new Date(b.startTime).getTime() : 0;
                    return timeA - timeB;
                  })
                  .map(([stepName, progress]) => (
                    <Timeline.Item
                      key={stepName}
                      dot={
                        progress.status === 'completed' ? <CheckCircleOutlined style={{ color: '#52c41a' }} /> :
                        progress.status === 'error' ? <CloseCircleOutlined style={{ color: '#ff4d4f' }} /> :
                        progress.status === 'running' ? <Spin size="small" /> :
                        <ClockCircleOutlined style={{ color: '#1890ff' }} />
                      }
                      color={
                        progress.status === 'completed' ? 'green' :
                        progress.status === 'error' ? 'red' :
                        progress.status === 'running' ? 'blue' : 'gray'
                      }
                    >
                      <Card size="small" style={{ marginBottom: 8 }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
                          <Text strong style={{ fontSize: '16px' }}>
                            {formatPipelineStepName ? formatPipelineStepName(stepName) : stepName}
                          </Text>
                          <Tag color={
                            progress.status === 'completed' ? 'green' :
                            progress.status === 'running' ? 'blue' :
                            progress.status === 'error' ? 'red' : 'default'
                          }>
                            {progress.status?.toUpperCase()}
                          </Tag>
                        </div>

                        <Space direction="vertical" style={{ width: '100%' }}>
                          <Text type="secondary">{progress.message || 'Processing...'}</Text>

                          {progress.status === 'running' && (
                            <Progress
                              percent={progress.progress || 0}
                              size="small"
                              status="active"
                              format={(percent) => `${percent}%`}
                            />
                          )}

                          <Row gutter={16}>
                            {progress.startTime && (
                              <Col span={12}>
                                <Text type="secondary" style={{ fontSize: '12px' }}>
                                  <ClockCircleOutlined /> Started: {new Date(progress.startTime).toLocaleTimeString()}
                                </Text>
                              </Col>
                            )}
                            {progress.endTime && (
                              <Col span={12}>
                                <Text type="secondary" style={{ fontSize: '12px' }}>
                                  <CheckCircleOutlined /> Ended: {new Date(progress.endTime).toLocaleTimeString()}
                                </Text>
                              </Col>
                            )}
                          </Row>

                          {progress.details && (
                            <details style={{ marginTop: 8 }}>
                              <summary style={{ cursor: 'pointer', color: '#1890ff' }}>
                                <Text type="secondary">View Details</Text>
                              </summary>
                              <div style={{
                                marginTop: 8,
                                padding: 8,
                                backgroundColor: '#f5f5f5',
                                borderRadius: 4,
                                fontSize: '12px',
                                fontFamily: 'monospace'
                              }}>
                                <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>
                                  {typeof progress.details === 'string' ? progress.details : JSON.stringify(progress.details, null, 2)}
                                </pre>
                              </div>
                            </details>
                          )}
                        </Space>
                      </Card>
                    </Timeline.Item>
                  ))}
              </Timeline>
            </div>
          ) : (
            <div style={{ textAlign: 'center', padding: 40 }}>
              <Empty
                description={
                  <div>
                    <Text type="secondary">No active step progress</Text>
                    <br />
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {isConnected ? 'Start a pipeline test to see real-time progress' : 'Connect to SignalR to monitor progress'}
                    </Text>
                  </div>
                }
              />
              {!isConnected && (
                <Button
                  type="primary"
                  icon={<ApiOutlined />}
                  onClick={connect}
                  style={{ marginTop: 16 }}
                >
                  Connect to Monitoring
                </Button>
              )}
            </div>
          )}
        </TabPane>
      </Tabs>
    </Card>
  );
};

export default ComprehensiveSignalRMonitor;
