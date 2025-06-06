import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { crossTabSync } from '../../lib/cross-tab-sync';
import { persistenceManager, persistenceConfigs } from '../../lib/enhanced-persistence';
import { useQueryClient } from '@tanstack/react-query';

// Types for state sync context
interface StateSyncContextType {
  isOnline: boolean;
  tabId: string;
  connectedTabs: string[];
  syncStatus: 'idle' | 'syncing' | 'error';
  lastSyncTime: number;
  
  // Methods
  broadcastToTabs: (type: string, payload: any) => void;
  invalidateQueryAcrossTabs: (queryKey: any[]) => void;
  clearCacheAcrossTabs: (pattern?: string) => void;
  getStorageStats: () => { localStorage: any; sessionStorage: any };
  cleanupExpiredData: () => Promise<number>;
  exportState: () => Promise<string>;
  importState: (data: string) => Promise<void>;
}

const StateSyncContext = createContext<StateSyncContextType | undefined>(undefined);

export const useStateSync = () => {
  const context = useContext(StateSyncContext);
  if (!context) {
    throw new Error('useStateSync must be used within a StateSyncProvider');
  }
  return context;
};

interface StateSyncProviderProps {
  children: React.ReactNode;
}

export const StateSyncProvider: React.FC<StateSyncProviderProps> = ({ children }) => {
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [connectedTabs, setConnectedTabs] = useState<string[]>([]);
  const [syncStatus, setSyncStatus] = useState<'idle' | 'syncing' | 'error'>('idle');
  const [lastSyncTime, setLastSyncTime] = useState(Date.now());
  
  const queryClient = useQueryClient();
  const tabId = crossTabSync.getTabId();

  // Setup online/offline detection
  useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  // Setup tab discovery and management
  useEffect(() => {
    // Announce this tab to others
    crossTabSync.broadcast('tab_announce', { tabId, timestamp: Date.now() });

    // Listen for tab announcements
    const unsubscribeAnnounce = crossTabSync.subscribe('tab_announce', (data) => {
      const { tabId: remoteTabId } = data;
      setConnectedTabs(prev => {
        if (!prev.includes(remoteTabId)) {
          return [...prev, remoteTabId];
        }
        return prev;
      });
    });

    // Listen for tab closures
    const unsubscribeClosing = crossTabSync.subscribe('tab_closing', (data) => {
      const { tabId: closingTabId } = data;
      setConnectedTabs(prev => prev.filter(id => id !== closingTabId));
    });

    // Periodic tab discovery
    const discoveryInterval = setInterval(() => {
      crossTabSync.broadcast('tab_ping', { tabId, timestamp: Date.now() });
    }, 30000); // Every 30 seconds

    // Listen for pings and respond
    const unsubscribePing = crossTabSync.subscribe('tab_ping', (data) => {
      const { tabId: remoteTabId } = data;
      crossTabSync.broadcast('tab_pong', { tabId, respondingTo: remoteTabId });
    });

    // Listen for pong responses
    const unsubscribePong = crossTabSync.subscribe('tab_pong', (data) => {
      const { tabId: remoteTabId } = data;
      setConnectedTabs(prev => {
        if (!prev.includes(remoteTabId)) {
          return [...prev, remoteTabId];
        }
        return prev;
      });
    });

    return () => {
      clearInterval(discoveryInterval);
      unsubscribeAnnounce();
      unsubscribeClosing();
      unsubscribePing();
      unsubscribePong();
    };
  }, [tabId]);

  // Setup React Query sync listeners
  useEffect(() => {
    // Listen for query invalidations from other tabs
    const unsubscribeInvalidate = crossTabSync.subscribe('query_invalidate_broadcast', (data) => {
      setSyncStatus('syncing');
      const { queryKey, options } = data;
      queryClient.invalidateQueries({ queryKey, ...options });
      setLastSyncTime(Date.now());
      setSyncStatus('idle');
    });

    // Listen for cache clear requests
    const unsubscribeClear = crossTabSync.subscribe('cache_clear_broadcast', (data) => {
      setSyncStatus('syncing');
      const { pattern } = data;
      if (pattern) {
        queryClient.invalidateQueries({
          predicate: (query) => query.queryKey.some(key => 
            typeof key === 'string' && key.includes(pattern)
          )
        });
      } else {
        queryClient.clear();
      }
      setLastSyncTime(Date.now());
      setSyncStatus('idle');
    });

    return () => {
      unsubscribeInvalidate();
      unsubscribeClear();
    };
  }, [queryClient]);

  // Setup periodic cleanup
  useEffect(() => {
    const cleanupInterval = setInterval(async () => {
      try {
        await cleanupExpiredData();
      } catch (error) {
        console.error('Periodic cleanup failed:', error);
      }
    }, 60 * 60 * 1000); // Every hour

    return () => clearInterval(cleanupInterval);
  }, [cleanupExpiredData]);

  // Methods
  const broadcastToTabs = useCallback((type: string, payload: any) => {
    crossTabSync.broadcast(type, payload);
  }, []);

  const invalidateQueryAcrossTabs = useCallback((queryKey: any[]) => {
    // Invalidate locally
    queryClient.invalidateQueries({ queryKey });
    
    // Broadcast to other tabs
    crossTabSync.broadcast('query_invalidate_broadcast', { queryKey });
    
    setLastSyncTime(Date.now());
  }, [queryClient]);

  const clearCacheAcrossTabs = useCallback((pattern?: string) => {
    // Clear locally
    if (pattern) {
      queryClient.invalidateQueries({
        predicate: (query) => query.queryKey.some(key => 
          typeof key === 'string' && key.includes(pattern)
        )
      });
    } else {
      queryClient.clear();
    }
    
    // Broadcast to other tabs
    crossTabSync.broadcast('cache_clear_broadcast', { pattern });
    
    setLastSyncTime(Date.now());
  }, [queryClient]);

  const cleanupExpiredData = useCallback(async (): Promise<number> => {
    let totalCleaned = 0;

    // Clean up localStorage
    totalCleaned += persistenceManager.cleanupExpiredStates(
      'localStorage',
      7 * 24 * 60 * 60 * 1000 // 7 days
    );

    // Clean up sessionStorage
    totalCleaned += persistenceManager.cleanupExpiredStates(
      'sessionStorage',
      24 * 60 * 60 * 1000 // 24 hours
    );

    console.log(`Cleaned up ${totalCleaned} expired state entries`);
    return totalCleaned;
  }, []);

  const getStorageStats = useCallback(() => {
    return {
      localStorage: persistenceManager.getStorageStats('localStorage'),
      sessionStorage: persistenceManager.getStorageStats('sessionStorage')
    };
  }, []);

  const exportState = useCallback(async (): Promise<string> => {
    const exportData = {
      timestamp: Date.now(),
      version: 1,
      data: {
        queryHistory: await persistenceManager.loadState(persistenceConfigs.queryHistory),
        userPreferences: await persistenceManager.loadState(persistenceConfigs.userPreferences),
        sessionState: await persistenceManager.loadState(persistenceConfigs.sessionState)
      }
    };
    
    return JSON.stringify(exportData, null, 2);
  }, []);

  const importState = useCallback(async (data: string): Promise<void> => {
    try {
      const importData = JSON.parse(data);
      
      if (importData.version !== 1) {
        throw new Error('Unsupported export version');
      }
      
      // Import each data type
      if (importData.data.queryHistory) {
        await persistenceManager.saveState(
          importData.data.queryHistory, 
          persistenceConfigs.queryHistory
        );
      }
      
      if (importData.data.userPreferences) {
        await persistenceManager.saveState(
          importData.data.userPreferences, 
          persistenceConfigs.userPreferences
        );
      }
      
      if (importData.data.sessionState) {
        await persistenceManager.saveState(
          importData.data.sessionState, 
          persistenceConfigs.sessionState
        );
      }
      
      // Broadcast state update to other tabs
      crossTabSync.broadcast('state_imported', { timestamp: Date.now() });
      
      // Refresh the page to load new state
      window.location.reload();
    } catch (error) {
      console.error('Failed to import state:', error);
      throw error;
    }
  }, []);

  // Setup migration handlers
  useEffect(() => {
    // Register migration functions for different versions
    persistenceManager.registerMigration(0, (oldData) => {
      // Migration from version 0 to 1
      console.log('Migrating data from version 0 to 1');
      return {
        ...oldData,
        version: 1,
        migrated: true
      };
    });
  }, []);

  const contextValue: StateSyncContextType = {
    isOnline,
    tabId,
    connectedTabs,
    syncStatus,
    lastSyncTime,
    broadcastToTabs,
    invalidateQueryAcrossTabs,
    clearCacheAcrossTabs,
    getStorageStats,
    cleanupExpiredData,
    exportState,
    importState
  };

  return (
    <StateSyncContext.Provider value={contextValue}>
      {children}
    </StateSyncContext.Provider>
  );
};

// Hook for monitoring sync status
export const useSyncStatus = () => {
  const { syncStatus, lastSyncTime, connectedTabs, isOnline } = useStateSync();
  
  return {
    isSyncing: syncStatus === 'syncing',
    hasError: syncStatus === 'error',
    lastSyncTime,
    connectedTabsCount: connectedTabs.length,
    isOnline,
    isConnected: isOnline && connectedTabs.length > 0
  };
};

// Hook for storage management
export const useStorageManagement = () => {
  const { getStorageStats, cleanupExpiredData, exportState, importState } = useStateSync();
  
  const [stats, setStats] = useState<any>(null);
  const [isCleaningUp, setIsCleaningUp] = useState(false);
  
  const refreshStats = useCallback(async () => {
    const newStats = getStorageStats();
    setStats(newStats);
  }, [getStorageStats]);
  
  const performCleanup = useCallback(async () => {
    setIsCleaningUp(true);
    try {
      const cleaned = await cleanupExpiredData();
      await refreshStats();
      return cleaned;
    } finally {
      setIsCleaningUp(false);
    }
  }, [cleanupExpiredData, refreshStats]);
  
  useEffect(() => {
    refreshStats();
  }, [refreshStats]);
  
  return {
    stats,
    isCleaningUp,
    refreshStats,
    performCleanup,
    exportState,
    importState
  };
};
