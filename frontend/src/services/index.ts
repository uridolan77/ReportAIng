/**
 * Services Index
 * 
 * Consolidated service layer with modern patterns and dependency injection.
 * Provides unified API for all application services.
 */

// Core Services
export { ApiService } from './core/ApiService';
export { AuthService } from './core/AuthService';
export { CacheService } from './core/CacheService';
export { ErrorService } from './core/ErrorService';
export { LoggingService } from './core/LoggingService';
export { ConfigService } from './core/ConfigService';

// Feature Services
export { QueryService } from './features/QueryService';
export { VisualizationService } from './features/VisualizationService';
export { DashboardService } from './features/DashboardService';
export { DatabaseService } from './features/DatabaseService';
export { AIService } from './features/AIService';
export { AdminService } from './features/AdminService';

// Advanced Services
export { SecurityService } from './advanced/SecurityService';
export { PerformanceService } from './advanced/PerformanceService';
export { AnalyticsService } from './advanced/AnalyticsService';
export { NotificationService } from './advanced/NotificationService';

// Legacy Services (for backward compatibility)
export { secureApiClient } from './secureApiClient';
export { errorService } from './errorService';

// Service Container & DI
export { ServiceContainer } from './container/ServiceContainer';
export { useService } from './container/useService';

// Types
export type * from './types';
