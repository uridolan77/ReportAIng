/**
 * useCurrentResult Hook - Provides access to current query results across all pages
 * This hook automatically selects the best available result from multiple sources
 */

import { useMemo } from 'react';
import { useActiveResult } from '../stores/activeResultStore';
import { useGlobalResult, usePageResults } from '../stores/globalResultStore';
import type { QueryResponse } from '../types/query';

export interface CurrentResultContext {
  result: QueryResponse | null;
  query: string;
  hasResult: boolean;
  isExpired: boolean;
  lastUpdated: number;
  source: 'active' | 'global' | 'page' | 'shared' | 'none';
  metadata?: {
    pageSource?: string;
    sessionId?: string;
    visualizationConfig?: any;
    dashboardId?: string;
  };
}

/**
 * Main hook for accessing current result data
 * Automatically selects the best available result from multiple sources
 */
export const useCurrentResult = (pageContext?: string): CurrentResultContext => {
  // Get results from different stores
  const activeResult = useActiveResult();
  const globalResult = useGlobalResult();
  const pageResults = usePageResults(pageContext || 'main');

  return useMemo(() => {
    // Priority order: Active > Page-specific > Global > Shared
    
    // 1. Check active result first (most recent)
    if (activeResult.hasResult && !activeResult.isExpired) {
      return {
        result: activeResult.result,
        query: activeResult.query,
        hasResult: true,
        isExpired: false,
        lastUpdated: activeResult.lastUpdated,
        source: 'active'
      };
    }

    // 2. Check page-specific results
    if (pageResults.currentResult) {
      return {
        result: pageResults.currentResult.result,
        query: pageResults.currentResult.query,
        hasResult: true,
        isExpired: false,
        lastUpdated: pageResults.currentResult.timestamp,
        source: 'page',
        metadata: pageResults.currentResult.metadata
      };
    }

    // 3. Check global result
    if (globalResult.hasResult) {
      return {
        result: globalResult.currentResult?.result || null,
        query: globalResult.currentResult?.query || '',
        hasResult: true,
        isExpired: false,
        lastUpdated: globalResult.currentResult?.timestamp || 0,
        source: 'global',
        metadata: globalResult.currentResult?.metadata
      };
    }

    // 4. Check shared results for this page
    if (pageResults.sharedResults.length > 0) {
      const latestShared = pageResults.sharedResults[0];
      return {
        result: latestShared.result,
        query: latestShared.query,
        hasResult: true,
        isExpired: false,
        lastUpdated: latestShared.timestamp,
        source: 'shared',
        metadata: latestShared.metadata
      };
    }

    // 5. No result available
    return {
      result: null,
      query: '',
      hasResult: false,
      isExpired: true,
      lastUpdated: 0,
      source: 'none'
    };
  }, [
    activeResult.hasResult,
    activeResult.isExpired,
    activeResult.result,
    activeResult.query,
    activeResult.lastUpdated,
    pageResults.currentResult,
    pageResults.sharedResults,
    globalResult.hasResult,
    globalResult.currentResult
  ]);
};

/**
 * Hook specifically for visualization pages
 * Provides additional visualization-specific context
 */
export const useVisualizationResult = () => {
  const currentResult = useCurrentResult('visualization');
  
  return useMemo(() => ({
    ...currentResult,
    // Additional visualization-specific helpers
    hasVisualizableData: currentResult.hasResult && 
                        currentResult.result?.success && 
                        currentResult.result?.result?.data?.length > 0,
    dataLength: currentResult.result?.result?.data?.length || 0,
    columnCount: currentResult.result?.result?.metadata?.columns?.length || 0,
    columns: currentResult.result?.result?.metadata?.columns || [],
    data: currentResult.result?.result?.data || [],
    isGamingData: currentResult.result?.result?.metadata?.columns?.some(
      (col: any) => ['GameName', 'Provider', 'TotalRevenue'].includes(col.name || col)
    ) || false
  }), [currentResult]);
};

/**
 * Hook specifically for dashboard pages
 * Provides dashboard-specific context and multi-result management
 */
export const useDashboardResult = () => {
  const currentResult = useCurrentResult('dashboard');
  
  return useMemo(() => ({
    ...currentResult,
    // Additional dashboard-specific helpers
    canCreateDashboard: currentResult.hasResult && currentResult.result?.success,
    suggestedChartTypes: currentResult.hasResult ? 
      ['bar', 'line', 'pie', 'area'] : [],
    multipleDataSources: false // Could be enhanced to support multiple results
  }), [currentResult]);
};

/**
 * Hook for export/sharing functionality
 * Provides export-specific context
 */
export const useExportResult = () => {
  const currentResult = useCurrentResult('export');
  
  return useMemo(() => ({
    ...currentResult,
    // Export-specific helpers
    canExport: currentResult.hasResult && currentResult.result?.success,
    exportFormats: currentResult.hasResult ? ['csv', 'xlsx', 'json', 'pdf'] : [],
    estimatedSize: currentResult.result?.result?.data?.length || 0,
    hasLargeDataset: (currentResult.result?.result?.data?.length || 0) > 10000
  }), [currentResult]);
};

/**
 * Hook for debugging and development
 * Provides detailed information about result sources and state
 */
export const useResultDebugInfo = () => {
  const activeResult = useActiveResult();
  const globalResult = useGlobalResult();
  const pageResults = usePageResults('debug');
  
  return useMemo(() => ({
    activeResult: {
      hasResult: activeResult.hasResult,
      isExpired: activeResult.isExpired,
      lastUpdated: activeResult.lastUpdated,
      query: activeResult.query?.substring(0, 50) + '...'
    },
    globalResult: {
      hasResult: globalResult.hasResult,
      resultId: globalResult.currentResult?.id,
      source: globalResult.currentResult?.source,
      pageSource: globalResult.currentResult?.metadata?.pageSource
    },
    pageResults: {
      hasCurrentResult: !!pageResults.currentResult,
      sharedResultsCount: pageResults.sharedResults.length,
      latestSharedTimestamp: pageResults.sharedResults[0]?.timestamp
    },
    recommendations: {
      preferredSource: activeResult.hasResult ? 'active' : 
                      globalResult.hasResult ? 'global' : 'none',
      dataAvailability: 'good', // Could be enhanced with actual analysis
      syncStatus: 'synchronized' // Could be enhanced with actual sync checking
    }
  }), [activeResult, globalResult, pageResults]);
};

/**
 * Utility hook to check if results are available for specific use cases
 */
export const useResultAvailability = () => {
  const currentResult = useCurrentResult();
  
  return useMemo(() => ({
    // Basic availability
    hasAnyResult: currentResult.hasResult,
    hasValidResult: currentResult.hasResult && currentResult.result?.success,
    hasData: currentResult.hasResult && 
             currentResult.result?.success && 
             currentResult.result?.result?.data?.length > 0,
    
    // Feature-specific availability
    canVisualize: currentResult.hasResult && 
                  currentResult.result?.success && 
                  currentResult.result?.result?.data?.length > 0,
    canExport: currentResult.hasResult && currentResult.result?.success,
    canShare: currentResult.hasResult,
    canCreateDashboard: currentResult.hasResult && 
                       currentResult.result?.success &&
                       currentResult.result?.result?.data?.length > 0,
    
    // Data characteristics
    isLargeDataset: (currentResult.result?.result?.data?.length || 0) > 1000,
    hasMultipleColumns: (currentResult.result?.result?.metadata?.columns?.length || 0) > 1,
    hasNumericData: currentResult.result?.result?.metadata?.columns?.some(
      (col: any) => typeof currentResult.result?.result?.data?.[0]?.[col.name || col] === 'number'
    ) || false,
    hasCategoricalData: currentResult.result?.result?.metadata?.columns?.some(
      (col: any) => typeof currentResult.result?.result?.data?.[0]?.[col.name || col] === 'string'
    ) || false
  }), [currentResult]);
};
