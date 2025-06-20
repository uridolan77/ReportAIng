/**
 * Unified Dashboard Widget Component
 * 
 * Consolidates common patterns from existing dashboard implementations:
 * - Admin Dashboard statistic cards
 * - Cost Dashboard metrics
 * - API Status Dashboard indicators
 * - Real-time Dashboard metrics
 */

import React from 'react'
import { Card, Statistic, Spin, Alert, Typography, Space, Button, Tooltip } from 'antd'
import { ReloadOutlined, InfoCircleOutlined } from '@ant-design/icons'
import type { StatisticProps } from 'antd'

const { Text } = Typography

export interface DashboardWidgetProps {
  /** Widget title */
  title: string
  /** Widget value */
  value: string | number
  /** Loading state */
  loading?: boolean
  /** Error state */
  error?: string | null
  /** Statistic prefix icon */
  prefix?: React.ReactNode
  /** Statistic suffix */
  suffix?: string
  /** Value precision for numbers */
  precision?: number
  /** Custom value formatter */
  formatter?: StatisticProps['formatter']
  /** Value style (color, etc.) */
  valueStyle?: React.CSSProperties
  /** Widget size */
  size?: 'small' | 'default' | 'large'
  /** Additional description */
  description?: string
  /** Tooltip content */
  tooltip?: string
  /** Refresh handler */
  onRefresh?: () => void
  /** Custom actions */
  actions?: React.ReactNode
  /** Card extra content */
  extra?: React.ReactNode
  /** Custom card props */
  cardProps?: React.ComponentProps<typeof Card>
  /** Trend indicator */
  trend?: {
    value: number
    isPositive?: boolean
    suffix?: string
  }
}

export const DashboardWidget: React.FC<DashboardWidgetProps> = ({
  title,
  value,
  loading = false,
  error = null,
  prefix,
  suffix,
  precision,
  formatter,
  valueStyle,
  size = 'default',
  description,
  tooltip,
  onRefresh,
  actions,
  extra,
  cardProps,
  trend,
}) => {
  const cardSize = size === 'small' ? 'small' : 'default'

  // Handle error state
  if (error) {
    return (
      <Card size={cardSize} {...cardProps}>
        <Alert
          message="Error loading data"
          description={error}
          type="error"
          showIcon
          action={
            onRefresh && (
              <Button size="small" icon={<ReloadOutlined />} onClick={onRefresh}>
                Retry
              </Button>
            )
          }
        />
      </Card>
    )
  }

  const titleWithTooltip = tooltip ? (
    <Space>
      {title}
      <Tooltip title={tooltip}>
        <InfoCircleOutlined style={{ color: '#8c8c8c' }} />
      </Tooltip>
    </Space>
  ) : title

  const cardExtra = extra || (onRefresh && (
    <Button
      type="text"
      size="small"
      icon={<ReloadOutlined />}
      onClick={onRefresh}
      loading={loading}
    />
  ))

  return (
    <Card size={cardSize} extra={cardExtra} {...cardProps}>
      <Spin spinning={loading}>
        <Statistic
          title={titleWithTooltip}
          value={value}
          prefix={prefix}
          suffix={suffix}
          precision={precision}
          formatter={formatter}
          valueStyle={valueStyle}
        />
        
        {/* Trend indicator */}
        {trend && (
          <div style={{ marginTop: 8 }}>
            <Text
              type={trend.isPositive ? 'success' : 'danger'}
              style={{ fontSize: '12px' }}
            >
              {trend.isPositive ? '↗' : '↘'} {Math.abs(trend.value)}{trend.suffix}
            </Text>
          </div>
        )}
        
        {/* Description */}
        {description && (
          <div style={{ marginTop: 8 }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {description}
            </Text>
          </div>
        )}
        
        {/* Custom actions */}
        {actions && (
          <div style={{ marginTop: 12 }}>
            {actions}
          </div>
        )}
      </Spin>
    </Card>
  )
}

// Specialized widget variants for common use cases
export const MetricWidget: React.FC<Omit<DashboardWidgetProps, 'size'> & { 
  size?: 'small' | 'default' 
}> = (props) => (
  <DashboardWidget {...props} size={props.size || 'small'} />
)

export const KPIWidget: React.FC<Omit<DashboardWidgetProps, 'size' | 'cardProps'> & {
  highlight?: boolean
}> = ({ highlight, ...props }) => (
  <DashboardWidget
    {...props}
    size="large"
    cardProps={{
      style: {
        background: highlight ? '#f6ffed' : undefined,
        borderColor: highlight ? '#52c41a' : undefined,
      },
    }}
  />
)

export default DashboardWidget
