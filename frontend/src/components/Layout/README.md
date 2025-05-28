# Database Status Indicators

This directory contains components for monitoring and displaying the database connection status in the BI Reporting Copilot application.

## Components

### DatabaseStatusIndicator
A compact status indicator that appears in the header navigation bar.

**Features:**
- Real-time connection status monitoring
- Color-coded status (green = connected, red = disconnected, blue = checking)
- Tooltip with connection details
- Click to open detailed status modal
- Automatic refresh every 30 seconds

**Usage:**
```tsx
import { DatabaseStatusIndicator } from './DatabaseStatusIndicator';

<DatabaseStatusIndicator />
```

### DatabaseConnectionBanner
A prominent banner that appears at the top of the query interface when the database is disconnected.

**Features:**
- Only shows when database is disconnected
- Clear error messaging
- Retry connection button
- Auto-hides when connection is restored

**Usage:**
```tsx
import { DatabaseConnectionBanner } from './DatabaseConnectionBanner';

<DatabaseConnectionBanner />
```

## Status Detection

Both components check database connectivity by:

1. **Health Check Endpoint**: Calls `/api/health` to verify backend connectivity
2. **Schema Endpoint**: Optionally calls `/api/schema/datasources` to get table count
3. **Error Handling**: Captures and displays connection errors

## Status States

- **Connected** (Green): Database is accessible and ready for queries
- **Disconnected** (Red): Database connection failed or backend is unreachable
- **Checking** (Blue): Currently testing the connection

## Configuration

The components automatically detect the database connection status by calling:
- `GET /api/health` - Main health check
- `GET /api/schema/datasources` - Additional database info (optional)

## Styling

Custom CSS classes are available in `DatabaseStatus.css`:
- `.database-status-indicator` - Main indicator styling
- `.database-connection-banner` - Banner styling
- `.database-status-modal` - Modal dialog styling

## Integration

The components are integrated into:
- **Header**: `DatabaseStatusIndicator` in the main navigation
- **Query Interface**: `DatabaseConnectionBanner` above the query input

## Real-time Updates

- Automatic status checks every 30 seconds
- Manual refresh available via "Retry Connection" button
- Immediate feedback on connection state changes

## Error Handling

- Network errors are caught and displayed
- SSL certificate issues are handled gracefully
- Timeout errors show appropriate messaging
- Detailed error information available in the status modal
