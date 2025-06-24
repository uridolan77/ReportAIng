import React, { useEffect } from 'react';
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
  Tooltip
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
  HeartOutlined
} from '@ant-design/icons';
import { usePipelineTestMonitoring } from '../hooks/usePipelineTestMonitoring';
import { PipelineStep } from '../types/aiPipelineTest';

const { Title, Text } = Typography;

interface PipelineTestMonitoringDashboardProps {
  testId?: string;
  autoJoinSession?: boolean;
}

const PipelineTestMonitoringDashboard: React.FC<PipelineTestMonitoringDashboardProps> = ({
  testId,
  autoJoinSession = true
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

  // Auto-join test session when testId is provided
  useEffect(() => {
    if (testId && isConnected && autoJoinSession) {
      joinTestSession(testId);
    }
  }, [testId, isConnected, autoJoinSession, joinTestSession]);

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

  const renderSessionOverview = () => {
    if (!currentSession) {
      return (
        <Card title="Session Status" className="mb-4">
          <Alert
            message="No Active Session"
            description="Start a pipeline test to see real-time monitoring data."
            type="info"
            showIcon
          />
        </Card>
      );
    }

    const completedSteps = Object.values(stepProgress).filter(s => s.status === 'completed').length;
    const totalSteps = currentSession.steps.length;
    const overallProgress = totalSteps > 0 ? Math.round((completedSteps / totalSteps) * 100) : 0;

    return (
      <Card title="Session Overview" className="mb-4">
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
    );
  };

  const renderStepProgress = () => {
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
      <Card title="Step Progress" className="mb-4">
        <Timeline items={timelineItems} />
      </Card>
    );
  };

  const renderPerformanceMetrics = () => {
    if (!currentSession || !Object.keys(stepProgress).length) return null;

    const completedSteps = Object.values(stepProgress).filter(s => s.status === 'completed');
    const avgDuration = completedSteps.length > 0 
      ? completedSteps.reduce((sum, step) => {
          if (step.startTime && step.endTime) {
            return sum + (new Date(step.endTime).getTime() - new Date(step.startTime).getTime());
          }
          return sum;
        }, 0) / completedSteps.length / 1000
      : 0;

    const totalDuration = currentSession.startTime && currentSession.endTime
      ? (new Date(currentSession.endTime).getTime() - new Date(currentSession.startTime).getTime()) / 1000
      : currentSession.startTime
      ? (Date.now() - new Date(currentSession.startTime).getTime()) / 1000
      : 0;

    return (
      <Card title="Performance Metrics" size="small">
        <Row gutter={[16, 16]}>
          <Col span={8}>
            <Statistic
              title="Total Duration"
              value={totalDuration}
              suffix="s"
              precision={1}
            />
          </Col>
          <Col span={8}>
            <Statistic
              title="Avg Step Duration"
              value={avgDuration}
              suffix="s"
              precision={1}
            />
          </Col>
          <Col span={8}>
            <Statistic
              title="Completed Steps"
              value={completedSteps.length}
              suffix={`/ ${currentSession.steps.length}`}
            />
          </Col>
        </Row>
      </Card>
    );
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Title level={4}>Real-time Pipeline Monitoring</Title>
        {testId && (
          <Text type="secondary">Monitoring Test: {testId}</Text>
        )}
      </div>

      {renderConnectionStatus()}
      {renderSessionOverview()}
      {renderStepProgress()}
      {renderPerformanceMetrics()}
    </div>
  );
};

export default PipelineTestMonitoringDashboard;
