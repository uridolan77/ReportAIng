// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\test\FilteringTest.tsx
import React from 'react';
import { Card, Typography } from 'antd';
import DataTable from '../DataTableMain';
import { DataTableColumn } from '../types';

const { Title } = Typography;

// Simple test data
const testData = [
  {
    id: 1,
    name: 'John Doe',
    salary: 50000,
    department: 'Engineering',
    joinDate: '2023-01-15',
    isActive: true
  },
  {
    id: 2,
    name: 'Jane Smith',
    salary: 45000,
    department: 'Marketing',
    joinDate: '2023-02-20',
    isActive: false
  },
  {
    id: 3,
    name: 'Bob Johnson',
    salary: 60000,
    department: 'Engineering',
    joinDate: '2023-03-10',
    isActive: true
  }
];

// Simple column configuration
const testColumns: DataTableColumn[] = [
  {
    key: 'name',
    title: 'Name',
    dataIndex: 'name',
    dataType: 'string',
    filterType: 'text',
    filterable: true,
    sortable: true
  },
  {
    key: 'salary',
    title: 'Salary',
    dataIndex: 'salary',
    dataType: 'money',
    filterType: 'money',
    filterable: true,
    sortable: true
  },
  {
    key: 'department',
    title: 'Department',
    dataIndex: 'department',
    dataType: 'string',
    filterType: 'multiselect',
    filterable: true,
    filterOptions: [
      { label: 'Engineering', value: 'Engineering' },
      { label: 'Marketing', value: 'Marketing' }
    ]
  },
  {
    key: 'isActive',
    title: 'Active',
    dataIndex: 'isActive',
    dataType: 'boolean',
    filterType: 'boolean',
    filterable: true
  }
];

const FilteringTest: React.FC = () => {
  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>DataTable Filtering Test</Title>
      
      <Card>
        <DataTable
          data={testData}
          columns={testColumns}
          keyField="id"
          features={{
            filtering: true,
            searching: true,
            sorting: true
          }}
        />
      </Card>
    </div>
  );
};

export default FilteringTest;
