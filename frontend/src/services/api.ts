import axios from 'axios';
import type { QueryRequest as TypedQueryRequest, QueryResponse as FrontendQueryResponse } from '../types/query';

// API Configuration
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:55243';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Function to get token from Zustand store
const getAuthToken = async (): Promise<string | null> => {
  try {
    // Import SecurityUtils for token decryption
    const { SecurityUtils } = await import('../utils/security');

    // Get token from Zustand persist storage
    const authStorage = localStorage.getItem('auth-storage');

    if (authStorage) {
      const parsed = JSON.parse(authStorage);
      const encryptedToken = parsed?.state?.token || null;

      if (encryptedToken) {
        // Decrypt the token before using it
        try {
          const decryptedToken = await SecurityUtils.decryptToken(encryptedToken);
          console.log('🔍 Successfully decrypted token');
          return decryptedToken;
        } catch (decryptError) {
          console.warn('❌ Failed to decrypt token:', decryptError);
          // Token might be corrupted, clear it
          localStorage.removeItem('auth-storage');
          return null;
        }
      }
    }

    // Fallback to legacy authToken storage (unencrypted)
    const legacyToken = localStorage.getItem('authToken');
    if (legacyToken) {
      console.log('🔍 Using legacy token');
      return legacyToken;
    }

    return null;
  } catch (error) {
    console.warn('❌ Error getting auth token:', error);
    return localStorage.getItem('authToken');
  }
};

// Request interceptor to add auth token
api.interceptors.request.use(
  async (config) => {
    const token = await getAuthToken();
    if (token) {
      console.log(`🔑 Adding auth token to ${config.method?.toUpperCase()} ${config.url}`);
      config.headers.Authorization = `Bearer ${token}`;
    } else {
      console.log(`⚠️ No auth token available for ${config.method?.toUpperCase()} ${config.url}`);
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      console.warn('🔒 401 Unauthorized - logging out user');
      console.log('❌ Request details:', {
        url: error.config?.url,
        method: error.config?.method,
        hasAuthHeader: !!error.config?.headers?.Authorization
      });

      // Import and call the auth store logout method
      try {
        const { useAuthStore } = await import('../stores/authStore');
        const authStore = useAuthStore.getState();
        authStore.logout();
      } catch (importError) {
        console.error('Failed to import auth store for logout:', importError);
        // Fallback to manual cleanup
        localStorage.removeItem('authToken');
        localStorage.removeItem('auth-storage');
        sessionStorage.clear();
      }

      // Redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Types
export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthenticationResult {
  success?: boolean;     // Lowercase for compatibility
  Success?: boolean;     // Backend uses capital S
  token?: string;
  refreshToken?: string;
  AccessToken?: string;  // Backend uses capital A
  RefreshToken?: string; // Backend uses capital R
  expiresAt?: string;
  user?: UserProfile;
  errorMessage?: string;
}

export interface UserProfile {
  id: string;
  username: string;
  email: string;
  displayName: string;
  profilePictureUrl?: string;
  createdDate: string;
  lastLoginDate: string;
  isActive: boolean;
  roles: string[];
}

// Legacy interface for backward compatibility
export interface LegacyQueryRequest {
  naturalLanguageQuery: string;
  connectionName?: string;
  maxRows?: number;
}

// Backend QueryRequest interface to match actual API request
export interface BackendQueryRequest {
  question: string;
  sessionId: string;
  options: {
    includeVisualization?: boolean;
    maxRows?: number;
    enableCache?: boolean;
    confidenceThreshold?: number;
    timeoutSeconds?: number;
    chunkSize?: number;
    enableStreaming?: boolean;
    dataSource?: string;
    dataSources?: string[];
    parameters?: Record<string, any>;
  };
}

export interface QueryResult {
  success: boolean;
  data?: any[];
  columns?: string[];
  executedSql?: string;
  executionTimeMs?: number;
  rowCount?: number;
  errorMessage?: string;
}

// Backend QueryResponse interface to match actual API response (PascalCase)
export interface QueryResponse {
  QueryId: string;
  Sql: string;
  Result?: {
    Data: any[];
    Metadata: {
      Columns: Array<{ Name: string; DataType: string; IsNullable: boolean }>;
      RowCount: number;
      ColumnCount: number;
      ExecutionTimeMs: number;
    };
    IsSuccessful: boolean;
  };
  Visualization?: any;
  Confidence: number;
  Suggestions: string[];
  Cached: boolean;
  Success: boolean;
  Timestamp: string;
  ExecutionTimeMs: number;
  Error?: string;
  PromptDetails?: {
    FullPrompt: string;
    TemplateName: string;
    TemplateVersion: string;
    Sections: Array<{
      Name: string;
      Title: string;
      Content: string;
      Type: string;
      Order: number;
      Metadata?: Record<string, any>;
    }>;
    Variables: Record<string, string>;
    TokenCount: number;
    GeneratedAt: string;
  };

  // Also support camelCase for backward compatibility
  queryId?: string;
  sql?: string;
  result?: {
    data: any[];
    metadata: {
      columns: Array<{ name: string; dataType: string; isNullable: boolean }>;
      rowCount: number;
      columnCount: number;
      executionTimeMs: number;
    };
    isSuccessful: boolean;
  };
  visualization?: any;
  confidence?: number;
  suggestions?: string[];
  cached?: boolean;
  success?: boolean;
  timestamp?: string;
  executionTimeMs?: number;
  error?: string;
}

// Enhanced AI Types
export interface EnhancedQueryRequest {
  query: string;
  executeQuery?: boolean;
  includeAlternatives?: boolean;
  includeSemanticAnalysis?: boolean;
}

export interface EnhancedQueryResponse {
  processedQuery?: ProcessedQuery;
  queryResult?: QueryResult;
  semanticAnalysis?: SemanticAnalysisResponse;
  classification?: ClassificationResponse;
  alternatives?: AlternativeQueryResponse[];
  success: boolean;
  errorMessage?: string;
  timestamp: string;
}

export interface ProcessedQuery {
  sql: string;
  explanation: string;
  confidence: number;
  alternativeQueries?: AlternativeQueryResponse[];
  semanticEntities?: SemanticEntity[];
  classification?: QueryClassification;
  usedSchema?: SchemaContext;
}

export interface SemanticAnalysisResponse {
  intent: string;
  entities: EntityResponse[];
  keywords: string[];
  confidence: number;
  processedQuery: string;
  metadata: Record<string, any>;
}

export interface EntityResponse {
  text: string;
  type: string;
  confidence: number;
  startPosition?: number;
  endPosition?: number;
}

export interface ClassificationResponse {
  category: string;
  complexity: string;
  requiredJoins: string[];
  predictedTables: string[];
  estimatedExecutionTime: string;
  recommendedVisualization: string;
  confidenceScore: number;
  optimizationSuggestions: string[];
}

export interface AlternativeQueryResponse {
  sql: string;
  score: number;
  reasoning: string;
  strengths: string[];
  weaknesses: string[];
}

export interface SimilarQueryResponse {
  sql: string;
  explanation: string;
  confidence: number;
  classification: string;
}

export interface UserContextResponse {
  domain: string;
  preferredTables: string[];
  commonFilters: string[];
  recentPatterns: QueryPatternResponse[];
  lastUpdated: string;
}

export interface QueryPatternResponse {
  pattern: string;
  frequency: number;
  lastUsed: string;
  intent: string;
  associatedTables: string[];
}

export interface SimilarityRequest {
  query1: string;
  query2: string;
}

export interface SimilarityResponse {
  similarityScore: number;
  commonEntities: string[];
  commonKeywords: string[];
  analysis: string;
}

// Additional types for enhanced features
export interface SemanticEntity {
  text: string;
  type: string;
  confidence: number;
  startPosition?: number;
  endPosition?: number;
  resolvedValue?: string;
  properties?: Record<string, any>;
}

export interface QueryClassification {
  category: string;
  complexity: string;
  requiredJoins: string[];
  predictedTables: string[];
  estimatedExecutionTime: string;
  recommendedVisualization: string;
  confidenceScore: number;
  optimizationSuggestions: string[];
}

export interface SchemaContext {
  relevantTables: any[];
  relationships: any[];
  suggestedJoins: string[];
  columnMappings: Record<string, string>;
  businessTerms: string[];
}

export interface DashboardOverview {
  userActivity: UserActivitySummary;
  recentQueries: QueryHistoryItem[];
  systemMetrics: SystemMetrics;
  quickStats: QuickStats;
}

export interface UserActivitySummary {
  totalQueries: number;
  queriesThisWeek: number;
  queriesThisMonth: number;
  averageQueryTime: number;
  lastActivity: string;
}

export interface QueryHistoryItem {
  id: string;
  naturalLanguageQuery: string;
  executedSql: string;
  executionTimeMs: number;
  timestamp: string;
  success: boolean;
  rowCount?: number;
}

export interface SystemMetrics {
  databaseConnections: number;
  cacheHitRate: number;
  averageQueryTime: number;
  systemUptime: string;
}

export interface QuickStats {
  totalQueries: number;
  queriesThisWeek: number;
  averageQueryTime: number;
  favoriteTable: string;
}

// API Service Class
export class ApiService {
  // Authentication
  static async login(credentials: LoginRequest): Promise<AuthenticationResult> {
    const response = await api.post('/api/auth/login', credentials);
    return response.data;
  }

  static async validateToken(): Promise<any> {
    const response = await api.get('/api/auth/validate');
    return response.data;
  }

  static async logout(): Promise<void> {
    // Get refresh token from Zustand storage or fallback
    let refreshToken = null;
    try {
      const authStorage = localStorage.getItem('auth-storage');
      if (authStorage) {
        const parsed = JSON.parse(authStorage);
        refreshToken = parsed?.state?.refreshToken;
      }
    } catch (error) {
      console.warn('Error getting refresh token from Zustand storage:', error);
    }

    // Fallback to legacy storage
    if (!refreshToken) {
      refreshToken = localStorage.getItem('refreshToken');
    }

    if (refreshToken) {
      await api.post('/api/auth/logout', { refreshToken });
    }

    // Clear all storage mechanisms
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('auth-storage');
  }

  // Query Operations
  static async executeQuery(request: TypedQueryRequest | LegacyQueryRequest): Promise<FrontendQueryResponse> {
    // Handle both new and legacy request formats
    let backendRequest;

    if ('question' in request) {
      // New format - already correct
      backendRequest = request;
    } else {
      // Legacy format - transform to new format
      backendRequest = {
        question: (request as LegacyQueryRequest).naturalLanguageQuery,
        sessionId: Date.now().toString(),
        options: {
          includeVisualization: true,
          maxRows: (request as LegacyQueryRequest).maxRows || 1000,
          enableCache: true,
          confidenceThreshold: 0.7,
          timeoutSeconds: 30
        }
      };
    }

    const response = await api.post('/api/query/execute', backendRequest);
    const backendResponse: QueryResponse = response.data;

    // Transform backend QueryResponse to match frontend QueryResponse interface
    // Note: Backend uses PascalCase (Success, Result, Data) while frontend expects camelCase
    return {
      queryId: backendResponse.QueryId || backendResponse.queryId || '',
      sql: backendResponse.Sql || backendResponse.sql || '',
      result: {
        data: backendResponse.Result?.Data || backendResponse.result?.data || [],
        metadata: {
          columnCount: backendResponse.Result?.Metadata?.ColumnCount || backendResponse.result?.metadata?.columnCount || 0,
          rowCount: backendResponse.Result?.Metadata?.RowCount || backendResponse.result?.metadata?.rowCount || 0,
          executionTimeMs: backendResponse.Result?.Metadata?.ExecutionTimeMs || backendResponse.result?.metadata?.executionTimeMs || 0,
          columns: backendResponse.Result?.Metadata?.Columns?.map(col => ({
            name: col.Name || col.name || '',
            dataType: col.DataType || col.dataType || '',
            isNullable: col.IsNullable || col.isNullable || false,
            description: col.Description || col.description,
            semanticTags: col.SemanticTags || col.semanticTags || []
          })) || backendResponse.result?.metadata?.columns || [],
          dataSource: backendResponse.Result?.Metadata?.DataSource || backendResponse.result?.metadata?.dataSource,
          queryTimestamp: backendResponse.Result?.Metadata?.QueryTimestamp || backendResponse.result?.metadata?.queryTimestamp || new Date().toISOString()
        }
      },
      visualization: backendResponse.Visualization || backendResponse.visualization,
      confidence: backendResponse.Confidence || backendResponse.confidence || 0,
      suggestions: backendResponse.Suggestions || backendResponse.suggestions || [],
      cached: backendResponse.Cached || backendResponse.cached || false,
      success: backendResponse.Success || backendResponse.success || false,
      error: backendResponse.Error || backendResponse.error,
      timestamp: backendResponse.Timestamp || backendResponse.timestamp || new Date().toISOString(),
      executionTimeMs: backendResponse.ExecutionTimeMs || backendResponse.executionTimeMs || 0,
      promptDetails: backendResponse.PromptDetails ? {
        fullPrompt: backendResponse.PromptDetails.FullPrompt,
        templateName: backendResponse.PromptDetails.TemplateName,
        templateVersion: backendResponse.PromptDetails.TemplateVersion,
        sections: backendResponse.PromptDetails.Sections.map(section => ({
          name: section.Name,
          title: section.Title,
          content: section.Content,
          type: section.Type,
          order: section.Order,
          metadata: section.Metadata
        })),
        variables: backendResponse.PromptDetails.Variables,
        tokenCount: backendResponse.PromptDetails.TokenCount,
        generatedAt: backendResponse.PromptDetails.GeneratedAt
      } : undefined
    };
  }

  static async getQueryHistory(page: number = 1, pageSize: number = 20): Promise<any> {
    const response = await api.get(`/api/query/history?page=${page}&pageSize=${pageSize}`);
    return response.data;
  }

  static async getQuerySuggestions(context?: string): Promise<string[]> {
    const response = await api.get('/api/query/suggestions', { params: { context } });
    return response.data;
  }

  // Enhanced AI Query Operations
  static async processEnhancedQuery(request: EnhancedQueryRequest): Promise<EnhancedQueryResponse> {
    const response = await api.post('/api/enhanced-query/process', request);
    return response.data;
  }

  static async analyzeQuery(query: string): Promise<SemanticAnalysisResponse> {
    const response = await api.post('/api/enhanced-query/analyze', JSON.stringify(query), {
      headers: { 'Content-Type': 'application/json' }
    });
    return response.data;
  }

  static async classifyQuery(query: string): Promise<ClassificationResponse> {
    const response = await api.post('/api/enhanced-query/classify', JSON.stringify(query), {
      headers: { 'Content-Type': 'application/json' }
    });
    return response.data;
  }

  static async getEnhancedQuerySuggestions(context?: string): Promise<string[]> {
    const response = await api.get('/api/enhanced-query/suggestions', { params: { context } });
    return response.data;
  }

  static async findSimilarQueries(query: string, limit: number = 5): Promise<SimilarQueryResponse[]> {
    const response = await api.post('/api/enhanced-query/similar', JSON.stringify(query), {
      params: { limit },
      headers: { 'Content-Type': 'application/json' }
    });
    return response.data;
  }

  static async calculateSimilarity(request: SimilarityRequest): Promise<SimilarityResponse> {
    const response = await api.post('/api/enhanced-query/similarity', request);
    return response.data;
  }

  static async getUserContext(): Promise<UserContextResponse> {
    const response = await api.get('/api/enhanced-query/context');
    return response.data;
  }

  // Dashboard
  static async getDashboardOverview(): Promise<DashboardOverview> {
    const response = await api.get('/api/dashboard/overview');
    return response.data;
  }

  static async getAnalytics(days: number = 30): Promise<any> {
    const response = await api.get(`/api/dashboard/analytics?days=${days}`);
    return response.data;
  }

  // Schema
  static async getSchema(connectionName?: string): Promise<any> {
    const response = await api.get('/api/schema', { params: { connectionName } });
    return response.data;
  }

  static async getTableDetails(tableName: string, connectionName?: string): Promise<any> {
    const response = await api.get(`/api/schema/tables/${tableName}`, { params: { connectionName } });
    return response.data;
  }

  // User
  static async getUserProfile(): Promise<UserProfile> {
    const response = await api.get('/api/user/profile');
    return response.data;
  }

  static async updateUserProfile(profile: Partial<UserProfile>): Promise<UserProfile> {
    const response = await api.put('/api/user/profile', profile);
    return response.data;
  }

  // Health Check
  static async getHealthStatus(): Promise<any> {
    const response = await api.get('/health');
    return response.data;
  }

  static async healthCheck(checkType: 'database' | 'api' | 'cache'): Promise<any> {
    const response = await api.get(`/health/${checkType}`);
    return response.data;
  }

  // Favorites
  static async addToFavorites(query: { query: string; timestamp: string; description: string }): Promise<any> {
    const response = await api.post('/api/query/favorites', query);
    return response.data;
  }

  static async getFavorites(): Promise<any[]> {
    const response = await api.get('/api/query/favorites');
    return response.data;
  }

  // Cache Management
  static async clearCache(pattern?: string): Promise<any> {
    const response = await api.delete('/api/cache/clear', { params: { pattern } });
    return response.data;
  }

  static async getCacheMetrics(): Promise<any> {
    const response = await api.get('/api/cache/metrics');
    return response.data;
  }
}

export default api;
