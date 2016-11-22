/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8.0 to 3.70.8.1
 *
 */

-- Resource strings

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Product(s)')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.Product(s)', 'Product(s)')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.SaveFrom')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.SaveFrom', 'Save from')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Save')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.Save', 'Save')
