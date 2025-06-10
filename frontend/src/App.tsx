import React, { Suspense, lazy, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, theme } from 'antd';
import { ErrorBoundary } from './components/ErrorBoundary/ErrorBoundary';
import { Layout } from './components/Layout/Layout';
import { Login } from './components/Auth/Login';
import { useAuthStore } from './stores/authStore';
import { ErrorService } from './services/errorService';
import { DevTools } from './components/DevTools/DevTools';
import { ReactQueryProvider } from './components/Providers/ReactQueryProvider';
import { StateSyncProvider } from './components/Providers/StateSyncProvider';
import { secureApiClient } from './services/secureApiClient';
// Import new UI components
import {
  LoadingFallback,
  PerformanceMonitor,
  BundleAnalyzer
} from './components/ui';
import './App.css';

// Lazy load heavy components
const QueryInterface = lazy(() => import('./components/QueryInterface/QueryInterface'));
const ResultsPage = lazy(() => import('./pages/ResultsPage'));
const HistoryPage = lazy(() => import('./pages/HistoryPage'));
const TemplatesPage = lazy(() => import('./pages/TemplatesPage'));
const SuggestionsPage = lazy(() => import('./pages/SuggestionsPage'));
const EnhancedQueryBuilder = lazy(() => import('./components/QueryInterface/QueryBuilder').then(module => ({ default: module.QueryBuilder })));
const UserContextPanel = lazy(() => import('./components/AI/UserContextPanel'));
const QuerySimilarityAnalyzer = lazy(() => import('./components/AI/QuerySimilarityAnalyzer'));

const DashboardBuilder = lazy(() => import('./components/Dashboard/DashboardBuilder'));
const InteractiveVisualization = lazy(() => import('./components/Visualization/InteractiveVisualization').then(module => ({ default: module.InteractiveVisualization })));
const AdvancedFeaturesDemo = lazy(() => import('./components/Demo/AdvancedFeaturesDemo').then(module => ({ default: module.AdvancedFeaturesDemo })));
const SecurityDashboard = lazy(() => import('./components/Security/SecurityDashboard').then(module => ({ default: module.SecurityDashboard })));
const RequestSigningDemo = lazy(() => import('./components/Security/RequestSigningDemo').then(module => ({ default: module.RequestSigningDemo })));
const TypeSafetyDemo = lazy(() => import('./components/TypeSafety/TypeSafetyDemo').then(module => ({ default: module.TypeSafetyDemo })));
const CacheManager = lazy(() => import('./components/Cache/CacheManager').then(module => ({ default: module.CacheManager })));
const TuningDashboard = lazy(() => import('./components/Tuning/TuningDashboard').then(module => ({ default: module.TuningDashboard })));
const SchemaManagementDashboard = lazy(() => import('./components/SchemaManagement/SchemaManagementDashboard').then(module => ({ default: module.SchemaManagementDashboard })));
const QuerySuggestionsManager = lazy(() => import('./components/Admin/QuerySuggestionsManager').then(module => ({ default: module.QuerySuggestionsManager })));
const MinimalistQueryPage = lazy(() => import('./pages/MinimalistQueryPage'));
const DBExplorer = lazy(() => import('./components/DBExplorer/DBExplorer').then(module => ({ default: module.DBExplorer })));
const EnhancedQueryInterface = lazy(() => import('./components/QueryInterface/QueryInterface').then(module => ({ default: module.default })));
const EnhancedFeaturesDemo = lazy(() => import('./components/Demo/AdvancedFeaturesDemo').then(module => ({ default: module.AdvancedFeaturesDemo })));

const PerformanceMonitoringDashboard = lazy(() => import('./components/Performance/PerformanceMonitoringDashboard').then(module => ({ default: module.default })));
const AdvancedStreamingQuery = lazy(() => import('./components/QueryInterface/AdvancedStreamingQuery').then(module => ({ default: module.default })));
const GlobalResultDemo = lazy(() => import('./components/Demo/GlobalResultDemo').then(module => ({ default: module.GlobalResultDemo })));

const App: React.FC = () => {
  const { isAuthenticated, user, isAdmin } = useAuthStore();

  // Debug authentication state
  useEffect(() => {
    console.log('🔍 App component auth state:', {
      isAuthenticated,
      user,
      isAdmin,
      hasUser: !!user
    });
  }, [isAuthenticated, user, isAdmin]);

  // Initialize services
  useEffect(() => {
    ErrorService.initialize();

    // Initialize secure API client with enhanced security features
    secureApiClient.updateConfig({
      enableSigning: true,
      enableEncryption: false, // Can be enabled for sensitive data
      retryAttempts: 3,
      rateLimitConfig: {
        maxRequests: 100,
        windowMs: 60000, // 1 minute
      },
    });
  }, []);

  return (
    <PerformanceMonitor
      onMetrics={(metrics) => {
        console.log('App performance metrics:', metrics);
        // Send to analytics service in production
      }}
    >
      <BundleAnalyzer
        onAnalysis={(analysis) => {
          console.log('Bundle analysis:', analysis);
        }}
      />
      <ReactQueryProvider>
        <StateSyncProvider>
          <ErrorBoundary>
            <ConfigProvider
            theme={{
              algorithm: theme.defaultAlgorithm,
              token: {
                // Primary Colors
                colorPrimary: '#3b82f6',
                colorSuccess: '#10b981',
                colorWarning: '#f59e0b',
                colorError: '#ef4444',
                colorInfo: '#3b82f6',

                // Typography
                fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif",
                fontSize: 16,
                fontSizeHeading1: 36,
                fontSizeHeading2: 30,
                fontSizeHeading3: 24,
                fontSizeHeading4: 20,
                fontSizeHeading5: 16,

                // Layout
                borderRadius: 12,
                borderRadiusLG: 16,
                borderRadiusSM: 8,

                // Spacing
                padding: 16,
                paddingLG: 24,
                paddingSM: 12,
                margin: 16,
                marginLG: 24,
                marginSM: 12,

                // Colors
                colorBgContainer: '#ffffff',
                colorBgElevated: '#ffffff',
                colorBgLayout: '#f9fafb',
                colorBorder: '#e5e7eb',
                colorBorderSecondary: '#f3f4f6',
                colorText: '#374151',
                colorTextSecondary: '#6b7280',
                colorTextTertiary: '#9ca3af',

                // Shadows
                boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
                boxShadowSecondary: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
                boxShadowTertiary: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
              },
              components: {
                Button: {
                  borderRadius: 12,
                  fontWeight: 600,
                  primaryShadow: '0 4px 6px -1px rgba(59, 130, 246, 0.3)',
                },
                Input: {
                  borderRadius: 12,
                  fontSize: 16,
                  paddingBlock: 12,
                  paddingInline: 16,
                },
                Card: {
                  borderRadius: 16,
                  boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
                },
                Menu: {
                  borderRadius: 12,
                  itemBorderRadius: 8,
                },
                Tabs: {
                  borderRadius: 12,
                  cardBg: '#ffffff',
                },
              },
            }}
          >
            <Router>
              <div className="App">
                {isAuthenticated ? (
                  <Suspense fallback={<LoadingFallback />}>
                    <Routes>
                      {/* Minimalist Interface - No Layout */}
                      <Route path="/minimal" element={<MinimalistQueryPage />} />

                      {/* Standard Interface with Layout */}
                      <Route path="/*" element={
                        <Layout>
                          <Routes>
                            {/* Main Interface */}
                            <Route path="/" element={<QueryInterface />} />

                            {/* Analytics & Visualization */}
                            <Route path="/results" element={<ResultsPage />} />
                            <Route path="/dashboard" element={<DashboardBuilder />} />
                            <Route path="/interactive" element={<InteractiveVisualization />} />
                            {/* Legacy route redirect */}
                            <Route path="/advanced-viz" element={<Navigate to="/interactive" replace />} />

                            {/* Query Tools */}
                            <Route path="/history" element={<HistoryPage />} />
                            <Route path="/templates" element={<TemplatesPage />} />
                            <Route path="/suggestions" element={<SuggestionsPage />} />
                            <Route path="/streaming" element={<AdvancedStreamingQuery />} />
                            <Route path="/enhanced-query" element={<EnhancedQueryBuilder onExecuteQuery={(query) => console.log('Executing query:', query)} />} />
                            <Route path="/enhanced-ai" element={<EnhancedQueryInterface />} />
                            <Route path="/enhanced-demo" element={<EnhancedFeaturesDemo />} />
                            <Route path="/performance-monitoring" element={<PerformanceMonitoringDashboard />} />
                            <Route path="/db-explorer" element={<DBExplorer />} />
                            <Route path="/global-result-demo" element={<GlobalResultDemo />} />

                            {/* Admin Routes */}
                            <Route path="/admin/tuning" element={<TuningDashboard />} />
                            <Route path="/admin/schemas" element={<SchemaManagementDashboard />} />
                            <Route path="/admin/cache" element={<CacheManager />} />
                            <Route path="/admin/security" element={<SecurityDashboard />} />
                            <Route path="/admin/suggestions" element={<QuerySuggestionsManager />} />

                            {/* Legacy Routes */}
                            <Route path="/query" element={<Navigate to="/" replace />} />
                            <Route path="/ai-profile" element={<UserContextPanel />} />
                            <Route path="/similarity" element={<QuerySimilarityAnalyzer />} />
                            <Route path="/demo" element={<AdvancedFeaturesDemo />} />
                            <Route path="/showcase" element={<AdvancedFeaturesDemo />} />
                            <Route path="/security" element={<Navigate to="/admin/security" replace />} />
                            <Route path="/security/signing" element={<RequestSigningDemo />} />
                            <Route path="/security/types" element={<TypeSafetyDemo />} />

                            {/* Catch all */}
                            <Route path="*" element={<Navigate to="/" replace />} />
                          </Routes>
                        </Layout>
                      } />
                    </Routes>
                  </Suspense>
                ) : (
                  <Routes>
                    <Route path="/login" element={<Login />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />
                  </Routes>
                )}
                <DevTools />
              </div>
            </Router>
          </ConfigProvider>
        </ErrorBoundary>
      </StateSyncProvider>
    </ReactQueryProvider>
    </PerformanceMonitor>
  );
};

export default App;
