/**
 * Utility functions for session management
 */

/**
 * Generate a unique session ID
 */
export const generateSessionId = (): string => {
  return `session-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
};

/**
 * Get or create a session ID from session storage
 */
export const getOrCreateSessionId = (): string => {
  let sessionId = sessionStorage.getItem('sessionId');
  if (!sessionId) {
    sessionId = generateSessionId();
    sessionStorage.setItem('sessionId', sessionId);
  }
  return sessionId;
};

/**
 * Clear the current session ID
 */
export const clearSessionId = (): void => {
  sessionStorage.removeItem('sessionId');
};

/**
 * Set a specific session ID
 */
export const setSessionId = (sessionId: string): void => {
  sessionStorage.setItem('sessionId', sessionId);
};
