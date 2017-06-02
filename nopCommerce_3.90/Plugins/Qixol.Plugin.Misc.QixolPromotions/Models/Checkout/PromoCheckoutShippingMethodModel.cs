using Nop.Web.Models.Checkout;
using Qixol.Plugin.Misc.Promo.Models.Shared;
using System.Collections;
using System.Collections.Generic;

namespace Qixol.Plugin.Misc.Promo.Models.Checkout
{
    public class PromoCheckoutShippingMethodModel : CheckoutShippingMethodModel
    {
        private IList<PromoShippingMethodModel> _promoShippingMethods;

        public IList<PromoShippingMethodModel> PromoShippingMethods
        {
            get
            {
                return _promoShippingMethods ?? (_promoShippingMethods = new List<PromoShippingMethodModel>());
            }
            set
            {
                _promoShippingMethods = value;
            }
        }

        public class PromoShippingMethodModel : ShippingMethodModel
        {
            private IList<PromotionModel> _promotions;

            public string DiscountAmount { get; set; }

            public IList<PromotionModel> Promotions
            {
                get
                {
                    return _promotions ?? (_promotions = new List<PromotionModel>());
                }
                set
                {
                    _promotions = value;
                }
            }
        }
    }
}