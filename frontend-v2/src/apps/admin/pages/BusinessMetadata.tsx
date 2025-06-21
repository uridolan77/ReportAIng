import { FC, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, Table, Button, Space, Tag, Typography, Tabs, Modal, message, Alert, Spin } from 'antd'
import { EditOutlined, DeleteOutlined, PlusOutlined, TableOutlined, BookOutlined, SettingOutlined, ReloadOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import {
  useGetAllSchemaTablesQuery,
  useDeleteBusinessTableMutation,
  useGetBusinessTablesQuery
} from '@shared/store/api/businessApi'
import { useApiMode } from '@shared/components/core/ApiModeToggle'
// BusinessTableEditor moved to dedicated page
import { BusinessGlossaryManager } from '../components/BusinessGlossaryManager'
import { RealApiTester } from '../components/RealApiTester'
import { MetadataValidationReport } from '../components/MetadataValidationReport'
import type { BusinessTableInfoDto } from '@shared/store/api/businessApi'
import { useAppSelector } from '@shared/hooks'
import { selectAccessToken } from '@shared/store/auth'

const { Text } = Typography
const { TabPane } = Tabs

export default function BusinessMetadata() {
  const navigate = useNavigate()

  // Use schema tables to get actual database tables
  const { data: schemaTables, isLoading, error, refetch } = useGetAllSchemaTablesQuery()
  const [deleteTable] = useDeleteBusinessTableMutation()
  const { useMockData } = useApiMode()

  // Fetch business tables to check if table exists in business metadata
  const { data: businessTables } = useGetBusinessTablesQuery()

  // State to store detailed business tables with metrics
  const [detailedBusinessTables, setDetailedBusinessTables] = useState<BusinessTableInfoDto[]>([])
  const [loadingDetailedTables, setLoadingDetailedTables] = useState(false)

  // Debug logging
  console.log('🔍 Schema tables data:', schemaTables)

  // Add custom styles for table rows
  const tableStyles = `
    .table-row-light {
      background-color: #fafafa !important;
    }
    .table-row-dark {
      background-color: #ffffff !important;
    }
    .table-row-light:hover,
    .table-row-dark:hover {
      background-color: #e6f7ff !important;
    }
    .low-coverage-row {
      background-color: #fff2e8 !important;
    }
    .low-coverage-row:hover {
      background-color: #ffe7ba !important;
    }
    .ant-table-thead > tr > th {
      background-color: #f0f2f5 !important;
      font-weight: 600 !important;
      color: #262626 !important;
      border-bottom: 2px solid #d9d9d9 !important;
    }
    .ant-table-tbody > tr > td {
      border-bottom: 1px solid #f0f0f0 !important;
      vertical-align: top !important;
      padding: 8px 12px !important;
    }
  `

  // Debug: Log both data sources
  console.log('🔍 Raw schemaTables data:', schemaTables)
  console.log('🏢 Raw businessTables data:', businessTables)

  // Debug: Show which data source is being used
  const usingBusinessTables = businessTables && businessTables.length > 0
  console.log(`📊 Using ${usingBusinessTables ? 'business tables' : 'schema tables'} for display`)

  if (usingBusinessTables && businessTables) {
    console.table(businessTables.map(table => ({
      tableName: table.tableName,
      importanceScore: table.importanceScore,
      usageFrequency: table.usageFrequency,
      semanticCoverageScore: table.semanticCoverageScore,
      hasMetrics: table.importanceScore !== undefined
    })))

    // Log the complete structure of the first table
    console.log('🔍 Complete first business table structure:', businessTables[0])
    console.log('🔍 All field names in first table:', Object.keys(businessTables[0]))
  }

  // Use business tables if available, otherwise fall back to schema tables
  const displayTables = businessTables && businessTables.length > 0
    ? businessTables.map((table: BusinessTableInfoDto) => ({
        ...table,
        // Ensure proper data types for display
        importanceScore: typeof table.importanceScore === 'number' ? table.importanceScore : 0.5,
        usageFrequency: typeof table.usageFrequency === 'number' ? table.usageFrequency : 0.3,
        semanticCoverageScore: typeof table.semanticCoverageScore === 'number' ? table.semanticCoverageScore : 0.2,
      }))
    : schemaTables?.map((table: any) => {
    // Handle both new API format and old business metadata format
    if (table.schemaName && table.tableName) {
      // New API format from /api/schema/tables
      return {
        id: `${table.schemaName}.${table.tableName}`,
        schemaName: table.schemaName,
        tableName: table.tableName,
        businessPurpose: table.businessPurpose || 'No business purpose defined',
        domainClassification: table.domainClassification || 'Unclassified',
        businessOwner: 'Not specified',
        primaryUseCase: table.domainClassification || 'No primary use case defined',
        businessContext: table.businessPurpose || 'No context provided',
        // Schema tables don't have business metrics, so we set reasonable defaults
        // These will be overridden when the table is edited and saved as business metadata
        importanceScore: 0.5, // Default medium importance
        usageFrequency: 0.3,   // Default low usage
        semanticCoverageScore: 0.2, // Default low coverage
        isActive: table.isActive !== undefined ? table.isActive : true,
        dataGovernancePolicies: table.ruleGovernance ? [table.ruleGovernance] : [],
        updatedDate: table.lastUpdated || '',
        createdBy: 'System',
      } as BusinessTableInfoDto
    } else {
      // Schema discovery format from backend
      return {
        id: `${table.schema}.${table.name}`,
        schemaName: table.schema || 'dbo',
        tableName: table.name,
        businessPurpose: table.description || 'No business purpose defined',
        domainClassification: 'Unclassified',
        businessOwner: 'Not specified',
        primaryUseCase: 'No primary use case defined',
        businessContext: table.description || 'No context provided',
        // Schema discovery tables don't have business metrics, use defaults
        importanceScore: 0.5,
        usageFrequency: 0.3,
        semanticCoverageScore: 0.2,
        isActive: table.isActive !== undefined ? table.isActive : true,
        dataGovernancePolicies: [],
        updatedDate: table.lastUpdated ? new Date(table.lastUpdated).toISOString() : '',
        createdBy: 'System',
      } as BusinessTableInfoDto
    }
  }) || []

  const [activeTab, setActiveTab] = useState('tables')
  // Removed modal state - using dedicated page now

  // Get authentication token for manual API calls
  const accessToken = useAppSelector(selectAccessToken)

  // Debug business tables data
  console.log('🏢 Business tables from API:', businessTables)
  console.log('🔑 Access token available:', !!accessToken)

  // Debug specific table data
  if (businessTables && businessTables.length > 0) {
    const countriesTable = businessTables.find(t => t.tableName === 'tbl_Countries')
    if (countriesTable) {
      console.log('🌍 Countries table data:', {
        tableName: countriesTable.tableName,
        importanceScore: countriesTable.importanceScore,
        usageFrequency: countriesTable.usageFrequency,
        semanticCoverageScore: countriesTable.semanticCoverageScore,
        allFields: Object.keys(countriesTable),
        rawData: countriesTable
      })

      // Check for different possible field names
      console.log('🔍 Checking alternative field names:', {
        ImportanceScore: (countriesTable as any).ImportanceScore,
        UsageFrequency: (countriesTable as any).UsageFrequency,
        SemanticCoverageScore: (countriesTable as any).SemanticCoverageScore,
        importance_score: (countriesTable as any).importance_score,
        usage_frequency: (countriesTable as any).usage_frequency,
        semantic_coverage_score: (countriesTable as any).semantic_coverage_score
      })
    }
  }

  const handleEdit = (record: BusinessTableInfoDto) => {
    // Navigate to dedicated edit page instead of opening modal
    const tableId = typeof record.id === 'number' ? record.id : `${record.schemaName}.${record.tableName}`
    navigate(`/admin/business-metadata/edit/${tableId}`)
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
    // Navigate to dedicated add page
    navigate('/admin/business-metadata/add')
  }

  const tableColumns = [
    {
      title: 'Business Friendly Name',
      dataIndex: 'businessName',
      key: 'businessName',
      width: 200,
      fixed: 'left' as const,
      render: (text: string, record: BusinessTableInfoDto) => (
        <div style={{ padding: '4px 0' }}>
          <div style={{ marginBottom: '3px' }}>
            <Text strong style={{ fontSize: '13px', color: '#1890ff' }}>
              {text || record.tableName}
            </Text>
          </div>
          <div>
            <Text type="secondary" style={{ fontSize: '10px', color: '#8c8c8c' }}>
              {record.schemaName}.{record.tableName}
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Table Information',
      dataIndex: 'tableName',
      key: 'tableName',
      width: 280,
      render: (text: string, record: BusinessTableInfoDto) => (
        <div style={{ padding: '4px 0' }}>
          <div style={{ marginBottom: '2px' }}>
            <Text type="secondary" style={{ fontSize: '11px', lineHeight: '1.3' }}>
              {record.businessPurpose && record.businessPurpose !== 'No business purpose defined'
                ? (record.businessPurpose.length > 70 ? `${record.businessPurpose.substring(0, 70)}...` : record.businessPurpose)
                : 'No business purpose defined'}
            </Text>
          </div>
          <div>
            <Text type="secondary" style={{ fontSize: '10px', color: '#8c8c8c' }}>
              Owner: {record.businessOwner || 'Not specified'}
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Domain & Classification',
      key: 'domain',
      width: 180,
      render: (record: BusinessTableInfoDto) => (
        <div style={{ padding: '4px 0' }}>
          <div style={{ marginBottom: '4px' }}>
            <Tag
              color={record.domainClassification === 'Unclassified' ? 'default' : 'blue'}
              style={{ fontSize: '10px', padding: '1px 6px' }}
            >
              {record.domainClassification || 'Unclassified'}
            </Tag>
          </div>
          <div>
            <Text style={{ fontSize: '10px', color: '#666', lineHeight: '1.2' }}>
              {record.primaryUseCase && record.primaryUseCase !== 'No primary use case defined'
                ? (record.primaryUseCase.length > 50 ? `${record.primaryUseCase.substring(0, 50)}...` : record.primaryUseCase)
                : 'No primary use case defined'}
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Business Context',
      dataIndex: 'businessContext',
      key: 'context',
      width: 220,
      render: (context: string) => (
        <div style={{ padding: '4px 0' }}>
          <Text style={{ fontSize: '11px', lineHeight: '1.3', color: '#595959' }}>
            {context && context !== 'No context provided'
              ? (context.length > 100 ? `${context.substring(0, 100)}...` : context)
              : 'No context provided'}
          </Text>
        </div>
      ),
    },
    {
      title: 'Quality & Usage',
      key: 'metrics',
      width: 280,
      align: 'center' as const,
      render: (record: BusinessTableInfoDto) => (
        <div style={{ padding: '4px 0', textAlign: 'center', lineHeight: '1.2' }}>
          {/* Horizontal layout for all metrics */}
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', flexWrap: 'wrap' }}>
            {/* Importance Score */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '2px' }}>
              <Text style={{ fontSize: '9px', color: '#8c8c8c' }}>
                Imp:
              </Text>
              <Tag
                color={
                  record.importanceScore > (record.importanceScore <= 1 ? 0.8 : 8) ? 'red' :
                  record.importanceScore > (record.importanceScore <= 1 ? 0.6 : 6) ? 'orange' : 'green'
                }
                style={{ fontSize: '10px', minWidth: '35px', margin: 0, padding: '1px 4px' }}
              >
                {record.importanceScore !== undefined && record.importanceScore !== null
                  ? (record.importanceScore <= 1
                     ? (record.importanceScore * 10).toFixed(1)  // 0-1 scale, multiply by 10
                     : record.importanceScore.toFixed(1))        // 1-10 scale, use as is
                  : 'N/A'}
              </Tag>
            </div>

            {/* Usage Frequency */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '2px' }}>
              <Text style={{ fontSize: '9px', color: '#8c8c8c' }}>
                Use:
              </Text>
              <Tag
                color="cyan"
                style={{ fontSize: '10px', minWidth: '35px', margin: 0, padding: '1px 4px' }}
              >
                {record.usageFrequency !== undefined && record.usageFrequency !== null
                  ? (record.usageFrequency <= 1
                     ? (record.usageFrequency * 100).toFixed(0) + '%'  // 0-1 scale, convert to percentage
                     : record.usageFrequency.toFixed(0) + '%')
                  : 'N/A'}
              </Tag>
            </div>

            {/* Coverage Score - only show if > 0 */}
            {record.semanticCoverageScore > 0 && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '2px' }}>
                <Text style={{ fontSize: '9px', color: '#8c8c8c' }}>
                  Cov:
                </Text>
                <Tag
                  color={
                    record.semanticCoverageScore > (record.semanticCoverageScore <= 1 ? 0.8 : 80) ? 'green' : 'orange'
                  }
                  style={{ fontSize: '10px', minWidth: '35px', margin: 0, padding: '1px 4px' }}
                >
                  {record.semanticCoverageScore <= 1
                    ? (record.semanticCoverageScore * 100).toFixed(0) + '%'  // 0-1 scale, convert to percentage
                    : record.semanticCoverageScore.toFixed(0) + '%'}
                </Tag>
              </div>
            )}
          </div>
        </div>
      ),
    },
    {
      title: 'Governance',
      key: 'governance',
      width: 100,
      align: 'center' as const,
      render: (record: BusinessTableInfoDto) => (
        <div style={{ padding: '4px 0', textAlign: 'center' }}>
          <div style={{ marginBottom: '4px' }}>
            <Tag
              color={record.isActive ? 'green' : 'red'}
              style={{ fontSize: '10px', fontWeight: '500', padding: '1px 4px' }}
            >
              {record.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </div>
          {record.dataGovernancePolicies && record.dataGovernancePolicies.length > 0 && (
            <div>
              <Text style={{ fontSize: '9px', color: '#666' }}>
                {Array.isArray(record.dataGovernancePolicies)
                  ? `${record.dataGovernancePolicies.length} policies`
                  : 'Policies'}
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
      width: 110,
      align: 'center' as const,
      render: (date: string, record: BusinessTableInfoDto) => (
        <div style={{ padding: '4px 0', textAlign: 'center' }}>
          <div style={{ marginBottom: '2px' }}>
            <Text style={{ fontSize: '10px', color: '#595959' }}>
              {date ? new Date(date).toLocaleDateString() : 'Never'}
            </Text>
          </div>
          {record.createdBy && (
            <div>
              <Text style={{ fontSize: '9px', color: '#8c8c8c' }}>
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
      align: 'center' as const,
      render: (record: BusinessTableInfoDto) => (
        <Space size="small">
          <Button
            size="small"
            type="primary"
            ghost
            icon={<EditOutlined />}
            title="View/Edit Details"
            onClick={() => handleEdit(record)}
            style={{ borderRadius: '4px' }}
          />
          <Button
            size="small"
            danger
            ghost
            icon={<DeleteOutlined />}
            title="Delete"
            onClick={() => handleDelete(record)}
            style={{ borderRadius: '4px' }}
          />
        </Space>
      ),
    },
  ]

  return (
    <>
      <style>
        {tableStyles}
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
      {/* Debug Info Alert */}
      {displayTables && displayTables.length > 0 && (
        <Alert
          message={`Debug: Using ${businessTables && businessTables.length > 0 ? 'Business Tables' : 'Schema Tables'}`}
          description={`Found ${displayTables.length} tables. First table sample: ${JSON.stringify({
            tableName: displayTables[0]?.tableName,
            importanceScore: displayTables[0]?.importanceScore,
            usageFrequency: displayTables[0]?.usageFrequency,
            semanticCoverageScore: displayTables[0]?.semanticCoverageScore,
            hasMetrics: displayTables[0]?.importanceScore !== undefined,
            dataSource: businessTables && businessTables.length > 0 ? 'business' : 'schema'
          }, null, 2)}`}
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

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
              Business Tables ({displayTables?.length || 0})
            </span>
          }
          key="tables"
        >
          <Card
            style={{
              borderRadius: '8px',
              boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
              overflow: 'hidden'
            }}
          >
            <Table
              columns={tableColumns}
              dataSource={displayTables || []}
              loading={isLoading}
              rowKey="id"
              size="middle"
              bordered
              scroll={{ x: 1600, y: 'calc(100vh - 320px)' }}
              pagination={{
                pageSize: 12,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} business tables`,
                pageSizeOptions: ['10', '12', '20', '50'],
                size: 'default',
                style: { marginTop: '16px' }
              }}
              rowClassName={(record, index) => {
                let className = index % 2 === 0 ? 'table-row-light' : 'table-row-dark'
                if (record.semanticCoverageScore && record.semanticCoverageScore < 0.5) {
                  className += ' low-coverage-row'
                }
                return className
              }}
              style={{
                backgroundColor: '#fff',
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

      {/* BusinessTableEditor moved to dedicated page */}
    </PageLayout>
    </>
  )
}
