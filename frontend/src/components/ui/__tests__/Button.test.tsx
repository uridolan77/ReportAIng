import React from 'react';
import { render, screen, fireEvent } from '../../../test-utils';
import { Button } from '../index';

describe('Button Component', () => {
  it('renders with default props', () => {
    render(<Button>Click me</Button>);
    const button = screen.getByRole('button', { name: /click me/i });
    expect(button).toBeInTheDocument();
  });

  it('renders with primary variant', () => {
    render(<Button variant="primary">Primary Button</Button>);
    const button = screen.getByRole('button', { name: /primary button/i });
    expect(button).toBeInTheDocument();
    expect(button).toHaveClass('ant-btn-primary');
  });

  it('renders with secondary variant', () => {
    render(<Button variant="secondary">Secondary Button</Button>);
    const button = screen.getByRole('button', { name: /secondary button/i });
    expect(button).toBeInTheDocument();
  });

  it('renders with danger variant', () => {
    render(<Button variant="danger">Danger Button</Button>);
    const button = screen.getByRole('button', { name: /danger button/i });
    expect(button).toBeInTheDocument();
  });

  it('handles click events', () => {
    const handleClick = jest.fn();
    render(<Button onClick={handleClick}>Clickable</Button>);
    
    const button = screen.getByRole('button', { name: /clickable/i });
    fireEvent.click(button);
    
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('can be disabled', () => {
    render(<Button disabled>Disabled Button</Button>);
    const button = screen.getByRole('button', { name: /disabled button/i });
    expect(button).toBeDisabled();
  });

  it('supports loading state', () => {
    render(<Button loading>Loading Button</Button>);
    const button = screen.getByRole('button');
    expect(button).toHaveClass('ant-btn-loading');
  });

  it('supports different sizes', () => {
    render(<Button size="large">Large Button</Button>);
    const button = screen.getByRole('button', { name: /large button/i });
    expect(button).toHaveClass('ant-btn-lg');
  });

  it('supports icon prop', () => {
    const TestIcon = () => <span data-testid="test-icon">Icon</span>;
    render(<Button icon={<TestIcon />}>Button with Icon</Button>);
    
    expect(screen.getByTestId('test-icon')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /button with icon/i })).toBeInTheDocument();
  });

  it('forwards ref correctly', () => {
    const ref = React.createRef<HTMLButtonElement>();
    render(<Button ref={ref}>Ref Button</Button>);
    
    expect(ref.current).toBeInstanceOf(HTMLButtonElement);
  });

  it('applies custom className', () => {
    render(<Button className="custom-class">Custom Button</Button>);
    const button = screen.getByRole('button', { name: /custom button/i });
    expect(button).toHaveClass('custom-class');
  });

  it('supports custom styles', () => {
    const customStyle = { backgroundColor: 'red' };
    render(<Button style={customStyle}>Styled Button</Button>);
    const button = screen.getByRole('button', { name: /styled button/i });
    expect(button).toHaveStyle('background-color: red');
  });
});
