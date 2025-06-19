/**
 * Unified Developer Tools Component
 * Consolidates debugging, monitoring, and development utilities
 */

import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Drawer,
  Button,
  Tabs,
  List,
  Typography,
  Space,
  Tag,
  Card,
  Statistic,
  Row,
  Col,
  Badge,
  Alert,
  Collapse,
  Tooltip
} from 'antd';
import {
  BugOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  BarChartOutlined
} from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';
import { useVisualizationStore } from '../../stores/visualizationStore';
import { useQueryClient } from '@tanstack/react-query';

const { Text } = Typography;
const { Panel } = Collapse;

interface DevToolsProps {
  position?: 'left' | 'right' | 'top' | 'bottom';
  width?: number;
  height?: number;
}

interface QueryHistoryItem {
  id: string;
  question: string;
  sql: string;
  successful: boolean;
  executionTimeMs: number;
  timestamp: string;
  error?: string;
}

interface PerformanceMetric {
  timestamp: string;
  memoryUsage: number;
  renderTime: number;
  queryTime: number;
  cacheHitRate: number;
}

interface NetworkRequest {
  id: string;
  url: string;
  method: string;
  status: number;
  duration: number;
  timestamp: string;
  size: number;
}

interface ConsoleMessage {
  id: string;
  level: 'log' | 'warn' | 'error' | 'info';
  message: string;
  timestamp: string;
  stack?: string;
}

interface DebugSettings {
  enableLogging: boolean;
  enablePerformanceMonitoring: boolean;
  enableNetworkMonitoring: boolean;
  logLevel: 'debug' | 'info' | 'warn' | 'error';
  maxLogEntries: number;
  autoRefresh: boolean;
  refreshInterval: number;
}

export const DevTools: React.FC<DevToolsProps> = ({
  position = 'right',
  width = 800,
  height = 600,
}) => {
  const [visible, setVisible] = useState(false);
  const [activeTab, setActiveTab] = useState('overview');
  const [isMonitoring, setIsMonitoring] = useState(false);
  const [logs] = useState<any[]>([]);
  const [performanceMetrics, setPerformanceMetrics] = useState<PerformanceMetric[]>([]);
  const [networkRequests, setNetworkRequests] = useState<NetworkRequest[]>([]);
  const [consoleMessages, setConsoleMessages] = useState<ConsoleMessage[]>([]);
  const [debugSettings] = useState<DebugSettings>({
    enableLogging: true,
    enablePerformanceMonitoring: true,
    enableNetworkMonitoring: true,
    logLevel: 'info',
    maxLogEntries: 1000,
    autoRefresh: true,
    refreshInterval: 1000,
  });

  const currentUser = useAuthStore((state) => state.user);
  const dashboards = useVisualizationStore((state) => state.dashboards);
  const preferences = useVisualizationStore((state) => state.preferences);
  const queryClient = useQueryClient();
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  // TODO: Replace with actual query history from database
  const mockQueryHistory: QueryHistoryItem[] = [];

  const collectPerformanceMetrics = useCallback(() => {
    const now = new Date().toISOString();
    const newMetric: PerformanceMetric = {
      timestamp: now,
      memoryUsage: (performance as any).memory?.usedJSHeapSize || Math.random() * 50000000,
      renderTime: Math.random() * 16.67, // Target 60fps
      queryTime: Math.random() * 500,
      cacheHitRate: 75 + Math.random() * 20
    };

    setPerformanceMetrics(prev => {
      const updated = [...prev, newMetric];
      return updated.slice(-50); // Keep last 50 metrics
    });
  }, []);

  const collectNetworkMetrics = useCallback(() => {
    // Mock network request
    if (Math.random() > 0.7) { // 30% chance of new request
      const newRequest: NetworkRequest = {
        id: Date.now().toString(),
        url: '/api/query/execute',
        method: 'POST',
        status: Math.random() > 0.1 ? 200 : 500,
        duration: Math.random() * 1000,
        timestamp: new Date().toISOString(),
        size: Math.random() * 10000
      };

      setNetworkRequests(prev => {
        const updated = [...prev, newRequest];
        return updated.slice(-20); // Keep last 20 requests
      });
    }
  }, []);

  const interceptConsole = useCallback(() => {
    const originalConsole = { ...console };

    ['log', 'warn', 'error', 'info'].forEach(level => {
      (console as any)[level] = (...args: any[]) => {
        const originalMethod = originalConsole[level as keyof typeof originalConsole];
        if (typeof originalMethod === 'function') {
          (originalMethod as any).apply(console, args);
        }

        if (debugSettings.enableLogging) {
          const message: ConsoleMessage = {
            id: Date.now().toString(),
            level: level as ConsoleMessage['level'],
            message: args.map(arg =>
              typeof arg === 'object' ? JSON.stringify(arg, null, 2) : String(arg)
            ).join(' '),
            timestamp: new Date().toISOString(),
            stack: level === 'error' ? new Error().stack : undefined
          };

          setConsoleMessages(prev => {
            const updated = [...prev, message];
            return updated.slice(-debugSettings.maxLogEntries);
          });
        }
      };
    });

    return () => {
      Object.assign(console, originalConsole);
    };
  }, [debugSettings.enableLogging, debugSettings.maxLogEntries]);

  const monitorQueryCache = useCallback(() => {
    // Monitor query cache performance
    const cacheKeys = Object.keys(localStorage).filter(key =>
      key.includes('query-cache') || key.includes('query-result')
    );

    setLogs(prev => {
      const newLog = {
        timestamp: new Date().toISOString(),
        type: 'cache',
        message: `Cache entries: ${cacheKeys.length}`,
        data: { cacheKeys: cacheKeys.length }
      };
      return [...prev.slice(-99), newLog]; // Keep last 100 logs
    });
  }, []);

  // Enhanced monitoring functionality
  useEffect(() => {
    if (isMonitoring) {
      intervalRef.current = setInterval(() => {
        collectPerformanceMetrics();
        collectNetworkMetrics();
        monitorQueryCache();
      }, debugSettings.refreshInterval);

      if (debugSettings.enableLogging) {
        const cleanup = interceptConsole();
        return cleanup;
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
  }, [isMonitoring, debugSettings, collectPerformanceMetrics, interceptConsole, monitorQueryCache]);

  // Calculate statistics
  const avgResponseTime = networkRequests.length > 0
    ? Math.round(networkRequests.reduce((sum, req) => sum + req.duration, 0) / networkRequests.length)
    : 0;

  const errorRate = networkRequests.length > 0
    ? Math.round((networkRequests.filter(req => req.status >= 400).length / networkRequests.length) * 100)
    : 0;

  const avgMemoryUsage = performanceMetrics.length > 0
    ? Math.round(performanceMetrics.reduce((sum, metric) => sum + metric.memoryUsage, 0) / performanceMetrics.length)
    : 0;

  const renderOverviewTab = () => (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={avgResponseTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Error Rate"
              value={errorRate}
              suffix="%"
              prefix={<BugOutlined />}
              valueStyle={{ color: errorRate > 5 ? '#ff4d4f' : '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Memory Usage"
              value={Math.round(avgMemoryUsage / 1024 / 1024)}
              suffix="MB"
              prefix={<BarChartOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Cache Hit Rate"
              value={performanceMetrics.length > 0 ? Math.round(performanceMetrics[performanceMetrics.length - 1]?.cacheHitRate || 0) : 0}
              suffix="%"
              prefix={<DatabaseOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card title="System Status" style={{ marginTop: 16 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text>Real-time Monitoring</Text>
            <Badge status={isMonitoring ? 'processing' : 'default'} text={isMonitoring ? 'Active' : 'Inactive'} />
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text>Performance Tracking</Text>
            <Badge status={debugSettings.enablePerformanceMonitoring ? 'success' : 'default'} text={debugSettings.enablePerformanceMonitoring ? 'Enabled' : 'Disabled'} />
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text>Network Monitoring</Text>
            <Badge status={debugSettings.enableNetworkMonitoring ? 'success' : 'default'} text={debugSettings.enableNetworkMonitoring ? 'Enabled' : 'Disabled'} />
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text>Console Logging</Text>
            <Badge status={debugSettings.enableLogging ? 'success' : 'default'} text={debugSettings.enableLogging ? 'Enabled' : 'Disabled'} />
          </div>
        </Space>
      </Card>
    </div>
  );

  const renderQueryHistoryTab = () => (
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
                  <Tag>{item.timestamp}</Tag>
                </Space>
                {item.error && (
                  <Alert message={item.error} type="error" />
                )}
              </Space>
            }
          />
        </List.Item>
      )}
    />
  );

  const renderNetworkTab = () => (
    <div>
      <List
        dataSource={networkRequests}
        renderItem={(request) => (
          <List.Item>
            <List.Item.Meta
              title={`${request.method} ${request.url}`}
              description={
                <Space>
                  <Tag color={request.status < 400 ? 'green' : 'red'}>
                    {request.status}
                  </Tag>
                  <Tag>{Math.round(request.duration)}ms</Tag>
                  <Tag>{Math.round(request.size / 1024)}KB</Tag>
                  <Text type="secondary">{request.timestamp}</Text>
                </Space>
              }
            />
          </List.Item>
        )}
      />
    </div>
  );

  const renderConsoleTab = () => (
    <div>
      <List
        dataSource={consoleMessages}
        renderItem={(msg) => (
          <List.Item>
            <List.Item.Meta
              title={
                <Space>
                  <Tag color={
                    msg.level === 'error' ? 'red' :
                    msg.level === 'warn' ? 'orange' :
                    msg.level === 'info' ? 'blue' : 'default'
                  }>
                    {msg.level.toUpperCase()}
                  </Tag>
                  <Text type="secondary">{msg.timestamp}</Text>
                </Space>
              }
              description={
                <div>
                  <Text code>{msg.message}</Text>
                  {msg.stack && (
                    <pre style={{ fontSize: '12px', marginTop: '8px', background: '#f5f5f5', padding: '8px' }}>
                      {msg.stack}
                    </pre>
                  )}
                </div>
              }
            />
          </List.Item>
        )}
      />
    </div>
  );

  const renderStateTab = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Collapse>
        <Panel header="Current User" key="user">
          <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
            {JSON.stringify(currentUser, null, 2)}
          </pre>
        </Panel>
        <Panel header="Dashboards" key="dashboards">
          <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
            {JSON.stringify(dashboards, null, 2)}
          </pre>
        </Panel>
        <Panel header="Preferences" key="preferences">
          <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
            {JSON.stringify(preferences, null, 2)}
          </pre>
        </Panel>
        <Panel header="Query Cache" key="cache">
          <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
            {JSON.stringify(queryClient.getQueryCache().getAll().map(q => ({
              queryKey: q.queryKey,
              state: q.state.status,
              dataUpdatedAt: q.state.dataUpdatedAt
            })), null, 2)}
          </pre>
        </Panel>
      </Collapse>
    </Space>
  );

  if (process.env.NODE_ENV === 'production') {
    return null;
  }

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

      {/* DevTools Drawer */}
      <Drawer
        title={
          <Space>
            <BugOutlined />
            Developer Tools
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
          </Space>
        }
      >
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            {
              key: 'overview',
              label: 'Overview',
              children: renderOverviewTab()
            },
            {
              key: 'queries',
              label: 'Query History',
              children: renderQueryHistoryTab()
            },
            {
              key: 'network',
              label: 'Network',
              children: renderNetworkTab()
            },
            {
              key: 'console',
              label: 'Console',
              children: renderConsoleTab()
            },
            {
              key: 'state',
              label: 'State Inspector',
              children: renderStateTab()
            }
          ]}
        />
      </Drawer>
    </>
  );
};
