/**
 * Hooks Index
 * 
 * Consolidated hook system with modern patterns and utilities.
 * Provides unified API for all application hooks.
 */

// ===== CORE HOOKS =====
// Only export hooks that actually exist
export { useApi } from './core/useApi';

// ===== FEATURE HOOKS =====
// API and data management hooks
export { useQueryApi } from './useQueryApi';
export { useVisualizationApi } from './useVisualizationApi';
export { useTuningApi } from './useTuningApi';
export { useValidatedQuery } from './useValidatedQuery';

// ===== UTILITY HOOKS =====
// State and utility hooks
export { useEnhancedState } from './useEnhancedState';
export { useOptimization } from './useOptimization';
export { useAnimations } from './useAnimations';
export { useChartAnimations } from './useChartAnimations';
export { useAccessibility } from './useAccessibility';
export { useKeyboardNavigation } from './useKeyboardNavigation';

// ===== NETWORK HOOKS =====
export { useWebSocket } from './useWebSocket';

// ===== TEMPLATE HOOKS =====
export { useQueryTemplates } from './useQueryTemplates';

// Performance Hooks (consolidated from usePerformance.ts)
export {
  useDebounce,
  useThrottle,
  useIntersectionObserver,
  useVirtualScrolling,
  useMemoryMonitor,
  usePerformanceMeasure,
  useRenderOptimization,
  useLazyImage,
  useComponentSize
} from './usePerformance';

// ===== LEGACY HOOKS =====
// Backward compatibility hooks (only export if they exist)
export { useCurrentResult } from './useCurrentResult';
