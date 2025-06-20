-- Create missing SuggestionCategories table

PRINT 'Creating missing SuggestionCategories table...';

-- Create SuggestionCategories table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SuggestionCategories')
BEGIN
    CREATE TABLE [SuggestionCategories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_SuggestionCategories] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_SuggestionCategories_Name] ON [SuggestionCategories] ([Name]);
    CREATE INDEX [IX_SuggestionCategories_IsActive] ON [SuggestionCategories] ([IsActive]);
    CREATE INDEX [IX_SuggestionCategories_DisplayOrder] ON [SuggestionCategories] ([DisplayOrder]);
    
    -- Insert some default categories
    INSERT INTO [SuggestionCategories] ([Name], [Description], [IsActive], [DisplayOrder])
    VALUES 
        ('General', 'General suggestions and recommendations', 1, 1),
        ('Performance', 'Performance optimization suggestions', 1, 2),
        ('Data Quality', 'Data quality and validation suggestions', 1, 3),
        ('Security', 'Security-related suggestions', 1, 4),
        ('Best Practices', 'Best practice recommendations', 1, 5);
    
    PRINT 'SuggestionCategories table created with default data.';
END
ELSE
BEGIN
    PRINT 'SuggestionCategories table already exists.';
END

PRINT 'SuggestionCategories setup completed!';
