import { SecurityUtils } from '../utils/security';
import { ApiService } from './api';
import { API_CONFIG } from '../config/api';

interface TokenPair {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

interface TokenMetadata {
  issuedAt: number;
  expiresAt: number;
  rotationCount: number;
  lastUsed: number;
}

class TokenManager {
  private refreshTokenRotationEnabled = true;
  private tokenExpiryBuffer = 60000; // 1 minute buffer
  private maxRotationCount = 10; // Prevent infinite rotation
  private refreshPromise: Promise<TokenPair> | null = null;
  private tokenMetadata: Map<string, TokenMetadata> = new Map();

  async refreshAccessToken(): Promise<TokenPair> {
    // Prevent concurrent refresh attempts
    if (this.refreshPromise) {
      return this.refreshPromise;
    }

    this.refreshPromise = this.performTokenRefresh();

    try {
      const result = await this.refreshPromise;
      return result;
    } finally {
      this.refreshPromise = null;
    }
  }

  private async performTokenRefresh(): Promise<TokenPair> {
    const currentRefreshToken = await this.getRefreshToken();

    if (!currentRefreshToken) {
      throw new Error('No refresh token available');
    }

    // Check rotation count to prevent abuse
    const metadata = this.tokenMetadata.get(currentRefreshToken);
    if (metadata && metadata.rotationCount >= this.maxRotationCount) {
      throw new Error('Maximum token rotation count exceeded');
    }

    try {
      const response = await ApiService.post<{
        success: boolean;
        accessToken: string;
        refreshToken: string;
        expiresIn: number;
      }>(API_CONFIG.ENDPOINTS.AUTH.REFRESH, {
        refreshToken: currentRefreshToken,
        rotateToken: this.refreshTokenRotationEnabled
      });

      if (!response.success) {
        throw new Error('Token refresh failed');
      }

      const { accessToken, refreshToken, expiresIn } = response;

      // Store new tokens securely
      await this.storeTokens({ accessToken, refreshToken, expiresIn });

      // Update metadata
      const now = Date.now();
      this.tokenMetadata.set(refreshToken, {
        issuedAt: now,
        expiresAt: now + (expiresIn * 1000),
        rotationCount: (metadata?.rotationCount || 0) + 1,
        lastUsed: now
      });

      // Clean up old metadata
      this.tokenMetadata.delete(currentRefreshToken);

      // Schedule next refresh
      this.scheduleTokenRefresh(expiresIn - this.tokenExpiryBuffer);

      console.log('Token refreshed successfully');
      return { accessToken, refreshToken, expiresIn };
    } catch (error) {
      console.error('Token refresh failed:', error);
      await this.handleRefreshFailure(error);
      throw error;
    }
  }

  private async storeTokens(tokens: TokenPair): Promise<void> {
    try {
      // Encrypt tokens before storing
      const encryptedAccessToken = await SecurityUtils.encryptToken(tokens.accessToken);
      const encryptedRefreshToken = await SecurityUtils.encryptToken(tokens.refreshToken);

      // Store in secure storage with integrity checks
      const tokenData = {
        accessToken: encryptedAccessToken,
        refreshToken: encryptedRefreshToken,
        expiresIn: tokens.expiresIn,
        storedAt: Date.now(),
        integrity: await this.generateTokenIntegrity(tokens)
      };

      SecurityUtils.setSecureSessionStorage('secure-tokens', JSON.stringify(tokenData));

      // Also update the auth store
      const { useAuthStore } = await import('../stores/authStore');
      const authStore = useAuthStore.getState();
      authStore.refreshAuth();

    } catch (error) {
      console.error('Failed to store tokens:', error);
      throw new Error('Token storage failed');
    }
  }

  private async getRefreshToken(): Promise<string | null> {
    try {
      const tokenData = SecurityUtils.getSecureSessionStorage('secure-tokens');
      if (!tokenData) {
        // Fallback to auth store
        const { useAuthStore } = await import('../stores/authStore');
        const authStore = useAuthStore.getState();
        return authStore.refreshToken ? await SecurityUtils.decryptToken(authStore.refreshToken) : null;
      }

      const parsed = JSON.parse(tokenData);

      // Verify integrity
      const isValid = await this.verifyTokenIntegrity(parsed);
      if (!isValid) {
        console.warn('Token integrity check failed');
        this.clearTokens();
        return null;
      }

      return await SecurityUtils.decryptToken(parsed.refreshToken);
    } catch (error) {
      console.error('Failed to get refresh token:', error);
      return null;
    }
  }

  private async generateTokenIntegrity(tokens: TokenPair): Promise<string> {
    const data = `${tokens.accessToken}:${tokens.refreshToken}:${tokens.expiresIn}`;
    const encoder = new TextEncoder();
    const dataBuffer = encoder.encode(data);
    const hashBuffer = await crypto.subtle.digest('SHA-256', dataBuffer);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  }

  private async verifyTokenIntegrity(tokenData: any): Promise<boolean> {
    try {
      const decryptedAccess = await SecurityUtils.decryptToken(tokenData.accessToken);
      const decryptedRefresh = await SecurityUtils.decryptToken(tokenData.refreshToken);

      const expectedIntegrity = await this.generateTokenIntegrity({
        accessToken: decryptedAccess,
        refreshToken: decryptedRefresh,
        expiresIn: tokenData.expiresIn
      });

      return expectedIntegrity === tokenData.integrity;
    } catch (error) {
      console.error('Integrity verification failed:', error);
      return false;
    }
  }

  private scheduleTokenRefresh(delay: number): void {
    // Clear any existing timeout
    if (this.refreshTimeoutId) {
      clearTimeout(this.refreshTimeoutId);
    }

    // Ensure delay is positive and reasonable
    const safeDelay = Math.max(delay, 30000); // Minimum 30 seconds

    this.refreshTimeoutId = setTimeout(() => {
      this.refreshAccessToken().catch(error => {
        console.error('Scheduled token refresh failed:', error);
        this.handleRefreshFailure(error);
      });
    }, safeDelay);
  }

  private refreshTimeoutId: NodeJS.Timeout | null = null;

  private async handleRefreshFailure(error: any): Promise<void> {
    console.error('Token refresh failure:', error);

    // Clear stored tokens
    this.clearTokens();

    // Clear metadata
    this.tokenMetadata.clear();

    // Notify auth store to logout
    try {
      const { useAuthStore } = await import('../stores/authStore');
      const authStore = useAuthStore.getState();
      authStore.logout();
    } catch (importError) {
      console.error('Failed to import auth store:', importError);
    }

    // Redirect to login
    window.location.href = '/login';
  }

  private clearTokens(): void {
    try {
      SecurityUtils.clearSecureSessionStorage('secure-tokens');
      localStorage.removeItem('auth-storage');
      sessionStorage.removeItem('secure-tokens');
    } catch (error) {
      console.error('Failed to clear tokens:', error);
    }
  }

  // Check if token needs refresh
  async shouldRefreshToken(): Promise<boolean> {
    try {
      const tokenData = SecurityUtils.getSecureSessionStorage('secure-tokens');
      if (!tokenData) return false;

      const parsed = JSON.parse(tokenData);
      const now = Date.now();
      const expiresAt = parsed.storedAt + (parsed.expiresIn * 1000);

      // Refresh if token expires within the buffer time
      return (expiresAt - now) <= this.tokenExpiryBuffer;
    } catch (error) {
      console.error('Error checking token expiry:', error);
      return false;
    }
  }

  // Get current access token
  async getAccessToken(): Promise<string | null> {
    try {
      const tokenData = SecurityUtils.getSecureSessionStorage('secure-tokens');
      if (!tokenData) {
        // Fallback to auth store
        const { useAuthStore } = await import('../stores/authStore');
        const authStore = useAuthStore.getState();
        return authStore.getDecryptedToken();
      }

      const parsed = JSON.parse(tokenData);

      // Verify integrity
      const isValid = await this.verifyTokenIntegrity(parsed);
      if (!isValid) {
        console.warn('Access token integrity check failed');
        this.clearTokens();
        return null;
      }

      return await SecurityUtils.decryptToken(parsed.accessToken);
    } catch (error) {
      console.error('Failed to get access token:', error);
      return null;
    }
  }

  // Initialize token manager
  async initialize(): Promise<void> {
    try {
      // Check if we need to refresh on startup
      const shouldRefresh = await this.shouldRefreshToken();
      if (shouldRefresh) {
        await this.refreshAccessToken();
      }

      console.log('Token manager initialized');
    } catch (error) {
      console.error('Token manager initialization failed:', error);
    }
  }

  // Cleanup
  destroy(): void {
    if (this.refreshTimeoutId) {
      clearTimeout(this.refreshTimeoutId);
      this.refreshTimeoutId = null;
    }
    this.tokenMetadata.clear();
    this.refreshPromise = null;
  }
}

export const tokenManager = new TokenManager();
