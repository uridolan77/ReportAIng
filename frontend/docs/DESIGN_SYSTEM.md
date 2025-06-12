# ReportAIng Design System

## Overview

The ReportAIng Design System is a comprehensive collection of design tokens, components, and guidelines that ensure consistency, accessibility, and maintainability across the entire application. This system provides a unified visual language and interaction patterns for all user interfaces.

## 🎨 Design Principles

### 1. **Consistency**
- Unified visual language across all components
- Standardized spacing, typography, and color usage
- Predictable interaction patterns

### 2. **Accessibility**
- WCAG 2.1 AA compliance
- Proper color contrast ratios
- Keyboard navigation support
- Screen reader compatibility

### 3. **Scalability**
- Modular component architecture
- Flexible design tokens
- Responsive design patterns

### 4. **Performance**
- Optimized CSS delivery
- Minimal runtime overhead
- Efficient animation system

## 🏗️ Architecture

### Design System Structure
```
src/styles/design-system/
├── index.css           # Main entry point
├── tokens.css          # Design tokens (colors, spacing, etc.)
├── typography.css      # Typography system
├── layout.css          # Layout utilities and grid
├── components.css      # Component styles
├── animations.css      # Animation system
└── antd-theme.ts      # Ant Design theme configuration
```

### Import Order
The design system files are imported in a specific order to ensure proper cascade:
1. **Tokens** - CSS custom properties and variables
2. **Typography** - Font families, sizes, and text styles
3. **Layout** - Grid system and spacing utilities
4. **Components** - Standardized component styles
5. **Animations** - Transitions and micro-interactions

## 🎯 Design Tokens

### Color System
```css
/* Primary Brand Colors */
--brand-primary: #3b82f6;
--brand-primary-hover: #2563eb;
--brand-primary-active: #1d4ed8;

/* Semantic Colors */
--color-success: #10b981;
--color-warning: #f59e0b;
--color-error: #ef4444;
--color-info: #3b82f6;

/* Neutral Colors (Light Mode) */
--neutral-0: #ffffff;
--neutral-50: #f9fafb;
--neutral-100: #f3f4f6;
--neutral-900: #111827;
```

### Spacing System (8pt Grid)
```css
--space-1: 4px;   /* 0.25rem */
--space-2: 8px;   /* 0.5rem */
--space-3: 12px;  /* 0.75rem */
--space-4: 16px;  /* 1rem */
--space-6: 24px;  /* 1.5rem */
--space-8: 32px;  /* 2rem */
```

### Typography Scale
```css
--font-size-xs: 12px;
--font-size-sm: 14px;
--font-size-base: 16px;
--font-size-lg: 18px;
--font-size-xl: 20px;
--font-size-2xl: 24px;
```

### Border Radius
```css
--radius-sm: 6px;
--radius-base: 8px;
--radius-md: 12px;
--radius-lg: 16px;
--radius-xl: 20px;
```

## 📝 Typography System

### Font Families
- **Primary**: Inter (UI text)
- **Secondary**: Poppins (headings)
- **Monospace**: JetBrains Mono (code)

### Heading Hierarchy
```css
h1 { font-size: 36px; font-weight: 700; }
h2 { font-size: 30px; font-weight: 700; }
h3 { font-size: 24px; font-weight: 600; }
h4 { font-size: 20px; font-weight: 600; }
h5 { font-size: 18px; font-weight: 500; }
h6 { font-size: 16px; font-weight: 500; }
```

### Text Utilities
```css
.text-primary    /* Primary text color */
.text-secondary  /* Secondary text color */
.text-tertiary   /* Tertiary text color */
.font-medium     /* Medium font weight */
.font-semibold   /* Semibold font weight */
.font-bold       /* Bold font weight */
```

## 🧩 Component System

### Button Components
```css
.btn              /* Base button styles */
.btn-primary      /* Primary button variant */
.btn-secondary    /* Secondary button variant */
.btn-outline      /* Outline button variant */
.btn-ghost        /* Ghost button variant */
.btn-sm           /* Small button size */
.btn-lg           /* Large button size */
```

### Card Components
```css
.card             /* Base card styles */
.card-header      /* Card header section */
.card-content     /* Card content section */
.card-footer      /* Card footer section */
.card-elevated    /* Elevated card with shadow */
.card-interactive /* Interactive card with hover effects */
```

### Form Components
```css
.input            /* Base input styles */
.input-sm         /* Small input size */
.input-lg         /* Large input size */
.input-error      /* Error state styling */
.input-success    /* Success state styling */
```

## 📐 Layout System

### Container System
```css
.container        /* Responsive container */
.container-fluid  /* Full-width container */
.container-narrow /* Narrow container (768px) */
.container-wide   /* Wide container (1600px) */
```

### Grid System
```css
.grid             /* CSS Grid container */
.grid-cols-1      /* 1 column grid */
.grid-cols-2      /* 2 column grid */
.grid-cols-3      /* 3 column grid */
.grid-cols-4      /* 4 column grid */
```

### Flexbox Utilities
```css
.flex             /* Display flex */
.flex-col         /* Flex direction column */
.justify-center   /* Justify content center */
.items-center     /* Align items center */
.gap-4            /* Gap of 16px */
```

### Spacing Utilities
```css
.m-4              /* Margin 16px */
.p-4              /* Padding 16px */
.mt-4             /* Margin top 16px */
.mb-4             /* Margin bottom 16px */
.ml-4             /* Margin left 16px */
.mr-4             /* Margin right 16px */
```

## 🎬 Animation System

### Transition Classes
```css
.transition-all     /* Transition all properties */
.transition-colors  /* Transition color properties */
.duration-fast      /* 150ms duration */
.duration-normal    /* 300ms duration */
.duration-slow      /* 500ms duration */
```

### Animation Classes
```css
.animate-fade-in      /* Fade in animation */
.animate-slide-in-up  /* Slide in from bottom */
.animate-bounce       /* Bounce animation */
.animate-pulse        /* Pulse animation */
.animate-spin         /* Spin animation */
```

### Hover Effects
```css
.hover-lift         /* Lift on hover */
.hover-scale        /* Scale on hover */
.hover-glow         /* Glow effect on hover */
```

## 🌙 Dark Mode Support

The design system includes comprehensive dark mode support:

```css
[data-theme="dark"] {
  --surface-primary: #1a1a1a;
  --surface-secondary: #262626;
  --text-primary: #ffffff;
  --text-secondary: #a3a3a3;
  --border-primary: #404040;
}
```

### Theme Switching
```typescript
import { useTheme } from '../contexts/ThemeContext';

const { isDarkMode, toggleTheme } = useTheme();
```

## ♿ Accessibility Features

### Focus Management
- Visible focus indicators
- Logical tab order
- Skip links for navigation

### Color Contrast
- WCAG AA compliant contrast ratios
- High contrast mode support
- Color-blind friendly palette

### Screen Reader Support
- Semantic HTML structure
- ARIA labels and descriptions
- Screen reader only content

## 📱 Responsive Design

### Breakpoints
```css
--breakpoint-sm: 640px;
--breakpoint-md: 768px;
--breakpoint-lg: 1024px;
--breakpoint-xl: 1280px;
```

### Responsive Utilities
```css
.hidden-mobile      /* Hide on mobile */
.block-mobile       /* Show on mobile */
.grid-cols-1        /* 1 column on mobile */
.grid-cols-2        /* 2 columns on tablet+ */
```

## 🚀 Usage Guidelines

### Getting Started
1. Import the design system in your main CSS file:
```css
@import './styles/design-system/index.css';
```

2. Use design tokens in your components:
```css
.my-component {
  padding: var(--space-4);
  background: var(--surface-primary);
  border-radius: var(--radius-base);
  color: var(--text-primary);
}
```

3. Apply utility classes for common patterns:
```jsx
<div className="flex items-center gap-4 p-6">
  <Button className="btn-primary">Primary Action</Button>
  <Button className="btn-secondary">Secondary Action</Button>
</div>
```

### Best Practices
1. **Use design tokens** instead of hardcoded values
2. **Follow the spacing system** (8pt grid)
3. **Maintain consistent typography** hierarchy
4. **Test in both light and dark modes**
5. **Ensure accessibility compliance**
6. **Use semantic HTML** elements
7. **Follow responsive design patterns**

### Component Development
When creating new components:
1. Use existing design tokens
2. Follow established patterns
3. Include hover and focus states
4. Support dark mode
5. Add proper ARIA attributes
6. Test with keyboard navigation

## 🔧 Customization

### Extending the Theme
```typescript
// Custom theme extension
export const customTheme = {
  ...createTheme(isDarkMode),
  token: {
    ...createTheme(isDarkMode).token,
    colorPrimary: '#your-custom-color',
  },
};
```

### Adding Custom Tokens
```css
:root {
  /* Custom design tokens */
  --custom-color: #your-color;
  --custom-spacing: 20px;
  --custom-radius: 10px;
}
```

## 📊 Performance Considerations

### CSS Optimization
- CSS custom properties for theming
- Minimal specificity conflicts
- Efficient selector usage
- Tree-shaking friendly structure

### Animation Performance
- GPU-accelerated animations
- Reduced motion support
- Optimized keyframes
- Will-change properties

## 🧪 Testing

### Visual Regression Testing
- Component screenshots
- Cross-browser testing
- Responsive breakpoint testing
- Dark/light mode testing

### Accessibility Testing
- Screen reader testing
- Keyboard navigation testing
- Color contrast validation
- ARIA attribute verification

## 📚 Resources

### Tools
- [Figma Design Files](link-to-figma)
- [Storybook Component Library](link-to-storybook)
- [Design Token Documentation](link-to-tokens)

### References
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Ant Design Documentation](https://ant.design/)
- [CSS Custom Properties](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)

---

*This design system is continuously evolving. For questions or contributions, please refer to our development guidelines.*
