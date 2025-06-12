import React, { useState } from 'react';
import { Card, Timeline, Progress, Typography, Space, Tag, Button } from 'antd';
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
import '../styles/query-interface.css';

const { Text } = Typography;

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
        timestamp: s?.timestamp || 'undefined',
        details: s?.details ? Object.keys(s.details) : 'no details',
        fullDetails: s?.details // Log full details to see structure
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

  // Helper function to format timing
  const formatDuration = (ms: number) => {
    if (ms < 1000) return `${Math.round(ms)}ms`;
    if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
    return `${(ms / 60000).toFixed(1)}m`;
  };

  // Helper function to get step timing
  const getStepTiming = (stageData: ProcessingStage, index: number) => {
    const stepTiming = timingInfo.stepTimes.find(t => t.stage === stageData.stage);
    return stepTiming || null;
  };

  // Calculate values used in different modes with improved progress calculation
  const calculateOverallProgress = () => {
    if (stages.length === 0) {
      return isProcessing ? 0 : 0; // Start at 0 when processing begins
    }

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
      const stageProgress = stage.progress !== undefined && stage.progress !== null ? stage.progress : 0;
      completedWeight += (stageProgress / 100) * weight;
    });

    // If we have active stages, calculate weighted progress
    if (totalWeight > 0) {
      const weightedProgress = Math.round((completedWeight / totalWeight) * 100);
      // Ensure progress doesn't exceed 100% and starts properly
      return Math.min(Math.max(weightedProgress, 0), 100);
    }

    // Fallback to simple average progress
    const validStages = stages.filter(s => s.progress !== undefined && s.progress !== null);
    if (validStages.length > 0) {
      return Math.round(validStages.reduce((sum, s) => sum + s.progress, 0) / validStages.length);
    }

    return 0;
  };

  const currentProgress = calculateOverallProgress();
  const completedStages = stages.filter(s => s.progress === 100).length;
  const totalStages = stages.length;
  const overallProgress = currentProgress; // Use the weighted progress calculation instead
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

  // Calculate timing information
  const calculateTimingInfo = () => {
    if (stages.length === 0) return { totalTime: 0, stepTimes: [] };

    const sortedStages = [...stages].sort((a, b) => {
      const timeA = new Date(a.timestamp).getTime();
      const timeB = new Date(b.timestamp).getTime();
      return timeA - timeB;
    });

    const firstStage = sortedStages[0];
    const lastStage = sortedStages[sortedStages.length - 1];

    const startTime = new Date(firstStage.timestamp).getTime();
    const endTime = isProcessing ? Date.now() : new Date(lastStage.timestamp).getTime();
    const totalTime = endTime - startTime;

    const stepTimes = sortedStages.map((stage, index) => {
      const stageTime = new Date(stage.timestamp).getTime();
      const prevTime = index > 0 ? new Date(sortedStages[index - 1].timestamp).getTime() : startTime;
      const stepDuration = stageTime - prevTime;

      return {
        stage: stage.stage,
        duration: stepDuration,
        timestamp: stageTime,
        cumulativeTime: stageTime - startTime
      };
    });

    return { totalTime, stepTimes };
  };

  const timingInfo = calculateTimingInfo();

  // Debug logging for hidden mode (only when stages actually change)
  React.useEffect(() => {
    if ((mode === 'hidden' || !isVisible) && process.env.NODE_ENV === 'development') {
      console.log('🔍 QueryProcessingViewer Hidden Mode:', {
        stages: stages.length,
        completedStages,
        totalStages,
        overallProgress,
        lastStage: lastStage?.stage,
        currentStateText,
        queryId,
        onModeChange: !!onModeChange,
        timingInfo
      });
    }
  }, [mode, isVisible, stages.length, queryId]); // Reduced dependencies to prevent infinite loops

  const getStageIcon = (stage: string, status: string = 'pending') => {
    const baseStyle = { fontSize: '16px' };

    if (status === 'error') {
      return <ExclamationCircleOutlined style={{ ...baseStyle, color: '#ff4d4f' }} />;
    }

    if (status === 'completed') {
      return <CheckCircleOutlined style={{ ...baseStyle, color: '#52c41a' }} />;
    }

    if (status === 'active') {
      return <LoadingOutlined style={{ ...baseStyle, color: '#1890ff' }} />;
    }

    // Stage-specific icons
    switch (stage) {
      case 'started':
      case 'cache_check':
        return <ClockCircleOutlined style={{ ...baseStyle, color: '#8c8c8c' }} />;
      case 'schema_loading':
      case 'schema_analysis':
        return <DatabaseOutlined style={{ ...baseStyle, color: '#722ed1' }} />;
      case 'prompt_building':
      case 'prompt_details':
      case 'ai_processing':
      case 'ai_completed':
        return <RobotOutlined style={{ ...baseStyle, color: '#1890ff' }} />;
      case 'sql_validation':
      case 'sql_execution':
      case 'sql_completed':
        return <CodeOutlined style={{ ...baseStyle, color: '#52c41a' }} />;
      case 'confidence_calculation':
      case 'visualization_generation':
      case 'suggestions_generation':
        return <BarChartOutlined style={{ ...baseStyle, color: '#fa8c16' }} />;
      default:
        return <ClockCircleOutlined style={{ ...baseStyle, color: '#8c8c8c' }} />;
    }
  };





  const renderStageDetails = (stageData: ProcessingStage) => {
    if (!stageData.details) return null;

    // Special handling for prompt details - this is the most important data to show
    if (stageData.details.promptDetails || stageData.details.PromptDetails) {
      const promptDetails = stageData.details.promptDetails || stageData.details.PromptDetails;
      return (
        <div style={{
          marginTop: '8px',
          padding: '12px',
          background: 'linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%)',
          borderRadius: '8px',
          border: '1px solid #0ea5e9'
        }}>
          <Text strong style={{ color: '#0c4a6e', fontSize: '12px', marginBottom: '8px', display: 'block' }}>
            🤖 AI Prompt Details
          </Text>
          <div style={{ display: 'grid', gap: '8px' }}>
            <div style={{
              padding: '8px',
              background: '#ffffff',
              borderRadius: '6px',
              border: '1px solid #e0f2fe'
            }}>
              <Text strong style={{ fontSize: '11px', color: '#0c4a6e', display: 'block', marginBottom: '4px' }}>
                📝 Full Prompt Sent to AI:
              </Text>
              <div style={{
                fontFamily: 'monospace',
                fontSize: '11px',
                whiteSpace: 'pre-wrap',
                maxHeight: '200px',
                overflow: 'auto',
                padding: '8px',
                backgroundColor: '#f8fafc',
                borderRadius: '4px',
                border: '1px solid #e2e8f0',
                color: '#374151'
              }}>
                {promptDetails.fullPrompt || promptDetails.FullPrompt || 'No prompt available'}
              </div>
            </div>
            {(promptDetails.templateName || promptDetails.TemplateName) && (
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '6px 10px',
                background: '#ffffff',
                borderRadius: '6px',
                border: '1px solid #e0f2fe'
              }}>
                <Text style={{ fontSize: '11px', color: '#0c4a6e', fontWeight: 500 }}>
                  📋 Template:
                </Text>
                <Text style={{ fontSize: '11px', color: '#374151' }}>
                  {promptDetails.templateName || promptDetails.TemplateName}
                </Text>
              </div>
            )}
            {(promptDetails.tokenCount || promptDetails.TokenCount) && (
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '6px 10px',
                background: '#ffffff',
                borderRadius: '6px',
                border: '1px solid #e0f2fe'
              }}>
                <Text style={{ fontSize: '11px', color: '#0c4a6e', fontWeight: 500 }}>
                  🔢 Token Count:
                </Text>
                <Text style={{ fontSize: '11px', color: '#374151' }}>
                  {promptDetails.tokenCount || promptDetails.TokenCount}
                </Text>
              </div>
            )}
          </div>
        </div>
      );
    }

    // Enhanced cache details handling
    const isCacheStage = stageData.stage?.includes('cache') || stageData.stage === 'cache_check' || stageData.stage === 'cache_lookup';

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
            // Skip prompt details as they're handled above
            if (key === 'promptDetails' || key === 'PromptDetails') {
              return null;
            }

            // Format specific keys for better readability
            let displayValue = value;
            let displayKey = key;

            // Enhanced cache-specific formatting
            if (isCacheStage) {
              if (key === 'cacheKey') {
                displayKey = '🔑 Cache Key';
                displayValue = typeof value === 'string' ? value : JSON.stringify(value);
              } else if (key === 'cacheHit') {
                displayKey = '🎯 Cache Result';
                displayValue = value ? '✅ HIT - Data found in cache' : '❌ MISS - Data not found in cache';
              } else if (key === 'cacheValue') {
                displayKey = '💾 Cached Data';
                if (typeof value === 'object') {
                  displayValue = `${JSON.stringify(value).length} characters of cached data`;
                } else {
                  displayValue = value;
                }
              } else if (key === 'ttl') {
                displayKey = '⏰ Cache TTL';
                displayValue = `${value} seconds`;
              } else if (key === 'cacheSize') {
                displayKey = '📊 Cache Size';
                displayValue = `${value} bytes`;
              } else if (key === 'searchQuery') {
                displayKey = '🔍 Search Query';
                displayValue = value;
              } else if (key === 'queryHash') {
                displayKey = '🔐 Query Hash';
                displayValue = value;
              }
            }

            // General formatting
            if (key === 'promptLength') {
              displayKey = '📝 Prompt Length';
              displayValue = `${value} characters`;
            } else if (key === 'aiExecutionTime') {
              displayKey = '🤖 AI Response Time';
              displayValue = `${value}ms`;
            } else if (key === 'dbExecutionTime') {
              displayKey = '🗄️ Database Execution Time';
              displayValue = `${value}ms`;
            } else if (key === 'rowCount') {
              displayKey = '📊 Rows Returned';
              displayValue = `${value} rows`;
            } else if (key === 'templateName') {
              displayKey = '📋 Prompt Template';
            } else if (key === 'confidence') {
              displayKey = '🎯 Confidence Score';
              displayValue = `${((value as number) * 100).toFixed(1)}%`;
            } else if (typeof value === 'object' && !isCacheStage) {
              displayValue = JSON.stringify(value, null, 2);
            } else if (typeof value === 'string') {
              // Don't truncate strings - show full content
              displayValue = value;
            }

            return (
              <div key={key} style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '6px 10px',
                background: '#ffffff',
                borderRadius: '6px',
                border: '1px solid #e5e7eb',
                boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)'
              }}>
                <Text style={{ fontSize: '11px', color: '#6b7280', fontWeight: 500 }}>
                  {displayKey}:
                </Text>
                <div style={{
                  fontSize: '11px',
                  color: '#374151',
                  textAlign: 'right',
                  maxWidth: '60%',
                  fontFamily: key.includes('cache') || key.includes('query') || key.includes('sql') ? 'monospace' : 'inherit',
                  maxHeight: '100px',
                  overflowY: 'auto',
                  whiteSpace: 'pre-wrap'
                }}>
                  {displayValue as React.ReactNode}
                </div>
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

  // Hidden mode - show as closed panel (minimal mode)
  if (mode === 'hidden' || !isVisible) {
    // Show completion status with proper data
    const completionText = !isProcessing && stages.length > 0 ? 'Completed' : currentStateText;
    const hasValidData = stages.length > 0;

    return (
      <Card
        size="small"
        style={{
          borderRadius: '12px',
          border: '1px solid #e5e7eb',
          background: 'white',
          marginBottom: '16px',
          cursor: 'pointer',
          transition: 'all 0.3s ease',
          boxShadow: '0 1px 3px rgba(0, 0, 0, 0.1)'
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
          e.currentTarget.style.transform = 'translateY(-1px)';
          e.currentTarget.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.15)';
          e.currentTarget.style.borderColor = '#d1d5db';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.transform = 'translateY(0)';
          e.currentTarget.style.boxShadow = '0 1px 3px rgba(0, 0, 0, 0.1)';
          e.currentTarget.style.borderColor = '#e5e7eb';
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Space>
            <RobotOutlined style={{
              color: '#6b7280',
              fontSize: '16px'
            }} />
            <Text style={{
              color: '#374151',
              fontSize: '14px',
              fontWeight: 500
            }}>
              AI Processing - {completionText}
            </Text>
            {queryId && (
              <Tag color="default" style={{ fontSize: '10px', color: '#6b7280', borderColor: '#d1d5db' }}>
                ID: {queryId.substring(0, 8)}...
              </Tag>
            )}
          </Space>

          <Space size="small">
            {totalStages > 0 && (
              <Tag color="default" style={{ fontSize: '10px', fontWeight: 500, color: '#6b7280', borderColor: '#d1d5db' }}>
                {completedStages}/{totalStages} stages
              </Tag>
            )}
            {timingInfo.totalTime > 0 && (
              <Tag color="blue" style={{ fontSize: '10px', fontWeight: 500 }}>
                {formatDuration(timingInfo.totalTime)}
              </Tag>
            )}
            <Tag color="default" style={{ fontSize: '10px', fontWeight: 500, color: '#6b7280', borderColor: '#d1d5db' }}>
              {isProcessing ? `${currentProgress}% complete` : (hasValidData ? '100% complete' : '0% complete')}
            </Tag>
            <EyeOutlined style={{
              color: '#9ca3af',
              fontSize: '12px'
            }} />
          </Space>
        </div>

        {/* Progress bar in minimal mode */}
        <div style={{ marginTop: '12px' }}>
          <Progress
            percent={isProcessing ? currentProgress : (hasValidData ? 100 : 0)}
            status={isProcessing ? 'active' : 'success'}
            strokeColor={{
              '0%': '#3b82f6',
              '50%': '#2563eb',
              '100%': '#1d4ed8',
            }}
            trailColor="#f3f4f6"
            strokeWidth={6}
            showInfo={true}
            format={(percent) => (
              <span style={{
                fontSize: '12px',
                fontWeight: 600,
                color: '#3b82f6'
              }}>
                {percent}%
              </span>
            )}
            style={{ marginBottom: '8px' }}
          />
          {hasValidData && stages.length > 0 && !isProcessing && (
            <Text style={{
              fontSize: '11px',
              color: '#9ca3af',
              fontStyle: 'italic'
            }}>
              Click to view processing details and AI insights
            </Text>
          )}
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
        <div
          style={{ cursor: 'pointer', padding: '4px 0' }}
          onClick={() => handleModeChange('hidden')}
        >
          <Space>
            <RobotOutlined style={{ color: '#6b7280' }} />
            <Text strong style={{ color: '#374151' }}>AI Query Processing</Text>
            {queryId && (
              <Tag color="default" style={{ fontSize: '10px', color: '#6b7280', borderColor: '#d1d5db' }}>
                ID: {queryId.substring(0, 8)}...
              </Tag>
            )}
            {isProcessing && (
              <Tag color="default" style={{ fontSize: '10px', color: '#6b7280', borderColor: '#d1d5db' }}>
                <LoadingOutlined style={{ marginRight: '4px' }} />
                PROCESSING
              </Tag>
            )}
          </Space>
        </div>
      }
      extra={
        <Space size="small">
          <Button
            type={mode === 'advanced' ? 'default' : 'text'}
            size="small"
            onClick={() => handleModeChange('advanced')}
            style={{
              fontSize: '10px',
              padding: '2px 8px',
              height: '24px',
              color: mode === 'advanced' ? '#374151' : '#9ca3af',
              borderColor: mode === 'advanced' ? '#d1d5db' : 'transparent'
            }}
          >
            ADVANCED
          </Button>
          <Button
            type={mode === 'processing' ? 'default' : 'text'}
            size="small"
            onClick={() => handleModeChange('processing')}
            style={{
              fontSize: '10px',
              padding: '2px 8px',
              height: '24px',
              color: mode === 'processing' ? '#374151' : '#9ca3af',
              borderColor: mode === 'processing' ? '#d1d5db' : 'transparent'
            }}
          >
            PROCESSING
          </Button>
          <Button
            type="text"
            size="small"
            onClick={() => handleModeChange('hidden')}
            style={{
              fontSize: '10px',
              padding: '2px 8px',
              height: '24px',
              color: '#9ca3af'
            }}
          >
            MINIMIZE
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
          <Text strong style={{ fontSize: '14px', color: '#374151' }}>
            Processing Progress
          </Text>
          <Space size="small">
            {timingInfo.totalTime > 0 && (
              <Tag color="blue" style={{ fontSize: '11px', fontWeight: 500 }}>
                ⏱️ Total: {formatDuration(timingInfo.totalTime)}
              </Tag>
            )}
            <Text style={{ fontSize: '14px', color: '#6b7280', fontWeight: 600 }}>
              {currentProgress}%
            </Text>
          </Space>
        </div>
        <div className={isProcessing ? 'stage-progress-bar' : ''}>
          <Progress
            percent={currentProgress}
            status={isProcessing ? 'active' : 'success'}
            strokeColor={{
              '0%': '#3b82f6',
              '50%': '#2563eb',
              '100%': '#1d4ed8',
            }}
            trailColor="#f3f4f6"
            strokeWidth={12}
            format={(percent) => (
              <span style={{
                fontSize: '13px',
                fontWeight: 600,
                color: '#3b82f6'
              }}>
                {percent}%
              </span>
            )}
            className="progress-smooth"
            style={{
              marginBottom: '8px'
            }}
          />
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          {currentStage && (
            <Text style={{ fontSize: '12px', color: '#9ca3af', fontStyle: 'italic' }}>
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
          <Text strong style={{ fontSize: '14px', color: '#374151', marginBottom: '12px', display: 'block' }}>
            Processing Steps
          </Text>

        {stages.length === 0 && isProcessing ? (
          <div style={{
            padding: '20px',
            textAlign: 'center',
            background: '#f9fafb',
            border: '2px dashed #d1d5db',
            borderRadius: '8px',
            marginBottom: '16px'
          }}>
            <Space direction="vertical" size="small">
              <LoadingOutlined style={{ fontSize: '24px', color: '#6b7280' }} />
              <Text style={{ color: '#374151', fontWeight: 500 }}>
                Initializing query processing...
              </Text>
              <Text style={{ color: '#9ca3af', fontSize: '12px' }}>
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
                color={status === 'active' ? '#6b7280' : status === 'completed' ? '#374151' : '#d1d5db'}
              >
                <div style={{
                  padding: '12px 16px',
                  background: status === 'active' ? '#f9fafb' : '#ffffff',
                  border: status === 'active' ? '2px solid #d1d5db' : '1px solid #e5e7eb',
                  borderRadius: '8px',
                  marginBottom: '8px',
                  cursor: 'pointer',
                  transition: 'all 0.2s ease'
                }}
                onClick={() => {
                  const panelKey = `details-${index}`;
                  setExpandedPanels(prev =>
                    prev.includes(panelKey)
                      ? prev.filter(key => key !== panelKey)
                      : [...prev, panelKey]
                  );
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.borderColor = '#9ca3af';
                  e.currentTarget.style.boxShadow = '0 2px 8px rgba(0, 0, 0, 0.1)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.borderColor = status === 'active' ? '#d1d5db' : '#e5e7eb';
                  e.currentTarget.style.boxShadow = 'none';
                }}
                >
                  <Space direction="vertical" size="small" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Text strong style={{
                        color: status === 'active' ? '#374151' : '#6b7280',
                        fontSize: '13px'
                      }}>
                        {formatStageTitle(stageData.stage)}
                      </Text>
                      <Space size="small">
                        {(() => {
                          const stepTiming = getStepTiming(stageData, index);
                          return stepTiming && stepTiming.duration > 0 ? (
                            <Tag color="cyan" style={{ fontSize: '9px', fontWeight: 500 }}>
                              {formatDuration(stepTiming.duration)}
                            </Tag>
                          ) : null;
                        })()}
                        <Tag
                          color="default"
                          style={{
                            fontSize: '10px',
                            fontWeight: 600,
                            color: '#6b7280',
                            borderColor: '#d1d5db'
                          }}
                        >
                          {stageData.progress !== undefined && stageData.progress !== null ? `${stageData.progress}%` : '0%'}
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

                    {/* Expandable details for each step */}
                    {expandedPanels.includes(`details-${index}`) && (
                      <div style={{
                        marginTop: '12px',
                        padding: '12px',
                        background: '#f3f4f6',
                        borderRadius: '6px',
                        border: '1px solid #e5e7eb'
                      }}>
                        <Space direction="vertical" size="small" style={{ width: '100%' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text strong style={{ fontSize: '12px', color: '#374151' }}>
                              Step Details
                            </Text>
                            {(() => {
                              const stepTiming = getStepTiming(stageData, index);
                              return stepTiming ? (
                                <Space size="small">
                                  <Tag color="blue" style={{ fontSize: '10px' }}>
                                    Duration: {formatDuration(stepTiming.duration)}
                                  </Tag>
                                  <Tag color="purple" style={{ fontSize: '10px' }}>
                                    Cumulative: {formatDuration(stepTiming.cumulativeTime)}
                                  </Tag>
                                </Space>
                              ) : null;
                            })()}
                          </div>

                          {/* Stage-specific details */}
                          <div style={{
                            background: '#f8fafc',
                            padding: '8px',
                            borderRadius: '6px',
                            border: '1px solid #e2e8f0'
                          }}>
                            <Text strong style={{ fontSize: '11px', color: '#374151', display: 'block', marginBottom: '6px' }}>
                              📊 Stage Information
                            </Text>
                            <div style={{ display: 'grid', gap: '4px' }}>
                              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                <Text style={{ fontSize: '10px', color: '#6b7280' }}>Stage:</Text>
                                <Text style={{ fontSize: '10px', color: '#374151', fontFamily: 'monospace' }}>{stageData.stage}</Text>
                              </div>
                              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                <Text style={{ fontSize: '10px', color: '#6b7280' }}>Status:</Text>
                                <Text style={{ fontSize: '10px', color: '#374151' }}>{status.toUpperCase()}</Text>
                              </div>
                              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                <Text style={{ fontSize: '10px', color: '#6b7280' }}>Progress:</Text>
                                <Text style={{ fontSize: '10px', color: '#374151' }}>{stageData.progress || 0}%</Text>
                              </div>
                              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                <Text style={{ fontSize: '10px', color: '#6b7280' }}>Timestamp:</Text>
                                <Text style={{ fontSize: '10px', color: '#374151', fontFamily: 'monospace' }}>
                                  {stageData.timestamp ? new Date(stageData.timestamp).toISOString() : 'N/A'}
                                </Text>
                              </div>
                            </div>
                          </div>

                          {/* Prompt - check multiple possible field names */}
                          {(stageData.details?.prompt || stageData.details?.promptText || stageData.details?.query || stageData.details?.userQuery) && (
                            <div>
                              <Text strong style={{ fontSize: '11px', color: '#6b7280' }}>💬 Prompt:</Text>
                              <div style={{
                                background: 'white',
                                padding: '8px',
                                borderRadius: '4px',
                                border: '1px solid #e5e7eb',
                                marginTop: '4px',
                                fontSize: '11px',
                                color: '#374151',
                                fontFamily: 'monospace',
                                maxHeight: '400px',
                                overflowY: 'auto',
                                whiteSpace: 'pre-wrap'
                              }}>
                                {stageData.details.prompt ||
                                 stageData.details.promptText ||
                                 stageData.details.query ||
                                 stageData.details.userQuery}
                              </div>
                            </div>
                          )}

                          {/* Response - check multiple possible field names */}
                          {(stageData.details?.response || stageData.details?.aiResponse || stageData.details?.result || stageData.details?.output) && (
                            <div>
                              <Text strong style={{ fontSize: '11px', color: '#6b7280' }}>🤖 AI Response:</Text>
                              <div style={{
                                background: 'white',
                                padding: '8px',
                                borderRadius: '4px',
                                border: '1px solid #e5e7eb',
                                marginTop: '4px',
                                fontSize: '11px',
                                color: '#374151',
                                fontFamily: 'monospace',
                                maxHeight: '500px',
                                overflowY: 'auto',
                                whiteSpace: 'pre-wrap'
                              }}>
                                {(() => {
                                  const responseData = stageData.details.response ||
                                                     stageData.details.aiResponse ||
                                                     stageData.details.result ||
                                                     stageData.details.output;

                                  return typeof responseData === 'string'
                                    ? responseData
                                    : JSON.stringify(responseData, null, 2);
                                })()}
                              </div>
                            </div>
                          )}

                          {/* SQL Query - check multiple possible field names */}
                          {(stageData.details?.sql || stageData.details?.sqlQuery || stageData.details?.generatedSql || stageData.details?.query) && (
                            <div>
                              <Text strong style={{ fontSize: '11px', color: '#6b7280' }}>🗄️ Generated SQL:</Text>
                              <div style={{
                                background: 'white',
                                padding: '8px',
                                borderRadius: '4px',
                                border: '1px solid #e5e7eb',
                                marginTop: '4px',
                                fontSize: '11px',
                                color: '#374151',
                                fontFamily: 'monospace',
                                maxHeight: '500px',
                                overflowY: 'auto',
                                whiteSpace: 'pre-wrap'
                              }}>
                                {stageData.details.sql ||
                                 stageData.details.sqlQuery ||
                                 stageData.details.generatedSql ||
                                 stageData.details.query}
                              </div>
                            </div>
                          )}

                          {/* Results Summary */}
                          {stageData.details?.results && (
                            <div>
                              <Text strong style={{ fontSize: '11px', color: '#6b7280' }}>📊 Results Summary:</Text>
                              <div style={{
                                background: 'white',
                                padding: '8px',
                                borderRadius: '4px',
                                border: '1px solid #e5e7eb',
                                marginTop: '4px',
                                fontSize: '11px',
                                color: '#374151'
                              }}>
                                {typeof stageData.details.results === 'object' ? (
                                  <div style={{ display: 'grid', gap: '4px' }}>
                                    {Object.entries(stageData.details.results).map(([key, value]) => (
                                      <div key={key} style={{ display: 'flex', justifyContent: 'space-between' }}>
                                        <Text style={{ fontSize: '10px', color: '#6b7280' }}>{key}:</Text>
                                        <Text style={{ fontSize: '10px', color: '#374151' }}>
                                          {typeof value === 'object' ? JSON.stringify(value) : String(value)}
                                        </Text>
                                      </div>
                                    ))}
                                  </div>
                                ) : (
                                  <Text style={{ fontFamily: 'monospace' }}>{String(stageData.details.results)}</Text>
                                )}
                              </div>
                            </div>
                          )}

                          {/* Error Information */}
                          {stageData.details?.error && (
                            <div>
                              <Text strong style={{ fontSize: '11px', color: '#dc2626' }}>❌ Error Details:</Text>
                              <div style={{
                                background: '#fef2f2',
                                padding: '8px',
                                borderRadius: '4px',
                                border: '1px solid #fecaca',
                                marginTop: '4px',
                                fontSize: '11px',
                                color: '#dc2626',
                                fontFamily: 'monospace'
                              }}>
                                {typeof stageData.details.error === 'string'
                                  ? stageData.details.error
                                  : JSON.stringify(stageData.details.error, null, 2)}
                              </div>
                            </div>
                          )}

                          {/* All Available Details - show everything in the details object */}
                          {stageData.details && Object.keys(stageData.details).length > 0 && (
                            <div>
                              <Text strong style={{ fontSize: '11px', color: '#6b7280' }}>🔍 All Available Details:</Text>
                              <div style={{
                                background: '#f8fafc',
                                padding: '8px',
                                borderRadius: '4px',
                                border: '1px solid #e2e8f0',
                                marginTop: '4px'
                              }}>
                                {Object.entries(stageData.details).map(([key, value]) => {
                                  // Skip if we already showed this field above
                                  const alreadyShown = ['prompt', 'promptText', 'query', 'userQuery',
                                                       'response', 'aiResponse', 'result', 'output',
                                                       'sql', 'sqlQuery', 'generatedSql',
                                                       'results', 'error'].includes(key);

                                  if (alreadyShown) return null;

                                  let displayValue = value;
                                  if (typeof value === 'object' && value !== null) {
                                    displayValue = JSON.stringify(value, null, 2);
                                  } else if (typeof value === 'string' && value.length > 500) {
                                    // For very long strings, show first 500 chars with expand option
                                    displayValue = value.substring(0, 500) + '... (truncated)';
                                  }

                                  return (
                                    <div key={key} style={{
                                      marginBottom: '8px',
                                      padding: '6px',
                                      background: 'white',
                                      borderRadius: '4px',
                                      border: '1px solid #e5e7eb'
                                    }}>
                                      <Text strong style={{ fontSize: '10px', color: '#374151', display: 'block', marginBottom: '4px' }}>
                                        {key}:
                                      </Text>
                                      <div style={{
                                        fontSize: '10px',
                                        color: '#6b7280',
                                        fontFamily: typeof value === 'object' || key.includes('sql') || key.includes('query') ? 'monospace' : 'inherit',
                                        whiteSpace: 'pre-wrap',
                                        maxHeight: '200px',
                                        overflowY: 'auto'
                                      }}>
                                        {String(displayValue)}
                                      </div>
                                    </div>
                                  );
                                })}
                              </div>
                            </div>
                          )}

                          {/* Other technical details */}
                          {renderStageDetails(stageData)}
                        </Space>
                      </div>
                    )}

                    {/* Click hint - show if there are ANY details available */}
                    {stageData.details && Object.keys(stageData.details).length > 0 ? (
                      <Text style={{
                        fontSize: '10px',
                        color: '#9ca3af',
                        fontStyle: 'italic',
                        marginTop: '4px'
                      }}>
                        Click to {expandedPanels.includes(`details-${index}`) ? 'hide' : 'view'} details ({Object.keys(stageData.details).length} fields available)
                      </Text>
                    ) : (
                      <Text style={{
                        fontSize: '10px',
                        color: '#d1d5db',
                        fontStyle: 'italic',
                        marginTop: '4px'
                      }}>
                        No additional details available
                      </Text>
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
          background: '#f9fafb',
          border: '2px solid #d1d5db',
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
            background: 'linear-gradient(90deg, transparent, rgba(107, 114, 128, 0.1), transparent)',
            animation: 'shimmer 2s infinite'
          }} />

          <Space direction="vertical" size="small">
            <Space>
              <LoadingOutlined style={{ color: '#6b7280', fontSize: '18px' }} />
              <Text style={{ color: '#374151', fontWeight: 600, fontSize: '16px' }}>
                AI is processing your query...
              </Text>
            </Space>
            <Text style={{ color: '#9ca3af', fontSize: '12px' }}>
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
