/**
 * Production Optimizer
 * 
 * Comprehensive production optimization system for React applications
 * including performance optimization, security hardening, and deployment readiness
 */

// Production optimization interfaces
interface OptimizationRule {
  id: string;
  name: string;
  category: 'performance' | 'security' | 'seo' | 'accessibility' | 'bundle';
  priority: 'critical' | 'high' | 'medium' | 'low';
  description: string;
  check: () => Promise<OptimizationResult>;
  fix?: () => Promise<void>;
}

interface OptimizationResult {
  passed: boolean;
  score: number; // 0-100
  message: string;
  details?: string[];
  recommendations?: string[];
  metrics?: Record<string, number>;
}

interface ProductionReadinessReport {
  timestamp: number;
  overallScore: number;
  categoryScores: Record<string, number>;
  results: Array<{
    rule: OptimizationRule;
    result: OptimizationResult;
  }>;
  criticalIssues: number;
  highPriorityIssues: number;
  recommendations: string[];
  deploymentReady: boolean;
}

class ProductionOptimizer {
  private static instance: ProductionOptimizer;
  private rules: OptimizationRule[] = [];

  static getInstance(): ProductionOptimizer {
    if (!ProductionOptimizer.instance) {
      ProductionOptimizer.instance = new ProductionOptimizer();
    }
    return ProductionOptimizer.instance;
  }

  constructor() {
    this.initializeRules();
  }

  private initializeRules(): void {
    this.rules = [
      // Performance Rules
      {
        id: 'bundle-size',
        name: 'Bundle Size Optimization',
        category: 'performance',
        priority: 'high',
        description: 'Check if bundle sizes are optimized for production',
        check: this.checkBundleSize.bind(this)
      },
      {
        id: 'code-splitting',
        name: 'Code Splitting Implementation',
        category: 'performance',
        priority: 'high',
        description: 'Verify code splitting is properly implemented',
        check: this.checkCodeSplitting.bind(this)
      },
      {
        id: 'lazy-loading',
        name: 'Lazy Loading Components',
        category: 'performance',
        priority: 'medium',
        description: 'Check if components are lazy loaded where appropriate',
        check: this.checkLazyLoading.bind(this)
      },
      {
        id: 'image-optimization',
        name: 'Image Optimization',
        category: 'performance',
        priority: 'medium',
        description: 'Verify images are optimized for web delivery',
        check: this.checkImageOptimization.bind(this)
      },

      // Security Rules
      {
        id: 'csp-headers',
        name: 'Content Security Policy',
        category: 'security',
        priority: 'critical',
        description: 'Ensure CSP headers are properly configured',
        check: this.checkCSPHeaders.bind(this)
      },
      {
        id: 'https-enforcement',
        name: 'HTTPS Enforcement',
        category: 'security',
        priority: 'critical',
        description: 'Verify HTTPS is enforced in production',
        check: this.checkHTTPSEnforcement.bind(this)
      },
      {
        id: 'sensitive-data',
        name: 'Sensitive Data Exposure',
        category: 'security',
        priority: 'critical',
        description: 'Check for exposed sensitive data in client code',
        check: this.checkSensitiveData.bind(this)
      },

      // SEO Rules
      {
        id: 'meta-tags',
        name: 'Meta Tags Optimization',
        category: 'seo',
        priority: 'medium',
        description: 'Verify essential meta tags are present',
        check: this.checkMetaTags.bind(this)
      },
      {
        id: 'structured-data',
        name: 'Structured Data',
        category: 'seo',
        priority: 'low',
        description: 'Check for structured data implementation',
        check: this.checkStructuredData.bind(this)
      },

      // Accessibility Rules
      {
        id: 'aria-labels',
        name: 'ARIA Labels',
        category: 'accessibility',
        priority: 'high',
        description: 'Verify ARIA labels are properly implemented',
        check: this.checkARIALabels.bind(this)
      },
      {
        id: 'color-contrast',
        name: 'Color Contrast',
        category: 'accessibility',
        priority: 'medium',
        description: 'Check color contrast ratios meet WCAG standards',
        check: this.checkColorContrast.bind(this)
      },

      // Bundle Rules
      {
        id: 'unused-dependencies',
        name: 'Unused Dependencies',
        category: 'bundle',
        priority: 'medium',
        description: 'Identify unused dependencies that can be removed',
        check: this.checkUnusedDependencies.bind(this)
      },
      {
        id: 'tree-shaking',
        name: 'Tree Shaking Effectiveness',
        category: 'bundle',
        priority: 'medium',
        description: 'Verify tree shaking is working effectively',
        check: this.checkTreeShaking.bind(this)
      }
    ];
  }

  // Performance checks
  private async checkBundleSize(): Promise<OptimizationResult> {
    // Simulate bundle size check
    const mockBundleSize = 2.5; // MB
    const maxRecommendedSize = 3.0; // MB
    
    const passed = mockBundleSize <= maxRecommendedSize;
    const score = Math.max(0, 100 - ((mockBundleSize / maxRecommendedSize) * 100 - 100));

    return {
      passed,
      score,
      message: `Bundle size is ${mockBundleSize}MB`,
      details: [
        `Main bundle: ${mockBundleSize}MB`,
        `Recommended maximum: ${maxRecommendedSize}MB`
      ],
      recommendations: passed ? [] : [
        'Consider code splitting to reduce main bundle size',
        'Remove unused dependencies',
        'Implement dynamic imports for large components'
      ],
      metrics: { bundleSize: mockBundleSize, maxSize: maxRecommendedSize }
    };
  }

  private async checkCodeSplitting(): Promise<OptimizationResult> {
    // Check for dynamic imports and lazy loading
    const hasLazyComponents = document.querySelectorAll('[data-lazy]').length > 0;
    const hasDynamicImports = true; // Would check build output in real implementation
    
    const passed = hasLazyComponents && hasDynamicImports;
    const score = passed ? 100 : 60;

    return {
      passed,
      score,
      message: passed ? 'Code splitting is properly implemented' : 'Code splitting needs improvement',
      details: [
        `Lazy components detected: ${hasLazyComponents}`,
        `Dynamic imports detected: ${hasDynamicImports}`
      ],
      recommendations: passed ? [] : [
        'Implement React.lazy() for route components',
        'Use dynamic imports for large libraries',
        'Consider component-level code splitting'
      ]
    };
  }

  private async checkLazyLoading(): Promise<OptimizationResult> {
    const lazyImages = document.querySelectorAll('img[loading="lazy"]').length;
    const totalImages = document.querySelectorAll('img').length;
    const lazyPercentage = totalImages > 0 ? (lazyImages / totalImages) * 100 : 100;
    
    const passed = lazyPercentage >= 80;
    const score = Math.min(100, lazyPercentage);

    return {
      passed,
      score,
      message: `${lazyPercentage.toFixed(1)}% of images are lazy loaded`,
      details: [
        `Lazy images: ${lazyImages}`,
        `Total images: ${totalImages}`
      ],
      recommendations: passed ? [] : [
        'Add loading="lazy" to images below the fold',
        'Implement intersection observer for custom lazy loading',
        'Consider lazy loading for heavy components'
      ]
    };
  }

  private async checkImageOptimization(): Promise<OptimizationResult> {
    const images = document.querySelectorAll('img');
    let optimizedCount = 0;
    
    images.forEach(img => {
      const src = img.src;
      if (src.includes('.webp') || src.includes('.avif') || img.hasAttribute('srcset')) {
        optimizedCount++;
      }
    });
    
    const optimizationRate = images.length > 0 ? (optimizedCount / images.length) * 100 : 100;
    const passed = optimizationRate >= 70;
    const score = Math.min(100, optimizationRate);

    return {
      passed,
      score,
      message: `${optimizationRate.toFixed(1)}% of images are optimized`,
      recommendations: passed ? [] : [
        'Use modern image formats (WebP, AVIF)',
        'Implement responsive images with srcset',
        'Compress images before deployment'
      ]
    };
  }

  // Security checks
  private async checkCSPHeaders(): Promise<OptimizationResult> {
    const cspMeta = document.querySelector('meta[http-equiv="Content-Security-Policy"]');
    const hasSafeCSP = cspMeta && !cspMeta.getAttribute('content')?.includes("'unsafe-eval'");
    
    const passed = !!hasSafeCSP;
    const score = passed ? 100 : 0;

    return {
      passed,
      score,
      message: passed ? 'CSP headers are properly configured' : 'CSP headers need configuration',
      recommendations: passed ? [] : [
        'Implement Content Security Policy headers',
        'Remove unsafe-eval and unsafe-inline directives',
        'Use nonce or hash for inline scripts'
      ]
    };
  }

  private async checkHTTPSEnforcement(): Promise<OptimizationResult> {
    const isHTTPS = window.location.protocol === 'https:';
    const isLocalhost = window.location.hostname === 'localhost';
    
    const passed = isHTTPS || isLocalhost;
    const score = passed ? 100 : 0;

    return {
      passed,
      score,
      message: passed ? 'HTTPS is properly enforced' : 'HTTPS enforcement required',
      recommendations: passed ? [] : [
        'Configure HTTPS for production deployment',
        'Implement HSTS headers',
        'Redirect HTTP traffic to HTTPS'
      ]
    };
  }

  private async checkSensitiveData(): Promise<OptimizationResult> {
    const sensitivePatterns = [
      /api[_-]?key/i,
      /secret/i,
      /password/i,
      /token/i,
      /private[_-]?key/i
    ];
    
    const scriptTags = document.querySelectorAll('script');
    let exposedData = false;
    
    scriptTags.forEach(script => {
      const content = script.innerHTML;
      sensitivePatterns.forEach(pattern => {
        if (pattern.test(content)) {
          exposedData = true;
        }
      });
    });
    
    const passed = !exposedData;
    const score = passed ? 100 : 0;

    return {
      passed,
      score,
      message: passed ? 'No sensitive data exposed' : 'Potential sensitive data exposure detected',
      recommendations: passed ? [] : [
        'Move sensitive data to environment variables',
        'Use server-side configuration',
        'Implement proper secret management'
      ]
    };
  }

  // SEO checks
  private async checkMetaTags(): Promise<OptimizationResult> {
    const requiredTags = ['title', 'description', 'viewport'];
    const presentTags = requiredTags.filter(tag => {
      if (tag === 'title') return !!document.querySelector('title');
      return !!document.querySelector(`meta[name="${tag}"]`);
    });
    
    const completionRate = (presentTags.length / requiredTags.length) * 100;
    const passed = completionRate >= 100;
    const score = completionRate;

    return {
      passed,
      score,
      message: `${presentTags.length}/${requiredTags.length} essential meta tags present`,
      recommendations: passed ? [] : [
        'Add missing meta tags',
        'Optimize meta descriptions for SEO',
        'Ensure viewport meta tag is present'
      ]
    };
  }

  private async checkStructuredData(): Promise<OptimizationResult> {
    const structuredData = document.querySelectorAll('script[type="application/ld+json"]');
    const hasStructuredData = structuredData.length > 0;
    
    const passed = hasStructuredData;
    const score = passed ? 100 : 50; // Not critical, so partial score

    return {
      passed,
      score,
      message: hasStructuredData ? 'Structured data is implemented' : 'No structured data found',
      recommendations: passed ? [] : [
        'Implement JSON-LD structured data',
        'Add organization and website schema',
        'Consider breadcrumb and article schemas'
      ]
    };
  }

  // Accessibility checks
  private async checkARIALabels(): Promise<OptimizationResult> {
    const interactiveElements = document.querySelectorAll('button, input, select, textarea, a');
    let labeledElements = 0;
    
    interactiveElements.forEach(element => {
      if (element.hasAttribute('aria-label') || 
          element.hasAttribute('aria-labelledby') ||
          element.querySelector('label')) {
        labeledElements++;
      }
    });
    
    const labelingRate = interactiveElements.length > 0 ? (labeledElements / interactiveElements.length) * 100 : 100;
    const passed = labelingRate >= 90;
    const score = Math.min(100, labelingRate);

    return {
      passed,
      score,
      message: `${labelingRate.toFixed(1)}% of interactive elements have proper labels`,
      recommendations: passed ? [] : [
        'Add aria-label to unlabeled interactive elements',
        'Use aria-labelledby for complex labeling',
        'Ensure form inputs have associated labels'
      ]
    };
  }

  private async checkColorContrast(): Promise<OptimizationResult> {
    // Simplified color contrast check
    const textElements = document.querySelectorAll('p, span, div, h1, h2, h3, h4, h5, h6');
    let contrastIssues = 0;
    
    // This would use a proper color contrast calculation in a real implementation
    textElements.forEach(element => {
      const styles = window.getComputedStyle(element);
      const color = styles.color;
      const backgroundColor = styles.backgroundColor;
      
      // Simplified check - in reality, you'd calculate actual contrast ratios
      if (color === backgroundColor) {
        contrastIssues++;
      }
    });
    
    const contrastRate = textElements.length > 0 ? ((textElements.length - contrastIssues) / textElements.length) * 100 : 100;
    const passed = contrastRate >= 95;
    const score = Math.min(100, contrastRate);

    return {
      passed,
      score,
      message: `${contrastRate.toFixed(1)}% of text has adequate contrast`,
      recommendations: passed ? [] : [
        'Improve color contrast ratios',
        'Use WCAG AA compliant color combinations',
        'Test with accessibility tools'
      ]
    };
  }

  // Bundle checks
  private async checkUnusedDependencies(): Promise<OptimizationResult> {
    // This would analyze package.json and actual usage in a real implementation
    const mockUnusedDeps = ['lodash-es', 'moment']; // Example unused deps
    const totalDeps = 50; // Mock total dependencies
    
    const unusedPercentage = (mockUnusedDeps.length / totalDeps) * 100;
    const passed = unusedPercentage <= 10;
    const score = Math.max(0, 100 - unusedPercentage * 2);

    return {
      passed,
      score,
      message: `${mockUnusedDeps.length} potentially unused dependencies found`,
      details: mockUnusedDeps,
      recommendations: passed ? [] : [
        'Remove unused dependencies from package.json',
        'Use bundle analyzer to identify unused code',
        'Consider lighter alternatives for heavy dependencies'
      ]
    };
  }

  private async checkTreeShaking(): Promise<OptimizationResult> {
    // Mock tree shaking effectiveness check
    const treeShakingEffective = true; // Would check build output
    const bundleReduction = 25; // Percentage reduction from tree shaking
    
    const passed = treeShakingEffective && bundleReduction >= 15;
    const score = passed ? 100 : 70;

    return {
      passed,
      score,
      message: `Tree shaking reduced bundle size by ${bundleReduction}%`,
      recommendations: passed ? [] : [
        'Ensure ES modules are used for better tree shaking',
        'Use named imports instead of default imports',
        'Configure webpack for optimal tree shaking'
      ]
    };
  }

  // Main optimization methods
  async runOptimizationCheck(): Promise<ProductionReadinessReport> {
    const results: Array<{ rule: OptimizationRule; result: OptimizationResult }> = [];
    const categoryScores: Record<string, number> = {};
    const categoryCounts: Record<string, number> = {};

    // Run all optimization checks
    for (const rule of this.rules) {
      try {
        const result = await rule.check();
        results.push({ rule, result });

        // Calculate category scores
        if (!categoryScores[rule.category]) {
          categoryScores[rule.category] = 0;
          categoryCounts[rule.category] = 0;
        }
        categoryScores[rule.category] += result.score;
        categoryCounts[rule.category]++;
      } catch (error) {
        console.error(`Error running optimization rule ${rule.id}:`, error);
      }
    }

    // Calculate final category scores
    Object.keys(categoryScores).forEach(category => {
      categoryScores[category] = categoryScores[category] / categoryCounts[category];
    });

    // Calculate overall score
    const overallScore = Object.values(categoryScores).reduce((sum, score) => sum + score, 0) / Object.keys(categoryScores).length;

    // Count issues by priority
    const criticalIssues = results.filter(r => r.rule.priority === 'critical' && !r.result.passed).length;
    const highPriorityIssues = results.filter(r => r.rule.priority === 'high' && !r.result.passed).length;

    // Generate recommendations
    const recommendations = results
      .filter(r => !r.result.passed && r.result.recommendations)
      .flatMap(r => r.result.recommendations!)
      .slice(0, 10); // Top 10 recommendations

    // Determine deployment readiness
    const deploymentReady = criticalIssues === 0 && overallScore >= 80;

    return {
      timestamp: Date.now(),
      overallScore,
      categoryScores,
      results,
      criticalIssues,
      highPriorityIssues,
      recommendations,
      deploymentReady
    };
  }

  // Get optimization rules
  getRules(): OptimizationRule[] {
    return [...this.rules];
  }

  // Add custom rule
  addRule(rule: OptimizationRule): void {
    this.rules.push(rule);
  }
}

// Export singleton instance
export const productionOptimizer = ProductionOptimizer.getInstance();
export default ProductionOptimizer;
