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
  
  const { data: columns, refetch: refetchColumns } = useGetBusinessColumnsQuery(
    table?.id || 0,
    { skip: !table?.id }
  )

  const isEditing = !!table
  const isLoading = isCreating || isUpdating

  useEffect(() => {
    if (open && table) {
      form.setFieldsValue({
        ...table,
        // Convert arrays/objects to strings for form fields
        naturalLanguageAliases: table.naturalLanguageAliases || '',
        usagePatterns: table.usagePatterns || '',
        dataQualityIndicators: table.dataQualityIndicators || '',
        relationshipSemantics: table.relationshipSemantics || '',
        dataGovernancePolicies: table.dataGovernancePolicies || '',
      })
    } else if (open) {
      form.resetFields()
      form.setFieldsValue({
        isActive: true,
        importanceScore: 5,
        usageFrequency: 0,
      })
    }
  }, [open, table, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (isEditing) {
        await updateTable({
          id: table.id,
          ...values,
        }).unwrap()
        message.success('Business table updated successfully')
      } else {
        await createTable(values).unwrap()
        message.success('Business table created successfully')
      }
      
      onClose()
    } catch (error: any) {
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
                    name="isActive"
                    label="Active Status"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                </Col>
              </Row>
            </Form>
          </TabPane>

          <TabPane tab="Advanced Metadata" key="advanced">
            <Form form={form} layout="vertical">
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

              <Form.Item
                name="naturalLanguageAliases"
                label="Natural Language Aliases"
              >
                <TextArea 
                  rows={2} 
                  placeholder="Alternative names users might use (comma-separated)"
                />
              </Form.Item>

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
                name="dataQualityIndicators"
                label="Data Quality Indicators"
              >
                <TextArea 
                  rows={2} 
                  placeholder="Notes about data quality, completeness, accuracy..."
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

              <Form.Item
                name="dataGovernancePolicies"
                label="Data Governance Policies"
              >
                <TextArea 
                  rows={2} 
                  placeholder="Relevant data governance policies and compliance notes..."
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="importanceScore"
                    label="Importance Score (1-10)"
                  >
                    <InputNumber min={1} max={10} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="usageFrequency"
                    label="Usage Frequency"
                  >
                    <InputNumber min={0} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
              </Row>
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
              </div>
              
              <Table
                columns={columnTableColumns}
                dataSource={columns || []}
                rowKey="id"
                size="small"
                pagination={false}
                scroll={{ y: 400 }}
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
