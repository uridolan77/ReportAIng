import React, { useState } from 'react';
import { Card, Table, Typography, Space, Tag, Button, Tabs } from 'antd';
import {
  ReloadOutlined,
  CodeOutlined,
  DownloadOutlined,
  TableOutlined,
  BugOutlined,
  CopyOutlined,
  CheckOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined
} from '@ant-design/icons';
import VisualizationRecommendations from '../Visualization/VisualizationRecommendations';
import { QueryResponse } from '../../types/query';

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
  // State for active tab
  const [activeTab, setActiveTab] = useState('data');
  // State for debug mode
  const [debugMode, setDebugMode] = useState(false);
  // State for pagination
  const [pageSize, setPageSize] = useState(50);
  const [currentPage, setCurrentPage] = useState(1);

  // Debug logging for prompt details and error handling
  React.useEffect(() => {
    console.log('🔍 QueryResult - Debug info:', {
      hasResult: !!result,
      success: result?.success,
      error: result?.error,
      hasPromptDetails: !!result?.promptDetails,
      promptDetails: result?.promptDetails,
      promptDetailsKeys: result?.promptDetails ? Object.keys(result.promptDetails) : 'N/A',
      queryId: result?.queryId,
      allResultKeys: result ? Object.keys(result) : 'N/A',
      willShowError: !result?.success
    });
  }, [result]);
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

          {/* Show SQL if available */}
          {result.sql && (
            <details style={{ marginBottom: '16px' }}>
              <summary style={{
                cursor: 'pointer',
                color: 'rgba(255, 255, 255, 0.9)',
                fontWeight: 500,
                padding: '8px',
                borderRadius: '6px',
                background: 'rgba(255, 255, 255, 0.1)',
                border: '1px solid rgba(255, 255, 255, 0.2)'
              }}>
                <CodeOutlined style={{ marginRight: '8px' }} /> View Generated SQL
              </summary>
              <Paragraph
                code
                copyable
                style={{
                  marginTop: '12px',
                  background: 'rgba(0, 0, 0, 0.3)',
                  padding: '16px',
                  borderRadius: '8px',
                  fontSize: '13px',
                  border: '1px solid rgba(255, 255, 255, 0.2)',
                  fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
                  color: 'white'
                }}
              >
                {result.sql}
              </Paragraph>
            </details>
          )}

          {/* Show prompt details if available */}
          {result.promptDetails && (
            <details style={{ marginBottom: '16px' }}>
              <summary style={{
                cursor: 'pointer',
                color: 'rgba(255, 255, 255, 0.9)',
                fontWeight: 500,
                padding: '8px',
                borderRadius: '6px',
                background: 'rgba(255, 255, 255, 0.1)',
                border: '1px solid rgba(255, 255, 255, 0.2)'
              }}>
                🔍 View AI Prompt Details
              </summary>
              <div style={{
                marginTop: '12px',
                background: 'rgba(0, 0, 0, 0.3)',
                padding: '16px',
                borderRadius: '8px',
                border: '1px solid rgba(255, 255, 255, 0.2)',
                color: 'white'
              }}>
                {/* Prompt Overview */}
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                  gap: '16px',
                  marginBottom: '16px',
                  padding: '12px',
                  background: 'rgba(255, 255, 255, 0.1)',
                  borderRadius: '6px',
                  border: '1px solid rgba(255, 255, 255, 0.2)'
                }}>
                  <div>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px', display: 'block' }}>Template</Text>
                    <Text strong style={{ color: 'rgba(255, 255, 255, 0.9)' }}>{result.promptDetails.templateName}</Text>
                  </div>
                  <div>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px', display: 'block' }}>Version</Text>
                    <Tag color="blue" style={{ margin: 0 }}>{result.promptDetails.templateVersion}</Tag>
                  </div>
                  <div>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px', display: 'block' }}>Token Count</Text>
                    <Tag color="purple" style={{ margin: 0 }}>{result.promptDetails.tokenCount} tokens</Tag>
                  </div>
                  <div>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px', display: 'block' }}>Generated</Text>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.9)', fontSize: '12px' }}>
                      {new Date(result.promptDetails.generatedAt).toLocaleTimeString()}
                    </Text>
                  </div>
                </div>

                {/* Template Variables */}
                {result.promptDetails.variables && Object.keys(result.promptDetails.variables).length > 0 && (
                  <div style={{ marginBottom: '16px' }}>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.8)', display: 'block', marginBottom: '8px', fontWeight: 500 }}>
                      📝 Template Variables:
                    </Text>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                      {Object.entries(result.promptDetails.variables).map(([key, value]) => (
                        <Tag
                          key={key}
                          color="geekblue"
                          style={{ margin: 0, cursor: 'help' }}
                          title={String(value).substring(0, 200) + (String(value).length > 200 ? '...' : '')}
                        >
                          {key}
                        </Tag>
                      ))}
                    </div>
                  </div>
                )}

                {/* Prompt Sections */}
                {result.promptDetails.sections && result.promptDetails.sections.length > 0 && (
                  <div style={{ marginBottom: '16px' }}>
                    <Text style={{ color: 'rgba(255, 255, 255, 0.8)', display: 'block', marginBottom: '8px', fontWeight: 500 }}>
                      📋 Prompt Sections ({result.promptDetails.sections.length}):
                    </Text>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px', marginBottom: '12px' }}>
                      {result.promptDetails.sections.map((section, index) => (
                        <Tag
                          key={index}
                          color={section.type === 'system' ? 'red' : section.type === 'schema' ? 'green' : section.type === 'context' ? 'orange' : 'blue'}
                          style={{ margin: 0 }}
                        >
                          {section.title || section.name}
                        </Tag>
                      ))}
                    </div>
                  </div>
                )}

                {/* Full Prompt */}
                <div>
                  <Text style={{ color: 'rgba(255, 255, 255, 0.8)', display: 'block', marginBottom: '8px', fontWeight: 500 }}>
                    🤖 Complete AI Prompt:
                  </Text>
                  <pre style={{
                    whiteSpace: 'pre-wrap',
                    fontSize: '11px',
                    color: 'rgba(255, 255, 255, 0.9)',
                    maxHeight: '300px',
                    overflow: 'auto',
                    fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
                    background: 'rgba(255, 255, 255, 0.1)',
                    padding: '12px',
                    border: '1px solid rgba(255, 255, 255, 0.2)',
                    borderRadius: '4px'
                  }}>
                    {result.promptDetails.fullPrompt}
                  </pre>
                </div>
              </div>
            </details>
          )}

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
    width: 150,
    render: (value: any, record: any, index: number) => {
      if (debugMode) {
        console.log('Column render:', { columnName: col.name, value, record, index });
      }
      if (value === null || value === undefined) {
        return <Text type="secondary">NULL</Text>;
      }
      return <span style={{ color: '#000', fontWeight: 'normal' }}>{String(value)}</span>;
    },
  })) || [];

  const dataSource = result.result?.data.map((row, index) => ({
    ...row,
    key: index,
  })) || [];

  // Debug logging for data structure (only when debug mode is enabled)
  if (debugMode) {
    console.log('QueryResult Debug:', {
      hasResult: !!result.result,
      hasMetadata: !!result.result?.metadata,
      hasColumns: !!result.result?.metadata?.columns,
      columnsCount: result.result?.metadata?.columns?.length,
      columns: result.result?.metadata?.columns,
      hasData: !!result.result?.data,
      dataCount: result.result?.data?.length,
      dataSource: dataSource,
      firstRow: dataSource[0],
      fullResult: result.result,
      fullMetadata: result.result?.metadata
    });
  }

  // If no columns in metadata, generate them from the first row of data
  const finalColumns = columns.length > 0 ? columns :
    (dataSource.length > 0 ?
      Object.keys(dataSource[0])
        .filter(key => key !== 'key') // Exclude the React key
        .map(key => ({
          title: key,
          dataIndex: key,
          key: key,
          width: 150,
          render: (value: any) => {
            if (value === null || value === undefined) {
              return <Text type="secondary">NULL</Text>;
            }
            return <span style={{ color: '#000', fontWeight: 'normal' }}>{String(value)}</span>;
          },
        })) : []);

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
              color: '#3b82f6',
              fontWeight: 600,
              padding: '12px 16px',
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
              border: '2px solid #3b82f620',
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              transition: 'all 0.3s ease'
            }}>
              <CodeOutlined style={{ fontSize: '16px' }} />
              <span>View Generated SQL</span>
              <div style={{
                background: '#3b82f6',
                color: 'white',
                padding: '2px 6px',
                borderRadius: '8px',
                fontSize: '10px',
                fontWeight: 700,
                textTransform: 'uppercase',
                letterSpacing: '0.05em'
              }}>
                Copy
              </div>
            </summary>
            <Paragraph
              code
              copyable={{
                tooltips: ['Copy SQL', 'Copied!']
              }}
              style={{
                marginTop: '12px',
                background: '#f6f8fa',
                padding: '20px',
                borderRadius: '12px',
                fontSize: '14px',
                border: '1px solid #e1e4e8',
                fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
                lineHeight: '1.6'
              }}
            >
              {result.sql}
            </Paragraph>
          </details>

          {/* Show prompt details if available */}
          {result.promptDetails && (
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
                🔍 View AI Prompt Details
              </summary>
              <div style={{
                marginTop: '12px',
                background: '#f6f8fa',
                padding: '16px',
                borderRadius: '8px',
                border: '1px solid #e1e4e8'
              }}>
                {/* Prompt Overview */}
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                  gap: '16px',
                  marginBottom: '16px',
                  padding: '12px',
                  background: '#ffffff',
                  borderRadius: '6px',
                  border: '1px solid #e1e4e8'
                }}>
                  <div>
                    <Text style={{ color: '#8c8c8c', fontSize: '12px', display: 'block' }}>Template</Text>
                    <Text strong style={{ color: '#262626' }}>{result.promptDetails.templateName}</Text>
                  </div>
                  <div>
                    <Text style={{ color: '#8c8c8c', fontSize: '12px', display: 'block' }}>Version</Text>
                    <Tag color="blue" style={{ margin: 0 }}>{result.promptDetails.templateVersion}</Tag>
                  </div>
                  <div>
                    <Text style={{ color: '#8c8c8c', fontSize: '12px', display: 'block' }}>Token Count</Text>
                    <Tag color="purple" style={{ margin: 0 }}>{result.promptDetails.tokenCount} tokens</Tag>
                  </div>
                  <div>
                    <Text style={{ color: '#8c8c8c', fontSize: '12px', display: 'block' }}>Generated</Text>
                    <Text style={{ color: '#262626', fontSize: '12px' }}>
                      {new Date(result.promptDetails.generatedAt).toLocaleTimeString()}
                    </Text>
                  </div>
                </div>

                {/* Template Variables */}
                {result.promptDetails.variables && Object.keys(result.promptDetails.variables).length > 0 && (
                  <div style={{ marginBottom: '16px' }}>
                    <Text style={{ color: '#595959', display: 'block', marginBottom: '8px', fontWeight: 500 }}>
                      📝 Template Variables:
                    </Text>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                      {Object.entries(result.promptDetails.variables).map(([key, value]) => (
                        <Tag
                          key={key}
                          color="geekblue"
                          style={{ margin: 0, cursor: 'help' }}
                          title={String(value).substring(0, 200) + (String(value).length > 200 ? '...' : '')}
                        >
                          {key}
                        </Tag>
                      ))}
                    </div>
                  </div>
                )}

                {/* Prompt Sections */}
                {result.promptDetails.sections && result.promptDetails.sections.length > 0 && (
                  <div style={{ marginBottom: '16px' }}>
                    <Text style={{ color: '#595959', display: 'block', marginBottom: '8px', fontWeight: 500 }}>
                      📋 Prompt Sections ({result.promptDetails.sections.length}):
                    </Text>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px', marginBottom: '12px' }}>
                      {result.promptDetails.sections.map((section, index) => (
                        <Tag
                          key={index}
                          color={section.type === 'system' ? 'red' : section.type === 'schema' ? 'green' : section.type === 'context' ? 'orange' : 'blue'}
                          style={{ margin: 0 }}
                        >
                          {section.title || section.name}
                        </Tag>
                      ))}
                    </div>

                    {/* Section Details */}
                    <details style={{ marginTop: '8px' }}>
                      <summary style={{
                        cursor: 'pointer',
                        color: '#595959',
                        fontSize: '12px',
                        padding: '4px 8px',
                        background: '#ffffff',
                        border: '1px solid #e1e4e8',
                        borderRadius: '4px'
                      }}>
                        View Section Details
                      </summary>
                      <div style={{ marginTop: '8px', maxHeight: '200px', overflow: 'auto' }}>
                        {result.promptDetails.sections.map((section, index) => (
                          <div key={index} style={{
                            marginBottom: '12px',
                            padding: '8px',
                            background: '#ffffff',
                            border: '1px solid #e1e4e8',
                            borderRadius: '4px'
                          }}>
                            <div style={{ marginBottom: '4px' }}>
                              <Tag color={section.type === 'system' ? 'red' : section.type === 'schema' ? 'green' : section.type === 'context' ? 'orange' : 'blue'} size="small">
                                {section.type}
                              </Tag>
                              <Text strong style={{ fontSize: '12px', marginLeft: '8px' }}>
                                {section.title || section.name}
                              </Text>
                            </div>
                            <pre style={{
                              whiteSpace: 'pre-wrap',
                              fontSize: '10px',
                              color: '#595959',
                              maxHeight: '100px',
                              overflow: 'auto',
                              margin: 0,
                              fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace'
                            }}>
                              {section.content.substring(0, 500)}{section.content.length > 500 ? '...' : ''}
                            </pre>
                          </div>
                        ))}
                      </div>
                    </details>
                  </div>
                )}

                {/* Full Prompt */}
                <div>
                  <Text style={{ color: '#595959', display: 'block', marginBottom: '8px', fontWeight: 500 }}>
                    🤖 Complete AI Prompt:
                  </Text>
                  <pre style={{
                    whiteSpace: 'pre-wrap',
                    fontSize: '11px',
                    color: '#24292e',
                    maxHeight: '300px',
                    overflow: 'auto',
                    fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
                    background: '#ffffff',
                    padding: '12px',
                    border: '1px solid #e1e4e8',
                    borderRadius: '4px'
                  }}>
                    {result.promptDetails.fullPrompt}
                  </pre>
                </div>
              </div>
            </details>
          )}
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
              <Space>
                <Button
                  icon={<BugOutlined />}
                  onClick={() => setDebugMode(!debugMode)}
                  type={debugMode ? 'primary' : 'default'}
                  style={{
                    borderRadius: '8px',
                    border: debugMode ? '1px solid #722ed1' : '1px solid #d9d9d9',
                    color: debugMode ? 'white' : '#722ed1'
                  }}
                >
                  {debugMode ? 'Hide Debug' : 'Show Debug'}
                </Button>
                <Button
                  icon={<DownloadOutlined />}
                  onClick={() => {
                    // Export functionality
                    const csvContent = [
                      finalColumns.map(col => col.title).join(','),
                      ...dataSource.map(row =>
                        finalColumns.map(col => row[col.dataIndex] || '').join(',')
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
              </Space>
            </div>
            {/* Debug info */}
            {debugMode && (
              <div style={{ marginBottom: '16px', padding: '8px', background: '#f0f0f0', borderRadius: '4px', fontSize: '12px' }}>
                <strong>Debug Info:</strong> Original Columns: {columns.length}, Final Columns: {finalColumns.length}, Rows: {dataSource.length}
                {dataSource.length > 0 && (
                  <div>
                    <div>First row keys: {Object.keys(dataSource[0]).join(', ')}</div>
                    <div>First row data: {JSON.stringify(dataSource[0])}</div>
                    <div>Final column dataIndex values: {finalColumns.map(c => c.dataIndex).join(', ')}</div>
                  </div>
                )}
              </div>
            )}

            <Table
              columns={finalColumns}
              dataSource={dataSource}
              pagination={{
                current: currentPage,
                pageSize: pageSize,
                showSizeChanger: true,
                showQuickJumper: true,
                pageSizeOptions: ['10', '25', '50', '100', '200'],
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} items`,
                style: { marginTop: '16px' },
                onChange: (page, size) => {
                  setCurrentPage(page);
                  if (size !== pageSize) {
                    setPageSize(size);
                    setCurrentPage(1); // Reset to first page when page size changes
                  }
                },
                onShowSizeChange: (current, size) => {
                  setPageSize(size);
                  setCurrentPage(1); // Reset to first page when page size changes
                }
              }}
              scroll={{ x: true }}
              size="small"
              style={{
                borderRadius: '8px',
                overflow: 'visible',
                minHeight: '200px',
                backgroundColor: 'white',
                ...(debugMode && { border: '2px solid blue' })
              }}
              className={debugMode ? "debug-table" : ""}
            />

            {/* Fallback simple table for debugging */}
            {debugMode && dataSource.length > 0 && (
              <div style={{ marginTop: '16px', padding: '16px', background: '#f9f9f9', borderRadius: '8px' }}>
                <h4>Fallback Table (for debugging):</h4>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                  <thead>
                    <tr>
                      {finalColumns.map(col => (
                        <th key={col.key} style={{ border: '1px solid #ddd', padding: '8px', background: '#f0f0f0' }}>
                          {col.title}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {dataSource.map((row, index) => (
                      <tr key={index}>
                        {finalColumns.map(col => (
                          <td key={`${index}-${col.key}`} style={{ border: '1px solid #ddd', padding: '8px' }}>
                            {row[col.dataIndex] !== null && row[col.dataIndex] !== undefined
                              ? String(row[col.dataIndex])
                              : 'NULL'}
                          </td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>

                {/* Raw data dump for debugging */}
                <div style={{ marginTop: '16px', padding: '8px', background: '#fff', border: '1px solid #ddd' }}>
                  <strong>Raw Data:</strong>
                  <pre style={{ fontSize: '10px', margin: '8px 0' }}>
                    {JSON.stringify(dataSource, null, 2)}
                  </pre>
                </div>
              </div>
            )}
          </TabPane>

          {/* Charts & Visualizations Tab */}
          <TabPane
            tab={
              <Space>
                <BarChartOutlined />
                Charts & Visualizations
              </Space>
            }
            key="charts"
          >
            <VisualizationRecommendations
              data={dataSource}
              columns={result.result?.metadata.columns || []}
              query={query}
              onVisualizationSelect={(config) => {
                console.log('Visualization selected:', config);
                // Handle visualization selection
                if (onVisualizationRequest) {
                  onVisualizationRequest(config.type, dataSource, result.result?.metadata.columns || []);
                }
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

      {/* Fallback Suggestions - Always Show */}
      {(!result.suggestions || result.suggestions.length === 0) && (
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
            {[
              "Show me total deposits for yesterday",
              "Top 10 players by deposits in the last 7 days",
              "Show me daily revenue for the last week",
              "Count of active players yesterday",
              "Show me casino vs sports betting revenue for last week",
              "Total bets and wins for this month",
              "Show me player activity for the last 3 days",
              "Revenue breakdown by country for last week"
            ].map((suggestion, index) => (
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
