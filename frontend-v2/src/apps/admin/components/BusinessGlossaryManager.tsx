import React, { useState } from 'react'
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Typography,
  Input,
  Select,
  Modal,
  Form,
  Row,
  Col,
  message,
  Tooltip,
  Statistic
} from 'antd'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  BookOutlined,
  StarOutlined
} from '@ant-design/icons'
import {
  useGetBusinessGlossaryQuery,
  useCreateGlossaryTermMutation,
  useUpdateGlossaryTermMutation,
  useDeleteGlossaryTermMutation,
  type BusinessGlossaryDto
} from '@shared/store/api/businessApi'

const { Text, Title } = Typography
const { TextArea } = Input
const { Option } = Select
const { Search } = Input

export const BusinessGlossaryManager: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCategory, setSelectedCategory] = useState<string>()
  const [selectedTerm, setSelectedTerm] = useState<BusinessGlossaryDto | null>(null)
  const [isEditorOpen, setIsEditorOpen] = useState(false)
  const [form] = Form.useForm()

  const { data: glossaryData, isLoading } = useGetBusinessGlossaryQuery({
    page: 1,
    limit: 100,
    category: selectedCategory,
  })

  const [createTerm, { isLoading: isCreating }] = useCreateGlossaryTermMutation()
  const [updateTerm, { isLoading: isUpdating }] = useUpdateGlossaryTermMutation()
  const [deleteTerm] = useDeleteGlossaryTermMutation()

  const isEditing = !!selectedTerm
  const isFormLoading = isCreating || isUpdating

  const handleEdit = (record: BusinessGlossaryDto) => {
    setSelectedTerm(record)
    form.setFieldsValue({
      ...record,
      synonyms: record.synonyms || '',
      relatedTerms: record.relatedTerms || '',
      examples: record.examples || '',
      mappedTables: record.mappedTables || '',
      mappedColumns: record.mappedColumns || '',
      hierarchicalRelations: record.hierarchicalRelations || '',
      preferredCalculation: record.preferredCalculation || '',
      disambiguationRules: record.disambiguationRules || '',
      regulationReferences: record.regulationReferences || '',
      contextualVariations: record.contextualVariations || '',
    })
    setIsEditorOpen(true)
  }

  const handleAdd = () => {
    setSelectedTerm(null)
    form.resetFields()
    form.setFieldsValue({
      isActive: true,
      confidenceScore: 5,
      ambiguityScore: 1,
      usageCount: 0,
    })
    setIsEditorOpen(true)
  }

  const handleDelete = (record: BusinessGlossaryDto) => {
    Modal.confirm({
      title: 'Delete Glossary Term',
      content: `Are you sure you want to delete the term "${record.term}"?`,
      okText: 'Delete',
      okType: 'danger',
      onOk: async () => {
        try {
          await deleteTerm(record.id).unwrap()
          message.success('Glossary term deleted successfully')
        } catch (error) {
          message.error('Failed to delete glossary term')
        }
      },
    })
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (isEditing) {
        await updateTerm({
          id: selectedTerm.id,
          ...values,
        }).unwrap()
        message.success('Glossary term updated successfully')
      } else {
        await createTerm(values).unwrap()
        message.success('Glossary term created successfully')
      }
      
      setIsEditorOpen(false)
      setSelectedTerm(null)
    } catch (error: any) {
      message.error(error.data?.message || 'Failed to save glossary term')
    }
  }

  const filteredTerms = glossaryData?.terms?.filter(term =>
    term.term.toLowerCase().includes(searchTerm.toLowerCase()) ||
    term.definition.toLowerCase().includes(searchTerm.toLowerCase())
  ) || []

  const categories = [...new Set(glossaryData?.terms?.map(term => term.category) || [])]

  const columns = [
    {
      title: 'Term',
      dataIndex: 'term',
      key: 'term',
      width: 200,
      render: (text: string, record: BusinessGlossaryDto) => (
        <div>
          <Text strong>{text}</Text>
          <br />
          <Tag color="blue">{record.category}</Tag>
          <Tag color="green">{record.domain}</Tag>
        </div>
      ),
    },
    {
      title: 'Definition',
      dataIndex: 'definition',
      key: 'definition',
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Business Context',
      dataIndex: 'businessContext',
      key: 'businessContext',
      width: 200,
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text type="secondary">{text}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Metrics',
      key: 'metrics',
      width: 120,
      render: (record: BusinessGlossaryDto) => (
        <div>
          <div>
            <Text style={{ fontSize: '11px' }}>Confidence: </Text>
            <Tag color={record.confidenceScore > 8 ? 'green' : record.confidenceScore > 6 ? 'orange' : 'red'}>
              {record.confidenceScore}/10
            </Tag>
          </div>
          <div style={{ marginTop: 4 }}>
            <Text style={{ fontSize: '11px' }}>Usage: {record.usageCount}</Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Owner',
      dataIndex: 'businessOwner',
      key: 'businessOwner',
      width: 120,
      render: (owner: string) => owner || 'Not specified',
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'status',
      width: 80,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (record: BusinessGlossaryDto) => (
        <Space>
          <Button 
            size="small" 
            icon={<EditOutlined />} 
            onClick={() => handleEdit(record)}
          />
          <Button 
            size="small" 
            icon={<DeleteOutlined />} 
            danger 
            onClick={() => handleDelete(record)}
          />
        </Space>
      ),
    },
  ]

  return (
    <>
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Total Terms"
              value={glossaryData?.total || 0}
              prefix={<BookOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Categories"
              value={categories.length}
              prefix={<StarOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Active Terms"
              value={filteredTerms.filter(t => t.isActive).length}
              prefix={<BookOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Avg Confidence"
              value={
                filteredTerms.length > 0
                  ? (filteredTerms.reduce((sum, t) => sum + t.confidenceScore, 0) / filteredTerms.length).toFixed(1)
                  : 0
              }
              suffix="/10"
            />
          </Card>
        </Col>
      </Row>

      <Card
        title="Business Glossary Terms"
        extra={
          <Space>
            <Search
              placeholder="Search terms..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{ width: 200 }}
              prefix={<SearchOutlined />}
            />
            <Select
              placeholder="Filter by category"
              value={selectedCategory}
              onChange={setSelectedCategory}
              allowClear
              style={{ width: 150 }}
            >
              {categories.map(category => (
                <Option key={category} value={category}>{category}</Option>
              ))}
            </Select>
            <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
              Add Term
            </Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={filteredTerms}
          loading={isLoading}
          rowKey="id"
          size="small"
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} terms`,
          }}
        />
      </Card>

      <Modal
        title={isEditing ? 'Edit Glossary Term' : 'Add Glossary Term'}
        open={isEditorOpen}
        onCancel={() => setIsEditorOpen(false)}
        width={800}
        footer={[
          <Button key="cancel" onClick={() => setIsEditorOpen(false)}>
            Cancel
          </Button>,
          <Button key="submit" type="primary" loading={isFormLoading} onClick={handleSubmit}>
            {isEditing ? 'Update' : 'Create'}
          </Button>,
        ]}
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="term"
                label="Term"
                rules={[{ required: true, message: 'Term is required' }]}
              >
                <Input placeholder="e.g., Customer Lifetime Value" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="category"
                label="Category"
                rules={[{ required: true, message: 'Category is required' }]}
              >
                <Select placeholder="Select category">
                  <Option value="Metrics">Metrics</Option>
                  <Option value="Dimensions">Dimensions</Option>
                  <Option value="Business Rules">Business Rules</Option>
                  <Option value="Processes">Processes</Option>
                  <Option value="Systems">Systems</Option>
                  <Option value="Compliance">Compliance</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="definition"
            label="Definition"
            rules={[{ required: true, message: 'Definition is required' }]}
          >
            <TextArea rows={3} placeholder="Clear, concise definition of the term..." />
          </Form.Item>

          <Form.Item
            name="businessContext"
            label="Business Context"
          >
            <TextArea rows={2} placeholder="Additional business context..." />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="domain"
                label="Domain"
              >
                <Select placeholder="Select domain">
                  <Option value="Sales">Sales</Option>
                  <Option value="Marketing">Marketing</Option>
                  <Option value="Finance">Finance</Option>
                  <Option value="Operations">Operations</Option>
                  <Option value="Customer">Customer</Option>
                  <Option value="Product">Product</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="businessOwner"
                label="Business Owner"
              >
                <Input placeholder="e.g., Sales Team, John Smith" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="synonyms"
                label="Synonyms"
              >
                <Input placeholder="Alternative terms (comma-separated)" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="relatedTerms"
                label="Related Terms"
              >
                <Input placeholder="Related glossary terms (comma-separated)" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="examples"
            label="Examples"
          >
            <TextArea rows={2} placeholder="Practical examples of usage..." />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}
