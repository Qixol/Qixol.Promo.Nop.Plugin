/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8.10 to 3.70.8.11
 *
 */

-- correct spelling of resource key
update LocaleStringResource set ResourceName = 'Plugins.Widgets.QixolPromo.PromoPicture.AddButton' where ResourceName = 'Plugins.Widgets.QixolPromo.PromoPicture.AddButon'