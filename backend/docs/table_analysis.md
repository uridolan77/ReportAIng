# DailyActionsDB Table Structure Analysis

## Overview
Analysis of 8 tables from DailyActionsDB for systematic business metadata population.

## Table Summary

| Schema | Table Name | Column Count | Primary Key | Business Domain |
|--------|------------|--------------|-------------|-----------------|
| common | tbl_Countries | 11 | CountryID | Geographic/Reference |
| common | tbl_Currencies | 12 | CurrencyID | Financial/Reference |
| common | tbl_Daily_actions | 109 | ID | Gaming/Financial Metrics |
| common | tbl_Daily_actions_games | 20 | ID | Gaming Activity |
| common | tbl_Daily_actions_players | 125 | PlayerID | Player Demographics |
| common | tbl_Daily_actionsGBP_transactions | 14 | TransactionID | Financial Transactions |
| common | tbl_White_labels | 24 | LabelID | Brand/Configuration |
| dbo | Games | 28 | GameID | Game Catalog |

**Total Columns: 343**

## Business Domain Classification

### 1. Reference Data Tables (47 columns)
- **tbl_Countries** (11): Geographic reference, jurisdictions, localization
- **tbl_Currencies** (12): Financial reference, exchange rates, localization  
- **tbl_White_labels** (24): Brand configuration, operational settings

### 2. Gaming Core Tables (157 columns)
- **tbl_Daily_actions** (109): Comprehensive gaming and financial metrics
- **tbl_Daily_actions_games** (20): Game-specific activity and performance
- **Games** (28): Game catalog, providers, compliance

### 3. Player Data Tables (125 columns)
- **tbl_Daily_actions_players** (125): Player demographics, preferences, status

### 4. Financial Transaction Tables (14 columns)
- **tbl_Daily_actionsGBP_transactions** (14): Payment transactions, methods

## Column Type Patterns

### Common Data Types
- **bigint**: Primary keys, player IDs, transaction IDs
- **money**: Financial amounts, exchange rates, gaming metrics
- **datetime/smalldatetime**: Timestamps, dates, audit fields
- **varchar/nvarchar**: Names, codes, descriptions, URLs
- **bit**: Boolean flags, status indicators
- **int/smallint/tinyint**: Numeric identifiers, counts, references

### Naming Conventions
- **ID suffix**: Primary keys and foreign keys
- **Date suffix**: Timestamp fields
- **Rate/Amount**: Financial values
- **Is/Has prefix**: Boolean flags
- **Code suffix**: Standard codes (ISO, etc.)

## Semantic Tag Categories

### Financial Domain
- exchange_rate, financial, conversion, market_data
- payment, transaction, monetary, revenue
- deposit, withdrawal, bonus, wagering

### Gaming Domain  
- gaming_metrics, player_activity, game_performance
- betting, casino, sports, live_games, bingo
- provider, game_mechanics, compliance

### Reference Data
- master_data, reference_data, lookup
- geographic, regulatory, localization
- iso_standard, configuration

### Player Data
- demographics, preferences, status, behavior
- registration, verification, compliance
- communication, marketing, personalization

### Audit & System
- audit, timestamp, data_freshness, tracking
- system_generated, calculated_field
- data_quality, validation, constraints
