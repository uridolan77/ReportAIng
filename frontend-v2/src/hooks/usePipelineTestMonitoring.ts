import { useState, useEffect, useCallback, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { message } from 'antd';
import { useAppSelector } from '@shared/hooks';
import { selectAccessToken } from '@shared/store/auth';
import { useSignalRTokenMonitor } from '@shared/hooks/useTokenMonitor';
import { PipelineTestSession, PipelineStepProgress, PipelineStep } from '../types/aiPipelineTest';

interface PipelineTestMonitoringState {
  isConnected: boolean;
  currentSession: PipelineTestSession | null;
  stepProgress: Record<string, PipelineStepProgress>;
  connectionError: string | null;
  isReconnecting: boolean;
}

export const usePipelineTestMonitoring = () => {
  const [state, setState] = useState<PipelineTestMonitoringState>({
    isConnected: false,
    currentSession: null,
    stepProgress: {},
    connectionError: null,
    isReconnecting: false
  });

  const connectionRef = useRef<HubConnection | null>(null);
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const accessToken = useAppSelector(selectAccessToken);

  const createConnection = useCallback(() => {
    if (!accessToken) {
      setState(prev => ({ ...prev, connectionError: 'No authentication token available' }));
      return null;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${window.location.origin}/hubs/pipeline-test`, {
        accessTokenFactory: () => accessToken
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .configureLogging(LogLevel.Information)
      .build();

    // Connection event handlers
    connection.onreconnecting(() => {
      setState(prev => ({ ...prev, isReconnecting: true, connectionError: null }));
    });

    connection.onreconnected(() => {
      setState(prev => ({ ...prev, isConnected: true, isReconnecting: false, connectionError: null }));
      message.success('Reconnected to pipeline test monitoring');
    });

    connection.onclose((error) => {
      setState(prev => ({ 
        ...prev, 
        isConnected: false, 
        isReconnecting: false,
        connectionError: error?.message || 'Connection closed'
      }));
      
      if (error) {
        console.error('Pipeline test hub connection closed with error:', error);
        message.error('Lost connection to pipeline test monitoring');
      }
    });

    // Test event handlers
    connection.on('TestStarted', (data) => {
      console.log('🧪 Test started:', data);
      setState(prev => ({
        ...prev,
        currentSession: prev.currentSession ? {
          ...prev.currentSession,
          query: data.testRequest.query,
          steps: data.testRequest.steps,
          status: 'running',
          startTime: data.timestamp,
          stepProgress: data.testRequest.steps.map((step: PipelineStep) => ({
            step,
            status: 'pending',
            progress: 0
          }))
        } : {
          sessionId: data.testId,
          testId: data.testId,
          query: data.testRequest.query,
          steps: data.testRequest.steps,
          status: 'running',
          startTime: data.timestamp,
          stepProgress: data.testRequest.steps.map((step: PipelineStep) => ({
            step,
            status: 'pending',
            progress: 0
          }))
        },
        stepProgress: {}
      }));
    });

    connection.on('StepStarted', (data) => {
      console.log('🔄 Step started:', data);
      setState(prev => ({
        ...prev,
        stepProgress: {
          ...prev.stepProgress,
          [data.stepName]: {
            step: data.stepName,
            status: 'running',
            startTime: data.timestamp,
            progress: 0,
            message: 'Starting...'
          }
        }
      }));
    });

    connection.on('StepProgress', (data) => {
      console.log('📊 Step progress:', data);
      setState(prev => ({
        ...prev,
        stepProgress: {
          ...prev.stepProgress,
          [data.stepName]: {
            ...prev.stepProgress[data.stepName],
            progress: data.progressPercent,
            message: data.message || 'Processing...'
          }
        }
      }));
    });

    connection.on('StepCompleted', (data) => {
      console.log('✅ Step completed:', data);
      console.log('✅ Step completed - stepName:', data.stepName);
      console.log('✅ Step completed - stepResult:', data.stepResult);

      if (data.stepName === 'AIGeneration') {
        console.log('🤖 [AI-GENERATION-SIGNALR] AIGeneration step completed!');
        console.log('🤖 [AI-GENERATION-SIGNALR] Step result:', data.stepResult);
        console.log('🤖 [AI-GENERATION-SIGNALR] Full data:', data);
      }

      setState(prev => ({
        ...prev,
        stepProgress: {
          ...prev.stepProgress,
          [data.stepName]: {
            ...prev.stepProgress[data.stepName],
            status: 'completed',
            endTime: data.timestamp,
            progress: 100,
            message: 'Completed',
            details: data.stepResult
          }
        }
      }));

      if (data.stepName === 'AIGeneration') {
        console.log('🤖 [AI-GENERATION-SIGNALR] Updated state for AIGeneration');
      }
    });

    connection.on('StepError', (data) => {
      console.error('❌ Step error:', data);
      setState(prev => ({
        ...prev,
        stepProgress: {
          ...prev.stepProgress,
          [data.stepName]: {
            ...prev.stepProgress[data.stepName],
            status: 'error',
            endTime: data.timestamp,
            message: data.error,
            details: data.details
          }
        }
      }));
      message.error(`Step ${data.stepName} failed: ${data.error}`);
    });

    connection.on('TestSessionJoined', (data) => {
      console.log('👥 Joined test session:', data);
      message.success(`Joined test session: ${data.testId}`);

      // Set up initial session state when joining
      setState(prev => ({
        ...prev,
        currentSession: {
          sessionId: data.testId,
          testId: data.testId,
          query: '', // Will be updated when test starts
          steps: [], // Will be updated when test starts
          status: 'waiting',
          startTime: data.joinedAt,
          stepProgress: []
        }
      }));
    });

    connection.on('TestCompleted', (data) => {
      console.log('🎉 Test completed:', data);
      setState(prev => ({
        ...prev,
        currentSession: prev.currentSession ? {
          ...prev.currentSession,
          status: 'completed',
          endTime: data.timestamp,
          results: data.testResult
        } : null
      }));
      message.success('Pipeline test completed successfully');
    });

    connection.on('TestError', (data) => {
      console.error('💥 Test error:', data);
      setState(prev => ({
        ...prev,
        currentSession: prev.currentSession ? {
          ...prev.currentSession,
          status: 'error',
          endTime: data.timestamp
        } : null
      }));
      message.error(`Pipeline test failed: ${data.error}`);
    });

    connection.on('TestSessionLeft', (data) => {
      console.log('👋 Left test session:', data);
    });

    connection.on('HeartbeatResponse', (data) => {
      console.debug('💓 Heartbeat response:', data);
    });

    return connection;
  }, [accessToken]);

  const connect = useCallback(async () => {
    if (connectionRef.current?.state === 'Connected') {
      return;
    }

    try {
      setState(prev => ({ ...prev, isReconnecting: true, connectionError: null }));

      const connection = createConnection();
      if (!connection) {
        setState(prev => ({ ...prev, isReconnecting: false, connectionError: 'Failed to create connection' }));
        return;
      }

      connectionRef.current = connection;
      await connection.start();

      setState(prev => ({ ...prev, isConnected: true, connectionError: null, isReconnecting: false }));
      console.log('🔗 Connected to pipeline test hub');

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to connect';
      setState(prev => ({ ...prev, connectionError: errorMessage, isReconnecting: false }));
      console.error('Failed to connect to pipeline test hub:', error);

      // Don't show error message for negotiation errors as they often recover
      if (!errorMessage.includes('negotiation')) {
        message.error(`Connection failed: ${errorMessage}`);
      }
    }
  }, [createConnection]);

  const disconnect = useCallback(async () => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.stop();
        connectionRef.current = null;
        setState(prev => ({ 
          ...prev, 
          isConnected: false, 
          currentSession: null, 
          stepProgress: {},
          connectionError: null 
        }));
        console.log('🔌 Disconnected from pipeline test hub');
      } catch (error) {
        console.error('Error disconnecting from pipeline test hub:', error);
      }
    }
  }, []);

  const joinTestSession = useCallback(async (testId: string) => {
    if (connectionRef.current?.state === 'Connected') {
      try {
        await connectionRef.current.invoke('JoinTestSession', testId);
        console.log(`👥 Joined test session: ${testId}`);
      } catch (error) {
        console.error('Failed to join test session:', error);
        message.error('Failed to join test session');
      }
    }
  }, []);

  const leaveTestSession = useCallback(async (testId: string) => {
    if (connectionRef.current?.state === 'Connected') {
      try {
        await connectionRef.current.invoke('LeaveTestSession', testId);
        console.log(`👋 Left test session: ${testId}`);
      } catch (error) {
        console.error('Failed to leave test session:', error);
      }
    }
  }, []);

  const sendHeartbeat = useCallback(async () => {
    if (connectionRef.current?.state === 'Connected') {
      try {
        await connectionRef.current.invoke('Heartbeat');
      } catch (error) {
        console.error('Failed to send heartbeat:', error);
      }
    }
  }, []);

  // Auto-connect on mount
  useEffect(() => {
    connect();
    return () => {
      disconnect();
    };
  }, [connect, disconnect]);

  // Heartbeat interval
  useEffect(() => {
    if (state.isConnected) {
      const interval = setInterval(sendHeartbeat, 30000); // Every 30 seconds
      return () => clearInterval(interval);
    }
  }, [state.isConnected, sendHeartbeat]);

  // Clear reconnect timeout on unmount
  useEffect(() => {
    return () => {
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
      }
    };
  }, []);

  // Monitor token expiration and disconnect SignalR when token expires
  useSignalRTokenMonitor(() => {
    console.log('🔌 Token expired - disconnecting pipeline test monitoring SignalR');
    disconnect();
  });

  return {
    ...state,
    connect,
    disconnect,
    joinTestSession,
    leaveTestSession,
    sendHeartbeat
  };
};
