import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Input,
  Select,
  Space,
  Popconfirm,
  message,
  Tag,
  Typography,
  Row,
  Col,
  InputNumber,
  Divider
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  PlayCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import { tuningApi, QueryPattern, CreateQueryPatternRequest } from '../../services/tuningApi';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

interface QueryPatternManagerProps {
  onDataChange?: () => void;
}

export const QueryPatternManager: React.FC<QueryPatternManagerProps> = ({ onDataChange }) => {
  const [patterns, setPatterns] = useState<QueryPattern[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [testModalVisible, setTestModalVisible] = useState(false);
  const [editingPattern, setEditingPattern] = useState<QueryPattern | null>(null);
  const [testingPattern, setTestingPattern] = useState<QueryPattern | null>(null);
  const [testQuery, setTestQuery] = useState('');
  const [testResult, setTestResult] = useState('');
  const [form] = Form.useForm();

  useEffect(() => {
    loadPatterns();
  }, []);

  const loadPatterns = async () => {
    try {
      setLoading(true);
      const data = await tuningApi.getQueryPatterns();
      setPatterns(data);
    } catch (error) {
      message.error('Failed to load query patterns');
      console.error('Error loading patterns:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingPattern(null);
    form.resetFields();
    form.setFieldsValue({
      priority: 1,
      keywords: [],
      requiredTables: []
    });
    setModalVisible(true);
  };

  const handleEdit = (pattern: QueryPattern) => {
    setEditingPattern(pattern);
    form.setFieldsValue({
      patternName: pattern.patternName,
      naturalLanguagePattern: pattern.naturalLanguagePattern,
      sqlTemplate: pattern.sqlTemplate,
      description: pattern.description,
      businessContext: pattern.businessContext,
      keywords: pattern.keywords,
      requiredTables: pattern.requiredTables,
      priority: pattern.priority
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await tuningApi.deleteQueryPattern(id);
      message.success('Pattern deleted successfully');
      loadPatterns();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to delete pattern');
      console.error('Error deleting pattern:', error);
    }
  };

  const handleTest = (pattern: QueryPattern) => {
    setTestingPattern(pattern);
    setTestQuery('');
    setTestResult('');
    setTestModalVisible(true);
  };

  const runTest = async () => {
    if (!testingPattern || !testQuery.trim()) {
      message.warning('Please enter a test query');
      return;
    }

    try {
      const result = await tuningApi.testQueryPattern(testingPattern.id, testQuery);
      setTestResult(result);
    } catch (error) {
      message.error('Failed to test pattern');
      console.error('Error testing pattern:', error);
    }
  };

  const handleSubmit = async (values: any) => {
    try {
      const request: CreateQueryPatternRequest = {
        patternName: values.patternName,
        naturalLanguagePattern: values.naturalLanguagePattern,
        sqlTemplate: values.sqlTemplate,
        description: values.description || '',
        businessContext: values.businessContext || '',
        keywords: values.keywords || [],
        requiredTables: values.requiredTables || [],
        priority: values.priority || 1
      };

      if (editingPattern) {
        await tuningApi.updateQueryPattern(editingPattern.id, request);
        message.success('Pattern updated successfully');
      } else {
        await tuningApi.createQueryPattern(request);
        message.success('Pattern created successfully');
      }

      setModalVisible(false);
      loadPatterns();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to save pattern');
      console.error('Error saving pattern:', error);
    }
  };

  const columns = [
    {
      title: 'Pattern Name',
      dataIndex: 'patternName',
      key: 'patternName',
      render: (text: string, record: QueryPattern) => (
        <Space direction="vertical" size="small">
          <Text strong>{text}</Text>
          <Tag color={record.priority === 1 ? 'red' : record.priority === 2 ? 'orange' : 'blue'}>
            Priority {record.priority}
          </Tag>
        </Space>
      ),
    },
    {
      title: 'Natural Language Pattern',
      dataIndex: 'naturalLanguagePattern',
      key: 'naturalLanguagePattern',
      ellipsis: true,
      render: (text: string) => (
        <Text code style={{ fontSize: '12px' }}>{text}</Text>
      ),
    },
    {
      title: 'Keywords',
      dataIndex: 'keywords',
      key: 'keywords',
      render: (keywords: string[]) => (
        <Space wrap>
          {keywords.slice(0, 3).map((keyword, index) => (
            <Tag key={index} color="green" style={{ fontSize: '11px' }}>
              {keyword}
            </Tag>
          ))}
          {keywords.length > 3 && (
            <Tag color="default">+{keywords.length - 3} more</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Required Tables',
      dataIndex: 'requiredTables',
      key: 'requiredTables',
      render: (tables: string[]) => (
        <Space wrap>
          {tables.slice(0, 2).map((table, index) => (
            <Tag key={index} color="purple" style={{ fontSize: '11px' }}>
              {table}
            </Tag>
          ))}
          {tables.length > 2 && (
            <Tag color="default">+{tables.length - 2} more</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: any, record: QueryPattern) => (
        <Space>
          <Button
            type="link"
            icon={<PlayCircleOutlined />}
            onClick={() => handleTest(record)}
            size="small"
            title="Test Pattern"
          />
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            size="small"
          />
          <Popconfirm
            title="Are you sure you want to delete this pattern?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
              size="small"
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card
        title={
          <Space>
            <SearchOutlined />
            <span>Query Pattern Management</span>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleCreate}
          >
            Add Pattern
          </Button>
        }
      >
        <Table
          columns={columns}
          dataSource={patterns}
          rowKey="id"
          loading={loading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
          }}
          expandable={{
            expandedRowRender: (record) => (
              <div style={{ padding: '16px', backgroundColor: '#fafafa' }}>
                <Row gutter={[16, 16]}>
                  <Col span={12}>
                    <Title level={5}>Description</Title>
                    <Text>{record.description || 'No description provided'}</Text>
                    <Divider />
                    <Title level={5}>Business Context</Title>
                    <Text>{record.businessContext || 'No context provided'}</Text>
                  </Col>
                  <Col span={12}>
                    <Title level={5}>SQL Template</Title>
                    <div style={{ 
                      backgroundColor: '#f6f8fa', 
                      padding: '12px', 
                      borderRadius: '4px',
                      fontFamily: 'monospace',
                      fontSize: '12px',
                      whiteSpace: 'pre-wrap'
                    }}>
                      {record.sqlTemplate}
                    </div>
                  </Col>
                </Row>
              </div>
            ),
            rowExpandable: () => true,
          }}
        />
      </Card>

      <Modal
        title={editingPattern ? 'Edit Query Pattern' : 'Add Query Pattern'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={900}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Row gutter={16}>
            <Col span={16}>
              <Form.Item
                name="patternName"
                label="Pattern Name"
                rules={[{ required: true, message: 'Please enter pattern name' }]}
              >
                <Input placeholder="e.g., Daily Totals Pattern" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="priority"
                label="Priority"
                rules={[{ required: true, message: 'Please set priority' }]}
              >
                <InputNumber min={1} max={10} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="naturalLanguagePattern"
            label="Natural Language Pattern"
            rules={[{ required: true, message: 'Please enter natural language pattern' }]}
          >
            <Input placeholder="e.g., totals today, daily summary, player activity today" />
          </Form.Item>

          <Form.Item
            name="sqlTemplate"
            label="SQL Template"
            rules={[{ required: true, message: 'Please enter SQL template' }]}
          >
            <TextArea
              rows={6}
              placeholder="SELECT ... FROM ... WHERE ..."
              style={{ fontFamily: 'monospace' }}
            />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="keywords"
                label="Keywords"
              >
                <Select
                  mode="tags"
                  placeholder="Add keywords that trigger this pattern"
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="requiredTables"
                label="Required Tables"
              >
                <Select
                  mode="tags"
                  placeholder="Tables required for this pattern"
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="description"
            label="Description"
          >
            <TextArea
              rows={2}
              placeholder="Describe when and how this pattern should be used..."
            />
          </Form.Item>

          <Form.Item
            name="businessContext"
            label="Business Context"
          >
            <TextArea
              rows={2}
              placeholder="Business context and use cases for this pattern..."
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="Test Query Pattern"
        open={testModalVisible}
        onCancel={() => setTestModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setTestModalVisible(false)}>
            Close
          </Button>,
          <Button key="test" type="primary" onClick={runTest}>
            Run Test
          </Button>
        ]}
        width={800}
      >
        {testingPattern && (
          <div>
            <Title level={5}>Testing Pattern: {testingPattern.patternName}</Title>
            <Form layout="vertical">
              <Form.Item label="Enter test query">
                <Input
                  value={testQuery}
                  onChange={(e) => setTestQuery(e.target.value)}
                  placeholder="e.g., show me totals for today"
                />
              </Form.Item>
            </Form>
            {testResult && (
              <div>
                <Divider />
                <Title level={5}>Test Result</Title>
                <div style={{ 
                  backgroundColor: '#f6f8fa', 
                  padding: '12px', 
                  borderRadius: '4px',
                  fontFamily: 'monospace',
                  fontSize: '12px',
                  whiteSpace: 'pre-wrap'
                }}>
                  {testResult}
                </div>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
};
