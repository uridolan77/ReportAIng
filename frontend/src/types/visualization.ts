// Advanced visualization types for frontend

export type AdvancedChartType = 
  | 'Bar' | 'Line' | 'Pie' | 'Scatter' | 'Area' | 'Heatmap' | 'Treemap' 
  | 'Sunburst' | 'Gauge' | 'Radar' | 'Waterfall' | 'Funnel' | 'Sankey'
  | 'Candlestick' | 'BoxPlot' | 'Violin' | 'Histogram' | 'Bubble'
  | 'Timeline' | 'Gantt' | 'Network' | 'Choropleth' | 'Parallel' | 'Polar';

export interface AdvancedVisualizationConfig {
  type: string;
  title?: string;
  xAxis?: string;
  yAxis?: string;
  series?: string[];
  config: Record<string, any>;
  chartType: AdvancedChartType;
  animation?: AnimationConfig;
  interaction?: InteractionConfig;
  theme?: ThemeConfig;
  dataProcessing?: DataProcessingConfig;
  export?: ExportConfig;
  accessibility?: AccessibilityConfig;
  performance?: PerformanceConfig;
  customSettings?: Record<string, any>;
}

export interface AnimationConfig {
  enabled: boolean;
  duration: number;
  easing: string;
  delayByCategory: boolean;
  delayIncrement: number;
  animateOnDataChange: boolean;
  animatedProperties: string[];
}

export interface InteractionConfig {
  enableZoom: boolean;
  enablePan: boolean;
  enableBrush: boolean;
  enableCrosshair: boolean;
  enableTooltip: boolean;
  enableLegendToggle: boolean;
  enableDataPointSelection: boolean;
  enableDrillDown: boolean;
  drillDown?: DrillDownConfig;
  tooltip?: TooltipConfig;
}

export interface DrillDownConfig {
  levels: string[];
  levelQueries: Record<string, string>;
  enableBreadcrumb: boolean;
  maxDepth: number;
}

export interface TooltipConfig {
  enabled: boolean;
  position: string;
  displayFields: string[];
  customTemplate?: string;
  showStatistics: boolean;
  enableHtml: boolean;
}

export interface ThemeConfig {
  name: string;
  colors?: ColorPalette;
  fonts?: FontConfig;
  borders?: BorderConfig;
  shadows?: ShadowConfig;
  darkMode: boolean;
}

export interface ColorPalette {
  primary: string[];
  secondary: string[];
  background: string;
  text: string;
  grid: string;
  axis: string;
  gradients?: GradientConfig;
}

export interface GradientConfig {
  enabled: boolean;
  direction: string;
  colors: string[];
  stops: number[];
}

export interface FontConfig {
  family: string;
  titleSize: number;
  labelSize: number;
  legendSize: number;
  tooltipSize: number;
  weight: string;
}

export interface BorderConfig {
  color: string;
  width: number;
  style: string;
  radius: number;
}

export interface ShadowConfig {
  enabled: boolean;
  color: string;
  offsetX: number;
  offsetY: number;
  blur: number;
}

export interface DataProcessingConfig {
  enableSampling: boolean;
  sampleSize: number;
  samplingMethod: string;
  enableAggregation: boolean;
  aggregation?: AggregationConfig;
  enableOutlierDetection: boolean;
  outliers?: OutlierConfig;
}

export interface AggregationConfig {
  method: string;
  groupBy: string;
  binCount: number;
  timeInterval: string;
}

export interface OutlierConfig {
  method: string;
  threshold: number;
  removeOutliers: boolean;
  highlightOutliers: boolean;
  outlierColor: string;
}

export interface ExportConfig {
  supportedFormats: string[];
  imageWidth: number;
  imageHeight: number;
  imageDPI: number;
  includeData: boolean;
  includeMetadata: boolean;
  defaultFilename: string;
}

export interface AccessibilityConfig {
  enabled: boolean;
  highContrast: boolean;
  screenReaderSupport: boolean;
  keyboardNavigation: boolean;
  ariaLabels: string[];
  description?: string;
  colorBlindFriendly: boolean;
}

export interface PerformanceConfig {
  enableVirtualization: boolean;
  virtualizationThreshold: number;
  enableLazyLoading: boolean;
  enableCaching: boolean;
  cacheTTL: number;
  enableWebGL: boolean;
  maxDataPoints: number;
}

export interface AdvancedDashboardConfig {
  title: string;
  description: string;
  charts: AdvancedVisualizationConfig[];
  layout: DashboardLayout;
  globalFilters: GlobalFilter[];
  refreshInterval?: number;
  theme?: DashboardTheme;
  responsive?: ResponsiveConfig;
  realTime?: RealTimeConfig;
  collaboration?: CollaborationConfig;
  security?: SecurityConfig;
  analytics?: AnalyticsConfig;
}

export interface DashboardLayout {
  type: string;
  columns: number;
  rows: number;
  spacing: number;
  padding: number;
  chartSizes: string[];
}

export interface GlobalFilter {
  name: string;
  type: string;
  column: string;
  defaultValue?: any;
  options?: any[];
}

export interface DashboardTheme {
  name: string;
  backgroundColor: string;
  cardColor: string;
  textColor: string;
  accentColor: string;
  borderRadius: number;
  fontFamily: string;
}

export interface ResponsiveConfig {
  enabled: boolean;
  breakpoints: Record<string, BreakpointConfig>;
  autoResize: boolean;
  maintainAspectRatio: boolean;
}

export interface BreakpointConfig {
  minWidth?: number;
  maxWidth?: number;
  columns: number;
  chartSizes: string[];
  hideCharts?: boolean;
  hiddenChartIds?: string[];
}

export interface RealTimeConfig {
  enabled: boolean;
  refreshInterval: number;
  autoRefresh: boolean;
  showLastUpdated: boolean;
  enableNotifications: boolean;
  alerts: AlertConfig[];
}

export interface AlertConfig {
  id: string;
  name: string;
  condition: string;
  threshold: string;
  action: string;
  enabled: boolean;
}

export interface CollaborationConfig {
  enabled: boolean;
  allowComments: boolean;
  allowSharing: boolean;
  allowEditing: boolean;
  sharedWith: string[];
  permission: string;
}

export interface SecurityConfig {
  requireAuthentication: boolean;
  allowedRoles: string[];
  enableRowLevelSecurity: boolean;
  dataFilters: Record<string, string>;
  enableAuditLog: boolean;
}

export interface AnalyticsConfig {
  enabled: boolean;
  trackViews: boolean;
  trackInteractions: boolean;
  trackPerformance: boolean;
  customEvents: string[];
}

export interface ChartPerformanceMetrics {
  renderTime: string;
  memoryUsage: number;
  dataPointsRendered: number;
  usedSampling: boolean;
  usedVirtualization: boolean;
  usedWebGL: boolean;
  frameRate?: number;
}

export interface VisualizationRecommendation {
  chartType: AdvancedChartType;
  confidence: number;
  reasoning: string;
  bestFor: string;
  limitations: string[];
  estimatedPerformance: PerformanceEstimate;
  suggestedConfig: Record<string, any>;
}

export interface PerformanceEstimate {
  estimatedRenderTime: string;
  memoryUsageMB: number;
  requiresWebGL: boolean;
  requiresSampling: boolean;
  recommendedMaxDataPoints: number;
}

export interface VisualizationPreferences {
  preferredChartType?: string;
  theme?: string;
  enableAnimations: boolean;
  enableInteractivity: boolean;
  colorScheme?: string;
  performance: 'HighQuality' | 'Balanced' | 'HighPerformance';
  accessibility: 'Basic' | 'Standard' | 'Enhanced';
}

export interface DashboardPreferences {
  title?: string;
  themeName?: string;
  refreshInterval?: number;
  enableRealTime: boolean;
  enableCollaboration: boolean;
  enableAnalytics: boolean;
  layout: 'Auto' | 'Grid' | 'Masonry' | 'Responsive' | 'Custom';
  preferredChartTypes: string[];
}

// API Request/Response types
export interface AdvancedVisualizationRequest {
  query: string;
  preferences?: VisualizationPreferences;
  optimizeForPerformance: boolean;
}

export interface AdvancedVisualizationResponse {
  success: boolean;
  visualization?: AdvancedVisualizationConfig;
  dataSummary?: DataSummary;
  performanceMetrics?: ChartPerformanceMetrics;
  errorMessage?: string;
}

export interface AdvancedDashboardRequest {
  query: string;
  preferences?: DashboardPreferences;
}

export interface AdvancedDashboardResponse {
  success: boolean;
  dashboard?: AdvancedDashboardConfig;
  dataSummary?: DataSummary;
  generationMetrics?: DashboardGenerationMetrics;
  errorMessage?: string;
}

export interface VisualizationRecommendationsRequest {
  query: string;
  context?: string;
}

export interface VisualizationRecommendationsResponse {
  success: boolean;
  recommendations?: VisualizationRecommendation[];
  dataSummary?: DataSummary;
  analysisMetrics?: RecommendationAnalysisMetrics;
  errorMessage?: string;
}

export interface DataSummary {
  rowCount: number;
  columnCount: number;
  executionTime: string;
  dataTypes: Record<string, string>;
}

export interface DashboardGenerationMetrics {
  totalCharts: number;
  generationTime: string;
  complexityScore: number;
  recommendationConfidence: number;
}

export interface RecommendationAnalysisMetrics {
  totalRecommendations: number;
  highConfidenceCount: number;
  analysisTime: string;
  dataComplexity: string;
}
