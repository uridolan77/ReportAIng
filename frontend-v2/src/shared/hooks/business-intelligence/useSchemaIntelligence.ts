import { useQuery } from '@tanstack/react-query'

export const useSchemaIntelligence = () => {
  return useQuery({
    queryKey: ['schema-intelligence'],
    queryFn: () => Promise.resolve(null),
    enabled: false // Disabled until backend is ready
  })
}
