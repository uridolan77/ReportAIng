import React from 'react'
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  Area,
  AreaChart
} from 'recharts'

interface ChartData {
  [key: string]: any
}

interface LineConfig {
  key: string
  color: string
  name: string
  strokeWidth?: number
  strokeDasharray?: string
}

interface PerformanceLineChartProps {
  data: ChartData[]
  xAxisKey: string
  lines: LineConfig[]
  height?: number
  showGrid?: boolean
  showLegend?: boolean
}

export const PerformanceLineChart: React.FC<PerformanceLineChartProps> = ({
  data,
  xAxisKey,
  lines,
  height = 300,
  showGrid = true,
  showLegend = true
}) => {
  return (
    <ResponsiveContainer width="100%" height={height}>
      <LineChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
        {showGrid && <CartesianGrid strokeDasharray="3 3" />}
        <XAxis 
          dataKey={xAxisKey}
          tick={{ fontSize: 12 }}
          tickLine={{ stroke: '#d9d9d9' }}
        />
        <YAxis 
          tick={{ fontSize: 12 }}
          tickLine={{ stroke: '#d9d9d9' }}
        />
        <Tooltip 
          contentStyle={{
            backgroundColor: '#fff',
            border: '1px solid #d9d9d9',
            borderRadius: '6px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
          }}
        />
        {showLegend && <Legend />}
        {lines.map((line) => (
          <Line
            key={line.key}
            type="monotone"
            dataKey={line.key}
            stroke={line.color}
            strokeWidth={line.strokeWidth || 2}
            strokeDasharray={line.strokeDasharray}
            name={line.name}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
          />
        ))}
      </LineChart>
    </ResponsiveContainer>
  )
}

interface PerformanceBarChartProps {
  data: ChartData[]
  xAxisKey: string
  yAxisKey: string
  height?: number
  color?: string
  showGrid?: boolean
}

export const PerformanceBarChart: React.FC<PerformanceBarChartProps> = ({
  data,
  xAxisKey,
  yAxisKey,
  height = 300,
  color = '#1890ff',
  showGrid = true
}) => {
  return (
    <ResponsiveContainer width="100%" height={height}>
      <BarChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
        {showGrid && <CartesianGrid strokeDasharray="3 3" />}
        <XAxis 
          dataKey={xAxisKey}
          tick={{ fontSize: 12 }}
          tickLine={{ stroke: '#d9d9d9' }}
        />
        <YAxis 
          tick={{ fontSize: 12 }}
          tickLine={{ stroke: '#d9d9d9' }}
        />
        <Tooltip 
          contentStyle={{
            backgroundColor: '#fff',
            border: '1px solid #d9d9d9',
            borderRadius: '6px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
          }}
        />
        <Bar 
          dataKey={yAxisKey} 
          fill={color}
          radius={[4, 4, 0, 0]}
        />
      </BarChart>
    </ResponsiveContainer>
  )
}

interface PerformancePieChartProps {
  data: ChartData[]
  height?: number
  colors?: string[]
}

export const PerformancePieChart: React.FC<PerformancePieChartProps> = ({
  data,
  height = 300,
  colors = ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2']
}) => {
  return (
    <ResponsiveContainer width="100%" height={height}>
      <PieChart>
        <Pie
          data={data}
          cx="50%"
          cy="50%"
          labelLine={false}
          label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
          outerRadius={80}
          fill="#8884d8"
          dataKey="value"
        >
          {data.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
          ))}
        </Pie>
        <Tooltip />
      </PieChart>
    </ResponsiveContainer>
  )
}

interface PerformanceAreaChartProps {
  data: ChartData[]
  xAxisKey: string
  areas: LineConfig[]
  height?: number
  showGrid?: boolean
  showLegend?: boolean
}

export const PerformanceAreaChart: React.FC<PerformanceAreaChartProps> = ({
  data,
  xAxisKey,
  areas,
  height = 300,
  showGrid = true,
  showLegend = true
}) => {
  return (
    <ResponsiveContainer width="100%" height={height}>
      <AreaChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
        {showGrid && <CartesianGrid strokeDasharray="3 3" />}
        <XAxis 
          dataKey={xAxisKey}
          tick={{ fontSize: 12 }}
          tickLine={{ stroke: '#d9d9d9' }}
        />
        <YAxis 
          tick={{ fontSize: 12 }}
          tickLine={{ stroke: '#d9d9d9' }}
        />
        <Tooltip 
          contentStyle={{
            backgroundColor: '#fff',
            border: '1px solid #d9d9d9',
            borderRadius: '6px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
          }}
        />
        {showLegend && <Legend />}
        {areas.map((area) => (
          <Area
            key={area.key}
            type="monotone"
            dataKey={area.key}
            stackId="1"
            stroke={area.color}
            fill={area.color}
            fillOpacity={0.6}
            name={area.name}
          />
        ))}
      </AreaChart>
    </ResponsiveContainer>
  )
}

// Metric Card Component
interface MetricCardProps {
  title: string
  value: string | number
  subtitle?: string
  trend?: {
    value: number
    isPositive: boolean
  }
  color?: string
  icon?: React.ReactNode
}

export const MetricCard: React.FC<MetricCardProps> = ({
  title,
  value,
  subtitle,
  trend,
  color = '#1890ff',
  icon
}) => {
  return (
    <div style={{
      padding: '20px',
      background: '#fff',
      borderRadius: '8px',
      border: '1px solid #f0f0f0',
      boxShadow: '0 2px 4px rgba(0,0,0,0.02)'
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div style={{ flex: 1 }}>
          <div style={{ 
            fontSize: '14px', 
            color: '#666', 
            marginBottom: '8px',
            fontWeight: 500
          }}>
            {title}
          </div>
          <div style={{ 
            fontSize: '28px', 
            fontWeight: 'bold', 
            color: color,
            lineHeight: 1,
            marginBottom: '4px'
          }}>
            {value}
          </div>
          {subtitle && (
            <div style={{ fontSize: '12px', color: '#999' }}>
              {subtitle}
            </div>
          )}
          {trend && (
            <div style={{ 
              fontSize: '12px', 
              color: trend.isPositive ? '#52c41a' : '#f5222d',
              marginTop: '4px'
            }}>
              {trend.isPositive ? '↗' : '↘'} {Math.abs(trend.value)}%
            </div>
          )}
        </div>
        {icon && (
          <div style={{ 
            fontSize: '24px', 
            color: color,
            opacity: 0.8
          }}>
            {icon}
          </div>
        )}
      </div>
    </div>
  )
}
