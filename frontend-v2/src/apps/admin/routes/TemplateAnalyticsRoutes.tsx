import React, { lazy, Suspense } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { Spin } from 'antd'

// Lazy load components for code splitting
const TemplatePerformanceDashboard = lazy(() => 
  import('../pages/template-analytics/TemplatePerformanceDashboard')
)

const ABTestingDashboard = lazy(() =>
  import('../pages/template-analytics/ABTestingDashboard')
)

const TemplateManagementHub = lazy(() =>
  import('../pages/template-analytics/TemplateManagementHub')
)

const AdvancedAnalytics = lazy(() =>
  import('../pages/template-analytics/AdvancedAnalyticsDashboard')
)

const ReportsExport = lazy(() =>
  import('../pages/template-analytics/ReportsExportDashboard')
)

const EnhancedTemplateAnalytics = lazy(() =>
  import('../pages/template-analytics/EnhancedTemplateAnalytics')
)

const TemplateAnalyticsDemo = lazy(() =>
  import('../pages/template-analytics/TemplateAnalyticsDemo')
)

const LoadingSkeleton = () => (
  <div style={{ 
    display: 'flex', 
    justifyContent: 'center', 
    alignItems: 'center', 
    height: '400px' 
  }}>
    <Spin size="large" />
  </div>
)

export const TemplateAnalyticsRoutes: React.FC = () => {
  return (
    <Suspense fallback={<LoadingSkeleton />}>
      <Routes>
        <Route path="/enhanced" element={<EnhancedTemplateAnalytics />} />
        <Route path="/demo" element={<TemplateAnalyticsDemo />} />
        <Route path="/performance" element={<TemplatePerformanceDashboard />} />
        <Route path="/ab-testing" element={<ABTestingDashboard />} />
        <Route path="/management" element={<TemplateManagementHub />} />
        <Route path="/analytics" element={<AdvancedAnalytics />} />
        <Route path="/reports" element={<ReportsExport />} />
        <Route path="/" element={<Navigate to="/admin/template-analytics/enhanced" replace />} />
        <Route path="*" element={<Navigate to="/admin/template-analytics/enhanced" replace />} />
      </Routes>
    </Suspense>
  )
}

export default TemplateAnalyticsRoutes
