// Branded types for enhanced type safety
// These prevent mixing up different string/number types that represent different concepts

// Base branded type utility
export type Brand<T, TBrand extends string> = T & { readonly __brand: TBrand };

// ID types - prevent mixing different types of IDs
export type UserId = Brand<string, 'UserId'>;
export type QueryId = Brand<string, 'QueryId'>;
export type SessionId = Brand<string, 'SessionId'>;
export type RequestId = Brand<string, 'RequestId'>;
export type TableId = Brand<string, 'TableId'>;
export type ColumnId = Brand<string, 'ColumnId'>;
export type SchemaId = Brand<string, 'SchemaId'>;
export type CacheKey = Brand<string, 'CacheKey'>;
export type TabId = Brand<string, 'TabId'>;

// Timestamp types - prevent mixing different time representations
export type UnixTimestamp = Brand<number, 'UnixTimestamp'>;
export type ISOTimestamp = Brand<string, 'ISOTimestamp'>;
export type ExecutionTimeMs = Brand<number, 'ExecutionTimeMs'>;

// SQL types - prevent mixing different SQL representations
export type SqlQuery = Brand<string, 'SqlQuery'>;
export type NaturalLanguageQuery = Brand<string, 'NaturalLanguageQuery'>;
export type TableName = Brand<string, 'TableName'>;
export type ColumnName = Brand<string, 'ColumnName'>;
export type SchemaName = Brand<string, 'SchemaName'>;

// Numeric types with specific meanings
export type ConfidenceScore = Brand<number, 'ConfidenceScore'>; // 0-1
export type RowCount = Brand<number, 'RowCount'>;
export type ColumnCount = Brand<number, 'ColumnCount'>;
export type PageNumber = Brand<number, 'PageNumber'>; // 1-based
export type PageSize = Brand<number, 'PageSize'>;
export type Percentage = Brand<number, 'Percentage'>; // 0-100

// URL and path types
export type ApiEndpoint = Brand<string, 'ApiEndpoint'>;
export type FilePath = Brand<string, 'FilePath'>;
export type ImageUrl = Brand<string, 'ImageUrl'>;

// Security types
export type JwtToken = Brand<string, 'JwtToken'>;
export type ApiKey = Brand<string, 'ApiKey'>;
export type PasswordHash = Brand<string, 'PasswordHash'>;
export type Salt = Brand<string, 'Salt'>;

// Email and validation types
export type EmailAddress = Brand<string, 'EmailAddress'>;
export type PhoneNumber = Brand<string, 'PhoneNumber'>;
export type IpAddress = Brand<string, 'IpAddress'>;
export type UserAgent = Brand<string, 'UserAgent'>;

// Business domain types
export type Currency = Brand<string, 'Currency'>; // ISO currency codes
export type CountryCode = Brand<string, 'CountryCode'>; // ISO country codes
export type LanguageCode = Brand<string, 'LanguageCode'>; // ISO language codes
export type TimeZone = Brand<string, 'TimeZone'>; // IANA timezone

// Version types
export type SemanticVersion = Brand<string, 'SemanticVersion'>; // e.g., "1.2.3"
export type ApiVersion = Brand<string, 'ApiVersion'>; // e.g., "v1", "2.0"
export type SchemaVersion = Brand<number, 'SchemaVersion'>;

// Utility functions for creating branded types
export const createUserId = (id: string): UserId => id as UserId;
export const createQueryId = (id: string): QueryId => id as QueryId;
export const createSessionId = (id: string): SessionId => id as SessionId;
export const createRequestId = (id: string): RequestId => id as RequestId;
export const createTableId = (id: string): TableId => id as TableId;
export const createColumnId = (id: string): ColumnId => id as ColumnId;
export const createSchemaId = (id: string): SchemaId => id as SchemaId;
export const createCacheKey = (key: string): CacheKey => key as CacheKey;
export const createTabId = (id: string): TabId => id as TabId;

export const createUnixTimestamp = (timestamp: number): UnixTimestamp => timestamp as UnixTimestamp;
export const createISOTimestamp = (timestamp: string): ISOTimestamp => timestamp as ISOTimestamp;
export const createExecutionTimeMs = (time: number): ExecutionTimeMs => time as ExecutionTimeMs;

export const createSqlQuery = (sql: string): SqlQuery => sql as SqlQuery;
export const createNaturalLanguageQuery = (query: string): NaturalLanguageQuery => query as NaturalLanguageQuery;
export const createTableName = (name: string): TableName => name as TableName;
export const createColumnName = (name: string): ColumnName => name as ColumnName;
export const createSchemaName = (name: string): SchemaName => name as SchemaName;

export const createConfidenceScore = (score: number): ConfidenceScore => {
  if (score < 0 || score > 1) {
    throw new Error('Confidence score must be between 0 and 1');
  }
  return score as ConfidenceScore;
};

export const createRowCount = (count: number): RowCount => {
  if (count < 0) {
    throw new Error('Row count cannot be negative');
  }
  return count as RowCount;
};

export const createColumnCount = (count: number): ColumnCount => {
  if (count < 0) {
    throw new Error('Column count cannot be negative');
  }
  return count as ColumnCount;
};

export const createPageNumber = (page: number): PageNumber => {
  if (page < 1) {
    throw new Error('Page number must be at least 1');
  }
  return page as PageNumber;
};

export const createPageSize = (size: number): PageSize => {
  if (size < 1) {
    throw new Error('Page size must be at least 1');
  }
  return size as PageSize;
};

export const createPercentage = (value: number): Percentage => {
  if (value < 0 || value > 100) {
    throw new Error('Percentage must be between 0 and 100');
  }
  return value as Percentage;
};

export const createApiEndpoint = (endpoint: string): ApiEndpoint => endpoint as ApiEndpoint;
export const createFilePath = (path: string): FilePath => path as FilePath;
export const createImageUrl = (url: string): ImageUrl => url as ImageUrl;

export const createJwtToken = (token: string): JwtToken => token as JwtToken;
export const createApiKey = (key: string): ApiKey => key as ApiKey;
export const createPasswordHash = (hash: string): PasswordHash => hash as PasswordHash;
export const createSalt = (salt: string): Salt => salt as Salt;

export const createEmailAddress = (email: string): EmailAddress => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) {
    throw new Error('Invalid email address format');
  }
  return email as EmailAddress;
};

export const createPhoneNumber = (phone: string): PhoneNumber => phone as PhoneNumber;
export const createIpAddress = (ip: string): IpAddress => ip as IpAddress;
export const createUserAgent = (userAgent: string): UserAgent => userAgent as UserAgent;

export const createCurrency = (currency: string): Currency => {
  if (currency.length !== 3) {
    throw new Error('Currency code must be 3 characters');
  }
  return currency.toUpperCase() as Currency;
};

export const createCountryCode = (country: string): CountryCode => {
  if (country.length !== 2) {
    throw new Error('Country code must be 2 characters');
  }
  return country.toUpperCase() as CountryCode;
};

export const createLanguageCode = (language: string): LanguageCode => {
  if (language.length !== 2) {
    throw new Error('Language code must be 2 characters');
  }
  return language.toLowerCase() as LanguageCode;
};

export const createTimeZone = (timezone: string): TimeZone => timezone as TimeZone;

export const createSemanticVersion = (version: string): SemanticVersion => {
  const semverRegex = /^\d+\.\d+\.\d+$/;
  if (!semverRegex.test(version)) {
    throw new Error('Invalid semantic version format (expected x.y.z)');
  }
  return version as SemanticVersion;
};

export const createApiVersion = (version: string): ApiVersion => version as ApiVersion;
export const createSchemaVersion = (version: number): SchemaVersion => {
  if (version < 0) {
    throw new Error('Schema version cannot be negative');
  }
  return version as SchemaVersion;
};

// Type guards for branded types
export const isUserId = (value: unknown): value is UserId =>
  typeof value === 'string' && value.length > 0;

export const isQueryId = (value: unknown): value is QueryId =>
  typeof value === 'string' && value.length > 0;

export const isConfidenceScore = (value: unknown): value is ConfidenceScore =>
  typeof value === 'number' && value >= 0 && value <= 1;

export const isRowCount = (value: unknown): value is RowCount =>
  typeof value === 'number' && value >= 0 && Number.isInteger(value);

export const isColumnCount = (value: unknown): value is ColumnCount =>
  typeof value === 'number' && value >= 0 && Number.isInteger(value);

export const isPageNumber = (value: unknown): value is PageNumber =>
  typeof value === 'number' && value >= 1 && Number.isInteger(value);

export const isPageSize = (value: unknown): value is PageSize =>
  typeof value === 'number' && value >= 1 && Number.isInteger(value);

export const isPercentage = (value: unknown): value is Percentage =>
  typeof value === 'number' && value >= 0 && value <= 100;

export const isEmailAddress = (value: unknown): value is EmailAddress => {
  if (typeof value !== 'string') return false;
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(value);
};

export const isCurrency = (value: unknown): value is Currency =>
  typeof value === 'string' && value.length === 3 && /^[A-Z]{3}$/.test(value);

export const isCountryCode = (value: unknown): value is CountryCode =>
  typeof value === 'string' && value.length === 2 && /^[A-Z]{2}$/.test(value);

export const isLanguageCode = (value: unknown): value is LanguageCode =>
  typeof value === 'string' && value.length === 2 && /^[a-z]{2}$/.test(value);

export const isSemanticVersion = (value: unknown): value is SemanticVersion => {
  if (typeof value !== 'string') return false;
  const semverRegex = /^\d+\.\d+\.\d+$/;
  return semverRegex.test(value);
};

// Utility functions for working with branded types
export const unwrapBrand = <T>(value: Brand<T, any>): T => value as T;

export const rebrand = <T, TBrandFrom extends string, TBrandTo extends string>(
  value: Brand<T, TBrandFrom>
): Brand<T, TBrandTo> => (value as unknown) as Brand<T, TBrandTo>;

// Common branded type combinations
export interface BrandedQueryExecution {
  queryId: QueryId;
  sql: SqlQuery;
  naturalLanguageQuery: NaturalLanguageQuery;
  executionTimeMs: ExecutionTimeMs;
  rowCount: RowCount;
  columnCount: ColumnCount;
  confidence: ConfidenceScore;
  timestamp: ISOTimestamp;
}

export interface BrandedUserSession {
  userId: UserId;
  sessionId: SessionId;
  email: EmailAddress;
  lastActivity: ISOTimestamp;
  ipAddress: IpAddress;
  userAgent: UserAgent;
}

export interface BrandedApiRequest {
  requestId: RequestId;
  endpoint: ApiEndpoint;
  timestamp: ISOTimestamp;
  userId?: UserId;
  sessionId?: SessionId;
}

// All types are already exported above, no need for duplicate exports
