import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from './index'

export interface User {
  id: string
  username: string
  email: string
  displayName: string
  roles: string[]
  isActive: boolean
  lastLoginDate?: string
  isMfaEnabled: boolean
  mfaMethod?: 'TOTP' | 'SMS'
}

export interface UserPreferences {
  theme: 'light' | 'dark' | 'auto'
  language: string
  timezone: string
  defaultQueryLimit: number
  enableNotifications: boolean
  autoSaveQueries: boolean
  preferredChartTypes: string[]
}

export interface AuthState {
  isAuthenticated: boolean
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  preferences: UserPreferences | null
  isLoading: boolean
  error: string | null
  requiresMfa: boolean
  mfaChallenge: string | null
}

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  accessToken: null,
  refreshToken: null,
  preferences: null,
  isLoading: false,
  error: null,
  requiresMfa: false,
  mfaChallenge: null,
}

export const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
    },
    loginSuccess: (state, action: PayloadAction<{
      user: User
      accessToken: string
      refreshToken: string
    }>) => {
      state.isAuthenticated = true
      state.user = action.payload.user
      state.accessToken = action.payload.accessToken
      state.refreshToken = action.payload.refreshToken
      state.error = null
      state.requiresMfa = false
      state.mfaChallenge = null
    },
    loginMfaRequired: (state, action: PayloadAction<{
      challengeId: string
    }>) => {
      state.requiresMfa = true
      state.mfaChallenge = action.payload.challengeId
      state.error = null
    },
    logout: (state) => {
      state.isAuthenticated = false
      state.user = null
      state.accessToken = null
      state.refreshToken = null
      state.preferences = null
      state.requiresMfa = false
      state.mfaChallenge = null
      state.error = null
    },
    updateTokens: (state, action: PayloadAction<{
      accessToken: string
      refreshToken: string
    }>) => {
      state.accessToken = action.payload.accessToken
      state.refreshToken = action.payload.refreshToken
    },
    updateUser: (state, action: PayloadAction<User>) => {
      state.user = action.payload
    },
    updatePreferences: (state, action: PayloadAction<UserPreferences>) => {
      state.preferences = action.payload
    },
  },
})

export const authActions = authSlice.actions

// Selectors
export const selectAuth = (state: RootState) => state.auth
export const selectIsAuthenticated = (state: RootState) => state.auth.isAuthenticated
export const selectUser = (state: RootState) => state.auth.user
export const selectIsAdmin = (state: RootState) => 
  state.auth.user?.roles.includes('Admin') ?? false
export const selectAccessToken = (state: RootState) => state.auth.accessToken
export const selectRequiresMfa = (state: RootState) => state.auth.requiresMfa
export const selectMfaChallenge = (state: RootState) => state.auth.mfaChallenge
export const selectUserPreferences = (state: RootState) => state.auth.preferences
