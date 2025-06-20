import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from './index'

export interface UIState {
  theme: 'light' | 'dark' | 'auto'
  sidebarCollapsed: boolean
  loading: {
    global: boolean
    [key: string]: boolean
  }
  notifications: Array<{
    id: string
    type: 'success' | 'error' | 'warning' | 'info'
    title: string
    message: string
    timestamp: number
    duration?: number
  }>
  modals: {
    [key: string]: {
      open: boolean
      data?: any
    }
  }
}

const initialState: UIState = {
  theme: 'light',
  sidebarCollapsed: false,
  loading: {
    global: false,
  },
  notifications: [],
  modals: {},
}

export const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    setTheme: (state, action: PayloadAction<'light' | 'dark' | 'auto'>) => {
      state.theme = action.payload
    },
    setSidebarCollapsed: (state, action: PayloadAction<boolean>) => {
      state.sidebarCollapsed = action.payload
    },
    setLoading: (state, action: PayloadAction<{ key: string; loading: boolean }>) => {
      state.loading[action.payload.key] = action.payload.loading
    },
    setGlobalLoading: (state, action: PayloadAction<boolean>) => {
      state.loading.global = action.payload
    },
    addNotification: (state, action: PayloadAction<{
      type: 'success' | 'error' | 'warning' | 'info'
      title: string
      message: string
      duration?: number
    }>) => {
      const notification = {
        id: Date.now().toString(),
        timestamp: Date.now(),
        ...action.payload,
      }
      state.notifications.push(notification)
    },
    removeNotification: (state, action: PayloadAction<string>) => {
      state.notifications = state.notifications.filter(
        (notification) => notification.id !== action.payload
      )
    },
    clearNotifications: (state) => {
      state.notifications = []
    },
    openModal: (state, action: PayloadAction<{ key: string; data?: any }>) => {
      state.modals[action.payload.key] = {
        open: true,
        data: action.payload.data,
      }
    },
    closeModal: (state, action: PayloadAction<string>) => {
      if (state.modals[action.payload]) {
        state.modals[action.payload].open = false
        state.modals[action.payload].data = undefined
      }
    },
  },
})

export const uiActions = uiSlice.actions

// Selectors
export const selectUI = (state: RootState) => state.ui
export const selectTheme = (state: RootState) => state.ui.theme
export const selectSidebarCollapsed = (state: RootState) => state.ui.sidebarCollapsed
export const selectLoading = (state: RootState, key: string) => state.ui.loading[key] || false
export const selectGlobalLoading = (state: RootState) => state.ui.loading.global
export const selectNotifications = (state: RootState) => state.ui.notifications
export const selectModal = (state: RootState, key: string) => state.ui.modals[key]
