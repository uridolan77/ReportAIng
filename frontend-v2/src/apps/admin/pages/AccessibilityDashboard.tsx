/**
 * Accessibility Dashboard Demonstration
 * 
 * Showcases WCAG 2.1 AA compliant features including:
 * - Accessible charts with keyboard navigation
 * - Accessible data tables with screen reader support
 * - Real-time updates with accessibility announcements
 * - Customizable accessibility settings
 * - Comprehensive keyboard navigation
 */

import React, { useState, useMemo } from 'react'
import { Card, Row, Col, Button, Space, Typography, Alert, Divider } from 'antd'
import {
  AccessibilityOutlined,
  EyeOutlined,
  SoundOutlined,
  ControlOutlined,
  SettingOutlined,
  BarChartOutlined,
  TableOutlined,
} from '@ant-design/icons'
import {
  Dashboard,
  AccessibilityProvider,
  useAccessibility,
  AccessibleChart,
  AccessibleDataTable,
  AccessibilitySettings,
  useEnhancedRealTime,
} from '@shared/components/core'

const { Title, Text, Paragraph } = Typography

// Sample data for demonstrations
const generateAccessibilityData = () => {
  const data = []
  const categories = ['Navigation', 'Forms', 'Charts', 'Tables', 'Media']
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun']
  
  months.forEach((month, monthIndex) => {
    categories.forEach((category, categoryIndex) => {
      data.push({
        id: `${monthIndex}-${categoryIndex}`,
        month,
        category,
        score: Math.floor(Math.random() * 40) + 60, // 60-100 range
        issues: Math.floor(Math.random() * 10),
        improvements: Math.floor(Math.random() * 15) + 5,
        compliance: Math.random() > 0.3 ? 'AA' : 'A',
      })
    })
  })
  
  return data
}

const AccessibilityDashboardContent: React.FC = () => {
  const [showSettings, setShowSettings] = useState(false)
  const { settings, announce } = useAccessibility()
  
  // Sample data
  const accessibilityData = useMemo(() => generateAccessibilityData(), [])
  
  // Chart data for accessibility scores over time
  const chartData = useMemo(() => {
    const monthlyScores = accessibilityData.reduce((acc, item) => {
      if (!acc[item.month]) {
        acc[item.month] = { month: item.month, totalScore: 0, count: 0 }
      }
      acc[item.month].totalScore += item.score
      acc[item.month].count += 1
      return acc
    }, {} as Record<string, any>)
    
    return Object.values(monthlyScores).map((item: any) => ({
      month: item.month,
      averageScore: Math.round(item.totalScore / item.count),
      targetScore: 95, // WCAG AA target
    }))
  }, [accessibilityData])
  
  // Real-time connection for demo
  const realTime = useEnhancedRealTime({
    endpoint: 'ws://localhost:3001/accessibility',
    debug: true,
  })
  
  // Table columns for accessibility data
  const tableColumns = [
    {
      key: 'month',
      title: 'Month',
      dataIndex: 'month',
      sortable: true,
      searchable: true,
      description: 'Month of accessibility assessment',
    },
    {
      key: 'category',
      title: 'Category',
      dataIndex: 'category',
      sortable: true,
      filterable: true,
      searchable: true,
      description: 'Accessibility category being assessed',
    },
    {
      key: 'score',
      title: 'Accessibility Score',
      dataIndex: 'score',
      sortable: true,
      description: 'Accessibility compliance score (0-100)',
      render: (score: number) => (
        <span style={{ 
          color: score >= 90 ? '#52c41a' : score >= 70 ? '#faad14' : '#ff4d4f',
          fontWeight: 'bold',
        }}>
          {score}%
        </span>
      ),
    },
    {
      key: 'issues',
      title: 'Issues Found',
      dataIndex: 'issues',
      sortable: true,
      description: 'Number of accessibility issues identified',
    },
    {
      key: 'improvements',
      title: 'Improvements',
      dataIndex: 'improvements',
      sortable: true,
      description: 'Number of accessibility improvements made',
    },
    {
      key: 'compliance',
      title: 'WCAG Level',
      dataIndex: 'compliance',
      filterable: true,
      description: 'WCAG compliance level achieved',
      render: (level: string) => (
        <span style={{ 
          background: level === 'AA' ? '#52c41a' : '#faad14',
          color: 'white',
          padding: '2px 8px',
          borderRadius: '4px',
          fontWeight: 'bold',
        }}>
          {level}
        </span>
      ),
    },
  ]

  return (
    <Dashboard.Root>
      <Dashboard.Layout
        title="Accessibility Dashboard"
        subtitle="WCAG 2.1 AA Compliance Monitoring & Demonstration"
        extra={
          <Space>
            <Button
              icon={<SettingOutlined />}
              onClick={() => setShowSettings(true)}
              aria-label="Open accessibility settings"
            >
              Accessibility Settings
            </Button>
          </Space>
        }
        sections={[
          {
            title: "Current Accessibility Status",
            children: (
              <Alert
                message="WCAG 2.1 AA Compliance Active"
                description={
                  <Space direction="vertical">
                    <Text>
                      This dashboard demonstrates comprehensive accessibility features including 
                      keyboard navigation, screen reader support, and customizable accessibility settings.
                    </Text>
                    <Space>
                      <Text strong>Active Settings:</Text>
                      {settings.highContrast && <Text type="secondary">High Contrast</Text>}
                      {settings.largeFonts && <Text type="secondary">Large Fonts</Text>}
                      {settings.reduceMotion && <Text type="secondary">Reduced Motion</Text>}
                      {settings.keyboardOnly && <Text type="secondary">Keyboard Only</Text>}
                      {settings.screenReader && <Text type="secondary">Screen Reader Mode</Text>}
                    </Space>
                  </Space>
                }
                type="success"
                showIcon
                icon={<AccessibilityOutlined />}
              />
            ),
          },
          {
            title: "Accessibility Metrics",
            children: (
              <Row gutter={[16, 16]}>
                <Col span={6}>
                  <Dashboard.KPIWidget
                    title="Overall Score"
                    value="94%"
                    prefix={<EyeOutlined />}
                    highlight
                    trend={{ value: 8.2, isPositive: true, suffix: '%' }}
                    tooltip="Average accessibility compliance score across all components"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="WCAG AA Compliance"
                    value="98%"
                    prefix={<AccessibilityOutlined />}
                    tooltip="Percentage of components meeting WCAG 2.1 AA standards"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Keyboard Navigation"
                    value="100%"
                    prefix={<ControlOutlined />}
                    tooltip="All interactive elements are keyboard accessible"
                  />
                </Col>
                <Col span={6}>
                  <Dashboard.MetricWidget
                    title="Screen Reader Support"
                    value="96%"
                    prefix={<SoundOutlined />}
                    tooltip="Components with proper ARIA labels and descriptions"
                  />
                </Col>
              </Row>
            ),
          },
          {
            title: "Accessible Charts",
            children: (
              <Row gutter={[16, 16]}>
                <Col span={12}>
                  <AccessibleChart
                    data={chartData}
                    type="line"
                    title="Accessibility Score Trend"
                    description="Monthly accessibility compliance scores showing improvement over time"
                    xAxisKey="month"
                    yAxisKey="averageScore"
                    series={[
                      { key: 'targetScore', name: 'Target Score', color: '#52c41a' }
                    ]}
                    summary="Line chart showing accessibility scores improving from 85% to 94% over 6 months"
                    trendsDescription="Scores show consistent upward trend with target of 95% nearly achieved"
                    keyboardInstructions="Use arrow keys to navigate data points, T to toggle table view"
                    showTableToggle
                    enableExport
                    ariaLabel="Accessibility score trend chart with monthly data points"
                  />
                </Col>
                <Col span={12}>
                  <AccessibleChart
                    data={accessibilityData.slice(0, 10)}
                    type="bar"
                    title="Issues by Category"
                    description="Number of accessibility issues found in different component categories"
                    xAxisKey="category"
                    yAxisKey="issues"
                    summary="Bar chart showing accessibility issues across 5 categories"
                    showTableToggle
                    enableExport
                  />
                </Col>
              </Row>
            ),
          },
          {
            title: "Accessible Data Table",
            children: (
              <AccessibleDataTable
                data={accessibilityData}
                columns={tableColumns}
                caption="Accessibility assessment data by month and category"
                summary="Table showing accessibility scores, issues, and improvements across different categories and time periods"
                searchable
                exportable
                exportFilename="accessibility-data.csv"
                selectable
                onSelectionChange={(selected) => {
                  announce(`${selected.length} rows selected in accessibility table`)
                }}
                emptyText="No accessibility data available"
              />
            ),
          },
          {
            title: "Real-time Accessibility Monitoring",
            children: (
              <Card>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Space>
                    <Text strong>Connection Status:</Text>
                    <Text type={realTime.connectionStatus.status === 'connected' ? 'success' : 'secondary'}>
                      {realTime.connectionStatus.status.toUpperCase()}
                    </Text>
                    {realTime.connectionStatus.latency && (
                      <Text type="secondary">
                        (Latency: {realTime.connectionStatus.latency}ms)
                      </Text>
                    )}
                  </Space>
                  
                  <Space>
                    <Button
                      onClick={() => realTime.connect()}
                      disabled={realTime.connectionStatus.status === 'connected'}
                    >
                      Connect
                    </Button>
                    <Button
                      onClick={() => realTime.disconnect()}
                      disabled={realTime.connectionStatus.status === 'disconnected'}
                    >
                      Disconnect
                    </Button>
                    <Button
                      onClick={() => realTime.subscribe('accessibility-updates')}
                      disabled={!realTime.connectionStatus.status || realTime.isSubscribed}
                    >
                      Subscribe to Updates
                    </Button>
                  </Space>
                  
                  {realTime.updates.length > 0 && (
                    <div>
                      <Text strong>Recent Updates:</Text>
                      <div style={{ maxHeight: '200px', overflow: 'auto', marginTop: '8px' }}>
                        {realTime.updates.slice(-5).map((update, index) => (
                          <div key={update.id} style={{ padding: '4px 0', borderBottom: '1px solid #f0f0f0' }}>
                            <Text code>{update.type}</Text>
                            <Text type="secondary" style={{ marginLeft: '8px' }}>
                              {update.timestamp.toLocaleTimeString()}
                            </Text>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </Space>
              </Card>
            ),
          },
          {
            title: "Accessibility Features Guide",
            children: (
              <Card>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Title level={5}>
                    <ControlOutlined /> Keyboard Navigation
                  </Title>
                  <Paragraph>
                    All components support full keyboard navigation:
                  </Paragraph>
                  <ul>
                    <li><kbd>Tab</kbd> / <kbd>Shift+Tab</kbd> - Navigate between elements</li>
                    <li><kbd>Enter</kbd> / <kbd>Space</kbd> - Activate buttons and controls</li>
                    <li><kbd>Arrow Keys</kbd> - Navigate within charts and tables</li>
                    <li><kbd>T</kbd> - Toggle chart/table view in accessible charts</li>
                    <li><kbd>Esc</kbd> - Close modals and dropdowns</li>
                  </ul>
                  
                  <Divider />
                  
                  <Title level={5}>
                    <SoundOutlined /> Screen Reader Support
                  </Title>
                  <Paragraph>
                    Enhanced screen reader compatibility with:
                  </Paragraph>
                  <ul>
                    <li>Comprehensive ARIA labels and descriptions</li>
                    <li>Live regions for dynamic content updates</li>
                    <li>Proper heading hierarchy and landmarks</li>
                    <li>Alternative text for charts and visualizations</li>
                    <li>Data table alternatives for complex charts</li>
                  </ul>
                  
                  <Divider />
                  
                  <Title level={5}>
                    <EyeOutlined /> Visual Accessibility
                  </Title>
                  <Paragraph>
                    Customizable visual features:
                  </Paragraph>
                  <ul>
                    <li>High contrast mode for better visibility</li>
                    <li>Large font options for improved readability</li>
                    <li>Reduced motion for vestibular sensitivity</li>
                    <li>Enhanced focus indicators</li>
                    <li>Color-blind friendly chart palettes</li>
                  </ul>
                </Space>
              </Card>
            ),
          },
        ]}
      />
      
      {/* Accessibility Settings Modal */}
      <AccessibilitySettings
        modal
        visible={showSettings}
        onClose={() => setShowSettings(false)}
      />
    </Dashboard.Root>
  )
}

// Wrap with AccessibilityProvider for demo
export default function AccessibilityDashboard() {
  return (
    <AccessibilityProvider>
      <AccessibilityDashboardContent />
    </AccessibilityProvider>
  )
}
