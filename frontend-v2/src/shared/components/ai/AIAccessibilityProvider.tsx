import React, { createContext, useContext, useEffect, useRef, ReactNode } from 'react'
import { message } from 'antd'

// ============================================================================
// ACCESSIBILITY CONTEXT AND TYPES
// ============================================================================

interface AIAccessibilityContextType {
  announceToScreenReader: (message: string, priority?: 'polite' | 'assertive') => void
  focusElement: (elementId: string) => void
  setAriaLive: (elementId: string, value: 'polite' | 'assertive' | 'off') => void
  addKeyboardShortcut: (key: string, callback: () => void, description: string) => void
  removeKeyboardShortcut: (key: string) => void
  isHighContrast: boolean
  isReducedMotion: boolean
  fontSize: 'small' | 'medium' | 'large'
  setFontSize: (size: 'small' | 'medium' | 'large') => void
}

const AIAccessibilityContext = createContext<AIAccessibilityContextType | undefined>(undefined)

// ============================================================================
// ACCESSIBILITY PROVIDER
// ============================================================================

interface AIAccessibilityProviderProps {
  children: ReactNode
  enableKeyboardShortcuts?: boolean
  enableScreenReaderAnnouncements?: boolean
  enableHighContrastDetection?: boolean
}

export const AIAccessibilityProvider: React.FC<AIAccessibilityProviderProps> = ({
  children,
  enableKeyboardShortcuts = true,
  enableScreenReaderAnnouncements = true,
  enableHighContrastDetection = true
}) => {
  const [fontSize, setFontSize] = React.useState<'small' | 'medium' | 'large'>('medium')
  const [isHighContrast, setIsHighContrast] = React.useState(false)
  const [isReducedMotion, setIsReducedMotion] = React.useState(false)
  
  const screenReaderRef = useRef<HTMLDivElement>(null)
  const keyboardShortcuts = useRef<Map<string, { callback: () => void; description: string }>>(new Map())
  
  // Announce messages to screen readers
  const announceToScreenReader = React.useCallback((
    messageText: string, 
    priority: 'polite' | 'assertive' = 'polite'
  ) => {
    if (!enableScreenReaderAnnouncements || !screenReaderRef.current) return
    
    const announcement = document.createElement('div')
    announcement.setAttribute('aria-live', priority)
    announcement.setAttribute('aria-atomic', 'true')
    announcement.textContent = messageText
    announcement.style.position = 'absolute'
    announcement.style.left = '-10000px'
    announcement.style.width = '1px'
    announcement.style.height = '1px'
    announcement.style.overflow = 'hidden'
    
    screenReaderRef.current.appendChild(announcement)
    
    // Remove after announcement
    setTimeout(() => {
      if (screenReaderRef.current?.contains(announcement)) {
        screenReaderRef.current.removeChild(announcement)
      }
    }, 1000)
  }, [enableScreenReaderAnnouncements])
  
  // Focus element by ID
  const focusElement = React.useCallback((elementId: string) => {
    const element = document.getElementById(elementId)
    if (element) {
      element.focus()
      announceToScreenReader(`Focused on ${element.getAttribute('aria-label') || elementId}`)
    }
  }, [announceToScreenReader])
  
  // Set aria-live attribute
  const setAriaLive = React.useCallback((elementId: string, value: 'polite' | 'assertive' | 'off') => {
    const element = document.getElementById(elementId)
    if (element) {
      element.setAttribute('aria-live', value)
    }
  }, [])
  
  // Add keyboard shortcut
  const addKeyboardShortcut = React.useCallback((
    key: string, 
    callback: () => void, 
    description: string
  ) => {
    if (!enableKeyboardShortcuts) return
    
    keyboardShortcuts.current.set(key, { callback, description })
  }, [enableKeyboardShortcuts])
  
  // Remove keyboard shortcut
  const removeKeyboardShortcut = React.useCallback((key: string) => {
    keyboardShortcuts.current.delete(key)
  }, [])
  
  // Handle keyboard events
  React.useEffect(() => {
    if (!enableKeyboardShortcuts) return
    
    const handleKeyDown = (event: KeyboardEvent) => {
      const key = `${event.ctrlKey ? 'Ctrl+' : ''}${event.altKey ? 'Alt+' : ''}${event.shiftKey ? 'Shift+' : ''}${event.key}`
      const shortcut = keyboardShortcuts.current.get(key)
      
      if (shortcut) {
        event.preventDefault()
        shortcut.callback()
        announceToScreenReader(`Executed shortcut: ${shortcut.description}`)
      }
    }
    
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [enableKeyboardShortcuts, announceToScreenReader])
  
  // Detect high contrast mode
  React.useEffect(() => {
    if (!enableHighContrastDetection) return
    
    const mediaQuery = window.matchMedia('(prefers-contrast: high)')
    setIsHighContrast(mediaQuery.matches)
    
    const handleChange = (e: MediaQueryListEvent) => {
      setIsHighContrast(e.matches)
    }
    
    mediaQuery.addEventListener('change', handleChange)
    return () => mediaQuery.removeEventListener('change', handleChange)
  }, [enableHighContrastDetection])
  
  // Detect reduced motion preference
  React.useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)')
    setIsReducedMotion(mediaQuery.matches)
    
    const handleChange = (e: MediaQueryListEvent) => {
      setIsReducedMotion(e.matches)
    }
    
    mediaQuery.addEventListener('change', handleChange)
    return () => mediaQuery.removeEventListener('change', handleChange)
  }, [])
  
  // Apply font size to document
  React.useEffect(() => {
    const fontSizeMap = {
      small: '14px',
      medium: '16px',
      large: '18px'
    }
    
    document.documentElement.style.setProperty('--ai-font-size', fontSizeMap[fontSize])
  }, [fontSize])
  
  const contextValue: AIAccessibilityContextType = {
    announceToScreenReader,
    focusElement,
    setAriaLive,
    addKeyboardShortcut,
    removeKeyboardShortcut,
    isHighContrast,
    isReducedMotion,
    fontSize,
    setFontSize
  }
  
  return (
    <AIAccessibilityContext.Provider value={contextValue}>
      {children}
      {/* Screen reader announcement area */}
      <div
        ref={screenReaderRef}
        aria-live="polite"
        aria-atomic="true"
        style={{
          position: 'absolute',
          left: '-10000px',
          width: '1px',
          height: '1px',
          overflow: 'hidden'
        }}
      />
    </AIAccessibilityContext.Provider>
  )
}

// ============================================================================
// CUSTOM HOOKS
// ============================================================================

export const useAIAccessibility = () => {
  const context = useContext(AIAccessibilityContext)
  if (context === undefined) {
    throw new Error('useAIAccessibility must be used within an AIAccessibilityProvider')
  }
  return context
}

/**
 * Hook for managing focus within AI components
 */
export const useAIFocusManagement = (componentId: string) => {
  const { focusElement, announceToScreenReader } = useAIAccessibility()
  const focusableElements = useRef<HTMLElement[]>([])
  const currentFocusIndex = useRef(0)
  
  // Get focusable elements within component
  const updateFocusableElements = React.useCallback(() => {
    const container = document.getElementById(componentId)
    if (!container) return
    
    const focusableSelectors = [
      'button:not([disabled])',
      'input:not([disabled])',
      'select:not([disabled])',
      'textarea:not([disabled])',
      'a[href]',
      '[tabindex]:not([tabindex="-1"])'
    ].join(', ')
    
    focusableElements.current = Array.from(container.querySelectorAll(focusableSelectors))
  }, [componentId])
  
  // Navigate to next focusable element
  const focusNext = React.useCallback(() => {
    updateFocusableElements()
    if (focusableElements.current.length === 0) return
    
    currentFocusIndex.current = (currentFocusIndex.current + 1) % focusableElements.current.length
    const element = focusableElements.current[currentFocusIndex.current]
    element.focus()
    announceToScreenReader(`Focused on ${element.getAttribute('aria-label') || element.tagName}`)
  }, [updateFocusableElements, announceToScreenReader])
  
  // Navigate to previous focusable element
  const focusPrevious = React.useCallback(() => {
    updateFocusableElements()
    if (focusableElements.current.length === 0) return
    
    currentFocusIndex.current = currentFocusIndex.current === 0 
      ? focusableElements.current.length - 1 
      : currentFocusIndex.current - 1
    const element = focusableElements.current[currentFocusIndex.current]
    element.focus()
    announceToScreenReader(`Focused on ${element.getAttribute('aria-label') || element.tagName}`)
  }, [updateFocusableElements, announceToScreenReader])
  
  // Focus first element
  const focusFirst = React.useCallback(() => {
    updateFocusableElements()
    if (focusableElements.current.length === 0) return
    
    currentFocusIndex.current = 0
    const element = focusableElements.current[0]
    element.focus()
    announceToScreenReader(`Focused on first element: ${element.getAttribute('aria-label') || element.tagName}`)
  }, [updateFocusableElements, announceToScreenReader])
  
  return {
    focusNext,
    focusPrevious,
    focusFirst,
    updateFocusableElements
  }
}

/**
 * Hook for AI component keyboard navigation
 */
export const useAIKeyboardNavigation = (componentId: string, shortcuts: Record<string, () => void> = {}) => {
  const { addKeyboardShortcut, removeKeyboardShortcut } = useAIAccessibility()
  const { focusNext, focusPrevious, focusFirst } = useAIFocusManagement(componentId)
  
  React.useEffect(() => {
    // Default navigation shortcuts
    addKeyboardShortcut('Tab', focusNext, 'Navigate to next element')
    addKeyboardShortcut('Shift+Tab', focusPrevious, 'Navigate to previous element')
    addKeyboardShortcut('Home', focusFirst, 'Focus first element')
    
    // Custom shortcuts
    Object.entries(shortcuts).forEach(([key, callback]) => {
      addKeyboardShortcut(key, callback, `Custom shortcut: ${key}`)
    })
    
    return () => {
      removeKeyboardShortcut('Tab')
      removeKeyboardShortcut('Shift+Tab')
      removeKeyboardShortcut('Home')
      
      Object.keys(shortcuts).forEach(key => {
        removeKeyboardShortcut(key)
      })
    }
  }, [addKeyboardShortcut, removeKeyboardShortcut, focusNext, focusPrevious, focusFirst, shortcuts])
}

/**
 * Hook for AI component announcements
 */
export const useAIAnnouncements = () => {
  const { announceToScreenReader } = useAIAccessibility()
  
  const announceLoading = React.useCallback((componentName: string) => {
    announceToScreenReader(`${componentName} is loading`, 'polite')
  }, [announceToScreenReader])
  
  const announceLoaded = React.useCallback((componentName: string) => {
    announceToScreenReader(`${componentName} has loaded`, 'polite')
  }, [announceToScreenReader])
  
  const announceError = React.useCallback((componentName: string, error: string) => {
    announceToScreenReader(`Error in ${componentName}: ${error}`, 'assertive')
  }, [announceToScreenReader])
  
  const announceSuccess = React.useCallback((action: string) => {
    announceToScreenReader(`${action} completed successfully`, 'polite')
  }, [announceToScreenReader])
  
  const announceConfidence = React.useCallback((confidence: number) => {
    const percentage = Math.round(confidence * 100)
    announceToScreenReader(`AI confidence: ${percentage} percent`, 'polite')
  }, [announceToScreenReader])
  
  return {
    announceLoading,
    announceLoaded,
    announceError,
    announceSuccess,
    announceConfidence,
    announce: announceToScreenReader
  }
}

// ============================================================================
// ACCESSIBILITY UTILITIES
// ============================================================================

/**
 * Generate accessible IDs for AI components
 */
export const generateAccessibleId = (prefix: string, suffix?: string) => {
  const timestamp = Date.now()
  const random = Math.random().toString(36).substr(2, 9)
  return `${prefix}-${timestamp}-${random}${suffix ? `-${suffix}` : ''}`
}

/**
 * Create ARIA attributes for AI components
 */
export const createAriaAttributes = (options: {
  label?: string
  describedBy?: string
  expanded?: boolean
  hasPopup?: boolean
  live?: 'polite' | 'assertive' | 'off'
  atomic?: boolean
  busy?: boolean
  invalid?: boolean
  required?: boolean
}) => {
  const attributes: Record<string, any> = {}
  
  if (options.label) attributes['aria-label'] = options.label
  if (options.describedBy) attributes['aria-describedby'] = options.describedBy
  if (options.expanded !== undefined) attributes['aria-expanded'] = options.expanded
  if (options.hasPopup) attributes['aria-haspopup'] = options.hasPopup
  if (options.live) attributes['aria-live'] = options.live
  if (options.atomic !== undefined) attributes['aria-atomic'] = options.atomic
  if (options.busy !== undefined) attributes['aria-busy'] = options.busy
  if (options.invalid !== undefined) attributes['aria-invalid'] = options.invalid
  if (options.required !== undefined) attributes['aria-required'] = options.required
  
  return attributes
}

/**
 * HOC for adding accessibility features to AI components
 */
export const withAIAccessibility = <T extends React.ComponentType<any>>(
  Component: T,
  options: {
    componentName: string
    enableKeyboardNavigation?: boolean
    enableAnnouncements?: boolean
    customShortcuts?: Record<string, () => void>
  }
) => {
  const AccessibleComponent = React.forwardRef<any, React.ComponentProps<T>>((props, ref) => {
    const componentId = React.useMemo(() => generateAccessibleId(options.componentName), [])
    const { announceLoaded, announceError } = useAIAnnouncements()
    
    // Setup keyboard navigation
    useAIKeyboardNavigation(
      componentId, 
      options.enableKeyboardNavigation ? options.customShortcuts : {}
    )
    
    // Announce component lifecycle
    React.useEffect(() => {
      if (options.enableAnnouncements) {
        announceLoaded(options.componentName)
      }
    }, [announceLoaded])
    
    // Handle errors
    const handleError = React.useCallback((error: Error) => {
      if (options.enableAnnouncements) {
        announceError(options.componentName, error.message)
      }
    }, [announceError])
    
    return (
      <div id={componentId} onError={handleError}>
        <Component ref={ref} {...props} />
      </div>
    )
  })
  
  AccessibleComponent.displayName = `Accessible(${Component.displayName || Component.name})`
  return AccessibleComponent
}

export default AIAccessibilityProvider
