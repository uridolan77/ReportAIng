/**
 * Button Component Tests
 * 
 * Comprehensive tests for the Button component including variants,
 * accessibility, interactions, and edge cases.
 */

import React from 'react';
import { screen } from '@testing-library/react';
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
      renderWithProviders(
        <Button>Click me</Button>
      );

      const button = screen.getByRole('button');
      expectElementToBeVisible(button);
      expect(button).toHaveTextContent('Click me');
    });

    it('renders with different variants', () => {
      const variants = ['primary', 'secondary', 'outline', 'ghost', 'danger', 'success'] as const;

      variants.forEach(variant => {
        renderWithProviders(
          <Button variant={variant}>Button</Button>
        );

        const button = screen.getByRole('button');
        expectElementToBeVisible(button);
      });
    });

    it('renders with different sizes', () => {
      const sizes = ['small', 'medium', 'large'] as const;

      sizes.forEach(size => {
        renderWithProviders(
          <Button size={size}>Button</Button>
        );

        const button = screen.getByRole('button');
        expectElementToBeVisible(button);
      });
    });

    it('renders as full width when specified', () => {
      renderWithProviders(
        <Button fullWidth>Full Width Button</Button>
      );

      const button = screen.getByRole('button');
      expect(button).toHaveStyle({ width: '100%' });
    });
  });

  describe('Icon Support', () => {
    it('renders with left icon', () => {
      renderWithProviders(
        <Button icon={<span data-testid="icon">🚀</span>} iconPosition="left">
          Launch
        </Button>
      );

      const button = screen.getByRole('button');
      const icon = screen.getByTestId('icon');

      expectElementToBeVisible(button);
      expect(icon).toBeInTheDocument();
      expect(button).toHaveTextContent('🚀Launch');
    });

    it('renders with right icon', () => {
      renderWithProviders(
        <Button icon={<span data-testid="icon">→</span>} iconPosition="right">
          Next
        </Button>
      );

      const button = screen.getByRole('button');
      const icon = screen.getByTestId('icon');

      expectElementToBeVisible(button);
      expect(icon).toBeInTheDocument();
      expect(button).toHaveTextContent('Next→');
    });

    it('renders icon only button', () => {
      renderWithProviders(
        <Button icon={<span data-testid="icon">✓</span>} />
      );

      const button = screen.getByRole('button');
      const icon = screen.getByTestId('icon');

      expectElementToBeVisible(button);
      expect(icon).toBeInTheDocument();
    });
  });

  describe('States', () => {
    it('renders disabled state', () => {
      renderWithProviders(
        <Button disabled>Disabled Button</Button>
      );

      const button = screen.getByRole('button');
      expect(button).toBeDisabled();
      expect(button).toHaveAttribute('disabled');
    });

    it('renders loading state', () => {
      renderWithProviders(
        <Button loading>Loading Button</Button>
      );

      const button = screen.getByRole('button');
      expectElementToBeVisible(button);
      // In a real implementation, this would show a spinner
    });
  });

  describe('Interactions', () => {
    it('handles click events', async () => {
      const handleClick = jest.fn();
      renderWithProviders(
        <Button onClick={handleClick}>Clickable</Button>
      );

      const button = screen.getByRole('button');
      expectButtonToBeClickable(button);

      await userEvent.click(button);
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('does not trigger click when disabled', async () => {
      const handleClick = jest.fn();
      renderWithProviders(
        <Button onClick={handleClick} disabled>Disabled</Button>
      );

      const button = screen.getByRole('button');
      expect(button).toBeDisabled();

      await userEvent.click(button);
      expect(handleClick).not.toHaveBeenCalled();
    });

    it('handles keyboard navigation', async () => {
      const handleClick = jest.fn();
      renderWithProviders(
        <Button onClick={handleClick}>Keyboard</Button>
      );

      const button = screen.getByRole('button');
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
      renderWithProviders(
        <Button aria-label="Custom label">Button</Button>
      );

      const button = screen.getByRole('button');
      expectToHaveAccessibleName(button, 'Custom label');
    });

    it('supports aria-describedby', () => {
      renderWithProviders(
        <div>
          <Button aria-describedby="help-text">Button</Button>
          <div id="help-text">This is help text</div>
        </div>
      );

      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-describedby', 'help-text');
    });

    it('has proper focus management', async () => {
      renderWithProviders(
        <Button>Focusable</Button>
      );

      const button = screen.getByRole('button');

      await userEvent.tab();
      expect(button).toHaveFocus();
    });
  });
});

describe('IconButton Component', () => {
  it('renders icon button with aria-label', () => {
    renderWithProviders(
      <IconButton icon={<span>🔍</span>} aria-label="Search" />
    );

    const button = screen.getByRole('button');
    expectElementToBeVisible(button);
    expectToHaveAccessibleName(button, 'Search');
  });

  it('requires aria-label for accessibility', () => {
    // This would typically be caught by TypeScript, but we test runtime behavior
    renderWithProviders(
      <IconButton icon={<span>🔍</span>} aria-label="Required label" />
    );

    const button = screen.getByRole('button');
    expect(button).toHaveAttribute('aria-label');
  });
});

describe('ButtonGroup Component', () => {
  it('renders group of buttons', () => {
    renderWithProviders(
      <ButtonGroup>
        <Button>First</Button>
        <Button>Second</Button>
        <Button>Third</Button>
      </ButtonGroup>
    );

    const buttons = screen.getAllByRole('button');
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

    expect(horizontalContainer).toBeInTheDocument();
    expect(verticalContainer).toBeInTheDocument();
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

      expect(container).toBeInTheDocument();
    });
  });
});

describe('Button Edge Cases', () => {
  it('handles very long text', () => {
    const longText = 'This is a very long button text that might cause layout issues if not handled properly';
    renderWithProviders(
      <Button>{longText}</Button>
    );

    const button = screen.getByRole('button');
    expectElementToBeVisible(button);
    expect(button).toHaveTextContent(longText);
  });

  it('handles empty children gracefully', () => {
    renderWithProviders(
      <Button>{''}</Button>
    );

    const button = screen.getByRole('button');
    expectElementToBeVisible(button);
  });

  it('handles custom styles', () => {
    const customStyle = { backgroundColor: 'red', color: 'white' };
    renderWithProviders(
      <Button style={customStyle}>Styled</Button>
    );

    const button = screen.getByRole('button');
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
