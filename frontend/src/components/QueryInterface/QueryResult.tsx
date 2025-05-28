import React from 'react';
import { Card, Table, Typography, Space, Tag, Button } from 'antd';
import { ReloadOutlined, CodeOutlined } from '@ant-design/icons';
import { QueryResponse } from '../../types/query';

const { Title, Text, Paragraph } = Typography;

interface QueryResultProps {
  result: QueryResponse;
  query: string;
  onRequery: () => void;
  onSuggestionClick?: (suggestion: string) => void;
}

export const QueryResult: React.FC<QueryResultProps> = ({ result, query, onRequery, onSuggestionClick }) => {
  if (!result.success) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Title level={4} type="danger">Query Failed</Title>
          <Text type="danger">{typeof result.error === 'string' ? result.error : result.error?.message || 'An error occurred'}</Text>
          <div style={{ marginTop: '16px' }}>
            <Button icon={<ReloadOutlined />} onClick={onRequery}>
              Try Again
            </Button>
          </div>
        </div>
      </Card>
    );
  }

  const columns = result.result?.metadata.columns.map(col => ({
    title: col.name,
    dataIndex: col.name,
    key: col.name,
    render: (value: any) => {
      if (value === null || value === undefined) {
        return <Text type="secondary">NULL</Text>;
      }
      return String(value);
    },
  })) || [];

  const dataSource = result.result?.data.map((row, index) => ({
    ...row,
    key: index,
  })) || [];

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      {/* Query Info */}
      <Card size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Title level={5} style={{ margin: 0 }}>Query Results</Title>
            <Space>
              <Tag color={result.confidence > 0.8 ? 'green' : result.confidence > 0.6 ? 'orange' : 'red'}>
                Confidence: {(result.confidence * 100).toFixed(0)}%
              </Tag>
              <Tag color="blue">
                {result.executionTimeMs}ms
              </Tag>
              {result.cached && <Tag color="purple">Cached</Tag>}
            </Space>
          </div>

          <Text type="secondary">
            <strong>Question:</strong> {query}
          </Text>

          <details>
            <summary style={{ cursor: 'pointer', color: '#1890ff' }}>
              <CodeOutlined /> View Generated SQL
            </summary>
            <Paragraph
              code
              copyable
              style={{
                marginTop: '8px',
                background: '#f6f8fa',
                padding: '12px',
                borderRadius: '6px',
                fontSize: '12px'
              }}
            >
              {result.sql}
            </Paragraph>
          </details>
        </Space>
      </Card>

      {/* Results Table */}
      <Card>
        <Table
          columns={columns}
          dataSource={dataSource}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} items`,
          }}
          scroll={{ x: true }}
          size="small"
        />
      </Card>

      {/* Suggestions */}
      {result.suggestions && result.suggestions.length > 0 && (
        <Card title="Suggested Follow-up Queries" size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            {result.suggestions.map((suggestion, index) => (
              <Button
                key={index}
                type="link"
                style={{ textAlign: 'left', padding: '4px 0', height: 'auto' }}
                onClick={() => {
                  if (onSuggestionClick) {
                    onSuggestionClick(suggestion);
                  }
                }}
              >
                {suggestion}
              </Button>
            ))}
          </Space>
        </Card>
      )}
    </Space>
  );
};
