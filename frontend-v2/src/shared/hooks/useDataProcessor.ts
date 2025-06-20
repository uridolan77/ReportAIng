/**
 * Data Processor Hook
 * 
 * React hook for using the Web Worker to process large datasets
 * without blocking the main thread.
 */

import React, { useRef, useCallback, useState } from 'react'
import type { WorkerMessage, WorkerResponse } from '../workers/dataProcessor.worker'

export interface DataProcessorOptions {
  /** Maximum number of concurrent operations */
  maxConcurrent?: number
  /** Timeout for operations in milliseconds */
  timeout?: number
  /** Enable debug logging */
  debug?: boolean
}

export interface ProcessingOperation {
  id: string
  type: WorkerMessage['type']
  status: 'pending' | 'processing' | 'completed' | 'error'
  startTime: number
  endTime?: number
  error?: string
}

export const useDataProcessor = (options: DataProcessorOptions = {}) => {
  const {
    maxConcurrent = 3,
    timeout = 30000, // 30 seconds
    debug = false,
  } = options
  
  const workerRef = useRef<Worker | null>(null)
  const operationsRef = useRef<Map<string, ProcessingOperation>>(new Map())
  const pendingCallbacksRef = useRef<Map<string, {
    resolve: (result: any) => void
    reject: (error: Error) => void
  }>>(new Map())
  
  const [isProcessing, setIsProcessing] = useState(false)
  const [activeOperations, setActiveOperations] = useState<ProcessingOperation[]>([])
  
  // Initialize worker
  const initWorker = useCallback(() => {
    if (workerRef.current) return workerRef.current
    
    try {
      // Create worker from the worker file
      workerRef.current = new Worker(
        new URL('../workers/dataProcessor.worker.ts', import.meta.url),
        { type: 'module' }
      )
      
      // Handle worker messages
      workerRef.current.onmessage = (e: MessageEvent<WorkerResponse>) => {
        const { id, success, result, error, processingTime } = e.data
        
        const operation = operationsRef.current.get(id)
        if (!operation) return
        
        // Update operation status
        operation.status = success ? 'completed' : 'error'
        operation.endTime = Date.now()
        operation.error = error
        
        if (debug) {
          console.log(`Data processing ${success ? 'completed' : 'failed'} [${id}]:`, {
            type: operation.type,
            processingTime,
            error,
          })
        }
        
        // Resolve or reject the promise
        const callbacks = pendingCallbacksRef.current.get(id)
        if (callbacks) {
          if (success) {
            callbacks.resolve(result)
          } else {
            callbacks.reject(new Error(error || 'Processing failed'))
          }
          pendingCallbacksRef.current.delete(id)
        }
        
        // Update state
        setActiveOperations(prev => 
          prev.map(op => op.id === id ? operation : op)
        )
        
        // Check if all operations are complete
        const hasActiveOps = Array.from(operationsRef.current.values())
          .some(op => op.status === 'pending' || op.status === 'processing')
        setIsProcessing(hasActiveOps)
      }
      
      // Handle worker errors
      workerRef.current.onerror = (error) => {
        console.error('Data processor worker error:', error)
        
        // Reject all pending operations
        pendingCallbacksRef.current.forEach(({ reject }) => {
          reject(new Error('Worker error'))
        })
        pendingCallbacksRef.current.clear()
        
        // Update operations status
        operationsRef.current.forEach(operation => {
          if (operation.status === 'pending' || operation.status === 'processing') {
            operation.status = 'error'
            operation.error = 'Worker error'
            operation.endTime = Date.now()
          }
        })
        
        setIsProcessing(false)
        setActiveOperations(Array.from(operationsRef.current.values()))
      }
      
      return workerRef.current
    } catch (error) {
      console.error('Failed to create data processor worker:', error)
      return null
    }
  }, [debug])
  
  // Process data with the worker
  const processData = useCallback(async <T = any>(
    type: WorkerMessage['type'],
    data: any[],
    options: Record<string, any> = {}
  ): Promise<T> => {
    const worker = initWorker()
    if (!worker) {
      throw new Error('Failed to initialize worker')
    }
    
    // Check concurrent operations limit
    const activeCount = Array.from(operationsRef.current.values())
      .filter(op => op.status === 'pending' || op.status === 'processing').length
    
    if (activeCount >= maxConcurrent) {
      throw new Error(`Maximum concurrent operations (${maxConcurrent}) exceeded`)
    }
    
    const id = `${type}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
    
    // Create operation record
    const operation: ProcessingOperation = {
      id,
      type,
      status: 'pending',
      startTime: Date.now(),
    }
    
    operationsRef.current.set(id, operation)
    setActiveOperations(Array.from(operationsRef.current.values()))
    setIsProcessing(true)
    
    return new Promise<T>((resolve, reject) => {
      // Store callbacks
      pendingCallbacksRef.current.set(id, { resolve, reject })
      
      // Set timeout
      const timeoutId = setTimeout(() => {
        const callbacks = pendingCallbacksRef.current.get(id)
        if (callbacks) {
          callbacks.reject(new Error('Operation timeout'))
          pendingCallbacksRef.current.delete(id)
          
          // Update operation status
          const op = operationsRef.current.get(id)
          if (op) {
            op.status = 'error'
            op.error = 'Timeout'
            op.endTime = Date.now()
          }
          
          setActiveOperations(Array.from(operationsRef.current.values()))
        }
      }, timeout)
      
      // Clear timeout when operation completes
      const originalResolve = resolve
      const originalReject = reject
      
      pendingCallbacksRef.current.set(id, {
        resolve: (result) => {
          clearTimeout(timeoutId)
          originalResolve(result)
        },
        reject: (error) => {
          clearTimeout(timeoutId)
          originalReject(error)
        },
      })
      
      // Update operation status
      operation.status = 'processing'
      setActiveOperations(Array.from(operationsRef.current.values()))
      
      // Send message to worker
      const message: WorkerMessage = {
        id,
        type,
        data,
        options,
      }
      
      if (debug) {
        console.log(`Starting data processing [${id}]:`, { type, dataLength: data.length, options })
      }
      
      worker.postMessage(message)
    })
  }, [initWorker, maxConcurrent, timeout, debug])
  
  // Convenience methods for different operations
  const aggregate = useCallback((data: any[], options: any = {}) => 
    processData('aggregate', data, options), [processData])
  
  const filter = useCallback((data: any[], options: any = {}) => 
    processData('filter', data, options), [processData])
  
  const sort = useCallback((data: any[], options: any = {}) => 
    processData('sort', data, options), [processData])
  
  const transform = useCallback((data: any[], options: any = {}) => 
    processData('transform', data, options), [processData])
  
  const analyze = useCallback((data: any[], options: any = {}) => 
    processData('analyze', data, options), [processData])
  
  // Clear completed operations
  const clearHistory = useCallback(() => {
    const activeOps = Array.from(operationsRef.current.values())
      .filter(op => op.status === 'pending' || op.status === 'processing')
    
    operationsRef.current.clear()
    activeOps.forEach(op => operationsRef.current.set(op.id, op))
    
    setActiveOperations(activeOps)
  }, [])
  
  // Cancel all pending operations
  const cancelAll = useCallback(() => {
    // Reject all pending callbacks
    pendingCallbacksRef.current.forEach(({ reject }) => {
      reject(new Error('Operation cancelled'))
    })
    pendingCallbacksRef.current.clear()
    
    // Update operations status
    operationsRef.current.forEach(operation => {
      if (operation.status === 'pending' || operation.status === 'processing') {
        operation.status = 'error'
        operation.error = 'Cancelled'
        operation.endTime = Date.now()
      }
    })
    
    setIsProcessing(false)
    setActiveOperations(Array.from(operationsRef.current.values()))
    
    // Terminate and recreate worker
    if (workerRef.current) {
      workerRef.current.terminate()
      workerRef.current = null
    }
  }, [])
  
  // Cleanup on unmount
  React.useEffect(() => {
    return () => {
      if (workerRef.current) {
        workerRef.current.terminate()
      }
    }
  }, [])
  
  return {
    // Processing methods
    processData,
    aggregate,
    filter,
    sort,
    transform,
    analyze,
    
    // State
    isProcessing,
    activeOperations,
    
    // Control methods
    clearHistory,
    cancelAll,
  }
}

export default useDataProcessor
