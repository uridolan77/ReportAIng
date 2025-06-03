import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Select,
  Table,
  Tabs,
  Tooltip,
  Dropdown,
  Menu,
  Modal,
  message,
  Tag,
  Statistic,
  Progress
} from 'antd';
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  TableOutlined,
  DownloadOutlined,
  ShareAltOutlined,
  FullscreenOutlined,
  SettingOutlined,
  EyeOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
  LinkOutlined
} from '@ant-design/icons';
// Note: Recharts dependency temporarily removed for compatibility
// import {
//   BarChart,
//   Bar,
//   LineChart,
//   Line,
//   PieChart,
//   Pie,
//   Cell,
//   XAxis,
//   YAxis,
//   CartesianGrid,
//   Tooltip as RechartsTooltip,
//   Legend,
//   ResponsiveContainer
// } from 'recharts';

const { Title, Text } = Typography;
const { Option } = Select;

interface ResultData {
  columns: string[];
  rows: any[][];
  metadata?: {
    totalRows: number;
    executionTime: number;
    confidence: number;
  };
}

interface InteractiveResultsDisplayProps {
  data: ResultData;
  query: string;
  onExport: (format: string) => void;
  onShare: () => void;
}

export const InteractiveResultsDisplay: React.FC<InteractiveResultsDisplayProps> = ({
  data,
  query,
  onExport,
  onShare
}) => {
  const [activeView, setActiveView] = useState<'table' | 'bar' | 'line' | 'pie'>('table');
  const [chartConfig, setChartConfig] = useState({
    xAxis: data.columns[0] || '',
    yAxis: data.columns[1] || '',
    groupBy: ''
  });
  const [showExportModal, setShowExportModal] = useState(false);
  const [fullscreen, setFullscreen] = useState(false);

  // Transform data for charts
  const chartData = React.useMemo(() => {
    if (!data.rows || data.rows.length === 0) return [];
    
    const xIndex = data.columns.indexOf(chartConfig.xAxis);
    const yIndex = data.columns.indexOf(chartConfig.yAxis);
    
    if (xIndex === -1 || yIndex === -1) return [];
    
    return data.rows.map(row => ({
      name: row[xIndex],
      value: Number(row[yIndex]) || 0,
      [chartConfig.xAxis]: row[xIndex],
      [chartConfig.yAxis]: row[yIndex]
    }));
  }, [data, chartConfig]);

  // Table columns configuration
  const tableColumns = data.columns.map((col, index) => ({
    title: col,
    dataIndex: index,
    key: col,
    sorter: (a: any, b: any) => {
      const aVal = a[index];
      const bVal = b[index];
      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return aVal - bVal;
      }
      return String(aVal).localeCompare(String(bVal));
    },
    render: (value: any) => {
      if (typeof value === 'number') {
        return value.toLocaleString();
      }
      return value;
    }
  }));

  const tableData = data.rows.map((row, index) => ({
    key: index,
    ...row.reduce((acc, cell, cellIndex) => {
      acc[cellIndex] = cell;
      return acc;
    }, {} as any)
  }));

  const colors = ['#8884d8', '#82ca9d', '#ffc658', '#ff7300', '#00ff00', '#ff00ff'];

  const renderChart = () => {
    switch (activeView) {
      case 'bar':
        return (
          <div style={{
            height: '400px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
            borderRadius: '12px',
            border: '2px dashed #e2e8f0'
          }}>
            <div style={{ textAlign: 'center' }}>
              <BarChartOutlined style={{ fontSize: '48px', color: '#3b82f6', marginBottom: '16px' }} />
              <Text style={{ fontSize: '18px', color: '#4b5563', display: 'block' }}>
                Bar Chart View
              </Text>
              <Text style={{ fontSize: '14px', color: '#6b7280' }}>
                Chart visualization will be displayed here
              </Text>
            </div>
          </div>
        );

      case 'line':
        return (
          <div style={{
            height: '400px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
            borderRadius: '12px',
            border: '2px dashed #e2e8f0'
          }}>
            <div style={{ textAlign: 'center' }}>
              <LineChartOutlined style={{ fontSize: '48px', color: '#10b981', marginBottom: '16px' }} />
              <Text style={{ fontSize: '18px', color: '#4b5563', display: 'block' }}>
                Line Chart View
              </Text>
              <Text style={{ fontSize: '14px', color: '#6b7280' }}>
                Chart visualization will be displayed here
              </Text>
            </div>
          </div>
        );

      case 'pie':
        return (
          <div style={{
            height: '400px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
            borderRadius: '12px',
            border: '2px dashed #e2e8f0'
          }}>
            <div style={{ textAlign: 'center' }}>
              <PieChartOutlined style={{ fontSize: '48px', color: '#8b5cf6', marginBottom: '16px' }} />
              <Text style={{ fontSize: '18px', color: '#4b5563', display: 'block' }}>
                Pie Chart View
              </Text>
              <Text style={{ fontSize: '14px', color: '#6b7280' }}>
                Chart visualization will be displayed here
              </Text>
            </div>
          </div>
        );

      default:
        return (
          <Table
            columns={tableColumns}
            dataSource={tableData}
            pagination={{
              pageSize: 50,
              showSizeChanger: true,
              showQuickJumper: true,
              pageSizeOptions: ['10', '25', '50', '100', '200'],
              showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`
            }}
            scroll={{ x: true }}
            size="small"
          />
        );
    }
  };

  const exportMenu = (
    <Menu onClick={({ key }) => onExport(key)}>
      <Menu.Item key="csv" icon={<FileExcelOutlined />}>
        Export as CSV
      </Menu.Item>
      <Menu.Item key="excel" icon={<FileExcelOutlined />}>
        Export as Excel
      </Menu.Item>
      <Menu.Item key="pdf" icon={<FilePdfOutlined />}>
        Export as PDF
      </Menu.Item>
      <Menu.Item key="json">
        Export as JSON
      </Menu.Item>
    </Menu>
  );

  const viewButtons = [
    { key: 'table', icon: <TableOutlined />, label: 'Table' },
    { key: 'bar', icon: <BarChartOutlined />, label: 'Bar Chart' },
    { key: 'line', icon: <LineChartOutlined />, label: 'Line Chart' },
    { key: 'pie', icon: <PieChartOutlined />, label: 'Pie Chart' }
  ];

  return (
    <div>
      {/* Results Header */}
      <Card
        style={{ marginBottom: '16px' }}
        bodyStyle={{ padding: '16px' }}
      >
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Title level={4} style={{ margin: 0 }}>
                Query Results
              </Title>
              {data.metadata && (
                <Space>
                  <Tag color="blue">
                    {data.metadata.totalRows.toLocaleString()} rows
                  </Tag>
                  <Tag color="green">
                    {data.metadata.executionTime}s
                  </Tag>
                  <Tag color="purple">
                    {Math.round(data.metadata.confidence * 100)}% confidence
                  </Tag>
                </Space>
              )}
            </Space>
          </Col>
          <Col>
            <Space>
              {/* View Type Selector */}
              <Space.Compact>
                {viewButtons.map(button => (
                  <Tooltip key={button.key} title={button.label}>
                    <Button
                      type={activeView === button.key ? 'primary' : 'default'}
                      icon={button.icon}
                      onClick={() => setActiveView(button.key as any)}
                    />
                  </Tooltip>
                ))}
              </Space.Compact>

              {/* Chart Configuration */}
              {activeView !== 'table' && (
                <Dropdown
                  overlay={
                    <div style={{ padding: '16px', background: 'white', borderRadius: '8px', boxShadow: '0 4px 12px rgba(0,0,0,0.15)' }}>
                      <Space direction="vertical">
                        <div>
                          <Text strong>X-Axis:</Text>
                          <Select
                            value={chartConfig.xAxis}
                            onChange={(value) => setChartConfig(prev => ({ ...prev, xAxis: value }))}
                            style={{ width: '150px', marginLeft: '8px' }}
                          >
                            {data.columns.map(col => (
                              <Option key={col} value={col}>{col}</Option>
                            ))}
                          </Select>
                        </div>
                        <div>
                          <Text strong>Y-Axis:</Text>
                          <Select
                            value={chartConfig.yAxis}
                            onChange={(value) => setChartConfig(prev => ({ ...prev, yAxis: value }))}
                            style={{ width: '150px', marginLeft: '8px' }}
                          >
                            {data.columns.map(col => (
                              <Option key={col} value={col}>{col}</Option>
                            ))}
                          </Select>
                        </div>
                      </Space>
                    </div>
                  }
                  trigger={['click']}
                >
                  <Button icon={<SettingOutlined />}>
                    Configure
                  </Button>
                </Dropdown>
              )}

              {/* Export & Share */}
              <Dropdown overlay={exportMenu} trigger={['click']}>
                <Button icon={<DownloadOutlined />}>
                  Export
                </Button>
              </Dropdown>
              
              <Button
                icon={<ShareAltOutlined />}
                onClick={onShare}
              >
                Share
              </Button>
              
              <Button
                icon={<FullscreenOutlined />}
                onClick={() => setFullscreen(true)}
              >
                Fullscreen
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Results Display */}
      <Card>
        {renderChart()}
      </Card>

      {/* Summary Statistics */}
      {data.rows && data.rows.length > 0 && (
        <Card title="Summary Statistics" style={{ marginTop: '16px' }}>
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={6}>
              <Statistic
                title="Total Rows"
                value={data.rows.length}
                formatter={(value) => value.toLocaleString()}
              />
            </Col>
            <Col xs={24} sm={6}>
              <Statistic
                title="Columns"
                value={data.columns.length}
              />
            </Col>
            {data.metadata && (
              <>
                <Col xs={24} sm={6}>
                  <Statistic
                    title="Execution Time"
                    value={data.metadata.executionTime}
                    suffix="s"
                    precision={2}
                  />
                </Col>
                <Col xs={24} sm={6}>
                  <div>
                    <Text strong>Confidence</Text>
                    <Progress
                      percent={Math.round(data.metadata.confidence * 100)}
                      size="small"
                      status={data.metadata.confidence > 0.8 ? 'success' : 'normal'}
                    />
                  </div>
                </Col>
              </>
            )}
          </Row>
        </Card>
      )}

      {/* Fullscreen Modal */}
      <Modal
        title="Results - Fullscreen View"
        open={fullscreen}
        onCancel={() => setFullscreen(false)}
        footer={null}
        width="90vw"
        style={{ top: 20 }}
      >
        <div style={{ height: '70vh' }}>
          {renderChart()}
        </div>
      </Modal>
    </div>
  );
};
