/**
 * Global Test Setup
 * Runs once before all tests
 */

module.exports = async () => {
  // Set test environment variables
  process.env.NODE_ENV = 'test';
  process.env.REACT_APP_API_BASE_URL = 'http://localhost:55243/api';
  process.env.REACT_APP_ENVIRONMENT = 'test';
  
  // Suppress console output during tests
  if (!process.env.DEBUG_TESTS) {
    console.log = jest.fn();
    console.info = jest.fn();
    console.debug = jest.fn();
  }
  
  // Set timezone for consistent date testing
  process.env.TZ = 'UTC';
};
