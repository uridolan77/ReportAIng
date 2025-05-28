import React, { useState, useEffect, useCallback } from 'react';
import { Card, Avatar, Badge, Tooltip, Button, Space, Typography, notification } from 'antd';
import { UserOutlined, EyeOutlined, EditOutlined } from '@ant-design/icons';
import { useWebSocket } from '../../services/websocketService';
import { useAuthStore } from '../../stores/authStore';
import { MemoizedDashboardGrid } from '../Performance/MemoizedComponents';

const { Text } = Typography;

interface CollaborativeUser {
  id: string;
  name: string;
  avatar?: string;
  cursor?: { x: number; y: number };
  isActive: boolean;
  lastSeen: number;
}

interface CollaborativeDashboardProps {
  dashboardId: string;
  widgets: any[];
  onWidgetUpdate?: (widgetId: string, changes: any) => void;
  readOnly?: boolean;
}

export const CollaborativeDashboard: React.FC<CollaborativeDashboardProps> = ({
  dashboardId,
  widgets,
  onWidgetUpdate,
  readOnly = false
}) => {
  const [collaborativeUsers, setCollaborativeUsers] = useState<Map<string, CollaborativeUser>>(new Map());
  const [isEditing, setIsEditing] = useState(false);
  const [lastActivity, setLastActivity] = useState<any>(null);
  
  const { user } = useAuthStore();
  const {
    connectionState,
    subscribe,
    joinRoom,
    leaveRoom,
    broadcastCursorPosition,
    broadcastDashboardUpdate
  } = useWebSocket();

  const roomId = `dashboard-${dashboardId}`;

  // Join/leave room on mount/unmount
  useEffect(() => {
    if (connectionState === 'connected') {
      joinRoom(roomId);
    }

    return () => {
      if (connectionState === 'connected') {
        leaveRoom(roomId);
      }
    };
  }, [connectionState, roomId, joinRoom, leaveRoom]);

  // Subscribe to collaborative events
  useEffect(() => {
    const unsubscribers: (() => void)[] = [];

    // User joined
    unsubscribers.push(subscribe('user_joined', (data) => {
      if (data.roomId === roomId && data.user.id !== user?.id) {
        setCollaborativeUsers(prev => {
          const updated = new Map(prev);
          updated.set(data.user.id, {
            ...data.user,
            isActive: true,
            lastSeen: Date.now()
          });
          return updated;
        });

        notification.info({
          message: 'User Joined',
          description: `${data.user.name} joined the dashboard`,
          duration: 3
        });
      }
    }));

    // User left
    unsubscribers.push(subscribe('user_left', (data) => {
      if (data.roomId === roomId) {
        setCollaborativeUsers(prev => {
          const updated = new Map(prev);
          const user = updated.get(data.user.id);
          if (user) {
            updated.set(data.user.id, { ...user, isActive: false });
          }
          return updated;
        });

        notification.info({
          message: 'User Left',
          description: `${data.user.name} left the dashboard`,
          duration: 3
        });
      }
    }));

    // Cursor movement
    unsubscribers.push(subscribe('cursor_moved', (data) => {
      if (data.roomId === roomId && data.user.id !== user?.id) {
        setCollaborativeUsers(prev => {
          const updated = new Map(prev);
          const existingUser = updated.get(data.user.id);
          if (existingUser) {
            updated.set(data.user.id, {
              ...existingUser,
              cursor: data.position,
              lastSeen: Date.now()
            });
          }
          return updated;
        });
      }
    }));

    // Dashboard updates
    unsubscribers.push(subscribe('dashboard_updated', (data) => {
      if (data.roomId === roomId && data.user.id !== user?.id) {
        setLastActivity({
          type: 'dashboard_update',
          user: data.user,
          changes: data.changes,
          timestamp: Date.now()
        });

        notification.success({
          message: 'Dashboard Updated',
          description: `${data.user.name} updated the dashboard`,
          duration: 3
        });

        // Apply changes if callback provided
        if (onWidgetUpdate && data.changes.widgetId) {
          onWidgetUpdate(data.changes.widgetId, data.changes);
        }
      }
    }));

    // Query execution
    unsubscribers.push(subscribe('query_executed', (data) => {
      if (data.roomId === roomId && data.user.id !== user?.id) {
        setLastActivity({
          type: 'query_execution',
          user: data.user,
          query: data.query,
          timestamp: Date.now()
        });

        notification.info({
          message: 'Query Executed',
          description: `${data.user.name} executed a query`,
          duration: 3
        });
      }
    }));

    return () => {
      unsubscribers.forEach(unsub => unsub());
    };
  }, [roomId, user?.id, subscribe, onWidgetUpdate]);

  // Handle mouse movement for cursor broadcasting
  const handleMouseMove = useCallback((event: React.MouseEvent) => {
    if (connectionState === 'connected' && !readOnly) {
      const rect = event.currentTarget.getBoundingClientRect();
      const position = {
        x: ((event.clientX - rect.left) / rect.width) * 100,
        y: ((event.clientY - rect.top) / rect.height) * 100
      };
      
      // Throttle cursor updates
      const now = Date.now();
      if (!handleMouseMove.lastUpdate || now - handleMouseMove.lastUpdate > 100) {
        broadcastCursorPosition(roomId, position);
        handleMouseMove.lastUpdate = now;
      }
    }
  }, [connectionState, readOnly, broadcastCursorPosition, roomId]);

  // Handle widget interactions
  const handleWidgetInteraction = useCallback((widgetId: string, data: any) => {
    if (!readOnly && isEditing) {
      const changes = { widgetId, data, timestamp: Date.now() };
      broadcastDashboardUpdate(roomId, dashboardId, changes);
      onWidgetUpdate?.(widgetId, changes);
    }
  }, [readOnly, isEditing, broadcastDashboardUpdate, roomId, dashboardId, onWidgetUpdate]);

  // Render collaborative cursors
  const renderCollaborativeCursors = () => {
    return Array.from(collaborativeUsers.values())
      .filter(user => user.isActive && user.cursor)
      .map(user => (
        <div
          key={user.id}
          style={{
            position: 'absolute',
            left: `${user.cursor!.x}%`,
            top: `${user.cursor!.y}%`,
            pointerEvents: 'none',
            zIndex: 1000,
            transform: 'translate(-50%, -50%)'
          }}
        >
          <div
            style={{
              width: '12px',
              height: '12px',
              borderRadius: '50%',
              backgroundColor: `hsl(${user.id.charCodeAt(0) * 137.5 % 360}, 70%, 50%)`,
              border: '2px solid white',
              boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
            }}
          />
          <div
            style={{
              marginTop: '4px',
              padding: '2px 6px',
              backgroundColor: 'rgba(0,0,0,0.8)',
              color: 'white',
              borderRadius: '4px',
              fontSize: '10px',
              whiteSpace: 'nowrap'
            }}
          >
            {user.name}
          </div>
        </div>
      ));
  };

  // Render user avatars
  const renderCollaborativeUsers = () => {
    const activeUsers = Array.from(collaborativeUsers.values()).filter(u => u.isActive);
    
    return (
      <Space>
        <Text type="secondary">
          <EyeOutlined /> {activeUsers.length} viewing
        </Text>
        <Avatar.Group maxCount={5} size="small">
          {activeUsers.map(user => (
            <Tooltip key={user.id} title={user.name}>
              <Avatar
                size="small"
                src={user.avatar}
                icon={<UserOutlined />}
                style={{
                  backgroundColor: `hsl(${user.id.charCodeAt(0) * 137.5 % 360}, 70%, 50%)`
                }}
              />
            </Tooltip>
          ))}
        </Avatar.Group>
      </Space>
    );
  };

  return (
    <div style={{ position: 'relative' }}>
      {/* Header with collaboration info */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <Badge
              status={connectionState === 'connected' ? 'success' : 'error'}
              text={connectionState === 'connected' ? 'Connected' : 'Disconnected'}
            />
          </div>
          
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            {renderCollaborativeUsers()}
            
            {!readOnly && (
              <Button
                type={isEditing ? 'primary' : 'default'}
                icon={<EditOutlined />}
                onClick={() => setIsEditing(!isEditing)}
                size="small"
              >
                {isEditing ? 'Stop Editing' : 'Edit'}
              </Button>
            )}
          </div>
        </div>
        
        {lastActivity && (
          <div style={{ marginTop: '8px', fontSize: '12px', color: '#666' }}>
            <Text type="secondary">
              Last activity: {lastActivity.user.name} {lastActivity.type.replace('_', ' ')} 
              {' '}({new Date(lastActivity.timestamp).toLocaleTimeString()})
            </Text>
          </div>
        )}
      </Card>

      {/* Dashboard content with cursor tracking */}
      <div
        style={{ position: 'relative' }}
        onMouseMove={handleMouseMove}
      >
        <MemoizedDashboardGrid
          widgets={widgets}
          onWidgetInteraction={handleWidgetInteraction}
        />
        
        {/* Collaborative cursors overlay */}
        {renderCollaborativeCursors()}
      </div>
    </div>
  );
};

// Hook for collaborative features
export const useCollaboration = (roomId: string) => {
  const [users, setUsers] = useState<CollaborativeUser[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  
  const { connectionState, subscribe, joinRoom, leaveRoom } = useWebSocket();

  useEffect(() => {
    setIsConnected(connectionState === 'connected');
    
    if (connectionState === 'connected') {
      joinRoom(roomId);
    }

    return () => {
      if (connectionState === 'connected') {
        leaveRoom(roomId);
      }
    };
  }, [connectionState, roomId, joinRoom, leaveRoom]);

  useEffect(() => {
    const unsubscribers = [
      subscribe('user_joined', (data) => {
        if (data.roomId === roomId) {
          setUsers(prev => [...prev.filter(u => u.id !== data.user.id), data.user]);
        }
      }),
      subscribe('user_left', (data) => {
        if (data.roomId === roomId) {
          setUsers(prev => prev.filter(u => u.id !== data.user.id));
        }
      })
    ];

    return () => unsubscribers.forEach(unsub => unsub());
  }, [roomId, subscribe]);

  return {
    users,
    isConnected,
    userCount: users.length
  };
};
