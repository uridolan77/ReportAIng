using BIReportingCopilot.Infrastructure.AI.Management;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BIReportingCopilot.Tests.Unit.Services;

public class MetadataEnhancementServiceTests : IDisposable
{
    private readonly BICopilotContext _context;
    private readonly MetadataEnhancementService _service;
    private readonly Mock<ILogger<MetadataEnhancementService>> _mockLogger;

    public MetadataEnhancementServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BICopilotContext(options);
        _mockLogger = new Mock<ILogger<MetadataEnhancementService>>();
        _service = new MetadataEnhancementService(_context, _mockLogger.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add test BusinessTableInfo
        var testTable = new BusinessTableInfoEntity
        {
            Id = Guid.NewGuid(),
            TableName = "tbl_Test_Table",
            SchemaName = "dbo",
            BusinessPurpose = "Test table for unit testing",
            SemanticDescription = null, // Empty field to be enhanced
            LLMContextHints = null, // Empty field to be enhanced
            CreatedBy = "test",
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = "test",
            UpdatedDate = DateTime.UtcNow
        };

        _context.BusinessTableInfo.Add(testTable);

        // Add test BusinessColumnInfo
        var testColumn = new BusinessColumnInfoEntity
        {
            Id = Guid.NewGuid(),
            TableInfoId = testTable.Id,
            ColumnName = "TestColumn",
            BusinessMeaning = "Test column for unit testing",
            SemanticContext = null, // Empty field to be enhanced
            AnalyticalContext = null, // Empty field to be enhanced
            SemanticRelevanceScore = 0, // Empty field to be enhanced
            CreatedBy = "test",
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = "test",
            UpdatedDate = DateTime.UtcNow
        };

        _context.BusinessColumnInfo.Add(testColumn);

        // Add test BusinessGlossary
        var testGlossary = new BusinessGlossaryEntity
        {
            Id = Guid.NewGuid(),
            Term = "Test Term",
            Definition = "A test term for unit testing",
            Category = "Testing",
            ContextualVariations = null, // Empty field to be enhanced
            QueryPatterns = null, // Empty field to be enhanced
            CreatedBy = "test",
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = "test",
            UpdatedDate = DateTime.UtcNow
        };

        _context.BusinessGlossary.Add(testGlossary);

        _context.SaveChanges();
    }

    [Fact]
    public async Task EnhanceMetadataAsync_EmptyFieldsOnly_ShouldEnhanceEmptyFields()
    {
        // Arrange
        var request = new MetadataEnhancementService.MetadataEnhancementRequest
        {
            Mode = MetadataEnhancementService.EnhancementMode.EmptyFieldsOnly,
            BatchSize = 10,
            PreviewOnly = false
        };

        // Act
        var result = await _service.EnhanceMetadataAsync(request, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.FieldsEnhanced > 0);
        Assert.Equal(1, result.ColumnsProcessed);
        Assert.Equal(1, result.TablesProcessed);
        Assert.Equal(1, result.GlossaryTermsProcessed);

        // Verify that fields were actually enhanced
        var enhancedColumn = await _context.BusinessColumnInfo.FirstAsync();
        Assert.NotNull(enhancedColumn.SemanticContext);
        Assert.NotNull(enhancedColumn.AnalyticalContext);
        Assert.True(enhancedColumn.SemanticRelevanceScore > 0);

        var enhancedTable = await _context.BusinessTableInfo.FirstAsync();
        Assert.NotNull(enhancedTable.SemanticDescription);
        Assert.NotNull(enhancedTable.LLMContextHints);

        var enhancedGlossary = await _context.BusinessGlossary.FirstAsync();
        Assert.NotNull(enhancedGlossary.ContextualVariations);
        Assert.NotNull(enhancedGlossary.QueryPatterns);
    }

    [Fact]
    public async Task EnhanceMetadataAsync_PreviewMode_ShouldNotSaveChanges()
    {
        // Arrange
        var request = new MetadataEnhancementService.MetadataEnhancementRequest
        {
            Mode = MetadataEnhancementService.EnhancementMode.EmptyFieldsOnly,
            BatchSize = 10,
            PreviewOnly = true
        };

        // Get original values
        var originalColumn = await _context.BusinessColumnInfo.FirstAsync();
        var originalSemanticContext = originalColumn.SemanticContext;

        // Act
        var result = await _service.EnhanceMetadataAsync(request, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.FieldsEnhanced > 0);

        // Verify that fields were NOT actually saved to database
        _context.ChangeTracker.Clear(); // Clear tracking to force reload from DB
        var unchangedColumn = await _context.BusinessColumnInfo.FirstAsync();
        Assert.Equal(originalSemanticContext, unchangedColumn.SemanticContext);
    }

    [Fact]
    public async Task EnhanceMetadataAsync_EmptyRequest_ShouldProcessAllRecords()
    {
        // Arrange
        var request = new MetadataEnhancementService.MetadataEnhancementRequest();

        // Act
        var result = await _service.EnhanceMetadataAsync(request, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Enhanced", result.Message);
        Assert.True(result.ProcessingTime.TotalMilliseconds > 0);
    }

    [Fact]
    public async Task EnhanceMetadataAsync_WithBatchSize_ShouldRespectBatchLimit()
    {
        // Arrange
        var request = new MetadataEnhancementService.MetadataEnhancementRequest
        {
            BatchSize = 1 // Very small batch size
        };

        // Act
        var result = await _service.EnhanceMetadataAsync(request, "test-user");

        // Assert
        Assert.True(result.Success);
        // With batch size 1, we should process at most 1 record per entity type
        Assert.True(result.ColumnsProcessed <= 1);
        Assert.True(result.TablesProcessed <= 1);
        Assert.True(result.GlossaryTermsProcessed <= 1);
    }

    [Fact]
    public async Task EnhanceMetadataAsync_ShouldUpdateAuditFields()
    {
        // Arrange
        var request = new MetadataEnhancementService.MetadataEnhancementRequest
        {
            Mode = MetadataEnhancementService.EnhancementMode.EmptyFieldsOnly,
            PreviewOnly = false
        };

        var userId = "test-user-123";
        var beforeTime = DateTime.UtcNow;

        // Act
        var result = await _service.EnhanceMetadataAsync(request, userId);

        // Assert
        Assert.True(result.Success);

        // Verify audit fields were updated
        var updatedColumn = await _context.BusinessColumnInfo.FirstAsync();
        Assert.Equal(userId, updatedColumn.UpdatedBy);
        Assert.True(updatedColumn.UpdatedDate >= beforeTime);

        var updatedTable = await _context.BusinessTableInfo.FirstAsync();
        Assert.Equal(userId, updatedTable.UpdatedBy);
        Assert.True(updatedTable.UpdatedDate >= beforeTime);

        var updatedGlossary = await _context.BusinessGlossary.FirstAsync();
        Assert.Equal(userId, updatedGlossary.UpdatedBy);
        Assert.True(updatedGlossary.UpdatedDate >= beforeTime);
    }

    [Fact]
    public async Task EnhanceMetadataAsync_WithAlreadyPopulatedFields_ShouldSkipThem()
    {
        // Arrange
        // Pre-populate some fields
        var column = await _context.BusinessColumnInfo.FirstAsync();
        column.SemanticContext = "Already populated";
        await _context.SaveChangesAsync();

        var request = new MetadataEnhancementService.MetadataEnhancementRequest
        {
            Mode = MetadataEnhancementService.EnhancementMode.EmptyFieldsOnly
        };

        // Act
        var result = await _service.EnhanceMetadataAsync(request, "test-user");

        // Assert
        Assert.True(result.Success);

        // Verify the pre-populated field was not changed
        var unchangedColumn = await _context.BusinessColumnInfo.FirstAsync();
        Assert.Equal("Already populated", unchangedColumn.SemanticContext);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
