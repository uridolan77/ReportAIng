import { useQuery } from '@tanstack/react-query'

export const useQueryHistory = () => {
  return useQuery({
    queryKey: ['query-history'],
    queryFn: () => Promise.resolve([]),
    enabled: false // Disabled until backend is ready
  })
}
