/**
 * Chart Animation Hooks
 * Advanced hooks for managing chart animations and data visualization effects
 */

import { useEffect, useRef, useState, useCallback, useMemo } from 'react';
import { useReducedMotion } from './useAnimations';

export type ChartType = 'bar' | 'line' | 'pie' | 'area' | 'scatter' | 'combo';
export type AnimationPreset = 'smooth' | 'bouncy' | 'fast' | 'elegant' | 'minimal' | 'dramatic';
export type AnimationTrigger = 'onMount' | 'onDataChange' | 'onHover' | 'onFocus';

interface ChartAnimationConfig {
  type: ChartType;
  preset: AnimationPreset;
  duration: number;
  delay: number;
  stagger: boolean;
  staggerDelay: number;
  easing: string;
  triggers: AnimationTrigger[];
}

interface AnimationMetrics {
  fps: number;
  duration: number;
  frameCount: number;
  frameTime: number; // Added missing frameTime property
  animationCount: number; // Added missing animationCount property
  isLowPerformance: boolean;
}

// Default animation configurations for different chart types
const DEFAULT_CONFIGS: Record<ChartType, ChartAnimationConfig> = {
  bar: {
    type: 'bar',
    preset: 'smooth',
    duration: 1000,
    delay: 0,
    stagger: true,
    staggerDelay: 100,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    triggers: ['onMount', 'onDataChange']
  },
  line: {
    type: 'line',
    preset: 'elegant',
    duration: 2000,
    delay: 0,
    stagger: false,
    staggerDelay: 0,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    triggers: ['onMount', 'onDataChange']
  },
  pie: {
    type: 'pie',
    preset: 'bouncy',
    duration: 1000,
    delay: 0,
    stagger: true,
    staggerDelay: 100,
    easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
    triggers: ['onMount', 'onDataChange']
  },
  area: {
    type: 'area',
    preset: 'smooth',
    duration: 1500,
    delay: 0,
    stagger: false,
    staggerDelay: 0,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    triggers: ['onMount', 'onDataChange']
  },
  scatter: {
    type: 'scatter',
    preset: 'dramatic',
    duration: 800,
    delay: 0,
    stagger: true,
    staggerDelay: 50,
    easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
    triggers: ['onMount', 'onDataChange']
  },
  combo: {
    type: 'combo',
    preset: 'elegant',
    duration: 1200,
    delay: 0,
    stagger: true,
    staggerDelay: 150,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    triggers: ['onMount', 'onDataChange']
  }
};

// Animation preset configurations
const ANIMATION_PRESETS: Record<AnimationPreset, Partial<ChartAnimationConfig>> = {
  smooth: {
    duration: 1000,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    staggerDelay: 100
  },
  bouncy: {
    duration: 800,
    easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
    staggerDelay: 80
  },
  fast: {
    duration: 400,
    easing: 'cubic-bezier(0.25, 0.46, 0.45, 0.94)',
    staggerDelay: 50
  },
  elegant: {
    duration: 1500,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    staggerDelay: 150
  },
  minimal: {
    duration: 300,
    easing: 'ease',
    staggerDelay: 30
  },
  dramatic: {
    duration: 2000,
    easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
    staggerDelay: 200
  }
};

// Main chart animation hook
export const useChartAnimation = (
  chartType: ChartType,
  data: any[] = [],
  customConfig?: Partial<ChartAnimationConfig>
) => {
  const prefersReducedMotion = useReducedMotion();
  const [isAnimating, setIsAnimating] = useState(false);
  const [animationPhase, setAnimationPhase] = useState<'idle' | 'entering' | 'updating' | 'exiting'>('idle');
  const [metrics, setMetrics] = useState<AnimationMetrics>({
    fps: 60,
    duration: 0,
    frameCount: 0,
    frameTime: 16.67, // 60 FPS = 16.67ms per frame
    animationCount: 0,
    isLowPerformance: false
  });

  const chartRef = useRef<HTMLDivElement>(null);
  const animationStartTime = useRef<number>(0);
  const frameCount = useRef<number>(0);
  const animationId = useRef<number>(0);

  // Merge default config with custom config
  const config = useMemo(() => {
    const defaultConfig = DEFAULT_CONFIGS[chartType];
    const presetConfig = ANIMATION_PRESETS[customConfig?.preset || defaultConfig.preset];
    
    return {
      ...defaultConfig,
      ...presetConfig,
      ...customConfig
    };
  }, [chartType, customConfig]);

  // Performance monitoring
  const measurePerformance = useCallback(() => {
    if (!isAnimating) return;

    frameCount.current++;
    const currentTime = performance.now();
    const elapsed = currentTime - animationStartTime.current;

    if (elapsed >= 1000) {
      const fps = Math.round((frameCount.current * 1000) / elapsed);
      setMetrics(prev => ({
        ...prev,
        fps,
        isLowPerformance: fps < 30,
        frameCount: frameCount.current,
        duration: elapsed
      }));

      frameCount.current = 0;
      animationStartTime.current = currentTime;
    }

    if (isAnimating) {
      animationId.current = requestAnimationFrame(measurePerformance);
    }
  }, [isAnimating]);

  // Start animation
  const startAnimation = useCallback((phase: 'entering' | 'updating' | 'exiting' = 'entering') => {
    if (prefersReducedMotion) return;

    setIsAnimating(true);
    setAnimationPhase(phase);
    animationStartTime.current = performance.now();
    frameCount.current = 0;
    
    // Start performance monitoring
    animationId.current = requestAnimationFrame(measurePerformance);

    // Auto-stop animation after duration
    setTimeout(() => {
      setIsAnimating(false);
      setAnimationPhase('idle');
      if (animationId.current) {
        cancelAnimationFrame(animationId.current);
      }
    }, config.duration + (config.stagger ? config.staggerDelay * 10 : 0));
  }, [config, prefersReducedMotion, measurePerformance]);

  // Stop animation
  const stopAnimation = useCallback(() => {
    setIsAnimating(false);
    setAnimationPhase('idle');
    if (animationId.current) {
      cancelAnimationFrame(animationId.current);
    }
  }, []);

  // Get animation classes
  const getAnimationClasses = useCallback(() => {
    if (prefersReducedMotion || !isAnimating) return '';

    const baseClass = `${chartType}-chart-${animationPhase}`;
    const presetClass = `chart-preset-${config.preset}`;
    const axisClass = animationPhase === 'entering' ? 'chart-axis-enter' : '';
    const gridClass = animationPhase === 'entering' ? 'chart-grid-enter' : '';
    const legendClass = animationPhase === 'entering' ? 'chart-legend-enter' : '';

    return [baseClass, presetClass, axisClass, gridClass, legendClass]
      .filter(Boolean)
      .join(' ');
  }, [chartType, animationPhase, config.preset, isAnimating, prefersReducedMotion]);

  // Get animation styles
  const getAnimationStyles = useCallback(() => {
    if (prefersReducedMotion) return {};

    return {
      '--animation-duration': `${config.duration}ms`,
      '--animation-delay': `${config.delay}ms`,
      '--stagger-delay': `${config.staggerDelay}ms`,
      '--animation-easing': config.easing,
    } as React.CSSProperties;
  }, [config, prefersReducedMotion]);

  // Handle data changes
  useEffect(() => {
    if (config.triggers.includes('onDataChange') && data.length > 0) {
      startAnimation('updating');
    }
  }, [data, config.triggers, startAnimation]);

  // Handle mount
  useEffect(() => {
    if (config.triggers.includes('onMount')) {
      startAnimation('entering');
    }

    return () => {
      if (animationId.current) {
        cancelAnimationFrame(animationId.current);
      }
    };
  }, [config.triggers, startAnimation]);

  return {
    chartRef,
    isAnimating,
    animationPhase,
    metrics,
    config,
    startAnimation,
    stopAnimation,
    getAnimationClasses,
    getAnimationStyles,
    prefersReducedMotion
  };
};

// Hook for chart hover animations
export const useChartHover = () => {
  const [hoveredElement, setHoveredElement] = useState<string | null>(null);
  const [hoverPosition, setHoverPosition] = useState<{ x: number; y: number } | null>(null);

  const handleMouseEnter = useCallback((elementId: string, event: React.MouseEvent) => {
    setHoveredElement(elementId);
    setHoverPosition({ x: event.clientX, y: event.clientY });
  }, []);

  const handleMouseLeave = useCallback(() => {
    setHoveredElement(null);
    setHoverPosition(null);
  }, []);

  const handleMouseMove = useCallback((event: React.MouseEvent) => {
    if (hoveredElement) {
      setHoverPosition({ x: event.clientX, y: event.clientY });
    }
  }, [hoveredElement]);

  const getHoverProps = useCallback((elementId: string) => ({
    onMouseEnter: (e: React.MouseEvent) => handleMouseEnter(elementId, e),
    onMouseLeave: handleMouseLeave,
    onMouseMove: handleMouseMove,
    className: hoveredElement === elementId ? 'chart-element-hover' : ''
  }), [hoveredElement, handleMouseEnter, handleMouseLeave, handleMouseMove]);

  return {
    hoveredElement,
    hoverPosition,
    getHoverProps,
    isHovered: (elementId: string) => hoveredElement === elementId
  };
};

// Hook for chart loading animations
export const useChartLoading = (isLoading: boolean) => {
  const [loadingPhase, setLoadingPhase] = useState<'idle' | 'loading' | 'complete'>('idle');

  useEffect(() => {
    if (isLoading) {
      setLoadingPhase('loading');
    } else {
      setLoadingPhase('complete');
      // Reset to idle after animation completes
      setTimeout(() => setLoadingPhase('idle'), 300);
    }
  }, [isLoading]);

  const getLoadingClasses = useCallback(() => {
    switch (loadingPhase) {
      case 'loading': return 'chart-loading';
      case 'complete': return 'chart-loading-complete';
      default: return '';
    }
  }, [loadingPhase]);

  return {
    loadingPhase,
    getLoadingClasses,
    isLoading: loadingPhase === 'loading'
  };
};

// Hook for chart performance optimization
export const useChartPerformance = (dataLength: number, threshold: number = 1000) => {
  const [performanceMode, setPerformanceMode] = useState<'normal' | 'optimized'>('normal');
  const [shouldReduceAnimations, setShouldReduceAnimations] = useState(false);

  useEffect(() => {
    const isLargeDataset = dataLength > threshold;
    setPerformanceMode(isLargeDataset ? 'optimized' : 'normal');
    setShouldReduceAnimations(isLargeDataset);
  }, [dataLength, threshold]);

  const getPerformanceClasses = useCallback(() => {
    return performanceMode === 'optimized' ? 'chart-performance-mode' : '';
  }, [performanceMode]);

  const getOptimizedConfig = useCallback((config: ChartAnimationConfig): ChartAnimationConfig => {
    if (performanceMode === 'optimized') {
      return {
        ...config,
        duration: Math.min(config.duration, 500),
        stagger: false,
        staggerDelay: 0
      };
    }
    return config;
  }, [performanceMode]);

  return {
    performanceMode,
    shouldReduceAnimations,
    getPerformanceClasses,
    getOptimizedConfig,
    isLargeDataset: dataLength > threshold
  };
};

// Export animation presets for external use
export { ANIMATION_PRESETS };
