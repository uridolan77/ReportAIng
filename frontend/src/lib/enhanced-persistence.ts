// Enhanced state persistence with versioning and migration
import { queryClient } from './react-query';

// Types for persistence system
export interface PersistenceConfig {
  key: string;
  version: number;
  storage: 'localStorage' | 'sessionStorage' | 'indexedDB';
  compression: boolean;
  encryption: boolean;
  maxAge: number; // milliseconds
  maxSize: number; // bytes
}

export interface PersistedState {
  data: any;
  timestamp: number;
  version: number;
  checksum?: string;
}

export interface MigrationFunction {
  (oldData: any, oldVersion: number): any;
}

// Enhanced persistence manager
export class EnhancedPersistenceManager {
  private static instance: EnhancedPersistenceManager;
  private migrations: Map<number, MigrationFunction>;
  private compressionWorker?: Worker;

  private constructor() {
    this.migrations = new Map();
    this.setupCompressionWorker();
  }

  static getInstance(): EnhancedPersistenceManager {
    if (!EnhancedPersistenceManager.instance) {
      EnhancedPersistenceManager.instance = new EnhancedPersistenceManager();
    }
    return EnhancedPersistenceManager.instance;
  }

  private setupCompressionWorker(): void {
    // Setup web worker for compression (if available)
    if (typeof Worker !== 'undefined') {
      try {
        // Create inline worker for compression
        const workerScript = `
          self.onmessage = function(e) {
            const { action, data } = e.data;
            try {
              if (action === 'compress') {
                // Simple compression using JSON stringify optimization
                const compressed = JSON.stringify(data);
                self.postMessage({ success: true, result: compressed });
              } else if (action === 'decompress') {
                const decompressed = JSON.parse(data);
                self.postMessage({ success: true, result: decompressed });
              }
            } catch (error) {
              self.postMessage({ success: false, error: error.message });
            }
          };
        `;
        
        const blob = new Blob([workerScript], { type: 'application/javascript' });
        this.compressionWorker = new Worker(URL.createObjectURL(blob));
      } catch (error) {
        console.warn('Compression worker not available:', error);
      }
    }
  }

  // Register migration function for version upgrade
  registerMigration(fromVersion: number, migrationFn: MigrationFunction): void {
    this.migrations.set(fromVersion, migrationFn);
  }

  // Calculate checksum for data integrity
  private calculateChecksum(data: string): string {
    let hash = 0;
    for (let i = 0; i < data.length; i++) {
      const char = data.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return hash.toString(36);
  }

  // Compress data if compression is enabled
  private async compressData(data: any, useCompression: boolean): Promise<string> {
    const jsonString = JSON.stringify(data);
    
    if (!useCompression || !this.compressionWorker) {
      return jsonString;
    }

    return new Promise((resolve, reject) => {
      const timeout = setTimeout(() => {
        reject(new Error('Compression timeout'));
      }, 5000);

      this.compressionWorker!.onmessage = (e) => {
        clearTimeout(timeout);
        const { success, result, error } = e.data;
        if (success) {
          resolve(result);
        } else {
          reject(new Error(error));
        }
      };

      this.compressionWorker!.postMessage({ action: 'compress', data });
    });
  }

  // Decompress data if compression was used
  private async decompressData(data: string, useCompression: boolean): Promise<any> {
    if (!useCompression || !this.compressionWorker) {
      return JSON.parse(data);
    }

    return new Promise((resolve, reject) => {
      const timeout = setTimeout(() => {
        reject(new Error('Decompression timeout'));
      }, 5000);

      this.compressionWorker!.onmessage = (e) => {
        clearTimeout(timeout);
        const { success, result, error } = e.data;
        if (success) {
          resolve(result);
        } else {
          reject(new Error(error));
        }
      };

      this.compressionWorker!.postMessage({ action: 'decompress', data });
    });
  }

  // Get storage interface based on config
  private getStorage(storageType: 'localStorage' | 'sessionStorage' | 'indexedDB'): Storage {
    switch (storageType) {
      case 'localStorage':
        return localStorage;
      case 'sessionStorage':
        return sessionStorage;
      case 'indexedDB':
        // For now, fallback to localStorage for IndexedDB
        // In production, implement proper IndexedDB wrapper
        console.warn('IndexedDB not implemented, falling back to localStorage');
        return localStorage;
      default:
        return localStorage;
    }
  }

  // Save state with all enhancements
  async saveState(data: any, config: PersistenceConfig): Promise<void> {
    try {
      // Check data size before processing
      const estimatedSize = JSON.stringify(data).length * 2; // Rough estimate
      if (estimatedSize > config.maxSize) {
        throw new Error(`Data size (${estimatedSize}) exceeds maximum (${config.maxSize})`);
      }

      // Compress data if enabled
      const compressedData = await this.compressData(data, config.compression);
      
      // Calculate checksum for integrity
      const checksum = this.calculateChecksum(compressedData);

      // Create persisted state object
      const persistedState: PersistedState = {
        data: config.compression ? compressedData : data,
        timestamp: Date.now(),
        version: config.version,
        checksum: config.encryption ? undefined : checksum // Skip checksum if encrypting
      };

      // Encrypt if enabled (simple XOR for demo - use proper encryption in production)
      let finalData = JSON.stringify(persistedState);
      if (config.encryption) {
        finalData = this.simpleEncrypt(finalData, config.key);
      }

      // Save to storage
      const storage = this.getStorage(config.storage);
      storage.setItem(config.key, finalData);

      console.log(`State saved to ${config.storage} with key: ${config.key}`);
    } catch (error) {
      console.error('Failed to save state:', error);
      throw error;
    }
  }

  // Load state with migration support
  async loadState(config: PersistenceConfig): Promise<any | null> {
    try {
      const storage = this.getStorage(config.storage);
      const rawData = storage.getItem(config.key);
      
      if (!rawData) {
        return null;
      }

      // Decrypt if needed
      let decryptedData = rawData;
      if (config.encryption) {
        decryptedData = this.simpleDecrypt(rawData, config.key);
      }

      const persistedState: PersistedState = JSON.parse(decryptedData);

      // Check age
      const age = Date.now() - persistedState.timestamp;
      if (age > config.maxAge) {
        console.warn(`Persisted state expired (age: ${age}ms, max: ${config.maxAge}ms)`);
        this.removeState(config);
        return null;
      }

      // Verify checksum if available
      if (persistedState.checksum && !config.encryption) {
        const dataString = typeof persistedState.data === 'string' 
          ? persistedState.data 
          : JSON.stringify(persistedState.data);
        const calculatedChecksum = this.calculateChecksum(dataString);
        
        if (calculatedChecksum !== persistedState.checksum) {
          console.warn('Checksum mismatch, data may be corrupted');
          this.removeState(config);
          return null;
        }
      }

      // Decompress if needed
      let data = persistedState.data;
      if (config.compression && typeof data === 'string') {
        data = await this.decompressData(data, true);
      }

      // Handle version migration
      if (persistedState.version < config.version) {
        data = await this.migrateData(data, persistedState.version, config.version);
      }

      return data;
    } catch (error) {
      console.error('Failed to load state:', error);
      return null;
    }
  }

  // Migrate data through version chain
  private async migrateData(data: any, fromVersion: number, toVersion: number): Promise<any> {
    let currentData = data;
    let currentVersion = fromVersion;

    while (currentVersion < toVersion) {
      const migration = this.migrations.get(currentVersion);
      if (migration) {
        console.log(`Migrating data from version ${currentVersion} to ${currentVersion + 1}`);
        currentData = await migration(currentData, currentVersion);
        currentVersion++;
      } else {
        console.warn(`No migration found for version ${currentVersion}, skipping to ${toVersion}`);
        break;
      }
    }

    return currentData;
  }

  // Simple encryption (XOR) - use proper encryption in production
  private simpleEncrypt(data: string, key: string): string {
    let result = '';
    for (let i = 0; i < data.length; i++) {
      result += String.fromCharCode(data.charCodeAt(i) ^ key.charCodeAt(i % key.length));
    }
    return btoa(result);
  }

  // Simple decryption (XOR)
  private simpleDecrypt(encryptedData: string, key: string): string {
    const data = atob(encryptedData);
    let result = '';
    for (let i = 0; i < data.length; i++) {
      result += String.fromCharCode(data.charCodeAt(i) ^ key.charCodeAt(i % key.length));
    }
    return result;
  }

  // Remove state from storage
  removeState(config: PersistenceConfig): void {
    const storage = this.getStorage(config.storage);
    storage.removeItem(config.key);
  }

  // Get storage usage statistics
  getStorageStats(storageType: 'localStorage' | 'sessionStorage'): { used: number; available: number } {
    const storage = this.getStorage(storageType);
    let used = 0;
    
    for (let i = 0; i < storage.length; i++) {
      const key = storage.key(i);
      if (key) {
        const value = storage.getItem(key);
        used += key.length + (value?.length || 0);
      }
    }

    // Estimate available space (5MB typical limit for localStorage)
    const estimated = storageType === 'localStorage' ? 5 * 1024 * 1024 : 5 * 1024 * 1024;
    
    return {
      used: used * 2, // UTF-16 encoding
      available: Math.max(0, estimated - (used * 2))
    };
  }

  // Clean up expired states
  cleanupExpiredStates(storageType: 'localStorage' | 'sessionStorage', maxAge: number): number {
    const storage = this.getStorage(storageType);
    const now = Date.now();
    let cleaned = 0;

    for (let i = storage.length - 1; i >= 0; i--) {
      const key = storage.key(i);
      if (key && key.startsWith('bi-reporting-')) {
        try {
          const rawData = storage.getItem(key);
          if (rawData) {
            const persistedState: PersistedState = JSON.parse(rawData);
            if (now - persistedState.timestamp > maxAge) {
              storage.removeItem(key);
              cleaned++;
            }
          }
        } catch (error) {
          // Remove corrupted data
          storage.removeItem(key);
          cleaned++;
        }
      }
    }

    return cleaned;
  }
}

// Global instance
export const persistenceManager = EnhancedPersistenceManager.getInstance();

// Default configurations for different data types
export const persistenceConfigs = {
  queryHistory: {
    key: 'bi-reporting-query-history',
    version: 1,
    storage: 'localStorage' as const,
    compression: true,
    encryption: false,
    maxAge: 7 * 24 * 60 * 60 * 1000, // 7 days
    maxSize: 1024 * 1024 // 1MB
  },
  userPreferences: {
    key: 'bi-reporting-user-preferences',
    version: 1,
    storage: 'localStorage' as const,
    compression: false,
    encryption: false,
    maxAge: 30 * 24 * 60 * 60 * 1000, // 30 days
    maxSize: 100 * 1024 // 100KB
  },
  sessionState: {
    key: 'bi-reporting-session-state',
    version: 1,
    storage: 'sessionStorage' as const,
    compression: false,
    encryption: false,
    maxAge: 24 * 60 * 60 * 1000, // 24 hours
    maxSize: 500 * 1024 // 500KB
  },
  cacheData: {
    key: 'bi-reporting-cache-data',
    version: 1,
    storage: 'localStorage' as const,
    compression: true,
    encryption: false,
    maxAge: 60 * 60 * 1000, // 1 hour
    maxSize: 2 * 1024 * 1024 // 2MB
  }
};
