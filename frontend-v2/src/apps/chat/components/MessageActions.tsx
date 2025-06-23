import React from 'react'
import { Button, Space, Tooltip, Dropdown, message as antMessage } from 'antd'
import {
  CopyOutlined,
  ShareAltOutlined,
  StarOutlined,
  StarFilled,
  PlayCircleOutlined,
  EditOutlined,
  DownloadOutlined,
  MoreOutlined,
  DeleteOutlined,
  FlagOutlined,
  EyeOutlined,
  ApiOutlined
} from '@ant-design/icons'
import type { ChatMessage } from '@shared/types/chat'

interface MessageActionsProps {
  message: ChatMessage
  onCopy?: () => void
  onShare?: () => void
  onFavorite?: () => void
  onRerun?: () => void
  onEdit?: (messageId: string) => void
  onDelete?: (messageId: string) => void
  onReport?: (messageId: string) => void
  onExport?: (messageId: string) => void
  onShowProcessFlow?: (messageId: string) => void
}

export const MessageActions: React.FC<MessageActionsProps> = ({
  message,
  onCopy,
  onShare,
  onFavorite,
  onRerun,
  onEdit,
  onDelete,
  onReport,
  onExport,
  onShowProcessFlow,
}) => {
  const isUser = message.type === 'user'
  const isAssistant = message.type === 'assistant'
  const hasResults = message.results && message.results.length > 0
  const hasSQL = !!message.sql

  const handleCopy = () => {
    onCopy?.()
    antMessage.success('Copied to clipboard')
  }

  const handleShare = () => {
    onShare?.()
    antMessage.success('Share link copied to clipboard')
  }

  const handleFavorite = () => {
    onFavorite?.()
    antMessage.success(message.isFavorite ? 'Removed from favorites' : 'Added to favorites')
  }

  const primaryActions = []
  const secondaryActions = []

  // Copy action - always available
  primaryActions.push(
    <Tooltip key="copy" title={hasSQL ? "Copy SQL" : "Copy message"}>
      <Button
        type="text"
        size="small"
        icon={<CopyOutlined />}
        onClick={handleCopy}
      />
    </Tooltip>
  )

  // Favorite action - always available
  primaryActions.push(
    <Tooltip key="favorite" title={message.isFavorite ? "Remove from favorites" : "Add to favorites"}>
      <Button
        type="text"
        size="small"
        icon={message.isFavorite ? <StarFilled style={{ color: '#faad14' }} /> : <StarOutlined />}
        onClick={handleFavorite}
      />
    </Tooltip>
  )

  // Rerun action - for user messages or assistant messages with SQL
  if ((isUser || (isAssistant && hasSQL)) && onRerun) {
    primaryActions.push(
      <Tooltip key="rerun" title={isUser ? "Ask again" : "Re-execute query"}>
        <Button
          type="text"
          size="small"
          icon={<PlayCircleOutlined />}
          onClick={onRerun}
        />
      </Tooltip>
    )
  }

  // Process Flow action - for assistant messages (AI responses)
  if (isAssistant && onShowProcessFlow) {
    primaryActions.push(
      <Tooltip key="process-flow" title="View AI process flow and transparency">
        <Button
          type="text"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => onShowProcessFlow(message.id)}
        />
      </Tooltip>
    )
  }

  // Share action
  if (onShare) {
    secondaryActions.push({
      key: 'share',
      icon: <ShareAltOutlined />,
      label: 'Share',
      onClick: handleShare,
    })
  }

  // Edit action - for user messages
  if (isUser && onEdit) {
    secondaryActions.push({
      key: 'edit',
      icon: <EditOutlined />,
      label: 'Edit',
      onClick: () => onEdit(message.id),
    })
  }

  // Export action - for messages with results
  if (hasResults && onExport) {
    secondaryActions.push({
      key: 'export',
      icon: <DownloadOutlined />,
      label: 'Export Results',
      onClick: () => onExport(message.id),
    })
  }

  // Delete action
  if (onDelete) {
    secondaryActions.push({
      type: 'divider' as const,
    })
    secondaryActions.push({
      key: 'delete',
      icon: <DeleteOutlined />,
      label: 'Delete',
      danger: true,
      onClick: () => onDelete(message.id),
    })
  }

  // Report action - for assistant messages
  if (isAssistant && onReport) {
    secondaryActions.push({
      key: 'report',
      icon: <FlagOutlined />,
      label: 'Report Issue',
      onClick: () => onReport(message.id),
    })
  }

  return (
    <Space size="small">
      {primaryActions}
      
      {secondaryActions.length > 0 && (
        <Dropdown
          menu={{ items: secondaryActions }}
          placement="topRight"
          trigger={['click']}
        >
          <Button
            type="text"
            size="small"
            icon={<MoreOutlined />}
          />
        </Dropdown>
      )}
    </Space>
  )
}
