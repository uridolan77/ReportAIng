export class ErrorService {
  static initialize() {
    // In production, you would initialize Sentry here
    if (process.env.NODE_ENV === 'production') {
      // Sentry.init({
      //   dsn: process.env.REACT_APP_SENTRY_DSN,
      //   integrations: [
      //     new BrowserTracing(),
      //   ],
      //   tracesSampleRate: 0.1,
      //   environment: process.env.NODE_ENV,
      // });
    }
  }

  static logError(error: Error, context?: Record<string, any>) {
    console.error('Error occurred:', error, context);
    
    if (process.env.NODE_ENV === 'production') {
      // Sentry.captureException(error, {
      //   extra: context,
      // });
    }
  }

  static logWarning(message: string, context?: Record<string, any>) {
    if (process.env.NODE_ENV === 'development') {
      console.warn('Warning:', message, context);
    }

    if (process.env.NODE_ENV === 'production') {
      // Sentry.captureMessage(message, 'warning');
    }
  }

  static logInfo(message: string, context?: Record<string, any>) {
    if (process.env.NODE_ENV === 'development') {
      console.info('Info:', message, context);
    }
  }

  // Custom error types
  static createApiError(message: string, status?: number, endpoint?: string): ApiError {
    return new ApiError(message, status, endpoint);
  }

  static createValidationError(message: string, field?: string): ValidationError {
    return new ValidationError(message, field);
  }

  static createNetworkError(message: string): NetworkError {
    return new NetworkError(message);
  }
}

// Custom Error Classes
export class ApiError extends Error {
  constructor(
    message: string,
    public status?: number,
    public endpoint?: string
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export class ValidationError extends Error {
  constructor(
    message: string,
    public field?: string
  ) {
    super(message);
    this.name = 'ValidationError';
  }
}

export class NetworkError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'NetworkError';
  }
}

// Error boundary hook
export const useErrorHandler = () => {
  const handleError = (error: Error, errorInfo?: any) => {
    ErrorService.logError(error, errorInfo);
  };

  const handleApiError = (error: ApiError) => {
    ErrorService.logError(error, {
      status: error.status,
      endpoint: error.endpoint,
      type: 'API_ERROR'
    });
  };

  const handleValidationError = (error: ValidationError) => {
    ErrorService.logWarning(error.message, {
      field: error.field,
      type: 'VALIDATION_ERROR'
    });
  };

  return {
    handleError,
    handleApiError,
    handleValidationError
  };
};
