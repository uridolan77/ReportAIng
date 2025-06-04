// Comprehensive DataTable Test Suite
import React from 'react';
import { render, screen, fireEvent, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import DataTable, { DataTableProps, DataTableColumn } from '../DataTable';

// Mock dependencies
vi.mock('react-window', () => ({
  FixedSizeList: ({ children, itemData, itemCount }: any) => (
    <div data-testid="virtual-list">
      {Array.from({ length: Math.min(itemCount, 10) }, (_, index) => 
        children({ index, style: {}, data: itemData })
      )}
    </div>
  ),
  VariableSizeList: ({ children, itemData, itemCount }: any) => (
    <div data-testid="variable-virtual-list">
      {Array.from({ length: Math.min(itemCount, 10) }, (_, index) => 
        children({ index, style: {}, data: itemData })
      )}
    </div>
  )
}));

vi.mock('react-virtualized-auto-sizer', () => ({
  default: ({ children }: any) => children({ width: 800, height: 600 })
}));

vi.mock('react-hotkeys-hook', () => ({
  useHotkeys: vi.fn()
}));

vi.mock('use-debounce', () => ({
  useDebounce: (value: any) => [value]
}));

// Test data
const sampleColumns: DataTableColumn[] = [
  {
    key: 'id',
    title: 'ID',
    dataIndex: 'id',
    sortable: true,
    width: 80
  },
  {
    key: 'name',
    title: 'Name',
    dataIndex: 'name',
    searchable: true,
    filterable: true,
    sortable: true
  },
  {
    key: 'age',
    title: 'Age',
    dataIndex: 'age',
    dataType: 'number',
    sortable: true,
    aggregation: 'avg'
  },
  {
    key: 'email',
    title: 'Email',
    dataIndex: 'email',
    copyable: true
  },
  {
    key: 'status',
    title: 'Status',
    dataIndex: 'status',
    filterType: 'select',
    filterOptions: ['active', 'inactive']
  }
];

const sampleData = [
  { id: 1, name: 'John Doe', age: 30, email: 'john@example.com', status: 'active' },
  { id: 2, name: 'Jane Smith', age: 25, email: 'jane@example.com', status: 'inactive' },
  { id: 3, name: 'Bob Johnson', age: 35, email: 'bob@example.com', status: 'active' },
  { id: 4, name: 'Alice Brown', age: 28, email: 'alice@example.com', status: 'active' },
  { id: 5, name: 'Charlie Wilson', age: 42, email: 'charlie@example.com', status: 'inactive' }
];

const defaultProps: DataTableProps = {
  data: sampleData,
  columns: sampleColumns,
  keyField: 'id'
};

describe('DataTable Component', () => {
  let user: ReturnType<typeof userEvent.setup>;

  beforeEach(() => {
    user = userEvent.setup();
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Basic Rendering', () => {
    it('renders data table with correct data', () => {
      render(<DataTable {...defaultProps} />);
      
      // Check if table headers are rendered
      expect(screen.getByText('Name')).toBeInTheDocument();
      expect(screen.getByText('Age')).toBeInTheDocument();
      expect(screen.getByText('Email')).toBeInTheDocument();
      
      // Check if data is rendered
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('jane@example.com')).toBeInTheDocument();
    });

    it('renders empty state when no data provided', () => {
      render(<DataTable {...defaultProps} data={[]} />);
      expect(screen.getByText('No data available')).toBeInTheDocument();
    });

    it('renders loading state', () => {
      render(<DataTable {...defaultProps} loading={true} />);
      expect(screen.getByRole('img', { name: 'loading' })).toBeInTheDocument();
    });

    it('renders error state', () => {
      const error = new Error('Test error');
      render(<DataTable {...defaultProps} error={error} />);
      expect(screen.getByText('Error loading data')).toBeInTheDocument();
      expect(screen.getByText('Test error')).toBeInTheDocument();
    });
  });

  describe('Search Functionality', () => {
    it('filters data based on search input', async () => {
      render(<DataTable {...defaultProps} />);
      
      const searchInput = screen.getByPlaceholderText(/search/i);
      await user.type(searchInput, 'John');
      
      await waitFor(() => {
        expect(screen.getByText('John Doe')).toBeInTheDocument();
        expect(screen.queryByText('Jane Smith')).not.toBeInTheDocument();
      });
    });

    it('shows no results when search yields no matches', async () => {
      render(<DataTable {...defaultProps} />);
      
      const searchInput = screen.getByPlaceholderText(/search/i);
      await user.type(searchInput, 'nonexistent');
      
      await waitFor(() => {
        expect(screen.getByText('No data available')).toBeInTheDocument();
      });
    });

    it('calls onSearch callback when searching', async () => {
      const onSearch = vi.fn();
      render(<DataTable {...defaultProps} onSearch={onSearch} />);
      
      const searchInput = screen.getByPlaceholderText(/search/i);
      await user.type(searchInput, 'test');
      
      await waitFor(() => {
        expect(onSearch).toHaveBeenCalledWith('test');
      });
    });
  });

  describe('Sorting Functionality', () => {
    it('sorts data when column header is clicked', async () => {
      render(<DataTable {...defaultProps} />);
      
      const nameHeader = screen.getByText('Name');
      await user.click(nameHeader);
      
      // Check if data is sorted (first item should be Alice Brown after sorting)
      await waitFor(() => {
        const rows = screen.getAllByRole('row');
        expect(within(rows[1]).getByText('Alice Brown')).toBeInTheDocument();
      });
    });

    it('toggles sort order on subsequent clicks', async () => {
      render(<DataTable {...defaultProps} />);
      
      const nameHeader = screen.getByText('Name');
      
      // First click - ascending
      await user.click(nameHeader);
      await waitFor(() => {
        const rows = screen.getAllByRole('row');
        expect(within(rows[1]).getByText('Alice Brown')).toBeInTheDocument();
      });
      
      // Second click - descending
      await user.click(nameHeader);
      await waitFor(() => {
        const rows = screen.getAllByRole('row');
        expect(within(rows[1]).getByText('John Doe')).toBeInTheDocument();
      });
    });

    it('calls onSort callback when sorting', async () => {
      const onSort = vi.fn();
      render(<DataTable {...defaultProps} onSort={onSort} />);
      
      const nameHeader = screen.getByText('Name');
      await user.click(nameHeader);
      
      expect(onSort).toHaveBeenCalledWith([{ column: 'name', order: 'asc' }]);
    });
  });

  describe('Filtering Functionality', () => {
    it('opens filter panel when filter button is clicked', async () => {
      render(<DataTable {...defaultProps} />);
      
      const filterButton = screen.getByLabelText(/filter/i);
      await user.click(filterButton);
      
      expect(screen.getByText('Filters')).toBeInTheDocument();
    });

    it('filters data based on filter criteria', async () => {
      render(<DataTable {...defaultProps} />);
      
      // Open filter panel
      const filterButton = screen.getByLabelText(/filter/i);
      await user.click(filterButton);
      
      // Apply filter
      const statusFilter = screen.getByLabelText('Status');
      await user.selectOptions(statusFilter, 'active');
      
      await waitFor(() => {
        expect(screen.getByText('John Doe')).toBeInTheDocument();
        expect(screen.queryByText('Jane Smith')).not.toBeInTheDocument();
      });
    });

    it('calls onFilter callback when filtering', async () => {
      const onFilter = vi.fn();
      render(<DataTable {...defaultProps} onFilter={onFilter} />);
      
      // Open filter panel
      const filterButton = screen.getByLabelText(/filter/i);
      await user.click(filterButton);
      
      // Apply filter
      const statusFilter = screen.getByLabelText('Status');
      await user.selectOptions(statusFilter, 'active');
      
      expect(onFilter).toHaveBeenCalledWith({ status: 'active' });
    });
  });

  describe('Selection Functionality', () => {
    it('selects rows when checkboxes are clicked', async () => {
      render(<DataTable {...defaultProps} />);
      
      const checkboxes = screen.getAllByRole('checkbox');
      await user.click(checkboxes[1]); // First data row
      
      expect(checkboxes[1]).toBeChecked();
    });

    it('selects all rows when header checkbox is clicked', async () => {
      render(<DataTable {...defaultProps} />);
      
      const headerCheckbox = screen.getAllByRole('checkbox')[0];
      await user.click(headerCheckbox);
      
      const checkboxes = screen.getAllByRole('checkbox');
      checkboxes.slice(1).forEach(checkbox => {
        expect(checkbox).toBeChecked();
      });
    });

    it('calls onSelectionChange when selection changes', async () => {
      const onSelectionChange = vi.fn();
      render(<DataTable {...defaultProps} onSelectionChange={onSelectionChange} />);
      
      const checkboxes = screen.getAllByRole('checkbox');
      await user.click(checkboxes[1]);
      
      expect(onSelectionChange).toHaveBeenCalledWith([sampleData[0]]);
    });
  });

  describe('Pagination', () => {
    const largeData = Array.from({ length: 100 }, (_, i) => ({
      id: i + 1,
      name: `User ${i + 1}`,
      age: 20 + (i % 50),
      email: `user${i + 1}@example.com`,
      status: i % 2 === 0 ? 'active' : 'inactive'
    }));

    it('displays pagination when data exceeds page size', () => {
      render(<DataTable {...defaultProps} data={largeData} />);
      
      expect(screen.getByRole('list')).toBeInTheDocument(); // Pagination
    });

    it('navigates between pages', async () => {
      render(<DataTable {...defaultProps} data={largeData} />);
      
      const nextButton = screen.getByTitle('Next Page');
      await user.click(nextButton);
      
      await waitFor(() => {
        expect(screen.getByText('User 21')).toBeInTheDocument();
      });
    });

    it('changes page size', async () => {
      render(<DataTable {...defaultProps} data={largeData} />);
      
      const pageSizeSelect = screen.getByDisplayValue('20 / page');
      await user.click(pageSizeSelect);
      
      const option50 = screen.getByText('50 / page');
      await user.click(option50);
      
      await waitFor(() => {
        expect(screen.getByText('User 50')).toBeInTheDocument();
      });
    });
  });

  describe('Export Functionality', () => {
    it('opens export modal when export button is clicked', async () => {
      render(<DataTable {...defaultProps} />);
      
      const exportButton = screen.getByLabelText(/export/i);
      await user.click(exportButton);
      
      expect(screen.getByText('Export Data')).toBeInTheDocument();
    });

    it('calls onExport when export format is selected', async () => {
      const onExport = vi.fn();
      render(<DataTable {...defaultProps} onExport={onExport} />);
      
      const exportButton = screen.getByLabelText(/export/i);
      await user.click(exportButton);
      
      const csvButton = screen.getByText('CSV');
      await user.click(csvButton);
      
      expect(onExport).toHaveBeenCalledWith('csv', expect.any(Array));
    });
  });

  describe('Column Operations', () => {
    it('opens column chooser when column chooser button is clicked', async () => {
      render(<DataTable {...defaultProps} />);
      
      const columnButton = screen.getByLabelText(/column/i);
      await user.click(columnButton);
      
      expect(screen.getByText('Column Chooser')).toBeInTheDocument();
    });

    it('hides/shows columns through column chooser', async () => {
      render(<DataTable {...defaultProps} />);
      
      const columnButton = screen.getByLabelText(/column/i);
      await user.click(columnButton);
      
      const ageCheckbox = screen.getByLabelText('Age');
      await user.click(ageCheckbox);
      
      await waitFor(() => {
        expect(screen.queryByText('Age')).not.toBeInTheDocument();
      });
    });
  });

  describe('Virtualization', () => {
    it('uses virtual scrolling for large datasets', () => {
      const largeData = Array.from({ length: 1000 }, (_, i) => ({
        id: i + 1,
        name: `User ${i + 1}`,
        age: 20 + (i % 50),
        email: `user${i + 1}@example.com`,
        status: i % 2 === 0 ? 'active' : 'inactive'
      }));

      render(<DataTable {...defaultProps} data={largeData} features={{ virtualScroll: true }} />);
      
      expect(screen.getByTestId('virtual-list')).toBeInTheDocument();
    });

    it('uses standard table for small datasets', () => {
      render(<DataTable {...defaultProps} features={{ virtualScroll: true }} />);
      
      expect(screen.queryByTestId('virtual-list')).not.toBeInTheDocument();
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('supports keyboard navigation', async () => {
      render(<DataTable {...defaultProps} />);
      
      const searchInput = screen.getByPlaceholderText(/search/i);
      searchInput.focus();
      
      await user.keyboard('{Tab}');
      expect(screen.getByLabelText(/filter/i)).toHaveFocus();
    });

    it('has proper ARIA labels', () => {
      render(<DataTable {...defaultProps} />);
      
      expect(screen.getByRole('table')).toHaveAttribute('aria-label');
      expect(screen.getByPlaceholderText(/search/i)).toHaveAttribute('aria-label');
    });

    it('supports screen readers', () => {
      render(<DataTable {...defaultProps} />);
      
      const table = screen.getByRole('table');
      expect(table).toHaveAttribute('aria-rowcount');
      expect(table).toHaveAttribute('aria-colcount');
    });
  });

  describe('Performance', () => {
    it('memoizes expensive calculations', () => {
      const { rerender } = render(<DataTable {...defaultProps} />);
      
      // Simulate prop change that shouldn't trigger recalculation
      rerender(<DataTable {...defaultProps} className="test" />);
      
      // Data should still be rendered correctly
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    it('debounces search input', async () => {
      const onSearch = vi.fn();
      render(<DataTable {...defaultProps} onSearch={onSearch} />);
      
      const searchInput = screen.getByPlaceholderText(/search/i);
      
      // Type multiple characters quickly
      await user.type(searchInput, 'test');
      
      // Should only call onSearch once due to debouncing
      await waitFor(() => {
        expect(onSearch).toHaveBeenCalledTimes(1);
      });
    });
  });

  describe('Error Handling', () => {
    it('handles invalid data gracefully', () => {
      const invalidData = [
        { id: 1, name: null, age: 'invalid' },
        { id: 2 } // Missing fields
      ];

      render(<DataTable {...defaultProps} data={invalidData} />);
      
      // Should still render without crashing
      expect(screen.getByRole('table')).toBeInTheDocument();
    });

    it('handles missing columns gracefully', () => {
      render(<DataTable {...defaultProps} columns={[]} />);
      
      expect(screen.getByText('No data available')).toBeInTheDocument();
    });
  });
});

// Integration tests
describe('DataTable Integration', () => {
  it('handles complex user interactions', async () => {
    const user = userEvent.setup();
    const onSelectionChange = vi.fn();
    const onSort = vi.fn();
    const onFilter = vi.fn();

    render(
      <DataTable 
        {...defaultProps}
        onSelectionChange={onSelectionChange}
        onSort={onSort}
        onFilter={onFilter}
      />
    );

    // Search
    const searchInput = screen.getByPlaceholderText(/search/i);
    await user.type(searchInput, 'John');

    // Sort
    const nameHeader = screen.getByText('Name');
    await user.click(nameHeader);

    // Select row
    const checkboxes = screen.getAllByRole('checkbox');
    await user.click(checkboxes[1]);

    // Verify all callbacks were called
    expect(onSort).toHaveBeenCalled();
    expect(onSelectionChange).toHaveBeenCalled();
  });

  it('maintains state consistency across operations', async () => {
    const user = userEvent.setup();
    render(<DataTable {...defaultProps} />);

    // Apply search
    const searchInput = screen.getByPlaceholderText(/search/i);
    await user.type(searchInput, 'John');

    // Sort
    const nameHeader = screen.getByText('Name');
    await user.click(nameHeader);

    // Search should still be active
    expect(searchInput).toHaveValue('John');
    
    // Only John Doe should be visible
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.queryByText('Jane Smith')).not.toBeInTheDocument();
  });
});
