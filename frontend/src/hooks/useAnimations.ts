/**
 * Animation Hooks
 * Custom hooks for managing animations and micro-interactions
 */

import { useEffect, useRef, useState, useCallback } from 'react';

// Animation types
export type AnimationType = 
  | 'fade' 
  | 'slide' 
  | 'slideUp' 
  | 'scale' 
  | 'rotate' 
  | 'blur' 
  | 'elastic' 
  | 'flip';

export type MicroInteractionType = 
  | 'bounce' 
  | 'shake' 
  | 'pulse' 
  | 'glow' 
  | 'ripple';

// Hook for page transitions
export const usePageTransition = (animationType: AnimationType = 'fade') => {
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [currentAnimation, setCurrentAnimation] = useState(animationType);

  const startTransition = useCallback((newAnimation?: AnimationType) => {
    if (newAnimation) {
      setCurrentAnimation(newAnimation);
    }
    setIsTransitioning(true);
  }, []);

  const endTransition = useCallback(() => {
    setIsTransitioning(false);
  }, []);

  const getTransitionClasses = useCallback(() => {
    const baseClass = `page-${currentAnimation}`;
    return {
      enter: `${baseClass}-enter`,
      enterActive: `${baseClass}-enter-active`,
      exit: `${baseClass}-exit`,
      exitActive: `${baseClass}-exit-active`,
    };
  }, [currentAnimation]);

  return {
    isTransitioning,
    currentAnimation,
    startTransition,
    endTransition,
    getTransitionClasses,
  };
};

// Hook for scroll-based animations
export const useScrollAnimation = (threshold: number = 0.1) => {
  const [isVisible, setIsVisible] = useState(false);
  const elementRef = useRef<HTMLElement>(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          // Optionally disconnect after first trigger
          observer.disconnect();
        }
      },
      { threshold }
    );

    const currentElement = elementRef.current;
    if (currentElement) {
      observer.observe(currentElement);
    }

    return () => {
      if (currentElement) {
        observer.unobserve(currentElement);
      }
    };
  }, [threshold]);

  return { elementRef, isVisible };
};

// Hook for micro-interactions
export const useMicroInteraction = (type: MicroInteractionType) => {
  const [isActive, setIsActive] = useState(false);
  const elementRef = useRef<HTMLElement>(null);

  const trigger = useCallback(() => {
    setIsActive(true);
    
    // Auto-reset after animation
    const duration = type === 'pulse' ? 2000 : 600;
    setTimeout(() => setIsActive(false), duration);
  }, [type]);

  const getClassName = useCallback(() => {
    return isActive ? `${type}-micro` : '';
  }, [type, isActive]);

  return { elementRef, trigger, isActive, getClassName };
};

// Hook for staggered animations
export const useStaggeredAnimation = (itemCount: number, delay: number = 100) => {
  const [visibleItems, setVisibleItems] = useState<number[]>([]);
  const [isComplete, setIsComplete] = useState(false);

  const startStagger = useCallback(() => {
    setVisibleItems([]);
    setIsComplete(false);

    for (let i = 0; i < itemCount; i++) {
      setTimeout(() => {
        setVisibleItems(prev => [...prev, i]);
        
        if (i === itemCount - 1) {
          setIsComplete(true);
        }
      }, i * delay);
    }
  }, [itemCount, delay]);

  const isItemVisible = useCallback((index: number) => {
    return visibleItems.includes(index);
  }, [visibleItems]);

  return { startStagger, isItemVisible, isComplete, visibleItems };
};

// Hook for loading animations
export const useLoadingAnimation = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [progress, setProgress] = useState(0);

  const startLoading = useCallback(() => {
    setIsLoading(true);
    setProgress(0);
  }, []);

  const updateProgress = useCallback((newProgress: number) => {
    setProgress(Math.min(100, Math.max(0, newProgress)));
  }, []);

  const completeLoading = useCallback(() => {
    setProgress(100);
    setTimeout(() => {
      setIsLoading(false);
      setProgress(0);
    }, 300);
  }, []);

  return { isLoading, progress, startLoading, updateProgress, completeLoading };
};

// Hook for hover animations
export const useHoverAnimation = (animationClass: string = 'card-micro') => {
  const [isHovered, setIsHovered] = useState(false);
  const elementRef = useRef<HTMLElement>(null);

  const handleMouseEnter = useCallback(() => {
    setIsHovered(true);
  }, []);

  const handleMouseLeave = useCallback(() => {
    setIsHovered(false);
  }, []);

  const getProps = useCallback(() => ({
    ref: elementRef,
    onMouseEnter: handleMouseEnter,
    onMouseLeave: handleMouseLeave,
    className: isHovered ? animationClass : '',
  }), [isHovered, animationClass, handleMouseEnter, handleMouseLeave]);

  return { elementRef, isHovered, getProps };
};

// Hook for focus animations
export const useFocusAnimation = () => {
  const [isFocused, setIsFocused] = useState(false);
  const elementRef = useRef<HTMLElement>(null);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
  }, []);

  const handleBlur = useCallback(() => {
    setIsFocused(false);
  }, []);

  const getProps = useCallback(() => ({
    ref: elementRef,
    onFocus: handleFocus,
    onBlur: handleBlur,
    className: isFocused ? 'focus-micro' : '',
  }), [isFocused, handleFocus, handleBlur]);

  return { elementRef, isFocused, getProps };
};

// Hook for reduced motion preference
export const useReducedMotion = () => {
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    setPrefersReducedMotion(mediaQuery.matches);

    const handleChange = (e: MediaQueryListEvent) => {
      setPrefersReducedMotion(e.matches);
    };

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  return prefersReducedMotion;
};

// Hook for animation performance monitoring
export const useAnimationPerformance = () => {
  const [fps, setFps] = useState(60);
  const [isLowPerformance, setIsLowPerformance] = useState(false);
  const frameCount = useRef(0);
  const lastTime = useRef(performance.now());

  useEffect(() => {
    let animationId: number;

    const measureFPS = () => {
      frameCount.current++;
      const currentTime = performance.now();
      
      if (currentTime - lastTime.current >= 1000) {
        const currentFPS = Math.round((frameCount.current * 1000) / (currentTime - lastTime.current));
        setFps(currentFPS);
        setIsLowPerformance(currentFPS < 30);
        
        frameCount.current = 0;
        lastTime.current = currentTime;
      }
      
      animationId = requestAnimationFrame(measureFPS);
    };

    animationId = requestAnimationFrame(measureFPS);

    return () => {
      if (animationId) {
        cancelAnimationFrame(animationId);
      }
    };
  }, []);

  return { fps, isLowPerformance };
};

// Hook for animation queue management
export const useAnimationQueue = () => {
  const [queue, setQueue] = useState<Array<{ id: string; animation: () => void; delay?: number }>>([]);
  const [isProcessing, setIsProcessing] = useState(false);

  const addToQueue = useCallback((id: string, animation: () => void, delay: number = 0) => {
    setQueue(prev => [...prev, { id, animation, delay }]);
  }, []);

  const processQueue = useCallback(async () => {
    if (isProcessing || queue.length === 0) return;

    setIsProcessing(true);

    for (const item of queue) {
      if (item.delay > 0) {
        await new Promise(resolve => setTimeout(resolve, item.delay));
      }
      item.animation();
    }

    setQueue([]);
    setIsProcessing(false);
  }, [queue, isProcessing]);

  const clearQueue = useCallback(() => {
    setQueue([]);
    setIsProcessing(false);
  }, []);

  useEffect(() => {
    if (queue.length > 0 && !isProcessing) {
      processQueue();
    }
  }, [queue, isProcessing, processQueue]);

  return { addToQueue, clearQueue, queueLength: queue.length, isProcessing };
};
