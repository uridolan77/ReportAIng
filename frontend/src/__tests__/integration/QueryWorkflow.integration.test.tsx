/**
 * Query Workflow Integration Tests
 * 
 * Comprehensive integration tests for the complete query workflow including
 * query creation, execution, result display, and visualization.
 */

import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { IntegrationTestFramework } from '../../test-utils/integration/IntegrationTestFramework';
import QueryPage from '../../pages/QueryPage';
import ResultsPage from '../../pages/ResultsPage';
import VisualizationPage from '../../pages/VisualizationPage';

describe('Query Workflow Integration Tests', () => {
  let framework: IntegrationTestFramework;

  beforeEach(() => {
    framework = new IntegrationTestFramework({
      mockApi: true,
      mockAuth: true,
      mockDatabase: true,
      userRole: 'user',
      enablePerformanceMonitoring: true,
      performanceThresholds: {
        renderTime: 200,
        interactionTime: 100,
        apiResponseTime: 1000,
      },
      initialData: {
        queries: [
          {
            id: '1',
            query: 'SELECT * FROM users WHERE active = 1',
            name: 'Active Users',
            createdAt: '2024-01-01T00:00:00Z',
          },
        ],
      },
    });
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('Complete Query Creation and Execution Flow', () => {
    it('should handle complete query workflow from creation to visualization', async () => {
      const result = await framework.runIntegrationTest(
        'Complete Query Workflow',
        async (fw) => {
          // Step 1: Render Query Page
          await fw.executeStep('Render Query Page', async () => {
            await fw.renderComponent(<QueryPage />, { waitForLoad: true });
            
            // Verify page elements are present
            expect(screen.getByText('Query Interface')).toBeInTheDocument();
            expect(screen.getByText('Ask questions about your data using natural language')).toBeInTheDocument();
          });

          // Step 2: Navigate to Query Interface Tab
          await fw.simulateUserInteraction('Navigate to Query Interface', async () => {
            const user = userEvent.setup();
            const queryTab = screen.getByText('Query Interface');
            await user.click(queryTab);
            
            await waitFor(() => {
              expect(screen.getByPlaceholderText(/ask a question/i)).toBeInTheDocument();
            });
          });

          // Step 3: Enter Natural Language Query
          await fw.simulateUserInteraction('Enter Natural Language Query', async () => {
            const user = userEvent.setup();
            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            
            await user.type(queryInput, 'Show me all active users from the last month');
            expect(queryInput).toHaveValue('Show me all active users from the last month');
          });

          // Step 4: Submit Query
          await fw.simulateUserInteraction('Submit Query', async () => {
            const user = userEvent.setup();
            const submitButton = screen.getByRole('button', { name: /submit|execute/i });
            await user.click(submitButton);
            
            // Verify loading state
            await waitFor(() => {
              expect(screen.getByText(/processing/i)).toBeInTheDocument();
            });
          });

          // Step 5: Mock API Response
          const mockQueryResult = {
            id: 'query-123',
            query: 'SELECT * FROM users WHERE active = 1 AND created_at >= DATE_SUB(NOW(), INTERVAL 1 MONTH)',
            data: [
              { id: 1, name: 'John Doe', email: 'john@example.com', active: 1, created_at: '2024-01-15' },
              { id: 2, name: 'Jane Smith', email: 'jane@example.com', active: 1, created_at: '2024-01-20' },
              { id: 3, name: 'Bob Johnson', email: 'bob@example.com', active: 1, created_at: '2024-01-25' },
            ],
            columns: ['id', 'name', 'email', 'active', 'created_at'],
            executedAt: new Date().toISOString(),
            executionTime: 150,
            rowCount: 3,
            success: true,
          };

          await fw.simulateApiCall('/api/query/execute', mockQueryResult, 200);

          // Step 6: Verify Query Results Display
          await fw.executeStep('Verify Query Results', async () => {
            await waitFor(() => {
              expect(screen.getByText('Query executed successfully')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('3 rows returned')).toBeInTheDocument();
            });

            // Verify data table
            expect(screen.getByText('John Doe')).toBeInTheDocument();
            expect(screen.getByText('Jane Smith')).toBeInTheDocument();
            expect(screen.getByText('Bob Johnson')).toBeInTheDocument();
          });

          // Step 7: Navigate to Results Page
          await fw.simulateUserInteraction('Navigate to Results Page', async () => {
            const user = userEvent.setup();
            const viewResultsButton = screen.getByText('View Results');
            await user.click(viewResultsButton);
          });

          // Step 8: Render Results Page
          await fw.executeStep('Render Results Page', async () => {
            await fw.renderComponent(<ResultsPage />, { waitForLoad: true });
            
            // Verify results page elements
            expect(screen.getByText('Query Results')).toBeInTheDocument();
            expect(screen.getByText('Export')).toBeInTheDocument();
            expect(screen.getByText('Visualize')).toBeInTheDocument();
          });

          // Step 9: Test Data Export
          await fw.simulateUserInteraction('Test Data Export', async () => {
            const user = userEvent.setup();
            const exportButton = screen.getByText('Export');
            await user.click(exportButton);
            
            await waitFor(() => {
              expect(screen.getByText('Export Options')).toBeInTheDocument();
            });
            
            // Select CSV export
            const csvOption = screen.getByText('CSV');
            await user.click(csvOption);
            
            const downloadButton = screen.getByText('Download');
            await user.click(downloadButton);
          });

          // Step 10: Navigate to Visualization
          await fw.simulateUserInteraction('Navigate to Visualization', async () => {
            const user = userEvent.setup();
            const visualizeButton = screen.getByText('Visualize');
            await user.click(visualizeButton);
          });

          // Step 11: Render Visualization Page
          await fw.executeStep('Render Visualization Page', async () => {
            await fw.renderComponent(<VisualizationPage />, { waitForLoad: true });
            
            // Verify visualization page elements
            expect(screen.getByText('Data Visualization')).toBeInTheDocument();
            expect(screen.getByText('Chart Type')).toBeInTheDocument();
          });

          // Step 12: Create Visualization
          await fw.simulateUserInteraction('Create Visualization', async () => {
            const user = userEvent.setup();
            
            // Select chart type
            const chartTypeSelect = screen.getByLabelText('Chart Type');
            await user.click(chartTypeSelect);
            
            const barChartOption = screen.getByText('Bar Chart');
            await user.click(barChartOption);
            
            // Configure chart
            const xAxisSelect = screen.getByLabelText('X-Axis');
            await user.click(xAxisSelect);
            
            const nameOption = screen.getByText('name');
            await user.click(nameOption);
            
            const yAxisSelect = screen.getByLabelText('Y-Axis');
            await user.click(yAxisSelect);
            
            const idOption = screen.getByText('id');
            await user.click(idOption);
            
            // Generate chart
            const generateButton = screen.getByText('Generate Chart');
            await user.click(generateButton);
          });

          // Step 13: Verify Chart Display
          await fw.executeStep('Verify Chart Display', async () => {
            await waitFor(() => {
              expect(screen.getByText('Chart generated successfully')).toBeInTheDocument();
            });
            
            // Verify chart container
            const chartContainer = screen.getByTestId('chart-container');
            expect(chartContainer).toBeInTheDocument();
          });

          // Step 14: Test Chart Interactions
          await fw.simulateUserInteraction('Test Chart Interactions', async () => {
            const user = userEvent.setup();
            
            // Test chart configuration panel
            const configButton = screen.getByText('Configure');
            await user.click(configButton);
            
            await waitFor(() => {
              expect(screen.getByText('Chart Configuration')).toBeInTheDocument();
            });
            
            // Test chart export
            const chartExportButton = screen.getByText('Export Chart');
            await user.click(chartExportButton);
            
            await waitFor(() => {
              expect(screen.getByText('Export Format')).toBeInTheDocument();
            });
          });

          // Step 15: Validate Final State
          await fw.executeStep('Validate Final State', async () => {
            // Verify all components are in correct state
            expect(screen.getByText('Data Visualization')).toBeInTheDocument();
            expect(screen.getByTestId('chart-container')).toBeInTheDocument();
            
            // Verify no error messages
            expect(screen.queryByText(/error/i)).not.toBeInTheDocument();
            expect(screen.queryByText(/failed/i)).not.toBeInTheDocument();
          });
        }
      );

      // Validate test results
      expect(result.success).toBe(true);
      expect(result.errors).toHaveLength(0);
      expect(result.steps).toHaveLength(15);
      
      // Validate performance
      expect(result.performance.totalRenderTime).toBeLessThan(600); // 3 pages * 200ms
      expect(result.performance.averageInteractionTime).toBeLessThan(100);
      expect(result.performance.averageApiResponseTime).toBeLessThan(1000);
      
      console.log('✅ Complete Query Workflow Test Passed');
      console.log(`📊 Performance: ${JSON.stringify(result.performance, null, 2)}`);
    });

    it('should handle query errors gracefully', async () => {
      const result = await framework.runIntegrationTest(
        'Query Error Handling',
        async (fw) => {
          // Step 1: Render Query Page
          await fw.renderComponent(<QueryPage />, { waitForLoad: true });

          // Step 2: Enter Invalid Query
          await fw.simulateUserInteraction('Enter Invalid Query', async () => {
            const user = userEvent.setup();
            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            await user.type(queryInput, 'INVALID SQL QUERY WITH SYNTAX ERROR');
          });

          // Step 3: Submit Query
          await fw.simulateUserInteraction('Submit Invalid Query', async () => {
            const user = userEvent.setup();
            const submitButton = screen.getByRole('button', { name: /submit|execute/i });
            await user.click(submitButton);
          });

          // Step 4: Mock API Error Response
          const mockErrorResponse = {
            success: false,
            error: 'SQL syntax error near "INVALID"',
            message: 'Query execution failed',
          };

          await fw.simulateApiCall('/api/query/execute', mockErrorResponse, 100);

          // Step 5: Verify Error Handling
          await fw.executeStep('Verify Error Display', async () => {
            await waitFor(() => {
              expect(screen.getByText(/query execution failed/i)).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText(/sql syntax error/i)).toBeInTheDocument();
            });

            // Verify error styling
            const errorMessage = screen.getByText(/query execution failed/i);
            expect(errorMessage).toHaveClass('error');
          });

          // Step 6: Test Error Recovery
          await fw.simulateUserInteraction('Test Error Recovery', async () => {
            const user = userEvent.setup();
            
            // Clear error and try again
            const tryAgainButton = screen.getByText('Try Again');
            await user.click(tryAgainButton);
            
            // Verify error is cleared
            await waitFor(() => {
              expect(screen.queryByText(/query execution failed/i)).not.toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Query Error Handling Test Passed');
    });

    it('should handle concurrent queries correctly', async () => {
      const result = await framework.runIntegrationTest(
        'Concurrent Query Handling',
        async (fw) => {
          // Step 1: Render Query Page
          await fw.renderComponent(<QueryPage />, { waitForLoad: true });

          // Step 2: Submit Multiple Queries Rapidly
          await fw.simulateUserInteraction('Submit Multiple Queries', async () => {
            const user = userEvent.setup();
            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            const submitButton = screen.getByRole('button', { name: /submit|execute/i });

            // First query
            await user.type(queryInput, 'SELECT * FROM users');
            await user.click(submitButton);

            // Second query (before first completes)
            await user.clear(queryInput);
            await user.type(queryInput, 'SELECT * FROM orders');
            await user.click(submitButton);

            // Third query
            await user.clear(queryInput);
            await user.type(queryInput, 'SELECT * FROM products');
            await user.click(submitButton);
          });

          // Step 3: Verify Only Latest Query Executes
          await fw.executeStep('Verify Query Cancellation', async () => {
            // Mock responses for all queries
            const mockResponse = {
              success: true,
              data: [{ id: 1, name: 'Product 1' }],
              query: 'SELECT * FROM products',
            };

            await fw.simulateApiCall('/api/query/execute', mockResponse, 150);

            await waitFor(() => {
              expect(screen.getByText('Product 1')).toBeInTheDocument();
            });

            // Verify previous queries were cancelled
            expect(screen.queryByText('User 1')).not.toBeInTheDocument();
            expect(screen.queryByText('Order 1')).not.toBeInTheDocument();
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Concurrent Query Handling Test Passed');
    });
  });

  describe('Query History and Suggestions Integration', () => {
    it('should integrate query history with suggestions', async () => {
      const result = await framework.runIntegrationTest(
        'Query History Integration',
        async (fw) => {
          // Step 1: Render Query Page
          await fw.renderComponent(<QueryPage />, { waitForLoad: true });

          // Step 2: Navigate to History Tab
          await fw.simulateUserInteraction('Navigate to History', async () => {
            const user = userEvent.setup();
            const historyTab = screen.getByText('Query History');
            await user.click(historyTab);
          });

          // Step 3: Verify History Display
          await fw.executeStep('Verify History Display', async () => {
            await waitFor(() => {
              expect(screen.getByText('Active Users')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('SELECT * FROM users WHERE active = 1')).toBeInTheDocument();
            });
          });

          // Step 4: Re-execute Historical Query
          await fw.simulateUserInteraction('Re-execute Historical Query', async () => {
            const user = userEvent.setup();
            const executeButton = screen.getByText('Execute');
            await user.click(executeButton);
          });

          // Step 5: Navigate to Suggestions Tab
          await fw.simulateUserInteraction('Navigate to Suggestions', async () => {
            const user = userEvent.setup();
            const suggestionsTab = screen.getByText('AI Suggestions');
            await user.click(suggestionsTab);
          });

          // Step 6: Verify Suggestions Based on History
          await fw.executeStep('Verify AI Suggestions', async () => {
            const mockSuggestions = [
              'Show inactive users',
              'Count users by registration date',
              'Find users with recent activity',
            ];

            await fw.simulateApiCall('/api/suggestions', { suggestions: mockSuggestions });

            await waitFor(() => {
              expect(screen.getByText('Show inactive users')).toBeInTheDocument();
            });
            await waitFor(() => {
              expect(screen.getByText('Count users by registration date')).toBeInTheDocument();
            });
          });

          // Step 7: Use Suggestion
          await fw.simulateUserInteraction('Use AI Suggestion', async () => {
            const user = userEvent.setup();
            const suggestionButton = screen.getByText('Show inactive users');
            await user.click(suggestionButton);

            // Verify suggestion is applied to query input
            await waitFor(() => {
              const queryInput = screen.getByDisplayValue(/inactive users/i);
              expect(queryInput).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Query History Integration Test Passed');
    });
  });

  describe('Performance and Accessibility Integration', () => {
    it('should maintain performance standards throughout workflow', async () => {
      const result = await framework.runIntegrationTest(
        'Performance Standards',
        async (fw) => {
          // Test with strict performance thresholds
          fw = new IntegrationTestFramework({
            performanceThresholds: {
              renderTime: 50,
              interactionTime: 25,
              apiResponseTime: 200,
            },
          });

          // Execute complete workflow with performance monitoring
          await fw.renderComponent(<QueryPage />, { waitForLoad: true });
          
          // Multiple interactions to test average performance
          for (let i = 0; i < 5; i++) {
            await fw.simulateUserInteraction(`Performance Test ${i + 1}`, async () => {
              const user = userEvent.setup();
              const queryInput = screen.getByPlaceholderText(/ask a question/i);
              await user.type(queryInput, `Test query ${i + 1}`);
              await user.clear(queryInput);
            });
          }

          await fw.validatePerformance();
        }
      );

      expect(result.success).toBe(true);
      expect(result.performance.averageInteractionTime).toBeLessThan(50);
      console.log('✅ Performance Standards Test Passed');
    });

    it('should maintain accessibility throughout workflow', async () => {
      const result = await framework.runIntegrationTest(
        'Accessibility Standards',
        async (fw) => {
          // Step 1: Render and validate initial accessibility
          await fw.renderComponent(<QueryPage />, { waitForLoad: true });
          await fw.validateAccessibility();

          // Step 2: Test keyboard navigation
          await fw.simulateUserInteraction('Keyboard Navigation', async () => {
            const user = userEvent.setup();
            
            // Tab through interactive elements
            await user.tab();
            const firstButton = screen.getByRole('button');
            expect(firstButton).toHaveFocus();

            await user.tab();
            const textInput = screen.getByRole('textbox');
            expect(textInput).toHaveFocus();
          });

          // Step 3: Test screen reader compatibility
          await fw.executeStep('Screen Reader Compatibility', async () => {
            const buttons = screen.getAllByRole('button');
            buttons.forEach(button => {
              expect(button).toHaveAccessibleName();
            });

            const inputs = screen.getAllByRole('textbox');
            inputs.forEach(input => {
              expect(input).toHaveAccessibleName();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Accessibility Standards Test Passed');
    });
  });
});
