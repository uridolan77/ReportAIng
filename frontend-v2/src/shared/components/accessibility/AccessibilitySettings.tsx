/**
 * Accessibility Settings Panel
 * 
 * Allows users to customize accessibility features:
 * - High contrast mode
 * - Reduced motion
 * - Large fonts
 * - Focus indicators
 * - Screen reader optimizations
 * - Keyboard navigation preferences
 */

import React, { useState } from 'react'
import { 
  Card, 
  Switch, 
  Space, 
  Typography, 
  Divider, 
  Button, 
  Alert, 
  Tooltip,
  Row,
  Col,
  Modal,
} from 'antd'
import {
  EyeOutlined,
  FontSizeOutlined,
  ThunderboltOutlined,
  SoundOutlined,
  ControlOutlined,
  SettingOutlined,
  InfoCircleOutlined,
  ReloadOutlined,
} from '@ant-design/icons'
import { useAccessibility } from './AccessibilityProvider'

const { Title, Text, Paragraph } = Typography

interface AccessibilitySettingsProps {
  /** Show as modal */
  modal?: boolean
  /** Modal visibility */
  visible?: boolean
  /** Modal close handler */
  onClose?: () => void
  /** Compact layout */
  compact?: boolean
}

export const AccessibilitySettings: React.FC<AccessibilitySettingsProps> = ({
  modal = false,
  visible = false,
  onClose,
  compact = false,
}) => {
  const { settings, updateSettings, announce } = useAccessibility()
  const [showHelp, setShowHelp] = useState(false)

  const handleSettingChange = (key: keyof typeof settings, value: boolean) => {
    updateSettings({ [key]: value })
    
    const settingNames = {
      highContrast: 'High contrast mode',
      reduceMotion: 'Reduced motion',
      largeFonts: 'Large fonts',
      focusIndicators: 'Focus indicators',
      screenReader: 'Screen reader optimizations',
      keyboardOnly: 'Keyboard-only navigation',
    }
    
    announce(`${settingNames[key]} ${value ? 'enabled' : 'disabled'}`)
  }

  const resetSettings = () => {
    updateSettings({
      highContrast: false,
      reduceMotion: false,
      largeFonts: false,
      focusIndicators: true,
      screenReader: false,
      keyboardOnly: false,
    })
    announce('Accessibility settings reset to defaults')
  }

  const settingsContent = (
    <div className="accessibility-settings">
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {!compact && (
          <>
            <div>
              <Title level={4}>
                <SettingOutlined /> Accessibility Settings
              </Title>
              <Paragraph>
                Customize your experience to meet your accessibility needs. 
                These settings are saved automatically.
              </Paragraph>
            </div>

            <Alert
              message="WCAG 2.1 AA Compliance"
              description="These settings help ensure the application meets Web Content Accessibility Guidelines (WCAG) 2.1 AA standards."
              type="info"
              showIcon
              icon={<InfoCircleOutlined />}
            />
          </>
        )}

        {/* Visual Settings */}
        <Card size="small" title={<><EyeOutlined /> Visual</>}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Row justify="space-between" align="middle">
              <Col>
                <Space>
                  <Text strong>High Contrast Mode</Text>
                  <Tooltip title="Increases contrast between text and background for better visibility">
                    <InfoCircleOutlined />
                  </Tooltip>
                </Space>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Improves visibility for users with low vision
                </Text>
              </Col>
              <Col>
                <Switch
                  checked={settings.highContrast}
                  onChange={(checked) => handleSettingChange('highContrast', checked)}
                  aria-label="Toggle high contrast mode"
                />
              </Col>
            </Row>

            <Divider style={{ margin: '12px 0' }} />

            <Row justify="space-between" align="middle">
              <Col>
                <Space>
                  <Text strong>Large Fonts</Text>
                  <Tooltip title="Increases font size throughout the application">
                    <InfoCircleOutlined />
                  </Tooltip>
                </Space>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Makes text easier to read
                </Text>
              </Col>
              <Col>
                <Switch
                  checked={settings.largeFonts}
                  onChange={(checked) => handleSettingChange('largeFonts', checked)}
                  aria-label="Toggle large fonts"
                />
              </Col>
            </Row>

            <Divider style={{ margin: '12px 0' }} />

            <Row justify="space-between" align="middle">
              <Col>
                <Space>
                  <Text strong>Enhanced Focus Indicators</Text>
                  <Tooltip title="Shows clear visual indicators when elements are focused">
                    <InfoCircleOutlined />
                  </Tooltip>
                </Space>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Helps with keyboard navigation
                </Text>
              </Col>
              <Col>
                <Switch
                  checked={settings.focusIndicators}
                  onChange={(checked) => handleSettingChange('focusIndicators', checked)}
                  aria-label="Toggle focus indicators"
                />
              </Col>
            </Row>
          </Space>
        </Card>

        {/* Motion Settings */}
        <Card size="small" title={<><ThunderboltOutlined /> Motion</>}>
          <Row justify="space-between" align="middle">
            <Col>
              <Space>
                <Text strong>Reduce Motion</Text>
                <Tooltip title="Reduces or eliminates animations and transitions">
                  <InfoCircleOutlined />
                </Tooltip>
              </Space>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Prevents motion-triggered vestibular disorders
              </Text>
            </Col>
            <Col>
              <Switch
                checked={settings.reduceMotion}
                onChange={(checked) => handleSettingChange('reduceMotion', checked)}
                aria-label="Toggle reduced motion"
              />
            </Col>
          </Row>
        </Card>

        {/* Navigation Settings */}
        <Card size="small" title={<><ControlOutlined /> Navigation</>}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Row justify="space-between" align="middle">
              <Col>
                <Space>
                  <Text strong>Keyboard-Only Navigation</Text>
                  <Tooltip title="Optimizes interface for keyboard-only users">
                    <InfoCircleOutlined />
                  </Tooltip>
                </Space>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Removes mouse-specific interactions
                </Text>
              </Col>
              <Col>
                <Switch
                  checked={settings.keyboardOnly}
                  onChange={(checked) => handleSettingChange('keyboardOnly', checked)}
                  aria-label="Toggle keyboard-only navigation"
                />
              </Col>
            </Row>

            <Divider style={{ margin: '12px 0' }} />

            <Row justify="space-between" align="middle">
              <Col>
                <Space>
                  <Text strong>Screen Reader Optimizations</Text>
                  <Tooltip title="Enhances compatibility with screen reading software">
                    <InfoCircleOutlined />
                  </Tooltip>
                </Space>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Improves screen reader experience
                </Text>
              </Col>
              <Col>
                <Switch
                  checked={settings.screenReader}
                  onChange={(checked) => handleSettingChange('screenReader', checked)}
                  aria-label="Toggle screen reader optimizations"
                />
              </Col>
            </Row>
          </Space>
        </Card>

        {/* Help and Reset */}
        <Card size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Button
              type="link"
              icon={<InfoCircleOutlined />}
              onClick={() => setShowHelp(true)}
              style={{ padding: 0 }}
            >
              Accessibility Help & Keyboard Shortcuts
            </Button>
            
            <Button
              icon={<ReloadOutlined />}
              onClick={resetSettings}
              style={{ width: '100%' }}
            >
              Reset to Defaults
            </Button>
          </Space>
        </Card>
      </Space>

      {/* Help Modal */}
      <Modal
        title="Accessibility Help"
        open={showHelp}
        onCancel={() => setShowHelp(false)}
        footer={[
          <Button key="close" onClick={() => setShowHelp(false)}>
            Close
          </Button>
        ]}
        width={600}
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Title level={5}>Keyboard Shortcuts</Title>
            <ul>
              <li><kbd>Tab</kbd> - Navigate forward through interactive elements</li>
              <li><kbd>Shift + Tab</kbd> - Navigate backward through interactive elements</li>
              <li><kbd>Enter</kbd> or <kbd>Space</kbd> - Activate buttons and links</li>
              <li><kbd>Arrow Keys</kbd> - Navigate within components (tables, charts)</li>
              <li><kbd>Esc</kbd> - Close modals and dropdowns</li>
              <li><kbd>T</kbd> - Toggle chart/table view (in accessible charts)</li>
            </ul>
          </div>

          <div>
            <Title level={5}>Screen Reader Support</Title>
            <Paragraph>
              This application is optimized for screen readers including NVDA, JAWS, and VoiceOver. 
              All interactive elements have proper labels and descriptions.
            </Paragraph>
          </div>

          <div>
            <Title level={5}>Browser Accessibility Features</Title>
            <Paragraph>
              You can also use your browser's built-in accessibility features:
            </Paragraph>
            <ul>
              <li>Zoom in/out with <kbd>Ctrl +</kbd> / <kbd>Ctrl -</kbd></li>
              <li>Use browser's high contrast mode</li>
              <li>Enable browser's reduced motion settings</li>
            </ul>
          </div>

          <Alert
            message="Need More Help?"
            description="If you need additional accessibility accommodations, please contact our support team."
            type="info"
            showIcon
          />
        </Space>
      </Modal>
    </div>
  )

  if (modal) {
    return (
      <Modal
        title="Accessibility Settings"
        open={visible}
        onCancel={onClose}
        footer={[
          <Button key="close" onClick={onClose}>
            Close
          </Button>
        ]}
        width={600}
      >
        {settingsContent}
      </Modal>
    )
  }

  return <Card>{settingsContent}</Card>
}

export default AccessibilitySettings
