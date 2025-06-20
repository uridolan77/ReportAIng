import React, { useState, useEffect, useCallback } from 'react';
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
  Divider,
  Switch,
  Alert,
  Tooltip
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  TableOutlined,
  ColumnHeightOutlined,
  KeyOutlined,
  MinusCircleOutlined,
  SyncOutlined,
  DatabaseOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import { tuningApi, BusinessTableInfo, CreateTableRequest } from '../../services/tuningApi';
import { DBExplorerAPI } from '../../services/dbExplorerApi';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Panel } = Collapse;

interface BusinessTableManagerProps {
  onDataChange?: () => void;
}

export const BusinessTableManager: React.FC<BusinessTableManagerProps> = ({ onDataChange }) => {
  const [tables, setTables] = useState<BusinessTableInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingTable, setEditingTable] = useState<BusinessTableInfo | null>(null);
  const [form] = Form.useForm();
  const [populatingFromSchema, setPopulatingFromSchema] = useState(false);
  const [schemaPopulationModalVisible, setSchemaPopulationModalVisible] = useState(false);

  useEffect(() => {
    loadTables();
  }, []);

  const loadTables = useCallback(async () => {
    try {
      setLoading(true);
      console.log('🔍 Loading business tables from API...');
      const data = await tuningApi.getBusinessTables();
      console.log('📊 Received business tables data:', data);
      console.log('📊 Number of tables:', data?.length || 0);
      if (data && data.length > 0) {
        console.log('📊 First table sample:', data[0]);
      }
      setTables(data);
    } catch (error) {
      message.error('Failed to load business tables');
      console.error('❌ Error loading tables:', error);
    } finally {
      setLoading(false);
    }
  }, []);

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
      businessOwner: table.businessOwner,
      domainClassification: table.domainClassification,
      naturalLanguageDescription: table.naturalLanguageDescription,
      naturalLanguageAliases: table.naturalLanguageAliases,
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
        businessOwner: values.businessOwner || '',
        domainClassification: values.domainClassification || '',
        naturalLanguageDescription: values.naturalLanguageDescription || '',
        naturalLanguageAliases: values.naturalLanguageAliases || [],
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
      title: 'Table Information',
      dataIndex: 'tableName',
      key: 'tableName',
      width: 250,
      render: (text: string, record: BusinessTableInfo) => (
        <Space direction="vertical" size="small">
          <Text strong>{record.schemaName}.{text}</Text>
          {record.businessFriendlyName && (
            <Text type="secondary" style={{ fontSize: '12px', fontStyle: 'italic' }}>
              {record.businessFriendlyName}
            </Text>
          )}
          <Space size="small">
            {record.domainClassification && (
              <Tag color="blue" style={{ fontSize: '10px' }}>
                {record.domainClassification}
              </Tag>
            )}
            {record.importanceScore && (
              <Tag color={record.importanceScore > 0.7 ? 'green' : record.importanceScore > 0.4 ? 'orange' : 'red'} style={{ fontSize: '10px' }}>
                Score: {(record.importanceScore * 100).toFixed(0)}%
              </Tag>
            )}
          </Space>
        </Space>
      ),
    },
    {
      title: 'Business Purpose',
      dataIndex: 'businessPurpose',
      key: 'businessPurpose',
      ellipsis: true,
      width: 300,
    },
    {
      title: 'Owner & Governance',
      key: 'governance',
      width: 200,
      render: (record: BusinessTableInfo) => (
        <Space direction="vertical" size="small">
          {record.businessOwner && (
            <Text style={{ fontSize: '12px' }}>
              <strong>Owner:</strong> {record.businessOwner}
            </Text>
          )}
          {record.dataGovernanceLevel && (
            <Tag color="purple" style={{ fontSize: '10px' }}>
              {record.dataGovernanceLevel}
            </Tag>
          )}
          {record.lastBusinessReview && (
            <Text type="secondary" style={{ fontSize: '11px' }}>
              Last Review: {new Date(record.lastBusinessReview).toLocaleDateString()}
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Usage & Quality',
      key: 'metrics',
      width: 150,
      render: (record: BusinessTableInfo) => (
        <Space direction="vertical" size="small">
          {record.usageFrequency !== undefined && (
            <div>
              <Text style={{ fontSize: '11px' }}>Usage: </Text>
              <Tag color={record.usageFrequency > 0.7 ? 'green' : record.usageFrequency > 0.4 ? 'orange' : 'red'} style={{ fontSize: '10px' }}>
                {(record.usageFrequency * 100).toFixed(0)}%
              </Tag>
            </div>
          )}
          {record.semanticCoverageScore !== undefined && (
            <div>
              <Text style={{ fontSize: '11px' }}>Coverage: </Text>
              <Tag color={record.semanticCoverageScore > 0.8 ? 'green' : record.semanticCoverageScore > 0.6 ? 'orange' : 'red'} style={{ fontSize: '10px' }}>
                {(record.semanticCoverageScore * 100).toFixed(0)}%
              </Tag>
            </div>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: any, record: BusinessTableInfo) => (
        <Space size="middle">
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            size="small"
            title="Edit table"
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
              title="Delete table"
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Alert
        message="Business Table Metadata Management"
        description={
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text>
              This interface manages business metadata stored in <code>[BIReportingCopilot_Dev].[dbo].[BusinessTableInfo]</code>
            </Text>
            <Text type="secondary">
              View and edit business context, governance information, and semantic metadata for database tables.
              Data includes business purpose, ownership, quality metrics, and natural language descriptions.
            </Text>
          </Space>
        }
        type="info"
        showIcon
        style={{ marginBottom: '16px' }}
      />

      <Card
        title={
          <Space size="middle">
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
                  <Col span={8}>
                    <Title level={5}>Business Context</Title>
                    <Text>{record.businessContext || 'No context provided'}</Text>

                    {record.naturalLanguageDescription && (
                      <>
                        <Title level={5} style={{ marginTop: '16px' }}>Natural Language Description</Title>
                        <Text>{record.naturalLanguageDescription}</Text>
                      </>
                    )}
                  </Col>
                  <Col span={8}>
                    <Title level={5}>Business Rules</Title>
                    <Text>{record.businessRules || 'No rules defined'}</Text>

                    {record.businessProcesses && record.businessProcesses.length > 0 && (
                      <>
                        <Title level={5} style={{ marginTop: '16px' }}>Business Processes</Title>
                        <Space wrap>
                          {record.businessProcesses.map((process, idx) => (
                            <Tag key={idx} color="blue">{process}</Tag>
                          ))}
                        </Space>
                      </>
                    )}
                  </Col>
                  <Col span={8}>
                    <Title level={5}>Governance & Quality</Title>
                    {record.dataGovernancePolicies && record.dataGovernancePolicies.length > 0 && (
                      <div style={{ marginBottom: '8px' }}>
                        <Text strong>Policies: </Text>
                        <Space wrap>
                          {record.dataGovernancePolicies.map((policy, idx) => (
                            <Tag key={idx} color="purple">{policy}</Tag>
                          ))}
                        </Space>
                      </div>
                    )}

                    {record.analyticalUseCases && record.analyticalUseCases.length > 0 && (
                      <div style={{ marginBottom: '8px' }}>
                        <Text strong>Use Cases: </Text>
                        <Space wrap>
                          {record.analyticalUseCases.map((useCase, idx) => (
                            <Tag key={idx} color="green">{useCase}</Tag>
                          ))}
                        </Space>
                      </div>
                    )}

                    {record.reportingCategories && record.reportingCategories.length > 0 && (
                      <div>
                        <Text strong>Reporting: </Text>
                        <Space wrap>
                          {record.reportingCategories.map((category, idx) => (
                            <Tag key={idx} color="orange">{category}</Tag>
                          ))}
                        </Space>
                      </div>
                    )}
                  </Col>
                </Row>

                {record.naturalLanguageAliases && record.naturalLanguageAliases.length > 0 && (
                  <>
                    <Divider />
                    <Title level={5}>Natural Language Aliases</Title>
                    <Space wrap>
                      {record.naturalLanguageAliases.map((alias, idx) => (
                        <Tag key={idx} color="cyan">{alias}</Tag>
                      ))}
                    </Space>
                  </>
                )}

                {record.llmContextHints && record.llmContextHints.length > 0 && (
                  <>
                    <Divider />
                    <Title level={5}>LLM Context Hints</Title>
                    <ul style={{ margin: 0, paddingLeft: '20px' }}>
                      {record.llmContextHints.map((hint, idx) => (
                        <li key={idx}><Text>{hint}</Text></li>
                      ))}
                    </ul>
                  </>
                )}

                {record.columns && record.columns.length > 0 && (
                  <>
                    <Divider />
                    <Title level={5}>Column Documentation</Title>
                    <Collapse size="small">
                      {record.columns.map((column, index) => (
                        <Panel
                          header={
                            <Space size="middle">
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
                            {column.dataExamples && column.dataExamples.length > 0 && (
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
        width={1000}
        destroyOnClose
        style={{ top: 20 }}
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

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="businessOwner"
                label="Business Owner"
              >
                <Input placeholder="e.g., Data Analytics Team, Finance Department" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="domainClassification"
                label="Domain Classification"
              >
                <Select placeholder="Select domain classification">
                  <Select.Option value="Reference">Reference</Select.Option>
                  <Select.Option value="Transactional">Transactional</Select.Option>
                  <Select.Option value="Analytical">Analytical</Select.Option>
                  <Select.Option value="Master">Master</Select.Option>
                  <Select.Option value="Staging">Staging</Select.Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="naturalLanguageDescription"
            label="Natural Language Description"
          >
            <TextArea
              rows={2}
              placeholder="Human-friendly description of what this table contains and how it's used..."
            />
          </Form.Item>

          <Form.Item
            name="naturalLanguageAliases"
            label="Natural Language Aliases"
          >
            <Select
              mode="tags"
              placeholder="Add alternative names (e.g., 'Countries', 'Jurisdictions', 'Geographic Data')"
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Divider orientation="left">
            <Space size="middle">
              <ColumnHeightOutlined />
              <span>Column Documentation</span>
            </Space>
          </Divider>

          <Form.List name="columns">
            {(fields, { add, remove }) => (
              <>
                {fields.map(({ key, name, ...restField }) => (
                  <Card
                    key={key}
                    size="small"
                    style={{ marginBottom: 16 }}
                    title={
                      <Space size="middle">
                        <ColumnHeightOutlined />
                        <span>Column {name + 1}</span>
                      </Space>
                    }
                    extra={
                      <Button
                        type="link"
                        danger
                        icon={<MinusCircleOutlined />}
                        onClick={() => remove(name)}
                        size="small"
                      >
                        Remove
                      </Button>
                    }
                  >
                    <Row gutter={16}>
                      <Col span={12}>
                        <Form.Item
                          {...restField}
                          name={[name, 'columnName']}
                          label="Column Name"
                          rules={[{ required: true, message: 'Please enter column name' }]}
                        >
                          <Input placeholder="e.g., Balance, Status, DepositCode" />
                        </Form.Item>
                      </Col>
                      <Col span={12}>
                        <Form.Item
                          {...restField}
                          name={[name, 'businessMeaning']}
                          label="Business Meaning"
                          rules={[{ required: true, message: 'Please enter business meaning' }]}
                        >
                          <Input placeholder="e.g., Player account balance, Bonus status" />
                        </Form.Item>
                      </Col>
                    </Row>

                    <Form.Item
                      {...restField}
                      name={[name, 'businessContext']}
                      label="Business Context"
                    >
                      <TextArea
                        rows={2}
                        placeholder="Detailed explanation of how this column is used in business processes..."
                      />
                    </Form.Item>

                    <Row gutter={16}>
                      <Col span={12}>
                        <Form.Item
                          {...restField}
                          name={[name, 'dataExamples']}
                          label="Data Examples"
                        >
                          <Select
                            mode="tags"
                            placeholder="Add example values (e.g., 100.50, Active, BONUS123)"
                            style={{ width: '100%' }}
                          />
                        </Form.Item>
                      </Col>
                      <Col span={12}>
                        <Form.Item
                          {...restField}
                          name={[name, 'validationRules']}
                          label="Validation Rules"
                        >
                          <Input placeholder="e.g., Must be positive, Required field" />
                        </Form.Item>
                      </Col>
                    </Row>

                    <Form.Item
                      {...restField}
                      name={[name, 'isKeyColumn']}
                      label="Key Column"
                      valuePropName="checked"
                    >
                      <Switch
                        checkedChildren={<span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}><KeyOutlined /></span>}
                        unCheckedChildren="No"
                      />
                    </Form.Item>
                  </Card>
                ))}

                <Form.Item>
                  <Button
                    type="dashed"
                    onClick={() => add()}
                    block
                    icon={<PlusOutlined />}
                  >
                    Add Column Documentation
                  </Button>
                </Form.Item>
              </>
            )}
          </Form.List>
        </Form>
      </Modal>
    </div>
  );
};
