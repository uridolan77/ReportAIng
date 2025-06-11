/**
 * Theme Customization UI
 * Advanced theme customization interface with live preview
 */

import React, { useState, useCallback, useEffect } from 'react';
import { Card, Row, Col, ColorPicker, Slider, Select, Switch, Button, Space, Typography, Divider, Tabs, Alert, Input } from 'antd';
import {
  BgColorsOutlined,
  EyeOutlined,
  SaveOutlined,
  ReloadOutlined,
  DownloadOutlined,
  UploadOutlined,
  ShareAltOutlined,
  CopyOutlined
} from '@ant-design/icons';
import { useTheme } from '../../contexts/ThemeContext';

const { Title, Text } = Typography;
const { TabPane } = Tabs;
const { TextArea } = Input;

interface CustomTheme {
  name: string;
  colors: {
    primary: string;
    secondary: string;
    success: string;
    warning: string;
    error: string;
    info: string;
  };
  backgrounds: {
    primary: string;
    secondary: string;
    tertiary: string;
    quaternary: string;
  };
  text: {
    primary: string;
    secondary: string;
    tertiary: string;
    quaternary: string;
  };
  borders: {
    primary: string;
    secondary: string;
    tertiary: string;
  };
  shadows: {
    sm: string;
    md: string;
    lg: string;
    xl: string;
  };
  borderRadius: {
    sm: number;
    md: number;
    lg: number;
    xl: number;
  };
  spacing: {
    scale: number;
  };
  typography: {
    fontFamily: string;
    fontSize: number;
    lineHeight: number;
  };
}

const DEFAULT_LIGHT_THEME: CustomTheme = {
  name: 'Custom Light',
  colors: {
    primary: '#3b82f6',
    secondary: '#8b5cf6',
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6'
  },
  backgrounds: {
    primary: '#ffffff',
    secondary: '#f9fafb',
    tertiary: '#f3f4f6',
    quaternary: '#e5e7eb'
  },
  text: {
    primary: '#111827',
    secondary: '#374151',
    tertiary: '#6b7280',
    quaternary: '#9ca3af'
  },
  borders: {
    primary: '#e5e7eb',
    secondary: '#d1d5db',
    tertiary: '#9ca3af'
  },
  shadows: {
    sm: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
    xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)'
  },
  borderRadius: {
    sm: 6,
    md: 12,
    lg: 20,
    xl: 24
  },
  spacing: {
    scale: 1
  },
  typography: {
    fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, sans-serif',
    fontSize: 16,
    lineHeight: 1.5
  }
};

const DEFAULT_DARK_THEME: CustomTheme = {
  ...DEFAULT_LIGHT_THEME,
  name: 'Custom Dark',
  colors: {
    primary: '#60a5fa',
    secondary: '#a78bfa',
    success: '#34d399',
    warning: '#fbbf24',
    error: '#f87171',
    info: '#60a5fa'
  },
  backgrounds: {
    primary: '#1f2937',
    secondary: '#111827',
    tertiary: '#374151',
    quaternary: '#4b5563'
  },
  text: {
    primary: '#f9fafb',
    secondary: '#d1d5db',
    tertiary: '#9ca3af',
    quaternary: '#6b7280'
  },
  borders: {
    primary: '#374151',
    secondary: '#4b5563',
    tertiary: '#6b7280'
  },
  shadows: {
    sm: '0 1px 2px 0 rgba(0, 0, 0, 0.3)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.4), 0 2px 4px -1px rgba(0, 0, 0, 0.3)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.4), 0 4px 6px -2px rgba(0, 0, 0, 0.3)',
    xl: '0 20px 25px -5px rgba(0, 0, 0, 0.4), 0 10px 10px -5px rgba(0, 0, 0, 0.3)'
  }
};

export const ThemeCustomization: React.FC = () => {
  const { actualTheme } = useTheme();
  const [customTheme, setCustomTheme] = useState<CustomTheme>(
    actualTheme === 'dark' ? DEFAULT_DARK_THEME : DEFAULT_LIGHT_THEME
  );
  const [isPreviewMode, setIsPreviewMode] = useState(false);
  const [savedThemes, setSavedThemes] = useState<CustomTheme[]>([]);

  // Apply custom theme to CSS variables
  const applyTheme = useCallback((theme: CustomTheme, preview: boolean = false) => {
    const root = document.documentElement;
    const prefix = preview ? '--preview-' : '--';

    // Apply colors
    Object.entries(theme.colors).forEach(([key, value]) => {
      root.style.setProperty(`${prefix}color-${key}`, value);
    });

    // Apply backgrounds
    Object.entries(theme.backgrounds).forEach(([key, value]) => {
      root.style.setProperty(`${prefix}bg-${key}`, value);
    });

    // Apply text colors
    Object.entries(theme.text).forEach(([key, value]) => {
      root.style.setProperty(`${prefix}text-${key}`, value);
    });

    // Apply borders
    Object.entries(theme.borders).forEach(([key, value]) => {
      root.style.setProperty(`${prefix}border-${key}`, value);
    });

    // Apply shadows
    Object.entries(theme.shadows).forEach(([key, value]) => {
      root.style.setProperty(`${prefix}shadow-${key}`, value);
    });

    // Apply border radius
    Object.entries(theme.borderRadius).forEach(([key, value]) => {
      root.style.setProperty(`${prefix}radius-${key}`, `${value}px`);
    });

    // Apply spacing
    root.style.setProperty(`${prefix}space-scale`, String(theme.spacing.scale));

    // Apply typography
    root.style.setProperty(`${prefix}font-family-primary`, theme.typography.fontFamily);
    root.style.setProperty(`${prefix}text-base`, `${theme.typography.fontSize}px`);
    root.style.setProperty(`${prefix}line-height-normal`, String(theme.typography.lineHeight));
  }, []);

  // Update theme property
  const updateTheme = useCallback((path: string, value: any) => {
    setCustomTheme(prev => {
      const newTheme = { ...prev };
      const keys = path.split('.');
      let current: any = newTheme;
      
      for (let i = 0; i < keys.length - 1; i++) {
        current = current[keys[i]];
      }
      
      current[keys[keys.length - 1]] = value;
      
      if (isPreviewMode) {
        applyTheme(newTheme, true);
      }
      
      return newTheme;
    });
  }, [isPreviewMode, applyTheme]);

  // Toggle preview mode
  const togglePreview = useCallback(() => {
    setIsPreviewMode(prev => {
      const newPreviewMode = !prev;
      if (newPreviewMode) {
        applyTheme(customTheme, true);
      } else {
        // Reset to original theme
        const root = document.documentElement;
        const previewVars = Array.from(root.style).filter(prop => prop.startsWith('--preview-'));
        previewVars.forEach(prop => root.style.removeProperty(prop));
      }
      return newPreviewMode;
    });
  }, [customTheme, applyTheme]);

  // Save theme
  const saveTheme = useCallback(() => {
    const newTheme = { ...customTheme, name: customTheme.name || `Custom Theme ${Date.now()}` };
    setSavedThemes(prev => [...prev, newTheme]);
    localStorage.setItem('custom-themes', JSON.stringify([...savedThemes, newTheme]));
  }, [customTheme, savedThemes]);

  // Load saved themes
  useEffect(() => {
    const saved = localStorage.getItem('custom-themes');
    if (saved) {
      try {
        setSavedThemes(JSON.parse(saved));
      } catch (error) {
        console.error('Failed to load saved themes:', error);
      }
    }
  }, []);

  // Export theme
  const exportTheme = useCallback(() => {
    const blob = new Blob([JSON.stringify(customTheme, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${customTheme.name.replace(/\s+/g, '-').toLowerCase()}.json`;
    a.click();
    URL.revokeObjectURL(url);
  }, [customTheme]);

  // Import theme
  const importTheme = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const imported = JSON.parse(e.target?.result as string);
        setCustomTheme(imported);
      } catch (error) {
        console.error('Failed to import theme:', error);
      }
    };
    reader.readAsText(file);
  }, []);

  // Reset theme
  const resetTheme = useCallback(() => {
    setCustomTheme(actualTheme === 'dark' ? DEFAULT_DARK_THEME : DEFAULT_LIGHT_THEME);
  }, [actualTheme]);

  return (
    <div className="theme-customization">
      <Card 
        title="Theme Customization" 
        extra={
          <Space>
            <Switch
              checked={isPreviewMode}
              onChange={togglePreview}
              checkedChildren={<EyeOutlined />}
              unCheckedChildren="Preview"
            />
            <Button icon={<SaveOutlined />} onClick={saveTheme}>
              Save
            </Button>
            <Button icon={<ReloadOutlined />} onClick={resetTheme}>
              Reset
            </Button>
            <Button icon={<DownloadOutlined />} onClick={exportTheme}>
              Export
            </Button>
            <Button icon={<UploadOutlined />}>
              <input
                type="file"
                accept=".json"
                onChange={importTheme}
                style={{ position: 'absolute', opacity: 0, width: '100%', height: '100%', cursor: 'pointer' }}
              />
              Import
            </Button>
          </Space>
        }
      >
        {isPreviewMode && (
          <Alert
            message="Preview Mode Active"
            description="Changes are being applied in real-time. Toggle off to return to normal view."
            type="info"
            showIcon
            style={{ marginBottom: 'var(--space-4)' }}
          />
        )}

        <Tabs defaultActiveKey="colors">
          <TabPane tab="Colors" key="colors">
            <Row gutter={[24, 16]}>
              <Col span={12}>
                <Title level={5}>Brand Colors</Title>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Primary</Text>
                    <ColorPicker
                      value={customTheme.colors.primary}
                      onChange={(color) => updateTheme('colors.primary', color.toHexString())}
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Secondary</Text>
                    <ColorPicker
                      value={customTheme.colors.secondary}
                      onChange={(color) => updateTheme('colors.secondary', color.toHexString())}
                    />
                  </div>
                </Space>
              </Col>
              <Col span={12}>
                <Title level={5}>Semantic Colors</Title>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Success</Text>
                    <ColorPicker
                      value={customTheme.colors.success}
                      onChange={(color) => updateTheme('colors.success', color.toHexString())}
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Warning</Text>
                    <ColorPicker
                      value={customTheme.colors.warning}
                      onChange={(color) => updateTheme('colors.warning', color.toHexString())}
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Error</Text>
                    <ColorPicker
                      value={customTheme.colors.error}
                      onChange={(color) => updateTheme('colors.error', color.toHexString())}
                    />
                  </div>
                </Space>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Layout" key="layout">
            <Row gutter={[24, 16]}>
              <Col span={12}>
                <Title level={5}>Border Radius</Title>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text>Small: {customTheme.borderRadius.sm}px</Text>
                    <Slider
                      min={0}
                      max={20}
                      value={customTheme.borderRadius.sm}
                      onChange={(value) => updateTheme('borderRadius.sm', value)}
                    />
                  </div>
                  <div>
                    <Text>Medium: {customTheme.borderRadius.md}px</Text>
                    <Slider
                      min={0}
                      max={30}
                      value={customTheme.borderRadius.md}
                      onChange={(value) => updateTheme('borderRadius.md', value)}
                    />
                  </div>
                  <div>
                    <Text>Large: {customTheme.borderRadius.lg}px</Text>
                    <Slider
                      min={0}
                      max={40}
                      value={customTheme.borderRadius.lg}
                      onChange={(value) => updateTheme('borderRadius.lg', value)}
                    />
                  </div>
                </Space>
              </Col>
              <Col span={12}>
                <Title level={5}>Spacing</Title>
                <div>
                  <Text>Scale Factor: {customTheme.spacing.scale}x</Text>
                  <Slider
                    min={0.5}
                    max={2}
                    step={0.1}
                    value={customTheme.spacing.scale}
                    onChange={(value) => updateTheme('spacing.scale', value)}
                  />
                </div>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Typography" key="typography">
            <Row gutter={[24, 16]}>
              <Col span={24}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Font Family</Text>
                    <Input
                      value={customTheme.typography.fontFamily}
                      onChange={(e) => updateTheme('typography.fontFamily', e.target.value)}
                      placeholder="Inter, sans-serif"
                    />
                  </div>
                  <div>
                    <Text strong>Base Font Size: {customTheme.typography.fontSize}px</Text>
                    <Slider
                      min={12}
                      max={24}
                      value={customTheme.typography.fontSize}
                      onChange={(value) => updateTheme('typography.fontSize', value)}
                    />
                  </div>
                  <div>
                    <Text strong>Line Height: {customTheme.typography.lineHeight}</Text>
                    <Slider
                      min={1}
                      max={2}
                      step={0.1}
                      value={customTheme.typography.lineHeight}
                      onChange={(value) => updateTheme('typography.lineHeight', value)}
                    />
                  </div>
                </Space>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Export/Import" key="export">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Theme Name</Text>
                <Input
                  value={customTheme.name}
                  onChange={(e) => updateTheme('name', e.target.value)}
                  placeholder="My Custom Theme"
                />
              </div>
              <Divider />
              <div>
                <Text strong>CSS Variables</Text>
                <TextArea
                  rows={10}
                  value={Object.entries(customTheme.colors).map(([key, value]) => `--color-${key}: ${value};`).join('\n')}
                  readOnly
                />
              </div>
              <Space>
                <Button icon={<CopyOutlined />}>Copy CSS</Button>
                <Button icon={<ShareAltOutlined />}>Share Theme</Button>
              </Space>
            </Space>
          </TabPane>
        </Tabs>
      </Card>
    </div>
  );
};
