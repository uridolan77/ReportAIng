# Enhanced DataTable Filtering

This document describes the enhanced filtering capabilities of the DataTable component with automatic data type detection and intelligent filter selection.

## Features

### Automatic Data Type Detection

The DataTable can automatically detect column data types and assign appropriate filter types:

- **Text Fields** → Multiselect dropdown filters
- **Money/Currency Fields** → Range filters with >=, <= operators
- **Date Fields** → Date range pickers
- **Number Fields** → Min/Max range inputs
- **Boolean Fields** → Yes/No/All radio buttons

### Enhanced Filter Types

#### Money Filters
- Range filtering with min/max amounts
- Operator-based filtering (>=, <=)
- Currency symbol support ($, €, £)
- Comma-separated number formatting

#### Text Multiselect Filters
- Automatic generation of filter options from unique values
- Searchable dropdown with up to 50 unique values
- Fallback to text search for fields with too many unique values

#### Date Range Filters
- Date picker with range selection
- Support for various date formats
- Keyword-based detection (date, created, updated, etc.)

## Usage

### Basic Usage with Auto-Detection

```tsx
import DataTable from './components/DataTable';

const MyComponent = () => {
  const data = [
    {
      id: 1,
      name: 'John Doe',
      salary: '$50,000',
      joinDate: '2023-01-15',
      isActive: true
    },
    // ... more data
  ];

  // Minimal column configuration - types will be auto-detected
  const columns = [
    { key: 'name', title: 'Name', dataIndex: 'name' },
    { key: 'salary', title: 'Salary', dataIndex: 'salary' },
    { key: 'joinDate', title: 'Join Date', dataIndex: 'joinDate' },
    { key: 'isActive', title: 'Active', dataIndex: 'isActive' }
  ];

  return (
    <DataTable
      data={data}
      columns={columns}
      autoDetectTypes={true}  // Enable automatic type detection
      features={{
        filtering: true,
        searching: true,
        sorting: true
      }}
    />
  );
};
```

### Using Enhanced Columns Hook

```tsx
import { useEnhancedColumns } from './components/DataTable';

const MyComponent = () => {
  const { enhancedColumns, columnAnalysis } = useEnhancedColumns({
    data,
    columns: basicColumns,
    autoDetectTypes: true
  });

  // View detected types
  console.log('Column Analysis:', columnAnalysis);

  return (
    <DataTable
      data={data}
      columns={enhancedColumns}
      features={{ filtering: true }}
    />
  );
};
```

### Auto-Generated Columns

```tsx
import { useAutoColumns } from './components/DataTable';

const MyComponent = () => {
  const { columns } = useAutoColumns(data, ['id']); // Exclude 'id' column

  return (
    <DataTable
      data={data}
      columns={columns}
      features={{ filtering: true }}
    />
  );
};
```

## Configuration Options

### DataTable Props

- `autoDetectTypes?: boolean` - Enable automatic data type detection (default: false)
- `autoGenerateFilterOptions?: boolean` - Auto-generate filter options for multiselect (default: true)

### Column Configuration

```tsx
interface DataTableColumn {
  // ... existing props
  dataType?: 'string' | 'number' | 'date' | 'boolean' | 'money' | 'currency';
  filterType?: 'text' | 'number' | 'money' | 'date' | 'dateRange' | 'select' | 'multiselect' | 'boolean';
  filterOptions?: Array<{ label: string; value: any }>;
}
```

## Filter Types

### Money Filter
```tsx
{
  key: 'revenue',
  title: 'Revenue',
  dataIndex: 'revenue',
  dataType: 'money',
  filterType: 'money'
}
```

Provides:
- Min/Max amount inputs with currency formatting
- >= (Greater or Equal) operator button
- <= (Less or Equal) operator button

### Multiselect Filter
```tsx
{
  key: 'category',
  title: 'Category',
  dataIndex: 'category',
  dataType: 'string',
  filterType: 'multiselect',
  filterOptions: [
    { label: 'Category A', value: 'A' },
    { label: 'Category B', value: 'B' }
  ]
}
```

### Date Range Filter
```tsx
{
  key: 'createdAt',
  title: 'Created Date',
  dataIndex: 'createdAt',
  dataType: 'date',
  filterType: 'dateRange'
}
```

## Demo

See `EnhancedFilteringDemo.tsx` for a complete working example with sample data demonstrating all filter types.

## Type Detection Logic

The automatic type detection analyzes sample data to determine:

1. **Money Detection**: Looks for currency symbols, money-related keywords in column names
2. **Date Detection**: Checks for date patterns and date-related keywords
3. **Boolean Detection**: Identifies true/false, yes/no, 1/0 patterns
4. **Number Detection**: Validates numeric values
5. **String Detection**: Determines if multiselect is appropriate based on unique value count

## Performance

- Type detection runs on a sample of data (default: 100 rows)
- Filter options are generated only for reasonable numbers of unique values (≤50)
- Debounced filtering for smooth user experience
