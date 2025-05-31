// API Configuration for BI Reporting Copilot Frontend

export const API_CONFIG = {
  // Base URL for API calls
  BASE_URL: process.env.NODE_ENV === 'production'
    ? 'https://your-production-api.com'
    : 'http://localhost:55243',

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

    // Health check
    HEALTH: '/health',
  },

  // SignalR Hub URL
  SIGNALR_HUB_URL: process.env.NODE_ENV === 'production'
    ? 'https://your-production-api.com/queryHub'
    : 'http://localhost:55243/queryHub',

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
            authToken = null;
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
    console.error(`API request failed for ${endpoint}:`, error);
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
