import { configureStore } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import { authSlice } from './auth'
import { uiSlice } from './ui'
import { chatSlice } from './chat'
import { authApi } from './api/authApi'
import { queryApi } from './api/queryApi'
import { businessApi } from './api/businessApi'
import { semanticApi } from './api/semanticApi'
import { adminApi } from './api/adminApi'
import { chatApi } from './api/chatApi'

export const store = configureStore({
  reducer: {
    // Feature slices
    auth: authSlice.reducer,
    ui: uiSlice.reducer,
    chat: chatSlice.reducer,

    // API slices
    [authApi.reducerPath]: authApi.reducer,
    [queryApi.reducerPath]: queryApi.reducer,
    [businessApi.reducerPath]: businessApi.reducer,
    [semanticApi.reducerPath]: semanticApi.reducer,
    [adminApi.reducerPath]: adminApi.reducer,
    [chatApi.reducerPath]: chatApi.reducer,
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
      authApi.middleware,
      queryApi.middleware,
      businessApi.middleware,
      semanticApi.middleware,
      adminApi.middleware,
      chatApi.middleware,
    ),
  devTools: process.env.NODE_ENV !== 'production',
})

// Enable listener behavior for the store
setupListeners(store.dispatch)

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

// Export actions
export { authActions } from './auth'
export { uiActions } from './ui'
export { chatActions } from './chat'
