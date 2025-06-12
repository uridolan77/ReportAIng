/**
 * Advanced Debugging Utilities
 * 
 * Comprehensive debugging tools for React applications including
 * component profiling, state tracking, performance monitoring, and error analysis
 */

// Debug configuration
interface DebugConfig {
  enabled: boolean;
  logLevel: 'debug' | 'info' | 'warn' | 'error';
  enablePerformanceTracking: boolean;
  enableStateTracking: boolean;
  enableComponentProfiling: boolean;
  maxLogEntries: number;
}

// Performance tracking
interface PerformanceTracker {
  name: string;
  startTime: number;
  endTime?: number;
  duration?: number;
  metadata?: Record<string, any>;
}

// Component profiling
interface ComponentProfile {
  name: string;
  renderCount: number;
  totalRenderTime: number;
  averageRenderTime: number;
  lastRenderTime: number;
  props: any[];
  state: any[];
}

// Error tracking
interface ErrorInfo {
  message: string;
  stack?: string;
  componentStack?: string;
  timestamp: number;
  userAgent: string;
  url: string;
  userId?: string;
}

class DebugManager {
  private static instance: DebugManager;
  private config: DebugConfig;
  private performanceTrackers = new Map<string, PerformanceTracker>();
  private componentProfiles = new Map<string, ComponentProfile>();
  private errorLog: ErrorInfo[] = [];
  private stateHistory: Array<{ timestamp: number; state: any; action?: string }> = [];

  private constructor() {
    this.config = {
      enabled: process.env.NODE_ENV === 'development',
      logLevel: 'debug',
      enablePerformanceTracking: true,
      enableStateTracking: true,
      enableComponentProfiling: true,
      maxLogEntries: 1000
    };
  }

  static getInstance(): DebugManager {
    if (!DebugManager.instance) {
      DebugManager.instance = new DebugManager();
    }
    return DebugManager.instance;
  }

  // Configuration
  updateConfig(newConfig: Partial<DebugConfig>): void {
    this.config = { ...this.config, ...newConfig };
  }

  getConfig(): DebugConfig {
    return { ...this.config };
  }

  // Performance tracking
  startPerformanceTracking(name: string, metadata?: Record<string, any>): void {
    if (!this.config.enabled || !this.config.enablePerformanceTracking) return;

    this.performanceTrackers.set(name, {
      name,
      startTime: performance.now(),
      metadata
    });
  }

  endPerformanceTracking(name: string): number | null {
    if (!this.config.enabled || !this.config.enablePerformanceTracking) return null;

    const tracker = this.performanceTrackers.get(name);
    if (!tracker) return null;

    const endTime = performance.now();
    const duration = endTime - tracker.startTime;

    tracker.endTime = endTime;
    tracker.duration = duration;

    this.log('debug', `Performance: ${name} took ${duration.toFixed(2)}ms`, tracker);
    return duration;
  }

  getPerformanceMetrics(): PerformanceTracker[] {
    return Array.from(this.performanceTrackers.values())
      .filter(tracker => tracker.duration !== undefined);
  }

  // Component profiling
  profileComponent(componentName: string, renderTime: number, props?: any, state?: any): void {
    if (!this.config.enabled || !this.config.enableComponentProfiling) return;

    let profile = this.componentProfiles.get(componentName);
    if (!profile) {
      profile = {
        name: componentName,
        renderCount: 0,
        totalRenderTime: 0,
        averageRenderTime: 0,
        lastRenderTime: 0,
        props: [],
        state: []
      };
      this.componentProfiles.set(componentName, profile);
    }

    profile.renderCount++;
    profile.totalRenderTime += renderTime;
    profile.averageRenderTime = profile.totalRenderTime / profile.renderCount;
    profile.lastRenderTime = renderTime;

    if (props) profile.props.push({ timestamp: Date.now(), props });
    if (state) profile.state.push({ timestamp: Date.now(), state });

    // Keep only last 10 entries
    profile.props = profile.props.slice(-10);
    profile.state = profile.state.slice(-10);
  }

  getComponentProfiles(): ComponentProfile[] {
    return Array.from(this.componentProfiles.values());
  }

  // State tracking
  trackStateChange(state: any, action?: string): void {
    if (!this.config.enabled || !this.config.enableStateTracking) return;

    this.stateHistory.push({
      timestamp: Date.now(),
      state: JSON.parse(JSON.stringify(state)), // Deep clone
      action
    });

    // Keep only recent entries
    this.stateHistory = this.stateHistory.slice(-this.config.maxLogEntries);
  }

  getStateHistory(): Array<{ timestamp: number; state: any; action?: string }> {
    return [...this.stateHistory];
  }

  // Error tracking
  trackError(error: Error, componentStack?: string, userId?: string): void {
    const errorInfo: ErrorInfo = {
      message: error.message,
      stack: error.stack,
      componentStack,
      timestamp: Date.now(),
      userAgent: navigator.userAgent,
      url: window.location.href,
      userId
    };

    this.errorLog.push(errorInfo);
    this.errorLog = this.errorLog.slice(-this.config.maxLogEntries);

    this.log('error', 'Error tracked', errorInfo);
  }

  getErrorLog(): ErrorInfo[] {
    return [...this.errorLog];
  }

  // Logging
  log(level: DebugConfig['logLevel'], message: string, data?: any): void {
    if (!this.config.enabled) return;

    const levels = ['debug', 'info', 'warn', 'error'];
    const currentLevelIndex = levels.indexOf(this.config.logLevel);
    const messageLevelIndex = levels.indexOf(level);

    if (messageLevelIndex < currentLevelIndex) return;

    const timestamp = new Date().toISOString();
    const logMessage = `[${timestamp}] [${level.toUpperCase()}] ${message}`;

    switch (level) {
      case 'debug':
        console.debug(logMessage, data);
        break;
      case 'info':
        console.info(logMessage, data);
        break;
      case 'warn':
        console.warn(logMessage, data);
        break;
      case 'error':
        console.error(logMessage, data);
        break;
    }
  }

  // Export debug data
  exportDebugData(): string {
    const debugData = {
      config: this.config,
      performance: this.getPerformanceMetrics(),
      components: this.getComponentProfiles(),
      errors: this.getErrorLog(),
      stateHistory: this.getStateHistory(),
      timestamp: new Date().toISOString()
    };

    return JSON.stringify(debugData, null, 2);
  }

  // Clear debug data
  clearDebugData(): void {
    this.performanceTrackers.clear();
    this.componentProfiles.clear();
    this.errorLog = [];
    this.stateHistory = [];
  }
}

// Singleton instance
export const debugManager = DebugManager.getInstance();

// Utility functions
export const debug = {
  // Performance tracking
  time: (name: string, metadata?: Record<string, any>) => {
    debugManager.startPerformanceTracking(name, metadata);
  },

  timeEnd: (name: string) => {
    return debugManager.endPerformanceTracking(name);
  },

  // Component profiling
  profileRender: (componentName: string, renderFn: () => any, props?: any, state?: any) => {
    const startTime = performance.now();
    const result = renderFn();
    const renderTime = performance.now() - startTime;
    debugManager.profileComponent(componentName, renderTime, props, state);
    return result;
  },

  // State tracking
  trackState: (state: any, action?: string) => {
    debugManager.trackStateChange(state, action);
  },

  // Error tracking
  trackError: (error: Error, componentStack?: string, userId?: string) => {
    debugManager.trackError(error, componentStack, userId);
  },

  // Logging
  log: (message: string, data?: any) => debugManager.log('debug', message, data),
  info: (message: string, data?: any) => debugManager.log('info', message, data),
  warn: (message: string, data?: any) => debugManager.log('warn', message, data),
  error: (message: string, data?: any) => debugManager.log('error', message, data),

  // Data export
  export: () => debugManager.exportDebugData(),
  clear: () => debugManager.clearDebugData(),

  // Configuration
  config: (newConfig: Partial<DebugConfig>) => debugManager.updateConfig(newConfig),
  getConfig: () => debugManager.getConfig()
};

// React hooks for debugging
export const useDebugRender = (componentName: string, props?: any, state?: any) => {
  const renderCount = React.useRef(0);
  
  React.useEffect(() => {
    renderCount.current++;
    const startTime = performance.now();
    
    return () => {
      const renderTime = performance.now() - startTime;
      debugManager.profileComponent(componentName, renderTime, props, state);
    };
  });

  return renderCount.current;
};

export const useDebugState = <T>(initialState: T, stateName?: string): [T, (newState: T) => void] => {
  const [state, setState] = React.useState(initialState);
  
  const setDebugState = React.useCallback((newState: T) => {
    debugManager.trackStateChange(newState, stateName);
    setState(newState);
  }, [stateName]);

  return [state, setDebugState];
};

// Error boundary with debugging
export class DebugErrorBoundary extends React.Component<
  { children: React.ReactNode; componentName?: string },
  { hasError: boolean }
> {
  constructor(props: { children: React.ReactNode; componentName?: string }) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): { hasError: boolean } {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    debugManager.trackError(error, errorInfo.componentStack);
    debugManager.log('error', `Error in ${this.props.componentName || 'Unknown Component'}`, {
      error: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack
    });
  }

  render() {
    if (this.state.hasError) {
      return (
        <div style={{ padding: '20px', border: '1px solid #ff4d4f', borderRadius: '4px' }}>
          <h3>Something went wrong in {this.props.componentName || 'this component'}</h3>
          <p>Check the debug tools for more information.</p>
        </div>
      );
    }

    return this.props.children;
  }
}

// Development-only imports
let React: any;
if (typeof window !== 'undefined') {
  React = require('react');
}

export default debug;
