import React, { useEffect, useState } from 'react'
import {
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  Switch,
  Button,
  Row,
  Col,
  message,
  Typography,
  Divider,
  DatePicker,
  Tabs,
  Spin
} from 'antd'
import dayjs from 'dayjs'
import type { BusinessColumnInfoDto } from '@shared/store/api/businessApi'
import { useUpdateBusinessColumnMutation, useGetBusinessColumnQuery } from '@shared/store/api/businessApi'

const { TextArea } = Input
const { Option } = Select
const { Text } = Typography
const { TabPane } = Tabs

interface BusinessColumnEditorProps {
  open: boolean
  column: BusinessColumnInfoDto | null
  tableId: number
  onClose: () => void
}

export const BusinessColumnEditor: React.FC<BusinessColumnEditorProps> = ({
  open,
  column,
  tableId,
  onClose,
}) => {
  const [form] = Form.useForm()
  const [activeTab, setActiveTab] = useState('basic')
  const [updateColumn, { isLoading: isUpdating }] = useUpdateBusinessColumnMutation()

  const isEditing = !!column

  // Fetch full column details when editing
  const { data: fullColumnData, isLoading: isLoadingColumn } = useGetBusinessColumnQuery(
    column?.id || 0,
    { skip: !column?.id || !open }
  )

  // Use the full column data if available, otherwise fall back to the passed column
  const columnData = fullColumnData || column

  useEffect(() => {
    if (open && columnData) {
      console.log('🔍 Column data received:', columnData)

      // Reset form first to clear any previous values
      form.resetFields()

      // Add a small delay to ensure the modal is fully rendered
      setTimeout(() => {
        // Helper function to convert arrays to strings for form display
        const convertToString = (value: any) => {
          if (value === undefined || value === null) {
            return ''
          }
          if (Array.isArray(value)) {
            return value.length > 0 ? value.join(', ') : ''
          }
          if (typeof value === 'object') {
            return JSON.stringify(value, null, 2)
          }
          return String(value)
        }

        // Map the column data directly (it's already in camelCase)
        const formValues = {
          id: columnData.id,
          tableInfoId: columnData.tableInfoId || tableId,
          columnName: convertToString(columnData.columnName),
          businessMeaning: convertToString(columnData.businessMeaning),
          businessContext: convertToString(columnData.businessContext),
          businessPurpose: convertToString(columnData.businessPurpose),
          businessFriendlyName: convertToString(columnData.businessName || columnData.businessFriendlyName),
          naturalLanguageDescription: convertToString(columnData.naturalLanguageDescription),
          businessDataType: convertToString(columnData.businessDataType || columnData.dataType),

          // Convert arrays/objects to strings for form fields
          dataExamples: convertToString(columnData.dataExamples || columnData.sampleValues),
          valueExamples: convertToString(columnData.valueExamples || columnData.sampleValues),
          validationRules: convertToString(columnData.validationRules),
          naturalLanguageAliases: convertToString(columnData.naturalLanguageAliases),
          dataLineage: convertToString(columnData.dataLineage),
          calculationRules: convertToString(columnData.calculationRules),
          semanticTags: convertToString(columnData.semanticTags),
          constraintsAndRules: convertToString(columnData.constraintsAndRules),
          relatedBusinessTerms: convertToString(columnData.relatedBusinessTerms),
          vectorSearchTags: convertToString(columnData.vectorSearchTags),
          semanticContext: convertToString(columnData.semanticContext),
          conceptualRelationships: convertToString(columnData.conceptualRelationships),
          domainSpecificTerms: convertToString(columnData.domainSpecificTerms),
          queryIntentMapping: convertToString(columnData.queryIntentMapping),
          businessQuestionTypes: convertToString(columnData.businessQuestionTypes),
          semanticSynonyms: convertToString(columnData.semanticSynonyms),
          analyticalContext: convertToString(columnData.analyticalContext),
          businessMetrics: convertToString(columnData.businessMetrics),
          llmPromptHints: convertToString(columnData.llmPromptHints),
          relationshipContext: convertToString(columnData.relationshipContext),
          businessRules: convertToString(columnData.businessRules),

          // Numeric fields with defaults
          dataQualityScore: columnData.dataQualityScore !== undefined ? columnData.dataQualityScore : 5,
          usageFrequency: columnData.usageFrequency !== undefined ? columnData.usageFrequency : 0,
          semanticRelevanceScore: columnData.semanticRelevanceScore !== undefined ? columnData.semanticRelevanceScore : 0.5,
          importanceScore: columnData.importanceScore !== undefined ? columnData.importanceScore : 0.5,

          // Boolean fields
          isKeyColumn: columnData.isKeyColumn || columnData.isKey || false,
          isSensitiveData: columnData.isSensitiveData || false,
          isCalculatedField: columnData.isCalculatedField || false,
          isActive: columnData.isActive !== undefined ? columnData.isActive : true,

          // Other fields - ensure they're not undefined
          preferredAggregation: columnData.preferredAggregation || undefined,
          dataGovernanceLevel: columnData.dataGovernanceLevel || undefined,

          // Handle date field
          lastBusinessReview: columnData.lastBusinessReview ? dayjs(columnData.lastBusinessReview) : undefined,
        }

        console.log('📝 Setting form values:', formValues)
        form.setFieldsValue(formValues)

        // Force a re-render to ensure values are displayed
        setTimeout(() => {
          const currentValues = form.getFieldsValue()
          console.log('✅ Current form values after setting:', currentValues)
        }, 100)
      }, 100)
    } else if (open) {
      console.log('🆕 Opening form for new column')
      form.resetFields()
      form.setFieldsValue({
        tableInfoId: tableId,
        isActive: true,
        dataQualityScore: 5,
        usageFrequency: 0,
        semanticRelevanceScore: 0.5,
        importanceScore: 0.5,
        isKeyColumn: false,
        isSensitiveData: false,
        isCalculatedField: false,
      })
    }
  }, [open, columnData, tableId, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()

      if (isEditing && columnData) {
        // Convert form values to API format
        const updateRequest = {
          columnId: columnData.id,
          columnName: values.columnName,
          businessFriendlyName: values.businessFriendlyName,
          businessDataType: values.businessDataType,
          naturalLanguageDescription: values.naturalLanguageDescription,
          businessMeaning: values.businessMeaning,
          businessContext: values.businessContext,
          businessPurpose: values.businessPurpose,
          dataExamples: values.dataExamples || [],
          valueExamples: values.valueExamples || [],
          validationRules: values.validationRules,
          businessRules: values.businessRules,
          preferredAggregation: values.preferredAggregation,
          dataGovernanceLevel: values.dataGovernanceLevel,
          lastBusinessReview: values.lastBusinessReview ? values.lastBusinessReview.toISOString() : undefined,
          dataQualityScore: values.dataQualityScore,
          usageFrequency: values.usageFrequency,
          semanticRelevanceScore: values.semanticRelevanceScore,
          importanceScore: values.importanceScore,
          isActive: values.isActive,
          isKeyColumn: values.isKeyColumn,
          isSensitiveData: values.isSensitiveData,
          isCalculatedField: values.isCalculatedField,
        }

        await updateColumn(updateRequest).unwrap()
        message.success('Column updated successfully')
      } else {
        // TODO: Implement column creation when backend supports it
        message.info('Column creation not yet implemented')
      }

      onClose()
    } catch (error: any) {
      console.error('Failed to save column:', error)
      message.error(error?.data?.message || 'Failed to save column')
    }
  }

  return (
    <Modal
      key={`column-editor-${columnData?.id || 'new'}-${open}`}
      title={isEditing ? `Edit Business Column: ${columnData?.columnName || 'Unknown'}` : 'Add Business Column'}
      open={open}
      onCancel={onClose}
      width={1200}
      style={{ top: 20 }}
      destroyOnClose={true}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        <Button key="submit" type="primary" onClick={handleSubmit} loading={isUpdating}>
          {isEditing ? 'Update' : 'Create'}
        </Button>,
      ]}
    >
      {isLoadingColumn && isEditing ? (
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>Loading column details...</div>
        </div>
      ) : (
      <Form
        form={form}
        layout="vertical"
        preserve={false}
        key={`form-${columnData?.id || 'new'}`}
      >
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane tab="Basic Information" key="basic">
            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="columnName"
                  label="Column Name"
                  rules={[{ required: true, message: 'Column name is required' }]}
                >
                  <Input placeholder="e.g., customer_id, order_date" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="businessFriendlyName"
                  label="Business Friendly Name"
                >
                  <Input placeholder="User-friendly display name" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="businessDataType"
                  label="Business Data Type"
                >
                  <Select placeholder="Select business data type">
                    <Option value="Identifier">Identifier</Option>
                    <Option value="Name">Name</Option>
                    <Option value="Description">Description</Option>
                    <Option value="Date">Date</Option>
                    <Option value="Amount">Amount</Option>
                    <Option value="Quantity">Quantity</Option>
                    <Option value="Percentage">Percentage</Option>
                    <Option value="Status">Status</Option>
                    <Option value="Category">Category</Option>
                    <Option value="Flag">Flag</Option>
                    <Option value="Code">Code</Option>
                    <Option value="Address">Address</Option>
                    <Option value="Contact">Contact</Option>
                    <Option value="Text">Text</Option>
                    <Option value="Integer">Integer</Option>
                    <Option value="Decimal">Decimal</Option>
                    <Option value="DateTime">DateTime</Option>
                  </Select>
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              name="naturalLanguageDescription"
              label="Natural Language Description"
            >
              <TextArea
                rows={2}
                placeholder="Human-readable description of this column's purpose and usage..."
              />
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="businessMeaning"
                  label="Business Meaning"
                  rules={[{ required: true, message: 'Business meaning is required' }]}
                >
                  <TextArea
                    rows={2}
                    placeholder="Describe what this column represents in business terms..."
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="businessContext"
                  label="Business Context"
                >
                  <TextArea
                    rows={2}
                    placeholder="Additional context about how this column is used..."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              name="businessPurpose"
              label="Business Purpose"
            >
              <TextArea
                rows={1}
                placeholder="Primary business purpose and objectives for this column..."
              />
            </Form.Item>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="dataExamples"
                  label="Data Examples"
                >
                  <TextArea
                    rows={1}
                    placeholder="Example values (comma-separated)"
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="valueExamples"
                  label="Value Examples"
                >
                  <TextArea
                    rows={1}
                    placeholder="Typical value examples"
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="validationRules"
                  label="Validation Rules"
                >
                  <TextArea
                    rows={1}
                    placeholder="Business validation rules..."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              name="businessRules"
              label="Business Rules"
            >
              <TextArea
                rows={1}
                placeholder="Business rules and constraints..."
              />
            </Form.Item>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="preferredAggregation"
                  label="Preferred Aggregation"
                >
                  <Select placeholder="Select preferred aggregation">
                    <Option value="SUM">Sum</Option>
                    <Option value="COUNT">Count</Option>
                    <Option value="AVG">Average</Option>
                    <Option value="MIN">Minimum</Option>
                    <Option value="MAX">Maximum</Option>
                    <Option value="DISTINCT_COUNT">Distinct Count</Option>
                    <Option value="NONE">None</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="dataGovernanceLevel"
                  label="Data Governance Level"
                >
                  <Select placeholder="Select governance level">
                    <Option value="Public">Public</Option>
                    <Option value="Internal">Internal</Option>
                    <Option value="Confidential">Confidential</Option>
                    <Option value="Restricted">Restricted</Option>
                    <Option value="Highly Restricted">Highly Restricted</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="lastBusinessReview"
                  label="Last Business Review"
                >
                  <DatePicker style={{ width: '100%' }} />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={6}>
                <Form.Item
                  name="dataQualityScore"
                  label="Data Quality Score (1-10)"
                >
                  <InputNumber min={1} max={10} style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={6}>
                <Form.Item
                  name="usageFrequency"
                  label="Usage Frequency"
                >
                  <InputNumber min={0} style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={6}>
                <Form.Item
                  name="semanticRelevanceScore"
                  label="Semantic Relevance Score"
                >
                  <InputNumber min={0} max={1} step={0.01} style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={6}>
                <Form.Item
                  name="importanceScore"
                  label="Importance Score"
                >
                  <InputNumber min={0} max={1} step={0.01} style={{ width: '100%' }} />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="isActive"
                  label="Active"
                  valuePropName="checked"
                >
                  <Switch />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="isKeyColumn"
                  label="Key Column"
                  valuePropName="checked"
                >
                  <Switch />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="isSensitiveData"
                  label="Sensitive Data"
                  valuePropName="checked"
                >
                  <Switch />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="isCalculatedField"
                  label="Calculated Field"
                  valuePropName="checked"
                >
                  <Switch />
                </Form.Item>
              </Col>
              <Col span={16}>
                {/* Empty space for layout */}
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Advanced & Semantic" key="advanced">
            <Form.Item
              name="naturalLanguageAliases"
              label="Natural Language Aliases"
            >
              <Input placeholder="Alternative names users might use (comma-separated)" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="dataLineage"
                  label="Data Lineage"
                >
                  <TextArea
                    rows={1}
                    placeholder="Source systems and transformations..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="calculationRules"
                  label="Calculation Rules"
                >
                  <TextArea
                    rows={1}
                    placeholder="How calculated fields are derived..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="constraintsAndRules"
                  label="Constraints and Rules"
                >
                  <TextArea
                    rows={1}
                    placeholder="Technical and business constraints..."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              name="semanticTags"
              label="Semantic Tags"
            >
              <Input placeholder="Tags for semantic understanding (comma-separated)" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="semanticContext"
                  label="Semantic Context"
                >
                  <TextArea
                    rows={1}
                    placeholder="Semantic context and meaning..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="conceptualRelationships"
                  label="Conceptual Relationships"
                >
                  <TextArea
                    rows={1}
                    placeholder="Relationships to other concepts..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="domainSpecificTerms"
                  label="Domain Specific Terms"
                >
                  <TextArea
                    rows={1}
                    placeholder="Industry or domain specific terminology..."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="queryIntentMapping"
                  label="Query Intent Mapping"
                >
                  <TextArea
                    rows={1}
                    placeholder="How this maps to user query intents..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="businessQuestionTypes"
                  label="Business Question Types"
                >
                  <TextArea
                    rows={1}
                    placeholder="Types of business questions this answers..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="semanticSynonyms"
                  label="Semantic Synonyms"
                >
                  <TextArea
                    rows={1}
                    placeholder="Alternative terms with same meaning..."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="relatedBusinessTerms"
                  label="Related Business Terms"
                >
                  <Input placeholder="Related glossary terms (comma-separated)" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="analyticalContext"
                  label="Analytical Context"
                >
                  <TextArea
                    rows={1}
                    placeholder="How this is used in analytics..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="businessMetrics"
                  label="Business Metrics"
                >
                  <TextArea
                    rows={1}
                    placeholder="Related business metrics and KPIs..."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={8}>
                <Form.Item
                  name="relationshipContext"
                  label="Relationship Context"
                >
                  <TextArea
                    rows={1}
                    placeholder="Context about relationships to other data..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="llmPromptHints"
                  label="LLM Prompt Hints"
                >
                  <TextArea
                    rows={1}
                    placeholder="Hints for AI/LLM understanding..."
                  />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item
                  name="vectorSearchTags"
                  label="Vector Search Tags"
                >
                  <Input placeholder="Tags for vector search (comma-separated)" />
                </Form.Item>
              </Col>
            </Row>
          </TabPane>
        </Tabs>
      </Form>
      )}
    </Modal>
  )
}
