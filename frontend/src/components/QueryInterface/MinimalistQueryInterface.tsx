import React, { useState } from 'react';
import { Input, Button, Space, Typography, Card, Spin, Alert } from 'antd';
import { SendOutlined, LoadingOutlined } from '@ant-design/icons';
import DataTable from '../DataTable/DataTable';
import { QueryResponse } from '../../types/query';

const { Text } = Typography;
const { TextArea } = Input;

interface MinimalistQueryInterfaceProps {
  onQuery?: (query: string) => Promise<QueryResponse>;
  loading?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export const MinimalistQueryInterface: React.FC<MinimalistQueryInterfaceProps> = ({
  onQuery,
  loading = false,
  className,
  style
}) => {
  const [query, setQuery] = useState('');
  const [result, setResult] = useState<QueryResponse | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async () => {
    if (!query.trim() || !onQuery) return;
    
    setIsProcessing(true);
    try {
      const response = await onQuery(query.trim());
      setResult(response);
    } catch (error) {
      console.error('Query failed:', error);
      setResult({
        success: false,
        error: error instanceof Error ? error.message : 'Query failed',
        queryId: '',
        confidence: 0,
        executionTimeMs: 0,
        sql: '',
        cached: false
      });
    } finally {
      setIsProcessing(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSubmit();
    }
  };

  const isLoading = loading || isProcessing;

  // Prepare data for DataTable
  const dataSource = result?.success && result.result?.data 
    ? result.result.data.map((row, index) => ({ ...row, id: index }))
    : [];

  const columns = result?.success && result.result?.metadata?.columns
    ? result.result.metadata.columns.map(col => ({
        title: col.name,
        dataIndex: col.name,
        key: col.name,
        width: 150,
        dataType: col.type === 'number' || col.type === 'integer' || col.type === 'decimal' ? 'number' :
                  col.type === 'date' || col.type === 'datetime' || col.type === 'timestamp' ? 'date' :
                  col.type === 'boolean' ? 'boolean' : 'string',
        sortable: true,
        filterable: true,
        searchable: true,
        resizable: true,
        copyable: true,
        formatter: (value: any) => {
          if (value === null || value === undefined) {
            return <Text type="secondary">NULL</Text>;
          }
          return <span>{String(value)}</span>;
        },
      }))
    : [];

  return (
    <div 
      className={className}
      style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
        padding: '40px 20px',
        ...style
      }}
    >
      <div style={{
        maxWidth: '1200px',
        margin: '0 auto',
        display: 'flex',
        flexDirection: 'column',
        gap: '32px'
      }}>
        
        {/* Chat Input Section */}
        <Card
          style={{
            borderRadius: '16px',
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
            border: 'none'
          }}
          bodyStyle={{ padding: '32px' }}
        >
          <Space direction="vertical" size="large" style={{ width: '100%' }}>
            <div style={{ textAlign: 'center' }}>
              <Text style={{ 
                fontSize: '24px', 
                fontWeight: 600, 
                color: '#2c3e50',
                display: 'block',
                marginBottom: '8px'
              }}>
                Ask DailyActionsDB anything
              </Text>
              <Text style={{ 
                fontSize: '16px', 
                color: '#7f8c8d' 
              }}>
                Type your question in natural language
              </Text>
            </div>

            <div style={{ position: 'relative' }}>
              <TextArea
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyDown={handleKeyPress}
                placeholder="What insights would you like to discover? (Ctrl+Enter to send)"
                autoSize={{ minRows: 3, maxRows: 6 }}
                style={{
                  fontSize: '16px',
                  borderRadius: '12px',
                  border: '2px solid #e9ecef',
                  paddingRight: '60px',
                  resize: 'none'
                }}
                disabled={isLoading}
              />
              
              <Button
                type="primary"
                icon={isLoading ? <LoadingOutlined /> : <SendOutlined />}
                onClick={handleSubmit}
                disabled={!query.trim() || isLoading}
                style={{
                  position: 'absolute',
                  right: '12px',
                  bottom: '12px',
                  borderRadius: '8px',
                  height: '40px',
                  width: '40px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  background: isLoading ? '#95a5a6' : '#3498db',
                  borderColor: isLoading ? '#95a5a6' : '#3498db'
                }}
              />
            </div>

            {query.trim() && (
              <Text style={{ 
                fontSize: '12px', 
                color: '#95a5a6',
                textAlign: 'center',
                display: 'block'
              }}>
                Press Ctrl+Enter to send or click the send button
              </Text>
            )}
          </Space>
        </Card>

        {/* Loading State */}
        {isLoading && (
          <Card
            style={{
              borderRadius: '16px',
              boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
              border: 'none',
              textAlign: 'center'
            }}
            bodyStyle={{ padding: '48px' }}
          >
            <Spin size="large" />
            <div style={{ marginTop: '16px' }}>
              <Text style={{ fontSize: '16px', color: '#7f8c8d' }}>
                Processing your query...
              </Text>
            </div>
          </Card>
        )}

        {/* Results Section */}
        {result && !isLoading && (
          <Card
            style={{
              borderRadius: '16px',
              boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
              border: 'none'
            }}
            bodyStyle={{ padding: '32px' }}
          >
            {result.success ? (
              <Space direction="vertical" size="large" style={{ width: '100%' }}>
                <div style={{ 
                  display: 'flex', 
                  justifyContent: 'space-between', 
                  alignItems: 'center',
                  flexWrap: 'wrap',
                  gap: '16px'
                }}>
                  <Text style={{ 
                    fontSize: '20px', 
                    fontWeight: 600, 
                    color: '#2c3e50' 
                  }}>
                    Results ({dataSource.length} rows)
                  </Text>
                  <Space>
                    <Text style={{ fontSize: '14px', color: '#7f8c8d' }}>
                      {result.executionTimeMs}ms
                    </Text>
                    {result.cached && (
                      <Text style={{ fontSize: '14px', color: '#27ae60' }}>
                        • Cached
                      </Text>
                    )}
                  </Space>
                </div>

                {dataSource.length > 0 ? (
                  <DataTable
                    data={dataSource}
                    columns={columns}
                    keyField="id"
                    features={{
                      pagination: true,
                      sorting: true,
                      filtering: true,
                      searching: true,
                      selection: true,
                      rowHiding: true,
                      resizing: true,
                      copying: true,
                      export: true,
                      exportFormats: ['csv', 'excel', 'pdf'],
                      print: false,
                      columnChooser: true,
                      fullscreen: false,
                      virtualScroll: dataSource.length > 100,
                      keyboard: true,
                      contextMenu: false
                    }}
                    config={{
                      pageSize: 25,
                      pageSizeOptions: [10, 25, 50, 100],
                      stripedRows: true,
                      hoverEffect: true,
                      stickyHeader: false,
                      exportFileName: `query-results-${new Date().toISOString().split('T')[0]}`,
                      density: 'standard'
                    }}
                    style={{
                      borderRadius: '12px',
                      border: '1px solid #e9ecef'
                    }}
                  />
                ) : (
                  <Alert
                    message="No data found"
                    description="Your query executed successfully but returned no results."
                    type="info"
                    showIcon
                    style={{ borderRadius: '8px' }}
                  />
                )}
              </Space>
            ) : (
              <Alert
                message="Query Failed"
                description={typeof result.error === 'string' ? result.error : result.error?.message || 'An error occurred'}
                type="error"
                showIcon
                style={{ borderRadius: '8px' }}
              />
            )}
          </Card>
        )}
      </div>
    </div>
  );
};

export default MinimalistQueryInterface;
