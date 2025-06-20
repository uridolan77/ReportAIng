import { FC, useState } from 'react'
import { Card, Table, Button, Space, Tag, Typography, Tabs, Modal, message, Alert, Spin } from 'antd'
import { EditOutlined, DeleteOutlined, PlusOutlined, TableOutlined, BookOutlined, SettingOutlined, ReloadOutlined, ApiOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { useGetBusinessTablesQuery, useDeleteBusinessTableMutation } from '@shared/store/api/businessApi'
import { useEnhancedBusinessTables } from '@shared/hooks/useEnhancedApi'
import { useApiToggle } from '@shared/services/apiToggleService'
import { BusinessTableEditor } from '../components/BusinessTableEditor'
import { BusinessGlossaryManager } from '../components/BusinessGlossaryManager'
import { MetadataValidationReport } from '../components/MetadataValidationReport'
import type { BusinessTableInfoDto } from '@shared/store/api/businessApi'

const { Text } = Typography
const { TabPane } = Tabs

export default function BusinessMetadata() {
  // Enhanced API with automatic fallback to mock data
  const { data: businessTables, isLoading, error, refetch } = useEnhancedBusinessTables()
  const [deleteTable] = useDeleteBusinessTableMutation()
  const { isUsingMockData, toggleMockData, status } = useApiToggle()

  const [selectedTable, setSelectedTable] = useState<BusinessTableInfoDto | null>(null)
  const [isEditorOpen, setIsEditorOpen] = useState(false)
  const [activeTab, setActiveTab] = useState('tables')

  const handleEdit = (record: BusinessTableInfoDto) => {
    setSelectedTable(record)
    setIsEditorOpen(true)
  }

  const handleDelete = (record: BusinessTableInfoDto) => {
    Modal.confirm({
      title: 'Delete Business Table',
      content: `Are you sure you want to delete metadata for "${record.schemaName}.${record.tableName}"?`,
      okText: 'Delete',
      okType: 'danger',
      onOk: async () => {
        try {
          await deleteTable(record.id).unwrap()
          message.success('Business table deleted successfully')
        } catch (error) {
          message.error('Failed to delete business table')
        }
      },
    })
  }

  const handleAdd = () => {
    setSelectedTable(null)
    setIsEditorOpen(true)
  }

  const handleEditorClose = () => {
    setIsEditorOpen(false)
    setSelectedTable(null)
  }

  const tableColumns = [
    {
      title: 'Table Information',
      dataIndex: 'tableName',
      key: 'tableName',
      render: (text: string, record: BusinessTableInfoDto) => (
        <div>
          <Text strong>{record.schemaName}.{text}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.businessPurpose}
          </Text>
          <br />
          <Text type="secondary" style={{ fontSize: '11px' }}>
            Owner: {record.businessOwner || 'Not specified'}
          </Text>
        </div>
      ),
    },
    {
      title: 'Classification',
      dataIndex: 'domainClassification',
      key: 'domain',
      width: 140,
      render: (domain: string, record: BusinessTableInfoDto) => (
        <div>
          <Tag color="blue">{domain}</Tag>
          <br />
          <Text style={{ fontSize: '11px' }}>
            Use Case: {record.primaryUseCase}
          </Text>
        </div>
      ),
    },
    {
      title: 'Metrics',
      key: 'metrics',
      width: 120,
      render: (record: BusinessTableInfoDto) => (
        <div>
          <div>
            <Text style={{ fontSize: '11px' }}>Importance: </Text>
            <Tag color={record.importanceScore > 8 ? 'red' : record.importanceScore > 6 ? 'orange' : 'green'}>
              {record.importanceScore}/10
            </Tag>
          </div>
          <div style={{ marginTop: 4 }}>
            <Text style={{ fontSize: '11px' }}>Usage: {record.usageFrequency}</Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Columns',
      key: 'columns',
      width: 80,
      render: (record: BusinessTableInfoDto) => (
        <Tag color="purple">
          {record.columns?.length || 0} cols
        </Tag>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'status',
      width: 80,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'updatedDate',
      key: 'updated',
      width: 120,
      render: (date: string) => (
        <Text style={{ fontSize: '11px' }}>
          {date ? new Date(date).toLocaleDateString() : 'Never'}
        </Text>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (record: BusinessTableInfoDto) => (
        <Space>
          <Button
            size="small"
            icon={<EditOutlined />}
            title="Edit"
            onClick={() => handleEdit(record)}
          />
          <Button
            size="small"
            icon={<DeleteOutlined />}
            danger
            title="Delete"
            onClick={() => handleDelete(record)}
          />
        </Space>
      ),
    },
  ]

  return (
    <PageLayout
      title="Business Metadata Management"
      subtitle="Comprehensive management of table metadata, column definitions, and business glossary"
      extra={
        <Space>
          <Button
            icon={<ApiOutlined />}
            onClick={toggleMockData}
            type={isUsingMockData ? 'default' : 'primary'}
            size="small"
          >
            {isUsingMockData ? 'Using Mock Data' : 'Using Real API'}
          </Button>
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} loading={isLoading}>
            Refresh
          </Button>
          <Button icon={<SettingOutlined />}>
            Validation Settings
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Add Business Table
          </Button>
        </Space>
      }
    >
      {/* API Status Alert */}
      {isUsingMockData && (
        <Alert
          message="Development Mode"
          description="Currently using mock data. Toggle to real API when backend is available."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          action={
            <Button size="small" onClick={toggleMockData}>
              Switch to Real API
            </Button>
          }
        />
      )}

      {error && (
        <Alert
          message="API Error"
          description={`Failed to load data: ${error instanceof Error ? error.message : 'Unknown error'}`}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          action={
            <Button size="small" onClick={() => refetch()}>
              Retry
            </Button>
          }
        />
      )}

      {isLoading && (
        <div style={{ textAlign: 'center', padding: '20px' }}>
          <Spin size="large" />
          <div style={{ marginTop: 8 }}>Loading business metadata...</div>
        </div>
      )}
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane
          tab={
            <span>
              <TableOutlined />
              Business Tables ({businessTables?.length || 0})
            </span>
          }
          key="tables"
        >
          <Card>
            <Table
              columns={tableColumns}
              dataSource={businessTables || []}
              loading={isLoading}
              rowKey="id"
              size="small"
              expandable={{
                expandedRowRender: (record) => (
                  <div style={{ padding: '12px 0' }}>
                    <div style={{ marginBottom: 8 }}>
                      <Text strong>Business Context: </Text>
                      <Text>{record.businessContext || 'Not specified'}</Text>
                    </div>
                    <div style={{ marginBottom: 8 }}>
                      <Text strong>Common Query Patterns: </Text>
                      <Text>{record.commonQueryPatterns || 'Not specified'}</Text>
                    </div>
                    <div style={{ marginBottom: 8 }}>
                      <Text strong>Business Rules: </Text>
                      <Text>{record.businessRules || 'Not specified'}</Text>
                    </div>
                    <div>
                      <Text strong>Natural Language Aliases: </Text>
                      <Text>{record.naturalLanguageAliases || 'Not specified'}</Text>
                    </div>
                  </div>
                ),
                rowExpandable: (record) => true,
              }}
              pagination={{
                pageSize: 20,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} tables`,
              }}
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <BookOutlined />
              Business Glossary
            </span>
          }
          key="glossary"
        >
          <BusinessGlossaryManager />
        </TabPane>

        <TabPane
          tab={
            <span>
              <SettingOutlined />
              Validation Report
            </span>
          }
          key="validation"
        >
          <MetadataValidationReport />
        </TabPane>
      </Tabs>

      <BusinessTableEditor
        open={isEditorOpen}
        table={selectedTable}
        onClose={handleEditorClose}
      />
    </PageLayout>
  )
}
