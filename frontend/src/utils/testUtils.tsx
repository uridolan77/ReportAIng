/**
 * Enhanced Testing Utilities
 * 
 * Comprehensive testing utilities for React applications including
 * custom render functions, mock providers, and testing helpers
 */

import React, { ReactElement, ReactNode } from 'react';
import { render, RenderOptions, RenderResult } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import { ThemeProvider } from '../contexts/ThemeContext';
import { AuthProvider } from '../contexts/AuthContext';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';

// Mock data generators
export const mockUser = {
  id: '1',
  username: 'testuser',
  email: 'test@example.com',
  role: 'admin',
  isAuthenticated: true
};

export const mockQueryResult = {
  id: '1',
  query: 'SELECT * FROM users',
  results: [
    { id: 1, name: 'John Doe', email: 'john@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com' }
  ],
  executionTime: 150,
  timestamp: new Date().toISOString()
};

export const mockPerformanceMetrics = {
  usedJSHeapSize: 50 * 1024 * 1024, // 50MB
  totalJSHeapSize: 100 * 1024 * 1024, // 100MB
  jsHeapSizeLimit: 2 * 1024 * 1024 * 1024, // 2GB
  componentCount: 25,
  listenerCount: 15,
  observerCount: 5
};

// Custom render function with providers
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  initialEntries?: string[];
  queryClient?: QueryClient;
  theme?: 'light' | 'dark';
  authenticated?: boolean;
  user?: typeof mockUser;
}

export const renderWithProviders = (
  ui: ReactElement,
  options: CustomRenderOptions = {}
): RenderResult => {
  const {
    initialEntries = ['/'],
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false }
      }
    }),
    theme = 'light',
    authenticated = true,
    user = mockUser,
    ...renderOptions
  } = options;

  const AllTheProviders = ({ children }: { children: ReactNode }) => (
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <ConfigProvider>
          <ThemeProvider>
            <AuthProvider>
              <QueryProvider>
                {children}
              </QueryProvider>
            </AuthProvider>
          </ThemeProvider>
        </ConfigProvider>
      </QueryClientProvider>
    </BrowserRouter>
  );

  return render(ui, { wrapper: AllTheProviders, ...renderOptions });
};

// Mock implementations
export const mockLocalStorage = (() => {
  let store: Record<string, string> = {};

  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value.toString();
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      store = {};
    },
    get length() {
      return Object.keys(store).length;
    },
    key: (index: number) => Object.keys(store)[index] || null
  };
})();

export const mockSessionStorage = (() => {
  let store: Record<string, string> = {};

  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value.toString();
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      store = {};
    },
    get length() {
      return Object.keys(store).length;
    },
    key: (index: number) => Object.keys(store)[index] || null
  };
})();

// Mock fetch function
export const mockFetch = (response: any, options: { status?: number; ok?: boolean } = {}) => {
  const { status = 200, ok = true } = options;
  
  return jest.fn().mockResolvedValue({
    ok,
    status,
    json: async () => response,
    text: async () => JSON.stringify(response),
    headers: new Headers(),
    redirected: false,
    statusText: ok ? 'OK' : 'Error',
    type: 'basic' as ResponseType,
    url: 'http://localhost:3000/api/test'
  });
};

// Performance testing utilities
export const measureRenderTime = async (renderFn: () => void): Promise<number> => {
  const start = performance.now();
  renderFn();
  await new Promise(resolve => setTimeout(resolve, 0)); // Wait for next tick
  const end = performance.now();
  return end - start;
};

export const measureMemoryUsage = (): { before: number; after: number; diff: number } | null => {
  if ('memory' in performance) {
    const memory = (performance as any).memory;
    const before = memory.usedJSHeapSize;
    
    // Force garbage collection if available
    if ('gc' in window && typeof (window as any).gc === 'function') {
      (window as any).gc();
    }
    
    const after = memory.usedJSHeapSize;
    return {
      before,
      after,
      diff: after - before
    };
  }
  return null;
};

// Component testing helpers
export const waitForLoadingToFinish = async () => {
  await new Promise(resolve => setTimeout(resolve, 100));
};

export const triggerResize = (width: number, height: number) => {
  Object.defineProperty(window, 'innerWidth', {
    writable: true,
    configurable: true,
    value: width
  });
  Object.defineProperty(window, 'innerHeight', {
    writable: true,
    configurable: true,
    value: height
  });
  window.dispatchEvent(new Event('resize'));
};

// Mock API responses
export const mockApiResponses = {
  health: {
    status: 'healthy',
    database: 'connected',
    cache: 'operational',
    timestamp: new Date().toISOString()
  },
  
  queryExecution: {
    success: true,
    results: mockQueryResult.results,
    executionTime: mockQueryResult.executionTime,
    rowCount: mockQueryResult.results.length
  },
  
  userProfile: mockUser,
  
  performanceMetrics: mockPerformanceMetrics,
  
  error: {
    success: false,
    error: 'Test error message',
    code: 'TEST_ERROR'
  }
};

// Test data factories
export const createMockQuery = (overrides: Partial<typeof mockQueryResult> = {}) => ({
  ...mockQueryResult,
  ...overrides
});

export const createMockUser = (overrides: Partial<typeof mockUser> = {}) => ({
  ...mockUser,
  ...overrides
});

export const createMockPerformanceData = (overrides: Partial<typeof mockPerformanceMetrics> = {}) => ({
  ...mockPerformanceMetrics,
  ...overrides
});

// Accessibility testing helpers
export const checkAccessibility = async (container: HTMLElement) => {
  const { axe } = await import('@axe-core/react');
  const results = await axe(container);
  return results;
};

// Visual regression testing helpers
export const takeScreenshot = async (element: HTMLElement, name: string) => {
  // This would integrate with a visual regression testing tool
  console.log(`Taking screenshot: ${name}`, element);
};

// Custom matchers for Jest
export const customMatchers = {
  toBeAccessible: async (received: HTMLElement) => {
    try {
      const results = await checkAccessibility(received);
      const violations = results.violations;
      
      if (violations.length === 0) {
        return {
          message: () => 'Element is accessible',
          pass: true
        };
      } else {
        return {
          message: () => `Element has ${violations.length} accessibility violations: ${violations.map(v => v.description).join(', ')}`,
          pass: false
        };
      }
    } catch (error) {
      return {
        message: () => `Accessibility check failed: ${error}`,
        pass: false
      };
    }
  },
  
  toHavePerformantRender: async (renderFn: () => void, maxTime: number = 100) => {
    const renderTime = await measureRenderTime(renderFn);
    
    if (renderTime <= maxTime) {
      return {
        message: () => `Render time ${renderTime.toFixed(2)}ms is within acceptable range`,
        pass: true
      };
    } else {
      return {
        message: () => `Render time ${renderTime.toFixed(2)}ms exceeds maximum of ${maxTime}ms`,
        pass: false
      };
    }
  }
};

// Setup function for tests
export const setupTests = () => {
  // Mock localStorage
  Object.defineProperty(window, 'localStorage', {
    value: mockLocalStorage
  });
  
  // Mock sessionStorage
  Object.defineProperty(window, 'sessionStorage', {
    value: mockSessionStorage
  });
  
  // Mock IntersectionObserver
  global.IntersectionObserver = class IntersectionObserver {
    constructor() {}
    observe() {}
    disconnect() {}
    unobserve() {}
  };
  
  // Mock ResizeObserver
  global.ResizeObserver = class ResizeObserver {
    constructor() {}
    observe() {}
    disconnect() {}
    unobserve() {}
  };
  
  // Mock matchMedia
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: jest.fn().mockImplementation(query => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: jest.fn(),
      removeListener: jest.fn(),
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      dispatchEvent: jest.fn()
    }))
  });
};

// Phase 8 Production Testing Utilities
export const productionTestUtils = {
  // Security testing
  testSecurityHeaders: () => {
    const csp = document.querySelector('meta[http-equiv="Content-Security-Policy"]');
    return {
      hasCSP: !!csp,
      isSecure: csp ? !csp.getAttribute('content')?.includes("'unsafe-eval'") : false
    };
  },

  // Performance testing
  testPerformanceMetrics: () => {
    if (!('performance' in window)) return null;

    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    return {
      loadTime: navigation ? navigation.loadEventEnd - navigation.loadEventStart : 0,
      domContentLoaded: navigation ? navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart : 0,
      firstPaint: performance.getEntriesByName('first-paint')[0]?.startTime || 0,
      firstContentfulPaint: performance.getEntriesByName('first-contentful-paint')[0]?.startTime || 0
    };
  },

  // Bundle analysis
  testBundleOptimization: () => {
    const scripts = document.querySelectorAll('script[src]');
    const totalScripts = scripts.length;
    const asyncScripts = document.querySelectorAll('script[async], script[defer]').length;

    return {
      totalScripts,
      asyncScripts,
      asyncPercentage: totalScripts > 0 ? (asyncScripts / totalScripts) * 100 : 0,
      hasServiceWorker: 'serviceWorker' in navigator
    };
  }
};

// Export everything
export * from '@testing-library/react';
export { renderWithProviders as render };
