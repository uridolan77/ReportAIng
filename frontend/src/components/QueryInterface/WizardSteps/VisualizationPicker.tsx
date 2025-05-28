import React, { useState } from 'react';
import { Card, Row, Col, Typography, Space, Tag, Radio } from 'antd';
import {
  TableOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DotChartOutlined,
  AreaChartOutlined,
  HeatMapOutlined,
  FunnelPlotOutlined
} from '@ant-design/icons';
import { QueryBuilderData } from '../QueryWizard';

const { Title, Text, Paragraph } = Typography;

interface VisualizationType {
  type: string;
  name: string;
  description: string;
  icon: React.ReactNode;
  bestFor: string[];
  recommended?: boolean;
  dataRequirements: {
    minMetrics: number;
    maxMetrics?: number;
    supportsCategories: boolean;
    supportsTime: boolean;
  };
}

interface VisualizationPickerProps {
  data: QueryBuilderData;
  onChange: (data: Partial<QueryBuilderData>) => void;
  onNext?: () => void;
  onPrevious?: () => void;
}

export const VisualizationPicker: React.FC<VisualizationPickerProps> = ({
  data,
  onChange,
}) => {
  const [selectedType, setSelectedType] = useState<string>(data.visualization?.type || '');

  const visualizationTypes: VisualizationType[] = [
    {
      type: 'table',
      name: 'Data Table',
      description: 'Display data in rows and columns with sorting and filtering',
      icon: <TableOutlined />,
      bestFor: ['Detailed data review', 'Exact values', 'Multiple metrics'],
      recommended: true,
      dataRequirements: {
        minMetrics: 1,
        supportsCategories: true,
        supportsTime: true
      }
    },
    {
      type: 'bar',
      name: 'Bar Chart',
      description: 'Compare values across categories with vertical or horizontal bars',
      icon: <BarChartOutlined />,
      bestFor: ['Comparing categories', 'Ranking data', 'Single metric analysis'],
      recommended: true,
      dataRequirements: {
        minMetrics: 1,
        maxMetrics: 3,
        supportsCategories: true,
        supportsTime: false
      }
    },
    {
      type: 'line',
      name: 'Line Chart',
      description: 'Show trends and changes over time with connected data points',
      icon: <LineChartOutlined />,
      bestFor: ['Time series data', 'Trend analysis', 'Multiple metrics over time'],
      dataRequirements: {
        minMetrics: 1,
        maxMetrics: 5,
        supportsCategories: false,
        supportsTime: true
      }
    },
    {
      type: 'pie',
      name: 'Pie Chart',
      description: 'Show proportions and percentages of a whole',
      icon: <PieChartOutlined />,
      bestFor: ['Part-to-whole relationships', 'Percentage breakdown', 'Limited categories'],
      dataRequirements: {
        minMetrics: 1,
        maxMetrics: 1,
        supportsCategories: true,
        supportsTime: false
      }
    },
    {
      type: 'scatter',
      name: 'Scatter Plot',
      description: 'Explore relationships between two numeric variables',
      icon: <DotChartOutlined />,
      bestFor: ['Correlation analysis', 'Outlier detection', 'Two metric comparison'],
      dataRequirements: {
        minMetrics: 2,
        maxMetrics: 2,
        supportsCategories: false,
        supportsTime: false
      }
    },
    {
      type: 'area',
      name: 'Area Chart',
      description: 'Show cumulative totals and trends over time',
      icon: <AreaChartOutlined />,
      bestFor: ['Cumulative data', 'Stacked metrics', 'Time series with volume'],
      dataRequirements: {
        minMetrics: 1,
        maxMetrics: 4,
        supportsCategories: false,
        supportsTime: true
      }
    },
    {
      type: 'heatmap',
      name: 'Heatmap',
      description: 'Visualize data density and patterns with color intensity',
      icon: <HeatMapOutlined />,
      bestFor: ['Pattern recognition', 'Correlation matrices', 'Time-based patterns'],
      dataRequirements: {
        minMetrics: 1,
        maxMetrics: 1,
        supportsCategories: true,
        supportsTime: true
      }
    },
    {
      type: 'funnel',
      name: 'Funnel Chart',
      description: 'Show progressive reduction of data through stages',
      icon: <FunnelPlotOutlined />,
      bestFor: ['Conversion analysis', 'Process stages', 'Sequential data'],
      dataRequirements: {
        minMetrics: 1,
        maxMetrics: 1,
        supportsCategories: true,
        supportsTime: false
      }
    }
  ];

  const getRecommendedVisualizations = (): VisualizationType[] => {
    const selectedMetrics = data.metrics?.selected || [];
    const hasTimeData = data.dataSource?.table === 'tbl_Daily_actions' &&
                       (selectedMetrics.includes('ActionDate') || data.filters?.conditions?.some(c => c.column === 'ActionDate'));
    const metricCount = selectedMetrics.length;

    return visualizationTypes.filter(viz => {
      // Check metric count requirements
      if (metricCount < viz.dataRequirements.minMetrics) return false;
      if (viz.dataRequirements.maxMetrics && metricCount > viz.dataRequirements.maxMetrics) return false;

      // Check time data compatibility
      if (hasTimeData && !viz.dataRequirements.supportsTime && viz.type !== 'table') return false;

      return true;
    }).sort((a, b) => {
      // Prioritize recommended visualizations
      if (a.recommended && !b.recommended) return -1;
      if (!a.recommended && b.recommended) return 1;
      return 0;
    });
  };

  const handleVisualizationSelect = (type: string) => {
    setSelectedType(type);
    const selectedViz = visualizationTypes.find(v => v.type === type);

    onChange({
      visualization: {
        type,
        config: {
          title: `${selectedViz?.name} Analysis`,
          interactive: true,
          responsive: true
        }
      }
    });
  };

  const recommendedViz = getRecommendedVisualizations();
  const otherViz = visualizationTypes.filter(v => !recommendedViz.includes(v));

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <Title level={4}>
          <BarChartOutlined /> Choose Visualization
        </Title>
        <Paragraph type="secondary">
          Select how you want to display your data. We've recommended the best options based on your selected metrics and filters.
        </Paragraph>
      </div>

      {recommendedViz.length > 0 && (
        <>
          <Title level={5} style={{ color: '#52c41a' }}>
            ✨ Recommended for Your Data
          </Title>
          <Radio.Group
            value={selectedType}
            onChange={(e) => handleVisualizationSelect(e.target.value)}
            style={{ width: '100%' }}
          >
            <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
              {recommendedViz.map((viz) => (
                <Col xs={24} sm={12} md={8} key={viz.type}>
                  <Card
                    hoverable
                    style={{
                      border: selectedType === viz.type ? '2px solid #1890ff' : '1px solid #d9d9d9',
                      backgroundColor: selectedType === viz.type ? '#f6ffed' : 'white',
                      cursor: 'pointer'
                    }}
                    onClick={() => handleVisualizationSelect(viz.type)}
                  >
                    <Radio value={viz.type} style={{ display: 'none' }} />
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div style={{ fontSize: '24px', color: '#1890ff' }}>
                          {viz.icon}
                        </div>
                        {viz.recommended && <Tag color="green">Recommended</Tag>}
                      </div>

                      <Title level={5} style={{ margin: 0 }}>
                        {viz.name}
                      </Title>

                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {viz.description}
                      </Text>

                      <div>
                        <Text strong style={{ fontSize: '11px' }}>Best for:</Text>
                        <div style={{ marginTop: '4px' }}>
                          {viz.bestFor.map((use, index) => (
                            <Tag key={index} color="blue" style={{ margin: '1px', fontSize: '11px' }}>
                              {use}
                            </Tag>
                          ))}
                        </div>
                      </div>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
          </Radio.Group>
        </>
      )}

      {otherViz.length > 0 && (
        <>
          <Title level={5}>Other Options</Title>
          <Radio.Group
            value={selectedType}
            onChange={(e) => handleVisualizationSelect(e.target.value)}
            style={{ width: '100%' }}
          >
            <Row gutter={[16, 16]}>
              {otherViz.map((viz) => (
                <Col xs={24} sm={12} md={8} key={viz.type}>
                  <Card
                    hoverable
                    style={{
                      border: selectedType === viz.type ? '2px solid #1890ff' : '1px solid #d9d9d9',
                      backgroundColor: selectedType === viz.type ? '#f6ffed' : 'white',
                      cursor: 'pointer',
                      opacity: 0.8
                    }}
                    onClick={() => handleVisualizationSelect(viz.type)}
                  >
                    <Radio value={viz.type} style={{ display: 'none' }} />
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div style={{ fontSize: '24px', color: '#8c8c8c' }}>
                        {viz.icon}
                      </div>

                      <Title level={5} style={{ margin: 0, color: '#8c8c8c' }}>
                        {viz.name}
                      </Title>

                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {viz.description}
                      </Text>

                      <div>
                        <Text strong style={{ fontSize: '11px' }}>Best for:</Text>
                        <div style={{ marginTop: '4px' }}>
                          {viz.bestFor.map((use, index) => (
                            <Tag key={index} color="default" style={{ margin: '1px', fontSize: '11px' }}>
                              {use}
                            </Tag>
                          ))}
                        </div>
                      </div>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
          </Radio.Group>
        </>
      )}

      {selectedType && (
        <Card style={{ marginTop: '16px', backgroundColor: '#f6ffed', border: '1px solid #b7eb8f' }}>
          <Text strong style={{ color: '#52c41a' }}>
            ✓ Selected: {visualizationTypes.find(v => v.type === selectedType)?.name}
          </Text>
          <br />
          <Text type="secondary">
            {visualizationTypes.find(v => v.type === selectedType)?.description}
          </Text>
        </Card>
      )}
    </div>
  );
};
