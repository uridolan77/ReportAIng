import DOMPurify from 'dompurify';

interface ValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
  sanitizedQuery?: string;
  riskLevel: 'low' | 'medium' | 'high';
}

interface QueryPattern {
  pattern: RegExp;
  severity: 'error' | 'warning';
  message: string;
  riskLevel: 'low' | 'medium' | 'high';
}

export class QueryValidator {
  private static readonly MAX_QUERY_LENGTH = 5000;
  private static readonly MAX_NESTED_DEPTH = 5;
  
  // Dangerous SQL patterns that should be blocked
  private static readonly DANGEROUS_PATTERNS: QueryPattern[] = [
    {
      pattern: /\b(DROP|DELETE|TRUNCATE|ALTER|CREATE|INSERT|UPDATE)\s+/i,
      severity: 'error',
      message: 'Data modification operations are not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(EXEC|EXECUTE|SP_|XP_)\s*\(/i,
      severity: 'error',
      message: 'Stored procedure execution is not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(UNION\s+ALL\s+SELECT|UNION\s+SELECT)\b/i,
      severity: 'warning',
      message: 'UNION operations should be used carefully',
      riskLevel: 'medium'
    },
    {
      pattern: /\b(INFORMATION_SCHEMA|SYS\.|SYSOBJECTS|SYSCOLUMNS)\b/i,
      severity: 'error',
      message: 'System schema access is not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(WAITFOR|DELAY)\s+/i,
      severity: 'error',
      message: 'Time delay operations are not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(OPENROWSET|OPENDATASOURCE|OPENQUERY)\s*\(/i,
      severity: 'error',
      message: 'External data source access is not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(BULK\s+INSERT|BCP)\b/i,
      severity: 'error',
      message: 'Bulk operations are not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(SHUTDOWN|KILL)\s+/i,
      severity: 'error',
      message: 'System control operations are not allowed',
      riskLevel: 'high'
    },
    {
      pattern: /\b(CAST|CONVERT)\s*\(\s*.*\s+AS\s+(XML|VARBINARY)\s*\)/i,
      severity: 'warning',
      message: 'XML and binary conversions should be used carefully',
      riskLevel: 'medium'
    },
    {
      pattern: /\b(CURSOR|FETCH|DEALLOCATE)\b/i,
      severity: 'warning',
      message: 'Cursor operations may impact performance',
      riskLevel: 'medium'
    }
  ];

  // Suspicious patterns that indicate potential injection attempts
  private static readonly INJECTION_PATTERNS: QueryPattern[] = [
    {
      pattern: /['"];\s*(DROP|DELETE|INSERT|UPDATE|CREATE|ALTER)/i,
      severity: 'error',
      message: 'Potential SQL injection detected',
      riskLevel: 'high'
    },
    {
      pattern: /\b(OR|AND)\s+['"]?\d+['"]?\s*=\s*['"]?\d+['"]?/i,
      severity: 'error',
      message: 'Potential tautology-based injection detected',
      riskLevel: 'high'
    },
    {
      pattern: /\b(OR|AND)\s+['"].*['"]?\s*=\s*['"].*['"]?/i,
      severity: 'warning',
      message: 'Suspicious comparison pattern detected',
      riskLevel: 'medium'
    },
    {
      pattern: /\b(UNION\s+SELECT\s+NULL|UNION\s+ALL\s+SELECT\s+NULL)/i,
      severity: 'error',
      message: 'Potential UNION-based injection detected',
      riskLevel: 'high'
    },
    {
      pattern: /\b(CONCAT|CHAR|ASCII|SUBSTRING)\s*\(/i,
      severity: 'warning',
      message: 'String manipulation functions should be used carefully',
      riskLevel: 'medium'
    },
    {
      pattern: /\b(BENCHMARK|SLEEP|PG_SLEEP)\s*\(/i,
      severity: 'error',
      message: 'Time-based attack patterns detected',
      riskLevel: 'high'
    },
    {
      pattern: /\b(LOAD_FILE|INTO\s+OUTFILE|INTO\s+DUMPFILE)/i,
      severity: 'error',
      message: 'File system access attempts detected',
      riskLevel: 'high'
    }
  ];

  // Comment patterns that might hide malicious code
  private static readonly COMMENT_PATTERNS: QueryPattern[] = [
    {
      pattern: /\/\*.*?\*\//gs,
      severity: 'warning',
      message: 'Block comments detected - review for hidden content',
      riskLevel: 'low'
    },
    {
      pattern: /--.*$/gm,
      severity: 'warning',
      message: 'Line comments detected - review for hidden content',
      riskLevel: 'low'
    },
    {
      pattern: /#.*$/gm,
      severity: 'warning',
      message: 'Hash comments detected - review for hidden content',
      riskLevel: 'low'
    }
  ];

  static validateQuery(query: string): ValidationResult {
    const result: ValidationResult = {
      isValid: true,
      errors: [],
      warnings: [],
      riskLevel: 'low'
    };

    // Basic validation
    if (!query || typeof query !== 'string') {
      result.isValid = false;
      result.errors.push('Query must be a non-empty string');
      result.riskLevel = 'high';
      return result;
    }

    // Length validation
    if (query.length > this.MAX_QUERY_LENGTH) {
      result.isValid = false;
      result.errors.push(`Query exceeds maximum length of ${this.MAX_QUERY_LENGTH} characters`);
      result.riskLevel = 'high';
      return result;
    }

    // Sanitize the query
    const sanitizedQuery = this.sanitizeQuery(query);
    result.sanitizedQuery = sanitizedQuery;

    // Check for dangerous patterns
    this.checkPatterns(sanitizedQuery, this.DANGEROUS_PATTERNS, result);
    
    // Check for injection patterns
    this.checkPatterns(sanitizedQuery, this.INJECTION_PATTERNS, result);
    
    // Check for suspicious comments
    this.checkPatterns(sanitizedQuery, this.COMMENT_PATTERNS, result);

    // Check nesting depth
    this.validateNestingDepth(sanitizedQuery, result);

    // Check for balanced parentheses
    this.validateParentheses(sanitizedQuery, result);

    // Determine overall risk level
    result.riskLevel = this.calculateRiskLevel(result);

    // Set validity based on errors
    result.isValid = result.errors.length === 0;

    return result;
  }

  private static sanitizeQuery(query: string): string {
    // Remove potential XSS vectors
    let sanitized = DOMPurify.sanitize(query, { 
      ALLOWED_TAGS: [],
      ALLOWED_ATTR: [],
      KEEP_CONTENT: true
    });

    // Normalize whitespace
    sanitized = sanitized.replace(/\s+/g, ' ').trim();

    // Remove null bytes and other control characters
    sanitized = sanitized.replace(/[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]/g, '');

    // Escape potential escape sequences
    sanitized = sanitized.replace(/\\[rnt'"\\]/g, ' ');

    return sanitized;
  }

  private static checkPatterns(query: string, patterns: QueryPattern[], result: ValidationResult): void {
    for (const pattern of patterns) {
      const matches = query.match(pattern.pattern);
      if (matches) {
        const message = `${pattern.message} (found: ${matches[0]})`;
        
        if (pattern.severity === 'error') {
          result.errors.push(message);
        } else {
          result.warnings.push(message);
        }

        // Update risk level if higher
        if (this.getRiskLevelValue(pattern.riskLevel) > this.getRiskLevelValue(result.riskLevel)) {
          result.riskLevel = pattern.riskLevel;
        }
      }
    }
  }

  private static validateNestingDepth(query: string, result: ValidationResult): void {
    let depth = 0;
    let maxDepth = 0;

    for (const char of query) {
      if (char === '(') {
        depth++;
        maxDepth = Math.max(maxDepth, depth);
      } else if (char === ')') {
        depth--;
      }
    }

    if (maxDepth > this.MAX_NESTED_DEPTH) {
      result.warnings.push(`Query nesting depth (${maxDepth}) exceeds recommended maximum (${this.MAX_NESTED_DEPTH})`);
      if (result.riskLevel === 'low') {
        result.riskLevel = 'medium';
      }
    }
  }

  private static validateParentheses(query: string, result: ValidationResult): void {
    let balance = 0;
    
    for (const char of query) {
      if (char === '(') {
        balance++;
      } else if (char === ')') {
        balance--;
        if (balance < 0) {
          result.errors.push('Unbalanced parentheses detected');
          return;
        }
      }
    }

    if (balance !== 0) {
      result.errors.push('Unbalanced parentheses detected');
    }
  }

  private static calculateRiskLevel(result: ValidationResult): 'low' | 'medium' | 'high' {
    if (result.errors.length > 0) {
      return 'high';
    }
    
    if (result.warnings.length >= 3) {
      return 'medium';
    }
    
    if (result.warnings.length > 0) {
      return 'low';
    }
    
    return 'low';
  }

  private static getRiskLevelValue(level: 'low' | 'medium' | 'high'): number {
    switch (level) {
      case 'low': return 1;
      case 'medium': return 2;
      case 'high': return 3;
      default: return 0;
    }
  }

  // Additional validation for natural language queries
  static validateNaturalLanguageQuery(query: string): ValidationResult {
    const result = this.validateQuery(query);
    
    // Additional checks for natural language
    const suspiciousKeywords = [
      'javascript:', 'data:', 'vbscript:', 'onload', 'onerror', 'onclick',
      '<script', '</script>', 'eval(', 'setTimeout(', 'setInterval('
    ];

    for (const keyword of suspiciousKeywords) {
      if (query.toLowerCase().includes(keyword.toLowerCase())) {
        result.errors.push(`Suspicious keyword detected: ${keyword}`);
        result.riskLevel = 'high';
        result.isValid = false;
      }
    }

    return result;
  }

  // Validate query context and permissions
  static validateQueryContext(query: string, userRoles: string[], allowedTables: string[]): ValidationResult {
    const result = this.validateQuery(query);
    
    // Check if user has permission for advanced queries
    const hasAdvancedPermissions = userRoles.includes('admin') || userRoles.includes('analyst');
    
    if (!hasAdvancedPermissions) {
      // More restrictive validation for regular users
      const advancedPatterns = [
        /\b(JOIN|UNION|SUBQUERY|CTE|WITH)\b/i,
        /\b(CASE\s+WHEN|IF\s*\(|COALESCE|ISNULL)\b/i
      ];

      for (const pattern of advancedPatterns) {
        if (pattern.test(query)) {
          result.warnings.push('Advanced SQL features require elevated permissions');
          if (result.riskLevel === 'low') {
            result.riskLevel = 'medium';
          }
        }
      }
    }

    // Validate table access
    const tablePattern = /\bFROM\s+(\w+)/gi;
    let match;
    while ((match = tablePattern.exec(query)) !== null) {
      const tableName = match[1];
      if (!allowedTables.includes(tableName.toLowerCase())) {
        result.errors.push(`Access to table '${tableName}' is not permitted`);
        result.riskLevel = 'high';
        result.isValid = false;
      }
    }

    return result;
  }
}

// Export validation utilities
export const validateQuery = QueryValidator.validateQuery.bind(QueryValidator);
export const validateNaturalLanguageQuery = QueryValidator.validateNaturalLanguageQuery.bind(QueryValidator);
export const validateQueryContext = QueryValidator.validateQueryContext.bind(QueryValidator);
