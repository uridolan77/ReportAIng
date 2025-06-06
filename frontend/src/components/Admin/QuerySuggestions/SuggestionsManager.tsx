/**
 * Suggestions Manager Component
 * Manages individual query suggestions
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
  Tooltip,
  Row,
  Col,
  Statistic
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  FileTextOutlined,
  EyeOutlined,
  EyeInvisibleOutlined,
  BarChartOutlined,
  ClockCircleOutlined
} from '@ant-design/icons';
import DataTable from '../../DataTable';
import { querySuggestionService, QuerySuggestion, SuggestionCategory, CreateUpdateSuggestion } from '../../../services/querySuggestionService';

const { TextArea } = Input;
const { Option } = Select;

interface SuggestionsManagerProps {
  suggestions: QuerySuggestion[];
  categories: SuggestionCategory[];
  onSuggestionsChange: (suggestions: QuerySuggestion[]) => void;
  onRefresh: () => void;
}

interface SuggestionFormData {
  categoryId: number;
  queryText: string;
  description: string;
  defaultTimeFrame?: string;
  sortOrder: number;
  isActive: boolean;
  targetTables: string[];
  complexity: number;
  requiredPermissions: string[];
  tags: string[];
}

const timeFrameOptions = [
  { value: 'today', label: 'Today' },
  { value: 'yesterday', label: 'Yesterday' },
  { value: 'last_7_days', label: 'Last 7 Days' },
  { value: 'last_30_days', label: 'Last 30 Days' },
  { value: 'this_week', label: 'This Week' },
  { value: 'this_month', label: 'This Month' },
  { value: 'this_quarter', label: 'This Quarter' },
  { value: 'this_year', label: 'This Year' }
];

const complexityLabels = {
  1: 'Simple',
  2: 'Medium',
  3: 'Complex',
  4: 'Advanced',
  5: 'Expert'
};

export const SuggestionsManager: React.FC<SuggestionsManagerProps> = ({
  suggestions,
  categories,
  onSuggestionsChange,
  onRefresh
}) => {
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingSuggestion, setEditingSuggestion] = useState<QuerySuggestion | null>(null);
  const [loading, setLoading] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [form] = Form.useForm<SuggestionFormData>();

  // Filter suggestions by category
  const filteredSuggestions = selectedCategory 
    ? suggestions.filter(s => s.categoryKey === selectedCategory)
    : suggestions;

  const columns = [
    {
      title: 'Suggestion',
      dataIndex: 'description',
      key: 'description',
      sortable: true,
      render: (description: string, record: QuerySuggestion) => (
        <div>
          <div style={{ fontWeight: 'bold', marginBottom: '4px' }}>{description}</div>
          <div style={{
            fontSize: '12px',
            color: '#666',
            fontFamily: 'monospace',
            background: '#f5f5f5',
            padding: '4px 8px',
            borderRadius: '4px',
            maxWidth: '300px',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap'
          }}>
            {record.queryText}
          </div>
        </div>
      ),
    },
    {
      title: 'Category',
      dataIndex: 'categoryTitle',
      key: 'categoryTitle',
      width: 150,
      sortable: true,
      render: (categoryTitle: string) => (
        <Tag color="blue">{categoryTitle}</Tag>
      )
    },
    {
      title: 'Complexity',
      dataIndex: 'complexity',
      key: 'complexity',
      width: 100,
      sortable: true,
      render: (complexity: number) => (
        <Tag color={
          complexity <= 2 ? 'green' :
          complexity <= 3 ? 'orange' :
          'red'
        }>
          {complexityLabels[complexity as keyof typeof complexityLabels] || complexity}
        </Tag>
      )
    },
    {
      title: 'Usage',
      dataIndex: 'usageCount',
      key: 'usageCount',
      width: 100,
      sortable: true,
      render: (usageCount: number) => (
        <Statistic
          value={usageCount}
          prefix={<BarChartOutlined />}
          valueStyle={{ fontSize: '14px' }}
        />
      )
    },
    {
      title: 'Last Used',
      dataIndex: 'lastUsed',
      key: 'lastUsed',
      width: 120,
      sortable: true,
      render: (lastUsed: string) => lastUsed ? (
        <Tooltip title={new Date(lastUsed).toLocaleString()}>
          <Tag icon={<ClockCircleOutlined />}>
            {new Date(lastUsed).toLocaleDateString()}
          </Tag>
        </Tooltip>
      ) : (
        <Tag color="default">Never</Tag>
      )
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
      render: (_: any, record: QuerySuggestion) => (
        <Space>
          <Tooltip title="Edit Suggestion">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
              size="small"
            />
          </Tooltip>
          <Popconfirm
            title="Delete Suggestion"
            description="Are you sure you want to delete this suggestion?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Tooltip title="Delete Suggestion">
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
    setEditingSuggestion(null);
    form.resetFields();
    form.setFieldsValue({
      sortOrder: Math.max(...suggestions.map(s => s.sortOrder), 0) + 10,
      isActive: true,
      complexity: 1,
      targetTables: [],
      requiredPermissions: [],
      tags: []
    });
    setIsModalVisible(true);
  };

  const handleEdit = (suggestion: QuerySuggestion) => {
    setEditingSuggestion(suggestion);
    form.setFieldsValue({
      categoryId: suggestion.categoryId,
      queryText: suggestion.queryText,
      description: suggestion.description,
      defaultTimeFrame: suggestion.defaultTimeFrame,
      sortOrder: suggestion.sortOrder,
      isActive: suggestion.isActive,
      targetTables: suggestion.targetTables,
      complexity: suggestion.complexity,
      requiredPermissions: suggestion.requiredPermissions,
      tags: suggestion.tags
    });
    setIsModalVisible(true);
  };

  const handleDelete = async (suggestionId: number) => {
    try {
      setLoading(true);
      await querySuggestionService.deleteSuggestion(suggestionId);
      message.success('Suggestion deleted successfully');
      onRefresh();
    } catch (error) {
      console.error('Error deleting suggestion:', error);
      message.error('Failed to delete suggestion');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: SuggestionFormData) => {
    try {
      setLoading(true);
      
      const suggestionData: CreateUpdateSuggestion = {
        categoryId: values.categoryId,
        queryText: values.queryText,
        description: values.description,
        defaultTimeFrame: values.defaultTimeFrame,
        sortOrder: values.sortOrder,
        isActive: values.isActive,
        targetTables: values.targetTables,
        complexity: values.complexity,
        requiredPermissions: values.requiredPermissions,
        tags: values.tags
      };

      if (editingSuggestion) {
        await querySuggestionService.updateSuggestion(editingSuggestion.id, suggestionData);
        message.success('Suggestion updated successfully');
      } else {
        await querySuggestionService.createSuggestion(suggestionData);
        message.success('Suggestion created successfully');
      }

      setIsModalVisible(false);
      onRefresh();
    } catch (error) {
      console.error('Error saving suggestion:', error);
      message.error('Failed to save suggestion');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      {/* Stats Row */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={8}>
          <Card size="small">
            <Statistic
              title="Total Suggestions"
              value={suggestions.length}
              prefix={<FileTextOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card size="small">
            <Statistic
              title="Active Suggestions"
              value={suggestions.filter(s => s.isActive).length}
              prefix={<EyeOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card size="small">
            <Statistic
              title="Total Usage"
              value={suggestions.reduce((sum, s) => sum + s.usageCount, 0)}
              prefix={<BarChartOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card
        title={
          <Space>
            <FileTextOutlined />
            <span>Query Suggestions</span>
          </Space>
        }
        extra={
          <Space>
            <Select
              placeholder="Filter by category"
              style={{ width: 200 }}
              allowClear
              value={selectedCategory || undefined}
              onChange={setSelectedCategory}
            >
              {categories.map(category => (
                <Option key={category.categoryKey} value={category.categoryKey}>
                  {category.title}
                </Option>
              ))}
            </Select>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleCreate}
            >
              Add Suggestion
            </Button>
          </Space>
        }
      >
        <DataTable
          data={filteredSuggestions}
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
        title={editingSuggestion ? 'Edit Suggestion' : 'Create Suggestion'}
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        onOk={() => form.submit()}
        confirmLoading={loading}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="categoryId"
                label="Category"
                rules={[{ required: true, message: 'Please select a category' }]}
              >
                <Select placeholder="Select category">
                  {categories.map(category => (
                    <Option key={category.id} value={category.id}>
                      {category.title}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="complexity"
                label="Complexity"
                rules={[{ required: true, message: 'Please select complexity' }]}
              >
                <Select placeholder="Select complexity">
                  {Object.entries(complexityLabels).map(([value, label]) => (
                    <Option key={value} value={parseInt(value)}>
                      {label}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="description"
            label="Description"
            rules={[{ required: true, message: 'Please enter description' }]}
          >
            <Input placeholder="Brief description of what this query does" />
          </Form.Item>

          <Form.Item
            name="queryText"
            label="Query Text"
            rules={[{ required: true, message: 'Please enter query text' }]}
          >
            <TextArea
              rows={4}
              placeholder="Natural language query text..."
            />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="defaultTimeFrame"
                label="Default Time Frame"
              >
                <Select placeholder="Select default time frame" allowClear>
                  {timeFrameOptions.map(option => (
                    <Option key={option.value} value={option.value}>
                      {option.label}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="sortOrder"
                label="Sort Order"
                rules={[{ required: true, message: 'Please enter sort order' }]}
              >
                <InputNumber min={0} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="targetTables"
            label="Target Tables"
          >
            <Select
              mode="tags"
              placeholder="Enter target table names"
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            name="tags"
            label="Tags"
          >
            <Select
              mode="tags"
              placeholder="Enter tags for categorization"
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            name="requiredPermissions"
            label="Required Permissions"
          >
            <Select
              mode="tags"
              placeholder="Enter required permissions"
              style={{ width: '100%' }}
            />
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
