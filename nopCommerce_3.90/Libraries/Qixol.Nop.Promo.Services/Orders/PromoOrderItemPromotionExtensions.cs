using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using global::Nop.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.ProductMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Promo.Integration.Lib.Basket;

namespace Qixol.Nop.Promo.Services.Orders
{
    public static class PromoOrderItemPromotionExtensions
    {
        public static string DisplayDetails(this PromoOrderItemPromotion promo)
        {
            var promoSettings = EngineContext.Current.Resolve<PromoSettings>();

            switch (promoSettings.ShowPromotionDetailsInBasket)
            {
                case PromotionDetailsDisplayOptions.ShowEndUserText:
                    // The display text is not mandatory, so default it to the promotion type if there is no text.
                    if (!string.IsNullOrEmpty(promo.DisplayText))
                        return promo.DisplayText;
                    else
                        return promo.PromotionTypeDisplay;

                case PromotionDetailsDisplayOptions.ShowPromotionName:
                    return promo.PromotionName;

                case PromotionDetailsDisplayOptions.ShowNoText:
                    return string.Empty;

                default:
                    return promo.PromotionTypeDisplay;
            }
        }
    }
}
