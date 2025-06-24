import React, { useEffect, useRef, useCallback } from 'react'

/**
 * Accessibility utilities and hooks for transparency components
 */

// Screen reader announcements
export const announceToScreenReader = (message: string, priority: 'polite' | 'assertive' = 'polite') => {
  const announcement = document.createElement('div')
  announcement.setAttribute('aria-live', priority)
  announcement.setAttribute('aria-atomic', 'true')
  announcement.setAttribute('class', 'sr-only')
  announcement.style.position = 'absolute'
  announcement.style.left = '-10000px'
  announcement.style.width = '1px'
  announcement.style.height = '1px'
  announcement.style.overflow = 'hidden'
  
  document.body.appendChild(announcement)
  announcement.textContent = message
  
  // Remove after announcement
  setTimeout(() => {
    document.body.removeChild(announcement)
  }, 1000)
}

// Focus management hook
export const useFocusManagement = () => {
  const focusRef = useRef<HTMLElement | null>(null)
  
  const setFocus = useCallback((element: HTMLElement | null) => {
    if (element) {
      focusRef.current = element
      element.focus()
    }
  }, [])
  
  const restoreFocus = useCallback(() => {
    if (focusRef.current) {
      focusRef.current.focus()
    }
  }, [])
  
  return { setFocus, restoreFocus }
}

// Keyboard navigation hook
export const useKeyboardNavigation = (
  onEnter?: () => void,
  onEscape?: () => void,
  onArrowUp?: () => void,
  onArrowDown?: () => void,
  onArrowLeft?: () => void,
  onArrowRight?: () => void
) => {
  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    switch (event.key) {
      case 'Enter':
        onEnter?.()
        break
      case 'Escape':
        onEscape?.()
        break
      case 'ArrowUp':
        event.preventDefault()
        onArrowUp?.()
        break
      case 'ArrowDown':
        event.preventDefault()
        onArrowDown?.()
        break
      case 'ArrowLeft':
        onArrowLeft?.()
        break
      case 'ArrowRight':
        onArrowRight?.()
        break
    }
  }, [onEnter, onEscape, onArrowUp, onArrowDown, onArrowLeft, onArrowRight])

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [handleKeyDown])

  return handleKeyDown
}

// ARIA attributes generator
export const generateAriaAttributes = (options: {
  label?: string
  labelledBy?: string
  describedBy?: string
  expanded?: boolean
  selected?: boolean
  disabled?: boolean
  required?: boolean
  invalid?: boolean
  live?: 'polite' | 'assertive' | 'off'
  atomic?: boolean
  busy?: boolean
  controls?: string
  owns?: string
  role?: string
}) => {
  const attributes: Record<string, any> = {}

  if (options.label) attributes['aria-label'] = options.label
  if (options.labelledBy) attributes['aria-labelledby'] = options.labelledBy
  if (options.describedBy) attributes['aria-describedby'] = options.describedBy
  if (options.expanded !== undefined) attributes['aria-expanded'] = options.expanded
  if (options.selected !== undefined) attributes['aria-selected'] = options.selected
  if (options.disabled !== undefined) attributes['aria-disabled'] = options.disabled
  if (options.required !== undefined) attributes['aria-required'] = options.required
  if (options.invalid !== undefined) attributes['aria-invalid'] = options.invalid
  if (options.live) attributes['aria-live'] = options.live
  if (options.atomic !== undefined) attributes['aria-atomic'] = options.atomic
  if (options.busy !== undefined) attributes['aria-busy'] = options.busy
  if (options.controls) attributes['aria-controls'] = options.controls
  if (options.owns) attributes['aria-owns'] = options.owns
  if (options.role) attributes['role'] = options.role

  return attributes
}

// Skip link component
export const SkipLink: React.FC<{ href: string; children: React.ReactNode }> = ({ href, children }) => (
  <a
    href={href}
    style={{
      position: 'absolute',
      left: '-10000px',
      top: 'auto',
      width: '1px',
      height: '1px',
      overflow: 'hidden',
      zIndex: 9999,
      padding: '8px 16px',
      background: '#000',
      color: '#fff',
      textDecoration: 'none',
      borderRadius: '4px'
    }}
    onFocus={(e) => {
      e.target.style.left = '6px'
      e.target.style.top = '6px'
      e.target.style.width = 'auto'
      e.target.style.height = 'auto'
    }}
    onBlur={(e) => {
      e.target.style.left = '-10000px'
      e.target.style.top = 'auto'
      e.target.style.width = '1px'
      e.target.style.height = '1px'
    }}
  >
    {children}
  </a>
)

// High contrast mode detection
export const useHighContrastMode = () => {
  const [isHighContrast, setIsHighContrast] = React.useState(false)

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-contrast: high)')
    setIsHighContrast(mediaQuery.matches)

    const handleChange = (e: MediaQueryListEvent) => {
      setIsHighContrast(e.matches)
    }

    mediaQuery.addEventListener('change', handleChange)
    return () => mediaQuery.removeEventListener('change', handleChange)
  }, [])

  return isHighContrast
}

// Reduced motion detection
export const useReducedMotion = () => {
  const [prefersReducedMotion, setPrefersReducedMotion] = React.useState(false)

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)')
    setPrefersReducedMotion(mediaQuery.matches)

    const handleChange = (e: MediaQueryListEvent) => {
      setPrefersReducedMotion(e.matches)
    }

    mediaQuery.addEventListener('change', handleChange)
    return () => mediaQuery.removeEventListener('change', handleChange)
  }, [])

  return prefersReducedMotion
}

// Color contrast utilities
export const getContrastRatio = (color1: string, color2: string): number => {
  const getLuminance = (color: string): number => {
    const rgb = parseInt(color.slice(1), 16)
    const r = (rgb >> 16) & 0xff
    const g = (rgb >> 8) & 0xff
    const b = (rgb >> 0) & 0xff

    const [rs, gs, bs] = [r, g, b].map(c => {
      c = c / 255
      return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4)
    })

    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs
  }

  const lum1 = getLuminance(color1)
  const lum2 = getLuminance(color2)
  const brightest = Math.max(lum1, lum2)
  const darkest = Math.min(lum1, lum2)

  return (brightest + 0.05) / (darkest + 0.05)
}

export const meetsWCAGContrast = (color1: string, color2: string, level: 'AA' | 'AAA' = 'AA'): boolean => {
  const ratio = getContrastRatio(color1, color2)
  return level === 'AA' ? ratio >= 4.5 : ratio >= 7
}

// Focus trap hook
export const useFocusTrap = (isActive: boolean) => {
  const containerRef = useRef<HTMLElement>(null)

  useEffect(() => {
    if (!isActive || !containerRef.current) return

    const container = containerRef.current
    const focusableElements = container.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    )
    const firstElement = focusableElements[0] as HTMLElement
    const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement

    const handleTabKey = (e: KeyboardEvent) => {
      if (e.key !== 'Tab') return

      if (e.shiftKey) {
        if (document.activeElement === firstElement) {
          lastElement.focus()
          e.preventDefault()
        }
      } else {
        if (document.activeElement === lastElement) {
          firstElement.focus()
          e.preventDefault()
        }
      }
    }

    document.addEventListener('keydown', handleTabKey)
    firstElement?.focus()

    return () => {
      document.removeEventListener('keydown', handleTabKey)
    }
  }, [isActive])

  return containerRef
}

// Live region hook for dynamic content
export const useLiveRegion = () => {
  const liveRegionRef = useRef<HTMLDivElement>(null)

  const announce = useCallback((message: string, priority: 'polite' | 'assertive' = 'polite') => {
    if (liveRegionRef.current) {
      liveRegionRef.current.setAttribute('aria-live', priority)
      liveRegionRef.current.textContent = message
    }
  }, [])

  const LiveRegion = () => (
    <div
      ref={liveRegionRef}
      aria-live="polite"
      aria-atomic="true"
      className="sr-only"
      style={{
        position: 'absolute',
        left: '-10000px',
        width: '1px',
        height: '1px',
        overflow: 'hidden'
      }}
    />
  )

  return { announce, LiveRegion }
}

// Accessibility testing utilities
export const checkAccessibility = (element: HTMLElement): string[] => {
  const issues: string[] = []

  // Check for missing alt text on images
  const images = element.querySelectorAll('img')
  images.forEach(img => {
    if (!img.alt && !img.getAttribute('aria-label')) {
      issues.push('Image missing alt text')
    }
  })

  // Check for missing form labels
  const inputs = element.querySelectorAll('input, select, textarea')
  inputs.forEach(input => {
    const id = input.id
    const label = id ? element.querySelector(`label[for="${id}"]`) : null
    const ariaLabel = input.getAttribute('aria-label')
    const ariaLabelledBy = input.getAttribute('aria-labelledby')

    if (!label && !ariaLabel && !ariaLabelledBy) {
      issues.push('Form input missing label')
    }
  })

  // Check for missing heading hierarchy
  const headings = element.querySelectorAll('h1, h2, h3, h4, h5, h6')
  let lastLevel = 0
  headings.forEach(heading => {
    const level = parseInt(heading.tagName.charAt(1))
    if (level > lastLevel + 1) {
      issues.push('Heading hierarchy skipped')
    }
    lastLevel = level
  })

  // Check for missing focus indicators
  const focusableElements = element.querySelectorAll(
    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
  )
  focusableElements.forEach(el => {
    const styles = window.getComputedStyle(el, ':focus')
    if (styles.outline === 'none' && !styles.boxShadow && !styles.border) {
      issues.push('Focusable element missing focus indicator')
    }
  })

  return issues
}

export default {
  announceToScreenReader,
  useFocusManagement,
  useKeyboardNavigation,
  generateAriaAttributes,
  SkipLink,
  useHighContrastMode,
  useReducedMotion,
  getContrastRatio,
  meetsWCAGContrast,
  useFocusTrap,
  useLiveRegion,
  checkAccessibility
}
