namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Business schema documentation for gaming/casino database
/// This provides business context to improve AI SQL generation
/// </summary>
public static class BusinessSchemaDocumentation
{
    /// <summary>
    /// Core business tables with their purposes and relationships
    /// </summary>
    public static readonly Dictionary<string, TableBusinessInfo> Tables = new()
    {
        ["tbl_Daily_actions"] = new TableBusinessInfo
        {
            TableName = "tbl_Daily_actions",
            Schema = "common",
            BusinessPurpose = "Main statistics table holding all player statistics aggregated by player by day",
            BusinessContext = "Core table for daily reporting and player activity analysis. Contains comprehensive daily financial and gaming metrics per player.",
            PrimaryUseCase = "Daily reporting, player activity tracking, financial summaries, gaming analytics",
            KeyColumns = new Dictionary<string, string>
            {
                ["ID"] = "Primary key - unique record identifier",
                ["Date"] = "Business date when activities occurred (use for 'today' queries)",
                ["WhiteLabelID"] = "Casino brand/operator identifier (correct spelling)",
                ["PlayerID"] = "Unique identifier for the player (Foreign Key to tbl_Daily_actions_players)",
                ["Registration"] = "Number of new player registrations on this date",
                ["FTD"] = "First Time Deposit - number of players making their first deposit",
                ["FTDA"] = "First Time Deposit Amount - total amount of first deposits",
                ["Deposits"] = "Total deposit amount for this player on this date",
                ["DepositsCreditCard"] = "Deposits made via credit card",
                ["DepositsNeteller"] = "Deposits made via Neteller",
                ["DepositsMoneyBookers"] = "Deposits made via MoneyBookers (Skrill)",
                ["DepositsOther"] = "Deposits made via other payment methods",
                ["CashoutRequests"] = "Total amount of withdrawal requests",
                ["PaidCashouts"] = "Total amount of paid withdrawals",
                ["Chargebacks"] = "Chargeback amounts",
                ["Bonuses"] = "Total bonus amounts awarded",
                ["CollectedBonuses"] = "Bonus amounts actually collected by players",
                ["ExpiredBonuses"] = "Bonus amounts that expired",
                ["BetsReal"] = "Real money bets placed",
                ["BetsBonus"] = "Bonus money bets placed",
                ["WinsReal"] = "Real money winnings",
                ["WinsBonus"] = "Bonus money winnings",
                ["BetsSport"] = "Sports betting amounts",
                ["WinsSport"] = "Sports betting winnings",
                ["BetsCasino"] = "Casino game betting amounts",
                ["WinsCasino"] = "Casino game winnings"
            },
            CommonQueries = new[]
            {
                "deposits today -> SUM(Deposits) WHERE Date = CAST(GETDATE() AS DATE)",
                "deposits by brand today -> SUM(Deposits) GROUP BY WhiteLabelID WHERE Date = today",
                "player activity today -> GROUP BY PlayerID with SUM aggregations",
                "total revenue today -> SUM(Deposits) - SUM(PaidCashouts) WHERE Date = today",
                "sports vs casino -> SUM(BetsSport) vs SUM(BetsCasino)",
                "bonus analysis -> SUM(Bonuses), SUM(CollectedBonuses), SUM(ExpiredBonuses)"
            }
        },

        ["tbl_Daily_actions_players"] = new TableBusinessInfo
        {
            TableName = "tbl_Daily_actions_players",
            Schema = "common",
            BusinessPurpose = "Player master data table containing all player information and demographics",
            BusinessContext = "Central player registry with account details, registration info, and current status",
            PrimaryUseCase = "Player demographics, account management, player segmentation",
            KeyColumns = new Dictionary<string, string>
            {
                ["PlayerID"] = "Primary key - unique player identifier",
                ["Username"] = "Player's login username",
                ["Email"] = "Player's email address",
                ["RegistrationDate"] = "When player first registered",
                ["LastLoginDate"] = "Most recent login timestamp",
                ["Status"] = "Current account status - ONLY 'Active' or 'Blocked' are valid (use 'Blocked' for suspended players)",
                ["CountryID"] = "Player's country (Foreign Key to countries table)",
                ["CurrencyID"] = "Player's preferred currency (Foreign Key to currencies table)",
                ["WhitelabelID"] = "Casino brand where player registered",
                ["Balance"] = "Current account balance",
                ["TotalDeposits"] = "Lifetime total of all deposits",
                ["TotalWithdraws"] = "Lifetime total of all withdrawals"
            },
            CommonQueries = new[]
            {
                "player info -> SELECT player details with country/currency names",
                "new players today -> WHERE RegistrationDate = today",
                "active players -> WHERE Status = 'Active' AND LastLoginDate recent",
                "players by country -> JOIN countries, GROUP BY country"
            }
        },

        ["tbl_Bonus_balances"] = new TableBusinessInfo
        {
            TableName = "tbl_Bonus_balances",
            Schema = "common",
            BusinessPurpose = "Tracks bonus amounts and balances for players",
            BusinessContext = "Linked to daily actions that trigger bonus calculations. Contains promotional and bonus financial data",
            PrimaryUseCase = "Bonus tracking, promotional analysis, bonus liability reporting",
            KeyColumns = new Dictionary<string, string>
            {
                ["BonusBalanceID"] = "Primary key for bonus balance record",
                ["PlayerID"] = "Player who owns this bonus (Foreign Key)",
                ["Amount"] = "Bonus amount value",
                ["BonusType"] = "Type of bonus (Welcome, Deposit Match, Free Spins, etc.)",
                ["Status"] = "Bonus status (Active, Used, Expired, etc.)",
                ["CreatedDate"] = "When bonus was created",
                ["ExpiryDate"] = "When bonus expires",
                ["WhitelabelID"] = "Casino brand that issued the bonus"
            },
            CommonQueries = new[]
            {
                "bonus totals -> SUM(Amount) for active bonuses",
                "bonus by type -> GROUP BY BonusType with SUM(Amount)",
                "expired bonuses -> WHERE ExpiryDate < GETDATE()",
                "player bonus history -> WHERE PlayerID = X ORDER BY CreatedDate"
            }
        },

        ["whitelabels"] = new TableBusinessInfo
        {
            TableName = "whitelabels",
            Schema = "common",
            BusinessPurpose = "Metadata table defining different casino brands/operators within the platform",
            BusinessContext = "Reference table for multi-brand casino operations. Each whitelabel represents a different casino brand",
            PrimaryUseCase = "Brand segmentation, operator reporting, multi-brand analytics",
            KeyColumns = new Dictionary<string, string>
            {
                ["WhitelabelID"] = "Primary key - unique brand identifier",
                ["Name"] = "Brand/casino name for display",
                ["Code"] = "Short code for the brand",
                ["Status"] = "Brand status (Active, Inactive)",
                ["LaunchDate"] = "When this brand was launched",
                ["DefaultCurrencyID"] = "Default currency for this brand"
            },
            CommonQueries = new[]
            {
                "brand performance -> JOIN with daily actions, GROUP BY whitelabel name",
                "active brands -> WHERE Status = 'Active'",
                "brand comparison -> Compare metrics across different whitelabels"
            }
        },

        ["countries"] = new TableBusinessInfo
        {
            TableName = "countries",
            Schema = "common",
            BusinessPurpose = "Reference table for country codes, names, and geographical information",
            BusinessContext = "Used for player geographical segmentation and regulatory compliance",
            PrimaryUseCase = "Geographical analysis, compliance reporting, market segmentation",
            KeyColumns = new Dictionary<string, string>
            {
                ["CountryID"] = "Primary key - unique country identifier",
                ["CountryCode"] = "ISO country code (US, UK, DE, etc.)",
                ["CountryName"] = "Full country name for display",
                ["Region"] = "Geographical region (Europe, Asia, Americas, etc.)",
                ["IsRestricted"] = "Whether country has restrictions",
                ["CurrencyID"] = "Default currency for this country"
            },
            CommonQueries = new[]
            {
                "players by country -> JOIN with players, GROUP BY CountryName",
                "revenue by region -> JOIN with daily actions, GROUP BY Region",
                "restricted countries -> WHERE IsRestricted = 1"
            }
        },

        ["currencies"] = new TableBusinessInfo
        {
            TableName = "currencies",
            Schema = "common",
            BusinessPurpose = "Reference table for supported currencies and exchange rates",
            BusinessContext = "Manages multi-currency operations and financial conversions",
            PrimaryUseCase = "Currency conversion, financial reporting, multi-currency analytics",
            KeyColumns = new Dictionary<string, string>
            {
                ["CurrencyID"] = "Primary key - unique currency identifier",
                ["CurrencyCode"] = "ISO currency code (USD, EUR, GBP, etc.)",
                ["CurrencyName"] = "Full currency name",
                ["Symbol"] = "Currency symbol ($, €, £, etc.)",
                ["ExchangeRate"] = "Exchange rate to base currency",
                ["IsActive"] = "Whether currency is currently supported"
            },
            CommonQueries = new[]
            {
                "revenue by currency -> JOIN with daily actions, GROUP BY CurrencyCode",
                "active currencies -> WHERE IsActive = 1",
                "currency conversion -> Use ExchangeRate for calculations"
            }
        }
    };

    /// <summary>
    /// Common business query patterns and their SQL implementations
    /// </summary>
    public static readonly Dictionary<string, string> QueryPatterns = new()
    {
        ["totals today"] = @"
            SELECT SUM(Deposits) as TotalDeposits, COUNT(DISTINCT PlayerID) as PlayerCount
            FROM common.tbl_Daily_actions
            WHERE Date = CAST(GETDATE() AS DATE)",

        ["deposits by brand today"] = @"
            SELECT da.WhiteLabelID, SUM(da.Deposits) as TotalDeposits, COUNT(DISTINCT da.PlayerID) as PlayerCount
            FROM common.tbl_Daily_actions da
            WHERE da.Date = CAST(GETDATE() AS DATE)
            GROUP BY da.WhiteLabelID
            ORDER BY TotalDeposits DESC",

        ["player activity today"] = @"
            SELECT da.PlayerID, p.Username, SUM(da.Amount) as TotalAmount,
                   SUM(da.TotalDeposits) as Deposits, SUM(da.TotalBets) as Bets
            FROM common.tbl_Daily_actions da
            JOIN common.tbl_Daily_actions_players p ON da.PlayerID = p.PlayerID
            WHERE da.Date = CAST(GETDATE() AS DATE)
            GROUP BY da.PlayerID, p.Username",

        ["revenue by country today"] = @"
            SELECT c.CountryName, SUM(da.Amount) as Revenue, COUNT(DISTINCT da.PlayerID) as Players
            FROM common.tbl_Daily_actions da
            JOIN common.tbl_Daily_actions_players p ON da.PlayerID = p.PlayerID
            JOIN common.countries c ON p.CountryID = c.CountryID
            WHERE da.Date = CAST(GETDATE() AS DATE)
            GROUP BY c.CountryName
            ORDER BY Revenue DESC",

        ["bonus totals"] = @"
            SELECT SUM(Amount) as TotalBonusAmount, COUNT(*) as BonusCount
            FROM common.tbl_Bonus_balances
            WHERE Status = 'Active'"
    };
}

public class TableBusinessInfo
{
    public string TableName { get; set; } = string.Empty;
    public string Schema { get; set; } = "common";
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public Dictionary<string, string> KeyColumns { get; set; } = new();
    public string[] CommonQueries { get; set; } = Array.Empty<string>();
}
