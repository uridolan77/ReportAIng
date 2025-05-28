import { z } from 'zod';

// Base schemas for common types
export const BaseApiResponseSchema = z.object({
  success: z.boolean(),
  message: z.string().optional(),
  error: z.string().optional(),
  timestamp: z.string().optional(),
  requestId: z.string().optional(),
  apiVersion: z.string().optional(),
});

export const ApiMetadataSchema = z.object({
  totalCount: z.number().optional(),
  page: z.number().optional(),
  pageSize: z.number().optional(),
  totalPages: z.number().optional(),
  hasNextPage: z.boolean().optional(),
  hasPreviousPage: z.boolean().optional(),
  processingTimeMs: z.number().optional(),
  cache: z.object({
    hit: z.boolean(),
    key: z.string().optional(),
    expiresAt: z.string().optional(),
  }).optional(),
  custom: z.record(z.unknown()).optional(),
});

export const PaginationSchema = z.object({
  currentPage: z.number().min(1),
  totalPages: z.number().min(0),
  totalItems: z.number().min(0),
  hasNext: z.boolean(),
  hasPrevious: z.boolean(),
});

// Generic API response wrapper
export const ApiResponseSchema = <T extends z.ZodTypeAny>(dataSchema: T) =>
  BaseApiResponseSchema.extend({
    data: dataSchema.optional(),
    metadata: ApiMetadataSchema.optional(),
  });

export const PaginatedResponseSchema = <T extends z.ZodTypeAny>(itemSchema: T) =>
  z.object({
    data: z.array(itemSchema),
    pagination: PaginationSchema,
  });

// Authentication schemas
export const UserProfileSchema = z.object({
  id: z.string(),
  username: z.string(),
  email: z.string().email(),
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  role: z.enum(['Admin', 'User', 'Viewer']),
  permissions: z.array(z.string()),
  lastLogin: z.string().optional(),
  isActive: z.boolean(),
  preferences: z.record(z.unknown()).optional(),
});

export const AuthenticationResultSchema = z.object({
  success: z.boolean(),
  token: z.string().optional(),
  refreshToken: z.string().optional(),
  expiresAt: z.string().optional(),
  user: UserProfileSchema.optional(),
  errorMessage: z.string().optional(),
});

export const LoginRequestSchema = z.object({
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(1, 'Password is required'),
});

// Query execution schemas
export const QueryResultSchema = z.object({
  data: z.array(z.record(z.unknown())),
  columns: z.array(z.object({
    name: z.string(),
    type: z.string(),
    nullable: z.boolean().optional(),
    maxLength: z.number().optional(),
  })),
  metadata: z.object({
    rowCount: z.number(),
    columnCount: z.number(),
    executionTimeMs: z.number(),
    fromCache: z.boolean().optional(),
    cacheKey: z.string().optional(),
  }),
});

export const QueryRequestSchema = z.object({
  naturalLanguageQuery: z.string().min(1, 'Query is required'),
  includeExplanation: z.boolean().optional(),
  maxRows: z.number().min(1).max(10000).optional(),
  timeout: z.number().min(1).max(300).optional(),
  useCache: z.boolean().optional(),
  context: z.record(z.unknown()).optional(),
});

export const QueryResponseSchema = z.object({
  queryId: z.string(),
  sql: z.string(),
  result: QueryResultSchema.optional(),
  explanation: z.string().optional(),
  confidence: z.number().min(0).max(1),
  suggestions: z.array(z.string()).optional(),
  warnings: z.array(z.string()).optional(),
  executionTimeMs: z.number(),
  fromCache: z.boolean().optional(),
});

// Enhanced query schemas
export const SemanticEntitySchema = z.object({
  text: z.string(),
  type: z.string(),
  confidence: z.number().min(0).max(1),
  startPosition: z.number().optional(),
  endPosition: z.number().optional(),
  resolvedValue: z.string().optional(),
  properties: z.record(z.unknown()).optional(),
});

export const QueryClassificationSchema = z.object({
  category: z.string(),
  complexity: z.enum(['Simple', 'Medium', 'Complex', 'Advanced']),
  requiredJoins: z.array(z.string()),
  predictedTables: z.array(z.string()),
  estimatedExecutionTime: z.string(),
  recommendedVisualization: z.string(),
  confidenceScore: z.number().min(0).max(1),
  optimizationSuggestions: z.array(z.string()),
});

export const SemanticAnalysisResponseSchema = z.object({
  intent: z.string(),
  entities: z.array(z.object({
    text: z.string(),
    type: z.string(),
    confidence: z.number().min(0).max(1),
    startPosition: z.number().optional(),
    endPosition: z.number().optional(),
  })),
  keywords: z.array(z.string()),
  confidence: z.number().min(0).max(1),
  processedQuery: z.string(),
  metadata: z.record(z.unknown()),
});

export const AlternativeQueryResponseSchema = z.object({
  sql: z.string(),
  score: z.number().min(0).max(1),
  reasoning: z.string(),
  strengths: z.array(z.string()),
  weaknesses: z.array(z.string()),
});

export const ProcessedQuerySchema = z.object({
  sql: z.string(),
  explanation: z.string(),
  confidence: z.number().min(0).max(1),
  alternativeQueries: z.array(AlternativeQueryResponseSchema).optional(),
  semanticEntities: z.array(SemanticEntitySchema).optional(),
  classification: QueryClassificationSchema.optional(),
  usedSchema: z.object({
    relevantTables: z.array(z.unknown()),
    relationships: z.array(z.unknown()),
    suggestedJoins: z.array(z.string()),
    columnMappings: z.record(z.string()),
    businessTerms: z.array(z.string()),
  }).optional(),
});

export const EnhancedQueryResponseSchema = z.object({
  processedQuery: ProcessedQuerySchema.optional(),
  queryResult: QueryResultSchema.optional(),
  semanticAnalysis: SemanticAnalysisResponseSchema.optional(),
  classification: QueryClassificationSchema.optional(),
  alternatives: z.array(AlternativeQueryResponseSchema).optional(),
  success: z.boolean(),
  errorMessage: z.string().optional(),
  timestamp: z.string(),
});

// Schema metadata schemas
export const ColumnMetadataSchema = z.object({
  name: z.string(),
  dataType: z.string(),
  isNullable: z.boolean(),
  maxLength: z.number().optional(),
  precision: z.number().optional(),
  scale: z.number().optional(),
  isIdentity: z.boolean().optional(),
  isPrimaryKey: z.boolean().optional(),
  isForeignKey: z.boolean().optional(),
  defaultValue: z.string().optional(),
  description: z.string().optional(),
  businessName: z.string().optional(),
  businessDescription: z.string().optional(),
});

export const TableMetadataSchema = z.object({
  name: z.string(),
  schema: z.string(),
  type: z.enum(['Table', 'View', 'Function']),
  columns: z.array(ColumnMetadataSchema),
  rowCount: z.number().optional(),
  description: z.string().optional(),
  businessName: z.string().optional(),
  businessDescription: z.string().optional(),
  tags: z.array(z.string()).optional(),
  lastUpdated: z.string().optional(),
});

export const SchemaMetadataSchema = z.object({
  databaseName: z.string(),
  tables: z.array(TableMetadataSchema),
  views: z.array(TableMetadataSchema).optional(),
  relationships: z.array(z.object({
    fromTable: z.string(),
    fromColumn: z.string(),
    toTable: z.string(),
    toColumn: z.string(),
    relationshipType: z.string(),
  })).optional(),
  lastUpdated: z.string(),
  version: z.string(),
  properties: z.record(z.unknown()).optional(),
});

// Query history schemas
export const QueryHistoryItemSchema = z.object({
  id: z.string(),
  naturalLanguageQuery: z.string(),
  executedSql: z.string(),
  executionTimeMs: z.number(),
  timestamp: z.string(),
  success: z.boolean(),
  rowCount: z.number().optional(),
  errorMessage: z.string().optional(),
  userId: z.string().optional(),
  confidence: z.number().min(0).max(1).optional(),
});

// Dashboard schemas
export const UserActivitySummarySchema = z.object({
  totalQueries: z.number(),
  queriesThisWeek: z.number(),
  queriesThisMonth: z.number(),
  averageQueryTime: z.number(),
  lastActivity: z.string(),
});

export const SystemMetricsSchema = z.object({
  databaseConnections: z.number(),
  cacheHitRate: z.number().min(0).max(1),
  averageQueryTime: z.number(),
  systemUptime: z.string(),
  memoryUsage: z.number().optional(),
  cpuUsage: z.number().optional(),
});

export const QuickStatsSchema = z.object({
  totalQueries: z.number(),
  queriesThisWeek: z.number(),
  averageQueryTime: z.number(),
  favoriteTable: z.string(),
});

export const DashboardOverviewSchema = z.object({
  userActivity: UserActivitySummarySchema,
  recentQueries: z.array(QueryHistoryItemSchema),
  systemMetrics: SystemMetricsSchema,
  quickStats: QuickStatsSchema,
});

// Health check schemas
export const HealthCheckSchema = z.object({
  service: z.string(),
  status: z.enum(['Healthy', 'Degraded', 'Unhealthy']),
  description: z.string().optional(),
  responseTime: z.number().optional(),
  lastChecked: z.string(),
  details: z.record(z.unknown()).optional(),
});

export const HealthStatusSchema = z.object({
  overall: z.enum(['Healthy', 'Degraded', 'Unhealthy']),
  checks: z.array(HealthCheckSchema),
  timestamp: z.string(),
  version: z.string().optional(),
});

// Error schemas
export const ApiErrorSchema = z.object({
  code: z.string(),
  message: z.string(),
  details: z.string().optional(),
  timestamp: z.string(),
  requestId: z.string().optional(),
  stackTrace: z.string().optional(),
});

// Type exports
export type BaseApiResponse = z.infer<typeof BaseApiResponseSchema>;
export type ApiMetadata = z.infer<typeof ApiMetadataSchema>;
export type UserProfile = z.infer<typeof UserProfileSchema>;
export type AuthenticationResult = z.infer<typeof AuthenticationResultSchema>;
export type LoginRequest = z.infer<typeof LoginRequestSchema>;
export type QueryResult = z.infer<typeof QueryResultSchema>;
export type QueryRequest = z.infer<typeof QueryRequestSchema>;
export type QueryResponse = z.infer<typeof QueryResponseSchema>;
export type EnhancedQueryResponse = z.infer<typeof EnhancedQueryResponseSchema>;
export type SchemaMetadata = z.infer<typeof SchemaMetadataSchema>;
export type TableMetadata = z.infer<typeof TableMetadataSchema>;
export type ColumnMetadata = z.infer<typeof ColumnMetadataSchema>;
export type QueryHistoryItem = z.infer<typeof QueryHistoryItemSchema>;
export type DashboardOverview = z.infer<typeof DashboardOverviewSchema>;
export type HealthStatus = z.infer<typeof HealthStatusSchema>;
export type ApiError = z.infer<typeof ApiErrorSchema>;
