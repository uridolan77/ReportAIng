/**
 * Token utility functions for JWT handling
 */

/**
 * Check if JWT token is expired
 * Note: JWT exp claims are in UTC, so we must use UTC time for comparison
 */
export function isTokenExpired(token: string): boolean {
  if (!token) return true

  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    // Use UTC time for comparison since JWT exp claims are in UTC
    const currentTimeUTC = Math.floor(Date.now() / 1000)
    return payload.exp < currentTimeUTC
  } catch {
    return true // If we can't parse it, consider it expired
  }
}

/**
 * Check if JWT token is valid format
 */
export function isValidTokenFormat(token: string): boolean {
  if (!token) return false
  
  try {
    const parts = token.split('.')
    if (parts.length !== 3) return false
    
    // Try to parse the payload
    JSON.parse(atob(parts[1]))
    return true
  } catch {
    return false
  }
}

/**
 * Get token expiration time
 */
export function getTokenExpiration(token: string): Date | null {
  if (!token) return null
  
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return new Date(payload.exp * 1000)
  } catch {
    return null
  }
}

/**
 * Get time until token expires (in milliseconds)
 */
export function getTimeUntilExpiration(token: string): number {
  const expiration = getTokenExpiration(token)
  if (!expiration) return 0
  
  return Math.max(0, expiration.getTime() - Date.now())
}

/**
 * Check if token will expire soon (within specified minutes)
 */
export function willExpireSoon(token: string, minutesThreshold: number = 5): boolean {
  const timeUntilExpiration = getTimeUntilExpiration(token)
  return timeUntilExpiration < (minutesThreshold * 60 * 1000)
}

/**
 * Extract user ID from token
 */
export function getUserIdFromToken(token: string): string | null {
  if (!token) return null
  
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.nameid || payload.sub || payload.userId || null
  } catch {
    return null
  }
}

/**
 * Extract username from token
 */
export function getUsernameFromToken(token: string): string | null {
  if (!token) return null
  
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.unique_name || payload.username || payload.name || null
  } catch {
    return null
  }
}

/**
 * Check if error is authentication related
 */
export function isAuthenticationError(error: any): boolean {
  if (!error) return false
  
  // Check error message
  const message = error.message || error.toString() || ''
  const authKeywords = ['401', 'unauthorized', 'authentication', 'token', 'expired', 'invalid']
  
  return authKeywords.some(keyword => 
    message.toLowerCase().includes(keyword.toLowerCase())
  )
}

/**
 * Check if error indicates token expiration
 */
export function isTokenExpirationError(error: any): boolean {
  if (!error) return false
  
  const message = error.message || error.toString() || ''
  const expirationKeywords = ['expired', 'lifetime validation failed', 'token is expired']
  
  return expirationKeywords.some(keyword => 
    message.toLowerCase().includes(keyword.toLowerCase())
  )
}

/**
 * Safe token validation for SignalR connections
 */
export function validateTokenForSignalR(token: string | null | undefined): {
  isValid: boolean
  reason?: string
  token?: string
} {
  if (!token) {
    return { isValid: false, reason: 'No token provided' }
  }

  if (!isValidTokenFormat(token)) {
    return { isValid: false, reason: 'Invalid token format' }
  }

  if (isTokenExpired(token)) {
    return { isValid: false, reason: 'Token is expired' }
  }

  return { isValid: true, token }
}
