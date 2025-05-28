import { apiClient } from './apiClient';
import { API_CONFIG } from '../config/api';

export interface DataInsight {
  id: string;
  type: 'trend' | 'anomaly' | 'correlation' | 'pattern' | 'summary' | 'recommendation';
  title: string;
  description: string;
  confidence: number;
  severity: 'low' | 'medium' | 'high';
  visualization?: {
    type: 'line' | 'bar' | 'scatter' | 'heatmap' | 'metric';
    config: Record<string, any>;
    data?: any[];
  };
  actions?: InsightAction[];
  metadata: {
    generatedAt: string;
    dataPoints: number;
    columns: string[];
    timeRange?: string;
  };
}

export interface InsightAction {
  id: string;
  label: string;
  type: 'query' | 'filter' | 'drill_down' | 'export' | 'alert';
  payload: Record<string, any>;
}

export interface InsightRequest {
  data: any[];
  columns: ColumnInfo[];
  analysisDepth: 'quick' | 'standard' | 'comprehensive';
  focusAreas?: string[];
  timeColumn?: string;
  valueColumns?: string[];
}

export interface ColumnInfo {
  name: string;
  type: 'numeric' | 'text' | 'date' | 'boolean';
  nullable: boolean;
  unique?: boolean;
  description?: string;
}

class InsightsService {
  private cache = new Map<string, { insights: DataInsight[]; timestamp: number }>();
  private readonly CACHE_TTL = 300000; // 5 minutes

  async generateInsights(request: InsightRequest): Promise<DataInsight[]> {
    const cacheKey = this.generateCacheKey(request);
    const cached = this.cache.get(cacheKey);
    
    if (cached && Date.now() - cached.timestamp < this.CACHE_TTL) {
      return cached.insights;
    }

    try {
      // For now, generate insights locally until backend AI service is ready
      const insights = await this.generateLocalInsights(request);
      
      // Cache the results
      this.cache.set(cacheKey, {
        insights,
        timestamp: Date.now()
      });

      return insights;
    } catch (error) {
      console.error('Failed to generate insights:', error);
      return [];
    }
  }

  private async generateLocalInsights(request: InsightRequest): Promise<DataInsight[]> {
    const insights: DataInsight[] = [];
    const { data, columns, analysisDepth } = request;

    if (!data || data.length === 0) {
      return insights;
    }

    // Generate summary insights
    insights.push(...this.generateSummaryInsights(data, columns));

    // Generate trend insights for numeric columns
    insights.push(...this.generateTrendInsights(data, columns, request.timeColumn));

    // Generate anomaly detection
    if (analysisDepth !== 'quick') {
      insights.push(...this.generateAnomalyInsights(data, columns));
    }

    // Generate correlation insights
    if (analysisDepth === 'comprehensive') {
      insights.push(...this.generateCorrelationInsights(data, columns));
    }

    // Generate pattern insights
    insights.push(...this.generatePatternInsights(data, columns));

    // Generate recommendations
    insights.push(...this.generateRecommendations(data, columns));

    return insights.sort((a, b) => b.confidence - a.confidence);
  }

  private generateSummaryInsights(data: any[], columns: ColumnInfo[]): DataInsight[] {
    const insights: DataInsight[] = [];
    const numericColumns = columns.filter(col => col.type === 'numeric');

    for (const column of numericColumns) {
      const values = data.map(row => row[column.name]).filter(val => val != null && !isNaN(val));
      
      if (values.length === 0) continue;

      const sum = values.reduce((a, b) => a + b, 0);
      const avg = sum / values.length;
      const min = Math.min(...values);
      const max = Math.max(...values);
      const median = this.calculateMedian(values);

      insights.push({
        id: `summary_${column.name}`,
        type: 'summary',
        title: `${column.name} Summary Statistics`,
        description: `Average: ${avg.toLocaleString()}, Range: ${min.toLocaleString()} - ${max.toLocaleString()}, Median: ${median.toLocaleString()}`,
        confidence: 0.95,
        severity: 'low',
        visualization: {
          type: 'metric',
          config: {
            metrics: [
              { label: 'Average', value: avg, format: 'number' },
              { label: 'Total', value: sum, format: 'number' },
              { label: 'Min', value: min, format: 'number' },
              { label: 'Max', value: max, format: 'number' }
            ]
          }
        },
        actions: [
          {
            id: 'drill_down_summary',
            label: 'View Detailed Distribution',
            type: 'query',
            payload: { column: column.name, type: 'distribution' }
          }
        ],
        metadata: {
          generatedAt: new Date().toISOString(),
          dataPoints: values.length,
          columns: [column.name]
        }
      });
    }

    return insights;
  }

  private generateTrendInsights(data: any[], columns: ColumnInfo[], timeColumn?: string): DataInsight[] {
    const insights: DataInsight[] = [];
    
    if (!timeColumn) {
      // Try to find a date column
      timeColumn = columns.find(col => col.type === 'date')?.name;
    }

    if (!timeColumn) return insights;

    const numericColumns = columns.filter(col => col.type === 'numeric');
    
    for (const column of numericColumns) {
      const timeSeriesData = data
        .filter(row => row[timeColumn!] && row[column.name] != null)
        .sort((a, b) => new Date(a[timeColumn!]).getTime() - new Date(b[timeColumn!]).getTime())
        .map(row => ({
          date: new Date(row[timeColumn!]),
          value: row[column.name]
        }));

      if (timeSeriesData.length < 3) continue;

      const trend = this.calculateTrend(timeSeriesData);
      
      if (Math.abs(trend.slope) > 0.1) {
        const direction = trend.slope > 0 ? 'increasing' : 'decreasing';
        const strength = Math.abs(trend.slope) > 0.5 ? 'strongly' : 'gradually';
        
        insights.push({
          id: `trend_${column.name}`,
          type: 'trend',
          title: `${column.name} is ${strength} ${direction}`,
          description: `${column.name} shows a ${direction} trend over time with ${Math.abs(trend.slope * 100).toFixed(1)}% change rate`,
          confidence: Math.min(0.9, Math.abs(trend.correlation)),
          severity: Math.abs(trend.slope) > 0.3 ? 'high' : 'medium',
          visualization: {
            type: 'line',
            config: {
              xAxis: timeColumn,
              yAxis: column.name,
              showTrendLine: true
            },
            data: timeSeriesData
          },
          actions: [
            {
              id: 'forecast_trend',
              label: 'Generate Forecast',
              type: 'query',
              payload: { column: column.name, type: 'forecast' }
            }
          ],
          metadata: {
            generatedAt: new Date().toISOString(),
            dataPoints: timeSeriesData.length,
            columns: [timeColumn, column.name],
            timeRange: `${timeSeriesData[0].date.toISOString()} to ${timeSeriesData[timeSeriesData.length - 1].date.toISOString()}`
          }
        });
      }
    }

    return insights;
  }

  private generateAnomalyInsights(data: any[], columns: ColumnInfo[]): DataInsight[] {
    const insights: DataInsight[] = [];
    const numericColumns = columns.filter(col => col.type === 'numeric');

    for (const column of numericColumns) {
      const values = data.map(row => row[column.name]).filter(val => val != null && !isNaN(val));
      
      if (values.length < 10) continue;

      const anomalies = this.detectAnomalies(values);
      
      if (anomalies.length > 0) {
        const anomalyPercentage = (anomalies.length / values.length) * 100;
        
        insights.push({
          id: `anomaly_${column.name}`,
          type: 'anomaly',
          title: `${anomalies.length} anomalies detected in ${column.name}`,
          description: `${anomalyPercentage.toFixed(1)}% of values are statistical outliers`,
          confidence: 0.8,
          severity: anomalyPercentage > 5 ? 'high' : 'medium',
          visualization: {
            type: 'scatter',
            config: {
              highlightAnomalies: true,
              anomalyIndices: anomalies
            }
          },
          actions: [
            {
              id: 'filter_anomalies',
              label: 'Filter Out Anomalies',
              type: 'filter',
              payload: { column: column.name, excludeAnomalies: true }
            }
          ],
          metadata: {
            generatedAt: new Date().toISOString(),
            dataPoints: values.length,
            columns: [column.name]
          }
        });
      }
    }

    return insights;
  }

  private generateCorrelationInsights(data: any[], columns: ColumnInfo[]): DataInsight[] {
    const insights: DataInsight[] = [];
    const numericColumns = columns.filter(col => col.type === 'numeric');

    if (numericColumns.length < 2) return insights;

    for (let i = 0; i < numericColumns.length; i++) {
      for (let j = i + 1; j < numericColumns.length; j++) {
        const col1 = numericColumns[i];
        const col2 = numericColumns[j];
        
        const correlation = this.calculateCorrelation(data, col1.name, col2.name);
        
        if (Math.abs(correlation) > 0.5) {
          const strength = Math.abs(correlation) > 0.8 ? 'strong' : 'moderate';
          const direction = correlation > 0 ? 'positive' : 'negative';
          
          insights.push({
            id: `correlation_${col1.name}_${col2.name}`,
            type: 'correlation',
            title: `${strength} ${direction} correlation between ${col1.name} and ${col2.name}`,
            description: `Correlation coefficient: ${correlation.toFixed(3)}`,
            confidence: Math.abs(correlation),
            severity: Math.abs(correlation) > 0.8 ? 'high' : 'medium',
            visualization: {
              type: 'scatter',
              config: {
                xAxis: col1.name,
                yAxis: col2.name,
                showTrendLine: true
              }
            },
            actions: [
              {
                id: 'explore_correlation',
                label: 'Explore Relationship',
                type: 'drill_down',
                payload: { columns: [col1.name, col2.name] }
              }
            ],
            metadata: {
              generatedAt: new Date().toISOString(),
              dataPoints: data.length,
              columns: [col1.name, col2.name]
            }
          });
        }
      }
    }

    return insights;
  }

  private generatePatternInsights(data: any[], columns: ColumnInfo[]): DataInsight[] {
    const insights: DataInsight[] = [];
    
    // Detect seasonal patterns, cyclical patterns, etc.
    // This is a simplified implementation
    
    return insights;
  }

  private generateRecommendations(data: any[], columns: ColumnInfo[]): DataInsight[] {
    const insights: DataInsight[] = [];
    
    // Generate actionable recommendations based on the data
    if (data.length > 1000) {
      insights.push({
        id: 'recommendation_sampling',
        type: 'recommendation',
        title: 'Consider data sampling for better performance',
        description: 'Your dataset is large. Consider using sampling or pagination for faster analysis.',
        confidence: 0.9,
        severity: 'medium',
        actions: [
          {
            id: 'apply_sampling',
            label: 'Apply Random Sampling',
            type: 'filter',
            payload: { type: 'sample', size: 1000 }
          }
        ],
        metadata: {
          generatedAt: new Date().toISOString(),
          dataPoints: data.length,
          columns: columns.map(col => col.name)
        }
      });
    }

    return insights;
  }

  // Utility methods
  private calculateMedian(values: number[]): number {
    const sorted = [...values].sort((a, b) => a - b);
    const mid = Math.floor(sorted.length / 2);
    return sorted.length % 2 === 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
  }

  private calculateTrend(data: { date: Date; value: number }[]): { slope: number; correlation: number } {
    const n = data.length;
    const x = data.map((_, i) => i);
    const y = data.map(d => d.value);
    
    const sumX = x.reduce((a, b) => a + b, 0);
    const sumY = y.reduce((a, b) => a + b, 0);
    const sumXY = x.reduce((sum, xi, i) => sum + xi * y[i], 0);
    const sumXX = x.reduce((sum, xi) => sum + xi * xi, 0);
    
    const slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
    
    // Calculate correlation coefficient
    const meanX = sumX / n;
    const meanY = sumY / n;
    const numerator = x.reduce((sum, xi, i) => sum + (xi - meanX) * (y[i] - meanY), 0);
    const denomX = Math.sqrt(x.reduce((sum, xi) => sum + (xi - meanX) ** 2, 0));
    const denomY = Math.sqrt(y.reduce((sum, yi) => sum + (yi - meanY) ** 2, 0));
    const correlation = numerator / (denomX * denomY);
    
    return { slope, correlation };
  }

  private detectAnomalies(values: number[]): number[] {
    const mean = values.reduce((a, b) => a + b, 0) / values.length;
    const variance = values.reduce((sum, val) => sum + (val - mean) ** 2, 0) / values.length;
    const stdDev = Math.sqrt(variance);
    const threshold = 2 * stdDev;
    
    return values
      .map((value, index) => ({ value, index }))
      .filter(({ value }) => Math.abs(value - mean) > threshold)
      .map(({ index }) => index);
  }

  private calculateCorrelation(data: any[], col1: string, col2: string): number {
    const pairs = data
      .filter(row => row[col1] != null && row[col2] != null && !isNaN(row[col1]) && !isNaN(row[col2]))
      .map(row => ({ x: row[col1], y: row[col2] }));
    
    if (pairs.length < 2) return 0;
    
    const n = pairs.length;
    const sumX = pairs.reduce((sum, p) => sum + p.x, 0);
    const sumY = pairs.reduce((sum, p) => sum + p.y, 0);
    const sumXY = pairs.reduce((sum, p) => sum + p.x * p.y, 0);
    const sumXX = pairs.reduce((sum, p) => sum + p.x * p.x, 0);
    const sumYY = pairs.reduce((sum, p) => sum + p.y * p.y, 0);
    
    const numerator = n * sumXY - sumX * sumY;
    const denominator = Math.sqrt((n * sumXX - sumX * sumX) * (n * sumYY - sumY * sumY));
    
    return denominator === 0 ? 0 : numerator / denominator;
  }

  private generateCacheKey(request: InsightRequest): string {
    return btoa(JSON.stringify({
      dataHash: this.hashData(request.data),
      columns: request.columns.map(col => col.name).sort(),
      analysisDepth: request.analysisDepth
    }));
  }

  private hashData(data: any[]): string {
    // Simple hash of first and last few rows for cache key
    const sample = [...data.slice(0, 3), ...data.slice(-3)];
    return btoa(JSON.stringify(sample)).slice(0, 16);
  }

  clearCache(): void {
    this.cache.clear();
  }
}

export const insightsService = new InsightsService();
