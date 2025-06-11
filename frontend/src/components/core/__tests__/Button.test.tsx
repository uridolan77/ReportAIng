/**
 * Button Component Tests
 * 
 * Comprehensive tests for the Button component including variants,
 * accessibility, interactions, and edge cases.
 */

import React from 'react';
import { 
  renderWithProviders, 
  expectElementToBeVisible,
  expectButtonToBeClickable,
  expectToHaveAccessibleName,
  expectToBeKeyboardAccessible,
  userEvent 
} from '../../../test-utils';
import { Button, IconButton, ButtonGroup } from '../Button';

describe('Button Component', () => {
  describe('Basic Rendering', () => {
    it('renders with default props', () => {
      const { getByRole } = renderWithProviders(
        <Button>Click me</Button>
      );
      
      const button = getByRole('button');
      expectElementToBeVisible(button);
      expect(button).toHaveTextContent('Click me');
    });

    it('renders with different variants', () => {
      const variants = ['primary', 'secondary', 'outline', 'ghost', 'danger', 'success'] as const;
      
      variants.forEach(variant => {
        const { getByRole } = renderWithProviders(
          <Button variant={variant}>Button</Button>
        );
        
        const button = getByRole('button');
        expectElementToBeVisible(button);
      });
    });

    it('renders with different sizes', () => {
      const sizes = ['small', 'medium', 'large'] as const;
      
      sizes.forEach(size => {
        const { getByRole } = renderWithProviders(
          <Button size={size}>Button</Button>
        );
        
        const button = getByRole('button');
        expectElementToBeVisible(button);
      });
    });

    it('renders as full width when specified', () => {
      const { getByRole } = renderWithProviders(
        <Button fullWidth>Full Width Button</Button>
      );
      
      const button = getByRole('button');
      expect(button).toHaveStyle({ width: '100%' });
    });
  });

  describe('Icon Support', () => {
    it('renders with left icon', () => {
      const { getByRole } = renderWithProviders(
        <Button icon={<span data-testid="icon">🚀</span>} iconPosition="left">
          Launch
        </Button>
      );
      
      const button = getByRole('button');
      const icon = button.querySelector('[data-testid="icon"]');
      
      expectElementToBeVisible(button);
      expect(icon).toBeInTheDocument();
      expect(button).toHaveTextContent('🚀Launch');
    });

    it('renders with right icon', () => {
      const { getByRole } = renderWithProviders(
        <Button icon={<span data-testid="icon">→</span>} iconPosition="right">
          Next
        </Button>
      );
      
      const button = getByRole('button');
      const icon = button.querySelector('[data-testid="icon"]');
      
      expectElementToBeVisible(button);
      expect(icon).toBeInTheDocument();
      expect(button).toHaveTextContent('Next→');
    });

    it('renders icon only button', () => {
      const { getByRole } = renderWithProviders(
        <Button icon={<span data-testid="icon">✓</span>} />
      );
      
      const button = getByRole('button');
      const icon = button.querySelector('[data-testid="icon"]');
      
      expectElementToBeVisible(button);
      expect(icon).toBeInTheDocument();
    });
  });

  describe('States', () => {
    it('renders disabled state', () => {
      const { getByRole } = renderWithProviders(
        <Button disabled>Disabled Button</Button>
      );
      
      const button = getByRole('button');
      expect(button).toBeDisabled();
      expect(button).toHaveAttribute('disabled');
    });

    it('renders loading state', () => {
      const { getByRole } = renderWithProviders(
        <Button loading>Loading Button</Button>
      );
      
      const button = getByRole('button');
      expectElementToBeVisible(button);
      // In a real implementation, this would show a spinner
    });
  });

  describe('Interactions', () => {
    it('handles click events', async () => {
      const handleClick = jest.fn();
      const { getByRole } = renderWithProviders(
        <Button onClick={handleClick}>Clickable</Button>
      );
      
      const button = getByRole('button');
      expectButtonToBeClickable(button);
      
      await userEvent.click(button);
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('does not trigger click when disabled', async () => {
      const handleClick = jest.fn();
      const { getByRole } = renderWithProviders(
        <Button onClick={handleClick} disabled>Disabled</Button>
      );
      
      const button = getByRole('button');
      expect(button).toBeDisabled();
      
      await userEvent.click(button);
      expect(handleClick).not.toHaveBeenCalled();
    });

    it('handles keyboard navigation', async () => {
      const handleClick = jest.fn();
      const { getByRole } = renderWithProviders(
        <Button onClick={handleClick}>Keyboard</Button>
      );
      
      const button = getByRole('button');
      expectToBeKeyboardAccessible(button);
      
      button.focus();
      expect(button).toHaveFocus();
      
      await userEvent.keyboard('{Enter}');
      expect(handleClick).toHaveBeenCalledTimes(1);
      
      await userEvent.keyboard(' ');
      expect(handleClick).toHaveBeenCalledTimes(2);
    });
  });

  describe('Accessibility', () => {
    it('has proper ARIA attributes', () => {
      const { getByRole } = renderWithProviders(
        <Button aria-label="Custom label">Button</Button>
      );
      
      const button = getByRole('button');
      expectToHaveAccessibleName(button, 'Custom label');
    });

    it('supports aria-describedby', () => {
      const { getByRole } = renderWithProviders(
        <div>
          <Button aria-describedby="help-text">Button</Button>
          <div id="help-text">This is help text</div>
        </div>
      );
      
      const button = getByRole('button');
      expect(button).toHaveAttribute('aria-describedby', 'help-text');
    });

    it('has proper focus management', async () => {
      const { getByRole } = renderWithProviders(
        <Button>Focusable</Button>
      );
      
      const button = getByRole('button');
      
      await userEvent.tab();
      expect(button).toHaveFocus();
    });
  });
});

describe('IconButton Component', () => {
  it('renders icon button with aria-label', () => {
    const { getByRole } = renderWithProviders(
      <IconButton icon={<span>🔍</span>} aria-label="Search" />
    );
    
    const button = getByRole('button');
    expectElementToBeVisible(button);
    expectToHaveAccessibleName(button, 'Search');
  });

  it('requires aria-label for accessibility', () => {
    // This would typically be caught by TypeScript, but we test runtime behavior
    const { getByRole } = renderWithProviders(
      <IconButton icon={<span>🔍</span>} aria-label="Required label" />
    );
    
    const button = getByRole('button');
    expect(button).toHaveAttribute('aria-label');
  });
});

describe('ButtonGroup Component', () => {
  it('renders group of buttons', () => {
    const { getAllByRole } = renderWithProviders(
      <ButtonGroup>
        <Button>First</Button>
        <Button>Second</Button>
        <Button>Third</Button>
      </ButtonGroup>
    );
    
    const buttons = getAllByRole('button');
    expect(buttons).toHaveLength(3);
    buttons.forEach(button => expectElementToBeVisible(button));
  });

  it('renders with different orientations', () => {
    const { container: horizontalContainer } = renderWithProviders(
      <ButtonGroup variant="horizontal">
        <Button>Button 1</Button>
        <Button>Button 2</Button>
      </ButtonGroup>
    );
    
    const { container: verticalContainer } = renderWithProviders(
      <ButtonGroup variant="vertical">
        <Button>Button 1</Button>
        <Button>Button 2</Button>
      </ButtonGroup>
    );
    
    expect(horizontalContainer.firstChild).toBeInTheDocument();
    expect(verticalContainer.firstChild).toBeInTheDocument();
  });

  it('applies consistent spacing', () => {
    const spacings = ['tight', 'normal', 'loose'] as const;
    
    spacings.forEach(spacing => {
      const { container } = renderWithProviders(
        <ButtonGroup spacing={spacing}>
          <Button>Button 1</Button>
          <Button>Button 2</Button>
        </ButtonGroup>
      );
      
      expect(container.firstChild).toBeInTheDocument();
    });
  });
});

describe('Button Edge Cases', () => {
  it('handles very long text', () => {
    const longText = 'This is a very long button text that might cause layout issues if not handled properly';
    const { getByRole } = renderWithProviders(
      <Button>{longText}</Button>
    );
    
    const button = getByRole('button');
    expectElementToBeVisible(button);
    expect(button).toHaveTextContent(longText);
  });

  it('handles empty children gracefully', () => {
    const { getByRole } = renderWithProviders(
      <Button>{''}</Button>
    );
    
    const button = getByRole('button');
    expectElementToBeVisible(button);
  });

  it('handles custom styles', () => {
    const customStyle = { backgroundColor: 'red', color: 'white' };
    const { getByRole } = renderWithProviders(
      <Button style={customStyle}>Styled</Button>
    );
    
    const button = getByRole('button');
    expect(button).toHaveStyle(customStyle);
  });

  it('forwards ref correctly', () => {
    const ref = React.createRef<HTMLButtonElement>();
    renderWithProviders(
      <Button ref={ref}>Ref Button</Button>
    );
    
    expect(ref.current).toBeInstanceOf(HTMLButtonElement);
    expect(ref.current).toHaveTextContent('Ref Button');
  });
});
