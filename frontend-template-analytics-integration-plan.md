# 🎯 Frontend Template Analytics Integration Plan

## 📋 **Executive Summary**

This document outlines the comprehensive frontend integration plan for the newly implemented Template Management and Analytics endpoints. The plan covers UI components, data flow, user experience, and implementation timeline for integrating advanced template analytics, A/B testing, and performance monitoring into the existing BI Reporting Copilot frontend.

## 🏗️ **Architecture Overview**

### **New Frontend Module Structure**
```
frontend-v2/src/
├── features/
│   └── template-analytics/
│       ├── components/
│       │   ├── dashboard/
│       │   ├── performance/
│       │   ├── abtesting/
│       │   ├── management/
│       │   └── shared/
│       ├── hooks/
│       ├── services/
│       ├── types/
│       └── utils/
├── shared/
│   ├── components/
│   │   ├── charts/
│   │   ├── metrics/
│   │   └── analytics/
│   └── hooks/
└── pages/
    └── admin/
        ├── template-analytics/
        ├── template-management/
        └── ab-testing/
```

## 🎨 **UI Components Architecture**

### **1. Template Performance Dashboard**

**Main Dashboard Component (`TemplatePerformanceDashboard.tsx`)**
```typescript
interface PerformanceDashboardProps {
  timeRange?: TimeRange;
  refreshInterval?: number;
  filters?: DashboardFilters;
}

interface DashboardFilters {
  intentTypes?: string[];
  templateKeys?: string[];
  performanceThreshold?: number;
}
```

**Key Features:**
- Real-time performance metrics display
- Interactive charts with drill-down capabilities
- Performance alerts and notifications
- Template health status indicators
- Export functionality for reports

**Sub-Components:**
- `PerformanceMetricsGrid` - Key metrics overview
- `TemplateHealthChart` - Visual health status
- `TopPerformersTable` - Best performing templates
- `AlertsPanel` - Performance alerts
- `TrendsChart` - Performance trends over time

### **2. A/B Testing Management**

**A/B Testing Dashboard (`ABTestingDashboard.tsx`)**
```typescript
interface ABTestingDashboardProps {
  activeTests?: ABTestDetails[];
  completedTests?: ABTestDetails[];
  onCreateTest?: (request: ABTestRequest) => void;
  onCompleteTest?: (testId: number) => void;
}
```

**Key Features:**
- Active test monitoring with real-time updates
- Test creation wizard with template selection
- Statistical significance indicators
- Winner implementation controls
- Test history and analytics

**Sub-Components:**
- `ActiveTestsGrid` - Current running tests
- `TestCreationWizard` - Multi-step test creation
- `TestAnalysisPanel` - Statistical analysis
- `TestHistoryTable` - Historical test data
- `RecommendationsPanel` - AI-powered test suggestions

### **3. Template Management Interface**

**Template Management Hub (`TemplateManagementHub.tsx`)**
```typescript
interface TemplateManagementProps {
  templates?: TemplateWithMetrics[];
  searchCriteria?: TemplateSearchCriteria;
  onTemplateUpdate?: (templateKey: string, updates: UpdateTemplateRequest) => void;
  onTemplateCreate?: (request: CreateTemplateRequest) => void;
}
```

**Key Features:**
- Template CRUD operations with version control
- Business metadata management
- Quality scoring and validation
- Template recommendations
- Import/export capabilities

**Sub-Components:**
- `TemplateGrid` - Template overview with metrics
- `TemplateEditor` - Monaco-based template editor
- `MetadataPanel` - Business context management
- `QualityIndicators` - Template quality metrics
- `VersionHistory` - Template version management

## 📊 **Data Flow & State Management**

### **Redux Store Structure**
```typescript
interface TemplateAnalyticsState {
  performance: {
    dashboard: PerformanceDashboardData | null;
    metrics: Record<string, TemplatePerformanceMetrics>;
    alerts: PerformanceAlert[];
    trends: Record<string, TemplatePerformanceTrends>;
    loading: boolean;
    error: string | null;
  };
  abTesting: {
    activeTests: ABTestDetails[];
    completedTests: ABTestDetails[];
    recommendations: ABTestRecommendation[];
    analysis: Record<number, ABTestAnalysis>;
    loading: boolean;
    error: string | null;
  };
  management: {
    templates: TemplateWithMetrics[];
    searchResults: TemplateSearchResult | null;
    selectedTemplate: TemplateWithMetrics | null;
    dashboard: TemplateManagementDashboard | null;
    loading: boolean;
    error: string | null;
  };
  ui: {
    activeTab: string;
    filters: DashboardFilters;
    timeRange: TimeRange;
    refreshInterval: number;
  };
}
```

### **RTK Query API Slices**
```typescript
// Template Performance API
export const templatePerformanceApi = createApi({
  reducerPath: 'templatePerformanceApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/templateanalytics/',
    prepareHeaders: (headers, { getState }) => {
      headers.set('authorization', `Bearer ${getToken(getState())}`);
      return headers;
    },
  }),
  tagTypes: ['Performance', 'Alerts', 'Trends'],
  endpoints: (builder) => ({
    getDashboard: builder.query<PerformanceDashboardData, void>({
      query: () => 'dashboard',
      providesTags: ['Performance'],
    }),
    getTemplatePerformance: builder.query<TemplatePerformanceMetrics, string>({
      query: (templateKey) => `performance/${templateKey}`,
      providesTags: (result, error, templateKey) => [
        { type: 'Performance', id: templateKey },
      ],
    }),
    getPerformanceAlerts: builder.query<PerformanceAlert[], void>({
      query: () => 'alerts',
      providesTags: ['Alerts'],
    }),
    // ... more endpoints
  }),
});

// A/B Testing API
export const abTestingApi = createApi({
  reducerPath: 'abTestingApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/templateanalytics/abtests/',
  }),
  tagTypes: ['ABTest', 'Analysis', 'Recommendations'],
  endpoints: (builder) => ({
    getActiveTests: builder.query<ABTestDetails[], void>({
      query: () => 'active',
      providesTags: ['ABTest'],
    }),
    createABTest: builder.mutation<ABTestResult, ABTestRequest>({
      query: (request) => ({
        url: '',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['ABTest'],
    }),
    // ... more endpoints
  }),
});
```

## 🎯 **User Experience Design**

### **Navigation Integration**
- Add "Template Analytics" section to admin navigation
- Implement breadcrumb navigation for deep-linking
- Add quick access shortcuts in main dashboard
- Integrate with existing user permissions system

### **Real-time Updates**
- WebSocket integration for live performance metrics
- Auto-refresh capabilities with configurable intervals
- Push notifications for critical alerts
- Optimistic updates for user interactions

### **Responsive Design**
- Mobile-first approach for dashboard components
- Adaptive layouts for different screen sizes
- Touch-friendly controls for mobile devices
- Progressive enhancement for advanced features

## 🔧 **Implementation Phases**

### **Phase 1: Foundation (Week 1-2)**
**Deliverables:**
- [ ] Set up template analytics module structure
- [ ] Create base Redux slices and RTK Query APIs
- [ ] Implement core TypeScript interfaces
- [ ] Create shared chart components
- [ ] Set up routing for new pages

**Components to Build:**
- `TemplateAnalyticsLayout` - Main layout wrapper
- `MetricsCard` - Reusable metrics display
- `PerformanceChart` - Base chart component
- `LoadingStates` - Loading and error states
- `FilterPanel` - Common filtering interface

### **Phase 2: Performance Dashboard (Week 3-4)**
**Deliverables:**
- [ ] Complete performance dashboard implementation
- [ ] Real-time metrics display
- [ ] Interactive charts and visualizations
- [ ] Performance alerts system
- [ ] Export functionality

**Components to Build:**
- `PerformanceDashboard` - Main dashboard
- `PerformanceMetricsGrid` - Metrics overview
- `TemplateHealthIndicators` - Health status
- `PerformanceTrendsChart` - Trend visualization
- `AlertsNotificationPanel` - Alert management

### **Phase 3: A/B Testing Interface (Week 5-6)**
**Deliverables:**
- [ ] A/B testing dashboard
- [ ] Test creation wizard
- [ ] Statistical analysis display
- [ ] Test management controls
- [ ] Recommendations system

**Components to Build:**
- `ABTestingDashboard` - Main A/B testing interface
- `TestCreationWizard` - Multi-step test creation
- `StatisticalAnalysisPanel` - Analysis display
- `TestManagementGrid` - Test overview
- `TestRecommendations` - AI recommendations

### **Phase 4: Template Management (Week 7-8)**
**Deliverables:**
- [ ] Template management interface
- [ ] Template editor with Monaco integration
- [ ] Business metadata management
- [ ] Version control system
- [ ] Quality assessment tools

**Components to Build:**
- `TemplateManagementHub` - Main management interface
- `TemplateEditor` - Monaco-based editor
- `BusinessMetadataPanel` - Metadata management
- `TemplateVersionHistory` - Version control
- `QualityAssessment` - Quality indicators

### **Phase 5: Advanced Features (Week 9-10)**
**Deliverables:**
- [ ] Advanced analytics and insights
- [ ] Predictive analytics display
- [ ] Automated recommendations
- [ ] Integration with existing chat interface
- [ ] Performance optimization

**Components to Build:**
- `AdvancedAnalytics` - Deep analytics
- `PredictiveInsights` - ML-powered insights
- `AutomatedRecommendations` - Smart suggestions
- `ChatIntegration` - Template analytics in chat
- `PerformanceOptimizer` - Optimization tools

## 🎨 **Design System Integration**

### **Color Palette for Analytics**
```scss
// Performance Status Colors
$performance-excellent: #10b981; // Green
$performance-good: #3b82f6;      // Blue
$performance-fair: #f59e0b;      // Amber
$performance-poor: #ef4444;      // Red
$performance-critical: #dc2626;  // Dark Red

// A/B Testing Colors
$test-control: #6b7280;          // Gray
$test-variant: #8b5cf6;          // Purple
$test-winner: #10b981;           // Green
$test-statistical: #3b82f6;      // Blue

// Chart Colors
$chart-primary: #3b82f6;
$chart-secondary: #8b5cf6;
$chart-success: #10b981;
$chart-warning: #f59e0b;
$chart-danger: #ef4444;
```

### **Typography Scale**
```scss
// Analytics-specific typography
.analytics-title {
  font-size: 1.875rem;
  font-weight: 700;
  color: $gray-900;
}

.metrics-value {
  font-size: 2.25rem;
  font-weight: 800;
  font-variant-numeric: tabular-nums;
}

.metrics-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: $gray-600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
```

## 📱 **Mobile Responsiveness**

### **Breakpoint Strategy**
```scss
// Mobile-first breakpoints
$mobile: 320px;
$tablet: 768px;
$desktop: 1024px;
$wide: 1280px;

// Component-specific responsive behavior
.performance-dashboard {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
  
  @media (min-width: $tablet) {
    grid-template-columns: repeat(2, 1fr);
  }
  
  @media (min-width: $desktop) {
    grid-template-columns: repeat(3, 1fr);
  }
  
  @media (min-width: $wide) {
    grid-template-columns: repeat(4, 1fr);
  }
}
```

### **Touch-Friendly Design**
- Minimum 44px touch targets
- Swipe gestures for mobile navigation
- Pull-to-refresh functionality
- Haptic feedback for interactions

## 🔒 **Security & Permissions**

### **Role-Based Access Control**
```typescript
interface TemplateAnalyticsPermissions {
  canViewPerformance: boolean;
  canManageTemplates: boolean;
  canCreateABTests: boolean;
  canExportData: boolean;
  canViewSensitiveMetrics: boolean;
}

// Permission-based component rendering
const TemplateAnalyticsDashboard: React.FC = () => {
  const permissions = useTemplateAnalyticsPermissions();
  
  return (
    <div>
      {permissions.canViewPerformance && <PerformanceDashboard />}
      {permissions.canManageTemplates && <TemplateManagement />}
      {permissions.canCreateABTests && <ABTestingInterface />}
    </div>
  );
};
```

### **Data Privacy**
- Anonymize sensitive user data in analytics
- Implement data retention policies
- Secure API communication with JWT tokens
- Audit logging for sensitive operations

## 🚀 **Performance Optimization**

### **Code Splitting Strategy**
```typescript
// Lazy load analytics modules
const TemplateAnalytics = lazy(() => import('./features/template-analytics'));
const ABTestingDashboard = lazy(() => import('./features/template-analytics/components/abtesting'));
const TemplateManagement = lazy(() => import('./features/template-analytics/components/management'));

// Route-based code splitting
const AnalyticsRoutes = () => (
  <Routes>
    <Route path="/analytics" element={
      <Suspense fallback={<AnalyticsLoadingSkeleton />}>
        <TemplateAnalytics />
      </Suspense>
    } />
  </Routes>
);
```

### **Data Caching Strategy**
- RTK Query automatic caching
- Selective cache invalidation
- Background data refresh
- Optimistic updates for better UX

### **Bundle Optimization**
- Tree shaking for unused code
- Dynamic imports for heavy libraries
- Webpack bundle analysis
- CDN integration for static assets

## 📊 **Analytics & Monitoring**

### **User Interaction Tracking**
```typescript
// Analytics event tracking
const trackTemplateAnalyticsEvent = (event: AnalyticsEvent) => {
  analytics.track('Template Analytics', {
    action: event.action,
    templateKey: event.templateKey,
    timestamp: new Date().toISOString(),
    userId: getCurrentUserId(),
  });
};

// Usage examples
trackTemplateAnalyticsEvent({
  action: 'view_performance_dashboard',
  templateKey: 'sql_generation',
});

trackTemplateAnalyticsEvent({
  action: 'create_ab_test',
  templateKey: 'insight_generation',
});
```

### **Performance Monitoring**
- Component render time tracking
- API response time monitoring
- Error boundary implementation
- User session recording

## 🧪 **Testing Strategy**

### **Unit Testing**
```typescript
// Component testing with React Testing Library
describe('PerformanceDashboard', () => {
  it('displays performance metrics correctly', async () => {
    const mockData = createMockPerformanceData();
    render(<PerformanceDashboard data={mockData} />);
    
    expect(screen.getByText('Template Performance')).toBeInTheDocument();
    expect(screen.getByText('85.2%')).toBeInTheDocument(); // Success rate
  });
  
  it('handles loading states', () => {
    render(<PerformanceDashboard loading={true} />);
    expect(screen.getByTestId('loading-skeleton')).toBeInTheDocument();
  });
});
```

### **Integration Testing**
- API integration tests with MSW
- Redux store testing
- End-to-end testing with Playwright
- Visual regression testing

### **Accessibility Testing**
- Screen reader compatibility
- Keyboard navigation testing
- Color contrast validation
- ARIA label verification

## 📋 **Implementation Checklist**

### **Week 1-2: Foundation**
- [ ] Create module structure and routing
- [ ] Set up Redux store and RTK Query
- [ ] Implement base TypeScript interfaces
- [ ] Create shared components library
- [ ] Set up testing infrastructure

### **Week 3-4: Performance Dashboard**
- [ ] Build main dashboard layout
- [ ] Implement metrics visualization
- [ ] Add real-time data updates
- [ ] Create alert system
- [ ] Add export functionality

### **Week 5-6: A/B Testing**
- [ ] Build A/B testing dashboard
- [ ] Implement test creation wizard
- [ ] Add statistical analysis display
- [ ] Create test management interface
- [ ] Integrate recommendations system

### **Week 7-8: Template Management**
- [ ] Build template management hub
- [ ] Integrate Monaco editor
- [ ] Implement metadata management
- [ ] Add version control system
- [ ] Create quality assessment tools

### **Week 9-10: Advanced Features**
- [ ] Add advanced analytics
- [ ] Implement predictive insights
- [ ] Create automated recommendations
- [ ] Integrate with chat interface
- [ ] Optimize performance

## 🎯 **Success Metrics**

### **Technical Metrics**
- Page load time < 2 seconds
- API response time < 500ms
- Bundle size < 500KB per route
- 95%+ test coverage
- Zero accessibility violations

### **User Experience Metrics**
- User adoption rate > 80%
- Task completion rate > 90%
- User satisfaction score > 4.5/5
- Support ticket reduction > 30%
- Feature usage engagement > 70%

### **Business Impact Metrics**
- Template optimization rate increase
- A/B test success rate improvement
- Query performance enhancement
- User productivity increase
- System reliability improvement

## 🚀 **Deployment Strategy**

### **Staging Environment**
- Feature flag controlled rollout
- A/B testing of new features
- Performance monitoring
- User acceptance testing
- Security vulnerability scanning

### **Production Rollout**
- Gradual user group rollout
- Real-time monitoring
- Rollback procedures
- Performance tracking
- User feedback collection

This comprehensive plan ensures a systematic, user-focused implementation of the template analytics features while maintaining high code quality, performance, and user experience standards.

## 💻 **Detailed Component Implementations**

### **1. Performance Dashboard Component**

```typescript
// TemplatePerformanceDashboard.tsx
import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Select, DatePicker, Button, Spin, Alert } from 'antd';
import { LineChart, BarChart, PieChart } from '@/shared/components/charts';
import { useTemplatePerformanceQuery, usePerformanceAlertsQuery } from '@/features/template-analytics/api';

interface PerformanceDashboardProps {
  className?: string;
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const TemplatePerformanceDashboard: React.FC<PerformanceDashboardProps> = ({
  className,
  autoRefresh = true,
  refreshInterval = 30000
}) => {
  const [timeRange, setTimeRange] = useState<[Date, Date]>([
    new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), // 7 days ago
    new Date()
  ]);
  const [selectedIntentType, setSelectedIntentType] = useState<string>('all');

  const {
    data: dashboardData,
    isLoading,
    error,
    refetch
  } = useTemplatePerformanceQuery({
    startDate: timeRange[0].toISOString(),
    endDate: timeRange[1].toISOString(),
    intentType: selectedIntentType === 'all' ? undefined : selectedIntentType
  });

  const { data: alerts } = usePerformanceAlertsQuery();

  // Auto-refresh logic
  useEffect(() => {
    if (!autoRefresh) return;

    const interval = setInterval(() => {
      refetch();
    }, refreshInterval);

    return () => clearInterval(interval);
  }, [autoRefresh, refreshInterval, refetch]);

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spin size="large" />
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Performance Data"
        description="Unable to load template performance metrics. Please try again."
        type="error"
        showIcon
        action={
          <Button size="small" onClick={() => refetch()}>
            Retry
          </Button>
        }
      />
    );
  }

  return (
    <div className={`template-performance-dashboard ${className || ''}`}>
      {/* Header Controls */}
      <div className="dashboard-header mb-6">
        <Row gutter={16} align="middle">
          <Col span={8}>
            <h1 className="text-2xl font-bold">Template Performance</h1>
          </Col>
          <Col span={16} className="text-right">
            <Select
              value={selectedIntentType}
              onChange={setSelectedIntentType}
              style={{ width: 200, marginRight: 16 }}
              placeholder="Select Intent Type"
            >
              <Select.Option value="all">All Intent Types</Select.Option>
              <Select.Option value="sql_generation">SQL Generation</Select.Option>
              <Select.Option value="insight_generation">Insight Generation</Select.Option>
              <Select.Option value="explanation">Explanation</Select.Option>
            </Select>
            <DatePicker.RangePicker
              value={timeRange}
              onChange={(dates) => dates && setTimeRange(dates)}
              style={{ marginRight: 16 }}
            />
            <Button onClick={() => refetch()}>Refresh</Button>
          </Col>
        </Row>
      </div>

      {/* Alerts Section */}
      {alerts && alerts.length > 0 && (
        <div className="alerts-section mb-6">
          <Alert
            message={`${alerts.length} Performance Alert${alerts.length > 1 ? 's' : ''}`}
            description={
              <ul>
                {alerts.slice(0, 3).map(alert => (
                  <li key={alert.templateKey}>
                    <strong>{alert.templateKey}:</strong> {alert.message}
                  </li>
                ))}
                {alerts.length > 3 && <li>... and {alerts.length - 3} more</li>}
              </ul>
            }
            type="warning"
            showIcon
            closable
          />
        </div>
      )}

      {/* Key Metrics Grid */}
      <Row gutter={16} className="metrics-grid mb-6">
        <Col span={6}>
          <Card>
            <div className="metric-card">
              <div className="metric-value text-3xl font-bold text-blue-600">
                {dashboardData?.totalTemplates || 0}
              </div>
              <div className="metric-label text-gray-600">Total Templates</div>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div className="metric-card">
              <div className="metric-value text-3xl font-bold text-green-600">
                {((dashboardData?.overallSuccessRate || 0) * 100).toFixed(1)}%
              </div>
              <div className="metric-label text-gray-600">Overall Success Rate</div>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div className="metric-card">
              <div className="metric-value text-3xl font-bold text-purple-600">
                {dashboardData?.totalUsagesToday || 0}
              </div>
              <div className="metric-label text-gray-600">Usage Today</div>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div className="metric-card">
              <div className="metric-value text-3xl font-bold text-orange-600">
                {dashboardData?.needsAttention?.length || 0}
              </div>
              <div className="metric-label text-gray-600">Needs Attention</div>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Charts Section */}
      <Row gutter={16} className="charts-section">
        <Col span={12}>
          <Card title="Top Performing Templates" className="h-96">
            <BarChart
              data={dashboardData?.topPerformers?.map(template => ({
                name: template.templateName,
                successRate: template.successRate * 100,
                usageCount: template.totalUsages
              })) || []}
              xAxisKey="name"
              yAxisKey="successRate"
              height={300}
            />
          </Card>
        </Col>
        <Col span={12}>
          <Card title="Usage by Intent Type" className="h-96">
            <PieChart
              data={Object.entries(dashboardData?.usageByIntentType || {}).map(([type, count]) => ({
                name: type,
                value: count
              }))}
              height={300}
            />
          </Card>
        </Col>
      </Row>

      {/* Performance Trends */}
      <Row gutter={16} className="mt-6">
        <Col span={24}>
          <Card title="Performance Trends" className="h-96">
            <LineChart
              data={dashboardData?.recentTrends?.map(trend => ({
                date: new Date(trend.timestamp).toLocaleDateString(),
                successRate: trend.successRate * 100,
                usageCount: trend.usageCount,
                avgConfidence: trend.averageConfidenceScore * 100
              })) || []}
              xAxisKey="date"
              lines={[
                { key: 'successRate', color: '#10b981', name: 'Success Rate (%)' },
                { key: 'avgConfidence', color: '#3b82f6', name: 'Avg Confidence (%)' }
              ]}
              height={300}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};
```

### **2. A/B Testing Dashboard Component**

```typescript
// ABTestingDashboard.tsx
import React, { useState } from 'react';
import { Card, Table, Button, Modal, Form, Input, Select, Progress, Tag, Space } from 'antd';
import { PlusOutlined, PlayCircleOutlined, PauseCircleOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { useABTestsQuery, useCreateABTestMutation, useCompleteTestMutation } from '@/features/template-analytics/api';

export const ABTestingDashboard: React.FC = () => {
  const [isCreateModalVisible, setIsCreateModalVisible] = useState(false);
  const [form] = Form.useForm();

  const { data: activeTests, isLoading } = useABTestsQuery({ status: 'active' });
  const [createABTest] = useCreateABTestMutation();
  const [completeTest] = useCompleteTestMutation();

  const handleCreateTest = async (values: any) => {
    try {
      await createABTest({
        testName: values.testName,
        originalTemplateKey: values.originalTemplate,
        variantTemplateContent: values.variantContent,
        trafficSplitPercent: values.trafficSplit,
        startDate: new Date().toISOString(),
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(), // 30 days
        createdBy: 'current-user' // Get from auth context
      }).unwrap();

      setIsCreateModalVisible(false);
      form.resetFields();
    } catch (error) {
      console.error('Failed to create A/B test:', error);
    }
  };

  const handleCompleteTest = async (testId: number) => {
    try {
      await completeTest({ testId, implementWinner: true }).unwrap();
    } catch (error) {
      console.error('Failed to complete test:', error);
    }
  };

  const getStatusTag = (status: string) => {
    const statusConfig = {
      running: { color: 'blue', icon: <PlayCircleOutlined /> },
      paused: { color: 'orange', icon: <PauseCircleOutlined /> },
      completed: { color: 'green', icon: <CheckCircleOutlined /> }
    };

    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.running;

    return (
      <Tag color={config.color} icon={config.icon}>
        {status.toUpperCase()}
      </Tag>
    );
  };

  const columns = [
    {
      title: 'Test Name',
      dataIndex: 'testName',
      key: 'testName',
      render: (text: string, record: any) => (
        <div>
          <div className="font-semibold">{text}</div>
          <div className="text-sm text-gray-500">
            {record.originalTemplateKey} vs {record.variantTemplateKey}
          </div>
        </div>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => getStatusTag(status)
    },
    {
      title: 'Progress',
      key: 'progress',
      render: (record: any) => {
        const totalSamples = record.originalUsageCount + record.variantUsageCount;
        const targetSamples = 1000; // Configurable target
        const progress = Math.min((totalSamples / targetSamples) * 100, 100);

        return (
          <div>
            <Progress percent={progress} size="small" />
            <div className="text-xs text-gray-500">
              {totalSamples} / {targetSamples} samples
            </div>
          </div>
        );
      }
    },
    {
      title: 'Performance',
      key: 'performance',
      render: (record: any) => (
        <div className="text-sm">
          <div>Original: {((record.originalSuccessRate || 0) * 100).toFixed(1)}%</div>
          <div>Variant: {((record.variantSuccessRate || 0) * 100).toFixed(1)}%</div>
          {record.statisticalSignificance && (
            <div className="text-blue-600">
              Significance: {(record.statisticalSignificance * 100).toFixed(1)}%
            </div>
          )}
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: any) => (
        <Space>
          <Button size="small" onClick={() => handleCompleteTest(record.id)}>
            Complete
          </Button>
          <Button size="small" type="link">
            Analyze
          </Button>
        </Space>
      )
    }
  ];

  return (
    <div className="ab-testing-dashboard">
      <div className="dashboard-header mb-6">
        <div className="flex justify-between items-center">
          <h1 className="text-2xl font-bold">A/B Testing</h1>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setIsCreateModalVisible(true)}
          >
            Create Test
          </Button>
        </div>
      </div>

      <Card title="Active Tests">
        <Table
          columns={columns}
          dataSource={activeTests}
          loading={isLoading}
          rowKey="id"
          pagination={{ pageSize: 10 }}
        />
      </Card>

      {/* Create Test Modal */}
      <Modal
        title="Create A/B Test"
        open={isCreateModalVisible}
        onCancel={() => setIsCreateModalVisible(false)}
        onOk={() => form.submit()}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleCreateTest}
        >
          <Form.Item
            name="testName"
            label="Test Name"
            rules={[{ required: true, message: 'Please enter test name' }]}
          >
            <Input placeholder="Enter descriptive test name" />
          </Form.Item>

          <Form.Item
            name="originalTemplate"
            label="Original Template"
            rules={[{ required: true, message: 'Please select original template' }]}
          >
            <Select placeholder="Select template to test">
              <Select.Option value="sql_generation">SQL Generation</Select.Option>
              <Select.Option value="insight_generation">Insight Generation</Select.Option>
              <Select.Option value="explanation">Explanation</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="variantContent"
            label="Variant Template Content"
            rules={[{ required: true, message: 'Please enter variant content' }]}
          >
            <Input.TextArea
              rows={6}
              placeholder="Enter the variant template content..."
            />
          </Form.Item>

          <Form.Item
            name="trafficSplit"
            label="Traffic Split (%)"
            initialValue={50}
            rules={[{ required: true, message: 'Please enter traffic split' }]}
          >
            <Select>
              <Select.Option value={10}>10% Variant</Select.Option>
              <Select.Option value={25}>25% Variant</Select.Option>
              <Select.Option value={50}>50% Variant</Select.Option>
              <Select.Option value={75}>75% Variant</Select.Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
```

### **3. RTK Query API Integration**

```typescript
// features/template-analytics/api/templateAnalyticsApi.ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  PerformanceDashboardData,
  TemplatePerformanceMetrics,
  ABTestDetails,
  ABTestRequest,
  ABTestResult,
  TemplateWithMetrics
} from '../types';

export const templateAnalyticsApi = createApi({
  reducerPath: 'templateAnalyticsApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/templateanalytics/',
    prepareHeaders: (headers, { getState }) => {
      // Add auth token from state
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: [
    'Performance',
    'ABTest',
    'Template',
    'Alerts',
    'Recommendations'
  ],
  endpoints: (builder) => ({
    // Performance endpoints
    getPerformanceDashboard: builder.query<PerformanceDashboardData, void>({
      query: () => 'dashboard',
      providesTags: ['Performance'],
    }),

    getTemplatePerformance: builder.query<TemplatePerformanceMetrics, string>({
      query: (templateKey) => `performance/${templateKey}`,
      providesTags: (result, error, templateKey) => [
        { type: 'Performance', id: templateKey },
      ],
    }),

    getTopPerformingTemplates: builder.query<TemplatePerformanceMetrics[], {
      intentType?: string;
      count?: number;
    }>({
      query: ({ intentType, count = 10 }) => ({
        url: 'performance/top',
        params: { intentType, count },
      }),
      providesTags: ['Performance'],
    }),

    getPerformanceAlerts: builder.query<PerformanceAlert[], void>({
      query: () => 'alerts',
      providesTags: ['Alerts'],
    }),

    // A/B Testing endpoints
    getABTests: builder.query<ABTestDetails[], { status?: string }>({
      query: ({ status }) => ({
        url: status ? `abtests/${status}` : 'abtests/active',
      }),
      providesTags: ['ABTest'],
    }),

    createABTest: builder.mutation<ABTestResult, ABTestRequest>({
      query: (request) => ({
        url: 'abtests',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['ABTest'],
    }),

    completeABTest: builder.mutation<any, { testId: number; implementWinner?: boolean }>({
      query: ({ testId, implementWinner = true }) => ({
        url: `abtests/${testId}/complete`,
        method: 'POST',
        params: { implementWinner },
      }),
      invalidatesTags: ['ABTest'],
    }),

    getABTestAnalysis: builder.query<ABTestAnalysis, number>({
      query: (testId) => `abtests/${testId}/analysis`,
      providesTags: (result, error, testId) => [
        { type: 'ABTest', id: testId },
      ],
    }),

    // Template Management endpoints
    getTemplateWithMetrics: builder.query<TemplateWithMetrics, string>({
      query: (templateKey) => `templates/${templateKey}`,
      providesTags: (result, error, templateKey) => [
        { type: 'Template', id: templateKey },
      ],
    }),

    getTemplateManagementDashboard: builder.query<TemplateManagementDashboard, void>({
      query: () => 'templates/dashboard',
      providesTags: ['Template'],
    }),

    // User feedback
    trackUserFeedback: builder.mutation<void, {
      usageId: string;
      rating: number;
      userId?: string;
      comments?: string;
    }>({
      query: (feedback) => ({
        url: 'feedback',
        method: 'POST',
        body: feedback,
      }),
    }),
  }),
});

// Export hooks for use in components
export const {
  useGetPerformanceDashboardQuery,
  useGetTemplatePerformanceQuery,
  useGetTopPerformingTemplatesQuery,
  useGetPerformanceAlertsQuery,
  useGetABTestsQuery,
  useCreateABTestMutation,
  useCompleteABTestMutation,
  useGetABTestAnalysisQuery,
  useGetTemplateWithMetricsQuery,
  useGetTemplateManagementDashboardQuery,
  useTrackUserFeedbackMutation,
} = templateAnalyticsApi;
```

### **4. Redux Store Integration**

```typescript
// store/index.ts
import { configureStore } from '@reduxjs/toolkit';
import { templateAnalyticsApi } from '@/features/template-analytics/api';
import templateAnalyticsReducer from '@/features/template-analytics/slice';

export const store = configureStore({
  reducer: {
    templateAnalytics: templateAnalyticsReducer,
    [templateAnalyticsApi.reducerPath]: templateAnalyticsApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(templateAnalyticsApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
```

### **5. Routing Configuration**

```typescript
// routes/TemplateAnalyticsRoutes.tsx
import React, { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';
import { AnalyticsLoadingSkeleton } from '@/shared/components/loading';

// Lazy load components for code splitting
const TemplatePerformanceDashboard = lazy(() =>
  import('@/features/template-analytics/components/dashboard/TemplatePerformanceDashboard')
);
const ABTestingDashboard = lazy(() =>
  import('@/features/template-analytics/components/abtesting/ABTestingDashboard')
);
const TemplateManagementHub = lazy(() =>
  import('@/features/template-analytics/components/management/TemplateManagementHub')
);

export const TemplateAnalyticsRoutes: React.FC = () => {
  return (
    <Suspense fallback={<AnalyticsLoadingSkeleton />}>
      <Routes>
        <Route path="/performance" element={<TemplatePerformanceDashboard />} />
        <Route path="/ab-testing" element={<ABTestingDashboard />} />
        <Route path="/management" element={<TemplateManagementHub />} />
        <Route path="/" element={<TemplatePerformanceDashboard />} />
      </Routes>
    </Suspense>
  );
};
```

This detailed implementation plan provides concrete code examples and architectural patterns for integrating the template analytics features into the frontend application.
