/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.7 to 3.70.8
 *
 */

-- Resource strings

if not exists(select * from LocaleStringResource where ResourceName = 'Plugin.Misc.QixolPromo.MissedPromotions.PageTitle')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugin.Misc.QixolPromo.MissedPromotions.PageTitle', 'Missed Promotions')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.ShowMissedPromotions')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.ShowMissedPromotions', 'Show Missed Promotions')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.ShowMissedPromotions.Hint')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.ShowMissedPromotions.Hint', 'When stepping from the cart to checkout show the missed promotions page.')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugin.Misc.QixolPromo.MissedPromotions')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugin.Misc.QixolPromo.MissedPromotions', 'Missed Promotions')

-- Settings
if not exists(select * from Setting where [Name] = 'promosettings.showmissedpromotions')
	insert into Setting ([Name], [Value], StoreId) values ('promosettings.showmissedpromotions', 'True', 0)