/**
 * Core UI Components - Modern Reusable System
 * 
 * This is the new consolidated component system that replaces all scattered UI components.
 * All components follow modern React patterns with compound components, proper TypeScript,
 * and consistent design system integration.
 */

// Base Components
export { Button, IconButton, ButtonGroup } from './Button';
export { Card, CardHeader, CardContent, CardFooter } from './Card';
export { Input, Select, Checkbox, Radio, Switch, FormField } from './Form';
export { Container, Grid, Flex, Stack, Spacer, Divider } from './Layout';

// Navigation Components
export { Menu, Breadcrumb, Tabs, Steps, Pagination } from './Navigation';

// Data Display Components
export { Table, List, Tree, Tag, Badge, Avatar, Statistic } from './Data';

// Feedback Components
export { Alert, Notification, Progress, Skeleton, Spinner } from './Feedback';

// Modal & Overlay Components
export { Modal, Drawer, Popover, Tooltip, Dialog, ConfirmDialog } from './Modal';

// Chart & Visualization Components
export { Chart, ChartContainer, ChartLegend, ChartTooltip } from './Chart';

// Search & Filter Components
export { SearchBox, FilterPanel, SortControl } from './Search';

// Error Handling Components
export { ErrorBoundary, ErrorFallback } from './Error';

// Theme Components
export { ThemeProvider, ThemeToggle, DarkModeToggle } from './Theme';

// Layout Components
export { AppLayout, PageLayout, ModernPageLayout, ContentLayout, SidebarLayout } from './Layouts';
export { PageHeader, useBreadcrumbs } from './PageHeader';

// Performance Components
export { LazyComponent, VirtualList, Memoized, InView, PerformanceMonitor, BundleAnalyzer, LoadingFallback } from './Performance';

// Types - commented out until implemented
// export type * from './types';

// Design System Tokens
export { designTokens, spacing, colors, typography, shadows, animations } from './design-system';
