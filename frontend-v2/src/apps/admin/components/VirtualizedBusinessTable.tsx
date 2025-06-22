import React, { useMemo, useState, useCallback } from 'react'
import { FixedSizeList as List } from 'react-window'
import {
  Card,
  Space,
  Typography,
  Tag,
  Button,
  Tooltip,
  Checkbox,
  Input,
  Row,
  Col,
} from 'antd'
import {
  EyeOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import type { BusinessTableInfo } from '@shared/store/api/businessApi'

const { Text } = Typography
const { Search } = Input

interface VirtualizedBusinessTableProps {
  data: BusinessTableInfo[]
  loading?: boolean
  selectedRowKeys: React.Key[]
  onSelectionChange: (selectedRowKeys: React.Key[]) => void
  onView: (record: BusinessTableInfo) => void
  onEdit: (record: BusinessTableInfo) => void
  onDelete: (record: BusinessTableInfo) => void
  onValidate: (record: BusinessTableInfo) => void
  height?: number
}

interface TableRowProps {
  index: number
  style: React.CSSProperties
  data: {
    items: BusinessTableInfo[]
    selectedRowKeys: React.Key[]
    onSelectionChange: (selectedRowKeys: React.Key[]) => void
    onView: (record: BusinessTableInfo) => void
    onEdit: (record: BusinessTableInfo) => void
    onDelete: (record: BusinessTableInfo) => void
    onValidate: (record: BusinessTableInfo) => void
  }
}

const TableRow: React.FC<TableRowProps> = ({ index, style, data }) => {
  const { items, selectedRowKeys, onSelectionChange, onView, onEdit, onDelete, onValidate } = data
  const record = items[index]

  if (!record) return null

  const isSelected = selectedRowKeys.includes(record.id)

  const handleSelectionChange = (checked: boolean) => {
    if (checked) {
      onSelectionChange([...selectedRowKeys, record.id])
    } else {
      onSelectionChange(selectedRowKeys.filter(key => key !== record.id))
    }
  }

  return (
    <div style={style}>
      <Card
        size="small"
        style={{
          margin: '4px 8px',
          backgroundColor: isSelected ? '#e6f7ff' : undefined,
          borderColor: isSelected ? '#1890ff' : undefined,
        }}
        bodyStyle={{ padding: '12px' }}
      >
        <Row align="middle" gutter={16}>
          {/* Selection Checkbox */}
          <Col flex="none">
            <Checkbox
              checked={isSelected}
              onChange={(e) => handleSelectionChange(e.target.checked)}
            />
          </Col>

          {/* Table Info */}
          <Col flex="200px">
            <Space direction="vertical" size="small">
              <Text strong>{record.tableName}</Text>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {record.schemaName}
              </Text>
            </Space>
          </Col>

          {/* Business Purpose */}
          <Col flex="auto">
            <Tooltip title={record.businessPurpose}>
              <Text ellipsis style={{ maxWidth: '100%', display: 'block' }}>
                {record.businessPurpose || 'No purpose defined'}
              </Text>
            </Tooltip>
          </Col>

          {/* Domain */}
          <Col flex="120px">
            <Tag color="blue">{record.domainClassification || 'Unclassified'}</Tag>
          </Col>

          {/* Status */}
          <Col flex="100px">
            <Tag 
              color={record.isActive ? 'green' : 'red'} 
              icon={record.isActive ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
            >
              {record.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Col>

          {/* Columns Count */}
          <Col flex="80px">
            <Text>{record.columns?.length || 0} cols</Text>
          </Col>

          {/* Last Updated */}
          <Col flex="120px">
            <Text style={{ fontSize: '12px' }}>
              {record.updatedDate ? new Date(record.updatedDate).toLocaleDateString() : 'Never'}
            </Text>
          </Col>

          {/* Actions */}
          <Col flex="140px">
            <Space size="small">
              <Tooltip title="View Details">
                <Button
                  size="small"
                  icon={<EyeOutlined />}
                  onClick={() => onView(record)}
                />
              </Tooltip>
              <Tooltip title="Edit">
                <Button
                  size="small"
                  icon={<EditOutlined />}
                  onClick={() => onEdit(record)}
                />
              </Tooltip>
              <Tooltip title="Validate">
                <Button
                  size="small"
                  icon={<CheckCircleOutlined />}
                  onClick={() => onValidate(record)}
                />
              </Tooltip>
              <Tooltip title="Delete">
                <Button
                  size="small"
                  danger
                  icon={<DeleteOutlined />}
                  onClick={() => onDelete(record)}
                />
              </Tooltip>
            </Space>
          </Col>
        </Row>
      </Card>
    </div>
  )
}

export const VirtualizedBusinessTable: React.FC<VirtualizedBusinessTableProps> = ({
  data,
  loading = false,
  selectedRowKeys,
  onSelectionChange,
  onView,
  onEdit,
  onDelete,
  onValidate,
  height = 600,
}) => {
  const [searchTerm, setSearchTerm] = useState('')

  // Filter data based on search term
  const filteredData = useMemo(() => {
    if (!searchTerm) return data
    
    const term = searchTerm.toLowerCase()
    return data.filter(item => 
      item.tableName.toLowerCase().includes(term) ||
      item.schemaName.toLowerCase().includes(term) ||
      item.businessPurpose.toLowerCase().includes(term) ||
      item.domainClassification.toLowerCase().includes(term)
    )
  }, [data, searchTerm])

  const handleSelectAll = useCallback((checked: boolean) => {
    if (checked) {
      onSelectionChange(filteredData.map(item => item.id))
    } else {
      onSelectionChange([])
    }
  }, [filteredData, onSelectionChange])

  const isAllSelected = filteredData.length > 0 && filteredData.every(item => selectedRowKeys.includes(item.id))
  const isIndeterminate = selectedRowKeys.length > 0 && !isAllSelected

  const itemData = {
    items: filteredData,
    selectedRowKeys,
    onSelectionChange,
    onView,
    onEdit,
    onDelete,
    onValidate,
  }

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Text>Loading tables...</Text>
        </div>
      </Card>
    )
  }

  return (
    <Card>
      {/* Header Controls */}
      <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
        <Col>
          <Space>
            <Checkbox
              indeterminate={isIndeterminate}
              checked={isAllSelected}
              onChange={(e) => handleSelectAll(e.target.checked)}
            >
              Select All ({filteredData.length} items)
            </Checkbox>
            {selectedRowKeys.length > 0 && (
              <Text type="secondary">
                {selectedRowKeys.length} selected
              </Text>
            )}
          </Space>
        </Col>
        <Col>
          <Search
            placeholder="Search tables..."
            allowClear
            style={{ width: 300 }}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            prefix={<SearchOutlined />}
          />
        </Col>
      </Row>

      {/* Column Headers */}
      <Card size="small" style={{ marginBottom: 8 }}>
        <Row align="middle" gutter={16}>
          <Col flex="none" style={{ width: 40 }}></Col>
          <Col flex="200px">
            <Text strong>Table</Text>
          </Col>
          <Col flex="auto">
            <Text strong>Business Purpose</Text>
          </Col>
          <Col flex="120px">
            <Text strong>Domain</Text>
          </Col>
          <Col flex="100px">
            <Text strong>Status</Text>
          </Col>
          <Col flex="80px">
            <Text strong>Columns</Text>
          </Col>
          <Col flex="120px">
            <Text strong>Updated</Text>
          </Col>
          <Col flex="140px">
            <Text strong>Actions</Text>
          </Col>
        </Row>
      </Card>

      {/* Virtualized List */}
      <div style={{ border: '1px solid #f0f0f0', borderRadius: '6px' }}>
        <List
          height={height}
          itemCount={filteredData.length}
          itemSize={80}
          itemData={itemData}
          overscanCount={5}
        >
          {TableRow}
        </List>
      </div>

      {/* Footer Info */}
      <div style={{ marginTop: 16, textAlign: 'center' }}>
        <Text type="secondary">
          Showing {filteredData.length} of {data.length} tables
          {searchTerm && ` (filtered by "${searchTerm}")`}
        </Text>
      </div>
    </Card>
  )
}

export default VirtualizedBusinessTable
