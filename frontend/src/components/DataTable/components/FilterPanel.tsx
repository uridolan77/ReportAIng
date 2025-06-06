import React from 'react';
import { Card, Button, Space, Input, InputNumber, DatePicker, Select, Radio, Typography } from 'antd';
import { CloseCircleOutlined } from '@ant-design/icons';
import { useToken } from 'antd/es/theme/internal';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;

const { Text } = Typography;


interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  dataType?: 'string' | 'number' | 'date' | 'boolean' | 'json' | 'array' | 'object' | 'custom';
  filterable?: boolean;
  filterType?: 'text' | 'number' | 'date' | 'dateRange' | 'select' | 'multiselect' | 'boolean' | 'custom';
  filterOptions?: any[];
  customFilter?: (value: any, filterValue: any) => boolean;
}

interface FilterPanelProps {
  visible: boolean;
  columns: DataTableColumn[];
  filterConfig: Record<string, any>;
  onFilterChange: (column: string, value: any) => void;
  onClose: () => void;
  onClearAll: () => void;
}

export const FilterPanel: React.FC<FilterPanelProps> = ({
  visible,
  columns,
  filterConfig,
  onFilterChange,
  onClose,
  onClearAll
}) => {
  const [, token] = useToken();

  if (!visible) return null;

  const renderFilterControl = (column: DataTableColumn) => {
    const filterValue = filterConfig[column.key];
    
    switch (column.filterType || column.dataType) {
      case 'text':
      case 'string':
        return (
          <Input
            placeholder={`Filter ${column.title}`}
            value={filterValue || ''}
            onChange={e => onFilterChange(column.key, e.target.value)}
            style={{ marginTop: 8 }}
          />
        );
      
      case 'number':
        return (
          <Space style={{ marginTop: 8 }}>
            <InputNumber
              placeholder="Min"
              value={filterValue?.min}
              onChange={min => onFilterChange(column.key, { ...filterValue, min })}
            />
            <InputNumber
              placeholder="Max"
              value={filterValue?.max}
              onChange={max => onFilterChange(column.key, { ...filterValue, max })}
            />
          </Space>
        );
      
      case 'date':
        return (
          <DatePicker
            value={filterValue ? dayjs(filterValue) : null}
            onChange={date => onFilterChange(column.key, date?.toISOString())}
            style={{ marginTop: 8, width: '100%' }}
          />
        );
      
      case 'dateRange':
        return (
          <RangePicker
            value={filterValue}
            onChange={dates => onFilterChange(column.key, dates)}
            style={{ marginTop: 8, width: '100%' }}
          />
        );
      
      case 'select':
        return (
          <Select
            placeholder={`Select ${column.title}`}
            value={filterValue}
            onChange={value => onFilterChange(column.key, value)}
            options={column.filterOptions}
            style={{ marginTop: 8, width: '100%' }}
            allowClear
          />
        );
      
      case 'multiselect':
        return (
          <Select
            mode="multiple"
            placeholder={`Select ${column.title}`}
            value={filterValue || []}
            onChange={value => onFilterChange(column.key, value)}
            options={column.filterOptions}
            style={{ marginTop: 8, width: '100%' }}
          />
        );
      
      case 'boolean':
        return (
          <Radio.Group
            value={filterValue}
            onChange={e => onFilterChange(column.key, e.target.value)}
            style={{ marginTop: 8 }}
          >
            <Radio value={true}>Yes</Radio>
            <Radio value={false}>No</Radio>
            <Radio value={undefined}>All</Radio>
          </Radio.Group>
        );
      
      default:
        return null;
    }
  };

  return (
    <Card
      style={{
        margin: '16px',
        background: token.colorBgContainer,
        borderRadius: token.borderRadius,
        boxShadow: token.boxShadow
      }}
      title="Filters"
      extra={
        <Button
          type="text"
          icon={<CloseCircleOutlined />}
          onClick={onClose}
        />
      }
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        {columns.filter(col => col.filterable !== false).map(column => (
          <div key={column.key}>
            <Text strong>{column.title}</Text>
            {renderFilterControl(column)}
          </div>
        ))}
        <Space>
          <Button
            type="primary"
            onClick={onClose}
          >
            Apply
          </Button>
          <Button
            onClick={() => {
              onClearAll();
              onClose();
            }}
          >
            Clear All
          </Button>
        </Space>
      </Space>
    </Card>
  );
};
