import { FC } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '@shared/components/core/Layout'
import Dashboard from './pages/Dashboard'
import BusinessMetadata from './pages/BusinessMetadata'
import BusinessTableEditPage from './pages/BusinessTableEditPage'
import SystemConfiguration from './pages/SystemConfiguration'
import UserManagement from './pages/UserManagement'
import Analytics from './pages/Analytics'
import CostManagement from './pages/CostManagement'
import PerformanceMonitoring from './pages/PerformanceMonitoring'
import AIAnalyticsDashboard from './pages/AIAnalyticsDashboard'
import LLMManagementDashboard from './pages/LLMManagementDashboard'
import AITransparencyDashboard from './pages/AITransparencyDashboard'
import AITransparencyDemo from './pages/AITransparencyDemo'
import BusinessIntelligenceDemo from './pages/BusinessIntelligenceDemo'
import AIManagementDemo from './pages/AIManagementDemo'
import AdvancedAIFeaturesDemo from './pages/AdvancedAIFeaturesDemo'
import AIIntegrationTestPage from './pages/AIIntegrationTestPage'
import TransparencyManagementPage from './pages/TransparencyManagementPage'
import TransparencyReviewPage from './pages/TransparencyReviewPage'

export default function AdminApp() {
  return (
    <AppLayout>
      <Routes>
        {/* Main admin dashboard */}
        <Route path="/" element={<Dashboard />} />
        
        {/* Business metadata management */}
        <Route path="/business-metadata" element={<BusinessMetadata />} />
        <Route path="/business-metadata/edit/:tableId" element={<BusinessTableEditPage />} />
        <Route path="/business-metadata/add" element={<BusinessTableEditPage />} />
        
        {/* System configuration */}
        <Route path="/system-config" element={<SystemConfiguration />} />
        
        {/* User management */}
        <Route path="/users" element={<UserManagement />} />
        
        {/* Analytics and monitoring */}
        <Route path="/analytics" element={<Analytics />} />

        {/* Cost management */}
        <Route path="/cost-management" element={<CostManagement />} />

        {/* Performance monitoring */}
        <Route path="/performance" element={<PerformanceMonitoring />} />

        {/* AI Analytics and Management */}
        <Route path="/ai-analytics" element={<AIAnalyticsDashboard />} />
        <Route path="/llm-management" element={<LLMManagementDashboard />} />
        <Route path="/ai-transparency" element={<AITransparencyDashboard />} />
        <Route path="/ai-transparency-demo" element={<AITransparencyDemo />} />
        <Route path="/business-intelligence-demo" element={<BusinessIntelligenceDemo />} />
        <Route path="/ai-management-demo" element={<AIManagementDemo />} />
        <Route path="/advanced-ai-features-demo" element={<AdvancedAIFeaturesDemo />} />
        <Route path="/ai-integration-test" element={<AIIntegrationTestPage />} />

        {/* New Comprehensive Transparency Pages */}
        <Route path="/transparency-management" element={<TransparencyManagementPage />} />
        <Route path="/transparency-review" element={<TransparencyReviewPage />} />

        {/* Default redirect */}
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Routes>
    </AppLayout>
  )
}
