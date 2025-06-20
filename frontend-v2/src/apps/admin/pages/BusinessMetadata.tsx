import { FC, useState } from 'react'
import { Card, Table, Button, Space, Tag, Typography, Tabs, Modal, message, Alert, Spin } from 'antd'
import { EditOutlined, DeleteOutlined, PlusOutlined, TableOutlined, BookOutlined, SettingOutlined, ReloadOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { useGetBusinessTablesQuery, useDeleteBusinessTableMutation } from '@shared/store/api/businessApi'
import { useEnhancedBusinessTables } from '@shared/hooks/useEnhancedApi'
import { useApiMode } from '@shared/components/core/ApiModeToggle'
import { BusinessTableEditor } from '../components/BusinessTableEditor'
import { BusinessGlossaryManager } from '../components/BusinessGlossaryManager'
import { RealApiTester } from '../components/RealApiTester'
import { MetadataValidationReport } from '../components/MetadataValidationReport'
import type { BusinessTableInfoDto } from '@shared/store/api/businessApi'

const { Text } = Typography
const { TabPane } = Tabs

export default function BusinessMetadata() {
  // Enhanced API with automatic fallback to mock data
  const { data: businessTables, isLoading, error, refetch } = useEnhancedBusinessTables()
  const [deleteTable] = useDeleteBusinessTableMutation()
  const { useMockData } = useApiMode()

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
      width: 250,
      render: (text: string, record: BusinessTableInfoDto) => (
        <div>
          <Text strong>{record.schemaName}.{text}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.businessPurpose || 'No business purpose defined'}
          </Text>
          <br />
          <Text type="secondary" style={{ fontSize: '11px' }}>
            Owner: {record.businessOwner || 'Not specified'}
          </Text>
        </div>
      ),
    },
    {
      title: 'Domain & Use Case',
      key: 'domain',
      width: 180,
      render: (record: BusinessTableInfoDto) => (
        <div>
          <Tag color="blue">{record.domainClassification || 'Unclassified'}</Tag>
          <br />
          <Text style={{ fontSize: '11px', marginTop: 4 }}>
            {record.primaryUseCase || 'No primary use case defined'}
          </Text>
        </div>
      ),
    },
    {
      title: 'Business Context',
      dataIndex: 'businessContext',
      key: 'context',
      width: 200,
      render: (context: string) => (
        <Text style={{ fontSize: '12px' }}>
          {context ? (context.length > 100 ? `${context.substring(0, 100)}...` : context) : 'No context provided'}
        </Text>
      ),
    },
    {
      title: 'Quality & Usage',
      key: 'metrics',
      width: 140,
      render: (record: BusinessTableInfoDto) => (
        <div>
          <div style={{ marginBottom: 4 }}>
            <Text style={{ fontSize: '11px' }}>Importance: </Text>
            <Tag color={record.importanceScore > 0.8 ? 'red' : record.importanceScore > 0.6 ? 'orange' : 'green'}>
              {record.importanceScore ? (record.importanceScore * 10).toFixed(1) : 'N/A'}
            </Tag>
          </div>
          <div style={{ marginBottom: 4 }}>
            <Text style={{ fontSize: '11px' }}>Usage: </Text>
            <Tag color="cyan">{record.usageFrequency ? (record.usageFrequency * 100).toFixed(0) + '%' : 'N/A'}</Tag>
          </div>
          {record.semanticCoverageScore && (
            <div>
              <Text style={{ fontSize: '11px' }}>Coverage: </Text>
              <Tag color={record.semanticCoverageScore > 0.8 ? 'green' : 'orange'}>
                {(record.semanticCoverageScore * 100).toFixed(0)}%
              </Tag>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Data Governance',
      key: 'governance',
      width: 120,
      render: (record: BusinessTableInfoDto) => (
        <div>
          <div style={{ marginBottom: 4 }}>
            <Tag color={record.isActive ? 'green' : 'red'}>
              {record.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </div>
          {record.dataGovernancePolicies && (
            <div>
              <Text style={{ fontSize: '10px', color: '#666' }}>
                {Array.isArray(record.dataGovernancePolicies)
                  ? record.dataGovernancePolicies.length + ' policies'
                  : 'Policies defined'}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'updatedDate',
      key: 'updated',
      width: 100,
      render: (date: string, record: BusinessTableInfoDto) => (
        <div>
          <Text style={{ fontSize: '11px' }}>
            {date ? new Date(date).toLocaleDateString() : 'Never'}
          </Text>
          {record.createdBy && (
            <div>
              <Text style={{ fontSize: '10px', color: '#666' }}>
                by {record.createdBy}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      fixed: 'right' as const,
      render: (record: BusinessTableInfoDto) => (
        <Space>
          <Button
            size="small"
            icon={<EditOutlined />}
            title="View/Edit Details"
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
    <>
      <style>
        {`
          .low-coverage-row {
            background-color: #fff2e8 !important;
          }
          .low-coverage-row:hover {
            background-color: #ffe7ba !important;
          }
        `}
      </style>
      <PageLayout
      title="Business Metadata Management"
      subtitle="Comprehensive management of table metadata, column definitions, and business glossary"
      extra={
        <Space>
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
      {useMockData && (
        <Alert
          message="Development Mode"
          description="Currently using mock data. Use the API toggle in the header to switch to real API when backend is available."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
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
              scroll={{ x: 1200 }}
              pagination={{
                pageSize: 15,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} business tables`,
                pageSizeOptions: ['10', '15', '25', '50']
              }}
              rowClassName={(record) =>
                record.semanticCoverageScore && record.semanticCoverageScore < 0.5
                  ? 'low-coverage-row'
                  : ''
              }
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

        <TabPane
          tab={
            <span>
              <ReloadOutlined />
              API Connection Test
            </span>
          }
          key="api-test"
        >
          <RealApiTester />
        </TabPane>
      </Tabs>

      <BusinessTableEditor
        open={isEditorOpen}
        table={selectedTable}
        onClose={handleEditorClose}
      />
    </PageLayout>
    </>
  )
}
