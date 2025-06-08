/**
 * Query Templates Hook
 * Manages query templates, shortcuts, and user productivity features
 */

import { useState, useEffect, useCallback, useMemo } from 'react';
import { message } from 'antd';
import {
  queryTemplateService,
  QueryTemplate,
  QueryShortcut,
  QuerySuggestion
} from '../services/queryTemplateService';

export interface UseQueryTemplatesReturn {
  // Templates
  templates: QueryTemplate[];
  filteredTemplates: QueryTemplate[];
  popularTemplates: QueryTemplate[];
  favoriteTemplates: QueryTemplate[];
  
  // Shortcuts
  shortcuts: QueryShortcut[];
  
  // Suggestions
  suggestions: QuerySuggestion[];
  
  // Categories
  categories: string[];
  selectedCategory: string;
  setSelectedCategory: (category: string) => void;
  
  // Search
  searchTerm: string;
  setSearchTerm: (term: string) => void;
  
  // Actions
  createTemplate: (template: Omit<QueryTemplate, 'id' | 'createdAt' | 'updatedAt' | 'usageCount'>) => Promise<QueryTemplate>;
  createShortcut: (shortcut: Omit<QueryShortcut, 'id' | 'createdAt' | 'usageCount'>) => Promise<QueryShortcut>;
  toggleFavorite: (templateId: string) => void;
  incrementUsage: (id: string, type: 'template' | 'shortcut') => void;
  processTemplate: (template: QueryTemplate, variables: Record<string, string>) => string;
  addToRecent: (query: string) => void;
  
  // State
  loading: boolean;
  error: string | null;
  
  // Utilities
  getSuggestions: (query: string) => QuerySuggestion[];
  getTemplateById: (id: string) => QueryTemplate | undefined;
  getShortcutById: (id: string) => QueryShortcut | undefined;
}

export const useQueryTemplates = (): UseQueryTemplatesReturn => {
  const [templates, setTemplates] = useState<QueryTemplate[]>([]);
  const [shortcuts, setShortcuts] = useState<QueryShortcut[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Load initial data
  useEffect(() => {
    try {
      setLoading(true);
      setTemplates(queryTemplateService.getTemplates());
      setShortcuts(queryTemplateService.getShortcuts());
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load templates');
      message.error('Failed to load query templates');
    } finally {
      setLoading(false);
    }
  }, []);

  // Filtered templates based on category and search
  const filteredTemplates = useMemo(() => {
    let filtered = templates;

    // Filter by category
    if (selectedCategory === 'favorites') {
      filtered = queryTemplateService.getFavoriteTemplates();
    } else if (selectedCategory !== 'all') {
      filtered = filtered.filter(t => t.category === selectedCategory);
    }

    // Filter by search term
    if (searchTerm.trim()) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(template => 
        template.name.toLowerCase().includes(term) ||
        template.description.toLowerCase().includes(term) ||
        template.tags.some(tag => tag.toLowerCase().includes(term)) ||
        template.template.toLowerCase().includes(term)
      );
    }

    return filtered;
  }, [templates, selectedCategory, searchTerm]);

  // Popular templates
  const popularTemplates = useMemo(() => {
    return queryTemplateService.getPopularTemplates();
  }, [templates]);

  // Favorite templates
  const favoriteTemplates = useMemo(() => {
    return queryTemplateService.getFavoriteTemplates();
  }, [templates]);

  // Categories
  const categories = useMemo(() => {
    return queryTemplateService.getCategories();
  }, [templates]);

  // Suggestions based on search term
  const suggestions = useMemo(() => {
    return queryTemplateService.searchSuggestions(searchTerm);
  }, [searchTerm, templates, shortcuts]);

  // Create template
  const createTemplate = useCallback(async (
    templateData: Omit<QueryTemplate, 'id' | 'createdAt' | 'updatedAt' | 'usageCount'>
  ): Promise<QueryTemplate> => {
    try {
      setLoading(true);
      const newTemplate = queryTemplateService.createTemplate(templateData);
      setTemplates(queryTemplateService.getTemplates());
      message.success('Template created successfully!');
      return newTemplate;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create template';
      setError(errorMessage);
      message.error(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  // Create shortcut
  const createShortcut = useCallback(async (
    shortcutData: Omit<QueryShortcut, 'id' | 'createdAt' | 'usageCount'>
  ): Promise<QueryShortcut> => {
    try {
      setLoading(true);
      const newShortcut = queryTemplateService.createShortcut(shortcutData);
      setShortcuts(queryTemplateService.getShortcuts());
      message.success('Shortcut created successfully!');
      return newShortcut;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create shortcut';
      setError(errorMessage);
      message.error(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  // Toggle favorite
  const toggleFavorite = useCallback((templateId: string) => {
    try {
      queryTemplateService.toggleFavorite(templateId);
      setTemplates(queryTemplateService.getTemplates());
      
      const template = templates.find(t => t.id === templateId);
      if (template) {
        const action = template.isFavorite ? 'removed from' : 'added to';
        message.success(`Template ${action} favorites`);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to toggle favorite';
      setError(errorMessage);
      message.error(errorMessage);
    }
  }, [templates]);

  // Increment usage
  const incrementUsage = useCallback((id: string, type: 'template' | 'shortcut') => {
    try {
      queryTemplateService.incrementUsage(id, type);
      
      if (type === 'template') {
        setTemplates(queryTemplateService.getTemplates());
      } else {
        setShortcuts(queryTemplateService.getShortcuts());
      }
    } catch (err) {
      console.warn('Failed to increment usage:', err);
    }
  }, []);

  // Process template with variables
  const processTemplate = useCallback((
    template: QueryTemplate, 
    variables: Record<string, string>
  ): string => {
    try {
      return queryTemplateService.processTemplate(template, variables);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to process template';
      setError(errorMessage);
      message.error(errorMessage);
      return template.template;
    }
  }, []);

  // Add to recent queries
  const addToRecent = useCallback((query: string) => {
    try {
      queryTemplateService.addToRecent(query);
    } catch (err) {
      console.warn('Failed to add to recent queries:', err);
    }
  }, []);

  // Get suggestions for a query
  const getSuggestions = useCallback((query: string): QuerySuggestion[] => {
    try {
      return queryTemplateService.searchSuggestions(query);
    } catch (err) {
      console.warn('Failed to get suggestions:', err);
      return [];
    }
  }, []);

  // Get template by ID
  const getTemplateById = useCallback((id: string): QueryTemplate | undefined => {
    return templates.find(t => t.id === id);
  }, [templates]);

  // Get shortcut by ID
  const getShortcutById = useCallback((id: string): QueryShortcut | undefined => {
    return shortcuts.find(s => s.id === id);
  }, [shortcuts]);

  return {
    // Templates
    templates,
    filteredTemplates,
    popularTemplates,
    favoriteTemplates,
    
    // Shortcuts
    shortcuts,
    
    // Suggestions
    suggestions,
    
    // Categories
    categories,
    selectedCategory,
    setSelectedCategory,
    
    // Search
    searchTerm,
    setSearchTerm,
    
    // Actions
    createTemplate,
    createShortcut,
    toggleFavorite,
    incrementUsage,
    processTemplate,
    addToRecent,
    
    // State
    loading,
    error,
    
    // Utilities
    getSuggestions,
    getTemplateById,
    getShortcutById,
  };
};

// Hook for template analytics
export const useTemplateAnalytics = () => {
  const [analytics, setAnalytics] = useState({
    totalTemplates: 0,
    totalShortcuts: 0,
    totalUsage: 0,
    popularCategories: [] as { category: string; count: number }[],
    recentActivity: [] as { type: string; name: string; timestamp: string }[],
  });

  useEffect(() => {
    const templates = queryTemplateService.getTemplates();
    const shortcuts = queryTemplateService.getShortcuts();
    
    const totalUsage = templates.reduce((sum, t) => sum + t.usageCount, 0) +
                      shortcuts.reduce((sum, s) => sum + s.usageCount, 0);

    const categoryCount = templates.reduce((acc, template) => {
      acc[template.category] = (acc[template.category] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    const popularCategories = Object.entries(categoryCount)
      .map(([category, count]) => ({ category, count }))
      .sort((a, b) => b.count - a.count);

    setAnalytics({
      totalTemplates: templates.length,
      totalShortcuts: shortcuts.length,
      totalUsage,
      popularCategories,
      recentActivity: [], // Would be populated from actual usage logs
    });
  }, []);

  return analytics;
};

// Hook for keyboard shortcuts
export const useQueryKeyboardShortcuts = (
  onSubmit: () => void,
  onClear: () => void,
  onFocus: () => void
) => {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      // Ctrl/Cmd + Enter to submit
      if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') {
        event.preventDefault();
        onSubmit();
      }
      
      // Ctrl/Cmd + K to focus search
      if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
        event.preventDefault();
        onFocus();
      }
      
      // Escape to clear
      if (event.key === 'Escape') {
        onClear();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onSubmit, onClear, onFocus]);
};
