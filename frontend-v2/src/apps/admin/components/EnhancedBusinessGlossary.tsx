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
  Row,
  Col,
  Modal,
  Form,
  message,
  Tooltip,
  Statistic,
  Tabs,
  Badge,
  Tree,
  Drawer,
} from 'antd'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  BookOutlined,
  StarOutlined,
  BranchesOutlined,
  TagsOutlined,
  FilterOutlined,
  ExportOutlined,
  LinkOutlined,
} from '@ant-design/icons'
import {
  useGetEnhancedBusinessGlossaryQuery,
  useCreateEnhancedGlossaryTermMutation,
  useUpdateEnhancedGlossaryTermMutation,
  useDeleteEnhancedGlossaryTermMutation,
  useSearchEnhancedGlossaryTermsMutation,
  type EnhancedBusinessGlossaryTerm,
  type CreateGlossaryTermRequest,
  type UpdateGlossaryTermRequest,
} from '@shared/store/api/businessApi'
import type { ColumnsType } from 'antd/es/table'

const { Text, Title } = Typography
const { TextArea } = Input
const { Option } = Select
const { Search } = Input
const { TabPane } = Tabs

export const EnhancedBusinessGlossary: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCategory, setSelectedCategory] = useState<string>('')
  const [selectedDomain, setSelectedDomain] = useState<string>('')
  const [selectedTags, setSelectedTags] = useState<string[]>([])
  const [selectedTerm, setSelectedTerm] = useState<EnhancedBusinessGlossaryTerm | null>(null)
  const [isEditorOpen, setIsEditorOpen] = useState(false)
  const [isAdvancedSearchOpen, setIsAdvancedSearchOpen] = useState(false)
  const [activeTab, setActiveTab] = useState('list')
  const [form] = Form.useForm()

  const { data: glossaryData, isLoading, refetch } = useGetEnhancedBusinessGlossaryQuery({
    page: 1,
    pageSize: 50,
    search: searchTerm,
    category: selectedCategory,
    domain: selectedDomain,
    tags: selectedTags,
    includeRelationships: true,
  })

  const [createTerm, { isLoading: isCreating }] = useCreateEnhancedGlossaryTermMutation()
  const [updateTerm, { isLoading: isUpdating }] = useUpdateEnhancedGlossaryTermMutation()
  const [deleteTerm] = useDeleteEnhancedGlossaryTermMutation()
  const [searchTerms, { isLoading: isSearching }] = useSearchEnhancedGlossaryTermsMutation()

  const isEditing = !!selectedTerm
  const isFormLoading = isCreating || isUpdating

  const handleEdit = (record: EnhancedBusinessGlossaryTerm) => {
    setSelectedTerm(record)
    form.setFieldsValue({
      ...record,
      synonyms: record.synonyms?.join(', ') || '',
      relatedTerms: record.relatedTerms?.join(', ') || '',
      examples: record.examples?.join(', ') || '',
      dataExamples: record.dataExamples?.join(', ') || '',
      tags: record.tags || [],
    })
    setIsEditorOpen(true)
  }

  const handleCreate = () => {
    setSelectedTerm(null)
    form.resetFields()
    setIsEditorOpen(true)
  }

  const handleSave = async () => {
    try {
      const values = await form.validateFields()
      
      const termData = {
        ...values,
        synonyms: values.synonyms ? values.synonyms.split(',').map((s: string) => s.trim()) : [],
        relatedTerms: values.relatedTerms ? values.relatedTerms.split(',').map((s: string) => s.trim()) : [],
        examples: values.examples ? values.examples.split(',').map((s: string) => s.trim()) : [],
        dataExamples: values.dataExamples ? values.dataExamples.split(',').map((s: string) => s.trim()) : [],
      }

      if (isEditing && selectedTerm) {
        await updateTerm({ id: selectedTerm.id, ...termData }).unwrap()
        message.success('Term updated successfully')
      } else {
        await createTerm(termData as CreateGlossaryTermRequest).unwrap()
        message.success('Term created successfully')
      }

      setIsEditorOpen(false)
      setSelectedTerm(null)
      refetch()
    } catch (error) {
      message.error('Failed to save term')
    }
  }

  const handleDelete = async (id: number) => {
    try {
      await deleteTerm(id).unwrap()
      message.success('Term deleted successfully')
      refetch()
    } catch (error) {
      message.error('Failed to delete term')
    }
  }

  const columns: ColumnsType<EnhancedBusinessGlossaryTerm> = [
    {
      title: 'Term',
      dataIndex: 'term',
      key: 'term',
      width: 200,
      render: (term: string, record) => (
        <div>
          <Text strong>{term}</Text>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {record.category} • {record.domain}
          </div>
        </div>
      ),
      sorter: true,
    },
    {
      title: 'Definition',
      dataIndex: 'definition',
      key: 'definition',
      ellipsis: {
        showTitle: false,
      },
      render: (definition: string) => (
        <Tooltip title={definition}>
          <Text>{definition}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      width: 120,
      render: (category: string) => <Tag color="blue">{category}</Tag>,
      filters: [
        { text: 'Business Process', value: 'Business Process' },
        { text: 'Data Element', value: 'Data Element' },
        { text: 'Metric', value: 'Metric' },
        { text: 'Dimension', value: 'Dimension' },
        { text: 'Rule', value: 'Rule' },
      ],
    },
    {
      title: 'Domain',
      dataIndex: 'domain',
      key: 'domain',
      width: 120,
      render: (domain: string) => <Tag color="green">{domain}</Tag>,
      filters: [
        { text: 'Sales', value: 'Sales' },
        { text: 'Finance', value: 'Finance' },
        { text: 'HR', value: 'HR' },
        { text: 'Operations', value: 'Operations' },
        { text: 'Marketing', value: 'Marketing' },
      ],
    },
    {
      title: 'Quality Score',
      dataIndex: 'qualityScore',
      key: 'qualityScore',
      width: 120,
      render: (score: number) => (
        <Space>
          <Text style={{ color: score >= 80 ? '#52c41a' : score >= 60 ? '#faad14' : '#ff4d4f' }}>
            {score}%
          </Text>
          <div style={{ width: 60 }}>
            <div
              style={{
                height: 4,
                backgroundColor: '#f0f0f0',
                borderRadius: 2,
                overflow: 'hidden',
              }}
            >
              <div
                style={{
                  height: '100%',
                  width: `${score}%`,
                  backgroundColor: score >= 80 ? '#52c41a' : score >= 60 ? '#faad14' : '#ff4d4f',
                }}
              />
            </div>
          </div>
        </Space>
      ),
      sorter: true,
    },
    {
      title: 'Usage',
      dataIndex: 'usageFrequency',
      key: 'usageFrequency',
      width: 80,
      render: (frequency: number) => (
        <Badge count={frequency} style={{ backgroundColor: '#52c41a' }} />
      ),
      sorter: true,
    },
    {
      title: 'Tags',
      dataIndex: 'tags',
      key: 'tags',
      width: 150,
      render: (tags: string[]) => (
        <Space wrap>
          {tags?.slice(0, 2).map((tag, index) => (
            <Tag key={index} size="small" color="purple">
              {tag}
            </Tag>
          ))}
          {tags?.length > 2 && (
            <Tag size="small" color="default">
              +{tags.length - 2}
            </Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Edit">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
            />
          </Tooltip>
          <Tooltip title="View Relationships">
            <Button
              size="small"
              icon={<BranchesOutlined />}
              onClick={() => {
                setSelectedTerm(record)
                setActiveTab('relationships')
              }}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                Modal.confirm({
                  title: 'Delete Term',
                  content: `Are you sure you want to delete "${record.term}"?`,
                  onOk: () => handleDelete(record.id),
                })
              }}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const renderTermEditor = () => (
    <Modal
      title={isEditing ? 'Edit Business Term' : 'Create Business Term'}
      open={isEditorOpen}
      onOk={handleSave}
      onCancel={() => {
        setIsEditorOpen(false)
        setSelectedTerm(null)
      }}
      confirmLoading={isFormLoading}
      width={800}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          category: 'Data Element',
          domain: 'General',
          isActive: true,
        }}
      >
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="term"
              label="Term"
              rules={[{ required: true, message: 'Please enter the term' }]}
            >
              <Input placeholder="e.g., Customer Lifetime Value" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="businessOwner"
              label="Business Owner"
              rules={[{ required: true, message: 'Please enter the business owner' }]}
            >
              <Input placeholder="e.g., Sales Team, John Smith" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          name="definition"
          label="Definition"
          rules={[{ required: true, message: 'Please enter the definition' }]}
        >
          <TextArea
            rows={3}
            placeholder="Clear, concise definition of the business term..."
          />
        </Form.Item>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="category"
              label="Category"
              rules={[{ required: true, message: 'Please select a category' }]}
            >
              <Select placeholder="Select category">
                <Option value="Business Process">Business Process</Option>
                <Option value="Data Element">Data Element</Option>
                <Option value="Metric">Metric</Option>
                <Option value="Dimension">Dimension</Option>
                <Option value="Rule">Rule</Option>
                <Option value="KPI">KPI</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="domain"
              label="Domain"
              rules={[{ required: true, message: 'Please select a domain' }]}
            >
              <Select placeholder="Select domain">
                <Option value="Sales">Sales</Option>
                <Option value="Marketing">Marketing</Option>
                <Option value="Finance">Finance</Option>
                <Option value="Operations">Operations</Option>
                <Option value="Customer">Customer</Option>
                <Option value="Product">Product</Option>
                <Option value="General">General</Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="synonyms" label="Synonyms">
              <Input placeholder="Comma-separated synonyms" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="relatedTerms" label="Related Terms">
              <Input placeholder="Comma-separated related terms" />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="examples" label="Examples">
              <Input placeholder="Comma-separated examples" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="dataExamples" label="Data Examples">
              <Input placeholder="Comma-separated data examples" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item name="tags" label="Tags">
          <Select
            mode="tags"
            placeholder="Add tags"
            style={{ width: '100%' }}
          />
        </Form.Item>
      </Form>
    </Modal>
  )

  return (
    <div>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Title level={4} style={{ margin: 0 }}>
                <BookOutlined /> Enhanced Business Glossary
              </Title>
              <Badge count={glossaryData?.data?.length || 0} style={{ backgroundColor: '#52c41a' }} />
            </Space>
          </Col>
          <Col>
            <Space>
              <Button icon={<FilterOutlined />} onClick={() => setIsAdvancedSearchOpen(true)}>
                Advanced Search
              </Button>
              <Button icon={<ExportOutlined />}>
                Export
              </Button>
              <Button type="primary" icon={<PlusOutlined />} onClick={handleCreate}>
                Add Term
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Search and Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16} align="middle">
          <Col flex="auto">
            <Search
              placeholder="Search terms, definitions, or examples..."
              allowClear
              enterButton={<SearchOutlined />}
              size="large"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </Col>
          <Col>
            <Select
              placeholder="Category"
              style={{ width: 150 }}
              allowClear
              value={selectedCategory}
              onChange={setSelectedCategory}
            >
              <Option value="Business Process">Business Process</Option>
              <Option value="Data Element">Data Element</Option>
              <Option value="Metric">Metric</Option>
              <Option value="Dimension">Dimension</Option>
              <Option value="Rule">Rule</Option>
            </Select>
          </Col>
          <Col>
            <Select
              placeholder="Domain"
              style={{ width: 150 }}
              allowClear
              value={selectedDomain}
              onChange={setSelectedDomain}
            >
              <Option value="Sales">Sales</Option>
              <Option value="Finance">Finance</Option>
              <Option value="HR">HR</Option>
              <Option value="Operations">Operations</Option>
              <Option value="Marketing">Marketing</Option>
            </Select>
          </Col>
        </Row>
      </Card>

      {/* Main Content */}
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane
          tab={
            <Space>
              <BookOutlined />
              Terms List
            </Space>
          }
          key="list"
        >
          <Card>
            <Table
              columns={columns}
              dataSource={glossaryData?.data || []}
              rowKey="id"
              loading={isLoading}
              pagination={{
                current: glossaryData?.pagination?.currentPage,
                pageSize: glossaryData?.pagination?.pageSize,
                total: glossaryData?.pagination?.totalItems,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} terms`,
              }}
              scroll={{ x: 1200 }}
              size="small"
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BranchesOutlined />
              Relationships
            </Space>
          }
          key="relationships"
        >
          <Card title="Term Relationships">
            {selectedTerm ? (
              <div>
                <Title level={5}>{selectedTerm.term}</Title>
                <Text>{selectedTerm.definition}</Text>
                {/* Relationship visualization would go here */}
                <div style={{ marginTop: 16, textAlign: 'center', padding: 40 }}>
                  <Text type="secondary">
                    Relationship visualization component would be implemented here
                  </Text>
                </div>
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: 40 }}>
                <Text type="secondary">
                  Select a term from the list to view its relationships
                </Text>
              </div>
            )}
          </Card>
        </TabPane>
      </Tabs>

      {/* Term Editor Modal */}
      {renderTermEditor()}
    </div>
  )
}

export default EnhancedBusinessGlossary
