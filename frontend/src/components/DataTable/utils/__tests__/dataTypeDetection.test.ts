// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\utils\__tests__\dataTypeDetection.test.ts
import { DataTypeDetector } from '../dataTypeDetection';

describe('DataTypeDetector', () => {
  const sampleData = [
    {
      id: 1,
      name: 'John Doe',
      age: 30,
      salary: '$50,000',
      revenue: 12345.67,
      isActive: true,
      joinDate: '2023-01-15',
      status: 'Active'
    },
    {
      id: 2,
      name: 'Jane Smith',
      age: 25,
      salary: '$45,000',
      revenue: 23456.78,
      isActive: false,
      joinDate: '2023-02-20',
      status: 'Active'
    },
    {
      id: 3,
      name: 'Bob Johnson',
      age: 35,
      salary: '$60,000',
      revenue: 34567.89,
      isActive: true,
      joinDate: '2023-03-10',
      status: 'Inactive'
    }
  ];

  describe('analyzeColumn', () => {
    it('should detect string type for text fields', () => {
      const analysis = DataTypeDetector.analyzeColumn(sampleData, 'name');
      expect(analysis.dataType).toBe('string');
      expect(analysis.filterType).toBe('multiselect');
      expect(analysis.confidence).toBeGreaterThan(0.8);
    });

    it('should detect number type for numeric fields', () => {
      const analysis = DataTypeDetector.analyzeColumn(sampleData, 'age');
      expect(analysis.dataType).toBe('number');
      expect(analysis.filterType).toBe('number');
      expect(analysis.confidence).toBeGreaterThan(0.8);
    });

    it('should detect money type for currency fields', () => {
      const analysis = DataTypeDetector.analyzeColumn(sampleData, 'salary');
      expect(analysis.dataType).toBe('money');
      expect(analysis.filterType).toBe('money');
      expect(analysis.confidence).toBeGreaterThan(0.7);
    });

    it('should detect boolean type for boolean fields', () => {
      const analysis = DataTypeDetector.analyzeColumn(sampleData, 'isActive');
      expect(analysis.dataType).toBe('boolean');
      expect(analysis.filterType).toBe('boolean');
      expect(analysis.confidence).toBeGreaterThan(0.8);
    });

    it('should detect date type for date fields', () => {
      const analysis = DataTypeDetector.analyzeColumn(sampleData, 'joinDate');
      expect(analysis.dataType).toBe('date');
      expect(analysis.filterType).toBe('dateRange');
      expect(analysis.confidence).toBeGreaterThan(0.8);
    });

    it('should provide filter options for multiselect fields', () => {
      const analysis = DataTypeDetector.analyzeColumn(sampleData, 'status');
      expect(analysis.dataType).toBe('string');
      expect(analysis.filterType).toBe('multiselect');
      expect(analysis.filterOptions).toBeDefined();
      expect(analysis.filterOptions?.length).toBe(2); // Active, Inactive
    });
  });

  describe('analyzeColumns', () => {
    it('should analyze multiple columns correctly', () => {
      const columns = ['name', 'age', 'salary', 'isActive'];
      const analyses = DataTypeDetector.analyzeColumns(sampleData, columns);
      
      expect(analyses).toHaveLength(4);
      expect(analyses[0].dataType).toBe('string'); // name
      expect(analyses[1].dataType).toBe('number'); // age
      expect(analyses[2].dataType).toBe('money');  // salary
      expect(analyses[3].dataType).toBe('boolean'); // isActive
    });
  });

  describe('createColumnConfig', () => {
    it('should create proper column configuration', () => {
      const analysis = {
        columnName: 'salary',
        title: 'Salary',
        dataType: 'money' as const,
        filterType: 'money' as const,
        confidence: 0.9,
        sampleValues: ['$50,000', '$45,000'],
        uniqueCount: 3,
        nullCount: 0
      };

      const config = DataTypeDetector.createColumnConfig(analysis);
      
      expect(config.key).toBe('salary');
      expect(config.title).toBe('Salary');
      expect(config.dataType).toBe('money');
      expect(config.filterType).toBe('money');
      expect(config.filterable).toBe(true);
      expect(config.sortable).toBe(true);
      expect(config.aggregation).toBe('sum');
    });
  });
});
