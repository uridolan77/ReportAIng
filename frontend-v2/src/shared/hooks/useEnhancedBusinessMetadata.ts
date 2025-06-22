import { useState, useCallback } from 'react'
import {
  useGetEnhancedBusinessTablesQuery,
  useGetEnhancedBusinessTableQuery,
  useCreateEnhancedBusinessTableMutation,
  useUpdateEnhancedBusinessTableMutation,
  useDeleteEnhancedBusinessTableMutation,
  useSearchEnhancedBusinessTablesMutation,
  useBulkOperateEnhancedBusinessTablesMutation,
  useValidateEnhancedBusinessTableMutation,
  useGetEnhancedBusinessMetadataStatisticsQuery,
  type GetBusinessTablesRequest,
  type BusinessTableSearchRequest,
  type BulkBusinessTableRequest,
  type BusinessTableValidationRequest,
  type CreateTableRequest,
  type UpdateTableRequest,
} from '@shared/store/api/businessApi'

export interface UseEnhancedBusinessMetadataOptions {
  initialPage?: number
  initialPageSize?: number
  initialSearch?: string
  initialSchema?: string
  initialDomain?: string
}

export const useEnhancedBusinessMetadata = (options: UseEnhancedBusinessMetadataOptions = {}) => {
  const {
    initialPage = 1,
    initialPageSize = 20,
    initialSearch = '',
    initialSchema = '',
    initialDomain = '',
  } = options

  // State for table listing
  const [tableParams, setTableParams] = useState<GetBusinessTablesRequest>({
    page: initialPage,
    pageSize: initialPageSize,
    search: initialSearch,
    schema: initialSchema,
    domain: initialDomain,
  })

  // State for bulk operations
  const [selectedTableIds, setSelectedTableIds] = useState<number[]>([])

  // Queries
  const {
    data: tablesResponse,
    isLoading: tablesLoading,
    error: tablesError,
    refetch: refetchTables,
  } = useGetEnhancedBusinessTablesQuery(tableParams)

  const {
    data: statistics,
    isLoading: statisticsLoading,
    error: statisticsError,
    refetch: refetchStatistics,
  } = useGetEnhancedBusinessMetadataStatisticsQuery()

  // Mutations
  const [createTable, { isLoading: creating }] = useCreateEnhancedBusinessTableMutation()
  const [updateTable, { isLoading: updating }] = useUpdateEnhancedBusinessTableMutation()
  const [deleteTable, { isLoading: deleting }] = useDeleteEnhancedBusinessTableMutation()
  const [searchTables, { isLoading: searching }] = useSearchEnhancedBusinessTablesMutation()
  const [bulkOperate, { isLoading: bulkOperating }] = useBulkOperateEnhancedBusinessTablesMutation()
  const [validateTable, { isLoading: validating }] = useValidateEnhancedBusinessTableMutation()

  // Helper functions
  const updateTableParams = useCallback((newParams: Partial<GetBusinessTablesRequest>) => {
    setTableParams(prev => ({ ...prev, ...newParams }))
  }, [])

  const resetTableParams = useCallback(() => {
    setTableParams({
      page: 1,
      pageSize: 20,
      search: '',
      schema: '',
      domain: '',
    })
  }, [])

  const handleSearch = useCallback((searchQuery: string) => {
    updateTableParams({ search: searchQuery, page: 1 })
  }, [updateTableParams])

  const handleFilterChange = useCallback((filters: { schema?: string; domain?: string }) => {
    updateTableParams({ ...filters, page: 1 })
  }, [updateTableParams])

  const handlePageChange = useCallback((page: number, pageSize?: number) => {
    updateTableParams({ page, ...(pageSize && { pageSize }) })
  }, [updateTableParams])

  const handleTableSelection = useCallback((tableIds: number[]) => {
    setSelectedTableIds(tableIds)
  }, [])

  const clearSelection = useCallback(() => {
    setSelectedTableIds([])
  }, [])

  const handleAdvancedSearch = useCallback(async (searchRequest: BusinessTableSearchRequest) => {
    try {
      const result = await searchTables(searchRequest).unwrap()
      return result
    } catch (error) {
      console.error('Advanced search failed:', error)
      throw error
    }
  }, [searchTables])

  const handleBulkOperation = useCallback(async (operation: BulkBusinessTableRequest['operation'], operationData?: any) => {
    if (selectedTableIds.length === 0) {
      throw new Error('No tables selected for bulk operation')
    }

    try {
      const result = await bulkOperate({
        tableIds: selectedTableIds,
        operation,
        operationData,
      }).unwrap()
      
      // Refresh data after successful bulk operation
      refetchTables()
      refetchStatistics()
      clearSelection()
      
      return result
    } catch (error) {
      console.error('Bulk operation failed:', error)
      throw error
    }
  }, [selectedTableIds, bulkOperate, refetchTables, refetchStatistics, clearSelection])

  const handleCreateTable = useCallback(async (tableData: CreateTableRequest) => {
    try {
      const result = await createTable(tableData).unwrap()
      refetchTables()
      refetchStatistics()
      return result
    } catch (error) {
      console.error('Create table failed:', error)
      throw error
    }
  }, [createTable, refetchTables, refetchStatistics])

  const handleUpdateTable = useCallback(async (id: number, tableData: UpdateTableRequest) => {
    try {
      const result = await updateTable({ id, ...tableData }).unwrap()
      refetchTables()
      refetchStatistics()
      return result
    } catch (error) {
      console.error('Update table failed:', error)
      throw error
    }
  }, [updateTable, refetchTables, refetchStatistics])

  const handleDeleteTable = useCallback(async (id: number) => {
    try {
      const result = await deleteTable(id).unwrap()
      refetchTables()
      refetchStatistics()
      return result
    } catch (error) {
      console.error('Delete table failed:', error)
      throw error
    }
  }, [deleteTable, refetchTables, refetchStatistics])

  const handleValidateTable = useCallback(async (validationRequest: BusinessTableValidationRequest) => {
    try {
      const result = await validateTable(validationRequest).unwrap()
      return result
    } catch (error) {
      console.error('Table validation failed:', error)
      throw error
    }
  }, [validateTable])

  return {
    // Data
    tables: tablesResponse?.data || [],
    pagination: tablesResponse?.pagination,
    filters: tablesResponse?.filters,
    statistics,
    selectedTableIds,

    // Loading states
    tablesLoading,
    statisticsLoading,
    creating,
    updating,
    deleting,
    searching,
    bulkOperating,
    validating,

    // Errors
    tablesError,
    statisticsError,

    // Actions
    updateTableParams,
    resetTableParams,
    handleSearch,
    handleFilterChange,
    handlePageChange,
    handleTableSelection,
    clearSelection,
    handleAdvancedSearch,
    handleBulkOperation,
    handleCreateTable,
    handleUpdateTable,
    handleDeleteTable,
    handleValidateTable,
    refetchTables,
    refetchStatistics,

    // Current params
    tableParams,
  }
}

// Hook for individual table management
export const useEnhancedBusinessTable = (tableId: number) => {
  const {
    data: tableResponse,
    isLoading,
    error,
    refetch,
  } = useGetEnhancedBusinessTableQuery(tableId, {
    skip: !tableId || tableId === 0,
  })

  return {
    table: tableResponse?.data,
    isLoading,
    error,
    refetch,
  }
}
