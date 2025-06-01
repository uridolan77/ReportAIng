/**
 * Inline Chart Component - Displays charts directly in query results
 */

import React from 'react';
import { Card, Typography, Empty } from 'antd';
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';
import { BarChartOutlined } from '@ant-design/icons';

const { Title, Text } = Typography;

interface InlineChartProps {
  type: 'bar' | 'line' | 'pie';
  data: any[];
  columns: any[];
  title?: string;
  height?: number;
}

export const InlineChart: React.FC<InlineChartProps> = ({
  type,
  data,
  columns,
  title,
  height = 300
}) => {
  // Color palette for charts
  const colors = [
    '#1890ff', '#52c41a', '#fa8c16', '#722ed1', '#eb2f96',
    '#13c2c2', '#faad14', '#a0d911', '#2f54eb', '#f5222d'
  ];

  // Prepare data for charts
  const prepareChartData = () => {
    if (!data || data.length === 0) {
      console.log('InlineChart: No data provided', { data, columns });
      return [];
    }

    console.log('InlineChart: Preparing chart data', {
      dataLength: data.length,
      columns,
      firstRow: data[0],
      dataKeys: Object.keys(data[0] || {})
    });

    // For simple data, try to find numeric and categorical columns
    const numericColumns = columns.filter(col => {
      const value = data[0]?.[col];
      const isNumeric = typeof value === 'number' || !isNaN(Number(value));
      console.log(`Column ${col}: value=${value}, type=${typeof value}, isNumeric=${isNumeric}`);
      return isNumeric;
    });

    const categoricalColumns = columns.filter(col => {
      const value = data[0]?.[col];
      const isCategorical = typeof value === 'string' || (!numericColumns.includes(col) && value !== undefined);
      console.log(`Column ${col}: value=${value}, type=${typeof value}, isCategorical=${isCategorical}`);
      return isCategorical;
    });

    console.log('Column analysis:', { numericColumns, categoricalColumns });

    // Use first categorical column as X-axis and first numeric as Y-axis
    const xColumn = categoricalColumns[0] || columns[0];
    const yColumn = numericColumns[0] || columns[1] || columns[0];

    console.log('Selected columns:', { xColumn, yColumn });

    const chartData = data.slice(0, 20).map((row, index) => {
      const result = {
        name: String(row[xColumn] || `Item ${index + 1}`),
        value: Number(row[yColumn]) || 0,
        ...row
      };
      console.log(`Row ${index}:`, result);
      return result;
    });

    console.log('Final chart data:', chartData);
    return chartData;
  };

  const chartData = prepareChartData();

  // Always show a test chart for debugging
  const testData = [
    { name: 'A', value: 100 },
    { name: 'B', value: 200 },
    { name: 'C', value: 150 },
    { name: 'D', value: 300 }
  ];

  const finalChartData = chartData.length > 0 ? chartData : testData;

  console.log('InlineChart: Final rendering decision', {
    originalDataLength: chartData.length,
    finalDataLength: finalChartData.length,
    usingTestData: chartData.length === 0
  });

  if (!finalChartData || finalChartData.length === 0) {
    return (
      <Card>
        <Empty
          image={<BarChartOutlined style={{ fontSize: 48, color: '#ccc' }} />}
          description="No data available for visualization"
        />
      </Card>
    );
  }

  const renderChart = () => {
    switch (type) {
      case 'bar':
        return (
          <ResponsiveContainer width="100%" height={height}>
            <BarChart data={finalChartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="value" fill={colors[0]}>
                {finalChartData.map((_, index) => (
                  <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer width="100%" height={height}>
            <LineChart data={finalChartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="value"
                stroke={colors[0]}
                strokeWidth={2}
                dot={{ fill: colors[0], strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6 }}
              />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer width="100%" height={height}>
            <PieChart margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <Pie
                data={finalChartData}
                dataKey="value"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={Math.min(height * 0.35, 120)}
                fill={colors[0]}
                label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(1)}%`}
              >
                {finalChartData.map((_, index) => (
                  <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
                ))}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        );

      default:
        return (
          <Empty description={`Chart type "${type}" not supported`} />
        );
    }
  };

  return (
    <Card
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <BarChartOutlined style={{ color: '#1890ff' }} />
          <span>{title || `${type.charAt(0).toUpperCase() + type.slice(1)} Chart`}</span>
        </div>
      }
      style={{ marginTop: '16px' }}
    >
      {renderChart()}
      <div style={{ marginTop: '12px', textAlign: 'center' }}>
        <Text type="secondary" style={{ fontSize: '12px' }}>
          Showing {finalChartData.length} data points
          {chartData.length === 0 && ' (using test data)'}
          {data.length > 20 && ` (limited from ${data.length} total)`}
        </Text>
      </div>
    </Card>
  );
};
