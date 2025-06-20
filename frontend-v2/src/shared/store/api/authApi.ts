import { baseApi } from './baseApi'
import type { User, UserPreferences } from '../auth'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginWithMfaRequest extends LoginRequest {
  mfaCode: string
  challengeId?: string
}

export interface AuthenticationResult {
  success: boolean
  accessToken: string
  refreshToken: string
  expiresAt: string
  user?: User
  requiresMfa?: boolean
  mfaChallenge?: {
    challengeId: string
    qrCode?: string
  }
  errorMessage?: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface MfaSetupResponse {
  qrCode: string
  secret: string
  backupCodes: string[]
}

export interface UserActivitySummary {
  totalQueries: number
  queriesThisWeek: number
  averageQueryTime: number
  favoriteChartTypes: string[]
  lastActiveDate: string
}

export interface UserInfo extends User {
  preferences: UserPreferences
  activitySummary: UserActivitySummary
}

export const authApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Authentication
    login: builder.mutation<AuthenticationResult, LoginRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        body: credentials,
      }),
      invalidatesTags: ['User'],
    }),
    
    loginWithMfa: builder.mutation<AuthenticationResult, LoginWithMfaRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        body: credentials,
      }),
      invalidatesTags: ['User'],
    }),
    
    refreshToken: builder.mutation<AuthenticationResult, RefreshTokenRequest>({
      query: (body) => ({
        url: '/auth/refresh',
        method: 'POST',
        body,
      }),
    }),
    
    logout: builder.mutation<{ success: boolean }, RefreshTokenRequest>({
      query: (body) => ({
        url: '/auth/logout',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['User'],
    }),
    
    // MFA
    setupMfa: builder.mutation<MfaSetupResponse, void>({
      query: () => ({
        url: '/auth/mfa/setup',
        method: 'POST',
      }),
    }),
    
    verifyMfa: builder.mutation<{ success: boolean }, { code: string }>({
      query: (body) => ({
        url: '/auth/mfa/verify',
        method: 'POST',
        body,
      }),
    }),
    
    // User Profile
    getCurrentUser: builder.query<UserInfo, void>({
      query: () => '/user/profile',
      providesTags: ['User'],
    }),
    
    updateUserProfile: builder.mutation<UserInfo, Partial<UserInfo>>({
      query: (body) => ({
        url: '/user/profile',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['User'],
    }),
    
    getUserPreferences: builder.query<UserPreferences, void>({
      query: () => '/user/preferences',
      providesTags: ['User'],
    }),
    
    updateUserPreferences: builder.mutation<UserPreferences, Partial<UserPreferences>>({
      query: (body) => ({
        url: '/user/preferences',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['User'],
    }),
    
    getUserActivity: builder.query<UserActivitySummary, { days?: number }>({
      query: ({ days = 30 }) => `/user/activity?days=${days}`,
      providesTags: ['User'],
    }),
  }),
})

export const {
  useLoginMutation,
  useLoginWithMfaMutation,
  useRefreshTokenMutation,
  useLogoutMutation,
  useSetupMfaMutation,
  useVerifyMfaMutation,
  useGetCurrentUserQuery,
  useUpdateUserProfileMutation,
  useGetUserPreferencesQuery,
  useUpdateUserPreferencesMutation,
  useGetUserActivityQuery,
} = authApi
