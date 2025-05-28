import React, { useState, useEffect } from 'react';
import { Card, Button, Select, Input, DatePicker, Space, Typography, Row, Col, Tag, Divider } from 'antd';
import { FilterOutlined, PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { QueryBuilderData } from '../QueryWizard';
import dayjs from 'dayjs';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

interface FilterCondition {
  column: string;
  operator: string;
  value: any;
  type: 'and' | 'or';
}

interface FilterColumn {
  name: string;
  type: string;
  category: 'numeric' | 'text' | 'date' | 'boolean';
  values?: string[]; // For categorical columns
}

interface FilterBuilderProps {
  data: QueryBuilderData;
  onChange: (data: Partial<QueryBuilderData>) => void;
  onNext?: () => void;
  onPrevious?: () => void;
}

export const FilterBuilder: React.FC<FilterBuilderProps> = ({
  data,
  onChange,
}) => {
  const [availableColumns, setAvailableColumns] = useState<FilterColumn[]>([]);
  const [conditions, setConditions] = useState<FilterCondition[]>(
    data.filters?.conditions || []
  );

  // Mock columns based on selected data source
  useEffect(() => {
    if (!data.dataSource?.table) return;

    const getColumnsForTable = (tableName: string): FilterColumn[] => {
      switch (tableName) {
        case 'tbl_Daily_actions':
          return [
            { name: 'ActionDate', type: 'date', category: 'date' },
            { name: 'WhiteLabelID', type: 'int', category: 'numeric', values: ['1', '2', '3', '4', '5'] },
            { name: 'PlayerID', type: 'int', category: 'numeric' },
            { name: 'Deposits', type: 'decimal', category: 'numeric' },
            { name: 'Bets', type: 'decimal', category: 'numeric' },
            { name: 'Wins', type: 'decimal', category: 'numeric' }
          ];
        case 'tbl_Daily_actions_players':
          return [
            { name: 'RegistrationDate', type: 'date', category: 'date' },
            { name: 'CountryID', type: 'int', category: 'numeric' },
            { name: 'Status', type: 'varchar', category: 'text', values: ['Active', 'Suspended', 'Closed'] },
            { name: 'PlayerID', type: 'int', category: 'numeric' }
          ];
        default:
          return [
            { name: 'ID', type: 'int', category: 'numeric' },
            { name: 'Name', type: 'varchar', category: 'text' }
          ];
      }
    };

    setAvailableColumns(getColumnsForTable(data.dataSource.table));
  }, [data.dataSource]);

  const getOperatorsForColumn = (column: FilterColumn): Array<{ value: string; label: string }> => {
    switch (column.category) {
      case 'numeric':
        return [
          { value: '=', label: 'equals' },
          { value: '!=', label: 'not equals' },
          { value: '>', label: 'greater than' },
          { value: '>=', label: 'greater than or equal' },
          { value: '<', label: 'less than' },
          { value: '<=', label: 'less than or equal' },
          { value: 'between', label: 'between' },
          { value: 'in', label: 'in list' }
        ];
      case 'text':
        return [
          { value: '=', label: 'equals' },
          { value: '!=', label: 'not equals' },
          { value: 'like', label: 'contains' },
          { value: 'not like', label: 'does not contain' },
          { value: 'in', label: 'in list' },
          { value: 'starts with', label: 'starts with' },
          { value: 'ends with', label: 'ends with' }
        ];
      case 'date':
        return [
          { value: '=', label: 'on date' },
          { value: '>', label: 'after' },
          { value: '<', label: 'before' },
          { value: 'between', label: 'between dates' },
          { value: 'last 7 days', label: 'last 7 days' },
          { value: 'last 30 days', label: 'last 30 days' },
          { value: 'this month', label: 'this month' },
          { value: 'this year', label: 'this year' }
        ];
      default:
        return [{ value: '=', label: 'equals' }];
    }
  };

  const addCondition = () => {
    const newCondition: FilterCondition = {
      column: '',
      operator: '',
      value: '',
      type: conditions.length === 0 ? 'and' : 'and'
    };
    const newConditions = [...conditions, newCondition];
    setConditions(newConditions);
    updateParent(newConditions);
  };

  const removeCondition = (index: number) => {
    const newConditions = conditions.filter((_, i) => i !== index);
    setConditions(newConditions);
    updateParent(newConditions);
  };

  const updateCondition = (index: number, field: keyof FilterCondition, value: any) => {
    const newConditions = [...conditions];
    newConditions[index] = { ...newConditions[index], [field]: value };

    // Reset operator and value when column changes
    if (field === 'column') {
      newConditions[index].operator = '';
      newConditions[index].value = '';
    }

    setConditions(newConditions);
    updateParent(newConditions);
  };

  const updateParent = (newConditions: FilterCondition[]) => {
    onChange({
      filters: {
        conditions: newConditions
      }
    });
  };

  const renderValueInput = (condition: FilterCondition, index: number) => {
    const column = availableColumns.find(col => col.name === condition.column);
    if (!column || !condition.operator) return null;

    const commonProps = {
      style: { width: '100%' },
      placeholder: 'Enter value...',
      value: condition.value,
      onChange: (value: any) => updateCondition(index, 'value', value)
    };

    // Special operators that don't need value input
    if (['last 7 days', 'last 30 days', 'this month', 'this year'].includes(condition.operator)) {
      return <Text type="secondary">No additional value needed</Text>;
    }

    // Date inputs
    if (column.category === 'date') {
      if (condition.operator === 'between') {
        return (
          <RangePicker
            style={{ width: '100%' }}
            placeholder={['Start date', 'End date']}
            value={condition.value ? [dayjs(condition.value[0]), dayjs(condition.value[1])] : null}
            onChange={(dates) => updateCondition(index, 'value', dates ? [dates[0]?.format('YYYY-MM-DD'), dates[1]?.format('YYYY-MM-DD')] : null)}
          />
        );
      }
      return (
        <DatePicker
          {...commonProps}
          value={condition.value ? dayjs(condition.value) : null}
          onChange={(date) => updateCondition(index, 'value', date?.format('YYYY-MM-DD'))}
        />
      );
    }

    // Categorical values (dropdown)
    if (column.values && ['=', '!=', 'in'].includes(condition.operator)) {
      return (
        <Select
          {...commonProps}
          mode={condition.operator === 'in' ? 'multiple' : undefined}
          placeholder={condition.operator === 'in' ? 'Select values...' : 'Select value...'}
          onChange={(value) => updateCondition(index, 'value', value)}
        >
          {column.values.map(val => (
            <Option key={val} value={val}>{val}</Option>
          ))}
        </Select>
      );
    }

    // Numeric range input
    if (column.category === 'numeric' && condition.operator === 'between') {
      return (
        <Input.Group compact>
          <Input
            style={{ width: '45%' }}
            placeholder="Min"
            value={condition.value?.[0] || ''}
            onChange={(e) => updateCondition(index, 'value', [e.target.value, condition.value?.[1] || ''])}
          />
          <Input
            style={{ width: '10%', textAlign: 'center', pointerEvents: 'none' }}
            placeholder="~"
            disabled
          />
          <Input
            style={{ width: '45%' }}
            placeholder="Max"
            value={condition.value?.[1] || ''}
            onChange={(e) => updateCondition(index, 'value', [condition.value?.[0] || '', e.target.value])}
          />
        </Input.Group>
      );
    }

    // Multiple values input
    if (condition.operator === 'in') {
      return (
        <Select
          {...commonProps}
          mode="tags"
          placeholder="Enter values separated by comma..."
          onChange={(value) => updateCondition(index, 'value', value)}
        />
      );
    }

    // Default text input
    return (
      <Input
        {...commonProps}
        onChange={(e) => updateCondition(index, 'value', e.target.value)}
      />
    );
  };

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <Title level={4}>
          <FilterOutlined /> Apply Filters
        </Title>
        <Paragraph type="secondary">
          Add filters to narrow down your data. Filters are optional but can help you focus on specific subsets of data.
        </Paragraph>
      </div>

      {conditions.length === 0 ? (
        <Card style={{ textAlign: 'center', padding: '40px' }}>
          <Text type="secondary">No filters applied. Your query will include all data.</Text>
          <br />
          <Button
            type="dashed"
            icon={<PlusOutlined />}
            onClick={addCondition}
            style={{ marginTop: '16px' }}
            disabled={!data.dataSource?.table}
          >
            Add Filter
          </Button>
        </Card>
      ) : (
        <Space direction="vertical" style={{ width: '100%' }}>
          {conditions.map((condition, index) => (
            <Card key={index} size="small">
              <Row gutter={[8, 8]} align="middle">
                {index > 0 && (
                  <Col span={2}>
                    <Select
                      value={condition.type}
                      onChange={(value) => updateCondition(index, 'type', value)}
                      style={{ width: '100%' }}
                    >
                      <Option value="and">AND</Option>
                      <Option value="or">OR</Option>
                    </Select>
                  </Col>
                )}

                <Col span={index > 0 ? 5 : 6}>
                  <Select
                    placeholder="Select column"
                    value={condition.column}
                    onChange={(value) => updateCondition(index, 'column', value)}
                    style={{ width: '100%' }}
                  >
                    {availableColumns.map(col => (
                      <Option key={col.name} value={col.name}>
                        {col.name} ({col.type})
                      </Option>
                    ))}
                  </Select>
                </Col>

                <Col span={index > 0 ? 4 : 5}>
                  <Select
                    placeholder="Operator"
                    value={condition.operator}
                    onChange={(value) => updateCondition(index, 'operator', value)}
                    style={{ width: '100%' }}
                    disabled={!condition.column}
                  >
                    {condition.column && getOperatorsForColumn(
                      availableColumns.find(col => col.name === condition.column)!
                    ).map(op => (
                      <Option key={op.value} value={op.value}>{op.label}</Option>
                    ))}
                  </Select>
                </Col>

                <Col span={index > 0 ? 10 : 11}>
                  {renderValueInput(condition, index)}
                </Col>

                <Col span={2}>
                  <Button
                    type="text"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={() => removeCondition(index)}
                  />
                </Col>
              </Row>
            </Card>
          ))}

          <Button
            type="dashed"
            icon={<PlusOutlined />}
            onClick={addCondition}
            style={{ width: '100%' }}
          >
            Add Another Filter
          </Button>
        </Space>
      )}

      {conditions.length > 0 && (
        <Card style={{ marginTop: '16px', backgroundColor: '#f6ffed', border: '1px solid #b7eb8f' }}>
          <Text strong style={{ color: '#52c41a' }}>
            ✓ Applied {conditions.length} filter{conditions.length > 1 ? 's' : ''}
          </Text>
          <div style={{ marginTop: '8px' }}>
            {conditions.map((condition, index) => (
              <Tag key={index} color="green" style={{ margin: '2px' }}>
                {index > 0 && `${condition.type.toUpperCase()} `}
                {condition.column} {condition.operator} {
                  Array.isArray(condition.value) ? condition.value.join(', ') : condition.value
                }
              </Tag>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
};
