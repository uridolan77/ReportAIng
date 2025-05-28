using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing BI Reporting Copilot Health Check...");
        
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            // Test health check endpoint
            var healthResponse = await client.GetAsync("https://localhost:7001/health");
            var healthContent = await healthResponse.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Health Check Status: {healthResponse.StatusCode}");
            Console.WriteLine($"Health Check Response: {healthContent}");
            
            if (healthResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Application health check passed!");
            }
            else
            {
                Console.WriteLine("❌ Application health check failed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error testing health check: {ex.Message}");
            Console.WriteLine("Note: Make sure the application is running on https://localhost:7001");
        }
    }
}
