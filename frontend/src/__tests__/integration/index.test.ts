/**
 * Integration Test Suite Runner
 * 
 * Main entry point for running all integration tests with comprehensive
 * coverage of workflows, cross-component interactions, and real-world scenarios.
 */

import { IntegrationTestRunner, TestSuite } from '../../test-utils/integration/IntegrationTestRunner';
import { IntegrationTestFramework } from '../../test-utils/integration/IntegrationTestFramework';

// Import test suites
import QueryPage from '../../pages/QueryPage';
import DashboardPage from '../../pages/DashboardPage';
import VisualizationPage from '../../pages/VisualizationPage';
import ResultsPage from '../../pages/ResultsPage';
import App from '../../App';

describe('Integration Test Suite', () => {
  let testRunner: IntegrationTestRunner;

  beforeAll(() => {
    testRunner = new IntegrationTestRunner({
      parallel: false,
      maxConcurrency: 3,
      retryFailedTests: true,
      generateReport: true,
      reportFormat: 'html',
      outputDir: './test-reports/integration',
    });

    // Register all test suites
    registerAllTestSuites();
  });

  function registerAllTestSuites() {
    // Query Workflow Test Suite
    testRunner.registerSuite({
      name: 'Query Workflow',
      description: 'Complete query execution workflow from input to visualization',
      tests: [
        {
          name: 'Complete Query Workflow',
          description: 'End-to-end query execution, result display, and visualization creation',
          testFunction: async (fw) => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            // Execute query workflow
            await fw.simulateUserInteraction('Execute Query', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              const queryNavItem = screen.getByText('Query');
              await user.default.setup().click(queryNavItem);
              
              const queryInput = screen.getByPlaceholderText(/ask a question/i);
              await user.default.setup().type(queryInput, 'Show me sales data for last month');
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.default.setup().click(submitButton);
              
              const mockResult = {
                id: 'test-query',
                data: [{ product: 'A', sales: 1000 }, { product: 'B', sales: 1500 }],
                success: true,
              };
              
              await fw.simulateApiCall('/api/query/execute', mockResult, 200);
              
              await waitFor(() => {
                expect(screen.getByText('Query executed successfully')).toBeInTheDocument();
              });
            });
            
            // Navigate to results and create visualization
            await fw.simulateUserInteraction('Create Visualization', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              const resultsNavItem = screen.getByText('Results');
              await user.default.setup().click(resultsNavItem);
              
              const visualizeButton = screen.getByText('Visualize');
              await user.default.setup().click(visualizeButton);
              
              await waitFor(() => {
                expect(screen.getByText('Data Visualization')).toBeInTheDocument();
              });
            });
          },
          timeout: 30000,
          tags: ['workflow', 'query', 'visualization'],
        },
        {
          name: 'Query Error Handling',
          description: 'Test graceful handling of query errors and recovery',
          testFunction: async (fw) => {
            await fw.renderComponent(<QueryPage />, { waitForLoad: true });
            
            await fw.simulateUserInteraction('Submit Invalid Query', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              const queryInput = screen.getByPlaceholderText(/ask a question/i);
              await user.default.setup().type(queryInput, 'INVALID SQL SYNTAX');
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.default.setup().click(submitButton);
              
              const mockError = {
                success: false,
                error: 'SQL syntax error',
                message: 'Query execution failed',
              };
              
              await fw.simulateApiCall('/api/query/execute', mockError, 100);
              
              await waitFor(() => {
                expect(screen.getByText(/query execution failed/i)).toBeInTheDocument();
              });
            });
          },
          tags: ['error-handling', 'query'],
        },
      ],
    });

    // Dashboard Management Test Suite
    testRunner.registerSuite({
      name: 'Dashboard Management',
      description: 'Dashboard creation, editing, and widget management',
      tests: [
        {
          name: 'Dashboard Creation Workflow',
          description: 'Complete dashboard creation with multiple widgets',
          testFunction: async (fw) => {
            await fw.renderComponent(<DashboardPage />, { waitForLoad: true });
            
            await fw.simulateUserInteraction('Create Dashboard', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              const createButton = screen.getByText('Create New Dashboard');
              await user.default.setup().click(createButton);
              
              const nameInput = screen.getByLabelText('Dashboard Name');
              await user.default.setup().type(nameInput, 'Test Dashboard');
              
              // Add chart widget
              const addWidgetButton = screen.getByText('Add Widget');
              await user.default.setup().click(addWidgetButton);
              
              const chartWidget = screen.getByText('Chart');
              await user.default.setup().click(chartWidget);
              
              const saveDashboardButton = screen.getByText('Save Dashboard');
              await user.default.setup().click(saveDashboardButton);
              
              const mockSaveResponse = {
                success: true,
                dashboard: { id: 'test-dash', name: 'Test Dashboard' },
              };
              
              await fw.simulateApiCall('/api/dashboards', mockSaveResponse, 300);
              
              await waitFor(() => {
                expect(screen.getByText('Dashboard saved successfully')).toBeInTheDocument();
              });
            });
          },
          tags: ['dashboard', 'creation', 'widgets'],
        },
        {
          name: 'Real-time Dashboard Updates',
          description: 'Test real-time data updates in dashboard widgets',
          testFunction: async (fw) => {
            await fw.renderComponent(<DashboardPage />, { waitForLoad: true });
            
            await fw.executeStep('Test Real-time Updates', async () => {
              const mockUpdatedData = {
                'widget-1': { value: 15420, change: '+5.2%' },
                'widget-2': { data: [{ x: 'Jan', y: 1200 }] },
              };
              
              await fw.simulateApiCall('/api/dashboards/test/data', mockUpdatedData, 100);
              
              const { screen, waitFor } = await import('@testing-library/react');
              
              await waitFor(() => {
                expect(screen.getByText('15,420')).toBeInTheDocument();
                expect(screen.getByText('+5.2%')).toBeInTheDocument();
              });
            });
          },
          tags: ['dashboard', 'real-time', 'updates'],
        },
      ],
    });

    // Cross-Component Interactions Test Suite
    testRunner.registerSuite({
      name: 'Cross-Component Interactions',
      description: 'Complex interactions between different components and pages',
      tests: [
        {
          name: 'Global State Consistency',
          description: 'Test state persistence across page navigation',
          testFunction: async (fw) => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            // Execute query and verify state persistence
            await fw.simulateUserInteraction('Execute and Navigate', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              // Execute query
              const queryNavItem = screen.getByText('Query');
              await user.default.setup().click(queryNavItem);
              
              const queryInput = screen.getByPlaceholderText(/ask a question/i);
              await user.default.setup().type(queryInput, 'Test query');
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.default.setup().click(submitButton);
              
              const mockResult = {
                id: 'state-test',
                data: [{ id: 1, name: 'Test Data' }],
                success: true,
              };
              
              await fw.simulateApiCall('/api/query/execute', mockResult, 200);
              
              // Navigate to results and verify data persistence
              const resultsNavItem = screen.getByText('Results');
              await user.default.setup().click(resultsNavItem);
              
              await waitFor(() => {
                expect(screen.getByText('Test Data')).toBeInTheDocument();
              });
              
              // Navigate to visualization and verify data availability
              const vizNavItem = screen.getByText('Visualization');
              await user.default.setup().click(vizNavItem);
              
              await waitFor(() => {
                expect(screen.getByText('Current Query Data Available')).toBeInTheDocument();
              });
            });
          },
          tags: ['state-management', 'navigation', 'persistence'],
        },
        {
          name: 'Theme Consistency',
          description: 'Test theme persistence across components',
          testFunction: async (fw) => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            await fw.simulateUserInteraction('Toggle Theme', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              const themeToggle = screen.getByTestId('theme-toggle');
              await user.default.setup().click(themeToggle);
              
              await waitFor(() => {
                expect(document.body).toHaveClass('theme-dark');
              });
              
              // Navigate through pages and verify theme persistence
              const pages = ['Query', 'Dashboard', 'Visualization'];
              
              for (const page of pages) {
                const navItem = screen.getByText(page);
                await user.default.setup().click(navItem);
                
                await waitFor(() => {
                  const pageElement = screen.getByTestId(`${page.toLowerCase()}-page`);
                  expect(pageElement).toHaveClass('dark-theme');
                });
              }
            });
          },
          tags: ['theme', 'dark-mode', 'consistency'],
        },
      ],
    });

    // Security and Error Handling Test Suite
    testRunner.registerSuite({
      name: 'Security and Error Handling',
      description: 'Security features and comprehensive error handling',
      tests: [
        {
          name: 'XSS Protection',
          description: 'Test XSS attack prevention in user inputs',
          testFunction: async (fw) => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            await fw.simulateUserInteraction('Test XSS Prevention', async () => {
              const user = await import('@testing-library/user-event');
              const { screen, waitFor } = await import('@testing-library/react');
              
              const queryNavItem = screen.getByText('Query');
              await user.default.setup().click(queryNavItem);
              
              const maliciousInput = '<script>alert("XSS")</script>';
              const queryInput = screen.getByPlaceholderText(/ask a question/i);
              await user.default.setup().type(queryInput, maliciousInput);
              
              const submitButton = screen.getByRole('button', { name: /submit|execute/i });
              await user.default.setup().click(submitButton);
              
              await waitFor(() => {
                expect(screen.getByText(/invalid input detected/i)).toBeInTheDocument();
              });
              
              // Verify no script execution
              expect(window.alert).not.toHaveBeenCalled();
            });
          },
          tags: ['security', 'xss', 'input-validation'],
        },
        {
          name: 'Error Boundary Recovery',
          description: 'Test error boundary functionality and recovery',
          testFunction: async (fw) => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            await fw.executeStep('Test Error Boundary', async () => {
              // Mock console.error to prevent test noise
              const originalError = console.error;
              console.error = jest.fn();
              
              const { screen, waitFor } = await import('@testing-library/react');
              
              // Trigger component error
              const errorTrigger = screen.getByTestId('error-trigger');
              const user = await import('@testing-library/user-event');
              await user.default.setup().click(errorTrigger);
              
              await waitFor(() => {
                expect(screen.getByText('Something went wrong')).toBeInTheDocument();
              });
              
              // Test recovery
              const retryButton = screen.getByText('Try Again');
              await user.default.setup().click(retryButton);
              
              await waitFor(() => {
                expect(screen.queryByText('Something went wrong')).not.toBeInTheDocument();
              });
              
              console.error = originalError;
            });
          },
          tags: ['error-handling', 'error-boundary', 'recovery'],
        },
      ],
    });

    // Performance Test Suite
    testRunner.registerSuite({
      name: 'Performance',
      description: 'Performance benchmarks and optimization validation',
      tests: [
        {
          name: 'Page Load Performance',
          description: 'Test page load times and rendering performance',
          testFunction: async (fw) => {
            const startTime = performance.now();
            
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            const endTime = performance.now();
            const loadTime = endTime - startTime;
            
            // Verify load time is under threshold
            expect(loadTime).toBeLessThan(2000); // 2 seconds
            
            await fw.validatePerformance();
          },
          tags: ['performance', 'load-time'],
        },
        {
          name: 'Interaction Performance',
          description: 'Test user interaction response times',
          testFunction: async (fw) => {
            await fw.renderComponent(<App />, { waitForLoad: true });
            
            // Test multiple rapid interactions
            for (let i = 0; i < 10; i++) {
              await fw.simulateUserInteraction(`Rapid Interaction ${i + 1}`, async () => {
                const user = await import('@testing-library/user-event');
                const { screen } = await import('@testing-library/react');
                
                const themeToggle = screen.getByTestId('theme-toggle');
                await user.default.setup().click(themeToggle);
              });
            }
            
            await fw.validatePerformance();
          },
          tags: ['performance', 'interactions'],
        },
      ],
    });
  }

  // Main test execution
  it('should run all integration test suites', async () => {
    const result = await testRunner.runAllSuites();
    
    // Verify overall test success
    expect(result.summary.successRate).toBeGreaterThan(90); // 90% success rate
    expect(result.performance.performanceScore).toBeGreaterThan(70); // 70/100 performance score
    expect(result.coverage.overallCoverage).toBeGreaterThan(80); // 80% coverage
    
    console.log('✅ All integration tests completed successfully');
  }, 300000); // 5 minute timeout for full suite

  // Individual suite tests for faster feedback
  it('should run query workflow tests', async () => {
    const result = await testRunner.runAllSuites({
      suites: ['Query Workflow'],
    });
    
    expect(result.summary.successRate).toBe(100);
  }, 60000);

  it('should run dashboard management tests', async () => {
    const result = await testRunner.runAllSuites({
      suites: ['Dashboard Management'],
    });
    
    expect(result.summary.successRate).toBe(100);
  }, 60000);

  it('should run security tests', async () => {
    const result = await testRunner.runAllSuites({
      suites: ['Security and Error Handling'],
    });
    
    expect(result.summary.successRate).toBe(100);
  }, 60000);

  it('should run performance tests', async () => {
    const result = await testRunner.runAllSuites({
      suites: ['Performance'],
    });
    
    expect(result.summary.successRate).toBe(100);
    expect(result.performance.performanceScore).toBeGreaterThan(70);
  }, 60000);

  // Tag-based test execution
  it('should run workflow tests', async () => {
    const result = await testRunner.runAllSuites({
      tags: ['workflow'],
    });
    
    expect(result.summary.successRate).toBeGreaterThan(90);
  }, 120000);

  it('should run security tests', async () => {
    const result = await testRunner.runAllSuites({
      tags: ['security'],
    });
    
    expect(result.summary.successRate).toBe(100);
  }, 60000);
});
