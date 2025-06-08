// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\demo\EnhancedFilteringDemo.tsx
import React, { useState } from 'react';
import { Card, Space, Typography, Button, Tag, Divider } from 'antd';
import { FilterOutlined, TableOutlined, DollarOutlined, CalendarOutlined } from '@ant-design/icons';
import DataTable from '../DataTableMain';
import { useEnhancedColumns, useAutoColumns } from '../hooks/useEnhancedColumns';
import { DataTableColumn } from '../types';

const { Title, Text, Paragraph } = Typography;

// Sample data with different data types for demonstration
const generateSampleData = () => {
  const gameNames = ['Sports Betting', 'Big Bass Vegas', 'Bigger Bass Splash', 'Golden Winner Grand Chance', 'Slots'];
  const providers = ['BetConstruct', 'PragmaticPlay', 'LIGHT & WONDER'];
  const gameTypes = ['Sport', 'Slots'];
  const currencies = ['USD', 'EUR', 'GBP'];
  const statuses = ['Active', 'Inactive', 'Pending'];
  
  return Array.from({ length: 50 }, (_, i) => ({
    id: i + 1,
    GameName: gameNames[i % gameNames.length],
    Provider: providers[i % providers.length],
    GameType: gameTypes[i % gameTypes.length],
    TotalRevenue: Math.round((Math.random() * 100000 + 10000) * 100) / 100,
    TotalBets: Math.floor(Math.random() * 10000 + 1000),
    TotalSessions: Math.floor(Math.random() * 5000 + 500),
    Currency: currencies[i % currencies.length],
    Status: statuses[i % statuses.length],
    LastUpdated: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    IsActive: Math.random() > 0.5,
    PlayerCount: Math.floor(Math.random() * 1000 + 100),
    AverageSessionTime: Math.round((Math.random() * 60 + 10) * 100) / 100,
    ConversionRate: Math.round(Math.random() * 100 * 100) / 100
  }));
};

const EnhancedFilteringDemo: React.FC = () => {
  const [sampleData] = useState(generateSampleData);
  const [useAutoDetection, setUseAutoDetection] = useState(true);

  // Manual column configuration (traditional approach)
  const manualColumns: DataTableColumn[] = [
    {
      key: 'id',
      title: 'ID',
      dataIndex: 'id',
      dataType: 'number',
      filterType: 'number',
      sortable: true,
      width: 80
    },
    {
      key: 'GameName',
      title: 'Game Name',
      dataIndex: 'GameName',
      dataType: 'string',
      filterType: 'multiselect',
      filterable: true,
      searchable: true,
      sortable: true
    },
    {
      key: 'Provider',
      title: 'Provider',
      dataIndex: 'Provider',
      dataType: 'string',
      filterType: 'multiselect',
      filterable: true,
      sortable: true
    },
    {
      key: 'TotalRevenue',
      title: 'Total Revenue',
      dataIndex: 'TotalRevenue',
      dataType: 'money',
      filterType: 'money',
      filterable: true,
      sortable: true,
      aggregation: 'sum',
      format: (value) => `$${value.toLocaleString()}`
    },
    {
      key: 'TotalBets',
      title: 'Total Bets',
      dataIndex: 'TotalBets',
      dataType: 'number',
      filterType: 'number',
      filterable: true,
      sortable: true,
      aggregation: 'sum'
    },
    {
      key: 'LastUpdated',
      title: 'Last Updated',
      dataIndex: 'LastUpdated',
      dataType: 'date',
      filterType: 'dateRange',
      filterable: true,
      sortable: true
    },
    {
      key: 'IsActive',
      title: 'Is Active',
      dataIndex: 'IsActive',
      dataType: 'boolean',
      filterType: 'boolean',
      filterable: true,
      render: (value: boolean) => (
        <Tag color={value ? 'green' : 'red'}>
          {value ? 'Yes' : 'No'}
        </Tag>
      )
    }
  ];

  // Auto-detected columns
  const { columns: autoColumns, columnAnalysis } = useAutoColumns(sampleData, ['id']);

  const currentColumns = useAutoDetection ? autoColumns : manualColumns;

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>
        <FilterOutlined /> Enhanced DataTable Filtering Demo
      </Title>
      
      <Paragraph>
        This demo showcases the enhanced filtering capabilities of the DataTable component with automatic 
        data type detection and intelligent filter selection.
      </Paragraph>

      <Card title="Filter Type Examples" style={{ marginBottom: '24px' }}>
        <Space wrap>
          <Tag icon={<TableOutlined />} color="blue">Text Fields → Multiselect</Tag>
          <Tag icon={<DollarOutlined />} color="green">Money Fields → Range + Operators</Tag>
          <Tag icon={<CalendarOutlined />} color="orange">Date Fields → Date Range</Tag>
          <Tag color="purple">Number Fields → Min/Max Range</Tag>
          <Tag color="cyan">Boolean Fields → Yes/No/All</Tag>
        </Space>
      </Card>

      <Card 
        title="Configuration Mode" 
        extra={
          <Button 
            type={useAutoDetection ? 'primary' : 'default'}
            onClick={() => setUseAutoDetection(!useAutoDetection)}
          >
            {useAutoDetection ? 'Auto Detection ON' : 'Manual Config'}
          </Button>
        }
        style={{ marginBottom: '24px' }}
      >
        <Text>
          {useAutoDetection 
            ? 'Using automatic data type detection and filter configuration'
            : 'Using manual column configuration'
          }
        </Text>
        
        {useAutoDetection && (
          <div style={{ marginTop: '16px' }}>
            <Title level={5}>Detected Column Types:</Title>
            <Space wrap>
              {columnAnalysis.map(analysis => (
                <Tag 
                  key={analysis.columnName}
                  color={
                    analysis.dataType === 'money' ? 'green' :
                    analysis.dataType === 'date' ? 'orange' :
                    analysis.dataType === 'number' ? 'blue' :
                    analysis.dataType === 'boolean' ? 'purple' :
                    'default'
                  }
                >
                  {analysis.title}: {analysis.dataType} ({analysis.filterType})
                </Tag>
              ))}
            </Space>
          </div>
        )}
      </Card>

      <Divider />

      <DataTable
        data={sampleData}
        columns={currentColumns}
        keyField="id"
        features={{
          filtering: true,
          searching: true,
          sorting: true,
          pagination: true,
          aggregation: true,
          selection: true,
          export: true
        }}
        config={{
          pageSize: 10,
          showAggregationRow: true
        }}
      />

      <Card title="Filter Instructions" style={{ marginTop: '24px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text><strong>Text Fields (Game Name, Provider):</strong> Use multiselect dropdown to filter by specific values</Text>
          <Text><strong>Money Fields (Total Revenue):</strong> Set min/max amounts or use ≥/≤ operators for threshold filtering</Text>
          <Text><strong>Number Fields (Total Bets):</strong> Set minimum and maximum range values</Text>
          <Text><strong>Date Fields (Last Updated):</strong> Select date ranges using the date picker</Text>
          <Text><strong>Boolean Fields (Is Active):</strong> Choose Yes, No, or All to filter active status</Text>
        </Space>
      </Card>
    </div>
  );
};

export default EnhancedFilteringDemo;
