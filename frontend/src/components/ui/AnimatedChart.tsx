/**
 * Animated Chart Component
 * Enhanced chart component with advanced animations and performance monitoring
 */

import React, { useEffect, useState, useRef } from 'react';
import { Card, Button, Space, Tooltip, Alert } from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  ReloadOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  WarningOutlined
} from '@ant-design/icons';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  AreaChart,
  Area,
  ScatterChart,
  Scatter,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend
} from 'recharts';
import { useChartAnimation, useChartHover, useChartLoading, useChartPerformance } from '../../hooks/useChartAnimations';
import type { ChartType, AnimationPreset } from '../../hooks/useChartAnimations';

interface AnimatedChartProps {
  type: ChartType;
  data: any[];
  title?: string;
  subtitle?: string;
  animationPreset?: AnimationPreset;
  showPerformanceMetrics?: boolean;
  enableHoverEffects?: boolean;
  isLoading?: boolean;
  width?: number | string;
  height?: number | string;
  colors?: string[];
  onAnimationComplete?: () => void;
  onPerformanceWarning?: (metrics: any) => void;
}

const DEFAULT_COLORS = [
  'var(--color-primary)',
  'var(--color-secondary)',
  'var(--color-success)',
  'var(--color-warning)',
  'var(--color-error)',
  'var(--color-info)'
];

export const AnimatedChart: React.FC<AnimatedChartProps> = ({
  type,
  data,
  title,
  subtitle,
  animationPreset = 'smooth',
  showPerformanceMetrics = false,
  enableHoverEffects = true,
  isLoading = false,
  width = '100%',
  height = 400,
  colors = DEFAULT_COLORS,
  onAnimationComplete,
  onPerformanceWarning
}) => {
  const [isPlaying, setIsPlaying] = useState(false);
  const chartContainerRef = useRef<HTMLDivElement>(null);

  // Animation hooks
  const {
    chartRef,
    isAnimating,
    animationPhase,
    metrics,
    startAnimation,
    stopAnimation,
    getAnimationClasses,
    getAnimationStyles,
    prefersReducedMotion
  } = useChartAnimation(type, data, { preset: animationPreset });

  // Hover effects
  const { hoveredElement, getHoverProps } = useChartHover();

  // Loading animations
  const { getLoadingClasses } = useChartLoading(isLoading);

  // Performance monitoring
  const {
    performanceMode,
    shouldReduceAnimations,
    getPerformanceClasses,
    isLargeDataset
  } = useChartPerformance(data.length);

  // Handle animation completion
  useEffect(() => {
    if (!isAnimating && animationPhase === 'idle' && isPlaying) {
      setIsPlaying(false);
      onAnimationComplete?.();
    }
  }, [isAnimating, animationPhase, isPlaying, onAnimationComplete]);

  // Handle performance warnings
  useEffect(() => {
    if (metrics.isLowPerformance && onPerformanceWarning) {
      onPerformanceWarning(metrics);
    }
  }, [metrics.isLowPerformance, metrics, onPerformanceWarning]);

  // Play animation
  const handlePlay = () => {
    setIsPlaying(true);
    startAnimation('entering');
  };

  // Stop animation
  const handleStop = () => {
    setIsPlaying(false);
    stopAnimation();
  };

  // Restart animation
  const handleRestart = () => {
    stopAnimation();
    setTimeout(() => {
      setIsPlaying(true);
      startAnimation('entering');
    }, 100);
  };

  // Render chart based on type
  const renderChart = () => {
    const commonProps = {
      data,
      width: typeof width === 'string' ? undefined : width,
      height: typeof height === 'string' ? undefined : height
    };

    switch (type) {
      case 'bar':
        return (
          <BarChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <RechartsTooltip />
            <Legend />
            {data.length > 0 && Object.keys(data[0]).filter(key => key !== 'name').map((key, index) => (
              <Bar
                key={key}
                dataKey={key}
                fill={colors[index % colors.length]}
                {...(enableHoverEffects ? getHoverProps(`bar-${key}`) : {})}
              />
            ))}
          </BarChart>
        );

      case 'line':
        return (
          <LineChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <RechartsTooltip />
            <Legend />
            {data.length > 0 && Object.keys(data[0]).filter(key => key !== 'name').map((key, index) => (
              <Line
                key={key}
                type="monotone"
                dataKey={key}
                stroke={colors[index % colors.length]}
                strokeWidth={2}
                dot={enableHoverEffects}
                {...(enableHoverEffects ? getHoverProps(`line-${key}`) : {})}
              />
            ))}
          </LineChart>
        );

      case 'pie':
        const pieData = data.map((item, index) => ({
          ...item,
          fill: colors[index % colors.length]
        }));
        
        return (
          <PieChart {...commonProps}>
            <Pie
              data={pieData}
              cx="50%"
              cy="50%"
              outerRadius={Math.min(height as number, width as number) / 4}
              dataKey="value"
              {...(enableHoverEffects ? getHoverProps('pie') : {})}
            >
              {pieData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.fill} />
              ))}
            </Pie>
            <RechartsTooltip />
            <Legend />
          </PieChart>
        );

      case 'area':
        return (
          <AreaChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <RechartsTooltip />
            <Legend />
            {data.length > 0 && Object.keys(data[0]).filter(key => key !== 'name').map((key, index) => (
              <Area
                key={key}
                type="monotone"
                dataKey={key}
                stackId="1"
                stroke={colors[index % colors.length]}
                fill={colors[index % colors.length]}
                fillOpacity={0.6}
                {...(enableHoverEffects ? getHoverProps(`area-${key}`) : {})}
              />
            ))}
          </AreaChart>
        );

      case 'scatter':
        return (
          <ScatterChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="x" />
            <YAxis dataKey="y" />
            <RechartsTooltip />
            <Legend />
            <Scatter
              data={data}
              fill={colors[0]}
              {...(enableHoverEffects ? getHoverProps('scatter') : {})}
            />
          </ScatterChart>
        );

      default:
        return <div>Unsupported chart type: {type}</div>;
    }
  };

  const animationClasses = [
    getAnimationClasses(),
    getLoadingClasses(),
    getPerformanceClasses(),
    prefersReducedMotion ? 'reduced-motion' : ''
  ].filter(Boolean).join(' ');

  return (
    <Card
      title={title}
      subtitle={subtitle}
      extra={
        <Space>
          {showPerformanceMetrics && (
            <Tooltip title={`FPS: ${metrics.fps} | Performance: ${performanceMode}`}>
              <Button
                size="small"
                icon={<ThunderboltOutlined />}
                type={metrics.isLowPerformance ? 'primary' : 'default'}
                danger={metrics.isLowPerformance}
              >
                {metrics.fps}
              </Button>
            </Tooltip>
          )}
          <Button
            size="small"
            icon={isPlaying ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
            onClick={isPlaying ? handleStop : handlePlay}
            disabled={prefersReducedMotion}
          >
            {isPlaying ? 'Pause' : 'Play'}
          </Button>
          <Button
            size="small"
            icon={<ReloadOutlined />}
            onClick={handleRestart}
            disabled={prefersReducedMotion}
          />
          <Button size="small" icon={<SettingOutlined />} />
        </Space>
      }
    >
      {/* Performance Warnings */}
      {shouldReduceAnimations && (
        <Alert
          message="Large Dataset Detected"
          description="Animations have been optimized for better performance with large datasets."
          type="info"
          showIcon
          style={{ marginBottom: 'var(--space-4)' }}
        />
      )}

      {metrics.isLowPerformance && (
        <Alert
          message="Performance Warning"
          description={`Low FPS detected (${metrics.fps}). Consider reducing animation complexity.`}
          type="warning"
          showIcon
          icon={<WarningOutlined />}
          style={{ marginBottom: 'var(--space-4)' }}
        />
      )}

      {/* Chart Container */}
      <div
        ref={chartRef}
        className={`chart-container ${animationClasses}`}
        style={{
          width,
          height,
          position: 'relative',
          ...getAnimationStyles()
        }}
      >
        <ResponsiveContainer width="100%" height="100%">
          {renderChart()}
        </ResponsiveContainer>

        {/* Loading Overlay */}
        {isLoading && (
          <div
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              bottom: 0,
              background: 'rgba(255, 255, 255, 0.8)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              zIndex: 10
            }}
          >
            <div className="loading-dots-micro">
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
        )}
      </div>

      {/* Performance Metrics */}
      {showPerformanceMetrics && (
        <div style={{ 
          marginTop: 'var(--space-2)', 
          fontSize: 'var(--text-xs)', 
          color: 'var(--text-tertiary)',
          display: 'flex',
          justifyContent: 'space-between'
        }}>
          <span>FPS: {metrics.fps}</span>
          <span>Frame Time: {metrics.frameTime.toFixed(2)}ms</span>
          <span>Elements: {metrics.animationCount}</span>
          <span>Mode: {performanceMode}</span>
        </div>
      )}
    </Card>
  );
};
