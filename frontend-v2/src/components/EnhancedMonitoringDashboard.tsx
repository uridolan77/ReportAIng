import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Progress,
  Timeline,
  Tag,
  Typography,
  Space,
  Alert,
  Statistic,
  Badge,
  Spin,
  Button,
  Tooltip,
  Tabs,
  Table,
  List,
  Avatar,
  Divider
} from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  WifiOutlined,
  DisconnectOutlined,
  ReloadOutlined,
  HeartOutlined,
  ThunderboltOutlined,
  BarChartOutlined,
  EyeOutlined,
  DownloadOutlined
} from '@ant-design/icons';
import { usePipelineTestMonitoring } from '../hooks/usePipelineTestMonitoring';
import { PipelineStep } from '../types/aiPipelineTest';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

interface EnhancedMonitoringDashboardProps {
  testId?: string;
  autoJoinSession?: boolean;
  showAnalytics?: boolean;
  showLogs?: boolean;
}

interface PerformanceMetric {
  name: string;
  value: number;
  unit: string;
  trend: 'up' | 'down' | 'stable';
  color: string;
}

const EnhancedMonitoringDashboard: React.FC<EnhancedMonitoringDashboardProps> = ({
  testId,
  autoJoinSession = true,
  showAnalytics = true,
  showLogs = true
}) => {
  const [activeTab, setActiveTab] = useState('overview');
  const [performanceMetrics, setPerformanceMetrics] = useState<PerformanceMetric[]>([]);
  const [logs, setLogs] = useState<any[]>([]);

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

  // Auto-join test session when testId is provided
  useEffect(() => {
    if (testId && isConnected && autoJoinSession) {
      joinTestSession(testId);
    }
  }, [testId, isConnected, autoJoinSession, joinTestSession]);

  // Update performance metrics when step progress changes
  useEffect(() => {
    if (currentSession && Object.keys(stepProgress).length > 0) {
      updatePerformanceMetrics();
    }
  }, [currentSession, stepProgress]);

  const updatePerformanceMetrics = () => {
    const completedSteps = Object.values(stepProgress).filter(s => s.status === 'completed');
    const avgDuration = completedSteps.length > 0 
      ? completedSteps.reduce((sum, step) => {
          if (step.startTime && step.endTime) {
            return sum + (new Date(step.endTime).getTime() - new Date(step.startTime).getTime());
          }
          return sum;
        }, 0) / completedSteps.length / 1000
      : 0;

    const totalDuration = currentSession?.startTime && currentSession?.endTime
      ? (new Date(currentSession.endTime).getTime() - new Date(currentSession.startTime).getTime()) / 1000
      : currentSession?.startTime
      ? (Date.now() - new Date(currentSession.startTime).getTime()) / 1000
      : 0;

    const successRate = completedSteps.length > 0 
      ? (completedSteps.filter(s => s.status === 'completed').length / completedSteps.length) * 100
      : 0;

    const metrics: PerformanceMetric[] = [
      {
        name: 'Total Duration',
        value: totalDuration,
        unit: 's',
        trend: 'stable',
        color: '#1890ff'
      },
      {
        name: 'Avg Step Duration',
        value: avgDuration,
        unit: 's',
        trend: 'down',
        color: '#52c41a'
      },
      {
        name: 'Success Rate',
        value: successRate,
        unit: '%',
        trend: 'up',
        color: '#722ed1'
      },
      {
        name: 'Steps Completed',
        value: completedSteps.length,
        unit: '',
        trend: 'up',
        color: '#fa8c16'
      }
    ];

    setPerformanceMetrics(metrics);
  };

  const getStepIcon = (step: string) => {
    const icons: Record<string, string> = {
      'BusinessContextAnalysis': '🧠',
      'TokenBudgetManagement': '💰',
      'SchemaRetrieval': '📊',
      'PromptBuilding': '🔧',
      'AIGeneration': '🤖'
    };
    return icons[step] || '⚙️';
  };

  const getStepStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return 'success';
      case 'running': return 'processing';
      case 'error': return 'error';
      default: return 'default';
    }
  };

  const getStepStatusIcon = (status: string) => {
    switch (status) {
      case 'completed': return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'running': return <Spin size="small" />;
      case 'error': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      default: return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const renderConnectionStatus = () => (
    <Card size="small" className="mb-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <Badge 
            status={isConnected ? 'success' : 'error'} 
            text={
              <span className="flex items-center space-x-2">
                {isConnected ? <WifiOutlined /> : <DisconnectOutlined />}
                <Text strong>
                  {isReconnecting ? 'Reconnecting...' : isConnected ? 'Connected' : 'Disconnected'}
                </Text>
              </span>
            }
          />
          {connectionError && (
            <Text type="danger" className="text-sm">
              {connectionError}
            </Text>
          )}
        </div>
        
        <Space>
          <Tooltip title="Send heartbeat">
            <Button 
              type="text" 
              size="small" 
              icon={<HeartOutlined />}
              onClick={sendHeartbeat}
              disabled={!isConnected}
            />
          </Tooltip>
          <Button 
            type="text" 
            size="small" 
            icon={<ReloadOutlined />}
            onClick={isConnected ? disconnect : connect}
            loading={isReconnecting}
          >
            {isConnected ? 'Disconnect' : 'Connect'}
          </Button>
        </Space>
      </div>
    </Card>
  );

  const renderOverviewTab = () => {
    if (!currentSession) {
      return (
        <Alert
          message="No Active Session"
          description="Start a pipeline test to see real-time monitoring data."
          type="info"
          showIcon
        />
      );
    }

    const completedSteps = Object.values(stepProgress).filter(s => s.status === 'completed').length;
    const totalSteps = currentSession.steps.length;
    const overallProgress = totalSteps > 0 ? Math.round((completedSteps / totalSteps) * 100) : 0;

    return (
      <div className="space-y-4">
        {/* Session Overview */}
        <Card title="Session Overview" size="small">
          <Row gutter={[16, 16]}>
            <Col span={6}>
              <Statistic
                title="Test ID"
                value={currentSession.testId}
                prefix={<PlayCircleOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Status"
                value={currentSession.status}
                prefix={
                  currentSession.status === 'running' ? <Spin size="small" /> :
                  currentSession.status === 'completed' ? <CheckCircleOutlined style={{ color: '#52c41a' }} /> :
                  <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
                }
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Progress"
                value={overallProgress}
                suffix="%"
                prefix={<ClockCircleOutlined />}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Steps"
                value={`${completedSteps}/${totalSteps}`}
                prefix={<CheckCircleOutlined />}
              />
            </Col>
          </Row>

          <div className="mt-4">
            <Text strong>Query: </Text>
            <Text code className="bg-gray-50 p-2 rounded block mt-1">
              {currentSession.query}
            </Text>
          </div>

          <div className="mt-4">
            <Progress 
              percent={overallProgress} 
              status={currentSession.status === 'error' ? 'exception' : 'active'}
              strokeColor={{
                '0%': '#108ee9',
                '100%': '#87d068',
              }}
            />
          </div>
        </Card>

        {/* Performance Metrics */}
        {showAnalytics && (
          <Card title={<><BarChartOutlined /> Performance Metrics</>} size="small">
            <Row gutter={[16, 16]}>
              {performanceMetrics.map((metric, index) => (
                <Col span={6} key={index}>
                  <Statistic
                    title={metric.name}
                    value={metric.value}
                    suffix={metric.unit}
                    precision={metric.unit === 's' ? 1 : 0}
                    valueStyle={{ color: metric.color }}
                  />
                </Col>
              ))}
            </Row>
          </Card>
        )}
      </div>
    );
  };

  const renderStepsTab = () => {
    if (!currentSession) return null;

    const timelineItems = currentSession.steps.map((step: PipelineStep) => {
      const progress = stepProgress[step] || { 
        step, 
        status: 'pending', 
        progress: 0, 
        message: 'Waiting...' 
      };

      return {
        color: getStepStatusColor(progress.status),
        dot: getStepStatusIcon(progress.status),
        children: (
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <span className="text-lg">{getStepIcon(step)}</span>
                <Text strong>{step.replace(/([A-Z])/g, ' $1').trim()}</Text>
                <Tag color={getStepStatusColor(progress.status)}>
                  {progress.status}
                </Tag>
              </div>
              {progress.status === 'running' && (
                <Text type="secondary" className="text-sm">
                  {progress.progress}%
                </Text>
              )}
            </div>

            {progress.status === 'running' && (
              <Progress 
                percent={progress.progress} 
                size="small" 
                status="active"
                format={() => progress.message || 'Processing...'}
              />
            )}

            {progress.message && progress.status !== 'running' && (
              <Text type="secondary" className="text-sm block">
                {progress.message}
              </Text>
            )}

            {progress.startTime && (
              <Text type="secondary" className="text-xs block">
                Started: {new Date(progress.startTime).toLocaleTimeString()}
              </Text>
            )}

            {progress.endTime && (
              <Text type="secondary" className="text-xs block">
                Completed: {new Date(progress.endTime).toLocaleTimeString()}
                {progress.startTime && (
                  <span className="ml-2">
                    ({Math.round((new Date(progress.endTime).getTime() - new Date(progress.startTime).getTime()) / 1000)}s)
                  </span>
                )}
              </Text>
            )}

            {progress.details && progress.status === 'completed' && (
              <details className="mt-2">
                <summary className="cursor-pointer text-blue-600 text-sm">
                  View Details
                </summary>
                <pre className="bg-gray-50 p-2 rounded text-xs mt-1 overflow-auto max-h-32">
                  {JSON.stringify(progress.details, null, 2)}
                </pre>
              </details>
            )}
          </div>
        )
      };
    });

    return (
      <Card title="Step Progress" size="small">
        <Timeline items={timelineItems} />
      </Card>
    );
  };

  const renderLogsTab = () => {
    if (!showLogs) return null;

    // Mock logs for demonstration
    const mockLogs = [
      { timestamp: new Date().toISOString(), level: 'info', message: 'Pipeline test started', source: 'Controller' },
      { timestamp: new Date().toISOString(), level: 'debug', message: 'Business context analysis initiated', source: 'BusinessContextAnalyzer' },
      { timestamp: new Date().toISOString(), level: 'info', message: 'Token budget calculated', source: 'TokenBudgetManager' },
      { timestamp: new Date().toISOString(), level: 'warning', message: 'High token usage detected', source: 'TokenBudgetManager' },
    ];

    const columns = [
      {
        title: 'Timestamp',
        dataIndex: 'timestamp',
        key: 'timestamp',
        render: (timestamp: string) => new Date(timestamp).toLocaleTimeString()
      },
      {
        title: 'Level',
        dataIndex: 'level',
        key: 'level',
        render: (level: string) => (
          <Tag color={level === 'error' ? 'red' : level === 'warning' ? 'orange' : level === 'info' ? 'blue' : 'default'}>
            {level.toUpperCase()}
          </Tag>
        )
      },
      {
        title: 'Source',
        dataIndex: 'source',
        key: 'source'
      },
      {
        title: 'Message',
        dataIndex: 'message',
        key: 'message'
      }
    ];

    return (
      <Card title="System Logs" size="small">
        <Table
          columns={columns}
          dataSource={mockLogs}
          size="small"
          pagination={{ pageSize: 10 }}
        />
      </Card>
    );
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Title level={4}>
          <ThunderboltOutlined className="mr-2" />
          Enhanced Pipeline Monitoring
        </Title>
        {testId && (
          <Text type="secondary">Monitoring Test: {testId}</Text>
        )}
      </div>

      {renderConnectionStatus()}

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Overview" key="overview">
          {renderOverviewTab()}
        </TabPane>
        <TabPane tab="Step Details" key="steps">
          {renderStepsTab()}
        </TabPane>
        {showLogs && (
          <TabPane tab="System Logs" key="logs">
            {renderLogsTab()}
          </TabPane>
        )}
      </Tabs>
    </div>
  );
};

export default EnhancedMonitoringDashboard;
