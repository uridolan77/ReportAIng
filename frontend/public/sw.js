/**
 * Service Worker for ReportAIng
 * 
 * Provides advanced caching strategies, offline support, and performance optimization
 */

const CACHE_NAME = 'reportaing-v1.0.0';
const STATIC_CACHE = 'reportaing-static-v1.0.0';
const DYNAMIC_CACHE = 'reportaing-dynamic-v1.0.0';
const API_CACHE = 'reportaing-api-v1.0.0';

// Assets to cache immediately
const STATIC_ASSETS = [
  '/',
  '/static/js/bundle.js',
  '/static/css/main.css',
  '/manifest.json',
  '/favicon.ico',
];

// API endpoints to cache
const API_ENDPOINTS = [
  '/api/health',
  '/api/database/schema',
  '/api/suggestions',
];

// Cache strategies
const CACHE_STRATEGIES = {
  CACHE_FIRST: 'cache-first',
  NETWORK_FIRST: 'network-first',
  STALE_WHILE_REVALIDATE: 'stale-while-revalidate',
  NETWORK_ONLY: 'network-only',
  CACHE_ONLY: 'cache-only',
};

// Route configurations
const ROUTE_CONFIG = {
  // Static assets - cache first
  '/static/': CACHE_STRATEGIES.CACHE_FIRST,
  '/images/': CACHE_STRATEGIES.CACHE_FIRST,
  '/fonts/': CACHE_STRATEGIES.CACHE_FIRST,
  
  // API endpoints - network first with fallback
  '/api/health': CACHE_STRATEGIES.STALE_WHILE_REVALIDATE,
  '/api/database/schema': CACHE_STRATEGIES.STALE_WHILE_REVALIDATE,
  '/api/suggestions': CACHE_STRATEGIES.STALE_WHILE_REVALIDATE,
  '/api/query': CACHE_STRATEGIES.NETWORK_FIRST,
  
  // Pages - stale while revalidate
  '/': CACHE_STRATEGIES.STALE_WHILE_REVALIDATE,
  '/dashboard': CACHE_STRATEGIES.STALE_WHILE_REVALIDATE,
  '/visualization': CACHE_STRATEGIES.STALE_WHILE_REVALIDATE,
};

// Install event - cache static assets
self.addEventListener('install', (event) => {
  console.log('Service Worker: Installing...');
  
  event.waitUntil(
    caches.open(STATIC_CACHE)
      .then((cache) => {
        console.log('Service Worker: Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => {
        console.log('Service Worker: Static assets cached');
        return self.skipWaiting();
      })
      .catch((error) => {
        console.error('Service Worker: Failed to cache static assets', error);
      })
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  console.log('Service Worker: Activating...');
  
  event.waitUntil(
    caches.keys()
      .then((cacheNames) => {
        return Promise.all(
          cacheNames.map((cacheName) => {
            if (cacheName !== STATIC_CACHE && 
                cacheName !== DYNAMIC_CACHE && 
                cacheName !== API_CACHE) {
              console.log('Service Worker: Deleting old cache', cacheName);
              return caches.delete(cacheName);
            }
          })
        );
      })
      .then(() => {
        console.log('Service Worker: Activated');
        return self.clients.claim();
      })
  );
});

// Fetch event - handle requests with appropriate caching strategy
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);
  
  // Skip non-GET requests
  if (request.method !== 'GET') {
    return;
  }
  
  // Skip chrome-extension requests
  if (url.protocol === 'chrome-extension:') {
    return;
  }
  
  // Determine caching strategy
  const strategy = getCachingStrategy(url.pathname);
  
  event.respondWith(
    handleRequest(request, strategy)
  );
});

// Get caching strategy for a given path
function getCachingStrategy(pathname) {
  for (const [route, strategy] of Object.entries(ROUTE_CONFIG)) {
    if (pathname.startsWith(route)) {
      return strategy;
    }
  }
  
  // Default strategy
  return CACHE_STRATEGIES.STALE_WHILE_REVALIDATE;
}

// Handle request based on strategy
async function handleRequest(request, strategy) {
  const url = new URL(request.url);
  
  switch (strategy) {
    case CACHE_STRATEGIES.CACHE_FIRST:
      return cacheFirst(request);
    
    case CACHE_STRATEGIES.NETWORK_FIRST:
      return networkFirst(request);
    
    case CACHE_STRATEGIES.STALE_WHILE_REVALIDATE:
      return staleWhileRevalidate(request);
    
    case CACHE_STRATEGIES.NETWORK_ONLY:
      return fetch(request);
    
    case CACHE_STRATEGIES.CACHE_ONLY:
      return caches.match(request);
    
    default:
      return staleWhileRevalidate(request);
  }
}

// Cache first strategy
async function cacheFirst(request) {
  const cachedResponse = await caches.match(request);
  
  if (cachedResponse) {
    return cachedResponse;
  }
  
  try {
    const networkResponse = await fetch(request);
    
    if (networkResponse.ok) {
      const cache = await caches.open(getCacheName(request));
      cache.put(request, networkResponse.clone());
    }
    
    return networkResponse;
  } catch (error) {
    console.error('Service Worker: Network request failed', error);
    return new Response('Offline', { status: 503 });
  }
}

// Network first strategy
async function networkFirst(request) {
  try {
    const networkResponse = await fetch(request);
    
    if (networkResponse.ok) {
      const cache = await caches.open(getCacheName(request));
      cache.put(request, networkResponse.clone());
    }
    
    return networkResponse;
  } catch (error) {
    console.log('Service Worker: Network failed, trying cache');
    
    const cachedResponse = await caches.match(request);
    
    if (cachedResponse) {
      return cachedResponse;
    }
    
    return new Response('Offline', { status: 503 });
  }
}

// Stale while revalidate strategy
async function staleWhileRevalidate(request) {
  const cachedResponse = await caches.match(request);
  
  const networkResponsePromise = fetch(request)
    .then((networkResponse) => {
      if (networkResponse.ok) {
        const cache = caches.open(getCacheName(request));
        cache.then(c => c.put(request, networkResponse.clone()));
      }
      return networkResponse;
    })
    .catch((error) => {
      console.log('Service Worker: Network request failed', error);
      return null;
    });
  
  return cachedResponse || networkResponsePromise;
}

// Get appropriate cache name for request
function getCacheName(request) {
  const url = new URL(request.url);
  
  if (url.pathname.startsWith('/api/')) {
    return API_CACHE;
  }
  
  if (url.pathname.startsWith('/static/')) {
    return STATIC_CACHE;
  }
  
  return DYNAMIC_CACHE;
}

// Background sync for offline actions
self.addEventListener('sync', (event) => {
  if (event.tag === 'background-sync') {
    event.waitUntil(doBackgroundSync());
  }
});

async function doBackgroundSync() {
  console.log('Service Worker: Performing background sync');
  
  // Handle any queued offline actions
  // This would integrate with your app's offline queue
}

// Push notifications (if needed)
self.addEventListener('push', (event) => {
  if (event.data) {
    const data = event.data.json();
    
    const options = {
      body: data.body,
      icon: '/favicon.ico',
      badge: '/favicon.ico',
      data: data.data,
    };
    
    event.waitUntil(
      self.registration.showNotification(data.title, options)
    );
  }
});

// Notification click handler
self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  
  event.waitUntil(
    clients.openWindow(event.notification.data.url || '/')
  );
});

// Message handler for communication with main thread
self.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
  
  if (event.data && event.data.type === 'GET_VERSION') {
    event.ports[0].postMessage({ version: CACHE_NAME });
  }
  
  if (event.data && event.data.type === 'CLEAR_CACHE') {
    event.waitUntil(
      caches.keys().then((cacheNames) => {
        return Promise.all(
          cacheNames.map((cacheName) => caches.delete(cacheName))
        );
      })
    );
  }
});

console.log('Service Worker: Loaded');
