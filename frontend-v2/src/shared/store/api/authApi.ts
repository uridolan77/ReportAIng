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

    // Additional Authentication Features
    changePassword: builder.mutation<{ success: boolean }, {
      currentPassword: string
      newPassword: string
    }>({
      query: (body) => ({
        url: '/auth/change-password',
        method: 'POST',
        body,
      }),
    }),

    forgotPassword: builder.mutation<{ success: boolean }, { email: string }>({
      query: (body) => ({
        url: '/auth/forgot-password',
        method: 'POST',
        body,
      }),
    }),

    resetPassword: builder.mutation<{ success: boolean }, {
      token: string
      newPassword: string
    }>({
      query: (body) => ({
        url: '/auth/reset-password',
        method: 'POST',
        body,
      }),
    }),

    disableMfa: builder.mutation<{ success: boolean }, { code: string }>({
      query: (body) => ({
        url: '/auth/mfa/disable',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['User'],
    }),

    // Avatar Management
    uploadAvatar: builder.mutation<{ success: boolean; avatarUrl: string }, FormData>({
      query: (formData) => ({
        url: '/user/avatar',
        method: 'POST',
        body: formData,
      }),
      invalidatesTags: ['User'],
    }),

    deleteAvatar: builder.mutation<{ success: boolean }, void>({
      query: () => ({
        url: '/user/avatar',
        method: 'DELETE',
      }),
      invalidatesTags: ['User'],
    }),

    // Session Management
    getActiveSessions: builder.query<Array<{
      id: string
      deviceInfo: string
      ipAddress: string
      location: string
      lastActivity: string
      isCurrent: boolean
    }>, void>({
      query: () => '/user/sessions',
      providesTags: ['UserSessions'],
    }),

    revokeSession: builder.mutation<{ success: boolean }, string>({
      query: (sessionId) => ({
        url: `/user/sessions/${sessionId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['UserSessions'],
    }),

    revokeAllSessions: builder.mutation<{ success: boolean }, void>({
      query: () => ({
        url: '/user/sessions/revoke-all',
        method: 'POST',
      }),
      invalidatesTags: ['UserSessions'],
    }),

    // Email Verification
    sendVerificationEmail: builder.mutation<{ success: boolean }, void>({
      query: () => ({
        url: '/user/verify-email/send',
        method: 'POST',
      }),
    }),

    verifyEmail: builder.mutation<{ success: boolean }, { token: string }>({
      query: (body) => ({
        url: '/user/verify-email',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['User'],
    }),

    // Account Management
    deleteAccount: builder.mutation<{ success: boolean }, { password: string }>({
      query: (body) => ({
        url: '/user/account',
        method: 'DELETE',
        body,
      }),
    }),

    exportUserData: builder.mutation<Blob, void>({
      query: () => ({
        url: '/user/export',
        method: 'POST',
        responseHandler: (response) => response.blob(),
      }),
    }),
  }),
})

export const {
  // Authentication
  useLoginMutation,
  useLoginWithMfaMutation,
  useRefreshTokenMutation,
  useLogoutMutation,

  // MFA
  useSetupMfaMutation,
  useVerifyMfaMutation,
  useDisableMfaMutation,

  // Password Management
  useChangePasswordMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,

  // User Profile
  useGetCurrentUserQuery,
  useUpdateUserProfileMutation,
  useGetUserPreferencesQuery,
  useUpdateUserPreferencesMutation,
  useGetUserActivityQuery,

  // Avatar Management
  useUploadAvatarMutation,
  useDeleteAvatarMutation,

  // Session Management
  useGetActiveSessionsQuery,
  useRevokeSessionMutation,
  useRevokeAllSessionsMutation,

  // Email Verification
  useSendVerificationEmailMutation,
  useVerifyEmailMutation,

  // Account Management
  useDeleteAccountMutation,
  useExportUserDataMutation,
} = authApi
