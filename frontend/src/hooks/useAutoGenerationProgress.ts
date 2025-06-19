import { useState, useCallback, useRef } from 'react';
import { GenerationPhase } from '../components/DBExplorer/AutoGenerationProgressPanel';

export interface AutoGenerationProgressState {
  phases: GenerationPhase[];
  overallProgress: number;
  isActive: boolean;
  isCompleted: boolean;
  hasErrors: boolean;
  currentPhase?: string;
}

export interface AutoGenerationProgressActions {
  startGeneration: () => void;
  updatePhase: (phase: string, update: Partial<GenerationPhase>) => void;
  addPhase: (phase: GenerationPhase) => void;
  completeGeneration: (success: boolean) => void;
  reset: () => void;
}

const initialPhases: GenerationPhase[] = [
  {
    phase: 'Initialization',
    status: 'pending',
    progress: 0,
    message: 'Preparing auto-generation request...',
    timestamp: new Date()
  },
  {
    phase: 'Schema Analysis',
    status: 'pending',
    progress: 0,
    message: 'Analyzing database schema and selected tables...',
    timestamp: new Date()
  },
  {
    phase: 'AI Processing',
    status: 'pending',
    progress: 0,
    message: 'Generating business context using AI...',
    timestamp: new Date()
  },
  {
    phase: 'Content Generation',
    status: 'pending',
    progress: 0,
    message: 'Creating table contexts and glossary terms...',
    timestamp: new Date()
  },
  {
    phase: 'Validation',
    status: 'pending',
    progress: 0,
    message: 'Validating generated content...',
    timestamp: new Date()
  },
  {
    phase: 'Completion',
    status: 'pending',
    progress: 0,
    message: 'Finalizing results...',
    timestamp: new Date()
  }
];

export const useAutoGenerationProgress = (): [AutoGenerationProgressState, AutoGenerationProgressActions] => {
  const [phases, setPhases] = useState<GenerationPhase[]>(initialPhases);
  const [isActive, setIsActive] = useState(false);
  const [isCompleted, setIsCompleted] = useState(false);
  const progressTimerRef = useRef<NodeJS.Timeout | null>(null);

  const calculateOverallProgress = useCallback((currentPhases: GenerationPhase[]) => {
    const totalPhases = currentPhases.length;
    const completedPhases = currentPhases.filter(p => p.status === 'completed').length;
    const activePhase = currentPhases.find(p => p.status === 'active');
    
    let progress = (completedPhases / totalPhases) * 100;
    
    // Add partial progress for active phase
    if (activePhase) {
      progress += (activePhase.progress / totalPhases);
    }
    
    return Math.min(Math.round(progress), 100);
  }, []);

  const hasErrors = phases.some(p => p.status === 'error');
  const overallProgress = calculateOverallProgress(phases);
  const currentPhase = phases.find(p => p.status === 'active')?.phase;

  const startGeneration = useCallback(() => {
    setIsActive(true);
    setIsCompleted(false);
    setPhases(initialPhases.map(phase => ({ ...phase, timestamp: new Date() })));
    
    // Start with initialization phase
    setPhases(prev => prev.map((phase, index) => 
      index === 0 
        ? { ...phase, status: 'active', message: 'Initializing auto-generation process...' }
        : phase
    ));
  }, []);

  const updatePhase = useCallback((phaseName: string, update: Partial<GenerationPhase>) => {
    setPhases(prev => prev.map(phase => 
      phase.phase === phaseName 
        ? { ...phase, ...update, timestamp: new Date() }
        : phase
    ));
  }, []);

  const addPhase = useCallback((newPhase: GenerationPhase) => {
    setPhases(prev => [...prev, newPhase]);
  }, []);

  const completeGeneration = useCallback((success: boolean) => {
    setIsActive(false);
    setIsCompleted(true);
    
    // Complete all remaining phases
    setPhases(prev => prev.map(phase => {
      if (phase.status === 'active') {
        return {
          ...phase,
          status: success ? 'completed' : 'error',
          progress: 100,
          message: success ? 'Completed successfully' : 'Failed to complete',
          timestamp: new Date()
        };
      }
      if (phase.status === 'pending') {
        return {
          ...phase,
          status: success ? 'completed' : 'pending',
          progress: success ? 100 : 0,
          timestamp: new Date()
        };
      }
      return phase;
    }));

    if (progressTimerRef.current) {
      clearInterval(progressTimerRef.current);
      progressTimerRef.current = null;
    }
  }, []);

  const reset = useCallback(() => {
    setPhases(initialPhases);
    setIsActive(false);
    setIsCompleted(false);
    
    if (progressTimerRef.current) {
      clearInterval(progressTimerRef.current);
      progressTimerRef.current = null;
    }
  }, []);

  const state: AutoGenerationProgressState = {
    phases,
    overallProgress,
    isActive,
    isCompleted,
    hasErrors,
    currentPhase
  };

  const actions: AutoGenerationProgressActions = {
    startGeneration,
    updatePhase,
    addPhase,
    completeGeneration,
    reset
  };

  return [state, actions];
};

// Helper functions for common progress updates
export const createPhaseUpdate = (
  status: GenerationPhase['status'],
  progress: number,
  message: string,
  details?: GenerationPhase['details']
): Partial<GenerationPhase> => ({
  status,
  progress,
  message,
  details,
  timestamp: new Date()
});

export const createTableProcessingUpdate = (
  currentTable: string,
  tablesProcessed: number,
  totalTables: number,
  operation: string
): Partial<GenerationPhase> => ({
  status: 'active',
  progress: Math.round((tablesProcessed / totalTables) * 100),
  message: `Processing table ${tablesProcessed + 1} of ${totalTables}`,
  details: {
    currentTable,
    tablesProcessed,
    totalTables,
    currentOperation: operation
  },
  timestamp: new Date()
});

export const createAIInteractionUpdate = (
  prompt: string,
  response?: string,
  errors?: string[],
  warnings?: string[]
): Partial<GenerationPhase> => ({
  status: response ? 'active' : 'active',
  message: response ? 'AI response received' : 'Waiting for AI response...',
  details: {
    prompt,
    response,
    errors,
    warnings
  },
  timestamp: new Date()
});
