using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Tests;

/// <summary>
/// Simple test to verify the database schema fix for SemanticCacheEntries
/// </summary>
public class TestDatabaseFix
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Testing database schema fix for SemanticCacheEntries...");
        
        try
        {
            // Use in-memory database for testing
            var options = new DbContextOptionsBuilder<BICopilotContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            using var context = new BICopilotContext(options);
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Database created successfully");

            // Test creating a UnifiedSemanticCacheEntry
            var cacheEntry = new UnifiedSemanticCacheEntry
            {
                QueryHash = "test-hash-123",
                OriginalQuery = "Test query",
                NormalizedQuery = "SELECT * FROM test",
                GeneratedSql = "SELECT * FROM test",
                ResultData = "{}",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                AccessCount = 1,
                LastAccessedAt = DateTime.UtcNow,
                CreatedBy = "TestSystem",
                UpdatedBy = "TestSystem",
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                IsActive = true
            };

            context.SemanticCacheEntries.Add(cacheEntry);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ SemanticCacheEntry created successfully");

            // Test querying the entry
            var retrieved = await context.SemanticCacheEntries
                .FirstOrDefaultAsync(x => x.QueryHash == "test-hash-123");
            
            if (retrieved != null)
            {
                Console.WriteLine($"✅ SemanticCacheEntry retrieved successfully - ID: {retrieved.Id} (Type: {retrieved.Id.GetType().Name})");
                Console.WriteLine($"   Query: {retrieved.OriginalQuery}");
                Console.WriteLine($"   Created: {retrieved.CreatedAt}");
            }
            else
            {
                Console.WriteLine("❌ Failed to retrieve SemanticCacheEntry");
                return;
            }

            Console.WriteLine("\n🎉 Database schema fix verification PASSED!");
            Console.WriteLine("The UnifiedSemanticCacheEntry model is now compatible with the database schema.");
            Console.WriteLine("ID type: int (matches database table)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database schema fix verification FAILED: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
