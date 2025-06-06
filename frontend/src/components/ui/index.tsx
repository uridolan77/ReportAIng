import React from 'react';
import {
  Button as AntButton,
  ButtonProps as AntButtonProps,
  Card as AntCard,
  CardProps as AntCardProps,
  Input as AntInput,
  InputProps as AntInputProps,
  Badge as AntBadge,
  BadgeProps as AntBadgeProps,
  Tabs as AntTabs,
  TabsProps as AntTabsProps,
  Switch as AntSwitch,
  SwitchProps as AntSwitchProps,
  Modal,
  Collapse
} from 'antd';

const { TextArea } = AntInput;

// Unified Button component
export interface ButtonProps extends Omit<AntButtonProps, 'variant'> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger' | 'outline' | 'default';
}

export const Button: React.FC<ButtonProps> = ({ variant, style, ...props }) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'secondary':
        return {
          background: '#f0f0f0',
          borderColor: '#d9d9d9',
          color: '#262626',
        };
      case 'ghost':
        return {
          background: 'transparent',
          borderColor: 'transparent',
        };
      case 'danger':
        return {
          background: '#ff4d4f',
          borderColor: '#ff4d4f',
          color: 'white',
        };
      case 'outline':
        return {
          background: 'transparent',
          borderColor: '#d9d9d9',
          color: '#262626',
        };
      case 'default':
        return {
          background: '#1890ff',
          borderColor: '#1890ff',
          color: 'white',
        };
      default:
        return {};
    }
  };

  return (
    <AntButton
      {...props}
      style={{
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Card components
export interface CardProps extends AntCardProps {
  children: React.ReactNode;
}

export const Card: React.FC<CardProps> = ({ children, style, ...props }) => (
  <AntCard
    {...props}
    style={{
      background: 'white',
      borderRadius: 8,
      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
      marginBottom: 16,
      ...style,
    }}
  >
    {children}
  </AntCard>
);

export const CardContent: React.FC<{ children: React.ReactNode; className?: string; style?: React.CSSProperties }> = ({
  children,
  className,
  style
}) => (
  <div className={className} style={{ padding: '16px', ...style }}>
    {children}
  </div>
);

export const CardHeader: React.FC<{ children: React.ReactNode; className?: string; style?: React.CSSProperties }> = ({
  children,
  className,
  style
}) => (
  <div className={className} style={{ padding: '16px 16px 0 16px', borderBottom: '1px solid #f0f0f0', marginBottom: '16px', ...style }}>
    {children}
  </div>
);

export const CardTitle: React.FC<{ children: React.ReactNode; className?: string; style?: React.CSSProperties }> = ({
  children,
  className,
  style
}) => (
  <h3 className={className} style={{ margin: 0, fontSize: '16px', fontWeight: 600, ...style }}>
    {children}
  </h3>
);

// Input components
export interface InputProps extends AntInputProps {}

export const Input: React.FC<InputProps> = (props) => (
  <AntInput {...props} />
);

export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {}

export const Textarea: React.FC<TextareaProps> = (props) => (
  <TextArea {...props} />
);

// Badge component
export interface BadgeProps extends AntBadgeProps {
  variant?: 'default' | 'secondary' | 'outline';
}

export const Badge: React.FC<BadgeProps> = ({ variant, style, children, ...props }) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'secondary':
        return {
          background: '#f0f0f0',
          color: '#262626',
          border: 'none',
        };
      case 'outline':
        return {
          background: 'transparent',
          color: '#262626',
          border: '1px solid #d9d9d9',
        };
      default:
        return {
          background: '#1890ff',
          color: 'white',
        };
    }
  };

  if (children) {
    return (
      <span style={{
        display: 'inline-flex',
        alignItems: 'center',
        padding: '2px 8px',
        borderRadius: 4,
        fontSize: '12px',
        fontWeight: 500,
        ...getVariantStyle(),
        ...style,
      }}>
        {children}
      </span>
    );
  }

  return <AntBadge {...props} style={style} />;
};

// Tabs components
export interface TabsProps extends AntTabsProps {}

export const Tabs: React.FC<TabsProps> = (props) => (
  <AntTabs {...props} />
);

export const TabsContent: React.FC<{
  children: React.ReactNode;
  value: string;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, value, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const TabsList: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{ display: 'flex', borderBottom: '1px solid #f0f0f0', marginBottom: '16px', ...style }}>
    {children}
  </div>
);

export const TabsTrigger: React.FC<{
  children: React.ReactNode;
  value: string;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, value, className, style }) => (
  <button className={className} style={{
    padding: '8px 16px',
    border: 'none',
    background: 'transparent',
    cursor: 'pointer',
    borderBottom: '2px solid transparent',
    ...style
  }}>
    {children}
  </button>
);

// Switch component
export interface SwitchProps extends AntSwitchProps {}

export const Switch: React.FC<SwitchProps> = (props) => (
  <AntSwitch {...props} />
);

// Label component
export const Label: React.FC<{
  children: React.ReactNode;
  htmlFor?: string;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, htmlFor, className, style }) => (
  <label htmlFor={htmlFor} className={className} style={{
    display: 'block',
    marginBottom: '4px',
    fontSize: '14px',
    fontWeight: 500,
    color: '#262626',
    ...style
  }}>
    {children}
  </label>
);

// Dialog components (using Ant Design Modal)

export interface DialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
}

export const Dialog: React.FC<DialogProps> = ({ open, onOpenChange, children }) => (
  <Modal
    open={open}
    onCancel={() => onOpenChange(false)}
    footer={null}
    destroyOnClose
  >
    {children}
  </Modal>
);

export const DialogContent: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const DialogHeader: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{ marginBottom: '16px', ...style }}>
    {children}
  </div>
);

export const DialogTitle: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <h2 className={className} style={{ margin: 0, fontSize: '18px', fontWeight: 600, ...style }}>
    {children}
  </h2>
);

export const DialogDescription: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <p className={className} style={{ margin: '8px 0 0 0', color: '#8c8c8c', fontSize: '14px', ...style }}>
    {children}
  </p>
);

export const DialogFooter: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '8px',
    marginTop: '24px',
    paddingTop: '16px',
    borderTop: '1px solid #f0f0f0',
    ...style
  }}>
    {children}
  </div>
);

// Accordion components (using Ant Design Collapse)

export const Accordion: React.FC<{
  children: React.ReactNode;
  type?: 'single' | 'multiple';
  collapsible?: boolean;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, type, collapsible, className, style }) => (
  <Collapse
    accordion={type === 'single'}
    className={className}
    style={style}
  >
    {children}
  </Collapse>
);

export const AccordionItem: React.FC<{
  children: React.ReactNode;
  value: string;
  key?: string;
}> = ({ children, value, key }) => (
  <Collapse.Panel header="" key={key || value}>
    {children}
  </Collapse.Panel>
);

export const AccordionTrigger: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const AccordionContent: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

// Alert Dialog components (using Ant Design Modal)
export interface AlertDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
}

export const AlertDialog: React.FC<AlertDialogProps> = ({ open, onOpenChange, children }) => (
  <Modal
    open={open}
    onCancel={() => onOpenChange(false)}
    footer={null}
    destroyOnClose
    centered
  >
    {children}
  </Modal>
);

export const AlertDialogContent: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const AlertDialogHeader: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{ marginBottom: '16px', ...style }}>
    {children}
  </div>
);

export const AlertDialogTitle: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <h2 className={className} style={{ margin: 0, fontSize: '18px', fontWeight: 600, color: '#ff4d4f', ...style }}>
    {children}
  </h2>
);

export const AlertDialogDescription: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <p className={className} style={{ margin: '8px 0 0 0', color: '#8c8c8c', fontSize: '14px', ...style }}>
    {children}
  </p>
);

export const AlertDialogFooter: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '8px',
    marginTop: '24px',
    paddingTop: '16px',
    borderTop: '1px solid #f0f0f0',
    ...style
  }}>
    {children}
  </div>
);

export const AlertDialogAction: React.FC<ButtonProps> = (props) => (
  <Button {...props} />
);

export const AlertDialogCancel: React.FC<ButtonProps> = (props) => (
  <Button variant="outline" {...props} />
);

// Dropdown Menu components (using Ant Design Dropdown)

export const DropdownMenu: React.FC<{
  children: React.ReactNode;
}> = ({ children }) => (
  <div>{children}</div>
);

export const DropdownMenuTrigger: React.FC<{
  children: React.ReactNode;
  asChild?: boolean;
}> = ({ children, asChild }) => (
  <div>{children}</div>
);

export const DropdownMenuContent: React.FC<{
  children: React.ReactNode;
  align?: 'start' | 'center' | 'end';
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, align, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const DropdownMenuItem: React.FC<{
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, onClick, disabled, className, style }) => (
  <div
    className={className}
    style={{
      padding: '8px 12px',
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.5 : 1,
      ...style
    }}
    onClick={disabled ? undefined : onClick}
  >
    {children}
  </div>
);

export const DropdownMenuSeparator: React.FC<{
  className?: string;
  style?: React.CSSProperties;
}> = ({ className, style }) => (
  <div className={className} style={{ height: '1px', background: '#f0f0f0', margin: '4px 0', ...style }} />
);

// Loading Fallback Component
export const LoadingFallback: React.FC = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '200px'
  }}>
    <div>Loading...</div>
  </div>
);



// Common spacing utilities
interface SpacerProps {
  size?: 'small' | 'medium' | 'large';
}

export const Spacer: React.FC<SpacerProps> = ({ size = 'medium' }) => {
  const getHeight = () => {
    switch (size) {
      case 'small': return 8;
      case 'large': return 32;
      default: return 16;
    }
  };

  return <div style={{ height: getHeight() }} />;
};

// Flex utilities
interface FlexContainerProps {
  children: React.ReactNode;
  direction?: 'row' | 'column';
  justify?: 'flex-start' | 'center' | 'flex-end' | 'space-between' | 'space-around';
  align?: 'flex-start' | 'center' | 'flex-end' | 'stretch';
  gap?: string;
  style?: React.CSSProperties;
}

export const FlexContainer: React.FC<FlexContainerProps> = ({
  children,
  direction = 'row',
  justify = 'flex-start',
  align = 'flex-start',
  gap = '0',
  style
}) => (
  <div style={{
    display: 'flex',
    flexDirection: direction,
    justifyContent: justify,
    alignItems: align,
    gap,
    ...style,
  }}>
    {children}
  </div>
);

// Typography
interface TitleProps {
  children: React.ReactNode;
  level?: 1 | 2 | 3 | 4 | 5;
  style?: React.CSSProperties;
}

export const Title: React.FC<TitleProps> = ({ children, level = 2, style }) => {
  const getFontSize = () => {
    switch (level) {
      case 1: return '2.5rem';
      case 2: return '2rem';
      case 3: return '1.5rem';
      case 4: return '1.25rem';
      case 5: return '1rem';
      default: return '2rem';
    }
  };

  const Tag = `h${level}` as keyof JSX.IntrinsicElements;

  return (
    <Tag style={{
      fontSize: getFontSize(),
      fontWeight: 600,
      margin: '0 0 16px 0',
      color: '#262626',
      ...style,
    }}>
      {children}
    </Tag>
  );
};

interface TextProps {
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large';
  weight?: 'normal' | 'medium' | 'bold';
  color?: string;
  style?: React.CSSProperties;
}

export const Text: React.FC<TextProps> = ({
  children,
  size = 'medium',
  weight = 'normal',
  color = '#595959',
  style
}) => {
  const getFontSize = () => {
    switch (size) {
      case 'small': return '0.875rem';
      case 'large': return '1.125rem';
      default: return '1rem';
    }
  };

  const getFontWeight = () => {
    switch (weight) {
      case 'medium': return '500';
      case 'bold': return '600';
      default: return '400';
    }
  };

  return (
    <p style={{
      fontSize: getFontSize(),
      fontWeight: getFontWeight(),
      color,
      margin: '0 0 8px 0',
      lineHeight: 1.5,
      ...style,
    }}>
      {children}
    </p>
  );
};

// Status indicators
interface StatusBadgeProps {
  children: React.ReactNode;
  status: 'success' | 'warning' | 'error' | 'info' | 'default';
  style?: React.CSSProperties;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ children, status, style }) => {
  const getStatusStyle = () => {
    switch (status) {
      case 'success':
        return {
          background: '#f6ffed',
          color: '#52c41a',
          border: '1px solid #b7eb8f',
        };
      case 'warning':
        return {
          background: '#fffbe6',
          color: '#faad14',
          border: '1px solid #ffe58f',
        };
      case 'error':
        return {
          background: '#fff2f0',
          color: '#ff4d4f',
          border: '1px solid #ffccc7',
        };
      case 'info':
        return {
          background: '#f0f5ff',
          color: '#1890ff',
          border: '1px solid #adc6ff',
        };
      default:
        return {
          background: '#fafafa',
          color: '#8c8c8c',
          border: '1px solid #d9d9d9',
        };
    }
  };

  return (
    <span style={{
      display: 'inline-flex',
      alignItems: 'center',
      padding: '4px 8px',
      borderRadius: 4,
      fontSize: '0.75rem',
      fontWeight: 500,
      textTransform: 'uppercase',
      letterSpacing: '0.5px',
      ...getStatusStyle(),
      ...style,
    }}>
      {children}
    </span>
  );
};

// Grid system
interface GridProps {
  children: React.ReactNode;
  columns?: number;
  gap?: string;
  responsive?: boolean;
  style?: React.CSSProperties;
}

export const Grid: React.FC<GridProps> = ({
  children,
  columns = 1,
  gap = '16px',
  responsive = false,
  style
}) => (
  <div style={{
    display: 'grid',
    gridTemplateColumns: `repeat(${columns}, 1fr)`,
    gap,
    ...(responsive && {
      '@media (max-width: 768px)': {
        gridTemplateColumns: '1fr',
      }
    }),
    ...style,
  }}>
    {children}
  </div>
);

// Common animations
interface AnimatedContainerProps {
  children: React.ReactNode;
  animation?: 'fadeIn' | 'slideIn';
  style?: React.CSSProperties;
}

export const AnimatedContainer: React.FC<AnimatedContainerProps> = ({
  children,
  animation,
  style
}) => {
  const getAnimationStyle = () => {
    switch (animation) {
      case 'fadeIn':
        return {
          animation: 'fadeIn 0.3s ease-in-out',
        };
      case 'slideIn':
        return {
          animation: 'slideIn 0.3s ease-in-out',
        };
      default:
        return {};
    }
  };

  return (
    <div style={{
      ...getAnimationStyle(),
      ...style,
    }}>
      {children}
    </div>
  );
};
