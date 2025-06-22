import React from 'react'
import { Spin, Skeleton, Card, Space, Typography, Progress, Alert } from 'antd'
import {
  LoadingOutlined,
  EyeOutlined,
  BarChartOutlined,
  DatabaseOutlined,
  MessageOutlined,
  ClockCircleOutlined
} from '@ant-design/icons'

const { Text } = Typography

export interface LoadingStateProps {
  size?: 'small' | 'default' | 'large'
  tip?: string
  spinning?: boolean
  delay?: number
  className?: string
}

export interface SkeletonStateProps {
  active?: boolean
  loading?: boolean
  rows?: number
  title?: boolean
  avatar?: boolean
  paragraph?: boolean
  className?: string
}

/**
 * Comprehensive loading and skeleton states for different contexts
 */

// Generic loading spinner
export const LoadingSpinner: React.FC<LoadingStateProps> = ({
  size = 'default',
  tip = 'Loading...',
  spinning = true,
  delay = 0,
  className = ''
}) => (
  <div className={`loading-spinner ${className}`} style={{ textAlign: 'center', padding: '40px 0' }}>
    <Spin 
      size={size} 
      tip={tip} 
      spinning={spinning} 
      delay={delay}
      indicator={<LoadingOutlined style={{ fontSize: size === 'large' ? 32 : size === 'small' ? 16 : 24 }} spin />}
    />
  </div>
)

// Transparency-specific loading states
export const TransparencyLoading: React.FC<{ message?: string }> = ({ 
  message = 'Loading transparency data...' 
}) => (
  <Card style={{ textAlign: 'center', padding: '40px 0' }}>
    <Space direction="vertical" size="large">
      <EyeOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
      <Spin size="large" tip={message} />
      <Text type="secondary">Analyzing query transparency</Text>
    </Space>
  </Card>
)

export const TraceAnalysisLoading: React.FC = () => (
  <Card>
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Skeleton.Input active style={{ width: '200px' }} />
      <Skeleton active paragraph={{ rows: 3 }} />
      <div style={{ display: 'flex', gap: '16px' }}>
        <Skeleton.Button active style={{ width: '100px' }} />
        <Skeleton.Button active style={{ width: '100px' }} />
        <Skeleton.Button active style={{ width: '100px' }} />
      </div>
    </Space>
  </Card>
)

export const MetricsLoading: React.FC = () => (
  <Card>
    <Space direction="vertical" style={{ width: '100%' }}>
      <Space>
        <BarChartOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
        <Skeleton.Input active style={{ width: '150px' }} />
      </Space>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: '16px' }}>
        {[1, 2, 3, 4].map(i => (
          <div key={i} style={{ textAlign: 'center' }}>
            <Skeleton.Input active style={{ width: '60px', height: '40px' }} />
            <Skeleton.Input active style={{ width: '80px', marginTop: '8px' }} />
          </div>
        ))}
      </div>
    </Space>
  </Card>
)

// Chat-specific loading states
export const ChatLoading: React.FC<{ message?: string }> = ({ 
  message = 'AI is thinking...' 
}) => (
  <div style={{ padding: '16px', textAlign: 'center' }}>
    <Space direction="vertical">
      <MessageOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
      <Spin tip={message} />
    </Space>
  </div>
)

export const MessageLoading: React.FC = () => (
  <div style={{ padding: '16px' }}>
    <Space direction="vertical" style={{ width: '100%' }}>
      <Space>
        <Skeleton.Avatar active />
        <Skeleton.Input active style={{ width: '100px' }} />
      </Space>
      <Skeleton active paragraph={{ rows: 2, width: ['100%', '80%'] }} title={false} />
    </Space>
  </div>
)

export const QueryProcessingLoading: React.FC<{ 
  step?: string
  progress?: number 
}> = ({ 
  step = 'Processing query...', 
  progress 
}) => (
  <Card style={{ textAlign: 'center' }}>
    <Space direction="vertical" style={{ width: '100%' }}>
      <ClockCircleOutlined style={{ fontSize: '32px', color: '#1890ff' }} />
      <Text strong>{step}</Text>
      {progress !== undefined && (
        <Progress 
          percent={progress} 
          status="active"
          strokeColor="#1890ff"
          style={{ width: '200px' }}
        />
      )}
      <Text type="secondary" style={{ fontSize: '12px' }}>
        This may take a few moments...
      </Text>
    </Space>
  </Card>
)

// Data loading states
export const DataTableLoading: React.FC<{ rows?: number }> = ({ rows = 5 }) => (
  <Card>
    <Space direction="vertical" style={{ width: '100%' }}>
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} style={{ display: 'flex', gap: '16px', alignItems: 'center' }}>
          <Skeleton.Button active style={{ width: '40px' }} />
          <Skeleton.Input active style={{ width: '200px' }} />
          <Skeleton.Input active style={{ width: '150px' }} />
          <Skeleton.Input active style={{ width: '100px' }} />
          <Skeleton.Button active style={{ width: '80px' }} />
        </div>
      ))}
    </Space>
  </Card>
)

export const DatabaseLoading: React.FC<{ message?: string }> = ({ 
  message = 'Connecting to database...' 
}) => (
  <Card style={{ textAlign: 'center', padding: '40px 0' }}>
    <Space direction="vertical" size="large">
      <DatabaseOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
      <Spin size="large" tip={message} />
      <Text type="secondary">Fetching data from server</Text>
    </Space>
  </Card>
)

// Empty states
export const EmptyState: React.FC<{
  icon?: React.ReactNode
  title?: string
  description?: string
  action?: React.ReactNode
}> = ({
  icon = <DatabaseOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />,
  title = 'No data available',
  description = 'There is no data to display at the moment.',
  action
}) => (
  <div style={{ textAlign: 'center', padding: '40px 0' }}>
    <Space direction="vertical" size="large">
      {icon}
      <div>
        <Text strong style={{ fontSize: '16px', color: '#666' }}>{title}</Text>
        <div style={{ marginTop: '8px' }}>
          <Text type="secondary">{description}</Text>
        </div>
      </div>
      {action}
    </Space>
  </div>
)

export const NoTransparencyData: React.FC = () => (
  <EmptyState
    icon={<EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />}
    title="No transparency data"
    description="No transparency traces are available for the selected criteria."
  />
)

export const NoMetricsData: React.FC = () => (
  <EmptyState
    icon={<BarChartOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />}
    title="No metrics available"
    description="No performance metrics are available for the selected time period."
  />
)

export const NoMessagesData: React.FC = () => (
  <EmptyState
    icon={<MessageOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />}
    title="No messages"
    description="Start a conversation by sending your first message."
  />
)

// Error states
export const ErrorState: React.FC<{
  title?: string
  description?: string
  action?: React.ReactNode
}> = ({
  title = 'Something went wrong',
  description = 'An error occurred while loading the data.',
  action
}) => (
  <Alert
    message={title}
    description={description}
    type="error"
    showIcon
    action={action}
    style={{ margin: '16px 0' }}
  />
)

export const ConnectionError: React.FC<{ onRetry?: () => void }> = ({ onRetry }) => (
  <ErrorState
    title="Connection Error"
    description="Unable to connect to the server. Please check your internet connection and try again."
    action={onRetry && (
      <button onClick={onRetry} style={{ 
        background: 'none', 
        border: 'none', 
        color: '#1890ff', 
        cursor: 'pointer',
        textDecoration: 'underline'
      }}>
        Retry
      </button>
    )}
  />
)

export const TimeoutError: React.FC<{ onRetry?: () => void }> = ({ onRetry }) => (
  <ErrorState
    title="Request Timeout"
    description="The request took too long to complete. Please try again."
    action={onRetry && (
      <button onClick={onRetry} style={{ 
        background: 'none', 
        border: 'none', 
        color: '#1890ff', 
        cursor: 'pointer',
        textDecoration: 'underline'
      }}>
        Try Again
      </button>
    )}
  />
)

// Accessibility-enhanced loading states
export const AccessibleLoading: React.FC<LoadingStateProps & {
  ariaLabel?: string
  announceText?: string
}> = ({
  size = 'default',
  tip = 'Loading...',
  ariaLabel = 'Loading content',
  announceText = 'Content is loading, please wait',
  className = ''
}) => (
  <div 
    className={`accessible-loading ${className}`}
    role="status"
    aria-label={ariaLabel}
    aria-live="polite"
    style={{ textAlign: 'center', padding: '40px 0' }}
  >
    <Spin size={size} tip={tip} />
    <span className="sr-only">{announceText}</span>
  </div>
)

// Progressive loading with steps
export const ProgressiveLoading: React.FC<{
  steps: string[]
  currentStep: number
  progress?: number
}> = ({ steps, currentStep, progress }) => (
  <Card style={{ textAlign: 'center' }}>
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Text strong style={{ fontSize: '16px' }}>
        {steps[currentStep] || 'Processing...'}
      </Text>
      
      {progress !== undefined && (
        <Progress 
          percent={progress} 
          status="active"
          strokeColor="#1890ff"
          style={{ width: '300px' }}
        />
      )}
      
      <div>
        <Text type="secondary" style={{ fontSize: '12px' }}>
          Step {currentStep + 1} of {steps.length}
        </Text>
      </div>
      
      <div style={{ fontSize: '12px', color: '#999' }}>
        {steps.map((step, index) => (
          <div key={index} style={{ 
            color: index === currentStep ? '#1890ff' : 
                   index < currentStep ? '#52c41a' : '#d9d9d9'
          }}>
            {index + 1}. {step}
          </div>
        ))}
      </div>
    </Space>
  </Card>
)

export default {
  LoadingSpinner,
  TransparencyLoading,
  TraceAnalysisLoading,
  MetricsLoading,
  ChatLoading,
  MessageLoading,
  QueryProcessingLoading,
  DataTableLoading,
  DatabaseLoading,
  EmptyState,
  NoTransparencyData,
  NoMetricsData,
  NoMessagesData,
  ErrorState,
  ConnectionError,
  TimeoutError,
  AccessibleLoading,
  ProgressiveLoading
}
