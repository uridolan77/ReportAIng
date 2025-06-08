import React, { useState } from 'react';
import { Card, Collapse, Typography, Table, Tag, Space, Button, Alert } from 'antd';
import { BugOutlined } from '@ant-design/icons';

const { Panel } = Collapse;
const { Text } = Typography;

interface ChartDebugPanelProps {
  originalData: any[];
  processedData: any[];
  config: any;
  columns: string[];
  title?: string;
}

export const ChartDebugPanel: React.FC<ChartDebugPanelProps> = ({
  originalData,
  processedData,
  config,
  columns,
  title = "Chart Debug Information"
}) => {
  const [isVisible, setIsVisible] = useState(false);

  // Analyze data differences
  const dataAnalysis = {
    originalCount: originalData.length,
    processedCount: processedData.length,
    dataLoss: originalData.length - processedData.length,
    dataLossPercentage: originalData.length > 0 ? 
      ((originalData.length - processedData.length) / originalData.length * 100).toFixed(1) : 0
  };

  // Analyze column mapping
  const columnAnalysis = {
    availableColumns: originalData.length > 0 ? Object.keys(originalData[0]) : [],
    requestedXAxis: config?.xAxis,
    requestedYAxis: config?.yAxis,
    requestedSeries: config?.series || [],
    xAxisExists: originalData.length > 0 && config?.xAxis ? 
      Object.keys(originalData[0]).includes(config.xAxis) : false,
    yAxisExists: originalData.length > 0 && config?.yAxis ? 
      Object.keys(originalData[0]).includes(config.yAxis) : false
  };

  // Sample data comparison
  const sampleComparison = originalData.slice(0, 5).map((originalRow, index) => {
    const processedRow = processedData[index];
    return {
      index,
      original: originalRow,
      processed: processedRow,
      xValue: originalRow?.[config?.xAxis || ''],
      yValue: originalRow?.[config?.yAxis || ''],
      processedXValue: processedRow?.name || processedRow?.[config?.xAxis || ''],
      processedYValue: processedRow?.value || processedRow?.[config?.yAxis || '']
    };
  });

  const getStatusColor = (condition: boolean) => condition ? 'success' : 'error';

  if (!isVisible) {
    return (
      <Button 
        icon={<BugOutlined />} 
        onClick={() => setIsVisible(true)}
        style={{ marginBottom: 16 }}
      >
        Show Chart Debug Info
      </Button>
    );
  }

  return (
    <Card 
      title={
        <Space>
          <BugOutlined />
          {title}
          <Button size="small" onClick={() => setIsVisible(false)}>Hide</Button>
        </Space>
      }
      style={{ marginBottom: 16 }}
    >
      <Collapse>
        <Panel header="Data Flow Analysis" key="data-flow">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Alert
              message={`Data Processing: ${dataAnalysis.originalCount} → ${dataAnalysis.processedCount} rows`}
              description={
                dataAnalysis.dataLoss > 0 ? 
                  `⚠️ ${dataAnalysis.dataLoss} rows lost (${dataAnalysis.dataLossPercentage}%)` :
                  '✅ No data loss detected'
              }
              type={dataAnalysis.dataLoss > 0 ? 'warning' : 'success'}
            />
            
            <div>
              <Text strong>Original Data Sample:</Text>
              <pre style={{ background: '#f5f5f5', padding: 8, fontSize: 12 }}>
                {JSON.stringify(originalData.slice(0, 2), null, 2)}
              </pre>
            </div>
            
            <div>
              <Text strong>Processed Data Sample:</Text>
              <pre style={{ background: '#f5f5f5', padding: 8, fontSize: 12 }}>
                {JSON.stringify(processedData.slice(0, 2), null, 2)}
              </pre>
            </div>
          </Space>
        </Panel>

        <Panel header="Column Mapping Analysis" key="column-mapping">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Available Columns: </Text>
              {columnAnalysis.availableColumns.map(col => (
                <Tag key={col} color="blue">{col}</Tag>
              ))}
            </div>
            
            <div>
              <Text strong>Chart Configuration:</Text>
              <ul>
                <li>
                  X-Axis: <Tag color={getStatusColor(columnAnalysis.xAxisExists)}>
                    {columnAnalysis.requestedXAxis || 'Not set'}
                  </Tag>
                  {!columnAnalysis.xAxisExists && columnAnalysis.requestedXAxis && 
                    <Text type="danger"> (Column not found!)</Text>
                  }
                </li>
                <li>
                  Y-Axis: <Tag color={getStatusColor(columnAnalysis.yAxisExists)}>
                    {columnAnalysis.requestedYAxis || 'Not set'}
                  </Tag>
                  {!columnAnalysis.yAxisExists && columnAnalysis.requestedYAxis && 
                    <Text type="danger"> (Column not found!)</Text>
                  }
                </li>
                <li>
                  Series: {columnAnalysis.requestedSeries.map(series => (
                    <Tag key={series} color={getStatusColor(columnAnalysis.availableColumns.includes(series))}>
                      {series}
                    </Tag>
                  ))}
                </li>
              </ul>
            </div>
          </Space>
        </Panel>

        <Panel header="Data Transformation Details" key="transformation">
          <Table
            size="small"
            dataSource={sampleComparison}
            pagination={false}
            columns={[
              {
                title: 'Row',
                dataIndex: 'index',
                key: 'index',
                width: 50
              },
              {
                title: 'Original X',
                dataIndex: 'xValue',
                key: 'xValue',
                render: (value) => <Text code>{JSON.stringify(value)}</Text>
              },
              {
                title: 'Processed X',
                dataIndex: 'processedXValue',
                key: 'processedXValue',
                render: (value) => <Text code>{JSON.stringify(value)}</Text>
              },
              {
                title: 'Original Y',
                dataIndex: 'yValue',
                key: 'yValue',
                render: (value) => <Text code>{JSON.stringify(value)}</Text>
              },
              {
                title: 'Processed Y',
                dataIndex: 'processedYValue',
                key: 'processedYValue',
                render: (value) => <Text code>{JSON.stringify(value)}</Text>
              }
            ]}
          />
        </Panel>

        <Panel header="Full Configuration" key="config">
          <pre style={{ background: '#f5f5f5', padding: 8, fontSize: 12, overflow: 'auto' }}>
            {JSON.stringify(config, null, 2)}
          </pre>
        </Panel>
      </Collapse>
    </Card>
  );
};

export default ChartDebugPanel;
