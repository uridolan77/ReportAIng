using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BIReportingCopilot.ApiTester;

public class Program
{
    private static readonly HttpClient httpClient = new();
    private static string? authToken;
    private static readonly string baseUrl = "http://localhost:55244";
    private static readonly string username = "admin";
    private static readonly string password = "Admin123!";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 BI Reporting Copilot API Tester");
        Console.WriteLine("=" * 50);
        Console.WriteLine($"Base URL: {baseUrl}");
        Console.WriteLine($"Username: {username}");
        Console.WriteLine();

        try
        {
            // Authenticate
            await AuthenticateAsync();
            
            if (string.IsNullOrEmpty(authToken))
            {
                Console.WriteLine("❌ Authentication failed. Cannot proceed.");
                return;
            }

            // Parse command line arguments
            var testCostManagement = args.Contains("--cost") || args.Contains("--all");
            var testAll = args.Contains("--all");
            var verbose = args.Contains("--verbose");

            if (testCostManagement || testAll)
            {
                await TestCostManagementEndpointsAsync(verbose);
            }

            if (testAll)
            {
                await TestCoreEndpointsAsync(verbose);
                await TestLLMManagementEndpointsAsync(verbose);
                await TestBusinessContextEndpointsAsync(verbose);
            }

            if (!testCostManagement && !testAll)
            {
                Console.WriteLine("💡 Usage Examples:");
                Console.WriteLine("  dotnet run --cost        # Test Cost Management endpoints");
                Console.WriteLine("  dotnet run --all         # Test all endpoints");
                Console.WriteLine("  dotnet run --all --verbose  # Test all with detailed output");
                Console.WriteLine();
                Console.WriteLine("🔧 Testing Core Endpoints by default...");
                await TestCoreEndpointsAsync(verbose);
            }

            Console.WriteLine();
            Console.WriteLine("✅ Testing completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task AuthenticateAsync()
    {
        Console.WriteLine("🔐 Authenticating...");
        
        try
        {
            var loginData = new
            {
                email = username,
                password = password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{baseUrl}/api/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                if (loginResponse.TryGetProperty("token", out var tokenElement))
                {
                    authToken = tokenElement.GetString();
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", authToken);
                    Console.WriteLine("✅ Authentication successful!");
                }
                else
                {
                    Console.WriteLine("❌ No token received in response");
                }
            }
            else
            {
                Console.WriteLine($"❌ Authentication failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Authentication error: {ex.Message}");
        }
    }

    private static async Task<T?> MakeRequestAsync<T>(string endpoint, bool verbose = false)
    {
        try
        {
            if (verbose)
            {
                Console.WriteLine($"🔍 GET {endpoint}");
            }

            var response = await httpClient.GetAsync($"{baseUrl}{endpoint}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ GET {endpoint} - Success");
                
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)content;
                }
                
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                Console.WriteLine($"❌ GET {endpoint} - Failed ({response.StatusCode})");
                return default;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GET {endpoint} - Error: {ex.Message}");
            return default;
        }
    }

    private static async Task TestCostManagementEndpointsAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine("🧮 Testing Cost Management Endpoints");
        Console.WriteLine("=" * 50);

        // Test analytics endpoint
        var analytics = await MakeRequestAsync<JsonElement>("/api/cost-management/analytics", verbose);
        if (analytics.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("📊 Cost Analytics: Retrieved data successfully");
        }

        // Test cost trends
        var trends = await MakeRequestAsync<JsonElement>("/api/cost-management/trends?days=30", verbose);
        if (trends.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("📈 Cost Trends: Retrieved trends data");
        }

        // Test budget status
        var budget = await MakeRequestAsync<JsonElement>("/api/cost-management/budget/status", verbose);
        if (budget.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("💰 Budget Status: Retrieved budget information");
        }

        // Test cost predictions
        var predictions = await MakeRequestAsync<JsonElement>("/api/cost-management/predictions", verbose);
        if (predictions.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("🔮 Cost Predictions: Retrieved prediction data");
        }

        // Test optimization recommendations
        var recommendations = await MakeRequestAsync<JsonElement>("/api/cost-management/recommendations", verbose);
        if (recommendations.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("💡 Optimization Recommendations: Found recommendations");
        }
    }

    private static async Task TestCoreEndpointsAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine("🔧 Testing Core API Endpoints");
        Console.WriteLine("=" * 50);

        // Test health check (no auth required)
        try
        {
            var healthResponse = await httpClient.GetAsync($"{baseUrl}/health");
            if (healthResponse.IsSuccessStatusCode)
            {
                var healthContent = await healthResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❤️ Health Check: Success");
            }
            else
            {
                Console.WriteLine("❌ Health Check: Failed");
            }
        }
        catch
        {
            Console.WriteLine("❌ Health Check: Failed");
        }

        // Test user profile
        var profile = await MakeRequestAsync<JsonElement>("/api/auth/profile", verbose);
        if (profile.ValueKind != JsonValueKind.Undefined)
        {
            if (profile.TryGetProperty("email", out var emailElement))
            {
                Console.WriteLine($"👤 User Profile: {emailElement.GetString()}");
            }
        }

        // Test features
        var features = await MakeRequestAsync<JsonElement>("/api/features", verbose);
        if (features.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("🎯 Features: Retrieved features data");
        }

        // Test schema endpoints
        var schema = await MakeRequestAsync<JsonElement>("/api/schema/tables", verbose);
        if (schema.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("🗄️ Schema Tables: Retrieved schema data");
        }
    }

    private static async Task TestLLMManagementEndpointsAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine("🤖 Testing LLM Management Endpoints");
        Console.WriteLine("=" * 50);

        // Test providers
        var providers = await MakeRequestAsync<JsonElement>("/api/llm-management/providers", verbose);
        if (providers.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("🏭 LLM Providers: Retrieved providers data");
        }

        // Test models
        var models = await MakeRequestAsync<JsonElement>("/api/llm-management/models", verbose);
        if (models.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("🧠 LLM Models: Retrieved models data");
        }

        // Test usage logs
        var usage = await MakeRequestAsync<JsonElement>("/api/llm-management/usage?limit=10", verbose);
        if (usage.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("📋 Usage Logs: Retrieved usage data");
        }

        // Test performance metrics
        var performance = await MakeRequestAsync<JsonElement>("/api/llm-management/performance", verbose);
        if (performance.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("⚡ Performance Metrics: Retrieved metrics");
        }
    }

    private static async Task TestBusinessContextEndpointsAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine("📊 Testing Business Context Endpoints");
        Console.WriteLine("=" * 50);

        // Test business metadata
        var metadata = await MakeRequestAsync<JsonElement>("/api/business-metadata/tables", verbose);
        if (metadata.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("📋 Business Metadata: Retrieved metadata");
        }

        // Test glossary
        var glossary = await MakeRequestAsync<JsonElement>("/api/business-metadata/glossary", verbose);
        if (glossary.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("📖 Business Glossary: Retrieved glossary data");
        }

        // Test auto-generation status
        var autoGen = await MakeRequestAsync<JsonElement>("/api/business-metadata/auto-generation/status", verbose);
        if (autoGen.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine("🤖 Auto-generation Status: Retrieved status");
        }
    }
}
