/**
 * API Testing Utilities
 * Utilities for testing API calls, services, and data fetching
 */

import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { QueryClient } from '@tanstack/react-query';
import { 
  createMockQueryResult, 
  createMockHealthStatus, 
  createMockTemplate, 
  createMockShortcut 
} from './testing-providers';

// API Base URL for testing
export const TEST_API_BASE_URL = 'http://localhost:55244/api';

// Mock API Responses
export const mockApiResponses = {
  // Query API responses
  executeQuery: {
    success: createMockQueryResult(),
    error: {
      success: false,
      error: 'Database connection failed',
      details: 'Unable to connect to the database server',
    },
    timeout: {
      success: false,
      error: 'Query timeout',
      details: 'Query execution exceeded the maximum allowed time',
    },
  },

  // Health API responses
  health: {
    healthy: createMockHealthStatus(),
    unhealthy: createMockHealthStatus({
      database: { connected: false, responseTime: 0, lastCheck: '2024-01-01T00:00:00Z' },
      overall: 'unhealthy',
    }),
  },

  // Tuning API responses
  tuning: {
    dashboard: {
      totalQueries: 150,
      successRate: 0.95,
      avgResponseTime: 250,
      popularTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players'],
      recentActivity: [],
    },
    businessTables: [
      {
        id: 1,
        tableName: 'tbl_Daily_actions',
        businessName: 'Daily Player Actions',
        description: 'Main table containing daily player statistics',
        columns: [
          { name: 'PlayerID', businessName: 'Player ID', description: 'Unique player identifier' },
          { name: 'ActionDate', businessName: 'Action Date', description: 'Date of the action' },
        ],
      },
    ],
    glossary: [
      {
        id: 1,
        term: 'GGR',
        definition: 'Gross Gaming Revenue - total amount wagered minus winnings paid out',
        category: 'financial',
      },
    ],
  },

  // Authentication responses
  auth: {
    login: {
      success: true,
      token: 'mock-jwt-token',
      user: {
        id: 1,
        username: 'testuser',
        email: 'test@example.com',
        roles: ['user'],
      },
    },
    refresh: {
      success: true,
      token: 'new-mock-jwt-token',
    },
  },
};

// MSW Request Handlers
export const createApiHandlers = (overrides: Record<string, any> = {}) => [
  // Query API
  http.post(`${TEST_API_BASE_URL}/query/execute`, () => {
    const response = overrides.executeQuery || mockApiResponses.executeQuery.success;
    return HttpResponse.json(response);
  }),

  http.get(`${TEST_API_BASE_URL}/query/tables`, () => {
    const response = overrides.tables || ['tbl_Daily_actions', 'tbl_Daily_actions_players'];
    return HttpResponse.json(response);
  }),

  // Health API
  http.get(`${TEST_API_BASE_URL}/health`, () => {
    const response = overrides.health || mockApiResponses.health.healthy;
    return HttpResponse.json(response);
  }),

  // Tuning API
  http.get(`${TEST_API_BASE_URL}/tuning/dashboard`, () => {
    const response = overrides.tuningDashboard || mockApiResponses.tuning.dashboard;
    return HttpResponse.json(response);
  }),

  http.get(`${TEST_API_BASE_URL}/tuning/business-tables`, () => {
    const response = overrides.businessTables || mockApiResponses.tuning.businessTables;
    return HttpResponse.json(response);
  }),

  http.post(`${TEST_API_BASE_URL}/tuning/business-tables`, () => {
    return HttpResponse.json({ success: true, id: Date.now() });
  }),

  http.put(`${TEST_API_BASE_URL}/tuning/business-tables/:id`, () => {
    return HttpResponse.json({ success: true });
  }),

  http.get(`${TEST_API_BASE_URL}/tuning/glossary`, () => {
    const response = overrides.glossary || mockApiResponses.tuning.glossary;
    return HttpResponse.json(response);
  }),

  // Authentication API
  http.post(`${TEST_API_BASE_URL}/auth/login`, () => {
    const response = overrides.login || mockApiResponses.auth.login;
    return HttpResponse.json(response);
  }),

  http.post(`${TEST_API_BASE_URL}/auth/refresh`, () => {
    const response = overrides.refresh || mockApiResponses.auth.refresh;
    return HttpResponse.json(response);
  }),

  // Catch-all handler for unhandled requests
  http.all('*', ({ request }) => {
    console.warn(`Unhandled ${request.method} request to ${request.url}`);
    return new HttpResponse(null, { status: 404 });
  }),
];

// MSW Server Setup
export const createMockServer = (overrides: Record<string, any> = {}) => {
  return setupServer(...createApiHandlers(overrides));
};

// Default mock server
export const mockServer = createMockServer();

// API Testing Utilities
export class ApiTestUtils {
  static server = mockServer;

  static setupApiMocking() {
    beforeAll(() => this.server.listen({ onUnhandledRequest: 'warn' }));
    afterEach(() => this.server.resetHandlers());
    afterAll(() => this.server.close());
  }

  static mockApiResponse(endpoint: string, response: any, status: number = 200) {
    this.server.use(
      http.all(`${TEST_API_BASE_URL}${endpoint}`, () => {
        return HttpResponse.json(response, { status });
      })
    );
  }

  static mockApiError(endpoint: string, error: string, status: number = 500) {
    this.server.use(
      http.all(`${TEST_API_BASE_URL}${endpoint}`, () => {
        return HttpResponse.json({ error }, { status });
      })
    );
  }

  static mockApiDelay(endpoint: string, delay: number) {
    this.server.use(
      http.all(`${TEST_API_BASE_URL}${endpoint}`, async () => {
        await new Promise(resolve => setTimeout(resolve, delay));
        return HttpResponse.json({ success: true });
      })
    );
  }

  static mockNetworkError(endpoint: string) {
    this.server.use(
      http.all(`${TEST_API_BASE_URL}${endpoint}`, () => {
        return HttpResponse.error();
      })
    );
  }

  static getRequestHistory() {
    // This would require additional setup to track requests
    // For now, return empty array
    return [];
  }

  static clearRequestHistory() {
    // Implementation would clear the request history
  }
}

// Query Client Testing Utilities
export class QueryClientTestUtils {
  static createTestQueryClient(): QueryClient {
    return new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
          gcTime: 0,
          staleTime: 0,
        },
        mutations: {
          retry: false,
        },
      },
    });
  }

  static async waitForQuery(queryClient: QueryClient, queryKey: any[]) {
    await queryClient.ensureQueryData({
      queryKey,
      queryFn: () => Promise.resolve({}),
    });
  }

  static async invalidateQueries(queryClient: QueryClient, queryKey?: any[]) {
    await queryClient.invalidateQueries({ queryKey });
  }

  static getQueryData(queryClient: QueryClient, queryKey: any[]) {
    return queryClient.getQueryData(queryKey);
  }

  static setQueryData(queryClient: QueryClient, queryKey: any[], data: any) {
    queryClient.setQueryData(queryKey, data);
  }

  static getQueryState(queryClient: QueryClient, queryKey: any[]) {
    return queryClient.getQueryState(queryKey);
  }

  static getMutationState(queryClient: QueryClient, mutationKey?: any[]) {
    return queryClient.getMutationCache().findAll({ mutationKey });
  }
}

// Service Testing Utilities
export class ServiceTestUtils {
  static mockQueryTemplateService() {
    return {
      getTemplates: jest.fn(() => [createMockTemplate()]),
      getShortcuts: jest.fn(() => [createMockShortcut()]),
      searchSuggestions: jest.fn(() => []),
      createTemplate: jest.fn((template) => ({ ...template, id: 'new-id' })),
      createShortcut: jest.fn((shortcut) => ({ ...shortcut, id: 'new-id' })),
      toggleFavorite: jest.fn(),
      incrementUsage: jest.fn(),
      processTemplate: jest.fn((template, variables) => template.template),
      addToRecent: jest.fn(),
      getCategories: jest.fn(() => ['financial', 'operational']),
      getPopularTemplates: jest.fn(() => []),
      getFavoriteTemplates: jest.fn(() => []),
    };
  }

  static mockApiService() {
    return {
      executeQuery: jest.fn(() => Promise.resolve(mockApiResponses.executeQuery.success)),
      getTableDetails: jest.fn(() => Promise.resolve([])),
      getHealthStatus: jest.fn(() => Promise.resolve(mockApiResponses.health.healthy)),
      refreshToken: jest.fn(() => Promise.resolve(mockApiResponses.auth.refresh)),
    };
  }

  static mockTuningApiService() {
    return {
      getDashboard: jest.fn(() => Promise.resolve(mockApiResponses.tuning.dashboard)),
      getBusinessTables: jest.fn(() => Promise.resolve(mockApiResponses.tuning.businessTables)),
      updateBusinessTable: jest.fn(() => Promise.resolve({ success: true })),
      getBusinessGlossary: jest.fn(() => Promise.resolve(mockApiResponses.tuning.glossary)),
      getQueryPatterns: jest.fn(() => Promise.resolve([])),
      getPromptLogs: jest.fn(() => Promise.resolve([])),
      autoGenerateTableContexts: jest.fn(() => Promise.resolve([])),
      autoGenerateGlossaryTerms: jest.fn(() => Promise.resolve([])),
      clearPromptCache: jest.fn(() => Promise.resolve({ success: true })),
    };
  }

  static mockSecurityService() {
    return {
      signRequest: jest.fn(() => 'mock-signature'),
      validateSignature: jest.fn(() => true),
      generateNonce: jest.fn(() => 'mock-nonce'),
      hashData: jest.fn(() => 'mock-hash'),
      encryptData: jest.fn(() => 'encrypted-data'),
      decryptData: jest.fn(() => 'decrypted-data'),
    };
  }
}

// HTTP Testing Utilities
export class HttpTestUtils {
  static createMockFetch(responses: Record<string, any> = {}) {
    return jest.fn((url: string, options?: RequestInit) => {
      const response = responses[url] || { ok: true, json: () => Promise.resolve({}) };
      
      return Promise.resolve({
        ok: response.ok !== false,
        status: response.status || 200,
        statusText: response.statusText || 'OK',
        json: () => Promise.resolve(response.data || response.json || {}),
        text: () => Promise.resolve(response.text || ''),
        blob: () => Promise.resolve(new Blob()),
        headers: new Headers(response.headers || {}),
        url,
      } as Response);
    });
  }

  static mockFetchSuccess(data: any) {
    global.fetch = jest.fn(() =>
      Promise.resolve({
        ok: true,
        status: 200,
        json: () => Promise.resolve(data),
      } as Response)
    );
  }

  static mockFetchError(error: string, status: number = 500) {
    global.fetch = jest.fn(() =>
      Promise.resolve({
        ok: false,
        status,
        statusText: error,
        json: () => Promise.resolve({ error }),
      } as Response)
    );
  }

  static mockFetchNetworkError() {
    global.fetch = jest.fn(() => Promise.reject(new Error('Network error')));
  }

  static restoreFetch() {
    if (global.fetch && 'mockRestore' in global.fetch) {
      (global.fetch as jest.Mock).mockRestore();
    }
  }
}

// WebSocket Testing Utilities
export class WebSocketTestUtils {
  static createMockWebSocket() {
    const mockWebSocket = {
      send: jest.fn(),
      close: jest.fn(),
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      readyState: WebSocket.OPEN,
      CONNECTING: WebSocket.CONNECTING,
      OPEN: WebSocket.OPEN,
      CLOSING: WebSocket.CLOSING,
      CLOSED: WebSocket.CLOSED,
    };

    global.WebSocket = jest.fn(() => mockWebSocket) as any;
    return mockWebSocket;
  }

  static simulateWebSocketMessage(mockWebSocket: any, data: any) {
    const messageEvent = new MessageEvent('message', { data: JSON.stringify(data) });
    mockWebSocket.onmessage?.(messageEvent);
  }

  static simulateWebSocketOpen(mockWebSocket: any) {
    const openEvent = new Event('open');
    mockWebSocket.onopen?.(openEvent);
  }

  static simulateWebSocketClose(mockWebSocket: any) {
    const closeEvent = new CloseEvent('close');
    mockWebSocket.onclose?.(closeEvent);
  }

  static simulateWebSocketError(mockWebSocket: any, error: Error) {
    const errorEvent = new ErrorEvent('error', { error });
    mockWebSocket.onerror?.(errorEvent);
  }
}

// Local Storage Testing Utilities
export class StorageTestUtils {
  static mockLocalStorage() {
    const store: Record<string, string> = {};
    
    return {
      getItem: jest.fn((key: string) => store[key] || null),
      setItem: jest.fn((key: string, value: string) => {
        store[key] = value;
      }),
      removeItem: jest.fn((key: string) => {
        delete store[key];
      }),
      clear: jest.fn(() => {
        Object.keys(store).forEach(key => delete store[key]);
      }),
      get length() {
        return Object.keys(store).length;
      },
      key: jest.fn((index: number) => Object.keys(store)[index] || null),
    };
  }

  static mockSessionStorage() {
    return this.mockLocalStorage(); // Same interface
  }

  static setupStorageMocks() {
    Object.defineProperty(window, 'localStorage', {
      value: this.mockLocalStorage(),
      writable: true,
    });

    Object.defineProperty(window, 'sessionStorage', {
      value: this.mockSessionStorage(),
      writable: true,
    });
  }
}

// All utilities are already exported above
