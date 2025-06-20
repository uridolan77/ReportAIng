// Performance Optimization Examples

// Code Splitting
const CostDashboard = lazy(() => import('../features/cost-management/components/CostDashboard'))
const PerformanceDashboard = lazy(() => import('../features/performance/components/PerformanceDashboard'))

// Use Suspense for loading states
<Suspense fallback={<DashboardLoading />}>
  <CostDashboard />
</Suspense>

// Memoization
const expensiveMetrics = useMemo(() => {
  return calculateComplexMetrics(rawData)
}, [rawData])

// Memoize components
export const CostTrendsChart = memo<CostTrendsChartProps>(({ data }) => {
  // Component implementation
})

// Accessibility Examples

// ARIA Labels and Roles
<ResponsiveContainer 
  width="100%" 
  height={300}
  role="img"
  aria-label="Cost trends over time showing daily spending patterns"
>
  <LineChart data={chartData}>
    {/* Chart content */}
  </LineChart>
</ResponsiveContainer>

// Keyboard Navigation
<Button
  onClick={handleAction}
  onKeyDown={(e) => {
    if (e.key === 'Enter' || e.key === ' ') {
      handleAction()
    }
  }}
  aria-label="Create new budget"
>
  Create Budget
</Button>
