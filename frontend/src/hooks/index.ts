/**
 * Hooks Index
 * 
 * Consolidated hook system with modern patterns and utilities.
 * Provides unified API for all application hooks.
 */

// Core Hooks
export { useApi } from './core/useApi';
export { useAuth } from './core/useAuth';
export { useCache } from './core/useCache';
export { useConfig } from './core/useConfig';
export { useError } from './core/useError';
export { useLoading } from './core/useLoading';
export { useLocalStorage } from './core/useLocalStorage';
export { useSessionStorage } from './core/useSessionStorage';

// Feature Hooks
export { useQuery } from './features/useQuery';
export { useVisualization } from './features/useVisualization';
export { useDashboard } from './features/useDashboard';
export { useDatabase } from './features/useDatabase';
export { useAI } from './features/useAI';
export { useAdmin } from './features/useAdmin';

// UI Hooks
export { useTheme } from './ui/useTheme';
export { useDarkMode } from './ui/useDarkMode';
export { useAnimation } from './ui/useAnimation';
export { useResponsive } from './ui/useResponsive';
export { useKeyboard } from './ui/useKeyboard';
export { useFocus } from './ui/useFocus';
export { useClickOutside } from './ui/useClickOutside';
export { useIntersection } from './ui/useIntersection';

// Performance Hooks
export { useDebounce } from './performance/useDebounce';
export { useThrottle } from './performance/useThrottle';
export { useMemoized } from './performance/useMemoized';
export { useVirtualization } from './performance/useVirtualization';
export { useLazyLoading } from './performance/useLazyLoading';

// Advanced Hooks
export { useWebSocket } from './advanced/useWebSocket';
export { useNotifications } from './advanced/useNotifications';
export { useAnalytics } from './advanced/useAnalytics';
export { useSecurity } from './advanced/useSecurity';
export { usePerformanceMonitor } from './advanced/usePerformanceMonitor';

// Legacy Hooks (for backward compatibility)
export { useCurrentResult } from './useCurrentResult';
export { useQueryHistory } from './useQueryHistory';
export { useQuerySuggestions } from './useQuerySuggestions';

// Hook Utilities
export { createAsyncHook } from './utils/createAsyncHook';
export { createStateHook } from './utils/createStateHook';
export { createEffectHook } from './utils/createEffectHook';

// Types
export type * from './types';
