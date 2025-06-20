// features/cost-management/components/CostTrendsChart.tsx
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'

interface CostTrendsChartProps {
  data?: CostTrend[]
  height?: number
}

export const CostTrendsChart: React.FC<CostTrendsChartProps> = ({ 
  data = [], 
  height = 300 
}) => {
  const chartData = data.map(trend => ({
    date: format(new Date(trend.date), 'MMM dd'),
    amount: trend.amount,
    category: trend.category
  }))

  return (
    <Card>
      <CardHeader title="Cost Trends" />
      <CardContent>
        <ResponsiveContainer width="100%" height={height}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis tickFormatter={(value) => `$${value.toFixed(2)}`} />
            <Tooltip 
              formatter={(value: number) => [`$${value.toFixed(2)}`, 'Cost']}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Line 
              type="monotone" 
              dataKey="amount" 
              stroke="#8884d8" 
              strokeWidth={2}
              dot={{ fill: '#8884d8' }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}
