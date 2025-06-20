import { useDispatch, useSelector } from 'react-redux'
import type { TypedUseSelectorHook } from 'react-redux'
import type { RootState, AppDispatch } from '../store'

// Use throughout your app instead of plain `useDispatch` and `useSelector`
export const useAppDispatch = () => useDispatch<AppDispatch>()
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector

// Re-export commonly used hooks
export { useCallback, useEffect, useMemo, useState } from 'react'
export { useNavigate, useLocation, useParams } from 'react-router-dom'

// Cost Management Hooks
export {
  useCostMetrics,
  useCostBreakdown,
  useCostAlerts,
  useCostEfficiency
} from './useCostMetrics'

// Performance Monitoring Hooks
export {
  usePerformanceMonitoring,
  usePerformanceAlerts,
  usePerformanceBenchmarks,
  usePerformanceScore,
  usePerformanceComparison
} from './usePerformanceMonitoring'
