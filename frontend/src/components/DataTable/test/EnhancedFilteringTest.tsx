import React from 'react';
import { Card, Typography, Space, Tag } from 'antd';
import { FilterOutlined } from '@ant-design/icons';
import DataTable from '../DataTableMain';

const { Title, Paragraph } = Typography;

// Test data that matches the screenshot
const testData = [
  {
    id: 1,
    CountryName: 'United States',
    Revenue: 200000,
    Players: 1500,
    GameType: 'Slots',
    Provider: 'NetEnt',
    CreatedDate: '2024-01-15'
  },
  {
    id: 2,
    CountryName: 'Canada',
    Revenue: 150000,
    Players: 1200,
    GameType: 'Blackjack',
    Provider: 'Evolution',
    CreatedDate: '2024-01-16'
  },
  {
    id: 3,
    CountryName: 'United Kingdom',
    Revenue: 180000,
    Players: 1350,
    GameType: 'Roulette',
    Provider: 'Pragmatic Play',
    CreatedDate: '2024-01-17'
  },
  {
    id: 4,
    CountryName: 'Germany',
    Revenue: 120000,
    Players: 900,
    GameType: 'Slots',
    Provider: 'NetEnt',
    CreatedDate: '2024-01-18'
  },
  {
    id: 5,
    CountryName: 'France',
    Revenue: 95000,
    Players: 750,
    GameType: 'Poker',
    Provider: 'PokerStars',
    CreatedDate: '2024-01-19'
  }
];

const testColumns = [
  {
    key: 'CountryName',
    title: 'Country Name',
    dataIndex: 'CountryName'
  },
  {
    key: 'Revenue',
    title: 'Revenue',
    dataIndex: 'Revenue'
  },
  {
    key: 'Players',
    title: 'Players',
    dataIndex: 'Players'
  },
  {
    key: 'GameType',
    title: 'Game Type',
    dataIndex: 'GameType'
  },
  {
    key: 'Provider',
    title: 'Provider',
    dataIndex: 'Provider'
  },
  {
    key: 'CreatedDate',
    title: 'Created Date',
    dataIndex: 'CreatedDate'
  }
];

const EnhancedFilteringTest: React.FC = () => {
  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>
        <FilterOutlined /> Enhanced Filtering Test
      </Title>
      
      <Paragraph>
        This test uses the same data structure as shown in the screenshot to verify 
        enhanced filtering is working correctly.
      </Paragraph>

      <Card title="Expected Filter Types" style={{ marginBottom: '24px' }}>
        <Space wrap>
          <Tag color="blue">CountryName → Multiselect</Tag>
          <Tag color="green">Revenue → Money Range</Tag>
          <Tag color="purple">Players → Number Range</Tag>
          <Tag color="cyan">GameType → Multiselect</Tag>
          <Tag color="orange">Provider → Multiselect</Tag>
          <Tag color="red">CreatedDate → Date Range</Tag>
        </Space>
      </Card>

      <Card>
        <DataTable
          data={testData}
          columns={testColumns}
          keyField="id"
          autoDetectTypes={true}
          autoGenerateFilterOptions={true}
          features={{
            filtering: true,
            searching: true,
            sorting: true,
            pagination: true,
            selection: true,
            export: true
          }}
          config={{
            pageSize: 10
          }}
        />
      </Card>
    </div>
  );
};

export default EnhancedFilteringTest;
