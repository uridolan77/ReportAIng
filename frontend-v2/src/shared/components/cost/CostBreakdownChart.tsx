import React from 'react'
import { Empty, Spin, List, Progress } from 'antd'
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts'

interface CostBreakdownItem {
  label: string
  value: number
  percentage: number
  trend?: number
}

interface CostBreakdownChartProps {
  data?: CostBreakdownItem[]
  loading?: boolean
  showPieChart?: boolean
  height?: number
}

const COLORS = [
  '#1890ff',
  '#52c41a', 
  '#faad14',
  '#f5222d',
  '#722ed1',
  '#fa8c16',
  '#13c2c2',
  '#eb2f96',
  '#a0d911',
  '#2f54eb'
]

export const CostBreakdownChart: React.FC<CostBreakdownChartProps> = ({ 
  data = [], 
  loading = false,
  showPieChart = true,
  height = 300
}) => {
  if (loading) {
    return (
      <div style={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!data || data.length === 0) {
    return (
      <div style={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Empty description="No breakdown data available" />
      </div>
    )
  }

  const chartData = data.map((item, index) => ({
    ...item,
    color: COLORS[index % COLORS.length]
  }))

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload
      return (
        <div style={{
          backgroundColor: 'white',
          border: '1px solid #d9d9d9',
          borderRadius: '6px',
          padding: '12px',
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)'
        }}>
          <p style={{ margin: 0, fontWeight: 'bold' }}>{data.label}</p>
          <p style={{ margin: 0, color: data.color }}>
            {`Cost: $${data.value.toLocaleString(undefined, { minimumFractionDigits: 2 })}`}
          </p>
          <p style={{ margin: 0, color: '#666' }}>
            {`Percentage: ${data.percentage.toFixed(1)}%`}
          </p>
        </div>
      )
    }
    return null
  }

  if (showPieChart) {
    return (
      <div>
        <ResponsiveContainer width="100%" height={height * 0.7}>
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              innerRadius={40}
              outerRadius={80}
              paddingAngle={2}
              dataKey="value"
            >
              {chartData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
            </Pie>
            <Tooltip content={<CustomTooltip />} />
          </PieChart>
        </ResponsiveContainer>
        
        {/* Legend */}
        <div style={{ marginTop: '16px' }}>
          {chartData.map((item, index) => (
            <div 
              key={index}
              style={{ 
                display: 'flex', 
                alignItems: 'center', 
                marginBottom: '8px',
                fontSize: '12px'
              }}
            >
              <div 
                style={{ 
                  width: '12px', 
                  height: '12px', 
                  backgroundColor: item.color,
                  marginRight: '8px',
                  borderRadius: '2px'
                }} 
              />
              <span style={{ flex: 1 }}>{item.label}</span>
              <span style={{ fontWeight: 'bold' }}>
                ${item.value.toLocaleString(undefined, { minimumFractionDigits: 2 })}
              </span>
            </div>
          ))}
        </div>
      </div>
    )
  }

  // List view with progress bars
  return (
    <List
      size="small"
      dataSource={chartData}
      renderItem={(item, index) => (
        <List.Item style={{ padding: '8px 0' }}>
          <div style={{ width: '100%' }}>
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              marginBottom: '4px',
              fontSize: '12px'
            }}>
              <span>{item.label}</span>
              <span style={{ fontWeight: 'bold' }}>
                ${item.value.toLocaleString(undefined, { minimumFractionDigits: 2 })}
              </span>
            </div>
            <Progress
              percent={item.percentage}
              size="small"
              strokeColor={item.color}
              showInfo={false}
            />
            <div style={{ 
              textAlign: 'right', 
              fontSize: '11px', 
              color: '#666',
              marginTop: '2px'
            }}>
              {item.percentage.toFixed(1)}%
            </div>
          </div>
        </List.Item>
      )}
    />
  )
}
