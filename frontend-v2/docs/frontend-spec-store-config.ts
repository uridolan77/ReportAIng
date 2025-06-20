// store/index.ts
import { configureStore } from '@reduxjs/toolkit'
import { baseApi } from '../services/api/baseApi'
import { costManagementApi } from '../services/api/costManagementApi'
import { performanceApi } from '../services/api/performanceApi'
// ... other API slices

export const store = configureStore({
  reducer: {
    [baseApi.reducerPath]: baseApi.reducer,
    // Add other reducers as needed
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      baseApi.middleware,
      // Add other middleware
    ),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
