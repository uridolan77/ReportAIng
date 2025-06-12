/**
 * Bundle Optimization Utilities
 * 
 * Advanced bundle optimization strategies for React 18 applications
 * including code splitting, lazy loading, and performance monitoring
 */

// Dynamic import with retry logic
export const dynamicImport = async <T = any>(
  importFn: () => Promise<T>,
  retries = 3,
  delay = 1000
): Promise<T> => {
  for (let i = 0; i < retries; i++) {
    try {
      return await importFn();
    } catch (error) {
      if (i === retries - 1) throw error;
      await new Promise(resolve => setTimeout(resolve, delay * (i + 1)));
    }
  }
  throw new Error('Dynamic import failed after retries');
};

// Preload strategy for critical resources
export const preloadCriticalResources = () => {
  const criticalChunks = [
    '/static/js/query-page',
    '/static/js/dashboard-page',
    '/static/js/results-page'
  ];

  criticalChunks.forEach(chunk => {
    const link = document.createElement('link');
    link.rel = 'preload';
    link.as = 'script';
    link.href = `${chunk}.js`;
    document.head.appendChild(link);
  });
};

// Intelligent code splitting based on user behavior
export class IntelligentCodeSplitter {
  private static instance: IntelligentCodeSplitter;
  private loadedChunks = new Set<string>();
  private preloadQueue = new Map<string, Promise<any>>();
  private userBehavior = {
    visitedRoutes: new Set<string>(),
    timeSpent: new Map<string, number>(),
    interactions: new Map<string, number>()
  };

  static getInstance(): IntelligentCodeSplitter {
    if (!IntelligentCodeSplitter.instance) {
      IntelligentCodeSplitter.instance = new IntelligentCodeSplitter();
    }
    return IntelligentCodeSplitter.instance;
  }

  // Track user behavior for intelligent preloading
  trackRouteVisit(route: string): void {
    this.userBehavior.visitedRoutes.add(route);
    const startTime = Date.now();
    
    return () => {
      const timeSpent = Date.now() - startTime;
      this.userBehavior.timeSpent.set(route, timeSpent);
    };
  }

  trackInteraction(component: string): void {
    const current = this.userBehavior.interactions.get(component) || 0;
    this.userBehavior.interactions.set(component, current + 1);
  }

  // Predict next likely routes based on user behavior
  predictNextRoutes(currentRoute: string): string[] {
    const routePatterns: Record<string, string[]> = {
      '/': ['/dashboard', '/results', '/history'],
      '/dashboard': ['/visualization', '/results', '/db-explorer'],
      '/results': ['/visualization', '/dashboard', '/history'],
      '/visualization': ['/results', '/dashboard'],
      '/history': ['/results', '/templates'],
      '/templates': ['/history', '/suggestions'],
      '/suggestions': ['/templates', '/'],
      '/db-explorer': ['/dashboard', '/results']
    };

    return routePatterns[currentRoute] || [];
  }

  // Preload chunks based on prediction
  async preloadPredictedChunks(currentRoute: string): Promise<void> {
    const predictedRoutes = this.predictNextRoutes(currentRoute);
    
    for (const route of predictedRoutes) {
      if (!this.loadedChunks.has(route) && !this.preloadQueue.has(route)) {
        const preloadPromise = this.preloadChunk(route);
        this.preloadQueue.set(route, preloadPromise);
      }
    }
  }

  private async preloadChunk(route: string): Promise<void> {
    try {
      const chunkMap: Record<string, () => Promise<any>> = {
        '/': () => import('../pages/QueryPage'),
        '/dashboard': () => import('../pages/DashboardPage'),
        '/results': () => import('../pages/ResultsPage'),
        '/visualization': () => import('../pages/VisualizationPage'),
        '/history': () => import('../pages/HistoryPage'),
        '/templates': () => import('../pages/TemplatesPage'),
        '/suggestions': () => import('../pages/SuggestionsPage'),
        '/db-explorer': () => import('../pages/DBExplorerPage')
      };

      const loader = chunkMap[route];
      if (loader) {
        await loader();
        this.loadedChunks.add(route);
        this.preloadQueue.delete(route);
      }
    } catch (error) {
      console.warn(`Failed to preload chunk for route ${route}:`, error);
      this.preloadQueue.delete(route);
    }
  }

  // Get loading statistics
  getStats() {
    return {
      loadedChunks: Array.from(this.loadedChunks),
      preloadQueueSize: this.preloadQueue.size,
      userBehavior: {
        visitedRoutes: Array.from(this.userBehavior.visitedRoutes),
        averageTimeSpent: this.calculateAverageTimeSpent(),
        mostInteractedComponents: this.getMostInteractedComponents()
      }
    };
  }

  private calculateAverageTimeSpent(): number {
    const times = Array.from(this.userBehavior.timeSpent.values());
    return times.length > 0 ? times.reduce((a, b) => a + b, 0) / times.length : 0;
  }

  private getMostInteractedComponents(): Array<{ component: string; interactions: number }> {
    return Array.from(this.userBehavior.interactions.entries())
      .map(([component, interactions]) => ({ component, interactions }))
      .sort((a, b) => b.interactions - a.interactions)
      .slice(0, 5);
  }
}

// Bundle analyzer for runtime optimization
export class RuntimeBundleAnalyzer {
  private static instance: RuntimeBundleAnalyzer;
  private metrics = {
    chunkLoadTimes: new Map<string, number>(),
    chunkSizes: new Map<string, number>(),
    failedLoads: new Set<string>(),
    cacheHits: new Map<string, number>()
  };

  static getInstance(): RuntimeBundleAnalyzer {
    if (!RuntimeBundleAnalyzer.instance) {
      RuntimeBundleAnalyzer.instance = new RuntimeBundleAnalyzer();
    }
    return RuntimeBundleAnalyzer.instance;
  }

  // Track chunk loading performance
  trackChunkLoad(chunkName: string, loadTime: number, size?: number): void {
    this.metrics.chunkLoadTimes.set(chunkName, loadTime);
    if (size) {
      this.metrics.chunkSizes.set(chunkName, size);
    }
  }

  trackChunkFailure(chunkName: string): void {
    this.metrics.failedLoads.add(chunkName);
  }

  trackCacheHit(chunkName: string): void {
    const current = this.metrics.cacheHits.get(chunkName) || 0;
    this.metrics.cacheHits.set(chunkName, current + 1);
  }

  // Get performance recommendations
  getOptimizationRecommendations(): Array<{
    type: 'warning' | 'info' | 'error';
    message: string;
    action?: string;
  }> {
    const recommendations = [];

    // Check for slow loading chunks
    for (const [chunk, loadTime] of this.metrics.chunkLoadTimes) {
      if (loadTime > 3000) {
        recommendations.push({
          type: 'warning' as const,
          message: `Chunk ${chunk} is loading slowly (${loadTime}ms)`,
          action: 'Consider code splitting or optimization'
        });
      }
    }

    // Check for large chunks
    for (const [chunk, size] of this.metrics.chunkSizes) {
      if (size > 500000) { // 500KB
        recommendations.push({
          type: 'warning' as const,
          message: `Chunk ${chunk} is large (${(size / 1024).toFixed(1)}KB)`,
          action: 'Consider further code splitting'
        });
      }
    }

    // Check for failed loads
    if (this.metrics.failedLoads.size > 0) {
      recommendations.push({
        type: 'error' as const,
        message: `${this.metrics.failedLoads.size} chunks failed to load`,
        action: 'Check network connectivity and CDN status'
      });
    }

    // Check cache efficiency
    const totalChunks = this.metrics.chunkLoadTimes.size;
    const totalCacheHits = Array.from(this.metrics.cacheHits.values()).reduce((a, b) => a + b, 0);
    const cacheHitRate = totalChunks > 0 ? (totalCacheHits / totalChunks) * 100 : 0;

    if (cacheHitRate < 50) {
      recommendations.push({
        type: 'info' as const,
        message: `Cache hit rate is low (${cacheHitRate.toFixed(1)}%)`,
        action: 'Consider implementing better caching strategies'
      });
    }

    return recommendations;
  }

  // Export metrics for analysis
  exportMetrics() {
    return {
      chunkLoadTimes: Object.fromEntries(this.metrics.chunkLoadTimes),
      chunkSizes: Object.fromEntries(this.metrics.chunkSizes),
      failedLoads: Array.from(this.metrics.failedLoads),
      cacheHits: Object.fromEntries(this.metrics.cacheHits),
      summary: {
        totalChunks: this.metrics.chunkLoadTimes.size,
        averageLoadTime: this.calculateAverageLoadTime(),
        totalFailures: this.metrics.failedLoads.size,
        cacheHitRate: this.calculateCacheHitRate()
      }
    };
  }

  private calculateAverageLoadTime(): number {
    const times = Array.from(this.metrics.chunkLoadTimes.values());
    return times.length > 0 ? times.reduce((a, b) => a + b, 0) / times.length : 0;
  }

  private calculateCacheHitRate(): number {
    const totalChunks = this.metrics.chunkLoadTimes.size;
    const totalCacheHits = Array.from(this.metrics.cacheHits.values()).reduce((a, b) => a + b, 0);
    return totalChunks > 0 ? (totalCacheHits / totalChunks) * 100 : 0;
  }
}

// Initialize optimization utilities
export const initializeBundleOptimization = () => {
  // Preload critical resources
  if (typeof window !== 'undefined') {
    preloadCriticalResources();
    
    // Initialize intelligent code splitter
    const splitter = IntelligentCodeSplitter.getInstance();
    const analyzer = RuntimeBundleAnalyzer.getInstance();

    // Track initial route
    const currentRoute = window.location.pathname;
    splitter.trackRouteVisit(currentRoute);
    splitter.preloadPredictedChunks(currentRoute);

    return { splitter, analyzer };
  }

  return null;
};
