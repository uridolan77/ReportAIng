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
  Divider
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  BookOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import { tuningApi, BusinessGlossaryTerm } from '../../services/tuningApi';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

interface BusinessGlossaryManagerProps {
  onDataChange?: () => void;
}

const CATEGORIES = [
  'Financial',
  'Player',
  'Gaming',
  'Bonus',
  'Transaction',
  'Reporting',
  'Technical',
  'Business',
  'Other'
];

export const BusinessGlossaryManager: React.FC<BusinessGlossaryManagerProps> = ({ onDataChange }) => {
  const [terms, setTerms] = useState<BusinessGlossaryTerm[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingTerm, setEditingTerm] = useState<BusinessGlossaryTerm | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [form] = Form.useForm();

  useEffect(() => {
    loadTerms();
  }, []);

  const loadTerms = async () => {
    try {
      setLoading(true);
      const data = await tuningApi.getGlossaryTerms();
      setTerms(data);
    } catch (error) {
      message.error('Failed to load glossary terms');
      console.error('Error loading terms:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingTerm(null);
    form.resetFields();
    form.setFieldsValue({
      category: 'Business',
      synonyms: [],
      relatedTerms: []
    });
    setModalVisible(true);
  };

  const handleEdit = (term: BusinessGlossaryTerm) => {
    setEditingTerm(term);
    form.setFieldsValue({
      term: term.term,
      definition: term.definition,
      businessContext: term.businessContext,
      synonyms: term.synonyms,
      relatedTerms: term.relatedTerms,
      category: term.category
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await tuningApi.deleteGlossaryTerm(id);
      message.success('Term deleted successfully');
      loadTerms();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to delete term');
      console.error('Error deleting term:', error);
    }
  };

  const handleSubmit = async (values: any) => {
    try {
      const request: BusinessGlossaryTerm = {
        id: editingTerm?.id || 0,
        term: values.term,
        definition: values.definition,
        businessContext: values.businessContext || '',
        synonyms: values.synonyms || [],
        relatedTerms: values.relatedTerms || [],
        category: values.category || 'Business',
        isActive: true
      };

      if (editingTerm) {
        await tuningApi.updateGlossaryTerm(editingTerm.id, request);
        message.success('Term updated successfully');
      } else {
        await tuningApi.createGlossaryTerm(request);
        message.success('Term created successfully');
      }

      setModalVisible(false);
      loadTerms();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to save term');
      console.error('Error saving term:', error);
    }
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      'Financial': 'green',
      'Player': 'blue',
      'Gaming': 'purple',
      'Bonus': 'orange',
      'Transaction': 'cyan',
      'Reporting': 'magenta',
      'Technical': 'red',
      'Business': 'geekblue',
      'Other': 'default'
    };
    return colors[category] || 'default';
  };

  const filteredTerms = selectedCategory 
    ? terms.filter(term => term.category === selectedCategory)
    : terms;

  const columns = [
    {
      title: 'Term',
      dataIndex: 'term',
      key: 'term',
      render: (text: string, record: BusinessGlossaryTerm) => (
        <Space direction="vertical" size="small">
          <Text strong style={{ fontSize: '14px' }}>{text}</Text>
          <Tag color={getCategoryColor(record.category)}>
            {record.category}
          </Tag>
        </Space>
      ),
    },
    {
      title: 'Definition',
      dataIndex: 'definition',
      key: 'definition',
      ellipsis: true,
      render: (text: string) => (
        <Text style={{ fontSize: '13px' }}>{text}</Text>
      ),
    },
    {
      title: 'Synonyms',
      dataIndex: 'synonyms',
      key: 'synonyms',
      render: (synonyms: string[]) => (
        <Space wrap>
          {synonyms.slice(0, 3).map((synonym, index) => (
            <Tag key={index} color="blue" style={{ fontSize: '11px' }}>
              {synonym}
            </Tag>
          ))}
          {synonyms.length > 3 && (
            <Tag color="default">+{synonyms.length - 3} more</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Related Terms',
      dataIndex: 'relatedTerms',
      key: 'relatedTerms',
      render: (relatedTerms: string[]) => (
        <Space wrap>
          {relatedTerms.slice(0, 2).map((term, index) => (
            <Tag key={index} color="green" style={{ fontSize: '11px' }}>
              {term}
            </Tag>
          ))}
          {relatedTerms.length > 2 && (
            <Tag color="default">+{relatedTerms.length - 2} more</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: any, record: BusinessGlossaryTerm) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            size="small"
          />
          <Popconfirm
            title="Are you sure you want to delete this term?"
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
            <BookOutlined />
            <span>Business Glossary</span>
          </Space>
        }
        extra={
          <Space>
            <Select
              placeholder="Filter by category"
              style={{ width: 150 }}
              allowClear
              value={selectedCategory}
              onChange={setSelectedCategory}
            >
              {CATEGORIES.map(category => (
                <Option key={category} value={category}>
                  {category}
                </Option>
              ))}
            </Select>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleCreate}
            >
              Add Term
            </Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={filteredTerms}
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
                  <Col span={24}>
                    <Title level={5}>Business Context</Title>
                    <Text>{record.businessContext || 'No business context provided'}</Text>
                  </Col>
                </Row>
                {(record.synonyms.length > 0 || record.relatedTerms.length > 0) && (
                  <>
                    <Divider />
                    <Row gutter={[16, 16]}>
                      {record.synonyms.length > 0 && (
                        <Col span={12}>
                          <Title level={5}>All Synonyms</Title>
                          <Space wrap>
                            {record.synonyms.map((synonym, index) => (
                              <Tag key={index} color="blue">{synonym}</Tag>
                            ))}
                          </Space>
                        </Col>
                      )}
                      {record.relatedTerms.length > 0 && (
                        <Col span={12}>
                          <Title level={5}>All Related Terms</Title>
                          <Space wrap>
                            {record.relatedTerms.map((term, index) => (
                              <Tag key={index} color="green">{term}</Tag>
                            ))}
                          </Space>
                        </Col>
                      )}
                    </Row>
                  </>
                )}
              </div>
            ),
            rowExpandable: () => true,
          }}
        />
      </Card>

      <Modal
        title={editingTerm ? 'Edit Glossary Term' : 'Add Glossary Term'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={700}
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
                name="term"
                label="Term"
                rules={[{ required: true, message: 'Please enter the term' }]}
              >
                <Input placeholder="e.g., Daily Actions" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="category"
                label="Category"
                rules={[{ required: true, message: 'Please select a category' }]}
              >
                <Select placeholder="Select category">
                  {CATEGORIES.map(category => (
                    <Option key={category} value={category}>
                      {category}
                    </Option>
                  ))}
                </Select>
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
              placeholder="Clear and concise definition of the term..."
            />
          </Form.Item>

          <Form.Item
            name="businessContext"
            label="Business Context"
          >
            <TextArea
              rows={2}
              placeholder="How this term is used in the business context..."
            />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="synonyms"
                label="Synonyms"
              >
                <Select
                  mode="tags"
                  placeholder="Add synonyms or alternative terms"
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="relatedTerms"
                label="Related Terms"
              >
                <Select
                  mode="tags"
                  placeholder="Add related business terms"
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </div>
  );
};
