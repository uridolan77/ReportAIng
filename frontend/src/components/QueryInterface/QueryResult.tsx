import React, { useState } from 'react';
import { Card, Table, Typography, Space, Tag, Button, Divider, Row, Col, Tabs } from 'antd';
import {
  ReloadOutlined,
  CodeOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DashboardOutlined,
  DownloadOutlined,
  TableOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { QueryResponse } from '../../types/query';
import { InlineChart } from '../Visualization/InlineChart';
import AdvancedVisualizationPanel from '../Visualization/AdvancedVisualizationPanel';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

interface QueryResultProps {
  result: QueryResponse;
  query: string;
  onRequery: () => void;
  onSuggestionClick?: (suggestion: string) => void;
  onVisualizationRequest?: (type: string, data: any[], columns: any[]) => void;
}

export const QueryResult: React.FC<QueryResultProps> = ({ result, query, onRequery, onSuggestionClick, onVisualizationRequest }) => {
  const [showVisualizationOptions, setShowVisualizationOptions] = useState(false);
  const [selectedChartType, setSelectedChartType] = useState<'bar' | 'line' | 'pie' | null>(null);
  const [activeTab, setActiveTab] = useState('data');
  const [advancedVisualizationConfig, setAdvancedVisualizationConfig] = useState<any>(null);
  if (!result.success) {
    return (
      <Card className="enhanced-card">
        <div className="query-error">
          <Title level={4} style={{ color: 'white', marginBottom: '16px' }}>
            Query Failed
          </Title>
          <Text style={{ color: 'white', fontSize: '16px', display: 'block', marginBottom: '24px' }}>
            {typeof result.error === 'string' ? result.error : result.error?.message || 'An error occurred while processing your query. Please check your query and try again.'}
          </Text>
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={onRequery}
              style={{
                background: 'rgba(255, 255, 255, 0.2)',
                border: '1px solid rgba(255, 255, 255, 0.3)',
                color: 'white',
                borderRadius: '8px'
              }}
            >
              Try Again
            </Button>
            <Button
              type="text"
              style={{
                color: 'rgba(255, 255, 255, 0.8)',
                textDecoration: 'underline'
              }}
              onClick={() => {
                // Could open help modal or documentation
                console.log('Help requested');
              }}
            >
              Need Help?
            </Button>
          </Space>
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
      {/* Enhanced Query Info */}
      <Card className="enhanced-card" size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: '12px'
          }}>
            <Title level={5} style={{ margin: 0, color: '#667eea' }}>
              Query Results
            </Title>
            <Space wrap>
              <Tag
                color={result.confidence > 0.8 ? 'green' : result.confidence > 0.6 ? 'orange' : 'red'}
                style={{ borderRadius: '6px', fontWeight: 500 }}
              >
                Confidence: {(result.confidence * 100).toFixed(0)}%
              </Tag>
              <Tag color="blue" style={{ borderRadius: '6px', fontWeight: 500 }}>
                {result.executionTimeMs}ms
              </Tag>
              {result.cached && (
                <Tag color="purple" style={{ borderRadius: '6px', fontWeight: 500 }}>
                  Cached
                </Tag>
              )}
            </Space>
          </div>

          <div style={{
            background: 'linear-gradient(135deg, #f8f9ff 0%, #e8f4fd 100%)',
            padding: '12px',
            borderRadius: '8px',
            border: '1px solid #e8f4fd'
          }}>
            <Text style={{ fontSize: '14px', color: '#595959' }}>
              <strong style={{ color: '#667eea' }}>Question:</strong> {query}
            </Text>
          </div>

          <details style={{ marginTop: '8px' }}>
            <summary style={{
              cursor: 'pointer',
              color: '#667eea',
              fontWeight: 500,
              padding: '8px',
              borderRadius: '6px',
              background: '#f8f9ff',
              border: '1px solid #e8f4fd'
            }}>
              <CodeOutlined style={{ marginRight: '8px' }} /> View Generated SQL
            </summary>
            <Paragraph
              code
              copyable
              style={{
                marginTop: '12px',
                background: '#f6f8fa',
                padding: '16px',
                borderRadius: '8px',
                fontSize: '13px',
                border: '1px solid #e1e4e8',
                fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace'
              }}
            >
              {result.sql}
            </Paragraph>
          </details>
        </Space>
      </Card>

      {/* Results & Charts Tabs */}
      <Card className="enhanced-card">
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          size="large"
          style={{ minHeight: '500px' }}
        >
          <TabPane
            tab={
              <Space>
                <TableOutlined />
                Data Results ({dataSource.length} rows)
              </Space>
            }
            key="data"
          >
            <div style={{ marginBottom: '16px', textAlign: 'right' }}>
              <Button
                icon={<DownloadOutlined />}
                onClick={() => {
                  // Export functionality
                  const csvContent = [
                    columns.map(col => col.title).join(','),
                    ...dataSource.map(row =>
                      columns.map(col => row[col.dataIndex] || '').join(',')
                    )
                  ].join('\n');

                  const blob = new Blob([csvContent], { type: 'text/csv' });
                  const url = window.URL.createObjectURL(blob);
                  const a = document.createElement('a');
                  a.href = url;
                  a.download = `query-results-${Date.now()}.csv`;
                  a.click();
                  window.URL.revokeObjectURL(url);
                }}
                style={{
                  borderRadius: '8px',
                  border: '1px solid #13c2c2',
                  color: '#13c2c2'
                }}
              >
                Export to CSV
              </Button>
            </div>
            <Table
              columns={columns}
              dataSource={dataSource}
              pagination={{
                pageSize: 10,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} items`,
                style: { marginTop: '16px' }
              }}
              scroll={{ x: true }}
              size="small"
              style={{
                borderRadius: '8px',
                overflow: 'hidden'
              }}
            />
          </TabPane>

          <TabPane
            tab={
              <Space>
                <ThunderboltOutlined />
                Advanced Visualizations
                <Tag color="green" size="small">NEW</Tag>
              </Space>
            }
            key="advanced"
          >
            <AdvancedVisualizationPanel
              data={dataSource}
              columns={result.result?.metadata.columns || []}
              query={query}
              onConfigChange={setAdvancedVisualizationConfig}
              onExport={(format) => {
                console.log('Export requested:', format);
              }}
            />
          </TabPane>
        </Tabs>
      </Card>



      {/* Enhanced Suggestions */}
      {result.suggestions && result.suggestions.length > 0 && (
        <Card
          className="enhanced-card"
          size="small"
          title={
            <Space>
              <span style={{ color: '#667eea' }}>💡 Suggested Follow-up Queries</span>
            </Space>
          }
        >
          <Space direction="vertical" style={{ width: '100%' }} size="small">
            {result.suggestions.map((suggestion, index) => (
              <Button
                key={index}
                type="text"
                style={{
                  textAlign: 'left',
                  padding: '12px 16px',
                  height: 'auto',
                  width: '100%',
                  background: 'linear-gradient(135deg, #f8f9ff 0%, #e8f4fd 100%)',
                  border: '1px solid #e8f4fd',
                  borderRadius: '8px',
                  color: '#595959',
                  fontWeight: 400,
                  transition: 'all 0.3s ease'
                }}
                onClick={() => {
                  if (onSuggestionClick) {
                    onSuggestionClick(suggestion);
                  }
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.background = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
                  e.currentTarget.style.color = 'white';
                  e.currentTarget.style.transform = 'translateY(-2px)';
                  e.currentTarget.style.boxShadow = '0 4px 16px rgba(102, 126, 234, 0.3)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.background = 'linear-gradient(135deg, #f8f9ff 0%, #e8f4fd 100%)';
                  e.currentTarget.style.color = '#595959';
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
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
