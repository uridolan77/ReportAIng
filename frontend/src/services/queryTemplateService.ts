/**
 * Query Template Service
 * Manages query shortcuts, templates, and user productivity features
 */

import { z } from 'zod';

// Template schemas
export const QueryTemplateSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string(),
  category: z.enum(['financial', 'operational', 'marketing', 'custom', 'favorites']),
  template: z.string(),
  variables: z.array(z.object({
    name: z.string(),
    type: z.enum(['string', 'number', 'date', 'select']),
    description: z.string(),
    required: z.boolean(),
    defaultValue: z.string().optional(),
    options: z.array(z.string()).optional(), // For select type
  })),
  tags: z.array(z.string()),
  difficulty: z.enum(['beginner', 'intermediate', 'advanced']),
  estimatedRows: z.number().optional(),
  executionTime: z.string().optional(),
  createdAt: z.string(),
  updatedAt: z.string(),
  usageCount: z.number().default(0),
  isFavorite: z.boolean().default(false),
  isPublic: z.boolean().default(true),
  createdBy: z.string().optional(),
});

export const QueryShortcutSchema = z.object({
  id: z.string(),
  name: z.string(),
  shortcut: z.string(), // e.g., "rev", "users", "top10"
  query: z.string(),
  description: z.string(),
  category: z.string(),
  isActive: z.boolean().default(true),
  usageCount: z.number().default(0),
  createdAt: z.string(),
});

export type QueryTemplate = z.infer<typeof QueryTemplateSchema>;
export type QueryShortcut = z.infer<typeof QueryShortcutSchema>;

export interface TemplateVariable {
  name: string;
  type: 'string' | 'number' | 'date' | 'select';
  description: string;
  required: boolean;
  defaultValue?: string;
  options?: string[];
}

export interface QuerySuggestion {
  id: string;
  text: string;
  type: 'template' | 'shortcut' | 'recent' | 'suggestion';
  category: string;
  confidence: number;
  description: string;
  variables?: TemplateVariable[];
}

export class QueryTemplateService {
  private static instance: QueryTemplateService;
  private templates: QueryTemplate[] = [];
  private shortcuts: QueryShortcut[] = [];
  private recentQueries: string[] = [];
  private favorites: Set<string> = new Set();

  private constructor() {
    this.initializeDefaultTemplates();
    this.initializeDefaultShortcuts();
    this.loadUserData();
  }

  public static getInstance(): QueryTemplateService {
    if (!QueryTemplateService.instance) {
      QueryTemplateService.instance = new QueryTemplateService();
    }
    return QueryTemplateService.instance;
  }

  /**
   * Initialize default query templates
   */
  private initializeDefaultTemplates(): void {
    this.templates = [
      {
        id: 'revenue-by-period',
        name: 'Revenue by Time Period',
        description: 'Analyze revenue trends over a specific time period',
        category: 'financial',
        template: 'Show me revenue data from {{startDate}} to {{endDate}} grouped by {{groupBy}}',
        variables: [
          {
            name: 'startDate',
            type: 'date',
            description: 'Start date for analysis',
            required: true,
            defaultValue: '2024-01-01',
          },
          {
            name: 'endDate',
            type: 'date',
            description: 'End date for analysis',
            required: true,
            defaultValue: '2024-12-31',
          },
          {
            name: 'groupBy',
            type: 'select',
            description: 'Group results by',
            required: true,
            defaultValue: 'month',
            options: ['day', 'week', 'month', 'quarter', 'year'],
          },
        ],
        tags: ['revenue', 'financial', 'trends', 'time-series'],
        difficulty: 'beginner',
        estimatedRows: 12,
        executionTime: '< 1s',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        usageCount: 0,
        isFavorite: false,
        isPublic: true,
      },
      {
        id: 'top-players-by-deposits',
        name: 'Top Players by Deposits',
        description: 'Find the highest depositing players in a given period',
        category: 'operational',
        template: 'Show me the top {{limit}} players by deposits from {{startDate}} to {{endDate}}',
        variables: [
          {
            name: 'limit',
            type: 'number',
            description: 'Number of top players to show',
            required: true,
            defaultValue: '10',
          },
          {
            name: 'startDate',
            type: 'date',
            description: 'Start date for analysis',
            required: true,
            defaultValue: '2024-01-01',
          },
          {
            name: 'endDate',
            type: 'date',
            description: 'End date for analysis',
            required: true,
            defaultValue: '2024-12-31',
          },
        ],
        tags: ['players', 'deposits', 'ranking', 'top-performers'],
        difficulty: 'beginner',
        estimatedRows: 10,
        executionTime: '< 2s',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        usageCount: 0,
        isFavorite: false,
        isPublic: true,
      },
      {
        id: 'conversion-funnel',
        name: 'Player Conversion Funnel',
        description: 'Analyze player conversion from registration to first deposit',
        category: 'marketing',
        template: 'Show me the conversion funnel for players registered in {{period}} including registration, first login, and first deposit rates',
        variables: [
          {
            name: 'period',
            type: 'select',
            description: 'Time period for analysis',
            required: true,
            defaultValue: 'last_month',
            options: ['last_week', 'last_month', 'last_quarter', 'last_year'],
          },
        ],
        tags: ['conversion', 'funnel', 'marketing', 'registration'],
        difficulty: 'intermediate',
        estimatedRows: 5,
        executionTime: '< 3s',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        usageCount: 0,
        isFavorite: false,
        isPublic: true,
      },
      {
        id: 'daily-kpis',
        name: 'Daily KPI Dashboard',
        description: 'Get key performance indicators for a specific date',
        category: 'operational',
        template: 'Show me daily KPIs for {{date}} including total deposits, withdrawals, new registrations, and active players',
        variables: [
          {
            name: 'date',
            type: 'date',
            description: 'Date for KPI analysis',
            required: true,
            defaultValue: new Date().toISOString().split('T')[0],
          },
        ],
        tags: ['kpi', 'daily', 'dashboard', 'metrics'],
        difficulty: 'beginner',
        estimatedRows: 1,
        executionTime: '< 1s',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        usageCount: 0,
        isFavorite: false,
        isPublic: true,
      },
    ];
  }

  /**
   * Initialize default query shortcuts
   */
  private initializeDefaultShortcuts(): void {
    this.shortcuts = [
      {
        id: 'rev',
        name: 'Revenue Today',
        shortcut: 'rev',
        query: 'Show me total revenue for today',
        description: 'Quick access to today\'s revenue',
        category: 'financial',
        isActive: true,
        usageCount: 0,
        createdAt: new Date().toISOString(),
      },
      {
        id: 'users',
        name: 'Active Users',
        shortcut: 'users',
        query: 'Show me active users in the last 24 hours',
        description: 'Quick access to active user count',
        category: 'operational',
        isActive: true,
        usageCount: 0,
        createdAt: new Date().toISOString(),
      },
      {
        id: 'top10',
        name: 'Top 10 Players',
        shortcut: 'top10',
        query: 'Show me the top 10 players by total deposits this month',
        description: 'Quick access to top performers',
        category: 'operational',
        isActive: true,
        usageCount: 0,
        createdAt: new Date().toISOString(),
      },
      {
        id: 'new',
        name: 'New Registrations',
        shortcut: 'new',
        query: 'Show me new player registrations today',
        description: 'Quick access to new registrations',
        category: 'marketing',
        isActive: true,
        usageCount: 0,
        createdAt: new Date().toISOString(),
      },
    ];
  }

  /**
   * Load user data from storage
   */
  private loadUserData(): void {
    try {
      const savedTemplates = localStorage.getItem('queryTemplates');
      const savedShortcuts = localStorage.getItem('queryShortcuts');
      const savedRecent = localStorage.getItem('recentQueries');
      const savedFavorites = localStorage.getItem('favoriteTemplates');

      if (savedTemplates) {
        const userTemplates = JSON.parse(savedTemplates);
        this.templates = [...this.templates, ...userTemplates];
      }

      if (savedShortcuts) {
        const userShortcuts = JSON.parse(savedShortcuts);
        this.shortcuts = [...this.shortcuts, ...userShortcuts];
      }

      if (savedRecent) {
        this.recentQueries = JSON.parse(savedRecent);
      }

      if (savedFavorites) {
        this.favorites = new Set(JSON.parse(savedFavorites));
      }
    } catch (error) {
      console.warn('Failed to load user template data:', error);
    }
  }

  /**
   * Save user data to storage
   */
  private saveUserData(): void {
    try {
      const userTemplates = this.templates.filter(t => !t.isPublic);
      const userShortcuts = this.shortcuts.filter(s => s.id.startsWith('user_'));

      localStorage.setItem('queryTemplates', JSON.stringify(userTemplates));
      localStorage.setItem('queryShortcuts', JSON.stringify(userShortcuts));
      localStorage.setItem('recentQueries', JSON.stringify(this.recentQueries));
      localStorage.setItem('favoriteTemplates', JSON.stringify([...this.favorites]));
    } catch (error) {
      console.warn('Failed to save user template data:', error);
    }
  }

  /**
   * Get all templates
   */
  public getTemplates(category?: string): QueryTemplate[] {
    let filtered = this.templates;
    
    if (category) {
      filtered = filtered.filter(t => t.category === category);
    }

    return filtered.sort((a, b) => {
      // Sort by favorites first, then usage count, then name
      if (a.isFavorite && !b.isFavorite) return -1;
      if (!a.isFavorite && b.isFavorite) return 1;
      if (a.usageCount !== b.usageCount) return b.usageCount - a.usageCount;
      return a.name.localeCompare(b.name);
    });
  }

  /**
   * Get all shortcuts
   */
  public getShortcuts(): QueryShortcut[] {
    return this.shortcuts
      .filter(s => s.isActive)
      .sort((a, b) => b.usageCount - a.usageCount);
  }

  /**
   * Search templates and shortcuts
   */
  public searchSuggestions(query: string): QuerySuggestion[] {
    const suggestions: QuerySuggestion[] = [];
    const searchTerm = query.toLowerCase().trim();

    if (!searchTerm) {
      return this.getRecentSuggestions();
    }

    // Check for exact shortcut matches
    const shortcut = this.shortcuts.find(s => 
      s.isActive && s.shortcut.toLowerCase() === searchTerm
    );

    if (shortcut) {
      suggestions.push({
        id: shortcut.id,
        text: shortcut.query,
        type: 'shortcut',
        category: shortcut.category,
        confidence: 1.0,
        description: `Shortcut: ${shortcut.name}`,
      });
    }

    // Search templates
    this.templates.forEach(template => {
      const nameMatch = template.name.toLowerCase().includes(searchTerm);
      const descMatch = template.description.toLowerCase().includes(searchTerm);
      const tagMatch = template.tags.some(tag => tag.toLowerCase().includes(searchTerm));
      
      if (nameMatch || descMatch || tagMatch) {
        const confidence = nameMatch ? 0.9 : (descMatch ? 0.7 : 0.5);
        suggestions.push({
          id: template.id,
          text: template.template,
          type: 'template',
          category: template.category,
          confidence,
          description: template.description,
          variables: template.variables,
        });
      }
    });

    // Search shortcuts by name/description
    this.shortcuts.forEach(shortcut => {
      if (shortcut.isActive && shortcut.id !== shortcut?.id) { // Avoid duplicate
        const nameMatch = shortcut.name.toLowerCase().includes(searchTerm);
        const descMatch = shortcut.description.toLowerCase().includes(searchTerm);
        
        if (nameMatch || descMatch) {
          const confidence = nameMatch ? 0.8 : 0.6;
          suggestions.push({
            id: shortcut.id,
            text: shortcut.query,
            type: 'shortcut',
            category: shortcut.category,
            confidence,
            description: shortcut.description,
          });
        }
      }
    });

    return suggestions
      .sort((a, b) => b.confidence - a.confidence)
      .slice(0, 10);
  }

  /**
   * Get recent query suggestions
   */
  private getRecentSuggestions(): QuerySuggestion[] {
    return this.recentQueries.slice(0, 5).map((query, index) => ({
      id: `recent_${index}`,
      text: query,
      type: 'recent' as const,
      category: 'recent',
      confidence: 0.3,
      description: 'Recent query',
    }));
  }

  /**
   * Process template with variables
   */
  public processTemplate(template: QueryTemplate, variables: Record<string, string>): string {
    let processed = template.template;

    template.variables.forEach(variable => {
      const value = variables[variable.name] || variable.defaultValue || '';
      const placeholder = `{{${variable.name}}}`;
      processed = processed.replace(new RegExp(placeholder, 'g'), value);
    });

    return processed;
  }

  /**
   * Add query to recent history
   */
  public addToRecent(query: string): void {
    // Remove if already exists
    this.recentQueries = this.recentQueries.filter(q => q !== query);
    
    // Add to beginning
    this.recentQueries.unshift(query);
    
    // Keep only last 20
    this.recentQueries = this.recentQueries.slice(0, 20);
    
    this.saveUserData();
  }

  /**
   * Increment usage count
   */
  public incrementUsage(id: string, type: 'template' | 'shortcut'): void {
    if (type === 'template') {
      const template = this.templates.find(t => t.id === id);
      if (template) {
        template.usageCount++;
        template.updatedAt = new Date().toISOString();
      }
    } else {
      const shortcut = this.shortcuts.find(s => s.id === id);
      if (shortcut) {
        shortcut.usageCount++;
      }
    }
    
    this.saveUserData();
  }

  /**
   * Toggle favorite status
   */
  public toggleFavorite(templateId: string): void {
    const template = this.templates.find(t => t.id === templateId);
    if (template) {
      template.isFavorite = !template.isFavorite;
      
      if (template.isFavorite) {
        this.favorites.add(templateId);
      } else {
        this.favorites.delete(templateId);
      }
      
      this.saveUserData();
    }
  }

  /**
   * Create custom template
   */
  public createTemplate(template: Omit<QueryTemplate, 'id' | 'createdAt' | 'updatedAt' | 'usageCount'>): QueryTemplate {
    const newTemplate: QueryTemplate = {
      ...template,
      id: `user_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      usageCount: 0,
      isPublic: false,
    };

    this.templates.push(newTemplate);
    this.saveUserData();
    
    return newTemplate;
  }

  /**
   * Create custom shortcut
   */
  public createShortcut(shortcut: Omit<QueryShortcut, 'id' | 'createdAt' | 'usageCount'>): QueryShortcut {
    const newShortcut: QueryShortcut = {
      ...shortcut,
      id: `user_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      createdAt: new Date().toISOString(),
      usageCount: 0,
    };

    this.shortcuts.push(newShortcut);
    this.saveUserData();
    
    return newShortcut;
  }

  /**
   * Get template categories
   */
  public getCategories(): string[] {
    const categories = new Set(this.templates.map(t => t.category));
    return Array.from(categories).sort();
  }

  /**
   * Get popular templates
   */
  public getPopularTemplates(limit: number = 5): QueryTemplate[] {
    return this.templates
      .filter(t => t.usageCount > 0)
      .sort((a, b) => b.usageCount - a.usageCount)
      .slice(0, limit);
  }

  /**
   * Get favorite templates
   */
  public getFavoriteTemplates(): QueryTemplate[] {
    return this.templates.filter(t => t.isFavorite);
  }
}

// Export singleton instance
export const queryTemplateService = QueryTemplateService.getInstance();
