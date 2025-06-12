import { API_CONFIG } from '../config/api';

// Types for LLM Management
export interface LLMProviderConfig {
  providerId: string;
  name: string;
  type: string;
  apiKey: string;
  endpoint: string;
  organization: string;
  isEnabled: boolean;
  isDefault: boolean;
  settings: Record<string, any>;
  createdAt: string;
  updatedAt: string;
}

export interface LLMModelConfig {
  modelId: string;
  providerId: string;
  name: string;
  displayName: string;
  temperature: number;
  maxTokens: number;
  topP: number;
  frequencyPenalty: number;
  presencePenalty: number;
  isEnabled: boolean;
  useCase: string;
  costPerToken: number;
  capabilities: Record<string, any>;
}

export interface LLMUsageLog {
  id: number;
  requestId: string;
  userId: string;
  providerId: string;
  modelId: string;
  requestType: string;
  requestText: string;
  responseText: string;
  inputTokens: number;
  outputTokens: number;
  totalTokens: number;
  cost: number;
  durationMs: number;
  success: boolean;
  errorMessage?: string;
  timestamp: string;
  metadata: Record<string, any>;
}

export interface LLMUsageAnalytics {
  startDate: string;
  endDate: string;
  totalRequests: number;
  totalTokens: number;
  totalCost: number;
  averageResponseTime: number;
  successRate: number;
  costByProvider: LLMCostSummary[];
  costByModel: LLMCostSummary[];
  requestsByType: Record<string, number>;
  costByType: Record<string, number>;
  topRequests: LLMUsageLog[];
}

export interface LLMCostSummary {
  providerId: string;
  modelId: string;
  date: string;
  totalRequests: number;
  totalTokens: number;
  totalCost: number;
  averageCost: number;
  averageResponseTime: number;
  successRate: number;
}

export interface LLMProviderStatus {
  providerId: string;
  name: string;
  isConnected: boolean;
  isHealthy: boolean;
  lastError?: string;
  lastChecked: string;
  lastResponseTime: number;
  healthDetails: Record<string, any>;
}

export interface CostAlert {
  id: string;
  providerId: string;
  thresholdAmount: number;
  alertType: string;
  isEnabled: boolean;
  createdAt: string;
}

export interface ProviderPerformanceMetrics {
  providerId: string;
  averageResponseTime: number;
  medianResponseTime: number;
  p95ResponseTime: number;
  successRate: number;
  errorRate: number;
  totalRequests: number;
  errorsByType: Record<string, number>;
}

export interface ErrorAnalysis {
  providerId: string;
  totalErrors: number;
  errorRate: number;
  errorsByType: Record<string, number>;
  errorsByModel: Record<string, number>;
  commonErrorMessages: string[];
}

export interface DashboardSummary {
  providers: {
    total: number;
    enabled: number;
    healthy: number;
  };
  usage: {
    totalRequests: number;
    totalTokens: number;
    averageResponseTime: number;
    successRate: number;
  };
  costs: {
    currentMonth: number;
    total30Days: number;
    activeAlerts: number;
  };
  performance: {
    averageResponseTime: number;
    overallSuccessRate: number;
    totalErrors: number;
  };
  lastUpdated: string;
}

class LLMManagementService {
  private baseUrl = API_CONFIG.BASE_URL;

  // Provider Management
  async getProviders(): Promise<LLMProviderConfig[]> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDERS}`, {
      headers: await this.getHeaders(),
    });

    if (response.status === 401) {
      throw new Error('Access denied. LLM Management requires Admin or Analyst role. Please log in with appropriate credentials.');
    }

    if (!response.ok) throw new Error('Failed to fetch providers');
    return response.json();
  }

  async getProvider(providerId: string): Promise<LLMProviderConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_BY_ID}/${providerId}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch provider');
    return response.json();
  }

  async saveProvider(provider: LLMProviderConfig): Promise<LLMProviderConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDERS}`, {
      method: 'POST',
      headers: await this.getHeaders(),
      body: JSON.stringify(provider),
    });
    if (!response.ok) throw new Error('Failed to save provider');
    return response.json();
  }

  async deleteProvider(providerId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_BY_ID}/${providerId}`, {
      method: 'DELETE',
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete provider');
  }

  async testProvider(providerId: string): Promise<LLMProviderStatus> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_TEST}/${providerId}/test`, {
      method: 'POST',
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to test provider');
    return response.json();
  }

  async getProviderHealth(): Promise<LLMProviderStatus[]> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_HEALTH}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch provider health');
    return response.json();
  }

  // Model Management
  async getModels(providerId?: string): Promise<LLMModelConfig[]> {
    const url = providerId
      ? `${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODELS}?providerId=${providerId}`
      : `${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODELS}`;

    const response = await fetch(url, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch models');
    return response.json();
  }

  async getModel(modelId: string): Promise<LLMModelConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODEL_BY_ID}/${modelId}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch model');
    return response.json();
  }

  async saveModel(model: LLMModelConfig): Promise<LLMModelConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODELS}`, {
      method: 'POST',
      headers: await this.getHeaders(),
      body: JSON.stringify(model),
    });
    if (!response.ok) throw new Error('Failed to save model');
    return response.json();
  }

  async deleteModel(modelId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODEL_BY_ID}/${modelId}`, {
      method: 'DELETE',
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete model');
  }

  async getDefaultModel(useCase: string): Promise<LLMModelConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.DEFAULT_MODEL}/${useCase}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch default model');
    return response.json();
  }

  async setDefaultModel(modelId: string, useCase: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.SET_DEFAULT_MODEL}/${modelId}/set-default/${useCase}`, {
      method: 'POST',
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to set default model');
  }

  // Usage Tracking
  async getUsageHistory(params: {
    startDate?: string;
    endDate?: string;
    providerId?: string;
    modelId?: string;
    userId?: string;
    requestType?: string;
    skip?: number;
    take?: number;
  } = {}): Promise<LLMUsageLog[]> {
    const queryParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined) queryParams.append(key, value.toString());
    });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.USAGE_HISTORY}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch usage history');
    return response.json();
  }

  async getUsageAnalytics(
    startDate: string,
    endDate: string,
    providerId?: string,
    modelId?: string
  ): Promise<LLMUsageAnalytics> {
    const queryParams = new URLSearchParams({
      startDate,
      endDate,
      ...(providerId && { providerId }),
      ...(modelId && { modelId }),
    });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.USAGE_ANALYTICS}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch usage analytics');
    return response.json();
  }

  async exportUsageData(startDate: string, endDate: string, format: string = 'csv'): Promise<Blob> {
    const queryParams = new URLSearchParams({ startDate, endDate, format });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.USAGE_EXPORT}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to export usage data');
    return response.blob();
  }

  // Cost Management
  async getCostSummary(startDate: string, endDate: string, providerId?: string): Promise<LLMCostSummary[]> {
    const queryParams = new URLSearchParams({
      startDate,
      endDate,
      ...(providerId && { providerId }),
    });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_SUMMARY}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cost summary');
    return response.json();
  }

  async getCurrentMonthCost(providerId?: string): Promise<number> {
    const queryParams = providerId ? new URLSearchParams({ providerId }) : '';

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.CURRENT_MONTH_COST}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch current month cost');
    return response.json();
  }

  async getCostProjections(): Promise<Record<string, number>> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_PROJECTIONS}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cost projections');
    return response.json();
  }

  async setCostLimit(providerId: string, monthlyLimit: number): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_LIMITS}/${providerId}`, {
      method: 'POST',
      headers: await this.getHeaders(),
      body: JSON.stringify(monthlyLimit),
    });
    if (!response.ok) throw new Error('Failed to set cost limit');
  }

  async getCostAlerts(): Promise<CostAlert[]> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_ALERTS}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cost alerts');
    return response.json();
  }

  // Performance Monitoring
  async getPerformanceMetrics(startDate: string, endDate: string): Promise<Record<string, ProviderPerformanceMetrics>> {
    const queryParams = new URLSearchParams({ startDate, endDate });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PERFORMANCE_METRICS}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch performance metrics');
    return response.json();
  }

  async getCacheHitRates(): Promise<Record<string, number>> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.CACHE_HIT_RATES}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cache hit rates');
    return response.json();
  }

  async getErrorAnalysis(startDate: string, endDate: string): Promise<Record<string, ErrorAnalysis>> {
    const queryParams = new URLSearchParams({ startDate, endDate });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.ERROR_ANALYSIS}?${queryParams}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch error analysis');
    return response.json();
  }

  // Dashboard
  async getDashboardSummary(): Promise<DashboardSummary> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.DASHBOARD_SUMMARY}`, {
      headers: await this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch dashboard summary');
    return response.json();
  }

  private async getHeaders(): Promise<HeadersInit> {
    const token = await this.getAuthToken();
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  private async getAuthToken(): Promise<string | null> {
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
        return legacyToken;
      }

      return null;
    } catch (error) {
      console.warn('❌ Error getting auth token:', error);
      return localStorage.getItem('authToken');
    }
  }
}

export const llmManagementService = new LLMManagementService();
