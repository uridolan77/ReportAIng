// Centralized API Endpoints Configuration
// This file defines all API endpoints used by the frontend

export const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:55243';

/**
 * Health Check Endpoints
 *
 * The backend has two types of health endpoints:
 * 1. ASP.NET Core Health Checks (/health) - Comprehensive system health with database, OpenAI, Redis checks
 * 2. Custom Health Controller (/api/health) - Simple application health status
 */
export const HEALTH_ENDPOINTS = {
  // ASP.NET Core Health Checks - Use for comprehensive system health monitoring
  SYSTEM_HEALTH: '/health',                    // JSON response with all health checks
  SYSTEM_HEALTH_READY: '/health/ready',        // Ready state health checks

  // Custom Health Controller - Use for simple application status
  APP_HEALTH: '/api/health',                   // Simple application health
  APP_HEALTH_DETAILED: '/api/health/detailed', // Detailed application info
} as const;

/**
 * Authentication Endpoints
 */
export const AUTH_ENDPOINTS = {
  LOGIN: '/api/auth/login',
  LOGOUT: '/api/auth/logout',
  REFRESH: '/api/auth/refresh',
  REGISTER: '/api/auth/register',
  VALIDATE: '/api/auth/validate',
} as const;

/**
 * Query Endpoints
 */
export const QUERY_ENDPOINTS = {
  NATURAL_LANGUAGE: '/api/query/natural-language',
  EXECUTE_SQL: '/api/query/execute-sql',
  VALIDATE_SQL: '/api/query/validate-sql',
  HISTORY: '/api/query/history',
  SUGGESTIONS: '/api/query/suggestions',
  SCHEMA: '/api/query/schema',
} as const;

/**
 * Schema Endpoints
 */
export const SCHEMA_ENDPOINTS = {
  DATASOURCES: '/api/schema/datasources',
  TABLES: '/api/schema/tables',
  COLUMNS: '/api/schema/columns',
} as const;

/**
 * Streaming Endpoints
 */
export const STREAMING_ENDPOINTS = {
  BASIC: '/api/streaming/stream-query',
  BACKPRESSURE: '/api/streaming/stream-backpressure',
  PROGRESS: '/api/streaming/stream-progress',
} as const;

/**
 * Visualization Endpoints
 */
export const VISUALIZATION_ENDPOINTS = {
  GENERATE: '/api/visualization/generate',
  INTERACTIVE: '/api/visualization/interactive',
  RECOMMENDATIONS: '/api/visualization/recommendations',
  ADVANCED: '/api/advanced-visualization',
} as const;

/**
 * Dashboard Endpoints
 */
export const DASHBOARD_ENDPOINTS = {
  LIST: '/api/dashboard',
  CREATE: '/api/dashboard',
  UPDATE: '/api/dashboard',
  DELETE: '/api/dashboard',
  VISUALIZATIONS: '/api/dashboard/visualizations',
} as const;

/**
 * Export Endpoints
 */
export const EXPORT_ENDPOINTS = {
  CSV: '/api/export/csv',
  EXCEL: '/api/export/excel',
  PDF: '/api/export/pdf',
} as const;

/**
 * SignalR Hub URLs
 */
export const SIGNALR_HUBS = {
  QUERY_STATUS: '/hubs/query-status',
  QUERY: '/hubs/query',
} as const;

/**
 * Helper function to get full API URL
 */
export const getApiUrl = (endpoint: string): string => {
  return `${API_BASE_URL}${endpoint}`;
};

/**
 * Helper function to get SignalR hub URL
 */
export const getHubUrl = (hubPath: string): string => {
  return `${API_BASE_URL}${hubPath}`;
};

/**
 * Endpoint usage guidelines:
 *
 * For Health Monitoring:
 * - Use HEALTH_ENDPOINTS.SYSTEM_HEALTH for comprehensive health checks
 * - Use HEALTH_ENDPOINTS.APP_HEALTH for simple application status
 *
 * For Database Connection Status:
 * - Use HEALTH_ENDPOINTS.SYSTEM_HEALTH to get database health from health checks
 * - Use SCHEMA_ENDPOINTS.DATASOURCES to get available data sources
 *
 * For Authentication:
 * - Use AUTH_ENDPOINTS.* for all authentication operations
 *
 * For Queries:
 * - Use QUERY_ENDPOINTS.* for query operations
 * - Use STREAMING_ENDPOINTS.* for streaming query responses
 */

export default {
  API_BASE_URL,
  HEALTH_ENDPOINTS,
  AUTH_ENDPOINTS,
  QUERY_ENDPOINTS,
  SCHEMA_ENDPOINTS,
  STREAMING_ENDPOINTS,
  VISUALIZATION_ENDPOINTS,
  DASHBOARD_ENDPOINTS,
  EXPORT_ENDPOINTS,
  SIGNALR_HUBS,
  getApiUrl,
  getHubUrl,
};
