import React from 'react';
import { Card, Typography, Space, Button, Tag, Divider, Row, Col } from 'antd';
import {
  PlayCircleOutlined,
  EditOutlined,
  CopyOutlined,
  DatabaseOutlined,
  BarChartOutlined,
  FilterOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { QueryBuilderData } from '../QueryWizard';

const { Title, Text, Paragraph } = Typography;

interface QueryPreviewProps {
  query: string;
  data: QueryBuilderData;
  onExecute: () => void;
  onEdit: () => void;
}

export const QueryPreview: React.FC<QueryPreviewProps> = ({
  query,
  data,
  onExecute,
  onEdit
}) => {
  const copyToClipboard = () => {
    navigator.clipboard.writeText(query);
  };

  const renderDataSourceSummary = () => {
    if (!data.dataSource) return null;
    
    return (
      <Card size="small" style={{ marginBottom: '12px' }}>
        <Space>
          <DatabaseOutlined style={{ color: '#1890ff' }} />
          <Text strong>Data Source:</Text>
          <Tag color="blue">{data.dataSource.table}</Tag>
        </Space>
        <br />
        <Text type="secondary" style={{ fontSize: '12px' }}>
          {data.dataSource.description}
        </Text>
      </Card>
    );
  };

  const renderMetricsSummary = () => {
    if (!data.metrics?.selected || data.metrics.selected.length === 0) return null;
    
    return (
      <Card size="small" style={{ marginBottom: '12px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Space>
            <BarChartOutlined style={{ color: '#52c41a' }} />
            <Text strong>Metrics ({data.metrics.selected.length}):</Text>
          </Space>
          <div>
            {data.metrics.selected.map(metric => {
              const agg = data.metrics?.aggregations?.[metric];
              return (
                <Tag key={metric} color="green" style={{ margin: '2px' }}>
                  {agg && agg !== 'none' ? `${agg}(${metric})` : metric}
                </Tag>
              );
            })}
          </div>
        </Space>
      </Card>
    );
  };

  const renderFiltersSummary = () => {
    if (!data.filters?.conditions || data.filters.conditions.length === 0) return null;
    
    return (
      <Card size="small" style={{ marginBottom: '12px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Space>
            <FilterOutlined style={{ color: '#fa8c16' }} />
            <Text strong>Filters ({data.filters.conditions.length}):</Text>
          </Space>
          <div>
            {data.filters.conditions.map((condition, index) => (
              <Tag key={index} color="orange" style={{ margin: '2px' }}>
                {index > 0 && `${condition.type.toUpperCase()} `}
                {condition.column} {condition.operator} {
                  Array.isArray(condition.value) ? condition.value.join(', ') : condition.value
                }
              </Tag>
            ))}
          </div>
        </Space>
      </Card>
    );
  };

  const renderVisualizationSummary = () => {
    if (!data.visualization?.type) return null;
    
    const vizNames: Record<string, string> = {
      table: 'Data Table',
      bar: 'Bar Chart',
      line: 'Line Chart',
      pie: 'Pie Chart',
      scatter: 'Scatter Plot',
      area: 'Area Chart',
      heatmap: 'Heatmap',
      funnel: 'Funnel Chart'
    };
    
    return (
      <Card size="small" style={{ marginBottom: '12px' }}>
        <Space>
          <EyeOutlined style={{ color: '#722ed1' }} />
          <Text strong>Visualization:</Text>
          <Tag color="purple">{vizNames[data.visualization.type] || data.visualization.type}</Tag>
        </Space>
      </Card>
    );
  };

  return (
    <div>
      <Title level={4}>Query Preview</Title>
      
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={14}>
          <Card title="Generated Natural Language Query" style={{ height: '100%' }}>
            <Paragraph
              style={{
                fontSize: '16px',
                lineHeight: '1.6',
                padding: '16px',
                backgroundColor: '#f6f8fa',
                borderRadius: '6px',
                border: '1px solid #e1e4e8',
                minHeight: '80px'
              }}
            >
              "{query}"
            </Paragraph>
            
            <Space style={{ marginTop: '16px' }}>
              <Button
                type="primary"
                icon={<PlayCircleOutlined />}
                onClick={onExecute}
                size="large"
              >
                Execute Query
              </Button>
              
              <Button
                icon={<EditOutlined />}
                onClick={onEdit}
              >
                Edit Query
              </Button>
              
              <Button
                icon={<CopyOutlined />}
                onClick={copyToClipboard}
              >
                Copy
              </Button>
            </Space>
          </Card>
        </Col>
        
        <Col xs={24} lg={10}>
          <Card title="Query Summary" style={{ height: '100%' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              {renderDataSourceSummary()}
              {renderMetricsSummary()}
              {renderFiltersSummary()}
              {renderVisualizationSummary()}
              
              {(!data.dataSource || !data.metrics?.selected?.length || !data.visualization?.type) && (
                <Card size="small" style={{ backgroundColor: '#fff2e8', border: '1px solid #ffbb96' }}>
                  <Text type="warning">
                    ⚠️ Some required fields are missing. Please complete all wizard steps.
                  </Text>
                </Card>
              )}
            </Space>
          </Card>
        </Col>
      </Row>
      
      <Divider />
      
      <Card title="What happens next?" size="small">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text>
            <strong>1.</strong> Your natural language query will be sent to our AI system
          </Text>
          <Text>
            <strong>2.</strong> The AI will generate optimized SQL based on your requirements
          </Text>
          <Text>
            <strong>3.</strong> The query will be executed against the selected data source
          </Text>
          <Text>
            <strong>4.</strong> Results will be displayed in your chosen visualization format
          </Text>
          <Text>
            <strong>5.</strong> You can further refine or export the results as needed
          </Text>
        </Space>
      </Card>
    </div>
  );
};
