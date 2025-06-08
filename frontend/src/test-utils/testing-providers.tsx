/**
 * Testing Providers
 * Comprehensive testing utilities with all necessary providers for component testing
 */

import React, { ReactElement, ReactNode } from 'react';
import { render, RenderOptions, RenderResult } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { ConfigProvider } from 'antd';
// import { ThemeProvider } from 'styled-components'; // Commented out - not used
import { QueryProvider } from '../components/QueryInterface/QueryProvider';

// Mock data and configurations
export const mockQueryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
      gcTime: 0,
      staleTime: 0,
    },
    mutations: {
      retry: false,
    },
  },
});

// Theme configuration for testing
export const testTheme = {
  colors: {
    primary: '#1890ff',
    secondary: '#52c41a',
    error: '#ff4d4f',
    warning: '#faad14',
    success: '#52c41a',
    info: '#1890ff',
  },
  spacing: {
    xs: '4px',
    sm: '8px',
    md: '16px',
    lg: '24px',
    xl: '32px',
  },
  breakpoints: {
    xs: '480px',
    sm: '576px',
    md: '768px',
    lg: '992px',
    xl: '1200px',
    xxl: '1600px',
  },
};

// Ant Design configuration for testing
export const testAntdConfig = {
  theme: {
    token: {
      colorPrimary: '#1890ff',
    },
  },
};

// Mock implementations for external services
export const mockApiService = {
  executeQuery: jest.fn(),
  getTableDetails: jest.fn(),
  getHealthStatus: jest.fn(),
  refreshToken: jest.fn(),
};

export const mockQueryTemplateService = {
  getTemplates: jest.fn(() => []),
  getShortcuts: jest.fn(() => []),
  searchSuggestions: jest.fn(() => []),
  createTemplate: jest.fn(),
  createShortcut: jest.fn(),
  toggleFavorite: jest.fn(),
  incrementUsage: jest.fn(),
  processTemplate: jest.fn(),
  addToRecent: jest.fn(),
  getCategories: jest.fn(() => []),
  getPopularTemplates: jest.fn(() => []),
  getFavoriteTemplates: jest.fn(() => []),
};

export const mockTuningApiService = {
  getDashboard: jest.fn(),
  getBusinessTables: jest.fn(),
  updateBusinessTable: jest.fn(),
  getBusinessGlossary: jest.fn(),
  getQueryPatterns: jest.fn(),
  getPromptLogs: jest.fn(),
  autoGenerateTableContexts: jest.fn(),
  autoGenerateGlossaryTerms: jest.fn(),
  clearPromptCache: jest.fn(),
};

// Provider wrapper for testing
interface TestProvidersProps {
  children: ReactNode;
  queryClient?: QueryClient;
  initialRoute?: string;
  theme?: any;
  antdConfig?: any;
}

export const TestProviders: React.FC<TestProvidersProps> = ({
  children,
  queryClient = mockQueryClient,
  initialRoute = '/',
  theme = testTheme,
  antdConfig = testAntdConfig,
}) => {
  // Set initial route if provided
  if (initialRoute !== '/') {
    window.history.pushState({}, 'Test page', initialRoute);
  }

  return (
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <ConfigProvider {...antdConfig}>
          <QueryProvider>
            {children}
          </QueryProvider>
        </ConfigProvider>
      </QueryClientProvider>
    </BrowserRouter>
  );
};

// Custom render function with providers
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  queryClient?: QueryClient;
  initialRoute?: string;
  theme?: any;
  antdConfig?: any;
}

export const renderWithProviders = (
  ui: ReactElement,
  options: CustomRenderOptions = {}
): RenderResult => {
  const {
    queryClient,
    initialRoute,
    theme,
    antdConfig,
    ...renderOptions
  } = options;

  const Wrapper: React.FC<{ children: ReactNode }> = ({ children }) => (
    <TestProviders
      queryClient={queryClient}
      initialRoute={initialRoute}
      theme={theme}
      antdConfig={antdConfig}
    >
      {children}
    </TestProviders>
  );

  return render(ui, { wrapper: Wrapper, ...renderOptions });
};

// Minimal provider for simple tests
export const MinimalTestProvider: React.FC<{ children: ReactNode }> = ({ children }) => (
  <QueryClientProvider client={mockQueryClient}>
    <ConfigProvider {...testAntdConfig}>
      {children}
    </ConfigProvider>
  </QueryClientProvider>
);

// Query-only provider for testing React Query hooks
export const QueryTestProvider: React.FC<{ children: ReactNode; queryClient?: QueryClient }> = ({
  children,
  queryClient = mockQueryClient
}) => (
  <QueryClientProvider client={queryClient}>
    {children}
  </QueryClientProvider>
);

// Router-only provider for testing routing
export const RouterTestProvider: React.FC<{ children: ReactNode; initialRoute?: string }> = ({
  children,
  initialRoute = '/'
}) => {
  if (initialRoute !== '/') {
    window.history.pushState({}, 'Test page', initialRoute);
  }

  return (
    <BrowserRouter>
      {children}
    </BrowserRouter>
  );
};

// Theme-only provider for testing styled components
export const ThemeTestProvider: React.FC<{ children: ReactNode; theme?: any }> = ({
  children,
  theme = testTheme
}) => (
  <div data-testid="theme-provider">
    {children}
  </div>
);

// Utility functions for testing
export const createMockQueryClient = (options?: any): QueryClient => {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
        staleTime: 0,
        ...options?.queries,
      },
      mutations: {
        retry: false,
        ...options?.mutations,
      },
    },
  });
};

// Mock data generators
export const createMockTemplate = (overrides = {}) => ({
  id: 'test-template-1',
  name: 'Test Template',
  description: 'A test template for unit tests',
  category: 'financial' as const,
  template: 'Show me revenue from {{startDate}} to {{endDate}}',
  variables: [
    {
      name: 'startDate',
      type: 'date' as const,
      description: 'Start date',
      required: true,
      defaultValue: '2024-01-01',
    },
    {
      name: 'endDate',
      type: 'date' as const,
      description: 'End date',
      required: true,
      defaultValue: '2024-12-31',
    },
  ],
  tags: ['revenue', 'financial'],
  difficulty: 'beginner' as const,
  estimatedRows: 12,
  executionTime: '< 1s',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  usageCount: 5,
  isFavorite: false,
  isPublic: true,
  ...overrides,
});

export const createMockShortcut = (overrides = {}) => ({
  id: 'test-shortcut-1',
  name: 'Test Shortcut',
  shortcut: 'test',
  query: 'Show me test data',
  description: 'A test shortcut for unit tests',
  category: 'test',
  isActive: true,
  usageCount: 3,
  createdAt: '2024-01-01T00:00:00Z',
  ...overrides,
});

export const createMockQueryResult = (overrides = {}) => ({
  queryId: 'test-query-1',
  sql: 'SELECT * FROM test_table',
  result: {
    data: [
      { id: 1, name: 'Test 1', value: 100 },
      { id: 2, name: 'Test 2', value: 200 },
    ],
    metadata: {
      columnCount: 3,
      rowCount: 2,
      executionTimeMs: 150,
    },
  },
  success: true,
  confidence: 0.95,
  suggestions: [],
  explanation: 'This query retrieves test data',
  visualization: {
    type: 'table',
    config: {},
  },
  ...overrides,
});

export const createMockHealthStatus = (overrides = {}) => ({
  database: {
    connected: true,
    responseTime: 50,
    lastCheck: '2024-01-01T00:00:00Z',
  },
  api: {
    connected: true,
    responseTime: 25,
    lastCheck: '2024-01-01T00:00:00Z',
  },
  cache: {
    connected: true,
    hitRate: 0.85,
    size: 1024,
  },
  overall: 'healthy' as const,
  ...overrides,
});

// Test utilities for async operations
export const waitForQueryToSettle = async (queryClient: QueryClient) => {
  await queryClient.getQueryCache().getAll().forEach(query => {
    if (query.state.fetchStatus === 'fetching') {
      return query.promise;
    }
  });
};

export const flushPromises = () => new Promise(resolve => setTimeout(resolve, 0));

// Mock localStorage for testing
export const mockLocalStorage = (() => {
  let store: Record<string, string> = {};

  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString();
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      store = {};
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: jest.fn((index: number) => Object.keys(store)[index] || null),
  };
})();

// Mock sessionStorage for testing
export const mockSessionStorage = (() => {
  let store: Record<string, string> = {};

  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString();
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      store = {};
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: jest.fn((index: number) => Object.keys(store)[index] || null),
  };
})();

// Setup function for tests
export const setupTestEnvironment = () => {
  // Mock localStorage
  Object.defineProperty(window, 'localStorage', {
    value: mockLocalStorage,
    writable: true,
  });

  // Mock sessionStorage
  Object.defineProperty(window, 'sessionStorage', {
    value: mockSessionStorage,
    writable: true,
  });

  // Mock window.matchMedia
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
      dispatchEvent: jest.fn(),
    })),
  });

  // Mock IntersectionObserver
  global.IntersectionObserver = jest.fn().mockImplementation(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn(),
  }));

  // Mock ResizeObserver
  global.ResizeObserver = jest.fn().mockImplementation(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn(),
  }));

  // Clear all mocks before each test
  beforeEach(() => {
    jest.clearAllMocks();
    mockLocalStorage.clear();
    mockSessionStorage.clear();
    mockQueryClient.clear();
  });
};

// Export everything for easy importing
export * from '@testing-library/react';
export { renderWithProviders as render };

// Re-export common testing utilities
export { default as userEvent } from '@testing-library/user-event';
export { waitFor, act } from '@testing-library/react';
