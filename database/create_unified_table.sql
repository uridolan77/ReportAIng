-- Create the missing UnifiedAIGenerationAttempt table
-- This is a new unified model, not a duplicate of AIGenerationAttempts

PRINT 'Creating UnifiedAIGenerationAttempt table...';

-- Create UnifiedAIGenerationAttempt table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UnifiedAIGenerationAttempt')
BEGIN
    CREATE TABLE [UnifiedAIGenerationAttempt] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(500) NOT NULL,
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

PRINT 'UnifiedAIGenerationAttempt table setup completed!';
