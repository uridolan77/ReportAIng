import React, { useState } from 'react';
import { 
  Card, 
  Typography, 
  Space, 
  Tag, 
  Progress, 
  Collapse, 
  Button,
  Alert,
  Spin,
  Descriptions,
  Timeline,
  Statistic,
  Row,
  Col
} from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  ApiOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  PercentageOutlined
} from '@ant-design/icons';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { tomorrow } from 'react-syntax-highlighter/dist/esm/styles/prism';
import type { ProcessFlowSession, ProcessFlowStep, ProcessFlowLog } from '@shared/types/transparency';
import { useProcessFlowSession } from '@shared/store/api/transparencyApi';

const { Title, Text, Paragraph } = Typography;
const { Panel } = Collapse;

export interface ProcessFlowSessionViewerProps {
  sessionId: string;
  showDetailedSteps?: boolean;
  showLogs?: boolean;
  showTransparency?: boolean;
  compact?: boolean;
  className?: string;
  testId?: string;
}

const getStepIcon = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed':
      return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
    case 'inprogress':
      return <LoadingOutlined style={{ color: '#1890ff' }} />;
    case 'failed':
      return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
    default:
      return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
  }
};

const getStepColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed': return 'success';
    case 'inprogress': return 'processing';
    case 'failed': return 'error';
    default: return 'default';
  }
};

const formatDuration = (ms?: number) => {
  if (!ms) return 'N/A';
  if (ms < 1000) return `${ms}ms`;
  return `${(ms / 1000).toFixed(2)}s`;
};

/**
 * ProcessFlowSessionViewer - Displays ProcessFlow session data with comprehensive details
 * 
 * Features:
 * - Session overview with status and metrics
 * - Step-by-step process visualization
 * - Real-time logs display
 * - AI transparency information
 * - Performance metrics
 */
export const ProcessFlowSessionViewer: React.FC<ProcessFlowSessionViewerProps> = ({
  sessionId,
  showDetailedSteps = true,
  showLogs = true,
  showTransparency = true,
  compact = false,
  className,
  testId = 'processflow-session-viewer'
}) => {
  const [expandedSteps, setExpandedSteps] = useState<string[]>([]);
  
  const { 
    session, 
    steps, 
    logs, 
    transparency,
    isLoading,
    error,
    refetch
  } = useProcessFlowSession(sessionId);

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>
          <Text type="secondary">Loading ProcessFlow session...</Text>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Session"
        description="Failed to load ProcessFlow session data"
        type="error"
        showIcon
        action={
          <Button size="small" onClick={refetch}>
            Retry
          </Button>
        }
      />
    );
  }

  if (!session) {
    return (
      <Alert
        message="Session Not Found"
        description="The requested ProcessFlow session could not be found"
        type="warning"
        showIcon
      />
    );
  }

  const completedSteps = steps.filter(s => s.status === 'Completed').length;
  const totalSteps = steps.length;
  const progressPercent = totalSteps > 0 ? (completedSteps / totalSteps) * 100 : 0;

  const toggleStepExpansion = (stepId: string) => {
    setExpandedSteps(prev => 
      prev.includes(stepId) 
        ? prev.filter(id => id !== stepId)
        : [...prev, stepId]
    );
  };

  return (
    <div className={className} data-testid={testId}>
      {/* Session Overview */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={[16, 16]}>
          <Col span={compact ? 24 : 16}>
            <Title level={4}>
              <ApiOutlined /> ProcessFlow Session
            </Title>
            <Descriptions column={compact ? 1 : 2} size="small">
              <Descriptions.Item label="Session ID">
                <Text code>{session.sessionId}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={getStepColor(session.status)}>{session.status}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="User Query">
                <Paragraph ellipsis={{ rows: 2, expandable: true }}>
                  {session.userQuery}
                </Paragraph>
              </Descriptions.Item>
              <Descriptions.Item label="Query Type">
                <Tag>{session.queryType}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Started">
                {new Date(session.startTime).toLocaleString()}
              </Descriptions.Item>
              {session.endTime && (
                <Descriptions.Item label="Completed">
                  {new Date(session.endTime).toLocaleString()}
                </Descriptions.Item>
              )}
            </Descriptions>
          </Col>
          
          {!compact && (
            <Col span={8}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <Statistic
                  title="Progress"
                  value={progressPercent}
                  precision={1}
                  suffix="%"
                  prefix={<PercentageOutlined />}
                />
                <Progress 
                  percent={progressPercent}
                  status={session.status === 'Failed' ? 'exception' : 
                          session.status === 'InProgress' ? 'active' : 'success'}
                />
                {session.totalDurationMs && (
                  <Statistic
                    title="Duration"
                    value={formatDuration(session.totalDurationMs)}
                    prefix={<ClockCircleOutlined />}
                  />
                )}
                {session.overallConfidence && (
                  <Statistic
                    title="Confidence"
                    value={session.overallConfidence * 100}
                    precision={1}
                    suffix="%"
                    prefix={<ThunderboltOutlined />}
                  />
                )}
              </Space>
            </Col>
          )}
        </Row>
      </Card>

      {/* AI Transparency */}
      {showTransparency && transparency && (
        <Card style={{ marginBottom: 16 }}>
          <Title level={5}>AI Transparency</Title>
          <Row gutter={[16, 8]}>
            {transparency.model && (
              <Col span={compact ? 12 : 6}>
                <Statistic title="Model" value={transparency.model} />
              </Col>
            )}
            {transparency.totalTokens && (
              <Col span={compact ? 12 : 6}>
                <Statistic 
                  title="Total Tokens" 
                  value={transparency.totalTokens.toLocaleString()} 
                />
              </Col>
            )}
            {transparency.estimatedCost && (
              <Col span={compact ? 12 : 6}>
                <Statistic 
                  title="Estimated Cost" 
                  value={transparency.estimatedCost}
                  precision={4}
                  prefix={<DollarOutlined />}
                />
              </Col>
            )}
            {transparency.apiCallCount && (
              <Col span={compact ? 12 : 6}>
                <Statistic title="API Calls" value={transparency.apiCallCount} />
              </Col>
            )}
          </Row>
        </Card>
      )}

      {/* Process Steps */}
      {showDetailedSteps && (
        <Card>
          <Title level={5}>Process Steps ({steps.length})</Title>
          <Timeline>
            {steps.map((step) => (
              <Timeline.Item
                key={step.stepId}
                dot={getStepIcon(step.status)}
                color={getStepColor(step.status)}
              >
                <div style={{ marginBottom: 8 }}>
                  <Space>
                    <Text strong>{step.name}</Text>
                    <Tag color={getStepColor(step.status)}>{step.status}</Tag>
                    {step.durationMs && (
                      <Tag color="blue">{formatDuration(step.durationMs)}</Tag>
                    )}
                    {step.confidence && (
                      <Tag color="purple">{(step.confidence * 100).toFixed(1)}%</Tag>
                    )}
                  </Space>
                  
                  <div style={{ marginTop: 4 }}>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {step.stepType}
                    </Text>
                    {step.errorMessage && (
                      <div style={{ marginTop: 4 }}>
                        <Text type="danger" style={{ fontSize: '12px' }}>
                          Error: {step.errorMessage}
                        </Text>
                      </div>
                    )}
                  </div>

                  {(step.inputData || step.outputData || 
                    (showLogs && logs.filter(log => log.stepId === step.stepId).length > 0)) && (
                    <div style={{ marginTop: 8 }}>
                      <Button
                        size="small"
                        type="link"
                        icon={<EyeOutlined />}
                        onClick={() => toggleStepExpansion(step.stepId)}
                      >
                        {expandedSteps.includes(step.stepId) ? 'Hide' : 'Show'} Details
                      </Button>
                    </div>
                  )}

                  {expandedSteps.includes(step.stepId) && (
                    <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid #f0f0f0' }}>
                      <Collapse size="small" ghost>
                        {step.inputData && (
                          <Panel header="Input Data" key="input">
                            <SyntaxHighlighter 
                              language="json" 
                              style={tomorrow}
                              customStyle={{ fontSize: '11px', margin: 0 }}
                            >
                              {JSON.stringify(step.inputData, null, 2)}
                            </SyntaxHighlighter>
                          </Panel>
                        )}
                        
                        {step.outputData && (
                          <Panel header="Output Data" key="output">
                            <SyntaxHighlighter 
                              language="json" 
                              style={tomorrow}
                              customStyle={{ fontSize: '11px', margin: 0 }}
                            >
                              {JSON.stringify(step.outputData, null, 2)}
                            </SyntaxHighlighter>
                          </Panel>
                        )}
                        
                        {showLogs && logs.filter(log => log.stepId === step.stepId).length > 0 && (
                          <Panel 
                            header={`Logs (${logs.filter(log => log.stepId === step.stepId).length})`} 
                            key="logs"
                          >
                            <div style={{ maxHeight: '200px', overflow: 'auto' }}>
                              {logs
                                .filter(log => log.stepId === step.stepId)
                                .map((log, index) => (
                                  <div key={index} style={{ 
                                    fontSize: '11px', 
                                    fontFamily: 'monospace',
                                    padding: '2px 0',
                                    borderBottom: '1px solid #f0f0f0',
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
                          </Panel>
                        )}
                      </Collapse>
                    </div>
                  )}
                </div>
              </Timeline.Item>
            ))}
          </Timeline>
        </Card>
      )}
    </div>
  );
};

export default ProcessFlowSessionViewer;
