// shared/hooks/useRealTimeUpdates.ts
import { useEffect } from 'react'
import { io, Socket } from 'socket.io-client'

export const useRealTimeUpdates = () => {
  const [socket, setSocket] = useState<Socket | null>(null)
  
  useEffect(() => {
    const newSocket = io('/cost-monitoring', {
      auth: {
        token: getAuthToken()
      }
    })
    
    setSocket(newSocket)
    
    // Listen for cost updates
    newSocket.on('cost-update', (data) => {
      // Invalidate relevant queries
      store.dispatch(costManagementApi.util.invalidateTags(['CostAnalytics']))
    })
    
    // Listen for performance alerts
    newSocket.on('performance-alert', (alert) => {
      // Show notification
      showNotification({
        type: 'warning',
        title: 'Performance Alert',
        message: alert.message
      })
    })
    
    return () => newSocket.close()
  }, [])
  
  return socket
}
