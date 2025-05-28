import { openDB, IDBPDatabase } from 'idb';
import { QueryRequest, QueryResponse } from '../types/query';

interface CachedQueryResult {
  id: string;
  queryHash: string;
  request: QueryRequest;
  result: QueryResponse;
  timestamp: number;
  ttl: number;
  accessCount: number;
  lastAccessed: number;
  size: number;
}

interface CacheMetrics {
  totalQueries: number;
  cacheHits: number;
  cacheMisses: number;
  totalSize: number;
  hitRate: number;
}

class QueryCacheService {
  private db: IDBPDatabase | null = null;
  private readonly DB_NAME = 'BIReportingCache';
  private readonly DB_VERSION = 1;
  private readonly STORE_NAME = 'queryResults';
  private readonly DEFAULT_TTL = 3600000; // 1 hour
  private readonly MAX_CACHE_SIZE = 100 * 1024 * 1024; // 100MB
  private readonly MAX_ENTRIES = 1000;

  async init(): Promise<void> {
    try {
      this.db = await openDB(this.DB_NAME, this.DB_VERSION, {
        upgrade(db) {
          if (!db.objectStoreNames.contains('queryResults')) {
            const store = db.createObjectStore('queryResults', { keyPath: 'id' });
            store.createIndex('queryHash', 'queryHash', { unique: false });
            store.createIndex('timestamp', 'timestamp', { unique: false });
            store.createIndex('lastAccessed', 'lastAccessed', { unique: false });
          }
        },
      });

      // Clean up expired entries on initialization
      await this.cleanupExpiredEntries();
      console.log('Query cache service initialized successfully');
    } catch (error) {
      console.error('Failed to initialize query cache service:', error);
    }
  }

  private generateQueryHash(request: QueryRequest): string {
    const hashData = {
      naturalLanguageQuery: request.naturalLanguageQuery,
      sessionId: request.sessionId,
      // Include relevant parameters that affect the result
      includeExplanation: request.includeExplanation,
      maxRows: request.maxRows
    };

    return btoa(JSON.stringify(hashData)).replace(/[+/=]/g, '');
  }

  private calculateSize(data: any): number {
    return new Blob([JSON.stringify(data)]).size;
  }

  async cacheResult(request: QueryRequest, result: QueryResponse, ttl: number = this.DEFAULT_TTL): Promise<void> {
    if (!this.db) {
      console.warn('Cache service not initialized');
      return;
    }

    try {
      const queryHash = this.generateQueryHash(request);
      const id = `${queryHash}_${Date.now()}`;
      const size = this.calculateSize(result);

      const cachedResult: CachedQueryResult = {
        id,
        queryHash,
        request,
        result,
        timestamp: Date.now(),
        ttl,
        accessCount: 0,
        lastAccessed: Date.now(),
        size
      };

      // Check cache size limits before adding
      await this.ensureCacheSize(size);

      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');
      await tx.store.put(cachedResult);
      await tx.done;

      console.log(`Query result cached with hash: ${queryHash}, size: ${size} bytes`);
    } catch (error) {
      console.error('Failed to cache query result:', error);
    }
  }

  async getCachedResult(request: QueryRequest): Promise<QueryResponse | null> {
    if (!this.db) {
      console.warn('Cache service not initialized');
      return null;
    }

    try {
      const queryHash = this.generateQueryHash(request);
      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');
      const index = tx.store.index('queryHash');
      const results = await index.getAll(queryHash);

      if (results.length === 0) {
        return null;
      }

      // Find the most recent non-expired result
      const now = Date.now();
      const validResults = results.filter(result =>
        now - result.timestamp < result.ttl
      );

      if (validResults.length === 0) {
        // Clean up expired results
        await this.removeExpiredResults(queryHash);
        return null;
      }

      // Get the most recent valid result
      const latestResult = validResults.sort((a, b) => b.timestamp - a.timestamp)[0];

      // Update access statistics
      latestResult.accessCount++;
      latestResult.lastAccessed = now;
      await tx.store.put(latestResult);
      await tx.done;

      console.log(`Cache hit for query hash: ${queryHash}`);
      return latestResult.result;
    } catch (error) {
      console.error('Failed to get cached result:', error);
      return null;
    }
  }

  async invalidateCache(pattern?: string): Promise<void> {
    if (!this.db) return;

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');

      if (pattern) {
        // Invalidate specific pattern
        const results = await tx.store.getAll();
        const toDelete = results.filter(result =>
          result.queryHash.includes(pattern) ||
          result.request.naturalLanguageQuery.toLowerCase().includes(pattern.toLowerCase())
        );

        for (const result of toDelete) {
          await tx.store.delete(result.id);
        }

        console.log(`Invalidated ${toDelete.length} cache entries matching pattern: ${pattern}`);
      } else {
        // Clear all cache
        await tx.store.clear();
        console.log('All cache entries cleared');
      }

      await tx.done;
    } catch (error) {
      console.error('Failed to invalidate cache:', error);
    }
  }

  async getCacheMetrics(): Promise<CacheMetrics> {
    if (!this.db) {
      return {
        totalQueries: 0,
        cacheHits: 0,
        cacheMisses: 0,
        totalSize: 0,
        hitRate: 0
      };
    }

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readonly');
      const results = await tx.store.getAll();

      const totalQueries = results.length;
      const cacheHits = results.reduce((sum, result) => sum + result.accessCount, 0);
      const totalSize = results.reduce((sum, result) => sum + result.size, 0);
      const cacheMisses = Math.max(0, totalQueries - cacheHits);
      const hitRate = totalQueries > 0 ? (cacheHits / (cacheHits + cacheMisses)) * 100 : 0;

      return {
        totalQueries,
        cacheHits,
        cacheMisses,
        totalSize,
        hitRate
      };
    } catch (error) {
      console.error('Failed to get cache metrics:', error);
      return {
        totalQueries: 0,
        cacheHits: 0,
        cacheMisses: 0,
        totalSize: 0,
        hitRate: 0
      };
    }
  }

  private async ensureCacheSize(newEntrySize: number): Promise<void> {
    if (!this.db) return;

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');
      const results = await tx.store.getAll();

      const currentSize = results.reduce((sum, result) => sum + result.size, 0);
      const totalSizeAfterAdd = currentSize + newEntrySize;

      // Remove entries if we exceed size or count limits
      if (totalSizeAfterAdd > this.MAX_CACHE_SIZE || results.length >= this.MAX_ENTRIES) {
        // Sort by last accessed (LRU eviction)
        const sortedResults = results.sort((a, b) => a.lastAccessed - b.lastAccessed);

        let sizeToRemove = Math.max(0, totalSizeAfterAdd - this.MAX_CACHE_SIZE);
        let entriesToRemove = Math.max(0, results.length - this.MAX_ENTRIES + 1);

        for (const result of sortedResults) {
          if (sizeToRemove <= 0 && entriesToRemove <= 0) break;

          await tx.store.delete(result.id);
          sizeToRemove -= result.size;
          entriesToRemove--;
        }
      }

      await tx.done;
    } catch (error) {
      console.error('Failed to ensure cache size:', error);
    }
  }

  private async cleanupExpiredEntries(): Promise<void> {
    if (!this.db) return;

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');
      const results = await tx.store.getAll();
      const now = Date.now();

      const expiredResults = results.filter(result =>
        now - result.timestamp > result.ttl
      );

      for (const result of expiredResults) {
        await tx.store.delete(result.id);
      }

      await tx.done;

      if (expiredResults.length > 0) {
        console.log(`Cleaned up ${expiredResults.length} expired cache entries`);
      }
    } catch (error) {
      console.error('Failed to cleanup expired entries:', error);
    }
  }

  private async removeExpiredResults(queryHash: string): Promise<void> {
    if (!this.db) return;

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');
      const index = tx.store.index('queryHash');
      const results = await index.getAll(queryHash);
      const now = Date.now();

      const expiredResults = results.filter(result =>
        now - result.timestamp > result.ttl
      );

      for (const result of expiredResults) {
        await tx.store.delete(result.id);
      }

      await tx.done;
    } catch (error) {
      console.error('Failed to remove expired results:', error);
    }
  }

  async exportCache(): Promise<CachedQueryResult[]> {
    if (!this.db) return [];

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readonly');
      return await tx.store.getAll();
    } catch (error) {
      console.error('Failed to export cache:', error);
      return [];
    }
  }

  async importCache(data: CachedQueryResult[]): Promise<void> {
    if (!this.db) return;

    try {
      const tx = this.db.transaction(this.STORE_NAME, 'readwrite');

      for (const item of data) {
        await tx.store.put(item);
      }

      await tx.done;
      console.log(`Imported ${data.length} cache entries`);
    } catch (error) {
      console.error('Failed to import cache:', error);
    }
  }
}

export const queryCacheService = new QueryCacheService();
