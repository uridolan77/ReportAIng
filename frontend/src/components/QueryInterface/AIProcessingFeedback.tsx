import React, { useState, useEffect } from 'react';
import {
  Card,
  Typography,
  Space,
  Progress,
  Tag,
  Collapse,
  Timeline,
  Alert
} from 'antd';
import {
  RobotOutlined,
  LoadingOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BulbOutlined,
  CodeOutlined
} from '@ant-design/icons';

const { Text, Title } = Typography;
const { Panel } = Collapse;

interface AIProcessingStep {
  id: string;
  title: string;
  description: string;
  status: 'pending' | 'processing' | 'completed' | 'failed' | 'retrying';
  timestamp: Date;
  details?: string;
  sql?: string;
  error?: string;
}

interface AIProcessingFeedbackProps {
  isProcessing: boolean;
  currentStep?: string;
  steps: AIProcessingStep[];
  onDismiss?: () => void;
  showDetails?: boolean;
}

export const AIProcessingFeedback: React.FC<AIProcessingFeedbackProps> = ({
  isProcessing,
  currentStep,
  steps,
  onDismiss,
  showDetails = false
}) => {
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    if (steps.length > 0) {
      const completedSteps = steps.filter(step => step.status === 'completed').length;
      const newProgress = (completedSteps / steps.length) * 100;
      setProgress(newProgress);
    }
  }, [steps]);

  const getStepIcon = (status: string) => {
    switch (status) {
      case 'processing':
        return <LoadingOutlined style={{ color: '#1890ff' }} />;
      case 'completed':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'failed':
        return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'retrying':
        return <LoadingOutlined style={{ color: '#faad14' }} />;
      default:
        return <RobotOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getStepColor = (status: string) => {
    switch (status) {
      case 'processing': return 'processing';
      case 'completed': return 'success';
      case 'failed': return 'error';
      case 'retrying': return 'warning';
      default: return 'default';
    }
  };

  if (!isProcessing && steps.length === 0) {
    return null;
  }

  return (
    <Card
      style={{
        borderRadius: '12px',
        border: '2px solid #1890ff',
        background: 'linear-gradient(135deg, #f0f8ff 0%, #e6f7ff 100%)',
        marginBottom: '16px'
      }}
    >
      <div style={{ marginBottom: '16px' }}>
        <Space>
          <RobotOutlined style={{ color: '#1890ff', fontSize: '20px' }} />
          <Title level={5} style={{ margin: 0, color: '#1890ff' }}>
            🤖 AI is Working on Your Query
          </Title>
        </Space>
        
        {isProcessing && (
          <Text type="secondary" style={{ display: 'block', marginTop: '4px' }}>
            The AI is analyzing your request and generating the best SQL query...
          </Text>
        )}
      </div>

      {/* Progress Bar */}
      <div style={{ marginBottom: '16px' }}>
        <Progress
          percent={progress}
          status={isProcessing ? 'active' : 'success'}
          strokeColor={{
            '0%': '#1890ff',
            '100%': '#52c41a',
          }}
          showInfo={false}
        />
        
        {currentStep && (
          <Text style={{ fontSize: '12px', color: '#666' }}>
            Current: {currentStep}
          </Text>
        )}
      </div>

      {/* Quick Status */}
      <div style={{ marginBottom: '16px' }}>
        <Space wrap>
          {steps.map((step) => (
            <Tag
              key={step.id}
              color={getStepColor(step.status)}
              icon={getStepIcon(step.status)}
            >
              {step.title}
            </Tag>
          ))}
        </Space>
      </div>

      {/* Detailed Timeline (Collapsible) */}
      {steps.length > 0 && (
        <Collapse ghost>
          <Panel 
            header={
              <Space>
                <CodeOutlined />
                <Text>View Processing Details</Text>
              </Space>
            } 
            key="details"
          >
            <Timeline>
              {steps.map((step) => (
                <Timeline.Item
                  key={step.id}
                  dot={getStepIcon(step.status)}
                  color={step.status === 'completed' ? 'green' : 
                         step.status === 'failed' ? 'red' : 
                         step.status === 'processing' ? 'blue' : 'gray'}
                >
                  <div>
                    <Text strong>{step.title}</Text>
                    <br />
                    <Text type="secondary">{step.description}</Text>
                    
                    {step.details && (
                      <div style={{ marginTop: '8px' }}>
                        <Text code style={{ fontSize: '11px', background: '#f5f5f5' }}>
                          {step.details}
                        </Text>
                      </div>
                    )}
                    
                    {step.sql && (
                      <div style={{ marginTop: '8px' }}>
                        <Text strong>Generated SQL:</Text>
                        <pre style={{ 
                          background: '#f5f5f5', 
                          padding: '8px', 
                          borderRadius: '4px',
                          fontSize: '11px',
                          marginTop: '4px'
                        }}>
                          {step.sql}
                        </pre>
                      </div>
                    )}
                    
                    {step.error && (
                      <Alert
                        message="Step Error"
                        description={step.error}
                        type="error"

                        style={{ marginTop: '8px' }}
                      />
                    )}
                    
                    <Text type="secondary" style={{ fontSize: '11px' }}>
                      {step.timestamp.toLocaleTimeString()}
                    </Text>
                  </div>
                </Timeline.Item>
              ))}
            </Timeline>
          </Panel>
        </Collapse>
      )}

      {/* Helpful Tips */}
      <div style={{ marginTop: '16px', padding: '12px', background: 'rgba(24, 144, 255, 0.1)', borderRadius: '8px' }}>
        <Space>
          <BulbOutlined style={{ color: '#1890ff' }} />
          <Text style={{ fontSize: '12px', color: '#1890ff' }}>
            <strong>Tip:</strong> The AI may try multiple approaches to get the best result. This is normal and ensures accuracy.
          </Text>
        </Space>
      </div>
    </Card>
  );
};

// Helper function to create processing steps from error messages
export const createProcessingStepFromError = (error: any): AIProcessingStep => {
  return {
    id: `step-${Date.now()}`,
    title: error.message.includes('SELECT') ? 'SQL Structure Validation' : 
           error.message.includes('FROM') ? 'Table Reference Check' : 'Query Analysis',
    description: error.message,
    status: 'retrying',
    timestamp: new Date(),
    error: error.message
  };
};

// Hook to manage AI processing state
export const useAIProcessingFeedback = () => {
  const [isProcessing, setIsProcessing] = useState(false);
  const [steps, setSteps] = useState<AIProcessingStep[]>([]);
  const [currentStep, setCurrentStep] = useState<string>('');

  const startProcessing = (initialSteps: string[] = []) => {
    setIsProcessing(true);
    setSteps(initialSteps.map((title, index) => ({
      id: `step-${index}`,
      title,
      description: `Processing ${title.toLowerCase()}...`,
      status: 'pending' as const,
      timestamp: new Date()
    })));
  };

  const updateStep = (stepId: string, updates: Partial<AIProcessingStep>) => {
    setSteps(prev => prev.map(step => 
      step.id === stepId ? { ...step, ...updates } : step
    ));
  };

  const addStep = (step: AIProcessingStep) => {
    setSteps(prev => [...prev, step]);
  };

  const completeProcessing = () => {
    setIsProcessing(false);
    setCurrentStep('');
  };

  const reset = () => {
    setIsProcessing(false);
    setSteps([]);
    setCurrentStep('');
  };

  return {
    isProcessing,
    steps,
    currentStep,
    startProcessing,
    updateStep,
    addStep,
    completeProcessing,
    reset,
    setCurrentStep
  };
};
