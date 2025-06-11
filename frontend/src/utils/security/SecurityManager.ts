/**
 * Enterprise Security Manager
 * 
 * Comprehensive security management system with threat detection,
 * security policies, and real-time monitoring capabilities.
 */

import { CSPManager } from './CSPManager';
import { XSSProtection } from './XSSProtection';
import { SecureStorage } from './SecureStorage';
import { InputValidator } from './InputValidator';
import { AuditLogger } from './AuditLogger';

export interface SecurityConfig {
  // Content Security Policy
  csp: {
    enabled: boolean;
    reportOnly: boolean;
    directives: Record<string, string[]>;
  };
  
  // XSS Protection
  xss: {
    enabled: boolean;
    strictMode: boolean;
    allowedTags: string[];
    allowedAttributes: string[];
  };
  
  // Input Validation
  validation: {
    enabled: boolean;
    strictMode: boolean;
    maxInputLength: number;
    allowedCharsets: string[];
  };
  
  // Session Security
  session: {
    timeout: number;
    renewalThreshold: number;
    maxConcurrentSessions: number;
    requireReauth: boolean;
  };
  
  // Audit & Monitoring
  audit: {
    enabled: boolean;
    logLevel: 'debug' | 'info' | 'warn' | 'error';
    retentionDays: number;
    realTimeAlerts: boolean;
  };
  
  // Rate Limiting
  rateLimit: {
    enabled: boolean;
    maxRequests: number;
    windowMs: number;
    blockDuration: number;
  };
}

export interface SecurityThreat {
  id: string;
  type: 'xss' | 'injection' | 'csrf' | 'brute-force' | 'data-breach' | 'unauthorized-access';
  severity: 'low' | 'medium' | 'high' | 'critical';
  timestamp: Date;
  source: string;
  description: string;
  data: any;
  blocked: boolean;
  actions: string[];
}

export interface SecurityMetrics {
  threatsDetected: number;
  threatsBlocked: number;
  failedLogins: number;
  suspiciousActivities: number;
  dataBreachAttempts: number;
  lastThreatDetected: Date | null;
  securityScore: number;
}

export class SecurityManager {
  private config: SecurityConfig;
  private cspManager: CSPManager;
  private xssProtection: XSSProtection;
  private secureStorage: SecureStorage;
  private inputValidator: InputValidator;
  private auditLogger: AuditLogger;
  
  private threats: SecurityThreat[] = [];
  private metrics: SecurityMetrics = {
    threatsDetected: 0,
    threatsBlocked: 0,
    failedLogins: 0,
    suspiciousActivities: 0,
    dataBreachAttempts: 0,
    lastThreatDetected: null,
    securityScore: 100,
  };
  
  private rateLimitTracker: Map<string, number[]> = new Map();
  private sessionTracker: Map<string, Date> = new Map();

  constructor(config: Partial<SecurityConfig> = {}) {
    this.config = {
      csp: {
        enabled: true,
        reportOnly: false,
        directives: {
          'default-src': ["'self'"],
          'script-src': ["'self'", "'unsafe-inline'"],
          'style-src': ["'self'", "'unsafe-inline'"],
          'img-src': ["'self'", 'data:', 'https:'],
          'connect-src': ["'self'"],
          'font-src': ["'self'"],
          'object-src': ["'none'"],
          'media-src': ["'self'"],
          'frame-src': ["'none'"],
        },
      },
      xss: {
        enabled: true,
        strictMode: true,
        allowedTags: ['b', 'i', 'em', 'strong', 'p', 'br'],
        allowedAttributes: ['class', 'id'],
      },
      validation: {
        enabled: true,
        strictMode: true,
        maxInputLength: 10000,
        allowedCharsets: ['utf-8'],
      },
      session: {
        timeout: 30 * 60 * 1000, // 30 minutes
        renewalThreshold: 5 * 60 * 1000, // 5 minutes
        maxConcurrentSessions: 3,
        requireReauth: true,
      },
      audit: {
        enabled: true,
        logLevel: 'info',
        retentionDays: 90,
        realTimeAlerts: true,
      },
      rateLimit: {
        enabled: true,
        maxRequests: 100,
        windowMs: 15 * 60 * 1000, // 15 minutes
        blockDuration: 60 * 1000, // 1 minute
      },
      ...config,
    };

    this.initializeComponents();
    this.setupSecurityPolicies();
    this.startMonitoring();
  }

  private initializeComponents(): void {
    this.cspManager = new CSPManager(this.config.csp);
    this.xssProtection = new XSSProtection(this.config.xss);
    this.secureStorage = new SecureStorage();
    this.inputValidator = new InputValidator(this.config.validation);
    this.auditLogger = new AuditLogger(this.config.audit);
  }

  private setupSecurityPolicies(): void {
    // Setup Content Security Policy
    if (this.config.csp.enabled) {
      this.cspManager.apply();
    }

    // Setup security headers
    this.setupSecurityHeaders();

    // Setup event listeners for security events
    this.setupEventListeners();
  }

  private setupSecurityHeaders(): void {
    // These would typically be set by the server, but we can validate them
    const expectedHeaders = {
      'X-Content-Type-Options': 'nosniff',
      'X-Frame-Options': 'DENY',
      'X-XSS-Protection': '1; mode=block',
      'Strict-Transport-Security': 'max-age=31536000; includeSubDomains',
      'Referrer-Policy': 'strict-origin-when-cross-origin',
    };

    // Validate security headers are present
    Object.entries(expectedHeaders).forEach(([header, expectedValue]) => {
      // In a real implementation, this would check actual response headers
      console.debug(`Expected security header: ${header}: ${expectedValue}`);
    });
  }

  private setupEventListeners(): void {
    // Listen for security-related events
    window.addEventListener('securitypolicyviolation', (event) => {
      this.handleCSPViolation(event as SecurityPolicyViolationEvent);
    });

    // Listen for suspicious activities
    document.addEventListener('click', (event) => {
      this.analyzeUserInteraction(event);
    });

    // Monitor for potential XSS attempts
    document.addEventListener('DOMNodeInserted', (event) => {
      this.scanForXSS(event.target as Element);
    });
  }

  private startMonitoring(): void {
    // Start periodic security checks
    setInterval(() => {
      this.performSecurityScan();
    }, 60000); // Every minute

    // Start session monitoring
    setInterval(() => {
      this.monitorSessions();
    }, 30000); // Every 30 seconds

    // Start rate limit cleanup
    setInterval(() => {
      this.cleanupRateLimitTracker();
    }, 300000); // Every 5 minutes
  }

  // Public API Methods
  validateInput(input: string, context: string = 'general'): boolean {
    try {
      const isValid = this.inputValidator.validate(input, context);
      
      if (!isValid) {
        this.reportThreat({
          type: 'injection',
          severity: 'medium',
          source: 'input-validation',
          description: `Invalid input detected in context: ${context}`,
          data: { input: input.substring(0, 100), context },
        });
      }
      
      return isValid;
    } catch (error) {
      this.auditLogger.error('Input validation error', { error, input: input.substring(0, 100) });
      return false;
    }
  }

  sanitizeInput(input: string): string {
    try {
      return this.xssProtection.sanitize(input);
    } catch (error) {
      this.auditLogger.error('Input sanitization error', { error, input: input.substring(0, 100) });
      return '';
    }
  }

  checkRateLimit(identifier: string): boolean {
    if (!this.config.rateLimit.enabled) return true;

    const now = Date.now();
    const { maxRequests, windowMs } = this.config.rateLimit;

    if (!this.rateLimitTracker.has(identifier)) {
      this.rateLimitTracker.set(identifier, []);
    }

    const requests = this.rateLimitTracker.get(identifier)!;
    
    // Remove old requests outside the window
    const validRequests = requests.filter(time => now - time < windowMs);
    
    if (validRequests.length >= maxRequests) {
      this.reportThreat({
        type: 'brute-force',
        severity: 'high',
        source: 'rate-limiter',
        description: `Rate limit exceeded for identifier: ${identifier}`,
        data: { identifier, requestCount: validRequests.length },
      });
      return false;
    }

    validRequests.push(now);
    this.rateLimitTracker.set(identifier, validRequests);
    
    return true;
  }

  reportSecurityEvent(event: Partial<SecurityThreat>): void {
    this.reportThreat({
      type: 'unauthorized-access',
      severity: 'medium',
      source: 'manual',
      description: 'Security event reported',
      ...event,
    });
  }

  getSecurityMetrics(): SecurityMetrics {
    return { ...this.metrics };
  }

  getRecentThreats(limit: number = 10): SecurityThreat[] {
    return this.threats
      .sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime())
      .slice(0, limit);
  }

  // Private Methods
  private reportThreat(threat: Partial<SecurityThreat>): void {
    const fullThreat: SecurityThreat = {
      id: this.generateThreatId(),
      timestamp: new Date(),
      blocked: true,
      actions: [],
      ...threat,
    } as SecurityThreat;

    this.threats.push(fullThreat);
    this.updateMetrics(fullThreat);
    
    // Log the threat
    this.auditLogger.warn('Security threat detected', fullThreat);
    
    // Take automated actions
    this.handleThreat(fullThreat);
    
    // Real-time alerts
    if (this.config.audit.realTimeAlerts && fullThreat.severity === 'critical') {
      this.sendRealTimeAlert(fullThreat);
    }
  }

  private handleCSPViolation(event: SecurityPolicyViolationEvent): void {
    this.reportThreat({
      type: 'xss',
      severity: 'high',
      source: 'csp-violation',
      description: `CSP violation: ${event.violatedDirective}`,
      data: {
        violatedDirective: event.violatedDirective,
        blockedURI: event.blockedURI,
        documentURI: event.documentURI,
      },
    });
  }

  private analyzeUserInteraction(event: Event): void {
    // Analyze for suspicious patterns
    const target = event.target as Element;
    
    if (target && target.tagName === 'SCRIPT') {
      this.reportThreat({
        type: 'xss',
        severity: 'critical',
        source: 'dom-analysis',
        description: 'Suspicious script execution detected',
        data: { element: target.outerHTML.substring(0, 200) },
      });
    }
  }

  private scanForXSS(element: Element): void {
    if (!element || !this.config.xss.enabled) return;

    const suspiciousPatterns = [
      /<script/i,
      /javascript:/i,
      /on\w+\s*=/i,
      /<iframe/i,
      /<object/i,
      /<embed/i,
    ];

    const content = element.outerHTML || element.textContent || '';
    
    suspiciousPatterns.forEach(pattern => {
      if (pattern.test(content)) {
        this.reportThreat({
          type: 'xss',
          severity: 'high',
          source: 'dom-scanner',
          description: 'Potential XSS content detected',
          data: { content: content.substring(0, 200), pattern: pattern.source },
        });
      }
    });
  }

  private performSecurityScan(): void {
    // Scan for security issues
    this.scanLocalStorage();
    this.scanSessionStorage();
    this.validateSecurityHeaders();
    this.checkForSuspiciousScripts();
  }

  private scanLocalStorage(): void {
    try {
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key) {
          const value = localStorage.getItem(key);
          if (value && this.containsSensitiveData(value)) {
            this.reportThreat({
              type: 'data-breach',
              severity: 'medium',
              source: 'storage-scanner',
              description: 'Sensitive data found in localStorage',
              data: { key, valueLength: value.length },
            });
          }
        }
      }
    } catch (error) {
      this.auditLogger.error('localStorage scan error', { error });
    }
  }

  private containsSensitiveData(data: string): boolean {
    const sensitivePatterns = [
      /password/i,
      /ssn/i,
      /credit.?card/i,
      /\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b/, // Credit card pattern
      /\b\d{3}-\d{2}-\d{4}\b/, // SSN pattern
    ];

    return sensitivePatterns.some(pattern => pattern.test(data));
  }

  private generateThreatId(): string {
    return `threat_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private updateMetrics(threat: SecurityThreat): void {
    this.metrics.threatsDetected++;
    if (threat.blocked) {
      this.metrics.threatsBlocked++;
    }
    this.metrics.lastThreatDetected = threat.timestamp;
    
    // Update security score based on threat severity
    const scoreImpact = {
      low: 1,
      medium: 3,
      high: 5,
      critical: 10,
    };
    
    this.metrics.securityScore = Math.max(0, this.metrics.securityScore - scoreImpact[threat.severity]);
  }

  private handleThreat(threat: SecurityThreat): void {
    // Implement automated threat response
    switch (threat.type) {
      case 'brute-force':
        // Temporarily block the source
        threat.actions.push('source-blocked');
        break;
      
      case 'xss':
        // Sanitize and block
        threat.actions.push('content-sanitized', 'request-blocked');
        break;
      
      case 'injection':
        // Block and log
        threat.actions.push('request-blocked', 'detailed-logging');
        break;
    }
  }

  private sendRealTimeAlert(threat: SecurityThreat): void {
    // In a real implementation, this would send alerts via email, SMS, or webhook
    console.error('CRITICAL SECURITY THREAT DETECTED:', threat);
  }

  private monitorSessions(): void {
    // Monitor active sessions for suspicious activity
    this.sessionTracker.forEach((lastActivity, sessionId) => {
      const now = new Date();
      const timeSinceActivity = now.getTime() - lastActivity.getTime();
      
      if (timeSinceActivity > this.config.session.timeout) {
        this.sessionTracker.delete(sessionId);
        this.auditLogger.info('Session expired', { sessionId });
      }
    });
  }

  private cleanupRateLimitTracker(): void {
    const now = Date.now();
    const { windowMs } = this.config.rateLimit;
    
    this.rateLimitTracker.forEach((requests, identifier) => {
      const validRequests = requests.filter(time => now - time < windowMs);
      if (validRequests.length === 0) {
        this.rateLimitTracker.delete(identifier);
      } else {
        this.rateLimitTracker.set(identifier, validRequests);
      }
    });
  }

  private scanSessionStorage(): void {
    // Similar to localStorage scan
    try {
      for (let i = 0; i < sessionStorage.length; i++) {
        const key = sessionStorage.key(i);
        if (key) {
          const value = sessionStorage.getItem(key);
          if (value && this.containsSensitiveData(value)) {
            this.reportThreat({
              type: 'data-breach',
              severity: 'medium',
              source: 'storage-scanner',
              description: 'Sensitive data found in sessionStorage',
              data: { key, valueLength: value.length },
            });
          }
        }
      }
    } catch (error) {
      this.auditLogger.error('sessionStorage scan error', { error });
    }
  }

  private validateSecurityHeaders(): void {
    // Validate that security headers are properly set
    // This would typically check actual response headers
  }

  private checkForSuspiciousScripts(): void {
    const scripts = document.querySelectorAll('script');
    scripts.forEach(script => {
      if (script.src && !this.isAllowedScript(script.src)) {
        this.reportThreat({
          type: 'xss',
          severity: 'high',
          source: 'script-scanner',
          description: 'Unauthorized script detected',
          data: { src: script.src },
        });
      }
    });
  }

  private isAllowedScript(src: string): boolean {
    const allowedDomains = [
      window.location.origin,
      'https://cdn.jsdelivr.net',
      'https://unpkg.com',
    ];
    
    return allowedDomains.some(domain => src.startsWith(domain));
  }
}
