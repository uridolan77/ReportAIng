# Dark Mode Implementation

This document describes the comprehensive Dark Mode implementation for the BI Reporting Copilot application.

## Overview

The Dark Mode feature provides users with three theme options:
- **Light Mode**: Traditional light theme
- **Dark Mode**: Dark theme for reduced eye strain
- **Auto Mode**: Automatically follows system preference

## Architecture

### 1. Theme Context (`ThemeContext.tsx`)
- Centralized theme state management using React Context
- Persists theme preference in localStorage
- Listens to system theme changes
- Provides dynamic Ant Design theme configuration

### 2. Theme Toggle Component (`ThemeToggle.tsx`)
- Three variants: `button`, `icon`, `compact`
- Cycles through theme modes: Light → Dark → Auto → Light
- Accessible with proper ARIA labels and keyboard support
- Smooth animations and hover effects

### 3. CSS Variables System
Enhanced CSS variables in `variables.css` support both manual theme control and system preference fallback:

```css
/* Manual theme control */
[data-theme="dark"] { /* dark theme styles */ }

/* System preference fallback */
@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) { /* dark theme styles */ }
}
```

## Usage

### Basic Theme Toggle
```tsx
import { ThemeToggle } from './components/ThemeToggle';

// Icon variant (recommended for headers)
<ThemeToggle variant="icon" size="middle" />

// Button variant with label
<ThemeToggle variant="button" showLabel />

// Compact variant for menus
<ThemeToggle variant="compact" size="small" showLabel />
```

### Using Theme Context
```tsx
import { useTheme } from './contexts/ThemeContext';

const MyComponent = () => {
  const { themeMode, isDarkMode, toggleTheme, setThemeMode } = useTheme();
  
  return (
    <div className={isDarkMode ? 'dark-mode' : 'light-mode'}>
      Current theme: {themeMode}
    </div>
  );
};
```

## Integration Points

### 1. App.tsx
- Wrapped with `ThemeProvider`
- Uses dynamic Ant Design theme configuration
- Applies theme classes to document root

### 2. Layout Component
- Theme toggle in header (icon variant)
- Theme option in user dropdown menu (compact variant)
- Responsive design considerations

### 3. CSS Components
All major CSS files updated with dark mode support:
- `layout.css` - Header, sidebar, content areas
- `variables.css` - CSS custom properties
- `App.css` - Component-specific styles

## Features

### Accessibility
- WCAG compliant color contrast ratios
- Keyboard navigation support
- Screen reader friendly
- High contrast mode support
- Reduced motion preferences

### Performance
- CSS-only theme switching (no JavaScript re-renders)
- Efficient CSS variable system
- Smooth transitions with `cubic-bezier` easing
- Minimal bundle size impact

### User Experience
- Remembers user preference across sessions
- Respects system theme preference
- Smooth visual transitions
- Consistent styling across all components
- Visual feedback on theme changes

## Browser Support

- Modern browsers with CSS custom properties support
- Graceful fallback for older browsers
- System preference detection where supported

## Testing

Run the theme toggle tests:
```bash
npm test ThemeToggle.test.tsx
```

## Future Enhancements

1. **Additional Themes**: Support for custom color schemes
2. **Theme Scheduling**: Automatic theme switching based on time
3. **Component Themes**: Per-component theme overrides
4. **Theme Analytics**: Track user theme preferences
