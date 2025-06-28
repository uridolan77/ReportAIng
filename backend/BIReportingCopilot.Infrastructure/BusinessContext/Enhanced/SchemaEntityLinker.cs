using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced implementation of entity linking that maps entities to actual database schema
/// </summary>
public class SchemaEntityLinker : ISemanticEntityLinker
{
    private readonly ILogger<SchemaEntityLinker> _logger;
    private readonly BIReportingCopilot.Core.Interfaces.BusinessContext.IBusinessMetadataRetrievalService _metadataService;

    // Business term to database mapping dictionary based on actual schema and business metadata
    private static readonly Dictionary<string, TableColumnMapping> BusinessTermMappings = new()
    {
        // Financial/Banking terms - Core entities for depositor queries (based on actual BusinessGlossary data)
        { "depositors", new("tbl_Daily_actions", "PlayerID", "Deposits") },
        { "deposits", new("tbl_Daily_actions", "Deposits") },
        { "deposit", new("tbl_Daily_actions", "Deposits") },
        { "depositing", new("tbl_Daily_actions", "Deposits") },
        { "deposited", new("tbl_Daily_actions", "Deposits") },
        { "ftd", new("tbl_Daily_actions", "FTD") }, // First Time Deposit from glossary
        { "first time deposit", new("tbl_Daily_actions", "FTD") },
        { "first deposit", new("tbl_Daily_actions", "FTD") },
        { "withdrawals", new("tbl_Daily_actions", "Withdrawals") },
        { "withdrawal", new("tbl_Daily_actions", "Withdrawals") },
        { "withdraw", new("tbl_Daily_actions", "Withdrawals") },
        
        // Revenue and Gaming metrics (from BusinessGlossary)
        { "ggr", new("tbl_Daily_actions", "GGR") }, // Gross Gaming Revenue
        { "gross gaming revenue", new("tbl_Daily_actions", "GGR") },
        { "ngr", new("tbl_Daily_actions", "NGR") }, // Net Gaming Revenue  
        { "net gaming revenue", new("tbl_Daily_actions", "NGR") },
        { "revenue", new("tbl_Daily_actions", "NGR") },
        { "bets", new("tbl_Daily_actions", "Bets") },
        { "bet", new("tbl_Daily_actions", "Bets") },
        { "wins", new("tbl_Daily_actions", "Wins") },
        { "win", new("tbl_Daily_actions", "Wins") },
        { "winnings", new("tbl_Daily_actions", "Wins") },
        
        // Player/Customer entities (based on BusinessTableInfo)
        { "players", new("tbl_Daily_actions_players", "PlayerID") },
        { "player", new("tbl_Daily_actions_players", "PlayerID") },
        { "customers", new("tbl_Daily_actions_players", "PlayerID") },
        { "customer", new("tbl_Daily_actions_players", "PlayerID") },
        { "users", new("tbl_Daily_actions_players", "PlayerID") },
        { "user", new("tbl_Daily_actions_players", "PlayerID") },
        { "player id", new("tbl_Daily_actions_players", "PlayerID") },
        
        // Country/Location entities (based on actual player data structure)
        { "countries", new("tbl_Countries", "CountryID", "CountryIntlCode") },
        { "country", new("tbl_Daily_actions_players", "Country", "Country") },
        { "uk", new("tbl_Daily_actions_players", "Country", "UK") },
        { "united kingdom", new("tbl_Daily_actions_players", "Country", "United Kingdom") },
        { "gb", new("tbl_Daily_actions_players", "Country", "UK") },
        { "britain", new("tbl_Daily_actions_players", "Country", "UK") },
        { "england", new("tbl_Daily_actions_players", "Country", "UK") },
        { "from uk", new("tbl_Daily_actions_players", "Country", "UK") },
        
        // Currency entities (from BusinessColumnInfo)
        { "currency", new("tbl_Currencies", "CurrencyID", "CurrencyCode") },
        { "currencies", new("tbl_Currencies", "CurrencyID", "CurrencyCode") },
        { "eur", new("tbl_Currencies", "CurrencyCode", "EUR") },
        { "usd", new("tbl_Currencies", "CurrencyCode", "USD") },
        { "gbp", new("tbl_Currencies", "CurrencyCode", "GBP") },
        { "exchange rate", new("tbl_Currencies", "RateInEUR") },
        
        // Gaming/Activity terms (based on BusinessTableInfo and Games table)
        { "games", new("Games", "GameID") },
        { "game", new("Games", "GameID") },
        { "gaming", new("tbl_Daily_actions_games", "GameID") },
        { "slots", new("Games", "GameType", "Slots") },
        { "slot", new("Games", "GameType", "Slots") },
        { "live casino", new("Games", "GameType", "Live Casino") },
        { "blackjack", new("Games", "GameType", "Blackjack") },
        { "roulette", new("Games", "GameType", "Roulette") },
        { "provider", new("Games", "Provider") },
        { "game provider", new("Games", "Provider") },
        { "rtp", new("Games", "PayoutLow") }, // Return to Player
        { "volatility", new("Games", "Volatility") },
        
        // White Label/Brand entities (from BusinessTableInfo)
        { "white label", new("tbl_White_labels", "LabelID") },
        { "brand", new("tbl_White_labels", "LabelID") },
        { "brands", new("tbl_White_labels", "LabelID") },
        { "casino", new("tbl_White_labels", "LabelName") },
        { "label", new("tbl_White_labels", "LabelID") },
        
        // Time references
        { "yesterday", new("tbl_Daily_actions", "Date") },
        { "today", new("tbl_Daily_actions", "Date") },
        { "date", new("tbl_Daily_actions", "Date") },
        { "day", new("tbl_Daily_actions", "Date") },
        { "daily", new("tbl_Daily_actions", "Date") },
        { "last day", new("tbl_Daily_actions", "Date") },
        { "previous day", new("tbl_Daily_actions", "Date") },
        
        // Aggregation/Metric terms
        { "top", new("", "", "ORDER BY") },
        { "top 10", new("", "", "TOP 10") },
        { "highest", new("", "", "ORDER BY DESC") },
        { "largest", new("", "", "ORDER BY DESC") },
        { "biggest", new("", "", "ORDER BY DESC") },
        { "sum", new("", "", "SUM") },
        { "total", new("", "", "SUM") },
        { "count", new("", "", "COUNT") },
        { "number", new("", "", "COUNT") },
        { "amount", new("", "", "SUM") },
        { "value", new("", "", "SUM") }
    };

    public SchemaEntityLinker(
        ILogger<SchemaEntityLinker> logger,
        BIReportingCopilot.Core.Interfaces.BusinessContext.IBusinessMetadataRetrievalService metadataService)
    {
        _logger = logger;
        _metadataService = metadataService;
    }

    public async Task<List<BusinessEntity>> LinkToSchemaAsync(List<BusinessEntity> entities, string userQuestion)
    {
        _logger.LogDebug("🔍 Linking {Count} entities to schema for question: {Question}", 
            entities.Count, userQuestion.Substring(0, Math.Min(50, userQuestion.Length)));
        
        var linkedEntities = new List<BusinessEntity>();

        foreach (var entity in entities)
        {
            var linkedEntity = await MapEntityToSchemaAsync(entity, userQuestion);
            linkedEntities.Add(linkedEntity);
        }

        _logger.LogInformation("✅ Successfully linked {Count} entities to schema", linkedEntities.Count);
        return linkedEntities;
    }

    private async Task<BusinessEntity> MapEntityToSchemaAsync(BusinessEntity entity, string userQuestion)
    {
        var entityName = entity.Name.ToLower().Trim();
        
        // Try direct mapping first
        if (BusinessTermMappings.TryGetValue(entityName, out var mapping))
        {
            entity.MappedTableName = mapping.TableName;
            entity.MappedColumnName = mapping.ColumnName;
            entity.ConfidenceScore = Math.Min(entity.ConfidenceScore + 0.2, 1.0); // Boost confidence for direct mapping
            
            // Add mapping metadata
            entity.Metadata["mapping_method"] = "direct_business_term";
            entity.Metadata["mapped_table"] = mapping.TableName;
            entity.Metadata["mapped_column"] = mapping.ColumnName;
            
            if (!string.IsNullOrEmpty(mapping.Value))
            {
                entity.Metadata["mapped_value"] = mapping.Value;
            }

            _logger.LogDebug("✅ Direct mapping: {Entity} -> {Table}.{Column}", 
                entityName, mapping.TableName, mapping.ColumnName);
            
            return entity;
        }

        // Try semantic matching with business metadata
        await TrySemanticMappingAsync(entity, userQuestion);
        
        return entity;
    }

    private async Task TrySemanticMappingAsync(BusinessEntity entity, string userQuestion)
    {
        try
        {
            var entityName = entity.Name.ToLower().Trim();
            
            // First try business glossary for term definitions
            await TryBusinessGlossaryMappingAsync(entity, entityName);
            if (!string.IsNullOrEmpty(entity.MappedTableName))
                return;

            // Then try business tables for semantic matching
            await TryBusinessTableMappingAsync(entity, entityName);
            if (!string.IsNullOrEmpty(entity.MappedTableName))
                return;

            // Finally try business columns for detailed mapping
            await TryBusinessColumnMappingAsync(entity, entityName);
            if (!string.IsNullOrEmpty(entity.MappedTableName))
                return;

            // If no mapping found, mark as unmapped but keep the entity
            entity.Metadata["mapping_method"] = "unmapped";
            _logger.LogDebug("❌ No mapping found for entity: {Entity}", entity.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during semantic mapping for entity: {Entity}", entity.Name);
            entity.Metadata["mapping_method"] = "error";
            entity.Metadata["mapping_error"] = ex.Message;
        }
    }

    private async Task TryBusinessGlossaryMappingAsync(BusinessEntity entity, string entityName)
    {
        try
        {
            // Use business glossary for term mapping
            var businessTerms = new List<string> { entityName };
            var glossaryTerms = await _metadataService.FindRelevantGlossaryTermsAsync(businessTerms);

            var matchingTerm = glossaryTerms.FirstOrDefault(g =>
                g.Term.ToLower() == entityName ||
                g.Synonyms?.Any(s => s.ToLower().Contains(entityName)) == true ||
                g.RelatedTerms?.Any(r => r.ToLower().Contains(entityName)) == true ||
                g.Definition?.ToLower().Contains(entityName) == true);

            if (matchingTerm != null)
            {
                // Parse mapped tables and columns from glossary
                if (!string.IsNullOrEmpty(matchingTerm.MappedTables))
                {
                    var tables = matchingTerm.MappedTables.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    entity.MappedTableName = tables.FirstOrDefault()?.Trim();
                }

                if (!string.IsNullOrEmpty(matchingTerm.MappedColumns))
                {
                    var columns = matchingTerm.MappedColumns.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    entity.MappedColumnName = columns.FirstOrDefault()?.Trim();
                }

                entity.ConfidenceScore = Math.Min(entity.ConfidenceScore + 0.25, 1.0); // High confidence for glossary match
                entity.Metadata["mapping_method"] = "business_glossary_match";
                entity.Metadata["glossary_term"] = matchingTerm.Term;
                entity.Metadata["glossary_definition"] = matchingTerm.Definition;
                entity.Metadata["business_domain"] = matchingTerm.Domain;

                _logger.LogDebug("📚 Business glossary mapping: {Entity} -> {Table}.{Column} (Term: {Term})",
                    entity.Name, entity.MappedTableName, entity.MappedColumnName, matchingTerm.Term);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during business glossary mapping for entity: {Entity}", entity.Name);
        }
    }

    private async Task TryBusinessTableMappingAsync(BusinessEntity entity, string entityName)
    {
        try
        {
            // Create a dummy business context profile for table search
            var dummyProfile = new BusinessContextProfile
            {
                AnalysisId = Guid.NewGuid().ToString(),
                OriginalQuestion = entityName,
                UserId = "system",
                Entities = new List<BusinessEntity> { entity },
                BusinessTerms = new List<string> { entityName },
                Domain = new BusinessDomain { Name = "General" },
                Intent = new QueryIntent { Type = IntentType.Exploratory }
            };

            var businessTables = await _metadataService.FindRelevantTablesAsync(dummyProfile, 10);

            var matchingTable = businessTables.FirstOrDefault(t =>
                t.TableName.ToLower().Contains(entityName) ||
                t.BusinessName?.ToLower().Contains(entityName) == true ||
                t.NaturalLanguageAliases?.Any(alias => alias.ToLower().Contains(entityName)) == true ||
                t.BusinessPurpose?.ToLower().Contains(entityName) == true ||
                entityName.Contains(t.TableName.ToLower().Replace("tbl_", "").Replace("_", " ")));

            if (matchingTable != null)
            {
                entity.MappedTableName = matchingTable.TableName;
                entity.ConfidenceScore = Math.Min(entity.ConfidenceScore + 0.15, 1.0);
                entity.Metadata["mapping_method"] = "business_table_match";
                entity.Metadata["table_business_purpose"] = matchingTable.BusinessPurpose;
                entity.Metadata["table_domain"] = matchingTable.DomainClassification;
                entity.Metadata["table_friendly_name"] = matchingTable.BusinessName;

                _logger.LogDebug("📊 Business table mapping: {Entity} -> {Table} (Purpose: {Purpose})",
                    entity.Name, matchingTable.TableName, matchingTable.BusinessPurpose?.Substring(0, Math.Min(50, matchingTable.BusinessPurpose?.Length ?? 0)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during business table mapping for entity: {Entity}", entity.Name);
        }
    }

    private async Task TryBusinessColumnMappingAsync(BusinessEntity entity, string entityName)
    {
        try
        {
            if (entity.Type == EntityType.Column || entity.Type == EntityType.Dimension || entity.Type == EntityType.Metric)
            {
                // Create a dummy business context profile for table search
                var dummyProfile = new BusinessContextProfile
                {
                    AnalysisId = Guid.NewGuid().ToString(),
                    OriginalQuestion = entityName,
                    UserId = "system",
                    Entities = new List<BusinessEntity> { entity },
                    BusinessTerms = new List<string> { entityName },
                    Domain = new BusinessDomain { Name = "General" },
                    Intent = new QueryIntent { Type = IntentType.Exploratory }
                };

                var businessTables = await _metadataService.FindRelevantTablesAsync(dummyProfile, 10);

                foreach (var table in businessTables.Take(10)) // Limit to avoid performance issues
                {
                    var tableIds = new List<long> { table.Id };
                    var columns = await _metadataService.FindRelevantColumnsAsync(tableIds, dummyProfile);
                    var matchingColumn = columns.FirstOrDefault(c =>
                        c.ColumnName.ToLower().Contains(entityName) ||
                        c.BusinessName?.ToLower().Contains(entityName) == true ||
                        c.BusinessMeaning?.ToLower().Contains(entityName) == true ||
                        entityName.Contains(c.ColumnName.ToLower()));

                    if (matchingColumn != null)
                    {
                        entity.MappedTableName = table.TableName;
                        entity.MappedColumnName = matchingColumn.ColumnName;
                        entity.ConfidenceScore = Math.Min(entity.ConfidenceScore + 0.2, 1.0);
                        entity.Metadata["mapping_method"] = "business_column_match";
                        entity.Metadata["column_business_meaning"] = matchingColumn.BusinessMeaning;
                        entity.Metadata["column_business_type"] = matchingColumn.DataType;
                        entity.Metadata["column_friendly_name"] = matchingColumn.BusinessName;

                        _logger.LogDebug("📈 Business column mapping: {Entity} -> {Table}.{Column} (Type: {Type})",
                            entity.Name, table.TableName, matchingColumn.ColumnName, matchingColumn.DataType);
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during business column mapping for entity: {Entity}", entity.Name);
        }
    }
}

/// <summary>
/// Represents a mapping from business term to database table/column
/// </summary>
public record TableColumnMapping(string TableName, string ColumnName = "", string Value = "");
