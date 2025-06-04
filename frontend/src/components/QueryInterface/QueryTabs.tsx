import React from 'react';
import {
  Tabs,
  Button,
  Space,
  Tooltip,
  Typography,
  Row,
  Col
} from 'antd';
import './QueryTabs.css';
import {
  BarChartOutlined,
  HistoryOutlined,
  StarOutlined,
  DownloadOutlined,
  CodeOutlined,
  ThunderboltOutlined,
  DashboardOutlined,
  InteractionOutlined,
  SettingOutlined,
  SafetyOutlined,
  FileTextOutlined
} from '@ant-design/icons';
import { useQueryContext } from './QueryProvider';
import { QueryResult } from './QueryResult';
import { QueryHistory } from './QueryHistory';
import { QuerySuggestions } from './QuerySuggestions';
import { AdvancedStreamingQuery } from './AdvancedStreamingQuery';
import { InteractiveVisualization } from './InteractiveVisualization';
import { DashboardView } from './DashboardView';
import { QueryLoadingState, CopilotThinking } from './LoadingStates';
import { QueryProcessingViewer } from './QueryProcessingViewer';
import AdvancedVisualizationPanel from '../Visualization/AdvancedVisualizationPanel';
import { TuningDashboard } from '../Tuning/TuningDashboard';
import { CacheManager } from '../Performance/CacheManager';
import { SecurityDashboard } from '../Security/SecurityDashboard';
import { DataInsightsPanel } from '../Insights/DataInsightsPanel';
import PromptDetailsPanel from './PromptDetailsPanel';

const { Text } = Typography;
const { TabPane } = Tabs;

export const QueryTabs: React.FC = () => {
  const {
    activeTab,
    setActiveTab,
    currentResult,
    queryHistory,
    query,
    isAdmin,
    isLoading,
    progress,
    showInsightsPanel,
    setShowInsightsPanel,
    handleSubmitQuery,
    handleFollowUpSuggestionClick,
    handleSuggestionClick,
    handleAddToFavorites,
    handleVisualizationRequest,
    setShowExportModal,
    setQuery,
    // Processing state
    processingStages,
    currentProcessingStage,
    showProcessingDetails,
    setShowProcessingDetails,
    currentQueryId
  } = useQueryContext();

  // Debug logging for prompt details
  React.useEffect(() => {
    if (currentResult) {
      console.log('QueryTabs - Current result:', {
        hasResult: !!currentResult,
        hasPromptDetails: !!currentResult.promptDetails,
        promptDetails: currentResult.promptDetails,
        queryId: currentResult.queryId,
        success: currentResult.success
      });
    }
  }, [currentResult]);

  return (
    <Tabs
      activeKey={activeTab}
      onChange={setActiveTab}
      className="query-tabs enhanced-query-tabs"
      size="large"
      tabBarExtraContent={
        currentResult && (
          <Space size="small">
            <Tooltip title="Add to Favorites">
              <Button
                icon={<StarOutlined />}
                onClick={handleAddToFavorites}
                size="small"
                type="text"
                style={{
                  borderRadius: '12px',
                  border: '1px solid #e5e7eb',
                  background: 'white',
                  color: '#6b7280',
                  padding: '4px 8px',
                  height: '32px',
                  transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.borderColor = '#f59e0b';
                  e.currentTarget.style.color = '#f59e0b';
                  e.currentTarget.style.background = '#fffbeb';
                  e.currentTarget.style.transform = 'translateY(-1px)';
                  e.currentTarget.style.boxShadow = '0 4px 8px rgba(245, 158, 11, 0.2)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.borderColor = '#e5e7eb';
                  e.currentTarget.style.color = '#6b7280';
                  e.currentTarget.style.background = 'white';
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}
              />
            </Tooltip>
            <Tooltip title="Export Results">
              <Button
                icon={<DownloadOutlined />}
                onClick={() => setShowExportModal(true)}
                size="small"
                type="text"
                style={{
                  borderRadius: '12px',
                  border: '1px solid #e5e7eb',
                  background: 'white',
                  color: '#6b7280',
                  padding: '4px 8px',
                  height: '32px',
                  transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.borderColor = '#3b82f6';
                  e.currentTarget.style.color = '#3b82f6';
                  e.currentTarget.style.background = '#eff6ff';
                  e.currentTarget.style.transform = 'translateY(-1px)';
                  e.currentTarget.style.boxShadow = '0 4px 8px rgba(59, 130, 246, 0.2)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.borderColor = '#e5e7eb';
                  e.currentTarget.style.color = '#6b7280';
                  e.currentTarget.style.background = 'white';
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}
              />
            </Tooltip>
          </Space>
        )
      }
    >
      <TabPane
        tab={
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', fontSize: '15px', fontWeight: '600' }}>
            <BarChartOutlined style={{ fontSize: '20px', color: '#3b82f6' }} />
            <span>Results</span>
            {currentResult && currentResult.confidence && (
              <div style={{
                background: currentResult.confidence > 0.8 ? 'linear-gradient(135deg, #10b981 0%, #059669 100%)' :
                           currentResult.confidence > 0.6 ? 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)' :
                           'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)',
                color: 'white',
                padding: '4px 8px',
                borderRadius: '12px',
                fontSize: '11px',
                fontWeight: 700,
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
                boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)'
              }}>
                {(currentResult.confidence * 100).toFixed(0)}%
              </div>
            )}
            {currentResult && (
              <Tooltip title={showInsightsPanel ? 'Hide AI Insights Panel' : 'Show AI Insights Panel'}>
                <Button
                  type="text"
                  size="small"
                  icon={<ThunderboltOutlined />}
                  onClick={(e) => {
                    e.stopPropagation();
                    setShowInsightsPanel(!showInsightsPanel);
                  }}
                  style={{
                    color: showInsightsPanel ? '#10b981' : '#6b7280',
                    padding: '4px 6px',
                    height: '24px',
                    fontSize: '12px',
                    borderRadius: '8px',
                    background: showInsightsPanel ? '#ecfdf5' : 'transparent',
                    border: showInsightsPanel ? '1px solid #10b981' : '1px solid transparent',
                    transition: 'all 0.3s ease'
                  }}
                />
              </Tooltip>
            )}
          </div>
        }
        key="result"
      >
        {isLoading ? (
          <>
            {/* Query Processing Details during loading */}
            <QueryProcessingViewer
              stages={processingStages}
              isProcessing={isLoading}
              currentStage={currentProcessingStage}
              queryId={currentQueryId}
              isVisible={showProcessingDetails}
              onToggleVisibility={() => setShowProcessingDetails(!showProcessingDetails)}
            />

            <QueryLoadingState
              progress={progress}
              stage="analyzing"
              message="AI Copilot is analyzing your question and preparing results..."
            />
          </>
        ) : currentResult ? (
          <>
            {/* Query Processing Details */}
            <QueryProcessingViewer
              stages={processingStages}
              isProcessing={isLoading}
              currentStage={currentProcessingStage}
              queryId={currentQueryId}
              isVisible={showProcessingDetails}
              onToggleVisibility={() => setShowProcessingDetails(!showProcessingDetails)}
            />

            <Row gutter={[16, 16]}>
              <Col xs={24} lg={showInsightsPanel && currentResult.success ? 16 : 24}>
                <QueryResult
                  result={currentResult}
                  query={query}
                  onRequery={handleSubmitQuery}
                  onSuggestionClick={handleFollowUpSuggestionClick}
                  onVisualizationRequest={handleVisualizationRequest}
                />
              </Col>
              {showInsightsPanel && currentResult.success && (
                <Col xs={24} lg={8}>
                  <div data-testid="insights-panel" tabIndex={0}>
                    <DataInsightsPanel
                      queryResult={currentResult}
                      onInsightAction={(action) => {
                        console.log('Insight action:', action);
                        // Handle insight actions like drill-down, filtering, etc.
                      }}
                      autoGenerate={true}
                    />
                  </div>
                </Col>
              )}
            </Row>
          </>
        ) : (
          <div className="empty-result" style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '300px',
            textAlign: 'center',
            padding: '40px 20px',
            background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
            borderRadius: '16px',
            border: '2px dashed #e2e8f0',
            margin: '20px 0'
          }}>
            <div style={{
              fontSize: '48px',
              marginBottom: '16px',
              opacity: 0.6
            }}>
              📊
            </div>
            <Text style={{
              fontSize: '18px',
              color: '#4b5563',
              fontWeight: 600,
              marginBottom: '8px',
              fontFamily: "'Inter', sans-serif"
            }}>
              Your query results will appear here
            </Text>
            <Text style={{
              fontSize: '14px',
              color: '#6b7280',
              fontWeight: 400
            }}>
              Ask a question about your data to see visualized insights
            </Text>
          </div>
        )}
      </TabPane>

      <TabPane
        tab={
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', fontSize: '15px', fontWeight: '600' }}>
            <HistoryOutlined style={{ fontSize: '20px', color: '#10b981' }} />
            <span>History</span>
            <div style={{
              background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
              color: 'white',
              padding: '2px 8px',
              borderRadius: '12px',
              fontSize: '11px',
              fontWeight: 700,
              minWidth: '20px',
              textAlign: 'center'
            }}>
              {queryHistory.length}
            </div>
          </div>
        }
        key="history"
      >
        <QueryHistory
          onQuerySelect={(selectedQuery) => {
            setQuery(selectedQuery);
            setActiveTab('result');
          }}
        />
      </TabPane>

      <TabPane
        tab={
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', fontSize: '15px', fontWeight: '600' }}>
            <StarOutlined style={{ fontSize: '20px', color: '#8b5cf6' }} />
            <span>Suggestions</span>
            <div style={{
              background: 'linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)',
              color: 'white',
              padding: '2px 6px',
              borderRadius: '8px',
              fontSize: '9px',
              fontWeight: 700,
              textTransform: 'uppercase',
              letterSpacing: '0.05em'
            }}>
              AI
            </div>
          </div>
        }
        key="suggestions"
      >
        <QuerySuggestions
          onSuggestionClick={handleSuggestionClick}
        />
      </TabPane>

      <TabPane
        tab={
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', fontSize: '15px', fontWeight: '600' }}>
            <FileTextOutlined style={{ fontSize: '20px', color: '#f59e0b' }} />
            <span>Prompt Details</span>
            {currentResult?.promptDetails && (
              <div style={{
                background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                color: 'white',
                fontSize: '9px',
                padding: '3px 8px',
                borderRadius: '12px',
                fontWeight: 700,
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
                boxShadow: '0 2px 4px rgba(16, 185, 129, 0.3)'
              }}>
                ✓ Available
              </div>
            )}
          </div>
        }
        key="prompt"
      >
        {currentResult && currentResult.promptDetails ? (
          <PromptDetailsPanel promptDetails={currentResult.promptDetails} />
        ) : (
          <div className="empty-result">
            <Text type="secondary">
              {currentResult ? (
                <>
                  No prompt details available for this query.
                  <br />
                  <small style={{ color: '#999' }}>
                    Debug: currentResult exists but promptDetails is {currentResult.promptDetails ? 'truthy' : 'falsy'}
                    <br />
                    currentResult keys: {Object.keys(currentResult).join(', ')}
                    <br />
                    promptDetails value: {JSON.stringify(currentResult.promptDetails)}
                  </small>
                </>
              ) : (
                'Execute a query to see the AI prompt details'
              )}
            </Text>
          </div>
        )}
      </TabPane>
    </Tabs>
  );
};
