import React, { useState } from 'react';
import { Card, Timeline, Progress, Typography, Space, Tag, Collapse, Button } from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  DatabaseOutlined,
  RobotOutlined,
  CodeOutlined,
  BarChartOutlined
} from '@ant-design/icons';
import './QueryProcessingViewer.css';

const { Text } = Typography;
const { Panel } = Collapse;

export interface ProcessingStage {
  stage: string;
  message: string;
  progress: number;
  timestamp: string;
  details?: any;
  status?: 'pending' | 'active' | 'completed' | 'error';
}

type ProcessingViewMode = 'minimal' | 'processing' | 'advanced' | 'hidden';

interface QueryProcessingViewerProps {
  stages: ProcessingStage[];
  isProcessing: boolean;
  currentStage?: string;
  queryId?: string;
  onToggleVisibility?: () => void;
  isVisible?: boolean;
  mode?: ProcessingViewMode;
  onModeChange?: (mode: ProcessingViewMode) => void;
}

export const QueryProcessingViewer: React.FC<QueryProcessingViewerProps> = ({
  stages,
  isProcessing,
  currentStage,
  queryId,
  onToggleVisibility,
  isVisible = true,
  mode = 'minimal',
  onModeChange
}) => {
  // ALL HOOKS MUST BE CALLED BEFORE ANY CONDITIONAL RETURNS
  const [expandedPanels, setExpandedPanels] = useState<string[]>([]);

  // Debug logging
  React.useEffect(() => {
    console.log('🔍 QueryProcessingViewer props:', {
      stagesCount: stages.length,
      isProcessing,
      currentStage,
      queryId,
      isVisible,
      stages: stages.map(s => ({
        stage: s?.stage || 'undefined',
        progress: s?.progress || 0,
        message: s?.message || 'undefined',
        timestamp: s?.timestamp || 'undefined'
      }))
    });
  }, [stages, isProcessing, currentStage, queryId, isVisible]);

  // Helper function to format stage titles
  const formatStageTitle = (stage: string) => {
    if (!stage || typeof stage !== 'string') {
      return 'Unknown Stage';
    }
    return stage.split('_').map(word =>
      word.charAt(0).toUpperCase() + word.slice(1)
    ).join(' ');
  };

  // Calculate values used in different modes with improved progress calculation
  const calculateOverallProgress = () => {
    if (stages.length === 0) return 0;

    // Define stage weights for more realistic progress
    const stageWeights: Record<string, number> = {
      'initializing': 5,
      'connecting': 5,
      'started': 5,
      'cache_check': 5,
      'schema_loading': 10,
      'schema_analysis': 10,
      'prompt_building': 10,
      'ai_processing': 30,
      'ai_completed': 5,
      'sql_validation': 5,
      'sql_execution': 15,
      'confidence_calculation': 5,
      'visualization_generation': 5,
      'suggestions_generation': 5,
      'completed': 0
    };

    let totalWeight = 0;
    let completedWeight = 0;

    stages.forEach(stage => {
      const weight = stageWeights[stage.stage] || 5;
      totalWeight += weight;
      completedWeight += (stage.progress / 100) * weight;
    });

    // If we have active stages, calculate weighted progress
    if (totalWeight > 0) {
      const weightedProgress = Math.round((completedWeight / totalWeight) * 100);
      // Ensure minimum progress for active processing
      return Math.max(weightedProgress, isProcessing ? 5 : 0);
    }

    // Fallback to simple max progress
    return Math.max(...stages.map(s => s.progress));
  };

  const currentProgress = calculateOverallProgress();
  const completedStages = stages.filter(s => s.progress === 100).length;
  const totalStages = stages.length;
  const overallProgress = totalStages > 0 ? Math.round((completedStages / totalStages) * 100) : 100;
  const lastStage = stages.length > 0 ? stages[stages.length - 1] : null;
  const currentStateText = lastStage ? formatStageTitle(lastStage.stage) : 'Complete';

  // Calculate estimated time remaining
  const calculateEstimatedTime = () => {
    if (!isProcessing || stages.length === 0) return null;

    const firstStage = stages[0];
    if (!firstStage?.details?.startTime) return null;

    const startTime = new Date(firstStage.details.startTime).getTime();
    const currentTime = Date.now();
    const elapsedTime = currentTime - startTime;

    if (currentProgress <= 5) return "Estimating...";

    const estimatedTotalTime = (elapsedTime / currentProgress) * 100;
    const remainingTime = estimatedTotalTime - elapsedTime;

    if (remainingTime <= 0) return "Almost done...";

    if (remainingTime < 5000) return "A few seconds";
    if (remainingTime < 30000) return `~${Math.round(remainingTime / 1000)}s`;
    if (remainingTime < 120000) return `~${Math.round(remainingTime / 60000)}m`;

    return "Processing...";
  };

  const estimatedTime = calculateEstimatedTime();

  // Debug logging for hidden mode (only when stages actually change)
  React.useEffect(() => {
    if (mode === 'hidden' || !isVisible) {
      console.log('🔍 QueryProcessingViewer Hidden Mode:', {
        stages: stages.length,
        completedStages,
        totalStages,
        overallProgress,
        lastStage: lastStage?.stage,
        currentStateText,
        queryId,
        onModeChange: !!onModeChange
      });
    }
  }, [mode, isVisible, stages.length, queryId]); // Reduced dependencies to prevent infinite loops

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



  const renderStageDetails = (stageData: ProcessingStage) => {
    if (!stageData.details) return null;

    return (
      <div style={{
        marginTop: '8px',
        padding: '12px',
        background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
        borderRadius: '8px',
        border: '1px solid #e2e8f0'
      }}>
        <Text strong style={{ color: '#374151', fontSize: '12px', marginBottom: '8px', display: 'block' }}>
          📋 Technical Details
        </Text>
        <div style={{ display: 'grid', gap: '6px' }}>
          {Object.entries(stageData.details).map(([key, value]) => {
            // Format specific keys for better readability
            let displayValue = value;
            let displayKey = key;

            if (key === 'promptLength') {
              displayKey = 'Prompt Length';
              displayValue = `${value} characters`;
            } else if (key === 'aiExecutionTime') {
              displayKey = 'AI Response Time';
              displayValue = `${value}ms`;
            } else if (key === 'dbExecutionTime') {
              displayKey = 'Database Execution Time';
              displayValue = `${value}ms`;
            } else if (key === 'rowCount') {
              displayKey = 'Rows Returned';
              displayValue = `${value} rows`;
            } else if (key === 'templateName') {
              displayKey = 'Prompt Template';
            } else if (typeof value === 'string' && value.length > 100) {
              displayValue = value.substring(0, 100) + '...';
            } else if (typeof value === 'object') {
              displayValue = JSON.stringify(value, null, 2);
            }

            return (
              <div key={key} style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '4px 8px',
                background: '#ffffff',
                borderRadius: '4px',
                border: '1px solid #e5e7eb'
              }}>
                <Text style={{ fontSize: '11px', color: '#6b7280', fontWeight: 500 }}>
                  {displayKey}:
                </Text>
                <Text style={{ fontSize: '11px', color: '#374151', textAlign: 'right', maxWidth: '60%' }}>
                  {displayValue}
                </Text>
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  const handleModeChange = (newMode: ProcessingViewMode) => {
    if (onModeChange) {
      onModeChange(newMode);
    }
  };

  // Hidden mode - show as closed panel
  if (mode === 'hidden' || !isVisible) {
    // Show completion status with proper data
    const completionText = !isProcessing && stages.length > 0 ? 'Completed' : currentStateText;
    const hasValidData = stages.length > 0;

    return (
      <Card
        size="small"
        style={{
          borderRadius: '8px',
          border: hasValidData ? '1px solid #e0e7ff' : '1px solid #e8f4fd',
          background: hasValidData
            ? 'linear-gradient(135deg, #f0f4ff 0%, #e0e7ff 100%)'
            : 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
          marginBottom: '16px',
          cursor: 'pointer',
          transition: 'all 0.3s ease'
        }}
        onClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
          console.log('🔍 Processing panel clicked, changing mode to processing');
          console.log('🔍 Current mode:', mode, 'onModeChange available:', !!onModeChange);
          console.log('🔍 Available stages:', stages.length, stages.map(s => s.stage));
          if (onModeChange) {
            onModeChange('processing');
            console.log('🔍 Mode change called to processing');
          } else {
            console.error('🔍 onModeChange not available!');
          }
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.transform = 'translateY(-2px)';
          e.currentTarget.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
          e.currentTarget.style.borderColor = hasValidData ? '#3b82f6' : '#3b82f6';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.transform = 'translateY(0)';
          e.currentTarget.style.boxShadow = 'none';
          e.currentTarget.style.borderColor = hasValidData ? '#e0e7ff' : '#e8f4fd';
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Space>
            <RobotOutlined style={{
              color: hasValidData ? '#3b82f6' : '#6b7280',
              fontSize: '16px'
            }} />
            <Text style={{
              color: hasValidData ? '#4c1d95' : '#6b7280',
              fontSize: '14px',
              fontWeight: 500
            }}>
              🤖 AI Processing - {completionText}
            </Text>
            {queryId && (
              <Tag color="blue" style={{ fontSize: '10px' }}>
                ID: {queryId.substring(0, 8)}...
              </Tag>
            )}
          </Space>

          <Space size="small">
            {hasValidData && totalStages > 0 && (
              <Tag color="purple" style={{ fontSize: '10px', fontWeight: 500 }}>
                {completedStages}/{totalStages} stages
              </Tag>
            )}
            <Tag color={hasValidData ? "purple" : "blue"} style={{ fontSize: '10px', fontWeight: 500 }}>
              {hasValidData ? '100% complete' : `${overallProgress}% complete`}
            </Tag>
            <EyeOutlined style={{
              color: hasValidData ? '#3b82f6' : '#6b7280',
              fontSize: '12px'
            }} />
          </Space>
        </div>

        {/* Show hint text */}
        <div style={{ marginTop: '8px' }}>
          <Text style={{
            fontSize: '11px',
            color: hasValidData ? '#5b21b6' : '#9ca3af',
            fontStyle: 'italic'
          }}>
            {hasValidData
              ? 'Click to view processing details and AI insights'
              : 'Processing information will appear here'
            }
          </Text>
        </div>
      </Card>
    );
  }

  // Minimal mode - only progress bar
  if (mode === 'minimal') {
    const minimalStateText = currentStage ? formatStageTitle(currentStage) : 'Processing';

    return (
      <Card
        size="small"
        style={{
          borderRadius: '8px',
          border: '1px solid #e0e7ff',
          background: 'linear-gradient(135deg, #f0f4ff 0%, #e0e7ff 100%)',
          marginBottom: '16px'
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Space>
            <RobotOutlined style={{ color: '#3b82f6', fontSize: '16px' }} />
            <Text strong style={{ color: '#3b82f6', fontSize: '14px' }}>
              🤖 AI Processing - {minimalStateText}
            </Text>
            {isProcessing && (
              <Tag color="processing" style={{ fontSize: '10px' }}>
                <LoadingOutlined style={{ marginRight: '4px' }} />
                PROCESSING
              </Tag>
            )}
          </Space>

          <Space size="small">
            <Button
              type="text"
              size="small"
              onClick={() => handleModeChange('processing')}
              style={{ fontSize: '10px', padding: '2px 6px', height: '20px' }}
            >
              ADVANCED
            </Button>
            <Button
              type="text"
              size="small"
              onClick={() => handleModeChange('hidden')}
              style={{ fontSize: '10px', padding: '2px 6px', height: '20px' }}
            >
              HIDE
            </Button>
          </Space>
        </div>

        <div style={{ marginTop: '8px' }} className={isProcessing ? 'progress-glow' : ''}>
          <Progress
            percent={currentProgress}
            status={isProcessing ? 'active' : 'success'}
            strokeColor={{
              '0%': '#3b82f6',
              '50%': '#1d4ed8',
              '100%': '#1e40af',
            }}
            showInfo={true}
            size="small"
            format={(percent) => (
              <span style={{
                fontSize: '11px',
                fontWeight: 600,
                color: isProcessing ? '#3b82f6' : '#1e40af'
              }} className={isProcessing ? 'processing-pulse' : ''}>
                {percent}%
              </span>
            )}
            className={`progress-smooth ${isProcessing ? 'progress-bar-animated' : ''}`}
          />
          {isProcessing && (
            <div style={{
              marginTop: '4px',
              fontSize: '10px',
              color: '#6b7280',
              fontStyle: 'italic',
              textAlign: 'center'
            }} className="processing-pulse">
              Processing... Please wait
            </div>
          )}
        </div>
      </Card>
    );
  }

  // Processing mode - current view with mode buttons
  return (
    <Card
      size="small"
      title={
        <Space>
          <RobotOutlined style={{ color: '#3b82f6' }} />
          <Text strong>🤖 AI Query Processing</Text>
          {queryId && (
            <Tag color="blue" style={{ fontSize: '10px' }}>
              ID: {queryId.substring(0, 8)}...
            </Tag>
          )}
          {isProcessing && (
            <Tag color="processing" style={{ fontSize: '10px' }}>
              <LoadingOutlined style={{ marginRight: '4px' }} />
              PROCESSING
            </Tag>
          )}
        </Space>
      }
      extra={
        <Space size="small">
          <Button
            type={mode === 'advanced' ? 'primary' : 'text'}
            size="small"
            onClick={() => handleModeChange('advanced')}
            style={{ fontSize: '10px', padding: '2px 8px', height: '24px' }}
          >
            ADVANCED
          </Button>
          <Button
            type={mode === 'processing' ? 'primary' : 'text'}
            size="small"
            onClick={() => handleModeChange('processing')}
            style={{ fontSize: '10px', padding: '2px 8px', height: '24px' }}
          >
            PROCESSING
          </Button>
          <Button
            type="text"
            size="small"
            onClick={() => handleModeChange('hidden')}
            style={{ fontSize: '10px', padding: '2px 8px', height: '24px' }}
          >
            HIDE
          </Button>
        </Space>
      }
      className="processing-card"
      style={{
        marginBottom: '16px',
        background: isProcessing ? 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)' : '#ffffff',
        border: isProcessing ? '2px solid #3b82f6' : '1px solid #d1d5db',
        borderRadius: '12px',
        boxShadow: isProcessing ? '0 4px 20px rgba(59, 130, 246, 0.15)' : '0 2px 8px rgba(0, 0, 0, 0.1)'
      }}
    >
      {/* Overall Progress */}
      <div style={{ marginBottom: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
          <Text strong style={{ fontSize: '14px', color: '#1f2937' }}>
            Processing Progress
          </Text>
          <Text style={{ fontSize: '14px', color: '#1e40af', fontWeight: 600 }}>
            {currentProgress}%
          </Text>
        </div>
        <div className={isProcessing ? 'stage-progress-bar' : ''}>
          <Progress
            percent={currentProgress}
            status={isProcessing ? 'active' : 'success'}
            strokeColor={{
              '0%': '#3b82f6',
              '30%': '#1d4ed8',
              '70%': '#1e40af',
              '100%': '#1e3a8a',
            }}
            trailColor="#e5e7eb"
            strokeWidth={12}
            format={(percent) => (
              <span style={{
                fontSize: '13px',
                fontWeight: 700,
                color: isProcessing ? '#3b82f6' : '#1e40af',
                textShadow: '0 1px 2px rgba(0,0,0,0.1)'
              }} className={isProcessing ? 'processing-text-glow' : ''}>
                {percent}%
              </span>
            )}
            className={`progress-smooth ${isProcessing ? 'progress-bar-animated' : ''}`}
            style={{
              marginBottom: '8px'
            }}
          />
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          {currentStage && (
            <Text style={{ fontSize: '12px', color: '#6b7280', fontStyle: 'italic' }}>
              Current: {formatStageTitle(currentStage)}
            </Text>
          )}
          {estimatedTime && isProcessing && (
            <Text style={{ fontSize: '12px', color: '#1e40af', fontWeight: 500 }}
                  className="estimated-time-bounce">
              ⏱️ {estimatedTime}
            </Text>
          )}
        </div>
      </div>

      {/* Processing Timeline - only show in processing and advanced modes */}
      {(mode === 'processing' || mode === 'advanced') && (
        <div style={{ marginTop: '16px' }}>
          <Text strong style={{ fontSize: '14px', color: '#1f2937', marginBottom: '12px', display: 'block' }}>
            🔄 Processing Steps
          </Text>

        {stages.length === 0 && isProcessing ? (
          <div style={{
            padding: '20px',
            textAlign: 'center',
            background: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
            border: '2px dashed #3b82f6',
            borderRadius: '8px',
            marginBottom: '16px'
          }}>
            <Space direction="vertical" size="small">
              <LoadingOutlined style={{ fontSize: '24px', color: '#3b82f6' }} />
              <Text style={{ color: '#1d4ed8', fontWeight: 500 }}>
                Initializing query processing...
              </Text>
              <Text style={{ color: '#2563eb', fontSize: '12px' }}>
                Connecting to AI service and preparing your request
              </Text>
            </Space>
          </div>
        ) : (
          <Timeline mode="left">
            {stages.filter(stageData => stageData && stageData.stage).map((stageData, index) => {
            const isCurrentStage = stageData.stage === currentStage;
            const status = isCurrentStage && isProcessing ? 'active' :
                          stageData.progress === 100 ? 'completed' : 'pending';

            return (
              <Timeline.Item
                key={`${stageData.stage || 'unknown'}-${index}`}
                dot={getStageIcon(stageData.stage || 'unknown', status)}
                color={getStageColor(stageData.stage || 'unknown', status)}
              >
                <div style={{
                  padding: '8px 12px',
                  background: status === 'active' ? 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)' : '#ffffff',
                  border: status === 'active' ? '2px solid #3b82f6' : '1px solid #e5e7eb',
                  borderRadius: '8px',
                  marginBottom: '8px'
                }}>
                  <Space direction="vertical" size="small" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Text strong style={{
                        color: status === 'active' ? '#1d4ed8' : '#374151',
                        fontSize: '13px'
                      }}>
                        {formatStageTitle(stageData.stage)}
                      </Text>
                      <Space>
                        <Tag
                          color={getStageColor(stageData.stage, status)}
                          size="small"
                          style={{ fontWeight: 600 }}
                        >
                          {stageData.progress}%
                        </Tag>
                        <Text style={{ fontSize: '10px', color: '#9ca3af' }}>
                          {stageData.timestamp ? new Date(stageData.timestamp).toLocaleTimeString() : 'N/A'}
                        </Text>
                      </Space>
                    </div>

                    <Text style={{
                      fontSize: '12px',
                      color: '#6b7280',
                      lineHeight: '1.4'
                    }}>
                      {stageData.message || 'Processing...'}
                    </Text>

                    {/* Technical details only in advanced mode */}
                    {mode === 'advanced' && stageData.details && (
                      <Collapse
                        ghost
                        size="small"
                        activeKey={expandedPanels}
                        onChange={(keys) => setExpandedPanels(keys as string[])}
                      >
                        <Panel
                          header={
                            <Text style={{ fontSize: '11px', color: '#3b82f6', fontWeight: 500 }}>
                              📊 View Technical Details
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
        )}
        </div>
      )}

      {isProcessing && (
        <div style={{
          textAlign: 'center',
          padding: '20px',
          background: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
          border: '2px solid #3b82f6',
          borderRadius: '12px',
          marginTop: '20px',
          position: 'relative',
          overflow: 'hidden'
        }}>
          {/* Animated background */}
          <div style={{
            position: 'absolute',
            top: 0,
            left: '-100%',
            width: '100%',
            height: '100%',
            background: 'linear-gradient(90deg, transparent, rgba(59, 130, 246, 0.1), transparent)',
            animation: 'shimmer 2s infinite'
          }} />

          <Space direction="vertical" size="small">
            <Space>
              <LoadingOutlined style={{ color: '#3b82f6', fontSize: '18px' }} />
              <Text style={{ color: '#3b82f6', fontWeight: 600, fontSize: '16px' }}>
                🤖 AI is processing your query...
              </Text>
            </Space>
            <Text style={{ color: '#1d4ed8', fontSize: '12px' }}>
              This may take a few moments depending on query complexity
            </Text>
          </Space>
        </div>
      )}

      {/* Add shimmer animation styles */}
      <style>{`
        @keyframes shimmer {
          0% { left: -100%; }
          100% { left: 100%; }
        }
      `}</style>
    </Card>
  );
};

export default QueryProcessingViewer;
