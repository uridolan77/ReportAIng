/**
 * Modern API Service
 * 
 * Consolidated API service with advanced features including retry logic,
 * caching, request/response interceptors, and comprehensive error handling.
 */

import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { CacheService } from './CacheService';
import { LoggingService } from './LoggingService';
import { ErrorService } from './ErrorService';

export interface ApiConfig {
  baseURL: string;
  timeout: number;
  retryAttempts: number;
  retryDelay: number;
  enableCaching: boolean;
  enableLogging: boolean;
  enableSigning: boolean;
  enableEncryption: boolean;
  rateLimitConfig?: {
    maxRequests: number;
    windowMs: number;
  };
}

export interface ApiResponse<T = any> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
  metadata?: {
    timestamp: string;
    requestId: string;
    executionTime: number;
  };
}

export class ApiService {
  private client: AxiosInstance;
  private config: ApiConfig;
  private requestQueue: Map<string, Promise<any>> = new Map();
  private rateLimitTracker: Map<string, number[]> = new Map();

  constructor(
    config: Partial<ApiConfig> = {},
    private cacheService?: CacheService,
    private loggingService?: LoggingService,
    private errorService?: ErrorService
  ) {
    this.config = {
      baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000',
      timeout: 30000,
      retryAttempts: 3,
      retryDelay: 1000,
      enableCaching: true,
      enableLogging: true,
      enableSigning: false,
      enableEncryption: false,
      ...config,
    };

    this.client = this.createAxiosInstance();
    this.setupInterceptors();
  }

  private createAxiosInstance(): AxiosInstance {
    return axios.create({
      baseURL: this.config.baseURL,
      timeout: this.config.timeout,
      headers: {
        'Content-Type': 'application/json',
        'X-Client-Version': process.env.REACT_APP_VERSION || '1.0.0',
      },
    });
  }

  private setupInterceptors(): void {
    // Request interceptor
    this.client.interceptors.request.use(
      (config) => {
        // Add request ID for tracking
        config.headers['X-Request-ID'] = this.generateRequestId();
        
        // Add timestamp
        config.metadata = { startTime: Date.now() };
        
        // Rate limiting check
        if (this.config.rateLimitConfig && !this.checkRateLimit(config.url || '')) {
          throw new Error('Rate limit exceeded');
        }
        
        // Logging
        if (this.config.enableLogging && this.loggingService) {
          this.loggingService.logRequest(config);
        }
        
        return config;
      },
      (error) => {
        if (this.errorService) {
          this.errorService.handleError(error);
        }
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => {
        // Calculate execution time
        const executionTime = Date.now() - (response.config.metadata?.startTime || 0);
        
        // Add metadata
        if (response.data && typeof response.data === 'object') {
          response.data.metadata = {
            ...response.data.metadata,
            executionTime,
            timestamp: new Date().toISOString(),
            requestId: response.config.headers['X-Request-ID'],
          };
        }
        
        // Logging
        if (this.config.enableLogging && this.loggingService) {
          this.loggingService.logResponse(response);
        }
        
        return response;
      },
      async (error) => {
        // Retry logic
        if (this.shouldRetry(error)) {
          return this.retryRequest(error.config);
        }
        
        // Error handling
        if (this.errorService) {
          this.errorService.handleError(error);
        }
        
        return Promise.reject(error);
      }
    );
  }

  private generateRequestId(): string {
    return `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private checkRateLimit(endpoint: string): boolean {
    if (!this.config.rateLimitConfig) return true;
    
    const now = Date.now();
    const { maxRequests, windowMs } = this.config.rateLimitConfig;
    
    if (!this.rateLimitTracker.has(endpoint)) {
      this.rateLimitTracker.set(endpoint, []);
    }
    
    const requests = this.rateLimitTracker.get(endpoint)!;
    
    // Remove old requests outside the window
    const validRequests = requests.filter(time => now - time < windowMs);
    
    if (validRequests.length >= maxRequests) {
      return false;
    }
    
    validRequests.push(now);
    this.rateLimitTracker.set(endpoint, validRequests);
    
    return true;
  }

  private shouldRetry(error: any): boolean {
    if (!error.config || error.config.__retryCount >= this.config.retryAttempts) {
      return false;
    }
    
    // Retry on network errors or 5xx status codes
    return !error.response || (error.response.status >= 500 && error.response.status < 600);
  }

  private async retryRequest(config: any): Promise<any> {
    config.__retryCount = (config.__retryCount || 0) + 1;
    
    // Exponential backoff
    const delay = this.config.retryDelay * Math.pow(2, config.__retryCount - 1);
    await new Promise(resolve => setTimeout(resolve, delay));
    
    return this.client(config);
  }

  // Public API methods
  async get<T = any>(
    url: string, 
    config?: AxiosRequestConfig,
    options?: { cache?: boolean; cacheKey?: string; cacheTTL?: number }
  ): Promise<ApiResponse<T>> {
    const cacheKey = options?.cacheKey || `GET:${url}:${JSON.stringify(config?.params || {})}`;
    
    // Check cache first
    if (options?.cache !== false && this.config.enableCaching && this.cacheService) {
      const cached = await this.cacheService.get<ApiResponse<T>>(cacheKey);
      if (cached) {
        return cached;
      }
    }
    
    // Deduplicate identical requests
    if (this.requestQueue.has(cacheKey)) {
      return this.requestQueue.get(cacheKey);
    }
    
    const request = this.client.get<ApiResponse<T>>(url, config)
      .then(response => {
        const result = response.data;
        
        // Cache successful responses
        if (this.config.enableCaching && this.cacheService && result.success) {
          this.cacheService.set(cacheKey, result, options?.cacheTTL);
        }
        
        return result;
      })
      .finally(() => {
        this.requestQueue.delete(cacheKey);
      });
    
    this.requestQueue.set(cacheKey, request);
    return request;
  }

  async post<T = any>(
    url: string, 
    data?: any, 
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.post<ApiResponse<T>>(url, data, config);
    return response.data;
  }

  async put<T = any>(
    url: string, 
    data?: any, 
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.put<ApiResponse<T>>(url, data, config);
    return response.data;
  }

  async patch<T = any>(
    url: string, 
    data?: any, 
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.patch<ApiResponse<T>>(url, data, config);
    return response.data;
  }

  async delete<T = any>(
    url: string, 
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.delete<ApiResponse<T>>(url, config);
    return response.data;
  }

  // Configuration methods
  updateConfig(newConfig: Partial<ApiConfig>): void {
    this.config = { ...this.config, ...newConfig };
    
    // Update axios instance if needed
    if (newConfig.baseURL || newConfig.timeout) {
      this.client.defaults.baseURL = this.config.baseURL;
      this.client.defaults.timeout = this.config.timeout;
    }
  }

  getConfig(): ApiConfig {
    return { ...this.config };
  }

  // Utility methods
  clearCache(): void {
    if (this.cacheService) {
      this.cacheService.clear();
    }
  }

  clearRequestQueue(): void {
    this.requestQueue.clear();
  }

  getRequestQueueSize(): number {
    return this.requestQueue.size;
  }
}
