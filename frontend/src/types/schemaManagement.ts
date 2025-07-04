// Schema Management Types

export interface BusinessSchemaDto {
  id: string;
  name: string;
  description?: string;
  createdBy: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
  isActive: boolean;
  isDefault: boolean;
  tags: string[];
  totalVersions: number;
  currentVersion?: BusinessSchemaVersionDto;
}

export interface BusinessSchemaVersionDto {
  id: string;
  schemaId: string;
  versionNumber: number;
  versionName?: string;
  description?: string;
  createdBy: string;
  createdAt: string;
  isActive: boolean;
  isCurrent: boolean;
  changeLog: ChangeLogEntry[];
  tableContexts?: SchemaTableContextDto[];
  columnContexts?: SchemaColumnContextDto[];
  glossaryTerms?: SchemaGlossaryTermDto[];
  relationships?: SchemaRelationshipDto[];
}

export interface SchemaTableContextDto {
  id: string;
  schemaVersionId: string;
  tableName: string;
  schemaName: string;
  businessPurpose?: string;
  businessContext?: string;
  primaryUseCase?: string;
  keyBusinessMetrics: string[];
  commonQueryPatterns: string[];
  businessRules: string[];
  confidenceScore?: number;
  isAutoGenerated: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SchemaColumnContextDto {
  id: string;
  tableContextId: string;
  columnName: string;
  businessName?: string;
  businessDescription?: string;
  businessDataType?: string;
  dataExamples: string[];
  validationRules: string[];
  commonUseCases: string[];
  isKeyColumn: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  confidenceScore?: number;
  isAutoGenerated: boolean;
  createdAt: string;
  updatedAt: string;
  tableName?: string; // For display purposes
}

export interface SchemaGlossaryTermDto {
  id: string;
  schemaVersionId: string;
  term: string;
  definition: string;
  businessContext?: string;
  category?: string;
  synonyms: string[];
  relatedTerms: string[];
  sourceTables: string[];
  sourceColumns: string[];
  confidenceScore?: number;
  isAutoGenerated: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SchemaRelationshipDto {
  id: string;
  schemaVersionId: string;
  fromTable: string;
  toTable: string;
  relationshipType: string;
  fromColumns: string[];
  toColumns: string[];
  businessDescription?: string;
  confidenceScore?: number;
  isAutoGenerated: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface UserSchemaPreferenceDto {
  id: string;
  userId: string;
  schemaVersionId: string;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
  schemaName?: string;
  versionName?: string;
}

export interface ChangeLogEntry {
  timestamp: string;
  type: 'Created' | 'Updated' | 'Deleted' | 'Applied';
  category: 'Schema' | 'Version' | 'Table' | 'Column' | 'Glossary' | 'Relationship';
  item: string;
  description: string;
  userId?: string;
}

// Request/Response Types

export interface CreateSchemaRequest {
  name: string;
  description?: string;
  tags?: string[];
}

export interface UpdateSchemaRequest {
  name?: string;
  description?: string;
  tags?: string[];
  isActive?: boolean;
}

export interface CreateSchemaVersionRequest {
  schemaId: string;
  versionName?: string;
  description?: string;
  basedOnVersionId?: string;
}

export interface UpdateTableContextRequest {
  businessPurpose?: string;
  businessContext?: string;
  primaryUseCase?: string;
  keyBusinessMetrics?: string[];
  commonQueryPatterns?: string[];
  businessRules?: string[];
}

export interface UpdateColumnContextRequest {
  businessName?: string;
  businessDescription?: string;
  businessDataType?: string;
  dataExamples?: string[];
  validationRules?: string[];
  commonUseCases?: string[];
  isKeyColumn?: boolean;
}

export interface UpdateGlossaryTermRequest {
  term?: string;
  definition?: string;
  businessContext?: string;
  category?: string;
  synonyms?: string[];
  relatedTerms?: string[];
  sourceTables?: string[];
  sourceColumns?: string[];
}

export interface UpdateRelationshipRequest {
  relationshipType?: string;
  fromColumns?: string[];
  toColumns?: string[];
  businessDescription?: string;
}

export interface ApplyAutoGeneratedContentRequest {
  schemaVersionId: string;
  tableContexts?: AutoGeneratedTableContext[];
  columnContexts?: AutoGeneratedColumnContext[];
  glossaryTerms?: AutoGeneratedGlossaryTerm[];
  relationships?: AutoGeneratedRelationship[];
  overwriteExisting?: boolean;
}

export interface AutoGeneratedTableContext {
  tableName: string;
  schemaName: string;
  businessPurpose: string;
  businessContext: string;
  primaryUseCase: string;
  keyBusinessMetrics: string[];
  commonQueryPatterns: string[];
  businessRules: string[];
  confidenceScore: number;
}

export interface AutoGeneratedColumnContext {
  tableName: string;
  columnName: string;
  businessName: string;
  businessDescription: string;
  businessDataType: string;
  dataExamples: string[];
  validationRules: string[];
  commonUseCases: string[];
  isKeyColumn: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  confidenceScore: number;
}

export interface AutoGeneratedGlossaryTerm {
  term: string;
  definition: string;
  businessContext: string;
  category: string;
  synonyms: string[];
  relatedTerms: string[];
  sourceTables: string[];
  sourceColumns: string[];
  confidenceScore: number;
}

export interface AutoGeneratedRelationship {
  fromTable: string;
  toTable: string;
  relationshipType: string;
  fromColumns: string[];
  toColumns: string[];
  businessDescription: string;
  confidenceScore: number;
}

export interface SchemaComparisonResult {
  version1: BusinessSchemaVersionDto;
  version2: BusinessSchemaVersionDto;
  differences: SchemaDifference[];
  summary: ComparisonSummary;
}

export interface SchemaDifference {
  type: 'Added' | 'Removed' | 'Modified';
  category: 'Table' | 'Column' | 'Glossary' | 'Relationship';
  item: string;
  description: string;
  oldValue?: any;
  newValue?: any;
}

export interface ComparisonSummary {
  totalDifferences: number;
  addedItems: number;
  removedItems: number;
  modifiedItems: number;
  categoryCounts: Record<string, number>;
}

export interface SchemaExportData {
  schema: BusinessSchemaDto;
  version: BusinessSchemaVersionDto;
  tableContexts: SchemaTableContextDto[];
  columnContexts: SchemaColumnContextDto[];
  glossaryTerms: SchemaGlossaryTermDto[];
  relationships: SchemaRelationshipDto[];
  exportedAt: string;
  exportedBy: string;
  displayName: string;
}

export interface SchemaImportRequest {
  targetSchemaId?: string;
  createNewSchema?: boolean;
  newSchemaName?: string;
  importData: SchemaExportData;
  mergeStrategy: 'Replace' | 'Merge' | 'Skip';
}

// UI State Types

export interface SchemaManagementState {
  schemas: BusinessSchemaDto[];
  selectedSchema: BusinessSchemaDto | null;
  selectedVersion: BusinessSchemaVersionDto | null;
  loading: boolean;
  error: string | null;
}

export interface SchemaEditorState {
  editingItem: string | null;
  unsavedChanges: boolean;
  validationErrors: Record<string, string>;
}

// Filter and Search Types

export interface SchemaFilter {
  searchTerm: string;
  isActive?: boolean;
  isDefault?: boolean;
  tags?: string[];
  createdBy?: string;
  dateRange?: {
    start: string;
    end: string;
  };
}

export interface ContentFilter {
  searchTerm: string;
  contentType: 'all' | 'auto' | 'manual';
  confidenceThreshold?: number;
  category?: string;
}

// API Response Types

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Validation Types

export interface ValidationResult {
  isValid: boolean;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export interface ValidationError {
  field: string;
  message: string;
  code: string;
}

export interface ValidationWarning {
  field: string;
  message: string;
  suggestion?: string;
}
