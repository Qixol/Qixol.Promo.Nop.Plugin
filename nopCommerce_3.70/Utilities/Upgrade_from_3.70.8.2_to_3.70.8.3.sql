/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8.0 to 3.70.8.1
 *
 */

-- Resource strings

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.BasketTotalDiscountDescription')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.BasketTotalDiscountDescription', 'Your total savings')

