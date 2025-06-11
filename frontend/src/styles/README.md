# 🎨 Design System & CSS Architecture

This document outlines the organized CSS architecture for the BI Reporting Copilot frontend application.

## 📁 File Structure

```
frontend/src/styles/
├── foundation/           # Core foundation styles
│   ├── reset.css        # Global reset & base styles
│   ├── typography.css   # Typography system
│   └── layout.css       # Layout utilities & grid system
├── components/          # Component-specific styles
│   ├── antd-overrides.css  # Ant Design customizations
│   └── ui-components.css   # Custom UI component styles
├── utilities/           # Utility classes & helpers
│   ├── focus.css       # Focus states & accessibility
│   ├── scrollbars.css  # Custom scrollbar styles
│   ├── loading.css     # Loading states & animations
│   └── responsive.css  # Responsive design utilities
└── README.md           # This documentation file
```

## 🏗️ Architecture Principles

### 1. **Layered Architecture**
- **Foundation Layer**: Core styles that everything builds upon
- **Component Layer**: Specific component styling and overrides
- **Utility Layer**: Helper classes and responsive utilities

### 2. **Import Order**
The CSS files are imported in a specific order to ensure proper cascade:

```css
/* Variables first */
@import './components/styles/variables.css';

/* Foundation styles */
@import './styles/foundation/reset.css';
@import './styles/foundation/typography.css';
@import './styles/foundation/layout.css';

/* Component styles */
@import './styles/components/antd-overrides.css';
@import './styles/components/ui-components.css';

/* Utilities last */
@import './styles/utilities/focus.css';
@import './styles/utilities/scrollbars.css';
@import './styles/utilities/loading.css';
@import './styles/utilities/responsive.css';
```

### 3. **Design System Integration**
All styles use CSS custom properties (variables) defined in `variables.css`:
- Colors: `var(--color-primary)`, `var(--text-primary)`, etc.
- Spacing: `var(--space-1)` through `var(--space-16)`
- Typography: `var(--text-xs)` through `var(--text-4xl)`
- Borders: `var(--radius-sm)` through `var(--radius-full)`

## 📋 File Descriptions

### Foundation Files

#### `reset.css`
- Global CSS reset for cross-browser consistency
- Base HTML element styling
- Print styles
- Accessibility defaults

#### `typography.css`
- Heading styles (h1-h6)
- Text utilities (colors, weights, sizes)
- Code and pre-formatted text
- Responsive typography
- Selection styles

#### `layout.css`
- Flexbox and Grid utilities
- Container classes
- Spacing utilities (margin, padding)
- Position and display utilities
- Width and height utilities

### Component Files

#### `antd-overrides.css`
- Customizations for Ant Design components
- Consistent styling with design system
- Button, Input, Card, Table, Modal overrides
- Form component styling

#### `ui-components.css`
- Custom UI component styles
- Page layout components
- Card components
- Button variants
- Form components
- Navigation components
- Loading components

### Utility Files

#### `focus.css`
- Focus states and accessibility
- Skip links for keyboard navigation
- Screen reader utilities
- High contrast mode support
- Reduced motion preferences

#### `scrollbars.css`
- Custom scrollbar styling
- Webkit and Firefox scrollbars
- Scrollbar variants (thin, wide, hidden)
- Scroll behavior utilities
- Scroll snap and padding utilities

#### `loading.css`
- Loading animations and keyframes
- Spinner components
- Skeleton screens
- Progress indicators
- Loading overlays and states

#### `responsive.css`
- Breakpoint-specific styles
- Responsive containers
- Mobile-first utilities
- Component responsive behavior
- Print styles

## 🎯 Usage Guidelines

### 1. **Using Utility Classes**

```html
<!-- Layout utilities -->
<div class="flex items-center justify-between gap-4">
  <h1 class="text-2xl font-bold text-primary">Title</h1>
  <button class="ui-button ui-button-primary">Action</button>
</div>

<!-- Responsive utilities -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <div class="ui-card hover-lift">Card content</div>
</div>
```

### 2. **Component Styling**

```tsx
// Use standardized components
import { PageLayout, PageSection } from '../components/ui/PageLayout';
import { Card } from '../components/ui/Card';

// Components automatically use design system styles
<PageLayout title="Page Title" maxWidth="lg">
  <PageSection background="card" padding="lg">
    <Card variant="elevated" hover>
      Content
    </Card>
  </PageSection>
</PageLayout>
```

### 3. **Custom Styling**

```css
/* Use design system variables */
.custom-component {
  background: var(--bg-primary);
  border: 1px solid var(--border-primary);
  border-radius: var(--radius-md);
  padding: var(--space-4);
  color: var(--text-primary);
  transition: all var(--transition-fast);
}

.custom-component:hover {
  box-shadow: var(--shadow-lg);
  transform: translateY(-2px);
}
```

## 📱 Responsive Design

### Breakpoints
- **xs**: 480px and down (mobile)
- **sm**: 640px and up (large mobile)
- **md**: 768px and up (tablet)
- **lg**: 1024px and up (desktop)
- **xl**: 1280px and up (large desktop)
- **2xl**: 1536px and up (extra large)

### Mobile-First Approach
All styles are mobile-first, with larger screens adding enhancements:

```css
/* Mobile first */
.component {
  padding: var(--space-4);
}

/* Tablet and up */
@media (min-width: 768px) {
  .component {
    padding: var(--space-6);
  }
}

/* Desktop and up */
@media (min-width: 1024px) {
  .component {
    padding: var(--space-8);
  }
}
```

## ♿ Accessibility

### Focus Management
- Consistent focus indicators
- Skip links for keyboard navigation
- Screen reader utilities
- High contrast mode support

### Reduced Motion
- Respects `prefers-reduced-motion`
- Disables animations when requested
- Provides static alternatives

### Color Contrast
- WCAG AA compliant color combinations
- High contrast mode support
- Semantic color usage

## 🔧 Maintenance

### Adding New Styles
1. **Foundation styles**: Add to appropriate foundation file
2. **Component styles**: Add to component-specific file
3. **Utilities**: Add to appropriate utility file
4. **Always use design system variables**

### Refactoring Legacy Styles
1. Identify duplicate or inconsistent styles
2. Move to appropriate organized file
3. Use design system variables
4. Test across all breakpoints
5. Update documentation

### Performance Considerations
- CSS is organized for optimal loading
- Utilities are grouped logically
- Critical styles load first
- Non-critical styles can be lazy-loaded

## 🚀 Benefits of This Architecture

1. **Maintainability**: Easy to find and modify styles
2. **Consistency**: Design system ensures visual coherence
3. **Performance**: Organized loading and minimal duplication
4. **Scalability**: Easy to add new components and utilities
5. **Developer Experience**: Clear structure and documentation
6. **Accessibility**: Built-in accessibility features
7. **Responsive**: Mobile-first, comprehensive breakpoint coverage

## 📚 Related Documentation

- [Design System Variables](../components/styles/variables.css)
- [Component Documentation](../components/ui/README.md)
- [Accessibility Guidelines](./utilities/focus.css)
- [Responsive Design Patterns](./utilities/responsive.css)
