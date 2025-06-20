import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { RootState } from '../index'

const baseQuery = fetchBaseQuery({
  baseUrl: '/api',
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.accessToken
    if (token) {
      headers.set('authorization', `Bearer ${token}`)
    }
    headers.set('content-type', 'application/json')
    return headers
  },
})

const baseQueryWithReauth = async (args: any, api: any, extraOptions: any) => {
  let result = await baseQuery(args, api, extraOptions)
  
  if (result.error && result.error.status === 401) {
    // Try to get a new token
    const refreshToken = (api.getState() as RootState).auth.refreshToken
    if (refreshToken) {
      const refreshResult = await baseQuery(
        {
          url: '/auth/refresh',
          method: 'POST',
          body: { refreshToken },
        },
        api,
        extraOptions
      )
      
      if (refreshResult.data) {
        // Store the new token
        api.dispatch({
          type: 'auth/updateTokens',
          payload: refreshResult.data,
        })
        
        // Retry the original query
        result = await baseQuery(args, api, extraOptions)
      } else {
        // Refresh failed, logout user
        api.dispatch({ type: 'auth/logout' })
      }
    } else {
      // No refresh token, logout user
      api.dispatch({ type: 'auth/logout' })
    }
  }
  
  return result
}

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: baseQueryWithReauth,
  tagTypes: [
    'User',
    'Query',
    'QueryHistory',
    'BusinessTable',
    'BusinessColumn',
    'GlossaryTerm',
    'Schema',
    'SystemConfig',
    'Analytics',
  ],
  endpoints: () => ({}),
})
