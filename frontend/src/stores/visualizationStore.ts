import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { subscribeWithSelector } from 'zustand/middleware';

interface AdvancedVisualizationConfig {
  id: string;
  type: string;
  title: string;
  data: any[];
  config: any;
  createdAt: number;
  updatedAt: number;
}

interface AdvancedDashboardConfig {
  id: string;
  name: string;
  description: string;
  visualizations: AdvancedVisualizationConfig[];
  layout: any;
  createdAt: number;
  updatedAt: number;
}

interface VisualizationPreferences {
  enableAnimations: boolean;
  enableInteractivity: boolean;
  performance: 'High' | 'Balanced' | 'Quality';
  accessibility: 'Standard' | 'Enhanced';
  theme: 'light' | 'dark' | 'auto';
}

interface ChartPerformanceMetrics {
  renderTime: number;
  dataPoints: number;
  memoryUsage: number;
  lastUpdated: number;
}

interface VisualizationState {
  currentVisualization: any | null; // Allow any chart config type
  dashboards: AdvancedDashboardConfig[];
  preferences: VisualizationPreferences;
  performanceMetrics: ChartPerformanceMetrics | null;

  // New chart management
  charts: AdvancedVisualizationConfig[];
  selectedChart: string | null;

  // Actions
  setVisualization: (config: any | null) => void; // Allow null to clear
  addDashboard: (dashboard: AdvancedDashboardConfig) => void;
  updateDashboard: (id: string, updates: Partial<AdvancedDashboardConfig>) => void;
  deleteDashboard: (id: string) => void;
  updatePreferences: (preferences: Partial<VisualizationPreferences>) => void;
  setPerformanceMetrics: (metrics: ChartPerformanceMetrics) => void;

  // New chart actions
  addChart: (chart: Omit<AdvancedVisualizationConfig, 'id' | 'createdAt' | 'updatedAt'>) => void;
  updateChart: (id: string, updates: Partial<AdvancedVisualizationConfig>) => void;
  removeChart: (id: string) => void;
  selectChart: (id: string | null) => void;

  // Selectors
  getDashboardById: (id: string) => AdvancedDashboardConfig | undefined;
  getRecentDashboards: (limit: number) => AdvancedDashboardConfig[];
  getVisualizationsByType: (type: string) => AdvancedVisualizationConfig[];
}

export const useVisualizationStore = create<VisualizationState>()(
  subscribeWithSelector(
    persist(
      (set, get) => ({
        currentVisualization: null,
        dashboards: [],
        charts: [],
        selectedChart: null,
        preferences: {
          enableAnimations: true,
          enableInteractivity: true,
          performance: 'Balanced',
          accessibility: 'Standard',
          theme: 'light'
        },
        performanceMetrics: null,

        setVisualization: (config) => set({ currentVisualization: config }),

        addDashboard: (dashboard) => set((state) => ({
          dashboards: [...state.dashboards, dashboard]
        })),

        updateDashboard: (id, updates) => set((state) => ({
          dashboards: state.dashboards.map(d =>
            d.id === id ? { ...d, ...updates, updatedAt: Date.now() } : d
          )
        })),

        deleteDashboard: (id) => set((state) => ({
          dashboards: state.dashboards.filter(d => d.id !== id)
        })),

        updatePreferences: (preferences) => set((state) => ({
          preferences: { ...state.preferences, ...preferences }
        })),

        setPerformanceMetrics: (metrics) => set({ performanceMetrics: metrics }),

        // New chart actions
        addChart: (chartData) => {
          const chart: AdvancedVisualizationConfig = {
            ...chartData,
            id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
            createdAt: Date.now(),
            updatedAt: Date.now(),
          };

          set((state) => ({
            charts: [...state.charts, chart],
            selectedChart: chart.id,
          }));
        },

        updateChart: (id, updates) => {
          set((state) => ({
            charts: state.charts.map((chart) =>
              chart.id === id
                ? { ...chart, ...updates, updatedAt: Date.now() }
                : chart
            ),
          }));
        },

        removeChart: (id) => {
          set((state) => ({
            charts: state.charts.filter((chart) => chart.id !== id),
            selectedChart: state.selectedChart === id ? null : state.selectedChart,
          }));
        },

        selectChart: (id) => {
          set({ selectedChart: id });
        },

        getDashboardById: (id) => {
          return get().dashboards.find(d => d.id === id);
        },

        getRecentDashboards: (limit) => {
          return get().dashboards
            .sort((a, b) => b.updatedAt - a.updatedAt)
            .slice(0, limit);
        },

        getVisualizationsByType: (type) => {
          const dashboards = get().dashboards;
          const visualizations: AdvancedVisualizationConfig[] = [];

          dashboards.forEach(dashboard => {
            dashboard.visualizations.forEach(viz => {
              if (viz.type === type) {
                visualizations.push(viz);
              }
            });
          });

          return visualizations;
        }
      }),
      {
        name: 'visualization-storage',
        storage: createJSONStorage(() => localStorage),
        partialize: (state) => ({
          currentVisualization: state.currentVisualization, // Include current visualization in persistence
          dashboards: state.dashboards,
          charts: state.charts,
          preferences: state.preferences
        })
      }
    )
  )
);

// Selector hooks to prevent unnecessary re-renders
export const useVisualizationPreferences = () =>
  useVisualizationStore((state) => state.preferences);

export const useDashboards = () =>
  useVisualizationStore((state) => state.dashboards);

export const useCurrentVisualization = () =>
  useVisualizationStore((state) => state.currentVisualization);

export const usePerformanceMetrics = () =>
  useVisualizationStore((state) => state.performanceMetrics);
