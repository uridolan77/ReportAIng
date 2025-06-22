import React, { useState, useEffect, useCallback, useRef } from 'react'
import { 
  Card, 
  Steps, 
  Progress, 
  Alert, 
  Space, 
  Typography, 
  Tag, 
  Button,
  Tooltip,
  Statistic,
  Row,
  Col,
  Timeline,
  Badge
} from 'antd'
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { transparencyHub } from '@shared/services/signalr/transparencyHub'
import type { 
  TransparencyUpdateEvent, 
  StepCompletedEvent, 
  ConfidenceUpdateEvent,
  TraceCompletedEvent,
  TransparencyErrorEvent
} from '@shared/services/signalr/transparencyHub'

const { Title, Text } = Typography
const { Step } = Steps

export interface LiveTransparencyPanelProps {
  traceId: string
  autoStart?: boolean
  showDetailedSteps?: boolean
  compact?: boolean
  onTraceComplete?: (result: TraceCompletedEvent) => void
  onError?: (error: TransparencyErrorEvent) => void
  className?: string
}

interface ProcessingStep {
  id: string
  stepName: string
  stepOrder: number
  status: 'wait' | 'process' | 'finish' | 'error'
  confidence: number
  tokensAdded: number
  content: string
  startTime?: string
  endTime?: string
  duration?: number
}

/**
 * LiveTransparencyPanel - Real-time query processing visualization
 * 
 * Features:
 * - Real-time step-by-step progress tracking
 * - Live confidence updates
 * - Token usage monitoring
 * - Processing time tracking
 * - Error handling and notifications
 * - SignalR integration for live updates
 */
export const LiveTransparencyPanel: React.FC<LiveTransparencyPanelProps> = ({
  traceId,
  autoStart = true,
  showDetailedSteps = true,
  compact = false,
  onTraceComplete,
  onError,
  className
}) => {
  const [isConnected, setIsConnected] = useState(false)
  const [currentStep, setCurrentStep] = useState(0)
  const [overallProgress, setOverallProgress] = useState(0)
  const [confidence, setConfidence] = useState(0)
  const [steps, setSteps] = useState<ProcessingStep[]>([])
  const [isComplete, setIsComplete] = useState(false)
  const [hasError, setHasError] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string>('')
  const [totalTokens, setTotalTokens] = useState(0)
  const [processingTime, setProcessingTime] = useState(0)
  const [isTracking, setIsTracking] = useState(false)

  const startTimeRef = useRef<Date | null>(null)
  const intervalRef = useRef<NodeJS.Timeout | null>(null)

  // Initialize SignalR connection and event handlers
  useEffect(() => {
    const initializeConnection = async () => {
      try {
        if (!transparencyHub.isConnected) {
          await transparencyHub.connect()
        }
        setIsConnected(true)

        if (autoStart) {
          await startTracking()
        }
      } catch (error) {
        console.error('Failed to connect to transparency hub:', error)
        setHasError(true)
        setErrorMessage('Failed to connect to real-time updates')
      }
    }

    initializeConnection()

    return () => {
      stopTracking()
    }
  }, [traceId, autoStart])

  // Setup event listeners
  useEffect(() => {
    if (!isConnected) return

    const handleTransparencyUpdate = (update: TransparencyUpdateEvent) => {
      if (update.traceId === traceId) {
        setOverallProgress(update.progress)
        setConfidence(update.confidence)
      }
    }

    const handleStepCompleted = (data: StepCompletedEvent) => {
      if (data.traceId === traceId) {
        const newStep: ProcessingStep = {
          id: data.step.id,
          stepName: data.step.stepName,
          stepOrder: data.step.stepOrder,
          status: data.step.success ? 'finish' : 'error',
          confidence: data.step.confidence,
          tokensAdded: data.step.tokensAdded,
          content: data.step.content,
          endTime: new Date().toISOString()
        }

        setSteps(prev => {
          const updated = [...prev]
          const existingIndex = updated.findIndex(s => s.id === newStep.id)
          
          if (existingIndex >= 0) {
            updated[existingIndex] = { ...updated[existingIndex], ...newStep }
          } else {
            updated.push(newStep)
          }
          
          return updated.sort((a, b) => a.stepOrder - b.stepOrder)
        })

        setCurrentStep(data.step.stepOrder)
        setTotalTokens(prev => prev + data.step.tokensAdded)
      }
    }

    const handleConfidenceUpdate = (data: ConfidenceUpdateEvent) => {
      if (data.traceId === traceId) {
        setConfidence(data.confidence)
      }
    }

    const handleTraceCompleted = (data: TraceCompletedEvent) => {
      if (data.traceId === traceId) {
        setIsComplete(true)
        setIsTracking(false)
        setOverallProgress(100)
        setConfidence(data.finalConfidence)
        setTotalTokens(data.totalTokens)
        setProcessingTime(data.processingTime)
        
        if (intervalRef.current) {
          clearInterval(intervalRef.current)
        }

        onTraceComplete?.(data)
      }
    }

    const handleTransparencyError = (data: TransparencyErrorEvent) => {
      if (!data.traceId || data.traceId === traceId) {
        setHasError(true)
        setErrorMessage(data.error)
        setIsTracking(false)
        onError?.(data)
      }
    }

    // Subscribe to events
    transparencyHub.on('transparencyUpdate', handleTransparencyUpdate)
    transparencyHub.on('stepCompleted', handleStepCompleted)
    transparencyHub.on('confidenceUpdate', handleConfidenceUpdate)
    transparencyHub.on('traceCompleted', handleTraceCompleted)
    transparencyHub.on('transparencyError', handleTransparencyError)

    return () => {
      // Cleanup event listeners
      transparencyHub.off('transparencyUpdate', handleTransparencyUpdate)
      transparencyHub.off('stepCompleted', handleStepCompleted)
      transparencyHub.off('confidenceUpdate', handleConfidenceUpdate)
      transparencyHub.off('traceCompleted', handleTraceCompleted)
      transparencyHub.off('transparencyError', handleTransparencyError)
    }
  }, [isConnected, traceId, onTraceComplete, onError])

  // Start tracking a trace
  const startTracking = useCallback(async () => {
    try {
      await transparencyHub.subscribeToTrace(traceId)
      setIsTracking(true)
      startTimeRef.current = new Date()
      
      // Start processing time counter
      intervalRef.current = setInterval(() => {
        if (startTimeRef.current && !isComplete) {
          const elapsed = Date.now() - startTimeRef.current.getTime()
          setProcessingTime(Math.floor(elapsed / 1000))
        }
      }, 1000)

    } catch (error) {
      console.error('Failed to start tracking:', error)
      setHasError(true)
      setErrorMessage('Failed to start real-time tracking')
    }
  }, [traceId, isComplete])

  // Stop tracking a trace
  const stopTracking = useCallback(async () => {
    try {
      await transparencyHub.unsubscribeFromTrace(traceId)
      setIsTracking(false)
      
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
        intervalRef.current = null
      }
    } catch (error) {
      console.error('Failed to stop tracking:', error)
    }
  }, [traceId])

  // Reset tracking state
  const resetTracking = useCallback(() => {
    setCurrentStep(0)
    setOverallProgress(0)
    setConfidence(0)
    setSteps([])
    setIsComplete(false)
    setHasError(false)
    setErrorMessage('')
    setTotalTokens(0)
    setProcessingTime(0)
    startTimeRef.current = null
    
    if (intervalRef.current) {
      clearInterval(intervalRef.current)
      intervalRef.current = null
    }
  }, [])

  // Get confidence color
  const getConfidenceColor = (conf: number) => {
    if (conf >= 0.8) return '#52c41a'
    if (conf >= 0.6) return '#faad14'
    return '#ff4d4f'
  }

  // Get progress status
  const getProgressStatus = () => {
    if (hasError) return 'exception'
    if (isComplete) return 'success'
    if (isTracking) return 'active'
    return 'normal'
  }

  return (
    <Card
      title={
        <Space>
          <EyeOutlined />
          <span>Live Query Processing</span>
          {isTracking && <Badge status="processing" text="Live" />}
          {isComplete && <Badge status="success" text="Complete" />}
          {hasError && <Badge status="error" text="Error" />}
        </Space>
      }
      className={className}
      extra={
        <Space>
          {!isTracking && !isComplete && (
            <Button
              type="primary"
              size="small"
              icon={<PlayCircleOutlined />}
              onClick={startTracking}
              disabled={!isConnected}
            >
              Start
            </Button>
          )}
          {isTracking && (
            <Button
              size="small"
              icon={<PauseCircleOutlined />}
              onClick={stopTracking}
            >
              Stop
            </Button>
          )}
          <Button
            size="small"
            icon={<ReloadOutlined />}
            onClick={resetTracking}
          >
            Reset
          </Button>
        </Space>
      }
    >
      {/* Error Alert */}
      {hasError && (
        <Alert
          message="Processing Error"
          description={errorMessage}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          action={
            <Button size="small" onClick={resetTracking}>
              Retry
            </Button>
          }
        />
      )}

      {/* Connection Status */}
      {!isConnected && (
        <Alert
          message="Connection Status"
          description="Not connected to real-time updates"
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Progress Overview */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={compact ? 24 : 8}>
          <Card size="small">
            <Statistic
              title="Overall Progress"
              value={overallProgress}
              suffix="%"
              valueStyle={{ color: getProgressStatus() === 'exception' ? '#ff4d4f' : '#3f8600' }}
            />
            <Progress
              percent={overallProgress}
              status={getProgressStatus()}
              showInfo={false}
              strokeWidth={8}
            />
          </Card>
        </Col>
        
        <Col span={compact ? 12 : 8}>
          <Card size="small">
            <Statistic
              title="Confidence"
              value={Math.round(confidence * 100)}
              suffix="%"
              valueStyle={{ color: getConfidenceColor(confidence) }}
              prefix={<ThunderboltOutlined />}
            />
          </Card>
        </Col>
        
        <Col span={compact ? 12 : 8}>
          <Card size="small">
            <Statistic
              title="Processing Time"
              value={processingTime}
              suffix="s"
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Processing Steps */}
      {showDetailedSteps && steps.length > 0 && (
        <div>
          <Title level={5}>Processing Steps</Title>
          <Steps
            current={currentStep}
            direction={compact ? 'horizontal' : 'vertical'}
            size={compact ? 'small' : 'default'}
            items={steps.map((step, index) => ({
              title: step.stepName,
              description: compact ? undefined : (
                <Space direction="vertical" size="small">
                  <Text type="secondary">
                    Confidence: {Math.round(step.confidence * 100)}% | 
                    Tokens: +{step.tokensAdded}
                  </Text>
                  {step.content && (
                    <Text style={{ fontSize: '12px' }}>{step.content}</Text>
                  )}
                </Space>
              ),
              status: step.status,
              icon: step.status === 'finish' ? <CheckCircleOutlined /> : 
                    step.status === 'error' ? <ExclamationCircleOutlined /> : undefined,
            }))}
          />
        </div>
      )}

      {/* Completion Summary */}
      {isComplete && (
        <Alert
          message="Query Processing Complete"
          description={
            <Space split=" | ">
              <span>Final Confidence: {Math.round(confidence * 100)}%</span>
              <span>Total Tokens: {totalTokens}</span>
              <span>Processing Time: {processingTime}s</span>
            </Space>
          }
          type="success"
          showIcon
          style={{ marginTop: 16 }}
        />
      )}
    </Card>
  )
}

export default LiveTransparencyPanel
