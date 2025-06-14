/**
 * Layout Components Index
 * 
 * Legacy layout components for backward compatibility.
 * Modern layout components are in /components/core/Layouts.tsx
 */

// Legacy Layout Components
export { DatabaseConnectionBanner } from './DatabaseConnectionBanner';
export { DatabaseStatusIndicator } from './DatabaseStatusIndicator';

// Corner Components
export { CornerStatusPanel } from './AppHeader';

// Note: Modern layout components are available from '../core/Layouts'
// They are not re-exported here to avoid duplicate exports

// Modern Sidebar Component
export { default as ModernSidebar } from './ModernSidebar';
export { default as Sidebar } from './Sidebar';
