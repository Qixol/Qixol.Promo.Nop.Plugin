/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8 to 3.70.9
 *
 */

-- Resource strings

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Product(s)')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.Product(s)', 'Product(s)')
