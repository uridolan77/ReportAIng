/**
 * Query Interface Styles Export
 * Consolidates all query interface related styles
 */

// Import CSS files
import './query-interface.css';

// Export style constants for TypeScript usage
export const queryInterfaceStyles = {
  container: 'query-interface-container',
  card: 'enhanced-card',
  header: 'query-builder-header',
  title: 'query-builder-title',
  content: 'query-builder-content',
  footer: 'query-builder-footer',
  loading: 'loading-skeleton',
  error: 'error-state',
  success: 'success-state'
} as const;

export const queryBuilderStyles = {
  wrapper: 'enhanced-query-builder',
  tabs: 'query-builder-tabs',
  tabContent: 'query-builder-tab-content',
  form: 'query-builder-form',
  field: 'query-builder-field',
  button: 'query-builder-button'
} as const;

export const minimalInterfaceStyles = {
  container: 'minimal-query-interface',
  input: 'minimal-query-input',
  suggestions: 'query-suggestions-panel',
  results: 'query-results-panel'
} as const;
