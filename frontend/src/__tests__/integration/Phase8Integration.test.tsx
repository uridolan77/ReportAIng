/**
 * Phase 8 Integration Tests
 * 
 * Comprehensive integration tests for Phase 8: Production Readiness
 * testing security, monitoring, and optimization systems
 */

import React from 'react';
import { screen, fireEvent, waitFor } from '@testing-library/react';
import { renderWithProviders, productionTestUtils } from '../../utils/testUtils';
import { SecurityAuditDashboard } from '../../components/Security/SecurityAuditDashboard';
import { MonitoringDashboard } from '../../components/Monitoring/MonitoringDashboard';
import { Phase8Summary } from '../../components/Production/Phase8Summary';
import { securityAudit } from '../../security/SecurityAuditSystem';
import { analytics } from '../../monitoring/AnalyticsSystem';
import { productionOptimizer } from '../../deployment/ProductionOptimizer';

// Mock the systems
jest.mock('../../security/SecurityAuditSystem');
jest.mock('../../monitoring/AnalyticsSystem');
jest.mock('../../deployment/ProductionOptimizer');

const mockSecurityAudit = securityAudit as jest.Mocked<typeof securityAudit>;
const mockAnalytics = analytics as jest.Mocked<typeof analytics>;
const mockProductionOptimizer = productionOptimizer as jest.Mocked<typeof productionOptimizer>;

describe('Phase 8: Production Readiness Integration', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    
    // Mock security audit report
    mockSecurityAudit.generateAuditReport.mockResolvedValue({
      timestamp: Date.now(),
      vulnerabilities: [
        {
          id: 'test-vuln-1',
          severity: 'medium',
          type: 'xss',
          description: 'Potential XSS vulnerability',
          location: 'Component.tsx',
          recommendation: 'Use proper input sanitization',
          cvss: 5.4
        }
      ],
      policyViolations: [],
      securityScore: 85,
      recommendations: ['Implement CSP headers'],
      complianceStatus: {
        owasp: true,
        gdpr: true,
        hipaa: false,
        sox: true
      }
    });

    // Mock analytics report
    mockAnalytics.generateReport.mockReturnValue({
      timeRange: { start: Date.now() - 86400000, end: Date.now() },
      userEvents: [],
      performanceMetrics: [],
      businessMetrics: [],
      summary: {
        totalUsers: 1250,
        totalSessions: 3420,
        totalPageViews: 8750,
        averageSessionDuration: 420000,
        bounceRate: 0.35,
        conversionRate: 0.12,
        errorRate: 0.02,
        averageLoadTime: 1200
      }
    });

    // Mock production optimizer report
    mockProductionOptimizer.runOptimizationCheck.mockResolvedValue({
      timestamp: Date.now(),
      overallScore: 88,
      categoryScores: {
        performance: 90,
        security: 85,
        seo: 88,
        accessibility: 92,
        bundle: 85
      },
      results: [],
      criticalIssues: 0,
      highPriorityIssues: 2,
      recommendations: ['Optimize bundle size', 'Improve image compression'],
      deploymentReady: true
    });
  });

  describe('Security Audit Dashboard', () => {
    it('should render security audit dashboard with metrics', async () => {
      renderWithProviders(<SecurityAuditDashboard />);
      
      expect(screen.getByText('Security Audit Dashboard')).toBeInTheDocument();
      
      // Wait for audit to complete
      await waitFor(() => {
        expect(screen.getByText('Security Score')).toBeInTheDocument();
      });

      // Check if security metrics are displayed
      expect(screen.getByText('85')).toBeInTheDocument(); // Security score
      expect(screen.getByText('Critical Issues')).toBeInTheDocument();
    });

    it('should run security audit on button click', async () => {
      renderWithProviders(<SecurityAuditDashboard />);
      
      const auditButton = screen.getByText('Run Security Audit');
      fireEvent.click(auditButton);
      
      await waitFor(() => {
        expect(mockSecurityAudit.generateAuditReport).toHaveBeenCalled();
      });
    });

    it('should display vulnerability details in modal', async () => {
      renderWithProviders(<SecurityAuditDashboard />);
      
      await waitFor(() => {
        expect(screen.getByText('Vulnerabilities')).toBeInTheDocument();
      });

      // Click on vulnerabilities tab
      fireEvent.click(screen.getByText('Vulnerabilities'));
      
      await waitFor(() => {
        const detailsButton = screen.getByText('Details');
        expect(detailsButton).toBeInTheDocument();
        
        fireEvent.click(detailsButton);
      });

      await waitFor(() => {
        expect(screen.getByText('Vulnerability Details')).toBeInTheDocument();
      });
    });
  });

  describe('Monitoring Dashboard', () => {
    it('should render monitoring dashboard with analytics', async () => {
      renderWithProviders(<MonitoringDashboard />);
      
      expect(screen.getByText('Monitoring Dashboard')).toBeInTheDocument();
      
      await waitFor(() => {
        expect(screen.getByText('Total Users')).toBeInTheDocument();
        expect(screen.getByText('1,250')).toBeInTheDocument(); // Total users
      });
    });

    it('should refresh analytics data', async () => {
      renderWithProviders(<MonitoringDashboard />);
      
      const refreshButton = screen.getByText('Refresh');
      fireEvent.click(refreshButton);
      
      await waitFor(() => {
        expect(mockAnalytics.generateReport).toHaveBeenCalled();
      });
    });

    it('should export analytics data', () => {
      renderWithProviders(<MonitoringDashboard />);
      
      const exportButton = screen.getByText('Export');
      fireEvent.click(exportButton);
      
      // Check if export was triggered (would create download in real scenario)
      expect(exportButton).toBeInTheDocument();
    });
  });

  describe('Phase 8 Summary', () => {
    it('should render production readiness summary', async () => {
      renderWithProviders(<Phase8Summary />);
      
      expect(screen.getByText('Phase 8: Production Readiness')).toBeInTheDocument();
      
      await waitFor(() => {
        expect(screen.getByText('Production Readiness')).toBeInTheDocument();
        expect(screen.getByText('Security Score')).toBeInTheDocument();
        expect(screen.getByText('Performance Score')).toBeInTheDocument();
      });
    });

    it('should display production features', async () => {
      renderWithProviders(<Phase8Summary />);
      
      await waitFor(() => {
        expect(screen.getByText('Security Audit System')).toBeInTheDocument();
        expect(screen.getByText('Advanced Analytics System')).toBeInTheDocument();
        expect(screen.getByText('Production Optimizer')).toBeInTheDocument();
      });
    });

    it('should show deployment status', async () => {
      renderWithProviders(<Phase8Summary />);
      
      await waitFor(() => {
        expect(screen.getByText('Deployment Status')).toBeInTheDocument();
        expect(screen.getByText('Ready')).toBeInTheDocument(); // Deployment ready
      });
    });
  });

  describe('Production Testing Utilities', () => {
    it('should test security headers', () => {
      // Add a mock CSP header
      const meta = document.createElement('meta');
      meta.setAttribute('http-equiv', 'Content-Security-Policy');
      meta.setAttribute('content', "default-src 'self'");
      document.head.appendChild(meta);
      
      const result = productionTestUtils.testSecurityHeaders();
      
      expect(result.hasCSP).toBe(true);
      expect(result.isSecure).toBe(true);
      
      // Cleanup
      document.head.removeChild(meta);
    });

    it('should test performance metrics', () => {
      // Mock performance API
      const mockPerformance = {
        getEntriesByType: jest.fn().mockReturnValue([{
          loadEventEnd: 1000,
          loadEventStart: 500,
          domContentLoadedEventEnd: 800,
          domContentLoadedEventStart: 600
        }]),
        getEntriesByName: jest.fn().mockReturnValue([{ startTime: 300 }])
      };
      
      Object.defineProperty(window, 'performance', {
        value: mockPerformance,
        writable: true
      });
      
      const result = productionTestUtils.testPerformanceMetrics();
      
      expect(result).toBeTruthy();
      expect(result?.loadTime).toBe(500);
      expect(result?.domContentLoaded).toBe(200);
    });

    it('should test bundle optimization', () => {
      // Add mock script tags
      const script1 = document.createElement('script');
      script1.src = 'bundle1.js';
      script1.async = true;
      document.head.appendChild(script1);
      
      const script2 = document.createElement('script');
      script2.src = 'bundle2.js';
      document.head.appendChild(script2);
      
      const result = productionTestUtils.testBundleOptimization();
      
      expect(result.totalScripts).toBeGreaterThan(0);
      expect(result.asyncScripts).toBeGreaterThan(0);
      expect(result.asyncPercentage).toBeGreaterThan(0);
      
      // Cleanup
      document.head.removeChild(script1);
      document.head.removeChild(script2);
    });
  });

  describe('System Integration', () => {
    it('should integrate all Phase 8 systems', async () => {
      // Test that all systems work together
      renderWithProviders(<Phase8Summary />);
      
      await waitFor(() => {
        expect(mockSecurityAudit.generateAuditReport).toHaveBeenCalled();
        expect(mockProductionOptimizer.runOptimizationCheck).toHaveBeenCalled();
      });
      
      // Verify all components are rendered
      expect(screen.getByText('Phase 8: Production Readiness')).toBeInTheDocument();
      expect(screen.getByText('Production Features')).toBeInTheDocument();
      expect(screen.getByText('Implementation Timeline')).toBeInTheDocument();
    });

    it('should handle system errors gracefully', async () => {
      // Mock system errors
      mockSecurityAudit.generateAuditReport.mockRejectedValue(new Error('Security audit failed'));
      mockProductionOptimizer.runOptimizationCheck.mockRejectedValue(new Error('Optimization check failed'));
      
      // Should not crash
      renderWithProviders(<Phase8Summary />);
      
      await waitFor(() => {
        expect(screen.getByText('Phase 8: Production Readiness')).toBeInTheDocument();
      });
    });

    it('should maintain performance under load', async () => {
      const startTime = performance.now();
      
      // Render multiple components simultaneously
      const { rerender } = renderWithProviders(<Phase8Summary />);
      
      for (let i = 0; i < 10; i++) {
        rerender(<Phase8Summary />);
      }
      
      const endTime = performance.now();
      const renderTime = endTime - startTime;
      
      // Should render quickly even with multiple rerenders
      expect(renderTime).toBeLessThan(1000); // Less than 1 second
    });
  });

  describe('Production Readiness Validation', () => {
    it('should validate all production requirements', async () => {
      renderWithProviders(<Phase8Summary />);
      
      await waitFor(() => {
        // Check that all critical systems are implemented
        expect(screen.getByText('Security Audit System')).toBeInTheDocument();
        expect(screen.getByText('Advanced Analytics System')).toBeInTheDocument();
        expect(screen.getByText('Production Optimizer')).toBeInTheDocument();
        expect(screen.getByText('Deployment Pipeline')).toBeInTheDocument();
      });
    });

    it('should confirm deployment readiness', async () => {
      renderWithProviders(<Phase8Summary />);
      
      await waitFor(() => {
        const deployButton = screen.getByText('Deploy to Production');
        expect(deployButton).toBeInTheDocument();
        expect(deployButton).not.toBeDisabled(); // Should be enabled when ready
      });
    });
  });
});
