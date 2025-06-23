import React, { useState } from 'react';
import { Card, Row, Col, Tabs, Button, Space, Alert, Typography } from 'antd';
import { 
  BarChartOutlined, 
  ThunderboltOutlined, 
  EyeOutlined,
  ApiOutlined 
} from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  ProcessFlowAnalyticsDashboard,
  ProcessFlowTokenUsage,
  ProcessFlowSessionViewer,
  TokenUsageAnalyzer,
  useProcessFlowSession,
  useTokenUsageDashboard,
  useAnalyticsDashboard
} from './index';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

/**
 * ProcessFlowAnalyticsExample - Comprehensive example showing how to use ProcessFlow analytics
 * 
 * This component demonstrates:
 * - Main analytics dashboard
 * - Token usage analysis
 * - Session viewing with transparency
 * - Real-time monitoring
 * - Export capabilities
 */
export const ProcessFlowAnalyticsExample: React.FC = () => {
  const [selectedSessionId, setSelectedSessionId] = useState<string>('');
  const [activeTab, setActiveTab] = useState('dashboard');

  // Example session data
  const exampleSessionId = 'session-123-example';
  
  // Hook usage examples
  const tokenDashboard = useTokenUsageDashboard({
    startDate: dayjs().subtract(30, 'days').toISOString(),
    endDate: dayjs().toISOString()
  });

  const analyticsDashboard = useAnalyticsDashboard({
    startDate: dayjs().subtract(7, 'days').toISOString(),
    endDate: dayjs().toISOString(),
    includeRealTime: true
  });

  const processFlowSession = useProcessFlowSession(selectedSessionId);

  return (
    <div style={{ padding: '24px' }}>
      <Card style={{ marginBottom: 24 }}>
        <Title level={2}>
          <BarChartOutlined /> ProcessFlow Analytics System
        </Title>
        <Paragraph>
          This example demonstrates the comprehensive ProcessFlow analytics system that provides:
        </Paragraph>
        <ul>
          <li><strong>Token Usage Analytics:</strong> Track token consumption, costs, and efficiency</li>
          <li><strong>Performance Monitoring:</strong> Monitor session performance and processing times</li>
          <li><strong>Transparency Insights:</strong> Detailed AI processing transparency and confidence metrics</li>
          <li><strong>Real-time Monitoring:</strong> Live system status and active session tracking</li>
          <li><strong>Export Capabilities:</strong> Export analytics data in multiple formats</li>
        </ul>
        
        <Alert
          message="Migration Complete"
          description="The system has been successfully migrated from legacy transparency tracking to the new ProcessFlow system with enhanced analytics capabilities."
          type="success"
          showIcon
          style={{ marginTop: 16 }}
        />
      </Card>

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Analytics Dashboard" key="dashboard">
          <ProcessFlowAnalyticsDashboard
            defaultTimeRange={[dayjs().subtract(30, 'days'), dayjs()]}
            showRealTime={true}
            testId="example-analytics-dashboard"
          />
        </TabPane>

        <TabPane tab="Token Usage" key="tokens">
          <ProcessFlowTokenUsage
            defaultTimeRange={[dayjs().subtract(30, 'days'), dayjs()]}
            showUserBreakdown={true}
            showModelBreakdown={true}
            testId="example-token-usage"
          />
        </TabPane>

        <TabPane tab="Session Viewer" key="session">
          <Card>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Title level={4}>ProcessFlow Session Viewer</Title>
                <Text type="secondary">
                  View detailed ProcessFlow session data with step-by-step transparency
                </Text>
              </div>
              
              <Space>
                <Button
                  type="primary"
                  icon={<EyeOutlined />}
                  onClick={() => setSelectedSessionId(exampleSessionId)}
                >
                  Load Example Session
                </Button>
                <Button
                  onClick={() => setSelectedSessionId('')}
                >
                  Clear
                </Button>
              </Space>

              {selectedSessionId && (
                <ProcessFlowSessionViewer
                  sessionId={selectedSessionId}
                  showDetailedSteps={true}
                  showLogs={true}
                  showTransparency={true}
                  testId="example-session-viewer"
                />
              )}

              {!selectedSessionId && (
                <Alert
                  message="No Session Selected"
                  description="Click 'Load Example Session' to view ProcessFlow session details"
                  type="info"
                  showIcon
                />
              )}
            </Space>
          </Card>
        </TabPane>

        <TabPane tab="Token Analyzer" key="analyzer">
          <Card>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Title level={4}>Token Usage Analyzer</Title>
                <Text type="secondary">
                  Analyze token usage patterns with ProcessFlow transparency data
                </Text>
              </div>

              {processFlowSession.session && processFlowSession.transparency ? (
                <TokenUsageAnalyzer
                  processFlowSteps={processFlowSession.steps}
                  transparency={processFlowSession.transparency}
                  showCostAnalysis={true}
                  showOptimizationSuggestions={true}
                  testId="example-token-analyzer"
                />
              ) : (
                <Alert
                  message="ProcessFlow Data Required"
                  description="Select a session in the Session Viewer tab to analyze token usage"
                  type="info"
                  showIcon
                />
              )}
            </Space>
          </Card>
        </TabPane>

        <TabPane tab="API Integration" key="api">
          <Card>
            <Title level={4}>
              <ApiOutlined /> API Integration Examples
            </Title>
            
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Title level={5}>Hook Usage Examples</Title>
                <Paragraph>
                  The ProcessFlow analytics system provides comprehensive React hooks for easy integration:
                </Paragraph>
                
                <pre style={{ 
                  background: '#f5f5f5', 
                  padding: '16px', 
                  borderRadius: '4px',
                  overflow: 'auto'
                }}>
{`// Token usage dashboard
const tokenDashboard = useTokenUsageDashboard({
  startDate: dayjs().subtract(30, 'days').toISOString(),
  endDate: dayjs().toISOString()
});

// Analytics dashboard with real-time data
const analyticsDashboard = useAnalyticsDashboard({
  includeRealTime: true
});

// ProcessFlow session details
const session = useProcessFlowSession(sessionId);

// Query with ProcessFlow transparency
const queryWithTransparency = useQueryWithTransparency(queryId);`}
                </pre>
              </div>

              <div>
                <Title level={5}>Current Hook States</Title>
                <Row gutter={[16, 16]}>
                  <Col span={12}>
                    <Card size="small" title="Token Dashboard">
                      <Text>Loading: {tokenDashboard.isLoading ? 'Yes' : 'No'}</Text><br />
                      <Text>Error: {tokenDashboard.error ? 'Yes' : 'No'}</Text><br />
                      <Text>Statistics: {tokenDashboard.statistics ? 'Loaded' : 'None'}</Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" title="Analytics Dashboard">
                      <Text>Loading: {analyticsDashboard.isLoading ? 'Yes' : 'No'}</Text><br />
                      <Text>Error: {analyticsDashboard.error ? 'Yes' : 'No'}</Text><br />
                      <Text>Dashboard: {analyticsDashboard.dashboard ? 'Loaded' : 'None'}</Text>
                    </Card>
                  </Col>
                </Row>
              </div>

              <div>
                <Title level={5}>Migration Benefits</Title>
                <ul>
                  <li><strong>Unified Data Model:</strong> Single ProcessFlow system replaces multiple legacy systems</li>
                  <li><strong>Enhanced Transparency:</strong> Detailed step-by-step AI processing insights</li>
                  <li><strong>Real-time Analytics:</strong> Live monitoring and performance tracking</li>
                  <li><strong>Cost Optimization:</strong> Comprehensive token usage and cost analysis</li>
                  <li><strong>Better Performance:</strong> Optimized data structures and API endpoints</li>
                  <li><strong>Backward Compatibility:</strong> Legacy components continue to work during transition</li>
                </ul>
              </div>
            </Space>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  );
};

export default ProcessFlowAnalyticsExample;
