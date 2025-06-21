/**
 * PWA Manager Component
 * 
 * Manages Progressive Web App features:
 * - Service worker registration
 * - Install prompt handling
 * - Offline status detection
 * - Background sync management
 * - Push notification setup
 */

import React, { useState, useEffect, useCallback, createContext, useContext } from 'react'
import { Button, notification, Modal, Space, Typography, Alert } from 'antd'
import { 
  DownloadOutlined, 
  WifiOutlined, 
  DisconnectOutlined,
  SyncOutlined,
  BellOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons'

const { Text, Title } = Typography

export interface PWAContextType {
  isOnline: boolean
  isInstallable: boolean
  isInstalled: boolean
  isServiceWorkerReady: boolean
  promptInstall: () => void
  enableNotifications: () => Promise<boolean>
  syncOfflineData: () => void
  clearCache: () => Promise<void>
  updateAvailable: boolean
  updateApp: () => void
}

const PWAContext = createContext<PWAContextType | null>(null)

export const usePWA = () => {
  const context = useContext(PWAContext)
  if (!context) {
    throw new Error('usePWA must be used within PWAManager')
  }
  return context
}

interface PWAManagerProps {
  children: React.ReactNode
  /** Enable automatic service worker updates */
  autoUpdate?: boolean
  /** Show install prompt automatically */
  autoPromptInstall?: boolean
  /** Enable push notifications */
  enablePushNotifications?: boolean
}

export const PWAManager: React.FC<PWAManagerProps> = ({
  children,
  autoUpdate = true,
  autoPromptInstall = false,
  enablePushNotifications = true,
}) => {
  const [isOnline, setIsOnline] = useState(navigator.onLine)
  const [isInstallable, setIsInstallable] = useState(false)
  const [isInstalled, setIsInstalled] = useState(false)
  const [isServiceWorkerReady, setIsServiceWorkerReady] = useState(false)
  const [updateAvailable, setUpdateAvailable] = useState(false)
  const [deferredPrompt, setDeferredPrompt] = useState<any>(null)
  const [serviceWorker, setServiceWorker] = useState<ServiceWorkerRegistration | null>(null)

  // Check if app is installed
  useEffect(() => {
    const checkInstalled = () => {
      const isStandalone = window.matchMedia('(display-mode: standalone)').matches
      const isInWebAppiOS = (window.navigator as any).standalone === true
      setIsInstalled(isStandalone || isInWebAppiOS)
    }

    checkInstalled()
    window.matchMedia('(display-mode: standalone)').addEventListener('change', checkInstalled)

    return () => {
      window.matchMedia('(display-mode: standalone)').removeEventListener('change', checkInstalled)
    }
  }, [])

  // Monitor online/offline status
  useEffect(() => {
    const handleOnline = () => {
      setIsOnline(true)
      notification.success({
        message: 'Back Online',
        description: 'Your connection has been restored. Syncing data...',
        icon: <WifiOutlined style={{ color: '#52c41a' }} />,
      })
      syncOfflineData()
    }

    const handleOffline = () => {
      setIsOnline(false)
      notification.warning({
        message: 'You\'re Offline',
        description: 'Some features may be limited. Data will sync when you\'re back online.',
        icon: <DisconnectOutlined style={{ color: '#faad14' }} />,
      })
    }

    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)

    return () => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [])

  // Handle install prompt
  useEffect(() => {
    const handleBeforeInstallPrompt = (e: Event) => {
      e.preventDefault()
      setDeferredPrompt(e)
      setIsInstallable(true)

      if (autoPromptInstall && !isInstalled) {
        setTimeout(() => {
          showInstallPrompt()
        }, 5000) // Show after 5 seconds
      }
    }

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt)

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt)
    }
  }, [autoPromptInstall, isInstalled])

  // Register service worker
  useEffect(() => {
    if ('serviceWorker' in navigator) {
      registerServiceWorker()
    }
  }, [])

  const registerServiceWorker = async () => {
    try {
      const registration = await navigator.serviceWorker.register('/sw.js')
      setServiceWorker(registration)
      setIsServiceWorkerReady(true)

      console.log('[PWA] Service worker registered:', registration)

      // Check for updates
      registration.addEventListener('updatefound', () => {
        const newWorker = registration.installing
        if (newWorker) {
          newWorker.addEventListener('statechange', () => {
            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
              setUpdateAvailable(true)
              
              if (autoUpdate) {
                updateApp()
              } else {
                notification.info({
                  message: 'Update Available',
                  description: 'A new version of the app is available.',
                  btn: (
                    <Button type="primary" size="small" onClick={updateApp}>
                      Update Now
                    </Button>
                  ),
                  duration: 0,
                })
              }
            }
          })
        }
      })

      // Listen for messages from service worker
      navigator.serviceWorker.addEventListener('message', (event) => {
        console.log('[PWA] Message from service worker:', event.data)
        
        if (event.data.type === 'SYNC_OFFLINE_QUERIES') {
          notification.info({
            message: 'Syncing Data',
            description: 'Synchronizing offline queries...',
            icon: <SyncOutlined spin />,
          })
        }
      })

    } catch (error) {
      console.error('[PWA] Service worker registration failed:', error)
    }
  }

  const showInstallPrompt = () => {
    Modal.confirm({
      title: 'Install BI Copilot',
      content: (
        <Space direction="vertical">
          <Text>Install BI Copilot for a better experience:</Text>
          <ul>
            <li>Faster loading times</li>
            <li>Offline access to cached data</li>
            <li>Desktop app experience</li>
            <li>Push notifications for insights</li>
          </ul>
        </Space>
      ),
      icon: <DownloadOutlined />,
      okText: 'Install',
      cancelText: 'Maybe Later',
      onOk: promptInstall,
    })
  }

  const promptInstall = useCallback(async () => {
    if (!deferredPrompt) return

    try {
      deferredPrompt.prompt()
      const { outcome } = await deferredPrompt.userChoice
      
      if (outcome === 'accepted') {
        notification.success({
          message: 'App Installed',
          description: 'BI Copilot has been installed successfully!',
          icon: <CheckCircleOutlined style={{ color: '#52c41a' }} />,
        })
        setIsInstalled(true)
      }
      
      setDeferredPrompt(null)
      setIsInstallable(false)
    } catch (error) {
      console.error('[PWA] Install prompt failed:', error)
    }
  }, [deferredPrompt])

  const enableNotifications = useCallback(async (): Promise<boolean> => {
    if (!enablePushNotifications || !('Notification' in window)) {
      return false
    }

    try {
      const permission = await Notification.requestPermission()
      
      if (permission === 'granted') {
        notification.success({
          message: 'Notifications Enabled',
          description: 'You\'ll receive notifications for important insights and updates.',
          icon: <BellOutlined style={{ color: '#52c41a' }} />,
        })

        // Subscribe to push notifications if service worker is ready
        if (serviceWorker) {
          const vapidKey = (typeof process !== 'undefined' && process.env?.REACT_APP_VAPID_PUBLIC_KEY) || undefined

          if (!vapidKey) {
            console.warn('[PWA] VAPID public key not configured, push notifications may not work')
          }

          const subscription = await serviceWorker.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: vapidKey,
          })

          // Send subscription to server
          await fetch('/api/notifications/subscribe', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(subscription),
          })
        }

        return true
      } else {
        notification.warning({
          message: 'Notifications Blocked',
          description: 'Enable notifications in your browser settings to receive updates.',
        })
        return false
      }
    } catch (error) {
      console.error('[PWA] Failed to enable notifications:', error)
      return false
    }
  }, [enablePushNotifications, serviceWorker])

  const syncOfflineData = useCallback(() => {
    if (serviceWorker && 'sync' in window.ServiceWorkerRegistration.prototype) {
      serviceWorker.sync.register('background-sync-queries')
      serviceWorker.sync.register('background-sync-analytics')
    }
  }, [serviceWorker])

  const clearCache = useCallback(async () => {
    try {
      if ('caches' in window) {
        const cacheNames = await caches.keys()
        await Promise.all(cacheNames.map(name => caches.delete(name)))
        
        notification.success({
          message: 'Cache Cleared',
          description: 'Application cache has been cleared successfully.',
        })
      }
    } catch (error) {
      console.error('[PWA] Failed to clear cache:', error)
      notification.error({
        message: 'Cache Clear Failed',
        description: 'Failed to clear application cache.',
      })
    }
  }, [])

  const updateApp = useCallback(() => {
    if (serviceWorker && serviceWorker.waiting) {
      serviceWorker.waiting.postMessage({ type: 'SKIP_WAITING' })
      window.location.reload()
    }
  }, [serviceWorker])

  const contextValue: PWAContextType = {
    isOnline,
    isInstallable,
    isInstalled,
    isServiceWorkerReady,
    promptInstall,
    enableNotifications,
    syncOfflineData,
    clearCache,
    updateAvailable,
    updateApp,
  }

  return (
    <PWAContext.Provider value={contextValue}>
      {children}
      
      {/* Offline indicator */}
      {!isOnline && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          zIndex: 9999,
        }}>
          <Alert
            message="You're currently offline"
            description="Some features may be limited. Data will sync when you're back online."
            type="warning"
            showIcon
            icon={<DisconnectOutlined />}
            banner
          />
        </div>
      )}
    </PWAContext.Provider>
  )
}

// Install button component
export const InstallButton: React.FC<{ style?: React.CSSProperties }> = ({ style }) => {
  const { isInstallable, isInstalled, promptInstall } = usePWA()

  if (isInstalled || !isInstallable) {
    return null
  }

  return (
    <Button
      type="primary"
      icon={<DownloadOutlined />}
      onClick={promptInstall}
      style={style}
    >
      Install App
    </Button>
  )
}

// Offline status component
export const OfflineStatus: React.FC = () => {
  const { isOnline } = usePWA()

  return (
    <Space>
      {isOnline ? (
        <WifiOutlined style={{ color: '#52c41a' }} />
      ) : (
        <DisconnectOutlined style={{ color: '#faad14' }} />
      )}
      <Text type={isOnline ? 'success' : 'warning'}>
        {isOnline ? 'Online' : 'Offline'}
      </Text>
    </Space>
  )
}

export default PWAManager
