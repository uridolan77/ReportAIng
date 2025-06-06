import React, { useState } from 'react';
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Input,
  Switch,
  Space,
  message,
  Popconfirm,
  Tag,
  Typography,
  Row,
  Col,
  Statistic,
  Tooltip,
  Alert,
  Select
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  FileTextOutlined,
  PlayCircleOutlined,
  CheckCircleOutlined,
  StopOutlined,
  CopyOutlined,
  HistoryOutlined,
  BugOutlined
} from '@ant-design/icons';
import { PromptTemplate, CreatePromptTemplateRequest } from '../../services/tuningApi';
import {
  usePromptTemplates,
  useCreatePromptTemplate,
  useUpdatePromptTemplate,
  useDeletePromptTemplate,
  useActivatePromptTemplate,
  useDeactivatePromptTemplate,
  useTestPromptTemplate
} from '../../hooks/useTuningApi';

const { Text, Title } = Typography;
const { TextArea } = Input;

interface PromptTemplateManagerProps {
  onDataChange?: () => void;
}

export const PromptTemplateManager: React.FC<PromptTemplateManagerProps> = ({ onDataChange }) => {
  const [modalVisible, setModalVisible] = useState(false);
  const [testModalVisible, setTestModalVisible] = useState(false);
  const [editingTemplate, setEditingTemplate] = useState<PromptTemplate | null>(null);
  const [testingTemplate, setTestingTemplate] = useState<PromptTemplate | null>(null);
  const [form] = Form.useForm();
  const [testForm] = Form.useForm();

  // React Query hooks
  const { data: templates = [], isLoading: loading, refetch } = usePromptTemplates();
  const createMutation = useCreatePromptTemplate();
  const updateMutation = useUpdatePromptTemplate();
  const deleteMutation = useDeletePromptTemplate();
  const activateMutation = useActivatePromptTemplate();
  const deactivateMutation = useDeactivatePromptTemplate();
  const testMutation = useTestPromptTemplate();

  const handleCreate = () => {
    setEditingTemplate(null);
    form.resetFields();
    form.setFieldsValue({
      version: '1.0',
      isActive: true,
      parameters: {}
    });
    setModalVisible(true);
  };

  const handleEdit = (template: PromptTemplate) => {
    setEditingTemplate(template);
    form.setFieldsValue({
      name: template.name,
      version: template.version,
      content: template.content,
      description: template.description,
      isActive: template.isActive,
      parameters: JSON.stringify(template.parameters, null, 2)
    });
    setModalVisible(true);
  };

  const handleCreateNewVersion = (template: PromptTemplate) => {
    setEditingTemplate(null);
    const versionParts = template.version.split('.');
    const majorVersion = parseInt(versionParts[0]);
    const minorVersion = parseInt(versionParts[1] || '0');
    const newVersion = `${majorVersion}.${minorVersion + 1}`;
    
    form.resetFields();
    form.setFieldsValue({
      name: template.name,
      version: newVersion,
      content: template.content,
      description: template.description,
      isActive: false, // New versions start inactive
      parameters: JSON.stringify(template.parameters, null, 2)
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteMutation.mutateAsync(id);
      message.success('Template deleted successfully');
      onDataChange?.();
    } catch (error) {
      message.error('Failed to delete template');
      console.error('Error deleting template:', error);
    }
  };

  const handleActivate = async (id: number) => {
    try {
      await activateMutation.mutateAsync(id);
      message.success('Template activated successfully');
      onDataChange?.();
    } catch (error) {
      message.error('Failed to activate template');
      console.error('Error activating template:', error);
    }
  };

  const handleDeactivate = async (id: number) => {
    try {
      await deactivateMutation.mutateAsync(id);
      message.success('Template deactivated successfully');
      onDataChange?.();
    } catch (error) {
      message.error('Failed to deactivate template');
      console.error('Error deactivating template:', error);
    }
  };

  const handleTest = (template: PromptTemplate) => {
    setTestingTemplate(template);
    testForm.resetFields();
    setTestModalVisible(true);
  };

  const handleSubmit = async (values: any) => {
    try {
      let parameters = {};
      if (values.parameters) {
        try {
          parameters = JSON.parse(values.parameters);
        } catch (e) {
          message.error('Invalid JSON in parameters field');
          return;
        }
      }

      const request: CreatePromptTemplateRequest = {
        name: values.name,
        version: values.version,
        content: values.content,
        description: values.description || '',
        isActive: values.isActive,
        parameters
      };

      if (editingTemplate?.id) {
        await updateMutation.mutateAsync({ id: editingTemplate.id, template: request });
        message.success('Template updated successfully');
      } else {
        await createMutation.mutateAsync(request);
        message.success('Template created successfully');
      }

      setModalVisible(false);
      onDataChange?.();
    } catch (error) {
      message.error('Failed to save template');
      console.error('Error saving template:', error);
    }
  };

  const handleTestSubmit = async (values: any) => {
    if (!testingTemplate?.id) return;

    try {
      const result = await testMutation.mutateAsync({ id: testingTemplate.id, testData: values });
      message.success('Template test completed successfully');
      console.log('Test result:', result);
      // You could show the result in a separate modal or section
    } catch (error) {
      message.error('Template test failed');
      console.error('Error testing template:', error);
    }
  };

  // Group templates by name for version management
  const groupedTemplates = templates.reduce((acc, template) => {
    if (!acc[template.name]) {
      acc[template.name] = [];
    }
    acc[template.name].push(template);
    return acc;
  }, {} as Record<string, PromptTemplate[]>);

  // Flatten for table display but show version info
  const tableData = Object.entries(groupedTemplates).map(([name, versions]) => {
    const activeVersion = versions.find(v => v.isActive);
    const latestVersion = versions.sort((a, b) => 
      new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime()
    )[0];
    
    return {
      key: name,
      name,
      versions,
      activeVersion,
      latestVersion,
      totalUsage: versions.reduce((sum, v) => sum + v.usageCount, 0),
      avgSuccessRate: versions.reduce((sum, v) => sum + (v.successRate || 0), 0) / versions.length
    };
  });

  const columns = [
    {
      title: 'Template Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: any) => (
        <Space direction="vertical" size="small">
          <Text strong>{text}</Text>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.versions.length} version{record.versions.length !== 1 ? 's' : ''}
          </Text>
        </Space>
      ),
    },
    {
      title: 'Active Version',
      key: 'activeVersion',
      render: (_: any, record: any) => (
        record.activeVersion ? (
          <Tag color="green">v{record.activeVersion.version}</Tag>
        ) : (
          <Tag color="red">No Active Version</Tag>
        )
      ),
    },
    {
      title: 'Latest Version',
      key: 'latestVersion',
      render: (_: any, record: any) => (
        <Tag color="blue">v{record.latestVersion.version}</Tag>
      ),
    },
    {
      title: 'Usage Stats',
      key: 'stats',
      render: (_: any, record: any) => (
        <Space direction="vertical" size="small">
          <Text style={{ fontSize: '12px' }}>
            {record.totalUsage} total uses
          </Text>
          {record.avgSuccessRate > 0 && (
            <Text style={{ fontSize: '12px' }}>
              {record.avgSuccessRate.toFixed(1)}% success rate
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      render: (_: any, record: any) => (
        <Space wrap>
          <Tooltip title="Edit Latest Version">
            <Button
              type="link"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record.latestVersion)}
              size="small"
            />
          </Tooltip>
          <Tooltip title="Create New Version">
            <Button
              type="link"
              icon={<CopyOutlined />}
              onClick={() => handleCreateNewVersion(record.latestVersion)}
              size="small"
            />
          </Tooltip>
          <Tooltip title="Test Template">
            <Button
              type="link"
              icon={<BugOutlined />}
              onClick={() => handleTest(record.activeVersion || record.latestVersion)}
              size="small"
            />
          </Tooltip>
          <Tooltip title="Version History">
            <Button
              type="link"
              icon={<HistoryOutlined />}
              size="small"
              // Could open a version history modal
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card
        title={
          <Space>
            <FileTextOutlined />
            <span>Prompt Template Management</span>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleCreate}
          >
            Create Template
          </Button>
        }
      >
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={8}>
            <Statistic
              title="Total Templates"
              value={Object.keys(groupedTemplates).length}
              prefix={<FileTextOutlined />}
            />
          </Col>
          <Col xs={24} sm={8}>
            <Statistic
              title="Total Versions"
              value={templates.length}
              prefix={<HistoryOutlined />}
            />
          </Col>
          <Col xs={24} sm={8}>
            <Statistic
              title="Active Templates"
              value={templates.filter(t => t.isActive).length}
              prefix={<CheckCircleOutlined />}
            />
          </Col>
        </Row>

        <Table
          columns={columns}
          dataSource={tableData}
          loading={loading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
          }}
          expandable={{
            expandedRowRender: (record) => (
              <div style={{ margin: 0 }}>
                <Title level={5}>All Versions:</Title>
                {record.versions
                  .sort((a: PromptTemplate, b: PromptTemplate) => 
                    new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime()
                  )
                  .map((version: PromptTemplate) => (
                    <div key={version.id} style={{ marginBottom: 8, padding: 8, border: '1px solid #f0f0f0', borderRadius: 4 }}>
                      <Row justify="space-between" align="middle">
                        <Col>
                          <Space>
                            <Tag color={version.isActive ? 'green' : 'default'}>
                              v{version.version}
                            </Tag>
                            <Text type="secondary">
                              {new Date(version.createdDate).toLocaleDateString()}
                            </Text>
                            <Text type="secondary">by {version.createdBy}</Text>
                            {version.usageCount > 0 && (
                              <Text type="secondary">({version.usageCount} uses)</Text>
                            )}
                          </Space>
                        </Col>
                        <Col>
                          <Space>
                            {version.isActive ? (
                              <Button
                                size="small"
                                icon={<StopOutlined />}
                                onClick={() => version.id && handleDeactivate(version.id)}
                              >
                                Deactivate
                              </Button>
                            ) : (
                              <Button
                                size="small"
                                type="primary"
                                icon={<PlayCircleOutlined />}
                                onClick={() => version.id && handleActivate(version.id)}
                              >
                                Activate
                              </Button>
                            )}
                            <Button
                              size="small"
                              icon={<EditOutlined />}
                              onClick={() => handleEdit(version)}
                            >
                              Edit
                            </Button>
                            <Popconfirm
                              title="Are you sure you want to delete this version?"
                              onConfirm={() => version.id && handleDelete(version.id)}
                              okText="Yes"
                              cancelText="No"
                            >
                              <Button
                                size="small"
                                danger
                                icon={<DeleteOutlined />}
                              >
                                Delete
                              </Button>
                            </Popconfirm>
                          </Space>
                        </Col>
                      </Row>
                      {version.description && (
                        <div style={{ marginTop: 4 }}>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {version.description}
                          </Text>
                        </div>
                      )}
                    </div>
                  ))}
              </div>
            ),
            rowExpandable: (record) => record.versions.length > 1,
          }}
        />
      </Card>

      {/* Create/Edit Modal */}
      <Modal
        title={editingTemplate ? `Edit Template: ${editingTemplate.name}` : 'Create New Template'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={800}
        destroyOnHidden
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Row gutter={16}>
            <Col xs={24} sm={12}>
              <Form.Item
                name="name"
                label="Template Name"
                rules={[{ required: true, message: 'Please enter template name' }]}
              >
                <Input placeholder="e.g., sql_generation, insight_analysis" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item
                name="version"
                label="Version"
                rules={[{ required: true, message: 'Please enter version' }]}
              >
                <Input placeholder="e.g., 1.0, 2.1" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="description"
            label="Description"
          >
            <Input.TextArea
              rows={2}
              placeholder="Brief description of what this template does..."
            />
          </Form.Item>

          <Form.Item
            name="content"
            label="Template Content"
            rules={[{ required: true, message: 'Please enter template content' }]}
          >
            <TextArea
              rows={12}
              placeholder="Enter the prompt template content with placeholders like {schema}, {question}, etc."
              style={{ fontFamily: 'monospace' }}
            />
          </Form.Item>

          <Form.Item
            name="parameters"
            label="Parameters (JSON)"
            help="Optional JSON object with template parameters and their default values"
          >
            <TextArea
              rows={4}
              placeholder='{"temperature": 0.7, "max_tokens": 1000}'
              style={{ fontFamily: 'monospace' }}
            />
          </Form.Item>

          <Form.Item
            name="isActive"
            label="Active"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>

          {editingTemplate && (
            <Alert
              message="Template Information"
              description={
                <Space direction="vertical" size="small">
                  <Text>Created by: {editingTemplate.createdBy}</Text>
                  <Text>Created: {new Date(editingTemplate.createdDate).toLocaleString()}</Text>
                  <Text>Usage Count: {editingTemplate.usageCount}</Text>
                  {editingTemplate.successRate && (
                    <Text>Success Rate: {editingTemplate.successRate.toFixed(1)}%</Text>
                  )}
                </Space>
              }
              type="info"
              style={{ marginTop: 16 }}
            />
          )}
        </Form>
      </Modal>

      {/* Test Modal */}
      <Modal
        title={`Test Template: ${testingTemplate?.name} v${testingTemplate?.version}`}
        open={testModalVisible}
        onCancel={() => setTestModalVisible(false)}
        onOk={() => testForm.submit()}
        width={600}
      >
        <Form
          form={testForm}
          layout="vertical"
          onFinish={handleTestSubmit}
        >
          <Alert
            message="Template Testing"
            description="Provide test data to validate how the template performs with real inputs."
            type="info"
            style={{ marginBottom: 16 }}
          />

          <Form.Item
            name="question"
            label="Test Question"
            rules={[{ required: true, message: 'Please enter a test question' }]}
          >
            <TextArea
              rows={3}
              placeholder="Enter a natural language question to test the template..."
            />
          </Form.Item>

          <Form.Item
            name="schema"
            label="Test Schema (Optional)"
          >
            <TextArea
              rows={4}
              placeholder="Provide schema information for testing..."
              style={{ fontFamily: 'monospace' }}
            />
          </Form.Item>

          <Form.Item
            name="context"
            label="Additional Context (Optional)"
          >
            <TextArea
              rows={2}
              placeholder="Any additional context for the test..."
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
