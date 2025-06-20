import React, { useState, useMemo } from 'react'
import {
  Table,
  Card,
  Button,
  Space,
  Typography,
  Input,
  Select,
  Tooltip,
  Tag,
  Pagination
} from 'antd'
import {
  SearchOutlined,
  DownloadOutlined,
  FilterOutlined,
  FullscreenOutlined,
  SettingOutlined
} from '@ant-design/icons'
import type { ColumnsType, TableProps } from 'antd/es/table'

const { Text } = Typography
const { Search } = Input
const { Option } = Select

interface DataTableColumn {
  key: string
  title: string
  dataIndex: string
  type?: 'text' | 'number' | 'date' | 'boolean'
  sortable?: boolean
  filterable?: boolean
  width?: number
}

interface DataTableProps {
  data: any[]
  columns: DataTableColumn[]
  title?: string
  loading?: boolean
  pagination?: boolean
  pageSize?: number
  searchable?: boolean
  filterable?: boolean
  exportable?: boolean
  selectable?: boolean
  onRowSelect?: (selectedRows: any[]) => void
  onExport?: (data: any[]) => void
  height?: number
  className?: string
}

export const DataTable: React.FC<DataTableProps> = ({
  data,
  columns,
  title = 'Data Table',
  loading = false,
  pagination = true,
  pageSize = 50,
  searchable = true,
  filterable = true,
  exportable = true,
  selectable = false,
  onRowSelect,
  onExport,
  height,
  className = ''
}) => {
  const [searchText, setSearchText] = useState('')
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([])
  const [currentPage, setCurrent] = useState(1)
  const [pageLimit, setPageLimit] = useState(pageSize)
  const [sortConfig, setSortConfig] = useState<{ key: string; direction: 'asc' | 'desc' } | null>(null)
  const [filters, setFilters] = useState<Record<string, any>>({})

  // Filter and search data
  const filteredData = useMemo(() => {
    let result = [...data]

    // Apply search
    if (searchText) {
      result = result.filter(row =>
        Object.values(row).some(value =>
          String(value).toLowerCase().includes(searchText.toLowerCase())
        )
      )
    }

    // Apply column filters
    Object.entries(filters).forEach(([key, value]) => {
      if (value) {
        result = result.filter(row => {
          const cellValue = String(row[key]).toLowerCase()
          return cellValue.includes(String(value).toLowerCase())
        })
      }
    })

    // Apply sorting
    if (sortConfig) {
      result.sort((a, b) => {
        const aVal = a[sortConfig.key]
        const bVal = b[sortConfig.key]
        
        if (aVal < bVal) return sortConfig.direction === 'asc' ? -1 : 1
        if (aVal > bVal) return sortConfig.direction === 'asc' ? 1 : -1
        return 0
      })
    }

    return result
  }, [data, searchText, filters, sortConfig])

  // Paginated data
  const paginatedData = useMemo(() => {
    if (!pagination) return filteredData
    
    const start = (currentPage - 1) * pageLimit
    const end = start + pageLimit
    return filteredData.slice(start, end)
  }, [filteredData, currentPage, pageLimit, pagination])

  // Convert columns to Ant Design format
  const tableColumns: ColumnsType<any> = useMemo(() => {
    return columns.map(col => ({
      title: col.title,
      dataIndex: col.dataIndex,
      key: col.key,
      width: col.width,
      sorter: col.sortable ? (a: any, b: any) => {
        const aVal = a[col.dataIndex]
        const bVal = b[col.dataIndex]
        if (aVal < bVal) return -1
        if (aVal > bVal) return 1
        return 0
      } : false,
      filterDropdown: col.filterable ? ({ setSelectedKeys, selectedKeys, confirm, clearFilters }) => (
        <div style={{ padding: 8 }}>
          <Input
            placeholder={`Search ${col.title}`}
            value={selectedKeys[0]}
            onChange={e => setSelectedKeys(e.target.value ? [e.target.value] : [])}
            onPressEnter={() => confirm()}
            style={{ marginBottom: 8, display: 'block' }}
          />
          <Space>
            <Button
              type="primary"
              onClick={() => confirm()}
              icon={<SearchOutlined />}
              size="small"
              style={{ width: 90 }}
            >
              Search
            </Button>
            <Button
              onClick={() => clearFilters && clearFilters()}
              size="small"
              style={{ width: 90 }}
            >
              Reset
            </Button>
          </Space>
        </div>
      ) : false,
      filterIcon: col.filterable ? (filtered: boolean) => (
        <SearchOutlined style={{ color: filtered ? '#1890ff' : undefined }} />
      ) : false,
      onFilter: col.filterable ? (value: any, record: any) =>
        String(record[col.dataIndex]).toLowerCase().includes(String(value).toLowerCase())
      : false,
      render: (value: any) => {
        if (col.type === 'number' && typeof value === 'number') {
          return value.toLocaleString()
        }
        if (col.type === 'date' && value) {
          return new Date(value).toLocaleDateString()
        }
        if (col.type === 'boolean') {
          return <Tag color={value ? 'green' : 'red'}>{value ? 'Yes' : 'No'}</Tag>
        }
        return value
      }
    }))
  }, [columns])

  const handleRowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[], selectedRows: any[]) => {
      setSelectedRowKeys(newSelectedRowKeys)
      onRowSelect?.(selectedRows)
    },
  }

  const handleExport = () => {
    onExport?.(filteredData)
  }

  const handlePageChange = (page: number, size?: number) => {
    setCurrent(page)
    if (size) setPageLimit(size)
  }

  return (
    <Card
      title={
        <Space>
          <Text strong>{title}</Text>
          <Tag color="blue">{filteredData.length} rows</Tag>
          {selectedRowKeys.length > 0 && (
            <Tag color="green">{selectedRowKeys.length} selected</Tag>
          )}
        </Space>
      }
      extra={
        <Space>
          {searchable && (
            <Search
              placeholder="Search table..."
              value={searchText}
              onChange={e => setSearchText(e.target.value)}
              style={{ width: 200 }}
              allowClear
            />
          )}
          {exportable && (
            <Tooltip title="Export Data">
              <Button
                icon={<DownloadOutlined />}
                onClick={handleExport}
                disabled={filteredData.length === 0}
              >
                Export
              </Button>
            </Tooltip>
          )}
          <Tooltip title="Table Settings">
            <Button icon={<SettingOutlined />} />
          </Tooltip>
        </Space>
      }
      className={className}
      bodyStyle={{ padding: 0 }}
    >
      <Table
        columns={tableColumns}
        dataSource={paginatedData}
        loading={loading}
        pagination={false}
        rowSelection={selectable ? handleRowSelection : undefined}
        scroll={{ 
          y: height ? height - 120 : undefined,
          x: 'max-content'
        }}
        size="small"
        rowKey={(record, index) => record.id || index}
        style={{
          '.ant-table-thead > tr > th': {
            backgroundColor: '#fafafa',
            fontWeight: 600
          }
        }}
      />
      
      {pagination && (
        <div style={{ 
          padding: '16px', 
          borderTop: '1px solid #f0f0f0',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <Space>
            <Text type="secondary">
              Showing {((currentPage - 1) * pageLimit) + 1} to {Math.min(currentPage * pageLimit, filteredData.length)} of {filteredData.length} entries
            </Text>
            <Select
              value={pageLimit}
              onChange={setPageLimit}
              style={{ width: 80 }}
              size="small"
            >
              <Option value={25}>25</Option>
              <Option value={50}>50</Option>
              <Option value={100}>100</Option>
              <Option value={200}>200</Option>
            </Select>
            <Text type="secondary">per page</Text>
          </Space>
          
          <Pagination
            current={currentPage}
            total={filteredData.length}
            pageSize={pageLimit}
            onChange={handlePageChange}
            showSizeChanger={false}
            showQuickJumper
            size="small"
          />
        </div>
      )}
    </Card>
  )
}
