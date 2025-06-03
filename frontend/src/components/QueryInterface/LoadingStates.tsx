import React from 'react';
import { Spin, Typography, Progress, Space, Card } from 'antd';
import { 
  RobotOutlined, 
  DatabaseOutlined, 
  BarChartOutlined,
  ThunderboltOutlined 
} from '@ant-design/icons';

const { Text } = Typography;

interface QueryLoadingProps {
  progress?: number;
  stage?: 'analyzing' | 'generating' | 'executing' | 'processing';
  message?: string;
}

export const QueryLoadingState: React.FC<QueryLoadingProps> = ({ 
  progress = 0, 
  stage = 'analyzing',
  message 
}) => {
  const getStageInfo = (currentStage: string) => {
    switch (currentStage) {
      case 'analyzing':
        return {
          icon: <RobotOutlined style={{ fontSize: '24px', color: '#3b82f6' }} />,
          title: 'AI is analyzing your question...',
          description: 'Understanding your query and identifying relevant data sources'
        };
      case 'generating':
        return {
          icon: <DatabaseOutlined style={{ fontSize: '24px', color: '#10b981' }} />,
          title: 'Generating SQL query...',
          description: 'Creating optimized database query based on your request'
        };
      case 'executing':
        return {
          icon: <ThunderboltOutlined style={{ fontSize: '24px', color: '#f59e0b' }} />,
          title: 'Executing query...',
          description: 'Running query against your database'
        };
      case 'processing':
        return {
          icon: <BarChartOutlined style={{ fontSize: '24px', color: '#8b5cf6' }} />,
          title: 'Processing results...',
          description: 'Analyzing data and generating insights'
        };
      default:
        return {
          icon: <RobotOutlined style={{ fontSize: '24px', color: '#3b82f6' }} />,
          title: 'Processing...',
          description: 'Working on your request'
        };
    }
  };

  const stageInfo = getStageInfo(stage);

  return (
    <Card
      style={{
        textAlign: 'center',
        padding: '40px 20px',
        background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
        border: '2px solid #e2e8f0',
        borderRadius: '16px',
        margin: '20px 0',
        position: 'relative',
        overflow: 'hidden'
      }}
    >
      {/* Animated background */}
      <div style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: 'linear-gradient(45deg, transparent 30%, rgba(59, 130, 246, 0.05) 50%, transparent 70%)',
        animation: 'shimmer 2s infinite'
      }} />

      <Space direction="vertical" size="large" style={{ position: 'relative', zIndex: 1 }}>
        <div style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          marginBottom: '16px'
        }}>
          <Spin 
            size="large" 
            indicator={stageInfo.icon}
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          />
        </div>

        <div>
          <Text style={{
            fontSize: '20px',
            fontWeight: 700,
            color: '#1f2937',
            display: 'block',
            marginBottom: '8px',
            fontFamily: "'Poppins', sans-serif"
          }}>
            {stageInfo.title}
          </Text>
          <Text style={{
            fontSize: '14px',
            color: '#6b7280',
            fontWeight: 400
          }}>
            {message || stageInfo.description}
          </Text>
        </div>

        {progress > 0 && (
          <div style={{ width: '100%', maxWidth: '300px' }}>
            <Progress
              percent={progress}
              strokeColor={{
                '0%': '#3b82f6',
                '100%': '#8b5cf6',
              }}
              trailColor="#e2e8f0"
              strokeWidth={8}
              style={{
                marginBottom: '8px'
              }}
            />
            <Text style={{
              fontSize: '12px',
              color: '#6b7280'
            }}>
              {progress}% complete
            </Text>
          </div>
        )}

        {/* Stage indicators */}
        <div style={{
          display: 'flex',
          justifyContent: 'center',
          gap: '12px',
          marginTop: '16px'
        }}>
          {['analyzing', 'generating', 'executing', 'processing'].map((s, index) => (
            <div
              key={s}
              style={{
                width: '8px',
                height: '8px',
                borderRadius: '50%',
                background: s === stage ? '#3b82f6' : '#e2e8f0',
                transition: 'all 0.3s ease',
                animation: s === stage ? 'pulse 1.5s infinite' : 'none'
              }}
            />
          ))}
        </div>
      </Space>

      <style jsx>{`
        @keyframes shimmer {
          0% { transform: translateX(-100%); }
          100% { transform: translateX(100%); }
        }
        
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }
      `}</style>
    </Card>
  );
};

interface ProcessingIndicatorProps {
  message?: string;
  type?: 'query' | 'analysis' | 'visualization';
}

export const ProcessingIndicator: React.FC<ProcessingIndicatorProps> = ({ 
  message = 'Processing...', 
  type = 'query' 
}) => {
  const getIcon = () => {
    switch (type) {
      case 'analysis':
        return <RobotOutlined style={{ fontSize: '16px', color: '#3b82f6' }} />;
      case 'visualization':
        return <BarChartOutlined style={{ fontSize: '16px', color: '#8b5cf6' }} />;
      default:
        return <DatabaseOutlined style={{ fontSize: '16px', color: '#10b981' }} />;
    }
  };

  return (
    <div style={{
      display: 'inline-flex',
      alignItems: 'center',
      gap: '8px',
      padding: '8px 16px',
      background: 'rgba(59, 130, 246, 0.1)',
      borderRadius: '20px',
      border: '1px solid rgba(59, 130, 246, 0.2)'
    }}>
      <Spin size="small" indicator={getIcon()} />
      <Text style={{
        fontSize: '14px',
        color: '#3b82f6',
        fontWeight: 500
      }}>
        {message}
      </Text>
    </div>
  );
};

export const CopilotThinking: React.FC<{ message?: string }> = ({ 
  message = "Copilot is thinking..." 
}) => {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      gap: '12px',
      padding: '16px 20px',
      background: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
      borderRadius: '12px',
      border: '1px solid #3b82f620',
      margin: '16px 0'
    }}>
      <div style={{
        display: 'flex',
        gap: '4px'
      }}>
        {[0, 1, 2].map((i) => (
          <div
            key={i}
            style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              background: '#3b82f6',
              animation: `bounce 1.4s infinite ease-in-out ${i * 0.16}s`
            }}
          />
        ))}
      </div>
      <Text style={{
        fontSize: '14px',
        color: '#3b82f6',
        fontWeight: 500,
        fontFamily: "'Inter', sans-serif"
      }}>
        {message}
      </Text>

      <style jsx>{`
        @keyframes bounce {
          0%, 80%, 100% {
            transform: scale(0);
          }
          40% {
            transform: scale(1);
          }
        }
      `}</style>
    </div>
  );
};
