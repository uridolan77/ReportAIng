import React, { useState, useEffect } from 'react';
import { Card, Timeline, Progress, Typography, Space, Tag, Collapse, Button, Divider } from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  EyeInvisibleOutlined,
  DatabaseOutlined,
  RobotOutlined,
  CodeOutlined,
  BarChartOutlined
} from '@ant-design/icons';

const { Text, Paragraph } = Typography;
const { Panel } = Collapse;

export interface ProcessingStage {
  stage: string;
  message: string;
  progress: number;
  timestamp: string;
  details?: any;
  status?: 'pending' | 'active' | 'completed' | 'error';
}

interface QueryProcessingViewerProps {
  stages: ProcessingStage[];
  isProcessing: boolean;
  currentStage?: string;
  queryId?: string;
  onToggleVisibility?: () => void;
  isVisible?: boolean;
}

export const QueryProcessingViewer: React.FC<QueryProcessingViewerProps> = ({
  stages,
  isProcessing,
  currentStage,
  queryId,
  onToggleVisibility,
  isVisible = true
}) => {
  const [expandedPanels, setExpandedPanels] = useState<string[]>([]);

  const getStageIcon = (stage: string, status: string = 'pending') => {
    const iconProps = { style: { fontSize: '16px' } };
    
    if (status === 'error') {
      return <ExclamationCircleOutlined {...iconProps} style={{ ...iconProps.style, color: '#ff4d4f' }} />;
    }
    
    if (status === 'completed') {
      return <CheckCircleOutlined {...iconProps} style={{ ...iconProps.style, color: '#52c41a' }} />;
    }
    
    if (status === 'active') {
      return <LoadingOutlined {...iconProps} style={{ ...iconProps.style, color: '#1890ff' }} />;
    }

    // Stage-specific icons
    switch (stage) {
      case 'started':
      case 'cache_check':
        return <ClockCircleOutlined {...iconProps} style={{ ...iconProps.style, color: '#8c8c8c' }} />;
      case 'schema_loading':
      case 'schema_analysis':
        return <DatabaseOutlined {...iconProps} style={{ ...iconProps.style, color: '#722ed1' }} />;
      case 'prompt_building':
      case 'prompt_details':
      case 'ai_processing':
      case 'ai_completed':
        return <RobotOutlined {...iconProps} style={{ ...iconProps.style, color: '#1890ff' }} />;
      case 'sql_validation':
      case 'sql_execution':
      case 'sql_completed':
        return <CodeOutlined {...iconProps} style={{ ...iconProps.style, color: '#52c41a' }} />;
      case 'confidence_calculation':
      case 'visualization_generation':
      case 'suggestions_generation':
        return <BarChartOutlined {...iconProps} style={{ ...iconProps.style, color: '#fa8c16' }} />;
      default:
        return <ClockCircleOutlined {...iconProps} style={{ ...iconProps.style, color: '#8c8c8c' }} />;
    }
  };

  const getStageColor = (stage: string, status: string = 'pending') => {
    if (status === 'error') return 'red';
    if (status === 'completed') return 'green';
    if (status === 'active') return 'blue';
    
    switch (stage) {
      case 'ai_processing':
      case 'ai_completed':
        return 'blue';
      case 'sql_execution':
      case 'sql_completed':
        return 'green';
      case 'schema_loading':
      case 'schema_analysis':
        return 'purple';
      default:
        return 'default';
    }
  };

  const formatStageTitle = (stage: string) => {
    return stage.split('_').map(word => 
      word.charAt(0).toUpperCase() + word.slice(1)
    ).join(' ');
  };

  const renderStageDetails = (stageData: ProcessingStage) => {
    if (!stageData.details) return null;

    return (
      <div style={{ 
        marginTop: '8px', 
        padding: '12px', 
        background: '#f8f9fa', 
        borderRadius: '6px',
        border: '1px solid #e9ecef'
      }}>
        <Text strong style={{ color: '#495057', fontSize: '12px' }}>Stage Details:</Text>
        <div style={{ marginTop: '8px' }}>
          {Object.entries(stageData.details).map(([key, value]) => (
            <div key={key} style={{ marginBottom: '4px' }}>
              <Text style={{ fontSize: '11px', color: '#6c757d' }}>
                <strong>{key}:</strong> {typeof value === 'string' ? value : JSON.stringify(value)}
              </Text>
            </div>
          ))}
        </div>
      </div>
    );
  };

  const currentProgress = stages.length > 0 ? Math.max(...stages.map(s => s.progress)) : 0;

  if (!isVisible) {
    return (
      <Button 
        type="text" 
        icon={<EyeOutlined />} 
        onClick={onToggleVisibility}
        style={{ marginBottom: '16px' }}
      >
        Show Processing Details
      </Button>
    );
  }

  return (
    <Card 
      size="small"
      title={
        <Space>
          <RobotOutlined style={{ color: '#1890ff' }} />
          <Text strong>Query Processing Details</Text>
          {queryId && (
            <Tag color="blue" style={{ fontSize: '10px' }}>
              ID: {queryId.substring(0, 8)}...
            </Tag>
          )}
          <Button 
            type="text" 
            size="small"
            icon={<EyeInvisibleOutlined />} 
            onClick={onToggleVisibility}
          >
            Hide
          </Button>
        </Space>
      }
      style={{ marginBottom: '16px' }}
    >
      {/* Overall Progress */}
      <div style={{ marginBottom: '16px' }}>
        <Progress
          percent={currentProgress}
          status={isProcessing ? 'active' : 'success'}
          strokeColor={{
            '0%': '#1890ff',
            '100%': '#52c41a',
          }}
          trailColor="#f0f0f0"
          strokeWidth={8}
        />
        <Text style={{ fontSize: '12px', color: '#8c8c8c' }}>
          Overall Progress: {currentProgress}%
        </Text>
      </div>

      {/* Processing Timeline */}
      <Timeline mode="left" style={{ marginTop: '16px' }}>
        {stages.map((stageData, index) => {
          const isCurrentStage = stageData.stage === currentStage;
          const status = isCurrentStage && isProcessing ? 'active' : 
                        stageData.progress === 100 ? 'completed' : 'pending';
          
          return (
            <Timeline.Item
              key={`${stageData.stage}-${index}`}
              dot={getStageIcon(stageData.stage, status)}
              color={getStageColor(stageData.stage, status)}
            >
              <div>
                <Space direction="vertical" size="small" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text strong style={{ color: status === 'active' ? '#1890ff' : '#262626' }}>
                      {formatStageTitle(stageData.stage)}
                    </Text>
                    <Space>
                      <Tag color={getStageColor(stageData.stage, status)} size="small">
                        {stageData.progress}%
                      </Tag>
                      <Text style={{ fontSize: '11px', color: '#8c8c8c' }}>
                        {new Date(stageData.timestamp).toLocaleTimeString()}
                      </Text>
                    </Space>
                  </div>
                  
                  <Text style={{ fontSize: '13px', color: '#595959' }}>
                    {stageData.message}
                  </Text>

                  {stageData.details && (
                    <Collapse 
                      ghost 
                      size="small"
                      activeKey={expandedPanels}
                      onChange={(keys) => setExpandedPanels(keys as string[])}
                    >
                      <Panel 
                        header={
                          <Text style={{ fontSize: '11px', color: '#1890ff' }}>
                            View Technical Details
                          </Text>
                        } 
                        key={`details-${index}`}
                        style={{ padding: 0 }}
                      >
                        {renderStageDetails(stageData)}
                      </Panel>
                    </Collapse>
                  )}
                </Space>
              </div>
            </Timeline.Item>
          );
        })}
      </Timeline>

      {isProcessing && (
        <div style={{ 
          textAlign: 'center', 
          padding: '16px',
          background: '#f6ffed',
          border: '1px solid #b7eb8f',
          borderRadius: '6px',
          marginTop: '16px'
        }}>
          <Space>
            <LoadingOutlined style={{ color: '#52c41a' }} />
            <Text style={{ color: '#52c41a', fontWeight: 500 }}>
              Processing in progress...
            </Text>
          </Space>
        </div>
      )}
    </Card>
  );
};

export default QueryProcessingViewer;
