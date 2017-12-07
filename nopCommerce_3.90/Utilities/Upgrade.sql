/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 */

-- correct spelling of resource key
update LocaleStringResource set ResourceName = 'Plugins.Widgets.QixolPromo.PromoPicture.AddButton' where ResourceName = 'Plugins.Widgets.QixolPromo.PromoPicture.AddButon'

-- allow three product attributes for variants (Nike Floral Roshe shoe!)
update Setting set [Value] = 3 where [Name] = 'promosettings.maximumattributesforvariants'

-- new resource strings
if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.IntegrationCodes.CustomerRoles.Priority.Help')
	insert into LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.IntegrationCodes.CustomerRoles.Priority.Help', 'If a customer has more than one role then the integration code for the role with the highest numeric priority value will be passed to Promo in the customer group attribute of basket requests.')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Help.Resources.ShowHelperMessagesTitle')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.Help.Resources.ShowHelperMessagesTitle', 'Show Help Messages')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.ProductAttributes.Category')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.ProductAttributes.Category', 'Category');

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Quantity')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Quantity', 'Quantity')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Spend')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Spend', 'Spend')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Percentage')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Percentage', 'You save')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Fixed')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Fixed', 'Save from')

-- replacement for removed nopCommerce resource string
update PromoProductAttributeConfig set [NameResource] = 'Plugins.Misc.QixolPromo.ProductAttributes.Category' where [NameResource] = 'Admin.Catalog.Products.Categories.Fields.Category'

-- resource strings moved from widget to misc plugin
delete from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Coupons.Code'
delete from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Coupons.Status'
delete from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Coupons.ValidTo'
delete from LocaleStringResource where ResourceName = 'Plugins.Widgets.QixolPromo.Coupons.Description'

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Coupons.Code')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugin.Misc.QixolPromo.Coupons.Code', 'Code')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Coupons.Status')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugin.Misc.QixolPromo.Coupons.Status', 'Status')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Coupons.ValidTo')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugin.Misc.QixolPromo.Coupons.ValidTo', 'Valid To')

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.Coupons.Description')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugin.Misc.QixolPromo.Coupons.Description', 'Description')
