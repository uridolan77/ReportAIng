/**
 * Test Utilities
 * 
 * Comprehensive collection of testing utilities, mocks, and helpers
 * for React components, API calls, and integration testing.
 */

// ===== CORE TESTING LIBRARY EXPORTS =====
export * from '@testing-library/react';
export { default as userEvent } from '@testing-library/user-event';

// ===== COMPREHENSIVE TEST PROVIDERS =====
// Re-export from the comprehensive testing-providers file
export {
  renderWithProviders,
  TestProviders,
  MinimalTestProvider,
  QueryTestProvider,
  mockQueryClient,
  testTheme,
  testAntdConfig,
  mockApiService,
  mockWebSocketService,
  mockLocalStorage,
  mockSessionStorage,
  setupTestEnvironment
} from './testing-providers';

// ===== CUSTOM MATCHERS =====
// Re-export custom matchers
export {
  toBeValidQuery,
  toHaveValidTemplate,
  toBeAccessible,
  toHaveCorrectAriaAttributes,
  toBeWithinPerformanceThreshold,
  toHaveValidSecuritySignature,
  toMatchQueryPattern,
  toBeValidBusinessData
} from './customMatchers';

// ===== API TEST UTILITIES =====
// Re-export API testing utilities
export {
  ApiTestUtils,
  MockApiResponse,
  QueryClientTestUtils
} from './api-test-utils';

// Mock implementations for stores
export const mockAuthStore = () => ({
  isAuthenticated: false,
  user: null,
  token: null,
  refreshToken: null,
  login: jest.fn(),
  logout: jest.fn(),
  refreshAuth: jest.fn(),
});

export const mockQueryStore = () => ({
  queries: [],
  currentQuery: null,
  isLoading: false,
  error: null,
  addQuery: jest.fn(),
  updateQuery: jest.fn(),
  removeQuery: jest.fn(),
  setCurrentQuery: jest.fn(),
  executeQuery: jest.fn(),
  clearError: jest.fn(),
});

export const mockDashboardStore = () => ({
  dashboards: [],
  currentDashboard: null,
  isLoading: false,
  error: null,
  createDashboard: jest.fn(),
  updateDashboard: jest.fn(),
  deleteDashboard: jest.fn(),
  setCurrentDashboard: jest.fn(),
  addWidget: jest.fn(),
  updateWidget: jest.fn(),
  removeWidget: jest.fn(),
});

export const mockVisualizationStore = () => ({
  charts: [],
  currentChart: null,
  isLoading: false,
  error: null,
  createChart: jest.fn(),
  updateChart: jest.fn(),
  removeChart: jest.fn(),
  selectChart: jest.fn(),
  currentVisualization: null,
  setVisualization: jest.fn(),
});

// Mock API responses
export const mockApiResponse = function<T>(data: T, delay = 0): Promise<T> {
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

// ===== ACCESSIBILITY TESTING HELPERS =====
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


