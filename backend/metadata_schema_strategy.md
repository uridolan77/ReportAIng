# Business Metadata Schema Strategy

## Metadata Population Framework

### Core Principles
1. **Domain-Driven**: Group columns by business domain for consistent metadata
2. **Pattern-Based**: Use naming conventions to infer business meaning
3. **Semantic-Rich**: Include comprehensive semantic tags for AI understanding
4. **Quality-Focused**: Assign data quality scores based on criticality
5. **Usage-Aware**: Set usage frequency based on business importance

## Column Classification System

### 1. Primary Keys & Identifiers
**Pattern**: `*ID`, `ID`
**Business Meaning**: "Unique identifier for [entity]"
**Semantic Tags**: `primary_key,identifier,reference_data`
**Quality Score**: 9.8-10.0
**Usage Frequency**: 0.95-1.0

### 2. Financial & Monetary Fields
**Pattern**: `*Amount`, `*Rate*`, `Rate*`, `*Fee*`, `*Revenue*`, `*Deposit*`, `*Withdrawal*`
**Business Meaning**: "[Type] amount/rate for [context]"
**Semantic Tags**: `financial,monetary,exchange_rate,gaming_metrics`
**Quality Score**: 8.5-9.5
**Usage Frequency**: 0.70-0.90

### 3. Date & Timestamp Fields
**Pattern**: `*Date`, `*Time`, `Updated*`, `Created*`
**Business Meaning**: "[Event] timestamp for [purpose]"
**Semantic Tags**: `audit,timestamp,data_freshness,tracking`
**Quality Score**: 9.0-9.5
**Usage Frequency**: 0.60-0.80

### 4. Boolean Flags
**Pattern**: `Is*`, `Has*`, `*Enabled`, `*Active`
**Business Meaning**: "Indicates whether [condition]"
**Semantic Tags**: `boolean,status,flag,configuration`
**Quality Score**: 8.0-9.0
**Usage Frequency**: 0.40-0.70

### 5. Reference Codes
**Pattern**: `*Code`, `*Symbol`
**Business Meaning**: "Standard code for [entity]"
**Semantic Tags**: `iso_standard,code,reference_data`
**Quality Score**: 9.0-9.8
**Usage Frequency**: 0.80-0.95

### 6. Names & Descriptions
**Pattern**: `*Name`, `*Title`, `*Description`
**Business Meaning**: "Human-readable [type] of [entity]"
**Semantic Tags**: `display_name,localization,user_interface`
**Quality Score**: 8.5-9.0
**Usage Frequency**: 0.70-0.85

## Domain-Specific Templates

### Financial Domain
```sql
BusinessMeaning: "[Financial metric] for [context/period]"
BusinessContext: "Used for [revenue/cost/regulatory] [reporting/analysis/compliance]"
SemanticTags: "financial,monetary,[specific_type]"
RelatedBusinessTerms: "Revenue,Financial Metrics,Gaming Economics"
PreferredAggregation: "SUM" (amounts), "AVG" (rates)
```

### Gaming Domain
```sql
BusinessMeaning: "[Gaming activity] metric for [game type/platform]"
BusinessContext: "Tracks [player behavior/game performance/operator metrics]"
SemanticTags: "gaming_metrics,player_activity,[game_type]"
RelatedBusinessTerms: "Gaming Activity,Player Behavior,Game Performance"
PreferredAggregation: "SUM" (bets/wins), "COUNT" (sessions)
```

### Player Domain
```sql
BusinessMeaning: "Player [attribute/preference/status] information"
BusinessContext: "Used for [personalization/compliance/marketing/analysis]"
SemanticTags: "player_data,demographics,[specific_category]"
RelatedBusinessTerms: "Player Profile,Customer Data,Demographics"
IsSensitiveData: true (for PII fields)
```

### Reference Domain
```sql
BusinessMeaning: "[Reference entity] [attribute] for [purpose]"
BusinessContext: "Master reference data for [system/regulatory/operational] use"
SemanticTags: "reference_data,master_data,[domain]"
RelatedBusinessTerms: "Reference Data,Master Data,Configuration"
DataQualityScore: 9.0+ (high quality for reference data)
```

## Validation Rules Patterns

### Data Type Based Rules
- **money**: `CHECK (column > 0)` for amounts, `PRECISION(19,4)` for rates
- **varchar**: `LENGTH <= [max]`, `NOT NULL` for required fields
- **datetime**: `CHECK (column <= GETDATE())` for historical dates
- **bit**: `IN (0,1)` for boolean values
- **int**: `CHECK (column > 0)` for IDs, `FOREIGN KEY` for references

### Business Logic Rules
- **Exchange Rates**: Must be positive, updated regularly
- **Player Data**: Age validation, email format, phone format
- **Financial**: Non-negative amounts, currency consistency
- **Gaming**: Valid game types, platform constraints

## Natural Language Aliases Strategy

### Systematic Alias Generation
1. **Primary Term**: Column name variations
2. **Business Terms**: Domain-specific synonyms  
3. **Common Terms**: User-friendly alternatives
4. **Abbreviations**: Standard industry abbreviations

### Examples
- `CurrencyID`: "currency id, currency identifier, currency key, money id"
- `PlayerID`: "player id, player identifier, customer id, user id"
- `TransactionAmount`: "transaction amount, payment amount, transaction value"
- `GameType`: "game type, game category, game classification"

## Implementation Approach

### Phase 1: Reference Tables (47 columns)
1. tbl_Countries (11 columns)
2. tbl_Currencies (12 columns) 
3. tbl_White_labels (24 columns)

### Phase 2: Core Gaming Tables (157 columns)
1. Games (28 columns)
2. tbl_Daily_actions_games (20 columns)
3. tbl_Daily_actions (109 columns)

### Phase 3: Player & Transaction Tables (139 columns)
1. tbl_Daily_actions_players (125 columns)
2. tbl_Daily_actionsGBP_transactions (14 columns)

### Batch Processing Strategy
- Process tables in domain groups
- Use parameterized SQL for consistent metadata
- Validate after each batch
- Test semantic search capabilities progressively
