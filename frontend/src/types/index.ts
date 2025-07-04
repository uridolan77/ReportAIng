/**
 * Unified Types Index
 * Centralized export of all type definitions for better organization and tree-shaking
 */

// ===== CORE TYPES =====
// Query-related types
export type {
  QueryRequest,
  QueryOptions,
  QueryResponse,
  QueryResult,
  QueryMetadata,
  ColumnInfo,
  QueryHistoryItem,
  QueryFeedback,
  QuerySuggestion,
  QueryError,
  QueryPerformanceMetrics,
  PromptDetails,
  PromptSection,
  WebSocketMessage,
  ExportOptions,
  ChartConfig,
  StreamingQueryRequest,
  StreamingQueryChunk,
  StreamingQueryMetadata,
  StreamingProgressUpdate,
  AdvancedStreamingRequest,
  StreamingQueryStartedEvent,
  StreamingQueryChunkEvent,
  StreamingQueryCompletedEvent,
  StreamingQueryErrorEvent,
  VisualizationRequest
} from './query';

// Visualization types
export type {
  AdvancedChartType,
  AdvancedVisualizationConfig,
  AnimationConfig,
  InteractionConfig,
  DrillDownConfig,
  TooltipConfig,
  ThemeConfig,
  ColorPalette,
  GradientConfig,
  FontConfig,
  BorderConfig,
  ShadowConfig,
  DataProcessingConfig,
  AggregationConfig,
  OutlierConfig,
  ExportConfig,
  AccessibilityConfig,
  PerformanceConfig,
  AdvancedDashboardConfig,
  DashboardLayout,
  GlobalFilter,
  DashboardTheme,
  ResponsiveConfig,
  BreakpointConfig,
  RealTimeConfig,
  AlertConfig,
  CollaborationConfig,
  SecurityConfig,
  AnalyticsConfig,
  ChartPerformanceMetrics,
  VisualizationRecommendation,
  PerformanceEstimate,
  VisualizationPreferences,
  DashboardPreferences,
  AdvancedVisualizationRequest,
  AdvancedVisualizationResponse,
  AdvancedDashboardRequest,
  AdvancedDashboardResponse,
  VisualizationRecommendationsRequest,
  VisualizationRecommendationsResponse,
  DataSummary,
  DashboardGenerationMetrics,
  RecommendationAnalysisMetrics
} from './visualization';

// Legacy visualization types from query.ts (for backward compatibility)
export type {
  VisualizationConfig,
  VisualizationMetadata,
  ChartTheme,
  ColorScheme,
  VisualizationOption,
  VisualizationOptionsResponse,
  ChartTypeInfo,
  InteractiveVisualizationConfig,
  FilterConfig,
  DrillDownOption,
  DashboardConfig,
  EnhancedChartTypeInfo
} from './query';

// Schema management types
export type {
  BusinessSchemaDto,
  BusinessSchemaVersionDto,
  SchemaTableContextDto,
  SchemaColumnContextDto,
  SchemaGlossaryTermDto,
  SchemaRelationshipDto,
  UserSchemaPreferenceDto,
  ChangeLogEntry,
  CreateSchemaRequest,
  UpdateSchemaRequest,
  CreateSchemaVersionRequest,
  UpdateTableContextRequest,
  UpdateColumnContextRequest,
  UpdateGlossaryTermRequest,
  UpdateRelationshipRequest,
  ApplyAutoGeneratedContentRequest,
  AutoGeneratedTableContext,
  AutoGeneratedColumnContext,
  AutoGeneratedGlossaryTerm,
  AutoGeneratedRelationship,
  SchemaComparisonResult,
  SchemaDifference,
  ComparisonSummary,
  SchemaExportData,
  SchemaImportRequest,
  SchemaManagementState,
  SchemaEditorState,
  SchemaFilter,
  ContentFilter,
  ValidationResult,
  ValidationError,
  ValidationWarning
} from './schemaManagement';

// DB Explorer types
export type {
  DatabaseTable,
  DatabaseColumn,
  DatabaseIndex,
  ForeignKey,
  DatabaseSchema,
  TableDataPreview,
  DBExplorerState,
  TableSearchResult,
  DBExplorerConfig
} from './dbExplorer';

// ===== BRANDED TYPES =====
// Enhanced type safety with branded types
export type {
  Brand,
  UserId,
  QueryId,
  SessionId,
  RequestId,
  TableId,
  ColumnId,
  SchemaId,
  CacheKey,
  TabId,
  UnixTimestamp,
  ISOTimestamp,
  ExecutionTimeMs,
  SqlQuery,
  NaturalLanguageQuery,
  TableName,
  ColumnName,
  SchemaName,
  ConfidenceScore,
  RowCount,
  ColumnCount,
  PageNumber,
  PageSize,
  Percentage,
  ApiEndpoint,
  FilePath,
  ImageUrl,
  JwtToken,
  ApiKey,
  PasswordHash,
  Salt,
  EmailAddress,
  PhoneNumber,
  IpAddress,
  UserAgent,
  Currency,
  CountryCode,
  LanguageCode,
  TimeZone,
  SemanticVersion,
  ApiVersion,
  SchemaVersion,
  BrandedQueryExecution,
  BrandedUserSession,
  BrandedApiRequest
} from './branded';

// ===== TYPE UTILITIES =====
// Common type utility patterns
export type Optional<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>;
export type RequiredFields<T, K extends keyof T> = T & Required<Pick<T, K>>;
export type Nullable<T> = T | null;
export type Maybe<T> = T | undefined;
export type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P];
};
export type DeepRequired<T> = {
  [P in keyof T]-?: T[P] extends object ? DeepRequired<T[P]> : T[P];
};
export type DeepReadonly<T> = {
  readonly [P in keyof T]: T[P] extends object ? DeepReadonly<T[P]> : T[P];
};
export type KeysOfType<T, U> = {
  [K in keyof T]: T[K] extends U ? K : never;
}[keyof T];
export type ValuesOfType<T, U> = T[KeysOfType<T, U>];
export type Mutable<T> = {
  -readonly [P in keyof T]: T[P];
};
export type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>;
export type RequiredBy<T, K extends keyof T> = T & Required<Pick<T, K>>;
