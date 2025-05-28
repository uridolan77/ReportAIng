// Cross-tab state synchronization system
import { queryClient } from './react-query';

// Types for cross-tab communication
export interface TabSyncMessage {
  type: string;
  payload: any;
  timestamp: number;
  tabId: string;
  version: number;
}

export interface TabSyncConfig {
  channel: string;
  debounceMs: number;
  maxRetries: number;
  conflictResolution: 'timestamp' | 'manual' | 'merge';
}

// Cross-tab synchronization manager
export class CrossTabSyncManager {
  private static instance: CrossTabSyncManager;
  private channel: BroadcastChannel;
  private tabId: string;
  private listeners: Map<string, Set<(data: any) => void>>;
  private debounceTimers: Map<string, NodeJS.Timeout>;
  private config: TabSyncConfig;
  private version: number = 1;

  private constructor(config: TabSyncConfig) {
    this.config = config;
    this.tabId = this.generateTabId();
    this.channel = new BroadcastChannel(config.channel);
    this.listeners = new Map();
    this.debounceTimers = new Map();
    
    this.setupMessageHandler();
    this.setupUnloadHandler();
    
    console.log(`CrossTabSync initialized for tab: ${this.tabId}`);
  }

  static getInstance(config?: TabSyncConfig): CrossTabSyncManager {
    if (!CrossTabSyncManager.instance) {
      const defaultConfig: TabSyncConfig = {
        channel: 'bi-reporting-sync',
        debounceMs: 100,
        maxRetries: 3,
        conflictResolution: 'timestamp'
      };
      CrossTabSyncManager.instance = new CrossTabSyncManager(config || defaultConfig);
    }
    return CrossTabSyncManager.instance;
  }

  private generateTabId(): string {
    return `tab_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private setupMessageHandler(): void {
    this.channel.addEventListener('message', (event) => {
      const message: TabSyncMessage = event.data;
      
      // Ignore messages from the same tab
      if (message.tabId === this.tabId) return;
      
      // Handle version conflicts
      if (message.version > this.version) {
        this.handleVersionConflict(message);
      }
      
      this.handleIncomingMessage(message);
    });
  }

  private setupUnloadHandler(): void {
    window.addEventListener('beforeunload', () => {
      this.broadcast('tab_closing', { tabId: this.tabId });
      this.channel.close();
    });
  }

  private handleVersionConflict(message: TabSyncMessage): void {
    console.warn(`Version conflict detected. Local: ${this.version}, Remote: ${message.version}`);
    
    switch (this.config.conflictResolution) {
      case 'timestamp':
        // Accept newer version
        this.version = message.version;
        break;
      case 'manual':
        // Emit conflict event for manual resolution
        this.emit('version_conflict', { local: this.version, remote: message.version });
        break;
      case 'merge':
        // Attempt to merge versions
        this.version = Math.max(this.version, message.version) + 1;
        break;
    }
  }

  private handleIncomingMessage(message: TabSyncMessage): void {
    const listeners = this.listeners.get(message.type);
    if (listeners) {
      listeners.forEach(listener => {
        try {
          listener(message.payload);
        } catch (error) {
          console.error(`Error in cross-tab listener for ${message.type}:`, error);
        }
      });
    }
  }

  // Public API
  broadcast(type: string, payload: any): void {
    const message: TabSyncMessage = {
      type,
      payload,
      timestamp: Date.now(),
      tabId: this.tabId,
      version: this.version
    };

    // Debounce rapid broadcasts
    const key = `${type}_${JSON.stringify(payload)}`;
    if (this.debounceTimers.has(key)) {
      clearTimeout(this.debounceTimers.get(key)!);
    }

    const timer = setTimeout(() => {
      this.channel.postMessage(message);
      this.debounceTimers.delete(key);
    }, this.config.debounceMs);

    this.debounceTimers.set(key, timer);
  }

  subscribe(type: string, listener: (data: any) => void): () => void {
    if (!this.listeners.has(type)) {
      this.listeners.set(type, new Set());
    }
    
    this.listeners.get(type)!.add(listener);
    
    // Return unsubscribe function
    return () => {
      const listeners = this.listeners.get(type);
      if (listeners) {
        listeners.delete(listener);
        if (listeners.size === 0) {
          this.listeners.delete(type);
        }
      }
    };
  }

  emit(type: string, data: any): void {
    const listeners = this.listeners.get(type);
    if (listeners) {
      listeners.forEach(listener => listener(data));
    }
  }

  getTabId(): string {
    return this.tabId;
  }

  getVersion(): number {
    return this.version;
  }

  incrementVersion(): void {
    this.version++;
  }

  destroy(): void {
    this.debounceTimers.forEach(timer => clearTimeout(timer));
    this.debounceTimers.clear();
    this.listeners.clear();
    this.channel.close();
  }
}

// React Query integration for cross-tab sync
export class ReactQueryTabSync {
  private syncManager: CrossTabSyncManager;
  private syncedQueries: Set<string>;

  constructor(syncManager: CrossTabSyncManager) {
    this.syncManager = syncManager;
    this.syncedQueries = new Set();
    this.setupQuerySync();
  }

  private setupQuerySync(): void {
    // Listen for query invalidations from other tabs
    this.syncManager.subscribe('query_invalidate', (data) => {
      const { queryKey, options } = data;
      queryClient.invalidateQueries({ queryKey, ...options });
    });

    // Listen for query data updates from other tabs
    this.syncManager.subscribe('query_update', (data) => {
      const { queryKey, queryData } = data;
      queryClient.setQueryData(queryKey, queryData);
    });

    // Listen for cache clearing from other tabs
    this.syncManager.subscribe('cache_clear', (data) => {
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
    });

    // Listen for mutations from other tabs
    this.syncManager.subscribe('mutation_success', (data) => {
      const { mutationKey, variables, result } = data;
      // Handle optimistic updates or cache invalidations based on mutation
      this.handleMutationSync(mutationKey, variables, result);
    });
  }

  private handleMutationSync(mutationKey: string[], variables: any, result: any): void {
    // Define mutation-specific sync logic
    switch (mutationKey[0]) {
      case 'executeQuery':
        // Invalidate query history when new query is executed
        queryClient.invalidateQueries({ queryKey: ['queries', 'history'] });
        break;
      case 'addToFavorites':
        // Invalidate favorites list
        queryClient.invalidateQueries({ queryKey: ['favorites'] });
        break;
      case 'updateBusinessTable':
        // Invalidate tuning data
        queryClient.invalidateQueries({ queryKey: ['tuning'] });
        break;
      case 'clearCache':
        // Clear all caches
        queryClient.clear();
        break;
    }
  }

  // Public API for manual sync operations
  invalidateQuery(queryKey: any[], options?: any): void {
    this.syncManager.broadcast('query_invalidate', { queryKey, options });
    queryClient.invalidateQueries({ queryKey, ...options });
  }

  updateQueryData(queryKey: any[], queryData: any): void {
    this.syncManager.broadcast('query_update', { queryKey, queryData });
    queryClient.setQueryData(queryKey, queryData);
  }

  clearCache(pattern?: string): void {
    this.syncManager.broadcast('cache_clear', { pattern });
    if (pattern) {
      queryClient.invalidateQueries({
        predicate: (query) => query.queryKey.some(key => 
          typeof key === 'string' && key.includes(pattern)
        )
      });
    } else {
      queryClient.clear();
    }
  }

  broadcastMutation(mutationKey: string[], variables: any, result: any): void {
    this.syncManager.broadcast('mutation_success', { mutationKey, variables, result });
  }

  enableQuerySync(queryKey: string): void {
    this.syncedQueries.add(queryKey);
  }

  disableQuerySync(queryKey: string): void {
    this.syncedQueries.delete(queryKey);
  }

  isSynced(queryKey: string): boolean {
    return this.syncedQueries.has(queryKey);
  }
}

// Global instances
export const crossTabSync = CrossTabSyncManager.getInstance();
export const reactQueryTabSync = new ReactQueryTabSync(crossTabSync);

// React hooks for cross-tab sync
export const useCrossTabSync = () => {
  const subscribe = (type: string, listener: (data: any) => void) => {
    return crossTabSync.subscribe(type, listener);
  };

  const broadcast = (type: string, payload: any) => {
    crossTabSync.broadcast(type, payload);
  };

  const getTabId = () => crossTabSync.getTabId();
  const getVersion = () => crossTabSync.getVersion();

  return {
    subscribe,
    broadcast,
    getTabId,
    getVersion,
    syncManager: crossTabSync
  };
};

// React hook for React Query cross-tab sync
export const useReactQueryTabSync = () => {
  const invalidateQuery = (queryKey: any[], options?: any) => {
    reactQueryTabSync.invalidateQuery(queryKey, options);
  };

  const updateQueryData = (queryKey: any[], queryData: any) => {
    reactQueryTabSync.updateQueryData(queryKey, queryData);
  };

  const clearCache = (pattern?: string) => {
    reactQueryTabSync.clearCache(pattern);
  };

  const broadcastMutation = (mutationKey: string[], variables: any, result: any) => {
    reactQueryTabSync.broadcastMutation(mutationKey, variables, result);
  };

  return {
    invalidateQuery,
    updateQueryData,
    clearCache,
    broadcastMutation,
    tabSync: reactQueryTabSync
  };
};
