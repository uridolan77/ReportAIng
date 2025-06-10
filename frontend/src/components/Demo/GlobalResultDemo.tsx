/**
 * Global Result Demo Component
 * Demonstrates how current results are available across all pages
 */

import React from 'react';
import {
  Card,
  Row,
  Col,
  Typography,
  Tag,
  Space,
  Button,
  Statistic,
  Alert,
  Divider,
  Timeline,
  Badge
} from 'antd';
import {
  DatabaseOutlined,
  BarChartOutlined,
  ShareAltOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import {
  useCurrentResult,
  useVisualizationResult,
  useDashboardResult,
  useExportResult,
  useResultAvailability,
  useResultDebugInfo
} from '../../hooks/useCurrentResult';
import {
  useGlobalResult,
  useGlobalResultHistory,
  usePageResults
} from '../../stores/globalResultStore';

const { Title, Text, Paragraph } = Typography;

export const GlobalResultDemo: React.FC = () => {
  // Demonstrate different ways to access current results
  const mainResult = useCurrentResult('main');
  const visualizationResult = useVisualizationResult();
  const dashboardResult = useDashboardResult();
  const exportResult = useExportResult();
  const availability = useResultAvailability();
  const debugInfo = useResultDebugInfo();
  
  // Global result management
  const globalResult = useGlobalResult();
  const { history, getRecentResults } = useGlobalResultHistory();
  const pageResults = usePageResults('demo');

  const formatTimestamp = (timestamp: number) => {
    return new Date(timestamp).toLocaleString();
  };

  const getSourceColor = (source: string) => {
    switch (source) {
      case 'active': return 'green';
      case 'global': return 'blue';
      case 'page': return 'orange';
      case 'shared': return 'purple';
      default: return 'default';
    }
  };

  return (
    <div style={{ padding: '24px', background: '#f5f5f5', minHeight: '100vh' }}>
      <div style={{ maxWidth: '1400px', margin: '0 auto' }}>
        <Title level={2} style={{ marginBottom: '24px', textAlign: 'center' }}>
          🌐 Global Result System Demo
        </Title>
        
        <Paragraph style={{ textAlign: 'center', fontSize: '16px', marginBottom: '32px' }}>
          This demonstrates how query results are automatically available across all pages and components.
        </Paragraph>

        {/* Current Result Status */}
        <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
          <Col xs={24} lg={6}>
            <Card>
              <Statistic
                title="Current Result Status"
                value={mainResult.hasResult ? 'Available' : 'None'}
                prefix={mainResult.hasResult ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
                valueStyle={{ color: mainResult.hasResult ? '#3f8600' : '#cf1322' }}
              />
              <div style={{ marginTop: '8px' }}>
                <Tag color={getSourceColor(mainResult.source)}>
                  Source: {mainResult.source}
                </Tag>
              </div>
            </Card>
          </Col>
          
          <Col xs={24} lg={6}>
            <Card>
              <Statistic
                title="Data Rows"
                value={visualizationResult.dataLength}
                prefix={<DatabaseOutlined />}
              />
              <div style={{ marginTop: '8px' }}>
                <Tag color={visualizationResult.hasVisualizableData ? 'green' : 'default'}>
                  {visualizationResult.hasVisualizableData ? 'Visualizable' : 'No Data'}
                </Tag>
              </div>
            </Card>
          </Col>
          
          <Col xs={24} lg={6}>
            <Card>
              <Statistic
                title="Columns"
                value={visualizationResult.columnCount}
                prefix={<BarChartOutlined />}
              />
              <div style={{ marginTop: '8px' }}>
                <Tag color={visualizationResult.isGamingData ? 'blue' : 'default'}>
                  {visualizationResult.isGamingData ? 'Gaming Data' : 'General Data'}
                </Tag>
              </div>
            </Card>
          </Col>
          
          <Col xs={24} lg={6}>
            <Card>
              <Statistic
                title="History Items"
                value={history.length}
                prefix={<ClockCircleOutlined />}
              />
              <div style={{ marginTop: '8px' }}>
                <Tag color="purple">
                  Recent: {getRecentResults(5).length}
                </Tag>
              </div>
            </Card>
          </Col>
        </Row>

        {/* Feature Availability */}
        <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
          <Col xs={24} lg={12}>
            <Card title="Feature Availability" extra={<Badge status="processing" text="Live" />}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Can Visualize:</Text>
                  <Tag color={availability.canVisualize ? 'green' : 'red'}>
                    {availability.canVisualize ? 'Yes' : 'No'}
                  </Tag>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Can Export:</Text>
                  <Tag color={availability.canExport ? 'green' : 'red'}>
                    {availability.canExport ? 'Yes' : 'No'}
                  </Tag>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Can Create Dashboard:</Text>
                  <Tag color={availability.canCreateDashboard ? 'green' : 'red'}>
                    {availability.canCreateDashboard ? 'Yes' : 'No'}
                  </Tag>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Has Numeric Data:</Text>
                  <Tag color={availability.hasNumericData ? 'green' : 'red'}>
                    {availability.hasNumericData ? 'Yes' : 'No'}
                  </Tag>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Has Categorical Data:</Text>
                  <Tag color={availability.hasCategoricalData ? 'green' : 'red'}>
                    {availability.hasCategoricalData ? 'Yes' : 'No'}
                  </Tag>
                </div>
              </Space>
            </Card>
          </Col>
          
          <Col xs={24} lg={12}>
            <Card title="Page-Specific Results" extra={<ShareAltOutlined />}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <div>
                  <Text strong>Visualization Page:</Text>
                  <div style={{ marginTop: '4px' }}>
                    <Tag color={visualizationResult.hasResult ? 'green' : 'default'}>
                      {visualizationResult.hasResult ? 'Has Data' : 'No Data'}
                    </Tag>
                    {visualizationResult.hasResult && (
                      <Text type="secondary">
                        {visualizationResult.dataLength} rows
                      </Text>
                    )}
                  </div>
                </div>
                
                <div>
                  <Text strong>Dashboard Page:</Text>
                  <div style={{ marginTop: '4px' }}>
                    <Tag color={dashboardResult.canCreateDashboard ? 'green' : 'default'}>
                      {dashboardResult.canCreateDashboard ? 'Ready' : 'Not Ready'}
                    </Tag>
                    {dashboardResult.suggestedChartTypes.length > 0 && (
                      <Text type="secondary">
                        {dashboardResult.suggestedChartTypes.length} chart types
                      </Text>
                    )}
                  </div>
                </div>
                
                <div>
                  <Text strong>Export Page:</Text>
                  <div style={{ marginTop: '4px' }}>
                    <Tag color={exportResult.canExport ? 'green' : 'default'}>
                      {exportResult.canExport ? 'Can Export' : 'Cannot Export'}
                    </Tag>
                    {exportResult.hasLargeDataset && (
                      <Tag color="orange">Large Dataset</Tag>
                    )}
                  </div>
                </div>
              </Space>
            </Card>
          </Col>
        </Row>

        {/* Current Result Details */}
        {mainResult.hasResult && (
          <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
            <Col span={24}>
              <Card title="Current Result Details">
                <Row gutter={[16, 16]}>
                  <Col xs={24} lg={12}>
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div>
                        <Text strong>Query:</Text>
                        <div style={{ marginTop: '4px' }}>
                          <Text code>{mainResult.query}</Text>
                        </div>
                      </div>
                      <div>
                        <Text strong>Last Updated:</Text>
                        <div style={{ marginTop: '4px' }}>
                          <Text>{formatTimestamp(mainResult.lastUpdated)}</Text>
                        </div>
                      </div>
                      <div>
                        <Text strong>Source:</Text>
                        <div style={{ marginTop: '4px' }}>
                          <Tag color={getSourceColor(mainResult.source)}>
                            {mainResult.source}
                          </Tag>
                        </div>
                      </div>
                    </Space>
                  </Col>
                  <Col xs={24} lg={12}>
                    {mainResult.metadata && (
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text strong>Metadata:</Text>
                        </div>
                        {mainResult.metadata.pageSource && (
                          <div>
                            <Text>Page Source: </Text>
                            <Tag>{mainResult.metadata.pageSource}</Tag>
                          </div>
                        )}
                        {mainResult.metadata.sessionId && (
                          <div>
                            <Text>Session ID: </Text>
                            <Text code>{mainResult.metadata.sessionId.substring(0, 8)}...</Text>
                          </div>
                        )}
                      </Space>
                    )}
                  </Col>
                </Row>
              </Card>
            </Col>
          </Row>
        )}

        {/* Result History */}
        <Row gutter={[24, 24]}>
          <Col xs={24} lg={12}>
            <Card title="Recent Result History" extra={<Badge count={history.length} />}>
              {history.length > 0 ? (
                <Timeline
                  items={history.slice(0, 5).map((item, index) => ({
                    color: getSourceColor(item.source),
                    children: (
                      <div>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Text strong>{item.query.substring(0, 40)}...</Text>
                          <Tag color={getSourceColor(item.source)} size="small">
                            {item.source}
                          </Tag>
                        </div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {formatTimestamp(item.timestamp)}
                        </Text>
                      </div>
                    )
                  }))}
                />
              ) : (
                <Text type="secondary">No history available</Text>
              )}
            </Card>
          </Col>
          
          <Col xs={24} lg={12}>
            <Card title="Debug Information">
              <Space direction="vertical" style={{ width: '100%' }}>
                <div>
                  <Text strong>Active Result:</Text>
                  <div style={{ marginLeft: '16px', marginTop: '4px' }}>
                    <Text>Has Result: {debugInfo.activeResult.hasResult ? 'Yes' : 'No'}</Text><br />
                    <Text>Is Expired: {debugInfo.activeResult.isExpired ? 'Yes' : 'No'}</Text><br />
                    <Text>Query: {debugInfo.activeResult.query}</Text>
                  </div>
                </div>
                
                <Divider />
                
                <div>
                  <Text strong>Global Result:</Text>
                  <div style={{ marginLeft: '16px', marginTop: '4px' }}>
                    <Text>Has Result: {debugInfo.globalResult.hasResult ? 'Yes' : 'No'}</Text><br />
                    <Text>Source: {debugInfo.globalResult.source || 'N/A'}</Text><br />
                    <Text>Page Source: {debugInfo.globalResult.pageSource || 'N/A'}</Text>
                  </div>
                </div>
                
                <Divider />
                
                <div>
                  <Text strong>Recommendations:</Text>
                  <div style={{ marginLeft: '16px', marginTop: '4px' }}>
                    <Text>Preferred Source: </Text>
                    <Tag color={getSourceColor(debugInfo.recommendations.preferredSource)}>
                      {debugInfo.recommendations.preferredSource}
                    </Tag><br />
                    <Text>Data Availability: {debugInfo.recommendations.dataAvailability}</Text><br />
                    <Text>Sync Status: {debugInfo.recommendations.syncStatus}</Text>
                  </div>
                </div>
              </Space>
            </Card>
          </Col>
        </Row>

        {/* Instructions */}
        <Row style={{ marginTop: '32px' }}>
          <Col span={24}>
            <Alert
              message="How to Use Global Results"
              description={
                <div>
                  <p><strong>1. Execute a Query:</strong> Run any query from the main interface - it will be automatically available across all pages.</p>
                  <p><strong>2. Navigate to Different Pages:</strong> Visit visualization, dashboard, or export pages - the current result will be available.</p>
                  <p><strong>3. Cross-Page Sharing:</strong> Results are automatically shared between pages with proper context and metadata.</p>
                  <p><strong>4. History Management:</strong> All results are stored in history for easy access and reuse.</p>
                  <p><strong>5. Smart Selection:</strong> The system automatically selects the best available result based on recency and context.</p>
                </div>
              }
              type="info"
              showIcon
            />
          </Col>
        </Row>
      </div>
    </div>
  );
};
