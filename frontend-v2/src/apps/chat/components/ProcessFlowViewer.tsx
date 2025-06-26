import React, { useState, useEffect } from 'react';
import {
  Drawer,
  Timeline,
  Card,
  Typography,
  Space,
  Tag,
  Progress,
  Collapse,
  Button,
  Tooltip,
  Divider,
  Alert,
  Spin,
  Badge
} from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  ApiOutlined,
  DatabaseOutlined,
  BranchesOutlined,
  ThunderboltOutlined,
  CodeOutlined,
  PlayCircleOutlined,
  FileTextOutlined
} from '@ant-design/icons';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { tomorrow } from 'react-syntax-highlighter/dist/esm/styles/prism';
import type { ProcessFlowSession, ProcessFlowStep, ProcessFlowLog } from '@shared/types/transparency';
import { useProcessFlowSession } from '@shared/store/api/transparencyApi';

const { Title, Text, Paragraph } = Typography;

interface ProcessFlowViewerProps {
  visible: boolean;
  onClose: () => void;
  sessionId: string; // ProcessFlow session ID
  realTimeUpdates?: boolean;
}

const getStepIcon = (step: ProcessFlowStep) => {
  switch (step.status) {
    case 'Completed':
      return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
    case 'InProgress':
      return <LoadingOutlined style={{ color: '#1890ff' }} />;
    case 'Failed':
      return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
    default:
      return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
  }
};

const getStepColor = (status: string) => {
  switch (status) {
    case 'Completed': return '#52c41a';
    case 'InProgress': return '#1890ff';
    case 'Failed': return '#ff4d4f';
    default: return '#d9d9d9';
  }
};

const formatDuration = (ms?: number) => {
  if (!ms) return 'N/A';
  if (ms < 1000) return `${ms}ms`;
  return `${(ms / 1000).toFixed(2)}s`;
};

const ProcessStepCard: React.FC<{
  step: ProcessFlowStep;
  level?: number;
  logs?: ProcessFlowLog[];
}> = ({ step, level = 0, logs = [] }) => {
  const [expanded, setExpanded] = useState(false);

  // Get step-specific logs
  const stepLogs = logs.filter(log => log.stepId === step.stepId);

  // Check if we have details to show
  const hasDetails = step.inputData || step.outputData || step.errorMessage || stepLogs.length > 0;

  return (
    <Card
      size="small"
      style={{
        marginLeft: level * 16,
        marginBottom: 8,
        border: `1px solid ${getStepColor(stepStatus)}20`,
        borderLeft: `4px solid ${getStepColor(stepStatus)}`
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div style={{ flex: 1 }}>
          <Space>
            {getStepIcon(step)}
            <Text strong>{step.name}</Text>
            <Tag color={getStepColor(step.status)}>{step.status.toUpperCase()}</Tag>
            {step.durationMs && (
              <Tag color="blue">{formatDuration(step.durationMs)}</Tag>
            )}
            {step.confidence && (
              <Tag color="purple">{(step.confidence * 100).toFixed(1)}%</Tag>
            )}
          </Space>
          <div style={{ marginTop: 4 }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {step.stepType} step
            </Text>
            {step.errorMessage && (
              <div style={{ marginTop: 4 }}>
                <Text type="danger" style={{ fontSize: '12px' }}>
                  Error: {step.errorMessage}
                </Text>
              </div>
            )}
          </div>
        </div>

        {hasDetails && (
          <Button
            type="text"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => setExpanded(!expanded)}
          >
            Details
          </Button>
        )}
      </div>

      {expanded && hasDetails && (
        <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid #f0f0f0' }}>
          <Collapse
            size="small"
            ghost
            items={[
              ...(step.inputData ? [{
                key: 'inputData',
                label: 'Input Data',
                children: (
                  <SyntaxHighlighter
                    language="json"
                    style={tomorrow}
                    customStyle={{ fontSize: '11px', margin: 0 }}
                  >
                    {JSON.stringify(step.inputData, null, 2)}
                  </SyntaxHighlighter>
                )
              }] : []),
              ...(step.outputData ? [{
                key: 'outputData',
                label: 'Output Data',
                children: (
                  <SyntaxHighlighter
                    language="json"
                    style={tomorrow}
                    customStyle={{ fontSize: '11px', margin: 0 }}
                  >
                    {JSON.stringify(step.outputData, null, 2)}
                  </SyntaxHighlighter>
                )
              }] : []),
              ...(stepLogs.length > 0 ? [{
                key: 'stepLogs',
                label: `Logs (${stepLogs.length})`,
                children: (
                  <div style={{ maxHeight: '200px', overflow: 'auto' }}>
                    {stepLogs.map((log, index) => (
                      <div key={index} style={{
                        fontSize: '11px',
                        fontFamily: 'monospace',
                        padding: '2px 0',
                        borderBottom: index < stepLogs.length - 1 ? '1px solid #f0f0f0' : 'none',
                        color: log.level === 'Error' ? '#ff4d4f' :
                               log.level === 'Warning' ? '#faad14' :
                               log.level === 'Info' ? '#1890ff' : '#666'
                      }}>
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          [{log.level}] {new Date(log.timestamp).toLocaleTimeString()}:
                        </Text> {log.message}
                      </div>
                    ))}
                  </div>
                )
              }] : [])
            ]}
          />
        </div>
      )}

      {step.subSteps && step.subSteps.length > 0 && (
        <div style={{ marginTop: 12 }}>
          {step.subSteps.map((subStep) => (
            <ProcessStepCard key={subStep.id} step={subStep} level={level + 1} />
          ))}
        </div>
      )}
    </Card>
  );
};

export const ProcessFlowViewer: React.FC<ProcessFlowViewerProps> = ({
  visible,
  onClose,
  sessionId,
  realTimeUpdates = false
}) => {
  const [currentStep, setCurrentStep] = useState<string | null>(null);

  // Use ProcessFlow session data
  const {
    session,
    steps,
    logs,
    transparency,
    isLoading,
    error
  } = useProcessFlowSession(sessionId);

  useEffect(() => {
    if (session && realTimeUpdates) {
      const runningStep = steps.find(step => step.status === 'InProgress');
      setCurrentStep(runningStep?.stepId || null);
    }
  }, [session, steps, realTimeUpdates]);

  if (isLoading) {
    return (
      <Drawer
        title="ProcessFlow Session"
        placement="right"
        width={800}
        onClose={onClose}
        open={visible}
      >
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text type="secondary">Loading ProcessFlow session...</Text>
          </div>
        </div>
      </Drawer>
    );
  }

  if (error) {
    return (
      <Drawer
        title="ProcessFlow Session"
        placement="right"
        width={800}
        onClose={onClose}
        open={visible}
      >
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Alert
            message="Error Loading ProcessFlow Session"
            description="Failed to load ProcessFlow session data"
            type="error"
            showIcon
          />
        </div>
      </Drawer>
    );
  }

  if (!session) {
    return (
      <Drawer
        title="ProcessFlow Session"
        placement="right"
        width={800}
        onClose={onClose}
        open={visible}
      >
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Text type="secondary">No ProcessFlow session data available</Text>
        </div>
      </Drawer>
    );
  }

  const completedSteps = steps.filter(s => s.status === 'Completed').length;
  const totalSteps = steps.length;
  const progressPercent = totalSteps > 0 ? (completedSteps / totalSteps) * 100 : 0;

  return (
    <Drawer
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <ApiOutlined />
          <span>ProcessFlow Session</span>
          {realTimeUpdates && session.status === 'InProgress' && (
            <Badge status="processing" text="Live" />
          )}
        </div>
      }
      placement="right"
      width={900}
      onClose={onClose}
      open={visible}
      extra={
        <Space>
          <Tag color="blue">Session: {session.sessionId.slice(-8)}</Tag>
          <Tag color={session.status === 'Completed' ? 'green' : session.status === 'Failed' ? 'red' : 'orange'}>
            {session.status.toUpperCase()}
          </Tag>
        </Space>
      }
    >
      <div style={{ padding: '0 0 24px' }}>
        {/* Query Information */}
        <Card style={{ marginBottom: 16 }}>
          <Title level={5}>Query Information</Title>
          <Paragraph style={{ margin: 0, fontSize: '14px' }}>
            <Text strong>Query:</Text> {session.userQuery}
          </Paragraph>
          <div style={{ marginTop: 8, display: 'flex', gap: '16px', fontSize: '12px' }}>
            <Text type="secondary">User: {session.userId}</Text>
            <Text type="secondary">Started: {new Date(session.startTime).toLocaleTimeString()}</Text>
            {session.totalDurationMs && (
              <Text type="secondary">Duration: {formatDuration(session.totalDurationMs)}</Text>
            )}
          </div>
        </Card>

        {/* Progress Overview */}
        <Card style={{ marginBottom: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <Title level={5} style={{ margin: 0 }}>Progress Overview</Title>
            <Text>{completedSteps}/{totalSteps} steps completed</Text>
          </div>
          <Progress
            percent={progressPercent}
            status={session.status === 'Failed' ? 'exception' : session.status === 'InProgress' ? 'active' : 'success'}
            strokeColor={session.status === 'Failed' ? '#ff4d4f' : '#52c41a'}
          />
        </Card>

        {/* Transparency Information */}
        {transparency && (
          <Card style={{ marginBottom: 16 }}>
            <Title level={5}>AI Transparency</Title>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))', gap: '12px' }}>
              {transparency.model && (
                <div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>Model</Text>
                  <div><Text strong>{transparency.model}</Text></div>
                </div>
              )}
              {transparency.confidence !== undefined && (
                <div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>Confidence</Text>
                  <div><Text strong>{(transparency.confidence * 100).toFixed(1)}%</Text></div>
                </div>
              )}
              {transparency.totalTokens && (
                <div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>Total Tokens</Text>
                  <div><Text strong>{transparency.totalTokens.toLocaleString()}</Text></div>
                </div>
              )}
              {transparency.temperature !== undefined && (
                <div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>Temperature</Text>
                  <div><Text strong>{transparency.temperature}</Text></div>
                </div>
              )}
            </div>
          </Card>
        )}

        {/* Process Steps */}
        <Card>
          <Title level={5}>Process Steps</Title>
          {session.status === 'InProgress' && (
            <Alert
              message="ProcessFlow is running"
              description="Steps will update in real-time as they complete"
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          <div style={{ maxHeight: '60vh', overflow: 'auto' }}>
            {steps.map((step) => (
              <ProcessStepCard
                key={step.stepId}
                step={step}
                logs={logs}
              />
            ))}
          </div>
        </Card>
      </div>
    </Drawer>
  );
};

export default ProcessFlowViewer;
