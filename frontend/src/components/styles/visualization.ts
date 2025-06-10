/**
 * Visualization Styles Export
 * Type-safe style constants for visualization components
 */

// Import CSS files
import './visualization.css';

// Export style constants for TypeScript usage
export const visualizationStyles = {
  panel: 'visualization-panel',
  header: 'visualization-header',
  title: 'visualization-title',
  controls: 'visualization-controls'
} as const;

export const chartStyles = {
  container: 'chart-container',
  wrapper: 'chart-wrapper',
  canvas: 'chart-canvas',
  loading: 'chart-loading',
  skeleton: 'chart-skeleton',
  error: 'chart-error',
  errorIcon: 'chart-error-icon',
  errorTitle: 'chart-error-title',
  errorMessage: 'chart-error-message'
} as const;

export const chartTypeStyles = {
  selector: 'chart-type-selector',
  button: 'chart-type-button',
  buttonActive: 'chart-type-button active'
} as const;

export const chartConfigStyles = {
  panel: 'chart-config-panel',
  section: 'chart-config-section',
  label: 'chart-config-label',
  group: 'chart-config-group',
  field: 'chart-config-field'
} as const;

export const interactiveStyles = {
  visualization: 'interactive-visualization',
  toolbar: 'interactive-toolbar',
  tool: 'interactive-tool',
  toolActive: 'interactive-tool active'
} as const;

export const legendStyles = {
  legend: 'chart-legend',
  item: 'legend-item',
  color: 'legend-color'
} as const;

export const tooltipStyles = {
  tooltip: 'chart-tooltip',
  title: 'chart-tooltip-title',
  content: 'chart-tooltip-content',
  item: 'chart-tooltip-item'
} as const;

export const d3Styles = {
  container: 'd3-chart-container',
  svg: 'd3-chart-svg',
  axis: 'd3-axis',
  axisLabel: 'd3-axis-label',
  gridLine: 'd3-grid-line'
} as const;

// Type definitions
export interface VisualizationProps {
  className?: string;
  loading?: boolean;
  error?: string | null;
  interactive?: boolean;
}

export interface ChartProps {
  type: 'bar' | 'line' | 'pie' | 'scatter' | 'area';
  data: any[];
  config?: ChartConfig;
  className?: string;
}

export interface ChartConfig {
  width?: number;
  height?: number;
  colors?: string[];
  showLegend?: boolean;
  showTooltip?: boolean;
  responsive?: boolean;
}

export interface InteractiveToolProps {
  tools: ('zoom' | 'pan' | 'select' | 'filter')[];
  activeTool?: string;
  onToolChange: (tool: string) => void;
}
