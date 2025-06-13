/**
 * Modern AI Tuning Admin Page
 * 
 * Consolidated AI tuning interface for administrators to configure
 * AI models, prompts, and system behavior.
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
  Alert,
  Badge
} from '../../components/core';
import { SettingOutlined } from '@ant-design/icons';
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
            <CardHeader>
              <h3 style={{ margin: 0 }}>AI Tuning Overview</h3>
            </CardHeader>
            <CardContent>
              <TuningOverview onNavigateToTab={setActiveTab} />
            </CardContent>
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
              <CardHeader>
                <h3 style={{ margin: 0 }}>AI Model & Behavior Configuration</h3>
              </CardHeader>
              <CardContent>
                <AIConfigurationManager />
              </CardContent>
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
            <CardHeader>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Prompt Templates & Testing</h3>
                <Button variant="primary">
                  ➕ New Template
                </Button>
              </Flex>
            </CardHeader>
            <CardContent>
              <PromptManagementHub />
            </CardContent>
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
            <CardHeader>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>Business Knowledge & Data Management</h3>
                <Button variant="primary">
                  ➕ Add Knowledge
                </Button>
              </Flex>
            </CardHeader>
            <CardContent>
              <KnowledgeBaseManager />
            </CardContent>
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
            <CardHeader>
              <Flex justify="between" align="center">
                <h3 style={{ margin: 0 }}>AI Content Auto-Generation</h3>
                <Button variant="primary">
                  ▶️ Start Generation
                </Button>
              </Flex>
            </CardHeader>
            <CardContent>
              <AutoGenerationManager />
            </CardContent>
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
            <CardHeader>
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
            </CardHeader>
            <CardContent>
              <MonitoringDashboard />
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
          <SettingOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          AI Tuning
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Configure AI models, prompts, and system behavior
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

export default TuningPage;
