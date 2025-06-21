/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string
  readonly VITE_API_TIMEOUT: string
  // Mock data environment variables removed
  readonly VITE_DEBUG_MODE: string
  readonly VITE_ENABLE_DEVTOOLS: string
  readonly VITE_BUILD_TIME: string
  readonly VITE_VERSION: string
  readonly VITE_ENABLE_REAL_TIME: string
  readonly VITE_ENABLE_COST_TRACKING: string
  readonly VITE_ENABLE_PERFORMANCE_MONITORING: string
  readonly VITE_ENABLE_ADVANCED_CHARTS: string
  readonly VITE_ENABLE_CSP: string
  readonly VITE_SECURE_COOKIES: string
  readonly VITE_ENABLE_ANALYTICS: string
  readonly VITE_ANALYTICS_ID: string
  readonly VITE_ENABLE_ERROR_REPORTING: string
  readonly VITE_SENTRY_DSN: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
