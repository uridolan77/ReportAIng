/**
 * Centralized Styles Index
 * Consolidates all component styles for better organization
 */

// Import all CSS files to ensure they're included in the build
// Note: variables.css and animations.css have been consolidated into the design system
import './utilities.css';
import './query-interface.css';
import './data-table.css';
import './visualization.css';
import './schema-management.css';
import './ui.css';
import './navigation.css';

// All CSS files now consolidated into centralized system
// No remaining scattered CSS files to import

// Re-export component-specific styles for backward compatibility
export * from './query-interface';
export * from './data-table';
export * from './visualization';
export * from './schema-management';
