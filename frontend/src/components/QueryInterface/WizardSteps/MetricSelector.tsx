import React, { useState, useEffect } from 'react';
import { Card, Checkbox, Select, Typography, Space, Row, Col, Tag, Divider, Alert } from 'antd';
import { BarChartOutlined, NumberOutlined, CalendarOutlined } from '@ant-design/icons';
import { QueryBuilderData } from '../QueryWizard';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;

interface MetricColumn {
  name: string;
  type: string;
  description?: string;
  category: 'numeric' | 'text' | 'date' | 'boolean';
  aggregatable: boolean;
  suggested?: boolean;
}

interface MetricSelectorProps {
  data: QueryBuilderData;
  onChange: (data: Partial<QueryBuilderData>) => void;
  onNext?: () => void;
  onPrevious?: () => void;
}

export const MetricSelector: React.FC<MetricSelectorProps> = ({
  data,
  onChange,
}) => {
  const [availableMetrics, setAvailableMetrics] = useState<MetricColumn[]>([]);
  const [selectedMetrics, setSelectedMetrics] = useState<string[]>(data.metrics?.selected || []);
  const [aggregations, setAggregations] = useState<Record<string, string>>(data.metrics?.aggregations || {});

  // Mock metrics based on selected data source
  useEffect(() => {
    if (!data.dataSource?.table) return;

    const getMetricsForTable = (tableName: string): MetricColumn[] => {
      switch (tableName) {
        case 'tbl_Daily_actions':
          return [
            { name: 'Deposits', type: 'decimal', description: 'Total deposit amounts', category: 'numeric', aggregatable: true, suggested: true },
            { name: 'Bets', type: 'decimal', description: 'Total betting amounts', category: 'numeric', aggregatable: true, suggested: true },
            { name: 'Wins', type: 'decimal', description: 'Total winnings', category: 'numeric', aggregatable: true, suggested: true },
            { name: 'SportBets', type: 'decimal', description: 'Sports betting amounts', category: 'numeric', aggregatable: true },
            { name: 'CasinoBets', type: 'decimal', description: 'Casino game bets', category: 'numeric', aggregatable: true },
            { name: 'LiveBets', type: 'decimal', description: 'Live casino bets', category: 'numeric', aggregatable: true },
            { name: 'BingoBets', type: 'decimal', description: 'Bingo game bets', category: 'numeric', aggregatable: true },
            { name: 'PlayerID', type: 'int', description: 'Player identifier', category: 'numeric', aggregatable: true },
            { name: 'WhiteLabelID', type: 'int', description: 'White label identifier', category: 'numeric', aggregatable: false },
            { name: 'ActionDate', type: 'date', description: 'Date of activity', category: 'date', aggregatable: false }
          ];
        case 'tbl_Daily_actions_players':
          return [
            { name: 'PlayerID', type: 'int', description: 'Player count', category: 'numeric', aggregatable: true, suggested: true },
            { name: 'Username', type: 'varchar', description: 'Player usernames', category: 'text', aggregatable: false },
            { name: 'Email', type: 'varchar', description: 'Player emails', category: 'text', aggregatable: false },
            { name: 'RegistrationDate', type: 'date', description: 'Registration dates', category: 'date', aggregatable: false },
            { name: 'CountryID', type: 'int', description: 'Country distribution', category: 'numeric', aggregatable: true },
            { name: 'Status', type: 'varchar', description: 'Account status', category: 'text', aggregatable: false }
          ];
        default:
          return [
            { name: 'ID', type: 'int', description: 'Record count', category: 'numeric', aggregatable: true, suggested: true },
            { name: 'Name', type: 'varchar', description: 'Names', category: 'text', aggregatable: false }
          ];
      }
    };

    setAvailableMetrics(getMetricsForTable(data.dataSource.table));
  }, [data.dataSource]);

  const handleMetricToggle = (metricName: string, checked: boolean) => {
    let newSelected: string[];
    if (checked) {
      newSelected = [...selectedMetrics, metricName];
      // Set default aggregation for numeric fields
      const metric = availableMetrics.find(m => m.name === metricName);
      if (metric?.aggregatable && metric.category === 'numeric') {
        setAggregations(prev => ({
          ...prev,
          [metricName]: metricName.toLowerCase().includes('count') || metricName === 'PlayerID' ? 'count' : 'sum'
        }));
      }
    } else {
      newSelected = selectedMetrics.filter(m => m !== metricName);
      // Remove aggregation
      const newAggregations = { ...aggregations };
      delete newAggregations[metricName];
      setAggregations(newAggregations);
    }
    
    setSelectedMetrics(newSelected);
    updateParent(newSelected, aggregations);
  };

  const handleAggregationChange = (metricName: string, aggregation: string) => {
    const newAggregations = {
      ...aggregations,
      [metricName]: aggregation
    };
    setAggregations(newAggregations);
    updateParent(selectedMetrics, newAggregations);
  };

  const updateParent = (metrics: string[], aggs: Record<string, string>) => {
    onChange({
      metrics: {
        selected: metrics,
        aggregations: aggs
      }
    });
  };

  const getAggregationOptions = (metric: MetricColumn) => {
    if (!metric.aggregatable) return [];
    
    if (metric.category === 'numeric') {
      return [
        { value: 'sum', label: 'Sum' },
        { value: 'avg', label: 'Average' },
        { value: 'count', label: 'Count' },
        { value: 'min', label: 'Minimum' },
        { value: 'max', label: 'Maximum' }
      ];
    }
    
    return [
      { value: 'count', label: 'Count' },
      { value: 'count_distinct', label: 'Count Distinct' }
    ];
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'numeric': return <NumberOutlined />;
      case 'date': return <CalendarOutlined />;
      default: return <BarChartOutlined />;
    }
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'numeric': return 'blue';
      case 'date': return 'green';
      case 'text': return 'orange';
      default: return 'default';
    }
  };

  const suggestedMetrics = availableMetrics.filter(m => m.suggested);
  const otherMetrics = availableMetrics.filter(m => !m.suggested);

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <Title level={4}>
          <BarChartOutlined /> Select Metrics
        </Title>
        <Paragraph type="secondary">
          Choose the data fields you want to analyze. You can apply aggregations like sum, average, or count to numeric fields.
        </Paragraph>
      </div>

      {!data.dataSource?.table && (
        <Alert
          message="Please select a data source first"
          type="warning"
          style={{ marginBottom: '16px' }}
        />
      )}

      {suggestedMetrics.length > 0 && (
        <>
          <Title level={5}>Suggested Metrics</Title>
          <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
            {suggestedMetrics.map((metric) => (
              <Col xs={24} sm={12} md={8} key={metric.name}>
                <Card
                  size="small"
                  style={{
                    border: selectedMetrics.includes(metric.name) ? '2px solid #1890ff' : '1px solid #d9d9d9',
                    backgroundColor: selectedMetrics.includes(metric.name) ? '#f6ffed' : 'white'
                  }}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Checkbox
                        checked={selectedMetrics.includes(metric.name)}
                        onChange={(e) => handleMetricToggle(metric.name, e.target.checked)}
                      >
                        <Text strong>{metric.name}</Text>
                      </Checkbox>
                      <Tag color={getCategoryColor(metric.category)} icon={getCategoryIcon(metric.category)}>
                        {metric.type}
                      </Tag>
                    </div>
                    
                    {metric.description && (
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {metric.description}
                      </Text>
                    )}
                    
                    {selectedMetrics.includes(metric.name) && metric.aggregatable && (
                      <Select
                        style={{ width: '100%' }}
                        placeholder="Select aggregation"
                        value={aggregations[metric.name] || 'sum'}
                        onChange={(value) => handleAggregationChange(metric.name, value)}
                        size="small"
                      >
                        {getAggregationOptions(metric).map(option => (
                          <Option key={option.value} value={option.value}>
                            {option.label}
                          </Option>
                        ))}
                      </Select>
                    )}
                  </Space>
                </Card>
              </Col>
            ))}
          </Row>
          <Divider />
        </>
      )}

      {otherMetrics.length > 0 && (
        <>
          <Title level={5}>All Available Metrics</Title>
          <Row gutter={[16, 16]}>
            {otherMetrics.map((metric) => (
              <Col xs={24} sm={12} md={8} key={metric.name}>
                <Card
                  size="small"
                  style={{
                    border: selectedMetrics.includes(metric.name) ? '2px solid #1890ff' : '1px solid #d9d9d9',
                    backgroundColor: selectedMetrics.includes(metric.name) ? '#f6ffed' : 'white'
                  }}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Checkbox
                        checked={selectedMetrics.includes(metric.name)}
                        onChange={(e) => handleMetricToggle(metric.name, e.target.checked)}
                      >
                        <Text strong>{metric.name}</Text>
                      </Checkbox>
                      <Tag color={getCategoryColor(metric.category)} icon={getCategoryIcon(metric.category)}>
                        {metric.type}
                      </Tag>
                    </div>
                    
                    {metric.description && (
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {metric.description}
                      </Text>
                    )}
                    
                    {selectedMetrics.includes(metric.name) && metric.aggregatable && (
                      <Select
                        style={{ width: '100%' }}
                        placeholder="Select aggregation"
                        value={aggregations[metric.name] || 'sum'}
                        onChange={(value) => handleAggregationChange(metric.name, value)}
                        size="small"
                      >
                        {getAggregationOptions(metric).map(option => (
                          <Option key={option.value} value={option.value}>
                            {option.label}
                          </Option>
                        ))}
                      </Select>
                    )}
                  </Space>
                </Card>
              </Col>
            ))}
          </Row>
        </>
      )}

      {selectedMetrics.length > 0 && (
        <Card style={{ marginTop: '16px', backgroundColor: '#f6ffed', border: '1px solid #b7eb8f' }}>
          <Text strong style={{ color: '#52c41a' }}>
            ✓ Selected {selectedMetrics.length} metric{selectedMetrics.length > 1 ? 's' : ''}:
          </Text>
          <div style={{ marginTop: '8px' }}>
            {selectedMetrics.map(metric => {
              const agg = aggregations[metric];
              return (
                <Tag key={metric} color="green" style={{ margin: '2px' }}>
                  {agg && agg !== 'none' ? `${agg}(${metric})` : metric}
                </Tag>
              );
            })}
          </div>
        </Card>
      )}
    </div>
  );
};
