// features/cost-management/components/CostDashboard.tsx
export const CostDashboard: React.FC = () => {
  const { data: analytics } = useGetCostAnalyticsQuery({})
  const { data: realTimeMetrics } = useGetRealTimeCostMetricsQuery()
  const { data: trends } = useGetCostTrendsQuery({ periods: 30 })

  return (
    <Grid container spacing={3}>
      {/* Key Metrics Cards */}
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Total Cost (Month)"
          value={analytics?.monthlyCost}
          format="currency"
          trend={calculateTrend(trends)}
        />
      </Grid>
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Daily Average"
          value={analytics?.dailyCost}
          format="currency"
        />
      </Grid>
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Cost Efficiency"
          value={analytics?.costEfficiency}
          format="percentage"
        />
      </Grid>
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Potential Savings"
          value={analytics?.costSavingsOpportunities}
          format="currency"
        />
      </Grid>

      {/* Cost Trends Chart */}
      <Grid item xs={12} md={8}>
        <CostTrendsChart data={trends} />
      </Grid>

      {/* Cost Breakdown */}
      <Grid item xs={12} md={4}>
        <CostBreakdownChart data={analytics?.costByProvider} />
      </Grid>

      {/* Budget Status */}
      <Grid item xs={12} md={6}>
        <BudgetStatusWidget />
      </Grid>

      {/* Recent Recommendations */}
      <Grid item xs={12} md={6}>
        <RecommendationsWidget />
      </Grid>
    </Grid>
  )
}
