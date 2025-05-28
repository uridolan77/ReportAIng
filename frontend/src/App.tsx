import React, { Suspense, lazy, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, theme } from 'antd';
import { ErrorBoundary } from './components/ErrorBoundary/ErrorBoundary';
import { LoadingFallback } from './components/ui';
import { Layout } from './components/Layout/Layout';
import { Login } from './components/Auth/Login';
import { useAuthStore } from './stores/authStore';
import { ErrorService } from './services/errorService';
import { DevTools } from './components/DevTools/DevTools';
import './App.css';

// Lazy load heavy components
const QueryInterface = lazy(() => import('./components/QueryInterface/QueryInterface').then(module => ({ default: module.QueryInterface })));
const EnhancedQueryBuilder = lazy(() => import('./components/QueryInterface/EnhancedQueryBuilder'));
const UserContextPanel = lazy(() => import('./components/AI/UserContextPanel'));
const QuerySimilarityAnalyzer = lazy(() => import('./components/AI/QuerySimilarityAnalyzer'));
const AdvancedVisualizationPanel = lazy(() => import('./components/Visualization/AdvancedVisualizationPanel'));
const AdvancedVisualizationDemo = lazy(() => import('./components/Demo/AdvancedVisualizationDemo'));
const AdvancedFeaturesDemo = lazy(() => import('./components/Demo/AdvancedFeaturesDemo').then(module => ({ default: module.AdvancedFeaturesDemo })));
const UltimateShowcase = lazy(() => import('./components/Demo/UltimateShowcase').then(module => ({ default: module.UltimateShowcase })));

const App: React.FC = () => {
  const { isAuthenticated } = useAuthStore();

  // Initialize error service
  useEffect(() => {
    ErrorService.initialize();
  }, []);

  return (
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
                    <Route path="/" element={<QueryInterface />} />
                    <Route path="/query" element={<QueryInterface />} />
                    <Route path="/enhanced-query" element={<EnhancedQueryBuilder />} />
                    <Route path="/ai-profile" element={<UserContextPanel />} />
                    <Route path="/similarity" element={<QuerySimilarityAnalyzer />} />
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
                    <Route path="/demo" element={<AdvancedVisualizationDemo />} />
                    <Route path="/advanced-demo" element={<AdvancedFeaturesDemo />} />
                    <Route path="/showcase" element={<UltimateShowcase />} />
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
            <DevTools />
          </div>
        </Router>
      </ConfigProvider>
    </ErrorBoundary>
  );
};

export default App;
