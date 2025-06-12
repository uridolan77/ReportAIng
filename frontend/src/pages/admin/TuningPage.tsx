/**
 * Modern AI Tuning Admin Page
 * 
 * Consolidated AI tuning interface for administrators to configure
 * AI models, prompts, and system behavior.
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
  Alert,
  Badge
} from '../../components/core';
import { TuningDashboard } from '../../components/Tuning/TuningDashboard';
import { AISettingsManager } from '../../components/Tuning/AISettingsManager';
import { PromptTemplateManager } from '../../components/Tuning/PromptTemplateManager';
import { BusinessGlossaryManager } from '../../components/Tuning/BusinessGlossaryManager';
import { QueryPatternManager } from '../../components/Tuning/QueryPatternManager';
import { PromptLogsViewer } from '../../components/Tuning/PromptLogsViewer';

const TuningPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const tabItems = [
    {
      key: 'dashboard',
      label: '📊 Dashboard',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <h3 style={{ margin: 0 }}>AI Tuning Dashboard</h3>
            </Card.Header>
            <Card.Content>
              <TuningDashboard />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'ai-settings',
      label: '🤖 AI Settings',
      children: (
        <div className="full-width-content">
          <Stack spacing="lg">
            <Alert
              variant="warning"
              message="Admin Access Required"
              description="Changes to AI settings will affect all users. Please review carefully before applying."
            />
            <Card variant="default" size="large">
              <Card.Header>
                <h3 style={{ margin: 0 }}>AI Model Configuration</h3>
              </Card.Header>
              <Card.Content>
                <AISettingsManager />
              </Card.Content>
            </Card>
          </Stack>
        </div>
      ),
    },
    {
      key: 'prompts',
      label: '📝 Prompt Templates',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Prompt Template Management</h3>
                <Button variant="primary">
                  ➕ New Template
                </Button>
              </Flex>
            </Card.Header>
            <Card.Content>
              <PromptTemplateManager />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'glossary',
      label: '📚 Business Glossary',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Business Terms & Definitions</h3>
                <Button variant="primary">
                  ➕ Add Term
                </Button>
              </Flex>
            </Card.Header>
            <Card.Content>
              <BusinessGlossaryManager />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'patterns',
      label: '🔍 Query Patterns',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Query Pattern Analysis</h3>
                <Badge variant="secondary">Auto-learning enabled</Badge>
              </Flex>
            </Card.Header>
            <Card.Content>
              <QueryPatternManager />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'logs',
      label: '📋 Prompt Logs',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>AI Prompt Logs & Analytics</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    📤 Export Logs
                  </Button>
                  <Button variant="outline" size="small">
                    🔄 Refresh
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <PromptLogsViewer />
            </Card.Content>
          </Card>
        </div>
      ),
    },
  ];

  return (
    <PageLayout
      title="AI Tuning"
      subtitle="Configure AI models, prompts, and system behavior"
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
            📊 Performance Report
          </Button>
          <Button variant="primary">
            💾 Save All Changes
          </Button>
        </Flex>
      }
    >
      {/* Tab content is handled by the Tabs component */}
    </PageLayout>
  );
};

export default TuningPage;
