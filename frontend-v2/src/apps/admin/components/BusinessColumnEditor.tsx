import React, { useEffect } from 'react'
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
  Divider
} from 'antd'
import type { BusinessColumnInfoDto } from '@shared/store/api/businessApi'

const { TextArea } = Input
const { Option } = Select
const { Text } = Typography

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

  const isEditing = !!column

  useEffect(() => {
    if (open && column) {
      form.setFieldsValue({
        ...column,
        // Convert arrays/objects to strings for form fields
        naturalLanguageAliases: column.naturalLanguageAliases || '',
        valueExamples: column.valueExamples || '',
        dataLineage: column.dataLineage || '',
        calculationRules: column.calculationRules || '',
        semanticTags: column.semanticTags || '',
        constraintsAndRules: column.constraintsAndRules || '',
        relatedBusinessTerms: column.relatedBusinessTerms || '',
      })
    } else if (open) {
      form.resetFields()
      form.setFieldsValue({
        tableInfoId: tableId,
        isActive: true,
        dataQualityScore: 5,
        usageFrequency: 0,
        isKeyColumn: false,
        isSensitiveData: false,
        isCalculatedField: false,
      })
    }
  }, [open, column, tableId, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      // Here you would call the API to create/update the column
      // For now, just show success message
      message.success(isEditing ? 'Column updated successfully' : 'Column created successfully')
      onClose()
    } catch (error: any) {
      message.error('Failed to save column')
    }
  }

  return (
    <Modal
      title={isEditing ? 'Edit Business Column' : 'Add Business Column'}
      open={open}
      onCancel={onClose}
      width={1200}
      style={{ top: 20 }}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        <Button key="submit" type="primary" onClick={handleSubmit}>
          {isEditing ? 'Update' : 'Create'}
        </Button>,
      ]}
    >
      <Form form={form} layout="vertical">
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="columnName"
              label="Column Name"
              rules={[{ required: true, message: 'Column name is required' }]}
            >
              <Input placeholder="e.g., customer_id, order_date" />
            </Form.Item>
          </Col>
          <Col span={12}>
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
              </Select>
            </Form.Item>
          </Col>
        </Row>

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
          <Col span={12}>
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
          <Col span={12}>
            <Form.Item
              name="relatedBusinessTerms"
              label="Related Business Terms"
            >
              <Input placeholder="Related glossary terms (comma-separated)" />
            </Form.Item>
          </Col>
        </Row>

        <Divider>Metrics and Flags</Divider>

        <Row gutter={16}>
          <Col span={8}>
            <Form.Item
              name="dataQualityScore"
              label="Data Quality Score (1-10)"
            >
              <InputNumber min={1} max={10} style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              name="usageFrequency"
              label="Usage Frequency"
            >
              <InputNumber min={0} style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              name="isActive"
              label="Active"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
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
          <Col span={8}>
            <Form.Item
              name="isCalculatedField"
              label="Calculated Field"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>
          </Col>
        </Row>
      </Form>
    </Modal>
  )
}
