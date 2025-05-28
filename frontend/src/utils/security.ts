import DOMPurify from 'dompurify';

export class SecurityUtils {
  // Secure token storage using encryption
  private static readonly ENCRYPTION_KEY = process.env.REACT_APP_ENCRYPTION_KEY || 'default-key';

  static encryptToken(token: string): string {
    // In production, use Web Crypto API
    if (window.crypto && window.crypto.subtle) {
      // Implementation using Web Crypto API
      return btoa(token); // Simplified for example
    }
    return token;
  }

  static decryptToken(encryptedToken: string): string {
    if (window.crypto && window.crypto.subtle) {
      // Implementation using Web Crypto API
      return atob(encryptedToken); // Simplified for example
    }
    return encryptedToken;
  }

  // XSS Protection
  static sanitizeHtml(html: string): string {
    return DOMPurify.sanitize(html, {
      ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'span', 'p', 'br'],
      ALLOWED_ATTR: ['class', 'style']
    });
  }

  static sanitizeSQL(sql: string): string {
    // Remove potentially dangerous SQL keywords for display
    const dangerous = ['DROP', 'DELETE', 'TRUNCATE', 'INSERT', 'UPDATE'];
    let sanitized = sql;
    
    dangerous.forEach(keyword => {
      const regex = new RegExp(`\\b${keyword}\\b`, 'gi');
      sanitized = sanitized.replace(regex, `[${keyword}]`);
    });
    
    return sanitized;
  }

  // Content Security Policy
  static setCSPHeaders(): void {
    const meta = document.createElement('meta');
    meta.httpEquiv = 'Content-Security-Policy';
    meta.content = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';";
    document.head.appendChild(meta);
  }

  // Input validation
  static validateInput(input: string, type: 'email' | 'username' | 'sql' | 'general'): boolean {
    switch (type) {
      case 'email':
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(input);
      case 'username':
        const usernameRegex = /^[a-zA-Z0-9_]{3,20}$/;
        return usernameRegex.test(input);
      case 'sql':
        // Basic SQL injection prevention
        const sqlInjectionPatterns = [
          /(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)/i,
          /(;|\-\-|\/\*|\*\/)/,
          /(\b(UNION|OR|AND)\b.*\b(SELECT|INSERT|UPDATE|DELETE)\b)/i
        ];
        return !sqlInjectionPatterns.some(pattern => pattern.test(input));
      case 'general':
        // Remove potentially dangerous characters
        const dangerousChars = /<script|javascript:|data:|vbscript:/i;
        return !dangerousChars.test(input);
      default:
        return true;
    }
  }

  // Rate limiting helper
  static createRateLimiter(maxRequests: number, windowMs: number) {
    const requests: number[] = [];
    
    return () => {
      const now = Date.now();
      const windowStart = now - windowMs;
      
      // Remove old requests outside the window
      while (requests.length > 0 && requests[0] < windowStart) {
        requests.shift();
      }
      
      if (requests.length >= maxRequests) {
        return false; // Rate limit exceeded
      }
      
      requests.push(now);
      return true; // Request allowed
    };
  }
}
