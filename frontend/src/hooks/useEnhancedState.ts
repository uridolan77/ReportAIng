import { useState, useEffect, useCallback, useRef } from 'react';
import { useCrossTabSync } from '../lib/cross-tab-sync';
import { persistenceManager, PersistenceConfig } from '../lib/enhanced-persistence';

// Enhanced state hook with cross-tab sync and persistence
export const useEnhancedState = <T>(
  key: string,
  initialValue: T,
  options: {
    persistence?: PersistenceConfig;
    crossTabSync?: boolean;
    debounceMs?: number;
    onConflict?: (localValue: T, remoteValue: T) => T;
  } = {}
) => {
  const {
    persistence,
    crossTabSync = true,
    debounceMs = 300,
    onConflict
  } = options;

  const [state, setState] = useState<T>(initialValue);
  const [isLoading, setIsLoading] = useState(!!persistence);
  const [error, setError] = useState<string | null>(null);
  const [lastSyncTimestamp, setLastSyncTimestamp] = useState<number>(0);
  
  const { subscribe, broadcast } = useCrossTabSync();
  const debounceTimerRef = useRef<NodeJS.Timeout>();
  const isInitializedRef = useRef(false);

  // Load persisted state on mount
  useEffect(() => {
    if (persistence && !isInitializedRef.current) {
      loadPersistedState();
    } else {
      setIsLoading(false);
      isInitializedRef.current = true;
    }
  }, []);

  // Setup cross-tab sync
  useEffect(() => {
    if (!crossTabSync || !isInitializedRef.current) return;

    const unsubscribe = subscribe(`state_update_${key}`, (data) => {
      const { value, timestamp, tabId } = data;
      
      // Avoid infinite loops
      if (timestamp <= lastSyncTimestamp) return;
      
      // Handle conflicts
      if (onConflict && timestamp > lastSyncTimestamp) {
        const resolvedValue = onConflict(state, value);
        setState(resolvedValue);
      } else {
        setState(value);
      }
      
      setLastSyncTimestamp(timestamp);
    });

    return unsubscribe;
  }, [key, crossTabSync, state, lastSyncTimestamp, onConflict, subscribe]);

  const loadPersistedState = async () => {
    if (!persistence) return;

    try {
      setIsLoading(true);
      setError(null);
      
      const persistedValue = await persistenceManager.loadState(persistence);
      
      if (persistedValue !== null) {
        setState(persistedValue);
        setLastSyncTimestamp(Date.now());
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load persisted state');
      console.error('Failed to load persisted state:', err);
    } finally {
      setIsLoading(false);
      isInitializedRef.current = true;
    }
  };

  const savePersistedState = async (value: T) => {
    if (!persistence) return;

    try {
      await persistenceManager.saveState(value, persistence);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save state');
      console.error('Failed to save persisted state:', err);
    }
  };

  const debouncedSave = useCallback((value: T) => {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = setTimeout(() => {
      savePersistedState(value);
    }, debounceMs);
  }, [debounceMs]);

  const updateState = useCallback((newValue: T | ((prev: T) => T)) => {
    setState(prevState => {
      const nextState = typeof newValue === 'function' 
        ? (newValue as (prev: T) => T)(prevState)
        : newValue;

      const timestamp = Date.now();
      setLastSyncTimestamp(timestamp);

      // Broadcast to other tabs
      if (crossTabSync && isInitializedRef.current) {
        broadcast(`state_update_${key}`, {
          value: nextState,
          timestamp,
          tabId: 'current'
        });
      }

      // Save to persistence
      if (persistence && isInitializedRef.current) {
        debouncedSave(nextState);
      }

      return nextState;
    });
  }, [key, crossTabSync, persistence, debouncedSave, broadcast]);

  const clearState = useCallback(() => {
    setState(initialValue);
    
    if (persistence) {
      persistenceManager.removeState(persistence);
    }
    
    if (crossTabSync) {
      broadcast(`state_clear_${key}`, { timestamp: Date.now() });
    }
  }, [key, initialValue, persistence, crossTabSync, broadcast]);

  const forceSync = useCallback(() => {
    if (crossTabSync) {
      broadcast(`state_request_${key}`, { timestamp: Date.now() });
    }
  }, [key, crossTabSync, broadcast]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, []);

  return {
    state,
    setState: updateState,
    isLoading,
    error,
    clearState,
    forceSync,
    lastSyncTimestamp
  };
};

// Hook for managing user preferences with enhanced features
export const useUserPreferences = <T extends Record<string, any>>(
  defaultPreferences: T
) => {
  const { state, setState, isLoading, error } = useEnhancedState(
    'user-preferences',
    defaultPreferences,
    {
      persistence: {
        key: 'bi-reporting-user-preferences',
        version: 1,
        storage: 'localStorage',
        compression: false,
        encryption: false,
        maxAge: 30 * 24 * 60 * 60 * 1000, // 30 days
        maxSize: 100 * 1024 // 100KB
      },
      crossTabSync: true,
      debounceMs: 500
    }
  );

  const updatePreference = useCallback(<K extends keyof T>(
    key: K,
    value: T[K]
  ) => {
    setState(prev => ({ ...prev, [key]: value }));
  }, [setState]);

  const resetPreferences = useCallback(() => {
    setState(defaultPreferences);
  }, [setState, defaultPreferences]);

  return {
    preferences: state,
    updatePreference,
    resetPreferences,
    isLoading,
    error
  };
};

// Hook for managing session state
export const useSessionState = <T>(key: string, initialValue: T) => {
  return useEnhancedState(key, initialValue, {
    persistence: {
      key: `bi-reporting-session-${key}`,
      version: 1,
      storage: 'sessionStorage',
      compression: false,
      encryption: false,
      maxAge: 24 * 60 * 60 * 1000, // 24 hours
      maxSize: 500 * 1024 // 500KB
    },
    crossTabSync: false, // Session state is tab-specific
    debounceMs: 100
  });
};

// Hook for managing query history with enhanced features
export const useQueryHistoryState = () => {
  const { state, setState, isLoading, error } = useEnhancedState(
    'query-history',
    [] as any[],
    {
      persistence: {
        key: 'bi-reporting-query-history',
        version: 1,
        storage: 'localStorage',
        compression: true,
        encryption: false,
        maxAge: 7 * 24 * 60 * 60 * 1000, // 7 days
        maxSize: 1024 * 1024 // 1MB
      },
      crossTabSync: true,
      debounceMs: 1000,
      onConflict: (local, remote) => {
        // Merge query histories by timestamp, keeping unique queries
        const merged = [...local, ...remote];
        const unique = merged.filter((query, index, arr) => 
          arr.findIndex(q => q.id === query.id) === index
        );
        return unique.sort((a, b) => 
          new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
        ).slice(0, 100); // Keep only last 100 queries
      }
    }
  );

  const addQuery = useCallback((query: any) => {
    setState(prev => {
      const newHistory = [query, ...prev.filter(q => q.id !== query.id)];
      return newHistory.slice(0, 100); // Limit to 100 queries
    });
  }, [setState]);

  const removeQuery = useCallback((queryId: string) => {
    setState(prev => prev.filter(q => q.id !== queryId));
  }, [setState]);

  const clearHistory = useCallback(() => {
    setState([]);
  }, [setState]);

  return {
    history: state,
    addQuery,
    removeQuery,
    clearHistory,
    isLoading,
    error
  };
};

// Hook for managing application state with conflict resolution
export const useApplicationState = () => {
  const { state, setState, isLoading, error } = useEnhancedState(
    'app-state',
    {
      theme: 'light',
      sidebarCollapsed: false,
      activeTab: 'result',
      showInsightsPanel: true,
      lastActiveTime: Date.now()
    },
    {
      persistence: {
        key: 'bi-reporting-app-state',
        version: 1,
        storage: 'localStorage',
        compression: false,
        encryption: false,
        maxAge: 7 * 24 * 60 * 60 * 1000, // 7 days
        maxSize: 50 * 1024 // 50KB
      },
      crossTabSync: true,
      debounceMs: 200,
      onConflict: (local, remote) => {
        // Use the most recently active state
        return local.lastActiveTime > remote.lastActiveTime ? local : remote;
      }
    }
  );

  const updateAppState = useCallback((updates: Partial<typeof state>) => {
    setState(prev => ({
      ...prev,
      ...updates,
      lastActiveTime: Date.now()
    }));
  }, [setState]);

  return {
    appState: state,
    updateAppState,
    isLoading,
    error
  };
};
