import React, { useState } from 'react'
import {
  Card,
  Tree,
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
  List,
  Descriptions,
  Drawer,
} from 'antd'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  ApartmentOutlined,
  UserOutlined,
  SafetyOutlined,
  BranchesOutlined,
  TagsOutlined,
  ExportOutlined,
  EyeOutlined,
} from '@ant-design/icons'
import {
  useGetEnhancedBusinessDomainsQuery,
  useCreateEnhancedBusinessDomainMutation,
  useUpdateEnhancedBusinessDomainMutation,
  useDeleteEnhancedBusinessDomainMutation,
  type BusinessDomain,
  type CreateDomainRequest,
  type UpdateDomainRequest,
} from '@shared/store/api/businessApi'
import type { DataNode } from 'antd/es/tree'

const { Text, Title } = Typography
const { TextArea } = Input
const { Option } = Select
const { Search } = Input
const { TabPane } = Tabs

export const EnhancedBusinessDomains: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedDomain, setSelectedDomain] = useState<BusinessDomain | null>(null)
  const [isEditorOpen, setIsEditorOpen] = useState(false)
  const [isDetailsOpen, setIsDetailsOpen] = useState(false)
  const [activeTab, setActiveTab] = useState('hierarchy')
  const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([])
  const [form] = Form.useForm()

  const { data: domainsData, isLoading, refetch } = useGetEnhancedBusinessDomainsQuery({
    page: 1,
    pageSize: 100,
    search: searchTerm,
    includeSubDomains: true,
  })

  const [createDomain, { isLoading: isCreating }] = useCreateEnhancedBusinessDomainMutation()
  const [updateDomain, { isLoading: isUpdating }] = useUpdateEnhancedBusinessDomainMutation()
  const [deleteDomain] = useDeleteEnhancedBusinessDomainMutation()

  const isEditing = !!selectedDomain
  const isFormLoading = isCreating || isUpdating

  // Convert domains to tree data structure
  const buildTreeData = (domains: BusinessDomain[]): DataNode[] => {
    const domainMap = new Map<number, BusinessDomain>()
    const rootDomains: BusinessDomain[] = []

    // First pass: create map and identify root domains
    domains.forEach(domain => {
      domainMap.set(domain.id, domain)
      if (!domain.parentDomainId) {
        rootDomains.push(domain)
      }
    })

    // Second pass: build tree structure
    const buildNode = (domain: BusinessDomain): DataNode => {
      const children = domains
        .filter(d => d.parentDomainId === domain.id)
        .map(buildNode)

      return {
        key: domain.id,
        title: (
          <Space>
            <Text strong>{domain.name}</Text>
            <Badge count={domain.relatedTables?.length || 0} size="small" style={{ backgroundColor: '#52c41a' }} />
            <Tag color={domain.isActive ? 'green' : 'red'} size="small">
              {domain.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Space>
        ),
        children: children.length > 0 ? children : undefined,
        data: domain,
      }
    }

    return rootDomains.map(buildNode)
  }

  const treeData = buildTreeData(domainsData?.data || [])

  const handleEdit = (domain: BusinessDomain) => {
    setSelectedDomain(domain)
    form.setFieldsValue({
      ...domain,
      domainExperts: domain.domainExperts?.join(', ') || '',
      governanceRules: domain.governanceRules?.join(', ') || '',
      complianceRequirements: domain.complianceRequirements?.join(', ') || '',
    })
    setIsEditorOpen(true)
  }

  const handleCreate = (parentDomainId?: number) => {
    setSelectedDomain(null)
    form.resetFields()
    if (parentDomainId) {
      form.setFieldValue('parentDomainId', parentDomainId)
    }
    setIsEditorOpen(true)
  }

  const handleSave = async () => {
    try {
      const values = await form.validateFields()
      
      const domainData = {
        ...values,
        domainExperts: values.domainExperts ? values.domainExperts.split(',').map((s: string) => s.trim()) : [],
        governanceRules: values.governanceRules ? values.governanceRules.split(',').map((s: string) => s.trim()) : [],
        complianceRequirements: values.complianceRequirements ? values.complianceRequirements.split(',').map((s: string) => s.trim()) : [],
      }

      if (isEditing && selectedDomain) {
        await updateDomain({ id: selectedDomain.id, ...domainData }).unwrap()
        message.success('Domain updated successfully')
      } else {
        await createDomain(domainData as CreateDomainRequest).unwrap()
        message.success('Domain created successfully')
      }

      setIsEditorOpen(false)
      setSelectedDomain(null)
      refetch()
    } catch (error) {
      message.error('Failed to save domain')
    }
  }

  const handleDelete = async (id: number, name: string) => {
    try {
      await deleteDomain(id).unwrap()
      message.success('Domain deleted successfully')
      refetch()
    } catch (error) {
      message.error('Failed to delete domain')
    }
  }

  const handleViewDetails = (domain: BusinessDomain) => {
    setSelectedDomain(domain)
    setIsDetailsOpen(true)
  }

  const onTreeSelect = (selectedKeys: React.Key[], info: any) => {
    if (selectedKeys.length > 0 && info.node.data) {
      setSelectedDomain(info.node.data)
    }
  }

  const renderDomainEditor = () => (
    <Modal
      title={isEditing ? 'Edit Business Domain' : 'Create Business Domain'}
      open={isEditorOpen}
      onOk={handleSave}
      onCancel={() => {
        setIsEditorOpen(false)
        setSelectedDomain(null)
      }}
      confirmLoading={isFormLoading}
      width={800}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          dataGovernanceLevel: 'Standard',
          dataClassification: 'Internal',
          isActive: true,
        }}
      >
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="name"
              label="Domain Name"
              rules={[{ required: true, message: 'Please enter the domain name' }]}
            >
              <Input placeholder="e.g., Customer Management" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="parentDomainId" label="Parent Domain">
              <Select placeholder="Select parent domain (optional)" allowClear>
                {domainsData?.data?.map(domain => (
                  <Option key={domain.id} value={domain.id}>
                    {domain.name}
                  </Option>
                ))}
              </Select>
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          name="description"
          label="Description"
          rules={[{ required: true, message: 'Please enter the description' }]}
        >
          <TextArea
            rows={3}
            placeholder="Describe the business domain, its scope, and purpose..."
          />
        </Form.Item>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="businessOwner"
              label="Business Owner"
              rules={[{ required: true, message: 'Please enter the business owner' }]}
            >
              <Input placeholder="e.g., Sales Director, John Smith" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="dataGovernanceLevel"
              label="Data Governance Level"
              rules={[{ required: true, message: 'Please select governance level' }]}
            >
              <Select placeholder="Select governance level">
                <Option value="Basic">Basic</Option>
                <Option value="Standard">Standard</Option>
                <Option value="Advanced">Advanced</Option>
                <Option value="Enterprise">Enterprise</Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="dataClassification"
              label="Data Classification"
              rules={[{ required: true, message: 'Please select data classification' }]}
            >
              <Select placeholder="Select data classification">
                <Option value="Public">Public</Option>
                <Option value="Internal">Internal</Option>
                <Option value="Confidential">Confidential</Option>
                <Option value="Restricted">Restricted</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="domainExperts" label="Domain Experts">
              <Input placeholder="Comma-separated list of domain experts" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item name="governanceRules" label="Governance Rules">
          <Input placeholder="Comma-separated governance rules" />
        </Form.Item>

        <Form.Item name="complianceRequirements" label="Compliance Requirements">
          <Input placeholder="Comma-separated compliance requirements" />
        </Form.Item>
      </Form>
    </Modal>
  )

  const renderDomainDetails = () => (
    <Drawer
      title={selectedDomain ? `Domain: ${selectedDomain.name}` : 'Domain Details'}
      width={600}
      open={isDetailsOpen}
      onClose={() => setIsDetailsOpen(false)}
      extra={
        selectedDomain && (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => handleEdit(selectedDomain)}>
              Edit
            </Button>
            <Button
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                Modal.confirm({
                  title: 'Delete Domain',
                  content: `Are you sure you want to delete "${selectedDomain.name}"?`,
                  onOk: () => handleDelete(selectedDomain.id, selectedDomain.name),
                })
              }}
            >
              Delete
            </Button>
          </Space>
        )
      }
    >
      {selectedDomain && (
        <Space direction="vertical" style={{ width: '100%' }}>
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Name">{selectedDomain.name}</Descriptions.Item>
            <Descriptions.Item label="Description">{selectedDomain.description}</Descriptions.Item>
            <Descriptions.Item label="Business Owner">{selectedDomain.businessOwner}</Descriptions.Item>
            <Descriptions.Item label="Data Governance Level">
              <Tag color="blue">{selectedDomain.dataGovernanceLevel}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Data Classification">
              <Tag color="orange">{selectedDomain.dataClassification}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color={selectedDomain.isActive ? 'green' : 'red'}>
                {selectedDomain.isActive ? 'Active' : 'Inactive'}
              </Tag>
            </Descriptions.Item>
          </Descriptions>

          {selectedDomain.domainExperts && selectedDomain.domainExperts.length > 0 && (
            <Card title="Domain Experts" size="small">
              <Space wrap>
                {selectedDomain.domainExperts.map((expert, index) => (
                  <Tag key={index} icon={<UserOutlined />}>
                    {expert}
                  </Tag>
                ))}
              </Space>
            </Card>
          )}

          {selectedDomain.relatedTables && selectedDomain.relatedTables.length > 0 && (
            <Card title="Related Tables" size="small">
              <List
                size="small"
                dataSource={selectedDomain.relatedTables}
                renderItem={(table) => (
                  <List.Item>
                    <Text>{table}</Text>
                  </List.Item>
                )}
              />
            </Card>
          )}

          {selectedDomain.governanceRules && selectedDomain.governanceRules.length > 0 && (
            <Card title="Governance Rules" size="small">
              <List
                size="small"
                dataSource={selectedDomain.governanceRules}
                renderItem={(rule) => (
                  <List.Item>
                    <Text>{rule}</Text>
                  </List.Item>
                )}
              />
            </Card>
          )}

          {selectedDomain.complianceRequirements && selectedDomain.complianceRequirements.length > 0 && (
            <Card title="Compliance Requirements" size="small">
              <Space wrap>
                {selectedDomain.complianceRequirements.map((req, index) => (
                  <Tag key={index} color="red" icon={<SafetyOutlined />}>
                    {req}
                  </Tag>
                ))}
              </Space>
            </Card>
          )}
        </Space>
      )}
    </Drawer>
  )

  return (
    <div>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Title level={4} style={{ margin: 0 }}>
                <ApartmentOutlined /> Enhanced Business Domains
              </Title>
              <Badge count={domainsData?.data?.length || 0} style={{ backgroundColor: '#52c41a' }} />
            </Space>
          </Col>
          <Col>
            <Space>
              <Button icon={<ExportOutlined />}>
                Export
              </Button>
              <Button type="primary" icon={<PlusOutlined />} onClick={() => handleCreate()}>
                Add Domain
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Search */}
      <Card style={{ marginBottom: 16 }}>
        <Search
          placeholder="Search domains..."
          allowClear
          enterButton={<SearchOutlined />}
          size="large"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </Card>

      {/* Main Content */}
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane
          tab={
            <Space>
              <ApartmentOutlined />
              Domain Hierarchy
            </Space>
          }
          key="hierarchy"
        >
          <Card>
            <Tree
              treeData={treeData}
              onSelect={onTreeSelect}
              expandedKeys={expandedKeys}
              onExpand={setExpandedKeys}
              showLine={{ showLeafIcon: false }}
              titleRender={(nodeData) => (
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', width: '100%' }}>
                  <div>{nodeData.title}</div>
                  <Space size="small">
                    <Tooltip title="View Details">
                      <Button
                        size="small"
                        icon={<EyeOutlined />}
                        onClick={(e) => {
                          e.stopPropagation()
                          handleViewDetails(nodeData.data as BusinessDomain)
                        }}
                      />
                    </Tooltip>
                    <Tooltip title="Add Sub-domain">
                      <Button
                        size="small"
                        icon={<PlusOutlined />}
                        onClick={(e) => {
                          e.stopPropagation()
                          handleCreate((nodeData.data as BusinessDomain).id)
                        }}
                      />
                    </Tooltip>
                    <Tooltip title="Edit">
                      <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={(e) => {
                          e.stopPropagation()
                          handleEdit(nodeData.data as BusinessDomain)
                        }}
                      />
                    </Tooltip>
                  </Space>
                </div>
              )}
              loading={isLoading}
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BranchesOutlined />
              Domain Analytics
            </Space>
          }
          key="analytics"
        >
          <Card title="Domain Analytics">
            <Row gutter={16}>
              <Col span={6}>
                <Statistic
                  title="Total Domains"
                  value={domainsData?.data?.length || 0}
                  prefix={<ApartmentOutlined />}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Active Domains"
                  value={domainsData?.data?.filter(d => d.isActive).length || 0}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Root Domains"
                  value={domainsData?.data?.filter(d => !d.parentDomainId).length || 0}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Sub-domains"
                  value={domainsData?.data?.filter(d => d.parentDomainId).length || 0}
                />
              </Col>
            </Row>
          </Card>
        </TabPane>
      </Tabs>

      {/* Domain Editor Modal */}
      {renderDomainEditor()}

      {/* Domain Details Drawer */}
      {renderDomainDetails()}
    </div>
  )
}

export default EnhancedBusinessDomains
