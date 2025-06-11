/**
 * Cross-Component Interactions Integration Tests
 * 
 * Tests for complex interactions between different components,
 * state management, and data flow across the application.
 */

import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { IntegrationTestFramework } from '../../test-utils/integration/IntegrationTestFramework';
import App from '../../App';

describe('Cross-Component Interactions Integration Tests', () => {
  let framework: IntegrationTestFramework;

  beforeEach(() => {
    framework = new IntegrationTestFramework({
      mockApi: true,
      mockAuth: true,
      userRole: 'admin',
      enablePerformanceMonitoring: true,
      initialRoute: '/',
    });
  });

  describe('Global State Management', () => {
    it('should maintain state consistency across page navigation', async () => {
      const result = await framework.runIntegrationTest(
        'Global State Consistency',
        async (fw) => {
          // Step 1: Render full application
          await fw.executeStep('Render Application', async () => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            expect(screen.getByText('BI Reporting Copilot')).toBeInTheDocument();
          });

          // Step 2: Execute query on Query page
          await fw.simulateUserInteraction('Execute Query', async () => {
            const user = userEvent.setup();
            
            // Navigate to Query page
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);
            
            await waitFor(() => {
              expect(screen.getByText('Query Interface')).toBeInTheDocument();
            });
            
            // Execute a query
            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            await user.type(queryInput, 'Show me sales data for last month');
            
            const submitButton = screen.getByRole('button', { name: /submit|execute/i });
            await user.click(submitButton);
            
            // Mock successful query response
            const mockQueryResult = {
              id: 'query-123',
              query: 'SELECT * FROM sales WHERE date >= DATE_SUB(NOW(), INTERVAL 1 MONTH)',
              data: [
                { id: 1, product: 'Product A', sales: 1000, date: '2024-01-15' },
                { id: 2, product: 'Product B', sales: 1500, date: '2024-01-20' },
              ],
              columns: ['id', 'product', 'sales', 'date'],
              success: true,
            };
            
            await fw.simulateApiCall('/api/query/execute', mockQueryResult, 200);
            
            await waitFor(() => {
              expect(screen.getByText('Query executed successfully')).toBeInTheDocument();
            });
          });

          // Step 3: Navigate to Results page and verify data persistence
          await fw.simulateUserInteraction('Navigate to Results', async () => {
            const user = userEvent.setup();
            
            const resultsNavItem = screen.getByText('Results');
            await user.click(resultsNavItem);
            
            await waitFor(() => {
              expect(screen.getByText('Query Results')).toBeInTheDocument();
              expect(screen.getByText('Product A')).toBeInTheDocument();
              expect(screen.getByText('Product B')).toBeInTheDocument();
            });
          });

          // Step 4: Navigate to Visualization and verify data availability
          await fw.simulateUserInteraction('Navigate to Visualization', async () => {
            const user = userEvent.setup();
            
            const vizNavItem = screen.getByText('Visualization');
            await user.click(vizNavItem);
            
            await waitFor(() => {
              expect(screen.getByText('Data Visualization')).toBeInTheDocument();
              expect(screen.getByText('Current Query Data Available')).toBeInTheDocument();
            });
            
            // Verify data is available for visualization
            const dataPreview = screen.getByTestId('data-preview');
            expect(dataPreview).toHaveTextContent('Product A');
            expect(dataPreview).toHaveTextContent('Product B');
          });

          // Step 5: Create visualization and verify it appears in Dashboard
          await fw.simulateUserInteraction('Create Visualization', async () => {
            const user = userEvent.setup();
            
            // Configure chart
            const chartTypeSelect = screen.getByLabelText('Chart Type');
            await user.click(chartTypeSelect);
            
            const barChart = screen.getByText('Bar Chart');
            await user.click(barChart);
            
            const xAxisSelect = screen.getByLabelText('X-Axis');
            await user.click(xAxisSelect);
            
            const productColumn = screen.getByText('product');
            await user.click(productColumn);
            
            const yAxisSelect = screen.getByLabelText('Y-Axis');
            await user.click(yAxisSelect);
            
            const salesColumn = screen.getByText('sales');
            await user.click(salesColumn);
            
            // Generate chart
            const generateButton = screen.getByText('Generate Chart');
            await user.click(generateButton);
            
            await waitFor(() => {
              expect(screen.getByTestId('chart-container')).toBeInTheDocument();
            });
            
            // Save chart to dashboard
            const saveToDashboardButton = screen.getByText('Save to Dashboard');
            await user.click(saveToDashboardButton);
            
            const dashboardSelect = screen.getByLabelText('Select Dashboard');
            await user.click(dashboardSelect);
            
            const newDashboard = screen.getByText('Create New Dashboard');
            await user.click(newDashboard);
            
            const dashboardNameInput = screen.getByLabelText('Dashboard Name');
            await user.type(dashboardNameInput, 'Sales Analysis Dashboard');
            
            const saveButton = screen.getByText('Save');
            await user.click(saveButton);
          });

          // Step 6: Navigate to Dashboard and verify chart appears
          await fw.simulateUserInteraction('Verify Dashboard Integration', async () => {
            const user = userEvent.setup();
            
            const dashboardNavItem = screen.getByText('Dashboard');
            await user.click(dashboardNavItem);
            
            await waitFor(() => {
              expect(screen.getByText('Sales Analysis Dashboard')).toBeInTheDocument();
            });
            
            // Select the dashboard
            const dashboardCard = screen.getByText('Sales Analysis Dashboard');
            await user.click(dashboardCard);
            
            await waitFor(() => {
              expect(screen.getByTestId('chart-container')).toBeInTheDocument();
              expect(screen.getByText('Product A')).toBeInTheDocument();
              expect(screen.getByText('Product B')).toBeInTheDocument();
            });
          });

          // Step 7: Test real-time data updates across components
          await fw.executeStep('Test Real-time Updates', async () => {
            // Simulate data update
            const updatedData = [
              { id: 1, product: 'Product A', sales: 1200, date: '2024-01-15' },
              { id: 2, product: 'Product B', sales: 1800, date: '2024-01-20' },
              { id: 3, product: 'Product C', sales: 900, date: '2024-01-25' },
            ];
            
            await fw.simulateApiCall('/api/query/query-123/refresh', { data: updatedData }, 100);
            
            // Verify chart updates
            await waitFor(() => {
              expect(screen.getByText('Product C')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      expect(result.errors).toHaveLength(0);
      console.log('✅ Global State Consistency Test Passed');
    });

    it('should handle concurrent user actions across components', async () => {
      const result = await framework.runIntegrationTest(
        'Concurrent User Actions',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Start multiple operations simultaneously
          await fw.simulateUserInteraction('Concurrent Operations', async () => {
            const user = userEvent.setup();
            
            // Start query execution
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);
            
            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            await user.type(queryInput, 'SELECT * FROM users');
            
            const submitButton = screen.getByRole('button', { name: /submit|execute/i });
            await user.click(submitButton);
            
            // Immediately navigate to another page while query is processing
            const dashboardNavItem = screen.getByText('Dashboard');
            await user.click(dashboardNavItem);
            
            // Start dashboard creation
            const createDashboardButton = screen.getByText('Create New Dashboard');
            await user.click(createDashboardButton);
            
            // Navigate back to query page
            const queryNavItem2 = screen.getByText('Query');
            await user.click(queryNavItem2);
          });

          // Step 2: Verify application handles concurrent operations gracefully
          await fw.executeStep('Verify Graceful Handling', async () => {
            // Mock query completion
            const mockQueryResult = {
              id: 'concurrent-query',
              data: [{ id: 1, name: 'User 1' }],
              success: true,
            };
            
            await fw.simulateApiCall('/api/query/execute', mockQueryResult, 300);
            
            // Verify query completed and result is available
            await waitFor(() => {
              expect(screen.getByText('Query executed successfully')).toBeInTheDocument();
            });
            
            // Verify no errors occurred
            expect(screen.queryByText(/error/i)).not.toBeInTheDocument();
            expect(screen.queryByText(/failed/i)).not.toBeInTheDocument();
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Concurrent User Actions Test Passed');
    });
  });

  describe('Component Communication', () => {
    it('should handle complex component interactions', async () => {
      const result = await framework.runIntegrationTest(
        'Complex Component Interactions',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Test sidebar navigation state persistence
          await fw.simulateUserInteraction('Test Sidebar State', async () => {
            const user = userEvent.setup();
            
            // Collapse sidebar
            const collapseButton = screen.getByTestId('sidebar-collapse');
            await user.click(collapseButton);
            
            await waitFor(() => {
              expect(screen.getByTestId('sidebar')).toHaveClass('collapsed');
            });
            
            // Navigate to different pages and verify sidebar state persists
            const pages = ['Query', 'Dashboard', 'Visualization', 'Results'];
            
            for (const page of pages) {
              const navItem = screen.getByText(page);
              await user.click(navItem);
              
              await waitFor(() => {
                expect(screen.getByTestId('sidebar')).toHaveClass('collapsed');
              });
            }
          });

          // Step 2: Test theme consistency across components
          await fw.simulateUserInteraction('Test Theme Consistency', async () => {
            const user = userEvent.setup();
            
            // Toggle dark mode
            const themeToggle = screen.getByTestId('theme-toggle');
            await user.click(themeToggle);
            
            await waitFor(() => {
              expect(document.body).toHaveClass('theme-dark');
            });
            
            // Navigate through pages and verify dark theme is applied
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);
            
            await waitFor(() => {
              const queryPage = screen.getByTestId('query-page');
              expect(queryPage).toHaveClass('dark-theme');
            });
            
            const dashboardNavItem = screen.getByText('Dashboard');
            await user.click(dashboardNavItem);
            
            await waitFor(() => {
              const dashboardPage = screen.getByTestId('dashboard-page');
              expect(dashboardPage).toHaveClass('dark-theme');
            });
          });

          // Step 3: Test error boundary propagation
          await fw.executeStep('Test Error Boundary', async () => {
            // Simulate component error
            const mockError = new Error('Component rendering error');
            
            // Mock a component that throws an error
            jest.spyOn(console, 'error').mockImplementation(() => {});
            
            // Trigger error in a child component
            await fw.simulateApiCall('/api/error-trigger', { error: mockError }, 100);
            
            await waitFor(() => {
              expect(screen.getByText('Something went wrong')).toBeInTheDocument();
              expect(screen.getByText('Error Boundary')).toBeInTheDocument();
            });
            
            // Verify error boundary doesn't crash entire app
            expect(screen.getByTestId('sidebar')).toBeInTheDocument();
            expect(screen.getByTestId('header')).toBeInTheDocument();
            
            jest.restoreAllMocks();
          });

          // Step 4: Test modal and overlay management
          await fw.simulateUserInteraction('Test Modal Management', async () => {
            const user = userEvent.setup();
            
            // Open multiple modals
            const settingsButton = screen.getByText('Settings');
            await user.click(settingsButton);
            
            await waitFor(() => {
              expect(screen.getByTestId('settings-modal')).toBeInTheDocument();
            });
            
            // Open another modal from within settings
            const advancedButton = screen.getByText('Advanced Settings');
            await user.click(advancedButton);
            
            await waitFor(() => {
              expect(screen.getByTestId('advanced-settings-modal')).toBeInTheDocument();
            });
            
            // Verify modal stacking
            const modals = screen.getAllByRole('dialog');
            expect(modals).toHaveLength(2);
            
            // Close modals in correct order
            const closeAdvancedButton = screen.getByTestId('close-advanced-modal');
            await user.click(closeAdvancedButton);
            
            await waitFor(() => {
              expect(screen.queryByTestId('advanced-settings-modal')).not.toBeInTheDocument();
              expect(screen.getByTestId('settings-modal')).toBeInTheDocument();
            });
          });

          // Step 5: Test keyboard navigation across components
          await fw.simulateUserInteraction('Test Keyboard Navigation', async () => {
            const user = userEvent.setup();
            
            // Test tab navigation
            await user.tab();
            expect(document.activeElement).toHaveAttribute('data-testid', 'sidebar-toggle');
            
            await user.tab();
            expect(document.activeElement).toHaveAttribute('data-testid', 'theme-toggle');
            
            await user.tab();
            expect(document.activeElement).toHaveAttribute('role', 'link');
            
            // Test keyboard shortcuts
            await user.keyboard('{Control>}k{/Control}');
            
            await waitFor(() => {
              expect(screen.getByTestId('command-palette')).toBeInTheDocument();
            });
            
            // Test escape key
            await user.keyboard('{Escape}');
            
            await waitFor(() => {
              expect(screen.queryByTestId('command-palette')).not.toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Complex Component Interactions Test Passed');
    });

    it('should handle data flow between query and visualization components', async () => {
      const result = await framework.runIntegrationTest(
        'Query-Visualization Data Flow',
        async (fw) => {
          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Execute query with specific data structure
          await fw.simulateUserInteraction('Execute Structured Query', async () => {
            const user = userEvent.setup();
            
            const queryNavItem = screen.getByText('Query');
            await user.click(queryNavItem);
            
            const queryInput = screen.getByPlaceholderText(/ask a question/i);
            await user.type(queryInput, 'Show sales by region and month');
            
            const submitButton = screen.getByRole('button', { name: /submit|execute/i });
            await user.click(submitButton);
            
            // Mock structured data response
            const mockStructuredData = {
              id: 'structured-query',
              data: [
                { region: 'North', month: 'January', sales: 10000 },
                { region: 'North', month: 'February', sales: 12000 },
                { region: 'South', month: 'January', sales: 8000 },
                { region: 'South', month: 'February', sales: 9500 },
                { region: 'East', month: 'January', sales: 11000 },
                { region: 'East', month: 'February', sales: 13000 },
              ],
              columns: ['region', 'month', 'sales'],
              metadata: {
                dataTypes: {
                  region: 'string',
                  month: 'string',
                  sales: 'number',
                },
                aggregatable: ['sales'],
                groupable: ['region', 'month'],
              },
              success: true,
            };
            
            await fw.simulateApiCall('/api/query/execute', mockStructuredData, 200);
          });

          // Step 2: Navigate to visualization and verify data structure recognition
          await fw.simulateUserInteraction('Verify Data Structure Recognition', async () => {
            const user = userEvent.setup();
            
            const vizNavItem = screen.getByText('Visualization');
            await user.click(vizNavItem);
            
            await waitFor(() => {
              expect(screen.getByText('Data Visualization')).toBeInTheDocument();
            });
            
            // Verify AI recommendations based on data structure
            await waitFor(() => {
              expect(screen.getByText('Recommended Charts')).toBeInTheDocument();
              expect(screen.getByText('Grouped Bar Chart')).toBeInTheDocument();
              expect(screen.getByText('Line Chart')).toBeInTheDocument();
              expect(screen.getByText('Heatmap')).toBeInTheDocument();
            });
            
            // Verify column suggestions
            const xAxisSelect = screen.getByLabelText('X-Axis');
            await user.click(xAxisSelect);
            
            expect(screen.getByText('region')).toBeInTheDocument();
            expect(screen.getByText('month')).toBeInTheDocument();
            
            const yAxisSelect = screen.getByLabelText('Y-Axis');
            await user.click(yAxisSelect);
            
            expect(screen.getByText('sales')).toBeInTheDocument();
          });

          // Step 3: Create multiple visualizations from same data
          await fw.simulateUserInteraction('Create Multiple Visualizations', async () => {
            const user = userEvent.setup();
            
            // Create first chart - Bar chart by region
            const chartTypeSelect = screen.getByLabelText('Chart Type');
            await user.click(chartTypeSelect);
            
            const barChart = screen.getByText('Bar Chart');
            await user.click(barChart);
            
            const xAxisSelect = screen.getByLabelText('X-Axis');
            await user.click(xAxisSelect);
            
            const regionOption = screen.getByText('region');
            await user.click(regionOption);
            
            const yAxisSelect = screen.getByLabelText('Y-Axis');
            await user.click(yAxisSelect);
            
            const salesOption = screen.getByText('sales');
            await user.click(salesOption);
            
            const generateButton = screen.getByText('Generate Chart');
            await user.click(generateButton);
            
            await waitFor(() => {
              expect(screen.getByTestId('chart-container-1')).toBeInTheDocument();
            });
            
            // Create second chart - Line chart by month
            const addChartButton = screen.getByText('Add Chart');
            await user.click(addChartButton);
            
            const lineChart = screen.getByText('Line Chart');
            await user.click(lineChart);
            
            const monthOption = screen.getByText('month');
            await user.click(monthOption);
            
            const generateButton2 = screen.getByText('Generate Chart');
            await user.click(generateButton2);
            
            await waitFor(() => {
              expect(screen.getByTestId('chart-container-2')).toBeInTheDocument();
            });
          });

          // Step 4: Test chart interactions and data filtering
          await fw.simulateUserInteraction('Test Chart Interactions', async () => {
            const user = userEvent.setup();
            
            // Click on bar in first chart to filter data
            const barElement = screen.getByTestId('chart-bar-north');
            await user.click(barElement);
            
            await waitFor(() => {
              expect(screen.getByText('Filtered by: North')).toBeInTheDocument();
            });
            
            // Verify second chart updates with filtered data
            await waitFor(() => {
              const chart2 = screen.getByTestId('chart-container-2');
              expect(chart2).toHaveAttribute('data-filtered', 'true');
            });
            
            // Clear filter
            const clearFilterButton = screen.getByText('Clear Filter');
            await user.click(clearFilterButton);
            
            await waitFor(() => {
              expect(screen.queryByText('Filtered by: North')).not.toBeInTheDocument();
            });
          });

          // Step 5: Test data export from visualization
          await fw.simulateUserInteraction('Test Data Export', async () => {
            const user = userEvent.setup();
            
            const exportButton = screen.getByText('Export Data');
            await user.click(exportButton);
            
            await waitFor(() => {
              expect(screen.getByText('Export Options')).toBeInTheDocument();
            });
            
            // Test different export formats
            const csvOption = screen.getByText('CSV');
            await user.click(csvOption);
            
            const downloadButton = screen.getByText('Download');
            await user.click(downloadButton);
            
            // Verify download initiated
            await waitFor(() => {
              expect(screen.getByText('Download started')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Query-Visualization Data Flow Test Passed');
    });
  });

  describe('Performance Under Load', () => {
    it('should maintain performance with complex interactions', async () => {
      const result = await framework.runIntegrationTest(
        'Performance Under Load',
        async (fw) => {
          // Set strict performance thresholds
          fw = new IntegrationTestFramework({
            performanceThresholds: {
              renderTime: 100,
              interactionTime: 50,
              apiResponseTime: 300,
            },
          });

          await fw.renderComponent(<App />, { waitForLoad: true });

          // Step 1: Rapid navigation between pages
          await fw.executeStep('Rapid Navigation Test', async () => {
            const user = userEvent.setup();
            const pages = ['Query', 'Dashboard', 'Visualization', 'Results', 'Admin'];
            
            for (let i = 0; i < 3; i++) {
              for (const page of pages) {
                const navItem = screen.getByText(page);
                await user.click(navItem);
                
                await waitFor(() => {
                  expect(screen.getByTestId(`${page.toLowerCase()}-page`)).toBeInTheDocument();
                });
              }
            }
          });

          // Step 2: Multiple simultaneous operations
          await fw.executeStep('Simultaneous Operations', async () => {
            const user = userEvent.setup();
            
            // Start multiple operations
            const operations = [
              () => user.click(screen.getByText('Query')),
              () => user.click(screen.getByText('Dashboard')),
              () => user.click(screen.getByTestId('theme-toggle')),
              () => user.click(screen.getByTestId('sidebar-collapse')),
            ];
            
            // Execute all operations rapidly
            await Promise.all(operations.map(op => op()));
            
            // Verify application remains responsive
            await waitFor(() => {
              expect(screen.getByTestId('app-container')).toBeInTheDocument();
            });
          });

          await fw.validatePerformance();
        }
      );

      expect(result.success).toBe(true);
      expect(result.performance.averageInteractionTime).toBeLessThan(100);
      console.log('✅ Performance Under Load Test Passed');
    });
  });
});
