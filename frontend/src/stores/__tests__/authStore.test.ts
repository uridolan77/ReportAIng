import { renderHook, act } from '@testing-library/react';
import { useAuthStore } from '../authStore';
import { server } from '../../mocks/server';
import { rest } from 'msw';

// Mock the session utils
jest.mock('../../utils/sessionUtils', () => ({
  setSessionId: jest.fn(),
  clearSessionId: jest.fn(),
  generateSessionId: jest.fn(() => 'mock-session-id')
}));

// Mock the security utils
jest.mock('../../utils/security', () => ({
  SecurityUtils: {
    validateInput: jest.fn(() => true),
    encryptToken: jest.fn((token) => `encrypted-${token}`),
    decryptToken: jest.fn((token) => token.replace('encrypted-', ''))
  }
}));

describe('Auth Store', () => {
  beforeEach(() => {
    // Clear the store state before each test
    useAuthStore.getState().logout();
    localStorage.clear();
  });

  it('should have initial state', () => {
    const { result } = renderHook(() => useAuthStore());
    
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBe(null);
    expect(result.current.token).toBe(null);
    expect(result.current.refreshToken).toBe(null);
  });

  it('should login successfully', async () => {
    const { result } = renderHook(() => useAuthStore());
    
    await act(async () => {
      const success = await result.current.login('testuser', 'password');
      expect(success).toBe(true);
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).toEqual({
      id: '1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      roles: ['user']
    });
    expect(result.current.token).toBe('encrypted-mock-access-token');
    expect(result.current.refreshToken).toBe('encrypted-mock-refresh-token');
  });

  it('should handle login failure', async () => {
    // Override the login endpoint to return an error
    server.use(
      rest.post('/api/auth/login', (req, res, ctx) => {
        return res(
          ctx.status(401),
          ctx.json({
            success: false,
            error: 'Invalid credentials'
          })
        );
      })
    );

    const { result } = renderHook(() => useAuthStore());
    
    await act(async () => {
      const success = await result.current.login('wronguser', 'wrongpassword');
      expect(success).toBe(false);
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBe(null);
  });

  it('should logout successfully', () => {
    const { result } = renderHook(() => useAuthStore());
    
    // First login
    act(() => {
      useAuthStore.setState({
        isAuthenticated: true,
        user: { id: '1', username: 'test', displayName: 'Test', email: 'test@test.com', roles: [] },
        token: 'test-token',
        refreshToken: 'test-refresh-token'
      });
    });

    expect(result.current.isAuthenticated).toBe(true);

    // Then logout
    act(() => {
      result.current.logout();
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBe(null);
    expect(result.current.token).toBe(null);
    expect(result.current.refreshToken).toBe(null);
  });

  it('should refresh auth token successfully', async () => {
    const { result } = renderHook(() => useAuthStore());
    
    // Set initial state with refresh token
    act(() => {
      useAuthStore.setState({
        isAuthenticated: true,
        refreshToken: 'encrypted-mock-refresh-token'
      });
    });

    await act(async () => {
      const success = await result.current.refreshAuth();
      expect(success).toBe(true);
    });

    expect(result.current.token).toBe('encrypted-new-mock-access-token');
    expect(result.current.refreshToken).toBe('encrypted-new-mock-refresh-token');
  });

  it('should handle refresh token failure', async () => {
    // Override the refresh endpoint to return an error
    server.use(
      rest.post('/api/auth/refresh', (req, res, ctx) => {
        return res(
          ctx.status(401),
          ctx.json({
            success: false,
            error: 'Invalid refresh token'
          })
        );
      })
    );

    const { result } = renderHook(() => useAuthStore());
    
    // Set initial state with refresh token
    act(() => {
      useAuthStore.setState({
        isAuthenticated: true,
        refreshToken: 'encrypted-invalid-refresh-token'
      });
    });

    await act(async () => {
      const success = await result.current.refreshAuth();
      expect(success).toBe(false);
    });

    // Should be logged out after failed refresh
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBe(null);
    expect(result.current.token).toBe(null);
    expect(result.current.refreshToken).toBe(null);
  });

  it('should return null when no refresh token available', async () => {
    const { result } = renderHook(() => useAuthStore());
    
    await act(async () => {
      const success = await result.current.refreshAuth();
      expect(success).toBe(false);
    });
  });

  it('should get decrypted token', () => {
    const { result } = renderHook(() => useAuthStore());
    
    act(() => {
      useAuthStore.setState({
        token: 'encrypted-test-token'
      });
    });

    const decryptedToken = result.current.getDecryptedToken();
    expect(decryptedToken).toBe('test-token');
  });

  it('should return null for decrypted token when no token exists', () => {
    const { result } = renderHook(() => useAuthStore());
    
    const decryptedToken = result.current.getDecryptedToken();
    expect(decryptedToken).toBe(null);
  });

  it('should validate username input during login', async () => {
    const SecurityUtils = require('../../utils/security').SecurityUtils;
    SecurityUtils.validateInput.mockReturnValueOnce(false);

    const { result } = renderHook(() => useAuthStore());
    
    await act(async () => {
      const success = await result.current.login('invalid-username!', 'password');
      expect(success).toBe(false);
    });

    expect(SecurityUtils.validateInput).toHaveBeenCalledWith('invalid-username!', 'username');
  });

  it('should persist state to localStorage', async () => {
    const { result } = renderHook(() => useAuthStore());
    
    await act(async () => {
      await result.current.login('testuser', 'password');
    });

    const storedState = localStorage.getItem('auth-storage');
    expect(storedState).toBeTruthy();
    
    const parsedState = JSON.parse(storedState!);
    expect(parsedState.state.isAuthenticated).toBe(true);
    expect(parsedState.state.user.username).toBe('testuser');
  });
});
