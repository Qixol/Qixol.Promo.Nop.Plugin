/*
 * update script for Qixol Promo Nop Commerce plugins
 *
 * from version 3.70.8.8 to 3.70.8.9
 *
 */

-- handle the promotion xml schema change - these rows are repopulated by the "pull" scheduled task
truncate table PromoPromotion