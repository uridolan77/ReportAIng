import React, { useState } from 'react'
import { 
  Card, 
  Button, 
  Table, 
  Tag, 
  Space, 
  Modal, 
  Form, 
  Input, 
  Select, 
  InputNumber, 
  DatePicker, 
  message,
  Popconfirm,
  Progress,
  Typography
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import { 
  useGetBudgetsQuery,
  useCreateBudgetMutation,
  useUpdateBudgetMutation,
  useDeleteBudgetMutation
} from '../../store/api/costApi'
import type { BudgetManagement, CreateBudgetRequest, BudgetPeriod } from '../../types/cost'
import dayjs from 'dayjs'

const { Title } = Typography
const { RangePicker } = DatePicker

interface BudgetFormData extends Omit<CreateBudgetRequest, 'startDate' | 'endDate'> {
  dateRange: [dayjs.Dayjs, dayjs.Dayjs]
}

export const BudgetManagementComponent: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingBudget, setEditingBudget] = useState<BudgetManagement | null>(null)
  const [form] = Form.useForm<BudgetFormData>()

  const { data: budgetsData, isLoading } = useGetBudgetsQuery()
  const [createBudget, { isLoading: isCreating }] = useCreateBudgetMutation()
  const [updateBudget, { isLoading: isUpdating }] = useUpdateBudgetMutation()
  const [deleteBudget] = useDeleteBudgetMutation()

  const handleCreate = () => {
    setEditingBudget(null)
    form.resetFields()
    setIsModalOpen(true)
  }

  const handleEdit = (budget: BudgetManagement) => {
    setEditingBudget(budget)
    form.setFieldsValue({
      name: budget.name,
      type: budget.type,
      entityId: budget.entityId,
      budgetAmount: budget.budgetAmount,
      period: budget.period,
      alertThreshold: budget.alertThreshold,
      blockThreshold: budget.blockThreshold,
      dateRange: [dayjs(budget.startDate), dayjs(budget.endDate)]
    })
    setIsModalOpen(true)
  }

  const handleDelete = async (budgetId: string) => {
    try {
      await deleteBudget(budgetId).unwrap()
      message.success('Budget deleted successfully')
    } catch (error) {
      message.error('Failed to delete budget')
    }
  }

  const handleSubmit = async (values: BudgetFormData) => {
    try {
      const budgetData: CreateBudgetRequest = {
        ...values,
        startDate: values.dateRange[0].toISOString(),
        endDate: values.dateRange[1].toISOString()
      }

      if (editingBudget) {
        await updateBudget({ 
          budgetId: editingBudget.id, 
          budget: budgetData 
        }).unwrap()
        message.success('Budget updated successfully')
      } else {
        await createBudget(budgetData).unwrap()
        message.success('Budget created successfully')
      }
      
      setIsModalOpen(false)
      form.resetFields()
      setEditingBudget(null)
    } catch (error) {
      message.error('Failed to save budget')
    }
  }

  const getBudgetStatus = (budget: BudgetManagement) => {
    const usagePercentage = (budget.spentAmount / budget.budgetAmount) * 100
    
    if (usagePercentage >= budget.blockThreshold) {
      return { status: 'Over Budget', color: 'red' }
    } else if (usagePercentage >= budget.alertThreshold) {
      return { status: 'Near Limit', color: 'orange' }
    } else {
      return { status: 'On Track', color: 'green' }
    }
  }

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: BudgetManagement) => (
        <div>
          <div style={{ fontWeight: 'bold' }}>{text}</div>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {record.type} • {record.period}
          </div>
        </div>
      )
    },
    {
      title: 'Budget Amount',
      dataIndex: 'budgetAmount',
      key: 'budgetAmount',
      render: (amount: number) => `$${amount.toLocaleString(undefined, { minimumFractionDigits: 2 })}`
    },
    {
      title: 'Usage',
      key: 'usage',
      render: (record: BudgetManagement) => {
        const usagePercentage = (record.spentAmount / record.budgetAmount) * 100
        const { status, color } = getBudgetStatus(record)
        
        return (
          <div>
            <Progress 
              percent={Math.min(usagePercentage, 100)} 
              size="small"
              status={usagePercentage >= record.blockThreshold ? 'exception' : 
                     usagePercentage >= record.alertThreshold ? 'active' : 'success'}
            />
            <div style={{ fontSize: '12px', marginTop: '4px' }}>
              ${record.spentAmount.toLocaleString()} / ${record.budgetAmount.toLocaleString()}
            </div>
          </div>
        )
      }
    },
    {
      title: 'Status',
      key: 'status',
      render: (record: BudgetManagement) => {
        const { status, color } = getBudgetStatus(record)
        return (
          <Space direction="vertical" size="small">
            <Tag color={color}>{status}</Tag>
            {!record.isActive && <Tag color="default">Inactive</Tag>}
          </Space>
        )
      }
    },
    {
      title: 'Period',
      key: 'period',
      render: (record: BudgetManagement) => (
        <div style={{ fontSize: '12px' }}>
          <div>{dayjs(record.startDate).format('MMM DD, YYYY')}</div>
          <div>{dayjs(record.endDate).format('MMM DD, YYYY')}</div>
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: BudgetManagement) => (
        <Space>
          <Button 
            type="link" 
            size="small" 
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Delete Budget"
            description="Are you sure you want to delete this budget?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
            icon={<ExclamationCircleOutlined style={{ color: 'red' }} />}
          >
            <Button 
              type="link" 
              size="small" 
              danger
              icon={<DeleteOutlined />}
            >
              Delete
            </Button>
          </Popconfirm>
        </Space>
      )
    }
  ]

  return (
    <Card>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <Title level={4} style={{ margin: 0 }}>Budget Management</Title>
        <Button 
          type="primary" 
          icon={<PlusOutlined />}
          onClick={handleCreate}
        >
          Create Budget
        </Button>
      </div>

      <Table
        columns={columns}
        dataSource={budgetsData?.budgets || []}
        rowKey="id"
        loading={isLoading}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total) => `Total ${total} budgets`
        }}
      />

      <Modal
        title={editingBudget ? 'Edit Budget' : 'Create Budget'}
        open={isModalOpen}
        onCancel={() => {
          setIsModalOpen(false)
          form.resetFields()
          setEditingBudget(null)
        }}
        footer={null}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            name="name"
            label="Budget Name"
            rules={[{ required: true, message: 'Please enter budget name' }]}
          >
            <Input placeholder="Enter budget name" />
          </Form.Item>

          <Form.Item
            name="type"
            label="Budget Type"
            rules={[{ required: true, message: 'Please select budget type' }]}
          >
            <Select placeholder="Select budget type">
              <Select.Option value="user">User</Select.Option>
              <Select.Option value="department">Department</Select.Option>
              <Select.Option value="project">Project</Select.Option>
              <Select.Option value="global">Global</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="entityId"
            label="Entity ID"
            rules={[{ required: true, message: 'Please enter entity ID' }]}
          >
            <Input placeholder="Enter entity ID" />
          </Form.Item>

          <Form.Item
            name="budgetAmount"
            label="Budget Amount"
            rules={[{ required: true, message: 'Please enter budget amount' }]}
          >
            <InputNumber
              style={{ width: '100%' }}
              min={0}
              precision={2}
              formatter={(value) => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
              parser={(value) => value!.replace(/\$\s?|(,*)/g, '')}
              placeholder="Enter budget amount"
            />
          </Form.Item>

          <Form.Item
            name="period"
            label="Budget Period"
            rules={[{ required: true, message: 'Please select budget period' }]}
          >
            <Select placeholder="Select budget period">
              <Select.Option value="Daily">Daily</Select.Option>
              <Select.Option value="Weekly">Weekly</Select.Option>
              <Select.Option value="Monthly">Monthly</Select.Option>
              <Select.Option value="Quarterly">Quarterly</Select.Option>
              <Select.Option value="Yearly">Yearly</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="dateRange"
            label="Budget Period"
            rules={[{ required: true, message: 'Please select date range' }]}
          >
            <RangePicker style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            name="alertThreshold"
            label="Alert Threshold (%)"
            rules={[{ required: true, message: 'Please enter alert threshold' }]}
          >
            <InputNumber
              style={{ width: '100%' }}
              min={0}
              max={100}
              placeholder="Enter alert threshold percentage"
            />
          </Form.Item>

          <Form.Item
            name="blockThreshold"
            label="Block Threshold (%)"
            rules={[{ required: true, message: 'Please enter block threshold' }]}
          >
            <InputNumber
              style={{ width: '100%' }}
              min={0}
              max={100}
              placeholder="Enter block threshold percentage"
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setIsModalOpen(false)}>
                Cancel
              </Button>
              <Button 
                type="primary" 
                htmlType="submit"
                loading={isCreating || isUpdating}
              >
                {editingBudget ? 'Update' : 'Create'} Budget
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  )
}
