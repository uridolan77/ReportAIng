-- Quick check for provider names and variations
SELECT DISTINCT Provider, COUNT(*) as GameCount
FROM dbo.Games WITH (NOLOCK)
WHERE Provider LIKE '%net%' OR Provider LIKE '%Net%' OR Provider LIKE '%NET%'
GROUP BY Provider
ORDER BY Provider;

-- Check all providers to see exact spelling
SELECT DISTINCT Provider
FROM dbo.Games WITH (NOLOCK)
ORDER BY Provider;
