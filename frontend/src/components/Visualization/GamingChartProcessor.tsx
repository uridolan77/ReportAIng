import React, { useMemo } from 'react';
import { AdvancedVisualizationConfig } from '../../types/visualization';

export interface GamingChartData {
  data: any[];
  columns: string[];
  isGamingData: boolean;
  gamingMetrics: {
    labelColumns: string[];
    revenueColumns: string[];
    sessionColumns: string[];
    betColumns: string[];
  };
}

export interface ProcessedChartData {
  chartData: any[];
  config: Partial<AdvancedVisualizationConfig>;
  recommendations: ChartRecommendation[];
}

export interface ChartRecommendation {
  type: 'Bar' | 'Line' | 'Pie' | 'Area';
  title: string;
  description: string;
  xAxis: string;
  metrics: string[];
  priority: number;
}

/**
 * Gaming-specific chart data processor
 * Automatically detects gaming data and provides optimized chart configurations
 */
export const useGamingChartProcessor = (data: any[], columns: string[]): GamingChartData => {
  return useMemo(() => {
    const columnNames = columns.map(col => typeof col === 'string' ? col : (col as any)?.name || col);
    
    // Detect gaming data patterns
    const gamingKeywords = ['GameName', 'Provider', 'GameType', 'TotalRevenue', 'NetGamingRevenue', 'TotalSessions', 'TotalBets'];
    const isGamingData = gamingKeywords.some(keyword => columnNames.includes(keyword));
    
    // Categorize gaming columns
    const labelColumns = columnNames.filter(col => 
      ['GameName', 'Provider', 'GameType', 'Platform'].includes(col)
    );
    
    const revenueColumns = columnNames.filter(col => 
      ['TotalRevenue', 'NetGamingRevenue', 'RealBetAmount', 'RealWinAmount', 'BonusBetAmount', 'BonusWinAmount'].includes(col)
    );
    
    const sessionColumns = columnNames.filter(col => 
      ['TotalSessions', 'NumberofSessions', 'NumberofRealBets', 'NumberofBonusBets'].includes(col)
    );
    
    const betColumns = columnNames.filter(col => 
      ['TotalBets', 'RealBetAmount', 'BonusBetAmount', 'NumberofRealBets', 'NumberofBonusBets'].includes(col)
    );
    
    return {
      data,
      columns: columnNames,
      isGamingData,
      gamingMetrics: {
        labelColumns,
        revenueColumns,
        sessionColumns,
        betColumns
      }
    };
  }, [data, columns]);
};

/**
 * Process gaming data for optimal chart visualization
 */
export const processGamingChartData = (gamingData: GamingChartData): ProcessedChartData => {
  const { data, isGamingData, gamingMetrics } = gamingData;
  
  if (!isGamingData || data.length === 0) {
    return {
      chartData: data,
      config: {},
      recommendations: []
    };
  }
  
  // Generate chart recommendations based on available data
  const recommendations: ChartRecommendation[] = [];
  
  // Game Revenue Analysis
  if (gamingMetrics.labelColumns.includes('GameName') && gamingMetrics.revenueColumns.length > 0) {
    recommendations.push({
      type: 'Bar',
      title: 'Game Revenue Performance',
      description: 'Compare revenue performance across games',
      xAxis: 'GameName',
      metrics: gamingMetrics.revenueColumns.slice(0, 2),
      priority: 1
    });
  }
  
  // Provider Comparison
  if (gamingMetrics.labelColumns.includes('Provider') && gamingMetrics.revenueColumns.length > 0) {
    recommendations.push({
      type: 'Bar',
      title: 'Provider Performance',
      description: 'Compare performance across game providers',
      xAxis: 'Provider',
      metrics: [...gamingMetrics.revenueColumns.slice(0, 1), ...gamingMetrics.sessionColumns.slice(0, 1)],
      priority: 2
    });
  }
  
  // Game Type Distribution
  if (gamingMetrics.labelColumns.includes('GameType') && gamingMetrics.revenueColumns.length > 0) {
    recommendations.push({
      type: 'Pie',
      title: 'Game Type Distribution',
      description: 'Revenue distribution by game type',
      xAxis: 'GameType',
      metrics: [gamingMetrics.revenueColumns[0]],
      priority: 3
    });
  }
  
  // Session Analysis
  if (gamingMetrics.labelColumns.includes('GameName') && gamingMetrics.sessionColumns.length > 0) {
    recommendations.push({
      type: 'Line',
      title: 'Gaming Sessions Analysis',
      description: 'Session patterns and betting behavior',
      xAxis: 'GameName',
      metrics: [...gamingMetrics.sessionColumns.slice(0, 1), ...gamingMetrics.betColumns.slice(0, 1)],
      priority: 4
    });
  }
  
  // Sort recommendations by priority
  recommendations.sort((a, b) => a.priority - b.priority);
  
  // Process data for better chart display
  console.log('🎮 GamingChartProcessor - Starting data processing:', {
    isGamingData,
    dataLength: data.length,
    gamingMetrics,
    firstRow: data[0]
  });

  const processedData = data.map((row, index) => {
    const processed = { ...row, id: index };

    // Track changes for debugging
    const changes: any = {};

    // Ensure numeric values are properly formatted
    gamingMetrics.revenueColumns.forEach(col => {
      if (processed[col] !== undefined) {
        const originalValue = processed[col];
        processed[col] = Number(processed[col]) || 0;
        if (originalValue !== processed[col]) {
          changes[col] = { from: originalValue, to: processed[col] };
        }
      }
    });

    gamingMetrics.sessionColumns.forEach(col => {
      if (processed[col] !== undefined) {
        const originalValue = processed[col];
        processed[col] = Number(processed[col]) || 0;
        if (originalValue !== processed[col]) {
          changes[col] = { from: originalValue, to: processed[col] };
        }
      }
    });

    gamingMetrics.betColumns.forEach(col => {
      if (processed[col] !== undefined) {
        const originalValue = processed[col];
        processed[col] = Number(processed[col]) || 0;
        if (originalValue !== processed[col]) {
          changes[col] = { from: originalValue, to: processed[col] };
        }
      }
    });

    // Log significant changes for first few rows
    if (index < 3 && Object.keys(changes).length > 0) {
      console.log(`🎮 GamingChartProcessor Row ${index} changes:`, changes);
    }

    return processed;
  });

  console.log('🎮 GamingChartProcessor - Data processing complete:', {
    originalLength: data.length,
    processedLength: processedData.length,
    firstOriginal: data[0],
    firstProcessed: processedData[0]
  });
  
  // Generate optimal configuration based on top recommendation
  const topRecommendation = recommendations[0];
  const config: Partial<AdvancedVisualizationConfig> = topRecommendation ? {
    chartType: topRecommendation.type,
    title: topRecommendation.title,
    xAxis: topRecommendation.xAxis,
    yAxis: topRecommendation.metrics[0],
    series: topRecommendation.metrics,
    config: {
      showLegend: topRecommendation.metrics.length > 1,
      showTooltip: true,
      enableAnimation: true,
      colorScheme: 'gaming'
    }
  } : {};
  
  return {
    chartData: processedData,
    config,
    recommendations
  };
};

/**
 * Gaming Chart Processor Component
 * Provides enhanced chart processing for gaming data
 */
export const GamingChartProcessor: React.FC<{
  data: any[];
  columns: string[];
  onProcessed: (processed: ProcessedChartData) => void;
  children?: React.ReactNode;
}> = ({ data, columns, onProcessed, children }) => {
  const gamingData = useGamingChartProcessor(data, columns);
  const processedData = useMemo(() => processGamingChartData(gamingData), [gamingData]);
  
  React.useEffect(() => {
    onProcessed(processedData);
  }, [processedData, onProcessed]);
  
  return <>{children}</>;
};

/**
 * Gaming-specific chart color schemes
 */
export const GAMING_COLOR_SCHEMES = {
  gaming: [
    '#1890ff', // Primary blue for revenue
    '#52c41a', // Green for sessions/wins
    '#faad14', // Orange for bets
    '#f5222d', // Red for losses/risks
    '#722ed1', // Purple for bonuses
    '#13c2c2', // Cyan for special metrics
    '#eb2f96', // Pink for engagement
    '#fa8c16'  // Amber for performance
  ],
  provider: [
    '#1890ff', // Pragmatic Play
    '#52c41a', // Evolution
    '#faad14', // NetEnt
    '#f5222d', // Microgaming
    '#722ed1', // Play'n GO
    '#13c2c2', // Red Tiger
    '#eb2f96', // Big Time Gaming
    '#fa8c16'  // Others
  ],
  gameType: [
    '#1890ff', // Slots
    '#52c41a', // Live Casino
    '#faad14', // Table Games
    '#f5222d', // Jackpots
    '#722ed1'  // Others
  ]
};

export default GamingChartProcessor;
