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

const { Text } = Typography;

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
      console.log('🔍 InlineChart: No data provided', { data, columns });
      return [];
    }

    console.log('🔍 InlineChart: Starting chart data preparation', {
      dataLength: data.length,
      columns,
      firstRow: data[0],
      dataKeys: Object.keys(data[0] || {}),
      allDataSample: data.slice(0, 3) // Show first 3 rows for debugging
    });

    // For simple data, try to find numeric and categorical columns
    const numericColumns = columns.filter(col => {
      const value = data[0]?.[col];
      const isNumeric = typeof value === 'number' || !isNaN(Number(value));
      console.log(`🔍 Column analysis - ${col}: value=${value}, type=${typeof value}, isNumeric=${isNumeric}`);
      return isNumeric;
    });

    const categoricalColumns = columns.filter(col => {
      const value = data[0]?.[col];
      const isCategorical = typeof value === 'string' || (!numericColumns.includes(col) && value !== undefined);
      console.log(`🔍 Column analysis - ${col}: value=${value}, type=${typeof value}, isCategorical=${isCategorical}`);
      return isCategorical;
    });

    console.log('🔍 InlineChart: Column categorization:', {
      numericColumns,
      categoricalColumns,
      totalColumns: columns.length
    });

    // Use first categorical column as X-axis and first numeric as Y-axis
    const xColumn = categoricalColumns[0] || columns[0];
    const yColumn = numericColumns[0] || columns[1] || columns[0];

    console.log('🔍 InlineChart: Selected axis columns:', {
      xColumn,
      yColumn,
      xColumnExists: data[0]?.hasOwnProperty(xColumn),
      yColumnExists: data[0]?.hasOwnProperty(yColumn)
    });

    // CRITICAL FIX: Don't arbitrarily limit to 20 rows - this causes charts to not reflect actual results
    // Only limit for very large datasets and make it configurable
    const maxRows = 100; // Increased from 20 to 100 for better representation
    const shouldLimit = data.length > maxRows;
    const dataToProcess = shouldLimit ? data.slice(0, maxRows) : data;

    if (shouldLimit) {
      console.warn(`⚠️ InlineChart: Data limited to ${maxRows} rows (original: ${data.length}) - this may not reflect full results`);
    } else {
      console.log(`✅ InlineChart: Processing full dataset (${dataToProcess.length} rows)`);
    }

    const chartData = dataToProcess.map((row, index) => {
      const xValue = row[xColumn];
      const yValue = row[yColumn];

      const result = {
        name: String(xValue || `Item ${index + 1}`),
        value: Number(yValue) || 0,
        originalXValue: xValue,
        originalYValue: yValue,
        rowIndex: index,
        ...row
      };

      if (index < 5) { // Log first 5 rows for debugging
        console.log(`🔍 InlineChart Row ${index}:`, {
          original: { [xColumn]: xValue, [yColumn]: yValue },
          processed: { name: result.name, value: result.value },
          fullRow: result
        });
      }

      return result;
    });

    console.log('✅ InlineChart: Final chart data prepared:', {
      chartDataLength: chartData.length,
      originalDataLength: data.length,
      firstChartRow: chartData[0],
      lastChartRow: chartData[chartData.length - 1],
      dataLimitationApplied: data.length > 20
    });

    return chartData;
  };

  const chartData = prepareChartData();

  // Don't render chart if no data
  if (chartData.length === 0) {
    return (
      <div className="text-center text-gray-500 py-4">
        No data available for visualization
      </div>
    );
  }

  console.log('InlineChart: Rendering chart', {
    dataLength: chartData.length,
    type,
    data: chartData
  });

  const renderChart = () => {
    switch (type) {
      case 'bar':
        return (
          <ResponsiveContainer width="100%" height={height}>
            <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="value" fill={colors[0]}>
                {chartData.map((_, index) => (
                  <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer width="100%" height={height}>
            <LineChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
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
                data={chartData}
                dataKey="value"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={Math.min(height * 0.35, 120)}
                fill={colors[0]}
                label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(1)}%`}
              >
                {chartData.map((_, index) => (
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
          Showing {chartData.length} data points
          {data.length > 100 && ` (limited from ${data.length} total for performance)`}
        </Text>
      </div>
    </Card>
  );
};
