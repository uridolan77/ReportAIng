import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Spin,
  Alert,
  Badge,
  Progress,
  Tooltip,
  Tag,
  Divider,
  message
} from 'antd';
import {
  BulbOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DotChartOutlined,
  HeatMapOutlined,
  FunnelPlotOutlined,
  RadarChartOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
  WarningOutlined
} from '@ant-design/icons';
import {
  VisualizationRecommendation,
  AdvancedChartType,
  AdvancedVisualizationConfig
} from '../../types/visualization';
import advancedVisualizationService from '../../services/advancedVisualizationService';

const { Title, Text, Paragraph } = Typography;

interface VisualizationRecommendationsProps {
  data: any[];
  columns: any[];
  query: string;
  onRecommendationSelect?: (recommendation: VisualizationRecommendation) => void;
  onConfigGenerated?: (config: AdvancedVisualizationConfig) => void;
}

const VisualizationRecommendations: React.FC<VisualizationRecommendationsProps> = ({
  data,
  columns,
  query,
  onRecommendationSelect,
  onConfigGenerated
}) => {
  const [loading, setLoading] = useState(false);
  const [recommendations, setRecommendations] = useState<VisualizationRecommendation[]>([]);
  const [selectedRecommendation, setSelectedRecommendation] = useState<VisualizationRecommendation | null>(null);
  const [generatingConfig, setGeneratingConfig] = useState(false);
  const [lastFetchKey, setLastFetchKey] = useState<string>(''); // Track last fetch to prevent duplicates

  // Chart type icons mapping
  const getChartIcon = (chartType: AdvancedChartType) => {
    const iconMap: Record<string, React.ReactNode> = {
      'Bar': <BarChartOutlined />,
      'Line': <LineChartOutlined />,
      'Pie': <PieChartOutlined />,
      'Scatter': <DotChartOutlined />,
      'Bubble': <DotChartOutlined />,
      'Area': <LineChartOutlined />,
      'Heatmap': <HeatMapOutlined />,
      'Funnel': <FunnelPlotOutlined />,
      'Radar': <RadarChartOutlined />,
      'Timeline': <LineChartOutlined />,
      'Histogram': <BarChartOutlined />
    };
    return iconMap[chartType] || <BarChartOutlined />;
  };

  // Get confidence color
  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return '#52c41a';
    if (confidence >= 0.6) return '#faad14';
    return '#ff4d4f';
  };

  // Get confidence status for Progress component
  const getConfidenceStatus = (confidence: number): "success" | "exception" | "normal" | "active" => {
    if (confidence >= 0.8) return 'success';
    if (confidence >= 0.6) return 'normal';
    return 'exception';
  };

  // Fetch recommendations with duplicate prevention
  const fetchRecommendations = useCallback(async () => {
    if (!data.length || !query) return;

    // Create a unique key for this fetch request
    const fetchKey = `${query}-${data.length}-${columns.length}`;

    // Prevent duplicate fetches
    if (fetchKey === lastFetchKey || loading) {
      return;
    }

    setLastFetchKey(fetchKey);
    setLoading(true);

    try {
      // Create context string with current values
      const currentColumns = columns || [];
      const context = `Data contains ${data.length} rows and ${currentColumns.length} columns. Column types: ${currentColumns.map(c => `${c.name}(${c.type})`).join(', ')}`;

      const response = await advancedVisualizationService.getVisualizationRecommendations({
        query,
        context
      });

      if (response.success && response.recommendations) {
        setRecommendations(response.recommendations);
        message.success(`Found ${response.recommendations.length} AI-powered recommendations!`);
      } else {
        message.error(response.errorMessage || 'Failed to get recommendations');
      }
    } catch (error) {
      console.error('Error fetching recommendations:', error);
      message.error('Failed to fetch recommendations');
    } finally {
      setLoading(false);
    }
  }, [data.length, query, columns, lastFetchKey, loading]);

  // Generate configuration from recommendation
  const generateConfigFromRecommendation = useCallback(async (recommendation: VisualizationRecommendation) => {
    setGeneratingConfig(true);
    setSelectedRecommendation(recommendation);

    try {
      // Normalize chart type to proper case (capitalize first letter)
      const normalizedChartType = recommendation.chartType.charAt(0).toUpperCase() + recommendation.chartType.slice(1).toLowerCase();

      // Smart column detection for better data mapping
      const dateColumns = columns.filter(c =>
        c.type === 'date' ||
        c.type === 'datetime' ||
        c.type === 'timestamp' ||
        c.name.toLowerCase().includes('date') ||
        c.name.toLowerCase().includes('time')
      );

      const numericColumns = columns.filter(c =>
        c.type === 'number' ||
        c.type === 'integer' ||
        c.type === 'decimal' ||
        c.type === 'float' ||
        c.type === 'double' ||
        // Also check if the column name suggests it's numeric
        c.name.toLowerCase().includes('total') ||
        c.name.toLowerCase().includes('amount') ||
        c.name.toLowerCase().includes('revenue') ||
        c.name.toLowerCase().includes('count') ||
        c.name.toLowerCase().includes('sum')
      );

      const stringColumns = columns.filter(c => c.type === 'string' || c.type === 'text');

      // Determine best X-axis (prefer date, then string, then first column)
      let xAxisColumn = dateColumns[0]?.name || stringColumns[0]?.name || columns[0]?.name;

      // Determine best Y-axis (prefer numeric columns, prioritize revenue/amount columns)
      let yAxisColumn = numericColumns.find(c =>
        c.name.toLowerCase().includes('revenue') ||
        c.name.toLowerCase().includes('amount') ||
        c.name.toLowerCase().includes('total')
      )?.name || numericColumns[0]?.name || columns[1]?.name;

      // Fallback: If no proper columns detected, analyze the actual data
      if ((dateColumns.length === 0 || numericColumns.length === 0) && data.length > 0) {
        const sampleRow = data[0];
        const dataKeys = Object.keys(sampleRow).filter(key => key !== 'id'); // Exclude DataTable id

        // Try to detect date columns from actual data
        const detectedDateColumns = dataKeys.filter(key => {
          const value = sampleRow[key];
          return (typeof value === 'string' && (
            value.includes('T') ||
            (value.includes('-') && value.length >= 10) ||
            key.toLowerCase().includes('date')
          )) || key.toLowerCase() === 'date';
        });

        // Try to detect numeric columns from actual data
        const detectedNumericColumns = dataKeys.filter(key => {
          const value = sampleRow[key];
          return (typeof value === 'number' ||
                 (typeof value === 'string' && !isNaN(parseFloat(value)))) &&
                 key !== 'id' && // Exclude DataTable id
                 !key.toLowerCase().includes('date'); // Exclude date columns
        });

        // Override with detected columns if original detection failed
        if (dateColumns.length === 0 && detectedDateColumns.length > 0) {
          xAxisColumn = detectedDateColumns[0];
        }
        if (numericColumns.length === 0 && detectedNumericColumns.length > 0) {
          yAxisColumn = detectedNumericColumns.find(col =>
            col.toLowerCase().includes('revenue') ||
            col.toLowerCase().includes('netrevenue') ||
            col.toLowerCase().includes('total')
          ) || detectedNumericColumns[0];
        }

        console.log('Fallback detection:', {
          detectedDateColumns,
          detectedNumericColumns,
          finalXAxis: xAxisColumn,
          finalYAxis: yAxisColumn
        });
      }

      // Special handling for known data structures
      if (data.length > 0) {
        const dataKeys = Object.keys(data[0]);

        // Handle the specific case of your daily actions data
        if (dataKeys.includes('Date') && dataKeys.includes('NetRevenue')) {
          xAxisColumn = 'Date';
          yAxisColumn = 'NetRevenue';
        } else if (dataKeys.includes('Date') && dataKeys.includes('TotalDeposits')) {
          xAxisColumn = 'Date';
          yAxisColumn = 'TotalDeposits';
        }
      }

      console.log('Chart mapping:', {
        availableColumns: columns.map(c => ({ name: c.name, type: c.type })),
        selectedXAxis: xAxisColumn,
        selectedYAxis: yAxisColumn,
        dateColumns: dateColumns.map(c => c.name),
        numericColumns: numericColumns.map(c => c.name),
        sampleDataRow: data[0],
        dataLength: data.length,
        dataKeys: data.length > 0 ? Object.keys(data[0]) : []
      });

      // Create a basic configuration based on the recommendation
      const config: AdvancedVisualizationConfig = {
        type: 'advanced',
        chartType: normalizedChartType as AdvancedChartType,
        title: `${normalizedChartType} Chart - ${query.substring(0, 50)}...`,
        config: recommendation.suggestedConfig,
        xAxis: xAxisColumn,
        yAxis: yAxisColumn,
        animation: {
          enabled: true,
          duration: 1000,
          easing: 'ease-in-out',
          delayByCategory: false,
          delayIncrement: 100,
          animateOnDataChange: true,
          animatedProperties: ['opacity', 'transform']
        },
        interaction: {
          enableZoom: true,
          enablePan: true,
          enableBrush: false,
          enableCrosshair: true,
          enableTooltip: true,
          enableLegendToggle: true,
          enableDataPointSelection: true,
          enableDrillDown: false,
          tooltip: {
            enabled: true,
            position: 'auto',
            displayFields: columns.slice(0, 3).map(c => c.name),
            showStatistics: true,
            enableHtml: false
          }
        },
        theme: {
          name: 'default',
          darkMode: false,
          colors: {
            primary: ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2'],
            secondary: ['#d9d9d9', '#bfbfbf'],
            background: '#ffffff',
            text: '#000000',
            grid: '#f0f0f0',
            axis: '#d9d9d9'
          }
        },
        performance: {
          enableVirtualization: data.length > 10000,
          virtualizationThreshold: 10000,
          enableLazyLoading: true,
          enableCaching: true,
          cacheTTL: 300,
          enableWebGL: data.length > 50000,
          maxDataPoints: 100000
        },
        accessibility: {
          enabled: true,
          highContrast: false,
          screenReaderSupport: true,
          keyboardNavigation: true,
          ariaLabels: [`${recommendation.chartType} chart showing ${query}`],
          colorBlindFriendly: true
        }
      };

      onConfigGenerated?.(config);
      onRecommendationSelect?.(recommendation);
      message.success(`Generated ${recommendation.chartType} configuration!`);
    } catch (error) {
      console.error('Error generating config:', error);
      message.error('Failed to generate configuration');
    } finally {
      setGeneratingConfig(false);
    }
  }, [columns, data.length, query, onConfigGenerated, onRecommendationSelect]);

  // Auto-fetch recommendations when data changes (with debounce)
  useEffect(() => {
    if (data.length > 0 && query) {
      const timeoutId = setTimeout(() => {
        fetchRecommendations();
      }, 500); // 500ms debounce

      return () => clearTimeout(timeoutId);
    }
  }, [data.length, query]); // Remove fetchRecommendations from dependencies to prevent infinite loop

  const renderRecommendationCard = (recommendation: VisualizationRecommendation, index: number) => (
    <Col span={8} key={index}>
      <Card
        size="small"
        hoverable
        className={selectedRecommendation?.chartType === recommendation.chartType ? 'selected-recommendation' : ''}
        title={
          <Space>
            {getChartIcon(recommendation.chartType)}
            <Text strong>{recommendation.chartType}</Text>
            <Badge
              count={`${(recommendation.confidence * 100).toFixed(0)}%`}
              style={{ backgroundColor: getConfidenceColor(recommendation.confidence) }}
            />
          </Space>
        }
        extra={
          <Button
            size="small"
            type="primary"
            loading={generatingConfig && selectedRecommendation?.chartType === recommendation.chartType}
            onClick={() => generateConfigFromRecommendation(recommendation)}
            icon={<CheckCircleOutlined />}
          >
            Apply
          </Button>
        }
        actions={[
          <Tooltip title="Performance Estimate">
            <Space>
              <InfoCircleOutlined />
              <Text type="secondary">
                {recommendation.estimatedPerformance.estimatedRenderTime}
              </Text>
            </Space>
          </Tooltip>,
          <Tooltip title="Memory Usage">
            <Space>
              <WarningOutlined />
              <Text type="secondary">
                {recommendation.estimatedPerformance.memoryUsageMB}MB
              </Text>
            </Space>
          </Tooltip>
        ]}
      >
        <div style={{ marginBottom: 12 }}>
          <Progress
            percent={recommendation.confidence * 100}
            size="small"
            status={getConfidenceStatus(recommendation.confidence)}
            format={(percent) => `${percent?.toFixed(0)}% confidence`}
          />
        </div>

        <div style={{ marginBottom: 8 }}>
          <Text strong>Best for:</Text>
          <br />
          <Text type="secondary">{recommendation.bestFor}</Text>
        </div>

        <div style={{ marginBottom: 8 }}>
          <Text strong>AI Reasoning:</Text>
          <br />
          <Paragraph
            type="secondary"
            style={{ fontSize: 12, margin: 0 }}
            ellipsis={{ rows: 2, expandable: true }}
          >
            {recommendation.reasoning}
          </Paragraph>
        </div>

        {recommendation.limitations.length > 0 && (
          <div>
            <Text strong>Limitations:</Text>
            <div style={{ marginTop: 4 }}>
              {recommendation.limitations.slice(0, 2).map((limitation, i) => (
                <Tag key={i} color="orange" style={{ fontSize: 10, marginBottom: 2 }}>
                  {limitation}
                </Tag>
              ))}
              {recommendation.limitations.length > 2 && (
                <Tag color="default" style={{ fontSize: 10 }}>
                  +{recommendation.limitations.length - 2} more
                </Tag>
              )}
            </div>
          </div>
        )}

        <Divider style={{ margin: '8px 0' }} />

        <div style={{ fontSize: 11, color: '#999' }}>
          <Space split={<Divider type="vertical" />}>
            <span>Max: {recommendation.estimatedPerformance.recommendedMaxDataPoints.toLocaleString()}</span>
            <span>{recommendation.estimatedPerformance.requiresWebGL ? 'WebGL' : 'Canvas'}</span>
            <span>{recommendation.estimatedPerformance.requiresSampling ? 'Sampling' : 'Full'}</span>
          </Space>
        </div>
      </Card>
    </Col>
  );

  return (
    <div className="visualization-recommendations">
      <Card
        title={
          <Space>
            <BulbOutlined />
            <Title level={4} style={{ margin: 0 }}>AI-Powered Chart Recommendations</Title>
            <Badge count={recommendations.length} style={{ backgroundColor: '#1890ff' }} />
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<BulbOutlined />}
            onClick={fetchRecommendations}
            loading={loading}
            disabled={!data.length || !query}
          >
            Get New Recommendations
          </Button>
        }
      >
        {!data.length || !query ? (
          <Alert
            message="No Data Available"
            description="Execute a query first to get AI-powered visualization recommendations."
            type="info"
            showIcon
          />
        ) : loading ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Spin size="large" />
            <br />
            <Text type="secondary">Analyzing your data and generating AI recommendations...</Text>
          </div>
        ) : recommendations.length === 0 ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <BulbOutlined style={{ fontSize: 48, color: '#ccc' }} />
            <br />
            <Text type="secondary">No recommendations available. Try executing a query first.</Text>
          </div>
        ) : (
          <>
            <div style={{ marginBottom: 16 }}>
              <Alert
                message="AI Analysis Complete"
                description={`Found ${recommendations.length} visualization recommendations based on your data characteristics and query context.`}
                type="success"
                showIcon
                closable
              />
            </div>

            <Row gutter={[16, 16]}>
              {recommendations.map((recommendation, index) =>
                renderRecommendationCard(recommendation, index)
              )}
            </Row>
          </>
        )}
      </Card>

      <style>{`
        .selected-recommendation {
          border-color: #1890ff !important;
          box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2) !important;
        }
      `}</style>
    </div>
  );
};

export default VisualizationRecommendations;
