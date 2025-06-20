import React from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '@shared/components/core/Layout'
import Dashboard from './pages/Dashboard'
import BusinessMetadata from './pages/BusinessMetadata'
import SystemConfiguration from './pages/SystemConfiguration'
import UserManagement from './pages/UserManagement'
import Analytics from './pages/Analytics'

export default function AdminApp() {
  return (
    <AppLayout>
      <Routes>
        {/* Main admin dashboard */}
        <Route path="/" element={<Dashboard />} />
        
        {/* Business metadata management */}
        <Route path="/business-metadata" element={<BusinessMetadata />} />
        
        {/* System configuration */}
        <Route path="/system-config" element={<SystemConfiguration />} />
        
        {/* User management */}
        <Route path="/users" element={<UserManagement />} />
        
        {/* Analytics and monitoring */}
        <Route path="/analytics" element={<Analytics />} />
        
        {/* Default redirect */}
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Routes>
    </AppLayout>
  )
}
