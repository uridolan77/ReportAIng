import React, { useState, useEffect } from 'react';
import {
  Card,
  Tabs,
  Button,
  Badge,
  Row,
  Col,
  Typography,
  Space,
  Statistic,
  Spin,
  Empty,
  Divider,
  Switch
} from 'antd';
import {
  PlusOutlined,
  SettingOutlined,
  HistoryOutlined,
  DownloadOutlined,
  UploadOutlined,
  DatabaseOutlined,
  ReloadOutlined,
  EyeOutlined,
  EyeInvisibleOutlined
} from '@ant-design/icons';
import { SchemaList } from './SchemaList';
import { SchemaEditor } from './SchemaEditor';
import { SchemaVersions } from './SchemaVersions';
import { SchemaComparison } from './SchemaComparison';
import { CreateSchemaDialog } from './CreateSchemaDialog';
import { ImportSchemaDialog } from './ImportSchemaDialog';
import { DatabaseSchemaViewer } from './DatabaseSchemaViewer';
import { schemaManagementApi } from '../../services/schemaManagementApi';
import { ApiService } from '../../services/api';
import { BusinessSchemaDto, BusinessSchemaVersionDto } from '../../types/schemaManagement';
import './SchemaManagement.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

export const SchemaManagementDashboard: React.FC = () => {
  const [schemas, setSchemas] = useState<BusinessSchemaDto[]>([]);
  const [selectedSchema, setSelectedSchema] = useState<BusinessSchemaDto | null>(null);
  const [selectedVersion, setSelectedVersion] = useState<BusinessSchemaVersionDto | null>(null);
  const [activeTab, setActiveTab] = useState('overview');
  const [loading, setLoading] = useState(true);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showImportDialog, setShowImportDialog] = useState(false);

  // Database schema state
  const [databaseSchema, setDatabaseSchema] = useState<any>(null);
  const [databaseLoading, setDatabaseLoading] = useState(false);
  const [showDatabaseSchema, setShowDatabaseSchema] = useState(true);
  const [mainTab, setMainTab] = useState('business-schemas');

  useEffect(() => {
    loadSchemas();
    loadDatabaseSchema();
  }, []);

  const loadSchemas = async () => {
    try {
      setLoading(true);
      const data = await schemaManagementApi.getSchemas();
      setSchemas(data);
      
      // Select default schema if available
      const defaultSchema = data.find(s => s.isDefault);
      if (defaultSchema && !selectedSchema) {
        setSelectedSchema(defaultSchema);
        if (defaultSchema.currentVersion) {
          setSelectedVersion(defaultSchema.currentVersion);
        }
      }
    } catch (error) {
      console.error('Error loading schemas:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadDatabaseSchema = async () => {
    try {
      setDatabaseLoading(true);
      const schema = await ApiService.getSchema();
      setDatabaseSchema(schema);
    } catch (error) {
      console.error('Error loading database schema:', error);
    } finally {
      setDatabaseLoading(false);
    }
  };

  const refreshDatabaseSchema = async () => {
    try {
      setDatabaseLoading(true);
      // Call refresh endpoint
      await fetch('/api/query/refresh-schema', { method: 'POST' });
      // Reload schema
      await loadDatabaseSchema();
    } catch (error) {
      console.error('Error refreshing database schema:', error);
    }
  };

  const handleSchemaCreated = (newSchema: BusinessSchemaDto) => {
    setSchemas(prev => [...prev, newSchema]);
    setSelectedSchema(newSchema);
    setShowCreateDialog(false);
  };

  const handleSchemaUpdated = (updatedSchema: BusinessSchemaDto) => {
    setSchemas(prev => prev.map(s => s.id === updatedSchema.id ? updatedSchema : s));
    if (selectedSchema?.id === updatedSchema.id) {
      setSelectedSchema(updatedSchema);
    }
  };

  const handleSchemaDeleted = (schemaId: string) => {
    setSchemas(prev => prev.filter(s => s.id !== schemaId));
    if (selectedSchema?.id === schemaId) {
      setSelectedSchema(null);
      setSelectedVersion(null);
    }
  };

  const handleSetDefault = async (schemaId: string) => {
    try {
      await schemaManagementApi.setDefaultSchema(schemaId);
      await loadSchemas(); // Reload to update default status
    } catch (error) {
      console.error('Error setting default schema:', error);
    }
  };

  const handleExportSchema = async (versionId: string) => {
    try {
      const schemaData = await schemaManagementApi.exportSchemaVersion(versionId);
      const blob = new Blob([JSON.stringify(schemaData, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `schema-${schemaData.displayName}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error exporting schema:', error);
    }
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
        <Spin size="large" tip="Loading schemas..." />
      </div>
    );
  }

  return (
    <div className="schema-management-container">
      {/* Header */}
      <div className="schema-management-header">
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={2} style={{ margin: 0, color: '#1f2937' }}>
              Schema Management
            </Title>
            <Text type="secondary" style={{ fontSize: '16px' }}>
              Manage business context schemas with versioning and collaboration
            </Text>
            <div style={{ marginTop: '8px' }}>
              <Space>
                <Badge
                  status={databaseSchema ? "processing" : "error"}
                  text={databaseSchema ? "Database Connected" : "Database Disconnected"}
                />
                <Badge
                  status={schemas.length > 0 ? "success" : "default"}
                  text={`${schemas.length} Business Schema${schemas.length !== 1 ? 's' : ''}`}
                />
              </Space>
            </div>
          </Col>
          <Col>
            <Space>
              <Switch
                checked={showDatabaseSchema}
                onChange={setShowDatabaseSchema}
                checkedChildren={<EyeOutlined />}
                unCheckedChildren={<EyeInvisibleOutlined />}
              />
              <Text type="secondary">Show Database Schema</Text>
            </Space>
          </Col>
        </Row>
      </div>

      {/* Stats Cards */}
      <Row gutter={[16, 16]} className="schema-stats-row">
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Business Schemas"
              value={schemas.length}
              prefix={<SettingOutlined style={{ color: '#1890ff' }} />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Database Tables"
              value={databaseSchema?.tables?.length || 0}
              prefix={<DatabaseOutlined style={{ color: '#52c41a' }} />}
              suffix={databaseLoading ? <Spin size="small" /> : null}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Versions"
              value={schemas.reduce((sum, s) => sum + s.totalVersions, 0)}
              prefix={<HistoryOutlined style={{ color: '#722ed1' }} />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <div>
              <Text type="secondary" style={{ fontSize: '14px' }}>Database</Text>
              <div style={{ marginTop: '8px' }}>
                <DatabaseOutlined style={{ color: '#fa8c16', marginRight: '8px' }} />
                <Text strong>{databaseSchema?.databaseName || 'Not Connected'}</Text>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Main Content */}
      <Card style={{ marginTop: '24px' }}>
        <Tabs
          activeKey={mainTab}
          onChange={setMainTab}
          type="card"
          tabBarExtraContent={
            <Space>
              {mainTab === 'database-schema' && (
                <Button
                  icon={<ReloadOutlined />}
                  onClick={refreshDatabaseSchema}
                  loading={databaseLoading}
                  size="small"
                >
                  Refresh
                </Button>
              )}
              {mainTab === 'business-schemas' && (
                <Space>
                  <Button
                    icon={<UploadOutlined />}
                    onClick={() => setShowImportDialog(true)}
                    size="small"
                  >
                    Import
                  </Button>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={() => setShowCreateDialog(true)}
                    size="small"
                  >
                    New Schema
                  </Button>
                </Space>
              )}
            </Space>
          }
        >
          <TabPane
            tab={
              <Space>
                <SettingOutlined />
                <span>Business Schemas</span>
                <Badge count={schemas.length} style={{ backgroundColor: '#52c41a' }} />
              </Space>
            }
            key="business-schemas"
          >
            <Row gutter={[24, 24]} className="schema-main-content">
              {/* Schema List */}
              <Col xs={24} lg={8}>
                <Card
                  title="Business Schemas"
                  className="schema-list-card"
                >
                  <SchemaList
                    schemas={schemas}
                    selectedSchema={selectedSchema}
                    onSchemaSelect={setSelectedSchema}
                    onSchemaUpdated={handleSchemaUpdated}
                    onSchemaDeleted={handleSchemaDeleted}
                    onSetDefault={handleSetDefault}
                  />
                </Card>
              </Col>

              {/* Schema Details */}
              <Col xs={24} lg={16}>
                {selectedSchema ? (
                  <Card className="schema-details-card">
                    <Tabs activeKey={activeTab} onChange={setActiveTab} type="card">
                      <TabPane tab="Overview" key="overview">
                        <div className="schema-overview-content">
                          <div className="schema-overview-header">
                            <Title level={4} style={{ margin: 0 }}>{selectedSchema.name}</Title>
                            <Space>
                              {selectedSchema.isDefault && (
                                <Badge status="success" text="Default" />
                              )}
                              <Badge
                                status={selectedSchema.isActive ? "processing" : "default"}
                                text={selectedSchema.isActive ? "Active" : "Inactive"}
                              />
                            </Space>
                          </div>

                          <Divider />

                          <div className="schema-overview-section">
                            <Title level={5}>Description</Title>
                            <Text>{selectedSchema.description || 'No description provided'}</Text>
                          </div>

                          <Row gutter={[16, 16]} className="schema-overview-grid">
                            <Col span={12}>
                              <Title level={5}>Created By</Title>
                              <Text>{selectedSchema.createdBy}</Text>
                            </Col>
                            <Col span={12}>
                              <Title level={5}>Created At</Title>
                              <Text>{new Date(selectedSchema.createdAt).toLocaleDateString()}</Text>
                            </Col>
                          </Row>

                          <div className="schema-overview-section">
                            <Title level={5}>Tags</Title>
                            <Space wrap>
                              {selectedSchema.tags.map((tag, index) => (
                                <Badge key={index} count={tag} style={{ backgroundColor: '#f0f0f0', color: '#666' }} />
                              ))}
                            </Space>
                          </div>

                          <Button
                            icon={<DownloadOutlined />}
                            onClick={() => selectedSchema.currentVersion && handleExportSchema(selectedSchema.currentVersion.id)}
                          >
                            Export Schema
                          </Button>
                        </div>
                      </TabPane>

                      <TabPane tab="Versions" key="versions">
                        <SchemaVersions
                          schema={selectedSchema}
                          selectedVersion={selectedVersion}
                          onVersionSelect={setSelectedVersion}
                          onVersionUpdated={loadSchemas}
                        />
                      </TabPane>

                      <TabPane tab="Editor" key="editor">
                        <SchemaEditor
                          schema={selectedSchema}
                          version={selectedVersion}
                          onSchemaUpdated={handleSchemaUpdated}
                        />
                      </TabPane>

                      <TabPane tab="Compare" key="compare">
                        <SchemaComparison
                          schema={selectedSchema}
                          currentVersion={selectedVersion}
                        />
                      </TabPane>
                    </Tabs>
                  </Card>
                ) : (
                  <Card style={{ height: '600px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Empty
                      image={<SettingOutlined style={{ fontSize: '64px', color: '#d9d9d9' }} />}
                      description={
                        <div>
                          <Title level={4}>No Schema Selected</Title>
                          <Text type="secondary">Select a schema from the list to view its details and manage versions.</Text>
                        </div>
                      }
                    >
                      <Button type="primary" onClick={() => setShowCreateDialog(true)}>
                        Create New Schema
                      </Button>
                    </Empty>
                  </Card>
                )}
              </Col>
            </Row>
          </TabPane>

          <TabPane
            tab={
              <Space>
                <DatabaseOutlined />
                <span>Database Schema</span>
                <Badge count={databaseSchema?.tables?.length || 0} style={{ backgroundColor: '#1890ff' }} />
              </Space>
            }
            key="database-schema"
          >
            <DatabaseSchemaViewer
              schema={databaseSchema}
              loading={databaseLoading}
              onRefresh={refreshDatabaseSchema}
            />
          </TabPane>
        </Tabs>
      </Card>

      {/* Dialogs */}
      <CreateSchemaDialog
        open={showCreateDialog}
        onOpenChange={setShowCreateDialog}
        onSchemaCreated={handleSchemaCreated}
      />

      <ImportSchemaDialog
        open={showImportDialog}
        onOpenChange={setShowImportDialog}
        schemas={schemas}
        onSchemaImported={loadSchemas}
      />
    </div>
  );
};
