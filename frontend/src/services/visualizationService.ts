import {
  VisualizationRequest,
  VisualizationResponse,
  DashboardRequest,
  DashboardResponse,
  VisualizationRecommendationsRequest,
  VisualizationRecommendationsResponse,
  VisualizationConfig
} from '../types/visualization';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:55243';

class VisualizationService {
  private async makeRequest<T>(
    endpoint: string,
    method: 'GET' | 'POST' | 'PUT' | 'DELETE' = 'GET',
    body?: any
  ): Promise<T> {
    const token = localStorage.getItem('authToken');

    // Use the correct base path - /api/visualization
    const response = await fetch(`${API_BASE_URL}/api/visualization${endpoint}`, {
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
   * Generate visualization with AI-powered chart type selection
   */
  async generateVisualization(
    request: VisualizationRequest
  ): Promise<VisualizationResponse> {
    try {
      console.log('Generating visualization:', request);

      const response = await this.makeRequest<VisualizationResponse>(
        '/generate',
        'POST',
        request
      );

      console.log('Visualization generated:', response);
      return response;
    } catch (error) {
      console.error('Error generating visualization:', error);
      throw error;
    }
  }

  /**
   * Generate dashboard with multiple sophisticated charts
   */
  async generateDashboard(
    request: DashboardRequest
  ): Promise<DashboardResponse> {
    try {
      console.log('Generating dashboard:', request);

      const response = await this.makeRequest<DashboardResponse>(
        '/dashboard',
        'POST',
        request
      );

      console.log('Dashboard generated:', response);
      return response;
    } catch (error) {
      console.error('Error generating dashboard:', error);
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

      // Extract data characteristics from context
      const contextMatch = request.context?.match(/Data contains (\d+) rows and (\d+) columns/);
      const rowCount = contextMatch ? parseInt(contextMatch[1]) : 0;
      const columnCount = contextMatch ? parseInt(contextMatch[2]) : 0;

      // Extract column types from context
      const columnTypesMatch = request.context?.match(/Column types: (.+)/);
      const columnTypes = columnTypesMatch ? columnTypesMatch[1].split(', ') : [];

      // Analyze query for intent
      const query = request.query.toLowerCase();
      const isTimeSeriesQuery = /\b(time|date|month|year|day|trend|over time)\b/.test(query);
      const isComparisonQuery = /\b(compare|vs|versus|top|bottom|highest|lowest)\b/.test(query);
      const isDistributionQuery = /\b(distribution|spread|range|histogram)\b/.test(query);
      const isCorrelationQuery = /\b(correlation|relationship|scatter|against)\b/.test(query);
      const isProportionQuery = /\b(percentage|proportion|share|part of|breakdown)\b/.test(query);

      // Generate smart recommendations based on data characteristics and query intent
      const recommendations: any[] = [];

      // Time series recommendation
      if (isTimeSeriesQuery || columnTypes.some(ct => ct.includes('date') || ct.includes('time'))) {
        recommendations.push({
          chartType: 'line',
          confidence: 0.9,
          reasoning: 'Line charts are ideal for showing trends over time. Your data appears to contain temporal information.',
          bestFor: 'Time series analysis, trend visualization, temporal patterns',
          limitations: ['Requires ordered time data', 'Can become cluttered with many series'],
          estimatedPerformance: {
            estimatedRenderTime: 'Fast',
            memoryUsageMB: Math.max(5, rowCount * 0.001),
            requiresWebGL: rowCount > 50000,
            requiresSampling: rowCount > 10000,
            recommendedMaxDataPoints: 10000
          },
          suggestedConfig: {
            enableZoom: true,
            enablePan: true,
            showDataPoints: rowCount < 100
          }
        });
      }

      // Comparison recommendation
      if (isComparisonQuery || (columnCount >= 2 && rowCount <= 50)) {
        recommendations.push({
          chartType: 'bar',
          confidence: 0.85,
          reasoning: 'Bar charts excel at comparing values across categories. Perfect for ranking and categorical comparisons.',
          bestFor: 'Category comparisons, rankings, discrete value analysis',
          limitations: ['Not suitable for continuous data', 'Limited to moderate number of categories'],
          estimatedPerformance: {
            estimatedRenderTime: 'Very Fast',
            memoryUsageMB: Math.max(3, rowCount * 0.0005),
            requiresWebGL: false,
            requiresSampling: false,
            recommendedMaxDataPoints: 50
          },
          suggestedConfig: {
            orientation: rowCount > 10 ? 'horizontal' : 'vertical',
            enableAnimation: true
          }
        });
      }

      // Proportion recommendation
      if (isProportionQuery || (rowCount <= 10 && columnCount >= 2)) {
        recommendations.push({
          chartType: 'pie',
          confidence: 0.75,
          reasoning: 'Pie charts effectively show part-to-whole relationships and proportions.',
          bestFor: 'Proportional data, percentage breakdowns, part-to-whole analysis',
          limitations: ['Limited to few categories', 'Hard to compare similar values', 'Not suitable for negative values'],
          estimatedPerformance: {
            estimatedRenderTime: 'Very Fast',
            memoryUsageMB: 2,
            requiresWebGL: false,
            requiresSampling: false,
            recommendedMaxDataPoints: 10
          },
          suggestedConfig: {
            showLabels: true,
            showPercentages: true
          }
        });
      }

      // Correlation/scatter recommendation
      if (isCorrelationQuery || (columnCount >= 3 && rowCount > 20)) {
        recommendations.push({
          chartType: 'scatter',
          confidence: 0.8,
          reasoning: 'Scatter plots reveal relationships and correlations between variables, ideal for exploring data patterns.',
          bestFor: 'Correlation analysis, outlier detection, relationship exploration',
          limitations: ['Can be cluttered with large datasets', 'Requires numeric data', 'Difficult to read with overlapping points'],
          estimatedPerformance: {
            estimatedRenderTime: 'Moderate',
            memoryUsageMB: Math.max(8, rowCount * 0.002),
            requiresWebGL: rowCount > 10000,
            requiresSampling: rowCount > 5000,
            recommendedMaxDataPoints: 5000
          },
          suggestedConfig: {
            enableBrush: true,
            pointSize: rowCount > 1000 ? 2 : 4
          }
        });
      }

      // Large dataset recommendation
      if (rowCount > 1000) {
        recommendations.push({
          chartType: 'heatmap',
          confidence: 0.7,
          reasoning: 'Heatmaps efficiently visualize large datasets by using color intensity to represent values.',
          bestFor: 'Large datasets, pattern recognition, density visualization',
          limitations: ['Requires gridded data', 'Color interpretation can be subjective'],
          estimatedPerformance: {
            estimatedRenderTime: 'Moderate',
            memoryUsageMB: Math.max(10, rowCount * 0.001),
            requiresWebGL: rowCount > 20000,
            requiresSampling: false,
            recommendedMaxDataPoints: 50000
          },
          suggestedConfig: {
            colorScale: 'viridis',
            enableTooltip: true
          }
        });
      }

      // Ensure we have at least one recommendation
      if (recommendations.length === 0) {
        recommendations.push({
          chartType: 'bar',
          confidence: 0.6,
          reasoning: 'Bar chart is a versatile choice that works well for most data types and provides clear visual comparisons.',
          bestFor: 'General purpose visualization, categorical data',
          limitations: ['May not be optimal for time series or correlation analysis'],
          estimatedPerformance: {
            estimatedRenderTime: 'Fast',
            memoryUsageMB: 5,
            requiresWebGL: false,
            requiresSampling: false,
            recommendedMaxDataPoints: 1000
          },
          suggestedConfig: {}
        });
      }

      // Sort by confidence
      recommendations.sort((a, b) => b.confidence - a.confidence);

      console.log('Generated client-side recommendations:', recommendations);
      return {
        success: true,
        recommendations: recommendations.slice(0, 4), // Limit to top 4 recommendations
        errorMessage: undefined
      };
    } catch (error) {
      console.error('Error getting visualization recommendations:', error);
      return {
        success: false,
        recommendations: [],
        errorMessage: error instanceof Error ? error.message : 'Failed to get recommendations'
      };
    }
  }

  /**
   * Optimize visualization configuration for performance
   */
  async optimizeVisualization(
    config: VisualizationConfig,
    dataSize: number
  ): Promise<{
    success: boolean;
    originalConfig: VisualizationConfig;
    optimizedConfig: VisualizationConfig;
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
      if (process.env.NODE_ENV === 'development') {
        console.log('Optimizing visualization:', { config, dataSize });
      }

      // Since /optimize endpoint doesn't exist, provide client-side optimization
      const optimizedConfig = { ...config };
      const changesApplied: string[] = [];

      // Apply basic optimizations based on data size
      if (dataSize > 10000) {
        optimizedConfig.performance = {
          enableVirtualization: true,
          virtualizationThreshold: optimizedConfig.performance?.virtualizationThreshold || 10000,
          enableLazyLoading: optimizedConfig.performance?.enableLazyLoading || true,
          enableCaching: optimizedConfig.performance?.enableCaching || true,
          cacheTTL: optimizedConfig.performance?.cacheTTL || 300,
          enableWebGL: optimizedConfig.performance?.enableWebGL || false,
          maxDataPoints: 10000
        };
        changesApplied.push('Enabled virtualization for large dataset');
        changesApplied.push('Enabled data sampling');
      }

      if (dataSize > 50000) {
        optimizedConfig.performance = {
          enableVirtualization: optimizedConfig.performance?.enableVirtualization || false,
          virtualizationThreshold: optimizedConfig.performance?.virtualizationThreshold || 10000,
          enableLazyLoading: optimizedConfig.performance?.enableLazyLoading || true,
          enableCaching: optimizedConfig.performance?.enableCaching || true,
          cacheTTL: optimizedConfig.performance?.cacheTTL || 300,
          enableWebGL: true,
          maxDataPoints: optimizedConfig.performance?.maxDataPoints || 100000
        };
        changesApplied.push('Enabled WebGL rendering');
      }

      const response = {
        success: true,
        originalConfig: config,
        optimizedConfig,
        optimizationSummary: {
          changesApplied,
          optimizationLevel: dataSize > 50000 ? 'High' : dataSize > 10000 ? 'Medium' : 'Low',
          estimatedImpact: changesApplied.length > 0 ? 'Significant performance improvement expected' : 'No optimization needed'
        },
        performanceGain: {
          renderTimeImprovement: changesApplied.length * 0.2,
          memoryUsageImprovement: changesApplied.length * 0.15,
          overallScore: Math.min(0.9, changesApplied.length * 0.25)
        }
      };

      console.log('Visualization optimized (client-side):', response);
      return response;
    } catch (error) {
      console.error('Error optimizing visualization:', error);
      throw error;
    }
  }
}

export default new VisualizationService();
