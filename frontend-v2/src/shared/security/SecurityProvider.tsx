/**
 * Security Provider
 * 
 * Comprehensive security framework providing:
 * - Content Security Policy (CSP) management
 * - XSS protection
 * - CSRF protection
 * - Input sanitization
 * - Security headers validation
 * - Threat detection and reporting
 */

import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react'
import DOMPurify from 'dompurify'

export interface SecurityConfig {
  /** Enable CSP enforcement */
  enableCSP: boolean
  /** Enable XSS protection */
  enableXSSProtection: boolean
  /** Enable CSRF protection */
  enableCSRFProtection: boolean
  /** Enable input sanitization */
  enableInputSanitization: boolean
  /** Security reporting endpoint */
  reportingEndpoint?: string
  /** Allowed domains for external resources */
  allowedDomains: string[]
  /** Enable security monitoring */
  enableMonitoring: boolean
}

export interface SecurityThreat {
  id: string
  type: 'xss' | 'csrf' | 'injection' | 'unauthorized' | 'suspicious'
  severity: 'low' | 'medium' | 'high' | 'critical'
  description: string
  timestamp: Date
  source: string
  blocked: boolean
  details: Record<string, any>
}

export interface SecurityContextType {
  config: SecurityConfig
  updateConfig: (config: Partial<SecurityConfig>) => void
  sanitizeHTML: (html: string) => string
  sanitizeInput: (input: string) => string
  validateURL: (url: string) => boolean
  reportThreat: (threat: Omit<SecurityThreat, 'id' | 'timestamp'>) => void
  getThreats: () => SecurityThreat[]
  clearThreats: () => void
  isSecureContext: boolean
  csrfToken: string | null
  generateCSRFToken: () => string
}

const defaultConfig: SecurityConfig = {
  enableCSP: true,
  enableXSSProtection: true,
  enableCSRFProtection: true,
  enableInputSanitization: true,
  allowedDomains: ['localhost', '127.0.0.1'],
  enableMonitoring: true,
}

const SecurityContext = createContext<SecurityContextType | null>(null)

export const useSecurity = () => {
  const context = useContext(SecurityContext)
  if (!context) {
    throw new Error('useSecurity must be used within SecurityProvider')
  }
  return context
}

interface SecurityProviderProps {
  children: React.ReactNode
  initialConfig?: Partial<SecurityConfig>
}

export const SecurityProvider: React.FC<SecurityProviderProps> = ({
  children,
  initialConfig = {},
}) => {
  const [config, setConfig] = useState<SecurityConfig>({
    ...defaultConfig,
    ...initialConfig,
  })
  
  const [threats, setThreats] = useState<SecurityThreat[]>([])
  const [csrfToken, setCSRFToken] = useState<string | null>(null)
  const [isSecureContext, setIsSecureContext] = useState(false)
  
  const threatIdCounter = useRef(0)

  // Check if we're in a secure context
  useEffect(() => {
    setIsSecureContext(window.isSecureContext || location.protocol === 'https:')
  }, [])

  // Initialize DOMPurify configuration
  useEffect(() => {
    if (config.enableXSSProtection) {
      // Configure DOMPurify for strict XSS protection
      DOMPurify.addHook('beforeSanitizeElements', (node) => {
        // Log potential XSS attempts
        if (node.tagName && ['SCRIPT', 'IFRAME', 'OBJECT', 'EMBED'].includes(node.tagName)) {
          reportThreat({
            type: 'xss',
            severity: 'high',
            description: `Blocked potentially malicious ${node.tagName} element`,
            source: 'DOMPurify',
            blocked: true,
            details: { tagName: node.tagName, innerHTML: node.innerHTML },
          })
        }
      })
    }
  }, [config.enableXSSProtection])

  // Set up Content Security Policy
  useEffect(() => {
    if (config.enableCSP) {
      const cspDirectives = [
        "default-src 'self'",
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'", // Note: In production, remove unsafe-inline and unsafe-eval
        "style-src 'self' 'unsafe-inline'",
        "img-src 'self' data: blob:",
        "font-src 'self' data:",
        "connect-src 'self' ws: wss:",
        "media-src 'self'",
        "object-src 'none'",
        "base-uri 'self'",
        "form-action 'self'",
        "frame-ancestors 'none'",
        "upgrade-insecure-requests",
      ]

      // Add allowed domains
      if (config.allowedDomains.length > 0) {
        const domains = config.allowedDomains.join(' ')
        cspDirectives[1] += ` ${domains}` // script-src
        cspDirectives[2] += ` ${domains}` // style-src
        cspDirectives[4] += ` ${domains}` // connect-src
      }

      const cspHeader = cspDirectives.join('; ')
      
      // Set CSP via meta tag (for development)
      let cspMeta = document.querySelector('meta[http-equiv="Content-Security-Policy"]')
      if (!cspMeta) {
        cspMeta = document.createElement('meta')
        cspMeta.setAttribute('http-equiv', 'Content-Security-Policy')
        document.head.appendChild(cspMeta)
      }
      cspMeta.setAttribute('content', cspHeader)
    }
  }, [config.enableCSP, config.allowedDomains])

  // Generate CSRF token
  const generateCSRFToken = useCallback(() => {
    const token = Array.from(crypto.getRandomValues(new Uint8Array(32)))
      .map(b => b.toString(16).padStart(2, '0'))
      .join('')
    
    setCSRFToken(token)
    
    // Store in sessionStorage for server validation
    sessionStorage.setItem('csrf-token', token)
    
    return token
  }, [])

  // Initialize CSRF token
  useEffect(() => {
    if (config.enableCSRFProtection) {
      const existingToken = sessionStorage.getItem('csrf-token')
      if (existingToken) {
        setCSRFToken(existingToken)
      } else {
        generateCSRFToken()
      }
    }
  }, [config.enableCSRFProtection, generateCSRFToken])

  // Update configuration
  const updateConfig = useCallback((newConfig: Partial<SecurityConfig>) => {
    setConfig(prev => ({ ...prev, ...newConfig }))
  }, [])

  // Sanitize HTML content
  const sanitizeHTML = useCallback((html: string): string => {
    if (!config.enableXSSProtection) return html
    
    try {
      const sanitized = DOMPurify.sanitize(html, {
        ALLOWED_TAGS: [
          'p', 'br', 'strong', 'em', 'u', 'i', 'b', 'span', 'div',
          'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
          'ul', 'ol', 'li',
          'a', 'img',
          'table', 'thead', 'tbody', 'tr', 'th', 'td',
          'blockquote', 'code', 'pre'
        ],
        ALLOWED_ATTR: [
          'href', 'src', 'alt', 'title', 'class', 'id',
          'target', 'rel', 'width', 'height'
        ],
        ALLOW_DATA_ATTR: false,
        ALLOW_UNKNOWN_PROTOCOLS: false,
      })
      
      if (sanitized !== html) {
        reportThreat({
          type: 'xss',
          severity: 'medium',
          description: 'HTML content was sanitized',
          source: 'sanitizeHTML',
          blocked: true,
          details: { original: html, sanitized },
        })
      }
      
      return sanitized
    } catch (error) {
      reportThreat({
        type: 'xss',
        severity: 'high',
        description: 'HTML sanitization failed',
        source: 'sanitizeHTML',
        blocked: true,
        details: { error: error instanceof Error ? error.message : 'Unknown error', html },
      })
      return '' // Return empty string if sanitization fails
    }
  }, [config.enableXSSProtection])

  // Sanitize user input
  const sanitizeInput = useCallback((input: string): string => {
    if (!config.enableInputSanitization) return input
    
    // Remove potentially dangerous characters and patterns
    const sanitized = input
      .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '') // Remove script tags
      .replace(/javascript:/gi, '') // Remove javascript: protocol
      .replace(/on\w+\s*=/gi, '') // Remove event handlers
      .replace(/data:/gi, '') // Remove data: protocol
      .replace(/vbscript:/gi, '') // Remove vbscript: protocol
      .trim()
    
    if (sanitized !== input) {
      reportThreat({
        type: 'injection',
        severity: 'medium',
        description: 'User input was sanitized',
        source: 'sanitizeInput',
        blocked: true,
        details: { original: input, sanitized },
      })
    }
    
    return sanitized
  }, [config.enableInputSanitization])

  // Validate URL safety
  const validateURL = useCallback((url: string): boolean => {
    try {
      const urlObj = new URL(url)
      
      // Check protocol
      if (!['http:', 'https:', 'mailto:', 'tel:'].includes(urlObj.protocol)) {
        reportThreat({
          type: 'suspicious',
          severity: 'medium',
          description: `Blocked URL with suspicious protocol: ${urlObj.protocol}`,
          source: 'validateURL',
          blocked: true,
          details: { url, protocol: urlObj.protocol },
        })
        return false
      }
      
      // Check for suspicious patterns
      const suspiciousPatterns = [
        /javascript:/i,
        /vbscript:/i,
        /data:/i,
        /file:/i,
        /<script/i,
        /onload=/i,
        /onerror=/i,
      ]
      
      for (const pattern of suspiciousPatterns) {
        if (pattern.test(url)) {
          reportThreat({
            type: 'xss',
            severity: 'high',
            description: `Blocked URL with suspicious pattern: ${pattern}`,
            source: 'validateURL',
            blocked: true,
            details: { url, pattern: pattern.toString() },
          })
          return false
        }
      }
      
      return true
    } catch (error) {
      reportThreat({
        type: 'suspicious',
        severity: 'low',
        description: 'Invalid URL format',
        source: 'validateURL',
        blocked: true,
        details: { url, error: error instanceof Error ? error.message : 'Unknown error' },
      })
      return false
    }
  }, [])

  // Report security threat
  const reportThreat = useCallback((threat: Omit<SecurityThreat, 'id' | 'timestamp'>) => {
    const newThreat: SecurityThreat = {
      ...threat,
      id: `threat-${++threatIdCounter.current}`,
      timestamp: new Date(),
    }
    
    setThreats(prev => [...prev.slice(-99), newThreat]) // Keep last 100 threats
    
    // Log to console in development
    if (process.env.NODE_ENV === 'development') {
      console.warn('[Security]', newThreat)
    }
    
    // Report to monitoring endpoint if configured
    if (config.reportingEndpoint && config.enableMonitoring) {
      fetch(config.reportingEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(csrfToken && { 'X-CSRF-Token': csrfToken }),
        },
        body: JSON.stringify(newThreat),
      }).catch(error => {
        console.error('Failed to report security threat:', error)
      })
    }
  }, [config.reportingEndpoint, config.enableMonitoring, csrfToken])

  // Get all threats
  const getThreats = useCallback(() => threats, [threats])

  // Clear threats
  const clearThreats = useCallback(() => {
    setThreats([])
  }, [])

  // Monitor for suspicious activity
  useEffect(() => {
    if (!config.enableMonitoring) return

    const handleError = (event: ErrorEvent) => {
      // Check for potential XSS in error messages
      if (event.message.includes('<script') || event.message.includes('javascript:')) {
        reportThreat({
          type: 'xss',
          severity: 'high',
          description: 'Potential XSS detected in error message',
          source: 'errorHandler',
          blocked: false,
          details: { message: event.message, filename: event.filename, lineno: event.lineno },
        })
      }
    }

    const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
      reportThreat({
        type: 'suspicious',
        severity: 'low',
        description: 'Unhandled promise rejection',
        source: 'promiseRejectionHandler',
        blocked: false,
        details: { reason: event.reason },
      })
    }

    window.addEventListener('error', handleError)
    window.addEventListener('unhandledrejection', handleUnhandledRejection)

    return () => {
      window.removeEventListener('error', handleError)
      window.removeEventListener('unhandledrejection', handleUnhandledRejection)
    }
  }, [config.enableMonitoring, reportThreat])

  const contextValue: SecurityContextType = {
    config,
    updateConfig,
    sanitizeHTML,
    sanitizeInput,
    validateURL,
    reportThreat,
    getThreats,
    clearThreats,
    isSecureContext,
    csrfToken,
    generateCSRFToken,
  }

  return (
    <SecurityContext.Provider value={contextValue}>
      {children}
    </SecurityContext.Provider>
  )
}

export default SecurityProvider
