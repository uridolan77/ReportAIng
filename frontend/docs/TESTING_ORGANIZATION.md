# Frontend Testing Organization

## Overview
Comprehensive testing structure for the BI Reporting Copilot frontend application.

## Test Structure

### Current Test Locations
```
frontend/src/
├── __tests__/                    # App-level integration tests
│   └── App.integration.test.tsx
├── components/
│   ├── __tests__/               # Centralized component tests
│   │   └── index.ts             # Test exports and utilities
│   ├── DataTable/
│   │   └── __tests__/           # DataTable-specific tests
│   │       └── DataTable.test.tsx
│   ├── QueryInterface/
│   │   └── __tests__/           # QueryInterface-specific tests
│   │       └── QueryShortcuts.test.tsx
│   └── [other components]/
└── test-utils/                  # Testing utilities
    ├── testing-providers.tsx   # Test providers and setup
    ├── component-test-utils.tsx # Component testing helpers
    ├── customMatchers.ts       # Custom Jest matchers
    ├── globalSetup.js          # Global test setup
    └── globalTeardown.js       # Global test teardown
```

## Test Categories

### 1. Unit Tests
- **Component rendering**
- **Props handling**
- **State management**
- **Event handling**
- **Utility functions**

### 2. Integration Tests
- **Component interactions**
- **Data flow**
- **API integration**
- **User workflows**

### 3. Performance Tests
- **Render performance**
- **Memory usage**
- **Virtual scrolling**
- **Large dataset handling**

### 4. Accessibility Tests
- **Keyboard navigation**
- **Screen reader compatibility**
- **ARIA attributes**
- **Color contrast**
- **Focus management**

## Test Configuration

### Jest Configuration
```javascript
// jest.config.js
module.exports = {
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: [
    '<rootDir>/src/setupTests.ts',
    '<rootDir>/src/test-utils/customMatchers.ts',
  ],
  collectCoverageFrom: [
    'src/**/*.{ts,tsx}',
    '!src/**/*.d.ts',
    '!src/**/__tests__/**',
    '!src/**/*.test.{ts,tsx}',
    '!src/test-utils/**',
  ],
  coverageThreshold: {
    global: {
      branches: 70,
      functions: 70,
      lines: 70,
      statements: 70,
    },
    './src/components/': {
      branches: 75,
      functions: 75,
      lines: 75,
      statements: 75,
    },
  },
};
```

## Test Utilities

### Custom Matchers
- `toBeValidQuery()` - Validates SQL query structure
- `toHaveValidTemplate()` - Validates query templates
- `toBeAccessible()` - Checks accessibility compliance
- `toHaveCorrectAriaAttributes()` - Validates ARIA attributes
- `toBeWithinPerformanceThreshold()` - Performance validation
- `toHaveValidSecuritySignature()` - Security validation

### Component Test Utils
- `QueryInterfaceTestUtils` - Query interface testing helpers
- `TemplateTestUtils` - Template testing utilities
- `VisualizationTestUtils` - Chart and visualization testing
- `TableTestUtils` - DataTable testing helpers
- `AccessibilityTestUtils` - Accessibility testing tools
- `PerformanceTestUtils` - Performance measurement tools

### Test Data Factories
- `createMockUser()` - Mock user data
- `createMockQuery()` - Mock query data
- `createMockData()` - Mock table data
- `createMockColumns()` - Mock column definitions

## Running Tests

### All Tests
```bash
npm test
```

### Specific Categories
```bash
# Unit tests only
npm test -- --testPathPattern="__tests__"

# Integration tests
npm test -- --testNamePattern="integration"

# Performance tests
npm test -- --testNamePattern="performance"

# Accessibility tests
npm test -- --testNamePattern="accessibility"
```

### Specific Components
```bash
# DataTable tests
npm test DataTable

# QueryInterface tests
npm test QueryInterface

# Visualization tests
npm test Visualization
```

### Coverage Reports
```bash
# Generate coverage report
npm test -- --coverage

# Coverage with threshold enforcement
npm test -- --coverage --coverageThreshold
```

## Test Best Practices

### 1. Test Organization
- Group related tests in describe blocks
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Keep tests focused and isolated

### 2. Component Testing
- Test component behavior, not implementation
- Use semantic queries (getByRole, getByLabelText)
- Test user interactions
- Verify accessibility

### 3. Performance Testing
- Measure render times
- Test with large datasets
- Monitor memory usage
- Validate virtual scrolling

### 4. Accessibility Testing
- Test keyboard navigation
- Verify screen reader compatibility
- Check color contrast
- Validate ARIA attributes

## Test Maintenance

### Regular Tasks
- Update test data when schemas change
- Review and update coverage thresholds
- Maintain test utilities and helpers
- Update mocks when APIs change

### Continuous Integration
- Run tests on every commit
- Enforce coverage thresholds
- Generate test reports
- Monitor test performance

## Future Enhancements

### Planned Improvements
- Visual regression testing
- End-to-end test automation
- Performance benchmarking
- Cross-browser testing
- Mobile responsiveness testing

### Tools to Consider
- Playwright for E2E testing
- Storybook for component testing
- Lighthouse for performance auditing
- axe-core for accessibility testing
