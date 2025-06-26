import { baseApi } from './baseApi'
import type {
  PerformanceDashboardData,
  TemplatePerformanceMetrics,
  PerformanceAlert,
  PerformanceTrend,
  ABTestDetails,
  ABTestRequest,
  ABTestResult,
  ABTestAnalysis,
  ABTestRecommendation,
  TemplateWithMetrics,
  TemplateManagementDashboard,
  CreateTemplateRequest,
  UpdateTemplateRequest,
  TemplateSearchCriteria,
  TemplateSearchResult,
  UserFeedback,
  ExportRequest,
  // Template Improvement Types
  TemplateImprovementSuggestion,
  OptimizedTemplate,
  PerformancePrediction,
  TemplateVariant,
  ContentQualityAnalysis,
  ReviewResult,
  OptimizationStrategy,
  SuggestionReviewAction,
  // Comprehensive Analytics Types
  ComprehensiveAnalyticsDashboard,
  PerformanceTrendsData,
  UsageInsightsData,
  QualityMetricsData,
  RealTimeAnalyticsData,
  AnalyticsExportConfig,
} from '@shared/types/templateAnalytics'

export const templateAnalyticsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Comprehensive Analytics Dashboard Endpoints
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

    getQualityMetrics: builder.query<QualityMetricsData, {
      intentType?: string;
    }>({
      query: ({ intentType }) => ({
        url: 'templateanalytics/metrics/quality',
        params: { intentType },
      }),
      providesTags: ['Analytics'],
    }),

    getRealTimeAnalytics: builder.query<RealTimeAnalyticsData, void>({
      query: () => 'templateanalytics/realtime',
      providesTags: ['Analytics'],
    }),

    exportAnalytics: builder.mutation<Blob, AnalyticsExportConfig>({
      query: (config) => ({
        url: 'templateanalytics/export',
        method: 'POST',
        body: config,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Performance Dashboard Endpoints
    getPerformanceDashboard: builder.query<PerformanceDashboardData, {
      startDate?: string;
      endDate?: string;
      intentType?: string;
    }>({
      query: ({ startDate, endDate, intentType }) => ({
        url: 'templateanalytics/dashboard',
        params: { startDate, endDate, intentType },
      }),
      providesTags: ['TemplatePerformance'],
    }),

    getTemplatePerformance: builder.query<TemplatePerformanceMetrics, string>({
      query: (templateKey) => `templateanalytics/performance/${templateKey}`,
      providesTags: (result, error, templateKey) => [
        { type: 'TemplatePerformance', id: templateKey },
      ],
    }),

    getTopPerformingTemplates: builder.query<TemplatePerformanceMetrics[], {
      intentType?: string;
      count?: number;
      timeRange?: { start: string; end: string };
    }>({
      query: ({ intentType, count = 10, timeRange }) => ({
        url: 'templateanalytics/performance/top',
        params: { 
          intentType, 
          count,
          startDate: timeRange?.start,
          endDate: timeRange?.end,
        },
      }),
      providesTags: ['TemplatePerformance'],
    }),

    getPerformanceAlerts: builder.query<PerformanceAlert[], {
      severity?: string;
      resolved?: boolean;
    }>({
      query: ({ severity, resolved }) => ({
        url: 'templateanalytics/alerts',
        params: { severity, resolved },
      }),
      providesTags: ['PerformanceAlert'],
    }),

    getTemplateTrends: builder.query<PerformanceTrend[], {
      templateKey?: string;
      startDate: string;
      endDate: string;
      granularity?: 'hour' | 'day' | 'week';
    }>({
      query: ({ templateKey, startDate, endDate, granularity = 'day' }) => ({
        url: 'templateanalytics/trends',
        params: { templateKey, startDate, endDate, granularity },
      }),
      providesTags: ['PerformanceTrend'],
    }),

    resolvePerformanceAlert: builder.mutation<void, number>({
      query: (alertId) => ({
        url: `templateanalytics/alerts/${alertId}/resolve`,
        method: 'POST',
      }),
      invalidatesTags: ['PerformanceAlert'],
    }),

    // A/B Testing Endpoints
    getABTests: builder.query<ABTestDetails[], { 
      status?: 'running' | 'paused' | 'completed' | 'cancelled';
      page?: number;
      pageSize?: number;
    }>({
      query: ({ status, page = 1, pageSize = 10 }) => ({
        url: 'templateanalytics/abtests',
        params: { status, page, pageSize },
      }),
      providesTags: ['ABTest'],
    }),

    getABTest: builder.query<ABTestDetails, number>({
      query: (testId) => `templateanalytics/abtests/${testId}`,
      providesTags: (result, error, testId) => [
        { type: 'ABTest', id: testId },
      ],
    }),

    createABTest: builder.mutation<ABTestResult, ABTestRequest>({
      query: (request) => ({
        url: 'templateanalytics/abtests',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['ABTest'],
    }),

    updateABTest: builder.mutation<void, { 
      testId: number; 
      updates: Partial<ABTestRequest> 
    }>({
      query: ({ testId, updates }) => ({
        url: `templateanalytics/abtests/${testId}`,
        method: 'PATCH',
        body: updates,
      }),
      invalidatesTags: (result, error, { testId }) => [
        { type: 'ABTest', id: testId },
        'ABTest',
      ],
    }),

    completeABTest: builder.mutation<void, { 
      testId: number; 
      implementWinner?: boolean;
      notes?: string;
    }>({
      query: ({ testId, implementWinner = true, notes }) => ({
        url: `templateanalytics/abtests/${testId}/complete`,
        method: 'POST',
        body: { implementWinner, notes },
      }),
      invalidatesTags: ['ABTest', 'TemplateManagement'],
    }),

    pauseABTest: builder.mutation<void, number>({
      query: (testId) => ({
        url: `templateanalytics/abtests/${testId}/pause`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, testId) => [
        { type: 'ABTest', id: testId },
      ],
    }),

    resumeABTest: builder.mutation<void, number>({
      query: (testId) => ({
        url: `templateanalytics/abtests/${testId}/resume`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, testId) => [
        { type: 'ABTest', id: testId },
      ],
    }),

    getABTestAnalysis: builder.query<ABTestAnalysis, number>({
      query: (testId) => `templateanalytics/abtests/${testId}/analysis`,
      providesTags: (result, error, testId) => [
        { type: 'ABTestAnalysis', id: testId },
      ],
    }),

    getABTestRecommendations: builder.query<ABTestRecommendation[], {
      templateKey?: string;
      intentType?: string;
    }>({
      query: ({ templateKey, intentType }) => ({
        url: 'templateanalytics/abtests/recommendations',
        params: { templateKey, intentType },
      }),
      providesTags: ['ABTestRecommendation'],
    }),

    // Template Management Endpoints
    getTemplateManagementDashboard: builder.query<TemplateManagementDashboard, void>({
      query: () => 'templateanalytics/templates/dashboard',
      providesTags: ['TemplateManagement'],
    }),

    getTemplateWithMetrics: builder.query<TemplateWithMetrics, string>({
      query: (templateKey) => `templateanalytics/templates/${templateKey}`,
      providesTags: (result, error, templateKey) => [
        { type: 'TemplateManagement', id: templateKey },
      ],
    }),

    searchTemplates: builder.query<TemplateSearchResult, {
      criteria: TemplateSearchCriteria;
      page?: number;
      pageSize?: number;
    }>({
      query: ({ criteria, page = 1, pageSize = 20 }) => ({
        url: 'templateanalytics/templates/search',
        method: 'POST',
        body: { ...criteria, page, pageSize },
      }),
      providesTags: ['TemplateManagement'],
    }),

    createTemplate: builder.mutation<TemplateWithMetrics, CreateTemplateRequest>({
      query: (request) => ({
        url: 'templateanalytics/templates',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['TemplateManagement'],
    }),

    updateTemplate: builder.mutation<TemplateWithMetrics, {
      templateKey: string;
      updates: UpdateTemplateRequest;
    }>({
      query: ({ templateKey, updates }) => ({
        url: `templateanalytics/templates/${templateKey}`,
        method: 'PATCH',
        body: updates,
      }),
      invalidatesTags: (result, error, { templateKey }) => [
        { type: 'TemplateManagement', id: templateKey },
        'TemplateManagement',
      ],
    }),

    deleteTemplate: builder.mutation<void, string>({
      query: (templateKey) => ({
        url: `templateanalytics/templates/${templateKey}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['TemplateManagement'],
    }),

    // User Feedback Endpoints
    trackUserFeedback: builder.mutation<void, UserFeedback>({
      query: (feedback) => ({
        url: 'templateanalytics/feedback',
        method: 'POST',
        body: feedback,
      }),
    }),

    // Export Endpoints
    exportData: builder.mutation<Blob, ExportRequest>({
      query: (request) => ({
        url: 'templateanalytics/export',
        method: 'POST',
        body: request,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Template Improvement Endpoints
    analyzeTemplatePerformance: builder.mutation<TemplateImprovementSuggestion[], string>({
      query: (templateKey) => ({
        url: `templateimprovement/analyze/${templateKey}`,
        method: 'POST',
      }),
      invalidatesTags: ['TemplateImprovement'],
    }),

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

    analyzeContentQuality: builder.mutation<ContentQualityAnalysis, {
      templateContent: string;
    }>({
      query: (params) => ({
        url: 'templateimprovement/analyze-content',
        method: 'POST',
        body: params,
      }),
    }),

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
  }),
})

// Export hooks for use in components
export const {
  // Comprehensive Analytics
  useGetComprehensiveDashboardQuery,
  useGetPerformanceTrendsQuery,
  useGetUsageInsightsQuery,
  useGetQualityMetricsQuery,
  useGetRealTimeAnalyticsQuery,
  useExportAnalyticsMutation,

  // Performance Dashboard
  useGetPerformanceDashboardQuery,
  useGetTemplatePerformanceQuery,
  useGetTopPerformingTemplatesQuery,
  useGetTemplateTrendsQuery,
  useGetPerformanceAlertsQuery,
  useResolvePerformanceAlertMutation,

  // A/B Testing
  useGetABTestsQuery,
  useGetABTestQuery,
  useCreateABTestMutation,
  useUpdateABTestMutation,
  useCompleteABTestMutation,
  usePauseABTestMutation,
  useResumeABTestMutation,
  useGetABTestAnalysisQuery,
  useGetABTestRecommendationsQuery,

  // Template Management
  useGetTemplateManagementDashboardQuery,
  useGetTemplateWithMetricsQuery,
  useSearchTemplatesQuery,
  useCreateTemplateMutation,
  useUpdateTemplateMutation,
  useDeleteTemplateMutation,

  // User Feedback & Export
  useTrackUserFeedbackMutation,
  useExportDataMutation,

  // Template Improvement
  useAnalyzeTemplatePerformanceMutation,
  useGenerateImprovementSuggestionsMutation,
  useOptimizeTemplateMutation,
  usePredictTemplatePerformanceMutation,
  useGenerateTemplateVariantsMutation,
  useReviewImprovementSuggestionMutation,
  useAnalyzeContentQualityMutation,
  useExportImprovementSuggestionsMutation,
} = templateAnalyticsApi
