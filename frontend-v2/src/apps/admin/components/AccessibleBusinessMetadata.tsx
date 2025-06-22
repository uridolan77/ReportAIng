import React, { useState, useEffect, useRef } from 'react'
import {
  Card,
  Table,
  Button,
  Space,
  Typography,
  Input,
  Select,
  Row,
  Col,
  Drawer,
  Tag,
  Tooltip,
  message,
} from 'antd'
import {
  SearchOutlined,
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  MenuOutlined,
  FilterOutlined,
} from '@ant-design/icons'
import { useResponsive } from '@shared/hooks/useResponsive'
import type { BusinessTableInfo } from '@shared/store/api/businessApi'
import type { ColumnsType } from 'antd/es/table'

const { Title, Text } = Typography
const { Search } = Input
const { Option } = Select

interface AccessibleBusinessMetadataProps {
  data: BusinessTableInfo[]
  loading?: boolean
  selectedRowKeys: React.Key[]
  onSelectionChange: (selectedRowKeys: React.Key[]) => void
  onView: (record: BusinessTableInfo) => void
  onEdit: (record: BusinessTableInfo) => void
  onDelete: (record: BusinessTableInfo) => void
  onValidate: (record: BusinessTableInfo) => void
  pagination?: any
  onPageChange?: (page: number, pageSize?: number) => void
}

export const AccessibleBusinessMetadata: React.FC<AccessibleBusinessMetadataProps> = ({
  data,
  loading = false,
  selectedRowKeys,
  onSelectionChange,
  onView,
  onEdit,
  onDelete,
  onValidate,
  pagination,
  onPageChange,
}) => {
  const { isMobile, isTablet } = useResponsive()
  const [filtersVisible, setFiltersVisible] = useState(false)
  const [searchValue, setSearchValue] = useState('')
  const [selectedSchema, setSelectedSchema] = useState<string>('')
  const [selectedDomain, setSelectedDomain] = useState<string>('')
  const searchInputRef = useRef<any>(null)

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      // Ctrl/Cmd + F to focus search
      if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
        event.preventDefault()
        searchInputRef.current?.focus()
        message.info('Search focused. Use Tab to navigate through results.')
      }

      // Escape to clear search
      if (event.key === 'Escape' && searchValue) {
        setSearchValue('')
        message.info('Search cleared')
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [searchValue])

  // Mobile-optimized columns
  const mobileColumns: ColumnsType<BusinessTableInfo> = [
    {
      title: 'Table Information',
      key: 'tableInfo',
      render: (_, record) => (
        <div>
          <div style={{ fontWeight: 'bold', marginBottom: 4 }}>
            {record.tableName}
          </div>
          <div style={{ fontSize: '12px', color: '#666', marginBottom: 4 }}>
            Schema: {record.schemaName}
          </div>
          <div style={{ fontSize: '12px', marginBottom: 8 }}>
            {record.businessPurpose || 'No purpose defined'}
          </div>
          <Space size="small">
            <Tag color="blue">{record.domainClassification || 'Unclassified'}</Tag>
            <Tag color={record.isActive ? 'green' : 'red'}>
              {record.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Space>
        </div>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space direction="vertical" size="small">
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => onView(record)}
            aria-label={`View details for ${record.tableName}`}
            block
          >
            View
          </Button>
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => onEdit(record)}
            aria-label={`Edit ${record.tableName}`}
            block
          >
            Edit
          </Button>
        </Space>
      ),
    },
  ]

  // Desktop columns with accessibility enhancements
  const desktopColumns: ColumnsType<BusinessTableInfo> = [
    {
      title: 'Table',
      key: 'table',
      width: 200,
      render: (_, record) => (
        <div>
          <div style={{ fontWeight: 'bold' }}>{record.tableName}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.schemaName}
          </Text>
        </div>
      ),
      sorter: true,
    },
    {
      title: 'Business Purpose',
      dataIndex: 'businessPurpose',
      key: 'businessPurpose',
      ellipsis: {
        showTitle: false,
      },
      render: (purpose: string, record) => (
        <Tooltip title={purpose || 'No purpose defined'}>
          <span aria-label={`Business purpose: ${purpose || 'No purpose defined'}`}>
            {purpose || 'No purpose defined'}
          </span>
        </Tooltip>
      ),
    },
    {
      title: 'Domain',
      dataIndex: 'domainClassification',
      key: 'domainClassification',
      width: 120,
      render: (domain: string) => (
        <Tag color="blue" aria-label={`Domain: ${domain || 'Unclassified'}`}>
          {domain || 'Unclassified'}
        </Tag>
      ),
      filters: [
        { text: 'Sales', value: 'Sales' },
        { text: 'Finance', value: 'Finance' },
        { text: 'HR', value: 'HR' },
        { text: 'Operations', value: 'Operations' },
        { text: 'Marketing', value: 'Marketing' },
      ],
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => (
        <Tag 
          color={isActive ? 'green' : 'red'} 
          icon={isActive ? <CheckCircleOutlined /> : undefined}
          aria-label={`Status: ${isActive ? 'Active' : 'Inactive'}`}
        >
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
      filters: [
        { text: 'Active', value: true },
        { text: 'Inactive', value: false },
      ],
    },
    {
      title: 'Columns',
      key: 'columns',
      width: 80,
      render: (_, record) => (
        <span aria-label={`${record.columns?.length || 0} columns`}>
          {record.columns?.length || 0}
        </span>
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'updatedDate',
      key: 'updatedDate',
      width: 120,
      render: (date: string) => {
        const formattedDate = date ? new Date(date).toLocaleDateString() : 'Never'
        return (
          <span aria-label={`Last updated: ${formattedDate}`} style={{ fontSize: '12px' }}>
            {formattedDate}
          </span>
        )
      },
      sorter: true,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 160,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => onView(record)}
              aria-label={`View details for ${record.tableName}`}
            />
          </Tooltip>
          <Tooltip title="Edit">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => onEdit(record)}
              aria-label={`Edit ${record.tableName}`}
            />
          </Tooltip>
          <Tooltip title="Validate">
            <Button
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={() => onValidate(record)}
              aria-label={`Validate ${record.tableName}`}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={() => onDelete(record)}
              aria-label={`Delete ${record.tableName}`}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const columns = isMobile ? mobileColumns : desktopColumns

  // Row selection with accessibility
  const rowSelection = {
    selectedRowKeys,
    onChange: onSelectionChange,
    onSelectAll: (selected: boolean, selectedRows: BusinessTableInfo[], changeRows: BusinessTableInfo[]) => {
      const action = selected ? 'selected' : 'deselected'
      message.info(`All visible rows ${action}`)
    },
    getCheckboxProps: (record: BusinessTableInfo) => ({
      'aria-label': `Select ${record.tableName}`,
    }),
  }

  const renderFilters = () => (
    <Space direction={isMobile ? 'vertical' : 'horizontal'} style={{ width: '100%' }}>
      <Search
        ref={searchInputRef}
        placeholder="Search tables, purposes, or domains..."
        allowClear
        enterButton={<SearchOutlined />}
        size={isMobile ? 'large' : 'middle'}
        value={searchValue}
        onChange={(e) => setSearchValue(e.target.value)}
        style={{ width: isMobile ? '100%' : 300 }}
        aria-label="Search business metadata tables"
      />
      <Select
        placeholder="Schema"
        style={{ width: isMobile ? '100%' : 150 }}
        allowClear
        value={selectedSchema}
        onChange={setSelectedSchema}
        size={isMobile ? 'large' : 'middle'}
        aria-label="Filter by schema"
      >
        <Option value="dbo">dbo</Option>
        <Option value="sales">sales</Option>
        <Option value="finance">finance</Option>
        <Option value="hr">hr</Option>
      </Select>
      <Select
        placeholder="Domain"
        style={{ width: isMobile ? '100%' : 150 }}
        allowClear
        value={selectedDomain}
        onChange={setSelectedDomain}
        size={isMobile ? 'large' : 'middle'}
        aria-label="Filter by domain"
      >
        <Option value="Sales">Sales</Option>
        <Option value="Finance">Finance</Option>
        <Option value="HR">HR</Option>
        <Option value="Operations">Operations</Option>
        <Option value="Marketing">Marketing</Option>
      </Select>
    </Space>
  )

  return (
    <div>
      {/* Mobile Filter Drawer */}
      {isMobile && (
        <>
          <Card style={{ marginBottom: 16 }}>
            <Button
              icon={<FilterOutlined />}
              onClick={() => setFiltersVisible(true)}
              block
              size="large"
              aria-label="Open filters"
            >
              Filters & Search
            </Button>
          </Card>
          
          <Drawer
            title="Filters & Search"
            placement="bottom"
            height="auto"
            open={filtersVisible}
            onClose={() => setFiltersVisible(false)}
            bodyStyle={{ padding: '16px' }}
          >
            {renderFilters()}
          </Drawer>
        </>
      )}

      {/* Desktop Filters */}
      {!isMobile && (
        <Card style={{ marginBottom: 16 }}>
          {renderFilters()}
        </Card>
      )}

      {/* Selection Info */}
      {selectedRowKeys.length > 0 && (
        <Card 
          style={{ 
            marginBottom: 16, 
            backgroundColor: '#f6ffed', 
            borderColor: '#b7eb8f' 
          }}
          role="status"
          aria-live="polite"
        >
          <Text strong>
            {selectedRowKeys.length} table(s) selected
          </Text>
          <Button 
            size="small" 
            style={{ marginLeft: 8 }}
            onClick={() => onSelectionChange([])}
            aria-label="Clear selection"
          >
            Clear Selection
          </Button>
        </Card>
      )}

      {/* Main Table */}
      <Card>
        <Table
          rowSelection={rowSelection}
          columns={columns}
          dataSource={data}
          rowKey="id"
          loading={loading}
          pagination={pagination ? {
            ...pagination,
            showSizeChanger: !isMobile,
            showQuickJumper: !isMobile,
            showTotal: (total, range) => 
              `${range[0]}-${range[1]} of ${total} tables`,
            onChange: onPageChange,
            onShowSizeChange: onPageChange,
            size: isMobile ? 'small' : 'default',
          } : false}
          scroll={{ x: isMobile ? 300 : 1200 }}
          size={isMobile ? 'small' : 'middle'}
          locale={{
            emptyText: 'No business metadata tables found',
          }}
          aria-label="Business metadata tables"
        />
      </Card>

      {/* Keyboard shortcuts info */}
      <Card size="small" style={{ marginTop: 16 }}>
        <Text type="secondary" style={{ fontSize: '12px' }}>
          <strong>Keyboard shortcuts:</strong> Ctrl/Cmd + F (focus search), Escape (clear search), 
          Tab (navigate), Enter (activate), Space (select)
        </Text>
      </Card>
    </div>
  )
}

export default AccessibleBusinessMetadata
