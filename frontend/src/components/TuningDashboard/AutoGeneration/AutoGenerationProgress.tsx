import React from 'react';
import { Card, Progress, Typography, Space, Spin } from 'antd';
import { LoadingOutlined, RobotOutlined } from '@ant-design/icons';

const { Title, Text } = Typography;

interface AutoGenerationProgressProps {
  progress: number;
  currentTask: string;
  estimatedTimeRemaining?: string;
  recentlyCompleted?: string[];
}

export const AutoGenerationProgress: React.FC<AutoGenerationProgressProps> = ({
  progress,
  currentTask,
  estimatedTimeRemaining,
  recentlyCompleted = []
}) => {
  const getProgressStatus = () => {
    if (progress < 30) return 'active';
    if (progress < 70) return 'active';
    if (progress < 100) return 'active';
    return 'success';
  };

  const getProgressColor = () => {
    if (progress < 30) return '#1890ff';
    if (progress < 70) return '#52c41a';
    if (progress < 100) return '#722ed1';
    return '#52c41a';
  };

  return (
    <Card style={{ marginBottom: '24px' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <Title level={4}>
          <RobotOutlined style={{ marginRight: '8px', color: '#1890ff' }} />
          Auto-Generation in Progress
        </Title>
      </div>

      <div style={{ marginBottom: '24px' }}>
        <Progress
          percent={Math.round(progress)}
          status={getProgressStatus()}
          strokeColor={getProgressColor()}
          showInfo={true}
          format={(percent) => `${percent}%`}
        />
      </div>

      <div style={{ textAlign: 'center', marginBottom: '16px' }}>
        <Space direction="vertical" size="small">
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <Spin
              indicator={<LoadingOutlined style={{ fontSize: 16, marginRight: '8px' }} spin />}
            />
            <Text strong style={{ fontSize: '16px' }}>
              {currentTask}
            </Text>
          </div>

          {estimatedTimeRemaining && (
            <Text type="secondary">
              Estimated time remaining: {estimatedTimeRemaining}
            </Text>
          )}
        </Space>
      </div>

      {recentlyCompleted.length > 0 && (
        <div style={{ marginTop: '16px' }}>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            Recently completed:
          </Text>
          <div style={{ marginTop: '8px' }}>
            {recentlyCompleted.slice(-3).map((item, index) => (
              <div key={index} style={{ marginBottom: '4px' }}>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  ✓ {item}
                </Text>
              </div>
            ))}
          </div>
        </div>
      )}

      <div style={{ marginTop: '24px', padding: '16px', backgroundColor: '#f6f8fa', borderRadius: '6px' }}>
        <Space direction="vertical" size="small" style={{ width: '100%' }}>
          <Text strong style={{ fontSize: '14px' }}>
            What's happening:
          </Text>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 10 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 10 ? undefined : '#bfbfbf' }}>
              Analyzing database schema and extracting metadata
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 30 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 30 ? undefined : '#bfbfbf' }}>
              Generating business-friendly table descriptions using AI
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 60 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 60 ? undefined : '#bfbfbf' }}>
              Creating business glossary terms from column patterns
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 80 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 80 ? undefined : '#bfbfbf' }}>
              Analyzing table relationships and business domains
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 95 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 95 ? undefined : '#bfbfbf' }}>
              Finalizing results and calculating confidence scores
            </Text>
          </div>
        </Space>
      </div>

      <div style={{ marginTop: '16px', textAlign: 'center' }}>
        <Text type="secondary" style={{ fontSize: '12px' }}>
          This process typically takes 1-3 minutes depending on your database size.
          <br />
          You can review and edit all generated content before applying it.
        </Text>
      </div>
    </Card>
  );
};
