/**
 * Modern Dashboard Page
 * 
 * Consolidated dashboard interface that replaces scattered dashboard components.
 * Provides a unified dashboard building and viewing experience.
 */

import React, { useState, useCallback } from 'react';
import {
  Card,
  Button,
  Grid,
  Container,
  Stack,
  Flex,
  Badge,
  Alert
} from '../components/core';
import { DashboardOutlined } from '@ant-design/icons';
import DashboardBuilder from '../components/Dashboard/DashboardBuilder';
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

  // Real dashboards - loaded from API
  const [dashboards, setDashboards] = useState<any[]>([]);
  const [loadingDashboards, setLoadingDashboards] = useState(true);
  const [dashboardError, setDashboardError] = useState<string | null>(null);

  // Load real dashboards on component mount
  React.useEffect(() => {
    const loadDashboards = async () => {
      try {
        setLoadingDashboards(true);
        setDashboardError(null);

        // TODO: Replace with actual dashboard API calls
        // const response = await dashboardApi.getUserDashboards();
        // setDashboards(response.dashboards);

        console.log('Loading real dashboards...');

        // For now, show empty state until real API is connected
        setDashboards([]);

      } catch (err) {
        console.error('Failed to load dashboards:', err);
        setDashboardError('Failed to load dashboards. Please check your connection.');
      } finally {
        setLoadingDashboards(false);
      }
    };

    loadDashboards();
  }, []);

  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <div>
            <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
              <DashboardOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
              Dashboard
            </h1>
            <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
              Create and manage interactive dashboards
            </p>
          </div>
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
        </div>
      </div>
      <div className="full-width-content">
        {viewMode === 'view' ? (
          <Stack spacing="lg">
            {/* Quick Stats */}
            <Grid columns={4} gap="md">
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div className="text-4xl font-bold" style={{ color: '#667eea' }}>
                      {loadingDashboards ? '...' : dashboards.length}
                    </div>
                    <div className="text-sm" style={{ color: '#6b7280' }}>
                      Total Dashboards
                    </div>
                  </Flex>
                </Card.Content>
              </Card>

              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div className="text-4xl font-bold" style={{ color: '#10b981' }}>
                      {loadingDashboards ? '...' : dashboards.reduce((sum, d) => sum + (d.widgets || 0), 0)}
                    </div>
                    <div className="text-sm" style={{ color: '#6b7280' }}>
                      Total Widgets
                    </div>
                  </Flex>
                </Card.Content>
              </Card>
              
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div className="text-4xl font-bold" style={{ color: '#f59e0b' }}>
                      2.3k
                    </div>
                    <div className="text-sm" style={{ color: '#6b7280' }}>
                      Total Views
                    </div>
                  </Flex>
                </Card.Content>
              </Card>
              
              <Card variant="filled" size="medium">
                <Card.Content>
                  <Flex direction="column" align="center">
                    <div className="text-4xl font-bold" style={{ color: '#ef4444' }}>
                      95%
                    </div>
                    <div className="text-sm" style={{ color: '#6b7280' }}>
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
                  <Badge variant="secondary">{dashboards.length} dashboards</Badge>
                </Flex>
              </Card.Header>
              <Card.Content>
                <Grid columns={1} gap="md">
                  {dashboards.map((dashboard) => (
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
                      {dashboards.find(d => d.id === selectedDashboard)?.name}
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
      </div>
    </div>
  );
};

export default DashboardPage;
