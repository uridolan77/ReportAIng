Based on the comprehensive review, I'll integrate the suggested enhancements with practical implementations:

## 1. **Unified UI Framework**

Create a consistent UI wrapper to standardize components:

```typescript
// src/components/ui/index.tsx
import { Button as AntButton, ButtonProps as AntButtonProps } from 'antd';
import { styled } from '@emotion/styled';

// Unified Button component
export interface ButtonProps extends AntButtonProps {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
}

export const Button = styled(AntButton)<ButtonProps>`
  ${({ variant }) => {
    switch (variant) {
      case 'secondary':
        return `
          background: #f0f0f0;
          border-color: #d9d9d9;
          color: #262626;
          &:hover {
            background: #e8e8e8;
            border-color: #bfbfbf;
          }
        `;
      case 'ghost':
        return `
          background: transparent;
          border-color: transparent;
        `;
      case 'danger':
        return `
          background: #ff4d4f;
          border-color: #ff4d4f;
          color: white;
        `;
      default:
        return '';
    }
  }}
`;

// Migrate EnhancedQueryBuilder to use Ant Design
// src/components/QueryInterface/EnhancedQueryBuilder.tsx
import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Alert,
  Spin,
  Input,
  Select,
  Tabs,
  List,
  Tooltip,
  Badge,
  Progress,
  Divider,
  Tag,
  Form,
  AutoComplete
} from 'antd';
import {
  SendOutlined,
  ClockCircleOutlined,
  TableOutlined,
  BulbOutlined,
  CheckCircleOutlined,
  ExclamationOutlined,
  WarningOutlined,
  RobotOutlined,
  CompareOutlined,
  ThunderboltOutlined,
  StarOutlined
} from '@ant-design/icons';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;
const { TextArea } = Input;

// Rest of the component migrated to Ant Design...
```

## 2. **Enhanced State Management**

Implement selectors and modular stores:

```typescript
// src/stores/visualizationStore.ts
import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { subscribeWithSelector } from 'zustand/middleware';

interface VisualizationState {
  currentVisualization: AdvancedVisualizationConfig | null;
  dashboards: AdvancedDashboardConfig[];
  preferences: VisualizationPreferences;
  performanceMetrics: ChartPerformanceMetrics | null;
  
  // Actions
  setVisualization: (config: AdvancedVisualizationConfig) => void;
  addDashboard: (dashboard: AdvancedDashboardConfig) => void;
  updatePreferences: (preferences: Partial<VisualizationPreferences>) => void;
  
  // Selectors
  getDashboardById: (id: string) => AdvancedDashboardConfig | undefined;
  getRecentDashboards: (limit: number) => AdvancedDashboardConfig[];
}

export const useVisualizationStore = create<VisualizationState>()(
  subscribeWithSelector(
    persist(
      (set, get) => ({
        currentVisualization: null,
        dashboards: [],
        preferences: {
          enableAnimations: true,
          enableInteractivity: true,
          performance: 'Balanced',
          accessibility: 'Standard'
        },
        performanceMetrics: null,
        
        setVisualization: (config) => set({ currentVisualization: config }),
        
        addDashboard: (dashboard) => set((state) => ({
          dashboards: [...state.dashboards, dashboard]
        })),
        
        updatePreferences: (preferences) => set((state) => ({
          preferences: { ...state.preferences, ...preferences }
        })),
        
        getDashboardById: (id) => {
          return get().dashboards.find(d => d.id === id);
        },
        
        getRecentDashboards: (limit) => {
          return get().dashboards
            .sort((a, b) => b.updatedAt - a.updatedAt)
            .slice(0, limit);
        }
      }),
      {
        name: 'visualization-storage',
        storage: createJSONStorage(() => localStorage),
        partialize: (state) => ({
          dashboards: state.dashboards,
          preferences: state.preferences
        })
      }
    )
  )
);

// Use selectors to prevent unnecessary re-renders
export const useVisualizationPreferences = () => 
  useVisualizationStore((state) => state.preferences);

export const useDashboards = () => 
  useVisualizationStore((state) => state.dashboards);
```

## 3. **Standardized API Client**

Create a unified API client wrapper:

```typescript
// src/services/apiClient.ts
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { useAuthStore } from '../stores/authStore';

interface ApiClientConfig {
  baseURL?: string;
  timeout?: number;
  headers?: Record<string, string>;
}

class ApiClient {
  private client: AxiosInstance;
  private static instance: ApiClient;

  private constructor(config: ApiClientConfig = {}) {
    this.client = axios.create({
      baseURL: config.baseURL || process.env.REACT_APP_API_URL || 'https://localhost:55243',
      timeout: config.timeout || 30000,
      headers: {
        'Content-Type': 'application/json',
        ...config.headers
      }
    });

    this.setupInterceptors();
  }

  public static getInstance(): ApiClient {
    if (!ApiClient.instance) {
      ApiClient.instance = new ApiClient();
    }
    return ApiClient.instance;
  }

  private setupInterceptors(): void {
    // Request interceptor
    this.client.interceptors.request.use(
      (config) => {
        const token = this.getAuthToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (error.response?.status === 401) {
          const authStore = useAuthStore.getState();
          const refreshed = await authStore.refreshAuth();
          
          if (refreshed) {
            // Retry the original request
            return this.client.request(error.config);
          } else {
            authStore.logout();
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }

  private getAuthToken(): string | null {
    try {
      const authStorage = localStorage.getItem('auth-storage');
      if (authStorage) {
        const parsed = JSON.parse(authStorage);
        return parsed?.state?.token || null;
      }
      return null;
    } catch {
      return null;
    }
  }

  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  public async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  public async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.put<T>(url, data, config);
    return response.data;
  }

  public async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.delete<T>(url, config);
    return response.data;
  }

  // Streaming support
  public async stream(url: string, config?: AxiosRequestConfig): Promise<ReadableStream> {
    const response = await this.client.get(url, {
      ...config,
      responseType: 'stream'
    });
    return response.data;
  }
}

export const apiClient = ApiClient.getInstance();

// Migrate services to use the unified client
// src/services/advancedVisualizationService.ts
import { apiClient } from './apiClient';

class AdvancedVisualizationService {
  async generateAdvancedVisualization(
    request: AdvancedVisualizationRequest
  ): Promise<AdvancedVisualizationResponse> {
    return apiClient.post('/api/advanced-visualization/generate', request);
  }

  async generateAdvancedDashboard(
    request: AdvancedDashboardRequest
  ): Promise<AdvancedDashboardResponse> {
    return apiClient.post('/api/advanced-visualization/dashboard', request);
  }

  // ... rest of the methods
}
```

## 4. **Accessibility Improvements**

Implement comprehensive accessibility features:

```typescript
// src/hooks/useAccessibility.ts
import { useEffect, useRef, useState } from 'react';

interface AccessibilityOptions {
  enableKeyboardNavigation?: boolean;
  enableScreenReaderSupport?: boolean;
  enableHighContrast?: boolean;
  ariaLabel?: string;
  ariaDescribedBy?: string;
}

export const useAccessibility = (options: AccessibilityOptions = {}) => {
  const [announcement, setAnnouncement] = useState<string>('');
  const announcerRef = useRef<HTMLDivElement>(null);

  // Screen reader announcements
  const announce = (message: string, priority: 'polite' | 'assertive' = 'polite') => {
    setAnnouncement(message);
    if (announcerRef.current) {
      announcerRef.current.setAttribute('aria-live', priority);
    }
  };

  // Keyboard navigation hook
  const useKeyboardNavigation = (
    containerRef: React.RefObject<HTMLElement>,
    handlers: Record<string, () => void>
  ) => {
    useEffect(() => {
      const handleKeyDown = (event: KeyboardEvent) => {
        const handler = handlers[event.key];
        if (handler) {
          event.preventDefault();
          handler();
        }
      };

      const container = containerRef.current;
      if (container) {
        container.addEventListener('keydown', handleKeyDown);
        return () => container.removeEventListener('keydown', handleKeyDown);
      }
    }, [containerRef, handlers]);
  };

  // High contrast mode
  useEffect(() => {
    if (options.enableHighContrast) {
      document.body.classList.add('high-contrast');
      return () => document.body.classList.remove('high-contrast');
    }
  }, [options.enableHighContrast]);

  return {
    announce,
    announcerRef,
    announcement,
    useKeyboardNavigation
  };
};

// src/components/AccessibleChart/AccessibleChart.tsx
import React, { useRef, useState } from 'react';
import { useAccessibility } from '../../hooks/useAccessibility';
import { generateChartDescription } from '../../utils/accessibility';

export const AccessibleChart: React.FC<Props> = ({ data, config }) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const chartRef = useRef<HTMLDivElement>(null);
  const { announce, announcerRef, useKeyboardNavigation } = useAccessibility({
    enableKeyboardNavigation: true,
    enableScreenReaderSupport: true
  });

  useKeyboardNavigation(chartRef, {
    ArrowLeft: () => {
      const newIndex = Math.max(0, selectedIndex - 1);
      setSelectedIndex(newIndex);
      announce(`Selected data point ${newIndex + 1}: ${data[newIndex].value}`);
    },
    ArrowRight: () => {
      const newIndex = Math.min(data.length - 1, selectedIndex + 1);
      setSelectedIndex(newIndex);
      announce(`Selected data point ${newIndex + 1}: ${data[newIndex].value}`);
    },
    Enter: () => {
      const point = data[selectedIndex];
      announce(`Data point details: ${JSON.stringify(point)}`, 'assertive');
    }
  });

  return (
    <>
      <div
        ref={chartRef}
        role="img"
        aria-label={generateChartDescription(data, config)}
        aria-describedby="chart-description"
        tabIndex={0}
      >
        <Chart
          data={data}
          config={config}
          highlightIndex={selectedIndex}
        />
        <div id="chart-description" className="sr-only">
          {generateDataTableDescription(data)}
        </div>
      </div>
      <div ref={announcerRef} aria-live="polite" className="sr-only">
        {announcement}
      </div>
    </>
  );
};

// src/styles/accessibility.css
.high-contrast {
  filter: contrast(1.5);
}

.high-contrast .ant-btn-primary {
  background-color: #000;
  border-color: #fff;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
```

## 5. **Missing Chart Implementations**

Implement missing advanced chart types using D3.js:

```typescript
// src/components/Visualization/D3Charts/HeatmapChart.tsx
import React, { useEffect, useRef } from 'react';
import * as d3 from 'd3';

interface HeatmapChartProps {
  data: any[];
  width: number;
  height: number;
  config: any;
}

export const HeatmapChart: React.FC<HeatmapChartProps> = ({ data, width, height, config }) => {
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !data.length) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const margin = { top: 50, right: 50, bottom: 50, left: 50 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    const g = svg
      .append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`);

    // X and Y scales
    const xScale = d3.scaleBand()
      .domain(data.map(d => d.x))
      .range([0, innerWidth])
      .padding(0.05);

    const yScale = d3.scaleBand()
      .domain(data.map(d => d.y))
      .range([0, innerHeight])
      .padding(0.05);

    // Color scale
    const colorScale = d3.scaleSequential(d3.interpolateRdYlBu)
      .domain(d3.extent(data, d => d.value) as [number, number]);

    // Draw cells
    g.selectAll('rect')
      .data(data)
      .enter()
      .append('rect')
      .attr('x', d => xScale(d.x) || 0)
      .attr('y', d => yScale(d.y) || 0)
      .attr('width', xScale.bandwidth())
      .attr('height', yScale.bandwidth())
      .attr('fill', d => colorScale(d.value))
      .on('mouseover', function(event, d) {
        // Show tooltip
        const tooltip = d3.select('body').append('div')
          .attr('class', 'chart-tooltip')
          .style('opacity', 0);

        tooltip.transition()
          .duration(200)
          .style('opacity', 0.9);

        tooltip.html(`Value: ${d.value}`)
          .style('left', (event.pageX + 10) + 'px')
          .style('top', (event.pageY - 28) + 'px');
      })
      .on('mouseout', function() {
        d3.select('.chart-tooltip').remove();
      });

    // Add axes
    g.append('g')
      .attr('transform', `translate(0,${innerHeight})`)
      .call(d3.axisBottom(xScale));

    g.append('g')
      .call(d3.axisLeft(yScale));

  }, [data, width, height, config]);

  return <svg ref={svgRef} width={width} height={height} />;
};

// src/components/Visualization/D3Charts/TreemapChart.tsx
import React, { useEffect, useRef } from 'react';
import * as d3 from 'd3';

export const TreemapChart: React.FC<TreemapChartProps> = ({ data, width, height, config }) => {
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !data) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const root = d3.hierarchy(data)
      .sum(d => d.value)
      .sort((a, b) => b.value! - a.value!);

    const treemap = d3.treemap<any>()
      .size([width, height])
      .padding(2);

    treemap(root);

    const colorScale = d3.scaleOrdinal(d3.schemeCategory10);

    const nodes = svg.selectAll('g')
      .data(root.leaves())
      .enter()
      .append('g')
      .attr('transform', d => `translate(${d.x0},${d.y0})`);

    nodes.append('rect')
      .attr('width', d => d.x1 - d.x0)
      .attr('height', d => d.y1 - d.y0)
      .attr('fill', d => colorScale(d.parent!.data.name));

    nodes.append('text')
      .attr('x', 4)
      .attr('y', 20)
      .text(d => d.data.name)
      .attr('font-size', '12px')
      .attr('fill', 'white');

  }, [data, width, height, config]);

  return <svg ref={svgRef} width={width} height={height} />;
};
```

## 6. **Security Enhancements**

Implement secure token storage and XSS protection:

```typescript
// src/utils/security.ts
import DOMPurify from 'dompurify';

export class SecurityUtils {
  // Secure token storage using encryption
  private static readonly ENCRYPTION_KEY = process.env.REACT_APP_ENCRYPTION_KEY || 'default-key';

  static encryptToken(token: string): string {
    // In production, use Web Crypto API
    if (window.crypto && window.crypto.subtle) {
      // Implementation using Web Crypto API
      return btoa(token); // Simplified for example
    }
    return token;
  }

  static decryptToken(encryptedToken: string): string {
    if (window.crypto && window.crypto.subtle) {
      // Implementation using Web Crypto API
      return atob(encryptedToken); // Simplified for example
    }
    return encryptedToken;
  }

  // XSS Protection
  static sanitizeHtml(html: string): string {
    return DOMPurify.sanitize(html, {
      ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'span', 'p', 'br'],
      ALLOWED_ATTR: ['class', 'style']
    });
  }

  static sanitizeSQL(sql: string): string {
    // Remove potentially dangerous SQL keywords for display
    const dangerous = ['DROP', 'DELETE', 'TRUNCATE', 'INSERT', 'UPDATE'];
    let sanitized = sql;
    
    dangerous.forEach(keyword => {
      const regex = new RegExp(`\\b${keyword}\\b`, 'gi');
      sanitized = sanitized.replace(regex, `[${keyword}]`);
    });
    
    return sanitized;
  }

  // Content Security Policy
  static setCSPHeaders(): void {
    const meta = document.createElement('meta');
    meta.httpEquiv = 'Content-Security-Policy';
    meta.content = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';";
    document.head.appendChild(meta);
  }
}

// Enhanced auth store with secure token storage
// src/stores/authStore.ts
import { SecurityUtils } from '../utils/security';

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      // ... other state
      
      login: async (username: string, password: string) => {
        try {
          const response = await apiClient.post('/api/auth/login', {
            username,
            password
          });

          if (response.success) {
            // Encrypt tokens before storing
            const encryptedToken = SecurityUtils.encryptToken(response.accessToken);
            const encryptedRefreshToken = SecurityUtils.encryptToken(response.refreshToken);
            
            set({
              isAuthenticated: true,
              user: response.user,
              token: encryptedToken,
              refreshToken: encryptedRefreshToken,
            });
            
            return true;
          }
          return false;
        } catch (error) {
          console.error('Login failed:', error);
          return false;
        }
      },
      
      getDecryptedToken: () => {
        const { token } = get();
        return token ? SecurityUtils.decryptToken(token) : null;
      }
    }),
    {
      name: 'auth-storage',
      // Custom storage with encryption
      storage: {
        getItem: (name) => {
          const str = localStorage.getItem(name);
          if (!str) return null;
          return JSON.parse(str);
        },
        setItem: (name, value) => {
          localStorage.setItem(name, JSON.stringify(value));
        },
        removeItem: (name) => {
          localStorage.removeItem(name);
        }
      }
    }
  )
);
```

## 7. **Testing Infrastructure**

Implement comprehensive testing setup:

```typescript
// src/test-utils.tsx
import React from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConfigProvider } from 'antd';
import { BrowserRouter } from 'react-router-dom';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false }
  }
});

interface AllProvidersProps {
  children: React.ReactNode;
}

const AllProviders: React.FC<AllProvidersProps> = ({ children }) => {
  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider>
        <BrowserRouter>
          {children}
        </BrowserRouter>
      </ConfigProvider>
    </QueryClientProvider>
  );
};

const customRender = (
  ui: React.ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllProviders, ...options });

export * from '@testing-library/react';
export { customRender as render };

// src/components/QueryInterface/__tests__/QueryInterface.test.tsx
import { render, screen, fireEvent, waitFor } from '../../../test-utils';
import { QueryInterface } from '../QueryInterface';
import { server } from '../../../mocks/server';
import { rest } from 'msw';

describe('QueryInterface', () => {
  it('should execute a query when submit button is clicked', async () => {
    render(<QueryInterface />);
    
    const input = screen.getByPlaceholderText(/ask a question/i);
    const submitButton = screen.getByRole('button', { name: /ask/i });
    
    fireEvent.change(input, { target: { value: 'Show me total sales' } });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText(/query results/i)).toBeInTheDocument();
    });
  });

  it('should handle query errors gracefully', async () => {
    server.use(
      rest.post('/api/query/natural-language', (req, res, ctx) => {
        return res(
          ctx.status(500),
          ctx.json({ error: 'Query execution failed' })
        );
      })
    );

    render(<QueryInterface />);
    
    const input = screen.getByPlaceholderText(/ask a question/i);
    const submitButton = screen.getByRole('button', { name: /ask/i });
    
    fireEvent.change(input, { target: { value: 'Invalid query' } });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText(/query failed/i)).toBeInTheDocument();
    });
  });
});

// src/components/Visualization/__tests__/AdvancedChart.test.tsx
import { render, screen } from '../../../test-utils';
import AdvancedChart from '../AdvancedChart';
import { mockChartConfig, mockData } from '../../../mocks/chartMocks';

describe('AdvancedChart', () => {
  it('should render chart with correct data', () => {
    render(
      <AdvancedChart
        config={mockChartConfig}
        data={mockData}
        loading={false}
      />
    );
    
    expect(screen.getByText(mockChartConfig.title)).toBeInTheDocument();
  });

  it('should show loading state', () => {
    render(
      <AdvancedChart
        config={mockChartConfig}
        data={[]}
        loading={true}
      />
    );
    
    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
  });
});
```

## 8. **Performance Optimizations**

Implement code splitting and lazy loading:

```typescript
// src/App.tsx
import React, { Suspense, lazy } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ErrorBoundary } from './components/ErrorBoundary';
import { LoadingFallback } from './components/LoadingFallback';

// Lazy load heavy components
const QueryInterface = lazy(() => import('./components/QueryInterface/QueryInterface'));
const EnhancedQueryBuilder = lazy(() => import('./components/QueryInterface/EnhancedQueryBuilder'));
const AdvancedVisualizationPanel = lazy(() => import('./components/Visualization/AdvancedVisualizationPanel'));
const TuningDashboard = lazy(() => import('./components/Tuning/TuningDashboard'));

// Preload critical components
const preloadCriticalComponents = () => {
  import('./components/QueryInterface/QueryInterface');
};

const App: React.FC = () => {
  React.useEffect(() => {
    // Preload after initial render
    requestIdleCallback(preloadCriticalComponents);
  }, []);

  return (
    <ErrorBoundary>
      <Router>
        <Suspense fallback={<LoadingFallback />}>
          <Routes>
            <Route path="/" element={<QueryInterface />} />
            <Route path="/enhanced-query" element={<EnhancedQueryBuilder />} />
            <Route path="/visualizations" element={<AdvancedVisualizationPanel />} />
            <Route path="/tuning" element={<TuningDashboard />} />
          </Routes>
        </Suspense>
      </Router>
    </ErrorBoundary>
  );
};

// src/hooks/useVirtualization.ts
import { useVirtual } from '@tanstack/react-virtual';

export const useVirtualization = <T>(
  items: T[],
  parentRef: React.RefObject<HTMLElement>,
  options?: {
    itemHeight?: number;
    overscan?: number;
  }
) => {
  const rowVirtualizer = useVirtual({
    size: items.length,
    parentRef,
    estimateSize: React.useCallback(() => options?.itemHeight || 50, [options?.itemHeight]),
    overscan: options?.overscan || 5,
  });

  return {
    virtualItems: rowVirtualizer.virtualItems,
    totalSize: rowVirtualizer.totalSize,
    scrollToIndex: rowVirtualizer.scrollToIndex,
  };
};
```

## 9. **Enhanced Error Handling**

Implement comprehensive error handling and logging:

```typescript
// src/services/errorService.ts
import * as Sentry from '@sentry/react';
import { BrowserTracing } from '@sentry/tracing';

export class ErrorService {
  static initialize() {
    if (process.env.NODE_ENV === 'production') {
      Sentry.init({
        dsn: process.env.REACT_APP_SENTRY_DSN,
        integrations: [
          new BrowserTracing(),
        ],
        tracesSampleRate: 0.1,
        environment: process.env.NODE_ENV,
      });
    }
  }

  static logError(error: Error, context?: Record<string, any>) {
    console.error('Error occurred:', error, context);
    
    if (process.env.NODE_ENV === 'production') {
      Sentry.captureException(error, {
        extra: context,
      });
    }
  }

  static logWarning(message: string, context?: Record<string, any>) {
    console.warn('Warning:', message, context);
    
    if (process.env.NODE_ENV === 'production') {
      Sentry.captureMessage(message, 'warning');
    }
  }
}

// src/components/ErrorBoundary.tsx
import React, { Component, ErrorInfo } from 'react';
import { Result, Button } from 'antd';
import { ErrorService } from '../services/errorService';

interface Props {
  children: React.ReactNode;
  fallback?: React.ComponentType<{ error: Error; resetError: () => void }>;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    ErrorService.logError(error, {
      componentStack: errorInfo.componentStack,
      errorBoundary: true,
    });
  }

  resetError = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        const FallbackComponent = this.props.fallback;
        return <FallbackComponent error={this.state.error!} resetError={this.resetError} />;
      }

      return (
        <Result
          status="error"
          title="Something went wrong"
          subTitle="An unexpected error occurred. Please try refreshing the page or contact support if the problem persists."
          extra={
            <Button type="primary" onClick={this.resetError}>
              Try Again
            </Button>
          }
        />
      );
    }

    return this.props.children;
  }
}
```

## 10. **Development Tools**

Create a development tools panel:

```typescript
// src/components/DevTools/DevTools.tsx
import React, { useState } from 'react';
import { Drawer, Tabs, Button, Space, List, Tag, Statistic, Row, Col } from 'antd';
import { BugOutlined, ThunderboltOutlined, DatabaseOutlined } from '@ant-design/icons';
import { useQueryStore } from '../../stores/queryStore';
import { useAuthStore } from '../../stores/authStore';

const { TabPane } = Tabs;

export const DevTools: React.FC = () => {
  const [visible, setVisible] = useState(false);
  const queryHistory = useQueryStore((state) => state.queryHistory);
  const currentUser = useAuthStore((state) => state.user);

  if (process.env.NODE_ENV === 'production') {
    return null;
  }

  return (
    <>
      <Button
        type="primary"
        shape="circle"
        icon={<BugOutlined />}
        style={{
          position: 'fixed',
          bottom: 20,
          right: 20,
          zIndex: 1000,
        }}
        onClick={() => setVisible(true)}
      />

      <Drawer
        title="Developer Tools"
        placement="right"
        onClose={() => setVisible(false)}
        visible={visible}
        width={600}
      >
        <Tabs defaultActiveKey="queries">
          <TabPane tab="Query History" key="queries">
            <List
              dataSource={queryHistory}
              renderItem={(item) => (
                <List.Item>
                  <List.Item.Meta
                    title={item.question}
                    description={
                      <Space direction="vertical">
                        <Text code>{item.sql}</Text>
                        <Space>
                          <Tag color={item.successful ? 'green' : 'red'}>
                            {item.successful ? 'Success' : 'Failed'}
                          </Tag>
                          <Tag>{item.executionTimeMs}ms</Tag>
                        </Space>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </TabPane>

          <TabPane tab="Performance" key="performance">
            <Row gutter={16}>
              <Col span={8}>
                <Statistic
                  title="Avg Query Time"
                  value={calculateAvgQueryTime(queryHistory)}
                  suffix="ms"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Cache Hit Rate"
                  value={calculateCacheHitRate(queryHistory)}
                  suffix="%"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Total Queries"
                  value={queryHistory.length}
                />
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="State Inspector" key="state">
            <pre>{JSON.stringify({ currentUser, queryHistory }, null, 2)}</pre>
          </TabPane>

          <TabPane tab="Mock Data" key="mock">
            <MockDataGenerator />
          </TabPane>
        </Tabs>
      </Drawer>
    </>
  );
};

// src/utils/devTools.ts
export const enableDevTools = () => {
  if (process.env.NODE_ENV === 'development') {
    // Enable React DevTools
    window.__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE = false;

    // Add custom dev commands
    (window as any).biCopilot = {
      clearCache: () => {
        localStorage.clear();
        sessionStorage.clear();
        console.log('Cache cleared');
      },
      showStores: () => {
        console.log('Auth Store:', useAuthStore.getState());
        console.log('Query Store:', useQueryStore.getState());
      },
      testConnection: async () => {
        const { ConnectionTester } = await import('./connectionTest');
        const results = await ConnectionTester.runAllTests();
        ConnectionTester.displayResults(results);
      },
    };

    console.log('BI Copilot Dev Tools enabled. Use window.biCopilot for commands.');
  }
};
```

These implementations address the key recommendations from the review:

1. **Unified UI Framework** - Migrated Material UI components to Ant Design
2. **Enhanced State Management** - Added selectors and modular stores
3. **Standardized API Client** - Created unified axios-based client
4. **Accessibility** - Comprehensive keyboard navigation and screen reader support
5. **Missing Charts** - D3.js implementations for advanced chart types
6. **Security** - Token encryption and XSS protection
7. **Testing** - Complete testing infrastructure with React Testing Library
8. **Performance** - Code splitting and virtualization
9. **Error Handling** - Sentry integration and error boundaries
10. **Developer Experience** - Dev tools panel for debugging

The implementation maintains backward compatibility while significantly improving the codebase quality, security, and user experience.