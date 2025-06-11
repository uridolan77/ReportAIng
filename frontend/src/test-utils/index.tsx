/**
 * Test Utilities
 * 
 * Comprehensive testing utilities for React components with providers,
 * mocks, and custom render functions for consistent testing.
 */

import React, { ReactElement } from 'react';
import { render, RenderOptions, RenderResult } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../contexts/ThemeContext';
import { DarkModeProvider } from '../components/advanced/DarkModeProvider';

// Mock data generators
export const mockUser = {
  id: '1',
  name: 'Test User',
  email: 'test@example.com',
  role: 'user',
  isAdmin: false,
};

export const mockAdminUser = {
  ...mockUser,
  role: 'admin',
  isAdmin: true,
};

export const mockQueryResult = {
  id: '1',
  query: 'SELECT * FROM test_table',
  data: [
    { id: 1, name: 'Test Item 1', value: 100 },
    { id: 2, name: 'Test Item 2', value: 200 },
    { id: 3, name: 'Test Item 3', value: 300 },
  ],
  columns: ['id', 'name', 'value'],
  executedAt: new Date().toISOString(),
  executionTime: 150,
  rowCount: 3,
  success: true,
};

export const mockChartData = [
  { x: 'Jan', y: 100, category: 'A' },
  { x: 'Feb', y: 150, category: 'A' },
  { x: 'Mar', y: 120, category: 'B' },
  { x: 'Apr', y: 180, category: 'B' },
];

// Test providers wrapper
interface TestProvidersProps {
  children: React.ReactNode;
  initialEntries?: string[];
  queryClient?: QueryClient;
  darkMode?: boolean;
}

const TestProviders: React.FC<TestProvidersProps> = ({
  children,
  initialEntries = ['/'],
  queryClient,
  darkMode = false,
}) => {
  const testQueryClient = queryClient || new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
    },
  });

  return (
    <BrowserRouter>
      <QueryClientProvider client={testQueryClient}>
        <ConfigProvider>
          <ThemeProvider>
            <DarkModeProvider defaultMode={darkMode ? 'dark' : 'light'}>
              {children}
            </DarkModeProvider>
          </ThemeProvider>
        </ConfigProvider>
      </QueryClientProvider>
    </BrowserRouter>
  );
};

// Custom render function
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  initialEntries?: string[];
  queryClient?: QueryClient;
  darkMode?: boolean;
}

export const renderWithProviders = (
  ui: ReactElement,
  options: CustomRenderOptions = {}
): RenderResult => {
  const { initialEntries, queryClient, darkMode, ...renderOptions } = options;

  const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <TestProviders
      initialEntries={initialEntries}
      queryClient={queryClient}
      darkMode={darkMode}
    >
      {children}
    </TestProviders>
  );

  return render(ui, { wrapper: Wrapper, ...renderOptions });
};

// Mock hooks
export const mockUseCurrentResult = (hasResult = true, result = mockQueryResult) => ({
  hasResult,
  currentResult: hasResult ? result : null,
  isLoading: false,
  error: null,
  clearResult: jest.fn(),
  setResult: jest.fn(),
});

export const mockUseAuthStore = (user = mockUser) => ({
  isAuthenticated: true,
  user,
  isAdmin: user.isAdmin,
  login: jest.fn(),
  logout: jest.fn(),
  updateUser: jest.fn(),
});

export const mockUseVisualizationStore = () => ({
  charts: [],
  selectedChart: null,
  addChart: jest.fn(),
  updateChart: jest.fn(),
  removeChart: jest.fn(),
  selectChart: jest.fn(),
  currentVisualization: null,
  setVisualization: jest.fn(),
});

// Mock API responses
export const mockApiResponse = <T>(data: T, delay = 0): Promise<T> => {
  return new Promise((resolve) => {
    setTimeout(() => resolve(data), delay);
  });
};

export const mockApiError = (message = 'API Error', delay = 0): Promise<never> => {
  return new Promise((_, reject) => {
    setTimeout(() => reject(new Error(message)), delay);
  });
};

// Component testing helpers
export const expectElementToBeVisible = (element: HTMLElement) => {
  expect(element).toBeInTheDocument();
  expect(element).toBeVisible();
};

export const expectElementToHaveText = (element: HTMLElement, text: string) => {
  expect(element).toBeInTheDocument();
  expect(element).toHaveTextContent(text);
};

export const expectButtonToBeClickable = (button: HTMLElement) => {
  expect(button).toBeInTheDocument();
  expect(button).toBeEnabled();
  expect(button).not.toHaveAttribute('aria-disabled', 'true');
};

// Async testing helpers
export const waitForLoadingToFinish = async () => {
  const { waitForElementToBeRemoved, queryByText } = await import('@testing-library/react');
  const loadingElement = queryByText(/loading/i);
  if (loadingElement) {
    await waitForElementToBeRemoved(loadingElement);
  }
};

export const waitForErrorToAppear = async (errorMessage: string) => {
  const { waitFor, getByText } = await import('@testing-library/react');
  await waitFor(() => {
    expect(getByText(errorMessage)).toBeInTheDocument();
  });
};

// Form testing helpers
export const fillFormField = async (fieldName: string, value: string) => {
  const { userEvent } = await import('@testing-library/user-event');
  const { getByLabelText } = await import('@testing-library/react');
  
  const field = getByLabelText(new RegExp(fieldName, 'i'));
  await userEvent.clear(field);
  await userEvent.type(field, value);
};

export const submitForm = async (formElement: HTMLElement) => {
  const { userEvent } = await import('@testing-library/user-event');
  await userEvent.click(formElement);
};

// Navigation testing helpers
export const expectToBeOnPage = (pathname: string) => {
  expect(window.location.pathname).toBe(pathname);
};

export const navigateToPage = async (linkText: string) => {
  const { userEvent } = await import('@testing-library/user-event');
  const { getByRole } = await import('@testing-library/react');
  
  const link = getByRole('link', { name: new RegExp(linkText, 'i') });
  await userEvent.click(link);
};

// Accessibility testing helpers
export const expectToHaveAccessibleName = (element: HTMLElement, name: string) => {
  expect(element).toHaveAccessibleName(name);
};

export const expectToHaveAriaLabel = (element: HTMLElement, label: string) => {
  expect(element).toHaveAttribute('aria-label', label);
};

export const expectToBeKeyboardAccessible = (element: HTMLElement) => {
  expect(element).toHaveAttribute('tabindex');
  expect(element).not.toHaveAttribute('tabindex', '-1');
};

// Performance testing helpers
export const measureRenderTime = async (renderFn: () => void): Promise<number> => {
  const start = performance.now();
  renderFn();
  const end = performance.now();
  return end - start;
};

export const expectRenderTimeToBeUnder = async (renderFn: () => void, maxTime: number) => {
  const renderTime = await measureRenderTime(renderFn);
  expect(renderTime).toBeLessThan(maxTime);
};

// Custom matchers (would be added to jest setup)
export const customMatchers = {
  toBeVisibleAndAccessible: (element: HTMLElement) => {
    const pass = element.offsetParent !== null && 
                 element.getAttribute('aria-hidden') !== 'true' &&
                 !element.hasAttribute('hidden');
    
    return {
      pass,
      message: () => pass 
        ? `Expected element not to be visible and accessible`
        : `Expected element to be visible and accessible`,
    };
  },
  
  toHaveValidAriaAttributes: (element: HTMLElement) => {
    const ariaAttributes = Array.from(element.attributes)
      .filter(attr => attr.name.startsWith('aria-'));
    
    const invalidAttributes = ariaAttributes.filter(attr => 
      attr.value === '' || attr.value === 'undefined'
    );
    
    const pass = invalidAttributes.length === 0;
    
    return {
      pass,
      message: () => pass
        ? `Expected element to have invalid ARIA attributes`
        : `Expected element to have valid ARIA attributes, but found: ${invalidAttributes.map(attr => attr.name).join(', ')}`,
    };
  },
};

// Re-export testing library utilities
export * from '@testing-library/react';
export { default as userEvent } from '@testing-library/user-event';
