/**
 * Enhanced LLM Modal Component
 * 
 * Provides consistent modal styling with gradients, shadows, and modern UI patterns
 * for all LLM management forms and dialogs.
 */

import React from 'react';
import { Modal, Form, Button, Space } from 'antd';
import { designTokens } from '../../core/design-system';

export interface LLMModalProps {
  title: string;
  open: boolean;
  onCancel: () => void;
  onOk?: () => void;
  children: React.ReactNode;
  width?: number;
  footer?: React.ReactNode | null;
  loading?: boolean;
  okText?: string;
  cancelText?: string;
  okButtonProps?: any;
  cancelButtonProps?: any;
  className?: string;
  destroyOnClose?: boolean;
}

const LLMModal: React.FC<LLMModalProps> = ({
  title,
  open,
  onCancel,
  onOk,
  children,
  width = 600,
  footer,
  loading = false,
  okText = 'Save',
  cancelText = 'Cancel',
  okButtonProps = {},
  cancelButtonProps = {},
  className = '',
  destroyOnClose = true,
}) => {
  const defaultFooter = footer === null ? null : footer || (
    <Space>
      <Button 
        onClick={onCancel}
        {...cancelButtonProps}
        style={{
          borderRadius: designTokens.borderRadius.medium,
          ...cancelButtonProps.style,
        }}
      >
        {cancelText}
      </Button>
      <Button 
        type="primary" 
        onClick={onOk}
        loading={loading}
        {...okButtonProps}
        style={{
          borderRadius: designTokens.borderRadius.medium,
          background: `linear-gradient(135deg, ${designTokens.colors.primary} 0%, ${designTokens.colors.primaryHover} 100%)`,
          border: 'none',
          boxShadow: designTokens.shadows.medium,
          ...okButtonProps.style,
        }}
      >
        {okText}
      </Button>
    </Space>
  );

  return (
    <>
      <Modal
        title={
          <div style={{
            fontSize: designTokens.typography.fontSize.lg,
            fontWeight: designTokens.typography.fontWeight.semibold,
            color: designTokens.colors.text,
            padding: `${designTokens.spacing.sm} 0`,
          }}>
            {title}
          </div>
        }
        open={open}
        onCancel={onCancel}
        footer={defaultFooter}
        width={width}
        destroyOnClose={destroyOnClose}
        className={`llm-modal ${className}`}
        styles={{
          header: {
            background: `linear-gradient(135deg, ${designTokens.colors.backgroundSecondary} 0%, ${designTokens.colors.white} 100%)`,
            borderBottom: `2px solid ${designTokens.colors.border}`,
            borderRadius: `${designTokens.borderRadius.large} ${designTokens.borderRadius.large} 0 0`,
          },
          body: {
            padding: designTokens.spacing.lg,
            background: designTokens.colors.white,
          },
          footer: {
            background: `linear-gradient(135deg, ${designTokens.colors.backgroundSecondary} 0%, ${designTokens.colors.white} 100%)`,
            borderTop: `1px solid ${designTokens.colors.border}`,
            borderRadius: `0 0 ${designTokens.borderRadius.large} ${designTokens.borderRadius.large}`,
            padding: designTokens.spacing.md,
          },
        }}
        style={{
          top: '5vh',
        }}
      >
        <div style={{
          background: `linear-gradient(135deg, ${designTokens.colors.white} 0%, ${designTokens.colors.backgroundSecondary} 100%)`,
          borderRadius: designTokens.borderRadius.medium,
          padding: designTokens.spacing.md,
          margin: `-${designTokens.spacing.md}`,
          marginTop: 0,
          marginBottom: 0,
        }}>
          {children}
        </div>
      </Modal>
      
      <style jsx global>{`
        .llm-modal .ant-modal-content {
          border-radius: ${designTokens.borderRadius.large} !important;
          box-shadow: ${designTokens.shadows.xl} !important;
          overflow: hidden;
        }
        
        .llm-modal .ant-modal-header {
          margin: 0 !important;
          padding: ${designTokens.spacing.md} ${designTokens.spacing.lg} !important;
        }
        
        .llm-modal .ant-modal-close {
          top: ${designTokens.spacing.md} !important;
          right: ${designTokens.spacing.md} !important;
        }
        
        .llm-modal .ant-modal-close-x {
          width: 32px !important;
          height: 32px !important;
          line-height: 32px !important;
          border-radius: ${designTokens.borderRadius.medium} !important;
          background: rgba(0, 0, 0, 0.05) !important;
          transition: all ${designTokens.animations.duration.fast} ${designTokens.animations.easing.easeInOut} !important;
        }
        
        .llm-modal .ant-modal-close-x:hover {
          background: rgba(0, 0, 0, 0.1) !important;
          transform: scale(1.05);
        }
      `}</style>
    </>
  );
};

export default LLMModal;

// Enhanced Form Modal specifically for LLM forms
export interface LLMFormModalProps extends Omit<LLMModalProps, 'children' | 'onOk'> {
  form: any;
  onFinish: (values: any) => void | Promise<void>;
  children: React.ReactNode;
  submitOnOk?: boolean;
}

export const LLMFormModal: React.FC<LLMFormModalProps> = ({
  form,
  onFinish,
  children,
  submitOnOk = true,
  ...modalProps
}) => {
  const handleOk = () => {
    if (submitOnOk) {
      form.submit();
    }
  };

  return (
    <LLMModal
      {...modalProps}
      onOk={handleOk}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        style={{
          background: 'transparent',
        }}
      >
        {children}
      </Form>
    </LLMModal>
  );
};
