/**
 * Concurrent Router
 * 
 * High-performance router wrapper with React 18 concurrent features
 * for smooth navigation and optimal user experience
 */

import React, { 
  startTransition, 
  useDeferredValue, 
  useState, 
  useCallback,
  useEffect,
  ReactNode,
  Suspense
} from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Progress, message } from 'antd';
import { ConcurrentSuspense } from './ConcurrentSuspense';

interface ConcurrentRouterProps {
  children: ReactNode;
  enablePreloading?: boolean;
  enableProgressiveNavigation?: boolean;
  navigationTimeout?: number;
}

interface NavigationState {
  isNavigating: boolean;
  progress: number;
  targetPath: string;
  startTime: number;
}

// Route preloading cache
const routeCache = new Map<string, Promise<any>>();

// Route component mapping for preloading
const routeComponents: Record<string, () => Promise<any>> = {
  '/': () => import('../../pages/QueryPage'),
  '/dashboard': () => import('../../pages/DashboardPage'),
  '/results': () => import('../../pages/ResultsPage'),
  '/visualization': () => import('../../pages/VisualizationPage'),
  '/history': () => import('../../pages/HistoryPage'),
  '/templates': () => import('../../pages/TemplatesPage'),
  '/suggestions': () => import('../../pages/SuggestionsPage'),
  '/db-explorer': () => import('../../pages/DBExplorerPage'),
  '/performance': () => import('../../components/Performance/PerformanceMonitoringDashboard'),
  '/admin/tuning': () => import('../../pages/admin/TuningPage'),
  '/admin/llm': () => import('../../pages/admin/LLMManagementPage'),
  '/admin/cache': () => import('../../components/Cache/CacheManager'),
  '/admin/security': () => import('../../components/Security/SecurityDashboard'),
  '/ui-demo': () => import('../../pages/DesignSystemShowcase')
};

export const ConcurrentRouter: React.FC<ConcurrentRouterProps> = ({
  children,
  enablePreloading = true,
  enableProgressiveNavigation = true,
  navigationTimeout = 5000
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  
  const [navigationState, setNavigationState] = useState<NavigationState>({
    isNavigating: false,
    progress: 0,
    targetPath: '',
    startTime: 0
  });

  // Deferred navigation state for smooth transitions
  const deferredNavigating = useDeferredValue(navigationState.isNavigating);

  // Preload route component
  const preloadRoute = useCallback((path: string) => {
    if (!enablePreloading || routeCache.has(path)) return;

    const componentLoader = routeComponents[path];
    if (componentLoader) {
      const loadPromise = componentLoader().catch(error => {
        console.warn(`Failed to preload route ${path}:`, error);
        return null;
      });
      routeCache.set(path, loadPromise);
    }
  }, [enablePreloading]);

  // Enhanced navigation with concurrent features
  const navigateWithTransition = useCallback((path: string, options?: { replace?: boolean }) => {
    if (path === location.pathname) return;

    startTransition(() => {
      setNavigationState({
        isNavigating: true,
        progress: 0,
        targetPath: path,
        startTime: Date.now()
      });

      // Preload target route
      preloadRoute(path);

      // Simulate progressive navigation
      if (enableProgressiveNavigation) {
        const progressInterval = setInterval(() => {
          setNavigationState(prev => {
            const newProgress = Math.min(prev.progress + 20, 90);
            if (newProgress >= 90) {
              clearInterval(progressInterval);
            }
            return { ...prev, progress: newProgress };
          });
        }, 100);

        // Complete navigation
        setTimeout(() => {
          clearInterval(progressInterval);
          navigate(path, options);
          setNavigationState(prev => ({ 
            ...prev, 
            isNavigating: false, 
            progress: 100 
          }));
        }, 500);
      } else {
        navigate(path, options);
        setNavigationState(prev => ({ 
          ...prev, 
          isNavigating: false, 
          progress: 100 
        }));
      }
    });
  }, [location.pathname, navigate, preloadRoute, enableProgressiveNavigation]);

  // Preload adjacent routes on hover
  const handleRouteHover = useCallback((path: string) => {
    if (enablePreloading) {
      startTransition(() => {
        preloadRoute(path);
      });
    }
  }, [enablePreloading, preloadRoute]);

  // Navigation timeout handling
  useEffect(() => {
    if (navigationState.isNavigating) {
      const timeoutId = setTimeout(() => {
        setNavigationState(prev => ({ ...prev, isNavigating: false }));
        message.warning('Navigation is taking longer than expected');
      }, navigationTimeout);

      return () => clearTimeout(timeoutId);
    }
  }, [navigationState.isNavigating, navigationTimeout]);

  // Preload common routes on mount
  useEffect(() => {
    if (enablePreloading) {
      startTransition(() => {
        // Preload most common routes
        const commonRoutes = ['/', '/dashboard', '/results'];
        commonRoutes.forEach(route => {
          if (route !== location.pathname) {
            preloadRoute(route);
          }
        });
      });
    }
  }, [enablePreloading, location.pathname, preloadRoute]);

  // Progressive navigation indicator
  const renderNavigationProgress = () => {
    if (!deferredNavigating || !enableProgressiveNavigation) return null;

    return (
      <div style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 9999,
        background: 'rgba(255, 255, 255, 0.9)',
        backdropFilter: 'blur(4px)'
      }}>
        <Progress
          percent={navigationState.progress}
          strokeColor={{
            '0%': '#108ee9',
            '100%': '#87d068',
          }}
          showInfo={false}
          strokeWidth={3}
          style={{ margin: 0 }}
        />
        <div style={{
          textAlign: 'center',
          padding: '8px',
          fontSize: '12px',
          color: '#666'
        }}>
          Navigating to {navigationState.targetPath}...
        </div>
      </div>
    );
  };

  // Enhanced children with navigation context
  const enhancedChildren = React.Children.map(children, child => {
    if (React.isValidElement(child)) {
      return React.cloneElement(child, {
        ...child.props,
        onNavigate: navigateWithTransition,
        onRouteHover: handleRouteHover,
        isNavigating: navigationState.isNavigating
      } as any);
    }
    return child;
  });

  return (
    <>
      {renderNavigationProgress()}
      <ConcurrentSuspense
        enableProgressiveLoading={enableProgressiveNavigation}
        priority="high"
        timeout={navigationTimeout}
      >
        {enhancedChildren}
      </ConcurrentSuspense>
    </>
  );
};

// Hook for using concurrent navigation
export const useConcurrentNavigation = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const navigateWithTransition = useCallback((path: string, options?: { replace?: boolean }) => {
    startTransition(() => {
      navigate(path, options);
    });
  }, [navigate]);

  const preloadRoute = useCallback((path: string) => {
    const componentLoader = routeComponents[path];
    if (componentLoader && !routeCache.has(path)) {
      const loadPromise = componentLoader().catch(error => {
        console.warn(`Failed to preload route ${path}:`, error);
        return null;
      });
      routeCache.set(path, loadPromise);
    }
  }, []);

  return {
    navigate: navigateWithTransition,
    preloadRoute,
    currentPath: location.pathname,
    isRoutePreloaded: (path: string) => routeCache.has(path)
  };
};

export default ConcurrentRouter;
