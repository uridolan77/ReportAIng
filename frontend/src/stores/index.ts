/**
 * Stores Index
 * 
 * Consolidated store system with modern patterns and utilities.
 * Provides unified state management for the entire application.
 */

// Core Stores
export { useAppStore } from './core/appStore';
export { useAuthStore } from './core/authStore';
export { useConfigStore } from './core/configStore';
export { useErrorStore } from './core/errorStore';
export { useLoadingStore } from './core/loadingStore';
export { useNotificationStore } from './core/notificationStore';

// Feature Stores
export { useQueryStore } from './features/queryStore';
export { useVisualizationStore } from './features/visualizationStore';
export { useDashboardStore } from './features/dashboardStore';
export { useDatabaseStore } from './features/databaseStore';
export { useAIStore } from './features/aiStore';
export { useAdminStore } from './features/adminStore';

// UI Stores
export { useThemeStore } from './ui/themeStore';
export { useLayoutStore } from './ui/layoutStore';
export { useNavigationStore } from './ui/navigationStore';
export { useModalStore } from './ui/modalStore';

// Performance Stores
export { useCacheStore } from './performance/cacheStore';
export { usePerformanceStore } from './performance/performanceStore';

// Legacy Stores (for backward compatibility)
export { useAuthStore as authStore } from './authStore';
export { useVisualizationStore as visualizationStore } from './visualizationStore';

// Store Utilities
export { createStore } from './utils/createStore';
export { createAsyncStore } from './utils/createAsyncStore';
export { createPersistentStore } from './utils/createPersistentStore';
export { combineStores } from './utils/combineStores';

// Store Middleware
export { withLogging } from './middleware/withLogging';
export { withPersistence } from './middleware/withPersistence';
export { withDevtools } from './middleware/withDevtools';
export { withErrorHandling } from './middleware/withErrorHandling';

// Types
export type * from './types';
