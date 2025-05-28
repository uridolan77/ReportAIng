import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { API_CONFIG, getApiUrl, getAuthHeaders } from '../config/api';
import { setSessionId, clearSessionId, generateSessionId } from '../utils/sessionUtils';
import { SecurityUtils } from '../utils/security';
import { apiClient } from '../services/apiClient';

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
  getDecryptedToken: () => string | null;
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

          if (response.success) {
            // Encrypt tokens before storing
            const encryptedToken = SecurityUtils.encryptToken(response.accessToken);
            const encryptedRefreshToken = SecurityUtils.encryptToken(response.refreshToken);

            set({
              isAuthenticated: true,
              user: response.user,
              token: encryptedToken,
              refreshToken: encryptedRefreshToken,
            });

            // Create a new session ID for the authenticated user
            setSessionId(generateSessionId());
            return true;
          }
          return false;
        } catch (error) {
          console.error('Login failed:', error);
          return false;
        }
      },

      logout: () => {
        set({
          isAuthenticated: false,
          user: null,
          token: null,
          refreshToken: null,
        });
        // Clear the session ID when logging out
        clearSessionId();
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

          if (response.success) {
            // Encrypt new tokens
            const encryptedToken = SecurityUtils.encryptToken(response.accessToken);
            const encryptedRefreshToken = SecurityUtils.encryptToken(response.refreshToken);

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

      getDecryptedToken: () => {
        const { token } = get();
        return token ? SecurityUtils.decryptToken(token) : null;
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
