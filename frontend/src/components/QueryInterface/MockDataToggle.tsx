import React, { useState, useEffect } from 'react';
import { Switch, Tooltip, Space, Typography, Card, Divider, Tag } from 'antd';
import { 
  DatabaseOutlined, 
  ExperimentOutlined, 
  InfoCircleOutlined,
  CheckCircleOutlined,
  DisconnectOutlined
} from '@ant-design/icons';
import { MockDataService } from '../../services/mockDataService';

const { Text } = Typography;

interface MockDataToggleProps {
  style?: React.CSSProperties;
  size?: 'small' | 'default';
  showDetails?: boolean;
}

export const MockDataToggle: React.FC<MockDataToggleProps> = ({ 
  style, 
  size = 'default',
  showDetails = true 
}) => {
  const [isEnabled, setIsEnabled] = useState(false);
  const [availableQueries, setAvailableQueries] = useState<string[]>([]);

  useEffect(() => {
    // Load initial state
    const config = MockDataService.getConfig();
    setIsEnabled(config.enabled);
    setAvailableQueries(MockDataService.getAvailableQueries());
  }, []);

  const handleToggle = (enabled: boolean) => {
    setIsEnabled(enabled);
    MockDataService.setConfig({ enabled });
    
    if (enabled) {
      console.log('🎭 Mock data enabled - queries will return sample data');
    } else {
      console.log('🔌 Mock data disabled - queries will use real database');
    }
  };

  const getStatusIcon = () => {
    if (isEnabled) {
      return <ExperimentOutlined style={{ color: '#f59e0b' }} />;
    }
    return <DatabaseOutlined style={{ color: '#10b981' }} />;
  };

  const getStatusText = () => {
    if (isEnabled) {
      return 'Mock Data Mode';
    }
    return 'Live Database';
  };

  const getStatusColor = () => {
    return isEnabled ? '#f59e0b' : '#10b981';
  };

  if (size === 'small') {
    return (
      <Tooltip 
        title={isEnabled ? 'Using mock data - switch to live database' : 'Using live database - switch to mock data'}
        placement="bottom"
      >
        <Space 
          size="small" 
          style={{ 
            cursor: 'pointer',
            padding: '4px 8px',
            borderRadius: '6px',
            background: isEnabled ? '#fef3c7' : '#d1fae5',
            border: `1px solid ${isEnabled ? '#f59e0b' : '#10b981'}`,
            ...style 
          }}
          onClick={() => handleToggle(!isEnabled)}
        >
          {getStatusIcon()}
          <Text style={{ 
            fontSize: '12px', 
            fontWeight: 500,
            color: getStatusColor()
          }}>
            {isEnabled ? 'Mock' : 'Live'}
          </Text>
          <Switch
            size="small"
            checked={isEnabled}
            onChange={handleToggle}
            style={{ marginLeft: '4px' }}
          />
        </Space>
      </Tooltip>
    );
  }

  return (
    <Card
      size="small"
      style={{
        borderRadius: '12px',
        border: `1px solid ${isEnabled ? '#f59e0b' : '#10b981'}`,
        background: isEnabled ? '#fefbf3' : '#f0fdf4',
        ...style
      }}
      bodyStyle={{ padding: '12px 16px' }}
    >
      <Space direction="vertical" size="small" style={{ width: '100%' }}>
        {/* Header */}
        <Space size="small" style={{ width: '100%', justifyContent: 'space-between' }}>
          <Space size="small">
            {getStatusIcon()}
            <Text strong style={{ color: getStatusColor() }}>
              {getStatusText()}
            </Text>
            {isEnabled ? (
              <Tag color="orange" icon={<ExperimentOutlined />}>
                Testing Mode
              </Tag>
            ) : (
              <Tag color="green" icon={<CheckCircleOutlined />}>
                Production
              </Tag>
            )}
          </Space>
          <Switch
            checked={isEnabled}
            onChange={handleToggle}
            checkedChildren="Mock"
            unCheckedChildren="Live"
          />
        </Space>

        {/* Details */}
        {showDetails && (
          <>
            <Divider style={{ margin: '8px 0' }} />
            <Space direction="vertical" size={4} style={{ width: '100%' }}>
              <Space size="small">
                <InfoCircleOutlined style={{ color: '#6b7280', fontSize: '12px' }} />
                <Text style={{ fontSize: '12px', color: '#6b7280' }}>
                  {isEnabled 
                    ? 'Queries return sample data for testing without database connection'
                    : 'Queries execute against the live ProgressPlayDBTest database'
                  }
                </Text>
              </Space>
              
              {isEnabled && (
                <Space direction="vertical" size={2} style={{ marginLeft: '16px' }}>
                  <Text style={{ fontSize: '11px', color: '#9ca3af', fontWeight: 500 }}>
                    Available mock queries:
                  </Text>
                  {availableQueries.slice(0, 3).map((query, index) => (
                    <Text key={index} style={{ fontSize: '11px', color: '#9ca3af' }}>
                      • {query}
                    </Text>
                  ))}
                  {availableQueries.length > 3 && (
                    <Text style={{ fontSize: '11px', color: '#9ca3af' }}>
                      • ... and {availableQueries.length - 3} more
                    </Text>
                  )}
                </Space>
              )}
            </Space>
          </>
        )}
      </Space>
    </Card>
  );
};
