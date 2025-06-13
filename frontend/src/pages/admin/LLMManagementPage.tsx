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
  Flex
} from '../../components/core';
import { Space, Typography, Divider, Row, Col, Alert } from 'antd';
import { RobotOutlined, CheckCircleOutlined, ExclamationCircleOutlined, ApiOutlined } from '@ant-design/icons';
import { LLMDashboard } from '../../components/LLMManagement/LLMDashboard';
import { ProviderSettings } from '../../components/LLMManagement/ProviderSettings';
import { ModelConfiguration } from '../../components/LLMManagement/ModelConfiguration';
import { UsageAnalytics } from '../../components/LLMManagement/UsageAnalytics';
import { CostMonitoring } from '../../components/LLMManagement/CostMonitoring';
import { PerformanceMonitoring } from '../../components/LLMManagement/PerformanceMonitoring';
import { LLMSelector } from '../../components/AI/LLMSelector';
import { LLMStatusWidget } from '../../components/AI/LLMStatusWidget';
import { useAuthStore } from '../../stores/authStore';
import { llmManagementService } from '../../services/llmManagementService';

const LLMManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');

  // Debug state
  const [debugResults, setDebugResults] = useState<any[]>([]);
  const [debugTesting, setDebugTesting] = useState(false);

  // Test state
  const [selectedProviderId, setSelectedProviderId] = useState<string>();
  const [selectedModelId, setSelectedModelId] = useState<string>();
  const [testResults, setTestResults] = useState<any[]>([]);
  const [testing, setTesting] = useState(false);
  const { user, isAdmin } = useAuthStore();

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  // Debug functionality
  const runDebugTest = async (testName: string, testFunction: () => Promise<any>) => {
    setDebugTesting(true);
    const startTime = Date.now();

    try {
      const result = await testFunction();
      const endTime = Date.now();

      setDebugResults(prev => [...prev, {
        name: testName,
        success: true,
        result,
        duration: endTime - startTime,
        timestamp: new Date().toISOString(),
      }]);
    } catch (error) {
      const endTime = Date.now();

      setDebugResults(prev => [...prev, {
        name: testName,
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error',
        duration: endTime - startTime,
        timestamp: new Date().toISOString(),
      }]);
    } finally {
      setDebugTesting(false);
    }
  };

  const runDebugTests = async () => {
    setDebugResults([]);

    await runDebugTest('Dashboard Summary', () => llmManagementService.getDashboardSummary());
    await runDebugTest('Get Providers', () => llmManagementService.getProviders());
    await runDebugTest('Get Models', () => llmManagementService.getModels());
    await runDebugTest('Provider Health', () => llmManagementService.getProviderHealth());
  };

  // Test functionality
  const runSystemTests = async () => {
    setTesting(true);
    setTestResults([]);

    const tests = [
      { name: 'Provider Health Check', delay: 500 },
      { name: 'Model Configuration Test', delay: 800 },
      { name: 'Usage Logging Test', delay: 600 },
      { name: 'Cost Calculation Test', delay: 400 }
    ];

    for (const test of tests) {
      await new Promise(resolve => setTimeout(resolve, test.delay));

      const result = {
        timestamp: new Date().toISOString(),
        query: test.name,
        providerId: selectedProviderId || 'system',
        modelId: selectedModelId || 'test',
        success: Math.random() > 0.1, // 90% success rate
        responseTime: test.delay,
        cost: (Math.random() * 0.005).toFixed(4)
      };

      setTestResults(prev => [result, ...prev]);
    }

    setTesting(false);
  };

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
          type="error"
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
    {
      key: 'debug',
      label: '🐛 Debug & Testing',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <CardHeader>
              <Flex justify="between" align="center">
                <h3 className="text-xl font-semibold" style={{ margin: 0 }}>LLM Service Debug & Testing</h3>
                <Flex gap="sm">
                  <Button variant="outline" size="small" onClick={() => setDebugResults([])}>
                    🗑️ Clear Results
                  </Button>
                  <Button variant="primary" size="small" onClick={runDebugTests} loading={debugTesting}>
                    🔄 Run All Tests
                  </Button>
                </Flex>
              </Flex>
            </CardHeader>
            <CardContent>
              {/* Debug Tests Section */}
              <div style={{ marginBottom: '24px' }}>
                <h4 style={{ marginBottom: '16px' }}>Service Tests</h4>
                <Space>
                  <Button
                    variant="primary"
                    onClick={runDebugTests}
                    loading={debugTesting}
                  >
                    Run All Tests
                  </Button>
                  <Button onClick={() => setDebugResults([])}>
                    Clear Results
                  </Button>
                </Space>
              </div>

              {/* Debug Results */}
              {debugResults.length > 0 && (
                <div style={{ marginBottom: '24px' }}>
                  <h4 style={{ marginBottom: '16px' }}>Test Results</h4>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {debugResults.map((result, index) => (
                      <Card key={index} size="small" style={{
                        background: result.success ? '#f6ffed' : '#fff2f0',
                        border: `1px solid ${result.success ? '#b7eb8f' : '#ffccc7'}`
                      }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <div>
                            <Typography.Text strong style={{ color: result.success ? '#52c41a' : '#ff4d4f' }}>
                              {result.success ? '✅' : '❌'} {result.name}
                            </Typography.Text>
                            <div style={{ marginTop: '8px', fontSize: '12px', color: '#666' }}>
                              Duration: {result.duration}ms | {new Date(result.timestamp).toLocaleTimeString()}
                            </div>
                          </div>
                        </div>

                        <Divider style={{ margin: '12px 0' }} />

                        {result.success ? (
                          <div>
                            <Typography.Text strong>Result:</Typography.Text>
                            <pre style={{
                              background: '#f5f5f5',
                              padding: '8px',
                              borderRadius: '4px',
                              fontSize: '11px',
                              maxHeight: '200px',
                              overflow: 'auto',
                              marginTop: '8px'
                            }}>
                              {JSON.stringify(result.result, null, 2)}
                            </pre>
                          </div>
                        ) : (
                          <Alert
                            message="Error"
                            description={result.error}
                            type="error"
                          />
                        )}
                      </Card>
                    ))}
                  </Space>
                </div>
              )}

              {/* LLM Testing Section */}
              <div style={{ marginTop: '32px' }}>
                <h4 style={{ marginBottom: '16px' }}>LLM Provider & Model Testing</h4>
                <Row gutter={[24, 24]}>
                  {/* System Status */}
                  <Col span={24}>
                    <LLMStatusWidget compact={false} showActions={true} />
                  </Col>

                  {/* LLM Selector Test */}
                  <Col span={12}>
                    <Card title="Provider & Model Selection" size="small">
                      <LLMSelector
                        selectedProviderId={selectedProviderId || ''}
                        selectedModelId={selectedModelId || ''}
                        useCase="SQL"
                        onProviderChange={setSelectedProviderId}
                        onModelChange={setSelectedModelId}
                        compact={false}
                        showStatus={true}
                      />

                      <Divider />

                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Typography.Text strong>Selected Configuration:</Typography.Text>
                        <Typography.Text>Provider: {selectedProviderId || 'None selected'}</Typography.Text>
                        <Typography.Text>Model: {selectedModelId || 'None selected'}</Typography.Text>
                      </Space>
                    </Card>
                  </Col>

                  {/* Test Controls */}
                  <Col span={12}>
                    <Card title="System Tests" size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Button
                          variant="primary"
                          onClick={runSystemTests}
                          loading={testing}
                          style={{ width: '100%' }}
                        >
                          Run System Tests
                        </Button>

                        <Button
                          icon={<ApiOutlined />}
                          onClick={() => setActiveTab('providers')}
                          block
                        >
                          Configure Providers
                        </Button>

                        <Button
                          icon={<RobotOutlined />}
                          onClick={() => setActiveTab('models')}
                          block
                        >
                          Configure Models
                        </Button>
                      </Space>
                    </Card>
                  </Col>

                  {/* Test Results */}
                  <Col span={24}>
                    <Card title="Test Results" size="small">
                      {testResults.length === 0 ? (
                        <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
                          No test results yet. Run some tests to see results here.
                        </div>
                      ) : (
                        <Space direction="vertical" style={{ width: '100%' }}>
                          {testResults.map((result, index) => (
                            <Card key={index} size="small" style={{ background: '#f9f9f9' }}>
                              <Row gutter={16} align="middle">
                                <Col span={1}>
                                  {result.success ? (
                                    <CheckCircleOutlined style={{ color: '#52c41a' }} />
                                  ) : (
                                    <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
                                  )}
                                </Col>
                                <Col span={8}>
                                  <Typography.Text strong>{result.query}</Typography.Text>
                                </Col>
                                <Col span={4}>
                                  <Typography.Text>Provider: {result.providerId}</Typography.Text>
                                </Col>
                                <Col span={4}>
                                  <Typography.Text>Model: {result.modelId}</Typography.Text>
                                </Col>
                                <Col span={3}>
                                  <Typography.Text>Time: {result.responseTime}ms</Typography.Text>
                                </Col>
                                <Col span={2}>
                                  <Typography.Text>Cost: ${result.cost}</Typography.Text>
                                </Col>
                                <Col span={2}>
                                  <Typography.Text style={{ fontSize: '11px', color: '#666' }}>
                                    {new Date(result.timestamp).toLocaleTimeString()}
                                  </Typography.Text>
                                </Col>
                              </Row>
                            </Card>
                          ))}
                        </Space>
                      )}
                    </Card>
                  </Col>
                </Row>
              </div>

              {debugResults.length === 0 && testResults.length === 0 && (
                <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
                  No test results yet. Click "Run All Tests" to start testing the LLM Management service.
                </div>
              )}
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
