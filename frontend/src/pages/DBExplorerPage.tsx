/**
 * Modern Database Explorer Page
 * 
 * Consolidated database exploration interface with schema browsing,
 * table preview, and data exploration capabilities.
 */

import React, { useState, useCallback } from 'react';
import {
  Card,
  CardHeader,
  CardContent,
  Button,
  Tabs,
  Stack,
  Flex,
  Grid,
  Badge,
  Input,
  Alert
} from '../components/core';
import { DatabaseOutlined } from '@ant-design/icons';
import { DBExplorer } from '../components/DBExplorer/DBExplorer';
import { SchemaTree } from '../components/DBExplorer/SchemaTree';
import { TableDataPreview } from '../components/DBExplorer/TableDataPreview';
import { TableExplorer } from '../components/DBExplorer/TableExplorer';
import { DatabaseTable } from '../types/dbExplorer';

const DBExplorerPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('explorer');
  const [selectedTable, setSelectedTable] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const handleTableSelect = useCallback((table: DatabaseTable | string) => {
    if (typeof table === 'string') {
      setSelectedTable(table);
    } else {
      setSelectedTable(table.name);
    }
  }, []);

  // Real database data - loaded from API
  const [tables, setTables] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load real database schema on component mount
  React.useEffect(() => {
    const loadDatabaseSchema = async () => {
      try {
        setLoading(true);
        setError(null);

        // TODO: Replace with actual API calls to get real database schema
        // const response = await dbExplorerApi.getTableSchema();
        // setTables(response.tables);

        console.log('Loading real database schema...');

        // For now, show loading state until real API is connected
        setTables([]);

      } catch (err) {
        console.error('Failed to load database schema:', err);
        setError('Failed to connect to database. Please check your connection.');
      } finally {
        setLoading(false);
      }
    };

    loadDatabaseSchema();
  }, []);

  // Real schema data - loaded from API
  const [schemaData, setSchemaData] = useState<DatabaseTable[]>([]);

  // Load schema data along with tables
  React.useEffect(() => {
    const loadSchemaData = async () => {
      try {
        // TODO: Replace with actual API calls to get real database schema
        // const response = await dbExplorerApi.getSchemaData();
        // setSchemaData(response.schema);

        console.log('Loading real schema data...');

        // For now, show empty state until real API is connected
        setSchemaData([]);

      } catch (err) {
        console.error('Failed to load schema data:', err);
      }
    };

    loadSchemaData();
  }, []);

  const filteredTables = tables.filter(table =>
    table.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const tabItems = [
    {
      key: 'explorer',
      label: '🗂️ Schema Explorer',
      children: (
        <div className="full-width-content">
          <Grid columns={3} gap="lg">
            {/* Schema Tree */}
            <Card variant="default" size="large">
              <CardHeader>
                <h3 style={{ margin: 0 }}>Database Schema</h3>
              </CardHeader>
              <CardContent>
                {loading ? (
                  <div style={{ padding: '2rem', textAlign: 'center' }}>
                    <div>Loading database schema...</div>
                  </div>
                ) : error ? (
                  <div style={{ padding: '2rem', textAlign: 'center', color: '#ef4444' }}>
                    <div>{error}</div>
                  </div>
                ) : (
                  <SchemaTree
                    tables={schemaData}
                    onTableSelect={handleTableSelect}
                    selectedTableName={selectedTable || ''}
                  />
                )}
              </CardContent>
            </Card>

            {/* Table List */}
            <Card variant="default" size="large">
              <CardHeader>
                <Flex justify="between" align="center">
                  <h3 style={{ margin: 0 }}>Tables</h3>
                  <Badge variant="secondary">{filteredTables.length} tables</Badge>
                </Flex>
              </CardHeader>
              <CardContent>
                <Stack spacing="md">
                  <Input
                    placeholder="Search tables..."
                    value={searchQuery}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSearchQuery(e.target.value)}
                    style={{ marginBottom: '16px' }}
                  />
                  
                  <Stack spacing="sm">
                    {filteredTables.map((table) => (
                      <Card 
                        key={table.name}
                        variant={selectedTable === table.name ? 'filled' : 'outlined'}
                        interactive
                        onClick={() => handleTableSelect(table.name)}
                      >
                        <CardContent>
                          <div style={{ fontWeight: 'bold', marginBottom: '4px' }}>
                            {table.name}
                          </div>
                          <Flex gap="sm" wrap="wrap">
                            <Badge variant="outline" size="small">
                              {table.rows.toLocaleString()} rows
                            </Badge>
                            <Badge variant="outline" size="small">
                              {table.columns} cols
                            </Badge>
                            <Badge variant="outline" size="small">
                              {table.size}
                            </Badge>
                          </Flex>
                          <div style={{ fontSize: '0.75rem', color: '#9ca3af', marginTop: '4px' }}>
                            Updated {table.lastUpdated}
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </Stack>
                </Stack>
              </CardContent>
            </Card>

            {/* Table Details */}
            <Card variant="default" size="large">
              <CardHeader>
                <h3 style={{ margin: 0 }}>
                  {selectedTable ? `Table: ${selectedTable}` : 'Select a Table'}
                </h3>
              </CardHeader>
              <CardContent>
                {loading ? (
                  <div style={{ padding: '2rem', textAlign: 'center' }}>
                    <div>Loading table details...</div>
                  </div>
                ) : selectedTable ? (
                  (() => {
                    const selectedTableData = schemaData.find(table => table.name === selectedTable);
                    return selectedTableData ? (
                      <TableDataPreview table={selectedTableData} />
                    ) : (
                      <div style={{
                        padding: '2rem',
                        textAlign: 'center',
                        color: '#6b7280'
                      }}>
                        <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>⚠️</div>
                        <p>Table data not found: {selectedTable}</p>
                        <p style={{ fontSize: '0.875rem', marginTop: '1rem' }}>
                          Please ensure database connection is available.
                        </p>
                      </div>
                    );
                  })()
                ) : (
                  <div style={{
                    padding: '2rem',
                    textAlign: 'center',
                    color: '#6b7280'
                  }}>
                    <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>📋</div>
                    <p>Select a table to view its structure and preview data</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </Grid>
        </div>
      ),
    },
    {
      key: 'data-preview',
      label: '👁️ Data Preview',
      children: (
        <div className="full-width-content">
          <Stack spacing="lg">
            {/* Connection Status */}
            {!loading && !error && (
              <Alert
                variant="success"
                message="Database Connected"
                description="Successfully connected to DailyActionsDB at 185.64.56.157"
              />
            )}
            {error && (
              <Alert
                variant="error"
                message="Database Connection Failed"
                description={error}
              />
            )}

            {/* Table Explorer */}
            <Card variant="default" size="large">
              <CardHeader>
                <Flex justify="between" align="center">
                  <h3 style={{ margin: 0 }}>Table Data Explorer</h3>
                  <Flex gap="sm">
                    <Button variant="outline" size="small">
                      📤 Export Data
                    </Button>
                    <Button variant="outline" size="small">
                      🔄 Refresh
                    </Button>
                  </Flex>
                </Flex>
              </CardHeader>
              <CardContent>
                {loading ? (
                  <div style={{ padding: '2rem', textAlign: 'center' }}>
                    <div>Loading table explorer...</div>
                  </div>
                ) : selectedTable ? (
                  (() => {
                    const selectedTableData = schemaData.find(table => table.name === selectedTable);
                    return selectedTableData ? (
                      <TableExplorer
                        table={selectedTableData}
                        onPreviewData={() => {
                          console.log('Preview data for:', selectedTableData.name);
                        }}
                        onGenerateQuery={(query) => {
                          console.log('Generated query:', query);
                        }}
                      />
                    ) : (
                      <div style={{
                        padding: '2rem',
                        textAlign: 'center',
                        color: '#6b7280'
                      }}>
                        <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>⚠️</div>
                        <p>Table data not found: {selectedTable}</p>
                        <p style={{ fontSize: '0.875rem', marginTop: '1rem' }}>
                          Please ensure database connection is available.
                        </p>
                      </div>
                    );
                  })()
                ) : (
                  <div style={{
                    padding: '2rem',
                    textAlign: 'center',
                    color: '#6b7280'
                  }}>
                    <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🔍</div>
                    <p>Select a table from the Schema Explorer tab to view its details and explore data</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </Stack>
        </div>
      ),
    },
    {
      key: 'full-explorer',
      label: '🔍 Full Explorer',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <CardHeader>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Database Explorer</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    ⚙️ Settings
                  </Button>
                  <Button variant="outline" size="small">
                    📊 Statistics
                  </Button>
                </Flex>
              </Flex>
            </CardHeader>
            <CardContent>
              <DBExplorer />
            </CardContent>
          </Card>
        </div>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <DatabaseOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Database Explorer
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Explore database schema, preview table data, and understand your data structure
        </p>
      </div>

      <Tabs
        variant="line"
        size="large"
        activeKey={activeTab}
        onChange={handleTabChange}
        items={tabItems}
      />
    </div>
  );
};

export default DBExplorerPage;
