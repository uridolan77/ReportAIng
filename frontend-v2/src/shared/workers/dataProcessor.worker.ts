/**
 * Data Processing Web Worker
 * 
 * Handles heavy data processing operations off the main thread
 * to maintain smooth UI performance during large dataset operations.
 */

// Types for worker communication
export interface WorkerMessage {
  id: string
  type: 'aggregate' | 'filter' | 'sort' | 'transform' | 'analyze'
  data: any[]
  options?: Record<string, any>
}

export interface WorkerResponse {
  id: string
  success: boolean
  result?: any
  error?: string
  processingTime?: number
}

// Data aggregation functions
const aggregateData = (data: any[], options: any = {}) => {
  const { groupBy, aggregations = {} } = options
  
  if (!groupBy) {
    // Simple aggregations without grouping
    const result: Record<string, any> = {}
    
    Object.entries(aggregations).forEach(([key, operation]) => {
      const values = data.map(item => item[key]).filter(val => val != null)
      
      switch (operation) {
        case 'sum':
          result[key] = values.reduce((sum, val) => sum + Number(val), 0)
          break
        case 'avg':
          result[key] = values.reduce((sum, val) => sum + Number(val), 0) / values.length
          break
        case 'min':
          result[key] = Math.min(...values.map(Number))
          break
        case 'max':
          result[key] = Math.max(...values.map(Number))
          break
        case 'count':
          result[key] = values.length
          break
        default:
          result[key] = values.length
      }
    })
    
    return result
  }
  
  // Group by aggregations
  const groups = data.reduce((acc, item) => {
    const groupKey = item[groupBy]
    if (!acc[groupKey]) {
      acc[groupKey] = []
    }
    acc[groupKey].push(item)
    return acc
  }, {} as Record<string, any[]>)
  
  const result = Object.entries(groups).map(([groupKey, groupData]) => {
    const aggregated: Record<string, any> = { [groupBy]: groupKey }
    
    Object.entries(aggregations).forEach(([key, operation]) => {
      const values = groupData.map(item => item[key]).filter(val => val != null)
      
      switch (operation) {
        case 'sum':
          aggregated[key] = values.reduce((sum, val) => sum + Number(val), 0)
          break
        case 'avg':
          aggregated[key] = values.reduce((sum, val) => sum + Number(val), 0) / values.length
          break
        case 'min':
          aggregated[key] = Math.min(...values.map(Number))
          break
        case 'max':
          aggregated[key] = Math.max(...values.map(Number))
          break
        case 'count':
          aggregated[key] = values.length
          break
        default:
          aggregated[key] = values.length
      }
    })
    
    return aggregated
  })
  
  return result
}

// Data filtering functions
const filterData = (data: any[], options: any = {}) => {
  const { filters = [], searchTerm = '', searchFields = [] } = options
  
  let result = data
  
  // Apply filters
  filters.forEach((filter: any) => {
    const { field, operator, value } = filter
    
    result = result.filter(item => {
      const itemValue = item[field]
      
      switch (operator) {
        case 'equals':
          return itemValue === value
        case 'not_equals':
          return itemValue !== value
        case 'greater_than':
          return Number(itemValue) > Number(value)
        case 'less_than':
          return Number(itemValue) < Number(value)
        case 'greater_equal':
          return Number(itemValue) >= Number(value)
        case 'less_equal':
          return Number(itemValue) <= Number(value)
        case 'contains':
          return String(itemValue).toLowerCase().includes(String(value).toLowerCase())
        case 'starts_with':
          return String(itemValue).toLowerCase().startsWith(String(value).toLowerCase())
        case 'ends_with':
          return String(itemValue).toLowerCase().endsWith(String(value).toLowerCase())
        case 'in':
          return Array.isArray(value) && value.includes(itemValue)
        case 'not_in':
          return Array.isArray(value) && !value.includes(itemValue)
        default:
          return true
      }
    })
  })
  
  // Apply search term
  if (searchTerm && searchFields.length > 0) {
    const term = searchTerm.toLowerCase()
    result = result.filter(item =>
      searchFields.some((field: string) =>
        String(item[field]).toLowerCase().includes(term)
      )
    )
  }
  
  return result
}

// Data sorting functions
const sortData = (data: any[], options: any = {}) => {
  const { sortBy, sortOrder = 'asc' } = options
  
  if (!sortBy) return data
  
  return [...data].sort((a, b) => {
    const aVal = a[sortBy]
    const bVal = b[sortBy]
    
    // Handle null/undefined values
    if (aVal == null && bVal == null) return 0
    if (aVal == null) return sortOrder === 'asc' ? -1 : 1
    if (bVal == null) return sortOrder === 'asc' ? 1 : -1
    
    // Handle different data types
    if (typeof aVal === 'number' && typeof bVal === 'number') {
      return sortOrder === 'asc' ? aVal - bVal : bVal - aVal
    }
    
    if (aVal instanceof Date && bVal instanceof Date) {
      return sortOrder === 'asc' 
        ? aVal.getTime() - bVal.getTime()
        : bVal.getTime() - aVal.getTime()
    }
    
    // String comparison
    const aStr = String(aVal).toLowerCase()
    const bStr = String(bVal).toLowerCase()
    
    if (sortOrder === 'asc') {
      return aStr < bStr ? -1 : aStr > bStr ? 1 : 0
    } else {
      return aStr > bStr ? -1 : aStr < bStr ? 1 : 0
    }
  })
}

// Data transformation functions
const transformData = (data: any[], options: any = {}) => {
  const { transformations = [] } = options
  
  let result = data
  
  transformations.forEach((transform: any) => {
    const { type, field, newField, operation, value } = transform
    
    switch (type) {
      case 'add_field':
        result = result.map(item => ({
          ...item,
          [newField]: operation === 'constant' ? value : eval(operation.replace(/\{(\w+)\}/g, 'item.$1'))
        }))
        break
      
      case 'modify_field':
        result = result.map(item => ({
          ...item,
          [field]: operation === 'constant' ? value : eval(operation.replace(/\{(\w+)\}/g, 'item.$1'))
        }))
        break
      
      case 'remove_field':
        result = result.map(item => {
          const { [field]: removed, ...rest } = item
          return rest
        })
        break
      
      case 'rename_field':
        result = result.map(item => {
          const { [field]: value, ...rest } = item
          return { ...rest, [newField]: value }
        })
        break
    }
  })
  
  return result
}

// Data analysis functions
const analyzeData = (data: any[], options: any = {}) => {
  const { analysisType = 'basic', fields = [] } = options
  
  const result: Record<string, any> = {
    totalRecords: data.length,
    fields: {},
  }
  
  // Analyze each field
  fields.forEach((field: string) => {
    const values = data.map(item => item[field]).filter(val => val != null)
    const fieldAnalysis: Record<string, any> = {
      totalValues: values.length,
      nullCount: data.length - values.length,
      uniqueCount: new Set(values).size,
    }
    
    // Numeric analysis
    const numericValues = values.filter(val => !isNaN(Number(val))).map(Number)
    if (numericValues.length > 0) {
      fieldAnalysis.numeric = {
        min: Math.min(...numericValues),
        max: Math.max(...numericValues),
        avg: numericValues.reduce((sum, val) => sum + val, 0) / numericValues.length,
        median: numericValues.sort((a, b) => a - b)[Math.floor(numericValues.length / 2)],
      }
    }
    
    // String analysis
    const stringValues = values.filter(val => typeof val === 'string')
    if (stringValues.length > 0) {
      fieldAnalysis.string = {
        avgLength: stringValues.reduce((sum, val) => sum + val.length, 0) / stringValues.length,
        minLength: Math.min(...stringValues.map(val => val.length)),
        maxLength: Math.max(...stringValues.map(val => val.length)),
      }
    }
    
    // Top values
    const valueCounts = values.reduce((acc, val) => {
      acc[val] = (acc[val] || 0) + 1
      return acc
    }, {} as Record<string, number>)
    
    fieldAnalysis.topValues = Object.entries(valueCounts)
      .sort(([, a], [, b]) => b - a)
      .slice(0, 10)
      .map(([value, count]) => ({ value, count }))
    
    result.fields[field] = fieldAnalysis
  })
  
  return result
}

// Main worker message handler
self.onmessage = function(e: MessageEvent<WorkerMessage>) {
  const { id, type, data, options = {} } = e.data
  const startTime = Date.now()
  
  try {
    let result: any
    
    switch (type) {
      case 'aggregate':
        result = aggregateData(data, options)
        break
      case 'filter':
        result = filterData(data, options)
        break
      case 'sort':
        result = sortData(data, options)
        break
      case 'transform':
        result = transformData(data, options)
        break
      case 'analyze':
        result = analyzeData(data, options)
        break
      default:
        throw new Error(`Unknown operation type: ${type}`)
    }
    
    const processingTime = Date.now() - startTime
    
    const response: WorkerResponse = {
      id,
      success: true,
      result,
      processingTime,
    }
    
    self.postMessage(response)
  } catch (error) {
    const response: WorkerResponse = {
      id,
      success: false,
      error: error instanceof Error ? error.message : 'Unknown error',
      processingTime: Date.now() - startTime,
    }
    
    self.postMessage(response)
  }
}

// Export types for TypeScript
export type { WorkerMessage, WorkerResponse }
