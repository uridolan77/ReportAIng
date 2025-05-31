import React, { Suspense, lazy, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, theme } from 'antd';
import { ErrorBoundary } from './components/ErrorBoundary/ErrorBoundary';
import { LoadingFallback } from './components/ui';
import { Layout } from './components/Layout/Layout';
import { Login } from './components/Auth/Login';
import { useAuthStore } from './stores/authStore';
import { ErrorService } from './services/errorService';
import { EnhancedDevTools } from './components/DevTools/EnhancedDevTools';
import { ReactQueryProvider } from './components/Providers/ReactQueryProvider';
import { StateSyncProvider } from './components/Providers/StateSyncProvider';
import { secureApiClient } from './services/secureApiClient';
import './App.css';

// Lazy load heavy components
const QueryInterface = lazy(() => import('./components/QueryInterface/QueryInterface').then(module => ({ default: module.QueryInterface })));
const ResultsPage = lazy(() => import('./pages/ResultsPage').then(module => ({ default: module.ResultsPage })));
const HistoryPage = lazy(() => import('./pages/HistoryPage').then(module => ({ default: module.HistoryPage })));
const TemplatesPage = lazy(() => import('./pages/TemplatesPage').then(module => ({ default: module.TemplatesPage })));
const SuggestionsPage = lazy(() => import('./pages/SuggestionsPage').then(module => ({ default: module.SuggestionsPage })));
const EnhancedQueryBuilder = lazy(() => import('./components/QueryInterface/EnhancedQueryBuilder'));
const UserContextPanel = lazy(() => import('./components/AI/UserContextPanel'));
const QuerySimilarityAnalyzer = lazy(() => import('./components/AI/QuerySimilarityAnalyzer'));
const AdvancedVisualizationPanel = lazy(() => import('./components/Visualization/AdvancedVisualizationPanel'));
const AdvancedFeaturesDemo = lazy(() => import('./components/Demo/AdvancedFeaturesDemo').then(module => ({ default: module.AdvancedFeaturesDemo })));
const SecurityDashboard = lazy(() => import('./components/Security/SecurityDashboard').then(module => ({ default: module.SecurityDashboard })));
const RequestSigningDemo = lazy(() => import('./components/Security/RequestSigningDemo').then(module => ({ default: module.RequestSigningDemo })));
const TypeSafetyDemo = lazy(() => import('./components/TypeSafety/TypeSafetyDemo').then(module => ({ default: module.TypeSafetyDemo })));

const App: React.FC = () => {
  const { isAuthenticated } = useAuthStore();

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
    <ReactQueryProvider>
      <StateSyncProvider>
        <ErrorBoundary>
          <ConfigProvider
            theme={{
              algorithm: theme.defaultAlgorithm,
              token: {
                colorPrimary: '#1890ff',
                borderRadius: 6,
              },
            }}
          >
            <Router>
              <div className="App">
                {isAuthenticated ? (
                  <Layout>
                    <Suspense fallback={<LoadingFallback />}>
                      <Routes>
                        {/* Main Interface */}
                        <Route path="/" element={<QueryInterface />} />

                        {/* Analytics & Visualization */}
                        <Route path="/results" element={<ResultsPage />} />
                        <Route path="/dashboard" element={<div style={{ padding: '40px', textAlign: 'center' }}>Dashboard View - Coming Soon</div>} />
                        <Route path="/interactive" element={<div style={{ padding: '40px', textAlign: 'center' }}>Interactive Visualization - Coming Soon</div>} />
                        <Route
                          path="/advanced-viz"
                          element={
                            <AdvancedVisualizationPanel
                              data={[]}
                              columns={[]}
                              query=""
                            />
                          }
                        />

                        {/* Query Tools */}
                        <Route path="/history" element={<HistoryPage />} />
                        <Route path="/templates" element={<TemplatesPage />} />
                        <Route path="/suggestions" element={<SuggestionsPage />} />
                        <Route path="/streaming" element={<div style={{ padding: '40px', textAlign: 'center' }}>Streaming Queries - Coming Soon</div>} />
                        <Route path="/enhanced-query" element={<EnhancedQueryBuilder />} />

                        {/* Admin Routes */}
                        <Route path="/admin/tuning" element={<div style={{ padding: '40px', textAlign: 'center' }}>AI Tuning - Coming Soon</div>} />
                        <Route path="/admin/cache" element={<div style={{ padding: '40px', textAlign: 'center' }}>Cache Manager - Coming Soon</div>} />
                        <Route path="/admin/security" element={<SecurityDashboard />} />

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
                    </Suspense>
                  </Layout>
                ) : (
                  <Routes>
                    <Route path="/login" element={<Login />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />
                  </Routes>
                )}
                <EnhancedDevTools />
              </div>
            </Router>
          </ConfigProvider>
        </ErrorBoundary>
      </StateSyncProvider>
    </ReactQueryProvider>
  );
};

export default App;
