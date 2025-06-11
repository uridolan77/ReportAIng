/**
 * Animation Presets Component
 * UI for selecting and customizing animation presets
 */

import React, { useState, useCallback } from 'react';
import { Card, Select, Slider, Switch, Button, Space, Typography, Divider, Row, Col, Tooltip } from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  ReloadOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  HeartOutlined,
  StarOutlined,
  RocketOutlined,
  BulbOutlined,
  FireOutlined
} from '@ant-design/icons';

const { Title, Text } = Typography;
const { Option } = Select;

export interface AnimationPreset {
  id: string;
  name: string;
  description: string;
  icon: React.ReactNode;
  duration: number;
  easing: string;
  staggerDelay: number;
  intensity: 'low' | 'medium' | 'high';
  category: 'business' | 'creative' | 'technical' | 'playful';
  config: {
    scale: number;
    opacity: number;
    blur: number;
    rotation: number;
    bounce: boolean;
    elastic: boolean;
  };
}

export const ANIMATION_PRESETS: AnimationPreset[] = [
  {
    id: 'smooth',
    name: 'Smooth Professional',
    description: 'Clean, professional animations perfect for business presentations',
    icon: <BulbOutlined />,
    duration: 1000,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    staggerDelay: 100,
    intensity: 'medium',
    category: 'business',
    config: {
      scale: 1.02,
      opacity: 0.9,
      blur: 0,
      rotation: 0,
      bounce: false,
      elastic: false
    }
  },
  {
    id: 'bouncy',
    name: 'Bouncy Playful',
    description: 'Fun, energetic animations with elastic effects',
    icon: <HeartOutlined />,
    duration: 800,
    easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
    staggerDelay: 80,
    intensity: 'high',
    category: 'playful',
    config: {
      scale: 1.1,
      opacity: 1,
      blur: 0,
      rotation: 5,
      bounce: true,
      elastic: true
    }
  },
  {
    id: 'fast',
    name: 'Lightning Fast',
    description: 'Quick, snappy animations for rapid interactions',
    icon: <ThunderboltOutlined />,
    duration: 400,
    easing: 'cubic-bezier(0.25, 0.46, 0.45, 0.94)',
    staggerDelay: 50,
    intensity: 'low',
    category: 'technical',
    config: {
      scale: 1.01,
      opacity: 0.95,
      blur: 0,
      rotation: 0,
      bounce: false,
      elastic: false
    }
  },
  {
    id: 'elegant',
    name: 'Elegant Luxury',
    description: 'Sophisticated, refined animations for premium experiences',
    icon: <StarOutlined />,
    duration: 1500,
    easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
    staggerDelay: 150,
    intensity: 'medium',
    category: 'creative',
    config: {
      scale: 1.05,
      opacity: 0.8,
      blur: 2,
      rotation: 2,
      bounce: false,
      elastic: false
    }
  },
  {
    id: 'dramatic',
    name: 'Dramatic Impact',
    description: 'Bold, attention-grabbing animations for key moments',
    icon: <FireOutlined />,
    duration: 2000,
    easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
    staggerDelay: 200,
    intensity: 'high',
    category: 'creative',
    config: {
      scale: 1.2,
      opacity: 1,
      blur: 0,
      rotation: 10,
      bounce: true,
      elastic: true
    }
  },
  {
    id: 'minimal',
    name: 'Minimal Subtle',
    description: 'Understated animations that don\'t distract from content',
    icon: <RocketOutlined />,
    duration: 300,
    easing: 'ease',
    staggerDelay: 30,
    intensity: 'low',
    category: 'business',
    config: {
      scale: 1.005,
      opacity: 0.98,
      blur: 0,
      rotation: 0,
      bounce: false,
      elastic: false
    }
  }
];

interface AnimationPresetsProps {
  selectedPreset?: string;
  onPresetChange?: (preset: AnimationPreset) => void;
  onCustomConfigChange?: (config: Partial<AnimationPreset>) => void;
  showPreview?: boolean;
  allowCustomization?: boolean;
  category?: 'business' | 'creative' | 'technical' | 'playful' | 'all';
}

export const AnimationPresets: React.FC<AnimationPresetsProps> = ({
  selectedPreset = 'smooth',
  onPresetChange,
  onCustomConfigChange,
  showPreview = true,
  allowCustomization = true,
  category = 'all'
}) => {
  const [customConfig, setCustomConfig] = useState<Partial<AnimationPreset>>({});
  const [isPlaying, setIsPlaying] = useState(false);
  const [previewElement, setPreviewElement] = useState<HTMLDivElement | null>(null);

  // Filter presets by category
  const filteredPresets = category === 'all' 
    ? ANIMATION_PRESETS 
    : ANIMATION_PRESETS.filter(preset => preset.category === category);

  const currentPreset = ANIMATION_PRESETS.find(p => p.id === selectedPreset) || ANIMATION_PRESETS[0];
  const mergedConfig = { ...currentPreset, ...customConfig };

  // Handle preset selection
  const handlePresetSelect = useCallback((presetId: string) => {
    const preset = ANIMATION_PRESETS.find(p => p.id === presetId);
    if (preset && onPresetChange) {
      onPresetChange(preset);
      setCustomConfig({});
    }
  }, [onPresetChange]);

  // Handle custom configuration changes
  const handleConfigChange = useCallback((key: string, value: any) => {
    const newConfig = { ...customConfig, [key]: value };
    setCustomConfig(newConfig);
    
    if (onCustomConfigChange) {
      onCustomConfigChange(newConfig);
    }
  }, [customConfig, onCustomConfigChange]);

  // Play preview animation
  const playPreview = useCallback(() => {
    if (!previewElement || !showPreview) return;

    setIsPlaying(true);
    
    // Apply animation styles
    const element = previewElement;
    element.style.transition = `all ${mergedConfig.duration}ms ${mergedConfig.easing}`;
    element.style.transform = `scale(${mergedConfig.config.scale}) rotate(${mergedConfig.config.rotation}deg)`;
    element.style.opacity = String(mergedConfig.config.opacity);
    
    if (mergedConfig.config.blur > 0) {
      element.style.filter = `blur(${mergedConfig.config.blur}px)`;
    }

    // Reset after animation
    setTimeout(() => {
      element.style.transform = 'scale(1) rotate(0deg)';
      element.style.opacity = '1';
      element.style.filter = 'none';
      setIsPlaying(false);
    }, mergedConfig.duration);
  }, [previewElement, mergedConfig, showPreview]);

  // Get intensity color
  const getIntensityColor = (intensity: string) => {
    switch (intensity) {
      case 'low': return '#52c41a';
      case 'medium': return '#faad14';
      case 'high': return '#f5222d';
      default: return '#1890ff';
    }
  };

  // Get category color
  const getCategoryColor = (cat: string) => {
    switch (cat) {
      case 'business': return '#1890ff';
      case 'creative': return '#722ed1';
      case 'technical': return '#13c2c2';
      case 'playful': return '#eb2f96';
      default: return '#1890ff';
    }
  };

  return (
    <div className="animation-presets">
      {/* Preset Selection */}
      <Card title="Animation Presets" style={{ marginBottom: 'var(--space-4)' }}>
        <Row gutter={[16, 16]}>
          {filteredPresets.map((preset) => (
            <Col key={preset.id} xs={24} sm={12} md={8} lg={6}>
              <Card
                size="small"
                hoverable
                className={selectedPreset === preset.id ? 'preset-selected' : ''}
                onClick={() => handlePresetSelect(preset.id)}
                style={{
                  border: selectedPreset === preset.id ? '2px solid var(--color-primary)' : '1px solid var(--border-primary)',
                  cursor: 'pointer'
                }}
              >
                <div style={{ textAlign: 'center' }}>
                  <div style={{ 
                    fontSize: '24px', 
                    color: getCategoryColor(preset.category),
                    marginBottom: 'var(--space-2)'
                  }}>
                    {preset.icon}
                  </div>
                  <Title level={5} style={{ margin: 0, marginBottom: 'var(--space-1)' }}>
                    {preset.name}
                  </Title>
                  <Text type="secondary" style={{ fontSize: 'var(--text-xs)' }}>
                    {preset.description}
                  </Text>
                  <div style={{ marginTop: 'var(--space-2)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <span style={{ 
                      fontSize: 'var(--text-xs)', 
                      color: getIntensityColor(preset.intensity),
                      fontWeight: 'bold'
                    }}>
                      {preset.intensity.toUpperCase()}
                    </span>
                    <span style={{ 
                      fontSize: 'var(--text-xs)', 
                      color: 'var(--text-tertiary)'
                    }}>
                      {preset.duration}ms
                    </span>
                  </div>
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </Card>

      {/* Preview Section */}
      {showPreview && (
        <Card title="Preview" style={{ marginBottom: 'var(--space-4)' }}>
          <div style={{ textAlign: 'center', padding: 'var(--space-8)' }}>
            <div
              ref={setPreviewElement}
              style={{
                width: '100px',
                height: '100px',
                background: 'linear-gradient(135deg, var(--color-primary), var(--color-secondary))',
                borderRadius: 'var(--radius-lg)',
                margin: '0 auto',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'white',
                fontSize: '24px',
                boxShadow: 'var(--shadow-lg)'
              }}
            >
              {currentPreset.icon}
            </div>
            <div style={{ marginTop: 'var(--space-4)' }}>
              <Button
                type="primary"
                icon={isPlaying ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
                onClick={playPreview}
                disabled={isPlaying}
              >
                {isPlaying ? 'Playing...' : 'Play Preview'}
              </Button>
            </div>
          </div>
        </Card>
      )}

      {/* Customization Panel */}
      {allowCustomization && (
        <Card title="Customize Animation" extra={
          <Button 
            size="small" 
            icon={<ReloadOutlined />}
            onClick={() => setCustomConfig({})}
          >
            Reset
          </Button>
        }>
          <Row gutter={[24, 16]}>
            <Col span={12}>
              <div>
                <Text strong>Duration (ms)</Text>
                <Slider
                  min={100}
                  max={3000}
                  step={100}
                  value={mergedConfig.duration}
                  onChange={(value) => handleConfigChange('duration', value)}
                  tooltip={{ formatter: (value) => `${value}ms` }}
                />
              </div>
            </Col>
            <Col span={12}>
              <div>
                <Text strong>Stagger Delay (ms)</Text>
                <Slider
                  min={0}
                  max={500}
                  step={10}
                  value={mergedConfig.staggerDelay}
                  onChange={(value) => handleConfigChange('staggerDelay', value)}
                  tooltip={{ formatter: (value) => `${value}ms` }}
                />
              </div>
            </Col>
            <Col span={12}>
              <div>
                <Text strong>Scale Factor</Text>
                <Slider
                  min={1}
                  max={1.5}
                  step={0.01}
                  value={mergedConfig.config.scale}
                  onChange={(value) => handleConfigChange('config', { ...mergedConfig.config, scale: value })}
                  tooltip={{ formatter: (value) => `${value}x` }}
                />
              </div>
            </Col>
            <Col span={12}>
              <div>
                <Text strong>Opacity</Text>
                <Slider
                  min={0.1}
                  max={1}
                  step={0.05}
                  value={mergedConfig.config.opacity}
                  onChange={(value) => handleConfigChange('config', { ...mergedConfig.config, opacity: value })}
                  tooltip={{ formatter: (value) => `${Math.round(value * 100)}%` }}
                />
              </div>
            </Col>
            <Col span={12}>
              <div>
                <Text strong>Rotation (degrees)</Text>
                <Slider
                  min={0}
                  max={360}
                  step={5}
                  value={mergedConfig.config.rotation}
                  onChange={(value) => handleConfigChange('config', { ...mergedConfig.config, rotation: value })}
                  tooltip={{ formatter: (value) => `${value}°` }}
                />
              </div>
            </Col>
            <Col span={12}>
              <div>
                <Text strong>Blur (px)</Text>
                <Slider
                  min={0}
                  max={10}
                  step={0.5}
                  value={mergedConfig.config.blur}
                  onChange={(value) => handleConfigChange('config', { ...mergedConfig.config, blur: value })}
                  tooltip={{ formatter: (value) => `${value}px` }}
                />
              </div>
            </Col>
            <Col span={12}>
              <Space direction="vertical">
                <div>
                  <Text strong>Bounce Effect</Text>
                  <Switch
                    checked={mergedConfig.config.bounce}
                    onChange={(checked) => handleConfigChange('config', { ...mergedConfig.config, bounce: checked })}
                    style={{ marginLeft: 'var(--space-2)' }}
                  />
                </div>
                <div>
                  <Text strong>Elastic Effect</Text>
                  <Switch
                    checked={mergedConfig.config.elastic}
                    onChange={(checked) => handleConfigChange('config', { ...mergedConfig.config, elastic: checked })}
                    style={{ marginLeft: 'var(--space-2)' }}
                  />
                </div>
              </Space>
            </Col>
          </Row>
        </Card>
      )}
    </div>
  );
};
