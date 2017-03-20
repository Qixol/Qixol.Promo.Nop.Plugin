/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8.14 to 3.80.0.1
 *
 */

-- new resource strings
if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Help.Resources.ShowHelperMessagesTitle')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.Help.Resources.ShowHelperMessagesTitle', 'Show Help Messages')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.ProductAttributes.Category')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.ProductAttributes.Category', 'Category');

-- replacement for removed nopCommerce resource string
update PromoProductAttributeConfig set NameResource = 'Plugins.Misc.QixolPromo.ProductAttributes.Category' where NameResource = 'Admin.Catalog.Products.Categories.Fields.Category'