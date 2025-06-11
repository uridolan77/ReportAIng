/**
 * Modern Query Page
 * 
 * Consolidated query interface that replaces the old QueryInterface component.
 * Uses modern core components and provides a clean, focused query experience.
 */

import React, { useState, useCallback } from 'react';
import { 
  PageLayout, 
  Card, 
  Button, 
  Input, 
  Alert, 
  LoadingFallback,
  Tabs,
  Container,
  Stack,
  Flex
} from '../components/core';
import { QueryInterface } from '../components/QueryInterface/QueryInterface';
import { QueryHistory } from '../components/QueryInterface/QueryHistory';
import { QuerySuggestions } from '../components/QueryInterface/QuerySuggestions';
// MockDataToggle removed - database connection always required
import { useCurrentResult } from '../hooks/useCurrentResult';
import { useAuthStore } from '../stores/authStore';

const QueryPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('query');
  const { hasResult } = useCurrentResult();
  const { user } = useAuthStore();

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const tabItems = [
    {
      key: 'query',
      label: 'Query Interface',
      children: (
        <div style={{ width: '80%', margin: '0 auto' }}>
          <Stack spacing="lg">
            {/* Welcome Section */}
            <Card variant="filled" size="medium">
              <Card.Content>
                <Flex justify="between" align="center">
                  <div>
                    <h2 style={{ margin: 0, marginBottom: '8px' }}>
                      Welcome back, {user?.name || 'User'}!
                    </h2>
                    <p style={{ margin: 0, color: '#6b7280' }}>
                      Ask questions about your data using natural language
                    </p>
                  </div>
                  {/* Database connection always required - no toggle needed */}
                </Flex>
              </Card.Content>
            </Card>

            {/* Main Query Interface */}
            <Card variant="default" size="large">
              <QueryInterface />
            </Card>

            {/* Quick Actions */}
            <Card variant="outlined" size="medium">
              <Card.Header>
                <h3 style={{ margin: 0 }}>Quick Actions</h3>
              </Card.Header>
              <Card.Content>
                <Flex gap="md" wrap="wrap">
                  <Button variant="outline" size="medium">
                    📊 Sample Reports
                  </Button>
                  <Button variant="outline" size="medium">
                    📈 Trending Queries
                  </Button>
                  <Button variant="outline" size="medium">
                    🎯 Query Templates
                  </Button>
                  <Button variant="outline" size="medium">
                    💡 AI Suggestions
                  </Button>
                </Flex>
              </Card.Content>
            </Card>

            {/* Results Preview */}
            {hasResult && (
              <Alert
                variant="success"
                message="Query Results Available"
                description="Your query has been executed successfully. View results in the Results page."
                action={
                  <Button variant="primary" size="small" href="/results">
                    View Results
                  </Button>
                }
              />
            )}
          </Stack>
        </div>
      ),
    },
    {
      key: 'history',
      label: 'Query History',
      children: (
        <div style={{ width: '80%', margin: '0 auto' }}>
          <Card variant="default" size="large">
            <Card.Header>
              <h3 style={{ margin: 0 }}>Recent Queries</h3>
            </Card.Header>
            <Card.Content>
              <QueryHistory />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'suggestions',
      label: 'AI Suggestions',
      children: (
        <div style={{ width: '80%', margin: '0 auto' }}>
          <Card variant="default" size="large">
            <Card.Header>
              <h3 style={{ margin: 0 }}>Smart Query Suggestions</h3>
            </Card.Header>
            <Card.Content>
              <QuerySuggestions />
            </Card.Content>
          </Card>
        </div>
      ),
    },
  ];

  return (
    <PageLayout
      title="Query Interface"
      subtitle="Ask questions about your data using natural language"
      tabs={
        <Tabs
          variant="line"
          size="large"
          activeKey={activeTab}
          onChange={handleTabChange}
          items={tabItems}
        />
      }
    >
      {/* Tab content is handled by the Tabs component */}
    </PageLayout>
  );
};

export default QueryPage;
