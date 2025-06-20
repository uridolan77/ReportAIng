-- Quick fix for the index error
-- The main IDENTITY fix worked, this just cleans up the index issue

PRINT 'Fixing index creation issue...';

-- Remove the problematic index if it was partially created
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AIFeedbackEntries_GenerationAttemptId')
BEGIN
    DROP INDEX [IX_AIFeedbackEntries_GenerationAttemptId] ON [AIFeedbackEntries];
    PRINT 'Dropped problematic index IX_AIFeedbackEntries_GenerationAttemptId';
END

-- Create the indexes that can be created (skip nvarchar(max) columns)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AIFeedbackEntries_SubmittedAt')
BEGIN
    CREATE INDEX [IX_AIFeedbackEntries_SubmittedAt] ON [AIFeedbackEntries] ([SubmittedAt]);
    PRINT 'Created index IX_AIFeedbackEntries_SubmittedAt';
END

PRINT 'Index fix completed!';
PRINT 'The main IDENTITY issue has been resolved.';
PRINT 'You can now run the Entity Framework migration.';
