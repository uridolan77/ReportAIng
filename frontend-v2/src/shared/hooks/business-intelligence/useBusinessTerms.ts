import { useQuery } from '@tanstack/react-query'

export const useBusinessTerms = () => {
  return useQuery({
    queryKey: ['business-terms'],
    queryFn: () => Promise.resolve([]),
    enabled: false // Disabled until backend is ready
  })
}
