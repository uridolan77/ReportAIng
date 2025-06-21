// Domain/Repositories/IBusinessTableRepository.cs
namespace ReportAIng.Domain.Repositories
{
    public interface IBusinessTableRepository : IRepository<BusinessTableInfoEntity>
    {
        Task<List<BusinessTableInfoEntity>> GetByDomainAsync(string domain);
        Task<BusinessTableInfoEntity?> GetWithColumnsAsync(long id);
        Task<List<BusinessTableInfoEntity>> GetByIdsAsync(List<long> ids);
        Task<List<BusinessTableInfoEntity>> SearchByNameAsync(string searchTerm);
        Task<List<BusinessTableInfoEntity>> GetActiveTablesAsync(int maxResults = 100);
        Task<List<ForeignKeyRelationship>> GetForeignKeyRelationshipsAsync(List<string> tableNames);
        Task<Dictionary<string, TableStatistics>> GetTableStatisticsAsync(List<string> tableNames);
    }

    public interface IBusinessColumnRepository : IRepository<BusinessColumnInfoEntity>
    {
        Task<List<BusinessColumnInfoEntity>> GetByTableIdAsync(long tableId);
        Task<List<BusinessColumnInfoEntity>> GetByTableIdsAsync(List<long> tableIds);
        Task<List<BusinessColumnInfoEntity>> SearchByNameAsync(string searchTerm, long? tableId = null);
        Task<Dictionary<long, int>> GetColumnCountsByTableAsync(List<long> tableIds);
    }

    public interface IBusinessGlossaryRepository : IRepository<BusinessGlossaryEntity>
    {
        Task<List<BusinessGlossaryEntity>> GetActiveTermsAsync();
        Task<BusinessGlossaryEntity?> GetByTermAsync(string term);
        Task<List<BusinessGlossaryEntity>> SearchByTermAsync(string searchTerm);
        Task<List<BusinessGlossaryEntity>> GetByDomainAsync(string domain);
        Task<List<BusinessGlossaryEntity>> GetByCategoryAsync(string category);
    }

    public interface IBusinessDomainRepository : IRepository<BusinessDomainEntity>
    {
        Task<List<BusinessDomainEntity>> GetActiveDomainsAsync();
        Task<BusinessDomainEntity?> GetByNameAsync(string domainName);
        Task<List<BusinessDomainEntity>> GetRelatedDomainsAsync(string domainName);
    }

    public interface IPromptTemplateRepository : IRepository<PromptTemplateEntity>
    {
        Task<PromptTemplateEntity?> GetByKeyAsync(string templateKey);
        Task<List<PromptTemplateEntity>> GetByIntentTypeAsync(string intentType);
        Task<List<PromptTemplateEntity>> GetActiveTemplatesAsync();
    }

    public interface IQueryExampleRepository : IRepository<QueryExampleEntity>
    {
        Task<List<QueryExampleEntity>> GetByIntentTypeAsync(string intentType, int maxResults);
        Task<List<QueryExampleEntity>> GetByDomainAsync(string domain, int maxResults);
        Task<List<QueryExampleEntity>> GetByTableAsync(string tableName, int maxResults);
        Task<List<QueryExampleEntity>> GetSuccessfulExamplesAsync(int maxResults);
    }
}

// Infrastructure/Repositories/BusinessTableRepository.cs
namespace ReportAIng.Infrastructure.Repositories
{
    public class BusinessTableRepository : Repository<BusinessTableInfoEntity>, IBusinessTableRepository
    {
        private readonly BIReportingContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BusinessTableRepository> _logger;

        public BusinessTableRepository(
            BIReportingContext context,
            IMemoryCache cache,
            ILogger<BusinessTableRepository> logger) : base(context)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<BusinessTableInfoEntity>> GetByDomainAsync(string domain)
        {
            var cacheKey = $"tables_by_domain_{domain}";
            
            if (_cache.TryGetValue<List<BusinessTableInfoEntity>>(cacheKey, out var cachedTables))
            {
                return cachedTables!;
            }

            var tables = await _context.BusinessTableInfo
                .Where(t => t.IsActive && t.DomainClassification.Contains(domain))
                .OrderByDescending(t => t.ImportanceScore)
                .ToListAsync();

            _cache.Set(cacheKey, tables, TimeSpan.FromMinutes(30));
            
            return tables;
        }

        public async Task<BusinessTableInfoEntity?> GetWithColumnsAsync(long id)
        {
            return await _context.BusinessTableInfo
                .Include(t => t.Columns.Where(c => c.IsActive))
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<List<BusinessTableInfoEntity>> GetByIdsAsync(List<long> ids)
        {
            return await _context.BusinessTableInfo
                .Where(t => ids.Contains(t.Id) && t.IsActive)
                .ToListAsync();
        }

        public async Task<List<BusinessTableInfoEntity>> SearchByNameAsync(string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower();
            
            return await _context.BusinessTableInfo
                .Where(t => t.IsActive && 
                    (t.TableName.ToLower().Contains(normalizedSearch) ||
                     t.BusinessFriendlyName.ToLower().Contains(normalizedSearch) ||
                     t.NaturalLanguageAliases.ToLower().Contains(normalizedSearch)))
                .OrderByDescending(t => t.ImportanceScore)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<BusinessTableInfoEntity>> GetActiveTablesAsync(int maxResults = 100)
        {
            var cacheKey = $"active_tables_{maxResults}";
            
            if (_cache.TryGetValue<List<BusinessTableInfoEntity>>(cacheKey, out var cachedTables))
            {
                return cachedTables!;
            }

            var tables = await _context.BusinessTableInfo
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.UsageFrequency)
                .ThenByDescending(t => t.ImportanceScore)
                .Take(maxResults)
                .ToListAsync();

            _cache.Set(cacheKey, tables, TimeSpan.FromHours(1));
            
            return tables;
        }

        public async Task<List<ForeignKeyRelationship>> GetForeignKeyRelationshipsAsync(
            List<string> tableNames)
        {
            // This would query system tables or a custom relationship table
            var query = @"
                SELECT 
                    FK.TABLE_SCHEMA + '.' + FK.TABLE_NAME AS FromTable,
                    PK.TABLE_SCHEMA + '.' + PK.TABLE_NAME AS ToTable,
                    CU.COLUMN_NAME AS FromColumn,
                    PT.COLUMN_NAME AS ToColumn
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK 
                    ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK 
                    ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU 
                    ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
                INNER JOIN (
                    SELECT i1.TABLE_NAME, i2.COLUMN_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 
                        ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                    WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) PT ON PT.TABLE_NAME = PK.TABLE_NAME
                WHERE FK.TABLE_SCHEMA + '.' + FK.TABLE_NAME IN @tableNames
                   OR PK.TABLE_SCHEMA + '.' + PK.TABLE_NAME IN @tableNames";

            var relationships = await _context.Database
                .SqlQueryRaw<ForeignKeyRelationship>(query, tableNames)
                .ToListAsync();

            return relationships;
        }

        public async Task<Dictionary<string, TableStatistics>> GetTableStatisticsAsync(
            List<string> tableNames)
        {
            var statistics = new Dictionary<string, TableStatistics>();

            foreach (var tableName in tableNames)
            {
                var parts = tableName.Split('.');
                var schema = parts[0];
                var table = parts[1];

                var stats = await _context.Database
                    .SqlQueryRaw<TableStatistics>(@"
                        SELECT 
                            @schema + '.' + @table as TableName,
                            SUM(p.rows) as RowCount,
                            SUM(a.total_pages) * 8 / 1024.0 as SizeMB,
                            MAX(s.last_user_update) as LastModified
                        FROM sys.tables t
                        INNER JOIN sys.indexes i ON t.object_id = i.object_id
                        INNER JOIN sys.partitions p ON i.object_id = p.object_id 
                            AND i.index_id = p.index_id
                        INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
                        LEFT JOIN sys.dm_db_index_usage_stats s ON t.object_id = s.object_id
                        WHERE t.schema_id = SCHEMA_ID(@schema) 
                            AND t.name = @table
                            AND i.index_id <= 1
                        GROUP BY t.name", 
                        new SqlParameter("@schema", schema),
                        new SqlParameter("@table", table))
                    .FirstOrDefaultAsync();

                if (stats != null)
                {
                    statistics[tableName] = stats;
                }
            }

            return statistics;
        }
    }
}

// Infrastructure/Repositories/BusinessColumnRepository.cs
namespace ReportAIng.Infrastructure.Repositories
{
    public class BusinessColumnRepository : Repository<BusinessColumnInfoEntity>, IBusinessColumnRepository
    {
        private readonly BIReportingContext _context;
        private readonly IMemoryCache _cache;

        public BusinessColumnRepository(
            BIReportingContext context,
            IMemoryCache cache) : base(context)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<BusinessColumnInfoEntity>> GetByTableIdAsync(long tableId)
        {
            var cacheKey = $"columns_by_table_{tableId}";
            
            if (_cache.TryGetValue<List<BusinessColumnInfoEntity>>(cacheKey, out var cached))
            {
                return cached!;
            }

            var columns = await _context.BusinessColumnInfo
                .Where(c => c.TableInfoId == tableId && c.IsActive)
                .OrderByDescending(c => c.ImportanceScore)
                .ThenBy(c => c.ColumnName)
                .ToListAsync();

            _cache.Set(cacheKey, columns, TimeSpan.FromMinutes(30));
            
            return columns;
        }

        public async Task<List<BusinessColumnInfoEntity>> GetByTableIdsAsync(List<long> tableIds)
        {
            return await _context.BusinessColumnInfo
                .Where(c => tableIds.Contains(c.TableInfoId) && c.IsActive)
                .OrderByDescending(c => c.ImportanceScore)
                .ToListAsync();
        }

        public async Task<List<BusinessColumnInfoEntity>> SearchByNameAsync(
            string searchTerm, 
            long? tableId = null)
        {
            var query = _context.BusinessColumnInfo
                .Where(c => c.IsActive);

            if (tableId.HasValue)
            {
                query = query.Where(c => c.TableInfoId == tableId.Value);
            }

            var normalizedSearch = searchTerm.ToLower();
            
            return await query
                .Where(c => c.ColumnName.ToLower().Contains(normalizedSearch) ||
                           c.BusinessFriendlyName.ToLower().Contains(normalizedSearch) ||
                           c.NaturalLanguageAliases.ToLower().Contains(normalizedSearch))
                .Take(20)
                .ToListAsync();
        }

        public async Task<Dictionary<long, int>> GetColumnCountsByTableAsync(List<long> tableIds)
        {
            return await _context.BusinessColumnInfo
                .Where(c => tableIds.Contains(c.TableInfoId) && c.IsActive)
                .GroupBy(c => c.TableInfoId)
                .Select(g => new { TableId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TableId, x => x.Count);
        }
    }
}

// Infrastructure/Repositories/PromptTemplateRepository.cs
namespace ReportAIng.Infrastructure.Repositories
{
    public class PromptTemplateRepository : Repository<PromptTemplateEntity>, IPromptTemplateRepository
    {
        private readonly BIReportingContext _context;
        private readonly IMemoryCache _cache;

        public PromptTemplateRepository(
            BIReportingContext context,
            IMemoryCache cache) : base(context)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<PromptTemplateEntity?> GetByKeyAsync(string templateKey)
        {
            var cacheKey = $"prompt_template_{templateKey}";
            
            if (_cache.TryGetValue<PromptTemplateEntity>(cacheKey, out var cached))
            {
                return cached;
            }

            var template = await _context.PromptTemplates
                .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.IsActive);

            if (template != null)
            {
                _cache.Set(cacheKey, template, TimeSpan.FromHours(1));
            }

            return template;
        }

        public async Task<List<PromptTemplateEntity>> GetByIntentTypeAsync(string intentType)
        {
            return await _context.PromptTemplates
                .Where(t => t.IntentType == intentType && t.IsActive)
                .OrderBy(t => t.Priority)
                .ToListAsync();
        }

        public async Task<List<PromptTemplateEntity>> GetActiveTemplatesAsync()
        {
            var cacheKey = "active_prompt_templates";
            
            if (_cache.TryGetValue<List<PromptTemplateEntity>>(cacheKey, out var cached))
            {
                return cached!;
            }

            var templates = await _context.PromptTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.IntentType)
                .ThenBy(t => t.Priority)
                .ToListAsync();

            _cache.Set(cacheKey, templates, TimeSpan.FromHours(1));
            
            return templates;
        }
    }
}

// Domain/Entities/Relationships.cs
namespace ReportAIng.Domain.Entities
{
    public class ForeignKeyRelationship
    {
        public string FromTable { get; set; } = string.Empty;
        public string ToTable { get; set; } = string.Empty;
        public string FromColumn { get; set; } = string.Empty;
        public string ToColumn { get; set; } = string.Empty;
    }

    public class TableStatistics
    {
        public string TableName { get; set; } = string.Empty;
        public long RowCount { get; set; }
        public decimal SizeMB { get; set; }
        public DateTime? LastModified { get; set; }
    }
}

// Domain/Entities/PromptTemplateEntity.cs
namespace ReportAIng.Domain.Entities
{
    public class PromptTemplateEntity : BaseEntity
    {
        public string TemplateKey { get; set; } = string.Empty;
        public string IntentType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

// Domain/Entities/QueryExampleEntity.cs
namespace ReportAIng.Domain.Entities
{
    public class QueryExampleEntity : BaseEntity
    {
        public string NaturalLanguageQuery { get; set; } = string.Empty;
        public string GeneratedSql { get; set; } = string.Empty;
        public string IntentType { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string UsedTables { get; set; } = string.Empty;
        public string BusinessContext { get; set; } = string.Empty;
        public double SuccessRate { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUsed { get; set; }
    }
}