/**
 * Components Index - Phase 4 Enterprise Edition
 *
 * Advanced component system with enterprise-grade features,
 * performance optimization, and comprehensive organization.
 */

// Core Components (15 modern, reusable components)
export * from './core';

// Advanced Features (Dark mode, animations, performance monitoring)
export * from './advanced';

// Feature Modules (Organized domain components)
export * from './features';

// Layout Components (Modern layout system)
export * from './Layout';

// UI Components (Advanced UI features only)
export * from './ui';

// Legacy Support (Backward compatibility - marked for deprecation)
export { QueryInterface } from './QueryInterface/QueryInterface';
export { default as DashboardBuilder } from './Dashboard/DashboardBuilder';
export { InteractiveVisualization } from './Visualization/InteractiveVisualization';
export { DBExplorer } from './DBExplorer/DBExplorer';
export { default as DataTable } from './DataTable/DataTable';

// Utility Components
export * from './ErrorBoundary';
export * from './ThemeToggle';

// Development Tools (for development environment only)
export * from './DevTools';
export * from './Demo';

// Types
export type * from './types';
export type * from './core/types';
export type * from './features/types';
