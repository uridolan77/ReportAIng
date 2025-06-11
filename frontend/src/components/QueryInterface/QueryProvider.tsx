import React, { createContext, useContext, useState, useCallback, useRef, useEffect } from 'react';
import { useAuthStore } from '../../stores/authStore';
import { useActiveResult, useActiveResultActions } from '../../stores/activeResultStore';
import { useGlobalResult, useGlobalResultActions } from '../../stores/globalResultStore';
import { useWebSocket } from '../../hooks/useWebSocket';
import { useKeyboardNavigation } from '../../hooks/useKeyboardNavigation';
import { getOrCreateSessionId } from '../../utils/sessionUtils';
import {
  useExecuteQuery,
  useQueryHistory,
  useQuerySuggestions,
  useAddToFavorites,
  usePrefetchRelatedData
} from '../../hooks/useQueryApi';
// MockDataService removed - database connection always required
import type { QueryRequest } from '../../types/query';

interface QueryContextType {
  // State
  query: string;
  setQuery: (query: string) => void;
  isLoading: boolean;
  setIsLoading: (loading: boolean) => void;
  progress: number;
  setProgress: (progress: number) => void;
  activeTab: string;
  setActiveTab: (tab: string) => void;
  showExportModal: boolean;
  setShowExportModal: (show: boolean) => void;
  showWizard: boolean;
  setShowWizard: (show: boolean) => void;
  showTemplateLibrary: boolean;
  setShowTemplateLibrary: (show: boolean) => void;
  showInsightsPanel: boolean;
  setShowInsightsPanel: (show: boolean) => void;
  showCommandPalette: boolean;
  setShowCommandPalette: (show: boolean) => void;

  // Processing state
  processingStages: any[];
  currentProcessingStage: string;
  showProcessingDetails: boolean;
  setShowProcessingDetails: (show: boolean) => void;
  processingViewMode: 'minimal' | 'processing' | 'advanced' | 'hidden';
  setProcessingViewMode: (mode: 'minimal' | 'processing' | 'advanced' | 'hidden') => void;
  currentQueryId: string;

  // React Query data
  currentResult: any;
  queryHistory: any[];
  queryHistoryLoading: boolean;
  queryHistoryError: any;
  suggestions: string[];
  suggestionsLoading: boolean;
  user: any;
  isAdmin: boolean;

  // WebSocket
  isConnected: boolean;

  // Refs
  textAreaRef: React.RefObject<any>;

  // Handlers
  handleSubmitQuery: () => Promise<void>;
  handleSuggestionClick: (suggestion: string) => void;
  handleFollowUpSuggestionClick: (suggestion: string) => Promise<void>;
  handleAddToFavorites: () => void;
  handleWizardQueryGenerated: (generatedQuery: string, wizardData: any) => Promise<void>;
  handleTemplateApply: (generatedQuery: string, template: any) => Promise<void>;
  handleVisualizationRequest: (type: string, data: any[], columns: any[]) => void;
  handleKeyDown: (e: React.KeyboardEvent) => void;

  // React Query mutations
  executeQueryMutation: any;
  addToFavoritesMutation: any;
}

const QueryContext = createContext<QueryContextType | undefined>(undefined);

export const useQueryContext = () => {
  const context = useContext(QueryContext);
  if (!context) {
    throw new Error('useQueryContext must be used within a QueryProvider');
  }
  return context;
};

interface QueryProviderProps {
  children: React.ReactNode;
}

export const QueryProvider: React.FC<QueryProviderProps> = ({ children }) => {
  // State management
  const [query, setQuery] = useState('');
  const [progress, setProgress] = useState(0);
  const [activeTab, setActiveTab] = useState('result');
  const [showExportModal, setShowExportModal] = useState(false);
  const [showWizard, setShowWizard] = useState(false);
  const [showTemplateLibrary, setShowTemplateLibrary] = useState(false);
  const [showInsightsPanel, setShowInsightsPanel] = useState(false);
  const [showCommandPalette, setShowCommandPalette] = useState(false);

  // Active result management
  const { result: currentResult } = useActiveResult();
  const { setActiveResult, clearActiveResult } = useActiveResultActions();

  // Global result management for cross-page availability
  const { currentResult: globalCurrentResult } = useGlobalResult();
  const { setCurrentResult: setGlobalResult, setActivePageId } = useGlobalResultActions();

  // Unified result setter that syncs both stores
  const setUnifiedResult = useCallback((result: any, query: string, source = 'query', pageContext = 'main') => {
    // Set in active result store (for current session)
    setActiveResult(result, query);

    // Set in global result store (for cross-page availability)
    setGlobalResult(result, query, source, {
      pageSource: pageContext,
      timestamp: Date.now(),
      sessionId: result.sessionId || getOrCreateSessionId()
    });

    if (process.env.NODE_ENV === 'development') {
      console.log('🔄 Unified result set:', {
        hasResult: !!result,
        success: result?.success,
        query: query.substring(0, 50) + '...',
        source,
        pageContext,
        dataLength: result?.result?.data?.length
      });
    }
  }, [setActiveResult, setGlobalResult]);

  // React Query hooks
  const executeQueryMutation = useExecuteQuery();
  const addToFavoritesMutation = useAddToFavorites();
  const {
    data: queryHistoryData = [],
    isLoading: queryHistoryLoading,
    error: queryHistoryError
  } = useQueryHistory(1, 20);

  // Ensure queryHistory is always an array
  const queryHistory = Array.isArray(queryHistoryData) ? queryHistoryData : [];
  const {
    data: suggestions = [],
    isLoading: suggestionsLoading
  } = useQuerySuggestions(query.length > 2 ? query : undefined, query.length > 2);
  const { prefetchHistory, prefetchSuggestions, prefetchSchema } = usePrefetchRelatedData();

  const { user } = useAuthStore();

  // Check if user is admin
  const isAdmin = user?.roles?.includes('Admin') || user?.roles?.includes('admin') || false;

  // WebSocket for real-time updates
  const { isConnected, lastMessage } = useWebSocket('/ws/query-status');

  // Query processing state
  const [processingStages, setProcessingStages] = useState<any[]>([]);
  const [currentProcessingStage, setCurrentProcessingStage] = useState<string>('');
  const [showProcessingDetails, setShowProcessingDetails] = useState(false);
  const [processingViewMode, setProcessingViewMode] = useState<'minimal' | 'processing' | 'advanced' | 'hidden'>('hidden');
  const [currentQueryId, setCurrentQueryId] = useState<string>('');

  // Refs
  const textAreaRef = useRef<any>(null);

  // Handle WebSocket messages for query progress
  useEffect(() => {
    if (lastMessage) {
      console.log('📨 Received WebSocket message:', lastMessage);

      try {
        // Handle different message types - check for both old and new event names
        if (lastMessage.type === 'QueryProcessingProgress' ||
            lastMessage.type === 'QueryProgress' ||
            lastMessage.type === 'DetailedProgress') {
          // Parse the data which is already JSON stringified by the WebSocket hook
          const message = JSON.parse(lastMessage.data);
          console.log('🔄 Query processing progress update:', {
            type: lastMessage.type,
            message,
            messageKeys: Object.keys(message),
            hasDetails: !!(message.Details || message.details)
          });

          // Update current query ID and stage - handle both uppercase and lowercase field names
          const queryId = message.QueryId || message.queryId;
          const stage = message.Stage || message.stage;
          const messageText = message.Message || message.message;
          const progress = message.Progress || message.progress;
          const timestamp = message.Timestamp || message.timestamp;
          const details = message.Details || message.details;

          // Debug log the WebSocket message details
          console.log('🔍 WebSocket message received:', {
            stage,
            messageText,
            progress,
            timestamp,
            details: details ? Object.keys(details) : 'no details',
            fullDetails: details,
            rawMessage: message
          });

          if (queryId) {
            setCurrentQueryId(queryId);
          }
          if (stage) {
            setCurrentProcessingStage(stage);
          }

          // Add or update processing stage
          setProcessingStages(prev => {
            const existingIndex = prev.findIndex(stageData => stageData.stage === stage);

            // Determine status based on stage type and progress
            let status = 'active';
            if (stage === 'sql_error' || stage === 'validation_failed' || stage === 'error') {
              status = 'error';
            } else if (progress === 100) {
              status = 'completed';
            }

            const newStage = {
              stage: stage,
              message: messageText,
              progress: progress,
              timestamp: timestamp,
              details: details,
              status: status
            };

            if (existingIndex >= 0) {
              // Update existing stage
              const updated = [...prev];
              updated[existingIndex] = newStage;
              return updated;
            } else {
              // Add new stage
              return [...prev, newStage];
            }
          });

          // Auto-show processing details when query starts
          if (stage === 'started') {
            setShowProcessingDetails(true);
            setProcessingStages([]); // Clear previous stages
            console.log('🚀 Query processing started - showing processing details');
          }

          // Handle validation failures
          if (stage === 'validation_failed') {
            console.error('❌ SQL Validation Failed:', messageText);
            console.error('❌ Failed SQL:', details?.sql);
            console.error('❌ Error Details:', details);

            // Keep processing details visible to show the error
            setShowProcessingDetails(true);
            // Keep processing view mode visible for errors
            setProcessingViewMode('processing');

            // Create an error result to display in the UI
            const errorResult = {
              success: false,
              error: details?.error || messageText || 'SQL validation failed',
              sql: details?.sql,
              queryId: currentQueryId,
              timestamp: new Date().toISOString(),
              validationError: true,
              confidence: 0,
              suggestions: [],
              cached: false,
              executionTimeMs: 0
            };

            // Set this as the current result so it shows in the UI
            setUnifiedResult(errorResult, query, 'query', 'main');
            setActiveTab('result');
          }

          // Handle SQL execution errors
          if (stage === 'sql_error') {
            console.error('❌ SQL Execution Failed:', messageText);
            console.error('❌ Failed SQL:', details?.sql);
            console.error('❌ Error Details:', details);

            // Keep processing details visible to show the error
            setShowProcessingDetails(true);
            // Keep processing view mode visible for errors
            setProcessingViewMode('processing');

            // Create an error result to display in the UI
            const errorResult = {
              success: false,
              error: details?.error || messageText || 'SQL execution failed',
              sql: details?.sql,
              queryId: currentQueryId,
              timestamp: new Date().toISOString(),
              executionError: true,
              confidence: 0,
              suggestions: [],
              cached: false,
              executionTimeMs: 0
            };

            // Set this as the current result so it shows in the UI
            setUnifiedResult(errorResult, query, 'query', 'main');
            setActiveTab('result');
          }

          // Keep processing details visible during processing
          if (progress < 100) {
            setShowProcessingDetails(true);
          }

          // Update overall progress with smooth transition and interpolation
          const newProgress = progress || 0;
          setProgress(prev => {
            // Ensure progress never goes backwards (except on reset)
            if (newProgress === 0) return 0;

            // Smooth progress interpolation for better UX
            const currentProgress = Math.max(prev, newProgress);

            // Add small incremental progress for stages that might take longer
            if (stage === 'ai_processing' && currentProgress < 60) {
              return Math.min(currentProgress + 2, newProgress);
            }

            return currentProgress;
          });
        } else {
          // Handle other message types
          const message = JSON.parse(lastMessage.data);

          switch (lastMessage.type) {
            case 'query_progress':
              setProgress(message.progress * 100);
              break;
            case 'query_completed':
            case 'QueryCompleted':
              // setIsLoading handled by React Query mutation state
              setProgress(0);
              // Don't clear processing details immediately - let them be available for the hidden panel
              // setShowProcessingDetails(false);
              console.log('🔍 Query completed via WebSocket - marking all stages as complete');

              // Mark all processing stages as completed
              setProcessingStages(prev => {
                console.log('🔍 WebSocket completion - Processing stages before update:', prev.length, prev.map(s => `${s.stage}:${s.progress}%`));

                // Mark all existing stages as 100% complete
                const updatedStages = prev.map(stage => ({
                  ...stage,
                  progress: 100
                }));

                // Add completion stage if it doesn't exist
                const hasCompletionStage = updatedStages.some(s => s.stage === 'completed');
                if (!hasCompletionStage) {
                  const completionStage = {
                    stage: 'completed',
                    message: 'Query completed successfully',
                    progress: 100,
                    timestamp: new Date().toISOString(),
                    details: {
                      success: true,
                      completedViaWebSocket: true
                    }
                  };
                  updatedStages.push(completionStage);
                }

                console.log('🔍 WebSocket completion - Updated stages:', updatedStages.length, updatedStages.map(s => `${s.stage}:${s.progress}%`));
                return updatedStages;
              });

              setCurrentProcessingStage('completed');

              // Set processing view mode to hidden after completion to show the collapsed panel
              setProcessingViewMode('hidden');
              break;
            case 'query_error':
              // setIsLoading handled by React Query mutation state
              setProgress(0);
              setShowProcessingDetails(false);
              break;
            default:
              console.log('📨 Unhandled message type:', lastMessage.type, message);
          }
        }
      } catch (error) {
        console.error('Error parsing WebSocket message:', error, lastMessage);
      }
    }
  }, [lastMessage]);

  // Create query request helper
  const createQueryRequest = useCallback((
    question: string,
    llmOptions?: { providerId?: string; modelId?: string }
  ): QueryRequest => ({
    question: question.trim(),
    sessionId: getOrCreateSessionId(),
    options: {
      includeVisualization: true,
      maxRows: 1000,
      enableCache: true,
      confidenceThreshold: 0.7,
      providerId: llmOptions?.providerId,
      modelId: llmOptions?.modelId
    }
  }), []);

  // Handle query submission
  const handleSubmitQuery = useCallback(async (llmOptions?: { providerId?: string; modelId?: string }) => {
    if (!query.trim() || executeQueryMutation.isPending) return;

    // Always use real database connection - no mock data

    // Create query request first
    const queryRequest = createQueryRequest(query, llmOptions);

    // Initialize processing state with immediate feedback
    setProgress(0);
    setShowProcessingDetails(true);
    setProcessingStages([]);
    setCurrentProcessingStage('initializing');
    clearActiveResult(); // Clear previous result

    // Provide immediate visual feedback with smooth progress
    const startTime = Date.now();

    // Add initial stage immediately
    setProcessingStages([{
      stage: 'initializing',
      message: 'Preparing your query...',
      progress: 5,
      timestamp: new Date().toISOString(),
      details: { startTime },
      status: 'active'
    }]);
    setProgress(5);

    // Quick progress updates for better UX
    setTimeout(() => {
      setProcessingStages(prev => [...prev, {
        stage: 'connecting',
        message: 'Connecting to AI service...',
        progress: 10,
        timestamp: new Date().toISOString(),
        details: {},
        status: 'active'
      }]);
      setProgress(10);
    }, 100);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      console.log('🔍 Query execution completed - Full result:', result);
      console.log('🔍 Result structure:', {
        success: result?.success,
        hasResult: !!result?.result,
        hasData: !!result?.result?.data,
        dataLength: result?.result?.data?.length,
        hasMetadata: !!result?.result?.metadata,
        error: result?.error,
        sql: result?.sql,
        hasPromptDetails: !!(result?.promptDetails),
        promptDetailsKeys: result?.promptDetails ? Object.keys(result.promptDetails) : 'None',
        allKeys: result ? Object.keys(result) : 'N/A'
      });

      // Mark all stages as completed and add a completion stage
      setProcessingStages(prev => {
        console.log('🔍 Processing stages before completion update:', prev.length, prev.map(s => `${s.stage}:${s.progress}%`));

        // If no stages exist, create some default ones
        if (prev.length === 0) {
          console.log('🔍 No processing stages found, creating default stages');
          const defaultStages = [
            {
              stage: 'started',
              message: 'Query processing started',
              progress: 100,
              timestamp: new Date().toISOString(),
              details: {
                operation: 'Query initialization',
                queryText: query,
                sessionId: queryRequest.sessionId,
                startTime: new Date().toISOString()
              }
            },
            {
              stage: 'ai_processing',
              message: 'AI analyzing query',
              progress: 100,
              timestamp: new Date().toISOString(),
              details: {
                operation: 'AI query analysis',
                model: 'GPT-4 Turbo',
                queryText: query,
                analysisType: 'Natural language to SQL conversion',
                ...(result?.promptDetails && { promptDetails: result.promptDetails })
              }
            },
            {
              stage: 'sql_execution',
              message: 'Executing SQL query',
              progress: 100,
              timestamp: new Date().toISOString(),
              details: {
                operation: 'Database query execution',
                database: 'Production database',
                queryText: query,
                executionMode: 'Standard'
              }
            }
          ];
          prev = defaultStages;
        }

        // Mark all existing stages as 100% complete
        const updatedStages = prev.map(stage => ({
          ...stage,
          progress: 100
        }));

        const completionStage = {
          stage: 'completed',
          message: result?.success ? 'Query completed successfully' : 'Query completed with errors',
          progress: 100,
          timestamp: new Date().toISOString(),
          details: {
            success: result?.success,
            executionTime: result?.executionTimeMs,
            rowCount: result?.result?.data?.length || 0,
            queryText: query,
            sql: result?.sql || 'SQL not available',
            hasVisualization: !!(result as any)?.result?.visualization,
            dataColumns: result?.result?.metadata?.columns?.length || 0,
            cacheUpdated: true,
            finalStatus: result?.success ? 'Success' : 'Error',
            ...(result?.error && { error: result.error }),
            ...(result?.promptDetails && { promptDetails: result.promptDetails }),
            ...(result?.result?.metadata && {
              metadata: {
                totalColumns: result.result.metadata.columns?.length || 0,
                columnNames: result.result.metadata.columns?.map(c => c.name) || [],
                dataTypes: result.result.metadata.columns?.map(c => c.dataType) || []
              }
            })
          }
        };

        // Check if completion stage already exists
        const existingIndex = updatedStages.findIndex(s => s.stage === 'completed');
        if (existingIndex >= 0) {
          updatedStages[existingIndex] = completionStage;
          console.log('🔍 Updated existing completion stage, total stages:', updatedStages.length);
          return updatedStages;
        } else {
          const finalStages = [...updatedStages, completionStage];
          console.log('🔍 Added new completion stage, total stages:', finalStages.length);
          return finalStages;
        }
      });

      setCurrentProcessingStage('completed');
      setUnifiedResult(result, query, 'query', 'main'); // Save to both stores
      setActiveTab('result');

      // Set processing view mode to hidden after completion to show the collapsed panel
      setProcessingViewMode('hidden');
    } catch (error) {
      console.error('Query execution failed:', error);
      console.error('Error details:', {
        message: error instanceof Error ? error.message : 'Unknown error',
        response: (error as any)?.response?.data,
        status: (error as any)?.response?.status
      });
    } finally {
      setProgress(0);
      // Don't automatically hide processing details - let the UI manage this
      // The processing stages will be available for the hidden panel
      console.log('🔍 Query execution finished - processing stages preserved');
    }
  }, [query, executeQueryMutation, createQueryRequest, setActiveResult, clearActiveResult]);

  // Handle keyboard shortcuts
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSubmitQuery();
    }
  }, [handleSubmitQuery]);

  // Handle query suggestions
  const handleSuggestionClick = useCallback((suggestion: string) => {
    setQuery(suggestion);
    textAreaRef.current?.focus();
  }, []);

  // Handle follow-up suggestion clicks (execute immediately)
  const handleFollowUpSuggestionClick = useCallback(async (suggestion: string) => {
    setQuery(suggestion);
    setProgress(10);
    clearActiveResult(); // Clear previous result

    // Always use real database connection - no mock data

    const queryRequest = createQueryRequest(suggestion);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      setActiveResult(result, suggestion); // Save to active result store
      setActiveTab('result');
    } catch (error) {
      console.error('Follow-up query execution failed:', error);
    } finally {
      setProgress(0);
    }
  }, [executeQueryMutation, createQueryRequest, setActiveResult, clearActiveResult]);

  // Handle adding to favorites
  const handleAddToFavorites = useCallback(() => {
    if (currentResult && query) {
      addToFavoritesMutation.mutate({
        query: query.trim(),
        timestamp: new Date().toISOString(),
        description: `Query: ${query.substring(0, 50)}...`
      });
    }
  }, [currentResult, query, addToFavoritesMutation]);

  // Handle wizard query generation
  const handleWizardQueryGenerated = useCallback(async (generatedQuery: string, wizardData: any) => {
    setQuery(generatedQuery);
    setShowWizard(false);
    setProgress(10);
    clearActiveResult(); // Clear previous result

    // Always use real database connection - no mock data

    const queryRequest = createQueryRequest(generatedQuery);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      setUnifiedResult(result, generatedQuery, 'query', 'wizard'); // Save to both stores
      setActiveTab('result');
    } catch (error) {
      console.error('Wizard query execution failed:', error);
    } finally {
      setProgress(0);
    }
  }, [executeQueryMutation, createQueryRequest, setActiveResult, clearActiveResult]);

  // Handle template application
  const handleTemplateApply = useCallback(async (generatedQuery: string, template: any) => {
    setQuery(generatedQuery);
    setShowTemplateLibrary(false);
    setProgress(10);
    clearActiveResult(); // Clear previous result

    // Always use real database connection - no mock data

    const queryRequest = createQueryRequest(generatedQuery);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      setUnifiedResult(result, generatedQuery, 'query', 'template'); // Save to both stores
      setActiveTab('result');
    } catch (error) {
      console.error('Template query execution failed:', error);
    } finally {
      setProgress(0);
    }
  }, [executeQueryMutation, createQueryRequest, setActiveResult, clearActiveResult]);

  // Handle visualization request
  const handleVisualizationRequest = useCallback((type: string, data: any[], columns: any[]) => {
    console.log('📊 Visualization requested:', { type, dataRows: data.length, columns: columns.length });

    // Store visualization data in the active result store for persistence
    const visualizationData = {
      type,
      data,
      columns,
      query,
      timestamp: new Date().toISOString()
    };

    console.log('Visualization data prepared:', visualizationData);

    // All chart types will be handled inline in QueryResult
    // No need to open new windows for any chart type
    console.log('Visualization request handled inline for type:', type);
  }, [query]);

  // Keyboard navigation (setup only, variable not used)
  useKeyboardNavigation({
    onExecuteQuery: handleSubmitQuery,
    onSaveQuery: () => handleAddToFavorites(),
    onFocusQueryInput: () => textAreaRef.current?.focus(),
    onClearQuery: () => setQuery(''),
    onNewQuery: () => {
      setQuery('');
      clearActiveResult();
      setActiveTab('result');
    },
    onOpenTemplates: () => setShowTemplateLibrary(true),
    onToggleInsights: () => setShowInsightsPanel(prev => !prev),
    onToggleHistory: () => setActiveTab('history'),
    onExportResults: () => setShowExportModal(true),
    onOpenCommandPalette: () => setShowCommandPalette(true),
    onToggleHelp: () => {
      // Show help modal or navigate to help
      console.log('Help requested');
    }
  });

  // Handle command palette actions
  useEffect(() => {
    const handleCommandAction = (event: CustomEvent) => {
      const { action } = event.detail;

      switch (action) {
        case 'execute-query':
          handleSubmitQuery();
          break;
        case 'new-query':
          setQuery('');
          clearActiveResult();
          setActiveTab('result');
          break;
        case 'save-query':
          handleAddToFavorites();
          break;
        case 'toggle-history':
          setActiveTab('history');
          break;
        case 'open-templates':
          setShowTemplateLibrary(true);
          break;
        case 'export-results':
          setShowExportModal(true);
          break;
        case 'toggle-insights':
          setShowInsightsPanel(prev => !prev);
          break;
        case 'open-cache-manager':
          setActiveTab('cache');
          break;
        case 'open-security':
          setActiveTab('security');
          break;
        case 'open-visualizations':
          setActiveTab('advanced');
          break;
        default:
          console.log('Unknown command action:', action);
      }
    };

    window.addEventListener('command-palette-action' as any, handleCommandAction);
    return () => window.removeEventListener('command-palette-action' as any, handleCommandAction);
  }, [handleSubmitQuery, handleAddToFavorites]);

  // Prefetch related data on mount with delay to allow authentication to settle
  useEffect(() => {
    const prefetchTimeout = setTimeout(() => {
      prefetchHistory();
      prefetchSuggestions();
      prefetchSchema();
    }, 2000); // 2 second delay

    return () => clearTimeout(prefetchTimeout);
  }, [prefetchHistory, prefetchSuggestions, prefetchSchema]);

  const contextValue: QueryContextType = {
    // State
    query,
    setQuery,
    isLoading: executeQueryMutation.isPending,
    setIsLoading: () => {}, // Not needed with React Query
    progress,
    setProgress,
    activeTab,
    setActiveTab,
    showExportModal,
    setShowExportModal,
    showWizard,
    setShowWizard,
    showTemplateLibrary,
    setShowTemplateLibrary,
    showInsightsPanel,
    setShowInsightsPanel,
    showCommandPalette,
    setShowCommandPalette,

    // Processing state
    processingStages,
    currentProcessingStage,
    showProcessingDetails,
    setShowProcessingDetails,
    processingViewMode,
    setProcessingViewMode,
    currentQueryId,

    // React Query data
    currentResult,
    queryHistory,
    queryHistoryLoading,
    queryHistoryError,
    suggestions,
    suggestionsLoading,
    user,
    isAdmin,

    // WebSocket
    isConnected,

    // Refs
    textAreaRef,

    // Handlers
    handleSubmitQuery,
    handleSuggestionClick,
    handleFollowUpSuggestionClick,
    handleAddToFavorites,
    handleWizardQueryGenerated,
    handleTemplateApply,
    handleVisualizationRequest,
    handleKeyDown,

    // React Query mutations
    executeQueryMutation,
    addToFavoritesMutation,
  };

  return (
    <QueryContext.Provider value={contextValue}>
      {children}
    </QueryContext.Provider>
  );
};
