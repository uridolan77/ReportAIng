import { useState, useEffect, useCallback } from 'react';
import { ProcessFlowData, ProcessStep } from '../components/ProcessFlowViewer';
import { socketService } from '@shared/services/socketService';

interface UseProcessFlowOptions {
  sessionId?: string;
  enableRealTime?: boolean;
}

interface ProcessFlowHook {
  processData: ProcessFlowData | null;
  isVisible: boolean;
  showProcessFlow: (sessionId: string, initialData?: Partial<ProcessFlowData>) => void;
  hideProcessFlow: () => void;
  updateStep: (stepId: string, updates: Partial<ProcessStep>) => void;
  addStep: (step: ProcessStep) => void;
  setTransparencyData: (transparency: ProcessFlowData['transparency']) => void;
}

export const useProcessFlow = (options: UseProcessFlowOptions = {}): ProcessFlowHook => {
  const [processData, setProcessData] = useState<ProcessFlowData | null>(null);
  const [isVisible, setIsVisible] = useState(false);

  // Initialize default process steps based on the chat flow
  const getDefaultSteps = (query: string): ProcessStep[] => [
    {
      id: 'auth',
      name: 'Authentication & Authorization',
      description: 'Validating JWT token and user permissions',
      status: 'pending',
      details: {
        metadata: {
          endpoint: '/api/query/enhanced',
          method: 'POST'
        }
      }
    },
    {
      id: 'request-validation',
      name: 'Request Validation',
      description: 'Validating request parameters and structure',
      status: 'pending',
      details: {
        input: {
          query,
          executeQuery: true,
          includeAlternatives: true,
          includeSemanticAnalysis: true
        }
      }
    },
    {
      id: 'semantic-analysis',
      name: 'Semantic Analysis',
      description: 'Analyzing query intent and extracting semantic meaning',
      status: 'pending',
      subSteps: [
        {
          id: 'intent-detection',
          name: 'Intent Detection',
          description: 'Determining query type and business intent',
          status: 'pending'
        },
        {
          id: 'entity-extraction',
          name: 'Entity Extraction',
          description: 'Identifying tables, columns, and business entities',
          status: 'pending'
        }
      ]
    },
    {
      id: 'schema-retrieval',
      name: 'Schema Retrieval',
      description: 'Fetching relevant database schema information',
      status: 'pending',
      details: {
        metadata: {
          schemaSource: 'semantic-layer'
        }
      }
    },
    {
      id: 'prompt-building',
      name: 'Prompt Construction',
      description: 'Building enhanced prompt with schema and business rules',
      status: 'pending',
      subSteps: [
        {
          id: 'context-gathering',
          name: 'Context Gathering',
          description: 'Collecting schema, examples, and business rules',
          status: 'pending'
        },
        {
          id: 'prompt-assembly',
          name: 'Prompt Assembly',
          description: 'Assembling final prompt for AI model',
          status: 'pending'
        }
      ]
    },
    {
      id: 'ai-generation',
      name: 'AI SQL Generation',
      description: 'Calling OpenAI API to generate SQL query',
      status: 'pending',
      subSteps: [
        {
          id: 'openai-request',
          name: 'OpenAI API Call',
          description: 'Sending request to OpenAI GPT-4 model',
          status: 'pending'
        },
        {
          id: 'response-parsing',
          name: 'Response Parsing',
          description: 'Parsing and validating AI response',
          status: 'pending'
        }
      ]
    },
    {
      id: 'sql-validation',
      name: 'SQL Validation',
      description: 'Validating generated SQL syntax and semantics',
      status: 'pending'
    },
    {
      id: 'sql-execution',
      name: 'SQL Execution',
      description: 'Executing validated SQL query against database',
      status: 'pending',
      subSteps: [
        {
          id: 'query-execution',
          name: 'Query Execution',
          description: 'Running SQL query on database',
          status: 'pending'
        },
        {
          id: 'result-processing',
          name: 'Result Processing',
          description: 'Processing and formatting query results',
          status: 'pending'
        }
      ]
    },
    {
      id: 'response-assembly',
      name: 'Response Assembly',
      description: 'Assembling final response with results and metadata',
      status: 'pending'
    }
  ];

  const showProcessFlow = useCallback((sessionId: string, initialData?: Partial<ProcessFlowData>) => {
    const defaultData: ProcessFlowData = {
      sessionId,
      query: initialData?.query || '',
      userId: initialData?.userId || 'current-user',
      startTime: new Date().toISOString(),
      status: 'running',
      steps: getDefaultSteps(initialData?.query || ''),
      ...initialData
    };

    setProcessData(defaultData);
    setIsVisible(true);

    // Start the first step
    updateStepStatus('auth', 'running');
  }, []);

  const hideProcessFlow = useCallback(() => {
    setIsVisible(false);
  }, []);

  const updateStepStatus = useCallback((stepId: string, status: ProcessStep['status'], details?: Partial<ProcessStep>) => {
    setProcessData(prev => {
      if (!prev) return prev;

      const updateStepInArray = (steps: ProcessStep[]): ProcessStep[] => {
        return steps.map(step => {
          if (step.id === stepId) {
            const updatedStep = {
              ...step,
              status,
              ...details
            };

            // Set timing information
            if (status === 'running' && !updatedStep.startTime) {
              updatedStep.startTime = new Date().toISOString();
            } else if ((status === 'completed' || status === 'error') && updatedStep.startTime && !updatedStep.endTime) {
              updatedStep.endTime = new Date().toISOString();
              updatedStep.duration = new Date(updatedStep.endTime).getTime() - new Date(updatedStep.startTime).getTime();
            }

            return updatedStep;
          }

          // Check substeps
          if (step.subSteps) {
            return {
              ...step,
              subSteps: updateStepInArray(step.subSteps)
            };
          }

          return step;
        });
      };

      return {
        ...prev,
        steps: updateStepInArray(prev.steps)
      };
    });
  }, []);

  const updateStep = useCallback((stepId: string, updates: Partial<ProcessStep>) => {
    updateStepStatus(stepId, updates.status || 'pending', updates);
  }, [updateStepStatus]);

  const addStep = useCallback((step: ProcessStep) => {
    setProcessData(prev => {
      if (!prev) return prev;
      return {
        ...prev,
        steps: [...prev.steps, step]
      };
    });
  }, []);

  const setTransparencyData = useCallback((transparency: ProcessFlowData['transparency']) => {
    setProcessData(prev => {
      if (!prev) return prev;
      return {
        ...prev,
        transparency
      };
    });
  }, []);

  // Socket event handlers for real-time updates
  useEffect(() => {
    if (!options.enableRealTime || !processData?.sessionId) return;

    const handleStepUpdate = (data: { stepId: string; status: string; details?: any }) => {
      updateStepStatus(data.stepId, data.status as ProcessStep['status'], data.details);
    };

    const handleProcessComplete = (data: { status: string; totalDuration: number; transparency?: any }) => {
      setProcessData(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          status: data.status as ProcessFlowData['status'],
          endTime: new Date().toISOString(),
          totalDuration: data.totalDuration,
          transparency: data.transparency || prev.transparency
        };
      });
    };

    const handleStepLog = (data: { stepId: string; log: string }) => {
      setProcessData(prev => {
        if (!prev) return prev;

        const updateStepLogs = (steps: ProcessStep[]): ProcessStep[] => {
          return steps.map(step => {
            if (step.id === data.stepId) {
              return {
                ...step,
                details: {
                  ...step.details,
                  logs: [...(step.details?.logs || []), data.log]
                }
              };
            }
            if (step.subSteps) {
              return {
                ...step,
                subSteps: updateStepLogs(step.subSteps)
              };
            }
            return step;
          });
        };

        return {
          ...prev,
          steps: updateStepLogs(prev.steps)
        };
      });
    };

    // Subscribe to socket events
    socketService.on('process:step:update', handleStepUpdate);
    socketService.on('process:complete', handleProcessComplete);
    socketService.on('process:step:log', handleStepLog);

    return () => {
      socketService.off('process:step:update', handleStepUpdate);
      socketService.off('process:complete', handleProcessComplete);
      socketService.off('process:step:log', handleStepLog);
    };
  }, [options.enableRealTime, processData?.sessionId, updateStepStatus]);

  // Simulate process flow for demo purposes (remove in production)
  useEffect(() => {
    if (!processData || processData.status !== 'running') return;

    const simulateProgress = async () => {
      const steps = [
        'auth',
        'request-validation', 
        'semantic-analysis',
        'intent-detection',
        'entity-extraction',
        'schema-retrieval',
        'prompt-building',
        'context-gathering',
        'prompt-assembly',
        'ai-generation',
        'openai-request',
        'response-parsing',
        'sql-validation',
        'sql-execution',
        'query-execution',
        'result-processing',
        'response-assembly'
      ];

      for (let i = 0; i < steps.length; i++) {
        await new Promise(resolve => setTimeout(resolve, 1000 + Math.random() * 2000));
        updateStepStatus(steps[i], 'running');
        
        await new Promise(resolve => setTimeout(resolve, 500 + Math.random() * 1500));
        updateStepStatus(steps[i], 'completed', {
          details: {
            logs: [`Step ${steps[i]} completed successfully`],
            metadata: { timestamp: new Date().toISOString() }
          }
        });
      }

      // Complete the process
      setProcessData(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          status: 'completed',
          endTime: new Date().toISOString(),
          totalDuration: new Date().getTime() - new Date(prev.startTime).getTime()
        };
      });
    };

    simulateProgress();
  }, [processData?.sessionId, updateStepStatus]);

  return {
    processData,
    isVisible,
    showProcessFlow,
    hideProcessFlow,
    updateStep,
    addStep,
    setTransparencyData
  };
};
