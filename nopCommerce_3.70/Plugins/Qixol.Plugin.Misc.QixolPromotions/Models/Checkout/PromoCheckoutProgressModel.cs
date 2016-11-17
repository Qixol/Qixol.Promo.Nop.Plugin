using global::Nop.Web.Framework.Mvc;

namespace Qixol.Plugin.Misc.Promo.Models.Checkout
{
    public class PromoCheckoutProgressModel : global::Nop.Web.Models.Checkout.CheckoutProgressModel
    {
        public PromoCheckoutProgressStep PromoCheckoutProgressStep { get; set; }

        public bool ShowMissedPromotions { get; set; }
    }

    public enum PromoCheckoutProgressStep
    {
        Cart,
        Address,
        Shipping,
        Payment,
        Confirm,
        Complete,
        MissedPromotions
    }
}