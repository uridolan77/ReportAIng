/**
 * Common Components
 * Consolidates single-file components for better organization
 */

// AI Components
export { default as QuerySimilarityAnalyzer } from '../AI/QuerySimilarityAnalyzer';
export { default as UserContextPanel } from '../AI/UserContextPanel';

// Authentication
export { Login } from '../Auth/Login';

// Collaboration
export { CollaborativeDashboard } from '../Collaboration/CollaborativeDashboard';

// Command Palette
export { CommandPalette } from '../CommandPalette/CommandPalette';

// Error Handling
export { ErrorBoundary } from '../ErrorBoundary/ErrorBoundary';

// Insights
export { DataInsightsPanel } from '../Insights/DataInsightsPanel';

// Query Templates
export { QueryTemplateLibrary } from '../QueryTemplates/QueryTemplateLibrary';

// Security Components
export { SecurityDashboard } from '../Security/SecurityDashboard';
export { RequestSigningDemo } from '../Security/RequestSigningDemo';

// State Sync Components
export { StateSyncDemo } from '../StateSync/StateSyncDemo';

// Type Safety Components
export { TypeSafetyDemo } from '../TypeSafety/TypeSafetyDemo';

// Type definitions for common component props
export interface CommonComponentProps {
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
}

export interface LoadingProps extends CommonComponentProps {
  loading?: boolean;
  size?: 'small' | 'default' | 'large';
}

export interface ErrorProps extends CommonComponentProps {
  error?: string | Error | null;
  onRetry?: () => void;
}

export interface SecurityProps extends CommonComponentProps {
  onSecurityEvent?: (event: string) => void;
  securityLevel?: 'low' | 'medium' | 'high';
}

export interface DemoProps extends CommonComponentProps {
  interactive?: boolean;
  showCode?: boolean;
}
