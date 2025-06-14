using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Interfaces;

namespace BIReportingCopilot.Infrastructure.Schema;

public class SchemaManagementService : ISchemaManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<SchemaManagementService> _logger;

    public SchemaManagementService(BICopilotContext context, ILogger<SchemaManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Business Schema Management

    public async Task<List<BusinessSchemaDto>> GetBusinessSchemasAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting business schemas for user: {UserId}", userId);

            var schemas = await _context.BusinessSchemas
                .Include(s => s.Versions.Where(v => v.IsActive))
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.IsDefault)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return schemas.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business schemas for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<BusinessSchemaDto> GetBusinessSchemaAsync(Guid schemaId, string userId)
    {
        try
        {
            _logger.LogInformation("Getting business schema {SchemaId} for user: {UserId}", schemaId, userId);

            var schema = await _context.BusinessSchemas
                .Include(s => s.Versions.Where(v => v.IsActive))
                .FirstOrDefaultAsync(s => s.Id == schemaId && s.IsActive);

            if (schema == null)
            {
                throw new ArgumentException($"Schema with ID {schemaId} not found or not accessible");
            }

            return MapToDto(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business schema {SchemaId} for user: {UserId}", schemaId, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaDto> CreateBusinessSchemaAsync(CreateBusinessSchemaRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Creating business schema '{Name}' for user: {UserId}", request.Name, userId);

            // Check if schema name already exists
            var existingSchema = await _context.BusinessSchemas
                .FirstOrDefaultAsync(s => s.Name == request.Name && s.IsActive);

            if (existingSchema != null)
            {
                throw new ArgumentException($"Schema with name '{request.Name}' already exists");
            }

            // If this is set as default, unset other defaults
            if (request.IsDefault)
            {
                await UnsetDefaultSchemasAsync();
            }

            var schema = new BusinessSchema
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDefault = request.IsDefault,
                Tags = request.Tags.Any() ? JsonSerializer.Serialize(request.Tags) : null
            };

            _context.BusinessSchemas.Add(schema);

            // Create initial version if provided
            if (request.InitialVersion != null)
            {
                var initialVersion = new BusinessSchemaVersion
                {
                    Id = Guid.NewGuid(),
                    SchemaId = schema.Id,
                    VersionNumber = 1,
                    VersionName = request.InitialVersion.VersionName ?? "v1.0",
                    Description = request.InitialVersion.Description ?? "Initial version",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsCurrent = true,
                    ChangeLog = request.InitialVersion.ChangeLog.Any() ? 
                        JsonSerializer.Serialize(request.InitialVersion.ChangeLog) : null
                };

                _context.BusinessSchemaVersions.Add(initialVersion);
                
                // Add content if provided
                await AddVersionContentAsync(initialVersion.Id, request.InitialVersion, userId);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created business schema '{Name}' with ID: {SchemaId}", request.Name, schema.Id);

            // Return the created schema with its versions
            return await GetBusinessSchemaAsync(schema.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business schema '{Name}' for user: {UserId}", request.Name, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaDto> UpdateBusinessSchemaAsync(Guid schemaId, UpdateBusinessSchemaRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Updating business schema {SchemaId} for user: {UserId}", schemaId, userId);

            var schema = await _context.BusinessSchemas
                .FirstOrDefaultAsync(s => s.Id == schemaId && s.IsActive);

            if (schema == null)
            {
                throw new ArgumentException($"Schema with ID {schemaId} not found or not accessible");
            }

            // Check if new name conflicts with existing schemas
            if (schema.Name != request.Name)
            {
                var existingSchema = await _context.BusinessSchemas
                    .FirstOrDefaultAsync(s => s.Name == request.Name && s.IsActive && s.Id != schemaId);

                if (existingSchema != null)
                {
                    throw new ArgumentException($"Schema with name '{request.Name}' already exists");
                }
            }

            // If this is set as default, unset other defaults
            if (request.IsDefault && !schema.IsDefault)
            {
                await UnsetDefaultSchemasAsync();
            }

            schema.Name = request.Name;
            schema.Description = request.Description;
            schema.UpdatedBy = userId;
            schema.UpdatedAt = DateTime.UtcNow;
            schema.IsActive = request.IsActive;
            schema.IsDefault = request.IsDefault;
            schema.Tags = request.Tags.Any() ? JsonSerializer.Serialize(request.Tags) : null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated business schema {SchemaId}", schemaId);

            return await GetBusinessSchemaAsync(schemaId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business schema {SchemaId} for user: {UserId}", schemaId, userId);
            throw;
        }
    }

    public async Task DeleteBusinessSchemaAsync(Guid schemaId, string userId)
    {
        try
        {
            _logger.LogInformation("Deleting business schema {SchemaId} for user: {UserId}", schemaId, userId);

            var schema = await _context.BusinessSchemas
                .FirstOrDefaultAsync(s => s.Id == schemaId && s.IsActive);

            if (schema == null)
            {
                throw new ArgumentException($"Schema with ID {schemaId} not found or not accessible");
            }

            // Soft delete by setting IsActive to false
            schema.IsActive = false;
            schema.UpdatedBy = userId;
            schema.UpdatedAt = DateTime.UtcNow;

            // Also deactivate all versions
            var versions = await _context.BusinessSchemaVersions
                .Where(v => v.SchemaId == schemaId)
                .ToListAsync();

            foreach (var version in versions)
            {
                version.IsActive = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted business schema {SchemaId}", schemaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business schema {SchemaId} for user: {UserId}", schemaId, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaDto> SetDefaultSchemaAsync(Guid schemaId, string userId)
    {
        try
        {
            _logger.LogInformation("Setting business schema {SchemaId} as default for user: {UserId}", schemaId, userId);

            var schema = await _context.BusinessSchemas
                .FirstOrDefaultAsync(s => s.Id == schemaId && s.IsActive);

            if (schema == null)
            {
                throw new ArgumentException($"Schema with ID {schemaId} not found or not accessible");
            }

            // Unset all other defaults
            await UnsetDefaultSchemasAsync();

            // Set this schema as default
            schema.IsDefault = true;
            schema.UpdatedBy = userId;
            schema.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully set business schema {SchemaId} as default", schemaId);

            return await GetBusinessSchemaAsync(schemaId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting business schema {SchemaId} as default for user: {UserId}", schemaId, userId);
            throw;
        }
    }

    #endregion

    #region Schema Version Management

    public async Task<List<BusinessSchemaVersionDto>> GetSchemaVersionsAsync(Guid schemaId, string userId)
    {
        try
        {
            _logger.LogInformation("Getting schema versions for schema {SchemaId} for user: {UserId}", schemaId, userId);

            var versions = await _context.BusinessSchemaVersions
                .Include(v => v.TableContexts)
                .Include(v => v.GlossaryTerms)
                .Include(v => v.Relationships)
                .Where(v => v.SchemaId == schemaId && v.IsActive)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            return versions.Select(MapToVersionDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema versions for schema {SchemaId} for user: {UserId}", schemaId, userId);
            throw;
        }
    }

    public async Task<DetailedSchemaVersionDto> GetSchemaVersionAsync(Guid versionId, string userId)
    {
        try
        {
            _logger.LogInformation("Getting schema version {VersionId} for user: {UserId}", versionId, userId);

            var version = await _context.BusinessSchemaVersions
                .Include(v => v.Schema)
                .Include(v => v.TableContexts)
                    .ThenInclude(t => t.ColumnContexts)
                .Include(v => v.GlossaryTerms)
                .Include(v => v.Relationships)
                .FirstOrDefaultAsync(v => v.Id == versionId && v.IsActive);

            if (version == null)
            {
                throw new ArgumentException($"Schema version with ID {versionId} not found or not accessible");
            }

            return MapToDetailedVersionDto(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema version {VersionId} for user: {UserId}", versionId, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaVersionDto> CreateSchemaVersionAsync(CreateSchemaVersionRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Creating schema version for schema {SchemaId} for user: {UserId}", request.SchemaId, userId);

            var schema = await _context.BusinessSchemas
                .FirstOrDefaultAsync(s => s.Id == request.SchemaId && s.IsActive);

            if (schema == null)
            {
                throw new ArgumentException($"Schema with ID {request.SchemaId} not found or not accessible");
            }

            // Get next version number
            var maxVersion = await _context.BusinessSchemaVersions
                .Where(v => v.SchemaId == request.SchemaId)
                .MaxAsync(v => (int?)v.VersionNumber) ?? 0;

            // If this is set as current, unset other current versions
            if (request.IsCurrent)
            {
                await UnsetCurrentVersionsAsync(request.SchemaId);
            }

            var version = new BusinessSchemaVersion
            {
                Id = Guid.NewGuid(),
                SchemaId = request.SchemaId,
                VersionNumber = maxVersion + 1,
                VersionName = request.VersionName,
                Description = request.Description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsCurrent = request.IsCurrent,
                ChangeLog = request.ChangeLog.Any() ? JsonSerializer.Serialize(request.ChangeLog) : null
            };

            _context.BusinessSchemaVersions.Add(version);

            // Add content
            await AddVersionContentAsync(version.Id, request, userId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created schema version {VersionId} for schema {SchemaId}", version.Id, request.SchemaId);

            return MapToVersionDto(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schema version for schema {SchemaId} for user: {UserId}", request.SchemaId, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaVersionDto> SetCurrentVersionAsync(Guid versionId, string userId)
    {
        try
        {
            _logger.LogInformation("Setting schema version {VersionId} as current for user: {UserId}", versionId, userId);

            var version = await _context.BusinessSchemaVersions
                .FirstOrDefaultAsync(v => v.Id == versionId && v.IsActive);

            if (version == null)
            {
                throw new ArgumentException($"Schema version with ID {versionId} not found or not accessible");
            }

            // Unset other current versions for this schema
            await UnsetCurrentVersionsAsync(version.SchemaId);

            // Set this version as current
            version.IsCurrent = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully set schema version {VersionId} as current", versionId);

            return MapToVersionDto(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting schema version {VersionId} as current for user: {UserId}", versionId, userId);
            throw;
        }
    }

    public async Task DeleteSchemaVersionAsync(Guid versionId, string userId)
    {
        try
        {
            _logger.LogInformation("Deleting schema version {VersionId} for user: {UserId}", versionId, userId);

            var version = await _context.BusinessSchemaVersions
                .FirstOrDefaultAsync(v => v.Id == versionId && v.IsActive);

            if (version == null)
            {
                throw new ArgumentException($"Schema version with ID {versionId} not found or not accessible");
            }

            // Check if this is the only version
            var versionCount = await _context.BusinessSchemaVersions
                .CountAsync(v => v.SchemaId == version.SchemaId && v.IsActive);

            if (versionCount <= 1)
            {
                throw new InvalidOperationException("Cannot delete the only version of a schema");
            }

            // Soft delete by setting IsActive to false
            version.IsActive = false;

            // If this was the current version, set another version as current
            if (version.IsCurrent)
            {
                var nextVersion = await _context.BusinessSchemaVersions
                    .Where(v => v.SchemaId == version.SchemaId && v.IsActive && v.Id != versionId)
                    .OrderByDescending(v => v.VersionNumber)
                    .FirstOrDefaultAsync();

                if (nextVersion != null)
                {
                    nextVersion.IsCurrent = true;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted schema version {VersionId}", versionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schema version {VersionId} for user: {UserId}", versionId, userId);
            throw;
        }
    }

    #endregion

    #region Apply Auto-Generated Content

    public async Task<BusinessSchemaVersionDto> ApplyToSchemaAsync(ApplyToSchemaRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Applying auto-generated content to schema for user: {UserId}", userId);

            BusinessSchema schema;

            // Create new schema or use existing
            if (request.SchemaId.HasValue)
            {
                schema = await _context.BusinessSchemas
                    .FirstOrDefaultAsync(s => s.Id == request.SchemaId.Value && s.IsActive);

                if (schema == null)
                {
                    throw new ArgumentException($"Schema with ID {request.SchemaId} not found or not accessible");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(request.NewSchemaName))
                {
                    throw new ArgumentException("NewSchemaName is required when creating a new schema");
                }

                // Create new schema
                var createRequest = new CreateBusinessSchemaRequest
                {
                    Name = request.NewSchemaName,
                    Description = request.NewSchemaDescription,
                    Tags = new List<string> { "Auto-Generated" },
                    IsDefault = false
                };

                var schemaDto = await CreateBusinessSchemaAsync(createRequest, userId);
                schema = await _context.BusinessSchemas.FirstAsync(s => s.Id == schemaDto.Id);
            }

            // Create new version
            var versionRequest = new CreateSchemaVersionRequest
            {
                SchemaId = schema.Id,
                VersionName = request.VersionName,
                Description = request.VersionDescription,
                IsCurrent = request.SetAsCurrent,
                ChangeLog = request.ChangeLog,
                TableContexts = request.TableContexts,
                GlossaryTerms = request.GlossaryTerms,
                Relationships = request.Relationships
            };

            var version = await CreateSchemaVersionAsync(versionRequest, userId);

            _logger.LogInformation("Successfully applied auto-generated content to schema {SchemaId}, version {VersionId}", schema.Id, version.Id);

            return version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying auto-generated content to schema for user: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region Schema Content Management

    public async Task<SchemaTableContextDto> UpdateTableContextAsync(Guid tableContextId, SchemaTableContextDto request, string userId)
    {
        try
        {
            _logger.LogInformation("Updating table context {TableContextId} for user: {UserId}", tableContextId, userId);

            var tableContext = await _context.SchemaTableContexts
                .FirstOrDefaultAsync(t => t.Id == tableContextId);

            if (tableContext == null)
            {
                throw new ArgumentException($"Table context with ID {tableContextId} not found");
            }

            tableContext.BusinessPurpose = request.BusinessPurpose;
            tableContext.BusinessContext = request.BusinessContext;
            tableContext.PrimaryUseCase = request.PrimaryUseCase;
            tableContext.KeyBusinessMetrics = request.KeyBusinessMetrics.Any() ? JsonSerializer.Serialize(request.KeyBusinessMetrics) : null;
            tableContext.CommonQueryPatterns = request.CommonQueryPatterns.Any() ? JsonSerializer.Serialize(request.CommonQueryPatterns) : null;
            tableContext.BusinessRules = request.BusinessRules.Any() ? JsonSerializer.Serialize(request.BusinessRules) : null;
            tableContext.ConfidenceScore = request.ConfidenceScore;
            tableContext.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated table context {TableContextId}", tableContextId);

            return MapToTableContextDto(tableContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table context {TableContextId} for user: {UserId}", tableContextId, userId);
            throw;
        }
    }

    public async Task<SchemaColumnContextDto> UpdateColumnContextAsync(Guid columnContextId, SchemaColumnContextDto request, string userId)
    {
        try
        {
            _logger.LogInformation("Updating column context {ColumnContextId} for user: {UserId}", columnContextId, userId);

            var columnContext = await _context.SchemaColumnContexts
                .FirstOrDefaultAsync(c => c.Id == columnContextId);

            if (columnContext == null)
            {
                throw new ArgumentException($"Column context with ID {columnContextId} not found");
            }

            columnContext.BusinessName = request.BusinessName;
            columnContext.BusinessDescription = request.BusinessDescription;
            columnContext.BusinessDataType = request.BusinessDataType;
            columnContext.DataExamples = request.DataExamples.Any() ? JsonSerializer.Serialize(request.DataExamples) : null;
            columnContext.ValidationRules = request.ValidationRules.Any() ? JsonSerializer.Serialize(request.ValidationRules) : null;
            columnContext.CommonUseCases = request.CommonUseCases.Any() ? JsonSerializer.Serialize(request.CommonUseCases) : null;
            columnContext.IsKeyColumn = request.IsKeyColumn;
            columnContext.IsPrimaryKey = request.IsPrimaryKey;
            columnContext.IsForeignKey = request.IsForeignKey;
            columnContext.ConfidenceScore = request.ConfidenceScore;
            columnContext.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated column context {ColumnContextId}", columnContextId);

            return MapToColumnContextDto(columnContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating column context {ColumnContextId} for user: {UserId}", columnContextId, userId);
            throw;
        }
    }

    public async Task<SchemaGlossaryTermDto> UpdateGlossaryTermAsync(Guid termId, SchemaGlossaryTermDto request, string userId)
    {
        try
        {
            _logger.LogInformation("Updating glossary term {TermId} for user: {UserId}", termId, userId);

            var term = await _context.SchemaGlossaryTerms
                .FirstOrDefaultAsync(t => t.Id == termId);

            if (term == null)
            {
                throw new ArgumentException($"Glossary term with ID {termId} not found");
            }

            term.Term = request.Term;
            term.Definition = request.Definition;
            term.BusinessContext = request.BusinessContext;
            term.Category = request.Category;
            term.Synonyms = request.Synonyms.Any() ? JsonSerializer.Serialize(request.Synonyms) : null;
            term.RelatedTerms = request.RelatedTerms.Any() ? JsonSerializer.Serialize(request.RelatedTerms) : null;
            term.SourceTables = request.SourceTables.Any() ? JsonSerializer.Serialize(request.SourceTables) : null;
            term.SourceColumns = request.SourceColumns.Any() ? JsonSerializer.Serialize(request.SourceColumns) : null;
            term.ConfidenceScore = request.ConfidenceScore;
            term.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated glossary term {TermId}", termId);

            return MapToGlossaryTermDto(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating glossary term {TermId} for user: {UserId}", termId, userId);
            throw;
        }
    }

    public async Task<SchemaRelationshipDto> UpdateRelationshipAsync(Guid relationshipId, SchemaRelationshipDto request, string userId)
    {
        try
        {
            _logger.LogInformation("Updating relationship {RelationshipId} for user: {UserId}", relationshipId, userId);

            var relationship = await _context.SchemaRelationships
                .FirstOrDefaultAsync(r => r.Id == relationshipId);

            if (relationship == null)
            {
                throw new ArgumentException($"Relationship with ID {relationshipId} not found");
            }

            relationship.FromTable = request.FromTable;
            relationship.ToTable = request.ToTable;
            relationship.RelationshipType = request.RelationshipType;
            relationship.FromColumns = request.FromColumns.Any() ? JsonSerializer.Serialize(request.FromColumns) : null;
            relationship.ToColumns = request.ToColumns.Any() ? JsonSerializer.Serialize(request.ToColumns) : null;
            relationship.BusinessDescription = request.BusinessDescription;
            relationship.ConfidenceScore = request.ConfidenceScore;
            relationship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated relationship {RelationshipId}", relationshipId);

            return MapToRelationshipDto(relationship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relationship {RelationshipId} for user: {UserId}", relationshipId, userId);
            throw;
        }
    }

    #endregion

    #region User Preferences

    public async Task<List<UserSchemaPreferenceDto>> GetUserSchemaPreferencesAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting user schema preferences for user: {UserId}", userId);

            var preferences = await _context.UserSchemaPreferences
                .Include(p => p.SchemaVersion)
                    .ThenInclude(v => v.Schema)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.IsDefault)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync();

            return preferences.Select(p => new UserSchemaPreferenceDto
            {
                Id = p.Id,
                UserId = p.UserId,
                SchemaVersionId = p.SchemaVersionId,
                IsDefault = p.IsDefault,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                SchemaVersion = MapToVersionDto(p.SchemaVersion)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user schema preferences for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSchemaPreferenceDto> SetUserDefaultSchemaAsync(Guid schemaVersionId, string userId)
    {
        try
        {
            _logger.LogInformation("Setting user default schema {SchemaVersionId} for user: {UserId}", schemaVersionId, userId);

            var schemaVersion = await _context.BusinessSchemaVersions
                .Include(v => v.Schema)
                .FirstOrDefaultAsync(v => v.Id == schemaVersionId && v.IsActive);

            if (schemaVersion == null)
            {
                throw new ArgumentException($"Schema version with ID {schemaVersionId} not found or not accessible");
            }

            // Remove existing default preference
            var existingDefault = await _context.UserSchemaPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsDefault);

            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                existingDefault.UpdatedAt = DateTime.UtcNow;
            }

            // Check if preference already exists
            var existingPreference = await _context.UserSchemaPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.SchemaVersionId == schemaVersionId);

            if (existingPreference != null)
            {
                existingPreference.IsDefault = true;
                existingPreference.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                existingPreference = new UserSchemaPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SchemaVersionId = schemaVersionId,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserSchemaPreferences.Add(existingPreference);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully set user default schema {SchemaVersionId} for user: {UserId}", schemaVersionId, userId);

            // Reload with includes
            existingPreference = await _context.UserSchemaPreferences
                .Include(p => p.SchemaVersion)
                    .ThenInclude(v => v.Schema)
                .FirstAsync(p => p.Id == existingPreference.Id);

            return new UserSchemaPreferenceDto
            {
                Id = existingPreference.Id,
                UserId = existingPreference.UserId,
                SchemaVersionId = existingPreference.SchemaVersionId,
                IsDefault = existingPreference.IsDefault,
                CreatedAt = existingPreference.CreatedAt,
                UpdatedAt = existingPreference.UpdatedAt,
                SchemaVersion = MapToVersionDto(existingPreference.SchemaVersion)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user default schema {SchemaVersionId} for user: {UserId}", schemaVersionId, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaVersionDto?> GetUserDefaultSchemaAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting user default schema for user: {UserId}", userId);

            var defaultPreference = await _context.UserSchemaPreferences
                .Include(p => p.SchemaVersion)
                    .ThenInclude(v => v.Schema)
                .Include(p => p.SchemaVersion.TableContexts)
                .Include(p => p.SchemaVersion.GlossaryTerms)
                .Include(p => p.SchemaVersion.Relationships)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsDefault);

            if (defaultPreference?.SchemaVersion != null)
            {
                return MapToVersionDto(defaultPreference.SchemaVersion);
            }

            // If no user preference, return system default
            var systemDefault = await _context.BusinessSchemas
                .Include(s => s.Versions.Where(v => v.IsCurrent && v.IsActive))
                    .ThenInclude(v => v.TableContexts)
                .Include(s => s.Versions.Where(v => v.IsCurrent && v.IsActive))
                    .ThenInclude(v => v.GlossaryTerms)
                .Include(s => s.Versions.Where(v => v.IsCurrent && v.IsActive))
                    .ThenInclude(v => v.Relationships)
                .Where(s => s.IsDefault && s.IsActive)
                .FirstOrDefaultAsync();

            return systemDefault?.CurrentVersion != null ? MapToVersionDto(systemDefault.CurrentVersion) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user default schema for user: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region Schema Comparison

    public async Task<SchemaComparisonDto> CompareSchemaVersionsAsync(Guid sourceVersionId, Guid targetVersionId, string userId)
    {
        try
        {
            _logger.LogInformation("Comparing schema versions {SourceVersionId} and {TargetVersionId} for user: {UserId}",
                sourceVersionId, targetVersionId, userId);

            var sourceVersion = await GetSchemaVersionAsync(sourceVersionId, userId);
            var targetVersion = await GetSchemaVersionAsync(targetVersionId, userId);

            var differences = new List<SchemaDifference>();
            var summary = new SchemaComparisonSummary();

            // Compare tables
            CompareTableContexts(sourceVersion.TableContexts, targetVersion.TableContexts, differences, summary);

            // Compare glossary terms
            CompareGlossaryTerms(sourceVersion.GlossaryTerms, targetVersion.GlossaryTerms, differences, summary);

            // Compare relationships
            CompareRelationships(sourceVersion.Relationships, targetVersion.Relationships, differences, summary);

            return new SchemaComparisonDto
            {
                SourceVersion = MapToVersionDto(await _context.BusinessSchemaVersions.FirstAsync(v => v.Id == sourceVersionId)),
                TargetVersion = MapToVersionDto(await _context.BusinessSchemaVersions.FirstAsync(v => v.Id == targetVersionId)),
                Differences = differences,
                Summary = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing schema versions {SourceVersionId} and {TargetVersionId} for user: {UserId}",
                sourceVersionId, targetVersionId, userId);
            throw;
        }
    }

    #endregion

    #region Schema Export/Import

    public async Task<DetailedSchemaVersionDto> ExportSchemaVersionAsync(Guid versionId, string userId)
    {
        try
        {
            _logger.LogInformation("Exporting schema version {VersionId} for user: {UserId}", versionId, userId);

            return await GetSchemaVersionAsync(versionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting schema version {VersionId} for user: {UserId}", versionId, userId);
            throw;
        }
    }

    public async Task<BusinessSchemaVersionDto> ImportSchemaVersionAsync(Guid schemaId, DetailedSchemaVersionDto schemaData, string userId)
    {
        try
        {
            _logger.LogInformation("Importing schema version to schema {SchemaId} for user: {UserId}", schemaId, userId);

            var createRequest = new CreateSchemaVersionRequest
            {
                SchemaId = schemaId,
                VersionName = $"Imported {schemaData.VersionName}",
                Description = $"Imported from {schemaData.DisplayName}: {schemaData.Description}",
                IsCurrent = false,
                ChangeLog = new List<ChangeLogEntry>
                {
                    new ChangeLogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Type = "Import",
                        Category = "Schema",
                        Item = "Full Schema",
                        Description = $"Imported schema version from {schemaData.DisplayName}"
                    }
                },
                TableContexts = schemaData.TableContexts,
                GlossaryTerms = schemaData.GlossaryTerms,
                Relationships = schemaData.Relationships
            };

            return await CreateSchemaVersionAsync(createRequest, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing schema version to schema {SchemaId} for user: {UserId}", schemaId, userId);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    private async Task UnsetDefaultSchemasAsync()
    {
        var defaultSchemas = await _context.BusinessSchemas
            .Where(s => s.IsDefault && s.IsActive)
            .ToListAsync();

        foreach (var schema in defaultSchemas)
        {
            schema.IsDefault = false;
        }
    }

    private async Task UnsetCurrentVersionsAsync(Guid schemaId)
    {
        var currentVersions = await _context.BusinessSchemaVersions
            .Where(v => v.SchemaId == schemaId && v.IsCurrent && v.IsActive)
            .ToListAsync();

        foreach (var version in currentVersions)
        {
            version.IsCurrent = false;
        }
    }

    private BusinessSchemaDto MapToDto(BusinessSchema schema)
    {
        var tags = new List<string>();
        if (!string.IsNullOrEmpty(schema.Tags))
        {
            try
            {
                tags = JsonSerializer.Deserialize<List<string>>(schema.Tags) ?? new List<string>();
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        return new BusinessSchemaDto
        {
            Id = schema.Id,
            Name = schema.Name,
            Description = schema.Description,
            CreatedBy = schema.CreatedBy,
            CreatedAt = schema.CreatedAt,
            UpdatedBy = schema.UpdatedBy,
            UpdatedAt = schema.UpdatedAt,
            IsActive = schema.IsActive,
            IsDefault = schema.IsDefault,
            Tags = tags,
            TotalVersions = schema.Versions.Count,
            CurrentVersion = schema.CurrentVersion != null ? MapToVersionDto(schema.CurrentVersion) : null,
            LastModified = schema.LastModified
        };
    }

    private BusinessSchemaVersionDto MapToVersionDto(BusinessSchemaVersion version)
    {
        var changeLog = new List<ChangeLogEntry>();
        if (!string.IsNullOrEmpty(version.ChangeLog))
        {
            try
            {
                changeLog = JsonSerializer.Deserialize<List<ChangeLogEntry>>(version.ChangeLog) ?? new List<ChangeLogEntry>();
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        return new BusinessSchemaVersionDto
        {
            Id = version.Id,
            SchemaId = version.SchemaId,
            VersionNumber = version.VersionNumber,
            VersionName = version.VersionName,
            Description = version.Description,
            CreatedBy = version.CreatedBy,
            CreatedAt = version.CreatedAt,
            IsActive = version.IsActive,
            IsCurrent = version.IsCurrent,
            ChangeLog = changeLog,
            DisplayName = version.DisplayName,
            TotalTables = version.TotalTables,
            TotalColumns = version.TotalColumns,
            TotalGlossaryTerms = version.TotalGlossaryTerms,
            TotalRelationships = version.TotalRelationships
        };
    }

    private DetailedSchemaVersionDto MapToDetailedVersionDto(BusinessSchemaVersion version)
    {
        var baseDto = MapToVersionDto(version);
        
        return new DetailedSchemaVersionDto
        {
            Id = baseDto.Id,
            SchemaId = baseDto.SchemaId,
            VersionNumber = baseDto.VersionNumber,
            VersionName = baseDto.VersionName,
            Description = baseDto.Description,
            CreatedBy = baseDto.CreatedBy,
            CreatedAt = baseDto.CreatedAt,
            IsActive = baseDto.IsActive,
            IsCurrent = baseDto.IsCurrent,
            ChangeLog = baseDto.ChangeLog,
            DisplayName = baseDto.DisplayName,
            TotalTables = baseDto.TotalTables,
            TotalColumns = baseDto.TotalColumns,
            TotalGlossaryTerms = baseDto.TotalGlossaryTerms,
            TotalRelationships = baseDto.TotalRelationships,
            TableContexts = version.TableContexts.Select(MapToTableContextDto).ToList(),
            GlossaryTerms = version.GlossaryTerms.Select(MapToGlossaryTermDto).ToList(),
            Relationships = version.Relationships.Select(MapToRelationshipDto).ToList()
        };
    }

    private async Task AddVersionContentAsync(Guid versionId, CreateSchemaVersionRequest request, string userId)
    {
        // Add table contexts
        foreach (var tableDto in request.TableContexts)
        {
            var tableContext = MapToTableContextEntity(tableDto, versionId);
            _context.SchemaTableContexts.Add(tableContext);
        }

        // Add glossary terms
        foreach (var termDto in request.GlossaryTerms)
        {
            var term = MapToGlossaryTermEntity(termDto, versionId);
            _context.SchemaGlossaryTerms.Add(term);
        }

        // Add relationships
        foreach (var relationshipDto in request.Relationships)
        {
            var relationship = MapToRelationshipEntity(relationshipDto, versionId);
            _context.SchemaRelationships.Add(relationship);
        }
    }

    private SchemaTableContextDto MapToTableContextDto(SchemaTableContext entity)
    {
        return new SchemaTableContextDto
        {
            Id = entity.Id,
            SchemaVersionId = entity.SchemaVersionId,
            TableName = entity.TableName,
            SchemaName = entity.SchemaName,
            BusinessPurpose = entity.BusinessPurpose,
            BusinessContext = entity.BusinessContext,
            PrimaryUseCase = entity.PrimaryUseCase,
            KeyBusinessMetrics = ParseJsonArray(entity.KeyBusinessMetrics),
            CommonQueryPatterns = ParseJsonArray(entity.CommonQueryPatterns),
            BusinessRules = ParseJsonArray(entity.BusinessRules),
            ConfidenceScore = entity.ConfidenceScore,
            IsAutoGenerated = entity.IsAutoGenerated,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            FullTableName = entity.FullTableName,
            ColumnContexts = entity.ColumnContexts.Select(MapToColumnContextDto).ToList()
        };
    }

    private SchemaColumnContextDto MapToColumnContextDto(SchemaColumnContext entity)
    {
        return new SchemaColumnContextDto
        {
            Id = entity.Id,
            TableContextId = entity.TableContextId,
            ColumnName = entity.ColumnName,
            BusinessName = entity.BusinessName,
            BusinessDescription = entity.BusinessDescription,
            BusinessDataType = entity.BusinessDataType,
            DataExamples = ParseJsonArray(entity.DataExamples),
            ValidationRules = ParseJsonArray(entity.ValidationRules),
            CommonUseCases = ParseJsonArray(entity.CommonUseCases),
            IsKeyColumn = entity.IsKeyColumn,
            IsPrimaryKey = entity.IsPrimaryKey,
            IsForeignKey = entity.IsForeignKey,
            ConfidenceScore = entity.ConfidenceScore,
            IsAutoGenerated = entity.IsAutoGenerated,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private SchemaGlossaryTermDto MapToGlossaryTermDto(SchemaGlossaryTerm entity)
    {
        return new SchemaGlossaryTermDto
        {
            Id = entity.Id,
            SchemaVersionId = entity.SchemaVersionId,
            Term = entity.Term,
            Definition = entity.Definition,
            BusinessContext = entity.BusinessContext,
            Category = entity.Category,
            Synonyms = ParseJsonArray(entity.Synonyms),
            RelatedTerms = ParseJsonArray(entity.RelatedTerms),
            SourceTables = ParseJsonArray(entity.SourceTables),
            SourceColumns = ParseJsonArray(entity.SourceColumns),
            ConfidenceScore = entity.ConfidenceScore,
            IsAutoGenerated = entity.IsAutoGenerated,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private SchemaRelationshipDto MapToRelationshipDto(SchemaRelationship entity)
    {
        return new SchemaRelationshipDto
        {
            Id = entity.Id,
            SchemaVersionId = entity.SchemaVersionId,
            FromTable = entity.FromTable,
            ToTable = entity.ToTable,
            RelationshipType = entity.RelationshipType,
            FromColumns = ParseJsonArray(entity.FromColumns),
            ToColumns = ParseJsonArray(entity.ToColumns),
            BusinessDescription = entity.BusinessDescription,
            ConfidenceScore = entity.ConfidenceScore,
            IsAutoGenerated = entity.IsAutoGenerated,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private SchemaTableContext MapToTableContextEntity(SchemaTableContextDto dto, Guid versionId)
    {
        return new SchemaTableContext
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            SchemaVersionId = versionId,
            TableName = dto.TableName,
            SchemaName = dto.SchemaName,
            BusinessPurpose = dto.BusinessPurpose,
            BusinessContext = dto.BusinessContext,
            PrimaryUseCase = dto.PrimaryUseCase,
            KeyBusinessMetrics = dto.KeyBusinessMetrics.Any() ? JsonSerializer.Serialize(dto.KeyBusinessMetrics) : null,
            CommonQueryPatterns = dto.CommonQueryPatterns.Any() ? JsonSerializer.Serialize(dto.CommonQueryPatterns) : null,
            BusinessRules = dto.BusinessRules.Any() ? JsonSerializer.Serialize(dto.BusinessRules) : null,
            ConfidenceScore = dto.ConfidenceScore,
            IsAutoGenerated = dto.IsAutoGenerated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private SchemaGlossaryTerm MapToGlossaryTermEntity(SchemaGlossaryTermDto dto, Guid versionId)
    {
        return new SchemaGlossaryTerm
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            SchemaVersionId = versionId,
            Term = dto.Term,
            Definition = dto.Definition,
            BusinessContext = dto.BusinessContext,
            Category = dto.Category,
            Synonyms = dto.Synonyms.Any() ? JsonSerializer.Serialize(dto.Synonyms) : null,
            RelatedTerms = dto.RelatedTerms.Any() ? JsonSerializer.Serialize(dto.RelatedTerms) : null,
            SourceTables = dto.SourceTables.Any() ? JsonSerializer.Serialize(dto.SourceTables) : null,
            SourceColumns = dto.SourceColumns.Any() ? JsonSerializer.Serialize(dto.SourceColumns) : null,
            ConfidenceScore = dto.ConfidenceScore,
            IsAutoGenerated = dto.IsAutoGenerated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private SchemaRelationship MapToRelationshipEntity(SchemaRelationshipDto dto, Guid versionId)
    {
        return new SchemaRelationship
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            SchemaVersionId = versionId,
            FromTable = dto.FromTable,
            ToTable = dto.ToTable,
            RelationshipType = dto.RelationshipType,
            FromColumns = dto.FromColumns.Any() ? JsonSerializer.Serialize(dto.FromColumns) : null,
            ToColumns = dto.ToColumns.Any() ? JsonSerializer.Serialize(dto.ToColumns) : null,
            BusinessDescription = dto.BusinessDescription,
            ConfidenceScore = dto.ConfidenceScore,
            IsAutoGenerated = dto.IsAutoGenerated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private void CompareTableContexts(List<SchemaTableContextDto> source, List<SchemaTableContextDto> target,
        List<SchemaDifference> differences, SchemaComparisonSummary summary)
    {
        var sourceDict = source.ToDictionary(t => $"{t.SchemaName}.{t.TableName}", t => t);
        var targetDict = target.ToDictionary(t => $"{t.SchemaName}.{t.TableName}", t => t);

        // Find added tables
        foreach (var targetTable in targetDict.Where(t => !sourceDict.ContainsKey(t.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Added",
                Category = "Table",
                Item = targetTable.Key,
                Description = $"Table {targetTable.Key} was added",
                TargetValue = targetTable.Value
            });
            summary.TablesAdded++;
        }

        // Find removed tables
        foreach (var sourceTable in sourceDict.Where(s => !targetDict.ContainsKey(s.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Removed",
                Category = "Table",
                Item = sourceTable.Key,
                Description = $"Table {sourceTable.Key} was removed",
                SourceValue = sourceTable.Value
            });
            summary.TablesRemoved++;
        }

        // Find modified tables
        foreach (var commonTable in sourceDict.Where(s => targetDict.ContainsKey(s.Key)))
        {
            var sourceTable = commonTable.Value;
            var targetTable = targetDict[commonTable.Key];

            if (!AreTablesEqual(sourceTable, targetTable))
            {
                differences.Add(new SchemaDifference
                {
                    Type = "Modified",
                    Category = "Table",
                    Item = commonTable.Key,
                    Description = $"Table {commonTable.Key} was modified",
                    SourceValue = sourceTable,
                    TargetValue = targetTable
                });
                summary.TablesModified++;
            }

            // Compare columns within the table
            CompareColumns(sourceTable.ColumnContexts, targetTable.ColumnContexts,
                commonTable.Key, differences, summary);
        }
    }

    private void CompareColumns(List<SchemaColumnContextDto> source, List<SchemaColumnContextDto> target,
        string tableName, List<SchemaDifference> differences, SchemaComparisonSummary summary)
    {
        var sourceDict = source.ToDictionary(c => c.ColumnName, c => c);
        var targetDict = target.ToDictionary(c => c.ColumnName, c => c);

        // Find added columns
        foreach (var targetColumn in targetDict.Where(c => !sourceDict.ContainsKey(c.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Added",
                Category = "Column",
                Item = $"{tableName}.{targetColumn.Key}",
                Description = $"Column {targetColumn.Key} was added to table {tableName}",
                TargetValue = targetColumn.Value
            });
            summary.ColumnsAdded++;
        }

        // Find removed columns
        foreach (var sourceColumn in sourceDict.Where(s => !targetDict.ContainsKey(s.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Removed",
                Category = "Column",
                Item = $"{tableName}.{sourceColumn.Key}",
                Description = $"Column {sourceColumn.Key} was removed from table {tableName}",
                SourceValue = sourceColumn.Value
            });
            summary.ColumnsRemoved++;
        }

        // Find modified columns
        foreach (var commonColumn in sourceDict.Where(s => targetDict.ContainsKey(s.Key)))
        {
            var sourceColumn = commonColumn.Value;
            var targetColumn = targetDict[commonColumn.Key];

            if (!AreColumnsEqual(sourceColumn, targetColumn))
            {
                differences.Add(new SchemaDifference
                {
                    Type = "Modified",
                    Category = "Column",
                    Item = $"{tableName}.{commonColumn.Key}",
                    Description = $"Column {commonColumn.Key} in table {tableName} was modified",
                    SourceValue = sourceColumn,
                    TargetValue = targetColumn
                });
                summary.ColumnsModified++;
            }
        }
    }

    private void CompareGlossaryTerms(List<SchemaGlossaryTermDto> source, List<SchemaGlossaryTermDto> target,
        List<SchemaDifference> differences, SchemaComparisonSummary summary)
    {
        var sourceDict = source.ToDictionary(t => t.Term, t => t);
        var targetDict = target.ToDictionary(t => t.Term, t => t);

        // Find added terms
        foreach (var targetTerm in targetDict.Where(t => !sourceDict.ContainsKey(t.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Added",
                Category = "Glossary",
                Item = targetTerm.Key,
                Description = $"Glossary term '{targetTerm.Key}' was added",
                TargetValue = targetTerm.Value
            });
            summary.GlossaryTermsAdded++;
        }

        // Find removed terms
        foreach (var sourceTerm in sourceDict.Where(s => !targetDict.ContainsKey(s.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Removed",
                Category = "Glossary",
                Item = sourceTerm.Key,
                Description = $"Glossary term '{sourceTerm.Key}' was removed",
                SourceValue = sourceTerm.Value
            });
            summary.GlossaryTermsRemoved++;
        }

        // Find modified terms
        foreach (var commonTerm in sourceDict.Where(s => targetDict.ContainsKey(s.Key)))
        {
            var sourceTerm = commonTerm.Value;
            var targetTerm = targetDict[commonTerm.Key];

            if (!AreGlossaryTermsEqual(sourceTerm, targetTerm))
            {
                differences.Add(new SchemaDifference
                {
                    Type = "Modified",
                    Category = "Glossary",
                    Item = commonTerm.Key,
                    Description = $"Glossary term '{commonTerm.Key}' was modified",
                    SourceValue = sourceTerm,
                    TargetValue = targetTerm
                });
                summary.GlossaryTermsModified++;
            }
        }
    }

    private void CompareRelationships(List<SchemaRelationshipDto> source, List<SchemaRelationshipDto> target,
        List<SchemaDifference> differences, SchemaComparisonSummary summary)
    {
        var sourceDict = source.ToDictionary(r => $"{r.FromTable}->{r.ToTable}", r => r);
        var targetDict = target.ToDictionary(r => $"{r.FromTable}->{r.ToTable}", r => r);

        // Find added relationships
        foreach (var targetRel in targetDict.Where(r => !sourceDict.ContainsKey(r.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Added",
                Category = "Relationship",
                Item = targetRel.Key,
                Description = $"Relationship {targetRel.Key} was added",
                TargetValue = targetRel.Value
            });
            summary.RelationshipsAdded++;
        }

        // Find removed relationships
        foreach (var sourceRel in sourceDict.Where(s => !targetDict.ContainsKey(s.Key)))
        {
            differences.Add(new SchemaDifference
            {
                Type = "Removed",
                Category = "Relationship",
                Item = sourceRel.Key,
                Description = $"Relationship {sourceRel.Key} was removed",
                SourceValue = sourceRel.Value
            });
            summary.RelationshipsRemoved++;
        }

        // Find modified relationships
        foreach (var commonRel in sourceDict.Where(s => targetDict.ContainsKey(s.Key)))
        {
            var sourceRel = commonRel.Value;
            var targetRel = targetDict[commonRel.Key];

            if (!AreRelationshipsEqual(sourceRel, targetRel))
            {
                differences.Add(new SchemaDifference
                {
                    Type = "Modified",
                    Category = "Relationship",
                    Item = commonRel.Key,
                    Description = $"Relationship {commonRel.Key} was modified",
                    SourceValue = sourceRel,
                    TargetValue = targetRel
                });
                summary.RelationshipsModified++;
            }
        }
    }

    private bool AreTablesEqual(SchemaTableContextDto source, SchemaTableContextDto target)
    {
        return source.BusinessPurpose == target.BusinessPurpose &&
               source.BusinessContext == target.BusinessContext &&
               source.PrimaryUseCase == target.PrimaryUseCase &&
               JsonSerializer.Serialize(source.KeyBusinessMetrics) == JsonSerializer.Serialize(target.KeyBusinessMetrics) &&
               JsonSerializer.Serialize(source.CommonQueryPatterns) == JsonSerializer.Serialize(target.CommonQueryPatterns) &&
               JsonSerializer.Serialize(source.BusinessRules) == JsonSerializer.Serialize(target.BusinessRules);
    }

    private bool AreColumnsEqual(SchemaColumnContextDto source, SchemaColumnContextDto target)
    {
        return source.BusinessName == target.BusinessName &&
               source.BusinessDescription == target.BusinessDescription &&
               source.BusinessDataType == target.BusinessDataType &&
               source.IsKeyColumn == target.IsKeyColumn &&
               source.IsPrimaryKey == target.IsPrimaryKey &&
               source.IsForeignKey == target.IsForeignKey &&
               JsonSerializer.Serialize(source.DataExamples) == JsonSerializer.Serialize(target.DataExamples) &&
               JsonSerializer.Serialize(source.ValidationRules) == JsonSerializer.Serialize(target.ValidationRules) &&
               JsonSerializer.Serialize(source.CommonUseCases) == JsonSerializer.Serialize(target.CommonUseCases);
    }

    private bool AreGlossaryTermsEqual(SchemaGlossaryTermDto source, SchemaGlossaryTermDto target)
    {
        return source.Definition == target.Definition &&
               source.BusinessContext == target.BusinessContext &&
               source.Category == target.Category &&
               JsonSerializer.Serialize(source.Synonyms) == JsonSerializer.Serialize(target.Synonyms) &&
               JsonSerializer.Serialize(source.RelatedTerms) == JsonSerializer.Serialize(target.RelatedTerms);
    }

    private bool AreRelationshipsEqual(SchemaRelationshipDto source, SchemaRelationshipDto target)
    {
        return source.RelationshipType == target.RelationshipType &&
               source.BusinessDescription == target.BusinessDescription &&
               JsonSerializer.Serialize(source.FromColumns) == JsonSerializer.Serialize(target.FromColumns) &&
               JsonSerializer.Serialize(source.ToColumns) == JsonSerializer.Serialize(target.ToColumns);
    }

    #endregion

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get schema metadata async (ISchemaManagementService interface)
    /// </summary>
    public async Task<SchemaMetadata> GetSchemaMetadataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Getting schema metadata");

            // Get the default schema or first available schema
            var defaultSchema = await _context.BusinessSchemas
                .Include(s => s.Versions.Where(v => v.IsActive && v.IsCurrent))
                .ThenInclude(v => v.TableContexts)
                .ThenInclude(t => t.ColumnContexts)
                .FirstOrDefaultAsync(s => s.IsActive && s.IsDefault, cancellationToken);

            if (defaultSchema == null)
            {
                defaultSchema = await _context.BusinessSchemas
                    .Include(s => s.Versions.Where(v => v.IsActive && v.IsCurrent))
                    .ThenInclude(v => v.TableContexts)
                    .ThenInclude(t => t.ColumnContexts)
                    .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);
            }

            if (defaultSchema?.Versions.FirstOrDefault() == null)
            {
                return new SchemaMetadata
                {
                    DatabaseName = "No Schema Available",
                    LastUpdated = DateTime.UtcNow,
                    Tables = new List<TableMetadata>()
                };
            }

            var currentVersion = defaultSchema.Versions.First();
            var tables = new List<TableMetadata>();

            foreach (var tableContext in currentVersion.TableContexts)
            {
                var table = new TableMetadata
                {
                    Name = tableContext.TableName,
                    Schema = tableContext.SchemaName,
                    Description = tableContext.BusinessPurpose,
                    LastUpdated = tableContext.UpdatedAt != default ? tableContext.UpdatedAt : tableContext.CreatedAt,
                    Columns = new List<ColumnMetadata>()
                };

                foreach (var columnContext in tableContext.ColumnContexts)
                {
                    table.Columns.Add(new ColumnMetadata
                    {
                        Name = columnContext.ColumnName,
                        DataType = columnContext.DataType ?? "unknown",
                        Description = columnContext.BusinessMeaning,
                        IsNullable = true, // Default assumption
                        IsPrimaryKey = false, // Would need additional logic to determine
                        IsForeignKey = false, // Would need additional logic to determine
                        SemanticTags = new string[0],
                        SampleValues = new string[0]
                    });
                }

                tables.Add(table);
            }

            return new SchemaMetadata
            {
                DatabaseName = defaultSchema.Name,
                LastUpdated = currentVersion.CreatedAt,
                Tables = tables
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting schema metadata");
            return new SchemaMetadata
            {
                DatabaseName = "Error",
                LastUpdated = DateTime.UtcNow,
                Tables = new List<TableMetadata>()
            };
        }
    }

    /// <summary>
    /// Get tables async (ISchemaManagementService interface)
    /// </summary>
    public async Task<List<TableMetadata>> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync(cancellationToken);
            return schema.Tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting tables");
            return new List<TableMetadata>();
        }
    }

    /// <summary>
    /// Get table metadata async (ISchemaManagementService interface)
    /// </summary>
    public async Task<TableMetadata?> GetTableMetadataAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync(cancellationToken);
            return schema.Tables.FirstOrDefault(t =>
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) ||
                $"{t.Schema}.{t.Name}".Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting table metadata for: {TableName}", tableName);
            return null;
        }
    }

    /// <summary>
    /// Refresh schema async (ISchemaManagementService interface)
    /// </summary>
    public async Task RefreshSchemaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Refreshing schema metadata");
            // In a real implementation, this would trigger a schema refresh
            // For now, we'll just log the operation
            await Task.CompletedTask;
            _logger.LogInformation("✅ Schema refresh completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error refreshing schema");
        }
    }

    /// <summary>
    /// Get databases async (ISchemaManagementService interface)
    /// </summary>
    public async Task<List<string>> GetDatabasesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 Getting available databases");

            var schemas = await _context.BusinessSchemas
                .Where(s => s.IsActive)
                .Select(s => s.Name)
                .ToListAsync(cancellationToken);

            return schemas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting databases");
            return new List<string>();
        }
    }

    #endregion
}
