-- Systematic Business Metadata Population Script
-- Populates comprehensive business metadata for all DailyActionsDB tables

-- =============================================================================
-- PHASE 1: REFERENCE DATA TABLES (47 columns total)
-- =============================================================================

-- 1. CURRENCIES TABLE (12 columns) - TableInfoId = 2
INSERT INTO BusinessColumnInfo (
    TableInfoId, ColumnName, BusinessMeaning, BusinessContext, DataExamples, ValidationRules, 
    IsKeyColumn, IsActive, CreatedDate, CreatedBy, NaturalLanguageAliases, ValueExamples, 
    DataLineage, CalculationRules, SemanticTags, BusinessDataType, ConstraintsAndRules, 
    DataQualityScore, UsageFrequency, PreferredAggregation, RelatedBusinessTerms, 
    IsSensitiveData, IsCalculatedField
) VALUES 
-- Primary Key
(2, 'CurrencyID', 'Unique identifier for each currency in the system', 
 'Primary key used to reference currencies across all gaming and financial transactions. Links to player accounts, deposits, withdrawals, and gaming activities.',
 '1=EUR, 2=USD, 3=GBP, 4=CAD, 5=AUD', 'Must be unique, non-null, positive integer between 1-255',
 1, 1, GETDATE(), 'System', 'currency id, currency identifier, currency key, currency number, money id',
 'EUR=1, USD=2, GBP=3, CAD=4, AUD=5, SEK=6, NOK=7, DKK=8, CHF=9, PLN=10',
 'Master reference data maintained by finance team, sourced from regulatory requirements',
 'Auto-generated sequential identifier', 'primary_key,identifier,reference_data,master_data,financial',
 'Integer', 'UNIQUE, NOT NULL, CHECK (CurrencyID > 0)', 9.8, 0.95, 'COUNT',
 'Currency,Money,Financial Instrument,ISO Currency,Reference Data', 0, 0),

-- Display Name
(2, 'CurrencyName', 'Full descriptive name of the currency', 
 'Human-readable currency name used in user interfaces, reports, and customer communications. Supports multiple languages and regional variations.',
 'Euro, US Dollar, British Pound, Canadian Dollar', 'Required field, max 30 characters, must be valid currency name',
 0, 1, GETDATE(), 'System', 'currency name, currency title, currency description, money name, currency display name',
 'Euro, US Dollar, British Pound Sterling, Canadian Dollar, Australian Dollar, Swedish Krona',
 'Maintained by localization team, synchronized with ISO 4217 standards',
 'Direct input from currency master data', 'display_name,localization,user_interface,reference_data',
 'Text', 'NOT NULL, LENGTH <= 30, VALID_CURRENCY_NAME', 9.5, 0.85, 'COUNT',
 'Currency Name,Display Name,Localization,Reference Data', 0, 0),

-- Currency Symbol
(2, 'CurrencySymbol', 'Visual symbol representing the currency', 
 'Currency symbol displayed in user interfaces, transaction summaries, and financial reports. Used for quick visual identification.',
 '€, $, £, C$, A$', 'Optional field, max 5 characters, Unicode support for international symbols',
 0, 1, GETDATE(), 'System', 'currency symbol, money symbol, currency sign, currency icon, symbol',
 '€, $, £, C$, A$, kr, ₹, ¥, ₽, ₩',
 'Sourced from Unicode currency symbol standards and regional preferences',
 'Unicode character mapping from ISO standards', 'display,symbol,visual,unicode,user_interface',
 'Text', 'LENGTH <= 5, UNICODE_SYMBOL', 9.0, 0.75, 'COUNT',
 'Currency Symbol,Money Symbol,Display Symbol,Visual Identifier', 0, 0),

-- ISO Currency Code
(2, 'CurrencyCode', 'ISO 4217 three-letter currency code', 
 'Standard international currency code used for financial transactions, regulatory reporting, and integration with payment providers and banks.',
 'EUR, USD, GBP, CAD, AUD', 'Required field, exactly 3 uppercase letters, must be valid ISO 4217 code',
 0, 1, GETDATE(), 'System', 'currency code, iso code, currency abbreviation, money code, iso currency code',
 'EUR, USD, GBP, CAD, AUD, SEK, NOK, DKK, CHF, PLN',
 'ISO 4217 standard maintained by International Organization for Standardization',
 'Direct mapping from ISO 4217 currency code list', 'iso_standard,regulatory,integration,payment,reference_data',
 'Text', 'NOT NULL, LENGTH = 3, UPPERCASE, VALID_ISO_4217', 9.9, 0.90, 'COUNT',
 'ISO Currency Code,Currency Standard,Payment Code,Regulatory Code', 0, 0),

-- Exchange Rates
(2, 'RateInEUR', 'Exchange rate from this currency to Euro', 
 'Current exchange rate for converting this currency to EUR. Used for financial reporting, revenue calculations, and multi-currency consolidation.',
 '1.0000 (EUR), 0.8500 (USD), 1.1500 (GBP)', 'Must be positive decimal, updated regularly, precision to 4 decimal places',
 0, 1, GETDATE(), 'System', 'euro rate, eur exchange rate, euro conversion, eur rate, exchange rate to euro',
 '1.0000, 0.8500, 1.1500, 0.7200, 0.6800, 0.0950',
 'Updated daily from financial data providers (Reuters, Bloomberg, ECB)',
 'Base currency EUR = 1.0000, others calculated from market rates', 'exchange_rate,financial,conversion,market_data,calculated',
 'Decimal', 'CHECK (RateInEUR > 0), PRECISION(19,4)', 8.5, 0.80, 'AVG',
 'Exchange Rate,Currency Conversion,Financial Rate,Market Rate', 0, 1),

(2, 'RateInUSD', 'Exchange rate from this currency to US Dollar', 
 'Current exchange rate for converting this currency to USD. Critical for US market reporting, payment processing, and regulatory compliance.',
 '1.1800 (EUR), 1.0000 (USD), 1.3500 (GBP)', 'Must be positive decimal, updated regularly, precision to 4 decimal places',
 0, 1, GETDATE(), 'System', 'usd rate, dollar rate, usd exchange rate, dollar conversion, exchange rate to usd',
 '1.1800, 1.0000, 1.3500, 0.8500, 0.8000, 0.1120',
 'Updated daily from financial data providers, synchronized with major forex markets',
 'Base currency USD = 1.0000, others calculated from market rates', 'exchange_rate,financial,conversion,usd_market,calculated',
 'Decimal', 'CHECK (RateInUSD > 0), PRECISION(19,4)', 8.5, 0.75, 'AVG',
 'Exchange Rate,USD Conversion,Dollar Rate,Market Rate', 0, 1),

(2, 'RateInGBP', 'Exchange rate from this currency to British Pound', 
 'Current exchange rate for converting this currency to GBP. Essential for UK market operations, regulatory reporting, and Sterling-based transactions.',
 '0.8700 (EUR), 0.7400 (USD), 1.0000 (GBP)', 'Must be positive decimal, updated regularly, precision to 4 decimal places',
 0, 1, GETDATE(), 'System', 'gbp rate, pound rate, sterling rate, gbp exchange rate, exchange rate to gbp',
 '0.8700, 0.7400, 1.0000, 0.6200, 0.5900, 0.0820',
 'Updated daily from financial data providers, aligned with London forex markets',
 'Base currency GBP = 1.0000, others calculated from market rates', 'exchange_rate,financial,conversion,gbp_market,calculated',
 'Decimal', 'CHECK (RateInGBP > 0), PRECISION(19,4)', 8.5, 0.70, 'AVG',
 'Exchange Rate,GBP Conversion,Sterling Rate,Market Rate', 0, 1),

-- UI and Configuration Fields
(2, 'OrderBy', 'Display order for currency listing', 
 'Numeric value determining the sequence in which currencies appear in user interfaces, dropdown lists, and reports. Lower values appear first.',
 '1, 2, 3, 10, 15', 'Optional field, positive integer, used for UI sorting',
 0, 1, GETDATE(), 'System', 'order, sort order, display order, sequence, priority, ui order',
 '1, 2, 3, 5, 10, 15, 20, 25, 30, 99',
 'Manually configured by business users based on currency importance and usage frequency',
 'Manual assignment based on business priority and regional importance', 'ui_control,sorting,display,user_experience,configuration',
 'Integer', 'CHECK (OrderBy > 0)', 7.0, 0.30, 'MIN',
 'Display Order,Sort Priority,UI Sequence,User Interface', 0, 0),

(2, 'Multiplier', 'Currency conversion multiplier factor', 
 'Mathematical multiplier used in currency calculations and conversions. Handles currencies with different decimal place conventions.',
 '1, 100, 1000', 'Positive integer, typically powers of 10, used for decimal place adjustments',
 0, 1, GETDATE(), 'System', 'multiplier, conversion factor, decimal factor, scale factor, calculation multiplier',
 '1, 10, 100, 1000, 10000',
 'Configured based on currency decimal conventions and calculation requirements',
 'Applied to normalize currency values: Amount * Multiplier', 'calculation,conversion,decimal_handling,normalization,calculated',
 'Integer', 'CHECK (Multiplier > 0)', 6.5, 0.25, 'AVG',
 'Conversion Factor,Decimal Multiplier,Scale Factor,Calculation Factor', 0, 1),

-- Localization Fields
(2, 'ForLanguagesID', 'Language identifier for currency localization', 
 'Links to language settings for currency display formatting, number formats, and regional preferences. Supports multi-language gaming platforms.',
 '1, 2, 3, 9, 10', 'Optional field, references language master table, positive integer',
 0, 1, GETDATE(), 'System', 'language id, locale id, language reference, localization id, language key',
 '1, 2, 3, 4, 5, 9, 10, 11, 12, 15',
 'References language master data, maintained by localization team',
 'Foreign key reference to language configuration table', 'localization,language,regional,formatting,reference_data',
 'Integer', 'FOREIGN KEY reference to Languages table', 7.5, 0.40, 'COUNT',
 'Language Reference,Localization,Regional Settings,Language Configuration', 0, 0),

(2, 'ForLanguages', 'Language codes for currency localization', 
 'Comma-separated list of language codes where this currency formatting applies. Supports regional currency display variations.',
 'en-US,en-GB', 'Optional field, max 50 characters, comma-separated language codes',
 0, 1, GETDATE(), 'System', 'language codes, locale codes, language list, supported languages, localization codes',
 'en-US, en-GB, de-DE, fr-FR, es-ES, it-IT, sv-SE, no-NO, da-DK',
 'Maintained by localization team, follows ISO 639-1 and ISO 3166-1 standards',
 'Comma-separated concatenation of applicable language codes', 'localization,language_codes,regional,iso_standard,configuration',
 'Text', 'LENGTH <= 50, VALID_LANGUAGE_CODES', 7.0, 0.35, 'COUNT',
 'Language Codes,Locale Codes,Regional Languages,Localization Configuration', 0, 0),

-- Audit Field
(2, 'UpdatedDate', 'Last modification timestamp', 
 'Date and time when the currency record was last updated. Critical for tracking exchange rate changes and data freshness.',
 '2025-06-20 10:30:00', 'Required field, valid datetime, automatically updated on changes',
 0, 1, GETDATE(), 'System', 'updated date, last updated, modification date, timestamp, last modified',
 '2025-06-20 10:30:00, 2025-06-19 15:45:30, 2025-06-18 09:15:00',
 'Automatically maintained by database triggers and application logic',
 'SET UpdatedDate = GETDATE() ON UPDATE', 'audit,timestamp,data_freshness,tracking,system_generated',
 'DateTime', 'NOT NULL, CHECK (UpdatedDate <= GETDATE())', 9.5, 0.60, 'MAX',
 'Last Updated,Modification Time,Audit Timestamp,Data Freshness', 0, 0);

-- Continue with next table...
-- This script will be extended to include all 8 tables systematically
