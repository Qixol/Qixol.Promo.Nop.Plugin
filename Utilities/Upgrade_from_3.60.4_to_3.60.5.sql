/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.60.4 to 3.60.5
 *
 */

-- Removal of "IsTest" setting
delete from Setting where Name = 'promosettings.istest'
delete from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.IsTest'
delete from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.IsTest.Hint'

-- correct spelling of availibility -> availability
update LocaleStringResource set ResourceName = replace(ResourceName, 'Plugins.Misc.QixolPromo.Product.Promos.Item.Availibility', 'Plugins.Misc.QixolPromo.Product.Promos.Item.Availability')
