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
} from '@shared/types/templateAnalytics'

export const templateAnalyticsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
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

    getPerformanceTrends: builder.query<PerformanceTrend[], {
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
  }),
})

// Export hooks for use in components
export const {
  // Performance Dashboard
  useGetPerformanceDashboardQuery,
  useGetTemplatePerformanceQuery,
  useGetTopPerformingTemplatesQuery,
  useGetPerformanceAlertsQuery,
  useGetPerformanceTrendsQuery,
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
} = templateAnalyticsApi
