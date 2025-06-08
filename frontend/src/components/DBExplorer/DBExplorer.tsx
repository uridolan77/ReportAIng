import React, { useState, useEffect, useCallback } from 'react';
import {
  Layout,
  Card,
  Typography,
  Space,
  Button,
  Alert,
  Spin,
  message,
  Row,
  Col,
  Drawer
} from 'antd';
import { Resizable } from 'react-resizable';
import 'react-resizable/css/styles.css';
import './DBExplorer.css';
import {
  DatabaseOutlined,
  ReloadOutlined,
  SettingOutlined,
  QuestionCircleOutlined,
  FullscreenOutlined
} from '@ant-design/icons';
import { SchemaTree } from './SchemaTree';
import { TableExplorer } from './TableExplorer';
import { TableDataPreview } from './TableDataPreview';
import { DatabaseTable, DatabaseSchema, DBExplorerState } from '../../types/dbExplorer';
import DBExplorerAPI from '../../services/dbExplorerApi';
import { ApiService } from '../../services/api';
import api from '../../services/api';
import { useNavigate } from 'react-router-dom';

const { Title, Text } = Typography;
const { Sider, Content } = Layout;

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
  const [debugData, setDebugData] = useState<any>(null);

  // Transform SQL query result to schema structure
  const transformQueryResultToSchema = (data: any[]): DatabaseSchema => {
    console.log('🔄 Starting transformation with data:', {
      totalRows: data.length,
      sampleRows: data.slice(0, 3),
      allKeys: data.length > 0 ? Object.keys(data[0]) : []
    });

    const tablesMap = new Map<string, any>();

    // Group data by table
    data.forEach((row, index) => {
      if (index < 5) {
        console.log(`🔍 Processing row ${index}:`, row);
      }

      const tableKey = `${row.schema_name}.${row.table_name}`;

      if (!tablesMap.has(tableKey)) {
        const newTable = {
          name: row.table_name,
          schema: row.schema_name,
          type: row.table_type?.toLowerCase() === 'view' ? 'view' : 'table',
          columns: [],
          primaryKeys: [],
          foreignKeys: []
        };
        tablesMap.set(tableKey, newTable);
        console.log(`📋 Created new table: ${tableKey}`, newTable);
      }

      const table = tablesMap.get(tableKey);
      if (row.column_name) {
        const column = {
          name: row.column_name,
          dataType: row.data_type,
          isNullable: row.is_nullable === 'YES',
          isPrimaryKey: row.is_primary_key === 1,
          isForeignKey: false,
          defaultValue: row.default_value,
          maxLength: row.max_length
        };

        table.columns.push(column);

        if (column.isPrimaryKey) {
          table.primaryKeys.push(column.name);
        }

        if (index < 10) {
          console.log(`📝 Added column to ${tableKey}:`, column);
        }
      }
    });

    const result = {
      name: 'Database',
      lastUpdated: new Date().toISOString(),
      version: '1.0.0',
      views: Array.from(tablesMap.values()).filter(t => t.type === 'view'),
      tables: Array.from(tablesMap.values()).filter(t => t.type === 'table')
    };

    console.log('✅ Transformation complete:', {
      totalTables: result.tables.length,
      totalViews: result.views.length,
      sampleTable: result.tables[0],
      sampleTableColumnCount: result.tables[0]?.columns?.length
    });

    return result;
  };

  // Load database schema
  const loadSchema = useCallback(async (bustCache: boolean = false) => {
    setState(prev => ({ ...prev, loading: true, error: undefined }));

    try {
      // Use the same API call as SchemaManagementDashboard which works
      console.log('🔍 Attempting to load schema using ApiService.getSchema()...');
      const rawSchemaData = await ApiService.getSchema();
      console.log('🔍 Raw schema data from ApiService:', rawSchemaData);
      console.log('🔍 Schema data structure:', {
        keys: Object.keys(rawSchemaData || {}),
        databaseName: rawSchemaData?.databaseName,
        tablesCount: rawSchemaData?.tables?.length || 0,
        viewsCount: rawSchemaData?.views?.length || 0,
        sampleTable: rawSchemaData?.tables?.[0]
      });

      if (rawSchemaData && rawSchemaData.tables) {
        // Transform it to match our interface - use the data directly since it's already in the right format
        const transformedSchema: DatabaseSchema = {
          name: rawSchemaData.databaseName || 'Database',
          lastUpdated: rawSchemaData.lastUpdated || new Date().toISOString(),
          version: rawSchemaData.version || '1.0.0',
          views: rawSchemaData.views || [],
          tables: rawSchemaData.tables || []
        };

        console.log('🔍 Transformed schema for UI:', {
          name: transformedSchema.name,
          tablesCount: transformedSchema.tables.length,
          viewsCount: transformedSchema.views.length,
          sampleTable: transformedSchema.tables[0],
          sampleTableColumns: transformedSchema.tables[0]?.columns?.slice(0, 3)
        });

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
      console.log('🔄 Schema refresh completed, reloading with cache busting...');
      await loadSchema(true); // Pass true to bust cache
      console.log('🔄 Schema reload completed');
    } catch (error) {
      console.error('🔄 Schema refresh failed:', error);
      message.error('Failed to refresh schema');
    }
  }, [loadSchema]);

  const tables = schema?.tables || [];

  return (
    <div style={{ height: '100vh', background: '#f0f2f5' }}>
      {/* Header */}
      <Card 
        style={{ 
          margin: '16px 16px 0 16px', 
          borderRadius: '8px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
        }}
        bodyStyle={{ padding: '16px 24px' }}
      >
        <Row justify="space-between" align="middle">
          <Col>
            <Space size="middle">
              <DatabaseOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>
                  Database Explorer
                </Title>
                <Text type="secondary">
                  Explore your database schema, view table structures, and preview data
                </Text>
              </div>
            </Space>
          </Col>
          <Col>
            <Space>
              <Button
                icon={<QuestionCircleOutlined />}
                onClick={() => {
                  message.info('Use the tree view to explore tables, click on tables to see details, and use the preview button to see sample data.');
                }}
              >
                Help
              </Button>
              <Button
                icon={<ReloadOutlined />}
                onClick={handleRefresh}
                loading={state.loading}
              >
                Refresh Schema
              </Button>
              <Button
                icon={<FullscreenOutlined />}
                onClick={() => setSiderCollapsed(!siderCollapsed)}
              >
                {siderCollapsed ? 'Expand' : 'Collapse'} Tree
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Error Alert */}
      {state.error && (
        <Alert
          message="Error Loading Database Schema"
          description={state.error}
          type="error"
          showIcon
          closable
          action={
            <Button size="small" onClick={loadSchema}>
              Retry
            </Button>
          }
          style={{ margin: '16px' }}
        />
      )}

      {/* Main Content */}
      <div style={{ margin: '16px', height: 'calc(100vh - 140px)' }}>
        <div style={{ display: 'flex', height: '100%', gap: '16px' }}>
          {/* Resizable Schema Tree Sidebar */}
          {!siderCollapsed && (
            <Resizable
              width={siderWidth}
              height={0}
              onResize={(e, { size }) => {
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
                  selectedTableName={state.selectedTable?.name}
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
        placement="bottom"
        height="70%"
        open={previewDrawerVisible}
        onClose={() => setPreviewDrawerVisible(false)}
        destroyOnClose
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
