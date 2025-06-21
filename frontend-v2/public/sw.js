/**
 * Service Worker for BI Reporting Copilot
 * 
 * Provides:
 * - Offline functionality
 * - Background sync
 * - Push notifications
 * - Cache management
 * - Performance optimization
 */

const CACHE_NAME = 'bi-copilot-v1.0.0'
const STATIC_CACHE = 'bi-copilot-static-v1.0.0'
const DYNAMIC_CACHE = 'bi-copilot-dynamic-v1.0.0'
const API_CACHE = 'bi-copilot-api-v1.0.0'

// Assets to cache immediately
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/manifest.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png',
  // Add other critical assets
]

// API endpoints to cache
const API_ENDPOINTS = [
  '/api/dashboard/stats',
  '/api/user/profile',
  '/api/system/health',
]

// Install event - cache static assets
self.addEventListener('install', (event) => {
  console.log('[SW] Installing service worker...')
  
  event.waitUntil(
    Promise.all([
      // Cache static assets
      caches.open(STATIC_CACHE).then((cache) => {
        console.log('[SW] Caching static assets')
        return cache.addAll(STATIC_ASSETS)
      }),
      
      // Skip waiting to activate immediately
      self.skipWaiting()
    ])
  )
})

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  console.log('[SW] Activating service worker...')
  
  event.waitUntil(
    Promise.all([
      // Clean up old caches
      caches.keys().then((cacheNames) => {
        return Promise.all(
          cacheNames.map((cacheName) => {
            if (cacheName !== CACHE_NAME && 
                cacheName !== STATIC_CACHE && 
                cacheName !== DYNAMIC_CACHE && 
                cacheName !== API_CACHE) {
              console.log('[SW] Deleting old cache:', cacheName)
              return caches.delete(cacheName)
            }
          })
        )
      }),
      
      // Take control of all clients
      self.clients.claim()
    ])
  )
})

// Fetch event - handle requests with caching strategies
self.addEventListener('fetch', (event) => {
  const { request } = event
  const url = new URL(request.url)
  
  // Skip non-GET requests
  if (request.method !== 'GET') {
    return
  }
  
  // Handle different types of requests
  if (url.pathname.startsWith('/api/')) {
    // API requests - Network First with fallback
    event.respondWith(handleApiRequest(request))
  } else if (url.pathname.match(/\.(js|css|png|jpg|jpeg|gif|svg|woff|woff2)$/)) {
    // Static assets - Cache First
    event.respondWith(handleStaticAsset(request))
  } else {
    // HTML pages - Stale While Revalidate
    event.respondWith(handlePageRequest(request))
  }
})

// Network First strategy for API requests
async function handleApiRequest(request) {
  const cache = await caches.open(API_CACHE)
  
  try {
    // Try network first
    const networkResponse = await fetch(request)
    
    if (networkResponse.ok) {
      // Cache successful responses
      cache.put(request, networkResponse.clone())
    }
    
    return networkResponse
  } catch (error) {
    console.log('[SW] Network failed for API request, trying cache:', request.url)
    
    // Fallback to cache
    const cachedResponse = await cache.match(request)
    if (cachedResponse) {
      return cachedResponse
    }
    
    // Return offline response for critical endpoints
    if (API_ENDPOINTS.some(endpoint => request.url.includes(endpoint))) {
      return new Response(
        JSON.stringify({
          error: 'Offline',
          message: 'This data is not available offline',
          cached: false
        }),
        {
          status: 503,
          statusText: 'Service Unavailable',
          headers: { 'Content-Type': 'application/json' }
        }
      )
    }
    
    throw error
  }
}

// Cache First strategy for static assets
async function handleStaticAsset(request) {
  const cache = await caches.open(STATIC_CACHE)
  const cachedResponse = await cache.match(request)
  
  if (cachedResponse) {
    return cachedResponse
  }
  
  try {
    const networkResponse = await fetch(request)
    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone())
    }
    return networkResponse
  } catch (error) {
    console.log('[SW] Failed to fetch static asset:', request.url)
    throw error
  }
}

// Stale While Revalidate strategy for pages
async function handlePageRequest(request) {
  const cache = await caches.open(DYNAMIC_CACHE)
  const cachedResponse = await cache.match(request)
  
  // Fetch from network in background
  const networkPromise = fetch(request).then((networkResponse) => {
    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone())
    }
    return networkResponse
  }).catch(() => {
    // Network failed, return cached version if available
    return cachedResponse
  })
  
  // Return cached version immediately if available
  if (cachedResponse) {
    return cachedResponse
  }
  
  // Otherwise wait for network
  return networkPromise
}

// Background sync for offline actions
self.addEventListener('sync', (event) => {
  console.log('[SW] Background sync triggered:', event.tag)
  
  if (event.tag === 'background-sync-queries') {
    event.waitUntil(syncOfflineQueries())
  } else if (event.tag === 'background-sync-analytics') {
    event.waitUntil(syncOfflineAnalytics())
  }
})

// Sync offline queries when back online
async function syncOfflineQueries() {
  try {
    const clients = await self.clients.matchAll()
    
    clients.forEach(client => {
      client.postMessage({
        type: 'SYNC_OFFLINE_QUERIES',
        timestamp: Date.now()
      })
    })
    
    console.log('[SW] Offline queries sync completed')
  } catch (error) {
    console.error('[SW] Failed to sync offline queries:', error)
  }
}

// Sync offline analytics when back online
async function syncOfflineAnalytics() {
  try {
    const clients = await self.clients.matchAll()
    
    clients.forEach(client => {
      client.postMessage({
        type: 'SYNC_OFFLINE_ANALYTICS',
        timestamp: Date.now()
      })
    })
    
    console.log('[SW] Offline analytics sync completed')
  } catch (error) {
    console.error('[SW] Failed to sync offline analytics:', error)
  }
}

// Push notification handling
self.addEventListener('push', (event) => {
  console.log('[SW] Push notification received')
  
  const options = {
    body: 'You have new insights available',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    vibrate: [100, 50, 100],
    data: {
      dateOfArrival: Date.now(),
      primaryKey: 1
    },
    actions: [
      {
        action: 'explore',
        title: 'View Dashboard',
        icon: '/icons/action-dashboard.png'
      },
      {
        action: 'close',
        title: 'Close',
        icon: '/icons/action-close.png'
      }
    ]
  }
  
  if (event.data) {
    const data = event.data.json()
    options.body = data.body || options.body
    options.data = { ...options.data, ...data }
  }
  
  event.waitUntil(
    self.registration.showNotification('BI Copilot', options)
  )
})

// Notification click handling
self.addEventListener('notificationclick', (event) => {
  console.log('[SW] Notification clicked:', event.action)
  
  event.notification.close()
  
  if (event.action === 'explore') {
    event.waitUntil(
      clients.openWindow('/admin/dashboard')
    )
  } else if (event.action === 'close') {
    // Just close the notification
    return
  } else {
    // Default action - open the app
    event.waitUntil(
      clients.openWindow('/')
    )
  }
})

// Message handling from main thread
self.addEventListener('message', (event) => {
  console.log('[SW] Message received:', event.data)
  
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting()
  } else if (event.data && event.data.type === 'CACHE_URLS') {
    event.waitUntil(
      cacheUrls(event.data.urls)
    )
  } else if (event.data && event.data.type === 'CLEAR_CACHE') {
    event.waitUntil(
      clearCache(event.data.cacheName)
    )
  }
})

// Cache specific URLs
async function cacheUrls(urls) {
  const cache = await caches.open(DYNAMIC_CACHE)
  return cache.addAll(urls)
}

// Clear specific cache
async function clearCache(cacheName) {
  return caches.delete(cacheName)
}

// Periodic background sync (if supported)
self.addEventListener('periodicsync', (event) => {
  console.log('[SW] Periodic sync triggered:', event.tag)
  
  if (event.tag === 'analytics-sync') {
    event.waitUntil(syncAnalytics())
  }
})

// Sync analytics data periodically
async function syncAnalytics() {
  try {
    // Fetch latest analytics data
    const response = await fetch('/api/analytics/sync')
    if (response.ok) {
      const cache = await caches.open(API_CACHE)
      cache.put('/api/analytics/sync', response.clone())
    }
  } catch (error) {
    console.error('[SW] Failed to sync analytics:', error)
  }
}

console.log('[SW] Service worker loaded')
