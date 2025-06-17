USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessColumnInfo]    Script Date: 17/06/2025 0:48:17 ******/
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
 CONSTRAINT [PK_BusinessColumnInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((0)) FOR [IsKeyColumn]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessColumnInfo] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BusinessColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_BusinessColumnInfo_BusinessTableInfo] FOREIGN KEY([TableInfoId])
REFERENCES [dbo].[BusinessTableInfo] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BusinessColumnInfo] CHECK CONSTRAINT [FK_BusinessColumnInfo_BusinessTableInfo]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessTableInfo]    Script Date: 17/06/2025 0:48:29 ******/
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
 CONSTRAINT [PK_BusinessTableInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ('common') FOR [SchemaName]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessTableInfo] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessGlossary]    Script Date: 17/06/2025 0:48:11 ******/
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
 CONSTRAINT [PK_BusinessGlossary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT ((0)) FOR [UsageCount]
GO

ALTER TABLE [dbo].[BusinessGlossary] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessSchemas]    Script Date: 17/06/2025 0:48:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BusinessSchemas](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedBy] [nvarchar](100) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](100) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[Tags] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_BusinessSchemas_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessSchemas] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[BusinessSchemas] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[BusinessSchemas] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO

ALTER TABLE [dbo].[BusinessSchemas] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessSchemas] ADD  DEFAULT ((0)) FOR [IsDefault]
GO


USE [BIReportingCopilot_Dev]
GO

/****** Object:  Table [dbo].[BusinessSchemaVersions]    Script Date: 17/06/2025 0:49:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BusinessSchemaVersions](
	[Id] [uniqueidentifier] NOT NULL,
	[SchemaId] [uniqueidentifier] NOT NULL,
	[VersionNumber] [int] NOT NULL,
	[VersionName] [nvarchar](50) NULL,
	[Description] [nvarchar](1000) NULL,
	[CreatedBy] [nvarchar](100) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsCurrent] [bit] NOT NULL,
	[ChangeLog] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_BusinessSchemaVersions_SchemaVersion] UNIQUE NONCLUSTERED 
(
	[SchemaId] ASC,
	[VersionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BusinessSchemaVersions] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[BusinessSchemaVersions] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[BusinessSchemaVersions] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[BusinessSchemaVersions] ADD  DEFAULT ((0)) FOR [IsCurrent]
GO

ALTER TABLE [dbo].[BusinessSchemaVersions]  WITH CHECK ADD  CONSTRAINT [FK_BusinessSchemaVersions_Schema] FOREIGN KEY([SchemaId])
REFERENCES [dbo].[BusinessSchemas] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BusinessSchemaVersions] CHECK CONSTRAINT [FK_BusinessSchemaVersions_Schema]
GO


