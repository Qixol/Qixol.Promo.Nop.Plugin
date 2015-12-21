--------------------------------------------------------------------------------------------------------------------------------------------
--Author:			Dominic Manning
--Created:			2015-09-02
--Last Modifed:		--
--Modified By:		--
--Revision Number: 	1.0	
--------------------------------------------------------------------------------------------------------------------------------------------
--Description:		Restore the promo plugin data.  This restores the following data from tables prefixed with '_Old':
--					PromoAttributeValueMapping
--					PromoPicture
--					PromoProductAttributeConfig
--					PromoBanner
--					PromoBannerPicture
--					PromoBannerWidgetZone
--------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[SP_PNP_Restore]

AS

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoAttributeValueMapping]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoAttributeValueMapping]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoAttributeValueMapping]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoAttributeValueMapping]
				END

			INSERT INTO [dbo].[PromoAttributeValueMapping]
				(
					[AttributeName]
					,[AttributeValueId]
					,[Priority]
					,[Code]
					,[CreatedOnUtc]
					,[Synchronized]
					,[SynchronizedCode]
				)
			SELECT  [AttributeName]
					,[AttributeValueId]
					,[Priority]
					,[Code]
					,[CreatedOnUtc]
					,[Synchronized]
					,[SynchronizedCode]
			FROM	[dbo].[_OldPromoAttributeValueMapping]

			DROP TABLE [dbo].[_OldPromoAttributeValueMapping]
		END

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoPicture]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoPicture]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoPicture]) > 0
			BEGIN
				DELETE FROM [dbo].[PromoPicture]
			END

			INSERT INTO [dbo].[PromoPicture]
				(
					[PictureId]
					,[PromoReference]
					,[PromoTypeName]
					,[IsDefaultForType]
				)
			SELECT 
					[PictureId]
					,[PromoReference]
					,[PromoTypeName]
					,[IsDefaultForType]
			FROM	[dbo].[_OldPromoPicture]

			DROP TABLE [dbo].[_OldPromoPicture]

		END

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoProductAttributeConfig]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoProductAttributeConfig]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoProductAttributeConfig]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoProductAttributeConfig]
				END

			INSERT INTO [dbo].[PromoProductAttributeConfig]
				(
					 [SystemName]
					,[NameResource]
					,[Enabled]
					,[CreatedUtc]
					,[UpdatedUtc]
				)
			SELECT 
					 [SystemName]
					,[NameResource]
					,[Enabled]
					,[CreatedUtc]
					,[UpdatedUtc]
			FROM	[dbo].[_OldPromoProductAttributeConfig]

			DROP TABLE [dbo].[_OldPromoProductAttributeConfig]

		END
	/*
	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoProductMapping]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoProductMapping]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoProductMapping]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoProductMapping]
				END


			INSERT INTO [dbo].[PromoProductMapping]
				(
					[EntityName]
					,[EntityId]
					,[NoVariants]
					,[AttributesXml]
					,[VariantCode]
					,[CreatedOnUtc]
				)
			SELECT 
					[EntityName]
					,[EntityId]
					,[NoVariants]
					,[AttributesXml]
					,[VariantCode]
					,[CreatedOnUtc]
			FROM	[dbo].[_OldPromoProductMapping]

			DROP TABLE [dbo].[_OldPromoProductMapping]
		END

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoPromotion]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoPromotion]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoPromotion]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoPromotion]
				END

			INSERT INTO [dbo].[PromoPromotion]
				(
					[PromoId]
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
					[PromoId]
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
			FROM	[dbo].[_OldPromoPromotion]

			DROP TABLE [dbo].[_OldPromoPromotion]

		END

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoPromotionProductMapping]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoPromotionProductMapping]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoPromotionProductMapping]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoPromotionProductMapping]
				END

			INSERT INTO [dbo].[PromoPromotionProductMapping]
				(
					[PromotionId]
					,[ProductMappingId]
					,[RequiredQty]
					,[RequiredSpend]
					,[MultipleProductRestrictions]
					,[MatchingRestrictions]
					,[CreatedDate]
				)
			SELECT 
					[PromotionId]
					,[ProductMappingId]
					,[RequiredQty]
					,[RequiredSpend]
					,[MultipleProductRestrictions]
					,[MatchingRestrictions]
					,[CreatedDate]
			FROM	[dbo].[_OldPromoPromotionProductMapping]

			DROP TABLE [dbo].[_OldPromoPromotionProductMapping]

		END
	*/

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoBanner]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoBanner]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoBanner]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoBanner]
				END


			INSERT INTO [dbo].[PromoBanner]
				(
					[Name]
					,[Enabled]
					,[TransitionType]
				)
			SELECT 
					[Name]
					,[Enabled]
					,[TransitionType]
			FROM	[dbo].[_OldPromoBanner]

			DROP TABLE [dbo].[_OldPromoBanner]
		END

	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoBannerPicture]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoBannerPicture]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoBannerPicture]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoBannerPicture]
				END

			INSERT INTO [dbo].[PromoBannerPicture]
				(
					[PromoBannerId]
					,[PictureId]
					,[PromoReference]
					,[DisplaySequence]
					,[Comment]
					,[Url]
				)
			SELECT 
					[PromoBannerId]
					,[PictureId]
					,[PromoReference]
					,[DisplaySequence]
					,[Comment]
					,[Url]
			FROM	[dbo].[_OldPromoBannerPicture]

			DROP TABLE [dbo].[_OldPromoBannerPicture]

		END


	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoBannerWidgetZone]') AND TYPE IN (N'U'))
		AND EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[PromoBannerWidgetZone]') AND TYPE IN (N'U'))
		BEGIN

			IF (SELECT COUNT(*) FROM [dbo].[PromoBannerWidgetZone]) > 0
				BEGIN
					DELETE FROM [dbo].[PromoBannerWidgetZone]
				END

			INSERT INTO [dbo].[PromoBannerWidgetZone]
				(
					[PromoBannerId]
					,[WidgetZoneSystemName]
				)
			SELECT 
					[PromoBannerId]
					,[WidgetZoneSystemName]
			FROM	[dbo].[_OldPromoBannerWidgetZone]

			DROP TABLE [dbo].[_OldPromoBannerWidgetZone]
		END

	/*
	IF  EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[_OldPromoSetting]') AND TYPE IN (N'U'))
		BEGIN

			UPDATE		S
			SET			[Value] = OPS.[Value]
			FROM		[dbo].[Setting] S	
			INNER JOIN	[dbo].[_OldPromoSetting] OPS
				ON			S.[Name] = OPS.[Name]

			DROP TABLE [dbo].[_OldPromoSetting]

		END
	*/





GO


