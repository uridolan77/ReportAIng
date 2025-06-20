import { configureStore } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import { persistStore, persistReducer } from 'redux-persist'
import storage from 'redux-persist/lib/storage'
import { authSlice } from './auth'
import { uiSlice } from './ui'
import { chatSlice } from './chat'
import { baseApi } from './api/baseApi'

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

// Configure persistence for auth slice
const authPersistConfig = {
  key: 'auth',
  storage,
  whitelist: ['isAuthenticated', 'user', 'accessToken', 'refreshToken', 'preferences']
}

const persistedAuthReducer = persistReducer(authPersistConfig, authSlice.reducer)

export const store = configureStore({
  reducer: {
    // Feature slices
    auth: persistedAuthReducer,
    ui: uiSlice.reducer,
    chat: chatSlice.reducer,

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
  devTools: process.env.NODE_ENV !== 'production',
})

// Enable listener behavior for the store
setupListeners(store.dispatch)

// Create persistor
export const persistor = persistStore(store)

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

// Export actions
export { authActions } from './auth'
export { uiActions } from './ui'
export { chatActions } from './chat'
