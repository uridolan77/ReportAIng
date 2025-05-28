/**
 * Global Test Teardown
 * Runs once after all tests
 */

module.exports = async () => {
  // Clean up any global resources
  // This could include closing database connections, stopping servers, etc.
  
  // Restore console methods if they were mocked
  if (!process.env.DEBUG_TESTS) {
    console.log = console.log.mockRestore ? console.log.mockRestore() : console.log;
    console.info = console.info.mockRestore ? console.info.mockRestore() : console.info;
    console.debug = console.debug.mockRestore ? console.debug.mockRestore() : console.debug;
  }
};
