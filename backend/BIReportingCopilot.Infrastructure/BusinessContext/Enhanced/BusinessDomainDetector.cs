using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced implementation of business domain detection with proper business domain classification
/// </summary>
public class BusinessDomainDetector : IAdvancedDomainDetector
{
    private readonly ILogger<BusinessDomainDetector> _logger;
    private readonly BIReportingCopilot.Core.Interfaces.BusinessContext.IBusinessMetadataRetrievalService _metadataService;

    // Domain classification keywords with enhanced priority weighting
    private static readonly Dictionary<string, DomainClassification> DomainKeywords = new()
    {
        // Banking/Finance Domain (prioritized for financial transactions and deposits)
        {
            "Banking", new DomainClassification(
                Keywords: new[] {
                    // High priority financial terms (weight 3.0)
                    "depositor", "depositors", "deposit", "deposits", "depositing", "deposited",
                    "withdrawal", "withdrawals", "withdraw", "ftd", "first time deposit",
                    "financial", "banking", "payment", "payments", "transaction", "transactions",
                    // Medium priority financial terms (weight 2.0)
                    "balance", "balances", "account", "accounts", "money", "currency", "amount",
                    "revenue", "profit", "cost", "budget", "roi", "ggr", "ngr",
                    "chargeback", "exchange rate", "payment method", "payment provider"
                },
                RelatedTables: new[] { "tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Countries", "tbl_Currencies", "tbl_Daily_actionsGBP_transactions" },
                KeyConcepts: new[] { "Revenue", "Profit", "Cost", "Budget", "ROI", "financial transactions", "customer deposits", "payment processing", "banking operations" },
                Description: "Banking and financial data including deposits, withdrawals, transactions, and financial metrics"
            )
        },

        // Gaming Domain (focused on game-specific activities)
        {
            "Gaming", new DomainClassification(
                Keywords: new[] {
                    // High priority gaming terms (weight 3.0)
                    "game", "games", "gaming", "play", "playing", "bet", "bets", "betting",
                    "win", "wins", "winning", "casino", "live casino", "slot", "slots",
                    // Medium priority gaming terms (weight 2.0)
                    "session", "sessions", "blackjack", "roulette", "provider", "game provider",
                    "rtp", "volatility", "jackpot", "progressive", "bonus gaming",
                    "real money gaming", "sports betting", "sportsbook", "activity", "activities"
                },
                RelatedTables: new[] { "tbl_Daily_actions", "Games", "tbl_Daily_actions_games", "tbl_White_labels" },
                KeyConcepts: new[] { "Games", "Bets", "Wins", "Losses", "Activity", "game sessions", "player activities", "gaming metrics", "casino operations" },
                Description: "Gaming-specific metrics, player activity data, and casino operations"
            )
        },
        
        // Customer Domain (based on actual BusinessDomain and BusinessGlossary data)
        {
            "Customer", new DomainClassification(
                Keywords: new[] { "customer", "customers", "user", "users", "player", "players", "behavior", "behaviour", 
                                 "engagement", "retention", "segmentation", "demographics", "vip", "vip level", "player tier",
                                 "player verification", "kyc", "player demographics", "customer analytics", "player profile" },
                RelatedTables: new[] { "tbl_Daily_actions_players", "tbl_Countries", "tbl_Currencies" },
                KeyConcepts: new[] { "Customer", "User", "Player", "Engagement", "Retention", "customer behavior", "player analytics" },
                Description: "Customer data including demographics, behavior, and engagement"
            )
        },
        
        // Geographic/Location Domain
        {
            "Geographic", new DomainClassification(
                Keywords: new[] { "country", "countries", "region", "regions", "location", "locations", "geographic",
                                 "uk", "us", "gb", "united kingdom", "united states", "europe", "asia" },
                RelatedTables: new[] { "tbl_Countries", "tbl_Regions" },
                KeyConcepts: new[] { "geographic analysis", "location-based insights", "regional performance", "country metrics" },
                Description: "Geographic domain focusing on location-based analysis and regional insights"
            )
        }
    };

    public BusinessDomainDetector(
        ILogger<BusinessDomainDetector> logger,
        BIReportingCopilot.Core.Interfaces.BusinessContext.IBusinessMetadataRetrievalService metadataService)
    {
        _logger = logger;
        _metadataService = metadataService;
    }

    public async Task<BusinessDomain> DetectDomainAsync(string userQuestion)
    {
        _logger.LogDebug("🔍 Enhanced domain detection for: {Question}", userQuestion.Substring(0, Math.Min(50, userQuestion.Length)));
        
        var questionLower = userQuestion.ToLower();
        var domainScores = new Dictionary<string, double>();

        // Score each domain based on keyword matches
        foreach (var (domainName, classification) in DomainKeywords)
        {
            var score = CalculateDomainScore(questionLower, classification);
            domainScores[domainName] = score;
            
            _logger.LogDebug("Domain {Domain} score: {Score:F2}", domainName, score);
        }

        // Find the highest scoring domain with improved logic
        var bestDomain = domainScores.OrderByDescending(kvp => kvp.Value).First();
        var secondBestDomain = domainScores.OrderByDescending(kvp => kvp.Value).Skip(1).FirstOrDefault();

        // Apply disambiguation logic for close scores
        if (secondBestDomain.Value > 0 && Math.Abs(bestDomain.Value - secondBestDomain.Value) < 0.1)
        {
            _logger.LogDebug("Close domain scores detected, applying disambiguation logic");
            bestDomain = ApplyDomainDisambiguation(questionLower, bestDomain, secondBestDomain, domainScores);
        }

        // If no domain has a good score, try business metadata lookup
        if (bestDomain.Value < 0.3)
        {
            var metadataDomain = await TryBusinessMetadataDomainDetection(questionLower);
            if (metadataDomain != null)
            {
                _logger.LogInformation("✅ Domain detected via business metadata: {Domain}", metadataDomain.Name);
                return metadataDomain;
            }
        }

        var selectedClassification = DomainKeywords[bestDomain.Key];
        var result = new BusinessDomain
        {
            Name = bestDomain.Key,
            Description = selectedClassification.Description,
            KeyConcepts = selectedClassification.KeyConcepts.ToList(),
            RelatedTables = selectedClassification.RelatedTables.ToList(),
            RelevanceScore = bestDomain.Value,
            Metadata = new Dictionary<string, object>
            {
                ["detection_method"] = "keyword_matching",
                ["all_scores"] = domainScores,
                ["matched_keywords"] = GetMatchedKeywords(questionLower, selectedClassification)
            }
        };

        _logger.LogInformation("✅ Domain detected: {Domain} (Score: {Score:F2})", result.Name, result.RelevanceScore);
        return result;
    }

    private double CalculateDomainScore(string questionLower, DomainClassification classification)
    {
        var matchedKeywords = 0;
        var totalKeywordWeight = 0.0;
        var maxPossibleWeight = 0.0;

        // Define high-priority keywords for each domain
        var highPriorityFinancialTerms = new HashSet<string>
        {
            "depositor", "depositors", "deposit", "deposits", "depositing", "deposited",
            "withdrawal", "withdrawals", "withdraw", "ftd", "first time deposit",
            "financial", "banking", "payment", "payments", "transaction", "transactions"
        };

        var highPriorityGamingTerms = new HashSet<string>
        {
            "game", "games", "gaming", "play", "playing", "bet", "bets", "betting",
            "win", "wins", "winning", "casino", "live casino", "slot", "slots"
        };

        foreach (var keyword in classification.Keywords)
        {
            // Determine keyword weight based on domain and priority
            var weight = GetKeywordWeight(keyword, classification, highPriorityFinancialTerms, highPriorityGamingTerms);
            maxPossibleWeight += weight;

            if (questionLower.Contains(keyword))
            {
                matchedKeywords++;
                totalKeywordWeight += weight;

                _logger.LogDebug("Matched keyword '{Keyword}' with weight {Weight:F1}", keyword, weight);
            }
        }

        // Calculate score based on weighted matches
        var baseScore = maxPossibleWeight > 0 ? totalKeywordWeight / maxPossibleWeight : 0.0;

        // Apply domain-specific bonuses
        var domainBonus = ApplyDomainSpecificBonus(questionLower, classification, matchedKeywords);
        var finalScore = Math.Min(baseScore + domainBonus, 1.0);

        _logger.LogDebug("Domain score calculation: base={BaseScore:F3}, bonus={Bonus:F3}, final={Final:F3}",
            baseScore, domainBonus, finalScore);

        return finalScore;
    }

    private double GetKeywordWeight(string keyword, DomainClassification classification,
        HashSet<string> highPriorityFinancial, HashSet<string> highPriorityGaming)
    {
        // High priority terms get weight 3.0
        if ((classification.Description.Contains("Banking") && highPriorityFinancial.Contains(keyword)) ||
            (classification.Description.Contains("Gaming") && highPriorityGaming.Contains(keyword)))
        {
            return 3.0;
        }

        // Medium priority terms get weight 2.0
        if (keyword.Length > 6 || keyword.Contains(" "))
        {
            return 2.0;
        }

        // Standard terms get weight 1.0
        return 1.0;
    }

    private double ApplyDomainSpecificBonus(string questionLower, DomainClassification classification, int matchedKeywords)
    {
        var bonus = 0.0;

        // Banking domain bonuses
        if (classification.Description.Contains("Banking"))
        {
            // Strong bonus for deposit-related queries
            if (questionLower.Contains("depositor") || questionLower.Contains("deposits"))
                bonus += 0.2;

            // Bonus for financial transaction terms
            if (questionLower.Contains("transaction") || questionLower.Contains("payment"))
                bonus += 0.1;

            // Bonus for financial metrics
            if (questionLower.Contains("revenue") || questionLower.Contains("profit"))
                bonus += 0.1;
        }

        // Gaming domain bonuses
        if (classification.Description.Contains("Gaming"))
        {
            // Strong bonus for game-specific terms
            if (questionLower.Contains("game") || questionLower.Contains("casino") || questionLower.Contains("bet"))
                bonus += 0.2;

            // Bonus for gaming activities
            if (questionLower.Contains("play") || questionLower.Contains("session"))
                bonus += 0.1;
        }

        // Multiple keyword bonus
        if (matchedKeywords > 2)
            bonus += 0.05 * (matchedKeywords - 2);

        return Math.Min(bonus, 0.3); // Cap bonus at 0.3
    }

    private KeyValuePair<string, double> ApplyDomainDisambiguation(string questionLower,
        KeyValuePair<string, double> bestDomain, KeyValuePair<string, double> secondBestDomain,
        Dictionary<string, double> allScores)
    {
        _logger.LogDebug("Applying disambiguation between {Domain1} ({Score1:F3}) and {Domain2} ({Score2:F3})",
            bestDomain.Key, bestDomain.Value, secondBestDomain.Key, secondBestDomain.Value);

        // Strong financial indicators that should override gaming classification
        var strongFinancialIndicators = new[] { "depositor", "depositors", "deposit amount", "withdrawal", "ftd", "first time deposit" };
        var strongGamingIndicators = new[] { "game", "casino", "bet", "slot", "play session" };

        foreach (var indicator in strongFinancialIndicators)
        {
            if (questionLower.Contains(indicator))
            {
                _logger.LogDebug("Strong financial indicator '{Indicator}' found, favoring Banking domain", indicator);
                if (allScores.ContainsKey("Banking"))
                {
                    return new KeyValuePair<string, double>("Banking", allScores["Banking"] + 0.2);
                }
            }
        }

        foreach (var indicator in strongGamingIndicators)
        {
            if (questionLower.Contains(indicator))
            {
                _logger.LogDebug("Strong gaming indicator '{Indicator}' found, favoring Gaming domain", indicator);
                if (allScores.ContainsKey("Gaming"))
                {
                    return new KeyValuePair<string, double>("Gaming", allScores["Gaming"] + 0.2);
                }
            }
        }

        // Context-based disambiguation
        if (questionLower.Contains("top") && questionLower.Contains("depositor"))
        {
            _logger.LogDebug("'Top depositor' pattern detected, strongly favoring Banking domain");
            if (allScores.ContainsKey("Banking"))
            {
                return new KeyValuePair<string, double>("Banking", Math.Max(allScores["Banking"] + 0.3, 0.8));
            }
        }

        // Return the original best domain if no disambiguation rules apply
        return bestDomain;
    }

    private List<string> GetMatchedKeywords(string questionLower, DomainClassification classification)
    {
        return classification.Keywords.Where(keyword => questionLower.Contains(keyword)).ToList();
    }

    private async Task<BusinessDomain?> TryBusinessMetadataDomainDetection(string questionLower)
    {
        try
        {
            // Try to find domain from business glossary terms
            var businessTerms = new List<string> { questionLower };
            var glossaryTerms = await _metadataService.FindRelevantGlossaryTermsAsync(businessTerms);

            foreach (var term in glossaryTerms)
            {
                if (!string.IsNullOrEmpty(term.Domain))
                {
                    return new BusinessDomain
                    {
                        Name = term.Domain,
                        Description = term.Definition ?? "Business domain from glossary",
                        KeyConcepts = term.RelatedTerms ?? new List<string>(),
                        RelatedTables = term.MappedTables?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList() ?? new List<string>(),
                        RelevanceScore = 0.8,
                        Metadata = new Dictionary<string, object>
                        {
                            ["detection_method"] = "business_glossary",
                            ["glossary_term_id"] = term.Id,
                            ["glossary_term"] = term.Term
                        }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during business metadata domain detection");
        }

        return null;
    }
}

/// <summary>
/// Domain classification configuration
/// </summary>
public record DomainClassification(
    string[] Keywords,
    string[] RelatedTables,
    string[] KeyConcepts,
    string Description
);
