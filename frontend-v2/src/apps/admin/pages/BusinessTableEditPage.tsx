import React, { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Button, message, Spin, Alert } from 'antd'
import { ArrowLeftOutlined, SaveOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { BusinessTableEditor } from '../components/BusinessTableEditor'
import {
  useGetBusinessTableQuery,
  useCreateBusinessTableMutation,
  useUpdateBusinessTableMutation,
  useGetSchemaTableDetailsQuery,
  type BusinessTableInfoDto
} from '@shared/store/api/businessApi'
import { useAppSelector } from '@shared/hooks'
import { selectAccessToken } from '@shared/store/auth'

export const BusinessTableEditPage: React.FC = () => {
  const { tableId } = useParams<{ tableId: string }>()
  const navigate = useNavigate()
  const accessToken = useAppSelector(selectAccessToken)
  
  const [selectedTable, setSelectedTable] = useState<BusinessTableInfoDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [isNewTable, setIsNewTable] = useState(false)

  // Mutations for saving
  const [createBusinessTable] = useCreateBusinessTableMutation()
  const [updateBusinessTable] = useUpdateBusinessTableMutation()

  // Determine if this is a numeric ID (existing business table) or schema.table format (new table)
  const isNumericId = tableId && !isNaN(Number(tableId))
  
  // Fetch business table if it's an existing one
  const {
    data: businessTable,
    isLoading: loadingBusinessTable,
    error: businessTableError
  } = useGetBusinessTableQuery(Number(tableId), {
    skip: !isNumericId || tableId === 'add'
  })

  // For new tables or schema tables, we might need schema information
  const schemaTableName = !isNumericId && tableId !== 'add' ? tableId : undefined
  const [schemaName, tableName] = schemaTableName ? schemaTableName.split('.') : ['', '']
  const {
    data: schemaTable,
    isLoading: loadingSchemaTable
  } = useGetSchemaTableDetailsQuery(
    { schemaName: schemaName || '', tableName: tableName || '' },
    { skip: !schemaTableName || !schemaName || !tableName }
  )

  useEffect(() => {
    const loadTableData = async () => {
      setLoading(true)
      
      try {
        if (tableId === 'add') {
          // Creating a completely new table
          setIsNewTable(true)
          setSelectedTable({
            id: 0,
            schemaName: '',
            tableName: '',
            businessPurpose: '',
            businessContext: '',
            primaryUseCase: '',
            domainClassification: 'Reference',
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
            semanticCoverageScore: 0.2,
            isActive: true,
            createdBy: 'System',
            createdDate: new Date().toISOString(),
            updatedDate: new Date().toISOString(),
            lastAnalyzed: new Date().toISOString()
          })
        } else if (isNumericId && businessTable) {
          // Editing existing business table
          setIsNewTable(false)
          setSelectedTable(businessTable)
        } else if (schemaTable) {
          // Creating business metadata for existing schema table
          setIsNewTable(true)
          const [schemaName, tableName] = (tableId || '').split('.')
          setSelectedTable({
            id: 0,
            schemaName: schemaName || schemaTable.schemaName || '',
            tableName: tableName || schemaTable.tableName || '',
            businessPurpose: schemaTable.businessPurpose || `Table containing data for ${tableName}`,
            businessContext: schemaTable.businessPurpose || `Database table ${schemaName}.${tableName}`,
            primaryUseCase: 'Data storage and retrieval',
            domainClassification: schemaTable.domainClassification || 'Reference',
            businessOwner: 'Not specified',
            semanticDescription: `Database table with ${schemaTable.columns?.length || 0} columns`,
            commonQueryPatterns: 'Standard CRUD operations, filtering, and joins',
            businessRules: 'Standard database constraints and business logic apply',
            naturalLanguageAliases: [tableName || ''],
            businessProcesses: ['Data Management'],
            analyticalUseCases: ['Reporting', 'Analytics'],
            reportingCategories: ['Operational Data'],
            vectorSearchKeywords: [tableName || '', schemaName || ''],
            businessGlossaryTerms: [],
            llmContextHints: [`This table contains ${schemaTable.columns?.length || 0} columns`],
            queryComplexityHints: ['Standard table operations'],
            semanticRelationships: '',
            usagePatterns: 'Regular database operations',
            dataQualityIndicators: {},
            relationshipSemantics: '',
            dataGovernancePolicies: [],
            importanceScore: 0.5,
            usageFrequency: 0.5,
            semanticCoverageScore: 0.2,
            isActive: true,
            createdBy: 'System',
            createdDate: new Date().toISOString(),
            updatedDate: new Date().toISOString(),
            lastAnalyzed: new Date().toISOString()
          })
        } else {
          message.error('Table not found')
          navigate('/admin/business-metadata')
        }
      } catch (error) {
        console.error('Error loading table data:', error)
        message.error('Failed to load table data')
        navigate('/admin/business-metadata')
      } finally {
        setLoading(false)
      }
    }

    if (!loadingBusinessTable && !loadingSchemaTable) {
      loadTableData()
    }
  }, [tableId, businessTable, schemaTable, loadingBusinessTable, loadingSchemaTable, navigate, isNumericId])

  const handleSave = async (tableData: BusinessTableInfoDto) => {
    try {
      if (isNewTable) {
        await createBusinessTable(tableData).unwrap()
        message.success('Business table created successfully')
      } else {
        await updateBusinessTable(tableData).unwrap()
        message.success('Business table updated successfully')
      }
      navigate('/admin/business-metadata')
    } catch (error) {
      console.error('Error saving table:', error)
      message.error('Failed to save table')
    }
  }

  const handleCancel = () => {
    navigate('/admin/business-metadata')
  }

  if (loading || loadingBusinessTable || loadingSchemaTable) {
    return (
      <PageLayout title="Loading Table Details">
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Spin size="large" />
          <p style={{ marginTop: '16px' }}>Loading table information...</p>
        </div>
      </PageLayout>
    )
  }

  if (businessTableError) {
    return (
      <PageLayout title="Error">
        <Alert
          message="Error Loading Table"
          description="Failed to load table information. Please try again."
          type="error"
          showIcon
          action={
            <Button onClick={() => navigate('/admin/business-metadata')}>
              Back to Tables
            </Button>
          }
        />
      </PageLayout>
    )
  }

  const pageTitle = isNewTable
    ? (tableId === 'add' ? 'Add New Business Table' : `Add Business Metadata: ${selectedTable?.schemaName}.${selectedTable?.tableName}`)
    : `Edit Business Table: ${selectedTable?.businessName || `${selectedTable?.schemaName}.${selectedTable?.tableName}`}`

  return (
    <PageLayout 
      title={pageTitle}
      extra={[
        <Button 
          key="back" 
          icon={<ArrowLeftOutlined />} 
          onClick={handleCancel}
        >
          Back to Tables
        </Button>,
        <Button 
          key="save" 
          type="primary" 
          icon={<SaveOutlined />}
          onClick={() => selectedTable && handleSave(selectedTable)}
          disabled={!selectedTable}
        >
          {isNewTable ? 'Create Table' : 'Save Changes'}
        </Button>
      ]}
    >
      {selectedTable && (
        <BusinessTableEditor
          open={true}
          table={selectedTable}
          onClose={handleCancel}
          onSave={handleSave}
          isFullPage={true}
        />
      )}
    </PageLayout>
  )
}

export default BusinessTableEditPage
