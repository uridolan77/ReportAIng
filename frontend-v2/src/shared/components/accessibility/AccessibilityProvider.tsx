/**
 * Accessibility Provider
 * 
 * Provides comprehensive accessibility features including:
 * - WCAG 2.1 AA compliance
 * - Keyboard navigation
 * - Screen reader support
 * - Focus management
 * - Accessibility announcements
 */

import React, { createContext, useContext, useState, useCallback, useRef, useEffect } from 'react'

export interface AccessibilitySettings {
  /** Enable high contrast mode */
  highContrast: boolean
  /** Reduce motion for animations */
  reduceMotion: boolean
  /** Increase font size */
  largeFonts: boolean
  /** Enable focus indicators */
  focusIndicators: boolean
  /** Screen reader mode */
  screenReader: boolean
  /** Keyboard navigation only */
  keyboardOnly: boolean
}

export interface AccessibilityContextType {
  settings: AccessibilitySettings
  updateSettings: (settings: Partial<AccessibilitySettings>) => void
  announce: (message: string, priority?: 'polite' | 'assertive') => void
  focusElement: (elementId: string) => void
  skipToContent: () => void
  skipToNavigation: () => void
  isKeyboardUser: boolean
  setKeyboardUser: (isKeyboard: boolean) => void
}

const defaultSettings: AccessibilitySettings = {
  highContrast: false,
  reduceMotion: false,
  largeFonts: false,
  focusIndicators: true,
  screenReader: false,
  keyboardOnly: false,
}

const AccessibilityContext = createContext<AccessibilityContextType | null>(null)

export const useAccessibility = () => {
  const context = useContext(AccessibilityContext)
  if (!context) {
    throw new Error('useAccessibility must be used within AccessibilityProvider')
  }
  return context
}

interface AccessibilityProviderProps {
  children: React.ReactNode
  initialSettings?: Partial<AccessibilitySettings>
}

export const AccessibilityProvider: React.FC<AccessibilityProviderProps> = ({
  children,
  initialSettings = {},
}) => {
  const [settings, setSettings] = useState<AccessibilitySettings>({
    ...defaultSettings,
    ...initialSettings,
  })
  
  const [isKeyboardUser, setIsKeyboardUser] = useState(false)
  const announcementRef = useRef<HTMLDivElement>(null)
  const politeAnnouncementRef = useRef<HTMLDivElement>(null)

  // Detect keyboard usage
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Tab') {
        setIsKeyboardUser(true)
      }
    }

    const handleMouseDown = () => {
      setIsKeyboardUser(false)
    }

    document.addEventListener('keydown', handleKeyDown)
    document.addEventListener('mousedown', handleMouseDown)

    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      document.removeEventListener('mousedown', handleMouseDown)
    }
  }, [])

  // Apply accessibility settings to document
  useEffect(() => {
    const root = document.documentElement

    // High contrast mode
    if (settings.highContrast) {
      root.classList.add('high-contrast')
    } else {
      root.classList.remove('high-contrast')
    }

    // Reduce motion
    if (settings.reduceMotion) {
      root.classList.add('reduce-motion')
    } else {
      root.classList.remove('reduce-motion')
    }

    // Large fonts
    if (settings.largeFonts) {
      root.classList.add('large-fonts')
    } else {
      root.classList.remove('large-fonts')
    }

    // Focus indicators
    if (settings.focusIndicators) {
      root.classList.add('focus-indicators')
    } else {
      root.classList.remove('focus-indicators')
    }

    // Keyboard only mode
    if (settings.keyboardOnly || isKeyboardUser) {
      root.classList.add('keyboard-user')
    } else {
      root.classList.remove('keyboard-user')
    }

  }, [settings, isKeyboardUser])

  const updateSettings = useCallback((newSettings: Partial<AccessibilitySettings>) => {
    setSettings(prev => ({ ...prev, ...newSettings }))
    
    // Save to localStorage
    const updatedSettings = { ...settings, ...newSettings }
    localStorage.setItem('accessibility-settings', JSON.stringify(updatedSettings))
  }, [settings])

  // Load settings from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem('accessibility-settings')
    if (saved) {
      try {
        const savedSettings = JSON.parse(saved)
        setSettings(prev => ({ ...prev, ...savedSettings }))
      } catch (error) {
        console.warn('Failed to load accessibility settings:', error)
      }
    }
  }, [])

  const announce = useCallback((message: string, priority: 'polite' | 'assertive' = 'polite') => {
    const targetRef = priority === 'assertive' ? announcementRef : politeAnnouncementRef
    
    if (targetRef.current) {
      // Clear previous announcement
      targetRef.current.textContent = ''
      
      // Add new announcement after a brief delay to ensure screen readers pick it up
      setTimeout(() => {
        if (targetRef.current) {
          targetRef.current.textContent = message
        }
      }, 100)
      
      // Clear announcement after it's been read
      setTimeout(() => {
        if (targetRef.current) {
          targetRef.current.textContent = ''
        }
      }, 5000)
    }
  }, [])

  const focusElement = useCallback((elementId: string) => {
    const element = document.getElementById(elementId)
    if (element) {
      element.focus()
      // Announce focus change for screen readers
      const label = element.getAttribute('aria-label') || 
                   element.getAttribute('title') || 
                   element.textContent || 
                   'Element'
      announce(`Focused on ${label}`)
    }
  }, [announce])

  const skipToContent = useCallback(() => {
    const mainContent = document.getElementById('main-content') || 
                       document.querySelector('main') ||
                       document.querySelector('[role="main"]')
    
    if (mainContent) {
      mainContent.focus()
      announce('Skipped to main content')
    }
  }, [announce])

  const skipToNavigation = useCallback(() => {
    const navigation = document.getElementById('main-navigation') ||
                      document.querySelector('nav') ||
                      document.querySelector('[role="navigation"]')
    
    if (navigation) {
      navigation.focus()
      announce('Skipped to navigation')
    }
  }, [announce])

  const contextValue: AccessibilityContextType = {
    settings,
    updateSettings,
    announce,
    focusElement,
    skipToContent,
    skipToNavigation,
    isKeyboardUser,
    setKeyboardUser: setIsKeyboardUser,
  }

  return (
    <AccessibilityContext.Provider value={contextValue}>
      {children}
      
      {/* Screen reader announcements */}
      <div
        ref={announcementRef}
        aria-live="assertive"
        aria-atomic="true"
        className="sr-only"
        role="status"
      />
      <div
        ref={politeAnnouncementRef}
        aria-live="polite"
        aria-atomic="true"
        className="sr-only"
        role="status"
      />
      
      {/* Skip links */}
      <div className="skip-links">
        <button
          className="skip-link"
          onClick={skipToContent}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault()
              skipToContent()
            }
          }}
        >
          Skip to main content
        </button>
        <button
          className="skip-link"
          onClick={skipToNavigation}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault()
              skipToNavigation()
            }
          }}
        >
          Skip to navigation
        </button>
      </div>
    </AccessibilityContext.Provider>
  )
}

export default AccessibilityProvider
