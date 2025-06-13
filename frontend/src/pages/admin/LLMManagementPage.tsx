/**
 * LLM Management Admin Page
 * 
 * Comprehensive LLM management interface for administrators to configure
 * AI providers, models, monitor usage, track costs, and analyze performance.
 */

import React, { useState, useCallback } from 'react';
import {
  Card,
  CardHeader,
  CardContent,
  Button,
  Tabs,
  Flex,
  Alert
} from '../../components/core';
import { Card as AntCard, Button as AntButton, Space, Typography, Divider, Row, Col } from 'antd';
import { RobotOutlined, BugOutlined, PlayCircleOutlined, CheckCircleOutlined, ExclamationCircleOutlined, ApiOutlined } from '@ant-design/icons';
import { LLMDashboard } from '../../components/LLMManagement/LLMDashboard';
import { ProviderSettings } from '../../components/LLMManagement/ProviderSettings';
import { ModelConfiguration } from '../../components/LLMManagement/ModelConfiguration';
import { UsageAnalytics } from '../../components/LLMManagement/UsageAnalytics';
import { CostMonitoring } from '../../components/LLMManagement/CostMonitoring';
import { PerformanceMonitoring } from '../../components/LLMManagement/PerformanceMonitoring';
import { LLMSelector } from '../../components/AI/LLMSelector';
import { LLMStatusWidget } from '../../components/AI/LLMStatusWidget';
import { QueryInput } from '../../components/QueryInterface/QueryInput';
import { useAuthStore } from '../../stores/authStore';
import { llmManagementService } from '../../services/llmManagementService';

const { Text } = Typography;

const LLMManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');
  const { user, isAdmin } = useAuthStore();

  // Debug functionality state
  const [debugResults, setDebugResults] = useState<any[]>([]);
  const [debugTesting, setDebugTesting] = useState(false);

  // Test functionality state
  const [selectedProviderId, setSelectedProviderId] = useState<string>();
  const [selectedModelId, setSelectedModelId] = useState<string>();
  const [testResults, setTestResults] = useState<any[]>([]);
  const [testing, setTesting] = useState(false);

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
      <div style={{ padding: '24px' }}>
        <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
            <RobotOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
            LLM Management
          </h1>
          <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
            Access Denied
          </p>
        </div>

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
          variant="error"
          style={{ maxWidth: '800px' }}
        />
      </div>
    );
  }

  const tabItems = [
    {
      key: 'dashboard',
      label: '📊 Dashboard',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <CardHeader>
              <h3 className="text-xl font-semibold" style={{ margin: 0 }}>LLM Management Dashboard</h3>
            </CardHeader>
            <CardContent>
              <LLMDashboard />
            </CardContent>
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
            <CardHeader>
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
            </CardHeader>
            <CardContent>
              <ProviderSettings />
            </CardContent>
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
            <CardHeader>
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
            </CardHeader>
            <CardContent>
              <ModelConfiguration />
            </CardContent>
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
            <CardHeader>
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
            </CardHeader>
            <CardContent>
              <UsageAnalytics />
            </CardContent>
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
            <CardHeader>
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
            </CardHeader>
            <CardContent>
              <CostMonitoring />
            </CardContent>
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
            <CardHeader>
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
            </CardHeader>
            <CardContent>
              <PerformanceMonitoring />
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
          <RobotOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          LLM Management
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Configure AI providers, monitor usage, track costs, and optimize performance
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

export default LLMManagementPage;
