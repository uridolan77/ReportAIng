import React, { useState } from 'react';
import {
  Input,
  Button,
  Space,
  Typography,
  Tooltip,
  Progress,
  Tag,
  Row,
  Col,
  Card,
} from 'antd';
import {
  SendOutlined,
  ToolOutlined,
  BookOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import { useQueryContext } from './QueryProvider';
import { EnhancedQueryInput } from './EnhancedQueryInput';
import { QueryShortcuts } from './QueryShortcuts';
import { QueryTemplate } from '../../services/queryTemplateService';

const { TextArea } = Input;
const { Text } = Typography;

export const QueryEditor: React.FC = () => {
  const {
    query,
    setQuery,
    isLoading,
    progress,
    isConnected,
    textAreaRef,
    handleSubmitQuery,
    handleKeyDown,
    setShowWizard,
    setShowTemplateLibrary,
    setShowCommandPalette
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
      <div className="query-header">
        <Typography.Title level={3}>
          BI Reporting Copilot
          {!isConnected && (
            <Tag color="orange" style={{ marginLeft: 8 }}>
              Offline
            </Tag>
          )}
        </Typography.Title>
        <Text type="secondary">
          Ask questions about your business data in natural language
        </Text>
      </div>

      <Row gutter={[16, 16]}>
        {/* Main Query Input */}
        <Col xs={24} lg={showShortcuts ? 16 : 24}>
          <div className="query-input-section">
            <EnhancedQueryInput
              value={query}
              onChange={setQuery}
              onSubmit={handleSubmitQuery}
              loading={isLoading}
              placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
              showShortcuts={true}
            />

            <div style={{ textAlign: 'center', marginTop: 16 }}>
              <Space wrap>
                <Text type="secondary">
                  New to querying data?
                </Text>
                <Button
                  type="link"
                  icon={<ToolOutlined />}
                  onClick={() => setShowWizard(true)}
                  disabled={isLoading}
                >
                  Use Query Builder Wizard
                </Button>
                <Text type="secondary">•</Text>
                <Button
                  type="link"
                  icon={<BookOutlined />}
                  onClick={() => setShowTemplateLibrary(true)}
                  disabled={isLoading}
                >
                  Browse Templates
                </Button>
                <Text type="secondary">•</Text>
                <Button
                  type="link"
                  icon={<ThunderboltOutlined />}
                  onClick={() => setShowShortcuts(!showShortcuts)}
                  disabled={isLoading}
                >
                  {showShortcuts ? 'Hide' : 'Show'} Shortcuts
                </Button>
              </Space>
            </div>
          </div>
        </Col>

        {/* Query Shortcuts Panel */}
        {showShortcuts && (
          <Col xs={24} lg={8}>
            <QueryShortcuts
              onQuerySelect={handleQuerySelect}
              onTemplateSelect={handleTemplateSelect}
              currentQuery={query}
            />
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
