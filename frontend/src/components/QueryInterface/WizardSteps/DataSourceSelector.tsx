import React, { useState, useEffect } from 'react';
import { Card, List, Input, Typography, Space, Tag, Spin, Empty } from 'antd';
import { DatabaseOutlined, SearchOutlined, TableOutlined } from '@ant-design/icons';
import { QueryBuilderData } from '../QueryWizard';

const { Title, Text, Paragraph } = Typography;
const { Search } = Input;

interface DataSource {
  table: string;
  description: string;
  category: string;
  recordCount: number;
  lastUpdated: string;
  columns: Array<{
    name: string;
    type: string;
    description?: string;
  }>;
}

interface DataSourceSelectorProps {
  data: QueryBuilderData;
  onChange: (data: Partial<QueryBuilderData>) => void;
  onNext?: () => void;
  onPrevious?: () => void;
}

export const DataSourceSelector: React.FC<DataSourceSelectorProps> = ({
  data,
  onChange,
}) => {
  const [dataSources, setDataSources] = useState<DataSource[]>([]);
  const [filteredSources, setFilteredSources] = useState<DataSource[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  // Mock data sources - in real app, this would come from API
  useEffect(() => {
    const mockDataSources: DataSource[] = [
      {
        table: 'tbl_Daily_actions',
        description: 'Main statistics table containing all player gaming activities by day',
        category: 'Gaming Analytics',
        recordCount: 1250000,
        lastUpdated: '2024-01-15',
        columns: [
          { name: 'WhiteLabelID', type: 'int', description: 'White label identifier' },
          { name: 'PlayerID', type: 'int', description: 'Unique player identifier' },
          { name: 'ActionDate', type: 'date', description: 'Date of the gaming activity' },
          { name: 'Deposits', type: 'decimal', description: 'Total deposits amount' },
          { name: 'Bets', type: 'decimal', description: 'Total bets placed' },
          { name: 'Wins', type: 'decimal', description: 'Total winnings' },
          { name: 'SportBets', type: 'decimal', description: 'Sports betting amounts' },
          { name: 'CasinoBets', type: 'decimal', description: 'Casino game bets' },
          { name: 'LiveBets', type: 'decimal', description: 'Live casino bets' },
          { name: 'BingoBets', type: 'decimal', description: 'Bingo game bets' }
        ]
      },
      {
        table: 'tbl_Daily_actions_players',
        description: 'Player profile and demographic information',
        category: 'Player Management',
        recordCount: 45000,
        lastUpdated: '2024-01-14',
        columns: [
          { name: 'PlayerID', type: 'int', description: 'Unique player identifier' },
          { name: 'Username', type: 'varchar', description: 'Player username' },
          { name: 'Email', type: 'varchar', description: 'Player email address' },
          { name: 'RegistrationDate', type: 'date', description: 'Account registration date' },
          { name: 'CountryID', type: 'int', description: 'Player country identifier' },
          { name: 'CurrencyID', type: 'int', description: 'Player currency identifier' },
          { name: 'Status', type: 'varchar', description: 'Account status (Active, Suspended, etc.)' }
        ]
      },
      {
        table: 'tbl_Countries',
        description: 'Country reference data with regional information',
        category: 'Reference Data',
        recordCount: 195,
        lastUpdated: '2024-01-01',
        columns: [
          { name: 'CountryID', type: 'int', description: 'Unique country identifier' },
          { name: 'CountryName', type: 'varchar', description: 'Country name' },
          { name: 'CountryCode', type: 'varchar', description: 'ISO country code' },
          { name: 'Region', type: 'varchar', description: 'Geographic region' }
        ]
      },
      {
        table: 'tbl_Currencies',
        description: 'Currency reference data with exchange rates',
        category: 'Reference Data',
        recordCount: 25,
        lastUpdated: '2024-01-15',
        columns: [
          { name: 'CurrencyID', type: 'int', description: 'Unique currency identifier' },
          { name: 'CurrencyCode', type: 'varchar', description: 'Currency code (USD, EUR, etc.)' },
          { name: 'CurrencyName', type: 'varchar', description: 'Currency name' },
          { name: 'ExchangeRate', type: 'decimal', description: 'Exchange rate to base currency' }
        ]
      },
      {
        table: 'tbl_Whitelabels',
        description: 'White label partner configuration and branding information',
        category: 'Partner Management',
        recordCount: 12,
        lastUpdated: '2024-01-10',
        columns: [
          { name: 'WhiteLabelID', type: 'int', description: 'Unique white label identifier' },
          { name: 'WhiteLabelName', type: 'varchar', description: 'White label partner name' },
          { name: 'BrandName', type: 'varchar', description: 'Brand display name' },
          { name: 'Status', type: 'varchar', description: 'Partner status' }
        ]
      }
    ];

    setTimeout(() => {
      setDataSources(mockDataSources);
      setFilteredSources(mockDataSources);
      setLoading(false);
    }, 500);
  }, []);

  useEffect(() => {
    if (searchTerm) {
      const filtered = dataSources.filter(source =>
        source.table.toLowerCase().includes(searchTerm.toLowerCase()) ||
        source.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        source.category.toLowerCase().includes(searchTerm.toLowerCase())
      );
      setFilteredSources(filtered);
    } else {
      setFilteredSources(dataSources);
    }
  }, [searchTerm, dataSources]);

  const handleSelectDataSource = (source: DataSource) => {
    onChange({
      dataSource: {
        table: source.table,
        description: source.description
      }
    });
  };

  const formatNumber = (num: number): string => {
    return new Intl.NumberFormat().format(num);
  };

  const getCategoryColor = (category: string): string => {
    const colors: Record<string, string> = {
      'Gaming Analytics': 'blue',
      'Player Management': 'green',
      'Reference Data': 'orange',
      'Partner Management': 'purple'
    };
    return colors[category] || 'default';
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '60px' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text>Loading available data sources...</Text>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <Title level={4}>
          <DatabaseOutlined /> Select Data Source
        </Title>
        <Paragraph type="secondary">
          Choose the primary table you want to analyze. This will determine what data is available for your query.
        </Paragraph>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <Search
          placeholder="Search tables by name, description, or category..."
          prefix={<SearchOutlined />}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          style={{ width: '100%' }}
          size="large"
        />
      </div>

      {filteredSources.length === 0 ? (
        <Empty
          description="No data sources found"
          style={{ padding: '40px' }}
        />
      ) : (
        <List
          grid={{ gutter: 16, xs: 1, sm: 1, md: 2, lg: 2, xl: 2 }}
          dataSource={filteredSources}
          renderItem={(source) => (
            <List.Item>
              <Card
                hoverable
                className={data.dataSource?.table === source.table ? 'selected-card' : ''}
                onClick={() => handleSelectDataSource(source)}
                style={{
                  border: data.dataSource?.table === source.table ? '2px solid #1890ff' : '1px solid #d9d9d9',
                  cursor: 'pointer'
                }}
              >
                <Card.Meta
                  avatar={<TableOutlined style={{ fontSize: '24px', color: '#1890ff' }} />}
                  title={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <Text strong>{source.table}</Text>
                      <Tag color={getCategoryColor(source.category)}>{source.category}</Tag>
                    </Space>
                  }
                  description={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <Text>{source.description}</Text>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {formatNumber(source.recordCount)} records • Updated {source.lastUpdated}
                        </Text>
                      </div>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {source.columns.length} columns available
                        </Text>
                      </div>
                    </Space>
                  }
                />
              </Card>
            </List.Item>
          )}
        />
      )}

      {data.dataSource && (
        <Card style={{ marginTop: '16px', backgroundColor: '#f6ffed', border: '1px solid #b7eb8f' }}>
          <Text strong style={{ color: '#52c41a' }}>
            ✓ Selected: {data.dataSource.table}
          </Text>
          <br />
          <Text type="secondary">{data.dataSource.description}</Text>
        </Card>
      )}


    </div>
  );
};
