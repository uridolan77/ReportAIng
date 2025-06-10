/**
 * QueryShortcuts Component Tests
 * Comprehensive tests for the QueryShortcuts component using the testing utilities
 */

import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryShortcuts } from '../QueryShortcuts';
import { 
  renderWithProviders, 
  createMockTemplate, 
  createMockShortcut,
  setupTestEnvironment 
} from '../../../test-utils/testing-providers';
import {
  QueryInterfaceTestUtils,
  TemplateTestUtils
} from '../../../test-utils/component-test-utils';
import { queryTemplateService } from '../../../services/queryTemplateService';

// Setup test environment
setupTestEnvironment();

// Mock the query template service
jest.mock('../../../services/queryTemplateService', () => ({
  queryTemplateService: {
    getTemplates: jest.fn(),
    getShortcuts: jest.fn(),
    searchSuggestions: jest.fn(),
    createTemplate: jest.fn(),
    createShortcut: jest.fn(),
    toggleFavorite: jest.fn(),
    incrementUsage: jest.fn(),
    processTemplate: jest.fn(),
    addToRecent: jest.fn(),
    getCategories: jest.fn(),
    getPopularTemplates: jest.fn(),
    getFavoriteTemplates: jest.fn(),
  },
}));

const mockQueryTemplateService = queryTemplateService as jest.Mocked<typeof queryTemplateService>;

describe('QueryShortcuts Component', () => {
  const mockProps = {
    onQuerySelect: jest.fn(),
    onTemplateSelect: jest.fn(),
    currentQuery: '',
  };

  const mockTemplates = [
    createMockTemplate({
      id: 'template-1',
      name: 'Revenue Analysis',
      category: 'financial',
      variables: [
        {
          name: 'startDate',
          type: 'date' as const,
          description: 'Start date',
          required: true,
          defaultValue: '2024-01-01',
        },
      ],
    }),
    createMockTemplate({
      id: 'template-2',
      name: 'User Activity',
      category: 'operational',
      variables: [],
    }),
  ];

  const mockShortcuts = [
    createMockShortcut({
      id: 'shortcut-1',
      name: 'Today Revenue',
      shortcut: 'rev',
      query: 'Show me revenue for today',
    }),
    createMockShortcut({
      id: 'shortcut-2',
      name: 'Active Users',
      shortcut: 'users',
      query: 'Show me active users',
    }),
  ];

  beforeEach(() => {
    jest.clearAllMocks();
    
    // Setup default mock returns
    mockQueryTemplateService.getTemplates.mockReturnValue(mockTemplates);
    mockQueryTemplateService.getShortcuts.mockReturnValue(mockShortcuts);
    mockQueryTemplateService.getCategories.mockReturnValue(['financial', 'operational']);
    mockQueryTemplateService.getPopularTemplates.mockReturnValue([mockTemplates[0]]);
    mockQueryTemplateService.getFavoriteTemplates.mockReturnValue([]);
    mockQueryTemplateService.searchSuggestions.mockReturnValue([]);
  });

  describe('Rendering', () => {
    it('renders the shortcuts panel with all tabs', () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      expect(screen.getByText('Query Shortcuts & Templates')).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /suggestions/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /templates/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /shortcuts/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /popular/i })).toBeInTheDocument();
    });

    it('renders search input', () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      const searchInput = screen.getByPlaceholderText(/search templates/i);
      expect(searchInput).toBeInTheDocument();
    });

    it('renders create button', () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      const createButton = screen.getByRole('button', { name: /create/i });
      expect(createButton).toBeInTheDocument();
    });
  });

  describe('Templates Tab', () => {
    it('displays templates when templates tab is active', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await TemplateTestUtils.filterByCategory('all');

      await QueryInterfaceTestUtils.selectTemplate('templates');

      expect(screen.getByText('Revenue Analysis')).toBeInTheDocument();
      expect(screen.getByText('User Activity')).toBeInTheDocument();
    });

    it('filters templates by category', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      // Navigate to templates tab
      await QueryInterfaceTestUtils.selectTemplate('templates');

      // Filter by financial category
      await TemplateTestUtils.filterByCategory('financial');

      // Should show only financial templates
      expect(screen.getByText('Revenue Analysis')).toBeInTheDocument();
      expect(screen.queryByText('User Activity')).not.toBeInTheDocument();
    });

    it('handles template selection without variables', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('templates');

      // Click on template without variables
      await TemplateTestUtils.searchTemplates('User Activity');
      await QueryInterfaceTestUtils.selectTemplate('User Activity');

      expect(mockProps.onQuerySelect).toHaveBeenCalledWith(mockTemplates[1].template);
      expect(mockQueryTemplateService.incrementUsage).toHaveBeenCalledWith('template-2', 'template');
    });

    it('opens modal for template with variables', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('templates');

      // Click on template with variables
      await QueryInterfaceTestUtils.selectTemplate('Revenue Analysis');

      // Should open modal
      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });

      expect(screen.getByText('Revenue Analysis')).toBeInTheDocument();
      expect(screen.getByLabelText(/start date/i)).toBeInTheDocument();
    });

    it('processes template with variables', async () => {
      mockQueryTemplateService.processTemplate.mockReturnValue('Show me revenue from 2024-01-01 to 2024-12-31');

      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('templates');

      // Open template modal
      await QueryInterfaceTestUtils.selectTemplate('Revenue Analysis');

      // Fill variables
      await QueryInterfaceTestUtils.fillTemplateVariable('startDate', '2024-01-01');

      // Apply template
      await QueryInterfaceTestUtils.applyTemplate();

      expect(mockQueryTemplateService.processTemplate).toHaveBeenCalledWith(
        mockTemplates[0],
        expect.objectContaining({ startDate: '2024-01-01' })
      );
      expect(mockProps.onQuerySelect).toHaveBeenCalledWith('Show me revenue from 2024-01-01 to 2024-12-31');
    });

    it('toggles template favorite status', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('templates');

      // Toggle favorite
      await TemplateTestUtils.toggleFavorite('Revenue Analysis');

      expect(mockQueryTemplateService.toggleFavorite).toHaveBeenCalledWith('template-1');
    });
  });

  describe('Shortcuts Tab', () => {
    it('displays shortcuts when shortcuts tab is active', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('shortcuts');

      expect(screen.getByText('Today Revenue')).toBeInTheDocument();
      expect(screen.getByText('Active Users')).toBeInTheDocument();
      expect(screen.getByText('rev')).toBeInTheDocument();
      expect(screen.getByText('users')).toBeInTheDocument();
    });

    it('handles shortcut selection', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('shortcuts');

      // Click on shortcut
      await QueryInterfaceTestUtils.selectTemplate('Today Revenue');

      expect(mockProps.onQuerySelect).toHaveBeenCalledWith('Show me revenue for today');
      expect(mockQueryTemplateService.incrementUsage).toHaveBeenCalledWith('shortcut-1', 'shortcut');
      expect(mockQueryTemplateService.addToRecent).toHaveBeenCalledWith('Show me revenue for today');
    });
  });

  describe('Suggestions Tab', () => {
    it('shows suggestions based on search term', async () => {
      const mockSuggestions = [
        {
          id: 'suggestion-1',
          text: 'Show me revenue for today',
          type: 'shortcut' as const,
          category: 'financial',
          confidence: 0.9,
          description: 'Revenue shortcut',
        },
      ];

      mockQueryTemplateService.searchSuggestions.mockReturnValue(mockSuggestions);

      renderWithProviders(<QueryShortcuts {...mockProps} />);

      // Type in search
      await TemplateTestUtils.searchTemplates('revenue');

      expect(mockQueryTemplateService.searchSuggestions).toHaveBeenCalledWith('revenue');
      
      // Should show suggestions
      await waitFor(() => {
        expect(screen.getByText('Revenue shortcut')).toBeInTheDocument();
      });
    });

    it('handles suggestion selection', async () => {
      const mockSuggestions = [
        {
          id: 'suggestion-1',
          text: 'Show me revenue for today',
          type: 'shortcut' as const,
          category: 'financial',
          confidence: 0.9,
          description: 'Revenue shortcut',
        },
      ];

      mockQueryTemplateService.searchSuggestions.mockReturnValue(mockSuggestions);

      renderWithProviders(<QueryShortcuts {...mockProps} />);

      // Type in search to show suggestions
      await TemplateTestUtils.searchTemplates('revenue');

      // Click on suggestion
      await waitFor(() => {
        screen.getByText('Revenue shortcut');
        return QueryInterfaceTestUtils.selectTemplate('Revenue shortcut');
      });

      expect(mockProps.onQuerySelect).toHaveBeenCalledWith('Show me revenue for today');
      expect(mockQueryTemplateService.addToRecent).toHaveBeenCalledWith('Show me revenue for today');
    });

    it('shows empty state when no suggestions', () => {
      mockQueryTemplateService.searchSuggestions.mockReturnValue([]);

      renderWithProviders(<QueryShortcuts {...mockProps} />);

      expect(screen.getByText(/start typing to see suggestions/i)).toBeInTheDocument();
    });
  });

  describe('Popular Tab', () => {
    it('displays popular templates', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('popular');

      expect(screen.getByText('Revenue Analysis')).toBeInTheDocument();
    });

    it('shows empty state when no popular templates', async () => {
      mockQueryTemplateService.getPopularTemplates.mockReturnValue([]);

      renderWithProviders(<QueryShortcuts {...mockProps} />);

      await QueryInterfaceTestUtils.selectTemplate('popular');

      expect(screen.getByText(/no popular templates yet/i)).toBeInTheDocument();
    });
  });

  describe('Search Functionality', () => {
    it('updates suggestions when search term changes', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      // searchInput variable removed - not used in this test
      
      // Type search term
      await TemplateTestUtils.searchTemplates('revenue');

      expect(mockQueryTemplateService.searchSuggestions).toHaveBeenCalledWith('revenue');
    });

    it('clears search when input is cleared', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      // searchInput variable removed - not used in this test
      
      // Type and then clear
      await TemplateTestUtils.searchTemplates('revenue');
      await TemplateTestUtils.searchTemplates('');

      expect(mockQueryTemplateService.searchSuggestions).toHaveBeenLastCalledWith('');
    });
  });

  describe('Error Handling', () => {
    it('handles service errors gracefully', () => {
      mockQueryTemplateService.getTemplates.mockImplementation(() => {
        throw new Error('Service error');
      });

      // Should not crash
      expect(() => {
        renderWithProviders(<QueryShortcuts {...mockProps} />);
      }).not.toThrow();
    });
  });

  describe('Accessibility', () => {
    it('has proper ARIA labels', () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      const searchInput = screen.getByPlaceholderText(/search templates/i);
      expect(searchInput).toHaveAttribute('aria-label');

      const tabs = screen.getAllByRole('tab');
      tabs.forEach(tab => {
        expect(tab).toHaveAttribute('aria-selected');
      });
    });

    it('supports keyboard navigation', async () => {
      renderWithProviders(<QueryShortcuts {...mockProps} />);

      const firstTab = screen.getByRole('tab', { name: /suggestions/i });
      await userEvent.click(firstTab);

      // Tab should be focused after click
      expect(firstTab).toHaveFocus();
    });
  });
});
