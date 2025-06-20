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
  Switch,
  Avatar,
  Dropdown,
  Tooltip,
  Row,
  Col,
  Statistic,
  Progress,
  DatePicker,
  message
} from 'antd'
import {
  UserOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  LockOutlined,
  UnlockOutlined,
  MailOutlined,
  PhoneOutlined,
  TeamOutlined,
  CrownOutlined,
  SafetyOutlined,
  SearchOutlined,
  FilterOutlined,
  ExportOutlined,
  MoreOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { useGetUsersQuery, useCreateUserMutation, useUpdateUserMutation, useDeleteUserMutation } from '@shared/store/api/adminApi'

const { Text, Title } = Typography
const { Search } = Input
const { Option } = Select

interface User {
  id: string
  username: string
  email: string
  firstName: string
  lastName: string
  role: 'Admin' | 'Analyst' | 'Viewer'
  isActive: boolean
  lastLogin: string
  createdAt: string
  permissions: string[]
  department?: string
  phone?: string
  avatar?: string
}

export default function UserManagement() {
  const [searchText, setSearchText] = useState('')
  const [roleFilter, setRoleFilter] = useState<string>('')
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingUser, setEditingUser] = useState<User | null>(null)
  const [form] = Form.useForm()

  const { data: users, isLoading, refetch } = useGetUsersQuery({
    search: searchText,
    role: roleFilter,
    status: statusFilter
  })

  const [createUser, { isLoading: isCreating }] = useCreateUserMutation()
  const [updateUser, { isLoading: isUpdating }] = useUpdateUserMutation()
  const [deleteUser] = useDeleteUserMutation()

  // Mock data for demonstration
  const mockUsers: User[] = [
    {
      id: '1',
      username: 'admin',
      email: 'admin@company.com',
      firstName: 'System',
      lastName: 'Administrator',
      role: 'Admin',
      isActive: true,
      lastLogin: '2024-01-15T10:30:00Z',
      createdAt: '2023-01-01T00:00:00Z',
      permissions: ['all'],
      department: 'IT',
      phone: '+1-555-0101'
    },
    {
      id: '2',
      username: 'analyst1',
      email: 'john.doe@company.com',
      firstName: 'John',
      lastName: 'Doe',
      role: 'Analyst',
      isActive: true,
      lastLogin: '2024-01-15T09:15:00Z',
      createdAt: '2023-06-15T00:00:00Z',
      permissions: ['query', 'export', 'dashboard'],
      department: 'Sales',
      phone: '+1-555-0102'
    },
    {
      id: '3',
      username: 'viewer1',
      email: 'jane.smith@company.com',
      firstName: 'Jane',
      lastName: 'Smith',
      role: 'Viewer',
      isActive: true,
      lastLogin: '2024-01-14T16:45:00Z',
      createdAt: '2023-09-20T00:00:00Z',
      permissions: ['view'],
      department: 'Marketing'
    },
    {
      id: '4',
      username: 'analyst2',
      email: 'mike.wilson@company.com',
      firstName: 'Mike',
      lastName: 'Wilson',
      role: 'Analyst',
      isActive: false,
      lastLogin: '2023-12-20T14:30:00Z',
      createdAt: '2023-03-10T00:00:00Z',
      permissions: ['query', 'dashboard'],
      department: 'Finance',
      phone: '+1-555-0104'
    }
  ]

  const displayUsers = users || mockUsers

  const handleAddUser = () => {
    setEditingUser(null)
    form.resetFields()
    setIsModalOpen(true)
  }

  const handleEditUser = (user: User) => {
    setEditingUser(user)
    form.setFieldsValue(user)
    setIsModalOpen(true)
  }

  const handleDeleteUser = (userId: string) => {
    Modal.confirm({
      title: 'Delete User',
      content: 'Are you sure you want to delete this user? This action cannot be undone.',
      okText: 'Delete',
      okType: 'danger',
      onOk: async () => {
        try {
          await deleteUser(userId).unwrap()
          message.success('User deleted successfully')
          refetch()
        } catch (error) {
          message.error('Failed to delete user')
        }
      }
    })
  }

  const handleToggleStatus = async (user: User) => {
    try {
      await updateUser({
        id: user.id,
        isActive: !user.isActive
      }).unwrap()
      message.success(`User ${user.isActive ? 'deactivated' : 'activated'} successfully`)
      refetch()
    } catch (error) {
      message.error('Failed to update user status')
    }
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()

      if (editingUser) {
        await updateUser({
          id: editingUser.id,
          ...values
        }).unwrap()
        message.success('User updated successfully')
      } else {
        await createUser(values).unwrap()
        message.success('User created successfully')
      }

      setIsModalOpen(false)
      refetch()
    } catch (error: any) {
      message.error(error.data?.message || 'Failed to save user')
    }
  }

  const getRoleIcon = (role: string) => {
    switch (role) {
      case 'Admin': return <CrownOutlined style={{ color: '#ff4d4f' }} />
      case 'Analyst': return <TeamOutlined style={{ color: '#1890ff' }} />
      case 'Viewer': return <UserOutlined style={{ color: '#52c41a' }} />
      default: return <UserOutlined />
    }
  }

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'Admin': return 'red'
      case 'Analyst': return 'blue'
      case 'Viewer': return 'green'
      default: return 'default'
    }
  }

  const columns = [
    {
      title: 'User',
      key: 'user',
      render: (record: User) => (
        <Space>
          <Avatar
            src={record.avatar}
            icon={<UserOutlined />}
            style={{ backgroundColor: record.isActive ? '#1890ff' : '#d9d9d9' }}
          />
          <div>
            <div>
              <Text strong>{record.firstName} {record.lastName}</Text>
              {!record.isActive && <Tag color="red" style={{ marginLeft: 8 }}>Inactive</Tag>}
            </div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              @{record.username}
            </Text>
          </div>
        </Space>
      ),
    },
    {
      title: 'Contact',
      key: 'contact',
      render: (record: User) => (
        <div>
          <div style={{ marginBottom: 4 }}>
            <MailOutlined style={{ marginRight: 8, color: '#1890ff' }} />
            <Text>{record.email}</Text>
          </div>
          {record.phone && (
            <div>
              <PhoneOutlined style={{ marginRight: 8, color: '#52c41a' }} />
              <Text>{record.phone}</Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Role & Department',
      key: 'role',
      render: (record: User) => (
        <div>
          <div style={{ marginBottom: 4 }}>
            {getRoleIcon(record.role)}
            <Tag color={getRoleColor(record.role)} style={{ marginLeft: 8 }}>
              {record.role}
            </Tag>
          </div>
          {record.department && (
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.department}
            </Text>
          )}
        </div>
      ),
    },
    {
      title: 'Last Login',
      dataIndex: 'lastLogin',
      key: 'lastLogin',
      render: (date: string) => (
        <div>
          <div>{new Date(date).toLocaleDateString()}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {new Date(date).toLocaleTimeString()}
          </Text>
        </div>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      render: (record: User) => (
        <div>
          <Tag color={record.isActive ? 'green' : 'red'}>
            {record.isActive ? 'Active' : 'Inactive'}
          </Tag>
          <div style={{ marginTop: 4 }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.permissions.length} permissions
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: User) => (
        <Dropdown
          menu={{
            items: [
              {
                key: 'edit',
                label: 'Edit User',
                icon: <EditOutlined />,
                onClick: () => handleEditUser(record),
              },
              {
                key: 'toggle',
                label: record.isActive ? 'Deactivate' : 'Activate',
                icon: record.isActive ? <LockOutlined /> : <UnlockOutlined />,
                onClick: () => handleToggleStatus(record),
              },
              {
                type: 'divider',
              },
              {
                key: 'delete',
                label: 'Delete User',
                icon: <DeleteOutlined />,
                danger: true,
                onClick: () => handleDeleteUser(record.id),
              },
            ],
          }}
          trigger={['click']}
        >
          <Button icon={<MoreOutlined />} />
        </Dropdown>
      ),
    },
  ]

  const filteredUsers = displayUsers.filter(user => {
    const matchesSearch = !searchText ||
      user.firstName.toLowerCase().includes(searchText.toLowerCase()) ||
      user.lastName.toLowerCase().includes(searchText.toLowerCase()) ||
      user.email.toLowerCase().includes(searchText.toLowerCase()) ||
      user.username.toLowerCase().includes(searchText.toLowerCase())

    const matchesRole = !roleFilter || user.role === roleFilter
    const matchesStatus = !statusFilter ||
      (statusFilter === 'active' && user.isActive) ||
      (statusFilter === 'inactive' && !user.isActive)

    return matchesSearch && matchesRole && matchesStatus
  })

  const stats = {
    total: displayUsers.length,
    active: displayUsers.filter(u => u.isActive).length,
    admins: displayUsers.filter(u => u.role === 'Admin').length,
    analysts: displayUsers.filter(u => u.role === 'Analyst').length,
    viewers: displayUsers.filter(u => u.role === 'Viewer').length,
  }

  return (
    <PageLayout
      title="User Management"
      subtitle="Manage users, roles, and permissions"
    >
      {/* Statistics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Users"
              value={stats.total}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Users"
              value={stats.active}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
            <Progress
              percent={Math.round((stats.active / stats.total) * 100)}
              size="small"
              showInfo={false}
              strokeColor="#52c41a"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Administrators"
              value={stats.admins}
              prefix={<CrownOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Analysts"
              value={stats.analysts}
              prefix={<SafetyOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Main Content */}
      <Card>
        {/* Header with Search and Filters */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 16,
          flexWrap: 'wrap',
          gap: 16
        }}>
          <Space wrap>
            <Search
              placeholder="Search users..."
              allowClear
              style={{ width: 250 }}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              prefix={<SearchOutlined />}
            />

            <Select
              placeholder="Filter by role"
              allowClear
              style={{ width: 150 }}
              value={roleFilter}
              onChange={setRoleFilter}
              suffixIcon={<FilterOutlined />}
            >
              <Option value="Admin">Admin</Option>
              <Option value="Analyst">Analyst</Option>
              <Option value="Viewer">Viewer</Option>
            </Select>

            <Select
              placeholder="Filter by status"
              allowClear
              style={{ width: 150 }}
              value={statusFilter}
              onChange={setStatusFilter}
            >
              <Option value="active">Active</Option>
              <Option value="inactive">Inactive</Option>
            </Select>
          </Space>

          <Space>
            <Button icon={<ExportOutlined />}>
              Export
            </Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleAddUser}
            >
              Add User
            </Button>
          </Space>
        </div>

        {/* Users Table */}
        <Table
          columns={columns}
          dataSource={filteredUsers}
          rowKey="id"
          loading={isLoading}
          pagination={{
            total: filteredUsers.length,
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} users`,
          }}
          scroll={{ x: 800 }}
        />
      </Card>

      {/* User Form Modal */}
      <Modal
        title={editingUser ? 'Edit User' : 'Add New User'}
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        width={600}
        footer={[
          <Button key="cancel" onClick={() => setIsModalOpen(false)}>
            Cancel
          </Button>,
          <Button
            key="submit"
            type="primary"
            loading={isCreating || isUpdating}
            onClick={handleSubmit}
          >
            {editingUser ? 'Update' : 'Create'}
          </Button>,
        ]}
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="firstName"
                label="First Name"
                rules={[{ required: true, message: 'First name is required' }]}
              >
                <Input placeholder="Enter first name" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="lastName"
                label="Last Name"
                rules={[{ required: true, message: 'Last name is required' }]}
              >
                <Input placeholder="Enter last name" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="username"
                label="Username"
                rules={[
                  { required: true, message: 'Username is required' },
                  { min: 3, message: 'Username must be at least 3 characters' }
                ]}
              >
                <Input placeholder="Enter username" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="email"
                label="Email"
                rules={[
                  { required: true, message: 'Email is required' },
                  { type: 'email', message: 'Please enter a valid email' }
                ]}
              >
                <Input placeholder="Enter email address" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="role"
                label="Role"
                rules={[{ required: true, message: 'Role is required' }]}
              >
                <Select placeholder="Select role">
                  <Option value="Admin">
                    <Space>
                      <CrownOutlined style={{ color: '#ff4d4f' }} />
                      Administrator
                    </Space>
                  </Option>
                  <Option value="Analyst">
                    <Space>
                      <TeamOutlined style={{ color: '#1890ff' }} />
                      Analyst
                    </Space>
                  </Option>
                  <Option value="Viewer">
                    <Space>
                      <UserOutlined style={{ color: '#52c41a' }} />
                      Viewer
                    </Space>
                  </Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="department"
                label="Department"
              >
                <Select placeholder="Select department">
                  <Option value="IT">IT</Option>
                  <Option value="Sales">Sales</Option>
                  <Option value="Marketing">Marketing</Option>
                  <Option value="Finance">Finance</Option>
                  <Option value="HR">Human Resources</Option>
                  <Option value="Operations">Operations</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="phone"
            label="Phone Number"
          >
            <Input placeholder="Enter phone number" />
          </Form.Item>

          <Form.Item
            name="isActive"
            label="Account Status"
            valuePropName="checked"
            initialValue={true}
          >
            <Switch
              checkedChildren="Active"
              unCheckedChildren="Inactive"
            />
          </Form.Item>
        </Form>
      </Modal>
    </PageLayout>
  )
}
