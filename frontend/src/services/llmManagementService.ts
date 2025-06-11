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
    try {
      const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDERS}`, {
        headers: this.getHeaders(),
      });
      if (!response.ok) throw new Error('Failed to fetch providers');
      return response.json();
    } catch (error) {
      console.warn('LLM Management backend not available, using mock providers:', error);
      return this.getMockProviders();
    }
  }

  private getMockProviders(): LLMProviderConfig[] {
    return [
      {
        providerId: 'openai-1',
        name: 'OpenAI',
        type: 'OpenAI',
        apiKey: '***CONFIGURED***',
        endpoint: 'https://api.openai.com/v1',
        organization: '',
        isEnabled: true,
        isDefault: true,
        settings: {},
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
      {
        providerId: 'azure-openai-1',
        name: 'Azure OpenAI',
        type: 'AzureOpenAI',
        apiKey: '***CONFIGURED***',
        endpoint: 'https://your-resource.openai.azure.com',
        organization: '',
        isEnabled: false,
        isDefault: false,
        settings: {},
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
    ];
  }

  async getProvider(providerId: string): Promise<LLMProviderConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_BY_ID}/${providerId}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch provider');
    return response.json();
  }

  async saveProvider(provider: LLMProviderConfig): Promise<LLMProviderConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDERS}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(provider),
    });
    if (!response.ok) throw new Error('Failed to save provider');
    return response.json();
  }

  async deleteProvider(providerId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_BY_ID}/${providerId}`, {
      method: 'DELETE',
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete provider');
  }

  async testProvider(providerId: string): Promise<LLMProviderStatus> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_TEST}/${providerId}/test`, {
      method: 'POST',
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to test provider');
    return response.json();
  }

  async getProviderHealth(): Promise<LLMProviderStatus[]> {
    try {
      const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PROVIDER_HEALTH}`, {
        headers: this.getHeaders(),
      });
      if (!response.ok) throw new Error('Failed to fetch provider health');
      return response.json();
    } catch (error) {
      console.warn('LLM Management backend not available, using mock provider health:', error);
      return this.getMockProviderHealth();
    }
  }

  private getMockProviderHealth(): LLMProviderStatus[] {
    return [
      {
        providerId: 'openai-1',
        name: 'OpenAI',
        isConnected: true,
        isHealthy: true,
        lastChecked: new Date().toISOString(),
        lastResponseTime: 650,
        healthDetails: { status: 'operational' },
      },
      {
        providerId: 'azure-openai-1',
        name: 'Azure OpenAI',
        isConnected: false,
        isHealthy: false,
        lastError: 'Provider not configured',
        lastChecked: new Date().toISOString(),
        lastResponseTime: 0,
        healthDetails: { status: 'not_configured' },
      },
    ];
  }

  // Model Management
  async getModels(providerId?: string): Promise<LLMModelConfig[]> {
    try {
      const url = providerId
        ? `${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODELS}?providerId=${providerId}`
        : `${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODELS}`;

      const response = await fetch(url, {
        headers: this.getHeaders(),
      });
      if (!response.ok) throw new Error('Failed to fetch models');
      return response.json();
    } catch (error) {
      console.warn('LLM Management backend not available, using mock models:', error);
      return this.getMockModels(providerId);
    }
  }

  private getMockModels(providerId?: string): LLMModelConfig[] {
    const allModels = [
      {
        modelId: 'gpt-4-turbo',
        providerId: 'openai-1',
        name: 'gpt-4-turbo',
        displayName: 'GPT-4 Turbo',
        temperature: 0.1,
        maxTokens: 2000,
        topP: 1.0,
        frequencyPenalty: 0.0,
        presencePenalty: 0.0,
        isEnabled: true,
        useCase: 'SQL',
        costPerToken: 0.00003,
        capabilities: { reasoning: 'high', speed: 'medium' },
      },
      {
        modelId: 'gpt-3.5-turbo',
        providerId: 'openai-1',
        name: 'gpt-3.5-turbo',
        displayName: 'GPT-3.5 Turbo',
        temperature: 0.1,
        maxTokens: 1500,
        topP: 1.0,
        frequencyPenalty: 0.0,
        presencePenalty: 0.0,
        isEnabled: true,
        useCase: 'Insights',
        costPerToken: 0.000002,
        capabilities: { reasoning: 'medium', speed: 'high' },
      },
    ];

    return providerId ? allModels.filter(m => m.providerId === providerId) : allModels;
  }

  async getModel(modelId: string): Promise<LLMModelConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODEL_BY_ID}/${modelId}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch model');
    return response.json();
  }

  async saveModel(model: LLMModelConfig): Promise<LLMModelConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODELS}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(model),
    });
    if (!response.ok) throw new Error('Failed to save model');
    return response.json();
  }

  async deleteModel(modelId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.MODEL_BY_ID}/${modelId}`, {
      method: 'DELETE',
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete model');
  }

  async getDefaultModel(useCase: string): Promise<LLMModelConfig> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.DEFAULT_MODEL}/${useCase}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch default model');
    return response.json();
  }

  async setDefaultModel(modelId: string, useCase: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.SET_DEFAULT_MODEL}/${modelId}/set-default/${useCase}`, {
      method: 'POST',
      headers: this.getHeaders(),
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
      headers: this.getHeaders(),
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
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch usage analytics');
    return response.json();
  }

  async exportUsageData(startDate: string, endDate: string, format: string = 'csv'): Promise<Blob> {
    const queryParams = new URLSearchParams({ startDate, endDate, format });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.USAGE_EXPORT}?${queryParams}`, {
      headers: this.getHeaders(),
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
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cost summary');
    return response.json();
  }

  async getCurrentMonthCost(providerId?: string): Promise<number> {
    const queryParams = providerId ? new URLSearchParams({ providerId }) : '';

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.CURRENT_MONTH_COST}?${queryParams}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch current month cost');
    return response.json();
  }

  async getCostProjections(): Promise<Record<string, number>> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_PROJECTIONS}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cost projections');
    return response.json();
  }

  async setCostLimit(providerId: string, monthlyLimit: number): Promise<void> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_LIMITS}/${providerId}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(monthlyLimit),
    });
    if (!response.ok) throw new Error('Failed to set cost limit');
  }

  async getCostAlerts(): Promise<CostAlert[]> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.COST_ALERTS}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cost alerts');
    return response.json();
  }

  // Performance Monitoring
  async getPerformanceMetrics(startDate: string, endDate: string): Promise<Record<string, ProviderPerformanceMetrics>> {
    const queryParams = new URLSearchParams({ startDate, endDate });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.PERFORMANCE_METRICS}?${queryParams}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch performance metrics');
    return response.json();
  }

  async getCacheHitRates(): Promise<Record<string, number>> {
    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.CACHE_HIT_RATES}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch cache hit rates');
    return response.json();
  }

  async getErrorAnalysis(startDate: string, endDate: string): Promise<Record<string, ErrorAnalysis>> {
    const queryParams = new URLSearchParams({ startDate, endDate });

    const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.ERROR_ANALYSIS}?${queryParams}`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch error analysis');
    return response.json();
  }

  // Dashboard
  async getDashboardSummary(): Promise<DashboardSummary> {
    try {
      const response = await fetch(`${this.baseUrl}${API_CONFIG.ENDPOINTS.LLM_MANAGEMENT.DASHBOARD_SUMMARY}`, {
        headers: this.getHeaders(),
      });
      if (!response.ok) throw new Error('Failed to fetch dashboard summary');
      return response.json();
    } catch (error) {
      // Return mock data if backend is not available
      console.warn('LLM Management backend not available, using mock data:', error);
      return this.getMockDashboardSummary();
    }
  }

  private getMockDashboardSummary(): DashboardSummary {
    return {
      providers: {
        total: 2,
        enabled: 1,
        healthy: 1,
      },
      usage: {
        totalRequests: 1250,
        totalTokens: 45000,
        averageResponseTime: 850,
        successRate: 0.96,
      },
      costs: {
        currentMonth: 12.45,
        total30Days: 38.90,
        activeAlerts: 0,
      },
      performance: {
        averageResponseTime: 850,
        overallSuccessRate: 0.96,
        totalErrors: 12,
      },
      lastUpdated: new Date().toISOString(),
    };
  }

  private getHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }
}

export const llmManagementService = new LLMManagementService();
