import React from 'react';
import { render, screen, fireEvent, waitFor } from '../test-utils';
import userEvent from '@testing-library/user-event';
import App from '../App';
import { useAuthStore } from '../stores/authStore';

// Mock the chart components to avoid D3 rendering issues in tests
jest.mock('../components/Visualization/D3Charts/HeatmapChart', () => ({
  HeatmapChart: () => <div data-testid="heatmap-chart">Heatmap Chart</div>
}));

jest.mock('../components/Visualization/D3Charts/TreemapChart', () => ({
  TreemapChart: () => <div data-testid="treemap-chart">Treemap Chart</div>
}));

jest.mock('../components/Visualization/D3Charts/NetworkChart', () => ({
  NetworkChart: () => <div data-testid="network-chart">Network Chart</div>
}));

describe('App Integration Tests', () => {
  beforeEach(() => {
    // Clear auth state before each test
    useAuthStore.getState().logout();
  });

  it('should show login page when not authenticated', () => {
    render(<App />);
    
    expect(screen.getByText(/sign in/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('should login and redirect to main app', async () => {
    const user = userEvent.setup();
    render(<App />);
    
    // Fill in login form
    await user.type(screen.getByLabelText(/username/i), 'testuser');
    await user.type(screen.getByLabelText(/password/i), 'password');
    
    // Submit form
    await user.click(screen.getByRole('button', { name: /sign in/i }));
    
    // Wait for redirect to main app
    await waitFor(() => {
      expect(screen.queryByText(/sign in/i)).not.toBeInTheDocument();
    });
    
    // Should see main navigation
    expect(screen.getByText(/query interface/i)).toBeInTheDocument();
  });

  it('should show error message on login failure', async () => {
    const user = userEvent.setup();
    
    // Mock login failure
    const originalLogin = useAuthStore.getState().login;
    useAuthStore.setState({
      login: jest.fn().mockResolvedValue(false)
    });
    
    render(<App />);
    
    await user.type(screen.getByLabelText(/username/i), 'wronguser');
    await user.type(screen.getByLabelText(/password/i), 'wrongpassword');
    await user.click(screen.getByRole('button', { name: /sign in/i }));
    
    await waitFor(() => {
      expect(screen.getByText(/login failed/i)).toBeInTheDocument();
    });
    
    // Restore original login function
    useAuthStore.setState({ login: originalLogin });
  });

  it('should navigate between different pages when authenticated', async () => {
    const user = userEvent.setup();
    
    // Set authenticated state
    useAuthStore.setState({
      isAuthenticated: true,
      user: {
        id: '1',
        username: 'testuser',
        displayName: 'Test User',
        email: 'test@example.com',
        roles: ['user']
      },
      token: 'mock-token',
      refreshToken: 'mock-refresh-token'
    });
    
    render(<App />);
    
    // Should be on main page
    expect(screen.getByText(/query interface/i)).toBeInTheDocument();
    
    // Navigate to enhanced query builder
    const enhancedQueryLink = screen.getByText(/enhanced query/i);
    await user.click(enhancedQueryLink);
    
    await waitFor(() => {
      expect(window.location.pathname).toBe('/enhanced-query');
    });
    
    // Navigate to AI profile
    const aiProfileLink = screen.getByText(/ai profile/i);
    await user.click(aiProfileLink);
    
    await waitFor(() => {
      expect(window.location.pathname).toBe('/ai-profile');
    });
  });

  it('should show dev tools in development mode', () => {
    // Ensure we're in development mode
    const originalEnv = process.env.NODE_ENV;
    process.env.NODE_ENV = 'development';
    
    useAuthStore.setState({
      isAuthenticated: true,
      user: {
        id: '1',
        username: 'testuser',
        displayName: 'Test User',
        email: 'test@example.com',
        roles: ['user']
      }
    });
    
    render(<App />);
    
    // Should see dev tools button
    const devToolsButton = screen.getByRole('button', { name: /bug/i });
    expect(devToolsButton).toBeInTheDocument();
    
    // Restore original environment
    process.env.NODE_ENV = originalEnv;
  });

  it('should handle logout', async () => {
    const user = userEvent.setup();
    
    useAuthStore.setState({
      isAuthenticated: true,
      user: {
        id: '1',
        username: 'testuser',
        displayName: 'Test User',
        email: 'test@example.com',
        roles: ['user']
      }
    });
    
    render(<App />);
    
    // Find and click logout button (assuming it exists in the layout)
    const logoutButton = screen.getByText(/logout/i);
    await user.click(logoutButton);
    
    // Should redirect to login page
    await waitFor(() => {
      expect(screen.getByText(/sign in/i)).toBeInTheDocument();
    });
  });

  it('should handle error boundaries', () => {
    // Mock console.error to avoid noise in test output
    const originalError = console.error;
    console.error = jest.fn();
    
    // Create a component that throws an error
    const ThrowError = () => {
      throw new Error('Test error');
    };
    
    // Mock a route that throws an error
    jest.doMock('../components/QueryInterface/QueryInterface', () => ({
      QueryInterface: ThrowError
    }));
    
    useAuthStore.setState({
      isAuthenticated: true,
      user: {
        id: '1',
        username: 'testuser',
        displayName: 'Test User',
        email: 'test@example.com',
        roles: ['user']
      }
    });
    
    render(<App />);
    
    // Should show error boundary fallback
    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
    
    // Restore console.error
    console.error = originalError;
  });

  it('should handle route not found', () => {
    useAuthStore.setState({
      isAuthenticated: true,
      user: {
        id: '1',
        username: 'testuser',
        displayName: 'Test User',
        email: 'test@example.com',
        roles: ['user']
      }
    });
    
    // Navigate to non-existent route
    window.history.pushState({}, 'Test page', '/non-existent-route');
    
    render(<App />);
    
    // Should redirect to home page
    expect(window.location.pathname).toBe('/');
  });

  it('should lazy load components', async () => {
    useAuthStore.setState({
      isAuthenticated: true,
      user: {
        id: '1',
        username: 'testuser',
        displayName: 'Test User',
        email: 'test@example.com',
        roles: ['user']
      }
    });
    
    render(<App />);
    
    // Should show loading fallback initially
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
    
    // Wait for component to load
    await waitFor(() => {
      expect(screen.queryByText(/loading/i)).not.toBeInTheDocument();
    });
  });
});
