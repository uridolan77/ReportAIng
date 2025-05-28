import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { API_CONFIG } from '../config/api';
import { setSessionId, clearSessionId } from '../utils/sessionUtils';
import { SecurityUtils } from '../utils/security';
import { apiClient } from '../services/apiClient';
import { tokenManager } from '../services/tokenManager';

interface User {
  id: string;
  username: string;
  displayName: string;
  email: string;
  roles: string[];
}

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
  refreshAuth: () => Promise<boolean>;
  getDecryptedToken: () => Promise<string | null>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      isAuthenticated: false,
      user: null,
      token: null,
      refreshToken: null,

      login: async (username: string, password: string) => {
        try {
          // Validate inputs
          if (!SecurityUtils.validateInput(username, 'username')) {
            throw new Error('Invalid username format');
          }

          const response = await apiClient.post<{
            success: boolean;
            accessToken: string;
            refreshToken: string;
            user: User;
          }>(API_CONFIG.ENDPOINTS.AUTH.LOGIN, {
            username,
            password
          });

          if ((response as any).success) {
            // Encrypt tokens before storing (now async)
            const encryptedToken = await SecurityUtils.encryptToken((response as any).accessToken);
            const encryptedRefreshToken = await SecurityUtils.encryptToken((response as any).refreshToken);

            set({
              isAuthenticated: true,
              user: (response as any).user,
              token: encryptedToken,
              refreshToken: encryptedRefreshToken,
            });

            // Create a secure session ID
            const secureSessionId = SecurityUtils.generateSecureSessionId();
            setSessionId(secureSessionId);

            // Store session data securely
            SecurityUtils.setSecureSessionStorage('user-session', JSON.stringify({
              userId: (response as any).user.id,
              sessionId: secureSessionId,
              loginTime: Date.now()
            }));

            return true;
          }
          return false;
        } catch (error) {
          console.error('Login failed:', error);
          return false;
        }
      },

      logout: () => {
        // Clean up token manager
        tokenManager.destroy();

        set({
          isAuthenticated: false,
          user: null,
          token: null,
          refreshToken: null,
        });

        // Clear the session ID when logging out
        clearSessionId();

        // Clear all storage
        localStorage.removeItem('auth-storage');
        sessionStorage.clear();
      },

      refreshAuth: async () => {
        const { refreshToken } = get();
        if (!refreshToken) return false;

        try {
          const decryptedRefreshToken = SecurityUtils.decryptToken(refreshToken);
          const response = await apiClient.post<{
            success: boolean;
            accessToken: string;
            refreshToken: string;
          }>(API_CONFIG.ENDPOINTS.AUTH.REFRESH, {
            refreshToken: decryptedRefreshToken
          });

          if ((response as any).success) {
            // Encrypt new tokens
            const encryptedToken = await SecurityUtils.encryptToken((response as any).accessToken);
            const encryptedRefreshToken = await SecurityUtils.encryptToken((response as any).refreshToken);

            set({
              token: encryptedToken,
              refreshToken: encryptedRefreshToken,
            });
            return true;
          } else {
            // Token refresh failed, logout
            get().logout();
            return false;
          }
        } catch (error) {
          console.error('Token refresh error:', error);
          get().logout();
          return false;
        }
      },

      getDecryptedToken: async () => {
        const { token } = get();
        return token ? await SecurityUtils.decryptToken(token) : null;
      }
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        isAuthenticated: state.isAuthenticated,
        user: state.user,
        token: state.token,
        refreshToken: state.refreshToken,
      }),
    }
  )
);
