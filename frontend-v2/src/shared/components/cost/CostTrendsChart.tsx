import React from 'react'
import { Card, Empty, Spin } from 'antd'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Area, AreaChart } from 'recharts'
import { format } from 'date-fns'
import type { CostTrend } from '../../types/cost'

interface CostTrendsChartProps {
  data?: CostTrend[]
  height?: number
  loading?: boolean
  showArea?: boolean
}

export const CostTrendsChart: React.FC<CostTrendsChartProps> = ({ 
  data = [], 
  height = 300,
  loading = false,
  showArea = false
}) => {
  if (loading) {
    return (
      <Card title="Cost Trends">
        <div style={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Spin size="large" />
        </div>
      </Card>
    )
  }

  if (!data || data.length === 0) {
    return (
      <Card title="Cost Trends">
        <div style={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Empty description="No cost trend data available" />
        </div>
      </Card>
    )
  }

  const chartData = data.map(trend => ({
    date: format(new Date(trend.date), 'MMM dd'),
    fullDate: trend.date,
    amount: trend.amount,
    category: trend.category,
    formattedAmount: `$${trend.amount.toFixed(2)}`
  }))

  const CustomTooltip = ({ active, payload, label }: any) => {
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
          <p style={{ margin: 0, fontWeight: 'bold' }}>{`Date: ${label}`}</p>
          <p style={{ margin: 0, color: '#1890ff' }}>
            {`Cost: $${payload[0].value.toFixed(2)}`}
          </p>
          {data.category && (
            <p style={{ margin: 0, color: '#666' }}>
              {`Category: ${data.category}`}
            </p>
          )}
        </div>
      )
    }
    return null
  }

  const formatYAxis = (value: number) => {
    if (value >= 1000) {
      return `$${(value / 1000).toFixed(1)}k`
    }
    return `$${value.toFixed(0)}`
  }

  return (
    <Card title="Cost Trends">
      <ResponsiveContainer width="100%" height={height}>
        {showArea ? (
          <AreaChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
            <defs>
              <linearGradient id="costGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#1890ff" stopOpacity={0.8}/>
                <stop offset="95%" stopColor="#1890ff" stopOpacity={0.1}/>
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
            <XAxis 
              dataKey="date" 
              stroke="#666"
              fontSize={12}
            />
            <YAxis 
              tickFormatter={formatYAxis}
              stroke="#666"
              fontSize={12}
            />
            <Tooltip content={<CustomTooltip />} />
            <Area 
              type="monotone" 
              dataKey="amount" 
              stroke="#1890ff" 
              strokeWidth={2}
              fill="url(#costGradient)"
            />
          </AreaChart>
        ) : (
          <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
            <XAxis 
              dataKey="date" 
              stroke="#666"
              fontSize={12}
            />
            <YAxis 
              tickFormatter={formatYAxis}
              stroke="#666"
              fontSize={12}
            />
            <Tooltip content={<CustomTooltip />} />
            <Line 
              type="monotone" 
              dataKey="amount" 
              stroke="#1890ff" 
              strokeWidth={2}
              dot={{ fill: '#1890ff', strokeWidth: 2, r: 4 }}
              activeDot={{ r: 6, stroke: '#1890ff', strokeWidth: 2 }}
            />
          </LineChart>
        )}
      </ResponsiveContainer>
    </Card>
  )
}
