/**
 * Secure API Client with Request Signing
 * Enhanced API client that includes cryptographic request signing for security
 */

import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { requestSigning, SignedRequest } from './requestSigning';
import { SecurityUtils } from '../utils/security';
import { useAuthStore } from '../stores/authStore';

export interface SecureApiConfig {
  baseURL?: string;
  timeout?: number;
  enableSigning?: boolean;
  enableEncryption?: boolean;
  retryAttempts?: number;
  rateLimitConfig?: {
    maxRequests: number;
    windowMs: number;
  };
}

export interface RequestMetrics {
  requestId: string;
  timestamp: number;
  method: string;
  url: string;
  duration: number;
  status: number;
  signed: boolean;
  encrypted: boolean;
}

export class SecureApiClient {
  private client: AxiosInstance;
  private config: SecureApiConfig;
  private requestMetrics: RequestMetrics[] = [];
  private rateLimiter: (() => boolean) | null = null;
  private static instance: SecureApiClient;

  private constructor(config: SecureApiConfig = {}) {
    this.config = {
      baseURL: config.baseURL || process.env.REACT_APP_API_URL || 'https://localhost:55243',
      timeout: config.timeout || 30000,
      enableSigning: config.enableSigning ?? true,
      enableEncryption: config.enableEncryption ?? false,
      retryAttempts: config.retryAttempts ?? 3,
      rateLimitConfig: config.rateLimitConfig || { maxRequests: 100, windowMs: 60000 },
      ...config,
    };

    this.client = axios.create({
      baseURL: this.config.baseURL,
      timeout: this.config.timeout,
      headers: {
        'Content-Type': 'application/json',
        'X-Client-Version': process.env.REACT_APP_VERSION || '1.0.0',
        'X-Client-Type': 'web',
      },
    });

    this.setupInterceptors();
    this.initializeRateLimiter();
    this.initializeSigning();
  }

  public static getInstance(config?: SecureApiConfig): SecureApiClient {
    if (!SecureApiClient.instance) {
      SecureApiClient.instance = new SecureApiClient(config);
    }
    return SecureApiClient.instance;
  }

  /**
   * Initialize request signing
   */
  private async initializeSigning(): Promise<void> {
    if (this.config.enableSigning) {
      try {
        await requestSigning.initialize();
        console.log('✅ Request signing initialized');
      } catch (error) {
        console.error('❌ Failed to initialize request signing:', error);
        this.config.enableSigning = false;
      }
    }
  }

  /**
   * Initialize rate limiter
   */
  private initializeRateLimiter(): void {
    if (this.config.rateLimitConfig) {
      this.rateLimiter = SecurityUtils.createRateLimiter(
        this.config.rateLimitConfig.maxRequests,
        this.config.rateLimitConfig.windowMs
      );
    }
  }

  /**
   * Setup request/response interceptors
   */
  private setupInterceptors(): void {
    // Request interceptor
    this.client.interceptors.request.use(
      async (config) => {
        const startTime = Date.now();
        const requestId = this.generateRequestId();

        // Rate limiting check
        if (this.rateLimiter && !this.rateLimiter()) {
          throw new Error('Rate limit exceeded. Please try again later.');
        }

        // Add request ID for tracing
        config.headers['X-Request-ID'] = requestId;
        (config as any).metadata = { startTime, requestId };

        // Add authentication token
        const token = await this.getAuthToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }

        // Request signing
        if (this.config.enableSigning) {
          try {
            const signedRequest = await requestSigning.signRequest(
              config.method?.toUpperCase() || 'GET',
              config.url || '',
              config.headers as Record<string, string>,
              config.data
            );

            // Add signing headers
            Object.assign(config.headers, signedRequest.headers);
          } catch (error) {
            if (process.env.NODE_ENV === 'development') {
              console.warn('Request signing failed:', error);
            }
            // Continue without signing in case of error
          }
        }

        // Request encryption (if enabled)
        if (this.config.enableEncryption && config.data) {
          try {
            config.data = await this.encryptRequestData(config.data);
            config.headers['X-Content-Encrypted'] = 'true';
          } catch (error) {
            if (process.env.NODE_ENV === 'development') {
              console.warn('Request encryption failed:', error);
            }
          }
        }

        // Log request in development
        if (process.env.NODE_ENV === 'development') {
          console.log(`🔐 Secure API Request: ${config.method?.toUpperCase()} ${config.url}`, {
            requestId,
            signed: this.config.enableSigning,
            encrypted: this.config.enableEncryption && !!config.data,
            headers: this.sanitizeHeaders(config.headers),
          });
        }

        return config;
      },
      (error) => {
        console.error('Request interceptor error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => {
        this.recordRequestMetrics(response);

        // Decrypt response if needed
        if (response.headers['x-content-encrypted'] === 'true') {
          return this.decryptResponseData(response);
        }

        return response;
      },
      async (error) => {
        this.recordRequestMetrics(error.response, error);

        // Handle authentication errors
        if (error.response?.status === 401) {
          const authStore = useAuthStore.getState();
          const refreshed = await authStore.refreshAuth();

          if (refreshed && this.config.retryAttempts && this.config.retryAttempts > 0) {
            // Retry the original request
            return this.client.request(error.config);
          } else {
            authStore.logout();
            window.location.href = '/login';
          }
        }

        // Handle rate limiting
        if (error.response?.status === 429) {
          const retryAfter = error.response.headers['retry-after'];
          if (retryAfter && process.env.NODE_ENV === 'development') {
            console.warn(`Rate limited. Retry after ${retryAfter} seconds`);
          }
        }

        return Promise.reject(error);
      }
    );
  }

  /**
   * Get authentication token
   */
  private async getAuthToken(): Promise<string | null> {
    try {
      const authStore = useAuthStore.getState();
      return await authStore.getDecryptedToken();
    } catch (error) {
      console.warn('Failed to get auth token:', error);
      return null;
    }
  }

  /**
   * Encrypt request data
   */
  private async encryptRequestData(data: any): Promise<string> {
    try {
      const jsonData = JSON.stringify(data);
      return await SecurityUtils.encryptData(jsonData);
    } catch (error) {
      console.error('Request encryption failed:', error);
      throw error;
    }
  }

  /**
   * Decrypt response data
   */
  private async decryptResponseData(response: AxiosResponse): Promise<AxiosResponse> {
    try {
      if (typeof response.data === 'string') {
        const decryptedData = await SecurityUtils.decryptData(response.data);
        response.data = JSON.parse(decryptedData);
      }
      return response;
    } catch (error) {
      console.error('Response decryption failed:', error);
      throw error;
    }
  }

  /**
   * Record request metrics
   */
  private recordRequestMetrics(response?: AxiosResponse, error?: any): void {
    try {
      const config = response?.config || error?.config;
      const metadata = (config as any)?.metadata;
      if (!metadata) return;

      const { startTime, requestId } = metadata;
      const duration = Date.now() - startTime;

      const metrics: RequestMetrics = {
        requestId,
        timestamp: startTime,
        method: config.method?.toUpperCase() || 'UNKNOWN',
        url: config.url || '',
        duration,
        status: response?.status || error?.response?.status || 0,
        signed: !!config.headers['X-Signature'],
        encrypted: !!config.headers['X-Content-Encrypted'],
      };

      this.requestMetrics.push(metrics);

      // Keep only last 100 metrics
      if (this.requestMetrics.length > 100) {
        this.requestMetrics = this.requestMetrics.slice(-100);
      }
    } catch (error) {
      console.warn('Failed to record request metrics:', error);
    }
  }

  /**
   * Generate unique request ID
   */
  private generateRequestId(): string {
    return `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Sanitize headers for logging
   */
  private sanitizeHeaders(headers: any): Record<string, string> {
    const sanitized = { ...headers };
    const sensitiveHeaders = ['authorization', 'x-signature', 'x-api-key'];

    for (const header of sensitiveHeaders) {
      if (sanitized[header]) {
        sanitized[header] = '***REDACTED***';
      }
    }

    return sanitized;
  }

  // Public API methods
  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  public async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  public async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.put<T>(url, data, config);
    return response.data;
  }

  public async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.delete<T>(url, config);
    return response.data;
  }

  public async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.patch<T>(url, data, config);
    return response.data;
  }

  // Utility methods
  public getRequestMetrics(): RequestMetrics[] {
    return [...this.requestMetrics];
  }

  public clearMetrics(): void {
    this.requestMetrics = [];
  }

  public updateConfig(newConfig: Partial<SecureApiConfig>): void {
    this.config = { ...this.config, ...newConfig };
  }

  public getConfig(): SecureApiConfig {
    return { ...this.config };
  }
}

// Export singleton instance
export const secureApiClient = SecureApiClient.getInstance();
