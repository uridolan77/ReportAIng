import React, { useState, useEffect } from 'react';
import {
  Card,
  List,
  Button,
  Space,
  Typography,
  Tag,
  Spin,
  Empty,
  Tooltip,
  Progress,
  Row,
  Col,
  Statistic,
  Badge,
  Collapse,
  Select
} from 'antd';
import {
  BulbOutlined,
  RiseOutlined,
  ExclamationCircleOutlined,
  LinkOutlined,
  BarChartOutlined,
  ReloadOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { insightsService, DataInsight, InsightRequest, ColumnInfo } from '../../services/insightsService';
import { QueryResponse } from '../../types/query';

const { Text } = Typography;
const { Panel } = Collapse;
const { Option } = Select;

interface DataInsightsPanelProps {
  queryResult: QueryResponse | null;
  onInsightAction?: (action: any) => void;
  autoGenerate?: boolean;
}

export const DataInsightsPanel: React.FC<DataInsightsPanelProps> = ({
  queryResult,
  onInsightAction,
  autoGenerate = true
}) => {
  const [insights, setInsights] = useState<DataInsight[]>([]);
  const [loading, setLoading] = useState(false);
  const [analysisDepth, setAnalysisDepth] = useState<'quick' | 'standard' | 'comprehensive'>('standard');
  const [filterType, setFilterType] = useState<string>('all');

  useEffect(() => {
    if (autoGenerate && queryResult?.result?.data && queryResult.result.data.length > 0) {
      generateInsights();
    }
  }, [queryResult, autoGenerate]);

  const generateInsights = async () => {
    if (!queryResult?.result?.data || queryResult.result.data.length === 0) return;

    setLoading(true);
    try {
      const columns: ColumnInfo[] = queryResult.result.metadata.columns?.map(col => ({
        name: col.name,
        type: inferColumnType(queryResult.result!.data, col.name),
        nullable: col.isNullable,
        description: col.description
      })) || [];

      const request: InsightRequest = {
        data: queryResult.result.data,
        columns,
        analysisDepth,
        timeColumn: findTimeColumn(columns),
        valueColumns: findValueColumns(columns)
      };

      const generatedInsights = await insightsService.generateInsights(request);
      setInsights(generatedInsights);
    } catch (error) {
      console.error('Failed to generate insights:', error);
    } finally {
      setLoading(false);
    }
  };

  const inferColumnType = (data: any[], columnName: string): 'numeric' | 'text' | 'date' | 'boolean' => {
    const sample = data.slice(0, 10).map(row => row[columnName]).filter(val => val != null);

    if (sample.length === 0) return 'text';

    // Check if all values are numbers
    if (sample.every(val => !isNaN(val) && typeof val === 'number')) {
      return 'numeric';
    }

    // Check if all values are dates
    if (sample.every(val => !isNaN(Date.parse(val)))) {
      return 'date';
    }

    // Check if all values are booleans
    if (sample.every(val => typeof val === 'boolean' || val === 'true' || val === 'false')) {
      return 'boolean';
    }

    return 'text';
  };

  const findTimeColumn = (columns: ColumnInfo[]): string | undefined => {
    return columns.find(col =>
      col.type === 'date' ||
      col.name.toLowerCase().includes('date') ||
      col.name.toLowerCase().includes('time')
    )?.name;
  };

  const findValueColumns = (columns: ColumnInfo[]): string[] => {
    return columns.filter(col => col.type === 'numeric').map(col => col.name);
  };

  const getInsightIcon = (type: string) => {
    switch (type) {
      case 'trend': return <RiseOutlined />;
      case 'anomaly': return <ExclamationCircleOutlined />;
      case 'correlation': return <LinkOutlined />;
      case 'pattern': return <BarChartOutlined />;
      case 'summary': return <BarChartOutlined />;
      case 'recommendation': return <BulbOutlined />;
      default: return <BulbOutlined />;
    }
  };

  const getInsightColor = (type: string) => {
    switch (type) {
      case 'trend': return '#1890ff';
      case 'anomaly': return '#ff4d4f';
      case 'correlation': return '#52c41a';
      case 'pattern': return '#722ed1';
      case 'summary': return '#13c2c2';
      case 'recommendation': return '#faad14';
      default: return '#8c8c8c';
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'high': return 'red';
      case 'medium': return 'orange';
      case 'low': return 'green';
      default: return 'default';
    }
  };

  const handleInsightAction = (insight: DataInsight, action: any) => {
    if (onInsightAction) {
      onInsightAction({ insight, action });
    }
  };

  const filteredInsights = insights.filter(insight => {
    if (filterType === 'all') return true;
    return insight.type === filterType;
  });

  const insightTypes = [...new Set(insights.map(insight => insight.type))];

  const renderInsightVisualization = (insight: DataInsight) => {
    if (!insight.visualization) return null;

    const { type, config } = insight.visualization;

    switch (type) {
      case 'metric':
        return (
          <Row gutter={[8, 8]} style={{ marginTop: '12px' }}>
            {config.metrics?.map((metric: any, index: number) => (
              <Col span={6} key={index}>
                <Statistic
                  title={metric.label}
                  value={metric.value}
                  precision={metric.format === 'number' ? 0 : 2}
                  valueStyle={{ fontSize: '14px' }}
                />
              </Col>
            ))}
          </Row>
        );

      case 'line':
      case 'bar':
      case 'scatter':
        return (
          <div style={{
            marginTop: '12px',
            padding: '8px',
            backgroundColor: '#f5f5f5',
            borderRadius: '4px',
            textAlign: 'center'
          }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              📊 {type.charAt(0).toUpperCase() + type.slice(1)} Chart Available
            </Text>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <Card
      title={
        <Space>
          <ThunderboltOutlined />
          <span>AI Data Insights</span>
          <Badge count={insights.length} style={{ backgroundColor: '#52c41a' }} />
        </Space>
      }
      extra={
        <Space>
          <Select
            size="small"
            value={filterType}
            onChange={setFilterType}
            style={{ width: 120 }}
          >
            <Option value="all">All Types</Option>
            {insightTypes.map(type => (
              <Option key={type} value={type}>
                {type.charAt(0).toUpperCase() + type.slice(1)}
              </Option>
            ))}
          </Select>

          <Select
            size="small"
            value={analysisDepth}
            onChange={setAnalysisDepth}
            style={{ width: 120 }}
          >
            <Option value="quick">Quick</Option>
            <Option value="standard">Standard</Option>
            <Option value="comprehensive">Deep</Option>
          </Select>

          <Button
            size="small"
            icon={<ReloadOutlined />}
            onClick={generateInsights}
            loading={loading}
          >
            Refresh
          </Button>
        </Space>
      }
      style={{ height: '100%' }}
    >
      {!queryResult?.result?.data ? (
        <Empty
          description="Execute a query to generate insights"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      ) : loading ? (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text>Analyzing your data...</Text>
          </div>
        </div>
      ) : filteredInsights.length === 0 ? (
        <Empty
          description="No insights found for this data"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      ) : (
        <List
          dataSource={filteredInsights}
          renderItem={(insight) => (
            <List.Item style={{ padding: '12px 0' }}>
              <Card
                size="small"
                style={{ width: '100%' }}
                bodyStyle={{ padding: '12px' }}
              >
                <Space direction="vertical" style={{ width: '100%' }}>
                  {/* Header */}
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Space>
                      <span style={{ color: getInsightColor(insight.type) }}>
                        {getInsightIcon(insight.type)}
                      </span>
                      <Text strong style={{ fontSize: '13px' }}>
                        {insight.title}
                      </Text>
                    </Space>
                    <Space size="small">
                      <Tag color={getSeverityColor(insight.severity)} style={{ fontSize: '11px' }}>
                        {insight.severity}
                      </Tag>
                      <Tooltip title={`Confidence: ${(insight.confidence * 100).toFixed(0)}%`}>
                        <Progress
                          type="circle"
                          size={20}
                          percent={insight.confidence * 100}
                          showInfo={false}
                          strokeColor={insight.confidence > 0.8 ? '#52c41a' : insight.confidence > 0.6 ? '#faad14' : '#ff4d4f'}
                        />
                      </Tooltip>
                    </Space>
                  </div>

                  {/* Description */}
                  <Text style={{ fontSize: '12px' }}>
                    {insight.description}
                  </Text>

                  {/* Visualization */}
                  {renderInsightVisualization(insight)}

                  {/* Actions */}
                  {insight.actions && insight.actions.length > 0 && (
                    <Space size="small" wrap>
                      {insight.actions.map((action) => (
                        <Button
                          key={action.id}
                          size="small"
                          type="link"
                          style={{ padding: '0 4px', height: 'auto', fontSize: '11px' }}
                          onClick={() => handleInsightAction(insight, action)}
                        >
                          {action.label}
                        </Button>
                      ))}
                    </Space>
                  )}

                  {/* Metadata */}
                  <Collapse ghost size="small">
                    <Panel
                      header={<Text type="secondary" style={{ fontSize: '10px' }}>Details</Text>}
                      key="details"
                      style={{ padding: 0 }}
                    >
                      <Space direction="vertical" size="small" style={{ width: '100%' }}>
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          <strong>Data Points:</strong> {insight.metadata.dataPoints.toLocaleString()}
                        </Text>
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          <strong>Columns:</strong> {insight.metadata.columns.join(', ')}
                        </Text>
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          <strong>Generated:</strong> {new Date(insight.metadata.generatedAt).toLocaleString()}
                        </Text>
                        {insight.metadata.timeRange && (
                          <Text type="secondary" style={{ fontSize: '10px' }}>
                            <strong>Time Range:</strong> {insight.metadata.timeRange}
                          </Text>
                        )}
                      </Space>
                    </Panel>
                  </Collapse>
                </Space>
              </Card>
            </List.Item>
          )}
        />
      )}

      {/* Summary Stats */}
      {insights.length > 0 && (
        <div style={{
          marginTop: '16px',
          padding: '12px',
          backgroundColor: '#f9f9f9',
          borderRadius: '6px'
        }}>
          <Row gutter={[16, 8]}>
            <Col span={8}>
              <Statistic
                title="Total Insights"
                value={insights.length}
                valueStyle={{ fontSize: '16px' }}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="High Priority"
                value={insights.filter(i => i.severity === 'high').length}
                valueStyle={{ fontSize: '16px', color: '#ff4d4f' }}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Avg Confidence"
                value={insights.reduce((sum, i) => sum + i.confidence, 0) / insights.length * 100}
                precision={0}
                suffix="%"
                valueStyle={{ fontSize: '16px' }}
              />
            </Col>
          </Row>
        </div>
      )}
    </Card>
  );
};
