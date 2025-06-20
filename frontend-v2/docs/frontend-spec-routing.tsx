// App.tsx routing setup
import { Routes, Route } from 'react-router-dom'

const App: React.FC = () => {
  return (
    <Routes>
      {/* Cost Management Routes */}
      <Route path="/cost" element={<CostLayout />}>
        <Route index element={<CostDashboard />} />
        <Route path="analytics" element={<CostAnalytics />} />
        <Route path="budgets" element={<BudgetManagement />} />
        <Route path="forecasting" element={<CostForecasting />} />
        <Route path="recommendations" element={<CostRecommendations />} />
      </Route>

      {/* Performance Routes */}
      <Route path="/performance" element={<PerformanceLayout />}>
        <Route index element={<PerformanceDashboard />} />
        <Route path="analysis" element={<PerformanceAnalysis />} />
        <Route path="benchmarks" element={<BenchmarkManagement />} />
        <Route path="optimization" element={<PerformanceOptimization />} />
      </Route>

      {/* Cache Management Routes */}
      <Route path="/cache" element={<CacheLayout />}>
        <Route index element={<CacheDashboard />} />
        <Route path="statistics" element={<CacheStatistics />} />
        <Route path="configuration" element={<CacheConfiguration />} />
        <Route path="optimization" element={<CacheOptimization />} />
      </Route>

      {/* Resource Management Routes */}
      <Route path="/resources" element={<ResourceLayout />}>
        <Route index element={<ResourceDashboard />} />
        <Route path="quotas" element={<ResourceQuotas />} />
        <Route path="monitoring" element={<ResourceMonitoring />} />
        <Route path="load-balancing" element={<LoadBalancing />} />
      </Route>

      {/* Model Selection Routes */}
      <Route path="/models" element={<ModelLayout />}>
        <Route index element={<ModelDashboard />} />
        <Route path="selection" element={<ModelSelection />} />
        <Route path="performance" element={<ModelPerformance />} />
        <Route path="comparison" element={<ModelComparison />} />
      </Route>
    </Routes>
  )
}
