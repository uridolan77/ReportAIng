/**
 * Centralized Styles Index
 * Consolidates all component styles for better organization
 */

// Import all CSS files to ensure they're included in the build
import './variables.css';
import './animations.css';
import './utilities.css';
import './query-interface.css';
import './layout.css';
import './data-table.css';
import './visualization.css';

// Re-export component-specific styles for backward compatibility
export * from './query-interface';
export * from './layout';
export * from './data-table';
export * from './visualization';
