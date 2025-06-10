/**
 * Admin Components
 * Administrative components for managing query suggestions and system settings
 */

// Query Suggestions Management
export { default as QuerySuggestionsManager } from './QuerySuggestionsManager';
export { default as SuggestionsManager } from './SuggestionsManager';
export { default as CategoriesManager } from './CategoriesManager';
export { default as SuggestionAnalytics } from './SuggestionAnalytics';
export { default as SuggestionSyncUtility } from './SuggestionSyncUtility';

// Re-export all components as named exports
export {
  QuerySuggestionsManager,
  SuggestionsManager,
  CategoriesManager,
  SuggestionAnalytics,
  SuggestionSyncUtility,
};
