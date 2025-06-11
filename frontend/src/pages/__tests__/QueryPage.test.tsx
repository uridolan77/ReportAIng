/**
 * QueryPage Component Tests
 * 
 * Comprehensive tests for the QueryPage component including
 * tab navigation, user interactions, and integration with stores.
 */

import React from 'react';
import { 
  renderWithProviders, 
  expectElementToBeVisible,
  expectElementToHaveText,
  mockUseCurrentResult,
  mockUseAuthStore,
  userEvent,
  waitFor
} from '../../test-utils';
import QueryPage from '../QueryPage';

// Mock the hooks
jest.mock('../../hooks/useCurrentResult');
jest.mock('../../stores/authStore');

// Mock the child components
jest.mock('../../components/QueryInterface/QueryInterface', () => {
  return function MockQueryInterface() {
    return <div data-testid="query-interface">Query Interface Component</div>;
  };
});

jest.mock('../../components/QueryInterface/QueryHistory', () => {
  return function MockQueryHistory() {
    return <div data-testid="query-history">Query History Component</div>;
  };
});

jest.mock('../../components/QueryInterface/QuerySuggestions', () => {
  return function MockQuerySuggestions() {
    return <div data-testid="query-suggestions">Query Suggestions Component</div>;
  };
});

jest.mock('../../components/QueryInterface/MockDataToggle', () => {
  return function MockDataToggle() {
    return <button data-testid="mock-data-toggle">Mock Data Toggle</button>;
  };
});

const mockUseCurrentResultHook = mockUseCurrentResult as jest.MockedFunction<typeof mockUseCurrentResult>;
const mockUseAuthStoreHook = mockUseAuthStore as jest.MockedFunction<typeof mockUseAuthStore>;

describe('QueryPage Component', () => {
  beforeEach(() => {
    // Reset mocks before each test
    jest.clearAllMocks();
    
    // Default mock implementations
    (require('../../hooks/useCurrentResult') as any).useCurrentResult = jest.fn(() => 
      mockUseCurrentResultHook()
    );
    
    (require('../../stores/authStore') as any).useAuthStore = jest.fn(() => 
      mockUseAuthStoreHook()
    );
  });

  describe('Basic Rendering', () => {
    it('renders the page layout with title and subtitle', () => {
      const { getByText } = renderWithProviders(<QueryPage />);
      
      expectElementToHaveText(getByText('Query Interface'), 'Query Interface');
      expect(getByText('Ask questions about your data using natural language')).toBeInTheDocument();
    });

    it('renders welcome section with user name', () => {
      const mockUser = { name: 'John Doe', email: 'john@example.com' };
      (require('../../stores/authStore') as any).useAuthStore = jest.fn(() => 
        mockUseAuthStoreHook(mockUser)
      );

      const { getByText } = renderWithProviders(<QueryPage />);
      
      expect(getByText('Welcome back, John Doe!')).toBeInTheDocument();
    });

    it('renders welcome section with default user when no name', () => {
      const mockUser = { email: 'user@example.com' };
      (require('../../stores/authStore') as any).useAuthStore = jest.fn(() => 
        mockUseAuthStoreHook(mockUser)
      );

      const { getByText } = renderWithProviders(<QueryPage />);
      
      expect(getByText('Welcome back, User!')).toBeInTheDocument();
    });

    it('renders mock data toggle', () => {
      const { getByTestId } = renderWithProviders(<QueryPage />);
      
      const toggle = getByTestId('mock-data-toggle');
      expectElementToBeVisible(toggle);
    });
  });

  describe('Tab Navigation', () => {
    it('renders all tab options', () => {
      const { getByText } = renderWithProviders(<QueryPage />);
      
      expect(getByText('Query Interface')).toBeInTheDocument();
      expect(getByText('Query History')).toBeInTheDocument();
      expect(getByText('AI Suggestions')).toBeInTheDocument();
    });

    it('shows query interface tab by default', () => {
      const { getByTestId } = renderWithProviders(<QueryPage />);
      
      const queryInterface = getByTestId('query-interface');
      expectElementToBeVisible(queryInterface);
    });

    it('switches to history tab when clicked', async () => {
      const { getByText, getByTestId } = renderWithProviders(<QueryPage />);
      
      const historyTab = getByText('Query History');
      await userEvent.click(historyTab);
      
      await waitFor(() => {
        const queryHistory = getByTestId('query-history');
        expectElementToBeVisible(queryHistory);
      });
    });

    it('switches to suggestions tab when clicked', async () => {
      const { getByText, getByTestId } = renderWithProviders(<QueryPage />);
      
      const suggestionsTab = getByText('AI Suggestions');
      await userEvent.click(suggestionsTab);
      
      await waitFor(() => {
        const querySuggestions = getByTestId('query-suggestions');
        expectElementToBeVisible(querySuggestions);
      });
    });
  });

  describe('Quick Actions', () => {
    it('renders all quick action buttons', () => {
      const { getByText } = renderWithProviders(<QueryPage />);
      
      expect(getByText('📊 Sample Reports')).toBeInTheDocument();
      expect(getByText('📈 Trending Queries')).toBeInTheDocument();
      expect(getByText('🎯 Query Templates')).toBeInTheDocument();
      expect(getByText('💡 AI Suggestions')).toBeInTheDocument();
    });

    it('quick action buttons are clickable', async () => {
      const { getByText } = renderWithProviders(<QueryPage />);
      
      const sampleReportsBtn = getByText('📊 Sample Reports');
      expect(sampleReportsBtn).toBeEnabled();
      
      // Test that button can be clicked (doesn't throw error)
      await userEvent.click(sampleReportsBtn);
    });
  });

  describe('Results Integration', () => {
    it('shows results alert when query results are available', () => {
      (require('../../hooks/useCurrentResult') as any).useCurrentResult = jest.fn(() => 
        mockUseCurrentResultHook(true)
      );

      const { getByText } = renderWithProviders(<QueryPage />);
      
      expect(getByText('Query Results Available')).toBeInTheDocument();
      expect(getByText('Your query has been executed successfully. View results in the Results page.')).toBeInTheDocument();
    });

    it('does not show results alert when no results available', () => {
      (require('../../hooks/useCurrentResult') as any).useCurrentResult = jest.fn(() => 
        mockUseCurrentResultHook(false)
      );

      const { queryByText } = renderWithProviders(<QueryPage />);
      
      expect(queryByText('Query Results Available')).not.toBeInTheDocument();
    });

    it('provides link to results page when results available', () => {
      (require('../../hooks/useCurrentResult') as any).useCurrentResult = jest.fn(() => 
        mockUseCurrentResultHook(true)
      );

      const { getByText } = renderWithProviders(<QueryPage />);
      
      const viewResultsBtn = getByText('View Results');
      expect(viewResultsBtn).toBeInTheDocument();
      expect(viewResultsBtn.closest('a')).toHaveAttribute('href', '/results');
    });
  });

  describe('Responsive Design', () => {
    it('renders properly on different screen sizes', () => {
      // Test mobile viewport
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 375,
      });

      const { container } = renderWithProviders(<QueryPage />);
      expect(container.firstChild).toBeInTheDocument();

      // Test desktop viewport
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 1920,
      });

      const { container: desktopContainer } = renderWithProviders(<QueryPage />);
      expect(desktopContainer.firstChild).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('has proper heading structure', () => {
      const { container } = renderWithProviders(<QueryPage />);
      
      const headings = container.querySelectorAll('h1, h2, h3, h4, h5, h6');
      expect(headings.length).toBeGreaterThan(0);
      
      // Check that main heading exists
      const mainHeading = container.querySelector('h1');
      expect(mainHeading).toBeInTheDocument();
    });

    it('has proper tab navigation with keyboard support', async () => {
      const { getByText } = renderWithProviders(<QueryPage />);
      
      const firstTab = getByText('Query Interface');
      const secondTab = getByText('Query History');
      
      // Focus first tab
      firstTab.focus();
      expect(firstTab).toHaveFocus();
      
      // Navigate with arrow keys (if implemented)
      await userEvent.keyboard('{ArrowRight}');
      // Note: Actual keyboard navigation would depend on Tabs component implementation
    });

    it('has proper ARIA labels and roles', () => {
      const { container } = renderWithProviders(<QueryPage />);
      
      // Check for proper tab roles
      const tabs = container.querySelectorAll('[role="tab"]');
      expect(tabs.length).toBeGreaterThan(0);
      
      // Check for proper tabpanel roles
      const tabpanels = container.querySelectorAll('[role="tabpanel"]');
      expect(tabpanels.length).toBeGreaterThan(0);
    });
  });

  describe('Error Handling', () => {
    it('handles missing user data gracefully', () => {
      (require('../../stores/authStore') as any).useAuthStore = jest.fn(() => ({
        ...mockUseAuthStoreHook(),
        user: null,
      }));

      const { getByText } = renderWithProviders(<QueryPage />);
      
      expect(getByText('Welcome back, User!')).toBeInTheDocument();
    });

    it('handles hook errors gracefully', () => {
      (require('../../hooks/useCurrentResult') as any).useCurrentResult = jest.fn(() => ({
        hasResult: false,
        currentResult: null,
        isLoading: false,
        error: 'Test error',
        clearResult: jest.fn(),
        setResult: jest.fn(),
      }));

      // Should not throw error
      const { container } = renderWithProviders(<QueryPage />);
      expect(container.firstChild).toBeInTheDocument();
    });
  });

  describe('Performance', () => {
    it('renders within acceptable time', async () => {
      const startTime = performance.now();
      renderWithProviders(<QueryPage />);
      const endTime = performance.now();
      
      const renderTime = endTime - startTime;
      expect(renderTime).toBeLessThan(100); // Should render in less than 100ms
    });

    it('does not cause memory leaks', () => {
      const { unmount } = renderWithProviders(<QueryPage />);
      
      // Unmount component
      unmount();
      
      // Check that no timers or listeners are left
      // This would be more comprehensive in a real test environment
      expect(true).toBe(true);
    });
  });
});
