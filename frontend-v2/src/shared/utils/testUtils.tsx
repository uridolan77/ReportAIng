/**
 * Test Utilities
 * 
 * Provides comprehensive testing utilities for React components,
 * API mocking, and integration testing.
 */

import React from 'react'
import { render, RenderOptions } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { ConfigProvider } from 'antd'
import { store } from '../store'
import { antdTheme } from '../theme'

// Mock data for testing
export const mockUser = {
  id: '1',
  username: 'testuser',
  email: 'test@example.com',
  displayName: 'Test User',
  roles: ['User'],
  isActive: true,
  lastLoginDate: new Date().toISOString(),
  isMfaEnabled: false
}

export const mockBusinessTable = {
  id: 1,
  tableName: 'TestTable',
  schemaName: 'TestSchema',
  businessPurpose: 'Test table for unit testing',
  businessContext: 'Used in automated tests',
  primaryUseCase: 'Testing',
  commonQueryPatterns: 'SELECT * FROM TestTable',
  businessRules: 'No special rules',
  domainClassification: 'Test',
  naturalLanguageAliases: 'test, testing',
  usagePatterns: 'High frequency',
  dataQualityIndicators: 'Good quality',
  relationshipSemantics: 'Standalone',
  importanceScore: 5,
  usageFrequency: 50,
  businessOwner: 'Test Team',
  dataGovernancePolicies: 'Standard policies',
  isActive: true,
  createdDate: new Date().toISOString(),
  updatedDate: new Date().toISOString(),
  columns: []
}

export const mockQueryHistory = {
  id: '1',
  question: 'Test query',
  sql: 'SELECT * FROM TestTable',
  executedAt: new Date().toISOString(),
  executionTime: 1.0,
  rowCount: 10,
  isFavorite: false,
  tags: ['test']
}

// Custom render function with providers
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  preloadedState?: any
  store?: any
  queryClient?: QueryClient
}

export const renderWithProviders = (
  ui: React.ReactElement,
  {
    preloadedState = {},
    store: testStore = store,
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false }
      }
    }),
    ...renderOptions
  }: CustomRenderOptions = {}
) => {
  const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <QueryClientProvider client={queryClient}>
      <Provider store={testStore}>
        <BrowserRouter>
          <ConfigProvider theme={antdTheme}>
            {children}
          </ConfigProvider>
        </BrowserRouter>
      </Provider>
    </QueryClientProvider>
  )

  return {
    store: testStore,
    queryClient,
    ...render(ui, { wrapper: Wrapper, ...renderOptions })
  }
}

// API mocking utilities
export const createMockApiResponse = <T>(data: T, delay = 0) => {
  return new Promise<T>((resolve) => {
    setTimeout(() => resolve(data), delay)
  })
}

export const createMockApiError = (message = 'Test error', status = 500) => {
  const error = new Error(message) as any
  error.status = status
  error.response = { status, data: { message } }
  return Promise.reject(error)
}

// Mock API handlers
export const mockApiHandlers = {
  // Auth handlers
  login: jest.fn(() => createMockApiResponse({
    success: true,
    accessToken: 'mock-token',
    refreshToken: 'mock-refresh',
    expiresAt: new Date(Date.now() + 3600000).toISOString(),
    user: mockUser
  })),

  getCurrentUser: jest.fn(() => createMockApiResponse(mockUser)),

  // Business data handlers
  getBusinessTables: jest.fn(() => createMockApiResponse([mockBusinessTable])),

  getBusinessGlossary: jest.fn(() => createMockApiResponse({
    terms: [],
    total: 0
  })),

  // Query handlers
  executeQuery: jest.fn(() => createMockApiResponse({
    sql: 'SELECT * FROM TestTable',
    results: [{ id: 1, name: 'Test' }],
    metadata: {
      executionTime: 1.0,
      rowCount: 1,
      columnCount: 2,
      queryComplexity: 'Simple',
      tablesUsed: ['TestTable'],
      estimatedCost: 0.01
    }
  })),

  getQueryHistory: jest.fn(() => createMockApiResponse({
    queries: [mockQueryHistory],
    total: 1,
    page: 1
  }))
}

// Test component wrapper
export const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false }
    }
  })

  return (
    <QueryClientProvider client={queryClient}>
      <Provider store={store}>
        <BrowserRouter>
          <ConfigProvider theme={antdTheme}>
            {children}
          </ConfigProvider>
        </BrowserRouter>
      </Provider>
    </QueryClientProvider>
  )
}

// Custom hooks for testing
export const useTestSetup = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false }
    }
  })

  const resetMocks = () => {
    Object.values(mockApiHandlers).forEach(handler => {
      if (jest.isMockFunction(handler)) {
        handler.mockClear()
      }
    })
    queryClient.clear()
  }

  return {
    queryClient,
    resetMocks,
    mockApiHandlers
  }
}

// Assertion helpers
export const waitForLoadingToFinish = async () => {
  const { findByTestId } = await import('@testing-library/react')
  // Wait for any loading spinners to disappear
  await new Promise(resolve => setTimeout(resolve, 100))
}

export const expectElementToBeVisible = async (testId: string) => {
  const { findByTestId } = await import('@testing-library/react')
  const element = await findByTestId(testId)
  expect(element).toBeInTheDocument()
  expect(element).toBeVisible()
}

export const expectElementToHaveText = async (testId: string, text: string) => {
  const { findByTestId } = await import('@testing-library/react')
  const element = await findByTestId(testId)
  expect(element).toHaveTextContent(text)
}

// Mock localStorage
export const mockLocalStorage = (() => {
  let store: Record<string, string> = {}

  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString()
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key]
    }),
    clear: jest.fn(() => {
      store = {}
    }),
    get length() {
      return Object.keys(store).length
    },
    key: jest.fn((index: number) => Object.keys(store)[index] || null)
  }
})()

// Mock sessionStorage
export const mockSessionStorage = (() => {
  let store: Record<string, string> = {}

  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString()
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key]
    }),
    clear: jest.fn(() => {
      store = {}
    }),
    get length() {
      return Object.keys(store).length
    },
    key: jest.fn((index: number) => Object.keys(store)[index] || null)
  }
})()

// Setup function for tests
export const setupTests = () => {
  // Mock localStorage
  Object.defineProperty(window, 'localStorage', {
    value: mockLocalStorage
  })

  // Mock sessionStorage
  Object.defineProperty(window, 'sessionStorage', {
    value: mockSessionStorage
  })

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
  })

  // Mock IntersectionObserver
  global.IntersectionObserver = jest.fn().mockImplementation(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn(),
  }))

  // Mock ResizeObserver
  global.ResizeObserver = jest.fn().mockImplementation(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn(),
  }))
}

// Export all utilities
export * from '@testing-library/react'
export { renderWithProviders as render }
