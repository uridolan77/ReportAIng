import React from 'react';
import { Card, Button, Space, Typography, Divider } from 'antd';
import { BulbOutlined, BarChartOutlined, TableOutlined, UserOutlined } from '@ant-design/icons';

const { Title, Text } = Typography;

interface QuerySuggestionsProps {
  onSuggestionClick: (suggestion: string) => void;
}

export const QuerySuggestions: React.FC<QuerySuggestionsProps> = ({ onSuggestionClick }) => {
  const suggestionCategories = [
    {
      title: 'Revenue Analysis',
      icon: <BarChartOutlined />,
      color: '#1890ff',
      suggestions: [
        'Show me total revenue by month this year',
        'Compare revenue between this year and last year',
        'What are the top 10 products by revenue?',
        'Show revenue trends by region',
      ],
    },
    {
      title: 'Customer Insights',
      icon: <UserOutlined />,
      color: '#52c41a',
      suggestions: [
        'How many new customers did we acquire this month?',
        'Show customer retention rate by cohort',
        'What is the average customer lifetime value?',
        'Which customers have the highest purchase frequency?',
      ],
    },
    {
      title: 'Operational Metrics',
      icon: <TableOutlined />,
      color: '#fa8c16',
      suggestions: [
        'Show me daily active users for the past 30 days',
        'What is our current inventory status?',
        'Display order fulfillment times by region',
        'Show employee productivity metrics',
      ],
    },
  ];

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
