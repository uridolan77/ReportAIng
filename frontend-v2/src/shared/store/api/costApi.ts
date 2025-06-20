import { baseApi } from './baseApi'
import type {
  CostAnalyticsSummary,
  CostHistoryResponse,
  CostBreakdownResponse,
  CostTrendsResponse,
  BudgetsResponse,
  BudgetResponse,
  CreateBudgetRequest,
  UpdateBudgetRequest,
  CostPredictionRequest,
  CostPredictionResponse,
  CostForecastResponse,
  RecommendationsResponse,
  ROIAnalysisResponse,
  RealTimeMetricsResponse,
} from '../../types/cost'

export const costApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Analytics
    getCostAnalytics: builder.query<CostAnalyticsSummary, {
      startDate?: string
      endDate?: string
    }>({
      query: (params) => ({
        url: '/cost-management/analytics',
        params,
      }),
      providesTags: ['CostAnalytics'],
    }),

    getCostHistory: builder.query<CostHistoryResponse, {
      startDate?: string
      endDate?: string
      limit?: number
      page?: number
    }>({
      query: (params) => ({
        url: '/cost-management/history',
        params,
      }),
      providesTags: ['CostHistory'],
    }),

    getCostBreakdown: builder.query<CostBreakdownResponse, {
      dimension: string
      startDate?: string
      endDate?: string
    }>({
      query: ({ dimension, ...params }) => ({
        url: `/cost-management/breakdown/${dimension}`,
        params,
      }),
      providesTags: ['CostAnalytics'],
    }),

    getCostTrends: builder.query<CostTrendsResponse, {
      category?: string
      granularity?: string
      periods?: number
    }>({
      query: (params) => ({
        url: '/cost-management/trends',
        params,
      }),
      providesTags: ['CostAnalytics'],
    }),

    // Budgets
    getBudgets: builder.query<BudgetsResponse, void>({
      query: () => '/cost-management/budgets',
      providesTags: ['Budget'],
    }),

    createBudget: builder.mutation<BudgetResponse, CreateBudgetRequest>({
      query: (budget) => ({
        url: '/cost-management/budgets',
        method: 'POST',
        body: budget,
      }),
      invalidatesTags: ['Budget'],
    }),

    updateBudget: builder.mutation<BudgetResponse, {
      budgetId: string
      budget: UpdateBudgetRequest
    }>({
      query: ({ budgetId, budget }) => ({
        url: `/cost-management/budgets/${budgetId}`,
        method: 'PUT',
        body: budget,
      }),
      invalidatesTags: ['Budget'],
    }),

    deleteBudget: builder.mutation<void, string>({
      query: (budgetId) => ({
        url: `/cost-management/budgets/${budgetId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Budget'],
    }),

    // Predictions & Recommendations
    predictCost: builder.mutation<CostPredictionResponse, CostPredictionRequest>({
      query: (request) => ({
        url: '/cost-management/predict',
        method: 'POST',
        body: request,
      }),
    }),

    getCostForecast: builder.query<CostForecastResponse, { days?: number }>({
      query: (params) => ({
        url: '/cost-management/forecast',
        params,
      }),
    }),

    getOptimizationRecommendations: builder.query<RecommendationsResponse, void>({
      query: () => '/cost-management/recommendations',
      providesTags: ['CostRecommendations'],
    }),

    getROIAnalysis: builder.query<ROIAnalysisResponse, {
      startDate?: string
      endDate?: string
    }>({
      query: (params) => ({
        url: '/cost-management/roi',
        params,
      }),
    }),

    getRealTimeCostMetrics: builder.query<RealTimeMetricsResponse, void>({
      query: () => '/cost-management/realtime',
      // Polling every 30 seconds for real-time data
      pollingInterval: 30000,
    }),
  }),
})

export const {
  useGetCostAnalyticsQuery,
  useGetCostHistoryQuery,
  useGetCostBreakdownQuery,
  useGetCostTrendsQuery,
  useGetBudgetsQuery,
  useCreateBudgetMutation,
  useUpdateBudgetMutation,
  useDeleteBudgetMutation,
  usePredictCostMutation,
  useGetCostForecastQuery,
  useGetOptimizationRecommendationsQuery,
  useGetROIAnalysisQuery,
  useGetRealTimeCostMetricsQuery,
} = costApi
