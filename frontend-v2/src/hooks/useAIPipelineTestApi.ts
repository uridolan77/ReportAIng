import { useState, useCallback } from 'react';
import { message } from 'antd';
import { useAppSelector } from '@shared/hooks';
import { selectAccessToken } from '@shared/store/auth';
import {
  PipelineTestRequest,
  PipelineTestResult,
  PipelineStepInfo,
  PipelineTestSession,
  PipelineTestAnalytics,
  PipelineTestConfiguration,
  PipelineTestTemplate,
  SaveConfigurationRequest,
  ParameterValidationRequest,
  ParameterValidationResult,
  PipelineStep,
  PipelineTestParameters,
  normalizePipelineSteps
} from '../types/aiPipelineTest';

const API_BASE_URL = '/api'; // Use Vite proxy instead of direct backend URL

export const useAIPipelineTestApi = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const accessToken = useAppSelector(selectAccessToken);

  const getAuthHeaders = () => {
    return {
      'Content-Type': 'application/json',
      'Authorization': accessToken ? `Bearer ${accessToken}` : '',
    };
  };

  const handleApiCall = async <T>(
    apiCall: () => Promise<Response>,
    successMessage?: string
  ): Promise<T> => {
    setLoading(true);
    setError(null);

    try {
      const response = await apiCall();
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Unknown error' }));
        throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      
      if (successMessage) {
        message.success(successMessage);
      }

      return data;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(errorMessage);
      message.error(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Get available pipeline steps and their configurations
  const getAvailableSteps = useCallback(async (): Promise<PipelineStepInfo[]> => {
    console.log('🔄 API: Calling getAvailableSteps...');
    console.log('🔄 API: URL:', `${API_BASE_URL}/aipipelinetest/steps`);
    console.log('🔄 API: Headers:', getAuthHeaders());

    try {
      const result = await handleApiCall<PipelineStepInfo[]>(
        () => fetch(`${API_BASE_URL}/aipipelinetest/steps`, {
          method: 'GET',
          headers: getAuthHeaders()
        })
      );
      console.log('✅ API: Received result:', result);

      // Normalize enum values in case backend returns numeric values
      const normalizedResult = result.map(stepInfo => ({
        ...stepInfo,
        step: normalizePipelineSteps([stepInfo.step as any])[0]
      }));

      console.log('✅ API: Normalized result:', normalizedResult);
      return normalizedResult;
    } catch (error) {
      console.error('❌ API: getAvailableSteps failed:', error);
      throw error;
    }
  }, []);

  // Test pipeline steps
  const testPipelineSteps = useCallback(async (request: PipelineTestRequest): Promise<PipelineTestResult> => {
    const result = await handleApiCall<PipelineTestResult>(
      () => fetch(`${API_BASE_URL}/aipipelinetest/test-steps`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(request),
      }),
      'Pipeline test completed successfully'
    );

    // Normalize enum values in case backend returns numeric values
    if (result.requestedSteps) {
      result.requestedSteps = normalizePipelineSteps(result.requestedSteps as any[]);
    }

    return result;
  }, []);

  // Get test session details
  const getTestSession = useCallback(async (sessionId: string): Promise<PipelineTestSession> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/sessions/${sessionId}`, {
        method: 'GET',
        headers: getAuthHeaders(),
      })
    );
  }, []);

  // Get test analytics
  const getTestAnalytics = useCallback(async (
    startDate?: string,
    endDate?: string
  ): Promise<PipelineTestAnalytics> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/analytics?${params}`, {
        method: 'GET',
        headers: getAuthHeaders(),
      })
    );
  }, []);

  // Save test configuration
  const saveTestConfiguration = useCallback(async (config: SaveConfigurationRequest): Promise<PipelineTestConfiguration> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/configurations`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(config),
      }),
      'Test configuration saved successfully'
    );
  }, []);

  // Get saved test configurations
  const getTestConfigurations = useCallback(async (): Promise<PipelineTestConfiguration[]> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/configurations`, {
        method: 'GET',
        headers: getAuthHeaders(),
      })
    );
  }, []);

  // Delete test configuration
  const deleteTestConfiguration = useCallback(async (configId: string): Promise<void> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/configurations/${configId}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      }),
      'Test configuration deleted successfully'
    );
  }, []);

  // Get test templates
  const getTestTemplates = useCallback(async (): Promise<PipelineTestTemplate[]> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/templates`, {
        method: 'GET',
        headers: getAuthHeaders(),
      })
    );
  }, []);

  // Create test from template
  const createTestFromTemplate = useCallback(async (
    templateId: string,
    query: string,
    parameterOverrides?: Record<string, any>
  ): Promise<PipelineTestRequest> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/templates/${templateId}/create-test`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ query, parameterOverrides }),
      })
    );
  }, []);

  // Batch test multiple queries
  const batchTestQueries = useCallback(async (
    queries: string[],
    steps: string[],
    parameters: Record<string, any>
  ): Promise<PipelineTestResult[]> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/batch-test`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ queries, steps, parameters }),
      }),
      `Batch test completed for ${queries.length} queries`
    );
  }, []);

  // Export test results
  const exportTestResults = useCallback(async (
    testIds: string[],
    format: 'json' | 'csv' | 'excel' = 'json'
  ): Promise<Blob> => {
    const response = await fetch(`${API_BASE_URL}/aipipelinetest/export`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ testIds, format }),
    });

    if (!response.ok) {
      throw new Error(`Export failed: ${response.statusText}`);
    }

    return response.blob();
  }, []);

  // Real-time test monitoring (WebSocket connection)
  const subscribeToTestUpdates = useCallback((
    sessionId: string,
    onUpdate: (session: PipelineTestSession) => void,
    onError?: (error: string) => void
  ) => {
    const wsUrl = `${API_BASE_URL.replace('http', 'ws')}/ws/pipeline-test/${sessionId}?token=${accessToken}`;

    const ws = new WebSocket(wsUrl);
    
    ws.onmessage = (event) => {
      try {
        const session = JSON.parse(event.data) as PipelineTestSession;
        onUpdate(session);
      } catch (err) {
        console.error('Failed to parse WebSocket message:', err);
        onError?.('Failed to parse update message');
      }
    };

    ws.onerror = (event) => {
      console.error('WebSocket error:', event);
      onError?.('WebSocket connection error');
    };

    ws.onclose = (event) => {
      if (event.code !== 1000) {
        console.warn('WebSocket closed unexpectedly:', event.code, event.reason);
        onError?.(`Connection closed: ${event.reason || 'Unknown reason'}`);
      }
    };

    return () => {
      ws.close();
    };
  }, [accessToken]);

  // Validate test configuration
  const validateTestConfiguration = useCallback(async (
    steps: PipelineStep[],
    parameters: PipelineTestParameters
  ): Promise<ParameterValidationResult> => {
    return handleApiCall(
      () => fetch(`${API_BASE_URL}/aipipelinetest/validate`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ steps, parameters }),
      })
    );
  }, []);

  return {
    // State
    loading,
    error,

    // Core testing functions
    getAvailableSteps,
    testPipelineSteps,
    getTestSession,

    // Analytics and monitoring
    getTestAnalytics,
    subscribeToTestUpdates,

    // Configuration management
    saveTestConfiguration,
    getTestConfigurations,
    deleteTestConfiguration,

    // Templates
    getTestTemplates,
    createTestFromTemplate,

    // Batch operations
    batchTestQueries,
    exportTestResults,

    // Validation
    validateTestConfiguration,

    // Utility
    clearError: () => setError(null),
  };
};
