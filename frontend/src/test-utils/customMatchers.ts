/**
 * Custom Jest Matchers
 * Additional matchers for testing React components and business logic
 */

import { MatcherFunction } from 'expect';

// Extend Jest matchers
declare global {
  namespace jest {
    interface Matchers<R> {
      toBeValidQuery(): R;
      toHaveValidTemplate(): R;
      toBeAccessible(): R;
      toHaveCorrectAriaAttributes(): R;
      toBeWithinPerformanceThreshold(threshold: number): R;
      toHaveValidSecuritySignature(): R;
      toMatchQueryPattern(pattern: string): R;
      toBeValidBusinessData(): R;
    }
  }
}

// Custom matcher for query validation
const toBeValidQuery: MatcherFunction<[query: string]> = function (received) {
  const pass = typeof received === 'string' && 
               received.trim().length > 0 && 
               !received.includes('DROP') && 
               !received.includes('DELETE') &&
               !received.includes('UPDATE') &&
               !received.includes('INSERT');

  return {
    message: () => 
      pass 
        ? `Expected query not to be valid, but it was: ${received}`
        : `Expected query to be valid, but it was invalid: ${received}`,
    pass,
  };
};

// Custom matcher for template validation
const toHaveValidTemplate: MatcherFunction<[]> = function (received) {
  const template = received as any;
  const hasRequiredFields = template &&
                           typeof template.id === 'string' &&
                           typeof template.name === 'string' &&
                           typeof template.template === 'string' &&
                           Array.isArray(template.variables);

  const hasValidVariables = template.variables.every((variable: any) => 
    variable.name && 
    variable.type && 
    variable.description
  );

  const pass = hasRequiredFields && hasValidVariables;

  return {
    message: () => 
      pass 
        ? `Expected template not to be valid`
        : `Expected template to be valid, but it was missing required fields or had invalid variables`,
    pass,
  };
};

// Custom matcher for accessibility testing
const toBeAccessible: MatcherFunction<[]> = function (received) {
  const element = received as HTMLElement;
  
  if (!element || !element.nodeType) {
    return {
      message: () => `Expected element to be a valid DOM element`,
      pass: false,
    };
  }

  // Check for basic accessibility attributes
  const hasAriaLabel = element.hasAttribute('aria-label') || 
                      element.hasAttribute('aria-labelledby');
  const hasRole = element.hasAttribute('role') || 
                 ['button', 'link', 'input', 'textarea'].includes(element.tagName.toLowerCase());
  const isKeyboardAccessible = element.tabIndex >= 0 || 
                              ['button', 'a', 'input', 'textarea', 'select'].includes(element.tagName.toLowerCase());

  const pass = hasAriaLabel && hasRole && isKeyboardAccessible;

  return {
    message: () => 
      pass 
        ? `Expected element not to be accessible`
        : `Expected element to be accessible (have aria-label, role, and be keyboard accessible)`,
    pass,
  };
};

// Custom matcher for ARIA attributes
const toHaveCorrectAriaAttributes: MatcherFunction<[expectedAttributes: Record<string, string>]> = function (received, expectedAttributes) {
  const element = received as HTMLElement;
  
  if (!element || !element.nodeType) {
    return {
      message: () => `Expected element to be a valid DOM element`,
      pass: false,
    };
  }

  const missingAttributes: string[] = [];
  const incorrectAttributes: string[] = [];

  Object.entries(expectedAttributes).forEach(([attr, expectedValue]) => {
    const actualValue = element.getAttribute(attr);
    
    if (actualValue === null) {
      missingAttributes.push(attr);
    } else if (actualValue !== expectedValue) {
      incorrectAttributes.push(`${attr}: expected "${expectedValue}", got "${actualValue}"`);
    }
  });

  const pass = missingAttributes.length === 0 && incorrectAttributes.length === 0;

  return {
    message: () => {
      if (!pass) {
        const errors = [
          ...missingAttributes.map(attr => `Missing attribute: ${attr}`),
          ...incorrectAttributes,
        ];
        return `Expected element to have correct ARIA attributes:\n${errors.join('\n')}`;
      }
      return `Expected element not to have correct ARIA attributes`;
    },
    pass,
  };
};

// Custom matcher for performance testing
const toBeWithinPerformanceThreshold: MatcherFunction<[threshold: number]> = function (received, threshold) {
  const duration = received as number;
  
  if (typeof duration !== 'number') {
    return {
      message: () => `Expected duration to be a number, got ${typeof duration}`,
      pass: false,
    };
  }

  const pass = duration <= threshold;

  return {
    message: () => 
      pass 
        ? `Expected duration ${duration}ms not to be within threshold ${threshold}ms`
        : `Expected duration ${duration}ms to be within threshold ${threshold}ms`,
    pass,
  };
};

// Custom matcher for security signature validation
const toHaveValidSecuritySignature: MatcherFunction<[]> = function (received) {
  const request = received as any;
  
  const hasSignature = request && 
                      request.headers && 
                      request.headers['X-Signature'];
  
  const hasTimestamp = request && 
                      request.headers && 
                      request.headers['X-Timestamp'];

  const hasNonce = request && 
                  request.headers && 
                  request.headers['X-Nonce'];

  const pass = hasSignature && hasTimestamp && hasNonce;

  return {
    message: () => 
      pass 
        ? `Expected request not to have valid security signature`
        : `Expected request to have valid security signature (X-Signature, X-Timestamp, X-Nonce headers)`,
    pass,
  };
};

// Custom matcher for query pattern matching
const toMatchQueryPattern: MatcherFunction<[pattern: string]> = function (received, pattern) {
  const query = received as string;
  
  if (typeof query !== 'string') {
    return {
      message: () => `Expected query to be a string, got ${typeof query}`,
      pass: false,
    };
  }

  // Simple pattern matching - could be enhanced with more sophisticated logic
  const normalizedQuery = query.toLowerCase().replace(/\s+/g, ' ').trim();
  const normalizedPattern = pattern.toLowerCase().replace(/\s+/g, ' ').trim();
  
  const pass = normalizedQuery.includes(normalizedPattern) ||
               new RegExp(normalizedPattern.replace(/\*/g, '.*')).test(normalizedQuery);

  return {
    message: () => 
      pass 
        ? `Expected query "${query}" not to match pattern "${pattern}"`
        : `Expected query "${query}" to match pattern "${pattern}"`,
    pass,
  };
};

// Custom matcher for business data validation
const toBeValidBusinessData: MatcherFunction<[]> = function (received) {
  const data = received as any;
  
  if (!data || typeof data !== 'object') {
    return {
      message: () => `Expected data to be an object, got ${typeof data}`,
      pass: false,
    };
  }

  // Check for common business data patterns
  const hasValidStructure = Array.isArray(data) || 
                           (data.data && Array.isArray(data.data)) ||
                           (data.result && Array.isArray(data.result));

  const hasMetadata = !Array.isArray(data) || 
                     data.metadata || 
                     data.length >= 0;

  const pass = hasValidStructure && hasMetadata;

  return {
    message: () => 
      pass 
        ? `Expected data not to be valid business data`
        : `Expected data to be valid business data (array or object with data/result array and metadata)`,
    pass,
  };
};

// Register custom matchers
expect.extend({
  toBeValidQuery,
  toHaveValidTemplate,
  toBeAccessible,
  toHaveCorrectAriaAttributes,
  toBeWithinPerformanceThreshold,
  toHaveValidSecuritySignature,
  toMatchQueryPattern,
  toBeValidBusinessData,
});

// Export matchers for direct use
export {
  toBeValidQuery,
  toHaveValidTemplate,
  toBeAccessible,
  toHaveCorrectAriaAttributes,
  toBeWithinPerformanceThreshold,
  toHaveValidSecuritySignature,
  toMatchQueryPattern,
  toBeValidBusinessData,
};
