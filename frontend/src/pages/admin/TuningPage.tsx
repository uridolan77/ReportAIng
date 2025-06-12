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
import { Breadcrumb } from '../../components/core/Navigation';
import { HomeOutlined, SettingOutlined } from '@ant-design/icons';
import { TuningOverview } from '../../components/Tuning/TuningOverview';
import { AIConfigurationManager } from '../../components/Tuning/AIConfigurationManager';
import { PromptManagementHub } from '../../components/Tuning/PromptManagementHub';
import { KnowledgeBaseManager } from '../../components/Tuning/KnowledgeBaseManager';
import { AutoGenerationManager } from '../../components/Tuning/AutoGenerationManager';
import { MonitoringDashboard } from '../../components/Tuning/MonitoringDashboard';

const TuningPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview');

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const tabItems = [
    {
      key: 'overview',
      label: '📊 Overview',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <h3 style={{ margin: 0 }}>AI Tuning Overview</h3>
            </Card.Header>
            <Card.Content>
              <TuningOverview onNavigateToTab={setActiveTab} />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'ai-configuration',
      label: '🤖 AI Configuration',
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
                <h3 style={{ margin: 0 }}>AI Model & Behavior Configuration</h3>
              </Card.Header>
              <Card.Content>
                <AIConfigurationManager />
              </Card.Content>
            </Card>
          </Stack>
        </div>
      ),
    },
    {
      key: 'prompt-management',
      label: '📝 Prompt Management',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Prompt Templates & Testing</h3>
                <Button variant="primary">
                  ➕ New Template
                </Button>
              </Flex>
            </Card.Header>
            <Card.Content>
              <PromptManagementHub />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'knowledge-base',
      label: '📚 Knowledge Base',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Business Knowledge & Data Management</h3>
                <Button variant="primary">
                  ➕ Add Knowledge
                </Button>
              </Flex>
            </Card.Header>
            <Card.Content>
              <KnowledgeBaseManager />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'auto-generate',
      label: '🔧 Auto-Generate',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>AI Content Auto-Generation</h3>
                <Button variant="primary">
                  ▶️ Start Generation
                </Button>
              </Flex>
            </Card.Header>
            <Card.Content>
              <AutoGenerationManager />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'monitoring',
      label: '📋 Monitoring',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>AI Performance & Usage Analytics</h3>
                <Flex gap="sm">
                  <Badge variant="info">Real-time</Badge>
                  <Button variant="outline" size="small">
                    📤 Export Report
                  </Button>
                  <Button variant="outline" size="small">
                    🔄 Refresh
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <MonitoringDashboard />
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
      breadcrumb={
        <Breadcrumb
          items={[
            { title: 'Home', path: '/', icon: <HomeOutlined /> },
            { title: 'Admin', path: '/admin', icon: <SettingOutlined /> },
            { title: 'AI Tuning', icon: <SettingOutlined /> }
          ]}
        />
      }
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
