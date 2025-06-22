import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Typography,
  Input,
  Select,
  Row,
  Col,
  Statistic,
  Modal,
  message,
  Tooltip,
  Dropdown,
  Checkbox,
  Progress,
  Tabs,
  Drawer,
} from 'antd'
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  SettingOutlined,
  FilterOutlined,
  ExportOutlined,
  AppstoreOutlined,
  EyeOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BarChartOutlined,
  LineChartOutlined,
  BugOutlined,
  ThunderboltOutlined,
  MobileOutlined,
  BookOutlined,
  ApartmentOutlined,
  LinkOutlined,
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { useEnhancedBusinessMetadata } from '@shared/hooks/useEnhancedBusinessMetadata'
import AdvancedBusinessMetadataSearch from '../components/AdvancedBusinessMetadataSearch'
import ValidationDashboard from '../components/ValidationDashboard'
import QualityScoreVisualization from '../components/QualityScoreVisualization'
import MetadataAnalyticsDashboard from '../components/MetadataAnalyticsDashboard'
import MetadataExportManager from '../components/MetadataExportManager'
import EnhancedAPITester from '../components/EnhancedAPITester'
import AccessibleBusinessMetadata from '../components/AccessibleBusinessMetadata'
import VirtualizedBusinessTable from '../components/VirtualizedBusinessTable'
import EnhancedBusinessGlossary from '../components/EnhancedBusinessGlossary'
import EnhancedBusinessDomains from '../components/EnhancedBusinessDomains'
import GlossaryTableIntegration from '../components/GlossaryTableIntegration'
import DomainGlossaryAnalytics from '../components/DomainGlossaryAnalytics'
import type { BusinessTableInfo } from '@shared/store/api/businessApi'
import type { ColumnsType } from 'antd/es/table'

const { Title, Text } = Typography
const { Search } = Input
const { Option } = Select
const { TabPane } = Tabs

export const EnhancedBusinessMetadata: React.FC = () => {
  const navigate = useNavigate()
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([])
  const [bulkModalVisible, setBulkModalVisible] = useState(false)
  const [deleteModalVisible, setDeleteModalVisible] = useState(false)
  const [advancedSearchVisible, setAdvancedSearchVisible] = useState(false)
  const [validationDrawerVisible, setValidationDrawerVisible] = useState(false)
  const [exportModalVisible, setExportModalVisible] = useState(false)
  const [selectedTableForValidation, setSelectedTableForValidation] = useState<BusinessTableInfo | null>(null)
  const [activeTab, setActiveTab] = useState('tables')
  const [tableToDelete, setTableToDelete] = useState<BusinessTableInfo | null>(null)

  const {
    tables,
    pagination,
    statistics,
    selectedTableIds,
    tablesLoading,
    statisticsLoading,
    deleting,
    searching,
    bulkOperating,
    handleSearch,
    handleFilterChange,
    handlePageChange,
    handleTableSelection,
    clearSelection,
    handleBulkOperation,
    handleDeleteTable,
    handleAdvancedSearch,
    refetchTables,
    refetchStatistics,
    tableParams,
  } = useEnhancedBusinessMetadata()

  // Handle table row selection
  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[]) => {
      setSelectedRowKeys(newSelectedRowKeys)
      handleTableSelection(newSelectedRowKeys.map(key => Number(key)))
    },
    onSelectAll: (selected: boolean, selectedRows: BusinessTableInfo[], changeRows: BusinessTableInfo[]) => {
      if (selected) {
        const allKeys = tables.map(table => table.id)
        setSelectedRowKeys(allKeys)
        handleTableSelection(allKeys)
      } else {
        setSelectedRowKeys([])
        clearSelection()
      }
    },
  }

  // Table columns configuration
  const columns: ColumnsType<BusinessTableInfo> = [
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
      render: (purpose: string) => (
        <Tooltip title={purpose}>
          <Text>{purpose || 'No purpose defined'}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Domain',
      dataIndex: 'domainClassification',
      key: 'domainClassification',
      width: 120,
      render: (domain: string) => (
        <Tag color="blue">{domain || 'Unclassified'}</Tag>
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
        <Tag color={isActive ? 'green' : 'red'} icon={isActive ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}>
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
        <Text>{record.columns?.length || 0}</Text>
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'updatedDate',
      key: 'updatedDate',
      width: 120,
      render: (date: string) => (
        <Text style={{ fontSize: '12px' }}>
          {date ? new Date(date).toLocaleDateString() : 'Never'}
        </Text>
      ),
      sorter: true,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => navigate(`/admin/business-metadata/view/${record.id}`)}
            />
          </Tooltip>
          <Tooltip title="Edit">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => navigate(`/admin/business-metadata/edit/${record.id}`)}
            />
          </Tooltip>
          <Tooltip title="Validate">
            <Button
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={() => {
                setSelectedTableForValidation(record)
                setValidationDrawerVisible(true)
              }}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                setTableToDelete(record)
                setDeleteModalVisible(true)
              }}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  // Bulk operation menu items
  const bulkMenuItems = [
    {
      key: 'activate',
      label: 'Activate Selected',
      icon: <CheckCircleOutlined />,
      onClick: () => handleBulkAction('Activate'),
    },
    {
      key: 'deactivate',
      label: 'Deactivate Selected',
      icon: <ExclamationCircleOutlined />,
      onClick: () => handleBulkAction('Deactivate'),
    },
    {
      key: 'delete',
      label: 'Delete Selected',
      icon: <DeleteOutlined />,
      danger: true,
      onClick: () => handleBulkAction('Delete'),
    },
  ]

  const handleBulkAction = async (operation: string) => {
    if (selectedTableIds.length === 0) {
      message.warning('Please select tables first')
      return
    }

    try {
      await handleBulkOperation(operation as any)
      message.success(`${operation} operation completed successfully`)
    } catch (error) {
      message.error(`${operation} operation failed`)
    }
  }

  const handleDeleteConfirm = async () => {
    if (!tableToDelete) return

    try {
      await handleDeleteTable(tableToDelete.id)
      message.success('Table deleted successfully')
      setDeleteModalVisible(false)
      setTableToDelete(null)
    } catch (error) {
      message.error('Failed to delete table')
    }
  }

  const handleRefresh = () => {
    refetchTables()
    refetchStatistics()
  }

  return (
    <PageLayout
      title="Enhanced Business Metadata Management"
      subtitle="Comprehensive management of business metadata with advanced search, filtering, and bulk operations"
      extra={
        <Space>
          <Button icon={<ReloadOutlined />} onClick={handleRefresh} loading={tablesLoading}>
            Refresh
          </Button>
          <Button icon={<ExportOutlined />} onClick={() => setExportModalVisible(true)}>
            Export
          </Button>
          <Button icon={<SettingOutlined />}>
            Settings
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/admin/business-metadata/add')}>
            Add Table
          </Button>
        </Space>
      }
    >
      {/* Statistics Cards */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Total Tables"
              value={statistics?.totalTables || 0}
              prefix={<DatabaseOutlined />}
              loading={statisticsLoading}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Populated Tables"
              value={statistics?.populatedTables || 0}
              suffix={`/ ${statistics?.totalTables || 0}`}
              loading={statisticsLoading}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Metadata Completeness"
              value={statistics?.averageMetadataCompleteness || 0}
              precision={1}
              suffix="%"
              loading={statisticsLoading}
            />
            <Progress
              percent={statistics?.averageMetadataCompleteness || 0}
              size="small"
              showInfo={false}
              style={{ marginTop: 8 }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="AI Enhanced"
              value={statistics?.tablesWithAIMetadata || 0}
              loading={statisticsLoading}
            />
          </Card>
        </Col>
      </Row>

      {/* Search and Filter Controls */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16} align="middle">
          <Col flex="auto">
            <Search
              placeholder="Search tables, purposes, or domains..."
              allowClear
              enterButton={<SearchOutlined />}
              size="large"
              onSearch={handleSearch}
              defaultValue={tableParams.search}
            />
          </Col>
          <Col>
            <Select
              placeholder="Schema"
              style={{ width: 150 }}
              allowClear
              value={tableParams.schema}
              onChange={(value) => handleFilterChange({ schema: value })}
            >
              <Option value="dbo">dbo</Option>
              <Option value="sales">sales</Option>
              <Option value="finance">finance</Option>
              <Option value="hr">hr</Option>
            </Select>
          </Col>
          <Col>
            <Select
              placeholder="Domain"
              style={{ width: 150 }}
              allowClear
              value={tableParams.domain}
              onChange={(value) => handleFilterChange({ domain: value })}
            >
              <Option value="Sales">Sales</Option>
              <Option value="Finance">Finance</Option>
              <Option value="HR">HR</Option>
              <Option value="Operations">Operations</Option>
              <Option value="Marketing">Marketing</Option>
            </Select>
          </Col>
          <Col>
            <Button icon={<FilterOutlined />} onClick={() => setAdvancedSearchVisible(true)}>
              Advanced Search
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Bulk Operations Bar */}
      {selectedTableIds.length > 0 && (
        <Card style={{ marginBottom: 16, backgroundColor: '#f6ffed', borderColor: '#b7eb8f' }}>
          <Row justify="space-between" align="middle">
            <Col>
              <Space>
                <Text strong>{selectedTableIds.length} table(s) selected</Text>
                <Button size="small" onClick={clearSelection}>
                  Clear Selection
                </Button>
              </Space>
            </Col>
            <Col>
              <Dropdown menu={{ items: bulkMenuItems }} placement="bottomRight">
                <Button icon={<AppstoreOutlined />} loading={bulkOperating}>
                  Bulk Actions
                </Button>
              </Dropdown>
            </Col>
          </Row>
        </Card>
      )}

      {/* Main Content Tabs */}
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane
          tab={
            <Space>
              <DatabaseOutlined />
              Tables
            </Space>
          }
          key="tables"
        >
          <Card>
            <Table
              rowSelection={rowSelection}
              columns={columns}
              dataSource={tables}
              rowKey="id"
              loading={tablesLoading}
              pagination={{
                current: pagination?.currentPage,
                pageSize: pagination?.pageSize,
                total: pagination?.totalItems,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} tables`,
                onChange: handlePageChange,
                onShowSizeChange: handlePageChange,
              }}
              scroll={{ x: 1200 }}
              size="small"
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BarChartOutlined />
              Quality Overview
            </Space>
          }
          key="quality"
        >
          <Card title="Metadata Quality Overview">
            <Row gutter={16}>
              {tables.slice(0, 6).map((table) => (
                <Col key={table.id} span={8} style={{ marginBottom: 16 }}>
                  <QualityScoreVisualization
                    metrics={{
                      completeness: Math.floor(Math.random() * 40) + 60,
                      accuracy: Math.floor(Math.random() * 30) + 70,
                      consistency: Math.floor(Math.random() * 25) + 75,
                      timeliness: Math.floor(Math.random() * 35) + 65,
                      validity: Math.floor(Math.random() * 20) + 80,
                      overall: Math.floor(Math.random() * 30) + 70,
                    }}
                    tableName={table.tableName}
                    schemaName={table.schemaName}
                    showDetails={false}
                    size="small"
                  />
                </Col>
              ))}
            </Row>
          </Card>
        </TabPane>

        <TabPane
          tab={
            <Space>
              <LineChartOutlined />
              Table Analytics
            </Space>
          }
          key="analytics"
        >
          <MetadataAnalyticsDashboard />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <ThunderboltOutlined />
              Performance
            </Space>
          }
          key="performance"
        >
          <Card title="Virtualized Table (Performance Optimized)">
            <VirtualizedBusinessTable
              data={tables}
              loading={tablesLoading}
              selectedRowKeys={selectedRowKeys}
              onSelectionChange={setSelectedRowKeys}
              onView={(record) => navigate(`/admin/business-metadata/view/${record.id}`)}
              onEdit={(record) => navigate(`/admin/business-metadata/edit/${record.id}`)}
              onDelete={(record) => {
                setTableToDelete(record)
                setDeleteModalVisible(true)
              }}
              onValidate={(record) => {
                setSelectedTableForValidation(record)
                setValidationDrawerVisible(true)
              }}
              height={500}
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <Space>
              <MobileOutlined />
              Accessible
            </Space>
          }
          key="accessible"
        >
          <AccessibleBusinessMetadata
            data={tables}
            loading={tablesLoading}
            selectedRowKeys={selectedRowKeys}
            onSelectionChange={setSelectedRowKeys}
            onView={(record) => navigate(`/admin/business-metadata/view/${record.id}`)}
            onEdit={(record) => navigate(`/admin/business-metadata/edit/${record.id}`)}
            onDelete={(record) => {
              setTableToDelete(record)
              setDeleteModalVisible(true)
            }}
            onValidate={(record) => {
              setSelectedTableForValidation(record)
              setValidationDrawerVisible(true)
            }}
            pagination={pagination}
            onPageChange={handlePageChange}
          />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BookOutlined />
              Business Glossary
            </Space>
          }
          key="glossary"
        >
          <EnhancedBusinessGlossary />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <ApartmentOutlined />
              Business Domains
            </Space>
          }
          key="domains"
        >
          <EnhancedBusinessDomains />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <LinkOutlined />
              Integration
            </Space>
          }
          key="integration"
        >
          <GlossaryTableIntegration />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BarChartOutlined />
              Domain Analytics
            </Space>
          }
          key="domain-analytics"
        >
          <DomainGlossaryAnalytics />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BookOutlined />
              Business Glossary
            </Space>
          }
          key="glossary"
        >
          <EnhancedBusinessGlossary />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <ApartmentOutlined />
              Business Domains
            </Space>
          }
          key="domains"
        >
          <EnhancedBusinessDomains />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BugOutlined />
              API Testing
            </Space>
          }
          key="testing"
        >
          <EnhancedAPITester />
        </TabPane>
      </Tabs>

      {/* Delete Confirmation Modal */}
      <Modal
        title="Confirm Delete"
        open={deleteModalVisible}
        onOk={handleDeleteConfirm}
        onCancel={() => {
          setDeleteModalVisible(false)
          setTableToDelete(null)
        }}
        confirmLoading={deleting}
        okText="Delete"
        okType="danger"
      >
        <p>
          Are you sure you want to delete the table{' '}
          <strong>{tableToDelete?.schemaName}.{tableToDelete?.tableName}</strong>?
        </p>
        <p>This action cannot be undone.</p>
      </Modal>

      {/* Advanced Search Modal */}
      <AdvancedBusinessMetadataSearch
        visible={advancedSearchVisible}
        onClose={() => setAdvancedSearchVisible(false)}
        onSearch={handleAdvancedSearch}
        loading={searching}
      />

      {/* Validation Drawer */}
      <Drawer
        title={
          selectedTableForValidation ? (
            <Space>
              <CheckCircleOutlined />
              <span>
                Validation: {selectedTableForValidation.schemaName}.{selectedTableForValidation.tableName}
              </span>
            </Space>
          ) : (
            'Table Validation'
          )
        }
        width={800}
        open={validationDrawerVisible}
        onClose={() => {
          setValidationDrawerVisible(false)
          setSelectedTableForValidation(null)
        }}
        destroyOnClose
      >
        {selectedTableForValidation && (
          <ValidationDashboard
            tableId={selectedTableForValidation.id}
            schemaName={selectedTableForValidation.schemaName}
            tableName={selectedTableForValidation.tableName}
            onValidationComplete={(result) => {
              console.log('Validation completed:', result)
            }}
          />
        )}
      </Drawer>

      {/* Export Modal */}
      <MetadataExportManager
        visible={exportModalVisible}
        onClose={() => setExportModalVisible(false)}
        selectedTableIds={selectedTableIds}
      />
    </PageLayout>
  )
}

export default EnhancedBusinessMetadata
