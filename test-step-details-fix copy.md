USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessColumnInfo]    Script Date: 26/06/2025 19:34:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BusinessColumnInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TableInfoId] [bigint] NOT NULL,
	[ColumnName] [nvarchar](128) NOT NULL,
	[BusinessMeaning] [nvarchar](500) NULL,
	[BusinessContext] [nvarchar](1000) NULL,
	[DataExamples] [nvarchar](2000) NULL,
	[ValidationRules] [nvarchar](1000) NULL,
	[IsKeyColumn] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[NaturalLanguageAliases] [nvarchar](1000) NOT NULL,
	[ValueExamples] [nvarchar](2000) NOT NULL,
	[DataLineage] [nvarchar](1000) NOT NULL,
	[CalculationRules] [nvarchar](2000) NOT NULL,
	[SemanticTags] [nvarchar](1000) NOT NULL,
	[BusinessDataType] [nvarchar](500) NOT NULL,
	[ConstraintsAndRules] [nvarchar](1000) NOT NULL,
	[DataQualityScore] [decimal](5, 4) NOT NULL,
	[UsageFrequency] [decimal](5, 4) NOT NULL,
	[PreferredAggregation] [nvarchar](500) NOT NULL,
	[RelatedBusinessTerms] [nvarchar](1000) NOT NULL,
	[IsSensitiveData] [bit] NOT NULL,
	[IsCalculatedField] [bit] NOT NULL,
	[SemanticContext] [nvarchar](2000) NULL,
	[ConceptualRelationships] [nvarchar](1000) NULL,
	[DomainSpecificTerms] [nvarchar](500) NULL,
	[QueryIntentMapping] [nvarchar](1000) NULL,
	[BusinessQuestionTypes] [nvarchar](500) NULL,
	[SemanticSynonyms] [nvarchar](1000) NULL,
	[AnalyticalContext] [nvarchar](500) NULL,
	[BusinessMetrics] [nvarchar](500) NULL,
	[SemanticRelevanceScore] [decimal](3, 2) NULL,
	[LLMPromptHints] [nvarchar](1000) NULL,
	[VectorSearchTags] [nvarchar](500) NULL,
	[BusinessPurpose] [nvarchar](max) NULL,
	[BusinessFriendlyName] [nvarchar](max) NULL,
	[NaturalLanguageDescription] [nvarchar](max) NULL,
	[BusinessRules] [nvarchar](max) NULL,
	[RelationshipContext] [nvarchar](max) NULL,
	[DataGovernanceLevel] [nvarchar](max) NULL,
	[LastBusinessReview] [datetime2](7) NULL,
	[ImportanceScore] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BusinessColumnInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0)) FOR [IsKeyColumn]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('System') FOR [CreatedBy]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [NaturalLanguageAliases]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [ValueExamples]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [DataLineage]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [CalculationRules]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [SemanticTags]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [BusinessDataType]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [ConstraintsAndRules]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0.0)) FOR [DataQualityScore]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0.0)) FOR [UsageFrequency]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [PreferredAggregation]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [RelatedBusinessTerms]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0)) FOR [IsSensitiveData]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0)) FOR [IsCalculatedField]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [SemanticContext]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [ConceptualRelationships]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [DomainSpecificTerms]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [QueryIntentMapping]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [BusinessQuestionTypes]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [SemanticSynonyms]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [AnalyticalContext]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [BusinessMetrics]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0.5)) FOR [SemanticRelevanceScore]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [LLMPromptHints]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ('') FOR [VectorSearchTags]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0.5)) FOR [ImportanceScore]
GO

ALTER TABLE [dbo].[BusinessColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_BusinessColumnInfo_BusinessTableInfo] FOREIGN KEY([TableInfoId])
REFERENCES [dbo].[BusinessTableInfo] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BusinessColumnInfo] CHECK CONSTRAINT [FK_BusinessColumnInfo_BusinessTableInfo]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessTableInfo]    Script Date: 26/06/2025 19:34:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BusinessTableInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](128) NOT NULL,
	[SchemaName] [nvarchar](128) NOT NULL,
	[BusinessPurpose] [nvarchar](500) NULL,
	[BusinessContext] [nvarchar](2000) NULL,
	[PrimaryUseCase] [nvarchar](500) NULL,
	[CommonQueryPatterns] [nvarchar](4000) NULL,
	[BusinessRules] [nvarchar](2000) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[DomainClassification] [nvarchar](1000) NOT NULL,
	[NaturalLanguageAliases] [nvarchar](2000) NOT NULL,
	[UsagePatterns] [nvarchar](4000) NOT NULL,
	[DataQualityIndicators] [nvarchar](1000) NOT NULL,
	[RelationshipSemantics] [nvarchar](2000) NOT NULL,
	[ImportanceScore] [decimal](5, 4) NOT NULL,
	[UsageFrequency] [decimal](5, 4) NOT NULL,
	[LastAnalyzed] [datetime2](7) NULL,
	[BusinessOwner] [nvarchar](500) NOT NULL,
	[DataGovernancePolicies] [nvarchar](1000) NOT NULL,
	[SemanticDescription] [nvarchar](2000) NULL,
	[BusinessProcesses] [nvarchar](1000) NULL,
	[AnalyticalUseCases] [nvarchar](1000) NULL,
	[ReportingCategories] [nvarchar](500) NULL,
	[SemanticRelationships] [nvarchar](1000) NULL,
	[QueryComplexityHints] [nvarchar](500) NULL,
	[BusinessGlossaryTerms] [nvarchar](1000) NULL,
	[SemanticCoverageScore] [decimal](3, 2) NULL,
	[LLMContextHints] [nvarchar](500) NULL,
	[VectorSearchKeywords] [nvarchar](1000) NULL,
	[RelatedBusinessTerms] [nvarchar](max) NULL,
	[BusinessFriendlyName] [nvarchar](max) NULL,
	[NaturalLanguageDescription] [nvarchar](max) NULL,
	[RelationshipContext] [nvarchar](max) NULL,
	[DataGovernanceLevel] [nvarchar](max) NULL,
	[LastBusinessReview] [datetime2](7) NULL,
 CONSTRAINT [PK_BusinessTableInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('common') FOR [SchemaName]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('System') FOR [CreatedBy]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [DomainClassification]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [NaturalLanguageAliases]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [UsagePatterns]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [DataQualityIndicators]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [RelationshipSemantics]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ((0.5)) FOR [ImportanceScore]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ((0.0)) FOR [UsageFrequency]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [BusinessOwner]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [DataGovernancePolicies]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [SemanticDescription]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [BusinessProcesses]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [AnalyticalUseCases]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [ReportingCategories]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [SemanticRelationships]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [QueryComplexityHints]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [BusinessGlossaryTerms]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ((0.5)) FOR [SemanticCoverageScore]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [LLMContextHints]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('') FOR [VectorSearchKeywords]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessGlossary]    Script Date: 26/06/2025 19:34:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BusinessGlossary](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Term] [nvarchar](200) NOT NULL,
	[Definition] [nvarchar](2000) NOT NULL,
	[BusinessContext] [nvarchar](1000) NULL,
	[Synonyms] [nvarchar](1000) NULL,
	[RelatedTerms] [nvarchar](1000) NULL,
	[Category] [nvarchar](100) NULL,
	[IsActive] [bit] NOT NULL,
	[UsageCount] [int] NOT NULL,
	[LastUsed] [datetime2](7) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[Domain] [nvarchar](200) NOT NULL,
	[Examples] [nvarchar](2000) NOT NULL,
	[MappedTables] [nvarchar](1000) NOT NULL,
	[MappedColumns] [nvarchar](1000) NOT NULL,
	[HierarchicalRelations] [nvarchar](1000) NOT NULL,
	[PreferredCalculation] [nvarchar](500) NOT NULL,
	[DisambiguationRules] [nvarchar](1000) NOT NULL,
	[BusinessOwner] [nvarchar](500) NOT NULL,
	[RegulationReferences] [nvarchar](1000) NOT NULL,
	[ConfidenceScore] [decimal](5, 4) NOT NULL,
	[AmbiguityScore] [decimal](5, 4) NOT NULL,
	[ContextualVariations] [nvarchar](1000) NOT NULL,
	[LastValidated] [datetime2](7) NULL,
	[SemanticEmbedding] [nvarchar](2000) NULL,
	[QueryPatterns] [nvarchar](1000) NULL,
	[LLMPromptTemplates] [nvarchar](1000) NULL,
	[DisambiguationContext] [nvarchar](500) NULL,
	[SemanticRelationships] [nvarchar](1000) NULL,
	[ConceptualLevel] [nvarchar](500) NULL,
	[CrossDomainMappings] [nvarchar](1000) NULL,
	[SemanticStability] [decimal](3, 2) NULL,
	[InferenceRules] [nvarchar](1000) NULL,
	[BusinessPurpose] [nvarchar](max) NULL,
	[RelatedBusinessTerms] [nvarchar](max) NULL,
	[BusinessFriendlyName] [nvarchar](max) NULL,
	[NaturalLanguageDescription] [nvarchar](max) NULL,
	[BusinessRules] [nvarchar](max) NULL,
	[ImportanceScore] [decimal](18, 2) NOT NULL,
	[UsageFrequency] [decimal](18, 2) NOT NULL,
	[RelationshipContext] [nvarchar](max) NULL,
	[DataGovernanceLevel] [nvarchar](max) NULL,
	[LastBusinessReview] [datetime2](7) NULL,
 CONSTRAINT [PK_BusinessGlossary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((0)) FOR [UsageCount]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('System') FOR [CreatedBy]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [Domain]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [Examples]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [MappedTables]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [MappedColumns]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [HierarchicalRelations]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [PreferredCalculation]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [DisambiguationRules]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [BusinessOwner]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [RegulationReferences]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((1.0)) FOR [ConfidenceScore]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((0.0)) FOR [AmbiguityScore]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [ContextualVariations]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [SemanticEmbedding]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [QueryPatterns]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [LLMPromptTemplates]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [DisambiguationContext]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [SemanticRelationships]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [ConceptualLevel]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [CrossDomainMappings]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((1.0)) FOR [SemanticStability]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ('') FOR [InferenceRules]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((0.5)) FOR [ImportanceScore]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((0.0)) FOR [UsageFrequency]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessDomain]    Script Date: 26/06/2025 19:34:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BusinessDomain](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DomainName] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](1000) NOT NULL,
	[RelatedTables] [nvarchar](2000) NOT NULL,
	[KeyConcepts] [nvarchar](2000) NOT NULL,
	[CommonQueries] [nvarchar](1000) NOT NULL,
	[BusinessOwner] [nvarchar](500) NOT NULL,
	[RelatedDomains] [nvarchar](1000) NOT NULL,
	[ImportanceScore] [decimal](5, 4) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NOT NULL,
	[BusinessPurpose] [nvarchar](max) NULL,
	[RelatedBusinessTerms] [nvarchar](max) NULL,
	[BusinessFriendlyName] [nvarchar](max) NULL,
	[NaturalLanguageDescription] [nvarchar](max) NULL,
	[BusinessRules] [nvarchar](max) NULL,
	[RelationshipContext] [nvarchar](max) NULL,
	[DataGovernanceLevel] [nvarchar](max) NULL,
	[LastBusinessReview] [datetime2](7) NULL,
	[UsageFrequency] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BusinessDomain] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ('') FOR [Description]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ('') FOR [RelatedTables]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ('') FOR [KeyConcepts]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ('') FOR [CommonQueries]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ('') FOR [BusinessOwner]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ('') FOR [RelatedDomains]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ((0.5)) FOR [ImportanceScore]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT (getutcdate()) FOR [UpdatedDate]
GO

ALTER TABLE [dbo].[BusinessDomain] ADD  DEFAULT ((0.0)) FOR [UsageFrequency]
GO


