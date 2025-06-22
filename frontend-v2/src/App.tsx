import React, { Suspense, useEffect } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useAppSelector } from '@shared/hooks'
import { selectIsAuthenticated } from '@shared/store/auth'
import { ErrorBoundary } from '@shared/components/ErrorBoundary'
import { LoadingSpinner } from '@shared/components/LoadingSpinner'
import { AuthGuard } from '@shared/components/AuthGuard'
import { initializeApiMode } from '@shared/components/core/ApiModeToggle'

// Lazy load applications
const ChatApp = React.lazy(() => import('@chat/ChatApp'))
const AdminApp = React.lazy(() => import('@admin/AdminApp'))
const LoginPage = React.lazy(() => import('@shared/pages/LoginPage'))

function App() {
  const isAuthenticated = useAppSelector(selectIsAuthenticated)

  // Initialize API mode on app start
  useEffect(() => {
    initializeApiMode()
  }, [])

  return (
    <ErrorBoundary>
      <div className="min-h-screen bg-secondary">
        <Suspense fallback={<LoadingSpinner />}>
          <Routes>
            {/* Authentication */}
            <Route path="/login" element={<LoginPage />} />
            
            {/* Protected Routes */}
            <Route
              path="/chat/*"
              element={
                <AuthGuard>
                  <ChatApp />
                </AuthGuard>
              }
            />
            <Route
              path="/admin/*"
              element={
                <AuthGuard requireAdmin>
                  <AdminApp />
                </AuthGuard>
              }
            />



            {/* Default redirects */}
            <Route
              path="/"
              element={
                isAuthenticated ? (
                  <Navigate to="/chat" replace />
                ) : (
                  <Navigate to="/login" replace />
                )
              }
            />
            
            {/* Catch all */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Suspense>
      </div>
    </ErrorBoundary>
  )
}

export default App
