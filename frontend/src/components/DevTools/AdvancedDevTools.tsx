/**
 * Advanced Development Tools
 * 
 * Comprehensive development tools suite with debugging, profiling,
 * state inspection, and performance analysis capabilities
 */

import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  Drawer,
  Tabs,
  Card,
  Button,
  Space,
  Typography,
  Table,
  Tag,
  Alert,
  Switch,
  Input,
  Select,
  Statistic,
  Progress,
  Timeline,
  Tree,
  Collapse,
  Badge,
  Tooltip
} from 'antd';
import {
  BugOutlined,
  ToolOutlined,
  EyeOutlined,
  ThunderboltOutlined,
  DatabaseOutlined,
  ApiOutlined,
  SettingOutlined,
  CloseOutlined,
  ReloadOutlined,
  DownloadOutlined,
  ClearOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined
} from '@ant-design/icons';
import { useMemoryOptimization } from '../../hooks/useMemoryOptimization';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;
const { Panel } = Collapse;
const { Search } = Input;
const { Option } = Select;

interface DevToolsProps {
  visible: boolean;
  onClose: () => void;
}

interface LogEntry {
  id: string;
  timestamp: number;
  level: 'info' | 'warn' | 'error' | 'debug';
  message: string;
  data?: any;
  component?: string;
  stack?: string;
}

interface PerformanceEntry {
  name: string;
  duration: number;
  startTime: number;
  entryType: string;
}

interface StateSnapshot {
  timestamp: number;
  state: any;
  action?: string;
  component?: string;
}

export const AdvancedDevTools: React.FC<DevToolsProps> = ({ visible, onClose }) => {
  const [activeTab, setActiveTab] = useState('console');
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [performanceEntries, setPerformanceEntries] = useState<PerformanceEntry[]>([]);
  const [stateSnapshots, setStateSnapshots] = useState<StateSnapshot[]>([]);
  const [isRecording, setIsRecording] = useState(false);
  const [filterLevel, setFilterLevel] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [autoScroll, setAutoScroll] = useState(true);

  const logContainerRef = useRef<HTMLDivElement>(null);
  const performanceObserverRef = useRef<PerformanceObserver | null>(null);

  const {
    memoryMetrics,
    getMemoryMetrics,
    performCleanup,
    detectMemoryLeaks
  } = useMemoryOptimization({
    enableAutoCleanup: true,
    enableLeakDetection: true
  });

  // Initialize performance monitoring
  useEffect(() => {
    if (typeof window !== 'undefined' && 'PerformanceObserver' in window) {
      performanceObserverRef.current = new PerformanceObserver((list) => {
        const entries = list.getEntries().map(entry => ({
          name: entry.name,
          duration: entry.duration || 0,
          startTime: entry.startTime,
          entryType: entry.entryType
        }));
        setPerformanceEntries(prev => [...prev, ...entries].slice(-100));
      });

      performanceObserverRef.current.observe({ 
        entryTypes: ['navigation', 'resource', 'measure', 'mark'] 
      });
    }

    return () => {
      if (performanceObserverRef.current) {
        performanceObserverRef.current.disconnect();
      }
    };
  }, []);

  // Console interceptor
  useEffect(() => {
    if (!isRecording) return;

    const originalConsole = {
      log: console.log,
      warn: console.warn,
      error: console.error,
      debug: console.debug
    };

    const createLogEntry = (level: LogEntry['level'], args: any[]): LogEntry => ({
      id: `${Date.now()}-${Math.random()}`,
      timestamp: Date.now(),
      level,
      message: args.map(arg => 
        typeof arg === 'object' ? JSON.stringify(arg, null, 2) : String(arg)
      ).join(' '),
      data: args.length === 1 && typeof args[0] === 'object' ? args[0] : args,
      stack: new Error().stack
    });

    console.log = (...args) => {
      originalConsole.log(...args);
      setLogs(prev => [...prev, createLogEntry('info', args)].slice(-1000));
    };

    console.warn = (...args) => {
      originalConsole.warn(...args);
      setLogs(prev => [...prev, createLogEntry('warn', args)].slice(-1000));
    };

    console.error = (...args) => {
      originalConsole.error(...args);
      setLogs(prev => [...prev, createLogEntry('error', args)].slice(-1000));
    };

    console.debug = (...args) => {
      originalConsole.debug(...args);
      setLogs(prev => [...prev, createLogEntry('debug', args)].slice(-1000));
    };

    return () => {
      Object.assign(console, originalConsole);
    };
  }, [isRecording]);

  // Auto-scroll logs
  useEffect(() => {
    if (autoScroll && logContainerRef.current) {
      logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
    }
  }, [logs, autoScroll]);

  const filteredLogs = logs.filter(log => {
    const levelMatch = filterLevel === 'all' || log.level === filterLevel;
    const searchMatch = !searchTerm || 
      log.message.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.component?.toLowerCase().includes(searchTerm.toLowerCase());
    return levelMatch && searchMatch;
  });

  const getLevelColor = (level: string) => {
    switch (level) {
      case 'error': return '#ff4d4f';
      case 'warn': return '#faad14';
      case 'info': return '#1890ff';
      case 'debug': return '#52c41a';
      default: return '#d9d9d9';
    }
  };

  const exportLogs = useCallback(() => {
    const dataStr = JSON.stringify(logs, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `devtools-logs-${new Date().toISOString()}.json`;
    link.click();
    URL.revokeObjectURL(url);
  }, [logs]);

  const clearLogs = useCallback(() => {
    setLogs([]);
  }, []);

  const captureStateSnapshot = useCallback(() => {
    const snapshot: StateSnapshot = {
      timestamp: Date.now(),
      state: {
        memory: getMemoryMetrics(),
        performance: performanceEntries.slice(-10),
        logs: logs.slice(-5)
      }
    };
    setStateSnapshots(prev => [...prev, snapshot].slice(-50));
  }, [getMemoryMetrics, performanceEntries, logs]);

  const renderConsoleTab = () => (
    <div style={{ height: '500px', display: 'flex', flexDirection: 'column' }}>
      <div style={{ marginBottom: '16px' }}>
        <Space>
          <Button
            type={isRecording ? 'primary' : 'default'}
            icon={isRecording ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
            onClick={() => setIsRecording(!isRecording)}
          >
            {isRecording ? 'Stop Recording' : 'Start Recording'}
          </Button>
          <Select
            value={filterLevel}
            onChange={setFilterLevel}
            style={{ width: 120 }}
          >
            <Option value="all">All Levels</Option>
            <Option value="error">Errors</Option>
            <Option value="warn">Warnings</Option>
            <Option value="info">Info</Option>
            <Option value="debug">Debug</Option>
          </Select>
          <Search
            placeholder="Search logs..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ width: 200 }}
          />
          <Switch
            checked={autoScroll}
            onChange={setAutoScroll}
            checkedChildren="Auto-scroll"
            unCheckedChildren="Manual"
          />
          <Button icon={<ClearOutlined />} onClick={clearLogs}>
            Clear
          </Button>
          <Button icon={<DownloadOutlined />} onClick={exportLogs}>
            Export
          </Button>
        </Space>
      </div>
      
      <div
        ref={logContainerRef}
        style={{
          flex: 1,
          overflow: 'auto',
          border: '1px solid #d9d9d9',
          borderRadius: '6px',
          padding: '8px',
          backgroundColor: '#fafafa',
          fontFamily: 'Monaco, Consolas, monospace',
          fontSize: '12px'
        }}
      >
        {filteredLogs.map(log => (
          <div
            key={log.id}
            style={{
              marginBottom: '4px',
              padding: '4px 8px',
              borderLeft: `3px solid ${getLevelColor(log.level)}`,
              backgroundColor: 'white',
              borderRadius: '3px'
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Space>
                <Tag color={getLevelColor(log.level)} size="small">
                  {log.level.toUpperCase()}
                </Tag>
                <Text type="secondary" style={{ fontSize: '11px' }}>
                  {new Date(log.timestamp).toLocaleTimeString()}
                </Text>
                {log.component && (
                  <Tag size="small">{log.component}</Tag>
                )}
              </Space>
            </div>
            <div style={{ marginTop: '4px', wordBreak: 'break-word' }}>
              {log.message}
            </div>
          </div>
        ))}
        {filteredLogs.length === 0 && (
          <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
            {isRecording ? 'No logs captured yet...' : 'Start recording to capture logs'}
          </div>
        )}
      </div>
    </div>
  );

  const renderPerformanceTab = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Card title="Memory Metrics" size="small">
        {memoryMetrics ? (
          <Space direction="vertical" style={{ width: '100%' }}>
            <Statistic
              title="Used JS Heap Size"
              value={(memoryMetrics.usedJSHeapSize / 1024 / 1024).toFixed(2)}
              suffix="MB"
            />
            <Progress
              percent={Math.round((memoryMetrics.usedJSHeapSize / memoryMetrics.jsHeapSizeLimit) * 100)}
              strokeColor="#52c41a"
              size="small"
            />
            <Space>
              <Button size="small" onClick={performCleanup}>
                Force Cleanup
              </Button>
              <Button size="small" onClick={detectMemoryLeaks}>
                Detect Leaks
              </Button>
            </Space>
          </Space>
        ) : (
          <Text type="secondary">Memory metrics not available</Text>
        )}
      </Card>

      <Card title="Performance Entries" size="small">
        <Table
          dataSource={performanceEntries.slice(-20)}
          columns={[
            { title: 'Name', dataIndex: 'name', key: 'name', ellipsis: true },
            { title: 'Type', dataIndex: 'entryType', key: 'entryType' },
            { 
              title: 'Duration', 
              dataIndex: 'duration', 
              key: 'duration',
              render: (duration: number) => `${duration.toFixed(2)}ms`
            }
          ]}
          size="small"
          pagination={false}
          scroll={{ y: 200 }}
        />
      </Card>
    </Space>
  );

  const renderStateTab = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      <div>
        <Button
          type="primary"
          icon={<EyeOutlined />}
          onClick={captureStateSnapshot}
        >
          Capture State Snapshot
        </Button>
      </div>
      
      <Timeline>
        {stateSnapshots.slice(-10).reverse().map((snapshot, index) => (
          <Timeline.Item key={index}>
            <Card size="small">
              <Text strong>
                {new Date(snapshot.timestamp).toLocaleTimeString()}
              </Text>
              <Collapse size="small" style={{ marginTop: '8px' }}>
                <Panel header="State Data" key="1">
                  <pre style={{ fontSize: '11px', maxHeight: '200px', overflow: 'auto' }}>
                    {JSON.stringify(snapshot.state, null, 2)}
                  </pre>
                </Panel>
              </Collapse>
            </Card>
          </Timeline.Item>
        ))}
      </Timeline>
    </Space>
  );

  return (
    <Drawer
      title={
        <Space>
          <ToolOutlined />
          Advanced Dev Tools
          <Badge count={logs.length} showZero />
        </Space>
      }
      placement="right"
      width={800}
      open={visible}
      onClose={onClose}
      extra={
        <Button type="text" icon={<CloseOutlined />} onClick={onClose} />
      }
    >
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab={<span><BugOutlined />Console</span>} key="console">
          {renderConsoleTab()}
        </TabPane>
        <TabPane tab={<span><ThunderboltOutlined />Performance</span>} key="performance">
          {renderPerformanceTab()}
        </TabPane>
        <TabPane tab={<span><EyeOutlined />State</span>} key="state">
          {renderStateTab()}
        </TabPane>
      </Tabs>
    </Drawer>
  );
};

export default AdvancedDevTools;
