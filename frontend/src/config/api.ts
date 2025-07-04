// API Configuration for BI Reporting Copilot Frontend

export const API_CONFIG = {
  // Base URL for API calls
  BASE_URL: process.env.NODE_ENV === 'production'
    ? 'https://your-production-api.com'
    : 'http://localhost:55244',

  // API endpoints
  ENDPOINTS: {
    // Authentication
    AUTH: {
      LOGIN: '/api/auth/login',
      LOGOUT: '/api/auth/logout',
      REFRESH: '/api/auth/refresh',
      REGISTER: '/api/auth/register',
    },

    // Query endpoints
    QUERY: {
      NATURAL_LANGUAGE: '/api/query/execute',
      EXECUTE_SQL: '/api/query/execute',
      VALIDATE_SQL: '/api/query/validate',
      HISTORY: '/api/query/history',
      SUGGESTIONS: '/api/query/suggestions',
      SCHEMA: '/api/schema',
    },

    // Streaming endpoints
    STREAMING: {
      BASIC: '/api/streaming/stream-query',
      BACKPRESSURE: '/api/streaming/stream-backpressure',
      PROGRESS: '/api/streaming/stream-progress',
    },

    // Dashboard endpoints
    DASHBOARD: {
      LIST: '/api/dashboard',
      CREATE: '/api/dashboard',
      UPDATE: '/api/dashboard',
      DELETE: '/api/dashboard',
      VISUALIZATIONS: '/api/dashboard/visualizations',
    },

    // Export endpoints
    EXPORT: {
      CSV: '/api/export/csv',
      EXCEL: '/api/export/excel',
      PDF: '/api/export/pdf',
    },

    // Schema Management endpoints
    SCHEMA_MANAGEMENT: {
      SCHEMAS: '/api/SchemaManagement/schemas',
      SCHEMA_BY_ID: '/api/SchemaManagement/schemas',
      VERSIONS: '/api/SchemaManagement/versions',
      VERSION_BY_ID: '/api/SchemaManagement/versions',
      TABLE_CONTEXTS: '/api/SchemaManagement/table-contexts',
      COLUMN_CONTEXTS: '/api/SchemaManagement/column-contexts',
      GLOSSARY_TERMS: '/api/SchemaManagement/glossary-terms',
      RELATIONSHIPS: '/api/SchemaManagement/relationships',
      USER_PREFERENCES: '/api/SchemaManagement/user-preferences',
      APPLY_CONTENT: '/api/SchemaManagement/apply',
      COMPARE: '/api/SchemaManagement/compare',
      EXPORT: '/api/SchemaManagement/export',
      IMPORT: '/api/SchemaManagement/import',
    },

    // LLM Management endpoints
    LLM_MANAGEMENT: {
      PROVIDERS: '/api/LLMManagement/providers',
      PROVIDER_BY_ID: '/api/LLMManagement/providers',
      PROVIDER_TEST: '/api/LLMManagement/providers',
      PROVIDER_HEALTH: '/api/LLMManagement/providers/health',
      MODELS: '/api/LLMManagement/models',
      MODEL_BY_ID: '/api/LLMManagement/models',
      DEFAULT_MODEL: '/api/LLMManagement/models/default',
      SET_DEFAULT_MODEL: '/api/LLMManagement/models',
      USAGE_HISTORY: '/api/LLMManagement/usage/history',
      USAGE_ANALYTICS: '/api/LLMManagement/usage/analytics',
      USAGE_EXPORT: '/api/LLMManagement/usage/export',
      COST_SUMMARY: '/api/LLMManagement/costs/summary',
      CURRENT_MONTH_COST: '/api/LLMManagement/costs/current-month',
      COST_PROJECTIONS: '/api/LLMManagement/costs/projections',
      COST_LIMITS: '/api/LLMManagement/costs/limits',
      COST_ALERTS: '/api/LLMManagement/costs/alerts',
      PERFORMANCE_METRICS: '/api/LLMManagement/performance/metrics',
      CACHE_HIT_RATES: '/api/LLMManagement/performance/cache-hit-rates',
      ERROR_ANALYSIS: '/api/LLMManagement/performance/error-analysis',
      DASHBOARD_SUMMARY: '/api/LLMManagement/dashboard/summary',
    },

    // Cache Management endpoints
    CACHE: {
      CLEAR: '/api/cache/clear',
      CLEAR_ALL: '/api/cache/clear-all',
      STATS: '/api/cache/stats',
      EXISTS: '/api/cache/exists',
    },

    // Health check
    HEALTH: '/health',
  },

  // SignalR Hub URL
  SIGNALR_HUB_URL: process.env.NODE_ENV === 'production'
    ? 'https://your-production-api.com/hubs/query-status'
    : 'http://localhost:55244/hubs/query-status',

  // Request configuration
  REQUEST_CONFIG: {
    timeout: 30000, // 30 seconds
    headers: {
      'Content-Type': 'application/json',
    },
  },

  // Streaming configuration
  STREAMING_CONFIG: {
    defaultChunkSize: 1000,
    maxRetries: 3,
    retryDelay: 1000, // 1 second
  },
};

// Helper function to get full API URL
export const getApiUrl = (endpoint: string): string => {
  return `${API_CONFIG.BASE_URL}${endpoint}`;
};

// Helper function to get authorization headers
export const getAuthHeaders = async (token?: string): Promise<Record<string, string>> => {
  const headers: Record<string, string> = {
    ...API_CONFIG.REQUEST_CONFIG.headers,
  };

  // If no token provided, try to get it from Zustand storage
  let authToken = token;
  if (!authToken) {
    try {
      const authStorage = localStorage.getItem('auth-storage');
      if (authStorage) {
        const parsed = JSON.parse(authStorage);
        const encryptedToken = parsed?.state?.token || null;

        if (encryptedToken) {
          // Import SecurityUtils for token decryption
          const { SecurityUtils } = await import('../utils/security');
          try {
            authToken = await SecurityUtils.decryptToken(encryptedToken);
          } catch (decryptError) {
            console.warn('Failed to decrypt token in getAuthHeaders:', decryptError);
            authToken = undefined;
          }
        }
      }
    } catch (error) {
      console.warn('Error getting auth token in getAuthHeaders:', error);
    }
  }

  if (authToken) {
    headers.Authorization = `Bearer ${authToken}`;
  }

  return headers;
};

// Helper function for API requests with error handling
export const apiRequest = async <T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> => {
  const url = getApiUrl(endpoint);

  const config: RequestInit = {
    ...options,
    headers: {
      ...API_CONFIG.REQUEST_CONFIG.headers,
      ...options.headers,
    },
  };

  try {
    const response = await fetch(url, config);

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (process.env.NODE_ENV === 'development') {
      console.error(`API request failed for ${endpoint}:`, error);
    }
    throw error;
  }
};

// Development mode configuration
export const DEV_CONFIG = {
  // Skip SSL certificate validation in development
  ignoreCertificateErrors: true,

  // Enable mock responses when backend is unavailable
  enableMockResponses: true,

  // Debug logging
  enableDebugLogging: true,
};
