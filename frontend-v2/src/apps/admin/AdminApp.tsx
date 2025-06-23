import { FC } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '@shared/components/core/Layout'
import Dashboard from './pages/Dashboard'
import BusinessMetadata from './pages/BusinessMetadata'
import EnhancedBusinessMetadata from './pages/EnhancedBusinessMetadata'
import BusinessTableEditPage from './pages/BusinessTableEditPage'
import SystemConfiguration from './pages/SystemConfiguration'
import UserManagement from './pages/UserManagement'
import Analytics from './pages/Analytics'
import CostManagement from './pages/CostManagement'
import PerformanceMonitoring from './pages/PerformanceMonitoring'
import PerformanceCostManagement from './pages/PerformanceCostManagement'
import AIAnalyticsDashboard from './pages/AIAnalyticsDashboard'
import LLMManagementDashboard from './pages/LLMManagementDashboard'
import AITransparencyDashboard from './pages/AITransparencyDashboard'
import AITransparencyAnalysisPage from './pages/AITransparencyAnalysisPage'
import BusinessIntelligencePage from './pages/BusinessIntelligencePage'
import AIManagementDemo from './pages/AIManagementDemo'
import AdvancedAIFeaturesDemo from './pages/AdvancedAIFeaturesDemo'
import AIIntegrationTestPage from './pages/AIIntegrationTestPage'
import TransparencyDashboardPage from './pages/TransparencyDashboardPage'
import TransparencyManagementPage from './pages/TransparencyManagementPage'
import TransparencyReviewPage from './pages/TransparencyReviewPage'
import TemplateAnalyticsRoutes from './routes/TemplateAnalyticsRoutes'

// New consolidated pages
import ComprehensiveAdminDashboard from './pages/ComprehensiveAdminDashboard'
import ComprehensiveAITransparency from './pages/ComprehensiveAITransparency'

export default function AdminApp() {
  return (
    <AppLayout>
      <Routes>
        {/* Consolidated Admin Dashboard - combines dashboard, analytics, and reports */}
        <Route path="/" element={<ComprehensiveAdminDashboard />} />
        <Route path="/dashboard" element={<ComprehensiveAdminDashboard />} />

        {/* Business Intelligence - enhanced business metadata management */}
        <Route path="/business-intelligence" element={<BusinessIntelligencePage />} />
        <Route path="/business-metadata" element={<BusinessMetadata />} />
        <Route path="/business-metadata/edit/:tableId" element={<BusinessTableEditPage />} />
        <Route path="/business-metadata/add" element={<BusinessTableEditPage />} />
        <Route path="/business-metadata/view/:tableId" element={<BusinessTableEditPage />} />

        {/* Legacy enhanced route - redirect to main page */}
        <Route path="/business-metadata-enhanced" element={<BusinessMetadata />} />

        {/* System configuration */}
        <Route path="/system-config" element={<SystemConfiguration />} />

        {/* User management */}
        <Route path="/users" element={<UserManagement />} />

        {/* Legacy analytics route - redirect to consolidated dashboard */}
        <Route path="/analytics" element={<Navigate to="/admin/dashboard" replace />} />

        {/* Unified Performance & Cost Management */}
        <Route path="/performance-cost" element={<PerformanceCostManagement />} />

        {/* Legacy routes - redirect to unified page */}
        <Route path="/cost-management" element={<Navigate to="/admin/performance-cost" replace />} />
        <Route path="/performance" element={<Navigate to="/admin/performance-cost" replace />} />

        {/* AI Analytics and Management */}
        <Route path="/ai-analytics" element={<AIAnalyticsDashboard />} />
        <Route path="/llm-management" element={<LLMManagementDashboard />} />

        {/* Consolidated AI Transparency - combines dashboard, management, review, and analysis */}
        <Route path="/ai-transparency" element={<ComprehensiveAITransparency />} />

        {/* Legacy transparency routes - redirect to consolidated page */}
        <Route path="/ai-transparency-analysis" element={<Navigate to="/admin/ai-transparency" replace />} />
        <Route path="/transparency-dashboard" element={<Navigate to="/admin/ai-transparency" replace />} />
        <Route path="/transparency-management" element={<Navigate to="/admin/ai-transparency" replace />} />
        <Route path="/transparency-review" element={<Navigate to="/admin/ai-transparency" replace />} />

        {/* Legacy demo routes - redirect to main features */}
        <Route path="/ai-management-demo" element={<Navigate to="/admin/llm-management" replace />} />
        <Route path="/advanced-ai-features-demo" element={<Navigate to="/admin/ai-analytics" replace />} />
        <Route path="/ai-integration-test" element={<Navigate to="/admin/ai-transparency" replace />} />

        {/* Template Analytics */}
        <Route path="/template-analytics/*" element={<TemplateAnalyticsRoutes />} />

        {/* Default redirect */}
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Routes>
    </AppLayout>
  )
}
