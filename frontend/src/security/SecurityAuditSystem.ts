/**
 * Security Audit System
 * 
 * Comprehensive security audit and monitoring system for React applications
 * including vulnerability scanning, security policy enforcement, and threat detection
 */

// Security audit interfaces
interface SecurityVulnerability {
  id: string;
  severity: 'critical' | 'high' | 'medium' | 'low';
  type: 'xss' | 'csrf' | 'injection' | 'auth' | 'data' | 'network' | 'dependency';
  description: string;
  location: string;
  recommendation: string;
  cwe?: string; // Common Weakness Enumeration
  cvss?: number; // Common Vulnerability Scoring System
}

interface SecurityPolicy {
  name: string;
  enabled: boolean;
  rules: SecurityRule[];
  enforcement: 'strict' | 'warn' | 'log';
}

interface SecurityRule {
  id: string;
  description: string;
  check: () => boolean | Promise<boolean>;
  severity: 'critical' | 'high' | 'medium' | 'low';
}

interface SecurityAuditReport {
  timestamp: number;
  vulnerabilities: SecurityVulnerability[];
  policyViolations: Array<{ policy: string; rule: string; severity: string }>;
  securityScore: number;
  recommendations: string[];
  complianceStatus: {
    owasp: boolean;
    gdpr: boolean;
    hipaa: boolean;
    sox: boolean;
  };
}

class SecurityAuditSystem {
  private static instance: SecurityAuditSystem;
  private vulnerabilities: SecurityVulnerability[] = [];
  private policies: SecurityPolicy[] = [];
  private auditHistory: SecurityAuditReport[] = [];
  private isMonitoring = false;

  static getInstance(): SecurityAuditSystem {
    if (!SecurityAuditSystem.instance) {
      SecurityAuditSystem.instance = new SecurityAuditSystem();
    }
    return SecurityAuditSystem.instance;
  }

  // Initialize security policies
  initializePolicies(): void {
    this.policies = [
      {
        name: 'Content Security Policy',
        enabled: true,
        enforcement: 'strict',
        rules: [
          {
            id: 'csp-script-src',
            description: 'Ensure script-src directive is properly configured',
            check: () => this.checkCSPScriptSrc(),
            severity: 'high'
          },
          {
            id: 'csp-object-src',
            description: 'Ensure object-src is set to none',
            check: () => this.checkCSPObjectSrc(),
            severity: 'medium'
          }
        ]
      },
      {
        name: 'Authentication Security',
        enabled: true,
        enforcement: 'strict',
        rules: [
          {
            id: 'auth-token-storage',
            description: 'Ensure tokens are stored securely',
            check: () => this.checkTokenStorage(),
            severity: 'critical'
          },
          {
            id: 'auth-session-timeout',
            description: 'Ensure session timeout is configured',
            check: () => this.checkSessionTimeout(),
            severity: 'medium'
          }
        ]
      },
      {
        name: 'Data Protection',
        enabled: true,
        enforcement: 'strict',
        rules: [
          {
            id: 'data-encryption',
            description: 'Ensure sensitive data is encrypted',
            check: () => this.checkDataEncryption(),
            severity: 'high'
          },
          {
            id: 'data-sanitization',
            description: 'Ensure user input is sanitized',
            check: () => this.checkDataSanitization(),
            severity: 'high'
          }
        ]
      },
      {
        name: 'Network Security',
        enabled: true,
        enforcement: 'strict',
        rules: [
          {
            id: 'https-enforcement',
            description: 'Ensure HTTPS is enforced',
            check: () => this.checkHTTPSEnforcement(),
            severity: 'critical'
          },
          {
            id: 'api-rate-limiting',
            description: 'Ensure API rate limiting is configured',
            check: () => this.checkRateLimiting(),
            severity: 'medium'
          }
        ]
      }
    ];
  }

  // Security checks implementation
  private checkCSPScriptSrc(): boolean {
    const metaTags = document.querySelectorAll('meta[http-equiv="Content-Security-Policy"]');
    if (metaTags.length === 0) return false;
    
    const cspContent = metaTags[0].getAttribute('content') || '';
    return cspContent.includes('script-src') && !cspContent.includes("'unsafe-eval'");
  }

  private checkCSPObjectSrc(): boolean {
    const metaTags = document.querySelectorAll('meta[http-equiv="Content-Security-Policy"]');
    if (metaTags.length === 0) return false;
    
    const cspContent = metaTags[0].getAttribute('content') || '';
    return cspContent.includes("object-src 'none'");
  }

  private checkTokenStorage(): boolean {
    // Check if tokens are stored in localStorage (insecure)
    const hasTokenInLocalStorage = localStorage.getItem('token') !== null ||
                                  localStorage.getItem('authToken') !== null ||
                                  localStorage.getItem('accessToken') !== null;
    
    // Tokens should be in httpOnly cookies or secure storage
    return !hasTokenInLocalStorage;
  }

  private checkSessionTimeout(): boolean {
    // Check if session timeout is configured
    const sessionConfig = localStorage.getItem('sessionConfig');
    if (!sessionConfig) return false;
    
    try {
      const config = JSON.parse(sessionConfig);
      return config.timeout && config.timeout > 0 && config.timeout <= 3600000; // Max 1 hour
    } catch {
      return false;
    }
  }

  private checkDataEncryption(): boolean {
    // Check if sensitive data fields are encrypted
    const sensitiveKeys = ['password', 'ssn', 'creditCard', 'bankAccount'];
    const localStorageData = Object.keys(localStorage);
    
    return !sensitiveKeys.some(key => 
      localStorageData.some(storageKey => 
        storageKey.toLowerCase().includes(key.toLowerCase())
      )
    );
  }

  private checkDataSanitization(): boolean {
    // Check for potential XSS vulnerabilities in DOM
    const scripts = document.querySelectorAll('script');
    const inlineScripts = Array.from(scripts).filter(script => 
      script.innerHTML.includes('innerHTML') || 
      script.innerHTML.includes('document.write')
    );
    
    return inlineScripts.length === 0;
  }

  private checkHTTPSEnforcement(): boolean {
    return window.location.protocol === 'https:' || 
           window.location.hostname === 'localhost' ||
           window.location.hostname === '127.0.0.1';
  }

  private checkRateLimiting(): boolean {
    // Check if rate limiting headers are present in API responses
    const rateLimitConfig = sessionStorage.getItem('rateLimitConfig');
    return rateLimitConfig !== null;
  }

  // Vulnerability scanning
  async scanForVulnerabilities(): Promise<SecurityVulnerability[]> {
    const vulnerabilities: SecurityVulnerability[] = [];

    // XSS vulnerability scan
    const xssVulns = await this.scanXSSVulnerabilities();
    vulnerabilities.push(...xssVulns);

    // CSRF vulnerability scan
    const csrfVulns = await this.scanCSRFVulnerabilities();
    vulnerabilities.push(...csrfVulns);

    // Dependency vulnerability scan
    const depVulns = await this.scanDependencyVulnerabilities();
    vulnerabilities.push(...depVulns);

    // Authentication vulnerability scan
    const authVulns = await this.scanAuthVulnerabilities();
    vulnerabilities.push(...authVulns);

    this.vulnerabilities = vulnerabilities;
    return vulnerabilities;
  }

  private async scanXSSVulnerabilities(): Promise<SecurityVulnerability[]> {
    const vulnerabilities: SecurityVulnerability[] = [];
    
    // Check for dangerous innerHTML usage
    const elements = document.querySelectorAll('*');
    elements.forEach((element, index) => {
      if (element.innerHTML.includes('<script>') || 
          element.innerHTML.includes('javascript:') ||
          element.innerHTML.includes('onload=') ||
          element.innerHTML.includes('onerror=')) {
        vulnerabilities.push({
          id: `xss-${index}`,
          severity: 'high',
          type: 'xss',
          description: 'Potential XSS vulnerability detected in DOM element',
          location: `Element: ${element.tagName}`,
          recommendation: 'Use textContent instead of innerHTML for user data',
          cwe: 'CWE-79',
          cvss: 7.5
        });
      }
    });

    return vulnerabilities;
  }

  private async scanCSRFVulnerabilities(): Promise<SecurityVulnerability[]> {
    const vulnerabilities: SecurityVulnerability[] = [];
    
    // Check for CSRF token in forms
    const forms = document.querySelectorAll('form');
    forms.forEach((form, index) => {
      const hasCSRFToken = form.querySelector('input[name="csrf_token"]') ||
                          form.querySelector('input[name="_token"]') ||
                          form.querySelector('meta[name="csrf-token"]');
      
      if (!hasCSRFToken && form.method.toLowerCase() === 'post') {
        vulnerabilities.push({
          id: `csrf-${index}`,
          severity: 'medium',
          type: 'csrf',
          description: 'Form missing CSRF protection',
          location: `Form: ${form.action || 'current page'}`,
          recommendation: 'Add CSRF token to all POST forms',
          cwe: 'CWE-352',
          cvss: 6.1
        });
      }
    });

    return vulnerabilities;
  }

  private async scanDependencyVulnerabilities(): Promise<SecurityVulnerability[]> {
    // This would typically integrate with npm audit or similar tools
    // For demo purposes, we'll simulate some common vulnerabilities
    return [
      {
        id: 'dep-1',
        severity: 'medium',
        type: 'dependency',
        description: 'Outdated dependency with known vulnerabilities',
        location: 'package.json',
        recommendation: 'Update to latest secure version',
        cwe: 'CWE-1104',
        cvss: 5.3
      }
    ];
  }

  private async scanAuthVulnerabilities(): Promise<SecurityVulnerability[]> {
    const vulnerabilities: SecurityVulnerability[] = [];
    
    // Check for insecure token storage
    if (!this.checkTokenStorage()) {
      vulnerabilities.push({
        id: 'auth-storage',
        severity: 'critical',
        type: 'auth',
        description: 'Authentication tokens stored in localStorage',
        location: 'localStorage',
        recommendation: 'Use httpOnly cookies or secure token storage',
        cwe: 'CWE-922',
        cvss: 9.1
      });
    }

    return vulnerabilities;
  }

  // Policy enforcement
  async enforceSecurityPolicies(): Promise<Array<{ policy: string; rule: string; severity: string }>> {
    const violations: Array<{ policy: string; rule: string; severity: string }> = [];

    for (const policy of this.policies) {
      if (!policy.enabled) continue;

      for (const rule of policy.rules) {
        try {
          const passed = await rule.check();
          if (!passed) {
            violations.push({
              policy: policy.name,
              rule: rule.description,
              severity: rule.severity
            });

            if (policy.enforcement === 'strict') {
              console.error(`Security Policy Violation: ${policy.name} - ${rule.description}`);
            } else if (policy.enforcement === 'warn') {
              console.warn(`Security Policy Warning: ${policy.name} - ${rule.description}`);
            } else {
              console.log(`Security Policy Log: ${policy.name} - ${rule.description}`);
            }
          }
        } catch (error) {
          console.error(`Error checking security rule ${rule.id}:`, error);
        }
      }
    }

    return violations;
  }

  // Generate security audit report
  async generateAuditReport(): Promise<SecurityAuditReport> {
    const vulnerabilities = await this.scanForVulnerabilities();
    const policyViolations = await this.enforceSecurityPolicies();
    
    // Calculate security score (0-100)
    const criticalCount = vulnerabilities.filter(v => v.severity === 'critical').length;
    const highCount = vulnerabilities.filter(v => v.severity === 'high').length;
    const mediumCount = vulnerabilities.filter(v => v.severity === 'medium').length;
    const lowCount = vulnerabilities.filter(v => v.severity === 'low').length;
    
    const securityScore = Math.max(0, 100 - (criticalCount * 25 + highCount * 10 + mediumCount * 5 + lowCount * 1));
    
    // Generate recommendations
    const recommendations = this.generateRecommendations(vulnerabilities, policyViolations);
    
    // Check compliance status
    const complianceStatus = {
      owasp: criticalCount === 0 && highCount <= 2,
      gdpr: this.checkGDPRCompliance(),
      hipaa: this.checkHIPAACompliance(),
      sox: this.checkSOXCompliance()
    };

    const report: SecurityAuditReport = {
      timestamp: Date.now(),
      vulnerabilities,
      policyViolations,
      securityScore,
      recommendations,
      complianceStatus
    };

    this.auditHistory.push(report);
    return report;
  }

  private generateRecommendations(
    vulnerabilities: SecurityVulnerability[], 
    violations: Array<{ policy: string; rule: string; severity: string }>
  ): string[] {
    const recommendations: string[] = [];
    
    if (vulnerabilities.some(v => v.type === 'xss')) {
      recommendations.push('Implement Content Security Policy (CSP) headers');
      recommendations.push('Use DOMPurify for sanitizing user input');
    }
    
    if (vulnerabilities.some(v => v.type === 'csrf')) {
      recommendations.push('Implement CSRF tokens for all state-changing operations');
    }
    
    if (vulnerabilities.some(v => v.type === 'auth')) {
      recommendations.push('Use httpOnly cookies for authentication tokens');
      recommendations.push('Implement proper session management');
    }
    
    if (violations.some(v => v.severity === 'critical')) {
      recommendations.push('Address critical security policy violations immediately');
    }

    return recommendations;
  }

  private checkGDPRCompliance(): boolean {
    // Check for GDPR compliance indicators
    const hasPrivacyPolicy = document.querySelector('[data-privacy-policy]') !== null;
    const hasCookieConsent = document.querySelector('[data-cookie-consent]') !== null;
    return hasPrivacyPolicy && hasCookieConsent;
  }

  private checkHIPAACompliance(): boolean {
    // Check for HIPAA compliance indicators
    const hasDataEncryption = this.checkDataEncryption();
    const hasAccessControls = this.checkTokenStorage();
    return hasDataEncryption && hasAccessControls;
  }

  private checkSOXCompliance(): boolean {
    // Check for SOX compliance indicators
    const hasAuditLogging = sessionStorage.getItem('auditLog') !== null;
    const hasAccessControls = this.checkTokenStorage();
    return hasAuditLogging && hasAccessControls;
  }

  // Start continuous monitoring
  startMonitoring(interval: number = 300000): void { // 5 minutes default
    if (this.isMonitoring) return;
    
    this.isMonitoring = true;
    this.initializePolicies();
    
    setInterval(async () => {
      await this.generateAuditReport();
    }, interval);
  }

  // Stop monitoring
  stopMonitoring(): void {
    this.isMonitoring = false;
  }

  // Get audit history
  getAuditHistory(): SecurityAuditReport[] {
    return [...this.auditHistory];
  }

  // Export audit data
  exportAuditData(): string {
    return JSON.stringify({
      auditHistory: this.auditHistory,
      policies: this.policies,
      timestamp: Date.now()
    }, null, 2);
  }
}

// Export singleton instance
export const securityAudit = SecurityAuditSystem.getInstance();
export default SecurityAuditSystem;
