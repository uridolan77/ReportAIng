-- Mark the problematic migration as already applied
-- Since we've manually implemented all the changes, we can skip running it

PRINT 'Marking migration as applied...';

-- Check if the migration is already marked as applied
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory] 
    WHERE [MigrationId] = '20250616223300_AddBusinessSchemaManagement'
)
BEGIN
    -- Mark the migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250616223300_AddBusinessSchemaManagement', '8.0.0');
    
    PRINT 'Migration 20250616223300_AddBusinessSchemaManagement marked as applied.';
END
ELSE
BEGIN
    PRINT 'Migration 20250616223300_AddBusinessSchemaManagement already marked as applied.';
END

PRINT 'Migration marking completed!';
PRINT 'You can now run the decimal precision migration safely.';
