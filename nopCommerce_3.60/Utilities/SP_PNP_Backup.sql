--------------------------------------------------------------------------------------------------------------------------------------------
--Author:			Dominic Manning
--Created:			2015-09-02
--Last Modifed:		--
--Modified By:		--
--Revision Number: 	1.0	
--------------------------------------------------------------------------------------------------------------------------------------------
--Description:		Backup the promo plugin data.  This backs up the following data to tables prefixed with '_Old':
--					PromoAttributeValueMapping
--					PromoPicture
--					PromoProductAttributeConfig
--					PromoBanner
--					PromoBannerPicture
--					PromoBannerWidgetZone
--------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[SP_PNP_Backup]

AS


	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoAttributeValueMapping]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoAttributeValueMapping]

	CREATE TABLE [dbo].[_OldPromoAttributeValueMapping](
		[Id] [int] NOT NULL,
		[AttributeName] [nvarchar](max) NULL,
		[AttributeValueId] [int] NOT NULL,
		[Priority] [int] NULL,
		[Code] [nvarchar](max) NULL,
		[CreatedOnUtc] [datetime] NOT NULL,
		[Synchronized] [bit] NOT NULL,
		[SynchronizedCode] [nvarchar](max) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoPicture]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoPicture]

	CREATE TABLE [dbo].[_OldPromoPicture](
		[Id] [int] NOT NULL,
		[PictureId] [int] NOT NULL,
		[PromoReference] [nvarchar](max) NULL,
		[PromoTypeName] [nvarchar](max) NULL,
		[IsDefaultForType] [bit] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoProductAttributeConfig]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoProductAttributeConfig]

	CREATE TABLE [dbo].[_OldPromoProductAttributeConfig](
		[Id] [int] NOT NULL,
		[SystemName] [nvarchar](max) NULL,
		[NameResource] [nvarchar](max) NULL,
		[Enabled] [bit] NOT NULL,
		[CreatedUtc] [datetime] NOT NULL,
		[UpdatedUtc] [datetime] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	/*
	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoProductMapping]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoProductMapping]

	CREATE TABLE [dbo].[_OldPromoProductMapping](
		[Id] [int] NOT NULL,
		[EntityName] [nvarchar](max) NULL,
		[EntityId] [int] NOT NULL,
		[NoVariants] [bit] NOT NULL,
		[AttributesXml] [nvarchar](max) NULL,
		[VariantCode] [nvarchar](max) NULL,
		[CreatedOnUtc] [datetime] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoPromotion]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoPromotion]

	CREATE TABLE [dbo].[_OldPromoPromotion](
		[Id] [int] NOT NULL,
		[PromoId] [int] NOT NULL,
		[PromoTypeName] [nvarchar](max) NULL,
		[PromoName] [nvarchar](max) NULL,
		[YourReference] [nvarchar](max) NULL,
		[ReportingCode] [nvarchar](max) NULL,
		[DiscountAmount] [decimal](18, 2) NULL,
		[DiscountPercent] [decimal](18, 2) NULL,
		[BundlePrice] [decimal](18, 2) NULL,
		[MinimumSpend] [decimal](18, 2) NULL,
		[BasketRestrictions] [bit] NOT NULL,
		[CouponRestrictions] [bit] NOT NULL,
		[ValidFrom] [datetime] NULL,
		[ValidTo] [datetime] NULL,
		[DisplayText] [nvarchar](max) NULL,
		[AppliesToItems] [bit] NOT NULL,
		[AppliesToBasket] [bit] NOT NULL,
		[AppliesToDelivery] [bit] NOT NULL,
		[PromoXml] [nvarchar](max) NULL,
		[CreatedDate] [datetime] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoPromotionProductMapping]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoPromotionProductMapping]

	CREATE TABLE [dbo].[_OldPromoPromotionProductMapping](
		[Id] [int] NOT NULL,
		[PromotionId] [int] NOT NULL,
		[ProductMappingId] [int] NOT NULL,
		[RequiredQty] [int] NULL,
		[RequiredSpend] [decimal](18, 2) NULL,
		[MultipleProductRestrictions] [bit] NULL,
		[MatchingRestrictions] [nvarchar](max) NULL,
		[CreatedDate] [datetime] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	*/

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoBanner]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoBanner]

	CREATE TABLE [dbo].[_OldPromoBanner](
		[Id] [int] NOT NULL,
		[Name] [nvarchar](max) NULL,
		[Enabled] [bit] NOT NULL,
		[TransitionType] [nvarchar](max) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoBannerPicture]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoBannerPicture]

	CREATE TABLE [dbo].[_OldPromoBannerPicture](
		[Id] [int] NOT NULL,
		[PromoBannerId] [int] NOT NULL,
		[PictureId] [int] NOT NULL,
		[PromoReference] [nvarchar](max) NULL,
		[DisplaySequence] [int] NOT NULL,
		[Comment] [nvarchar](max) NULL,
		[Url] [nvarchar](max) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoBannerWidgetZone]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoBannerWidgetZone]

	CREATE TABLE [dbo].[_OldPromoBannerWidgetZone](
		[Id] [int] NOT NULL,
		[PromoBannerId] [int] NOT NULL,
		[WidgetZoneSystemName] [nvarchar](max) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	/*
	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoSetting]') AND TYPE IN (N'U'))
		DROP TABLE [dbo].[_OldPromoSetting]

	CREATE TABLE [dbo].[_OldPromoSetting](
		[Id] [int] NOT NULL,
		[Name] [nvarchar](200) NOT NULL,
		[Value] [nvarchar](2000) NOT NULL,
		[StoreId] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	*/

	INSERT INTO [dbo].[_OldPromoAttributeValueMapping]
		(
			[Id]
			,[AttributeName]
			,[AttributeValueId]
			,[Priority]
			,[Code]
			,[CreatedOnUtc]
			,[Synchronized]
			,[SynchronizedCode]
		)
	SELECT  [Id]
			,[AttributeName]
			,[AttributeValueId]
			,[Priority]
			,[Code]
			,[CreatedOnUtc]
			,[Synchronized]
			,[SynchronizedCode]
	FROM	[dbo].[PromoAttributeValueMapping]


	INSERT INTO [dbo].[_OldPromoPicture]
		(
			[Id]
			,[PictureId]
			,[PromoReference]
			,[PromoTypeName]
			,[IsDefaultForType]
		)
	SELECT 
			[Id]
			,[PictureId]
			,[PromoReference]
			,[PromoTypeName]
			,[IsDefaultForType]
	FROM	[dbo].[PromoPicture]


	INSERT INTO [dbo].[_OldPromoProductAttributeConfig]
		(
			 [Id]
			,[SystemName]
			,[NameResource]
			,[Enabled]
			,[CreatedUtc]
			,[UpdatedUtc]
		)
	SELECT 
			 [Id]
			,[SystemName]
			,[NameResource]
			,[Enabled]
			,[CreatedUtc]
			,[UpdatedUtc]
	FROM	[dbo].[PromoProductAttributeConfig]

	/*
	INSERT INTO [dbo].[_OldPromoProductMapping]
		(
			[Id]
			,[EntityName]
			,[EntityId]
			,[NoVariants]
			,[AttributesXml]
			,[VariantCode]
			,[CreatedOnUtc]
		)
	SELECT 
			[Id]
			,[EntityName]
			,[EntityId]
			,[NoVariants]
			,[AttributesXml]
			,[VariantCode]
			,[CreatedOnUtc]
	FROM	[dbo].[PromoProductMapping]


	INSERT INTO [dbo].[_OldPromoPromotion]
		(
			[Id]
			,[PromoId]
			,[PromoTypeName]
			,[PromoName]
			,[YourReference]
			,[ReportingCode]
			,[DiscountAmount]
			,[DiscountPercent]
			,[BundlePrice]
			,[MinimumSpend]
			,[BasketRestrictions]
			,[CouponRestrictions]
			,[ValidFrom]
			,[ValidTo]
			,[DisplayText]
			,[AppliesToItems]
			,[AppliesToBasket]
			,[AppliesToDelivery]
			,[PromoXml]
			,[CreatedDate]
		)
	SELECT 
			[Id]
			,[PromoId]
			,[PromoTypeName]
			,[PromoName]
			,[YourReference]
			,[ReportingCode]
			,[DiscountAmount]
			,[DiscountPercent]
			,[BundlePrice]
			,[MinimumSpend]
			,[BasketRestrictions]
			,[CouponRestrictions]
			,[ValidFrom]
			,[ValidTo]
			,[DisplayText]
			,[AppliesToItems]
			,[AppliesToBasket]
			,[AppliesToDelivery]
			,[PromoXml]
			,[CreatedDate]
	FROM	[dbo].[PromoPromotion]



	INSERT INTO [dbo].[_OldPromoPromotionProductMapping]
		(
			[Id]
			,[PromotionId]
			,[ProductMappingId]
			,[RequiredQty]
			,[RequiredSpend]
			,[MultipleProductRestrictions]
			,[MatchingRestrictions]
			,[CreatedDate]
		)
	SELECT 
			[Id]
			,[PromotionId]
			,[ProductMappingId]
			,[RequiredQty]
			,[RequiredSpend]
			,[MultipleProductRestrictions]
			,[MatchingRestrictions]
			,[CreatedDate]
	FROM	[dbo].[PromoPromotionProductMapping]
	*/

	INSERT INTO [dbo].[_OldPromoBanner]
		(
			[Id]
			,[Name]
			,[Enabled]
			,[TransitionType]
		)
	SELECT 
			[Id]
			,[Name]
			,[Enabled]
			,[TransitionType]
	FROM	[dbo].[PromoBanner]


	INSERT INTO [dbo].[_OldPromoBannerPicture]
		(
			[Id]
			,[PromoBannerId]
			,[PictureId]
			,[PromoReference]
			,[DisplaySequence]
			,[Comment]
			,[Url]
		)
	SELECT 
			[Id]
			,[PromoBannerId]
			,[PictureId]
			,[PromoReference]
			,[DisplaySequence]
			,[Comment]
			,[Url]
	FROM	[dbo].[PromoBannerPicture]


	INSERT INTO [dbo].[_OldPromoBannerWidgetZone]
		(
			[Id]
			,[PromoBannerId]
			,[WidgetZoneSystemName]
		)
	SELECT 
			[Id]
			,[PromoBannerId]
			,[WidgetZoneSystemName]
	FROM	[dbo].[PromoBannerWidgetZone]

	/*
	INSERT INTO	[dbo].[_OldPromoSetting]
		(
				[Id],
				[Name],
				[Value],
				[StoreId]
		)
	SELECT		
				[Id],
				[Name],
				[Value],
				[StoreId]
	FROM		[dbo].[Setting]
	WHERE		[Name] LIKE 'promo%'
	*/




GO


