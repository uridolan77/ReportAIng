/**
 * Modern Error Handling Components
 * 
 * Provides comprehensive error boundary and error display components.
 */

import React, { Component, forwardRef } from 'react';
import { designTokens } from './design-system';

// Error Boundary Component
export class ErrorBoundary extends Component<
  { children: React.ReactNode; fallback?: React.ComponentType<{ error: Error }> },
  { hasError: boolean; error: Error | null }
> {
  constructor(props: any) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      const FallbackComponent = this.props.fallback || ErrorFallback;
      return <FallbackComponent error={this.state.error!} />;
    }

    return this.props.children;
  }
}

// Error Fallback Component
export const ErrorFallback = forwardRef<any, { error: Error }>((props, ref) => (
  <div ref={ref} style={{
    padding: designTokens.spacing.xl,
    textAlign: 'center',
    backgroundColor: designTokens.colors.dangerLight,
    borderRadius: designTokens.borderRadius.medium,
    border: `1px solid ${designTokens.colors.danger}`,
  }}>
    <h2 style={{ color: designTokens.colors.danger, marginBottom: designTokens.spacing.md }}>
      Something went wrong
    </h2>
    <p style={{ color: designTokens.colors.textSecondary, marginBottom: designTokens.spacing.md }}>
      {props.error.message}
    </p>
    <button
      onClick={() => window.location.reload()}
      style={{
        padding: `${designTokens.spacing.sm} ${designTokens.spacing.md}`,
        backgroundColor: designTokens.colors.danger,
        color: 'white',
        border: 'none',
        borderRadius: designTokens.borderRadius.medium,
        cursor: 'pointer',
      }}
    >
      Reload Page
    </button>
  </div>
));
