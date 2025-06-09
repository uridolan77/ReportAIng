import apiClient, { ApiService, SqlExecutionRequest } from './api';
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
    const { limit = 1000, offset = 0, connectionName } = options;

    try {
      console.log(`🔍 Getting table data preview for: ${tableName}, limit: ${limit}`);

      // Use fully qualified table name for DailyActionsDB tables
      let qualifiedTableName = tableName;
      if (tableName.startsWith('tbl_Daily_actions') || tableName.startsWith('tbl_Countries') || tableName.startsWith('tbl_Currencies') || tableName.startsWith('tbl_White_labels')) {
        qualifiedTableName = `[DailyActionsDB].[common].[${tableName}]`;
      }

      // Start with a simple query first, then add ordering if it works
      let sql = `SELECT TOP ${limit} * FROM ${qualifiedTableName} WITH (NOLOCK)`;

      // Try to get table schema to find primary key or ID column for ordering
      try {
        const schema = await this.getSchema(connectionName);
        const table = schema.tables?.find((t: any) => t.name === tableName);

        // Find the best column to order by (prefer ID, then primary key, then first column)
        if (table?.columns) {
          const idColumn = table.columns.find((col: any) =>
            col.name.toLowerCase().includes('id') || col.isPrimaryKey
          );
          if (idColumn) {
            sql = `SELECT TOP ${limit} * FROM ${qualifiedTableName} WITH (NOLOCK) ORDER BY ${idColumn.name} DESC`;
            console.log(`🔍 Using order by column: ${idColumn.name}`);
          } else if (table.columns.length > 0) {
            sql = `SELECT TOP ${limit} * FROM ${qualifiedTableName} WITH (NOLOCK) ORDER BY ${table.columns[0].name} DESC`;
            console.log(`🔍 Using order by first column: ${table.columns[0].name}`);
          }
        }
      } catch (schemaError) {
        console.warn('🔍 Could not get schema for ordering, using simple query without ORDER BY:', schemaError);
      }

      console.log(`🔍 Executing SQL: ${sql}`);

      const requestPayload: SqlExecutionRequest = {
        sql,
        sessionId: `db-explorer-${Date.now()}`,
        options: {
          maxRows: limit,
          timeoutSeconds: 30,
          dataSource: connectionName
        }
      };

      console.log(`🔍 Request payload:`, requestPayload);

      // Use the proven ApiService.executeRawSQL method instead of direct API call
      const apiResponse = await ApiService.executeRawSQL(requestPayload);

      console.log(`🔍 API response:`, apiResponse);

      // Transform the response to match our TableDataPreview interface
      const previewData = {
        tableName,
        data: apiResponse.result?.data || [],
        totalRows: apiResponse.result?.totalRows || apiResponse.result?.data?.length || 0,
        columns: apiResponse.result?.columns || [],
        sampleSize: limit
      };

      console.log(`🔍 Preview data result:`, previewData);
      return previewData;
    } catch (error: any) {
      // Log detailed error information
      console.error('🔍 Table data preview API call failed:', error);
      console.error('🔍 Error response data:', error.response?.data);
      console.error('🔍 Error response status:', error.response?.status);
      console.error('🔍 Error response headers:', error.response?.headers);
      console.error('🔍 Error details:', {
        message: error instanceof Error ? error.message : 'Unknown error',
        tableName,
        limit,
        connectionName,
        sql
      });

      // Get more specific error message from response
      let errorMessage = `Failed to load data for table ${tableName}`;
      if (error.response?.data?.error) {
        errorMessage += `: ${error.response.data.error}`;
      } else if (error.response?.data?.message) {
        errorMessage += `: ${error.response.data.message}`;
      } else if (error instanceof Error) {
        errorMessage += `: ${error.message}`;
      }

      throw new Error(errorMessage);
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
