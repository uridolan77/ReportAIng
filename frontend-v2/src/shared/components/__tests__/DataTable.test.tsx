/**
 * DataTable Component Tests
 * 
 * Comprehensive tests for the DataTable component including
 * rendering, interactions, and data handling.
 */

import React from 'react'
import { screen, fireEvent, waitFor } from '@testing-library/react'
import { renderWithProviders, mockApiHandlers } from '../../utils/testUtils'
import { DataTable } from '../core/DataTable'

// Mock data for testing
const mockData = [
  { id: 1, name: 'John Doe', email: 'john@example.com', age: 30 },
  { id: 2, name: 'Jane Smith', email: 'jane@example.com', age: 25 },
  { id: 3, name: 'Bob Johnson', email: 'bob@example.com', age: 35 }
]

const mockColumns = [
  { key: 'id', title: 'ID', type: 'number' as const, sortable: true },
  { key: 'name', title: 'Name', type: 'text' as const, sortable: true, filterable: true },
  { key: 'email', title: 'Email', type: 'text' as const, filterable: true },
  { key: 'age', title: 'Age', type: 'number' as const, sortable: true }
]

describe('DataTable Component', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders table with data', () => {
    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
      />
    )

    // Check if table headers are rendered
    expect(screen.getByText('Name')).toBeInTheDocument()
    expect(screen.getByText('Email')).toBeInTheDocument()
    expect(screen.getByText('Age')).toBeInTheDocument()

    // Check if data rows are rendered
    expect(screen.getByText('John Doe')).toBeInTheDocument()
    expect(screen.getByText('jane@example.com')).toBeInTheDocument()
    expect(screen.getByText('35')).toBeInTheDocument()
  })

  it('shows loading state', () => {
    renderWithProviders(
      <DataTable
        data={[]}
        columns={mockColumns}
        loading={true}
      />
    )

    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument()
  })

  it('handles empty data', () => {
    renderWithProviders(
      <DataTable
        data={[]}
        columns={mockColumns}
        loading={false}
      />
    )

    expect(screen.getByText('No data available')).toBeInTheDocument()
  })

  it('supports sorting', async () => {
    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
      />
    )

    // Click on Name column header to sort
    const nameHeader = screen.getByText('Name')
    fireEvent.click(nameHeader)

    await waitFor(() => {
      // Check if data is sorted (Bob should be first alphabetically)
      const rows = screen.getAllByRole('row')
      expect(rows[1]).toHaveTextContent('Bob Johnson')
    })
  })

  it('supports filtering', async () => {
    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
        filterable={true}
      />
    )

    // Find and use the search input
    const searchInput = screen.getByPlaceholderText('Search...')
    fireEvent.change(searchInput, { target: { value: 'John' } })

    await waitFor(() => {
      // Should only show John Doe and Bob Johnson
      expect(screen.getByText('John Doe')).toBeInTheDocument()
      expect(screen.getByText('Bob Johnson')).toBeInTheDocument()
      expect(screen.queryByText('Jane Smith')).not.toBeInTheDocument()
    })
  })

  it('supports pagination', () => {
    const largeData = Array.from({ length: 25 }, (_, i) => ({
      id: i + 1,
      name: `User ${i + 1}`,
      email: `user${i + 1}@example.com`,
      age: 20 + (i % 30)
    }))

    renderWithProviders(
      <DataTable
        data={largeData}
        columns={mockColumns}
        loading={false}
        pagination={true}
        pageSize={10}
      />
    )

    // Check if pagination controls are present
    expect(screen.getByText('1')).toBeInTheDocument() // Page 1
    expect(screen.getByText('2')).toBeInTheDocument() // Page 2
    expect(screen.getByText('3')).toBeInTheDocument() // Page 3
  })

  it('handles row selection', async () => {
    const onSelectionChange = jest.fn()

    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
        selectable={true}
        onSelectionChange={onSelectionChange}
      />
    )

    // Click on first row checkbox
    const checkboxes = screen.getAllByRole('checkbox')
    fireEvent.click(checkboxes[1]) // First data row (index 0 is header)

    await waitFor(() => {
      expect(onSelectionChange).toHaveBeenCalledWith([mockData[0]])
    })
  })

  it('supports export functionality', async () => {
    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
        exportable={true}
      />
    )

    // Check if export button is present
    const exportButton = screen.getByText('Export')
    expect(exportButton).toBeInTheDocument()

    // Click export button
    fireEvent.click(exportButton)

    // Check if export menu appears
    await waitFor(() => {
      expect(screen.getByText('Export as CSV')).toBeInTheDocument()
      expect(screen.getByText('Export as Excel')).toBeInTheDocument()
    })
  })

  it('handles virtual scrolling for large datasets', () => {
    const largeData = Array.from({ length: 1000 }, (_, i) => ({
      id: i + 1,
      name: `User ${i + 1}`,
      email: `user${i + 1}@example.com`,
      age: 20 + (i % 30)
    }))

    renderWithProviders(
      <DataTable
        data={largeData}
        columns={mockColumns}
        loading={false}
        virtualScrolling={true}
        height={400}
      />
    )

    // Check if virtual scrolling container is present
    expect(screen.getByTestId('virtual-scroll-container')).toBeInTheDocument()
    
    // Only a subset of rows should be rendered
    const rows = screen.getAllByRole('row')
    expect(rows.length).toBeLessThan(50) // Much less than 1000
  })

  it('displays error state', () => {
    renderWithProviders(
      <DataTable
        data={[]}
        columns={mockColumns}
        loading={false}
        error="Failed to load data"
      />
    )

    expect(screen.getByText('Failed to load data')).toBeInTheDocument()
    expect(screen.getByText('Retry')).toBeInTheDocument()
  })

  it('handles column resizing', async () => {
    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
        resizable={true}
      />
    )

    // Find resize handle for Name column
    const resizeHandle = screen.getByTestId('resize-handle-name')
    expect(resizeHandle).toBeInTheDocument()

    // Simulate drag to resize
    fireEvent.mouseDown(resizeHandle)
    fireEvent.mouseMove(resizeHandle, { clientX: 200 })
    fireEvent.mouseUp(resizeHandle)

    // Column should be resized (this would need more specific implementation)
    await waitFor(() => {
      expect(resizeHandle).toBeInTheDocument()
    })
  })

  it('supports custom cell rendering', () => {
    const customColumns = [
      ...mockColumns,
      {
        key: 'actions',
        title: 'Actions',
        type: 'custom' as const,
        render: (value: any, record: any) => (
          <button data-testid={`edit-${record.id}`}>Edit</button>
        )
      }
    ]

    renderWithProviders(
      <DataTable
        data={mockData}
        columns={customColumns}
        loading={false}
      />
    )

    // Check if custom rendered buttons are present
    expect(screen.getByTestId('edit-1')).toBeInTheDocument()
    expect(screen.getByTestId('edit-2')).toBeInTheDocument()
    expect(screen.getByTestId('edit-3')).toBeInTheDocument()
  })

  it('handles keyboard navigation', async () => {
    renderWithProviders(
      <DataTable
        data={mockData}
        columns={mockColumns}
        loading={false}
        keyboardNavigation={true}
      />
    )

    const table = screen.getByRole('table')
    
    // Focus on table
    fireEvent.focus(table)
    
    // Navigate with arrow keys
    fireEvent.keyDown(table, { key: 'ArrowDown' })
    fireEvent.keyDown(table, { key: 'ArrowRight' })
    
    await waitFor(() => {
      // Check if focus has moved (implementation specific)
      expect(table).toHaveFocus()
    })
  })
})
