import { 
  AdvancedStreamingRequest, 
  StreamingQueryChunk, 
  StreamingProgressUpdate 
} from '../types/query';
import { API_CONFIG, getApiUrl, getAuthHeaders } from '../config/api';
import { useAuthStore } from '../stores/authStore';

export class StreamingQueryService {
  private static instance: StreamingQueryService;
  
  public static getInstance(): StreamingQueryService {
    if (!StreamingQueryService.instance) {
      StreamingQueryService.instance = new StreamingQueryService();
    }
    return StreamingQueryService.instance;
  }

  /**
   * Execute a streaming query with basic chunked results
   */
  async* executeStreamingQuery(request: AdvancedStreamingRequest): AsyncGenerator<StreamingQueryChunk> {
    const authState = useAuthStore.getState();
    const token = authState.token;

    try {
      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.STREAMING.BASIC), {
        method: 'POST',
        headers: getAuthHeaders(token || undefined),
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      if (!response.body) {
        throw new Error('Response body is null');
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      try {
        while (true) {
          const { done, value } = await reader.read();
          
          if (done) break;
          
          buffer += decoder.decode(value, { stream: true });
          
          // Process complete JSON objects from the buffer
          let newlineIndex;
          while ((newlineIndex = buffer.indexOf('\n')) !== -1) {
            const line = buffer.slice(0, newlineIndex).trim();
            buffer = buffer.slice(newlineIndex + 1);
            
            if (line) {
              try {
                const chunk: StreamingQueryChunk = JSON.parse(line);
                yield chunk;
                
                if (chunk.isLastChunk) {
                  return;
                }
              } catch (parseError) {
                console.error('Error parsing streaming chunk:', parseError);
              }
            }
          }
        }
      } finally {
        reader.releaseLock();
      }
    } catch (error) {
      console.error('Streaming query error:', error);
      
      // Yield an error chunk
      yield {
        chunkIndex: -1,
        data: [],
        totalRowsInChunk: 0,
        isLastChunk: true,
        timestamp: new Date().toISOString(),
        processingTimeMs: 0,
      };
    }
  }

  /**
   * Execute a streaming query with backpressure control
   */
  async* executeStreamingQueryWithBackpressure(request: AdvancedStreamingRequest): AsyncGenerator<StreamingQueryChunk> {
    const authState = useAuthStore.getState();
    const token = authState.token;

    try {
      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.STREAMING.BACKPRESSURE), {
        method: 'POST',
        headers: getAuthHeaders(token || undefined),
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      if (!response.body) {
        throw new Error('Response body is null');
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      try {
        while (true) {
          const { done, value } = await reader.read();
          
          if (done) break;
          
          buffer += decoder.decode(value, { stream: true });
          
          // Process complete JSON objects from the buffer
          let newlineIndex;
          while ((newlineIndex = buffer.indexOf('\n')) !== -1) {
            const line = buffer.slice(0, newlineIndex).trim();
            buffer = buffer.slice(newlineIndex + 1);
            
            if (line) {
              try {
                const chunk: StreamingQueryChunk = JSON.parse(line);
                yield chunk;
                
                if (chunk.isLastChunk) {
                  return;
                }
              } catch (parseError) {
                console.error('Error parsing streaming chunk:', parseError);
              }
            }
          }
        }
      } finally {
        reader.releaseLock();
      }
    } catch (error) {
      console.error('Streaming query with backpressure error:', error);
      
      // Yield an error chunk
      yield {
        chunkIndex: -1,
        data: [],
        totalRowsInChunk: 0,
        isLastChunk: true,
        timestamp: new Date().toISOString(),
        processingTimeMs: 0,
      };
    }
  }

  /**
   * Execute a streaming query with progress reporting
   */
  async* executeStreamingQueryWithProgress(request: AdvancedStreamingRequest): AsyncGenerator<StreamingProgressUpdate> {
    const authState = useAuthStore.getState();
    const token = authState.token;

    try {
      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.STREAMING.PROGRESS), {
        method: 'POST',
        headers: getAuthHeaders(token || undefined),
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      if (!response.body) {
        throw new Error('Response body is null');
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      try {
        while (true) {
          const { done, value } = await reader.read();
          
          if (done) break;
          
          buffer += decoder.decode(value, { stream: true });
          
          // Process complete JSON objects from the buffer
          let newlineIndex;
          while ((newlineIndex = buffer.indexOf('\n')) !== -1) {
            const line = buffer.slice(0, newlineIndex).trim();
            buffer = buffer.slice(newlineIndex + 1);
            
            if (line) {
              try {
                const progress: StreamingProgressUpdate = JSON.parse(line);
                yield progress;
                
                if (progress.isCompleted) {
                  return;
                }
              } catch (parseError) {
                console.error('Error parsing progress update:', parseError);
              }
            }
          }
        }
      } finally {
        reader.releaseLock();
      }
    } catch (error) {
      console.error('Streaming query with progress error:', error);
      
      // Yield an error progress update
      yield {
        rowsProcessed: 0,
        estimatedTotalRows: 0,
        progressPercentage: 0,
        elapsedTime: '00:00:00',
        estimatedTimeRemaining: '00:00:00',
        rowsPerSecond: 0,
        status: 'Error',
        isCompleted: true,
        errorMessage: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  /**
   * Cancel a streaming query (if supported by the backend)
   */
  async cancelStreamingQuery(queryId: string): Promise<boolean> {
    const authState = useAuthStore.getState();
    const token = authState.token;

    try {
      const response = await fetch(getApiUrl(`/api/streaming/cancel/${queryId}`), {
        method: 'POST',
        headers: getAuthHeaders(token || undefined),
      });

      return response.ok;
    } catch (error) {
      console.error('Error cancelling streaming query:', error);
      return false;
    }
  }
}

// Export singleton instance
export const streamingQueryService = StreamingQueryService.getInstance();
