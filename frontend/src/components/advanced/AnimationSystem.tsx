/**
 * Advanced Animation System
 * 
 * Comprehensive animation system with micro-interactions, performance monitoring,
 * and customizable animation presets for enhanced user experience.
 */

import React, { useState, useEffect, useRef, useCallback } from 'react';
import { designTokens } from '../core/design-system';

// Types
export type AnimationType = 'fadeIn' | 'slideIn' | 'scaleIn' | 'bounceIn' | 'rotateIn' | 'flipIn';
export type AnimationDirection = 'up' | 'down' | 'left' | 'right';
export type AnimationTiming = 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out' | 'linear';

export interface AnimationConfig {
  type: AnimationType;
  duration?: number;
  delay?: number;
  direction?: AnimationDirection;
  timing?: AnimationTiming;
  repeat?: number | 'infinite';
  fillMode?: 'none' | 'forwards' | 'backwards' | 'both';
}

export interface MicroInteractionConfig {
  hover?: AnimationConfig;
  focus?: AnimationConfig;
  active?: AnimationConfig;
  disabled?: boolean;
}

// Animation Presets
export const animationPresets = {
  // Entrance animations
  fadeIn: {
    type: 'fadeIn' as AnimationType,
    duration: 300,
    timing: 'ease-out' as AnimationTiming,
  },
  slideInUp: {
    type: 'slideIn' as AnimationType,
    direction: 'up' as AnimationDirection,
    duration: 400,
    timing: 'ease-out' as AnimationTiming,
  },
  scaleIn: {
    type: 'scaleIn' as AnimationType,
    duration: 250,
    timing: 'ease-out' as AnimationTiming,
  },
  bounceIn: {
    type: 'bounceIn' as AnimationType,
    duration: 600,
    timing: 'ease-out' as AnimationTiming,
  },
  
  // Micro-interactions
  buttonHover: {
    hover: {
      type: 'scaleIn' as AnimationType,
      duration: 150,
      timing: 'ease-out' as AnimationTiming,
    },
  },
  cardHover: {
    hover: {
      type: 'slideIn' as AnimationType,
      direction: 'up' as AnimationDirection,
      duration: 200,
      timing: 'ease-out' as AnimationTiming,
    },
  },
  pulseOnFocus: {
    focus: {
      type: 'scaleIn' as AnimationType,
      duration: 300,
      repeat: 'infinite' as const,
      timing: 'ease-in-out' as AnimationTiming,
    },
  },
};

// Animation Component
export const AnimatedComponent: React.FC<{
  children: React.ReactNode;
  animation?: AnimationConfig;
  microInteractions?: MicroInteractionConfig;
  trigger?: 'mount' | 'hover' | 'focus' | 'click' | 'inView';
  className?: string;
  style?: React.CSSProperties;
}> = ({
  children,
  animation,
  microInteractions,
  trigger = 'mount',
  className,
  style,
}) => {
  const [isAnimating, setIsAnimating] = useState(false);
  const [isHovered, setIsHovered] = useState(false);
  const [isFocused, setIsFocused] = useState(false);
  const [isActive, setIsActive] = useState(false);
  const elementRef = useRef<HTMLDivElement>(null);

  const getAnimationCSS = useCallback((config: AnimationConfig) => {
    const { type, duration = 300, delay = 0, direction, timing = 'ease', repeat = 1, fillMode = 'both' } = config;
    
    let keyframes = '';
    let transform = '';
    
    switch (type) {
      case 'fadeIn':
        keyframes = `
          @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
          }
        `;
        break;
      case 'slideIn':
        const translateValue = direction === 'up' ? 'translateY(20px)' :
                              direction === 'down' ? 'translateY(-20px)' :
                              direction === 'left' ? 'translateX(20px)' :
                              'translateX(-20px)';
        keyframes = `
          @keyframes slideIn {
            from { transform: ${translateValue}; opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
          }
        `;
        break;
      case 'scaleIn':
        keyframes = `
          @keyframes scaleIn {
            from { transform: scale(0.8); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
          }
        `;
        break;
      case 'bounceIn':
        keyframes = `
          @keyframes bounceIn {
            0% { transform: scale(0.3); opacity: 0; }
            50% { transform: scale(1.05); }
            70% { transform: scale(0.9); }
            100% { transform: scale(1); opacity: 1; }
          }
        `;
        break;
      case 'rotateIn':
        keyframes = `
          @keyframes rotateIn {
            from { transform: rotate(-180deg) scale(0.8); opacity: 0; }
            to { transform: rotate(0deg) scale(1); opacity: 1; }
          }
        `;
        break;
      case 'flipIn':
        keyframes = `
          @keyframes flipIn {
            from { transform: perspective(400px) rotateY(-90deg); opacity: 0; }
            to { transform: perspective(400px) rotateY(0deg); opacity: 1; }
          }
        `;
        break;
    }

    return {
      keyframes,
      animation: `${type} ${duration}ms ${timing} ${delay}ms ${repeat} ${fillMode}`,
    };
  }, []);

  // Trigger animation based on trigger type
  useEffect(() => {
    if (trigger === 'mount' && animation) {
      setIsAnimating(true);
    }
  }, [trigger, animation]);

  // Intersection Observer for 'inView' trigger
  useEffect(() => {
    if (trigger === 'inView' && animation && elementRef.current) {
      const observer = new IntersectionObserver(
        ([entry]) => {
          if (entry.isIntersecting) {
            setIsAnimating(true);
          }
        },
        { threshold: 0.1 }
      );

      observer.observe(elementRef.current);
      return () => observer.disconnect();
    }
  }, [trigger, animation]);

  const getCurrentAnimation = () => {
    if (microInteractions?.disabled) return null;
    
    if (isActive && microInteractions?.active) return microInteractions.active;
    if (isFocused && microInteractions?.focus) return microInteractions.focus;
    if (isHovered && microInteractions?.hover) return microInteractions.hover;
    if (isAnimating && animation) return animation;
    
    return null;
  };

  const currentAnimation = getCurrentAnimation();
  const animationCSS = currentAnimation ? getAnimationCSS(currentAnimation) : null;

  const handleMouseEnter = () => {
    setIsHovered(true);
    if (trigger === 'hover' && animation) {
      setIsAnimating(true);
    }
  };

  const handleMouseLeave = () => {
    setIsHovered(false);
  };

  const handleFocus = () => {
    setIsFocused(true);
    if (trigger === 'focus' && animation) {
      setIsAnimating(true);
    }
  };

  const handleBlur = () => {
    setIsFocused(false);
  };

  const handleMouseDown = () => {
    setIsActive(true);
  };

  const handleMouseUp = () => {
    setIsActive(false);
  };

  const handleClick = () => {
    if (trigger === 'click' && animation) {
      setIsAnimating(true);
    }
  };

  return (
    <>
      {animationCSS && (
        <style>
          {animationCSS.keyframes}
        </style>
      )}
      <div
        ref={elementRef}
        className={className}
        style={{
          ...(animationCSS && { animation: animationCSS.animation }),
          transition: 'all 0.2s ease',
          ...style,
        }}
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
        onFocus={handleFocus}
        onBlur={handleBlur}
        onMouseDown={handleMouseDown}
        onMouseUp={handleMouseUp}
        onClick={handleClick}
        tabIndex={microInteractions?.focus ? 0 : undefined}
      >
        {children}
      </div>
    </>
  );
};

// Performance Monitor for Animations
export const AnimationPerformanceMonitor: React.FC<{
  onMetrics?: (metrics: {
    fps: number;
    frameDrops: number;
    averageFrameTime: number;
  }) => void;
  enabled?: boolean;
}> = ({ onMetrics, enabled = process.env.NODE_ENV === 'development' }) => {
  const frameTimesRef = useRef<number[]>([]);
  const lastFrameTimeRef = useRef<number>(0);
  const frameCountRef = useRef<number>(0);

  useEffect(() => {
    if (!enabled || !onMetrics) return;

    let animationId: number;
    
    const measureFrame = (currentTime: number) => {
      if (lastFrameTimeRef.current > 0) {
        const frameTime = currentTime - lastFrameTimeRef.current;
        frameTimesRef.current.push(frameTime);
        
        // Keep only last 60 frames
        if (frameTimesRef.current.length > 60) {
          frameTimesRef.current.shift();
        }
        
        frameCountRef.current++;
        
        // Report metrics every 60 frames
        if (frameCountRef.current % 60 === 0) {
          const frameTimes = frameTimesRef.current;
          const averageFrameTime = frameTimes.reduce((a, b) => a + b, 0) / frameTimes.length;
          const fps = 1000 / averageFrameTime;
          const frameDrops = frameTimes.filter(time => time > 16.67).length; // 60fps = 16.67ms per frame
          
          onMetrics({
            fps: Math.round(fps),
            frameDrops,
            averageFrameTime: Math.round(averageFrameTime * 100) / 100,
          });
        }
      }
      
      lastFrameTimeRef.current = currentTime;
      animationId = requestAnimationFrame(measureFrame);
    };

    animationId = requestAnimationFrame(measureFrame);
    
    return () => {
      if (animationId) {
        cancelAnimationFrame(animationId);
      }
    };
  }, [enabled, onMetrics]);

  return null;
};

// Animation Presets Hook
export const useAnimationPresets = () => {
  return {
    presets: animationPresets,
    createCustomPreset: (name: string, config: AnimationConfig | MicroInteractionConfig) => {
      // In a real implementation, this would save to a store or context
      console.log(`Custom preset "${name}" created:`, config);
    },
  };
};
