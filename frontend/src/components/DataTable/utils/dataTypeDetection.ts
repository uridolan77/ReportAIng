// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\utils\dataTypeDetection.ts
import dayjs from 'dayjs';
import { DataTableColumn } from '../types';

export interface DataTypeAnalysis {
  dataType: 'string' | 'number' | 'date' | 'boolean' | 'money' | 'currency';
  filterType: 'text' | 'number' | 'money' | 'date' | 'dateRange' | 'select' | 'multiselect' | 'boolean';
  filterOptions?: any[];
  confidence: number;
  sampleValues: any[];
  uniqueCount: number;
  nullCount: number;
}

export interface ColumnAnalysis extends DataTypeAnalysis {
  columnName: string;
  title: string;
}

/**
 * Enhanced data type detection for DataTable columns
 */
export class DataTypeDetector {
  private static readonly MONEY_PATTERNS = [
    /^\$[\d,]+\.?\d*$/,           // $1,234.56
    /^[\d,]+\.?\d*\s*(USD|EUR|GBP|CAD|AUD)$/i, // 1,234.56 USD
    /^(USD|EUR|GBP|CAD|AUD)\s*[\d,]+\.?\d*$/i, // USD 1,234.56
    /^€[\d,]+\.?\d*$/,           // €1,234.56
    /^£[\d,]+\.?\d*$/,           // £1,234.56
  ];

  private static readonly MONEY_KEYWORDS = [
    'amount', 'price', 'cost', 'revenue', 'profit', 'loss', 'value',
    'sum', 'balance', 'payment', 'fee', 'charge', 'salary', 'wage', 'income',
    'expense', 'budget', 'currency', 'money', 'cash', 'dollar', 'euro', 'pound'
  ];

  private static readonly NON_MONEY_KEYWORDS = [
    'bet', 'bets', 'win', 'wins', 'session', 'sessions', 'player', 'players',
    'count', 'number', 'quantity', 'qty', 'total'
  ];

  private static readonly DATE_PATTERNS = [
    /^\d{4}-\d{2}-\d{2}$/,                    // 2024-01-15
    /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/,  // ISO datetime
    /^\d{2}\/\d{2}\/\d{4}$/,                 // 01/15/2024
    /^\d{2}-\d{2}-\d{4}$/,                   // 01-15-2024
  ];

  private static readonly DATE_KEYWORDS = [
    'date', 'time', 'created', 'updated', 'modified', 'timestamp', 'when',
    'start', 'end', 'begin', 'finish', 'expire', 'due', 'birth', 'death'
  ];

  /**
   * Analyze a single column's data type and filtering characteristics
   */
  static analyzeColumn(data: any[], columnName: string, sampleSize: number = 100): DataTypeAnalysis {
    const sample = data.slice(0, sampleSize).map(row => row[columnName]);
    const nonNullSample = sample.filter(val => val != null && val !== '');
    const uniqueValues = [...new Set(nonNullSample)];

    if (nonNullSample.length === 0) {
      return {
        dataType: 'string',
        filterType: 'text',
        confidence: 0,
        sampleValues: [],
        uniqueCount: 0,
        nullCount: sample.length
      };
    }

    const analysis = {
      sampleValues: nonNullSample.slice(0, 10),
      uniqueCount: uniqueValues.length,
      nullCount: sample.length - nonNullSample.length,
      confidence: 0,
      dataType: 'string' as const,
      filterType: 'text' as const,
      filterOptions: undefined as any[] | undefined
    };

    // Check for boolean values
    const booleanAnalysis = this.analyzeBooleanType(nonNullSample, columnName);
    if (booleanAnalysis.confidence && booleanAnalysis.confidence > 0.8) {
      const result = { ...analysis, ...booleanAnalysis };
      if (process.env.NODE_ENV === 'development') {
        console.log(`🔍 Column "${columnName}" detected as BOOLEAN:`, result);
      }
      return result;
    }

    // Check for money/currency values
    const moneyAnalysis = this.analyzeMoneyType(nonNullSample, columnName);
    if (moneyAnalysis.confidence && moneyAnalysis.confidence > 0.7) {
      const result = { ...analysis, ...moneyAnalysis };
      if (process.env.NODE_ENV === 'development') {
        console.log(`🔍 Column "${columnName}" detected as MONEY:`, result);
      }
      return result;
    }

    // Check for date values
    const dateAnalysis = this.analyzeDateType(nonNullSample, columnName);
    if (dateAnalysis.confidence && dateAnalysis.confidence > 0.8) {
      const result = { ...analysis, ...dateAnalysis };
      if (process.env.NODE_ENV === 'development') {
        console.log(`🔍 Column "${columnName}" detected as DATE:`, result);
      }
      return result;
    }

    // Check for numeric values
    const numericAnalysis = this.analyzeNumericType(nonNullSample, columnName);
    if (numericAnalysis.confidence && numericAnalysis.confidence > 0.8) {
      const result = { ...analysis, ...numericAnalysis };
      if (process.env.NODE_ENV === 'development') {
        console.log(`🔍 Column "${columnName}" detected as NUMBER:`, result);
      }
      return result;
    }

    // Default to string analysis
    const stringAnalysis = this.analyzeStringType(nonNullSample, uniqueValues, columnName);
    const result = { ...analysis, ...stringAnalysis };
    if (process.env.NODE_ENV === 'development') {
      console.log(`🔍 Column "${columnName}" detected as STRING:`, result);
    }
    return result;
  }

  /**
   * Analyze multiple columns and return enhanced column configurations
   */
  static analyzeColumns(data: any[], columns: string[]): ColumnAnalysis[] {
    return columns.map(columnName => ({
      columnName,
      title: this.formatColumnTitle(columnName),
      ...this.analyzeColumn(data, columnName)
    }));
  }

  /**
   * Convert analysis results to DataTable column configuration
   */
  static createColumnConfig(analysis: ColumnAnalysis): Partial<DataTableColumn> {
    const config: Partial<DataTableColumn> = {
      key: analysis.columnName,
      title: analysis.title,
      dataIndex: analysis.columnName,
      dataType: analysis.dataType,
      filterType: analysis.filterType,
      filterable: true,
      sortable: true,
      searchable: analysis.dataType === 'string'
    };

    if (analysis.filterOptions) {
      config.filterOptions = analysis.filterOptions;
    }

    // Add aggregation for numeric types
    if (analysis.dataType === 'number' || analysis.dataType === 'money') {
      config.aggregation = 'sum';
    }

    return config;
  }

  private static analyzeBooleanType(sample: any[], columnName: string): Partial<DataTypeAnalysis> {
    const booleanValues = sample.filter(val => 
      typeof val === 'boolean' || 
      val === 'true' || val === 'false' ||
      val === 'yes' || val === 'no' ||
      val === 'Y' || val === 'N' ||
      val === 1 || val === 0
    );

    const confidence = booleanValues.length / sample.length;
    
    if (confidence > 0.8) {
      return {
        dataType: 'boolean',
        filterType: 'boolean',
        confidence
      };
    }

    return { confidence: 0 };
  }

  private static analyzeMoneyType(sample: any[], columnName: string): Partial<DataTypeAnalysis> {
    const columnLower = columnName.toLowerCase();
    const hasMoneyKeyword = this.MONEY_KEYWORDS.some(keyword => columnLower.includes(keyword));
    const hasNonMoneyKeyword = this.NON_MONEY_KEYWORDS.some(keyword => columnLower.includes(keyword));

    // If it has non-money keywords, it's probably not a money field
    if (hasNonMoneyKeyword) {
      return { confidence: 0 };
    }

    let moneyPatternMatches = 0;
    let numericValues = 0;

    for (const val of sample) {
      const strVal = String(val);

      // Check for money patterns
      if (this.MONEY_PATTERNS.some(pattern => pattern.test(strVal))) {
        moneyPatternMatches++;
      }

      // Check for numeric values that could be money
      if (!isNaN(Number(val)) && Number(val) >= 0) {
        numericValues++;
      }
    }

    const patternConfidence = moneyPatternMatches / sample.length;
    const numericConfidence = numericValues / sample.length;
    const keywordBonus = hasMoneyKeyword ? 0.3 : 0;

    const confidence = Math.max(patternConfidence, numericConfidence * 0.7) + keywordBonus;

    if (confidence > 0.7) {
      return {
        dataType: 'money',
        filterType: 'money',
        confidence
      };
    }

    return { confidence: 0 };
  }

  private static analyzeDateType(sample: any[], columnName: string): Partial<DataTypeAnalysis> {
    const columnLower = columnName.toLowerCase();
    const hasDateKeyword = this.DATE_KEYWORDS.some(keyword => columnLower.includes(keyword));
    
    let datePatternMatches = 0;
    let validDates = 0;

    for (const val of sample) {
      const strVal = String(val);
      
      // Check for date patterns
      if (this.DATE_PATTERNS.some(pattern => pattern.test(strVal))) {
        datePatternMatches++;
      }
      
      // Check if it's a valid date
      if (dayjs(val).isValid() && !isNaN(Date.parse(strVal))) {
        validDates++;
      }
    }

    const patternConfidence = datePatternMatches / sample.length;
    const validDateConfidence = validDates / sample.length;
    const keywordBonus = hasDateKeyword ? 0.2 : 0;
    
    const confidence = Math.max(patternConfidence, validDateConfidence) + keywordBonus;

    if (confidence > 0.8) {
      return {
        dataType: 'date',
        filterType: 'dateRange',
        confidence
      };
    }

    return { confidence: 0 };
  }

  private static analyzeNumericType(sample: any[], columnName: string): Partial<DataTypeAnalysis> {
    const numericValues = sample.filter(val => !isNaN(Number(val)) && val !== '' && val !== null);
    const confidence = numericValues.length / sample.length;

    if (confidence > 0.8) {
      return {
        dataType: 'number',
        filterType: 'number',
        confidence
      };
    }

    return { confidence: 0 };
  }

  private static analyzeStringType(sample: any[], uniqueValues: any[], columnName: string): Partial<DataTypeAnalysis> {
    const uniqueRatio = uniqueValues.length / sample.length;
    
    // If there are few unique values relative to total, use multiselect
    if (uniqueValues.length <= 20 && uniqueRatio < 0.5) {
      return {
        dataType: 'string',
        filterType: 'multiselect',
        filterOptions: uniqueValues.map(val => ({ label: String(val), value: val })),
        confidence: 0.9
      };
    }

    // Otherwise use text filter
    return {
      dataType: 'string',
      filterType: 'text',
      confidence: 0.8
    };
  }

  private static formatColumnTitle(columnName: string): string {
    return columnName
      .replace(/([A-Z])/g, ' $1') // Add space before capital letters
      .replace(/[_-]/g, ' ')      // Replace underscores and hyphens with spaces
      .replace(/\b\w/g, l => l.toUpperCase()) // Capitalize first letter of each word
      .trim();
  }
}

/**
 * Utility function to enhance existing column configurations with auto-detected types
 */
export function enhanceColumnsWithTypeDetection(
  data: any[],
  columns: Partial<DataTableColumn>[]
): DataTableColumn[] {
  if (!data.length) return columns as DataTableColumn[];

  const columnNames = columns.map(col => col.dataIndex || col.key || '');
  const analyses = DataTypeDetector.analyzeColumns(data, columnNames);

  return columns.map((col, index) => {
    const analysis = analyses[index];
    const autoConfig = DataTypeDetector.createColumnConfig(analysis);

    // Merge auto-detected config with existing config, preferring existing values
    const enhancedColumn: DataTableColumn = {
      ...autoConfig,
      ...col,
      key: col.key || autoConfig.key || '',
      title: col.title || autoConfig.title || '',
      dataIndex: col.dataIndex || autoConfig.dataIndex || '',
      // Ensure filterType is properly typed
      filterType: (col.filterType || autoConfig.filterType) as DataTableColumn['filterType']
    } as DataTableColumn;

    return enhancedColumn;
  });
}
