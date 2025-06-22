USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMUsageLogs]    Script Date: 22/06/2025 9:29:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMUsageLogs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RequestId] [nvarchar](100) NOT NULL,
	[UserId] [nvarchar](256) NOT NULL,
	[ProviderId] [nvarchar](50) NOT NULL,
	[ModelId] [nvarchar](100) NOT NULL,
	[RequestType] [nvarchar](50) NOT NULL,
	[RequestText] [nvarchar](max) NOT NULL,
	[ResponseText] [nvarchar](max) NOT NULL,
	[InputTokens] [int] NOT NULL,
	[OutputTokens] [int] NOT NULL,
	[TotalTokens] [int] NOT NULL,
	[Cost] [decimal](18, 8) NOT NULL,
	[DurationMs] [bigint] NOT NULL,
	[Success] [bit] NOT NULL,
	[ErrorMessage] [nvarchar](1000) NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Metadata] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT ((0)) FOR [InputTokens]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT ((0)) FOR [OutputTokens]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT ((0)) FOR [TotalTokens]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT ((0.0)) FOR [Cost]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT ((0)) FOR [DurationMs]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT ((1)) FOR [Success]
GO

ALTER TABLE [dbo].[LLMUsageLogs] ADD  DEFAULT (getutcdate()) FOR [Timestamp]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMResourceUsage]    Script Date: 22/06/2025 9:29:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMResourceUsage](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](256) NOT NULL,
	[ResourceType] [nvarchar](100) NOT NULL,
	[ResourceId] [nvarchar](200) NULL,
	[UsageAmount] [decimal](18, 4) NOT NULL,
	[UsageUnit] [nvarchar](50) NOT NULL,
	[Cost] [decimal](18, 6) NULL,
	[StartTime] [datetime2](7) NOT NULL,
	[EndTime] [datetime2](7) NULL,
	[Duration] [bigint] NULL,
	[Metadata] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_LLMResourceUsage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMResourceUsage] ADD  DEFAULT ('Count') FOR [UsageUnit]
GO

ALTER TABLE [dbo].[LLMResourceUsage] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMResourceUsage]  WITH CHECK ADD  CONSTRAINT [FK_LLMResourceUsage_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[LLMResourceUsage] CHECK CONSTRAINT [FK_LLMResourceUsage_Users]
GO

ALTER TABLE [dbo].[LLMResourceUsage]  WITH CHECK ADD  CONSTRAINT [CK_LLMResourceUsage_UsageAmount] CHECK  (([UsageAmount]>=(0)))
GO

ALTER TABLE [dbo].[LLMResourceUsage] CHECK CONSTRAINT [CK_LLMResourceUsage_UsageAmount]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMResourceQuotas]    Script Date: 22/06/2025 9:29:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMResourceQuotas](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](256) NOT NULL,
	[ResourceType] [nvarchar](100) NOT NULL,
	[MaxQuantity] [int] NOT NULL,
	[CurrentUsage] [int] NOT NULL,
	[PeriodSeconds] [int] NOT NULL,
	[ResetDate] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_LLMResourceQuotas] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMResourceQuotas] ADD  DEFAULT ((0)) FOR [CurrentUsage]
GO

ALTER TABLE [dbo].[LLMResourceQuotas] ADD  DEFAULT ((86400)) FOR [PeriodSeconds]
GO

ALTER TABLE [dbo].[LLMResourceQuotas] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[LLMResourceQuotas] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMResourceQuotas]  WITH CHECK ADD  CONSTRAINT [FK_LLMResourceQuotas_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[LLMResourceQuotas] CHECK CONSTRAINT [FK_LLMResourceQuotas_Users]
GO

ALTER TABLE [dbo].[LLMResourceQuotas]  WITH CHECK ADD  CONSTRAINT [CK_LLMResourceQuotas_CurrentUsage] CHECK  (([CurrentUsage]>=(0)))
GO

ALTER TABLE [dbo].[LLMResourceQuotas] CHECK CONSTRAINT [CK_LLMResourceQuotas_CurrentUsage]
GO

ALTER TABLE [dbo].[LLMResourceQuotas]  WITH CHECK ADD  CONSTRAINT [CK_LLMResourceQuotas_MaxQuantity] CHECK  (([MaxQuantity]>(0)))
GO

ALTER TABLE [dbo].[LLMResourceQuotas] CHECK CONSTRAINT [CK_LLMResourceQuotas_MaxQuantity]
GO

ALTER TABLE [dbo].[LLMResourceQuotas]  WITH CHECK ADD  CONSTRAINT [CK_LLMResourceQuotas_PeriodSeconds] CHECK  (([PeriodSeconds]>(0)))
GO

ALTER TABLE [dbo].[LLMResourceQuotas] CHECK CONSTRAINT [CK_LLMResourceQuotas_PeriodSeconds]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMProviderConfigs]    Script Date: 22/06/2025 9:29:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMProviderConfigs](
	[ProviderId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[ApiKey] [nvarchar](500) NULL,
	[Endpoint] [nvarchar](500) NULL,
	[Organization] [nvarchar](100) NULL,
	[IsEnabled] [bit] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[Settings] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProviderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMProviderConfigs] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO

ALTER TABLE [dbo].[LLMProviderConfigs] ADD  DEFAULT ((0)) FOR [IsDefault]
GO

ALTER TABLE [dbo].[LLMProviderConfigs] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[LLMProviderConfigs] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMPerformanceMetrics]    Script Date: 22/06/2025 9:29:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMPerformanceMetrics](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EntityType] [nvarchar](100) NOT NULL,
	[EntityId] [nvarchar](200) NOT NULL,
	[MetricName] [nvarchar](100) NOT NULL,
	[MetricValue] [decimal](18, 4) NOT NULL,
	[MetricUnit] [nvarchar](50) NOT NULL,
	[AverageExecutionTime] [bigint] NOT NULL,
	[TotalOperations] [int] NOT NULL,
	[SuccessCount] [int] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
	[Metadata] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_LLMPerformanceMetrics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT ('Count') FOR [MetricUnit]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT ((0)) FOR [AverageExecutionTime]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT ((0)) FOR [TotalOperations]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT ((0)) FOR [SuccessCount]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT ((0)) FOR [ErrorCount]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT (getutcdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics]  WITH CHECK ADD  CONSTRAINT [CK_LLMPerformanceMetrics_ErrorCount] CHECK  (([ErrorCount]>=(0)))
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] CHECK CONSTRAINT [CK_LLMPerformanceMetrics_ErrorCount]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics]  WITH CHECK ADD  CONSTRAINT [CK_LLMPerformanceMetrics_SuccessCount] CHECK  (([SuccessCount]>=(0)))
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] CHECK CONSTRAINT [CK_LLMPerformanceMetrics_SuccessCount]
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics]  WITH CHECK ADD  CONSTRAINT [CK_LLMPerformanceMetrics_TotalOperations] CHECK  (([TotalOperations]>=(0)))
GO

ALTER TABLE [dbo].[LLMPerformanceMetrics] CHECK CONSTRAINT [CK_LLMPerformanceMetrics_TotalOperations]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMModelConfigs]    Script Date: 22/06/2025 9:29:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMModelConfigs](
	[ModelId] [nvarchar](100) NOT NULL,
	[ProviderId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DisplayName] [nvarchar](150) NOT NULL,
	[Temperature] [real] NOT NULL,
	[MaxTokens] [int] NOT NULL,
	[TopP] [real] NOT NULL,
	[FrequencyPenalty] [real] NOT NULL,
	[PresencePenalty] [real] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[UseCase] [nvarchar](50) NULL,
	[CostPerToken] [decimal](18, 8) NOT NULL,
	[Capabilities] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ModelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((0.1)) FOR [Temperature]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((2000)) FOR [MaxTokens]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((1.0)) FOR [TopP]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((0.0)) FOR [FrequencyPenalty]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((0.0)) FOR [PresencePenalty]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO

ALTER TABLE [dbo].[LLMModelConfigs] ADD  DEFAULT ((0.0)) FOR [CostPerToken]
GO

ALTER TABLE [dbo].[LLMModelConfigs]  WITH CHECK ADD  CONSTRAINT [FK_LLMModelConfigs_LLMProviderConfigs] FOREIGN KEY([ProviderId])
REFERENCES [dbo].[LLMProviderConfigs] ([ProviderId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[LLMModelConfigs] CHECK CONSTRAINT [FK_LLMModelConfigs_LLMProviderConfigs]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMCostTracking]    Script Date: 22/06/2025 9:29:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMCostTracking](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](256) NOT NULL,
	[QueryId] [nvarchar](256) NULL,
	[ProviderId] [nvarchar](100) NOT NULL,
	[ModelId] [nvarchar](100) NOT NULL,
	[Cost] [decimal](18, 6) NOT NULL,
	[CostPerToken] [decimal](18, 8) NOT NULL,
	[InputTokens] [int] NOT NULL,
	[OutputTokens] [int] NOT NULL,
	[TotalTokens] [int] NOT NULL,
	[DurationMs] [bigint] NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
	[Metadata] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_LLMCostTracking] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMCostTracking] ADD  DEFAULT ((0)) FOR [InputTokens]
GO

ALTER TABLE [dbo].[LLMCostTracking] ADD  DEFAULT ((0)) FOR [OutputTokens]
GO

ALTER TABLE [dbo].[LLMCostTracking] ADD  DEFAULT ((0)) FOR [TotalTokens]
GO

ALTER TABLE [dbo].[LLMCostTracking] ADD  DEFAULT ((0)) FOR [DurationMs]
GO

ALTER TABLE [dbo].[LLMCostTracking] ADD  DEFAULT ('Query') FOR [Category]
GO

ALTER TABLE [dbo].[LLMCostTracking] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMCostTracking]  WITH CHECK ADD  CONSTRAINT [FK_LLMCostTracking_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[LLMCostTracking] CHECK CONSTRAINT [FK_LLMCostTracking_Users]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMCostPredictions]    Script Date: 22/06/2025 9:29:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMCostPredictions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[QueryId] [nvarchar](256) NOT NULL,
	[UserId] [nvarchar](256) NOT NULL,
	[PredictedCost] [decimal](18, 6) NOT NULL,
	[ActualCost] [decimal](18, 6) NULL,
	[PredictionAccuracy] [decimal](5, 4) NULL,
	[ModelUsed] [nvarchar](100) NOT NULL,
	[InputFeatures] [nvarchar](max) NULL,
	[PredictionMetadata] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_LLMCostPredictions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMCostPredictions] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMCostPredictions]  WITH CHECK ADD  CONSTRAINT [FK_LLMCostPredictions_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[LLMCostPredictions] CHECK CONSTRAINT [FK_LLMCostPredictions_Users]
GO

ALTER TABLE [dbo].[LLMCostPredictions]  WITH CHECK ADD  CONSTRAINT [CK_LLMCostPredictions_Accuracy] CHECK  (([PredictionAccuracy] IS NULL OR [PredictionAccuracy]>=(0) AND [PredictionAccuracy]<=(1)))
GO

ALTER TABLE [dbo].[LLMCostPredictions] CHECK CONSTRAINT [CK_LLMCostPredictions_Accuracy]
GO

ALTER TABLE [dbo].[LLMCostPredictions]  WITH CHECK ADD  CONSTRAINT [CK_LLMCostPredictions_ActualCost] CHECK  (([ActualCost] IS NULL OR [ActualCost]>=(0)))
GO

ALTER TABLE [dbo].[LLMCostPredictions] CHECK CONSTRAINT [CK_LLMCostPredictions_ActualCost]
GO

ALTER TABLE [dbo].[LLMCostPredictions]  WITH CHECK ADD  CONSTRAINT [CK_LLMCostPredictions_PredictedCost] CHECK  (([PredictedCost]>=(0)))
GO

ALTER TABLE [dbo].[LLMCostPredictions] CHECK CONSTRAINT [CK_LLMCostPredictions_PredictedCost]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMCacheStatistics]    Script Date: 22/06/2025 9:29:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMCacheStatistics](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CacheType] [nvarchar](100) NOT NULL,
	[TotalOperations] [bigint] NOT NULL,
	[HitCount] [bigint] NOT NULL,
	[MissCount] [bigint] NOT NULL,
	[SetCount] [bigint] NOT NULL,
	[DeleteCount] [bigint] NOT NULL,
	[HitRate] [decimal](5, 4) NOT NULL,
	[AverageResponseTime] [decimal](10, 2) NOT NULL,
	[TotalSizeBytes] [bigint] NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
	[PeriodStart] [datetime2](7) NOT NULL,
	[PeriodEnd] [datetime2](7) NOT NULL,
	[Metadata] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_LLMCacheStatistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [TotalOperations]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [HitCount]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [MissCount]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [SetCount]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [DeleteCount]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [HitRate]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [AverageResponseTime]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT ((0)) FOR [TotalSizeBytes]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT (getutcdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[LLMCacheStatistics] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMCacheStatistics]  WITH CHECK ADD  CONSTRAINT [CK_LLMCacheStatistics_Counts] CHECK  (([HitCount]>=(0) AND [MissCount]>=(0) AND [SetCount]>=(0) AND [DeleteCount]>=(0)))
GO

ALTER TABLE [dbo].[LLMCacheStatistics] CHECK CONSTRAINT [CK_LLMCacheStatistics_Counts]
GO

ALTER TABLE [dbo].[LLMCacheStatistics]  WITH CHECK ADD  CONSTRAINT [CK_LLMCacheStatistics_HitRate] CHECK  (([HitRate]>=(0) AND [HitRate]<=(1)))
GO

ALTER TABLE [dbo].[LLMCacheStatistics] CHECK CONSTRAINT [CK_LLMCacheStatistics_HitRate]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMCacheConfiguration]    Script Date: 22/06/2025 9:30:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMCacheConfiguration](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CacheType] [nvarchar](100) NOT NULL,
	[MaxSize] [bigint] NOT NULL,
	[TTLSeconds] [int] NOT NULL,
	[EvictionPolicy] [nvarchar](50) NOT NULL,
	[CompressionEnabled] [bit] NOT NULL,
	[EncryptionEnabled] [bit] NOT NULL,
	[WarmupEnabled] [bit] NOT NULL,
	[WarmupQuery] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[Configuration] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_LLMCacheConfiguration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ((1000)) FOR [MaxSize]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ((3600)) FOR [TTLSeconds]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ('LRU') FOR [EvictionPolicy]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ((0)) FOR [CompressionEnabled]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ((0)) FOR [EncryptionEnabled]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ((0)) FOR [WarmupEnabled]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration]  WITH CHECK ADD  CONSTRAINT [CK_LLMCacheConfiguration_MaxSize] CHECK  (([MaxSize]>(0)))
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] CHECK CONSTRAINT [CK_LLMCacheConfiguration_MaxSize]
GO

ALTER TABLE [dbo].[LLMCacheConfiguration]  WITH CHECK ADD  CONSTRAINT [CK_LLMCacheConfiguration_TTLSeconds] CHECK  (([TTLSeconds]>(0)))
GO

ALTER TABLE [dbo].[LLMCacheConfiguration] CHECK CONSTRAINT [CK_LLMCacheConfiguration_TTLSeconds]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[LLMBudgetManagement]    Script Date: 22/06/2025 9:30:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LLMBudgetManagement](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[EntityId] [nvarchar](450) NOT NULL,
	[BudgetAmount] [decimal](18, 2) NOT NULL,
	[SpentAmount] [decimal](18, 2) NOT NULL,
	[RemainingAmount] [decimal](18, 2) NOT NULL,
	[Period] [nvarchar](20) NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[AlertThreshold] [decimal](5, 2) NOT NULL,
	[BlockThreshold] [decimal](5, 2) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_LLMBudgetManagement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT ('User') FOR [Type]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT ((0)) FOR [SpentAmount]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT ('Monthly') FOR [Period]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT ((0.80)) FOR [AlertThreshold]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT ((0.95)) FOR [BlockThreshold]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[LLMBudgetManagement] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LLMBudgetManagement]  WITH CHECK ADD  CONSTRAINT [CK_LLMBudgetManagement_AlertThreshold] CHECK  (([AlertThreshold]>=(0) AND [AlertThreshold]<=(1)))
GO

ALTER TABLE [dbo].[LLMBudgetManagement] CHECK CONSTRAINT [CK_LLMBudgetManagement_AlertThreshold]
GO

ALTER TABLE [dbo].[LLMBudgetManagement]  WITH CHECK ADD  CONSTRAINT [CK_LLMBudgetManagement_BlockThreshold] CHECK  (([BlockThreshold]>=(0) AND [BlockThreshold]<=(1)))
GO

ALTER TABLE [dbo].[LLMBudgetManagement] CHECK CONSTRAINT [CK_LLMBudgetManagement_BlockThreshold]
GO

ALTER TABLE [dbo].[LLMBudgetManagement]  WITH CHECK ADD  CONSTRAINT [CK_LLMBudgetManagement_BudgetAmount] CHECK  (([BudgetAmount]>=(0)))
GO

ALTER TABLE [dbo].[LLMBudgetManagement] CHECK CONSTRAINT [CK_LLMBudgetManagement_BudgetAmount]
GO

ALTER TABLE [dbo].[LLMBudgetManagement]  WITH CHECK ADD  CONSTRAINT [CK_LLMBudgetManagement_SpentAmount] CHECK  (([SpentAmount]>=(0)))
GO

ALTER TABLE [dbo].[LLMBudgetManagement] CHECK CONSTRAINT [CK_LLMBudgetManagement_SpentAmount]
GO


