using global::Nop.Web.Framework.Mvc;

namespace Qixol.Plugin.Misc.Promo.Models.Checkout
{
    public class PromoOnePageCheckoutModel : BaseNopModel
    {
        public PromoOnePageCheckoutModel()
        {

        }

        public bool DisableBillingAddressCheckoutStep { get; set; }
        public bool ShippingRequired { get; set; }
        public bool ShowMissedPromotions { get; set; }
    }
}