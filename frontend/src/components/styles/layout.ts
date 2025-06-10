/**
 * Layout Styles Export
 * Type-safe style constants for layout components
 */

// Import CSS files
import './layout.css';

// Export style constants for TypeScript usage
export const layoutStyles = {
  container: 'layout-container',
  content: 'layout-content',
  sidebar: 'layout-sidebar',
  sidebarCollapsed: 'layout-sidebar collapsed',
  sidebarOpen: 'layout-sidebar open'
} as const;

export const headerStyles = {
  header: 'app-header',
  logoContainer: 'app-logo-container',
  logoIcon: 'app-logo-icon',
  button: 'header-button',
  userDropdown: 'user-dropdown-button'
} as const;

export const statusStyles = {
  indicator: 'status-indicator',
  connected: 'status-indicator connected',
  disconnected: 'status-indicator disconnected',
  loading: 'status-indicator loading',
  databaseStatus: 'database-status-enhanced',
  databaseConnected: 'database-status-enhanced connected',
  connectionBanner: 'database-connection-banner'
} as const;

// Type definitions
export interface LayoutProps {
  className?: string;
  collapsed?: boolean;
  sidebarOpen?: boolean;
}

export interface HeaderProps {
  className?: string;
  showLogo?: boolean;
  showUserMenu?: boolean;
}

export interface StatusProps {
  status: 'connected' | 'disconnected' | 'loading';
  className?: string;
}
