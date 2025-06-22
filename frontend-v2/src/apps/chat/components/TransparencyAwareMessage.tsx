import React, { useState } from 'react'
import { Card, Space, Typography, Button, Collapse, Tag, Tooltip, Progress } from 'antd'
import {
  EyeOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  TrophyOutlined,
  InfoCircleOutlined,
  DownOutlined,
  UpOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { QueryFlowAnalyzer } from '@shared/components/ai/transparency/QueryFlowAnalyzer'
import type { ChatMessage } from '@shared/types/chat'
import type { TransparencyTrace } from '@shared/types/transparency'

const { Text, Paragraph } = Typography
const { Panel } = Collapse

export interface TransparencyAwareMessageProps {
  message: ChatMessage
  transparencyData?: TransparencyTrace
  showTransparencyInline?: boolean
  onViewFullTransparency?: (traceId: string) => void
  className?: string
}

/**
 * TransparencyAwareMessage - Chat message with integrated transparency information
 * 
 * Features:
 * - Inline transparency metrics
 * - Expandable transparency details
 * - Quick confidence indicators
 * - Performance metrics display
 * - Link to full transparency view
 */
export const TransparencyAwareMessage: React.FC<TransparencyAwareMessageProps> = ({
  message,
  transparencyData,
  showTransparencyInline = false,
  onViewFullTransparency,
  className = ''
}) => {
  const [showDetails, setShowDetails] = useState(false)

  const renderTransparencyBadge = () => {
    if (!transparencyData) return null

    return (
      <Space size="small">
        <ConfidenceIndicator
          confidence={transparencyData.overallConfidence}
          size="small"
          type="badge"
        />
        <Tooltip title={`Processing time: ${transparencyData.steps?.reduce((sum, step) => sum + step.processingTimeMs, 0) || 0}ms`}>
          <Tag size="small" icon={<ClockCircleOutlined />}>
            {transparencyData.steps?.reduce((sum, step) => sum + step.processingTimeMs, 0) || 0}ms
          </Tag>
        </Tooltip>
        <Tooltip title={`Total tokens: ${transparencyData.steps?.reduce((sum, step) => sum + step.tokensAdded, 0) || 0}`}>
          <Tag size="small" icon={<BarChartOutlined />}>
            {transparencyData.steps?.reduce((sum, step) => sum + step.tokensAdded, 0) || 0}t
          </Tag>
        </Tooltip>
        <Tooltip title="View full transparency details">
          <Button
            type="text"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => onViewFullTransparency?.(transparencyData.traceId)}
          />
        </Tooltip>
      </Space>
    )
  }

  const renderInlineTransparency = () => {
    if (!showTransparencyInline || !transparencyData) return null

    const totalSteps = transparencyData.steps?.length || 0
    const successfulSteps = transparencyData.steps?.filter(step => step.success).length || 0
    const totalTime = transparencyData.steps?.reduce((sum, step) => sum + step.processingTimeMs, 0) || 0
    const totalTokens = transparencyData.steps?.reduce((sum, step) => sum + step.tokensAdded, 0) || 0

    return (
      <Card 
        size="small" 
        style={{ 
          marginTop: '8px', 
          background: '#fafafa',
          border: '1px solid #e8e8e8'
        }}
      >
        <Collapse 
          ghost 
          size="small"
          expandIcon={({ isActive }) => isActive ? <UpOutlined /> : <DownOutlined />}
        >
          <Panel 
            header={
              <Space>
                <EyeOutlined style={{ color: '#1890ff' }} />
                <Text strong style={{ fontSize: '12px' }}>Transparency Details</Text>
                <Tag size="small" color="blue">{totalSteps} steps</Tag>
                <Tag size="small" color={transparencyData.success ? 'green' : 'red'}>
                  {transparencyData.success ? 'Success' : 'Failed'}
                </Tag>
              </Space>
            } 
            key="transparency"
          >
            <Space direction="vertical" style={{ width: '100%' }}>
              {/* Quick Metrics */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: '8px' }}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#1890ff' }}>
                    {(transparencyData.overallConfidence * 100).toFixed(0)}%
                  </div>
                  <Text type="secondary" style={{ fontSize: '10px' }}>Confidence</Text>
                </div>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#52c41a' }}>
                    {successfulSteps}/{totalSteps}
                  </div>
                  <Text type="secondary" style={{ fontSize: '10px' }}>Success Rate</Text>
                </div>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#faad14' }}>
                    {totalTime}ms
                  </div>
                  <Text type="secondary" style={{ fontSize: '10px' }}>Total Time</Text>
                </div>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#722ed1' }}>
                    {totalTokens}
                  </div>
                  <Text type="secondary" style={{ fontSize: '10px' }}>Tokens</Text>
                </div>
              </div>

              {/* Step Progress */}
              <div>
                <Text strong style={{ fontSize: '12px' }}>Processing Steps:</Text>
                <div style={{ marginTop: '4px' }}>
                  {transparencyData.steps?.map((step, index) => (
                    <div key={step.id} style={{ 
                      display: 'flex', 
                      justifyContent: 'space-between', 
                      alignItems: 'center',
                      padding: '2px 0',
                      fontSize: '11px'
                    }}>
                      <Space size="small">
                        <span style={{ 
                          width: '8px', 
                          height: '8px', 
                          borderRadius: '50%', 
                          backgroundColor: step.success ? '#52c41a' : '#ff4d4f',
                          display: 'inline-block'
                        }} />
                        <Text style={{ fontSize: '11px' }}>{step.stepName}</Text>
                      </Space>
                      <Space size="small">
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          {step.processingTimeMs}ms
                        </Text>
                        <ConfidenceIndicator
                          confidence={step.confidence}
                          size="small"
                          type="badge"
                        />
                      </Space>
                    </div>
                  )) || []}
                </div>
              </div>

              {/* Business Context */}
              {transparencyData.businessContext && (
                <div>
                  <Text strong style={{ fontSize: '12px' }}>Business Context:</Text>
                  <div style={{ marginTop: '4px' }}>
                    <Text style={{ fontSize: '11px' }}>
                      Domain: {transparencyData.businessContext.domain || 'Not specified'}
                    </Text>
                    {transparencyData.businessContext.entities && transparencyData.businessContext.entities.length > 0 && (
                      <div style={{ marginTop: '2px' }}>
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          Entities: {transparencyData.businessContext.entities.slice(0, 3).join(', ')}
                          {transparencyData.businessContext.entities.length > 3 && '...'}
                        </Text>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* Actions */}
              <div style={{ textAlign: 'center', marginTop: '8px' }}>
                <Button
                  type="link"
                  size="small"
                  icon={<EyeOutlined />}
                  onClick={() => onViewFullTransparency?.(transparencyData.traceId)}
                  style={{ fontSize: '11px' }}
                >
                  View Full Analysis
                </Button>
              </div>
            </Space>
          </Panel>
        </Collapse>
      </Card>
    )
  }

  const renderMessageContent = () => {
    return (
      <div>
        {/* Original message content */}
        <Paragraph style={{ margin: 0 }}>
          {message.content}
        </Paragraph>

        {/* SQL and Results if available */}
        {message.sql && (
          <Card size="small" style={{ marginTop: '8px' }}>
            <Text code style={{ fontSize: '12px' }}>
              {message.sql}
            </Text>
          </Card>
        )}

        {/* Results if available */}
        {message.results && (
          <Card size="small" style={{ marginTop: '8px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {Array.isArray(message.results) ? `${message.results.length} results` : 'Results available'}
            </Text>
          </Card>
        )}
      </div>
    )
  }

  return (
    <div className={`transparency-aware-message ${className}`}>
      {/* Message Header with Transparency Badge */}
      {message.type === 'assistant' && transparencyData && (
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center',
          marginBottom: '8px'
        }}>
          <Space>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              AI Response
            </Text>
            {transparencyData.intentType && (
              <Tag size="small" color="blue">
                {transparencyData.intentType}
              </Tag>
            )}
          </Space>
          {renderTransparencyBadge()}
        </div>
      )}

      {/* Message Content */}
      {renderMessageContent()}

      {/* Inline Transparency Details */}
      {renderInlineTransparency()}

      {/* Transparency Issues Alert */}
      {transparencyData && !transparencyData.success && (
        <Card 
          size="small" 
          style={{ 
            marginTop: '8px',
            borderColor: '#ff4d4f',
            background: '#fff2f0'
          }}
        >
          <Space>
            <InfoCircleOutlined style={{ color: '#ff4d4f' }} />
            <Text style={{ fontSize: '12px', color: '#ff4d4f' }}>
              This response had processing issues. 
              <Button 
                type="link" 
                size="small" 
                onClick={() => onViewFullTransparency?.(transparencyData.traceId)}
                style={{ padding: 0, fontSize: '12px' }}
              >
                View details
              </Button>
            </Text>
          </Space>
        </Card>
      )}

      {/* Low Confidence Warning */}
      {transparencyData && transparencyData.overallConfidence < 0.7 && (
        <Card 
          size="small" 
          style={{ 
            marginTop: '8px',
            borderColor: '#faad14',
            background: '#fffbe6'
          }}
        >
          <Space>
            <InfoCircleOutlined style={{ color: '#faad14' }} />
            <Text style={{ fontSize: '12px', color: '#faad14' }}>
              Lower confidence response. Consider reviewing the analysis.
            </Text>
          </Space>
        </Card>
      )}
    </div>
  )
}

export default TransparencyAwareMessage
