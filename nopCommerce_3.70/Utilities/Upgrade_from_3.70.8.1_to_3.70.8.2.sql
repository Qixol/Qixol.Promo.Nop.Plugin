/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8.0 to 3.70.8.1
 *
 */

-- Resource strings

if not exists(select * from LocaleStringResource where ResourceName = 'Plugins.Misc.QixolPromo.ToCompleteThePromotion')
	INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) values (1, 'Plugins.Misc.QixolPromo.ToCompleteThePromotion', 'To complete the promotion add products from')

