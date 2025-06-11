import React, { Suspense, lazy, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import { ErrorBoundary } from './components/ErrorBoundary/ErrorBoundary';
import { Login } from './components/Auth/Login';
import { useAuthStore } from './stores/authStore';
import { ErrorService } from './services/errorService';
import { DevTools } from './components/DevTools/DevTools';
import { ReactQueryProvider } from './components/Providers/ReactQueryProvider';
import { StateSyncProvider } from './components/Providers/StateSyncProvider';
import { ThemeProvider, useTheme } from './contexts/ThemeContext';
import { secureApiClient } from './services/secureApiClient';
// Import new modern UI components
import {
  LoadingFallback
} from './components/core';
import { PerformanceMonitor, BundleAnalyzer } from './components/ui';
import EnhancedSidebar from './components/Layout/EnhancedSidebar';
import { CornerStatusPanel } from './components/Layout/AppHeader';
import './App.css';

// Lazy load pages and features - Modern page components
const QueryInterface = lazy(() => import('./pages/QueryPage'));
const ResultsPage = lazy(() => import('./pages/ResultsPage'));
const HistoryPage = lazy(() => import('./pages/HistoryPage'));
const TemplatesPage = lazy(() => import('./pages/TemplatesPage'));
const SuggestionsPage = lazy(() => import('./pages/SuggestionsPage'));
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const VisualizationPage = lazy(() => import('./pages/VisualizationPage'));
const DBExplorerPage = lazy(() => import('./pages/DBExplorerPage'));

// Admin pages
const TuningPage = lazy(() => import('./pages/admin/TuningPage'));
const LLMManagementPage = lazy(() => import('./pages/admin/LLMManagementPage'));
const LLMTestPage = lazy(() => import('./pages/admin/LLMTestPage'));
const LLMDebugPage = lazy(() => import('./pages/admin/LLMDebugPage'));
const SchemaManagementPage = lazy(() => import('./components/SchemaManagement/SchemaManagementDashboard').then(module => ({ default: module.SchemaManagementDashboard })));
const CacheManagementPage = lazy(() => import('./components/Cache/CacheManager').then(module => ({ default: module.CacheManager })));
const SecurityPage = lazy(() => import('./components/Security/SecurityDashboard').then(module => ({ default: module.SecurityDashboard })));
const SuggestionsManagementPage = lazy(() => import('./components/Admin/QuerySuggestionsManager').then(module => ({ default: module.QuerySuggestionsManager })));

// Demo and development pages
const UIDemo = lazy(() => import('./pages/EnhancedUIDemo'));
const PerformancePage = lazy(() => import('./components/Performance/PerformanceMonitoringDashboard').then(module => ({ default: module.default })));

const App: React.FC = () => {
  const { isAuthenticated, isAdmin } = useAuthStore();

  // Initialize services
  useEffect(() => {
    ErrorService.initialize();
    secureApiClient.updateConfig({
      enableSigning: true,
      enableEncryption: false,
      retryAttempts: 3,
      rateLimitConfig: {
        maxRequests: 100,
        windowMs: 60000,
      },
    });
  }, []);

  return (
    <PerformanceMonitor
      onMetrics={(metrics) => {
        if (process.env.NODE_ENV === 'development') {
          console.log('App performance metrics:', metrics);
        }
      }}
    >
      <BundleAnalyzer
        onAnalysis={(analysis) => {
          if (process.env.NODE_ENV === 'development') {
            console.log('Bundle analysis:', analysis);
          }
        }}
      />
      <ThemeProvider>
        <ReactQueryProvider>
          <StateSyncProvider>
            <ErrorBoundary>
              <AppWithTheme isAuthenticated={isAuthenticated} isAdmin={isAdmin} />
            </ErrorBoundary>
          </StateSyncProvider>
        </ReactQueryProvider>
      </ThemeProvider>
    </PerformanceMonitor>
  );
};

// Separate component that uses the theme context
const AppWithTheme: React.FC<{ isAuthenticated: boolean; isAdmin: boolean }> = ({
  isAuthenticated,
  isAdmin
}) => {
  const { antdTheme } = useTheme();
  const [sidebarCollapsed, setSidebarCollapsed] = React.useState(false);

  return (
    <ConfigProvider theme={antdTheme}>
      <Router>
        <div className="App">
          {isAuthenticated ? (
            <Suspense fallback={<LoadingFallback />}>
              <CornerStatusPanel />
              <div style={{ display: 'flex', minHeight: '100vh' }}>
                <EnhancedSidebar
                  collapsed={sidebarCollapsed}
                  onCollapse={setSidebarCollapsed}
                />
                <div style={{
                  flex: 1,
                  backgroundColor: '#f5f5f5',
                  padding: '24px',
                  paddingTop: '80px' // Space for corner panel
                }}>
                  <Routes>
                  {/* Main Interface */}
                  <Route path="/" element={<QueryInterface />} />

                  {/* Analytics & Visualization */}
                  <Route path="/results" element={<ResultsPage />} />
                  <Route path="/dashboard" element={<DashboardPage />} />
                  <Route path="/visualization" element={<VisualizationPage />} />

                  {/* Query Tools */}
                  <Route path="/history" element={<HistoryPage />} />
                  <Route path="/templates" element={<TemplatesPage />} />
                  <Route path="/suggestions" element={<SuggestionsPage />} />

                  {/* System Tools */}
                  <Route path="/db-explorer" element={<DBExplorerPage />} />
                  <Route path="/performance" element={<PerformancePage />} />

                  {/* Admin Routes */}
                  {isAdmin && (
                    <>
                      <Route path="/admin/tuning" element={<TuningPage />} />
                      <Route path="/admin/llm" element={<LLMManagementPage />} />
                      <Route path="/admin/llm-test" element={<LLMTestPage />} />
                      <Route path="/admin/llm-debug" element={<LLMDebugPage />} />
                      <Route path="/admin/schemas" element={<SchemaManagementPage />} />
                      <Route path="/admin/cache" element={<CacheManagementPage />} />
                      <Route path="/admin/security" element={<SecurityPage />} />
                      <Route path="/admin/suggestions" element={<SuggestionsManagementPage />} />
                      <Route path="/ui-demo" element={<UIDemo />} />
                    </>
                  )}

                  {/* Legacy redirects */}
                  <Route path="/interactive" element={<Navigate to="/visualization" replace />} />
                  <Route path="/advanced-viz" element={<Navigate to="/visualization" replace />} />
                  <Route path="/query" element={<Navigate to="/" replace />} />
                  <Route path="/security" element={<Navigate to="/admin/security" replace />} />

                  {/* Catch all */}
                  <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
                </div>
              </div>
            </Suspense>
          ) : (
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
          )}
          {process.env.NODE_ENV === 'development' && <DevTools />}
        </div>
      </Router>
    </ConfigProvider>
  );
};

export default App;
