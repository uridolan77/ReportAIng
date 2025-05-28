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
  Collapse,
  Divider
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  TableOutlined,

} from '@ant-design/icons';
import { tuningApi, BusinessTableInfo, CreateTableRequest } from '../../services/tuningApi';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Panel } = Collapse;
// const { Option } = Select;

interface BusinessTableManagerProps {
  onDataChange?: () => void;
}

export const BusinessTableManager: React.FC<BusinessTableManagerProps> = ({ onDataChange }) => {
  const [tables, setTables] = useState<BusinessTableInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingTable, setEditingTable] = useState<BusinessTableInfo | null>(null);
  const [form] = Form.useForm();

  useEffect(() => {
    loadTables();
  }, []);

  const loadTables = async () => {
    try {
      setLoading(true);
      const data = await tuningApi.getBusinessTables();
      setTables(data);
    } catch (error) {
      message.error('Failed to load business tables');
      console.error('Error loading tables:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingTable(null);
    form.resetFields();
    form.setFieldsValue({
      schemaName: 'common',
      commonQueryPatterns: [],
      columns: []
    });
    setModalVisible(true);
  };

  const handleEdit = (table: BusinessTableInfo) => {
    setEditingTable(table);
    form.setFieldsValue({
      tableName: table.tableName,
      schemaName: table.schemaName,
      businessPurpose: table.businessPurpose,
      businessContext: table.businessContext,
      primaryUseCase: table.primaryUseCase,
      commonQueryPatterns: table.commonQueryPatterns,
      businessRules: table.businessRules,
      columns: table.columns
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await tuningApi.deleteBusinessTable(id);
      message.success('Table deleted successfully');
      loadTables();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to delete table');
      console.error('Error deleting table:', error);
    }
  };

  const handleSubmit = async (values: any) => {
    try {
      const request: CreateTableRequest = {
        tableName: values.tableName,
        schemaName: values.schemaName || 'common',
        businessPurpose: values.businessPurpose || '',
        businessContext: values.businessContext || '',
        primaryUseCase: values.primaryUseCase || '',
        commonQueryPatterns: values.commonQueryPatterns || [],
        businessRules: values.businessRules || '',
        columns: values.columns || []
      };

      if (editingTable) {
        await tuningApi.updateBusinessTable(editingTable.id, request);
        message.success('Table updated successfully');
      } else {
        await tuningApi.createBusinessTable(request);
        message.success('Table created successfully');
      }

      setModalVisible(false);
      loadTables();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to save table');
      console.error('Error saving table:', error);
    }
  };

  const columns = [
    {
      title: 'Table Name',
      dataIndex: 'tableName',
      key: 'tableName',
      render: (text: string, record: BusinessTableInfo) => (
        <Space direction="vertical" size="small">
          <Text strong>{record.schemaName}.{text}</Text>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.columns.length} columns documented
          </Text>
        </Space>
      ),
    },
    {
      title: 'Business Purpose',
      dataIndex: 'businessPurpose',
      key: 'businessPurpose',
      ellipsis: true,
    },
    {
      title: 'Primary Use Case',
      dataIndex: 'primaryUseCase',
      key: 'primaryUseCase',
      ellipsis: true,
    },
    {
      title: 'Query Patterns',
      dataIndex: 'commonQueryPatterns',
      key: 'commonQueryPatterns',
      render: (patterns: string[]) => (
        <Space wrap>
          {patterns.slice(0, 3).map((pattern, index) => (
            <Tag key={index} color="blue" style={{ fontSize: '11px' }}>
              {pattern}
            </Tag>
          ))}
          {patterns.length > 3 && (
            <Tag color="default">+{patterns.length - 3} more</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: any, record: BusinessTableInfo) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            size="small"
          />
          <Popconfirm
            title="Are you sure you want to delete this table?"
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
            <TableOutlined />
            <span>Business Table Documentation</span>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleCreate}
          >
            Add Table
          </Button>
        }
      >
        <Table
          columns={columns}
          dataSource={tables}
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
                    <Title level={5}>Business Context</Title>
                    <Text>{record.businessContext || 'No context provided'}</Text>
                  </Col>
                  <Col span={12}>
                    <Title level={5}>Business Rules</Title>
                    <Text>{record.businessRules || 'No rules defined'}</Text>
                  </Col>
                </Row>
                {record.columns.length > 0 && (
                  <>
                    <Divider />
                    <Title level={5}>Column Documentation</Title>
                    <Collapse size="small">
                      {record.columns.map((column, index) => (
                        <Panel
                          header={
                            <Space>
                              <Text strong>{column.columnName}</Text>
                              {column.isKeyColumn && <Tag color="gold">Key Column</Tag>}
                            </Space>
                          }
                          key={index}
                        >
                          <Space direction="vertical" style={{ width: '100%' }}>
                            <div>
                              <Text strong>Business Meaning: </Text>
                              <Text>{column.businessMeaning || 'Not documented'}</Text>
                            </div>
                            <div>
                              <Text strong>Context: </Text>
                              <Text>{column.businessContext || 'Not provided'}</Text>
                            </div>
                            {column.dataExamples.length > 0 && (
                              <div>
                                <Text strong>Examples: </Text>
                                <Space wrap>
                                  {column.dataExamples.map((example, idx) => (
                                    <Tag key={idx} color="cyan">{example}</Tag>
                                  ))}
                                </Space>
                              </div>
                            )}
                          </Space>
                        </Panel>
                      ))}
                    </Collapse>
                  </>
                )}
              </div>
            ),
            rowExpandable: () => true,
          }}
        />
      </Card>

      <Modal
        title={editingTable ? 'Edit Business Table' : 'Add Business Table'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={800}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="tableName"
                label="Table Name"
                rules={[{ required: true, message: 'Please enter table name' }]}
              >
                <Input placeholder="e.g., tbl_Daily_actions" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="schemaName"
                label="Schema Name"
                rules={[{ required: true, message: 'Please enter schema name' }]}
              >
                <Input placeholder="e.g., common" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="businessPurpose"
            label="Business Purpose"
            rules={[{ required: true, message: 'Please enter business purpose' }]}
          >
            <TextArea
              rows={2}
              placeholder="Brief description of what this table is used for..."
            />
          </Form.Item>

          <Form.Item
            name="businessContext"
            label="Business Context"
          >
            <TextArea
              rows={3}
              placeholder="Detailed explanation of the business context and importance..."
            />
          </Form.Item>

          <Form.Item
            name="primaryUseCase"
            label="Primary Use Case"
          >
            <Input placeholder="Main business scenario where this table is used" />
          </Form.Item>

          <Form.Item
            name="commonQueryPatterns"
            label="Common Query Patterns"
          >
            <Select
              mode="tags"
              placeholder="Add common query patterns (e.g., 'totals today', 'player activity')"
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            name="businessRules"
            label="Business Rules"
          >
            <TextArea
              rows={2}
              placeholder="Important business rules and constraints..."
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
