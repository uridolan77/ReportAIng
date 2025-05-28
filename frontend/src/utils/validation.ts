import { z } from 'zod';

// Validation result types
export interface ValidationResult<T> {
  success: boolean;
  data?: T;
  error?: ValidationError;
}

export interface ValidationError {
  message: string;
  code: string;
  path?: string[] | undefined;
  details?: Record<string, any> | undefined;
  originalError?: unknown;
}

export interface ValidationOptions {
  strict?: boolean;
  stripUnknown?: boolean;
  errorFormat?: 'detailed' | 'simple';
  customErrorMessages?: Record<string, string>;
}

// Custom error class for validation errors
export class SchemaValidationError extends Error {
  public readonly code: string;
  public readonly path?: string[] | undefined;
  public readonly details?: Record<string, any> | undefined;
  public readonly originalError?: unknown;

  constructor(
    message: string,
    code: string = 'VALIDATION_ERROR',
    path?: string[] | undefined,
    details?: Record<string, any> | undefined,
    originalError?: unknown
  ) {
    super(message);
    this.name = 'SchemaValidationError';
    this.code = code;
    this.path = path || undefined;
    this.details = details || undefined;
    this.originalError = originalError;
  }
}

// Main validation utility class
export class ValidationUtils {
  /**
   * Validate data against a Zod schema with enhanced error handling
   */
  static validate<T>(
    schema: z.ZodSchema<T>,
    data: unknown,
    options: ValidationOptions = {}
  ): ValidationResult<T> {
    try {
      const {
        strict = false,
        stripUnknown = true,
        errorFormat = 'detailed',
        customErrorMessages = {}
      } = options;

      // Configure schema based on options
      let processedSchema = schema;

      if (stripUnknown && schema instanceof z.ZodObject) {
        processedSchema = strict ? schema.strict() : schema.strip();
      }

      // Attempt validation
      const result = processedSchema.parse(data);

      return {
        success: true,
        data: result
      };
    } catch (error) {
      const validationError = this.formatValidationError(
        error,
        errorFormat,
        customErrorMessages
      );

      return {
        success: false,
        error: validationError
      };
    }
  }

  /**
   * Safe validation that returns null on error instead of throwing
   */
  static safeParse<T>(
    schema: z.ZodSchema<T>,
    data: unknown,
    options: ValidationOptions = {}
  ): T | null {
    const result = this.validate(schema, data, options);
    return result.success ? result.data! : null;
  }

  /**
   * Validate and throw on error
   */
  static validateOrThrow<T>(
    schema: z.ZodSchema<T>,
    data: unknown,
    options: ValidationOptions = {}
  ): T {
    const result = this.validate(schema, data, options);

    if (!result.success) {
      throw new SchemaValidationError(
        result.error!.message,
        result.error!.code,
        result.error!.path,
        result.error!.details,
        result.error!.originalError
      );
    }

    return result.data!;
  }

  /**
   * Validate multiple items with a schema
   */
  static validateArray<T>(
    schema: z.ZodSchema<T>,
    items: unknown[],
    options: ValidationOptions = {}
  ): ValidationResult<T[]> {
    const results: T[] = [];
    const errors: ValidationError[] = [];

    for (let i = 0; i < items.length; i++) {
      const result = this.validate(schema, items[i], options);

      if (result.success) {
        results.push(result.data!);
      } else {
        errors.push({
          ...result.error!,
          path: [`[${i}]`, ...(result.error!.path || [])]
        });
      }
    }

    if (errors.length > 0) {
      return {
        success: false,
        error: {
          message: `Validation failed for ${errors.length} items`,
          code: 'ARRAY_VALIDATION_ERROR',
          details: { errors, validCount: results.length, totalCount: items.length }
        }
      };
    }

    return {
      success: true,
      data: results
    };
  }

  /**
   * Create a validator function for reuse
   */
  static createValidator<T>(
    schema: z.ZodSchema<T>,
    options: ValidationOptions = {}
  ) {
    return (data: unknown): ValidationResult<T> => {
      return this.validate(schema, data, options);
    };
  }

  /**
   * Create a type guard function
   */
  static createTypeGuard<T>(schema: z.ZodSchema<T>) {
    return (data: unknown): data is T => {
      const result = this.validate(schema, data);
      return result.success;
    };
  }

  /**
   * Validate partial data (useful for updates)
   */
  static validatePartial<T extends z.ZodRawShape>(
    schema: z.ZodObject<T>,
    data: unknown,
    options: ValidationOptions = {}
  ): ValidationResult<Partial<z.infer<z.ZodObject<T>>>> {
    return this.validate(schema.partial(), data, options);
  }

  /**
   * Format Zod errors into a more user-friendly format
   */
  private static formatValidationError(
    error: unknown,
    format: 'detailed' | 'simple',
    customMessages: Record<string, string>
  ): ValidationError {
    if (error instanceof z.ZodError) {
      const issues = error.issues;

      if (format === 'simple') {
        const firstIssue = issues[0];
        const path = firstIssue.path.join('.');
        const customMessage = customMessages[firstIssue.code] || customMessages[path];

        return {
          message: customMessage || firstIssue.message,
          code: firstIssue.code,
          path: firstIssue.path,
          originalError: error
        };
      }

      // Detailed format
      const formattedIssues = issues.map(issue => ({
        path: issue.path.join('.'),
        message: customMessages[issue.code] || customMessages[issue.path.join('.')] || issue.message,
        code: issue.code,
        received: 'received' in issue ? issue.received : undefined,
        expected: 'expected' in issue ? issue.expected : undefined
      }));

      return {
        message: `Validation failed with ${issues.length} error(s)`,
        code: 'ZOD_VALIDATION_ERROR',
        details: {
          issues: formattedIssues,
          errorCount: issues.length
        },
        originalError: error
      };
    }

    // Handle non-Zod errors
    if (error instanceof Error) {
      return {
        message: error.message,
        code: 'UNKNOWN_VALIDATION_ERROR',
        originalError: error
      };
    }

    return {
      message: 'Unknown validation error occurred',
      code: 'UNKNOWN_ERROR',
      originalError: error
    };
  }

  /**
   * Merge multiple schemas (useful for extending base schemas)
   */
  static mergeSchemas<T extends z.ZodRawShape, U extends z.ZodRawShape>(
    baseSchema: z.ZodObject<T>,
    extensionSchema: z.ZodObject<U>
  ): z.ZodObject<T & U> {
    return baseSchema.merge(extensionSchema);
  }

  /**
   * Create a discriminated union validator
   */
  static createDiscriminatedUnion<T extends string, U extends z.ZodDiscriminatedUnionOption<T>[]>(
    discriminator: T,
    options: U
  ) {
    return z.discriminatedUnion(discriminator, options);
  }

  /**
   * Validate environment variables
   */
  static validateEnv<T>(
    schema: z.ZodSchema<T>,
    env: Record<string, string | undefined> = process.env
  ): T {
    const result = this.validate(schema, env, { strict: true });

    if (!result.success) {
      throw new SchemaValidationError(
        `Environment validation failed: ${result.error!.message}`,
        'ENV_VALIDATION_ERROR',
        result.error!.path,
        result.error!.details,
        result.error!.originalError
      );
    }

    return result.data!;
  }

  /**
   * Create a schema with custom error messages
   */
  static withCustomErrors<T>(
    schema: z.ZodSchema<T>,
    errorMap: z.ZodErrorMap
  ): z.ZodSchema<T> {
    return schema.setErrorMap(errorMap);
  }

  /**
   * Validate API response with automatic error handling
   */
  static validateApiResponse<T>(
    schema: z.ZodSchema<T>,
    response: unknown,
    context?: string
  ): T {
    try {
      return this.validateOrThrow(schema, response, {
        stripUnknown: true,
        errorFormat: 'detailed'
      });
    } catch (error) {
      if (error instanceof SchemaValidationError) {
        console.error(`API Response validation failed${context ? ` for ${context}` : ''}:`, {
          error: error.message,
          code: error.code,
          path: error.path,
          details: error.details,
          response
        });

        throw new Error(
          `Invalid API response${context ? ` for ${context}` : ''}: ${error.message}`
        );
      }
      throw error;
    }
  }

  /**
   * Create a runtime type assertion
   */
  static assertType<T>(
    schema: z.ZodSchema<T>,
    data: unknown,
    message?: string
  ): asserts data is T {
    const result = this.validate(schema, data);

    if (!result.success) {
      throw new SchemaValidationError(
        message || `Type assertion failed: ${result.error!.message}`,
        'TYPE_ASSERTION_ERROR',
        result.error!.path,
        result.error!.details,
        result.error!.originalError
      );
    }
  }

  /**
   * Validate with transformation
   */
  static validateAndTransform<T, U>(
    schema: z.ZodSchema<T>,
    data: unknown,
    transformer: (data: T) => U,
    options: ValidationOptions = {}
  ): ValidationResult<U> {
    const validationResult = this.validate(schema, data, options);

    if (!validationResult.success) {
      return validationResult as ValidationResult<U>;
    }

    try {
      const transformedData = transformer(validationResult.data!);
      return {
        success: true,
        data: transformedData
      };
    } catch (error) {
      return {
        success: false,
        error: {
          message: error instanceof Error ? error.message : 'Transformation failed',
          code: 'TRANSFORMATION_ERROR',
          originalError: error
        }
      };
    }
  }
}

// Utility functions for common validation patterns
export const isValidEmail = (email: string): boolean => {
  return z.string().email().safeParse(email).success;
};

export const isValidUrl = (url: string): boolean => {
  return z.string().url().safeParse(url).success;
};

export const isValidUuid = (uuid: string): boolean => {
  return z.string().uuid().safeParse(uuid).success;
};

export const isValidDate = (date: string): boolean => {
  return z.string().datetime().safeParse(date).success;
};

// Export the main validation function for convenience
export const validate = ValidationUtils.validate;
export const safeParse = ValidationUtils.safeParse;
export const validateOrThrow = ValidationUtils.validateOrThrow;
