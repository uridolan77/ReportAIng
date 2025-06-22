import { useState, useEffect, useCallback } from 'react'
import type { AIFeatureFlags } from '@shared/types/ai'
import type { UseAIFeatureFlagsResult } from '../types'

// Default feature flags - can be overridden by API or environment
const DEFAULT_FEATURE_FLAGS: AIFeatureFlags = {
  transparencyPanelEnabled: true,
  streamingProcessingEnabled: true,
  advancedAnalyticsEnabled: true,
  llmManagementEnabled: true,
  businessContextEnabled: true,
  costOptimizationEnabled: true,
  predictiveAnalyticsEnabled: false // Beta feature
}

/**
 * Hook for managing AI feature flags
 * 
 * This hook provides:
 * - Feature flag state management
 * - API integration for remote flags
 * - Local storage caching
 * - Real-time updates
 * - Error handling
 */
export const useAIFeatureFlags = (): UseAIFeatureFlagsResult => {
  const [flags, setFlags] = useState<AIFeatureFlags>(DEFAULT_FEATURE_FLAGS)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string>()

  // Check if a specific feature is enabled
  const isFeatureEnabled = useCallback((feature: keyof AIFeatureFlags): boolean => {
    return flags[feature] ?? false
  }, [flags])

  // Load feature flags from API
  const loadFeatureFlags = useCallback(async () => {
    setLoading(true)
    setError(undefined)

    try {
      // Try to load from localStorage first (cache)
      const cachedFlags = localStorage.getItem('ai-feature-flags')
      if (cachedFlags) {
        const parsed = JSON.parse(cachedFlags)
        setFlags({ ...DEFAULT_FEATURE_FLAGS, ...parsed })
      }

      // TODO: Replace with actual API call when endpoint is available
      // const response = await fetch('/api/ai/feature-flags')
      // const apiFlags = await response.json()
      
      // For now, use environment variables or defaults
      const envFlags: Partial<AIFeatureFlags> = {
        transparencyPanelEnabled: process.env.REACT_APP_AI_TRANSPARENCY_ENABLED !== 'false',
        streamingProcessingEnabled: process.env.REACT_APP_AI_STREAMING_ENABLED !== 'false',
        advancedAnalyticsEnabled: process.env.REACT_APP_AI_ANALYTICS_ENABLED !== 'false',
        llmManagementEnabled: process.env.REACT_APP_LLM_MANAGEMENT_ENABLED !== 'false',
        businessContextEnabled: process.env.REACT_APP_BUSINESS_CONTEXT_ENABLED !== 'false',
        costOptimizationEnabled: process.env.REACT_APP_COST_OPTIMIZATION_ENABLED !== 'false',
        predictiveAnalyticsEnabled: process.env.REACT_APP_PREDICTIVE_ANALYTICS_ENABLED === 'true'
      }

      const finalFlags = { ...DEFAULT_FEATURE_FLAGS, ...envFlags }
      setFlags(finalFlags)

      // Cache the flags
      localStorage.setItem('ai-feature-flags', JSON.stringify(finalFlags))

    } catch (err) {
      console.error('Failed to load AI feature flags:', err)
      setError(err instanceof Error ? err.message : 'Failed to load feature flags')
      
      // Fall back to cached flags or defaults
      const cachedFlags = localStorage.getItem('ai-feature-flags')
      if (cachedFlags) {
        try {
          const parsed = JSON.parse(cachedFlags)
          setFlags({ ...DEFAULT_FEATURE_FLAGS, ...parsed })
        } catch {
          setFlags(DEFAULT_FEATURE_FLAGS)
        }
      } else {
        setFlags(DEFAULT_FEATURE_FLAGS)
      }
    } finally {
      setLoading(false)
    }
  }, [])

  // Refresh flags from API
  const refreshFlags = useCallback(async () => {
    await loadFeatureFlags()
  }, [loadFeatureFlags])

  // Load flags on mount
  useEffect(() => {
    loadFeatureFlags()
  }, [loadFeatureFlags])

  // Set up periodic refresh (every 5 minutes)
  useEffect(() => {
    const interval = setInterval(() => {
      loadFeatureFlags()
    }, 5 * 60 * 1000) // 5 minutes

    return () => clearInterval(interval)
  }, [loadFeatureFlags])

  // Listen for storage changes (multi-tab sync)
  useEffect(() => {
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'ai-feature-flags' && e.newValue) {
        try {
          const newFlags = JSON.parse(e.newValue)
          setFlags({ ...DEFAULT_FEATURE_FLAGS, ...newFlags })
        } catch (err) {
          console.error('Failed to parse feature flags from storage:', err)
        }
      }
    }

    window.addEventListener('storage', handleStorageChange)
    return () => window.removeEventListener('storage', handleStorageChange)
  }, [])

  return {
    flags,
    isFeatureEnabled,
    loading,
    error,
    refreshFlags
  }
}

/**
 * Hook for checking a specific feature flag
 */
export const useFeatureFlag = (feature: keyof AIFeatureFlags): boolean => {
  const { isFeatureEnabled } = useAIFeatureFlags()
  return isFeatureEnabled(feature)
}

/**
 * Hook for multiple feature flags
 */
export const useFeatureFlags = (features: (keyof AIFeatureFlags)[]): Record<string, boolean> => {
  const { flags } = useAIFeatureFlags()
  
  return features.reduce((acc, feature) => {
    acc[feature] = flags[feature] ?? false
    return acc
  }, {} as Record<string, boolean>)
}

export default useAIFeatureFlags
