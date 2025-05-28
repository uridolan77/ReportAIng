import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { z } from 'zod';
import { ValidationUtils, ValidationResult, SchemaValidationError } from '../utils/validation';
import {
  ApiResponseSchema,
  AuthenticationResultSchema,
  QueryResponseSchema,
  EnhancedQueryResponseSchema,
  SchemaMetadataSchema,
  DashboardOverviewSchema,
  HealthStatusSchema,
  QueryHistoryItemSchema,
  UserProfileSchema,
  LoginRequestSchema,
  QueryRequestSchema,
} from '../schemas/api';

// Enhanced API client with automatic validation
export class ValidatedApiClient {
  private client: AxiosInstance;
  private baseURL: string;
  private defaultTimeout: number = 30000;

  constructor(baseURL: string, config?: AxiosRequestConfig) {
    this.baseURL = baseURL;
    this.client = axios.create({
      baseURL,
      timeout: this.defaultTimeout,
      headers: {
        'Content-Type': 'application/json',
      },
      ...config,
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    // Request interceptor for authentication and logging
    this.client.interceptors.request.use(
      (config) => {
        // Add auth token if available
        const token = localStorage.getItem('authToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }

        // Add request ID for tracing
        config.headers['X-Request-ID'] = this.generateRequestId();

        // Log request in development
        if (process.env.NODE_ENV === 'development') {
          console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`, {
            headers: config.headers,
            data: config.data,
          });
        }

        return config;
      },
      (error) => {
        console.error('Request interceptor error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor for error handling and logging
    this.client.interceptors.response.use(
      (response) => {
        // Log response in development
        if (process.env.NODE_ENV === 'development') {
          console.log(`API Response: ${response.status} ${response.config.url}`, {
            data: response.data,
            headers: response.headers,
          });
        }

        return response;
      },
      (error) => {
        // Handle authentication errors
        if (error.response?.status === 401) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('auth-storage');
          window.location.href = '/login';
        }

        // Log error details
        console.error('API Error:', {
          status: error.response?.status,
          statusText: error.response?.statusText,
          url: error.config?.url,
          method: error.config?.method,
          data: error.response?.data,
          message: error.message,
        });

        return Promise.reject(error);
      }
    );
  }

  private generateRequestId(): string {
    return `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Make a validated API request
   */
  private async makeValidatedRequest<T>(
    method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH',
    url: string,
    responseSchema: z.ZodSchema<T>,
    config?: AxiosRequestConfig,
    requestData?: unknown,
    requestSchema?: z.ZodSchema<unknown>
  ): Promise<T> {
    try {
      // Validate request data if schema provided
      if (requestData && requestSchema) {
        ValidationUtils.validateOrThrow(requestSchema, requestData, {
          stripUnknown: true,
          errorFormat: 'detailed',
        });
      }

      // Make the request
      const response: AxiosResponse = await this.client.request({
        method,
        url,
        data: requestData,
        ...config,
      });

      // Validate response data
      const validatedData = ValidationUtils.validateApiResponse(
        responseSchema,
        response.data,
        `${method} ${url}`
      );

      return validatedData;
    } catch (error) {
      if (error instanceof SchemaValidationError) {
        throw new Error(`API validation failed for ${method} ${url}: ${error.message}`);
      }
      throw error;
    }
  }

  /**
   * Make a request that returns an API response wrapper
   */
  private async makeApiRequest<T>(
    method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH',
    url: string,
    dataSchema: z.ZodSchema<T>,
    config?: AxiosRequestConfig,
    requestData?: unknown,
    requestSchema?: z.ZodSchema<unknown>
  ): Promise<T> {
    const responseSchema = ApiResponseSchema(dataSchema);
    const response = await this.makeValidatedRequest(
      method,
      url,
      responseSchema,
      config,
      requestData,
      requestSchema
    );

    if (!response.success) {
      throw new Error(response.error || response.message || 'API request failed');
    }

    if (!response.data) {
      throw new Error('API response missing data');
    }

    return response.data;
  }

  // Authentication methods
  async login(credentials: { username: string; password: string }): Promise<{
    success: boolean;
    token?: string;
    refreshToken?: string;
    expiresAt?: string;
    user?: any;
    errorMessage?: string;
  }> {
    return this.makeValidatedRequest(
      'POST',
      '/api/auth/login',
      AuthenticationResultSchema,
      {},
      credentials,
      LoginRequestSchema
    );
  }

  async logout(): Promise<void> {
    await this.makeValidatedRequest(
      'POST',
      '/api/auth/logout',
      z.object({ success: z.boolean() })
    );
  }

  async refreshToken(): Promise<{
    success: boolean;
    token?: string;
    expiresAt?: string;
  }> {
    return this.makeValidatedRequest(
      'POST',
      '/api/auth/refresh',
      z.object({
        success: z.boolean(),
        token: z.string().optional(),
        expiresAt: z.string().optional(),
      })
    );
  }

  async getCurrentUser(): Promise<any> {
    return this.makeApiRequest(
      'GET',
      '/api/auth/user',
      UserProfileSchema
    );
  }

  // Query methods
  async executeQuery(request: {
    naturalLanguageQuery: string;
    includeExplanation?: boolean;
    maxRows?: number;
    timeout?: number;
    useCache?: boolean;
    context?: Record<string, unknown>;
  }): Promise<{
    queryId: string;
    sql: string;
    result?: any;
    explanation?: string;
    confidence: number;
    suggestions?: string[];
    warnings?: string[];
    executionTimeMs: number;
    fromCache?: boolean;
  }> {
    return this.makeApiRequest(
      'POST',
      '/api/query/execute',
      QueryResponseSchema,
      {},
      request,
      QueryRequestSchema
    );
  }

  async executeEnhancedQuery(request: {
    naturalLanguageQuery: string;
    includeExplanation?: boolean;
    maxRows?: number;
    timeout?: number;
    useCache?: boolean;
    context?: Record<string, unknown>;
  }): Promise<any> {
    return this.makeApiRequest(
      'POST',
      '/api/query/enhanced',
      EnhancedQueryResponseSchema,
      {},
      request,
      QueryRequestSchema
    );
  }

  async validateSql(sql: string): Promise<{
    isValid: boolean;
    errors: string[];
    warnings: string[];
    suggestions: string[];
  }> {
    return this.makeApiRequest(
      'POST',
      '/api/query/validate',
      z.object({
        isValid: z.boolean(),
        errors: z.array(z.string()),
        warnings: z.array(z.string()),
        suggestions: z.array(z.string()),
      }),
      {},
      { sql },
      z.object({ sql: z.string() })
    );
  }

  async getQueryHistory(page: number = 1, pageSize: number = 20): Promise<any[]> {
    return this.makeApiRequest(
      'GET',
      '/api/query/history',
      z.array(QueryHistoryItemSchema),
      {
        params: { page, pageSize },
      }
    );
  }

  async getQuerySuggestions(): Promise<string[]> {
    return this.makeApiRequest(
      'GET',
      '/api/query/suggestions',
      z.array(z.string())
    );
  }

  // Schema methods
  async getSchemaMetadata(): Promise<any> {
    return this.makeApiRequest(
      'GET',
      '/api/schema',
      SchemaMetadataSchema
    );
  }

  async getTableMetadata(tableName: string): Promise<any> {
    return this.makeApiRequest(
      'GET',
      `/api/schema/tables/${tableName}`,
      z.object({
        name: z.string(),
        schema: z.string(),
        columns: z.array(z.object({
          name: z.string(),
          dataType: z.string(),
          isNullable: z.boolean(),
          maxLength: z.number().optional(),
          description: z.string().optional(),
        })),
        description: z.string().optional(),
      })
    );
  }

  // Dashboard methods
  async getDashboardOverview(): Promise<any> {
    return this.makeApiRequest(
      'GET',
      '/api/dashboard/overview',
      DashboardOverviewSchema
    );
  }

  // Health check methods
  async getHealthStatus(): Promise<any> {
    return this.makeApiRequest(
      'GET',
      '/api/health',
      HealthStatusSchema
    );
  }

  // Streaming methods (with validation)
  async startStreamingQuery(
    query: string,
    onData: (data: any) => void,
    onError: (error: Error) => void,
    onComplete: () => void
  ): Promise<void> {
    try {
      const response = await this.client.post('/api/streaming/stream-query', 
        { naturalLanguageQuery: query },
        {
          responseType: 'stream',
          timeout: 0, // No timeout for streaming
        }
      );

      const stream = response.data;
      let buffer = '';

      stream.on('data', (chunk: Buffer) => {
        buffer += chunk.toString();
        const lines = buffer.split('\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          if (line.trim()) {
            try {
              const data = JSON.parse(line);
              // Validate streaming data if needed
              onData(data);
            } catch (error) {
              console.warn('Failed to parse streaming data:', line);
            }
          }
        }
      });

      stream.on('end', () => {
        if (buffer.trim()) {
          try {
            const data = JSON.parse(buffer);
            onData(data);
          } catch (error) {
            console.warn('Failed to parse final streaming data:', buffer);
          }
        }
        onComplete();
      });

      stream.on('error', (error: Error) => {
        onError(error);
      });
    } catch (error) {
      onError(error instanceof Error ? error : new Error('Streaming request failed'));
    }
  }

  // Utility methods
  setAuthToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  clearAuthToken(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('auth-storage');
  }

  setTimeout(timeout: number): void {
    this.defaultTimeout = timeout;
    this.client.defaults.timeout = timeout;
  }

  getBaseURL(): string {
    return this.baseURL;
  }
}

// Create and export a default instance
export const validatedApi = new ValidatedApiClient(
  process.env.REACT_APP_API_BASE_URL || 'http://localhost:55243'
);

// Export individual methods for convenience
export const {
  login,
  logout,
  refreshToken,
  getCurrentUser,
  executeQuery,
  executeEnhancedQuery,
  validateSql,
  getQueryHistory,
  getQuerySuggestions,
  getSchemaMetadata,
  getTableMetadata,
  getDashboardOverview,
  getHealthStatus,
  startStreamingQuery,
} = validatedApi;
