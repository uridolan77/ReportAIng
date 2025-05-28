/**
 * Test Setup Configuration
 * Global test setup and configuration for Jest and React Testing Library
 */

import '@testing-library/jest-dom';
import { setupTestEnvironment } from './test-utils/testing-providers';
import { ApiTestUtils } from './test-utils/api-test-utils';

// Setup global test environment
setupTestEnvironment();

// Setup API mocking
ApiTestUtils.setupApiMocking();

// Polyfill for TextEncoder/TextDecoder
import { TextEncoder, TextDecoder } from 'util';
global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder as any;



// Mock ResizeObserver
global.ResizeObserver = jest.fn().mockImplementation(() => ({
  observe: jest.fn(),
  unobserve: jest.fn(),
  disconnect: jest.fn(),
}));

// Establish API mocking before all tests
// beforeAll(() => {
//   server.listen();
// });

// Reset any request handlers that we may add during the tests,
// so they don't affect other tests
// afterEach(() => {
//   server.resetHandlers();
// });

// Clean up after the tests are finished
// afterAll(() => {
//   server.close();
// });

// Mock window.matchMedia
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

// Mock ResizeObserver
global.ResizeObserver = jest.fn().mockImplementation(() => ({
  observe: jest.fn(),
  unobserve: jest.fn(),
  disconnect: jest.fn(),
}));

// Mock IntersectionObserver
global.IntersectionObserver = jest.fn().mockImplementation(() => ({
  observe: jest.fn(),
  unobserve: jest.fn(),
  disconnect: jest.fn(),
}));

// Mock requestIdleCallback
global.requestIdleCallback = jest.fn((cb) => setTimeout(cb, 0));
global.cancelIdleCallback = jest.fn();

// Mock Web Crypto API
Object.defineProperty(global, 'crypto', {
  value: {
    subtle: {
      encrypt: jest.fn(),
      decrypt: jest.fn(),
    },
    getRandomValues: jest.fn(() => new Uint32Array(1)),
  },
});

// Suppress console errors during tests unless explicitly needed
const originalError = console.error;
beforeAll(() => {
  console.error = (...args: any[]) => {
    if (
      typeof args[0] === 'string' &&
      args[0].includes('Warning: ReactDOM.render is no longer supported')
    ) {
      return;
    }
    originalError.call(console, ...args);
  };
});

afterAll(() => {
  console.error = originalError;
});
