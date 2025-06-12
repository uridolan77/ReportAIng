/**
 * LLM Management Admin Page
 * 
 * Comprehensive LLM management interface for administrators to configure
 * AI providers, models, monitor usage, track costs, and analyze performance.
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
import { HomeOutlined, SettingOutlined, RobotOutlined } from '@ant-design/icons';
import { LLMDashboard } from '../../components/LLMManagement/LLMDashboard';
import { ProviderSettings } from '../../components/LLMManagement/ProviderSettings';
import { ModelConfiguration } from '../../components/LLMManagement/ModelConfiguration';
import { UsageAnalytics } from '../../components/LLMManagement/UsageAnalytics';
import { CostMonitoring } from '../../components/LLMManagement/CostMonitoring';
import { PerformanceMonitoring } from '../../components/LLMManagement/PerformanceMonitoring';
import { useAuthStore } from '../../stores/authStore';

const LLMManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');
  const { user, isAdmin } = useAuthStore();

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  // Check if user has required role
  const hasRequiredRole = user?.roles?.some(role =>
    ['Admin', 'Analyst'].includes(role)
  ) || isAdmin;

  // Show access denied message if user doesn't have required role
  if (!hasRequiredRole) {
    return (
      <PageLayout
        title="LLM Management"
        subtitle="Access Denied"
        breadcrumb={
          <Breadcrumb
            items={[
              { title: 'Home', path: '/', icon: <HomeOutlined /> },
              { title: 'Admin', path: '/admin', icon: <SettingOutlined /> },
              { title: 'LLM Management', icon: <RobotOutlined /> }
            ]}
          />
        }
      >
        <div style={{ padding: '24px' }}>
          <Alert
            message="Access Denied"
            description={
              <div>
                <p>LLM Management requires Admin or Analyst role access.</p>
                <p><strong>Current user:</strong> {user?.username || 'Unknown'}</p>
                <p><strong>Current roles:</strong> {user?.roles?.join(', ') || 'None'}</p>
                <p><strong>Required roles:</strong> Admin or Analyst</p>
                <br />
                <p>To access LLM Management:</p>
                <ul>
                  <li>Log in as an Admin user (username: <code>admin</code>, password: <code>Admin123!</code>)</li>
                  <li>Log in as an Analyst user (username: <code>analyst</code>, password: <code>Analyst123!</code>)</li>
                  <li>Or contact your system administrator to assign you the appropriate role</li>
                </ul>
              </div>
            }
            type="error"
            showIcon
            style={{ maxWidth: '800px' }}
          />
        </div>
      </PageLayout>
    );
  }

  const tabItems = [
    {
      key: 'dashboard',
      label: '📊 Dashboard',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <h3 className="text-xl font-semibold" style={{ margin: 0 }}>LLM Management Dashboard</h3>
            </Card.Header>
            <Card.Content>
              <LLMDashboard />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'providers',
      label: '🔌 Provider Settings',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 className="text-xl font-semibold" style={{ margin: 0 }}>LLM Provider Configuration</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    🔄 Test All Connections
                  </Button>
                  <Button variant="primary" size="small">
                    ➕ Add Provider
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <ProviderSettings />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'models',
      label: '🤖 Model Configuration',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 className="text-xl font-semibold" style={{ margin: 0 }}>Model Configuration & Selection</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    📋 Model Capabilities
                  </Button>
                  <Button variant="primary" size="small">
                    ⚙️ Configure Model
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <ModelConfiguration />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'usage',
      label: '📈 Usage Analytics',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 className="text-xl font-semibold" style={{ margin: 0 }}>Usage History & Analytics</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    📤 Export Data
                  </Button>
                  <Button variant="outline" size="small">
                    🔍 Advanced Filters
                  </Button>
                  <Button variant="outline" size="small">
                    🔄 Refresh
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <UsageAnalytics />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'costs',
      label: '💰 Cost Management',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 className="text-xl font-semibold" style={{ margin: 0 }}>Cost Monitoring & Management</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    🚨 Set Alerts
                  </Button>
                  <Button variant="outline" size="small">
                    📊 Cost Reports
                  </Button>
                  <Button variant="primary" size="small">
                    💳 Billing Settings
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <CostMonitoring />
            </Card.Content>
          </Card>
        </div>
      ),
    },
    {
      key: 'performance',
      label: '⚡ Performance',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <Flex justify="between" align="center">
                <h3 className="text-xl font-semibold" style={{ margin: 0 }}>Performance Monitoring & Optimization</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small">
                    🔧 Optimize Settings
                  </Button>
                  <Button variant="outline" size="small">
                    📊 Performance Report
                  </Button>
                  <Button variant="outline" size="small">
                    🔄 Refresh Metrics
                  </Button>
                </Flex>
              </Flex>
            </Card.Header>
            <Card.Content>
              <PerformanceMonitoring />
            </Card.Content>
          </Card>
        </div>
      ),
    },
  ];

  return (
    <PageLayout
      title="LLM Management"
      subtitle="Configure AI providers, monitor usage, track costs, and optimize performance"
      breadcrumb={
        <Breadcrumb
          items={[
            { title: 'Home', path: '/', icon: <HomeOutlined /> },
            { title: 'Admin', path: '/admin', icon: <SettingOutlined /> },
            { title: 'LLM Management', icon: <RobotOutlined /> }
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
            📊 System Health
          </Button>
          <Button variant="outline">
            📋 Usage Report
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

export default LLMManagementPage;
