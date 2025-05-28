import {
  AdvancedVisualizationRequest,
  AdvancedVisualizationResponse,
  AdvancedDashboardRequest,
  AdvancedDashboardResponse,
  VisualizationRecommendationsRequest,
  VisualizationRecommendationsResponse,
  AdvancedVisualizationConfig
} from '../types/visualization';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:55243';

class AdvancedVisualizationService {
  private async makeRequest<T>(
    endpoint: string,
    method: 'GET' | 'POST' | 'PUT' | 'DELETE' = 'GET',
    body?: any
  ): Promise<T> {
    const token = localStorage.getItem('authToken');

    const response = await fetch(`${API_BASE_URL}/api/advanced-visualization${endpoint}`, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': token ? `Bearer ${token}` : '',
      },
      body: body ? JSON.stringify(body) : undefined,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ error: 'Network error' }));
      throw new Error(errorData.error || `HTTP ${response.status}: ${response.statusText}`);
    }

    return response.json();
  }

  /**
   * Generate advanced visualization with AI-powered chart type selection
   */
  async generateAdvancedVisualization(
    request: AdvancedVisualizationRequest
  ): Promise<AdvancedVisualizationResponse> {
    try {
      console.log('Generating advanced visualization:', request);

      const response = await this.makeRequest<AdvancedVisualizationResponse>(
        '/generate',
        'POST',
        request
      );

      console.log('Advanced visualization generated:', response);
      return response;
    } catch (error) {
      console.error('Error generating advanced visualization:', error);
      throw error;
    }
  }

  /**
   * Generate advanced dashboard with multiple sophisticated charts
   */
  async generateAdvancedDashboard(
    request: AdvancedDashboardRequest
  ): Promise<AdvancedDashboardResponse> {
    try {
      console.log('Generating advanced dashboard:', request);

      const response = await this.makeRequest<AdvancedDashboardResponse>(
        '/dashboard',
        'POST',
        request
      );

      console.log('Advanced dashboard generated:', response);
      return response;
    } catch (error) {
      console.error('Error generating advanced dashboard:', error);
      throw error;
    }
  }

  /**
   * Get AI-powered visualization recommendations
   */
  async getVisualizationRecommendations(
    request: VisualizationRecommendationsRequest
  ): Promise<VisualizationRecommendationsResponse> {
    try {
      console.log('Getting visualization recommendations:', request);

      const response = await this.makeRequest<VisualizationRecommendationsResponse>(
        '/recommendations',
        'POST',
        request
      );

      console.log('Visualization recommendations received:', response);
      return response;
    } catch (error) {
      console.error('Error getting visualization recommendations:', error);
      throw error;
    }
  }

  /**
   * Optimize visualization configuration for performance
   */
  async optimizeVisualization(
    config: AdvancedVisualizationConfig,
    dataSize: number
  ): Promise<{
    success: boolean;
    originalConfig: AdvancedVisualizationConfig;
    optimizedConfig: AdvancedVisualizationConfig;
    optimizationSummary: {
      changesApplied: string[];
      optimizationLevel: string;
      estimatedImpact: string;
    };
    performanceGain: {
      renderTimeImprovement: number;
      memoryUsageImprovement: number;
      overallScore: number;
    };
    errorMessage?: string;
  }> {
    try {
      console.log('Optimizing visualization:', { config, dataSize });

      const response = await this.makeRequest<any>(
        '/optimize',
        'POST',
        { configuration: config, dataSize }
      );

      console.log('Visualization optimized:', response);
      return response;
    } catch (error) {
      console.error('Error optimizing visualization:', error);
      throw error;
    }
  }

  /**
   * Export visualization in various formats
   */
  async exportVisualization(
    config: AdvancedVisualizationConfig,
    format: 'png' | 'svg' | 'pdf' | 'excel' | 'csv',
    data: any[]
  ): Promise<Blob> {
    try {
      console.log('Exporting visualization:', { format, config });

      const token = localStorage.getItem('authToken');

      const response = await fetch(`${API_BASE_URL}/api/visualization/export`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token ? `Bearer ${token}` : '',
        },
        body: JSON.stringify({
          config,
          format,
          data,
          options: {
            width: config.export?.imageWidth || 1200,
            height: config.export?.imageHeight || 800,
            dpi: config.export?.imageDPI || 300
          }
        }),
      });

      if (!response.ok) {
        throw new Error(`Export failed: ${response.statusText}`);
      }

      const blob = await response.blob();
      console.log('Visualization exported successfully');
      return blob;
    } catch (error) {
      console.error('Error exporting visualization:', error);
      throw error;
    }
  }

  /**
   * Save visualization configuration for later use
   */
  async saveVisualizationConfig(
    name: string,
    config: AdvancedVisualizationConfig,
    description?: string
  ): Promise<{ success: boolean; id: string }> {
    try {
      console.log('Saving visualization config:', { name, config });

      const response = await this.makeRequest<{ success: boolean; id: string }>(
        '/save-config',
        'POST',
        { name, config, description }
      );

      console.log('Visualization config saved:', response);
      return response;
    } catch (error) {
      console.error('Error saving visualization config:', error);
      throw error;
    }
  }

  /**
   * Load saved visualization configurations
   */
  async loadVisualizationConfigs(): Promise<{
    success: boolean;
    configs: Array<{
      id: string;
      name: string;
      description?: string;
      config: AdvancedVisualizationConfig;
      createdAt: string;
      updatedAt: string;
    }>;
  }> {
    try {
      console.log('Loading visualization configs');

      const response = await this.makeRequest<any>('/configs');

      console.log('Visualization configs loaded:', response);
      return response;
    } catch (error) {
      console.error('Error loading visualization configs:', error);
      throw error;
    }
  }

  /**
   * Get chart type capabilities and recommendations
   */
  async getChartTypeCapabilities(): Promise<{
    success: boolean;
    chartTypes: Array<{
      type: string;
      name: string;
      description: string;
      bestFor: string[];
      limitations: string[];
      requiredDataTypes: string[];
      maxRecommendedRows: number;
      interactiveFeatures: string[];
      performanceRating: number;
    }>;
  }> {
    try {
      console.log('Getting chart type capabilities');

      const response = await this.makeRequest<any>('/chart-types');

      console.log('Chart type capabilities loaded:', response);
      return response;
    } catch (error) {
      console.error('Error getting chart type capabilities:', error);
      throw error;
    }
  }

  /**
   * Validate visualization configuration
   */
  async validateVisualizationConfig(
    config: AdvancedVisualizationConfig,
    data: any[]
  ): Promise<{
    success: boolean;
    isValid: boolean;
    warnings: string[];
    errors: string[];
    suggestions: string[];
  }> {
    try {
      console.log('Validating visualization config:', config);

      const response = await this.makeRequest<any>(
        '/validate',
        'POST',
        { config, dataSize: data.length, sampleData: data.slice(0, 10) }
      );

      console.log('Visualization config validated:', response);
      return response;
    } catch (error) {
      console.error('Error validating visualization config:', error);
      throw error;
    }
  }

  /**
   * Get performance metrics for a visualization
   */
  async getPerformanceMetrics(
    config: AdvancedVisualizationConfig,
    dataSize: number
  ): Promise<{
    success: boolean;
    metrics: {
      estimatedRenderTime: number;
      estimatedMemoryUsage: number;
      recommendedOptimizations: string[];
      performanceScore: number;
      scalabilityRating: number;
    };
  }> {
    try {
      console.log('Getting performance metrics:', { config, dataSize });

      const response = await this.makeRequest<any>(
        '/performance-metrics',
        'POST',
        { config, dataSize }
      );

      console.log('Performance metrics received:', response);
      return response;
    } catch (error) {
      console.error('Error getting performance metrics:', error);
      throw error;
    }
  }

  /**
   * Download exported file
   */
  downloadFile(blob: Blob, filename: string, format: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${filename}.${format}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Generate filename for export
   */
  generateExportFilename(config: AdvancedVisualizationConfig, format: string): string {
    const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
    const chartType = config.chartType.toLowerCase();
    const title = config.title?.replace(/[^a-zA-Z0-9]/g, '_') || 'chart';

    return `${title}_${chartType}_${timestamp}.${format}`;
  }
}

// Create singleton instance
const advancedVisualizationService = new AdvancedVisualizationService();

export default advancedVisualizationService;
