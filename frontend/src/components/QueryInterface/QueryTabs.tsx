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
  SafetyOutlined
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

const { TabPane } = Tabs;
const { Text } = Typography;

export const QueryTabs: React.FC = () => {
  const {
    activeTab,
    setActiveTab,
    currentResult,
    queryHistory,
    query,
    isAdmin,
    showInsightsPanel,
    handleSubmitQuery,
    handleFollowUpSuggestionClick,
    handleSuggestionClick,
    handleAddToFavorites,
    setShowExportModal,
    setQuery
  } = useQueryContext();

  return (
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
            setActiveTab('result');
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
  );
};
