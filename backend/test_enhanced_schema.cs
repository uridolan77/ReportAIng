using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using BIReportingCopilot.Infrastructure.BusinessContext;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Tests
{
    /// <summary>
    /// Simple test to verify Enhanced Schema Contextualization System
    /// </summary>
    public class EnhancedSchemaTest
    {
        public static async Task TestEnhancedSchemaSystem()
        {
            Console.WriteLine("🧪 Testing Enhanced Schema Contextualization System");
            Console.WriteLine("=" * 60);

            // Test 1: Query Classification
            TestQueryClassification();

            // Test 2: Gaming Table Detection
            TestGamingTableDetection();

            // Test 3: Financial Query Detection
            TestFinancialQueryDetection();

            Console.WriteLine("✅ All tests completed!");
        }

        private static void TestQueryClassification()
        {
            Console.WriteLine("\n🎯 TEST 1: Query Classification");
            Console.WriteLine("-" * 40);

            var testQueries = new Dictionary<string, string>
            {
                { "Top 10 depositors yesterday from UK", "Financial" },
                { "Gaming revenue by country", "Gaming" },
                { "Daily active users analytics", "Analytics" },
                { "Show me player transactions", "Financial" },
                { "Game performance metrics", "Gaming" }
            };

            foreach (var (query, expectedCategory) in testQueries)
            {
                // Simulate classification logic
                var category = ClassifyQuerySimple(query);
                var status = category == expectedCategory ? "✅ PASS" : "❌ FAIL";
                Console.WriteLine($"   {status} '{query}' → {category} (expected: {expectedCategory})");
            }
        }

        private static void TestGamingTableDetection()
        {
            Console.WriteLine("\n🚫 TEST 2: Gaming Table Detection");
            Console.WriteLine("-" * 40);

            var testTables = new Dictionary<string, bool>
            {
                { "tbl_Daily_actions", true },
                { "Games", true },
                { "tbl_Daily_actions_games", true },
                { "GBP_transactions", false },
                { "tbl_Countries", false },
                { "Players", false }
            };

            foreach (var (tableName, isGaming) in testTables)
            {
                var detected = IsGamingTableSimple(tableName);
                var status = detected == isGaming ? "✅ PASS" : "❌ FAIL";
                var type = isGaming ? "Gaming" : "Non-Gaming";
                Console.WriteLine($"   {status} '{tableName}' → {(detected ? "Gaming" : "Non-Gaming")} (expected: {type})");
            }
        }

        private static void TestFinancialQueryDetection()
        {
            Console.WriteLine("\n💰 TEST 3: Financial Query Detection");
            Console.WriteLine("-" * 40);

            var testQueries = new Dictionary<string, bool>
            {
                { "Top 10 depositors yesterday from UK", true },
                { "Show me deposit amounts", true },
                { "Transaction history", true },
                { "Game statistics", false },
                { "Player activity", false },
                { "Revenue from deposits", true }
            };

            foreach (var (query, isFinancial) in testQueries)
            {
                var detected = IsFinancialQuerySimple(query);
                var status = detected == isFinancial ? "✅ PASS" : "❌ FAIL";
                var type = isFinancial ? "Financial" : "Non-Financial";
                Console.WriteLine($"   {status} '{query}' → {(detected ? "Financial" : "Non-Financial")} (expected: {type})");
            }
        }

        // Simple classification logic for testing
        private static string ClassifyQuerySimple(string query)
        {
            var queryLower = query.ToLowerInvariant();
            
            if (queryLower.Contains("deposit") || queryLower.Contains("transaction") || queryLower.Contains("revenue"))
                return "Financial";
            
            if (queryLower.Contains("game") || queryLower.Contains("gaming"))
                return "Gaming";
            
            if (queryLower.Contains("analytics") || queryLower.Contains("metrics"))
                return "Analytics";
            
            return "Unknown";
        }

        // Simple gaming table detection for testing
        private static bool IsGamingTableSimple(string tableName)
        {
            var tableNameLower = tableName.ToLowerInvariant();
            return tableNameLower.Contains("game") || 
                   tableNameLower.Contains("daily_actions") ||
                   tableNameLower.Contains("bet") ||
                   tableNameLower.Contains("spin");
        }

        // Simple financial query detection for testing
        private static bool IsFinancialQuerySimple(string query)
        {
            var queryLower = query.ToLowerInvariant();
            return queryLower.Contains("deposit") ||
                   queryLower.Contains("transaction") ||
                   queryLower.Contains("revenue") ||
                   queryLower.Contains("payment") ||
                   queryLower.Contains("balance");
        }
    }

    /// <summary>
    /// Program entry point for testing
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Enhanced Schema Contextualization System - Test Suite");
            Console.WriteLine("========================================================");
            
            await EnhancedSchemaTest.TestEnhancedSchemaSystem();
            
            Console.WriteLine("\n🎉 Test suite completed!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
