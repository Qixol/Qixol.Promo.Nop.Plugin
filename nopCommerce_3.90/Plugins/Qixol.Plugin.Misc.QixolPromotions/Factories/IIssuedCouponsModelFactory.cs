using Nop.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Promo.Integration.Lib.Basket;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial interface IIssuedCouponsModelFactory
    {
        /// <summary>
        /// Prepare the issued coupons model
        /// </summary>
        /// <returns>Issued coupons model</returns>
        IssuedCouponsModel PrepareIssuedCouponsModel(BasketResponse basketResponse, bool showSku);

        IssuedCouponsModel PrepareIssuedCouponsModel(Order order, bool showSku);

    }

    public partial interface IIssuedCouponModelFactory
    {
        IssuedCouponModel PrepareIssuedCouponModel(BasketResponseCoupon basketResponseCoupon);

        IssuedCouponModel PrepareIssuedCouponModel(PromoOrderCoupon promoOrderCoupon);
    }

}
