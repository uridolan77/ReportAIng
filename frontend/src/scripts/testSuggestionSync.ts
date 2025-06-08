/**
 * Test script for suggestion database sync
 * Run this to test the sync functionality from command line
 */

import { syncSuggestionsToDatabase } from '../utils/syncSuggestionsToDatabase';
import { suggestionCategories } from '../components/QueryInterface/QuerySuggestions';

async function testSync() {
  console.log('🧪 Testing Suggestion Database Sync');
  console.log('=====================================');
  
  console.log('\n📋 Preview of suggestions to be synced:');
  suggestionCategories.forEach((category, index) => {
    console.log(`\n${index + 1}. ${category.title} (${category.categoryKey})`);
    category.suggestions.forEach((suggestion, suggestionIndex) => {
      console.log(`   ${suggestionIndex + 1}. ${suggestion}`);
    });
  });
  
  console.log('\n🔄 Starting sync process...');
  
  try {
    await syncSuggestionsToDatabase();
    console.log('\n✅ Sync completed successfully!');
    process.exit(0);
  } catch (error) {
    console.error('\n❌ Sync failed:', error);
    process.exit(1);
  }
}

// Run the test if this script is executed directly
if (require.main === module) {
  testSync();
}

export { testSync };
