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

        {/* Default redirect */}
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Routes>
    </AppLayout>
  )
}
