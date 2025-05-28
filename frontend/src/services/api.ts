import axios from 'axios';

// API Configuration
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:55243';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Function to get token from Zustand store
const getAuthToken = (): string | null => {
  try {
    // Debug: Log all localStorage keys to see what's available
    console.log('🔍 Available localStorage keys:', Object.keys(localStorage));

    // Get token from Zustand persist storage
    const authStorage = localStorage.getItem('auth-storage');
    console.log('🔍 auth-storage content:', authStorage);

    if (authStorage) {
      const parsed = JSON.parse(authStorage);
      console.log('🔍 Parsed auth storage:', parsed);
      const token = parsed?.state?.token || null;
      console.log('🔍 Extracted token:', token ? `${token.substring(0, 20)}...` : 'null');
      return token;
    }

    // Fallback to legacy authToken storage
    const legacyToken = localStorage.getItem('authToken');
    console.log('🔍 Legacy authToken:', legacyToken ? `${legacyToken.substring(0, 20)}...` : 'null');
    return legacyToken;
  } catch (error) {
    console.warn('❌ Error getting auth token:', error);
    return localStorage.getItem('authToken');
  }
};

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
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
  (error) => {
    if (error.response?.status === 401) {
      // Clear both storage mechanisms
      localStorage.removeItem('authToken');
      localStorage.removeItem('auth-storage');
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
  success: boolean;
  token?: string;
  refreshToken?: string;
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

export interface QueryRequest {
  naturalLanguageQuery: string;
  connectionName?: string;
  maxRows?: number;
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
  static async executeQuery(request: QueryRequest): Promise<QueryResult> {
    const response = await api.post('/api/query/execute', request);
    return response.data;
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
}

export default api;
