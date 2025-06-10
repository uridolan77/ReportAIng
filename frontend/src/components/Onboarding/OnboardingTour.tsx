/**
 * Onboarding Tour Component
 * Provides guided tour for new users
 */

import React, { useState, useEffect } from 'react';
import {
  Tour,
  Button,
  Space,
  Typography,
  Card,
  FloatButton
} from 'antd';
import {
  QuestionCircleOutlined,
  // Icons removed - not used in current implementation
  CloseOutlined
} from '@ant-design/icons';

const { Text } = Typography;

interface OnboardingTourProps {
  isFirstVisit?: boolean;
  onComplete?: () => void;
}

export const OnboardingTour: React.FC<OnboardingTourProps> = ({ 
  isFirstVisit = false, 
  onComplete 
}) => {
  const [open, setOpen] = useState(isFirstVisit);
  const [current, setCurrent] = useState(0);

  const steps = [
    {
      title: 'Welcome to BI Reporting Copilot! 🎉',
      description: (
        <Space direction="vertical">
          <Text>
            Your AI-powered business intelligence assistant is ready to help you explore your data.
          </Text>
          <Text type="secondary">
            Let's take a quick tour to get you started!
          </Text>
        </Space>
      ),
      target: null,
      placement: 'center' as const,
    },
    {
      title: 'Ask Questions in Natural Language',
      description: (
        <Space direction="vertical">
          <Text>
            <strong>This is your main query input.</strong> Just type questions like:
          </Text>
          <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
            <li>"Show me revenue by country last month"</li>
            <li>"Top 10 players by deposits"</li>
            <li>"Compare casino vs sports betting"</li>
          </ul>
          <Text type="secondary">
            No SQL knowledge required - our AI understands natural language!
          </Text>
        </Space>
      ),
      target: () => document.querySelector('.minimal-query-card') as HTMLElement,
      placement: 'bottom' as const,
    },
    {
      title: 'Quick Action Cards',
      description: (
        <Space direction="vertical">
          <Text>
            <strong>Use these shortcuts to get started quickly:</strong>
          </Text>
          <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
            <li><strong>Query Templates:</strong> Pre-built queries for common tasks</li>
            <li><strong>Recent Queries:</strong> Access your query history</li>
            <li><strong>Quick Examples:</strong> Try sample queries</li>
          </ul>
        </Space>
      ),
      target: () => document.querySelector('.quick-action-card') as HTMLElement,
      placement: 'top' as const,
    },
    {
      title: 'Example Queries',
      description: (
        <Space direction="vertical">
          <Text>
            <strong>Click any example to try it out!</strong>
          </Text>
          <Text type="secondary">
            These examples show the types of questions you can ask your data.
          </Text>
        </Space>
      ),
      target: () => document.querySelector('.example-query-btn') as HTMLElement,
      placement: 'top' as const,
    },
    {
      title: 'Navigation Menu',
      description: (
        <Space direction="vertical">
          <Text>
            <strong>Access all features through the menu:</strong>
          </Text>
          <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
            <li><strong>Analytics & Visualization:</strong> Charts and dashboards</li>
            <li><strong>Query Tools:</strong> Templates, history, and suggestions</li>
            <li><strong>Admin Tools:</strong> AI tuning and settings (if admin)</li>
          </ul>
        </Space>
      ),
      target: () => document.querySelector('.ant-btn:has(.anticon-menu)') as HTMLElement,
      placement: 'bottomLeft' as const,
    },
    {
      title: 'Database Status',
      description: (
        <Space direction="vertical">
          <Text>
            <strong>Monitor your database connection here.</strong>
          </Text>
          <Text type="secondary">
            Green means connected, orange means offline mode.
          </Text>
        </Space>
      ),
      target: () => document.querySelector('[data-testid="database-status"]') as HTMLElement,
      placement: 'bottom' as const,
    },
    {
      title: 'You\'re All Set! 🚀',
      description: (
        <Space direction="vertical">
          <Text>
            <strong>Ready to explore your data!</strong>
          </Text>
          <Text type="secondary">
            Start by typing a question or clicking one of the examples below.
          </Text>
          <Text type="secondary">
            You can restart this tour anytime by clicking the help button.
          </Text>
        </Space>
      ),
      target: null,
      placement: 'center' as const,
    },
  ];

  const handleClose = () => {
    setOpen(false);
    onComplete?.();
    localStorage.setItem('onboarding-completed', 'true');
  };

  const handleNext = () => {
    if (current < steps.length - 1) {
      setCurrent(current + 1);
    } else {
      handleClose();
    }
  };

  const handlePrev = () => {
    if (current > 0) {
      setCurrent(current - 1);
    }
  };

  // Check if user has completed onboarding
  useEffect(() => {
    const hasCompleted = localStorage.getItem('onboarding-completed');
    if (!hasCompleted && !isFirstVisit) {
      // Show tour for new users
      setOpen(true);
    }
  }, [isFirstVisit]);

  return (
    <>
      <Tour
        open={open}
        onClose={handleClose}
        steps={steps}
        current={current}
        type="primary"
        arrow={true}
        placement="bottom"
        mask={{
          style: {
            boxShadow: 'inset 0 0 15px #fff',
          },
        }}

      />

      {/* Help Float Button */}
      <FloatButton
        icon={<QuestionCircleOutlined />}
        tooltip="Start Tour"
        onClick={() => {
          setCurrent(0);
          setOpen(true);
        }}
        style={{
          bottom: 24,
          right: 24,
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          border: 'none',
        }}
      />
    </>
  );
};
