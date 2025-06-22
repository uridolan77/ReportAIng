# Performance Analytics Dashboard - Frontend Integration Guide

## 📋 Overview

This guide provides comprehensive instructions for integrating the new Performance Analytics Dashboard endpoints and real-time features into the frontend application.

## 🔗 New API Endpoints

### 1. Comprehensive Analytics Dashboard
```typescript
GET /api/templateanalytics/dashboard/comprehensive
```

**Query Parameters:**
- `startDate?: string` (ISO date format)
- `endDate?: string` (ISO date format)  
- `intentType?: string` (filter by specific intent type)

**Response Type:** `ComprehensiveAnalyticsDashboard`

**Usage Example:**
```typescript
const { data: dashboard } = useGetComprehensiveDashboardQuery({
  startDate: '2024-01-01T00:00:00Z',
  endDate: '2024-01-31T23:59:59Z',
  intentType: 'query_generation'
});
```

### 2. Performance Trends
```typescript
GET /api/templateanalytics/trends/performance
```

**Query Parameters:**
- `startDate?: string`
- `endDate?: string`
- `intentType?: string`
- `granularity?: 'hourly' | 'daily' | 'weekly' | 'monthly'` (default: 'daily')

**Response Type:** `PerformanceTrendsData`

### 3. Usage Insights
```typescript
GET /api/templateanalytics/insights/usage
```

**Query Parameters:**
- `startDate?: string`
- `endDate?: string`
- `intentType?: string`

**Response Type:** `UsageInsightsData`

### 4. Quality Metrics
```typescript
GET /api/templateanalytics/metrics/quality
```

**Query Parameters:**
- `intentType?: string`

**Response Type:** `QualityMetricsData`

### 5. Real-Time Analytics
```typescript
GET /api/templateanalytics/realtime
```

**Response Type:** `RealTimeAnalyticsData`

### 6. Advanced Export
```typescript
POST /api/templateanalytics/export
```

**Request Body:** `AnalyticsExportConfig`
**Response:** File download (CSV, Excel, JSON)

## 📊 TypeScript Interfaces

### Core Analytics Models

```typescript
interface ComprehensiveAnalyticsDashboard {
  performanceOverview: PerformanceDashboardData;
  abTestingOverview: ABTestDashboard;
  managementOverview: TemplateManagementDashboard;
  performanceTrends: PerformanceTrendsData;
  usageInsights: UsageInsightsData;
  qualityMetrics: QualityMetricsData;
  activeAlerts: PerformanceAlert[];
  dateRange: DateRange;
  generatedDate: string;
}

interface PerformanceTrendsData {
  dataPoints: PerformanceTrendDataPoint[];
  timeRange: DateRange;
  granularity: string;
  intentType?: string;
  generatedDate: string;
}

interface PerformanceTrendDataPoint {
  timestamp: string;
  averageSuccessRate: number;
  averageConfidenceScore: number;
  totalUsage: number;
  activeTemplates: number;
  averageResponseTime: number;
  errorCount: number;
}

interface UsageInsightsData {
  totalUsage: number;
  averageSuccessRate: number;
  usageByIntentType: Record<string, number>;
  topPerformingTemplates: TemplatePerformanceMetrics[];
  underperformingTemplates: TemplatePerformanceMetrics[];
  insights: UsageInsight[];
  timeRange: DateRange;
  generatedDate: string;
}

interface UsageInsight {
  type: string;
  title: string;
  description: string;
  impact: 'High' | 'Medium' | 'Low';
  recommendation: string;
  data: Record<string, any>;
  generatedDate: string;
}

interface QualityMetricsData {
  overallQualityScore: number;
  averageConfidenceScore: number;
  qualityDistribution: Record<string, number>;
  totalTemplatesAnalyzed: number;
  templatesAboveThreshold: number;
  templatesBelowThreshold: number;
  detailedMetrics: QualityMetric[];
  intentType?: string;
  generatedDate: string;
}

interface RealTimeAnalyticsData {
  activeUsers: number;
  queriesPerMinute: number;
  currentSuccessRate: number;
  averageResponseTime: number;
  errorsInLastHour: number;
  recentActivities: RecentActivity[];
  activeTemplateUsage: Record<string, number>;
  lastUpdated: string;
}

interface PerformanceAlert {
  alertId: string;
  alertType: string;
  title: string;
  description: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  templateKey: string;
  intentType: string;
  alertData: Record<string, any>;
  createdDate: string;
  resolvedDate?: string;
  isResolved: boolean;
  resolvedBy?: string;
  resolutionNotes?: string;
}

interface AnalyticsExportConfig {
  format: 'CSV' | 'Excel' | 'JSON';
  dateRange: DateRange;
  includedMetrics: string[];
  intentTypeFilter?: string;
  includeCharts: boolean;
  includeRawData: boolean;
  exportedBy: string;
  requestedDate: string;
}

interface DateRange {
  startDate: string;
  endDate: string;
}
```

## 🔄 Real-Time Integration with SignalR

### Connection Setup

```typescript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl('/hubs/template-analytics', {
    accessTokenFactory: () => getAuthToken()
  })
  .withAutomaticReconnect()
  .configureLogging(LogLevel.Information)
  .build();

// Start connection
await connection.start();
```

### Event Subscriptions

```typescript
// Subscribe to dashboard updates
connection.on('DashboardUpdate', (dashboardData) => {
  // Update dashboard state
  setDashboardData(dashboardData);
});

// Subscribe to performance updates
connection.on('PerformanceUpdate', (templateKey, performanceData) => {
  // Update specific template performance
  updateTemplatePerformance(templateKey, performanceData);
});

// Subscribe to A/B test updates
connection.on('ABTestUpdate', (testId, testData) => {
  // Update A/B test status
  updateABTestStatus(testId, testData);
});

// Subscribe to alerts
connection.on('NewAlert', (alert) => {
  // Show new alert notification
  showAlertNotification(alert);
});

// Handle errors
connection.on('Error', (errorMessage) => {
  console.error('Analytics Hub Error:', errorMessage);
});
```

### Hub Methods

```typescript
// Subscribe to performance updates for specific intent type
await connection.invoke('SubscribeToPerformanceUpdates', 'query_generation');

// Subscribe to A/B test updates
await connection.invoke('SubscribeToABTestUpdates');

// Subscribe to alerts
await connection.invoke('SubscribeToAlerts');

// Get real-time dashboard data
await connection.invoke('GetRealTimeDashboard');

// Get specific template performance
await connection.invoke('GetTemplatePerformance', 'template_key');
```

## 📈 RTK Query Integration

### API Slice Extension

```typescript
export const templateAnalyticsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Comprehensive Dashboard
    getComprehensiveDashboard: builder.query<ComprehensiveAnalyticsDashboard, {
      startDate?: string;
      endDate?: string;
      intentType?: string;
    }>({
      query: ({ startDate, endDate, intentType }) => ({
        url: 'templateanalytics/dashboard/comprehensive',
        params: { startDate, endDate, intentType },
      }),
      providesTags: ['Analytics'],
    }),

    // Performance Trends
    getPerformanceTrends: builder.query<PerformanceTrendsData, {
      startDate?: string;
      endDate?: string;
      intentType?: string;
      granularity?: string;
    }>({
      query: ({ startDate, endDate, intentType, granularity }) => ({
        url: 'templateanalytics/trends/performance',
        params: { startDate, endDate, intentType, granularity },
      }),
      providesTags: ['Analytics'],
    }),

    // Usage Insights
    getUsageInsights: builder.query<UsageInsightsData, {
      startDate?: string;
      endDate?: string;
      intentType?: string;
    }>({
      query: ({ startDate, endDate, intentType }) => ({
        url: 'templateanalytics/insights/usage',
        params: { startDate, endDate, intentType },
      }),
      providesTags: ['Analytics'],
    }),

    // Quality Metrics
    getQualityMetrics: builder.query<QualityMetricsData, {
      intentType?: string;
    }>({
      query: ({ intentType }) => ({
        url: 'templateanalytics/metrics/quality',
        params: { intentType },
      }),
      providesTags: ['Analytics'],
    }),

    // Real-Time Analytics
    getRealTimeAnalytics: builder.query<RealTimeAnalyticsData, void>({
      query: () => 'templateanalytics/realtime',
      providesTags: ['Analytics'],
    }),

    // Export Analytics
    exportAnalytics: builder.mutation<Blob, AnalyticsExportConfig>({
      query: (config) => ({
        url: 'templateanalytics/export',
        method: 'POST',
        body: config,
        responseHandler: (response) => response.blob(),
      }),
    }),
  }),
});

export const {
  useGetComprehensiveDashboardQuery,
  useGetPerformanceTrendsQuery,
  useGetUsageInsightsQuery,
  useGetQualityMetricsQuery,
  useGetRealTimeAnalyticsQuery,
  useExportAnalyticsMutation,
} = templateAnalyticsApi;
```

## 🎨 React Component Examples

### Comprehensive Dashboard Component

```typescript
import React, { useEffect, useState } from 'react';
import { Card, Row, Col, DatePicker, Select, Spin } from 'antd';
import { useGetComprehensiveDashboardQuery } from '../api/templateAnalyticsApi';
import { PerformanceTrendsChart } from './PerformanceTrendsChart';
import { QualityMetricsChart } from './QualityMetricsChart';
import { AlertsList } from './AlertsList';

const { RangePicker } = DatePicker;
const { Option } = Select;

export const ComprehensiveAnalyticsDashboard: React.FC = () => {
  const [dateRange, setDateRange] = useState<[string, string]>([
    new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
    new Date().toISOString()
  ]);
  const [intentType, setIntentType] = useState<string | undefined>();

  const { data: dashboard, isLoading, error } = useGetComprehensiveDashboardQuery({
    startDate: dateRange[0],
    endDate: dateRange[1],
    intentType
  });

  if (isLoading) return <Spin size="large" />;
  if (error) return <div>Error loading dashboard</div>;

  return (
    <div className="analytics-dashboard">
      {/* Filters */}
      <Row gutter={16} className="mb-4">
        <Col span={12}>
          <RangePicker
            value={dateRange}
            onChange={(dates) => setDateRange(dates?.map(d => d.toISOString()) as [string, string])}
          />
        </Col>
        <Col span={12}>
          <Select
            placeholder="Select Intent Type"
            value={intentType}
            onChange={setIntentType}
            allowClear
          >
            <Option value="query_generation">Query Generation</Option>
            <Option value="data_analysis">Data Analysis</Option>
            <Option value="visualization">Visualization</Option>
          </Select>
        </Col>
      </Row>

      {/* Overview Cards */}
      <Row gutter={16} className="mb-4">
        <Col span={6}>
          <Card title="Total Templates">
            <div className="metric-value">{dashboard?.performanceOverview.totalTemplates}</div>
          </Card>
        </Col>
        <Col span={6}>
          <Card title="Success Rate">
            <div className="metric-value">
              {(dashboard?.performanceOverview.overallSuccessRate * 100).toFixed(1)}%
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card title="Active Tests">
            <div className="metric-value">{dashboard?.abTestingOverview.totalActiveTests}</div>
          </Card>
        </Col>
        <Col span={6}>
          <Card title="Active Alerts">
            <div className="metric-value">{dashboard?.activeAlerts.length}</div>
          </Card>
        </Col>
      </Row>

      {/* Charts */}
      <Row gutter={16} className="mb-4">
        <Col span={16}>
          <Card title="Performance Trends">
            <PerformanceTrendsChart data={dashboard?.performanceTrends} />
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Quality Distribution">
            <QualityMetricsChart data={dashboard?.qualityMetrics} />
          </Card>
        </Col>
      </Row>

      {/* Alerts and Insights */}
      <Row gutter={16}>
        <Col span={12}>
          <Card title="Active Alerts">
            <AlertsList alerts={dashboard?.activeAlerts} />
          </Card>
        </Col>
        <Col span={12}>
          <Card title="Usage Insights">
            {dashboard?.usageInsights.insights.map((insight, index) => (
              <div key={index} className="insight-item">
                <h4>{insight.title}</h4>
                <p>{insight.description}</p>
                <div className={`impact-badge impact-${insight.impact.toLowerCase()}`}>
                  {insight.impact} Impact
                </div>
              </div>
            ))}
          </Card>
        </Col>
      </Row>
    </div>
  );
};
```

### Real-Time Analytics Hook

```typescript
import { useEffect, useState } from 'react';
import { HubConnection } from '@microsoft/signalr';
import { useGetRealTimeAnalyticsQuery } from '../api/templateAnalyticsApi';

export const useRealTimeAnalytics = (connection: HubConnection | null) => {
  const [realTimeData, setRealTimeData] = useState<RealTimeAnalyticsData | null>(null);
  const { data: initialData } = useGetRealTimeAnalyticsQuery();

  useEffect(() => {
    if (initialData) {
      setRealTimeData(initialData);
    }
  }, [initialData]);

  useEffect(() => {
    if (!connection) return;

    // Subscribe to real-time updates
    connection.on('DashboardUpdate', (data) => {
      setRealTimeData(prev => ({ ...prev, ...data }));
    });

    // Request initial real-time data
    connection.invoke('GetRealTimeDashboard');

    return () => {
      connection.off('DashboardUpdate');
    };
  }, [connection]);

  return realTimeData;
};
```

## 📊 Chart Components with Recharts

### Performance Trends Chart

```typescript
import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { PerformanceTrendsData } from '../types/analytics';

interface Props {
  data?: PerformanceTrendsData;
}

export const PerformanceTrendsChart: React.FC<Props> = ({ data }) => {
  if (!data?.dataPoints.length) return <div>No data available</div>;

  const chartData = data.dataPoints.map(point => ({
    timestamp: new Date(point.timestamp).toLocaleDateString(),
    successRate: (point.averageSuccessRate * 100).toFixed(1),
    confidenceScore: (point.averageConfidenceScore * 100).toFixed(1),
    usage: point.totalUsage,
    responseTime: point.averageResponseTime
  }));

  return (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="timestamp" />
        <YAxis yAxisId="percentage" domain={[0, 100]} />
        <YAxis yAxisId="usage" orientation="right" />
        <Tooltip />
        <Legend />
        <Line
          yAxisId="percentage"
          type="monotone"
          dataKey="successRate"
          stroke="#8884d8"
          name="Success Rate (%)"
        />
        <Line
          yAxisId="percentage"
          type="monotone"
          dataKey="confidenceScore"
          stroke="#82ca9d"
          name="Confidence Score (%)"
        />
        <Line
          yAxisId="usage"
          type="monotone"
          dataKey="usage"
          stroke="#ffc658"
          name="Usage Count"
        />
      </LineChart>
    </ResponsiveContainer>
  );
};
```

### Quality Metrics Pie Chart

```typescript
import React from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';
import { QualityMetricsData } from '../types/analytics';

interface Props {
  data?: QualityMetricsData;
}

const COLORS = {
  Excellent: '#52c41a',
  Good: '#1890ff',
  Fair: '#faad14',
  Poor: '#ff4d4f'
};

export const QualityMetricsChart: React.FC<Props> = ({ data }) => {
  if (!data?.qualityDistribution) return <div>No data available</div>;

  const chartData = Object.entries(data.qualityDistribution).map(([key, value]) => ({
    name: key,
    value,
    color: COLORS[key as keyof typeof COLORS]
  }));

  return (
    <ResponsiveContainer width="100%" height={300}>
      <PieChart>
        <Pie
          data={chartData}
          cx="50%"
          cy="50%"
          labelLine={false}
          label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
          outerRadius={80}
          fill="#8884d8"
          dataKey="value"
        >
          {chartData.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={entry.color} />
          ))}
        </Pie>
        <Tooltip />
        <Legend />
      </PieChart>
    </ResponsiveContainer>
  );
};
```

## 🚨 Alert Management Component

```typescript
import React from 'react';
import { List, Tag, Button, Modal } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { PerformanceAlert } from '../types/analytics';

interface Props {
  alerts?: PerformanceAlert[];
  onResolveAlert?: (alertId: string) => void;
}

const getSeverityColor = (severity: string) => {
  switch (severity) {
    case 'Critical': return 'red';
    case 'High': return 'orange';
    case 'Medium': return 'yellow';
    case 'Low': return 'blue';
    default: return 'default';
  }
};

export const AlertsList: React.FC<Props> = ({ alerts = [], onResolveAlert }) => {
  const handleResolveAlert = (alert: PerformanceAlert) => {
    Modal.confirm({
      title: 'Resolve Alert',
      icon: <ExclamationCircleOutlined />,
      content: `Are you sure you want to resolve "${alert.title}"?`,
      onOk: () => onResolveAlert?.(alert.alertId),
    });
  };

  return (
    <List
      dataSource={alerts.filter(alert => !alert.isResolved)}
      renderItem={(alert) => (
        <List.Item
          actions={[
            <Button
              size="small"
              onClick={() => handleResolveAlert(alert)}
            >
              Resolve
            </Button>
          ]}
        >
          <List.Item.Meta
            title={
              <div>
                {alert.title}
                <Tag color={getSeverityColor(alert.severity)} className="ml-2">
                  {alert.severity}
                </Tag>
              </div>
            }
            description={
              <div>
                <p>{alert.description}</p>
                <small>Template: {alert.templateKey} | {new Date(alert.createdDate).toLocaleString()}</small>
              </div>
            }
          />
        </List.Item>
      )}
    />
  );
};
```

## 📤 Export Functionality

```typescript
import React, { useState } from 'react';
import { Button, Modal, Form, Select, DatePicker, Checkbox, message } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import { useExportAnalyticsMutation } from '../api/templateAnalyticsApi';

const { RangePicker } = DatePicker;
const { Option } = Select;

export const ExportAnalyticsButton: React.FC = () => {
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [form] = Form.useForm();
  const [exportAnalytics, { isLoading }] = useExportAnalyticsMutation();

  const handleExport = async (values: any) => {
    try {
      const config = {
        format: values.format,
        dateRange: {
          startDate: values.dateRange[0].toISOString(),
          endDate: values.dateRange[1].toISOString(),
        },
        includedMetrics: values.metrics || [],
        intentTypeFilter: values.intentType,
        includeCharts: values.includeCharts || false,
        includeRawData: values.includeRawData || true,
        exportedBy: 'current_user', // Get from auth context
        requestedDate: new Date().toISOString(),
      };

      const blob = await exportAnalytics(config).unwrap();

      // Create download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `analytics_export_${new Date().toISOString().split('T')[0]}.${values.format.toLowerCase()}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      message.success('Export completed successfully');
      setIsModalVisible(false);
      form.resetFields();
    } catch (error) {
      message.error('Export failed');
    }
  };

  return (
    <>
      <Button
        type="primary"
        icon={<DownloadOutlined />}
        onClick={() => setIsModalVisible(true)}
      >
        Export Analytics
      </Button>

      <Modal
        title="Export Analytics Data"
        visible={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        footer={null}
      >
        <Form form={form} onFinish={handleExport} layout="vertical">
          <Form.Item name="format" label="Export Format" rules={[{ required: true }]}>
            <Select placeholder="Select format">
              <Option value="CSV">CSV</Option>
              <Option value="Excel">Excel</Option>
              <Option value="JSON">JSON</Option>
            </Select>
          </Form.Item>

          <Form.Item name="dateRange" label="Date Range" rules={[{ required: true }]}>
            <RangePicker />
          </Form.Item>

          <Form.Item name="intentType" label="Intent Type">
            <Select placeholder="All intent types" allowClear>
              <Option value="query_generation">Query Generation</Option>
              <Option value="data_analysis">Data Analysis</Option>
              <Option value="visualization">Visualization</Option>
            </Select>
          </Form.Item>

          <Form.Item name="metrics" label="Include Metrics">
            <Checkbox.Group>
              <Checkbox value="performance">Performance Metrics</Checkbox>
              <Checkbox value="usage">Usage Statistics</Checkbox>
              <Checkbox value="quality">Quality Metrics</Checkbox>
              <Checkbox value="abtests">A/B Test Results</Checkbox>
            </Checkbox.Group>
          </Form.Item>

          <Form.Item>
            <Form.Item name="includeCharts" valuePropName="checked" noStyle>
              <Checkbox>Include Charts</Checkbox>
            </Form.Item>
          </Form.Item>

          <Form.Item>
            <Form.Item name="includeRawData" valuePropName="checked" noStyle>
              <Checkbox defaultChecked>Include Raw Data</Checkbox>
            </Form.Item>
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" loading={isLoading} block>
              Export Data
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};
```

## 🎯 Best Practices

### 1. Performance Optimization
- Use React.memo for chart components
- Implement virtual scrolling for large data sets
- Cache API responses with appropriate TTL
- Use skeleton loading states

### 2. Real-Time Updates
- Implement connection retry logic for SignalR
- Handle connection state in UI
- Debounce rapid updates to prevent UI flickering
- Provide manual refresh option as fallback

### 3. Error Handling
- Implement comprehensive error boundaries
- Show user-friendly error messages
- Provide retry mechanisms for failed requests
- Log errors for debugging

### 4. Accessibility
- Use semantic HTML elements
- Provide alt text for charts
- Implement keyboard navigation
- Ensure color contrast compliance

### 5. Mobile Responsiveness
- Use responsive chart containers
- Implement touch-friendly interactions
- Optimize layout for smaller screens
- Consider progressive disclosure for complex data

## 🔧 Configuration

### Environment Variables
```env
REACT_APP_API_BASE_URL=http://localhost:55244/api
REACT_APP_SIGNALR_HUB_URL=http://localhost:55244/hubs
REACT_APP_ANALYTICS_REFRESH_INTERVAL=300000
```

### SignalR Configuration
```typescript
const signalRConfig = {
  automaticReconnect: {
    nextRetryDelayInMilliseconds: (retryContext) => {
      return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
    }
  },
  serverTimeoutInMilliseconds: 60000,
  keepAliveIntervalInMilliseconds: 15000,
};
```

This guide provides comprehensive integration instructions for the new Performance Analytics Dashboard. The frontend team can use these examples as starting points and adapt them to match the existing application architecture and design system.
```
