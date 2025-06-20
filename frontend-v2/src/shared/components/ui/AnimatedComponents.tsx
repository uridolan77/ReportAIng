/**
 * Animated Components
 * 
 * Provides polished UI animations and transitions for enhanced user experience.
 */

import React, { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Card, Typography, Progress, Spin } from 'antd'
import { 
  CheckCircleOutlined, 
  LoadingOutlined, 
  RocketOutlined,
  TrophyOutlined,
  StarOutlined
} from '@ant-design/icons'

const { Text } = Typography

// Fade in animation
export const FadeIn: React.FC<{
  children: React.ReactNode
  delay?: number
  duration?: number
  direction?: 'up' | 'down' | 'left' | 'right'
}> = ({ children, delay = 0, duration = 0.5, direction = 'up' }) => {
  const variants = {
    hidden: {
      opacity: 0,
      y: direction === 'up' ? 20 : direction === 'down' ? -20 : 0,
      x: direction === 'left' ? 20 : direction === 'right' ? -20 : 0
    },
    visible: {
      opacity: 1,
      y: 0,
      x: 0,
      transition: {
        duration,
        delay,
        ease: 'easeOut'
      }
    }
  }

  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={variants}
    >
      {children}
    </motion.div>
  )
}

// Slide in animation
export const SlideIn: React.FC<{
  children: React.ReactNode
  direction?: 'left' | 'right' | 'up' | 'down'
  delay?: number
}> = ({ children, direction = 'left', delay = 0 }) => {
  const variants = {
    hidden: {
      x: direction === 'left' ? -100 : direction === 'right' ? 100 : 0,
      y: direction === 'up' ? -100 : direction === 'down' ? 100 : 0,
      opacity: 0
    },
    visible: {
      x: 0,
      y: 0,
      opacity: 1,
      transition: {
        type: 'spring',
        stiffness: 100,
        damping: 15,
        delay
      }
    }
  }

  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={variants}
    >
      {children}
    </motion.div>
  )
}

// Scale animation
export const ScaleIn: React.FC<{
  children: React.ReactNode
  delay?: number
  scale?: number
}> = ({ children, delay = 0, scale = 0.8 }) => {
  return (
    <motion.div
      initial={{ scale, opacity: 0 }}
      animate={{ scale: 1, opacity: 1 }}
      transition={{
        type: 'spring',
        stiffness: 200,
        damping: 20,
        delay
      }}
    >
      {children}
    </motion.div>
  )
}

// Stagger children animation
export const StaggerContainer: React.FC<{
  children: React.ReactNode
  staggerDelay?: number
}> = ({ children, staggerDelay = 0.1 }) => {
  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={{
        hidden: { opacity: 0 },
        visible: {
          opacity: 1,
          transition: {
            staggerChildren: staggerDelay
          }
        }
      }}
    >
      {children}
    </motion.div>
  )
}

// Animated counter
export const AnimatedCounter: React.FC<{
  value: number
  duration?: number
  prefix?: string
  suffix?: string
  decimals?: number
}> = ({ value, duration = 1, prefix = '', suffix = '', decimals = 0 }) => {
  const [displayValue, setDisplayValue] = useState(0)

  useEffect(() => {
    let startTime: number
    let animationFrame: number

    const animate = (timestamp: number) => {
      if (!startTime) startTime = timestamp
      const progress = Math.min((timestamp - startTime) / (duration * 1000), 1)
      
      const easeOutQuart = 1 - Math.pow(1 - progress, 4)
      setDisplayValue(value * easeOutQuart)

      if (progress < 1) {
        animationFrame = requestAnimationFrame(animate)
      }
    }

    animationFrame = requestAnimationFrame(animate)

    return () => {
      if (animationFrame) {
        cancelAnimationFrame(animationFrame)
      }
    }
  }, [value, duration])

  return (
    <span>
      {prefix}{displayValue.toFixed(decimals)}{suffix}
    </span>
  )
}

// Animated progress bar
export const AnimatedProgress: React.FC<{
  percent: number
  showInfo?: boolean
  strokeColor?: string
  trailColor?: string
  strokeWidth?: number
  delay?: number
}> = ({ percent, showInfo = true, strokeColor, trailColor, strokeWidth, delay = 0 }) => {
  const [animatedPercent, setAnimatedPercent] = useState(0)

  useEffect(() => {
    const timer = setTimeout(() => {
      setAnimatedPercent(percent)
    }, delay)

    return () => clearTimeout(timer)
  }, [percent, delay])

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.3, delay: delay / 1000 }}
    >
      <Progress
        percent={animatedPercent}
        showInfo={showInfo}
        strokeColor={strokeColor}
        trailColor={trailColor}
        strokeWidth={strokeWidth}
      />
    </motion.div>
  )
}

// Floating action button
export const FloatingActionButton: React.FC<{
  icon: React.ReactNode
  onClick: () => void
  tooltip?: string
  position?: 'bottom-right' | 'bottom-left' | 'top-right' | 'top-left'
}> = ({ icon, onClick, tooltip, position = 'bottom-right' }) => {
  const positionStyles = {
    'bottom-right': { bottom: 24, right: 24 },
    'bottom-left': { bottom: 24, left: 24 },
    'top-right': { top: 24, right: 24 },
    'top-left': { top: 24, left: 24 }
  }

  return (
    <motion.div
      style={{
        position: 'fixed',
        ...positionStyles[position],
        zIndex: 1000
      }}
      initial={{ scale: 0, opacity: 0 }}
      animate={{ scale: 1, opacity: 1 }}
      whileHover={{ scale: 1.1 }}
      whileTap={{ scale: 0.9 }}
      transition={{ type: 'spring', stiffness: 200, damping: 15 }}
    >
      <motion.button
        onClick={onClick}
        style={{
          width: 56,
          height: 56,
          borderRadius: '50%',
          border: 'none',
          backgroundColor: '#1890ff',
          color: 'white',
          fontSize: 20,
          cursor: 'pointer',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}
        whileHover={{
          boxShadow: '0 6px 16px rgba(0, 0, 0, 0.2)'
        }}
        title={tooltip}
      >
        {icon}
      </motion.button>
    </motion.div>
  )
}

// Success animation
export const SuccessAnimation: React.FC<{
  visible: boolean
  message?: string
  onComplete?: () => void
}> = ({ visible, message = 'Success!', onComplete }) => {
  useEffect(() => {
    if (visible && onComplete) {
      const timer = setTimeout(onComplete, 2000)
      return () => clearTimeout(timer)
    }
  }, [visible, onComplete])

  return (
    <AnimatePresence>
      {visible && (
        <motion.div
          initial={{ scale: 0, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0, opacity: 0 }}
          transition={{ type: 'spring', stiffness: 200, damping: 15 }}
          style={{
            position: 'fixed',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            zIndex: 2000,
            backgroundColor: 'white',
            padding: 24,
            borderRadius: 8,
            boxShadow: '0 8px 24px rgba(0, 0, 0, 0.15)',
            textAlign: 'center'
          }}
        >
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ delay: 0.2, type: 'spring', stiffness: 200 }}
          >
            <CheckCircleOutlined 
              style={{ 
                fontSize: 48, 
                color: '#52c41a',
                marginBottom: 16
              }} 
            />
          </motion.div>
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.4 }}
          >
            <Text strong style={{ fontSize: 16 }}>
              {message}
            </Text>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  )
}

// Loading animation with stages
export const StageLoadingAnimation: React.FC<{
  stages: string[]
  currentStage: number
  progress?: number
}> = ({ stages, currentStage, progress }) => {
  return (
    <Card style={{ textAlign: 'center', padding: 24 }}>
      <motion.div
        initial={{ scale: 0.8, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ duration: 0.5 }}
      >
        <Spin 
          indicator={<LoadingOutlined style={{ fontSize: 48 }} spin />}
          style={{ marginBottom: 24 }}
        />
        
        <motion.div
          key={currentStage}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <Text strong style={{ fontSize: 16, display: 'block', marginBottom: 16 }}>
            {stages[currentStage] || 'Processing...'}
          </Text>
        </motion.div>

        {progress !== undefined && (
          <AnimatedProgress 
            percent={progress} 
            strokeColor="#1890ff"
            delay={200}
          />
        )}

        <motion.div
          style={{ marginTop: 16 }}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
        >
          <Text type="secondary">
            Step {currentStage + 1} of {stages.length}
          </Text>
        </motion.div>
      </motion.div>
    </Card>
  )
}

// Celebration animation
export const CelebrationAnimation: React.FC<{
  visible: boolean
  onComplete?: () => void
}> = ({ visible, onComplete }) => {
  useEffect(() => {
    if (visible && onComplete) {
      const timer = setTimeout(onComplete, 3000)
      return () => clearTimeout(timer)
    }
  }, [visible, onComplete])

  return (
    <AnimatePresence>
      {visible && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            zIndex: 2000,
            pointerEvents: 'none',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}
        >
          {/* Confetti effect */}
          {[...Array(20)].map((_, i) => (
            <motion.div
              key={i}
              initial={{
                opacity: 0,
                scale: 0,
                x: 0,
                y: 0,
                rotate: 0
              }}
              animate={{
                opacity: [0, 1, 0],
                scale: [0, 1, 0.5],
                x: (Math.random() - 0.5) * 400,
                y: (Math.random() - 0.5) * 400,
                rotate: Math.random() * 360
              }}
              transition={{
                duration: 2,
                delay: i * 0.1,
                ease: 'easeOut'
              }}
              style={{
                position: 'absolute',
                fontSize: 24,
                color: ['#ff4d4f', '#52c41a', '#1890ff', '#faad14'][i % 4]
              }}
            >
              {[<StarOutlined />, <TrophyOutlined />, <RocketOutlined />][i % 3]}
            </motion.div>
          ))}
          
          {/* Success message */}
          <motion.div
            initial={{ scale: 0, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.5, type: 'spring', stiffness: 200 }}
            style={{
              backgroundColor: 'white',
              padding: 32,
              borderRadius: 16,
              boxShadow: '0 12px 32px rgba(0, 0, 0, 0.15)',
              textAlign: 'center'
            }}
          >
            <TrophyOutlined 
              style={{ 
                fontSize: 64, 
                color: '#faad14',
                marginBottom: 16
              }} 
            />
            <Text strong style={{ fontSize: 20, display: 'block' }}>
              Congratulations!
            </Text>
            <Text type="secondary">
              Task completed successfully
            </Text>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  )
}
