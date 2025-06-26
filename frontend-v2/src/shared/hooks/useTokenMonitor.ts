import { useEffect, useRef } from 'react'
import { message } from 'antd'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { selectAccessToken, authActions } from '@shared/store/auth'
import { isTokenExpired, getTimeUntilExpiration, willExpireSoon } from '@shared/utils/tokenUtils'

/**
 * Hook to monitor JWT token expiration and handle automatic logout
 * Also provides callbacks for SignalR disconnection when tokens expire
 */
export function useTokenMonitor() {
  const dispatch = useAppDispatch()
  const token = useAppSelector(selectAccessToken)
  const intervalRef = useRef<NodeJS.Timeout | null>(null)
  const callbacksRef = useRef<Set<() => void>>(new Set())

  // Register a callback to be called when token expires
  const onTokenExpired = (callback: () => void) => {
    callbacksRef.current.add(callback)
    
    // Return cleanup function
    return () => {
      callbacksRef.current.delete(callback)
    }
  }

  // Handle token expiration
  const handleTokenExpiration = () => {
    console.warn('🔐 Token expired - logging out and disconnecting services')

    // Show user notification
    message.warning({
      content: 'Your session has expired. Please log in again.',
      duration: 5,
      key: 'token-expired'
    })

    // Call all registered callbacks (e.g., SignalR disconnections)
    callbacksRef.current.forEach(callback => {
      try {
        callback()
      } catch (error) {
        console.error('Error in token expiration callback:', error)
      }
    })

    // Logout user
    dispatch(authActions.logout())
  }

  useEffect(() => {
    // Clear any existing interval
    if (intervalRef.current) {
      clearInterval(intervalRef.current)
      intervalRef.current = null
    }

    if (!token) {
      return
    }

    // Check if token is already expired
    if (isTokenExpired(token)) {
      console.warn('🔐 Token is already expired')
      handleTokenExpiration()
      return
    }

    // Set up monitoring interval
    intervalRef.current = setInterval(() => {
      if (!token) {
        return
      }

      // Check if token is expired
      if (isTokenExpired(token)) {
        handleTokenExpiration()
        return
      }

      // Show warning if token will expire soon
      if (willExpireSoon(token, 10)) {
        const timeLeft = getTimeUntilExpiration(token)
        const minutesLeft = Math.floor(timeLeft / (1000 * 60))
        console.warn(`🔐 Token will expire in ${minutesLeft} minutes`)

        // Show user notification for tokens expiring in 5 minutes or less
        if (minutesLeft <= 5 && minutesLeft > 0) {
          message.warning({
            content: `Your session will expire in ${minutesLeft} minute${minutesLeft !== 1 ? 's' : ''}. Please save your work.`,
            duration: 8,
            key: 'token-expiring-soon'
          })
        }
      }
    }, 30000) // Check every 30 seconds

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
        intervalRef.current = null
      }
    }
  }, [token, dispatch])

  return {
    onTokenExpired,
    isTokenValid: token ? !isTokenExpired(token) : false,
    willExpireSoon: token ? willExpireSoon(token, 5) : false,
    timeUntilExpiration: token ? getTimeUntilExpiration(token) : 0,
  }
}

/**
 * Hook specifically for SignalR services to auto-disconnect on token expiration
 */
export function useSignalRTokenMonitor(disconnectCallback: () => void) {
  const { onTokenExpired } = useTokenMonitor()

  useEffect(() => {
    const cleanup = onTokenExpired(() => {
      console.log('🔌 Disconnecting SignalR due to token expiration')
      disconnectCallback()
    })

    return cleanup
  }, [disconnectCallback, onTokenExpired])
}
