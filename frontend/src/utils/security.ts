import DOMPurify from 'dompurify';

export class SecurityUtils {
  // Secure token storage using Web Crypto API
  private static readonly ALGORITHM = 'AES-GCM';
  private static readonly KEY_LENGTH = 256;
  private static readonly IV_LENGTH = 12;

  // Generate a key from password using PBKDF2
  private static async deriveKey(password: string, salt: Uint8Array): Promise<CryptoKey> {
    const encoder = new TextEncoder();
    const keyMaterial = await window.crypto.subtle.importKey(
      'raw',
      encoder.encode(password),
      'PBKDF2',
      false,
      ['deriveKey']
    );

    return window.crypto.subtle.deriveKey(
      {
        name: 'PBKDF2',
        salt: salt,
        iterations: 100000,
        hash: 'SHA-256'
      },
      keyMaterial,
      { name: this.ALGORITHM, length: this.KEY_LENGTH },
      false,
      ['encrypt', 'decrypt']
    );
  }

  static async encryptToken(token: string): Promise<string> {
    if (!window.crypto || !window.crypto.subtle) {
      console.warn('Web Crypto API not available, using base64 encoding');
      return btoa(token);
    }

    try {
      const encoder = new TextEncoder();
      const data = encoder.encode(token);

      // Generate random salt and IV
      const salt = window.crypto.getRandomValues(new Uint8Array(16));
      const iv = window.crypto.getRandomValues(new Uint8Array(this.IV_LENGTH));

      // Use a consistent password base (user agent + session identifier)
      const passwordBase = `${navigator.userAgent}-bi-reporting-copilot-2025`;
      const key = await this.deriveKey(passwordBase, salt);

      const encrypted = await window.crypto.subtle.encrypt(
        { name: this.ALGORITHM, iv: iv },
        key,
        data
      );

      // Combine salt, iv, and encrypted data
      const combined = new Uint8Array(salt.length + iv.length + encrypted.byteLength);
      combined.set(salt, 0);
      combined.set(iv, salt.length);
      combined.set(new Uint8Array(encrypted), salt.length + iv.length);

      return btoa(String.fromCharCode(...combined));
    } catch (error) {
      console.error('Encryption failed:', error);
      return btoa(token); // Fallback to base64
    }
  }

  static async decryptToken(encryptedToken: string): Promise<string> {
    if (!window.crypto || !window.crypto.subtle) {
      console.warn('Web Crypto API not available, using base64 decoding');
      return atob(encryptedToken);
    }

    try {
      const combined = new Uint8Array(
        atob(encryptedToken).split('').map(char => char.charCodeAt(0))
      );

      // Extract salt, iv, and encrypted data
      const salt = combined.slice(0, 16);
      const iv = combined.slice(16, 16 + this.IV_LENGTH);
      const encrypted = combined.slice(16 + this.IV_LENGTH);

      // Derive the same key using consistent password base
      const passwordBase = `${navigator.userAgent}-bi-reporting-copilot-2025`;
      const key = await this.deriveKey(passwordBase, salt);

      const decrypted = await window.crypto.subtle.decrypt(
        { name: this.ALGORITHM, iv: iv },
        key,
        encrypted
      );

      const decoder = new TextDecoder();
      return decoder.decode(decrypted);
    } catch (error) {
      console.error('Decryption failed:', error);
      return atob(encryptedToken); // Fallback to base64
    }
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

  // Enhanced Content Security Policy
  static setCSPHeaders(): void {
    const meta = document.createElement('meta');
    meta.httpEquiv = 'Content-Security-Policy';

    // Production-ready CSP
    const cspDirectives = [
      "default-src 'self'",
      "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net",
      "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
      "font-src 'self' https://fonts.gstatic.com",
      "img-src 'self' data: https:",
      "connect-src 'self' https://localhost:* wss://localhost:*",
      "frame-ancestors 'none'",
      "base-uri 'self'",
      "form-action 'self'",
      "upgrade-insecure-requests"
    ];

    meta.content = cspDirectives.join('; ');
    document.head.appendChild(meta);
  }

  // Secure session management
  static generateSecureSessionId(): string {
    const array = new Uint8Array(32);
    window.crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }

  static setSecureSessionStorage(key: string, value: string): void {
    try {
      // Add timestamp and integrity check
      const sessionData = {
        value,
        timestamp: Date.now(),
        integrity: this.generateIntegrityHash(value)
      };
      sessionStorage.setItem(key, JSON.stringify(sessionData));
    } catch (error) {
      console.error('Failed to set secure session storage:', error);
    }
  }

  static getSecureSessionStorage(key: string): string | null {
    try {
      const data = sessionStorage.getItem(key);
      if (!data) return null;

      const sessionData = JSON.parse(data);

      // Verify integrity
      const expectedIntegrity = this.generateIntegrityHash(sessionData.value);
      if (expectedIntegrity !== sessionData.integrity) {
        console.warn('Session storage integrity check failed');
        sessionStorage.removeItem(key);
        return null;
      }

      // Check if data is too old (24 hours)
      const maxAge = 24 * 60 * 60 * 1000;
      if (Date.now() - sessionData.timestamp > maxAge) {
        sessionStorage.removeItem(key);
        return null;
      }

      return sessionData.value;
    } catch (error) {
      console.error('Failed to get secure session storage:', error);
      return null;
    }
  }

  static clearSecureSessionStorage(key: string): void {
    try {
      sessionStorage.removeItem(key);
    } catch (error) {
      console.error('Failed to clear secure session storage:', error);
    }
  }



  private static generateIntegrityHash(data: string): string {
    // Simple hash for integrity check (not cryptographically secure)
    let hash = 0;
    for (let i = 0; i < data.length; i++) {
      const char = data.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return hash.toString(16);
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
          /(;|--|\/\*|\*\/)/,
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
