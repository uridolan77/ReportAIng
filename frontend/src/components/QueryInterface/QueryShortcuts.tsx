/**
 * Query Shortcuts Component
 * Provides quick access to predefined query shortcuts and templates
 */

import React, { useState, useEffect, useMemo } from 'react';
import {
  Card,
  Input,
  Button,
  Tag,
  Tooltip,
  Space,
  Typography,
  Divider,
  Badge,
  Modal,
  Form,
  Select,
  InputNumber,
  DatePicker,
  message,
  Tabs,
  Empty,
  Row,
  Col,
  Dropdown,
  Menu,
} from 'antd';
import {
  SearchOutlined,
  StarOutlined,
  StarFilled,
  ThunderboltOutlined,
  HistoryOutlined,
  PlusOutlined,
  FireOutlined,
  TagsOutlined,
  ClockCircleOutlined,
  UserOutlined,
  MoreOutlined,
  EditOutlined,
  DeleteOutlined,
  CopyOutlined,
} from '@ant-design/icons';
import { queryTemplateService, QueryTemplate, QueryShortcut, QuerySuggestion } from '../../services/queryTemplateService';

const { Text, Title } = Typography;
const { TabPane } = Tabs;
const { Option } = Select;

interface QueryShortcutsProps {
  onQuerySelect: (query: string) => void;
  onTemplateSelect: (template: QueryTemplate, variables: Record<string, string>) => void;
  currentQuery?: string;
}

interface TemplateModalProps {
  template: QueryTemplate | null;
  visible: boolean;
  onClose: () => void;
  onSubmit: (variables: Record<string, string>) => void;
}

const TemplateModal: React.FC<TemplateModalProps> = ({
  template,
  visible,
  onClose,
  onSubmit,
}) => {
  const [form] = Form.useForm();

  useEffect(() => {
    if (template && visible) {
      // Set default values
      const defaultValues: Record<string, any> = {};
      template.variables.forEach(variable => {
        if (variable.defaultValue) {
          defaultValues[variable.name] = variable.defaultValue;
        }
      });
      form.setFieldsValue(defaultValues);
    }
  }, [template, visible, form]);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      onSubmit(values);
      onClose();
    } catch (error) {
      console.error('Form validation failed:', error);
    }
  };

  if (!template) return null;

  return (
    <Modal
      title={
        <Space>
          <ThunderboltOutlined />
          {template.name}
        </Space>
      }
      open={visible}
      onCancel={onClose}
      onOk={handleSubmit}
      okText="Apply Template"
      width={600}
    >
      <div style={{ marginBottom: 16 }}>
        <Text type="secondary">{template.description}</Text>
      </div>

      <div style={{ marginBottom: 16 }}>
        <Space wrap>
          <Tag color="blue">{template.category}</Tag>
          <Tag color="green">{template.difficulty}</Tag>
          {template.estimatedRows && (
            <Tag icon={<ClockCircleOutlined />}>
              ~{template.estimatedRows} rows
            </Tag>
          )}
          {template.executionTime && (
            <Tag icon={<ClockCircleOutlined />}>
              {template.executionTime}
            </Tag>
          )}
        </Space>
      </div>

      <Divider />

      <Form form={form} layout="vertical">
        {template.variables.map(variable => (
          <Form.Item
            key={variable.name}
            name={variable.name}
            label={variable.description}
            rules={[
              {
                required: variable.required,
                message: `${variable.description} is required`,
              },
            ]}
          >
            {variable.type === 'select' ? (
              <Select placeholder={`Select ${variable.description.toLowerCase()}`}>
                {variable.options?.map(option => (
                  <Option key={option} value={option}>
                    {option}
                  </Option>
                ))}
              </Select>
            ) : variable.type === 'number' ? (
              <InputNumber
                style={{ width: '100%' }}
                placeholder={`Enter ${variable.description.toLowerCase()}`}
              />
            ) : variable.type === 'date' ? (
              <DatePicker
                style={{ width: '100%' }}
                placeholder={`Select ${variable.description.toLowerCase()}`}
              />
            ) : (
              <Input
                placeholder={`Enter ${variable.description.toLowerCase()}`}
              />
            )}
          </Form.Item>
        ))}
      </Form>

      <div style={{ marginTop: 16, padding: 12, backgroundColor: '#f5f5f5', borderRadius: 6 }}>
        <Text strong>Template Preview:</Text>
        <div style={{ marginTop: 8, fontFamily: 'monospace', fontSize: '12px' }}>
          {template.template}
        </div>
      </div>
    </Modal>
  );
};

export const QueryShortcuts: React.FC<QueryShortcutsProps> = ({
  onQuerySelect,
  onTemplateSelect,
  currentQuery = '',
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [suggestions, setSuggestions] = useState<QuerySuggestion[]>([]);
  const [templates, setTemplates] = useState<QueryTemplate[]>([]);
  const [shortcuts, setShortcuts] = useState<QueryShortcut[]>([]);
  const [selectedTemplate, setSelectedTemplate] = useState<QueryTemplate | null>(null);
  const [templateModalVisible, setTemplateModalVisible] = useState(false);
  const [activeTab, setActiveTab] = useState('suggestions');

  // Load data
  useEffect(() => {
    setTemplates(queryTemplateService.getTemplates());
    setShortcuts(queryTemplateService.getShortcuts());
  }, []);

  // Update suggestions when search term changes
  useEffect(() => {
    const newSuggestions = queryTemplateService.searchSuggestions(searchTerm);
    setSuggestions(newSuggestions);
  }, [searchTerm]);

  // Filter templates by category
  const filteredTemplates = useMemo(() => {
    if (selectedCategory === 'all') {
      return templates;
    }
    if (selectedCategory === 'favorites') {
      return queryTemplateService.getFavoriteTemplates();
    }
    return templates.filter(t => t.category === selectedCategory);
  }, [templates, selectedCategory]);

  const categories = useMemo(() => {
    return queryTemplateService.getCategories();
  }, []);

  const popularTemplates = useMemo(() => {
    return queryTemplateService.getPopularTemplates();
  }, []);

  const handleSuggestionClick = (suggestion: QuerySuggestion) => {
    if (suggestion.type === 'template') {
      const template = templates.find(t => t.id === suggestion.id);
      if (template) {
        if (template.variables.length > 0) {
          setSelectedTemplate(template);
          setTemplateModalVisible(true);
        } else {
          onQuerySelect(template.template);
          queryTemplateService.incrementUsage(template.id, 'template');
        }
      }
    } else {
      onQuerySelect(suggestion.text);
      if (suggestion.type === 'shortcut') {
        queryTemplateService.incrementUsage(suggestion.id, 'shortcut');
      }
    }

    queryTemplateService.addToRecent(suggestion.text);
    setSearchTerm('');
  };

  const handleTemplateClick = (template: QueryTemplate) => {
    if (template.variables.length > 0) {
      setSelectedTemplate(template);
      setTemplateModalVisible(true);
    } else {
      onQuerySelect(template.template);
      queryTemplateService.incrementUsage(template.id, 'template');
      queryTemplateService.addToRecent(template.template);
    }
  };

  const handleTemplateSubmit = (variables: Record<string, string>) => {
    if (selectedTemplate) {
      const processedQuery = queryTemplateService.processTemplate(selectedTemplate, variables);
      onTemplateSelect(selectedTemplate, variables);
      onQuerySelect(processedQuery);
      queryTemplateService.incrementUsage(selectedTemplate.id, 'template');
      queryTemplateService.addToRecent(processedQuery);
      message.success('Template applied successfully!');
    }
  };

  const handleFavoriteToggle = (templateId: string) => {
    queryTemplateService.toggleFavorite(templateId);
    setTemplates(queryTemplateService.getTemplates());
  };

  const getSuggestionIcon = (type: string) => {
    switch (type) {
      case 'template':
        return <ThunderboltOutlined />;
      case 'shortcut':
        return <TagsOutlined />;
      case 'recent':
        return <HistoryOutlined />;
      default:
        return <SearchOutlined />;
    }
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'beginner':
        return 'green';
      case 'intermediate':
        return 'orange';
      case 'advanced':
        return 'red';
      default:
        return 'default';
    }
  };

  return (
    <Card
      title={
        <Space>
          <ThunderboltOutlined />
          Query Shortcuts & Templates
        </Space>
      }
      size="small"
      style={{ height: '100%' }}
    >
      {/* Search Input */}
      <div style={{ marginBottom: 16 }}>
        <Input
          placeholder="Search templates, shortcuts, or type a shortcut..."
          prefix={<SearchOutlined />}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          allowClear
        />
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        size="small"
        tabBarExtraContent={
          <Dropdown
            overlay={
              <Menu>
                <Menu.Item key="create-template" icon={<PlusOutlined />}>
                  Create Template
                </Menu.Item>
                <Menu.Item key="create-shortcut" icon={<TagsOutlined />}>
                  Create Shortcut
                </Menu.Item>
              </Menu>
            }
            trigger={['click']}
          >
            <Button size="small" icon={<PlusOutlined />}>
              Create
            </Button>
          </Dropdown>
        }
      >
        {/* Suggestions Tab */}
        <TabPane
          tab={
            <Space>
              <SearchOutlined />
              Suggestions
              {suggestions.length > 0 && <Badge count={suggestions.length} />}
            </Space>
          }
          key="suggestions"
        >
          {suggestions.length > 0 ? (
            <div style={{ maxHeight: 300, overflowY: 'auto' }}>
              {suggestions.map((suggestion, index) => (
                <Card
                  key={`${suggestion.id}_${index}`}
                  size="small"
                  hoverable
                  style={{ marginBottom: 8, cursor: 'pointer' }}
                  onClick={() => handleSuggestionClick(suggestion)}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Space>
                      {getSuggestionIcon(suggestion.type)}
                      <Text strong>{suggestion.description}</Text>
                      <Tag color="blue">{suggestion.type}</Tag>
                      <Tag>{suggestion.category}</Tag>
                    </Space>
                    <Text
                      type="secondary"
                      style={{
                        fontSize: '12px',
                        fontFamily: 'monospace',
                        wordBreak: 'break-all',
                      }}
                    >
                      {suggestion.text}
                    </Text>
                  </Space>
                </Card>
              ))}
            </div>
          ) : (
            <Empty
              description="Start typing to see suggestions"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          )}
        </TabPane>

        {/* Templates Tab */}
        <TabPane
          tab={
            <Space>
              <ThunderboltOutlined />
              Templates
              <Badge count={filteredTemplates.length} />
            </Space>
          }
          key="templates"
        >
          {/* Category Filter */}
          <div style={{ marginBottom: 16 }}>
            <Select
              value={selectedCategory}
              onChange={setSelectedCategory}
              style={{ width: '100%' }}
              placeholder="Select category"
            >
              <Option value="all">All Categories</Option>
              <Option value="favorites">
                <Space>
                  <StarFilled style={{ color: '#faad14' }} />
                  Favorites
                </Space>
              </Option>
              {categories.map(category => (
                <Option key={category} value={category}>
                  {category.charAt(0).toUpperCase() + category.slice(1)}
                </Option>
              ))}
            </Select>
          </div>

          {/* Templates List */}
          <div style={{ maxHeight: 400, overflowY: 'auto' }}>
            <Row gutter={[8, 8]}>
              {filteredTemplates.map(template => (
                <Col span={24} key={template.id}>
                  <Card
                    size="small"
                    hoverable
                    style={{ cursor: 'pointer' }}
                    onClick={() => handleTemplateClick(template)}
                    actions={[
                      <Tooltip title={template.isFavorite ? 'Remove from favorites' : 'Add to favorites'}>
                        <Button
                          type="text"
                          icon={template.isFavorite ? <StarFilled style={{ color: '#faad14' }} /> : <StarOutlined />}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleFavoriteToggle(template.id);
                          }}
                        />
                      </Tooltip>,
                      <Tooltip title="Usage count">
                        <Space>
                          <UserOutlined />
                          {template.usageCount}
                        </Space>
                      </Tooltip>,
                    ]}
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Space>
                        <Text strong>{template.name}</Text>
                        <Tag color={getDifficultyColor(template.difficulty)}>
                          {template.difficulty}
                        </Tag>
                      </Space>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {template.description}
                      </Text>
                      <Space wrap>
                        <Tag color="blue">{template.category}</Tag>
                        {template.variables.length > 0 && (
                          <Tag color="orange">{template.variables.length} variables</Tag>
                        )}
                        {template.estimatedRows && (
                          <Tag>{template.estimatedRows} rows</Tag>
                        )}
                      </Space>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
          </div>
        </TabPane>

        {/* Shortcuts Tab */}
        <TabPane
          tab={
            <Space>
              <TagsOutlined />
              Shortcuts
              <Badge count={shortcuts.length} />
            </Space>
          }
          key="shortcuts"
        >
          <div style={{ maxHeight: 400, overflowY: 'auto' }}>
            {shortcuts.map(shortcut => (
              <Card
                key={shortcut.id}
                size="small"
                hoverable
                style={{ marginBottom: 8, cursor: 'pointer' }}
                onClick={() => {
                  onQuerySelect(shortcut.query);
                  queryTemplateService.incrementUsage(shortcut.id, 'shortcut');
                  queryTemplateService.addToRecent(shortcut.query);
                }}
              >
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Space>
                    <Tag color="purple">{shortcut.shortcut}</Tag>
                    <Text strong>{shortcut.name}</Text>
                    <Badge count={shortcut.usageCount} />
                  </Space>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {shortcut.description}
                  </Text>
                  <Text
                    type="secondary"
                    style={{
                      fontSize: '11px',
                      fontFamily: 'monospace',
                      wordBreak: 'break-all',
                    }}
                  >
                    {shortcut.query}
                  </Text>
                </Space>
              </Card>
            ))}
          </div>
        </TabPane>

        {/* Popular Tab */}
        <TabPane
          tab={
            <Space>
              <FireOutlined />
              Popular
              <Badge count={popularTemplates.length} />
            </Space>
          }
          key="popular"
        >
          <div style={{ maxHeight: 400, overflowY: 'auto' }}>
            {popularTemplates.length > 0 ? (
              popularTemplates.map(template => (
                <Card
                  key={template.id}
                  size="small"
                  hoverable
                  style={{ marginBottom: 8, cursor: 'pointer' }}
                  onClick={() => handleTemplateClick(template)}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Space>
                      <Text strong>{template.name}</Text>
                      <Badge count={template.usageCount} />
                      <Tag color="red">Popular</Tag>
                    </Space>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {template.description}
                    </Text>
                  </Space>
                </Card>
              ))
            ) : (
              <Empty
                description="No popular templates yet"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </div>
        </TabPane>
      </Tabs>

      {/* Template Modal */}
      <TemplateModal
        template={selectedTemplate}
        visible={templateModalVisible}
        onClose={() => {
          setTemplateModalVisible(false);
          setSelectedTemplate(null);
        }}
        onSubmit={handleTemplateSubmit}
      />
    </Card>
  );
};
