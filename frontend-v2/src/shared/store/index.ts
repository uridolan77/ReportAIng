import { configureStore } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import { persistStore, persistReducer } from 'redux-persist'
import storage from 'redux-persist/lib/storage'
import { authSlice } from './auth'
import { uiSlice } from './ui'
import { chatSlice } from './chat'
import { baseApi } from './api/baseApi'
import aiTransparencyReducer from './aiTransparencySlice'
import streamingProcessingReducer from './streamingProcessingSlice'
import { isTokenExpired } from '@shared/utils/tokenUtils'

// Import API files to ensure endpoints are injected
import './api/authApi'
import './api/queryApi'
import './api/businessApi'
import './api/semanticApi'
import './api/adminApi'
import './api/chatApi'
import './api/featuresApi'
import './api/tuningApi'
import './api/costApi'
import './api/performanceApi'
import './api/transparencyApi'
import './api/aiStreamingApi'
import './api/analyticsApi'
import './api/intelligentAgentsApi'
import './api/llmManagementApi'
import './api/templateAnalyticsApi'

// Configure persistence for auth slice with token validation
const authPersistConfig = {
  key: 'auth',
  storage,
  whitelist: ['isAuthenticated', 'user', 'accessToken', 'refreshToken', 'preferences'],
  // Transform to validate tokens during rehydration
  transforms: [
    {
      in: (inboundState: any) => {
        // Validate tokens during rehydration from storage
        if (inboundState && inboundState.accessToken) {
          if (isTokenExpired(inboundState.accessToken)) {
            console.warn('🔐 Expired access token detected during rehydration - clearing auth state')
            return {
              ...inboundState,
              accessToken: null,
              refreshToken: null,
              isAuthenticated: false
            }
          }
        }
        return inboundState
      },
      out: (outboundState: any) => outboundState
    }
  ]
}

const persistedAuthReducer = persistReducer(authPersistConfig, authSlice.reducer)

export const store = configureStore({
  reducer: {
    // Feature slices
    auth: persistedAuthReducer,
    ui: uiSlice.reducer,
    chat: chatSlice.reducer,

    // AI feature slices
    aiTransparency: aiTransparencyReducer,
    streamingProcessing: streamingProcessingReducer,

    // Single API slice (all endpoints are injected into baseApi)
    [baseApi.reducerPath]: baseApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [
          // Ignore these action types
          'persist/PERSIST',
          'persist/REHYDRATE',
          'persist/REGISTER',
        ],
      },
    }).concat(
      baseApi.middleware,
    ),
  devTools: import.meta.env.DEV,
})

// Enable listener behavior for the store
setupListeners(store.dispatch)

// Create persistor
export const persistor = persistStore(store)

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

// Typed hooks for use throughout the app
import { useDispatch, useSelector, TypedUseSelectorHook } from 'react-redux'

export const useAppDispatch = () => useDispatch<AppDispatch>()
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector

// Export actions
export { authActions } from './auth'
export { uiActions } from './ui'
export { chatActions } from './chat'
export { aiTransparencyActions } from './aiTransparencySlice'
