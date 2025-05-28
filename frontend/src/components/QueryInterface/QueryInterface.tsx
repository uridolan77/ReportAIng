import React, { useState, useCallback, useRef, useEffect } from 'react';
import {
  Card,
  Input,
  Button,
  Space,
  Typography,
  Tabs,
  Tooltip,
  Progress,
  Tag,
  Row,
  Col
} from 'antd';
import {
  SendOutlined,
  HistoryOutlined,
  StarOutlined,
  DownloadOutlined,
  CodeOutlined,
  ToolOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  DashboardOutlined,
  InteractionOutlined,
  SettingOutlined,
  SafetyOutlined,
  BookOutlined
} from '@ant-design/icons';
import { useQueryStore } from '../../stores/queryStore';
import { useAuthStore } from '../../stores/authStore';
import { useWebSocket } from '../../hooks/useWebSocket';
import { QueryResult } from './QueryResult';
import { QueryHistory } from './QueryHistory';
import { QuerySuggestions } from './QuerySuggestions';
import { ExportModal } from './ExportModal';
import { AdvancedStreamingQuery } from './AdvancedStreamingQuery';
import { QueryWizard } from './QueryWizard';
import { getOrCreateSessionId } from '../../utils/sessionUtils';
import { InteractiveVisualization } from './InteractiveVisualization';
import { DashboardView } from './DashboardView';
import AdvancedVisualizationPanel from '../Visualization/AdvancedVisualizationPanel';
import { TuningDashboard } from '../Tuning/TuningDashboard';
import { DatabaseConnectionBanner } from '../Layout/DatabaseConnectionBanner';
import { CacheManager } from '../Performance/CacheManager';
import { SecurityDashboard } from '../Security/SecurityDashboard';
import { DataInsightsPanel } from '../Insights/DataInsightsPanel';
import { QueryTemplateLibrary } from '../QueryTemplates/QueryTemplateLibrary';
import { CommandPalette } from '../CommandPalette/CommandPalette';
import { useKeyboardNavigation, ScreenReaderAnnouncer } from '../../hooks/useKeyboardNavigation';
import type { QueryRequest } from '../../types/query';

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
  const [showWizard, setShowWizard] = useState(false);
  const [showTemplateLibrary, setShowTemplateLibrary] = useState(false);
  const [showInsightsPanel, setShowInsightsPanel] = useState(true);
  const [showCommandPalette, setShowCommandPalette] = useState(false);

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

  // Keyboard navigation
  const keyboardNavigation = useKeyboardNavigation({
    onExecuteQuery: handleSubmitQuery,
    onSaveQuery: () => handleAddToFavorites(),
    onFocusQueryInput: () => textAreaRef.current?.focus(),
    onClearQuery: () => setQuery(''),
    onNewQuery: () => {
      setQuery('');
      clearCurrentResult();
      setActiveTab('query');
    },
    onOpenTemplates: () => setShowTemplateLibrary(true),
    onToggleInsights: () => setShowInsightsPanel(prev => !prev),
    onToggleHistory: () => setActiveTab('history'),
    onExportResults: () => setShowExportModal(true),
    onOpenCommandPalette: () => setShowCommandPalette(true),
    onToggleHelp: () => {
      // Show help modal or navigate to help
      console.log('Help requested');
    }
  });

  // Handle wizard query generation
  const handleWizardQueryGenerated = useCallback(async (generatedQuery: string, wizardData: any) => {
    setQuery(generatedQuery);
    setShowWizard(false);
    setIsLoading(true);
    setProgress(10);
    clearCurrentResult();

    const queryRequest: QueryRequest = {
      question: generatedQuery.trim(),
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
      console.error('Wizard query execution failed:', error);
    } finally {
      setIsLoading(false);
      setProgress(0);
    }
  }, [executeQuery, clearCurrentResult]);

  // Handle command palette actions
  useEffect(() => {
    const handleCommandAction = (event: CustomEvent) => {
      const { action } = event.detail;

      switch (action) {
        case 'execute-query':
          handleSubmitQuery();
          break;
        case 'new-query':
          setQuery('');
          clearCurrentResult();
          setActiveTab('result');
          break;
        case 'save-query':
          handleAddToFavorites();
          break;
        case 'toggle-history':
          setActiveTab('history');
          break;
        case 'open-templates':
          setShowTemplateLibrary(true);
          break;
        case 'export-results':
          setShowExportModal(true);
          break;
        case 'toggle-insights':
          setShowInsightsPanel(prev => !prev);
          break;
        case 'open-cache-manager':
          setActiveTab('cache');
          break;
        case 'open-security':
          setActiveTab('security');
          break;
        case 'open-visualizations':
          setActiveTab('advanced');
          break;
        default:
          console.log('Unknown command action:', action);
      }
    };

    window.addEventListener('command-palette-action' as any, handleCommandAction);
    return () => window.removeEventListener('command-palette-action' as any, handleCommandAction);
  }, [handleSubmitQuery, handleAddToFavorites, clearCurrentResult]);

  // Handle template application
  const handleTemplateApply = useCallback(async (generatedQuery: string, template: any) => {
    setQuery(generatedQuery);
    setShowTemplateLibrary(false);
    setIsLoading(true);
    setProgress(10);
    clearCurrentResult();

    const queryRequest: QueryRequest = {
      question: generatedQuery.trim(),
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
      console.error('Template query execution failed:', error);
    } finally {
      setIsLoading(false);
      setProgress(0);
    }
  }, [executeQuery, clearCurrentResult]);

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
              <Row gutter={[16, 16]}>
                <Col xs={24} lg={showInsightsPanel ? 16 : 24}>
                  <QueryResult
                    result={currentResult}
                    query={query}
                    onRequery={handleSubmitQuery}
                    onSuggestionClick={handleFollowUpSuggestionClick}
                  />
                </Col>
                {showInsightsPanel && (
                  <Col xs={24} lg={8}>
                    <DataInsightsPanel
                      queryResult={currentResult}
                      onInsightAction={(action) => {
                        console.log('Insight action:', action);
                        // Handle insight actions like drill-down, filtering, etc.
                      }}
                      autoGenerate={true}
                    />
                  </Col>
                )}
              </Row>
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

          {isAdmin && (
            <TabPane
              tab={
                <span>
                  <ThunderboltOutlined />
                  Cache Manager
                </span>
              }
              key="cache"
            >
              <CacheManager />
            </TabPane>
          )}

          {isAdmin && (
            <TabPane
              tab={
                <span>
                  <SafetyOutlined />
                  Security
                </span>
              }
              key="security"
            >
              <SecurityDashboard />
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

      {/* Query Wizard Modal */}
      {showWizard && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          zIndex: 1000,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}>
          <div style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            maxWidth: '95vw',
            maxHeight: '95vh',
            overflow: 'auto',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
          }}>
            <QueryWizard
              onQueryGenerated={handleWizardQueryGenerated}
              onClose={() => setShowWizard(false)}
            />
          </div>
        </div>
      )}

      {/* Query Template Library Modal */}
      {showTemplateLibrary && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          zIndex: 1000,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}>
          <div style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            maxWidth: '95vw',
            maxHeight: '95vh',
            overflow: 'auto',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
          }}>
            <QueryTemplateLibrary
              onApplyTemplate={handleTemplateApply}
              onClose={() => setShowTemplateLibrary(false)}
            />
          </div>
        </div>
      )}

      {/* Command Palette */}
      <CommandPalette
        visible={showCommandPalette}
        onClose={() => setShowCommandPalette(false)}
      />

      {/* Screen Reader Announcer */}
      <ScreenReaderAnnouncer />

    </div>
  );
};
