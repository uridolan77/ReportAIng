/**
 * Modern Dashboard Page
 * 
 * Consolidated dashboard interface that replaces scattered dashboard components.
 * Provides a unified dashboard building and viewing experience.
 */

import React, { useState, useCallback } from 'react';
import { 
  PageLayout, 
  Card, 
  Button, 
  Grid, 
  Container,
  Stack,
  Flex,
  Badge,
  Alert
} from '../components/core';
import { DashboardBuilder } from '../components/Dashboard/DashboardBuilder';
import { DashboardView } from '../components/Dashboard/DashboardView';
import { useCurrentResult } from '../hooks/useCurrentResult';
import { useAuthStore } from '../stores/authStore';

const DashboardPage: React.FC = () => {
  const [viewMode, setViewMode] = useState<'builder' | 'view'>('view');
  const [selectedDashboard, setSelectedDashboard] = useState<string | null>(null);
  const { hasResult } = useCurrentResult();
  const { isAdmin } = useAuthStore();

  const handleModeChange = useCallback((mode: 'builder' | 'view') => {
    setViewMode(mode);
  }, []);

  const mockDashboards = [
    { id: '1', name: 'Sales Overview', description: 'Key sales metrics and trends', widgets: 6, lastUpdated: '2 hours ago' },
    { id: '2', name: 'Player Analytics', description: 'Player behavior and engagement', widgets: 8, lastUpdated: '1 day ago' },
    { id: '3', name: 'Financial Summary', description: 'Revenue and financial KPIs', widgets: 4, lastUpdated: '3 hours ago' },
  ];

  return (
    <PageLayout
      title="Dashboard"
      subtitle="Create and manage interactive dashboards"
      actions={
        <Flex gap="md">
          <Button
            variant={viewMode === 'view' ? 'primary' : 'outline'}
            onClick={() => handleModeChange('view')}
          >
            📊 View Dashboards
          </Button>
          {isAdmin && (
            <Button
              variant={viewMode === 'builder' ? 'primary' : 'outline'}
              onClick={() => handleModeChange('builder')}
            >
              🔧 Dashboard Builder
            </Button>
          )}
          <Button variant="success">
            ➕ New Dashboard
          </Button>
        </Flex>
      }
    >
      <Container maxWidth="2xl" padding={false}>
        {viewMode === 'view' ? (
          <Stack spacing="lg">
            {/* Quick Stats */}
            <Grid columns={4} gap="md">
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#667eea' }}>
                      {mockDashboards.length}
                    </div>
                    <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>
                      Total Dashboards
                    </div>
                  </Flex>
                </Card.Content>
              </Card>
              
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#10b981' }}>
                      18
                    </div>
                    <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>
                      Total Widgets
                    </div>
                  </Flex>
                </Card.Content>
              </Card>
              
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#f59e0b' }}>
                      2.3k
                    </div>
                    <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>
                      Total Views
                    </div>
                  </Flex>
                </Card.Content>
              </Card>
              
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#ef4444' }}>
                      95%
                    </div>
                    <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>
                      Uptime
                    </div>
                  </Flex>
                </Card.Content>
              </Card>
            </Grid>

            {/* Current Query Result Alert */}
            {hasResult && (
              <Alert
                variant="info"
                message="Query Results Available"
                description="You can add your latest query results to any dashboard as a new widget."
                action={
                  <Button variant="primary" size="small">
                    Add to Dashboard
                  </Button>
                }
              />
            )}

            {/* Dashboard List */}
            <Card variant="default" size="large">
              <Card.Header>
                <Flex justify="between" align="center">
                  <h3 style={{ margin: 0 }}>Your Dashboards</h3>
                  <Badge variant="secondary">{mockDashboards.length} dashboards</Badge>
                </Flex>
              </Card.Header>
              <Card.Content>
                <Grid columns={1} gap="md">
                  {mockDashboards.map((dashboard) => (
                    <Card 
                      key={dashboard.id} 
                      variant="outlined" 
                      interactive
                      onClick={() => setSelectedDashboard(dashboard.id)}
                    >
                      <Card.Content>
                        <Flex justify="between" align="start">
                          <div>
                            <h4 style={{ margin: '0 0 8px 0', fontSize: '1.125rem' }}>
                              {dashboard.name}
                            </h4>
                            <p style={{ margin: '0 0 12px 0', color: '#6b7280' }}>
                              {dashboard.description}
                            </p>
                            <Flex gap="md" align="center">
                              <Badge variant="outline">
                                {dashboard.widgets} widgets
                              </Badge>
                              <span style={{ fontSize: '0.875rem', color: '#9ca3af' }}>
                                Updated {dashboard.lastUpdated}
                              </span>
                            </Flex>
                          </div>
                          <Flex gap="sm">
                            <Button variant="outline" size="small">
                              📊 View
                            </Button>
                            {isAdmin && (
                              <Button variant="ghost" size="small">
                                ✏️ Edit
                              </Button>
                            )}
                          </Flex>
                        </Flex>
                      </Card.Content>
                    </Card>
                  ))}
                </Grid>
              </Card.Content>
            </Card>

            {/* Selected Dashboard View */}
            {selectedDashboard && (
              <Card variant="elevated" size="large">
                <Card.Header>
                  <Flex justify="between" align="center">
                    <h3 style={{ margin: 0 }}>
                      {mockDashboards.find(d => d.id === selectedDashboard)?.name}
                    </h3>
                    <Button variant="ghost" onClick={() => setSelectedDashboard(null)}>
                      ✕ Close
                    </Button>
                  </Flex>
                </Card.Header>
                <Card.Content>
                  <DashboardView dashboardId={selectedDashboard} />
                </Card.Content>
              </Card>
            )}
          </Stack>
        ) : (
          <Card variant="default" size="large">
            <Card.Header>
              <h3 style={{ margin: 0 }}>Dashboard Builder</h3>
            </Card.Header>
            <Card.Content>
              <DashboardBuilder />
            </Card.Content>
          </Card>
        )}
      </Container>
    </PageLayout>
  );
};

export default DashboardPage;
