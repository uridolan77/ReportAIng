// Template Analytics Types
export interface TemplatePerformanceMetrics {
  templateKey: string;
  templateName: string;
  intentType: string;
  totalUsages: number;
  successRate: number;
  averageConfidenceScore: number;
  averageResponseTime: number;
  errorRate: number;
  lastUsed: string;
  createdDate: string;
  lastModified: string;
  version: string;
  isActive: boolean;
}

export interface PerformanceDashboardData {
  totalTemplates: number;
  overallSuccessRate: number;
  totalUsagesToday: number;
  needsAttention: TemplatePerformanceMetrics[];
  topPerformers: TemplatePerformanceMetrics[];
  usageByIntentType: Record<string, number>;
  recentTrends: PerformanceTrend[];
}

export interface PerformanceTrend {
  timestamp: string;
  successRate: number;
  usageCount: number;
  averageConfidenceScore: number;
  errorCount: number;
}

export interface PerformanceAlert {
  id: number;
  templateKey: string;
  alertType: 'low_success_rate' | 'high_error_rate' | 'low_usage' | 'performance_degradation';
  message: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  timestamp: string;
  isResolved: boolean;
  threshold: number;
  currentValue: number;
}

// A/B Testing Types
export interface ABTestDetails {
  id: number;
  testName: string;
  originalTemplateKey: string;
  variantTemplateKey: string;
  status: 'running' | 'paused' | 'completed' | 'cancelled';
  trafficSplitPercent: number;
  startDate: string;
  endDate: string;
  createdBy: string;
  originalUsageCount: number;
  variantUsageCount: number;
  originalSuccessRate: number;
  variantSuccessRate: number;
  statisticalSignificance?: number;
  confidenceLevel: number;
  minimumSampleSize: number;
}

export interface ABTestRequest {
  testName: string;
  originalTemplateKey: string;
  variantTemplateContent: string;
  trafficSplitPercent: number;
  startDate: string;
  endDate: string;
  createdBy: string;
  minimumSampleSize?: number;
  confidenceLevel?: number;
}

export interface ABTestResult {
  testId: number;
  message: string;
  variantTemplateKey: string;
}

export interface ABTestAnalysis {
  testId: number;
  originalMetrics: {
    usageCount: number;
    successRate: number;
    averageConfidenceScore: number;
    averageResponseTime: number;
  };
  variantMetrics: {
    usageCount: number;
    successRate: number;
    averageConfidenceScore: number;
    averageResponseTime: number;
  };
  statisticalSignificance: number;
  confidenceInterval: {
    lower: number;
    upper: number;
  };
  recommendation: 'continue_test' | 'implement_variant' | 'keep_original' | 'inconclusive';
  pValue: number;
  effectSize: number;
}

export interface ABTestRecommendation {
  templateKey: string;
  recommendationType: 'performance_improvement' | 'usage_optimization' | 'error_reduction';
  description: string;
  expectedImprovement: number;
  confidence: number;
  suggestedVariant: string;
}

// Template Management Types
export interface TemplateWithMetrics {
  templateKey: string;
  templateName: string;
  content: string;
  intentType: string;
  version: string;
  isActive: boolean;
  createdDate: string;
  lastModified: string;
  createdBy: string;
  lastModifiedBy: string;
  businessMetadata?: TemplateBusinessMetadata;
  performanceMetrics: TemplatePerformanceMetrics;
  qualityScore: number;
  tags: string[];
  description?: string;
}

export interface TemplateBusinessMetadata {
  businessPurpose: string;
  businessFriendlyName: string;
  businessContext: string;
  useCases: string[];
  targetAudience: string[];
  businessRules: string[];
  dataGovernanceLevel: 'public' | 'internal' | 'confidential' | 'restricted';
  lastBusinessReview: string;
  businessOwner: string;
  technicalOwner: string;
  approvalStatus: 'draft' | 'pending_review' | 'approved' | 'deprecated';
}

export interface TemplateManagementDashboard {
  totalTemplates: number;
  activeTemplates: number;
  templatesNeedingReview: number;
  averageQualityScore: number;
  templatesByIntentType: Record<string, number>;
  recentlyModified: TemplateWithMetrics[];
  topQualityTemplates: TemplateWithMetrics[];
  templatesNeedingAttention: TemplateWithMetrics[];
}

export interface CreateTemplateRequest {
  templateKey: string;
  templateName: string;
  content: string;
  intentType: string;
  description?: string;
  tags?: string[];
  businessMetadata?: Partial<TemplateBusinessMetadata>;
}

export interface UpdateTemplateRequest {
  templateName?: string;
  content?: string;
  description?: string;
  tags?: string[];
  businessMetadata?: Partial<TemplateBusinessMetadata>;
  isActive?: boolean;
}

export interface TemplateSearchCriteria {
  query?: string;
  intentType?: string;
  tags?: string[];
  qualityScoreMin?: number;
  qualityScoreMax?: number;
  lastModifiedAfter?: string;
  lastModifiedBefore?: string;
  isActive?: boolean;
  businessOwner?: string;
  approvalStatus?: string;
}

export interface TemplateSearchResult {
  templates: TemplateWithMetrics[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

// UI State Types
export interface DashboardFilters {
  intentTypes?: string[];
  templateKeys?: string[];
  performanceThreshold?: number;
  timeRange?: TimeRange;
}

export interface TimeRange {
  start: Date;
  end: Date;
}

// User Feedback Types
export interface UserFeedback {
  usageId: string;
  rating: number;
  userId?: string;
  comments?: string;
  timestamp: string;
}

// Export Types
export interface ExportRequest {
  format: 'csv' | 'excel' | 'pdf' | 'json';
  data: 'performance' | 'abtests' | 'templates' | 'analytics';
  filters?: DashboardFilters;
  dateRange?: TimeRange;
}
