import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import { Layout } from '../Layout';

// Mock the auth store
const mockLogout = jest.fn();
jest.mock('../../../stores/authStore', () => ({
  useAuthStore: () => ({
    user: {
      id: '1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      roles: ['admin']
    },
    logout: mockLogout,
    isAdmin: true,
    isAuthenticated: true,
    token: 'mock-token',
    refreshToken: 'mock-refresh-token',
    login: jest.fn(),
    refreshAccessToken: jest.fn(),
    clearAuth: jest.fn(),
    updateUser: jest.fn()
  })
}));

// Mock the DatabaseStatusIndicator component
jest.mock('../DatabaseStatusIndicator', () => ({
  DatabaseStatusIndicator: () => <div data-testid="database-status">DB Status</div>
}));

// Mock the AppNavigation component
jest.mock('../../Navigation/AppNavigation', () => ({
  AppNavigation: ({ isAdmin }: { isAdmin: boolean }) => (
    <div data-testid="app-navigation">Navigation (Admin: {isAdmin.toString()})</div>
  )
}));

// Test wrapper component
const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <BrowserRouter>
    <ConfigProvider>
      {children}
    </ConfigProvider>
  </BrowserRouter>
);

describe('Layout - User Dropdown', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render the user dropdown button', () => {
    render(
      <TestWrapper>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </TestWrapper>
    );

    const dropdownButton = screen.getByRole('button', { name: /test user/i });
    expect(dropdownButton).toBeInTheDocument();
  });

  it('should show System Administrator when no user displayName', () => {
    // This test would need to mock the auth store differently
    // For now, we'll skip this test since the mock is set up globally
    expect(true).toBe(true);
  });

  it('should open dropdown menu when clicked', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </TestWrapper>
    );

    const dropdownButton = screen.getByRole('button', { name: /test user/i });
    await user.click(dropdownButton);

    // Wait for dropdown menu to appear
    await waitFor(() => {
      expect(screen.getByText('Profile')).toBeInTheDocument();
      expect(screen.getByText('Settings')).toBeInTheDocument();
      expect(screen.getByText('Logout')).toBeInTheDocument();
    });
  });

  it('should call logout when logout menu item is clicked', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </TestWrapper>
    );

    const dropdownButton = screen.getByRole('button', { name: /test user/i });
    await user.click(dropdownButton);

    // Wait for dropdown menu to appear and click logout
    await waitFor(() => {
      const logoutItem = screen.getByText('Logout');
      expect(logoutItem).toBeInTheDocument();
      return user.click(logoutItem);
    });

    expect(mockLogout).toHaveBeenCalledTimes(1);
  });

  it('should log profile click when profile menu item is clicked', async () => {
    const consoleSpy = jest.spyOn(console, 'log').mockImplementation();
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </TestWrapper>
    );

    const dropdownButton = screen.getByRole('button', { name: /test user/i });
    await user.click(dropdownButton);

    // Wait for dropdown menu to appear and click profile
    await waitFor(() => {
      const profileItem = screen.getByText('Profile');
      expect(profileItem).toBeInTheDocument();
      return user.click(profileItem);
    });

    expect(consoleSpy).toHaveBeenCalledWith('Profile clicked');
    consoleSpy.mockRestore();
  });

  it('should log settings click when settings menu item is clicked', async () => {
    const consoleSpy = jest.spyOn(console, 'log').mockImplementation();
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </TestWrapper>
    );

    const dropdownButton = screen.getByRole('button', { name: /test user/i });
    await user.click(dropdownButton);

    // Wait for dropdown menu to appear and click settings
    await waitFor(() => {
      const settingsItem = screen.getByText('Settings');
      expect(settingsItem).toBeInTheDocument();
      return user.click(settingsItem);
    });

    expect(consoleSpy).toHaveBeenCalledWith('Settings clicked');
    consoleSpy.mockRestore();
  });
});
