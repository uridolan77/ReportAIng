/**
 * UserContextPanel Integration Tests
 * 
 * Tests the UserContextPanel component integration with API services,
 * performance optimizations, and user interactions.
 */

import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import UserContextPanel from '../../components/AI/UserContextPanel';
import { ApiService } from '../../services/api';

// Mock the API service
jest.mock('../../services/api', () => ({
  ApiService: {
    getUserContext: jest.fn(),
  },
}));

const mockApiService = ApiService as jest.Mocked<typeof ApiService>;

// Test wrapper component
const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        cacheTime: 0,
      },
    },
  });

  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

// Mock user context data
const mockUserContext = {
  domain: 'Sales',
  lastUpdated: '2024-01-15T10:30:00Z',
  preferredTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players', 'tbl_Countries'],
  commonFilters: ['Active players', 'Last 30 days', 'Deposit > 100'],
  recentPatterns: [
    {
      pattern: 'Show me active players with deposits',
      intent: 'filtering',
      frequency: 15,
      lastUsed: '2024-01-15T09:45:00Z',
      associatedTables: ['tbl_Daily_actions_players', 'tbl_Daily_actions'],
    },
    {
      pattern: 'Revenue trends by country',
      intent: 'trend',
      frequency: 8,
      lastUsed: '2024-01-14T16:20:00Z',
      associatedTables: ['tbl_Daily_actions', 'tbl_Countries'],
    },
  ],
};

describe('UserContextPanel Integration Tests', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Loading States', () => {
    it('should show loading spinner while fetching data', async () => {
      // Mock a delayed API response
      mockApiService.getUserContext.mockImplementation(
        () => new Promise(resolve => setTimeout(() => resolve(mockUserContext), 100))
      );

      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      // Should show loading spinner initially
      expect(screen.getByRole('img', { name: /loading/i })).toBeInTheDocument();

      // Wait for data to load
      await waitFor(() => {
        expect(screen.getByText('Your AI Profile')).toBeInTheDocument();
      });

      // Loading spinner should be gone
      expect(screen.queryByRole('img', { name: /loading/i })).not.toBeInTheDocument();
    });

    it('should handle API errors gracefully', async () => {
      // Mock API error
      mockApiService.getUserContext.mockRejectedValue({
        response: {
          data: {
            message: 'Failed to fetch user context',
          },
        },
      });

      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      // Wait for error to be displayed
      await waitFor(() => {
        expect(screen.getByText('Failed to fetch user context')).toBeInTheDocument();
      });

      // Should show error alert
      expect(screen.getByRole('alert')).toBeInTheDocument();
    });
  });

  describe('Data Display', () => {
    beforeEach(() => {
      mockApiService.getUserContext.mockResolvedValue(mockUserContext);
    });

    it('should display user context data correctly', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Your AI Profile')).toBeInTheDocument();
      });

      // Check domain display
      expect(screen.getByText('Sales')).toBeInTheDocument();
      expect(screen.getByText('Domain Focus')).toBeInTheDocument();

      // Check last updated date
      expect(screen.getByText('Last Updated')).toBeInTheDocument();
    });

    it('should display preferred tables', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Preferred Tables')).toBeInTheDocument();
      });

      // Check if preferred tables are displayed
      expect(screen.getByText('tbl_Daily_actions')).toBeInTheDocument();
      expect(screen.getByText('tbl_Daily_actions_players')).toBeInTheDocument();
      expect(screen.getByText('tbl_Countries')).toBeInTheDocument();
    });

    it('should display common filters', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Common Filters')).toBeInTheDocument();
      });

      // Check if common filters are displayed
      expect(screen.getByText('Active players')).toBeInTheDocument();
      expect(screen.getByText('Last 30 days')).toBeInTheDocument();
      expect(screen.getByText('Deposit > 100')).toBeInTheDocument();
    });

    it('should display query patterns', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Query Patterns')).toBeInTheDocument();
      });

      // Check if query patterns are displayed
      expect(screen.getByText('Show me active players with deposits')).toBeInTheDocument();
      expect(screen.getByText('Revenue trends by country')).toBeInTheDocument();
      expect(screen.getByText('15x')).toBeInTheDocument();
      expect(screen.getByText('8x')).toBeInTheDocument();
    });
  });

  describe('Interactive Elements', () => {
    beforeEach(() => {
      mockApiService.getUserContext.mockResolvedValue(mockUserContext);
    });

    it('should expand and collapse sections', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Preferred Tables')).toBeInTheDocument();
      });

      // Find and click the preferred tables collapse header
      const preferredTablesHeader = screen.getByText('Preferred Tables');
      fireEvent.click(preferredTablesHeader);

      // Content should be visible after clicking
      await waitFor(() => {
        expect(screen.getByText('tbl_Daily_actions')).toBeInTheDocument();
      });
    });

    it('should handle empty state correctly', async () => {
      // Mock empty user context
      const emptyContext = {
        domain: 'General',
        lastUpdated: '2024-01-15T10:30:00Z',
        preferredTables: [],
        commonFilters: [],
        recentPatterns: [],
      };

      mockApiService.getUserContext.mockResolvedValue(emptyContext);

      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Start Building Your AI Profile')).toBeInTheDocument();
      });

      // Should show empty state message
      expect(screen.getByText(/Ask questions and execute queries/i)).toBeInTheDocument();
    });
  });

  describe('Performance Optimizations', () => {
    beforeEach(() => {
      mockApiService.getUserContext.mockResolvedValue(mockUserContext);
    });

    it('should render efficiently with large datasets', async () => {
      // Mock large dataset
      const largeContext = {
        ...mockUserContext,
        preferredTables: Array.from({ length: 100 }, (_, i) => `table_${i}`),
        commonFilters: Array.from({ length: 50 }, (_, i) => `filter_${i}`),
        recentPatterns: Array.from({ length: 200 }, (_, i) => ({
          pattern: `Pattern ${i}`,
          intent: 'filtering',
          frequency: Math.floor(Math.random() * 20) + 1,
          lastUsed: '2024-01-15T09:45:00Z',
          associatedTables: [`table_${i}`, `table_${i + 1}`],
        })),
      };

      mockApiService.getUserContext.mockResolvedValue(largeContext);

      const startTime = performance.now();

      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Your AI Profile')).toBeInTheDocument();
      });

      const renderTime = performance.now() - startTime;

      // Should render large datasets efficiently (under 1 second)
      expect(renderTime).toBeLessThan(1000);
    });

    it('should handle rapid state updates efficiently', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Your AI Profile')).toBeInTheDocument();
      });

      // Simulate rapid interactions
      const startTime = performance.now();

      for (let i = 0; i < 10; i++) {
        const collapseHeaders = screen.getAllByRole('button');
        if (collapseHeaders.length > 0) {
          fireEvent.click(collapseHeaders[0]);
          await new Promise(resolve => setTimeout(resolve, 10));
        }
      }

      const interactionTime = performance.now() - startTime;

      // Rapid interactions should be handled efficiently
      expect(interactionTime).toBeLessThan(500);
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      mockApiService.getUserContext.mockResolvedValue(mockUserContext);
    });

    it('should have proper ARIA attributes', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Your AI Profile')).toBeInTheDocument();
      });

      // Check for proper heading structure
      expect(screen.getByRole('heading', { level: 3 })).toBeInTheDocument();

      // Check for proper button roles
      const buttons = screen.getAllByRole('button');
      expect(buttons.length).toBeGreaterThan(0);

      // Check for proper list structure
      const lists = screen.getAllByRole('list');
      expect(lists.length).toBeGreaterThan(0);
    });

    it('should support keyboard navigation', async () => {
      render(
        <TestWrapper>
          <UserContextPanel />
        </TestWrapper>
      );

      await waitFor(() => {
        expect(screen.getByText('Your AI Profile')).toBeInTheDocument();
      });

      // Test keyboard navigation on collapse panels
      const buttons = screen.getAllByRole('button');
      expect(buttons.length).toBeGreaterThan(0);

      const firstButton = buttons[0];
      firstButton.focus();
      expect(firstButton).toHaveFocus();

      // Simulate Enter key press
      fireEvent.keyDown(firstButton, { key: 'Enter', code: 'Enter' });

      // Should handle keyboard interaction
      expect(firstButton).toBeInTheDocument();
    });
  });
});
