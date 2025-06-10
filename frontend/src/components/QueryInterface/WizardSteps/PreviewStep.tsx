/**
 * Preview Step
 */

import React from 'react';
import { Button, Space, Card, Typography } from 'antd';
import { PlayCircleOutlined } from '@ant-design/icons';
import { WizardStepProps } from './types';

const { Text, Paragraph } = Typography;

const PreviewStep: React.FC<WizardStepProps> = ({
  onPrevious,
  onComplete,
  data
}) => {
  const generatePreviewSQL = () => {
    if (!data?.dataSource || !data?.selectedFields) {
      return 'SELECT * FROM table_name';
    }

    const fields = data.selectedFields.map(f => f.name).join(', ');
    const tableName = data.dataSource.name;
    
    return `SELECT ${fields}\nFROM ${tableName}\nORDER BY ${data.selectedFields[0]?.name || 'id'} DESC`;
  };

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <h3 style={{ marginBottom: '8px' }}>Preview & Execute</h3>
        <p style={{ color: '#8c8c8c', marginBottom: '16px' }}>
          Review your query and execute it
        </p>
      </div>

      <Card title="Generated SQL Query" style={{ marginBottom: '24px' }}>
        <Paragraph
          code
          copyable
          style={{
            background: '#f5f5f5',
            padding: '16px',
            borderRadius: '8px',
            fontFamily: 'Courier New, monospace',
            fontSize: '14px',
            lineHeight: '1.5',
            whiteSpace: 'pre-wrap'
          }}
        >
          {generatePreviewSQL()}
        </Paragraph>
      </Card>

      <Card title="Query Summary">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Data Source: </Text>
            <Text>{data?.dataSource?.name || 'Not selected'}</Text>
          </div>
          <div>
            <Text strong>Selected Fields: </Text>
            <Text>{data?.selectedFields?.length || 0} fields</Text>
          </div>
          <div>
            <Text strong>Filters: </Text>
            <Text>{data?.filters?.length || 0} conditions</Text>
          </div>
          <div>
            <Text strong>Grouping: </Text>
            <Text>{data?.grouping?.length || 0} groups</Text>
          </div>
          <div>
            <Text strong>Sorting: </Text>
            <Text>{data?.sorting?.length || 0} sort orders</Text>
          </div>
        </Space>
      </Card>

      <div style={{ 
        marginTop: '24px', 
        paddingTop: '24px', 
        borderTop: '1px solid #f0f0f0',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <div />
        <Space>
          <Button onClick={onPrevious}>
            Previous
          </Button>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={onComplete}
            style={{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none'
            }}
          >
            Execute Query
          </Button>
        </Space>
      </div>
    </div>
  );
};

export default PreviewStep;
