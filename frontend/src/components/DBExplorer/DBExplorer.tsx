import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Typography,
  Space,
  Button,
  Alert,
  Spin,
  message,
  Drawer,
  Modal,
  Progress,
  List
} from 'antd';
import { Resizable } from 'react-resizable';
import './DBExplorer.css';
import {
  DatabaseOutlined,
  ReloadOutlined,
  QuestionCircleOutlined,
  FullscreenOutlined,
  CheckSquareOutlined,
  ClearOutlined,
  PlayCircleOutlined,
  SelectOutlined
} from '@ant-design/icons';
import { SchemaTree } from './SchemaTree';
import { TableExplorer } from './TableExplorer';
import { TableDataPreview } from './TableDataPreview';
import { ContentManagementModal } from './ContentManagementModal';
import { DatabaseTable, DatabaseSchema, DBExplorerState, AutoGenerationRequest } from '../../types/dbExplorer';
import DBExplorerAPI from '../../services/dbExplorerApi';
import { ApiService } from '../../services/api';
import { tuningApi } from '../../services/tuningApi';
import { useNavigate } from 'react-router-dom';

const { Title, Text } = Typography;

interface DBExplorerProps {
  onQueryGenerated?: (query: string) => void;
}

export const DBExplorer: React.FC<DBExplorerProps> = ({ onQueryGenerated }) => {
  const navigate = useNavigate();
  
  const [state, setState] = useState<DBExplorerState>({
    searchTerm: '',
    showDataPreview: false,
    expandedTables: new Set(),
    loading: false,
    selectedTables: new Set(),
    selectedFields: new Map(),
    selectionMode: false,
    autoGenerationInProgress: false
  });

  const [schema, setSchema] = useState<DatabaseSchema | null>(null);
  const [siderCollapsed, setSiderCollapsed] = useState(false);
  const [previewDrawerVisible, setPreviewDrawerVisible] = useState(false);
  const [siderWidth, setSiderWidth] = useState(550);
  const [progressModalVisible, setProgressModalVisible] = useState(false);
  const [generationResults, setGenerationResults] = useState<any>(null);
  const [contentModalVisible, setContentModalVisible] = useState(false);



  // Load database schema
  const loadSchema = useCallback(async () => {
    setState(prev => {
      const newState = { ...prev, loading: true };
      delete newState.error;
      return newState;
    });

    try {
      const rawSchemaData = await ApiService.getSchema();

      if (rawSchemaData && rawSchemaData.tables) {
        // Transform it to match our interface - use the data directly since it's already in the right format
        const transformedSchema: DatabaseSchema = {
          name: rawSchemaData.databaseName || 'Database',
          lastUpdated: rawSchemaData.lastUpdated || new Date().toISOString(),
          version: rawSchemaData.version || '1.0.0',
          views: rawSchemaData.views || [],
          tables: rawSchemaData.tables || []
        };



        setSchema(transformedSchema);
        message.success('Database schema loaded successfully');
      } else {
        throw new Error('No schema data received from API');
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to load database schema';
      setState(prev => ({ ...prev, error: errorMessage }));
      message.error(errorMessage);
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  }, []);

  // Load schema on component mount
  useEffect(() => {
    loadSchema();
  }, [loadSchema]);

  // Handle table selection
  const handleTableSelect = useCallback((table: DatabaseTable) => {
    setState(prev => ({
      ...prev,
      selectedTable: table,
      showDataPreview: false
    }));
  }, []);

  // Handle table preview
  const handleTablePreview = useCallback((table: DatabaseTable) => {
    setState(prev => ({ 
      ...prev, 
      selectedTable: table,
      showDataPreview: true 
    }));
    setPreviewDrawerVisible(true);
  }, []);

  // Handle query generation
  const handleQueryGenerated = useCallback((query: string) => {
    if (onQueryGenerated) {
      onQueryGenerated(query);
    } else {
      // Navigate to main query interface with the query using the correct state property
      navigate('/', { state: { suggestedQuery: query } });
      message.success('Query sent to main interface');
    }
  }, [onQueryGenerated, navigate]);

  // Handle expanded keys change
  const handleExpandedKeysChange = useCallback((keys: string[]) => {
    setState(prev => ({
      ...prev,
      expandedTables: new Set(keys)
    }));
  }, []);

  // Handle table selection for auto-generation
  const handleTableSelectionChange = useCallback((tableName: string, selected: boolean) => {
    setState(prev => {
      const newSelectedTables = new Set(prev.selectedTables);
      const newSelectedFields = new Map(prev.selectedFields);

      if (selected) {
        newSelectedTables.add(tableName);
      } else {
        newSelectedTables.delete(tableName);
        newSelectedFields.delete(tableName);
      }

      return {
        ...prev,
        selectedTables: newSelectedTables,
        selectedFields: newSelectedFields
      };
    });
  }, []);

  // Handle field selection for auto-generation
  const handleFieldSelectionChange = useCallback((tableName: string, fieldName: string, selected: boolean) => {
    setState(prev => {
      const newSelectedFields = new Map(prev.selectedFields);
      const tableFields = newSelectedFields.get(tableName) || new Set();

      if (selected) {
        tableFields.add(fieldName);
      } else {
        tableFields.delete(fieldName);
      }

      if (tableFields.size > 0) {
        newSelectedFields.set(tableName, tableFields);
      } else {
        newSelectedFields.delete(tableName);
      }

      return {
        ...prev,
        selectedFields: newSelectedFields
      };
    });
  }, []);

  // Toggle selection mode
  const handleToggleSelectionMode = useCallback(() => {
    setState(prev => ({
      ...prev,
      selectionMode: !prev.selectionMode,
      selectedTables: new Set(),
      selectedFields: new Map()
    }));
  }, []);

  // Select all tables
  const handleSelectAllTables = useCallback(() => {
    const currentTables = schema?.tables || [];
    setState(prev => ({
      ...prev,
      selectedTables: new Set(currentTables.map(t => t.name)),
      selectedFields: new Map()
    }));
  }, [schema]);

  // Clear all selections
  const handleClearSelections = useCallback(() => {
    setState(prev => ({
      ...prev,
      selectedTables: new Set(),
      selectedFields: new Map()
    }));
  }, []);

  // Get selection summary
  const getSelectionSummary = useCallback(() => {
    const selectedTableNames = Array.from(state.selectedTables);
    const tablesWithSelectedFields = Array.from(state.selectedFields.keys());
    const allSelectedTables = [...new Set([...selectedTableNames, ...tablesWithSelectedFields])];

    const totalTablesSelected = allSelectedTables.length;
    const totalFieldsSelected = Array.from(state.selectedFields.values())
      .reduce((sum, fields) => sum + fields.size, 0);

    return {
      totalTablesSelected,
      totalFieldsSelected,
      selectedTableNames: allSelectedTables,
      fieldsByTable: state.selectedFields
    };
  }, [state.selectedTables, state.selectedFields]);

  // Handle auto-generation
  const handleAutoGeneration = useCallback(async () => {
    if (state.selectedTables.size === 0 && state.selectedFields.size === 0) {
      message.warning('Please select tables or fields to generate business context for.');
      return;
    }

    setState(prev => ({ ...prev, autoGenerationInProgress: true }));
    setProgressModalVisible(true);
    setGenerationResults(null);

    try {
      const selectedTableNames = Array.from(state.selectedTables);

      // Add tables that have selected fields
      const tablesWithSelectedFields = Array.from(state.selectedFields.keys());
      const allSelectedTables = [...new Set([...selectedTableNames, ...tablesWithSelectedFields])];

      // Build specific fields map
      const specificFields: { [tableName: string]: string[] } = {};
      state.selectedFields.forEach((fieldSet, tableName) => {
        specificFields[tableName] = Array.from(fieldSet);
      });

      const request: AutoGenerationRequest = {
        generateTableContexts: true,
        generateGlossaryTerms: true,
        analyzeRelationships: true,
        specificTables: allSelectedTables,
        specificFields: Object.keys(specificFields).length > 0 ? specificFields : undefined,
        overwriteExisting: false,
        minimumConfidenceThreshold: 0.6,
        mockMode: false
      };

      console.log('🤖 Auto-generation request:', request);
      console.log('🤖 Selected tables:', allSelectedTables);
      console.log('🤖 Selected fields by table:', specificFields);

      const response = await tuningApi.autoGenerateBusinessContext(request);

      console.log('🤖 Auto-generation response:', response);
      console.log('🤖 Generated table contexts:', response.GeneratedTableContexts);
      console.log('🤖 Generated glossary terms:', response.GeneratedGlossaryTerms);

      setGenerationResults(response);

      if (response.success) {
        message.success(`Auto-generation completed! Generated contexts for ${response.totalTablesProcessed} tables and ${response.totalTermsGenerated} terms. Selections preserved for review.`);

        // Show content management modal
        setContentModalVisible(true);

        // Keep selections after successful generation so user can see what was processed
        // and potentially run generation again if needed
      } else {
        message.error('Auto-generation completed with errors. Check the results for details.');
      }
    } catch (error) {
      console.error('Auto-generation failed:', error);
      message.error('Failed to generate business context. Please try again.');
      setGenerationResults({
        success: false,
        errors: [error instanceof Error ? error.message : 'Unknown error occurred'],
        warnings: [],
        totalTablesProcessed: 0,
        totalTermsGenerated: 0
      });
    } finally {
      setState(prev => ({ ...prev, autoGenerationInProgress: false }));
    }
  }, [state.selectedTables, state.selectedFields]);

  // Refresh schema
  const handleRefresh = useCallback(async () => {
    try {
      console.log('🔄 Starting schema refresh...');
      await DBExplorerAPI.refreshSchema();
      console.log('🔄 Schema refresh completed, reloading...');
      await loadSchema();
      console.log('🔄 Schema reload completed');
    } catch (error) {
      console.error('🔄 Schema refresh failed:', error);
      message.error('Failed to refresh schema');
    }
  }, [loadSchema]);

  const tables = schema?.tables || [];

  return (
    <div className="db-explorer" style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Top Action Bar */}
      <div style={{
        padding: '16px 20px',
        borderBottom: '2px solid #e8e8e8',
        background: 'linear-gradient(135deg, #fafafa 0%, #f5f5f5 100%)',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        minHeight: '70px',
        boxShadow: '0 2px 4px rgba(0, 0, 0, 0.05)',
        zIndex: 10,
        position: 'relative'
      }}>
        <Space size={20}>
          <DatabaseOutlined style={{ fontSize: '18px', color: '#1890ff' }} />
          <Text strong style={{ fontSize: '16px' }}>Database Management</Text>
          <Text type="secondary" style={{ fontSize: '14px' }}>
            Schema browsing, table exploration, and data preview
          </Text>
          {!siderCollapsed && (
            <Text type="secondary" style={{ fontSize: '12px', fontStyle: 'italic' }}>
              • Drag sidebar edge to resize
            </Text>
          )}
        </Space>
        <Space size={16}>
          {/* Selection Mode Controls */}
          <Button
            icon={<SelectOutlined />}
            size="large"
            type={state.selectionMode ? 'primary' : 'default'}
            onClick={handleToggleSelectionMode}
            style={{
              fontSize: '14px',
              padding: '8px 20px',
              fontWeight: 600,
              height: '40px',
              borderRadius: '8px',
              boxShadow: state.selectionMode ? '0 2px 8px rgba(24, 144, 255, 0.3)' : '0 2px 4px rgba(0, 0, 0, 0.1)',
              border: state.selectionMode ? '2px solid #1890ff' : '2px solid #d9d9d9'
            }}
          >
            {state.selectionMode ? '✓ Exit Selection' : '📋 Select Mode'}
          </Button>

          {state.selectionMode && (
            <>
              <Button
                icon={<CheckSquareOutlined />}
                size="large"
                onClick={handleSelectAllTables}
                style={{
                  fontSize: '14px',
                  padding: '8px 16px',
                  height: '40px',
                  borderRadius: '8px',
                  fontWeight: 500
                }}
              >
                Select All
              </Button>
              <Button
                icon={<ClearOutlined />}
                size="large"
                onClick={handleClearSelections}
                style={{
                  fontSize: '14px',
                  padding: '8px 16px',
                  height: '40px',
                  borderRadius: '8px',
                  fontWeight: 500
                }}
              >
                Clear
              </Button>
              {(state.selectedTables.size > 0 || state.selectedFields.size > 0) && (() => {
                const summary = getSelectionSummary();
                return (
                  <Button
                    icon={<PlayCircleOutlined />}
                    size="large"
                    type="primary"
                    loading={state.autoGenerationInProgress}
                    onClick={handleAutoGeneration}
                    style={{
                      fontSize: '14px',
                      padding: '8px 20px',
                      height: '40px',
                      borderRadius: '8px',
                      fontWeight: 600,
                      boxShadow: '0 2px 8px rgba(24, 144, 255, 0.3)'
                    }}
                  >
                    🚀 Generate ({summary.totalTablesSelected} tables, {summary.totalFieldsSelected} fields)
                  </Button>
                );
              })()}
            </>
          )}

          <Button
            icon={<QuestionCircleOutlined />}
            size="middle"
            type="text"
            onClick={() => {
              message.info('Use the tree view to explore tables, click on tables to see details, and use the preview button to see sample data.');
            }}
            style={{ fontSize: '14px', padding: '6px 12px' }}
          />
          <Button
            icon={<ReloadOutlined />}
            size="middle"
            onClick={handleRefresh}
            loading={state.loading}
            style={{ fontSize: '14px', padding: '6px 16px' }}
          >
            Refresh
          </Button>
          <Button
            icon={<FullscreenOutlined />}
            size="middle"
            onClick={() => setSiderCollapsed(!siderCollapsed)}
            style={{ fontSize: '14px', padding: '6px 16px' }}
          >
            {siderCollapsed ? 'Show' : 'Hide'} Tree
          </Button>
        </Space>
      </div>

      {/* Error Alert */}
      {state.error && (
        <Alert
          style={{ margin: '0 16px 16px 16px' }}
          message="Error Loading Database Schema"
          description={state.error}
          type="error"
          showIcon
          closable
          action={
            <Button size="small" onClick={() => loadSchema()}>
              Retry
            </Button>
          }
        />
      )}

      {/* Main Content */}
      <div style={{ flex: 1, padding: '0 16px 16px 16px', display: 'flex', minHeight: 0 }}>
        <div style={{ display: 'flex', height: '100%', width: '100%', gap: '12px' }}>
          {/* Resizable Schema Tree Sidebar */}
          {!siderCollapsed && (
            <Resizable
              width={siderWidth}
              height={0}
              onResize={(_, { size }) => {
                setSiderWidth(size.width);
              }}
              minConstraints={[250, 0]}
              maxConstraints={[800, 0]}
              resizeHandles={['e']}
              className="db-explorer-resizable"
            >
              <div
                style={{
                  width: siderWidth,
                  height: '100%',
                  position: 'relative'
                }}
              >
                <SchemaTree
                  tables={tables}
                  loading={state.loading}
                  onTableSelect={handleTableSelect}
                  onPreviewTable={handleTablePreview}
                  onRefresh={handleRefresh}
                  selectedTableName={state.selectedTable?.name || ''}
                  expandedKeys={Array.from(state.expandedTables)}
                  onExpandedKeysChange={handleExpandedKeysChange}
                  selectionMode={state.selectionMode}
                  selectedTables={state.selectedTables}
                  selectedFields={state.selectedFields}
                  onTableSelectionChange={handleTableSelectionChange}
                  onFieldSelectionChange={handleFieldSelectionChange}
                />
                {/* Resize indicator */}
                <div
                  style={{
                    position: 'absolute',
                    right: -4,
                    top: 0,
                    bottom: 0,
                    width: '8px',
                    background: 'linear-gradient(90deg, transparent 25%, #d9d9d9 50%, transparent 75%)',
                    cursor: 'col-resize',
                    opacity: 0.6,
                    transition: 'all 0.2s ease',
                    zIndex: 10,
                    borderRadius: '2px'
                  }}
                  className="resize-indicator"
                  onMouseEnter={(e) => {
                    e.currentTarget.style.opacity = '1';
                    e.currentTarget.style.background = 'linear-gradient(90deg, transparent 15%, #1890ff 50%, transparent 85%)';
                    e.currentTarget.style.boxShadow = '0 0 4px rgba(24, 144, 255, 0.3)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.opacity = '0.6';
                    e.currentTarget.style.background = 'linear-gradient(90deg, transparent 25%, #d9d9d9 50%, transparent 75%)';
                    e.currentTarget.style.boxShadow = 'none';
                  }}
                />
              </div>
            </Resizable>
          )}

          {/* Main Content Area */}
          <div style={{ flex: 1, minWidth: 0 }}>
            {state.loading && !schema ? (
              <Card style={{ height: '100%', textAlign: 'center' }}>
                <div style={{ padding: '100px 0' }}>
                  <Spin size="large" />
                  <div style={{ marginTop: '16px' }}>
                    <Text type="secondary">Loading database schema...</Text>
                  </div>
                </div>
              </Card>
            ) : state.selectedTable ? (
              <TableExplorer
                table={state.selectedTable}
                onPreviewData={() => handleTablePreview(state.selectedTable!)}
                onGenerateQuery={handleQueryGenerated}
                selectionMode={state.selectionMode}
                selectedFields={state.selectedFields.get(state.selectedTable.name) || new Set()}
                onFieldSelectionChange={(fieldName, selected) =>
                  handleFieldSelectionChange(state.selectedTable!.name, fieldName, selected)
                }
              />
            ) : (
              <Card style={{ height: '100%', textAlign: 'center' }} bodyStyle={{ padding: '60px 30px' }}>
                <div>
                  <DatabaseOutlined style={{ fontSize: '64px', color: '#d9d9d9' }} />
                  <Title level={3} type="secondary" style={{ marginTop: '24px', fontSize: '18px', fontWeight: 500 }}>
                    Select a Table to Explore
                  </Title>
                  <Text type="secondary" style={{ fontSize: '14px', lineHeight: '1.5' }}>
                    Choose a table from the schema tree {siderCollapsed ? '(click "Show Tree" to expand)' : 'on the left'} to view its structure, columns, and sample data.
                  </Text>
                  {tables.length > 0 && (
                    <div style={{ marginTop: '20px', padding: '12px', background: '#f5f5f5', borderRadius: '6px', display: 'inline-block' }}>
                      <Text type="secondary" style={{ fontSize: '12px', fontWeight: 500 }}>
                        📊 Found {tables.length} tables in the database
                      </Text>
                    </div>
                  )}
                </div>
              </Card>
            )}
          </div>
        </div>
      </div>

      {/* Data Preview Drawer */}
      <Drawer
        title={`Data Preview: ${state.selectedTable?.name}`}
        placement="right"
        width="85%"
        open={previewDrawerVisible}
        onClose={() => setPreviewDrawerVisible(false)}
        destroyOnClose
        bodyStyle={{ padding: 0 }}
      >
        {state.selectedTable && (
          <TableDataPreview
            table={state.selectedTable}
            onClose={() => setPreviewDrawerVisible(false)}
          />
        )}
      </Drawer>

      {/* Auto-Generation Progress Modal */}
      <Modal
        title="Auto-Generation Progress"
        open={progressModalVisible}
        onCancel={() => setProgressModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setProgressModalVisible(false)}>
            Close
          </Button>,
          ...(generationResults && generationResults.success ? [
            <Button
              key="manage"
              type="primary"
              onClick={() => {
                setProgressModalVisible(false);
                setContentModalVisible(true);
              }}
            >
              Manage Generated Content
            </Button>
          ] : [])
        ]}
        width={600}
      >
        <Space direction="vertical" style={{ width: '100%' }} size="large">
          {state.autoGenerationInProgress ? (
            <>
              <div style={{ textAlign: 'center' }}>
                <Spin size="large" />
                <div style={{ marginTop: '16px' }}>
                  <Text>Generating business context and schema data...</Text>
                </div>
              </div>
              <Progress percent={50} status="active" />
              <Text type="secondary">
                Processing selected tables and fields. This may take a few minutes.
              </Text>
            </>
          ) : generationResults ? (
            <>
              <div style={{ textAlign: 'center' }}>
                <Title level={4} type={generationResults.success ? 'success' : 'danger'}>
                  {generationResults.success ? '✅ Generation Completed' : '❌ Generation Failed'}
                </Title>
              </div>

              <Space direction="vertical" style={{ width: '100%' }}>
                <Text><strong>Tables Processed:</strong> {generationResults.totalTablesProcessed}</Text>
                <Text><strong>Terms Generated:</strong> {generationResults.totalTermsGenerated}</Text>

                {generationResults.warnings?.length > 0 && (
                  <div>
                    <Text strong>Warnings:</Text>
                    <List
                      size="small"
                      dataSource={generationResults.warnings}
                      renderItem={(warning: string) => (
                        <List.Item>
                          <Text type="warning">⚠️ {warning}</Text>
                        </List.Item>
                      )}
                    />
                  </div>
                )}

                {generationResults.errors?.length > 0 && (
                  <div>
                    <Text strong>Errors:</Text>
                    <List
                      size="small"
                      dataSource={generationResults.errors}
                      renderItem={(error: string) => (
                        <List.Item>
                          <Text type="danger">❌ {error}</Text>
                        </List.Item>
                      )}
                    />
                  </div>
                )}
              </Space>
            </>
          ) : null}
        </Space>
      </Modal>

      {/* Content Management Modal */}
      <ContentManagementModal
        visible={contentModalVisible}
        onClose={() => setContentModalVisible(false)}
        generationResults={generationResults}
        onContentSaved={(savedContent) => {
          console.log('Content saved:', savedContent);
          message.success('Content saved to database successfully');
        }}
      />
    </div>
  );
};
