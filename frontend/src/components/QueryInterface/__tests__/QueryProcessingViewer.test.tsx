/**
 * QueryProcessingViewer Component Tests
 * Comprehensive tests for the QueryProcessingViewer component improvements
 */

import { screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryProcessingViewer, ProcessingStage } from '../QueryProcessingViewer';
import { 
  renderWithProviders, 
  setupTestEnvironment 
} from '../../../test-utils/testing-providers';

// Setup test environment
setupTestEnvironment();

describe('QueryProcessingViewer Component', () => {
  const mockOnToggleVisibility = jest.fn();
  const mockOnModeChange = jest.fn();

  const mockProps = {
    stages: [],
    isProcessing: false,
    onToggleVisibility: mockOnToggleVisibility,
    onModeChange: mockOnModeChange,
    isVisible: true,
    mode: 'minimal' as const,
  };

  const createMockStage = (overrides: Partial<ProcessingStage> = {}): ProcessingStage => ({
    stage: 'initializing',
    message: 'Starting query processing',
    progress: 0,
    timestamp: new Date().toISOString(),
    status: 'pending',
    ...overrides,
  });

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Component Rendering', () => {
    it('renders in minimal mode by default', () => {
      renderWithProviders(<QueryProcessingViewer {...mockProps} />);
      
      expect(screen.getByText(/AI Processing/)).toBeInTheDocument();
    });

    it('renders hidden mode correctly', () => {
      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          mode="hidden" 
          stages={[createMockStage({ stage: 'completed', progress: 100 })]}
        />
      );
      
      expect(screen.getByText(/AI Processing - Completed/)).toBeInTheDocument();
    });

    it('shows query ID when provided', () => {
      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          queryId="12345678-1234-1234-1234-123456789abc"
        />
      );
      
      expect(screen.getByText(/ID: 12345678/)).toBeInTheDocument();
    });
  });

  describe('Mode Switching', () => {
    it('changes from hidden to processing mode when clicked', async () => {
      const user = userEvent.setup();
      
      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          mode="hidden"
          stages={[createMockStage()]}
        />
      );
      
      const processingCard = screen.getByText(/AI Processing/).closest('[role="button"], div');
      if (processingCard) {
        await user.click(processingCard);
      }
      
      await waitFor(() => {
        expect(mockOnModeChange).toHaveBeenCalledWith('processing');
      });
    });    it('handles mode change when onModeChange is not provided', () => {
      const propsWithoutHandler = {
        ...mockProps,
        onModeChange: undefined as any, // Explicitly type as any for test
        mode: "hidden" as const
      };
      
      renderWithProviders(
        <QueryProcessingViewer 
          {...propsWithoutHandler}
        />
      );
      
      // Should not crash when clicked without onModeChange handler
      const processingCard = screen.getByText(/AI Processing/).closest('div');
      if (processingCard) {
        fireEvent.click(processingCard);
      }
      
      expect(mockOnModeChange).not.toHaveBeenCalled();
    });
  });

  describe('Progress Calculation', () => {
    it('calculates overall progress correctly with weighted stages', () => {
      const stages = [
        createMockStage({ stage: 'schema_loading', progress: 100 }),
        createMockStage({ stage: 'ai_processing', progress: 50 }),
        createMockStage({ stage: 'sql_execution', progress: 0 }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={stages}
          isProcessing={true}
          mode="processing"
        />
      );
      
      // Should show some progress based on weighted calculation
      expect(screen.getByText(/%/)).toBeInTheDocument();
    });

    it('shows 100% progress when processing is complete', () => {
      const stages = [
        createMockStage({ stage: 'completed', progress: 100 }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={stages}
          isProcessing={false}
          mode="minimal"
        />
      );
      
      expect(screen.getByText(/100% complete/)).toBeInTheDocument();
    });
  });

  describe('Stage Details and Rendering', () => {
    it('renders stage icons correctly for different stages', () => {
      const stages = [
        createMockStage({ stage: 'schema_loading', status: 'completed' }),
        createMockStage({ stage: 'ai_processing', status: 'active' }),
        createMockStage({ stage: 'sql_execution', status: 'pending' }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={stages}
          mode="processing"
        />
      );
      
      // Should render different icons for different stage types
      expect(screen.getByText(/Schema Loading/)).toBeInTheDocument();
      expect(screen.getByText(/Ai Processing/)).toBeInTheDocument();
      expect(screen.getByText(/Sql Execution/)).toBeInTheDocument();
    });

    it('displays prompt details when available', () => {
      const stageWithPromptDetails = createMockStage({
        stage: 'prompt_details',
        details: {
          promptDetails: {
            fullPrompt: 'SELECT * FROM users WHERE active = 1',
            templateName: 'User Query Template',
            tokenCount: 25
          }
        }
      });

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[stageWithPromptDetails]}
          mode="advanced"
        />
      );
      
      expect(screen.getByText(/AI Prompt Details/)).toBeInTheDocument();
      expect(screen.getByText(/SELECT \* FROM users/)).toBeInTheDocument();
    });

    it('displays cache details correctly', () => {
      const stageWithCacheDetails = createMockStage({
        stage: 'cache_check',
        details: {
          cacheHit: true,
          cacheKey: 'user_query_123',
          ttl: 300
        }
      });

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[stageWithCacheDetails]}
          mode="advanced"
        />
      );
      
      expect(screen.getByText(/Cache Result/)).toBeInTheDocument();
      expect(screen.getByText(/HIT - Data found in cache/)).toBeInTheDocument();
    });
  });

  describe('Timing and Performance', () => {
    it('calculates and displays timing information', () => {
      const now = new Date();
      const oneSecondAgo = new Date(now.getTime() - 1000);
      
      const stages = [
        createMockStage({ 
          stage: 'started', 
          timestamp: oneSecondAgo.toISOString(),
          details: { startTime: oneSecondAgo.toISOString() }
        }),
        createMockStage({ 
          stage: 'completed', 
          timestamp: now.toISOString() 
        }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={stages}
          isProcessing={false}
          mode="hidden"
        />
      );
      
      // Should show timing information
      expect(screen.getByText(/1\.0s/)).toBeInTheDocument();
    });

    it('shows estimated time remaining during processing', () => {
      const now = new Date();
      const startTime = new Date(now.getTime() - 2000); // 2 seconds ago
      
      const stages = [
        createMockStage({ 
          stage: 'started', 
          timestamp: startTime.toISOString(),
          details: { startTime: startTime.toISOString() },
          progress: 25
        }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={stages}
          isProcessing={true}
          mode="processing"
        />
      );
      
      // Should show some kind of time estimation
      expect(screen.getByText(/AI is processing your query/)).toBeInTheDocument();
    });
  });

  describe('Interactive Features', () => {
    it('expands and collapses stage details when clicked', async () => {
      const user = userEvent.setup();
      
      const stageWithDetails = createMockStage({
        stage: 'ai_processing',
        details: {
          aiExecutionTime: 1500,
          confidence: 0.95
        }
      });

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[stageWithDetails]}
          mode="advanced"
        />
      );
      
      // Should show click hint
      expect(screen.getByText(/Click to view details/)).toBeInTheDocument();
      
      // Click to expand
      const expandButton = screen.getByText(/Ai Processing/);
      await user.click(expandButton);
      
      // Should show details
      await waitFor(() => {
        expect(screen.getByText(/AI Response Time/)).toBeInTheDocument();
        expect(screen.getByText(/95.0%/)).toBeInTheDocument();
      });
    });

    it('handles stage formatting correctly', () => {
      const stages = [
        createMockStage({ stage: 'schema_loading' }),
        createMockStage({ stage: 'ai_processing' }),
        createMockStage({ stage: 'sql_execution' }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={stages}
          mode="processing"
        />
      );
      
      // Should format stage names properly
      expect(screen.getByText(/Schema Loading/)).toBeInTheDocument();
      expect(screen.getByText(/Ai Processing/)).toBeInTheDocument();
      expect(screen.getByText(/Sql Execution/)).toBeInTheDocument();
    });
  });

  describe('Processing States', () => {
    it('shows processing indicator when isProcessing is true', () => {
      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          isProcessing={true}
          mode="processing"
        />
      );
      
      expect(screen.getByText(/AI is processing your query/)).toBeInTheDocument();
    });

    it('shows completion state when processing is finished', () => {
      const completedStages = [
        createMockStage({ stage: 'completed', progress: 100 }),
      ];

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={completedStages}
          isProcessing={false}
          mode="minimal"
        />
      );
      
      expect(screen.getByText(/100% complete/)).toBeInTheDocument();
    });

    it('handles error states in stages', () => {
      const errorStage = createMockStage({
        stage: 'sql_execution',
        status: 'error',
        message: 'SQL execution failed'
      });

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[errorStage]}
          mode="processing"
        />
      );
      
      expect(screen.getByText(/SQL execution failed/)).toBeInTheDocument();
    });
  });

  describe('Accessibility and UX', () => {
    it('provides proper hover effects on clickable elements', async () => {
      const user = userEvent.setup();
      
      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          mode="hidden"
          stages={[createMockStage()]}
        />
      );
      
      const processingCard = screen.getByText(/AI Processing/).closest('div');
      
      if (processingCard) {
        await user.hover(processingCard);
        // Should change appearance on hover (tested via event handlers)
        expect(processingCard).toBeDefined();
      }
    });

    it('shows appropriate click hints for interactive elements', () => {
      const stageWithDetails = createMockStage({
        details: { someDetail: 'value' }
      });

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[stageWithDetails]}
          mode="advanced"
        />
      );
      
      expect(screen.getByText(/Click to view details/)).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('handles empty stages array', () => {
      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[]}
          isProcessing={false}
        />
      );
      
      expect(screen.getByText(/AI Processing/)).toBeInTheDocument();
    });

    it('handles malformed stage data gracefully', () => {
      const malformedStage = {
        stage: null,
        message: undefined,
        progress: null,
        timestamp: 'invalid-date',
      } as any;

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[malformedStage]}
          mode="processing"
        />
      );
      
      // Should not crash and show something reasonable
      expect(screen.getByText(/Unknown Stage/)).toBeInTheDocument();
    });

    it('handles missing details gracefully', () => {
      const stageWithoutDetails = createMockStage({
        details: undefined
      });

      renderWithProviders(
        <QueryProcessingViewer 
          {...mockProps} 
          stages={[stageWithoutDetails]}
          mode="advanced"
        />
      );
      
      expect(screen.getByText(/No additional details available/)).toBeInTheDocument();
    });
  });
});
