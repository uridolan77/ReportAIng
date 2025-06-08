import { querySuggestionService, CreateUpdateCategory, CreateUpdateSuggestion } from '../services/querySuggestionService';
import { suggestionCategories } from '../components/QueryInterface/QuerySuggestions';

/**
 * Utility to sync the hardcoded suggestions to the database
 * This will replace all existing suggestions with the improved ones from the frontend
 */
export class SuggestionDatabaseSync {
  
  /**
   * Main sync function - clears existing and creates new suggestions
   */
  static async syncSuggestionsToDatabase(): Promise<void> {
    try {
      console.log('🔄 Starting suggestion database sync...');
      
      // Step 1: Get existing categories and suggestions
      const existingCategories = await querySuggestionService.getCategories(true);
      const existingGroupedSuggestions = await querySuggestionService.getGroupedSuggestions(true);
      
      console.log(`📊 Found ${existingCategories.length} existing categories`);
      console.log(`📝 Found ${existingGroupedSuggestions.reduce((sum, group) => sum + group.suggestions.length, 0)} existing suggestions`);
      
      // Step 2: Delete existing suggestions
      for (const group of existingGroupedSuggestions) {
        for (const suggestion of group.suggestions) {
          await querySuggestionService.deleteSuggestion(suggestion.id);
          console.log(`🗑️ Deleted suggestion: ${suggestion.description}`);
        }
      }
      
      // Step 3: Delete existing categories
      for (const category of existingCategories) {
        await querySuggestionService.deleteCategory(category.id);
        console.log(`🗑️ Deleted category: ${category.title}`);
      }
      
      // Step 4: Create new categories and suggestions
      const categoryMap = new Map<string, number>();
      
      for (let i = 0; i < suggestionCategories.length; i++) {
        const categoryData = suggestionCategories[i];
        
        // Create category
        const newCategory: CreateUpdateCategory = {
          categoryKey: categoryData.categoryKey,
          title: categoryData.title,
          icon: this.getIconName(categoryData.icon),
          description: `${categoryData.title} related queries and insights`,
          sortOrder: i + 1,
          isActive: true
        };
        
        const createdCategory = await querySuggestionService.createCategory(newCategory);
        categoryMap.set(categoryData.categoryKey, createdCategory.id);
        console.log(`✅ Created category: ${createdCategory.title} (ID: ${createdCategory.id})`);
        
        // Create suggestions for this category
        for (let j = 0; j < categoryData.suggestions.length; j++) {
          const suggestionText = categoryData.suggestions[j];
          
          const newSuggestion: CreateUpdateSuggestion = {
            categoryId: createdCategory.id,
            queryText: suggestionText,
            description: suggestionText,
            defaultTimeFrame: this.getDefaultTimeFrame(suggestionText),
            sortOrder: j + 1,
            isActive: true,
            targetTables: this.getTargetTables(suggestionText),
            complexity: this.getComplexity(suggestionText),
            requiredPermissions: ['query:execute'],
            tags: this.getTags(suggestionText, categoryData.categoryKey)
          };
          
          const createdSuggestion = await querySuggestionService.createSuggestion(newSuggestion);
          console.log(`  ✅ Created suggestion: ${createdSuggestion.description} (ID: ${createdSuggestion.id})`);
        }
      }
      
      console.log('🎉 Suggestion database sync completed successfully!');
      
      // Step 5: Verify the sync
      const newCategories = await querySuggestionService.getCategories();
      const newGroupedSuggestions = await querySuggestionService.getGroupedSuggestions();
      
      console.log(`✅ Verification: ${newCategories.length} categories created`);
      console.log(`✅ Verification: ${newGroupedSuggestions.reduce((sum, group) => sum + group.suggestions.length, 0)} suggestions created`);
      
    } catch (error) {
      console.error('❌ Error syncing suggestions to database:', error);
      throw error;
    }
  }
  
  /**
   * Get icon name from React icon component
   */
  private static getIconName(iconComponent: React.ReactElement): string {
    const iconType = iconComponent.type as any;
    return iconType.displayName || iconType.name || 'BarChartOutlined';
  }
  
  /**
   * Determine default time frame based on suggestion text
   */
  private static getDefaultTimeFrame(suggestionText: string): string | undefined {
    const text = suggestionText.toLowerCase();
    
    if (text.includes('yesterday')) return 'yesterday';
    if (text.includes('last week') || text.includes('last 7 days')) return 'last-7-days';
    if (text.includes('last 3 days')) return 'last-3-days';
    if (text.includes('last 30 days')) return 'last-30-days';
    if (text.includes('this quarter')) return 'this-quarter';
    if (text.includes('this month')) return 'this-month';
    
    return 'last-7-days'; // Default fallback
  }
  
  /**
   * Determine target tables based on suggestion content
   */
  private static getTargetTables(suggestionText: string): string[] {
    const text = suggestionText.toLowerCase();
    const tables: string[] = [];
    
    if (text.includes('deposit') || text.includes('revenue')) {
      tables.push('transactions', 'deposits');
    }
    if (text.includes('player') || text.includes('user') || text.includes('registration')) {
      tables.push('users', 'players');
    }
    if (text.includes('bet') || text.includes('casino') || text.includes('sports')) {
      tables.push('bets', 'games');
    }
    if (text.includes('bonus')) {
      tables.push('bonuses', 'promotions');
    }
    if (text.includes('country') || text.includes('region')) {
      tables.push('users', 'geography');
    }
    
    return tables.length > 0 ? tables : ['transactions']; // Default fallback
  }
  
  /**
   * Determine complexity based on suggestion content
   */
  private static getComplexity(suggestionText: string): number {
    const text = suggestionText.toLowerCase();
    
    // Complex queries (3-4)
    if (text.includes('breakdown by') || text.includes('vs ') || text.includes('top ')) {
      return 3;
    }
    
    // Medium queries (2)
    if (text.includes('total') || text.includes('count') || text.includes('show me')) {
      return 2;
    }
    
    // Simple queries (1)
    return 1;
  }
  
  /**
   * Generate tags based on suggestion content and category
   */
  private static getTags(suggestionText: string, categoryKey: string): string[] {
    const text = suggestionText.toLowerCase();
    const tags: string[] = [categoryKey];
    
    // Add content-based tags
    if (text.includes('revenue') || text.includes('deposit')) tags.push('revenue', 'financial');
    if (text.includes('player') || text.includes('user')) tags.push('players', 'users');
    if (text.includes('yesterday')) tags.push('daily', 'recent');
    if (text.includes('week') || text.includes('7 days')) tags.push('weekly');
    if (text.includes('top ')) tags.push('ranking', 'top-performers');
    if (text.includes('country')) tags.push('geography', 'regional');
    if (text.includes('casino')) tags.push('casino', 'gaming');
    if (text.includes('sports')) tags.push('sports-betting', 'sportsbook');
    if (text.includes('bonus')) tags.push('bonuses', 'promotions');
    if (text.includes('registration')) tags.push('acquisition', 'new-users');
    if (text.includes('activity')) tags.push('engagement', 'activity');
    if (text.includes('transaction')) tags.push('transactions', 'payments');
    
    return [...new Set(tags)]; // Remove duplicates
  }
}

/**
 * Convenience function to run the sync
 */
export const syncSuggestionsToDatabase = () => SuggestionDatabaseSync.syncSuggestionsToDatabase();
