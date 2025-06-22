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

// Template Improvement Types
export interface TemplateImprovementSuggestion {
  id: number;
  templateKey: string;
  templateName: string;
  type: ImprovementType;
  currentVersion: string;
  suggestedChanges: string; // JSON object
  reasoningExplanation: string;
  expectedImprovementPercent: number;
  basedOnDataPoints: number;
  confidenceScore: number;
  status: SuggestionStatus;
  reviewedBy?: string;
  createdDate: string;
  reviewedDate?: string;
  reviewComments?: string;
}

export interface OptimizedTemplate {
  originalTemplateKey: string;
  optimizedContent: string;
  strategyUsed: OptimizationStrategy;
  changesApplied: OptimizationChange[];
  expectedPerformanceImprovement: number;
  confidenceScore: number;
  optimizationReasoning: string;
  metricPredictions: Record<string, number>;
  optimizedDate: string;
  optimizedBy: string;
}

export interface OptimizationChange {
  changeType: string;
  description: string;
  impactScore: number;
}

export interface PerformancePrediction {
  templateContent: string;
  intentType: string;
  predictedSuccessRate: number;
  predictedUserRating: number;
  predictedResponseTime: number;
  predictionConfidence: number;
  strengthFactors: string[];
  weaknessFactors: string[];
  improvementSuggestions: string[];
  featureScores: Record<string, number>;
  predictionDate: string;
}

export interface TemplateVariant {
  originalTemplateKey: string;
  variantType: VariantType;
  variantContent: string;
  expectedPerformanceChange: number;
  confidenceScore: number;
  generationReasoning: string;
  generatedDate: string;
  generatedBy: string;
}

export interface ContentQualityAnalysis {
  templateContent: string;
  overallQualityScore: number;
  qualityDimensions: Record<string, number>;
  identifiedIssues: QualityIssue[];
  strengths: QualityStrength[];
  improvementSuggestions: string[];
  readability: ReadabilityMetrics;
  structure: StructureAnalysis;
  completeness: ContentCompleteness;
  analysisDate: string;
}

export interface QualityIssue {
  issueType: string;
  description: string;
  severity: number;
  suggestion: string;
}

export interface QualityStrength {
  strengthType: string;
  description: string;
  impactScore: number;
}

export interface ReadabilityMetrics {
  overallScore: number;
  sentenceComplexity: number;
  vocabularyLevel: number;
  readingLevel: string;
}

export interface StructureAnalysis {
  overallScore: number;
  hasClearSections: boolean;
  hasNumberedSteps: boolean;
  hasBulletPoints: boolean;
  complexityScore: number;
}

export interface ContentCompleteness {
  overallScore: number;
  hasInstructions: boolean;
  hasExamples: boolean;
  hasContext: boolean;
  missingElements: string[];
}

export interface ReviewResult {
  suggestionId: number;
  action: SuggestionReviewAction;
  newStatus: string;
  reviewedBy: string;
  reviewDate: string;
  comments?: string;
  success: boolean;
  message: string;
}

// Comprehensive Analytics Types
export interface ComprehensiveAnalyticsDashboard {
  performanceOverview: PerformanceDashboardData;
  abTestingOverview: ABTestDashboard;
  managementOverview: TemplateManagementDashboard;
  performanceTrends: PerformanceTrendsData;
  usageInsights: UsageInsightsData;
  qualityMetrics: QualityMetricsData;
  activeAlerts: PerformanceAlert[];
  dateRange: DateRange;
  generatedDate: string;
}

export interface ABTestDashboard {
  totalActiveTests: number;
  totalCompletedTests: number;
  averageTestDuration: number;
  successfulTests: number;
  testsAwaitingResults: number;
  recentTests: ABTestDetails[];
}

export interface PerformanceTrendsData {
  dataPoints: PerformanceTrendDataPoint[];
  timeRange: DateRange;
  granularity: string;
  intentType?: string;
  generatedDate: string;
}

export interface PerformanceTrendDataPoint {
  timestamp: string;
  averageSuccessRate: number;
  averageConfidenceScore: number;
  totalUsage: number;
  activeTemplates: number;
  averageResponseTime: number;
  errorCount: number;
}

export interface UsageInsightsData {
  totalUsage: number;
  averageSuccessRate: number;
  usageByIntentType: Record<string, number>;
  topPerformingTemplates: TemplatePerformanceMetrics[];
  underperformingTemplates: TemplatePerformanceMetrics[];
  insights: UsageInsight[];
  timeRange: DateRange;
  generatedDate: string;
}

export interface UsageInsight {
  type: string;
  title: string;
  description: string;
  impact: 'High' | 'Medium' | 'Low';
  recommendation: string;
  data: Record<string, any>;
  generatedDate: string;
}

export interface QualityMetricsData {
  overallQualityScore: number;
  averageConfidenceScore: number;
  qualityDistribution: Record<string, number>;
  totalTemplatesAnalyzed: number;
  templatesAboveThreshold: number;
  templatesBelowThreshold: number;
  detailedMetrics: QualityMetric[];
  intentType?: string;
  generatedDate: string;
}

export interface QualityMetric {
  templateKey: string;
  templateName: string;
  qualityScore: number;
  qualityCategory: string;
  lastAnalyzed: string;
}

export interface RealTimeAnalyticsData {
  activeUsers: number;
  queriesPerMinute: number;
  currentSuccessRate: number;
  averageResponseTime: number;
  errorsInLastHour: number;
  recentActivities: RecentActivity[];
  activeTemplateUsage: Record<string, number>;
  lastUpdated: string;
}

export interface RecentActivity {
  id: string;
  type: string;
  description: string;
  timestamp: string;
  templateKey?: string;
  userId?: string;
}

export interface DateRange {
  startDate: string;
  endDate: string;
}

export interface AnalyticsExportConfig {
  format: 'CSV' | 'Excel' | 'JSON';
  dateRange: DateRange;
  includedMetrics: string[];
  intentTypeFilter?: string;
  includeCharts: boolean;
  includeRawData: boolean;
  exportedBy: string;
  requestedDate: string;
}

// Enums
export type ImprovementType =
  | 'ContentOptimization'
  | 'StructureImprovement'
  | 'ContextEnhancement'
  | 'ExampleAddition'
  | 'InstructionClarification'
  | 'PerformanceOptimization';

export type SuggestionStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Implemented'
  | 'NeedsChanges'
  | 'ScheduledForTesting';

export type OptimizationStrategy =
  | 'PerformanceFocused'
  | 'AccuracyFocused'
  | 'UserSatisfactionFocused'
  | 'ResponseTimeFocused'
  | 'Balanced';

export type VariantType =
  | 'ContentVariation'
  | 'StructureVariation'
  | 'StyleVariation'
  | 'ComplexityVariation';

export type SuggestionReviewAction =
  | 'Approve'
  | 'Reject'
  | 'RequestChanges'
  | 'ScheduleABTest';
