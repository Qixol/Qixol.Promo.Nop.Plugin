using Nop.Web.Models.ShoppingCart;
using Qixol.Plugin.Misc.Promo.Models.Shared;
using System.Collections.Generic;

namespace Qixol.Plugin.Misc.Promo.Models.ShoppingCart
{
    public class PromoEstimateShippingResultModel : EstimateShippingResultModel
    {
        private IList<PromoShippingOptionModel> _promoShippingOptions;

        public IList<PromoShippingOptionModel> PromoShippingOptions
        {
            get
            {
                return _promoShippingOptions ?? (_promoShippingOptions = new List<PromoShippingOptionModel>());
            }
        }

        public class PromoShippingOptionModel : ShippingOptionModel
        {
            private IList<PromotionModel> _promotions;

            public string DiscountAmount { get; set; }

            public IList<PromotionModel> Promotions
            {
                get
                {
                    return _promotions ?? (_promotions = new List<PromotionModel>());
                }
            }
        }
    }
}
