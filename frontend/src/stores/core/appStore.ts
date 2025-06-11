/**
 * App Store
 * 
 * Central application store managing global app state, configuration,
 * and cross-cutting concerns with advanced patterns.
 */

import { create } from 'zustand';
import { subscribeWithSelector, devtools, persist } from 'zustand/middleware';
import { immer } from 'zustand/middleware/immer';

// Types
export interface AppConfig {
  apiUrl: string;
  version: string;
  environment: 'development' | 'staging' | 'production';
  features: {
    darkMode: boolean;
    animations: boolean;
    analytics: boolean;
    debugging: boolean;
    mockData: boolean;
    caching: boolean;
    offlineMode: boolean;
  };
  performance: {
    enableVirtualization: boolean;
    enableLazyLoading: boolean;
    enableCodeSplitting: boolean;
    maxCacheSize: number;
    requestTimeout: number;
  };
  ui: {
    sidebarCollapsed: boolean;
    compactMode: boolean;
    showTooltips: boolean;
    animationSpeed: 'slow' | 'normal' | 'fast';
    density: 'comfortable' | 'compact' | 'spacious';
  };
}

export interface AppMetrics {
  sessionStart: Date;
  pageViews: number;
  apiCalls: number;
  errors: number;
  performance: {
    averageLoadTime: number;
    averageApiResponseTime: number;
    memoryUsage: number;
  };
}

export interface AppState {
  // Core State
  initialized: boolean;
  online: boolean;
  config: AppConfig;
  metrics: AppMetrics;
  
  // Feature Flags
  featureFlags: Record<string, boolean>;
  
  // Global Loading States
  globalLoading: boolean;
  loadingStates: Record<string, boolean>;
  
  // Error State
  globalError: string | null;
  
  // Actions
  initialize: () => Promise<void>;
  updateConfig: (updates: Partial<AppConfig>) => void;
  toggleFeature: (feature: keyof AppConfig['features']) => void;
  setOnlineStatus: (online: boolean) => void;
  setGlobalLoading: (loading: boolean) => void;
  setLoadingState: (key: string, loading: boolean) => void;
  setGlobalError: (error: string | null) => void;
  updateMetrics: (updates: Partial<AppMetrics>) => void;
  incrementMetric: (metric: keyof Pick<AppMetrics, 'pageViews' | 'apiCalls' | 'errors'>) => void;
  
  // Feature Flag Actions
  setFeatureFlag: (flag: string, enabled: boolean) => void;
  getFeatureFlag: (flag: string) => boolean;
  
  // Reset Actions
  reset: () => void;
  resetMetrics: () => void;
}

// Default Configuration
const defaultConfig: AppConfig = {
  apiUrl: process.env.REACT_APP_API_URL || 'http://localhost:5000',
  version: process.env.REACT_APP_VERSION || '1.0.0',
  environment: (process.env.NODE_ENV as any) || 'development',
  features: {
    darkMode: true,
    animations: true,
    analytics: true,
    debugging: process.env.NODE_ENV === 'development',
    mockData: false,
    caching: true,
    offlineMode: false,
  },
  performance: {
    enableVirtualization: true,
    enableLazyLoading: true,
    enableCodeSplitting: true,
    maxCacheSize: 100,
    requestTimeout: 30000,
  },
  ui: {
    sidebarCollapsed: false,
    compactMode: false,
    showTooltips: true,
    animationSpeed: 'normal',
    density: 'comfortable',
  },
};

const defaultMetrics: AppMetrics = {
  sessionStart: new Date(),
  pageViews: 0,
  apiCalls: 0,
  errors: 0,
  performance: {
    averageLoadTime: 0,
    averageApiResponseTime: 0,
    memoryUsage: 0,
  },
};

// Store Implementation
export const useAppStore = create<AppState>()(
  devtools(
    persist(
      subscribeWithSelector(
        immer((set, get) => ({
          // Initial State
          initialized: false,
          online: navigator.onLine,
          config: defaultConfig,
          metrics: defaultMetrics,
          featureFlags: {},
          globalLoading: false,
          loadingStates: {},
          globalError: null,

          // Actions
          initialize: async () => {
            try {
              set((state) => {
                state.globalLoading = true;
              });

              // Initialize app configuration
              const config = await loadAppConfig();
              const featureFlags = await loadFeatureFlags();

              set((state) => {
                state.config = { ...state.config, ...config };
                state.featureFlags = featureFlags;
                state.initialized = true;
                state.globalLoading = false;
                state.metrics.sessionStart = new Date();
              });

              // Setup online/offline listeners
              window.addEventListener('online', () => get().setOnlineStatus(true));
              window.addEventListener('offline', () => get().setOnlineStatus(false));

              // Setup performance monitoring
              if (get().config.features.analytics) {
                setupPerformanceMonitoring();
              }

            } catch (error) {
              set((state) => {
                state.globalError = 'Failed to initialize application';
                state.globalLoading = false;
              });
              console.error('App initialization failed:', error);
            }
          },

          updateConfig: (updates) => {
            set((state) => {
              state.config = { ...state.config, ...updates };
            });
          },

          toggleFeature: (feature) => {
            set((state) => {
              state.config.features[feature] = !state.config.features[feature];
            });
          },

          setOnlineStatus: (online) => {
            set((state) => {
              state.online = online;
            });
          },

          setGlobalLoading: (loading) => {
            set((state) => {
              state.globalLoading = loading;
            });
          },

          setLoadingState: (key, loading) => {
            set((state) => {
              if (loading) {
                state.loadingStates[key] = true;
              } else {
                delete state.loadingStates[key];
              }
            });
          },

          setGlobalError: (error) => {
            set((state) => {
              state.globalError = error;
              if (error) {
                state.metrics.errors += 1;
              }
            });
          },

          updateMetrics: (updates) => {
            set((state) => {
              state.metrics = { ...state.metrics, ...updates };
            });
          },

          incrementMetric: (metric) => {
            set((state) => {
              state.metrics[metric] += 1;
            });
          },

          setFeatureFlag: (flag, enabled) => {
            set((state) => {
              state.featureFlags[flag] = enabled;
            });
          },

          getFeatureFlag: (flag) => {
            return get().featureFlags[flag] ?? false;
          },

          reset: () => {
            set((state) => {
              state.config = defaultConfig;
              state.featureFlags = {};
              state.globalLoading = false;
              state.loadingStates = {};
              state.globalError = null;
            });
          },

          resetMetrics: () => {
            set((state) => {
              state.metrics = { ...defaultMetrics, sessionStart: new Date() };
            });
          },
        }))
      ),
      {
        name: 'app-store',
        partialize: (state) => ({
          config: state.config,
          featureFlags: state.featureFlags,
        }),
      }
    ),
    { name: 'AppStore' }
  )
);

// Helper Functions
async function loadAppConfig(): Promise<Partial<AppConfig>> {
  try {
    // In a real app, this would fetch from an API
    const response = await fetch('/api/config');
    if (response.ok) {
      return await response.json();
    }
  } catch (error) {
    console.warn('Failed to load remote config, using defaults');
  }
  return {};
}

async function loadFeatureFlags(): Promise<Record<string, boolean>> {
  try {
    // In a real app, this would fetch from a feature flag service
    const response = await fetch('/api/feature-flags');
    if (response.ok) {
      return await response.json();
    }
  } catch (error) {
    console.warn('Failed to load feature flags, using defaults');
  }
  return {};
}

function setupPerformanceMonitoring(): void {
  // Setup performance observers
  if ('PerformanceObserver' in window) {
    const observer = new PerformanceObserver((list) => {
      const entries = list.getEntries();
      // Process performance entries
      console.log('Performance entries:', entries);
    });
    
    observer.observe({ entryTypes: ['navigation', 'resource', 'measure'] });
  }
}

// Selectors
export const useAppConfig = () => useAppStore((state) => state.config);
export const useAppMetrics = () => useAppStore((state) => state.metrics);
export const useFeatureFlags = () => useAppStore((state) => state.featureFlags);
export const useGlobalLoading = () => useAppStore((state) => state.globalLoading);
export const useOnlineStatus = () => useAppStore((state) => state.online);
