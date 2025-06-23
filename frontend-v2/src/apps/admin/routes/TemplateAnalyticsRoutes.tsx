import React, { lazy, Suspense } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { Spin } from 'antd'

// Lazy load components for code splitting
const TemplateAnalytics = lazy(() =>
  import('../pages/template-analytics/TemplateAnalytics')
)

const TemplateManagementComplete = lazy(() =>
  import('../pages/template-analytics/TemplateManagementComplete')
)

// Legacy components for redirects
const TemplatePerformanceDashboard = lazy(() =>
  import('../pages/template-analytics/TemplatePerformanceDashboard')
)

const ABTestingDashboard = lazy(() =>
  import('../pages/template-analytics/ABTestingDashboard')
)

const TemplateManagementHub = lazy(() =>
  import('../pages/template-analytics/TemplateManagementHub')
)

const EnhancedTemplateAnalytics = lazy(() =>
  import('../pages/template-analytics/EnhancedTemplateAnalytics')
)

const TemplateFeatures = lazy(() =>
  import('../pages/template-analytics/TemplateFeatures')
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
        {/* Main consolidated routes */}
        <Route path="/analytics" element={<TemplateAnalytics />} />
        <Route path="/management" element={<TemplateManagementComplete />} />

        {/* Legacy route redirects */}
        <Route path="/enhanced" element={<Navigate to="/admin/template-analytics/analytics" replace />} />
        <Route path="/demo" element={<Navigate to="/admin/template-analytics/management" replace />} />
        <Route path="/features" element={<Navigate to="/admin/template-analytics/management" replace />} />
        <Route path="/performance" element={<Navigate to="/admin/template-analytics/analytics" replace />} />
        <Route path="/ab-testing" element={<Navigate to="/admin/template-analytics/analytics" replace />} />
        <Route path="/reports" element={<Navigate to="/admin/template-analytics/analytics" replace />} />

        <Route path="/" element={<Navigate to="/admin/template-analytics/analytics" replace />} />
        <Route path="*" element={<Navigate to="/admin/template-analytics/analytics" replace />} />
      </Routes>
    </Suspense>
  )
}

export default TemplateAnalyticsRoutes
