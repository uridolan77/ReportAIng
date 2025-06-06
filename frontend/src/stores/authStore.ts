import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { API_CONFIG } from '../config/api';
import { setSessionId, clearSessionId } from '../utils/sessionUtils';
import { SecurityUtils } from '../utils/security';
import { ApiService } from '../services/api';
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
  isAdmin: boolean;
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
      isAdmin: false,

      login: async (username: string, password: string) => {
        try {
          // Validate inputs
          if (!SecurityUtils.validateInput(username, 'username')) {
            throw new Error('Invalid username format');
          }

          const response = await ApiService.login({
            username,
            password
          });

          if (process.env.NODE_ENV === 'development') {
            console.log('🔐 Full login response:', response);
          }

          if (response.success) {
            // Backend now returns camelCase properties due to JSON serialization fix
            console.log('🔐 Login response:', {
              hasAccessToken: !!response.accessToken,
              hasRefreshToken: !!response.refreshToken,
              user: response.user
            });

            // Encrypt tokens before storing (now async)
            const accessToken = response.accessToken || '';
            const refreshToken = response.refreshToken || '';

            if (!accessToken) {
              console.error('❌ No access token received from backend');
              return false;
            }

            const encryptedToken = await SecurityUtils.encryptToken(accessToken);
            const encryptedRefreshToken = await SecurityUtils.encryptToken(refreshToken);

            // Backend now returns camelCase user object
            const userFromResponse = response.user;

            // User object is already in camelCase format
            const transformedUser = userFromResponse ? {
              id: userFromResponse.id,
              username: userFromResponse.username,
              displayName: userFromResponse.displayName,
              email: userFromResponse.email,
              roles: userFromResponse.roles || []
            } : null;

            console.log('🔐 Transformed user:', transformedUser);
            console.log('🔐 Is admin check:', transformedUser?.roles?.includes('Admin'));

            set({
              isAuthenticated: true,
              user: transformedUser,
              token: encryptedToken,
              refreshToken: encryptedRefreshToken,
              isAdmin: transformedUser?.roles?.includes('Admin') || transformedUser?.roles?.includes('admin') || false,
            });

            console.log('✅ Auth state updated successfully');
            console.log('✅ Current state after update:', get());

            // Create a secure session ID
            const secureSessionId = SecurityUtils.generateSecureSessionId();
            setSessionId(secureSessionId);

            // Store session data securely
            SecurityUtils.setSecureSessionStorage('user-session', JSON.stringify({
              userId: transformedUser?.id,
              sessionId: secureSessionId,
              loginTime: Date.now()
            }));

            // Force a small delay to ensure state is persisted
            await new Promise(resolve => setTimeout(resolve, 100));

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
          isAdmin: false,
        });

        // Clear the session ID when logging out
        clearSessionId();

        // Clear all storage
        localStorage.removeItem('auth-storage');
        sessionStorage.clear();

        // Force clear any other auth-related storage
        Object.keys(localStorage).forEach(key => {
          if (key.includes('auth') || key.includes('user') || key.includes('token')) {
            localStorage.removeItem(key);
          }
        });


      },

      refreshAuth: async () => {
        const { refreshToken } = get();
        if (!refreshToken) return false;

        try {
          const decryptedRefreshToken = await SecurityUtils.decryptToken(refreshToken);
          // Note: ApiService doesn't have a refresh method, so we'll use the axios instance directly
          const response = await fetch(`${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.AUTH.REFRESH}`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              refreshToken: decryptedRefreshToken
            })
          });

          const data = await response.json();

          if (data.success) {
            // Backend now returns camelCase properties
            // Encrypt new tokens
            const encryptedToken = await SecurityUtils.encryptToken(data.accessToken);
            const encryptedRefreshToken = await SecurityUtils.encryptToken(data.refreshToken);

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
        isAdmin: state.isAdmin,
      }),
    }
  )
);
