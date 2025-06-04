import React, { createContext, useContext, useState, useCallback, useRef, useEffect } from 'react';
import { useAuthStore } from '../../stores/authStore';
import { useActiveResult, useActiveResultActions } from '../../stores/activeResultStore';
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
  const [showInsightsPanel, setShowInsightsPanel] = useState(true);
  const [showCommandPalette, setShowCommandPalette] = useState(false);

  // Active result management
  const { result: currentResult } = useActiveResult();
  const { setActiveResult, clearActiveResult } = useActiveResultActions();

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
  const [currentQueryId, setCurrentQueryId] = useState<string>('');

  // Refs
  const textAreaRef = useRef<any>(null);

  // Handle WebSocket messages for query progress
  useEffect(() => {
    if (lastMessage) {
      console.log('📨 Received WebSocket message:', lastMessage);

      try {
        const message = JSON.parse(lastMessage.data);

        // Handle different message types
        if (lastMessage.type === 'QueryProcessingProgress') {
          console.log('🔄 Query processing progress update:', message);

          // Update current query ID and stage
          if (message.QueryId) {
            setCurrentQueryId(message.QueryId);
          }
          if (message.Stage) {
            setCurrentProcessingStage(message.Stage);
          }

          // Add or update processing stage
          setProcessingStages(prev => {
            const existingIndex = prev.findIndex(stage => stage.stage === message.Stage);
            const newStage = {
              stage: message.Stage,
              message: message.Message,
              progress: message.Progress,
              timestamp: message.Timestamp,
              details: message.Details,
              status: message.Progress === 100 ? 'completed' : 'active'
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
          if (message.Stage === 'started') {
            setShowProcessingDetails(true);
            setProcessingStages([]); // Clear previous stages
          }

          // Update overall progress
          setProgress(message.Progress || 0);
        } else {
          // Handle legacy message types
          switch (message.type) {
            case 'query_progress':
              setProgress(message.progress * 100);
              break;
            case 'query_completed':
              // setIsLoading handled by React Query mutation state
              setProgress(0);
              setShowProcessingDetails(false);
              break;
            case 'query_error':
              // setIsLoading handled by React Query mutation state
              setProgress(0);
              setShowProcessingDetails(false);
              break;
          }
        }
      } catch (error) {
        console.error('Error parsing WebSocket message:', error);
      }
    }
  }, [lastMessage]);

  // Create query request helper
  const createQueryRequest = useCallback((question: string): QueryRequest => ({
    question: question.trim(),
    sessionId: getOrCreateSessionId(),
    options: {
      includeVisualization: true,
      maxRows: 1000,
      enableCache: true,
      confidenceThreshold: 0.7
    }
  }), []);

  // Handle query submission
  const handleSubmitQuery = useCallback(async () => {
    if (!query.trim() || executeQueryMutation.isPending) return;

    setProgress(10);
    clearActiveResult(); // Clear previous result

    const queryRequest = createQueryRequest(query);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      setActiveResult(result, query); // Save to active result store
      setActiveTab('result');
    } catch (error) {
      console.error('Query execution failed:', error);
    } finally {
      setProgress(0);
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

    const queryRequest = createQueryRequest(generatedQuery);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      setActiveResult(result, generatedQuery); // Save to active result store
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

    const queryRequest = createQueryRequest(generatedQuery);

    try {
      const result = await executeQueryMutation.mutateAsync(queryRequest);
      setActiveResult(result, generatedQuery); // Save to active result store
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

    // For basic chart types (bar, line, pie), the inline chart will handle the display
    // For dashboard, navigate to dashboard builder
    if (type === 'dashboard') {
      window.open('/dashboard', '_blank');
    }
    // For other advanced types, navigate to appropriate pages
    else if (!['bar', 'line', 'pie'].includes(type)) {
      window.open('/interactive', '_blank');
    }
    // For bar, line, pie - the inline chart in QueryResult will handle the display
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
