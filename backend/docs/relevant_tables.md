USE [DailyActionsDB]
GO
/****** Object:  Table [common].[tbl_Countries]    Script Date: 20/06/2025 3:12:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_Countries](
	[CountryID] [int] NOT NULL,
	[CountryName] [varchar](50) NOT NULL,
	[IsActive] [bit] NULL,
	[CountryIntlCode] [varchar](2) NULL,
	[PhoneCode] [nvarchar](50) NULL,
	[IsoCode] [varchar](3) NULL,
	[JurisdictionCode] [nvarchar](15) NULL,
	[DefaultLanguage] [int] NULL,
	[DefaultCurrency] [tinyint] NULL,
	[UpdatedDate] [smalldatetime] NOT NULL,
	[JurisdictionID] [int] NULL,
	[Locales] [nvarchar](50) NULL,
 CONSTRAINT [PK_tbl_Countries] PRIMARY KEY CLUSTERED 
(
	[CountryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [common].[tbl_Currencies]    Script Date: 20/06/2025 3:12:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_Currencies](
	[CurrencyID] [tinyint] NOT NULL,
	[CurrencyName] [varchar](30) NOT NULL,
	[CurrencySymbol] [nvarchar](5) NULL,
	[CurrencyCode] [varchar](3) NOT NULL,
	[RateInEUR] [money] NULL,
	[RateInUSD] [money] NULL,
	[RateInGBP] [money] NULL,
	[OrderBy] [tinyint] NULL,
	[Multiplier] [int] NULL,
	[ForLanguagesID] [int] NULL,
	[ForLanguages] [nvarchar](50) NULL,
	[UpdatedDate] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_tbl_Currencies] PRIMARY KEY CLUSTERED 
(
	[CurrencyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [common].[tbl_Daily_actions]    Script Date: 20/06/2025 3:12:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_Daily_actions](
	[ID] [bigint] NOT NULL,
	[Date] [datetime] NOT NULL,
	[WhiteLabelID] [smallint] NULL,
	[PlayerID] [bigint] NULL,
	[Registration] [tinyint] NULL,
	[FTD] [tinyint] NULL,
	[FTDA] [tinyint] NULL,
	[Deposits] [money] NULL,
	[DepositsCreditCard] [money] NULL,
	[DepositsNeteller] [money] NULL,
	[DepositsMoneyBookers] [money] NULL,
	[DepositsOther] [money] NULL,
	[CashoutRequests] [money] NULL,
	[PaidCashouts] [money] NULL,
	[Chargebacks] [money] NULL,
	[Voids] [money] NULL,
	[ReverseChargebacks] [money] NULL,
	[Bonuses] [money] NULL,
	[CollectedBonuses] [money] NULL,
	[ExpiredBonuses] [money] NULL,
	[ClubPointsConversion] [money] NULL,
	[BankRoll] [money] NULL,
	[SideGamesBets] [money] NULL,
	[SideGamesRefunds] [money] NULL,
	[SideGamesWins] [money] NULL,
	[SideGamesTableGamesBets] [money] NULL,
	[SideGamesTableGamesWins] [money] NULL,
	[SideGamesCasualGamesBets] [money] NULL,
	[SideGamesCasualGamesWins] [money] NULL,
	[SideGamesSlotsBets] [money] NULL,
	[SideGamesSlotsWins] [money] NULL,
	[SideGamesJackpotsBets] [money] NULL,
	[SideGamesJackpotsWins] [money] NULL,
	[SideGamesFeaturedBets] [money] NULL,
	[SideGamesFeaturedWins] [money] NULL,
	[JackpotContribution] [money] NULL,
	[LottoBets] [money] NULL,
	[LottoAdvancedBets] [money] NULL,
	[LottoWins] [money] NULL,
	[LottoAdvancedWins] [money] NULL,
	[InsuranceContribution] [money] NULL,
	[Adjustments] [money] NULL,
	[AdjustmentsAdd] [money] NULL,
	[ClearedBalance] [money] NULL,
	[RevenueAdjustments] [money] NULL,
	[RevenueAdjustmentsAdd] [money] NULL,
	[UpdatedDate] [datetime] NULL,
	[AdministrativeFee] [money] NULL,
	[AdministrativeFeeReturn] [money] NULL,
	[BetsReal] [money] NULL,
	[BetsBonus] [money] NULL,
	[RefundsReal] [money] NULL,
	[RefundsBonus] [money] NULL,
	[WinsReal] [money] NULL,
	[WinsBonus] [money] NULL,
	[BonusConverted] [money] NULL,
	[DepositsFee] [money] NULL,
	[DepositsSport] [money] NULL,
	[BetsSport] [money] NULL,
	[RefundsSport] [money] NULL,
	[WinsSport] [money] NULL,
	[BetsSportReal] [money] NULL,
	[RefundsSportReal] [money] NULL,
	[WinsSportReal] [money] NULL,
	[BetsSportBonus] [money] NULL,
	[RefundsSportBonus] [money] NULL,
	[WinsSportBonus] [money] NULL,
	[BonusesSport] [money] NULL,
	[BetsCasino] [money] NULL,
	[RefundsCasino] [money] NULL,
	[WinsCasino] [money] NULL,
	[BetsCasinoReal] [money] NULL,
	[RefundsCasinoReal] [money] NULL,
	[WinsCasinoReal] [money] NULL,
	[BetsCasinoBonus] [money] NULL,
	[RefundsCasinoBonus] [money] NULL,
	[WinsCasinoBonus] [money] NULL,
	[EUR2GBP] [money] NULL,
	[AppBets] [money] NULL,
	[AppRefunds] [money] NULL,
	[AppWins] [money] NULL,
	[AppBetsCasino] [money] NULL,
	[AppRefundsCasino] [money] NULL,
	[AppWinsCasino] [money] NULL,
	[AppBetsSport] [money] NULL,
	[AppRefundsSport] [money] NULL,
	[AppWinsSport] [money] NULL,
	[DepositsLive] [money] NULL,
	[BonusesLive] [money] NULL,
	[BetsLive] [money] NULL,
	[RefundsLive] [money] NULL,
	[WinsLive] [money] NULL,
	[BetsLiveReal] [money] NULL,
	[RefundsLiveReal] [money] NULL,
	[WinsLiveReal] [money] NULL,
	[BetsLiveBonus] [money] NULL,
	[RefundsLiveBonus] [money] NULL,
	[WinsLiveBonus] [money] NULL,
	[DepositsBingo] [money] NULL,
	[BonusesBingo] [money] NULL,
	[BetsBingo] [money] NULL,
	[RefundsBingo] [money] NULL,
	[WinsBingo] [money] NULL,
	[BetsBingoReal] [money] NULL,
	[RefundsBingoReal] [money] NULL,
	[WinsBingoReal] [money] NULL,
	[BetsBingoBonus] [money] NULL,
	[RefundsBingoBonus] [money] NULL,
	[WinsBingoBonus] [money] NULL,
 CONSTRAINT [PK_tbl_Daily_actionsGBP] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF, DATA_COMPRESSION = ROW) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [common].[tbl_Daily_actions_games]    Script Date: 20/06/2025 3:12:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_Daily_actions_games](
	[ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[GameDate] [datetime] NULL,
	[PlayerID] [bigint] NULL,
	[GameID] [bigint] NULL,
	[Platform] [nvarchar](50) NULL,
	[RealBetAmount] [money] NULL,
	[RealWinAmount] [money] NULL,
	[BonusBetAmount] [money] NULL,
	[BonusWinAmount] [money] NULL,
	[NetGamingRevenue] [money] NULL,
	[NumberofRealBets] [money] NULL,
	[NumberofBonusBets] [money] NULL,
	[NumberofSessions] [money] NULL,
	[NumberofRealWins] [money] NULL,
	[NumberofBonusWins] [money] NULL,
	[RealBetAmountOriginal] [money] NULL,
	[RealWinAmountOriginal] [money] NULL,
	[BonusBetAmountOriginal] [money] NULL,
	[BonusWinAmountOriginal] [money] NULL,
	[NetGamingRevenueOriginal] [money] NULL,
	[UpdateDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_Daily_actionsGBP_games_ID] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF, DATA_COMPRESSION = PAGE) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [common].[tbl_Daily_actions_players]    Script Date: 20/06/2025 3:12:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_Daily_actions_players](
	[PlayerID] [bigint] NOT NULL,
	[CasinoName] [nvarchar](50) NULL,
	[Alias] [nvarchar](500) NULL,
	[RegisteredDate] [datetime] NULL,
	[FirstDepositDate] [datetime] NULL,
	[DateOfBirth] [date] NULL,
	[Gender] [nvarchar](10) NULL,
	[Country] [nvarchar](50) NULL,
	[Currency] [nvarchar](10) NULL,
	[Balance] [money] NULL,
	[OriginalBalance] [money] NULL,
	[AffiliateID] [nvarchar](10) NULL,
	[Language] [nvarchar](10) NULL,
	[RegisteredPlatform] [nvarchar](10) NULL,
	[Email] [nvarchar](256) NULL,
	[IsOptIn] [bit] NULL,
	[IsBlocked] [bit] NULL,
	[IsTest] [bit] NULL,
	[LastLoginDate] [datetime] NULL,
	[VIPLevel] [nvarchar](10) NULL,
	[LastUpdated] [datetime] NULL,
	[TotalDeposits] [money] NULL,
	[TotalWithdrawals] [money] NULL,
	[FirstName] [nvarchar](64) NULL,
	[LastName] [nvarchar](64) NULL,
	[DynamicParameter] [varchar](500) NULL,
	[ClickId] [nvarchar](500) NULL,
	[CountryCode] [varchar](5) NULL,
	[PhoneNumber] [varchar](64) NULL,
	[MobileNumber] [varchar](64) NULL,
	[City] [nvarchar](64) NULL,
	[Address] [nvarchar](256) NULL,
	[ZipCode] [nvarchar](50) NULL,
	[DocumentsStatus] [nvarchar](50) NULL,
	[PlatformString] [varchar](100) NULL,
	[SMSEnabled] [bit] NULL,
	[MailEnabled] [bit] NULL,
	[PromotionsEnabled] [bit] NULL,
	[BonusesEnabled] [bit] NULL,
	[IP] [varchar](50) NULL,
	[PromotionCode] [nvarchar](100) NULL,
	[LoggedIn] [bit] NULL,
	[Status] [varchar](50) NULL,
	[BlockDate] [datetime] NULL,
	[BlockReason] [nvarchar](1000) NULL,
	[BlockReleaseDate] [datetime] NULL,
	[BOAgent] [nvarchar](50) NULL,
	[LastDepositDate] [datetime] NULL,
	[DepositsCount] [int] NULL,
	[WelcomeBonus] [nvarchar](500) NULL,
	[BlockType] [nvarchar](50) NULL,
	[AccountBalance] [money] NULL,
	[BonusBalance] [money] NULL,
	[CustomerClubPoints] [decimal](10, 2) NULL,
	[TotalChargeBacks] [money] NULL,
	[TotalChargebackReverses] [money] NULL,
	[TotalVoids] [money] NULL,
	[TotalBonuses] [money] NULL,
	[TotalCustomerClubPoints] [decimal](10, 2) NULL,
	[MaxBalance] [money] NULL,
	[Ranking] [smallint] NULL,
	[Wagered] [money] NULL,
	[CasinoID] [int] NULL,
	[CurrencySymbol] [nvarchar](5) NULL,
	[RevenueEUR] [money] NULL,
	[LastLoginPlatform] [nvarchar](500) NULL,
	[PromotionsMailEnabled] [bit] NULL,
	[PromotionsSMSEnabled] [bit] NULL,
	[PromotionsPhoneEnabled] [bit] NULL,
	[PromotionsPostEnabled] [bit] NULL,
	[PromotionsPartnerEnabled] [bit] NULL,
	[WelcomeBonusCode] [nvarchar](500) NULL,
	[WelcomeBonusDesc] [nvarchar](500) NULL,
	[WelcomeBonusSport] [nvarchar](500) NULL,
	[WelcomeBonusSportCode] [nvarchar](500) NULL,
	[WelcomeBonusSportDesc] [nvarchar](500) NULL,
	[RegistrationPlayMode] [nvarchar](10) NULL,
	[TotalBetsCasino] [money] NULL,
	[TotalBetsSport] [money] NULL,
	[DepositsCountCasino] [int] NULL,
	[DepositsCountSport] [int] NULL,
	[FirstDepositMethod] [nvarchar](50) NULL,
	[LastDepositMethod] [nvarchar](50) NULL,
	[FirstBonusID] [int] NULL,
	[LastBonusID] [int] NULL,
	[BonusBalanceSport] [money] NULL,
	[PushEnabled] [bit] NULL,
	[PromotionsPushEnabled] [bit] NULL,
	[LastUpdate] [datetime] NULL,
	[FullMobileNumber] [varchar](64) NULL,
	[CountryID] [int] NULL,
	[JurisdictionID] [int] NULL,
	[Locale] [nvarchar](10) NULL,
	[IsActivated] [bit] NULL,
	[AffordabilityAttempts] [nvarchar](50) NULL,
	[AffordabilityTreshold] [money] NULL,
	[AffordabilityBalance] [money] NULL,
	[AffordabilityStatus] [nvarchar](50) NULL,
	[AffordabilityIncomeRangeChoice] [money] NULL,
	[AffordabilityLastUpdate] [datetime] NULL,
	[WinningsRestriction] [money] NULL,
	[CurrentWinnings] [money] NULL,
	[WinningsRestrictionFailedDate] [datetime] NULL,
	[PendingWithdrawals] [money] NULL,
	[Occupation] [nvarchar](100) NULL,
	[FTDAmount] [money] NULL,
	[FTDSettlementCompanyID] [int] NULL,
	[Age] [int] NULL,
	[OccupationID] [int] NULL,
	[OccupationYearlyIncome] [money] NULL,
	[DepositsCountLive] [int] NULL,
	[DepositsCountBingo] [int] NULL,
	[TotalBetsLive] [money] NULL,
	[TotalBetsBingo] [money] NULL,
	[PromotionalCasinoEmailEnabled] [bit] NULL,
	[PromotionalCasinoSMSEnabled] [bit] NULL,
	[PromotionalBingoEmailEnabled] [bit] NULL,
	[PromotionalBingoSMSEnabled] [bit] NULL,
	[PromotionalSportsEmailEnabled] [bit] NULL,
	[PromotionalSportsSMSEnabled] [bit] NULL,
	[PromotionalPushEnabled] [bit] NULL,
	[PromotionalPhoneEnabled] [bit] NULL,
	[PromotionalPostEnabled] [bit] NULL,
	[PromotionalPartnerEnabled] [bit] NULL,
 CONSTRAINT [PK_tbl_Daily_actions_players] PRIMARY KEY CLUSTERED 
(
	[PlayerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF, DATA_COMPRESSION = PAGE) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [common].[tbl_Daily_actionsGBP_transactions]    Script Date: 20/06/2025 3:12:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_Daily_actionsGBP_transactions](
	[TransactionID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[PlayerID] [bigint] NULL,
	[TransactionDate] [datetime] NULL,
	[TransactionType] [varchar](100) NULL,
	[TransactionAmount] [money] NULL,
	[TransactionOriginalAmount] [money] NULL,
	[TransactionDetails] [nvarchar](1000) NULL,
	[Platform] [nvarchar](50) NULL,
	[Status] [nvarchar](25) NULL,
	[CurrencyCode] [varchar](3) NULL,
	[LastUpdated] [datetime] NULL,
	[OriginalTransactionID] [bigint] NOT NULL,
	[TransactionSubDetails] [nvarchar](500) NULL,
	[TransactionComments] [nvarchar](500) NULL,
	[PaymentMethod] [nvarchar](500) NULL,
	[PaymentProvider] [nvarchar](500) NULL,
	[TransactionInfoID] [bigint] NULL,
 CONSTRAINT [PK_tbl_Daily_actionsGBP_transactions_TransactionID] PRIMARY KEY CLUSTERED 
(
	[TransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF, DATA_COMPRESSION = PAGE) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [common].[tbl_White_labels]    Script Date: 20/06/2025 3:12:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [common].[tbl_White_labels](
	[LabelID] [int] NOT NULL,
	[LabelName] [nvarchar](50) NOT NULL,
	[LabelUrlName] [varchar](50) NULL,
	[LabelUrl] [varchar](100) NOT NULL,
	[UrlPlay] [varchar](200) NULL,
	[UrlPlayShort] [varchar](50) NULL,
	[DefaultLanguage] [nvarchar](50) NULL,
	[DefaultCountry] [int] NULL,
	[DefaultCurrency] [int] NULL,
	[WelcomeBonusDesc] [nvarchar](500) NULL,
	[RestrictedCountries] [nvarchar](max) NULL,
	[FoundedYear] [nvarchar](10) NULL,
	[SportEnabled] [bit] NULL,
	[UpdatedDate] [smalldatetime] NOT NULL,
	[DefaultPlayMode] [nvarchar](10) NULL,
	[IsActive] [bit] NULL,
	[LabelTitle] [nvarchar](100) NULL,
	[GACode] [nvarchar](50) NULL,
	[IntileryCode] [nvarchar](50) NULL,
	[EmailDisplayName] [nvarchar](50) NULL,
	[LabelNameShort] [nvarchar](50) NULL,
	[ExcludedJurisdictions] [nvarchar](50) NULL,
	[IsNewSite] [bit] NULL,
 CONSTRAINT [PK_tbl_White_labels] PRIMARY KEY CLUSTERED 
(
	[LabelID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Games]    Script Date: 20/06/2025 3:12:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Games](
	[GameID] [int] NOT NULL,
	[GameName] [varchar](50) NOT NULL,
	[Provider] [nvarchar](50) NOT NULL,
	[SubProvider] [nvarchar](50) NULL,
	[GameType] [nvarchar](50) NOT NULL,
	[GameFilters] [nvarchar](250) NULL,
	[GameOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[DemoEnabled] [bit] NULL,
	[WagerPercent] [float] NULL,
	[JackpotContribution] [float] NULL,
	[PayoutLow] [float] NULL,
	[PayoutHigh] [float] NULL,
	[Volatility] [varchar](50) NULL,
	[UKCompliant] [bit] NULL,
	[IsDesktop] [bit] NULL,
	[ServerGameID] [varchar](50) NULL,
	[ProviderTitle] [varchar](100) NULL,
	[IsMobile] [bit] NULL,
	[MobileServerGameID] [varchar](50) NULL,
	[MobileProviderTitle] [varchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[ReleaseDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[HideInLobby] [bit] NULL,
	[ExcludedCountries] [nvarchar](1000) NULL,
	[ExcludedJurisdictions] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Games] PRIMARY KEY CLUSTERED 
(
	[GameID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
