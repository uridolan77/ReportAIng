import React, { useEffect, useState } from 'react'
import {
  Card,
  Progress,
  Typography,
  Space,
  Tag,
  Timeline,
  Button,
  Tooltip,
  Alert,
  Spin
} from 'antd'
import {
  LoadingOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  BulbOutlined,
  BarChartOutlined,
  StopOutlined
} from '@ant-design/icons'
import { useAppSelector } from '@shared/hooks'
import { selectStreamingProgress, selectIsConnected } from '@shared/store/chat'
import { socketService } from '@shared/services/socketService'
import type { StreamingProgress as StreamingProgressType } from '@shared/types/chat'

const { Text, Title } = Typography

interface StreamingProgressProps {
  onCancel?: () => void
  compact?: boolean
}

export const StreamingProgress: React.FC<StreamingProgressProps> = ({
  onCancel,
  compact = false,
}) => {
  const streamingProgress = useAppSelector(selectStreamingProgress)
  const isConnected = useAppSelector(selectIsConnected)
  const [elapsedTime, setElapsedTime] = useState(0)
  const [startTime, setStartTime] = useState<Date | null>(null)

  useEffect(() => {
    if (streamingProgress && !startTime) {
      setStartTime(new Date())
    } else if (!streamingProgress) {
      setStartTime(null)
      setElapsedTime(0)
    }
  }, [streamingProgress, startTime])

  useEffect(() => {
    let interval: NodeJS.Timeout
    
    if (startTime && streamingProgress) {
      interval = setInterval(() => {
        setElapsedTime(Date.now() - startTime.getTime())
      }, 100)
    }
    
    return () => {
      if (interval) clearInterval(interval)
    }
  }, [startTime, streamingProgress])

  if (!streamingProgress) {
    return null
  }

  const getPhaseIcon = (phase: string, isActive: boolean = false) => {
    const iconProps = {
      style: { 
        color: isActive ? '#1890ff' : '#d9d9d9',
        fontSize: '16px'
      }
    }

    switch (phase) {
      case 'parsing':
        return <BulbOutlined {...iconProps} />
      case 'analyzing':
        return <DatabaseOutlined {...iconProps} />
      case 'executing':
        return <LoadingOutlined {...iconProps} />
      case 'formatting':
        return <BarChartOutlined {...iconProps} />
      case 'complete':
        return <CheckCircleOutlined style={{ color: '#52c41a', fontSize: '16px' }} />
      case 'error':
        return <CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: '16px' }} />
      default:
        return <ClockCircleOutlined {...iconProps} />
    }
  }

  const getPhaseLabel = (phase: string) => {
    switch (phase) {
      case 'parsing':
        return 'Parsing Query'
      case 'analyzing':
        return 'Analyzing Context'
      case 'executing':
        return 'Executing Query'
      case 'formatting':
        return 'Formatting Results'
      case 'complete':
        return 'Complete'
      case 'error':
        return 'Error'
      default:
        return 'Processing'
    }
  }

  const getProgressColor = (phase: string) => {
    switch (phase) {
      case 'parsing':
        return '#722ed1'
      case 'analyzing':
        return '#1890ff'
      case 'executing':
        return '#faad14'
      case 'formatting':
        return '#52c41a'
      case 'error':
        return '#ff4d4f'
      default:
        return '#1890ff'
    }
  }

  const formatElapsedTime = (ms: number) => {
    const seconds = Math.floor(ms / 1000)
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60
    
    if (minutes > 0) {
      return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`
    }
    return `${remainingSeconds}s`
  }

  const handleCancel = () => {
    if (streamingProgress.sessionId) {
      socketService.cancelStreamingQuery(streamingProgress.sessionId)
    }
    onCancel?.()
  }

  const renderPhaseDetails = () => {
    const { phaseData, phase } = streamingProgress
    
    if (!phaseData || !phaseData[phase as keyof typeof phaseData]) {
      return null
    }

    const data = phaseData[phase as keyof typeof phaseData]

    switch (phase) {
      case 'parsing':
        const parsingData = data as any
        return (
          <Space direction="vertical" size="small">
            <Text type="secondary">
              Tokens processed: {parsingData?.tokensProcessed || 0}
            </Text>
            {parsingData?.entitiesFound?.length > 0 && (
              <div>
                <Text type="secondary">Entities found: </Text>
                <Space wrap>
                  {parsingData.entitiesFound.slice(0, 3).map((entity: string, index: number) => (
                    <Tag key={index} size="small">{entity}</Tag>
                  ))}
                  {parsingData.entitiesFound.length > 3 && (
                    <Text type="secondary">+{parsingData.entitiesFound.length - 3} more</Text>
                  )}
                </Space>
              </div>
            )}
          </Space>
        )

      case 'analyzing':
        const analyzingData = data as any
        return (
          <Space direction="vertical" size="small">
            <Text type="secondary">
              Confidence: {((analyzingData?.confidence || 0) * 100).toFixed(1)}%
            </Text>
            {analyzingData?.tablesIdentified?.length > 0 && (
              <div>
                <Text type="secondary">Tables identified: </Text>
                <Space wrap>
                  {analyzingData.tablesIdentified.slice(0, 3).map((table: string, index: number) => (
                    <Tag key={index} size="small" icon={<DatabaseOutlined />}>{table}</Tag>
                  ))}
                  {analyzingData.tablesIdentified.length > 3 && (
                    <Text type="secondary">+{analyzingData.tablesIdentified.length - 3} more</Text>
                  )}
                </Space>
              </div>
            )}
          </Space>
        )

      case 'executing':
        const executingData = data as any
        return (
          <Space direction="vertical" size="small">
            {executingData?.estimatedTime && (
              <Text type="secondary">
                Estimated time: {executingData.estimatedTime}ms
              </Text>
            )}
            {executingData?.rowsProcessed !== undefined && (
              <Text type="secondary">
                Rows processed: {executingData.rowsProcessed.toLocaleString()}
              </Text>
            )}
          </Space>
        )

      case 'formatting':
        const formattingData = data as any
        return (
          <Space direction="vertical" size="small">
            <Text type="secondary">
              Rows: {formattingData?.rowsProcessed?.toLocaleString() || 0} / {formattingData?.totalRows?.toLocaleString() || 0}
            </Text>
            {formattingData?.chartsGenerated && (
              <Text type="secondary">
                Charts generated: {formattingData.chartsGenerated}
              </Text>
            )}
          </Space>
        )

      default:
        return null
    }
  }

  if (compact) {
    return (
      <Card size="small" style={{ marginBottom: 16 }}>
        <Space>
          <Spin indicator={<LoadingOutlined style={{ fontSize: 16 }} spin />} />
          <Text>{streamingProgress.message}</Text>
          <Progress
            type="circle"
            size={24}
            percent={streamingProgress.progress}
            strokeColor={getProgressColor(streamingProgress.phase)}
            format={() => ''}
          />
          <Text type="secondary">{formatElapsedTime(elapsedTime)}</Text>
          {onCancel && (
            <Button size="small" icon={<StopOutlined />} onClick={handleCancel}>
              Cancel
            </Button>
          )}
        </Space>
      </Card>
    )
  }

  const phases = ['parsing', 'analyzing', 'executing', 'formatting', 'complete']
  const currentPhaseIndex = phases.indexOf(streamingProgress.phase)

  return (
    <Card
      title={
        <Space>
          <LoadingOutlined spin />
          <Text>Processing Query</Text>
          {!isConnected && (
            <Tag color="red">Disconnected</Tag>
          )}
        </Space>
      }
      extra={
        <Space>
          <Text type="secondary">{formatElapsedTime(elapsedTime)}</Text>
          {onCancel && (
            <Button size="small" icon={<StopOutlined />} onClick={handleCancel}>
              Cancel
            </Button>
          )}
        </Space>
      }
      style={{ marginBottom: 16 }}
    >
      {!isConnected && (
        <Alert
          type="warning"
          message="Connection lost"
          description="Attempting to reconnect..."
          style={{ marginBottom: 16 }}
          showIcon
        />
      )}

      {/* Overall Progress */}
      <div style={{ marginBottom: 16 }}>
        <Progress
          percent={streamingProgress.progress}
          strokeColor={getProgressColor(streamingProgress.phase)}
          status={streamingProgress.phase === 'error' ? 'exception' : 'active'}
        />
        <Text type="secondary" style={{ fontSize: '12px' }}>
          {streamingProgress.message}
        </Text>
      </div>

      {/* Phase Timeline */}
      <Timeline
        size="small"
        items={phases.map((phase, index) => ({
          dot: getPhaseIcon(phase, index <= currentPhaseIndex),
          color: index < currentPhaseIndex ? 'green' : 
                 index === currentPhaseIndex ? 'blue' : 'gray',
          children: (
            <div>
              <Text strong={index === currentPhaseIndex}>
                {getPhaseLabel(phase)}
              </Text>
              {index === currentPhaseIndex && renderPhaseDetails()}
            </div>
          ),
        }))}
      />
    </Card>
  )
}
