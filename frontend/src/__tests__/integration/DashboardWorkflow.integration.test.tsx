/**
 * Dashboard Workflow Integration Tests
 * 
 * Comprehensive integration tests for dashboard creation, editing,
 * widget management, and real-time data updates.
 */

import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { IntegrationTestFramework } from '../../test-utils/integration/IntegrationTestFramework';
import DashboardPage from '../../pages/DashboardPage';

describe('Dashboard Workflow Integration Tests', () => {
  let framework: IntegrationTestFramework;

  beforeEach(() => {
    framework = new IntegrationTestFramework({
      mockApi: true,
      mockAuth: true,
      userRole: 'admin',
      enablePerformanceMonitoring: true,
      initialData: {
        dashboards: [
          {
            id: '1',
            name: 'Sales Dashboard',
            widgets: [
              { id: 'w1', type: 'chart', title: 'Monthly Sales', query: 'SELECT * FROM sales' },
              { id: 'w2', type: 'metric', title: 'Total Revenue', value: '$125,000' },
            ],
          },
        ],
        queries: [
          { id: 'q1', name: 'Sales Data', query: 'SELECT * FROM sales WHERE date >= CURDATE() - INTERVAL 30 DAY' },
          { id: 'q2', name: 'User Analytics', query: 'SELECT COUNT(*) FROM users GROUP BY DATE(created_at)' },
        ],
      },
    });
  });

  describe('Dashboard Creation and Management', () => {
    it('should handle complete dashboard creation workflow', async () => {
      const result = await framework.runIntegrationTest(
        'Dashboard Creation Workflow',
        async (fw) => {
          // Step 1: Render Dashboard Page
          await fw.executeStep('Render Dashboard Page', async () => {
            await fw.renderComponent(<DashboardPage />, { waitForLoad: true });
            
            expect(screen.getByText('Dashboard Management')).toBeInTheDocument();
            expect(screen.getByText('Create New Dashboard')).toBeInTheDocument();
          });

          // Step 2: Start Dashboard Creation
          await fw.simulateUserInteraction('Start Dashboard Creation', async () => {
            const user = userEvent.setup();
            const createButton = screen.getByText('Create New Dashboard');
            await user.click(createButton);
            
            await waitFor(() => {
              expect(screen.getByText('Dashboard Builder')).toBeInTheDocument();
            });
          });

          // Step 3: Configure Dashboard Properties
          await fw.simulateUserInteraction('Configure Dashboard Properties', async () => {
            const user = userEvent.setup();
            
            // Set dashboard name
            const nameInput = screen.getByLabelText('Dashboard Name');
            await user.type(nameInput, 'Analytics Dashboard');
            
            // Set description
            const descriptionInput = screen.getByLabelText('Description');
            await user.type(descriptionInput, 'Comprehensive analytics dashboard for business metrics');
            
            // Select layout
            const layoutSelect = screen.getByLabelText('Layout');
            await user.click(layoutSelect);
            
            const gridLayout = screen.getByText('Grid Layout');
            await user.click(gridLayout);
          });

          // Step 4: Add Chart Widget
          await fw.simulateUserInteraction('Add Chart Widget', async () => {
            const user = userEvent.setup();
            
            // Open widget panel
            const addWidgetButton = screen.getByText('Add Widget');
            await user.click(addWidgetButton);
            
            await waitFor(() => {
              expect(screen.getByText('Widget Types')).toBeInTheDocument();
            });
            
            // Select chart widget
            const chartWidget = screen.getByText('Chart');
            await user.click(chartWidget);
            
            // Configure chart widget
            const widgetNameInput = screen.getByLabelText('Widget Name');
            await user.type(widgetNameInput, 'Sales Trend');
            
            // Select data source
            const dataSourceSelect = screen.getByLabelText('Data Source');
            await user.click(dataSourceSelect);
            
            const salesQuery = screen.getByText('Sales Data');
            await user.click(salesQuery);
            
            // Configure chart type
            const chartTypeSelect = screen.getByLabelText('Chart Type');
            await user.click(chartTypeSelect);
            
            const lineChart = screen.getByText('Line Chart');
            await user.click(lineChart);
            
            // Add widget
            const addButton = screen.getByText('Add to Dashboard');
            await user.click(addButton);
          });

          // Step 5: Add Metric Widget
          await fw.simulateUserInteraction('Add Metric Widget', async () => {
            const user = userEvent.setup();
            
            const addWidgetButton = screen.getByText('Add Widget');
            await user.click(addWidgetButton);
            
            // Select metric widget
            const metricWidget = screen.getByText('Metric');
            await user.click(metricWidget);
            
            // Configure metric widget
            const metricNameInput = screen.getByLabelText('Widget Name');
            await user.type(metricNameInput, 'Total Users');
            
            const metricQuerySelect = screen.getByLabelText('Data Source');
            await user.click(metricQuerySelect);
            
            const userQuery = screen.getByText('User Analytics');
            await user.click(userQuery);
            
            // Set aggregation
            const aggregationSelect = screen.getByLabelText('Aggregation');
            await user.click(aggregationSelect);
            
            const sumOption = screen.getByText('Sum');
            await user.click(sumOption);
            
            const addMetricButton = screen.getByText('Add to Dashboard');
            await user.click(addMetricButton);
          });

          // Step 6: Arrange Widgets
          await fw.simulateUserInteraction('Arrange Widgets', async () => {
            const user = userEvent.setup();
            
            // Test drag and drop functionality
            const chartWidget = screen.getByTestId('widget-sales-trend');
            const metricWidget = screen.getByTestId('widget-total-users');
            
            // Simulate drag and drop (simplified)
            await user.pointer([
              { target: chartWidget, coords: { x: 100, y: 100 } },
              { coords: { x: 200, y: 100 } },
            ]);
            
            // Resize widget
            const resizeHandle = screen.getByTestId('resize-handle-sales-trend');
            await user.pointer([
              { target: resizeHandle, coords: { x: 300, y: 200 } },
              { coords: { x: 400, y: 250 } },
            ]);
          });

          // Step 7: Configure Auto-Refresh
          await fw.simulateUserInteraction('Configure Auto-Refresh', async () => {
            const user = userEvent.setup();
            
            const settingsButton = screen.getByText('Dashboard Settings');
            await user.click(settingsButton);
            
            await waitFor(() => {
              expect(screen.getByText('Auto-Refresh Settings')).toBeInTheDocument();
            });
            
            const autoRefreshToggle = screen.getByLabelText('Enable Auto-Refresh');
            await user.click(autoRefreshToggle);
            
            const intervalSelect = screen.getByLabelText('Refresh Interval');
            await user.click(intervalSelect);
            
            const fiveMinutes = screen.getByText('5 minutes');
            await user.click(fiveMinutes);
            
            const saveSettingsButton = screen.getByText('Save Settings');
            await user.click(saveSettingsButton);
          });

          // Step 8: Save Dashboard
          await fw.simulateUserInteraction('Save Dashboard', async () => {
            const user = userEvent.setup();
            
            const saveDashboardButton = screen.getByText('Save Dashboard');
            await user.click(saveDashboardButton);
            
            // Mock save API call
            const mockSaveResponse = {
              success: true,
              dashboard: {
                id: 'dash-123',
                name: 'Analytics Dashboard',
                widgets: [
                  { id: 'w1', type: 'chart', title: 'Sales Trend' },
                  { id: 'w2', type: 'metric', title: 'Total Users' },
                ],
              },
            };
            
            await fw.simulateApiCall('/api/dashboards', mockSaveResponse, 300);
            
            await waitFor(() => {
              expect(screen.getByText('Dashboard saved successfully')).toBeInTheDocument();
            });
          });

          // Step 9: Switch to Viewer Mode
          await fw.simulateUserInteraction('Switch to Viewer Mode', async () => {
            const user = userEvent.setup();
            
            const viewerModeButton = screen.getByText('View Mode');
            await user.click(viewerModeButton);
            
            await waitFor(() => {
              expect(screen.getByText('Dashboard Viewer')).toBeInTheDocument();
              expect(screen.queryByText('Add Widget')).not.toBeInTheDocument();
            });
          });

          // Step 10: Test Real-time Data Updates
          await fw.executeStep('Test Real-time Updates', async () => {
            // Mock real-time data update
            const mockUpdatedData = {
              'widget-sales-trend': {
                data: [
                  { date: '2024-01-01', sales: 1000 },
                  { date: '2024-01-02', sales: 1200 },
                  { date: '2024-01-03', sales: 1100 },
                ],
              },
              'widget-total-users': {
                value: 15420,
                change: '+5.2%',
              },
            };
            
            await fw.simulateApiCall('/api/dashboards/dash-123/data', mockUpdatedData, 100);
            
            await waitFor(() => {
              expect(screen.getByText('15,420')).toBeInTheDocument();
              expect(screen.getByText('+5.2%')).toBeInTheDocument();
            });
          });

          // Step 11: Test Widget Interactions
          await fw.simulateUserInteraction('Test Widget Interactions', async () => {
            const user = userEvent.setup();
            
            // Click on chart widget
            const chartWidget = screen.getByTestId('widget-sales-trend');
            await user.click(chartWidget);
            
            await waitFor(() => {
              expect(screen.getByText('Widget Details')).toBeInTheDocument();
            });
            
            // Test drill-down functionality
            const drillDownButton = screen.getByText('Drill Down');
            await user.click(drillDownButton);
            
            await waitFor(() => {
              expect(screen.getByText('Detailed View')).toBeInTheDocument();
            });
          });

          // Step 12: Test Dashboard Sharing
          await fw.simulateUserInteraction('Test Dashboard Sharing', async () => {
            const user = userEvent.setup();
            
            const shareButton = screen.getByText('Share');
            await user.click(shareButton);
            
            await waitFor(() => {
              expect(screen.getByText('Share Dashboard')).toBeInTheDocument();
            });
            
            // Generate share link
            const generateLinkButton = screen.getByText('Generate Link');
            await user.click(generateLinkButton);
            
            await waitFor(() => {
              expect(screen.getByText('Share link generated')).toBeInTheDocument();
            });
            
            // Test email sharing
            const emailInput = screen.getByLabelText('Email Address');
            await user.type(emailInput, 'colleague@example.com');
            
            const sendEmailButton = screen.getByText('Send Email');
            await user.click(sendEmailButton);
          });
        }
      );

      expect(result.success).toBe(true);
      expect(result.errors).toHaveLength(0);
      expect(result.steps).toHaveLength(12);
      
      console.log('✅ Dashboard Creation Workflow Test Passed');
      console.log(`📊 Performance: ${JSON.stringify(result.performance, null, 2)}`);
    });

    it('should handle dashboard editing and widget management', async () => {
      const result = await framework.runIntegrationTest(
        'Dashboard Editing Workflow',
        async (fw) => {
          // Step 1: Render Dashboard Page with existing dashboard
          await fw.renderComponent(<DashboardPage />, { waitForLoad: true });

          // Step 2: Select existing dashboard
          await fw.simulateUserInteraction('Select Existing Dashboard', async () => {
            const user = userEvent.setup();
            
            const dashboardCard = screen.getByText('Sales Dashboard');
            await user.click(dashboardCard);
            
            await waitFor(() => {
              expect(screen.getByText('Monthly Sales')).toBeInTheDocument();
              expect(screen.getByText('Total Revenue')).toBeInTheDocument();
            });
          });

          // Step 3: Enter Edit Mode
          await fw.simulateUserInteraction('Enter Edit Mode', async () => {
            const user = userEvent.setup();
            
            const editButton = screen.getByText('Edit');
            await user.click(editButton);
            
            await waitFor(() => {
              expect(screen.getByText('Dashboard Builder')).toBeInTheDocument();
            });
          });

          // Step 4: Edit Widget Properties
          await fw.simulateUserInteraction('Edit Widget Properties', async () => {
            const user = userEvent.setup();
            
            // Click on widget to edit
            const widget = screen.getByTestId('widget-monthly-sales');
            await user.click(widget);
            
            const editWidgetButton = screen.getByText('Edit Widget');
            await user.click(editWidgetButton);
            
            // Update widget title
            const titleInput = screen.getByLabelText('Widget Title');
            await user.clear(titleInput);
            await user.type(titleInput, 'Quarterly Sales Trend');
            
            // Update chart type
            const chartTypeSelect = screen.getByLabelText('Chart Type');
            await user.click(chartTypeSelect);
            
            const areaChart = screen.getByText('Area Chart');
            await user.click(areaChart);
            
            const saveWidgetButton = screen.getByText('Save Widget');
            await user.click(saveWidgetButton);
          });

          // Step 5: Remove Widget
          await fw.simulateUserInteraction('Remove Widget', async () => {
            const user = userEvent.setup();
            
            const widget = screen.getByTestId('widget-total-revenue');
            await user.click(widget);
            
            const removeButton = screen.getByText('Remove Widget');
            await user.click(removeButton);
            
            // Confirm removal
            const confirmButton = screen.getByText('Confirm');
            await user.click(confirmButton);
            
            await waitFor(() => {
              expect(screen.queryByText('Total Revenue')).not.toBeInTheDocument();
            });
          });

          // Step 6: Add New Widget Type (Table)
          await fw.simulateUserInteraction('Add Table Widget', async () => {
            const user = userEvent.setup();
            
            const addWidgetButton = screen.getByText('Add Widget');
            await user.click(addWidgetButton);
            
            const tableWidget = screen.getByText('Table');
            await user.click(tableWidget);
            
            // Configure table widget
            const tableNameInput = screen.getByLabelText('Widget Name');
            await user.type(tableNameInput, 'Recent Orders');
            
            const dataSourceSelect = screen.getByLabelText('Data Source');
            await user.click(dataSourceSelect);
            
            const ordersQuery = screen.getByText('Recent Orders Query');
            await user.click(ordersQuery);
            
            // Configure columns
            const columnsSelect = screen.getByLabelText('Columns');
            await user.click(columnsSelect);
            
            const orderIdColumn = screen.getByText('order_id');
            await user.click(orderIdColumn);
            
            const customerColumn = screen.getByText('customer_name');
            await user.click(customerColumn);
            
            const amountColumn = screen.getByText('amount');
            await user.click(amountColumn);
            
            const addTableButton = screen.getByText('Add to Dashboard');
            await user.click(addTableButton);
          });

          // Step 7: Test Dashboard Layout Changes
          await fw.simulateUserInteraction('Test Layout Changes', async () => {
            const user = userEvent.setup();
            
            // Change dashboard layout
            const layoutButton = screen.getByText('Layout');
            await user.click(layoutButton);
            
            const masonry = screen.getByText('Masonry');
            await user.click(masonry);
            
            await waitFor(() => {
              expect(screen.getByTestId('dashboard-masonry-layout')).toBeInTheDocument();
            });
          });

          // Step 8: Save Changes
          await fw.simulateUserInteraction('Save Dashboard Changes', async () => {
            const user = userEvent.setup();
            
            const saveDashboardButton = screen.getByText('Save Dashboard');
            await user.click(saveDashboardButton);
            
            const mockUpdateResponse = {
              success: true,
              message: 'Dashboard updated successfully',
            };
            
            await fw.simulateApiCall('/api/dashboards/1', mockUpdateResponse, 200);
            
            await waitFor(() => {
              expect(screen.getByText('Dashboard updated successfully')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Dashboard Editing Workflow Test Passed');
    });

    it('should handle dashboard performance with many widgets', async () => {
      const result = await framework.runIntegrationTest(
        'Dashboard Performance Test',
        async (fw) => {
          // Create dashboard with many widgets
          const manyWidgetsDashboard = {
            id: 'perf-test',
            name: 'Performance Test Dashboard',
            widgets: Array.from({ length: 20 }, (_, i) => ({
              id: `widget-${i}`,
              type: i % 3 === 0 ? 'chart' : i % 3 === 1 ? 'metric' : 'table',
              title: `Widget ${i + 1}`,
              query: `SELECT * FROM table_${i}`,
            })),
          };

          // Mock API response with large dataset
          const mockLargeDataResponse = {
            success: true,
            data: Array.from({ length: 1000 }, (_, i) => ({
              id: i,
              value: Math.random() * 1000,
              category: `Category ${i % 10}`,
            })),
          };

          await fw.renderComponent(<DashboardPage />, { waitForLoad: true });

          // Load dashboard with many widgets
          await fw.simulateApiCall('/api/dashboards/perf-test', manyWidgetsDashboard, 100);
          await fw.simulateApiCall('/api/dashboards/perf-test/data', mockLargeDataResponse, 200);

          // Verify all widgets render within performance threshold
          await fw.executeStep('Verify Widget Rendering Performance', async () => {
            await waitFor(() => {
              const widgets = screen.getAllByTestId(/^widget-/);
              expect(widgets).toHaveLength(20);
            }, { timeout: 3000 });
          });

          // Test scrolling performance
          await fw.simulateUserInteraction('Test Scrolling Performance', async () => {
            const user = userEvent.setup();
            const dashboardContainer = screen.getByTestId('dashboard-container');
            
            // Simulate scrolling
            for (let i = 0; i < 10; i++) {
              await user.pointer({ target: dashboardContainer, coords: { x: 500, y: 300 + i * 50 } });
            }
          });

          await fw.validatePerformance();
        }
      );

      expect(result.success).toBe(true);
      expect(result.performance.totalRenderTime).toBeLessThan(2000); // 2 seconds for 20 widgets
      console.log('✅ Dashboard Performance Test Passed');
    });
  });

  describe('Dashboard Collaboration Features', () => {
    it('should handle real-time collaboration', async () => {
      const result = await framework.runIntegrationTest(
        'Real-time Collaboration',
        async (fw) => {
          await fw.renderComponent(<DashboardPage />, { waitForLoad: true });

          // Simulate another user joining
          await fw.executeStep('Simulate Collaborative Session', async () => {
            const mockCollaborationEvent = {
              type: 'user_joined',
              user: { id: '2', name: 'Jane Doe' },
              timestamp: new Date().toISOString(),
            };

            // Simulate WebSocket message
            await fw.simulateApiCall('/api/collaboration/events', mockCollaborationEvent, 50);

            await waitFor(() => {
              expect(screen.getByText('Jane Doe joined the session')).toBeInTheDocument();
            });
          });

          // Test concurrent editing
          await fw.simulateUserInteraction('Test Concurrent Editing', async () => {
            const user = userEvent.setup();
            
            // Start editing
            const editButton = screen.getByText('Edit');
            await user.click(editButton);
            
            // Simulate another user's edit
            const mockConcurrentEdit = {
              type: 'widget_updated',
              widgetId: 'w1',
              user: { id: '2', name: 'Jane Doe' },
              changes: { title: 'Updated by Jane' },
            };
            
            await fw.simulateApiCall('/api/collaboration/events', mockConcurrentEdit, 50);
            
            await waitFor(() => {
              expect(screen.getByText('Widget updated by Jane Doe')).toBeInTheDocument();
            });
          });
        }
      );

      expect(result.success).toBe(true);
      console.log('✅ Real-time Collaboration Test Passed');
    });
  });
});
