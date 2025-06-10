/**
 * Categories Manager Component
 * Manages query suggestion categories
 */

import React, { useState } from 'react';
import {
  Card,
  Button,
  Space,
  Modal,
  Form,
  Input,
  Switch,
  InputNumber,
  Select,
  message,
  Popconfirm,
  Tag,
  Tooltip
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  AppstoreOutlined,
  EyeOutlined,
  EyeInvisibleOutlined
} from '@ant-design/icons';
import DataTable from '../DataTable';
import { querySuggestionService, SuggestionCategory, CreateUpdateCategory } from '../../services/querySuggestionService';

const { TextArea } = Input;
const { Option } = Select;

interface CategoriesManagerProps {
  categories: SuggestionCategory[];
  onCategoriesChange: (categories: SuggestionCategory[]) => void;
  onRefresh: () => void;
}

interface CategoryFormData {
  categoryKey: string;
  title: string;
  icon?: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}

const iconOptions = [
  { value: 'BarChartOutlined', label: '📊 Bar Chart' },
  { value: 'UserOutlined', label: '👤 User' },
  { value: 'DollarOutlined', label: '💰 Dollar' },
  { value: 'ClockCircleOutlined', label: '🕐 Clock' },
  { value: 'DatabaseOutlined', label: '🗄️ Database' },
  { value: 'ThunderboltOutlined', label: '⚡ Thunderbolt' },
  { value: 'TrophyOutlined', label: '🏆 Trophy' },
  { value: 'RiseOutlined', label: '📈 Rise' },
  { value: 'TeamOutlined', label: '👥 Team' },
  { value: 'SettingOutlined', label: '⚙️ Settings' }
];

export const CategoriesManager: React.FC<CategoriesManagerProps> = ({
  categories,
  onCategoriesChange,
  onRefresh
}) => {
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingCategory, setEditingCategory] = useState<SuggestionCategory | null>(null);
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm<CategoryFormData>();

  const columns = [
    {
      title: 'Category',
      dataIndex: 'title',
      key: 'title',
      sortable: true,
      render: (title: string, record: SuggestionCategory) => (
        <Space>
          <span style={{ fontSize: '16px' }}>
            {record.icon ? `${record.icon} ` : '📁 '}
          </span>
          <div>
            <div style={{ fontWeight: 'bold' }}>{title}</div>
            <div style={{ fontSize: '12px', color: '#666' }}>
              Key: {record.categoryKey}
            </div>
          </div>
        </Space>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (description: string) => description || <span style={{ color: '#ccc' }}>No description</span>
    },
    {
      title: 'Suggestions',
      dataIndex: 'suggestionCount',
      key: 'suggestionCount',
      width: 120,
      sortable: true,
      render: (count: number) => (
        <Tag color={count > 0 ? 'blue' : 'default'}>
          {count} suggestions
        </Tag>
      )
    },
    {
      title: 'Sort Order',
      dataIndex: 'sortOrder',
      key: 'sortOrder',
      width: 100,
      sortable: true
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'} icon={isActive ? <EyeOutlined /> : <EyeInvisibleOutlined />}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      )
    },
    {
      title: 'Actions',
      dataIndex: 'actions',
      key: 'actions',
      width: 150,
      render: (_: any, record: SuggestionCategory) => (
        <Space>
          <Tooltip title="Edit Category">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
              size="small"
            />
          </Tooltip>
          <Popconfirm
            title="Delete Category"
            description="Are you sure you want to delete this category? This will also delete all associated suggestions."
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Tooltip title="Delete Category">
              <Button
                type="text"
                icon={<DeleteOutlined />}
                danger
                size="small"
              />
            </Tooltip>
          </Popconfirm>
        </Space>
      )
    }
  ];

  const handleCreate = () => {
    setEditingCategory(null);
    form.resetFields();
    form.setFieldsValue({
      sortOrder: Math.max(...categories.map(c => c.sortOrder), 0) + 10,
      isActive: true
    });
    setIsModalVisible(true);
  };

  const handleEdit = (category: SuggestionCategory) => {
    setEditingCategory(category);
    form.setFieldsValue({
      categoryKey: category.categoryKey,
      title: category.title,
      icon: category.icon,
      description: category.description,
      sortOrder: category.sortOrder,
      isActive: category.isActive
    });
    setIsModalVisible(true);
  };

  const handleDelete = async (categoryId: number) => {
    try {
      setLoading(true);
      await querySuggestionService.deleteCategory(categoryId);
      message.success('Category deleted successfully');
      onRefresh();
    } catch (error) {
      console.error('Error deleting category:', error);
      message.error('Failed to delete category');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: CategoryFormData) => {
    try {
      setLoading(true);
      
      const categoryData: CreateUpdateCategory = {
        categoryKey: values.categoryKey,
        title: values.title,
        icon: values.icon,
        description: values.description,
        sortOrder: values.sortOrder,
        isActive: values.isActive
      };

      if (editingCategory) {
        await querySuggestionService.updateCategory(editingCategory.id, categoryData);
        message.success('Category updated successfully');
      } else {
        await querySuggestionService.createCategory(categoryData);
        message.success('Category created successfully');
      }

      setIsModalVisible(false);
      onRefresh();
    } catch (error) {
      console.error('Error saving category:', error);
      message.error('Failed to save category');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      <Card
        title={
          <Space>
            <AppstoreOutlined />
            <span>Suggestion Categories</span>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleCreate}
          >
            Add Category
          </Button>
        }
      >
        <DataTable
          data={categories}
          columns={columns}
          keyField="id"
          loading={loading}
          features={{
            pagination: true,
            sorting: true,
            searching: true,
            selection: false
          }}
          config={{
            pageSize: 10,
            pageSizeOptions: [10, 20, 50]
          }}
        />
      </Card>

      <Modal
        title={editingCategory ? 'Edit Category' : 'Create Category'}
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        onOk={() => form.submit()}
        confirmLoading={loading}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            name="categoryKey"
            label="Category Key"
            rules={[
              { required: true, message: 'Please enter category key' },
              { pattern: /^[a-z0-9_]+$/, message: 'Only lowercase letters, numbers, and underscores allowed' }
            ]}
          >
            <Input placeholder="e.g., financial_reports" />
          </Form.Item>

          <Form.Item
            name="title"
            label="Title"
            rules={[{ required: true, message: 'Please enter title' }]}
          >
            <Input placeholder="e.g., Financial Reports" />
          </Form.Item>

          <Form.Item
            name="icon"
            label="Icon"
          >
            <Select placeholder="Select an icon" allowClear>
              {iconOptions.map(option => (
                <Option key={option.value} value={option.value}>
                  {option.label}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="description"
            label="Description"
          >
            <TextArea
              rows={3}
              placeholder="Brief description of this category..."
            />
          </Form.Item>

          <Form.Item
            name="sortOrder"
            label="Sort Order"
            rules={[{ required: true, message: 'Please enter sort order' }]}
          >
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            name="isActive"
            label="Active"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default CategoriesManager;
