import React, { useState } from 'react'
import {
  Avatar,
  Button,
  Space,
  Typography,
  Tag,
  Tooltip,
  Dropdown,
  Card,
  Collapse,
  Progress,
  Alert
} from 'antd'
import {
  UserOutlined,
  RobotOutlined,
  InfoCircleOutlined,
  CopyOutlined,
  ShareAltOutlined,
  StarOutlined,
  StarFilled,
  MoreOutlined,
  PlayCircleOutlined,
  DownloadOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  BugOutlined
} from '@ant-design/icons'
import { formatDistanceToNow } from 'date-fns'
import { SqlEditor, Chart } from '@shared/components/core'
import { useAppDispatch } from '@shared/hooks'
import { chatActions } from '@shared/store/chat'
import { useToggleMessageFavoriteMutation, useShareMessageMutation } from '@shared/store/api/chatApi'
import type { ChatMessage } from '@shared/types/chat'
import { SemanticAnalysisPanel } from './SemanticAnalysisPanel'
import { MessageActions } from './MessageActions'

const { Text, Paragraph } = Typography


interface MessageItemProps {
  message: ChatMessage
  showMetadata?: boolean
  showTimestamp?: boolean
  onRerun?: (query: string) => void
  onEdit?: (messageId: string) => void
  onShowProcessFlow?: (messageId: string) => void
}

export const MessageItem: React.FC<MessageItemProps> = ({
  message,
  showMetadata = false,
  showTimestamp = true,
  onRerun,
  onEdit,
  onShowProcessFlow,
}) => {
  const dispatch = useAppDispatch()
  const [showDetails, setShowDetails] = useState(false)
  const [toggleFavorite] = useToggleMessageFavoriteMutation()
  const [shareMessage] = useShareMessageMutation()

  const isUser = message.type === 'user'
  const isSystem = message.type === 'system'
  const isError = message.type === 'error'
  const isAssistant = message.type === 'assistant'

  const getMessageStyle = () => {
    const baseStyle = {
      maxWidth: '85%',
      marginBottom: 16,
    }

    if (isUser) {
      return {
        ...baseStyle,
        alignSelf: 'flex-end',
        marginLeft: 'auto',
      }
    }

    return baseStyle
  }

  const getAvatarProps = () => {
    if (isUser) {
      return {
        icon: <UserOutlined />,
        style: { backgroundColor: '#1890ff' },
      }
    }
    
    if (isSystem) {
      return {
        icon: <InfoCircleOutlined />,
        style: { backgroundColor: '#52c41a' },
      }
    }
    
    if (isError) {
      return {
        icon: <BugOutlined />,
        style: { backgroundColor: '#ff4d4f' },
      }
    }
    
    return {
      icon: <RobotOutlined />,
      style: { backgroundColor: '#722ed1' },
    }
  }

  const getMessageBackground = () => {
    if (isUser) return '#e6f7ff'
    if (isSystem) return '#f6ffed'
    if (isError) return '#fff2f0'
    return '#f9f0ff'
  }

  const getBorderColor = () => {
    if (isUser) return '#91d5ff'
    if (isSystem) return '#b7eb8f'
    if (isError) return '#ffccc7'
    return '#d3adf7'
  }

  const handleToggleFavorite = async () => {
    try {
      await toggleFavorite(message.id).unwrap()
      dispatch(chatActions.toggleMessageFavorite(message.id))
    } catch (error) {
      console.error('Failed to toggle favorite:', error)
    }
  }

  const handleShare = async () => {
    try {
      const result = await shareMessage({ messageId: message.id }).unwrap()
      navigator.clipboard.writeText(result.shareUrl)
      // Show success message
    } catch (error) {
      console.error('Failed to share message:', error)
    }
  }

  const handleCopy = () => {
    const textToCopy = message.sql || message.content
    navigator.clipboard.writeText(textToCopy)
  }

  const handleRerun = () => {
    if (message.sql && onRerun) {
      onRerun(message.sql)
    } else if (message.type === 'user' && onRerun) {
      onRerun(message.content)
    }
  }

  const renderStatusIndicator = () => {
    if (message.status === 'sending') {
      return <Progress type="circle" size={16} percent={50} showInfo={false} />
    }
    
    if (message.status === 'error') {
      return <Tag color="red">Error</Tag>
    }
    
    if (message.metadata?.confidence !== undefined) {
      const confidence = message.metadata.confidence
      const color = confidence > 0.8 ? 'green' : confidence > 0.6 ? 'orange' : 'red'
      return (
        <Tooltip title={`Confidence: ${(confidence * 100).toFixed(1)}%`}>
          <Tag color={color}>
            {(confidence * 100).toFixed(0)}%
          </Tag>
        </Tooltip>
      )
    }
    
    return null
  }

  const renderMetadata = () => {
    if (!showMetadata || !message.resultMetadata) return null

    const { executionTime, rowCount, columnCount, queryComplexity } = message.resultMetadata

    return (
      <Space size="small" style={{ fontSize: '12px', color: '#8c8c8c', marginTop: 8 }}>
        <Tooltip title="Execution Time">
          <span>
            <ClockCircleOutlined /> {executionTime}ms
          </span>
        </Tooltip>
        <Tooltip title="Rows Returned">
          <span>
            <DatabaseOutlined /> {rowCount} rows
          </span>
        </Tooltip>
        <span>•</span>
        <span>{columnCount} columns</span>
        <span>•</span>
        <Tag color={queryComplexity === 'Simple' ? 'green' : queryComplexity === 'Medium' ? 'orange' : 'red'}>
          {queryComplexity}
        </Tag>
      </Space>
    )
  }

  const renderContent = () => {
    if (isError && message.error) {
      return (
        <Alert
          message="Query Error"
          description={message.error.message}
          type="error"
          showIcon
          action={
            message.error.retryable ? (
              <Button size="small" onClick={handleRerun}>
                Retry
              </Button>
            ) : undefined
          }
        />
      )
    }

    return (
      <div>
        <Paragraph style={{ margin: 0, whiteSpace: 'pre-wrap' }}>
          {message.content}
        </Paragraph>
        
        {message.sql && (
          <div style={{ marginTop: 12 }}>
            <SqlEditor
              value={message.sql}
              height={150}
              readOnly
              showExecuteButton={!!onRerun}
              showFormatButton={false}
              onExecute={handleRerun}
            />
          </div>
        )}

        {message.results && message.results.length > 0 && (
          <div style={{ marginTop: 12 }}>
            <Chart
              data={message.results}
              columns={Object.keys(message.results[0] || {})}
              height={300}
              config={{
                type: 'bar',
                title: 'Query Results',
                xAxis: Object.keys(message.results[0] || {})[0] || 'x',
                yAxis: Object.keys(message.results[0] || {})[1] || 'y',
              }}
            />
          </div>
        )}

        {message.semanticAnalysis && (
          <Collapse
            ghost
            style={{ marginTop: 12 }}
            items={[
              {
                key: 'semantic',
                label: (
                  <Space>
                    <Text type="secondary">Semantic Analysis</Text>
                    <Tag color="blue">{message.semanticAnalysis.intent}</Tag>
                    <Tag color="green">{(message.semanticAnalysis.confidence * 100).toFixed(0)}%</Tag>
                  </Space>
                ),
                children: <SemanticAnalysisPanel analysis={message.semanticAnalysis} />,
              },
            ]}
          />
        )}

        {renderMetadata()}
      </div>
    )
  }

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: isUser ? 'row-reverse' : 'row',
        alignItems: 'flex-start',
        gap: 12,
        ...getMessageStyle(),
      }}
    >
      <Avatar {...getAvatarProps()} />
      
      <Card
        size="small"
        style={{
          backgroundColor: getMessageBackground(),
          border: `1px solid ${getBorderColor()}`,
          borderRadius: 12,
          minWidth: 200,
          maxWidth: '100%',
        }}
        bodyStyle={{ padding: '12px 16px' }}
        extra={
          <Space>
            {renderStatusIndicator()}
            {showTimestamp && (
              <Text type="secondary" style={{ fontSize: '11px' }}>
                {formatDistanceToNow(new Date(message.timestamp), { addSuffix: true })}
              </Text>
            )}
            {message.isFavorite && (
              <StarFilled style={{ color: '#faad14' }} />
            )}
          </Space>
        }
        actions={[
          <MessageActions
            key="actions"
            message={message}
            onCopy={handleCopy}
            onShare={handleShare}
            onFavorite={handleToggleFavorite}
            onRerun={handleRerun}
            onEdit={onEdit}
            onShowProcessFlow={onShowProcessFlow}
          />
        ]}
      >
        {renderContent()}
      </Card>
    </div>
  )
}
