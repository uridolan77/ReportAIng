import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import { performanceOptimizer } from './utils/performance/PerformanceOptimizer';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

// Initialize performance monitoring
performanceOptimizer.initialize();

// Add performance validation in development mode
if (process.env.NODE_ENV === 'development') {
  // Make performance validator available globally for testing
  import('./utils/validation/PerformanceValidator').then(({ performanceValidator }) => {
    (window as any).performanceValidator = performanceValidator;
    console.log('🔧 Performance validator available at window.performanceValidator');
    console.log('Run window.performanceValidator.validatePerformance() to test performance optimizations');
  });
}

// Register service worker for caching and offline support
if ('serviceWorker' in navigator && process.env.NODE_ENV === 'production') {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then((registration) => {
        console.log('Service Worker registered successfully:', registration.scope);

        // Check for updates
        registration.addEventListener('updatefound', () => {
          const newWorker = registration.installing;
          if (newWorker) {
            newWorker.addEventListener('statechange', () => {
              if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                // New content is available, prompt user to refresh
                if (window.confirm('New version available! Refresh to update?')) {
                  window.location.reload();
                }
              }
            });
          }
        });
      })
      .catch((error) => {
        console.error('Service Worker registration failed:', error);
      });
  });
}
