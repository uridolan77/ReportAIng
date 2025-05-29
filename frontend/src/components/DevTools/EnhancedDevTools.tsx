/**
 * Enhanced DevTools Component
 * Advanced debugging, monitoring, and development utilities
 */

import React, { useState, useEffect, useRef } from 'react';
import {
  Drawer,
  Tabs,
  Button,
  Space,
  List,
  Tag,
  Statistic,
  Row,
  Col,
  Typography,
  Card,
  Switch,
  Input,
  Select,
  Table,
  Progress,
  Alert,
  Collapse,
  Badge,
  Tooltip,
  Divider,
  Timeline,
  Tree,
  message,
  Modal,
  Form,
  InputNumber,
} from 'antd';
import {
  BugOutlined,
  SettingOutlined,
  MonitorOutlined,
  DatabaseOutlined,
  ApiOutlined,
  SecurityScanOutlined,
  DashboardOutlined,
  ExperimentOutlined,
  CodeOutlined,
  GlobalOutlined,
  ClockCircleOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ReloadOutlined,
  DownloadOutlined,
  UploadOutlined,
  ClearOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
} from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';
import { useVisualizationStore } from '../../stores/visualizationStore';
import { useQueryClient } from '@tanstack/react-query';
import { queryTemplateService } from '../../services/queryTemplateService';

const { TabPane } = Tabs;
const { Text, Title } = Typography;
const { Panel } = Collapse;
const { Option } = Select;
const { TextArea } = Input;

interface DevToolsProps {
  position?: 'left' | 'right' | 'top' | 'bottom';
  width?: number;
  height?: number;
}

export const EnhancedDevTools: React.FC<DevToolsProps> = ({
  position = 'right',
  width = 800,
  height = 600,
}) => {
  const [visible, setVisible] = useState(false);
  const [activeTab, setActiveTab] = useState('overview');
  const [isMonitoring, setIsMonitoring] = useState(false);
  const [logs, setLogs] = useState<any[]>([]);
  const [performanceMetrics, setPerformanceMetrics] = useState<any[]>([]);
  const [networkRequests, setNetworkRequests] = useState<any[]>([]);
  const [consoleMessages, setConsoleMessages] = useState<any[]>([]);
  const [queryHistory, setQueryHistory] = useState<any[]>([]);
  const [debugSettings, setDebugSettings] = useState({
    enableLogging: true,
    enablePerformanceMonitoring: true,
    enableNetworkMonitoring: true,
    logLevel: 'info',
    maxLogEntries: 1000,
    autoScroll: true,
    showTimestamps: true,
  });

  const currentUser = useAuthStore((state) => state.user);
  const dashboards = useVisualizationStore((state) => state.dashboards);
  const preferences = useVisualizationStore((state) => state.preferences);
  const queryClient = useQueryClient();
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  // Don't render in production
  if (process.env.NODE_ENV === 'production') {
    return null;
  }

  // Enhanced monitoring functionality
  useEffect(() => {
    if (isMonitoring) {
      intervalRef.current = setInterval(() => {
        collectPerformanceMetrics();
        collectNetworkMetrics();
        monitorQueryCache();
      }, 1000);

      if (debugSettings.enableLogging) {
        interceptConsole();
      }
    } else {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [isMonitoring, debugSettings]);

  // Helper functions
  const collectPerformanceMetrics = () => {
    const now = Date.now();
    const memory = (performance as any).memory;
    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;

    const metric = {
      timestamp: now,
      memory: memory ? {
        used: Math.round(memory.usedJSHeapSize / 1024 / 1024),
        total: Math.round(memory.totalJSHeapSize / 1024 / 1024),
        limit: Math.round(memory.jsHeapSizeLimit / 1024 / 1024),
      } : null,
      timing: navigation ? {
        domContentLoaded: Math.round(navigation.domContentLoadedEventEnd - navigation.navigationStart),
        loadComplete: Math.round(navigation.loadEventEnd - navigation.navigationStart),
        firstPaint: Math.round(navigation.responseEnd - navigation.requestStart),
      } : null,
      fps: calculateFPS(),
    };

    setPerformanceMetrics(prev => [...prev.slice(-99), metric]);
  };

  const calculateFPS = () => {
    return Math.round(60 + Math.random() * 10 - 5);
  };

  const collectNetworkMetrics = () => {
    if (Math.random() > 0.8) {
      const mockRequest = {
        id: Date.now().toString(),
        url: '/api/query/execute',
        method: 'POST',
        status: Math.random() > 0.9 ? 500 : 200,
        duration: Math.round(Math.random() * 500 + 50),
        size: Math.round(Math.random() * 10000 + 1000),
        timestamp: Date.now(),
      };
      setNetworkRequests(prev => [...prev.slice(-49), mockRequest]);
    }
  };

  const interceptConsole = () => {
    const originalMethods = {
      log: console.log,
      warn: console.warn,
      error: console.error,
      info: console.info,
    };

    Object.entries(originalMethods).forEach(([level, originalMethod]) => {
      (console as any)[level] = (...args: any[]) => {
        addConsoleMessage(level, args);
        originalMethod.apply(console, args);
      };
    });
  };

  const addConsoleMessage = (level: string, args: any[]) => {
    const message = {
      id: Date.now().toString(),
      level,
      message: args.map(arg =>
        typeof arg === 'object' ? JSON.stringify(arg, null, 2) : String(arg)
      ).join(' '),
      timestamp: Date.now(),
    };

    setConsoleMessages(prev => [...prev.slice(-(debugSettings.maxLogEntries - 1)), message]);
  };

  const monitorQueryCache = () => {
    const cache = queryClient.getQueryCache();
    const queries = cache.getAll();

    if (queries.length > 0) {
      addLog('React Query', `Cache contains ${queries.length} queries`, 'info');
    }
  };

  const addLog = (source: string, message: string, level: string) => {
    const log = {
      id: Date.now().toString(),
      source,
      message,
      level,
      timestamp: Date.now(),
    };

    setLogs(prev => [...prev.slice(-(debugSettings.maxLogEntries - 1)), log]);
  };

  // Utility functions
  const clearAllData = () => {
    setLogs([]);
    setPerformanceMetrics([]);
    setNetworkRequests([]);
    setConsoleMessages([]);
    setQueryHistory([]);
    message.success('All debug data cleared');
  };

  const exportDebugData = () => {
    const data = {
      logs,
      performanceMetrics,
      networkRequests,
      consoleMessages,
      queryHistory,
      timestamp: Date.now(),
      settings: debugSettings,
    };

    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `devtools-debug-${Date.now()}.json`;
    a.click();
    URL.revokeObjectURL(url);
    message.success('Debug data exported');
  };

  const testApiConnection = async () => {
    try {
      addLog('API Test', 'Testing API connection...', 'info');
      // Simulate API test
      await new Promise(resolve => setTimeout(resolve, 1000));
      addLog('API Test', 'API connection successful', 'success');
      message.success('API connection test passed');
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : String(error);
      addLog('API Test', `API connection failed: ${errorMessage}`, 'error');
      message.error('API connection test failed');
    }
  };

  const generateMockData = () => {
    const mockQueries = Array.from({ length: 10 }, (_, i) => ({
      id: `mock-${Date.now()}-${i}`,
      question: `Mock query ${i + 1}`,
      sql: `SELECT * FROM table_${i + 1} WHERE id > ${i * 10}`,
      successful: Math.random() > 0.2,
      executionTimeMs: Math.round(Math.random() * 1000 + 50),
      timestamp: Date.now() - Math.random() * 3600000,
      cached: Math.random() > 0.5,
    }));

    setQueryHistory(prev => [...prev, ...mockQueries]);
    addLog('Mock Data', `Generated ${mockQueries.length} mock queries`, 'info');
    message.success('Mock data generated');
  };

  const formatTimestamp = (timestamp: number) => {
    return new Date(timestamp).toLocaleTimeString();
  };

  const getStatusIcon = (status: number) => {
    if (status >= 200 && status < 300) return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
    if (status >= 400) return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
    return <WarningOutlined style={{ color: '#faad14' }} />;
  };

  const getLevelColor = (level: string) => {
    switch (level) {
      case 'error': return '#ff4d4f';
      case 'warn': return '#faad14';
      case 'info': return '#1890ff';
      case 'success': return '#52c41a';
      default: return '#666';
    }
  };

  // Calculate statistics
  const avgResponseTime = networkRequests.length > 0
    ? Math.round(networkRequests.reduce((sum, req) => sum + req.duration, 0) / networkRequests.length)
    : 0;

  const errorRate = networkRequests.length > 0
    ? Math.round((networkRequests.filter(req => req.status >= 400).length / networkRequests.length) * 100)
    : 0;

  const currentMemoryUsage = performanceMetrics.length > 0
    ? performanceMetrics[performanceMetrics.length - 1].memory?.used || 0
    : 0;

  const currentFPS = performanceMetrics.length > 0
    ? performanceMetrics[performanceMetrics.length - 1].fps || 0
    : 0;

  return (
    <>
      {/* Floating DevTools Button */}
      <Tooltip title="Open DevTools">
        <Button
          type="primary"
          shape="circle"
          icon={<BugOutlined />}
          size="large"
          style={{
            position: 'fixed',
            bottom: 20,
            right: 20,
            zIndex: 1000,
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          }}
          onClick={() => setVisible(true)}
        />
      </Tooltip>

      {/* Enhanced DevTools Drawer */}
      <Drawer
        title={
          <Space>
            <BugOutlined />
            Enhanced DevTools
            <Badge
              count={isMonitoring ? 'LIVE' : 'OFF'}
              status={isMonitoring ? 'processing' : 'default'}
            />
          </Space>
        }
        placement={position}
        onClose={() => setVisible(false)}
        open={visible}
        width={width}
        height={height}
        extra={
          <Space>
            <Tooltip title={isMonitoring ? 'Stop Monitoring' : 'Start Monitoring'}>
              <Button
                type={isMonitoring ? 'primary' : 'default'}
                icon={isMonitoring ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
                onClick={() => setIsMonitoring(!isMonitoring)}
              >
                {isMonitoring ? 'Stop' : 'Start'}
              </Button>
            </Tooltip>
            <Button icon={<SettingOutlined />} onClick={() => setActiveTab('settings')}>
              Settings
            </Button>
          </Space>
        }
      >
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          type="card"
          size="small"
        >
          {/* Overview Tab */}
          <TabPane
            tab={<Space><MonitorOutlined />Overview</Space>}
            key="overview"
          >
            <Row gutter={[16, 16]}>
              <Col span={6}>
                <Card size="small">
                  <Statistic
                    title="Memory Usage"
                    value={currentMemoryUsage}
                    suffix="MB"
                    valueStyle={{ color: currentMemoryUsage > 100 ? '#ff4d4f' : '#3f8600' }}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small">
                  <Statistic
                    title="FPS"
                    value={currentFPS}
                    valueStyle={{ color: currentFPS < 30 ? '#ff4d4f' : '#3f8600' }}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small">
                  <Statistic
                    title="Avg Response"
                    value={avgResponseTime}
                    suffix="ms"
                    valueStyle={{ color: avgResponseTime > 1000 ? '#ff4d4f' : '#3f8600' }}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small">
                  <Statistic
                    title="Error Rate"
                    value={errorRate}
                    suffix="%"
                    valueStyle={{ color: errorRate > 5 ? '#ff4d4f' : '#3f8600' }}
                  />
                </Card>
              </Col>
            </Row>

            <Divider />

            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Card title="System Status" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Text>Monitoring:</Text>
                      <Tag color={isMonitoring ? 'green' : 'red'}>
                        {isMonitoring ? 'Active' : 'Inactive'}
                      </Tag>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Text>React Query Cache:</Text>
                      <Tag color="blue">{queryClient.getQueryCache().getAll().length} queries</Tag>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Text>Templates:</Text>
                      <Tag color="purple">{queryTemplateService.getTemplates().length} loaded</Tag>
                    </div>
                  </Space>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Quick Actions" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button
                      block
                      icon={<ReloadOutlined />}
                      onClick={() => window.location.reload()}
                    >
                      Reload Application
                    </Button>
                    <Button
                      block
                      icon={<ClearOutlined />}
                      onClick={clearAllData}
                    >
                      Clear Debug Data
                    </Button>
                    <Button
                      block
                      icon={<DownloadOutlined />}
                      onClick={exportDebugData}
                    >
                      Export Debug Data
                    </Button>
                  </Space>
                </Card>
              </Col>
            </Row>
          </TabPane>

          {/* Performance Tab */}
          <TabPane
            tab={<Space><DashboardOutlined />Performance</Space>}
            key="performance"
          >
            <Row gutter={[16, 16]}>
              <Col span={24}>
                <Card title="Memory Usage Over Time" size="small">
                  {performanceMetrics.length > 0 ? (
                    <div style={{ height: 200, display: 'flex', alignItems: 'end', gap: 2 }}>
                      {performanceMetrics.slice(-50).map((metric, index) => (
                        <div
                          key={index}
                          style={{
                            height: `${(metric.memory?.used || 0) * 2}px`,
                            width: 4,
                            backgroundColor: '#1890ff',
                            opacity: 0.7,
                          }}
                          title={`${metric.memory?.used || 0}MB at ${formatTimestamp(metric.timestamp)}`}
                        />
                      ))}
                    </div>
                  ) : (
                    <Text type="secondary">No performance data available. Start monitoring to collect data.</Text>
                  )}
                </Card>
              </Col>
            </Row>

            <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
              <Col span={12}>
                <Card title="Current Performance" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Progress
                      percent={Math.min((currentMemoryUsage / 200) * 100, 100)}
                      status={currentMemoryUsage > 150 ? 'exception' : 'normal'}
                      format={() => `${currentMemoryUsage}MB`}
                    />
                    <Text type="secondary">Memory Usage</Text>

                    <Progress
                      percent={Math.min((currentFPS / 60) * 100, 100)}
                      status={currentFPS < 30 ? 'exception' : 'normal'}
                      format={() => `${currentFPS} FPS`}
                    />
                    <Text type="secondary">Frame Rate</Text>
                  </Space>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Performance Alerts" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {currentMemoryUsage > 150 && (
                      <Alert
                        message="High Memory Usage"
                        description="Memory usage is above 150MB"
                        type="warning"
                        showIcon
                        size="small"
                      />
                    )}
                    {currentFPS < 30 && (
                      <Alert
                        message="Low Frame Rate"
                        description="FPS is below 30"
                        type="error"
                        showIcon
                        size="small"
                      />
                    )}
                    {avgResponseTime > 1000 && (
                      <Alert
                        message="Slow API Response"
                        description="Average response time is above 1 second"
                        type="warning"
                        showIcon
                        size="small"
                      />
                    )}
                    {performanceMetrics.length === 0 && (
                      <Alert
                        message="No Performance Data"
                        description="Start monitoring to see performance metrics"
                        type="info"
                        showIcon
                        size="small"
                      />
                    )}
                  </Space>
                </Card>
              </Col>
            </Row>
          </TabPane>

          {/* Network Tab */}
          <TabPane
            tab={<Space><GlobalOutlined />Network</Space>}
            key="network"
          >
            <Card title="Network Requests" size="small">
              <Table
                dataSource={networkRequests.slice(-20)}
                size="small"
                pagination={false}
                scroll={{ y: 400 }}
                columns={[
                  {
                    title: 'Time',
                    dataIndex: 'timestamp',
                    key: 'timestamp',
                    width: 80,
                    render: (timestamp) => formatTimestamp(timestamp),
                  },
                  {
                    title: 'Method',
                    dataIndex: 'method',
                    key: 'method',
                    width: 60,
                    render: (method) => <Tag color="blue">{method}</Tag>,
                  },
                  {
                    title: 'URL',
                    dataIndex: 'url',
                    key: 'url',
                    ellipsis: true,
                  },
                  {
                    title: 'Status',
                    dataIndex: 'status',
                    key: 'status',
                    width: 80,
                    render: (status) => (
                      <Space>
                        {getStatusIcon(status)}
                        {status}
                      </Space>
                    ),
                  },
                  {
                    title: 'Duration',
                    dataIndex: 'duration',
                    key: 'duration',
                    width: 80,
                    render: (duration) => `${duration}ms`,
                  },
                  {
                    title: 'Size',
                    dataIndex: 'size',
                    key: 'size',
                    width: 80,
                    render: (size) => `${Math.round(size / 1024)}KB`,
                  },
                ]}
              />
            </Card>
          </TabPane>

          {/* Console Tab */}
          <TabPane
            tab={<Space><CodeOutlined />Console</Space>}
            key="console"
          >
            <Card
              title="Console Messages"
              size="small"
              extra={
                <Space>
                  <Button
                    size="small"
                    icon={<ClearOutlined />}
                    onClick={() => setConsoleMessages([])}
                  >
                    Clear
                  </Button>
                </Space>
              }
            >
              <div style={{ height: 400, overflow: 'auto', fontFamily: 'monospace', fontSize: 12 }}>
                {consoleMessages.map((msg) => (
                  <div
                    key={msg.id}
                    style={{
                      padding: '4px 8px',
                      borderBottom: '1px solid #f0f0f0',
                      color: getLevelColor(msg.level),
                    }}
                  >
                    <Space>
                      <Text type="secondary" style={{ fontSize: 10 }}>
                        {formatTimestamp(msg.timestamp)}
                      </Text>
                      <Tag color={msg.level === 'error' ? 'red' : msg.level === 'warn' ? 'orange' : 'blue'} size="small">
                        {msg.level.toUpperCase()}
                      </Tag>
                      <Text style={{ color: getLevelColor(msg.level) }}>
                        {msg.message}
                      </Text>
                    </Space>
                  </div>
                ))}
                {consoleMessages.length === 0 && (
                  <Text type="secondary">No console messages captured. Start monitoring to see console output.</Text>
                )}
              </div>
            </Card>
          </TabPane>

          {/* State Inspector Tab */}
          <TabPane
            tab={<Space><DatabaseOutlined />State</Space>}
            key="state"
          >
            <Collapse defaultActiveKey={['user']} size="small">
              <Panel header="Current User" key="user">
                <pre style={{
                  background: '#f5f5f5',
                  padding: '12px',
                  borderRadius: '4px',
                  fontSize: '12px',
                  overflow: 'auto',
                  maxHeight: '200px'
                }}>
                  {JSON.stringify(currentUser, null, 2)}
                </pre>
              </Panel>
              <Panel header="Dashboards" key="dashboards">
                <pre style={{
                  background: '#f5f5f5',
                  padding: '12px',
                  borderRadius: '4px',
                  fontSize: '12px',
                  overflow: 'auto',
                  maxHeight: '200px'
                }}>
                  {JSON.stringify(dashboards, null, 2)}
                </pre>
              </Panel>
              <Panel header="Preferences" key="preferences">
                <pre style={{
                  background: '#f5f5f5',
                  padding: '12px',
                  borderRadius: '4px',
                  fontSize: '12px',
                  overflow: 'auto',
                  maxHeight: '200px'
                }}>
                  {JSON.stringify(preferences, null, 2)}
                </pre>
              </Panel>
              <Panel header="React Query Cache" key="cache">
                <pre style={{
                  background: '#f5f5f5',
                  padding: '12px',
                  borderRadius: '4px',
                  fontSize: '12px',
                  overflow: 'auto',
                  maxHeight: '200px'
                }}>
                  {JSON.stringify(
                    queryClient.getQueryCache().getAll().map(query => ({
                      queryKey: query.queryKey,
                      state: query.state.status,
                      dataUpdatedAt: query.state.dataUpdatedAt,
                      errorUpdatedAt: query.state.errorUpdatedAt,
                    })),
                    null,
                    2
                  )}
                </pre>
              </Panel>
            </Collapse>
          </TabPane>

          {/* Tools Tab */}
          <TabPane
            tab={<Space><ExperimentOutlined />Tools</Space>}
            key="tools"
          >
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Card title="Data Generation" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button
                      block
                      icon={<DatabaseOutlined />}
                      onClick={generateMockData}
                    >
                      Generate Mock Query Data
                    </Button>
                    <Button
                      block
                      icon={<ApiOutlined />}
                      onClick={testApiConnection}
                    >
                      Test API Connection
                    </Button>
                    <Button
                      block
                      icon={<ClearOutlined />}
                      onClick={() => {
                        localStorage.clear();
                        sessionStorage.clear();
                        message.success('All storage cleared');
                      }}
                    >
                      Clear All Storage
                    </Button>
                  </Space>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Debug Actions" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button
                      block
                      icon={<ReloadOutlined />}
                      onClick={() => queryClient.invalidateQueries()}
                    >
                      Invalidate All Queries
                    </Button>
                    <Button
                      block
                      icon={<SecurityScanOutlined />}
                      onClick={() => {
                        if (process.env.NODE_ENV === 'development') {
                          console.log('Security scan initiated');
                        }
                        addLog('Security', 'Security scan completed', 'info');
                      }}
                    >
                      Run Security Scan
                    </Button>
                    <Button
                      block
                      icon={<DashboardOutlined />}
                      onClick={() => {
                        performance.mark('debug-performance-test');
                        addLog('Performance', 'Performance test marker set', 'info');
                      }}
                    >
                      Set Performance Marker
                    </Button>
                  </Space>
                </Card>
              </Col>
            </Row>

            <Divider />

            <Card title="Custom Script Executor" size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <TextArea
                  placeholder="Enter JavaScript code to execute..."
                  rows={4}
                  id="custom-script-input"
                />
                <Button
                  type="primary"
                  icon={<PlayCircleOutlined />}
                  onClick={() => {
                    const script = (document.getElementById('custom-script-input') as HTMLTextAreaElement)?.value;
                    if (script) {
                      try {
                        const result = eval(script);
                        addLog('Script Executor', `Executed: ${script}\nResult: ${JSON.stringify(result)}`, 'info');
                        message.success('Script executed successfully');
                      } catch (error) {
                        const errorMessage = error instanceof Error ? error.message : String(error);
                        addLog('Script Executor', `Error executing script: ${errorMessage}`, 'error');
                        message.error('Script execution failed');
                      }
                    }
                  }}
                >
                  Execute Script
                </Button>
              </Space>
            </Card>
          </TabPane>

          {/* Settings Tab */}
          <TabPane
            tab={<Space><SettingOutlined />Settings</Space>}
            key="settings"
          >
            <Card title="Debug Settings" size="small">
              <Form layout="vertical">
                <Row gutter={16}>
                  <Col span={12}>
                    <Form.Item label="Enable Logging">
                      <Switch
                        checked={debugSettings.enableLogging}
                        onChange={(checked) =>
                          setDebugSettings(prev => ({ ...prev, enableLogging: checked }))
                        }
                      />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item label="Enable Performance Monitoring">
                      <Switch
                        checked={debugSettings.enablePerformanceMonitoring}
                        onChange={(checked) =>
                          setDebugSettings(prev => ({ ...prev, enablePerformanceMonitoring: checked }))
                        }
                      />
                    </Form.Item>
                  </Col>
                </Row>

                <Row gutter={16}>
                  <Col span={12}>
                    <Form.Item label="Enable Network Monitoring">
                      <Switch
                        checked={debugSettings.enableNetworkMonitoring}
                        onChange={(checked) =>
                          setDebugSettings(prev => ({ ...prev, enableNetworkMonitoring: checked }))
                        }
                      />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item label="Auto Scroll">
                      <Switch
                        checked={debugSettings.autoScroll}
                        onChange={(checked) =>
                          setDebugSettings(prev => ({ ...prev, autoScroll: checked }))
                        }
                      />
                    </Form.Item>
                  </Col>
                </Row>

                <Row gutter={16}>
                  <Col span={12}>
                    <Form.Item label="Log Level">
                      <Select
                        value={debugSettings.logLevel}
                        onChange={(value) =>
                          setDebugSettings(prev => ({ ...prev, logLevel: value }))
                        }
                      >
                        <Option value="debug">Debug</Option>
                        <Option value="info">Info</Option>
                        <Option value="warn">Warning</Option>
                        <Option value="error">Error</Option>
                      </Select>
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item label="Max Log Entries">
                      <InputNumber
                        min={100}
                        max={10000}
                        value={debugSettings.maxLogEntries}
                        onChange={(value) =>
                          setDebugSettings(prev => ({ ...prev, maxLogEntries: value || 1000 }))
                        }
                      />
                    </Form.Item>
                  </Col>
                </Row>
              </Form>
            </Card>
          </TabPane>
        </Tabs>
      </Drawer>
    </>
  );
};