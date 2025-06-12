/**
 * Page Standardization Utility
 *
 * This utility provides the OFFICIAL standardized styling patterns for all pages
 * to ensure consistent full-width layout, typography, and spacing across the application.
 *
 * STANDARD DESIGN: Full-width layout with optimal screen utilization
 * - 100% width containers for maximum space usage
 * - Consistent Inter font family throughout
 * - Standardized spacing and typography scale
 * - Professional, modern appearance
 */

// Standardized CSS classes for consistent styling
export const STANDARD_CLASSES = {
  // Layout classes
  APP_CONTAINER: 'app-container',
  PAGE_CONTAINER: 'page-container', 
  CONTENT_CONTAINER: 'content-container',
  SECTION_CONTAINER: 'section-container',
  FULL_WIDTH: 'full-width',
  FULL_WIDTH_CONTENT: 'full-width-content',

  // Typography classes
  TEXT_XS: 'text-xs',
  TEXT_SM: 'text-sm', 
  TEXT_BASE: 'text-base',
  TEXT_LG: 'text-lg',
  TEXT_XL: 'text-xl',
  TEXT_2XL: 'text-2xl',
  TEXT_3XL: 'text-3xl',
  TEXT_4XL: 'text-4xl',

  // Font weight classes
  FONT_NORMAL: 'font-normal',
  FONT_MEDIUM: 'font-medium',
  FONT_SEMIBOLD: 'font-semibold',
  FONT_BOLD: 'font-bold',

  // Font family classes
  FONT_PRIMARY: 'font-primary',
  FONT_MONO: 'font-mono',

  // Line height classes
  LEADING_TIGHT: 'leading-tight',
  LEADING_NORMAL: 'leading-normal',
  LEADING_RELAXED: 'leading-relaxed',
} as const;

// Standardized inline styles for components that need them
export const STANDARD_STYLES = {
  // Full width layout
  FULL_WIDTH_LAYOUT: {
    width: '100%',
    maxWidth: '100%',
    margin: 0,
    padding: 'var(--container-padding)',
  },

  // Page container
  PAGE_CONTAINER: {
    width: '100%',
    maxWidth: '100%',
    margin: 0,
    fontFamily: 'var(--font-family-primary)',
    fontSize: 'var(--text-base)',
    lineHeight: 'var(--line-height-normal)',
  },

  // Content container
  CONTENT_CONTAINER: {
    width: '100%',
    maxWidth: '100%',
    margin: 0,
    padding: 0,
  },

  // Section spacing
  SECTION_SPACING: {
    marginBottom: 'var(--section-spacing)',
  },

  // Typography styles
  HEADING_1: {
    fontSize: 'var(--text-4xl)',
    fontWeight: 'var(--font-weight-bold)',
    fontFamily: 'var(--font-family-primary)',
    lineHeight: 'var(--line-height-tight)',
    margin: 0,
  },

  HEADING_2: {
    fontSize: 'var(--text-3xl)',
    fontWeight: 'var(--font-weight-bold)',
    fontFamily: 'var(--font-family-primary)',
    lineHeight: 'var(--line-height-tight)',
    margin: 0,
  },

  HEADING_3: {
    fontSize: 'var(--text-2xl)',
    fontWeight: 'var(--font-weight-semibold)',
    fontFamily: 'var(--font-family-primary)',
    lineHeight: 'var(--line-height-tight)',
    margin: 0,
  },

  BODY_TEXT: {
    fontSize: 'var(--text-base)',
    fontWeight: 'var(--font-weight-normal)',
    fontFamily: 'var(--font-family-primary)',
    lineHeight: 'var(--line-height-normal)',
  },

  SMALL_TEXT: {
    fontSize: 'var(--text-sm)',
    fontWeight: 'var(--font-weight-normal)',
    fontFamily: 'var(--font-family-primary)',
    lineHeight: 'var(--line-height-normal)',
  },
} as const;

// Page-specific standardization patterns
export const PAGE_PATTERNS = {
  // Dashboard page pattern - OFFICIAL STANDARD
  DASHBOARD: {
    containerClass: STANDARD_CLASSES.FULL_WIDTH_CONTENT,
    titleStyle: STANDARD_STYLES.HEADING_1,
    subtitleStyle: STANDARD_STYLES.BODY_TEXT,
    sectionSpacing: STANDARD_STYLES.SECTION_SPACING,
    // STANDARD: Titles and subtitles on background, not in white panels
    titleOnBackground: true,
    subtitleOnBackground: true,
  },

  // Query page pattern - OFFICIAL STANDARD
  QUERY: {
    containerClass: STANDARD_CLASSES.FULL_WIDTH_CONTENT,
    containerStyle: STANDARD_STYLES.FULL_WIDTH_LAYOUT,
    titleOnBackground: true,
    subtitleOnBackground: true,
  },

  // History page pattern - OFFICIAL STANDARD
  HISTORY: {
    containerClass: STANDARD_CLASSES.FULL_WIDTH_CONTENT,
    titleStyle: STANDARD_STYLES.HEADING_1,
    subtitleStyle: STANDARD_STYLES.BODY_TEXT,
    titleOnBackground: true,
    subtitleOnBackground: true,
  },

  // Templates page pattern - OFFICIAL STANDARD
  TEMPLATES: {
    containerClass: STANDARD_CLASSES.FULL_WIDTH_CONTENT,
    titleStyle: STANDARD_STYLES.HEADING_1,
    subtitleStyle: STANDARD_STYLES.BODY_TEXT,
    titleOnBackground: true,
    subtitleOnBackground: true,
  },

  // Visualization page pattern - OFFICIAL STANDARD
  VISUALIZATION: {
    containerClass: STANDARD_CLASSES.FULL_WIDTH_CONTENT,
    titleStyle: STANDARD_STYLES.HEADING_1,
    subtitleStyle: STANDARD_STYLES.BODY_TEXT,
    titleOnBackground: true,
    subtitleOnBackground: true,
  },

  // Results page pattern - OFFICIAL STANDARD
  RESULTS: {
    containerClass: STANDARD_CLASSES.FULL_WIDTH_CONTENT,
    titleStyle: STANDARD_STYLES.HEADING_1,
    subtitleStyle: STANDARD_STYLES.BODY_TEXT,
    titleOnBackground: true,
    subtitleOnBackground: true,
  },
} as const;

// Utility functions for applying standardization
export const applyStandardization = {
  // Apply full width to a page component
  fullWidth: (element: HTMLElement) => {
    element.style.width = '100%';
    element.style.maxWidth = '100%';
    element.style.margin = '0';
  },

  // Apply standard typography
  typography: (element: HTMLElement, variant: keyof typeof STANDARD_STYLES) => {
    const styles = STANDARD_STYLES[variant];
    Object.assign(element.style, styles);
  },

  // Apply standard container styles
  container: (element: HTMLElement) => {
    Object.assign(element.style, STANDARD_STYLES.PAGE_CONTAINER);
  },
};

// Export default standardization config
export const STANDARDIZATION_CONFIG = {
  classes: STANDARD_CLASSES,
  styles: STANDARD_STYLES,
  patterns: PAGE_PATTERNS,
  utils: applyStandardization,
} as const;

export default STANDARDIZATION_CONFIG;
