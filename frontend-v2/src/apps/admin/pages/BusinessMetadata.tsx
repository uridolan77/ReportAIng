import { FC, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Typography,
  Tabs,
  Modal,
  message,
  Alert,
  Spin,
  Input,
  Select,
  Row,
  Col,
  Statistic,
  Tooltip,
  Dropdown,
  Progress,
  Drawer
} from 'antd'
import {
  EditOutlined,
  DeleteOutlined,
  PlusOutlined,
  TableOutlined,
  BookOutlined,
  SettingOutlined,
  ReloadOutlined,
  SearchOutlined,
  FilterOutlined,
  ExportOutlined,
  AppstoreOutlined,
  EyeOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BarChartOutlined,
  LineChartOutlined,
  ThunderboltOutlined,
  MobileOutlined,
  BugOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import {
  useGetAllSchemaTablesQuery,
  useDeleteBusinessTableMutation,
  useGetBusinessTablesQuery,
  useGetEnhancedBusinessTablesQuery,
  useGetEnhancedBusinessMetadataStatisticsQuery,
  useBulkOperateEnhancedBusinessTablesMutation,
  useValidateEnhancedBusinessTableMutation
} from '@shared/store/api/businessApi'
import { useApiMode } from '@shared/components/core/ApiModeToggle'
// BusinessTableEditor moved to dedicated page
import { BusinessGlossaryManager } from '../components/BusinessGlossaryManager'
import { RealApiTester } from '../components/RealApiTester'
import { MetadataValidationReport } from '../components/MetadataValidationReport'
import type { BusinessTableInfoDto } from '@shared/store/api/businessApi'
import { useAppSelector } from '@shared/hooks'
import { selectAccessToken } from '@shared/store/auth'

const { Text, Title } = Typography
const { TabPane } = Tabs
const { Search } = Input
const { Option } = Select

export default function BusinessMetadata() {
  const navigate = useNavigate()

  // Use enhanced business tables API for better functionality
  const { data: enhancedTablesResponse, isLoading: enhancedLoading, error: enhancedError, refetch: refetchEnhanced } = useGetEnhancedBusinessTablesQuery({
    page: 1,
    pageSize: 100,
    search: searchTerm,
    schema: selectedSchema,
    domain: selectedDomain,
  })

  // Get real statistics from API
  const { data: statisticsResponse, isLoading: statisticsLoading, refetch: refetchStatistics } = useGetEnhancedBusinessMetadataStatisticsQuery()

  // Fallback to original APIs if enhanced not available
  const { data: schemaTables, isLoading: schemaLoading, error: schemaError, refetch: refetchSchema } = useGetAllSchemaTablesQuery()
  const { data: businessTables } = useGetBusinessTablesQuery()
  const [deleteTable] = useDeleteBusinessTableMutation()
  const [bulkOperate] = useBulkOperateEnhancedBusinessTablesMutation()
  const [validateTable] = useValidateEnhancedBusinessTableMutation()
  const { useMockData } = useApiMode()

  // Determine which data source to use and loading state
  const isLoading = enhancedLoading || schemaLoading
  const error = enhancedError || schemaError
  const refetch = () => {
    refetchEnhanced()
    refetchSchema()
    refetchStatistics()
  }

  // State to store detailed business tables with metrics
  const [detailedBusinessTables, setDetailedBusinessTables] = useState<BusinessTableInfoDto[]>([])
  const [loadingDetailedTables, setLoadingDetailedTables] = useState(false)

  // Enhanced state for advanced features
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedSchema, setSelectedSchema] = useState<string>()
  const [selectedDomain, setSelectedDomain] = useState<string>()
  const [advancedSearchVisible, setAdvancedSearchVisible] = useState(false)
  const [validationDrawerVisible, setValidationDrawerVisible] = useState(false)
  const [exportModalVisible, setExportModalVisible] = useState(false)
  const [selectedTableForValidation, setSelectedTableForValidation] = useState<BusinessTableInfoDto | null>(null)
  const [bulkOperating, setBulkOperating] = useState(false)

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

  // Use enhanced tables data if available, otherwise fallback to business tables or schema tables
  const rawTables = enhancedTablesResponse?.data || businessTables || schemaTables?.map((table: any) => {
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

  // Filter and search functionality
  const filteredTables = rawTables?.filter((table: any) => {
    const matchesSearch = !searchTerm ||
      table.tableName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      table.businessPurpose?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      table.domainClassification?.toLowerCase().includes(searchTerm.toLowerCase())

    const matchesSchema = !selectedSchema || table.schemaName === selectedSchema
    const matchesDomain = !selectedDomain || table.domainClassification === selectedDomain

    return matchesSearch && matchesSchema && matchesDomain
  }) || []

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

  // Use real statistics from API or calculate fallback
  const statistics = statisticsResponse?.data || {
    totalTables: filteredTables.length,
    populatedTables: filteredTables.filter(t => t.businessPurpose && t.businessPurpose !== 'No business purpose defined').length,
    averageMetadataCompleteness: filteredTables.length > 0
      ? Math.round(filteredTables.reduce((sum, t) => {
          let score = 0
          if (t.businessPurpose && t.businessPurpose !== 'No business purpose defined') score += 25
          if (t.domainClassification && t.domainClassification !== 'Unclassified') score += 25
          if (t.businessOwner && t.businessOwner !== 'Not specified') score += 25
          if (t.primaryUseCase && t.primaryUseCase !== 'No primary use case defined') score += 25
          return sum + score
        }, 0) / filteredTables.length)
      : 0,
    tablesWithAIMetadata: filteredTables.filter(t => t.semanticCoverageScore && t.semanticCoverageScore > 0).length
  }

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

  // Enhanced functionality handlers
  const handleSearch = (value: string) => {
    setSearchTerm(value)
  }

  const handleFilterChange = (filters: { schema?: string; domain?: string }) => {
    if (filters.schema !== undefined) setSelectedSchema(filters.schema)
    if (filters.domain !== undefined) setSelectedDomain(filters.domain)
  }

  const handleTableSelection = (keys: React.Key[]) => {
    setSelectedRowKeys(keys)
  }

  const clearSelection = () => {
    setSelectedRowKeys([])
  }

  const handleBulkOperation = async (operation: string) => {
    if (selectedRowKeys.length === 0) {
      message.warning('Please select tables first')
      return
    }

    setBulkOperating(true)
    try {
      // Use real bulk operation API
      await bulkOperate({
        tableIds: selectedRowKeys.map(key => Number(key)),
        operation: operation as 'Activate' | 'Deactivate' | 'Delete'
      }).unwrap()
      message.success(`${operation} operation completed for ${selectedRowKeys.length} tables`)
      clearSelection()
      refetch() // Refresh data after bulk operation
    } catch (error) {
      console.error('Bulk operation error:', error)
      message.error(`Failed to perform ${operation} operation`)
    } finally {
      setBulkOperating(false)
    }
  }

  const handleRefresh = () => {
    refetch()
  }

  // Row selection configuration
  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[]) => {
      setSelectedRowKeys(newSelectedRowKeys)
    },
    onSelectAll: (selected: boolean, selectedRows: BusinessTableInfoDto[], changeRows: BusinessTableInfoDto[]) => {
      if (selected) {
        const allKeys = filteredTables.map(table => table.id)
        setSelectedRowKeys(allKeys)
      } else {
        setSelectedRowKeys([])
      }
    },
  }

  // Bulk operation menu items
  const bulkMenuItems = [
    {
      key: 'activate',
      label: 'Activate Selected',
      icon: <CheckCircleOutlined />,
      onClick: () => handleBulkOperation('Activate'),
    },
    {
      key: 'deactivate',
      label: 'Deactivate Selected',
      icon: <ExclamationCircleOutlined />,
      onClick: () => handleBulkOperation('Deactivate'),
    },
    {
      key: 'delete',
      label: 'Delete Selected',
      icon: <DeleteOutlined />,
      danger: true,
      onClick: () => handleBulkOperation('Delete'),
    },
  ]

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
      width: 140,
      fixed: 'right' as const,
      align: 'center' as const,
      render: (record: BusinessTableInfoDto) => (
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
              type="primary"
              ghost
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
              style={{ borderRadius: '4px' }}
            />
          </Tooltip>
          <Tooltip title="Validate">
            <Button
              size="small"
              icon={<CheckCircleOutlined />}
              onClick={async () => {
                setSelectedTableForValidation(record)
                setValidationDrawerVisible(true)
                // Trigger real validation API call
                try {
                  await validateTable({
                    tableId: Number(record.id),
                    validateBusinessRules: true,
                    validateDataQuality: true,
                    validateRelationships: true
                  }).unwrap()
                } catch (error) {
                  console.error('Validation error:', error)
                }
              }}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button
              size="small"
              danger
              ghost
              icon={<DeleteOutlined />}
              onClick={() => handleDelete(record)}
              style={{ borderRadius: '4px' }}
            />
          </Tooltip>
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
      subtitle="Comprehensive management of business metadata with advanced search, filtering, and bulk operations"
      extra={
        <Space>
          <Button icon={<ReloadOutlined />} onClick={handleRefresh} loading={isLoading}>
            Refresh
          </Button>
          <Button icon={<ExportOutlined />} onClick={() => setExportModalVisible(true)}>
            Export
          </Button>
          <Button icon={<SettingOutlined />}>
            Settings
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
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
              value={statistics.totalTables}
              prefix={<DatabaseOutlined />}
              loading={isLoading}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Populated Tables"
              value={statistics.populatedTables}
              suffix={`/ ${statistics.totalTables}`}
              loading={isLoading}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Metadata Completeness"
              value={statistics.averageMetadataCompleteness}
              precision={1}
              suffix="%"
              loading={isLoading}
            />
            <Progress
              percent={statistics.averageMetadataCompleteness}
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
              value={statistics.tablesWithAIMetadata}
              loading={isLoading}
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
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </Col>
          <Col>
            <Select
              placeholder="Schema"
              style={{ width: 150 }}
              allowClear
              value={selectedSchema}
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
              value={selectedDomain}
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
      {selectedRowKeys.length > 0 && (
        <Card style={{ marginBottom: 16, backgroundColor: '#f6ffed', borderColor: '#b7eb8f' }}>
          <Row justify="space-between" align="middle">
            <Col>
              <Space>
                <Text strong>{selectedRowKeys.length} table(s) selected</Text>
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

      {/* Debug Info Alert */}
      {filteredTables && filteredTables.length > 0 && (
        <Alert
          message={`Debug: Using ${businessTables && businessTables.length > 0 ? 'Business Tables' : 'Schema Tables'}`}
          description={`Found ${filteredTables.length} tables. First table sample: ${JSON.stringify({
            tableName: filteredTables[0]?.tableName,
            importanceScore: filteredTables[0]?.importanceScore,
            usageFrequency: filteredTables[0]?.usageFrequency,
            semanticCoverageScore: filteredTables[0]?.semanticCoverageScore,
            hasMetrics: filteredTables[0]?.importanceScore !== undefined,
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
              Business Tables ({filteredTables?.length || 0})
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
              rowSelection={rowSelection}
              columns={tableColumns}
              dataSource={filteredTables || []}
              loading={isLoading}
              rowKey="id"
              size="middle"
              bordered
              scroll={{ x: 1600, y: 'calc(100vh - 420px)' }}
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
              <BarChartOutlined />
              Quality Overview
            </span>
          }
          key="quality"
        >
          <Card title="Metadata Quality Overview">
            <Row gutter={16}>
              {filteredTables.slice(0, 6).map((table, index) => {
                // Calculate real quality metrics based on actual data
                const completeness = table.businessPurpose && table.businessPurpose !== 'No business purpose defined' ?
                  Math.min(100, (
                    (table.businessPurpose ? 25 : 0) +
                    (table.domainClassification && table.domainClassification !== 'Unclassified' ? 25 : 0) +
                    (table.businessOwner && table.businessOwner !== 'Not specified' ? 25 : 0) +
                    (table.primaryUseCase && table.primaryUseCase !== 'No primary use case defined' ? 25 : 0)
                  )) : 20

                const accuracy = table.importanceScore ? Math.round(table.importanceScore * 10) : 50
                const consistency = table.semanticCoverageScore ? Math.round(table.semanticCoverageScore * 100) : 30
                const timeliness = table.updatedDate ?
                  Math.max(30, 100 - Math.floor((Date.now() - new Date(table.updatedDate).getTime()) / (1000 * 60 * 60 * 24))) : 40

                return (
                  <Col key={table.id || index} span={8} style={{ marginBottom: 16 }}>
                    <Card size="small" title={`${table.schemaName}.${table.tableName}`}>
                      <Row gutter={8}>
                        <Col span={12}>
                          <Statistic
                            title="Completeness"
                            value={completeness}
                            suffix="%"
                            valueStyle={{
                              fontSize: '14px',
                              color: completeness >= 80 ? '#52c41a' : completeness >= 60 ? '#faad14' : '#ff4d4f'
                            }}
                          />
                        </Col>
                        <Col span={12}>
                          <Statistic
                            title="Accuracy"
                            value={accuracy}
                            suffix="%"
                            valueStyle={{
                              fontSize: '14px',
                              color: accuracy >= 80 ? '#52c41a' : accuracy >= 60 ? '#faad14' : '#ff4d4f'
                            }}
                          />
                        </Col>
                      </Row>
                      <Row gutter={8} style={{ marginTop: 8 }}>
                        <Col span={12}>
                          <Statistic
                            title="Consistency"
                            value={consistency}
                            suffix="%"
                            valueStyle={{
                              fontSize: '14px',
                              color: consistency >= 80 ? '#52c41a' : consistency >= 60 ? '#faad14' : '#ff4d4f'
                            }}
                          />
                        </Col>
                        <Col span={12}>
                          <Statistic
                            title="Timeliness"
                            value={timeliness}
                            suffix="%"
                            valueStyle={{
                              fontSize: '14px',
                              color: timeliness >= 80 ? '#52c41a' : timeliness >= 60 ? '#faad14' : '#ff4d4f'
                            }}
                          />
                        </Col>
                      </Row>
                    </Card>
                  </Col>
                )
              })}
            </Row>
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <LineChartOutlined />
              Table Analytics
            </span>
          }
          key="analytics"
        >
          <Card title="Table Analytics Dashboard">
            <Row gutter={16}>
              <Col span={12}>
                <Card size="small" title="Usage Trends">
                  <div style={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Text type="secondary">Analytics visualization would go here</Text>
                  </div>
                </Card>
              </Col>
              <Col span={12}>
                <Card size="small" title="Quality Metrics">
                  <div style={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Text type="secondary">Quality metrics chart would go here</Text>
                  </div>
                </Card>
              </Col>
            </Row>
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <ThunderboltOutlined />
              Performance
            </span>
          }
          key="performance"
        >
          <Card title="Virtualized Table (Performance Optimized)">
            <Alert
              message="Performance View"
              description="This tab would contain a virtualized table component for handling large datasets efficiently."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <Table
              columns={tableColumns.slice(0, 4)} // Show fewer columns for performance
              dataSource={filteredTables.slice(0, 100)} // Limit to first 100 for demo
              loading={isLoading}
              rowKey="id"
              size="small"
              scroll={{ x: 800, y: 400 }}
              pagination={{
                pageSize: 20,
                showSizeChanger: false,
                showQuickJumper: true,
              }}
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <MobileOutlined />
              Accessible
            </span>
          }
          key="accessible"
        >
          <Card title="Accessible Business Metadata View">
            <Alert
              message="Accessibility Features"
              description="This view provides enhanced accessibility features including keyboard navigation, screen reader support, and high contrast mode."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <div style={{ padding: '16px 0' }}>
              {filteredTables.slice(0, 10).map((table, index) => (
                <Card
                  key={table.id || index}
                  size="small"
                  style={{ marginBottom: 8 }}
                  title={`${table.schemaName}.${table.tableName}`}
                  extra={
                    <Space>
                      <Button size="small" icon={<EyeOutlined />} aria-label={`View ${table.tableName}`}>
                        View
                      </Button>
                      <Button size="small" icon={<EditOutlined />} aria-label={`Edit ${table.tableName}`}>
                        Edit
                      </Button>
                    </Space>
                  }
                >
                  <Text>{table.businessPurpose || 'No business purpose defined'}</Text>
                  <div style={{ marginTop: 8 }}>
                    <Tag color="blue">{table.domainClassification}</Tag>
                    <Tag color={table.isActive ? 'green' : 'red'}>
                      {table.isActive ? 'Active' : 'Inactive'}
                    </Tag>
                  </div>
                </Card>
              ))}
            </div>
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
              <BugOutlined />
              API Testing
            </span>
          }
          key="api-test"
        >
          <RealApiTester />
        </TabPane>
      </Tabs>

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
          <div>
            <Alert
              message="Validation Results"
              description={`Validation results for ${selectedTableForValidation.schemaName}.${selectedTableForValidation.tableName} would be displayed here.`}
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <Card title="Validation Summary" size="small">
              <Row gutter={16}>
                <Col span={8}>
                  <Statistic
                    title="Schema Compliance"
                    value={95}
                    suffix="%"
                    valueStyle={{ color: '#3f8600' }}
                  />
                </Col>
                <Col span={8}>
                  <Statistic
                    title="Data Quality"
                    value={87}
                    suffix="%"
                    valueStyle={{ color: '#cf1322' }}
                  />
                </Col>
                <Col span={8}>
                  <Statistic
                    title="Business Rules"
                    value={92}
                    suffix="%"
                    valueStyle={{ color: '#3f8600' }}
                  />
                </Col>
              </Row>
            </Card>
          </div>
        )}
      </Drawer>

      {/* Export Modal */}
      <Modal
        title="Export Business Metadata"
        open={exportModalVisible}
        onCancel={() => setExportModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setExportModalVisible(false)}>
            Cancel
          </Button>,
          <Button key="export" type="primary">
            Export
          </Button>,
        ]}
      >
        <div>
          <Alert
            message="Export Options"
            description="Select the format and scope for exporting business metadata."
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Export Format:</Text>
              <Select defaultValue="excel" style={{ width: '100%', marginTop: 8 }}>
                <Option value="excel">Excel (.xlsx)</Option>
                <Option value="csv">CSV (.csv)</Option>
                <Option value="json">JSON (.json)</Option>
              </Select>
            </div>
            <div>
              <Text strong>Export Scope:</Text>
              <Select defaultValue="all" style={{ width: '100%', marginTop: 8 }}>
                <Option value="all">All Tables</Option>
                <Option value="selected">Selected Tables ({selectedRowKeys.length})</Option>
                <Option value="filtered">Filtered Results ({filteredTables.length})</Option>
              </Select>
            </div>
          </Space>
        </div>
      </Modal>

      {/* BusinessTableEditor moved to dedicated page */}
    </PageLayout>
    </>
  )
}
