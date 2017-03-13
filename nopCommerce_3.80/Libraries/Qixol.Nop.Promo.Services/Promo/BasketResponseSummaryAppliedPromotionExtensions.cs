using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System.Web.Mvc;

namespace Qixol.Nop.Promo.Services.Promo
{
    public static class BasketResponseSummaryAppliedPromotionExtensions
    {
        public static string DisplayDetails(this BasketResponseSummaryAppliedPromotion appliedPromotion)
        {
            var promoSettings = DependencyResolver.Current.GetService<PromoSettings>();

            switch (promoSettings.ShowPromotionDetailsInBasket)
            {
                case PromotionDetailsDisplayOptions.ShowEndUserText:
                    // The display text is not mandatory, so default it to the promotion type if there is no text.
                    if (!string.IsNullOrEmpty(appliedPromotion.DisplayText))
                        return appliedPromotion.DisplayText;
                    else
                        return appliedPromotion.PromotionTypeDisplay;

                case PromotionDetailsDisplayOptions.ShowPromotionName:
                    return appliedPromotion.PromotionName;

                case PromotionDetailsDisplayOptions.ShowNoText:
                    return string.Empty;

                default:
                    return appliedPromotion.PromotionTypeDisplay;
            }
        }

    }
}
