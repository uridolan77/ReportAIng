import api from './api';

// Types for query suggestions
export interface SuggestionCategory {
  id: number;
  categoryKey: string;
  title: string;
  icon?: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  suggestionCount: number;
}

export interface QuerySuggestion {
  id: number;
  categoryId: number;
  categoryKey: string;
  categoryTitle: string;
  queryText: string;
  description: string;
  defaultTimeFrame?: string;
  defaultTimeFrameDisplay?: string;
  sortOrder: number;
  isActive: boolean;
  targetTables: string[];
  complexity: number;
  requiredPermissions: string[];
  tags: string[];
  usageCount: number;
  lastUsed?: string;
  createdDate: string;
  createdBy: string;
}

export interface GroupedSuggestions {
  category: SuggestionCategory;
  suggestions: QuerySuggestion[];
}

export interface TimeFrameDefinition {
  id: number;
  timeFrameKey: string;
  displayName: string;
  description?: string;
  sqlExpression: string;
  sortOrder: number;
  isActive: boolean;
}

export interface SuggestionSearchParams {
  searchTerm?: string;
  categoryKey?: string;
  tags?: string[];
  minComplexity?: number;
  maxComplexity?: number;
  isActive?: boolean;
  skip?: number;
  take?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface SuggestionSearchResult {
  suggestions: QuerySuggestion[];
  totalCount: number;
  skip: number;
  take: number;
}

export interface RecordUsageParams {
  suggestionId: number;
  sessionId?: string;
  timeFrameUsed?: string;
  resultCount?: number;
  executionTimeMs?: number;
  wasSuccessful?: boolean;
  userFeedback?: number; // 1=Helpful, 0=Not helpful
}

export interface CreateUpdateSuggestion {
  categoryId: number;
  queryText: string;
  description: string;
  defaultTimeFrame?: string;
  sortOrder?: number;
  isActive?: boolean;
  targetTables?: string[];
  complexity?: number;
  requiredPermissions?: string[];
  tags?: string[];
}

export interface CreateUpdateCategory {
  categoryKey: string;
  title: string;
  icon?: string;
  description?: string;
  sortOrder?: number;
  isActive?: boolean;
}

class QuerySuggestionService {
  private readonly baseUrl = '/api/querysuggestions';

  // Public endpoints (all users)
  async getCategories(includeInactive = false): Promise<SuggestionCategory[]> {
    const response = await api.get(`${this.baseUrl}/categories`, {
      params: { includeInactive }
    });
    return response.data;
  }

  async getGroupedSuggestions(includeInactive = false): Promise<GroupedSuggestions[]> {
    const response = await api.get(`${this.baseUrl}/grouped`, {
      params: { includeInactive }
    });
    return response.data;
  }

  async getSuggestionsByCategory(categoryKey: string, includeInactive = false): Promise<QuerySuggestion[]> {
    const response = await api.get(`${this.baseUrl}/category/${categoryKey}`, {
      params: { includeInactive }
    });
    return response.data;
  }

  async searchSuggestions(params: SuggestionSearchParams): Promise<SuggestionSearchResult> {
    const response = await api.post(`${this.baseUrl}/search`, params);
    return response.data;
  }

  async getPopularSuggestions(count = 10): Promise<QuerySuggestion[]> {
    const response = await api.get(`${this.baseUrl}/popular`, {
      params: { count }
    });
    return response.data;
  }

  async getTimeFrames(includeInactive = false): Promise<TimeFrameDefinition[]> {
    const response = await api.get(`${this.baseUrl}/timeframes`, {
      params: { includeInactive }
    });
    return response.data;
  }

  async recordUsage(params: RecordUsageParams): Promise<void> {
    await api.post(`${this.baseUrl}/usage`, params);
  }

  // Admin-only endpoints
  async createCategory(category: CreateUpdateCategory): Promise<SuggestionCategory> {
    const response = await api.post(`${this.baseUrl}/categories`, category);
    return response.data;
  }

  async updateCategory(id: number, category: CreateUpdateCategory): Promise<SuggestionCategory> {
    const response = await api.put(`${this.baseUrl}/categories/${id}`, category);
    return response.data;
  }

  async createSuggestion(suggestion: CreateUpdateSuggestion): Promise<QuerySuggestion> {
    const response = await api.post(`${this.baseUrl}`, suggestion);
    return response.data;
  }

  async updateSuggestion(id: number, suggestion: CreateUpdateSuggestion): Promise<QuerySuggestion> {
    const response = await api.put(`${this.baseUrl}/${id}`, suggestion);
    return response.data;
  }

  async deleteSuggestion(id: number): Promise<void> {
    await api.delete(`${this.baseUrl}/${id}`);
  }

  async getUsageAnalytics(fromDate?: Date, toDate?: Date): Promise<any[]> {
    const params: any = {};
    if (fromDate) params.fromDate = fromDate.toISOString();
    if (toDate) params.toDate = toDate.toISOString();

    const response = await api.get(`${this.baseUrl}/analytics`, { params });
    return response.data;
  }

  // Utility methods
  getComplexityLabel(complexity: number): string {
    switch (complexity) {
      case 1: return 'Simple';
      case 2: return 'Medium';
      case 3: return 'Complex';
      default: return 'Unknown';
    }
  }

  getComplexityColor(complexity: number): string {
    switch (complexity) {
      case 1: return '#52c41a'; // green
      case 2: return '#faad14'; // orange
      case 3: return '#f5222d'; // red
      default: return '#d9d9d9'; // gray
    }
  }

  formatTimeFrame(timeFrameKey?: string): string {
    if (!timeFrameKey) return 'All Time';
    
    const timeFrameMap: Record<string, string> = {
      'today': 'Today',
      'yesterday': 'Yesterday',
      'last_7_days': 'Last 7 Days',
      'last_30_days': 'Last 30 Days',
      'this_week': 'This Week',
      'last_week': 'Last Week',
      'this_month': 'This Month',
      'last_month': 'Last Month',
      'this_quarter': 'This Quarter',
      'last_quarter': 'Last Quarter',
      'this_year': 'This Year',
      'last_year': 'Last Year',
      'all_time': 'All Time'
    };

    return timeFrameMap[timeFrameKey] || timeFrameKey;
  }
}

export const querySuggestionService = new QuerySuggestionService();
