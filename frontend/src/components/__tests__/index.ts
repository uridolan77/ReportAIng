/**
 * Centralized Test Exports
 * Consolidates all component tests for better organization
 */

// Test utilities
export * from '../../test-utils/testing-providers';
export * from '../../test-utils/component-test-utils';
export * from '../../test-utils/customMatchers';

// Component test suites
export { default as DataTableTests } from '../DataTable/__tests__/DataTable.test';
export { default as QueryShortcutsTests } from '../QueryInterface/__tests__/QueryShortcuts.test';

// Test categories for organized test running
export const testCategories = {
  unit: [
    'DataTable',
    'QueryInterface',
    'Visualization',
    'Layout',
    'Common'
  ],
  integration: [
    'QueryFlow',
    'DataProcessing',
    'UserInteraction',
    'APIIntegration'
  ],
  performance: [
    'VirtualScrolling',
    'DataTablePerformance',
    'ChartRendering',
    'MemoryUsage'
  ],
  accessibility: [
    'KeyboardNavigation',
    'ScreenReader',
    'ColorContrast',
    'FocusManagement'
  ]
} as const;

// Test configuration for different environments
export const testConfig = {
  development: {
    timeout: 10000,
    retries: 2,
    verbose: true
  },
  ci: {
    timeout: 30000,
    retries: 3,
    verbose: false
  },
  production: {
    timeout: 5000,
    retries: 1,
    verbose: false
  }
} as const;

// Common test data factories
export const testDataFactories = {
  createMockUser: () => ({
    id: '1',
    name: 'Test User',
    email: 'test@example.com',
    role: 'user'
  }),
  
  createMockQuery: () => ({
    id: '1',
    text: 'SELECT * FROM users',
    timestamp: new Date().toISOString(),
    status: 'completed'
  }),
  
  createMockData: (count: number = 10) => 
    Array.from({ length: count }, (_, i) => ({
      id: i + 1,
      name: `Item ${i + 1}`,
      value: Math.random() * 100,
      category: ['A', 'B', 'C'][i % 3]
    })),
    
  createMockColumns: () => [
    { key: 'id', title: 'ID', dataType: 'number' },
    { key: 'name', title: 'Name', dataType: 'string' },
    { key: 'value', title: 'Value', dataType: 'number' },
    { key: 'category', title: 'Category', dataType: 'string' }
  ]
} as const;

// Test helpers for common operations
export const testHelpers = {
  waitForDataLoad: async (timeout: number = 5000) => {
    await waitFor(() => {
      expect(screen.queryByTestId('loading-spinner')).not.toBeInTheDocument();
    }, { timeout });
  },
  
  expectNoErrors: () => {
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    expect(console.error).not.toHaveBeenCalled();
  },
  
  expectAccessibleComponent: (element: HTMLElement) => {
    expect(element).toBeAccessible();
    expect(element).toHaveCorrectAriaAttributes();
  },
  
  expectPerformantComponent: (renderTime: number, threshold: number = 100) => {
    expect(renderTime).toBeWithinPerformanceThreshold(threshold);
  }
} as const;
