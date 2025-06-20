// services/api/performanceApi.ts
export const performanceApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    analyzePerformance: builder.query<PerformanceAnalysisResponse, {
      entityType: string
      entityId: string
    }>({
      query: ({ entityType, entityId }) => 
        `/performance/analyze/${entityType}/${entityId}`,
      providesTags: ['Performance'],
    }),

    identifyBottlenecks: builder.query<BottlenecksResponse, {
      entityType: string
      entityId: string
    }>({
      query: ({ entityType, entityId }) => 
        `/performance/bottlenecks/${entityType}/${entityId}`,
      providesTags: ['Performance'],
    }),

    getOptimizationSuggestions: builder.query<SuggestionsResponse, {
      entityType: string
      entityId: string
    }>({
      query: ({ entityType, entityId }) => 
        `/performance/suggestions/${entityType}/${entityId}`,
      providesTags: ['Performance'],
    }),

    autoTunePerformance: builder.mutation<TuningResultResponse, {
      entityType: string
      entityId: string
    }>({
      query: ({ entityType, entityId }) => ({
        url: `/performance/auto-tune/${entityType}/${entityId}`,
        method: 'POST',
      }),
      invalidatesTags: ['Performance'],
    }),

    getBenchmarks: builder.query<BenchmarksResponse, { category?: string }>({
      query: (params) => ({
        url: '/performance/benchmarks',
        params,
      }),
      providesTags: ['Benchmarks'],
    }),

    createBenchmark: builder.mutation<BenchmarkResponse, CreateBenchmarkRequest>({
      query: (benchmark) => ({
        url: '/performance/benchmarks',
        method: 'POST',
        body: benchmark,
      }),
      invalidatesTags: ['Benchmarks'],
    }),

    getPerformanceMetrics: builder.query<MetricsResponse, {
      metricName: string
      startDate?: string
      endDate?: string
    }>({
      query: ({ metricName, ...params }) => ({
        url: `/performance/metrics/${metricName}`,
        params,
      }),
      providesTags: ['PerformanceMetrics'],
    }),

    recordMetric: builder.mutation<void, PerformanceMetricsEntry>({
      query: (metric) => ({
        url: '/performance/metrics',
        method: 'POST',
        body: metric,
      }),
      invalidatesTags: ['PerformanceMetrics'],
    }),

    getActiveAlerts: builder.query<AlertsResponse, void>({
      query: () => '/performance/alerts',
      // Poll every minute for alerts
      pollingInterval: 60000,
    }),

    resolveAlert: builder.mutation<void, {
      alertId: string
      resolutionNotes: string
    }>({
      query: ({ alertId, resolutionNotes }) => ({
        url: `/performance/alerts/${alertId}/resolve`,
        method: 'POST',
        body: { resolutionNotes },
      }),
      invalidatesTags: ['Performance'],
    }),
  }),
})
