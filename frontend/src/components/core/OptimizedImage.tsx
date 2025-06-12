/**
 * Optimized Image Component
 * 
 * High-performance image component with lazy loading, progressive enhancement,
 * and automatic format optimization.
 */

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useInView } from 'react-intersection-observer';

export interface OptimizedImageProps {
  src: string;
  alt: string;
  width?: number;
  height?: number;
  className?: string;
  style?: React.CSSProperties;
  placeholder?: string;
  blurDataURL?: string;
  priority?: boolean;
  quality?: number;
  sizes?: string;
  onLoad?: () => void;
  onError?: () => void;
  loading?: 'lazy' | 'eager';
  objectFit?: 'contain' | 'cover' | 'fill' | 'none' | 'scale-down';
}

export const OptimizedImage: React.FC<OptimizedImageProps> = ({
  src,
  alt,
  width,
  height,
  className,
  style,
  placeholder,
  blurDataURL,
  priority = false,
  quality = 75,
  sizes,
  onLoad,
  onError,
  loading = 'lazy',
  objectFit = 'cover',
}) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [hasError, setHasError] = useState(false);
  const [currentSrc, setCurrentSrc] = useState<string>('');
  const imgRef = useRef<HTMLImageElement>(null);

  const { ref, inView } = useInView({
    threshold: 0,
    triggerOnce: true,
    skip: priority, // Skip intersection observer for priority images
  });

  // Generate optimized image URLs based on device capabilities
  const generateOptimizedSrc = useCallback((originalSrc: string, targetWidth?: number) => {
    // In a real implementation, this would integrate with an image optimization service
    // like Cloudinary, ImageKit, or a custom solution
    
    // For now, return the original src with quality parameter if supported
    const url = new URL(originalSrc, window.location.origin);
    
    if (quality && quality < 100) {
      url.searchParams.set('quality', quality.toString());
    }
    
    if (targetWidth) {
      url.searchParams.set('w', targetWidth.toString());
    }
    
    // Add format optimization
    if (supportsWebP()) {
      url.searchParams.set('format', 'webp');
    } else if (supportsAVIF()) {
      url.searchParams.set('format', 'avif');
    }
    
    return url.toString();
  }, [quality]);

  // Check browser support for modern image formats
  const supportsWebP = useCallback(() => {
    const canvas = document.createElement('canvas');
    canvas.width = 1;
    canvas.height = 1;
    return canvas.toDataURL('image/webp').indexOf('data:image/webp') === 0;
  }, []);

  const supportsAVIF = useCallback(() => {
    const canvas = document.createElement('canvas');
    canvas.width = 1;
    canvas.height = 1;
    return canvas.toDataURL('image/avif').indexOf('data:image/avif') === 0;
  }, []);

  // Generate responsive image sources
  const generateSrcSet = useCallback((originalSrc: string) => {
    if (!width) return '';
    
    const breakpoints = [0.5, 1, 1.5, 2]; // Different density ratios
    return breakpoints
      .map(ratio => {
        const targetWidth = Math.round(width * ratio);
        const optimizedSrc = generateOptimizedSrc(originalSrc, targetWidth);
        return `${optimizedSrc} ${ratio}x`;
      })
      .join(', ');
  }, [width, generateOptimizedSrc]);

  // Load image when in view or priority
  useEffect(() => {
    if (priority || inView) {
      const optimizedSrc = generateOptimizedSrc(src, width);
      setCurrentSrc(optimizedSrc);
    }
  }, [inView, priority, src, width, generateOptimizedSrc]);

  // Handle image load
  const handleLoad = useCallback(() => {
    setIsLoaded(true);
    onLoad?.();
  }, [onLoad]);

  // Handle image error
  const handleError = useCallback(() => {
    setHasError(true);
    onError?.();
  }, [onError]);

  // Preload critical images
  useEffect(() => {
    if (priority && currentSrc) {
      const link = document.createElement('link');
      link.rel = 'preload';
      link.as = 'image';
      link.href = currentSrc;
      if (sizes) link.setAttribute('imagesizes', sizes);
      document.head.appendChild(link);

      return () => {
        document.head.removeChild(link);
      };
    }
  }, [priority, currentSrc, sizes]);

  // Placeholder styles
  const placeholderStyle: React.CSSProperties = {
    backgroundColor: '#f0f0f0',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    color: '#999',
    fontSize: '14px',
    width: width || '100%',
    height: height || 'auto',
    minHeight: height ? `${height}px` : '200px',
  };

  // Blur placeholder styles
  const blurPlaceholderStyle: React.CSSProperties = {
    backgroundImage: blurDataURL ? `url(${blurDataURL})` : undefined,
    backgroundSize: 'cover',
    backgroundPosition: 'center',
    filter: 'blur(10px)',
    transform: 'scale(1.1)', // Slightly scale to hide blur edges
  };

  // Image styles
  const imageStyle: React.CSSProperties = {
    ...style,
    width: width || '100%',
    height: height || 'auto',
    objectFit,
    transition: 'opacity 0.3s ease-in-out',
    opacity: isLoaded ? 1 : 0,
  };

  // Error fallback
  if (hasError) {
    return (
      <div
        className={className}
        style={{
          ...placeholderStyle,
          ...style,
          border: '1px dashed #ccc',
        }}
      >
        {placeholder || 'Failed to load image'}
      </div>
    );
  }

  // Loading state
  if (!currentSrc || (!isLoaded && !priority)) {
    return (
      <div
        ref={ref}
        className={className}
        style={{
          ...placeholderStyle,
          ...blurPlaceholderStyle,
          ...style,
        }}
      >
        {!blurDataURL && (placeholder || 'Loading...')}
      </div>
    );
  }

  return (
    <div
      ref={ref}
      className={className}
      style={{
        position: 'relative',
        width: width || '100%',
        height: height || 'auto',
        ...style,
      }}
    >
      {/* Blur placeholder background */}
      {blurDataURL && !isLoaded && (
        <div
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            ...blurPlaceholderStyle,
          }}
        />
      )}
      
      {/* Main image */}
      <img
        ref={imgRef}
        src={currentSrc}
        srcSet={generateSrcSet(src)}
        sizes={sizes}
        alt={alt}
        width={width}
        height={height}
        loading={loading}
        onLoad={handleLoad}
        onError={handleError}
        style={imageStyle}
        decoding="async"
      />
    </div>
  );
};

// Hook for image preloading
export const useImagePreloader = (imageSources: string[]) => {
  const [loadedImages, setLoadedImages] = useState<Set<string>>(new Set());
  const [failedImages, setFailedImages] = useState<Set<string>>(new Set());

  useEffect(() => {
    const preloadImage = (src: string): Promise<void> => {
      return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
          setLoadedImages(prev => new Set(prev).add(src));
          resolve();
        };
        img.onerror = () => {
          setFailedImages(prev => new Set(prev).add(src));
          reject();
        };
        img.src = src;
      });
    };

    // Preload all images
    imageSources.forEach(src => {
      if (!loadedImages.has(src) && !failedImages.has(src)) {
        preloadImage(src).catch(() => {
          // Error already handled in the promise
        });
      }
    });
  }, [imageSources, loadedImages, failedImages]);

  return {
    loadedImages,
    failedImages,
    isLoaded: (src: string) => loadedImages.has(src),
    hasFailed: (src: string) => failedImages.has(src),
  };
};

export default OptimizedImage;
