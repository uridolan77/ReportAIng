import React from 'react';
import { useAuthStore } from '../stores/authStore';

export interface WebSocketMessage {
  type: string;
  payload: any;
  timestamp: number;
  userId?: string;
  sessionId?: string;
}

export interface CollaborativeEvent {
  type: 'query_executed' | 'dashboard_updated' | 'user_joined' | 'user_left' | 'cursor_moved' | 'selection_changed';
  data: any;
  user: {
    id: string;
    name: string;
    avatar?: string;
  };
  timestamp: number;
}

class WebSocketService {
  private ws: WebSocket | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 1000;
  private heartbeatInterval: NodeJS.Timeout | null = null;
  private messageQueue: WebSocketMessage[] = [];
  private listeners: Map<string, Set<(data: any) => void>> = new Map();
  private isConnecting = false;

  constructor() {
    this.connect();
  }

  private async connect(): Promise<void> {
    if (this.isConnecting || (this.ws && this.ws.readyState === WebSocket.OPEN)) {
      return;
    }

    this.isConnecting = true;

    try {
      const authStore = useAuthStore.getState();
      const token = await authStore.getDecryptedToken();

      if (!token) {
        console.warn('No auth token available for WebSocket connection');
        this.isConnecting = false;
        return;
      }

      const wsUrl = process.env.REACT_APP_WS_URL || 'wss://localhost:55243/ws';
      this.ws = new WebSocket(`${wsUrl}?token=${encodeURIComponent(token)}`);

      this.ws.onopen = this.handleOpen.bind(this);
      this.ws.onmessage = this.handleMessage.bind(this);
      this.ws.onclose = this.handleClose.bind(this);
      this.ws.onerror = this.handleError.bind(this);

    } catch (error) {
      console.error('Failed to connect to WebSocket:', error);
      this.isConnecting = false;
      this.scheduleReconnect();
    }
  }

  private handleOpen(): void {
    console.log('WebSocket connected');
    this.isConnecting = false;
    this.reconnectAttempts = 0;

    // Send queued messages
    while (this.messageQueue.length > 0) {
      const message = this.messageQueue.shift();
      if (message) {
        this.send(message);
      }
    }

    // Start heartbeat
    this.startHeartbeat();

    // Notify listeners
    this.emit('connection', { status: 'connected' });
  }

  private handleMessage(event: MessageEvent): void {
    try {
      const message: WebSocketMessage = JSON.parse(event.data);

      // Handle system messages
      if (message.type === 'heartbeat') {
        this.send({ type: 'heartbeat_ack', payload: {}, timestamp: Date.now() });
        return;
      }

      // Emit to listeners
      this.emit(message.type, message.payload);

    } catch (error) {
      console.error('Failed to parse WebSocket message:', error);
    }
  }

  private handleClose(event: CloseEvent): void {
    console.log('WebSocket disconnected:', event.code, event.reason);
    this.isConnecting = false;
    this.stopHeartbeat();

    // Notify listeners
    this.emit('connection', { status: 'disconnected', code: event.code, reason: event.reason });

    // Attempt to reconnect if not a normal closure
    if (event.code !== 1000) {
      this.scheduleReconnect();
    }
  }

  private handleError(error: Event): void {
    console.error('WebSocket error:', error);
    this.emit('connection', { status: 'error', error });
  }

  private scheduleReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Max reconnection attempts reached');
      this.emit('connection', { status: 'failed', reason: 'Max reconnection attempts reached' });
      return;
    }

    const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts);
    console.log(`Reconnecting in ${delay}ms (attempt ${this.reconnectAttempts + 1})`);

    setTimeout(() => {
      this.reconnectAttempts++;
      this.connect();
    }, delay);
  }

  private startHeartbeat(): void {
    this.heartbeatInterval = setInterval(() => {
      if (this.ws && this.ws.readyState === WebSocket.OPEN) {
        this.send({ type: 'heartbeat', payload: {}, timestamp: Date.now() });
      }
    }, 30000); // 30 seconds
  }

  private stopHeartbeat(): void {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval);
      this.heartbeatInterval = null;
    }
  }

  public send(message: WebSocketMessage): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message));
    } else {
      // Queue message for when connection is restored
      this.messageQueue.push(message);

      // Attempt to reconnect if not already connecting
      if (!this.isConnecting) {
        this.connect();
      }
    }
  }

  public on(eventType: string, callback: (data: any) => void): () => void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, new Set());
    }

    this.listeners.get(eventType)!.add(callback);

    // Return unsubscribe function
    return () => {
      const listeners = this.listeners.get(eventType);
      if (listeners) {
        listeners.delete(callback);
        if (listeners.size === 0) {
          this.listeners.delete(eventType);
        }
      }
    };
  }

  private emit(eventType: string, data: any): void {
    const listeners = this.listeners.get(eventType);
    if (listeners) {
      listeners.forEach(callback => {
        try {
          callback(data);
        } catch (error) {
          console.error(`Error in WebSocket listener for ${eventType}:`, error);
        }
      });
    }
  }

  public disconnect(): void {
    this.stopHeartbeat();

    if (this.ws) {
      this.ws.close(1000, 'Client disconnect');
      this.ws = null;
    }

    this.messageQueue = [];
    this.listeners.clear();
  }

  public getConnectionState(): string {
    if (!this.ws) return 'disconnected';

    switch (this.ws.readyState) {
      case WebSocket.CONNECTING: return 'connecting';
      case WebSocket.OPEN: return 'connected';
      case WebSocket.CLOSING: return 'closing';
      case WebSocket.CLOSED: return 'disconnected';
      default: return 'unknown';
    }
  }

  // Collaborative features
  public joinRoom(roomId: string): void {
    this.send({
      type: 'join_room',
      payload: { roomId },
      timestamp: Date.now()
    });
  }

  public leaveRoom(roomId: string): void {
    this.send({
      type: 'leave_room',
      payload: { roomId },
      timestamp: Date.now()
    });
  }

  public broadcastCursorPosition(roomId: string, position: { x: number; y: number }): void {
    this.send({
      type: 'cursor_moved',
      payload: { roomId, position },
      timestamp: Date.now()
    });
  }

  public broadcastQueryExecution(roomId: string, query: string, results: any): void {
    this.send({
      type: 'query_executed',
      payload: { roomId, query, results },
      timestamp: Date.now()
    });
  }

  public broadcastDashboardUpdate(roomId: string, dashboardId: string, changes: any): void {
    this.send({
      type: 'dashboard_updated',
      payload: { roomId, dashboardId, changes },
      timestamp: Date.now()
    });
  }

  // Enhanced AI Features
  public startStreamingSession(config: any): void {
    this.send({
      type: 'start_streaming_session',
      payload: config,
      timestamp: Date.now()
    });
  }

  public processQueryStreamEvent(queryEvent: any): void {
    this.send({
      type: 'process_query_stream_event',
      payload: queryEvent,
      timestamp: Date.now()
    });
  }

  public processDataStreamEvent(dataEvent: any): void {
    this.send({
      type: 'process_data_stream_event',
      payload: dataEvent,
      timestamp: Date.now()
    });
  }

  public subscribeToDataStream(subscription: any): void {
    this.send({
      type: 'subscribe_to_data_stream',
      payload: subscription,
      timestamp: Date.now()
    });
  }

  public createRealTimeAlert(alert: any): void {
    this.send({
      type: 'create_real_time_alert',
      payload: alert,
      timestamp: Date.now()
    });
  }
}

// Singleton instance
export const websocketService = new WebSocketService();

// React hook for WebSocket integration
export const useWebSocket = () => {
  const [connectionState, setConnectionState] = React.useState(websocketService.getConnectionState());
  const [lastMessage, setLastMessage] = React.useState<any>(null);

  React.useEffect(() => {
    const unsubscribeConnection = websocketService.on('connection', (data) => {
      setConnectionState(data.status);
    });

    const unsubscribeMessage = websocketService.on('*', (data) => {
      setLastMessage({ type: '*', data, timestamp: Date.now() });
    });

    // Update initial state
    setConnectionState(websocketService.getConnectionState());

    return () => {
      unsubscribeConnection();
      unsubscribeMessage();
    };
  }, []);

  const sendMessage = React.useCallback((type: string, payload: any) => {
    websocketService.send({ type, payload, timestamp: Date.now() });
  }, []);

  const subscribe = React.useCallback((eventType: string, callback: (data: any) => void) => {
    return websocketService.on(eventType, callback);
  }, []);

  return {
    connectionState,
    lastMessage,
    sendMessage,
    subscribe,
    isConnected: connectionState === 'connected',
    joinRoom: websocketService.joinRoom.bind(websocketService),
    leaveRoom: websocketService.leaveRoom.bind(websocketService),
    broadcastCursorPosition: websocketService.broadcastCursorPosition.bind(websocketService),
    broadcastQueryExecution: websocketService.broadcastQueryExecution.bind(websocketService),
    broadcastDashboardUpdate: websocketService.broadcastDashboardUpdate.bind(websocketService),
    // Enhanced AI Features
    startStreamingSession: websocketService.startStreamingSession.bind(websocketService),
    processQueryStreamEvent: websocketService.processQueryStreamEvent.bind(websocketService),
    processDataStreamEvent: websocketService.processDataStreamEvent.bind(websocketService),
    subscribeToDataStream: websocketService.subscribeToDataStream.bind(websocketService),
    createRealTimeAlert: websocketService.createRealTimeAlert.bind(websocketService)
  };
};
