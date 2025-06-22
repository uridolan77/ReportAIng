import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Progress, 
  Timeline,
  Tag,
  Button,
  List,
  Avatar,
  Tooltip,
  Alert
} from 'antd'
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  UserOutlined,
  StopOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'

const { Title, Text } = Typography

export interface LiveQueryTrackerProps {
  queryId?: string
  autoTrack?: boolean
  showStepDetails?: boolean
  showUserInfo?: boolean
  onQueryComplete?: (queryId: string, result: QueryResult) => void
  onStepComplete?: (queryId: string, step: QueryStep) => void
  className?: string
  testId?: string
}

export interface QueryStep {
  id: string
  name: string
  status: 'pending' | 'running' | 'completed' | 'failed'
  startTime: string
  endTime?: string
  confidence?: number
  tokens?: number
  description?: string
  error?: string
}

export interface QueryResult {
  queryId: string
  userId: string
  question: string
  status: 'running' | 'completed' | 'failed'
  startTime: string
  endTime?: string
  totalSteps: number
  completedSteps: number
  overallConfidence: number
  totalTokens: number
  totalTime: number
  steps: QueryStep[]
}

/**
 * LiveQueryTracker - Tracks individual query execution in real-time
 * 
 * Features:
 * - Live query progress tracking
 * - Step-by-step execution monitoring
 * - Real-time confidence updates
 * - Performance metrics display
 * - Error detection and reporting
 * - User context information
 */
export const LiveQueryTracker: React.FC<LiveQueryTrackerProps> = ({
  queryId,
  autoTrack = true,
  showStepDetails = true,
  showUserInfo = true,
  onQueryComplete,
  onStepComplete,
  className,
  testId = 'live-query-tracker'
}) => {
  const [tracking, setTracking] = useState(autoTrack)
  const [currentQuery, setCurrentQuery] = useState<QueryResult | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  // Simulate query tracking for demo purposes
  useEffect(() => {
    if (tracking && queryId) {
      startQueryTracking(queryId)
    }
  }, [tracking, queryId])

  const startQueryTracking = (id: string) => {
    setIsLoading(true)
    
    // Simulate initial query setup
    const initialQuery: QueryResult = {
      queryId: id,
      userId: 'user-123',
      question: 'Show me quarterly sales by region with year-over-year comparison',
      status: 'running',
      startTime: new Date().toISOString(),
      totalSteps: 5,
      completedSteps: 0,
      overallConfidence: 0,
      totalTokens: 0,
      totalTime: 0,
      steps: [
        { id: 'step-1', name: 'Query Parsing', status: 'pending', startTime: new Date().toISOString() },
        { id: 'step-2', name: 'Intent Classification', status: 'pending', startTime: new Date().toISOString() },
        { id: 'step-3', name: 'Context Building', status: 'pending', startTime: new Date().toISOString() },
        { id: 'step-4', name: 'SQL Generation', status: 'pending', startTime: new Date().toISOString() },
        { id: 'step-5', name: 'Response Formatting', status: 'pending', startTime: new Date().toISOString() }
      ]
    }

    setCurrentQuery(initialQuery)
    setIsLoading(false)

    // Simulate step execution
    simulateStepExecution(initialQuery)
  }

  const simulateStepExecution = (query: QueryResult) => {
    let currentStepIndex = 0
    
    const executeNextStep = () => {
      if (currentStepIndex >= query.steps.length) {
        // Query completed
        const completedQuery: QueryResult = {
          ...query,
          status: 'completed',
          endTime: new Date().toISOString(),
          completedSteps: query.steps.length,
          overallConfidence: query.steps.reduce((sum, step) => sum + (step.confidence || 0), 0) / query.steps.length,
          totalTime: Date.now() - new Date(query.startTime).getTime()
        }
        
        setCurrentQuery(completedQuery)
        onQueryComplete?.(query.queryId, completedQuery)
        return
      }

      const step = query.steps[currentStepIndex]
      
      // Start step
      const updatedStep: QueryStep = {
        ...step,
        status: 'running',
        startTime: new Date().toISOString()
      }

      setCurrentQuery(prev => prev ? {
        ...prev,
        steps: prev.steps.map(s => s.id === step.id ? updatedStep : s)
      } : null)

      // Simulate step completion after random delay
      setTimeout(() => {
        const completedStep: QueryStep = {
          ...updatedStep,
          status: Math.random() > 0.1 ? 'completed' : 'failed', // 90% success rate
          endTime: new Date().toISOString(),
          confidence: Math.random() * 0.4 + 0.6, // 0.6 to 1.0
          tokens: Math.floor(Math.random() * 200) + 50,
          error: Math.random() > 0.9 ? 'Simulated error for demo' : undefined
        }

        setCurrentQuery(prev => prev ? {
          ...prev,
          completedSteps: prev.completedSteps + 1,
          totalTokens: prev.totalTokens + (completedStep.tokens || 0),
          steps: prev.steps.map(s => s.id === step.id ? completedStep : s)
        } : null)

        onStepComplete?.(query.queryId, completedStep)
        
        currentStepIndex++
        setTimeout(executeNextStep, 500) // Brief pause between steps
      }, Math.random() * 2000 + 1000) // 1-3 seconds per step
    }

    executeNextStep()
  }

  const handleToggleTracking = () => {
    setTracking(!tracking)
    if (!tracking && queryId) {
      startQueryTracking(queryId)
    }
  }

  const handleStopTracking = () => {
    setTracking(false)
    setCurrentQuery(null)
  }

  const getStepIcon = (status: string) => {
    switch (status) {
      case 'pending': return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />
      case 'running': return <PlayCircleOutlined style={{ color: '#1890ff' }} />
      case 'completed': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'failed': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
      default: return <ClockCircleOutlined />
    }
  }

  const getStepColor = (status: string) => {
    switch (status) {
      case 'pending': return 'gray'
      case 'running': return 'blue'
      case 'completed': return 'green'
      case 'failed': return 'red'
      default: return 'gray'
    }
  }

  const calculateProgress = () => {
    if (!currentQuery) return 0
    return (currentQuery.completedSteps / currentQuery.totalSteps) * 100
  }

  const getCurrentStep = () => {
    if (!currentQuery) return null
    return currentQuery.steps.find(step => step.status === 'running') || 
           currentQuery.steps[currentQuery.completedSteps]
  }

  if (!queryId) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No query selected for tracking</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <EyeOutlined />
          <span>Live Query Tracker</span>
          {currentQuery && (
            <Tag color={currentQuery.status === 'completed' ? 'green' : 
                        currentQuery.status === 'failed' ? 'red' : 'blue'}>
              {currentQuery.status.toUpperCase()}
            </Tag>
          )}
        </Space>
      }
      extra={
        <Space>
          <Button 
            size="small" 
            type={tracking ? 'default' : 'primary'}
            icon={tracking ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
            onClick={handleToggleTracking}
            loading={isLoading}
          >
            {tracking ? 'Pause' : 'Start'}
          </Button>
          {tracking && (
            <Button 
              size="small" 
              icon={<StopOutlined />}
              onClick={handleStopTracking}
            >
              Stop
            </Button>
          )}
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      {currentQuery ? (
        <Space direction="vertical" style={{ width: '100%' }} size="large">
          {/* Query Information */}
          {showUserInfo && (
            <Card size="small" title="Query Information">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Space>
                  <UserOutlined />
                  <Text strong>User:</Text>
                  <Text>{currentQuery.userId}</Text>
                </Space>
                <div>
                  <Text strong>Question:</Text>
                  <div style={{ marginTop: 4 }}>
                    <Text>{currentQuery.question}</Text>
                  </div>
                </div>
              </Space>
            </Card>
          )}

          {/* Progress Overview */}
          <Card size="small" title="Progress Overview">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Progress 
                percent={calculateProgress()}
                status={currentQuery.status === 'failed' ? 'exception' : 
                        currentQuery.status === 'completed' ? 'success' : 'active'}
                format={() => `${currentQuery.completedSteps}/${currentQuery.totalSteps} steps`}
              />
              
              <Space size="large">
                <Space>
                  <ClockCircleOutlined />
                  <Text>
                    {currentQuery.endTime ? 
                      `${((new Date(currentQuery.endTime).getTime() - new Date(currentQuery.startTime).getTime()) / 1000).toFixed(1)}s` :
                      `${((Date.now() - new Date(currentQuery.startTime).getTime()) / 1000).toFixed(1)}s`
                    }
                  </Text>
                </Space>
                <Space>
                  <BarChartOutlined />
                  <Text>{currentQuery.totalTokens} tokens</Text>
                </Space>
                {currentQuery.overallConfidence > 0 && (
                  <ConfidenceIndicator
                    confidence={currentQuery.overallConfidence}
                    size="small"
                    type="badge"
                    showPercentage
                  />
                )}
              </Space>
            </Space>
          </Card>

          {/* Current Step */}
          {getCurrentStep() && (
            <Alert
              message={
                <Space>
                  <ThunderboltOutlined />
                  <Text strong>Current Step: {getCurrentStep()?.name}</Text>
                </Space>
              }
              type="info"
              showIcon={false}
            />
          )}

          {/* Step Details */}
          {showStepDetails && (
            <Card size="small" title="Step Details">
              <Timeline>
                {currentQuery.steps.map((step, index) => (
                  <Timeline.Item
                    key={step.id}
                    color={getStepColor(step.status)}
                    dot={getStepIcon(step.status)}
                  >
                    <Space direction="vertical" size="small">
                      <Space>
                        <Text strong>{step.name}</Text>
                        <Tag color={getStepColor(step.status)} size="small">
                          {step.status.toUpperCase()}
                        </Tag>
                      </Space>
                      
                      {step.description && (
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {step.description}
                        </Text>
                      )}
                      
                      <Space size="large">
                        {step.confidence && (
                          <ConfidenceIndicator
                            confidence={step.confidence}
                            size="small"
                            type="badge"
                          />
                        )}
                        {step.tokens && (
                          <Space size="small">
                            <BarChartOutlined style={{ fontSize: '12px' }} />
                            <Text style={{ fontSize: '12px' }}>{step.tokens} tokens</Text>
                          </Space>
                        )}
                        {step.endTime && (
                          <Space size="small">
                            <ClockCircleOutlined style={{ fontSize: '12px' }} />
                            <Text style={{ fontSize: '12px' }}>
                              {((new Date(step.endTime).getTime() - new Date(step.startTime).getTime()) / 1000).toFixed(1)}s
                            </Text>
                          </Space>
                        )}
                      </Space>
                      
                      {step.error && (
                        <Alert
                          message={step.error}
                          type="error"
                          size="small"
                          showIcon
                        />
                      )}
                    </Space>
                  </Timeline.Item>
                ))}
              </Timeline>
            </Card>
          )}

          {/* Completion Status */}
          {currentQuery.status === 'completed' && (
            <Alert
              message="Query Completed Successfully"
              description={`Processed in ${((currentQuery.totalTime || 0) / 1000).toFixed(1)} seconds with ${currentQuery.totalTokens} tokens used.`}
              type="success"
              showIcon
            />
          )}

          {currentQuery.status === 'failed' && (
            <Alert
              message="Query Failed"
              description="One or more steps failed during query processing. Check step details for more information."
              type="error"
              showIcon
            />
          )}
        </Space>
      ) : (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <PlayCircleOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">Click Start to begin tracking query execution</Text>
          </Space>
        </div>
      )}
    </Card>
  )
}

export default LiveQueryTracker
