/**
 * Modern Database Explorer Page
 * 
 * Consolidated database exploration interface with schema browsing,
 * table preview, and data exploration capabilities.
 */

import React, { useState, useCallback } from 'react';
import { 
  PageLayout, 
  Card, 
  Button, 
  Tabs,
  Container,
  Stack,
  Flex,
  Grid,
  Badge,
  Input,
  Alert
} from '../components/core';
import { DBExplorer } from '../components/DBExplorer/DBExplorer';
import { SchemaTree } from '../components/DBExplorer/SchemaTree';
import { TableDataPreview } from '../components/DBExplorer/TableDataPreview';
import { TableExplorer } from '../components/DBExplorer/TableExplorer';

const DBExplorerPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('explorer');
  const [selectedTable, setSelectedTable] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const handleTableSelect = useCallback((tableName: string) => {
    setSelectedTable(tableName);
  }, []);

  const mockTables = [
    { name: 'tbl_Daily_actions', rows: 1250000, columns: 109, size: '2.3 GB', lastUpdated: '2 hours ago' },
    { name: 'tbl_Daily_actions_players', rows: 45000, columns: 15, size: '120 MB', lastUpdated: '1 hour ago' },
    { name: 'tbl_Countries', rows: 195, columns: 8, size: '2 MB', lastUpdated: '1 week ago' },
    { name: 'tbl_Currencies', rows: 168, columns: 6, size: '1 MB', lastUpdated: '1 week ago' },
    { name: 'tbl_White_labels', rows: 25, columns: 12, size: '500 KB', lastUpdated: '3 days ago' },
  ];

  const filteredTables = mockTables.filter(table => 
    table.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const tabItems = [
    {
      key: 'explorer',
      label: '🗂️ Schema Explorer',
      children: (
        <Container maxWidth="2xl" padding={false}>
          <Grid columns={3} gap="lg">
            {/* Schema Tree */}
            <Card variant="default" size="large">
              <Card.Header>
                <h3 style={{ margin: 0 }}>Database Schema</h3>
              </Card.Header>
              <Card.Content>
                <SchemaTree onTableSelect={handleTableSelect} />
              </Card.Content>
            </Card>

            {/* Table List */}
            <Card variant="default" size="large">
              <Card.Header>
                <Flex justify="between" align="center">
                  <h3 style={{ margin: 0 }}>Tables</h3>
                  <Badge variant="secondary">{filteredTables.length} tables</Badge>
                </Flex>
              </Card.Header>
              <Card.Content>
                <Stack spacing="md">
                  <Input
                    placeholder="Search tables..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
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
                        <Card.Content>
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
                        </Card.Content>
                      </Card>
                    ))}
                  </Stack>
                </Stack>
              </Card.Content>
            </Card>

            {/* Table Details */}
            <Card variant="default" size="large">
              <Card.Header>
                <h3 style={{ margin: 0 }}>
                  {selectedTable ? `Table: ${selectedTable}` : 'Select a Table'}
                </h3>
              </Card.Header>
              <Card.Content>
                {selectedTable ? (
                  <TableDataPreview tableName={selectedTable} />
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
              </Card.Content>
            </Card>
          </Grid>
        </Container>
      ),
    },
    {
      key: 'data-preview',
      label: '👁️ Data Preview',
      children: (
        <Container maxWidth="2xl" padding={false}>
          <Stack spacing="lg">
            {/* Connection Status */}
            <Alert
              variant="success"
              message="Database Connected"
              description="Successfully connected to DailyActionsDB at 185.64.56.157"
            />

            {/* Table Explorer */}
            <Card variant="default" size="large">
              <Card.Header>
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
              </Card.Header>
              <Card.Content>
                <TableExplorer />
              </Card.Content>
            </Card>
          </Stack>
        </Container>
      ),
    },
    {
      key: 'full-explorer',
      label: '🔍 Full Explorer',
      children: (
        <Container maxWidth="2xl" padding={false}>
          <Card variant="default" size="large">
            <Card.Header>
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
            </Card.Header>
            <Card.Content>
              <DBExplorer />
            </Card.Content>
          </Card>
        </Container>
      ),
    },
  ];

  return (
    <PageLayout
      title="Database Explorer"
      subtitle="Explore database schema, preview table data, and understand your data structure"
      tabs={
        <Tabs
          variant="line"
          size="large"
          activeKey={activeTab}
          onChange={handleTabChange}
          items={tabItems}
        />
      }
      actions={
        <Flex gap="md">
          <Button variant="outline">
            🔗 Connection Settings
          </Button>
          <Button variant="primary">
            📊 Generate Schema Report
          </Button>
        </Flex>
      }
    >
      {/* Tab content is handled by the Tabs component */}
    </PageLayout>
  );
};

export default DBExplorerPage;
