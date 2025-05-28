import React from 'react';
import {
  Input,
  Button,
  Space,
  Typography,
  Tooltip,
  Progress,
  Tag
} from 'antd';
import {
  SendOutlined,
  ToolOutlined,
  BookOutlined
} from '@ant-design/icons';
import { useQueryContext } from './QueryProvider';

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

      <div className="query-input-section">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Space.Compact style={{ width: '100%' }}>
            <TextArea
              ref={textAreaRef}
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
              autoSize={{ minRows: 2, maxRows: 6 }}
              disabled={isLoading}
              className="query-textarea"
            />
            <Tooltip title="Execute Query (Ctrl+Enter)">
              <Button
                type="primary"
                icon={<SendOutlined />}
                onClick={handleSubmitQuery}
                disabled={!query.trim() || isLoading}
                loading={isLoading}
                className="submit-button"
              >
                Ask
              </Button>
            </Tooltip>
          </Space.Compact>

          <div style={{ textAlign: 'center' }}>
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
                onClick={() => setShowCommandPalette(true)}
                disabled={isLoading}
              >
                Command Palette (Ctrl+K)
              </Button>
            </Space>
          </div>
        </Space>
      </div>

      {isLoading && (
        <div className="query-progress">
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
