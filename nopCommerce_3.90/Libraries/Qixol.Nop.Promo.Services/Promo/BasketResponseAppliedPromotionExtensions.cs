using Nop.Services.Localization;
using Qixol.Nop.Promo.Services.Localization;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Domain.Customers;

namespace Qixol.Nop.Promo.Services.Promo
{
    public static class BasketResponseAppliedPromotionExtensions
    {

        public static string DisplayDetails(this BasketResponseAppliedPromotion appliedPromotion, Customer customer)
        {
            var promoUtilities = DependencyResolver.Current.GetService<IPromoUtilities>();
            var basketResponse = promoUtilities.GetBasketResponse(customer);

            if (!basketResponse.IsValid())
                return "error promotion summary not found";

            var summaryAppliedPromotion = (from p in basketResponse.Summary.AppliedPromotions
                                           where p.PromotionId == appliedPromotion.PromotionId && p.InstanceId == appliedPromotion.InstanceId
                                           select p).FirstOrDefault();


            return summaryAppliedPromotion.DisplayDetails();
        }
    }
}