// features/performance/components/PerformanceDashboard.tsx
export const PerformanceDashboard: React.FC = () => {
  const [selectedEntity, setSelectedEntity] = useState({ type: 'system', id: 'main' })
  
  const { data: metrics } = useAnalyzePerformanceQuery(selectedEntity)
  const { data: bottlenecks } = useIdentifyBottlenecksQuery(selectedEntity)
  const { data: suggestions } = useGetOptimizationSuggestionsQuery(selectedEntity)
  const { data: alerts } = useGetActiveAlertsQuery()

  return (
    <Grid container spacing={3}>
      {/* Entity Selector */}
      <Grid item xs={12}>
        <EntitySelector 
          value={selectedEntity}
          onChange={setSelectedEntity}
        />
      </Grid>

      {/* Performance Metrics */}
      <Grid item xs={12} md={8}>
        <PerformanceMetricsChart data={metrics} />
      </Grid>

      {/* Performance Score */}
      <Grid item xs={12} md={4}>
        <PerformanceScoreGauge score={metrics?.performanceScore} />
      </Grid>

      {/* Bottlenecks */}
      <Grid item xs={12} md={6}>
        <BottlenecksWidget bottlenecks={bottlenecks} />
      </Grid>

      {/* Optimization Suggestions */}
      <Grid item xs={12} md={6}>
        <OptimizationSuggestionsWidget suggestions={suggestions} />
      </Grid>

      {/* Active Alerts */}
      <Grid item xs={12}>
        <AlertsTable alerts={alerts} />
      </Grid>
    </Grid>
  )
}
