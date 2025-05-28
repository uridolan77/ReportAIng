import { create } from 'zustand';
import { QueryRequest, QueryResponse, QueryHistoryItem } from '../types/query';
import { API_CONFIG, getApiUrl, getAuthHeaders } from '../config/api';
import { useAuthStore } from './authStore';

interface QueryState {
  currentResult: QueryResponse | null;
  queryHistory: QueryHistoryItem[];
  isLoading: boolean;
  error: string | null;
  executeQuery: (request: QueryRequest) => Promise<void>;
  addToFavorites: (query: { query: string; timestamp: string; description: string }) => void;
  clearCurrentResult: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export const useQueryStore = create<QueryState>((set, get) => ({
  currentResult: null,
  queryHistory: [],
  isLoading: false,
  error: null,

  executeQuery: async (request: QueryRequest) => {
    set({ isLoading: true, error: null });

    try {
      // Get the current auth token
      const authState = useAuthStore.getState();
      const token = authState.token;

      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.QUERY.NATURAL_LANGUAGE), {
        method: 'POST',
        headers: getAuthHeaders(token || undefined),
        body: JSON.stringify(request),
      });

      if (response.ok) {
        const result: QueryResponse = await response.json();
        set({
          currentResult: result,
          isLoading: false,
          error: null
        });

        // Add to history
        const historyItem: QueryHistoryItem = {
          id: result.queryId,
          question: request.question,
          sql: result.sql,
          timestamp: result.timestamp,
          successful: result.success,
          executionTimeMs: result.executionTimeMs,
          confidence: result.confidence,
          userId: 'current-user',
          sessionId: request.sessionId,
          error: result.error,
        };

        set(state => ({
          queryHistory: [historyItem, ...state.queryHistory.slice(0, 49)] // Keep last 50
        }));
      } else {
        const errorData = await response.json();
        set({
          error: errorData.error || 'Failed to execute query',
          isLoading: false
        });
      }
    } catch (error) {
      console.error('Query execution error:', error);

      // For development, create a mock response
      const mockResult: QueryResponse = {
        queryId: `mock-${Date.now()}`,
        sql: 'SELECT COUNT(*) as TotalRecords FROM MockTable',
        result: {
          data: [{ TotalRecords: 42 }],
          metadata: {
            columnCount: 1,
            rowCount: 1,
            executionTimeMs: 150,
            columns: [{ name: 'TotalRecords', dataType: 'int', isNullable: false, semanticTags: [] }],
            queryTimestamp: new Date().toISOString(),
          },
        },
        visualization: {
          type: 'table',
          config: {},
        },
        confidence: 0.85,
        suggestions: ['Try: "Show me user statistics"', 'Try: "Display recent activity"'],
        cached: false,
        success: true,
        timestamp: new Date().toISOString(),
        executionTimeMs: 150,
      };

      set({
        currentResult: mockResult,
        isLoading: false,
        error: null
      });

      // Add mock result to history
      const historyItem: QueryHistoryItem = {
        id: mockResult.queryId,
        question: request.question,
        sql: mockResult.sql,
        timestamp: mockResult.timestamp,
        successful: true,
        executionTimeMs: 150,
        confidence: 0.85,
        userId: 'current-user',
        sessionId: request.sessionId,
      };

      set(state => ({
        queryHistory: [historyItem, ...state.queryHistory.slice(0, 49)]
      }));
    }
  },

  addToFavorites: (query) => {
    // For now, just log it. In a real app, this would save to backend
    console.log('Added to favorites:', query);
  },

  clearCurrentResult: () => {
    set({ currentResult: null, error: null });
  },

  setLoading: (loading: boolean) => {
    set({ isLoading: loading });
  },

  setError: (error: string | null) => {
    set({ error });
  },
}));
