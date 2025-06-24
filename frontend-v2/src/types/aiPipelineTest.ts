// AI Pipeline Testing Types

export enum PipelineStep {
  BusinessContextAnalysis = 'BusinessContextAnalysis',
  TokenBudgetManagement = 'TokenBudgetManagement',
  SchemaRetrieval = 'SchemaRetrieval',
  PromptBuilding = 'PromptBuilding',
  AIGeneration = 'AIGeneration'
}

// Utility function to format pipeline step names for display
export const formatPipelineStepName = (step: PipelineStep): string => {
  return String(step).replace(/([A-Z])/g, ' $1').trim();
};

// Utility function to convert numeric enum values to string enum values
export const normalizePipelineStep = (step: any): PipelineStep => {
  // If it's already a string enum value, return as-is
  if (typeof step === 'string' && Object.values(PipelineStep).includes(step as PipelineStep)) {
    return step as PipelineStep;
  }

  // If it's a numeric value, convert to string enum
  if (typeof step === 'number') {
    const enumValues = Object.values(PipelineStep);
    if (step >= 0 && step < enumValues.length) {
      return enumValues[step];
    }
  }

  // Fallback: try to match by index
  const enumEntries = Object.entries(PipelineStep);
  for (let i = 0; i < enumEntries.length; i++) {
    if (step === i) {
      return enumEntries[i][1];
    }
  }

  // If all else fails, return the first enum value
  console.warn('Unknown pipeline step value:', step, 'defaulting to BusinessContextAnalysis');
  return PipelineStep.BusinessContextAnalysis;
};

// Utility function to normalize an array of pipeline steps
export const normalizePipelineSteps = (steps: any[]): PipelineStep[] => {
  return steps.map(normalizePipelineStep);
};

export interface PipelineTestRequest {
  testId?: string; // Optional test ID for real-time monitoring
  query: string;
  steps: PipelineStep[];
  parameters: PipelineTestParameters;
}

export interface PipelineTestParameters {
  // Business Context Analysis
  confidenceThreshold?: number;
  maxEntities?: number;

  // Token Budget Management
  maxTokens?: number;
  reservedResponseTokens?: number;

  // Schema Retrieval
  maxTables?: number;
  relevanceThreshold?: number;

  // Prompt Building
  includeExamples?: boolean;
  includeBusinessRules?: boolean;

  // AI Generation
  enableAIGeneration?: boolean;
  temperature?: number;

  // Additional parameters
  [key: string]: any;
}

export interface PipelineTestResult {
  testId: string;
  query: string;
  requestedSteps: PipelineStep[];
  startTime: string;
  endTime: string;
  totalDurationMs: number;
  success: boolean;
  error?: string;
  results: Record<string, any>;

  // Intermediate results
  businessProfile?: BusinessContextProfile;
  tokenBudget?: TokenBudget;
  schemaMetadata?: BusinessMetadata;
  generatedPrompt?: string;
  generatedSQL?: string;
}

// Real-time monitoring types
export interface PipelineTestSession {
  sessionId: string;
  testId: string;
  query: string;
  steps: PipelineStep[];
  status: 'running' | 'completed' | 'error';
  startTime: string;
  endTime?: string;
  stepProgress: PipelineStepProgress[];
  results?: any;
}

export interface PipelineStepProgress {
  step: string;
  status: 'pending' | 'running' | 'completed' | 'error';
  startTime?: string;
  endTime?: string;
  durationMs?: number;
  progress?: number; // 0-100
  message?: string;
  details?: any;
}

// Configuration management types
export interface PipelineTestConfiguration {
  id: string;
  name: string;
  description: string;
  steps: PipelineStep[];
  parameters: PipelineTestParameters;
  savedAt: string;
  createdBy: string;
  category: string;
}

export interface SaveConfigurationRequest {
  name: string;
  description: string;
  steps: PipelineStep[];
  parameters: PipelineTestParameters;
  category?: string;
}

export interface ParameterValidationRequest {
  steps: PipelineStep[];
  parameters: PipelineTestParameters;
}

export interface ParameterValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
  suggestions: string[];
}

export interface PipelineStepInfo {
  step: PipelineStep;
  name: string;
  description: string;
  parameters: ParameterInfo[];
}

export interface ParameterInfo {
  name: string;
  type: 'string' | 'int' | 'decimal' | 'bool';
  defaultValue: string;
  description: string;
}

// Base test result interface
export interface BaseTestResult {
  success: boolean;
  durationMs: number;
  error?: string;
}

// Business Context Analysis Result
export interface BusinessContextTestResult extends BaseTestResult {
  businessProfile?: BusinessContextProfile;
  extractedEntities: number;
  confidenceScore: number;
  intent: string;
  domain: string;
}

// Token Budget Management Result
export interface TokenBudgetTestResult extends BaseTestResult {
  tokenBudget?: TokenBudget;
  maxTokens: number;
  availableContextTokens: number;
  reservedTokens: number;
}

// Schema Retrieval Result
export interface SchemaRetrievalTestResult extends BaseTestResult {
  schemaMetadata?: BusinessMetadata;
  tablesRetrieved: number;
  relevanceScore: number;
  tableNames: string[];
}

// Prompt Building Result
export interface PromptBuildingTestResult extends BaseTestResult {
  prompt?: string;
  promptLength: number;
  estimatedTokens: number;
}

// AI Generation Result
export interface AIGenerationTestResult extends BaseTestResult {
  generatedSQL?: string;
  sqlLength: number;
  estimatedCost: number;
}

// Supporting interfaces (these should match your existing types)
export interface BusinessContextProfile {
  intent: {
    type: string;
  };
  domain: {
    name: string;
    relevanceScore: number;
  };
  entities: Array<{
    name: string;
    type: string;
    confidenceScore: number;
    originalText: string;
  }>;
  businessTerms: string[];
  timeContext?: {
    startDate?: string;
    endDate?: string;
    granularity: string;
  };
  confidenceScore: number;
}

export interface TokenBudget {
  intentType: string;
  maxTotalTokens: number;
  basePromptTokens: number;
  reservedResponseTokens: number;
  availableContextTokens: number;
  schemaContextBudget: number;
  businessContextBudget: number;
  examplesBudget: number;
  rulesBudget: number;
  glossaryBudget: number;
  createdAt: string;
}

export interface BusinessMetadata {
  relevantTables: Array<{
    tableName: string;
    schemaName: string;
    businessPurpose: string;
    columns: Array<{
      columnName: string;
      dataType: string;
      businessDescription: string;
    }>;
  }>;
  relevanceScore: number;
  businessRules: Array<{
    name: string;
    description: string;
    priority: string;
  }>;
}

// Real-time monitoring interfaces (using the interface defined above)

export interface PipelineTestSession {
  sessionId: string;
  testId: string;
  query: string;
  steps: PipelineStep[];
  status: 'running' | 'completed' | 'error';
  startTime: string;
  endTime?: string;
  totalDurationMs?: number;
  stepProgress: PipelineStepProgress[];
  results?: PipelineTestResult;
}

// Configuration interfaces (using the interface defined above)

export interface PipelineTestTemplate {
  id: string;
  name: string;
  description: string;
  category: 'debugging' | 'performance' | 'validation' | 'custom';
  steps: PipelineStep[];
  defaultParameters: PipelineTestParameters;
  sampleQueries: string[];
  expectedResults?: Partial<PipelineTestResult>;
}

// Analytics interfaces
export interface PipelineStepMetrics {
  step: PipelineStep;
  averageDurationMs: number;
  successRate: number;
  errorRate: number;
  totalExecutions: number;
  lastExecuted: string;
  commonErrors: Array<{
    error: string;
    count: number;
    lastOccurred: string;
  }>;
}

export interface PipelineTestAnalytics {
  totalTests: number;
  successRate: number;
  averageDurationMs: number;
  stepMetrics: PipelineStepMetrics[];
  popularStepCombinations: Array<{
    steps: PipelineStep[];
    count: number;
    averageDurationMs: number;
    successRate: number;
  }>;
  performanceTrends: Array<{
    date: string;
    averageDurationMs: number;
    successRate: number;
    testCount: number;
  }>;
}
