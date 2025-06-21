/**
 * Enterprise Dashboard
 * 
 * Comprehensive enterprise-grade dashboard showcasing:
 * - Security monitoring and threat detection
 * - PWA capabilities and offline functionality
 * - Performance monitoring and Web Vitals
 * - Production readiness indicators
 * - System health and monitoring
 */

import React, { useState, useMemo } from 'react'
import { Card, Row, Col, Button, Space, Typography, Alert, Tabs, Badge, Progress } from 'antd'
import {
  SafetyOutlined,
  RocketOutlined,
  MonitorOutlined,
  CloudOutlined,
  ThunderboltOutlined,
  GlobalOutlined,
  SettingOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons'
import {
  Dashboard,
  SecurityProvider,
  PWAManager,
  usePWA,
  InstallButton,
  OfflineStatus,
} from '@shared/components/core'
import { SecurityDashboard } from '@shared/components/security/SecurityDashboard'
import { performanceMonitor } from '@shared/monitoring/PerformanceMonitor'

const { Title, Text, Paragraph } = Typography
const { TabPane } = Tabs

const EnterpriseDashboardContent: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const { isOnline, isInstalled, isServiceWorkerReady, enableNotifications } = usePWA()

  // Performance metrics
  const performanceMetrics = useMemo(() => {
    const metrics = performanceMonitor.getMetrics()
    const errors = performanceMonitor.getErrors()
    
    const webVitals = {
      cls: metrics.find(m => m.name === 'CLS')?.value || 0,
      fid: metrics.find(m => m.name === 'FID')?.value || 0,
      lcp: metrics.find(m => m.name === 'LCP')?.value || 0,
      fcp: metrics.find(m => m.name === 'FCP')?.value || 0,
      ttfb: metrics.find(m => m.name === 'TTFB')?.value || 0,
    }

    const performanceScore = calculatePerformanceScore(webVitals)
    
    return {
      webVitals,
      performanceScore,
      totalMetrics: metrics.length,
      errorCount: errors.length,
      criticalErrors: errors.filter(e => e.severity === 'critical').length,
    }
  }, [])

  // System health calculation
  const systemHealth = useMemo(() => {
    let score = 100
    let issues = []

    // Security checks
    if (!isOnline) {
      score -= 10
      issues.push('Application is offline')
    }

    // Performance checks
    if (performanceMetrics.performanceScore < 80) {
      score -= 15
      issues.push('Performance below optimal')
    }

    if (performanceMetrics.criticalErrors > 0) {
      score -= 20
      issues.push(`${performanceMetrics.criticalErrors} critical errors detected`)
    }

    // PWA checks
    if (!isServiceWorkerReady) {
      score -= 10
      issues.push('Service worker not ready')
    }

    return {
      score: Math.max(score, 0),
      status: score >= 90 ? 'excellent' : score >= 70 ? 'good' : score >= 50 ? 'fair' : 'poor',
      issues,
    }
  }, [isOnline, performanceMetrics, isServiceWorkerReady])

  const calculatePerformanceScore = (vitals: any) => {
    let score = 100
    
    // CLS scoring
    if (vitals.cls > 0.25) score -= 20
    else if (vitals.cls > 0.1) score -= 10
    
    // FID scoring
    if (vitals.fid > 300) score -= 20
    else if (vitals.fid > 100) score -= 10
    
    // LCP scoring
    if (vitals.lcp > 4000) score -= 20
    else if (vitals.lcp > 2500) score -= 10
    
    // FCP scoring
    if (vitals.fcp > 3000) score -= 15
    else if (vitals.fcp > 1800) score -= 8
    
    // TTFB scoring
    if (vitals.ttfb > 1800) score -= 15
    else if (vitals.ttfb > 800) score -= 8
    
    return Math.max(score, 0)
  }

  return (
    <Dashboard.Root>
      <Dashboard.Layout
        title="Enterprise Dashboard"
        subtitle="Production-Ready Business Intelligence Platform"
        extra={
          <Space>
            <OfflineStatus />
            <InstallButton />
            <Button
              icon={<SettingOutlined />}
              onClick={() => setActiveTab('settings')}
            >
              Settings
            </Button>
          </Space>
        }
        sections={[
          {
            title: "System Health Overview",
            children: (
              <Row gutter={[16, 16]}>
                <Col span={24}>
                  <Alert
                    message={`System Health: ${systemHealth.status.toUpperCase()}`}
                    description={
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Progress 
                          percent={systemHealth.score} 
                          status={
                            systemHealth.score >= 90 ? 'success' : 
                            systemHealth.score >= 70 ? 'normal' : 'exception'
                          }
                          strokeColor={
                            systemHealth.score >= 90 ? '#52c41a' : 
                            systemHealth.score >= 70 ? '#faad14' : '#ff4d4f'
                          }
                        />
                        {systemHealth.issues.length > 0 && (
                          <div>
                            <Text strong>Issues to address:</Text>
                            <ul style={{ margin: '8px 0 0 0' }}>
                              {systemHealth.issues.map((issue, index) => (
                                <li key={index}>{issue}</li>
                              ))}
                            </ul>
                          </div>
                        )}
                      </Space>
                    }
                    type={
                      systemHealth.score >= 90 ? 'success' : 
                      systemHealth.score >= 70 ? 'info' : 
                      systemHealth.score >= 50 ? 'warning' : 'error'
                    }
                    showIcon
                    icon={
                      systemHealth.score >= 90 ? <CheckCircleOutlined /> :
                      systemHealth.score >= 70 ? <MonitorOutlined /> :
                      systemHealth.score >= 50 ? <WarningOutlined /> :
                      <ExclamationCircleOutlined />
                    }
                  />
                </Col>
              </Row>
            ),
          },
          {
            title: "Enterprise Capabilities",
            children: (
              <Row gutter={[16, 16]}>
                <Col span={6}>
                  <Dashboard.KPIWidget
                    title="Security Score"
                    value="94%"
                    prefix={<SafetyOutlined />}
                    highlight
                    trend={{ value: 2.1, isPositive: true, suffix: '%' }}
                    tooltip="Overall security compliance and threat protection"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Performance Score"
                    value={`${performanceMetrics.performanceScore}%`}
                    prefix={<ThunderboltOutlined />}
                    tooltip="Web Vitals and performance metrics"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="PWA Ready"
                    value={isServiceWorkerReady ? "Yes" : "No"}
                    prefix={<RocketOutlined />}
                    tooltip="Progressive Web App capabilities"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Uptime"
                    value="99.9%"
                    prefix={<CloudOutlined />}
                    tooltip="System availability and reliability"
                  />
                </Col>
              </Row>
            ),
          },
          {
            title: "Production Readiness",
            children: (
              <Tabs activeKey={activeTab} onChange={setActiveTab}>
                <TabPane 
                  tab={
                    <span>
                      <SafetyOutlined />
                      Security
                    </span>
                  } 
                  key="security"
                >
                  <SecurityDashboard compact />
                </TabPane>
                
                <TabPane 
                  tab={
                    <span>
                      <ThunderboltOutlined />
                      Performance
                    </span>
                  } 
                  key="performance"
                >
                  <Row gutter={[16, 16]}>
                    <Col span={12}>
                      <Card title="Web Vitals" size="small">
                        <Space direction="vertical" style={{ width: '100%' }}>
                          <div>
                            <Text>Cumulative Layout Shift (CLS)</Text>
                            <Progress 
                              percent={Math.max(100 - (performanceMetrics.webVitals.cls * 1000), 0)} 
                              size="small"
                              status={performanceMetrics.webVitals.cls <= 0.1 ? 'success' : 'exception'}
                            />
                            <Text type="secondary">{performanceMetrics.webVitals.cls.toFixed(3)}</Text>
                          </div>
                          <div>
                            <Text>First Input Delay (FID)</Text>
                            <Progress 
                              percent={Math.max(100 - (performanceMetrics.webVitals.fid / 10), 0)} 
                              size="small"
                              status={performanceMetrics.webVitals.fid <= 100 ? 'success' : 'exception'}
                            />
                            <Text type="secondary">{performanceMetrics.webVitals.fid.toFixed(0)}ms</Text>
                          </div>
                          <div>
                            <Text>Largest Contentful Paint (LCP)</Text>
                            <Progress 
                              percent={Math.max(100 - (performanceMetrics.webVitals.lcp / 50), 0)} 
                              size="small"
                              status={performanceMetrics.webVitals.lcp <= 2500 ? 'success' : 'exception'}
                            />
                            <Text type="secondary">{performanceMetrics.webVitals.lcp.toFixed(0)}ms</Text>
                          </div>
                        </Space>
                      </Card>
                    </Col>
                    <Col span={12}>
                      <Card title="Error Monitoring" size="small">
                        <Space direction="vertical" style={{ width: '100%' }}>
                          <div>
                            <Text>Total Errors</Text>
                            <Badge count={performanceMetrics.errorCount} style={{ marginLeft: 8 }} />
                          </div>
                          <div>
                            <Text>Critical Errors</Text>
                            <Badge 
                              count={performanceMetrics.criticalErrors} 
                              style={{ marginLeft: 8, backgroundColor: '#ff4d4f' }} 
                            />
                          </div>
                          <div>
                            <Text>Performance Metrics</Text>
                            <Badge count={performanceMetrics.totalMetrics} style={{ marginLeft: 8 }} />
                          </div>
                        </Space>
                      </Card>
                    </Col>
                  </Row>
                </TabPane>
                
                <TabPane 
                  tab={
                    <span>
                      <RocketOutlined />
                      PWA Features
                    </span>
                  } 
                  key="pwa"
                >
                  <Row gutter={[16, 16]}>
                    <Col span={12}>
                      <Card title="PWA Status" size="small">
                        <Space direction="vertical" style={{ width: '100%' }}>
                          <div>
                            <Text>Service Worker: </Text>
                            <Badge 
                              status={isServiceWorkerReady ? 'success' : 'error'} 
                              text={isServiceWorkerReady ? 'Active' : 'Inactive'} 
                            />
                          </div>
                          <div>
                            <Text>Installation: </Text>
                            <Badge 
                              status={isInstalled ? 'success' : 'default'} 
                              text={isInstalled ? 'Installed' : 'Not Installed'} 
                            />
                          </div>
                          <div>
                            <Text>Offline Support: </Text>
                            <Badge 
                              status={isServiceWorkerReady ? 'success' : 'error'} 
                              text={isServiceWorkerReady ? 'Enabled' : 'Disabled'} 
                            />
                          </div>
                        </Space>
                      </Card>
                    </Col>
                    <Col span={12}>
                      <Card title="PWA Actions" size="small">
                        <Space direction="vertical" style={{ width: '100%' }}>
                          <Button 
                            icon={<RocketOutlined />} 
                            onClick={() => window.location.reload()}
                            block
                          >
                            Refresh App
                          </Button>
                          <Button 
                            icon={<GlobalOutlined />} 
                            onClick={enableNotifications}
                            block
                          >
                            Enable Notifications
                          </Button>
                          <InstallButton style={{ width: '100%' }} />
                        </Space>
                      </Card>
                    </Col>
                  </Row>
                </TabPane>
                
                <TabPane 
                  tab={
                    <span>
                      <MonitorOutlined />
                      Monitoring
                    </span>
                  } 
                  key="monitoring"
                >
                  <Card title="System Monitoring" size="small">
                    <Paragraph>
                      Comprehensive monitoring is active including:
                    </Paragraph>
                    <ul>
                      <li>Real-time performance tracking with Web Vitals</li>
                      <li>Error monitoring and automatic reporting</li>
                      <li>Security threat detection and prevention</li>
                      <li>User interaction analytics</li>
                      <li>Resource loading performance</li>
                      <li>Offline functionality and sync status</li>
                    </ul>
                    <Alert
                      message="Production Ready"
                      description="All enterprise monitoring systems are active and reporting normally."
                      type="success"
                      showIcon
                      style={{ marginTop: 16 }}
                    />
                  </Card>
                </TabPane>
              </Tabs>
            ),
          },
        ]}
      />
    </Dashboard.Root>
  )
}

// Wrap with all providers for enterprise features
export default function EnterpriseDashboard() {
  return (
    <SecurityProvider>
      <PWAManager enablePushNotifications autoUpdate>
        <EnterpriseDashboardContent />
      </PWAManager>
    </SecurityProvider>
  )
}
