import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider } from '../../contexts/ThemeContext';
import { ThemeToggle } from './ThemeToggle';

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // deprecated
    removeListener: jest.fn(), // deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

const ThemeToggleWithProvider: React.FC<{ variant?: 'button' | 'icon' | 'compact' }> = ({ variant }) => (
  <ThemeProvider>
    <ThemeToggle variant={variant} />
  </ThemeProvider>
);

describe('ThemeToggle', () => {
  beforeEach(() => {
    localStorageMock.getItem.mockClear();
    localStorageMock.setItem.mockClear();
  });

  it('renders theme toggle button', () => {
    render(<ThemeToggleWithProvider />);
    expect(screen.getByRole('button')).toBeInTheDocument();
  });

  it('renders icon variant', () => {
    render(<ThemeToggleWithProvider variant="icon" />);
    const button = screen.getByRole('button');
    expect(button).toBeInTheDocument();
    expect(button).toHaveClass('theme-toggle-icon');
  });

  it('renders compact variant', () => {
    render(<ThemeToggleWithProvider variant="compact" />);
    const button = screen.getByRole('button');
    expect(button).toBeInTheDocument();
    expect(button).toHaveClass('theme-toggle-compact');
  });

  it('cycles through theme modes when clicked', () => {
    render(<ThemeToggleWithProvider />);
    const button = screen.getByRole('button');
    
    // Initial state should be light mode
    expect(button).toBeInTheDocument();
    
    // Click to go to dark mode
    fireEvent.click(button);
    
    // Click to go to auto mode
    fireEvent.click(button);
    
    // Click to go back to light mode
    fireEvent.click(button);
    
    // Verify localStorage was called
    expect(localStorageMock.setItem).toHaveBeenCalled();
  });

  it('applies correct CSS classes based on theme', () => {
    render(<ThemeToggleWithProvider />);
    const button = screen.getByRole('button');
    
    // Should have light class initially
    expect(button).toHaveClass('light');
  });
});
