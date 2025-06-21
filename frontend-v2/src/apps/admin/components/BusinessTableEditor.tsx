import React, { useEffect, useState } from 'react'
import {
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  Switch,
  Button,
  Space,
  Tabs,
  Card,
  Row,
  Col,
  message,
  Divider,
  Typography,
  Table,
  Tag
} from 'antd'
import { PlusOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import {
  useCreateBusinessTableMutation,
  useUpdateBusinessTableMutation,
  useGetBusinessTableQuery,
  useGetBusinessTablesQuery,
  type BusinessTableInfoDto,
  type BusinessColumnInfoDto
} from '@shared/store/api/businessApi'
import { BusinessColumnEditor } from './BusinessColumnEditor'

const { TextArea } = Input
const { Option } = Select
const { TabPane } = Tabs
const { Text } = Typography

interface BusinessTableEditorProps {
  open: boolean
  table: BusinessTableInfoDto | null
  onClose: () => void
}

export const BusinessTableEditor: React.FC<BusinessTableEditorProps> = ({
  open,
  table,
  onClose,
}) => {
  const [form] = Form.useForm()
  const [activeTab, setActiveTab] = useState('basic')
  const [selectedColumn, setSelectedColumn] = useState<BusinessColumnInfoDto | null>(null)
  const [isColumnEditorOpen, setIsColumnEditorOpen] = useState(false)

  const [createTable, { isLoading: isCreating }] = useCreateBusinessTableMutation()
  const [updateTable, { isLoading: isUpdating }] = useUpdateBusinessTableMutation()

  // Fetch all business tables to find matching one for schema tables
  const { data: allBusinessTables } = useGetBusinessTablesQuery()

  // Determine table type:
  // - Real business tables: numeric ID > 0
  // - Schema tables with business context: string ID with business data
  // - New table templates: ID = 0
  const hasNumericId = table?.id && !isNaN(Number(table.id))
  const hasBusinessData = table?.businessPurpose || table?.businessContext || table?.domainClassification

  const isBusinessTable = hasNumericId && Number(table.id) > 0
  const isSchemaWithBusinessData = !hasNumericId && hasBusinessData
  const isNewTemplate = table?.id === 0

  // For schema tables, try to find matching business table
  const matchingBusinessTable = isSchemaWithBusinessData && allBusinessTables
    ? allBusinessTables.find(bt =>
        bt.schemaName === table?.schemaName && bt.tableName === table?.tableName
      )
    : null

  const tableIdAsNumber = isBusinessTable ? Number(table.id) : (matchingBusinessTable?.id || 0)

  console.log('🔍 Table analysis:', {
    id: table?.id,
    hasNumericId,
    hasBusinessData,
    isBusinessTable,
    isSchemaWithBusinessData,
    isNewTemplate,
    matchingBusinessTable: matchingBusinessTable?.id
  })

  // Fetch the actual business table data if we found a matching one
  const { data: actualBusinessTableData, isLoading: businessTableLoading } = useGetBusinessTableQuery(
    matchingBusinessTable?.id || 0,
    { skip: !matchingBusinessTable?.id }
  )

  // Use columns from the table data instead of fetching separately
  const columns = actualBusinessTableData?.columns || table?.columns || []
  const columnsLoading = businessTableLoading
  const columnsError = null

  // Debug logging for columns
  useEffect(() => {
    if (table?.id) {
      console.log('🔍 Table ID:', table.id, 'Is Business Table:', isBusinessTable)
      console.log('🔢 Table ID as Number:', tableIdAsNumber)
      if (isBusinessTable) {
        console.log('📊 Columns data:', columns)
        console.log('❌ Columns error:', columnsError)
        console.log('⏳ Columns loading:', columnsLoading)
      } else {
        console.log('ℹ️ Schema table - columns not available via business API')
      }
    }
  }, [table?.id, isBusinessTable, tableIdAsNumber, columns, columnsError, columnsLoading])

  // We're "editing" if we have a table with business data (either real business table or schema with business context)
  const isEditing = isBusinessTable || isSchemaWithBusinessData
  const isLoading = isCreating || isUpdating

  useEffect(() => {
    console.log('🔄 BusinessTableEditor useEffect triggered:', { open, table, isEditing })

    if (open && table) {
      console.log('🔍 BusinessTableEditor received table data:', table)
      console.log('🔍 Table type analysis:', {
        isEditing,
        isBusinessTable,
        isSchemaWithBusinessData,
        isNewTemplate,
        tableId: table.id,
        tableIdType: typeof table.id,
        matchingBusinessTable: matchingBusinessTable?.id,
        actualBusinessTableData: actualBusinessTableData?.id,
        businessTableLoading
      })

      // Use actual business table data if available, otherwise use passed table data
      const tableDataToUse = actualBusinessTableData || table
      console.log('📊 Using table data:', tableDataToUse)

      // Helper function to convert arrays/objects to strings for form display
      const convertToString = (value: any) => {
        if (Array.isArray(value)) {
          return value.length > 0 ? JSON.stringify(value, null, 2) : ''
        }
        if (typeof value === 'object' && value !== null) {
          return Object.keys(value).length > 0 ? JSON.stringify(value, null, 2) : ''
        }
        return value || ''
      }

      const formValues = {
        // Basic fields from API response
        id: tableDataToUse.id,
        tableId: tableDataToUse.tableId,
        tableName: tableDataToUse.tableName,
        schemaName: tableDataToUse.schemaName,
        businessName: tableDataToUse.businessName,
        businessPurpose: tableDataToUse.businessPurpose,
        businessContext: tableDataToUse.businessContext,
        primaryUseCase: tableDataToUse.primaryUseCase,
        businessRules: tableDataToUse.businessRules,
        isActive: tableDataToUse.isActive,

        // Convert complex fields to strings for form editing
        commonQueryPatterns: convertToString(tableDataToUse.commonQueryPatterns),
        naturalLanguageAliases: convertToString(tableDataToUse.naturalLanguageAliases || []),
        businessProcesses: convertToString(tableDataToUse.businessProcesses || []),
        analyticalUseCases: convertToString(tableDataToUse.analyticalUseCases || []),
        reportingCategories: convertToString(tableDataToUse.reportingCategories || []),
        vectorSearchKeywords: convertToString(tableDataToUse.vectorSearchKeywords || []),
        businessGlossaryTerms: convertToString(tableDataToUse.businessGlossaryTerms || []),
        llmContextHints: convertToString(tableDataToUse.llmContextHints || []),
        queryComplexityHints: convertToString(tableDataToUse.queryComplexityHints || []),
        semanticRelationships: convertToString(tableDataToUse.semanticRelationships || {}),
        usagePatterns: convertToString(tableDataToUse.usagePatterns || []),
        dataQualityIndicators: convertToString(tableDataToUse.dataQualityIndicators || {}),
        relationshipSemantics: convertToString(tableDataToUse.relationshipSemantics || {}),
        dataGovernancePolicies: convertToString(tableDataToUse.dataGovernancePolicies || {}),

        // Numeric fields
        importanceScore: tableDataToUse.importanceScore || 0.5,
        usageFrequency: tableDataToUse.usageFrequency || 0.5,
        semanticCoverageScore: tableDataToUse.semanticCoverageScore || 0.0,

        // Format dates for display
        lastAnalyzed: tableDataToUse.lastAnalyzed ? tableDataToUse.lastAnalyzed.split('T')[0] : '',
        createdDate: tableDataToUse.createdDate ? new Date(tableDataToUse.createdDate).toLocaleString() : '',
        updatedDate: tableDataToUse.updatedDate ? new Date(tableDataToUse.updatedDate).toLocaleString() : '',
      }

      console.log('📝 Setting form values:', formValues)
      console.log('📝 Form instance:', form)
      form.setFieldsValue(formValues)

      // Verify the values were set
      setTimeout(() => {
        const currentValues = form.getFieldsValue()
        console.log('✅ Form values after setting:', currentValues)
      }, 100)
    } else if (open) {
      // New table creation
      console.log('🆕 Creating new table - no table data provided')
      form.resetFields()
      form.setFieldsValue({
        isActive: true,
        importanceScore: 0.5,
        usageFrequency: 0.5,
        semanticCoverageScore: 0.0,
      })
    } else if (!open) {
      console.log('🔒 Modal closed - not setting form values')
    }
  }, [open, table, form, isEditing, actualBusinessTableData, businessTableLoading])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()

      // Helper function to parse JSON strings back to objects/arrays
      const parseJsonField = (value: string) => {
        if (!value || typeof value !== 'string') return value

        try {
          // Try to parse as JSON first
          return JSON.parse(value)
        } catch {
          // If JSON parsing fails, treat as comma-separated string and convert to array
          if (value.includes(',')) {
            return value.split(',').map(item => item.trim()).filter(item => item.length > 0)
          }
          // Return as-is if it's just a simple string
          return value
        }
      }

      // Process form values to convert string fields back to proper types
      const processedValues = {
        ...values,
        // Parse JSON/array fields
        naturalLanguageAliases: parseJsonField(values.naturalLanguageAliases),
        businessProcesses: parseJsonField(values.businessProcesses),
        analyticalUseCases: parseJsonField(values.analyticalUseCases),
        reportingCategories: parseJsonField(values.reportingCategories),
        vectorSearchKeywords: parseJsonField(values.vectorSearchKeywords),
        businessGlossaryTerms: parseJsonField(values.businessGlossaryTerms),
        llmContextHints: parseJsonField(values.llmContextHints),
        queryComplexityHints: parseJsonField(values.queryComplexityHints),
        semanticRelationships: parseJsonField(values.semanticRelationships),
        dataQualityIndicators: parseJsonField(values.dataQualityIndicators),
        dataGovernancePolicies: parseJsonField(values.dataGovernancePolicies),
        // Handle date fields
        lastAnalyzed: values.lastAnalyzed ? `${values.lastAnalyzed}T00:00:00Z` : null,
        // Ensure numeric fields are properly typed
        importanceScore: Number(values.importanceScore) || 0,
        usageFrequency: Number(values.usageFrequency) || 0,
        semanticCoverageScore: Number(values.semanticCoverageScore) || 0,
      }

      if (isEditing) {
        await updateTable({
          id: table.id,
          ...processedValues,
        }).unwrap()
        message.success('Business table updated successfully')
      } else {
        await createTable(processedValues).unwrap()
        message.success('Business table created successfully')
      }

      onClose()
    } catch (error: any) {
      console.error('Form submission error:', error)
      message.error(error.data?.message || 'Failed to save business table')
    }
  }

  const handleEditColumn = (column: BusinessColumnInfoDto) => {
    setSelectedColumn(column)
    setIsColumnEditorOpen(true)
  }

  const handleAddColumn = () => {
    setSelectedColumn(null)
    setIsColumnEditorOpen(true)
  }

  const handleColumnEditorClose = () => {
    setIsColumnEditorOpen(false)
    setSelectedColumn(null)
    // Note: Column data will be refreshed when the parent table is refetched
  }

  const columnTableColumns = [
    {
      title: 'Column Name',
      dataIndex: 'columnName',
      key: 'columnName',
      render: (text: string, record: BusinessColumnInfoDto) => (
        <div>
          <Text strong>{text}</Text>
          {record.isKeyColumn && <Tag color="gold" style={{ marginLeft: 8 }}>Key</Tag>}
          {record.isSensitiveData && <Tag color="red" style={{ marginLeft: 4 }}>Sensitive</Tag>}
          {record.isCalculatedField && <Tag color="blue" style={{ marginLeft: 4 }}>Calculated</Tag>}
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.businessMeaning}
          </Text>
        </div>
      ),
    },
    {
      title: 'Business Data Type',
      dataIndex: 'businessDataType',
      key: 'businessDataType',
      width: 120,
    },
    {
      title: 'Quality Score',
      dataIndex: 'dataQualityScore',
      key: 'dataQualityScore',
      width: 100,
      render: (score: number) => (
        <Tag color={score > 8 ? 'green' : score > 6 ? 'orange' : 'red'}>
          {score}/10
        </Tag>
      ),
    },
    {
      title: 'Usage',
      dataIndex: 'usageFrequency',
      key: 'usageFrequency',
      width: 80,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (record: BusinessColumnInfoDto) => (
        <Space>
          <Button 
            size="small" 
            icon={<EditOutlined />} 
            onClick={() => handleEditColumn(record)}
          />
          <Button 
            size="small" 
            icon={<DeleteOutlined />} 
            danger
          />
        </Space>
      ),
    },
  ]

  return (
    <>
      <Modal
        title={
          isEditing
            ? `Edit Business Table: ${table?.schemaName}.${table?.tableName}`
            : table?.schemaName && table?.tableName
              ? `Create Business Metadata: ${table.schemaName}.${table.tableName}`
              : 'Add Business Table'
        }
        open={open}
        onCancel={onClose}
        width={1000}
        footer={[
          <Button key="cancel" onClick={onClose}>
            Cancel
          </Button>,
          <Button key="submit" type="primary" loading={isLoading} onClick={handleSubmit}>
            {isEditing ? 'Update' : 'Create'}
          </Button>,
        ]}
      >
        <Form form={form} layout="vertical">
          <Tabs activeKey={activeTab} onChange={setActiveTab}>
            <TabPane tab="Basic Information" key="basic">
              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="schemaName"
                    label="Schema Name"
                    rules={[{ required: true, message: 'Schema name is required' }]}
                  >
                    <Input placeholder="e.g., dbo, sales, hr" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="tableName"
                    label="Table Name"
                    rules={[{ required: true, message: 'Table name is required' }]}
                  >
                    <Input placeholder="e.g., customers, orders, products" />
                  </Form.Item>
                </Col>
              </Row>

              <Form.Item
                name="businessPurpose"
                label="Business Purpose"
                rules={[{ required: true, message: 'Business purpose is required' }]}
              >
                <TextArea 
                  rows={3} 
                  placeholder="Describe the main business purpose of this table..."
                />
              </Form.Item>

              <Form.Item
                name="businessContext"
                label="Business Context"
              >
                <TextArea 
                  rows={3} 
                  placeholder="Provide additional business context and background..."
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="primaryUseCase"
                    label="Primary Use Case"
                  >
                    <Input placeholder="e.g., Customer Analytics, Sales Reporting" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="domainClassification"
                    label="Domain Classification"
                  >
                    <Select placeholder="Select domain">
                      <Option value="Customer">Customer</Option>
                      <Option value="Sales">Sales</Option>
                      <Option value="Finance">Finance</Option>
                      <Option value="Operations">Operations</Option>
                      <Option value="HR">Human Resources</Option>
                      <Option value="Marketing">Marketing</Option>
                      <Option value="Product">Product</Option>
                      <Option value="Reference">Reference Data</Option>
                    </Select>
                  </Form.Item>
                </Col>
              </Row>

            </TabPane>

            <TabPane tab="Advanced Metadata" key="advanced">
              <Form.Item
                name="semanticDescription"
                label="Semantic Description"
              >
                <TextArea
                  rows={3}
                  placeholder="Detailed semantic description of the table's purpose and content..."
                />
              </Form.Item>

              <Form.Item
                name="commonQueryPatterns"
                label="Common Query Patterns"
              >
                <TextArea
                  rows={3}
                  placeholder="Describe common ways this table is queried..."
                />
              </Form.Item>

              <Form.Item
                name="businessRules"
                label="Business Rules"
              >
                <TextArea
                  rows={3}
                  placeholder="Document important business rules and constraints..."
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="businessProcesses"
                    label="Business Processes"
                  >
                    <TextArea
                      rows={3}
                      placeholder="Business processes that use this table (JSON array or comma-separated)"
                    />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="analyticalUseCases"
                    label="Analytical Use Cases"
                  >
                    <TextArea
                      rows={3}
                      placeholder="Analytical use cases and reporting scenarios (JSON array or comma-separated)"
                    />
                  </Form.Item>
                </Col>
              </Row>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="reportingCategories"
                    label="Reporting Categories"
                  >
                    <TextArea
                      rows={2}
                      placeholder="Categories for reporting and classification (JSON array or comma-separated)"
                    />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="naturalLanguageAliases"
                    label="Natural Language Aliases"
                  >
                    <TextArea
                      rows={2}
                      placeholder="Alternative names users might use (JSON array or comma-separated)"
                    />
                  </Form.Item>
                </Col>
              </Row>

              <Form.Item
                name="usagePatterns"
                label="Usage Patterns"
              >
                <TextArea
                  rows={2}
                  placeholder="Describe how this table is typically used..."
                />
              </Form.Item>

              <Form.Item
                name="relationshipSemantics"
                label="Relationship Semantics"
              >
                <TextArea
                  rows={2}
                  placeholder="Describe relationships with other tables..."
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="importanceScore"
                    label="Importance Score (0-1)"
                  >
                    <InputNumber min={0} max={1} step={0.1} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="usageFrequency"
                    label="Usage Frequency (0-1)"
                  >
                    <InputNumber min={0} max={1} step={0.1} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
              </Row>
            </TabPane>

            <TabPane tab="AI & Search Metadata" key="ai-metadata">
              <Form.Item
                name="vectorSearchKeywords"
                label="Vector Search Keywords"
              >
                <TextArea
                  rows={3}
                  placeholder="Keywords for vector search and semantic matching (JSON array or comma-separated)"
                />
              </Form.Item>

              <Form.Item
                name="businessGlossaryTerms"
                label="Business Glossary Terms"
              >
                <TextArea
                  rows={2}
                  placeholder="Related business glossary terms (JSON array or comma-separated)"
                />
              </Form.Item>

              <Form.Item
                name="llmContextHints"
                label="LLM Context Hints"
              >
                <TextArea
                  rows={3}
                  placeholder="Hints and context for Large Language Models (JSON array or comma-separated)"
                />
              </Form.Item>

              <Form.Item
                name="queryComplexityHints"
                label="Query Complexity Hints"
              >
                <TextArea
                  rows={2}
                  placeholder="Hints about query complexity and optimization (JSON array or comma-separated)"
                />
              </Form.Item>

              <Form.Item
                name="semanticRelationships"
                label="Semantic Relationships"
              >
                <TextArea
                  rows={3}
                  placeholder="Semantic relationships with other entities (JSON object)"
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="semanticCoverageScore"
                    label="Semantic Coverage Score (0-1)"
                  >
                    <InputNumber min={0} max={1} step={0.01} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="lastAnalyzed"
                    label="Last Analyzed Date"
                  >
                    <Input placeholder="YYYY-MM-DD format" />
                  </Form.Item>
                </Col>
              </Row>
            </TabPane>

            <TabPane tab="Data Governance" key="governance">
              <Form.Item
                name="dataQualityIndicators"
                label="Data Quality Indicators"
              >
                <TextArea
                  rows={3}
                  placeholder="Notes about data quality, completeness, accuracy (JSON object or text)"
                />
              </Form.Item>

              <Form.Item
                name="dataGovernancePolicies"
                label="Data Governance Policies"
              >
                <TextArea
                  rows={3}
                  placeholder="Relevant data governance policies and compliance notes (JSON array or comma-separated)"
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="businessOwner"
                    label="Business Owner"
                  >
                    <Input placeholder="e.g., John Smith, Sales Team" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="createdBy"
                    label="Created By"
                  >
                    <Input placeholder="System user who created this record" />
                  </Form.Item>
                </Col>
              </Row>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="createdDate"
                    label="Created Date"
                  >
                    <Input placeholder="YYYY-MM-DD HH:mm:ss format" disabled />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="updatedDate"
                    label="Updated Date"
                  >
                    <Input placeholder="YYYY-MM-DD HH:mm:ss format" disabled />
                  </Form.Item>
                </Col>
              </Row>

              <Form.Item
                name="isActive"
                label="Active Status"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>
            </TabPane>

            {isEditing && (
            <TabPane tab={`Columns (${isBusinessTable ? columns?.length || 0 : 'Schema Only'})`} key="columns">
              {isBusinessTable ? (
                <>
                  <div style={{ marginBottom: 16 }}>
                    <Button
                      type="primary"
                      icon={<PlusOutlined />}
                      onClick={handleAddColumn}
                    >
                      Add Column
                    </Button>
                    {columnsLoading && (
                      <Text style={{ marginLeft: 16, color: '#1890ff' }}>Loading columns...</Text>
                    )}
                    {columnsError && (
                      <Text style={{ marginLeft: 16, color: '#ff4d4f' }}>
                        Error loading columns: {JSON.stringify(columnsError)}
                      </Text>
                    )}
                  </div>

                  <Table
                    columns={columnTableColumns}
                    dataSource={columns || []}
                    rowKey="id"
                    size="small"
                    pagination={false}
                    scroll={{ y: 400 }}
                    loading={columnsLoading}
                  />
                </>
              ) : (
                <Card style={{ textAlign: 'center', padding: '40px' }}>
                  <div style={{ marginBottom: '16px' }}>
                    <Text type="secondary" style={{ fontSize: '16px' }}>
                      📋 Schema Table - Business Columns Not Available
                    </Text>
                  </div>
                  <div style={{ marginBottom: '16px' }}>
                    <Text type="secondary">
                      This is a schema-discovered table. To manage business column metadata,
                      you need to first convert it to a business table by saving the business metadata.
                    </Text>
                  </div>
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      💡 Tip: Fill out the business information in the other tabs and save to enable column management.
                    </Text>
                  </div>
                </Card>
              )}
            </TabPane>
          )}
          </Tabs>
        </Form>
      </Modal>

      <BusinessColumnEditor
        open={isColumnEditorOpen}
        column={selectedColumn}
        tableId={tableIdAsNumber}
        onClose={handleColumnEditorClose}
      />
    </>
  )
}
