using global::Nop.Web.Framework.Mvc;

namespace Qixol.Plugin.Misc.Promo.Models.Checkout
{
    public class PromoCheckoutProgressModel
    {
        public PromoCheckoutProgressStep PromoCheckoutProgressStep { get; set; }
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