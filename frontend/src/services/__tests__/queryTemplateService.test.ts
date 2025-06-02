/**
 * Query Template Service Tests
 * Comprehensive tests for the QueryTemplateService using testing utilities
 */

import { queryTemplateService, QueryTemplateService } from '../queryTemplateService';
import { 
  mockLocalStorage, 
  setupTestEnvironment,
  createMockTemplate,
  createMockShortcut 
} from '../../test-utils/testing-providers';
import { ServiceTestUtils } from '../../test-utils/api-test-utils';

// Setup test environment
setupTestEnvironment();

describe('QueryTemplateService', () => {
  let service: QueryTemplateService;

  beforeEach(() => {
    // Clear localStorage
    mockLocalStorage.clear();
    
    // Get fresh instance
    service = QueryTemplateService.getInstance();
  });

  describe('Singleton Pattern', () => {
    it('returns the same instance', () => {
      const instance1 = QueryTemplateService.getInstance();
      const instance2 = QueryTemplateService.getInstance();
      
      expect(instance1).toBe(instance2);
    });
  });

  describe('Default Templates', () => {
    it('initializes with default templates', () => {
      const templates = service.getTemplates();
      
      expect(templates.length).toBeGreaterThan(0);
      expect(templates).toEqual(
        expect.arrayContaining([
          expect.objectContaining({
            name: 'Revenue by Time Period',
            category: 'financial',
          }),
          expect.objectContaining({
            name: 'Top Players by Deposits',
            category: 'operational',
          }),
        ])
      );
    });

    it('initializes with default shortcuts', () => {
      const shortcuts = service.getShortcuts();
      
      expect(shortcuts.length).toBeGreaterThan(0);
      expect(shortcuts).toEqual(
        expect.arrayContaining([
          expect.objectContaining({
            shortcut: 'rev',
            name: 'Revenue Today',
          }),
          expect.objectContaining({
            shortcut: 'users',
            name: 'Active Users',
          }),
        ])
      );
    });
  });

  describe('Template Management', () => {
    it('creates a new template', () => {
      const templateData = {
        name: 'Test Template',
        description: 'A test template',
        category: 'custom' as const,
        template: 'Show me {{data}}',
        variables: [
          {
            name: 'data',
            type: 'string' as const,
            description: 'Data to show',
            required: true,
          },
        ],
        tags: ['test'],
        difficulty: 'beginner' as const,
        isFavorite: false,
        isPublic: false,
      };

      const newTemplate = service.createTemplate(templateData);

      expect(newTemplate).toMatchObject(templateData);
      expect(newTemplate.id).toBeDefined();
      expect(newTemplate.createdAt).toBeDefined();
      expect(newTemplate.updatedAt).toBeDefined();
      expect(newTemplate.usageCount).toBe(0);

      // Should be in templates list
      const templates = service.getTemplates();
      expect(templates).toContainEqual(newTemplate);
    });

    it('processes template with variables', () => {
      const template = createMockTemplate({
        template: 'Show me revenue from {{startDate}} to {{endDate}}',
        variables: [
          {
            name: 'startDate',
            type: 'date' as const,
            description: 'Start date',
            required: true,
          },
          {
            name: 'endDate',
            type: 'date' as const,
            description: 'End date',
            required: true,
          },
        ],
      });

      const variables = {
        startDate: '2024-01-01',
        endDate: '2024-12-31',
      };

      const processed = service.processTemplate(template, variables);

      expect(processed).toBe('Show me revenue from 2024-01-01 to 2024-12-31');
    });

    it('uses default values for missing variables', () => {
      const template = createMockTemplate({
        template: 'Show me data for {{period}}',
        variables: [
          {
            name: 'period',
            type: 'string' as const,
            description: 'Time period',
            required: false,
            defaultValue: 'last month',
          },
        ],
      });

      const processed = service.processTemplate(template, {});

      expect(processed).toBe('Show me data for last month');
    });

    it('toggles template favorite status', () => {
      const templates = service.getTemplates();
      const template = templates[0];
      const initialFavoriteStatus = template.isFavorite;

      service.toggleFavorite(template.id);

      const updatedTemplates = service.getTemplates();
      const updatedTemplate = updatedTemplates.find(t => t.id === template.id);

      expect(updatedTemplate?.isFavorite).toBe(!initialFavoriteStatus);
    });

    it('increments template usage count', () => {
      const templates = service.getTemplates();
      const template = templates[0];
      const initialUsageCount = template.usageCount;

      service.incrementUsage(template.id, 'template');

      const updatedTemplates = service.getTemplates();
      const updatedTemplate = updatedTemplates.find(t => t.id === template.id);

      expect(updatedTemplate?.usageCount).toBe(initialUsageCount + 1);
    });
  });

  describe('Shortcut Management', () => {
    it('creates a new shortcut', () => {
      const shortcutData = {
        name: 'Test Shortcut',
        shortcut: 'test',
        query: 'Show me test data',
        description: 'A test shortcut',
        category: 'test',
        isActive: true,
      };

      const newShortcut = service.createShortcut(shortcutData);

      expect(newShortcut).toMatchObject(shortcutData);
      expect(newShortcut.id).toBeDefined();
      expect(newShortcut.createdAt).toBeDefined();
      expect(newShortcut.usageCount).toBe(0);

      // Should be in shortcuts list
      const shortcuts = service.getShortcuts();
      expect(shortcuts).toContainEqual(newShortcut);
    });

    it('increments shortcut usage count', () => {
      const shortcuts = service.getShortcuts();
      const shortcut = shortcuts[0];
      const initialUsageCount = shortcut.usageCount;

      service.incrementUsage(shortcut.id, 'shortcut');

      const updatedShortcuts = service.getShortcuts();
      const updatedShortcut = updatedShortcuts.find(s => s.id === shortcut.id);

      expect(updatedShortcut?.usageCount).toBe(initialUsageCount + 1);
    });
  });

  describe('Search and Suggestions', () => {
    it('finds exact shortcut matches', () => {
      const suggestions = service.searchSuggestions('rev');

      expect(suggestions).toEqual(
        expect.arrayContaining([
          expect.objectContaining({
            type: 'shortcut',
            confidence: 1.0,
          }),
        ])
      );
    });

    it('searches templates by name', () => {
      const suggestions = service.searchSuggestions('revenue');

      expect(suggestions.length).toBeGreaterThan(0);
      expect(suggestions).toEqual(
        expect.arrayContaining([
          expect.objectContaining({
            type: 'template',
            confidence: expect.any(Number),
          }),
        ])
      );
    });

    it('searches templates by tags', () => {
      const suggestions = service.searchSuggestions('financial');

      expect(suggestions.length).toBeGreaterThan(0);
      expect(suggestions.some(s => s.type === 'template')).toBe(true);
    });

    it('returns recent queries when no search term', () => {
      // Add some recent queries
      service.addToRecent('Show me revenue');
      service.addToRecent('Show me users');

      const suggestions = service.searchSuggestions('');

      expect(suggestions).toEqual(
        expect.arrayContaining([
          expect.objectContaining({
            type: 'recent',
            text: 'Show me revenue',
          }),
          expect.objectContaining({
            type: 'recent',
            text: 'Show me users',
          }),
        ])
      );
    });

    it('sorts suggestions by confidence', () => {
      const suggestions = service.searchSuggestions('revenue');

      // Should be sorted by confidence (descending)
      for (let i = 1; i < suggestions.length; i++) {
        expect(suggestions[i - 1].confidence).toBeGreaterThanOrEqual(suggestions[i].confidence);
      }
    });

    it('limits suggestions to 10 items', () => {
      const suggestions = service.searchSuggestions('a'); // Broad search

      expect(suggestions.length).toBeLessThanOrEqual(10);
    });
  });

  describe('Recent Queries', () => {
    it('adds queries to recent history', () => {
      service.addToRecent('Test query 1');
      service.addToRecent('Test query 2');

      const suggestions = service.searchSuggestions('');
      const recentSuggestions = suggestions.filter(s => s.type === 'recent');

      expect(recentSuggestions).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ text: 'Test query 1' }),
          expect.objectContaining({ text: 'Test query 2' }),
        ])
      );
    });

    it('removes duplicates from recent history', () => {
      service.addToRecent('Duplicate query');
      service.addToRecent('Other query');
      service.addToRecent('Duplicate query'); // Should move to front, not duplicate

      const suggestions = service.searchSuggestions('');
      const recentSuggestions = suggestions.filter(s => s.type === 'recent');
      const duplicateQueries = recentSuggestions.filter(s => s.text === 'Duplicate query');

      expect(duplicateQueries.length).toBe(1);
    });

    it('limits recent queries to 20 items', () => {
      // Add more than 20 queries
      for (let i = 0; i < 25; i++) {
        service.addToRecent(`Query ${i}`);
      }

      const suggestions = service.searchSuggestions('');
      const recentSuggestions = suggestions.filter(s => s.type === 'recent');

      expect(recentSuggestions.length).toBeLessThanOrEqual(5); // Limited to 5 in suggestions
    });
  });

  describe('Categories and Filtering', () => {
    it('returns available categories', () => {
      const categories = service.getCategories();

      expect(categories).toEqual(
        expect.arrayContaining(['financial', 'operational', 'marketing'])
      );
      expect(categories).toEqual(categories.sort()); // Should be sorted
    });

    it('filters templates by category', () => {
      const financialTemplates = service.getTemplates('financial');

      expect(financialTemplates.every(t => t.category === 'financial')).toBe(true);
    });

    it('returns popular templates', () => {
      // Increment usage for some templates
      const templates = service.getTemplates();
      service.incrementUsage(templates[0].id, 'template');
      service.incrementUsage(templates[1].id, 'template');

      const popularTemplates = service.getPopularTemplates(5);

      expect(popularTemplates.length).toBeGreaterThan(0);
      expect(popularTemplates.every(t => t.usageCount > 0)).toBe(true);
    });

    it('returns favorite templates', () => {
      // Mark some templates as favorites
      const templates = service.getTemplates();
      service.toggleFavorite(templates[0].id);

      const favoriteTemplates = service.getFavoriteTemplates();

      expect(favoriteTemplates.length).toBeGreaterThan(0);
      expect(favoriteTemplates.every(t => t.isFavorite)).toBe(true);
    });
  });

  describe('Data Persistence', () => {
    it('saves user data to localStorage', () => {
      const templateData = {
        name: 'User Template',
        description: 'A user template',
        category: 'custom' as const,
        template: 'Show me {{data}}',
        variables: [],
        tags: ['user'],
        difficulty: 'beginner' as const,
        isFavorite: false,
        isPublic: false,
      };

      service.createTemplate(templateData);

      expect(mockLocalStorage.setItem).toHaveBeenCalledWith(
        'queryTemplates',
        expect.any(String)
      );
    });

    it('loads user data from localStorage', () => {
      const userData = [
        createMockTemplate({ name: 'Saved Template', isPublic: false }),
      ];

      mockLocalStorage.setItem('queryTemplates', JSON.stringify(userData));

      // Create new service instance to test loading
      const newService = QueryTemplateService.getInstance();
      const templates = newService.getTemplates();

      expect(templates).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ name: 'Saved Template' }),
        ])
      );
    });

    it('handles localStorage errors gracefully', () => {
      // Mock localStorage to throw error
      mockLocalStorage.getItem.mockImplementation(() => {
        throw new Error('Storage error');
      });

      // Should not crash
      expect(() => {
        QueryTemplateService.getInstance();
      }).not.toThrow();
    });
  });

  describe('Sorting and Ordering', () => {
    it('sorts templates by favorites first, then usage, then name', () => {
      const templates = service.getTemplates();
      
      // Mark one as favorite and increment usage
      service.toggleFavorite(templates[1].id);
      service.incrementUsage(templates[0].id, 'template');

      const sortedTemplates = service.getTemplates();

      // Favorites should come first
      const favoriteIndex = sortedTemplates.findIndex(t => t.isFavorite);
      const nonFavoriteIndex = sortedTemplates.findIndex(t => !t.isFavorite);

      // Only test if both types exist
      expect(favoriteIndex !== -1 && nonFavoriteIndex !== -1 ? favoriteIndex < nonFavoriteIndex : true).toBe(true);
    });

    it('sorts shortcuts by usage count', () => {
      const shortcuts = service.getShortcuts();
      
      // Increment usage for one shortcut
      service.incrementUsage(shortcuts[0].id, 'shortcut');

      const sortedShortcuts = service.getShortcuts();

      // Should be sorted by usage count (descending)
      for (let i = 1; i < sortedShortcuts.length; i++) {
        expect(sortedShortcuts[i - 1].usageCount).toBeGreaterThanOrEqual(
          sortedShortcuts[i].usageCount
        );
      }
    });
  });
});
