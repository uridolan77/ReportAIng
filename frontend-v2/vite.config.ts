import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
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
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          antd: ['antd', '@ant-design/icons'],
          redux: ['@reduxjs/toolkit', 'react-redux'],
          charts: ['recharts', 'd3'],
          editor: ['monaco-editor', '@monaco-editor/react'],
        },
      },
    },
  },
  optimizeDeps: {
    include: ['react', 'react-dom', 'antd', '@reduxjs/toolkit'],
  },
})
