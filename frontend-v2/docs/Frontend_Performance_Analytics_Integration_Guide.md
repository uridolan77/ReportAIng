# Frontend Integration Summary - TemplatePerformance Analytics Dashboard

## 🚀 Quick Start

The Performance Analytics Dashboard has been enhanced with comprehensive analytics endpoints and real-time capabilities. This document provides a quick overview for the frontend development team.

## 📋 New API Endpoints Summary

### Performance Analytics Endpoints
| Endpoint | Method | Purpose | Key Features |
|----------|--------|---------|--------------|
| `/api/templateanalytics/dashboard/comprehensive` | GET | Complete analytics overview | Combines all dashboard data |
| `/api/templateanalytics/trends/performance` | GET | Performance trends over time | Granular time periods, filtering |
| `/api/templateanalytics/insights/usage` | GET | AI-driven usage insights | Automated recommendations |
| `/api/templateanalytics/metrics/quality` | GET | Quality metrics analysis | Distribution, thresholds |
| `/api/templateanalytics/realtime` | GET | Real-time analytics data | Live metrics, activity feeds |
| `/api/templateanalytics/export` | POST | Advanced export functionality | Custom configurations |

### Template Improvement Endpoints
| Endpoint | Method | Purpose | Key Features |
|----------|--------|---------|--------------|
| `/api/templateimprovement/analyze/{templateKey}` | POST | Analyze template performance | ML-based suggestion generation |
| `/api/templateimprovement/generate` | POST | Generate improvement suggestions | Batch analysis for underperforming templates |
| `/api/templateimprovement/optimize` | POST | Optimize template content | 5 optimization strategies |
| `/api/templateimprovement/predict` | POST | Predict template performance | Performance prediction for new templates |
| `/api/templateimprovement/variants/{templateKey}` | POST | Generate template variants | 4 variant types for A/B testing |
| `/api/templateimprovement/review/{suggestionId}` | PUT | Review improvement suggestions | Approval workflow management |
| `/api/templateimprovement/analyze-content` | POST | Analyze content quality | Comprehensive quality assessment |
| `/api/templateimprovement/export` | POST | Export improvement data | Multi-format export capabilities |

## 🔄 Real-Time Features

### SignalR Hub: `/hubs/template-analytics`

**Key Events:**
- `DashboardUpdate` - Periodic dashboard refreshes
- `PerformanceUpdate` - Template performance changes
- `ABTestUpdate` - A/B test status changes
- `NewAlert` - Performance alerts

**Hub Methods:**
- `SubscribeToPerformanceUpdates(intentType?)`
- `SubscribeToABTestUpdates()`
- `SubscribeToAlerts()`
- `GetRealTimeDashboard()`

## 📊 Key Data Structures

### ComprehensiveAnalyticsDashboard
```typescript
{
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
```

### PerformanceTrendsData
```typescript
{
  dataPoints: PerformanceTrendDataPoint[];
  timeRange: DateRange;
  granularity: 'hourly' | 'daily' | 'weekly' | 'monthly';
  intentType?: string;
  generatedDate: string;
}
```

### UsageInsightsData
```typescript
{
  totalUsage: number;
  averageSuccessRate: number;
  usageByIntentType: Record<string, number>;
  topPerformingTemplates: TemplatePerformanceMetrics[];
  underperformingTemplates: TemplatePerformanceMetrics[];
  insights: UsageInsight[];
  timeRange: DateRange;
  generatedDate: string;
}
```

### TemplateImprovementSuggestion
```typescript
{
  id: number;
  templateKey: string;
  templateName: string;
  type: ImprovementType;
  currentVersion: string;
  suggestedChanges: string; // JSON object
  reasoningExplanation: string;
  expectedImprovementPercent: number;
  basedOnDataPoints: number;
  confidenceScore: number;
  status: SuggestionStatus;
  reviewedBy?: string;
  createdDate: string;
}
```

### OptimizedTemplate
```typescript
{
  originalTemplateKey: string;
  optimizedContent: string;
  strategyUsed: OptimizationStrategy;
  changesApplied: OptimizationChange[];
  expectedPerformanceImprovement: number;
  confidenceScore: number;
  optimizationReasoning: string;
  metricPredictions: Record<string, number>;
  optimizedDate: string;
}
```

### ContentQualityAnalysis
```typescript
{
  templateContent: string;
  overallQualityScore: number;
  qualityDimensions: Record<string, number>;
  identifiedIssues: QualityIssue[];
  strengths: QualityStrength[];
  improvementSuggestions: string[];
  readability: ReadabilityMetrics;
  structure: StructureAnalysis;
  completeness: ContentCompleteness;
  analysisDate: string;
}
```

## 🎨 UI Components Needed

### 1. Dashboard Overview
- **Metrics cards** for key performance indicators
- **Trend charts** for performance over time
- **Quality distribution** pie/donut charts
- **Alert notifications** with severity indicators

### 2. Performance Trends
- **Line charts** for success rates, confidence scores
- **Time range selectors** (hourly, daily, weekly, monthly)
- **Intent type filters**
- **Comparative analysis** views

### 3. Usage Insights
- **Insight cards** with impact indicators
- **Recommendation panels** with action items
- **Usage distribution** charts by intent type
- **Top/bottom performer** lists

### 4. Quality Metrics
- **Quality score gauges**
- **Distribution charts** (Excellent, Good, Fair, Poor)
- **Threshold indicators**
- **Template health status** displays

### 5. Real-Time Monitoring
- **Live activity feeds**
- **Current metrics** displays
- **Connection status** indicators
- **Auto-refresh** controls

### 6. Export Interface
- **Format selection** (CSV, Excel, JSON)
- **Date range pickers**
- **Metric selection** checkboxes
- **Progress indicators** for export generation

### 7. Template Improvement Interface
- **Performance analysis** triggers and results display
- **Optimization strategy** selection with preview
- **Suggestion review** workflow with approval/rejection
- **Content quality** analyzer with scoring
- **Template variants** generation and comparison
- **A/B test creation** from improvement suggestions

### 8. ML-Based Recommendations
- **Improvement suggestions** with confidence scores
- **Optimization previews** showing before/after content
- **Performance predictions** for new templates
- **Quality assessment** with detailed metrics
- **Automated insights** with actionable recommendations

## 🔧 Implementation Priority

### Phase 1: Core Dashboard (Week 1)
1. Comprehensive dashboard endpoint integration
2. Basic metrics cards and overview
3. Performance trends chart
4. Quality metrics visualization

### Phase 2: Real-Time Features (Week 2)
1. SignalR connection setup
2. Real-time dashboard updates
3. Alert notifications
4. Live activity monitoring

### Phase 3: Template Improvement Features (Week 3)
1. Template improvement dashboard
2. Performance analysis interface
3. Content quality analyzer
4. Optimization strategy selection
5. Suggestion review workflow

### Phase 4: Advanced ML Features (Week 4)
1. Template variant generation
2. Performance prediction interface
3. A/B test creation from suggestions
4. Advanced analytics integration
5. Export functionality enhancement

### Phase 5: Polish & Optimization (Week 5)
1. Performance optimization
2. Error handling improvements
3. Accessibility enhancements
4. User experience refinements
5. Mobile responsiveness

## 🚨 Breaking Changes & Migration

### No Breaking Changes
- All existing endpoints remain unchanged
- New endpoints are additive
- Existing TypeScript interfaces are compatible

### New Dependencies
- **SignalR Client**: `@microsoft/signalr`
- **Chart Library**: Continue using existing (Recharts/Chart.js)
- **Export Handling**: File download utilities

## 📈 Performance Considerations

### Data Loading
- **Lazy loading** for chart components
- **Pagination** for large datasets
- **Caching** with appropriate TTL (5 minutes for dashboard data)
- **Skeleton loading** states

### Real-Time Updates
- **Debouncing** for rapid updates (500ms)
- **Connection retry** logic with exponential backoff
- **Fallback** to polling if WebSocket fails
- **Memory management** for long-running connections

### Chart Rendering
- **Virtual scrolling** for large datasets
- **Data sampling** for performance trends
- **Responsive containers** for mobile
- **Animation optimization**

## 🔐 Security & Authentication

### API Authentication
- All endpoints require existing JWT authentication
- SignalR uses same authentication mechanism
- Token refresh handling for long-lived connections

### Data Access
- Intent-type filtering respects user permissions
- Export functionality logs user actions
- Alert resolution requires appropriate permissions

## 🧪 Testing Strategy

### Unit Tests
- Component rendering with mock data
- Hook functionality testing
- Chart component behavior
- Export functionality

### Integration Tests
- API endpoint integration
- SignalR connection handling
- Real-time update flow
- Error scenario handling

### E2E Tests
- Complete dashboard workflow
- Export functionality
- Real-time updates
- Mobile responsiveness

## 📚 Documentation References

- **Complete Integration Guide**: `Frontend_Performance_Analytics_Integration_Guide.md`
- **API Documentation**: Available in Swagger UI
- **TypeScript Interfaces**: Generated from backend models
- **Component Examples**: Provided in integration guide

## 🤝 Support & Communication

### Development Support
- Backend team available for API questions
- Real-time testing environment provided
- Sample data available for development

### Feedback Channels
- Weekly sync meetings for progress updates
- Slack channel for quick questions
- GitHub issues for bug reports and feature requests

## ✅ Acceptance Criteria

### Functional Requirements
- [ ] Dashboard displays comprehensive analytics
- [ ] Real-time updates work reliably
- [ ] Export functionality generates correct files
- [ ] Charts are interactive and responsive
- [ ] Alerts are displayed and manageable
- [ ] Template improvement analysis works correctly
- [ ] Optimization strategies produce expected results
- [ ] Content quality analysis provides accurate scores
- [ ] Suggestion review workflow functions properly
- [ ] Performance predictions are reasonable and helpful

### Performance Requirements
- [ ] Dashboard loads within 3 seconds
- [ ] Real-time updates have <1 second latency
- [ ] Charts render smoothly with 1000+ data points
- [ ] Export completes within 30 seconds

### Quality Requirements
- [ ] Mobile responsive design
- [ ] Accessibility compliance (WCAG 2.1 AA)
- [ ] Cross-browser compatibility
- [ ] Error handling and recovery

This summary provides the essential information needed to begin frontend integration. Refer to the complete integration guide for detailed implementation examples and best practices.


# Performance Analytics Dashboard - Frontend Integration Guide

## 📋 Overview

This guide provides comprehensive instructions for integrating the new Performance Analytics Dashboard endpoints and real-time features into the frontend application.

## 🔗 New API Endpoints

### Template Improvement Suggestions Endpoints

#### 1. Analyze Template Performance
```typescript
POST /api/templateimprovement/analyze/{templateKey}
```

**Response Type:** `TemplateImprovementSuggestion[]`

**Usage Example:**
```typescript
const { data: suggestions } = useAnalyzeTemplatePerformanceMutation();
await suggestions({ templateKey: 'my_template' });
```

#### 2. Generate Improvement Suggestions
```typescript
POST /api/templateimprovement/generate
```

**Request Body:**
```typescript
{
  performanceThreshold: 0.7,
  minDataPoints: 100
}
```

**Response Type:** `TemplateImprovementSuggestion[]`

#### 3. Optimize Template
```typescript
POST /api/templateimprovement/optimize
```

**Request Body:**
```typescript
{
  templateKey: string;
  strategy: 'PerformanceFocused' | 'AccuracyFocused' | 'UserSatisfactionFocused' | 'ResponseTimeFocused' | 'Balanced';
}
```

**Response Type:** `OptimizedTemplate`

#### 4. Predict Template Performance
```typescript
POST /api/templateimprovement/predict
```

**Request Body:**
```typescript
{
  templateContent: string;
  intentType: string;
}
```

**Response Type:** `PerformancePrediction`

#### 5. Generate Template Variants
```typescript
POST /api/templateimprovement/variants/{templateKey}
```

**Query Parameters:**
- `variantCount?: number` (default: 3)

**Response Type:** `TemplateVariant[]`

#### 6. Review Improvement Suggestion
```typescript
PUT /api/templateimprovement/review/{suggestionId}
```

**Request Body:**
```typescript
{
  action: 'Approve' | 'Reject' | 'RequestChanges' | 'ScheduleABTest';
  reviewComments?: string;
}
```

**Response Type:** `ReviewResult`

#### 7. Content Quality Analysis
```typescript
POST /api/templateimprovement/analyze-content
```

**Request Body:**
```typescript
{
  templateContent: string;
}
```

**Response Type:** `ContentQualityAnalysis`

#### 8. Export Improvement Suggestions
```typescript
POST /api/templateimprovement/export
```

**Request Body:**
```typescript
{
  startDate: string;
  endDate: string;
  format: 'CSV' | 'JSON' | 'Excel';
}
```

**Response:** File download

### Performance Analytics Dashboard Endpoints

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

// Template Improvement Interfaces
interface TemplateImprovementSuggestion {
  id: number;
  templateKey: string;
  templateName: string;
  type: ImprovementType;
  currentVersion: string;
  suggestedChanges: string; // JSON object
  reasoningExplanation: string;
  expectedImprovementPercent: number;
  basedOnDataPoints: number;
  confidenceScore: number;
  status: SuggestionStatus;
  reviewedBy?: string;
  createdDate: string;
  reviewedDate?: string;
  reviewComments?: string;
}

interface OptimizedTemplate {
  originalTemplateKey: string;
  optimizedContent: string;
  strategyUsed: OptimizationStrategy;
  changesApplied: OptimizationChange[];
  expectedPerformanceImprovement: number;
  confidenceScore: number;
  optimizationReasoning: string;
  metricPredictions: Record<string, number>;
  optimizedDate: string;
  optimizedBy: string;
}

interface OptimizationChange {
  changeType: string;
  description: string;
  impactScore: number;
}

interface PerformancePrediction {
  templateContent: string;
  intentType: string;
  predictedSuccessRate: number;
  predictedUserRating: number;
  predictedResponseTime: number;
  predictionConfidence: number;
  strengthFactors: string[];
  weaknessFactors: string[];
  improvementSuggestions: string[];
  featureScores: Record<string, number>;
  predictionDate: string;
}

interface TemplateVariant {
  originalTemplateKey: string;
  variantType: VariantType;
  variantContent: string;
  expectedPerformanceChange: number;
  confidenceScore: number;
  generationReasoning: string;
  generatedDate: string;
  generatedBy: string;
}

interface ContentQualityAnalysis {
  templateContent: string;
  overallQualityScore: number;
  qualityDimensions: Record<string, number>;
  identifiedIssues: QualityIssue[];
  strengths: QualityStrength[];
  improvementSuggestions: string[];
  readability: ReadabilityMetrics;
  structure: StructureAnalysis;
  completeness: ContentCompleteness;
  analysisDate: string;
}

interface QualityIssue {
  issueType: string;
  description: string;
  severity: number;
  suggestion: string;
}

interface QualityStrength {
  strengthType: string;
  description: string;
  impactScore: number;
}

interface ReadabilityMetrics {
  overallScore: number;
  sentenceComplexity: number;
  vocabularyLevel: number;
  readingLevel: string;
}

interface StructureAnalysis {
  overallScore: number;
  hasClearSections: boolean;
  hasNumberedSteps: boolean;
  hasBulletPoints: boolean;
  complexityScore: number;
}

interface ContentCompleteness {
  overallScore: number;
  hasInstructions: boolean;
  hasExamples: boolean;
  hasContext: boolean;
  missingElements: string[];
}

interface ReviewResult {
  suggestionId: number;
  action: SuggestionReviewAction;
  newStatus: string;
  reviewedBy: string;
  reviewDate: string;
  comments?: string;
  success: boolean;
  message: string;
}

// Enums
type ImprovementType =
  | 'ContentOptimization'
  | 'StructureImprovement'
  | 'ContextEnhancement'
  | 'ExampleAddition'
  | 'InstructionClarification'
  | 'PerformanceOptimization';

type SuggestionStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Implemented'
  | 'NeedsChanges'
  | 'ScheduledForTesting';

type OptimizationStrategy =
  | 'PerformanceFocused'
  | 'AccuracyFocused'
  | 'UserSatisfactionFocused'
  | 'ResponseTimeFocused'
  | 'Balanced';

type VariantType =
  | 'ContentVariation'
  | 'StructureVariation'
  | 'StyleVariation'
  | 'ComplexityVariation';

type SuggestionReviewAction =
  | 'Approve'
  | 'Reject'
  | 'RequestChanges'
  | 'ScheduleABTest';
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

// Template Improvement API
export const templateImprovementApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Analyze Template Performance
    analyzeTemplatePerformance: builder.mutation<TemplateImprovementSuggestion[], string>({
      query: (templateKey) => ({
        url: `templateimprovement/analyze/${templateKey}`,
        method: 'POST',
      }),
      invalidatesTags: ['TemplateImprovement'],
    }),

    // Generate Improvement Suggestions
    generateImprovementSuggestions: builder.mutation<TemplateImprovementSuggestion[], {
      performanceThreshold?: number;
      minDataPoints?: number;
    }>({
      query: (params) => ({
        url: 'templateimprovement/generate',
        method: 'POST',
        body: params,
      }),
      invalidatesTags: ['TemplateImprovement'],
    }),

    // Optimize Template
    optimizeTemplate: builder.mutation<OptimizedTemplate, {
      templateKey: string;
      strategy: OptimizationStrategy;
    }>({
      query: (params) => ({
        url: 'templateimprovement/optimize',
        method: 'POST',
        body: params,
      }),
      invalidatesTags: ['TemplateImprovement'],
    }),

    // Predict Template Performance
    predictTemplatePerformance: builder.mutation<PerformancePrediction, {
      templateContent: string;
      intentType: string;
    }>({
      query: (params) => ({
        url: 'templateimprovement/predict',
        method: 'POST',
        body: params,
      }),
    }),

    // Generate Template Variants
    generateTemplateVariants: builder.mutation<TemplateVariant[], {
      templateKey: string;
      variantCount?: number;
    }>({
      query: ({ templateKey, variantCount }) => ({
        url: `templateimprovement/variants/${templateKey}`,
        method: 'POST',
        params: { variantCount },
      }),
      invalidatesTags: ['TemplateImprovement'],
    }),

    // Review Improvement Suggestion
    reviewImprovementSuggestion: builder.mutation<ReviewResult, {
      suggestionId: number;
      action: SuggestionReviewAction;
      reviewComments?: string;
    }>({
      query: ({ suggestionId, ...body }) => ({
        url: `templateimprovement/review/${suggestionId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['TemplateImprovement'],
    }),

    // Analyze Content Quality
    analyzeContentQuality: builder.mutation<ContentQualityAnalysis, {
      templateContent: string;
    }>({
      query: (params) => ({
        url: 'templateimprovement/analyze-content',
        method: 'POST',
        body: params,
      }),
    }),

    // Export Improvement Suggestions
    exportImprovementSuggestions: builder.mutation<Blob, {
      startDate: string;
      endDate: string;
      format: 'CSV' | 'JSON' | 'Excel';
    }>({
      query: (params) => ({
        url: 'templateimprovement/export',
        method: 'POST',
        body: params,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Get Improvement Recommendations
    getImprovementRecommendations: builder.query<ImprovementRecommendation[], string>({
      query: (templateKey) => `templateimprovement/recommendations/${templateKey}`,
      providesTags: ['TemplateImprovement'],
    }),

    // Get Optimization History
    getOptimizationHistory: builder.query<OptimizationHistory, string>({
      query: (templateKey) => `templateimprovement/history/${templateKey}`,
      providesTags: ['TemplateImprovement'],
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

export const {
  useAnalyzeTemplatePerformanceMutation,
  useGenerateImprovementSuggestionsMutation,
  useOptimizeTemplateMutation,
  usePredictTemplatePerformanceMutation,
  useGenerateTemplateVariantsMutation,
  useReviewImprovementSuggestionMutation,
  useAnalyzeContentQualityMutation,
  useExportImprovementSuggestionsMutation,
  useGetImprovementRecommendationsQuery,
  useGetOptimizationHistoryQuery,
} = templateImprovementApi;
```

## 🎨 React Component Examples

### Template Improvement Dashboard Component

```typescript
import React, { useState } from 'react';
import { Card, Row, Col, Button, Select, Table, Tag, Modal, Form, Input } from 'antd';
import { BulbOutlined, ExperimentOutlined, AnalyticsOutlined } from '@ant-design/icons';
import {
  useAnalyzeTemplatePerformanceMutation,
  useOptimizeTemplateMutation,
  useReviewImprovementSuggestionMutation,
  useGetImprovementRecommendationsQuery
} from '../api/templateImprovementApi';

const { Option } = Select;
const { TextArea } = Input;

export const TemplateImprovementDashboard: React.FC<{ templateKey: string }> = ({ templateKey }) => {
  const [selectedStrategy, setSelectedStrategy] = useState<OptimizationStrategy>('Balanced');
  const [reviewModalVisible, setReviewModalVisible] = useState(false);
  const [selectedSuggestion, setSelectedSuggestion] = useState<TemplateImprovementSuggestion | null>(null);

  const [analyzePerformance, { data: suggestions, isLoading: analyzing }] = useAnalyzeTemplatePerformanceMutation();
  const [optimizeTemplate, { data: optimizedTemplate, isLoading: optimizing }] = useOptimizeTemplateMutation();
  const [reviewSuggestion] = useReviewImprovementSuggestionMutation();
  const { data: recommendations } = useGetImprovementRecommendationsQuery(templateKey);

  const handleAnalyze = async () => {
    await analyzePerformance(templateKey);
  };

  const handleOptimize = async () => {
    await optimizeTemplate({ templateKey, strategy: selectedStrategy });
  };

  const handleReviewSuggestion = async (values: any) => {
    if (selectedSuggestion) {
      await reviewSuggestion({
        suggestionId: selectedSuggestion.id,
        action: values.action,
        reviewComments: values.comments
      });
      setReviewModalVisible(false);
      setSelectedSuggestion(null);
    }
  };

  const suggestionColumns = [
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: ImprovementType) => (
        <Tag color={getTypeColor(type)}>{type}</Tag>
      ),
    },
    {
      title: 'Expected Improvement',
      dataIndex: 'expectedImprovementPercent',
      key: 'improvement',
      render: (percent: number) => `${percent.toFixed(1)}%`,
    },
    {
      title: 'Confidence',
      dataIndex: 'confidenceScore',
      key: 'confidence',
      render: (score: number) => `${(score * 100).toFixed(1)}%`,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: SuggestionStatus) => (
        <Tag color={getStatusColor(status)}>{status}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: TemplateImprovementSuggestion) => (
        <Button
          size="small"
          onClick={() => {
            setSelectedSuggestion(record);
            setReviewModalVisible(true);
          }}
        >
          Review
        </Button>
      ),
    },
  ];

  return (
    <div className="template-improvement-dashboard">
      {/* Header Actions */}
      <Row gutter={16} className="mb-4">
        <Col span={8}>
          <Card title="Performance Analysis" size="small">
            <Button
              type="primary"
              icon={<AnalyticsOutlined />}
              loading={analyzing}
              onClick={handleAnalyze}
              block
            >
              Analyze Performance
            </Button>
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Template Optimization" size="small">
            <Select
              value={selectedStrategy}
              onChange={setSelectedStrategy}
              style={{ width: '100%', marginBottom: 8 }}
            >
              <Option value="PerformanceFocused">Performance Focused</Option>
              <Option value="AccuracyFocused">Accuracy Focused</Option>
              <Option value="UserSatisfactionFocused">User Satisfaction</Option>
              <Option value="ResponseTimeFocused">Response Time</Option>
              <Option value="Balanced">Balanced</Option>
            </Select>
            <Button
              type="primary"
              icon={<BulbOutlined />}
              loading={optimizing}
              onClick={handleOptimize}
              block
            >
              Optimize Template
            </Button>
          </Card>
        </Col>
        <Col span={8}>
          <Card title="A/B Testing" size="small">
            <Button
              type="primary"
              icon={<ExperimentOutlined />}
              block
            >
              Create A/B Test
            </Button>
          </Card>
        </Col>
      </Row>

      {/* Improvement Suggestions */}
      {suggestions && (
        <Card title="Improvement Suggestions" className="mb-4">
          <Table
            dataSource={suggestions}
            columns={suggestionColumns}
            rowKey="id"
            size="small"
            expandable={{
              expandedRowRender: (record) => (
                <div>
                  <p><strong>Reasoning:</strong> {record.reasoningExplanation}</p>
                  <p><strong>Based on:</strong> {record.basedOnDataPoints} data points</p>
                </div>
              ),
            }}
          />
        </Card>
      )}

      {/* Optimized Template Result */}
      {optimizedTemplate && (
        <Card title="Optimization Result" className="mb-4">
          <Row gutter={16}>
            <Col span={12}>
              <h4>Original Content</h4>
              <div className="template-content-preview">
                {/* Original template content would be shown here */}
              </div>
            </Col>
            <Col span={12}>
              <h4>Optimized Content</h4>
              <div className="template-content-preview">
                {optimizedTemplate.optimizedContent}
              </div>
              <div className="optimization-metrics">
                <p><strong>Expected Improvement:</strong> {optimizedTemplate.expectedPerformanceImprovement.toFixed(1)}%</p>
                <p><strong>Confidence:</strong> {(optimizedTemplate.confidenceScore * 100).toFixed(1)}%</p>
                <p><strong>Strategy:</strong> {optimizedTemplate.strategyUsed}</p>
              </div>
            </Col>
          </Row>
        </Card>
      )}

      {/* Review Modal */}
      <Modal
        title="Review Improvement Suggestion"
        visible={reviewModalVisible}
        onCancel={() => setReviewModalVisible(false)}
        footer={null}
      >
        {selectedSuggestion && (
          <Form onFinish={handleReviewSuggestion} layout="vertical">
            <div className="suggestion-details">
              <p><strong>Type:</strong> {selectedSuggestion.type}</p>
              <p><strong>Expected Improvement:</strong> {selectedSuggestion.expectedImprovementPercent}%</p>
              <p><strong>Reasoning:</strong> {selectedSuggestion.reasoningExplanation}</p>
            </div>

            <Form.Item name="action" label="Review Action" rules={[{ required: true }]}>
              <Select placeholder="Select action">
                <Option value="Approve">Approve</Option>
                <Option value="Reject">Reject</Option>
                <Option value="RequestChanges">Request Changes</Option>
                <Option value="ScheduleABTest">Schedule A/B Test</Option>
              </Select>
            </Form.Item>

            <Form.Item name="comments" label="Comments">
              <TextArea rows={4} placeholder="Add review comments..." />
            </Form.Item>

            <Form.Item>
              <Button type="primary" htmlType="submit" block>
                Submit Review
              </Button>
            </Form.Item>
          </Form>
        )}
      </Modal>
    </div>
  );
};

const getTypeColor = (type: ImprovementType): string => {
  switch (type) {
    case 'PerformanceOptimization': return 'red';
    case 'ContentOptimization': return 'blue';
    case 'StructureImprovement': return 'green';
    case 'ExampleAddition': return 'orange';
    case 'InstructionClarification': return 'purple';
    case 'ContextEnhancement': return 'cyan';
    default: return 'default';
  }
};

const getStatusColor = (status: SuggestionStatus): string => {
  switch (status) {
    case 'Pending': return 'orange';
    case 'Approved': return 'green';
    case 'Rejected': return 'red';
    case 'Implemented': return 'blue';
    case 'NeedsChanges': return 'yellow';
    case 'ScheduledForTesting': return 'purple';
    default: return 'default';
  }
};
```

### Content Quality Analyzer Component

```typescript
import React, { useState } from 'react';
import { Card, Button, Input, Progress, List, Tag, Row, Col } from 'antd';
import { CheckCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { useAnalyzeContentQualityMutation } from '../api/templateImprovementApi';

const { TextArea } = Input;

export const ContentQualityAnalyzer: React.FC = () => {
  const [content, setContent] = useState('');
  const [analyzeQuality, { data: analysis, isLoading }] = useAnalyzeContentQualityMutation();

  const handleAnalyze = async () => {
    if (content.trim()) {
      await analyzeQuality({ templateContent: content });
    }
  };

  return (
    <div className="content-quality-analyzer">
      <Card title="Content Quality Analysis">
        <Row gutter={16}>
          <Col span={12}>
            <h4>Template Content</h4>
            <TextArea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="Enter template content to analyze..."
              rows={10}
              style={{ marginBottom: 16 }}
            />
            <Button
              type="primary"
              onClick={handleAnalyze}
              loading={isLoading}
              disabled={!content.trim()}
              block
            >
              Analyze Quality
            </Button>
          </Col>

          <Col span={12}>
            {analysis && (
              <div className="analysis-results">
                <h4>Quality Analysis Results</h4>

                {/* Overall Score */}
                <div className="overall-score mb-4">
                  <h5>Overall Quality Score</h5>
                  <Progress
                    percent={analysis.overallQualityScore * 100}
                    status={analysis.overallQualityScore > 0.7 ? 'success' : 'exception'}
                  />
                </div>

                {/* Quality Dimensions */}
                <div className="quality-dimensions mb-4">
                  <h5>Quality Dimensions</h5>
                  {Object.entries(analysis.qualityDimensions).map(([dimension, score]) => (
                    <div key={dimension} className="dimension-score">
                      <span>{dimension}:</span>
                      <Progress
                        percent={score * 100}
                        size="small"
                        style={{ marginLeft: 8, width: 100 }}
                      />
                    </div>
                  ))}
                </div>

                {/* Issues */}
                {analysis.identifiedIssues.length > 0 && (
                  <div className="quality-issues mb-4">
                    <h5>Identified Issues</h5>
                    <List
                      size="small"
                      dataSource={analysis.identifiedIssues}
                      renderItem={(issue) => (
                        <List.Item>
                          <List.Item.Meta
                            avatar={<ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />}
                            title={issue.issueType}
                            description={issue.description}
                          />
                          <Tag color="red">Severity: {(issue.severity * 100).toFixed(0)}%</Tag>
                        </List.Item>
                      )}
                    />
                  </div>
                )}

                {/* Strengths */}
                {analysis.strengths.length > 0 && (
                  <div className="quality-strengths mb-4">
                    <h5>Strengths</h5>
                    <List
                      size="small"
                      dataSource={analysis.strengths}
                      renderItem={(strength) => (
                        <List.Item>
                          <List.Item.Meta
                            avatar={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
                            title={strength.strengthType}
                            description={strength.description}
                          />
                          <Tag color="green">Impact: {(strength.impactScore * 100).toFixed(0)}%</Tag>
                        </List.Item>
                      )}
                    />
                  </div>
                )}

                {/* Improvement Suggestions */}
                {analysis.improvementSuggestions.length > 0 && (
                  <div className="improvement-suggestions">
                    <h5>Improvement Suggestions</h5>
                    <List
                      size="small"
                      dataSource={analysis.improvementSuggestions}
                      renderItem={(suggestion) => (
                        <List.Item>
                          <span>{suggestion}</span>
                        </List.Item>
                      )}
                    />
                  </div>
                )}
              </div>
            )}
          </Col>
        </Row>
      </Card>
    </div>
  );
};
```

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
