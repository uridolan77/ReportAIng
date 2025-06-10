/**
 * Form Components
 * Advanced form components with validation and consistent styling
 */

import React from 'react';
import { 
  Form as AntForm, 
  FormProps as AntFormProps,
  Input as AntInput,
  InputProps as AntInputProps,
  Select as AntSelect,
  SelectProps as AntSelectProps,
  Checkbox as AntCheckbox,
  CheckboxProps as AntCheckboxProps,
  Radio as AntRadio,
  RadioProps as AntRadioProps,
  Switch as AntSwitch,
  SwitchProps as AntSwitchProps,
  DatePicker as AntDatePicker,
  DatePickerProps as AntDatePickerProps,
  TimePicker as AntTimePicker,
  TimePickerProps as AntTimePickerProps,
  Upload as AntUpload,
  UploadProps as AntUploadProps
} from 'antd';

const { TextArea } = AntInput;
const { Option } = AntSelect;
const { Group: RadioGroup } = AntRadio;
const { Group: CheckboxGroup } = AntCheckbox;

// Form Component
export interface FormProps extends Omit<AntFormProps, 'children'> {
  variant?: 'filled' | 'borderless' | 'outlined';
  children?: React.ReactNode;
}

export const Form: React.FC<FormProps> = ({ 
  variant = 'default',
  className,
  ...props 
}) => {
  const getLayoutProps = () => {
    switch (variant) {
      case 'compact':
        return {
          labelCol: { span: 6 },
          wrapperCol: { span: 18 },
          size: 'small' as const,
        };
      case 'spacious':
        return {
          labelCol: { span: 4 },
          wrapperCol: { span: 20 },
          size: 'large' as const,
        };
      default:
        return {
          labelCol: { span: 6 },
          wrapperCol: { span: 18 },
          size: 'middle' as const,
        };
    }
  };

  return (
    <AntForm
      {...getLayoutProps()}
      {...props}
      className={`ui-form ui-form-${variant} ${className || ''}`}
      style={{
        ...props.style,
      }}
    />
  );
};

// Form Item Component
export interface FormItemProps {
  children: React.ReactNode;
  label?: string;
  name?: string;
  required?: boolean;
  rules?: any[];
  help?: string;
  validateStatus?: 'success' | 'warning' | 'error' | 'validating';
  className?: string;
  style?: React.CSSProperties;
}

export const FormItem: React.FC<FormItemProps> = ({
  children,
  className,
  ...props
}) => (
  <AntForm.Item
    {...props}
    className={`ui-form-item ${className || ''}`}
  >
    {children}
  </AntForm.Item>
);

// Input Components
export interface InputProps extends AntInputProps {
  variant?: 'filled' | 'borderless' | 'outlined';
}

export const Input: React.FC<InputProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'filled':
        return {
          backgroundColor: 'var(--bg-secondary)',
          border: '1px solid transparent',
        };
      case 'borderless':
        return {
          border: 'none',
          boxShadow: 'none',
        };
      default:
        return {};
    }
  };

  return (
    <AntInput
      {...props}
      className={`ui-input ui-input-${variant} ${className || ''}`}
      style={{
        borderRadius: 'var(--radius-md)',
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Textarea Component
export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  variant?: 'default' | 'filled' | 'borderless';
}

export const Textarea: React.FC<TextareaProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'filled':
        return {
          backgroundColor: 'var(--bg-secondary)',
          border: '1px solid transparent',
        };
      case 'borderless':
        return {
          border: 'none',
          boxShadow: 'none',
        };
      default:
        return {};
    }
  };

  return (
    <TextArea
      {...props}
      className={`ui-textarea ui-textarea-${variant} ${className || ''}`}
      style={{
        borderRadius: 'var(--radius-md)',
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Select Component
export interface SelectProps extends AntSelectProps {
  variant?: 'filled' | 'borderless' | 'outlined';
}

export const Select: React.FC<SelectProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'filled':
        return {
          backgroundColor: 'var(--bg-secondary)',
        };
      case 'borderless':
        return {
          border: 'none',
          boxShadow: 'none',
        };
      default:
        return {};
    }
  };

  return (
    <AntSelect
      {...props}
      className={`ui-select ui-select-${variant} ${className || ''}`}
      style={{
        borderRadius: 'var(--radius-md)',
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Checkbox Component
export interface CheckboxProps extends AntCheckboxProps {
  variant?: 'default' | 'card';
}

export const Checkbox: React.FC<CheckboxProps> = ({ 
  variant = 'default',
  className,
  style,
  children,
  ...props 
}) => {
  if (variant === 'card') {
    return (
      <div
        className={`ui-checkbox-card ${className || ''}`}
        style={{
          padding: 'var(--space-3)',
          border: '1px solid var(--border-primary)',
          borderRadius: 'var(--radius-md)',
          cursor: 'pointer',
          transition: 'all var(--transition-fast)',
          ...style,
        }}
      >
        <AntCheckbox {...props}>
          {children}
        </AntCheckbox>
      </div>
    );
  }

  return (
    <AntCheckbox
      {...props}
      className={`ui-checkbox ${className || ''}`}
      style={style}
    >
      {children}
    </AntCheckbox>
  );
};

// Radio Component
export interface RadioProps extends AntRadioProps {
  variant?: 'default' | 'button' | 'card';
}

export const Radio: React.FC<RadioProps> = ({ 
  variant = 'default',
  className,
  style,
  children,
  ...props 
}) => {
  if (variant === 'button') {
    return (
      <AntRadio.Button
        {...props}
        className={`ui-radio-button ${className || ''}`}
        style={style}
      >
        {children}
      </AntRadio.Button>
    );
  }

  if (variant === 'card') {
    return (
      <div
        className={`ui-radio-card ${className || ''}`}
        style={{
          padding: 'var(--space-3)',
          border: '1px solid var(--border-primary)',
          borderRadius: 'var(--radius-md)',
          cursor: 'pointer',
          transition: 'all var(--transition-fast)',
          ...style,
        }}
      >
        <AntRadio {...props}>
          {children}
        </AntRadio>
      </div>
    );
  }

  return (
    <AntRadio
      {...props}
      className={`ui-radio ${className || ''}`}
      style={style}
    >
      {children}
    </AntRadio>
  );
};

// Switch Component
export interface SwitchProps extends AntSwitchProps {
  variant?: 'default' | 'small' | 'large';
}

export const Switch: React.FC<SwitchProps> = ({ 
  variant = 'default',
  className,
  ...props 
}) => {
  const getSize = () => {
    switch (variant) {
      case 'small': return 'small';
      case 'large': return 'default';
      default: return 'default';
    }
  };

  return (
    <AntSwitch
      {...props}
      size={getSize()}
      className={`ui-switch ui-switch-${variant} ${className || ''}`}
    />
  );
};

// Export additional form components
export { RadioGroup, CheckboxGroup, Option };

export default {
  Form,
  FormItem,
  Input,
  Textarea,
  Select,
  Checkbox,
  Radio,
  Switch,
  RadioGroup,
  CheckboxGroup,
  Option,
};
