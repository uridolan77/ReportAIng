import React, { useState, useEffect } from 'react';
import {
  Card,
  Input,
  Select,
  Button,
  List,
  Tag,
  Space,
  Typography,
  Modal,
  Form,
  Row,
  Col,
  Tooltip,
  Badge,
  Empty,
  Divider
} from 'antd';
import {
  SearchOutlined,
  FilterOutlined,
  StarOutlined,
  StarFilled,
  PlayCircleOutlined,
  CopyOutlined,
  EditOutlined,
  PlusOutlined,
  BookOutlined
} from '@ant-design/icons';

const { Text, Title, Paragraph } = Typography;
const { Search } = Input;
const { Option } = Select;
const { TextArea } = Input;

export interface QueryTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  template: string;
  parameters: TemplateParameter[];
  preview: string;
  tags: string[];
  difficulty: 'beginner' | 'intermediate' | 'advanced';
  estimatedTime: string;
  author: string;
  createdAt: string;
  usageCount: number;
  isFavorite?: boolean;
  isCustom?: boolean;
}

export interface TemplateParameter {
  name: string;
  type: 'text' | 'number' | 'date' | 'select' | 'multiselect';
  label: string;
  description?: string;
  required: boolean;
  defaultValue?: any;
  options?: { label: string; value: any }[];
  validation?: {
    min?: number;
    max?: number;
    pattern?: string;
  };
}

interface QueryTemplateLibraryProps {
  onApplyTemplate: (query: string, template: QueryTemplate) => void;
  onClose: () => void;
}

export const QueryTemplateLibrary: React.FC<QueryTemplateLibraryProps> = ({
  onApplyTemplate,
  onClose
}) => {
  const [templates, setTemplates] = useState<QueryTemplate[]>([]);
  const [filteredTemplates, setFilteredTemplates] = useState<QueryTemplate[]>([]);
  const [selectedTemplate, setSelectedTemplate] = useState<QueryTemplate | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [selectedDifficulty, setSelectedDifficulty] = useState<string>('all');
  const [showParameterModal, setShowParameterModal] = useState(false);
  const [parameterValues, setParameterValues] = useState<Record<string, any>>({});
  const [favorites, setFavorites] = useState<Set<string>>(new Set());

  useEffect(() => {
    loadTemplates();
    loadFavorites();
  }, []);

  useEffect(() => {
    filterTemplates();
  }, [templates, searchTerm, selectedCategory, selectedDifficulty]);

  const loadTemplates = () => {
    // Mock templates - in real app, this would come from API
    const mockTemplates: QueryTemplate[] = [
      {
        id: 'revenue-by-period',
        name: 'Revenue Analysis by Time Period',
        description: 'Analyze revenue trends over a specified time period with breakdown by different dimensions',
        category: 'Financial Analysis',
        template: 'Show me total {{metric}} from {{table}} where {{date_column}} between {{start_date}} and {{end_date}} grouped by {{group_by}}',
        parameters: [
          {
            name: 'metric',
            type: 'select',
            label: 'Revenue Metric',
            required: true,
            options: [
              { label: 'Deposits', value: 'deposits' },
              { label: 'Bets', value: 'bets' },
              { label: 'Wins', value: 'wins' }
            ]
          },
          {
            name: 'table',
            type: 'select',
            label: 'Data Source',
            required: true,
            defaultValue: 'tbl_Daily_actions',
            options: [
              { label: 'Daily Actions', value: 'tbl_Daily_actions' },
              { label: 'Player Data', value: 'tbl_Daily_actions_players' }
            ]
          },
          {
            name: 'date_column',
            type: 'select',
            label: 'Date Column',
            required: true,
            defaultValue: 'ActionDate',
            options: [
              { label: 'Action Date', value: 'ActionDate' },
              { label: 'Registration Date', value: 'RegistrationDate' }
            ]
          },
          {
            name: 'start_date',
            type: 'date',
            label: 'Start Date',
            required: true
          },
          {
            name: 'end_date',
            type: 'date',
            label: 'End Date',
            required: true
          },
          {
            name: 'group_by',
            type: 'select',
            label: 'Group By',
            required: true,
            options: [
              { label: 'Day', value: 'day' },
              { label: 'Week', value: 'week' },
              { label: 'Month', value: 'month' },
              { label: 'White Label', value: 'WhiteLabelID' }
            ]
          }
        ],
        preview: 'Show me total deposits from tbl_Daily_actions where ActionDate between 2024-01-01 and 2024-01-31 grouped by month',
        tags: ['revenue', 'time-series', 'financial'],
        difficulty: 'beginner',
        estimatedTime: '< 1 minute',
        author: 'System',
        createdAt: '2024-01-01',
        usageCount: 245
      },
      {
        id: 'player-segmentation',
        name: 'Player Segmentation Analysis',
        description: 'Segment players based on their activity levels and value',
        category: 'Player Analytics',
        template: 'Analyze players from {{table}} where {{activity_metric}} {{operator}} {{threshold}} in the last {{time_period}} days',
        parameters: [
          {
            name: 'table',
            type: 'select',
            label: 'Player Table',
            required: true,
            defaultValue: 'tbl_Daily_actions_players',
            options: [
              { label: 'Player Data', value: 'tbl_Daily_actions_players' },
              { label: 'Daily Actions', value: 'tbl_Daily_actions' }
            ]
          },
          {
            name: 'activity_metric',
            type: 'select',
            label: 'Activity Metric',
            required: true,
            options: [
              { label: 'Total Deposits', value: 'total_deposits' },
              { label: 'Total Bets', value: 'total_bets' },
              { label: 'Session Count', value: 'session_count' }
            ]
          },
          {
            name: 'operator',
            type: 'select',
            label: 'Comparison',
            required: true,
            options: [
              { label: 'Greater than', value: '>' },
              { label: 'Less than', value: '<' },
              { label: 'Equal to', value: '=' }
            ]
          },
          {
            name: 'threshold',
            type: 'number',
            label: 'Threshold Value',
            required: true,
            validation: { min: 0 }
          },
          {
            name: 'time_period',
            type: 'number',
            label: 'Time Period (days)',
            required: true,
            defaultValue: 30,
            validation: { min: 1, max: 365 }
          }
        ],
        preview: 'Analyze players from tbl_Daily_actions_players where total_deposits > 1000 in the last 30 days',
        tags: ['players', 'segmentation', 'analytics'],
        difficulty: 'intermediate',
        estimatedTime: '2-3 minutes',
        author: 'System',
        createdAt: '2024-01-01',
        usageCount: 156
      },
      {
        id: 'top-performers',
        name: 'Top Performers by Metric',
        description: 'Find top performing entities by any metric with customizable ranking',
        category: 'Performance Analysis',
        template: 'Show me top {{limit}} {{entity}} by {{metric}} from {{table}} {{time_filter}}',
        parameters: [
          {
            name: 'limit',
            type: 'number',
            label: 'Number of Results',
            required: true,
            defaultValue: 10,
            validation: { min: 1, max: 100 }
          },
          {
            name: 'entity',
            type: 'select',
            label: 'Entity Type',
            required: true,
            options: [
              { label: 'Players', value: 'players' },
              { label: 'White Labels', value: 'white_labels' },
              { label: 'Countries', value: 'countries' }
            ]
          },
          {
            name: 'metric',
            type: 'select',
            label: 'Performance Metric',
            required: true,
            options: [
              { label: 'Total Revenue', value: 'total_revenue' },
              { label: 'Average Bet Size', value: 'avg_bet_size' },
              { label: 'Win Rate', value: 'win_rate' }
            ]
          },
          {
            name: 'table',
            type: 'select',
            label: 'Data Source',
            required: true,
            defaultValue: 'tbl_Daily_actions',
            options: [
              { label: 'Daily Actions', value: 'tbl_Daily_actions' },
              { label: 'Player Data', value: 'tbl_Daily_actions_players' }
            ]
          },
          {
            name: 'time_filter',
            type: 'text',
            label: 'Time Filter (optional)',
            required: false,
            defaultValue: 'in the last 30 days'
          }
        ],
        preview: 'Show me top 10 players by total_revenue from tbl_Daily_actions in the last 30 days',
        tags: ['ranking', 'performance', 'top-performers'],
        difficulty: 'beginner',
        estimatedTime: '< 1 minute',
        author: 'System',
        createdAt: '2024-01-01',
        usageCount: 189
      }
    ];

    setTemplates(mockTemplates);
  };

  const loadFavorites = () => {
    const savedFavorites = localStorage.getItem('query-template-favorites');
    if (savedFavorites) {
      setFavorites(new Set(JSON.parse(savedFavorites)));
    }
  };

  const filterTemplates = () => {
    let filtered = templates;

    if (searchTerm) {
      filtered = filtered.filter(template =>
        template.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        template.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        template.tags.some(tag => tag.toLowerCase().includes(searchTerm.toLowerCase()))
      );
    }

    if (selectedCategory !== 'all') {
      filtered = filtered.filter(template => template.category === selectedCategory);
    }

    if (selectedDifficulty !== 'all') {
      filtered = filtered.filter(template => template.difficulty === selectedDifficulty);
    }

    setFilteredTemplates(filtered);
  };

  const getCategories = () => {
    const categories = [...new Set(templates.map(t => t.category))];
    return categories.sort();
  };

  const toggleFavorite = (templateId: string) => {
    const newFavorites = new Set(favorites);
    if (newFavorites.has(templateId)) {
      newFavorites.delete(templateId);
    } else {
      newFavorites.add(templateId);
    }
    setFavorites(newFavorites);
    localStorage.setItem('query-template-favorites', JSON.stringify([...newFavorites]));
  };

  const handleTemplateSelect = (template: QueryTemplate) => {
    setSelectedTemplate(template);
    if (template.parameters.length > 0) {
      // Initialize parameter values with defaults
      const initialValues: Record<string, any> = {};
      template.parameters.forEach(param => {
        if (param.defaultValue !== undefined) {
          initialValues[param.name] = param.defaultValue;
        }
      });
      setParameterValues(initialValues);
      setShowParameterModal(true);
    } else {
      // Apply template directly if no parameters
      onApplyTemplate(template.template, template);
    }
  };

  const applyTemplate = () => {
    if (!selectedTemplate) return;

    let query = selectedTemplate.template;

    // Replace parameters
    Object.entries(parameterValues).forEach(([key, value]) => {
      const placeholder = `{{${key}}}`;
      query = query.replace(new RegExp(placeholder, 'g'), value);
    });

    onApplyTemplate(query, selectedTemplate);
    setShowParameterModal(false);
    onClose();
  };

  const copyTemplate = (template: QueryTemplate) => {
    navigator.clipboard.writeText(template.template);
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'beginner': return 'green';
      case 'intermediate': return 'orange';
      case 'advanced': return 'red';
      default: return 'default';
    }
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={3}>
          <BookOutlined /> Query Template Library
        </Title>
        <Paragraph type="secondary">
          Choose from pre-built query templates to quickly generate insights from your data.
        </Paragraph>
      </div>

      {/* Filters */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} sm={12} md={8}>
            <Search
              placeholder="Search templates..."
              prefix={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              allowClear
            />
          </Col>

          <Col xs={24} sm={6} md={4}>
            <Select
              style={{ width: '100%' }}
              placeholder="Category"
              value={selectedCategory}
              onChange={setSelectedCategory}
            >
              <Option value="all">All Categories</Option>
              {getCategories().map(category => (
                <Option key={category} value={category}>{category}</Option>
              ))}
            </Select>
          </Col>

          <Col xs={24} sm={6} md={4}>
            <Select
              style={{ width: '100%' }}
              placeholder="Difficulty"
              value={selectedDifficulty}
              onChange={setSelectedDifficulty}
            >
              <Option value="all">All Levels</Option>
              <Option value="beginner">Beginner</Option>
              <Option value="intermediate">Intermediate</Option>
              <Option value="advanced">Advanced</Option>
            </Select>
          </Col>

          <Col xs={24} sm={24} md={8}>
            <Text type="secondary">
              {filteredTemplates.length} template{filteredTemplates.length !== 1 ? 's' : ''} found
            </Text>
          </Col>
        </Row>
      </Card>

      {/* Template List */}
      {filteredTemplates.length === 0 ? (
        <Empty
          description="No templates found"
          style={{ padding: '60px' }}
        />
      ) : (
        <List
          grid={{ gutter: 16, xs: 1, sm: 1, md: 2, lg: 2, xl: 3 }}
          dataSource={filteredTemplates}
          renderItem={(template) => (
            <List.Item>
              <Card
                hoverable
                actions={[
                  <Tooltip title="Add to Favorites">
                    <Button
                      type="text"
                      icon={favorites.has(template.id) ? <StarFilled style={{ color: '#faad14' }} /> : <StarOutlined />}
                      onClick={() => toggleFavorite(template.id)}
                    />
                  </Tooltip>,
                  <Tooltip title="Copy Template">
                    <Button
                      type="text"
                      icon={<CopyOutlined />}
                      onClick={() => copyTemplate(template)}
                    />
                  </Tooltip>,
                  <Tooltip title="Use Template">
                    <Button
                      type="text"
                      icon={<PlayCircleOutlined />}
                      onClick={() => handleTemplateSelect(template)}
                    />
                  </Tooltip>
                ]}
              >
                <Card.Meta
                  title={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                        <Text strong>{template.name}</Text>
                        <Tag color={getDifficultyColor(template.difficulty)}>
                          {template.difficulty}
                        </Tag>
                      </div>
                      <Tag color="blue">{template.category}</Tag>
                    </Space>
                  }
                  description={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {template.description}
                      </Text>

                      <div>
                        <Text strong style={{ fontSize: '11px' }}>Preview:</Text>
                        <div style={{
                          backgroundColor: '#f5f5f5',
                          padding: '8px',
                          borderRadius: '4px',
                          fontSize: '11px',
                          fontFamily: 'monospace',
                          marginTop: '4px'
                        }}>
                          {template.preview}
                        </div>
                      </div>

                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Space size="small">
                          {template.tags.slice(0, 2).map(tag => (
                            <Tag key={tag} style={{ fontSize: '11px' }}>{tag}</Tag>
                          ))}
                          {template.tags.length > 2 && (
                            <Tag style={{ fontSize: '11px' }}>+{template.tags.length - 2}</Tag>
                          )}
                        </Space>
                        <Badge count={template.usageCount} style={{ backgroundColor: '#52c41a' }} />
                      </div>

                      <Text type="secondary" style={{ fontSize: '10px' }}>
                        Est. time: {template.estimatedTime}
                      </Text>
                    </Space>
                  }
                />
              </Card>
            </List.Item>
          )}
        />
      )}

      {/* Parameter Modal */}
      <Modal
        title={`Configure: ${selectedTemplate?.name}`}
        open={showParameterModal}
        onCancel={() => setShowParameterModal(false)}
        footer={[
          <Button key="cancel" onClick={() => setShowParameterModal(false)}>
            Cancel
          </Button>,
          <Button key="apply" type="primary" onClick={applyTemplate}>
            Apply Template
          </Button>
        ]}
        width={600}
      >
        {selectedTemplate && (
          <Form layout="vertical">
            {selectedTemplate.parameters.map((param) => (
              <Form.Item
                key={param.name}
                label={param.label}
                required={param.required}
                help={param.description}
              >
                {param.type === 'select' ? (
                  <Select
                    style={{ width: '100%' }}
                    placeholder={`Select ${param.label}`}
                    value={parameterValues[param.name]}
                    onChange={(value) => setParameterValues(prev => ({ ...prev, [param.name]: value }))}
                  >
                    {param.options?.map(option => (
                      <Option key={option.value} value={option.value}>
                        {option.label}
                      </Option>
                    ))}
                  </Select>
                ) : param.type === 'number' ? (
                  <Input
                    type="number"
                    placeholder={`Enter ${param.label}`}
                    value={parameterValues[param.name]}
                    onChange={(e) => setParameterValues(prev => ({ ...prev, [param.name]: e.target.value }))}
                    min={param.validation?.min}
                    max={param.validation?.max}
                  />
                ) : param.type === 'date' ? (
                  <Input
                    type="date"
                    value={parameterValues[param.name]}
                    onChange={(e) => setParameterValues(prev => ({ ...prev, [param.name]: e.target.value }))}
                  />
                ) : (
                  <Input
                    placeholder={`Enter ${param.label}`}
                    value={parameterValues[param.name]}
                    onChange={(e) => setParameterValues(prev => ({ ...prev, [param.name]: e.target.value }))}
                  />
                )}
              </Form.Item>
            ))}
          </Form>
        )}
      </Modal>
    </div>
  );
};
