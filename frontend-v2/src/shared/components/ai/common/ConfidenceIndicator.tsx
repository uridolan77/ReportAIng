import React from 'react'
import { Progress, Badge, Tooltip, Typography } from 'antd'
import { CheckCircleOutlined, ExclamationCircleOutlined, CloseCircleOutlined } from '@ant-design/icons'
import { useConfidenceThreshold } from './hooks/useConfidenceThreshold'
import type { ConfidenceIndicatorProps } from './types'

const { Text } = Typography

/**
 * ConfidenceIndicator - Displays AI confidence scores with visual indicators
 * 
 * Features:
 * - Multiple display types (circle, bar, badge)
 * - Configurable thresholds and colors
 * - Accessibility support
 * - Responsive design
 * - Tooltip explanations
 */
export const ConfidenceIndicator: React.FC<ConfidenceIndicatorProps> = ({
  confidence,
  threshold,
  showLabel = false,
  showPercentage = true,
  size = 'medium',
  type = 'circle',
  color = 'auto',
  className,
  style,
  loading = false,
  disabled = false,
  testId
}) => {
  const { 
    threshold: defaultThreshold, 
    getConfidenceLevel, 
    getConfidenceColor,
    isAboveThreshold 
  } = useConfidenceThreshold()

  const effectiveThreshold = threshold ?? defaultThreshold
  const confidenceLevel = getConfidenceLevel(confidence)
  const confidenceColor = color === 'auto' ? getConfidenceColor(confidence) : color
  const isHigh = isAboveThreshold(confidence)
  const percentage = Math.round(confidence * 100)

  // Size configurations
  const sizeConfig = {
    small: { 
      circleSize: 32, 
      fontSize: '12px', 
      iconSize: 12,
      barHeight: 6
    },
    medium: { 
      circleSize: 48, 
      fontSize: '14px', 
      iconSize: 16,
      barHeight: 8
    },
    large: { 
      circleSize: 64, 
      fontSize: '16px', 
      iconSize: 20,
      barHeight: 12
    }
  }

  const config = sizeConfig[size]

  // Get status icon
  const getStatusIcon = () => {
    if (confidence >= 0.8) return <CheckCircleOutlined style={{ color: '#52c41a', fontSize: config.iconSize }} />
    if (confidence >= 0.6) return <ExclamationCircleOutlined style={{ color: '#faad14', fontSize: config.iconSize }} />
    return <CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: config.iconSize }} />
  }

  // Get confidence description
  const getConfidenceDescription = () => {
    const descriptions = {
      high: 'High confidence - AI is very certain about this result',
      medium: 'Medium confidence - AI has reasonable certainty',
      low: 'Low confidence - AI result should be verified'
    }
    return descriptions[confidenceLevel]
  }

  // Render circle type
  if (type === 'circle') {
    return (
      <Tooltip title={getConfidenceDescription()}>
        <div 
          className={className}
          style={style}
          data-testid={testId}
          data-confidence={confidence}
          data-level={confidenceLevel}
        >
          <Progress
            type="circle"
            percent={percentage}
            size={config.circleSize}
            strokeColor={confidenceColor}
            format={() => showPercentage ? `${percentage}%` : ''}
            style={{ 
              opacity: disabled ? 0.6 : 1,
              fontSize: config.fontSize
            }}
          />
          {showLabel && (
            <div style={{ 
              textAlign: 'center', 
              marginTop: 4,
              fontSize: config.fontSize
            }}>
              <Text type="secondary">
                {confidenceLevel.charAt(0).toUpperCase() + confidenceLevel.slice(1)} Confidence
              </Text>
            </div>
          )}
        </div>
      </Tooltip>
    )
  }

  // Render bar type
  if (type === 'bar') {
    return (
      <Tooltip title={getConfidenceDescription()}>
        <div 
          className={className}
          style={{ ...style, minWidth: 120 }}
          data-testid={testId}
          data-confidence={confidence}
          data-level={confidenceLevel}
        >
          {showLabel && (
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center',
              marginBottom: 4,
              fontSize: config.fontSize
            }}>
              <Text strong>Confidence</Text>
              {showPercentage && <Text>{percentage}%</Text>}
            </div>
          )}
          <Progress
            percent={percentage}
            strokeColor={confidenceColor}
            strokeWidth={config.barHeight}
            showInfo={!showLabel && showPercentage}
            style={{ 
              opacity: disabled ? 0.6 : 1
            }}
          />
          {showLabel && (
            <div style={{ 
              marginTop: 4,
              fontSize: config.fontSize
            }}>
              <Text type="secondary">{confidenceLevel.charAt(0).toUpperCase() + confidenceLevel.slice(1)}</Text>
            </div>
          )}
        </div>
      </Tooltip>
    )
  }

  // Render badge type
  if (type === 'badge') {
    return (
      <Tooltip title={getConfidenceDescription()}>
        <Badge
          count={showPercentage ? `${percentage}%` : confidenceLevel.toUpperCase()}
          style={{
            backgroundColor: confidenceColor,
            fontSize: config.fontSize,
            opacity: disabled ? 0.6 : 1
          }}
          className={className}
          data-testid={testId}
          data-confidence={confidence}
          data-level={confidenceLevel}
        >
          {showLabel && (
            <div style={{ 
              display: 'flex', 
              alignItems: 'center', 
              gap: 4,
              fontSize: config.fontSize
            }}>
              {getStatusIcon()}
              <Text>Confidence</Text>
            </div>
          )}
        </Badge>
      </Tooltip>
    )
  }

  return null
}

/**
 * Compact confidence indicator for inline use
 */
export const CompactConfidenceIndicator: React.FC<Omit<ConfidenceIndicatorProps, 'size' | 'type'>> = (props) => (
  <ConfidenceIndicator {...props} size="small" type="badge" showLabel={false} />
)

/**
 * Detailed confidence indicator with full information
 */
export const DetailedConfidenceIndicator: React.FC<Omit<ConfidenceIndicatorProps, 'size' | 'type'>> = (props) => (
  <ConfidenceIndicator {...props} size="large" type="circle" showLabel={true} />
)

export default ConfidenceIndicator
