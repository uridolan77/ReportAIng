import React, { lazy, useEffect, startTransition } from 'react';
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
import { ConcurrentSuspense } from './components/Performance/ConcurrentSuspense';
import { initializeBundleOptimization } from './utils/bundleOptimization';
import Sidebar from './components/Layout/Sidebar';
import { CornerStatusPanel } from './components/Layout/AppHeader';
// Using public folder for background image
const mainBgImage = '/main-bg.jpg';
import './App.css';
import './styles/components/ui-components.css';

// Enhanced lazy loading with webpack chunk names for better caching
const SimpleQueryInterface = lazy(() =>
  import(/* webpackChunkName: "query-interface" */ './components/QueryInterface/SimpleQueryInterface')
);
const ResultsPage = lazy(() =>
  import(/* webpackChunkName: "results-page" */ './pages/ResultsPage')
);
const HistoryPage = lazy(() =>
  import(/* webpackChunkName: "history-page" */ './pages/HistoryPage')
);
const TemplatesPage = lazy(() =>
  import(/* webpackChunkName: "templates-page" */ './pages/TemplatesPage')
);
const SuggestionsPage = lazy(() =>
  import(/* webpackChunkName: "suggestions-page" */ './pages/SuggestionsPage')
);
const DashboardPage = lazy(() =>
  import(/* webpackChunkName: "dashboard-page" */ './pages/DashboardPage')
);
const VisualizationPage = lazy(() =>
  import(/* webpackChunkName: "visualization-page" */ './pages/VisualizationPage')
);
const DBExplorerPage = lazy(() =>
  import(/* webpackChunkName: "db-explorer-page" */ './pages/DBExplorerPage')
);

// Admin pages with chunk names
const TuningPage = lazy(() =>
  import(/* webpackChunkName: "admin-tuning" */ './pages/admin/TuningPage')
);
const LLMManagementPage = lazy(() =>
  import(/* webpackChunkName: "admin-llm" */ './pages/admin/LLMManagementPage')
);


const CacheManagementPage = lazy(() =>
  import(/* webpackChunkName: "admin-cache" */ './components/Cache/CacheManager').then(module => ({ default: module.CacheManager }))
);
const SecurityPage = lazy(() =>
  import(/* webpackChunkName: "admin-security" */ './components/Security/SecurityDashboard').then(module => ({ default: module.SecurityDashboard }))
);


// Demo and development pages
const UIDemo = lazy(() =>
  import(/* webpackChunkName: "ui-demo" */ './pages/DesignSystemShowcase')
);
const PerformancePage = lazy(() =>
  import(/* webpackChunkName: "performance-page" */ './pages/PerformancePage')
);

// Preload critical components for better performance using concurrent features
const preloadCriticalComponents = () => {
  startTransition(() => {
    // Preload most commonly used pages with low priority
    import('./pages/QueryPage');
    import('./pages/DashboardPage');
    import('./pages/ResultsPage');
  });
};

// Preload on user interaction (hover, focus)
const preloadOnInteraction = () => {
  startTransition(() => {
    // Preload remaining pages on first user interaction with low priority
    import('./pages/VisualizationPage');
    import('./pages/HistoryPage');
    import('./pages/SuggestionsPage');
  });
};

const App: React.FC = () => {
  const { isAuthenticated, isAdmin } = useAuthStore();

  // Initialize services and preload components
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

    // Initialize bundle optimization
    initializeBundleOptimization();

    // Preload critical components after initial render
    const timer = setTimeout(() => {
      preloadCriticalComponents();
    }, 1000);

    // Preload on first user interaction
    const handleFirstInteraction = () => {
      preloadOnInteraction();
      document.removeEventListener('mouseenter', handleFirstInteraction);
      document.removeEventListener('click', handleFirstInteraction);
    };

    document.addEventListener('mouseenter', handleFirstInteraction, { once: true });
    document.addEventListener('click', handleFirstInteraction, { once: true });

    return () => {
      clearTimeout(timer);
      document.removeEventListener('mouseenter', handleFirstInteraction);
      document.removeEventListener('click', handleFirstInteraction);
    };
  }, []);

  return (
    <PerformanceMonitor
      onMetrics={(metrics) => {
        if (process.env['NODE_ENV'] === 'development') {
          console.log('App performance metrics:', metrics);
        }
      }}
    >
      <BundleAnalyzer
        onAnalysis={(analysis) => {
          if (process.env['NODE_ENV'] === 'development') {
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
  const { antdTheme, isDarkMode } = useTheme();
  const [sidebarCollapsed, setSidebarCollapsed] = React.useState(false);

  // Debug background image and set CSS custom property
  React.useEffect(() => {
    console.log('Background image path:', mainBgImage);
    console.log('isDarkMode:', isDarkMode);

    // Set CSS custom property for background image
    document.documentElement.style.setProperty('--app-background-image', `url(${mainBgImage})`);
  }, [isDarkMode]);

  return (
    <ConfigProvider theme={antdTheme}>
      <Router>
        <div className="App app-with-background">
          {isAuthenticated ? (
            <ConcurrentSuspense
              fallback={<LoadingFallback />}
              enableProgressiveLoading={true}
              priority="high"
            >
              <CornerStatusPanel />
              <div style={{ display: 'flex', minHeight: '100vh' }}>
                <Sidebar
                  collapsed={sidebarCollapsed}
                  onCollapse={setSidebarCollapsed}
                />
                <div
                  className="main-content-area"
                  style={{
                    flex: 1,
                    padding: '24px',
                    paddingTop: '80px', // Space for corner panel
                    minHeight: '100vh',
                    position: 'relative'
                  }}
                >
                  {/* Subtle background overlay for content readability */}
                  <div
                    style={{
                      position: 'absolute',
                      top: 0,
                      left: 0,
                      right: 0,
                      bottom: 0,
                      backgroundColor: isDarkMode ? 'rgba(31, 41, 55, 0.15)' : 'rgba(248, 250, 252, 0.15)', // Theme-aware overlay
                      backdropFilter: 'blur(0.5px)',
                      zIndex: 0
                    }}
                  />

                  {/* Content container */}
                  <div style={{ position: 'relative', zIndex: 1 }}>
                    <Routes>
                  {/* Main Interface */}
                  <Route path="/" element={<SimpleQueryInterface />} />

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
                      <Route path="/admin/cache" element={<CacheManagementPage />} />
                      <Route path="/admin/security" element={<SecurityPage />} />
                      <Route path="/ui-demo" element={<UIDemo />} />
                    </>
                  )}

                  {/* Legacy redirects */}
                  <Route path="/interactive" element={<Navigate to="/visualization" replace />} />
                  <Route path="/advanced-viz" element={<Navigate to="/visualization" replace />} />
                  <Route path="/query" element={<Navigate to="/" replace />} />
                  <Route path="/security" element={<Navigate to="/admin/security" replace />} />
                  <Route path="/admin/llm-test" element={<Navigate to="/admin/llm" replace />} />
                  <Route path="/admin/llm-debug" element={<Navigate to="/admin/llm" replace />} />
                  <Route path="/admin/schemas" element={<Navigate to="/db-explorer" replace />} />
                  <Route path="/admin/suggestions" element={<Navigate to="/suggestions" replace />} />
                  <Route path="/admin/suggestions" element={<Navigate to="/suggestions" replace />} />

                    {/* Catch all */}
                    <Route path="*" element={<Navigate to="/" replace />} />
                  </Routes>
                  </div>
                </div>
              </div>
            </ConcurrentSuspense>
          ) : (
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
          )}
          {process.env['NODE_ENV'] === 'development' && <DevTools />}
        </div>
      </Router>
    </ConfigProvider>
  );
};

export default App;
