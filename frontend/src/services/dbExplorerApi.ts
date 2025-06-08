import apiClient from './api';
import { DatabaseSchema, DatabaseTable, TableDataPreview, DatabaseColumn } from '../types/dbExplorer';

export class DBExplorerAPI {
  private static readonly BASE_URL = '/api/schema';

  /**
   * Get complete database schema with all tables and their structure
   */
  static async getDatabaseSchema(connectionName?: string): Promise<DatabaseSchema> {
    try {
      const response = await apiClient.get<any>(`${this.BASE_URL}`, {
        params: { connectionName }
      });

      // Transform the API response to match our DatabaseSchema interface
      const apiData = response.data;

      if (!apiData) {
        throw new Error('No schema data received from API');
      }

      console.log('Full API Response:', apiData);
      console.log('API Response structure:', {
        databaseName: apiData.databaseName,
        tablesCount: apiData.tables?.length || 0,
        viewsCount: apiData.views?.length || 0,
        keys: Object.keys(apiData),
        sampleTable: apiData.tables?.[0] ? {
          name: apiData.tables[0].name,
          columnsCount: apiData.tables[0].columns?.length || 0,
          tableKeys: Object.keys(apiData.tables[0])
        } : null
      });

      return {
        name: apiData.databaseName || 'Database',
        lastUpdated: apiData.lastUpdated || new Date().toISOString(),
        version: apiData.version || '1.0.0',
        views: apiData.views?.map(DBExplorerAPI.transformTableMetadata) || [],
        tables: apiData.tables?.map(DBExplorerAPI.transformTableMetadata) || []
      };
    } catch (error) {
      console.error('Error fetching database schema:', error);
      // Return a fallback schema structure
      return {
        name: 'Database',
        lastUpdated: new Date().toISOString(),
        version: '1.0.0',
        views: [],
        tables: []
      };
    }
  }

  /**
   * Transform API table metadata to our DatabaseTable interface
   */
  private static transformTableMetadata(apiTable: any): DatabaseTable {
    if (!apiTable) {
      console.warn('Received null/undefined table metadata');
      return {
        name: 'Unknown',
        schema: 'dbo',
        type: 'table',
        columns: [],
        primaryKeys: [],
        foreignKeys: []
      };
    }

    const transformed = {
      name: apiTable.name || 'Unknown',
      schema: apiTable.schema || 'dbo',
      type: (apiTable.type?.toLowerCase() === 'view' ? 'view' : 'table') as 'table' | 'view',
      rowCount: apiTable.rowCount,
      description: apiTable.description || apiTable.businessDescription,
      columns: apiTable.columns?.map(DBExplorerAPI.transformColumnMetadata) || [],
      primaryKeys: apiTable.columns?.filter((col: any) => col.isPrimaryKey).map((col: any) => col.name) || [],
      foreignKeys: apiTable.columns?.filter((col: any) => col.isForeignKey).map((col: any) => ({
        name: `FK_${apiTable.name}_${col.name}`,
        column: col.name,
        referencedTable: col.referencedTable || 'Unknown',
        referencedColumn: col.referencedColumn || 'Unknown'
      })) || []
    };

    console.log(`Transformed table ${apiTable.name}:`, {
      originalColumns: apiTable.columns?.length || 0,
      transformedColumns: transformed.columns.length,
      primaryKeys: transformed.primaryKeys.length,
      foreignKeys: transformed.foreignKeys.length
    });

    return transformed;
  }

  /**
   * Transform API column metadata to our DatabaseColumn interface
   */
  private static transformColumnMetadata(apiColumn: any): DatabaseColumn {
    if (!apiColumn) {
      console.warn('Received null/undefined column metadata');
      return {
        name: 'Unknown',
        dataType: 'varchar',
        isNullable: true,
        isPrimaryKey: false,
        isForeignKey: false
      };
    }

    return {
      name: apiColumn.name || 'Unknown',
      dataType: apiColumn.dataType || 'varchar',
      isNullable: apiColumn.isNullable !== false, // Default to true if not specified
      isPrimaryKey: apiColumn.isPrimaryKey === true,
      isForeignKey: apiColumn.isForeignKey === true,
      defaultValue: apiColumn.defaultValue,
      maxLength: apiColumn.maxLength,
      precision: apiColumn.precision,
      scale: apiColumn.scale,
      description: apiColumn.description || apiColumn.businessDescription,
      referencedTable: apiColumn.referencedTable,
      referencedColumn: apiColumn.referencedColumn
    };
  }

  /**
   * Get detailed information about a specific table
   */
  static async getTableDetails(tableName: string, connectionName?: string): Promise<DatabaseTable> {
    const response = await apiClient.get<DatabaseTable>(`${this.BASE_URL}/tables/${tableName}`, {
      params: { connectionName }
    });
    return response.data;
  }

  /**
   * Get preview data from a table (limited rows)
   */
  static async getTableDataPreview(
    tableName: string,
    options: {
      limit?: number;
      offset?: number;
      connectionName?: string;
    } = {}
  ): Promise<TableDataPreview> {
    const { limit = 100, offset = 0, connectionName } = options;

    try {
      // Use the existing SQL execution endpoint to get table data
      const sql = `SELECT TOP ${limit} * FROM ${tableName}`;

      const response = await apiClient.post('/api/query/execute', {
        sql,
        sessionId: `db-explorer-${Date.now()}`,
        options: {
          maxRows: limit,
          dataSource: connectionName
        }
      });

      // Transform the response to match our TableDataPreview interface
      const result = response.data;

      return {
        tableName,
        data: result.result?.data || [],
        totalRows: result.result?.totalRows || result.result?.data?.length || 0,
        columns: result.result?.columns || [],
        sampleSize: limit
      };
    } catch (error) {
      // Fallback to mock data if API is not available
      console.warn('API not available, using mock data for table preview');
      return {
        tableName,
        data: [],
        totalRows: 0,
        columns: [],
        sampleSize: 0
      };
    }
  }

  /**
   * Search tables and columns by name or description
   */
  static async searchTables(
    searchTerm: string, 
    connectionName?: string
  ): Promise<DatabaseTable[]> {
    const schema = await this.getDatabaseSchema(connectionName);
    
    if (!searchTerm.trim()) {
      return schema.tables;
    }

    const term = searchTerm.toLowerCase();
    
    return schema.tables.filter(table => {
      // Search in table name
      if (table.name.toLowerCase().includes(term)) {
        return true;
      }
      
      // Search in table description
      if (table.description?.toLowerCase().includes(term)) {
        return true;
      }
      
      // Search in column names
      return table.columns.some(column => 
        column.name.toLowerCase().includes(term) ||
        column.description?.toLowerCase().includes(term)
      );
    });
  }

  /**
   * Get available data sources/connections
   */
  static async getDataSources(): Promise<string[]> {
    const response = await apiClient.get<string[]>('/api/schema/datasources');
    return response.data;
  }

  /**
   * Refresh database schema cache
   */
  static async refreshSchema(connectionName?: string): Promise<void> {
    await apiClient.post('/api/query/refresh-schema', {
      connectionName
    });
  }

  /**
   * Export table structure as SQL DDL
   */
  static async exportTableStructure(tableName: string, connectionName?: string): Promise<string> {
    const response = await apiClient.get<{ ddl: string }>(`${this.BASE_URL}/tables/${tableName}/ddl`, {
      params: { connectionName }
    });
    return response.data.ddl;
  }

  /**
   * Get table relationships (foreign keys)
   */
  static async getTableRelationships(tableName: string, connectionName?: string): Promise<any> {
    const response = await apiClient.get(`${this.BASE_URL}/tables/${tableName}/relationships`, {
      params: { connectionName }
    });
    return response.data;
  }
}

export default DBExplorerAPI;
