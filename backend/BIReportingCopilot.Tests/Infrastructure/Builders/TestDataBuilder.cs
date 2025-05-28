using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Data;
using System.Reflection;

namespace BIReportingCopilot.Tests.Infrastructure.Builders;

/// <summary>
/// Generic test data builder for creating test entities with fluent API
/// </summary>
public class TestDataBuilder<T> where T : class, new()
{
    private readonly T _instance;
    private readonly Dictionary<string, object> _propertyValues;
    private readonly Random _random;

    public TestDataBuilder()
    {
        _instance = new T();
        _propertyValues = new Dictionary<string, object>();
        _random = new Random();
        SetDefaultValues();
    }

    /// <summary>
    /// Set a property value using fluent API
    /// </summary>
    public TestDataBuilder<T> With<TProperty>(string propertyName, TProperty value)
    {
        _propertyValues[propertyName] = value!;
        return this;
    }

    /// <summary>
    /// Set a property value using expression
    /// </summary>
    public TestDataBuilder<T> With<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> propertyExpression, TProperty value)
    {
        var propertyName = GetPropertyName(propertyExpression);
        return With(propertyName, value);
    }

    /// <summary>
    /// Set multiple properties from an anonymous object
    /// </summary>
    public TestDataBuilder<T> With(object values)
    {
        var properties = values.GetType().GetProperties();
        foreach (var property in properties)
        {
            _propertyValues[property.Name] = property.GetValue(values)!;
        }
        return this;
    }

    /// <summary>
    /// Apply a custom configuration action
    /// </summary>
    public TestDataBuilder<T> Configure(Action<T> configureAction)
    {
        configureAction(_instance);
        return this;
    }

    /// <summary>
    /// Build the final instance
    /// </summary>
    public T Build()
    {
        ApplyPropertyValues();
        return _instance;
    }

    /// <summary>
    /// Build multiple instances with slight variations
    /// </summary>
    public List<T> BuildMany(int count, Action<TestDataBuilder<T>, int>? customizer = null)
    {
        var results = new List<T>();

        for (int i = 0; i < count; i++)
        {
            var builder = new TestDataBuilder<T>();

            // Copy current property values
            foreach (var kvp in _propertyValues)
            {
                builder._propertyValues[kvp.Key] = kvp.Value;
            }

            // Apply customization
            customizer?.Invoke(builder, i);

            results.Add(builder.Build());
        }

        return results;
    }

    private void SetDefaultValues()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanWrite)
                continue;

            var defaultValue = GenerateDefaultValue(property);
            if (defaultValue != null)
            {
                _propertyValues[property.Name] = defaultValue;
            }
        }
    }

    private object? GenerateDefaultValue(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return underlyingType.Name switch
        {
            nameof(String) => GenerateStringValue(property.Name),
            nameof(Int32) => _random.Next(1, 1000),
            nameof(Int64) => (long)_random.Next(1, 1000),
            nameof(Double) => _random.NextDouble() * 100,
            nameof(Decimal) => (decimal)(_random.NextDouble() * 100),
            nameof(Boolean) => _random.Next(2) == 1,
            nameof(DateTime) => DateTime.UtcNow.AddDays(_random.Next(-30, 30)),
            nameof(Guid) => Guid.NewGuid(),
            _ when underlyingType.IsEnum => GenerateEnumValue(underlyingType),
            _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>) =>
                Activator.CreateInstance(propertyType),
            _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) =>
                Activator.CreateInstance(propertyType),
            _ => null
        };
    }

    private string GenerateStringValue(string propertyName)
    {
        return propertyName.ToLower() switch
        {
            var name when name.Contains("id") => Guid.NewGuid().ToString(),
            var name when name.Contains("name") => $"Test{propertyName}{_random.Next(100, 999)}",
            var name when name.Contains("email") => $"test{_random.Next(100, 999)}@example.com",
            var name when name.Contains("description") => $"Test description for {propertyName}",
            var name when name.Contains("sql") || name.Contains("query") => "SELECT * FROM TestTable",
            var name when name.Contains("url") || name.Contains("uri") => $"https://example.com/{propertyName.ToLower()}",
            var name when name.Contains("phone") => $"+1-555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
            var name when name.Contains("address") => $"{_random.Next(100, 999)} Test Street",
            var name when name.Contains("code") => $"CODE{_random.Next(1000, 9999)}",
            _ => $"Test{propertyName}{_random.Next(100, 999)}"
        };
    }

    private object GenerateEnumValue(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        return values.GetValue(_random.Next(values.Length))!;
    }

    private void ApplyPropertyValues()
    {
        foreach (var kvp in _propertyValues)
        {
            var property = typeof(T).GetProperty(kvp.Key);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_instance, kvp.Value);
            }
        }
    }

    private string GetPropertyName<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is System.Linq.Expressions.MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));
    }
}

/// <summary>
/// Specialized builder for common test entities
/// </summary>
public static class TestDataBuilders
{
    public static TestDataBuilder<UserEntity> User()
    {
        return new TestDataBuilder<UserEntity>()
            .With(u => u.Id, Guid.NewGuid().ToString())
            .With(u => u.Username, $"testuser{Random.Shared.Next(1000, 9999)}")
            .With(u => u.Email, $"test{Random.Shared.Next(1000, 9999)}@example.com")
            .With(u => u.DisplayName, "Test User")
            .With(u => u.PasswordHash, "hashed_password")
            .With(u => u.Roles, "User")
            .With(u => u.IsActive, true)
            .With(u => u.CreatedDate, DateTime.UtcNow)
            .With(u => u.CreatedBy, "System");
    }

    public static TestDataBuilder<BIReportingCopilot.Infrastructure.Data.Entities.QueryHistoryEntity> QueryHistory()
    {
        return new TestDataBuilder<BIReportingCopilot.Infrastructure.Data.Entities.QueryHistoryEntity>()
            .With(q => q.Id, Random.Shared.NextInt64(1, long.MaxValue))
            .With(q => q.UserId, Guid.NewGuid().ToString())
            .With(q => q.SessionId, Guid.NewGuid().ToString())
            .With(q => q.NaturalLanguageQuery, "Show me all customers")
            .With(q => q.GeneratedSQL, "SELECT * FROM Customers")
            .With(q => q.IsSuccessful, true)
            .With(q => q.ExecutionTimeMs, Random.Shared.Next(100, 5000))
            .With(q => q.ResultRowCount, Random.Shared.Next(1, 1000))
            .With(q => q.QueryTimestamp, DateTime.UtcNow);
    }

    public static TestDataBuilder<BusinessTableInfoEntity> BusinessTableInfo()
    {
        return new TestDataBuilder<BusinessTableInfoEntity>()
            .With(t => t.Id, Random.Shared.NextInt64(1, long.MaxValue))
            .With(t => t.SchemaName, "dbo")
            .With(t => t.TableName, $"TestTable{Random.Shared.Next(100, 999)}")
            .With(t => t.BusinessPurpose, "Test table for unit testing")
            .With(t => t.BusinessContext, "Internal business table")
            .With(t => t.IsActive, true)
            .With(t => t.CreatedDate, DateTime.UtcNow)
            .With(t => t.CreatedBy, "System");
    }

    public static TestDataBuilder<AIFeedbackEntry> AIFeedback()
    {
        return new TestDataBuilder<AIFeedbackEntry>()
            .With(f => f.Id, Random.Shared.Next(1, int.MaxValue))
            .With(f => f.UserId, Guid.NewGuid().ToString())
            .With(f => f.OriginalQuery, "Show me sales data")
            .With(f => f.GeneratedSql, "SELECT * FROM Sales")
            .With(f => f.Rating, Random.Shared.Next(1, 6))
            .With(f => f.FeedbackType, "Positive")
            .With(f => f.CreatedAt, DateTime.UtcNow)
            .With(f => f.IsProcessed, false);
    }

    public static TestDataBuilder<SemanticCacheEntry> SemanticCache()
    {
        return new TestDataBuilder<SemanticCacheEntry>()
            .With(c => c.Id, Random.Shared.Next(1, int.MaxValue))
            .With(c => c.QueryHash, Guid.NewGuid().ToString("N")[..16])
            .With(c => c.OriginalQuery, "Show me customer data")
            .With(c => c.NormalizedQuery, "SELECT * FROM Customers")
            .With(c => c.GeneratedSql, "SELECT * FROM Customers")
            .With(c => c.ResultData, "{\"data\": [], \"success\": true}")
            .With(c => c.ResultMetadata, "{\"intent\": \"display\"}")
            .With(c => c.CreatedAt, DateTime.UtcNow)
            .With(c => c.ExpiresAt, DateTime.UtcNow.AddHours(24))
            .With(c => c.AccessCount, 1)
            .With(c => c.LastAccessedAt, DateTime.UtcNow);
    }

    public static TestDataBuilder<QueryResponse> QueryResponse()
    {
        return new TestDataBuilder<QueryResponse>()
            .With(r => r.Success, true)
            .With(r => r.Result, new QueryResult
            {
                Data = new object[] { },
                Metadata = new QueryMetadata { RowCount = 0 }
            })
            .With(r => r.ExecutionTimeMs, Random.Shared.Next(100, 1000))
            .With(r => r.Sql, "SELECT * FROM TestTable")
            .With(r => r.Confidence, Random.Shared.NextDouble())
            .With(r => r.Suggestions, Array.Empty<string>())
            .With(r => r.Error, null);
    }

    public static TestDataBuilder<SchemaMetadata> SchemaMetadata()
    {
        return new TestDataBuilder<SchemaMetadata>()
            .With(s => s.DatabaseName, "TestDatabase")
            .With(s => s.Tables, new List<TableMetadata>())
            .With(s => s.LastUpdated, DateTime.UtcNow);
    }

    public static TestDataBuilder<TableMetadata> TableMetadata()
    {
        return new TestDataBuilder<TableMetadata>()
            .With(t => t.Name, $"TestTable{Random.Shared.Next(100, 999)}")
            .With(t => t.Schema, "dbo")
            .With(t => t.Columns, new List<ColumnMetadata>())
            .With(t => t.RowCount, Random.Shared.Next(1, 10000))
            .With(t => t.Description, "Test table");
    }

    public static TestDataBuilder<ColumnMetadata> ColumnMetadata()
    {
        return new TestDataBuilder<ColumnMetadata>()
            .With(c => c.Name, $"TestColumn{Random.Shared.Next(100, 999)}")
            .With(c => c.DataType, "varchar")
            .With(c => c.IsNullable, false)
            .With(c => c.IsPrimaryKey, false)
            .With(c => c.MaxLength, 255)
            .With(c => c.Description, "Test column")
            .With(c => c.SampleValues, new string[] { "Sample1", "Sample2" });
    }

    public static TestDataBuilder<AIGenerationAttempt> AIGenerationAttempt()
    {
        return new TestDataBuilder<AIGenerationAttempt>()
            .With(a => a.Id, Random.Shared.Next(1, int.MaxValue))
            .With(a => a.UserQuery, "Show me data")
            .With(a => a.PromptTemplate, "Context: Focus on display queries. Query: Show me data")
            .With(a => a.GeneratedSql, "SELECT * FROM TestTable")
            .With(a => a.AttemptedAt, DateTime.UtcNow)
            .With(a => a.IsSuccessful, true)
            .With(a => a.ConfidenceScore, 0.8)
            .With(a => a.UserId, "test-user");
    }
}

/// <summary>
/// Builder for creating test data scenarios
/// </summary>
public class TestScenarioBuilder
{
    private readonly List<object> _entities = new();

    public TestScenarioBuilder AddUser(Action<TestDataBuilder<UserEntity>>? configure = null)
    {
        var builder = TestDataBuilders.User();
        configure?.Invoke(builder);
        _entities.Add(builder.Build());
        return this;
    }

    public TestScenarioBuilder AddQueryHistory(Action<TestDataBuilder<BIReportingCopilot.Infrastructure.Data.Entities.QueryHistoryEntity>>? configure = null)
    {
        var builder = TestDataBuilders.QueryHistory();
        configure?.Invoke(builder);
        _entities.Add(builder.Build());
        return this;
    }

    public TestScenarioBuilder AddBusinessTable(Action<TestDataBuilder<BusinessTableInfoEntity>>? configure = null)
    {
        var builder = TestDataBuilders.BusinessTableInfo();
        configure?.Invoke(builder);
        _entities.Add(builder.Build());
        return this;
    }

    public TestScenarioBuilder AddAIFeedback(Action<TestDataBuilder<AIFeedbackEntry>>? configure = null)
    {
        var builder = TestDataBuilders.AIFeedback();
        configure?.Invoke(builder);
        _entities.Add(builder.Build());
        return this;
    }

    public TestScenarioBuilder AddSemanticCache(Action<TestDataBuilder<SemanticCacheEntry>>? configure = null)
    {
        var builder = TestDataBuilders.SemanticCache();
        configure?.Invoke(builder);
        _entities.Add(builder.Build());
        return this;
    }

    public List<object> Build()
    {
        return _entities.ToList();
    }

    public async Task SeedAsync(BICopilotContext context)
    {
        foreach (var entity in _entities)
        {
            context.Add(entity);
        }

        await context.SaveChangesAsync();
    }
}
