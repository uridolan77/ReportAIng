import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Typography,
  Space,
  Button,
  Alert,
  Spin,
  message,
  Drawer
} from 'antd';
import { Resizable } from 'react-resizable';
import './DBExplorer.css';
import {
  DatabaseOutlined,
  ReloadOutlined,
  QuestionCircleOutlined,
  FullscreenOutlined
} from '@ant-design/icons';
import { SchemaTree } from './SchemaTree';
import { TableExplorer } from './TableExplorer';
import { TableDataPreview } from './TableDataPreview';
import { DatabaseTable, DatabaseSchema, DBExplorerState } from '../../types/dbExplorer';
import DBExplorerAPI from '../../services/dbExplorerApi';
import { ApiService } from '../../services/api';
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
    loading: false
  });

  const [schema, setSchema] = useState<DatabaseSchema | null>(null);
  const [siderCollapsed, setSiderCollapsed] = useState(false);
  const [previewDrawerVisible, setPreviewDrawerVisible] = useState(false);
  const [siderWidth, setSiderWidth] = useState(350);



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
        padding: '12px 16px', 
        borderBottom: '1px solid #f0f0f0',
        background: '#fafafa',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <Text type="secondary">
          Comprehensive database interface with schema browsing, table exploration, and data preview
        </Text>
        <Space>
          <Button
            icon={<QuestionCircleOutlined />}
            size="small"
            onClick={() => {
              message.info('Use the tree view to explore tables, click on tables to see details, and use the preview button to see sample data.');
            }}
          >
            Help
          </Button>
          <Button
            icon={<ReloadOutlined />}
            size="small"
            onClick={handleRefresh}
            loading={state.loading}
          >
            Refresh Schema
          </Button>
          <Button
            icon={<FullscreenOutlined />}
            size="small"
            onClick={() => setSiderCollapsed(!siderCollapsed)}
          >
            {siderCollapsed ? 'Expand' : 'Collapse'} Tree
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
      <div style={{ flex: 1, margin: '0 16px 16px 16px', display: 'flex', minHeight: 0 }}>
        <div style={{ display: 'flex', height: '100%', width: '100%', gap: '16px' }}>
          {/* Resizable Schema Tree Sidebar */}
          {!siderCollapsed && (
            <Resizable
              width={siderWidth}
              height={0}
              onResize={(_, { size }) => {
                setSiderWidth(size.width);
              }}
              minConstraints={[250, 0]}
              maxConstraints={[600, 0]}
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
              />
            ) : (
              <Card style={{ height: '100%', textAlign: 'center' }}>
                <div style={{ padding: '100px 20px' }}>
                  <DatabaseOutlined style={{ fontSize: '64px', color: '#d9d9d9' }} />
                  <Title level={3} type="secondary" style={{ marginTop: '24px' }}>
                    Select a Table to Explore
                  </Title>
                  <Text type="secondary">
                    Choose a table from the schema tree on the left to view its structure, columns, and sample data.
                  </Text>
                  {tables.length > 0 && (
                    <div style={{ marginTop: '24px' }}>
                      <Text type="secondary">
                        Found {tables.length} tables in the database
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
    </div>
  );
};
