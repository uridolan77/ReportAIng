/**
 * Security and Error Handling Integration Tests
 * 
 * Comprehensive tests for security features, error boundaries,
 * input validation, and graceful error recovery.
 */

import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { IntegrationTestFramework } from '../../test-utils/integration/IntegrationTestFramework';
import App from '../../App';

describe('Security and Error Handling Integration Tests', () => {
  let framework: IntegrationTestFramework;

  beforeEach(() => {
    framework = new IntegrationTestFramework({
      mockApi: true,
      mockAuth: true,
      userRole: 'user',
      enablePerformanceMonitoring: true,
    });
  });

  describe('Input Validation and XSS Protection', () => {
    it('should prevent XSS attacks in query inputs', async () => {
      const result = await framework.runIntegrationTest(
        'XSS Protection Test',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Navigate to Query page
          await fw.simulateUserInteraction('Navigate to Query Page', async () => {
            const user = userEvent.setup();
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);
          });

          // Step 2: Test XSS injection attempts
          await fw.executeStep('Test XSS Injection Prevention', async () => {
            const user = userEvent.setup();
            
            const maliciousInputs = [
              '<script>alert("XSS")</script>',
              'data:text/html,<script>alert("XSS")</script>', // XSS test case
              '<img src="x" onerror="alert(\'XSS\')">',
              '<svg onload="alert(\'XSS\')">',
              '"><script>alert("XSS")</script>',
              '\'; DROP TABLE users; --',
            ];

            const queryInput = screen.getByPlaceholderText(/ask a question/i);

            for (const maliciousInput of maliciousInputs) {
              await user.clear(queryInput);
              await user.type(queryInput, maliciousInput);
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.click(submitButton);

              // Verify input is sanitized
              await waitFor(() => {
                expect(screen.getByText(/invalid input detected/i)).toBeInTheDocument();
              });

              // Verify no script execution
              expect(window.alert).not.toHaveBeenCalled();
              
              // Clear error for next test
              const dismissButton = screen.getByText('Dismiss');
              await user.click(dismissButton);
            }
          });

          // Step 3: Test SQL injection prevention
          await fw.executeStep('Test SQL Injection Prevention', async () => {
            const user = userEvent.setup();
            
            const sqlInjectionAttempts = [
              "'; DROP TABLE users; --",
              "' OR '1'='1",
              "' UNION SELECT * FROM passwords --",
              "'; DELETE FROM users WHERE '1'='1'; --",
            ];

            const queryInput = screen.getByPlaceholderText(/ask a question/i);

            for (const injection of sqlInjectionAttempts) {
              await user.clear(queryInput);
              await user.type(queryInput, `Show users where name = '${injection}'`);
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.click(submitButton);

              // Verify SQL injection is blocked
              await waitFor(() => {
                expect(screen.getByText(/potentially dangerous query detected/i)).toBeInTheDocument();
              });

              const dismissButton = screen.getByText('Dismiss');
              await user.click(dismissButton);
            }
          });

          // Step 4: Test valid input acceptance
          await fw.executeStep('Test Valid Input Acceptance', async () => {
            const user = userEvent.setup();
            
            const validInputs = [
              'Show me all users',
              'What is the total sales for last month?',
              'Display top 10 products by revenue',
            ];

            const queryInput = screen.getByPlaceholderText(/ask a question/i);

            for (const validInput of validInputs) {
              await user.clear(queryInput);
              await user.type(queryInput, validInput);
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.click(submitButton);

              // Mock successful response
              const mockResponse = {
                success: true,
                data: [{ id: 1, name: 'Test' }],
                query: 'SELECT * FROM test',
              };

              await fw.simulateApiCall('/api/query/execute', mockResponse, 100);

              // Verify valid input is processed
              await waitFor(() => {
                expect(screen.getByText('Query executed successfully')).toBeInTheDocument();
              });
            }
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ XSS Protection Test Passed');
    });

    it('should handle CSRF protection', async () => {
      const result = await framework.runIntegrationTest(
        'CSRF Protection Test',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Test CSRF token validation
          await fw.executeStep('Test CSRF Token Validation', async () => {
            // Mock API call without CSRF token
            const mockCSRFError = {
              success: false,
              error: 'CSRF token missing or invalid',
              code: 'CSRF_ERROR',
            };

            await fw.simulateApiCall('/api/query/execute', mockCSRFError, 100);

            await waitFor(() => {
              expect(screen.getByText(/security error/i)).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText(/csrf token/i)).toBeInTheDocument();
            });
          });

          // Step 2: Test automatic CSRF token refresh
          await fw.executeStep('Test CSRF Token Refresh', async () => {
            const user = userEvent.setup();
            
            // Trigger token refresh
            const refreshButton = screen.getByText('Refresh Session');
            await user.click(refreshButton);

            // Mock successful token refresh
            const mockTokenResponse = {
              success: true,
              csrfToken: 'new-csrf-token-123',
            };

            await fw.simulateApiCall('/api/auth/csrf-token', mockTokenResponse, 50);

            await waitFor(() => {
              expect(screen.getByText('Session refreshed')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ CSRF Protection Test Passed');
    });
  });

  describe('Authentication and Authorization', () => {
    it('should handle authentication failures gracefully', async () => {
      const result = await framework.runIntegrationTest(
        'Authentication Failure Handling',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Simulate session expiration
          await fw.executeStep('Simulate Session Expiration', async () => {
            const mockAuthError = {
              success: false,
              error: 'Session expired',
              code: 'AUTH_EXPIRED',
            };

            await fw.simulateApiCall('/api/query/execute', mockAuthError, 100);

            await waitFor(() => {
              expect(screen.getByText('Session Expired')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('Please log in again')).toBeInTheDocument();
            });
          });

          // Step 2: Test automatic redirect to login
          await fw.executeStep('Test Login Redirect', async () => {
            const user = userEvent.setup();
            
            const loginButton = screen.getByText('Log In');
            await user.click(loginButton);

            await waitFor(() => {
              expect(screen.getByText('Login')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByLabelText('Username')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByLabelText('Password')).toBeInTheDocument();
            });
          });

          // Step 3: Test successful re-authentication
          await fw.executeStep('Test Re-authentication', async () => {
            const user = userEvent.setup();
            
            const usernameInput = screen.getByLabelText('Username');
            await user.type(usernameInput, 'testuser');
            
            const passwordInput = screen.getByLabelText('Password');
            await user.type(passwordInput, 'password123');
            
            const submitButton = screen.getByRole('button', { name: /log in/i });
            await user.click(submitButton);

            // Mock successful login
            const mockLoginResponse = {
              success: true,
              user: { id: '1', name: 'Test User', role: 'user' },
              token: 'new-auth-token',
            };

            await fw.simulateApiCall('/api/auth/login', mockLoginResponse, 200);

            await waitFor(() => {
              expect(screen.getByText('Welcome back, Test User!')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Authentication Failure Handling Test Passed');
    });

    it('should enforce role-based access control', async () => {
      const result = await framework.runIntegrationTest(
        'Role-Based Access Control',
        async (fw) => {
          // Test with regular user role
          fw = new IntegrationTestFramework({
            userRole: 'user',
            userPermissions: ['query:read', 'dashboard:read'],
          });

          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Test restricted admin access
          await fw.executeStep('Test Admin Access Restriction', async () => {
            const user = userEvent.setup();
            
            // Try to access admin page
            const adminNavItem = screen.getByText('Admin');
            await user.click(adminNavItem);

            await waitFor(() => {
              expect(screen.getByText('Access Denied')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('Insufficient permissions')).toBeInTheDocument();
            });
          });

          // Step 2: Test allowed access
          await fw.executeStep('Test Allowed Access', async () => {
            const user = userEvent.setup();
            
            // Access allowed pages
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);

            await waitFor(() => {
              expect(screen.getByText('Query Interface')).toBeInTheDocument();
            });

            const dashboardNavItem = screen.getByText('Dashboard');
            await user.click(dashboardNavItem);

            await waitFor(() => {
              expect(screen.getByText('Dashboard Management')).toBeInTheDocument();
            });
          });

          // Step 3: Test permission-based feature hiding
          await fw.executeStep('Test Feature Hiding', async () => {
            // Verify admin-only features are hidden
            expect(screen.queryByText('Delete Dashboard')).not.toBeInTheDocument();
            expect(screen.queryByText('Manage Users')).not.toBeInTheDocument();
            expect(screen.queryByText('System Settings')).not.toBeInTheDocument();
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Role-Based Access Control Test Passed');
    });
  });

  describe('Error Boundaries and Recovery', () => {
    it('should handle component errors gracefully', async () => {
      const result = await framework.runIntegrationTest(
        'Component Error Handling',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Simulate component error
          await fw.executeStep('Simulate Component Error', async () => {
            // Mock console.error to prevent test noise
            const originalError = console.error;
            console.error = jest.fn();

            // Trigger component error
            const errorTrigger = screen.getByTestId('error-trigger');
            const user = userEvent.setup();
            await user.click(errorTrigger);

            await waitFor(() => {
              expect(screen.getByText('Something went wrong')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('Error Boundary')).toBeInTheDocument();
            });

            console.error = originalError;
          });

          // Step 2: Test error recovery
          await fw.executeStep('Test Error Recovery', async () => {
            const user = userEvent.setup();
            
            const retryButton = screen.getByText('Try Again');
            await user.click(retryButton);

            await waitFor(() => {
              expect(screen.queryByText('Something went wrong')).not.toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('BI Reporting Copilot')).toBeInTheDocument();
            });
          });

          // Step 3: Test error reporting
          await fw.executeStep('Test Error Reporting', async () => {
            const user = userEvent.setup();
            
            // Trigger error again
            const errorTrigger = screen.getByTestId('error-trigger');
            await user.click(errorTrigger);

            // Test error reporting
            const reportButton = screen.getByText('Report Error');
            await user.click(reportButton);

            await waitFor(() => {
              expect(screen.getByText('Error Report')).toBeInTheDocument();
            });

            const descriptionInput = screen.getByLabelText('Description');
            await user.type(descriptionInput, 'Component crashed when clicking button');

            const submitReportButton = screen.getByText('Submit Report');
            await user.click(submitReportButton);

            // Mock error report submission
            const mockReportResponse = {
              success: true,
              reportId: 'error-123',
            };

            await fw.simulateApiCall('/api/errors/report', mockReportResponse, 100);

            await waitFor(() => {
              expect(screen.getByText('Error report submitted')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Component Error Handling Test Passed');
    });

    it('should handle network errors and offline scenarios', async () => {
      const result = await framework.runIntegrationTest(
        'Network Error Handling',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Simulate network failure
          await fw.executeStep('Simulate Network Failure', async () => {
            const user = userEvent.setup();
            
            // Navigate to Query page
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);

            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            await user.type(queryInput, 'Show me sales data');

            const submitButton = screen.getByRole('button', { name: /submit|execute/i });
            await user.click(submitButton);

            // Mock network error
            const networkError = new Error('Network request failed');
            networkError.name = 'NetworkError';

            await fw.simulateApiCall('/api/query/execute', networkError, 100);

            await waitFor(() => {
              expect(screen.getByText('Network Error')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('Please check your connection')).toBeInTheDocument();
            });
          });

          // Step 2: Test offline mode
          await fw.executeStep('Test Offline Mode', async () => {
            // Simulate going offline
            Object.defineProperty(navigator, 'onLine', {
              writable: true,
              value: false,
            });

            window.dispatchEvent(new Event('offline'));

            await waitFor(() => {
              expect(screen.getByText('You are offline')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('Some features may be limited')).toBeInTheDocument();
            });
          });

          // Step 3: Test reconnection
          await fw.executeStep('Test Reconnection', async () => {
            // Simulate coming back online
            Object.defineProperty(navigator, 'onLine', {
              writable: true,
              value: true,
            });

            window.dispatchEvent(new Event('online'));

            await waitFor(() => {
              expect(screen.getByText('Connection restored')).toBeInTheDocument();
            });

            // Test automatic retry
            const user = userEvent.setup();
            const retryButton = screen.getByText('Retry');
            await user.click(retryButton);

            // Mock successful retry
            const mockRetryResponse = {
              success: true,
              data: [{ id: 1, sales: 1000 }],
            };

            await fw.simulateApiCall('/api/query/execute', mockRetryResponse, 100);

            await waitFor(() => {
              expect(screen.getByText('Query executed successfully')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Network Error Handling Test Passed');
    });
  });

  describe('Data Validation and Sanitization', () => {
    it('should validate and sanitize all user inputs', async () => {
      const result = await framework.runIntegrationTest(
        'Input Validation and Sanitization',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Test dashboard name validation
          await fw.executeStep('Test Dashboard Name Validation', async () => {
            const user = userEvent.setup();
            
            const dashboardNavItem = screen.getByText('Dashboard');
            await user.click(dashboardNavItem);

            const createButton = screen.getByText('Create New Dashboard');
            await user.click(createButton);

            const nameInput = screen.getByLabelText('Dashboard Name');
            
            // Test invalid names
            const invalidNames = [
              '', // Empty
              'a', // Too short
              'a'.repeat(101), // Too long
              '<script>alert("xss")</script>', // XSS attempt
              'dashboard/with/slashes', // Invalid characters
            ];

            for (const invalidName of invalidNames) {
              await user.clear(nameInput);
              await user.type(nameInput, invalidName);

              const saveButton = screen.getByText('Save Dashboard');
              await user.click(saveButton);

              await waitFor(() => {
                expect(screen.getByText(/invalid dashboard name/i)).toBeInTheDocument();
              });
            }
          });

          // Step 2: Test email validation
          await fw.executeStep('Test Email Validation', async () => {
            const user = userEvent.setup();
            
            const shareButton = screen.getByText('Share');
            await user.click(shareButton);

            const emailInput = screen.getByLabelText('Email Address');
            
            const invalidEmails = [
              'invalid-email',
              '@domain.com',
              'user@',
              'user@domain',
              'user space@domain.com',
            ];

            for (const invalidEmail of invalidEmails) {
              await user.clear(emailInput);
              await user.type(emailInput, invalidEmail);

              const sendButton = screen.getByText('Send Email');
              await user.click(sendButton);

              await waitFor(() => {
                expect(screen.getByText(/invalid email address/i)).toBeInTheDocument();
              });
            }
          });

          // Step 3: Test file upload validation
          await fw.executeStep('Test File Upload Validation', async () => {
            const user = userEvent.setup();
            
            const uploadButton = screen.getByText('Upload Data');
            await user.click(uploadButton);

            // Mock file upload with invalid file
            const invalidFile = new File(['malicious content'], 'malware.exe', {
              type: 'application/x-msdownload',
            });

            const fileInput = screen.getByLabelText('Select File');
            await user.upload(fileInput, invalidFile);

            await waitFor(() => {
              expect(screen.getByText(/invalid file type/i)).toBeInTheDocument();
            });

            // Test valid file
            const validFile = new File(['col1,col2\nval1,val2'], 'data.csv', {
              type: 'text/csv',
            });

            await user.upload(fileInput, validFile);

            await waitFor(() => {
              expect(screen.getByText('File uploaded successfully')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Input Validation and Sanitization Test Passed');
    });
  });

  describe('Security Headers and CSP', () => {
    it('should enforce Content Security Policy', async () => {
      const result = await framework.runIntegrationTest(
        'Content Security Policy Enforcement',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Test CSP violation detection
          await fw.executeStep('Test CSP Violation Detection', async () => {
            // Mock CSP violation event
            const cspViolationEvent = new SecurityPolicyViolationEvent('securitypolicyviolation', {
              violatedDirective: 'script-src',
              blockedURI: 'https://malicious-site.com/script.js',
              documentURI: window.location.href,
              disposition: 'enforce',
              effectiveDirective: 'script-src',
              originalPolicy: "default-src 'self'; script-src 'self'",
              statusCode: 0,
            });

            window.dispatchEvent(cspViolationEvent);

            await waitFor(() => {
              expect(screen.getByText('Security Policy Violation')).toBeInTheDocument();
            });
          });

          // Step 2: Test inline script blocking
          await fw.executeStep('Test Inline Script Blocking', async () => {
            // Attempt to inject inline script
            const scriptElement = document.createElement('script');
            scriptElement.innerHTML = 'alert("CSP bypass attempt")';
            
            let cspError: Error | null = null;
            try {
              document.head.appendChild(scriptElement);
            } catch (error) {
              cspError = error as Error;
            }

            // CSP should block this
            expect(cspError).toBeTruthy();
            expect(cspError?.message).toContain('Content Security Policy');

            // Verify alert was not executed
            expect(window.alert).not.toHaveBeenCalled();
          });

          // Step 3: Test external resource validation
          await fw.executeStep('Test External Resource Validation', async () => {
            // Test loading external image from allowed domain
            const allowedImg = document.createElement('img');
            allowedImg.src = 'https://trusted-cdn.com/image.jpg';
            
            // This should be allowed
            expect(() => {
              document.body.appendChild(allowedImg);
            }).not.toThrow();

            // Test loading from disallowed domain
            const disallowedImg = document.createElement('img');
            disallowedImg.src = 'https://malicious-site.com/image.jpg';
            
            // This should be blocked by CSP
            disallowedImg.onerror = () => {
              expect(screen.getByText('Resource blocked by CSP')).toBeInTheDocument();
            };
            
            document.body.appendChild(disallowedImg);
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Content Security Policy Enforcement Test Passed');
    });
  });
});
