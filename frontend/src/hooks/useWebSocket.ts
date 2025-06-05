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
      if (!token) {
        console.warn('⚠️ No auth token available for SignalR connection');
        return;
      }

      // Prevent multiple connections
      if (connection.current?.state === 'Connected' || connection.current?.state === 'Connecting') {
        console.log('🔗 SignalR already connected or connecting, skipping...');
        return;
      }

      // Stop any existing connection first
      if (connection.current) {
        try {
          console.log('🔗 Stopping existing SignalR connection...');
          await connection.current.stop();
        } catch (error) {
          console.warn('⚠️ Error stopping existing SignalR connection:', error);
        }
        connection.current = null;
      }

      try {
        // Create SignalR connection
        const hubConnection = new signalR.HubConnectionBuilder()
          .withUrl(API_CONFIG.SIGNALR_HUB_URL, {
            accessTokenFactory: async () => {
              // Get the latest token from auth store
              const authStore = await import('../stores/authStore');
              const currentToken = authStore.useAuthStore.getState().token;

              console.log('🔗 SignalR accessTokenFactory called');
              console.log('🔗 Current encrypted token available:', !!currentToken);
              console.log('🔗 Current encrypted token length:', currentToken?.length || 0);

              if (!currentToken) {
                console.warn('⚠️ No token available for SignalR connection');
                return '';
              }

              // Decrypt the token if it's encrypted
              try {
                const { SecurityUtils } = await import('../utils/security');
                const decryptedToken = await SecurityUtils.decryptToken(currentToken);

                console.log('🔗 Token decryption successful');
                console.log('🔗 Decrypted token length:', decryptedToken?.length || 0);
                console.log('🔗 Decrypted token format check:', decryptedToken?.includes('.') ? 'Valid JWT format' : 'Invalid JWT format');
                console.log('🔗 SignalR using decrypted token (first 50 chars):', decryptedToken?.substring(0, 50) + '...');

                // Validate JWT format
                if (decryptedToken && decryptedToken.split('.').length === 3) {
                  console.log('✅ JWT token format is valid');
                  return decryptedToken;
                } else {
                  console.error('❌ JWT token format is invalid - expected 3 parts separated by dots');
                  return '';
                }
              } catch (error) {
                console.error('❌ Failed to decrypt token for SignalR:', error);
                console.log('🔗 Attempting to use original token as fallback...');

                // Check if the original token might already be decrypted
                if (currentToken.includes('.') && currentToken.split('.').length === 3) {
                  console.log('🔗 Original token appears to be a valid JWT, using as-is');
                  return currentToken;
                } else {
                  console.error('❌ Original token is also not a valid JWT format');
                  return '';
                }
              }
            },
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

        // Add handler for test connection response
        hubConnection.on('TestConnectionResponse', (data) => {
          console.log('🔗 Received TestConnectionResponse via SignalR:', data);
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

        await hubConnection.start();
        setIsConnected(true);

        console.log('🔗 SignalR Connected successfully');
        console.log('🔗 SignalR Connection ID:', hubConnection.connectionId);
        console.log('🔗 SignalR Connection State:', hubConnection.state);
        console.log('🔗 SignalR Hub URL:', API_CONFIG.SIGNALR_HUB_URL);

        // Expose connection globally for debugging
        (window as any).signalRConnection = hubConnection;
        console.log('🔗 SignalR connection exposed globally as window.signalRConnection');

        // Test the connection and get connection info
        try {
          await hubConnection.invoke('GetConnectionInfo');
          console.log('🔗 SignalR GetConnectionInfo called successfully');
        } catch (testError) {
          console.warn('🔗 SignalR GetConnectionInfo failed:', testError);
        }

        // Get user info and verify authentication
        try {
          const authStore = await import('../stores/authStore');
          const user = authStore.useAuthStore.getState().user;
          const currentToken = authStore.useAuthStore.getState().token;

          console.log('🔗 Current user info:', {
            userId: user?.id,
            username: user?.username,
            hasToken: !!currentToken,
            tokenLength: currentToken?.length || 0
          });

          if (user?.id) {
            console.log('🔗 User authenticated - should be automatically added to group user_' + user.id);

            // Test sending a message to verify the connection works
            try {
              await hubConnection.invoke('TestConnection');
              console.log('🔗 SignalR TestConnection called successfully');
            } catch (testConnError) {
              console.warn('🔗 SignalR TestConnection failed:', testConnError);
            }
          } else {
            console.warn('🔗 No user ID found - SignalR connection may not receive user-specific messages');
          }
        } catch (userError) {
          console.warn('🔗 Could not get user info for group verification:', userError);
        }
      } catch (error) {
        console.error('❌ SignalR connection failed:', error);
        console.error('❌ Error details:', {
          message: error instanceof Error ? error.message : 'Unknown error',
          stack: error instanceof Error ? error.stack : undefined,
          hubUrl: API_CONFIG.SIGNALR_HUB_URL,
          hasToken: !!token,
          tokenLength: token?.length || 0
        });
        setIsConnected(false);

        // Don't fall back to mock connection - let the user know it failed
        console.warn('⚠️ SignalR connection failed - auto-generation progress will not be available');
        return;
      }
    };

    if (token) {
      connectSignalR();
    }

    return () => {
      if (connection.current) {
        console.log('🔗 Cleaning up SignalR connection...');
        connection.current.stop().catch((error) => {
          console.warn('⚠️ Error during SignalR cleanup:', error);
        });
        connection.current = null;
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
