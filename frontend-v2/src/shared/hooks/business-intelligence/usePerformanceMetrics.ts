import { useQuery } from '@tanstack/react-query'

export const usePerformanceMetrics = () => {
  return useQuery({
    queryKey: ['performance-metrics'],
    queryFn: () => Promise.resolve(null),
    enabled: false // Disabled until backend is ready
  })
}
