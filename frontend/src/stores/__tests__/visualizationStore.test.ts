import { renderHook, act } from '@testing-library/react';
import { useVisualizationStore } from '../visualizationStore';

describe('Visualization Store', () => {
  beforeEach(() => {
    // Clear the store state before each test
    const store = useVisualizationStore.getState();
    store.dashboards.forEach(dashboard => {
      store.deleteDashboard(dashboard.id);
    });
    localStorage.clear();
  });

  it('should have initial state', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    expect(result.current.currentVisualization).toBe(null);
    expect(result.current.dashboards).toEqual([]);
    expect(result.current.preferences).toEqual({
      enableAnimations: true,
      enableInteractivity: true,
      performance: 'Balanced',
      accessibility: 'Standard',
      theme: 'light'
    });
    expect(result.current.performanceMetrics).toBe(null);
  });

  it('should set current visualization', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockVisualization = {
      id: 'viz-1',
      type: 'bar',
      title: 'Test Chart',
      data: [{ x: 1, y: 2 }],
      config: { color: 'blue' },
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    act(() => {
      result.current.setVisualization(mockVisualization);
    });

    expect(result.current.currentVisualization).toEqual(mockVisualization);
  });

  it('should add dashboard', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockDashboard = {
      id: 'dash-1',
      name: 'Test Dashboard',
      description: 'A test dashboard',
      visualizations: [],
      layout: {},
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    act(() => {
      result.current.addDashboard(mockDashboard);
    });

    expect(result.current.dashboards).toHaveLength(1);
    expect(result.current.dashboards[0]).toEqual(mockDashboard);
  });

  it('should update dashboard', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockDashboard = {
      id: 'dash-1',
      name: 'Test Dashboard',
      description: 'A test dashboard',
      visualizations: [],
      layout: {},
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    act(() => {
      result.current.addDashboard(mockDashboard);
    });

    const updates = {
      name: 'Updated Dashboard',
      description: 'Updated description'
    };

    act(() => {
      result.current.updateDashboard('dash-1', updates);
    });

    const updatedDashboard = result.current.dashboards[0];
    expect(updatedDashboard.name).toBe('Updated Dashboard');
    expect(updatedDashboard.description).toBe('Updated description');
    expect(updatedDashboard.updatedAt).toBeGreaterThan(mockDashboard.updatedAt);
  });

  it('should delete dashboard', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockDashboard = {
      id: 'dash-1',
      name: 'Test Dashboard',
      description: 'A test dashboard',
      visualizations: [],
      layout: {},
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    act(() => {
      result.current.addDashboard(mockDashboard);
    });

    expect(result.current.dashboards).toHaveLength(1);

    act(() => {
      result.current.deleteDashboard('dash-1');
    });

    expect(result.current.dashboards).toHaveLength(0);
  });

  it('should update preferences', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const newPreferences = {
      enableAnimations: false,
      theme: 'dark' as const
    };

    act(() => {
      result.current.updatePreferences(newPreferences);
    });

    expect(result.current.preferences.enableAnimations).toBe(false);
    expect(result.current.preferences.theme).toBe('dark');
    expect(result.current.preferences.enableInteractivity).toBe(true); // Should remain unchanged
  });

  it('should set performance metrics', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockMetrics = {
      renderTime: 150,
      dataPoints: 1000,
      memoryUsage: 50,
      lastUpdated: Date.now()
    };

    act(() => {
      result.current.setPerformanceMetrics(mockMetrics);
    });

    expect(result.current.performanceMetrics).toEqual(mockMetrics);
  });

  it('should get dashboard by id', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockDashboard = {
      id: 'dash-1',
      name: 'Test Dashboard',
      description: 'A test dashboard',
      visualizations: [],
      layout: {},
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    act(() => {
      result.current.addDashboard(mockDashboard);
    });

    const foundDashboard = result.current.getDashboardById('dash-1');
    expect(foundDashboard).toEqual(mockDashboard);

    const notFoundDashboard = result.current.getDashboardById('non-existent');
    expect(notFoundDashboard).toBeUndefined();
  });

  it('should get recent dashboards', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const now = Date.now();
    const dashboards = [
      {
        id: 'dash-1',
        name: 'Dashboard 1',
        description: '',
        visualizations: [],
        layout: {},
        createdAt: now - 3000,
        updatedAt: now - 1000
      },
      {
        id: 'dash-2',
        name: 'Dashboard 2',
        description: '',
        visualizations: [],
        layout: {},
        createdAt: now - 2000,
        updatedAt: now - 500
      },
      {
        id: 'dash-3',
        name: 'Dashboard 3',
        description: '',
        visualizations: [],
        layout: {},
        createdAt: now - 1000,
        updatedAt: now - 2000
      }
    ];

    act(() => {
      dashboards.forEach(dashboard => {
        result.current.addDashboard(dashboard);
      });
    });

    const recentDashboards = result.current.getRecentDashboards(2);
    expect(recentDashboards).toHaveLength(2);
    expect(recentDashboards[0].id).toBe('dash-2'); // Most recently updated
    expect(recentDashboards[1].id).toBe('dash-1');
  });

  it('should get visualizations by type', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockDashboard = {
      id: 'dash-1',
      name: 'Test Dashboard',
      description: '',
      visualizations: [
        {
          id: 'viz-1',
          type: 'bar',
          title: 'Bar Chart',
          data: [],
          config: {},
          createdAt: Date.now(),
          updatedAt: Date.now()
        },
        {
          id: 'viz-2',
          type: 'line',
          title: 'Line Chart',
          data: [],
          config: {},
          createdAt: Date.now(),
          updatedAt: Date.now()
        },
        {
          id: 'viz-3',
          type: 'bar',
          title: 'Another Bar Chart',
          data: [],
          config: {},
          createdAt: Date.now(),
          updatedAt: Date.now()
        }
      ],
      layout: {},
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    act(() => {
      result.current.addDashboard(mockDashboard);
    });

    const barCharts = result.current.getVisualizationsByType('bar');
    expect(barCharts).toHaveLength(2);
    expect(barCharts.every(viz => viz.type === 'bar')).toBe(true);

    const lineCharts = result.current.getVisualizationsByType('line');
    expect(lineCharts).toHaveLength(1);
    expect(lineCharts[0].type).toBe('line');

    const pieCharts = result.current.getVisualizationsByType('pie');
    expect(pieCharts).toHaveLength(0);
  });

  it('should persist state to localStorage', () => {
    const { result } = renderHook(() => useVisualizationStore());
    
    const mockDashboard = {
      id: 'dash-1',
      name: 'Test Dashboard',
      description: 'A test dashboard',
      visualizations: [],
      layout: {},
      createdAt: Date.now(),
      updatedAt: Date.now()
    };

    const newPreferences = {
      theme: 'dark' as const
    };

    act(() => {
      result.current.addDashboard(mockDashboard);
      result.current.updatePreferences(newPreferences);
    });

    const storedState = localStorage.getItem('visualization-storage');
    expect(storedState).toBeTruthy();
    
    const parsedState = JSON.parse(storedState!);
    expect(parsedState.state.dashboards).toHaveLength(1);
    expect(parsedState.state.preferences.theme).toBe('dark');
  });
});
