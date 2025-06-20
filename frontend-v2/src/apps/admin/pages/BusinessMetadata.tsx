import { FC, useState } from 'react'
import { Card, Table, Button, Space, Tag, Typography, Tabs, Modal, message, Alert, Spin } from 'antd'
import { EditOutlined, DeleteOutlined, PlusOutlined, TableOutlined, BookOutlined, SettingOutlined, ReloadOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import {
  useGetAllSchemaTablesQuery,
  useDeleteBusinessTableMutation,
  useGetBusinessTablesQuery
} from '@shared/store/api/businessApi'
import { useApiMode } from '@shared/components/core/ApiModeToggle'
import { BusinessTableEditor } from '../components/BusinessTableEditor'
import { BusinessGlossaryManager } from '../components/BusinessGlossaryManager'
import { RealApiTester } from '../components/RealApiTester'
import { MetadataValidationReport } from '../components/MetadataValidationReport'
import type { BusinessTableInfoDto } from '@shared/store/api/businessApi'

const { Text } = Typography
const { TabPane } = Tabs

export default function BusinessMetadata() {
  // Use schema tables to get actual database tables
  const { data: schemaTables, isLoading, error, refetch } = useGetAllSchemaTablesQuery()
  const [deleteTable] = useDeleteBusinessTableMutation()
  const { useMockData } = useApiMode()

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
      padding: 12px 16px !important;
    }
  `

  // Convert schema tables to business table format for display
  const displayTables = schemaTables?.map((table: any) => {
    // Handle both business metadata format and schema discovery format
    if (table.tableInformation) {
      // Business metadata format from backend
      const [schemaName, tableName] = table.tableInformation.split('.')
      return {
        id: table.tableInformation,
        schemaName: schemaName || 'dbo',
        tableName: tableName || table.tableInformation,
        businessPurpose: table.businessContext || 'No business purpose defined',
        domainClassification: table.domainUseCase || 'Unclassified',
        businessOwner: 'Not specified',
        primaryUseCase: table.domainUseCase || 'No primary use case defined',
        businessContext: table.businessContext || 'No context provided',
        importanceScore: table.qualityUsage?.includes('High') ? 0.9 : table.qualityUsage?.includes('Medium') ? 0.6 : 0.3,
        usageFrequency: table.qualityUsage?.includes('High') ? 0.8 : table.qualityUsage?.includes('Medium') ? 0.5 : 0.2,
        semanticCoverageScore: 0.7,
        isActive: true,
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
        importanceScore: 0.5,
        usageFrequency: 0.3,
        semanticCoverageScore: 0.2,
        isActive: true,
        dataGovernancePolicies: [],
        updatedDate: table.lastUpdated ? new Date(table.lastUpdated).toISOString() : '',
        createdBy: 'System',
      } as BusinessTableInfoDto
    }
  }) || []

  const [selectedTable, setSelectedTable] = useState<BusinessTableInfoDto | null>(null)
  const [isEditorOpen, setIsEditorOpen] = useState(false)
  const [activeTab, setActiveTab] = useState('tables')
  const [loadingTableDetails, setLoadingTableDetails] = useState(false)

  // Fetch business tables to check if table exists in business metadata
  const { data: businessTables } = useGetBusinessTablesQuery()

  // Debug business tables data
  console.log('🏢 Business tables from API:', businessTables)

  const handleEdit = async (record: BusinessTableInfoDto) => {
    console.log('🔍 Editing table:', record)
    console.log('🔍 Table properties:', {
      id: record.id,
      schemaName: record.schemaName,
      tableName: record.tableName,
      businessPurpose: record.businessPurpose,
      businessContext: record.businessContext,
      domainClassification: record.domainClassification
    })
    setLoadingTableDetails(true)

    try {
      // Check if this table exists in business metadata (has numeric ID)
      const existingBusinessTable = businessTables?.find(bt =>
        bt.schemaName === record.schemaName && bt.tableName === record.tableName
      )

      if (existingBusinessTable) {
        // Table exists in business metadata - fetch detailed business table data
        console.log('📊 Found existing business table, fetching detailed data...')

        // Use the business API to get the full business table details
        const response = await fetch(`/api/business/tables/${existingBusinessTable.id}`)
        if (response.ok) {
          const detailedBusinessTable = await response.json()
          console.log('✅ Fetched detailed business table:', detailedBusinessTable)
          setSelectedTable(detailedBusinessTable)
        } else {
          console.warn('⚠️ Failed to fetch business table details, using existing data')
          setSelectedTable(existingBusinessTable)
        }
      } else {
        // Table doesn't exist in business metadata - fetch schema details and create template
        console.log('🏗️ Table not in business metadata, fetching schema details...')

        try {
          // Fetch detailed schema information
          const tableFullName = `${record.schemaName}.${record.tableName}`
          const response = await fetch(`/api/schema/tables/${tableFullName}`)
          if (response.ok) {
            const schemaDetails = await response.json()
            console.log('✅ Fetched schema details:', schemaDetails)

            // Create a new business table template with enhanced schema info
            const newTableTemplate: BusinessTableInfoDto = {
              id: 0, // New table
              schemaName: schemaDetails.schemaName || record.schemaName,
              tableName: schemaDetails.tableName || record.tableName,
              businessPurpose: schemaDetails.businessPurpose || '',
              businessContext: schemaDetails.businessContext || '',
              primaryUseCase: schemaDetails.primaryUseCase || '',
              domainClassification: schemaDetails.domainClassification || '',
              businessOwner: '',
              semanticDescription: '',
              commonQueryPatterns: '',
              businessRules: '',
              naturalLanguageAliases: [],
              businessProcesses: [],
              analyticalUseCases: [],
              reportingCategories: [],
              vectorSearchKeywords: [],
              businessGlossaryTerms: [],
              llmContextHints: [],
              queryComplexityHints: [],
              semanticRelationships: '',
              usagePatterns: '',
              dataQualityIndicators: {},
              relationshipSemantics: '',
              dataGovernancePolicies: [],
              importanceScore: 0.5,
              usageFrequency: 0.5,
              semanticCoverageScore: 0.0,
              isActive: true,
              createdBy: '',
              createdDate: '',
              updatedDate: '',
              lastAnalyzed: ''
            }
            console.log('📝 Created enhanced table template:', newTableTemplate)
            setSelectedTable(newTableTemplate)
          } else {
            console.warn('⚠️ Failed to fetch schema details, using basic template')
            // Fallback to basic template
            setSelectedTable({
              id: 0,
              schemaName: record.schemaName,
              tableName: record.tableName,
              businessPurpose: '',
              businessContext: '',
              primaryUseCase: '',
              domainClassification: '',
              businessOwner: '',
              semanticDescription: '',
              commonQueryPatterns: '',
              businessRules: '',
              naturalLanguageAliases: [],
              businessProcesses: [],
              analyticalUseCases: [],
              reportingCategories: [],
              vectorSearchKeywords: [],
              businessGlossaryTerms: [],
              llmContextHints: [],
              queryComplexityHints: [],
              semanticRelationships: '',
              usagePatterns: '',
              dataQualityIndicators: {},
              relationshipSemantics: '',
              dataGovernancePolicies: [],
              importanceScore: 0.5,
              usageFrequency: 0.5,
              semanticCoverageScore: 0.0,
              isActive: true,
              createdBy: '',
              createdDate: '',
              updatedDate: '',
              lastAnalyzed: ''
            })
          }
        } catch (error) {
          console.error('❌ Error fetching schema details:', error)
          message.error('Failed to fetch table details')
          return
        }
      }

      setIsEditorOpen(true)
    } catch (error) {
      console.error('❌ Error in handleEdit:', error)
      message.error('Failed to load table information')
    } finally {
      setLoadingTableDetails(false)
    }
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
      width: 280,
      fixed: 'left' as const,
      render: (text: string, record: BusinessTableInfoDto) => (
        <div style={{ padding: '8px 0' }}>
          <div style={{ marginBottom: '6px' }}>
            <Text strong style={{ fontSize: '14px', color: '#1890ff' }}>
              {record.schemaName}.{text}
            </Text>
          </div>
          <div style={{ marginBottom: '4px' }}>
            <Text type="secondary" style={{ fontSize: '12px', lineHeight: '1.4' }}>
              {record.businessPurpose && record.businessPurpose !== 'No business purpose defined'
                ? (record.businessPurpose.length > 80 ? `${record.businessPurpose.substring(0, 80)}...` : record.businessPurpose)
                : 'No business purpose defined'}
            </Text>
          </div>
          <div>
            <Text type="secondary" style={{ fontSize: '11px', color: '#8c8c8c' }}>
              Owner: {record.businessOwner || 'Not specified'}
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Domain & Classification',
      key: 'domain',
      width: 200,
      render: (record: BusinessTableInfoDto) => (
        <div style={{ padding: '8px 0' }}>
          <div style={{ marginBottom: '8px' }}>
            <Tag
              color={record.domainClassification === 'Unclassified' ? 'default' : 'blue'}
              style={{ fontSize: '11px', padding: '2px 8px' }}
            >
              {record.domainClassification || 'Unclassified'}
            </Tag>
          </div>
          <div>
            <Text style={{ fontSize: '11px', color: '#666', lineHeight: '1.3' }}>
              {record.primaryUseCase && record.primaryUseCase !== 'No primary use case defined'
                ? (record.primaryUseCase.length > 60 ? `${record.primaryUseCase.substring(0, 60)}...` : record.primaryUseCase)
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
      width: 250,
      render: (context: string) => (
        <div style={{ padding: '8px 0' }}>
          <Text style={{ fontSize: '12px', lineHeight: '1.4', color: '#595959' }}>
            {context && context !== 'No context provided'
              ? (context.length > 120 ? `${context.substring(0, 120)}...` : context)
              : 'No context provided'}
          </Text>
        </div>
      ),
    },
    {
      title: 'Quality & Usage',
      key: 'metrics',
      width: 160,
      align: 'center' as const,
      render: (record: BusinessTableInfoDto) => (
        <div style={{ padding: '8px 0', textAlign: 'center' }}>
          <div style={{ marginBottom: '6px' }}>
            <Text style={{ fontSize: '10px', color: '#8c8c8c', display: 'block', marginBottom: '2px' }}>
              Importance
            </Text>
            <Tag
              color={record.importanceScore > 0.8 ? 'red' : record.importanceScore > 0.6 ? 'orange' : 'green'}
              style={{ fontSize: '11px', minWidth: '45px' }}
            >
              {record.importanceScore ? (record.importanceScore * 10).toFixed(1) : 'N/A'}
            </Tag>
          </div>
          <div style={{ marginBottom: '6px' }}>
            <Text style={{ fontSize: '10px', color: '#8c8c8c', display: 'block', marginBottom: '2px' }}>
              Usage
            </Text>
            <Tag
              color="cyan"
              style={{ fontSize: '11px', minWidth: '45px' }}
            >
              {record.usageFrequency ? (record.usageFrequency * 100).toFixed(0) + '%' : 'N/A'}
            </Tag>
          </div>
          {record.semanticCoverageScore > 0 && (
            <div>
              <Text style={{ fontSize: '10px', color: '#8c8c8c', display: 'block', marginBottom: '2px' }}>
                Coverage
              </Text>
              <Tag
                color={record.semanticCoverageScore > 0.8 ? 'green' : 'orange'}
                style={{ fontSize: '11px', minWidth: '45px' }}
              >
                {(record.semanticCoverageScore * 100).toFixed(0)}%
              </Tag>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Governance',
      key: 'governance',
      width: 140,
      align: 'center' as const,
      render: (record: BusinessTableInfoDto) => (
        <div style={{ padding: '8px 0', textAlign: 'center' }}>
          <div style={{ marginBottom: '8px' }}>
            <Tag
              color={record.isActive ? 'green' : 'red'}
              style={{ fontSize: '11px', fontWeight: '500' }}
            >
              {record.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </div>
          {record.dataGovernancePolicies && record.dataGovernancePolicies.length > 0 && (
            <div>
              <Text style={{ fontSize: '10px', color: '#666' }}>
                {Array.isArray(record.dataGovernancePolicies)
                  ? `${record.dataGovernancePolicies.length} policies`
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
      width: 120,
      align: 'center' as const,
      render: (date: string, record: BusinessTableInfoDto) => (
        <div style={{ padding: '8px 0', textAlign: 'center' }}>
          <div style={{ marginBottom: '4px' }}>
            <Text style={{ fontSize: '11px', color: '#595959' }}>
              {date ? new Date(date).toLocaleDateString() : 'Never'}
            </Text>
          </div>
          {record.createdBy && (
            <div>
              <Text style={{ fontSize: '10px', color: '#8c8c8c' }}>
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
            loading={loadingTableDetails}
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
              scroll={{ x: 1400, y: 'calc(100vh - 320px)' }}
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

      <BusinessTableEditor
        open={isEditorOpen}
        table={selectedTable}
        onClose={handleEditorClose}
      />
    </PageLayout>
    </>
  )
}
