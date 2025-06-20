import { useState, useEffect } from 'react'

export interface BreakpointValues {
  xs: boolean  // < 576px
  sm: boolean  // >= 576px
  md: boolean  // >= 768px
  lg: boolean  // >= 992px
  xl: boolean  // >= 1200px
  xxl: boolean // >= 1600px
}

export interface ResponsiveInfo extends BreakpointValues {
  isMobile: boolean
  isTablet: boolean
  isDesktop: boolean
  screenWidth: number
  screenHeight: number
  orientation: 'portrait' | 'landscape'
}

const breakpoints = {
  xs: 0,
  sm: 576,
  md: 768,
  lg: 992,
  xl: 1200,
  xxl: 1600,
}

export const useResponsive = (): ResponsiveInfo => {
  const [screenSize, setScreenSize] = useState({
    width: typeof window !== 'undefined' ? window.innerWidth : 1200,
    height: typeof window !== 'undefined' ? window.innerHeight : 800,
  })

  useEffect(() => {
    const handleResize = () => {
      setScreenSize({
        width: window.innerWidth,
        height: window.innerHeight,
      })
    }

    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  const { width, height } = screenSize

  const breakpointValues: BreakpointValues = {
    xs: width >= breakpoints.xs && width < breakpoints.sm,
    sm: width >= breakpoints.sm && width < breakpoints.md,
    md: width >= breakpoints.md && width < breakpoints.lg,
    lg: width >= breakpoints.lg && width < breakpoints.xl,
    xl: width >= breakpoints.xl && width < breakpoints.xxl,
    xxl: width >= breakpoints.xxl,
  }

  return {
    ...breakpointValues,
    isMobile: width < breakpoints.md,
    isTablet: width >= breakpoints.md && width < breakpoints.lg,
    isDesktop: width >= breakpoints.lg,
    screenWidth: width,
    screenHeight: height,
    orientation: width > height ? 'landscape' : 'portrait',
  }
}

export const useBreakpoint = (breakpoint: keyof typeof breakpoints): boolean => {
  const responsive = useResponsive()
  return responsive.screenWidth >= breakpoints[breakpoint]
}

export const useMediaQuery = (query: string): boolean => {
  const [matches, setMatches] = useState(false)

  useEffect(() => {
    if (typeof window === 'undefined') return

    const mediaQuery = window.matchMedia(query)
    setMatches(mediaQuery.matches)

    const handler = (event: MediaQueryListEvent) => {
      setMatches(event.matches)
    }

    mediaQuery.addEventListener('change', handler)
    return () => mediaQuery.removeEventListener('change', handler)
  }, [query])

  return matches
}

// Utility functions for responsive design
export const getResponsiveValue = <T>(
  values: Partial<Record<keyof BreakpointValues, T>>,
  responsive: ResponsiveInfo
): T | undefined => {
  const orderedBreakpoints: (keyof BreakpointValues)[] = ['xxl', 'xl', 'lg', 'md', 'sm', 'xs']
  
  for (const breakpoint of orderedBreakpoints) {
    if (responsive[breakpoint] && values[breakpoint] !== undefined) {
      return values[breakpoint]
    }
  }
  
  return undefined
}

export const getResponsiveSpacing = (responsive: ResponsiveInfo) => ({
  padding: responsive.isMobile ? '12px' : responsive.isTablet ? '16px' : '24px',
  margin: responsive.isMobile ? '8px' : responsive.isTablet ? '12px' : '16px',
  gap: responsive.isMobile ? '8px' : responsive.isTablet ? '12px' : '16px',
})

export const getResponsiveFontSize = (responsive: ResponsiveInfo) => ({
  small: responsive.isMobile ? '12px' : '13px',
  normal: responsive.isMobile ? '14px' : '15px',
  large: responsive.isMobile ? '16px' : '18px',
  title: responsive.isMobile ? '20px' : responsive.isTablet ? '24px' : '28px',
  heading: responsive.isMobile ? '24px' : responsive.isTablet ? '32px' : '40px',
})

export const getResponsiveColumns = (responsive: ResponsiveInfo) => {
  if (responsive.isMobile) return 1
  if (responsive.isTablet) return 2
  return responsive.screenWidth > 1400 ? 4 : 3
}

export const getResponsiveCardSize = (responsive: ResponsiveInfo) => {
  if (responsive.isMobile) return 'small'
  return 'default'
}

export const getResponsiveTableSize = (responsive: ResponsiveInfo) => {
  if (responsive.isMobile) return 'small'
  return 'middle'
}

export const getResponsiveButtonSize = (responsive: ResponsiveInfo) => {
  if (responsive.isMobile) return 'middle'
  return 'large'
}

// CSS-in-JS responsive utilities
export const createResponsiveStyles = (responsive: ResponsiveInfo) => ({
  container: {
    maxWidth: responsive.isMobile ? '100%' : responsive.isTablet ? '768px' : '1200px',
    margin: '0 auto',
    padding: getResponsiveSpacing(responsive).padding,
  },
  
  grid: {
    display: 'grid',
    gridTemplateColumns: `repeat(${getResponsiveColumns(responsive)}, 1fr)`,
    gap: getResponsiveSpacing(responsive).gap,
  },
  
  flexContainer: {
    display: 'flex',
    flexDirection: responsive.isMobile ? 'column' as const : 'row' as const,
    gap: getResponsiveSpacing(responsive).gap,
  },
  
  hiddenOnMobile: {
    display: responsive.isMobile ? 'none' : 'block',
  },
  
  hiddenOnDesktop: {
    display: responsive.isDesktop ? 'none' : 'block',
  },
  
  mobileOnly: {
    display: responsive.isMobile ? 'block' : 'none',
  },
  
  desktopOnly: {
    display: responsive.isDesktop ? 'block' : 'none',
  },
})

// Accessibility helpers
export const getAccessibilityProps = (responsive: ResponsiveInfo) => ({
  // Touch targets should be at least 44px on mobile
  minTouchTarget: responsive.isMobile ? '44px' : '32px',
  
  // Focus indicators should be more prominent on mobile
  focusOutlineWidth: responsive.isMobile ? '3px' : '2px',
  
  // Text should be larger on mobile for readability
  minFontSize: responsive.isMobile ? '16px' : '14px',
})

// Performance optimization for mobile
export const shouldReduceAnimations = (responsive: ResponsiveInfo): boolean => {
  // Reduce animations on mobile for better performance
  return responsive.isMobile || 
         (typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches)
}

export const getOptimalImageSize = (responsive: ResponsiveInfo) => {
  if (responsive.isMobile) return { width: 400, height: 300 }
  if (responsive.isTablet) return { width: 600, height: 400 }
  return { width: 800, height: 600 }
}
