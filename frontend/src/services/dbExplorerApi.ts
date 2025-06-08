import apiClient from './api';
import { DatabaseSchema, DatabaseTable, TableDataPreview, DatabaseColumn } from '../types/dbExplorer';

export class DBExplorerAPI {
  private static readonly BASE_URL = '/api/schema';

  /**
   * Get complete database schema with all tables and their structure
   */
  static async getDatabaseSchema(connectionName?: string, bustCache: boolean = false): Promise<DatabaseSchema> {
    try {
      const params: any = { connectionName };

      // Add cache busting parameter if requested
      if (bustCache) {
        params._t = Date.now();
        console.log('🔍 Cache busting enabled for schema request');
      }

      const response = await apiClient.get<any>(`${this.BASE_URL}`, {
        params
      });

      // Transform the API response to match our DatabaseSchema interface
      const apiData = response.data;

      if (!apiData) {
        throw new Error('No schema data received from API');
      }

      console.log('🔍 Full API Response:', apiData);
      console.log('🔍 API Response structure:', {
        databaseName: apiData.databaseName,
        tablesCount: apiData.tables?.length || 0,
        viewsCount: apiData.views?.length || 0,
        keys: Object.keys(apiData),
        sampleTable: apiData.tables?.[0] ? {
          name: apiData.tables[0].name,
          columnsCount: apiData.tables[0].columns?.length || 0,
          tableKeys: Object.keys(apiData.tables[0]),
          sampleColumns: apiData.tables[0].columns?.slice(0, 3)?.map((col: any) => ({
            name: col.name || col.columnName || col.Name,
            dataType: col.dataType || col.type || col.DataType,
            keys: Object.keys(col)
          }))
        } : null
      });

      // Handle different possible response structures
      let tables = [];
      let views = [];
      let databaseName = 'Database';

      if (apiData.tables && Array.isArray(apiData.tables)) {
        tables = apiData.tables;
      } else if (Array.isArray(apiData)) {
        // If the response is directly an array of tables
        tables = apiData;
      }

      if (apiData.views && Array.isArray(apiData.views)) {
        views = apiData.views;
      }

      if (apiData.databaseName) {
        databaseName = apiData.databaseName;
      } else if (apiData.name) {
        databaseName = apiData.name;
      }

      console.log('Processed data:', {
        databaseName,
        tablesCount: tables.length,
        viewsCount: views.length,
        sampleTable: tables[0]
      });

      return {
        name: databaseName,
        lastUpdated: apiData.lastUpdated || new Date().toISOString(),
        version: apiData.version || '1.0.0',
        views: views.map(DBExplorerAPI.transformTableMetadata),
        tables: tables.map(DBExplorerAPI.transformTableMetadata)
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

    console.log(`🔍 Transforming table ${apiTable.name}:`, {
      originalTable: apiTable,
      columnsReceived: apiTable.columns?.length || 0,
      sampleColumn: apiTable.columns?.[0],
      allColumnNames: apiTable.columns?.map((col: any) => col.name || col.columnName || col.Name).slice(0, 5),
      firstThreeColumns: apiTable.columns?.slice(0, 3)?.map((col: any) => ({
        originalKeys: Object.keys(col),
        name: col.name || col.columnName || col.Name,
        dataType: col.dataType || col.type || col.DataType,
        isPrimaryKey: col.isPrimaryKey || col.IsPrimaryKey,
        isNullable: col.isNullable
      }))
    });

    const transformed = {
      name: apiTable.name || apiTable.tableName || apiTable.Name || 'Unknown',
      schema: apiTable.schema || apiTable.schemaName || apiTable.Schema || 'dbo',
      type: (apiTable.type?.toLowerCase() === 'view' ? 'view' : 'table') as 'table' | 'view',
      rowCount: apiTable.rowCount || apiTable.RowCount,
      description: apiTable.description || apiTable.businessDescription || apiTable.Description,
      columns: apiTable.columns?.map(DBExplorerAPI.transformColumnMetadata) || [],
      primaryKeys: apiTable.columns?.filter((col: any) =>
        col.isPrimaryKey || col.IsPrimaryKey
      ).map((col: any) =>
        col.name || col.columnName || col.Name
      ) || [],
      foreignKeys: apiTable.columns?.filter((col: any) =>
        col.isForeignKey || col.IsForeignKey
      ).map((col: any) => ({
        name: `FK_${apiTable.name}_${col.name || col.columnName || col.Name}`,
        column: col.name || col.columnName || col.Name,
        referencedTable: col.referencedTable || col.ReferencedTable || 'Unknown',
        referencedColumn: col.referencedColumn || col.ReferencedColumn || 'Unknown'
      })) || []
    };

    console.log(`Transformed table ${apiTable.name} result:`, {
      transformedColumns: transformed.columns.length,
      primaryKeys: transformed.primaryKeys.length,
      foreignKeys: transformed.foreignKeys.length,
      sampleTransformedColumn: transformed.columns[0]
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

    const transformed = {
      name: apiColumn.name || apiColumn.columnName || apiColumn.Name || 'Unknown',
      dataType: apiColumn.dataType || apiColumn.type || apiColumn.DataType || 'varchar',
      isNullable: apiColumn.isNullable !== false, // Default to true if not specified
      isPrimaryKey: apiColumn.isPrimaryKey === true || apiColumn.IsPrimaryKey === true,
      isForeignKey: apiColumn.isForeignKey === true || apiColumn.IsForeignKey === true,
      defaultValue: apiColumn.defaultValue || apiColumn.DefaultValue,
      maxLength: apiColumn.maxLength || apiColumn.MaxLength,
      precision: apiColumn.precision || apiColumn.Precision,
      scale: apiColumn.scale || apiColumn.Scale,
      description: apiColumn.description || apiColumn.businessDescription || apiColumn.Description,
      referencedTable: apiColumn.referencedTable || apiColumn.ReferencedTable,
      referencedColumn: apiColumn.referencedColumn || apiColumn.ReferencedColumn
    };

    // Log first few columns for debugging
    if (Math.random() < 0.3) { // Log ~30% of columns to see more examples
      console.log('Column transformation:', {
        original: apiColumn,
        transformed: transformed,
        availableKeys: Object.keys(apiColumn)
      });
    }

    return transformed;
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
    console.log('🔄 Calling schema refresh API...');
    try {
      // Try the UnifiedQueryController endpoint first
      await apiClient.post('/api/query/refresh-schema', {
        connectionName
      });
      console.log('🔄 Schema refresh API call successful');
    } catch (error) {
      console.error('🔄 Schema refresh API call failed:', error);
      // Fallback to SchemaController endpoint
      try {
        await apiClient.post('/api/schema/refresh', {}, {
          params: { connectionName }
        });
        console.log('🔄 Schema refresh fallback API call successful');
      } catch (fallbackError) {
        console.error('🔄 Schema refresh fallback API call failed:', fallbackError);
        throw fallbackError;
      }
    }
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
