import React, { useState, useCallback, useRef, useEffect } from 'react';
import {
  Card,
  Input,
  Button,
  Space,
  Typography,
  Spin,
  Alert,
  Tabs,
  Tooltip,
  Progress,
  Tag
} from 'antd';
import {
  SendOutlined,
  HistoryOutlined,
  StarOutlined,
  DownloadOutlined,
  CodeOutlined,
  TableOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  DashboardOutlined,
  InteractionOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { useQueryStore } from '../../stores/queryStore';
import { useAuthStore } from '../../stores/authStore';
import { useWebSocket } from '../../hooks/useWebSocket';
import { QueryResult } from './QueryResult';
import { QueryHistory } from './QueryHistory';
import { QuerySuggestions } from './QuerySuggestions';
import { ExportModal } from './ExportModal';
import { AdvancedStreamingQuery } from './AdvancedStreamingQuery';
import { getOrCreateSessionId } from '../../utils/sessionUtils';
import { InteractiveVisualization } from './InteractiveVisualization';
import { DashboardView } from './DashboardView';
import AdvancedVisualizationPanel from '../Visualization/AdvancedVisualizationPanel';
import { TuningDashboard } from '../Tuning/TuningDashboard';
import { DatabaseConnectionBanner } from '../Layout/DatabaseConnectionBanner';
import type { QueryRequest, QueryResponse } from '../../types/query';

const { TextArea } = Input;
const { Title, Text } = Typography;
const { TabPane } = Tabs;

interface QueryInterfaceProps {
  className?: string;
}

export const QueryInterface: React.FC<QueryInterfaceProps> = ({ className }) => {
  // State management
  const [query, setQuery] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [activeTab, setActiveTab] = useState('query');
  const [showExportModal, setShowExportModal] = useState(false);

  // Store hooks
  const {
    currentResult,
    queryHistory,
    executeQuery,
    addToFavorites,
    clearCurrentResult
  } = useQueryStore();

  const { user } = useAuthStore();

  // Check if user is admin (temporarily allow all authenticated users to see tuning tab)
  const isAdmin = user !== null; // TODO: Change back to: user?.roles?.includes('Admin') || user?.roles?.includes('admin') || false;

  // WebSocket for real-time updates
  const { isConnected, lastMessage } = useWebSocket('/ws/query-status');

  // Refs
  const textAreaRef = useRef<any>(null);

  // Handle WebSocket messages for query progress
  useEffect(() => {
    if (lastMessage) {
      const message = JSON.parse(lastMessage.data);
      switch (message.type) {
        case 'query_progress':
          setProgress(message.progress * 100);
          break;
        case 'query_completed':
          setIsLoading(false);
          setProgress(0);
          break;
        case 'query_error':
          setIsLoading(false);
          setProgress(0);
          break;
      }
    }
  }, [lastMessage]);

  // Handle query submission
  const handleSubmitQuery = useCallback(async () => {
    if (!query.trim() || isLoading) return;

    setIsLoading(true);
    setProgress(10);
    clearCurrentResult();

    const queryRequest: QueryRequest = {
      question: query.trim(),
      sessionId: getOrCreateSessionId(),
      options: {
        includeVisualization: true,
        maxRows: 1000,
        enableCache: true,
        confidenceThreshold: 0.7
      }
    };

    try {
      await executeQuery(queryRequest);
      setActiveTab('result');
    } catch (error) {
      console.error('Query execution failed:', error);
    } finally {
      setIsLoading(false);
      setProgress(0);
    }
  }, [query, isLoading, executeQuery, clearCurrentResult]);

  // Handle keyboard shortcuts
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSubmitQuery();
    }
  }, [handleSubmitQuery]);

  // Handle query suggestions
  const handleSuggestionClick = useCallback((suggestion: string) => {
    setQuery(suggestion);
    textAreaRef.current?.focus();
  }, []);

  // Handle follow-up suggestion clicks (execute immediately)
  const handleFollowUpSuggestionClick = useCallback(async (suggestion: string) => {
    setQuery(suggestion);
    setIsLoading(true);
    setProgress(10);
    clearCurrentResult();

    const queryRequest: QueryRequest = {
      question: suggestion.trim(),
      sessionId: getOrCreateSessionId(),
      options: {
        includeVisualization: true,
        maxRows: 1000,
        enableCache: true,
        confidenceThreshold: 0.7
      }
    };

    try {
      await executeQuery(queryRequest);
      setActiveTab('result');
    } catch (error) {
      console.error('Follow-up query execution failed:', error);
    } finally {
      setIsLoading(false);
      setProgress(0);
    }
  }, [executeQuery, clearCurrentResult]);

  // Handle adding to favorites
  const handleAddToFavorites = useCallback(() => {
    if (currentResult && query) {
      addToFavorites({
        query: query.trim(),
        timestamp: new Date().toISOString(),
        description: `Query: ${query.substring(0, 50)}...`
      });
    }
  }, [currentResult, query, addToFavorites]);

  return (
    <div className={`query-interface ${className || ''}`}>
      <DatabaseConnectionBanner />
      <Card className="query-card">
        <div className="query-header">
          <Title level={3}>
            BI Reporting Copilot
            {!isConnected && (
              <Tag color="orange" style={{ marginLeft: 8 }}>
                Offline
              </Tag>
            )}
          </Title>
          <Text type="secondary">
            Ask questions about your business data in natural language
          </Text>
        </div>

        <div className="query-input-section">
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
        </div>

        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          className="query-tabs"
          tabBarExtraContent={
            currentResult && (
              <Space>
                <Tooltip title="Add to Favorites">
                  <Button
                    icon={<StarOutlined />}
                    onClick={handleAddToFavorites}
                    size="small"
                  />
                </Tooltip>
                <Tooltip title="Export Results">
                  <Button
                    icon={<DownloadOutlined />}
                    onClick={() => setShowExportModal(true)}
                    size="small"
                  />
                </Tooltip>
              </Space>
            )
          }
        >
          <TabPane
            tab={
              <span>
                <BarChartOutlined />
                Results
              </span>
            }
            key="result"
          >
            {currentResult ? (
              <QueryResult
                result={currentResult}
                query={query}
                onRequery={handleSubmitQuery}
                onSuggestionClick={handleFollowUpSuggestionClick}
              />
            ) : (
              <div className="empty-result">
                <Text type="secondary">
                  Submit a query to see results here
                </Text>
              </div>
            )}
          </TabPane>

          <TabPane
            tab={
              <span>
                <HistoryOutlined />
                History ({queryHistory.length})
              </span>
            }
            key="history"
          >
            <QueryHistory
              onQuerySelect={(selectedQuery) => {
                setQuery(selectedQuery);
                setActiveTab('query');
              }}
            />
          </TabPane>

          <TabPane
            tab={
              <span>
                <CodeOutlined />
                Suggestions
              </span>
            }
            key="suggestions"
          >
            <QuerySuggestions
              onSuggestionClick={handleSuggestionClick}
            />
          </TabPane>

          <TabPane
            tab={
              <span>
                <ThunderboltOutlined />
                Streaming
              </span>
            }
            key="streaming"
          >
            <AdvancedStreamingQuery
              onStreamingComplete={(data) => {
                // Handle streaming completion
                console.log('Streaming completed with', data.length, 'rows');
              }}
              onError={(error) => {
                console.error('Streaming error:', error);
              }}
            />
          </TabPane>

          <TabPane
            tab={
              <span>
                <InteractionOutlined />
                Interactive
              </span>
            }
            key="interactive"
          >
            {currentResult && currentResult.result?.data && currentResult.result?.metadata?.columns ? (
              <InteractiveVisualization
                data={currentResult.result.data}
                columns={currentResult.result.metadata.columns}
                query={query}
                onConfigChange={(config) => {
                  console.log('Interactive config changed:', config);
                }}
              />
            ) : (
              <div className="empty-result">
                <Text type="secondary">
                  Execute a query first to see interactive visualizations
                </Text>
              </div>
            )}
          </TabPane>

          <TabPane
            tab={
              <span>
                <DashboardOutlined />
                Dashboard
              </span>
            }
            key="dashboard"
          >
            {currentResult && currentResult.result?.data && currentResult.result?.metadata?.columns ? (
              <DashboardView
                data={currentResult.result.data}
                columns={currentResult.result.metadata.columns}
                query={query}
                onConfigChange={(config) => {
                  console.log('Dashboard config changed:', config);
                }}
              />
            ) : (
              <div className="empty-result">
                <Text type="secondary">
                  Execute a query first to see dashboard view
                </Text>
              </div>
            )}
          </TabPane>

          <TabPane
            tab={
              <span>
                <ThunderboltOutlined />
                Advanced AI
              </span>
            }
            key="advanced"
          >
            {currentResult && currentResult.result?.data && currentResult.result?.metadata?.columns ? (
              <AdvancedVisualizationPanel
                data={currentResult.result.data}
                columns={currentResult.result.metadata.columns}
                query={query}
                onConfigChange={(config) => {
                  console.log('Advanced visualization config changed:', config);
                }}
                onExport={(format, config) => {
                  console.log('Advanced visualization exported:', format, config);
                }}
              />
            ) : (
              <div className="empty-result">
                <Text type="secondary">
                  Execute a query first to see AI-powered advanced visualizations
                </Text>
              </div>
            )}
          </TabPane>

          {isAdmin && (
            <TabPane
              tab={
                <span>
                  <SettingOutlined />
                  AI Tuning
                </span>
              }
              key="tuning"
            >
              <TuningDashboard />
            </TabPane>
          )}
        </Tabs>
      </Card>

      {/* Export Modal */}
      <ExportModal
        visible={showExportModal}
        onClose={() => setShowExportModal(false)}
        result={currentResult}
        query={query}
      />


    </div>
  );
};
