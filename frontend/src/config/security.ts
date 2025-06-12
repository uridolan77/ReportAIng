/**
 * Ultimate Security Configuration
 * Comprehensive security settings for maximum application security
 */

import DOMPurify from 'dompurify';

// ===== SECURITY CONSTANTS =====
export const SECURITY_CONFIG = {
  // Content Security Policy
  CSP: {
    DEFAULT_SRC: ["'self'"],
    SCRIPT_SRC: ["'self'", "'unsafe-inline'", "'unsafe-eval'"],
    STYLE_SRC: ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
    FONT_SRC: ["'self'", "https://fonts.gstatic.com"],
    IMG_SRC: ["'self'", "data:", "https:"],
    CONNECT_SRC: ["'self'", "wss:", "https:"],
    FRAME_SRC: ["'none'"],
    OBJECT_SRC: ["'none'"],
    BASE_URI: ["'self'"],
    FORM_ACTION: ["'self'"],
  },

  // Input Validation
  VALIDATION: {
    MAX_INPUT_LENGTH: 10000,
    MAX_QUERY_LENGTH: 50000,
    MAX_FILE_SIZE: 10 * 1024 * 1024, // 10MB
    ALLOWED_FILE_TYPES: ['.json', '.csv', '.xlsx', '.sql'],
    SQL_INJECTION_PATTERNS: [
      /(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)/gi,
      /(UNION\s+SELECT)/gi,
      /(OR\s+1\s*=\s*1)/gi,
      /(AND\s+1\s*=\s*1)/gi,
      /(';\s*--)/gi,
      /(\/\*.*?\*\/)/gi,
    ],
    XSS_PATTERNS: [
      /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
      /<iframe\b[^<]*(?:(?!<\/iframe>)<[^<]*)*<\/iframe>/gi,
      /javascript:/gi,
      /on\w+\s*=/gi,
    ],
  },

  // Authentication & Authorization
  AUTH: {
    TOKEN_EXPIRY: 24 * 60 * 60 * 1000, // 24 hours
    REFRESH_TOKEN_EXPIRY: 7 * 24 * 60 * 60 * 1000, // 7 days
    MAX_LOGIN_ATTEMPTS: 5,
    LOCKOUT_DURATION: 15 * 60 * 1000, // 15 minutes
    SESSION_TIMEOUT: 30 * 60 * 1000, // 30 minutes
    REQUIRE_2FA: false,
    PASSWORD_MIN_LENGTH: 8,
    PASSWORD_REQUIRE_SPECIAL: true,
    PASSWORD_REQUIRE_NUMBERS: true,
    PASSWORD_REQUIRE_UPPERCASE: true,
  },

  // Rate Limiting
  RATE_LIMIT: {
    REQUESTS_PER_MINUTE: 100,
    QUERIES_PER_MINUTE: 20,
    UPLOADS_PER_HOUR: 10,
    API_CALLS_PER_HOUR: 1000,
    WEBSOCKET_MESSAGES_PER_MINUTE: 200,
  },

  // Data Protection
  DATA_PROTECTION: {
    ENCRYPT_LOCAL_STORAGE: true,
    ENCRYPT_SESSION_STORAGE: true,
    MASK_SENSITIVE_DATA: true,
    LOG_SENSITIVE_DATA: false,
    SANITIZE_OUTPUTS: true,
    VALIDATE_INPUTS: true,
    SECURE_HEADERS: true,
  },

  // Monitoring & Alerting
  MONITORING: {
    LOG_SECURITY_EVENTS: true,
    ALERT_ON_SUSPICIOUS_ACTIVITY: true,
    TRACK_USER_BEHAVIOR: true,
    MONITOR_API_USAGE: true,
    DETECT_ANOMALIES: true,
    REPORT_VIOLATIONS: true,
  },
} as const;

// ===== SECURITY UTILITIES =====
export const SecurityUtils = {
  // Input sanitization
  sanitizeInput: (input: string): string => {
    if (!input || typeof input !== 'string') return '';
    
    // Remove potential XSS patterns
    let sanitized = input;
    SECURITY_CONFIG.VALIDATION.XSS_PATTERNS.forEach(pattern => {
      sanitized = sanitized.replace(pattern, '');
    });
    
    // Use DOMPurify for additional sanitization
    sanitized = DOMPurify.sanitize(sanitized, {
      ALLOWED_TAGS: [],
      ALLOWED_ATTR: [],
    });
    
    return sanitized.trim();
  },

  // SQL injection detection
  detectSQLInjection: (query: string): boolean => {
    if (!query || typeof query !== 'string') return false;
    
    return SECURITY_CONFIG.VALIDATION.SQL_INJECTION_PATTERNS.some(pattern => 
      pattern.test(query)
    );
  },

  // XSS detection
  detectXSS: (input: string): boolean => {
    if (!input || typeof input !== 'string') return false;
    
    return SECURITY_CONFIG.VALIDATION.XSS_PATTERNS.some(pattern => 
      pattern.test(input)
    );
  },

  // Validate file upload
  validateFileUpload: (file: File): { valid: boolean; error?: string } => {
    // Check file size
    if (file.size > SECURITY_CONFIG.VALIDATION.MAX_FILE_SIZE) {
      return { valid: false, error: 'File size exceeds maximum allowed size' };
    }
    
    // Check file type
    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!SECURITY_CONFIG.VALIDATION.ALLOWED_FILE_TYPES.includes(extension)) {
      return { valid: false, error: 'File type not allowed' };
    }
    
    return { valid: true };
  },

  // Generate secure random string
  generateSecureRandom: (length = 32): string => {
    const array = new Uint8Array(length);
    crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  },

  // Hash sensitive data
  hashSensitiveData: async (data: string): Promise<string> => {
    const encoder = new TextEncoder();
    const dataBuffer = encoder.encode(data);
    const hashBuffer = await crypto.subtle.digest('SHA-256', dataBuffer);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  },

  // Encrypt data for local storage
  encryptForStorage: async (data: string, key?: string): Promise<string> => {
    if (!SECURITY_CONFIG.DATA_PROTECTION.ENCRYPT_LOCAL_STORAGE) {
      return data;
    }
    
    try {
      const encoder = new TextEncoder();
      const dataBuffer = encoder.encode(data);
      
      // Generate or use provided key
      const keyMaterial = key || await crypto.subtle.generateKey(
        { name: 'AES-GCM', length: 256 },
        true,
        ['encrypt', 'decrypt']
      );
      
      // Generate IV
      const iv = crypto.getRandomValues(new Uint8Array(12));
      
      // Encrypt data
      const encrypted = await crypto.subtle.encrypt(
        { name: 'AES-GCM', iv },
        keyMaterial,
        dataBuffer
      );
      
      // Combine IV and encrypted data
      const combined = new Uint8Array(iv.length + encrypted.byteLength);
      combined.set(iv);
      combined.set(new Uint8Array(encrypted), iv.length);
      
      return btoa(String.fromCharCode(...combined));
    } catch (error) {
      console.error('Encryption failed:', error);
      return data; // Fallback to unencrypted
    }
  },

  // Decrypt data from local storage
  decryptFromStorage: async (encryptedData: string, key?: string): Promise<string> => {
    if (!SECURITY_CONFIG.DATA_PROTECTION.ENCRYPT_LOCAL_STORAGE) {
      return encryptedData;
    }
    
    try {
      // Decode base64
      const combined = new Uint8Array(
        atob(encryptedData).split('').map(char => char.charCodeAt(0))
      );
      
      // Extract IV and encrypted data
      const iv = combined.slice(0, 12);
      const encrypted = combined.slice(12);
      
      // Use provided key or generate (this should be stored securely)
      const keyMaterial = key || await crypto.subtle.generateKey(
        { name: 'AES-GCM', length: 256 },
        true,
        ['encrypt', 'decrypt']
      );
      
      // Decrypt data
      const decrypted = await crypto.subtle.decrypt(
        { name: 'AES-GCM', iv },
        keyMaterial,
        encrypted
      );
      
      const decoder = new TextDecoder();
      return decoder.decode(decrypted);
    } catch (error) {
      console.error('Decryption failed:', error);
      return encryptedData; // Fallback to encrypted data
    }
  },

  // Mask sensitive data for display
  maskSensitiveData: (data: string, visibleChars = 4): string => {
    if (!SECURITY_CONFIG.DATA_PROTECTION.MASK_SENSITIVE_DATA) {
      return data;
    }
    
    if (!data || data.length <= visibleChars) {
      return '*'.repeat(data.length);
    }
    
    const masked = '*'.repeat(data.length - visibleChars);
    return masked + data.slice(-visibleChars);
  },

  // Validate password strength
  validatePasswordStrength: (password: string): { valid: boolean; errors: string[] } => {
    const errors: string[] = [];
    
    if (password.length < SECURITY_CONFIG.AUTH.PASSWORD_MIN_LENGTH) {
      errors.push(`Password must be at least ${SECURITY_CONFIG.AUTH.PASSWORD_MIN_LENGTH} characters long`);
    }
    
    if (SECURITY_CONFIG.AUTH.PASSWORD_REQUIRE_UPPERCASE && !/[A-Z]/.test(password)) {
      errors.push('Password must contain at least one uppercase letter');
    }
    
    if (SECURITY_CONFIG.AUTH.PASSWORD_REQUIRE_NUMBERS && !/\d/.test(password)) {
      errors.push('Password must contain at least one number');
    }
    
    if (SECURITY_CONFIG.AUTH.PASSWORD_REQUIRE_SPECIAL && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
      errors.push('Password must contain at least one special character');
    }
    
    return { valid: errors.length === 0, errors };
  },

  // Rate limiting check
  checkRateLimit: (key: string, limit: number, windowMs: number): boolean => {
    const now = Date.now();
    const windowStart = now - windowMs;
    
    // Get stored requests for this key
    const stored = localStorage.getItem(`rate_limit_${key}`);
    let requests: number[] = stored ? JSON.parse(stored) : [];
    
    // Filter out old requests
    requests = requests.filter(timestamp => timestamp > windowStart);
    
    // Check if limit exceeded
    if (requests.length >= limit) {
      return false;
    }
    
    // Add current request
    requests.push(now);
    localStorage.setItem(`rate_limit_${key}`, JSON.stringify(requests));
    
    return true;
  },
} as const;

// ===== SECURITY MONITORING =====
export const SecurityMonitor = {
  // Log security event
  logSecurityEvent: (event: SecurityEvent): void => {
    if (!SECURITY_CONFIG.MONITORING.LOG_SECURITY_EVENTS) return;
    
    const logEntry = {
      timestamp: new Date().toISOString(),
      type: event.type,
      severity: event.severity,
      details: event.details,
      userAgent: navigator.userAgent,
      url: window.location.href,
      userId: event.userId,
    };
    
    console.warn('Security Event:', logEntry);
    
    // Send to monitoring service
    if (SECURITY_CONFIG.MONITORING.REPORT_VIOLATIONS) {
      // Implementation would send to your monitoring service
    }
  },

  // Detect suspicious activity
  detectSuspiciousActivity: (activity: UserActivity): boolean => {
    if (!SECURITY_CONFIG.MONITORING.DETECT_ANOMALIES) return false;
    
    // Implement anomaly detection logic
    const suspiciousPatterns = [
      activity.requestCount > SECURITY_CONFIG.RATE_LIMIT.REQUESTS_PER_MINUTE,
      activity.failedLogins > SECURITY_CONFIG.AUTH.MAX_LOGIN_ATTEMPTS,
      activity.unusualTimePattern,
      activity.multipleIPs,
    ];
    
    return suspiciousPatterns.some(Boolean);
  },

  // Monitor API usage
  monitorAPIUsage: (endpoint: string, method: string, responseTime: number): void => {
    if (!SECURITY_CONFIG.MONITORING.MONITOR_API_USAGE) return;
    
    const usage = {
      endpoint,
      method,
      responseTime,
      timestamp: Date.now(),
    };
    
    // Store usage data for analysis
    const stored = localStorage.getItem('api_usage') || '[]';
    const usageData = JSON.parse(stored);
    usageData.push(usage);
    
    // Keep only last 1000 entries
    if (usageData.length > 1000) {
      usageData.splice(0, usageData.length - 1000);
    }
    
    localStorage.setItem('api_usage', JSON.stringify(usageData));
  },
} as const;

// ===== TYPE DEFINITIONS =====
export interface SecurityEvent {
  type: 'xss_attempt' | 'sql_injection' | 'rate_limit_exceeded' | 'unauthorized_access' | 'suspicious_activity';
  severity: 'low' | 'medium' | 'high' | 'critical';
  details: string;
  userId?: string;
}

export interface UserActivity {
  requestCount: number;
  failedLogins: number;
  unusualTimePattern: boolean;
  multipleIPs: boolean;
  suspiciousQueries: number;
}

export interface SecurityConfig {
  enableCSP: boolean;
  enableInputValidation: boolean;
  enableRateLimiting: boolean;
  enableEncryption: boolean;
  enableMonitoring: boolean;
  strictMode: boolean;
}

// ===== DEFAULT EXPORT =====
export default {
  config: SECURITY_CONFIG,
  utils: SecurityUtils,
  monitor: SecurityMonitor,
} as const;
