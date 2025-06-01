import React, { useState } from 'react';
import { Card, Table, Typography, Space, Tag, Button, Divider, Row, Col } from 'antd';
import {
  ReloadOutlined,
  CodeOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DashboardOutlined,
  DownloadOutlined,
  TableOutlined
} from '@ant-design/icons';
import { QueryResponse } from '../../types/query';
import { InlineChart } from '../Visualization/InlineChart';

const { Title, Text, Paragraph } = Typography;

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

      {/* Enhanced Results Table */}
      <Card className="enhanced-card">
        <div style={{ marginBottom: '16px' }}>
          <Title level={5} style={{ margin: 0, color: '#667eea' }}>
            Data Results ({dataSource.length} rows)
          </Title>
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
      </Card>

      {/* Visualization Options */}
      {dataSource.length > 0 && (
        <Card className="enhanced-card">
          <div style={{ marginBottom: '16px' }}>
            <Title level={5} style={{ margin: 0, color: '#667eea' }}>
              📊 Visualization Options
            </Title>
            <Text type="secondary" style={{ fontSize: '14px' }}>
              Transform your data into interactive charts and graphs
            </Text>
          </div>

          <Row gutter={[12, 12]}>
            <Col xs={12} sm={8} md={6}>
              <Button
                icon={<BarChartOutlined />}
                onClick={() => {
                  setSelectedChartType(selectedChartType === 'bar' ? null : 'bar');
                  if (onVisualizationRequest) {
                    onVisualizationRequest('bar', dataSource, columns);
                  }
                }}
                style={{
                  width: '100%',
                  height: '60px',
                  borderRadius: '8px',
                  border: `1px solid ${selectedChartType === 'bar' ? '#667eea' : '#667eea'}`,
                  color: selectedChartType === 'bar' ? '#fff' : '#667eea',
                  backgroundColor: selectedChartType === 'bar' ? '#667eea' : 'transparent',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '12px'
                }}
              >
                Bar Chart
              </Button>
            </Col>

            <Col xs={12} sm={8} md={6}>
              <Button
                icon={<LineChartOutlined />}
                onClick={() => {
                  setSelectedChartType(selectedChartType === 'line' ? null : 'line');
                  if (onVisualizationRequest) {
                    onVisualizationRequest('line', dataSource, columns);
                  }
                }}
                style={{
                  width: '100%',
                  height: '60px',
                  borderRadius: '8px',
                  border: `1px solid #52c41a`,
                  color: selectedChartType === 'line' ? '#fff' : '#52c41a',
                  backgroundColor: selectedChartType === 'line' ? '#52c41a' : 'transparent',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '12px'
                }}
              >
                Line Chart
              </Button>
            </Col>

            <Col xs={12} sm={8} md={6}>
              <Button
                icon={<PieChartOutlined />}
                onClick={() => {
                  setSelectedChartType(selectedChartType === 'pie' ? null : 'pie');
                  if (onVisualizationRequest) {
                    onVisualizationRequest('pie', dataSource, columns);
                  }
                }}
                style={{
                  width: '100%',
                  height: '60px',
                  borderRadius: '8px',
                  border: `1px solid #fa8c16`,
                  color: selectedChartType === 'pie' ? '#fff' : '#fa8c16',
                  backgroundColor: selectedChartType === 'pie' ? '#fa8c16' : 'transparent',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '12px'
                }}
              >
                Pie Chart
              </Button>
            </Col>

            <Col xs={12} sm={8} md={6}>
              <Button
                icon={<DashboardOutlined />}
                onClick={() => {
                  if (onVisualizationRequest) {
                    onVisualizationRequest('dashboard', dataSource, columns);
                  }
                  setShowVisualizationOptions(true);
                }}
                style={{
                  width: '100%',
                  height: '60px',
                  borderRadius: '8px',
                  border: '1px solid #722ed1',
                  color: '#722ed1',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '12px'
                }}
              >
                Dashboard
              </Button>
            </Col>
          </Row>

          <Divider style={{ margin: '16px 0' }} />

          <Row gutter={[12, 12]}>
            <Col xs={24} sm={12}>
              <Button
                icon={<DownloadOutlined />}
                style={{
                  width: '100%',
                  borderRadius: '8px',
                  border: '1px solid #13c2c2',
                  color: '#13c2c2'
                }}
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
              >
                Export to CSV
              </Button>
            </Col>

            <Col xs={24} sm={12}>
              <Button
                icon={<TableOutlined />}
                style={{
                  width: '100%',
                  borderRadius: '8px',
                  border: '1px solid #eb2f96',
                  color: '#eb2f96'
                }}
                onClick={() => {
                  // Advanced table view functionality
                  console.log('Advanced table view requested');
                }}
              >
                Advanced Table View
              </Button>
            </Col>
          </Row>
        </Card>
      )}

      {/* Inline Chart Display - Positioned above suggestions */}
      {selectedChartType && dataSource && dataSource.length > 0 && (
        <InlineChart
          type={selectedChartType}
          data={dataSource}
          columns={columns.map(col => col.dataIndex)}
          title={`${selectedChartType.charAt(0).toUpperCase() + selectedChartType.slice(1)} Chart`}
        />
      )}

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
