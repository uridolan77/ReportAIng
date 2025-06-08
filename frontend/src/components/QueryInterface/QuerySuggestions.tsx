import React from 'react';
import { Card, Button, Space, Typography, Divider } from 'antd';
import { BulbOutlined, BarChartOutlined, TableOutlined, UserOutlined } from '@ant-design/icons';

const { Title, Text } = Typography;

interface QuerySuggestionsProps {
  onSuggestionClick: (suggestion: string) => void;
}

// Export the suggestion categories for use in database sync
export const suggestionCategories = [
  {
    title: 'Revenue Analysis',
    icon: <BarChartOutlined />,
    color: '#1890ff',
    categoryKey: 'revenue-analysis',
    suggestions: [
      'Show me total deposits for yesterday',
      'Show me daily revenue for the last week',
      'Top 10 players by deposits in the last 7 days',
      'Revenue breakdown by country for last week',
    ],
  },
  {
    title: 'Customer Insights',
    icon: <UserOutlined />,
    color: '#52c41a',
    categoryKey: 'customer-insights',
    suggestions: [
      'Count of active players yesterday',
      'Show me new player registrations for the last 7 days',
      'Top 10 players by total bets in the last week',
      'Show me player activity for the last 3 days',
    ],
  },
  {
    title: 'Operational Metrics',
    icon: <TableOutlined />,
    color: '#fa8c16',
    categoryKey: 'operational-metrics',
    suggestions: [
      'Show me casino vs sports betting revenue for last week',
      'Total bets and wins for yesterday',
      'Show me bonus usage for the last 7 days',
      'Daily transaction volume for the last week',
    ],
  },
];

export const QuerySuggestions: React.FC<QuerySuggestionsProps> = ({ onSuggestionClick }) => {

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <BulbOutlined style={{ fontSize: '32px', color: '#faad14', marginBottom: '8px' }} />
        <Title level={4}>Query Suggestions</Title>
        <Text type="secondary">
          Click on any suggestion below to get started with your analysis
        </Text>
      </div>

      {suggestionCategories.map((category, categoryIndex) => (
        <Card
          key={categoryIndex}
          title={
            <Space>
              <span style={{ color: category.color }}>{category.icon}</span>
              {category.title}
            </Space>
          }
          size="small"
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {category.suggestions.map((suggestion, index) => (
              <Button
                key={index}
                type="text"
                style={{
                  textAlign: 'left',
                  height: 'auto',
                  padding: '8px 12px',
                  whiteSpace: 'normal',
                  wordWrap: 'break-word'
                }}
                onClick={() => onSuggestionClick(suggestion)}
                block
              >
                {suggestion}
              </Button>
            ))}
          </Space>
        </Card>
      ))}

      <Divider />

      <Card size="small" style={{ background: '#f6ffed', border: '1px solid #b7eb8f' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text strong style={{ color: '#389e0d' }}>
            💡 Pro Tips:
          </Text>
          <ul style={{ margin: 0, paddingLeft: '20px' }}>
            <li>
              <Text>Be specific about time ranges (e.g., "last 30 days", "this quarter")</Text>
            </li>
            <li>
              <Text>Include comparison criteria (e.g., "compared to last year")</Text>
            </li>
            <li>
              <Text>Ask for specific metrics (e.g., "revenue", "count", "average")</Text>
            </li>
            <li>
              <Text>Specify grouping dimensions (e.g., "by region", "by product category")</Text>
            </li>
          </ul>
        </Space>
      </Card>
    </Space>
  );
};
