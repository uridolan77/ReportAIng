/**
 * Modern Theme Components
 * 
 * Provides comprehensive theme provider and theme toggle components.
 */

import React, { forwardRef } from 'react';
import { Switch } from 'antd';
// Removed unused import

// Re-export existing theme provider
export { ThemeProvider } from '../../contexts/ThemeContext';

// Theme Toggle Component
export const ThemeToggle = forwardRef<any, any>((props, ref) => (
  <Switch ref={ref} {...props} />
));

// Dark Mode Toggle Component
export const DarkModeToggle = forwardRef<any, any>((props, ref) => (
  <Switch ref={ref} checkedChildren="🌙" unCheckedChildren="☀️" {...props} />
));
