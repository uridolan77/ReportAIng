# BI Reporting Copilot Frontend Technical Specification

## Executive Summary

This document provides a comprehensive technical specification for building a modern, slick frontend for the BI Reporting Copilot system. The frontend consists of two main applications:

1. **Chat Interface** - Modern conversational UI for natural language queries
2. **Admin Dashboard** - Comprehensive management interface for business contexts and system settings

## System Architecture Overview

### Backend API Structure
The backend provides a comprehensive REST API with the following key controllers:

- **Authentication & User Management**
- **Query Processing & AI Services** 
- **Enhanced Semantic Layer (Phase 2)**
- **Business Metadata Management**
- **Schema Discovery & Management**
- **Real-time Streaming & Features**
- **Admin Configuration & Monitoring**

### Technology Stack Recommendations

**Frontend Framework**: React 18+ with TypeScript
**State Management**: Redux Toolkit + RTK Query
**UI Framework**: Material-UI v5 or Ant Design
**Real-time**: Socket.IO client
**Charts/Visualization**: Recharts or D3.js
**Code Editor**: Monaco Editor (for SQL)
**Authentication**: JWT with refresh tokens

## Core API Endpoints Reference

### 1. Authentication & User Management

#### Auth Controller (`/api/auth`)
```typescript
// Login
POST /api/auth/login
Body: { username: string, password: string }
Response: { success: boolean, accessToken: string, refreshToken: string, expiresAt: string }

// Refresh Token
POST /api/auth/refresh
Body: { refreshToken: string }
Response: { success: boolean, accessToken: string, refreshToken: string, expiresAt: string }

// Logout
POST /api/auth/logout
Body: { refreshToken: string }
Response: { success: boolean }

// MFA Setup
POST /api/auth/mfa/setup
Response: { qrCode: string, secret: string, backupCodes: string[] }

// MFA Verify
POST /api/auth/mfa/verify
Body: { code: string }
Response: { success: boolean }
```

#### User Controller (`/api/user`)
```typescript
// Get Current User Profile
GET /api/user/profile
Response: UserInfo

// Update User Profile
PUT /api/user/profile
Body: UserInfo
Response: UserInfo

// Get User Preferences
GET /api/user/preferences
Response: UserPreferences

// Update User Preferences
PUT /api/user/preferences
Body: UserPreferences
Response: UserPreferences

// Get User Activity
GET /api/user/activity?days=30
Response: UserActivitySummary
```

### 2. Query Processing & AI Services

#### Query Controller (`/api/query`)
```typescript
// Standard Query Processing
POST /api/query
Body: { question: string, context?: string }
Response: { sql: string, results: any[], metadata: QueryMetadata }

// Enhanced Query with AI
POST /api/query/enhanced
Body: EnhancedQueryRequest
Response: EnhancedQueryResponse

// Streaming Query
POST /api/query/streaming
Body: StreamingQueryRequest
Response: Server-Sent Events

// Real-time SQL Generation
POST /api/query/streaming/ai
Body: StreamingSQLRequest
Response: Server-Sent Events

// Get Query History
GET /api/query/history?page=1&limit=20
Response: { queries: QueryHistoryItem[], total: number, page: number }

// Refresh Schema Cache (Admin)
POST /api/query/refresh-schema
Response: { message: string, timestamp: string }
```

### 3. Enhanced Semantic Layer (Phase 2)

#### Enhanced Semantic Layer Controller (`/api/semantic`)
```typescript
// Analyze Query Semantics
POST /api/semantic/analyze
Body: SemanticEnrichmentRequest
Response: EnhancedSchemaResult

// Enrich Schema Metadata
POST /api/semantic/enrich
Body: SemanticEnrichmentRequest
Response: SemanticEnrichmentResponse

// Get Relevant Glossary Terms
GET /api/semantic/glossary/relevant?query=string&maxTerms=10
Response: RelevantGlossaryTerm[]

// Update Table Semantic Metadata
PUT /api/semantic/tables/{schemaName}/{tableName}/metadata
Body: TableSemanticMetadata
Response: { success: boolean }

// Update Column Semantic Metadata
PUT /api/semantic/columns/{tableName}/{columnName}/metadata
Body: ColumnSemanticMetadata
Response: { success: boolean }

// Generate Semantic Embeddings
POST /api/semantic/embeddings/generate?forceRegeneration=false
Response: number (count of embeddings generated)

// Validate Semantic Metadata
GET /api/semantic/validate
Response: SemanticValidationResult

// Find Similar Schema Elements
POST /api/semantic/similarity/find
Body: { element: SchemaElement, threshold: number, maxResults: number }
Response: SimilarSchemaElement[]

// Generate LLM-Optimized Context
POST /api/semantic/llm-context
Body: { query: string, intent: QueryIntent }
Response: LLMOptimizedContext
```

#### Semantic Layer Controller (`/api/semantic-layer`)
```typescript
// Get Business-Friendly Schema
GET /api/semantic-layer/business-schema?query=string&maxTokens=2000
Response: { query: string, schemaDescription: string, generatedAt: string }

// Get Relevant Schema Elements
GET /api/semantic-layer/relevant-schema?query=string&relevanceThreshold=0.7&maxTables=10
Response: ContextualizedSchemaResult

// Update Table Semantic Metadata
PUT /api/semantic-layer/table/{tableId}/semantic-metadata
Body: UpdateTableSemanticRequest
Response: { success: boolean }

// Test Semantic Layer
GET /api/semantic-layer/test?query=string
Response: { businessFriendlySchema: string, relevantSchema: object, summary: object }
```

### 4. Business Metadata Management

#### Business Controller (`/api/business`)
```typescript
// Get Business Tables
GET /api/business/tables
Response: BusinessTableInfoDto[]

// Get Business Table by ID
GET /api/business/tables/{id}
Response: BusinessTableInfoDto

// Create Business Table
POST /api/business/tables
Body: CreateBusinessTableRequest
Response: BusinessTableInfoDto

// Update Business Table
PUT /api/business/tables/{id}
Body: UpdateBusinessTableRequest
Response: BusinessTableInfoDto

// Delete Business Table
DELETE /api/business/tables/{id}
Response: { success: boolean }

// Get Business Columns for Table
GET /api/business/tables/{tableId}/columns
Response: BusinessColumnInfoDto[]

// Get Business Glossary
GET /api/business/glossary?page=1&limit=50&category=string
Response: { terms: BusinessGlossaryDto[], total: number }

// Create Glossary Term
POST /api/business/glossary
Body: CreateGlossaryTermRequest
Response: BusinessGlossaryDto

// Update Glossary Term
PUT /api/business/glossary/{id}
Body: UpdateGlossaryTermRequest
Response: BusinessGlossaryDto

// Delete Glossary Term
DELETE /api/business/glossary/{id}
Response: { success: boolean }
```

#### Business Metadata Controller (`/api/business-metadata`)
```typescript
// Get Population Status
GET /api/business-metadata/status
Response: { totalTables: number, populatedTables: number, lastUpdate: string }

// Populate All Tables
POST /api/business-metadata/populate-all?useAI=true&overwriteExisting=false
Response: BusinessMetadataPopulationResult

// Populate Specific Table
POST /api/business-metadata/populate/{schemaName}/{tableName}?useAI=true&overwrite=false
Response: { success: boolean, message: string }

// Validate Metadata Completeness
GET /api/business-metadata/validate
Response: { coverage: number, missingTables: string[], issues: string[] }
```

### 5. Schema Discovery & Management

#### Schema Controller (`/api/schema`)
```typescript
// Get All Tables
GET /api/schema/tables
Response: TableMetadata[]

// Get Table Details
GET /api/schema/tables/{schemaName}/{tableName}
Response: TableMetadata

// Get Schema Summary
GET /api/schema/summary
Response: { databaseName: string, tableCount: number, lastUpdated: string }

// Get Data Sources
GET /api/schema/datasources
Response: { name: string, schema: string, type: string, rowCount: number }[]

// Refresh Schema
POST /api/schema/refresh
Response: { message: string, tablesDiscovered: number }
```

### 6. Real-time Features & Streaming

#### Features Controller (`/api/features`)
```typescript
// Start Streaming Session
POST /api/features/streaming/sessions/start
Body: StartStreamingRequest
Response: { sessionId: string, status: string }

// Stop Streaming Session
POST /api/features/streaming/sessions/{sessionId}/stop
Response: { success: boolean }

// Get Real-time Dashboard
GET /api/features/streaming/dashboard
Response: RealTimeDashboardData

// Get Live Charts
GET /api/features/streaming/charts
Response: LiveChartData[]

// Subscribe to Real-time Updates
WebSocket: /hub/query-status
Events: StreamingQueryProgress, StreamingQueryComplete, StreamingQueryError
```

### 7. Admin Configuration & Monitoring

#### Dashboard Controller (`/api/dashboard`)
```typescript
// Get User Dashboard
GET /api/dashboard/user
Response: UserDashboardData

// Get System Statistics (Admin Only)
GET /api/dashboard/system-stats?days=30
Response: SystemStatistics

// Get Query Analytics
GET /api/dashboard/analytics?period=week
Response: QueryAnalytics

// Get Performance Metrics
GET /api/dashboard/performance?days=7
Response: PerformanceMetrics
```

#### Configuration Controller (`/api/configuration`) - Admin Only
```typescript
// Get System Configuration
GET /api/configuration/system
Response: SystemConfiguration

// Update System Configuration
PUT /api/configuration/system
Body: SystemConfiguration
Response: { success: boolean }

// Get AI Provider Settings
GET /api/configuration/ai-providers
Response: AIProviderConfig[]

// Update AI Provider Settings
PUT /api/configuration/ai-providers/{providerId}
Body: AIProviderConfig
Response: { success: boolean }

// Get Security Settings
GET /api/configuration/security
Response: SecuritySettings

// Update Security Settings
PUT /api/configuration/security
Body: SecuritySettings
Response: { success: boolean }
```

#### Tuning Controller (`/api/tuning`)
```typescript
// Get Tuning Dashboard
GET /api/tuning/dashboard
Response: TuningDashboardData

// Get Business Tables for Tuning
GET /api/tuning/tables
Response: BusinessTableInfoDto[]

// Update Business Table
PUT /api/tuning/tables/{id}
Body: BusinessTableInfoDto
Response: { success: boolean }

// Get Query Patterns
GET /api/tuning/patterns
Response: QueryPatternDto[]

// Create Query Pattern
POST /api/tuning/patterns
Body: CreateQueryPatternRequest
Response: QueryPatternDto

// Get Prompt Templates
GET /api/tuning/prompts
Response: PromptTemplateDto[]

// Update Prompt Template
PUT /api/tuning/prompts/{id}
Body: PromptTemplateDto
Response: { success: boolean }
```

## Key Data Models & DTOs

### Authentication & User Models
```typescript
interface UserInfo {
  id: string;
  username: string;
  email: string;
  displayName: string;
  roles: string[];
  isActive: boolean;
  lastLoginDate?: string;
  preferences: UserPreferences;
  activitySummary: UserActivitySummary;
  isMfaEnabled: boolean;
  mfaMethod: MfaMethod;
}

interface UserPreferences {
  theme: 'light' | 'dark' | 'auto';
  language: string;
  timezone: string;
  defaultQueryLimit: number;
  enableNotifications: boolean;
  autoSaveQueries: boolean;
  preferredChartTypes: string[];
  dashboardLayout: DashboardLayout;
}

interface AuthenticationResult {
  success: boolean;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user?: UserInfo;
  requiresMfa?: boolean;
}
```

### Query & AI Models
```typescript
interface EnhancedQueryRequest {
  query: string;
  context?: string;
  includeExplanation: boolean;
  optimizeForPerformance: boolean;
  maxResults?: number;
  preferredTables?: string[];
  excludedTables?: string[];
}

interface EnhancedQueryResponse {
  sql: string;
  explanation: string;
  confidence: number;
  executionPlan?: QueryExecutionPlan;
  semanticAnalysis: QuerySemanticAnalysis;
  suggestedOptimizations: string[];
  estimatedExecutionTime: number;
  results?: any[];
  metadata: QueryMetadata;
}

interface QuerySemanticAnalysis {
  intent: QueryIntent;
  entities: ExtractedEntity[];
  businessTerms: string[];
  confidence: number;
  ambiguities: SemanticAmbiguity[];
  suggestedClarifications: string[];
}
```

### Enhanced Semantic Layer Models
```typescript
interface SemanticEnrichmentRequest {
  query: string;
  relevanceThreshold: number;
  maxTables: number;
  includeBusinessGlossary: boolean;
  optimizeForLLM: boolean;
  preferredDomains: string[];
  excludedTables: string[];
}

interface EnhancedSchemaResult {
  query: string;
  queryAnalysis: QuerySemanticAnalysis;
  relevantTables: EnhancedTableInfo[];
  llmContext: LLMOptimizedContext;
  generatedAt: string;
  confidenceScore: number;
}

interface EnhancedTableInfo {
  tableName: string;
  schemaName: string;
  businessPurpose: string;
  relevanceScore: number;
  enhancedColumns: EnhancedColumnInfo[];
  businessContext: string;
  usagePatterns: string[];
  relationshipSemantics: string;
  domainClassification: string;
}

interface RelevantGlossaryTerm {
  term: string;
  definition: string;
  businessContext: string;
  relevanceScore: number;
  relatedTables: string[];
  relatedColumns: string[];
}
```

### Business Metadata Models
```typescript
interface BusinessTableInfoDto {
  id: number;
  tableName: string;
  schemaName: string;
  businessPurpose: string;
  businessContext: string;
  primaryUseCase: string;
  commonQueryPatterns: string;
  businessRules: string;
  domainClassification: string;
  naturalLanguageAliases: string;
  usagePatterns: string;
  dataQualityIndicators: string;
  relationshipSemantics: string;
  importanceScore: number;
  usageFrequency: number;
  businessOwner: string;
  dataGovernancePolicies: string;
  isActive: boolean;
  createdDate: string;
  updatedDate?: string;
  columns: BusinessColumnInfoDto[];
}

interface BusinessColumnInfoDto {
  id: number;
  tableInfoId: number;
  columnName: string;
  businessMeaning: string;
  businessContext: string;
  dataExamples: string;
  validationRules: string;
  naturalLanguageAliases: string;
  valueExamples: string;
  dataLineage: string;
  calculationRules: string;
  semanticTags: string;
  businessDataType: string;
  constraintsAndRules: string;
  dataQualityScore: number;
  usageFrequency: number;
  preferredAggregation: string;
  relatedBusinessTerms: string;
  isKeyColumn: boolean;
  isSensitiveData: boolean;
  isCalculatedField: boolean;
  isActive: boolean;
}

interface BusinessGlossaryDto {
  id: number;
  term: string;
  definition: string;
  businessContext: string;
  synonyms: string;
  relatedTerms: string;
  category: string;
  domain: string;
  examples: string;
  mappedTables: string;
  mappedColumns: string;
  hierarchicalRelations: string;
  preferredCalculation: string;
  disambiguationRules: string;
  businessOwner: string;
  regulationReferences: string;
  confidenceScore: number;
  ambiguityScore: number;
  contextualVariations: string;
  usageCount: number;
  lastUsed?: string;
  lastValidated?: string;
  isActive: boolean;
}
```

### System Configuration Models
```typescript
interface SystemConfiguration {
  applicationName: string;
  version: string;
  environment: string;
  features: FeatureFlags;
  limits: SystemLimits;
  security: SecuritySettings;
  ai: AIConfiguration;
  database: DatabaseConfiguration;
  logging: LoggingConfiguration;
}

interface AIConfiguration {
  defaultProvider: string;
  providers: AIProviderConfig[];
  semanticLayer: SemanticLayerConfig;
  queryGeneration: QueryGenerationConfig;
  validation: ValidationConfig;
}

interface SemanticLayerConfig {
  enabled: boolean;
  relevanceThreshold: number;
  maxTablesPerQuery: number;
  embeddingModel: string;
  vectorSearchEnabled: boolean;
  businessGlossaryEnabled: boolean;
  autoEnrichment: boolean;
}
```

## Frontend Application Structure

### 1. Chat Interface Application

#### Core Components
- **ChatContainer** - Main chat interface with message history
- **MessageInput** - Enhanced input with autocomplete and suggestions
- **QueryResults** - Tabular and chart visualization of results
- **SQLViewer** - Syntax-highlighted SQL display with editing
- **SemanticAnalysis** - Visual representation of query understanding
- **QueryHistory** - Searchable history with favorites

#### Key Features
- Real-time streaming responses
- Semantic query suggestions
- Interactive result exploration
- SQL editing and re-execution
- Export capabilities (CSV, Excel, PDF)
- Query sharing and collaboration

### 2. Admin Dashboard Application

#### Core Modules

**User Management**
- User list with roles and permissions
- User activity monitoring
- MFA configuration
- Access control management

**Business Metadata Management**
- Table and column metadata editing
- Business glossary management
- Semantic enrichment tools
- Metadata validation and coverage reports

**System Configuration**
- AI provider settings
- Security configuration
- Feature flags management
- Performance tuning

**Analytics & Monitoring**
- Query analytics and trends
- System performance metrics
- Error analysis and debugging
- Usage statistics and reporting

**Schema Management**
- Database schema exploration
- Semantic layer configuration
- Relationship mapping
- Data lineage visualization

## Implementation Guidelines

### State Management Strategy
```typescript
// Redux store structure
interface RootState {
  auth: AuthState;
  user: UserState;
  chat: ChatState;
  semantic: SemanticState;
  business: BusinessState;
  admin: AdminState;
  ui: UIState;
}

// RTK Query API slices
const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/auth',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) headers.set('authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  endpoints: (builder) => ({
    login: builder.mutation<AuthenticationResult, LoginRequest>({
      query: (credentials) => ({
        url: 'login',
        method: 'POST',
        body: credentials,
      }),
    }),
    // ... other auth endpoints
  }),
});
```

### Real-time Integration
```typescript
// Socket.IO integration for real-time features
const useSocketConnection = () => {
  const socket = useRef<Socket>();
  const dispatch = useAppDispatch();

  useEffect(() => {
    socket.current = io('/hub/query-status', {
      auth: { token: getAuthToken() }
    });

    socket.current.on('StreamingQueryProgress', (data) => {
      dispatch(updateQueryProgress(data));
    });

    socket.current.on('StreamingQueryComplete', (data) => {
      dispatch(completeQuery(data));
    });

    return () => socket.current?.disconnect();
  }, []);
};
```

### Component Architecture
```typescript
// Example semantic analysis component
const SemanticAnalysisPanel: React.FC<{ query: string }> = ({ query }) => {
  const { data: analysis, isLoading } = useAnalyzeQuerySemanticsQuery(
    { query, relevanceThreshold: 0.7, maxTables: 10 },
    { skip: !query }
  );

  return (
    <Card>
      <CardHeader title="Semantic Analysis" />
      <CardContent>
        {isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : (
          <>
            <QueryIntentDisplay intent={analysis?.queryAnalysis.intent} />
            <BusinessTermsList terms={analysis?.queryAnalysis.businessTerms} />
            <RelevantTablesList tables={analysis?.relevantTables} />
            <ConfidenceScore score={analysis?.confidenceScore} />
          </>
        )}
      </CardContent>
    </Card>
  );
};
```

## Security Considerations

### Authentication Flow
1. JWT-based authentication with refresh tokens
2. MFA support (TOTP, SMS)
3. Role-based access control (RBAC)
4. Session management and timeout

### API Security
1. All endpoints require authentication (except login/register)
2. Admin endpoints require Admin role
3. Rate limiting on sensitive endpoints
4. Input validation and sanitization

### Data Protection
1. Sensitive data masking in logs
2. Encrypted storage of secrets
3. HTTPS enforcement
4. CORS configuration

## Performance Optimization

### Frontend Optimization
1. Code splitting by route and feature
2. Lazy loading of heavy components
3. Virtual scrolling for large datasets
4. Memoization of expensive calculations
5. Optimistic updates for better UX

### API Integration
1. Request deduplication
2. Intelligent caching strategies
3. Pagination for large datasets
4. Compression for large responses
5. Connection pooling

## Development Workflow

### Project Structure
```
frontend/
├── src/
│   ├── components/          # Reusable UI components
│   ├── pages/              # Page components
│   ├── features/           # Feature-specific modules
│   │   ├── auth/
│   │   ├── chat/
│   │   ├── semantic/
│   │   ├── business/
│   │   └── admin/
│   ├── hooks/              # Custom React hooks
│   ├── services/           # API services and utilities
│   ├── store/              # Redux store configuration
│   ├── types/              # TypeScript type definitions
│   └── utils/              # Utility functions
├── public/
└── package.json
```

### Testing Strategy
1. Unit tests for components and utilities
2. Integration tests for API interactions
3. E2E tests for critical user flows
4. Performance testing for large datasets

This specification provides a comprehensive foundation for building a modern, feature-rich frontend that fully leverages the enhanced BI Reporting Copilot backend capabilities.
