using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BIReportingCopilot.DatabaseFix
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting database schema fix...");
                
                // Read configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("BIReportingCopilot.API/appsettings.json", optional: false)
                    .AddJsonFile("BIReportingCopilot.API/appsettings.Development.json", optional: true)
                    .Build();
                
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("ERROR: Connection string not found in configuration.");
                    return;
                }
                
                Console.WriteLine($"Using connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
                
                // Read SQL script
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "database", "migrations", "FixQuerySuggestionsSchema.sql");
                
                if (!File.Exists(scriptPath))
                {
                    Console.WriteLine($"ERROR: SQL script not found at: {scriptPath}");
                    return;
                }
                
                var sqlScript = await File.ReadAllTextAsync(scriptPath);
                Console.WriteLine($"Loaded SQL script ({sqlScript.Length} characters)");
                
                // Execute SQL script
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                Console.WriteLine("Connected to database successfully.");
                
                // Split script by GO statements and execute each batch
                var batches = sqlScript.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n", "\r\nGO\n" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var batch in batches)
                {
                    var trimmedBatch = batch.Trim();
                    if (string.IsNullOrEmpty(trimmedBatch) || trimmedBatch.Equals("GO", StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    try
                    {
                        using var command = new SqlCommand(trimmedBatch, connection);
                        command.CommandTimeout = 60; // 60 seconds timeout
                        
                        var result = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Executed batch successfully. Rows affected: {result}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing batch: {ex.Message}");
                        Console.WriteLine($"Batch content: {trimmedBatch.Substring(0, Math.Min(100, trimmedBatch.Length))}...");
                    }
                }
                
                Console.WriteLine("Database schema fix completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
