// Query-related types for the frontend
export interface QueryRequest {
  question: string;
  sessionId: string;
  options: QueryOptions;
  naturalLanguageQuery?: string;
  includeExplanation?: boolean;
  maxRows?: number;
}

export interface QueryOptions {
  includeVisualization?: boolean;
  maxRows?: number;
  enableCache?: boolean;
  confidenceThreshold?: number;
  timeoutSeconds?: number;
  chunkSize?: number;
  enableStreaming?: boolean;
  dataSources?: string[];
  parameters?: Record<string, any>;
  // LLM Management options
  providerId?: string;
  modelId?: string;
}

export interface QueryResponse {
  queryId: string;
  sql: string;
  result?: QueryResult;
  visualization?: VisualizationConfig;
  confidence: number;
  suggestions: string[];
  cached: boolean;
  success: boolean;
  error?: string;
  timestamp: string;
  executionTimeMs: number;
  promptDetails?: PromptDetails;
  sessionId?: string;
  source?: string; // Added missing source property
}

export interface QueryResult {
  data: any[];
  metadata: QueryMetadata;
  totalRows?: number;
  columns?: any[];
  success?: boolean; // Added missing success property
}

export interface QueryMetadata {
  columnCount: number;
  rowCount: number;
  executionTimeMs: number;
  columns: ColumnInfo[];
  dataSource?: string;
  queryTimestamp: string;
}

export interface ColumnInfo {
  name: string;
  dataType: string;
  type?: string; // Added for backward compatibility with mock data
  isNullable: boolean;
  description?: string;
  semanticTags: string[];
  displayName?: string; // Added for mock data compatibility
}

export interface VisualizationConfig {
  type: string; // 'bar', 'line', 'pie', 'table', 'scatter', 'heatmap', etc.
  config: Record<string, any>;
  title?: string;
  xAxis?: string;
  yAxis?: string;
  series?: string[];
  theme?: ChartTheme;
  colorScheme?: ColorScheme;
  enableInteractivity?: boolean;
  enableAnimation?: boolean;
  metadata?: VisualizationMetadata;
}

export interface VisualizationMetadata {
  dataSource: string;
  generatedAt: string;
  dataPointCount: number;
  suggestedAlternatives: string[];
  confidenceScore: number;
  optimizationLevel: string;
}

export enum ChartTheme {
  Modern = 'Modern',
  Classic = 'Classic',
  Dark = 'Dark',
  Light = 'Light',
  Minimal = 'Minimal',
  Corporate = 'Corporate'
}

export enum ColorScheme {
  Business = 'Business',
  Vibrant = 'Vibrant',
  Pastel = 'Pastel',
  Monochrome = 'Monochrome',
  Accessible = 'Accessible',
  Custom = 'Custom'
}

export interface QueryHistoryItem {
  id: string;
  question: string;
  sql: string;
  timestamp: string;
  successful: boolean;
  executionTimeMs: number;
  confidence: number;
  error?: string;
  userId: string;
  sessionId: string;
}

export interface QueryFeedback {
  queryId: string;
  feedback: 'positive' | 'negative' | 'neutral';
  comments?: string;
  suggestedImprovement?: string;
}

export interface QuerySuggestion {
  category: string;
  queries: string[];
  description?: string;
  priority: number;
  requiredTables: string[];
}

// API Response types
export interface ApiResponse<T> {
  data?: T;
  success: boolean;
  error?: string;
  message?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    currentPage: number;
    totalPages: number;
    totalItems: number;
    hasNext: boolean;
    hasPrevious: boolean;
  };
}

export interface PromptDetails {
  fullPrompt: string;
  templateName: string;
  templateVersion: string;
  sections: PromptSection[];
  variables: Record<string, string>;
  tokenCount: number;
  generatedAt: string;
}

export interface PromptSection {
  name: string;
  title: string;
  content: string;
  type: string; // schema, business_rules, examples, context, template, user_input
  order: number;
  metadata?: Record<string, any>;
}

// WebSocket message types
export interface WebSocketMessage {
  type: string;
  queryId?: string;
  progress?: number;
  message?: string;
  timestamp: string;
  success?: boolean;
  executionTime?: number;
}

// Export options
export interface ExportOptions {
  format: 'csv' | 'excel' | 'pdf' | 'json';
  includeMetadata: boolean;
  filename?: string;
}

// Chart configuration types
export interface ChartConfig {
  type: 'bar' | 'line' | 'pie' | 'area' | 'scatter' | 'table';
  title?: string;
  xAxisLabel?: string;
  yAxisLabel?: string;
  showLegend: boolean;
  showGrid: boolean;
  colors?: string[];
  height?: number;
  width?: number;
}

// Error types
export interface QueryError {
  code: string;
  message: string;
  details?: string;
  timestamp: string;
  traceId?: string;
}

// Streaming query types
export interface StreamingQueryRequest {
  question: string;
  sessionId: string;
  options: QueryOptions;
}

export interface StreamingQueryChunk {
  chunkIndex: number;
  data: any[];
  totalRowsInChunk: number;
  isLastChunk: boolean;
  timestamp: string;
  processingTimeMs: number;
}

export interface StreamingQueryMetadata {
  columns: ColumnInfo[];
  estimatedRowCount: number;
  chunkSize: number;
  estimatedChunks: number;
  supportsStreaming: boolean;
  queryComplexity: string;
}

// Enhanced visualization types
export interface VisualizationOption {
  config: VisualizationConfig;
  isSuitable: boolean;
  recommendationScore: number;
  performanceImpact: string;
  description: string;
}

export interface VisualizationOptionsResponse {
  options: VisualizationOption[];
  dataSummary: DataSummary;
  recommendedOption?: VisualizationConfig;
}

export interface DataSummary {
  rowCount: number;
  columnCount: number;
  hasNumericData: boolean;
  hasCategoricalData: boolean;
  hasTimeData: boolean;
}

export interface ChartTypeInfo {
  type: string;
  name: string;
  description: string;
  bestFor: string;
}

// SignalR streaming events
export interface StreamingQueryStartedEvent {
  queryId: string;
  question: string;
  timestamp: string;
}

export interface StreamingQueryChunkEvent {
  queryId: string;
  chunkIndex: number;
  data: any[];
  totalRowsInChunk: number;
  isLastChunk: boolean;
  processingTimeMs: number;
  totalRowsSoFar: number;
  timestamp: string;
}

export interface StreamingQueryCompletedEvent {
  queryId: string;
  totalRows: number;
  totalChunks: number;
  executionTimeMs: number;
  timestamp: string;
}

export interface StreamingQueryErrorEvent {
  queryId: string;
  error: string;
  timestamp: string;
}

// Performance metrics
export interface QueryPerformanceMetrics {
  executionTime: number;
  cacheHit: boolean;
  rowsReturned: number;
  confidence: number;
  complexity: 'simple' | 'medium' | 'complex';
}

// Enhanced streaming types
export interface StreamingProgressUpdate {
  rowsProcessed: number;
  estimatedTotalRows: number;
  progressPercentage: number;
  elapsedTime: string; // TimeSpan as string
  estimatedTimeRemaining: string;
  rowsPerSecond: number;
  status: string;
  currentRow?: Record<string, any>;
  isCompleted: boolean;
  errorMessage?: string;
}

export interface AdvancedStreamingRequest {
  question: string;
  maxRows: number;
  timeoutSeconds: number;
  chunkSize: number;
  enableProgressReporting: boolean;
}

// Enhanced visualization types
export interface InteractiveVisualizationConfig {
  baseVisualization: VisualizationConfig;
  interactiveFeatures: Record<string, any>;
  filters: FilterConfig[];
  drillDownOptions: DrillDownOption[];
  crossFiltering: boolean;
  realTimeUpdates: boolean;
  exportOptions: string[];
}

export interface FilterConfig {
  columnName: string;
  filterType: string; // 'range', 'multiSelect', 'dateRange'
  label: string;
  defaultValue?: any;
  options?: any[];
}

export interface DrillDownOption {
  name: string;
  levels: string[];
  targetColumn: string;
}

export interface DashboardConfig {
  title: string;
  description: string;
  charts: VisualizationConfig[];
  layout: DashboardLayout;
  globalFilters: FilterConfig[];
  refreshInterval?: number; // seconds
}

export interface DashboardLayout {
  rows: number;
  columns: number;
  chartSizes: string[]; // 'full', 'half', 'quarter', 'third'
}

export interface EnhancedChartTypeInfo extends ChartTypeInfo {
  interactiveFeatures: string[];
  supportedDataTypes: string[];
  maxRecommendedRows: number;
}

// Visualization request types
export interface VisualizationRequest {
  query: string;
  columns: ColumnInfo[];
  data: any[];
}
