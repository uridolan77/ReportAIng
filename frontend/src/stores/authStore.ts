import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { API_CONFIG, getApiUrl, getAuthHeaders } from '../config/api';
import { setSessionId, clearSessionId, generateSessionId } from '../utils/sessionUtils';

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
          const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.AUTH.LOGIN), {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({ username, password }),
          });

          if (response.ok) {
            const data = await response.json();
            set({
              isAuthenticated: true,
              user: data.user,
              token: data.accessToken,
              refreshToken: data.refreshToken,
            });
            // Create a new session ID for the authenticated user
            setSessionId(generateSessionId());
            return true;
          } else {
            console.error('Login failed:', response.status, response.statusText);
            return false;
          }
        } catch (error) {
          console.error('Login error:', error);
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
          const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.AUTH.REFRESH), {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({ refreshToken }),
          });

          if (response.ok) {
            const data = await response.json();
            set({
              token: data.accessToken,
              refreshToken: data.refreshToken,
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
