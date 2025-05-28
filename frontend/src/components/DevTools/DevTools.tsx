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
  PerformanceOutlined,
  ExperimentOutlined,
  ConsoleOutlined,
  NetworkOutlined,
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

// Enhanced DevTools with comprehensive debugging and monitoring
export const DevTools: React.FC = () => {
  const [visible, setVisible] = useState(false);
  const [activeTab, setActiveTab] = useState('overview');
  const [isMonitoring, setIsMonitoring] = useState(false);
  const [logs, setLogs] = useState<any[]>([]);
  const [performanceMetrics, setPerformanceMetrics] = useState<any[]>([]);
  const [networkRequests, setNetworkRequests] = useState<any[]>([]);
  const [consoleMessages, setConsoleMessages] = useState<any[]>([]);
  const [debugSettings, setDebugSettings] = useState({
    enableLogging: true,
    enablePerformanceMonitoring: true,
    enableNetworkMonitoring: true,
    logLevel: 'info',
    maxLogEntries: 1000,
  });

  const currentUser = useAuthStore((state) => state.user);
  const dashboards = useVisualizationStore((state) => state.dashboards);
  const preferences = useVisualizationStore((state) => state.preferences);
  const queryClient = useQueryClient();
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  // Enhanced monitoring and debugging functionality
  useEffect(() => {
    if (isMonitoring) {
      // Start performance monitoring
      intervalRef.current = setInterval(() => {
        collectPerformanceMetrics();
        collectNetworkMetrics();
      }, 1000);

      // Override console methods to capture logs
      if (debugSettings.enableLogging) {
        interceptConsole();
      }

      // Monitor React Query cache
      monitorQueryCache();
    } else {
      // Stop monitoring
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

  if (process.env.NODE_ENV === 'production') {
    return null;
  }

  // Enhanced helper functions
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
    // Simple FPS calculation - in real implementation, this would be more sophisticated
    return Math.round(60 + Math.random() * 10 - 5); // Mock FPS between 55-65
  };

  const collectNetworkMetrics = () => {
    // In a real implementation, this would monitor actual network requests
    // For now, we'll simulate network monitoring
    const mockRequest = {
      id: Date.now().toString(),
      url: '/api/query/execute',
      method: 'POST',
      status: 200,
      duration: Math.round(Math.random() * 500 + 50),
      size: Math.round(Math.random() * 10000 + 1000),
      timestamp: Date.now(),
    };

    if (Math.random() > 0.7) { // Only add occasionally to simulate real requests
      setNetworkRequests(prev => [...prev.slice(-49), mockRequest]);
    }
  };

  const interceptConsole = () => {
    const originalLog = console.log;
    const originalWarn = console.warn;
    const originalError = console.error;

    console.log = (...args) => {
      addConsoleMessage('log', args);
      originalLog.apply(console, args);
    };

    console.warn = (...args) => {
      addConsoleMessage('warn', args);
      originalWarn.apply(console, args);
    };

    console.error = (...args) => {
      addConsoleMessage('error', args);
      originalError.apply(console, args);
    };
  };

  const addConsoleMessage = (level: string, args: any[]) => {
    const message = {
      id: Date.now().toString(),
      level,
      message: args.map(arg => typeof arg === 'object' ? JSON.stringify(arg) : String(arg)).join(' '),
      timestamp: Date.now(),
    };

    setConsoleMessages(prev => [...prev.slice(-(debugSettings.maxLogEntries - 1)), message]);
  };

  const monitorQueryCache = () => {
    const cache = queryClient.getQueryCache();
    const queries = cache.getAll();

    const cacheInfo = queries.map(query => ({
      queryKey: JSON.stringify(query.queryKey),
      state: query.state.status,
      dataUpdatedAt: query.state.dataUpdatedAt,
      errorUpdatedAt: query.state.errorUpdatedAt,
      fetchStatus: query.state.fetchStatus,
    }));

    // Add to logs for monitoring
    if (cacheInfo.length > 0) {
      addLog('React Query Cache', `${cacheInfo.length} queries in cache`, 'info');
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

  return (
    <>
      <Button
        type="primary"
        shape="circle"
        icon={<BugOutlined />}
        style={{
          position: 'fixed',
          bottom: 20,
          right: 20,
          zIndex: 1000,
        }}
        onClick={() => setVisible(true)}
      />

      <Drawer
        title="Developer Tools"
        placement="right"
        onClose={() => setVisible(false)}
        open={visible}
        width={600}
      >
        <Tabs defaultActiveKey="queries">
          <TabPane tab="Query History" key="queries">
            <List
              dataSource={mockQueryHistory}
              renderItem={(item) => (
                <List.Item>
                  <List.Item.Meta
                    title={item.question}
                    description={
                      <Space direction="vertical">
                        <Text code>{item.sql}</Text>
                        <Space>
                          <Tag color={item.successful ? 'green' : 'red'}>
                            {item.successful ? 'Success' : 'Failed'}
                          </Tag>
                          <Tag>{item.executionTimeMs}ms</Tag>
                        </Space>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </TabPane>

          <TabPane tab="Performance" key="performance">
            <Row gutter={16}>
              <Col span={8}>
                <Statistic
                  title="Avg Query Time"
                  value={calculateAvgQueryTime(mockQueryHistory)}
                  suffix="ms"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Cache Hit Rate"
                  value={calculateCacheHitRate(mockQueryHistory)}
                  suffix="%"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Total Queries"
                  value={mockQueryHistory.length}
                />
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="State Inspector" key="state">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Current User:</Text>
                <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
                  {JSON.stringify(currentUser, null, 2)}
                </pre>
              </div>
              <div>
                <Text strong>Dashboards:</Text>
                <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
                  {JSON.stringify(dashboards, null, 2)}
                </pre>
              </div>
              <div>
                <Text strong>Preferences:</Text>
                <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
                  {JSON.stringify(preferences, null, 2)}
                </pre>
              </div>
            </Space>
          </TabPane>

          <TabPane tab="Mock Data" key="mock">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button
                onClick={() => {
                  console.log('Generating mock data...');
                  // Add mock data generation logic here
                }}
              >
                Generate Mock Query Data
              </Button>
              <Button
                onClick={() => {
                  console.log('Clearing cache...');
                  localStorage.clear();
                  sessionStorage.clear();
                }}
              >
                Clear All Cache
              </Button>
              <Button
                onClick={() => {
                  console.log('Testing API connection...');
                  // Add connection test logic here
                }}
              >
                Test API Connection
              </Button>
            </Space>
          </TabPane>
        </Tabs>
      </Drawer>
    </>
  );
};
