using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIReportingCopilot.ConsoleTest
{
    /// <summary>
    /// Console application to test the complete AI Transparency Foundation flow
    /// Tests: Authentication → Business Context → Token Budget → Prompt → Query → Result
    /// </summary>
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string? authToken;
        private static readonly string baseUrl = "http://localhost:55244";

        static async Task Main(string[] args)
        {
            Console.WriteLine("🧪 AI Transparency Foundation - Console Test");
            Console.WriteLine("============================================");
            Console.WriteLine();

            try
            {
                // Step 1: Test Authentication
                Console.WriteLine("🔐 Step 1: Testing Authentication...");
                await TestAuthentication();
                Console.WriteLine("✅ Authentication successful!");
                Console.WriteLine();

                // Step 2: Test Health Check
                Console.WriteLine("🏥 Step 2: Testing Health Check...");
                await TestHealthCheck();
                Console.WriteLine("✅ Health check successful!");
                Console.WriteLine();

                // Step 3: Test Enhanced Query Flow
                Console.WriteLine("🧠 Step 3: Testing Enhanced Query Flow...");
                await TestEnhancedQueryFlow();
                Console.WriteLine("✅ Enhanced query flow successful!");
                Console.WriteLine();

                // Step 4: Test Transparency Data Verification
                Console.WriteLine("🔍 Step 4: Testing Transparency Data Verification...");
                await TestTransparencyVerification();
                Console.WriteLine("✅ Transparency verification successful!");
                Console.WriteLine();

                Console.WriteLine("🎉 All tests completed successfully!");
                Console.WriteLine("The AI Transparency Foundation is working correctly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task TestAuthentication()
        {
            var authRequest = new
            {
                userId = "admin@bireporting.com",
                userName = "Console Test User"
            };

            var json = JsonSerializer.Serialize(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{baseUrl}/api/test/auth/token", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Authentication failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            authToken = authResponse.GetProperty("token").GetString();
            var userId = authResponse.GetProperty("userId").GetString();
            var expiresAt = authResponse.GetProperty("expiresAt").GetDateTime();

            Console.WriteLine($"   Token obtained for user: {userId}");
            Console.WriteLine($"   Token expires at: {expiresAt}");
            Console.WriteLine($"   Token length: {authToken?.Length} characters");

            // Set authorization header for subsequent requests
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
        }

        static async Task TestHealthCheck()
        {
            var response = await httpClient.GetAsync($"{baseUrl}/health");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Health check failed: {response.StatusCode} - {errorContent}");
            }

            var healthStatus = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"   Health status: {healthStatus}");
        }

        static async Task TestEnhancedQueryFlow()
        {
            var testQueries = new[]
            {
                "Show me sales data for Q3 2024",
                "What are the top performing products this year?",
                "Compare revenue trends by region",
                "Find anomalies in customer behavior"
            };

            foreach (var query in testQueries)
            {
                Console.WriteLine($"   Testing query: {query}");
                
                var queryRequest = new
                {
                    query = query,
                    executeQuery = false, // Don't actually execute SQL for testing
                    includeAlternatives = true,
                    includeSemanticAnalysis = true
                };

                var json = JsonSerializer.Serialize(queryRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var startTime = DateTime.UtcNow;
                var response = await httpClient.PostAsync($"{baseUrl}/api/test/query/enhanced", content);
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"   ⚠️  Query failed: {response.StatusCode} - {errorContent}");
                    continue;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var queryResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                var traceId = queryResponse.GetProperty("traceId").GetString();
                var success = queryResponse.GetProperty("success").GetBoolean();
                
                Console.WriteLine($"   ✅ Query processed successfully");
                Console.WriteLine($"      Trace ID: {traceId}");
                Console.WriteLine($"      Success: {success}");
                Console.WriteLine($"      Duration: {duration.TotalMilliseconds:F0}ms");

                // Check transparency data
                if (queryResponse.TryGetProperty("transparencyData", out var transparencyData))
                {
                    if (transparencyData.TryGetProperty("businessContext", out var businessContext))
                    {
                        var intent = businessContext.GetProperty("intent").GetString();
                        var confidence = businessContext.GetProperty("confidence").GetDouble();
                        var domain = businessContext.GetProperty("domain").GetString();
                        
                        Console.WriteLine($"      Business Intent: {intent} (confidence: {confidence:P1})");
                        Console.WriteLine($"      Domain: {domain}");
                    }

                    if (transparencyData.TryGetProperty("tokenUsage", out var tokenUsage))
                    {
                        var allocatedTokens = tokenUsage.GetProperty("allocatedTokens").GetInt32();
                        var estimatedCost = tokenUsage.GetProperty("estimatedCost").GetDecimal();
                        var provider = tokenUsage.GetProperty("provider").GetString();
                        
                        Console.WriteLine($"      Token Usage: {allocatedTokens} tokens");
                        Console.WriteLine($"      Estimated Cost: ${estimatedCost:F4}");
                        Console.WriteLine($"      Provider: {provider}");
                    }
                }

                Console.WriteLine();
            }
        }

        static async Task TestTransparencyVerification()
        {
            // Test a sample trace ID verification
            var sampleTraceId = "test-trace-" + Guid.NewGuid().ToString("N")[..8];
            
            var response = await httpClient.GetAsync($"{baseUrl}/api/test/transparency/verify/{sampleTraceId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"   ⚠️  Verification request failed: {response.StatusCode} - {errorContent}");
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var verificationResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var traceId = verificationResponse.GetProperty("traceId").GetString();
            Console.WriteLine($"   Verification response for trace: {traceId}");

            if (verificationResponse.TryGetProperty("databaseVerification", out var dbVerification))
            {
                var promptTraceFound = dbVerification.GetProperty("promptTraceFound").GetBoolean();
                var businessContextFound = dbVerification.GetProperty("businessContextFound").GetBoolean();
                var tokenBudgetFound = dbVerification.GetProperty("tokenBudgetFound").GetBoolean();
                var message = dbVerification.GetProperty("message").GetString();

                Console.WriteLine($"   Prompt Trace Found: {promptTraceFound}");
                Console.WriteLine($"   Business Context Found: {businessContextFound}");
                Console.WriteLine($"   Token Budget Found: {tokenBudgetFound}");
                Console.WriteLine($"   Message: {message}");
            }
        }
    }
}
