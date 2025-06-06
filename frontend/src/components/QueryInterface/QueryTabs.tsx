import React, { useState, useEffect, useMemo } from 'react';
import {
  Tabs,
  Button,
  Space,
  Tooltip,
  Typography,
  Row,
  Col,
  Card,
  Tag
} from 'antd';
import './QueryTabs.css';
import {
  BarChartOutlined,
  StarOutlined,
  DownloadOutlined,
  FileTextOutlined,
  ThunderboltOutlined,
  SettingOutlined,
  BulbOutlined
} from '@ant-design/icons';
import { useQueryContext } from './QueryProvider';
import { QueryResult } from './QueryResult';
import { QueryLoadingState } from './LoadingStates';
import { DataInsightsPanel } from '../Insights/DataInsightsPanel';
// import { PromptDetailsPanel } from './PromptDetailsPanel';
import VisualizationRecommendations from '../Visualization/VisualizationRecommendations';
import AdvancedChart from '../Visualization/AdvancedChart';
import ChartConfigurationPanel from '../Visualization/ChartConfigurationPanel';
import { VisualizationRecommendation } from '../../types/visualization';
import { useVisualizationStore } from '../../stores/visualizationStore';

const { Text } = Typography;
const { TabPane } = Tabs;

export const QueryTabs: React.FC = () => {
  const {
    activeTab,
    setActiveTab,
    currentResult,
    query,
    isLoading,
    progress,
    showInsightsPanel,
    setShowInsightsPanel,
    handleSubmitQuery,
    handleFollowUpSuggestionClick,
    handleAddToFavorites,
    handleVisualizationRequest,
    setShowExportModal
  } = useQueryContext();

  // Use visualization store for persistent chart state
  const { currentVisualization, setVisualization } = useVisualizationStore();
  const [selectedRecommendation, setSelectedRecommendation] = useState<VisualizationRecommendation | null>(null);

  // Create a unique key for this query result to associate with chart
  const resultKey = `${query}-${currentResult?.queryId || 'unknown'}`;

  // Detect if this is gaming data
  const isGamingData = useMemo(() => {
    if (!currentResult?.result?.metadata?.columns) return false;
    const columnNames = currentResult.result.metadata.columns.map(col => col.name || col);
    return columnNames.some(col =>
      ['GameName', 'Provider', 'GameType', 'TotalRevenue', 'NetGamingRevenue', 'TotalSessions', 'TotalNetGamingRevenue'].includes(col)
    );
  }, [currentResult?.result?.metadata?.columns]);

  // Debug logging for current visualization
  useEffect(() => {
    console.log('QueryTabs - Current visualization state:', {
      hasVisualization: !!currentVisualization,
      visualizationType: currentVisualization?.chartType,
      isGamingData,
      resultKey
    });
  }, [currentVisualization, isGamingData, resultKey]);

  // Debug logging for prompt details
  React.useEffect(() => {
    if (currentResult) {
      console.log('QueryTabs - Current result:', {
        hasResult: !!currentResult,
        hasPromptDetails: !!currentResult.promptDetails,
        promptDetails: currentResult.promptDetails,
        queryId: currentResult.queryId,
        success: currentResult.success,
        error: currentResult.error,
        resultKeys: Object.keys(currentResult),
        willRenderQueryResult: true
      });
    } else {
      console.log('QueryTabs - No current result available');
    }
  }, [currentResult]);

  return (
    <div style={{
      background: 'white',
      borderRadius: '16px',
      border: '1px solid #e5e7eb',
      boxShadow: '0 4px 20px rgba(0, 0, 0, 0.08)',
      overflow: 'hidden'
    }}>
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
          <QueryLoadingState
            progress={progress}
            stage="analyzing"
            message="AI Copilot is analyzing your question and preparing results..."
          />
        ) : currentResult ? (
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
            <BarChartOutlined style={{ fontSize: '20px', color: '#10b981' }} />
            <span>Charts & Visualizations</span>
            {currentResult?.success && currentResult?.result?.data?.length > 0 && (
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
        key="charts"
      >
        {currentResult && currentResult.success && currentResult.result?.data?.length > 0 ? (
          <div style={{
            background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
            borderRadius: '16px',
            padding: '24px',
            minHeight: '600px'
          }}>
            {/* Enhanced Header Section */}
            <div style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '24px',
              padding: '16px 20px',
              background: 'white',
              borderRadius: '12px',
              boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)',
              border: '1px solid #e5e7eb'
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                <div style={{
                  width: '40px',
                  height: '40px',
                  borderRadius: '10px',
                  background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                  fontSize: '18px'
                }}>
                  📊
                </div>
                <div>
                  <Text style={{
                    fontSize: '18px',
                    fontWeight: 600,
                    color: '#1f2937',
                    display: 'block'
                  }}>
                    Data Visualization Studio
                  </Text>
                  <Text style={{
                    fontSize: '14px',
                    color: '#6b7280'
                  }}>
                    {currentResult.result.data.length} rows • {currentResult.result?.metadata.columns?.length || 0} columns
                  </Text>
                </div>
              </div>
              <Space>
                <Button
                  icon={<DownloadOutlined />}
                  onClick={() => console.log('Export all charts')}
                  style={{
                    borderRadius: '8px',
                    border: '1px solid #e5e7eb',
                    background: 'white',
                    color: '#6b7280'
                  }}
                >
                  Export
                </Button>
                {currentVisualization && (
                  <Button
                    type="primary"
                    danger
                    onClick={() => {
                      setVisualization(null);
                      setSelectedRecommendation(null);
                    }}
                    style={{ borderRadius: '8px' }}
                  >
                    Clear Chart
                  </Button>
                )}
              </Space>
            </div>

            <Row gutter={[24, 24]}>
              {/* Left Panel - Configuration */}
              <Col xs={24} lg={8}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                  {/* Chart Configuration */}
                  <Card
                    title={
                      <Space>
                        <SettingOutlined style={{ color: '#3b82f6' }} />
                        <span style={{ color: '#1f2937', fontWeight: 600 }}>Chart Builder</span>
                        {isGamingData && (
                          <Tag color="blue" style={{ fontSize: '10px' }}>Gaming Data</Tag>
                        )}
                      </Space>
                    }
                    size="small"
                    style={{
                      borderRadius: '12px',
                      border: '1px solid #e5e7eb',
                      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)'
                    }}
                    bodyStyle={{ padding: '16px' }}
                  >
                    <ChartConfigurationPanel
                      data={currentResult.result.data.map((row, index) => ({ ...row, id: index }))}
                      columns={currentResult.result?.metadata.columns || []}
                      currentConfig={currentVisualization}
                      onConfigChange={(config) => {
                        console.log('QueryTabs - Chart config changed:', config);
                        setVisualization(config);
                      }}
                    />
                  </Card>

                  {/* AI Recommendations */}
                  <Card
                    title={
                      <Space>
                        <BulbOutlined style={{ color: '#f59e0b' }} />
                        <span style={{ color: '#1f2937', fontWeight: 600 }}>AI Suggestions</span>
                      </Space>
                    }
                    size="small"
                    style={{
                      borderRadius: '12px',
                      border: '1px solid #e5e7eb',
                      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)'
                    }}
                    bodyStyle={{ padding: '16px' }}
                  >
                    <VisualizationRecommendations
                      data={currentResult.result.data.map((row, index) => ({ ...row, id: index }))}
                      columns={currentResult.result?.metadata.columns || []}
                      query={query}
                      onRecommendationSelect={(recommendation) => {
                        console.log('Recommendation selected:', recommendation);
                        setSelectedRecommendation(recommendation);
                      }}
                      onConfigGenerated={(config) => {
                        console.log('Config generated:', config);
                        setVisualization(config);
                        if (handleVisualizationRequest) {
                          handleVisualizationRequest(config.type, currentResult.result.data, currentResult.result?.metadata.columns || []);
                        }
                      }}
                    />
                  </Card>
                </div>
              </Col>

              {/* Right Panel - Chart Display */}
              <Col xs={24} lg={16}>
                {currentVisualization ? (
                  <Card
                    style={{
                      borderRadius: '12px',
                      border: '1px solid #e5e7eb',
                      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)',
                      minHeight: '500px'
                    }}
                    bodyStyle={{ padding: '20px' }}
                  >
                    <div style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      marginBottom: '20px',
                      paddingBottom: '16px',
                      borderBottom: '1px solid #f3f4f6'
                    }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                        <BarChartOutlined style={{ fontSize: '20px', color: '#3b82f6' }} />
                        <Text style={{
                          fontSize: '16px',
                          fontWeight: 600,
                          color: '#1f2937'
                        }}>
                          {currentVisualization.chartType} Chart
                        </Text>
                        {selectedRecommendation && (
                          <Tag
                            color="blue"
                            style={{
                              borderRadius: '6px',
                              fontSize: '11px',
                              fontWeight: 500
                            }}
                          >
                            AI: {(selectedRecommendation.confidence * 100).toFixed(0)}% confidence
                          </Tag>
                        )}
                      </div>
                    </div>

                    <AdvancedChart
                      data={currentResult.result.data.map((row, index) => ({ ...row, id: index }))}
                      config={currentVisualization}
                      onConfigChange={(config) => {
                        setVisualization(config);
                      }}
                      onExport={(format) => {
                        console.log('Export requested:', format);
                      }}
                    />
                  </Card>
                ) : (
                  <Card
                    style={{
                      borderRadius: '12px',
                      border: '2px dashed #d1d5db',
                      background: 'white',
                      minHeight: '500px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}
                    bodyStyle={{
                      padding: '40px',
                      textAlign: 'center',
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'center',
                      justifyContent: 'center',
                      height: '100%'
                    }}
                  >
                    <div style={{
                      fontSize: '64px',
                      marginBottom: '16px',
                      opacity: 0.4
                    }}>
                      📈
                    </div>
                    <Text style={{
                      fontSize: '18px',
                      color: '#4b5563',
                      fontWeight: 600,
                      marginBottom: '8px'
                    }}>
                      Create Your First Chart
                    </Text>
                    <Text style={{
                      fontSize: '14px',
                      color: '#6b7280',
                      marginBottom: '16px'
                    }}>
                      Use the Chart Builder or AI Suggestions to visualize your data
                    </Text>
                    <div style={{
                      display: 'flex',
                      gap: '8px',
                      flexWrap: 'wrap',
                      justifyContent: 'center'
                    }}>
                      <Tag color="blue">Bar Charts</Tag>
                      <Tag color="green">Line Charts</Tag>
                      <Tag color="orange">Pie Charts</Tag>
                      <Tag color="blue">Area Charts</Tag>
                      <Tag color="cyan">Scatter Plots</Tag>
                    </div>
                  </Card>
                )}
              </Col>
            </Row>
          </div>
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
              Charts & visualizations will appear here
            </Text>
            <Text style={{
              fontSize: '14px',
              color: '#6b7280',
              fontWeight: 400
            }}>
              Execute a query to see chart recommendations and visualizations
            </Text>
          </div>
        )}
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
          <div>
            <p>Prompt Details Panel temporarily disabled for debugging</p>
            <pre>{JSON.stringify(currentResult.promptDetails, null, 2)}</pre>
          </div>
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
    </div>
  );
};
