/**
 * API Toggle Service
 * 
 * Provides a centralized way to toggle between real API calls and mock data.
 * Supports runtime toggling and environment-based configuration.
 */

import { MockDataService } from './mockDataService'

export interface ApiToggleConfig {
  useMockData: boolean
  mockDelay: number
  debugMode: boolean
  fallbackToMock: boolean // Fall back to mock if real API fails
}

class ApiToggleServiceClass {
  private config: ApiToggleConfig = {
    useMockData: import.meta.env.DEV && import.meta.env.VITE_USE_MOCK_DATA !== 'false',
    mockDelay: Number(import.meta.env.VITE_MOCK_DELAY) || 500,
    debugMode: import.meta.env.DEV,
    fallbackToMock: import.meta.env.VITE_FALLBACK_TO_MOCK !== 'false'
  }

  private listeners: Array<(config: ApiToggleConfig) => void> = []

  /**
   * Get current configuration
   */
  getConfig(): ApiToggleConfig {
    return { ...this.config }
  }

  /**
   * Update configuration
   */
  updateConfig(updates: Partial<ApiToggleConfig>): void {
    this.config = { ...this.config, ...updates }
    this.notifyListeners()
    
    if (this.config.debugMode) {
      console.log('🔄 API Toggle Config Updated:', this.config)
    }
  }

  /**
   * Toggle between mock and real API
   */
  toggleMockData(): void {
    this.updateConfig({ useMockData: !this.config.useMockData })
  }

  /**
   * Check if mock data should be used
   */
  shouldUseMockData(): boolean {
    return this.config.useMockData
  }

  /**
   * Execute API call with fallback logic
   */
  async executeWithFallback<T>(
    realApiCall: () => Promise<T>,
    mockDataCall: () => Promise<T>,
    endpoint?: string
  ): Promise<T> {
    if (this.config.useMockData) {
      if (this.config.debugMode) {
        console.log(`🎭 Using mock data for: ${endpoint || 'unknown endpoint'}`)
      }
      return mockDataCall()
    }

    try {
      if (this.config.debugMode) {
        console.log(`🌐 Using real API for: ${endpoint || 'unknown endpoint'}`)
      }
      return await realApiCall()
    } catch (error) {
      if (this.config.fallbackToMock) {
        console.warn(`⚠️ Real API failed for ${endpoint}, falling back to mock data:`, error)
        return mockDataCall()
      }
      throw error
    }
  }

  /**
   * Add listener for configuration changes
   */
  addListener(listener: (config: ApiToggleConfig) => void): () => void {
    this.listeners.push(listener)
    
    // Return unsubscribe function
    return () => {
      const index = this.listeners.indexOf(listener)
      if (index > -1) {
        this.listeners.splice(index, 1)
      }
    }
  }

  /**
   * Notify all listeners of configuration changes
   */
  private notifyListeners(): void {
    this.listeners.forEach(listener => {
      try {
        listener(this.config)
      } catch (error) {
        console.error('Error in API toggle listener:', error)
      }
    })
  }

  /**
   * Get status information for debugging
   */
  getStatus(): {
    isUsingMockData: boolean
    mockDataAvailable: boolean
    config: ApiToggleConfig
    environment: string
  } {
    return {
      isUsingMockData: this.config.useMockData,
      mockDataAvailable: MockDataService.isEnabled(),
      config: this.config,
      environment: import.meta.env.MODE || 'unknown'
    }
  }

  /**
   * Reset to default configuration
   */
  reset(): void {
    this.config = {
      useMockData: import.meta.env.DEV && import.meta.env.VITE_USE_MOCK_DATA !== 'false',
      mockDelay: Number(import.meta.env.VITE_MOCK_DELAY) || 500,
      debugMode: import.meta.env.DEV,
      fallbackToMock: import.meta.env.VITE_FALLBACK_TO_MOCK !== 'false'
    }
    this.notifyListeners()
  }
}

// Export singleton instance
export const ApiToggleService = new ApiToggleServiceClass()

// React hook for using API toggle in components
import { useState, useEffect } from 'react'

export function useApiToggle() {
  const [config, setConfig] = useState(ApiToggleService.getConfig())

  useEffect(() => {
    const unsubscribe = ApiToggleService.addListener(setConfig)
    return unsubscribe
  }, [])

  return {
    config,
    isUsingMockData: config.useMockData,
    toggleMockData: () => ApiToggleService.toggleMockData(),
    updateConfig: (updates: Partial<ApiToggleConfig>) => ApiToggleService.updateConfig(updates),
    status: ApiToggleService.getStatus()
  }
}

// Utility function for API calls with automatic fallback
export async function apiCall<T>(
  realApiCall: () => Promise<T>,
  mockDataCall: () => Promise<T>,
  endpoint?: string
): Promise<T> {
  return ApiToggleService.executeWithFallback(realApiCall, mockDataCall, endpoint)
}

// Development tools for runtime toggling
if (import.meta.env.DEV) {
  // Add global access for debugging
  (window as any).__apiToggle = {
    service: ApiToggleService,
    toggle: () => ApiToggleService.toggleMockData(),
    status: () => ApiToggleService.getStatus(),
    useMock: () => ApiToggleService.updateConfig({ useMockData: true }),
    useReal: () => ApiToggleService.updateConfig({ useMockData: false }),
    enableFallback: () => ApiToggleService.updateConfig({ fallbackToMock: true }),
    disableFallback: () => ApiToggleService.updateConfig({ fallbackToMock: false })
  }

  console.log('🔧 API Toggle Service available at window.__apiToggle')
  console.log('🎭 Current status:', ApiToggleService.getStatus())
}
