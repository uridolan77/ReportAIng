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
  useGetBusinessColumnsQuery,
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
  
  const { data: columns, refetch: refetchColumns, error: columnsError, isLoading: columnsLoading } = useGetBusinessColumnsQuery(
    table?.id || 0,
    { skip: !table?.id }
  )

  // Debug logging for columns
  useEffect(() => {
    if (table?.id) {
      console.log('🔍 Fetching columns for table ID:', table.id)
      console.log('📊 Columns data:', columns)
      console.log('❌ Columns error:', columnsError)
      console.log('⏳ Columns loading:', columnsLoading)
    }
  }, [table?.id, columns, columnsError, columnsLoading])

  const isEditing = !!table
  const isLoading = isCreating || isUpdating

  useEffect(() => {
    if (open && table) {
      console.log('🔍 BusinessTableEditor received table data:', table)

      // Helper function to convert arrays/objects to strings for form display
      const convertToString = (value: any) => {
        if (Array.isArray(value)) {
          return JSON.stringify(value, null, 2)
        }
        if (typeof value === 'object' && value !== null) {
          return JSON.stringify(value, null, 2)
        }
        return value || ''
      }

      const formValues = {
        ...table,
        // Convert complex fields to strings for form editing
        naturalLanguageAliases: convertToString(table.naturalLanguageAliases),
        businessProcesses: convertToString(table.businessProcesses),
        analyticalUseCases: convertToString(table.analyticalUseCases),
        reportingCategories: convertToString(table.reportingCategories),
        vectorSearchKeywords: convertToString(table.vectorSearchKeywords),
        businessGlossaryTerms: convertToString(table.businessGlossaryTerms),
        llmContextHints: convertToString(table.llmContextHints),
        queryComplexityHints: convertToString(table.queryComplexityHints),
        semanticRelationships: convertToString(table.semanticRelationships),
        usagePatterns: convertToString(table.usagePatterns),
        dataQualityIndicators: convertToString(table.dataQualityIndicators),
        relationshipSemantics: convertToString(table.relationshipSemantics),
        dataGovernancePolicies: convertToString(table.dataGovernancePolicies),
        // Format dates for display
        lastAnalyzed: table.lastAnalyzed ? table.lastAnalyzed.split('T')[0] : '',
        createdDate: table.createdDate ? new Date(table.createdDate).toLocaleString() : '',
        updatedDate: table.updatedDate ? new Date(table.updatedDate).toLocaleString() : '',
      }

      console.log('📝 Setting form values:', formValues)
      form.setFieldsValue(formValues)
    } else if (open) {
      form.resetFields()
      form.setFieldsValue({
        isActive: true,
        importanceScore: 0.5,
        usageFrequency: 0.5,
        semanticCoverageScore: 0.0,
      })
    }
  }, [open, table, form])

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
    refetchColumns()
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
        title={isEditing ? 'Edit Business Table' : 'Add Business Table'}
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
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane tab="Basic Information" key="basic">
            <Form form={form} layout="vertical">
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


            </Form>
          </TabPane>

          <TabPane tab="Advanced Metadata" key="advanced">
            <Form form={form} layout="vertical">
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
            </Form>
          </TabPane>

          <TabPane tab="AI & Search Metadata" key="ai-metadata">
            <Form form={form} layout="vertical">
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
            </Form>
          </TabPane>

          <TabPane tab="Data Governance" key="governance">
            <Form form={form} layout="vertical">
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
            </Form>
          </TabPane>

          {isEditing && (
            <TabPane tab={`Columns (${columns?.length || 0})`} key="columns">
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
            </TabPane>
          )}
        </Tabs>
      </Modal>

      <BusinessColumnEditor
        open={isColumnEditorOpen}
        column={selectedColumn}
        tableId={table?.id || 0}
        onClose={handleColumnEditorClose}
      />
    </>
  )
}
