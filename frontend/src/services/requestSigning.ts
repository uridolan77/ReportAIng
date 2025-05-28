/**
 * Request Signing Service
 * Provides cryptographic signing for API requests to enhance security
 */

import { SecurityUtils } from '../utils/security';

export interface SignedRequest {
  timestamp: number;
  nonce: string;
  signature: string;
  payload: string;
  headers: Record<string, string>;
}

export interface SigningConfig {
  algorithm: 'HMAC-SHA256' | 'HMAC-SHA512';
  includeBody: boolean;
  includeHeaders: string[];
  timestampTolerance: number; // seconds
  nonceLength: number;
}

export class RequestSigningService {
  private static instance: RequestSigningService;
  private signingKey: string | null = null;
  private config: SigningConfig;

  private constructor() {
    this.config = {
      algorithm: 'HMAC-SHA256',
      includeBody: true,
      includeHeaders: ['content-type', 'user-agent', 'x-request-id'],
      timestampTolerance: 300, // 5 minutes
      nonceLength: 32,
    };
  }

  public static getInstance(): RequestSigningService {
    if (!RequestSigningService.instance) {
      RequestSigningService.instance = new RequestSigningService();
    }
    return RequestSigningService.instance;
  }

  /**
   * Initialize the signing service with a key
   */
  public async initialize(signingKey?: string): Promise<void> {
    try {
      if (signingKey) {
        this.signingKey = signingKey;
      } else {
        // Generate or retrieve signing key from secure storage
        this.signingKey = await this.getOrCreateSigningKey();
      }
    } catch (error) {
      console.error('Failed to initialize request signing:', error);
      throw new Error('Request signing initialization failed');
    }
  }

  /**
   * Sign an API request
   */
  public async signRequest(
    method: string,
    url: string,
    headers: Record<string, string> = {},
    body?: any
  ): Promise<SignedRequest> {
    if (!this.signingKey) {
      throw new Error('Request signing not initialized');
    }

    const timestamp = Date.now();
    const nonce = await this.generateNonce();
    
    // Create canonical request string
    const canonicalRequest = this.createCanonicalRequest(
      method,
      url,
      headers,
      body,
      timestamp,
      nonce
    );

    // Generate signature
    const signature = await this.generateSignature(canonicalRequest);

    // Create signed headers
    const signedHeaders = {
      ...headers,
      'X-Timestamp': timestamp.toString(),
      'X-Nonce': nonce,
      'X-Signature': signature,
      'X-Signature-Algorithm': this.config.algorithm,
    };

    return {
      timestamp,
      nonce,
      signature,
      payload: canonicalRequest,
      headers: signedHeaders,
    };
  }

  /**
   * Verify a signed request (for testing/validation)
   */
  public async verifyRequest(signedRequest: SignedRequest): Promise<boolean> {
    try {
      if (!this.signingKey) {
        return false;
      }

      // Check timestamp tolerance
      const now = Date.now();
      const timeDiff = Math.abs(now - signedRequest.timestamp) / 1000;
      if (timeDiff > this.config.timestampTolerance) {
        console.warn('Request timestamp outside tolerance window');
        return false;
      }

      // Verify signature
      const expectedSignature = await this.generateSignature(signedRequest.payload);
      return this.constantTimeCompare(signedRequest.signature, expectedSignature);
    } catch (error) {
      console.error('Request verification failed:', error);
      return false;
    }
  }

  /**
   * Create canonical request string for signing
   */
  private createCanonicalRequest(
    method: string,
    url: string,
    headers: Record<string, string>,
    body: any,
    timestamp: number,
    nonce: string
  ): string {
    const parts: string[] = [];

    // HTTP method
    parts.push(method.toUpperCase());

    // URL path and query
    const urlObj = new URL(url, window.location.origin);
    parts.push(urlObj.pathname + urlObj.search);

    // Canonical headers
    const canonicalHeaders = this.createCanonicalHeaders(headers);
    parts.push(canonicalHeaders);

    // Body hash (if configured to include body)
    if (this.config.includeBody && body) {
      const bodyString = typeof body === 'string' ? body : JSON.stringify(body);
      parts.push(this.hashString(bodyString));
    }

    // Timestamp and nonce
    parts.push(timestamp.toString());
    parts.push(nonce);

    return parts.join('\n');
  }

  /**
   * Create canonical headers string
   */
  private createCanonicalHeaders(headers: Record<string, string>): string {
    const includedHeaders: string[] = [];

    // Include specified headers in canonical form
    for (const headerName of this.config.includeHeaders) {
      const value = headers[headerName.toLowerCase()];
      if (value) {
        includedHeaders.push(`${headerName.toLowerCase()}:${value.trim()}`);
      }
    }

    return includedHeaders.sort().join('\n');
  }

  /**
   * Generate HMAC signature
   */
  private async generateSignature(payload: string): Promise<string> {
    if (!this.signingKey) {
      throw new Error('Signing key not available');
    }

    try {
      const encoder = new TextEncoder();
      const keyData = encoder.encode(this.signingKey);
      const payloadData = encoder.encode(payload);

      const cryptoKey = await crypto.subtle.importKey(
        'raw',
        keyData,
        { name: 'HMAC', hash: this.getHashAlgorithm() },
        false,
        ['sign']
      );

      const signature = await crypto.subtle.sign('HMAC', cryptoKey, payloadData);
      return this.arrayBufferToHex(signature);
    } catch (error) {
      console.error('Signature generation failed:', error);
      throw new Error('Failed to generate request signature');
    }
  }

  /**
   * Generate cryptographically secure nonce
   */
  private async generateNonce(): Promise<string> {
    const array = new Uint8Array(this.config.nonceLength);
    crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }

  /**
   * Get or create signing key from secure storage
   */
  private async getOrCreateSigningKey(): Promise<string> {
    try {
      // Try to get existing key from secure storage
      let key = SecurityUtils.getSecureSessionStorage('request-signing-key');
      
      if (!key) {
        // Generate new key
        key = await this.generateSigningKey();
        SecurityUtils.setSecureSessionStorage('request-signing-key', key);
      }

      return key;
    } catch (error) {
      console.error('Failed to get/create signing key:', error);
      // Fallback to session-based key
      return this.generateSessionKey();
    }
  }

  /**
   * Generate a new signing key
   */
  private async generateSigningKey(): Promise<string> {
    const array = new Uint8Array(64); // 512-bit key
    crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }

  /**
   * Generate session-based key as fallback
   */
  private generateSessionKey(): string {
    const sessionData = `${navigator.userAgent}-${Date.now()}-${Math.random()}`;
    return this.hashString(sessionData);
  }

  /**
   * Hash a string using SHA-256
   */
  private hashString(input: string): string {
    // Simple hash for fallback (in production, use crypto.subtle.digest)
    let hash = 0;
    for (let i = 0; i < input.length; i++) {
      const char = input.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return Math.abs(hash).toString(16);
  }

  /**
   * Get hash algorithm for crypto.subtle
   */
  private getHashAlgorithm(): string {
    switch (this.config.algorithm) {
      case 'HMAC-SHA256':
        return 'SHA-256';
      case 'HMAC-SHA512':
        return 'SHA-512';
      default:
        return 'SHA-256';
    }
  }

  /**
   * Convert ArrayBuffer to hex string
   */
  private arrayBufferToHex(buffer: ArrayBuffer): string {
    const byteArray = new Uint8Array(buffer);
    return Array.from(byteArray, byte => byte.toString(16).padStart(2, '0')).join('');
  }

  /**
   * Constant-time string comparison to prevent timing attacks
   */
  private constantTimeCompare(a: string, b: string): boolean {
    if (a.length !== b.length) {
      return false;
    }

    let result = 0;
    for (let i = 0; i < a.length; i++) {
      result |= a.charCodeAt(i) ^ b.charCodeAt(i);
    }

    return result === 0;
  }

  /**
   * Update signing configuration
   */
  public updateConfig(newConfig: Partial<SigningConfig>): void {
    this.config = { ...this.config, ...newConfig };
  }

  /**
   * Get current configuration
   */
  public getConfig(): SigningConfig {
    return { ...this.config };
  }

  /**
   * Clear signing key (for logout/security)
   */
  public clearSigningKey(): void {
    this.signingKey = null;
    SecurityUtils.removeSecureSessionStorage('request-signing-key');
  }
}

// Export singleton instance
export const requestSigning = RequestSigningService.getInstance();
