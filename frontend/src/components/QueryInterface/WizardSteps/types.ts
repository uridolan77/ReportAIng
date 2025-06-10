/**
 * Types for Query Wizard Steps
 */

export interface WizardStepProps {
  onNext?: () => void;
  onPrevious?: () => void;
  onComplete?: () => void;
  isFirst?: boolean;
  isLast?: boolean;
  data?: any;
  onChange?: (data: any) => void;
}

export interface DataSource {
  id: string;
  name: string;
  type: 'table' | 'view' | 'query';
  schema?: string;
  description?: string;
}

export interface Field {
  name: string;
  type: string;
  description?: string;
  nullable?: boolean;
  primaryKey?: boolean;
  foreignKey?: boolean;
}

export interface Filter {
  field: string;
  operator: 'equals' | 'not_equals' | 'greater_than' | 'less_than' | 'contains' | 'starts_with' | 'ends_with' | 'in' | 'not_in' | 'is_null' | 'is_not_null';
  value: any;
  logicalOperator?: 'AND' | 'OR';
}

export interface Grouping {
  field: string;
  aggregation?: 'count' | 'sum' | 'avg' | 'min' | 'max';
}

export interface Sorting {
  field: string;
  direction: 'ASC' | 'DESC';
}

export interface QueryWizardData {
  dataSource?: DataSource;
  selectedFields?: Field[];
  filters?: Filter[];
  grouping?: Grouping[];
  sorting?: Sorting[];
  limit?: number;
}
