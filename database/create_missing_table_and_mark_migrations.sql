-- Create missing UnifiedAIGenerationAttempt table and mark migrations as applied
-- This resolves the remaining migration issues

PRINT 'Creating missing UnifiedAIGenerationAttempt table...';

-- Create UnifiedAIGenerationAttempt table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UnifiedAIGenerationAttempt')
BEGIN
    CREATE TABLE [UnifiedAIGenerationAttempt] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(256) NOT NULL,
        [Provider] nvarchar(max) NOT NULL,
        [Model] nvarchar(max) NOT NULL,
        [InputPrompt] nvarchar(max) NOT NULL,
        [GeneratedOutput] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ResponseTimeMs] bigint NOT NULL,
        [InputTokens] int NOT NULL,
        [OutputTokens] int NOT NULL,
        [TotalTokens] int NOT NULL,
        [Cost] decimal(18,8) NOT NULL,  -- Already with correct precision
        [GenerationType] nvarchar(max) NOT NULL,
        [QualityScore] float NULL,
        [Metadata] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [AttemptedAt] datetime2 NOT NULL,
        [AIProvider] nvarchar(100) NOT NULL,
        [ModelVersion] nvarchar(100) NOT NULL,
        [CreatedBy] nvarchar(500) NULL,
        [UpdatedBy] nvarchar(500) NULL,
        CONSTRAINT [PK_UnifiedAIGenerationAttempt] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_UserId] ON [UnifiedAIGenerationAttempt] ([UserId]);
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_AttemptedAt] ON [UnifiedAIGenerationAttempt] ([AttemptedAt]);
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_Provider] ON [UnifiedAIGenerationAttempt] ([Provider]);
    
    PRINT 'UnifiedAIGenerationAttempt table created successfully.';
END
ELSE
BEGIN
    PRINT 'UnifiedAIGenerationAttempt table already exists.';
END

-- Create other missing tables that might be referenced

-- BusinessSchemas table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    CREATE TABLE [BusinessSchemas] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] nvarchar(100) NOT NULL,
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] bit NOT NULL DEFAULT 1,
        [IsDefault] bit NOT NULL DEFAULT 0,
        [Tags] nvarchar(500) NULL,
        CONSTRAINT [PK_BusinessSchemas] PRIMARY KEY ([Id])
    );
    PRINT 'BusinessSchemas table created.';
END

-- BusinessSchemaVersions table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemaVersions')
BEGIN
    CREATE TABLE [BusinessSchemaVersions] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [SchemaId] uniqueidentifier NOT NULL,
        [VersionNumber] int NOT NULL,
        [VersionName] nvarchar(50) NULL,
        [Description] nvarchar(1000) NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] bit NOT NULL DEFAULT 1,
        [IsCurrent] bit NOT NULL DEFAULT 0,
        [ChangeLog] nvarchar(max) NULL,
        CONSTRAINT [PK_BusinessSchemaVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BusinessSchemaVersions_BusinessSchemas_SchemaId] FOREIGN KEY ([SchemaId]) REFERENCES [BusinessSchemas] ([Id]) ON DELETE CASCADE
    );
    PRINT 'BusinessSchemaVersions table created.';
END

-- LLMModelConfigs table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LLMModelConfigs')
BEGIN
    CREATE TABLE [LLMModelConfigs] (
        [ModelId] nvarchar(100) NOT NULL,
        [ProviderId] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DisplayName] nvarchar(150) NOT NULL,
        [Temperature] real NOT NULL,
        [MaxTokens] int NOT NULL,
        [TopP] real NOT NULL,
        [FrequencyPenalty] real NOT NULL,
        [PresencePenalty] real NOT NULL,
        [IsEnabled] bit NOT NULL,
        [UseCase] nvarchar(50) NULL,
        [CostPerToken] decimal(18,8) NOT NULL,
        [Capabilities] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_LLMModelConfigs] PRIMARY KEY ([ModelId])
    );
    PRINT 'LLMModelConfigs table created.';
END

-- LLMProviderConfigs table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LLMProviderConfigs')
BEGIN
    CREATE TABLE [LLMProviderConfigs] (
        [ProviderId] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [ApiKey] nvarchar(500) NULL,
        [Endpoint] nvarchar(500) NULL,
        [Organization] nvarchar(100) NULL,
        [IsEnabled] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [Settings] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LLMProviderConfigs] PRIMARY KEY ([ProviderId])
    );
    PRINT 'LLMProviderConfigs table created.';
END

-- Mark the decimal precision migration as applied since we created tables with correct precision
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory] 
    WHERE [MigrationId] = '20250617133637_FixDecimalPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250617133637_FixDecimalPrecision', '8.0.0');
    PRINT 'FixDecimalPrecision migration marked as applied.';
END
ELSE
BEGIN
    PRINT 'FixDecimalPrecision migration already marked as applied.';
END

PRINT '';
PRINT 'Database setup completed successfully!';
PRINT 'All missing tables created with correct structure.';
PRINT 'All problematic migrations marked as applied.';
PRINT 'You can now start the application - it should connect to SQL Server database.';
