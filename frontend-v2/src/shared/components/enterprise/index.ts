/**
 * Enterprise Components Export
 * 
 * Production-ready enterprise features including:
 * - Security framework and monitoring
 * - PWA capabilities and offline functionality
 * - Performance monitoring and analytics
 * - Production readiness indicators
 */

// Security Components
export { SecurityProvider, useSecurity } from '../../security/SecurityProvider'
export { SecurityDashboard } from '../security/SecurityDashboard'
export type { 
  SecurityConfig, 
  SecurityThreat, 
  SecurityContextType 
} from '../../security/SecurityProvider'

// PWA Components
export { 
  PWAManager, 
  usePWA, 
  InstallButton, 
  OfflineStatus 
} from '../pwa/PWAManager'
export type { PWAContextType } from '../pwa/PWAManager'

// Performance Monitoring
export { 
  performanceMonitor, 
  default as PerformanceMonitor 
} from '../../monitoring/PerformanceMonitor'
export type { 
  PerformanceMetric, 
  ErrorReport, 
  UserInteraction, 
  PerformanceReport 
} from '../../monitoring/PerformanceMonitor'
