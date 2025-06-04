import { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { API_CONFIG } from '../config/api';
import { useAuthStore } from '../stores/authStore';

interface WebSocketMessage {
  data: string;
  type?: string;
  timestamp?: string;
}

interface UseSignalRReturn {
  isConnected: boolean;
  lastMessage: WebSocketMessage | null;
  sendMessage: (message: string) => void;
  disconnect: () => void;
  connection: signalR.HubConnection | null;
}

export const useSignalR = (): UseSignalRReturn => {
  const [isConnected, setIsConnected] = useState(false);
  const [lastMessage, setLastMessage] = useState<WebSocketMessage | null>(null);
  const connection = useRef<signalR.HubConnection | null>(null);
  const { token } = useAuthStore();

  const sendMessage = (message: string) => {
    if (connection.current && connection.current.state === signalR.HubConnectionState.Connected) {
      connection.current.send('SendMessage', message);
    }
  };

  const disconnect = () => {
    if (connection.current) {
      connection.current.stop();
    }
  };

  useEffect(() => {
    const connectSignalR = async () => {
      try {
        // Create SignalR connection
        const hubConnection = new signalR.HubConnectionBuilder()
          .withUrl(API_CONFIG.SIGNALR_HUB_URL, {
            accessTokenFactory: () => token || '',
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets,
          })
          .withAutomaticReconnect()
          .configureLogging(signalR.LogLevel.Information)
          .build();

        connection.current = hubConnection;

        // Set up event handlers
        hubConnection.on('QueryProgress', (data) => {
          const message: WebSocketMessage = {
            data: JSON.stringify(data),
            type: 'QueryProgress',
            timestamp: new Date().toISOString(),
          };
          setLastMessage(message);
        });

        hubConnection.on('DetailedProgress', (data) => {
          const message: WebSocketMessage = {
            data: JSON.stringify(data),
            type: 'DetailedProgress',
            timestamp: new Date().toISOString(),
          };
          setLastMessage(message);
        });

        hubConnection.on('QueryCompleted', (data) => {
          const message: WebSocketMessage = {
            data: JSON.stringify(data),
            type: 'QueryCompleted',
            timestamp: new Date().toISOString(),
          };
          setLastMessage(message);
        });

        hubConnection.on('QueryError', (data) => {
          const message: WebSocketMessage = {
            data: JSON.stringify(data),
            type: 'QueryError',
            timestamp: new Date().toISOString(),
          };
          setLastMessage(message);
        });

        hubConnection.on('AutoGenerationProgress', (data) => {
          console.log('🔄 Received AutoGenerationProgress via SignalR:', data);
          console.log('🔄 AutoGenerationProgress data type:', typeof data);
          console.log('🔄 AutoGenerationProgress data keys:', Object.keys(data || {}));
          const message: WebSocketMessage = {
            data: JSON.stringify(data),
            type: 'AutoGenerationProgress',
            timestamp: new Date().toISOString(),
          };
          setLastMessage(message);
        });

        hubConnection.on('QueryProcessingProgress', (data) => {
          console.log('🔄 Received QueryProcessingProgress via SignalR:', data);
          const message: WebSocketMessage = {
            data: JSON.stringify(data),
            type: 'QueryProcessingProgress',
            timestamp: new Date().toISOString(),
          };
          setLastMessage(message);
        });

        // Add handler for connection info debugging
        hubConnection.on('ConnectionInfo', (data) => {
          console.log('🔗 Received ConnectionInfo via SignalR:', data);
        });

        // Connection state handlers
        hubConnection.onclose((error) => {
          console.log('🔗 SignalR connection closed', error);
          setIsConnected(false);
        });

        hubConnection.onreconnecting((error) => {
          console.log('🔗 SignalR reconnecting...', error);
          setIsConnected(false);
        });

        hubConnection.onreconnected((connectionId) => {
          console.log('🔗 SignalR reconnected with ID:', connectionId);
          setIsConnected(true);
        });

        // Start the connection
        console.log('🔗 Starting SignalR connection to:', API_CONFIG.SIGNALR_HUB_URL);
        console.log('🔗 Auth Token (first 20 chars):', token?.substring(0, 20) + '...');

        await hubConnection.start();
        setIsConnected(true);

        console.log('🔗 SignalR Connected successfully');
        console.log('🔗 SignalR Connection ID:', hubConnection.connectionId);
        console.log('🔗 SignalR Connection State:', hubConnection.state);
        console.log('🔗 SignalR Hub URL:', API_CONFIG.SIGNALR_HUB_URL);

        // Test the connection and get connection info
        try {
          await hubConnection.invoke('GetConnectionInfo');
          console.log('🔗 SignalR GetConnectionInfo called successfully');
        } catch (testError) {
          console.warn('🔗 SignalR GetConnectionInfo failed:', testError);
        }

        // Join user group for receiving notifications
        try {
          const authStore = await import('../stores/authStore');
          const user = authStore.useAuthStore.getState().user;
          if (user?.id) {
            console.log('🔗 Joining user group for user:', user.id);
            // The hub automatically adds users to their group on connection
          }
        } catch (userError) {
          console.warn('🔗 Could not get user info for group joining:', userError);
        }
      } catch (error) {
        console.error('SignalR connection error:', error);
        setIsConnected(false);

        // Fallback to mock connection for development
        setIsConnected(true);
        const interval = setInterval(() => {
          const mockMessage: WebSocketMessage = {
            data: JSON.stringify({
              type: 'heartbeat',
              timestamp: new Date().toISOString(),
            }),
          };
          setLastMessage(mockMessage);
        }, 30000);

        return () => clearInterval(interval);
      }
    };

    if (token) {
      connectSignalR();
    }

    return () => {
      if (connection.current) {
        connection.current.stop();
      }
    };
  }, [token]);

  return {
    isConnected,
    lastMessage,
    sendMessage,
    disconnect,
    connection: connection.current,
  };
};

// Keep the old useWebSocket for backward compatibility
export const useWebSocket = (url: string) => {
  const signalR = useSignalR();
  return {
    isConnected: signalR.isConnected,
    lastMessage: signalR.lastMessage,
    sendMessage: signalR.sendMessage,
    disconnect: signalR.disconnect,
  };
};
