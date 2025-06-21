import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  define: {
    // Define environment variables for browser
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'development'),
    'process.env.REACT_APP_PERFORMANCE_ENDPOINT': JSON.stringify(process.env.REACT_APP_PERFORMANCE_ENDPOINT || ''),
    'process.env.REACT_APP_VAPID_PUBLIC_KEY': JSON.stringify(process.env.REACT_APP_VAPID_PUBLIC_KEY || ''),
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@shared': path.resolve(__dirname, './src/shared'),
      '@chat': path.resolve(__dirname, './src/apps/chat'),
      '@admin': path.resolve(__dirname, './src/apps/admin'),
    },
  },
  server: {
    port: 3001,
    proxy: {
      '/api': {
        target: 'http://localhost:55244',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => {
          console.log(`🔄 Proxying ${path} to http://localhost:55244${path}`)
          return path
        },
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Proxy error:', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log(`📡 Proxying ${req.method} ${req.url} to backend`)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📨 Backend response: ${proxyRes.statusCode} for ${req.url}`)
          })
        },
      },
      '/hub': {
        target: 'http://localhost:55244',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    target: 'es2022',
    rollupOptions: {
      output: {
        manualChunks: {
          // Core React
          vendor: ['react', 'react-dom'],

          // UI Framework
          antd: ['antd', '@ant-design/icons'],

          // State Management
          redux: ['@reduxjs/toolkit', 'react-redux', 'redux-persist'],

          // Data Fetching
          query: ['@tanstack/react-query'],

          // Charts and Visualization
          charts: ['recharts', 'd3'],

          // Code Editor
          editor: ['monaco-editor', '@monaco-editor/react'],

          // Utilities
          utils: ['lodash', 'date-fns', 'dayjs'],

          // Dashboard (new consolidated components)
          dashboard: ['./src/shared/components/dashboard'],
        },
      },
    },
    chunkSizeWarningLimit: 1000,
  },
  optimizeDeps: {
    include: ['react', 'react-dom', 'antd', '@reduxjs/toolkit'],
  },
})
