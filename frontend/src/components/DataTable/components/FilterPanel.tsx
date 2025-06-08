import React from 'react';
import { Card, Button, Space, Input, InputNumber, DatePicker, Select, Radio, Typography } from 'antd';
import { CloseCircleOutlined, DollarOutlined } from '@ant-design/icons';
import { useToken } from 'antd/es/theme/internal';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;

const { Text } = Typography;


interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  dataType?: 'string' | 'number' | 'date' | 'boolean' | 'money' | 'currency' | 'json' | 'array' | 'object' | 'custom';
  filterable?: boolean;
  filterType?: 'text' | 'number' | 'money' | 'date' | 'dateRange' | 'select' | 'multiselect' | 'boolean' | 'custom';
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

  // Debug logging for filter panel
  React.useEffect(() => {
    if (process.env.NODE_ENV === 'development' && visible) {
      console.log('🔍 FilterPanel Debug:', {
        columns: columns.map(col => ({
          key: col.key,
          title: col.title,
          dataType: col.dataType,
          filterType: col.filterType,
          hasFilterOptions: !!col.filterOptions,
          filterOptionsCount: col.filterOptions?.length
        })),
        filterConfig
      });
    }
  }, [visible, columns, filterConfig]);

  if (!visible) return null;

  const renderFilterControl = (column: DataTableColumn) => {
    const filterValue = filterConfig[column.key];

    switch (column.filterType || column.dataType) {
      case 'text':
      case 'string':
        // If column has filterOptions, use multiselect, otherwise use text input
        if (column.filterOptions && column.filterOptions.length > 0) {
          return (
            <Select
              mode="multiple"
              placeholder={`Select ${column.title}`}
              value={filterValue || []}
              onChange={value => onFilterChange(column.key, value)}
              options={column.filterOptions}
              style={{ marginTop: 8, width: '100%' }}
              allowClear
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
            />
          );
        }
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
          <Space direction="vertical" style={{ marginTop: 8, width: '100%' }}>
            <Space>
              <InputNumber
                placeholder="Min"
                value={filterValue?.min}
                onChange={min => onFilterChange(column.key, { ...filterValue, min })}
                style={{ width: '100%' }}
              />
              <InputNumber
                placeholder="Max"
                value={filterValue?.max}
                onChange={max => onFilterChange(column.key, { ...filterValue, max })}
                style={{ width: '100%' }}
              />
            </Space>
          </Space>
        );

      case 'money':
      case 'currency':
        return (
          <Space direction="vertical" style={{ marginTop: 8, width: '100%' }}>
            <Typography.Text type="secondary" style={{ fontSize: '12px' }}>
              <DollarOutlined /> Money Filter
            </Typography.Text>
            <Space>
              <InputNumber
                placeholder="Min Amount"
                value={filterValue?.min}
                onChange={min => onFilterChange(column.key, { ...filterValue, min })}
                prefix="$"
                formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                parser={value => value!.replace(/\$\s?|(,*)/g, '')}
                style={{ width: '120px' }}
              />
              <InputNumber
                placeholder="Max Amount"
                value={filterValue?.max}
                onChange={max => onFilterChange(column.key, { ...filterValue, max })}
                prefix="$"
                formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                parser={value => value!.replace(/\$\s?|(,*)/g, '')}
                style={{ width: '120px' }}
              />
            </Space>
            <Space>
              <Button
                size="small"
                type={filterValue?.operator === 'gte' ? 'primary' : 'default'}
                onClick={() => onFilterChange(column.key, {
                  ...filterValue,
                  operator: filterValue?.operator === 'gte' ? undefined : 'gte'
                })}
              >
                ≥ Greater or Equal
              </Button>
              <Button
                size="small"
                type={filterValue?.operator === 'lte' ? 'primary' : 'default'}
                onClick={() => onFilterChange(column.key, {
                  ...filterValue,
                  operator: filterValue?.operator === 'lte' ? undefined : 'lte'
                })}
              >
                ≤ Less or Equal
              </Button>
            </Space>
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
            value={filterValue ? [
              filterValue[0] ? dayjs(filterValue[0]) : null,
              filterValue[1] ? dayjs(filterValue[1]) : null
            ] : null}
            onChange={dates => onFilterChange(column.key, dates ? [
              dates[0]?.toISOString(),
              dates[1]?.toISOString()
            ] : null)}
            style={{ marginTop: 8, width: '100%' }}
            placeholder={['Start Date', 'End Date']}
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
