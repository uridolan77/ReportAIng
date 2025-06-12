import React, { useState } from 'react';
import {
  Button,
  Space,
  Typography,
  Progress,
  Tag,
  Row,
  Col,
} from 'antd';
import {
  ToolOutlined,
  BookOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import { useQueryContext } from './QueryProvider';
import { QueryInput } from './QueryInput';
import { QueryShortcuts } from './QueryShortcuts';
import { QueryTemplate } from '../../services/queryTemplateService';

// TextArea not used in this component
const { Text } = Typography;

export const QueryEditor: React.FC = () => {
  const {
    query,
    setQuery,
    isLoading,
    progress,
    isConnected,
    handleSubmitQuery,
    setShowWizard,
    setShowTemplateLibrary
  } = useQueryContext();

  const [showShortcuts, setShowShortcuts] = useState(true);

  const handleQuerySelect = (selectedQuery: string) => {
    setQuery(selectedQuery);
  };

  const handleTemplateSelect = (template: QueryTemplate, variables: Record<string, string>) => {
    // Template processing is handled in the QueryShortcuts component
    // The processed query is passed to handleQuerySelect
  };

  return (
    <>
      {/* Enhanced Header Section */}
      <div className="query-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          BI Reporting Copilot
          {!isConnected && (
            <Tag color="orange" style={{ marginLeft: 12, fontSize: '14px' }}>
              Offline Mode
            </Tag>
          )}
        </Typography.Title>
        <Text type="secondary" style={{ fontSize: '16px', marginTop: '8px', display: 'block' }}>
          Ask questions about your business data in natural language
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        {/* Main Query Input Section */}
        <Col xs={24} lg={showShortcuts ? 16 : 24}>
          <div className="query-input-section">
            <QueryInput
              value={query}
              onChange={setQuery}
              onSubmit={handleSubmitQuery}
              loading={isLoading}
              placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
              showLLMSelector={true}
            />

            {/* Enhanced Action Buttons */}
            <div style={{
              textAlign: 'center',
              marginTop: 24,
              padding: '16px',
              background: 'linear-gradient(135deg, #f8f9ff 0%, #e8f4fd 100%)',
              borderRadius: '12px',
              border: '1px solid #e8f4fd'
            }}>
              <Space wrap size="large">
                <Button
                  type="text"
                  icon={<ToolOutlined />}
                  onClick={() => setShowWizard(true)}
                  disabled={isLoading}
                  style={{
                    color: '#667eea',
                    fontWeight: 500,
                    border: '1px solid #667eea',
                    borderRadius: '8px',
                    padding: '4px 16px'
                  }}
                >
                  Query Builder Wizard
                </Button>

                <Button
                  type="text"
                  icon={<BookOutlined />}
                  onClick={() => setShowTemplateLibrary(true)}
                  disabled={isLoading}
                  style={{
                    color: '#667eea',
                    fontWeight: 500,
                    border: '1px solid #667eea',
                    borderRadius: '8px',
                    padding: '4px 16px'
                  }}
                >
                  Browse Templates
                </Button>

                <Button
                  type="text"
                  icon={<ThunderboltOutlined />}
                  onClick={() => setShowShortcuts(!showShortcuts)}
                  disabled={isLoading}
                  style={{
                    color: showShortcuts ? '#52c41a' : '#667eea',
                    fontWeight: 500,
                    border: `1px solid ${showShortcuts ? '#52c41a' : '#667eea'}`,
                    borderRadius: '8px',
                    padding: '4px 16px',
                    background: showShortcuts ? 'rgba(82, 196, 26, 0.1)' : 'transparent'
                  }}
                >
                  {showShortcuts ? 'Hide' : 'Show'} Shortcuts
                </Button>
              </Space>
            </div>
          </div>
        </Col>

        {/* Enhanced Query Shortcuts Panel */}
        {showShortcuts && (
          <Col xs={24} lg={8}>
            <div className="shortcuts-panel">
              <QueryShortcuts
                onQuerySelect={handleQuerySelect}
                onTemplateSelect={handleTemplateSelect}
                currentQuery={query}
              />
            </div>
          </Col>
        )}
      </Row>

      {isLoading && (
        <div className="query-progress" style={{ marginTop: 16 }}>
          <Progress
            percent={progress}
            size="small"
            status="active"
            format={(percent) => `Processing... ${percent}%`}
          />
        </div>
      )}
    </>
  );
};
