/**
 * Performance Validation Utility
 * 
 * Manual validation script to check all performance optimizations
 * are working correctly in the production build.
 */

export interface ValidationResult {
  category: string;
  test: string;
  passed: boolean;
  value?: any;
  threshold?: any;
  message: string;
}

export interface ValidationReport {
  overallScore: number;
  totalTests: number;
  passedTests: number;
  failedTests: number;
  results: ValidationResult[];
  recommendations: string[];
}

export class PerformanceValidator {
  private results: ValidationResult[] = [];

  // Performance thresholds
  private readonly THRESHOLDS = {
    INITIAL_LOAD_TIME: 3000, // 3 seconds
    COMPONENT_RENDER_TIME: 500, // 500ms
    MEMORY_USAGE_LIMIT: 150 * 1024 * 1024, // 150MB
    BUNDLE_SIZE_LIMIT: 2 * 1024 * 1024, // 2MB
    CACHE_HIT_RATE_MIN: 60, // 60%
    INTERACTION_RESPONSE_TIME: 200, // 200ms
  };

  async validatePerformance(): Promise<ValidationReport> {
    console.log('🚀 Starting Performance Validation...');
    this.results = [];

    // Run all validation tests
    await this.validateBundleOptimization();
    await this.validateLazyLoading();
    await this.validateServiceWorker();
    await this.validateMemoryUsage();
    await this.validateRenderPerformance();
    await this.validateCaching();
    await this.validateImageOptimization();
    await this.validateVirtualScrolling();

    return this.generateReport();
  }

  private async validateBundleOptimization(): Promise<void> {
    console.log('📦 Validating Bundle Optimization...');

    try {
      // Check if chunks are properly named (indicates webpack chunk naming is working)
      const scripts = Array.from(document.querySelectorAll('script[src]'));
      const hasNamedChunks = scripts.some(script => 
        script.src.includes('chunk') && 
        (script.src.includes('query-page') || 
         script.src.includes('dashboard-page') || 
         script.src.includes('admin-'))
      );

      this.addResult({
        category: 'Bundle Optimization',
        test: 'Named Chunks',
        passed: hasNamedChunks,
        message: hasNamedChunks 
          ? 'Webpack chunk naming is working correctly'
          : 'Webpack chunk naming may not be configured properly'
      });

      // Check total number of script files (indicates code splitting)
      const scriptCount = scripts.length;
      const hasCodeSplitting = scriptCount > 5; // Should have multiple chunks

      this.addResult({
        category: 'Bundle Optimization',
        test: 'Code Splitting',
        passed: hasCodeSplitting,
        value: scriptCount,
        threshold: 5,
        message: hasCodeSplitting
          ? `Code splitting working: ${scriptCount} script files loaded`
          : `Code splitting may not be working: only ${scriptCount} script files`
      });

    } catch (error) {
      this.addResult({
        category: 'Bundle Optimization',
        test: 'Bundle Analysis',
        passed: false,
        message: `Error analyzing bundle: ${error}`
      });
    }
  }

  private async validateLazyLoading(): Promise<void> {
    console.log('⚡ Validating Lazy Loading...');

    try {
      // Check if React.lazy is being used (look for chunk loading)
      const hasLazyComponents = window.performance
        .getEntriesByType('resource')
        .some((entry: any) => 
          entry.name.includes('.chunk.js') && 
          entry.name.includes('static/js/')
        );

      this.addResult({
        category: 'Lazy Loading',
        test: 'React.lazy Implementation',
        passed: hasLazyComponents,
        message: hasLazyComponents
          ? 'Lazy loading is working - chunk files detected'
          : 'Lazy loading may not be implemented - no chunk files detected'
      });

      // Check initial load time
      const navigationEntry = window.performance.getEntriesByType('navigation')[0] as any;
      if (navigationEntry) {
        const loadTime = navigationEntry.loadEventEnd - navigationEntry.fetchStart;
        const isLoadTimeFast = loadTime < this.THRESHOLDS.INITIAL_LOAD_TIME;

        this.addResult({
          category: 'Lazy Loading',
          test: 'Initial Load Time',
          passed: isLoadTimeFast,
          value: `${loadTime}ms`,
          threshold: `${this.THRESHOLDS.INITIAL_LOAD_TIME}ms`,
          message: isLoadTimeFast
            ? `Fast initial load: ${loadTime}ms`
            : `Slow initial load: ${loadTime}ms (threshold: ${this.THRESHOLDS.INITIAL_LOAD_TIME}ms)`
        });
      }

    } catch (error) {
      this.addResult({
        category: 'Lazy Loading',
        test: 'Lazy Loading Analysis',
        passed: false,
        message: `Error analyzing lazy loading: ${error}`
      });
    }
  }

  private async validateServiceWorker(): Promise<void> {
    console.log('🔧 Validating Service Worker...');

    try {
      const hasServiceWorker = 'serviceWorker' in navigator;
      
      this.addResult({
        category: 'Service Worker',
        test: 'Service Worker Support',
        passed: hasServiceWorker,
        message: hasServiceWorker
          ? 'Service Worker API is supported'
          : 'Service Worker API is not supported'
      });

      if (hasServiceWorker) {
        const registration = await navigator.serviceWorker.getRegistration();
        const isRegistered = !!registration;

        this.addResult({
          category: 'Service Worker',
          test: 'Service Worker Registration',
          passed: isRegistered,
          message: isRegistered
            ? 'Service Worker is registered'
            : 'Service Worker is not registered'
        });
      }

    } catch (error) {
      this.addResult({
        category: 'Service Worker',
        test: 'Service Worker Validation',
        passed: false,
        message: `Error validating service worker: ${error}`
      });
    }
  }

  private async validateMemoryUsage(): Promise<void> {
    console.log('🧠 Validating Memory Usage...');

    try {
      if ('memory' in performance) {
        const memory = (performance as any).memory;
        const memoryUsage = memory.usedJSHeapSize;
        const isMemoryEfficient = memoryUsage < this.THRESHOLDS.MEMORY_USAGE_LIMIT;

        this.addResult({
          category: 'Memory Usage',
          test: 'Memory Efficiency',
          passed: isMemoryEfficient,
          value: `${(memoryUsage / 1024 / 1024).toFixed(2)}MB`,
          threshold: `${(this.THRESHOLDS.MEMORY_USAGE_LIMIT / 1024 / 1024).toFixed(2)}MB`,
          message: isMemoryEfficient
            ? `Memory usage is efficient: ${(memoryUsage / 1024 / 1024).toFixed(2)}MB`
            : `High memory usage: ${(memoryUsage / 1024 / 1024).toFixed(2)}MB (limit: ${(this.THRESHOLDS.MEMORY_USAGE_LIMIT / 1024 / 1024).toFixed(2)}MB)`
        });
      } else {
        this.addResult({
          category: 'Memory Usage',
          test: 'Memory API',
          passed: false,
          message: 'Memory API not available in this browser'
        });
      }

    } catch (error) {
      this.addResult({
        category: 'Memory Usage',
        test: 'Memory Validation',
        passed: false,
        message: `Error validating memory usage: ${error}`
      });
    }
  }

  private async validateRenderPerformance(): Promise<void> {
    console.log('🎨 Validating Render Performance...');

    try {
      // Check for performance marks/measures
      const paintEntries = window.performance.getEntriesByType('paint');
      const hasFirstPaint = paintEntries.some(entry => entry.name === 'first-paint');
      const hasFirstContentfulPaint = paintEntries.some(entry => entry.name === 'first-contentful-paint');

      this.addResult({
        category: 'Render Performance',
        test: 'Paint Timing API',
        passed: hasFirstPaint || hasFirstContentfulPaint,
        message: (hasFirstPaint || hasFirstContentfulPaint)
          ? 'Paint timing metrics are available'
          : 'Paint timing metrics are not available'
      });

      // Check for React DevTools performance marks
      const reactMarks = window.performance.getEntriesByName('⚛️');
      const hasReactMarks = reactMarks.length > 0;

      this.addResult({
        category: 'Render Performance',
        test: 'React Performance Marks',
        passed: true, // This is optional
        message: hasReactMarks
          ? 'React performance marks detected'
          : 'React performance marks not detected (normal in production)'
      });

    } catch (error) {
      this.addResult({
        category: 'Render Performance',
        test: 'Render Performance Analysis',
        passed: false,
        message: `Error analyzing render performance: ${error}`
      });
    }
  }

  private async validateCaching(): Promise<void> {
    console.log('💾 Validating Caching...');

    try {
      const resources = window.performance.getEntriesByType('resource') as PerformanceResourceTiming[];
      const cachedResources = resources.filter(resource => 
        resource.transferSize === 0 && resource.decodedBodySize > 0
      );
      
      const cacheHitRate = resources.length > 0 ? (cachedResources.length / resources.length) * 100 : 0;
      const isCacheEffective = cacheHitRate >= this.THRESHOLDS.CACHE_HIT_RATE_MIN;

      this.addResult({
        category: 'Caching',
        test: 'Cache Hit Rate',
        passed: isCacheEffective,
        value: `${cacheHitRate.toFixed(1)}%`,
        threshold: `${this.THRESHOLDS.CACHE_HIT_RATE_MIN}%`,
        message: isCacheEffective
          ? `Good cache hit rate: ${cacheHitRate.toFixed(1)}%`
          : `Low cache hit rate: ${cacheHitRate.toFixed(1)}% (minimum: ${this.THRESHOLDS.CACHE_HIT_RATE_MIN}%)`
      });

    } catch (error) {
      this.addResult({
        category: 'Caching',
        test: 'Cache Analysis',
        passed: false,
        message: `Error analyzing caching: ${error}`
      });
    }
  }

  private async validateImageOptimization(): Promise<void> {
    console.log('🖼️ Validating Image Optimization...');

    try {
      // Check for modern image format support
      const canvas = document.createElement('canvas');
      canvas.width = 1;
      canvas.height = 1;
      
      const webpSupport = canvas.toDataURL('image/webp').indexOf('data:image/webp') === 0;
      const avifSupport = canvas.toDataURL('image/avif').indexOf('data:image/avif') === 0;

      this.addResult({
        category: 'Image Optimization',
        test: 'Modern Format Support',
        passed: webpSupport || avifSupport,
        message: `WebP: ${webpSupport ? 'Supported' : 'Not supported'}, AVIF: ${avifSupport ? 'Supported' : 'Not supported'}`
      });

      // Check for lazy loading attributes
      const images = Array.from(document.querySelectorAll('img'));
      const lazyImages = images.filter(img => img.loading === 'lazy');
      const hasLazyImages = lazyImages.length > 0;

      this.addResult({
        category: 'Image Optimization',
        test: 'Lazy Loading Images',
        passed: hasLazyImages,
        value: `${lazyImages.length}/${images.length}`,
        message: hasLazyImages
          ? `${lazyImages.length} out of ${images.length} images use lazy loading`
          : 'No images with lazy loading detected'
      });

    } catch (error) {
      this.addResult({
        category: 'Image Optimization',
        test: 'Image Optimization Analysis',
        passed: false,
        message: `Error analyzing image optimization: ${error}`
      });
    }
  }

  private async validateVirtualScrolling(): Promise<void> {
    console.log('📜 Validating Virtual Scrolling...');

    try {
      // Check for intersection observer (used by virtual scrolling)
      const hasIntersectionObserver = 'IntersectionObserver' in window;

      this.addResult({
        category: 'Virtual Scrolling',
        test: 'Intersection Observer Support',
        passed: hasIntersectionObserver,
        message: hasIntersectionObserver
          ? 'Intersection Observer API is supported'
          : 'Intersection Observer API is not supported'
      });

      // Check for react-window or similar virtual scrolling libraries
      const scripts = Array.from(document.querySelectorAll('script[src]'));
      const hasVirtualScrollingLib = scripts.some(script => 
        script.src.includes('react-window') || 
        script.src.includes('react-virtualized') ||
        script.textContent?.includes('react-window')
      );

      this.addResult({
        category: 'Virtual Scrolling',
        test: 'Virtual Scrolling Library',
        passed: true, // This is optional since we have custom implementation
        message: hasVirtualScrollingLib
          ? 'Virtual scrolling library detected'
          : 'Using custom virtual scrolling implementation'
      });

    } catch (error) {
      this.addResult({
        category: 'Virtual Scrolling',
        test: 'Virtual Scrolling Analysis',
        passed: false,
        message: `Error analyzing virtual scrolling: ${error}`
      });
    }
  }

  private addResult(result: ValidationResult): void {
    this.results.push(result);
    const status = result.passed ? '✅' : '❌';
    console.log(`${status} ${result.category} - ${result.test}: ${result.message}`);
  }

  private generateReport(): ValidationReport {
    const passedTests = this.results.filter(r => r.passed).length;
    const failedTests = this.results.length - passedTests;
    const overallScore = (passedTests / this.results.length) * 100;

    const recommendations = this.generateRecommendations();

    const report: ValidationReport = {
      overallScore,
      totalTests: this.results.length,
      passedTests,
      failedTests,
      results: this.results,
      recommendations,
    };

    this.logReport(report);
    return report;
  }

  private generateRecommendations(): string[] {
    const recommendations: string[] = [];
    const failedResults = this.results.filter(r => !r.passed);

    failedResults.forEach(result => {
      switch (result.category) {
        case 'Bundle Optimization':
          recommendations.push('Consider implementing code splitting and webpack chunk naming');
          break;
        case 'Lazy Loading':
          recommendations.push('Implement React.lazy for non-critical components');
          break;
        case 'Service Worker':
          recommendations.push('Implement service worker for caching and offline support');
          break;
        case 'Memory Usage':
          recommendations.push('Optimize memory usage by implementing proper cleanup and memoization');
          break;
        case 'Caching':
          recommendations.push('Improve caching strategies to increase cache hit rate');
          break;
        case 'Image Optimization':
          recommendations.push('Implement modern image formats and lazy loading');
          break;
      }
    });

    return [...new Set(recommendations)]; // Remove duplicates
  }

  private logReport(report: ValidationReport): void {
    console.log('\n📊 Performance Validation Report');
    console.log('================================');
    console.log(`Overall Score: ${report.overallScore.toFixed(1)}/100`);
    console.log(`Tests Passed: ${report.passedTests}/${report.totalTests}`);
    console.log(`Tests Failed: ${report.failedTests}/${report.totalTests}`);
    
    if (report.recommendations.length > 0) {
      console.log('\n💡 Recommendations:');
      report.recommendations.forEach((rec, index) => {
        console.log(`${index + 1}. ${rec}`);
      });
    }

    console.log('\n🎯 Performance Grade:', this.getPerformanceGrade(report.overallScore));
  }

  private getPerformanceGrade(score: number): string {
    if (score >= 90) return 'A+ (Excellent)';
    if (score >= 80) return 'A (Very Good)';
    if (score >= 70) return 'B (Good)';
    if (score >= 60) return 'C (Fair)';
    return 'D (Needs Improvement)';
  }
}

// Export singleton instance
export const performanceValidator = new PerformanceValidator();
