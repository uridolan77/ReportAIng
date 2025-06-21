/**
 * Accessibility Components Export
 * 
 * WCAG 2.1 AA compliant components for inclusive user experience
 */

// Core accessibility provider
export { AccessibilityProvider, useAccessibility } from './AccessibilityProvider'
export type { AccessibilitySettings, AccessibilityContextType } from './AccessibilityProvider'

// Accessible chart component
export { AccessibleChart } from './AccessibleChart'
export type { AccessibleChartProps } from './AccessibleChart'

// Accessible data table component
export { AccessibleDataTable } from './AccessibleDataTable'
export type { AccessibleDataTableProps } from './AccessibleDataTable'

// Accessibility settings panel
export { AccessibilitySettings } from './AccessibilitySettings'

// Enhanced real-time hook
export { useEnhancedRealTime } from '../../hooks/useEnhancedRealTime'
export type { RealTimeConfig, ConnectionStatus, RealTimeUpdate } from '../../hooks/useEnhancedRealTime'
