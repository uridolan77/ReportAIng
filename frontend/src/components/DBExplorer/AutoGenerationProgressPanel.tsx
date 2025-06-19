import React, { useState, useEffect } from 'react';
import {
  Modal,
  Steps,
  Progress,
  Typography,
  Space,
  Card,
  Collapse,
  Tag,
  Button,
  Tabs,
  List,
  Spin,
  Timeline,
  Divider,
  Alert
} from 'antd';
import {
  LoadingOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  EyeOutlined,
  DownloadOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  BulbOutlined,
  BookOutlined
} from '@ant-design/icons';

const { Title, Text, Paragraph } = Typography;
const { Panel } = Collapse;
const { TabPane } = Tabs;

export interface ProgressStep {
  id: string;
  title: string;
  description: string;
  status: 'waiting' | 'process' | 'finish' | 'error';
  startTime?: Date;
  endTime?: Date;
  details?: string;
  subSteps?: ProgressStep[];
}

export interface GenerationPhase {
  phase: string;
  status: 'pending' | 'active' | 'completed' | 'error';
  progress: number;
  message: string;
  details?: {
    currentTable?: string;
    tablesProcessed?: number;
    totalTables?: number;
    currentOperation?: string;
    prompt?: string;
    response?: string;
    errors?: string[];
    warnings?: string[];
  };
  timestamp: Date;
}

export interface AutoGenerationProgressPanelProps {
  visible: boolean;
  onClose: () => void;
  phases: GenerationPhase[];
  overallProgress: number;
  isCompleted: boolean;
  hasErrors: boolean;
  onViewResults?: (() => void) | undefined;
  onRetry?: (() => void) | undefined;
  onCancel?: (() => void) | undefined;
}

export const AutoGenerationProgressPanel: React.FC<AutoGenerationProgressPanelProps> = ({
  visible,
  onClose,
  phases,
  overallProgress,
  isCompleted,
  hasErrors,
  onViewResults,
  onRetry,
  onCancel
}) => {
  const [activeTab, setActiveTab] = useState('progress');
  const [expandedPhases, setExpandedPhases] = useState<string[]>([]);

  // Auto-expand active phase
  useEffect(() => {
    const activePhase = phases.find(p => p.status === 'active');
    if (activePhase && !expandedPhases.includes(activePhase.phase)) {
      setExpandedPhases(prev => [...prev, activePhase.phase]);
    }
  }, [phases, expandedPhases]);

  const getPhaseIcon = (status: GenerationPhase['status']) => {
    switch (status) {
      case 'pending':
        return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
      case 'active':
        return <LoadingOutlined style={{ color: '#1890ff' }} />;
      case 'completed':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'error':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
      default:
        return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getPhaseColor = (status: GenerationPhase['status']) => {
    switch (status) {
      case 'pending': return '#d9d9d9';
      case 'active': return '#1890ff';
      case 'completed': return '#52c41a';
      case 'error': return '#ff4d4f';
      default: return '#d9d9d9';
    }
  };

  const formatDuration = (start: Date, end?: Date) => {
    const endTime = end || new Date();
    const duration = endTime.getTime() - start.getTime();
    const seconds = Math.floor(duration / 1000);
    const minutes = Math.floor(seconds / 60);
    
    if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    }
    return `${seconds}s`;
  };

  const renderPhaseDetails = (phase: GenerationPhase) => {
    if (!phase.details) return null;

    return (
      <div style={{ marginTop: '12px' }}>
        <Space direction="vertical" style={{ width: '100%' }} size="small">
          {phase.details.currentTable && (
            <Text type="secondary">
              <DatabaseOutlined /> Processing: <Text code>{phase.details.currentTable}</Text>
            </Text>
          )}
          
          {phase.details.tablesProcessed !== undefined && phase.details.totalTables && (
            <div>
              <Text type="secondary">
                Progress: {phase.details.tablesProcessed} / {phase.details.totalTables} tables
              </Text>
              <Progress 
                percent={Math.round((phase.details.tablesProcessed / phase.details.totalTables) * 100)}
                size="small"
                style={{ marginTop: '4px' }}
              />
            </div>
          )}

          {phase.details.currentOperation && (
            <Text type="secondary">
              <BulbOutlined /> {phase.details.currentOperation}
            </Text>
          )}

          {phase.details.errors && phase.details.errors.length > 0 && (
            <Alert
              type="error"
              size="small"
              message={`${phase.details.errors.length} error(s) occurred`}
              description={
                <List
                  size="small"
                  dataSource={phase.details.errors}
                  renderItem={(error) => <List.Item>{error}</List.Item>}
                />
              }
            />
          )}

          {phase.details.warnings && phase.details.warnings.length > 0 && (
            <Alert
              type="warning"
              size="small"
              message={`${phase.details.warnings.length} warning(s)`}
              description={
                <List
                  size="small"
                  dataSource={phase.details.warnings}
                  renderItem={(warning) => <List.Item>{warning}</List.Item>}
                />
              }
            />
          )}
        </Space>
      </div>
    );
  };

  const renderPromptResponse = (phase: GenerationPhase) => {
    if (!phase.details?.prompt && !phase.details?.response) return null;

    return (
      <Tabs size="small" style={{ marginTop: '12px' }}>
        {phase.details.prompt && (
          <TabPane tab="Prompt" key="prompt">
            <Card size="small" style={{ maxHeight: '200px', overflow: 'auto' }}>
              <Paragraph>
                <pre style={{ whiteSpace: 'pre-wrap', fontSize: '12px' }}>
                  {phase.details.prompt}
                </pre>
              </Paragraph>
            </Card>
          </TabPane>
        )}
        
        {phase.details.response && (
          <TabPane tab="Response" key="response">
            <Card size="small" style={{ maxHeight: '200px', overflow: 'auto' }}>
              <Paragraph>
                <pre style={{ whiteSpace: 'pre-wrap', fontSize: '12px' }}>
                  {phase.details.response}
                </pre>
              </Paragraph>
            </Card>
          </TabPane>
        )}
      </Tabs>
    );
  };

  const renderProgressTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Overall Progress */}
      <Card size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Title level={5} style={{ margin: 0 }}>Overall Progress</Title>
            <Text strong>{overallProgress}%</Text>
          </div>
          <Progress 
            percent={overallProgress} 
            status={hasErrors ? 'exception' : isCompleted ? 'success' : 'active'}
            strokeWidth={8}
          />
          <Text type="secondary">
            {isCompleted 
              ? hasErrors 
                ? 'Completed with errors' 
                : 'Successfully completed'
              : 'Processing...'
            }
          </Text>
        </Space>
      </Card>

      {/* Phase Timeline */}
      <Card size="small" title="Generation Phases">
        <Timeline>
          {phases.map((phase, index) => (
            <Timeline.Item
              key={phase.phase}
              dot={getPhaseIcon(phase.status)}
              color={getPhaseColor(phase.status)}
            >
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text strong>{phase.phase}</Text>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {formatDuration(phase.timestamp)}
                  </Text>
                </div>
                <Text type="secondary">{phase.message}</Text>
                {renderPhaseDetails(phase)}
              </div>
            </Timeline.Item>
          ))}
        </Timeline>
      </Card>
    </Space>
  );

  const renderDetailsTab = () => (
    <Collapse 
      activeKey={expandedPhases}
      onChange={setExpandedPhases}
      size="small"
    >
      {phases.map((phase) => (
        <Panel
          key={phase.phase}
          header={
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Space>
                {getPhaseIcon(phase.status)}
                <Text strong>{phase.phase}</Text>
                <Tag color={getPhaseColor(phase.status)}>
                  {phase.status.toUpperCase()}
                </Tag>
              </Space>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {phase.progress}%
              </Text>
            </div>
          }
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text>{phase.message}</Text>
            {renderPhaseDetails(phase)}
            {renderPromptResponse(phase)}
          </Space>
        </Panel>
      ))}
    </Collapse>
  );

  return (
    <Modal
      title={
        <Space>
          <BulbOutlined />
          <span>Auto-Generation Progress</span>
          {!isCompleted && <Spin size="small" />}
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width={1200}
      centered
      maskClosable={false}
      closable={isCompleted}
      destroyOnClose={false}
      footer={[
        <Button key="close" onClick={onClose}>
          Close
        </Button>,
        ...(isCompleted && onViewResults ? [
          <Button key="view" type="primary" icon={<EyeOutlined />} onClick={onViewResults}>
            View Results
          </Button>
        ] : []),
        ...(hasErrors && onRetry ? [
          <Button key="retry" icon={<LoadingOutlined />} onClick={onRetry}>
            Retry
          </Button>
        ] : []),
        ...(!isCompleted && onCancel ? [
          <Button key="cancel" danger onClick={onCancel}>
            Cancel
          </Button>
        ] : [])
      ]}
    >
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Progress" key="progress">
          {renderProgressTab()}
        </TabPane>
        <TabPane tab="Details" key="details">
          {renderDetailsTab()}
        </TabPane>
      </Tabs>
    </Modal>
  );
};
