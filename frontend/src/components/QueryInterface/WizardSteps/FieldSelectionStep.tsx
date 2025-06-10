/**
 * Field Selection Step
 */

import React, { useState, useEffect } from 'react';
import { Card, Checkbox, Button, Space, Input, Tag, Empty } from 'antd';
import { SearchOutlined, KeyOutlined, LinkOutlined } from '@ant-design/icons';
import { WizardStepProps, Field } from './types';

const { Search } = Input;

const mockFields: Field[] = [
  { name: 'player_id', type: 'int', primaryKey: true, description: 'Unique player identifier' },
  { name: 'action_date', type: 'datetime', description: 'Date and time of the action' },
  { name: 'deposit_amount', type: 'decimal', description: 'Amount deposited by player' },
  { name: 'withdrawal_amount', type: 'decimal', description: 'Amount withdrawn by player' },
  { name: 'country_id', type: 'int', foreignKey: true, description: 'Player country reference' },
  { name: 'currency_id', type: 'int', foreignKey: true, description: 'Currency reference' },
  { name: 'game_type', type: 'varchar', description: 'Type of game played' },
  { name: 'bet_amount', type: 'decimal', description: 'Amount bet by player' },
  { name: 'win_amount', type: 'decimal', description: 'Amount won by player' },
  { name: 'session_duration', type: 'int', description: 'Session duration in minutes' }
];

const FieldSelectionStep: React.FC<WizardStepProps> = ({
  onNext,
  onPrevious,
  data,
  onChange
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedFields, setSelectedFields] = useState<Field[]>(
    data?.selectedFields || []
  );
  const [filteredFields, setFilteredFields] = useState<Field[]>(mockFields);

  useEffect(() => {
    const filtered = mockFields.filter(field =>
      field.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      field.description?.toLowerCase().includes(searchTerm.toLowerCase())
    );
    setFilteredFields(filtered);
  }, [searchTerm]);

  const handleFieldToggle = (field: Field, checked: boolean) => {
    let newSelectedFields: Field[];
    
    if (checked) {
      newSelectedFields = [...selectedFields, field];
    } else {
      newSelectedFields = selectedFields.filter(f => f.name !== field.name);
    }
    
    setSelectedFields(newSelectedFields);
    onChange?.({ ...data, selectedFields: newSelectedFields });
  };

  const handleSelectAll = () => {
    setSelectedFields(filteredFields);
    onChange?.({ ...data, selectedFields: filteredFields });
  };

  const handleClearAll = () => {
    setSelectedFields([]);
    onChange?.({ ...data, selectedFields: [] });
  };

  const isFieldSelected = (field: Field) => {
    return selectedFields.some(f => f.name === field.name);
  };

  const getFieldIcon = (field: Field) => {
    if (field.primaryKey) return <KeyOutlined style={{ color: '#fa8c16' }} />;
    if (field.foreignKey) return <LinkOutlined style={{ color: '#52c41a' }} />;
    return null;
  };

  const getFieldTypeColor = (type: string) => {
    switch (type.toLowerCase()) {
      case 'int':
      case 'bigint':
        return 'blue';
      case 'varchar':
      case 'nvarchar':
      case 'text':
        return 'green';
      case 'decimal':
      case 'float':
      case 'money':
        return 'orange';
      case 'datetime':
      case 'date':
        return 'purple';
      case 'bit':
        return 'red';
      default:
        return 'default';
    }
  };

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <h3 style={{ marginBottom: '8px' }}>Select Fields</h3>
        <p style={{ color: '#8c8c8c', marginBottom: '16px' }}>
          Choose the fields you want to include in your query
        </p>
        
        <div style={{ display: 'flex', gap: '12px', marginBottom: '16px' }}>
          <Search
            placeholder="Search fields..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ flex: 1 }}
            prefix={<SearchOutlined />}
          />
          <Button onClick={handleSelectAll} size="small">
            Select All
          </Button>
          <Button onClick={handleClearAll} size="small">
            Clear All
          </Button>
        </div>
      </div>

      {filteredFields.length > 0 ? (
        <div style={{ 
          maxHeight: '400px', 
          overflowY: 'auto',
          border: '1px solid #f0f0f0',
          borderRadius: '8px',
          padding: '16px'
        }}>
          <Space direction="vertical" style={{ width: '100%' }} size="middle">
            {filteredFields.map((field) => (
              <Card
                key={field.name}
                size="small"
                style={{
                  background: isFieldSelected(field) ? '#f0f8ff' : '#fafafa',
                  border: isFieldSelected(field) ? '1px solid #1890ff' : '1px solid #f0f0f0'
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                  <Checkbox
                    checked={isFieldSelected(field)}
                    onChange={(e) => handleFieldToggle(field, e.target.checked)}
                  />
                  
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '4px' }}>
                      {getFieldIcon(field)}
                      <span style={{ fontWeight: 600, fontSize: '14px' }}>
                        {field.name}
                      </span>
                      <Tag color={getFieldTypeColor(field.type)}>
                        {field.type}
                      </Tag>
                      {field.nullable && (
                        <Tag style={{ fontSize: '10px' }}>
                          NULL
                        </Tag>
                      )}
                    </div>
                    {field.description && (
                      <div style={{ fontSize: '12px', color: '#8c8c8c' }}>
                        {field.description}
                      </div>
                    )}
                  </div>
                </div>
              </Card>
            ))}
          </Space>
        </div>
      ) : (
        <Empty
          description="No fields found"
          style={{ margin: '40px 0' }}
        />
      )}

      <div style={{ 
        marginTop: '24px', 
        paddingTop: '24px', 
        borderTop: '1px solid #f0f0f0',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <div>
          <Space>
            <span style={{ color: '#8c8c8c' }}>Selected:</span>
            <Tag color="blue">{selectedFields.length} fields</Tag>
          </Space>
        </div>
        <Space>
          <Button onClick={onPrevious}>
            Previous
          </Button>
          <Button
            type="primary"
            onClick={onNext}
            disabled={selectedFields.length === 0}
            style={{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none'
            }}
          >
            Next: Add Filters
          </Button>
        </Space>
      </div>
    </div>
  );
};

export default FieldSelectionStep;
