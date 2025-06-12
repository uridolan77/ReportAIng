# Enhanced PageHeader Component

The Enhanced PageHeader component provides a comprehensive, customizable header solution with advanced features for modern web applications. It follows the user's preferences for titles and subtitles on background (not in white panels) while offering extensive customization options.

## ✨ Key Features

### 🎨 Multiple Variants
- **Default**: Clean, minimal styling with transparent background
- **Gradient**: Beautiful gradient backgrounds with enhanced visual appeal
- **Minimal**: Ultra-clean design with optional dividers
- **Elevated**: Card-like appearance with shadows and borders
- **Glassmorphism**: Modern glass effect with backdrop blur

### 📱 Responsive Design
- Mobile-first approach with adaptive layouts
- Automatic stacking of actions on mobile devices
- Collapsible titles and responsive typography
- Smart breakpoint handling

### ⚡ Enhanced Actions
- Pre-built action button patterns (refresh, save, export, etc.)
- Tooltip support for better UX
- Loading states and disabled states
- Flexible button grouping and alignment

### 🧭 Smart Breadcrumbs
- Utility functions for common breadcrumb patterns
- Icon support with customizable breadcrumb items
- Automatic navigation handling
- Admin and standard breadcrumb trails

### 🎭 Advanced Styling
- Animation support with fade-in effects
- Shadow customization (small, medium, large)
- Background gradient options
- Blur effects for glassmorphism
- Size variants (small, medium, large)

## 📖 Basic Usage

```tsx
import { PageHeader } from './components/core/PageHeader';

// Simple header
<PageHeader
  title="Dashboard"
  subtitle="Overview of your analytics"
/>

// With breadcrumbs and actions
<PageHeader
  title="User Management"
  subtitle="Manage users and permissions"
  breadcrumbItems={[
    { title: 'Home', path: '/', icon: '🏠' },
    { title: 'Admin', path: '/admin', icon: '⚙️' },
    { title: 'Users', icon: '👥' }
  ]}
  actions={[
    {
      key: 'add',
      label: 'Add User',
      icon: '➕',
      onClick: () => handleAddUser(),
      type: 'primary'
    }
  ]}
/>
```

## 🎯 Variant Examples

### Dashboard Style (Gradient)
```tsx
<PageHeader
  variant="gradient"
  size="large"
  title="Analytics Dashboard"
  subtitle="Real-time insights and metrics"
  backgroundGradient={true}
  shadow="medium"
  animated={true}
/>
```

### Admin Style (Elevated)
```tsx
<PageHeader
  variant="elevated"
  title="System Settings"
  subtitle="Configure system parameters"
  shadow="large"
  showDivider={true}
/>
```

### Modern Glass Effect
```tsx
<PageHeader
  variant="glassmorphism"
  title="Modern Interface"
  subtitle="Beautiful glassmorphism design"
  blur={true}
  animated={true}
/>
```

### Minimal Clean Design
```tsx
<PageHeader
  variant="minimal"
  title="Simple Page"
  subtitle="Clean and minimal"
  showDivider={true}
/>
```

## 🛠️ Utility Functions

### Action Creators
```tsx
import { createHeaderActions } from './components/core/PageHeader';

const actions = [
  createHeaderActions.refresh(() => handleRefresh(), loading),
  createHeaderActions.save(() => handleSave()),
  createHeaderActions.export(() => handleExport()),
  createHeaderActions.add(() => handleAdd(), 'Add Report'),
];
```

### Breadcrumb Helpers
```tsx
import { createEnhancedBreadcrumbs } from './components/core/PageHeader';

// Standard trail
const breadcrumbs = createEnhancedBreadcrumbs.trail(
  createEnhancedBreadcrumbs.dashboard(),
  createEnhancedBreadcrumbs.create('Reports', '/reports', '📊')
);

// Admin trail
const adminBreadcrumbs = createEnhancedBreadcrumbs.adminTrail(
  createEnhancedBreadcrumbs.create('Users', '/admin/users', '👥')
);
```

## 📋 Preset Configurations

Use pre-configured styles for common patterns:

```tsx
import { PageHeaderPresets } from './components/core/PageHeader';

// Dashboard preset
<PageHeader
  {...PageHeaderPresets.dashboard}
  title="Analytics"
  subtitle="Dashboard overview"
/>

// Admin preset
<PageHeader
  {...PageHeaderPresets.admin}
  title="User Management"
  subtitle="Manage system users"
/>

// Mobile preset
<PageHeader
  {...PageHeaderPresets.mobile}
  title="Mobile View"
  subtitle="Optimized for mobile"
/>
```

## 🎣 Responsive Hooks

### Mobile Detection
```tsx
import { usePageHeaderResponsive } from './components/core/PageHeader';

const { isMobile, mobileProps } = usePageHeaderResponsive();

<PageHeader
  title="Responsive Header"
  {...mobileProps}
/>
```

### Theme Support
```tsx
import { usePageHeaderTheme } from './components/core/PageHeader';

const themeProps = usePageHeaderTheme(darkMode);

<PageHeader
  title="Themed Header"
  {...themeProps}
/>
```

## 🎛️ Complete API Reference

### PageHeaderProps

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `title` | `ReactNode` | - | Main heading text |
| `subtitle` | `ReactNode` | - | Secondary description text |
| `breadcrumbItems` | `BreadcrumbItem[]` | - | Navigation breadcrumb items |
| `actions` | `ReactNode \| PageHeaderAction[]` | - | Action buttons or custom content |
| `variant` | `'default' \| 'gradient' \| 'minimal' \| 'elevated' \| 'glassmorphism'` | `'default'` | Visual style variant |
| `size` | `'small' \| 'medium' \| 'large'` | `'medium'` | Size of typography and spacing |
| `showDivider` | `boolean` | `false` | Show bottom divider line |
| `responsive` | `boolean` | `true` | Enable responsive behavior |
| `background` | `string` | - | Custom background color/gradient |
| `backgroundGradient` | `boolean \| string` | `false` | Enable gradient background |
| `shadow` | `boolean \| 'small' \| 'medium' \| 'large'` | `false` | Shadow intensity |
| `blur` | `boolean` | `false` | Enable backdrop blur effect |
| `align` | `'left' \| 'center' \| 'right'` | `'left'` | Text alignment |
| `actionsAlign` | `'start' \| 'center' \| 'end'` | `'end'` | Actions alignment |
| `direction` | `'horizontal' \| 'vertical'` | `'horizontal'` | Layout direction |
| `titleLevel` | `1 \| 2 \| 3 \| 4 \| 5 \| 6` | `1` | HTML heading level |
| `animated` | `boolean` | `false` | Enable fade-in animation |
| `mobileCollapse` | `boolean` | `true` | Collapse on mobile |
| `mobileStackActions` | `boolean` | `true` | Stack actions on mobile |

### PageHeaderAction

| Prop | Type | Description |
|------|------|-------------|
| `key` | `string` | Unique identifier |
| `label` | `ReactNode` | Button text |
| `icon` | `ReactNode` | Button icon |
| `onClick` | `() => void` | Click handler |
| `type` | `'primary' \| 'default' \| 'dashed' \| 'link' \| 'text'` | Button style |
| `size` | `'small' \| 'middle' \| 'large'` | Button size |
| `loading` | `boolean` | Loading state |
| `disabled` | `boolean` | Disabled state |
| `tooltip` | `string` | Tooltip text |
| `href` | `string` | Link URL |
| `target` | `string` | Link target |

## 🎨 Design System Integration

The PageHeader component fully integrates with the design system:

- Uses design tokens for consistent spacing, typography, and colors
- Follows accessibility best practices
- Supports dark mode themes
- Maintains consistent visual hierarchy
- Responsive breakpoints aligned with the grid system

## 🌟 Best Practices

1. **Use appropriate variants**: 
   - `gradient` for dashboards and hero sections
   - `elevated` for admin and settings pages
   - `minimal` for content pages
   - `glassmorphism` for modern, visual interfaces

2. **Keep actions focused**: Limit to 3-4 primary actions for better UX

3. **Use breadcrumbs consistently**: Always include Home and maintain logical hierarchy

4. **Consider mobile users**: Test responsive behavior and use mobile presets when needed

5. **Maintain accessibility**: Use proper heading levels and provide aria-labels

## 🚀 Migration from Old PageHeader

The enhanced PageHeader is backward compatible. Existing implementations will work with default styling, but you can gradually adopt new features:

```tsx
// Old way (still works)
<PageHeader title="My Page" />

// Enhanced way
<PageHeader
  title="My Page"
  variant="gradient"
  animated={true}
  actions={[createHeaderActions.refresh(() => {})]}
/>
```

This enhanced PageHeader component provides a comprehensive solution that grows with your application needs while maintaining consistency and excellent user experience across all devices and use cases.
