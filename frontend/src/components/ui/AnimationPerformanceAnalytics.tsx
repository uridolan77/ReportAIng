/**
 * Animation Performance Analytics
 * Real-time monitoring and analytics for animation performance
 */

import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Card, Row, Col, Progress, Statistic, Alert, Switch, Button, Space, Typography, Tooltip, Tag } from 'antd';
import {
  DashboardOutlined,
  ThunderboltOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ReloadOutlined,
  DownloadOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, AreaChart, Area } from 'recharts';

const { Title, Text } = Typography;

interface PerformanceMetrics {
  fps: number;
  frameTime: number;
  memoryUsage: number;
  animationCount: number;
  droppedFrames: number;
  cpuUsage: number;
  timestamp: number;
}

interface AnimationEvent {
  id: string;
  type: 'start' | 'end' | 'frame-drop' | 'performance-warning';
  timestamp: number;
  duration?: number;
  fps?: number;
  elementCount?: number;
}

interface PerformanceThresholds {
  minFPS: number;
  maxFrameTime: number;
  maxMemoryUsage: number;
  maxAnimationCount: number;
}

const DEFAULT_THRESHOLDS: PerformanceThresholds = {
  minFPS: 30,
  maxFrameTime: 16.67, // 60 FPS = 16.67ms per frame
  maxMemoryUsage: 100, // MB
  maxAnimationCount: 10
};

export const AnimationPerformanceAnalytics: React.FC = () => {
  const [isMonitoring, setIsMonitoring] = useState(false);
  const [metrics, setMetrics] = useState<PerformanceMetrics[]>([]);
  const [currentMetrics, setCurrentMetrics] = useState<PerformanceMetrics>({
    fps: 60,
    frameTime: 16.67,
    memoryUsage: 0,
    animationCount: 0,
    droppedFrames: 0,
    cpuUsage: 0,
    timestamp: Date.now()
  });
  const [events, setEvents] = useState<AnimationEvent[]>([]);
  const [thresholds, setThresholds] = useState<PerformanceThresholds>(DEFAULT_THRESHOLDS);
  const [performanceScore, setPerformanceScore] = useState(100);

  const frameCount = useRef(0);
  const lastTime = useRef(performance.now());
  const animationId = useRef<number>(0);
  const memoryObserver = useRef<PerformanceObserver | null>(null);

  // Calculate performance score
  const calculatePerformanceScore = useCallback((metrics: PerformanceMetrics) => {
    let score = 100;
    
    // FPS penalty
    if (metrics.fps < thresholds.minFPS) {
      score -= (thresholds.minFPS - metrics.fps) * 2;
    }
    
    // Frame time penalty
    if (metrics.frameTime > thresholds.maxFrameTime) {
      score -= (metrics.frameTime - thresholds.maxFrameTime) * 3;
    }
    
    // Memory usage penalty
    if (metrics.memoryUsage > thresholds.maxMemoryUsage) {
      score -= (metrics.memoryUsage - thresholds.maxMemoryUsage) * 0.5;
    }
    
    // Animation count penalty
    if (metrics.animationCount > thresholds.maxAnimationCount) {
      score -= (metrics.animationCount - thresholds.maxAnimationCount) * 5;
    }
    
    // Dropped frames penalty
    score -= metrics.droppedFrames * 10;
    
    return Math.max(0, Math.min(100, score));
  }, [thresholds]);

  // Monitor performance
  const monitorPerformance = useCallback(() => {
    if (!isMonitoring) return;

    frameCount.current++;
    const currentTime = performance.now();
    const deltaTime = currentTime - lastTime.current;

    if (deltaTime >= 1000) {
      const fps = Math.round((frameCount.current * 1000) / deltaTime);
      const frameTime = deltaTime / frameCount.current;
      
      // Get memory usage (if available)
      let memoryUsage = 0;
      if ('memory' in performance) {
        const memory = (performance as any).memory;
        memoryUsage = memory.usedJSHeapSize / (1024 * 1024); // Convert to MB
      }

      // Count active animations
      const animationCount = document.querySelectorAll('[style*="animation"], [style*="transition"]').length;
      
      // Detect dropped frames
      const expectedFrames = Math.round(deltaTime / 16.67);
      const droppedFrames = Math.max(0, expectedFrames - frameCount.current);

      const newMetrics: PerformanceMetrics = {
        fps,
        frameTime,
        memoryUsage,
        animationCount,
        droppedFrames,
        cpuUsage: Math.min(100, (frameTime / 16.67) * 100),
        timestamp: currentTime
      };

      setCurrentMetrics(newMetrics);
      setMetrics(prev => [...prev.slice(-59), newMetrics]); // Keep last 60 seconds
      
      const score = calculatePerformanceScore(newMetrics);
      setPerformanceScore(score);

      // Generate performance warnings
      if (fps < thresholds.minFPS) {
        setEvents(prev => [...prev.slice(-99), {
          id: `warning-${Date.now()}`,
          type: 'performance-warning',
          timestamp: currentTime,
          fps
        }]);
      }

      frameCount.current = 0;
      lastTime.current = currentTime;
    }

    if (isMonitoring) {
      animationId.current = requestAnimationFrame(monitorPerformance);
    }
  }, [isMonitoring, thresholds, calculatePerformanceScore]);

  // Start monitoring
  const startMonitoring = useCallback(() => {
    setIsMonitoring(true);
    setMetrics([]);
    setEvents([]);
    frameCount.current = 0;
    lastTime.current = performance.now();
    
    // Start performance observer for memory
    if ('PerformanceObserver' in window) {
      try {
        memoryObserver.current = new PerformanceObserver((list) => {
          // Handle performance entries
        });
        memoryObserver.current.observe({ entryTypes: ['measure', 'navigation'] });
      } catch (error) {
        console.warn('Performance Observer not supported:', error);
      }
    }
    
    animationId.current = requestAnimationFrame(monitorPerformance);
  }, [monitorPerformance]);

  // Stop monitoring
  const stopMonitoring = useCallback(() => {
    setIsMonitoring(false);
    if (animationId.current) {
      cancelAnimationFrame(animationId.current);
    }
    if (memoryObserver.current) {
      memoryObserver.current.disconnect();
    }
  }, []);

  // Export data
  const exportData = useCallback(() => {
    const data = {
      metrics,
      events,
      thresholds,
      performanceScore,
      timestamp: new Date().toISOString()
    };
    
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `animation-performance-${Date.now()}.json`;
    a.click();
    URL.revokeObjectURL(url);
  }, [metrics, events, thresholds, performanceScore]);

  // Get performance status
  const getPerformanceStatus = () => {
    if (performanceScore >= 80) return { status: 'success', text: 'Excellent' };
    if (performanceScore >= 60) return { status: 'warning', text: 'Good' };
    if (performanceScore >= 40) return { status: 'error', text: 'Poor' };
    return { status: 'error', text: 'Critical' };
  };

  const status = getPerformanceStatus();

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (animationId.current) {
        cancelAnimationFrame(animationId.current);
      }
      if (memoryObserver.current) {
        memoryObserver.current.disconnect();
      }
    };
  }, []);

  return (
    <div className="animation-performance-analytics">
      {/* Control Panel */}
      <Card 
        title="Animation Performance Monitor" 
        extra={
          <Space>
            <Switch
              checked={isMonitoring}
              onChange={isMonitoring ? stopMonitoring : startMonitoring}
              checkedChildren="ON"
              unCheckedChildren="OFF"
            />
            <Button icon={<DownloadOutlined />} onClick={exportData} disabled={metrics.length === 0}>
              Export
            </Button>
            <Button icon={<SettingOutlined />}>
              Settings
            </Button>
          </Space>
        }
      >
        {/* Performance Score */}
        <Row gutter={[24, 16]} style={{ marginBottom: 'var(--space-4)' }}>
          <Col span={6}>
            <Card size="small">
              <Statistic
                title="Performance Score"
                value={performanceScore}
                suffix="/100"
                valueStyle={{ color: status.status === 'success' ? '#3f8600' : status.status === 'warning' ? '#cf1322' : '#cf1322' }}
                prefix={status.status === 'success' ? <CheckCircleOutlined /> : <WarningOutlined />}
              />
              <Text type="secondary">{status.text}</Text>
            </Card>
          </Col>
          <Col span={6}>
            <Card size="small">
              <Statistic
                title="Current FPS"
                value={currentMetrics.fps}
                valueStyle={{ color: currentMetrics.fps >= thresholds.minFPS ? '#3f8600' : '#cf1322' }}
                prefix={<ThunderboltOutlined />}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card size="small">
              <Statistic
                title="Frame Time"
                value={currentMetrics.frameTime.toFixed(2)}
                suffix="ms"
                valueStyle={{ color: currentMetrics.frameTime <= thresholds.maxFrameTime ? '#3f8600' : '#cf1322' }}
                prefix={<DashboardOutlined />}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card size="small">
              <Statistic
                title="Active Animations"
                value={currentMetrics.animationCount}
                valueStyle={{ color: currentMetrics.animationCount <= thresholds.maxAnimationCount ? '#3f8600' : '#cf1322' }}
              />
            </Card>
          </Col>
        </Row>

        {/* Performance Alerts */}
        {performanceScore < 60 && (
          <Alert
            message="Performance Warning"
            description={`Animation performance is below optimal levels. Current score: ${performanceScore}/100`}
            type="warning"
            showIcon
            style={{ marginBottom: 'var(--space-4)' }}
          />
        )}

        {currentMetrics.droppedFrames > 0 && (
          <Alert
            message="Dropped Frames Detected"
            description={`${currentMetrics.droppedFrames} frames were dropped in the last second. Consider reducing animation complexity.`}
            type="error"
            showIcon
            style={{ marginBottom: 'var(--space-4)' }}
          />
        )}
      </Card>

      {/* Performance Charts */}
      <Row gutter={[16, 16]}>
        <Col span={12}>
          <Card title="FPS Over Time" size="small">
            <ResponsiveContainer width="100%" height={200}>
              <LineChart data={metrics}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis 
                  dataKey="timestamp" 
                  tickFormatter={(value) => new Date(value).toLocaleTimeString()}
                />
                <YAxis domain={[0, 60]} />
                <RechartsTooltip 
                  labelFormatter={(value) => new Date(value).toLocaleTimeString()}
                  formatter={(value: number) => [`${value} FPS`, 'FPS']}
                />
                <Line 
                  type="monotone" 
                  dataKey="fps" 
                  stroke="var(--color-primary)" 
                  strokeWidth={2}
                  dot={false}
                />
                <Line 
                  type="monotone" 
                  dataKey={thresholds.minFPS} 
                  stroke="var(--color-error)" 
                  strokeDasharray="5 5"
                  dot={false}
                />
              </LineChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col span={12}>
          <Card title="Memory Usage" size="small">
            <ResponsiveContainer width="100%" height={200}>
              <AreaChart data={metrics}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis 
                  dataKey="timestamp" 
                  tickFormatter={(value) => new Date(value).toLocaleTimeString()}
                />
                <YAxis />
                <RechartsTooltip 
                  labelFormatter={(value) => new Date(value).toLocaleTimeString()}
                  formatter={(value: number) => [`${value.toFixed(2)} MB`, 'Memory']}
                />
                <Area 
                  type="monotone" 
                  dataKey="memoryUsage" 
                  stroke="var(--color-secondary)" 
                  fill="var(--color-secondary)"
                  fillOpacity={0.3}
                />
              </AreaChart>
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>

      {/* Recent Events */}
      <Card title="Recent Events" style={{ marginTop: 'var(--space-4)' }}>
        {events.length === 0 ? (
          <Text type="secondary">No events recorded yet. Start monitoring to see performance events.</Text>
        ) : (
          <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
            {events.slice(-10).reverse().map((event) => (
              <div key={event.id} style={{ 
                padding: 'var(--space-2)', 
                borderBottom: '1px solid var(--border-primary)',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}>
                <Space>
                  {event.type === 'performance-warning' && <WarningOutlined style={{ color: 'var(--color-warning)' }} />}
                  {event.type === 'frame-drop' && <CloseCircleOutlined style={{ color: 'var(--color-error)' }} />}
                  <Text>{event.type.replace('-', ' ').toUpperCase()}</Text>
                  {event.fps && <Tag color="orange">FPS: {event.fps}</Tag>}
                </Space>
                <Text type="secondary" style={{ fontSize: 'var(--text-xs)' }}>
                  {new Date(event.timestamp).toLocaleTimeString()}
                </Text>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
};
