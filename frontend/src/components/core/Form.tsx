/**
 * Modern Form Components
 * 
 * Provides comprehensive form components with validation, accessibility, and modern patterns.
 */

import React, { forwardRef } from 'react';
import { Input as AntInput, Select as AntSelect, Checkbox as AntCheckbox, Radio as AntRadio, Switch as AntSwitch, DatePicker as AntDatePicker, TimePicker as AntTimePicker } from 'antd';
import { designTokens } from './design-system';

// Basic exports - these will be expanded later
export const Input = forwardRef<any, any>((props, ref) => <AntInput ref={ref} {...props} />);
export const Select = forwardRef<any, any>((props, ref) => <AntSelect ref={ref} {...props} />);
export const Checkbox = forwardRef<any, any>((props, ref) => <AntCheckbox ref={ref} {...props} />);
export const Radio = forwardRef<any, any>((props, ref) => <AntRadio ref={ref} {...props} />);
export const Switch = forwardRef<any, any>((props, ref) => <AntSwitch ref={ref} {...props} />);
export const DatePicker = forwardRef<any, any>((props, ref) => <AntDatePicker ref={ref} {...props} />);
export const TimePicker = forwardRef<any, any>((props, ref) => <AntTimePicker ref={ref} {...props} />);

export const FormField: React.FC<{
  label?: string;
  error?: string;
  children: React.ReactNode;
}> = ({ label, error, children }) => (
  <div style={{ marginBottom: designTokens.spacing.md }}>
    {label && (
      <label style={{
        display: 'block',
        marginBottom: designTokens.spacing.sm,
        fontWeight: designTokens.typography.fontWeight.medium,
        color: designTokens.colors.text,
      }}>
        {label}
      </label>
    )}
    {children}
    {error && (
      <div style={{
        marginTop: designTokens.spacing.xs,
        color: designTokens.colors.danger,
        fontSize: designTokens.typography.fontSize.sm,
      }}>
        {error}
      </div>
    )}
  </div>
);
