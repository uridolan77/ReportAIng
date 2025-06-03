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
    showInsightsPanel,
    setShowInsightsPanel,
    handleSubmitQuery,
    handleFollowUpSuggestionClick,
    handleSuggestionClick,
    handleAddToFavorites,
    handleVisualizationRequest,
    setShowExportModal,
    setQuery
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
          <Space>
            <Tooltip title="Add to Favorites">
              <Button
                icon={<StarOutlined />}
                onClick={handleAddToFavorites}
                size="small"
                type="text"
                style={{ borderRadius: '8px' }}
              />
            </Tooltip>
            <Tooltip title="Export Results">
              <Button
                icon={<DownloadOutlined />}
                onClick={() => setShowExportModal(true)}
                size="small"
                type="text"
                style={{ borderRadius: '8px' }}
              />
            </Tooltip>
          </Space>
        )
      }
    >
      <TabPane
        tab={
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <BarChartOutlined style={{ fontSize: '18px' }} />
            <span>Results</span>
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
                    color: showInsightsPanel ? '#52c41a' : '#8c8c8c',
                    padding: '2px 4px',
                    height: '20px',
                    fontSize: '12px'
                  }}
                />
              </Tooltip>
            )}
          </div>
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
                onVisualizationRequest={handleVisualizationRequest}
              />
            </Col>
            {showInsightsPanel && currentResult.success && (
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
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <HistoryOutlined style={{ fontSize: '18px' }} />
            History ({queryHistory.length})
          </span>
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
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <CodeOutlined style={{ fontSize: '18px' }} />
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
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <FileTextOutlined style={{ fontSize: '18px' }} />
            Prompt Details
            {currentResult?.promptDetails && (
              <span style={{
                backgroundColor: '#52c41a',
                color: 'white',
                fontSize: '10px',
                padding: '2px 6px',
                borderRadius: '10px',
                marginLeft: '8px'
              }}>
                Available
              </span>
            )}
          </span>
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

      <TabPane
        tab={
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <ThunderboltOutlined style={{ fontSize: '18px' }} />
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
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <InteractionOutlined style={{ fontSize: '18px' }} />
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
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <DashboardOutlined style={{ fontSize: '18px' }} />
            Dashboard Builder
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
          <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
            <ThunderboltOutlined style={{ fontSize: '18px' }} />
            Advanced Visualizations
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
              Execute a query first to see advanced visualizations and AI recommendations
            </Text>
          </div>
        )}
      </TabPane>

      {isAdmin && (
        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
              <SettingOutlined style={{ fontSize: '18px' }} />
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
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
              <ThunderboltOutlined style={{ fontSize: '18px' }} />
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
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
              <SafetyOutlined style={{ fontSize: '18px' }} />
              Security
            </span>
          }
          key="security"
        >
          <SecurityDashboard />
        </TabPane>
      )}
    </Tabs>
  );
};
