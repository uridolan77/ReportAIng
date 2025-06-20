import React from 'react'
import { Card, Statistic, Space, Tag, Tooltip, Typography } from 'antd'
import { DollarOutlined, InfoCircleOutlined, TrendingUpOutlined, TrendingDownOutlined } from '@ant-design/icons'
import { useGetRealTimeCostMetricsQuery } from '../../store/api/costApi'

const { Text } = Typography

interface QueryCostWidgetProps {
  queryId?: string
  estimatedCost?: number
  actualCost?: number
  compact?: boolean
  showTrend?: boolean
}

export const QueryCostWidget: React.FC<QueryCostWidgetProps> = ({
  queryId,
  estimatedCost,
  actualCost,
  compact = false,
  showTrend = true
}) => {
  const { data: realTimeMetrics } = useGetRealTimeCostMetricsQuery()

  const getCostColor = (cost: number) => {
    if (cost < 0.01) return '#52c41a' // Green for low cost
    if (cost < 0.1) return '#faad14'  // Orange for medium cost
    return '#f5222d' // Red for high cost
  }

  const formatCost = (cost: number) => {
    if (cost < 0.001) return `$${(cost * 1000).toFixed(3)}k`
    return `$${cost.toFixed(4)}`
  }

  const currentCost = actualCost || estimatedCost || realTimeMetrics?.costPerQuery || 0
  const isEstimate = !actualCost && estimatedCost

  if (compact) {
    return (
      <Space size="small" style={{ fontSize: '12px' }}>
        <DollarOutlined style={{ color: getCostColor(currentCost) }} />
        <Text style={{ color: getCostColor(currentCost), fontWeight: 'bold' }}>
          {formatCost(currentCost)}
        </Text>
        {isEstimate && (
          <Tag size="small" color="blue">Est.</Tag>
        )}
        {showTrend && realTimeMetrics && (
          <Tooltip title={`Efficiency: ${(realTimeMetrics.efficiency * 100).toFixed(1)}%`}>
            {realTimeMetrics.efficiency > 0.8 ? (
              <TrendingDownOutlined style={{ color: '#52c41a' }} />
            ) : (
              <TrendingUpOutlined style={{ color: '#f5222d' }} />
            )}
          </Tooltip>
        )}
      </Space>
    )
  }

  return (
    <Card size="small" style={{ width: '100%' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Statistic
            title={
              <Space size="small">
                <span>Query Cost</span>
                {isEstimate && (
                  <Tooltip title="This is an estimated cost. Actual cost will be shown after query completion.">
                    <InfoCircleOutlined style={{ color: '#1890ff' }} />
                  </Tooltip>
                )}
              </Space>
            }
            value={currentCost}
            precision={4}
            prefix={<DollarOutlined />}
            valueStyle={{ 
              color: getCostColor(currentCost),
              fontSize: '18px'
            }}
          />
          
          {isEstimate && (
            <Tag color="blue" size="small" style={{ marginTop: '4px' }}>
              Estimated
            </Tag>
          )}
        </div>

        {realTimeMetrics && (
          <div style={{ textAlign: 'right' }}>
            <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>
              Avg Cost/Query
            </div>
            <div style={{ 
              fontSize: '14px', 
              fontWeight: 'bold',
              color: getCostColor(realTimeMetrics.costPerQuery)
            }}>
              {formatCost(realTimeMetrics.costPerQuery)}
            </div>
            
            {showTrend && (
              <div style={{ marginTop: '4px' }}>
                <Space size="small">
                  <Text style={{ fontSize: '11px' }}>
                    Efficiency: {(realTimeMetrics.efficiency * 100).toFixed(1)}%
                  </Text>
                  {realTimeMetrics.efficiency > 0.8 ? (
                    <TrendingDownOutlined style={{ color: '#52c41a', fontSize: '12px' }} />
                  ) : (
                    <TrendingUpOutlined style={{ color: '#f5222d', fontSize: '12px' }} />
                  )}
                </Space>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Cost Breakdown */}
      {realTimeMetrics && (
        <div style={{ 
          marginTop: '12px', 
          paddingTop: '8px', 
          borderTop: '1px solid #f0f0f0',
          fontSize: '11px',
          color: '#666'
        }}>
          <Space split={<span>•</span>}>
            <span>Current: {formatCost(realTimeMetrics.currentCost)}</span>
            <span>Per Min: {formatCost(realTimeMetrics.costPerMinute)}</span>
            <span>Active: {realTimeMetrics.activeQueries}</span>
          </Space>
        </div>
      )}
    </Card>
  )
}

// Simplified version for inline display
export const InlineQueryCost: React.FC<{ cost?: number; isEstimate?: boolean }> = ({ 
  cost = 0, 
  isEstimate = false 
}) => {
  const getCostColor = (cost: number) => {
    if (cost < 0.01) return '#52c41a'
    if (cost < 0.1) return '#faad14'
    return '#f5222d'
  }

  const formatCost = (cost: number) => {
    if (cost < 0.001) return `$${(cost * 1000).toFixed(3)}k`
    return `$${cost.toFixed(4)}`
  }

  return (
    <Space size="small" style={{ fontSize: '12px' }}>
      <DollarOutlined style={{ color: getCostColor(cost) }} />
      <Text style={{ color: getCostColor(cost), fontWeight: 'bold' }}>
        {formatCost(cost)}
      </Text>
      {isEstimate && <Tag size="small" color="blue">Est.</Tag>}
    </Space>
  )
}
