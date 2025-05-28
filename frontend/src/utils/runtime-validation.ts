import { z } from 'zod';
import { ValidationUtils } from './validation';

// Runtime type checking utilities for enhanced type safety

// Type assertion with runtime validation
export function assertType<T>(
  schema: z.ZodSchema<T>,
  value: unknown,
  context?: string
): asserts value is T {
  const result = ValidationUtils.validate(schema, value);
  
  if (!result.success) {
    const errorMessage = context 
      ? `Type assertion failed for ${context}: ${result.error!.message}`
      : `Type assertion failed: ${result.error!.message}`;
    
    throw new TypeError(errorMessage);
  }
}

// Safe type checking that returns boolean
export function isType<T>(
  schema: z.ZodSchema<T>,
  value: unknown
): value is T {
  const result = ValidationUtils.validate(schema, value);
  return result.success;
}

// Runtime type guard creator
export function createTypeGuard<T>(schema: z.ZodSchema<T>) {
  return (value: unknown): value is T => {
    return isType(schema, value);
  };
}

// Validate and cast with fallback
export function validateWithFallback<T>(
  schema: z.ZodSchema<T>,
  value: unknown,
  fallback: T,
  context?: string
): T {
  const result = ValidationUtils.validate(schema, value);
  
  if (result.success) {
    return result.data!;
  }
  
  if (context && process.env.NODE_ENV === 'development') {
    console.warn(`Validation failed for ${context}, using fallback:`, {
      error: result.error!.message,
      value,
      fallback
    });
  }
  
  return fallback;
}

// Array validation with filtering
export function validateArrayWithFilter<T>(
  schema: z.ZodSchema<T>,
  array: unknown[],
  context?: string
): T[] {
  const validItems: T[] = [];
  const errors: string[] = [];
  
  array.forEach((item, index) => {
    const result = ValidationUtils.validate(schema, item);
    
    if (result.success) {
      validItems.push(result.data!);
    } else {
      errors.push(`Item ${index}: ${result.error!.message}`);
    }
  });
  
  if (errors.length > 0 && process.env.NODE_ENV === 'development') {
    console.warn(`Array validation errors${context ? ` for ${context}` : ''}:`, errors);
  }
  
  return validItems;
}

// Deep object validation with path tracking
export function validateObjectPaths<T extends Record<string, any>>(
  schema: z.ZodSchema<T>,
  obj: unknown,
  context?: string
): { valid: T | null; errors: Array<{ path: string; message: string }> } {
  const result = ValidationUtils.validate(schema, obj, { errorFormat: 'detailed' });
  
  if (result.success) {
    return { valid: result.data!, errors: [] };
  }
  
  const errors = result.error!.details?.issues || [];
  const formattedErrors = errors.map((issue: any) => ({
    path: issue.path || 'unknown',
    message: issue.message || 'Unknown error'
  }));
  
  if (context && process.env.NODE_ENV === 'development') {
    console.warn(`Object validation failed for ${context}:`, formattedErrors);
  }
  
  return { valid: null, errors: formattedErrors };
}

// Conditional validation based on runtime conditions
export function validateConditionally<T>(
  condition: boolean,
  schema: z.ZodSchema<T>,
  value: unknown,
  context?: string
): T | null {
  if (!condition) {
    return null;
  }
  
  const result = ValidationUtils.validate(schema, value);
  
  if (!result.success) {
    if (context && process.env.NODE_ENV === 'development') {
      console.warn(`Conditional validation failed for ${context}:`, result.error!.message);
    }
    return null;
  }
  
  return result.data!;
}

// Validate API response with automatic error handling
export function validateApiResponse<T>(
  schema: z.ZodSchema<T>,
  response: unknown,
  endpoint?: string
): T {
  try {
    return ValidationUtils.validateOrThrow(schema, response);
  } catch (error) {
    const errorMessage = endpoint 
      ? `API response validation failed for ${endpoint}`
      : 'API response validation failed';
    
    console.error(errorMessage, {
      error: error instanceof Error ? error.message : 'Unknown error',
      response
    });
    
    throw new Error(`${errorMessage}: ${error instanceof Error ? error.message : 'Unknown error'}`);
  }
}

// Validate environment variables with detailed error reporting
export function validateEnvironment<T>(
  schema: z.ZodSchema<T>,
  env: Record<string, string | undefined> = process.env
): T {
  const result = ValidationUtils.validate(schema, env, { 
    strict: true, 
    errorFormat: 'detailed' 
  });
  
  if (!result.success) {
    const errors = result.error!.details?.issues || [];
    const missingVars = errors
      .filter((issue: any) => issue.code === 'invalid_type' && issue.received === 'undefined')
      .map((issue: any) => issue.path);
    
    const invalidVars = errors
      .filter((issue: any) => issue.code !== 'invalid_type' || issue.received !== 'undefined')
      .map((issue: any) => ({ path: issue.path, message: issue.message }));
    
    let errorMessage = 'Environment validation failed:\n';
    
    if (missingVars.length > 0) {
      errorMessage += `Missing required environment variables: ${missingVars.join(', ')}\n`;
    }
    
    if (invalidVars.length > 0) {
      errorMessage += 'Invalid environment variables:\n';
      invalidVars.forEach(({ path, message }) => {
        errorMessage += `  ${path}: ${message}\n`;
      });
    }
    
    throw new Error(errorMessage);
  }
  
  return result.data!;
}

// Validate with transformation and error recovery
export function validateAndTransform<TInput, TOutput>(
  inputSchema: z.ZodSchema<TInput>,
  transformer: (input: TInput) => TOutput,
  value: unknown,
  context?: string
): TOutput | null {
  try {
    const validatedInput = ValidationUtils.validateOrThrow(inputSchema, value);
    return transformer(validatedInput);
  } catch (error) {
    if (context && process.env.NODE_ENV === 'development') {
      console.warn(`Validation and transformation failed for ${context}:`, {
        error: error instanceof Error ? error.message : 'Unknown error',
        value
      });
    }
    return null;
  }
}

// Batch validation with detailed reporting
export function validateBatch<T>(
  schema: z.ZodSchema<T>,
  items: unknown[],
  context?: string
): {
  valid: T[];
  invalid: Array<{ index: number; value: unknown; error: string }>;
  successRate: number;
} {
  const valid: T[] = [];
  const invalid: Array<{ index: number; value: unknown; error: string }> = [];
  
  items.forEach((item, index) => {
    const result = ValidationUtils.validate(schema, item);
    
    if (result.success) {
      valid.push(result.data!);
    } else {
      invalid.push({
        index,
        value: item,
        error: result.error!.message
      });
    }
  });
  
  const successRate = items.length > 0 ? valid.length / items.length : 0;
  
  if (invalid.length > 0 && process.env.NODE_ENV === 'development') {
    console.warn(`Batch validation${context ? ` for ${context}` : ''} completed with errors:`, {
      successRate: `${(successRate * 100).toFixed(1)}%`,
      validCount: valid.length,
      invalidCount: invalid.length,
      errors: invalid.slice(0, 5) // Show first 5 errors
    });
  }
  
  return { valid, invalid, successRate };
}

// Validate with retry logic for network responses
export async function validateWithRetry<T>(
  schema: z.ZodSchema<T>,
  valueProvider: () => Promise<unknown>,
  maxRetries: number = 3,
  context?: string
): Promise<T> {
  let lastError: Error | null = null;
  
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      const value = await valueProvider();
      return ValidationUtils.validateOrThrow(schema, value);
    } catch (error) {
      lastError = error instanceof Error ? error : new Error('Unknown error');
      
      if (attempt < maxRetries) {
        const delay = Math.min(1000 * Math.pow(2, attempt - 1), 5000); // Exponential backoff
        
        if (context && process.env.NODE_ENV === 'development') {
          console.warn(`Validation attempt ${attempt} failed for ${context}, retrying in ${delay}ms:`, lastError.message);
        }
        
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }
  
  throw new Error(
    `Validation failed after ${maxRetries} attempts${context ? ` for ${context}` : ''}: ${lastError?.message || 'Unknown error'}`
  );
}

// Create a validation middleware for React Query
export function createValidationMiddleware<T>(
  schema: z.ZodSchema<T>,
  context?: string
) {
  return (data: unknown): T => {
    return validateApiResponse(schema, data, context);
  };
}

// Validate form data with field-level errors
export function validateFormData<T extends Record<string, any>>(
  schema: z.ZodSchema<T>,
  formData: unknown,
  context?: string
): {
  data: T | null;
  errors: Record<string, string>;
  isValid: boolean;
} {
  const result = ValidationUtils.validate(schema, formData, { errorFormat: 'detailed' });
  
  if (result.success) {
    return {
      data: result.data!,
      errors: {},
      isValid: true
    };
  }
  
  const errors: Record<string, string> = {};
  const issues = result.error!.details?.issues || [];
  
  issues.forEach((issue: any) => {
    const path = Array.isArray(issue.path) ? issue.path.join('.') : String(issue.path);
    errors[path] = issue.message;
  });
  
  if (context && process.env.NODE_ENV === 'development') {
    console.warn(`Form validation failed for ${context}:`, errors);
  }
  
  return {
    data: null,
    errors,
    isValid: false
  };
}

// Type-safe localStorage with validation
export function createValidatedStorage<T>(
  key: string,
  schema: z.ZodSchema<T>,
  defaultValue: T
) {
  return {
    get(): T {
      try {
        const stored = localStorage.getItem(key);
        if (!stored) return defaultValue;
        
        const parsed = JSON.parse(stored);
        return validateWithFallback(schema, parsed, defaultValue, `localStorage:${key}`);
      } catch {
        return defaultValue;
      }
    },
    
    set(value: T): void {
      try {
        assertType(schema, value, `localStorage:${key}`);
        localStorage.setItem(key, JSON.stringify(value));
      } catch (error) {
        console.error(`Failed to store value in localStorage:${key}:`, error);
      }
    },
    
    remove(): void {
      localStorage.removeItem(key);
    }
  };
}

// Export commonly used validation functions
export {
  ValidationUtils,
  assertType as assert,
  isType as is,
  validateWithFallback as withFallback,
  validateApiResponse as apiResponse,
  validateEnvironment as environment
};
