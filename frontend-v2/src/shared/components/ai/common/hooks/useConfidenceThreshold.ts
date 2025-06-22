import { useState, useCallback, useEffect } from 'react'
import type { UseConfidenceThresholdResult } from '../types'

// Default confidence threshold (60%)
const DEFAULT_THRESHOLD = 0.6

// Confidence level thresholds
const CONFIDENCE_THRESHOLDS = {
  high: 0.8,
  medium: 0.6,
  low: 0.0
}

// Confidence colors
const CONFIDENCE_COLORS = {
  high: '#52c41a',    // Green
  medium: '#faad14',  // Orange
  low: '#ff4d4f'      // Red
}

/**
 * Hook for managing confidence thresholds and calculations
 * 
 * This hook provides:
 * - Configurable confidence thresholds
 * - Confidence level calculations
 * - Color coding for confidence levels
 * - Persistence of user preferences
 * - Validation and bounds checking
 */
export const useConfidenceThreshold = (): UseConfidenceThresholdResult => {
  const [threshold, setThresholdState] = useState<number>(DEFAULT_THRESHOLD)

  // Load threshold from localStorage on mount
  useEffect(() => {
    const savedThreshold = localStorage.getItem('ai-confidence-threshold')
    if (savedThreshold) {
      const parsed = parseFloat(savedThreshold)
      if (!isNaN(parsed) && parsed >= 0 && parsed <= 1) {
        setThresholdState(parsed)
      }
    }
  }, [])

  // Set threshold with validation and persistence
  const setThreshold = useCallback((newThreshold: number) => {
    // Validate threshold bounds
    const validThreshold = Math.max(0, Math.min(1, newThreshold))
    
    setThresholdState(validThreshold)
    localStorage.setItem('ai-confidence-threshold', validThreshold.toString())
  }, [])

  // Check if confidence is above threshold
  const isAboveThreshold = useCallback((confidence: number): boolean => {
    return confidence >= threshold
  }, [threshold])

  // Get confidence level (high/medium/low)
  const getConfidenceLevel = useCallback((confidence: number): 'high' | 'medium' | 'low' => {
    if (confidence >= CONFIDENCE_THRESHOLDS.high) return 'high'
    if (confidence >= CONFIDENCE_THRESHOLDS.medium) return 'medium'
    return 'low'
  }, [])

  // Get color for confidence level
  const getConfidenceColor = useCallback((confidence: number): string => {
    const level = getConfidenceLevel(confidence)
    return CONFIDENCE_COLORS[level]
  }, [getConfidenceLevel])

  return {
    threshold,
    setThreshold,
    isAboveThreshold,
    getConfidenceLevel,
    getConfidenceColor
  }
}

/**
 * Hook for confidence statistics and analysis
 */
export const useConfidenceStats = (confidenceValues: number[]) => {
  const { getConfidenceLevel, getConfidenceColor } = useConfidenceThreshold()

  const stats = {
    count: confidenceValues.length,
    average: confidenceValues.length > 0 
      ? confidenceValues.reduce((sum, val) => sum + val, 0) / confidenceValues.length 
      : 0,
    min: confidenceValues.length > 0 ? Math.min(...confidenceValues) : 0,
    max: confidenceValues.length > 0 ? Math.max(...confidenceValues) : 0,
    distribution: {
      high: confidenceValues.filter(val => getConfidenceLevel(val) === 'high').length,
      medium: confidenceValues.filter(val => getConfidenceLevel(val) === 'medium').length,
      low: confidenceValues.filter(val => getConfidenceLevel(val) === 'low').length
    }
  }

  return {
    ...stats,
    averageLevel: getConfidenceLevel(stats.average),
    averageColor: getConfidenceColor(stats.average)
  }
}

/**
 * Hook for confidence validation and warnings
 */
export const useConfidenceValidation = () => {
  const { threshold, getConfidenceLevel } = useConfidenceThreshold()

  const validateConfidence = useCallback((confidence: number) => {
    const warnings: string[] = []
    const level = getConfidenceLevel(confidence)

    if (confidence < threshold) {
      warnings.push(`Confidence (${(confidence * 100).toFixed(1)}%) is below threshold (${(threshold * 100).toFixed(1)}%)`)
    }

    if (level === 'low') {
      warnings.push('Low confidence result - consider manual verification')
    }

    if (confidence < 0.3) {
      warnings.push('Very low confidence - result may be unreliable')
    }

    return {
      isValid: confidence >= threshold,
      level,
      warnings,
      recommendation: warnings.length > 0 
        ? 'Consider reviewing the input or trying a different approach'
        : 'Confidence level is acceptable'
    }
  }, [threshold, getConfidenceLevel])

  return { validateConfidence }
}

/**
 * Hook for confidence trend analysis
 */
export const useConfidenceTrend = (confidenceHistory: { value: number; timestamp: string }[]) => {
  const trend = confidenceHistory.length >= 2 
    ? confidenceHistory[confidenceHistory.length - 1].value - confidenceHistory[0].value
    : 0

  const direction = trend > 0.05 ? 'improving' : trend < -0.05 ? 'declining' : 'stable'
  
  const recentAverage = confidenceHistory.length > 0
    ? confidenceHistory.slice(-5).reduce((sum, item) => sum + item.value, 0) / Math.min(5, confidenceHistory.length)
    : 0

  return {
    trend,
    direction,
    recentAverage,
    isImproving: direction === 'improving',
    isStable: direction === 'stable',
    isDeclining: direction === 'declining'
  }
}

export default useConfidenceThreshold
